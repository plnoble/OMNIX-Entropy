using FluentAssertions;

namespace Css.Tests;

public sealed class BeginnerVisibleFailureBoundaryTests
{
    [Fact]
    public void MainWindow_never_copies_raw_exception_details_to_beginner_visible_text()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");

        code.Should().NotContain("ex.Message")
            .And.NotContain("catch (Exception ex)");
    }

    [Fact]
    public void Six_failure_paths_keep_truthful_path_free_conclusions_and_next_steps()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");

        code.Should().Contain("失败；没有修改系统。可以稍后重试，或从 Windows 设置中手动打开。")
            .And.Contain("快照没有完成；没有运行安装包，也没有修改系统。请稍后重新捕获。")
            .And.Contain("永久整理没有确认完成；请重新加载后悔药中心核对当前状态，再决定是否重试。")
            .And.Contain("还原没有确认完成；请重新加载后悔药中心核对当前状态，再决定是否重试。")
            .And.Contain("残留复查没有完成；不能据此判断没有残留。")
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
