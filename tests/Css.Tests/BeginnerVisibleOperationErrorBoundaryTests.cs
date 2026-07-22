using FluentAssertions;

namespace Css.Tests;

public sealed class BeginnerVisibleOperationErrorBoundaryTests
{
    [Fact]
    public void MainWindow_never_copies_raw_operation_policy_or_validation_errors()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");

        code.Should().NotContain("result.Error")
            .And.NotContain("policy.Error")
            .And.NotContain("validation.Error");
    }

    [Fact]
    public void Pre_execution_refusals_state_no_change_and_offer_a_fresh_review()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");

        code.Should().Contain("永久整理方案未通过安全检查；没有永久删除任何隔离内容。请重新加载后再生成方案。")
            .And.Contain("安全策略没有批准这项残留处理；没有移动或删除任何残留。")
            .And.Contain("这张清理卡没有通过安全检查；没有移动任何文件。请重新扫描后再选择。");
    }

    [Fact]
    public void Post_attempt_failures_keep_completion_unknown_and_name_the_authoritative_review()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");

        code.Should().Contain("自启动处理没有确认完成；请重新扫描应用并到后悔药中心核对当前状态。")
            .And.Contain("永久整理没有确认完成；请重新加载后悔药中心核对当前状态。")
            .And.Contain("残留处理没有确认完成；请到后悔药中心核对记录，并重新扫描应用。")
            .And.Contain("自启动还原没有确认完成；请重新扫描应用并重新加载后悔药中心。")
            .And.Contain("清理没有确认完成；请到后悔药中心核对当前记录，再重新扫描。");
    }

    private static string Read(params string[] segments) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

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
