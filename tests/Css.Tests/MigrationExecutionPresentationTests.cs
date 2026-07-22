using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Css.App;
using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Operations;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationExecutionPresentationTests
{
    public static TheoryData<MigrationExecutionStatus, string> StatusCases =>
        new()
        {
            { MigrationExecutionStatus.Completed, "迁移完成，开始观察 C 盘" },
            { MigrationExecutionStatus.Refused, "迁移没有开始" },
            { MigrationExecutionStatus.FailedRolledBack, "迁移失败，已还原" },
            { MigrationExecutionStatus.FailedRollbackIncomplete, "迁移未完成，需要人工检查" }
        };

    [Theory]
    [MemberData(nameof(StatusCases))]
    public void Every_state_is_a_path_free_beginner_conclusion(
        MigrationExecutionStatus status,
        string title)
    {
        var view = MigrationExecutionResultPresenter.Create(Result(status));

        view.Title.Should().Be(title);
        view.StatusLabel.Should().NotBeNullOrWhiteSpace();
        view.Conclusion.Should().NotBeNullOrWhiteSpace();
        view.AgentAdvice.Should().NotBeNullOrWhiteSpace();
        view.SafetyText.Should().NotBeNullOrWhiteSpace();
        view.CanExecuteDirectly.Should().BeFalse();
        view.VisibleText.Should().NotContain(@"C:\Users\Private");
        view.VisibleText.Should().NotContain(@"D:\Agent\Private");
        view.VisibleText.Should().NotContain("Injected private failure");
    }

    [Fact]
    public void Result_window_keeps_agent_conclusion_first_with_stable_automation_ids()
    {
        var xaml = Read("src", "Css.App", "MigrationExecutionResultWindow.xaml");
        var code = Read("src", "Css.App", "MigrationExecutionResultWindow.xaml.cs");
        var plan = Read("src", "Css.App", "MigrationPlanWindow.xaml.cs");

        var title = xaml.IndexOf("MigrationExecutionResultTitleTextBlock", StringComparison.Ordinal);
        var status = xaml.IndexOf("MigrationExecutionResultStatusTextBlock", StringComparison.Ordinal);
        var conclusion = xaml.IndexOf("MigrationExecutionResultConclusionTextBlock", StringComparison.Ordinal);
        var agent = xaml.IndexOf("MigrationExecutionResultAgentAdviceTextBlock", StringComparison.Ordinal);
        var safety = xaml.IndexOf("MigrationExecutionResultSafetyTextBlock", StringComparison.Ordinal);
        var close = xaml.IndexOf("MigrationExecutionResultCloseButton", StringComparison.Ordinal);

        title.Should().BeGreaterThan(-1);
        status.Should().BeGreaterThan(title);
        conclusion.Should().BeGreaterThan(status);
        agent.Should().BeGreaterThan(conclusion);
        safety.Should().BeGreaterThan(agent);
        close.Should().BeGreaterThan(safety);
        foreach (var id in new[]
                 {
                     "MigrationExecutionResultTitleTextBlock",
                     "MigrationExecutionResultStatusTextBlock",
                     "MigrationExecutionResultConclusionTextBlock",
                     "MigrationExecutionResultAgentAdviceTextBlock",
                     "MigrationExecutionResultSafetyTextBlock",
                     "MigrationExecutionResultCloseButton"
                 })
        {
            xaml.Should().Contain($"AutomationProperties.AutomationId=\"{id}\"");
        }
        code.Should().NotContain("Process.Start");
        code.Should().NotContain("Directory.Move");
        code.Should().NotContain("File.Move");
        plan.Should().NotContain("MigrationOperationHandler");
        plan.Should().NotContain("WindowsDirectoryMigrationPathAdapter");
        plan.Should().NotContain("SafetyOperationPipeline");
    }

    [Fact]
    public void Completed_result_renders_nonblank_with_all_conclusions_in_first_view()
    {
        var render = RunSta(() =>
        {
            var window = new MigrationExecutionResultWindow(
                MigrationExecutionResultPresenter.Create(
                    Result(MigrationExecutionStatus.Completed)));
            window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            var elements = new[]
            {
                Find(window, "MigrationExecutionResultTitleTextBlock"),
                Find(window, "MigrationExecutionResultConclusionTextBlock"),
                Find(window, "MigrationExecutionResultAgentAdviceTextBlock"),
                Find(window, "MigrationExecutionResultSafetyTextBlock"),
                (FrameworkElement)(window.FindName("MigrationExecutionResultCloseButton")
                    ?? throw new InvalidOperationException("Missing migration result close button."))
            };
            var png = Render(window);
            PersistEvidenceIfRequested(png);
            var result = new RenderResult
            {
                PngLength = png.Length,
                Sha256 = Convert.ToHexString(SHA256.HashData(png)),
                AllVisible = elements.All(element => IsVisible(element, window))
            };
            window.Close();
            return result;
        });

        render.PngLength.Should().BeGreaterThan(10_000);
        render.Sha256.Should().MatchRegex("^[0-9A-F]{64}$");
        render.AllVisible.Should().BeTrue();
    }

    private static OperationResult Result(MigrationExecutionStatus status) =>
        new()
        {
            Success = status == MigrationExecutionStatus.Completed,
            Error = status == MigrationExecutionStatus.Completed ? null : "Private technical error",
            Payload = new MigrationExecutionResult
            {
                Status = status,
                Summary = @"Injected private failure C:\Users\Private D:\Agent\Private",
                MovedPathCount = 2,
                RollbackAttempted = status is MigrationExecutionStatus.FailedRolledBack
                    or MigrationExecutionStatus.FailedRollbackIncomplete,
                RollbackSucceeded = status != MigrationExecutionStatus.FailedRollbackIncomplete,
                Errors = [@"C:\Users\Private\cache", "Injected private failure"]
            }
        };

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

    private static TextBlock Find(Window window, string name) =>
        (TextBlock)(window.FindName(name)
            ?? throw new InvalidOperationException($"Missing text block {name}."));

    private static void PersistEvidenceIfRequested(byte[] png)
    {
        var requested = Environment.GetEnvironmentVariable("OMNIX_MIGRATION_RESULT_RENDER_OUTPUT");
        if (string.IsNullOrWhiteSpace(requested))
            return;
        var root = Path.GetFullPath(Path.Combine(FindRepositoryRoot(), ".omx"))
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var output = Path.GetFullPath(requested);
        if (!output.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Path.GetExtension(output), ".png", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Migration render evidence must be a PNG inside .omx.");
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
            throw new TimeoutException("The migration result WPF test did not finish.");
        if (failure is not null)
            throw new InvalidOperationException("The migration result WPF test failed.", failure);
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

    private sealed class RenderResult
    {
        public required int PngLength { get; init; }
        public required string Sha256 { get; init; }
        public required bool AllVisible { get; init; }
    }
}
