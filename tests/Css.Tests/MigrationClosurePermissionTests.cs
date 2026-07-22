using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationClosurePermissionTests
{
    [Fact]
    public void Protected_profiles_keep_their_safety_conclusion_and_cannot_open_a_stale_closure_plan()
    {
        var windowsRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        windowsRoot.Should().NotBeNullOrWhiteSpace();
        var profiles = new[]
        {
            new SoftwareProfile
            {
                Name = "Windows Component",
                Category = SoftwareCategory.SystemTool,
                InstallPath = Path.Combine(windowsRoot, "System32", "Component")
            },
            new SoftwareProfile
            {
                Name = "Unknown Managed Component",
                Category = SoftwareCategory.Unknown,
                InstallPath = Path.Combine(windowsRoot, "System32", "UnknownComponent")
            }
        };

        var states = profiles
            .Select(profile =>
            {
                var drawer = AppPresentationBuilder.CreateDrawer(profile);
                return (Drawer: drawer, State: MigrationClosureDrawerStatePresenter.Create(
                    profile,
                    drawer,
                    Closure(profile.Name)));
            })
            .ToList();

        states.Should().OnlyContain(item => !item.State.CanOpenPlan);
        states[0].State.AdviceText.Should().StartWith(states[0].Drawer.AgentAdvice.Text)
            .And.Contain("迁移记录提醒")
            .And.Contain("又开始写入原位置");
        states[1].State.AdviceText.Should().StartWith(states[1].Drawer.AgentAdvice.Text)
            .And.Contain("迁移记录提醒")
            .And.Contain("又开始写入原位置");
        states[0].State.ButtonReason.Should().Contain("系统");
        states[1].State.ButtonReason.Should().Contain("归属");
        string.Join("\n", states.Select(item => item.State.AdviceText))
            .Should().NotContain(windowsRoot);
    }

    [Fact]
    public void Ordinary_app_on_d_can_review_a_closure_warning_without_direct_execution()
    {
        var profile = new SoftwareProfile
        {
            Name = "Migrated App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\Migrated App\Install"
        };
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var baseMigration = drawer.AvailableActions.Single(action => action.Kind == AppActionKind.Migration);

        var state = MigrationClosureDrawerStatePresenter.Create(profile, drawer, Closure(profile.Name));

        baseMigration.IsEnabled.Should().BeFalse("the main program is already on D");
        state.CanOpenPlan.Should().BeTrue("a stale closure needs a fresh read-only plan review");
        state.CanExecuteDirectly.Should().BeFalse();
        state.ButtonText.Should().Be("复查迁移");
        state.ButtonReason.Should().Contain("重新扫描")
            .And.Contain("旧记录不会直接执行");
        state.AdviceText.Should().Contain("又开始写入原位置")
            .And.NotContain(@"C:\")
            .And.NotContain(@"D:\");
    }

    [Fact]
    public void Main_window_uses_the_combined_closure_state_for_button_and_plan_entry()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var show = ExtractMethod(
            code,
            "private void ShowAppDrawer",
            "private static void ApplyActionState");
        var plan = ExtractMethod(
            code,
            "private async Task ShowMigrationPlanAsync",
            "private void PreviewCacheCleanup_Click");

        show.Should().Contain("MigrationClosureDrawerStatePresenter.Create(profile, drawer, migrationClosure)")
            .And.Contain("DrawerAdviceTextBlock.Text = migrationState.AdviceText;")
            .And.Contain("DrawerMigrateButton.IsEnabled = migrationState.CanOpenPlan;")
            .And.Contain("DrawerMigrateButton.ToolTip = migrationState.ButtonReason;")
            .And.NotContain("DrawerMigrateButton.IsEnabled = true;")
            .And.NotContain("if (migrationClosure?.NeedsAttention == true)");

        var stateIndex = plan.IndexOf("MigrationClosureDrawerStatePresenter.Create", StringComparison.Ordinal);
        var guardIndex = plan.IndexOf("if (!migrationState.CanOpenPlan)", StringComparison.Ordinal);
        var windowIndex = plan.IndexOf("new MigrationPlanWindow", StringComparison.Ordinal);
        stateIndex.Should().BeGreaterThanOrEqualTo(0);
        guardIndex.Should().BeGreaterThan(stateIndex);
        windowIndex.Should().BeGreaterThan(guardIndex);
    }

    private static MigrationClosureSummaryViewModel Closure(string softwareName) =>
        new()
        {
            SoftwareName = softwareName,
            DisplayName = softwareName,
            TargetAppNameCandidate = softwareName,
            State = MigrationClosureFindingKind.OriginalWriteReturned,
            Headline = "迁移后又开始写入原位置",
            Detail = "旧迁移未形成闭环，需要重新检查。",
            ObservedPathCount = 1,
            MonitoringStartedAtUtc = DateTimeOffset.UtcNow
        };

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path))
                return path;
            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            "Could not locate repository file.",
            Path.Combine(segments));
    }

    private static string ExtractMethod(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }
}
