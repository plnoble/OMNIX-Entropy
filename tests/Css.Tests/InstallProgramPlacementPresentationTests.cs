using Css.Core.Software;
using Css.InstallGuard.Installers;
using FluentAssertions;

namespace Css.Tests;

public class InstallProgramPlacementPresentationTests
{
    [Fact]
    public void D_drive_main_program_and_owned_c_drive_data_are_explained_as_separate_facts()
    {
        var report = BuildReport(
            Profile(
                "Marvis",
                @"D:\Software\Marvis",
                [@"C:\Users\Fixture\AppData\Local\Marvis\Cache"]));

        var view = InstallSnapshotDiffPresenter.Create(report);
        var agent = InstallSnapshotDiffAgentPresenter.Create(report);
        var software = view.Cards.Single(card => card.Title.Contains("装了什么"));
        var cDrive = view.Cards.Single(card => card.Title.Contains("C 盘"));

        software.Body.Should().Contain("主程序装在 D 盘");
        cDrive.Body.Should().Contain("主程序在 D 盘")
            .And.Contain("1 个 C 盘数据或写入线索");
        cDrive.Detail.Should().Contain("不需要重复迁移主程序");
        agent.Headline.Should().Contain("主程序装在 D 盘").And.Contain("C 盘");
        agent.WhatThisMeans.Should().Contain("1 个").And.Contain("不代表主程序装错位置");
        agent.NextSteps.Should().Contain(step => step.Contains("不要重复迁移主程序"));
        Visible(view, agent).Should().NotContain(@"D:\Software\Marvis")
            .And.NotContain(@"C:\Users\Fixture");
        agent.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void C_drive_main_program_is_distinguished_from_one_external_data_location()
    {
        var report = BuildReport(
            Profile(
                "New Tool",
                @"C:\Program Files\New Tool",
                [
                    @"C:\Program Files\New Tool",
                    @"C:\Users\Fixture\AppData\Local\New Tool\Data",
                    @"c:\users\fixture\appdata\local\new tool\data"
                ]));

        var view = InstallSnapshotDiffPresenter.Create(report);
        var agent = InstallSnapshotDiffAgentPresenter.Create(report);
        var software = view.Cards.Single(card => card.Title.Contains("装了什么"));
        var cDrive = view.Cards.Single(card => card.Title.Contains("C 盘"));

        software.Body.Should().Contain("主程序装在 C 盘");
        cDrive.Body.Should().Contain("主程序在 C 盘")
            .And.Contain("安装目录之外 1 个 C 盘数据或写入线索");
        agent.WhatThisMeans.Should().Contain("主程序本身在 C 盘")
            .And.Contain("安装目录之外 1 个");
        Visible(view, agent).Should().NotContain(@"C:\Program Files")
            .And.NotContain(@"C:\Users\Fixture");
    }

    [Fact]
    public void Footprint_only_c_drive_change_is_not_attributed_to_the_unique_d_drive_app()
    {
        var report = BuildReport(
            Profile("New Tool", @"D:\Software\New Tool"),
            afterFootprint: [@"C:\Unrelated\ConcurrentChange"]);

        var view = InstallSnapshotDiffPresenter.Create(report);
        var agent = InstallSnapshotDiffAgentPresenter.Create(report);
        var cDrive = view.Cards.Single(card => card.Title.Contains("C 盘"));

        cDrive.Body.Should().Contain("主程序在 D 盘")
            .And.Contain("1 个同期 C 盘变化候选")
            .And.Contain("不能确认属于这个软件");
        agent.WhatThisMeans.Should().Contain("不能确认属于这个软件");
        agent.NextSteps.Should().Contain(step => step.Contains("不要把同期变化当成这个软件的数据"));
        Visible(view, agent).Should().NotContain(@"C:\Unrelated");
    }

    [Fact]
    public void Missing_unique_software_keeps_main_program_location_unknown()
    {
        var report = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 15, 8, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 15, 8, 5, 0, TimeSpan.Zero),
            NewCDrivePaths = [@"C:\Users\Fixture\AppData\Local\Mystery\Cache"],
            HasCDriveWrites = true,
            Summary = "fixture"
        };

        var view = InstallSnapshotDiffPresenter.Create(report);
        var agent = InstallSnapshotDiffAgentPresenter.Create(report);
        var software = view.Cards.Single(card => card.Title.Contains("装了什么"));
        var cDrive = view.Cards.Single(card => card.Title.Contains("C 盘"));

        software.Body.Should().Contain("没有确认新增软件")
            .And.Contain("主程序装到哪里");
        cDrive.Body.Should().Contain("不能归到某个主程序");
        agent.WhatThisMeans.Should().Contain("没有唯一新增软件")
            .And.Contain("不能判断主程序装在 C 盘还是 D 盘");
        Visible(view, agent).Should().NotContain(@"C:\Users\Fixture");
        agent.CanExecuteDirectly.Should().BeFalse();
    }

    private static InstallSnapshotDiffReport BuildReport(
        SoftwareProfile profile,
        IReadOnlyList<string>? afterFootprint = null)
    {
        var before = new InstallSystemSnapshot(
            new DateTimeOffset(2026, 7, 15, 8, 0, 0, TimeSpan.Zero),
            [],
            InstallFootprintCapture.EmptyComplete);
        var after = new InstallSystemSnapshot(
            new DateTimeOffset(2026, 7, 15, 8, 5, 0, TimeSpan.Zero),
            [profile],
            new InstallFootprintCapture
            {
                Status = InstallFootprintCaptureStatus.Complete,
                Paths = afterFootprint ?? []
            });
        return InstallSnapshotDiffBuilder.Build(before, after);
    }

    private static SoftwareProfile Profile(
        string name,
        string installPath,
        IReadOnlyList<string>? cDriveWritePaths = null) =>
        new()
        {
            Name = name,
            InstallPath = installPath,
            CDriveWritePaths = cDriveWritePaths ?? []
        };

    private static string Visible(
        InstallSnapshotDiffViewModel view,
        InstallSnapshotDiffAgentViewModel agent) =>
        string.Join("\n", view.Cards.SelectMany(card =>
            new[] { card.Title, card.Body, card.Detail }))
        + "\n"
        + string.Join("\n", new[]
        {
            agent.Title,
            agent.Headline,
            agent.WhatThisMeans,
            agent.SafetyBoundary
        }.Concat(agent.NextSteps));
}
