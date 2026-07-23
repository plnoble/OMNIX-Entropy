using FluentAssertions;

namespace Css.Tests;

public sealed class PersonalSigningCertificateScriptTests
{
    [Fact]
    public void Personal_signer_requires_separate_thumbprint_bound_Root_attestation()
    {
        var script = ReadScript();

        script.Should().Contain("I approve CurrentUser OMNIX personal publisher trust")
            .And.Contain("Cert:\\CurrentUser\\My")
            .And.Contain("Cert:\\CurrentUser\\TrustedPeople")
            .And.Contain("Cert:\\CurrentUser\\TrustedPublisher")
            .And.Contain("Cert:\\CurrentUser\\Root")
            .And.Contain("KeyExportPolicy NonExportable")
            .And.Contain("KeyAlgorithm RSA")
            .And.Contain("KeyLength 3072")
            .And.Contain("Type CodeSigningCert")
            .And.Contain("PrivateKeyExported = $false")
            .And.Contain("I approve CurrentUser Root trust for $($certificate.Thumbprint)")
            .And.Contain("Exact thumbprint-bound CurrentUser Root attestation is required")
            .And.Contain("if (-not [string]::IsNullOrWhiteSpace($RootTrustAttestation))")
            .And.Contain("RootStoreModified = $rootStoreModified")
            .And.Contain("RootTrusted = Test-Path");
        script.Should().NotContain("LocalMachine")
            .And.NotContain("Export-PfxCertificate")
            .And.NotContain(".pfx")
            .And.NotContain("password")
            .And.NotContain("Remove-Item");
    }

    [Fact]
    public void Public_certificate_evidence_is_ignored_and_private_material_is_never_named()
    {
        var script = ReadScript();
        var ignore = Read(".gitignore");

        script.Should().Contain(".artifacts")
            .And.Contain("Export-Certificate")
            .And.Contain("OMNIX-Entropy-Personal-Publisher.cer");
        ignore.Should().Contain(".artifacts/")
            .And.Contain("*.cer")
            .And.Contain("*.pfx");
    }

    private static string ReadScript() => Read(
        "scripts",
        "new-personal-signing-certificate.ps1");

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
