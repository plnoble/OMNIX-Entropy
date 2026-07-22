using System.Collections.Generic;

namespace Css.Core.Software;

public static class UninstallResidueReviewPlanner
{
    public static UninstallResidueScanReport? TryBuildStillInstalledReport(
        SoftwareProfile before,
        IReadOnlyList<SoftwareProfile> currentProfiles)
    {
        if (currentProfiles.Count == 0)
            return null;

        var report = UninstallResidueScanBuilder.Build(
            before,
            currentProfiles,
            pathExists: _ => false);

        return report.OfficialUninstallAppearsComplete ? null : report;
    }
}
