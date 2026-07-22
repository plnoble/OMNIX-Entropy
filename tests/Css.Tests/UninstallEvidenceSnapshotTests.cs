using Css.Core.Apps;
using Css.Core.Software;
using Css.Snapshot.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class UninstallEvidenceSnapshotTests
{
    private static readonly DateTimeOffset ValidationNow =
        new(2026, 7, 10, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Snapshot_store_persists_and_verifies_pre_uninstall_evidence_without_claiming_rollback()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-uninstall-snapshot-" + Guid.NewGuid().ToString("N"));
        var now = new DateTimeOffset(2026, 7, 10, 20, 0, 0, TimeSpan.Zero);
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"""D:\Software\Example\Install\Uninstall.exe"" /remove",
            DataPaths = [@"C:\Users\Me\AppData\Local\Example\Data"],
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            StartupEntries = ["Example Startup"],
            Services = ["ExampleService"]
        };
        var recovery = new OfficialUninstallRecoveryEvidence
        {
            Method = OfficialUninstallRecoveryMethod.ReinstallSource,
            Reference = @"D:\Installers\ExampleSetup.exe",
            CanRecoverApplication = true,
            UserDataBackupConfirmed = true
        };

        try
        {
            var store = new UninstallEvidenceSnapshotStore(root, () => now);

            var evidence = await store.CreateAsync(profile, recovery);
            var verification = await store.VerifyAsync(evidence, profile);
            var manifest = await store.LoadAsync(evidence.ManifestPath);

            File.Exists(evidence.ManifestPath).Should().BeTrue();
            verification.IsValid.Should().BeTrue();
            evidence.SoftwareName.Should().Be("Example App");
            evidence.CanRestoreApplication.Should().BeFalse();
            evidence.Sha256.Should().NotBeNullOrWhiteSpace();
            manifest.Should().NotBeNull();
            manifest!.SchemaVersion.Should().Be(1);
            manifest.Purpose.Should().Be("pre-uninstall-evidence");
            manifest.CanRestoreApplication.Should().BeFalse();
            manifest.InstallPath.Should().Be(profile.InstallPath);
            manifest.DataPaths.Should().Contain(profile.DataPaths);
            manifest.RecoveryMethod.Should().Be("ReinstallSource");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Snapshot_verification_rejects_a_modified_manifest()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-uninstall-snapshot-" + Guid.NewGuid().ToString("N"));
        var profile = new SoftwareProfile { Name = "Example App" };
        var recovery = new OfficialUninstallRecoveryEvidence
        {
            Method = OfficialUninstallRecoveryMethod.ReinstallSource,
            Reference = "verified-installer",
            CanRecoverApplication = true
        };

        try
        {
            var store = new UninstallEvidenceSnapshotStore(root);
            var evidence = await store.CreateAsync(profile, recovery);

            await File.AppendAllTextAsync(evidence.ManifestPath, "tampered");

            var verification = await store.VerifyAsync(evidence, profile);

            verification.IsValid.Should().BeFalse();
            verification.Reasons.Should().Contain(reason => reason.Contains("\u54c8\u5e0c"));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Snapshot_evidence_validator_rejects_evidence_for_another_software()
    {
        var profile = new SoftwareProfile { Name = "Example App" };
        var evidence = CreateEvidence(softwareName: "Different App");

        var result = OfficialUninstallSnapshotEvidenceValidator.Validate(
            profile,
            evidence,
            _ => evidence.Sha256,
            ValidationNow);

        result.IsValid.Should().BeFalse();
        result.Reasons.Should().Contain(reason => reason.Contains("\u53e6\u4e00\u4e2a\u8f6f\u4ef6"));
    }

    [Fact]
    public void Snapshot_evidence_validator_rejects_stale_evidence()
    {
        var profile = new SoftwareProfile { Name = "Example App" };
        var evidence = CreateEvidence(createdAt: ValidationNow.AddHours(-2));

        var result = OfficialUninstallSnapshotEvidenceValidator.Validate(
            profile,
            evidence,
            _ => evidence.Sha256,
            ValidationNow);

        result.IsValid.Should().BeFalse();
        result.Reasons.Should().Contain(reason => reason.Contains("\u8fc7\u671f"));
    }

    [Fact]
    public void Snapshot_evidence_validator_rejects_a_manifest_that_claims_application_rollback()
    {
        var profile = new SoftwareProfile { Name = "Example App" };
        var evidence = CreateEvidence(canRestoreApplication: true);

        var result = OfficialUninstallSnapshotEvidenceValidator.Validate(
            profile,
            evidence,
            _ => evidence.Sha256,
            ValidationNow);

        result.IsValid.Should().BeFalse();
        result.Reasons.Should().Contain(reason =>
            reason.Contains("\u4e0d\u80fd\u58f0\u79f0") && reason.Contains("\u6062\u590d"));
    }

    [Fact]
    public async Task Final_confirmation_draft_refuses_incomplete_preparation_without_writing_snapshot()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-uninstall-draft-" + Guid.NewGuid().ToString("N"));
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            DataPaths = [@"C:\Users\Me\AppData\Local\Example\Data"]
        };
        var reinstall = ReinstallSourceReadinessPresenter.Create(
            profile,
            fileExists: _ => false,
            directoryExists: _ => false,
            signatureResolver: _ => null);
        var preparation = UninstallRecoveryPreparationPresenter.Create(
            profile,
            reinstall,
            [],
            personalDataBackupAcknowledged: false);
        var service = new UninstallFinalConfirmationDraftService(
            new UninstallEvidenceSnapshotStore(root));

        var draft = await service.CreateAsync(profile, preparation);

        draft.Status.Should().Be(UninstallFinalConfirmationDraftStatus.Refused);
        draft.SnapshotEvidence.Should().BeNull();
        draft.CanExecuteDirectly.Should().BeFalse();
        draft.MissingRequirements.Should().Contain(item => item.Contains("\u5b98\u65b9\u5b89\u88c5\u5305"));
        Directory.Exists(root).Should().BeFalse();
    }

    [Fact]
    public async Task Final_confirmation_draft_creates_verified_snapshot_but_never_executes()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-uninstall-draft-" + Guid.NewGuid().ToString("N"));
        var now = new DateTimeOffset(2026, 7, 10, 20, 0, 0, TimeSpan.Zero);
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"""D:\Software\Example\Install\Uninstall.exe"" /remove",
            DataPaths = [@"C:\Users\Me\AppData\Local\Example\Data"]
        };
        var reinstall = ReinstallSourceReadinessPresenter.CreateForSelectedInstaller(
            profile,
            @"D:\Installers\ExampleSetup.exe",
            fileExists: _ => true,
            signatureResolver: _ => "CN=Example Inc.");
        var preparation = UninstallRecoveryPreparationPresenter.Create(
            profile,
            reinstall,
            [],
            personalDataBackupAcknowledged: true);
        var service = new UninstallFinalConfirmationDraftService(
            new UninstallEvidenceSnapshotStore(root, () => now));

        try
        {
            var draft = await service.CreateAsync(profile, preparation);

            draft.Status.Should().Be(UninstallFinalConfirmationDraftStatus.ReadyForFinalConfirmation);
            draft.SnapshotEvidence.Should().NotBeNull();
            draft.SnapshotValidation.Should().NotBeNull();
            draft.SnapshotValidation!.IsValid.Should().BeTrue();
            draft.RecoveryEvidence.Should().NotBeNull();
            draft.RecoveryEvidence!.UserDataBackupConfirmed.Should().BeTrue();
            draft.CanExecuteDirectly.Should().BeFalse();
            draft.PendingConfirmations.Should().Contain(item => item.Contains("\u5173\u95ed\u8f6f\u4ef6"));
            draft.PendingConfirmations.Should().Contain(item => item.Contains("\u4e0d\u80fd\u4e00\u952e\u6062\u590d"));
            draft.PendingConfirmations.Should().Contain(item => item.Contains("\u5378\u8f7d\u540e\u91cd\u65b0\u626b\u63cf"));

            var beginnerText = string.Join(
                Environment.NewLine,
                new[] { draft.Headline, draft.Summary }
                    .Concat(draft.ReadyFacts)
                    .Concat(draft.PendingConfirmations));
            beginnerText.Should().NotContain(@"C:\");
            beginnerText.Should().NotContain(@"D:\");
            Directory.GetFiles(root, "*.json").Should().ContainSingle();
            Directory.GetFiles(root, "*.tmp-*").Should().BeEmpty();
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Final_confirmation_draft_service_source_has_no_execution_path()
    {
        var sourcePath = FindRepositoryFile(
            "src", "Css.Snapshot", "Uninstall", "UninstallFinalConfirmationDraftService.cs");
        var source = File.ReadAllText(sourcePath);

        source.Should().NotContain("OperationDescriptor");
        source.Should().NotContain("SafetyOperationPipeline");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("Start-Process");
    }

    [Fact]
    public async Task Retention_planner_keeps_newest_and_marks_old_or_excess_valid_manifests()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-uninstall-retention-" + Guid.NewGuid().ToString("N"));
        var now = new DateTimeOffset(2026, 7, 10, 20, 0, 0, TimeSpan.Zero);
        var current = now;
        var profile = new SoftwareProfile { Name = "Example App" };
        var recovery = new OfficialUninstallRecoveryEvidence
        {
            Method = OfficialUninstallRecoveryMethod.ReinstallSource,
            Reference = "verified-installer",
            CanRecoverApplication = true
        };

        try
        {
            var store = new UninstallEvidenceSnapshotStore(root, () => current);
            current = now.AddDays(-40);
            var expired = await store.CreateAsync(profile, recovery);
            current = now.AddDays(-20);
            var excess = await store.CreateAsync(profile, recovery);
            current = now.AddDays(-10);
            var recent = await store.CreateAsync(profile, recovery);
            current = now.AddDays(-1);
            var latest = await store.CreateAsync(profile, recovery);
            var planner = new UninstallEvidenceRetentionPlanner(root);

            var plan = await planner.PlanAsync(
                new UninstallEvidenceRetentionPolicy(
                    MaximumAge: TimeSpan.FromDays(30),
                    MaximumCount: 2),
                now);

            plan.CanApplyDirectly.Should().BeFalse();
            plan.Keep.Select(item => item.SnapshotId).Should().Contain([recent.SnapshotId, latest.SnapshotId]);
            plan.Candidates.Should().Contain(item =>
                item.SnapshotId == expired.SnapshotId && item.Reason.Contains("\u8fc7\u671f"));
            plan.Candidates.Should().Contain(item =>
                item.SnapshotId == excess.SnapshotId && item.Reason.Contains("\u6570\u91cf"));
            plan.PreservedUnknown.Should().BeEmpty();
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Retention_planner_preserves_unknown_and_corrupt_files()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-uninstall-retention-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            var unknown = Path.Combine(root, "uninstall-unknown.json");
            var corrupt = Path.Combine(root, "uninstall-corrupt.json");
            await File.WriteAllTextAsync(unknown, "{ \"purpose\": \"other\" }");
            await File.WriteAllTextAsync(corrupt, "not-json");
            var planner = new UninstallEvidenceRetentionPlanner(root);

            var plan = await planner.PlanAsync(
                new UninstallEvidenceRetentionPolicy(
                    MaximumAge: TimeSpan.FromDays(30),
                    MaximumCount: 2),
                ValidationNow);

            plan.Candidates.Should().BeEmpty();
            plan.PreservedUnknown.Select(item => item.ManifestPath)
                .Should().BeEquivalentTo([unknown, corrupt]);
            File.Exists(unknown).Should().BeTrue();
            File.Exists(corrupt).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Retention_planner_source_is_plan_only()
    {
        var sourcePath = FindRepositoryFile(
            "src", "Css.Snapshot", "Uninstall", "UninstallEvidenceRetentionPlanner.cs");
        var source = File.ReadAllText(sourcePath);

        source.Should().Contain("SearchOption.TopDirectoryOnly");
        source.Should().Contain("Path.GetFullPath");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("File.Move");
        source.Should().NotContain("Directory.Delete");
        source.Should().NotContain("CanApplyDirectly = true");
    }

    private static OfficialUninstallSnapshotEvidence CreateEvidence(
        string softwareName = "Example App",
        DateTimeOffset? createdAt = null,
        bool canRestoreApplication = false) =>
        new()
        {
            SnapshotId = "snapshot-test",
            ManifestPath = @"D:\OMNIX\Snapshots\snapshot-test.json",
            SoftwareName = softwareName,
            CreatedAtUtc = createdAt ?? ValidationNow,
            Sha256 = "A1B2C3D4",
            CanRestoreApplication = canRestoreApplication
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
