using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Software;
using Css.Core.Timeline;
using Css.Win32.Quarantine;
using FluentAssertions;

namespace Css.Tests;

public sealed class CommunityRuleCacheCleanupPlanTests
{
    private const string PackHash = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    [Fact]
    public void Eligible_exact_files_create_an_unconfirmed_identity_bound_preview()
    {
        var root = Canonical(Path.Combine(Path.GetTempPath(), "omnix-community-plan"));
        var first = Candidate(Path.Combine(root, "Example", "Cache", "first.tmp"), 10);
        var second = Candidate(Path.Combine(root, "Example", "Code Cache", "second.log"), 20);
        var profile = Profile([first, second], skippedRecent: 2, skippedUnsupported: 1);

        var plan = CommunityRuleCacheCleanupPlanBuilder.Create(profile, PackHash);

        plan.CanContinue.Should().BeTrue();
        plan.EligibleFileCount.Should().Be(2);
        plan.EligibleBytes.Should().Be(30);
        plan.SkippedRecentFileCount.Should().Be(2);
        plan.SkippedUnsupportedFileCount.Should().Be(1);
        plan.Summary.Should().Contain("2 个旧缓存文件").And.NotContain(root);
        plan.Operation.Should().NotBeNull();
        plan.Operation!.Kind.Should().Be(CommunityRuleCacheCleanupPlanBuilder.OperationKind);
        plan.Operation.Risk.Should().Be(RiskLevel.Low);
        plan.Operation.RollbackRequired.Should().BeTrue();
        plan.Operation.ConfirmationAccepted.Should().BeFalse();
        plan.Operation.AffectedPaths.Should().Equal(first.Path, second.Path);
        plan.Operation.EstimatedImpactBytes.Should().Be(30);
        CommunityRuleCacheCleanupPlanBuilder.TryGetBinding(plan.Operation, out var binding)
            .Should().BeTrue();
        binding.ActiveRulePackSha256.Should().Be(PackHash);
        binding.Profile.InventorySource.Should().Be(@"HKCU\Software\Example");
        binding.Files.Should().HaveCount(2);
        binding.Files.Should().OnlyContain(file => file.RuleNames.SequenceEqual(new[] { "Example Cache" }));
    }

    [Fact]
    public void Preview_refuses_stale_pack_running_profile_conflicting_overlap_and_candidate_overflow()
    {
        var root = Canonical(Path.Combine(Path.GetTempPath(), "omnix-community-refusal"));
        var first = Candidate(Path.Combine(root, "Example", "Cache", "old.tmp"), 10);
        var ordinary = Profile([first]);
        var running = new SoftwareProfile(ordinary) { RunningProcesses = ["Example.exe"] };
        var stalePack = new SoftwareProfile(ordinary)
        {
            CommunityCacheEvidence = [Evidence([first], packHash: new string('B', 64))]
        };
        var conflict = new SoftwareProfile(ordinary)
        {
            CommunityCacheEvidence =
            [
                Evidence([first], ruleName: "First"),
                Evidence([Candidate(first.Path, 99)], ruleName: "Second")
            ]
        };
        var overflow = Profile(Enumerable.Range(0, QuarantineCandidatePathPolicy.MaximumCandidateCount + 1)
            .Select(index => Candidate(Path.Combine(root, "Example", "Cache", $"old-{index}.tmp"), 1))
            .ToArray());

        CommunityRuleCacheCleanupPlanBuilder.Create(running, PackHash).CanContinue.Should().BeFalse();
        CommunityRuleCacheCleanupPlanBuilder.Create(stalePack, PackHash).CanContinue.Should().BeFalse();
        CommunityRuleCacheCleanupPlanBuilder.Create(conflict, PackHash).CanContinue.Should().BeFalse();
        CommunityRuleCacheCleanupPlanBuilder.Create(overflow, PackHash).CanContinue.Should().BeFalse();
    }

