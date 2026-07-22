using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Software;
using Css.Core.Startup;
using Css.Core.Timeline;
using FluentAssertions;

namespace Css.Tests;

public sealed class StartupControlExperienceTests
{
    [Fact]
    public async Task Ready_startup_entry_becomes_a_path_free_local_review()
    {
        var now = DateTimeOffset.UtcNow;
        var observation = StartupObservation(now);
        var state = StartupEntryStateFactory.Create(
            observation,
            StartupRegistryValueKind.String,
            "\"D:\\Software\\Fixture\\fixture.exe\" --background",
            Sha('A'),
            now);
        var preparation = await StartupControlPreparationService.PrepareAsync(
            Profile(observation),
            new FakeStartupStore(state),
            now);
        var drawer = AppPresentationBuilder.CreateDrawer(Profile(observation));
        var handoff = AppStartupSettingsHandoffPresenter.Create(Profile(observation));

        var host = AppDrawerActionHostPresenter.ShowStartupControl(drawer, preparation, handoff);

        host.PrimaryActionText.Should().Be("审核关闭方案");
        host.PrimaryActionKey.Should().Be("StartupDisableReview");
        host.CanExecuteDirectly.Should().BeFalse();
        host.Summary.Should().Contain("可还原");
        string.Join("\n", host.Lines).Should().NotContain(@"D:\Software");
        string.Join("\n", host.Lines).Should().NotContain("HKCU");
    }

    [Fact]
    public async Task Startup_confirmation_explains_scope_and_requires_two_acknowledgements()
    {
        var now = DateTimeOffset.UtcNow;
        var observation = StartupObservation(now);
        var state = StartupEntryStateFactory.Create(
            observation,
            StartupRegistryValueKind.String,
            "fixture.exe --background",
            Sha('B'),
            now);
        var preparation = await StartupControlPreparationService.PrepareAsync(
            Profile(observation),
            new FakeStartupStore(state),
            now);
        var evidence = new StartupRollbackManifestEvidence(
            "startup-snapshot-0123456789abcdef0123456789abcdef",
            @"C:\Evidence\startup.json",
            Sha('C'));
        var operation = StartupEntryControlOperationPolicy.CreateDisablePlan(preparation, evidence);

        var view = StartupControlConfirmationPresenter.Create(preparation, operation);

        view.Title.Should().Be("确认关闭自启动");
        view.Headline.Should().Contain("Fixture App");
        view.OutcomeLines.Should().Contain(line => line.Contains("不会关闭") && line.Contains("正在运行"));
        view.OutcomeLines.Should().Contain(line => line.Contains("服务") && line.Contains("计划任务"));
        view.OutcomeLines.Should().Contain(line => line.Contains("后悔药中心") && line.Contains("还原"));
        view.FirstAcknowledgementText.Should().NotBeNullOrWhiteSpace();
        view.SecondAcknowledgementText.Should().Contain("重新创建");
        view.ConfirmButtonText.Should().Be("确认关闭自启动");
        string.Join("\n", view.OutcomeLines).Should().NotContain(@"C:\Evidence");
        string.Join("\n", view.OutcomeLines).Should().NotContain("fixture.exe");
        view.TechnicalDetails.Should().Contain(line => line.Contains(StartupEntryControlPolicy.SupportedSourceLocator));
        view.TechnicalDetails.Should().NotContain(line => line.Contains("fixture.exe"));
    }

    [Fact]
    public void Startup_confirmation_window_has_stable_first_view_and_consent_hooks()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "StartupControlConfirmationWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "StartupControlConfirmationWindow.xaml.cs"));

