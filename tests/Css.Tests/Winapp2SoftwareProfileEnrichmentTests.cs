using System.Security.Cryptography;
using System.Text;
using Css.Core.Software;
using Css.Rules.Winapp2;
using Css.Scanner.Winapp2;
using FluentAssertions;

namespace Css.Tests;

public sealed class Winapp2SoftwareProfileEnrichmentTests
{
    [Fact]
    public void Enricher_adds_bounded_rule_evidence_without_promoting_samples_to_cleanup_paths()
    {
        var root = TemporaryRoot();
        var cache = Path.Combine(root, "Cache");
        Directory.CreateDirectory(cache);
        var old = DateTimeOffset.UtcNow.AddDays(-45);
        Write(Path.Combine(cache, "old.tmp"), 40, old);
        Write(Path.Combine(cache, "new.tmp"), 60, DateTimeOffset.UtcNow);
        const string content = "[Example Cache]\nFileKey1=%PROFILE%\\Cache|*.tmp|RECURSE\n";
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            DisplayVersion = "2.0",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"D:\Software\Example\uninstall.exe",
            DataPaths = [root],
            Services = ["ExampleService"]
        };
        var unrelated = new SoftwareProfile
        {
            Name = "Other App",
            DataPaths = [Path.Combine(root, "Other")]
        };

        try
        {
            var result = new Winapp2SoftwareProfileEnricher().Enrich(
                [profile, unrelated],
                Catalog(content),
                new Winapp2SoftwareProfileEnrichmentOptions
                {
                    ApprovedUserDataRoots = [root],
                    ResolverOptions = new Winapp2EvidenceResolverOptions
                    {
                        StaleAge = TimeSpan.FromDays(30),
                        MaxSamplePaths = 1
                    }
                },
                value => value.Replace("%PROFILE%", root, StringComparison.OrdinalIgnoreCase));

            var enriched = result.Profiles[0];
            enriched.Should().NotBeSameAs(profile);
            enriched.Publisher.Should().Be(profile.Publisher);
            enriched.DisplayVersion.Should().Be(profile.DisplayVersion);
            enriched.UninstallCommand.Should().Be(profile.UninstallCommand);
            enriched.Services.Should().Equal(profile.Services);
            enriched.CachePaths.Should().BeEmpty("community evidence is not cleanup authority");
            enriched.CacheSizeBytes.Should().Be(0);
            var evidence = enriched.CommunityCacheEvidence.Should().ContainSingle().Subject;
            evidence.RuleName.Should().Be("Example Cache");
            evidence.FileCount.Should().Be(2);
            evidence.SizeBytes.Should().Be(100);
            evidence.StaleFileCount.Should().Be(1);
            evidence.StaleSizeBytes.Should().Be(40);
            evidence.SamplePaths.Should().ContainSingle();
            evidence.CandidateFiles.Should().HaveCount(2);
            evidence.CandidateFilesComplete.Should().BeTrue();
            evidence.CandidateAssessment.Should().NotBeNull();
            evidence.CandidateAssessment!.Disposition.Should().Be(
                CommunityRuleCandidateDisposition.EligibleForSafePreview);
            evidence.CandidateAssessment.EligibleFiles.Should().ContainSingle()
                .Which.Path.Should().EndWith("old.tmp");
            evidence.IsExecutionAuthorized.Should().BeFalse();
            result.Profiles[1].Should().BeSameAs(unrelated);
            result.EnrichedProfileCount.Should().Be(1);
            result.RuleEvidenceCount.Should().Be(1);
            result.IsExecutionAuthorized.Should().BeFalse();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Enricher_reports_rule_limits_and_propagates_cancellation()
    {
        var root = TemporaryRoot();
        Directory.CreateDirectory(Path.Combine(root, "Cache"));
        Write(Path.Combine(root, "Cache", "one.tmp"), 1, DateTimeOffset.UtcNow);
        const string content = """
            [First Cache]
            FileKey1=%PROFILE%\Cache|*.tmp

            [Second Cache]
            FileKey1=%PROFILE%\Cache|*.tmp
            """;
        var profile = new SoftwareProfile { Name = "Example App", DataPaths = [root] };
        var enricher = new Winapp2SoftwareProfileEnricher();

        try
        {
            var limited = enricher.Enrich(
                [profile],
                Catalog(content),
                new Winapp2SoftwareProfileEnrichmentOptions { MaxRulesPerProfile = 1 },
                value => value.Replace("%PROFILE%", root, StringComparison.OrdinalIgnoreCase));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var cancelled = () => enricher.Enrich(
                [profile],
                Catalog(content),
                expandVariables: value => value.Replace("%PROFILE%", root, StringComparison.OrdinalIgnoreCase),
                cancellationToken: cancellation.Token);

            limited.RuleEvidenceCount.Should().Be(1);
            limited.IsComplete.Should().BeFalse();
            limited.LimitReasons.Should().Contain(Winapp2SoftwareProfileEnrichmentLimit.RulesPerProfile);
            cancelled.Should().Throw<OperationCanceledException>();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Enricher_skips_managed_ignored_rule_keys_without_creating_cleanup_paths()
    {
        var root = TemporaryRoot();
        var cache = Path.Combine(root, "Cache");
        Directory.CreateDirectory(cache);
        Write(Path.Combine(cache, "one.tmp"), 10, DateTimeOffset.UtcNow);
        const string content = "[Ignored Cache]\nFileKey1=%PROFILE%\\Cache|*.tmp\n";
        var catalog = Catalog(content);
        var profile = new SoftwareProfile { Name = "Example App", DataPaths = [root] };
        var key = Winapp2RulePreferenceKey.Create(
            catalog.Descriptor.ExpectedSha256,
            "Ignored Cache");

        try
        {
            var result = new Winapp2SoftwareProfileEnricher().Enrich(
                [profile],
                catalog,
                new Winapp2SoftwareProfileEnrichmentOptions
                {
                    IgnoredRuleKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { key }
                },
                value => value.Replace("%PROFILE%", root, StringComparison.OrdinalIgnoreCase));

            result.RuleEvidenceCount.Should().Be(0);
            result.IgnoredRuleCount.Should().Be(1);
            result.Profiles[0].CommunityCacheEvidence.Should().BeEmpty();
            result.Profiles[0].CachePaths.Should().BeEmpty();
            result.IsExecutionAuthorized.Should().BeFalse();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static Winapp2RuleCatalog Catalog(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new Winapp2RuleCatalogLoader().Load(
            new MemoryStream(bytes),
            new Winapp2RulePackDescriptor
            {
                SourceName = "Fixture pack",
                SourceUri = new Uri("https://example.invalid/winapp2.ini"),
                Version = "fixture-v1",
                LicenseName = "Fixture-only",
                LicenseUri = new Uri("https://example.invalid/license"),
                ExpectedSha256 = Convert.ToHexString(SHA256.HashData(bytes))
            });
    }

    private static void Write(string path, int length, DateTimeOffset lastWrite)
    {
        File.WriteAllBytes(path, new byte[length]);
        File.SetLastWriteTimeUtc(path, lastWrite.UtcDateTime);
    }

    private static string TemporaryRoot() =>
        Path.Combine(Path.GetTempPath(), "omnix-profile-rules-" + Guid.NewGuid().ToString("N"));
}
