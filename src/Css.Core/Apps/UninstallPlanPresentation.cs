using System.Collections.Generic;
using Css.Core.Operations;
using Css.Core.Recovery;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class UninstallPlanDecisionSummaryViewModel
{
    public required string Conclusion { get; init; }
    public required string ProcessSummary { get; init; }
    public required string ResidueSummary { get; init; }
    public required string UndoSummary { get; init; }
    public required string NextStep { get; init; }
}

public sealed class UninstallPlanPreviewViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string OfficialUninstallerLine { get; init; }
    public required string PostUninstallScanLine { get; init; }
    public required UninstallRecoveryAssessmentViewModel RecoveryAssessment { get; init; }
    public required ReinstallSourceReadinessViewModel ReinstallReadiness { get; init; }
    public required UninstallRecoveryPreparationViewModel RecoveryPreparation { get; init; }
    public required UninstallWorkflowGuideViewModel WorkflowGuide { get; init; }
    public required OfficialUninstallConfirmationViewModel OfficialConfirmation { get; init; }
    public required bool CanRunOfficialUninstaller { get; init; }
    public required IReadOnlyList<UninstallPlanSectionViewModel> Sections { get; init; }
    public required string FinalReminder { get; init; }
    public UninstallPlanDecisionSummaryViewModel DecisionSummary =>
        UninstallPlanDecisionSummaryPresenter.Create(this);
}

public sealed class UninstallPlanSectionViewModel
{
    public required string Title { get; init; }
    public required string RiskLabel { get; init; }
    public required string ActionLine { get; init; }
    public required IReadOnlyList<string> Items { get; init; }
}

public static class UninstallPlanDecisionSummaryPresenter
{
    public static UninstallPlanDecisionSummaryViewModel Create(UninstallPlanPreviewViewModel preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        return new UninstallPlanDecisionSummaryViewModel
        {
            Conclusion = "现在只查看方案，不会卸载软件，也不会处理残留。",
            ProcessSummary = "真正开始后，OMNIX 会先运行这个软件自己的官方卸载器，再重新扫描确认结果。",
            ResidueSummary = "卸载完成后只把确认过的低风险缓存或日志列入隔离方案；服务、启动项和系统配置不会自动处理。",
            UndoSummary = preview.RecoveryAssessment.UndoHeadline,
            NextStep = preview.RecoveryAssessment.NextAction
        };
    }
}

