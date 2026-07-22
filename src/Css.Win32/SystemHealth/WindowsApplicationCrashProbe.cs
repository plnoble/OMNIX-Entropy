using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using Css.Core.Apps;
using Css.Core.Software;

namespace Css.Win32.SystemHealth;

public sealed class WindowsApplicationCrashLogEntry
{
    public required int EventId { get; init; }
    public required string ProviderName { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required IReadOnlyList<string> PropertyValues { get; init; }
}

public interface IWindowsApplicationCrashLogReader
{
    IReadOnlyList<WindowsApplicationCrashLogEntry> ReadRecent(
        DateTimeOffset sinceUtc,
        int maximumRecords);
}

public sealed class WindowsApplicationCrashProbe : IApplicationCrashProbe
{
    public const int MaximumCandidateRecords = 128;
    private static readonly TimeSpan ObservationWindow = TimeSpan.FromHours(24);
    private static readonly HashSet<string> GenericTokens = new(StringComparer.Ordinal)
    {
        "app", "application", "bin", "client", "desktop", "exe", "install", "launcher",
        "program", "programfiles", "software", "system", "update", "updater", "windows"
    };

    private readonly IWindowsApplicationCrashLogReader _reader;
    private readonly Func<DateTimeOffset> _utcNow;

    public WindowsApplicationCrashProbe()
        : this(new WindowsApplicationCrashLogReader(), () => DateTimeOffset.UtcNow)
    {
    }

    public WindowsApplicationCrashProbe(
        IWindowsApplicationCrashLogReader reader,
        Func<DateTimeOffset> utcNow)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
    }

    public ApplicationCrashObservation Observe(SoftwareProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var observedAtUtc = _utcNow().ToUniversalTime();
        var windowStartUtc = observedAtUtc.Subtract(ObservationWindow);
        try
        {
            var tokens = CorrelationTokens(profile);
            var matches = _reader
                .ReadRecent(windowStartUtc, MaximumCandidateRecords)
                .Take(MaximumCandidateRecords)
                .Where(entry => IsApproved(entry.EventId, entry.ProviderName))
                .Where(entry => entry.CreatedAtUtc.ToUniversalTime() >= windowStartUtc
                                && entry.CreatedAtUtc.ToUniversalTime() <= observedAtUtc)
                .Where(entry => MatchesAnyToken(entry.PropertyValues, tokens))
                .ToArray();

            return new ApplicationCrashObservation
            {
                Availability = matches.Length == 0
                    ? ApplicationCrashObservationAvailability.NotFound
                    : ApplicationCrashObservationAvailability.Available,
                SoftwareName = profile.Name,
                ObservedAtUtc = observedAtUtc,
                WindowStartUtc = windowStartUtc,
                MatchCount = matches.Length,
                LatestOccurrenceUtc = matches.Length == 0
                    ? null
                    : matches.Max(entry => entry.CreatedAtUtc.ToUniversalTime())
            };
        }
        catch
        {
            return new ApplicationCrashObservation
            {
                Availability = ApplicationCrashObservationAvailability.Unavailable,
                SoftwareName = profile.Name,
                ObservedAtUtc = observedAtUtc,
                WindowStartUtc = windowStartUtc,
                MatchCount = 0
            };
        }
    }

    private static bool IsApproved(int eventId, string providerName) =>
        (eventId == 1000
         && providerName.Equals("Application Error", StringComparison.OrdinalIgnoreCase))
        || (eventId == 1001
            && providerName.Equals("Windows Error Reporting", StringComparison.OrdinalIgnoreCase))
        || (eventId == 1002
            && providerName.Equals("Application Hang", StringComparison.OrdinalIgnoreCase));

    private static bool MatchesAnyToken(
        IEnumerable<string> values,
        IReadOnlySet<string> tokens)
    {
        if (tokens.Count == 0)
            return false;

        return values
            .Take(12)
            .Select(value => NormalizeToken(value.Length > 512 ? value[..512] : value))
            .Any(value => tokens.Any(value.Contains));
    }

    private static IReadOnlySet<string> CorrelationTokens(SoftwareProfile profile)
    {
        var candidates = new List<string?>
        {
            profile.Name,
            FileNameWithoutExtension(profile.DisplayIconPath),
            LastDirectoryName(profile.InstallPath)
        };
        candidates.AddRange(profile.RunningProcesses.Select(FileNameWithoutExtension));

        return candidates
            .Select(NormalizeToken)
            .Where(IsSpecificToken)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static bool IsSpecificToken(string token)
    {
        if (GenericTokens.Contains(token))
            return false;

        var containsNonAsciiLetter = token.Any(character => character > 127 && char.IsLetter(character));
        return containsNonAsciiLetter ? token.Length >= 2 : token.Length >= 4;
    }

    private static string NormalizeToken(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());

    private static string? FileNameWithoutExtension(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            var executable = value.Split(',', 2)[0].Trim().Trim('"');
            return Path.GetFileNameWithoutExtension(executable);
        }
        catch
        {
            return null;
        }
    }

    private static string? LastDirectoryName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return new DirectoryInfo(value.Trim().Trim('"')).Name;
        }
        catch
        {
            return null;
        }
    }
}

public sealed class WindowsApplicationCrashLogReader : IWindowsApplicationCrashLogReader
{
    public IReadOnlyList<WindowsApplicationCrashLogEntry> ReadRecent(
        DateTimeOffset sinceUtc,
        int maximumRecords)
    {
        var boundedMaximum = Math.Clamp(maximumRecords, 1, WindowsApplicationCrashProbe.MaximumCandidateRecords);
        var systemTime = sinceUtc
            .ToUniversalTime()
            .UtcDateTime
            .ToString("o", CultureInfo.InvariantCulture);
        var queryText = $"*[System[(EventID=1000 or EventID=1001 or EventID=1002) and TimeCreated[@SystemTime >= '{systemTime}']]]";
        var query = new EventLogQuery("Application", PathType.LogName, queryText)
        {
            ReverseDirection = true,
            TolerateQueryErrors = true
        };
        using var reader = new EventLogReader(query);
        var entries = new List<WindowsApplicationCrashLogEntry>(boundedMaximum);
        while (entries.Count < boundedMaximum)
        {
            using var record = reader.ReadEvent();
            if (record is null)
                break;

            var createdAt = record.TimeCreated;
            if (createdAt is null)
                continue;

            entries.Add(new WindowsApplicationCrashLogEntry
            {
                EventId = record.Id,
                ProviderName = record.ProviderName ?? string.Empty,
                CreatedAtUtc = new DateTimeOffset(createdAt.Value).ToUniversalTime(),
                PropertyValues = record.Properties
                    .Take(12)
                    .Select(property => Convert.ToString(property.Value, CultureInfo.InvariantCulture) ?? string.Empty)
                    .Select(value => value.Length > 512 ? value[..512] : value)
                    .ToArray()
            });
        }

        return entries;
    }
}
