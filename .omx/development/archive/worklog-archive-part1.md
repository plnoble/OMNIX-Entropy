# Archived worklog (2026-06-17 to 2026-07-11)

Historical entries moved out of `.omx/development/worklog.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

## 2026-06-17

- Initialized the agent collaboration protocol for `Computer Security Software`.
- 读取 GitHub 协议 README/AGENTS/init 脚本；克隆协议仓库到临时目录并运行 init 脚本，生成 11 个协议文件。
- 完成需求与架构设计（见计划存档）。记录开发计划到 `current.md` 与 `decisions.md`（6 项关键决策）。
- Phase 0 进行中：下一步 git init + 装 .NET 8 SDK + 建解决方案。
- `git init -b main` 于项目路径，加 .NET .gitignore。
- `winget install Microsoft.DotNet.SDK.8` → .NET 8 SDK 8.0.422 安装成功。
- 创建 `ComputerSecuritySoftware.sln` + 10 个项目（Css.Core/Win32/Rules/Scanner/InstallGuard/Agent/Snapshot/Elevated/App + tests/Css.Tests）。修正：`dotnet new` 模板不接受 `net8.0-windows` 作 `-f`，改用 `-f net8.0` 后编辑 .csproj TargetFramework 为 net8.0-windows。
- 设置项目引用依赖图；清理默认 Class1.cs。
- 添加 NuGet 包（DI/Sqlite/Serilog/System.Management/Vanara/WPF-UI/FluentAssertions 等）。
- **验证**：`dotnet build` 成功，10 项目，0 警告 0 错误。Phase 0 完成。

## 2026-06-30 - Official uninstall preflight checklist

- Continued the "uninstall cleaner" loop by converting the execution gate into a user-facing preflight checklist.
- TDD red: added tests for missing safe-user steps, all-ready gate behavior, and confirmation view-model exposure.
- Added `OfficialUninstallPreflightChecklistBuilder` with step states `Complete`, `Waiting`, and `Blocked`.
- The checklist covers feature enablement, command trust, uninstaller file existence, pre-uninstall snapshot, official command review, close-app confirmation, and post-uninstall rescan confirmation.
- `OfficialUninstallConfirmationViewModel` now exposes `PreflightChecklist`.
- `UninstallPlanWindow.xaml` renders the preflight checklist before the lower-level execution gate status.
- No official uninstaller process execution path was added.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Official_uninstall_preflight|Official_uninstall_confirmation_exposes_preflight|Official_uninstall_execution_gate"` passed 9/9.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 26/26.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 76/76.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-06-30 - App drawer migration preview

- Continued the app-management loop after the user asked whether everything was done; did not claim completion.
- TDD red: added product tests for C-drive app migration preview, D-drive "already reasonable" status, cache-only migration when the install root is unknown, and system-tool migration blocking.
- Implemented `MigrationSummary` and `MigrationPreviewLines` on `AppDrawerViewModel`.
- Connected migration scoring/advice from `MigrationPlanner` into `AppPresentationBuilder`.
- WPF app drawer now renders a "Migration plan preview" section under the app action buttons.
- The preview explicitly states that no files are moved from the drawer; real migration still needs snapshot, rollback, app-close checks, and post-migration monitoring.
- Recovery note: `ProductExperienceTests.cs` was rewritten as explicit UTF-8/ASCII after a bad PowerShell encoding write corrupted the file.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "App_drawer_shows_migration|App_drawer_marks_d_drive|App_drawer_limits_migration|App_drawer_blocks_migration"` passed 4/4.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 30/30.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 80/80.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-06-30 - Publisher signature trust for external uninstallers

- Continued the official uninstall safety loop by adding a second trust source for external official uninstallers.
- TDD red: added tests for publisher-signed external uninstaller trust, signature mismatch blocking, and execution-gate publisher/signature forwarding.
- Implemented `TrustedPublisherSignature` and `BlockedPublisherSignatureMismatch` trust decisions.
- `OfficialUninstallCommandTrustEvaluator.Evaluate` now accepts expected publisher and executable signature subject evidence.
- `OfficialUninstallExecutionGate` now passes `SoftwareProfile.Publisher` and `SoftwareProfile.SignatureSubject` into command trust evaluation.
- External uninstallers remain blocked unless the normalized publisher text is present in the normalized signature subject.
- No official uninstaller process execution path was added.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "publisher_signed|signature_mismatch|Official_uninstall_command_trust_allows_publisher|Official_uninstall_execution_gate_accepts_publisher"` passed 3/3.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Windows_installer|Official_uninstall_execution_gate_accepts_interactive|Official_uninstall_command_trust|Official_uninstall_execution_gate_blocks_untrusted_shell_command|publisher_signed|signature_mismatch"` passed 11/11.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 23/23.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 73/73.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-06-30 - Safe MSI official uninstall trust

- Continued the official uninstall safety loop by adding safe Windows Installer recognition before any real process execution path exists.
- TDD red: added tests for interactive `msiexec /x {GUID}` trust, silent MSI blocking, MSI install/repair blocking, and execution-gate argument forwarding.
- Implemented `TrustedWindowsInstaller`, `BlockedSilentWindowsInstaller`, and `BlockedUnsafeWindowsInstallerCommand` trust decisions.
- `OfficialUninstallCommandTrustEvaluator.Evaluate` now accepts optional uninstall arguments and recognizes only interactive MSI product uninstall commands.
- `OfficialUninstallExecutionGate` now passes parsed uninstall arguments into the trust evaluator.
- No official uninstaller process execution path was added.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Windows_installer|Official_uninstall_execution_gate_accepts_interactive|Official_uninstall_command_trust|Official_uninstall_execution_gate_blocks_untrusted_shell_command"` passed 8/8.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 20/20.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 70/70.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-06-30 - Official uninstall command trust

- Continued the official uninstall safety loop by adding command trust checks before any real execution path exists.
- TDD red: added product tests for trusted uninstallers inside the install directory, shell-wrapper blocking, outside-install-directory blocking, and gate blocking for suspicious shell commands.
- Implemented `OfficialUninstallCommandTrustEvaluator` and `OfficialUninstallCommandTrustDecision`.
- Integrated command trust into `OfficialUninstallExecutionGate`; untrusted commands block high-risk operation creation.
- Updated `UninstallPlanWindow.xaml` to show command trust summary in the execution gate section.
- No official uninstaller process execution path was added.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Official_uninstall_command_trust|Official_uninstall_execution_gate_blocks_untrusted_shell_command"` passed 4/4.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 16/16.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 66/66.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-06-30 - Official uninstaller execution gate

- Continued the uninstall safety loop by adding a core readiness gate for future official uninstaller execution.
- TDD red: added product tests for default-disabled official uninstaller execution, missing snapshot/close/rescan blockers, and a high-risk `uninstall.official.run` operation descriptor when every precondition is satisfied.
- Implemented `OfficialUninstallExecutionGate` and `OfficialUninstallExecutionReadiness` in `Css.Core.Apps`.
- Connected the gate to `OfficialUninstallConfirmationViewModel`.
- Updated `UninstallPlanWindow.xaml` to show execution gate status and blocking reasons in the uninstall safety plan.
- No process execution path or real uninstaller handler was added.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Official_uninstall_execution_gate|Official_uninstall_confirmation_parses_command_and_requires_safe_preflight"` passed 4/4.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 12/12.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 62/62.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-06-30 - Uninstall residue review UI and safety pipeline

- Continued the V1 app-management safety loop after the user asked whether everything was developed; answered by implementing the next missing slice rather than claiming completion.
- Added the WPF app-drawer button `DrawerResidueReviewButton` for post-uninstall residue review.
- Added `ReviewSelectedUninstallResidueAsync`: rescans software inventory, blocks residue handling when the same software is still installed, shows a beginner-friendly review message, and only lets low-risk cache/log residue proceed after a second confirmation.
- Connected low-risk residue movement to the existing `QuarantineOperationPolicy -> SafetyOperationPipeline -> QuarantineOperationHandler` path, so moved files enter quarantine and the undo timeline.
- Added safe path-existence and bounded size-estimation helpers for residue review.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter UninstallResidueScanTests` passed 6/6.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 59/59.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-06-30

- 将 V1 产品计划落成基础代码骨架：安全操作字段、决策卡片、增长追踪、软件画像、安装路由、迁移计划、后悔药时间线。
- TDD：新增 `V1FoundationTests` 和扩展 `DiskScannerTests`，先观察缺少类型/行为的失败，再补最小实现。
- 扩展 C 盘扫描结果到 `DiskRecommendationBuilder`，可把非预期根目录和临时目录转成可审计建议卡片。
- 替换 WPF 默认空窗口为 V1 仪表盘外壳，展示所有模块入口和决策卡片预览。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，12/12 tests passed。
- **验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- 继续推进到可落地测试：新增 SQLite `ScanSnapshotStore`、`DiskScanSessionBuilder`，将扫描报告、决策卡片、当前快照和增长榜聚合成一次扫描会话。
- WPF `MainWindow` 接入真实只读扫描：输入盘符、开始/取消扫描、显示报告、决策卡片、增长来源榜，并将快照保存到 `%LocalAppData%\ComputerAssistant\data.db`。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，14/14 tests passed。
- **验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **验证**：短暂启动 `Css.App.exe` 成功，进程正常关闭（exitCode=0）。
- **最终串行验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，33/33 tests passed。
- **最终串行验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **最终重复启动烟测**：提权启动命令被审批系统因额度限制拒绝；未绕过。最近一次本轮 WPF 启动烟测已通过（started=True exitCode=0）。
- 继续推进清理确认闭环：本轮目标是把低风险 `clean.temp` 决策卡接入确认对话框，确认后经 `SafetyOperationPipeline -> QuarantineOperationHandler` 移入隔离区并刷新后悔药时间线。
- TDD：新增 `QuarantineOperationPolicy` 测试，先观察缺少策略类型的红灯，再实现只允许 low-risk `clean.temp` 确认执行。
- WPF 决策卡区域新增“确认移动到隔离区”按钮；用户必须选中可执行清理卡并通过 MessageBox 确认，才会复制为 `ConfirmationAccepted=true` 后进入 `SafetyOperationPipeline`。
- 执行成功后写入 `ActionTimelineStore` 并刷新后悔药时间线；不可执行卡片会显示本地策略拒绝原因。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，34/34 tests passed。
- **验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **最终串行验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，34/34 tests passed。
- **最终串行验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- 统一产品命名为 `OMNIX-Entropy`：新增 `AppIdentity` 品牌常量，WPF 标题/侧栏、本地数据目录和默认 D 盘隔离区路径均改用品牌常量。
- TDD：新增 `AppIdentityTests`，先观察缺少 `AppIdentity` 的红灯，再实现品牌常量。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，35/35 tests passed。
- **验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **检查**：`rg "电脑助手|电脑全能助手|ComputerAssistant|CssQuarantine" -n src tests` 未发现旧用户可见命名残留。
- 继续推进软件画像：新增 `SoftwareInventoryBuilder`、`SoftwareInventoryScanner`、安装记录/启动项/服务/计划任务输入模型、`SignatureInspector`。
- 软件画像 scanner 当前只读读取卸载注册表、Run 自启动项和 WMI 服务路径；计划任务扫描尚未接真实来源。
- WPF 新增“扫描软件”按钮和软件画像列表，展示名称、分类、发布者、安装路径、自启动/服务数量、C 盘路径、签名主体。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，18/18 tests passed。
- **验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **验证**：短暂启动 `Css.App.exe` 成功，进程正常关闭（exitCode=0）。
- 继续推进安装管控：新增 `InstallerAnalyzer`，只读识别安装包类型线索、软件名、类别、D 盘推荐安装路径和候选安装参数，不运行安装包。
- WPF 安装位置策略卡新增安装包路径输入、选择文件和“分析安装包”按钮。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，24/24 tests passed。
- **验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **验证**：短暂启动 `Css.App.exe` 成功，进程正常关闭（exitCode=0）。
- 继续推进安装管控闭环：本轮目标是以 TDD 新增安装前/安装后软件画像快照 diff 报告，保持只读，识别新增软件、自启动、服务、计划任务和 C 盘写入点，并接入 WPF。
- TDD：新增 `InstallSnapshotDiffTests`，先观察缺少 `InstallSystemSnapshot` / `InstallSnapshotDiffBuilder` 的红灯，再实现安装快照 diff 报告模型。
- WPF 安装位置策略卡新增“捕获安装前”“捕获安装后”“生成变化报告”，调用软件画像扫描器生成只读快照，并显示新增软件、自启动、服务、计划任务和 C 盘路径。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，26/26 tests passed。
- **验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **验证**：短暂启动 `Css.App.exe` 成功，进程正常关闭（exitCode=0）。
- 继续补软件画像计划任务来源：新增 `ScheduledTaskXmlParser`，只读解析 Windows 计划任务 XML 中的 `Exec/Command`，并在 `SoftwareInventoryScanner` 中枚举 `Windows\System32\Tasks`。
- TDD：新增计划任务 XML 解析测试和计划任务归属测试，先观察缺少 `ScheduledTaskXmlParser` 的红灯，再接入实现。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，28/28 tests passed。
- **验证问题**：并行运行 `dotnet test` 和 `dotnet build` 导致 `Css.Scanner.dll` 被 VBCSCompiler 锁定；执行 `dotnet build-server shutdown` 后串行重跑构建。
- **验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 串行重跑通过，10 projects，0 warnings，0 errors。
- **验证**：短暂启动 `Css.App.exe` 成功，进程正常关闭（exitCode=0）。
- **最终串行验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，28/28 tests passed。
- **最终串行验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **最终启动烟测**：短暂启动 `Css.App.exe` 成功，进程正常关闭（exitCode=0）。
- 继续推进后悔药底座：本轮目标是以 TDD 新增隔离区移动/还原、动作时间线 SQLite 持久化，并把 WPF 后悔药中心接入只读最近动作列表。
- TDD：新增 `QuarantineAndTimelineTests`，先观察缺少 `Css.Core.Quarantine` 的红灯，再实现 `FileQuarantineService`、`QuarantineRecord`、`QuarantineRestoreResult`。
- 新增 `ActionTimelineStore`，用 SQLite 保存/读取最近动作，包含证据、影响路径、还原状态和还原操作类型。
- 新增 `QuarantineOperationHandler`，用于在 `SafetyOperationPipeline` 通过后移动路径到隔离区并写入时间线。
- TDD 补安全预检：如果一个隔离区操作包含多个路径且任一路径缺失，处理器会在移动任何内容前返回失败，避免半执行。
- WPF 后悔药中心新增“加载时间线”按钮和只读列表，读取 `%LocalAppData%\ComputerAssistant\data.db` 中的动作时间线。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，33/33 tests passed。
- **验证问题**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 因 WPF `ItemsSource` 使用无目标类型集合表达式失败；改为普通数组后重跑。
- **验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **验证**：短暂启动 `Css.App.exe` 成功，进程正常关闭（exitCode=0）。
- 修复人工 UI 反馈：C 盘路径从手输改为自动盘符下拉；左侧栏加宽并给 8 个导航按钮接入点击处理；软件画像增加摘要说明并过滤注册表占位符显示名；决策卡片改为“发现 / 建议 / 能不能动”的人话说明；执行区解释隔离区是可回滚暂存。
- TDD：新增软件画像占位符过滤测试，先观察 `${arpDisplayName}` 被错误显示，再在 `SoftwareInventoryBuilder` 过滤 `${...}` / `%...%` display name。
- **验证问题**：构建首次因 UI 映射使用不存在的 `RecommendationAction.FixInstallPath` 失败；改用实际枚举 `RepairInstallLocation` 后重跑成功。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，36/36 tests passed。
- **验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **UI 验证**：短暂启动 `Css.App.exe` 成功并正常关闭，截图 `.omx\qa-omnix-ui-current.png` 目检确认 `OMNIX-Entropy` 左侧标题未裁切。
- **UI 自动化验证**：Windows UI Automation 逐个触发 8 个左侧导航按钮，全部返回 `invoked`。
- 收到用户反馈：左侧按钮实际点击仍“没有反应”，且开发节奏过快、未充分确认用户真正要的产品体验。暂停继续实现，切换到产品计划澄清；记录教训：UI Automation 的 `Invoke` 只能证明按钮可触发，不能证明用户感知到导航结果。
- 用户批准新的 V1 产品重构计划：OMNIX-Entropy 改为直观电脑管家 + AI 运维 Agent。前台首页采用体检摘要，应用管理采用图标网格 + 右侧抽屉；软件画像只做后台引擎。Marvis 能力表纳入 Agent 技能目录参考，V1 按安全等级展示/解释/建议，不直接执行危险系统改动。
- TDD：新增 `ProductExperienceTests`，先观察缺少首页摘要、应用卡片/抽屉、Agent 技能目录和卸载计划模型的红灯，再实现 `HealthCheckSummary`、`AppPresentationBuilder`、`AgentSkillCatalog`、`UninstallPlan`。
- TDD：新增软件画像运行进程归属测试，先观察缺少 `ProcessEntry` 和 `runningProcesses` 输入，再扩展 `SoftwareInventoryScanner` 只读读取当前进程并归属到 `SoftwareProfile.RunningProcesses`。
- WPF 主窗口完成信息架构重构：左侧导航改为真实页面切换；首页展示体检摘要；应用管理改为图标网格 + 右侧抽屉；技术详情默认隐藏；AI Agent 页展示按 Marvis 能力表改写的安全分级技能目录。
- 修复构建红灯：主窗口 code-behind 仍引用旧 `RootCauseMetricTextBlock`、`SoftwareListBox` 等控件；改为绑定新 `HealthDimensionListView`、`AppTilesListBox`、`AppDrawer` 控件。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` 通过，4/4 tests passed。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Profile_builder_attaches_running_processes_by_path_or_name` 通过，1/1 tests passed。
- **最终验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，41/41 tests passed。
- **最终验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **UI 验证**：修正 UI Automation 检查脚本后，逐个点击 `首页体检`、`应用管理`、`C盘清理`、`安装管控`、`后悔药中心`、`AI Agent`，全部返回对应页面标题；截图保存为 `.omx\qa-omnix-v1-refactor-clicks.png`。
- 继续推进应用管理闭环：TDD 新增应用分类/搜索/排序测试和抽屉卸载预览测试，先观察缺少 `AppCatalogPresenter`、`AppCatalogQuery`、`UninstallPreviewLines` 的红灯，再实现核心行为。
- 新增 `AppCatalogPresenter`：支持全部、办公学习、开发工具、游戏娱乐、系统应用、占 C 盘、后台常驻、可卸载过滤；支持按风险、占用、最近增长、名称排序；搜索匹配应用名和发布者。
- 应用抽屉新增卸载方案预览：展示“只生成方案，不会直接卸载”、官方卸载器、低风险残留可进隔离区、高风险残留只解释不自动处理。
- WPF 应用管理页接入 8 个分类按钮、搜索框、排序下拉和卸载方案预览列表；搜索和排序补 `AutomationProperties.Name`，方便自动化和可访问性检查。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` 通过，6/6 tests passed。
- **最终验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，43/43 tests passed。
- **最终验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **UI 验证**：应用管理页 12 个关键控件均被 UI Automation 找到；截图保存为 `.omx\qa-omnix-app-management-loop-accessible.png`。
- Marvis 本机只读验证：PowerShell 观察到 `D:\Software\Marvis` 存在、注册表 `UninstallString` 指向 `D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe`、服务 `MarvisSvr` 存在、进程 `MarvisSvr` 运行、目录体积约 8.3GB。
- 发现真实扫描风险：本机 `Get-CimInstance Win32_Service` 被拒绝访问；仅靠 WMI 会漏服务。新增 `ServiceEntryFactory` 和注册表服务兜底读取 `HKLM\SYSTEM\CurrentControlSet\Services\*\ImagePath`。
- TDD：新增 Marvis 画像测试，覆盖从卸载命令上提安装根、归类 AI、关联服务/进程、填充安装体积；新增显式启用的本机扫描测试 `Real_machine_scan_identifies_marvis_when_enabled`。
- 新增卸载安全方案展示模型 `UninstallPlanPresentationBuilder`，默认 `CanRunOfficialUninstaller=false`，强调只预览、不执行。
- WPF 新增 `UninstallPlanWindow`；点击“卸载干净点”打开结构化方案窗口，展示官方卸载、低风险残留、高风险残留和后悔药提醒。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter SoftwareInventoryTests` 通过，10/10 tests passed。
- **本机验证**：`$env:OMNIX_REAL_MACHINE_TESTS='1'; dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Real_machine_scan_identifies_marvis_when_enabled` 通过，1/1 tests passed。
- **最终验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，47/47 tests passed。
- **最终验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- **未验证**：新卸载安全方案窗口的 GUI 冒烟启动被审批系统因额度限制拒绝，未绕过。
- 继续推进后悔药中心闭环：TDD 新增时间线 manifest 记录/状态更新测试，以及前台还原按钮展示模型测试。
- `ActionTimelineEntry` 新增 `Id` 与 `RestoreManifestPaths`；`ActionTimelineStore` 新增 `restore_manifest_paths_json` 列、旧库自动补列、`UpdateRestoreStateAsync`。
- `QuarantineOperationHandler` 写时间线时记录每个隔离区 manifest，后续 UI 不再靠原路径猜还原位置。
- 新增 `ActionTimelinePresenter`，把时间线条目转成用户能懂的标题、状态、还原按钮文案和 tooltip。
- WPF 后悔药中心每条记录新增“还原/不可还原”按钮；点击还原前二次确认，调用 `FileQuarantineService.RestoreAsync`，原路径已有内容时拒绝覆盖，并回写时间线还原状态。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter QuarantineAndTimelineTests` 通过，8/8 tests passed。
- **最终验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，49/49 tests passed。
- **最终验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- 继续推进隔离区容量/保留期策略：TDD 新增 manifest 只读盘点测试和过期/超容量/已还原整理建议测试。
- `FileQuarantineService.LoadRecordsAsync` 只读枚举隔离区 manifest，跳过不可读目录，不还原、不删除。
- 新增 `QuarantineRetentionPlanner`：生成过期、超容量、已还原候选；`WouldDeleteAutomatically=false`，所有候选都要求确认。
- WPF 后悔药中心新增隔离区策略摘要，显示保留期、容量上限、当前记录数/体积和“只生成建议，不会自动删除”。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter QuarantineAndTimelineTests` 通过，10/10 tests passed。
- **最终验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，51/51 tests passed。
- **最终验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- 继续推进“卸载干净点”：TDD 新增 `UninstallResidueScanTests`，先观察缺少 `UninstallResidueScanBuilder` 和低风险操作 planner 的红灯。
- 新增 `UninstallResidueScanBuilder`：卸载后若仍检测到同软件，则不建议清残留；若软件已消失，则按低/中/高风险分组残留候选。
- 新增 `UninstallResidueOperationPlanner`：仅把低风险缓存/日志路径转为 `uninstall.residue.quarantine` 操作描述；中高风险残留不进入执行计划。
- 扩展 `QuarantineOperationPolicy`，允许 low-risk `uninstall.residue.quarantine` 进入隔离区执行候选，但仍必须用户确认。
- 卸载安全方案窗口新增 `PostUninstallScanLine`，用人话说明卸载后会扫描残留、低风险才可进隔离区、中高风险只解释或需额外快照。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter UninstallResidueScanTests` 通过，4/4 tests passed。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` 通过，7/7 tests passed。
- **最终验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，55/55 tests passed。
- **最终验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。
- 继续推进官方卸载器确认页：TDD 新增确认模型测试，先观察缺少 `OfficialUninstallConfirmationBuilder` 的红灯。
- 新增 `OfficialUninstallConfirmationBuilder`：解析 quoted/unquoted 卸载命令，拆出 executable 和 arguments；生成运行中进程、服务、计划任务提醒和检查清单。
- `UninstallPlanPreviewViewModel` 新增 `OfficialConfirmation`，`UninstallPlanWindow` 新增“官方卸载确认”卡片，展示命令、参数、执行前提醒和检查清单。
- 确认页仍设置 `CanRunOfficialUninstaller=false`，按钮文案为“仅生成确认方案”或“不能运行”，不运行任何进程。
- **验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` 通过，9/9 tests passed。
- **最终验证**：`dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` 通过，57/57 tests passed。
- **最终验证**：`dotnet build ComputerSecuritySoftware.slnx --no-restore` 通过，10 projects，0 warnings，0 errors。

