using Css.Core.Migration;
using Css.Win32.Migration;
using FluentAssertions;

namespace Css.Tests;

public sealed class WindowsDirectoryMigrationPathAdapterTests
{
    [Fact]
    public async Task Copier_preserves_nested_files_and_detects_later_tampering()
    {
        using var fixture = new DirectoryFixture();
        var source = fixture.Path("source");
        var destination = fixture.Path("destination");
        Directory.CreateDirectory(Path.Combine(source, "nested", "empty"));
        await File.WriteAllTextAsync(Path.Combine(source, "root.txt"), "root-content");
        await File.WriteAllBytesAsync(
            Path.Combine(source, "nested", "payload.bin"),
            Enumerable.Range(0, 4096).Select(value => (byte)(value % 251)).ToArray());
        var copier = new WindowsDirectoryCopyVerifier();

        await copier.CopyVerifiedAsync(source, destination);

        File.ReadAllText(Path.Combine(destination, "root.txt")).Should().Be("root-content");
        File.Exists(Path.Combine(destination, "nested", "payload.bin")).Should().BeTrue();
        Directory.Exists(Path.Combine(destination, "nested", "empty")).Should().BeTrue();
        await copier.VerifyEqualAsync(source, destination);

        await File.AppendAllTextAsync(Path.Combine(destination, "root.txt"), "tampered");
        var verify = () => copier.VerifyEqualAsync(source, destination);
        await verify.Should().ThrowAsync<IOException>();
    }

    [Fact]
    public async Task Adapter_moves_verified_directory_creates_redirect_and_rolls_back()
    {
        using var fixture = new DirectoryFixture();
        var source = fixture.Path("source", "app");
        var destination = fixture.Path("destination", "app");
        Directory.CreateDirectory(Path.Combine(source, "data"));
        await File.WriteAllTextAsync(Path.Combine(source, "data", "state.db"), "verified-state");
        var redirects = new FakeRedirector();
        var adapter = new WindowsDirectoryMigrationPathAdapter(
            new WindowsMigrationPathPolicy(),
            redirects);
        var entry = Entry(source, destination);

        var moved = await adapter.MoveAndRedirectAsync(entry);

        moved.RedirectCreated.Should().BeTrue();
        Directory.Exists(source).Should().BeFalse("the fake redirect is held by the injected primitive");
        Directory.Exists(destination).Should().BeTrue();
        File.ReadAllText(Path.Combine(destination, "data", "state.db")).Should().Be("verified-state");
        var observation = await adapter.ObserveAsync(source);
        observation.IsRedirect.Should().BeTrue();
        observation.RedirectTarget.Should().Be(destination);
        fixture.StagingDirectories().Should().BeEmpty();

        await adapter.RollbackAsync(entry);

        redirects.TryGetTarget(source, out _).Should().BeFalse();
        Directory.Exists(source).Should().BeTrue();
        Directory.Exists(destination).Should().BeFalse();
        File.ReadAllText(Path.Combine(source, "data", "state.db")).Should().Be("verified-state");
        fixture.StagingDirectories().Should().BeEmpty();
    }

    [Fact]
    public async Task Redirect_failure_after_source_removal_can_be_restored_without_staging_residue()
    {
        using var fixture = new DirectoryFixture();
        var source = fixture.Path("source", "cache");
        var destination = fixture.Path("destination", "cache");
        Directory.CreateDirectory(source);
        await File.WriteAllTextAsync(Path.Combine(source, "cache.bin"), "cache-data");
        var redirects = new FakeRedirector { FailCreate = true };
        var adapter = new WindowsDirectoryMigrationPathAdapter(
            new WindowsMigrationPathPolicy(),
            redirects);
        var entry = Entry(source, destination);

        var move = () => adapter.MoveAndRedirectAsync(entry);
        await move.Should().ThrowAsync<IOException>();
        Directory.Exists(source).Should().BeFalse();
        Directory.Exists(destination).Should().BeTrue();

        await adapter.RollbackAsync(entry);

        Directory.Exists(source).Should().BeTrue();
        Directory.Exists(destination).Should().BeFalse();
        File.ReadAllText(Path.Combine(source, "cache.bin")).Should().Be("cache-data");
        fixture.StagingDirectories().Should().BeEmpty();
    }

