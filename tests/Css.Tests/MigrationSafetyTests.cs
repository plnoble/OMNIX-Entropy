using Css.Core.Migration;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public class MigrationSafetyTests
{
    [Fact]
    public void Migration_rollback_manifest_is_plan_only_and_maps_original_paths_to_destinations()
    {
        var root = CreateTempRoot();
        try
        {
            var installPath = Path.Combine(root, "CDrive", "Users", "Me", "Ollama");
            var cachePath = Path.Combine(root, "CDrive", "Users", "Me", ".ollama", "models");
            Directory.CreateDirectory(installPath);
            Directory.CreateDirectory(cachePath);
            File.WriteAllText(Path.Combine(installPath, "ollama.exe"), "binary");
            File.WriteAllText(Path.Combine(cachePath, "model.bin"), "model");
            var profile = CreateProfile(installPath, cachePath);
            var plan = MigrationPlanner.CreatePlan(profile, Path.Combine(root, "DDrive", "Agent", "Ollama", "Install"), snapshotAvailable: false);

            var manifest = MigrationRollbackManifestBuilder.Build(
                profile,
                plan,
                snapshotId: "snapshot-ollama",
                now: new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero));

            manifest.IsPlanOnly.Should().BeTrue();
            manifest.SoftwareName.Should().Be("Ollama");
            manifest.SnapshotId.Should().Be("snapshot-ollama");
            manifest.DestinationRoot.Should().Be(plan.DestinationRoot);
            manifest.Entries.Should().Contain(entry =>
                entry.OriginalPath == installPath &&
                entry.PlannedDestinationPath == plan.DestinationRoot &&
                entry.RestorePath == installPath);
            manifest.Entries.Should().Contain(entry =>
                entry.OriginalPath == cachePath &&
                entry.PlannedDestinationPath.Contains("MigratedData"));
            manifest.MonitorPaths.Should().Contain(cachePath);
            Directory.Exists(installPath).Should().BeTrue("building a rollback manifest must not move the app");
            Directory.Exists(cachePath).Should().BeTrue("building a rollback manifest must not move cache");
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Migration_rollback_manifest_store_writes_and_reads_json_without_touching_source_paths()
    {
        var root = CreateTempRoot();
        try
        {
            var installPath = Path.Combine(root, "CDrive", "Apps", "Example");
            var cachePath = Path.Combine(root, "CDrive", "Users", "Me", "ExampleCache");
            Directory.CreateDirectory(installPath);
            Directory.CreateDirectory(cachePath);
            File.WriteAllText(Path.Combine(cachePath, "cache.bin"), "cache");
            var profile = CreateProfile(installPath, cachePath);
            var plan = MigrationPlanner.CreatePlan(profile, Path.Combine(root, "DDrive", "Software", "Example", "Install"), snapshotAvailable: false);
            var manifest = MigrationRollbackManifestBuilder.Build(
                profile,
                plan,
                snapshotId: "snapshot-json",
                now: DateTimeOffset.Parse("2026-07-01T10:30:00+08:00"));
            var manifestPath = Path.Combine(root, "rollback", "example.migration.json");

            await MigrationRollbackManifestStore.WriteAsync(manifest, manifestPath);
            var loaded = await MigrationRollbackManifestStore.ReadAsync(manifestPath);

            File.Exists(manifestPath).Should().BeTrue();
            loaded.Should().NotBeNull();
            loaded!.SoftwareName.Should().Be("Ollama");
            loaded.Entries.Select(entry => entry.OriginalPath).Should().Contain([installPath, cachePath]);
            Directory.Exists(installPath).Should().BeTrue();
            Directory.Exists(cachePath).Should().BeTrue();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Migration_rollback_manifest_creation_writes_json_evidence_without_moving_sources()
    {
        var root = CreateTempRoot();
        try
        {
            var installPath = Path.Combine(root, "CDrive", "Apps", "Ollama");
            var cachePath = Path.Combine(root, "CDrive", "Users", "Me", ".ollama", "models");
            Directory.CreateDirectory(installPath);
            Directory.CreateDirectory(cachePath);
            File.WriteAllText(Path.Combine(installPath, "ollama.exe"), "binary");
            File.WriteAllText(Path.Combine(cachePath, "model.bin"), "model");
            var profile = CreateProfile(installPath, cachePath);
            var plan = MigrationPlanner.CreatePlan(profile, Path.Combine(root, "DDrive", "Agent", "Ollama", "Install"), snapshotAvailable: false);
            var manifestPath = Path.Combine(root, "rollback", "ollama.migration.json");

            var result = await MigrationRollbackManifestCreationService.CreateAsync(
                profile,
                plan,
                manifestPath,
                snapshotId: "snapshot-before-migration",
                now: DateTimeOffset.Parse("2026-07-01T12:00:00+08:00"));

            result.ManifestPath.Should().Be(manifestPath);
            result.Manifest.IsPlanOnly.Should().BeTrue();
            result.Manifest.Entries.Select(entry => entry.OriginalPath).Should().Contain([installPath, cachePath]);
            File.Exists(manifestPath).Should().BeTrue();
            Directory.Exists(installPath).Should().BeTrue("creating rollback evidence must not move the app");
            Directory.Exists(cachePath).Should().BeTrue("creating rollback evidence must not move app cache");
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void Migration_destination_space_probe_reports_ready_or_blocked_without_writing_to_disk()
    {
        var ready = MigrationDestinationSpaceProbe.Check(
            destinationRoot: @"D:\Agent\Ollama\Install",
            requiredBytes: 10L * 1024 * 1024 * 1024,
            availableBytesProvider: root => root == @"D:\" ? 20L * 1024 * 1024 * 1024 : 0);
        var blocked = MigrationDestinationSpaceProbe.Check(
            destinationRoot: @"D:\Agent\Ollama\Install",
            requiredBytes: 30L * 1024 * 1024 * 1024,
            availableBytesProvider: _ => 20L * 1024 * 1024 * 1024);

        ready.CanCheck.Should().BeTrue();
        ready.HasEnoughSpace.Should().BeTrue();
        ready.DriveRoot.Should().Be(@"D:\");
        ready.AvailableBytes.Should().Be(20L * 1024 * 1024 * 1024);
        blocked.CanCheck.Should().BeTrue();
        blocked.HasEnoughSpace.Should().BeFalse();
        blocked.Summary.Should().Contain("not enough");
    }

    [Fact]
    public void Migration_destination_space_probe_degrades_when_drive_cannot_be_checked()
    {
        var result = MigrationDestinationSpaceProbe.Check(
            destinationRoot: @"Z:\Missing\App",
            requiredBytes: 1024,
            availableBytesProvider: _ => throw new IOException("drive unavailable"));

        result.CanCheck.Should().BeFalse();
        result.HasEnoughSpace.Should().BeFalse();
        result.Error.Should().Contain("drive unavailable");
    }

    private static SoftwareProfile CreateProfile(string installPath, string cachePath) =>
        new()
        {
            Name = "Ollama",
            Category = SoftwareCategory.Ai,
            InstallPath = installPath,
            InstalledSizeBytes = 1024,
            CacheSizeBytes = 2048,
            CachePaths = [cachePath],
            CDriveWritePaths = [installPath, cachePath],
            Services = ["OllamaService"],
            RunningProcesses = ["ollama"]
        };

    private static string CreateTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-migration-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
