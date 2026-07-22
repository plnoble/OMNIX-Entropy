using System.Security.Cryptography;
using System.Text;
using Css.Core.Operations;
using Css.Core.Software;
using Css.InstallGuard.Installers;
using Css.Win32.Security;
using FluentAssertions;

namespace Css.Tests;

public sealed class InstallerLaunchOperationTests : IDisposable
{
    private readonly string _fixtureRoot = Path.Combine(
        Path.GetTempPath(),
        "omnix-installer-launch-fixture-" + Guid.NewGuid().ToString("N"));

    public InstallerLaunchOperationTests() => Directory.CreateDirectory(_fixtureRoot);

    [Fact]
    public async Task Confirmed_trusted_operation_revalidates_snapshot_and_package_before_launcher()
    {
        var fixture = await CreatePlanAsync();
        var launcher = new FakeLauncher(InteractiveInstallerLaunchStatus.Started);
        var handler = Handler(fixture.Package, launcher, fixture.Now);
        var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

        var result = await pipeline.ExecuteAsync(fixture.Plan.Operation);

        result.Success.Should().BeTrue();
        result.Summary.Should().Contain("不代表安装已经成功");
        launcher.Requests.Should().ContainSingle();
        launcher.Requests[0].PackagePath.Should().Be(fixture.Package.PackagePath);
        launcher.Requests[0].ExpectedSha256.Should().Be(fixture.Package.Sha256);
        launcher.Requests[0].Arguments.Should().ContainSingle()
            .Which.Should().StartWith("/DIR=");
    }

    [Fact]
    public async Task Planner_refuses_an_unavailable_target_before_final_consent()
    {
        var fixture = await CreatePlanAsync();
        var analysis = InstallerAnalyzer.AnalyzePackage(fixture.Package);

        var create = () => InstallerLaunchOperationPlanner.Create(
            analysis,
            fixture.Package,
            fixture.Plan.Capability,
            fixture.Plan.BeforeSnapshot,
            new DenyFixtureTargetPolicy());

        create.Should().Throw<InvalidOperationException>()
            .WithMessage("*target*");
    }

