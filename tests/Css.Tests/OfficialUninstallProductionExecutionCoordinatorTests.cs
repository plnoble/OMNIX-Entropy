using Css.App;
using Css.Core.Operations;
using Css.Core.Uninstall;
using Css.Ipc.Uninstall;
using Css.Win32.Security;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallProductionExecutionCoordinatorTests
{
    [Fact]
    public async Task Unsigned_package_stops_before_runner_creation_or_uac()
    {
        var runnerCreated = false;
        var coordinator = new OfficialUninstallProductionExecutionCoordinator(
            UnsignedTrust,
            _ =>
            {
                runnerCreated = true;
                return new FakeRunner(CompletedProduction(ReadyDraft()));
            });

        var outcome = await coordinator.ExecuteAsync(ReadyDraft());

        runnerCreated.Should().BeFalse();
        outcome.ProductionAttempted.Should().BeFalse();
        outcome.Lifecycle.Should().BeNull();
        outcome.PostScan.Should().BeNull();
        outcome.Summary.Title.Should().Be("当前是开发验证版本");
        outcome.Summary.SafetyText.Should().Contain("不会获得真实卸载");
    }

    [Fact]
    public async Task Trusted_typed_response_becomes_a_post_scan_result()
    {
        var request = ReadyDraft();
        var runner = new FakeRunner(CompletedProduction(request));
        var coordinator = new OfficialUninstallProductionExecutionCoordinator(
            TrustedTrust,
            _ => runner);

        var outcome = await coordinator.ExecuteAsync(request);

        runner.RunCount.Should().Be(1);
        outcome.ProductionAttempted.Should().BeTrue();
        outcome.CompletedProduction.Should().BeTrue();
        outcome.Response!.State.Should().Be(OfficialUninstallElevatedResponseState.PostScanReady);
        outcome.PostScan.Should().NotBeNull();
        outcome.PostScan!.State.Should().Be(OfficialUninstallPostScanState.ReviewNeeded);
        outcome.PostScan.CanReviewResidue.Should().BeTrue();
        var payload = outcome.Lifecycle!.Response!.Result.Payload
            .Should().BeOfType<OfficialUninstallHandlerPayload>().Subject;
        payload.PostScan.ResidueReport.Should().BeNull(
            "the cross-process result must remain path-free");
        outcome.Summary.Title.Should().Be("卸载完成，发现待检查内容");
    }

    [Fact]
    public async Task Trusted_lifecycle_failure_is_presented_without_a_post_scan_claim()
    {
        var coordinator = new OfficialUninstallProductionExecutionCoordinator(
            TrustedTrust,
            _ => new FakeRunner(new OfficialUninstallWorkerLifecycleResult
            {
                Status = OfficialUninstallWorkerLifecycleStatus.UserCanceledElevation,
                ChildExited = false
            }));

        var outcome = await coordinator.ExecuteAsync(ReadyDraft());

        outcome.ProductionAttempted.Should().BeTrue();
        outcome.CompletedProduction.Should().BeFalse();
        outcome.PostScan.Should().BeNull();
        outcome.Summary.Title.Should().Be("你取消了 Windows 确认");
    }

    [Fact]
    public void Wpf_window_depends_on_coordinator_and_does_not_construct_production_authority()
    {
        var window = Read("src", "Css.App", "UninstallPlanWindow.xaml.cs");
        var main = Read("src", "Css.App", "MainWindow.xaml.cs");
        var coordinator = Read(
            "src", "Css.App", "OfficialUninstallProductionExecutionCoordinator.cs");

        window.Should().Contain("IOfficialUninstallProductionExecutionCoordinator");
        window.Should().Contain("ExecuteAsync(_preparedRequest)");
        window.Should().NotContain("WindowsOfficialUninstallProductionWorkerLauncher");
        window.Should().NotContain("official-uninstall-production-worker");
        window.Should().NotContain("RunProductionOnceAsync");
        main.Should().Contain("CreateForCurrentPackage");
        main.Should().NotContain("official-uninstall-production-worker");
        coordinator.Should().Contain("WindowsOfficialUninstallProductionWorkerLauncher.Create");
        coordinator.Should().Contain("RunProductionOnceAsync");
    }

    [Fact]
    public void Completed_post_scan_reuses_captured_profile_for_local_residue_review()
    {
        var window = Read("src", "Css.App", "UninstallPlanWindow.xaml.cs");
        var main = Read("src", "Css.App", "MainWindow.xaml.cs");
        var wire = Read(
            "src", "Css.Ipc", "Uninstall", "OfficialUninstallSerializedPipeProtocol.cs");

        window.Should().Contain("ProductionResidueReviewRecommended");
        window.Should().Contain("outcome.PostScan?.CanReviewResidue == true");
        main.Should().Contain("ShowUninstallPlanAsync(selected.Profile)");
        main.Should().Contain("ReviewUninstallResidueAsync(profile, refreshedProfiles)");
        main.Should().Contain("knownAfterProfiles ?? await ScanSoftwareProfilesAsync()");
        main.Should().Contain("UninstallResidueScanBuilder.Build");
        main.Should().Contain("SafetyOperationPipeline(handler.ExecuteAsync)");
        main.Should().Contain("QuarantineOperationPolicy.ConfirmForExecution");
        main.Should().Contain("await LoadTimelineAsync()");
        wire.Should().NotContain("ResidueReport");

        main.Should().MatchRegex("RefreshAppCatalog\\(\\);\\s+ShowResidueReviewInline\\(review\\);");
        main.IndexOf("RefreshAppCatalog();", StringComparison.Ordinal).Should().BeGreaterThan(-1,
            "the local review must remain visible after the removed app disappears from the grid");
    }

    [Fact]
    public void Main_window_refreshes_inventory_after_every_production_attempt_but_gates_residue_review()
    {
        var main = Read("src", "Css.App", "MainWindow.xaml.cs");
        var method = SourceMethodExtractor.Extract(
            main,
            "private async Task ShowUninstallPlanAsync(SoftwareProfile profile)");

        method.Should().Contain("if (window.ProductionExecutionAttempted)");
        method.Should().Contain(
            "var refreshedProfiles = await TryScanSoftwareProfilesAfterProductionAttemptAsync();");
        method.Should().Contain("if (refreshedProfiles is null)");
        method.Should().Contain(
            "window.ProductionCompleted && window.ProductionResidueReviewRecommended");
        method.Should().Contain("SetSoftwareProfiles(refreshedProfiles);");
        method.IndexOf("if (window.ProductionExecutionAttempted)", StringComparison.Ordinal)
            .Should().BeLessThan(
                method.IndexOf(
                    "var refreshedProfiles = await TryScanSoftwareProfilesAfterProductionAttemptAsync();",
                    StringComparison.Ordinal));
        method.IndexOf("if (refreshedProfiles is null)", StringComparison.Ordinal)
            .Should().BeLessThan(method.IndexOf(
                "window.ProductionCompleted && window.ProductionResidueReviewRecommended",
                StringComparison.Ordinal));
        method.IndexOf(
                "window.ProductionCompleted && window.ProductionResidueReviewRecommended",
                StringComparison.Ordinal)
            .Should().BeLessThan(
                method.IndexOf("await ReviewUninstallResidueAsync(profile, refreshedProfiles);", StringComparison.Ordinal));
    }

    private static OfficialUninstallWorkerLifecycleResult CompletedProduction(
        OfficialUninstallElevatedRequestDraft request) =>
        new()
        {
            Status = OfficialUninstallWorkerLifecycleStatus.CompletedProduction,
            ChildExited = true,
            TransportStatus = OfficialUninstallTransportStatus.Completed,
            Response = new OfficialUninstallElevatedResponseEnvelope
            {
                RequestId = request.RequestId!,
                Result = OperationResult.Ok(payload: new OfficialUninstallHandlerPayload
                {
                    UninstallerStarted = true,
                    UninstallerCompleted = true,
                    ExitCode = 0,
                    RequiresPostScanRetry = false,
                    PostScan = new OfficialUninstallPostScanResult
                    {
                        Success = true,
                        SoftwareStillPresent = false,
                        ResidueCandidateCount = 2,
                        PathResidueCandidateCount = 2,
                        VerifiedBackgroundResidueCount = 0,
                        UnverifiedBackgroundHintCount = 0,
                        RequiresBackgroundRescan = false,
                        Summary = @"Private path must not surface: C:\Users\Example"
                    }
                })
            }
        };

    private static OfficialUninstallWorkerTrustAssessment UnsignedTrust() =>
        Trust(
            OfficialUninstallWorkerTrustStatus.AppNotSigned,
            AuthenticodeSignatureStatus.NotSigned);

    private static OfficialUninstallWorkerTrustAssessment TrustedTrust() =>
        Trust(
            OfficialUninstallWorkerTrustStatus.TrustedForProduction,
            AuthenticodeSignatureStatus.Trusted);

    private static OfficialUninstallWorkerTrustAssessment Trust(
        OfficialUninstallWorkerTrustStatus status,
        AuthenticodeSignatureStatus signatureStatus) =>
        new()
        {
            Status = status,
            AppEvidence = new AuthenticodeSignatureEvidence
            {
                Status = signatureStatus,
                SignerThumbprint = signatureStatus == AuthenticodeSignatureStatus.Trusted
                    ? new string('A', 40)
                    : null,
                FileSha256 = new string('1', 64)
            },
            WorkerEvidence = new AuthenticodeSignatureEvidence
            {
                Status = signatureStatus,
                SignerThumbprint = signatureStatus == AuthenticodeSignatureStatus.Trusted
                    ? new string('A', 40)
                    : null,
                FileSha256 = new string('2', 64)
            },
            WorkerExecutablePath = @"D:\OMNIX\Css.Elevated.exe"
        };

    private static OfficialUninstallElevatedRequestDraft ReadyDraft()
    {
        var operation = new OperationDescriptor
        {
            Kind = "uninstall.official.run",
            Title = "Coordinator Fixture",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "coordinator-snapshot",
            RollbackRequired = true,
            ConfirmationAccepted = true,
            EvidenceSummary = "verified coordinator fixture",
            ConfirmationText = "Run official uninstaller?",
            AffectedPaths = [@"D:\Software\CoordinatorFixture"],
            Arguments = new Dictionary<string, object?>
            {
                ["softwareName"] = "Coordinator Fixture",
                ["executablePath"] = @"D:\Software\CoordinatorFixture\Uninstall.exe",
                ["arguments"] = "/remove",
                ["snapshotManifestPath"] = @"D:\Evidence\coordinator.json",
                ["snapshotSha256"] = new string('A', 64),
                ["snapshotCanRestoreApplication"] = false,
                ["recoveryMethod"] = "ReinstallSource",
                ["recoveryReference"] = @"D:\Installers\CoordinatorSetup.exe"
            }
        };
        return new OfficialUninstallElevatedRequestDraft
        {
            Status = OfficialUninstallElevatedRequestStatus.Ready,
            MissingRequirements = [],
            PreparedAtUtc = DateTimeOffset.UtcNow,
            RequestId = $"coordinator-{Guid.NewGuid():N}",
            Operation = operation,
            DescriptorSha256 = OfficialUninstallElevatedRequestComposer
                .ComputeDescriptorSha256(operation)
        };
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

    private sealed class FakeRunner(OfficialUninstallWorkerLifecycleResult result)
        : IOfficialUninstallProductionLifecycleRunner
    {
        public int RunCount { get; private set; }

        public Task<OfficialUninstallWorkerLifecycleResult> RunAsync(
            OfficialUninstallElevatedRequestDraft request,
            CancellationToken cancellationToken = default)
        {
            RunCount++;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(result);
        }
    }
}
