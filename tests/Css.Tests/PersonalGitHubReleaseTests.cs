using FluentAssertions;

namespace Css.Tests;

public sealed class PersonalGitHubReleaseTests
{
    [Fact]
    public void Public_repository_ignores_machine_evidence_and_private_signing_material()
    {
        var ignore = Read(".gitignore");

        ignore.Should().Contain(".omx/*")
            .And.Contain("!.omx/development/**")
            .And.Contain("!.omx/*.ps1")
            .And.Contain("/quarantine/")
            .And.Contain("*.pfx")
            .And.Contain("*.p12")
            .And.Contain("*.key")
            .And.Contain(".env.*");
        ignore.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Should().NotContain("quarantine/");

        Read(".gitattributes").Should().Contain("* text=auto eol=lf");
    }

    [Fact]
    public void Ci_is_read_only_and_pins_official_actions_to_commits()
    {
        var workflow = Read(".github", "workflows", "ci.yml");

        workflow.Should().Contain("contents: read")
            .And.Contain("actions/checkout@fbc6f3992d24b796d5a048ff273f7fcc4a7b6c09")
            .And.Contain("actions/setup-dotnet@26b0ec14cb23fa6904739307f278c14f94c95bf1")
            .And.Contain("dotnet test ComputerSecuritySoftware.slnx --configuration Debug --no-restore")
            .And.Contain("dotnet build ComputerSecuritySoftware.slnx --configuration Release --no-restore")
            .And.Contain(".omx/verify-source-integrity.ps1");
        workflow.IndexOf("dotnet build ComputerSecuritySoftware.slnx --configuration Release --no-restore", StringComparison.Ordinal)
            .Should().BeLessThan(workflow.IndexOf("dotnet test ComputerSecuritySoftware.slnx --configuration Debug --no-restore", StringComparison.Ordinal));
        workflow.Should().NotContain("contents: write")
            .And.NotContain("pull_request_target")
            .And.NotContain("secrets.");

        Read("tests", "Css.Tests", "AssemblyInfo.cs")
            .Should().Contain("DisableTestParallelization = true");
    }

    [Fact]
    public void Personal_release_requires_verified_same_signer_installer_and_only_publishes_drafts()
    {
        var script = Read("scripts", "prepare-personal-github-release.ps1");

        script.Should().Contain("verify-personal-installer.ps1")
            .And.Contain("PersonalWindowsInstaller")
            .And.Contain("DirectorySelectionVisible")
            .And.Contain("SilentInstallAllowed")
            .And.Contain("installer-manifest.json")
            .And.Contain("win-x64-setup.exe")
            .And.Contain("SignerThumbprint")
            .And.Contain("Get-FileHash -LiteralPath $packageAssetPath -Algorithm SHA256")
            .And.Contain("Installer copy hash verification failed")
            .And.Contain("plnoble/OMNIX-Entropy")
            .And.Contain("--draft")
            .And.Contain("PublishDraft")
            .And.Contain("A committed source revision is required")
            .And.Contain("Release tag already exists");
        script.Should().NotContain("New-SelfSignedCertificate")
            .And.NotContain("Import-PfxCertificate")
            .And.NotContain("TrustedPublisher")
            .And.NotContain("--latest")
            .And.NotContain("Remove-Item")
            .And.NotContain("Move-Item")
            .And.NotContain("--force");
    }

    [Fact]
    public void Update_ui_is_user_started_and_only_exposes_a_validated_release_page()
    {
        var main = Read("src", "Css.App", "MainWindow.xaml");
        var window = Read("src", "Css.App", "UpdateWindow.xaml");
        var code = Read("src", "Css.App", "UpdateWindow.xaml.cs");

        main.Should().Contain("AutomationProperties.AutomationId=\"OpenUpdateButton\"")
            .And.Contain("Click=\"OpenUpdate_Click\"");
        window.Should().Contain("AutomationProperties.AutomationId=\"CheckForUpdatesButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"OpenReleasePageButton\"")
            .And.Contain("Visibility=\"Collapsed\"");
        code.Should().Contain("PersonalReleaseChannelPolicy.IsExpectedReleasePage")
            .And.Contain("PersonalReleaseCheckStatus.UpdateAvailable")
            .And.Contain("UseShellExecute = true")
            .And.NotContain("DownloadFile")
            .And.NotContain("ProcessStartInfo(\"powershell")
            .And.NotContain("runas");
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