## 2026-07-01 - App drawer top-summary localization

- Continued the app drawer readability pass after operation labels were localized.
- Objective: make the first visible drawer conclusions (`装在哪里`, `占多少`, `是否常驻`, and `Computer Agent 建议`) render as plain Chinese before users open technical details.
- Acceptance criteria: failing product test first, drawer top summaries/advice localized, old English summary phrases absent from production presentation code, and no system-changing action path added.
- Initial test attempt had a compile error because `StringAssertions.NotContain` did not accept `StringComparison`; fixed the test, then observed the expected red because the location summary still said `Installed on D drive; location is reasonable.`
- Implemented Chinese summaries in `LocationSummary`, `SizeSummary`, `ResidencySummary`, and `CreateAgentAdvice`.
- Updated the existing C-drive advice assertion from English `migration plan` to Chinese `迁移方案`.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_drawer_top_summary_uses_plain_chinese_before_technical_details` passed 1/1 after the red/green cycle.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 43/43.
- Final verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 98/98.
- Final verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Static check: `rg -n 'Installed on|Install size|data size|running process\(es\)|Observe for now|Looks normal|Generate a migration plan' src\Css.Core\Apps\AppPresentation.cs tests\Css.Tests\ProductExperienceTests.cs` found only the negative test assertion for `Install size`.
- GUI smoke for reading the drawer summary text was not run because the escalation request was rejected by usage limits; no workaround was attempted.

## 2026-07-01 - App drawer action localization

- Continued V1 app-management polish after Chinese status labels landed.
- Objective: make the app drawer operations read like beginner-friendly Chinese actions instead of English control names.
- Acceptance criteria: failing product test first, app drawer action labels/reasons localized, WPF buttons expose Chinese text through UI Automation, and no system-changing action path is added.
- TDD red: added `App_drawer_actions_use_beginner_friendly_chinese_labels_and_reasons`; the focused test failed because labels were still `Uninstall cleaner`, `Move to D drive`, `Clean cache`, `Disable startup`, and `Technical details`.
- Implemented Chinese action labels and reasons in `AppPresentationBuilder.CreateActions` and localized migration action reasons.
- Updated WPF drawer button content and migration preview title with XML numeric character references to avoid localized-source encoding regressions.
- Updated `MainWindow.xaml.cs` action lookup labels and migration status text to match the new Chinese actions.
- Localized the migration plan window shell labels: title, preflight headers, rollback manifest button, close button, and rollback-manifest confirmation messages.
- GUI smoke launched `Css.App.exe`, scanned apps, selected an app tile, and UIAutomation found `卸载干净点`, `迁移到 D 盘`, `清理缓存`, and `关闭自启动`; none of those operation buttons were invoked.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_drawer_actions_use_beginner_friendly_chinese_labels_and_reasons` passed 1/1 after the red/green cycle.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 42/42.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Final verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 97/97.
- Final verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-07-01 - App tile Chinese status labels

- Continuing V1 app-management polish after app tile accessibility names landed.
- Objective: replace English tile status labels with Chinese beginner-readable labels while keeping source edits ASCII-safe via Unicode escapes.
- Acceptance criteria: failing product test first, no install paths in accessible tile names, WPF builds, GUI smoke reads Chinese status labels from sampled app tiles, and no system-changing path is added.
- TDD red: added `App_tile_status_labels_are_localized_for_beginner_grid` and updated the Marvis tile test; the focused run failed because labels still said `Needs attention` and `Background resident`.
- Implemented Chinese short tags in `AppPresentationBuilder.CreateTile`: `系统组件`, `需关注`, `后台常驻`, `有建议`, and `正常`.
- Used C# Unicode escape literals in production and test code to avoid another localized-source encoding rewrite.
- GUI smoke launched the app, scanned inventory, found 130 app list items, and sampled names such as `火绒安全软件, 需关注`.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "App_tile_status_labels_are_localized|App_presentation_maps_software_profile"` passed 2/2.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 41/41.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Final verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 96/96.
- Final verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-07-01 - App tile accessibility names

- Continuing V1 app-management polish.
- Objective: replace generic `Css.App.MainWindow+AppTileUi` UI Automation names with app-specific beginner-readable names, without exposing technical paths.
- Acceptance criteria: failing product test first, WPF app tiles bind `AutomationProperties.Name`, GUI smoke sees real app names in UI Automation, and existing safety behavior remains unchanged.
- TDD red: added `AccessibilityName` assertions to `App_presentation_maps_software_profile_to_icon_tile_and_beginner_drawer`; initial run failed because `AppTileViewModel` had no `AccessibilityName`.
- Implemented `AppTileViewModel.AccessibilityName`, mapped it through `MainWindow.AppTileUi`, and bound WPF `ListBoxItem.AutomationProperties.Name` plus tile border automation name.
- GUI smoke launched the app, scanned app inventory, found 130 app list items, and read real names such as `火绒安全软件, Needs attention` instead of `Css.App.MainWindow+AppTileUi`.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_presentation_maps_software_profile_to_icon_tile_and_beginner_drawer` passed 1/1.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 40/40.
- Final verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 95/95.
- Final verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-07-01 - Migration rollback manifest UI action

- Continuing the migration safety loop after the user asked to keep developing.
- Objective: add a user-confirmed migration rollback manifest action that writes only JSON evidence and refreshes readiness, without moving app files or enabling migration execution.
- Acceptance criteria: failing test first, manifest write leaves source app/cache paths untouched, presentation shows rollback manifest readiness after creation, WPF build succeeds, and no migration handler is added.
- TDD red: added `Migration_rollback_manifest_creation_writes_json_evidence_without_moving_sources` and `Migration_plan_presentation_marks_rollback_manifest_ready_after_user_confirmed_creation`.
- Implemented `MigrationRollbackManifestCreationService`, extended `MigrationPlanPresentationOptions` with readiness/manifest existence evidence, and added `SuggestedRollbackManifestPath`.
- WPF `MigrationPlanWindow` now has a user-confirmed "Create rollback manifest" action that writes JSON evidence, refreshes readiness, and changes the button to "Rollback manifest saved".
- GUI verification exposed a UX bug: the app search box default text `搜索应用` filtered all scanned apps. Added a failing product test and fixed placeholder handling for `搜索应用` / `搜索软件`.
- GUI smoke after the fix scanned 391 apps, exposed 130 app UI items, opened a migration plan, confirmed rollback-manifest generation, and saved `.omx\qa-migration-manifest-created.png`.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Migration_rollback_manifest_creation|Migration_plan_presentation_marks_rollback"` passed 2/2.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_catalog_ignores_localized_search_placeholder` passed 1/1.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 40/40.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Final verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 95/95.
- Final verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-07-01 - Migration rollback manifest and destination space probe

- Continued the migration safety loop after adding the readiness gate.
- TDD red: added `MigrationSafetyTests` for plan-only rollback manifest building, JSON write/read, destination free-space success/blocking, and graceful probe failure.
- Implemented `MigrationRollbackManifestBuilder`, `MigrationRollbackManifestStore`, and `MigrationDestinationSpaceProbe`.
- Added `MigrationPlanPresentationOptions` so tests and future UI flows can provide deterministic snapshot id, rollback root, timestamp, and free-space provider.
- `MigrationPlanWindow` now displays a rollback manifest draft line and destination free-space line.
- The rollback manifest helper writes only JSON evidence when called; it does not move app files or update system settings.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter MigrationSafetyTests` passed 4/4.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Migration_plan_presentation_shows_manifest"` passed 1/1.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 38/38.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 92/92.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-07-01 - Migration readiness gate

- Continued the migration safety loop after adding the plan-only migration window.
- TDD red: added tests for default-disabled migration execution, missing snapshot/plan/app-close/rollback/space/monitoring blockers, all-ready high-risk operation descriptor creation, and migration plan readiness checklist exposure.
- Implemented `MigrationExecutionGate` and `MigrationExecutionReadiness`.
- Implemented `MigrationPreflightChecklistBuilder`, `MigrationPreflightChecklistViewModel`, and step states.
- Bound the migration readiness checklist into `MigrationPlanWindow`.
- The gate can create a high-risk `migration.execute` descriptor only when all readiness checks pass; no handler or file movement path was added.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Migration_execution_gate|Migration_plan_presentation_exposes_readiness"` passed 4/4.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 37/37.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 87/87.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-07-01 - Migration plan window

- Continued the migration safety loop after the app drawer gained a short migration preview.
- TDD red: added product tests for preview-only migration plan presentation, D-drive monitor-only handling, and system-tool migration blocking.
- Implemented `MigrationPlanPresentationBuilder`, `MigrationPlanPreviewViewModel`, and `MigrationPlanSectionViewModel`.
- Added `MigrationPlanWindow` and wired `DrawerMigrateButton` to `PreviewMigration_Click`.
- The migration plan page shows destination, migration score, blockers, preflight steps, rollback plan, and C-drive monitoring, but still cannot run migration.
- Fixed drawer clearing so stale migration preview lines disappear when app filters produce no selection.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Migration_plan_presentation"` passed 3/3.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 33/33.
- Verification: `dotnet build src\Css.App\Css.App.csproj --no-restore` passed with 0 warnings and 0 errors.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 83/83.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-07-07 - Migration snapshot evidence and plan-confirmation scope

- Continued migration safety without adding any file-moving capability.
- TDD: added `Migration_readiness_checklist_shows_snapshot_evidence_and_plan_confirmation_scope`; observed red because snapshot detail only said `快照 ID` and confirmed-plan detail only said the user viewed the migration plan.
- Updated `MigrationPreflightChecklistBuilder` so the snapshot step says `快照证据：...` and the confirmed-plan step states that the user confirmed target location, affected paths, rollback plan, and post-migration monitoring.
- Verification: focused migration evidence test passed 1/1; `ProductExperienceTests` passed 53/53; full suite passed 110/110; solution build passed with 0 warnings and 0 errors.
- No migration execution handler was added; no app files, services, startup entries, scheduled tasks, or registry keys were changed.

## 2026-07-07 - Post-uninstall residue review inline short-circuit

- Continued the app-management safety loop after GUI verification showed the residue-review button path could be slow and unclear when the app is still installed.
- TDD: added `Residue_review_presentation_is_non_executable_until_user_confirms_a_safe_operation`; observed red because `UninstallResidueReviewViewModel` lacked `SafetyText` and `CanExecuteDirectly`.
- TDD: added `Residue_review_planner_uses_cached_inventory_to_block_when_app_is_still_visible`; observed red because `UninstallResidueReviewPlanner` did not exist.
- Added `UninstallResidueReviewPlanner.TryBuildStillInstalledReport(...)` to reuse the current app inventory: if the selected app is still present at the same install path, the drawer immediately explains that residue handling is blocked.
- Added residue-review safety text and `CanExecuteDirectly=false`; non-actionable reviews now update the app drawer inline instead of opening a modal.
- Kept the existing low-risk residue path unchanged: only after official uninstall appears complete and the user confirms can low-risk cache/log paths enter the quarantine safety pipeline.
- Verification: `UninstallResidueScanTests` passed 8/8; full suite passed 109/109; solution build passed with 0 warnings and 0 errors.
- GUI note: the prior full GUI path was interrupted after proving the old full-rescan path was too slow/unclear; no `Css.App` process remained. Treat GUI proof for this new inline path as still pending.

## 2026-07-07 - C-drive automatic target and collapsed technical report

