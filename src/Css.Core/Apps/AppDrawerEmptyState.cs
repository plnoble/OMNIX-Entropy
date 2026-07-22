namespace Css.Core.Apps;

public sealed class AppDrawerEmptyStateViewModel
{
    public required string Title { get; init; }
    public required string SupportingText { get; init; }
    public required string CategorySummary { get; init; }
    public required string InstallLocationSummary { get; init; }
    public required string SizeSummary { get; init; }
    public required string ResidencySummary { get; init; }
    public required string AgentAdviceText { get; init; }
    public required string DisabledActionReason { get; init; }
}

public static class AppDrawerEmptyStatePresenter
{
    public static AppDrawerEmptyStateViewModel Create(string? reason) =>
        new()
        {
            Title = string.IsNullOrWhiteSpace(reason) ? "当前没有应用" : reason.Trim(),
            SupportingText = "可以调整分类、搜索词或排序方式。",
            CategorySummary = "当前没有应用，分类依据已清空。",
            InstallLocationSummary = "-",
            SizeSummary = "-",
            ResidencySummary = "-",
            AgentAdviceText = "当前没有应用可供分析。",
            DisabledActionReason = "请先选择应用，再查看可用方案。"
        };
}
