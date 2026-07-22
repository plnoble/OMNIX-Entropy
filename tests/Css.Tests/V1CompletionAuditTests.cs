using FluentAssertions;

namespace Css.Tests;

public sealed class V1CompletionAuditTests
{
    [Fact]
    public void Audit_preserves_the_full_v1_scope_and_external_acceptance_boundary()
    {
        var audit = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "docs",
            "development",
            "v1-completion-audit.zh-CN.md"));

        foreach (var requirement in new[]
                 {
                     "首页体检",
                     "应用管理",
                     "按最近安装",
                     "C 盘清理",
                     "增长追踪",
                     "大文件 / 重复文件",
                     "安装管控",
                     "卸载干净点",
                     "迁移闭环",
                     "启动项",
                     "后悔药中心",
                     "AI Agent",
                     "OperationPipeline",
                     "正式签名候选",
                     "一次性 Windows 行为验收"
                 })
        {
            audit.Should().Contain(requirement);
        }

        audit.Should().Contain("本地实现已接通")
            .And.Contain("待外部行为验收")
            .And.Contain("不能判定整个目标完成")
            .And.Contain("GUI 可视验收：Warn");
    }

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
