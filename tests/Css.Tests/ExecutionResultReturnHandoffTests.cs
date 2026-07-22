using Css.App;
using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Operations;
using FluentAssertions;

namespace Css.Tests;

public sealed class ExecutionResultReturnHandoffTests
{
    [Fact]
    public void Result_buttons_explain_that_current_application_state_will_be_rechecked()
    {
        var migration = MigrationExecutionResultPresenter.Create(OperationResult.Fail("test"));
        var uninstall = OfficialUninstallWorkerResultPresenter.CreateUnknownAttempt();

        migration.CloseButtonText.Should().Contain("\u8fd4\u56de")
            .And.Contain("\u91cd\u65b0\u68c0\u67e5");
        uninstall.ReturnToApplicationButtonText.Should().Contain("\u8fd4\u56de")
            .And.Contain("\u91cd\u65b0\u68c0\u67e5");
        uninstall.CloseButtonText.Should().Be("\u6211\u77e5\u9053\u4e86");
    }

    [Fact]
    public void Migration_attempt_returns_to_main_window_after_result_acknowledgement()
    {
        var code = Read("src", "Css.App", "MigrationPlanWindow.xaml.cs");
        var request = SourceMethodExtractor.Extract(
            code,
            "private async void RequestMigration_Click(object sender, RoutedEventArgs e)");

        request.Should().Contain("resultWindow.ShowDialog();\n            Close();");
        request.Should().Contain("ShowExecutionResult(OperationResult.Fail(\n                \"Migration execution outcome is unknown.\"));\n            Close();");
    }

    [Fact]
    public void Uninstall_attempt_returns_to_main_window_after_every_terminal_result()
    {
        var code = Read("src", "Css.App", "UninstallPlanWindow.xaml.cs");
        var request = SourceMethodExtractor.Extract(
            code,
            "private async void ContinueFinalConsent_Click(object sender, RoutedEventArgs e)");

        request.Should().Contain("unknownWindow.ShowDialog();")
            .And.Contain("returnsToApplication: true")
            .And.Contain("ShowRequestStatus(unknown.Conclusion, false);\n            Close();\n            return;");
        request.Should().Contain("ShowRequestStatus(\n            outcome.Summary.Conclusion,\n            outcome.CompletedProduction);\n        Close();");
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
