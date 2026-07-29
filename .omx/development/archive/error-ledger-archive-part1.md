# Archived error-ledger (2026-06-17 to 2026-07-15)

Historical entries moved out of `.omx/development/error-ledger.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

### 2026-06-17 - dotnet new 模板不接受 net8.0-windows 作 -f

- Symptom: `dotnet new classlib -f net8.0-windows` 报 "无效选项 -f net8.0-windows"，项目未创建。
- Wrong assumption: 以为 `-f` 接受带平台后缀的完整 TFM（net8.0-windows）。
- Root cause: `dotnet new` 模板的 `-f` 仅列基础框架（net8.0/net10.0/netstandard2.x），平台后缀 `-windows` 不在允许值内；需在生成后编辑 .csproj 的 `<TargetFramework>`。
- Detection method: 命令报错并列出可能值；`dotnet sln list` 确认仅 net8.0 项目被加入。
- Fix: 用 `-f net8.0` 生成，再正则替换 .csproj `<TargetFramework>net8.0</TargetFramework>` → `net8.0-windows`。WPF 模板自身会加 `-windows`。
- Prevention rule: 新建 Windows 专属项目一律 `-f net8.0` 生成后改 csproj；勿直接传 `net8.0-windows`。
- Skill candidate: yes

### 2026-06-30 - PowerShell default encoding corrupted a test file

- Symptom: `apply_patch` could not read `tests/Css.Tests\ProductExperienceTests.cs` and reported an invalid UTF-8 sequence after a PowerShell line rewrite.
- Wrong assumption: A quick `Get-Content` / `Set-Content` rewrite would preserve the source file encoding and localized text safely.
- Root cause: PowerShell decoded and re-encoded an already fragile localized/mojibake file with defaults that did not match the file content.
- Detection method: `apply_patch` failed with an invalid UTF-8 error, and the test file content was no longer stable enough for normal patching.
- Fix: Rewrote `ProductExperienceTests.cs` as an explicit UTF-8 file with ASCII assertions, then reran focused and full verification.
- Prevention rule: For localized or mojibake-affected source files, prefer `apply_patch` with stable ASCII anchors. If a full rewrite is unavoidable, use explicit UTF-8 encoding and immediately run tests/builds.
- Skill candidate: yes

### 2026-06-30 - Repeated parallel dotnet validation lock

- Symptom: Parallel `dotnet test` commands caused CS2012 because `Css.Core.dll` in `obj` was locked by `VBCSCompiler`.
- Wrong assumption: I treated two filtered test commands as safe to run in parallel because they were narrow.
- Root cause: They still shared the same project build outputs and compiler server.
- Detection method: One test command passed, the other failed with CS2012 and identified `VBCSCompiler` as the locking process.
- Fix: Ran `dotnet build-server shutdown`, then reran verification serially.
- Prevention rule: In this repository, do not parallelize `dotnet test` and `dotnet build` commands unless each command has isolated output directories. Serial verification is the default.
- Skill candidate: yes

### 2026-06-30 - Patch context copied from mojibake terminal output

- Symptom: `apply_patch` failed to add the new XAML button because the expected Chinese lines were not found.
- Wrong assumption: Chinese text shown by `Get-Content` in the current PowerShell output could be safely copied as patch context.
- Root cause: The terminal rendered UTF-8 Chinese strings as mojibake for some files, so the copied context did not match the actual file contents.
- Detection method: `apply_patch` reported “Failed to find expected lines”; `Select-String` against stable control names showed the real Chinese source text.
- Fix: Re-ran the patch using actual UTF-8 text and then used stable anchors such as `DrawerUninstallPreviewListBox`, `DrawerDisableStartupButton`, and method names.
- Prevention rule: When editing localized UI files, anchor patches on `x:Name`, method names, or other ASCII identifiers; use localized strings only after confirming the source text with `Select-String` or an editor that displays UTF-8 correctly.
- Skill candidate: no

### 2026-06-30 - 并行 dotnet 验证锁住 obj 输出

- Symptom: `dotnet build ComputerSecuritySoftware.slnx --no-restore` 报 CS2012，无法写入 `src\Css.Scanner\obj\Debug\net8.0-windows\Css.Scanner.dll`，文件被 `VBCSCompiler` 占用。
- Wrong assumption: 以为 `dotnet test` 和 `dotnet build` 可以在同一工作区安全并行运行。
- Root cause: 两个命令共享项目 `obj/bin` 输出路径，编译服务器持有输出文件句柄，导致构建抢写失败。
- Detection method: 构建输出明确显示 CS2012 和 `VBCSCompiler` 文件锁；测试本身已通过。
- Fix: 运行 `dotnet build-server shutdown` 释放编译服务器锁，再串行重跑 `dotnet build ComputerSecuritySoftware.slnx --no-restore` 成功。
- Prevention rule: .NET 验证中不要并行运行会写同一 `obj/bin` 的 `dotnet test` 与 `dotnet build`；需要并行时必须使用隔离输出目录。
- Skill candidate: yes

### 2026-06-30 - WPF ItemsSource 不能直接接无目标类型集合表达式

- Symptom: `dotnet build ComputerSecuritySoftware.slnx --no-restore` 报 CS9174：无法使用集合表达式初始化类型 `IEnumerable`，因为该类型不可构造。
- Wrong assumption: 以为 `TimelineListBox.ItemsSource = [item]` 可由 `ItemsSource` 自动推断集合类型。
- Root cause: `ItemsSource` 是非泛型 `IEnumerable`，C# 集合表达式需要可构造的目标类型或明确泛型目标。
- Detection method: 完整构建在 `MainWindow.xaml.cs` 的 `ItemsSource` 赋值处失败。
- Fix: 改为 `new[] { TimelineEntryView.Message(...) }`。
- Prevention rule: WPF `ItemsSource` 赋值单项集合时使用明确数组或 `List<T>`，不要依赖集合表达式推断非泛型接口。
- Skill candidate: no

### 2026-06-30 - 决策卡 UI 映射使用了计划名而非实际枚举名

- Symptom: `dotnet build ComputerSecuritySoftware.slnx --no-restore` 报 CS0117，`RecommendationAction` 不包含 `FixInstallPath`。
- Wrong assumption: 直接把产品计划里的“修复安装路径”概念写成枚举名。
- Root cause: 实际代码枚举使用 `RepairInstallLocation`，UI 文案映射没有先对照已有 public type。
- Detection method: 完整构建在 `MainWindow.xaml.cs` 的 action 文案映射处失败。
- Fix: 将映射改为 `RecommendationAction.RepairInstallLocation`。
- Prevention rule: 给 UI 加枚举文案时先打开定义或用编译器补全确认实际枚举成员，避免把计划术语当代码符号。
- Skill candidate: no

### 2026-06-30 - UI Automation 条件构造错误导致假阴性

- Symptom: 第一次 UI 冒烟脚本启动应用并截图成功，但所有导航按钮都报告 `missing button`，同时输出 `PropertyCondition value ... must be 'ControlType'`。
- Wrong assumption: 以为可以在 `New-Object System.Windows.Automation.AndCondition` 参数里直接内联创建 `PropertyCondition` 并传入 `[ControlType]::Button`。
- Root cause: PowerShell 一行构造函数参数绑定把 ControlType 条件解析错，导致条件对象为空，后续 `FindFirst` 也跟着失败。
- Detection method: UIA 命令输出异常堆栈，且所有按钮都显示 missing，不符合截图中实际可见按钮。
- Fix: 先分别创建 `$controlTypeCond` 和 `$nameCond`，再传给 `AndCondition`；重跑后 6 个导航按钮全部返回 `ok: <button> -> <title>`。
- Prevention rule: UIA 冒烟脚本用具名中间变量构造条件，并验证点击后的可见页面标题；不要只验证 `Invoke` 成功。
- Skill candidate: yes

### 2026-06-30 - WMI 服务查询被拒导致服务画像可能缺失

- Symptom: 本机执行 `Get-CimInstance Win32_Service -Filter "Name='MarvisSvr'"` 返回“拒绝访问”。
- Wrong assumption: 以为标准用户环境下 WMI `Win32_Service` 足够作为服务路径来源。
- Root cause: 本机 WMI/CIM 权限限制导致服务查询不可用；原 scanner 虽会吞掉异常，但会因此缺失服务归属。
- Detection method: Marvis 只读验证时直接查询 `MarvisSvr` 失败；同一服务的注册表 `HKLM:\SYSTEM\CurrentControlSet\Services\MarvisSvr\ImagePath` 可读。
- Fix: `SoftwareInventoryScanner` 增加注册表服务兜底，并用 `ServiceEntryFactory` 解析 `ImagePath`。
- Prevention rule: Windows 系统信息来源必须可降级；WMI/COM/命令行失败时优先寻找只读注册表或文件来源，且测试真实受限环境。
- Skill candidate: yes

### 2026-06-30 - 卸载残留测试漏 using 导致红灯噪音

- Symptom: 新增 `Quarantine_policy_accepts_low_risk_uninstall_residue_plan_only_after_confirmation` 后，测试编译失败提示找不到 `QuarantineOperationPolicy`。
- Wrong assumption: 以为测试文件已有所有 Core 命名空间引用。
- Root cause: `QuarantineOperationPolicy` 位于 `Css.Core.Quarantine`，新测试文件只引入了 `Css.Core.Operations` 和 `Css.Core.Software`。
- Detection method: 窄测试输出 CS0103，指出当前上下文不存在 `QuarantineOperationPolicy`。
- Fix: 添加 `using Css.Core.Quarantine;` 后重跑，进入预期缺少 `UninstallResidueOperationPlanner` 的红灯。
- Prevention rule: TDD 红灯应尽量来自缺少目标行为；跨命名空间用现有策略类时先检查 `using`，避免编译噪音。
- Skill candidate: no
### 2026-07-01 - FluentAssertions string NotContain overload mistake

- Symptom: The first run of `App_drawer_top_summary_uses_plain_chinese_before_technical_details` failed to compile with CS1503 instead of failing on old English UI text.
- Wrong assumption: `StringAssertions.NotContain` accepted a `StringComparison` overload like some .NET string APIs.
- Root cause: FluentAssertions string assertions in this project expose `NotContain(string, string because, params object[])`, not a `StringComparison` overload.
- Detection method: Focused `dotnet test` failed at compile time in `ProductExperienceTests.cs`.
- Fix: Removed the `StringComparison` argument, reran the focused test, and observed the intended red on `Installed on D drive`.
- Prevention rule: For TDD red tests, compile errors are not valid red; fix assertion API mistakes until the failure comes from missing behavior.
- Skill candidate: no

### 2026-07-01 - PowerShell regex command had broken quoting

- Symptom: A search for remaining English UI strings failed with `TerminatorExpectedAtEndOfString`.
- Wrong assumption: Escaping a quote inside a double-quoted PowerShell argument would stay harmless inside the larger command string.
- Root cause: The ad-hoc `rg` pattern mixed shell quoting and an embedded quote fragment, leaving PowerShell with an unterminated string.
- Detection method: PowerShell parser error before `rg` ran.
- Fix: Re-ran the search with a single-quoted pattern and removed the embedded quote requirement.
- Prevention rule: Use single-quoted PowerShell strings for `rg` alternation patterns unless interpolation is required.
- Skill candidate: no

### 2026-07-01 - GUI screenshot captured the wrong foreground window

- Symptom: `.omx\qa-app-tile-accessibility-names.png` showed another foreground application even though UIAutomation had verified OMNIX-Entropy app tile names.
- Wrong assumption: Capturing the primary screen after UIAutomation would necessarily show the tested WPF window.
- Root cause: The tested window was accessible to UIAutomation but was not foreground when the screenshot was taken.
- Detection method: Visual inspection of the screenshot showed a different app; command output still listed 130 OMNIX list items and readable tile names.
- Fix: Treat the command output, not that screenshot, as the evidence for this slice.
- Prevention rule: For GUI visual evidence, explicitly bring the WPF window to foreground before `CopyFromScreen`, or avoid citing the screenshot as visual proof.
- Skill candidate: no

### 2026-07-01 - Localized app search placeholder hid all scanned apps

- Symptom: GUI scan reported 391 apps, but the app grid showed 0 items and the drawer said no matching apps.
- Wrong assumption: The search placeholder handling covered the visible placeholder text.
- Root cause: `AppCatalogPresenter.MatchesSearch` only ignored the English placeholder `Search apps`, while WPF initialized the box with Chinese `搜索应用`.
- Detection method: GUI screenshot `.omx\qa-migration-scan-state.png` showed the scan count and empty grid; a failing test `App_catalog_ignores_localized_search_placeholder` reproduced it.
- Fix: Treat `搜索应用` and `搜索软件` as placeholder/empty search terms, then rerun product tests and GUI smoke.
- Prevention rule: Any visible placeholder used as a real `TextBox.Text` value must have a product test proving it does not affect filtering.
- Skill candidate: no

### 2026-07-01 - UIAutomation window detection was too narrow

- Symptom: A GUI smoke test reported `migration plan window not found` even though the screenshot showed the modal window was open.
- Wrong assumption: All WPF windows for the process would be returned as root children matching the process id condition.
- Root cause: The modal window was visible but not discovered by that narrow root-child query; searching descendants for the specific control was more reliable.
- Detection method: Screenshot `.omx\qa-after-move-click.png` showed the `Migration Plan` window while the script output listed only the main window.
- Fix: Use a broader root-descendant search for the `Create rollback manifest` button in the follow-up GUI verification.
- Prevention rule: For WPF modal verification, capture a screenshot on failure and search for stable control names, not only process root child windows.
- Skill candidate: no

### 2026-07-07 - Residue review GUI smoke depended on a slow full app rescan

- Symptom: GUI verification of `卸载后检查残留` did not produce a timely, clear result and a later GUI command was interrupted before evidence was captured.
- Wrong assumption: It was acceptable for a common "app still installed" residue-review click to perform another full software inventory scan before giving feedback.
- Root cause: The UI path lacked a fast decision based on the current app inventory, so the user-facing state could stay ambiguous while scanning.
- Detection method: GUI smoke waited for a residue-review modal/inline result and did not find one quickly; a subsequent run was interrupted. A process check found no remaining `Css.App` process after cleanup.
- Fix: Added `UninstallResidueReviewPlanner.TryBuildStillInstalledReport(...)` and inline drawer presentation for the still-installed case.
- Prevention rule: Common no-op safety decisions should be resolved from already-loaded evidence before starting expensive scans or external probes.
- Skill candidate: no

### 2026-07-07 - UIA `New-Object -ArgumentList` still misbound ControlType

- Symptom: The first C-drive GUI smoke completed the scan path but failed while creating a `ControlType.ListItem` `PropertyCondition`.
- Wrong assumption: Using `New-Object ... -ArgumentList` was enough to avoid the previous UIAutomation overload-binding bug.
- Root cause: PowerShell can still pass the control type value in a shape that UIAutomation rejects, even when `-ArgumentList` is present.
- Detection method: The GUI command failed with "PropertyCondition value for property 'AutomationElementIdentifiers.ControlTypeProperty' must be 'ControlType'."
- Fix: Rewrote the script to use direct constructors such as `[System.Windows.Automation.PropertyCondition]::new(property, value)` and reran the C-drive GUI verification successfully.
- Prevention rule: Future PowerShell UIA scripts should prefer direct .NET constructors for `PropertyCondition`/`AndCondition` and avoid `New-Object` for UIA conditions.
- Skill candidate: yes

### 2026-07-07 - Repeated unsafe UIA PropertyCondition construction

- Symptom: The first homepage button GUI smoke failed before reaching the app because `PropertyCondition` construction treated the control type value incorrectly.
- Wrong assumption: Inline `New-Object PropertyCondition (...)` syntax was safe enough for a new UIA script.
- Root cause: PowerShell overload binding is fragile for UIAutomation conditions; this exact issue had already appeared earlier in the project.
- Detection method: The GUI command failed with "PropertyCondition value ... must be 'ControlType'."
- Fix: Rewrote the script to use explicit `-ArgumentList` for `PropertyCondition` and `AndCondition`, then reran the GUI verification successfully.
- Prevention rule: All future PowerShell UIA scripts in this repo must construct conditions with explicit `-ArgumentList` variables, not inline constructor syntax.
- Skill candidate: yes

### 2026-07-07 - Mojibake block prevented safe growth-list cleanup

- Symptom: The old raw-path `GrowthListBox.ItemsSource` assignment could not be safely removed with a targeted patch.
- Wrong assumption: The localized surrounding block would be stable enough to patch directly after adding the presenter.
- Root cause: Historical mojibake/localized text around the assignment made exact patch context fragile.
- Detection method: Direct patch attempts failed to find the expected block.
- Fix: Added the new `GrowthFindingPresenter.CreateList(...)` assignment after the old block so runtime behavior used the beginner-facing presenter, then removed the old raw-path assignment once a stable ASCII anchor was found.
- Prevention rule: For mojibake-affected WPF code-behind, prefer ASCII anchors or smaller refactors; avoid relying on localized text as deletion context.
- Skill candidate: no

### 2026-07-07 - Mojibake block prevented safe dead-code removal

- Symptom: An `apply_patch` attempt to remove the now-unused private `RecommendationCardView` from `MainWindow.xaml.cs` failed to match the copied context.
- Wrong assumption: The mojibake text shown by `Get-Content` could be copied back into `apply_patch` as exact context.
- Root cause: The source contains historical localized/mojibake text that does not round-trip reliably through the terminal output, so the patch context differed from the actual file content.
- Detection method: `apply_patch` reported `Failed to find expected lines`; no file changes were applied.
- Fix: Left the unused private class in place and verified the active UI path uses `RecommendationCardPresenter`/`RecommendationCardViewModel` instead.
- Prevention rule: Do not perform large deletions across mojibake string blocks unless using stable ASCII boundaries or a dedicated encoding-safe cleanup pass.
- Skill candidate: no

### 2026-07-07 - Uninstall modal GUI smoke had UIA harness mistakes

- Symptom: The first uninstall-modal GUI smoke failed before business verification with `PropertyCondition value ... must be 'ControlType'`; the second found the app and button but reported `uninstall modal not found`; a third attempt failed by invoking a disabled modal button.
- Wrong assumption: Inline `New-Object PropertyCondition` argument binding was reliable, WPF modal windows would appear as process root children, and the first modal button would be a safe close button.
- Root cause: PowerShell constructor binding needed explicit `-ArgumentList`; this WPF dialog was discoverable by its `AutomationProperties.Name` as a root descendant rather than a process root child; not every button in the modal was enabled/actionable.
- Detection method: Follow-up diagnostic UIA script logged 130 app items, selected `火绒安全软件`, saw the uninstall button enabled, and confirmed `modalByName=True` while root child windows stayed at 1.
- Fix: Rewrote the script to use `AutomationId` for main controls, find the modal by `卸载安全方案窗口`, read text without invoking modal buttons, and close the process in `finally`.
- Prevention rule: For WPF modal smoke tests, use `AutomationId` for main-window controls, use modal `AutomationProperties.Name` or stable child controls for dialog lookup, and do not invoke arbitrary buttons during text verification.
- Skill candidate: yes

### 2026-07-07 - UIAutomation migration text scan was scoped too broadly

- Symptom: GUI smoke reported old English `Preview only` was still visible after migration-window production code and tests were localized.
- Wrong assumption: Searching all root descendants was a valid proxy for text visible inside the migration plan window.
- Root cause: The root UIAutomation tree can include unrelated windows or hidden/off-path controls from the app process; the broad search was not scoped to the modal window under test.
- Detection method: A follow-up script located the parent `ControlType.Window` of `CreateRollbackManifestButton` and found no `Preview only` text inside that migration window.
- Fix: Re-ran GUIA with all required/forbidden text checks scoped to the migration plan window itself; the scoped check passed.
- Prevention rule: For modal-window text QA, first identify the modal window from a stable child control, then search only that window's descendants.
- Skill candidate: yes

## 2026-07-07 - Residue planner file path assumption

- Symptom: `Get-Content src\Css.Core\Apps\UninstallResidueReviewPlanner.cs` failed because the file did not exist.
- Wrong assumption: Residue-review planner lived under `Css.Core.Apps`.
- Root cause: Similar presenter classes are split across `Apps` and `Software`, and I guessed the namespace before following the `rg` result.
- Detection method: PowerShell returned `ItemNotFoundException`.
- Fix: Used `rg` result and read `src\Css.Core\Software\UninstallResidueReviewPlanner.cs`.
- Prevention rule: For files discovered by `rg`, open the exact returned path instead of inferring a neighboring namespace.
- Skill candidate: no

## 2026-07-07 - Refresh reset residue inline result

- Symptom: GUI verification clicked `卸载后检查残留`; status text changed, but the drawer stayed on normal uninstall/migration preview instead of showing `残留检查结果`.
- Wrong assumption: Calling `RefreshAppCatalog()` after showing an inline review would preserve the selected drawer state.
- Root cause: `RefreshAppCatalog()` reselected an app and called `ShowAppDrawer`, replacing the just-rendered residue review.
- Detection method: GUI screenshot `.omx\qa-residue-review-inline-still-installed.png` plus UIA result `HasResultTitle=False`.
- Fix: Removed `RefreshAppCatalog()` from the cached still-installed branch where app inventory has not changed.
- Prevention rule: Do not refresh a selection-driven drawer after writing an inline result unless the result is restored after refresh; add a regression test for branch order.
- Skill candidate: yes

## 2026-07-07 - Residue result hidden below unrelated preview

- Symptom: UIA found `残留检查结果`, but the screenshot did not show it in the first app-drawer viewport.
- Wrong assumption: Any visible drawer list is enough if UIAutomation can find the text.
- Root cause: Uninstall/residue result was below migration preview, so the first viewport still emphasized a different workflow.
- Detection method: Screenshot `.omx\qa-residue-review-inline-fixed.png`.
- Fix: Moved uninstall/residue result above migration preview and added a test for XAML order.
- Prevention rule: UI for the action the user just clicked must appear before unrelated previews in the same drawer.
- Skill candidate: no
### 2026-07-07 - Drawer uninstall button UIA Invoke did not open modal

- Symptom: GUI smoke selected `火绒安全软件`, found `DrawerUninstallButton` enabled, invoked it with UIAutomation `InvokePattern`, but no uninstall safety modal opened and status text stayed on app-scan completion.
- Wrong assumption: `InvokePattern.Invoke()` on the WPF drawer button would behave exactly like a user click for this app-drawer action.
- Root cause: Not fully resolved. The diagnostic run proves the button was enabled and selected, but the UIA invoke path did not reach the `PreviewUninstall_Click` visible effect. A real mouse-click fallback was planned but could not run due usage-limit approval rejection.
- Detection method: Diagnostic UIA output showed `SelectedApp = 火绒安全软件, 需关注`, `ButtonEnabled = True`, `WindowCount = 1`, and screenshot `.omx\qa-uninstall-click-debug.png`.
- Fix: No product fix was applied because unit/build verification passed and the failed part was the GUI automation path. Record the limitation and use a real click helper for the next GUI pass when approvals are available.
- Prevention rule: For WPF app-drawer action smoke tests, verify button effects with a real clickable point or a known-good helper, and capture status/window diagnostics when `InvokePattern` reports success but no user-visible result appears.
- Skill candidate: yes
### 2026-07-07 - Mojibake-heavy handler made safe deletion too risky

- Symptom: `apply_patch` could not replace the old `RecommendationsListBox_SelectionChanged` body because the expected localized/mojibake lines did not match the source reliably.
- Wrong assumption: The handler block could be replaced directly using copied terminal output as context.
- Root cause: `MainWindow.xaml.cs` still contains historical mojibake UI strings; terminal-rendered text is not stable patch context.
- Detection method: `apply_patch` failed to find the expected handler body even though the method existed.
- Fix: Added a new concise `RecommendationsListBox_SelectionChanged` that uses `RecommendationSelectionPresenter`, renamed the old implementation to `RecommendationsListBox_SelectionChangedLegacy`, and kept it unreferenced until a safer code-behind cleanup can delete it.
- Prevention rule: For mojibake-heavy WPF code-behind, prefer stable ASCII method names/AutomationIds as patch anchors. Remove old localized blocks only during a deliberate UTF-8/code-behind cleanup pass with full tests and build.
- Skill candidate: no

### 2026-07-08 - Localized XAML anchor failed while adding Agent panel

- Symptom: `apply_patch` failed when inserting the Agent next-step panel after a localized `TextBlock` in `MainWindow.xaml`.
- Wrong assumption: The terminal-rendered localized Agent description was stable enough to use as patch context.
- Root cause: The XAML still contains historical mojibake in several visible strings; copied terminal output did not match the file bytes.
- Detection method: `apply_patch` reported `Failed to find expected lines`; a smaller patch anchored on the ASCII `Text="Computer Agent"` line succeeded.
- Fix: Re-ran the change using the stable ASCII `Computer Agent` title line and XAML numeric character references for new Chinese labels.
- Prevention rule: For existing localized XAML, anchor edits on `x:Name`, stable ASCII labels, or structural tags, and express new Chinese with XML numeric references when editing through patches.
- Skill candidate: no

### 2026-07-08 - Agent legacy XAML copy could not be safely deleted

- Symptom: After adding clean Agent identity copy near the top of the Agent card, attempts to remove the older mojibake identity lines failed with `apply_patch` context mismatches.
- Wrong assumption: The line text shown by `Get-Content` would match the file bytes closely enough for a delete patch.
- Root cause: Historical encoding damage in `MainWindow.xaml` makes terminal-rendered localized text unreliable as patch context.
- Detection method: `apply_patch` could not find the expected old lines even when copied from terminal output.
- Fix: Kept the compiled XAML intact and recorded the duplicate legacy copy as a known cleanup item rather than using a risky rewrite.
- Prevention rule: Remove mojibake-heavy XAML only during a dedicated UTF-8 cleanup pass, preferably by replacing a whole named region with stable ASCII/XAML-reference content and immediately running product tests plus build.
- Skill candidate: no
## 2026-07-08 - XAML/code-behind patch failed on mojibake context

- Symptom: `apply_patch` failed when matching long XAML/code-behind snippets containing older mojibake Chinese text.
- Wrong assumption: A nearby visible snippet copied from terminal output would match the file reliably.
- Root cause: Older localized text contains encoding artifacts, so long context around those strings is fragile.
- Detection method: `apply_patch` reported "Failed to find expected lines"; shorter `Get-Content` inspection showed stable control names nearby.
- Fix: Retried patches with stable ASCII/XAML control-name anchors such as `DrawerCleanCacheButton`, `DrawerDisableStartupButton`, and `DrawerMigrationSummaryTextBlock`.
- Prevention rule: For this repo, patch around stable x:Name/method/property anchors and avoid long contexts containing mojibake text.
- Skill candidate: yes

## 2026-07-08 - GUI smoke script broke on raw Chinese and PowerShell 5 API gaps

- Symptom: `.omx/gui-app-drawer-preview-smoke.ps1` first failed to parse after raw Chinese UI text was corrupted, then failed because Windows PowerShell 5 did not support `string.Contains(value, StringComparison)`, then failed because the scan button was named `scan app` rather than `scan software`.
- Wrong assumption: A verification script could safely embed raw Chinese strings and use newer .NET string APIs under Windows PowerShell.
- Root cause: Mixed console/source encodings and Windows PowerShell 5 compatibility limitations.
- Detection method: escalated GUI smoke command returned parser and method-overload errors; subsequent screenshot showed the actual app button label.
- Fix: Rebuilt the script with ASCII-only string literals using `[char]0x...` UI text construction, replaced `Contains(..., StringComparison)` with `IndexOf(..., StringComparison)`, and added a `scan app` fallback.
- Prevention rule: Repository QA scripts that match localized UI should use ASCII-safe code point construction and Windows PowerShell 5-compatible APIs.
- Skill candidate: yes

## 2026-07-08 - Real GUI smoke exposed missing production cache evidence

- Symptom: After a successful real app scan, the GUI smoke could not make the cache preview panel visible for any scanned app.
- Wrong assumption: Existing software profiles would already contain enough `CachePaths` or `CacheSizeBytes` for real apps.
- Root cause: `SoftwareInventoryBuilder` did not infer AppData cache/log candidates, and `SoftwareInventoryScanner` did not pass user-data roots.
- Detection method: GUI screenshot showed selected app drawer buttons disabled; script reported `Cache preview panel did not become visible`.
- Fix: Added AppData cache/log candidate inference and wired LocalAppData/Roaming/LocalLow roots into the scanner. Rerun GUI smoke passed with `cachePreviewVisible=True` and `startupPreviewVisible=True`.
- Prevention rule: UI actions that depend on profile evidence need at least one real-machine GUI smoke or real-machine test path, not only synthetic profiles.
- Skill candidate: yes

## 2026-07-08 - Static product test coupled to code-behind implementation

- Symptom: `ProductExperienceTests` failed after moving drawer preview status logic into `AppDrawerActionPreviewPresenter`.
- Wrong assumption: A static test should keep requiring `MainWindow.xaml.cs` to contain `CacheCleanupCanExecuteDirectly` and `StartupControlCanExecuteDirectly`.
- Root cause: The test checked an implementation detail instead of the product contract that WPF invokes a tested presenter and does not execute system changes.
- Detection method: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` failed two static assertions.
- Fix: Updated the tests to assert `AppDrawerActionPreviewPresenter.ShowCacheCleanup`, `ShowStartupControl`, and `ApplyDrawerActionPreviewState`.
- Prevention rule: Static UI tests should assert stable contracts, safety boundaries, and presenter wiring, not force product language or safety decisions to stay in code-behind.
- Skill candidate: no