    [Fact]
    public async Task Unconfirmed_plan_is_rejected_and_all_four_consent_items_are_required()
    {
        var fixture = await CreatePlanAsync(confirm: false);
        var launcher = new FakeLauncher(InteractiveInstallerLaunchStatus.Started);
        var handler = Handler(fixture.Package, launcher, fixture.Now);
        var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

        var result = await pipeline.ExecuteAsync(fixture.Plan.Operation);
        var incomplete = () => InstallerLaunchFinalConsentService.Confirm(
            fixture.Plan.Operation,
            new InstallerLaunchFinalConsentDecision
            {
                PackagePublisherAccepted = true,
                LocationLimitAccepted = true,
                InteractiveReviewAccepted = true,
                PostScanLimitAccepted = false
            },
            fixture.Now);

        fixture.Plan.Operation.ConfirmationAccepted.Should().BeFalse();
        result.Success.Should().BeFalse();
        incomplete.Should().Throw<InvalidOperationException>();
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Changed_package_hash_is_refused_before_launcher()
    {
        var fixture = await CreatePlanAsync();
        var changed = fixture.Package with { Sha256 = new string('B', 64) };
        var launcher = new FakeLauncher(InteractiveInstallerLaunchStatus.Started);
        var handler = Handler(changed, launcher, fixture.Now);

        var result = await handler.ExecuteAsync(fixture.Plan.Operation);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("安装包在确认后发生变化");
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Tampered_snapshot_file_is_refused_before_launcher()
    {
        var fixture = await CreatePlanAsync();
        await File.AppendAllTextAsync(fixture.Plan.BeforeSnapshot.EvidencePath, "tampered");
        var launcher = new FakeLauncher(InteractiveInstallerLaunchStatus.Started);
        var handler = Handler(fixture.Package, launcher, fixture.Now);

        var result = await handler.ExecuteAsync(fixture.Plan.Operation);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("快照证据无效");
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Silent_or_unconfirmed_arguments_are_refused_before_launcher()
    {
        var fixture = await CreatePlanAsync();
        var launcher = new FakeLauncher(InteractiveInstallerLaunchStatus.Started);
        var handler = Handler(fixture.Package, launcher, fixture.Now);
        var arguments = fixture.Plan.Operation.Arguments.ToDictionary(pair => pair.Key, pair => pair.Value);
        arguments["interactiveArguments"] = new[] { "/VERYSILENT" };
        var changed = Clone(fixture.Plan.Operation, arguments: arguments);

        var result = await handler.ExecuteAsync(changed);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("安装参数");
        launcher.Requests.Should().BeEmpty();
    }

    [Theory]
    [InlineData(OperationSource.Agent, true)]
    [InlineData(OperationSource.Manual, false)]
    public async Task Handler_independently_requires_manual_source_and_final_confirmation(
        OperationSource source,
        bool confirmationAccepted)
    {
        var fixture = await CreatePlanAsync();
        var launcher = new FakeLauncher(InteractiveInstallerLaunchStatus.Started);
        var handler = Handler(fixture.Package, launcher, fixture.Now);
        var changed = Clone(
            fixture.Plan.Operation,
            sourceOverride: source,
            confirmationAccepted: confirmationAccepted);

        var result = await handler.ExecuteAsync(changed);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("人工确认");
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Pipeline_rejects_a_missing_snapshot_before_handler_or_launcher()
    {
        var fixture = await CreatePlanAsync();
        var launcher = new FakeLauncher(InteractiveInstallerLaunchStatus.Started);
        var handler = Handler(fixture.Package, launcher, fixture.Now);
        var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
        var changed = Clone(fixture.Plan.Operation, snapshotId: string.Empty);

        var result = await pipeline.ExecuteAsync(changed);

        result.Success.Should().BeFalse();
        result.Error.Should().ContainEquivalentOf("snapshot");
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Stale_snapshot_is_refused_before_launcher()
    {
        var fixture = await CreatePlanAsync();
        var launcher = new FakeLauncher(InteractiveInstallerLaunchStatus.Started);
        var arguments = fixture.Plan.Operation.Arguments.ToDictionary(pair => pair.Key, pair => pair.Value);
        arguments["finalConsentUtc"] = fixture.Now.AddMinutes(31).ToString("O");
        var refreshedConsent = Clone(fixture.Plan.Operation, arguments: arguments);
        var handler = Handler(
            fixture.Package,
            launcher,
            fixture.Now.AddMinutes(31));

        var result = await handler.ExecuteAsync(refreshedConsent);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("快照已过期");
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Expired_final_consent_is_refused_before_snapshot_and_launcher()
    {
        var fixture = await CreatePlanAsync();
        var launcher = new FakeLauncher(InteractiveInstallerLaunchStatus.Started);
        var handler = Handler(
            fixture.Package,
            launcher,
            fixture.Now.AddMinutes(16));

        var result = await handler.ExecuteAsync(fixture.Plan.Operation);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("最终确认已过期");
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Snapshot_store_rejects_unknown_json_fields_and_hash_tamper()
    {
        var fixture = await CreatePlanAsync();
        var original = await File.ReadAllTextAsync(fixture.Plan.BeforeSnapshot.EvidencePath);
        var changed = original.TrimEnd().TrimEnd('}') + ",\n  \"Unexpected\": true\n}";
        await File.WriteAllTextAsync(fixture.Plan.BeforeSnapshot.EvidencePath, changed);
        var changedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(changed)));

        var strict = await InstallBeforeSnapshotEvidenceStore.ReadVerifiedAsync(
            fixture.Plan.BeforeSnapshot.EvidencePath,
            changedHash);
        var originalHash = await InstallBeforeSnapshotEvidenceStore.ReadVerifiedAsync(
            fixture.Plan.BeforeSnapshot.EvidencePath,
            fixture.Plan.BeforeSnapshot.Sha256);

        strict.IsValid.Should().BeFalse();
        originalHash.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Inventory_fingerprint_is_stable_for_equivalent_ordering_and_changes_with_background_items()
    {
        var first = Profile("Example", startup: ["Run B", "Run A"]);
        var second = Profile("Tool");
        var reordered = InstallBeforeSnapshotEvidenceService.ComputeInventoryFingerprint([second, first]);
        var original = InstallBeforeSnapshotEvidenceService.ComputeInventoryFingerprint([first, second]);
        var changed = InstallBeforeSnapshotEvidenceService.ComputeInventoryFingerprint(
            [Profile("Example", startup: ["Run C"]), second]);

        reordered.Should().Be(original);
        changed.Should().NotBe(original);
    }

    [Fact]
    public void Production_launcher_source_has_no_silent_or_forced_elevation_authority()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.InstallGuard", "Installers", "InstallerLaunchOperation.cs"));

        source.Should().Contain("UseShellExecute = true");
        source.Should().Contain("Process.Start(start)");
        source.Should().NotContain("Verb = \"runas\"");
        source.Should().NotContain("/VERYSILENT");
        source.Should().NotContain("/SILENT");
        source.Should().NotContain("/quiet");
        source.Should().NotContain("/qn");
        source.Should().NotContain("WindowStyle = ProcessWindowStyle.Hidden");
    }

    public void Dispose()
    {
        if (Directory.Exists(_fixtureRoot))
            Directory.Delete(_fixtureRoot, recursive: true);
    }

    private async Task<Fixture> CreatePlanAsync(bool confirm = true)
    {
        var now = DateTimeOffset.UtcNow;
        var packagePath = Path.Combine(_fixtureRoot, "ExampleTool.exe");
        await File.WriteAllTextAsync(packagePath, "Inno Setup Setup Data");
        var info = new FileInfo(packagePath);
        var packageBytes = await File.ReadAllBytesAsync(packagePath);
        var package = new InstallerPackageEvidence
        {
            Status = InstallerPackageInspectionStatus.Ready,
            PackagePath = packagePath,
            FileName = info.Name,
            LengthBytes = info.Length,
            LastWriteUtc = new DateTimeOffset(info.LastWriteTimeUtc, TimeSpan.Zero),
            Sha256 = Convert.ToHexString(SHA256.HashData(packageBytes)),
            SignatureStatus = AuthenticodeSignatureStatus.Trusted,
            SignerSubject = "CN=Fixture Publisher",
            DetectedKind = InstallerKind.InnoSetup,
            KindConfidence = InstallerKindConfidence.High,
            KindEvidence = ["fixture marker"]
        };
        var inventory = new InstallSystemSnapshot(now, [Profile("Existing")]);
        var snapshot = await InstallBeforeSnapshotEvidenceService.CreateAsync(
            package,
            inventory,
            Path.Combine(_fixtureRoot, Guid.NewGuid().ToString("N") + ".json"),
            now);
        var analysis = InstallerAnalyzer.AnalyzePackage(package);
        var capability = InstallerRoutingCapabilityPolicy.Evaluate(analysis, package);
        var plan = InstallerLaunchOperationPlanner.Create(
            analysis,
            package,
            capability,
            snapshot,
            new AllowFixtureTargetPolicy());
        if (confirm)
        {
            plan = plan with
            {
                Operation = InstallerLaunchFinalConsentService.Confirm(
                    plan.Operation,
                    CompleteConsent(),
                    now)
            };
        }
        return new Fixture(now, package, plan);
    }

    private static InstallerLaunchFinalConsentDecision CompleteConsent() =>
        new()
        {
            PackagePublisherAccepted = true,
            LocationLimitAccepted = true,
            InteractiveReviewAccepted = true,
            PostScanLimitAccepted = true
        };

    private static InstallerLaunchOperationHandler Handler(
        InstallerPackageEvidence currentPackage,
        FakeLauncher launcher,
        DateTimeOffset now) =>
        new(
            new FakeInspector(currentPackage),
            new InstallBeforeSnapshotEvidenceReader(),
            new AllowFixtureTargetPolicy(),
            launcher,
            () => now);

    private static OperationDescriptor Clone(
        OperationDescriptor source,
        OperationSource? sourceOverride = null,
        bool? confirmationAccepted = null,
        string? snapshotId = null,
        IReadOnlyDictionary<string, object?>? arguments = null) =>
        new()
        {
            Kind = source.Kind,
            Title = source.Title,
            Source = sourceOverride ?? source.Source,
            Risk = source.Risk,
            IsDestructive = source.IsDestructive,
            RequiresElevation = source.RequiresElevation,
            RequiresSnapshot = source.RequiresSnapshot,
            SnapshotId = snapshotId ?? source.SnapshotId,
            RollbackRequired = source.RollbackRequired,
            ConfirmationAccepted = confirmationAccepted ?? source.ConfirmationAccepted,
            EvidenceSummary = source.EvidenceSummary,
            EstimatedImpactBytes = source.EstimatedImpactBytes,
            ConfirmationText = source.ConfirmationText,
            AffectedPaths = source.AffectedPaths,
            AffectedRegistryKeys = source.AffectedRegistryKeys,
            AffectedServices = source.AffectedServices,
            Arguments = arguments ?? source.Arguments
        };

    private static SoftwareProfile Profile(
        string name,
        IReadOnlyList<string>? startup = null) =>
        new()
        {
            Name = name,
            Publisher = "Fixture",
            InstallPath = @"D:\Software\Fixture\Install",
            StartupEntries = startup ?? []
        };

    private static string FindRepositoryFile(params string[] segments) =>
        Path.Combine([FindRepositoryRoot(), .. segments]);

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

    private sealed record Fixture(
        DateTimeOffset Now,
        InstallerPackageEvidence Package,
        InstallerLaunchOperationPlan Plan);

    private sealed class FakeInspector(InstallerPackageEvidence evidence)
        : IInstallerPackageInspector
    {
        public InstallerPackageEvidence Inspect(string packagePath) => evidence;
    }

    private sealed class AllowFixtureTargetPolicy : IInstallerTargetPathPolicy
    {
        public bool IsAllowed(string targetPath, out string? reason)
        {
            reason = null;
            return true;
        }
    }

    private sealed class DenyFixtureTargetPolicy : IInstallerTargetPathPolicy
    {
        public bool IsAllowed(string targetPath, out string? reason)
        {
            reason = "fixture target unavailable";
            return false;
        }
    }

    private sealed class FakeLauncher(InteractiveInstallerLaunchStatus status)
        : IInteractiveInstallerProcessLauncher
    {
        public List<InteractiveInstallerLaunchRequest> Requests { get; } = [];

        public ValueTask<InteractiveInstallerLaunchResult> LaunchAsync(
            InteractiveInstallerLaunchRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return ValueTask.FromResult(new InteractiveInstallerLaunchResult
            {
                Status = status,
                Session = status == InteractiveInstallerLaunchStatus.Started
                    ? new FakeSession()
                    : null
            });
        }
    }

    private sealed class FakeSession : IInteractiveInstallerProcessSession
    {
        public int? ExitCode => 0;
        public Task WaitForExitAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
        public void Dispose()
        {
        }
    }
}
