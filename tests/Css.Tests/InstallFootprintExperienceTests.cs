using Css.Core.Software;
using Css.InstallGuard.Installers;
using FluentAssertions;

namespace Css.Tests;

public sealed class InstallFootprintExperienceTests
{
    [Fact]
    public void Complete_footprint_finds_an_unregistered_new_C_drive_landing_point()
    {
        var profile = Profile("Existing");
        var before = Snapshot(
            [profile],
            InstallFootprintCaptureStatus.Complete,
            @"C:\ProgramData\Existing");
        var after = Snapshot(
            [profile],
            InstallFootprintCaptureStatus.Complete,
            @"C:\ProgramData\Existing",
            @"C:\ProgramData\UnregisteredTool");

        var report = InstallSnapshotDiffBuilder.Build(before, after);
        var view = InstallSnapshotDiffPresenter.Create(report);

        report.AddedSoftware.Should().BeEmpty();
        report.NewCDrivePaths.Should().ContainSingle()
            .Which.Should().Be(@"C:\ProgramData\UnregisteredTool");
        report.CDriveFootprintStatus.Should().Be(InstallFootprintCaptureStatus.Complete);
        report.HasCDriveWrites.Should().BeTrue();
        view.Summary.Should().Contain("候选 1 个");
        view.Cards.Single(card => card.Title.Contains("C 盘")).Detail
            .Should().Contain("不等于").And.Contain("安装器");

        var firstLevelText = string.Join(
            "\n",
            view.Cards.SelectMany(card => new[] { card.Title, card.Body, card.Detail }));
        firstLevelText.Should().NotContain(@"C:\ProgramData");
        firstLevelText.Should().NotContain("UnregisteredTool");
    }

