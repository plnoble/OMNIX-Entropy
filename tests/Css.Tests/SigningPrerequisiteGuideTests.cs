using FluentAssertions;

namespace Css.Tests;

public sealed class SigningPrerequisiteGuideTests
{
    [Fact]
    public void Release_index_exposes_the_full_beginner_safe_sequence()
    {
        var index = Read("docs", "release", "README.zh-CN.md");

        index.Should().Contain("signing-prerequisites.zh-CN.md")
            .And.Contain("disposable-windows-acceptance.zh-CN.md")
            .And.Contain("inspect-release-signing-prerequisites.ps1")
            .And.Contain("verify-signed-release-candidate.ps1")
            .And.Contain("BehavioralAcceptanceComplete=true")
            .And.Contain("不要在日常使用的主电脑上执行行为验收");
    }

    [Fact]
    public void Guide_uses_official_references_and_matches_the_supported_local_signing_path()
    {
        var guide = Read(
            "docs",
            "release",
            "signing-prerequisites.zh-CN.md");

        guide.Should().Contain("https://learn.microsoft.com/en-us/windows/apps/windows-sdk/downloads")
            .And.Contain("https://learn.microsoft.com/en-us/dotnet/framework/tools/signtool-exe")
            .And.Contain("https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options")
            .And.Contain("inspect-release-signing-prerequisites.ps1")
            .And.Contain("publish-signed-release-package.ps1")
            .And.Contain("verify-signed-release-candidate.ps1")
            .And.Contain("new-disposable-acceptance-session.ps1")
            .And.Contain("CertificateThumbprint")
            .And.Contain("TimestampUrl")
            .And.Contain("SignToolPath")
            .And.Contain("CurrentUser\\My")
            .And.Contain("CanCreateSignedCandidate")
            .And.Contain("BehavioralAcceptanceComplete=true");
    }

    [Fact]
    public void Guide_refuses_trust_shortcuts_and_does_not_claim_external_steps_are_automated()
    {
        var guide = Read(
            "docs",
            "release",
            "signing-prerequisites.zh-CN.md");

        guide.Should().Contain("自签名证书不用于公开发布")
            .And.Contain("不会自动安装 Windows SDK")
            .And.Contain("不会自动购买、申请或导入证书")
            .And.Contain("不会修改受信任根证书库")
            .And.Contain("必须由你明确确认")
            .And.Contain("当前脚本不支持")
            .And.NotContain("关闭 SmartScreen")
            .And.NotContain("关闭杀毒")
            .And.NotContain("跳过签名")
            .And.NotContain("忽略证书错误");
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
