using Css.Core.Migration;
using Css.Core.Operations;
using Css.Elevated.Migration;
using Css.Ipc.Migration;
using Css.Ipc.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationProductionWorkerSessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 13, 4, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ActivityProbe_BlocksActiveAndUnqueryableComponents()
    {
        var probe = new WindowsMigrationActivityProbe(
            new StubProcessReader(name => name == "active-app"),
            new StubServiceReader(_ => throw new InvalidOperationException()),
            new StubTaskReader(_ => false));

        var findings = await probe.FindActiveAsync(new MigrationActivityRequest
        {
            ProcessNames = ["active-app", "closed-app"],
            ServiceNames = ["UnknownService"],
            ScheduledTasks = [@"\DisabledTask"]
        });

        findings.Should().Equal(
            "process: active-app",
            "service status unavailable: UnknownService");
    }

    [Fact]
    public async Task Session_DeniesUntrustedPackageBeforeRequestHandler()
    {
        using var fixture = await Fixture.CreateAsync();
        var server = new CapturingServer(fixture.Request);
        var session = new MigrationProductionWorkerSession(
            server,
            new StubAuthorizer(canAuthorize: false),
            fixture.Handler,
            () => Now);

        var result = await session.ServeOnceAsync(fixture.Options);

        result.Status.Should().Be(MigrationTransportStatus.AuthorizationFailed);
        server.AuthorizationCalls.Should().Be(1);
        server.ResponseFactoryCalls.Should().Be(0);
        fixture.Paths.MoveCalls.Should().Be(0);
    }

    [Fact]
    public async Task Session_TrustedFreshRequestExecutesPipelineOnce()
    {
        using var fixture = await Fixture.CreateAsync();
        var server = new CapturingServer(fixture.Request);
        var session = new MigrationProductionWorkerSession(
            server,
            new StubAuthorizer(canAuthorize: true),
            fixture.Handler,
            () => Now);

        var result = await session.ServeOnceAsync(fixture.Options);

        result.Status.Should().Be(MigrationTransportStatus.Completed);
        result.Response!.Result.Success.Should().BeTrue();
        result.Response.Result.Payload.Should().BeOfType<MigrationExecutionResult>()
            .Which.Status.Should().Be(MigrationExecutionStatus.Completed);
        fixture.Paths.MoveCalls.Should().Be(1);
        fixture.Monitoring.Saved.Should().NotBeNull();
    }

    [Fact]
    public void ProductionComposition_IsMigrationSpecificAndMutationAuthorityStaysElevated()
    {
        var root = FindRepoRoot();
        var worker = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Css.Elevated",
            "MigrationProductionWorker.cs"));
        var session = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Css.Elevated",
            "Migration",
            "MigrationProductionWorkerSession.cs"));
        var program = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Css.Elevated",
            "Program.cs"));
        var launcher = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Css.App",
            "OfficialUninstallWorkerLauncher.cs"));
        var wpfFiles = new[]
        {
            "MainWindow.xaml.cs",
            "MigrationPlanWindow.xaml.cs",
            "MigrationExecutionResultWindow.xaml.cs"
        }
            .Select(file => File.ReadAllText(Path.Combine(
                root,
                "src",
                "Css.App",
                file)))
            .ToArray();

        worker.Should().Contain("WindowsDirectoryMigrationPathAdapter");
        worker.Should().Contain("WindowsMigrationActivityProbe");
        worker.Should().Contain("WindowsMigrationProductionPackageAuthorizer");
        session.Should().Contain("SafetyOperationPipeline");
        session.Should().Contain("FixedTimeEquals");
        program.Should().Contain("migration-production-worker");
        launcher.Should().Contain("migration-production-worker");
        launcher.Should().Contain("WindowsMigrationProductionWorkerLauncher");
        wpfFiles.Should().OnlyContain(code =>
            !code.Contains("migration-production-worker", StringComparison.Ordinal));
        wpfFiles.Should().OnlyContain(code =>
            !code.Contains("WindowsMigrationProductionWorkerLauncher", StringComparison.Ordinal));
        wpfFiles.Should().OnlyContain(code =>
            !code.Contains("RunProductionOnceAsync", StringComparison.Ordinal));
    }

    private sealed class Fixture : IDisposable
    {
        private Fixture(
            string root,
            MigrationElevatedRequestDraft request,
            MigrationOperationHandler handler,
            FakePaths paths,
            FakeMonitoring monitoring,
            MigrationOneShotWorkerOptions options)
        {
            Root = root;
            Request = request;
            Handler = handler;
            Paths = paths;
            Monitoring = monitoring;
            Options = options;
        }

        public string Root { get; }
        public MigrationElevatedRequestDraft Request { get; }
        public MigrationOperationHandler Handler { get; }
        public FakePaths Paths { get; }
        public FakeMonitoring Monitoring { get; }
        public MigrationOneShotWorkerOptions Options { get; }

        public static async Task<Fixture> CreateAsync()
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "omnix-migration-worker-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var manifestPath = Path.Combine(root, "rollback.json");
            const string source = @"C:\Users\Fixture\AppData\Local\Demo";
            const string destinationRoot = @"D:\Software\Demo";
            const string destination = @"D:\Software\Demo\MigratedData\Demo";
            var manifest = new MigrationRollbackManifest
            {
                Id = "manifest-1",
                CreatedAt = Now,
                SoftwareName = "Demo",
                SnapshotId = "snapshot-1",
                DestinationRoot = destinationRoot,
                IsPlanOnly = true,
                Entries =
                [
                    new MigrationRollbackManifestEntry
                    {
                        OriginalPath = source,
                        PlannedDestinationPath = destination,
                        RestorePath = source,
                        Reason = "fixture"
                    }
                ],
                ServicesToRestore = [],
                StartupEntriesToRestore = [],
                ScheduledTasksToRestore = [],
                MonitorPaths = [source],
                VerificationSteps = ["verify"],
                RollbackSteps = ["restore"]
            };
            await MigrationRollbackManifestStore.WriteAsync(manifest, manifestPath);
            var manifestHash = await MigrationRollbackManifestStore.ComputeSha256Async(manifestPath);
            var snapshotEvidence = await MigrationSnapshotEvidenceService.CreateAsync(
                manifest,
                manifestPath,
                manifestHash,
                Path.Combine(root, "snapshot.json"),
                new FixedSnapshotSourceReader(source),
                Now);
            var operation = new OperationDescriptor
            {
                Kind = "migration.execute",
                Title = "Demo migration",
                Source = OperationSource.Manual,
                Risk = RiskLevel.High,
                IsDestructive = true,
                RequiresElevation = true,
                RequiresSnapshot = true,
                SnapshotId = "snapshot-1",
                RollbackRequired = true,
                ConfirmationAccepted = true,
                EvidenceSummary = "Fixture evidence",
                EstimatedImpactBytes = 1,
                ConfirmationText = "Migrate Demo?",
                AffectedPaths = [source],
                Arguments = new Dictionary<string, object?>
                {
                    ["destinationRoot"] = destinationRoot,
                    ["rollbackManifestPath"] = manifestPath,
                    ["rollbackManifestSha256"] = manifestHash,
                    ["snapshotEvidencePath"] = snapshotEvidence.EvidencePath,
                    ["snapshotEvidenceSha256"] = snapshotEvidence.Sha256,
                    ["affectedProcesses"] = Array.Empty<string>(),
                    ["scheduledTasks"] = Array.Empty<string>(),
                    ["startupEntries"] = Array.Empty<string>(),
                    ["monitorPaths"] = new[] { source }
                }
            };
            var request = new MigrationElevatedRequestDraft
            {
                Status = MigrationElevatedRequestStatus.Ready,
                MissingRequirements = [],
                PreparedAtUtc = Now,
                RequestId = "migration-request-fixture",
                DescriptorSha256 = MigrationElevatedRequestComposer
                    .ComputeDescriptorSha256(operation),
                Operation = operation
            };
            var paths = new FakePaths(source, destination);
            var monitoring = new FakeMonitoring();
            var handler = new MigrationOperationHandler(
                new EmptyActivityProbe(),
                paths,
                new AllowPolicy(),
                new FixedSnapshotSourceReader(source),
                monitoring,
                () => Now);
            var client = new OfficialUninstallPipePeerIdentity
            {
                UserSid = "S-1-5-21-fixture",
                ProcessId = 100,
                WindowsSessionId = 1
            };
            return new Fixture(
                root,
                request,
                handler,
                paths,
                monitoring,
                new MigrationOneShotWorkerOptions
                {
                    PipeName = "fixture-pipe",
                    SessionId = "fixture-session",
                    ExpectedClient = client,
                    Worker = client with { ProcessId = 200 },
                    Timeout = TimeSpan.FromSeconds(1)
                });
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class CapturingServer(MigrationElevatedRequestDraft request)
        : IMigrationOneShotWorkerServer
    {
        public int AuthorizationCalls { get; private set; }
        public int ResponseFactoryCalls { get; private set; }

        public async Task<MigrationTransportResult> ServeOnceAsync(
            MigrationOneShotWorkerOptions options,
            MigrationOneShotWorkerAuthorization authorization,
            Func<MigrationElevatedRequestDraft, CancellationToken,
                Task<MigrationElevatedResponseEnvelope>> responseFactory,
            CancellationToken cancellationToken = default)
        {
            AuthorizationCalls++;
            if (!await authorization(
                    options.ExpectedClient,
                    options.Worker,
                    cancellationToken))
            {
                return new MigrationTransportResult
                {
                    Status = MigrationTransportStatus.AuthorizationFailed
                };
            }

            ResponseFactoryCalls++;
            return new MigrationTransportResult
            {
                Status = MigrationTransportStatus.Completed,
                Response = await responseFactory(request, cancellationToken)
            };
        }
    }

    private sealed class StubAuthorizer(bool canAuthorize)
        : IMigrationProductionPackageAuthorizer
    {
        public MigrationProductionPackageTrustResult Authorize(
            OfficialUninstallPipePeerIdentity actualClient,
            OfficialUninstallPipePeerIdentity worker) =>
            new()
            {
                Status = canAuthorize
                    ? MigrationProductionPackageTrustStatus.Trusted
                    : MigrationProductionPackageTrustStatus.ClientNotTrusted
            };
    }

    private sealed class EmptyActivityProbe : IMigrationActivityProbe
    {
        public Task<IReadOnlyList<string>> FindActiveAsync(
            MigrationActivityRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);
    }

    private sealed class FixedSnapshotSourceReader(string source)
        : IMigrationSnapshotSourceReader
    {
        public Task<MigrationSnapshotSourceEvidence> ObserveAsync(
            string path,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new MigrationSnapshotSourceEvidence
            {
                Path = source,
                Exists = true,
                IsDirectory = true,
                IsRedirect = false,
                ObservedBytes = 1,
                LastWriteUtc = Now
            });
    }

    private sealed class AllowPolicy : IMigrationPathPolicy
    {
        public string? Validate(
            MigrationRollbackManifest manifest,
            MigrationRollbackManifestEntry entry) => null;
    }

    private sealed class FakePaths(string source, string destination) : IMigrationPathAdapter
    {
        public int MoveCalls { get; private set; }

        public Task<MigrationPathObservation> ObserveAsync(
            string path,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new MigrationPathObservation
            {
                Path = path,
                Exists = string.Equals(path, source, StringComparison.OrdinalIgnoreCase),
                IsDirectory = string.Equals(path, source, StringComparison.OrdinalIgnoreCase)
            });

        public Task<MigrationMoveResult> MoveAndRedirectAsync(
            MigrationRollbackManifestEntry entry,
            CancellationToken cancellationToken = default)
        {
            MoveCalls++;
            return Task.FromResult(new MigrationMoveResult
            {
                OriginalPath = source,
                DestinationPath = destination,
                RedirectCreated = true
            });
        }

        public Task RollbackAsync(
            MigrationRollbackManifestEntry entry,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeMonitoring : IMigrationMonitoringStore
    {
        public MigrationMonitoringRecord? Saved { get; private set; }

        public Task SaveAsync(
            MigrationMonitoringRecord record,
            CancellationToken cancellationToken = default)
        {
            Saved = record;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<MigrationMonitoringRecord>> LoadAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<MigrationMonitoringRecord>>(
                Saved is null ? [] : [Saved]);
    }

    private sealed class StubProcessReader(Func<string, bool> read)
        : IWindowsMigrationProcessStateReader
    {
        public bool IsRunning(string processName) => read(processName);
    }

    private sealed class StubServiceReader(Func<string, bool> read)
        : IWindowsMigrationServiceStateReader
    {
        public bool IsRunningOrTransitioning(string serviceName) => read(serviceName);
    }

    private sealed class StubTaskReader(Func<string, bool> read)
        : IWindowsMigrationScheduledTaskStateReader
    {
        public bool IsEnabledOrRunning(string taskPath) => read(taskPath);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ComputerSecuritySoftware.slnx")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Repository root not found.");
    }
}
