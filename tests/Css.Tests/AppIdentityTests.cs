using Css.Core;
using FluentAssertions;

namespace Css.Tests;

public class AppIdentityTests
{
    [Fact]
    public void App_identity_uses_omnix_entropy_brand_and_storage_names()
    {
        AppIdentity.ProductName.Should().Be("OMNIX-Entropy");
        AppIdentity.LocalDataFolderName.Should().Be("OMNIX-Entropy");
        AppIdentity.QuarantineFolderName.Should().Be("Quarantine");
        AppIdentity.DefaultQuarantineRootOnD.Should().Be(@"D:\OMNIX-Entropy\Quarantine");
    }

    [Fact]
    public void App_storage_paths_can_be_isolated_for_gui_smokes_without_touching_user_data()
    {
        var env = new Dictionary<string, string?>
        {
            [AppStoragePathResolver.DataRootEnvironmentVariable] = @"C:\tmp\omnix-smoke-data",
            [AppStoragePathResolver.QuarantineRootEnvironmentVariable] = @"C:\tmp\omnix-smoke-quarantine"
        };

        var paths = AppStoragePathResolver.Resolve(env.GetValueOrDefault, _ => true, @"C:\Users\Me\AppData\Local");

        paths.DatabasePath.Should().Be(@"C:\tmp\omnix-smoke-data\data.db");
        paths.MigrationRollbackRoot.Should().Be(@"C:\tmp\omnix-smoke-data\MigrationRollback");
        paths.InstallRoutingMemoryPath.Should().Be(@"C:\tmp\omnix-smoke-data\install-routing-memory.json");
        paths.QuarantineRoot.Should().Be(@"C:\tmp\omnix-smoke-quarantine");
    }

    [Fact]
    public void App_storage_paths_keep_existing_defaults_when_no_override_is_set()
    {
        var paths = AppStoragePathResolver.Resolve(_ => null, path => path == @"D:\", @"C:\Users\Me\AppData\Local");

        paths.DatabasePath.Should().Be(@"C:\Users\Me\AppData\Local\OMNIX-Entropy\data.db");
        paths.MigrationRollbackRoot.Should().Be(@"C:\Users\Me\AppData\Local\OMNIX-Entropy\MigrationRollback");
        paths.InstallRoutingMemoryPath.Should().Be(@"C:\Users\Me\AppData\Local\OMNIX-Entropy\install-routing-memory.json");
        paths.QuarantineRoot.Should().Be(AppIdentity.DefaultQuarantineRootOnD);
    }

    [Fact]
    public void C_drive_scan_root_override_is_process_scoped_for_gui_smoke_fixtures()
    {
        var env = new Dictionary<string, string?>
        {
            [AppDevelopmentPathResolver.CDriveScanRootEnvironmentVariable] = @"C:\tmp\omnix-cdrive-scan-fixture"
        };

        AppDevelopmentPathResolver.ResolveCDriveScanRoot(@"C:\", env.GetValueOrDefault)
            .Should().Be(@"C:\tmp\omnix-cdrive-scan-fixture");

        AppDevelopmentPathResolver.ResolveCDriveScanRoot(@"C:\", _ => null)
            .Should().Be(@"C:\");
    }

    [Fact]
    public void Software_inventory_fixture_override_is_process_scoped_for_gui_smoke_fixtures()
    {
        var env = new Dictionary<string, string?>
        {
            [AppDevelopmentPathResolver.SoftwareInventoryFixtureEnvironmentVariable] = @"C:\tmp\omnix-software-fixture.json"
        };

        AppDevelopmentPathResolver.ResolveSoftwareInventoryFixturePath(env.GetValueOrDefault)
            .Should().Be(@"C:\tmp\omnix-software-fixture.json");

        AppDevelopmentPathResolver.ResolveSoftwareInventoryFixturePath(_ => null)
            .Should().BeNull();
    }

    [Fact]
    public void Startup_entry_fixture_override_is_process_scoped_for_gui_smoke_fixtures()
    {
        var env = new Dictionary<string, string?>
        {
            [AppDevelopmentPathResolver.StartupEntryFixtureEnvironmentVariable] =
                @"C:\tmp\omnix-startup-fixture.json"
        };

        AppDevelopmentPathResolver.ResolveStartupEntryFixturePath(env.GetValueOrDefault)
            .Should().Be(@"C:\tmp\omnix-startup-fixture.json");
        AppDevelopmentPathResolver.ResolveStartupEntryFixturePath(_ => null)
            .Should().BeNull();
    }

    [Fact]
    public void Personal_storage_fixture_root_is_local_bounded_and_process_scoped()
    {
        var env = new Dictionary<string, string?>
        {
            [AppDevelopmentPathResolver.PersonalStorageFixtureRootEnvironmentVariable] =
                @"C:\tmp\omnix-personal-storage\Downloads"
        };

        AppDevelopmentPathResolver.ResolvePersonalStorageFixtureRoot(env.GetValueOrDefault)
            .Should().Be(@"C:\tmp\omnix-personal-storage\Downloads");
        AppDevelopmentPathResolver.ResolvePersonalStorageFixtureRoot(_ => null)
            .Should().BeNull();
        AppDevelopmentPathResolver.ResolvePersonalStorageFixtureRoot(_ => @"\\server\share")
            .Should().BeNull();
        AppDevelopmentPathResolver.ResolvePersonalStorageFixtureRoot(_ => @"C:\")
            .Should().BeNull();
        AppDevelopmentPathResolver.ResolvePersonalStorageFixtureRoot(_ => @"C:\tmp:stream")
            .Should().BeNull();
    }

    [Fact]
    public void Uninstall_evidence_root_override_is_process_scoped_for_gui_smoke_fixtures()
    {
        var defaultRoot = @"C:\Users\Me\AppData\Local\OMNIX-Entropy\Snapshots\Uninstall";
        var env = new Dictionary<string, string?>
        {
            [AppDevelopmentPathResolver.UninstallEvidenceRootEnvironmentVariable] =
                @"C:\tmp\omnix-uninstall-evidence-fixture"
        };

        AppDevelopmentPathResolver.ResolveUninstallEvidenceRoot(
                defaultRoot,
                env.GetValueOrDefault)
            .Should().Be(@"C:\tmp\omnix-uninstall-evidence-fixture");
        AppDevelopmentPathResolver.ResolveUninstallEvidenceRoot(defaultRoot, _ => null)
            .Should().Be(defaultRoot);
    }
}
