using Css.Core.Apps;
using Css.Core.Software;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class CDriveApplicationOwnershipSummaryTests
{
    [Fact]
    public void Ownership_catalog_is_exhaustive_and_matches_the_c_drive_filter()
    {
        var profiles = MixedProfiles();

        var ownership = CDriveApplicationOwnershipCatalog.Create(profiles);
        var filtered = AppCatalogPresenter.Apply(
            profiles,
            new AppCatalogQuery
            {
                Filter = AppCatalogFilter.CDrive,
                Sort = AppCatalogSort.Name
            });

        ownership.AllProfiles.Should().HaveCount(3);
        ownership.OrdinaryProfiles.Should().ContainSingle(profile => profile.Name == "Ordinary");
        ownership.SystemProfiles.Should().ContainSingle(profile => profile.Name == "System");
        ownership.OwnershipPendingProfiles.Should().ContainSingle(profile => profile.Name == "Ownership Pending");
        (ownership.OrdinaryProfiles.Count
            + ownership.SystemProfiles.Count
            + ownership.OwnershipPendingProfiles.Count)
            .Should().Be(ownership.AllProfiles.Count);
        filtered.Should().BeEquivalentTo(ownership.AllProfiles);
    }

    [Fact]
    public void Existing_first_view_summaries_separate_protected_c_drive_evidence()
    {
        var profiles = MixedProfiles();
        var appSummary = AppCatalogSummaryPresenter.Create(profiles, visibleCount: profiles.Count);
        var digest = HealthDigestBuilder.Create(
            @"C:\",
            new ScanSnapshot(
                new DateTimeOffset(2026, 7, 16, 12, 0, 0, TimeSpan.Zero),
                []),
            EmptyHealth(),
            profiles);
        var visible = appSummary.Text + "\n" + digest.Summary;

        appSummary.CDriveFootprintCount.Should().Be(3);
        appSummary.OrdinaryCDriveFootprintCount.Should().Be(1);
        appSummary.SystemCDriveFootprintCount.Should().Be(1);
        appSummary.OwnershipPendingCDriveFootprintCount.Should().Be(1);
        appSummary.Text.Should().Contain("普通应用 1 个")
            .And.Contain("系统组件 1 个")
            .And.Contain("归属待确认 1 个")
            .And.Contain("仅供查看");
        digest.Summary.Should().Contain("C 盘应用线索")
            .And.Contain("普通应用 1 个")
            .And.Contain("系统组件 1 个")
            .And.Contain("归属待确认 1 个")
            .And.Contain("仅供查看")
            .And.NotContain("观察到写入 C 盘的应用 3 个");
        visible.Should().NotContain("Ordinary")
            .And.NotContain("System")
            .And.NotContain("Ownership Pending")
            .And.NotContain(@"C:\Program Files")
            .And.NotContain(@"C:\Windows")
            .And.NotContain(@"D:\Software");
    }

    [Fact]
    public void Protected_only_c_drive_evidence_never_looks_like_an_ordinary_action_list()
    {
        var profiles = MixedProfiles()
            .Where(profile => profile.Name != "Ordinary" && profile.Name != "D Clean")
            .ToArray();

        var appSummary = AppCatalogSummaryPresenter.Create(profiles, visibleCount: profiles.Length);
        var digest = HealthDigestBuilder.Create(
            @"C:\",
            new ScanSnapshot(
                new DateTimeOffset(2026, 7, 16, 12, 5, 0, TimeSpan.Zero),
                []),
            EmptyHealth(),
            profiles);

        appSummary.OrdinaryCDriveFootprintCount.Should().Be(0);
        appSummary.Text.Should().Contain("普通应用 0 个")
            .And.Contain("仅供查看");
        digest.Summary.Should().Contain("普通应用 0 个")
            .And.Contain("仅供查看");
        (appSummary.Text + digest.Summary).Should().NotContain("可迁移")
            .And.NotContain("可清理")
            .And.NotContain("建议处理");
    }

    private static IReadOnlyList<SoftwareProfile> MixedProfiles() =>
    [
        new SoftwareProfile
        {
            Name = "Ordinary",
            Category = SoftwareCategory.Normal,
            InstallPath = @"C:\Program Files\Ordinary"
        },
        new SoftwareProfile
        {
            Name = "System",
            Category = SoftwareCategory.SystemTool,
            InstallPath = @"C:\Windows\System32\System"
        },
        new SoftwareProfile
        {
            Name = "Ownership Pending",
            Category = SoftwareCategory.Unknown,
            InstallPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "SystemApps",
                "OwnershipPending")
        },
        new SoftwareProfile
        {
            Name = "D Clean",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\Clean\Install"
        }
    ];

    private static HealthCheckSummary EmptyHealth() =>
        new()
        {
            OverallScore = 80,
            Dimensions =
            [
                new HealthDimensionResult
                {
                    Name = "磁盘健康",
                    Result = "C 盘摘要可用",
                    Rating = "有优化空间"
                }
            ],
            KeyFindings = []
        };
}
