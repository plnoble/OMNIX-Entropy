using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Software;
using Css.Core.Timeline;
using FluentAssertions;
using Css.Win32.Quarantine;

namespace Css.Tests;

public sealed class AppCacheCleanupTests
{
    [Fact]
    public void Plan_accepts_only_cache_named_directory_below_approved_user_root()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "omnix-cache-plan-root"));
        var safeCache = Path.Combine(root, "Example", "Code Cache");
        var outside = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "outside", "Cache"));
        var profile = new SoftwareProfile
        {
            Name = "Example",
            CachePaths = [safeCache, outside]
        };

        var plan = AppCacheCleanupPlanBuilder.Create(
            profile,
            [root],
            path => path.Equals(safeCache, StringComparison.OrdinalIgnoreCase),
            _ => false,
            _ => 512);

        plan.CanContinue.Should().BeTrue();
        plan.Summary.Should().Contain("1 个缓存位置通过校验");
        plan.Summary.Should().NotContain(root).And.NotContain(outside);
        plan.Operation.Should().NotBeNull();
        plan.Operation!.Kind.Should().Be(AppCacheCleanupPlanBuilder.OperationKind);
        plan.Operation.Risk.Should().Be(RiskLevel.Low);
        plan.Operation.RollbackRequired.Should().BeTrue();
        plan.Operation.ConfirmationAccepted.Should().BeFalse();
        plan.Operation.AffectedPaths.Should().Equal(safeCache);
        plan.Operation.EstimatedImpactBytes.Should().Be(512);
    }

    [Fact]
    public void Plan_refuses_running_system_reparse_and_unproven_paths()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "omnix-cache-refusal-root"));
        var cache = Path.Combine(root, "Example", "Cache");
        var running = Profile(cache, runningProcesses: ["Example"]);
        var system = Profile(cache, category: SoftwareCategory.SystemTool);
        var linked = Profile(cache);
        var unknown = Profile(Path.Combine(root, "Example", "User Data"));

        AppCacheCleanupPlanBuilder.Create(running, [root], _ => true, _ => false)
            .CanContinue.Should().BeFalse();
        AppCacheCleanupPlanBuilder.Create(system, [root], _ => true, _ => false)
            .CanContinue.Should().BeFalse();
        AppCacheCleanupPlanBuilder.Create(linked, [root], _ => true, path => path.Equals(cache, StringComparison.OrdinalIgnoreCase))
            .CanContinue.Should().BeFalse();
        AppCacheCleanupPlanBuilder.Create(linked, [root], _ => true, _ => throw new IOException("probe failed"))
            .CanContinue.Should().BeFalse();
        AppCacheCleanupPlanBuilder.Create(unknown, [root], _ => true, _ => false)
            .CanContinue.Should().BeFalse();
    }

    [Fact]
    public void Execution_validation_refuses_a_cache_that_changed_after_confirmation()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "omnix-cache-stale-root"));
        var cache = Path.Combine(root, "Example", "GPUCache");
        var plan = AppCacheCleanupPlanBuilder.Create(Profile(cache), [root], _ => true, _ => false);

        var missing = AppCacheCleanupPlanBuilder.ValidateForExecution(
            plan.Operation!,
            [root],
            _ => false,
            _ => false);
        var becameLink = AppCacheCleanupPlanBuilder.ValidateForExecution(
            plan.Operation!,
            [root],
            _ => true,
            path => path.Equals(cache, StringComparison.OrdinalIgnoreCase));

        missing.Success.Should().BeFalse();
        becameLink.Success.Should().BeFalse();
        AppCacheCleanupPlanBuilder.MatchesCurrentProfile(Profile(cache), plan.Operation!)
            .Should().BeTrue();
        AppCacheCleanupPlanBuilder.MatchesCurrentProfile(
                Profile(Path.Combine(root, "Other", "GPUCache")),
                plan.Operation!)
            .Should().BeFalse();
    }

    [Fact]
    public async Task Confirmed_cache_operation_uses_quarantine_timeline_and_restore()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-cache-operation-" + Guid.NewGuid().ToString("N"));
        var userData = Path.Combine(root, "UserData");
        var cache = Path.Combine(userData, "Example", "Cache");
        var quarantineRoot = Path.Combine(root, "Quarantine");
        var dbPath = Path.Combine(root, "timeline.db");
        Directory.CreateDirectory(cache);
        await File.WriteAllTextAsync(Path.Combine(cache, "entry.bin"), "fixture");

        try
        {
            var plan = AppCacheCleanupPlanBuilder.Create(
                Profile(cache),
                [userData],
                Directory.Exists,
                _ => false,
                _ => 7);
            var reader = new WindowsQuarantineCandidateIdentityReader();
            var preparation = QuarantineOperationPolicy.PrepareForConfirmation(
                plan.Operation!, quarantineRoot, reader);
            preparation.Success.Should().BeTrue(preparation.Error);
            var descriptor = QuarantineOperationPolicy.ConfirmForExecution(preparation.Operation!);
            var quarantine = new FileQuarantineService(quarantineRoot);
            var timeline = new ActionTimelineStore(dbPath);
            var handler = new AppCacheCleanupOperationHandler(
                quarantine,
                timeline,
                [userData],
                Directory.Exists,
                _ => false,
                reader);

            var result = await new SafetyOperationPipeline(handler.ExecuteAsync).ExecuteAsync(descriptor);
            var entries = await timeline.LoadRecentAsync(5);
            var records = await quarantine.LoadRecordsAsync();

            result.Success.Should().BeTrue(result.Error);
            Directory.Exists(cache).Should().BeFalse();
            entries.Should().ContainSingle(entry => entry.RestoreState == RestoreState.Restorable);
            records.Should().ContainSingle();

            var restore = await quarantine.RestoreAsync(records[0].ManifestPath);
            restore.Success.Should().BeTrue();
            Directory.Exists(cache).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Drawer_flow_rechecks_inventory_and_uses_only_the_specialized_handler()
    {
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var host = File.ReadAllText(FindRepositoryFile("src", "Css.Core", "Apps", "AppDrawerActionHost.cs"));
        var quarantine = File.ReadAllText(FindRepositoryFile("src", "Css.Core", "Quarantine", "FileQuarantineService.cs"));
        var quarantineHandler = File.ReadAllText(FindRepositoryFile("src", "Css.Core", "Quarantine", "QuarantineOperationHandler.cs"));
        var execute = Extract(
            main,
            "private async Task ExecutePendingAppCacheCleanupAsync()",
            "private async void ReviewUninstallResidue_Click");

        host.Should().Contain("PrimaryActionKey = plan.CanContinue ? \"CacheCleanup\"");
        host.Should().Contain("PrimaryActionKey = \"Timeline\"");
        execute.Should().Contain("await ScanSoftwareProfilesAsync()");
        execute.Should().Contain("AppDrawerTargetResolver.Resolve");
        execute.Should().Contain("resolution.Profile.RunningProcesses.Count > 0");
        execute.Should().Contain("AppCacheCleanupPlanBuilder.MatchesCurrentProfile");
        execute.Should().Contain("AppCacheCleanupPlanBuilder.ValidateForExecution");
        execute.Should().Contain("new AppCacheCleanupOperationHandler(");
        execute.Should().Contain("new SafetyOperationPipeline(handler.ExecuteAsync)");
        execute.Should().Contain("CleanupConfirmationWindow");
        execute.Should().Contain("await RefreshCacheCleanupStateAfterAttemptAsync()");
        execute.Should().NotContain("await LoadTimelineAsync()");
        execute.Should().NotContain("Process.Start").And.NotContain("Registry.");
        quarantine.IndexOf("await WriteManifestAsync(record, ct);", StringComparison.Ordinal)
            .Should().BeLessThan(quarantine.IndexOf("File.Move(originalPath, quarantinedPath);", StringComparison.Ordinal));
        quarantine.IndexOf("await WriteManifestAsync(record, ct);", StringComparison.Ordinal)
            .Should().BeLessThan(quarantine.IndexOf("Directory.Move(originalPath, quarantinedPath);", StringComparison.Ordinal));
        quarantineHandler.IndexOf("await _timeline.AddAsync(new ActionTimelineEntry", StringComparison.Ordinal)
            .Should().BeLessThan(quarantineHandler.IndexOf("catch", StringComparison.Ordinal));
        quarantineHandler.Should().Contain("await _quarantine.RestoreAsync(record.ManifestPath, CancellationToken.None)");
    }

    [Fact]
    public void Drawer_refreshes_timeline_and_inventory_after_every_pipeline_attempt()
    {
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var execute = Extract(
            main,
            "private async Task ExecutePendingAppCacheCleanupAsync()",
            "private async void ReviewUninstallResidue_Click");
        var helper = SourceMethodExtractor.Extract(
            main,
            "private async Task RefreshCacheCleanupStateAfterAttemptAsync()");

        const string attempted = "pipelineAttempted = true;";
        const string executePipeline = "await pipeline.ExecuteAsync(descriptor)";
        const string synchronize = "await RefreshCacheCleanupStateAfterAttemptAsync();";
        const string successGate = "if (!result.Success)";
        execute.Should().Contain("var pipelineAttempted = false;");
        execute.Should().Contain("var stateSynchronized = false;");
        execute.IndexOf(attempted, StringComparison.Ordinal)
            .Should().BeLessThan(execute.IndexOf(executePipeline, StringComparison.Ordinal));
        execute.IndexOf(executePipeline, StringComparison.Ordinal)
            .Should().BeLessThan(execute.IndexOf(synchronize, StringComparison.Ordinal));
        execute.IndexOf(synchronize, StringComparison.Ordinal)
            .Should().BeLessThan(execute.IndexOf(successGate, StringComparison.Ordinal));
        execute.Should().Contain("if (pipelineAttempted && !stateSynchronized)");
        execute.Split(synchronize, StringSplitOptions.None).Length.Should().Be(3);

        helper.Should().Contain("await LoadTimelineAsync();");
        helper.Should().Contain("SetSoftwareProfiles(await ScanSoftwareProfilesAsync());");
        helper.Should().NotContain("SafetyOperationPipeline");
        helper.Should().NotContain("QuarantineAsync");
        helper.Should().NotContain("RestoreAsync");
        helper.Should().NotContain("PurgeAsync");
        helper.Should().NotContain("File.Delete");
        helper.Should().NotContain("Directory.Delete");
    }

    private static SoftwareProfile Profile(
        string cache,
        SoftwareCategory category = SoftwareCategory.Normal,
        IReadOnlyList<string>? runningProcesses = null) =>
        new()
        {
            Name = "Example",
            Category = category,
            CachePaths = [cache],
            RunningProcesses = runningProcesses ?? []
        };

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
}
