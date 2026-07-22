#if DEBUG
if (args.Length > 0
    && args[0].Equals("official-uninstall-fake-worker", StringComparison.Ordinal))
{
    return await OfficialUninstallFakeWorker.RunAsync(args[1..]);
}
#endif

if (args.Length > 0
    && args[0].Equals("official-uninstall-production-worker", StringComparison.Ordinal))
{
    return await OfficialUninstallProductionWorker.RunAsync(args[1..]);
}

if (args.Length > 0
    && args[0].Equals("migration-production-worker", StringComparison.Ordinal))
{
    return await MigrationProductionWorker.RunAsync(args[1..]);
}

Console.Error.WriteLine("No elevated operation mode was selected.");
return 2;
