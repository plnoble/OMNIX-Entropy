using Css.Core.Software;
using Css.Core.Recovery;
using Css.Scanner.Recovery;
using Css.Scanner.Software;
using FluentAssertions;
using System.Diagnostics;

namespace Css.Tests;

public class SoftwareInventoryTests
{
    [Fact]
    public async Task Windows_restore_point_scanner_returns_newest_read_only_records_first()
    {
        var older = new WindowsRestorePointInfo
        {
            SequenceNumber = 10,
            Description = "Before update",
            CreatedAt = new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.FromHours(8)),
            RestorePointType = 0,
            EventType = 100
        };
        var newer = new WindowsRestorePointInfo
        {
            SequenceNumber = 11,
            Description = "Before driver install",
            CreatedAt = new DateTimeOffset(2026, 7, 9, 10, 0, 0, TimeSpan.FromHours(8)),
            RestorePointType = 0,
            EventType = 100
        };
        var scanner = new WindowsRestorePointScanner(_ => [older, newer]);

        var points = await scanner.ScanAsync();

        points.Should().ContainInOrder(newer, older);
        points.Should().OnlyContain(point => point.IsReadOnlyEvidence);
    }

    [Fact]
    public async Task Windows_restore_point_scanner_times_out_instead_of_blocking_the_uninstall_plan()
    {
        var scanner = new WindowsRestorePointScanner(
            cancellationToken =>
            {
                cancellationToken.WaitHandle.WaitOne();
                cancellationToken.ThrowIfCancellationRequested();
                return [];
            },
            TimeSpan.FromMilliseconds(50));
        var stopwatch = Stopwatch.StartNew();

        var result = await scanner.ScanWithStatusAsync();

        stopwatch.Stop();
        result.State.Should().Be(WindowsRestorePointScanState.TimedOut);
        result.Points.Should().BeEmpty();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Profile_builder_maps_installed_records_to_profiles_and_categories()
    {
        var records = new[]
        {
            new InstalledSoftwareRecord(
                DisplayName: "Ollama",
                Publisher: "Ollama",
                InstallLocation: @"C:\Users\Me\AppData\Local\Programs\Ollama",
                UninstallCommand: "\"C:\\Users\\Me\\AppData\\Local\\Programs\\Ollama\\uninstall.exe\"",
                DisplayIcon: null,
                RegistryKeyPath: @"HKCU\...\Ollama"),
            new InstalledSoftwareRecord(
                DisplayName: "Steam",
                Publisher: "Valve",
                InstallLocation: @"D:\Game\Steam\Install",
                UninstallCommand: "steam.exe -uninstall",
                DisplayIcon: null,
                RegistryKeyPath: @"HKLM\...\Steam"),
            new InstalledSoftwareRecord(
                DisplayName: "",
                Publisher: "Ignored",
                InstallLocation: @"C:\Ignored",
                UninstallCommand: null,
                DisplayIcon: null,
                RegistryKeyPath: @"HKLM\...\Ignored")
        };

        var profiles = SoftwareInventoryBuilder.Build(records, [], [], []);

        profiles.Should().HaveCount(2);
        profiles.Should().ContainSingle(p => p.Name == "Ollama")
            .Which.Category.Should().Be(SoftwareCategory.Ai);
        profiles.Should().ContainSingle(p => p.Name == "Steam")
            .Which.Category.Should().Be(SoftwareCategory.Game);
    }

    [Fact]
    public void Registry_record_factory_parses_windows_installer_recovery_metadata()
    {
        const string productCode = "{0A4E2C19-CC57-4E98-A560-0A9C9A12D135}";

        var record = InstalledSoftwareRegistryRecordFactory.Create(
            displayName: "Example App",
            publisher: "Example Inc.",
            installLocation: @"D:\Software\Example\Install",
            uninstallCommand: $"msiexec.exe /I{productCode}",
            displayIcon: null,
            registryKeyPath: @"HKLM\...\" + productCode,
            registrySubKeyName: productCode,
            installSource: @"D:\Installers\ExampleSetup.msi",
            windowsInstallerValue: 1);

        record.InstallSource.Should().Be(@"D:\Installers\ExampleSetup.msi");
        record.IsWindowsInstaller.Should().BeTrue();
        record.WindowsInstallerProductCode.Should().Be(productCode);
    }

    [Fact]
    public void Registry_record_factory_parses_only_recognized_install_dates()
    {
        var compact = InstalledSoftwareRegistryRecordFactory.Create(
            "Recent App", null, null, null, null, "HKCU\\Recent", "Recent",
            installSource: null, windowsInstallerValue: 0, installDateValue: "20260721");
        var dashed = InstalledSoftwareRegistryRecordFactory.Create(
            "Dashed App", null, null, null, null, "HKCU\\Dashed", "Dashed",
            installSource: null, windowsInstallerValue: 0, installDateValue: "2026-07-20");
        var invalid = InstalledSoftwareRegistryRecordFactory.Create(
            "Unknown App", null, null, null, null, "HKCU\\Unknown", "Unknown",
            installSource: null, windowsInstallerValue: 0, installDateValue: "last Tuesday");

        compact.InstallDate.Should().Be(new DateOnly(2026, 7, 21));
        dashed.InstallDate.Should().Be(new DateOnly(2026, 7, 20));
        invalid.InstallDate.Should().BeNull();

        var profile = SoftwareInventoryBuilder.Build([compact], [], [], []).Single();
        profile.InstallDate.Should().Be(compact.InstallDate);
    }

    [Fact]
    public void Display_icon_parser_accepts_local_paths_optional_index_and_commas_in_file_name()
    {
        var quoted = DisplayIconReferenceParser.Parse(
            @"""%LOCALAPPDATA%\Vendor\Example.exe"",-12",
            value => value.Replace(
                "%LOCALAPPDATA%",
                @"C:\Users\Fixture\AppData\Local",
                StringComparison.OrdinalIgnoreCase));
        var commaInName = DisplayIconReferenceParser.Parse(@"D:\Software\Example,Blue.ico");

        quoted.Should().NotBeNull();
        quoted!.Path.Should().Be(@"C:\Users\Fixture\AppData\Local\Vendor\Example.exe");
        quoted.ResourceIndex.Should().Be(-12);
        commaInName.Should().Be(new DisplayIconReference(@"D:\Software\Example,Blue.ico", 0));
    }

    [Fact]
    public void Display_icon_parser_refuses_network_uri_relative_unresolved_and_command_values()
    {
        var refused = new[]
        {
            @"\\server\share\Example.ico",
            "https://example.test/Example.ico",
            @"icons\Example.ico",
            @"%UNKNOWN_ICON_ROOT%\Example.ico",
            @"C:\Software\Example.txt",
            @"""C:\Software\Example.exe"" --open",
            @"C:\Software\Example.exe,not-an-index",
            @"C:\Software\Bad" + '\0' + ".ico",
            @"C:\" + new string('a', 1024) + ".ico"
        };

        refused.Should().OnlyContain(value =>
            DisplayIconReferenceParser.Parse(value, input => input) == null);
    }

    [Fact]
    public void Profile_builder_preserves_reinstall_source_and_windows_installer_metadata()
    {
        const string productCode = "{0A4E2C19-CC57-4E98-A560-0A9C9A12D135}";
        var records = new[]
        {
            new InstalledSoftwareRecord(
                DisplayName: "Example App",
                Publisher: "Example Inc.",
                InstallLocation: @"D:\Software\Example\Install",
                UninstallCommand: $"msiexec.exe /I{productCode}",
                DisplayIcon: null,
                RegistryKeyPath: @"HKLM\...\" + productCode,
                InstallSource: @"D:\Installers\ExampleSetup.msi",
                IsWindowsInstaller: true,
                WindowsInstallerProductCode: productCode)
        };

        var profile = SoftwareInventoryBuilder.Build(records, [], [], []).Single();

        profile.ReinstallSource.Should().Be(@"D:\Installers\ExampleSetup.msi");
        profile.IsWindowsInstaller.Should().BeTrue();
        profile.WindowsInstallerProductCode.Should().Be(productCode);
    }

    [Fact]
    public void Profile_builder_ignores_registry_placeholder_display_names()
    {
        var records = new[]
        {
            new InstalledSoftwareRecord(
                DisplayName: "${arpDisplayName}",
                Publisher: "Placeholder Vendor",
                InstallLocation: @"C:\Program Files\Placeholder",
                UninstallCommand: null,
                DisplayIcon: null,
                RegistryKeyPath: @"HKLM\...\Placeholder"),
            new InstalledSoftwareRecord(
                DisplayName: "Real Tool",
                Publisher: "Vendor",
                InstallLocation: @"D:\Software\Real Tool\Install",
                UninstallCommand: null,
                DisplayIcon: null,
                RegistryKeyPath: @"HKLM\...\RealTool")
        };

        var profiles = SoftwareInventoryBuilder.Build(records, [], [], []);

        profiles.Should().ContainSingle();
        profiles.Single().Name.Should().Be("Real Tool");
    }

    [Fact]
    public void Profile_builder_attaches_startup_and_service_entries_by_path()
    {
        var records = new[]
        {
            new InstalledSoftwareRecord(
                DisplayName: "Docker Desktop",
                Publisher: "Docker Inc.",
                InstallLocation: @"C:\Program Files\Docker\Docker",
                UninstallCommand: "uninstall",
                DisplayIcon: null,
                RegistryKeyPath: @"HKLM\...\Docker")
        };
        var startup = new[]
        {
            new StartupEntry("Docker Desktop", "\"C:\\Program Files\\Docker\\Docker\\Docker Desktop.exe\"", @"HKCU\Run")
        };
        var services = new[]
        {
            new ServiceEntry("com.docker.service", "Docker Desktop Service", "\"C:\\Program Files\\Docker\\Docker\\com.docker.service\"")
        };

        var profile = SoftwareInventoryBuilder.Build(records, startup, services, []).Single();

        profile.Category.Should().Be(SoftwareCategory.DevelopmentTool);
        profile.StartupEntries.Should().Contain("Docker Desktop");
        profile.Services.Should().Contain("com.docker.service");
        profile.CDriveWritePaths.Should().Contain(@"C:\Program Files\Docker\Docker");
    }

    [Fact]
    public void Profile_builder_dedupe_records_by_name_and_install_path()
    {
        var records = new[]
        {
            new InstalledSoftwareRecord("Notion", "Notion Labs", @"C:\Users\Me\AppData\Local\Programs\Notion", "uninstall", null, "HKCU"),
            new InstalledSoftwareRecord("Notion", "Notion Labs", @"C:\Users\Me\AppData\Local\Programs\Notion", "uninstall", null, "HKLM")
        };

        var profiles = SoftwareInventoryBuilder.Build(records, [], [], []);

        profiles.Should().ContainSingle();
    }

    [Fact]
    public void Profile_builder_populates_signature_subject_from_executable_hint()
    {
        var records = new[]
        {
            new InstalledSoftwareRecord(
                "Signed Tool",
                "Vendor",
                @"C:\Program Files\Signed Tool",
                null,
                "\"C:\\Program Files\\Signed Tool\\tool.exe\",0",
                "HKLM")
        };

        var profiles = SoftwareInventoryBuilder.Build(records, [], [], [], path => path.EndsWith("tool.exe") ? "CN=Vendor" : null);

        profiles.Single().SignatureSubject.Should().Be("CN=Vendor");
    }

    [Fact]
    public void Scheduled_task_xml_parser_extracts_exec_command()
    {
        const string xml = """
            <Task xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
              <Actions Context="Author">
                <Exec>
                  <Command>C:\Program Files\Docker\Docker\Docker Desktop.exe</Command>
                  <Arguments>--background</Arguments>
                </Exec>
              </Actions>
            </Task>
            """;

        var task = ScheduledTaskXmlParser.Parse(@"\Docker Desktop Update", xml);

        task.Should().NotBeNull();
        task!.Name.Should().Be(@"\Docker Desktop Update");
        task.ActionPath.Should().Be(@"C:\Program Files\Docker\Docker\Docker Desktop.exe");
        task.IsEnabled.Should().BeNull();
    }

    [Fact]
    public void Profile_builder_attaches_scheduled_tasks_by_path()
    {
        var records = new[]
        {
            new InstalledSoftwareRecord(
                DisplayName: "Docker Desktop",
                Publisher: "Docker Inc.",
                InstallLocation: @"C:\Program Files\Docker\Docker",
                UninstallCommand: null,
                DisplayIcon: null,
                RegistryKeyPath: @"HKLM\...\Docker")
        };
        var tasks = new[]
        {
            new ScheduledTaskEntry(@"\Docker Desktop Update", @"C:\Program Files\Docker\Docker\Docker Desktop.exe")
        };

        var profile = SoftwareInventoryBuilder.Build(records, [], [], tasks).Single();

        profile.ScheduledTasks.Should().Contain(@"\Docker Desktop Update");
    }

    [Fact]
    public void Profile_builder_attaches_running_processes_by_path_or_name()
    {
        var records = new[]
        {
            new InstalledSoftwareRecord(
                DisplayName: "Marvis",
                Publisher: "Tencent",
                InstallLocation: @"D:\Software\Marvis\Install\Marvis\Application",
                UninstallCommand: null,
                DisplayIcon: null,
                RegistryKeyPath: @"HKLM\...\Marvis")
        };
        var processes = new[]
        {
            new ProcessEntry("Marvis", @"D:\Software\Marvis\Install\Marvis\Application\Marvis.exe"),
            new ProcessEntry("MarvisAgent", @"D:\Software\Marvis\Install\Marvis\MarvisAgent\MarvisAgent.exe")
        };

        var profile = SoftwareInventoryBuilder.Build(records, [], [], [], runningProcesses: processes).Single();

        profile.RunningProcesses.Should().Contain(["Marvis", "MarvisAgent"]);
    }

    [Fact]
    public void Profile_builder_infers_appdata_cache_candidates_for_drawer_preview()
    {
        var localAppData = @"C:\Users\Me\AppData\Local";
        var appDataRoot = Path.Combine(localAppData, "Example App");
        var cachePath = Path.Combine(appDataRoot, "Cache");
        var logPath = Path.Combine(appDataRoot, "Logs");
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            appDataRoot,
            cachePath,
            logPath
        };
        var records = new[]
        {
            new InstalledSoftwareRecord(
                DisplayName: "Example App",
                Publisher: "Example",
                InstallLocation: @"D:\Software\Example App\Install",
                UninstallCommand: null,
                DisplayIcon: null,
                RegistryKeyPath: @"HKCU\...\Example")
        };

        var profile = SoftwareInventoryBuilder.Build(
            records,
            [],
            [],
            [],
            userDataRoots: [localAppData],
            pathExists: existing.Contains,
            cacheSizeResolver: path => path.Equals(cachePath, StringComparison.OrdinalIgnoreCase)
                ? 768L * 1024 * 1024
                : 0).Single();

        profile.DataPaths.Should().Contain(appDataRoot);
        profile.CachePaths.Should().Contain(cachePath);
        profile.LogPaths.Should().Contain(logPath);
        profile.CacheSizeBytes.Should().Be(768L * 1024 * 1024);
        profile.CDriveWritePaths.Should().Contain([appDataRoot, cachePath, logPath]);
    }

    [Fact]
    public void Profile_builder_infers_browser_profile_cache_candidates()
    {
        var localAppData = @"C:\Users\Me\AppData\Local";
        var browserRoot = Path.Combine(localAppData, "Google", "Chrome");
        var userDataRoot = Path.Combine(browserRoot, "User Data");
        var defaultProfile = Path.Combine(userDataRoot, "Default");
        var cachePath = Path.Combine(defaultProfile, "Cache");
        var codeCachePath = Path.Combine(defaultProfile, "Code Cache");
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            browserRoot,
            userDataRoot,
            defaultProfile,
            cachePath,
            codeCachePath
        };
        var records = new[]
        {
            new InstalledSoftwareRecord(
                DisplayName: "Google Chrome",
                Publisher: "Google LLC",
                InstallLocation: @"C:\Program Files\Google\Chrome\Application",
                UninstallCommand: null,
                DisplayIcon: null,
                RegistryKeyPath: @"HKLM\...\Chrome")
        };

        var profile = SoftwareInventoryBuilder.Build(
            records,
            [],
            [],
            [],
            userDataRoots: [localAppData],
            pathExists: existing.Contains,
            cacheSizeResolver: path => path.Equals(cachePath, StringComparison.OrdinalIgnoreCase)
                ? 512L * 1024 * 1024
                : path.Equals(codeCachePath, StringComparison.OrdinalIgnoreCase)
                    ? 128L * 1024 * 1024
                    : 0).Single();

        profile.DataPaths.Should().Contain(browserRoot);
        profile.DataPaths.Should().Contain(userDataRoot);
        profile.CachePaths.Should().Contain([cachePath, codeCachePath]);
        profile.CacheSizeBytes.Should().Be(640L * 1024 * 1024);
        profile.CDriveWritePaths.Should().Contain([browserRoot, userDataRoot, cachePath, codeCachePath]);
    }

    [Fact]
    public void Profile_builder_infers_electron_user_data_cache_candidates()
    {
        var localAppData = @"C:\Users\Me\AppData\Local";
        var appRoot = Path.Combine(localAppData, "Example Electron");
        var userDataRoot = Path.Combine(appRoot, "User Data");
        var cachePath = Path.Combine(userDataRoot, "Cache");
        var gpuCachePath = Path.Combine(userDataRoot, "GPUCache");
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            appRoot,
            userDataRoot,
            cachePath,
            gpuCachePath
        };
        var records = new[]
        {
            new InstalledSoftwareRecord(
                DisplayName: "Example Electron",
                Publisher: "Example",
                InstallLocation: @"D:\Software\Example Electron\Install",
                UninstallCommand: null,
                DisplayIcon: null,
                RegistryKeyPath: @"HKCU\...\ExampleElectron")
        };

        var profile = SoftwareInventoryBuilder.Build(
            records,
            [],
            [],
            [],
            userDataRoots: [localAppData],
            pathExists: existing.Contains,
            cacheSizeResolver: path => 64L * 1024 * 1024).Single();

        profile.DataPaths.Should().Contain([appRoot, userDataRoot]);
        profile.CachePaths.Should().Contain([cachePath, gpuCachePath]);
        profile.CacheSizeBytes.Should().Be(128L * 1024 * 1024);
        profile.CDriveWritePaths.Should().Contain([appRoot, userDataRoot, cachePath, gpuCachePath]);
    }

    [Fact]
    public void Profile_builder_infers_marvis_root_category_size_service_and_processes()
    {
        var records = new[]
        {
            new InstalledSoftwareRecord(
                DisplayName: "Marvis",
                Publisher: "腾讯科技(深圳)有限公司",
                InstallLocation: "",
                UninstallCommand: @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0",
                DisplayIcon: @"D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe,-2",
                RegistryKeyPath: @"HKCU\...\Marvis")
        };
        var services = new[]
        {
            new ServiceEntry("MarvisSvr", "MarvisSvr", @"""D:\Software\Marvis\Install\Marvis\Application\1.60.1500.80\MarvisSvr.exe""")
        };
        var processes = new[]
        {
            new ProcessEntry("MarvisSvr", null)
        };

        var profile = SoftwareInventoryBuilder.Build(
            records,
            [],
            services,
            [],
            runningProcesses: processes,
            installSizeResolver: path => path.Equals(@"D:\Software\Marvis\Install", StringComparison.OrdinalIgnoreCase)
                ? 8_333_798_192
                : 0).Single();

        profile.InstallPath.Should().Be(@"D:\Software\Marvis\Install");
        profile.Category.Should().Be(SoftwareCategory.Ai);
        profile.InstalledSizeBytes.Should().Be(8_333_798_192);
        profile.DisplayIconPath.Should().Be(@"D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe");
        profile.DisplayIconIndex.Should().Be(-2);
        profile.Services.Should().Contain("MarvisSvr");
        profile.RunningProcesses.Should().Contain("MarvisSvr");
    }

    [Fact]
    public void Service_entry_factory_creates_entry_from_registry_image_path()
    {
        var entry = ServiceEntryFactory.FromRegistryValues(
            "MarvisSvr",
            "MarvisSvr",
            @"""D:\Software\Marvis\Install\Marvis\Application\1.60.1500.80\MarvisSvr.exe""");

        entry.Should().NotBeNull();
        entry!.Name.Should().Be("MarvisSvr");
        entry.DisplayName.Should().Be("MarvisSvr");
        entry.PathName.Should().Contain("MarvisSvr.exe");
    }
}
