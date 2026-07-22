using Css.Core;
using Css.Core.Timeline;
using Css.Elevated.Uninstall;
using Css.Ipc.Uninstall;
using Css.Snapshot.Uninstall;
using Css.Win32.Processes;
using Css.Win32.Security;

internal static class OfficialUninstallProductionWorker
{
    internal static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var command = OfficialUninstallWorkerCommandLine.Parse(
                args,
                allowFakeOptions: false);
            var workerIdentity = OfficialUninstallWorkerCommandLine.CurrentProcessIdentity();
            var inventory = new SystemOfficialUninstallInventoryReader();
            var handler = new OfficialUninstallOperationHandler(
                new WindowsOfficialUninstallerLauncher(new SystemProcessRunner()),
                manifest => new InventoryOfficialUninstallPostScanner(
                    manifest,
                    inventory.ScanAsync,
                    PathExists,
                    backgroundScanner: new WindowsOfficialUninstallBackgroundScanner(
                        manifest,
                        new SystemWindowsBackgroundEntryReader())),
                new ActionTimelineStore(AppStoragePathResolver.Resolve().DatabasePath),
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256);
            var session = new OfficialUninstallProductionWorkerSession(
                new OfficialUninstallOneShotWorkerServer(
                    new WindowsOfficialUninstallPipePeerIdentityReader()),
                new WindowsOfficialUninstallProductionPackageAuthorizer(
                    new WindowsProcessImagePathResolver(),
                    new WindowsAuthenticodeSignatureVerifier()),
                handler);
            var result = await session.ServeOnceAsync(
                new OfficialUninstallOneShotWorkerOptions
                {
                    PipeName = command.PipeName,
                    SessionId = command.SessionId,
                    ExpectedClient = command.Client,
                    Worker = workerIdentity,
                    Timeout = TimeSpan.FromMilliseconds(command.TimeoutMilliseconds)
                });
            return result.Status == OfficialUninstallTransportStatus.Completed ? 0 : 1;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                $"official-uninstall-production-worker failed: {exception.GetType().Name}");
            return 1;
        }
    }

    private static bool PathExists(string path)
    {
        try
        {
            return File.Exists(path) || Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }
}
