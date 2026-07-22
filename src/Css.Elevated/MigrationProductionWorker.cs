using Css.Core;
using Css.Core.Migration;
using Css.Elevated.Migration;
using Css.Elevated.Uninstall;
using Css.Ipc.Migration;
using Css.Ipc.Uninstall;
using Css.Win32.Migration;
using Css.Win32.Processes;
using Css.Win32.Security;

internal static class MigrationProductionWorker
{
    internal static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var command = OfficialUninstallWorkerCommandLine.Parse(
                args,
                allowFakeOptions: false);
            var workerIdentity = OfficialUninstallWorkerCommandLine.CurrentProcessIdentity();
            var storage = AppStoragePathResolver.Resolve();
            var pathPolicy = new WindowsMigrationPathPolicy();
            var pathAdapter = new WindowsDirectoryMigrationPathAdapter(
                pathPolicy,
                new WindowsDirectorySymbolicLinkRedirector());
            var handler = new MigrationOperationHandler(
                new WindowsMigrationActivityProbe(),
                pathAdapter,
                pathPolicy,
                new WindowsMigrationSnapshotSourceReader(),
                new JsonMigrationMonitoringStore(
                    Path.Combine(storage.MigrationRollbackRoot, "Monitoring")));
            var packageAuthorizer = new WindowsMigrationProductionPackageAuthorizer(
                new WindowsOfficialUninstallProductionPackageAuthorizer(
                    new WindowsProcessImagePathResolver(),
                    new WindowsAuthenticodeSignatureVerifier()));
            var session = new MigrationProductionWorkerSession(
                new MigrationOneShotWorkerServer(
                    new WindowsOfficialUninstallPipePeerIdentityReader()),
                packageAuthorizer,
                handler);
            var result = await session.ServeOnceAsync(
                new MigrationOneShotWorkerOptions
                {
                    PipeName = command.PipeName,
                    SessionId = command.SessionId,
                    ExpectedClient = command.Client,
                    Worker = workerIdentity,
                    Timeout = TimeSpan.FromMilliseconds(command.TimeoutMilliseconds)
                });
            return result.Status == MigrationTransportStatus.Completed ? 0 : 1;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                $"migration-production-worker failed: {exception.GetType().Name}");
            return 1;
        }
    }
}