    [Fact]
    public void Execution_revalidation_refuses_pack_profile_process_and_exact_set_changes()
    {
        var root = Canonical(Path.Combine(Path.GetTempPath(), "omnix-community-stale"));
        var first = Candidate(Path.Combine(root, "Example", "Cache", "old.tmp"), 10);
        var original = Profile([first]);
        var operation = CommunityRuleCacheCleanupPlanBuilder.Create(original, PackHash).Operation!;
        var changedProfile = new SoftwareProfile(original) { DisplayVersion = "2.0" };
        var running = new SoftwareProfile(original) { RunningProcesses = ["Example.exe"] };
        var changedSet = Profile([Candidate(Path.Combine(root, "Example", "Cache", "other.tmp"), 10)]);

        CommunityRuleCacheCleanupPlanBuilder.ValidateForExecution(operation, original, PackHash)
            .Success.Should().BeTrue();
        CommunityRuleCacheCleanupPlanBuilder.ValidateForExecution(operation, original, new string('B', 64))
            .Success.Should().BeFalse();
        CommunityRuleCacheCleanupPlanBuilder.ValidateForExecution(operation, changedProfile, PackHash)
            .Success.Should().BeFalse();
        CommunityRuleCacheCleanupPlanBuilder.ValidateForExecution(operation, running, PackHash)
            .Success.Should().BeFalse();
        CommunityRuleCacheCleanupPlanBuilder.ValidateForExecution(operation, changedSet, PackHash)
            .Success.Should().BeFalse();
    }

    [Fact]
    public async Task Confirmed_exact_files_use_existing_quarantine_timeline_and_restore()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-community-operation-" + Guid.NewGuid().ToString("N"));
        var cache = Path.Combine(root, "UserData", "Example", "Cache");
        var firstPath = Path.Combine(cache, "first.tmp");
        var secondPath = Path.Combine(cache, "second.log");
        var quarantineRoot = Path.Combine(root, "Quarantine");
        var databasePath = Path.Combine(root, "timeline.db");
        Directory.CreateDirectory(cache);
        await File.WriteAllTextAsync(firstPath, "one");
        await File.WriteAllTextAsync(secondPath, "two");