- Continued the beginner UX pass on the C-drive page after real-scan GUI evidence showed the visible `C:\` selector looked like manual input and the raw technical report still occupied first-screen space.
- TDD: added `C_drive_page_chrome_marks_system_drive_as_automatic_and_hides_technical_report_by_default`; observed red because `CDrivePageChromePresenter` did not exist.
- Added `CDrivePageChromePresenter` to model a read-only automatic system-drive label and default-collapsed technical report.
- Updated WPF header to show `系统盘 C 盘` / automatic detection copy instead of a visible path selector; kept the hidden drive selection source for scanner input.
- Changed the C-drive raw report into an explicit `显示技术报告` toggle with the report hidden by default, keeping root-cause cards and growth cards in the first reading path.
- Verification: focused chrome test passed 1/1; `ProductExperienceTests` passed 52/52; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; full suite passed 107/107.
- GUI verification: real C-drive scan completed with 4 root-cause cards, 3 growth cards, 15 recommendation cards, system-drive label visible, technical report hidden before and after scan. Screenshot: `.omx\qa-cdrive-system-drive-and-collapsed-report.png`.
- No cleanup, quarantine movement, uninstall, migration, service, startup, scheduled-task, or registry operation was invoked.

## 2026-07-07 - Inline homepage Agent response panel

- GUI-verified the homepage key-finding buttons after a real C-drive scan. The first pass proved all three buttons responded, but the screenshot showed stacked modal messages.
- Replaced modal `MessageBox` responses with an inline `Agent 回答` panel above the key-finding list.
- Added `HomeAgentResponsePresenter` and `HomeAgentResponseViewModel` so explain/detail/plan actions share one non-executable page response model.
- Moved the response panel above the list after a screenshot showed the first placement below the list was not visible enough.
- Removed now-unused homepage health-finding MessageBox formatting helpers.
- Verification: focused homepage Agent tests passed 3/3.
- Verification: `ProductExperienceTests` passed 51/51.
- Verification: full suite passed 106/106.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUIA verification: after a real scan, clicking `让 Agent 解释`, `查看详情`, and `生成处理方案` updated the inline panel, found safety/pipeline text, and kept `processWindows=1`. Screenshot: `.omx\qa-home-agent-inline-response-visible.png`.

## 2026-07-07 - Homepage key finding Agent buttons

- Continued the "buttons must respond" UX loop by wiring homepage key-finding buttons.
- Added `HealthFindingAgentExplanationBuilder`, `HealthFindingDetailPresentationBuilder`, and `HealthFindingActionPlanBuilder`.
- Homepage buttons now do something useful: `让 Agent 解释` opens a plain-language explanation, `查看详情` explains where to inspect details, and `生成处理方案` creates a read-only plan. None of these creates or executes a system operation.
- TDD red observed for the missing explanation/detail/plan builders before implementation.
- Verification: focused Agent/detail/plan tests passed 2/2.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 50/50.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 105/105.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Static check: XAML key-finding buttons have `Tag="{Binding}"` and Click handlers; `HealthFindingAgentExplanation.cs` keeps `CanExecuteDirectly=false` and has no operation descriptor creation.

## 2026-07-07 - C-drive beginner summaries and growth cards

- Continued the V1 beginner-facing C-drive UX slice after validating the right-side recommendation cards with a real GUI scan.
- Added `CDriveRootCauseSummaryBuilder` to convert raw top-level C-drive nodes and system big rocks into beginner-readable cards such as user files, software data, temporary cache, system reserved space, and sources needing confirmation.
- Updated the C-drive page to show summary headline/cards first and keep the raw technical report as a smaller secondary section.
- Added `GrowthFindingPresenter` so growth findings no longer expose raw paths by default; they show friendly source, growth amount, plain explanation, and Agent suggestion.
- TDD red observed for both new presenters before implementation.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_root_cause_summary_turns_path_report_into_beginner_cards` passed 1/1.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_growth_presenter_hides_paths_and_explains_change` passed 1/1.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 48/48.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 103/103.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI status: real-scan GUI verification before these left-side summary changes passed for right-side recommendation cards; post-change GUI verification was rejected by usage limits and not bypassed.
- Cleanup: removed the old raw-path `GrowthListBox.ItemsSource` assignment once a stable ASCII anchor was found.
- Verification after cleanup: focused growth presenter test passed 1/1, `ProductExperienceTests` passed 48/48, full suite passed 103/103, and solution build passed with 0 warnings and 0 errors.

## 2026-07-07 - C-drive recommendation card presentation

- Continued the C-drive readability loop after the uninstall safety localization.
- Objective: make C-drive recommendation cards answer `what happened`, `Agent suggestion`, `can undo`, and `expected impact` as separate lines instead of one dense technical safety line.
- Acceptance criteria: failing product test first, presentation extracted out of WPF code-behind, UI binding uses the new fields, no cleanup execution behavior changes, full tests/build pass.
- TDD red: added `C_drive_recommendation_card_explains_happened_agent_advice_undo_and_impact`; it failed because `RecommendationCardPresenter` did not exist.
- Added `RecommendationCardPresenter` and `RecommendationCardViewModel` in `Css.Core.Apps`.
- WPF C-drive recommendations now bind `WhatHappened`, `AgentSuggestion`, `UndoStatus`, `ImpactText`, and `SafetyLine`.
- `ExecuteSelectedRecommendationAsync` now consumes `RecommendationCardViewModel`; the underlying `OperationDescriptor` and safety pipeline were not changed.
- Verification: focused C-drive card test passed 1/1 after red/green.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 46/46.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 101/101.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Static check confirmed active XAML bindings use the new fields and execution selection uses `RecommendationCardViewModel`.
- Cleanup note: an old unused private `RecommendationCardView` still remains in `MainWindow.xaml.cs`; removal was deferred because the historical mojibake block could not be patched safely. It is not referenced by the UI path.

## 2026-07-07 - Uninstall safety copy localization

- Continued V1 readability work on the "uninstall cleaner" flow while keeping official uninstaller execution disabled.
- Objective: make the uninstall safety window, preflight checklist, command trust summary, and app-drawer uninstall preview use beginner-readable Chinese instead of mixed English/internal terms.
- Acceptance criteria: failing product tests first, no process execution path added, official uninstaller remains preview-only, product/full tests and solution build pass, static old-phrase scan is clean except internal keys, and GUIA verifies the real preview modal.
- TDD red 1: added `Uninstall_safety_window_body_uses_plain_chinese_while_official_uninstaller_stays_disabled`; it first failed because the summary did not explicitly contain `只预览`.
- Implemented localized copy in `UninstallPlanPresentationBuilder`, `OfficialUninstallPreflightChecklistBuilder`, `OfficialUninstallConfirmationBuilder`, `OfficialUninstallCommandTrustEvaluator`, and the hidden high-risk operation text in `OfficialUninstallExecutionGate`.
- Updated `UninstallPlanWindow.xaml` preflight header to Chinese XML numeric character references.
- TDD red 2: tightened `App_drawer_contains_uninstall_preview_without_executing_uninstall`; it failed on old drawer text such as `Uninstall preview only` and `First step`.
- Implemented Chinese app-drawer uninstall preview lines in `AppPresentationBuilder.CreateUninstallPreview`.
- Verification: focused uninstall safety-window test passed 1/1 after red/green.
- Verification: focused drawer uninstall preview test passed 1/1 after red/green.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 45/45.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 100/100.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Static check: old uninstall-window/app-drawer phrases no longer appear in the relevant production files; the only match is the internal step key `post-uninstall-rescan`.
- GUI smoke: launched `Css.App.exe`, scanned apps, selected `火绒安全软件`, opened the uninstall safety preview modal, and found `只预览`, `不会运行卸载器`, `卸载前安全检查`, `命令可信度`, `卸载器文件`, and `卸载后重新扫描残留`; old English phrases were absent. No uninstaller was launched and no files were deleted.

## 2026-07-07 - Migration plan body localization

- Continued V1 readability work on the migration safety window, keeping migration preview-only and non-executable.
- Objective: replace the migration plan window's remaining English body copy with beginner-readable Chinese while preserving all safety gates.
- Acceptance criteria: failing product test first, migration page title/summary/banner/destination/rollback/space/checklist/sections localized, no migration execution handler added, full tests/build pass, and GUIA verifies the migration window copy.
- TDD red: added `Migration_plan_presentation_body_uses_plain_chinese_while_staying_preview_only`; it failed because the title still said `Ollama migration plan`.
- Implemented localized migration-plan copy in `MigrationPlanPresentationBuilder`: title, summary, safety banner, destination line, score line, blocking reasons, section titles/details/status labels, migration steps, rollback steps, monitoring lines, rollback manifest line, destination free-space line, and final reminder.
- Implemented localized migration preflight checklist copy in `MigrationPreflightChecklistBuilder`: step titles, status labels, details, next action, and primary action.
- Updated older product tests that still expected English migration presentation text to assert the new Chinese copy while keeping safety-state assertions.
- Verification: focused migration body test passed 1/1 after red/green.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 44/44.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 99/99.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Static check: old migration-window phrases such as `Preview only`, `Suggested destination`, `Rollback plan`, and `Monitoring after migration` no longer appear in production migration presentation/checklist files.
- GUI smoke: launched `Css.App.exe`, scanned apps, opened a migration preview window, and UIAutomation found Chinese migration-window text including `迁移方案`, `只预览`, `不会移动文件`, `迁移前检查`, `回滚方案`, `迁移后观察`, and `生成回滚清单`; no rollback manifest was created and no migration action was executed.

## 2026-07-07 - C-drive recommendation grouping and quarantine explanation

- Continued the C-drive beginner UX slice after user feedback that decision cards were too repetitive and hard to understand.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_recommendation_list_groups_repeated_observe_items_and_explains_quarantine` failed because `RecommendationListPresenter` did not exist.
- Added `RecommendationListPresenter` / `RecommendationListViewModel` in `Css.Core.Apps`.
- Repeated unexpected-root observe recommendations now become one non-executable beginner card such as "needs source confirmation: 4 C-drive root folders"; low-risk cleanup cards keep their original `OperationDescriptor`.
- C-drive WPF binding now uses `RecommendationListPresenter.Create(...)` and shows a stronger quarantine explanation: quarantine is not permanent deletion; it is an undo staging area.
- Verification: focused new test passed 1/1.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 54/54.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 111/111.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: first 120-second real C-drive scan wait did not complete; second longer wait completed and found grouped text plus quarantine explanation. Screenshot: `.omx\qa-cdrive-grouped-recommendations-longwait.png`.
- Visual issue found: the right recommendation list had a horizontal scrollbar because long beginner text was not constrained to wrap.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_recommendation_list_wraps_text_without_horizontal_scroll` failed because `RecommendationsListBox` did not disable horizontal scrolling.
- Fixed `RecommendationsListBox` with disabled horizontal scrolling and stretched item content. Verification passed, then GUI screenshot confirmed wrapped text with no right-side horizontal scrollbar: `.omx\qa-cdrive-grouped-recommendations-wrapped.png`.
- Visual issue found: the execution button stayed enabled even when the selected recommendation was non-executable/observe-only.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_recommendation_execute_button_starts_disabled_until_actionable_card_selected` failed because the list had no selection-change handler and the button was enabled by default.
- Added `RecommendationsListBox_SelectionChanged`; the execution button now starts disabled and only enables for cards with executable low-risk operations.
- Final verification: `ProductExperienceTests` passed 56/56; full suite passed 113/113; solution build passed with 0 warnings and 0 errors.
- GUI verification: real C-drive scan found grouped card and quarantine explanation; execute button name was `选择可清理项后继续` and `IsEnabled=False`. Screenshot: `.omx\qa-cdrive-grouped-button-disabled.png`.

## 2026-07-07 - App drawer residue-review inline result

- Continued the app-management safety loop by making post-uninstall residue review visible and testable in the app drawer without relying on a slow full real-machine scan.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Residue_drawer_inline_status_blocks_cleanup_when_app_still_installed_and_hides_paths` failed because `UninstallResidueDrawerReviewPresenter` did not exist.
- Added `UninstallResidueDrawerReviewPresenter` / `UninstallResidueDrawerReviewViewModel`.
- Still-installed residue review now produces a beginner-readable inline result: conclusion, next step, safety boundary, and evidence. It does not expose local `C:\`/`D:\` paths in the visible drawer text.
- WPF app drawer now has a named uninstall/residue title and uses the new presenter for inline residue results.
- GUI bug found: clicking `卸载后检查残留` showed the status text but `RefreshAppCatalog()` reset the drawer back to the normal uninstall preview.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Residue_review_cached_still_installed_branch_keeps_inline_result_visible` failed because the cached still-installed branch called `RefreshAppCatalog();`.
- Removed that refresh from the cached still-installed branch; the app list has not changed in that path, so the inline result should remain visible.
- GUI bug found: the inline residue result appeared below migration preview and was not visible in the first drawer viewport.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_drawer_places_uninstall_review_before_migration_preview` failed because uninstall/residue UI appeared after migration preview in XAML.
- Moved uninstall preview/residue result above migration preview, directly under the app action buttons.
- GUI bug found: the residue result list had a horizontal scrollbar and did not wrap long sentences.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_drawer_uninstall_review_wraps_text_without_horizontal_scroll` failed because `DrawerUninstallPreviewListBox` had no wrapping template.
- Added disabled horizontal scrolling, stretched content, and a wrapping `TextBlock` item template for `DrawerUninstallPreviewListBox`.
- Verification: `UninstallResidueScanTests` passed 9/9; `ProductExperienceTests` passed 59/59; full suite passed 117/117; solution build passed with 0 warnings and 0 errors.
- GUI verification: read-only app scan found 130 app tiles, selected `火绒安全软件`, clicked `卸载后检查残留`, and found `残留检查结果`, still-installed text, official-uninstall-first text, and no-file-move safety text. Screenshot: `.omx\qa-residue-review-inline-wrapped.png`.
- No official uninstaller was launched, no cleanup was executed, and no files were moved.
## 2026-07-07 - Shared uninstall next-step flow

- Continued the app-management safety loop by making `卸载干净点` drawer preview and the uninstall safety window share one beginner-readable workflow guide.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Uninstall_workflow_guide_is_shared_by_drawer_and_safety_window` failed because `UninstallWorkflowGuidePresenter` did not exist and `UninstallPlanPreviewViewModel` had no `WorkflowGuide`.
- Added `UninstallWorkflowGuidePresenter` / `UninstallWorkflowGuideViewModel` in `Css.Core.Apps`.
- The shared workflow now describes the same six steps in both places: review official uninstaller, close the app, require final confirmation before any future official-uninstall request, return to post-uninstall residue review, move only low-risk cache/log residue to quarantine, and explain/mark medium/high-risk residue without automatic handling.
- `AppPresentationBuilder.CreateUninstallPreview(...)` now returns the shared drawer lines instead of building its own residue preview copy.
- `UninstallPlanPresentationBuilder` now exposes `WorkflowGuide`, and `UninstallPlanWindow.xaml` renders the guide above the detailed official-uninstall preflight cards.
- Real official uninstaller execution remains disabled; no cleanup, migration, service/startup, scheduled-task, registry, or file-move execution path was added.
- Verification: focused shared-flow test passed 1/1.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 60/60.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 118/118.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: first UIA smoke selected `火绒安全软件` and found `DrawerUninstallButton` enabled, but `InvokePattern` did not open the modal; diagnostic screenshot `.omx\qa-uninstall-click-debug.png` captured the state. A safer real mouse-click rerun was requested but rejected by the usage-limit approval system. No workaround was attempted.
## 2026-07-07 - C-drive cleanup selection preview

- Continued C-drive cleanup UX by making recommendation selection state explain the exact next step before any confirmation dialog.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "C_drive_recommendation_selection_preview|C_drive_recommendation_selection_handler"` failed because `RecommendationSelectionPresenter` did not exist.
- Added `RecommendationSelectionPresenter` / `RecommendationSelectionViewModel` in `Css.Core.Apps`.
- Low-risk executable cleanup cards now produce beginner-readable selection text: nothing is cleaned immediately, the next click opens a second confirmation, affected scope and estimated release are shown, confirmed items move to OMNIX-Entropy quarantine, and restore remains available from the undo center.
- Non-executable observe cards keep the button disabled and explain that they are observation/explanation only.
- `RecommendationsListBox_SelectionChanged` now consumes `RecommendationSelectionPresenter.Create(...)` instead of hardcoding the selection-state text in code-behind.
- Real cleanup execution behavior did not change; eligible low-risk cleanup still goes through the existing confirmation, `QuarantineOperationPolicy`, `SafetyOperationPipeline`, and quarantine timeline path.
- Verification: focused selection tests passed 2/2.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 62/62.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 120/120.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Note: A renamed legacy selection handler remains in `MainWindow.xaml.cs` because deleting mojibake-heavy code safely would require a larger code-behind cleanup; it is not referenced by XAML.

## 2026-07-08 - Agent next-step panel

- Continued the AI Agent slice by making the Agent page show beginner-readable next-step guidance from local health summary and app profile signals.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Agent_next_step_panel|Agent_page_contains_next_step"` failed because `AgentNextStepPresenter` did not exist and the Agent page had no next-step panel controls or refresh hooks.
- Added `AgentNextStepPresenter` / `AgentNextStepViewModel` in `src/Css.Core/Agent/AgentNextStepPresentation.cs`.
- The presenter ranks local signals into a top recommendation, reasons, safe next actions, blocked actions, safety boundary, privacy line, and `CanExecuteDirectly=false`.
- The Agent page now has named next-step controls: `AgentNextStepTitleTextBlock`, `AgentNextStepReasonsListBox`, `AgentNextStepActionsListBox`, and `AgentBlockedActionsListBox`.
- `MainWindow` now stores `_lastHealthSummary`, refreshes Agent next steps on startup, after C-drive scans, and after app scans.
- No cloud AI, cleanup, uninstall, migration, service/startup, scheduled-task, registry, installer, or file-move execution path was added.
- Verification: focused Agent next-step tests passed 2/2.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 64/64.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 122/122.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-07-08 - Agent safe navigation actions

- Continued the Agent page by turning "safe next actions" into structured, navigation-only buttons that take the user to the relevant OMNIX-Entropy page.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Agent_next_step_panel|Agent_page_contains_next_step"` failed because `AgentNextStepViewModel.NavigationActions` did not exist.
- Added `AgentNextActionViewModel` with `Label`, `Description`, `TargetPage`, and `IsNavigationOnly=true`.
- `AgentNextStepPresenter` now emits navigation actions for C-drive cleanup, app management, undo center, homepage scan, and app scan contexts.
- `MainWindow.xaml` now includes `AgentNextStepActionButtonsItemsControl`; buttons bind `Label`, `Description`, and `TargetPage`.
- `MainWindow.xaml.cs` now binds `panel.NavigationActions` and handles `AgentNextAction_Click` by allowing only known internal pages and calling `ShowPage(targetPage)`.
- The Agent buttons do not execute cleanup, uninstall, migration, service/startup, scheduled-task, registry, installer, or file-move operations.
- Added clean XML-reference Chinese identity text under the `Computer Agent` title. A duplicate legacy mojibake identity block remains lower in the same XAML area because deleting it safely by patch failed; defer to a focused UTF-8/XAML cleanup pass.
- Verification: focused Agent next-step tests passed 3/3.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 65/65.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 123/123.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-07-08 - Agent left-card XAML cleanup

