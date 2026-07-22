using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallProductionWorkerModeTests
{
    [Fact]
    public void Production_mode_is_elevated_only_and_composes_the_narrow_real_authority()
    {
        var program = Read("src", "Css.Elevated", "Program.cs");
        var worker = Read("src", "Css.Elevated", "OfficialUninstallProductionWorker.cs");
        var parser = Read("src", "Css.Elevated", "OfficialUninstallWorkerCommandLine.cs");
        var app = Read("src", "Css.App", "App.xaml.cs");
        var appLauncher = Read("src", "Css.App", "OfficialUninstallWorkerLauncher.cs");

        program.Should().Contain("official-uninstall-production-worker");
        worker.Should().Contain("OfficialUninstallProductionWorkerSession");
        worker.Should().Contain("WindowsOfficialUninstallProductionPackageAuthorizer");
        worker.Should().Contain("WindowsOfficialUninstallerLauncher");
        worker.Should().Contain("InventoryOfficialUninstallPostScanner");
        worker.Should().Contain("SystemOfficialUninstallInventoryReader");
        worker.Should().Contain("WindowsOfficialUninstallBackgroundScanner");
        worker.Should().Contain("allowFakeOptions: false");
        parser.Should().Contain("FakeOnlyNames");
        app.Should().NotContain("official-uninstall-production-worker");
        app.Should().NotContain("OfficialUninstallProductionWorker");
        appLauncher.Should().Contain("official-uninstall-production-worker");
        appLauncher.Should().Contain("WindowsOfficialUninstallProductionWorkerLauncher");
        appLauncher.Should().Contain("trust.CanLaunchProduction");
    }

    [Fact]
    public void Production_mode_and_read_only_scanners_have_no_residue_mutation_authority()
    {
        var worker = Read("src", "Css.Elevated", "OfficialUninstallProductionWorker.cs");
        var inventory = Read(
            "src", "Css.Elevated", "Uninstall", "SystemOfficialUninstallInventoryReader.cs");
        var postScan = Read(
            "src", "Css.Elevated", "Uninstall", "InventoryOfficialUninstallPostScanner.cs");
        var project = Read("src", "Css.Elevated", "Css.Elevated.csproj");
        var combined = worker + inventory + postScan;

        inventory.Should().Contain("writable: false");
        combined.Should().NotContain("File.Delete");
        combined.Should().NotContain("File.Move");
        combined.Should().NotContain("Directory.Delete");
        combined.Should().NotContain("DeleteValue");
        combined.Should().NotContain("DeleteSubKey");
        combined.Should().NotContain("Quarantine");
        project.Should().NotContain("Css.Scanner");
    }

    [Fact]
    public void Real_process_start_remains_buried_behind_package_request_and_pipeline_gates()
    {
        var server = Read(
            "src", "Css.Ipc", "Uninstall", "OfficialUninstallOneShotWorkerServer.cs");
        var session = Read(
            "src", "Css.Elevated", "Uninstall", "OfficialUninstallProductionWorkerSession.cs");
        var handler = Read(
            "src", "Css.Elevated", "Uninstall", "OfficialUninstallOperationHandler.cs");
        var runner = Read(
            "src", "Css.Elevated", "Uninstall", "SystemProcessRunner.cs");

        server.IndexOf("authorization(actualClient", StringComparison.Ordinal)
            .Should().BeLessThan(server.IndexOf(
                "OfficialUninstallSessionBootstrapServer",
                StringComparison.Ordinal));
        session.Should().Contain("SafetyOperationPipeline");
        session.Should().Contain("IsFresh(request.PreparedAtUtc)");
        handler.Should().Contain("FixedScannerFactory");
        handler.Should().Contain("RequiresElevation = false");
        handler.Should().NotContain("Process.Start");
        runner.Should().Contain("Process.Start");
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
