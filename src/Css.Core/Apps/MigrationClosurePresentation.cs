using Css.Core.Migration;
using Css.Core.Operations;
using Css.Core.Recommendations;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class MigrationClosureSummaryViewModel
{
    public required string SoftwareName { get; init; }
    public required string DisplayName { get; init; }
    public string? TargetAppNameCandidate { get; init; }
    public required MigrationClosureFindingKind State { get; init; }
    public required string Headline { get; init; }
    public required string Detail { get; init; }
    public required int ObservedPathCount { get; init; }
    public required DateTimeOffset MonitoringStartedAtUtc { get; init; }
    public bool NeedsAttention => State != MigrationClosureFindingKind.RedirectHealthy;
    public bool CanExecuteDirectly => false;
}

public sealed class MigrationClosureDrawerStateViewModel
{
    public required string AdviceText { get; init; }
    public required string ButtonText { get; init; }
    public required bool CanOpenPlan { get; init; }
    public required string ButtonReason { get; init; }
    public bool CanExecuteDirectly => false;
}

public static class MigrationClosureDrawerStatePresenter
{
    public static MigrationClosureDrawerStateViewModel Create(
        SoftwareProfile profile,
        AppDrawerViewModel drawer,
        MigrationClosureSummaryViewModel? closure)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(drawer);

        var baseAction = drawer.AvailableActions
            .Single(action => action.Kind == AppActionKind.Migration);
        if (closure?.NeedsAttention != true)
        {
            return new MigrationClosureDrawerStateViewModel
            {
                AdviceText = closure is null
                    ? drawer.AgentAdvice.Text
                    : closure.Headline + "\n" + closure.Detail + "\n" + drawer.AgentAdvice.Text,
                ButtonText = baseAction.Label,
                CanOpenPlan = baseAction.IsEnabled,
                ButtonReason = baseAction.Reason
            };
        }

        var canReviewClosure = AppPresentationBuilder.CanReviewMigrationClosure(profile);
        return new MigrationClosureDrawerStateViewModel
        {
            AdviceText = canReviewClosure
                ? closure.Headline + "\n" + closure.Detail
                : drawer.AgentAdvice.Text + "\n迁移记录提醒：" + closure.Headline + "。" + closure.Detail,
            ButtonText = "复查迁移",
            CanOpenPlan = canReviewClosure,
            ButtonReason = canReviewClosure
                ? "重新扫描并生成新的迁移安全方案；旧记录不会直接执行。"
                : baseAction.Reason
        };
    }
}

public sealed class MigrationClosureTileStateViewModel
{
    public required string ShortTag { get; init; }
    public required AppTileStatus Status { get; init; }
    public required bool ShouldPrioritize { get; init; }
}

public static class MigrationClosureTileStatePresenter
{
    public static bool ShouldPrioritize(
        SoftwareProfile profile,
        MigrationClosureSummaryViewModel? closure) =>
        closure?.NeedsAttention == true
        && AppPresentationBuilder.CanReviewMigrationClosure(profile);

    public static MigrationClosureTileStateViewModel Create(
        SoftwareProfile profile,
        AppTileViewModel tile,
        MigrationClosureSummaryViewModel? closure)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(tile);

        var shouldPrioritize = ShouldPrioritize(profile, closure);
        if (shouldPrioritize)
        {
            return new MigrationClosureTileStateViewModel
            {
                ShortTag = "迁移未闭环",
                Status = AppTileStatus.Attention,
                ShouldPrioritize = true
            };
        }

        var showHealthyClosure = closure is { NeedsAttention: false }
            && AppPresentationBuilder.CanReviewMigrationClosure(profile)
            && tile.Status == AppTileStatus.Normal;
        return new MigrationClosureTileStateViewModel
        {
            ShortTag = showHealthyClosure ? "迁移正常" : tile.ShortTag,
            Status = tile.Status,
            ShouldPrioritize = false
        };
    }
}

public sealed class MigrationClosureCatalogSummaryViewModel
{
    public required int ReviewableRecordCount { get; init; }
    public required int AttentionCount { get; init; }
    public required int ProtectedHistoricalRecordCount { get; init; }
    public required string Text { get; init; }
}

public static class MigrationClosureCatalogSummaryPresenter
{
    public static MigrationClosureCatalogSummaryViewModel Create(
        IEnumerable<SoftwareProfile> profiles,
        Func<SoftwareProfile, MigrationClosureSummaryViewModel?> closureResolver)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        ArgumentNullException.ThrowIfNull(closureResolver);

