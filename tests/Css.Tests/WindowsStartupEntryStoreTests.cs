using Css.Core.Software;
using Css.Core.Startup;
using Css.Win32.Startup;
using FluentAssertions;

namespace Css.Tests;

public sealed class WindowsStartupEntryStoreTests
{
    [Fact]
    public async Task Capture_requires_fresh_command_approval_and_acl_evidence()
    {
        var now = DateTimeOffset.UtcNow;
        var observation = Observation(now);
        var raw = new WindowsCurrentUserRunRegistrySnapshot
        {
            Readable = true,
            Exists = true,
            ValueName = observation.Identity.DisplayName,
            ValueKind = StartupRegistryValueKind.String,
            ValueData = "fixture.exe --background",
            KeyAclSha256 = Sha('A'),
            StartupApproval = observation.StartupApproval,
            CapturedAtUtc = now
        };
        var backend = new FakeBackend(raw);
        var store = new WindowsCurrentUserRunStartupEntryStore(backend);

        var result = await store.CaptureAsync(observation);

        result.Success.Should().BeTrue();
        result.State!.ObservationFingerprint.Should().Be(observation.ObservationFingerprint);
        result.State.KeyAclSha256.Should().Be(Sha('A'));
        backend.CapturedNames.Should().Equal(observation.Identity.DisplayName);
    }

    [Fact]
    public async Task Capture_refuses_command_drift_and_never_mutates()
    {
        var now = DateTimeOffset.UtcNow;
        var observation = Observation(now);
        var raw = new WindowsCurrentUserRunRegistrySnapshot
        {
            Readable = true,
            Exists = true,
            ValueName = observation.Identity.DisplayName,
            ValueKind = StartupRegistryValueKind.String,
            ValueData = "different.exe",
            KeyAclSha256 = Sha('B'),
            StartupApproval = observation.StartupApproval,
            CapturedAtUtc = now
        };
        var backend = new FakeBackend(raw);
        var store = new WindowsCurrentUserRunStartupEntryStore(backend);

        var result = await store.CaptureAsync(observation);

        result.Success.Should().BeFalse();
        backend.Disabled.Should().BeEmpty();
        backend.Restored.Should().BeEmpty();
    }

    [Fact]
    public async Task Disable_and_restore_forward_only_verified_states()
    {
        var now = DateTimeOffset.UtcNow;
        var observation = Observation(now);
        var state = StartupEntryStateFactory.Create(
            observation,
            StartupRegistryValueKind.ExpandString,
            @"%LOCALAPPDATA%\Fixture\fixture.exe",
            Sha('C'),
            now);
        var backend = new FakeBackend(null);
        var store = new WindowsCurrentUserRunStartupEntryStore(backend);

        var disabled = await store.DisableAsync(state);
        var restored = await store.RestoreAsync(state);
        var forged = await store.DisableAsync(state with { StateFingerprint = Sha('D') });

        disabled.Success.Should().BeTrue();
        restored.Success.Should().BeTrue();
        forged.Success.Should().BeFalse();
        backend.Disabled.Should().ContainSingle().Which.Should().BeSameAs(state);
        backend.Restored.Should().ContainSingle().Which.Should().BeSameAs(state);
    }

    [Fact]
    public void Production_backend_is_hkcu64_only_and_preserves_startup_approval()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Win32", "Startup", "WindowsCurrentUserRunStartupEntryStore.cs"));

        source.Should().Contain("RegistryHive.CurrentUser")
            .And.Contain("RegistryView.Registry64")
            .And.Contain("StartupEntryControlPolicy.SupportedSourceLocator")
            .And.Contain("StartupEntryControlPolicy.SupportedApprovalLocator")
            .And.Contain("RegistryValueOptions.DoNotExpandEnvironmentNames")
            .And.Contain("DeleteValue")
            .And.Contain("SetValue")
            .And.Contain("GetSecurityDescriptorBinaryForm")
            .And.NotContain("RegistryHive.LocalMachine")
            .And.NotContain("ServiceController")
            .And.NotContain("TaskService")
            .And.NotContain("approvalKey.DeleteValue")
            .And.NotContain("approvalKey.SetValue");
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

    private static string Sha(char value) => new(value, 64);

    private static string FindRepositoryFile(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "ComputerSecuritySoftware.slnx")))
            current = current.Parent;
        return Path.Combine([current?.FullName ?? throw new DirectoryNotFoundException(), .. segments]);
    }

    private sealed class FakeBackend(WindowsCurrentUserRunRegistrySnapshot? snapshot)
        : IWindowsCurrentUserRunRegistryBackend
    {
        public List<string> CapturedNames { get; } = [];
        public List<StartupEntryState> Disabled { get; } = [];
        public List<StartupEntryState> Restored { get; } = [];

        public WindowsCurrentUserRunRegistrySnapshot Capture(string valueName)
        {
            CapturedNames.Add(valueName);
            return snapshot ?? WindowsCurrentUserRunRegistrySnapshot.Missing(valueName, DateTimeOffset.UtcNow);
        }

        public StartupEntryMutationResult DisableExact(StartupEntryState expected)
        {
            Disabled.Add(expected);
            return StartupEntryMutationResult.Completed("disabled");
        }

        public StartupEntryMutationResult RestoreExact(StartupEntryState expected)
        {
            Restored.Add(expected);
            return StartupEntryMutationResult.Completed("restored");
        }
    }
}
