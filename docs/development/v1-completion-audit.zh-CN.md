# OMNIX-Entropy V1 完成度审计

审计日期：2026-07-22。审计范围来自用户确认的“直观电脑管家 + AI 运维 Agent”计划，以及此前保留的 C 盘增长、安装管控、迁移闭环和后悔药安全要求。

## 判定规则

- **本地实现已接通**：入口、数据模型、业务逻辑和自动化测试能够形成完整链路；只读功能可以在当前机器验证。
- **待外部行为验收**：修改系统的代码链已经连接，但必须使用正式同签名候选包，在带还原检查点的一次性 Windows 环境验证真实成功、取消、失败、回滚和还原。
- **Warn**：有源码与自动化证据，但缺少当前版本的真实窗口或真实系统行为证据。
- 测试通过只能证明测试覆盖的行为，不能代替正式签名、UAC 人工操作或一次性环境证据。

## 逐项结果

| 要求 | 当前判定 | 直接实现证据 | 自动化证据 | 仍缺什么 |
| --- | --- | --- | --- | --- |
| 六个主入口与页面切换 | 本地实现已接通；GUI 可视验收：Warn | [MainWindow.xaml](../../src/Css.App/MainWindow.xaml) 的首页体检、应用管理、C 盘清理、安装管控、后悔药中心、AI Agent 导航按钮均绑定 `Nav_Click`；[MainWindow.xaml.cs](../../src/Css.App/MainWindow.xaml.cs) 统一切换页面、标题和高亮。 | `ProductExperienceTests` 覆盖六页标识、允许页面集合和处理器连接。 | 当前 Debug 启动通过 Computer Use 超时，随后未发现 OMNIX 窗口，因此本次没有新截图。 |
| 首页体检摘要与关键发现 | 本地实现已接通 | `HealthCheckSummaryBuilder` 生成评分、磁盘、内存、电池、自启动和关键发现；首页提供“让 Agent 解释 / 查看详情 / 生成处理方案”。 | `MachineHealthExperienceTests`、`AutomaticHealthDiagnosisTests`、`HomeKeyFindingsEmptyStateTests`、`CDriveFirstViewHierarchyTests`。 | 当前版本真实窗口首屏截图仍是 Warn。 |
| C 盘清理与决策卡片 | 本地实现已接通；修改动作待外部行为验收 | `Recommendation`、`RecommendationCardPresenter`、`QuarantineOperationPolicy`、`SafetyOperationPipeline` 和 MainWindow 清理确认链；低风险内容进入隔离区。 | `V1FoundationTests`、`RecommendationCleanupPostAttemptTests`、`QuarantineCandidateIdentityTests`、`QuarantineAndTimelineTests`。 | 正式签名候选上的真实清理、取消、隔离和还原证据。 |
| C 盘增长追踪与日报 | 本地实现已接通 | `ScanSnapshotStore` 使用 SQLite 保存快照；`GrowthAnalyzer`、`SoftwareGrowthProfileEnricher`、`HealthDigestStore` 生成增长来源和日报/周报。 | `DiskScannerTests`、`GrowthDecisionTests`、`HealthDigestTests`、`AgentApplicationGrowthAdviceTests`。 | 多日真实使用数据只能随实际使用逐步积累，不应伪造。 |
| 应用管理图标网格和右侧抽屉 | 本地实现已接通 | `AppCatalogPresenter`、`AppTileViewModel`、`AppDrawerViewModel` 与 MainWindow 网格/抽屉连接；支持分类、搜索、风险、占用、按最近安装、增长和名称排序。 | `ProductExperienceTests`、`AutomaticAppInventoryLoadingTests`、`AppDrawerEmptyStateTests`、`AppSizeSummaryTests`。 | 当前版本真实窗口截图仍是 Warn。 |
| 软件画像与 Marvis 归属 | 本地实现已接通（只读） | `SoftwareInventoryScanner` 读取卸载注册表、路径、图标、InstallDate、自启动、服务、任务和进程；`SoftwareInventoryBuilder` 形成共享 `SoftwareProfile`。 | `SoftwareInventoryTests`、`RealMachineSoftwareScanTests`、`BackgroundComponentEvidenceTests`；本机只读测试识别 Marvis 的 D 盘路径、服务、进程和大小。 | 部分软件不提供安装日期或可靠路径时保持未知，不猜测。 |
| 大文件 / 重复文件 | 本地实现已接通（只读建议） | `PersonalStorageAnalysis` 和 `PersonalStorageFindingPresenter` 生成长期未用大文件与疑似重复项，入口位于 C 盘页。 | `PersonalStorageAnalyzerTests`、`PersonalStorageInspectionTests`。 | 用户确认后的真实归档/处理不在主机上冒险执行。 |
| 安装管控、路径学习和安装后报告 | 本地实现已接通；启动安装器待外部行为验收 | `InstallerPackageInspection` 识别 MSI/MSIX/Inno/NSIS/Burn/未知；`InstallRoutingEngine` 映射 `D:\Software`、`D:\Game`、`D:\Agent`、`D:\Development`；`InstallRoutingMemory` 记忆规则；前后快照生成实际落点和 C 盘变化报告。 | `InstallerAnalyzerTests`、`InstallerPackageInspectionTests`、`InstallerExecutionCoordinatorTests`、`InstallSnapshotDiffTests`。 | 正式签名包中的真实安装器启动、人工安装界面选择和安装后报告证据。 |
| 卸载干净点 | 本地实现已接通；待外部行为验收 | 官方卸载器信任检查、快照/恢复证据、最终确认、签名 worker、卸载后扫描、残留风险分组、低风险隔离和时间线均已连接。 | `OfficialUninstallProductionExecutionCoordinatorTests`、`OfficialUninstallOperationHandlerTests`、`UninstallResidueScanTests`、`UninstallPostScanActionTests`。 | 正式候选上的 UAC 接受/取消、真实官方卸载和残留隔离/还原。 |
| 迁移闭环 | 本地实现已接通；待外部行为验收 | `MigrationPlanner`、快照、回滚 manifest、目标空间检查、签名 worker、闭环监控、回滚和原 C 盘继续写入提示均连接到应用抽屉。 | `MigrationSafetyTests`、`MigrationProductionExecutionCoordinatorTests`、`MigrationClosureExperienceTests`、`MigrationExecutionTests`。 | 一次性环境中的成功迁移、继续写入检测、失败回滚和证据复核。 |
| 启动项管理与恢复 | 本地实现已接通；待外部行为验收 | 结构化 HKCU Run 证据、准备、确认、管线执行、回滚 manifest、时间线和恢复链已连接；只有名称线索时拒绝猜测。 | `StartupEntryControlTests`、`StartupControlExperienceTests`、`StartupRestorePipelineTests`、`WindowsStartupEntryStoreTests`。 | 一次性环境中的真实禁用、取消和恢复。 |
| 后悔药中心 | 本地实现已接通；待外部行为验收 | `ActionTimelineStore`、隔离区保留策略、永久整理确认、普通隔离还原和启动项还原均从后悔药中心进入各自管线。 | `QuarantineAndTimelineTests`、`QuarantineRestorePipelineTests`、`QuarantineRetentionGovernanceTests`、`UndoCenterPostAttemptSynchronizationTests`。 | 正式候选上的真实时间线、隔离副本和还原结果。 |
| AI Agent 解释与建议 | 本地实现已接通 | Agent 读取本地摘要，解释体检、应用、增长、后台、自启动和故障线索；输出 `AgentRecommendation` 或页面跳转，不持有删除、迁移、注册表或服务修改权限。 | `AgentConversationTests`、`NaturalLanguageSystemDiagnosisTests`、`AgentApplicationActionHandoffTests`、`AgentStartupAdviceTests`。 | 云端深度分析仍是后续能力；V1 本地规则解释已经可用。 |
| 所有修改必须经过 OperationPipeline | 本地实现已接通；真实修改待外部行为验收 | `OperationDescriptor` 包含证据、风险、快照、回滚、影响范围和确认文本；清理、迁移、卸载、启动项、永久整理和还原都通过 `SafetyOperationPipeline` 或签名 worker 对应的同等边界。 | `V1FoundationTests` 以及各操作的 `*Operation*`、`*FinalConsent*`、`*Authenticated*`、`*OneShot*` 测试组。 | 十项一次性环境收据必须证明运行时没有旁路。 |
| 正式签名候选与一次性 Windows 行为验收 | 待外部行为验收 | 发布脚本、RSA 证书检查、签名后复核、传输复核、夹具包、十项会话和只读收据验证器均已实现。检查器已从标准 `KitsRoot10` 发现 D 盘 SignTool。 | 签名/复核/夹具/协议契约测试，以及全量回归。 | 当前机器没有合格 RSA 代码签名证书，也没有已声明的检查点一次性 Windows 环境。 |

## 本轮新发现与修复

- 原计划要求应用按最近安装排序，当前界面只有风险、占用、增长和名称排序。
- 已从卸载注册表读取 `InstallDate`，仅接受 `yyyyMMdd` 和 `yyyy-MM-dd`；无效或缺失值保持未知。
- 日期进入 `SoftwareProfile`，在增长富化时保留；`RecentInstall` 将有日期的软件按日期降序排列，未知日期排在最后。
- 应用排序下拉框新增稳定 `AutomationId=AppSortComboBox` 和“按最近安装”选项；技术详情可查看已知安装日期。

## 当前结论

V1 的本地只读能力和关键修改链已经从 UI 接到安全执行边界，本轮审计还修复了一个明确遗漏。但真实修改能力尚未在正式同签名候选和一次性 Windows 环境完成十项行为收据，GUI 当前版本也没有新的可视证据，因此**不能判定整个目标完成**。

下一步的唯一发布路径见[发布流程](../release/README.zh-CN.md)：生成正式签名候选，传输后复核，再完成一次性 Windows 行为验收。
