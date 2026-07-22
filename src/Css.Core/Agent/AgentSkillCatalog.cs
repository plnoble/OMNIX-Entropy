using System.Collections.Generic;
using Css.Core.Operations;

namespace Css.Core.Agent;

public enum AgentSkillCategory
{
    SystemDiagnosis,
    SystemSettings,
    Troubleshooting,
    WindowAndDesktop,
    ProcessAndServiceManagement,
    HardwareInfo,
    SystemTools,
    InputAndSession
}

public enum AgentExecutionMode
{
    ReadOnly,
    ExplainOnly,
    PlanOnly,
    OpenSystemTool
}

public sealed class AgentSkill
{
    public required AgentSkillCategory Category { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required AgentExecutionMode ExecutionMode { get; init; }
    public required RiskLevel Risk { get; init; }
}

public sealed class AgentSkillCatalog
{
    public required IReadOnlyList<AgentSkill> Skills { get; init; }

    public static AgentSkillCatalog CreateDefault() =>
        new()
        {
            Skills =
            [
                new()
                {
                    Category = AgentSkillCategory.SystemDiagnosis,
                    Name = "系统体检",
                    Description = "磁盘空间、垃圾扫描、性能、内存、进程、电池、自启动和使用习惯分析。",
                    ExecutionMode = AgentExecutionMode.ReadOnly,
                    Risk = RiskLevel.None
                },
                new()
                {
                    Category = AgentSkillCategory.SystemSettings,
                    Name = "设置优化建议",
                    Description = "解释网络、声音、显示、电源计划等设置，生成建议但不直接修改。",
                    ExecutionMode = AgentExecutionMode.PlanOnly,
                    Risk = RiskLevel.Medium
                },
                new()
                {
                    Category = AgentSkillCategory.Troubleshooting,
                    Name = "故障排查",
                    Description = "定位网络、驱动、应用崩溃、蓝屏、音频和显示问题。",
                    ExecutionMode = AgentExecutionMode.PlanOnly,
                    Risk = RiskLevel.Medium
                },
                new()
                {
                    Category = AgentSkillCategory.WindowAndDesktop,
                    Name = "桌面整理建议",
                    Description = "分析窗口和桌面图标状态，先给出整理方案。",
                    ExecutionMode = AgentExecutionMode.PlanOnly,
                    Risk = RiskLevel.Low
                },
                new()
                {
                    Category = AgentSkillCategory.ProcessAndServiceManagement,
                    Name = "启停服务",
                    Description = "查看服务和启动项，启停必须经用户确认和回滚检查。",
                    ExecutionMode = AgentExecutionMode.PlanOnly,
                    Risk = RiskLevel.High
                },
                new()
                {
                    Category = AgentSkillCategory.HardwareInfo,
                    Name = "硬件信息查询",
                    Description = "查询 CPU、显卡、内存和 Windows 版本；软件/游戏判断需要官方配置要求。",
                    ExecutionMode = AgentExecutionMode.ReadOnly,
                    Risk = RiskLevel.None
                },
                new()
                {
                    Category = AgentSkillCategory.SystemTools,
                    Name = "打开任务管理器",
                    Description = "一键打开 Windows 系统工具，不替用户结束进程。",
                    ExecutionMode = AgentExecutionMode.OpenSystemTool,
                    Risk = RiskLevel.Low
                },
                new()
                {
                    Category = AgentSkillCategory.InputAndSession,
                    Name = "会话控制建议",
                    Description = "锁屏、休眠、关机、重启等只给出确认入口，不自动执行。",
                    ExecutionMode = AgentExecutionMode.PlanOnly,
                    Risk = RiskLevel.High
                }
            ]
        };
}
