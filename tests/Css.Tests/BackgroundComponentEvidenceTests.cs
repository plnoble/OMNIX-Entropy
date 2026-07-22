using Css.Core.Apps;
using Css.Core.Software;
using Css.Scanner.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class BackgroundComponentEvidenceTests
{
    private static readonly DateTimeOffset ObservedAt =
        new(2026, 7, 13, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Exact_identity_survives_configuration_change_while_observation_fingerprint_changes()
    {
        var first = BackgroundComponentObservationFactory.Startup(
            "ExampleAgent",
            @"HKCU\Software\Microsoft\Windows\CurrentVersion\Run",
            @"D:\Software\Example\agent.exe --background",
            ObservedAt);
        var changed = BackgroundComponentObservationFactory.Startup(
            "exampleagent",
            @"hkcu\software\microsoft\windows\currentversion\run",
            @"D:\Software\Example\agent.exe --changed",
            ObservedAt.AddMinutes(5));

        first.Identity.StableId.Should().Be(changed.Identity.StableId);
        first.ObservationFingerprint.Should().NotBe(changed.ObservationFingerprint);
        first.Identity.StableId.Should().HaveLength(64);
        first.ActivationState.Should().Be(BackgroundComponentActivationState.Unknown);
        first.IsReadOnlyEvidence.Should().BeTrue();
        first.IsRollbackReady.Should().BeFalse();
        first.CanCreateChangeOperation.Should().BeFalse();
        first.RequiredRollbackEvidence.Should().Contain(
            BackgroundRollbackEvidenceRequirement.StartupApprovalState);

        var snapshot = BackgroundComponentInventorySnapshotBuilder.Create(new SoftwareProfile
        {
            Name = "Example",
            StartupEntries = ["ExampleAgent"],
            BackgroundComponents = [first]
        });
        snapshot.Observations.Should().ContainSingle();
        snapshot.ObservedAtUtc.Should().Be(ObservedAt);
        snapshot.SnapshotFingerprint.Should().HaveLength(64);
        snapshot.SnapshotId.Should().StartWith("background-observation-");
        snapshot.IsReadOnlyEvidence.Should().BeTrue();
        snapshot.IsRollbackReady.Should().BeFalse();
        snapshot.CanCreateChangeOperation.Should().BeFalse();
    }

    [Fact]
    public void Startup_approval_binary_is_fingerprinted_but_never_decoded_or_retained()
    {
        var firstBytes = new byte[] { 2, 0, 0, 0, 1, 2, 3, 4 };
        var changedBytes = new byte[] { 3, 0, 0, 0, 1, 2, 3, 4 };
        var firstApproval = StartupApprovalObservationFactory.FromRegistryValue(
            @"HKCU64\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run",
            "ExampleAgent",
            firstBytes);
        var changedApproval = StartupApprovalObservationFactory.FromRegistryValue(
            @"HKCU64\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run",
            "ExampleAgent",
            changedBytes);
        var first = BackgroundComponentObservationFactory.Startup(
            "ExampleAgent",
            @"HKCU64\Software\Microsoft\Windows\CurrentVersion\Run",
            @"D:\Software\Example\agent.exe",
            ObservedAt,
            firstApproval);
        var changed = BackgroundComponentObservationFactory.Startup(
            "ExampleAgent",
            @"HKCU64\Software\Microsoft\Windows\CurrentVersion\Run",
            @"D:\Software\Example\agent.exe",
            ObservedAt.AddMinutes(1),
            changedApproval);

        firstApproval.Status.Should().Be(StartupApprovalEvidenceStatus.PresentBinary);
        firstApproval.PayloadLength.Should().Be(firstBytes.Length);
        firstApproval.PayloadFingerprint.Should().HaveLength(64);
        firstApproval.PayloadFingerprint.Should().NotBe(changedApproval.PayloadFingerprint);
        firstApproval.EffectiveActivationState.Should().Be(BackgroundComponentActivationState.Unknown);
        firstApproval.IsStateDecoded.Should().BeFalse();
        firstApproval.IsReadOnlyEvidence.Should().BeTrue();
        firstApproval.CanAuthorizeChange.Should().BeFalse();
        typeof(StartupApprovalObservation).GetProperties()
            .Should().NotContain(property => property.PropertyType == typeof(byte[]));
        first.Identity.StableId.Should().Be(changed.Identity.StableId);
        first.ObservationFingerprint.Should().NotBe(changed.ObservationFingerprint);
        first.ActivationState.Should().Be(BackgroundComponentActivationState.Unknown);
    }

    [Fact]
    public void Missing_unsupported_and_unreadable_approval_evidence_stay_distinct_and_unknown()
    {
        const string key = @"HKLM64\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
        var missing = StartupApprovalObservationFactory.FromRegistryValue(key, "Missing", null);
        var unsupported = StartupApprovalObservationFactory.FromRegistryValue(key, "Unexpected", "not-binary");
        var unreadable = StartupApprovalObservationFactory.Unreadable(key, "Protected");

        missing.Status.Should().Be(StartupApprovalEvidenceStatus.Missing);
        unsupported.Status.Should().Be(StartupApprovalEvidenceStatus.PresentUnsupportedType);
        unreadable.Status.Should().Be(StartupApprovalEvidenceStatus.Unreadable);
        new[] { missing, unsupported, unreadable }.Should().OnlyContain(item =>
            item.EffectiveActivationState == BackgroundComponentActivationState.Unknown
            && !item.IsStateDecoded
            && !item.CanAuthorizeChange
            && item.PayloadFingerprint == null);
    }

    [Fact]
    public void Service_and_task_observations_keep_activation_separate_from_runtime()
    {
        var service = BackgroundComponentObservationFactory.Service(
            "ExampleService",
            @"HKLM\SYSTEM\CurrentControlSet\Services\ExampleService",
            @"D:\Software\Example\service.exe",
            "Automatic",
            "Running",
            ObservedAt);
        var task = BackgroundComponentObservationFactory.ScheduledTask(
            @"\Example\Updater",
            @"\Example\Updater",
            @"D:\Software\Example\updater.exe",
            false,
            ObservedAt);

        service.ActivationState.Should().Be(BackgroundComponentActivationState.Automatic);
        service.RuntimeState.Should().Be(BackgroundComponentRuntimeState.Running);
        service.RequiredRollbackEvidence.Should().Contain(
            BackgroundRollbackEvidenceRequirement.ServiceRecoveryConfiguration);
        task.ActivationState.Should().Be(BackgroundComponentActivationState.Disabled);
        task.RuntimeState.Should().Be(BackgroundComponentRuntimeState.NotApplicable);
        task.RequiredRollbackEvidence.Should().Contain(
            BackgroundRollbackEvidenceRequirement.ScheduledTaskDefinition);
    }

    [Fact]
    public void Inventory_builder_attaches_structured_components_and_keeps_legacy_name_views()
    {
        var records = new[]
        {
            new InstalledSoftwareRecord(
                "Example App",
                "Example",
                @"D:\Software\Example App\Install",
                null,
                null,
                @"HKLM\...\Example")
        };
        var startup = new[]
        {
            new StartupEntry(
                "Example App Agent",
                @"D:\Software\Example App\Install\agent.exe",
                @"HKCU64\Software\Microsoft\Windows\CurrentVersion\Run",
                StartupApprovalObservationFactory.FromRegistryValue(
                    @"HKCU64\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run",
                    "Example App Agent",
                    new byte[] { 7, 6, 5, 4 }))
        };
        var services = new[]
        {
            new ServiceEntry(
                "ExampleAppService",
                "Example App Service",
                @"D:\Software\Example App\Install\service.exe",
                "Manual",
                "Stopped")
        };
        var tasks = new[]
        {
            new ScheduledTaskEntry(
                @"\Example App\Update",
                @"D:\Software\Example App\Install\update.exe",
                true)
        };

        var profile = SoftwareInventoryBuilder.Build(
            records,
            startup,
            services,
            tasks,
            observedAtUtc: ObservedAt).Single();

        profile.StartupEntries.Should().Equal("Example App Agent");
        profile.Services.Should().Equal("ExampleAppService");
        profile.ScheduledTasks.Should().Equal(@"\Example App\Update");
        profile.BackgroundComponents.Should().HaveCount(3);
        profile.BackgroundComponents.Should().OnlyContain(item =>
            item.ObservedAtUtc == ObservedAt && item.IsReadOnlyEvidence && !item.IsRollbackReady);
        profile.BackgroundComponents.Select(item => item.Identity.StableId)
            .Should().OnlyHaveUniqueItems();
        profile.BackgroundComponents.Single(item =>
                item.Identity.Kind == BackgroundComponentKind.StartupEntry)
            .StartupApproval!.Status.Should().Be(StartupApprovalEvidenceStatus.PresentBinary);
    }

    [Fact]
    public void Legacy_or_structured_observations_remain_fail_closed_without_rollback_evidence()
    {
        var legacy = new SoftwareProfile
        {
            Name = "Legacy App",
            StartupEntries = ["LegacyAgent"]
        };
        var structured = new SoftwareProfile
        {
            Name = "Structured App",
            StartupEntries = ["StructuredAgent"],
            BackgroundComponents =
            [
                BackgroundComponentObservationFactory.Startup(
                    "StructuredAgent",
                    @"HKCU\Software\Microsoft\Windows\CurrentVersion\Run",
                    @"D:\Software\Structured\agent.exe",
                    ObservedAt)
            ]
        };

        var legacyReadiness = BackgroundComponentChangeReadinessPolicy.Evaluate(legacy);
        var structuredReadiness = BackgroundComponentChangeReadinessPolicy.Evaluate(structured);

        legacyReadiness.CanCreateChangeOperation.Should().BeFalse();
        legacyReadiness.Reasons.Should().Contain(reason => reason.Contains("名称级线索"));
        structuredReadiness.CanCreateChangeOperation.Should().BeFalse();
        structuredReadiness.StructuredObservationCount.Should().Be(1);
        structuredReadiness.Reasons.Should().Contain(reason => reason.Contains("启用状态仍未知"));
        structuredReadiness.Reasons.Should().Contain(reason => reason.Contains("原始配置"));
    }

    [Fact]
    public void Scheduled_task_parser_keeps_explicit_enabled_state()
    {
        const string xml = """
            <Task xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
              <Triggers><LogonTrigger><Enabled>true</Enabled></LogonTrigger></Triggers>
              <Settings><Enabled>false</Enabled></Settings>
              <Actions><Exec><Command>D:\Software\Example\update.exe</Command></Exec></Actions>
            </Task>
            """;

        var task = ScheduledTaskXmlParser.Parse(@"\Example\Update", xml);

        task.Should().NotBeNull();
        task!.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void Missing_service_start_value_remains_unknown_instead_of_becoming_boot_start()
    {
        var entry = ServiceEntryFactory.FromRegistryValues(
            "ExampleService",
            "Example Service",
            @"D:\Software\Example\service.exe",
            startValue: null);

        entry.Should().NotBeNull();
        entry!.StartMode.Should().BeNull();
    }

    [Fact]
    public void Structured_evidence_stays_in_hidden_technical_details_and_has_no_mutation_authority()
    {
        var observation = BackgroundComponentObservationFactory.Service(
            "ExampleService",
            @"HKLM\SYSTEM\CurrentControlSet\Services\ExampleService",
            @"D:\Software\Example\service.exe",
            "Manual",
            "Stopped",
            ObservedAt);
        var profile = new SoftwareProfile
        {
            Name = "Example",
            Services = ["ExampleService"],
            BackgroundComponents = [observation]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.TechnicalDetailsHiddenByDefault.Should().BeTrue();
        drawer.ResidencySummary.Should().NotContain("HKLM");
        drawer.AgentAdvice.Text.Should().NotContain("HKLM");
        drawer.TechnicalDetails.Should().Contain(line => line.Contains("read-only observations"));
        drawer.TechnicalDetails.Should().Contain(line => line.Contains("ExampleService"));

        var evidenceSource = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Software", "BackgroundComponentEvidence.cs"));
        evidenceSource.Should().NotContain("OperationDescriptor");
        evidenceSource.Should().NotContain("Registry.SetValue");
        evidenceSource.Should().NotContain("ServiceController");
        evidenceSource.Should().NotContain("Process.Start");
    }

    [Fact]
    public void Scanner_correlates_explicit_registry_views_without_startup_state_byte_rules()
    {
        var scanner = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Scanner", "Software", "SoftwareInventoryScanner.cs"));
        var evidence = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Software", "BackgroundComponentEvidence.cs"));

        scanner.Should().Contain("RegistryView.Registry64");
        scanner.Should().Contain("RegistryView.Registry32");
        scanner.Should().Contain("StartupApproved\\Run32");
        scanner.Should().Contain("\"HKLM32\"");
        scanner.Should().Contain("\"HKLM64\"");
        scanner.Should().Contain("RegistryValueOptions.DoNotExpandEnvironmentNames");
        scanner.Should().NotContain("Registry.SetValue");
        scanner.Should().NotContain("CreateSubKey");
        scanner.Should().NotContain("DeleteValue");
        evidence.Should().NotContain("bytes[");
        evidence.Should().NotContain("BitConverter");
        evidence.Should().NotContain("OperationDescriptor");
    }

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