        var records = profiles
            .Select(profile => (Profile: profile, Closure: closureResolver(profile)))
            .Where(item => item.Closure is not null)
            .ToArray();
        var reviewable = records
            .Where(item => AppPresentationBuilder.CanReviewMigrationClosure(item.Profile))
            .ToArray();
        var attention = reviewable.Count(item => item.Closure?.NeedsAttention == true);
        var protectedHistorical = records.Length - reviewable.Length;
        var protectedText = protectedHistorical == 0
            ? ""
            : $" 另有 {protectedHistorical} 条系统相关旧迁移记录，仅供查看。";
        var text = reviewable.Length switch
        {
            0 => protectedText,
            _ when attention == 0 =>
                $" 已复查 {reviewable.Length} 个普通应用的迁移，闭环正常。" + protectedText,
            _ =>
                $" 迁移复查：{reviewable.Length} 个普通应用中有 {attention} 个需要检查。" + protectedText
        };

        return new MigrationClosureCatalogSummaryViewModel
        {
            ReviewableRecordCount = reviewable.Length,
            AttentionCount = attention,
            ProtectedHistoricalRecordCount = protectedHistorical,
            Text = text
        };
    }
}

public static class MigrationClosurePresenter
{
    public static IReadOnlyList<MigrationClosureSummaryViewModel> CreateLatest(
        IEnumerable<MigrationClosureFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);

        return findings
            .Where(finding => !string.IsNullOrWhiteSpace(finding.SoftwareName))
            .GroupBy(finding => finding.SoftwareName.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(CreateLatestForSoftware)
            .Where(summary => summary is not null)
            .Cast<MigrationClosureSummaryViewModel>()
            .OrderByDescending(summary => summary.NeedsAttention)
            .ThenByDescending(summary => Severity(summary.State))
            .ThenBy(summary => summary.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private static MigrationClosureSummaryViewModel? CreateLatestForSoftware(
        IGrouping<string, MigrationClosureFinding> softwareFindings)
    {
        var latestRecord = softwareFindings
            .GroupBy(
                finding => string.IsNullOrWhiteSpace(finding.MonitoringRecordId)
                    ? "record-without-id"
                    : finding.MonitoringRecordId,
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                Id = group.Key,
                StartedAtUtc = group.Max(finding => finding.MonitoringStartedAtUtc),
                Findings = group.ToArray()
            })
            .OrderByDescending(record => record.StartedAtUtc)
            .ThenByDescending(record => record.Id, StringComparer.Ordinal)
            .FirstOrDefault();
        if (latestRecord is null || latestRecord.Findings.Length == 0)
            return null;

        var state = latestRecord.Findings
            .OrderByDescending(finding => Severity(finding.Kind))
            .First()
            .Kind;
        var rawName = softwareFindings.Key;
        var safeName = SafeSoftwareName(rawName);
        var copy = CopyFor(state, safeName.DisplayName);
        return new MigrationClosureSummaryViewModel
        {
            SoftwareName = rawName,
            DisplayName = safeName.DisplayName,
            TargetAppNameCandidate = safeName.TargetAppName,
            State = state,
            Headline = copy.Headline,
            Detail = copy.Detail,
            ObservedPathCount = latestRecord.Findings.Length,
            MonitoringStartedAtUtc = latestRecord.StartedAtUtc
        };
    }

    private static (string DisplayName, string? TargetAppName) SafeSoftwareName(string name)
    {
        var value = name.Trim();
        var unsafeName = value.Length is 0 or > 120
            || value.Any(char.IsControl)
            || value.Contains(':')
            || value.Contains('\\')
            || value.Contains('/');
        return unsafeName
            ? ("某个已迁移应用", null)
            : (value, value);
    }

    private static (string Headline, string Detail) CopyFor(
        MigrationClosureFindingKind state,
        string displayName) =>
        state switch
        {
            MigrationClosureFindingKind.OriginalWriteReturned =>
                ($"{displayName} 的迁移没有闭环",
                    "它又在 C 盘原位置建立了真实文件夹，空间可能继续增长。先不要手动移动，请重新扫描并生成新方案。"),
            MigrationClosureFindingKind.RedirectTargetChanged =>
                ($"{displayName} 的迁移需要检查",
                    "原位置现在指向了意外位置。先暂停继续迁移，重新核对快照、目标位置和回滚记录。"),
            MigrationClosureFindingKind.OriginalPathMissing =>
                ($"{displayName} 的迁移引导位置不见了",
                    "目前无法确认软件是否仍会写回 C 盘。先重新扫描应用和迁移状态，不要根据这个提示自行移动文件。"),
            _ =>
                ($"{displayName} 的迁移闭环正常",
                    "原 C 盘位置仍在把后续写入引导到已验证的目标位置，目前不需要处理。")
        };

    private static int Severity(MigrationClosureFindingKind kind) =>
        kind switch
        {
            MigrationClosureFindingKind.OriginalWriteReturned => 4,
            MigrationClosureFindingKind.RedirectTargetChanged => 3,
            MigrationClosureFindingKind.OriginalPathMissing => 2,
            _ => 1
        };
}

public enum MigrationClosureTargetDisposition
{
    Unavailable,
    Reviewable,
    ProtectedHistorical
}

public static class MigrationClosureHealthEnricher
{
    private const string DimensionName = "迁移闭环";

