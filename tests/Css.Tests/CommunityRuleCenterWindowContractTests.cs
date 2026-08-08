using FluentAssertions;

namespace Css.Tests;

public sealed class CommunityRuleCenterWindowContractTests
{
    [Fact]
    public void Application_page_has_one_secondary_rule_center_entry()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");

        xaml.Should().Contain("AutomationProperties.AutomationId=\"CommunityRulesButton\"")
            .And.Contain("Click=\"OpenCommunityRules_Click\"");
        xaml.IndexOf("CommunityRulesButton", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("ScanSoftwareButton", StringComparison.Ordinal));
        code.Should().Contain("new CommunityRuleCenterWindow")
            .And.Contain("DefaultWinapp2RulePackRoot()")
            .And.Contain("DefaultWinapp2RulePreferencesPath()")
            .And.Contain("RefreshSoftwareInventoryAsync");
    }

    [Fact]
    public void Rule_center_exposes_status_preview_preferences_and_explicit_consent()
    {
        var xaml = Read("src", "Css.App", "CommunityRuleCenterWindow.xaml");

        xaml.Should().Contain("AutomationProperties.AutomationId=\"RuleCenterStatusHeadlineTextBlock\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterSourceTextBlock\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterLicenseTextBlock\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterSafetyTextBlock\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterPreviewListBox\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterIgnoredRulesListBox\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterChooseFileButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterActivateFileButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterDownloadButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterRollbackButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterCancelOperationButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterLicenseCheckBox\"")
            .And.Contain("AutomationProperties.AutomationId=\"RuleCenterActivationCheckBox\"")
            .And.Contain("Property=\"AutomationProperties.Name\" Value=\"{Binding VisibleText}\"")
            .And.Contain("不会直接清理");
        xaml.IndexOf("RuleCenterStatusHeadlineTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("RuleCenterRollbackButton", StringComparison.Ordinal));
    }

    [Fact]
    public void Rule_center_calls_guarded_data_apis_and_has_no_computer_maintenance_authority()
    {
        var source = Read("src", "Css.App", "CommunityRuleCenterWindow.xaml.cs");

        source.Should().Contain("Winapp2RuleActivationRequestBuilder.Build")
            .And.Contain("ActivateAsync")
            .And.Contain("DownloadAndActivateAsync")
            .And.Contain("RollbackAsync")
            .And.Contain("IgnoreAsync")
            .And.Contain("RestoreAsync")
            .And.Contain("OnClosing")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("Process.Start")
            .And.NotContain("Registry.")
            .And.NotContain("File.Delete")
            .And.NotContain("File.Move");
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
        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
