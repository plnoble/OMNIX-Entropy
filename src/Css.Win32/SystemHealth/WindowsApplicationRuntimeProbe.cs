using System.Diagnostics;
using Css.Core.Apps;
using Css.Core.Software;

namespace Css.Win32.SystemHealth;

public sealed class WindowsApplicationRuntimeAggregate
{
    public int MatchedProcessCount { get; init; }
    public long TotalWorkingSetBytes { get; init; }
    public double? CpuPercent { get; init; }
}

public interface IWindowsApplicationRuntimeReader
{
    WindowsApplicationRuntimeAggregate Read(
        IReadOnlySet<string> exactProcessNames,
        TimeSpan sampleDuration,
        int maximumProcesses);
}

public sealed class WindowsApplicationRuntimeProbe : IApplicationRuntimeProbe
{
    public const int MaximumMatchingProcesses = 32;
    public const int SampleDurationMilliseconds = 350;
    private static readonly HashSet<string> GenericNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "app", "application", "client", "desktop", "helper", "host", "launcher",
        "program", "service", "software", "system", "update", "updater", "windows"
    };

    private readonly IWindowsApplicationRuntimeReader _reader;
    private readonly Func<DateTimeOffset> _utcNow;

    public WindowsApplicationRuntimeProbe()
        : this(new WindowsApplicationRuntimeReader(), () => DateTimeOffset.UtcNow)
    {
    }

    public WindowsApplicationRuntimeProbe(
        IWindowsApplicationRuntimeReader reader,
        Func<DateTimeOffset> utcNow)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
    }

    public ApplicationRuntimeObservation Observe(SoftwareProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var observedAtUtc = _utcNow().ToUniversalTime();
        var exactNames = ExactProcessNames(profile);
        if (exactNames.Count == 0)
            return Unavailable(profile.Name, observedAtUtc);

        try
        {
            var aggregate = _reader.Read(
                exactNames,
                TimeSpan.FromMilliseconds(SampleDurationMilliseconds),
                MaximumMatchingProcesses);
            var count = Math.Clamp(
                aggregate.MatchedProcessCount,
                0,
                MaximumMatchingProcesses);
            if (count == 0)
            {
                return new ApplicationRuntimeObservation
                {
                    Availability = ApplicationRuntimeObservationAvailability.NotRunning,
                    SoftwareName = profile.Name,
                    ObservedAtUtc = observedAtUtc,
                    SampleDurationMilliseconds = SampleDurationMilliseconds
                };
            }

            return new ApplicationRuntimeObservation
            {
                Availability = ApplicationRuntimeObservationAvailability.Available,
                SoftwareName = profile.Name,
                ObservedAtUtc = observedAtUtc,
                MatchedProcessCount = count,
                TotalWorkingSetBytes = Math.Max(0, aggregate.TotalWorkingSetBytes),
                CpuActivity = ToActivity(aggregate.CpuPercent),
                SampleDurationMilliseconds = SampleDurationMilliseconds
            };
        }
        catch
        {
            return Unavailable(profile.Name, observedAtUtc);
        }
    }

    private static IReadOnlySet<string> ExactProcessNames(SoftwareProfile profile)
    {
        var candidates = profile.RunningProcesses
            .Select(CanonicalProcessName)
            .Append(ProcessNameFromIcon(profile.DisplayIconPath));

        return candidates
            .Where(IsSpecificName)
            .Cast<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string? ProcessNameFromIcon(string? displayIcon)
    {
        if (string.IsNullOrWhiteSpace(displayIcon))
            return null;

        try
        {
            var executable = displayIcon.Split(',', 2)[0].Trim().Trim('"');
            return CanonicalProcessName(Path.GetFileName(executable));
        }
        catch
        {
            return null;
        }
    }

    internal static string? CanonicalProcessName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            var fileName = Path.GetFileName(value.Trim().Trim('"'));
            var name = Path.GetFileNameWithoutExtension(fileName).Trim();
            if (name.Any(character => char.IsControl(character)
                                      || character is '/' or '\\'))
            {
                return null;
            }

            return name.ToLowerInvariant();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsSpecificName(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Length >= 3
        && value.Length <= 128
        && !GenericNames.Contains(value);

    private static ApplicationCpuActivity ToActivity(double? cpuPercent)
    {
        if (cpuPercent is null || double.IsNaN(cpuPercent.Value) || double.IsInfinity(cpuPercent.Value))
            return ApplicationCpuActivity.Unknown;

        var bounded = Math.Clamp(cpuPercent.Value, 0, 100);
        if (bounded < 1)
            return ApplicationCpuActivity.Idle;
        if (bounded < 10)
            return ApplicationCpuActivity.Low;
        if (bounded < 40)
            return ApplicationCpuActivity.Moderate;
        return ApplicationCpuActivity.High;
    }

    private static ApplicationRuntimeObservation Unavailable(
        string softwareName,
        DateTimeOffset observedAtUtc) =>
        new()
        {
            Availability = ApplicationRuntimeObservationAvailability.Unavailable,
            SoftwareName = softwareName,
            ObservedAtUtc = observedAtUtc,
            SampleDurationMilliseconds = SampleDurationMilliseconds
        };
}

public sealed class WindowsApplicationRuntimeReader : IWindowsApplicationRuntimeReader
{
    public WindowsApplicationRuntimeAggregate Read(
        IReadOnlySet<string> exactProcessNames,
        TimeSpan sampleDuration,
        int maximumProcesses)
    {
        ArgumentNullException.ThrowIfNull(exactProcessNames);
        var boundedMaximum = Math.Clamp(
            maximumProcesses,
            1,
            WindowsApplicationRuntimeProbe.MaximumMatchingProcesses);
        var boundedDuration = TimeSpan.FromMilliseconds(Math.Clamp(
            sampleDuration.TotalMilliseconds,
            50,
            1_000));
        var baselines = new List<ProcessBaseline>(boundedMaximum);
        var allProcesses = Process.GetProcesses();
        try
        {
            foreach (var process in allProcesses)
            {
                if (baselines.Count >= boundedMaximum)
                    continue;

                try
                {
                    var name = WindowsApplicationRuntimeProbe.CanonicalProcessName(process.ProcessName);
                    if (name is null || !exactProcessNames.Contains(name))
                        continue;

                    TimeSpan? processorTime = null;
                    try
                    {
                        processorTime = process.TotalProcessorTime;
                    }
                    catch
                    {
                        // Access can disappear between enumeration and sampling.
                    }

                    baselines.Add(new ProcessBaseline(process, processorTime));
                }
                catch
                {
                    // A process may exit while its read-only identity is inspected.
                }
            }

            if (baselines.Count == 0)
                return new WindowsApplicationRuntimeAggregate();

            var timer = Stopwatch.StartNew();
            Thread.Sleep(boundedDuration);
            timer.Stop();

            var matchedCount = 0;
            var totalWorkingSet = 0L;
            var totalProcessorDelta = TimeSpan.Zero;
            var hasProcessorDelta = false;
            foreach (var baseline in baselines)
            {
                try
                {
                    baseline.Process.Refresh();
                    if (baseline.Process.HasExited)
                        continue;

                    matchedCount++;
                    try
                    {
                        totalWorkingSet = SaturatingAdd(
                            totalWorkingSet,
                            Math.Max(0, baseline.Process.WorkingSet64));
                    }
                    catch
                    {
                        // Keep the surviving process count even if memory is protected.
                    }

                    if (baseline.ProcessorTime is null)
                        continue;

                    try
                    {
                        var delta = baseline.Process.TotalProcessorTime - baseline.ProcessorTime.Value;
                        if (delta < TimeSpan.Zero)
                            continue;
                        totalProcessorDelta += delta;
                        hasProcessorDelta = true;
                    }
                    catch
                    {
                        // CPU remains Unknown when the second read is unavailable.
                    }
                }
                catch
                {
                    // A process that exits during the sample is not counted as current.
                }
            }

            double? cpuPercent = null;
            if (hasProcessorDelta && timer.Elapsed.TotalMilliseconds > 0)
            {
                cpuPercent = totalProcessorDelta.TotalMilliseconds
                             / timer.Elapsed.TotalMilliseconds
                             / Math.Max(1, Environment.ProcessorCount)
                             * 100;
            }

            return new WindowsApplicationRuntimeAggregate
            {
                MatchedProcessCount = matchedCount,
                TotalWorkingSetBytes = totalWorkingSet,
                CpuPercent = cpuPercent
            };
        }
        finally
        {
            foreach (var process in allProcesses)
                process.Dispose();
        }
    }

    private static long SaturatingAdd(long current, long value) =>
        value > long.MaxValue - current ? long.MaxValue : current + value;

    private sealed record ProcessBaseline(Process Process, TimeSpan? ProcessorTime);
}
