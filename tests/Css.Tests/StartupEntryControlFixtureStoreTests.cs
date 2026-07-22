using Css.Core.Software;
using Css.Core.Startup;
using FluentAssertions;

namespace Css.Tests;

public sealed class StartupEntryControlFixtureStoreTests
{
    [Fact]
    public async Task Fixture_store_simulates_one_exact_entry_without_system_mutation()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-startup-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var path = Path.Combine(root, "startup.json");
            await File.WriteAllTextAsync(path, """
                {
                  "ValueName": "Fixture Startup",
                  "ValueKind": "String",
                  "ValueData": "fixture.exe --background",
                  "KeyAclSha256": "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
                }
                """, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            var now = new DateTimeOffset(2026, 7, 15, 1, 2, 3, TimeSpan.Zero);
            var store = StartupEntryControlFixtureStore.TryCreate(path, () => now);
            var observation = Observation(now);

            var captured = await store!.CaptureAsync(observation);
            var disabled = await store.DisableAsync(captured.State!);
            var missing = await store.CaptureAsync(observation);
            var restored = await store.RestoreAsync(captured.State!);
            var recaptured = await store.CaptureAsync(observation);

            captured.Success.Should().BeTrue();
            captured.State!.CapturedAtUtc.Should().Be(now);
            disabled.Success.Should().BeTrue();
            missing.Success.Should().BeFalse();
            restored.Success.Should().BeTrue();
            recaptured.Success.Should().BeTrue();

            var source = File.ReadAllText(FindRepositoryFile("src", "Css.Core", "Startup", "StartupEntryControlFixtureStore.cs"));
            source.Should().NotContain("Microsoft.Win32");
            source.Should().NotContain("RegistryKey");
            source.Should().NotContain("Process.Start");
            source.Should().NotContain("File.Delete");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Fixture_store_is_absent_without_an_explicit_path()
    {
        StartupEntryControlFixtureStore.TryCreate(null).Should().BeNull();
        StartupEntryControlFixtureStore.TryCreate("   ").Should().BeNull();
    }

    private static BackgroundComponentObservation Observation(DateTimeOffset now) =>
        BackgroundComponentObservationFactory.Startup(
            "Fixture Startup",
            StartupEntryControlPolicy.SupportedSourceLocator,
            "fixture.exe --background",
            now,
            StartupApprovalObservationFactory.FromRegistryValue(
                StartupEntryControlPolicy.SupportedApprovalLocator,
                "Fixture Startup",
                new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));

    private static string FindRepositoryFile(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. segments]);
            if (File.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new FileNotFoundException("Repository file was not found.", Path.Combine(segments));
    }
}
