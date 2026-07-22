using System.Linq;
using Css.Core.Operations;

namespace Css.Core.Software;

public static class UninstallResidueOperationPlanner
{
    public static OperationDescriptor? CreateLowRiskQuarantineOperation(UninstallResidueScanReport report)
    {
        var lowRiskPaths = report.Groups
            .Where(group => group.Risk == RiskLevel.Low && group.CanMoveToQuarantine)
            .SelectMany(group => group.Candidates)
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Path))
            .ToList();

        if (lowRiskPaths.Count == 0)
            return null;

        return new OperationDescriptor
        {
            Kind = "uninstall.residue.quarantine",
            Title = $"清理 {report.SoftwareName} 低风险卸载残留",
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = report.Summary,
            EstimatedImpactBytes = lowRiskPaths.Sum(candidate => candidate.EstimatedBytes),
            ConfirmationText = $"确认将 {report.SoftwareName} 的低风险卸载残留移动到隔离区？",
            AffectedPaths = lowRiskPaths.Select(candidate => candidate.Path!).Distinct(System.StringComparer.OrdinalIgnoreCase).ToList()
        };
    }
}