    [Fact]
    public async Task Destination_collision_stops_before_copy_or_source_removal()
    {
        using var fixture = new DirectoryFixture();
        var source = fixture.Path("source");
        var destination = fixture.Path("destination");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(destination);
        await File.WriteAllTextAsync(Path.Combine(source, "source.txt"), "source");
        await File.WriteAllTextAsync(Path.Combine(destination, "existing.txt"), "existing");
        var adapter = new WindowsDirectoryMigrationPathAdapter(
            new WindowsMigrationPathPolicy(),
            new FakeRedirector());

        var move = () => adapter.MoveAndRedirectAsync(Entry(source, destination));

        await move.Should().ThrowAsync<IOException>();
        File.ReadAllText(Path.Combine(source, "source.txt")).Should().Be("source");
        File.ReadAllText(Path.Combine(destination, "existing.txt")).Should().Be("existing");
        fixture.StagingDirectories().Should().BeEmpty();
    }

    [Fact]
    public void Copier_source_reparse_check_is_mandatory_and_adapter_has_no_shell_redirect()
    {
        var source = Read("src", "Css.Win32", "Migration", "WindowsDirectoryMigrationPathAdapter.cs");

        source.Should().Contain("FileAttributes.ReparsePoint");
        source.Should().Contain("Migration does not follow reparse points");
        source.Should().Contain("FixedTimeEquals");
        source.Should().Contain("CopyVerifiedAsync(source, staging");
        source.Should().Contain("VerifyEqualAsync(source, destination");
        source.Should().Contain("Directory.Delete(source, recursive: true)");
        source.IndexOf("VerifyEqualAsync(source, destination", StringComparison.Ordinal)
            .Should().BeLessThan(
                source.IndexOf("Directory.Delete(source, recursive: true)", StringComparison.Ordinal));
        source.Should().NotContain("cmd.exe");
        source.Should().NotContain("mklink");
        source.Should().NotContain("Process.Start");
    }

    private static MigrationRollbackManifestEntry Entry(string source, string destination) =>
        new()
        {
            OriginalPath = source,
            PlannedDestinationPath = destination,
            RestorePath = source,
            Reason = "fixture"
        };

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

    private sealed class FakeRedirector : IWindowsDirectoryRedirector
    {
        private readonly Dictionary<string, string> _targets =
            new(StringComparer.OrdinalIgnoreCase);

        public bool FailCreate { get; init; }

        public void Create(string originalPath, string destinationPath)
        {
            if (FailCreate)
                throw new IOException("Injected redirect failure.");
            _targets[Key(originalPath)] = Path.GetFullPath(destinationPath);
        }

        public bool TryGetTarget(string originalPath, out string? destinationPath)
        {
            var found = _targets.TryGetValue(Key(originalPath), out var value);
            destinationPath = value;
            return found;
        }

        public void Remove(string originalPath)
        {
            if (!_targets.Remove(Key(originalPath)))
                throw new IOException("Injected redirect was missing.");
        }

        private static string Key(string path) => Path.GetFullPath(path);
    }

    private sealed class DirectoryFixture : IDisposable
    {
        public DirectoryFixture()
        {
            Root = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "omnix-windows-migration-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public string Path(params string[] parts) =>
            System.IO.Path.Combine([Root, .. parts]);

        public IReadOnlyList<string> StagingDirectories() =>
            Directory.EnumerateDirectories(
                    Root,
                    "*.omnix-*",
                    SearchOption.AllDirectories)
                .ToArray();

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