    [Theory]
    [InlineData(InstallFootprintCaptureStatus.Truncated)]
    [InlineData(InstallFootprintCaptureStatus.Unavailable)]
    public void Incomplete_footprint_never_claims_no_C_drive_change(
        InstallFootprintCaptureStatus incompleteStatus)
    {
        var profile = Profile("Existing");
        var before = Snapshot([profile], incompleteStatus, @"C:\ProgramData\Existing");
        var after = Snapshot([profile], InstallFootprintCaptureStatus.Complete, @"C:\ProgramData\Existing");

        var report = InstallSnapshotDiffBuilder.Build(before, after);
        var view = InstallSnapshotDiffPresenter.Create(report);
        var agent = InstallSnapshotDiffAgentPresenter.Create(report);
        var review = InstallSnapshotDiffEvidenceReviewPresenter.Create(report);
        var preview = InstallSnapshotCandidatePreviewPresenter.Create(
            report,
            InstallSnapshotEligibleActionKind.CacheCleanupPlan);

        report.NewCDrivePaths.Should().BeEmpty();
        report.CDriveFootprintStatus.Should().Be(incompleteStatus);
        report.Summary.Should().Contain("未完成").And.NotContain("未发现安装后");
        view.Cards.Single(card => card.Title.Contains("C 盘")).Body
            .Should().Contain("不能判断");
        agent.Headline.Should().Contain("不能判断");
        agent.NextSteps.Should().Contain(step => step.Contains("重新"));
        review.Summary.Should().Contain("未完成");
        review.EligibleActions.Should().ContainSingle(action =>
            action.Kind == InstallSnapshotEligibleActionKind.ObserveOnly
            && action.SafetyLabel.Contains("证据补齐"));
        preview.Status.Should().Be(InstallSnapshotCandidatePreviewStatus.Refused);
        preview.Summary.Should().Contain("未完成");
        preview.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Inventory_backed_candidate_remains_visible_when_footprint_is_incomplete()
    {
        var before = Snapshot(
            [Profile("Existing")],
            InstallFootprintCaptureStatus.Unavailable);
        var after = Snapshot(
            [Profile("Existing", [@"C:\Users\Fixture\AppData\Local\Existing\Cache"])],
            InstallFootprintCaptureStatus.Unavailable);

        var report = InstallSnapshotDiffBuilder.Build(before, after);

        report.NewCDrivePaths.Should().ContainSingle()
            .Which.Should().Contain("Existing\\Cache");
        report.CDriveFootprintStatus.Should().Be(InstallFootprintCaptureStatus.Unavailable);
        report.Summary.Should().Contain("可能仍有遗漏");
    }

    [Fact]
    public void Footprint_fingerprint_is_order_independent_status_bound_and_rejects_unsafe_paths()
    {
        var first = Capture(
            InstallFootprintCaptureStatus.Complete,
            @"C:\ProgramData\A",
            @"C:\Program Files\B");
        var reordered = Capture(
            InstallFootprintCaptureStatus.Complete,
            @"C:\Program Files\B",
            @"C:\ProgramData\A");
        var truncated = Capture(
            InstallFootprintCaptureStatus.Truncated,
            @"C:\ProgramData\A",
            @"C:\Program Files\B");

        InstallBeforeSnapshotEvidenceService.ComputeFootprintFingerprint(first)
            .Should().Be(InstallBeforeSnapshotEvidenceService.ComputeFootprintFingerprint(reordered));
        InstallBeforeSnapshotEvidenceService.ComputeFootprintFingerprint(truncated)
            .Should().NotBe(InstallBeforeSnapshotEvidenceService.ComputeFootprintFingerprint(first));

        var duplicate = () => InstallBeforeSnapshotEvidenceService.ComputeFootprintFingerprint(
            Capture(InstallFootprintCaptureStatus.Complete, @"C:\ProgramData\A", @"c:\programdata\a"));
        var wrongDrive = () => InstallBeforeSnapshotEvidenceService.ComputeFootprintFingerprint(
            Capture(InstallFootprintCaptureStatus.Complete, @"D:\Software\A"));
        var network = () => InstallBeforeSnapshotEvidenceService.ComputeFootprintFingerprint(
            Capture(InstallFootprintCaptureStatus.Complete, @"\\server\share\A"));

        duplicate.Should().Throw<InvalidOperationException>();
        wrongDrive.Should().Throw<InvalidOperationException>();
        network.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Production_probe_is_bounded_top_level_read_only_and_skips_reparse_points()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.InstallGuard", "Installers", "InstallFootprintCapture.cs"));

        source.Should().Contain("MaximumEntries = 4096");
        source.Should().Contain("SearchOption.TopDirectoryOnly");
        source.Should().Contain("FileAttributes.ReparsePoint");
        source.Should().Contain("TryNormalizeLocalCPath");
        source.Should().NotContain("SearchOption.AllDirectories");
        source.Should().NotContain("ReadAllBytes");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("File.Move");
        source.Should().NotContain("Directory.Delete");
        source.Should().NotContain("Directory.Move");
    }

    [Fact]
    public void Automatic_and_manual_install_snapshots_share_the_same_footprint_probe()
    {
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var coordinator = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "InstallerExecutionCoordinator.cs"));
        var evidence = File.ReadAllText(FindRepositoryFile(
            "src", "Css.InstallGuard", "Installers", "InstallBeforeSnapshotEvidence.cs"));

        main.Should().Contain("var beforeFootprint = await CaptureInstallFootprintAsync()");
        main.Should().Contain("var footprint = await CaptureInstallFootprintAsync()");
        coordinator.Should().Contain("var afterFootprint = await _scanCDriveFootprint");
        coordinator.Should().Contain("FootprintFingerprintSha256");
        coordinator.Should().Contain("FootprintPathCount");
        coordinator.Should().Contain("FootprintStatus");
        evidence.Should().Contain("CurrentSchemaVersion = 2");
        evidence.Should().Contain("ComputeFootprintFingerprint(footprint)");
    }

    private static InstallSystemSnapshot Snapshot(
        IReadOnlyList<SoftwareProfile> profiles,
        InstallFootprintCaptureStatus status,
        params string[] paths) =>
        new(
            new DateTimeOffset(2026, 7, 14, 12, 0, 0, TimeSpan.Zero),
            profiles,
            Capture(status, paths));

    private static InstallFootprintCapture Capture(
        InstallFootprintCaptureStatus status,
        params string[] paths) =>
        new()
        {
            Status = status,
            Paths = paths
        };

    private static SoftwareProfile Profile(
        string name,
        IReadOnlyList<string>? cDrivePaths = null) =>
        new()
        {
            Name = name,
            Publisher = "Fixture",
            InstallPath = @"D:\Software\Fixture\Install",
            CDriveWritePaths = cDrivePaths ?? []
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
}
