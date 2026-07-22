using FluentAssertions;

namespace Css.Tests;

public sealed class AcceptanceFixturePackageScriptTests
{
    [Fact]
    public void Publisher_creates_a_new_separate_fixture_artifact_without_running_it()
    {
        var script = Read("scripts", "publish-acceptance-fixture-kit.ps1");

        script.Should().Contain("Css.AcceptanceFixtures.csproj")
            .And.Contain("DisposableAcceptanceFixtureKit")
            .And.Contain(".artifacts")
            .And.Contain("OutputDirectory already exists")
            .And.Contain("dotnet publish")
            .And.Contain("package-manifest.json")
            .And.Contain("Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256")
            .And.Contain("MutationCommandsRequireAttestation = $true")
            .And.Contain("PrimaryMachineAllowed = $false");

        script.Should().NotContain("Css.App.csproj")
            .And.NotContain("Css.Elevated.csproj")
            .And.NotContain("Start-Process")
            .And.NotContain("Remove-Item")
            .And.NotContain("Move-Item")
            .And.NotContain("Css.AcceptanceFixtures.exe provision")
            .And.NotContain("Css.AcceptanceFixtures.exe reset");
    }

    [Fact]
    public void Verifier_rechecks_exact_fixture_payload_and_compiled_authority_tokens()
    {
        var script = Read("scripts", "verify-acceptance-fixture-kit.ps1");

        script.Should().Contain("DisposableAcceptanceFixtureKit")
            .And.Contain("MutationCommandsRequireAttestation")
            .And.Contain("PrimaryMachineAllowed")
            .And.Contain("Get-FileHash -LiteralPath $fullPath -Algorithm SHA256")
            .And.Contain("Fixture file length verification failed")
            .And.Contain("Fixture file hash verification failed")
            .And.Contain("Unlisted fixture payload file")
            .And.Contain("Css.AcceptanceFixtures.exe")
            .And.Contain("Css.AcceptanceFixtures.dll")
            .And.Contain("Css.AcceptanceFixtures.deps.json")
            .And.Contain("Css.AcceptanceFixtures.runtimeconfig.json")
            .And.Contain("I CONFIRM THIS IS A DISPOSABLE WINDOWS TEST ENVIRONMENT")
            .And.Contain("FixtureKitVerified = $true");

        foreach (var forbidden in new[]
                 {
                     "Start-Process",
                     "Set-Content",
                     "Add-Content",
                     "WriteAllText",
                     "Copy-Item",
                     "Move-Item",
                     "Remove-Item",
                     "Set-ItemProperty",
                     "New-ItemProperty",
                     "Import-PfxCertificate",
                     "New-SelfSignedCertificate"
                 })
        {
            script.Should().NotContain(forbidden);
        }
    }

    [Fact]
    public void Acceptance_session_and_receipt_bind_the_verified_fixture_manifest()
    {
        var initializer = Read("scripts", "new-disposable-acceptance-session.ps1");
        var verifier = Read("scripts", "verify-disposable-acceptance-receipt.ps1");

        initializer.Should().Contain("FixtureKitDirectory")
            .And.Contain("verify-acceptance-fixture-kit.ps1")
            .And.Contain("FixtureManifestSHA256")
            .And.Contain("FixtureKitVerified");
        IndexOf(initializer, "FixtureKitVerified")
            .Should().BeLessThan(IndexOf(initializer, "New-Item -ItemType Directory"));

        verifier.Should().Contain("FixtureKitDirectory")
            .And.Contain("verify-acceptance-fixture-kit.ps1")
            .And.Contain("FixtureManifestSHA256")
            .And.Contain("FixtureKitVerified");
    }

    [Fact]
    public void Operator_guide_keeps_fixture_commands_manual_disposable_and_resettable()
    {
        var guide = Read("docs", "release", "disposable-windows-acceptance.zh-CN.md");

        guide.Should().Contain("publish-acceptance-fixture-kit.ps1")
            .And.Contain("verify-acceptance-fixture-kit.ps1")
            .And.Contain("Css.AcceptanceFixtures.exe\" provision")
            .And.Contain("Css.AcceptanceFixtures.exe\" status")
            .And.Contain("Css.AcceptanceFixtures.exe\" lock")
            .And.Contain("Css.AcceptanceFixtures.exe\" reset")
            .And.Contain("不能在主电脑运行")
            .And.Contain("不会进入用户发布包")
            .And.Contain("FixtureKitDirectory");
    }

    private static int IndexOf(string text, string value)
    {
        var index = text.IndexOf(value, StringComparison.Ordinal);
        index.Should().BeGreaterThanOrEqualTo(0, value);
        return index;
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