## 2026-07-08 - Technical-details button naming patch failed on localized XAML

- Symptom: `apply_patch` could not add `x:Name` to the drawer technical-details button.
- Wrong assumption: The terminal-rendered button line around `ToggleTechnicalDetails_Click` was stable enough patch context.
- Root cause: The line contains historical mojibake text; copied terminal output did not match file bytes.
- Detection method: `apply_patch` reported `Failed to find expected lines`.
- Fix: Avoided touching that fragile XAML line. The handler now updates the clicked button through `sender as Button`, and the test asserts stable click-handler wiring rather than an `x:Name`.
- Prevention rule: Do not require new `x:Name` attributes on mojibake-heavy XAML lines unless replacing a wider stable region. Prefer handler sender or stable surrounding controls when behavior does not need a named element.
- Skill candidate: no

## 2026-07-08 - No-selection uninstall host inserted into wrong handler

- Symptom: `NoSelectionForUninstall` appeared in the cache-cleanup no-selection branch, while the uninstall branch still only set a direct status text.
- Wrong assumption: A patch anchored near a generic `return;` would apply to the intended uninstall handler.
- Root cause: Several app-drawer no-selection branches have similar structure, and one earlier patch matched the cache branch instead.
- Detection method: Final `rg` sanity check showed `NoSelectionForUninstall` near `PreviewCacheCleanup_Click`; the new `App_drawer_action_host_no_selection_wiring_matches_each_button` test failed on the missing uninstall wiring.
- Fix: Added the host call to `PreviewUninstall_Click`, removed it from `PreviewCacheCleanup_Click`, and added a static regression test that slices each handler separately.
- Prevention rule: When patching repeated WPF handler shapes, add or run a handler-specific static wiring test before relying on generic text search.
- Skill candidate: no

## 2026-07-08 - AgentSkillView replacement patch failed on mojibake block

- Symptom: `apply_patch` failed when replacing the whole `AgentSkillView` private class block in `MainWindow.xaml.cs`.
- Wrong assumption: The terminal-rendered mojibake strings in that block were stable enough for a full-block patch.
- Root cause: Historical localized text corruption makes copied terminal context unreliable, especially for long patches.
- Detection method: `apply_patch` reported `Failed to find expected lines`.
- Fix: Retried as smaller patches anchored on stable property and method signatures, then left unused old helper methods in place instead of risking a broad rewrite.
- Prevention rule: For mojibake-heavy code-behind/XAML, patch stable symbols in small chunks or replace a whole named region during a dedicated cleanup slice with tests.
- Skill candidate: no

## 2026-07-08 - GUI Agent tools smoke failed on PropertyCondition constructor

- Symptom: `.omx/gui-agent-system-tools-smoke.ps1` failed before verifying the app with `PropertyCondition value for ... ControlTypeProperty must be 'ControlType'`.
- Wrong assumption: Positional `New-Object System.Windows.Automation.PropertyCondition` arguments would bind reliably in Windows PowerShell.
- Root cause: Windows PowerShell interpreted the constructor arguments incorrectly without explicit `-ArgumentList`.
- Detection method: Escalated GUI smoke returned a constructor invocation exception at the button condition line.
- Fix: Updated every UIAutomation `PropertyCondition` construction in the script to use `New-Object ... -ArgumentList`.
- Prevention rule: UIAutomation smoke scripts should construct `PropertyCondition` with explicit `-ArgumentList`, especially for `ControlTypeProperty`.
- Skill candidate: yes

## 2026-07-08 - Agent background GUI smoke called function before definition

- Symptom: `.omx/gui-agent-background-review-smoke.ps1` failed with `Wait-Until : The term 'Wait-Until' is not recognized`.
- Wrong assumption: Windows PowerShell would make a function declared at the bottom of the script available before the call site in this execution path.
- Root cause: The helper was appended after the main script body instead of being declared with the other helpers before use.
- Detection method: Escalated GUI smoke returned a command-not-found error at the first `Wait-Until` call.
- Fix: Moved `Wait-Until` above the main script body and reran the smoke.
- Prevention rule: Put all PowerShell helper functions before executable script steps in `.omx` GUI smoke scripts.
- Skill candidate: yes

## 2026-07-08 - Agent background review was below the first visible area

- Symptom: GUI smoke found the Agent background review elements, but the screenshot showed the useful panel was below the visible first screen of the Agent card.
- Wrong assumption: Adding the summary under the existing Agent safety/privacy text would still be usable.
- Root cause: The Agent left card was already dense; placing a new panel at the bottom made it effectively hidden.
- Detection method: Visual inspection of `.omx/qa-agent-background-review.png` after a real app scan.
- Fix: Moved `AgentBackgroundReviewPanel` directly below the Agent next-step summary and before the reasons list; added a static order assertion so the panel stays above `AgentNextStepReasonsListBox`.
- Prevention rule: For beginner-facing Agent summaries, verify first-screen placement with a screenshot and protect the order with a static test when possible.
- Skill candidate: yes

## 2026-07-08 - Agent startup/service plan preview was too low in the first screen

- Symptom: GUI smoke passed control discovery, but screenshot review showed the plan preview was only partly visible near the bottom of the Agent left card.
- Wrong assumption: Placing the plan under the detailed resident-app list would still be understandable because UIAutomation could find it.
- Root cause: The Agent card already contains identity copy, next-step advice, background summary, and a scrollable app list; putting the plan after the list made the core recommendation compete with detail.
- Detection method: Visual inspection of `.omx/qa-agent-startup-service-plan.png` after a real app scan.
- Fix: Added a static order test requiring `AgentStartupServicePlanPanel` before `AgentBackgroundReviewItemsListBox`, then moved the plan preview above the detailed app list and reran GUI smoke.
- Prevention rule: Agent pages should show summary and plan before detailed lists; "visible to UIAutomation" is not enough for beginner-facing conclusions.
- Skill candidate: yes

## 2026-07-08 - PowerShell GUI smoke failed on raw Chinese string matching

- Symptom: `.omx/gui-agent-background-review-smoke.ps1` failed even though the Agent plan summary contained `这里只生成方案`.
- Wrong assumption: A raw Chinese string literal in a Windows PowerShell 5 script would reliably match UIAutomation text after patching.
- Root cause: Script-file encoding and localized string handling made the `*只生成方案*` wildcard pattern unreliable.
- Detection method: Escalated GUI smoke threw `Agent startup/service plan summary did not state preview-only behavior` while printing the correct Chinese text from the UI.
- Fix: Built the expected phrase from Unicode code points with `[char[]]` and used `.Contains(...)`.
- Prevention rule: `.omx` GUI smoke scripts should avoid raw localized string literals for assertions; construct localized phrases from code points or assert stable AutomationIds/state where possible.
- Skill candidate: yes

## 2026-07-08 - ScrollViewer patch landed in the wrong XAML region

- Symptom: `dotnet build ComputerSecuritySoftware.slnx --no-restore` failed with `MC3000` because a `ScrollViewer` start tag did not match the `Border` end tag.
- Wrong assumption: A generic `Border Grid.Column="1"` anchor was specific enough for the Agent right capability card.
- Root cause: `MainWindow.xaml` has several similar two-column page sections. The patch inserted an Agent-only `ScrollViewer` into the C-drive recommendation column and left a stray closing tag near the Agent left card.
- Detection method: XAML compiler error with line numbers during solution build.
- Fix: Removed the misplaced tags and re-applied the `AgentCapabilityScrollViewer` around the Agent right-card skill/settings/system-tools stack using nearby Agent-specific anchors.
- Prevention rule: In repeated WPF layouts, anchor patches on unique control names such as `AgentPage`, `AgentSkillListBox`, or `AgentWindowsSettingsListBox`, not only shared layout shapes.
- Skill candidate: no

## 2026-07-08 - Parallel dotnet build and test collided on compiler output

- Symptom: A parallel `dotnet test` failed with `CS2012` because `Css.Core.dll` in `obj\Debug` was locked by `VBCSCompiler`, while the parallel solution build succeeded.
- Wrong assumption: Independent `dotnet build` and filtered `dotnet test` commands could safely run in parallel in this Windows workspace.
- Root cause: Both commands wrote to the same project output/obj paths at the same time.
- Detection method: Shell output reported the locked `Css.Core.dll` and process `VBCSCompiler`.
- Fix: Reran the tests sequentially after the build; they passed.
- Prevention rule: Do not parallelize `dotnet build` and `dotnet test` for this solution unless isolated output paths are configured.
- Skill candidate: yes

## 2026-07-08 - MessageBox smoke searched only top-level windows

- Symptom: The settings confirmation smoke clicked the Storage shortcut but initially reported that no confirmation dialog appeared.
- Wrong assumption: WPF `MessageBox.Show(this, ...)` would always appear as a direct child of `AutomationElement.RootElement`.
- Root cause: The script searched only root children for process windows; the dialog was discoverable through descendant window search.
- Detection method: After broadening the search to root descendants, the same smoke captured `.omx/qa-agent-settings-confirm-cancel.png`.
- Fix: Changed `Find-WindowForProcess` to search root descendants and exclude the main window by native handle.
- Prevention rule: UIAutomation dialog smokes should search descendant windows for the launched process and exclude the known main window handle, not rely on direct root children only.
- Skill candidate: yes

## 2026-07-08 - Medium-risk settings were buried below other Agent capabilities

- Symptom: The Storage settings button existed but was partially hidden near the bottom of the Agent right card, making reliable click verification difficult.
- Wrong assumption: A nested settings ListBox scrollbar would be enough for usability and testing.
- Root cause: Settings appeared below skill catalog and system tools, while the right card had multiple scrollable regions and the primary Storage action was not first-screen accessible.
- Detection method: Visual inspection of `.omx/qa-agent-settings-before-click.png` and repeated smoke failures with button rectangles near the lower window edge.
- Fix: Moved Windows Settings direct links to the top of the Agent right card, ordered Storage/Installed Apps/Power first, and added a scrollable right-card container.
- Prevention rule: Primary safety-gated actions should not be nested below multiple scrollable sections; put the common action and its confirmation gate in the first visible area.
- Skill candidate: yes

## 2026-07-08 - App drawer smoke relied on a WPF Border AutomationId

- Symptom: The four-button app drawer smoke found the expected preview title after clicking `DrawerUninstallButton`, then failed because `DrawerActionPreviewPanel` could not be found.
- Wrong assumption: Setting `AutomationProperties.AutomationId` on a decorative `Border` would make the host reliably discoverable through UIAutomation.
- Root cause: WPF layout/decorative elements do not always expose stable automation peers, even when their child text/list controls are discoverable.
- Detection method: The script found `DrawerActionPreviewTitleTextBlock` with the expected title, but `Find-ControlByAutomationId` returned null for the `Border` host.
- Fix: Removed the host-container assertion from the product test and smoke; the smoke now verifies exposed title/list controls and button AutomationIds.
- Prevention rule: In WPF GUI smokes, target interactive or readable controls such as buttons, text blocks, and list boxes; do not use `Border`/layout containers as the only evidence.
- Skill candidate: yes