    public static HealthCheckSummary Apply(
        HealthCheckSummary health,
        IEnumerable<MigrationClosureSummaryViewModel> closureSummaries,
        Func<string, MigrationClosureTargetDisposition>? resolveTarget = null)
    {
        ArgumentNullException.ThrowIfNull(health);
        ArgumentNullException.ThrowIfNull(closureSummaries);

        var summaries = closureSummaries.ToArray();
        var dimensions = health.Dimensions
            .Where(dimension => !dimension.Name.Equals(DimensionName, StringComparison.Ordinal))
            .ToList();
        var findings = health.KeyFindings
            .Where(finding => finding.Kind != HealthFindingKind.MigrationClosure)
            .ToList();

        if (summaries.Length == 0)
        {
            return new HealthCheckSummary
            {
                OverallScore = health.OverallScore,
                Dimensions = dimensions,
                KeyFindings = findings,
                Hardware = health.Hardware
            };
        }

        var attention = summaries
            .Where(summary => summary.NeedsAttention)
            .Select(summary => new
            {
                Summary = summary,
                Disposition = summary.TargetAppNameCandidate is { } candidate
                    ? resolveTarget?.Invoke(candidate) ?? MigrationClosureTargetDisposition.Unavailable
                    : MigrationClosureTargetDisposition.Unavailable
            })
            .OrderByDescending(item => item.Disposition == MigrationClosureTargetDisposition.Reviewable)
            .ThenByDescending(item => item.Summary.MonitoringStartedAtUtc)
            .ThenBy(item => item.Summary.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        var reviewableAttention = attention.Count(item =>
            item.Disposition == MigrationClosureTargetDisposition.Reviewable);
        var readOnlyAttention = attention.Length - reviewableAttention;
        var dimensionResult = attention.Length == 0
            ? $"正在观察 {summaries.Length} 个已迁移应用，闭环正常"
            : reviewableAttention > 0 && readOnlyAttention > 0
                ? $"正在观察 {summaries.Length} 个已迁移应用，{reviewableAttention} 个普通应用需要复查，{readOnlyAttention} 条旧记录仅供查看"
                : reviewableAttention > 0
                    ? $"正在观察 {summaries.Length} 个已迁移应用，{reviewableAttention} 个普通应用需要复查"
                    : $"正在观察 {summaries.Length} 个已迁移应用，{readOnlyAttention} 条旧记录仅供查看";
        dimensions.Insert(Math.Min(4, dimensions.Count), new HealthDimensionResult
        {
            Name = DimensionName,
            Result = dimensionResult,
            Rating = attention.Length == 0
                ? "正常"
                : reviewableAttention > 0
                    ? "需关注"
                    : "仅供查看"
        });

        findings.InsertRange(0, attention.Take(3).Select(item => new HealthFinding
        {
            Text = item.Disposition switch
            {
                MigrationClosureTargetDisposition.Reviewable =>
                    item.Summary.Headline + "。" + item.Summary.Detail,
                MigrationClosureTargetDisposition.ProtectedHistorical =>
                    "系统相关旧迁移记录，仅供查看：" + item.Summary.Headline + "。" + item.Summary.Detail,
                _ =>
                    "旧迁移记录目前无法唯一对应应用，仅供查看：" + item.Summary.Headline + "。" + item.Summary.Detail
            },
            Kind = HealthFindingKind.MigrationClosure,
            TargetAppName = item.Disposition == MigrationClosureTargetDisposition.Reviewable
                ? item.Summary.TargetAppNameCandidate
                : null,
            Action = item.Disposition == MigrationClosureTargetDisposition.Reviewable
                ? RecommendationAction.Migrate
                : RecommendationAction.Observe,
            Risk = RiskLevel.Medium
        }));

        return new HealthCheckSummary
        {
            OverallScore = health.OverallScore,
            Dimensions = dimensions,
            KeyFindings = findings,
            Hardware = health.Hardware
        };
    }
}
