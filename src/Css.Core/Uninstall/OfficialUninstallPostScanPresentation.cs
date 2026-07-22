using Css.Core.Operations;

namespace Css.Core.Uninstall;

public static class OfficialUninstallPostScanPresenter
{
    public static OfficialUninstallPostScanViewModel Create(
        string softwareName,
        OfficialUninstallPostScanResult result)
    {
        if (string.IsNullOrWhiteSpace(softwareName))
            throw new ArgumentException("Software name is required.", nameof(softwareName));
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Success)
            return ScanFailed();
        if (result.SoftwareStillPresent)
            return SoftwareStillPresent();
        if (result.ResidueCandidateCount == 0 && !result.RequiresBackgroundRescan)
            return NoVisibleResidue();

        return ReviewNeeded(result);
    }

    private static OfficialUninstallPostScanViewModel ScanFailed() =>
        new()
        {
            State = OfficialUninstallPostScanState.ScanFailed,
            Title = "\u8fd8\u4e0d\u80fd\u786e\u8ba4\u662f\u5426\u5378\u8f7d\u5e72\u51c0",
            StatusLabel = "\u9700\u8981\u91cd\u8bd5",
            Conclusion = "\u5378\u8f7d\u540e\u7684\u590d\u67e5\u6ca1\u6709\u5b8c\u6210\uff0c\u6211\u4e0d\u4f1a\u628a\u672a\u77e5\u60c5\u51b5\u8bf4\u6210\u5df2\u7ecf\u5e72\u51c0\u3002",
            Facts = ["\u672c\u6b21\u6ca1\u6709\u751f\u6210\u53ef\u5904\u7406\u7684\u6b8b\u7559\u7ed3\u8bba\u3002"],
            AgentAdvice = "\u8bf7\u7a0d\u540e\u91cd\u65b0\u626b\u63cf\uff1b\u5728\u7ed3\u679c\u51fa\u6765\u524d\uff0c\u4e0d\u8981\u624b\u52a8\u5220\u9664\u76f8\u5173\u76ee\u5f55\u3002",
            PrimaryActionText = "\u91cd\u65b0\u626b\u63cf",
            PrimaryAction = OfficialUninstallPostScanAction.RetryReadOnlyScan,
            TechnicalDetailsAvailable = true
        };

    private static OfficialUninstallPostScanViewModel SoftwareStillPresent() =>
        new()
        {
            State = OfficialUninstallPostScanState.SoftwareStillPresent,
            Title = "\u8f6f\u4ef6\u53ef\u80fd\u8fd8\u6ca1\u6709\u5378\u8f7d\u5b8c\u6210",
            StatusLabel = "\u5148\u522b\u6e05\u7406",
            Conclusion = "\u7cfb\u7edf\u91cc\u4ecd\u80fd\u627e\u5230\u8fd9\u4e2a\u8f6f\u4ef6\uff0c\u6240\u4ee5\u73b0\u5728\u628a\u6587\u4ef6\u5f53\u4f5c\u6b8b\u7559\u5e76\u4e0d\u5b89\u5168\u3002",
            Facts =
            [
                "\u5df2\u5b89\u88c5\u8f6f\u4ef6\u6e05\u5355\u4e2d\u4ecd\u80fd\u627e\u5230\u5b83\u3002",
                "\u5df2\u963b\u6b62\u6b8b\u7559\u5904\u7406\u3002",
                "\u6ca1\u6709\u79fb\u52a8\u6216\u5220\u9664\u4efb\u4f55\u5185\u5bb9\u3002"
            ],
            AgentAdvice = "\u786e\u8ba4\u5b98\u65b9\u5378\u8f7d\u5668\u5df2\u7ecf\u7ed3\u675f\uff0c\u5fc5\u8981\u65f6\u91cd\u542f\u7535\u8111\uff0c\u7136\u540e\u91cd\u65b0\u626b\u63cf\u3002",
            PrimaryActionText = "\u91cd\u65b0\u626b\u63cf",
            PrimaryAction = OfficialUninstallPostScanAction.RetryReadOnlyScan
        };

    private static OfficialUninstallPostScanViewModel NoVisibleResidue() =>
        new()
        {
            State = OfficialUninstallPostScanState.NoVisibleResidue,
            Title = "\u5378\u8f7d\u540e\u7684\u590d\u67e5\u5df2\u5b8c\u6210",
            StatusLabel = "\u6682\u672a\u53d1\u73b0\u6b8b\u7559",
            Conclusion = "\u8fd9\u6b21\u53ea\u8bfb\u590d\u67e5\u6ca1\u6709\u53d1\u73b0\u4ecd\u7136\u5b58\u5728\u7684\u5df2\u77e5\u76ee\u5f55\u6216\u540e\u53f0\u8bb0\u5f55\u3002",
            Facts =
            [
                "\u672c\u6b21\u53ea\u8bfb\u590d\u67e5\u6ca1\u6709\u4fee\u6539\u7535\u8111\u3002",
                "\u8fd9\u4e2a\u7ed3\u8bba\u53ea\u4ee3\u8868\u5f53\u524d\u5df2\u68c0\u67e5\u8303\u56f4\u3002"
            ],
            AgentAdvice = "\u5148\u7ee7\u7eed\u89c2\u5bdf\uff1b\u5982\u679c C \u76d8\u4e4b\u540e\u53c8\u589e\u957f\uff0c\u6211\u4f1a\u91cd\u65b0\u5b9a\u4f4d\u6765\u6e90\u3002",
            PrimaryActionText = "\u5b8c\u6210",
            PrimaryAction = OfficialUninstallPostScanAction.Close
        };

    private static OfficialUninstallPostScanViewModel ReviewNeeded(
        OfficialUninstallPostScanResult result)
    {
        var facts = new List<string>();
        var pathResidueCount = result.PathResidueCandidateCount > 0
            || result.VerifiedBackgroundResidueCount > 0
                ? result.PathResidueCandidateCount
                : result.ResidueCandidateCount;
        if (pathResidueCount > 0)
            facts.Add($"\u53d1\u73b0 {pathResidueCount} \u9879\u76ee\u5f55\u6b8b\u7559\u5019\u9009\u3002");
        if (result.VerifiedBackgroundResidueCount > 0)
            facts.Add($"\u53d1\u73b0 {result.VerifiedBackgroundResidueCount} \u9879\u4ecd\u5b58\u5728\u7684\u540e\u53f0\u8bb0\u5f55\u3002");
        if (result.RequiresBackgroundRescan)
            facts.Add($"\u8fd8\u6709 {result.UnverifiedBackgroundHintCount} \u9879\u540e\u53f0\u8bb0\u5f55\u9700\u8981\u91cd\u65b0\u786e\u8ba4\u3002");

        var hasLowRiskCandidate = result.ResidueReport?.Groups.Any(group =>
            group.Risk == RiskLevel.Low && group.CanMoveToQuarantine) == true;
        var canReviewResidue = result.ResidueCandidateCount > 0;
        var advice = new List<string>();
        if (hasLowRiskCandidate)
            advice.Add("\u53ea\u6709\u4f4e\u98ce\u9669\u7f13\u5b58\u6216\u65e5\u5fd7\u624d\u4f1a\u5728\u4f60\u786e\u8ba4\u540e\u8fdb\u5165\u9694\u79bb\u533a\u3002");
        if (result.VerifiedBackgroundResidueCount > 0)
            advice.Add("\u540e\u53f0\u8bb0\u5f55\u5c5e\u4e8e\u9ad8\u98ce\u9669\u9879\uff0c\u6211\u53ea\u4f1a\u5c55\u793a\u8bc1\u636e\uff0c\u4e0d\u4f1a\u76f4\u63a5\u5173\u95ed\u3002");
        if (result.RequiresBackgroundRescan)
            advice.Add("\u540e\u53f0\u590d\u67e5\u5b8c\u6210\u524d\u4e0d\u5efa\u8bae\u5904\u7406\u3002");
        if (advice.Count == 0)
            advice.Add("\u8bf7\u5148\u67e5\u770b\u6e05\u5355\uff0c\u76ee\u524d\u4e0d\u5efa\u8bae\u5904\u7406\u3002");

        return new OfficialUninstallPostScanViewModel
        {
            State = OfficialUninstallPostScanState.ReviewNeeded,
            Title = "\u53d1\u73b0\u5378\u8f7d\u540e\u7684\u5f85\u68c0\u67e5\u5185\u5bb9",
            StatusLabel = "\u9700\u8981\u68c0\u67e5",
            Conclusion = "\u8fd9\u4e9b\u5185\u5bb9\u4e0d\u4f1a\u81ea\u52a8\u5220\u9664\uff0c\u6211\u4f1a\u5148\u6309\u98ce\u9669\u5206\u7ec4\u7ed9\u4f60\u770b\u3002",
            Facts = facts,
            AgentAdvice = "\u53ef\u4ee5\u5148\u67e5\u770b\u6e05\u5355\uff1b" + string.Join(string.Empty, advice).Trim(),
            PrimaryActionText = canReviewResidue
                ? "\u67e5\u770b\u6b8b\u7559\u6e05\u5355"
                : "\u91cd\u65b0\u626b\u63cf\u540e\u53f0\u9879\u76ee",
            PrimaryAction = canReviewResidue
                ? OfficialUninstallPostScanAction.ReviewResidue
                : OfficialUninstallPostScanAction.RetryReadOnlyScan,
            CanReviewResidue = canReviewResidue,
            TechnicalDetailsAvailable = true
        };
    }
}