## 2026-07-08 - App drawer smoke assumed one selected app enabled every action

- Symptom: The upgraded app drawer smoke failed with `Action button was disabled: DrawerMigrateButton`.
- Wrong assumption: The first selectable app tile would support uninstall, migration, cache, and startup previews.
- Root cause: Migration can be correctly disabled for apps already installed reasonably on D drive or otherwise not suitable for migration planning.
- Detection method: The script selected the first app and then found `DrawerMigrateButton` disabled while other drawer actions were available.
- Fix: Changed the smoke to select an eligible scanned app separately for each action before invoking the button.
- Prevention rule: GUI smokes for conditional actions should search for data satisfying each action's preconditions rather than forcing one selected item to cover all states.
- Skill candidate: yes

## 2026-07-08 - App drawer action card was below the visible drawer area

- Symptom: GUI smoke passed after adding action-card fields, but screenshot review showed only the preview header near the bottom of the right drawer; the useful Agent text was mostly below the visible area.
- Wrong assumption: Adding clearer fields above the list would improve comprehension without changing drawer scrolling behavior.
- Root cause: The right drawer already contained title, location, size, residency, advice, buttons, and the preview host; adding fields pushed the action card below the fold.
- Detection method: Visual inspection of `.omx/qa-app-drawer-action-previews.png` after the first action-card implementation.
- Fix: Added `AppDrawerScrollViewer` and called `DrawerActionPreviewPanel.BringIntoView()` after applying a visible action host state.
- Prevention rule: When adding content below action buttons, run a real screenshot and make the clicked feedback move into view; UI tree presence is not enough.
- Skill candidate: yes

## 2026-07-08 - GUI smoke used stale WPF executable after XAML changes

- Symptom: The enhanced app-drawer smoke could not find newly added action-card field AutomationIds.
- Wrong assumption: Running `dotnet test` after XAML/code changes was enough to update the WPF executable used by `.omx/gui-app-drawer-preview-smoke.ps1`.
- Root cause: The smoke launches `src\Css.App\bin\Debug\net8.0-windows\Css.App.exe`; tests rebuilt test dependencies but did not produce a fresh `Css.App.exe`.
- Detection method: The smoke failed on missing new AutomationIds until the solution build was run.
- Fix: Ran `dotnet build ComputerSecuritySoftware.slnx --no-restore` before rerunning the GUI smoke.
- Prevention rule: After WPF `MainWindow.xaml` or app code-behind changes, build the solution before any GUI smoke that launches `Css.App.exe`.
- Skill candidate: yes

## 2026-07-08 - New required action-card fields missed a direct initializer

- Symptom: Solution build failed with `CS9035` for missing `AgentTakeaway`, `NextStepText`, and `SafetyText` in `MainWindow.xaml.cs`.
- Wrong assumption: All `AppDrawerActionHostViewModel` instances were created through `AppDrawerActionHostPresenter`.
- Root cause: `ShowResidueReviewInline(...)` directly constructed an `AppDrawerActionHostViewModel` for post-uninstall residue review.
- Detection method: `dotnet build ComputerSecuritySoftware.slnx --no-restore` reported the missing required members at the direct initializer line.
- Fix: Added residue-review Agent takeaway, next step, and safety text to the direct initializer.
- Prevention rule: When adding required members to shared view models, search for direct object initializers outside the main presenter and update them before GUI/build verification.
- Skill candidate: yes
## 2026-07-08 - Timeline XAML had malformed mojibake attributes

- Symptom: The undo-center AutomationId patch could not match Timeline XAML lines, and inspection showed title/button text had swallowed style/width attributes into malformed mojibake text.
- Wrong assumption: The visible XAML lines were normal text that could be patched with ordinary context.
- Root cause: Historical encoding corruption left non-ASCII/mojibake characters in attribute values, making literal patch matching fragile and the UI harder to inspect.
- Detection method: Focused test failed on missing `TimelineTitleTextBlock`; line-number and character-code inspection showed the Timeline title/load-button attributes were malformed.
- Fix: Rewrote the small `TimelinePage` block using ASCII-safe XML character references and added stable AutomationIds to text, list, and button controls.
- Prevention rule: For corrupted WPF/XAML blocks, inspect line numbers and character codes before patching; prefer rewriting the affected small block with XML character references and immediately run build plus UI hook tests.
- Skill candidate: yes

### 2026-07-09 - Uninstall plan smoke searched only process top-level child windows

- Symptom: Real `.omx/gui-uninstall-plan-window-smoke.ps1` opened the app and invoked an enabled `DrawerUninstallButton`, but failed with `Uninstall plan window was not found`.
- Wrong assumption: The WPF uninstall plan modal would always appear as a direct root child window for the launched process.
- Root cause: This modal can be missed by process top-level child enumeration, while its stable child control `UninstallPlanTitleTextBlock` is discoverable through root-descendant UIAutomation.
- Detection method: The real GUI smoke failed after app scan/button invoke; existing project error ledger showed a prior uninstall modal with the same root-child discovery problem.
- Fix: Added a red static test requiring descendant lookup, then implemented `Find-WindowByDescendantAutomationId` using `TreeScope.Descendants` and `TreeWalker.ControlViewWalker` to walk from the stable child control back to its owning window. Rerunning the real GUI smoke passed and produced `.omx\qa-uninstall-plan-window.png`.
- Prevention rule: For WPF modal smokes, do not rely only on process root-child windows. Use stable descendant AutomationIds or modal names, then scope assertions to the owning window.
- Skill candidate: yes

## 2026-07-09 - Full restore failed inside sandbox network

- Symptom: `dotnet restore ComputerSecuritySoftware.slnx` failed with `NU1301` because `api.nuget.org` could not be reached from the sandbox.
- Wrong assumption: Adding a no-package dev console project would allow restore to complete from existing local assets without network access.
- Root cause: Solution restore still checked NuGet sources and vulnerability metadata for all projects, and sandbox networking blocked that access.
- Detection method: Restore output showed socket access denied for `https://api.nuget.org/v3/index.json`.
- Fix: Re-ran the same restore command with approved escalation; it completed and generated assets for `Css.SmokeTools`.
- Prevention rule: When adding a new project, expect a restore step. If restore fails with sandbox network errors, rerun the same command with explicit escalation instead of trying to bypass dependency resolution.
- Skill candidate: yes

## 2026-07-09 - Settings confirm-cancel smoke could not reliably find the dialog

- Symptom: After migrating `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` to shared helpers, the real GUI smoke first failed with `RPC_E_SERVERFAULT` during root-descendant UIAutomation search, then failed with "Confirmation dialog was not shown" after that exception was caught.
- Wrong assumption: Searching `AutomationElement.RootElement` descendants was a reliable way to find the WPF `MessageBox` confirmation dialog.
- Root cause: Desktop-wide UIAutomation descendant searches can touch unrelated or transient providers that throw COM exceptions, and they can also miss the dialog when the provider faults. The smoke needed a narrower window-discovery path for top-level windows owned by the launched WPF process.
- Detection method: Reproduced the failure twice with the real GUI smoke, inspected the failure line in `Find-WindowForProcess`, reviewed the before-click screenshot showing the Storage button was visible, and confirmed source code still required confirmation for Storage.
- Fix: Added tests requiring protected root-window search and a native Win32 fallback; implemented `EnumWindows`/`GetWindowThreadProcessId` process-window enumeration and converted handles with `AutomationElement.FromHandle`.
- Prevention rule: GUI smokes that need secondary windows should avoid relying only on desktop-wide UIAutomation descendant searches; prefer direct child search, native process-owned top-level window enumeration, then protected descendant fallback.
- Skill candidate: yes

## 2026-07-09 - Broad XAML patch failed on mojibake context

- Symptom: The first `apply_patch` attempts for the C-drive recommendation preview failed to find expected XAML/code-behind lines.
- Wrong assumption: The visible copied context around localized WPF strings was stable enough for a broad multi-hunk patch.
- Root cause: Historical mojibake/non-ASCII corruption makes exact matching fragile around some attribute values and button text.
- Detection method: `apply_patch` verification failed; line-number inspection showed stable ASCII control names/attributes but unstable localized text.
- Fix: Re-applied smaller patches anchored on stable control names and ASCII attributes, then verified with focused tests and solution build.
- Prevention rule: In `MainWindow.xaml`, patch around unique `x:Name`/AutomationId/ASCII attribute lines rather than localized text. Prefer smaller hunks when the surrounding block contains mojibake.
- Skill candidate: yes

## 2026-07-09 - Undo-center screenshot exposed a long local path

- Symptom: The first seeded undo-center GUI screenshot showed the full `.omx` test source path in the timeline row.
- Wrong assumption: Proving the restore button was enabled was enough for the beginner-facing undo center.
- Root cause: `ActionTimelinePresenter` built first-level `Detail` by joining affected paths directly.
- Detection method: Visual inspection of `.omx/qa-undo-center.png` after the seeded smoke passed.
- Fix: Added a failing presenter test and changed the first-level detail to summarize `影响范围：N 个位置`.
- Prevention rule: For beginner-facing timeline/history rows, visual proof must check that path-heavy evidence is summarized by default and reserved for technical detail views.
- Skill candidate: yes

## 2026-07-09 - Temp rule changed before red test

- Symptom: The `rules.scan.json` `Temp` patterns were updated for top-level `Temp`/`tmp` before a focused failing test existed.
- Wrong assumption: The change was small enough to fold into the smoke implementation without its own red-green proof.
- Root cause: The GUI fixture design needed a scanner-rule change, and the rule was patched while thinking through the smoke path.
- Detection method: Manual protocol check before final verification noticed the production rule change had no dedicated red test.
- Fix: Reverted the rule lines, added `App_rules_classify_top_level_temp_roots_for_cleanup_fixture`, observed it fail for `C:\Temp`, then re-added the patterns and observed the test pass.
- Prevention rule: If a fixture requires a product behavior tweak, create the failing behavior test before changing the rule or code, even when the tweak is tiny.
- Skill candidate: no

## 2026-07-09 - C-drive scan-root patch failed on mojibake context

- Symptom: A broad `apply_patch` for `RunScanAsync` failed to find expected lines around progress/status text.
- Wrong assumption: The visible localized status string context could be used safely as a patch anchor.
- Root cause: Historical mojibake makes localized status lines unreliable for exact patch matching.
- Detection method: `apply_patch` verification failed, then line inspection showed the stable lines were the ASCII method calls.
- Fix: Re-applied the change as narrow line replacements anchored only on ASCII code lines: `rulesPath`, `LoadLatestAsync`, `_scanner.ScanAsync`, and `SaveAsync`.
- Prevention rule: For `MainWindow.xaml.cs`, avoid localized text as patch context; patch one or two ASCII code lines at a time.
- Skill candidate: no

## 2026-07-09 - PowerShell rg quote check failed

- Symptom: Two `rg` verification commands failed because PowerShell interpreted parts of the regex as commands/modules.
- Wrong assumption: Escaped quotes inside a double-quoted PowerShell command would pass through to `rg` as intended.
- Root cause: The shell parsed `\"` and alternation text unexpectedly before `rg` received the pattern.
- Detection method: PowerShell errors named `ExecuteRecommendationButton` and `CDriveNavButton` as unrecognized commands instead of returning `rg` matches.
- Fix: Re-ran simpler fixed-string `rg` checks for each AutomationId.
- Prevention rule: In PowerShell, use simple fixed-string `rg` checks or single-quoted patterns without embedded escaped quotes for verification.
- Skill candidate: no

## 2026-07-09 - Development record append needed shell fallback

- Symptom: `apply_patch` could not append to `error-ledger.md` using tail context from the prior mojibake-heavy section.
- Wrong assumption: The displayed tail text would match the file bytes exactly enough for patch context.
- Root cause: Existing mojibake/non-ASCII text near EOF made patch matching fragile.
- Detection method: `apply_patch` repeatedly failed to find expected lines even after line-number inspection.
- Fix: Used a narrow PowerShell `Add-Content` append for the development record only; no production/source code was edited this way.
- Prevention rule: Keep development record entries ASCII when possible, and consider periodically normalizing `.omx/development/*.md` encoding.
- Skill candidate: no

## 2026-07-09 - Terminal mojibake caused wrong production patch context

- Symptom: Removing `BuildResidueConfirmMessage` initially failed because the patch used mojibake text shown by `Get-Content`.
- Wrong assumption: The terminal-rendered string was the actual file text.
- Root cause: The file was valid UTF-8, but the PowerShell console rendered Chinese strings as mojibake in normal output.
- Detection method: `apply_patch` failed, then `[System.IO.File]::ReadAllLines(...) | ConvertTo-Json` showed the actual UTF-8 Chinese text.
- Fix: Re-ran the patch with the real UTF-8 text and removed the old helper successfully.
- Prevention rule: When localized code context appears mojibake in the terminal, inspect exact UTF-8 lines with a JSON/escaped representation before patching.
- Skill candidate: no

## 2026-07-09 - C-drive cleanup confirmation smoke missed WPF modal

- Symptom: `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1` failed with `Cleanup confirmation window was not found`.
- Wrong assumption: Process root-child window enumeration would reliably find the WPF confirmation modal.
- Root cause: Like the uninstall-plan modal, the confirmation window can be missed as a root child but is discoverable through a stable descendant AutomationId.
- Detection method: Real GUI smoke failed after opening the flow; comparison with the already-fixed uninstall-plan smoke showed the missing descendant fallback.
- Fix: Added a failing static test, used descendant AutomationId parent-window walking, then promoted the repeated modal-discovery code into `.omx/wpf-smoke-helpers.ps1`.
- Prevention rule: New WPF modal smokes should use shared `Find-SecondaryWindowWithChild` instead of local root-child-only lookup.
- Skill candidate: yes

## 2026-07-09 - Residue review cached-first logic blocked the confirmation path

- Symptom: A dedicated low-risk uninstall-residue confirmation GUI fixture could not reach the confirmation path from a freshly selected app.
- Wrong assumption: Cached `_softwareProfiles` could be used to quickly decide whether the selected app was still installed before doing any fresh scan.
- Root cause: The selected app always comes from `_softwareProfiles`, so cached-first still-installed logic always reports it as installed and blocks the post-uninstall residue review path.
- Detection method: Handler analysis while designing the fixture, then a red test requiring residue review to call `ScanSoftwareProfilesAsync()` before building the residue report.
- Fix: Routed app scan/snapshot/residue review through `ScanSoftwareProfilesAsync()`, removed the cached-first still-installed branch, and updated `_softwareProfiles` from the fresh scan before building the report.
- Prevention rule: Any destructive-adjacent flow that depends on external state changing outside OMNIX-Entropy must refresh that state before deciding whether cleanup, migration, or residue handling is allowed.
- Skill candidate: no

## 2026-07-09 - Residue outcome patch failed on localized context

- Symptom: The first `apply_patch` attempt to wire residue cancel/quarantine outcomes into `MainWindow.xaml.cs` failed to find the confirmation-cancel block.
- Wrong assumption: Terminal-rendered localized string context could be used as a reliable patch anchor.
- Root cause: The file contains valid UTF-8 strings that render as mojibake in the PowerShell output, making visible text context fragile.
- Detection method: `apply_patch` failed; a smaller patch anchored on ASCII control-flow lines succeeded.
- Fix: Patched around stable lines such as `RefreshAppCatalog();`, `QuarantineOperationPolicy.ConfirmForExecution(lowRiskOperation)`, and method boundaries instead of localized strings.
- Prevention rule: For WPF handlers in this repository, patch control flow around ASCII method calls and identifiers, not localized status text.
- Skill candidate: no

## 2026-07-09 - Install routing file search used the wrong project path

- Symptom: A read command failed for `src\Css.Core\Apps\InstallRouting.cs`.
- Wrong assumption: Install-routing code lived under `Css.Core.Apps`.
- Root cause: The implementation is actually in `src\Css.InstallGuard\Routing\InstallRoutingEngine.cs`.
- Detection method: `Get-Content` returned path-not-found, then `rg --files` located the correct routing file.
- Fix: Read and modified the correct `Css.InstallGuard\Routing` files.
- Prevention rule: For cross-project features, use `rg --files` before opening an assumed path.
- Skill candidate: no

## 2026-07-09 - Install analysis output patch briefly misplaced a method boundary

- Symptom: Adding route-source text initially left `InstallerAnalysisTextBlock.Text += ...` outside `AnalyzeInstaller_Click`.
- Wrong assumption: A broad patch around terminal-rendered Chinese output would preserve the method boundary.
- Root cause: The console displayed mojibake, and the patch was applied using fragile localized context.
- Detection method: Inspecting exact UTF-8 lines with `[System.IO.File]::ReadAllLines(...)` showed the misplaced brace and append line.
- Fix: Patched with the real UTF-8 lines and moved the append line back inside the handler.
- Prevention rule: For localized WPF handlers, inspect exact UTF-8 lines before patching string chains or method boundaries.
- Skill candidate: no

## 2026-07-09 - Install route remember handler patch used stale mojibake context

- Symptom: The first `apply_patch` attempt to replace the install-route memory `MessageBox` failed to find the expected block.
- Wrong assumption: The mojibake-looking context in the earlier large file read matched the actual source text.
- Root cause: The earlier output was truncated/misrendered, while the actual file contained valid UTF-8 Chinese strings.
- Detection method: `[System.IO.File]::ReadAllLines(...)` with line numbers showed the real block at lines 121-138.
- Fix: Re-ran the patch against the exact UTF-8 lines and replaced the `MessageBox` flow with the scope choice window.
- Prevention rule: Before patching localized WPF handler text, inspect a narrow line-number range from `ReadAllLines`; do not rely on prior truncated terminal output.
- Skill candidate: no

## 2026-07-09 - Product test used wrong handler end marker

- Symptom: `Install_guard_forget_learned_rule_only_edits_memory_after_confirmation` failed to extract `ForgetInstallRoutingRule_Click` after implementation.
- Wrong assumption: `CancelScan_Click` was declared `private async void`.
- Root cause: The actual handler is `private void CancelScan_Click`.
- Detection method: The test failed with end marker `-1` after the production handler existed.
- Fix: Updated the end marker to `private void CancelScan_Click`.
- Prevention rule: Before adding static method extraction around existing WPF handlers, verify the exact method signature with `rg -n` or a narrow line read.
- Skill candidate: no

## 2026-07-10 - Current-state patch used handoff context

- Symptom: The first patch for the install-report Agent slice failed to find the expected `Current Exact Next Recommended Action` block in `current.md`.
- Wrong assumption: A block seen in the earlier combined/truncated tool output belonged to the tail of `current.md`.
- Root cause: The combined output included similarly worded handoff content, while the authoritative `current.md` tail used an active-slice format.
- Detection method: `apply_patch` failed without changing files; a direct tail read showed the actual unique context.
- Fix: Patch `current.md` after the unique final line of the latest active slice and verify the diff before continuing.
- Prevention rule: When several protocol files contain repeated next-action text, read the exact target-file tail immediately before patching and anchor on a unique active-slice line.
- Skill candidate: no

## 2026-07-10 - Repeated localized WPF patch anchor failure

- Symptom: The first production patch for the install-report Agent panel failed to find the end of `BuildInstallDiff_Click`.
- Wrong assumption: A mojibake status-string line from a broad terminal read was safe enough as one anchor in a multi-file patch.
- Root cause: The source contains valid UTF-8 Chinese while the earlier terminal output misrendered it; this repeated a documented repository failure mode.
- Detection method: `apply_patch` rejected the full patch before changing any file; `ReadAllLines` showed the exact UTF-8 method body.
- Fix: Split XAML and C# edits, inspect exact line ranges with `ReadAllLines`, and anchor C# patches only on method names and ASCII identifiers.
- Prevention rule: Never include localized string literals in `MainWindow.xaml.cs` patch context; use identifiers, braces, and method boundaries only.
- Skill candidate: yes

