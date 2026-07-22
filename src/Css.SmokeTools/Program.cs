using System.Text.Json;
using Css.Core;
using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Software;
using Css.Core.Startup;
using Css.Core.Timeline;
using Css.Win32.Quarantine;

if (args.Length > 0
    && args[0].Equals("official-uninstall-ipc-worker", StringComparison.OrdinalIgnoreCase))
{
    return await OfficialUninstallIpcWorker.RunAsync(args[1..]);
}

if (args.Length == 1 && args[0].Equals("seed-undo-center", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var seeded = await SeedUndoCenterAsync();
        Console.WriteLine(JsonSerializer.Serialize(seeded));
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 1;
    }
}

if (args.Length == 1 && args[0].Equals("seed-startup-undo-center", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var seeded = await SeedStartupUndoCenterAsync();
        Console.WriteLine(JsonSerializer.Serialize(seeded));
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 1;
    }
}

Console.Error.WriteLine(
    "Usage: Css.SmokeTools seed-undo-center | seed-startup-undo-center | official-uninstall-ipc-worker <bounded metadata>");
return 2;

static async Task<object> SeedUndoCenterAsync()
{
    var paths = AppStoragePathResolver.Resolve();
    var seedSourceRoot = Path.Combine(Path.GetDirectoryName(paths.DatabasePath)!, "smoke-seed-source");
    var sourceFile = Path.Combine(seedSourceRoot, "restorable-cache.tmp");

    Directory.CreateDirectory(seedSourceRoot);
    await File.WriteAllTextAsync(sourceFile, "OMNIX-Entropy undo center smoke seed.");

    var quarantine = new FileQuarantineService(paths.QuarantineRoot);
    var timeline = new ActionTimelineStore(paths.DatabasePath);
    var identityReader = new WindowsQuarantineCandidateIdentityReader();

    var descriptor = new OperationDescriptor
    {
        Kind = "clean.temp",
        Title = "\u540E\u6094\u836F\u70DF\u6D4B\u8BB0\u5F55",
        Source = OperationSource.System,
        Risk = RiskLevel.Low,
        IsDestructive = true,
        RollbackRequired = true,
        ConfirmationAccepted = false,
        EvidenceSummary = "\u9694\u79BB GUI \u70DF\u6D4B\u4E34\u65F6\u6587\u4EF6",
        EstimatedImpactBytes = new FileInfo(sourceFile).Length,
        AffectedPaths = [sourceFile]
    };

    var preparation = QuarantineOperationPolicy.PrepareForConfirmation(
        descriptor,
        paths.QuarantineRoot,
        identityReader);
    if (!preparation.Success || preparation.Operation is null)
        throw new InvalidOperationException(preparation.Error ?? "Failed to prepare undo center smoke data.");

    var confirmed = QuarantineOperationPolicy.ConfirmForExecution(preparation.Operation);
    var handler = new QuarantineOperationHandler(quarantine, timeline, identityReader);
    var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

    var result = await pipeline.ExecuteAsync(confirmed);
    if (!result.Success)
        throw new InvalidOperationException(result.Error ?? "Failed to seed undo center smoke data.");

    var entry = (await timeline.LoadRecentAsync(1)).SingleOrDefault()
        ?? throw new InvalidOperationException("Seeded timeline entry was not written.");

    if (entry.RestoreState != RestoreState.Restorable
        || !string.Equals(entry.RestoreOperationKind, "quarantine.restore", StringComparison.OrdinalIgnoreCase)
        || entry.RestoreManifestPaths.Count == 0)
    {
        throw new InvalidOperationException("Seeded timeline entry is not restorable.");
    }

    return new
    {
        entry.Id,
        entry.Title,
        entry.RestoreState,
        entry.RestoreManifestPaths,
        paths.DatabasePath,
        paths.QuarantineRoot
    };
}

static async Task<object> SeedStartupUndoCenterAsync()
{
    var paths = AppStoragePathResolver.Resolve();
    var now = DateTimeOffset.UtcNow;
    const string valueName = "OMNIX Startup Restore Fixture";
    var approval = StartupApprovalObservationFactory.FromRegistryValue(
        StartupEntryControlPolicy.SupportedApprovalLocator,
        valueName,
        new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
    var observation = BackgroundComponentObservationFactory.Startup(
        valueName,
        StartupEntryControlPolicy.SupportedSourceLocator,
        "fixture.exe --background",
        now,
        approval);
    var state = StartupEntryStateFactory.Create(
        observation,
        StartupRegistryValueKind.String,
        "fixture.exe --background",
        new string('A', 64),
        now);
    var preparation = new StartupControlPreparation
    {
        Status = StartupControlPreparationStatus.Ready,
        SoftwareName = "Startup Restore Fixture",
        Summary = "Fixture-only startup restore evidence.",
        Lines = ["Fixture-only startup restore evidence."],
        State = state
    };
    var manifestRoot = Path.Combine(
        Path.GetDirectoryName(paths.DatabasePath)!,
        "StartupRollback",
        "Manifests");
    var manifests = new StartupRollbackManifestStore(manifestRoot);
    var manifest = await manifests.CreateAsync(preparation, now);
    var candidate = StartupEntryControlOperationPolicy.CreateDisablePlan(preparation, manifest);
    var descriptor = StartupEntryControlOperationPolicy.ConfirmForExecution(candidate);
    var timeline = new ActionTimelineStore(paths.DatabasePath);
    var store = new SeedStartupEntryStore(state);
    var handler = new StartupEntryControlOperationHandler(store, manifests, timeline, () => now);
    var result = await new SafetyOperationPipeline(handler.ExecuteAsync).ExecuteAsync(descriptor);
    if (!result.Success)
        throw new InvalidOperationException(result.Error ?? "Failed to seed startup undo-center data.");

    var entry = (await timeline.LoadRecentAsync(1)).SingleOrDefault()
        ?? throw new InvalidOperationException("Seeded startup timeline entry was not written.");
    if (entry.RestoreState != RestoreState.Restorable
        || !string.Equals(
            entry.RestoreOperationKind,
            StartupEntryControlOperationPolicy.RestoreKind,
            StringComparison.OrdinalIgnoreCase)
        || entry.RestoreManifestPaths.Count != 1)
    {
        throw new InvalidOperationException("Seeded startup timeline entry is not restorable.");
    }

    return new
    {
        entry.Id,
        entry.Title,
        entry.RestoreState,
        entry.RestoreOperationKind,
        entry.RestoreManifestPaths,
        manifestRoot,
        paths.DatabasePath
    };
}

sealed class SeedStartupEntryStore(StartupEntryState state) : IStartupEntryControlStore
{
    public Task<StartupEntryCaptureResult> CaptureAsync(
        BackgroundComponentObservation observation,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(StartupEntryCaptureResult.Completed(state));

    public Task<StartupEntryMutationResult> DisableAsync(
        StartupEntryState expected,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(StartupEntryMutationResult.Completed("Fixture startup entry disabled in memory."));

    public Task<StartupEntryMutationResult> RestoreAsync(
        StartupEntryState expected,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(StartupEntryMutationResult.Completed("Fixture startup entry restored in memory."));
}