        xaml.Should().Contain("AutomationProperties.AutomationId=\"StartupConfirmationHeadlineTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"StartupConfirmationOutcomeListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"StartupConfirmationFirstCheckBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"StartupConfirmationSecondCheckBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"StartupConfirmationConfirmButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"StartupConfirmationCancelButton\"");
        xaml.IndexOf("StartupConfirmationOutcomeListBox", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("StartupConfirmationTechnicalDetailsExpander", StringComparison.Ordinal));
        code.Should().Contain("UpdateConfirmState");
        code.Should().Contain("FirstAcknowledgementCheckBox.IsChecked == true");
        code.Should().Contain("SecondAcknowledgementCheckBox.IsChecked == true");
    }

    [Fact]
    public void Main_window_routes_local_startup_change_through_confirmation_pipeline_and_timeline_restore_dispatch()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var preview = Extract(
            code,
            "private async void PreviewStartupControl_Click",
            "private void ApplyDrawerActionHost");
        var primary = Extract(
            code,
            "private async void DrawerActionPreviewPrimary_Click",
            "private async Task ExecutePendingAppCacheCleanupAsync");
        var startupExecution = Extract(
            code,
            "private async Task ReviewAndExecutePendingStartupDisableAsync",
            "private async Task RefreshCacheCleanupStateAfterAttemptAsync");
        var restore = Extract(
            code,
            "private async void RestoreTimeline_Click",
            "private async void ExecuteRecommendation_Click");

        preview.Should().Contain("StartupControlPreparationService.PrepareAsync");
        primary.Should().Contain("case \"StartupDisableReview\":");
        startupExecution.Should().Contain("new StartupControlConfirmationWindow");
        startupExecution.Should().Contain("StartupEntryControlOperationPolicy.ConfirmForExecution");
        startupExecution.Should().Contain("new SafetyOperationPipeline");
        startupExecution.Should().Contain("StartupEntryControlOperationHandler");
        startupExecution.IndexOf("ShowDialog()", StringComparison.Ordinal)
            .Should().BeLessThan(startupExecution.IndexOf("ConfirmForExecution", StringComparison.Ordinal));
        restore.Should().Contain("StartupEntryControlOperationPolicy.RestoreKind");
        restore.Should().Contain("RestoreStartupTimelineItemAsync");
    }

    [Fact]
    public void Startup_disable_refreshes_applications_and_timeline_after_every_pipeline_attempt()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var helper = SourceMethodExtractor.Extract(
            code,
            "private async Task RefreshStartupStateAfterAttemptAsync()");
        var execute = SourceMethodExtractor.Extract(
            code,
            "private async Task ReviewAndExecutePendingStartupDisableAsync()");

        const string attempted = "pipelineAttempted = true;";
        const string executePipeline = "await pipeline.ExecuteAsync(descriptor)";
        const string synchronize = "await RefreshStartupStateAfterAttemptAsync();";
        const string successGate = "if (!result.Success)";
        execute.Should().Contain("var pipelineAttempted = false;");
        execute.Should().Contain("var stateSynchronized = false;");
        execute.IndexOf("confirmed = true;", StringComparison.Ordinal)
            .Should().BeLessThan(execute.IndexOf(attempted, StringComparison.Ordinal));
        execute.IndexOf(attempted, StringComparison.Ordinal)
            .Should().BeLessThan(execute.IndexOf(executePipeline, StringComparison.Ordinal));
        execute.IndexOf(executePipeline, StringComparison.Ordinal)
            .Should().BeLessThan(execute.IndexOf(synchronize, StringComparison.Ordinal));
        execute.IndexOf(synchronize, StringComparison.Ordinal)
            .Should().BeLessThan(execute.IndexOf(successGate, StringComparison.Ordinal));
        execute.Should().Contain("if (pipelineAttempted && !stateSynchronized)");
        execute.Should().Contain("if (!confirmed && manifestEvidence is not null)");
        execute.Split(synchronize, StringSplitOptions.None).Length.Should().Be(3);

        helper.Should().Contain("SetSoftwareProfiles(await ScanSoftwareProfilesAsync());");
        helper.Should().Contain("await LoadTimelineAsync();");
        helper.Should().NotContain("SafetyOperationPipeline");
        helper.Should().NotContain("DisableAsync");
        helper.Should().NotContain("RestoreAsync");
        helper.Should().NotContain("DeleteUncommittedAsync");
        helper.Should().NotContain("Registry");
    }

    [Fact]
    public void Startup_timeline_restore_confirmation_is_path_free_and_truthful()
    {
        var manifestPath = @"C:\Users\Me\AppData\Local\OMNIX-Entropy\StartupRollback\secret.json";
        var item = new ActionTimelineItemViewModel
        {
            Id = 41,
            Title = "2026-07-15 09:00  关闭 Fixture App 的自启动",
            Detail = "手动 / 已保存恢复证据",
            TechnicalDetailsButtonText = "查看技术详情",
            RestoreLine = "可以还原",
            RestoreButtonText = "还原",
            RestoreHint = "恢复原始值",
            CanRestore = true,
            RestoreOperationKind = StartupEntryControlOperationPolicy.RestoreKind,
            RestoreManifestPaths = [manifestPath]
        };

        var view = TimelineRestoreConfirmationPresenter.Create(item);

        view.Title.Should().Be("确认恢复自启动");
        view.Summary.Should().Contain("下次登录");
        view.SafetyText.Should().Contain("不会立即启动软件");
        view.SafetyText.Should().Contain("服务").And.Contain("计划任务");
        string.Join("\n", [view.Headline, view.Summary, view.SafetyText])
            .Should().NotContain(manifestPath);
    }

    [Fact]
    public void Startup_gui_smoke_is_fixture_only_and_cancel_only()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-startup-control-cancel-smoke.ps1"));

        script.Should().Contain("OMNIX_ENTROPY_DATA_ROOT");
        script.Should().Contain("OMNIX_ENTROPY_SOFTWARE_FIXTURE");
        script.Should().Contain("OMNIX_ENTROPY_STARTUP_FIXTURE");
        script.Should().Contain("StartupConfirmationHeadlineTextBlock");
        script.Should().Contain("StartupConfirmationConfirmButton");
        script.Should().Contain("StartupConfirmationCancelButton");
        script.Should().Contain("manifestCountAfterCancel");
        script.Should().Contain("secondReviewReached");
        script.Should().Contain("noOperationExecuted = $true");
        script.Should().Contain("finally");
        script.Should().NotContain("Invoke-Element $confirmButton");
        script.Should().NotContain("Registry.SetValue");
        script.Should().NotContain("Remove-ItemProperty");
    }

    [Fact]
    public void Startup_restore_gui_smoke_seeds_core_timeline_data_and_cancels_the_window()
    {
        var tool = File.ReadAllText(FindRepositoryFile("src", "Css.SmokeTools", "Program.cs"));
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-startup-restore-cancel-smoke.ps1"));

        tool.Should().Contain("seed-startup-undo-center");
        tool.Should().Contain("StartupEntryControlOperationHandler");
        tool.Should().Contain("SafetyOperationPipeline");
        tool.Should().NotContain("Microsoft.Win32");
        tool.Should().NotContain("RegistryKey");
        script.Should().Contain("seed-startup-undo-center");
        script.Should().Contain("TimelineRestoreConfirmationHeadlineTextBlock");
        script.Should().Contain("TimelineRestoreConfirmationCancelButton");
        script.Should().Contain("startupManifestStillExists = $true");
        script.Should().Contain("noRestoreExecuted = $true");
        script.Should().NotContain("TimelineRestoreConfirmationConfirmButton");
        script.Should().NotContain("Invoke-Element $confirmButton");
    }

    private static BackgroundComponentObservation StartupObservation(DateTimeOffset now) =>
        BackgroundComponentObservationFactory.Startup(
            "Fixture Startup",
            StartupEntryControlPolicy.SupportedSourceLocator,
            "fixture.exe --background",
            now,
            StartupApprovalObservationFactory.FromRegistryValue(
                StartupEntryControlPolicy.SupportedApprovalLocator,
                "Fixture Startup",
                new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));

    private static SoftwareProfile Profile(BackgroundComponentObservation observation) =>
        new()
        {
            Name = "Fixture App",
            Category = SoftwareCategory.Normal,
            StartupEntries = [observation.Identity.DisplayName],
            BackgroundComponents = [observation]
        };

    private static string Sha(char value) => new(value, 64);

    private static string FindRepositoryFile(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. segments]);
            if (File.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new FileNotFoundException("Repository file was not found.", Path.Combine(segments));
    }

    private static string Extract(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }

    private sealed class FakeStartupStore(StartupEntryState state) : IStartupEntryControlStore
    {
        public Task<StartupEntryCaptureResult> CaptureAsync(
            BackgroundComponentObservation observation,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(StartupEntryCaptureResult.Completed(state));

        public Task<StartupEntryMutationResult> DisableAsync(
            StartupEntryState expected,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(StartupEntryMutationResult.Completed("disabled"));

        public Task<StartupEntryMutationResult> RestoreAsync(
            StartupEntryState expected,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(StartupEntryMutationResult.Completed("restored"));
    }
}