- Continued the Agent page UX cleanup by removing the duplicate legacy identity copy in the Agent left card.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Agent_left_card_has_single_clean_identity_copy` failed because the Agent left-card XAML still contained the old single-line identity copy with `FontSize="15" Foreground="#4B5563" Margin="0,8,0,0"` and the old description copy with `Margin="0,22,0,0"`.
- Added `ProductExperienceTests.Agent_left_card_has_single_clean_identity_copy` to require the clean XML-reference Chinese identity text, Agent next-step controls, and no duplicate legacy identity copy in the Agent left-card slice.
- Removed the duplicate old identity/description `TextBlock` pair from `src/Css.App/MainWindow.xaml`.
- No cleanup, uninstall, migration, service/startup, scheduled-task, registry, installer, cloud AI, or file-move behavior was changed.
- Verification: focused Agent left-card cleanup test passed 1/1.
- Verification: focused Agent tests passed 4/4.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 66/66.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 124/124.
- Verification: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.

## 2026-07-08 - Active Slice: App Drawer Cache Cleanup Preview

- Objective: Make the app drawer's cache-cleanup action explain what would happen in beginner-friendly language before any cleanup can be requested.
- Dependencies: AppPresentationBuilder, AppDrawerViewModel, MainWindow app drawer bindings, ProductExperienceTests.
- Risks: Must not add real cache deletion or bypass quarantine/safety-pipeline confirmation.
- Impact scope: App drawer presentation and local view model only.
- Acceptance criteria: Cache action exposes a non-executable preview with estimated cache impact, quarantine/undo explanation, no raw path clutter by default, and WPF displays it near the drawer actions.

## 2026-07-08 - App drawer action previews

- Implemented `AppCacheCleanupPreviewPresenter` and `AppStartupControlPreviewPresenter`.
- `AppDrawerViewModel` now exposes cache-cleanup and startup-control preview summaries, beginner-facing lines, and `CanExecuteDirectly=false` flags.
- WPF app drawer now wires `DrawerCleanCacheButton` and `DrawerDisableStartupButton` to preview handlers. Each handler only expands a collapsed preview panel and updates status text; no file, registry, service, startup, or scheduled-task mutation path was added.
- Startup-control button now enables when the app has startup entries, services, or scheduled tasks, not only explicit startup entries.
- TDD red observed: focused cache preview test failed because `AppDrawerViewModel` lacked `CacheCleanup*` fields; focused startup preview test failed because `AppDrawerViewModel` lacked `StartupControl*` fields.
- Verification: focused cache tests passed 2/2; focused startup tests passed 2/2; `ProductExperienceTests` passed 70/70; full suite passed 128/128; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: no GUI screenshot/click-through for the new collapsed preview panels in this slice.

## 2026-07-08 - Real GUI proof and AppData cache candidates

- Added `.omx/gui-app-drawer-preview-smoke.ps1`, a repeatable WPF UIAutomation smoke script. It launches the built app, opens app management, scans apps, clicks cache/startup preview buttons, captures `.omx/qa-app-drawer-action-previews.png`, and closes the process it launched.
- Early GUI smoke failures exposed real issues: the script needed ASCII-safe UI text, Windows PowerShell 5 compatibility, `Scan app` button matching, and the product had too few real `CachePaths` for cache preview to enable.
- Added `SoftwareInventoryBuilder` support for user-data roots, safe path-existence probes, AppData cache/log candidate detection, and cache-size estimation.
- `SoftwareInventoryScanner` now passes LocalAppData, Roaming AppData, and LocalLow roots plus `Directory.Exists` and bounded `EstimateDirectorySize` to the builder.
- TDD red observed: `Profile_builder_infers_appdata_cache_candidates_for_drawer_preview` failed because builder had no `userDataRoots` parameter; `Software_scanner_feeds_appdata_roots_to_profile_builder_for_cache_previews` failed because scanner did not pass AppData roots.
- Verification: focused cache-candidate tests passed 2/2; `SoftwareInventoryTests` passed 11/11; `ProductExperienceTests` passed 71/71; full suite passed 130/130; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: `.omx/gui-app-drawer-preview-smoke.ps1` passed with `cachePreviewVisible=True` and `startupPreviewVisible=True`; screenshot `.omx/qa-app-drawer-action-previews.png`.
- Safety state: scanner only reads directories and estimates bounded sizes. No cleanup, delete, quarantine move, registry edit, service/startup/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added.

## 2026-07-08 - Active Slice: Nested browser and Electron cache attribution

- Objective: Expand read-only cache attribution from direct AppData app folders to common nested layouts such as browser profile caches and Electron `User Data` caches.
- Dependencies: `SoftwareInventoryBuilder`, `SoftwareProfile`, `SoftwareInventoryTests`.
- Risks: Must avoid broad fuzzy matching that attributes unrelated vendor folders to the wrong app; must not add cleanup execution.
- Impact scope: Software profile evidence only. App drawer previews consume the new evidence but execution remains disabled/gated.
- Acceptance criteria: Builder recognizes `Vendor\App\User Data\Default\Cache`, `Vendor\App\User Data\Default\Code Cache`, and `App\User Data\Cache` as cache candidates, includes relevant data/C-drive evidence, and keeps behavior read-only.

## 2026-07-08 - Nested browser and Electron cache attribution

- Implemented conservative nested AppData attribution in `SoftwareInventoryBuilder`.
- Added exact relative-root candidates such as `Vendor\App` from display/install hints, then inspected only existing roots.
- Added nested `User Data` detection and known browser profile folders such as `Default` and `Profile 1`.
- Cache/log child detection is reused across app root, `User Data`, and browser profile roots.
- Cache size is now added only when a cache path is first discovered, avoiding duplicate size counts.
- TDD red observed: browser profile and Electron user-data tests failed because existing code only handled direct AppData roots.
- Verification: focused nested tests passed 2/2; `SoftwareInventoryTests` passed 13/13; `ProductExperienceTests` passed 71/71; full suite passed 132/132; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: `.omx/gui-app-drawer-preview-smoke.ps1` passed with `cachePreviewVisible=True` and `startupPreviewVisible=True`; screenshot `.omx/qa-app-drawer-action-previews.png`.
- Safety state: this remains read-only profile evidence. No cleanup, delete, quarantine move, registry edit, service/startup/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added.

## 2026-07-08 - App drawer action preview state presenter

- Continued reducing WPF code-behind ownership of app-drawer action behavior.
- Added `AppDrawerActionPreviewPresenter` and `AppDrawerActionPreviewState` in `src/Css.Core/Apps/AppDrawerActionPreview.cs`.
- The presenter decides which drawer preview panel is visible, which summary/lines to show, whether the preview is directly executable, and which safety status text to display.
- `PreviewCacheCleanup_Click` and `PreviewStartupControl_Click` now call the presenter and then apply one state object through `ApplyDrawerActionPreviewState`.
- No cleanup, startup disabling, registry, service, scheduled-task, uninstall, migration, installer, or cloud AI execution path was added.
- TDD red observed: `App_drawer_action_preview_presenter_switches_panels_without_execution` failed because `AppDrawerActionPreviewPresenter` did not exist.
- A product static test then failed because it still asserted that code-behind directly contained `CacheCleanupCanExecuteDirectly` / `StartupControlCanExecuteDirectly`; the test was updated to assert presenter integration instead.
- Verification: focused presenter test passed 1/1; `ProductExperienceTests` passed 72/72; full suite passed 133/133; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: `.omx/gui-app-drawer-preview-smoke.ps1` passed with `cachePreviewVisible=True` and `startupPreviewVisible=True`; screenshot `.omx/qa-app-drawer-action-previews.png`.

## 2026-07-08 - App drawer no-selection preview states

- Added no-selection states to `AppDrawerActionPreviewPresenter` for cache cleanup and startup control.
- `PreviewCacheCleanup_Click` and `PreviewStartupControl_Click` now call the presenter even when no app is selected, so the "please choose an app first" guidance is testable and panels are hidden consistently.
- TDD red observed: `App_drawer_action_preview_presenter_handles_no_selection` failed because the no-selection presenter methods did not exist.
- Verification: focused drawer preview presenter tests passed 2/2; `ProductExperienceTests` passed 73/73; full suite passed 134/134; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: no separate GUI smoke for the no-selection branch. The selected-app GUI smoke remains covered by `.omx/gui-app-drawer-preview-smoke.ps1`.
- Safety state: no cleanup, startup disabling, registry edit, service/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added.

## 2026-07-08 - App drawer technical details toggle presenter

- Added `AppDrawerTechnicalDetailsPresenter` and `AppDrawerTechnicalDetailsState`.
- `ToggleTechnicalDetails_Click` now delegates to the presenter and applies visibility, button text, and status text through `ApplyDrawerTechnicalDetailsState`.
- The technical details button now changes from "view technical details" to "hide technical details" after opening, without exposing technical content by default.
- TDD red observed: `App_drawer_technical_details_toggle_is_tested_and_changes_button_text` failed because `AppDrawerTechnicalDetailsPresenter` did not exist.
- Attempted to name the XAML button, but the localized/mojibake XAML line could not be patched reliably; the implementation uses `sender as Button` instead.
- Verification: focused technical-details toggle test passed 1/1; `ProductExperienceTests` passed 74/74; full suite passed 135/135; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: no GUI smoke for clicking the technical-details button.
- Safety state: no cleanup, startup disabling, registry edit, service/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added.

## 2026-07-08 - Shared app drawer action preview host

- Added `AppDrawerActionHostPresenter` and `AppDrawerActionHostViewModel`.
- Added one WPF `DrawerActionPreviewPanel` with title, summary, and wrapping list. Cache, startup, uninstall, migration, and residue-review output now write to this one host.
- Default app selection and clear-drawer states collapse the shared host so the drawer starts with conclusions and buttons instead of stacked action previews.
- Old cache/startup panels and old uninstall/migration preview controls remain in XAML as collapsed compatibility controls, but active click paths no longer write action content into them.
- `PreviewUninstall_Click`, `PreviewMigration_Click`, `PreviewCacheCleanup_Click`, `PreviewStartupControl_Click`, and `ShowResidueReviewInline` now use the shared host.
- TDD red observed: `App_drawer_shared_action_preview_host_replaces_stacked_action_sections` failed because `AppDrawerActionHostPresenter` did not exist.
- Verification: focused shared-host test passed 1/1; `ProductExperienceTests` passed 75/75; full suite passed 136/136; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification was attempted with `.omx/gui-app-drawer-preview-smoke.ps1` but was rejected by the usage-limit approval system; no workaround was attempted.
- Safety state: no cleanup, startup disabling, official uninstaller execution, migration execution, registry edit, service/scheduled-task mutation, installer, or cloud AI path was added.

## 2026-07-08 - Uninstall and migration no-selection host states

- Added `AppDrawerActionHostPresenter.NoSelectionForUninstall()` and `.NoSelectionForMigration()`.
- `PreviewUninstall_Click` and `PreviewMigration_Click` now route no-selection branches through the shared host state model instead of only hardcoded status text.
- TDD red observed: `App_drawer_action_host_handles_uninstall_and_migration_no_selection` failed because the no-selection host methods did not exist.
- A follow-up wiring regression test caught a bad patch that put `NoSelectionForUninstall` into the cache no-selection branch instead of the uninstall branch; it was fixed and covered by `App_drawer_action_host_no_selection_wiring_matches_each_button`.
- Verification: focused no-selection host/wiring tests passed 2/2; `ProductExperienceTests` passed 77/77; full suite passed 138/138; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Safety state: no cleanup, startup disabling, official uninstaller execution, migration execution, registry edit, service/scheduled-task mutation, installer, or cloud AI path was added.

## 2026-07-08 - App drawer legacy preview cleanup

- Removed the old collapsed app-drawer preview controls: cache preview panel, startup preview panel, uninstall preview title/list, and migration preview summary/list.
- Removed code-behind writes to those old controls; uninstall, migration, cache, startup, and residue review now use only `DrawerActionPreviewPanel` through `ApplyDrawerActionHost(...)`.
- Removed the overwritten uninstall no-selection `StatusTextBlock.Text` assignment so the no-selection message comes from `AppDrawerActionHostPresenter.NoSelectionForUninstall()`.
- TDD red observed: `App_drawer_uses_only_one_shared_action_preview_host` failed because `DrawerCachePreviewPanel` and other legacy controls still existed; `App_drawer_no_selection_status_comes_from_action_host_presenter` failed because the uninstall no-selection branch still wrote status directly.
- Verification: focused drawer shared-host cleanup tests passed 5/5; `ProductExperienceTests` passed 78/78; full suite passed 139/139; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Safety state: XAML/code-behind cleanup only. No cleanup, startup disabling, official uninstaller execution, migration execution, registry edit, service/scheduled-task mutation, installer, file move, or cloud AI path was added.

## 2026-07-08 - C-drive legacy recommendation selection cleanup

- Removed the unused `RecommendationsListBox_SelectionChangedLegacy` handler.
- The active C-drive recommendation selection handler still uses `RecommendationSelectionPresenter.Create(...)` to decide button enabled state, button text, and beginner explanation.
- TDD red observed: `C_drive_recommendation_selection_handler_uses_selection_presenter` failed because the legacy handler still appeared after the active handler.
- Verification: focused C-drive selection tests passed 3/3; `ProductExperienceTests` passed 78/78; full suite passed 139/139; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Safety state: no recommendation execution semantics changed. Low-risk cleanup still requires the existing confirmation, quarantine, and safety pipeline.

## 2026-07-08 - Agent skill capability cards

- Added `AgentSkillCardPresenter` and `AgentSkillCardViewModel` in `src/Css.Core/Agent/AgentSkillCardPresentation.cs`.
- The Agent skill catalog now presents clean capability cards with title, description, safety mode, risk label, next-step label, and safety hint.
- `MainWindow` now loads skills through `AgentSkillCardPresenter.CreateDefault()` and the Agent skill list binds `NextStepLabel` and `SafetyHint`.
- High-risk process/service and session-control capabilities remain plan-only with explicit "will not directly end processes/disable services/lock/shutdown/restart" safety copy; system tools are labeled open-only.
- TDD red observed: `Agent_skill_cards_show_next_step_and_safety_mode_for_beginner_users` failed because `AgentSkillCardPresenter` did not exist.
- Verification: focused Agent skill-card test passed 1/1; focused Agent tests passed 4/4; `ProductExperienceTests` passed 79/79; full suite passed 140/140; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Safety state: presentation-only. No system settings, process/service, session-control, installer, file move, cleanup, registry, cloud AI, or system-tool execution path was added.

## 2026-07-08 - Agent system tool shortcuts

- Added `SystemToolShortcut` and `SystemToolShortcutCatalog` in `src/Css.Core/Agent/SystemToolShortcuts.cs`.
- The catalog exposes a fixed allowlist for Task Manager, Device Manager, Disk Management, Event Viewer, Windows Security, and Registry Editor.
- The Agent page now shows a `AgentSystemToolListBox` section under the skill catalog. Each item has name, explanation, safety hint, and an explicit open button.
- `OpenSystemTool_Click` only looks up catalog ids, blocks unknown ids, requires confirmation for medium/high-risk tools, and uses `ProcessStartInfo { UseShellExecute = true }` to open the selected Windows tool.
- TDD red observed: `Agent_system_tool_shortcuts_are_allowlisted_open_only_and_confirm_risky_tools` failed because `SystemToolShortcutCatalog` did not exist.
- Verification: focused shortcut tests passed 2/2; focused Agent tests passed 5/5; `ProductExperienceTests` passed 81/81; full suite passed 142/142; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentSystemToolListFound=true`, `visibleOpenButtonCount=4`; screenshot `.omx/qa-agent-system-tools.png`.
- Safety state: no system tool was clicked during verification. Product code opens only allowlisted Windows tools after explicit user action; it does not click inside those tools or modify system settings.

## 2026-07-08 - Agent Windows settings shortcuts

- Added `WindowsSettingsShortcut` and `WindowsSettingsShortcutCatalog` in `src/Css.Core/Agent/WindowsSettingsShortcuts.cs`.
- The catalog exposes fixed `ms-settings:` links for Network/Wi-Fi, Bluetooth/devices, Sound, Display, Power/Sleep, Storage, and Installed Apps.
- The Agent page now shows `AgentWindowsSettingsListBox` under the system-tool list. Each item has name, explanation, safety hint, and an explicit open button.
- `OpenWindowsSettings_Click` blocks unknown ids, rejects non-`ms-settings:` links, and opens the setting page with `ProcessStartInfo { UseShellExecute = true }`.
- TDD red observed: `Agent_windows_settings_shortcuts_are_ms_settings_allowlisted_and_open_only` failed because `WindowsSettingsShortcutCatalog` did not exist.
- Verification: focused settings tests passed 2/2; focused Agent/system/settings tests passed 5/5; `ProductExperienceTests` passed 83/83; full suite passed 144/144; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: updated `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentSystemToolListFound=true`, `visibleOpenButtonCount=3`, `agentWindowsSettingsListFound=true`, `visibleSettingsOpenButtonCount=3`; screenshot `.omx/qa-agent-system-and-settings.png`.
- Safety state: no settings button was clicked during verification. Product code opens only allowlisted Windows Settings pages after explicit user action; it does not toggle options, uninstall apps, delete files, or modify system settings.

## 2026-07-08 - Windows settings confirmation gate

- Added `RequiresConfirmation` to `WindowsSettingsShortcut`.
- Low-risk settings such as Network/Wi-Fi, Bluetooth/devices, Sound, and Display remain direct open-only links.
- Medium-risk settings such as Power/Sleep, Storage, and Installed Apps now require a confirmation dialog before opening.
- `OpenWindowsSettings_Click` checks `shortcut.RequiresConfirmation`; if the user cancels, it updates status text and does not launch the `ms-settings:` URI.
- The visible settings shortcut name now adds a small "needs confirmation" marker for medium-risk entries.
- TDD red observed: focused settings tests failed to compile because `WindowsSettingsShortcut.RequiresConfirmation` did not exist.
- Verification: focused settings tests passed 2/2; `ProductExperienceTests` passed 83/83; full suite passed 144/144; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentSystemToolListFound=true`, `visibleOpenButtonCount=3`, `agentWindowsSettingsListFound=true`, `visibleSettingsOpenButtonCount=3`; screenshot `.omx/qa-agent-system-and-settings.png`.
- Safety state: no settings button was clicked during verification. No settings toggles, uninstall, cleanup, registry edit, service/startup/scheduled-task mutation, installer, file move, or cloud AI path was added.

## 2026-07-08 - Agent background priority

- Updated `AgentNextStepPresenter` so many resident/background apps can outrank C-drive app advice when there is no low-risk C-drive cleanup item waiting.
- Added a `ResidentPriorityThreshold` of 3 resident apps. Resident signals include the existing `AppPresentationBuilder.IsResident` evidence from running processes, startup entries, services, or scheduled tasks.
- When the threshold is met, the Agent next-step title and first safe action emphasize checking background/resident apps first.
- C-drive app advice still remains in the safe-action list when relevant, but it is not the first recommendation in this scenario.
- Navigation actions remain internal page navigation to Apps; they do not terminate processes or disable anything.
- TDD red observed: `Agent_next_step_prioritizes_many_resident_apps_before_c_drive_apps` failed because the title still said "C drive apps".
- Verification: focused Agent priority test passed 1/1; focused Agent next-step tests passed 4/4; `ProductExperienceTests` passed 84/84; full suite passed 145/145; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Safety state: no process termination, service disable, startup/scheduled-task mutation, cleanup, uninstall, migration, registry edit, installer, file move, or cloud AI path was added.

## 2026-07-08 - Agent background review panel

- Added `AgentBackgroundReviewPresenter`, `AgentBackgroundReviewViewModel`, and `AgentBackgroundReviewItemViewModel` in `src/Css.Core/Agent/AgentBackgroundReviewPresentation.cs`.
- The presenter summarizes resident apps into beginner-readable items: app name, evidence summary, risk label, recommended next step, and `CanExecuteDirectly=false`.
- It hides technical identifiers such as service names and scheduled-task paths from the first-level summary.
- `MainWindow.LoadAgentNextSteps()` now refreshes `AgentBackgroundReviewPanel` from `_softwareProfiles` after app scans.
- Added WPF controls: `AgentBackgroundReviewPanel`, `AgentBackgroundReviewSummaryTextBlock`, `AgentBackgroundReviewItemsListBox`, and `AgentBackgroundReviewSafetyTextBlock` with explicit AutomationIds for GUI smoke reliability.
- The panel was moved above the Agent reasons list after screenshot review showed the initial bottom placement was outside the first visible area.
- Added `.omx/gui-agent-background-review-smoke.ps1`, which launches the app, runs a read-only app scan, navigates to Agent, verifies the background summary/list, captures `.omx/qa-agent-background-review.png`, and closes only the launched app process.
- TDD red observed: focused background review tests failed to compile because `AgentBackgroundReviewPresenter` did not exist; the WPF binding test then failed because explicit AutomationIds were missing.
- GUI issues found and fixed: the first smoke failed because `Wait-Until` was defined after use; another smoke showed the panel was present but too low to be useful, so placement was fixed and rerun.
- Verification: focused background review tests passed 2/2; focused Agent/background tests passed 5/5; `ProductExperienceTests` passed 86/86; full suite passed 147/147; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: `.omx/gui-agent-background-review-smoke.ps1` passed with `appTileCount=120`, `backgroundSummaryFound=true`, `backgroundReviewItemCount=3`; screenshot `.omx/qa-agent-background-review.png`.
- Safety state: no process termination, service disable, startup/scheduled-task mutation, cleanup, uninstall, migration, registry edit, installer, file move, session control, or cloud AI path was added.

## 2026-07-08 - Agent startup/service plan preview

- Added `AgentStartupServicePlanPresenter` and `AgentStartupServicePlanViewModel` in `src/Css.Core/Agent/AgentStartupServicePlanPresentation.cs`.
- The presenter converts resident app evidence into an auditable plan-only review: summary, evidence counts, review steps, required snapshot/confirmation/rollback evidence, blocked actions, and `CanExecuteDirectly=false`.
- The Agent page now binds `AgentStartupServicePlanPanel`, title, summary, steps list, and safety line from `LoadAgentNextSteps()` after app scans.
- Added explicit AutomationIds for the plan title, summary, steps, and safety text so GUI smoke can verify the real visible controls.
- Moved the plan preview above the detailed background app list after screenshot review showed the plan title/summary had fallen near the bottom of the Agent card.
- Extended `.omx/gui-agent-background-review-smoke.ps1` to verify the plan preview after a real app scan and capture `.omx/qa-agent-startup-service-plan.png`.
- TDD red observed: the binding test first failed because plan title/summary/safety lacked AutomationIds; then failed because `AgentStartupServicePlanPanel` appeared after `AgentBackgroundReviewItemsListBox`.
- GUI issue found and fixed: the smoke script initially failed because a raw Chinese `只生成方案` literal did not match under Windows PowerShell script encoding; the script now constructs the phrase from Unicode code points.
- Verification: focused plan/binding tests passed 3/3; full suite passed 148/148; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: `.omx/gui-agent-background-review-smoke.ps1` passed with `appTileCount=120`, `backgroundSummaryFound=true`, `backgroundReviewItemCount=3`, `startupServicePlanFound=true`, `startupServicePlanStepCount=3`; screenshot `.omx/qa-agent-startup-service-plan.png`.
- Safety state: plan-only and display-only. No startup disabling, service/scheduled-task mutation, process termination, cleanup, uninstall, migration, registry edit, installer, file move, session control, or cloud AI path was added.

## 2026-07-08 - Windows Settings confirmation cancel GUI smoke

- Added dynamic AutomationIds to Windows Settings open buttons: `AgentWindowsSettingsOpenButton_<id>`.
- Reordered `WindowsSettingsShortcutCatalog` so Storage, Installed Apps, and Power/Sleep appear first; these are medium-risk and confirmation-gated.
- Moved the Windows Settings direct section above system tools and skill catalog in the Agent right card so storage/app-management entry points are visible without digging.
- Added `AgentCapabilityScrollViewer` around the Agent right-card capability column to prevent capability content from falling out of the visible area.
- Added `.omx/gui-agent-settings-confirm-cancel-smoke.ps1`, which launches `Css.App.exe`, opens AI Agent, clicks the visible Storage settings button, captures the confirmation dialog, cancels it, and verifies no new `SystemSettings` process exists.
- TDD red observed: settings binding test failed for missing dynamic button AutomationId; then failed because `AgentCapabilityScrollViewer` did not exist; settings catalog test failed on old low-risk-first order; settings binding test failed because Settings appeared after system tools.
- GUI issues found and fixed: the first dialog search only checked root children and missed the MessageBox; the script now searches descendant windows for the same process. Cancel lookup also needed a rightmost-button fallback because localized button names were not exposed as expected.
- Verification: focused settings tests passed 2/2; full suite passed 148/148; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` passed with `confirmationDialogFound=true`, `cancelClicked=true`, `newSettingsProcessCount=0`, screenshot `.omx/qa-agent-settings-confirm-cancel.png`; `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentSystemToolListFound=true`, `agentWindowsSettingsListFound=true`, and screenshot `.omx/qa-agent-system-and-settings.png`.
- Safety state: open-only and cancel-verified. No setting was opened in the cancel smoke, and no settings toggles, uninstall, cleanup, registry edit, service/startup/scheduled-task mutation, installer, file move, session control, or cloud AI path was added.

