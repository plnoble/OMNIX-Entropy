using Css.Core.Operations;
using Css.Core.Software;
using Css.Core.Uninstall;
using Css.Elevated.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class OfficialUninstallPostScanPresentationTests
{
    [Fact]
    public void Presenter_explains_scan_failure_without_claiming_clean_uninstall_or_leaking_error_paths()
    {
        var result = new OfficialUninstallPostScanResult
        {
            Success = false,
            Summary = @"Access failed at C:\Users\Me\Secret"
        };

        var view = OfficialUninstallPostScanPresenter.Create("Example App", result);

        view.State.Should().Be(OfficialUninstallPostScanState.ScanFailed);
        view.Title.Should().Contain("\u8fd8\u4e0d\u80fd\u786e\u8ba4");
        view.StatusLabel.Should().Be("\u9700\u8981\u91cd\u8bd5");
        view.AgentAdvice.Should().Contain("\u91cd\u65b0\u626b\u63cf");
        view.VisibleText.Should().NotContain(@"C:\");
        view.VisibleText.Should().NotContain("Secret");
        view.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Presenter_blocks_residue_handling_while_software_is_still_present()
    {
        var result = new OfficialUninstallPostScanResult
        {
            Success = true,
            SoftwareStillPresent = true,
            ResidueCandidateCount = 9,
            Summary = "still present"
        };

        var view = OfficialUninstallPostScanPresenter.Create("Example App", result);

        view.State.Should().Be(OfficialUninstallPostScanState.SoftwareStillPresent);
        view.StatusLabel.Should().Be("\u5148\u522b\u6e05\u7406");
        view.Facts.Should().Contain(line => line.Contains("\u4ecd\u80fd\u627e\u5230"));
        view.CanReviewResidue.Should().BeFalse();
        view.PrimaryActionText.Should().Be("\u91cd\u65b0\u626b\u63cf");
        view.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Presenter_reports_a_clean_read_only_scan_without_overstating_rollback()
    {
        var result = new OfficialUninstallPostScanResult
        {
            Success = true,
            SoftwareStillPresent = false,
            ResidueCandidateCount = 0,
            UnverifiedBackgroundHintCount = 0,
            Summary = "complete"
        };

        var view = OfficialUninstallPostScanPresenter.Create("Example App", result);

        view.State.Should().Be(OfficialUninstallPostScanState.NoVisibleResidue);
        view.StatusLabel.Should().Be("\u6682\u672a\u53d1\u73b0\u6b8b\u7559");
        view.Facts.Should().Contain(line => line.Contains("\u53ea\u8bfb\u590d\u67e5"));
        view.AgentAdvice.Should().Contain("\u89c2\u5bdf");
        view.CanReviewResidue.Should().BeFalse();
        view.CanExecuteDirectly.Should().BeFalse();
        view.VisibleText.Should().NotContain("\u4e00\u952e\u6062\u590d");
    }

    [Fact]
    public void Presenter_summarizes_residue_and_background_rescan_by_count_only()
    {
        var report = new UninstallResidueScanReport
        {
            SoftwareName = "Example App",
            Summary = @"Found C:\Users\Me\Example\Cache and ExampleService",
            OfficialUninstallAppearsComplete = true,
            WouldDeleteAutomatically = false,
            Groups =
            [
                new UninstallResidueGroup
                {
                    Title = "low",
                    Risk = RiskLevel.Low,
                    CanMoveToQuarantine = true,
                    Candidates =
                    [
                        new UninstallResidueCandidate
                        {
                            Kind = UninstallResidueKind.CacheDirectory,
                            Path = @"C:\Users\Me\Example\Cache"
                        }
                    ]
                }
            ]
        };
        var result = new OfficialUninstallPostScanResult
        {
            Success = true,
            ResidueCandidateCount = 1,
            UnverifiedBackgroundHintCount = 3,
            RequiresBackgroundRescan = true,
            ResidueReport = report,
            Summary = report.Summary
        };

        var view = OfficialUninstallPostScanPresenter.Create("Example App", result);

        view.State.Should().Be(OfficialUninstallPostScanState.ReviewNeeded);
        view.StatusLabel.Should().Be("\u9700\u8981\u68c0\u67e5");
        view.Facts.Should().Contain(line => line.Contains("1 \u9879\u76ee\u5f55\u6b8b\u7559"));
        view.Facts.Should().Contain(line => line.Contains("3 \u9879\u540e\u53f0\u8bb0\u5f55"));
        view.AgentAdvice.Should().Contain("\u4f4e\u98ce\u9669");
        view.CanReviewResidue.Should().BeTrue();
        view.PrimaryActionText.Should().Be("\u67e5\u770b\u6b8b\u7559\u6e05\u5355");
        view.TechnicalDetailsAvailable.Should().BeTrue();
        view.VisibleText.Should().NotContain(@"C:\");
        view.VisibleText.Should().NotContain("ExampleService");
        view.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Presenter_explains_verified_background_residue_without_exposing_or_disabling_it()
    {
        var report = new UninstallResidueScanReport
        {
            SoftwareName = "Example App",
            Summary = "technical",
            OfficialUninstallAppearsComplete = true,
            WouldDeleteAutomatically = false,
            Groups =
            [
                new UninstallResidueGroup
                {
                    Title = "high",
                    Risk = RiskLevel.High,
                    CanMoveToQuarantine = false,
                    Candidates =
                    [
                        new UninstallResidueCandidate
                        {
                            Kind = UninstallResidueKind.Service,
                            Identifier = "SecretExampleService"
                        }
                    ]
                }
            ]
        };
        var result = new OfficialUninstallPostScanResult
        {
            Success = true,
            ResidueCandidateCount = 1,
            VerifiedBackgroundResidueCount = 1,
            ResidueReport = report,
            Summary = "technical"
        };

        var view = OfficialUninstallPostScanPresenter.Create("Example App", result);

        view.Facts.Should().Contain(line => line.Contains("1 \u9879\u4ecd\u5b58\u5728\u7684\u540e\u53f0\u8bb0\u5f55"));
        view.AgentAdvice.Should().Contain("\u4e0d\u4f1a\u76f4\u63a5\u5173\u95ed");
        view.VisibleText.Should().NotContain("SecretExampleService");
        view.CanReviewResidue.Should().BeTrue();
        view.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Presenter_source_is_pure_and_non_executable()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Uninstall", "OfficialUninstallPostScanPresentation.cs"));

        source.Should().NotContain("OperationDescriptor");
        source.Should().NotContain("OperationPipeline");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("QuarantineOperation");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("File.Move");
    }

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

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }
}
