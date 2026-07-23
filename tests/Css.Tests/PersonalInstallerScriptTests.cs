using FluentAssertions;

namespace Css.Tests;

public sealed class PersonalInstallerScriptTests
{
    [Fact]
    public void Inno_definition_is_visible_D_first_and_refuses_silent_setup()
    {
        var script = Read("installer", "OMNIX-Entropy.iss");

        script.Should().Contain("DefaultDirName=D:\\Software\\OMNIX-Entropy\\Install")
            .And.Contain("DisableDirPage=no")
            .And.Contain("PrivilegesRequired=lowest")
            .And.Contain("Result := not WizardSilent")
            .And.Contain("SignedUninstaller=yes")
            .And.Contain("SignTool=omnix")
            .And.Contain("MessagesFile: \"Languages\\ChineseSimplified.isl\"")
            .And.Contain("ChangesAssociations=no")
            .And.Contain("ChangesEnvironment=no")
            .And.Contain("Source: \"{#SourcePackage}\\*\"")
            .And.Contain("Uninstallable=yes");
    }

    [Fact]
    public void Installer_carries_the_version_matched_simplified_Chinese_translation()
    {
        var translation = Read("installer", "Languages", "ChineseSimplified.isl");
        var notice = Read("installer", "THIRD-PARTY-NOTICES.md");

        translation.Should().Contain("Inno Setup version 6.5.0+ Chinese Simplified messages")
            .And.Contain("LanguageID=$0804")
            .And.Contain("LanguageName=");
        notice.Should().Contain("jrsoftware/issrc")
            .And.Contain("is-6_7_3")
            .And.Contain("7D544B9BB1D142CFA11F2E5D3CC8ABE2E55F8E066C5124E3772675AA236E1278")
            .And.Contain("75EC648A9C1B547B1C35113B06BC85CEDE51C1C1D7D089AF8FD974331F930570")
            .And.Contain("Inno Setup License");
    }

    [Fact]
    public void Builder_requires_verified_payload_explicit_tools_and_same_signer()
    {
        var script = Read("scripts", "build-personal-installer.ps1");

        script.Should().Contain("verify-signed-release-candidate.ps1")
            .And.Contain("PackageKind -ne \"SignedReleaseCandidate\"")
            .And.Contain("Installer signer must match the signed App and worker")
            .And.Contain("ISCC.exe")
            .And.Contain("signtool.exe")
            .And.Contain("Cert:\\CurrentUser\\My")
            .And.Contain("TimestampUrl must be an absolute HTTPS endpoint or the approved official HTTP RFC3161 endpoint")
            .And.Contain("\"timestamp.digicert.com\"")
            .And.Contain("$Uri.IsDefaultPort")
            .And.Contain("$Uri.AbsolutePath -eq \"/\"")
            .And.Contain("[string]::IsNullOrEmpty($Uri.Query)")
            .And.Contain("/Somnix=$signCommand")
            .And.Contain("'$q'")
            .And.Contain("cannot be a reparse point")
            .And.Contain("Get-AuthenticodeSignature -LiteralPath $installerPath")
            .And.Contain("installer-manifest.json")
            .And.Contain("SilentInstallAllowed = $false")
            .And.Contain("DirectorySelectionVisible = $true");
        script.Should().NotContain("New-SelfSignedCertificate")
            .And.NotContain("Import-PfxCertificate")
            .And.NotContain("TrustedPublisher")
            .And.NotContain("Remove-Item")
            .And.NotContain("Move-Item")
            .And.NotContain("--force");
    }

    [Fact]
    public void Installer_verifier_is_read_only_and_rechecks_every_release_boundary()
    {
        var script = Read("scripts", "verify-personal-installer.ps1");

        script.Should().Contain("fully qualified local path")
            .And.Contain("Fixed local drive")
            .And.Contain("ReparsePoint")
            .And.Contain("PersonalWindowsInstaller")
            .And.Contain("D:\\Software\\OMNIX-Entropy\\Install")
            .And.Contain("Installer file length verification failed")
            .And.Contain("Installer file hash verification failed")
            .And.Contain("Get-AuthenticodeSignature -LiteralPath $installerPath")
            .And.Contain("TimeStamperCertificate")
            .And.Contain("SourcePackageSignerThumbprint")
            .And.Contain("CanStageGitHubRelease = $true");

        foreach (var forbidden in new[]
                 {
                     "Start-Process",
                     "Remove-Item",
                     "Move-Item",
                     "Copy-Item",
                     "New-Item",
                     "Set-Content",
                     "WriteAllText",
                     "Set-ItemProperty",
                     "Import-PfxCertificate",
                     "New-SelfSignedCertificate",
                     "signtool"
                 })
        {
            script.Should().NotContain(forbidden);
        }
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