## 2026-07-08 - App drawer shared action host four-button GUI smoke

- Added stable AutomationIds to the app drawer's four primary action buttons and preview title/summary/list controls.
- Extended `.omx/gui-app-drawer-preview-smoke.ps1` from cache/startup-only proof to a four-action matrix: uninstall plan, migration plan, cache cleanup preview, and startup-control preview.
- The smoke now uses AutomationIds for drawer actions, picks an eligible scanned app per action, closes preview-only uninstall/migration plan windows, verifies the shared preview title/list, captures `.omx/qa-app-drawer-action-previews.png`, and closes only the launched app process.
- TDD red observed: `App_drawer_action_controls_have_stable_automation_ids_for_gui_smoke` failed because the drawer buttons lacked AutomationIds.
- GUI issues found and fixed: `DrawerActionPreviewPanel` was a `Border` and not reliably discoverable through UIAutomation, so the smoke verifies the exposed title/list controls instead; migration can be disabled for D-drive apps, so the smoke selects an eligible app per action.
- Verification: focused AutomationId test passed 1/1; `ProductExperienceTests` passed 88/88; full suite passed 149/149; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification: `.omx/gui-app-drawer-preview-smoke.ps1` passed with `verifiedActionButtons=4`, `verifiedActionButtonIds=DrawerUninstallButton,DrawerMigrateButton,DrawerCleanCacheButton,DrawerDisableStartupButton`, `closedDialogCount=2`; screenshot `.omx/qa-app-drawer-action-previews.png`.
- Safety state: preview-only. No cleanup, startup disabling, official uninstaller execution, migration execution, rollback manifest creation, registry edit, service/scheduled-task mutation, installer, file move, session control, settings change, or cloud AI path was added.

## 2026-07-08 - App drawer Agent action card fields

- Added `AgentTakeaway`, `NextStepText`, and `SafetyText` to `AppDrawerActionHostViewModel`.
- Populated the new fields for uninstall, migration, cache cleanup, startup control, and inline post-uninstall residue review.
- Updated WPF app drawer preview host with `DrawerActionPreviewAgentTextBlock`, `DrawerActionPreviewNextStepTextBlock`, and `DrawerActionPreviewSafetyTextBlock`.
- Added `AppDrawerScrollViewer` and `DrawerActionPreviewPanel.BringIntoView()` so clicking an action scrolls the right drawer to the Agent action card instead of leaving the useful text below the visible area.
- Enhanced `.omx/gui-app-drawer-preview-smoke.ps1` to verify the Agent/next-step/safety fields exist, are non-empty, and are visible for each action.
- TDD red observed: `App_drawer_action_host_presents_agent_takeaway_next_step_and_safety_text` failed because the new fields did not exist; `App_drawer_action_preview_scrolls_into_view_after_action_clicks` failed because the drawer lacked a scroll viewer and bring-into-view call.
- Build issue found and fixed: `ShowResidueReviewInline(...)` directly constructed `AppDrawerActionHostViewModel` and needed the new required fields.
- Verification: focused action-card/scroll tests passed 3/3; enhanced app-drawer GUI smoke passed with `verifiedActionButtons=4` and `closedDialogCount=2`; `ProductExperienceTests` passed 91/91; full suite passed 152/152; solution build passed with 0 warnings and 0 errors.
- Safety state: presentation-only. No cleanup, startup disabling, official uninstaller execution, migration execution, rollback manifest creation, registry edit, service/scheduled-task mutation, installer, file move, settings change, session control, or cloud AI path was added.

## 2026-07-08 - Selected Resident App Plan Details Started

- Objective: improve the app drawer startup/background action so selected resident apps get an Agent plan that says keep, observe, or candidate-for-future-disable in user-facing language.
- Scope: presentation-only core model and tests first; no startup/service/task/process mutation or execution handler.
- Plan: add failing ProductExperienceTests, implement the minimal presenter behavior, then run focused/full verification.

## 2026-07-08 - Selected Resident App Plan Details Verified

- Added focused tests for `建议保留`, `先观察`, and `未来可禁用候选` startup/background classifications; all three were observed red before implementation.
- Updated `AppStartupControlPreviewPresenter` and `AppDrawerActionHostPresenter` so the drawer startup action card explains keep/observe/future-disable decisions without raw service/task/process names.
- Verification: focused new tests passed 3/3; surrounding startup/action-host tests passed 4/4; `ProductExperienceTests` passed 94/94; full suite passed 155/155; solution build passed with 0 warnings/errors; app-drawer GUI smoke passed with `verifiedActionButtons=4`, `closedDialogCount=2`.

## 2026-07-08 - Undo Center Visual Proof Started

- Objective: add discoverable proof hooks and/or GUI smoke for undo-center timeline/quarantine/restore affordance before broadening any cleanup execution.
- Scope: proof and presentation only; no destructive cleanup, overwrite, registry/service/startup/task mutation, or cloud AI.
- Plan: inspect current undo-center UI, add failing tests for stable AutomationIds and safety copy, then minimally patch UI/smoke coverage.

## 2026-07-08 - Undo Center Visual Proof Static Verification

- Added `Undo_center_has_stable_visual_proof_hooks_for_timeline_quarantine_and_restore`; it first failed because Timeline controls had no stable AutomationIds.
- Rewrote the TimelinePage XAML block with XML character references after historical mojibake had swallowed attributes into visible text; added AutomationIds for title, load button, description, quarantine policy, list, restore line, and restore button.
- Added `.omx/gui-undo-center-smoke.ps1`, which opens the WPF app, navigates to the undo center, verifies timeline/quarantine/restore controls, and screenshots without moving or restoring files.
- Verification: focused undo hook test passed 1/1; `ProductExperienceTests` passed 95/95; full suite passed 156/156; solution build passed with 0 warnings/errors.
- GUI verification: `.omx/gui-undo-center-smoke.ps1` later passed with `timelineTitleFound=true`, `quarantinePolicyFound=true`, `timelineListFound=true`, `restoreButtonFound=true`, `restoreButtonEnabled=false`; screenshot `.omx/qa-undo-center.png`.

## 2026-07-09 - Isolated App Storage Roots for GUI Smokes

- Objective: prevent GUI smokes from touching the user's real LocalAppData timeline or D-drive quarantine root.
- TDD: added `App_storage_paths_can_be_isolated_for_gui_smokes_without_touching_user_data` and `App_storage_paths_keep_existing_defaults_when_no_override_is_set`; both first failed because `AppStoragePathResolver` did not exist.
- Implemented `AppStoragePathResolver` with `OMNIX_ENTROPY_DATA_ROOT` and `OMNIX_ENTROPY_QUARANTINE_ROOT`, plus `AppStoragePaths`.
- Updated `MainWindow` default database, migration rollback, and quarantine paths to use the resolver while preserving normal defaults.
- TDD: added `Undo_center_gui_smoke_uses_isolated_storage_overrides`; it first failed because `.omx/gui-undo-center-smoke.ps1` did not set isolated env vars.
- Updated the undo-center GUI smoke to create `.omx/qa-undo-center-data` and `.omx/qa-undo-center-quarantine`, set the env vars before launching `Css.App.exe`, then restore prior env values and remove both directories in `finally`.
- Verification: focused path/script tests passed 3/3; `ProductExperienceTests` passed 96/96; full suite passed 159/159; solution build passed with 0 warnings/errors; isolated undo-center GUI smoke passed and cleanup checks returned `False` for both temporary directories.

## 2026-07-09 - Seeded Undo-Center Restorable GUI Proof Started

- Objective: seed one restorable undo/quarantine record under isolated GUI-smoke roots and prove the restore button becomes enabled without clicking it.
- Scope: smoke/test tooling only unless UI discovery exposes a product bug; no real cleanup, restore, or user-data writes.
- Plan: add a failing ProductExperienceTests assertion for seeding/no-restore-click, implement the smallest safe seed path, then run focused tests, build, and the GUI smoke.

## 2026-07-09 - Seeded Undo-Center Restorable GUI Proof Verified

- TDD: added `Undo_center_gui_smoke_seeds_restorable_data_without_invoking_restore`; it first failed because `.omx/gui-undo-center-smoke.ps1` did not seed a restorable record or require the restore button to be enabled.
- Added `src/Css.SmokeTools`, a dev/test console tool with `seed-undo-center`. It uses the same `AppStoragePathResolver`, `FileQuarantineService`, `ActionTimelineStore`, and `SafetyOperationPipeline` to seed a restorable quarantine/timeline record under the process-scoped isolated roots.
- Extended `.omx/gui-undo-center-smoke.ps1` to run the seed tool before launching WPF, wait for an enabled `TimelineRestoreButton`, report `restoreButtonEnabled=true`, and still never invoke restore.
- Visual review showed the seeded timeline row exposed a long local path. Added `Timeline_presentation_summarizes_affected_paths_for_beginner_view`; it first failed on the raw path, then passed after `ActionTimelinePresenter` changed first-level detail to `影响范围：N 个位置`.
- Verification: focused undo smoke tests passed 3/3; focused timeline presentation tests passed 2/2; `ProductExperienceTests` passed 97/97; full suite passed 161/161; solution build passed with 0 warnings/errors; seeded undo GUI smoke passed and `.omx/qa-undo-center.png` shows an enabled restore button without raw paths.
- Safety state: the smoke used isolated `.omx` roots only, did not click restore, and cleanup checks returned `False` for both temporary roots.

## 2026-07-09 - Shared WPF Smoke Helper Foundation Started

- Objective: create a shared `.omx` PowerShell helper for common WPF smoke operations and make the undo-center smoke the first consumer.
- Scope: smoke tooling only; no product behavior, cleanup, restore, migration, uninstall, startup, service, task, registry, settings, or AI execution changes.
- Plan: add a failing static test for helper usage, move repeated UIAutomation functions into the helper, rerun the seeded undo GUI smoke.

## 2026-07-09 - Shared WPF Smoke Helper Foundation Verified

- TDD: added `Undo_center_gui_smoke_uses_shared_wpf_smoke_helpers`; it first failed because `.omx/wpf-smoke-helpers.ps1` did not exist.
- Added `.omx/wpf-smoke-helpers.ps1` with `Initialize-WpfSmokeAutomation`, `Find-ByAutomationId`, `Wait-Until`, `Invoke-Element`, and `Save-WindowScreenshot`.
- Updated `.omx/gui-undo-center-smoke.ps1` to dot-source the helper and remove local copies of the common functions. `Seed-RestorableUndoRecord` stays local because it is undo-center-specific.
- Verification: focused shared-helper test passed 1/1; focused undo smoke tests passed 4/4; seeded undo GUI smoke passed with `restoreButtonEnabled=true`; temp root cleanup checks returned `False`; full suite passed 162/162; solution build passed with 0 warnings/errors.

## 2026-07-09 - App Drawer Smoke Helper Migration Started

- Objective: migrate `.omx/gui-app-drawer-preview-smoke.ps1` to shared WPF smoke helpers without changing what it clicks or executes.
- Scope: smoke tooling only; the smoke must remain preview-only and no product execution path is added.
- TDD: added `App_drawer_gui_smoke_uses_shared_wpf_smoke_helpers`; it first failed because the app-drawer smoke did not reference `.omx/wpf-smoke-helpers.ps1`.

## 2026-07-09 - App Drawer Smoke Helper Migration Verified

- Added `Save-DesktopScreenshot` to `.omx/wpf-smoke-helpers.ps1`.
- Updated `.omx/gui-app-drawer-preview-smoke.ps1` to dot-source the helper and use shared `Initialize-WpfSmokeAutomation`, `Find-ByAutomationId`, `Invoke-Element`, and `Save-DesktopScreenshot`.
- Kept app-drawer-specific helpers local: Unicode text construction, name-part lookup, list-item selection, and preview-window closing.
- Verification: focused app-drawer helper/action-host tests passed 4/4; real app-drawer GUI smoke passed with `verifiedActionButtons=4`, `verifiedActionButtonIds=DrawerUninstallButton,DrawerMigrateButton,DrawerCleanCacheButton,DrawerDisableStartupButton`, `closedDialogCount=2`; screenshot `.omx/qa-app-drawer-action-previews.png`; no `Css.App` process remained.

## 2026-07-09 - GUI Smoke Development Documentation Verified

- TDD: added `Development_docs_describe_storage_overrides_as_test_only`; it first failed because no `docs/development/gui-smokes.md` file existed.
- Added `docs/development/gui-smokes.md` documenting `OMNIX_ENTROPY_DATA_ROOT`, `OMNIX_ENTROPY_QUARANTINE_ROOT`, shared smoke helpers, and `Css.SmokeTools seed-undo-center` as development/test-only tooling.
- Verification: focused docs test passed 1/1; `ProductExperienceTests` passed 100/100; full suite passed 164/164; solution build passed with 0 warnings/errors.

## 2026-07-09 - Agent System Tools Smoke Helper Migration Verified

- TDD: added `Agent_system_tools_gui_smoke_uses_shared_wpf_smoke_helpers`; it first failed because `.omx/gui-agent-system-tools-smoke.ps1` still owned its own UIAutomation setup and did not reference `.omx/wpf-smoke-helpers.ps1`.
- Updated `.omx/gui-agent-system-tools-smoke.ps1` to dot-source `.omx/wpf-smoke-helpers.ps1`, call `Initialize-WpfSmokeAutomation`, use shared `Wait-Until`, `Find-ByAutomationId`, `Invoke-Element`, and `Save-WindowScreenshot`.
- Kept the smoke's product-specific scope local: navigate to AI Agent, verify `AgentSystemToolListBox` and `AgentWindowsSettingsListBox`, count visible open buttons, and capture `.omx/qa-agent-system-and-settings.png`.
- Safety: the smoke still does not click any system-tool or Windows Settings open button.
- Verification: focused helper test passed 1/1; real GUI smoke passed with `agentSystemToolListFound=true`, `visibleOpenButtonCount=3`, `agentWindowsSettingsListFound=true`, and `visibleSettingsOpenButtonCount=3`; `ProductExperienceTests` passed 101/101; full suite passed 165/165; solution build passed with 0 warnings/errors; no `Css.App`/`Css.SmokeTools` process remained.

## 2026-07-09 - Agent Settings Confirm-Cancel Smoke Helper Migration Verified

- TDD: added `Agent_settings_confirm_cancel_gui_smoke_uses_shared_wpf_smoke_helpers`; it first failed because `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` owned local UIAutomation setup and helper functions.
- Updated `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` to dot-source `.omx/wpf-smoke-helpers.ps1` and use shared `Initialize-WpfSmokeAutomation`, `Find-ByAutomationId`, `Wait-Until`, `Invoke-Element`, and `Save-WindowScreenshot`.
- Kept settings-specific behavior local: clickable-point probing, mouse click, confirmation-window discovery, cancel-button fallback, and `SystemSettings` process checks.
- Debugging: the migrated GUI smoke initially failed with `RPC_E_SERVERFAULT` in root-descendant UIAutomation search, then with dialog-not-found after catching that error. Added native Win32 `EnumWindows`/`GetWindowThreadProcessId` fallback and protected root-window search.
- Safety: the real smoke clicks Storage only to open OMNIX-Entropy's confirmation dialog, cancels it, and verifies `newSettingsProcessCount=0`.
- Verification: focused settings smoke helper/native-window tests passed 3/3; real GUI smoke passed with `confirmationDialogFound=true`, `cancelClicked=true`, `newSettingsProcessCount=0`, screenshot `.omx/qa-agent-settings-confirm-cancel.png`; `ProductExperienceTests` passed 104/104; full suite passed 168/168; solution build passed with 0 warnings/errors.

