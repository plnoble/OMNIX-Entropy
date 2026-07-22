using Css.Core.Apps;
using Css.Core.Software;
using Css.Win32.SystemHealth;
using FluentAssertions;

namespace Css.Tests;

public sealed class ApplicationCrashObservationTests
{
    [Fact]
    public void Probe_keeps_only_bounded_approved_matching_crash_facts()
    {
        var now = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero);
        var reader = new FakeReader(
        [
            Entry(1000, "Application Error", now.AddHours(-2), "WeChat.exe", @"C:\Private\WeChat.exe"),
            Entry(1002, "Application Hang", now.AddHours(-1), "WeChat.exe"),
            Entry(1001, "Windows Error Reporting", now.AddHours(-3), "WeChat.exe"),
            Entry(1000, "Unreviewed Provider", now.AddMinutes(-5), "WeChat.exe"),
            Entry(9999, "Application Error", now.AddMinutes(-4), "WeChat.exe"),
            Entry(1000, "Application Error", now.AddHours(-25), "WeChat.exe"),
            Entry(1000, "Application Error", now.AddMinutes(-3), "Other.exe")
        ]);
        var probe = new WindowsApplicationCrashProbe(reader, () => now);

        var observation = probe.Observe(Profile());

        observation.Availability.Should().Be(ApplicationCrashObservationAvailability.Available);
        observation.SoftwareName.Should().Be("微信");
        observation.MatchCount.Should().Be(3);
        observation.LatestOccurrenceUtc.Should().Be(now.AddHours(-1));
        observation.WindowStartUtc.Should().Be(now.AddHours(-24));
        observation.ObservedAtUtc.Should().Be(now);
        observation.CanExecuteDirectly.Should().BeFalse();
        reader.LastMaximumRecords.Should().Be(WindowsApplicationCrashProbe.MaximumCandidateRecords);
        reader.LastSinceUtc.Should().Be(now.AddHours(-24));
    }

    [Fact]
    public void Readable_log_without_match_is_not_found_and_reader_failure_is_unavailable()
    {
        var now = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero);
        var noMatch = new WindowsApplicationCrashProbe(
            new FakeReader([Entry(1000, "Application Error", now.AddMinutes(-5), "Other.exe")]),
            () => now)
            .Observe(Profile());
        var unavailable = new WindowsApplicationCrashProbe(
            new ThrowingReader(),
            () => now)
            .Observe(Profile());

        noMatch.Availability.Should().Be(ApplicationCrashObservationAvailability.NotFound);
        noMatch.MatchCount.Should().Be(0);
        noMatch.LatestOccurrenceUtc.Should().BeNull();
        unavailable.Availability.Should().Be(ApplicationCrashObservationAvailability.Unavailable);
        unavailable.MatchCount.Should().Be(0);
        unavailable.LatestOccurrenceUtc.Should().BeNull();
        unavailable.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Reader_and_core_model_do_not_retain_or_mutate_private_event_data()
    {
        var reader = Read("src", "Css.Win32", "SystemHealth", "WindowsApplicationCrashProbe.cs");
        var model = Read("src", "Css.Core", "Apps", "ApplicationCrashObservation.cs");

        reader.Should().Contain("new EventLogQuery(\"Application\"")
            .And.Contain("ReverseDirection = true")
            .And.Contain("ReadEvent()")
            .And.Contain("MaximumCandidateRecords")
            .And.NotContain("FormatDescription")
            .And.NotContain("EventLog.Clear")
            .And.NotContain("ClearLog")
            .And.NotContain("ExportLog")
            .And.NotContain("DeleteEventSource")
            .And.NotContain("Process.Start")
            .And.NotContain("Registry.SetValue")
            .And.NotContain("File.Delete");
        model.Should().Contain("public bool CanExecuteDirectly => false;")
            .And.NotContain("Message")
            .And.NotContain("PropertyValues")
            .And.NotContain("ProviderName")
            .And.NotContain("EventId")
            .And.NotContain("Path");
    }

    private static WindowsApplicationCrashLogEntry Entry(
        int eventId,
        string provider,
        DateTimeOffset createdAtUtc,
        params string[] values) =>
        new()
        {
            EventId = eventId,
            ProviderName = provider,
            CreatedAtUtc = createdAtUtc,
            PropertyValues = values
        };

    private static SoftwareProfile Profile() => new()
    {
        Name = "微信",
        DisplayIconPath = @"C:\Software\WeChat\WeChat.exe",
        InstallPath = @"C:\Software\WeChat"
    };

    private sealed class FakeReader(IReadOnlyList<WindowsApplicationCrashLogEntry> entries)
        : IWindowsApplicationCrashLogReader
    {
        public DateTimeOffset LastSinceUtc { get; private set; }
        public int LastMaximumRecords { get; private set; }

        public IReadOnlyList<WindowsApplicationCrashLogEntry> ReadRecent(
            DateTimeOffset sinceUtc,
            int maximumRecords)
        {
            LastSinceUtc = sinceUtc;
            LastMaximumRecords = maximumRecords;
            return entries;
        }
    }

    private sealed class ThrowingReader : IWindowsApplicationCrashLogReader
    {
        public IReadOnlyList<WindowsApplicationCrashLogEntry> ReadRecent(
            DateTimeOffset sinceUtc,
            int maximumRecords) =>
            throw new InvalidOperationException("private fixture failure");
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
