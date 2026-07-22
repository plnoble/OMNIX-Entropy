using FluentAssertions;

namespace Css.Tests;

public sealed class DisposableAcceptanceProtocolScriptTests
{
    private static readonly string[] RequiredCaseIds =
    [
        "package-preflight",
        "uac-cancel-official-uninstall",
        "uac-cancel-migration",
        "cleanup-quarantine-restore",
        "app-cache-quarantine-restore",
        "startup-disable-restore",
        "official-uninstall-residue-review",
        "migration-complete-closure-monitor",
        "migration-failure-rollback",
        "undo-center-restore"
    ];

    [Fact]
    public void Session_initializer_requires_disposable_attestation_and_preflights_before_output()
    {
        var script = Read("scripts", "new-disposable-acceptance-session.ps1");

        script.Should().Contain("EnvironmentKind")
            .And.Contain("WindowsSandbox")
            .And.Contain("VirtualMachine")
            .And.Contain("DedicatedTestMachine")
            .And.Contain("PrimaryMachine")
            .And.Contain("IsDisposableEnvironment")
            .And.Contain("ResetCheckpointId")
            .And.Contain("OperatorAttestation")
            .And.Contain("I CONFIRM THIS IS A DISPOSABLE WINDOWS TEST ENVIRONMENT")
            .And.Contain("verify-signed-release-candidate.ps1")
            .And.Contain("CanBeginDisposableAcceptance")
            .And.Contain("Test-Path -LiteralPath $SessionDirectory")
            .And.Contain("SessionDirectory already exists");

        IndexOf(script, "CanBeginDisposableAcceptance")
            .Should().BeLessThan(IndexOf(script, "New-Item -ItemType Directory"));
    }

    [Fact]
    public void Session_template_contains_every_required_manual_case_once()
    {
        var script = Read("scripts", "new-disposable-acceptance-session.ps1");

        foreach (var caseId in RequiredCaseIds)
            Count(script, $"\"{caseId}\"").Should().Be(1, caseId);

        script.Should().Contain("NotRun")
            .And.Contain("CandidateManifestSHA256")
            .And.Contain("SignerThumbprint")
            .And.Contain("SessionManifestSHA256")
            .And.Contain("acceptance-receipt.template.json")
            .And.Contain("evidence");
    }

    [Fact]
    public void Session_initializer_never_launches_the_product_or_automates_uac()
    {
        var script = Read("scripts", "new-disposable-acceptance-session.ps1");

        foreach (var forbidden in new[]
                 {
                     "Start-Process",
                     "Css.App.exe",
                     "Css.Elevated.exe",
                     "SendKeys",
                     "UIAutomation",
                     "InvokeVerb",
                     "runas",
                     "Set-ItemProperty",
                     "New-ItemProperty",
                     "Remove-Item",
                     "Move-Item"
                 })
        {
            script.Should().NotContain(forbidden);
        }
    }

    [Fact]
    public void Receipt_verifier_requires_exact_passed_case_set_and_reset_attestation()
    {
        var script = Read("scripts", "verify-disposable-acceptance-receipt.ps1");

        foreach (var caseId in RequiredCaseIds)
            Count(script, $"\"{caseId}\"").Should().Be(1, caseId);

        script.Should().Contain("Receipt contains a duplicate case id")
            .And.Contain("Receipt does not contain the exact required case set")
            .And.Contain("Outcome -ne \"Pass\"")
            .And.Contain("FinalVerdict -ne \"Pass\"")
            .And.Contain("ResetCompleted")
            .And.Contain("ResetCompletedUtc")
            .And.Contain("PrimaryMachine -ne $false")
            .And.Contain("IsDisposableEnvironment -ne $true");
    }

    [Fact]
    public void Receipt_verifier_binds_candidate_session_and_every_evidence_file()
    {
        var script = Read("scripts", "verify-disposable-acceptance-receipt.ps1");

        script.Should().Contain("verify-signed-release-candidate.ps1")
            .And.Contain("package-manifest.json")
            .And.Contain("CandidateManifestSHA256")
            .And.Contain("SessionManifestSHA256")
            .And.Contain("SignerThumbprint")
            .And.Contain("Get-FileHash -LiteralPath $evidencePath -Algorithm SHA256")
            .And.Contain("Evidence file length verification failed")
            .And.Contain("Evidence file hash verification failed")
            .And.Contain("Evidence path must be relative and remain inside the evidence directory")
            .And.Contain("BehavioralAcceptanceComplete = $true");
    }

    [Fact]
    public void Receipt_verifier_is_read_only_and_protocol_requires_manual_uac_and_fixtures()
    {
        var script = Read("scripts", "verify-disposable-acceptance-receipt.ps1");
        var protocol = Read("docs", "release", "disposable-windows-acceptance.zh-CN.md");

        foreach (var forbidden in new[]
                 {
                     "Start-Process",
                     "New-Item",
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

        protocol.Should().Contain("人工确认 UAC")
            .And.Contain("人工取消 UAC")
            .And.Contain("不得使用个人文件")
            .And.Contain("重置一次性环境")
            .And.Contain("清理并还原")
            .And.Contain("关闭启动项并还原")
            .And.Contain("官方卸载")
            .And.Contain("迁移失败并回滚")
            .And.Contain("不会自动运行 OMNIX-Entropy");
    }

    private static int IndexOf(string text, string value)
    {
        var index = text.IndexOf(value, StringComparison.Ordinal);
        index.Should().BeGreaterThanOrEqualTo(0, value);
        return index;
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
