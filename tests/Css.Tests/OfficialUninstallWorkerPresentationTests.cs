using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Css.App;
using Css.Core.Operations;
using Css.Core.Uninstall;
using Css.Ipc.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallWorkerPresentationTests
{
    public static TheoryData<OfficialUninstallWorkerLifecycleStatus, string> StatusCases =>
        new()
        {
            { OfficialUninstallWorkerLifecycleStatus.CompletedFake, "安全连接测试已完成" },
            { OfficialUninstallWorkerLifecycleStatus.CompletedProduction, "卸载完成，发现待检查内容" },
            { OfficialUninstallWorkerLifecycleStatus.ProductionLauncherRejected, "正式执行身份没有通过" },
            { OfficialUninstallWorkerLifecycleStatus.UserCanceledElevation, "你取消了 Windows 确认" },
            { OfficialUninstallWorkerLifecycleStatus.LaunchFailed, "安全助手没有启动" },
            { OfficialUninstallWorkerLifecycleStatus.WorkerImageRejected, "安全助手文件不一致" },
            { OfficialUninstallWorkerLifecycleStatus.PeerRejected, "安全助手身份不一致" },
            { OfficialUninstallWorkerLifecycleStatus.BootstrapFailed, "安全验证没有完成" },
            { OfficialUninstallWorkerLifecycleStatus.ResponseTimedOut, "等待安全助手超时" },
            { OfficialUninstallWorkerLifecycleStatus.TransportFailed, "安全助手没有启动" },
            { OfficialUninstallWorkerLifecycleStatus.WorkerExitFailed, "安全助手没有正常关闭" },
            { OfficialUninstallWorkerLifecycleStatus.InvalidRequest, "安全清单已经失效" },
            { OfficialUninstallWorkerLifecycleStatus.Canceled, "操作已取消" }
        };

    [Theory]
    [MemberData(nameof(StatusCases))]
    public void Every_lifecycle_state_becomes_a_path_free_beginner_conclusion(
        OfficialUninstallWorkerLifecycleStatus status,
        string expectedTitle)
    {
        var view = OfficialUninstallWorkerResultPresenter.Create(new()
        {
            Status = status,
            Response = status == OfficialUninstallWorkerLifecycleStatus.CompletedProduction
                ? ProductionResponse(2)
                : null,
            BootstrapStatus = OfficialUninstallSessionBootstrapStatus.ProtocolRejected,
            TransportStatus = OfficialUninstallTransportStatus.ConnectionFailed,
            ChildExited = status != OfficialUninstallWorkerLifecycleStatus.WorkerExitFailed
        });

        view.Title.Should().Be(expectedTitle);
        view.StatusLabel.Should().NotBeNullOrWhiteSpace();
        view.Conclusion.Should().NotBeNullOrWhiteSpace();
        view.AgentAdvice.Should().NotBeNullOrWhiteSpace();
        view.SafetyText.Should().NotBeNullOrWhiteSpace();
        view.CanExecuteDirectly.Should().BeFalse();
        view.VisibleText.Should().NotContain(@"C:\");
        view.VisibleText.Should().NotContain(@"D:\");
        view.VisibleText.Should().NotContain("PID");
        view.VisibleText.Should().NotContain("ECDH");
        view.VisibleText.Should().NotContain("ProtocolRejected");
        view.VisibleText.Should().NotContain("ConnectionFailed");
    }

    [Fact]
    public void Fake_completion_explicitly_says_no_uninstaller_or_system_change_occurred()
    {
        var view = OfficialUninstallWorkerResultPresenter.Create(new()
        {
            Status = OfficialUninstallWorkerLifecycleStatus.CompletedFake,
            ChildExited = true
        });

        view.Conclusion.Should().Contain("不修改电脑");
        view.SafetyText.Should().Contain("没有运行卸载器");
        view.SafetyText.Should().Contain("没有删除");
        view.VisibleText.Should().NotContain("卸载完成");
    }

    [Fact]
    public void Production_completion_explains_post_scan_and_never_claims_residue_was_deleted()
    {
        var view = OfficialUninstallWorkerResultPresenter.Create(new()
        {
            Status = OfficialUninstallWorkerLifecycleStatus.CompletedProduction,
            Response = ProductionResponse(3),
            ChildExited = true
        });

        view.Title.Should().Be("卸载完成，发现待检查内容");
        view.Conclusion.Should().Contain("3 项可能的残留");
        view.AgentAdvice.Should().Contain("按风险分组");
        view.SafetyText.Should().Contain("还在原位");
        view.VisibleText.Should().NotContain(@"C:\");
        view.VisibleText.Should().NotContain("registry");
        view.VisibleText.Should().NotContain("service");
    }

    [Fact]
    public void Production_completion_without_residue_is_a_truthful_finished_conclusion()
    {
        var view = OfficialUninstallWorkerResultPresenter.Create(new()
        {
            Status = OfficialUninstallWorkerLifecycleStatus.CompletedProduction,
            Response = ProductionResponse(0),
            ChildExited = true
        });

        view.Title.Should().Be("卸载和复查已完成");
        view.StatusLabel.Should().Be("处理完成");
        view.Conclusion.Should().Contain("没有发现需要处理的残留");
        view.SafetyText.Should().Contain("没有执行额外的残留删除");
    }

    [Fact]
    public void Worker_path_resolver_accepts_only_the_exact_non_reparse_sibling()
    {
        var baseDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "omnix-app"));
        var expected = Path.Combine(baseDirectory, "Css.Elevated.exe");

        var ready = OfficialUninstallWorkerPathResolver.Resolve(
            baseDirectory,
            path => string.Equals(path, expected, StringComparison.OrdinalIgnoreCase),
            _ => FileAttributes.Normal);
        var missing = OfficialUninstallWorkerPathResolver.Resolve(
            baseDirectory,
            _ => false,
            _ => FileAttributes.Normal);
        var reparse = OfficialUninstallWorkerPathResolver.Resolve(
            baseDirectory,
            _ => true,
            _ => FileAttributes.ReparsePoint);
        var failed = OfficialUninstallWorkerPathResolver.Resolve(
            baseDirectory,
            _ => throw new IOException("private path must not surface"),
            _ => FileAttributes.Normal);

        ready.CanLaunchVerification.Should().BeTrue();
        ready.ExecutablePath.Should().Be(expected);
        missing.Status.Should().Be(OfficialUninstallWorkerAvailabilityStatus.Missing);
        reparse.Status.Should().Be(OfficialUninstallWorkerAvailabilityStatus.UnsafePath);
        failed.Status.Should().Be(OfficialUninstallWorkerAvailabilityStatus.ProbeFailed);
        missing.ExecutablePath.Should().BeNull();
        reparse.ExecutablePath.Should().BeNull();
        failed.ExecutablePath.Should().BeNull();
    }

    [Fact]
    public void App_build_output_packages_worker_without_an_assembly_dependency()
    {
        var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Name ?? "Debug";
        var appProject = FindRepositoryFile("src", "Css.App", "Css.App.csproj");
        var appOutput = Path.Combine(
            Path.GetDirectoryName(appProject)!,
            "bin",
            configuration,
            "net8.0-windows");

        foreach (var file in new[]
                 {
                     "Css.Elevated.exe",
                     "Css.Elevated.dll",
                     "Css.Elevated.deps.json",
                     "Css.Elevated.runtimeconfig.json"
                 })
        {
            File.Exists(Path.Combine(appOutput, file)).Should().BeTrue(
                $"App output should package the fake-only worker file {file}");
        }

        var appDeps = File.ReadAllText(Path.Combine(appOutput, "Css.App.deps.json"));
        appDeps.Should().NotContain("Css.Elevated");

        var project = File.ReadAllText(appProject);
        project.Should().Contain("BuildAndCopyElevatedWorker");
        project.Should().Contain("CopyElevatedWorkerToPublish");
        project.Should().Contain("Css.Elevated.runtimeconfig.json");
        project.Should().NotContain("<ProjectReference Include=\"..\\Css.Elevated");
    }

    [Fact]
    public void Result_window_keeps_the_agent_conclusion_first_and_has_stable_automation_peers()
    {
        var xaml = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "OfficialUninstallWorkerResultWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "OfficialUninstallWorkerResultWindow.xaml.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));

        var title = xaml.IndexOf("OfficialUninstallWorkerResultTitleTextBlock", StringComparison.Ordinal);
        var status = xaml.IndexOf("OfficialUninstallWorkerResultStatusTextBlock", StringComparison.Ordinal);
        var conclusion = xaml.IndexOf("OfficialUninstallWorkerResultConclusionTextBlock", StringComparison.Ordinal);
        var agent = xaml.IndexOf("OfficialUninstallWorkerResultAgentAdviceTextBlock", StringComparison.Ordinal);
        var safety = xaml.IndexOf("OfficialUninstallWorkerResultSafetyTextBlock", StringComparison.Ordinal);
        var close = xaml.IndexOf("OfficialUninstallWorkerResultCloseButton", StringComparison.Ordinal);

        title.Should().BeGreaterThan(-1);
        status.Should().BeGreaterThan(title);
        conclusion.Should().BeGreaterThan(status);
        agent.Should().BeGreaterThan(conclusion);
        safety.Should().BeGreaterThan(agent);
        close.Should().BeGreaterThan(safety);
        xaml.Should().Contain("AutomationProperties.AutomationId=\"OfficialUninstallWorkerResultTitleTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"OfficialUninstallWorkerResultConclusionTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"OfficialUninstallWorkerResultAgentAdviceTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"OfficialUninstallWorkerResultSafetyTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"OfficialUninstallWorkerResultCloseButton\"");
        code.Should().NotContain("Process.Start");
        code.Should().NotContain("OfficialUninstallOperationHandler");
        app.Should().Contain("--smoke-uninstall-worker-lifecycle");
        app.Should().Contain("#if DEBUG");
        app.Should().Contain("RunWorkerLifecycleSmokeAsync");

        var script = File.ReadAllText(FindRepositoryFile(
            ".omx", "gui-uninstall-worker-lifecycle-smoke.ps1"));
        var doc = File.ReadAllText(FindRepositoryFile(
            "docs", "development", "gui-smokes.md"));
        script.Should().Contain("--smoke-uninstall-worker-lifecycle");
        script.Should().Contain("ValidateSet('Accept', 'Cancel')");
        script.Should().Contain("OfficialUninstallWorkerResultTitleTextBlock");
        script.Should().Contain("OfficialUninstallWorkerResultAgentAdviceTextBlock");
        script.Should().Contain("OfficialUninstallWorkerResultSafetyTextBlock");
        script.Should().NotContain("OfficialUninstallOperationHandler");
        script.Should().NotContain("WindowsOfficialUninstallerLauncher");
        doc.Should().Contain("Elevated Worker Lifecycle Smoke");
        doc.Should().Contain("gui-uninstall-worker-lifecycle-smoke.ps1");
    }

    [Fact]
    public void Beginner_result_renders_nonblank_with_all_conclusions_in_the_first_view()
    {
        var render = RunSta(() =>
        {
            var window = new OfficialUninstallWorkerResultWindow(
                OfficialUninstallWorkerResultPresenter.Create(new()
                {
                    Status = OfficialUninstallWorkerLifecycleStatus.CompletedProduction,
                    Response = ProductionResponse(3),
                    ChildExited = true
                }));
            window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

            var title = FindText(window, "OfficialUninstallWorkerResultTitleTextBlock");
            var conclusion = FindText(window, "OfficialUninstallWorkerResultConclusionTextBlock");
            var agent = FindText(window, "OfficialUninstallWorkerResultAgentAdviceTextBlock");
            var safety = FindText(window, "OfficialUninstallWorkerResultSafetyTextBlock");
            var close = (FrameworkElement)(window.FindName("OfficialUninstallWorkerResultCloseButton")
                ?? throw new InvalidOperationException("Missing result close button."));
            var bytes = Render(window);
            PersistEvidenceIfRequested(bytes, "OMNIX_WORKER_RESULT_RENDER_OUTPUT");
            var result = new RenderResult
            {
                PngLength = bytes.Length,
                Sha256 = Convert.ToHexString(SHA256.HashData(bytes)),
                TitleVisible = IsVisible(title, window),
                ConclusionVisible = IsVisible(conclusion, window),
                AgentVisible = IsVisible(agent, window),
                SafetyVisible = IsVisible(safety, window),
                CloseVisible = IsVisible(close, window)
            };
            window.Close();
            return result;
        });

        render.PngLength.Should().BeGreaterThan(10_000);
        render.Sha256.Should().MatchRegex("^[0-9A-F]{64}$");
        render.TitleVisible.Should().BeTrue();
        render.ConclusionVisible.Should().BeTrue();
        render.AgentVisible.Should().BeTrue();
        render.SafetyVisible.Should().BeTrue();
        render.CloseVisible.Should().BeTrue();
    }

    [Fact]
    public void Unknown_attempt_renders_all_beginner_conclusions_in_the_first_view()
    {
        var render = RunSta(() =>
        {
            var window = new OfficialUninstallWorkerResultWindow(
                OfficialUninstallWorkerResultPresenter.CreateUnknownAttempt());
            window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

            var title = FindText(window, "OfficialUninstallWorkerResultTitleTextBlock");
            var conclusion = FindText(window, "OfficialUninstallWorkerResultConclusionTextBlock");
            var agent = FindText(window, "OfficialUninstallWorkerResultAgentAdviceTextBlock");
            var safety = FindText(window, "OfficialUninstallWorkerResultSafetyTextBlock");
            var close = (FrameworkElement)(window.FindName("OfficialUninstallWorkerResultCloseButton")
                ?? throw new InvalidOperationException("Missing result close button."));
            var bytes = Render(window);
            PersistEvidenceIfRequested(bytes, "OMNIX_WORKER_UNKNOWN_RENDER_OUTPUT");
            var result = new RenderResult
            {
                PngLength = bytes.Length,
                Sha256 = Convert.ToHexString(SHA256.HashData(bytes)),
                TitleVisible = IsVisible(title, window),
                ConclusionVisible = IsVisible(conclusion, window),
                AgentVisible = IsVisible(agent, window),
                SafetyVisible = IsVisible(safety, window),
                CloseVisible = IsVisible(close, window)
            };
            window.Close();
            return result;
        });

        render.PngLength.Should().BeGreaterThan(10_000);
        render.Sha256.Should().MatchRegex("^[0-9A-F]{64}$");
        render.TitleVisible.Should().BeTrue();
        render.ConclusionVisible.Should().BeTrue();
        render.AgentVisible.Should().BeTrue();
        render.SafetyVisible.Should().BeTrue();
        render.CloseVisible.Should().BeTrue();
    }

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

    private static TextBlock FindText(Window window, string name) =>
        (TextBlock)(window.FindName(name)
            ?? throw new InvalidOperationException($"Missing text block {name}."));

    private static OfficialUninstallElevatedResponseEnvelope ProductionResponse(
        int residueCount) =>
        new()
        {
            RequestId = "production-presentation-request",
            Result = OperationResult.Ok(payload: new OfficialUninstallHandlerPayload
            {
                UninstallerStarted = true,
                UninstallerCompleted = true,
                ExitCode = 0,
                RequiresPostScanRetry = false,
                PostScan = new OfficialUninstallPostScanResult
                {
                    Success = true,
                    SoftwareStillPresent = false,
                    ResidueCandidateCount = residueCount,
                    PathResidueCandidateCount = residueCount,
                    VerifiedBackgroundResidueCount = 0,
                    UnverifiedBackgroundHintCount = 0,
                    RequiresBackgroundRescan = false,
                    Summary = @"Private detail must stay hidden: C:\Users\Example"
                }
            })
        };

    private static void PersistEvidenceIfRequested(byte[] png, string environmentVariable)
    {
        var requested = Environment.GetEnvironmentVariable(environmentVariable);
        if (string.IsNullOrWhiteSpace(requested))
            return;
        var root = Path.GetFullPath(Path.Combine(FindRepositoryRoot(), ".omx"))
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var output = Path.GetFullPath(requested);
        if (!output.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Path.GetExtension(output), ".png", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Render evidence must be a PNG inside .omx.");
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
            throw new TimeoutException("The worker-result WPF test did not finish.");
        if (failure is not null)
            throw new InvalidOperationException("The worker-result WPF test failed.", failure);
        return result!;
    }

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

    private static string FindRepositoryFile(params string[] segments)
    {
        var path = Path.Combine([FindRepositoryRoot(), .. segments]);
        if (!File.Exists(path))
            throw new FileNotFoundException("Could not locate repository file.", path);
        return path;
    }

    private sealed class RenderResult
    {
        public required int PngLength { get; init; }
        public required string Sha256 { get; init; }
        public required bool TitleVisible { get; init; }
        public required bool ConclusionVisible { get; init; }
        public required bool AgentVisible { get; init; }
        public required bool SafetyVisible { get; init; }
        public required bool CloseVisible { get; init; }
    }
}