public static class UninstallPlanPresentationBuilder
{
    public static UninstallPlanPreviewViewModel Create(
        SoftwareProfile profile,
        Func<string, bool>? reinstallSourceFileExists = null,
        Func<string, bool>? reinstallSourceDirectoryExists = null,
        Func<string, string?>? reinstallSourceSignatureResolver = null,
        IReadOnlyList<WindowsRestorePointInfo>? restorePoints = null,
        WindowsRestorePointScanState restorePointScanState = WindowsRestorePointScanState.Completed)
    {
        var plan = UninstallPlanBuilder.Create(profile);
        var workflowGuide = UninstallWorkflowGuidePresenter.Create(profile);
        var reinstallReadiness = ReinstallSourceReadinessPresenter.Create(
            profile,
            reinstallSourceFileExists ?? (_ => false),
            reinstallSourceDirectoryExists ?? (_ => false),
            reinstallSourceSignatureResolver ?? (_ => null));
        var recoveryPreparation = UninstallRecoveryPreparationPresenter.Create(
            profile,
            reinstallReadiness,
            restorePoints ?? [],
            personalDataBackupAcknowledged: false,
            restorePointScanState);
        var sections = new List<UninstallPlanSectionViewModel>
        {
            new()
            {
                Title = "\u7b2c\u4e00\u6b65\uff1a\u5b98\u65b9\u5378\u8f7d",
                RiskLabel = "\u9700\u8981\u786e\u8ba4",
                ActionLine = "\u5148\u8ba9\u8f6f\u4ef6\u81ea\u5df1\u7684\u5378\u8f7d\u5668\u5904\u7406\u4e3b\u4f53\u6587\u4ef6\u3002\u6b64\u9875\u4e0d\u4f1a\u76f4\u63a5\u8fd0\u884c\uff1b\u5b8c\u6210\u6062\u590d\u51c6\u5907\u548c\u6700\u7ec8\u786e\u8ba4\u540e\u624d\u80fd\u7ee7\u7eed\u3002",
                Items = [profile.UninstallCommand ?? "\u672a\u53d1\u73b0\u5b98\u65b9\u5378\u8f7d\u547d\u4ee4"]
            }
        };

        foreach (var group in plan.ResidueGroups)
        {
            sections.Add(new UninstallPlanSectionViewModel
            {
                Title = group.Title,
                RiskLabel = RiskLabel(group.Risk),
                ActionLine = group.CanMoveToQuarantine
                    ? "\u5378\u8f7d\u540e\u53ef\u518d\u6b21\u786e\u8ba4\uff0c\u4f4e\u98ce\u9669\u6b8b\u7559\u53ef\u8fdb\u9694\u79bb\u533a\u3002"
                    : "\u53ea\u89e3\u91ca\uff0c\u4e0d\u81ea\u52a8\u5904\u7406\uff1b\u9700\u8981\u5feb\u7167\u548c\u56de\u6eda\u65b9\u6848\u540e\u624d\u53ef\u7ee7\u7eed\u3002",
                Items = group.Candidates
            });
        }

        if (plan.ResidueGroups.Count == 0)
        {
            sections.Add(new UninstallPlanSectionViewModel
            {
                Title = "\u6b8b\u7559\u626b\u63cf",
                RiskLabel = "\u5f85\u626b\u63cf",
                ActionLine = "\u5378\u8f7d\u540e\u9700\u8981\u91cd\u65b0\u626b\u63cf\u6b8b\u7559\uff0c\u518d\u51b3\u5b9a\u662f\u5426\u8fdb\u5165\u9694\u79bb\u533a\u3002",
                Items = ["\u5f53\u524d\u6ca1\u6709\u9884\u5148\u53d1\u73b0\u7684\u6b8b\u7559\u5019\u9009\u3002"]
            });
        }

        return new UninstallPlanPreviewViewModel
        {
            Title = profile.Name + " \u5378\u8f7d\u5b89\u5168\u65b9\u6848",
            Summary = "只预览：此页不会直接运行卸载器，也不会删除任何残留。正式版本会在最终确认后先运行官方卸载器，完成后再单独复查残留。",
            OfficialUninstallerLine = "\u5b98\u65b9\u5378\u8f7d\u5668\uff1a" + (profile.UninstallCommand ?? "\u672a\u53d1\u73b0"),
            PostUninstallScanLine = BuildPostUninstallScanLine(profile),
            RecoveryAssessment = UninstallRecoveryAssessmentPresenter.Create(profile),
            ReinstallReadiness = reinstallReadiness,
            RecoveryPreparation = recoveryPreparation,
            WorkflowGuide = workflowGuide,
            OfficialConfirmation = OfficialUninstallConfirmationBuilder.Create(profile),
            CanRunOfficialUninstaller = false,
            Sections = sections,
            FinalReminder = "\u771f\u6b63\u5904\u7406\u6b8b\u7559\u524d\u5fc5\u987b\u518d\u6b21\u786e\u8ba4\uff1b\u53ef\u8fd8\u539f\u7684\u5185\u5bb9\u4f1a\u8fdb\u5165\u540e\u6094\u836f\u4e2d\u5fc3\u3002"
        };
    }

    private static string BuildPostUninstallScanLine(SoftwareProfile profile)
    {
        var preview = UninstallResidueScanBuilder.Build(
            profile,
            afterProfiles: [],
            pathExists: _ => true);
        var lowRiskCount = preview.Groups
            .Where(group => group.Risk == RiskLevel.Low)
            .Sum(group => group.Candidates.Count);
        var protectedCount = preview.Groups
            .Where(group => group.Risk != RiskLevel.Low)
            .Sum(group => group.Candidates.Count);

        return $"\u5378\u8f7d\u540e\u91cd\u65b0\u626b\u63cf\u6b8b\u7559\uff1a\u4f4e\u98ce\u9669 {lowRiskCount} \u9879\u53ef\u786e\u8ba4\u540e\u8fdb\u9694\u79bb\u533a\uff0c" +
               $"\u4e2d/\u9ad8\u98ce\u9669 {protectedCount} \u9879\u53ea\u89e3\u91ca\u6216\u8981\u6c42\u989d\u5916\u5feb\u7167\uff1b\u4e0d\u4f1a\u81ea\u52a8\u5220\u9664\u3002";
    }

    private static string RiskLabel(RiskLevel risk) =>
        risk switch
        {
            RiskLevel.None => "\u65e0",
            RiskLevel.Low => "\u4f4e",
            RiskLevel.Medium => "\u4e2d",
            RiskLevel.High => "\u9ad8",
            _ => risk.ToString()
        };
}
