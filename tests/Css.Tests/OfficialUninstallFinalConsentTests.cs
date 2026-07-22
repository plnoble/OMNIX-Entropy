using Css.Core.Operations;
using Css.Core.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class OfficialUninstallFinalConsentTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 11, 13, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Presenter_explains_the_three_final_truths_without_paths_or_execution_authority()
    {
        var operation = ReadyOperation();

        var view = OfficialUninstallFinalConsentPresenter.Create(operation);

        view.Title.Should().Contain("\u6700\u540e\u4e00\u6b65");
        view.SoftwareName.Should().Be("Example App");
        view.ConfirmationText.Should().Be(operation.ConfirmationText);
        view.ImpactLines.Should().HaveCount(3);
        view.VisibleText.Should().Contain("\u5b98\u65b9\u5378\u8f7d\u5668");
        view.VisibleText.Should().Contain("\u5173\u95ed");
        view.VisibleText.Should().Contain("\u4e0d\u80fd\u9760\u9694\u79bb\u533a\u6062\u590d");
        view.VisibleText.Should().Contain("\u5378\u8f7d\u540e\u590d\u67e5");
        view.VisibleText.Should().NotContain(@"D:\Software");
        view.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Pending_presenter_builds_plain_consent_without_an_operation_descriptor()
    {
        var view = OfficialUninstallFinalConsentPresenter.CreatePending(
            "Example App",
            "Run Example App official uninstaller?");

        view.SoftwareName.Should().Be("Example App");
        view.ConfirmationText.Should().Be("Run Example App official uninstaller?");
        view.ImpactLines.Should().HaveCount(3);
        view.VisibleText.Should().Contain("\u6258\u76d8");
        view.CanExecuteDirectly.Should().BeFalse();
    }

    [Theory]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, true, true, false)]
    public void Builder_refuses_when_any_required_acknowledgement_is_missing(
        bool commandConfirmed,
        bool appsClosedConfirmed,
        bool noAutomaticUndoAcknowledged,
        bool postScanConfirmed)
    {
        var view = OfficialUninstallFinalConsentPresenter.Create(ReadyOperation());

        var result = OfficialUninstallFinalConsentBuilder.Create(
            view,
            new OfficialUninstallFinalConsentSelection
            {
                OfficialCommandConfirmed = commandConfirmed,
                AppsClosedConfirmed = appsClosedConfirmed,
                NoAutomaticUndoAcknowledged = noAutomaticUndoAcknowledged,
                PostUninstallRescanConfirmed = postScanConfirmed
            },
            Now);

        result.CanSubmit.Should().BeFalse();
        result.Consent.Should().BeNull();
        result.MissingRequirements.Should().NotBeEmpty();
    }

    [Fact]
    public void Builder_creates_exact_fresh_consent_only_after_all_acknowledgements()
    {
        var view = OfficialUninstallFinalConsentPresenter.Create(ReadyOperation());

        var result = OfficialUninstallFinalConsentBuilder.Create(
            view,
            new OfficialUninstallFinalConsentSelection
            {
                OfficialCommandConfirmed = true,
                AppsClosedConfirmed = true,
                NoAutomaticUndoAcknowledged = true,
                PostUninstallRescanConfirmed = true
            },
            Now);

        result.CanSubmit.Should().BeTrue();
        result.MissingRequirements.Should().BeEmpty();
        result.Consent.Should().NotBeNull();
        result.Consent!.ConfirmationText.Should().Be(view.ConfirmationText);
        result.Consent.ConfirmedAtUtc.Should().Be(Now);
        result.Consent.OfficialCommandConfirmed.Should().BeTrue();
        result.Consent.AppsClosedConfirmed.Should().BeTrue();
        result.Consent.NoAutomaticUndoAcknowledged.Should().BeTrue();
        result.Consent.PostUninstallRescanConfirmed.Should().BeTrue();
        result.Consent.ExecutionRequested.Should().BeTrue();
    }

    [Fact]
    public void Shared_consent_and_response_display_contracts_live_in_core()
    {
        var shared = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Uninstall", "OfficialUninstallFinalConsent.cs"));
        var boundary = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Uninstall", "OfficialUninstallElevatedBoundary.cs"));

        shared.Should().Contain("OfficialUninstallFinalUserConsent");
        shared.Should().Contain("OfficialUninstallElevatedResponseState");
        shared.Should().Contain("OfficialUninstallElevatedResponseViewModel");
        shared.Should().Contain("CanExecuteDirectly => false");
        boundary.Should().NotContain("public sealed record OfficialUninstallFinalUserConsent");
        boundary.Should().NotContain("public enum OfficialUninstallElevatedResponseState");
        boundary.Should().NotContain("public sealed class OfficialUninstallElevatedResponseViewModel");
    }

    [Fact]
    public void Consent_source_is_pure_and_non_executable()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Uninstall", "OfficialUninstallFinalConsent.cs"));

        source.Should().NotContain("SafetyOperationPipeline");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("File.Move");
        source.Should().NotContain("ExecuteAsync");
    }

    private static OperationDescriptor ReadyOperation() =>
        new()
        {
            Kind = "uninstall.official.run",
            Title = "Example App official uninstaller",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "snapshot-1",
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "verified",
            ConfirmationText = "Run the official uninstaller for Example App?",
            AffectedPaths = [@"D:\Software\Example"],
            Arguments = new Dictionary<string, object?>
            {
                ["softwareName"] = "Example App"
            }
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

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }
}
