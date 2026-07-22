using FluentAssertions;

namespace Css.Tests;

public sealed class ReleaseWorkerCommandSurfaceTests
{
    [Fact]
    public void Fake_worker_mode_is_debug_guarded_and_removed_from_release_compile()
    {
        var program = Read("src", "Css.Elevated", "Program.cs");
        var project = Read("src", "Css.Elevated", "Css.Elevated.csproj");

        var fakeMode = program.IndexOf(
            "official-uninstall-fake-worker",
            StringComparison.Ordinal);
        var debugStart = program.LastIndexOf(
            "#if DEBUG",
            fakeMode,
            StringComparison.Ordinal);
        var debugEnd = program.IndexOf(
            "#endif",
            fakeMode,
            StringComparison.Ordinal);
        var uninstallProduction = program.IndexOf(
            "official-uninstall-production-worker",
            StringComparison.Ordinal);
        var migrationProduction = program.IndexOf(
            "migration-production-worker",
            StringComparison.Ordinal);

        fakeMode.Should().BeGreaterThanOrEqualTo(0);
        debugStart.Should().BeGreaterThanOrEqualTo(0);
        debugEnd.Should().BeGreaterThan(fakeMode);
        uninstallProduction.Should().BeGreaterThan(debugEnd);
        migrationProduction.Should().BeGreaterThan(debugEnd);
        project.Should().Contain("Condition=\"'$(Configuration)' == 'Release'\"");
        project.Should().Contain("<Compile Remove=\"OfficialUninstallFakeWorker.cs\" />");
    }

    [Fact]
    public void Portable_package_refuses_a_release_worker_with_debug_command_metadata()
    {
        var script = Read("scripts", "publish-portable-test-package.ps1");

        script.Should().Contain("Css.Elevated.dll");
        script.Should().Contain("official-uninstall-fake-worker");
        script.Should().Contain("[Text.Encoding]::UTF8.GetBytes");
        script.Should().Contain("[Text.Encoding]::Unicode.GetBytes");
        script.Should().Contain("Release worker includes debug-only command surface");
        script.Should().Contain("ReleaseCommandSurface = \"ProductionOnly\"");
        script.Should().NotContain("Start-Process");
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
