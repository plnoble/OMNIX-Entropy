using Css.App;
using FluentAssertions;

namespace Css.Tests;

public sealed class AutomaticAppInventoryLoadingTests
{
    [Fact]
    public async Task Repeated_first_entry_shares_one_inflight_read_only_load()
    {
        var gate = new SoftwareInventoryLoadGate();
        var release = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;

        Task<bool> Load()
        {
            calls++;
            return release.Task;
        }

        var first = gate.EnsureLoadedAsync(Load);
        var second = gate.EnsureLoadedAsync(Load);

        calls.Should().Be(1);
        first.Should().BeSameAs(second);
        release.SetResult(true);
        await first;
        gate.HasCompletedLoad.Should().BeTrue();

        await gate.EnsureLoadedAsync(Load);
        calls.Should().Be(1, "a successful empty inventory is still a completed load");
    }

    [Fact]
    public async Task Failed_or_faulted_load_can_retry_and_manual_refresh_forces_a_new_load()
    {
        var gate = new SoftwareInventoryLoadGate();
        var calls = 0;

        await gate.EnsureLoadedAsync(() => Task.FromResult(++calls > 1));
        gate.HasCompletedLoad.Should().BeFalse();
        await gate.EnsureLoadedAsync(() => Task.FromResult(++calls > 1));
        gate.HasCompletedLoad.Should().BeTrue();

        await gate.RefreshAsync(() => Task.FromResult(++calls > 1));
        calls.Should().Be(3);

        var faulted = new SoftwareInventoryLoadGate();
        var faultCalls = 0;
        Func<Task> first = () => faulted.EnsureLoadedAsync(() =>
        {
            faultCalls++;
            return Task.FromException<bool>(new InvalidOperationException("fixture"));
        });
        await first.Should().ThrowAsync<InvalidOperationException>();
        await faulted.EnsureLoadedAsync(() =>
        {
            faultCalls++;
            return Task.FromResult(true);
        });
        faultCalls.Should().Be(2);
        faulted.HasCompletedLoad.Should().BeTrue();
    }

    [Fact]
    public void Apps_page_and_c_drive_handoff_use_the_lazy_load_instead_of_instructions()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");

        xaml.Should().Contain("Content=\"重新扫描\"");
        xaml.Should().NotContain("Content=\"扫描应用\"");
        code.Should().Contain("_softwareInventoryLoadGate.EnsureLoadedAsync");
        code.Should().Contain("_softwareInventoryLoadGate.RefreshAsync");
        code.Should().Contain("private async void OpenCDriveRootCauseAction_Click");
        code.Should().NotContain("请先执行只读应用扫描");

        var handoffStart = code.IndexOf(
            "case CDriveRootCauseAction.OpenCDriveApps:",
            StringComparison.Ordinal);
        var handoffEnd = code.IndexOf(
            "case CDriveRootCauseAction.ReviewPersonalStorage:",
            handoffStart,
            StringComparison.Ordinal);
        var handoff = code[handoffStart..handoffEnd];
        var sharedHandoff = SourceMethodExtractor.Extract(
            code,
            "private async Task OpenAgentAppCatalogFilterAsync(AppCatalogFilter filter)");

        handoff.Should().Contain("await OpenAgentAppCatalogFilterAsync(AppCatalogFilter.CDrive)")
            .And.NotContain("_softwareProfiles.Count")
            .And.NotContain("RefreshAppCatalog()")
            .And.NotContain("SetAppFilterSelected()");
        AssertBefore(sharedHandoff, "EnsureSoftwareInventoryLoadedAsync", "RefreshAppCatalog");
        sharedHandoff.Should().Contain("AppTilesListBox.Items.Count > 0");
    }

    [Fact]
    public void Automatic_load_is_navigation_only_and_failure_copy_is_path_free()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var showPage = Method(code, "private void ShowPage", "private static void SetNavSelected");
        var ensure = Method(
            code,
            "private Task EnsureSoftwareInventoryLoadedAsync",
            "private Task RefreshSoftwareInventoryAsync");
        var refresh = Method(
            code,
            "private Task RefreshSoftwareInventoryAsync",
            "private async Task<bool> RunSoftwareScanCoreAsync");
        var core = Method(
            code,
            "private async Task<bool> RunSoftwareScanCoreAsync",
            "private Task<IReadOnlyList<SoftwareProfile>> ScanSoftwareProfilesAsync");

        showPage.Should().Contain("EnsureSoftwareInventoryLoadedAsync");
        ensure.Should().Contain("EnsureLoadedAsync");
        refresh.Should().Contain("RefreshAsync");
        core.Should().Contain("ScanSoftwareProfilesAsync");
        core.Should().NotContain("ex.Message");
        string.Join("\n", showPage, ensure, refresh).Should().NotContain("Process.Start");
        string.Join("\n", showPage, ensure, refresh).Should().NotContain("SafetyOperationPipeline");
    }

    [Fact]
    public void Agent_awaits_shared_read_only_inventory_before_evidence_dependent_answers()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var ask = Method(
            code,
            "private async void AskComputerAgent_Click",
            "private void ApplyAgentConversationReply");
        var skill = Method(
            code,
            "private async void AgentSkillAction_Click",
            "private async void AgentConversationNavigate_Click");

        ask.Should().Contain("QuestionNeedsSoftwareInventory");
        ask.Should().Contain("await EnsureSoftwareInventoryLoadedAsync()");
        ask.Should().Contain("AskComputerAgentButton.IsEnabled = false");
        ask.Should().Contain("AskComputerAgentButton.IsEnabled = true");
        AssertBefore(ask, "await EnsureSoftwareInventoryLoadedAsync()", "AgentConversationPresenter.Answer");

        skill.Should().Contain("SkillNeedsSoftwareInventory");
        skill.Should().Contain("await EnsureSoftwareInventoryLoadedAsync()");
        AssertBefore(skill, "await EnsureSoftwareInventoryLoadedAsync()", "AgentConversationPresenter.ExplainSkill");

        string.Join("\n", ask, skill)
            .Should().NotContain("Process.Start")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("File.Delete")
            .And.NotContain("File.Move")
            .And.NotContain("Registry.SetValue");
    }

    private static string Method(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }

    private static void AssertBefore(string source, string first, string second)
    {
        var firstIndex = source.IndexOf(first, StringComparison.Ordinal);
        var secondIndex = source.IndexOf(second, StringComparison.Ordinal);
        firstIndex.Should().BeGreaterThanOrEqualTo(0);
        secondIndex.Should().BeGreaterThan(firstIndex);
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
