using System;
using System.IO;
using System.Linq;
using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class PersonalStorageAnalyzerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 14, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Analysis_only_includes_real_files_inside_explicit_personal_roots()
    {
        var downloads = DirectoryNode("Downloads", @"C:\Users\Fixture\Downloads");
        downloads.Children.Add(FileNode(
            "old-video.mp4",
            @"C:\Users\Fixture\Downloads\old-video.mp4",
            700L * 1024 * 1024,
            Now.UtcDateTime.AddDays(-240)));
        downloads.Children.Add(FileNode(
            "recent-video.mp4",
            @"C:\Users\Fixture\Downloads\recent-video.mp4",
            800L * 1024 * 1024,
            Now.UtcDateTime.AddDays(-2)));
        downloads.Children.Add(new CategoryNode
        {
            Name = "large-folder",
            Path = @"C:\Users\Fixture\Downloads\large-folder",
            SizeBytes = 5L * 1024 * 1024 * 1024,
            LastWriteUtc = Now.UtcDateTime.AddDays(-400),
            IsFile = false
        });

        var appData = DirectoryNode("AppData", @"C:\Users\Fixture\AppData");
        appData.Children.Add(FileNode(
            "outside.bin",
            @"C:\Users\Fixture\AppData\outside.bin",
            900L * 1024 * 1024,
            Now.UtcDateTime.AddDays(-300)));

        var result = Result(downloads, appData);
        var analysis = PersonalStorageAnalyzer.Analyze(
            result,
            [@"C:\Users\Fixture\Downloads"],
            Now);

        analysis.Findings
            .Where(item => item.Kind == PersonalStorageFindingKind.LongUnusedLargeFile)
            .Should().ContainSingle(item => item.DisplayName == "old-video.mp4");
        analysis.Findings.Should().NotContain(item => item.DisplayName == "recent-video.mp4");
        analysis.Findings.Should().NotContain(item => item.DisplayName == "large-folder");
        analysis.Findings.Should().NotContain(item => item.DisplayName == "outside.bin");
        analysis.EligibleFileCount.Should().Be(2);
    }

    [Fact]
    public void Duplicate_candidates_require_same_name_and_exact_size()
    {
        var downloads = DirectoryNode("Downloads", @"C:\Users\Fixture\Downloads");
        downloads.Children.Add(FileNode(
            "archive.zip",
            @"C:\Users\Fixture\Downloads\A\archive.zip",
            100L * 1024 * 1024,
            null));
        downloads.Children.Add(FileNode(
            "archive.zip",
            @"C:\Users\Fixture\Downloads\B\archive.zip",
            100L * 1024 * 1024,
            Now.UtcDateTime.AddDays(-10)));
        downloads.Children.Add(FileNode(
            "renamed.zip",
            @"C:\Users\Fixture\Downloads\C\renamed.zip",
            100L * 1024 * 1024,
            Now.UtcDateTime.AddDays(-10)));
        downloads.Children.Add(FileNode(
            "archive.zip",
            @"C:\Users\Fixture\Downloads\D\archive.zip",
            101L * 1024 * 1024,
            Now.UtcDateTime.AddDays(-10)));

        var analysis = PersonalStorageAnalyzer.Analyze(
            Result(downloads),
            [@"C:\Users\Fixture\Downloads"],
            Now);
        var duplicate = analysis.Findings.Single(
            item => item.Kind == PersonalStorageFindingKind.PossibleDuplicateGroup);

        duplicate.DisplayName.Should().Be("archive.zip");
        duplicate.ItemCount.Should().Be(2);
        duplicate.CandidateBytes.Should().Be(100L * 1024 * 1024);
        duplicate.EvidencePaths.Should().HaveCount(2);
        duplicate.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Analysis_is_bounded_and_reports_truncation()
    {
        var downloads = DirectoryNode("Downloads", @"C:\Users\Fixture\Downloads");
        downloads.Children.Add(FileNode(
            "one.bin",
            @"C:\Users\Fixture\Downloads\one.bin",
            100L * 1024 * 1024,
            null));
        downloads.Children.Add(FileNode(
            "two.bin",
            @"C:\Users\Fixture\Downloads\two.bin",
            100L * 1024 * 1024,
            null));

        var analysis = PersonalStorageAnalyzer.Analyze(
            Result(downloads),
            [@"C:\Users\Fixture\Downloads"],
            Now,
            new PersonalStorageAnalysisOptions
            {
                MaximumVisitedNodes = 2
            });

        analysis.WasTruncated.Should().BeTrue();
        analysis.VisitedNodeCount.Should().Be(2);
    }

    [Fact]
    public void Beginner_presentations_are_path_free_and_never_executable()
    {
        var analysis = new PersonalStorageAnalysis
        {
            Findings =
            [
                new PersonalStorageFinding
                {
                    Kind = PersonalStorageFindingKind.LongUnusedLargeFile,
                    DisplayName = "video.mp4",
                    ItemCount = 1,
                    ItemSizeBytes = 700L * 1024 * 1024,
                    CandidateBytes = 700L * 1024 * 1024,
                    EvidencePaths = [@"C:\Users\Fixture\Downloads\video.mp4"]
                },
                new PersonalStorageFinding
                {
                    Kind = PersonalStorageFindingKind.PossibleDuplicateGroup,
                    DisplayName = "archive.zip",
                    ItemCount = 2,
                    ItemSizeBytes = 100L * 1024 * 1024,
                    CandidateBytes = 100L * 1024 * 1024,
                    EvidencePaths =
                    [
                        @"C:\Users\Fixture\Downloads\A\archive.zip",
                        @"C:\Users\Fixture\Downloads\B\archive.zip"
                    ]
                }
            ]
        };

        var presentation = PersonalStorageFindingPresenter.Create(analysis);
        var visibleText = string.Join(
            "\n",
            presentation.Items.SelectMany(item =>
                new[] { item.Title, item.Summary, item.AgentSuggestion, item.SafetyText }));
        var summary = HealthCheckSummaryBuilder.Build(
            new DriveScanResult { Drive = @"C:\", TotalBytes = 100, FreeBytes = 40 },
            [],
            [],
            analysis);
        var personalFindings = summary.KeyFindings
            .Where(item => item.Kind == HealthFindingKind.PersonalStorage)
            .ToArray();

        visibleText.Should().Contain("疑似").And.NotContain(@"C:\");
        presentation.Items.Should().OnlyContain(item => !item.CanExecuteDirectly);
        personalFindings.Should().HaveCount(2);
        personalFindings.Should().OnlyContain(item => !item.Text.Contains(@"C:\"));
        personalFindings.Should().OnlyContain(item => item.Action == RecommendationAction.Observe);
        HealthFindingActionPlanBuilder.Create(personalFindings[1])
            .CanExecuteDirectly.Should().BeFalse();

        var response = HomeAgentResponsePresenter.ShowDetails(personalFindings[0]);
        response.NavigationDestination.Should().Be(HomeAgentNavigationDestination.CDrivePersonalStorage);
        response.NavigationLabel.Should().Contain("个人文件");
        response.CanNavigate.Should().BeTrue();
        response.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Wpf_wiring_keeps_personal_storage_read_only_and_automation_visible()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var analyzer = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Scanner", "Disk", "PersonalStorageAnalysis.cs"));

        xaml.Should().Contain("x:Name=\"PersonalStorageSummaryTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"PersonalStorageSummaryTextBlock\"");
        xaml.Should().Contain("x:Name=\"PersonalStorageFindingsListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"PersonalStorageFindingsListBox\"");
        xaml.Should().Contain("StringFormat=PersonalStorageFinding_{0}");
        xaml.Should().Contain("Value=\"{Binding Title}\"");
        xaml.IndexOf("x:Name=\"GrowthListBox\"", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("x:Name=\"PersonalStorageSummaryTextBlock\"", StringComparison.Ordinal));
        xaml.Split("AutomationProperties.AutomationId=\"PersonalStorageSummaryTextBlock\"")
            .Should().HaveCount(2);
        xaml.Split("AutomationProperties.AutomationId=\"PersonalStorageFindingsListBox\"")
            .Should().HaveCount(2);

        main.Should().Contain("PersonalStorageFindingPresenter.Create(session.PersonalStorage)");
        main.Should().Contain("PersonalStorageFindingsListBox.ItemsSource = personalStorage.Items");
        main.Should().Contain("session.PersonalStorage);");
        main.Should().Contain("ResolvePersonalStorageFixtureRoot()");
        main.Should().Contain("MinimumLargeFileBytes = 8 * 1024");
        main.Should().Contain("MinimumDuplicateFileBytes = 4 * 1024");
        main.Should().Contain("HomeAgentNavigationDestination.CDrivePersonalStorage => \"CDrive\"");
        main.Should().Contain("PersonalStorageFindingsListBox.BringIntoView()");
        main.Should().NotContain("PersonalStorageSummaryTextBlock.BringIntoView()");

        analyzer.Should().Contain("MaximumVisitedNodes").And.Contain("MaximumDuplicateGroups");
        analyzer.Should().NotContain("File.Read");
        analyzer.Should().NotContain("File.Delete");
        analyzer.Should().NotContain("Directory.Delete");
        analyzer.Should().NotContain("Process.Start");
        analyzer.Should().NotContain("OperationDescriptor");
        analyzer.Should().NotContain("SafetyOperationPipeline");
    }

    [Fact]
    public void Personal_storage_gui_smoke_is_fixture_only_read_only_and_exactly_navigated()
    {
        var smoke = File.ReadAllText(FindRepositoryFile(
            ".omx", "gui-personal-storage-candidates-smoke.ps1"));

        smoke.Should().Contain("OMNIX_ENTROPY_CDRIVE_SCAN_ROOT")
            .And.Contain("OMNIX_ENTROPY_PERSONAL_STORAGE_ROOT")
            .And.Contain("OMNIX-PersonalStorage-Smoke-")
            .And.Contain("Assert-ConfinedPath")
            .And.Contain("PersonalStorageSummaryTextBlock")
            .And.Contain("PersonalStorageFinding_")
            .And.Contain("HomeAgentResponseNavigateButton")
            .And.Contain("Current.IsOffscreen")
            .And.Contain("Get-DescendantText $window")
            .And.Contain("noOperationExecuted = $true")
            .And.Contain("Save-WindowScreenshot");
        smoke.Should().NotContain("SafetyOperationPipeline")
            .And.NotContain("CleanupConfirmationConfirmButton")
            .And.NotContain("File.Delete")
            .And.NotContain("Process.Start(");
    }

    private static CategoryNode DirectoryNode(string name, string path) =>
        new()
        {
            Name = name,
            Path = path,
            IsFile = false
        };

    private static CategoryNode FileNode(
        string name,
        string path,
        long size,
        DateTime? lastWriteUtc) =>
        new()
        {
            Name = name,
            Path = path,
            IsFile = true,
            SizeBytes = size,
            LastWriteUtc = lastWriteUtc
        };

    private static DriveScanResult Result(params CategoryNode[] topLevel) =>
        new()
        {
            Drive = @"C:\",
            TotalBytes = 100L * 1024 * 1024 * 1024,
            FreeBytes = 40L * 1024 * 1024 * 1024,
            TopLevel = [.. topLevel]
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