## 2026-07-10 - GUI automation state did not prove the Agent panel visually

- Symptom: The install-diff GUI smoke returned `agentHeadlineVisible=true`, but `qa-install-diff-agent.png` did not visibly show the Agent explanation and contained large black screenshot regions.
- Wrong assumption: UIAutomation `IsOffscreen` plus a window-bounds screenshot was sufficient visual proof after the page layout expanded.
- Root cause: The scroll position was not forced to the new bottom after expansion, and maximized WPF window bounds produced unreliable screen-copy coordinates.
- Detection method: Manual inspection of both generated PNGs contradicted the smoke JSON.
- Fix: Require a dedicated bottom-scroll step after explanation, use full desktop screenshots, rerun the smoke, and inspect the new image before marking the visual gate pass.
- Prevention rule: A GUI smoke pass is not visual proof until the screenshot itself shows the target conclusion without clipping or capture artifacts.
- Skill candidate: yes
# 2026-07-10 - Handoff update landed before the file tail

- Symptom: The latest install-report Agent handoff section appeared around line 876 while the authoritative file tail still ended at the older post-install-card state.
- Wrong assumption: Matching a repeated `Current Exact Next Recommended Action` heading would target the final occurrence.
- Root cause: `handoff.md` intentionally contains many historical sections with the same heading, so the patch anchor was ambiguous.
- Detection method: `Get-Content .omx/development/handoff.md -Tail 45` and `rg -n` showed the new section was not at EOF.
- Fix: Append an explicitly labeled authoritative-tail update after the unique final post-install-card action block.
- Prevention rule: For append-only protocol files, anchor on a unique final update title plus its exact trailing block, then verify with `Get-Content -Tail`.
- Skill candidate: no; this is already covered by the protocol's evidence-first handoff discipline.
# 2026-07-10 - Action-plan test patch used a nonexistent doc assertion anchor

- Symptom: The combined test patch was rejected because `doc.Should().Contain("fixture-only")` was not present in `ProductExperienceTests.cs`.
- Wrong assumption: The current static GUI smoke test ended with the wording remembered from the docs instead of its actual `does not run an installer` assertion.
- Root cause: A large multi-file patch relied on conversation memory instead of a unique current-file anchor.
- Detection method: `apply_patch` verification failure followed by `rg` and a focused file read confirmed no changes had landed.
- Fix: Split the tests into smaller patches anchored on unique method boundaries from the current file.
- Prevention rule: Before multi-file patches, read the exact insertion boundary and prefer one unique method name per hunk.
- Skill candidate: no.
# 2026-07-10 - Repeated combined patch failed on stale documentation wording

- Symptom: A script-plus-doc patch was rejected because the expected documentation sentence used stale wording (`an installer`) instead of the current `an installer package` text.
- Wrong assumption: After already recording an anchor mismatch, it was still acceptable to combine an unverified documentation hunk with the functional script hunk.
- Root cause: The correction did not fully apply the newly stated small-patch prevention rule.
- Detection method: `apply_patch` rejected the entire patch; a focused read of `docs/development/gui-smokes.md` showed the exact current sentence and confirmed the script had not changed.
- Fix: Patch the smoke script independently, then patch the documentation against its exact current sentence.
- Prevention rule: After any patch-anchor failure in a turn, all remaining files must be patched separately using freshly read, unique anchors.
- Skill candidate: yes; this can become an `apply_patch` recovery checklist.

## 2026-07-10 - Localized GUI smoke assertion failed despite correct rendered text

- Symptom: The real WPF smoke found the plan safety TextBlock and printed text containing `尚未执行`, yet `.Contains('尚未执行')` returned false; the next implementation using `[string]::Concat(char...)` threw `ArgumentNullException` under Windows PowerShell 5.1.
- Wrong assumption: A visible CJK literal in a no-BOM UTF-8 `.ps1` file and a multi-char `String.Concat` call would behave consistently in Windows PowerShell 5.1.
- Root cause: The script source literal was decoded using the host code page while the WPF UIA value was real Unicode; PowerShell also bound the four-char `Concat` call to an unsuitable overload.
- Detection method: Diagnostic failure output printed the actual UIA name, proving product binding was correct. Minimal `powershell.exe -NoProfile` expressions reproduced `Concat` failure and showed `-join @([char]...)` succeeds.
- Fix: Construct the keyword with ASCII-only Unicode char codes and PowerShell's `-join` operator, then protect that form with a static product test.
- Prevention rule: Windows PowerShell 5.1 smoke scripts must not use localized source literals for assertions; prefer ASCII AutomationIds and `-join` over Unicode char-code arrays when text is unavoidable.
- Skill candidate: yes; add this to a reusable Windows UIA smoke reference.

## 2026-07-10 - Verification shell commands violated command-isolation guidance

- Symptom: The first nested PowerShell reproduction expanded `$x` in the outer shell and produced a misleading `= is not recognized` error; a later cleanup check bundled several commands with semicolons.
- Wrong assumption: Double-quoted `-Command` text and compact command grouping were harmless in a PowerShell-hosted tool call.
- Root cause: Outer-shell interpolation consumed `$x`, and command aggregation reduced output isolation contrary to repository agent guidance.
- Detection method: The malformed reproduction output lacked `$x`; reviewing the issued cleanup command showed multiple semicolon-separated checks.
- Fix: Reran the reproduction with a single-quoted inner command and interpreted cleanup output by section. No product files were affected.
- Prevention rule: Pass nested PowerShell scripts in single quotes when they contain `$` variables, and issue independent verification commands separately rather than joining them with separators.
- Skill candidate: no; this is existing execution guidance.

## 2026-07-10 - Transient black composition blocks invalidated the first classification screenshot

- Symptom: The first post-classification GUI smoke returned all expected UIA values, but the action-plan screenshot showed large black rectangles over the three item text areas.
- Wrong assumption: A successful smoke run after the earlier screenshot fix would always yield valid rendered evidence.
- Root cause: The desktop capture intermittently raced Windows/WPF composition; the same binary, script, data, and layout produced a clean image on an unchanged immediate rerun.
- Detection method: Mandatory manual screenshot inspection contradicted the JSON output. A controlled unchanged rerun did not reproduce the blocks and showed all content.
- Fix: Reject the bad artifact, rerun without product changes, inspect the replacement, and retain only the clean final screenshot as evidence.
- Prevention rule: Never accept the first GUI artifact solely because automation passed; inspect every retained screenshot and rerun unchanged once when capture corruption appears before diagnosing product layout.
- Skill candidate: yes; screenshot helpers could add composition flush/retry or black-region detection.

## 2026-07-10 - One evidence-classification edit ignored the one-file recovery rule

- Symptom: The successful WPF binding patch changed both `MainWindow.xaml` and `MainWindow.xaml.cs` after this turn had already established a one-file-at-a-time recovery rule.
- Wrong assumption: Because both anchors were exact and the change was small, combining two files was acceptable.
- Root cause: The prevention rule was treated as advice instead of a turn-scoped constraint.
- Detection method: Review of the issued `apply_patch` call during protocol recording.
- Fix: No code rollback was needed because the patch applied exactly and tests/GUI verified it; all subsequent record edits were applied one file at a time.
- Prevention rule: Once a recovery rule is declared for a turn, follow it literally even for low-risk adjacent files.
- Skill candidate: no; covered by the apply-patch recovery candidate.

## 2026-07-10 - Current-state record patch used a mojibake anchor

- Symptom: The first `current.md` patch failed to match the final “why did Agent judge this way?” line.
- Wrong assumption: The terminal-rendered mojibake text was identical to the UTF-8 file content.
- Root cause: Windows PowerShell displayed curly quotes through the wrong output encoding.
- Detection method: `apply_patch` rejected the anchor; `rg` then showed the actual UTF-8 line.
- Fix: Re-read the exact line with `rg` and applied a one-file patch using the real Unicode text.
- Prevention rule: When localized record output is mojibake, use an ASCII substring with `rg` to recover the exact source line before patching.
- Skill candidate: no; existing localized-anchor guidance covers this case.

## 2026-07-10 - Startup inspection chained PowerShell commands

- Symptom: One startup read combined `AGENTS.md`, `current.md`, and `handoff.md` with PowerShell semicolon separators.
- Wrong assumption: Grouping three read-only files into one shell result was acceptable because it did not mutate state.
- Root cause: Latency optimization overrode the repository interaction rule that shell commands should not be chained with separators.
- Detection method: Review of the issued startup command during protocol recording.
- Fix: Subsequent independent reads were issued as separate `shell_command` calls inside parallel orchestration.
- Prevention rule: Parallelize independent reads at the tool-call level; never use `;`, `&&`, or similar shell separators to bundle them.
- Skill candidate: no; already a global development rule.

## 2026-07-10 - Localized record anchor mistake repeated

- Symptom: The first reflection append repeated the same mojibake-anchor failure already recorded for `current.md`.
- Wrong assumption: Copying the terminal-rendered tail was still safe for a nearby protocol file.
- Root cause: The newly written prevention rule was not applied immediately.
- Detection method: `apply_patch` rejected the reflections anchor; `rg` again showed the correct Unicode line.
- Fix: Re-read with an ASCII `rg` query and patched the exact UTF-8 source line.
- Prevention rule: For every localized protocol-file append in this turn, query an ASCII substring with `rg` first; do not copy terminal-rendered localized anchors.
- Skill candidate: no; this reinforces the existing localized-anchor rule.

## 2026-07-10 - PowerShell `rg` used a Windows wildcard path

- Symptom: `rg -n ... .omx\*.ps1` failed with Windows error 123 and prevented the parallel diagnostic batch from returning the other reads.
- Wrong assumption: `rg` would expand a Windows wildcard path the same way a Unix shell expands globs.
- Root cause: PowerShell passed the literal invalid path to `rg`; `rg` expects a directory plus `-g` for this pattern.
- Detection method: The diagnostic command returned `IO error ... 文件名、目录名或卷标语法不正确`.
- Fix: Re-ran as `rg ... .omx -g "*.ps1"` and issued the related reads independently in parallel.
- Prevention rule: On Windows, pass a real directory to `rg` and use `-g` for file globs.
- Skill candidate: no; standard `rg` usage rule.

## 2026-07-10 - `WaitForInputIdle` did not eliminate focus failures

- Symptom: The install GUI smoke still failed at `SetFocus()` after adding `WaitForInputIdle(10000)`.
- Wrong assumption: An idle WPF message loop guarantees UIAutomation can focus the top-level window on the first attempt.
- Root cause: Windows foreground/focus readiness is separate and can remain transient after the process becomes input-idle.
- Detection method: The exact failure reproduced immediately after the first fix; an unchanged later run succeeded, proving intermittent focus readiness.
- Fix: Added `Focus-WindowWhenReady`, which polls the actual `SetFocus()` condition for a bounded interval and reports window state only on timeout.
- Prevention rule: For GUI smoke startup, wait for input idle and then poll the operation that must succeed; do not infer focus readiness from process readiness.
- Skill candidate: yes; move bounded WPF focus into the shared smoke helper.

## 2026-07-10 - Nested-list `IsOffscreen` contradicted the screenshot

- Symptom: Automation reported the eligible-actions list as visible, but the screenshot did not show the Agent candidate section.
- Wrong assumption: `AutomationElement.Current.IsOffscreen` reliably represents visible intersection for controls nested inside WPF list/scroll containers.
- Root cause: Nested WPF automation peers can report the control as onscreen when its rendered rectangle is below the outer scroll viewport.
- Detection method: Immediate screenshot inspection contradicted the UIAutomation state.
- Fix: Added `Test-ElementIntersectsViewport` and made scrolling compare actual element/viewport bounding rectangles at each scroll percentage.
- Prevention rule: For screenshot proof, verify geometric viewport intersection; treat `IsOffscreen` as insufficient for nested controls.
- Skill candidate: yes; generalize viewport-aware scrolling in `wpf-smoke-helpers.ps1`.

## 2026-07-10 - Backticks in a PowerShell `rg` query changed the search text

- Symptom: A quality-gate anchor query returned no matches even though the sentence existed.
- Wrong assumption: Markdown backticks inside the double-quoted PowerShell argument would be passed literally.
- Root cause: PowerShell treats backtick as its escape character, so the query text was altered before reaching `rg`.
- Detection method: Re-running with the ASCII substring `End-user packaging still needs` returned both expected lines.
- Fix: Used a shorter query without backticks and then read the exact tail before patching.
- Prevention rule: Do not place Markdown backticks in double-quoted PowerShell command arguments; search a nearby ASCII substring instead.
- Skill candidate: no; covered by shell escaping guidance.

## 2026-07-10 - Product test helper read landed in the wrong method

- Symptom: A product-test edit produced a compile error because the `helper` file read was inserted into a nearby method while its assertions were added to the intended install-smoke method.
- Wrong assumption: A repeated repository-root setup sequence was a unique enough patch anchor.
- Root cause: `ProductExperienceTests.cs` contains many methods with the same `repoRoot` setup, so the broad anchor matched an earlier occurrence.
- Detection method: The focused test build reported `helper` missing at the assertion site; a narrow method read showed the declaration elsewhere.
- Fix: Removed the misplaced declaration and reinserted it using the unique test method name as context.
- Prevention rule: In large test files, anchor edits on the exact test method signature, not shared setup statements.
- Skill candidate: no; covered by the existing apply-patch recovery checklist.

## 2026-07-10 - WPF smoke incorrectly treated foreground focus as a prerequisite

- Symptom: UIAutomation focus polling timed out with `keyboardFocusable=false`; replacing it with `SetForegroundWindow` also timed out while another window remained foreground.
- Wrong assumption: A real GUI smoke must own keyboard focus before using `InvokePattern` or taking a screenshot.
- Root cause: Windows foreground-lock policy is independent of WPF readiness, and the smoke only needs UIAutomation invocation plus visible z-order.
- Detection method: Timeout diagnostics showed the app window was enabled, onscreen, and ready for interaction while never becoming keyboard-focusable or foreground.
- Fix: Replaced focus/foreground acquisition with `Show-WpfWindowForSmoke`, using `ShowWindowAsync` and `SetWindowPos(HWND_TOPMOST)` only.
- Prevention rule: Desktop smokes must wait for input-idle, then establish visibility/z-order only; request focus solely for a test that explicitly validates keyboard input.
- Skill candidate: yes; replace the earlier focus-polling candidate with a visibility-only shared helper rule.

## 2026-07-10 - Worklog append matched a repeated cleanup line

- Symptom: The candidate-preview worklog section was inserted before the eligible-actions section instead of at the authoritative file tail.
- Wrong assumption: The final-looking cleanup sentence was unique in an append-only development log.
- Root cause: Historical slices deliberately repeat cleanup wording.
- Detection method: `rg` and `Get-Content -Tail` showed the new section preceding its dependency slice.
- Fix: Removed the section and re-added it after the unique final eligible-actions context.
- Prevention rule: Append protocol sections using the latest section heading plus two unique trailing lines, then immediately verify the file tail.
- Skill candidate: no; already covered by append-only protocol guidance.

## 2026-07-10 - WPF bullet `StringFormat` broke XAML compilation

- Symptom: Solution build failed with `MC1000` at the protection-list bullet binding and an unrelated-looking missing `Extension` assembly message.
- Wrong assumption: `{Binding, StringFormat=&#x2022; {0}}` was a safe WPF markup-extension format.
- Root cause: The markup-extension parser treated the spaced format expression ambiguously.
- Detection method: Full solution build identified the exact XAML line; a focused static test then reproduced the forbidden form.
- Fix: Replaced the formatted binding with a two-column Grid containing a literal bullet TextBlock and a normal `{Binding}` TextBlock.
- Prevention rule: Do not place a leading symbol plus space and `{0}` directly inside WPF markup-extension `StringFormat`; use separate visual elements or a tested escaped format.
- Skill candidate: no; keep as a project XAML rule unless it repeats.

## 2026-07-10 - Repeated test initializer caused data to land in the wrong case

- Symptom: The recovery preflight test still reported no personal-data paths after the test edit.
- Wrong assumption: A repeated Marvis profile initializer was a unique patch anchor.
- Root cause: The patch matched a nearby official-uninstall test with the same install command and process list.
- Detection method: Failure output showed the `user-data-backup` step was complete; a method-scoped file read confirmed the intended profile lacked `DataPaths`.
- Fix: Reapplied the test-data edit with the exact test method signature as context.
- Prevention rule: In large test files, anchor repeated fixture edits on the method name, not shared initializer lines.
- Skill candidate: no; reinforces the existing large-test-file patch rule.

## 2026-07-10 - Assumed the signature inspector lived in the wrong folder

- Symptom: A parallel source-inspection command failed because `src/Css.Scanner/Security/SignatureInspector.cs` did not exist.
- Wrong assumption: Signature utilities were grouped under a `Security` directory.
- Root cause: The actual implementation is colocated with software inventory at `src/Css.Scanner/Software/SignatureInspector.cs`.
- Detection method: The shell error named the missing path; `rg --files src | rg "Signature|Inspector"` located the definition and `rg` confirmed its callers.
- Fix: Read the actual file and continued from its real API.
- Prevention rule: Locate implementation files with `rg --files` or symbol search before composing parallel reads from an assumed directory structure.
- Skill candidate: no; one-off repository navigation mistake.

## 2026-07-10 - Assumed scanner and pipeline filenames that do not exist

- Symptom: Source reads failed for `ISoftwareInventoryScanner.cs` and `OperationPipeline.cs`; a separate `rg` command also had an unterminated PowerShell quote.
- Wrong assumption: Common interface/class names and directory layouts matched this repository without symbol discovery.
- Root cause: Software scanning uses concrete scanner classes, and the pipeline implementation is `SafetyOperationPipeline.cs`; the quoted search pattern mixed shell and XML quoting.
- Detection method: PowerShell path/parser errors, followed by `rg --files` and symbol search.
- Fix: Located implementations by symbol/file search and read the actual files; no production code was changed based on the failed assumptions.
- Prevention rule: For unfamiliar components, locate symbols first and pass complex `rg` patterns as simpler individual commands rather than nesting fragile quotes.
- Skill candidate: no; reinforces the existing repository-navigation rule.

## 2026-07-10 - Collection spread was used in an unsupported `string.Join` call

- Symptom: Draft tests failed to compile with `IReadOnlyList<string>` cannot convert to `System.Index`.
- Wrong assumption: Collection-expression spread syntax could be embedded directly among `string.Join` params arguments.
- Root cause: The spread was parsed outside a target-typed collection expression.
- Detection method: Compiler errors pointed to both spread operands in the beginner-text assertion.
- Fix: Built the sequence with `new[] { ... }.Concat(...).Concat(...)` and passed the resulting enumerable to `string.Join`.
- Prevention rule: Use collection expressions only where a collection target type is explicit; use LINQ concatenation for variadic/enumerable APIs.
- Skill candidate: no; language-syntax correction.

## 2026-07-10 - Archive handler parsed camelCase manifests with case-sensitive defaults

- Symptom: The first confirmed archive test returned `Archive source is no longer a recognized OMNIX uninstall evidence manifest`; payload was null.
- Wrong assumption: Default `System.Text.Json` deserialization would map camelCase manifest properties to PascalCase model properties in the same way as the retention planner.
- Root cause: The store writes camelCase; the planner enabled `PropertyNameCaseInsensitive`, but the new handler initially did not.
- Detection method: Reordered the test to assert `Success` with `Error` before reading payload, exposing the exact validation failure.
- Fix: Added shared-equivalent case-insensitive `JsonSerializerOptions` in the handler.
- Prevention rule: Every reader of a persisted schema must reuse or explicitly mirror the writer's naming policy; assert operation success before inspecting payloads.
- Skill candidate: yes; persisted JSON reader/writer option consistency is cross-project and easy to encode in tests/templates.

## 2026-07-10 - New post-scan test contained two assertion compile errors

