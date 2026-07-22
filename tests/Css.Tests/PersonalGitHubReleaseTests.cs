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
            .And.Contain("/quarantine/")
            .And.Contain("*.pfx")
            .And.Contain("*.p12")
            .And.Contain("*.key")
            .And.Contain(".env.*");
        ignore.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Should().NotContain("quarantine/");
    }

    [Fact]
    public void Ci_is_read_only_and_pins_official_actions_to_commits()
    {
        var workflow = Read(".github", "workflows", "ci.yml");

        workflow.Should().Contain("contents: read")
            .And.Contain("actions/checkout@11d5960a326750d5838078e36cf38b85af677262")
            .And.Contain("actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9")
            .And.Contain("dotnet test ComputerSecuritySoftware.slnx --configuration Debug --no-restore")
            .And.Contain("dotnet build ComputerSecuritySoftware.slnx --configuration Release --no-restore")
            .And.Contain(".omx/verify-source-integrity.ps1");
        workflow.Should().NotContain("contents: write")
            .And.NotContain("pull_request_target")
            .And.NotContain("secrets.");
    }

    [Fact]
    public void Personal_release_requires_verified_same_signer_package_and_only_publishes_drafts()
    {
        var script = Read("scripts", "prepare-personal-github-release.ps1");

        script.Should().Contain("verify-signed-release-candidate.ps1")
            .And.Contain("SignedReleaseCandidate")
            .And.Contain("ValidSameSigner")
            .And.Contain("SignerThumbprint")
            .And.Contain("Get-FileHash -LiteralPath $packageAssetPath -Algorithm SHA256")
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
