using FluentAssertions;
using System.Diagnostics;
using System.Text.Json;

namespace Css.Tests;

public sealed class SigningPrerequisiteInspectorScriptTests
{
    [Fact]
    public void Inspector_is_read_only_and_never_selects_or_creates_a_certificate()
    {
        var script = Read("scripts", "inspect-release-signing-prerequisites.ps1");

        script.Should().Contain("InspectionMode")
            .And.Contain("ReadOnly")
            .And.Contain("Cert:\\CurrentUser\\My")
            .And.Contain("HasPrivateKey")
            .And.Contain("1.3.6.1.5.5.7.3.3")
            .And.Contain("1.2.840.113549.1.1.1")
            .And.Contain("X509EnhancedKeyUsageExtension")
            .And.Contain("RequiresExplicitCertificateSelection")
            .And.Contain("CanCreateSignedCandidate")
            .And.Contain("MissingRequirements");

        script.Should().NotContain("Import-PfxCertificate")
            .And.NotContain("New-SelfSignedCertificate")
            .And.NotContain("Set-AuthenticodeSignature")
            .And.NotContain("certutil")
            .And.NotContain("TrustedPublisher")
            .And.NotContain("LocalMachine\\Root")
            .And.NotContain("Install-Package")
            .And.NotContain("Start-Process")
            .And.NotContain("& $resolvedSignTool sign")
            .And.NotContain("Remove-Item")
            .And.NotContain("Move-Item")
            .And.NotContain("Copy-Item")
            .And.NotContain("Set-Content")
            .And.NotContain("Out-File");

        script.Should().NotContain("EnhancedKeyUsageList");
    }

    [Fact]
    public void Inspector_uses_only_explicit_path_path_lookup_and_bounded_windows_kits_locations()
    {
        var script = Read("scripts", "inspect-release-signing-prerequisites.ps1");

        script.Should().Contain("SignToolPath")
            .And.Contain("Get-Command signtool.exe")
            .And.Contain("Windows Kits")
            .And.Contain("Microsoft\\Windows Kits\\Installed Roots")
            .And.Contain("KitsRoot10")
            .And.Contain("Get-ItemProperty -LiteralPath $registryPath")
            .And.Contain("WindowsKitsRegistry")
            .And.Contain("signtool.exe")
            .And.Contain("Get-ChildItem -LiteralPath $windowsKitsBin")
            .And.Contain("-Directory")
            .And.NotContain("Get-ChildItem -Path C:\\")
            .And.NotContain("-Recurse");
    }

    [Fact]
    public void Inspector_reports_candidates_without_choosing_one_and_supports_json()
    {
        var script = Read("scripts", "inspect-release-signing-prerequisites.ps1");

        script.Should().Contain("AsJson")
            .And.Contain("ConvertTo-Json")
            .And.Contain("EligibleCodeSigningCertificates")
            .And.Contain("Thumbprint")
            .And.Contain("Subject")
            .And.Contain("NotAfter")
            .And.Contain("PublicKeyAlgorithm")
            .And.Contain("CodeSigningCertificateCount")
            .And.NotContain("SelectedCertificate")
            .And.NotContain("CertificateThumbprint =");
    }

    [Fact]
    public async Task Inspector_returns_json_when_prerequisites_are_missing()
    {
        var scriptPath = Path.Combine(
            FindRepositoryRoot(),
            "scripts",
            "inspect-release-signing-prerequisites.ps1");
        var start = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        start.ArgumentList.Add("-NoProfile");
        start.ArgumentList.Add("-ExecutionPolicy");
        start.ArgumentList.Add("Bypass");
        start.ArgumentList.Add("-File");
        start.ArgumentList.Add(scriptPath);
        start.ArgumentList.Add("-AsJson");

        using var process = Process.Start(start)
            ?? throw new InvalidOperationException("Could not start Windows PowerShell.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));
        var output = await outputTask;
        var error = await errorTask;

        process.ExitCode.Should().Be(0, error);
        using var document = JsonDocument.Parse(output);
        var root = document.RootElement;
        root.GetProperty("InspectionMode").GetString().Should().Be("ReadOnly");
        root.GetProperty("CanCreateSignedCandidate").ValueKind
            .Should().BeOneOf(JsonValueKind.True, JsonValueKind.False);
        root.GetProperty("EligibleCodeSigningCertificates").ValueKind
            .Should().Be(JsonValueKind.Array);
        root.GetProperty("MissingRequirements").ValueKind
            .Should().Be(JsonValueKind.Array);

        var signToolFound = root.GetProperty("SignToolFound").GetBoolean();
        if (signToolFound)
        {
            var signToolPath = root.GetProperty("SignToolPath").GetString();
            signToolPath.Should().NotBeNull();
            Path.IsPathFullyQualified(signToolPath!).Should().BeTrue();
            File.Exists(signToolPath).Should().BeTrue();
            Path.GetFileName(signToolPath).Should().Be("signtool.exe");
            root.GetProperty("SignToolResolution").GetString().Should().BeOneOf(
                "ExplicitPath",
                "Path",
                "WindowsKitsRegistry",
                "WindowsKitsDefault");
        }
        else
        {
            root.GetProperty("SignToolPath").ValueKind.Should().Be(JsonValueKind.Null);
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