- Symptom: The intended TDD red also reported an out-of-scope `path` variable and then an expression-tree error for `is not null`.
- Wrong assumption: The collection assertions were valid FluentAssertions expressions before the production type existed.
- Root cause: One assertion referenced a lambda variable outside its scope; `OnlyContain` captures an expression tree that does not support that pattern form.
- Detection method: Focused test compilation separated test-source errors from the expected missing-type error.
- Fix: Replaced the set assertion with `BeEquivalentTo(...)` and used `candidate.Path != null` in the expression tree.
- Prevention rule: After adding a missing-type TDD test, clear all unrelated test compilation errors before implementation so the red signal has one intended cause.
- Skill candidate: no; local assertion-authoring mistake.

## 2026-07-11 - Restore-point WMI read blocked the uninstall plan indefinitely

- Symptom: Real GUI smoke stayed on “正在只读检查恢复准备...” for more than 60 seconds and never opened the plan window.
- Wrong assumption: A SELECT-only WMI query was safe to await without a time boundary because failures were caught.
- Root cause: `ManagementScope.Connect()` / `searcher.Get()` had no WMI timeout, and the outer `Task.Run` used an uncancelled default token.
- Detection method: Reproduced at 10 and 60 seconds, then added status boundary markers proving the delay occurred before plan generation.
- Fix: Added WMI connection/enumeration timeouts, an outer `WaitAsync` bound, and typed Completed/TimedOut/Failed results; timeout is presented as unknown and the UI continues.
- Prevention rule: Every optional Windows inventory probe must have a bounded wait and a typed Unknown/TimedOut result; read-only does not mean non-blocking.
- Skill candidate: yes; bounded optional OS probes with truthful uncertainty is cross-project.

## 2026-07-11 - WPF smoke missed a visible owned modal window

- Symptom: The plan window completed SourceInitialized/Loaded/ContentRendered and was visible in a desktop screenshot, but the smoke reported no secondary window.
- Wrong assumption: Every owned WPF modal appears under UIAutomation RootElement children or descendants.
- Root cause: On this desktop session the modal had a distinct HWND but was omitted from the UIA root window enumeration.
- Detection method: Lifecycle status markers, native handle comparison, process-window diagnostics, and a failure screenshot.
- Fix: Added shared Win32 `EnumWindows` / `GetWindowThreadProcessId` fallback and converted matching handles with `AutomationElement.FromHandle`, while retaining child AutomationId verification.
- Prevention rule: For WPF modal discovery, use semantic UIA first, native process-window enumeration second, and never replace child AutomationId assertions with coordinates.
- Skill candidate: yes; reusable GUI smoke helper pattern.

## 2026-07-11 - Static GUI assertion was inserted into the wrong similar test

- Symptom: The intended uninstall-smoke TDD test passed even though the helper lacked `EnumWindows`.
- Wrong assumption: A short shared-helper context in a large test file uniquely identified the uninstall test.
- Root cause: `apply_patch` matched an earlier cleanup-smoke block with the same surrounding assertions.
- Detection method: `rg -n "EnumWindows"` showed the assertion near line 488 instead of the uninstall test near line 2710.
- Fix: Removed the misplaced assertions, reinserted them beside the unique uninstall window finder assertion, and observed the expected RED before implementation.
- Prevention rule: In large repetitive test files, anchor patches on the unique test name or unique nearby literal and verify line placement before accepting RED/GREEN evidence.
- Skill candidate: no; project-local patch placement mistake.

## 2026-07-11 - WMI enumeration option type was ambiguous

- Symptom: First GREEN compile failed because `EnumerationOptions` matched both `System.Management` and `System.IO`.
- Wrong assumption: Existing `using System.Management` made the new type reference unambiguous under .NET 8 implicit usings.
- Root cause: Implicit `System.IO` introduced a same-named type.
- Detection method: Focused compiler error CS0104.
- Fix: Fully qualified `System.Management.EnumerationOptions`.
- Prevention rule: Fully qualify framework types that collide with implicit-usings namespaces at API boundaries.
- Skill candidate: no; straightforward compiler-guided correction.

## 2026-07-11 - WPF ItemsSource rejected an untyped collection expression

- Symptom: `Css.App` build failed with CS9174 after automated tests passed.
- Wrong assumption: Assigning `["..."]` to WPF `ItemsSource` would infer a constructible collection type.
- Root cause: `ItemsSource` is typed as non-generic `IEnumerable`, which is not a target-constructible collection type.
- Detection method: Direct `dotnet build src\Css.App\Css.App.csproj --no-restore` after focused tests.
- Fix: Replaced the collection expression with an explicit `new[] { ... }`.
- Prevention rule: Always build the WPF project after source-contract tests; use explicit arrays when assigning collection literals to non-generic `IEnumerable` properties.
- Skill candidate: no; local compile-time correction.

## 2026-07-11 - PowerShell Chinese literal made a correct GUI assertion fail

- Symptom: GUI smoke reported that the no-execution sentence was missing while diagnostics printed the exact expected Chinese sentence.
- Wrong assumption: A Chinese wildcard literal in a UTF-8 `.ps1` would be interpreted identically by Windows PowerShell 5.1.
- Root cause: The script host decoded the source literal differently from UIAutomation's Unicode text.
- Detection method: Included `Current.Name` and a screenshot path in the failure; actual UI text was correct.
- Fix: Constructed the phrase from Unicode code points through the script's existing `Join-Chars` helper and used ordinal matching.
- Prevention rule: In Windows PowerShell GUI smokes, build non-ASCII assertion text from Unicode code points unless the script encoding/runtime is explicitly guaranteed.
- Skill candidate: yes; reusable cross-project Windows GUI smoke rule.

## 2026-07-11 - WPF smoke launched a stale application binary

- Symptom: A source change from checklist-title scrolling to checklist-status scrolling passed its source test, but the next GUI screenshot still showed the old layout.
- Wrong assumption: Building the test project had rebuilt Css.App.exe.
- Root cause: Css.Tests does not reference the WPF executable project; the smoke script intentionally launches the existing binary and does not build.
- Detection method: Compared the screenshot with source, then inspected the smoke startup path and ran a full solution build before rerunning.
- Fix: Built ComputerSecuritySoftware.slnx, reran the unchanged smoke, and verified the checklist status became visible.
- Prevention rule: After every WPF code/XAML change, build Css.App or the solution before running a binary-launch GUI smoke; source-contract tests do not prove binary freshness.
- Skill candidate: yes; reusable desktop GUI verification rule.

## 2026-07-11 - Original-detail image preview looked corrupted while the PNG was clean

- Symptom: view_image at original detail intermittently displayed large black rectangles over a newly captured WPF screenshot.
- Wrong assumption: The screen-copy PNG itself contained compositor black blocks.
- Root cause: The preview rendering path was inconsistent at the full image size; sampled PNG pixels showed about 0.2% dark pixels and a repeated high-detail view rendered the complete image.
- Detection method: Sampled alpha/dark pixels from the saved PNG and reopened it at high detail.
- Fix: Treated the file-level pixel evidence plus stable high-detail rendering as authoritative and did not modify screenshot capture code.
- Prevention rule: Before diagnosing capture corruption, distinguish the saved file from the viewer: inspect pixel statistics and a normal-size render. Reject the artifact only when the file itself contains the blocks.
- Skill candidate: yes; extend screenshot QA guidance with file-versus-viewer validation.

## 2026-07-11 - PowerShell bitmap probe passed a PathInfo instead of a string

- Symptom: The first pixel-sampling command constructed no bitmap and then emitted divide-by-zero/null follow-on errors.
- Wrong assumption: System.Drawing.Bitmap would coerce Resolve-Path's PathInfo object to a file path.
- Root cause: The constructor overload expects a string or image; PowerShell selected neither for PathInfo.
- Detection method: Constructor conversion error in the diagnostic output.
- Fix: Used the resolved Path property under ErrorActionPreference Stop before constructing the bitmap.
- Prevention rule: Pass Path explicitly to .NET file APIs and enable stop-on-error so diagnostics do not continue with invalid state.
- Skill candidate: no; one-off diagnostic command mistake.

## 2026-07-11 - Parallel inspection assumed a nonexistent OperationResult file

- Symptom: A parallel source-read batch aborted because src/Css.Core/Operations/OperationResult.cs did not exist.
- Wrong assumption: OperationResult had its own file based on the public type name.
- Root cause: The type is declared inside OperationDescriptor.cs.
- Detection method: Get-Content path failure followed by rg locating the declaration.
- Fix: Re-ran the reads with the located source path and kept the successful inspection commands separate from the failed lookup.
- Prevention rule: Use rg --files or rg type declarations before assuming a type-to-file naming convention in this repository.
- Skill candidate: no; local repository-navigation mistake.

## 2026-07-11 - Fake transport integration introduced a nullable warning

- Symptom: The first focused integration build passed its test but emitted CS8601 for assigning nullable OperationDescriptor.ConfirmationText to required consent text.
- Wrong assumption: The fixture gate's runtime guarantee was enough for the compiler to infer non-null.
- Root cause: The operation model correctly keeps ConfirmationText nullable for other operation kinds.
- Detection method: Focused test build warning at the consent initializer.
- Fix: Added an explicit null guard in the test fixture before constructing final consent.
- Prevention rule: Treat zero-warning builds as a gate even for passing focused tests; encode fixture invariants explicitly when shared models are nullable.
- Skill candidate: no; compiler-guided fixture correction.

## 2026-07-12 - PowerShell command chaining was used despite the repository shell constraint

- Symptom: three read-only inspection commands failed because Windows PowerShell does not accept the used `&&` separator, and chained commands also violated the agent instruction to avoid noisy shell chaining.
- Wrong assumption: a familiar cross-shell separator would be accepted and harmless for quick paired reads.
- Root cause: lapse in applying the known PowerShell and repository command-shape rule.
- Detection method: immediate PowerShell parser errors before any command executed.
- Fix: issue independent shell calls through parallel tool orchestration or use a structured multi-line PowerShell script without shell control separators.
- Prevention rule: never place `&&`, `;`, or output-label chains in repository shell commands; parallelize independent reads at the tool layer.
- Skill candidate: yes

## 2026-07-12 - Full Scanner dependency introduced framework assembly conflicts

- Symptom: first production-worker build succeeded with two MSB3277 warnings for System.Text.Json 8.0 versus 10.0 after adding `Css.Elevated -> Css.Scanner`.
- Wrong assumption: reusing the full normal-privilege inventory scanner would preserve the Elevated project's zero-warning dependency graph.
- Root cause: transitive package versions in the broad scanner graph conflicted when resolved in the net8 Elevated executable, and the dependency added more privileged surface than the post-scan needed.
- Detection method: immediate Debug solution build after production worker composition.
- Fix: removed the Scanner project reference and added a minimal read-only uninstall-registry reader that fails closed on unreadable entries.
- Prevention rule: privileged hosts should depend on the smallest read-only capability contract; run a zero-warning build immediately after every new project reference.
- Skill candidate: yes

## 2026-07-12 - Binary mode audit initially decoded only UTF-8

- Symptom: source and runtime tests proved the production mode existed, but the first Release binary string audit reported it absent from Elevated.
- Wrong assumption: decoding the whole .NET assembly as UTF-8 would reveal all managed user strings.
- Root cause: .NET metadata commonly stores user strings in UTF-16.
- Detection method: contradiction between source/runtime evidence and binary scan.
- Fix: scan both UTF-8 and UTF-16 views; production mode/session are present in Elevated and absent from App.
- Prevention rule: managed binary string audits must check UTF-16 as well as UTF-8 or use a metadata-aware reader.
- Skill candidate: yes

## 2026-07-12 - Bootstrap tamper test had one alternate fail-closed status

- Symptom: the first full freshness run had one existing test return `ProtocolRejected` instead of expected `KeyConfirmationFailed`; all other 420 tests passed.
- Wrong assumption: asynchronous tampered finished-frame handling would always retain a valid frame shape long enough to reach tag comparison.
- Root cause: not proven; both statuses reject before session establishment, and the result did not reproduce.
- Detection method: full Debug suite, followed by isolated and repeated execution.
- Fix: no product or assertion weakening applied; the exact test passed standalone 10/10 and the next full 421/421.
- Prevention rule: if this classification recurs, capture exception/message and replace the stream tamper fixture with deterministic whole-frame mutation before considering assertion changes.
- Skill candidate: yes

## 2026-07-12 - Async exception assertion targeted a Task instead of a delegate

- Symptom: the first authorization test compile failed because `ThrowAsync` was called on assertions for a `Task` object.
- Wrong assumption: FluentAssertions would infer an async action directly from the already-started task.
- Root cause: async exception assertions require a `Func<Task>` so execution and exception capture are controlled by the assertion.
- Detection method: focused authorization test compilation.
- Fix: wrap awaiting the server task in an async delegate before calling `ThrowAsync`.
- Prevention rule: use `Func<Task> action = async () => await task;` for async failure assertions, especially when the task was started before setup interaction.
- Skill candidate: no

## 2026-07-12 - First image-correlation test compile had fixture wiring errors

- Symptom: the first focused build failed because the lifecycle test lacked the `Css.App` namespace, used a FluentAssertions string overload that does not accept a comparer, and App passed a nullable hash to the now-required launcher parameter.
- Wrong assumption: the existing test imports and string assertion overload would cover the new App-owned inspector, and control-flow trust would satisfy nullable analysis.
- Root cause: the inspector crossed the App/test namespace boundary, the selected assertion API differed from the assumed overload, and nullable flow does not infer `FileSha256` from `CanLaunchDevelopmentVerification`.
- Detection method: first focused lifecycle/presentation compilation.
- Fix: import `Css.App`, assert `string.Equals(..., OrdinalIgnoreCase)`, and explicitly reject missing development hash before constructing the launcher.
- Prevention rule: after tightening a constructor contract, compile the narrowest cross-project test immediately and express security invariants explicitly instead of relying on correlated nullable state.
- Skill candidate: no

## 2026-07-12 - Early client close hid the endpoint security rejection

- Symptom: The tampered-request pipe test expected `InvalidRequest` but the server returned `ConnectionFailed`.
- Wrong assumption: Writing the rejection response would always succeed after the endpoint had classified the request.
- Root cause: The raw adversarial client intentionally closed after writing; the response write threw and the outer connection catch replaced the already-established rejection status.
- Detection method: Focused real named-pipe test failed while the fake endpoint call count remained zero.
- Fix: Preserve a non-completed endpoint status when response delivery fails; only a successfully handled request can be replaced by a connection-delivery failure.
- Prevention rule: Security classification and response delivery are separate outcomes. Never erase a verified pre-execution rejection because an untrusted peer disconnects before reading it.
- Skill candidate: yes; include early-disconnect behavior in reusable privileged-IPC tests.

## 2026-07-12 - New-project restore was attempted inside the restricted network sandbox

- Symptom: solution restore waited about 109 seconds and failed every project with NU1301/socket access errors.
- Wrong assumption: cached packages would let a normal solution restore complete without contacting NuGet after adding a project.
- Root cause: the new project needed an assets file and restore also attempted the configured vulnerability/package source, which the workspace sandbox blocks.
- Detection method: NU1301 output named `https://api.nuget.org/v3/index.json` and socket permission denial.
- Fix: reran the same restore with the required network escalation; all project assets restored successfully in seconds.
- Prevention rule: after adding a .NET project, expect one restore; if the first sandboxed attempt reports a network restriction, request the scoped `dotnet restore` escalation immediately.
- Skill candidate: no; standard environment permission handling.

## 2026-07-12 - Final-consent smoke initially hid the failing control identity

- Symptom: the first GUI run reported only that a required control was missing or offscreen; an unchanged second run passed.
- Wrong assumption: a grouped visibility assertion supplied enough evidence for GUI diagnosis.
- Root cause: the script collapsed five UIAutomation checks into one generic message; the first run was transient and could not identify which peer caused it.
- Detection method: replaced the grouped assertion with named missing/offscreen diagnostics, then reran the unchanged binary successfully.
- Fix: retained per-control diagnostic ids; the second run proved all controls, pipe facts, screenshots, close, and process exit.
- Prevention rule: GUI smokes must name the exact AutomationId/state on failure, even when one grouped assertion covers several required controls.
- Skill candidate: yes; fold named peer diagnostics into shared WPF smoke helpers.

## 2026-07-12 - FluentAssertions collection API was assumed incorrectly

- Symptom: the first bootstrap test compile failed because `GenericCollectionAssertions<byte>` has no `NotOnlyContain` method in the installed FluentAssertions version.
- Wrong assumption: the library exposed a direct inverse of `OnlyContain` for byte collections.
- Root cause: the intended API is not present in FluentAssertions 8.10.0.
- Detection method: focused compile error CS1061 before any bootstrap test ran.
- Fix: asserted `Any(value => value != 0).Should().BeTrue()` explicitly.
- Prevention rule: use simple LINQ boolean assertions for version-sensitive inverse collection predicates.
- Skill candidate: no; local test syntax correction.

## 2026-07-12 - Separate-process smoke test missed Apps namespace

- Symptom: the first focused build failed to resolve `OfficialUninstallRecoveryMethod`, `OfficialUninstallExecutionGateResult`, and `OfficialUninstallCommandTrustResult` in the new smoke-worker test.
- Wrong assumption: importing `Css.Core.Uninstall` would cover all official-uninstall fixture types.
- Root cause: recovery and execution-gate contracts intentionally live in `Css.Core.Apps`.
- Detection method: focused `dotnet test --filter OfficialUninstallSmokeWorkerProcessTests` compile failure.
- Fix: add the explicit `using Css.Core.Apps;` import.
- Prevention rule: copy the namespace set from an existing ready-draft fixture when adding a new cross-boundary uninstall test.
- Skill candidate: no

## 2026-07-12 - Smoke executable configuration path was one parent too high

- Symptom: the first runnable focused test searched for `src/Css.SmokeTools/bin/bin/net8.0-windows/Css.SmokeTools.exe`.
- Wrong assumption: two parent traversals from `AppContext.BaseDirectory` would yield the build configuration name.
- Root cause: the base directory itself is `.../bin/Debug/net8.0-windows`, so its immediate parent is already `Debug`.
- Detection method: focused test failure printed the computed executable path.
- Fix: read the immediate parent directory name.
- Prevention rule: derive and assert test-tool paths from the concrete current output layout before launching a child process.
- Skill candidate: no

## 2026-07-12 - Worker receipt relied on default JSON property casing

- Symptom: the successful cross-process test could not find the expected `workerProcessId` field in the worker receipt.
- Wrong assumption: serializing a typed receipt would use the same camel-case property names as an existing anonymous smoke output.
- Root cause: default `System.Text.Json` preserves typed CLR property names as PascalCase.
- Detection method: the round trip completed, but `JsonElement.GetProperty("workerProcessId")` failed.
- Fix: define explicit camel-case receipt serializer options.
- Prevention rule: give machine-readable smoke outputs an explicit naming policy instead of relying on serializer defaults.
- Skill candidate: no

## 2026-07-12 - Boundary static test retained the pre-receipt composition call

- Symptom: focused runtime-receipt tests passed behavior/compile checks but one static boundary test still required `App.xaml.cs` to contain `OfficialUninstallElevatedRequestComposer`.
- Wrong assumption: all ownership assertions had been updated when the pure issuer/session moved to Core.
- Root cause: the boundary test encoded the previous direct-composer DEBUG flow; the new flow correctly consumes `VisualTicketId` through `OfficialUninstallElevatedRequestSession`.
- Detection method: focused static assertion failure after the Core/App migration.
- Fix: require the request session and actual visual ticket instead of direct composer text.
- Prevention rule: when replacing a boundary API, search for both the moved type and the old caller symbol across all static contract tests.
- Skill candidate: no

## 2026-07-12 - Enabling WPF tests changed an implicit using assumption

- Symptom: after enabling WPF in `Css.Tests`, existing tests no longer resolved the standard implicit namespaces for `Stream`, `Path`, `File`, collection expressions, and related SDK conveniences.
- Wrong assumption: the test project's previous SDK implicit-usings set would remain identical with `UseWPF=true`.
- Root cause: the WindowsDesktop build configuration did not supply the same standard implicit-using set relied on by the existing test project.
- Detection method: focused visual-capture test compile.
- Fix: preserve the prior standard SDK using set explicitly in `Css.Tests.csproj`; keep `System.IO` explicit in the stream-based bootstrap test.
- Prevention rule: when enabling WPF in an established test project, compare and preserve its prior implicit-using surface before running focused tests.
- Skill candidate: no

