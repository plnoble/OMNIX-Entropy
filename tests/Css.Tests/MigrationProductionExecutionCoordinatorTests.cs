using Css.App;
using Css.Core.Migration;
using Css.Core.Operations;
using Css.Ipc.Migration;
using Css.Ipc.Uninstall;
using Css.Win32.Security;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationProductionExecutionCoordinatorTests
{
    [Fact]
    public async Task Unsigned_package_stops_before_runner_creation_or_uac()
    {
        var runnerCreated = false;
        var coordinator = new MigrationProductionExecutionCoordinator(
            UnsignedTrust,
            _ =>
            {
                runnerCreated = true;
                return new FakeRunner(Completed(ReadyDraft()));
            });

        var outcome = await coordinator.ExecuteAsync(ReadyDraft());

        runnerCreated.Should().BeFalse();
        outcome.ProductionAttempted.Should().BeFalse();
        outcome.CompletedProduction.Should().BeFalse();
        outcome.Lifecycle.Should().BeNull();
        outcome.Summary.Title.Should().Be("迁移没有开始");
        outcome.Summary.SafetyText.Should().Contain("不会启动提权");
    }

    [Fact]
    public async Task Trusted_correlated_typed_response_is_accepted()
    {
        var request = ReadyDraft();
        var runner = new FakeRunner(Completed(request));
        var coordinator = new MigrationProductionExecutionCoordinator(
            TrustedTrust,
            _ => runner);

        var outcome = await coordinator.ExecuteAsync(request);

        runner.RunCount.Should().Be(1);
        outcome.ProductionAttempted.Should().BeTrue();
        outcome.CompletedProduction.Should().BeTrue();
        outcome.Summary.Title.Should().Be("迁移完成，开始观察 C 盘");
        outcome.Summary.VisibleText.Should().NotContain(@"C:\Users\");
        outcome.Summary.VisibleText.Should().NotContain(@"D:\Software\");
    }

    [Fact]
    public async Task Mismatched_response_is_not_accepted_as_completed()
    {
        var request = ReadyDraft();
        var mismatched = Completed(request, requestId: "migration-other-request");
        var coordinator = new MigrationProductionExecutionCoordinator(
            TrustedTrust,
            _ => new FakeRunner(mismatched));

        var outcome = await coordinator.ExecuteAsync(request);

        outcome.ProductionAttempted.Should().BeTrue();
        outcome.CompletedProduction.Should().BeFalse();
        outcome.Summary.Title.Should().Be("无法确认迁移结果");
        outcome.Summary.SafetyText.Should().Contain("不会继续");
    }

    [Fact]
    public async Task Typed_refusal_is_presented_but_not_marked_as_completed()
    {
        var request = ReadyDraft();
        var refused = Completed(request, MigrationExecutionStatus.Refused);
        var coordinator = new MigrationProductionExecutionCoordinator(
            TrustedTrust,
            _ => new FakeRunner(refused));

        var outcome = await coordinator.ExecuteAsync(request);

        outcome.ProductionAttempted.Should().BeTrue();
        outcome.CompletedProduction.Should().BeFalse();
        outcome.Summary.Title.Should().Be("迁移没有开始");
        outcome.Summary.StatusLabel.Should().Be("没有改动");
    }

    [Fact]
    public async Task User_canceled_elevation_is_presented_as_no_change()
    {
        var coordinator = new MigrationProductionExecutionCoordinator(
            TrustedTrust,
            _ => new FakeRunner(new MigrationWorkerLifecycleResult
            {
                Status = MigrationWorkerLifecycleStatus.UserCanceledElevation,
                ChildExited = false
            }));

        var outcome = await coordinator.ExecuteAsync(ReadyDraft());

        outcome.ProductionAttempted.Should().BeTrue();
        outcome.CompletedProduction.Should().BeFalse();
        outcome.Summary.StatusLabel.Should().Be("你取消了系统确认");
        outcome.Summary.Conclusion.Should().Contain("没有移动");
    }

    [Fact]
    public void Wpf_surfaces_do_not_own_migration_worker_launch_authority()
    {
        var launcher = Read("src", "Css.App", "OfficialUninstallWorkerLauncher.cs");
        var coordinator = Read(
            "src", "Css.App", "MigrationProductionExecutionCoordinator.cs");
        var wpf = new[]
        {
            Read("src", "Css.App", "MainWindow.xaml.cs"),
            Read("src", "Css.App", "MigrationPlanWindow.xaml.cs"),
            Read("src", "Css.App", "MigrationExecutionResultWindow.xaml.cs")
        };

        launcher.Should().Contain("migration-production-worker");
        coordinator.Should().Contain("WindowsMigrationProductionWorkerLauncher.Create");
        coordinator.Should().Contain("RunProductionOnceAsync");
        wpf.Should().OnlyContain(source =>
            !source.Contains("migration-production-worker", StringComparison.Ordinal));
        wpf.Should().OnlyContain(source =>
            !source.Contains("WindowsMigrationProductionWorkerLauncher", StringComparison.Ordinal));
        wpf.Should().OnlyContain(source =>
            !source.Contains("RunProductionOnceAsync", StringComparison.Ordinal));
    }

    [Fact]
    public void Main_window_refreshes_inventory_and_closure_after_every_production_attempt()
    {
        var main = Read("src", "Css.App", "MainWindow.xaml.cs");
        var method = SourceMethodExtractor.Extract(
            main,
            "private async Task ShowMigrationPlanAsync(SoftwareProfile profile)");

        method.Should().Contain("if (window.ProductionExecutionAttempted)");
        method.Should().Contain("await TryScanSoftwareProfilesAfterProductionAttemptAsync()");
        method.Should().Contain("if (refreshedProfiles is null)");
        method.Should().Contain("await RefreshMigrationClosureAsync(refreshUi: true)");
        method.Should().Contain("if (window.ProductionCompleted)");
        method.Should().Contain("不会自动继续搬动");
        method.IndexOf("if (window.ProductionExecutionAttempted)", StringComparison.Ordinal)
            .Should().BeLessThan(method.IndexOf(
                "await TryScanSoftwareProfilesAfterProductionAttemptAsync()",
                StringComparison.Ordinal));
        method.IndexOf("if (refreshedProfiles is null)", StringComparison.Ordinal)
            .Should().BeLessThan(method.IndexOf(
                "await RefreshMigrationClosureAsync(refreshUi: true)",
                StringComparison.Ordinal));
        method.IndexOf("await RefreshMigrationClosureAsync(refreshUi: true)", StringComparison.Ordinal)
            .Should().BeLessThan(method.IndexOf("if (window.ProductionCompleted)", StringComparison.Ordinal));
    }

    private static MigrationWorkerLifecycleResult Completed(
        MigrationElevatedRequestDraft request,
        string? requestId = null) =>
        Completed(request, MigrationExecutionStatus.Completed, requestId);

    private static MigrationWorkerLifecycleResult Completed(
        MigrationElevatedRequestDraft request,
        MigrationExecutionStatus status,
        string? requestId = null) =>
        new()
        {
            Status = MigrationWorkerLifecycleStatus.CompletedProduction,
            ChildExited = true,
            TransportStatus = MigrationTransportStatus.Completed,
            Response = new MigrationElevatedResponseEnvelope
            {
                RequestId = requestId ?? request.RequestId!,
                Result = status == MigrationExecutionStatus.Completed
                    ? OperationResult.Ok(payload: Result(status))
                    : new OperationResult
                {
                    Success = false,
                    Error = "Private fixture failure must remain out of the UI.",
                    Payload = Result(status)
                }
            }
        };

    private static MigrationExecutionResult Result(MigrationExecutionStatus status) =>
        new()
        {
            Status = status,
            Summary = "Private fixture paths must remain out of the UI.",
            MovedPathCount = status == MigrationExecutionStatus.Completed ? 1 : 0,
            RollbackSucceeded = true
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

    private static MigrationElevatedRequestDraft ReadyDraft()
    {
        var operation = new OperationDescriptor
        {
            Kind = "migration.execute",
            Title = "Migration coordinator fixture",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "migration-coordinator-snapshot",
            RollbackRequired = true,
            ConfirmationAccepted = true,
            EvidenceSummary = "verified migration fixture",
            ConfirmationText = "Migrate fixture?",
            AffectedPaths = [@"C:\Users\Fixture\AppData\Local\Demo"],
            Arguments = new Dictionary<string, object?>
            {
                ["destinationRoot"] = @"D:\Software\Demo",
                ["rollbackManifestPath"] = @"D:\Evidence\migration.json",
                ["rollbackManifestSha256"] = new string('A', 64),
                ["snapshotEvidencePath"] = @"D:\Evidence\migration.snapshot.json",
                ["snapshotEvidenceSha256"] = new string('B', 64),
                ["affectedProcesses"] = Array.Empty<string>(),
                ["scheduledTasks"] = Array.Empty<string>(),
                ["startupEntries"] = Array.Empty<string>(),
                ["monitorPaths"] = new[] { @"C:\Users\Fixture\AppData\Local\Demo" }
            }
        };
        return new MigrationElevatedRequestDraft
        {
            Status = MigrationElevatedRequestStatus.Ready,
            MissingRequirements = [],
            PreparedAtUtc = DateTimeOffset.UtcNow,
            RequestId = "migration-coordinator-" + Guid.NewGuid().ToString("N"),
            Operation = operation,
            DescriptorSha256 = MigrationElevatedRequestComposer
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

    private sealed class FakeRunner(MigrationWorkerLifecycleResult result)
        : IMigrationProductionLifecycleRunner
    {
        public int RunCount { get; private set; }

        public Task<MigrationWorkerLifecycleResult> RunAsync(
            MigrationElevatedRequestDraft request,
            CancellationToken cancellationToken = default)
        {
            RunCount++;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(result);
        }
    }
}
