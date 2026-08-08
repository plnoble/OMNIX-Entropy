using Css.Core.Software;
using Css.Rules.Winapp2;
using FluentAssertions;

namespace Css.Tests;

public sealed class Winapp2RulePreferenceStoreTests
{
    [Fact]
    public async Task Ignore_and_restore_are_atomic_bounded_managed_preferences()
    {
        var root = TemporaryRoot();
        var path = Path.Combine(root, "preferences.json");
        var store = new Winapp2RulePreferenceStore(
            path,
            new FixedTimeProvider(DateTimeOffset.Parse("2026-08-01T01:02:03Z")));
        var evidence = Evidence("Example Cache", 'A');

        try
        {
            var ignored = await store.IgnoreAsync(evidence);
            var reloaded = store.Load();
            var restored = await store.RestoreAsync(ignored.IgnoredRules.Single().RuleKey);

            ignored.IsIgnored(evidence).Should().BeTrue();
            reloaded.IgnoredRules.Should().ContainSingle();
            reloaded.IgnoredRules[0].RuleName.Should().Be("Example Cache");
            reloaded.IgnoredRules[0].AddedAtUtc.Should().Be(DateTimeOffset.Parse("2026-08-01T01:02:03Z"));
            restored.IgnoredRules.Should().BeEmpty();
            Directory.GetFiles(root, "*.tmp-*", SearchOption.TopDirectoryOnly).Should().BeEmpty();
            ignored.IsExecutionAuthorized.Should().BeFalse();
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Missing_file_loads_empty_and_malformed_or_oversized_files_fail_closed()
    {
        var root = TemporaryRoot();
        var path = Path.Combine(root, "preferences.json");
        var store = new Winapp2RulePreferenceStore(path);

        try
        {
            store.Load().IgnoredRules.Should().BeEmpty();
            Directory.CreateDirectory(root);
            File.WriteAllText(path, "{bad json");
            var malformed = store.Load;
            malformed.Should().Throw<InvalidDataException>();
            File.WriteAllBytes(path, new byte[Winapp2RulePreferenceStore.MaximumFileBytes + 1]);
            var oversized = store.Load;
            oversized.Should().Throw<InvalidDataException>();
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Preference_key_changes_with_pack_hash_and_never_contains_rule_text()
    {
        var first = Winapp2RulePreferenceKey.Create(new string('A', 64), "Private Cache Name");
        var second = Winapp2RulePreferenceKey.Create(new string('B', 64), "Private Cache Name");

        first.Should().HaveLength(64)
            .And.NotContain("Private")
            .And.NotBe(second);
        first.All(Uri.IsHexDigit).Should().BeTrue();
    }

    [Fact]
    public void Preference_source_has_managed_persistence_but_no_maintenance_authority()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Rules", "Winapp2", "Winapp2RulePreferenceStore.cs"));

        source.Should().NotContain("OperationDescriptor")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("Process.Start")
            .And.NotContain("Registry.")
            .And.NotContain("HttpClient");
    }

    private static CommunityRuleCacheEvidence Evidence(string name, char hash) =>
        new()
        {
            RuleName = name,
            RulePackSource = "Fixture",
            RulePackVersion = "1",
            RulePackSha256 = new string(hash, 64),
            FileCount = 1,
            SizeBytes = 10
        };

    private static string TemporaryRoot() =>
        Path.Combine(Path.GetTempPath(), "omnix-rule-preferences-" + Guid.NewGuid().ToString("N"));

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path)) return path;
            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