## 2026-07-12 - Request-preparation test factory captured an out parameter

- Symptom: the first focused request-preparation build failed with CS1628 in the visual-ticket factory lambda.
- Wrong assumption: an `out` parameter could be captured directly by the issuer's deferred ticket-id lambda.
- Root cause: C# forbids capturing `ref`, `out`, or `in` parameters in lambdas.
- Detection method: focused test compile.
- Fix: capture a local constant and assign that value to the `out` parameter separately.
- Prevention rule: factory delegates in test helpers should capture local immutable values, never by-reference parameters.
- Skill candidate: no

## 2026-07-12 - Nonblank WPF receipt still contained crop and black blocks

- Symptom: first exported receipt cropped the right/bottom actions on a transparent black background; after copy changed, another immediate render showed partial black blocks and missing text.
- Wrong assumption: a nonblank `RenderTargetBitmap` plus viewport flags proved the captured image was visually trustworthy.
- Root cause: the rendered root carried a parent-relative margin, and direct capture could run before WPF completed its first Render-priority composition.
- Detection method: original-detail inspection of the exported receipt artifact.
- Fix: add a full-size background root, move margin inward, flush Dispatcher Render priority, and draw an origin-normalized `VisualBrush` before encoding.
- Prevention rule: every safety screenshot change must export and inspect the actual receipt bytes; nonblank/hash tests alone are insufficient.
- Skill candidate: yes

## 2026-07-12 - Static assertion decoded source Unicode escapes

- Symptom: a product static test expected decoded Chinese while the authoritative source intentionally stored `\u` escape text.
- Wrong assumption: a normal C# test string would compare against the literal source representation.
- Root cause: the assertion string decoded Unicode escapes at compile time.
- Detection method: focused product contract failure showing the full source.
- Fix: use a verbatim string to match literal `\u` source text.
- Prevention rule: source-text tests for escaped Unicode must use verbatim or double-escaped expected strings.
- Skill candidate: no

## 2026-07-12 - Parallel project builds contended on the shared Ipc output

- Symptom: the first three-project compile attempt failed with CS2012 because concurrent builds tried to write the same `Css.Ipc.dll`.
- Wrong assumption: independent project build commands could safely run in parallel despite sharing a project dependency and output path.
- Root cause: each build invoked the compiler for Ipc at the same time; one output handle was locked by `VBCSCompiler`.
- Detection method: first compile output identified the locked `src/Css.Ipc/obj/.../Css.Ipc.dll`.
- Fix: run dependent .NET builds sequentially with `-m:1 -p:UseSharedCompilation=false`.
- Prevention rule: parallelize read-only inspection, but serialize builds that share project outputs.
- Skill candidate: no

## 2026-07-12 - WPF worker launcher missed the System.IO import

- Symptom: the first App compile failed with CS0103 for `Path.GetFullPath`.
- Wrong assumption: the WPF project's implicit using set included `System.IO`.
- Root cause: this project does not provide that namespace implicitly for the new source file.
- Detection method: sequential App build.
- Fix: add `using System.IO;` and rerun the build.
- Prevention rule: add explicit standard-library imports in new WPF source files instead of relying on SDK implicit-using variants.
- Skill candidate: no

## 2026-07-12 - Combined publish inspection was misread as missing worker files

- Symptom: the first combined publish-inspection output appeared empty, and I reported that the four Elevated files were missing.
- Wrong assumption: no formatted rows in that combined command meant the copy target failed.
- Root cause: several checks were composed in one PowerShell command and their formatting/exit-state interaction obscured the first result; a direct `rg --files` immediately showed all four files existed.
- Detection method: explicit recursive file listing and one-file-at-a-time `Test-Path`/length checks.
- Fix: corrected the status, verified all four files and App deps independently, and kept the working target unchanged.
- Prevention rule: release gates must print one named boolean/length per artifact; never infer absence from an empty combined formatter result.
- Skill candidate: no

## 2026-07-12 - Raw Chinese broke Windows PowerShell smoke parsing

- Symptom: AST parsing reported unexpected tokens and an unterminated string in the new UAC smoke script.
- Wrong assumption: a UTF-8 script without BOM would parse raw Chinese literals consistently in Windows PowerShell 5.
- Root cause: Windows PowerShell treated the file as a legacy local encoding, corrupting multibyte source bytes.
- Detection method: `System.Management.Automation.Language.Parser.ParseFile` before running the GUI smoke.
- Fix: keep the script ASCII-only and construct the two expected Chinese titles from Unicode code points; AST parse and source ASCII audit now pass.
- Prevention rule: new `.ps1` smoke scripts must remain ASCII-only unless deliberately written with a verified BOM; parse every script before GUI execution.
- Skill candidate: yes

## 2026-07-12 - Quoted rg audit command failed PowerShell parsing

- Symptom: one source audit stopped with an unterminated double-quoted string.
- Wrong assumption: nested JSON, JavaScript, PowerShell, regex, and XML quote escaping remained correct in a single `rg` command.
- Root cause: the XML attribute quote inside the regex terminated the PowerShell string after tool-layer escaping.
- Detection method: PowerShell parser error; the preceding full test had already completed successfully.
- Fix: replace the regex with a single-quoted `Select-String -SimpleMatch` boolean check and run remaining audits independently.
- Prevention rule: use literal single-quoted `Select-String` for exact XML fragments; reserve `rg` for patterns that do not require four-layer quote escaping.
- Skill candidate: no

## 2026-07-12 - Empty dictionary collection expression was not constructible

- Symptom: the first trust-policy test build failed with CS9174 for `new FakeVerifier([])`.
- Wrong assumption: an empty collection expression could instantiate an `IReadOnlyDictionary` constructor argument.
- Root cause: the target interface has no constructible collection builder in that expression context.
- Detection method: focused trust-policy compile.
- Fix: pass an explicit `new Dictionary<string, AuthenticodeSignatureEvidence>()`.
- Prevention rule: use concrete dictionary construction for interface-typed test fixture parameters unless the target type is known to support collection expressions.
- Skill candidate: no

## 2026-07-12 - Catalog-signed system file was not an embedded-signature fixture

- Symptom: `Get-AuthenticodeSignature` called `notepad.exe` valid, while the new embedded-signature verifier correctly returned `NotSigned`.
- Wrong assumption: every Windows file reported as Authenticode-valid carries an embedded signer certificate.
- Root cause: Windows can trust files through signed catalogs; OMNIX's production policy intentionally requires embedded signatures on its separately shipped executables.
- Detection method: focused native verifier test plus `X509Certificate.CreateFromSignedFile` comparison across system candidates.
- Fix: use embedded-signed `Taskmgr.exe` as the positive fixture and retain `notepad.exe` only as evidence that catalog trust is a different mechanism.
- Prevention rule: signature tests must state whether they require embedded, catalog, or either trust and choose fixtures accordingly.
- Skill candidate: no

## 2026-07-12 - Tampering removed the discoverable embedded signature

- Symptom: appending one byte to the signed fixture returned `NotSigned` instead of the test's expected `Invalid` or `Untrusted`.
- Wrong assumption: WinTrust would always retain enough signature structure to classify altered PE data as a bad digest.
- Root cause: the modified file was no longer recognized as carrying a usable embedded signature; fail-closed status remained correct.
- Detection method: focused tamper test and an independent `CreateFromSignedFile` probe on the changed copy.
- Fix: assert the security invariant `IsTrusted=false` and permit Invalid, Untrusted, or NotSigned classification.
- Prevention rule: tamper tests should assert loss of authority first; native diagnostic categories may vary with the exact mutation.
- Skill candidate: no

## 2026-07-12 - Windows PowerShell could not reflect a net8 Vanara assembly

- Symptom: loading the net8 WinTrust package into Windows PowerShell failed to resolve `System.Runtime, Version=8.0.0.0` and yielded no reflected types.
- Wrong assumption: the Windows PowerShell/.NET Framework host could inspect a net8-targeted assembly directly.
- Root cause: runtime/target-framework mismatch in the reflection host.
- Detection method: `Assembly.LoadFrom`/`GetTypes` loader exceptions.
- Fix: stop using that host for API reflection, inspect package XML docs, and implement the narrow WinTrust boundary in the net8 Win32 project where compile/runtime verification is authoritative.
- Prevention rule: inspect net8 assemblies from a net8 process or metadata reader, not Windows PowerShell's .NET Framework runtime.
- Skill candidate: no

## 2026-07-13 - Static XAML audit used invalid PowerShell quote escaping

- Symptom: the combined static-audit command stopped at a PowerShell parser error before returning the intended XAML checks.
- Wrong assumption: backslash escaping inside a double-quoted PowerShell string would escape embedded quotes.
- Root cause: PowerShell uses the backtick, not backslash, for that quoting form; one rejected promise also hid independent command output.
- Detection method: the command failed immediately with `MissingEndParenthesisInMethodCall`; no PASS result was accepted.
- Fix: constructed the AutomationId needle by concatenating literal string parts and reran checks with `Promise.allSettled`, so each check reported independently.
- Prevention rule: avoid nested quote escaping in PowerShell static checks; build literals from single-quoted parts and ensure parallel checks cannot suppress sibling results.
- Skill candidate: no

## 2026-07-13 - Cache execution initially rechecked path state but not current app ownership

- Symptom: the first cache execution draft rescanned the app and revalidated that paths still existed, but did not prove the refreshed profile still listed those exact cache paths.
- Wrong assumption: a unique same-name app plus an unchanged cache path was sufficient correlation.
- Root cause: path safety and application ownership were reviewed as separate checks without an explicit binding invariant.
- Detection method: manual source review of the post-confirmation sequence before static acceptance.
- Fix: added `MatchesCurrentProfile` and refuse when the current unique profile no longer owns every planned cache path, is system-related, or is running.
- Prevention rule: every stale-plan recheck must revalidate both object identity/ownership and resource state; neither alone authorizes mutation.
- Skill candidate: yes

### Recurrence - 2026-07-13 final source audit

- Symptom: a final `rg` line-number query again treated part of a double-quoted regular expression as a path, and a later broad inventory query included two guessed source directories that do not exist.
- Wrong assumption: the existing prevention rule would be followed reliably while composing a nested JavaScript/PowerShell command, and guessed project names were safe in a parallel audit.
- Root cause: the regex was not passed as one PowerShell single-quoted literal, and directory inputs were not derived from `Get-ChildItem`/`rg --files` first.
- Detection method: `rg` returned an I/O/path error; `Promise.allSettled` showed the other read-only checks independently. No source edit or runtime action occurred.
- Fix: reran with single-quoted regex literals, enumerated actual `src` directories, and kept independent audit results fail-closed.
- Prevention rule: source-audit commands must discover paths first and pass complex `rg` patterns as one single-quoted PowerShell argument; repeated checks should move into a repository script instead of ad hoc nested quoting.
- Skill candidate: yes

### Second recurrence - 2026-07-13 structured evidence audit

- Symptom: the first seven-check static-audit script failed in the JavaScript parser because a PowerShell backtick-newline token appeared inside a JavaScript template literal.
- Wrong assumption: removing regex quoting was enough to make the remaining nested script safe.
- Root cause: two languages still used the same backtick delimiter in one command payload.
- Detection method: JavaScript `SyntaxError` occurred before any shell check executed; no PASS was accepted.
- Fix: replaced the PowerShell backtick newline with `[Environment]::NewLine` and reran all seven checks independently to real PASS results.
- Prevention rule: do not embed PowerShell backticks inside JavaScript template literals; use language-neutral APIs or a checked-in audit script.
- Skill candidate: yes

### Third recurrence - 2026-07-13 unrelated gate sanity check

- Symptom: a final read-only sanity check guessed `src\Css.Win32\Migration\MigrationProductionExecutionCoordinator.cs`, but the file actually lives in `src\Css.App`.
- Wrong assumption: the coordinator's namespace/authority suggested its physical project location.
- Root cause: the command read a predicted path before using `rg --files` to discover the real file.
- Detection method: immediate file-not-found output; no verification claim used the failed query.
- Fix: used `rg --files` and exact symbol search to locate the file, then stopped the unrelated audit without changing migration code.
- Prevention rule: never infer repository paths from type ownership; discover with `rg --files` or exact symbol search first.
- Skill candidate: yes

### Fourth recurrence - 2026-07-13 visible-control audit regex

- Symptom: a broad `rg` command intended to list XAML Click/Content attributes failed with an unclosed character class after nested escaping removed quote characters.
- Wrong assumption: a compact regular expression would survive JavaScript, PowerShell, and ripgrep parsing unchanged.
- Root cause: the audit again used an ad hoc multi-language regex instead of simple fixed-string searches or XML parsing.
- Detection method: ripgrep parser error; no audit result was accepted and sibling reads completed independently.
- Fix: used direct source ranges and fixed-string symbol searches for the actual control/handler audit.
- Prevention rule: XAML structure audits must use XML parsing or fixed-string searches; do not pass quote-heavy attribute regexes through nested command layers.
- Skill candidate: yes

## 2026-07-13 - Missing service start value was mapped to Boot

- Symptom: source inspection showed `Convert.ToInt32(null)` would return zero, causing a registry service without readable `Start` evidence to be labeled as Boot-start.
- Wrong assumption: conversion of a missing registry value would throw and fall into the unknown-state branch.
- Root cause: .NET converts null to integer zero for this conversion API.
- Detection method: manual source review before compilation during the antivirus-definition pause.
- Fix: return unknown before numeric conversion when the registry value is null; added a focused source test.
- Prevention rule: distinguish missing registry evidence before type coercion; unknown input must remain unknown in system-decision models.
- Skill candidate: no

## 2026-07-13 - Startup action label was overwritten with an execution promise

- Symptom: XAML said `管理自启动`, but `AppPresentationBuilder.CreateActions` still supplied `关闭自启动`, so selecting an app could restore the misleading label.
- Wrong assumption: changing the static XAML content was sufficient to change the runtime button copy.
- Root cause: the selected-app action binding owns the final button label.
- Detection method: scoped static search across startup labels while wiring structured evidence.
- Fix: changed the presentation action label and its product assertion to `管理自启动`.
- Prevention rule: when UI content is data-bound or overwritten in code, audit the final presentation source as well as XAML defaults.
- Skill candidate: no

## 2026-07-13 - Quarantine timeline failure was outside compensating rollback

- Symptom: all paths could be moved successfully and then timeline persistence could throw, leaving manifests on disk but no beginner-visible restore entry.
- Wrong assumption: once manifests existed, timeline persistence was bookkeeping rather than part of successful completion.
- Root cause: the original `try/catch` covered only per-path moves; `_timeline.AddAsync` ran after the compensation boundary.
- Detection method: transaction-order review while connecting application cache cleanup to the existing quarantine handler.
- Fix: moved timeline persistence into the same protected block; any move or timeline failure reverses completed moves, with partial-restore evidence attempted for incomplete compensation.
- Prevention rule: for reversible user-facing operations, success requires mutation, recovery evidence, and visible recovery indexing; keep all three in one commit/compensation boundary.
- Skill candidate: yes

## 2026-07-13 - Startup button label promised more authority than the implementation had

- Symptom: the drawer button said `关闭自启动` even though the safe implementation could only explain evidence and later open a Windows management page.
- Wrong assumption: the requested outcome could be used as the initial button label while execution remained preview-only.
- Root cause: capability wording was not re-evaluated when the safe handoff design replaced direct modification.
- Detection method: manual UX review after wiring the official Startup apps settings action.
- Fix: changed visible text to `管理自启动`; the follow-up button says `在 Windows 中查看`, and copy explicitly states that OMNIX does not toggle anything.
- Prevention rule: command labels must describe the authority the next click actually has, not the eventual user goal.
- Skill candidate: no

## 2026-07-13 - Focused test tried to widen an internal launcher seam

- Symptom: focused tests failed to compile because `WindowsOfficialUninstallWorkerMode` and `WorkerArguments` are intentionally internal to Css.App.
- Wrong assumption: the test assembly already had access to App internals because older tests inspected the same launcher indirectly.
- Root cause: no `InternalsVisibleTo` boundary exists, and exposing worker command construction publicly would enlarge the production API only for a test.
- Detection method: focused test compile errors CS0122 and CS0117.
- Fix: keep the production API narrow and invoke the internal argument builder through bounded reflection in the white-box test.
- Prevention rule: confirm assembly visibility before writing cross-project white-box tests; do not promote security-sensitive implementation details solely for test convenience.
- Skill candidate: no

## 2026-07-13 - Negative rg audit was treated as a command failure

- Symptom: a static audit command failed first because a pattern beginning with `--` was parsed as an option, then again because the expected no-match result returned exit code 1.
- Wrong assumption: the combined `rg` invocation would distinguish a leading-dash pattern and treat no matches as a successful security assertion.
- Root cause: the pattern lacked the `--` option terminator on the first run, and `rg` correctly uses exit code 1 for no matches.
- Detection method: immediate shell output from the launcher secret-channel audit.
- Fix: use `Select-String` with an explicit path-free success message for expected-negative audits.
- Prevention rule: use `rg -- <pattern>` for leading-dash patterns and avoid making an expected no-match `rg` the success exit condition of a combined audit.
- Skill candidate: no

## 2026-07-13 - Old non-execution UI assertion blocked intentional coordinator wiring

- Symptom: the first full run failed because `ProductExperienceTests` forbade any `ExecuteAsync` text in `UninstallPlanWindow`.
- Wrong assumption: focused coordinator tests covered every prior static invariant affected by replacing the inert request with an injected execution boundary.
- Root cause: the old test encoded the previous phase's architecture rather than the durable rule that WPF must not own privileged execution authority.
- Detection method: full 440-test run after focused coordinator tests passed.
- Fix: replace the obsolete assertion with checks that WPF calls only the coordinator and contains no production launcher, mode string, lifecycle client, process start, pipeline, or handler.
- Prevention rule: when intentionally advancing a staged architecture, search for static tests that encode the old stage and rewrite them around the lasting security invariant.
- Skill candidate: no

## 2026-07-13 - Finished tamper classification recurred under the full suite

- Symptom: the server-finished tamper test again returned `ProtocolRejected` instead of `KeyConfirmationFailed`, after an earlier isolated non-reproduction.
- Wrong assumption: the previous one-off alternate fail-closed status could remain under observation without changing the finished-phase boundary.
- Root cause: finished framing/decoding errors flowed through generic protocol catches even though the session key had already been derived and the only pending invariant was key confirmation.
- Detection method: full-suite recurrence after WPF coordinator integration; focused test alone remained green.
- Fix: add a scoped finished reader that preserves cancellation but maps malformed payload/frame/truncation/disconnect to `KeyConfirmationFailed` on both client and server.
- Prevention rule: protocol state machines should classify errors by the active security invariant, not only by the lowest-level exception type; recurring alternate statuses require a semantic fix, not a relaxed assertion.
- Skill candidate: yes

## 2026-07-13 - Residue rescan static test matched a temporary local variable shape

- Symptom: full 441-test regression failed because a static test expected the exact line `var afterProfiles = await ScanSoftwareProfilesAsync();` after the handler was generalized to reuse an already refreshed inventory.
- Wrong assumption: the earlier test encoded the durable “rescan before residue decision” invariant.
- Root cause: it actually encoded one implementation spelling and did not allow a correlated caller to supply a fresh scan result.
- Detection method: full regression after focused residue/coordinator tests passed.
- Fix: assert the `knownAfterProfiles ?? await ScanSoftwareProfilesAsync()` freshness fallback, captured-profile wrapper, and scan-before-build order.
- Prevention rule: static order tests should permit dependency injection/reuse of equally fresh evidence and assert provenance plus ordering, not one local declaration string.
- Skill candidate: no

## 2026-07-13 - Test used an object initializer on a factory result

