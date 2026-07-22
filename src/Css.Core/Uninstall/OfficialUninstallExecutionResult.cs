using Css.Core.Software;

namespace Css.Core.Uninstall;

public sealed class OfficialUninstallPostScanResult
{
    public required bool Success { get; init; }
    public bool SoftwareStillPresent { get; init; }
    public int ResidueCandidateCount { get; init; }
    public int PathResidueCandidateCount { get; init; }
    public int VerifiedBackgroundResidueCount { get; init; }
    public int UnverifiedBackgroundHintCount { get; init; }
    public bool RequiresBackgroundRescan { get; init; }
    public UninstallResidueScanReport? ResidueReport { get; init; }
    public required string Summary { get; init; }

    public static OfficialUninstallPostScanResult Completed(
        bool softwareStillPresent,
        int residueCandidateCount) =>
        new()
        {
            Success = true,
            SoftwareStillPresent = softwareStillPresent,
            ResidueCandidateCount = Math.Max(0, residueCandidateCount),
            Summary = softwareStillPresent
                ? "\u5378\u8f7d\u540e\u4ecd\u53d1\u73b0\u8f6f\u4ef6\u8bb0\u5f55\uff0c\u9700\u8981\u7ee7\u7eed\u89c2\u5bdf\u3002"
                : "\u5378\u8f7d\u540e\u5df2\u5b8c\u6210\u91cd\u65b0\u626b\u63cf\u3002"
        };

    public static OfficialUninstallPostScanResult NotRun(string summary) =>
        new() { Success = false, Summary = summary };
}

public sealed class OfficialUninstallHandlerPayload
{
    public required bool UninstallerStarted { get; init; }
    public required bool UninstallerCompleted { get; init; }
    public int? ExitCode { get; init; }
    public required OfficialUninstallPostScanResult PostScan { get; init; }
    public required bool RequiresPostScanRetry { get; init; }
}
