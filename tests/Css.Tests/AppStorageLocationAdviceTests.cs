using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AppStorageLocationAdviceTests
{
    [Fact]
    public void D_installed_but_C_writing_separates_main_program_from_data_location()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Category = SoftwareCategory.Ai,
            InstallPath = @"D:\Software\Marvis\Install",
            CachePaths = [@"C:\Users\Fixture\AppData\Local\Marvis\Cache"],
            CDriveWritePaths =
            [
                @"C:\Users\Fixture\AppData\Local\Marvis",
                @"c:\users\fixture\appdata\local\marvis",
                @"C:\Users\Fixture\AppData\Roaming\Marvis"
            ]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var migration = drawer.AvailableActions.Single(action =>
            action.Kind == AppActionKind.Migration);

        drawer.InstallLocationSummary.Should().Contain("主程序在 D 盘");
        drawer.InstallLocationSummary.Should().Contain("2 个 C 盘");
        drawer.InstallLocationSummary.Should().Contain("不等于 C 盘不会增长");
        drawer.AgentAdvice.Text.Should().Contain("不要重复迁移主程序");
        drawer.AgentAdvice.Text.Should().Contain("只清理一次");
        drawer.AgentAdvice.Action.Should().Be(RecommendationAction.RepairInstallLocation);
        drawer.MigrationSummary.Should().Contain("主程序不需要迁移");
        drawer.MigrationSummary.Should().Contain("C 盘数据");
        drawer.MigrationPreviewLines.Should().Contain(line =>
            line.Contains("主程序不需要迁移") && line.Contains("数据"));
        migration.IsEnabled.Should().BeFalse();
        migration.Reason.Should().Contain("没有可靠的数据位置重定向方案");
        Visible(drawer).Should().NotContain(@"C:\Users\Fixture")
            .And.NotContain(@"D:\Software\Marvis");
    }

    [Fact]
    public void D_installed_without_C_writes_is_a_bounded_reasonable_location()
    {
        var drawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Marvis",
            Category = SoftwareCategory.Ai,
            InstallPath = @"D:\Software\Marvis\Install"
        });

        drawer.InstallLocationSummary.Should().Contain("主程序在 D 盘");
        drawer.InstallLocationSummary.Should().Contain("暂未发现已归属的 C 盘写入线索");
        drawer.MigrationSummary.Should().Contain("不需要迁移主程序");
        drawer.AgentAdvice.Action.Should().Be(RecommendationAction.Keep);
    }

    [Fact]
    public void C_installed_app_explains_main_program_and_additional_C_write_evidence()
    {
        var drawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Example",
            Category = SoftwareCategory.Normal,
            InstallPath = @"C:\Program Files\Example",
            CDriveWritePaths = [@"C:\Users\Fixture\AppData\Local\Example"]
        });

        drawer.InstallLocationSummary.Should().Contain("主程序在 C 盘");
        drawer.InstallLocationSummary.Should().Contain("1 个 C 盘数据或缓存写入线索");
        drawer.InstallLocationSummary.Should().Contain("分别判断");
        drawer.AgentAdvice.Action.Should().Be(RecommendationAction.Migrate);
    }

    [Fact]
    public void Unknown_main_location_with_C_writes_does_not_guess_migration()
    {
        var drawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Unknown App",
            CDriveWritePaths = [@"C:\Users\Fixture\AppData\Local\Unknown"]
        });

        drawer.InstallLocationSummary.Should().Contain("主程序位置未知");
        drawer.InstallLocationSummary.Should().Contain("1 个 C 盘");
        drawer.InstallLocationSummary.Should().Contain("先确认来源");
        drawer.AgentAdvice.Text.Should().Contain("先确认主程序和数据分别属于哪里");
    }

    [Fact]
    public void Exact_location_answer_reuses_path_free_summary_and_remains_details_only()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Category = SoftwareCategory.Ai,
            InstallPath = @"D:\Software\Marvis\Install",
            CDriveWritePaths = [@"C:\Users\Fixture\AppData\Local\Marvis"]
        };

        var reply = AgentConversationPresenter.Answer("Marvis 装在哪里", null, [profile]);

        reply.Answer.Should().Contain("主程序在 D 盘");
        reply.Answer.Should().Contain("仍发现 1 个 C 盘");
        reply.TargetAppHandoff.Should().Be(AgentApplicationHandoff.Details);
        reply.NavigationLabel.Should().Be("打开这个应用");
        reply.CanExecuteDirectly.Should().BeFalse();
        string.Join("\n", new[] { reply.Answer }.Concat(reply.EvidenceLines))
            .Should().NotContain(@"C:\Users\Fixture")
            .And.NotContain(@"D:\Software\Marvis");
    }

    private static string Visible(AppDrawerViewModel drawer) =>
        string.Join(
            "\n",
            new[]
            {
                drawer.InstallLocationSummary,
                drawer.SizeSummary,
                drawer.ResidencySummary,
                drawer.AgentAdvice.Text,
                drawer.AgentAdvice.Reason,
                drawer.MigrationSummary
            }
            .Concat(drawer.MigrationPreviewLines)
            .Concat(drawer.AvailableActions.Select(action => action.Reason)));
}
