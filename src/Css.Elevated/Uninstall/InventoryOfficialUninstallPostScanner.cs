using Css.Core.Software;
using Css.Core.Uninstall;
using Css.Snapshot.Uninstall;

namespace Css.Elevated.Uninstall;

public sealed class InventoryOfficialUninstallPostScanner : IOfficialUninstallPostScanner
{
    private readonly UninstallEvidenceSnapshotManifest _manifest;
    private readonly Func<CancellationToken, Task<IReadOnlyList<SoftwareProfile>>> _inventoryScan;
    private readonly Func<string, bool> _pathExists;
    private readonly Func<string, long>? _sizeResolver;
    private readonly IOfficialUninstallBackgroundScanner? _backgroundScanner;

    public InventoryOfficialUninstallPostScanner(
        UninstallEvidenceSnapshotManifest manifest,
        Func<CancellationToken, Task<IReadOnlyList<SoftwareProfile>>> inventoryScan,
        Func<string, bool> pathExists,
        Func<string, long>? sizeResolver = null,
        IOfficialUninstallBackgroundScanner? backgroundScanner = null)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(inventoryScan);
        ArgumentNullException.ThrowIfNull(pathExists);
        _manifest = manifest;
        _inventoryScan = inventoryScan;
        _pathExists = pathExists;
        _sizeResolver = sizeResolver;
        _backgroundScanner = backgroundScanner;
    }

    public async Task<OfficialUninstallPostScanResult> ScanAsync(
        string softwareName,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(softwareName, _manifest.SoftwareName, StringComparison.OrdinalIgnoreCase))
        {
            return Failed("复查目标与卸载前证据不一致，已停止扫描。");
        }

        try
        {
            var currentProfiles = await _inventoryScan(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            OfficialUninstallBackgroundScanResult? background = null;
            if (_backgroundScanner is not null)
            {
                background = await _backgroundScanner.ScanAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (!background.Success)
                {
                    return Failed(
                        "后台项目复查未完成，不能确认卸载是否干净。",
                        background.UnverifiedHintCount);
                }
            }

            var report = UninstallResidueScanBuilder.Build(
                CreateBeforeProfile(background),
                currentProfiles,
                _pathExists,
                _sizeResolver);
            var residueCount = report.Groups.Sum(group => group.Candidates.Count);
            var pathResidueCount = report.Groups
                .SelectMany(group => group.Candidates)
                .Count(candidate => candidate.Path is not null);
            var verifiedBackgroundCount = background?.VerifiedResidueCount ?? 0;
            var backgroundHintCount = background is null ? CountBackgroundHints() : 0;
            var stillPresent = !report.OfficialUninstallAppearsComplete;

            return new OfficialUninstallPostScanResult
            {
                Success = true,
                SoftwareStillPresent = stillPresent,
                ResidueCandidateCount = residueCount,
                PathResidueCandidateCount = pathResidueCount,
                VerifiedBackgroundResidueCount = verifiedBackgroundCount,
                UnverifiedBackgroundHintCount = backgroundHintCount,
                RequiresBackgroundRescan = backgroundHintCount > 0,
                ResidueReport = report,
                Summary = BuildSummary(
                    stillPresent,
                    pathResidueCount,
                    verifiedBackgroundCount,
                    backgroundHintCount)
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Failed($"卸载后复查失败：{exception.Message}");
        }
    }

    private SoftwareProfile CreateBeforeProfile(OfficialUninstallBackgroundScanResult? background) =>
        new()
        {
            Name = _manifest.SoftwareName,
            Publisher = _manifest.Publisher,
            InstallPath = _manifest.InstallPath,
            UninstallCommand = _manifest.UninstallCommand,
            DataPaths = _manifest.DataPaths,
            CachePaths = _manifest.CachePaths,
            LogPaths = _manifest.LogPaths,
            StartupEntries = background?.ExistingStartupEntries ?? [],
            Services = background?.ExistingServices ?? [],
            ScheduledTasks = background?.ExistingScheduledTasks ?? []
        };

    private int CountBackgroundHints() =>
        _manifest.StartupEntries
            .Concat(_manifest.Services)
            .Concat(_manifest.ScheduledTasks)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

    private static string BuildSummary(
        bool softwareStillPresent,
        int pathResidueCount,
        int verifiedBackgroundCount,
        int backgroundHintCount)
    {
        if (softwareStillPresent)
            return "仍能找到这个软件，暂不处理任何残留。";

        if (pathResidueCount == 0 && verifiedBackgroundCount == 0 && backgroundHintCount == 0)
            return "本次只读复查没有发现可见残留。";

        if (backgroundHintCount > 0)
            return $"只读复查发现 {pathResidueCount} 项目录残留候选，另有 {backgroundHintCount} 项后台记录需要专项复查。";

        return $"只读复查发现 {pathResidueCount} 项目录残留候选和 {verifiedBackgroundCount} 项后台残留候选，尚未处理。";
    }

    private static OfficialUninstallPostScanResult Failed(
        string summary,
        int unverifiedBackgroundHintCount = 0) =>
        new()
        {
            Success = false,
            UnverifiedBackgroundHintCount = unverifiedBackgroundHintCount,
            RequiresBackgroundRescan = unverifiedBackgroundHintCount > 0,
            Summary = summary
        };
}