## 2026-07-09 - Agent Background Review Smoke Helper Migration Verified

- TDD: added `Agent_background_review_gui_smoke_uses_shared_wpf_smoke_helpers`; it first failed because `.omx/gui-agent-background-review-smoke.ps1` owned local UIAutomation setup, wait, invoke, and screenshot helpers.
- Updated `.omx/gui-agent-background-review-smoke.ps1` to dot-source `.omx/wpf-smoke-helpers.ps1` and use shared `Initialize-WpfSmokeAutomation`, `Find-ByAutomationId`, `Wait-Until`, `Invoke-Element`, and `Save-WindowScreenshot`.
- Kept background-review-specific behavior local: read-only app scan, app tile wait, Agent navigation, background summary assertions, startup/service plan assertions, and plan-only phrase check.
- Safety: the smoke remains read-only/plan-only. It does not disable startup entries, stop services/processes, edit tasks or registry, uninstall, migrate, open settings, run installers, or call cloud AI.
- Verification: focused helper test passed 1/1; real GUI smoke passed with `appTileCount=120`, `backgroundSummaryFound=true`, `backgroundReviewItemCount=3`, `startupServicePlanFound=true`, `startupServicePlanStepCount=3`, screenshot `.omx/qa-agent-startup-service-plan.png`; `ProductExperienceTests` passed 105/105; full suite passed 169/169; solution build passed with 0 warnings/errors.

## 2026-07-09 - Undo Center Collapsed Technical Details Verified

- Objective: let users keep the undo-center timeline readable while allowing exact affected paths and manifest paths to be inspected on demand.
- TDD: added `Timeline_presentation_keeps_raw_paths_in_collapsed_technical_details`; it first failed because `ActionTimelineItemViewModel` lacked `TechnicalDetailsButtonText` and `TechnicalDetails`.
- TDD: extended product tests for `TimelineTechnicalDetailsExpander`, `TimelineTechnicalDetailsListBox`, and smoke-script `technicalDetailsExpanderFound`; the static checks first failed because the XAML and smoke did not expose the expander.
- Implemented collapsed timeline technical details in `ActionTimelinePresentation.cs`; first-level `Detail` remains path-free, while `TechnicalDetails` stores record id, source, restore state, restore operation, affected paths, and manifest paths.
- Updated `MainWindow.xaml` so each undo timeline row has a collapsed `TimelineTechnicalDetailsExpander` and nested `TimelineTechnicalDetailsListBox`.
- Updated `.omx/gui-undo-center-smoke.ps1` so the seeded isolated GUI smoke verifies the expander exists without expanding it or invoking restore.
- Verification: focused timeline/product tests passed 3/3; seeded undo GUI smoke passed with `restoreButtonEnabled=true` and `technicalDetailsExpanderFound=true`; `ProductExperienceTests` passed 105/105; full suite passed 170/170; solution build passed with 0 warnings/errors.
- Safety: no cleanup, restore click, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.

## 2026-07-09 - Low-Risk Cleanup Preview Slice Started

- Objective: make low-risk cleanup suggestions explain the quarantine-first path in beginner language before any user confirmation or operation pipeline work.
- Scope: presentation/tests first; preserve existing safety pipeline and do not add direct delete or automatic cleanup.
- Plan: inspect current recommendation/cleanup/quarantine seams, write a failing test for the preview model, implement the smallest presenter or model change, then run focused and full verification.

## 2026-07-09 - Low-Risk Cleanup Selection Preview Verified

- TDD: added `C_drive_low_risk_cleanup_selection_preview_is_structured_and_quarantine_first`; it first failed because `RecommendationSelectionViewModel` did not expose `CanExecuteDirectly`, `AgentTakeaway`, `NextStepText`, `SafetyBoundary`, or `PlanLines`.
- TDD: added `C_drive_cleanup_selection_preview_has_stable_beginner_fields`; it first failed because the WPF C-drive recommendation area had no stable preview AutomationIds or code-behind assignments for the structured fields.
- Implemented structured selection preview fields in `RecommendationSelectionPresenter`. Actionable low-risk cleanup still sets `CanContinue=true` but `CanExecuteDirectly=false`, because the button only opens second confirmation before the existing safety pipeline.
- Updated `MainWindow.xaml.cs` to apply the structured preview fields on selection and reset them when scanning starts or scan results load.
- Updated `MainWindow.xaml` with the C-drive recommendation preview panel: `RecommendationActionTakeawayTextBlock`, `RecommendationActionNextStepTextBlock`, `RecommendationActionSafetyTextBlock`, and `RecommendationActionPlanListBox`.
- Verification: focused new tests passed 2/2; surrounding C-drive recommendation tests passed 8/8; `ProductExperienceTests` passed 107/107; full suite passed 172/172; solution build passed with 0 warnings/errors.
- Safety: no direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.

## 2026-07-09 - Low-Risk Cleanup Confirmation Copy Slice Started

- Objective: move the final cleanup confirmation from a path-first message toward a beginner summary plus technical details section.
- Scope: presenter and WPF handler integration only; no policy or execution permission change.
- Plan: write a failing product test for the confirmation model and handler usage, implement a small presenter, then run focused/full verification.

## 2026-07-09 - Low-Risk Cleanup Confirmation Copy Verified

- TDD: added `C_drive_cleanup_confirmation_puts_plain_summary_before_technical_paths`; it first failed because `CleanupConfirmationPresenter` did not exist.
- TDD: added `C_drive_cleanup_execution_confirmation_uses_confirmation_presenter`; it protected the WPF handler from continuing to build a path-first `MessageBox` inline.
- Added `CleanupConfirmationPresenter` / `CleanupConfirmationViewModel` in `Css.Core.Apps`.
- Replaced the inline cleanup confirmation message in `ExecuteSelectedRecommendationAsync` with `CleanupConfirmationPresenter.Create(...)`; the handler still calls `QuarantineOperationPolicy`, `SafetyOperationPipeline`, and `QuarantineOperationHandler` as before.
- The confirmation text now starts with Agent judgment, affected-count, estimated impact, quarantine-first behavior, Undo Center restore language, and the local safety-pipeline boundary. Technical details retain raw paths, evidence, operation kind, original confirmation text, and quarantine root.
- Verification: focused confirmation tests passed 2/2; surrounding C-drive tests passed 9/9; `ProductExperienceTests` passed 109/109; full suite passed 174/174; solution build passed with 0 warnings/errors.
- Safety: no direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.

## 2026-07-09 - Custom Cleanup Confirmation Dialog Slice Started

- Objective: replace the cleanup `MessageBox` with a WPF dialog that shows the presenter summary and collapses technical paths by default.
- Scope: UI/handler integration only; preserve the same `QuarantineOperationPolicy -> user OK -> SafetyOperationPipeline -> QuarantineOperationHandler` flow.
- Plan: inspect existing windows, write failing static/product tests, implement the smallest dialog, then run focused/full verification.

## 2026-07-09 - Custom Cleanup Confirmation Dialog Verified

- TDD: updated `C_drive_cleanup_execution_confirmation_uses_confirmation_presenter`; it first failed because `ExecuteSelectedRecommendationAsync` still used `MessageBox.Show`.
- TDD: added `C_drive_cleanup_confirmation_window_has_collapsed_technical_details_and_stable_hooks`; it first failed because `CleanupConfirmationWindow.xaml` did not exist.
- Added `CleanupConfirmationWindow.xaml` / `.xaml.cs`; summary is visible first, technical details are in an `Expander` with `IsExpanded=false`, and confirm/cancel buttons set `DialogResult=true/false`.
- Replaced the cleanup `MessageBox` call with `new CleanupConfirmationWindow(confirmation) { Owner = this }` and `ShowDialog() != true` cancellation handling.
- Verification: focused window/handler tests passed 2/2; surrounding C-drive tests passed 10/10; `ProductExperienceTests` passed 110/110; full suite passed 175/175; solution build passed with 0 warnings/errors.
- Safety: no execution policy changed. Confirm still gates the same quarantine safety pipeline; no direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.

## 2026-07-09 - C-drive Cleanup Fixture Smoke Script Added

- Objective: make the low-risk C-drive cleanup preview and custom confirmation dialog GUI-verifiable without scanning or touching the user's real C drive.
- TDD: added `C_drive_scan_root_override_is_process_scoped_for_gui_smoke_fixtures`; it first failed because `AppDevelopmentPathResolver` did not exist.
- TDD: added `C_drive_cleanup_preview_and_execute_controls_have_stable_automation_ids` and `C_drive_cleanup_gui_smoke_uses_isolated_scan_fixture_and_cancels_confirmation`; they first failed because C-drive controls lacked explicit AutomationIds and `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1` did not exist.
- TDD: added `App_rules_classify_top_level_temp_roots_for_cleanup_fixture`; it first failed because `C:\Temp` classified as `Other`.
- Added `AppDevelopmentPathResolver.ResolveCDriveScanRoot(...)` with `OMNIX_ENTROPY_CDRIVE_SCAN_ROOT`; normal app runs keep the selected system drive root.
- `RunScanAsync` now uses the scan-root override only for the crawl/snapshot key when the environment variable is present.
- Added explicit AutomationIds for `CDriveNavButton`, `StartScanButton`, `RecommendationsListBox`, and `ExecuteRecommendationButton`.
- Added `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1`: it creates isolated `.omx` data/quarantine/scan roots, seeds a tiny `Temp` fixture, scans it, selects an actionable cleanup card, verifies the recommendation preview fields, opens `CleanupConfirmationWindow`, screenshots, clicks cancel, and restores env vars/temporary roots in `finally`.
- Updated `docs/development/gui-smokes.md` to document `OMNIX_ENTROPY_CDRIVE_SCAN_ROOT` as development and GUI smoke test tooling only.
- Updated `src/Css.App/rules.scan.json` so top-level `Temp` and `tmp` directories classify as `Temp`, allowing fixture and real `C:\Temp`/`C:\tmp` directories to become quarantine-gated cleanup candidates.
- Verification: focused fixture/static tests passed 3/3; top-level temp rules test passed 1/1; `ProductExperienceTests` passed 112/112; full suite passed 179/179; solution build passed with 0 warnings/errors.
- GUI verification gap: running the real WPF smoke was rejected by the approval/usage-limit system, so no screenshot was captured. No `Css.App` or `Css.SmokeTools` process remained.
- Safety: the new smoke is cancel-only and fixture-scoped. It must not click `CleanupConfirmationConfirmButton`, execute cleanup, move real files, delete files, mutate registry/services/startup/tasks, run installers, change settings, control sessions, or call cloud AI.
## 2026-07-09 - Cleanup confirmation outcome preview planning

- Goal: Continue product-facing cleanup UX by showing a plain-language "after confirm" outcome preview in the low-risk cleanup confirmation dialog.
- Scope: Presentation model, WPF confirmation UI, and product/static tests only.
- Safety note: No execution policy change; cleanup remains behind confirmation, safety pipeline, quarantine, and timeline/undo center.

## 2026-07-09 - Cleanup confirmation outcome preview implementation

- Added `OutcomePreviewLines` to `CleanupConfirmationViewModel` and generated beginner-facing outcome copy in `CleanupConfirmationPresenter`.
- Added `CleanupConfirmationOutcomeHeaderTextBlock` and `CleanupConfirmationOutcomeListBox` before the collapsed technical-details expander in `CleanupConfirmationWindow.xaml`.
- Updated `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1` to require the outcome preview list before it captures the confirmation screenshot and cancels.
- Verification: focused red/green confirmation tests, focused red/green smoke static test, `ProductExperienceTests` 112/112, full suite 179/179, solution build 0 warnings/errors, and no `Css.App`/`Css.SmokeTools` process remained.

## 2026-07-09 - Uninstall plan window readability and hooks

- Goal: Make the "卸载干净点" plan window readable and GUI-smoke friendly while keeping it plan-only.
- Added stable AutomationIds to the uninstall plan window title, summary, safety text, official uninstaller line, post-scan line, workflow list, official confirmation summary, warnings/checklist/preflight lists, execution gate, residue sections, final reminder, and close button.
- Converted key plan collections from `ItemsControl` to `ListBox` so UIAutomation can reliably find them in future smokes.
- Safety note: no official uninstaller execution, residue cleanup, registry/service/startup/task mutation, installer execution, or cloud AI path was added.
- Verification: focused test failed first for missing hooks, then passed 1/1; `ProductExperienceTests` passed 113/113; full suite passed 180/180; solution build passed with 0 warnings/errors; no `Css.App`/`Css.SmokeTools` process remained.

## 2026-07-09 - Uninstall plan window GUI smoke script

- Added `.omx/gui-uninstall-plan-window-smoke.ps1`.
- The script launches the app, opens Application Management, scans apps, selects an app with enabled `DrawerUninstallButton`, opens the uninstall plan window, verifies key `UninstallPlan*` AutomationIds, saves `.omx\qa-uninstall-plan-window.png`, and clicks only `UninstallPlanCloseButton`.
- Safety note: the script does not run an official uninstaller or touch residue cleanup, registry, services, startup, tasks, migration, settings, sessions, installers, or cloud AI.
- Verification: focused static test failed first because the script was missing, then passed 1/1; `ProductExperienceTests` passed 114/114; full suite passed 181/181; solution build passed with 0 warnings/errors; no `Css.App`/`Css.SmokeTools` process remained.

## 2026-07-09 - Uninstall plan window real GUI smoke verified

- Ran `.omx/gui-uninstall-plan-window-smoke.ps1` with GUI approval. The first run failed with `Uninstall plan window was not found`.
- Root cause: the smoke only searched process-owned top-level child windows, but this WPF modal is reliably discoverable through a stable descendant control.
- Added a failing static test requiring descendant modal lookup, then updated the script with `Find-WindowByDescendantAutomationId` using root-descendant search plus `TreeWalker.ControlViewWalker` parent-window walking.
- Real GUI smoke passed with `planWindowFound=true`, `closedPlanWindow=true`, and screenshot `.omx\qa-uninstall-plan-window.png`.
- Visual inspection: screenshot shows readable plan-only uninstall copy, official uninstaller path, post-uninstall residue summary, workflow steps, and only the `知道了` close button.
- Verification: `ProductExperienceTests` passed 114/114; full suite passed 181/181; solution build passed with 0 warnings/errors; no `Css.App`/`Css.SmokeTools` process remained.

## 2026-07-09 - Started uninstall residue custom confirmation slice

- Objective: make low-risk post-uninstall residue cleanup reuse `CleanupConfirmationWindow` instead of the old path-first confirmation `MessageBox`.
- Scope: `ReviewSelectedUninstallResidueAsync` and product tests only; no official uninstaller execution, high-risk residue cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, or cloud AI path should be added.
- TDD plan: add a failing product test for the residue confirmation handler, then minimally wire the handler through `CleanupConfirmationPresenter` and the custom confirmation dialog.
- TDD red: `Uninstall_residue_low_risk_confirmation_uses_custom_quarantine_confirmation_window` failed because `ReviewSelectedUninstallResidueAsync` still used `MessageBox.Show(BuildResidueConfirmMessage(...))`.
- Implementation: stored `review.LowRiskOperation` in `lowRiskOperation`, validated that same descriptor, opened `CleanupConfirmationWindow` from `CleanupConfirmationPresenter.Create(lowRiskOperation, DefaultQuarantineRoot())`, and proceeded only when `ShowDialog() == true`.
- Removed the unused path-first `BuildResidueConfirmMessage` / `FormatPathList` helpers.
- Verification: focused red/green test passed 1/1; residue-focused tests passed 10/10; `ProductExperienceTests` passed 115/115; full suite passed 182/182; solution build passed with 0 warnings/errors.

## 2026-07-09 - C-drive cleanup confirmation GUI proof and shared modal helper

- Ran `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1`; the first run failed with `Cleanup confirmation window was not found`.
- Root cause: the C-drive smoke only checked root child windows, while this WPF modal is reliably discoverable through a stable descendant AutomationId.
- TDD: updated `C_drive_cleanup_gui_smoke_uses_isolated_scan_fixture_and_cancels_confirmation` to require descendant modal discovery, observed it fail, then added the fallback and watched the test pass.
- Real C-drive cleanup smoke passed with `confirmationDialogFound=true`, `cancelClicked=true`, `fixtureStillExists=true`, `quarantineItemCount=0`, and screenshot `.omx\qa-cdrive-cleanup-confirmation.png`; visual inspection shows the outcome preview before technical details.
- TDD: updated the C-drive and uninstall-plan smoke static tests to require shared helper extraction and observed both fail while scripts duplicated `Find-WindowByDescendantAutomationId`.
- Moved `Find-WindowByDescendantAutomationId` and `Find-SecondaryWindowWithChild` into `.omx/wpf-smoke-helpers.ps1`; both GUI smoke scripts now call the shared helper and no longer define duplicate functions.
- Real GUI smokes after extraction: C-drive cleanup confirmation passed; uninstall-plan passed with `planWindowFound=true`, `closedPlanWindow=true`.
- Verification: focused static tests passed 2/2; `ProductExperienceTests` passed 115/115; full suite passed 182/182; solution build passed with 0 warnings/errors; process check found no `Css.App`/`Css.SmokeTools`.

## 2026-07-09 - Residue confirmation fixture plumbing and cancel-only smoke script

- Objective: make the post-uninstall low-risk residue confirmation path GUI-smokeable without reading or changing real installed software.
- TDD: replaced the cached still-installed expectation with `Residue_review_handler_rescans_before_deciding_software_still_installed`; it first failed because `ReviewSelectedUninstallResidueAsync` checked cached `_softwareProfiles` before a fresh scan.
- Implementation: added `ScanSoftwareProfilesAsync()`, routed app scanning/snapshot/residue review through it, removed the cached-first still-installed branch, and updated `_softwareProfiles` from the fresh scan before building the residue report.
- TDD: added `Software_inventory_fixture_override_is_process_scoped_for_gui_smoke_fixtures` and `SoftwareInventoryFixtureScannerTests`; they first failed because `OMNIX_ENTROPY_SOFTWARE_FIXTURE` and `SoftwareInventoryFixtureScanner` did not exist.
- Implementation: added process-scoped software fixture resolution in `AppDevelopmentPathResolver` and `SoftwareInventoryFixtureScanner`, which reads scripted JSON scan sequences and repeats the final scan.
- TDD: added `Uninstall_residue_confirmation_gui_smoke_uses_software_fixture_and_cancels_confirmation`; it first failed because `.omx/gui-uninstall-residue-confirmation-smoke.ps1` did not exist.
- Added `.omx/gui-uninstall-residue-confirmation-smoke.ps1`: it creates isolated data/quarantine/residue roots, sets `OMNIX_ENTROPY_SOFTWARE_FIXTURE`, scans a fake app, clicks the residue review action, verifies `CleanupConfirmationWindow` outcome controls, saves `.omx\qa-uninstall-residue-confirmation.png`, clicks cancel only, and restores/removes fixture state in `finally`.
- Added AutomationIds for `AppsNavButton`, `ScanSoftwareButton`, `AppTilesListBox`, and `DrawerResidueReviewButton`.
- Updated `docs/development/gui-smokes.md` to document `OMNIX_ENTROPY_SOFTWARE_FIXTURE` as development and GUI smoke tests only.
- Verification: focused residue rescan test passed 1/1; software fixture tests passed 3/3; residue GUI smoke static test passed 1/1; combined focused tests passed 5/5; `ProductExperienceTests` passed 116/116; full suite passed 186/186; solution build passed with 0 warnings/errors.
- GUI verification gap: real `.omx/gui-uninstall-residue-confirmation-smoke.ps1` launch was rejected by the approval/usage-limit system, so no residue-confirmation screenshot is available yet. No `Css.App`/`Css.SmokeTools` process remained.
- Safety: the fixture is process-scoped and cancel-only. No official uninstaller execution, confirmation click, residue movement, real app mutation, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.

