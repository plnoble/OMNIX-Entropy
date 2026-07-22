using Css.Core.Operations;
using Css.Core.Uninstall;
using Css.Ipc.Uninstall;

internal static class OfficialUninstallFakeWorker
{
    internal static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var options = OfficialUninstallWorkerCommandLine.Parse(
                args,
                allowFakeOptions: true);
            var result = await new OfficialUninstallOneShotWorkerServer(
                    new WindowsOfficialUninstallPipePeerIdentityReader())
                .ServeFakeOnceAsync(
                    new OfficialUninstallOneShotWorkerOptions
                    {
                        PipeName = options.PipeName,
                        SessionId = options.SessionId,
                        ExpectedClient = options.Client,
                        Worker = OfficialUninstallWorkerCommandLine.CurrentProcessIdentity(),
                        Timeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds)
                    },
                    async (request, cancellationToken) =>
                    {
                        if (options.ResponseDelayMilliseconds > 0)
                            await Task.Delay(options.ResponseDelayMilliseconds, cancellationToken);
                        return FakeResponse(request.RequestId!);
                    });

            if (options.ExitDelayMilliseconds > 0)
                await Task.Delay(options.ExitDelayMilliseconds);
            return result.Status == OfficialUninstallTransportStatus.Completed ? 0 : 1;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                $"official-uninstall-fake-worker failed: {exception.GetType().Name}");
            return 1;
        }
    }

    private static OfficialUninstallElevatedResponseEnvelope FakeResponse(string requestId) =>
        new()
        {
            RequestId = requestId,
            Result = OperationResult.Ok(payload: new OfficialUninstallHandlerPayload
            {
                UninstallerStarted = false,
                UninstallerCompleted = false,
                ExitCode = null,
                RequiresPostScanRetry = false,
                PostScan = OfficialUninstallPostScanResult.NotRun(
                    "Fake worker verification only; no uninstaller was started.")
            })
        };

}
