using FluentAssertions;

namespace Css.Tests;

public sealed class SignedReleaseCandidateVerifierScriptTests
{
    [Fact]
    public void Verifier_requires_fixed_local_non_reparse_candidate_path()
    {
        var script = ReadScript();

        script.Should().Contain("PackageDirectory")
            .And.Contain("IsPathRooted")
            .And.Contain("DriveType")
            .And.Contain("Fixed")
            .And.Contain("ReparsePoint")
            .And.Contain("fully qualified local path");
    }

    [Fact]
    public void Verifier_rechecks_manifest_hash_inventory_and_rejects_extra_payloads()
    {
        var script = ReadScript();

        script.Should().Contain("SignedReleaseCandidate")
            .And.Contain("ProductionOnly")
            .And.Contain("EligibleForDisposableMachineAcceptance")
            .And.Contain("AwaitingDisposableMachineAcceptance")
            .And.Contain("Get-FileHash -LiteralPath $fullPath -Algorithm SHA256")
            .And.Contain("Package file length verification failed")
            .And.Contain("Package file hash verification failed")
            .And.Contain("Unlisted package payload file")
            .And.Contain("Manifest does not cover required package file")
            .And.Contain("Css.App.exe")
            .And.Contain("Css.Elevated.exe")
            .And.Contain("Css.Elevated.dll")
            .And.Contain("rules.scan.json");
    }

    [Fact]
    public void Verifier_rechecks_same_signer_timestamp_and_production_command_surface()
    {
        var script = ReadScript();

        script.Should().Contain("Get-AuthenticodeSignature -LiteralPath $appPath")
            .And.Contain("Get-AuthenticodeSignature -LiteralPath $workerPath")
            .And.Contain("Status.ToString() -ne \"Valid\"")
            .And.Contain("TimeStamperCertificate")
            .And.Contain("1.2.840.113549.1.1.1")
            .And.Contain("Signer certificate must use an RSA public key")
            .And.Contain("CertificatePublicKeyAlgorithm")
            .And.Contain("Signer thumbprints do not match")
            .And.Contain("Signer thumbprint does not match the manifest")
            .And.Contain("official-uninstall-fake-worker")
            .And.Contain("Release worker includes debug-only command surface");
    }

    [Fact]
    public void Success_allows_only_beginning_disposable_acceptance_and_has_no_mutation_authority()
    {
        var script = ReadScript();

        script.Should().Contain("CanBeginDisposableAcceptance = $true")
            .And.Contain("DisposableMachineAcceptance = $false")
            .And.Contain("AwaitingBehavioralAcceptance");

        foreach (var forbidden in new[]
                 {
                     "Start-Process",
                     "Remove-Item",
                     "Move-Item",
                     "Copy-Item",
                     "Set-Content",
                     "Add-Content",
                     "WriteAllText",
                     "Set-ItemProperty",
                     "New-ItemProperty",
                     "Import-PfxCertificate",
                     "New-SelfSignedCertificate",
                     "Set-AuthenticodeSignature",
                     "signtool",
                     "ExecutionPolicy Bypass"
                 })
        {
            script.Should().NotContain(forbidden);
        }
    }

    private static string ReadScript() => Read(
        "scripts",
        "verify-signed-release-candidate.ps1");

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