## 2026-07-09 - Residue cancel/quarantine inline outcome

- Objective: after a low-risk residue confirmation is canceled or successfully quarantined, show a beginner-readable inline result in the app drawer instead of relying only on the bottom status bar.
- TDD: added `Residue_drawer_inline_status_explains_cancel_and_quarantine_outcomes_without_paths`; it first failed because `UninstallResidueDrawerReviewPresenter` did not have `CreateCanceled` or `CreateQuarantined`.
- TDD: added `Residue_review_handler_shows_inline_cancel_and_quarantine_outcomes` before changing the WPF handler.
- Implementation: added `CreateCanceled(...)` and `CreateQuarantined(...)` outcome view models, both path-hidden and non-executable.
- Implementation: `ReviewSelectedUninstallResidueAsync` now calls `ShowResidueOutcomeInline(...)` after confirmation cancel and after successful quarantine, after refreshing app catalog/timeline so the outcome panel remains visible.
- Verification: focused new tests passed 2/2; `UninstallResidueScanTests|ProductExperienceTests` passed 127/127.
- Safety: display-only change. No official uninstaller execution, confirmation bypass, residue auto-cleanup, permanent delete, high-risk residue handling, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.

## 2026-07-09 - Residue outcome undo-center navigation button

- Objective: successful residue quarantine should give the user an obvious "view undo center" path, but the button must only navigate.
- TDD: extended `Residue_drawer_inline_status_explains_cancel_and_quarantine_outcomes_without_paths`; it first failed because the residue outcome model lacked `PrimaryActionText` and `PrimaryActionKey`.
- TDD: added `App_drawer_action_host_primary_button_only_navigates_to_safe_pages`; it required `DrawerActionPreviewPrimaryButton`, binding to state action fields, Timeline-only navigation, and no restore/pipeline calls in the click handler.
- Implementation: added optional primary action fields to `UninstallResidueDrawerReviewViewModel` and `AppDrawerActionHostViewModel`.
- Implementation: success outcome sets `PrimaryActionText` to "查看后悔药中心" and `PrimaryActionKey` to `Timeline`; cancel outcome leaves both empty.
- Implementation: WPF drawer action panel now has a hidden-by-default `DrawerActionPreviewPrimaryButton`; the click handler only calls `ShowPage("Timeline")` for the Timeline key and explains that no automatic restore occurs.
- Verification: focused new tests passed 2/2; `UninstallResidueScanTests|ProductExperienceTests` passed 128/128.
- Safety: navigation-only. No restore click, cleanup execution, official uninstaller execution, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.

## 2026-07-09 - Residue cancel outcome GUI smoke assertion

- Objective: make the residue confirmation smoke prove that cancel leaves a visible "nothing happened" outcome and does not show the undo-center action button.
- TDD: strengthened `Uninstall_residue_confirmation_gui_smoke_uses_software_fixture_and_cancels_confirmation`; it first failed because the script did not mention `DrawerActionPreviewTitleTextBlock`, `DrawerActionPreviewPrimaryButton`, `cancelOutcomeVisible`, or `primaryButtonHiddenAfterCancel`.
- Implementation: after clicking `CleanupConfirmationCancelButton`, `.omx/gui-uninstall-residue-confirmation-smoke.ps1` waits for `DrawerActionPreviewTitleTextBlock`, checks `DrawerActionPreviewPrimaryButton` is missing/offscreen, and emits `cancelOutcomeVisible=true` and `primaryButtonHiddenAfterCancel=true`.
- Verification: focused smoke static test passed 1/1; focused action/outcome/smoke tests passed 3/3.
- Safety: smoke-only assertion. The script still does not reference `CleanupConfirmationConfirmButton` or `Invoke-Element $confirm`; no files are moved and no restore/cleanup/registry/service/startup/task operation is invoked.

## 2026-07-09 - Residue cancel outcome screenshot path

- Objective: capture a second screenshot after cancel so the inline outcome panel can be visually inspected separately from the confirmation dialog.
- TDD: extended `Uninstall_residue_confirmation_gui_smoke_uses_software_fixture_and_cancels_confirmation`; it first failed because the script did not mention `qa-uninstall-residue-cancel-outcome.png`.
- Implementation: added `$cancelOutcomeScreenshotPath`, saved it after the cancel outcome panel is visible and the primary action button is hidden, and emitted `cancelOutcomeScreenshot` in the smoke JSON.
- Verification: focused static smoke test passed 1/1; focused action/outcome/smoke tests passed 3/3.
- Safety: smoke evidence only. The script remains cancel-only and does not click confirm, restore, or execute cleanup.

## 2026-07-09 - Install routing learning memory core

- Objective: implement the backend part of install guard learning mode: remember user-chosen target roots by software or category and reuse them in installer analysis.
- TDD: extended `InstallerAnalyzerTests`; first run failed because `InstallRoutingMemory`, `InstallRoutingMemoryStore`, `FromUserMemory`, and `MemoryScope` did not exist.
- Implementation: added `InstallRoutingMemory`, `InstallRoutingMemoryRule`, and `InstallRoutingMemoryStore` with JSON load/save.
- Implementation: `InstallRoutingEngine.Recommend(...)` now accepts optional memory, prefers exact software rule over category rule over default rules, and marks `FromUserMemory`/`MemoryScope`.
- Implementation: `InstallerAnalyzer.AnalyzePath(...)` accepts optional `routingMemory`; default behavior remains unchanged and still never runs installers.
- TDD: extended `AppIdentityTests` for `install-routing-memory.json` under the app data root.
- TDD: added `Install_guard_analysis_loads_remembered_routing_rules_without_running_installer`; it first failed because `AnalyzeInstaller_Click` still called `InstallerAnalyzer.AnalyzePath(path)` directly.
- Implementation: install-page analysis now loads `InstallRoutingMemoryStore.Load(DefaultInstallRoutingMemoryPath())`, passes it to the analyzer, and appends path-source text to the read-only analysis output.
- Verification: `InstallerAnalyzerTests` passed 8/8; AppIdentity/WPF focused tests passed 3/3; install/AppIdentity focused tests passed 14/14.
- Safety: read-only recommendation and persistence only. No installer execution, global install-directory change, automatic install-argument passing, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added.

## 2026-07-09 - Install route remember button

- Objective: finish the install-page action that lets the user remember the current recommended install route after explicit confirmation.
- TDD: added/used `Routing_memory_can_remember_a_confirmed_route_for_the_same_software` and `Install_guard_remember_route_button_writes_memory_only_after_confirmation`; they require a stable button, a stored last analysis result, confirmation before persistence, and no installer execution.
- Implementation: `MainWindow.xaml` exposes `InstallRememberRouteButton` with stable AutomationId and disabled initial state.
- Implementation: `AnalyzeInstaller_Click` stores `_lastInstallerAnalysis`, enables the remember button after read-only analysis, and `RememberInstallRoute_Click` loads memory, calls `memory.RememberRoute(...)`, and saves through `InstallRoutingMemoryStore.Save(...)` only after an OK confirmation.
- Verification: focused install/app identity/product tests passed 16/16; `ProductExperienceTests` passed 120/120; full suite passed 194/194; solution build passed with 0 warnings/errors; process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process.
- Safety: memory persistence only. No installer execution, global ProgramFiles change, automatic install-argument passing, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added.

## 2026-07-09 - Install route memory scope choice

- Objective: let users decide whether a remembered install route applies only to the current software or to the whole category.
- TDD: added `Routing_memory_can_remember_a_confirmed_route_for_the_whole_category`; it first failed because `InstallRoutingMemory` lacked `RememberRouteForCategory`.
- TDD: added `Install_route_memory_choice_presenter_explains_scope_without_running_installer`; it first failed because `InstallRouteMemoryChoicePresenter` did not exist.
- TDD: strengthened the install-page product test to require `InstallRouteMemoryChoiceWindow`, stable AutomationIds, scope selection, and no `MessageBox.Show` in `RememberInstallRoute_Click`.
- Implementation: added `InstallRoutingMemoryScope`, `RememberRouteForCategory(...)`, `InstallRouteMemoryChoicePresenter`, and `InstallRouteMemoryChoiceViewModel`.
- Implementation: added `InstallRouteMemoryChoiceWindow.xaml` / `.xaml.cs` with software-only, category, and cancel buttons.
- Implementation: `RememberInstallRoute_Click` now opens the choice window and saves either `memory.RememberRoute(...)` or `memory.RememberRouteForCategory(...)` based on `SelectedScope`; cancel writes nothing.
- Verification: focused new tests passed 3/3; install-focused tests passed 18/18; `ProductExperienceTests` passed 120/120; full suite passed 196/196; solution build passed with 0 warnings/errors; process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process.
- Safety: recommendation-memory UX only. No installer execution, global install-directory change, automatic install-argument passing, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added.

## 2026-07-09 - Learned install rules read-only view

- Objective: show what install-routing rules OMNIX-Entropy has learned so the user can audit the assistant's memory without opening JSON.
- TDD: added `Install_routing_memory_presenter_shows_plain_learned_rules_without_json`; it first failed because `InstallRoutingMemoryPresenter` did not exist.
- TDD: added `Install_guard_page_shows_learned_rules_read_only`; it first failed because the page lacked `LoadInstallRoutingMemoryRules` and the stable ListBox/Summary controls.
- Implementation: added `InstallRoutingMemoryPresenter`, `InstallRoutingMemoryListViewModel`, and `InstallRoutingMemoryRuleRowViewModel`.
- Implementation: install page now has `InstallRoutingMemorySummaryTextBlock` and `InstallRoutingMemoryListBox`; `LoadInstallRoutingMemoryRules()` reads memory and binds rows during startup and after a rule is remembered.
- Verification: focused new tests passed 2/2; install-focused tests passed 20/20; `ProductExperienceTests` passed 121/121; full suite passed 198/198; solution build passed with 0 warnings/errors; process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process.
- Safety: read-only display only. No learned-rule deletion/editing, installer execution, global install-directory change, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added.

## 2026-07-09 - Forget learned install rule

- Objective: let users remove an install-routing memory rule while making clear that this only changes future recommendations.
- TDD: added `Install_routing_memory_can_forget_a_presented_rule_by_key`; it first failed because rows lacked `RuleKey`/`CanForget` and memory lacked `ForgetRule`.
- TDD: added `Install_guard_forget_learned_rule_only_edits_memory_after_confirmation`; it first failed because the WPF page lacked selection and forget handlers.
- Implementation: learned-rule rows now carry `RuleKey` and `CanForget`; empty placeholder rows cannot be forgotten.
- Implementation: `InstallRoutingMemory.ForgetRule(...)` removes the matching software/category rule by stable key.
- Implementation: install page now has `ForgetInstallRoutingRuleButton`; it enables only for real learned rules and asks for confirmation before saving updated memory and refreshing the list.
- Verification: focused new tests passed 2/2; install-focused tests passed 22/22; `ProductExperienceTests` passed 122/122; full suite passed 200/200; solution build passed with 0 warnings/errors; process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process.
- Safety: app-memory edit only. No installed app mutation, installer execution, global install-directory change, file movement, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added.

## 2026-07-10 - Post-install change report cards

- Objective: turn the install after-change report into beginner-readable cards before raw technical details.
- TDD: added `Diff_presenter_creates_beginner_cards_before_technical_details`; it first failed because `InstallSnapshotDiffPresenter` did not exist.
- TDD: added `Install_diff_page_shows_beginner_cards_before_raw_technical_report`; it failed because the install page lacked `InstallDiffSummaryTextBlock`, `InstallDiffCardsListBox`, and `InstallDiffTechnicalDetailsExpander`.
- Implementation: added `InstallSnapshotDiffPresenter`, `InstallSnapshotDiffViewModel`, and `InstallSnapshotDiffCardViewModel` for plain summaries, C-drive write counts, background-change counts, Agent advice, safety text, and technical detail lines.
- Implementation: install page now shows `InstallDiffSummaryTextBlock`, `InstallDiffCardsListBox`, and a collapsed `InstallDiffTechnicalDetailsExpander` containing the raw `InstallDiffTextBox`.
- Implementation: `BuildInstallDiff_Click` now calls `InstallSnapshotDiffPresenter.Create(report)` and `ApplyInstallDiffPresentation(view)` instead of writing the raw diff directly to the first visible area.
- Verification: focused install-diff tests passed 2/2; `ProductExperienceTests` passed 123/123; install-focused tests passed 21/21; full suite passed 202/202; solution build passed with 0 warnings/errors; process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process.
- Safety: read-only presentation only. No installer execution, snapshot data expansion, software inventory behavior change, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added.

## 2026-07-10 - Install report Agent explanation and GUI proof

- Objective: explain install-report findings in plain language and tell a beginner what to do next without executing any action.
- TDD: added two presenter tests; they first failed with CS0103 because `InstallSnapshotDiffAgentPresenter` did not exist.
- TDD: added an on-demand WPF contract test; after the presenter existed it still failed because the Agent button, panel, and bindings were missing.
- Implementation: added `InstallSnapshotDiffAgentViewModel` and `InstallSnapshotDiffAgentPresenter` with separate C-drive/background/no-pressure advice, safe next steps, hidden technical identifiers, and `CanExecuteDirectly=false`.
- Implementation: added `InstallDiffAgentExplainButton` and a collapsed Agent panel before technical details; fresh snapshots invalidate stale advice.
- Inspection: found the long install page was not scrollable; added `InstallPageScrollViewer` plus stable AutomationIds for navigation, snapshot, report, Agent, and technical-detail controls.
- TDD/GUI: added `.omx/gui-install-diff-agent-smoke.ps1` and its static contract test. The script uses isolated data plus a two-scan software fixture and never runs an installer.
- GUI correction: first screenshot evidence did not visibly prove the Agent panel. Added forced bottom scrolling, desktop screenshots, and repeated maximize guards; rerun produced clean report and Agent screenshots.
- Verification: focused tests passed 4/4; `ProductExperienceTests` passed 125/125; install-focused tests passed 25/25; full suite passed 206/206; solution build passed with 0 warnings/errors.
- Real GUI evidence: `fixtureOnly=true`, 4 report cards, visible Agent headline, 4 plan steps, technical details collapsed; `.omx/qa-install-diff-cards.png` and `.omx/qa-install-diff-agent.png` visually inspected.
- Cleanup: no `Css.App`, `Css.SmokeTools`, or `OMNIX` process remained; temporary data and fixture files were removed.
- Safety: read-only local advice and fixture-only smoke. No installer, migration, cleanup, startup/service/task/registry mutation, routing-memory edit, restore, settings, session, or cloud AI action was added.

## 2026-07-10 - Install report action plan

- Objective: turn the install-report explanation into a short Agent-owned treatment sequence that a beginner can follow without choosing technical operations.
- TDD: two presenter tests first failed with CS0103 because `InstallSnapshotDiffActionPlanPresenter` did not exist; the WPF and smoke contract tests then failed on the missing surface and proof path.
- Implementation: added `InstallSnapshotDiffActionPlanPresenter`, plan/item view models, C-drive review, background review, follow-up observation, no-pressure observation, and explicit non-executable safety fields.
- Implementation: added `生成处理方案`, a collapsed action-plan panel, ordered list bindings, stable AutomationIds, stale-plan invalidation, and status copy stating no handling ran.
- GUI debugging: the first smoke proved the rendered TextBlock contained `尚未执行`, but Windows PowerShell 5.1 misread the UTF-8 source literal. A second attempt exposed unstable `[string]::Concat(char...)` overload binding. The final script builds the keyword with ASCII-only `-join` plus Unicode char codes.
- Verification: focused tests 4/4; `ProductExperienceTests` 127/127; install-focused tests 29/29; full suite 210/210; solution build 0 warnings/errors.
- Real GUI evidence: four report cards, four Agent explanation steps, three action-plan items, `nothingExecutedVisible=true`, and technical details collapsed. `.omx/qa-install-diff-action-plan.png` was visually inspected and shows all three ordered decisions plus the safety boundary without clipping.
- Cleanup: no app/smoke process remained; temporary fixture/data paths were removed; three retained PNG evidence files exist.
- Safety: plan-only. No system-changing handler or operation descriptor was added.

## 2026-07-10 - Install report evidence classification

- Objective: replace the generic “first determine purpose” step with rule-based, explicitly preliminary classifications for every new C-drive location and background mechanism.
- TDD: three presenter tests first failed because the classifier/enums and `ReviewSummary` did not exist; product tests also required the compact WPF summary and smoke proof.
- Implementation: added six C-drive categories (install files, cache, configuration, logs, model/data, unknown) and three background kinds (startup, service, scheduled task), with generic numbered display names, purpose, advice, confidence, risk, and `CanExecuteDirectly=false`.
- Privacy: raw C paths, startup names, service names, and task names stay in the technical report; beginner-facing review items and summaries expose only counts and categories.
- UI: added one blue `Agent 初步判断` summary above the ordered plan list instead of another default list/card group.
- Verification: focused tests 5/5; product tests 127/127; install-focused tests 32/32; full suite 213/213; solution build 0 warnings/errors.
- GUI: smoke returned `classificationSummaryVisible=true`, three plan items, `nothingExecutedVisible=true`, and technical details collapsed. The first screenshot showed transient black capture blocks and was rejected; an unchanged rerun produced a clean, visually inspected `.omx/qa-install-diff-action-plan.png`.
- Cleanup: no app/smoke process remained; temporary data and fixture files were removed.
- Safety: read-only classification only; no extra scan or system mutation was added.

## 2026-07-10 - On-demand install evidence review

- Objective: answer “why did Agent judge this way?” without restoring the old technical text pile.
- TDD: the model test first failed because `InstallSnapshotDiffActionPlanViewModel` lacked `EvidenceReview`; the product test then failed because the WPF expander and bindings were missing; the smoke contract failed because no expanded-review proof existed.
- Implementation: the action plan now carries the existing evidence review; the install page adds a collapsed `为什么这样判断` expander directly below the compact summary, with separate generic C-drive and background lists plus a safety boundary.
- UX correction: screenshot inspection showed blue selection highlighting on read-only lists. A second red/green loop required `IsHitTestVisible=false` and `Focusable=false` for action-plan and evidence lists.
- Privacy/safety: beginner text exposes generic numbered findings, purpose, advice, confidence, and risk only. Raw paths/startup/service/task identifiers remain in collapsed technical details; all review objects stay non-executable.
- Verification: full suite passed 215/215; solution build passed with 0 warnings/errors. GUI smoke proved default collapse, one C-drive item, three background items, identifier hiding, and collapsed technical details.
- Visual evidence: clean `.omx/qa-install-diff-action-plan.png` and `.omx/qa-install-diff-evidence-review.png` were inspected. Transient black composition captures were rejected and rerun unchanged.
- Cleanup: no app/smoke process remained; temporary fixture/data paths were removed.