        try
        {
            var profile = Profile(
            [
                Candidate(firstPath, new FileInfo(firstPath).Length, File.GetLastWriteTimeUtc(firstPath)),
                Candidate(secondPath, new FileInfo(secondPath).Length, File.GetLastWriteTimeUtc(secondPath))
            ]);
            var plan = CommunityRuleCacheCleanupPlanBuilder.Create(profile, PackHash);
            var reader = new WindowsQuarantineCandidateIdentityReader();
            var preparation = QuarantineOperationPolicy.PrepareForConfirmation(
                plan.Operation!, quarantineRoot, reader);
            preparation.Success.Should().BeTrue(preparation.Error);
            var descriptor = QuarantineOperationPolicy.ConfirmForExecution(preparation.Operation!);
            var quarantine = new FileQuarantineService(quarantineRoot);
            var timeline = new ActionTimelineStore(databasePath);
            var handler = new CommunityRuleCacheCleanupOperationHandler(
                quarantine,
                timeline,
                profile,
                () => PackHash,
                reader);

            var result = await new SafetyOperationPipeline(handler.ExecuteAsync).ExecuteAsync(descriptor);
            var entries = await timeline.LoadRecentAsync(5);
            var records = await quarantine.LoadRecordsAsync();

            result.Success.Should().BeTrue(result.Error);
            File.Exists(firstPath).Should().BeFalse();
            File.Exists(secondPath).Should().BeFalse();
            records.Should().HaveCount(2);
            entries.Should().ContainSingle(entry => entry.RestoreState == RestoreState.Restorable);
            foreach (var record in records)
                (await quarantine.RestoreAsync(record.ManifestPath)).Success.Should().BeTrue();
            File.Exists(firstPath).Should().BeTrue();
            File.Exists(secondPath).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Specialized_handler_refuses_when_active_pack_changes_after_confirmation()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-community-pack-change-" + Guid.NewGuid().ToString("N"));
        var cache = Path.Combine(root, "UserData", "Example", "Cache");
        var filePath = Path.Combine(cache, "old.tmp");
        var quarantineRoot = Path.Combine(root, "Quarantine");
        Directory.CreateDirectory(cache);
        await File.WriteAllTextAsync(filePath, "fixture");

        try
        {
            var profile = Profile(
            [
                Candidate(filePath, new FileInfo(filePath).Length, File.GetLastWriteTimeUtc(filePath))
            ]);
            var operation = CommunityRuleCacheCleanupPlanBuilder.Create(profile, PackHash).Operation!;
            var reader = new WindowsQuarantineCandidateIdentityReader();
            var preparation = QuarantineOperationPolicy.PrepareForConfirmation(
                operation,
                quarantineRoot,
                reader);
            var descriptor = QuarantineOperationPolicy.ConfirmForExecution(preparation.Operation!);
            var handler = new CommunityRuleCacheCleanupOperationHandler(
                new FileQuarantineService(quarantineRoot),
                new ActionTimelineStore(Path.Combine(root, "timeline.db")),
                profile,
                () => new string('B', 64),
                reader);

            var result = await new SafetyOperationPipeline(handler.ExecuteAsync).ExecuteAsync(descriptor);

            result.Success.Should().BeFalse();
            File.Exists(filePath).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Confirmation_uses_exact_file_language_and_keeps_paths_technical_only()
    {
        var root = Canonical(Path.Combine(Path.GetTempPath(), "omnix-community-confirmation"));
        var file = Candidate(Path.Combine(root, "Example", "Cache", "old.tmp"), 1024);
        var operation = CommunityRuleCacheCleanupPlanBuilder.Create(Profile([file]), PackHash).Operation!;

        var view = CleanupConfirmationPresenter.Create(operation, Path.Combine(root, "Quarantine"));

        view.BeginnerText.Should().Contain("1 个精确旧缓存文件")
            .And.Contain("后悔药中心")
            .And.NotContain(file.Path);
        view.OutcomePreviewLines.Should().Contain(line => line.Contains("社区规则只提供候选"));
        view.TechnicalDetails.Should().Contain(line => line.Contains(file.Path));
    }

    [Fact]
    public void Drawer_preview_does_not_mix_eligible_plan_with_the_old_insufficient_evidence_copy()
    {
        var root = Canonical(Path.Combine(Path.GetTempPath(), "omnix-community-drawer"));
        var profile = Profile([Candidate(Path.Combine(root, "Example", "Cache", "old.tmp"), 1024)]);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var plan = CommunityRuleCacheCleanupPlanBuilder.Create(profile, PackHash);

        var view = AppDrawerActionHostPresenter.ShowCommunityCacheCleanup(drawer, plan);

        view.Lines.Should().Contain(line => line.Contains("1 个精确旧缓存文件"));
        view.Lines.Should().NotContain(line => line.Contains("暂不生成清理操作"));
        view.PrimaryActionKey.Should().Be("CommunityCacheCleanup");
    }

    [Fact]
    public void Drawer_flow_revalidates_pack_and_exact_profile_around_confirmation_without_direct_mutation()
    {
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var planner = File.ReadAllText(FindRepositoryFile("src", "Css.Core", "Apps", "CommunityRuleCacheCleanupPlan.cs"));
        var execute = SourceMethodExtractor.Extract(
            main,
            "private async Task ExecutePendingCommunityRuleCacheCleanupAsync()");

        execute.Should().Contain("GetActiveCommunityRulePackSha256")
            .And.Contain("await ScanSoftwareProfilesAsync()")
            .And.Contain("TryResolveBoundProfile")
            .And.Contain("ValidateForExecution")
            .And.Contain("QuarantineOperationPolicy.PrepareForConfirmation")
            .And.Contain("CleanupConfirmationWindow")
            .And.Contain("CommunityRuleCacheCleanupOperationHandler")
            .And.Contain("new SafetyOperationPipeline(handler.ExecuteAsync)")
            .And.Contain("await RefreshCacheCleanupStateAfterAttemptAsync()");
        Occurrences(execute, "GetActiveCommunityRulePackSha256").Should().BeGreaterThanOrEqualTo(2);
        Occurrences(execute, "await ScanSoftwareProfilesAsync()").Should().BeGreaterThanOrEqualTo(2);
        execute.Should().NotContain("File.Delete")
            .And.NotContain("File.Move")
            .And.NotContain("QuarantineAsync")
            .And.NotContain("Registry.")
            .And.NotContain("Process.Start");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AppDrawerScrollViewer\"");
        main.Should().Contain("DrawerActionPreviewPrimaryButton.BringIntoView()");
        main.Should().Contain("GetActiveCommunityRulePackSha256")
            .And.Contain(".LoadActiveCatalog()");
        planner.Should().NotContain("File.Delete")
            .And.NotContain("File.Move")
            .And.NotContain("QuarantineAsync")
            .And.NotContain("SafetyOperationPipeline");
    }

    [Fact]
    public void Gui_smoke_uses_only_owned_fixture_files_and_cancels_before_operation()
    {
        var script = File.ReadAllText(FindRepositoryFile(
            ".omx",
            "gui-community-cache-preview-smoke.ps1"));

        script.Should().Contain("CommunityCachePreview-")
            .And.Contain("LocalApplicationData")
            .And.Contain("CleanupConfirmationCancelButton")
            .And.Contain("OperationExecuted = $false")
            .And.Contain("Remove-Item -LiteralPath $candidateRoot")
            .And.NotContain("CleanupConfirmationConfirmButton")
            .And.NotContain("QuarantineAsync")
            .And.NotContain("SafetyOperationPipeline");
        script.All(character => character <= 127).Should().BeTrue(
            "Windows PowerShell 5.1 smoke source must stay ASCII-only");
    }

    private static SoftwareProfile Profile(
        IReadOnlyList<CommunityRuleFileEvidence> files,
        int skippedRecent = 0,
        int skippedUnsupported = 0) =>
        new()
        {
            Name = "Example",
            Publisher = "Example Publisher",
            DisplayVersion = "1.0",
            InventorySource = @"HKCU\Software\Example",
            InstallPath = @"D:\Software\Example\Install",
            CommunityCacheEvidence =
            [
                Evidence(files, skippedRecent: skippedRecent, skippedUnsupported: skippedUnsupported)
            ]
        };

    private static CommunityRuleCacheEvidence Evidence(
        IReadOnlyList<CommunityRuleFileEvidence> files,
        string packHash = PackHash,
        string ruleName = "Example Cache",
        int skippedRecent = 0,
        int skippedUnsupported = 0) =>
        new()
        {
            RuleName = ruleName,
            RulePackSource = "Fixture",
            RulePackVersion = "1",
            RulePackSha256 = packHash,
            FileCount = files.Count,
            SizeBytes = files.Sum(file => file.SizeBytes),
            CandidateFiles = files,
            CandidateFilesComplete = true,
            CandidateAssessment = new CommunityRuleCandidateAssessment
            {
                Disposition = CommunityRuleCandidateDisposition.EligibleForSafePreview,
                Summary = $"{files.Count} 个旧缓存文件通过第一轮筛选，可进入安全预演。",
                Explanation = "仍需重新核验并准备隔离回滚。",
                Reasons = [CommunityRuleCandidateReason.EligibleStaleCacheFiles],
                EligibleFiles = files,
                EligibleBytes = files.Sum(file => file.SizeBytes),
                SkippedRecentFileCount = skippedRecent,
                SkippedUnsupportedFileCount = skippedUnsupported
            }
        };

    private static CommunityRuleFileEvidence Candidate(string path, long bytes) =>
        Candidate(path, bytes, DateTime.UtcNow.AddDays(-60));

    private static CommunityRuleFileEvidence Candidate(string path, long bytes, DateTime lastWriteUtc) =>
        new()
        {
            Path = Canonical(path),
            SizeBytes = bytes,
            LastWriteTimeUtc = new DateTimeOffset(DateTime.SpecifyKind(lastWriteUtc, DateTimeKind.Utc)),
            Attributes = FileAttributes.Normal
        };

    private static string Canonical(string path) => Path.GetFullPath(path);

    private static int Occurrences(string value, string token) =>
        value.Split(token, StringSplitOptions.None).Length - 1;

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path)) return path;
            directory = directory.Parent;
        }
        throw new FileNotFoundException(Path.Combine(segments));
    }
}
