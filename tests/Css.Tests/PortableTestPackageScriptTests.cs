using FluentAssertions;

namespace Css.Tests;

public sealed class PortableTestPackageScriptTests
{
    [Fact]
    public void Package_output_is_new_timestamped_artifact_and_never_replaces_existing_content()
    {
        var script = ReadScript();

        script.Should().Contain(".artifacts");
        script.Should().Contain("yyyyMMdd-HHmmss");
        script.Should().Contain("[IO.Path]::IsPathRooted");
        script.Should().NotContain("[IO.Path]::IsPathFullyQualified");
        script.Should().Contain("Test-Path -LiteralPath $OutputDirectory");
        script.Should().Contain("throw \"Output already exists:");
        script.Should().NotContain("Remove-Item");
        script.Should().NotContain("-Force");
    }

    [Fact]
    public void Package_publishes_and_verifies_the_app_worker_and_rules()
    {
        var script = ReadScript();

        script.Should().Contain("dotnet publish");
        script.Should().Contain("src\\Css.App\\Css.App.csproj");
        script.Should().Contain("src\\Css.Elevated\\Css.Elevated.csproj");
        script.Should().Contain("--no-restore");
        script.Should().Contain("Css.App.exe");
        script.Should().Contain("Css.Elevated.exe");
        script.Should().Contain("rules.scan.json");
        script.Should().Contain("Required package file is missing");
    }

    [Fact]
    public void Package_records_hash_signature_runtime_and_mutation_readiness_truth()
    {
        var script = ReadScript();

        script.Should().Contain("Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256");
        script.Should().Contain("MakeRelativeUri");
        script.Should().NotContain("[IO.Path]::GetRelativePath");
        script.Should().Contain("Get-AuthenticodeSignature -LiteralPath $appPath");
        script.Should().Contain("Get-AuthenticodeSignature -LiteralPath $workerPath");
        script.Should().Contain("ValidSameSigner");
        script.Should().Contain("BlockedUntilValidSameSignerPackage");
        script.Should().Contain("FrameworkDependent");
        script.Should().Contain("package-manifest.json");
        script.Should().Contain("README-TEST.txt");
        script.Should().Contain("Compress-Archive");
    }

    [Fact]
    public void Package_script_cannot_sign_import_certificates_or_relax_execution_trust()
    {
        var script = ReadScript();
        var readme = ReadRepositoryFile("scripts", "README-PORTABLE-TEST.zh-CN.txt");

        script.Should().NotContain("Set-AuthenticodeSignature");
        script.Should().NotContain("Import-PfxCertificate");
        script.Should().NotContain("certutil");
        script.Should().NotContain("signtool");
        script.Should().NotContain("TrustedPublisher");
        script.Should().NotContain("ExecutionPolicy Unrestricted");
        script.Should().NotContain("ExecutionPolicy Bypass");
        script.Should().Contain("README-PORTABLE-TEST.zh-CN.txt");
        readme.Should().Contain(".NET 8 Desktop Runtime");
        readme.Should().Contain("read-only");
        readme.Should().Contain("package-manifest.json");
    }

    private static string ReadScript() => ReadRepositoryFile(
        "scripts",
        "publish-portable-test-package.ps1");

    private static string ReadRepositoryFile(params string[] segments) =>
        File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            Path.Combine(segments)));

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
