using System.Threading;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Css.App;
using Css.Core.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class UninstallPostScanActionTests
{
    [Fact]
    public void Presenter_maps_each_result_to_one_typed_safe_next_action()
    {
        View(new OfficialUninstallPostScanResult { Success = false, Summary = "fixture" })
            .PrimaryAction.Should().Be(OfficialUninstallPostScanAction.RetryReadOnlyScan);
        View(new OfficialUninstallPostScanResult
            {
                Success = true,
                SoftwareStillPresent = true,
                Summary = "fixture"
            })
            .PrimaryAction.Should().Be(OfficialUninstallPostScanAction.RetryReadOnlyScan);
        View(new OfficialUninstallPostScanResult
            {
                Success = true,
                ResidueCandidateCount = 0,
                Summary = "fixture"
            })
            .PrimaryAction.Should().Be(OfficialUninstallPostScanAction.Close);
        View(new OfficialUninstallPostScanResult
            {
                Success = true,
                ResidueCandidateCount = 1,
                Summary = "fixture"
            })
            .PrimaryAction.Should().Be(OfficialUninstallPostScanAction.ReviewResidue);
        View(new OfficialUninstallPostScanResult
            {
                Success = true,
                RequiresBackgroundRescan = true,
                Summary = "fixture"
            })
            .PrimaryAction.Should().Be(OfficialUninstallPostScanAction.RetryReadOnlyScan);
    }

    [Fact]
    public void Result_window_returns_the_typed_action_without_executing_it()
    {
        var result = RunSta(() =>
        {
            var reviewWindow = new UninstallPostScanResultWindow(View(
                new OfficialUninstallPostScanResult
                {
                    Success = true,
                    ResidueCandidateCount = 1,
                    Summary = "fixture"
                }));
            var reviewButton = Find<Button>(reviewWindow, "UninstallPostScanPrimaryActionButton");
            var reviewState = (reviewButton.Visibility, reviewButton.Content?.ToString());
            reviewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var requested = reviewWindow.RequestedAction;

            var cleanWindow = new UninstallPostScanResultWindow(View(
                new OfficialUninstallPostScanResult
                {
                    Success = true,
                    ResidueCandidateCount = 0,
                    Summary = "fixture"
                }));
            var cleanButton = Find<Button>(cleanWindow, "UninstallPostScanPrimaryActionButton");
            var cleanVisibility = cleanButton.Visibility;
            cleanWindow.Close();
            return (reviewState, requested, cleanVisibility);
        });

        result.reviewState.Visibility.Should().Be(Visibility.Visible);
        result.reviewState.Item2.Should().Be("\u67e5\u770b\u6b8b\u7559\u6e05\u5355");
        result.requested.Should().Be(OfficialUninstallPostScanAction.ReviewResidue);
        result.cleanVisibility.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Plan_and_parent_keep_actions_typed_and_retry_read_only()
    {
        var resultXaml = Read("src", "Css.App", "UninstallPostScanResultWindow.xaml");
        var resultCode = Read("src", "Css.App", "UninstallPostScanResultWindow.xaml.cs");
        var planCode = Read("src", "Css.App", "UninstallPlanWindow.xaml.cs");
        var mainCode = Read("src", "Css.App", "MainWindow.xaml.cs");
        var plan = SourceMethodExtractor.Extract(
            planCode,
            "private async void ContinueFinalConsent_Click(object sender, RoutedEventArgs e)");
        var parent = SourceMethodExtractor.Extract(
            mainCode,
            "private async Task ShowUninstallPlanAsync(SoftwareProfile profile)");
        var retry = SourceMethodExtractor.Extract(
            mainCode,
            "private void ShowReadOnlyUninstallResidueReviewAfterRetry(");

        resultXaml.Should().Contain(
            "AutomationProperties.AutomationId=\"UninstallPostScanPrimaryActionButton\"")
            .And.Contain("Click=\"PrimaryAction_Click\"");
        resultCode.Should().Contain("RequestedAction")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("QuarantineOperation")
            .And.NotContain("Process.Start")
            .And.NotContain("File.Move")
            .And.NotContain("File.Delete");
        plan.Should().Contain("ProductionPostScanActionRequested")
            .And.Contain("postScanWindow.RequestedAction");
        parent.Should().Contain("OfficialUninstallPostScanAction.ReviewResidue")
            .And.Contain("OfficialUninstallPostScanAction.RetryReadOnlyScan")
            .And.Contain("ShowReadOnlyUninstallResidueReviewAfterRetry")
            .And.Contain("await ReviewUninstallResidueAsync(profile, refreshedProfiles)");
        retry.Should().Contain("UninstallResidueScanBuilder.Build")
            .And.Contain("UninstallResidueReviewPresentationBuilder.Create")
            .And.Contain("ShowResidueReviewInline")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("CleanupConfirmationWindow")
            .And.NotContain("QuarantineOperation");
    }

    [Fact]
    public void Review_action_renders_with_beginner_conclusions_in_the_first_view()
    {
        var result = RunSta(() =>
        {
            var window = new UninstallPostScanResultWindow(View(
                new OfficialUninstallPostScanResult
                {
                    Success = true,
                    ResidueCandidateCount = 1,
                    PathResidueCandidateCount = 1,
                    Summary = "private fixture details must stay hidden"
                }));
            window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

            var elements = new[]
            {
                Find<FrameworkElement>(window, "UninstallPostScanTitleTextBlock"),
                Find<FrameworkElement>(window, "UninstallPostScanConclusionTextBlock"),
                Find<FrameworkElement>(window, "UninstallPostScanAgentAdviceTextBlock"),
                Find<FrameworkElement>(window, "UninstallPostScanSafetyTextBlock"),
                Find<FrameworkElement>(window, "UninstallPostScanPrimaryActionButton"),
                Find<FrameworkElement>(window, "UninstallPostScanCloseButton")
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

    private static OfficialUninstallPostScanViewModel View(
        OfficialUninstallPostScanResult result) =>
        OfficialUninstallPostScanPresenter.Create("Example App", result);

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
            "OMNIX_POST_SCAN_ACTION_RENDER_OUTPUT");
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
            try
            {
                result = action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (!thread.Join(TimeSpan.FromSeconds(15)))
            throw new TimeoutException("The uninstall post-scan action test did not finish.");
        if (failure is not null)
            throw new InvalidOperationException("The uninstall post-scan action test failed.", failure);
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
