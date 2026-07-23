using FluentAssertions;

namespace Css.Tests;

public sealed class SignedReleasePackageScriptTests
{
    [Fact]
    public void Signed_release_transforms_a_verified_artifact_into_a_new_output()
    {
        var script = Read("scripts", "publish-signed-release-package.ps1");

        script.Should().Contain("SourcePackageDirectory")
            .And.Contain("OutputDirectory")
            .And.Contain("SignToolPath")
            .And.Contain("CertificateThumbprint")
            .And.Contain("TimestampUrl")
            .And.Contain(".artifacts")
            .And.Contain("PortableTestPackage")
            .And.Contain("ProductionOnly")
            .And.Contain("ReparsePoint")
            .And.Contain("Source manifest does not cover required package file")
            .And.Contain("Test-Path -LiteralPath $OutputDirectory")
            .And.Contain("Output already exists")
            .And.Contain("Copy-Item -LiteralPath $sourceItem.FullName");

        script.Should().NotContain("Remove-Item")
            .And.NotContain("Move-Item")
            .And.NotContain("-Force");
    }

    [Fact]
    public void Certificate_must_already_exist_with_private_key_and_code_signing_eku()
    {
        var script = Read("scripts", "publish-signed-release-package.ps1");

        script.Should().Contain("Cert:\\CurrentUser\\My")
            .And.Contain("HasPrivateKey")
            .And.Contain("NotBefore")
            .And.Contain("NotAfter")
            .And.Contain("1.3.6.1.5.5.7.3.3")
            .And.Contain("1.2.840.113549.1.1.1")
            .And.Contain("RSA")
            .And.Contain("Code-signing certificate")
            .And.Contain("Certificate thumbprint must contain exactly 40 hexadecimal characters")
            .And.Contain("\"timestamp.digicert.com\"")
            .And.Contain("$Uri.IsDefaultPort")
            .And.Contain("$Uri.AbsolutePath -eq \"/\"")
            .And.Contain("[string]::IsNullOrEmpty($Uri.Query)")
            .And.Contain("approved official HTTP RFC3161 endpoint");

        script.Should().NotContain("Import-PfxCertificate")
            .And.NotContain("New-SelfSignedCertificate")
            .And.NotContain("Set-AuthenticodeSignature")
            .And.NotContain("certutil")
            .And.NotContain("TrustedPublisher")
            .And.NotContain("LocalMachine\\Root")
            .And.NotContain("PfxPassword");
    }

    [Fact]
    public void App_and_worker_are_sha256_signed_timestamped_and_reverified_before_manifest()
    {
        var script = Read("scripts", "publish-signed-release-package.ps1");

        script.Should().Contain("signtool.exe")
            .And.Contain("& $resolvedSignTool sign")
            .And.Contain("/sha1")
            .And.Contain("/fd SHA256")
            .And.Contain("/tr $TimestampUrl")
            .And.Contain("/td SHA256")
            .And.Contain("Get-AuthenticodeSignature -LiteralPath $appPath")
            .And.Contain("Get-AuthenticodeSignature -LiteralPath $workerPath")
            .And.Contain("Signature verification failed")
            .And.Contain("Signer thumbprint does not match the requested certificate");

        Count(script, "& $resolvedSignTool sign").Should().Be(2);
        var signing = script.IndexOf("& $resolvedSignTool sign", StringComparison.Ordinal);
        var packageFiles = script.IndexOf("$packageFiles =", StringComparison.Ordinal);
        var manifest = script.IndexOf("$manifest =", StringComparison.Ordinal);
        signing.Should().BeGreaterThan(-1);
        packageFiles.Should().BeGreaterThan(signing);
        manifest.Should().BeGreaterThan(packageFiles);
    }

    [Fact]
    public void Manifest_records_signed_but_not_yet_disposable_machine_accepted_truth()
    {
        var script = Read("scripts", "publish-signed-release-package.ps1");
        var readme = Read("scripts", "README-SIGNED-RELEASE.zh-CN.txt");

        script.Should().Contain("SignedReleaseCandidate")
            .And.Contain("ValidSameSigner = $true")
            .And.Contain("MutationReadiness = \"EligibleForDisposableMachineAcceptance\"")
            .And.Contain("DisposableMachineAcceptance = $false")
            .And.Contain("AwaitingDisposableMachineAcceptance")
            .And.Contain("Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256")
            .And.Contain("package-manifest.json")
            .And.Contain("Compress-Archive");

        readme.Should().Contain("候选发布包")
            .And.Contain("不会导入证书")
            .And.Contain("一次性测试环境")
            .And.Contain("package-manifest.json")
            .And.Contain("不代表已经完成生产验收")
            .And.NotContain("状态：已经完成生产验收");
    }

    private static int Count(string text, string value)
    {
        var count = 0;
        var start = 0;
        while ((start = text.IndexOf(value, start, StringComparison.Ordinal)) >= 0)
        {
            count++;
            start += value.Length;
        }

        return count;
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
