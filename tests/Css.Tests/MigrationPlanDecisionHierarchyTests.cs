using System.Xml.Linq;
using Css.Core.Apps;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationPlanDecisionHierarchyTests
{
    [Fact]
    public void Recommended_migration_is_explained_as_a_beginner_decision()
    {
        var preview = MigrationPlanTestData.CreateRecommendedPreview();

        var decision = MigrationPlanDecisionSummaryPresenter.Create(preview);

        decision.StatusLabel.Should().Be("需要先准备");
        decision.Conclusion.Should().Contain("可以规划迁移")
            .And.NotContain("\\");
        decision.TargetSummary.Should().Contain("D 盘")
            .And.NotContain("\\");
        decision.NextStep.Should().Contain("快照")
            .And.Contain("回滚");
        decision.RollbackSummary.Should().Contain("可以后悔");
        decision.SpaceSummary.Should().Be("D 盘空间够用");
    }

    [Fact]
    public void Migration_window_puts_decision_before_collapsed_technical_evidence()
    {
        var document = XDocument.Load(
            Path.Combine(FindRepositoryRoot(), "src", "Css.App", "MigrationPlanWindow.xaml"),
            LoadOptions.PreserveWhitespace);
        var conclusion = NamedElement(document, "MigrationPlanDecisionConclusionTextBlock");
        var nextStep = NamedElement(document, "MigrationPlanDecisionNextStepTextBlock");
        var rollback = NamedElement(document, "MigrationPlanDecisionRollbackTextBlock");
        var technical = NamedElement(document, "MigrationPlanTechnicalDetailsExpander");

        AutomationId(conclusion).Should().Be("MigrationPlanDecisionConclusionTextBlock");
        AutomationId(nextStep).Should().Be("MigrationPlanDecisionNextStepTextBlock");
        AutomationId(rollback).Should().Be("MigrationPlanDecisionRollbackTextBlock");
        AutomationId(technical).Should().Be("MigrationPlanTechnicalDetailsExpander");
        AttributeValue(technical, "IsExpanded").Should().Be("False");
        conclusion.IsBefore(technical).Should().BeTrue();
        nextStep.IsBefore(technical).Should().BeTrue();
        rollback.IsBefore(technical).Should().BeTrue();
        technical.Descendants().Any(element =>
                element.Attribute("Text")?.Value == "{Binding RollbackManifestLine}")
            .Should().BeTrue();
        technical.Descendants().Any(element =>
                element.Attribute("ItemsSource")?.Value == "{Binding ReadinessChecklist.Steps}")
            .Should().BeTrue();
        technical.Descendants().Any(element =>
                element.Attribute("ItemsSource")?.Value == "{Binding Sections}")
            .Should().BeTrue();
    }

    [Fact]
    public void Unsigned_preview_hides_unavailable_mutation_buttons()
    {
        var code = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "src", "Css.App", "MigrationPlanWindow.xaml.cs"));
        var update = SourceMethodExtractor.Extract(
            code,
            "private void UpdateActionAvailability()");

        update.Should().Contain("var canPrepareExecution = _productionReadiness.CanPrepareExecution;")
            .And.Contain("CreateRollbackManifestButton.Visibility")
            .And.Contain("RequestMigrationButton.Visibility")
            .And.Contain("canPrepareExecution ? Visibility.Visible : Visibility.Collapsed");
        update.Should().NotContain("ExecuteAsync")
            .And.NotContain("Process.Start");
    }

    private static XElement NamedElement(XDocument document, string name) =>
        document.Descendants().Single(element =>
            element.Attributes().Any(attribute =>
                attribute.Name.LocalName == "Name" && attribute.Value == name));

    private static string? AutomationId(XElement element) =>
        element.Attributes().SingleOrDefault(attribute =>
            attribute.Name.LocalName.EndsWith(".AutomationId", StringComparison.Ordinal))?.Value;

    private static string? AttributeValue(XElement element, string name) =>
        element.Attribute(name)?.Value;

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}

internal static class MigrationPlanTestData
{
    public static MigrationPlanPreviewViewModel CreateRecommendedPreview() => new()
    {
        Title = "Notes 迁移方案",
        Summary = "这是迁移预览。",
        SafetyBanner = "只预览。",
        DestinationLine = @"建议目标位置：D:\Software\Notes\Install",
        ScoreLine = "迁移评分：需关闭后验证。",
        RollbackManifestLine = @"回滚清单草稿：C:\Private\migration.rollback.json。",
        SuggestedRollbackManifestPath = @"C:\Private\migration.rollback.json",
        DestinationSpaceLine = "目标盘空间：空间足够（可用 100 bytes / 需要 1 bytes）。",
        CanRunMigration = false,
        RequiresSnapshot = true,
        IsRecommended = true,
        IsAlreadyReasonable = false,
        BlockingReasons = ["需要快照。"],
        ReadinessChecklist = new MigrationPreflightChecklistViewModel
        {
            PrimaryActionText = "先完成恢复准备",
            NextActionText = "先创建快照。",
            Steps = [],
            CanRequestExecution = false,
            ExecutionGate = new MigrationExecutionGateResult
            {
                CanRequestExecution = false,
                PrimaryButtonText = "Prepare evidence",
                BlockingReasons = ["fixture"],
                RequiredBytes = 1,
                Operation = null
            }
        },
        Sections = [],
        PrimaryActionText = "先生成回滚证据",
        FinalReminder = "不会移动文件。"
    };
}
