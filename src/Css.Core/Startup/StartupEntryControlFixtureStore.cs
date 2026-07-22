using System.Text.Json;
using System.Text.Json.Serialization;
using Css.Core.Software;

namespace Css.Core.Startup;

public sealed class StartupEntryControlFixtureStore : IStartupEntryControlStore
{
    private const int MaximumFixtureBytes = 64 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly StartupEntryFixtureDocument _fixture;
    private readonly Func<DateTimeOffset> _clock;
    private readonly object _gate = new();
    private StartupEntryState? _currentState;
    private bool _enabled = true;

    private StartupEntryControlFixtureStore(
        StartupEntryFixtureDocument fixture,
        Func<DateTimeOffset> clock)
    {
        _fixture = fixture;
        _clock = clock;
    }

    public static StartupEntryControlFixtureStore? TryCreate(
        string? fixturePath,
        Func<DateTimeOffset>? clock = null)
    {
        if (string.IsNullOrWhiteSpace(fixturePath))
            return null;

        var info = new FileInfo(Path.GetFullPath(fixturePath));
        if (!info.Exists
            || info.Length is <= 0 or > MaximumFixtureBytes
            || info.Attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            throw new InvalidDataException("The startup fixture is missing or unsafe.");
        }

        var fixture = JsonSerializer.Deserialize<StartupEntryFixtureDocument>(
            File.ReadAllText(info.FullName),
            JsonOptions) ?? throw new InvalidDataException("The startup fixture is empty.");
        Validate(fixture);
        return new StartupEntryControlFixtureStore(
            fixture,
            clock ?? (() => DateTimeOffset.UtcNow));
    }

    public Task<StartupEntryCaptureResult> CaptureAsync(
        BackgroundComponentObservation observation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (!_enabled)
                return Task.FromResult(StartupEntryCaptureResult.Refused("Fixture startup entry is disabled."));
            if (!MatchesObservation(observation))
                return Task.FromResult(StartupEntryCaptureResult.Refused("Fixture startup identity does not match."));

            _currentState = StartupEntryStateFactory.Create(
                observation,
                _fixture.ValueKind,
                _fixture.ValueData,
                _fixture.KeyAclSha256,
                _clock());
            return Task.FromResult(StartupEntryCaptureResult.Completed(_currentState));
        }
    }

    public Task<StartupEntryMutationResult> DisableAsync(
        StartupEntryState expected,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (!_enabled || !MatchesCurrentState(expected))
            {
                return Task.FromResult(StartupEntryMutationResult.Refused(
                    "Fixture startup evidence changed; disable was refused."));
            }

            _enabled = false;
            return Task.FromResult(StartupEntryMutationResult.Completed(
                "Fixture startup entry was disabled in memory."));
        }
    }

    public Task<StartupEntryMutationResult> RestoreAsync(
        StartupEntryState expected,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (_enabled || !MatchesCurrentState(expected))
            {
                return Task.FromResult(StartupEntryMutationResult.Refused(
                    "Fixture startup evidence changed; restore was refused."));
            }

            _enabled = true;
            return Task.FromResult(StartupEntryMutationResult.Completed(
                "Fixture startup entry was restored in memory."));
        }
    }

    private bool MatchesObservation(BackgroundComponentObservation observation)
    {
        if (!StartupEntryControlPolicy.IsSupportedObservation(observation)
            || !string.Equals(
                observation.Identity.DisplayName,
                _fixture.ValueName,
                StringComparison.Ordinal))
        {
            return false;
        }

        var expected = BackgroundComponentObservationFactory.Startup(
            _fixture.ValueName,
            StartupEntryControlPolicy.SupportedSourceLocator,
            _fixture.ValueData,
            observation.ObservedAtUtc,
            observation.StartupApproval);
        return string.Equals(
                   expected.Identity.StableId,
                   observation.Identity.StableId,
                   StringComparison.OrdinalIgnoreCase)
               && string.Equals(
                   expected.ObservationFingerprint,
                   observation.ObservationFingerprint,
                   StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesCurrentState(StartupEntryState expected) =>
        _currentState is not null
        && StartupEntryStateFactory.Verify(expected)
        && string.Equals(
            _currentState.StateFingerprint,
            expected.StateFingerprint,
            StringComparison.OrdinalIgnoreCase);

    private static void Validate(StartupEntryFixtureDocument fixture)
    {
        if (!StartupEntryControlPolicy.IsSafeValueName(fixture.ValueName)
            || !Enum.IsDefined(fixture.ValueKind)
            || string.IsNullOrWhiteSpace(fixture.ValueData)
            || fixture.ValueData.Length > StartupEntryStateFactory.MaximumValueDataLength
            || fixture.ValueData.Contains('\0')
            || !StartupEntryControlPolicy.IsSha256(fixture.KeyAclSha256))
        {
            throw new InvalidDataException("The startup fixture is incomplete.");
        }
    }

    private sealed class StartupEntryFixtureDocument
    {
        public string ValueName { get; init; } = string.Empty;
        public StartupRegistryValueKind ValueKind { get; init; }
        public string ValueData { get; init; } = string.Empty;
        public string KeyAclSha256 { get; init; } = string.Empty;
    }
}