- Symptom: migration focused tests failed to compile with missing semicolon/brace errors around `FakePathAdapter.ForManifest(...){ ... }`.
- Wrong assumption: an object initializer can be applied to the result of an arbitrary factory method call.
- Root cause: C# object initializers apply to object creation expressions, not method-return expressions.
- Detection method: first focused migration compile, CS1002/CS1513.
- Fix: assign the factory result first, then set injected failure properties explicitly.
- Prevention rule: use initializer syntax only with `new`; configure factory-returned fixtures in following statements or via factory parameters.
- Skill candidate: no

## 2026-07-13 - Test fixture method shadowed System.IO.Path

- Symptom: adapter tests failed to compile because `DirectoryFixture.Path(...)` shadowed `System.IO.Path` inside the fixture constructor.
- Wrong assumption: `Path.Combine` would still resolve to the type despite an instance method named `Path` in the same class.
- Root cause: member-name lookup selected the method group in that context.
- Detection method: focused compile CS0119 at the constructor's `Path.Combine` and `Path.GetTempPath`.
- Fix: fully qualify `System.IO.Path` in the constructor and existing helper expression.
- Prevention rule: avoid naming fixture helpers after common framework types, or fully qualify the type consistently inside that scope.
- Skill candidate: no
## 2026-07-13 - Generated test assembly triggered antivirus

- Symptom: Huorong repeatedly reported `Trojan/ShellLoader.gx` and deleted Debug copies of `Css.Tests.dll` during builds.
- Wrong assumption: a source-reviewed, green managed test assembly could continue to be rebuilt safely without checking endpoint-security acceptance after new high-risk worker tests and a virus-definition update.
- Root cause: not yet proven. Timing and scope indicate a likely heuristic collision involving process/UAC/named-pipe test code and literal rejected shell-command fixtures, but a true-positive classification has not been independently ruled out.
- Detection method: user-provided `safe.txt`, process audit, and static searches for injection, loader, download, shell, P/Invoke, and process-start patterns.
- Fix: stopped builds/tests and did not restore or whitelist the artifact; marked security acceptance failed pending independent verification or transparent test-project isolation.
- Prevention rule: treat antivirus acceptance as a separate quality gate for worker/test artifacts; after adding process, IPC, elevation, or hostile-command fixtures, scan generated assemblies before repeated full builds.
- Skill candidate: yes

## 2026-07-13 - Duplicate file-read projection in source-only authority test

- Symptom: static review showed a second `.Select(File.ReadAllText)` being applied to strings that had already been read from disk.
- Wrong assumption: a partial replacement of the old file-enumeration pipeline had removed the old projection automatically.
- Root cause: the patch replaced the source sequence but left one method-group projection from the previous LINQ chain.
- Detection method: immediate `Get-Content` inspection of the edited test block before any build was attempted.
- Fix: removed the duplicate projection so `wpfFiles` is a string array produced by one explicit file read per WPF surface.
- Prevention rule: after replacing a multi-line LINQ source, inspect the complete expression through its terminal operator; source-only work still requires syntax-shape review even when compilation is intentionally paused.
- Skill candidate: no

## 2026-07-13 - Mixed-file safety patch used the wrong ownership context

- Symptom: `apply_patch` rejected a combined installer hardening patch because it searched for `CurrentPackageMatches` in the routing-capability file.
- Wrong assumption: all requested installer safety edits belonged to the same source file context.
- Root cause: routing policy, operation revalidation, and process-launch validation were combined in one patch without matching each hunk to its owning file.
- Detection method: immediate `apply_patch` context-verification failure; no partial edit was applied.
- Fix: split the patch by ownership and apply the unknown-kind policy, handler type allowlist, launcher MSI rule, and test separately.
- Prevention rule: for security changes spanning policy/handler/adapter layers, patch one owning file at a time or verify every update header before submission.
- Skill candidate: no

## 2026-07-13 - Source-only snapshot patch contained an invalid backslash literal

- Symptom: static inspection showed `TrimEnd('\', '/')` with an invalid single-backslash character literal in `ScanSnapshotBuilder.NormalizeRoot`.
- Wrong assumption: the earlier escaped patch text had produced a valid C# `\\` character literal.
- Root cause: one escaping layer was lost while composing the patch through JavaScript and `apply_patch`.
- Detection method: UTF-8 `Get-Content` plus targeted `rg` inspection before deferred compilation.
- Fix: replaced it with the valid `TrimEnd('\\', '/')` source literal and re-read the exact line.
- Prevention rule: after source-only edits containing Windows path character literals, inspect the exact UTF-8 source line; do not rely on the patch request's rendered escaping.
- Skill candidate: no

## 2026-07-13 - Failed Roslyn load printed false syntax PASS lines

- Symptom: a PowerShell static-parser attempt printed `syntax PASS` and `syntax errors: 0` even though `Add-Type` failed and `CSharpSyntaxTree` was unavailable.
- Wrong assumption: SDK Roslyn assemblies would load into Windows PowerShell and any load/type failure would terminate the script.
- Root cause: the .NET SDK assemblies were incompatible with the current Windows PowerShell host, errors were non-terminating, and the script treated null diagnostics as an empty successful set.
- Detection method: immediate inspection of the same command output showed loader/type errors below the misleading PASS lines.
- Fix: invalidated the entire result, did not cite it as evidence, and used explicit source invariants, XML parsing, scoped authority scans, and manual UTF-8 inspection instead.
- Prevention rule: source-validation helpers must set `$ErrorActionPreference = 'Stop'`, assert parser/type availability before reading diagnostics, and emit PASS only after a non-null parse result; parser startup failure is a failed check.
- Skill candidate: yes

## 2026-07-13 - Source-only audit batches were fragile and over-broad

- Symptom: three read-only batches stopped on a nested quote, an interpolated `$pattern:` token, an expected `rg` no-match exit, and one guessed nonexistent file; the first UTF-8 scan also included `obj/bin` and falsely reported thousands of generated lines.
- Wrong assumption: a large one-off PowerShell command would preserve quoting, treat expected no-match as success, know unverified paths, and scan only owned source.
- Root cause: too many parser layers and responsibilities were combined without explicit scope discovery or independent result handling.
- Detection method: PowerShell parser/file errors, `rg` exit code 1, and obviously impossible UTF-8 output against generated files.
- Fix: invalidated every affected result; switched to short fixed-string commands, confirmed paths before reads, used `Promise.allSettled`, parsed XML structurally, and ran strict UTF-8 decoding only on non-generated `.cs`/`.xaml` files.
- Prevention rule: source audits must discover paths first, keep each check independent, distinguish expected zero matches, exclude generated directories, and emit PASS only from a bounded verified scope.
- Skill candidate: yes
- Recurrence: immediately after recording this rule, one icon discovery batch again used combined bare `rg` commands and stopped on an expected no-match. A later `rg *.slnx` emitted a Windows path error that an unconditional `exit 0` would have hidden. Both outputs were treated as invalid; subsequent discovery used independent settled calls and known paths. Future wrapper work must fail on stderr as well as nonzero exit.

## 2026-07-13 - Agent scroll container was patched into the wrong page

- Symptom: `AgentConversationScrollViewer` appeared in the C-drive page, the Agent page had an unmatched closing tag, and the first broad closing-tag patch then inserted `</ScrollViewer>` into the home page.
- Wrong assumption: nearby generic `StackPanel`/`Border` context was sufficient to place both the opening and closing XAML tags correctly.
- Root cause: the first edit matched the wrong repeated layout shape; the repair used a context hunk that was still not uniquely owned by the C-drive section.
- Detection method: line inspection plus fail-closed XML parsing, which reported the exact mismatched start/end tags; no runtime was needed.
- Fix: renamed the C-drive viewer, added the Agent viewer inside `AgentPage`, removed the misplaced home closing tag, and anchored the C-drive close to the unique `GrowthListBox` block. XML parsing and unique AutomationId/order checks now pass.
- Prevention rule: XAML structural edits must use an owner `x:Name` or another unique local marker in every patch hunk, then parse the whole XAML immediately before any later edit.
- Skill candidate: yes

## 2026-07-14 - Ordinary lag question escaped machine-health intent

- Symptom: full regression classified `电脑为什么卡` as `General` instead of `MachineHealth`.
- Wrong assumption: matching `电脑卡` and `卡顿` also covered natural phrases with words between `电脑` and `卡`.
- Root cause: the phrase matcher uses literal substring containment.
- Detection method: current full regression after dependency recovery; `MachineHealthExperienceTests` failed at the intent assertion.
- Fix: added explicit beginner phrases `为什么卡` and `有点卡`; focused tests passed 5/5 and full regression passed 623/623.
- Prevention rule: intent tests must include ordinary sentence forms, not only compact keywords.
- Skill candidate: no

## 2026-07-14 - Uninstall assertion retained obsolete preview copy

- Symptom: full regression expected `只预览`, while the current recovery checklist correctly returned `先完成恢复准备`.
- Wrong assumption: source-only static checks had kept all product assertions synchronized during the dependency outage.
- Root cause: a prior safety-copy change updated the production flow and nearby tests, but one older assertion remained uncompiled.
- Detection method: current full regression after normal NuGet restore.
- Fix: updated only the stale assertion; the recovery-preparation gate and production code were not weakened.
- Prevention rule: after any compiler outage, run the entire suite and classify failures as product defects versus stale expectations before editing either side.
- Skill candidate: yes

## 2026-07-14 - Parallel audit treated an expected zero-match search as fatal

- Symptom: one multi-command audit returned no collected results because a forbidden-API `rg` correctly found zero matches and exited 1.
- Wrong assumption: the orchestration wrapper would preserve successful sibling results when one command used `rg`'s zero-match exit code.
- Root cause: expected absence was encoded as a raw search rather than an explicit assertion.
- Detection method: the tool call failed with no usable combined receipt.
- Fix: discarded the call and reran each absence check with explicit zero-match success text.
- Prevention rule: wrap expected-zero searches as assertions; never use raw `rg` exit status as the success contract of a parallel audit.
- Skill candidate: yes

## 2026-07-14 - Visible-control audit emitted output after XML traversal errors

- Symptom: the first visible-control inventory printed a plausible 49-row list and then emitted repeated PowerShell method errors while walking from an element to `XmlDocument`.
- Wrong assumption: every parent reached during the loop would remain an `XmlElement`, and non-terminating PowerShell errors would make the command fail.
- Root cause: the loop called `GetAttribute` on the document node and did not set `$ErrorActionPreference = 'Stop'`.
- Detection method: direct inspection of stderr in the same command result; the entire printed inventory was invalidated.
- Fix: reran with terminating errors, an explicit `XmlElement` type guard, buffered output, and a final row-count PASS. The corrected audit listed 49 controls without errors.
- Prevention rule: every PowerShell audit must enable terminating errors and validate node/type preconditions before buffering or printing PASS evidence.
- Skill candidate: yes

## 2026-07-14 - Source-only cache validator captured an out parameter

- Symptom: the first post-antivirus `Css.Tests` build failed with CS1628 in `AppCacheCleanupPlan.TryValidatePath`.
- Wrong assumption: the canonicalized `out` parameter could be read inside `FirstOrDefault` after assignment.
- Root cause: C# forbids capturing `ref`, `out`, or `in` parameters in lambdas even when the value has already been assigned.
- Detection method: the first narrow compiler run after corrected Huorong definitions were installed.
- Fix: copy the canonical path to a normal local and use that local in the predicate, existence check, and ancestor walk.
- Prevention rule: source-only edits must not pass `ref`/`out`/`in` parameters into lambdas; copy to a local before any LINQ or callback boundary.
- Skill candidate: no

## 2026-07-14 - FluentAssertions expression trees used pattern matching

- Symptom: after product projects compiled, `Css.Tests` failed with three CS8122 errors in `OnlyContain` predicates.
- Wrong assumption: `is null` would compile everywhere a normal C# predicate compiles.
- Root cause: these FluentAssertions overloads capture expression trees, which do not support pattern-matching operators.
- Detection method: the second narrow compiler run after the antivirus gate reopened.
- Fix: replaced the three null patterns with equivalent `== null` comparisons.
- Prevention rule: assertions captured as expression trees should use expression-tree-compatible equality and boolean operators, not pattern matching.
- Skill candidate: no

## 2026-07-14 - App-drawer smoke did not tolerate a UIAutomation server fault

- Symptom: `gui-app-drawer-preview-smoke.ps1` stopped with `RPC_E_SERVERFAULT` while enumerating secondary windows after opening a drawer preview.
- Wrong assumption: root-level UIAutomation `FindAll` would always return a collection or a normal empty result.
- Root cause: WPF/UIAutomation can transiently throw a COM server fault while the visual tree is changing, and the helper guarded individual windows but not the initial enumeration.
- Detection method: the first complete post-antivirus run of the existing app-drawer smoke.
- Fix: catch `COMException` around only the optional secondary-window enumeration and return zero closed dialogs; main-window preview assertions remain mandatory.
- Prevention rule: optional UIAutomation discovery helpers may tolerate transient COM enumeration faults, but required proof elements must still fail closed through separate AutomationId assertions.
- Skill candidate: yes

## 2026-07-14 - App-drawer smoke expected obsolete preview titles

- Symptom: after the UIAutomation retry fix, the drawer smoke stopped because cache showed `缓存清理方案` while the script required `缓存清理预览`; startup had the same stale `自启动预览` expectation versus `自启动检查`.
- Wrong assumption: old generic preview titles still represented the newer evidence-aware cache and Windows handoff flows.
- Root cause: product presenters evolved from generic read-only previews to explicit plan/check states, but the GUI smoke's localized title table was not updated.
- Detection method: required title assertion in the real post-antivirus WPF run, followed by direct presenter inspection.
- Fix: align the smoke with the current beginner-facing `缓存清理方案` and `自启动检查` titles; all safety/body/list assertions remain unchanged.
- Prevention rule: GUI smokes should assert current user-facing outcome states and safety fields together; when a title changes, inspect the presenter before deciding whether product or smoke is stale.
- Skill candidate: no

## 2026-07-14 - WPF startup action lookup used the retired label

- Symptom: the isolated app fixture contained an ordinary startup entry, the presenter exposed an enabled `管理自启动` action, but `DrawerDisableStartupButton` remained disabled in the real WPF app.
- Wrong assumption: changing the beginner action label in the presenter was sufficient to update the drawer button state.
- Root cause: `ShowAppDrawer` still called `ApplyActionState` with the old `关闭自启动` lookup key, so exact label matching returned no action.
- Detection method: deterministic four-action GUI smoke after removing real-machine dependency, followed by WPF/presenter source comparison.
- Fix: change the WPF lookup key to `管理自启动` and add a source contract that requires presenter/WPF key equality and rejects the retired key.
- Prevention rule: when labels act as internal lookup keys, bind through a shared id or at minimum add a cross-layer contract test; visible-copy changes must not silently disable controls.
- Skill candidate: yes

## 2026-07-14 - Focused test accidentally ran restore and invalidated NuGet assets

- Symptom: an installer-focused `dotnet test` ran for 108 seconds and failed with `NU1301`; subsequent `--no-restore` builds also failed, and an offline `--ignore-failed-sources` restore replaced the errors with missing-package `NU1101` failures.
- Wrong assumption: the focused test command included the same `--no-build`/`--no-restore` protection as the preceding verified command, and a failed restore would leave the prior `obj/project.assets.json` state usable.
- Root cause: `--no-build --no-restore` was omitted from the command. NuGet attempted blocked network access and rewrote generated restore assets with a failed graph; the local cache did not contain every required package/source record.
- Detection method: the long-running command ended before test discovery with `NU1301`; the next no-restore build immediately read the same errors; the offline repair then reported explicit `NU1101` package gaps.
- Fix: stopped treating all affected build/test outputs as code evidence, requested one normal network restore, and did not bypass the request after Codex approval was rejected by usage quota. Existing pre-incident assemblies were used only for clearly labeled compiled-regression evidence; current new tests remain uncompiled.
- Prevention rule: after dependencies are already restored, every test command in this repository must explicitly use both `--no-build` and `--no-restore` unless a deliberate restore is the named objective. If restore starts unexpectedly, terminate it before it can rewrite assets. Never run `--ignore-failed-sources` as a repair unless every required package is proven available in an offline source.
- Skill candidate: yes

## 2026-07-14 - Installer static audit looked for the pipeline in the handler file

- Symptom: the first fail-closed installer static audit reported `planner or pipeline gate missing` even though the last compiled tests had already exercised the pipeline.
- Wrong assumption: `SafetyOperationPipeline` construction lived in `InstallerLaunchOperation.cs` beside the handler.
- Root cause: the coordinator owns pipeline composition; the handler file owns descriptor and launch validation.
- Detection method: the explicit assertion failed before any PASS output, followed by inspection of `InstallerExecutionCoordinator.cs`.
- Fix: kept the failed result invalid and reran with separate known-file assertions for planner/handler and coordinator composition.
- Prevention rule: discover symbol ownership before combining cross-file static invariants; a fail-closed audit must not turn an ownership guess into a product finding.
- Skill candidate: no

## 2026-07-14 - Health presentation patch contained mojibake

- Symptom: strict UTF-8 reads succeeded, but newly touched health-summary and Home Agent strings displayed as malformed Chinese byte-decoding artifacts.
- Wrong assumption: valid UTF-8 encoding alone proved the user-visible text was intact after the interrupted patch.
- Root cause: malformed text was already present in the patch content; encoding validity cannot detect semantically corrupted Unicode.
- Detection method: read the exact touched files with explicit UTF-8 output and scan for known mojibake signatures before WPF wiring acceptance.
- Fix: replaced the affected presentation files with clean Chinese text and added a touched-file mojibake signature gate.
- Prevention rule: after any patch containing Chinese UI text, run both strict UTF-8 decoding and a semantic mojibake signature scan before recording PASS.
- Skill candidate: yes

## 2026-07-14 - Windows wildcard was passed directly to rg

- Symptom: `rg` rejected `tests\Css.Tests\*Tests.cs` as an invalid Windows path.
- Wrong assumption: PowerShell would expand the wildcard for `rg` in that argument position.
- Root cause: `rg` received the literal wildcard path on Windows.
- Detection method: immediate command error with OS error 123.
- Fix: searched the directory and let `rg` recurse.
- Prevention rule: pass directories or `-g` globs to `rg`; do not rely on shell expansion of Windows path wildcards.
- Skill candidate: no

## 2026-07-14 - Windows PowerShell could not load SDK Roslyn assemblies

- Symptom: the attempted read-only C# syntax pass failed in `Add-Type` with `ReflectionTypeLoadException` before parsing any source.
- Wrong assumption: Windows PowerShell's .NET Framework host could directly load the current .NET SDK CoreCLR Roslyn binaries.
- Root cause: the host/runtime and Roslyn dependency load contexts are incompatible.
- Detection method: `Add-Type` failed before any syntax tree was created or PASS output was printed.
- Fix: discarded the check, retained explicit XAML/UTF-8/source-policy gates, and left type/syntax compilation as Warn until the normal project restore is available.
- Prevention rule: do not claim SDK Roslyn parsing from Windows PowerShell unless a compatible host is already established; use the real project compiler after restore.
- Skill candidate: no

## 2026-07-14 - Default PowerShell decoding made valid UTF-8 look corrupted

- Symptom: `Get-Content` displayed migration result and final-consent Chinese as mojibake, while strict UTF-8 and `rg` found the intended Chinese text.
- Wrong assumption: default Windows PowerShell text decoding would preserve UTF-8 source without an explicit encoding.
- Root cause: the host's default decoding did not match the UTF-8 files; a later audit also tried to match decoded Chinese against literal `\uXXXX` source escapes.
- Detection method: repeated the reads with `Get-Content -Encoding utf8`, searched exact Chinese with `rg`, and compared the source escape representation.
- Fix: made no product edit for the false mojibake report; all subsequent source reads use explicit UTF-8, and escaped-source audits match escaped text.
- Prevention rule: use explicit UTF-8 for PowerShell source reads and distinguish rendered text from C# escape syntax before recording a copy defect.
- Skill candidate: yes

## 2026-07-14 - Repeated wildcard and quoting mistakes interrupted source audits