## 2026-07-10 - Evidence-driven eligible actions

- Objective: let Agent decide which kinds of plan are worth considering from classified evidence, without presenting technical choices or direct execution buttons.
- TDD: model tests first failed for missing `EligibleActions` and action kinds; the WPF product test failed for the missing candidate list/binding; the smoke contract failed for missing real-GUI proof.
- Implementation: added five deterministic candidate kinds: cache-clean plan, storage-setting guidance, reinstall/migration plan, startup-disable plan, and observe-only. Candidates are deduplicated and ordered, expose missing evidence and rollback/confirmation needs, and keep `CanExecuteDirectly=false`.
- Safety choice: unknown paths, services, and scheduled tasks only add observe-only; service/task names do not produce disable candidates. No operation descriptor or handler was added.
- UI: the existing collapsed evidence review now contains a compact, non-selectable `Agent 可以考虑` list with reason, evidence summary, missing evidence, and safety copy. It contains no buttons.
- GUI debugging: one rerun exposed transient `SetFocus` failure. `WaitForInputIdle` alone did not fix it; bounded focus polling did. Nested-list `IsOffscreen` also falsely claimed visibility, so screenshot scrolling now checks real element/viewport rectangle intersection.
- Verification: full suite passed 217/217; solution build passed with 0 warnings/errors. GUI smoke returned three candidates, `eligibleActionsPlanOnly=true`, hidden identifiers, and collapsed technical details.
- Visual evidence: clean `.omx/qa-install-diff-eligible-actions.png` shows the Agent candidate heading plus cache-clean and startup-review candidates with missing-evidence/safety text and no action buttons.
- Cleanup: no app/smoke process remained; temporary fixture/data paths were removed.

## 2026-07-10 - On-demand candidate plan preview

- Objective: expand an Agent candidate into a safe, beginner-readable preview only when the install diff contains enough ownership evidence.
- TDD: preview presenter tests first failed because the status/model/presenter did not exist; the product test then failed because the candidate button, preview panel, stable AutomationIds, and non-execution handler contract were missing.
- Implementation: cache and startup previews build a uniquely owned profile and reuse existing cleanup/startup presenters; migration reuses the existing migration presentation; storage and observe candidates remain generic; ambiguous app ownership returns an explicit refusal.
- UI: each candidate has a `查看方案预览` button. The inline panel shows readiness, Agent takeaway, preview lines, missing evidence, and safety text, with no execution button or pipeline call.
- Verification: focused preview/UI tests passed 5/5; install/product tests passed 146/146; after smoke-helper changes the full suite passed 222/222 and the solution build passed with 0 warnings/errors.
- GUI debugging: UIAutomation focus polling timed out because the top-level WPF element was not keyboard-focusable. A native `SetForegroundWindow` replacement also timed out because Windows foreground-lock policy rejected it. The smoke contract now requires `ShowWindowAsync` plus `SetWindowPos(HWND_TOPMOST)` for visibility only.
- GUI proof: not completed in this run. The local launch request was rejected by the Codex GUI usage limit, so no candidate-preview screenshot exists yet. This is recorded as a verification gap, not a product failure.
- Cleanup: no app/smoke process remained and both isolated fixture paths are absent.
- Safety: preview-only; no operation descriptor, pipeline invocation, cleanup, migration, startup/service/task/registry change, or installer execution was added.

## 2026-07-10 - Began uninstall recovery-truth slice

- Completed the previously blocked install-candidate GUI smoke: ready preview, no execution, hidden raw identifiers, collapsed technical details, and a clean screenshot.
- Selected the next V1 slice after inspecting the official-uninstall gate and residue flow: make reversibility limits explicit and collapse advanced uninstall details before any execution path is enabled.

## 2026-07-10 - Uninstall recovery truth and execution-gate hardening

- Added `UninstallRecoveryAssessmentPresenter`: official uninstall requires reinstall to recover, low-risk quarantined residue is restorable, personal data stays untouched by default, and all output is non-executable.
- Rebuilt the uninstall modal around the Agent conclusion and three steps; raw commands and detailed preflight are default-collapsed.
- Added typed recovery evidence and gate requirements for no-automatic-undo acknowledgment, recovery method/reference, and user-data backup confirmation.
- Verification: product tests 132/132, full tests 225/225, build 0 warnings/errors. Real GUI smoke proved three protection lines, three steps, collapsed details, no execution button, and a clean screenshot after rejecting one compositor-corrupted capture.
- Cleanup: no app/smoke process or temporary install/uninstall fixture state remained.

## 2026-07-10 - Began read-only reinstall-source discovery

- Existing software records preserve uninstall command, icon, and install location but drop `InstallSource`, Windows Installer flags, and MSI product-code metadata.
- Safety boundary: collect metadata read-only; automatically trust only an existing installer file with a publisher-matching signature. Directories, product codes, unsigned files, and signature mismatches remain confirmation-required hints and cannot satisfy the uninstall execution gate.

## 2026-07-10 - Completed read-only reinstall-source discovery

- Added registry record parsing for `InstallSource`, `WindowsInstaller`, and GUID product-code hints, then preserved those facts in `SoftwareProfile`.
- Added `ReinstallSourceReadinessPresenter`; only an existing EXE/MSI file with a publisher-matching signature creates typed reinstall evidence, and that evidence never claims personal-data backup.
- Added compact recovery-readiness copy to the first uninstall Agent panel and raw provenance to collapsed advanced details. The real app passes `File.Exists`, `Directory.Exists`, and `SignatureInspector.GetSignatureSubject` into the presenter.
- Verification: scanner tests 15/15, product tests 137/137, full suite 232/232, build 0 warnings/errors. GUI smoke proved readiness visible, advanced details collapsed, no execution control, and produced a clean inspected screenshot.
- Cleanup: no app/smoke process or temporary fixture state remained.

## 2026-07-10 - Began guided uninstall recovery preparation

- Next user-visible goal: replace passive recovery status with a guided, non-executing preparation panel for selecting an official installer, reviewing restore-point availability, and separately confirming personal-data backup.
- Safety boundary: file selection and WMI restore-point discovery are read-only. Existing restore points remain fallback hints; no selected file is launched and no restore point or backup is created in this slice.

## 2026-07-10 - Recovery preparation implemented; GUI proof pending

- Added read-only System Restore discovery, signed user-selected installer validation, separate backup acknowledgment, and local preview-session state. Existing restore points remain hints and cannot complete app recovery preparation.
- Added compact uninstall-window controls and real scanner composition; no selected file, installer, uninstaller, restore point, or backup operation is executed.
- Verification: scanner tests 16/16, product tests 142/142, full suite 238/238, build 0 warnings/errors.
- GUI launch was rejected by the Codex usage limit before any process started. The updated smoke contract is tested, but the new rendered layout and screenshot remain unverified.

## 2026-07-10 - Began verifiable uninstall evidence snapshots

- Safety audit found `OfficialUninstallExecutionGate` still accepts any non-empty `SnapshotId`; `Css.Snapshot` has no implementation behind that identifier.
- Chosen boundary: create a local, versioned evidence manifest for post-uninstall audit and comparison, explicitly `CanRestoreApplication=false`; typed reinstall evidence remains responsible for app recovery.

## 2026-07-10 - Verifiable uninstall evidence snapshot completed

- Implemented atomic local JSON manifests in `Css.Snapshot`, SHA-256 verification, typed snapshot evidence, and gate/preflight validation for presence, software identity, age, hash, rollback truth, and id consistency.
- Updated operation descriptors to carry snapshot manifest path/hash and `snapshotCanRestoreApplication=false` for future auditing.
- Verification: product tests 144/144, full suite 245/245, build 0 warnings/errors. Temp snapshot roots were removed and no app process remained.
- No real uninstall snapshot, installer, uninstaller, restore point, cleanup, or system mutation occurred.

## 2026-07-10 - Began non-executable uninstall final-confirmation draft

- Scope is backend-only while updated WPF remains visually unverified.
- The draft may write one verified OMNIX evidence manifest only after recovery preparation is complete; it cannot create an operation, invoke a pipeline, or launch any process.

## 2026-07-10 - Began read-only snapshot retention planning

- Local manifests contain paths and should not grow without bounds, but retention planning must not delete unfamiliar files or target-software data.
- This slice is plan-only: no filesystem move/delete API is allowed in the planner.

## 2026-07-10 - Read-only snapshot retention plan completed

- Added age/count policy, deterministic newest retention, expired/excess candidate reasons, and preserved-unknown reporting.
- Planner restricts enumeration to top-level OMNIX manifest names, rejects reparse/unknown/corrupt evidence, and exposes no move/delete path.

## 2026-07-10 - Began reversible snapshot archive operation

- Reuse the existing quarantine and timeline foundations instead of adding a second move/restore format.
- The handler must revalidate OMNIX root, manifest schema/name, and planned hash before any move, and roll back partial batches on failure.

## 2026-07-10 - Reversible snapshot archive operation completed

- Added hash-bound archive previews, explicit confirmation through `SafetyOperationPipeline`, execution-time root/manifest/hash checks, quarantine movement, restorable timeline entries, and reverse-order rollback on mid-batch failure.
- Verification: focused archive tests 6/6, full suite 257/257, build 0 warnings/errors. No temp directory/process residue.
- No permanent delete, real user archive, target-software change, installer, or uninstaller occurred.

## 2026-07-10 - Began unregistered official-uninstaller handler

- `Css.Elevated` is currently a Hello World stub. This slice adds only interfaces/handler logic and fake adapters in tests.
- No `Process.Start` adapter, `Program.cs` registration, app DI registration, or WPF execution control is allowed.

## 2026-07-10 - Unregistered official-uninstaller handler completed

- Added strict evidence/command revalidation, injected launcher and post-scan interfaces, exit-code/post-scan payloads, and non-restorable timeline recording.
- Empty argument commands are supported; descriptor argument tampering is rejected. No real launcher or registration exists.
- Verification: focused 7/7, full suite 264/264, build 0 warnings/errors; no temp directory/process residue and no App/Program registration match.
- Verification: focused 4/4, full suite 251/251, build 0 warnings/errors; no temp directory or process residue.

## 2026-07-10 - Non-executable final-confirmation draft completed

- Added refusal, snapshot-verification-failure, and ready-for-final-confirmation outcomes.
- Complete recovery preparation creates and verifies one local evidence manifest, then returns beginner-safe ready facts and pending confirmations. Incomplete preparation does not create the snapshot root.
- Static contract rejects operation/pipeline/process APIs in the service.
- Verification: focused 3/3, full suite 248/248, build 0 warnings/errors; no temp directory or process residue.

## 2026-07-10 - Began unregistered Windows launcher adapter

- The adapter will be real code but unreachable; tests inject a fake process runner and inspect `ProcessStartInfo`.
- `SystemProcessRunner` will be the only new file allowed to call the process API. Program/App registration remains forbidden.

## 2026-07-10 - Unregistered Windows launcher adapter completed

- Added exact `ProcessStartInfo` construction, UAC-cancel mapping, exit-code capture, cancellation propagation, and isolated real process runner.
- Tests use only a fake runner; no process started and no Program/App registration exists.
- Verification: focused 6/6, full suite 270/270, build 0 warnings/errors; no process/temp residue.

## 2026-07-10 - Began unregistered real post-uninstall scan adapter

- Adapter will reconstruct the before profile from the hashed manifest and reuse `UninstallResidueScanBuilder` against fresh inventory/path evidence.
- It is read-only and unregistered; no cleanup, quarantine, timeline mutation, or process launch occurs inside post-scan.

## 2026-07-10 - Unregistered real post-uninstall scan adapter completed

- Added fresh-inventory post-scan reconstruction from the pre-uninstall evidence manifest.
- Only paths reverified by the current path probe become residue candidates; old startup, service, and scheduled-task evidence remains a separate background-rescan hint.
- Inventory failure reports failure instead of claiming a clean uninstall, cancellation propagates, and manifest/software mismatch is refused before scanning.
- Verification: focused adapter tests 6/6; related uninstall tests 23/23; full suite 276/276; build 0 warnings/errors; no process/temp residue.
- No cleanup, quarantine, timeline mutation, process launch, App registration, or Elevated Program registration was added.

## 2026-07-10 - Began beginner post-uninstall result presentation

- Scope is a pure, non-executable presenter over typed post-scan outcomes.
- Beginner text must hide paths/background identifiers and must not turn scan failure or historical hints into a cleanup claim.

## 2026-07-10 - Beginner post-uninstall result presentation completed

- Added four path-free outcomes: scan failed, software still present, no visible residue, and review needed.
- The presenter ignores raw scanner summaries, shows counts and fixed Agent advice, blocks residue review while the app remains installed, and keeps `CanExecuteDirectly=false`.
- Verification: focused presenter tests 5/5; product/uninstall tests 178/178; full suite 281/281; build 0 warnings/errors; process/temp checks empty.
- No WPF wiring, operation creation, pipeline call, quarantine action, or process API was added.

## 2026-07-10 - Began fresh background residue re-enumeration

- Scope is exact-name read-only probes for manifest startup/service/task hints with Exists/Missing/Unknown results.
- Unknown or partial access must not become a clean-uninstall claim; verified matches remain high-risk technical evidence only.

## 2026-07-10 - Fresh background residue re-enumeration completed

- Added tri-state exact-name probes and a real read-only Windows reader for Run entries, service registry keys, and scheduled-task files.
- Crafted service/task identifiers are rejected; task traversal and reparse points become Unknown. Any Unknown makes the mandatory background recheck incomplete.
- Freshly verified matches enter only the high-risk residue report; beginner output shows counts and explicitly says background records will not be directly closed.
- Verification: scanner/presenter tests 12/12; product/uninstall tests 185/185; full suite 288/288; build 0 warnings/errors; no processes/temp items/registration matches.
- The reader, scanner, launcher, handler, and post-scan composition all remain unregistered and uncalled.

## 2026-07-11 - Elevated request/response boundary completed

- Added a pure final handoff contract that refuses missing/stale visual proof, changed confirmation text, incomplete safety flags, mutable/unsupported descriptor arguments, and invalid request ids.
- Ready drafts deep-copy and SHA-256-bind the confirmed descriptor. Typed responses require correlation and map to path-free beginner states; there is no transport, pipeline call, handler call, or registration.
- Verification: focused 7/7, official-uninstall related 38/38, then full suite 295/295 and build 0 warnings/errors before GUI work.

## 2026-07-11 - Recovery GUI timeout and owned-window discovery fixed

- The real smoke first showed a live main window stuck at “正在只读检查恢复准备...” for more than 60 seconds. `WindowsRestorePointScanner` had no WMI or outer timeout.
- Added a four-second typed restore-point scan result; timeout/failure stays unknown and no longer blocks the plan window.
- A desktop failure screenshot then proved the plan window was visible/rendered with its own native handle but absent from the UIAutomation root tree. Reused the repository's working `EnumWindows`/`FromHandle` fallback in the shared helper.
- Final GUI smoke passed at the original 10-second window gate and produced an inspected clean screenshot. Full suite passed 298/298; build passed with 0 warnings/errors; process/temp/registration checks were empty.

## 2026-07-11 - WPF final-confirmation checklist implemented

- Added a beginner panel before technical details with prepared/pending/missing lists and a fixed no-execution explanation. The button calls only the existing non-executable draft service.
- Added an isolated uninstall-evidence root override. Incomplete recovery preparation produces missing requirements without creating the root; complete preparation may create one verified audit manifest.
- Real GUI reached the panel and proved missing items plus no evidence-root creation. The initial safety-text check failed only because Windows PowerShell interpreted a Chinese source literal with the wrong encoding; the actual UI text was correct and the assertion now uses Unicode code points.
- A diagnostic screenshot was rejected for large composition black blocks. The corrected final rerun and cleanup were rejected by the GUI usage limit.
- Verification: full suite 300/300, solution build 0 warnings/errors, no processes/temp/evidence roots, and no execution references in the WPF window.

## 2026-07-11 - Final-confirmation visual gate accepted

- Rebuilt the full WPF application after changing the checklist scroll target from the title to the status line.
- Real GUI smoke passed with finalChecklistVisible=true, two missing recovery requirements, evidenceRootCreated=false, collapsed technical details, and no execution control.
- Inspected .omx/qa-uninstall-plan-window.png; the visible working area now includes both the final-checklist title and the plain incomplete-recovery status.

## 2026-07-11 - Beginner post-uninstall WPF result completed

- Moved the safe post-scan display contract into Css.Core.Uninstall so WPF can bind it without referencing the elevated executable project.
- Replaced previously hidden mojibake presenter copy with encoding-stable Chinese escapes for failure, software-still-present, clean, and review-needed states.
- Added UninstallPostScanResultWindow with stable AutomationIds, count-only facts, Agent advice, next action, hidden technical identifiers, a fixed no-further-mutation line, and only a close button.
- Added a DEBUG-only fixed-data launch argument and .omx/gui-uninstall-post-scan-result-smoke.ps1; real GUI proof passed with three facts, visible advice/safety text, no execution control, and a clean inspected screenshot.

## 2026-07-11 - One-time visual receipt and request session completed

- Added an in-memory receipt issuer that validates the UI contract, PNG signature/size, visible safety facts, and capture time; it hashes bytes immediately and stores no image.
- Tickets expire after ten minutes, cap outstanding entries, reject duplicate/unknown/future/stale evidence, and can be consumed once. Mutating the caller's PNG buffer after issue does not change the receipt hash.
- Added a request session that consumes the ticket before calling the existing correlation/hash-bound composer; replay is refused.
- Verification: focused receipt/session tests 7/7; final full suite 309/309; solution build 0 warnings/errors; registration, mutation-reference, process, and evidence-root audits passed.

## 2026-07-11 - Shared final consent and response display contracts completed

- Moved OfficialUninstallFinalUserConsent and the safe response display state/model into Css.Core.Uninstall so Css.App can use them without a Css.Elevated project reference.
- Added a pure final-consent presenter and builder. The view explains the official uninstaller, lack of automatic application rollback, and mandatory post-scan; all three acknowledgements are required before an exact timestamped consent is produced.
- Added the explicit softwareName operation argument while preserving fallback title parsing.
- Focused consent tests passed 7/7.

## 2026-07-11 - Final consent WPF and fake continuous flow completed

- Added OfficialUninstallFinalConsentWindow with stable AutomationIds, three plain checkboxes, count-based readiness, disabled-by-default confirmation, cancellation, and no technical paths or execution API.
- Added a DEBUG-only consent-to-result flow and GUI smoke. The real window began disabled, enabled only after all checks, and opened the existing fake post-scan result.
- Inspected qa-uninstall-final-consent.png and qa-uninstall-final-consent-result.png; both are clean and beginner-readable.
- The visual fixture is not the backend transport and invokes no handler, launcher, scanner, installer, or uninstaller.
