using Css.Snapshot.Uninstall;

namespace Css.Elevated.Uninstall;

public enum WindowsBackgroundEntryState
{
    Exists,
    Missing,
    Unknown
}

public interface IWindowsBackgroundEntryReader
{
    WindowsBackgroundEntryState ProbeStartupEntry(string name);
    WindowsBackgroundEntryState ProbeService(string name);
    WindowsBackgroundEntryState ProbeScheduledTask(string name);
}

public sealed class OfficialUninstallBackgroundScanResult
{
    public required bool Success { get; init; }
    public IReadOnlyList<string> ExistingStartupEntries { get; init; } = [];
    public IReadOnlyList<string> ExistingServices { get; init; } = [];
    public IReadOnlyList<string> ExistingScheduledTasks { get; init; } = [];
    public int UnverifiedHintCount { get; init; }

    public int VerifiedResidueCount =>
        ExistingStartupEntries.Count + ExistingServices.Count + ExistingScheduledTasks.Count;
}

public interface IOfficialUninstallBackgroundScanner
{
    Task<OfficialUninstallBackgroundScanResult> ScanAsync(CancellationToken cancellationToken);
}

public sealed class WindowsOfficialUninstallBackgroundScanner : IOfficialUninstallBackgroundScanner
{
    private readonly UninstallEvidenceSnapshotManifest _manifest;
    private readonly IWindowsBackgroundEntryReader _reader;

    public WindowsOfficialUninstallBackgroundScanner(
        UninstallEvidenceSnapshotManifest manifest,
        IWindowsBackgroundEntryReader reader)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(reader);
        _manifest = manifest;
        _reader = reader;
    }

    public Task<OfficialUninstallBackgroundScanResult> ScanAsync(
        CancellationToken cancellationToken)
    {
        var startup = Probe(
            _manifest.StartupEntries,
            _reader.ProbeStartupEntry,
            cancellationToken);
        var services = Probe(
            _manifest.Services,
            _reader.ProbeService,
            cancellationToken);
        var tasks = Probe(
            _manifest.ScheduledTasks,
            _reader.ProbeScheduledTask,
            cancellationToken);
        var unverified = startup.UnknownCount + services.UnknownCount + tasks.UnknownCount;

        return Task.FromResult(new OfficialUninstallBackgroundScanResult
        {
            Success = unverified == 0,
            ExistingStartupEntries = startup.Existing,
            ExistingServices = services.Existing,
            ExistingScheduledTasks = tasks.Existing,
            UnverifiedHintCount = unverified
        });
    }

    private static ProbeResult Probe(
        IEnumerable<string> identifiers,
        Func<string, WindowsBackgroundEntryState> probe,
        CancellationToken cancellationToken)
    {
        var existing = new List<string>();
        var unknown = 0;

        foreach (var identifier in identifiers
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (probe(identifier))
            {
                case WindowsBackgroundEntryState.Exists:
                    existing.Add(identifier);
                    break;
                case WindowsBackgroundEntryState.Unknown:
                    unknown++;
                    break;
            }
        }

        return new ProbeResult(existing, unknown);
    }

    private sealed record ProbeResult(IReadOnlyList<string> Existing, int UnknownCount);
}
