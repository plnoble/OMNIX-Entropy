using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class CommunityRuleCandidatePolicyTests
{
    [Fact]
    public void Complete_old_low_risk_files_can_enter_safe_preview_without_execution_authority()
    {
        var root = Canonical(Path.Combine(Path.GetTempPath(), "omnix-candidate-root"));
        var path = Canonical(Path.Combine(root, "Example", "Cache", "old.tmp"));
        var profile = new SoftwareProfile
        {
            Name = "Example",
            InstallPath = Canonical(Path.Combine(root, "Example", "Install")),
            DataPaths = [root]
        };

        var result = CommunityRuleCandidatePolicy.Evaluate(
            profile,
            Evidence([CandidateFile(path, 40, DateTimeOffset.UtcNow.AddDays(-45))]),
            [root],
            _ => true,
            _ => false,
            DateTimeOffset.UtcNow);

        result.Disposition.Should().Be(CommunityRuleCandidateDisposition.EligibleForSafePreview);
        result.EligibleFiles.Should().ContainSingle();
        result.EligibleFiles[0].Path.Should().Be(path);
        result.EligibleBytes.Should().Be(40);
        result.Reasons.Should().Equal(CommunityRuleCandidateReason.EligibleStaleCacheFiles);
        result.IsExecutionAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Running_warning_incomplete_and_mutating_rule_intent_remain_preview_only()
    {
        var root = Canonical(Path.Combine(Path.GetTempPath(), "omnix-candidate-preview"));
        var file = CandidateFile(
            Canonical(Path.Combine(root, "Example", "Cache", "old.tmp")),
            10,
            DateTimeOffset.UtcNow.AddDays(-60));
        var running = new SoftwareProfile { Name = "Example", DataPaths = [root], RunningProcesses = ["Example.exe"] };
        var ordinary = new SoftwareProfile { Name = "Example", DataPaths = [root] };

        var runningResult = Evaluate(running, Evidence([file]), root);
        var warningResult = Evaluate(ordinary, Evidence([file], warning: "Review first"), root);
        var incompleteResult = Evaluate(ordinary, Evidence([file], lowerBound: true), root);
        var truncatedCandidates = Evaluate(ordinary, Evidence([], candidateFilesComplete: false), root);
        var registryIntent = Evaluate(ordinary, Evidence([file], registryTargetCount: 1), root);
        var removeSelfIntent = Evaluate(ordinary, Evidence([file], includesRemoveSelf: true), root);

        runningResult.Disposition.Should().Be(CommunityRuleCandidateDisposition.PreviewOnly);
        runningResult.Reasons.Should().Contain(CommunityRuleCandidateReason.ApplicationRunning);
        warningResult.Reasons.Should().Contain(CommunityRuleCandidateReason.RuleWarning);
        incompleteResult.Reasons.Should().Contain(CommunityRuleCandidateReason.IncompleteScan);
        truncatedCandidates.Reasons.Should().Contain(CommunityRuleCandidateReason.IncompleteCandidateSet);
        registryIntent.Reasons.Should().Contain(CommunityRuleCandidateReason.RegistryIntentPresent);
        removeSelfIntent.Reasons.Should().Contain(CommunityRuleCandidateReason.DirectoryRemovalIntentPresent);
        new[] { runningResult, warningResult, incompleteResult, truncatedCandidates, registryIntent, removeSelfIntent }
            .Should().OnlyContain(result => !result.IsExecutionAuthorized && result.EligibleFiles.Count == 0);
    }

    [Fact]
    public void System_install_outside_and_reparse_paths_are_refused_fail_closed()
    {
        var root = Canonical(Path.Combine(Path.GetTempPath(), "omnix-candidate-refused"));
        var install = Canonical(Path.Combine(root, "Example", "Install"));
        var cache = Canonical(Path.Combine(root, "Example", "Cache", "old.tmp"));
        var installFile = Canonical(Path.Combine(install, "Cache", "old.tmp"));
        var outside = Canonical(Path.Combine(Path.GetTempPath(), "outside", "Cache", "old.tmp"));
        var ordinary = new SoftwareProfile { Name = "Example", InstallPath = install, DataPaths = [root] };
        var system = new SoftwareProfile { Name = "Driver", Category = SoftwareCategory.SystemTool, DataPaths = [root] };

        Evaluate(system, Evidence([CandidateFile(cache, 1, Old())]), root)
            .Reasons.Should().Contain(CommunityRuleCandidateReason.SystemApplication);
        Evaluate(ordinary, Evidence([CandidateFile(installFile, 1, Old())]), root)
            .Reasons.Should().Contain(CommunityRuleCandidateReason.InsideInstallLocation);
        Evaluate(ordinary, Evidence([CandidateFile(outside, 1, Old())]), root)
            .Reasons.Should().Contain(CommunityRuleCandidateReason.OutsideApprovedUserData);
        CommunityRuleCandidatePolicy.Evaluate(
                ordinary,
                Evidence([CandidateFile(cache, 1, Old())]),
                [root],
                _ => true,
                path => path.Equals(cache, StringComparison.OrdinalIgnoreCase),
                DateTimeOffset.UtcNow)
            .Reasons.Should().Contain(CommunityRuleCandidateReason.ReparsePath);

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrWhiteSpace(programFiles))
        {
            var protectedFile = Canonical(Path.Combine(programFiles, "Example", "Cache", "old.tmp"));
            Evaluate(
                    new SoftwareProfile { Name = "Example", DataPaths = [programFiles] },
                    Evidence([CandidateFile(protectedFile, 1, Old())]),
                    Canonical(programFiles))
                .Reasons.Should().Contain(CommunityRuleCandidateReason.InsideProtectedSystemLocation);
        }
    }

    [Fact]
    public void Changed_or_missing_files_require_a_fresh_scan()
    {
        var root = Canonical(Path.Combine(Path.GetTempPath(), "omnix-candidate-changed"));
        var path = Canonical(Path.Combine(root, "Example", "Cache", "old.tmp"));
        var profile = new SoftwareProfile { Name = "Example", DataPaths = [root] };

        var result = CommunityRuleCandidatePolicy.Evaluate(
            profile,
            Evidence([CandidateFile(path, 1, Old())]),
            [root],
            _ => false,
            _ => false,
            DateTimeOffset.UtcNow);

        result.Disposition.Should().Be(CommunityRuleCandidateDisposition.PreviewOnly);
        result.Reasons.Should().Contain(CommunityRuleCandidateReason.MissingFile);
        result.Summary.Should().Contain("重新扫描");
        result.IsExecutionAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Recent_and_unrecognized_files_stay_visible_but_do_not_enter_safe_preview()
    {
        var root = Canonical(Path.Combine(Path.GetTempPath(), "omnix-candidate-kept"));
        var profile = new SoftwareProfile { Name = "Example", DataPaths = [root] };
        var recent = CandidateFile(
            Canonical(Path.Combine(root, "Example", "Cache", "recent.tmp")),
            20,
            DateTimeOffset.UtcNow.AddDays(-2));
        var userState = CandidateFile(
            Canonical(Path.Combine(root, "Example", "Cache", "state.db")),
            30,
            Old());

        var result = Evaluate(profile, Evidence([recent, userState]), root);

        result.Disposition.Should().Be(CommunityRuleCandidateDisposition.PreviewOnly);
        result.Reasons.Should().Contain(CommunityRuleCandidateReason.NoEligibleStaleFiles);
        result.SkippedRecentFileCount.Should().Be(1);
        result.SkippedUnsupportedFileCount.Should().Be(1);
        result.EligibleFiles.Should().BeEmpty();
    }

    [Fact]
    public void Candidate_policy_source_has_no_operation_quarantine_or_mutation_authority()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Software", "CommunityRuleCandidatePolicy.cs"));

        source.Should().NotContain("OperationDescriptor");
        source.Should().NotContain("Quarantine");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("File.Move");
        source.Should().NotContain("Directory.Delete");
        source.Should().NotContain("RegistryKey");
        source.Should().NotContain("Process.Start");
    }

    private static CommunityRuleCandidateAssessment Evaluate(
        SoftwareProfile profile,
        CommunityRuleCacheEvidence evidence,
        string root) =>
        CommunityRuleCandidatePolicy.Evaluate(
            profile,
            evidence,
            [root],
            _ => true,
            _ => false,
            DateTimeOffset.UtcNow);

    private static CommunityRuleCacheEvidence Evidence(
        IReadOnlyList<CommunityRuleFileEvidence> files,
        string? warning = null,
        bool lowerBound = false,
        bool candidateFilesComplete = true,
        int registryTargetCount = 0,
        bool includesRemoveSelf = false) =>
        new()
        {
            RuleName = "Example Cache",
            RulePackSource = "Fixture",
            RulePackVersion = "1",
            RulePackSha256 = new string('A', 64),
            Warning = warning,
            FileCount = files.Count,
            SizeBytes = files.Sum(file => file.SizeBytes),
            StaleFileCount = files.Count(file => file.LastWriteTimeUtc <= DateTimeOffset.UtcNow.AddDays(-30)),
            StaleSizeBytes = files.Where(file => file.LastWriteTimeUtc <= DateTimeOffset.UtcNow.AddDays(-30)).Sum(file => file.SizeBytes),
            IsSizeLowerBound = lowerBound,
            CandidateFiles = files,
            CandidateFilesComplete = candidateFilesComplete,
            RegistryTargetCount = registryTargetCount,
            IncludesRemoveSelf = includesRemoveSelf
        };

    private static CommunityRuleFileEvidence CandidateFile(
        string path,
        long bytes,
        DateTimeOffset lastWriteTimeUtc) =>
        new()
        {
            Path = path,
            SizeBytes = bytes,
            LastWriteTimeUtc = lastWriteTimeUtc,
            Attributes = FileAttributes.Normal
        };

    private static DateTimeOffset Old() => DateTimeOffset.UtcNow.AddDays(-60);

    private static string Canonical(string path) => Path.GetFullPath(path);

    private static string FindRepositoryFile(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. segments]);
            if (File.Exists(candidate)) return candidate;
            current = current.Parent;
        }
        throw new FileNotFoundException(Path.Combine(segments));
    }
}
