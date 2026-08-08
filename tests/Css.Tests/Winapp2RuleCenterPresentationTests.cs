using Css.Core.Software;
using Css.Rules.Winapp2;
using FluentAssertions;

namespace Css.Tests;

public sealed class Winapp2RuleCenterPresentationTests
{
    [Fact]
    public void Inactive_center_is_plain_language_and_non_executable()
    {
        var view = Winapp2RuleCenterPresenter.Create(
            status: null,
            profiles: [],
            preferences: Winapp2RulePreferences.Empty);

        view.Title.Should().Be("扩展规则");
        view.StatusHeadline.Should().Contain("未启用");
        view.StatusSummary.Should().Contain("不影响基础扫描");
        view.CanRollback.Should().BeFalse();
        view.PreviewRows.Should().BeEmpty();
        view.IsExecutionAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Active_center_ranks_findings_without_summing_overlapping_rules_or_leaking_paths()
    {
        var descriptor = Descriptor("v2", 'A');
        var status = new Winapp2RulePackStatus
        {
            ActiveDescriptor = descriptor,
            PreviousDescriptor = Descriptor("v1", 'B'),
            ActivatedAtUtc = DateTimeOffset.Parse("2026-08-01T01:02:03Z"),
            StatePath = @"D:\Managed\active-state.json",
            ActivePackPath = @"D:\Managed\packs\active.ini",
            PreviousPackPath = @"D:\Managed\packs\previous.ini"
        };
        var profiles = new[]
        {
            Profile("Small App", Evidence("Nested Cache", descriptor, 512L * 1024 * 1024)),
            Profile("Large App", Evidence("Broad Cache", descriptor, 2L * 1024 * 1024 * 1024)),
            Profile("Large App", Evidence("Nested Cache", descriptor, 1536L * 1024 * 1024))
        };

        var view = Winapp2RuleCenterPresenter.Create(
            status,
            profiles,
            Winapp2RulePreferences.Empty);
        var beginnerText = string.Join(
            "\n",
            view.StatusHeadline,
            view.StatusSummary,
            view.SourceSummary,
            view.LicenseSummary,
            view.PreviewSummary,
            string.Join("\n", view.PreviewRows.Select(row => row.VisibleText)));

        view.ActiveVersion.Should().Be("v2");
        view.CanRollback.Should().BeTrue();
        view.PreviewRows.Should().HaveCount(3);
        view.PreviewRows[0].SizeBytes.Should().Be(2L * 1024 * 1024 * 1024);
        view.PreviewSummary.Should().Contain("至少 2.0 GB")
            .And.Contain("3 条只读发现")
            .And.NotContain("4.0 GB");
        view.PreviewRows.Should().OnlyContain(row => !row.IsExecutionAuthorized);
        beginnerText.Should().NotContain(@"D:\Managed")
            .And.NotContain(@"C:\Users\Private")
            .And.NotContain(descriptor.ExpectedSha256);
        view.PreviewRows[0].TechnicalDetails.Should().Contain(line =>
            line.Contains(@"C:\Users\Private", StringComparison.Ordinal));
    }

    [Fact]
    public void Descriptor_builder_requires_https_hash_and_two_explicit_confirmations()
    {
        var pending = Input() with { UserAcceptedLicense = false };
        var invalidTransport = Input() with { SourceUriText = "http://example.invalid/rules.ini" };
        var accepted = Input();

        var pendingResult = Winapp2RuleActivationRequestBuilder.Build(pending);
        var invalidResult = Winapp2RuleActivationRequestBuilder.Build(invalidTransport);
        var acceptedResult = Winapp2RuleActivationRequestBuilder.Build(accepted);

        pendingResult.CanActivate.Should().BeFalse();
        pendingResult.MissingRequirements.Should().Contain(item => item.Contains("许可证"));
        invalidResult.CanActivate.Should().BeFalse();
        invalidResult.MissingRequirements.Should().Contain(item => item.Contains("HTTPS"));
        acceptedResult.CanActivate.Should().BeTrue();
        acceptedResult.Descriptor!.ExpectedSha256.Should().Be(new string('C', 64));
        acceptedResult.Consent!.ReviewedSha256.Should().Be(new string('C', 64));
        acceptedResult.Consent.UserConfirmedActivation.Should().BeTrue();
        acceptedResult.Consent.UserAcceptedLicense.Should().BeTrue();
    }

    [Fact]
    public void Preview_rows_surface_candidate_decisions_without_enabling_execution()
    {
        var descriptor = Descriptor("v3", 'D');
        var evidence = Evidence("Old Cache", descriptor, 4096);
        evidence = new CommunityRuleCacheEvidence(evidence)
        {
            CandidateAssessment = new CommunityRuleCandidateAssessment
            {
                Disposition = CommunityRuleCandidateDisposition.EligibleForSafePreview,
                Summary = "2 个旧缓存文件通过第一轮筛选，可进入安全预演。",
                Explanation = "仍需隔离回滚。",
                Reasons = [CommunityRuleCandidateReason.EligibleStaleCacheFiles],
                EligibleFiles = [],
                EligibleBytes = 4096
            }
        };
        var view = Winapp2RuleCenterPresenter.Create(
            new Winapp2RulePackStatus
            {
                ActiveDescriptor = descriptor,
                ActivatedAtUtc = DateTimeOffset.UtcNow,
                StatePath = @"D:\Managed\state.json",
                ActivePackPath = @"D:\Managed\active.ini"
            },
            [Profile("Example", evidence)],
            Winapp2RulePreferences.Empty);

        view.PreviewSummary.Should().Contain("1 条通过第一轮筛选")
            .And.Contain("不会直接清理");
        view.PreviewRows.Should().ContainSingle()
            .Which.VisibleText.Should().Contain("可进入安全预演");
        view.PreviewRows.Should().OnlyContain(row => !row.IsExecutionAuthorized);
    }

    private static Winapp2RuleActivationInput Input() =>
        new()
        {
            SourceName = "Community rules",
            SourceUriText = "https://example.invalid/rules.ini",
            Version = "2026-08",
            LicenseName = "CC BY-SA 4.0",
            LicenseUriText = "https://example.invalid/license",
            Sha256 = new string('C', 64),
            UserConfirmedActivation = true,
            UserAcceptedLicense = true
        };

    private static SoftwareProfile Profile(string name, CommunityRuleCacheEvidence evidence) =>
        new()
        {
            Name = name,
            CommunityCacheEvidence = [evidence]
        };

    private static CommunityRuleCacheEvidence Evidence(
        string ruleName,
        Winapp2RulePackDescriptor descriptor,
        long bytes) =>
        new()
        {
            RuleName = ruleName,
            RulePackSource = descriptor.SourceName,
            RulePackVersion = descriptor.Version,
            RulePackSha256 = descriptor.ExpectedSha256,
            FileCount = 2,
            SizeBytes = bytes,
            SamplePaths = [@"C:\Users\Private\Cache\item.tmp"]
        };

    private static Winapp2RulePackDescriptor Descriptor(string version, char hash) =>
        new()
        {
            SourceName = "Fixture community rules",
            SourceUri = new Uri("https://example.invalid/rules.ini"),
            Version = version,
            LicenseName = "Fixture license",
            LicenseUri = new Uri("https://example.invalid/license"),
            ExpectedSha256 = new string(hash, 64)
        };
}
