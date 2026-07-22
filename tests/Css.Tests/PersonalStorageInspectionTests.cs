using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Css.App;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class PersonalStorageInspectionTests
{
    [Fact]
    public void Presenter_keeps_exact_evidence_out_of_the_default_beginner_text()
    {
        var source = Finding(
            PersonalStorageFindingKind.PossibleDuplicateGroup,
            @"C:\Users\Fixture\Downloads\A\archive.zip",
            @"C:\Users\Fixture\Downloads\B\archive.zip");

        var item = PersonalStorageFindingPresenter.Create(new PersonalStorageAnalysis
        {
            Findings = [source]
        }).Items.Single();

        item.EvidencePaths.Should().Equal(source.EvidencePaths);
        item.CanInspectLocations.Should().BeTrue();
        string.Join("\n", item.Title, item.Summary, item.AgentSuggestion, item.SafetyText)
            .Should().NotContain(@"C:\")
            .And.NotContain("Downloads");
        item.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Detail_window_returns_selection_only_and_has_no_launch_or_mutation_authority()
    {
        var item = PersonalStorageFindingPresenter.Create(new PersonalStorageAnalysis
        {
            Findings =
            [
                Finding(
                    PersonalStorageFindingKind.PossibleDuplicateGroup,
                    @"C:\Users\Fixture\Downloads\A\archive.zip",
                    @"C:\Users\Fixture\Downloads\B\archive.zip")
            ]
        }).Items.Single();

        var requested = RunSta(() =>
        {
            var window = new PersonalStorageInspectionWindow(item);
            var list = Find<ListBox>(window, "PersonalStorageInspectionPathsListBox");
            var button = Find<Button>(window, "PersonalStorageInspectionOpenButton");
            list.SelectedIndex = 1;
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            return window.RequestedEvidencePath;
        });

        requested.Should().Be(item.EvidencePaths[1]);

        var xaml = Read("src", "Css.App", "PersonalStorageInspectionWindow.xaml");
        var code = Read("src", "Css.App", "PersonalStorageInspectionWindow.xaml.cs");
        xaml.Should().Contain(
                "AutomationProperties.AutomationId=\"PersonalStorageInspectionPathsListBox\"")
            .And.Contain(
                "AutomationProperties.AutomationId=\"PersonalStorageInspectionOpenButton\"")
            .And.Contain(
                "AutomationProperties.AutomationId=\"PersonalStorageInspectionSafetyTextBlock\"")
            .And.NotContain("删除")
            .And.NotContain("移动");
        code.Should().Contain("RequestedEvidencePath")
            .And.NotContain("Process.Start")
            .And.NotContain("File.Delete")
            .And.NotContain("File.Move")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("Quarantine");
    }

    [Fact]
    public void Explorer_launcher_accepts_only_current_local_existing_evidence()
    {
        const string selected = @"C:\Users\Fixture\Downloads\video.mp4";
        const string explorer = @"C:\Windows\explorer.exe";
        ProcessStartInfo? observed = null;
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            selected,
            explorer
        };
        var launcher = new PersonalStorageExplorerLauncher(
            @"C:\Windows",
            existing.Contains,
            startInfo => observed = startInfo);

        var opened = launcher.TryOpenSelectedLocation(selected, [selected]);

        opened.Opened.Should().BeTrue();
        opened.Message.Should().NotContain(@"C:\");
        observed.Should().NotBeNull();
        observed!.FileName.Should().Be(explorer);
        observed.UseShellExecute.Should().BeTrue();
        observed.ArgumentList.Should().Equal("/select," + selected);

        observed = null;
        launcher.TryOpenSelectedLocation(
                @"C:\Users\Fixture\Downloads\other.mp4",
                [selected])
            .Opened.Should().BeFalse();
        launcher.TryOpenSelectedLocation(@"\\server\share\video.mp4", [selected])
            .Opened.Should().BeFalse();
        launcher.TryOpenSelectedLocation("video.mp4", [selected])
            .Opened.Should().BeFalse();
        launcher.TryOpenSelectedLocation(
                @"C:\Users\Fixture\Downloads\video.mp4:stream",
                [@"C:\Users\Fixture\Downloads\video.mp4:stream"])
            .Opened.Should().BeFalse();
        observed.Should().BeNull();

        var missingFileLauncher = new PersonalStorageExplorerLauncher(
            @"C:\Windows",
            path => string.Equals(path, explorer, StringComparison.OrdinalIgnoreCase),
            startInfo => observed = startInfo);
        missingFileLauncher.TryOpenSelectedLocation(selected, [selected])
            .Opened.Should().BeFalse();
        observed.Should().BeNull();
    }

    [Fact]
    public void Main_window_wires_one_explicit_read_only_inspection_action()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var handler = SourceMethodExtractor.Extract(
            code,
            "private void InspectPersonalStorageFinding_Click(object sender, RoutedEventArgs e)");

        xaml.Should().Contain("Content=\"查看位置\"")
            .And.Contain("Click=\"InspectPersonalStorageFinding_Click\"")
            .And.Contain("StringFormat=PersonalStorageInspect_{0}");
        handler.Should().Contain("PersonalStorageInspectionWindow")
            .And.Contain("RequestedEvidencePath")
            .And.Contain("_personalStorageEvidencePaths")
            .And.Contain("TryOpenSelectedLocation")
            .And.NotContain("File.Delete")
            .And.NotContain("File.Move")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("Quarantine");
    }

    [Fact]
    public void Detail_window_renders_locations_action_and_safety_in_the_first_view()
    {
        var item = PersonalStorageFindingPresenter.Create(new PersonalStorageAnalysis
        {
            Findings =
            [
                Finding(
                    PersonalStorageFindingKind.PossibleDuplicateGroup,
                    @"C:\Users\Fixture\Downloads\A\archive.zip",
                    @"C:\Users\Fixture\Documents\Backup\archive.zip")
            ]
        }).Items.Single();

        var result = RunSta(() =>
        {
            var window = new PersonalStorageInspectionWindow(item);
            window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            var elements = new[]
            {
                Find<FrameworkElement>(window, "PersonalStorageInspectionTitleTextBlock"),
                Find<FrameworkElement>(window, "PersonalStorageInspectionConclusionTextBlock"),
                Find<FrameworkElement>(window, "PersonalStorageInspectionPathsListBox"),
                Find<FrameworkElement>(window, "PersonalStorageInspectionSafetyTextBlock"),
                Find<FrameworkElement>(window, "PersonalStorageInspectionOpenButton"),
                Find<FrameworkElement>(window, "PersonalStorageInspectionCloseButton")
            };
            var png = Render(window);
            PersistEvidenceIfRequested(png);
            var capture = (
                PngLength: png.Length,
                Sha256: Convert.ToHexString(SHA256.HashData(png)),
                AllVisible: elements.All(element => IsVisible(element, window)));
            window.Close();
            return capture;
        });

        result.PngLength.Should().BeGreaterThan(10_000);
        result.Sha256.Should().MatchRegex("^[0-9A-F]{64}$");
        result.AllVisible.Should().BeTrue();
    }

    private static PersonalStorageFinding Finding(
        PersonalStorageFindingKind kind,
        params string[] paths) =>
        new()
        {
            Kind = kind,
            DisplayName = Path.GetFileName(paths[0]),
            ItemCount = paths.Length,
            ItemSizeBytes = 100L * 1024 * 1024,
            CandidateBytes = 100L * 1024 * 1024,
            EvidencePaths = paths
        };

    private static T Find<T>(Window window, string name) where T : FrameworkElement =>
        (T)(window.FindName(name)
            ?? throw new InvalidOperationException($"Missing control {name}."));

    private static byte[] Render(Window window)
    {
        var width = Math.Max(1, (int)Math.Ceiling(window.ActualWidth));
        var height = Math.Max(1, (int)Math.Ceiling(window.ActualHeight));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            context.DrawRectangle(
                new VisualBrush(window)
                {
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top
                },
                null,
                new Rect(0, 0, width, height));
        }
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    private static bool IsVisible(FrameworkElement element, Window window)
    {
        if (!element.IsVisible || element.ActualWidth <= 0 || element.ActualHeight <= 0)
            return false;
        var point = element.TransformToAncestor(window).Transform(new Point(0, 0));
        return point.X >= 0
            && point.Y >= 0
            && point.X + element.ActualWidth <= window.ActualWidth + 1
            && point.Y + element.ActualHeight <= window.ActualHeight + 1;
    }

    private static void PersistEvidenceIfRequested(byte[] png)
    {
        var requested = Environment.GetEnvironmentVariable(
            "OMNIX_PERSONAL_STORAGE_INSPECTION_RENDER_OUTPUT");
        if (string.IsNullOrWhiteSpace(requested))
            return;

        var root = Path.GetFullPath(Path.Combine(FindRepositoryRoot(), ".omx"))
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var output = Path.GetFullPath(requested);
        if (!output.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Path.GetExtension(output), ".png", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Render evidence must be a PNG inside the repository .omx directory.");
        }

        File.WriteAllBytes(output, png);
    }

    private static T RunSta<T>(Func<T> action)
    {
        T? result = default;
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { result = action(); }
            catch (Exception exception) { failure = exception; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (!thread.Join(TimeSpan.FromSeconds(15)))
            throw new TimeoutException("The personal-storage inspection test did not finish.");
        if (failure is not null)
            throw new InvalidOperationException("The inspection test failed.", failure);
        return result!;
    }

    private static string Read(params string[] segments) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

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