- Symptom: `rg` received literal Windows wildcard paths, one combined search failed on quoting, and a substring audit accidentally operated on a character array.
- Wrong assumption: PowerShell wildcard/quote/slice behavior matched shell or C# expectations in long one-line audits.
- Root cause: mixed command-language conventions and overly broad compound audit commands.
- Detection method: non-zero command results and incomplete output before any PASS claim.
- Fix: split audits into directory-based `rg` calls and small explicit-encoding checks; discarded every failed result.
- Prevention rule: use directories plus `-g` for `rg`, keep PowerShell audit steps short, and never cite partial output after a non-zero exit.
- Skill candidate: yes
# 2026-07-14 - Static audit reused PowerShell's reserved Host variable

- Symptom: the first combined closure invariant audit stopped before producing valid results because assigning `$host` raised a read-only-variable error.
- Wrong assumption: `$host` was an ordinary local variable name.
- Root cause: PowerShell exposes `$Host` as a built-in read-only automatic variable and variable names are case-insensitive.
- Detection method: immediate non-zero command output before any PASS result.
- Fix: discarded the failed audit and reran it with `$drawerHost`; all scoped checks then completed successfully.
- Prevention rule: do not use PowerShell automatic-variable names (`Host`, `Error`, `Args`, and similar) for audit temporaries; failed compound audits provide no evidence.
- Skill candidate: no
# 2026-07-14 - Compound rg audits repeated a known PowerShell quoting failure

- Symptom: two exploratory `rg` commands were parsed as pipelines or unterminated strings before a valid search ran.
- Wrong assumption: backslash-escaped double quotes would protect regex alternation in Windows PowerShell as they do in other shells.
- Root cause: the command repeated a quoting pattern already warned about in this ledger instead of using one single-quoted regex argument.
- Detection method: immediate parser/path errors and no valid search evidence.
- Fix: discarded both failed outputs and reran each search with one single-quoted pattern.
- Prevention rule: before running a PowerShell `rg` alternation, use the established form `rg -n 'a|b|c' directory`; do not improvise escaped double-quote forms in long commands.
- Skill candidate: yes

## 2026-07-15 - Migration wiring test assumed the next click handler was async

- Symptom: migration closure behavior tests passed, but the static MainWindow wiring test could not find its extraction end marker.
- Wrong assumption: `PreviewCacheCleanup_Click` was declared `async void` like the neighboring migration and startup handlers.
- Root cause: cache preview is synchronous and is declared `private void`.
- Detection method: the failure reported an absent end marker; `rg` of the handler declarations showed the exact signature.
- Fix: use the real synchronous handler signature as the extraction boundary.
- Prevention rule: inspect adjacent method declarations before writing source-extraction tests; do not infer `async` from neighboring handlers.
- Skill candidate: no.

## 2026-07-15 - Combined migration implementation patch used a fragile shared context

- Symptom: the first atomic Core/WPF migration-closure implementation patch was rejected at the plan-method hunk.
- Wrong assumption: one large patch could reliably distinguish two nearby methods containing the same `drawer`, `migrationClosure`, and presentation calls.
- Root cause: the multi-method patch used repeated local context across `ShowAppDrawer` and `ShowMigrationPlanAsync`.
- Detection method: `apply_patch` failed atomically; re-reading both method bodies confirmed no source hunk had been written.
- Fix: split the Core presenter, drawer binding, and plan-entry guard into separate exact patches.
- Prevention rule: patch similar neighboring methods independently and include the method signature in every hunk context.
- Skill candidate: no.

## 2026-07-15 - Record-closing patch assumed the wrong decision-log heading

- Symptom: the first atomic record-closing patch for residue-review availability was rejected before writing any file.
- Wrong assumption: the decisions file used the generic heading `# Decisions`.
- Root cause: the repository heading is `# Decision Log`, while the multi-file patch required an exact context match.
- Detection method: `apply_patch` reported the missing expected heading; inspection confirmed the patch was atomic and no intended status line changed.
- Fix: re-read the record headers and apply the updates with their exact stable headings.
- Prevention rule: inspect the first lines of every protocol record before a multi-file insertion instead of inferring its title from the filename.
- Skill candidate: no.

## 2026-07-15 - Summary wiring assertion scanned unrelated MainWindow code

- Symptom: the first green run left one static test failure because all of `MainWindow.xaml.cs` still contained `CDriveWritePaths.Count > 0`.
- Wrong assumption: that expression existed only in the removed app-catalog summary.
- Root cause: an unrelated legacy technical `SoftwareProfileView` still uses the field for its own detailed line.
- Detection method: behavior tests passed; the failure output showed the hit inside the unrelated nested technical view.
- Fix: scope the no-private-counter assertion to `RefreshAppCatalog` while separately proving the old `BuildSoftwareSummary` method is absent.
- Prevention rule: source-wiring tests on large UI files must extract the target method before asserting absence of common model expressions.
- Skill candidate: no.

## 2026-07-15 - Catalog summary patch assumed a block-method closing shape

- Symptom: the first multi-file summary implementation patch was rejected at the insertion point before `AppPresentationBuilder`.
- Wrong assumption: the preceding presenter method ended with a normal nested block matching the broad patch context.
- Root cause: `RiskOrder` is expression-bodied and ends with `};`, so the expected generic closing sequence did not match exactly.
- Detection method: `apply_patch` verification failed atomically; re-reading the boundary confirmed no source hunk was written.
- Fix: split the implementation into exact boundary, method-local predicate, and MainWindow patches.
- Prevention rule: inspect the exact five-to-ten lines around a class-boundary insertion and avoid combining it with unrelated method replacements.
- Skill candidate: no.

## 2026-07-15 - Authority scan covered an unrelated large test class

- Symptom: a first static authority scan reported 67 hits after the normal-filter rename even though the changed test contained no authority.
- Wrong assumption: scanning all of `ProductExperienceTests.cs` would represent the one newly added method.
- Root cause: the file contains many unrelated historical safety tests that intentionally mention operation and process APIs.
- Detection method: inspected the changed method range and reran the same patterns only there; hits were 0.
- Fix: recorded the bounded changed-method result rather than the unrelated whole-file count.
- Prevention rule: for shared omnibus test files, scope static authority checks to the changed method or use a dedicated focused test file.
- Skill candidate: no.

## 2026-07-15 - Worklog header was assumed instead of inspected

- Symptom: the first active-slice record patch was rejected because it expected `# Worklog`, while the file uses `# Development Worklog`.
- Wrong assumption: the record file header matched a generic name from memory.
- Root cause: the patch was prepared before reading the exact current header.
- Detection method: `apply_patch` verification failed atomically; inspection confirmed no partial record change.
- Fix: re-read the file header and reapplied the record update with exact context.
- Prevention rule: inspect the first lines of every protocol record before a multi-file insertion, even when its filename is familiar.
- Skill candidate: no.

## 2026-07-15 - Broad AppPresentation patch assumed a different method order

- Symptom: the unknown-system ownership patch was rejected because the expected `CDriveAttentionTag` context did not match the current file order.
- Wrong assumption: one broad patch could safely span several methods after recent edits without re-reading their exact order.
- Root cause: the file places the tile-label helper before `CreateDrawer`, while the patch context assumed a later placement.
- Detection method: `apply_patch` verification failed atomically before any source hunk was written.
- Fix: re-read symbol line positions and apply narrow method-local patches.
- Prevention rule: after multiple edits to one large file, patch one method or adjacent block at a time using freshly inspected context.
- Skill candidate: no

## 2026-07-15 - Collection expression was nested inside a fixed array initializer

- Symptom: the new installer-placement tests did not compile; the compiler tried to interpret `.. agent.NextSteps` as an array index/range expression.
- Wrong assumption: collection spread syntax could be mixed into an explicitly constructed `new[]` initializer.
- Root cause: C# collection expressions support spread elements, but a traditional array initializer does not.
- Detection method: the first focused red-test build failed with CS0029 and CS0826 before production code changed.
- Fix: construct the fixed scalar array and append `NextSteps` through `Concat`.
- Prevention rule: use `[item, .. sequence]` only when the whole expression is a collection expression; otherwise compose `IEnumerable<T>` explicitly.
- Skill candidate: no

Use this file to record mistakes that should not be repeated.

### 2026-07-15 - Unknown-state test prohibited its own safety phrase

- Symptom: the first focused growth test failed because the expected answer contained `不会把未知说成正常` while the same assertion prohibited the substring `正常`.
- Wrong assumption: banning one generic keyword would prove that unavailable evidence was not presented as healthy.
- Root cause: the safety sentence necessarily uses that keyword in a negated statement.
- Detection method: focused `Missing_or_mismatched_observation_remains_unknown` failure with 5/6 tests passing.
- Fix: assert the absence of actual false conclusions such as `没有问题` and `一切正常` while retaining the explicit unknown-state sentence.
- Prevention rule: presentation tests must assert semantic false claims, not ban a keyword that may appear inside a negation or safety explanation.
- Skill candidate: no

### 2026-07-15 - Related handoff suite omitted an uninstall source contract

- Symptom: focused and selected related tests passed, but full regression failed one static assertion that still expected `selected.Profile` inside the pre-refactor uninstall handler.
- Wrong assumption: the selected Agent/app/uninstall test filter covered every source contract affected by extracting the shared uninstall preview method.
- Root cause: `OfficialUninstallProductionExecutionCoordinatorTests` was omitted even though it verifies MainWindow's captured-profile residue-review wiring.
- Detection method: first full regression after the Agent application-action handoff, with 825/826 tests passing.
- Fix: update the assertion to require both `ShowUninstallPlanAsync(selected.Profile)` and `ReviewUninstallResidueAsync(profile, refreshedProfiles)`, then rerun the failed contract, focused handoff tests, and full suite.
- Prevention rule: any refactor of uninstall MainWindow source must include `OfficialUninstallProductionExecutionCoordinatorTests` in the related filter, even when execution behavior itself is unchanged.
- Skill candidate: no

### 2026-07-15 - Audit assumed an OperationResult filename without discovery

- Symptom: a read command failed because `src/Css.Core/Operations/OperationResult.cs` does not exist.
- Wrong assumption: the public type name matched a standalone source filename.
- Root cause: `OperationResult` is defined in a differently named operations source file.
- Detection method: direct `Get-Content` path error during the typed-error follow-up audit.
- Fix: use `rg -l "class OperationResult|record OperationResult"` or search the type definition before reading a guessed path.
- Prevention rule: discover source locations by symbol before opening files when the repository does not follow one-type-per-file naming consistently.
- Skill candidate: no

### 2026-07-15 - Resource phrase test omitted an internal-space variant

- Symptom: the runtime-target test recognized `CPU过高` but returned no target for `CPU 过高`.
- Wrong assumption: the existing case-insensitive phrase matching also normalized internal whitespace.
- Root cause: `ContainsAny` deliberately performs literal substring matching so application names and other question text are not globally rewritten.
- Detection method: focused natural-language boundary test for an exact named app resource question.
- Fix: add the bounded spaced variants for CPU high/too-high wording while preserving the existing exact subject/profile matching.
- Prevention rule: phrase-list tests for Latin abbreviations in Chinese sentences should include both attached and single-space forms; do not globally remove whitespace from identity-bearing input.
- Skill candidate: no

### 2026-07-15 - Whole-solution restore still waited on the blocked public source

- Symptom: `dotnet restore ComputerSecuritySoftware.slnx --ignore-failed-sources` repeatedly waited on NuGet.org and timed out after two minutes even though the new package already existed in the global package cache.
- Wrong assumption: `--ignore-failed-sources` would avoid network waits and complete from the global package cache.
- Root cause: NuGet still attempted the only configured public source for every project before treating it as unavailable.
- Detection method: restore output showed repeated `NU1801` messages for `https://api.nuget.org/v3/index.json` until the command timeout.
- Fix: restore the affected Win32 and test projects with the global package directory supplied as the only source; both completed from local `.nupkg` files.
- Prevention rule: for a confirmed cached package in this restricted workspace, use a targeted restore with the global package directory as the sole source instead of probing the public source across the whole solution.
- Skill candidate: yes

### 2026-07-15 - Real process-image test compared sandbox alias strings

- Symptom: full regression failed because `Environment.ProcessPath` and `QueryFullProcessImageName` returned different absolute strings for the current test image.
- Wrong assumption: two APIs observing the same executable must return the same path string under a workspace/sandbox mapping.
- Root cause: the managed process path can expose a mapped workspace alias while the Win32 process query returns the kernel-visible path; both files have the same image name and SHA-256.
- Detection method: full regression after the natural-language diagnosis change, followed by an isolated deterministic repro in the mapped sandbox.
- Fix: keep product exact-path/hash verification unchanged; make the real inspector test require an existing resolved file, matching executable filename, and the same SHA-256 as the current process image.
- Prevention rule: tests that prove OS-observed file identity across sandbox or virtualization boundaries must not require alias-sensitive path-string equality; bind to cryptographic content and relevant stable identity fields.
- Skill candidate: yes

### 2026-07-15 - Static authority check used a newer string overload

- Symptom: the Agent authority gate stopped because Windows PowerShell could not resolve `String.Contains(value, StringComparison)`.
- Wrong assumption: the shell runtime exposed the same string overloads as the .NET target used by the application.
- Root cause: the verification command ran under Windows PowerShell compatibility semantics.
- Detection method: visible `MethodCountCouldNotFindBest` error in the static gate output.
- Fix: use `IndexOf(value, StringComparison) -ge 0`, which is available in the verification runtime.
- Prevention rule: keep ad hoc Windows PowerShell gates compatible with the host runtime and reject any run containing a shell error even if later output appears.
- Skill candidate: no

### 2026-07-15 - Async failure assertion lacked an explicit delegate type

- Symptom: the first red test failed to compile with no `ThrowAsync` extension on an inferred `FunctionAssertions<?>`.
- Wrong assumption: `var` would infer the zero-argument async lambda as `Func<Task>` in the FluentAssertions call chain.
- Root cause: the lambda had no target delegate type.
- Detection method: compiler error in `AutomaticAppInventoryLoadingTests` before product implementation.
- Fix: declare the assertion subject as `Func<Task>`.
- Prevention rule: give async action delegates an explicit `Func<Task>` type before using `ThrowAsync`.
- Skill candidate: no

### 2026-07-15 - Static source tests used an obsolete exact method signature

- Symptom: two product tests could not extract the C-drive handler or the method following `OpenAppDrawerTarget` after async lazy loading was introduced.
- Wrong assumption: exact neighboring method signatures were stable source anchors.
- Root cause: the handler legitimately changed from `void` to `async void`, and `RunSoftwareScanAsync` was split into ensure/refresh/core methods.
- Detection method: related product test failures reported missing start/end markers while runtime-focused tests passed.
- Fix: update anchors to the new async signature and `EnsureSoftwareInventoryLoadedAsync`, preserving the same authority assertions.
- Prevention rule: static source order tests should anchor to stable method names or semantic tokens; when an async transition is required, update the anchor without removing safety assertions.
- Skill candidate: yes

### 2026-07-15 - Existing WPF fixture omitted the new explicit trust prerequisite

- Symptom: the related test run failed because a ready uninstall draft kept the final-confirmation button collapsed.
- Wrong assumption: injecting no production coordinator/readiness into the old window fixture should still represent a trusted package.
- Root cause: the new fail-closed constructor default correctly treats missing readiness as unavailable.
- Detection method: `UninstallPlanFinalConsentEntryTests.Ready_verified_draft_exposes_continue_but_not_a_run_control` failed after the production-readiness gate was added.
- Fix: pass an explicit trusted readiness model in the positive fixture and add a separate test proving omitted readiness remains blocked.
- Prevention rule: security-sensitive test fixtures must declare trust/readiness explicitly; never make production code default to trusted to preserve an old test.
- Skill candidate: no

### 2026-07-15 - Static-script discovery assumed a missing repository directory

- Symptom: `rg --files .omx scripts` returned an error because the repository has no `scripts` directory.
- Wrong assumption: static verification helpers were stored under a conventional `scripts` folder.
- Root cause: helper artifacts in this repository live under `.omx`, and no matching reusable verifier exists.
- Detection method: `rg` reported `scripts: 系统找不到指定的文件`.
- Fix: run bounded PowerShell static checks directly against existing `src` and `tests` roots.
- Prevention rule: enumerate repository roots before including optional paths in search commands.
- Skill candidate: no

### 2026-07-15 - Windows `rg` source wildcard was passed as a path

- Symptom: the capability audit returned exit code 1 and `文件名、目录名或卷标语法不正确` for `src\Css.App\*Window.xaml`.
- Wrong assumption: `rg` on Windows would accept shell-style wildcards in positional path arguments.
- Root cause: PowerShell did not expand the wildcard into valid paths for this invocation, and `rg` received an invalid Windows path expression.
- Detection method: direct command output named the invalid path arguments.
- Fix: rerun against the directory and use `-g '*Window.xaml'` / `-g '*Window.xaml.cs'` filters.
- Prevention rule: use `rg -g` globs for repository file filtering; keep positional arguments as real directories or files.
- Skill candidate: no

## 2026-07-15 - Startup fixture byte parser rejected PowerShell UTF-8 BOM

- Symptom: the first startup-control GUI smoke could not find the main window because `Css.App` terminated during `MainWindow` field initialization.
- Wrong assumption: `JsonSerializer.Deserialize(ReadOnlySpan<byte>)` would accept the UTF-8 BOM written by Windows PowerShell 5.1 `Set-Content -Encoding UTF8`.
- Root cause: the byte-span JSON reader treated `0xEF` as an invalid first token; unlike text reading, it did not remove the BOM.
- Detection method: queried the recent `.NET Runtime` Application event after the smoke failed and found the exact stack trace in `StartupEntryControlFixtureStore.TryCreate`.
- Fix: kept the bounded file check but deserialize from `File.ReadAllText`, and changed the fixture unit test to write an explicit UTF-8 BOM.
- Prevention rule: development fixture readers used by Windows PowerShell smokes must test UTF-8 BOM input; after a GUI process vanishes before UIAutomation discovery, inspect process exit and runtime events before changing window-search logic.
- Skill candidate: yes

## 2026-07-15 - Startup experience test repeated malformed quoted command syntax

- Symptom: the first new startup experience test did not compile because a verbatim C# string mixed backslash escaping with quoted executable syntax.
- Wrong assumption: `@\"...` style escaping was valid inside a verbatim string.
- Root cause: ordinary and verbatim C# string escaping rules were mixed, repeating an earlier startup-test mistake.
- Detection method: the intended TDD run stopped on C# parser errors before reaching the missing-feature failures.
- Fix: replaced it with a normal escaped string and reran until the genuine missing presenter/overload RED appeared.
- Prevention rule: use one established helper/constant for quoted Windows commands in startup tests; syntax failures are not valid product RED evidence.
- Skill candidate: yes

## 2026-07-15 - Concrete fixture and Win32 stores did not coalesce without interface typing

- Symptom: `CreateStartupEntryControlStore` failed compilation on `fixture ?? productionStore`.
- Wrong assumption: the expression-bodied method's interface return type would provide target typing across two unrelated concrete operands.
- Root cause: null-coalescing requires an implicit conversion between operands before the outer return conversion.
- Detection method: focused fixture test build reported CS0019.
- Fix: explicitly cast the nullable fixture operand to `IStartupEntryControlStore?`.
- Prevention rule: when selecting between unrelated concrete implementations, type the first operand or use an interface-typed local before `??`.
- Skill candidate: no

## 2026-07-15 - Personal-storage GUI smoke passed while the useful content was offscreen

- Symptom: the first smoke reported exact navigation success, but the screenshot showed only the personal-storage heading/summary at the bottom; the candidate cards were still below the viewport.
- Wrong assumption: visibility of the summary and list container proved that the requested candidate content was usable.
- Root cause: navigation called `BringIntoView` on the summary, and the smoke did not require the list items themselves to be onscreen.
- Detection method: manual inspection of `.omx/qa-personal-storage-candidates.png` after the mechanically passing run.
- Fix: navigate to the list, increase its stable height, and require both fixture list items to have `IsOffscreen=false` before capture.
- Prevention rule: for beginner conclusion panels, assert stable item AutomationIds, item-level onscreen state, and a screenshot of the actual content, not only the section container.
- Skill candidate: yes
