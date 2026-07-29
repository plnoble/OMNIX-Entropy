# Archived current (2026-07-07 to 2026-07-12)

Historical entries moved out of `.omx/development/current.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

## 2026-07-07 - Active Slice: C Drive Page Real-Scan UX Verification

- Objective: Verify the C drive page after a real scan, focusing on whether beginner-facing summary cards, growth cards, and recommendation cards are readable, non-technical by default, and clearly preview-only before any cleanup.
- Dependencies: Existing WPF app, local C-drive scanner, UIAutomation GUI smoke path, ProductExperienceTests.
- Risks: Real scan may touch large directories; verification must not execute cleanup, migration, uninstall, service, startup, or registry changes. GUI launch requires escalation.
- Impact scope: C-drive page presentation and tests only unless verification reveals a blocker.
- Acceptance criteria: Left navigation opens C-drive page, scan completes, user-facing summary/growth/recommendation text is visible without path-heavy clutter, buttons remain preview/confirmation gated, and no system-changing operation is invoked.
- Next action: Run a real-scan GUI smoke for the C-drive page, capture evidence, then fix any visible beginner UX issue with TDD.

## 2026-07-07 - Active Slice: C Drive Recommendation Grouping

- Objective: Reduce noisy repeated C-drive recommendation cards and explain why low-risk cleanup moves items to quarantine first.
- Dependencies: `RecommendationCardPresenter`, C-drive scan recommendations, WPF recommendation list binding, ProductExperienceTests.
- Risks: Grouping must not hide executable cleanup operations or bypass the existing safety pipeline.
- Impact scope: C-drive recommendation presentation only; no scanner, cleanup, quarantine, or execution behavior should change.
- Acceptance criteria: Repeated "confirm source first" observe findings become one beginner-readable group, executable cleanup cards remain actionable through the existing pipeline, and the action area explains quarantine as a reversible undo step rather than permanent deletion.
- Status: Implemented and verified at unit/build level. `RecommendationListPresenter` groups repeated unexpected-root observe recommendations, preserves low-risk cleanup operations, and supplies a clearer quarantine explanation for the C-drive action area.
- Last verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_recommendation_list_groups_repeated_observe_items_and_explains_quarantine` passed 1/1; `ProductExperienceTests` passed 54/54; full suite passed 111/111; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Next action: Run a real C-drive GUI scan to check grouped card wrapping/selection behavior, then continue reducing beginner-facing noise in the C-drive cleanup page.

## 2026-07-07 - Active Slice: GUI Proof for Grouped C-drive Cards

- Objective: Verify the grouped C-drive recommendation cards in the actual WPF layout after a real read-only C-drive scan.
- Dependencies: Built `Css.App.exe`, UIAutomation, local C-drive scanner.
- Risks: Real scan may take time and GUI launch requires escalation; verification must not click the cleanup execution button.
- Impact scope: Verification first; only presentation fixes if the grouped card is unreadable or hard to select.
- Acceptance criteria: C-drive scan completes, grouped "needs source confirmation" card is visible, quarantine explanation is visible, low-risk cleanup remains selectable but not executed, and no system-changing operation is invoked.
- Status: Implemented and verified. The grouped card appears after a real read-only C-drive scan; long card text wraps without a horizontal scrollbar; the execution button starts disabled and remains disabled for non-executable observe cards.
- Last verification: `ProductExperienceTests` passed 56/56; full suite passed 113/113; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. GUIA found grouped card and quarantine explanation, with execute button `IsEnabled=False`; screenshot `.omx\qa-cdrive-grouped-button-disabled.png`.
- Next action: Continue app-management safety loop: add lightweight proof for post-uninstall residue-review inline short-circuit, or continue making C-drive low-risk cleanup selection clearer without enabling new destructive behavior.

## 2026-07-07 - Active Slice: Uninstall Residue Inline Review UX

- Objective: Make the app drawer's post-uninstall residue review understandable and testable without relying on a slow full real-machine scan.
- Dependencies: `UninstallResidueReviewPlanner`, `UninstallResidueReviewViewModel`, app drawer WPF bindings, ProductExperienceTests or UninstallResidueScanTests.
- Risks: Must not run official uninstallers, delete residue, move files, or suggest cleanup while the app is still installed.
- Impact scope: App drawer presentation and view-model state only; no new system-changing execution path.
- Acceptance criteria: A still-installed app produces an inline Agent result that says residue cleanup is blocked, hides technical path details by default, disables cleanup, and tells the user to run official uninstall first.
- Status: Implemented and verified. The app drawer now shows `残留检查结果` directly under uninstall actions, keeps still-installed residue cleanup blocked, hides local paths in the beginner result, wraps text without horizontal scrolling, and no longer refreshes the inline result away.
- Last verification: `UninstallResidueScanTests` passed 9/9; `ProductExperienceTests` passed 59/59; full suite passed 117/117; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. GUIA read-only app scan found 130 app tiles, selected `火绒安全软件`, clicked `卸载后检查残留`, and found `残留检查结果`, still-installed, official-uninstall-first, and no-file-move safety text. Screenshot `.omx\qa-residue-review-inline-wrapped.png`.
- Next action: Continue app-management safety loop: make the official-uninstall safety window and app drawer share one clearer "what happens next" flow, while keeping real uninstaller execution disabled.

## 2026-07-07 - Active Slice: Shared Uninstall Next-Step Flow

- Objective: Make the app drawer `卸载干净点` preview and the uninstall safety window use the same beginner-readable "what happens next" flow.
- Dependencies: `AppPresentationBuilder.CreateUninstallPreview`, `UninstallPlanPresentationBuilder`, `UninstallPlanWindow`, ProductExperienceTests.
- Risks: Must not enable real official uninstaller execution or residue cleanup; this is presentation/planning only.
- Impact scope: Uninstall preview/window presentation only.
- Acceptance criteria: Both drawer preview and safety window describe the same sequence: review official uninstaller, close app, run official uninstall only after future confirmation, come back to residue review, move only low-risk residue to quarantine, explain high-risk residue. Execution remains disabled.
- Next action: Add a failing product test for a shared uninstall workflow guide, then implement and wire it into both surfaces.
## 2026-07-07 - Active Slice Update: Shared Uninstall Next-Step Flow

- Objective: Make the app drawer `卸载干净点` preview and the uninstall safety window use one beginner-readable next-step flow.
- Status: Implemented and verified at unit/build level. `UninstallWorkflowGuidePresenter` now drives both drawer uninstall preview and the safety-window `WorkflowGuide`; `UninstallPlanWindow.xaml` renders that shared guide above the detailed preflight cards.
- Last verification: TDD red observed for missing `UninstallWorkflowGuidePresenter` and missing `WorkflowGuide` on `UninstallPlanPreviewViewModel`; focused shared-flow test passed 1/1; `ProductExperienceTests` passed 60/60; full suite passed 118/118; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: Real-click GUI modal proof is pending. UIA diagnostic selected `火绒安全软件` and found `DrawerUninstallButton` enabled, but `InvokePattern` did not open the modal; real mouse-click retry was rejected by the usage-limit approval system. No workaround was attempted.
- Next action: When approvals are available, rerun a real-click app-drawer GUI smoke for the uninstall safety modal. Meanwhile continue improving C-drive low-risk cleanup selection clarity without enabling new destructive behavior.
## 2026-07-07 - Active Slice Update: C-drive Cleanup Selection Clarity

- Objective: Make selecting a low-risk C-drive cleanup card explain the exact next step before the user clicks any execution button.
- Risks: Must not expand the set of executable recommendations, bypass quarantine, or weaken confirmation requirements.
- Impact scope: C-drive recommendation presentation and WPF selection state only.
- Acceptance criteria: Actionable low-risk cleanup selection shows beginner-readable text that it will move candidates to OMNIX-Entropy quarantine, require another confirmation, and can be restored from the undo center; non-executable observe selections keep the execute button disabled.
- Next action: Add a failing product test for selected cleanup action preview text, then implement without adding new execution paths.
## 2026-07-07 - Active Slice Update: C-drive Cleanup Selection Preview

- Objective: Make selecting a low-risk C-drive cleanup card explain the exact next step before the user clicks any execution button.
- Status: Implemented and verified at unit/build level. `RecommendationSelectionPresenter` now produces selection-state button text and explanation for no-selection, non-executable card, and low-risk actionable cleanup card states. `RecommendationsListBox_SelectionChanged` consumes this presenter.
- Last verification: TDD red observed for missing `RecommendationSelectionPresenter`; focused selection tests passed 2/2; `ProductExperienceTests` passed 62/62; full suite passed 120/120; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: No GUI visual pass for selected C-drive cards because earlier GUI escalation hit the usage-limit approval rejection.
- Known cleanup: `RecommendationsListBox_SelectionChangedLegacy` remains unreferenced in `MainWindow.xaml.cs` until a safer mojibake/UTF-8 code-behind cleanup pass.
- Next action: GUI-verify actionable/non-actionable C-drive recommendation selection when approvals allow, or do a focused code-behind cleanup to remove legacy selection code without changing behavior.
## 2026-07-08 - Active Slice: Agent Next-Step Panel

- Objective: Make the AI Agent page provide beginner-readable next-step recommendations from local health/app signals instead of only showing a static skill catalog.
- Dependencies: `AgentSkillCatalog`, `HealthCheckSummary`, app/software summary state, `MainWindow.xaml`, `ProductExperienceTests`.
- Risks: Must remain local rules only; no cloud AI, no direct deletion, migration, uninstall, service/startup, scheduled-task, registry, or installer execution.
- Impact scope: Agent page presentation and local presenter only.
- Acceptance criteria: Agent page can show a top recommendation, supporting reasons, safe next actions, and blocked actions; recommendations are non-executable unless they become local operation plans through existing pipelines.
- Next action: Add a failing product test for an `AgentNextStepPresenter`, then wire it into the Agent page.

## 2026-07-08 - Active Slice Update: Agent Next-Step Panel

- Objective: Make the AI Agent page provide beginner-readable next-step recommendations from local health/app signals.
- Status: Implemented and verified at unit/build level. `AgentNextStepPresenter` now turns `HealthCheckSummary` and `SoftwareProfile` signals into a top suggestion, reasons, safe next actions, blocked actions, privacy text, and `CanExecuteDirectly=false`.
- UI state: `MainWindow` stores `_lastHealthSummary`, refreshes the Agent panel on startup, after C-drive scans, and after app scans. `MainWindow.xaml` contains the named Agent next-step panel controls.
- Safety state: No cloud AI, cleanup, uninstall, migration, service/startup, scheduled-task, registry, installer, or file-move execution path was added.
- Last verification: focused Agent tests passed 2/2; `ProductExperienceTests` passed 64/64; full suite passed 122/122; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: No GUI screenshot/real-click visual pass for the Agent page in this slice.
- Next action: GUI-verify the Agent page after a real C-drive scan and app scan when approvals allow, or continue the next small Agent/App UX presenter slice without enabling new destructive actions.

## 2026-07-08 - Active Slice: Agent Safe Navigation Actions

- Objective: Let the Agent next-step panel guide a beginner to the right local page with explicit safe navigation actions, without executing cleanup, uninstall, migration, startup/service, scheduled-task, registry, installer, or file-move operations.
- Dependencies: `AgentNextStepPresenter`, `AgentNextStepViewModel`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `ProductExperienceTests`.
- Risks: Navigation labels must not imply that clicking them performs the underlying fix. The action buttons must route only to existing pages and keep execution inside the existing page-specific confirmation/safety pipeline.
- Impact scope: Agent page presentation and local view model only.
- Acceptance criteria: Agent next-step model exposes structured actions with target pages; C-drive cleanup recommendations route to the C-drive page, C-drive app concerns route to app management, and WPF binds buttons that call `ShowPage(...)` rather than any mutation handler.
- Next action: Add failing product tests for structured Agent next actions and WPF navigation hooks, then implement.

## 2026-07-08 - Active Slice Update: Agent Safe Navigation Actions

- Objective: Let the Agent next-step panel guide a beginner to the right local page with explicit safe navigation actions.
- Status: Implemented and verified at unit/build level. `AgentNextActionViewModel` now gives each action a label, tooltip description, target internal page, and `IsNavigationOnly=true`.
- UI state: `AgentNextStepActionButtonsItemsControl` renders action buttons on the Agent page. `AgentNextAction_Click` accepts only known OMNIX internal pages and calls `ShowPage(targetPage)`.
- Safety state: Buttons only navigate to existing pages. They do not execute cleanup, uninstall, migration, service/startup, scheduled-task, registry, installer, or file-move operations.
- Last verification: focused Agent tests passed 3/3; `ProductExperienceTests` passed 65/65; full suite passed 123/123; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: No GUI screenshot/click-through for Agent navigation buttons. The Agent left-card XAML still contains a duplicate old mojibake identity block below the new clean identity copy; deleting it safely is deferred to a dedicated UTF-8/XAML cleanup pass.
- Next action: GUI-verify Agent navigation buttons after real scan/app scan when approvals allow, or start a focused XAML cleanup replacing the Agent left-card region with stable XML-reference text.

## 2026-07-08 - Active Slice Update: Agent Left-Card XAML Cleanup

- Objective: Remove duplicate legacy identity copy from the Agent left card so the page presents one clear Computer Agent identity before the next-step recommendations.
- Status: Implemented and verified at unit/build level. The duplicate old identity/description `TextBlock` pair was removed from `MainWindow.xaml`.
- Safety state: XAML-only cleanup; no cloud AI, cleanup, uninstall, migration, service/startup, scheduled-task, registry, installer, or file-move behavior changed.
- Last verification: focused cleanup test passed 1/1; focused Agent tests passed 4/4; `ProductExperienceTests` passed 66/66; full suite passed 124/124; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: No GUI screenshot/click-through for the cleaned Agent card.
- Next action: GUI-verify the Agent page card and navigation buttons after real scan/app scan when approvals allow, or continue another small Agent/App UX presenter slice.

## 2026-07-08 - Active Slice Update: App Drawer Action Preview Panels

- Objective: Make app-drawer `clean cache` and `disable startup` buttons respond with beginner-readable preview panels instead of doing nothing or exposing technical details.
- Status: Implemented and verified at unit/build level. Cache cleanup and startup control now have core presenters, drawer view-model fields, collapsed WPF preview panels, and click handlers.
- Safety state: Both preview models set `CanExecuteDirectly=false`. WPF handlers only update UI state and status text. No file deletion, quarantine movement, registry edit, service/startup/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added.
- Last verification: focused cache preview tests passed 2/2; focused startup preview tests passed 2/2; `ProductExperienceTests` passed 70/70; full suite passed 128/128; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: No GUI screenshot/click-through for the new app-drawer preview panels.
- Next action: GUI-verify app drawer cache/startup preview panels after a real app scan when approvals allow, or continue moving drawer action states into small core presenters without enabling destructive behavior.

## 2026-07-08 - Active Slice Update: AppData Cache Candidates and GUI Proof

- Objective: Make app-drawer cache preview usable with real scanned software, not just synthetic profiles, while keeping cleanup preview-only.
- Status: Implemented and verified. Software inventory now infers conservative AppData data/cache/log candidates from LocalAppData, Roaming AppData, and LocalLow roots. The app-drawer GUI smoke script now proves both cache and startup preview panels can appear after a real app scan.
- Safety state: New scanner behavior is read-only and uses bounded directory-size estimation. No cleanup, delete, quarantine movement, registry edit, service/startup/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added.
- Last verification: focused cache-candidate tests passed 2/2; `SoftwareInventoryTests` passed 11/11; `ProductExperienceTests` passed 71/71; full suite passed 130/130; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-app-drawer-preview-smoke.ps1` passed with `cachePreviewVisible=True` and `startupPreviewVisible=True`.
- GUI evidence: `.omx/qa-app-drawer-action-previews.png`.
- Not verified: No real cleanup execution exists or was tested. Browser/vendor-nested cache attribution remains limited.
- Next action: Expand cache attribution for common nested app data patterns, or continue moving app-drawer action preview orchestration out of `MainWindow.xaml.cs`.

## 2026-07-08 - Active Slice: Nested Browser/Electron Cache Attribution

- Objective: Expand read-only software cache attribution from direct AppData folders to conservative nested layouts used by browsers and Electron apps.
- Dependencies: `SoftwareInventoryBuilder`, `SoftwareProfile`, `SoftwareInventoryTests`.
- Risks: Do not broadly fuzzy-match unrelated AppData folders, do not double-count cache sizes, and do not add any cleanup execution.
- Impact scope: software inventory evidence only; app drawer cache preview may become more useful because profiles have better cache paths.
- Acceptance criteria: builder recognizes vendor/app roots such as `Google\Chrome`, nested `User Data`, browser profile caches such as `Default\Cache`, and Electron `User Data\Cache`, while keeping all behavior read-only.
- Status: Implemented and verified. Nested AppData attribution now covers exact `Vendor\App` roots, `User Data`, and known browser profile folders while keeping cache evidence read-only.
- Last verification: focused nested tests passed 2/2; `SoftwareInventoryTests` passed 13/13; `ProductExperienceTests` passed 71/71; full suite passed 132/132; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-app-drawer-preview-smoke.ps1` passed with `cachePreviewVisible=True` and `startupPreviewVisible=True`.
- Safety state: no cleanup, delete, quarantine move, registry edit, service/startup/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added.
- Next action: Continue extracting app-drawer action orchestration from `MainWindow.xaml.cs`, or add another small beginner-facing drawer/Agent improvement without enabling destructive behavior.

## 2026-07-08 - Active Slice Update: App Drawer Preview State Presenter

- Objective: Move cache/startup drawer preview switching out of WPF code-behind and into a tested core presenter.
- Status: Implemented and verified. `AppDrawerActionPreviewPresenter` now returns one `AppDrawerActionPreviewState` for cache cleanup or startup-control preview clicks; WPF applies the state through `ApplyDrawerActionPreviewState`.
- Safety state: preview clicks still only show guidance. No cleanup, startup disabling, registry edit, service/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added.
- Last verification: focused presenter test passed 1/1; `ProductExperienceTests` passed 72/72; full suite passed 133/133; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-app-drawer-preview-smoke.ps1` passed with `cachePreviewVisible=True` and `startupPreviewVisible=True`.
- Next action: Add a no-selection drawer action presenter state, or continue extracting technical-detail/uninstall/migration drawer state from `MainWindow.xaml.cs` without enabling destructive behavior.

## 2026-07-08 - Active Slice Update: App Drawer No-Selection States

- Objective: Move cache/startup "choose an app first" branches into the same tested presenter used for selected-app preview clicks.
- Status: Implemented and verified. `AppDrawerActionPreviewPresenter.NoSelectionForCacheCleanup()` and `.NoSelectionForStartupControl()` now hide preview panels, keep execution disabled, and provide the correct status text.
- Safety state: no cleanup, startup disabling, registry edit, service/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added.
- Last verification: focused drawer preview presenter tests passed 2/2; `ProductExperienceTests` passed 73/73; full suite passed 134/134; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: no separate GUI smoke for no-selection click branches.
- Next action: Continue extracting technical-detail, uninstall, or migration drawer state from `MainWindow.xaml.cs`, or move on to another beginner-facing Agent/app-drawer improvement.

## 2026-07-08 - Active Slice Update: App Drawer Technical Details Toggle

- Objective: Make the app drawer technical-details toggle a tested presentation state and update the button text after opening/closing.
- Status: Implemented and verified. `AppDrawerTechnicalDetailsPresenter` now models show/hide state, button text, and status text; WPF applies it through `ApplyDrawerTechnicalDetailsState`.
- Safety state: technical details remain hidden by default. No cleanup, startup disabling, registry edit, service/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added.
- Last verification: focused technical-details toggle test passed 1/1; `ProductExperienceTests` passed 74/74; full suite passed 135/135; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: no GUI smoke for clicking technical details.
- Next action: Continue extracting uninstall or migration drawer state from `MainWindow.xaml.cs`, or start a broader clean app-drawer action host model.

## 2026-07-08 - Active Slice: Shared App Drawer Action Preview Host

- Objective: Reduce app-drawer text clutter by moving uninstall/cache/startup/migration previews into one shared Agent action preview host that updates after the user clicks an action.
- Dependencies: `AppDrawerViewModel`, uninstall workflow guide, migration preview lines, cache/startup preview presenters, `MainWindow.xaml`, `MainWindow.xaml.cs`, `ProductExperienceTests`.
- Risks: Do not remove safety language, do not imply actions execute from the drawer, and do not enable cleanup, uninstall, migration, service/startup, scheduled-task, registry, installer, or cloud AI execution.
- Impact scope: app drawer presentation and WPF wiring only.
- Acceptance criteria: the drawer has one named action preview host; default drawer selection hides it; clicking uninstall/cache/startup/migration updates that one host with title, summary, lines, non-executable state, and status text.
- Status: Implemented and verified at unit/build level. `AppDrawerActionHostPresenter` now drives one `DrawerActionPreviewPanel` for uninstall, migration, cache, startup, and residue-review outputs.
- Safety state: no cleanup, startup disabling, official uninstaller execution, migration execution, registry edit, service/scheduled-task mutation, installer, or cloud AI path was added.
- Last verification: focused shared-host test passed 1/1; `ProductExperienceTests` passed 75/75; full suite passed 136/136; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: `.omx/gui-app-drawer-preview-smoke.ps1` was attempted but rejected by the usage-limit approval system, so no GUI screenshot for this slice.
- Next action: Run GUI smoke when approvals/usage allow, then remove old collapsed app-drawer preview compatibility controls in a focused XAML cleanup pass.

## 2026-07-08 - Active Slice Update: Uninstall/Migration No-Selection Host States

- Objective: Route uninstall and migration no-selection branches through the same shared drawer action host model.
- Status: Implemented and verified. `AppDrawerActionHostPresenter.NoSelectionForUninstall()` and `.NoSelectionForMigration()` now provide collapsed, non-executable host states with action-specific guidance. A handler-specific wiring regression test protects uninstall/cache/startup/migration no-selection branches from being crossed.
- Safety state: no cleanup, startup disabling, official uninstaller execution, migration execution, registry edit, service/scheduled-task mutation, installer, or cloud AI path was added.
- Last verification: focused no-selection host/wiring tests passed 2/2; `ProductExperienceTests` passed 77/77; full suite passed 138/138; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: no GUI smoke for no-selection branches.
- Next action: Run app-drawer GUI smoke when approvals/usage allow; then remove old collapsed preview compatibility controls and the overwritten uninstall no-selection status line in a safe cleanup pass.

## 2026-07-08 - Active Slice: App Drawer Legacy Preview Cleanup

- Objective: Remove old collapsed app-drawer preview controls and leftover code writes now that uninstall, migration, cache, startup, and residue review all use the shared action host.
- Dependencies: `MainWindow.xaml`, `MainWindow.xaml.cs`, `ProductExperienceTests`.
- Risks: Do not remove the residue-review action, technical-details toggle, shared host, or any safety copy. Do not add cleanup, startup disabling, official uninstall, migration, registry, service/scheduled-task, installer, file move, or cloud AI execution.
- Impact scope: WPF app-drawer presentation cleanup only.
- Acceptance criteria: XAML contains one shared action preview host and no legacy drawer preview panels/list boxes; code no longer references legacy preview controls; no-selection drawer action branches get status text from `AppDrawerActionHostPresenter`; app drawer action behavior remains preview-only through `AppDrawerActionHostPresenter`.
- Next action: Add a failing product test that asserts the uninstall no-selection branch no longer writes status directly, then remove the leftover assignment.

## 2026-07-08 - Active Slice: C-Drive Legacy Selection Handler Cleanup

- Objective: Remove the unused `RecommendationsListBox_SelectionChangedLegacy` handler after the C-drive recommendation selection path moved to `RecommendationSelectionPresenter`.
- Dependencies: `MainWindow.xaml`, `MainWindow.xaml.cs`, `ProductExperienceTests`.
- Risks: Do not change actionable/non-actionable recommendation semantics, quarantine requirements, or the safety pipeline.
- Impact scope: WPF code-behind cleanup only.
- Acceptance criteria: XAML binds only `RecommendationsListBox_SelectionChanged`; code contains no legacy selection handler; current handler still uses `RecommendationSelectionPresenter.Create(...)`.
- Next action: Add a failing product test for absence of the legacy handler, then remove it.

## 2026-07-08 - Active Slice: Agent Skill Capability Cards

- Objective: Turn the Marvis-inspired Agent skill catalog into clearer capability cards with user-facing next steps and safety modes.
- Dependencies: `AgentSkillCatalog`, `AgentSkillView`, `MainWindow.xaml`, `ProductExperienceTests`.
- Risks: Do not imply Agent can directly change system settings, terminate processes, disable services, lock/restart/shutdown, or run tools without confirmation.
- Impact scope: Agent page skill presentation only.
- Acceptance criteria: each skill exposes a short user action label, safety mode label, risk label, and next-step hint; process/service and input/session skills remain high-risk plan-only; system tools are labeled as open-only.
- Next action: Add a failing product test for capability-card fields, then implement the view model and XAML bindings.

## 2026-07-08 - Active Slice Updates: Cleanup and Agent Skill Cards

- App drawer legacy preview cleanup: implemented and verified. The drawer now has only one shared action preview host in XAML; old cache/startup/uninstall/migration preview controls and code references were removed. Uninstall no-selection status now comes from `AppDrawerActionHostPresenter`.
- C-drive legacy selection cleanup: implemented and verified. `RecommendationsListBox_SelectionChangedLegacy` was removed; the active handler continues to use `RecommendationSelectionPresenter`.
- Agent skill capability cards: implemented and verified. Added `AgentSkillCardPresenter` / `AgentSkillCardViewModel`; Agent skill UI now shows next-step labels and safety hints while keeping high-risk skills plan-only and system tools open-only.
- Last verification: focused drawer cleanup tests passed 5/5; focused C-drive selection tests passed 3/3; focused Agent skill-card test passed 1/1; focused Agent tests passed 4/4; `ProductExperienceTests` passed 79/79; full suite passed 140/140; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Not verified: no real GUI screenshot/click-through for the shared drawer host or Agent skill card list in this slice because prior WPF GUI smoke was blocked by the approval/usage-limit system.
- Next action: Continue with a small visible UX slice, preferably GUI-verifiable when approvals allow: Agent skill-card visual smoke, shared drawer host smoke, or next-step actions that remain navigation-only/plan-only.

## 2026-07-08 - Active Slice: System Tool Shortcuts

- Objective: Add a Marvis-inspired "system tools direct" section to the Agent page for common Windows tools such as Task Manager, Device Manager, Disk Management, Event Viewer, Windows Security, and Registry Editor.
- Dependencies: `Css.Core.Agent`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `ProductExperienceTests`.
- Risks: Tool shortcuts must be allowlisted and must not run shell wrappers or arbitrary commands. High-risk tools such as Registry Editor must show confirmation/safety text before launch.
- Impact scope: Agent page presentation and explicit tool-launch helper only.
- Acceptance criteria: core catalog exposes open-only shortcut cards with id/name/description/command/risk/confirmation; WPF displays the shortcuts; click handler only opens allowlisted commands after required confirmation and does not perform system modifications itself.
- Next action: Add failing product tests for the shortcut catalog and WPF binding, then implement.

## 2026-07-08 - Active Slice Update: System Tool Shortcuts

- Status: Implemented and verified. Added `SystemToolShortcutCatalog` with Task Manager, Device Manager, Disk Management, Event Viewer, Windows Security, and Registry Editor; Agent page shows a system-tool direct list with explicit open buttons.
- Safety state: Shortcuts are allowlisted; unknown ids are blocked; medium/high-risk tools require confirmation; the app only opens Windows tools and does not click inside them or modify settings.
- Last verification: focused shortcut tests passed 2/2; focused Agent tests passed 5/5; `ProductExperienceTests` passed 81/81; full suite passed 142/142; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentSystemToolListFound=true`, `visibleOpenButtonCount=4`, screenshot `.omx/qa-agent-system-tools.png`.
- Next action: Continue broadening Agent-led workflows while preserving safety boundaries. Good next slices: system settings deep-link suggestions, startup/service plan previews, or real GUI smoke for app drawer shared host.

## 2026-07-08 - Active Slice: Windows Settings Shortcuts

- Objective: Add Agent-led Windows Settings direct links for beginner-safe entry points such as Network/Wi-Fi, Bluetooth/devices, Sound, Display, Power, Storage, and Installed Apps.
- Dependencies: `Css.Core.Agent`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `ProductExperienceTests`, optional GUI smoke script.
- Risks: Deep links must be fixed `ms-settings:` URIs, not arbitrary commands. Opening a settings page must not imply that OMNIX-Entropy changes settings, uninstalls apps, or toggles system options.
- Impact scope: Agent page presentation and explicit open-only settings helper.
- Acceptance criteria: core catalog exposes allowlisted settings cards with id/title/description/URI/risk/safety hint; WPF displays them under the Agent page; click handler only opens known `ms-settings:` URIs via `UseShellExecute=true` and blocks unknown ids.
- Status: Implemented and verified. `WindowsSettingsShortcutCatalog` now exposes allowlisted `ms-settings:` links for Network/Wi-Fi, Bluetooth/devices, Sound, Display, Power/Sleep, Storage, and Installed Apps. The Agent page renders `AgentWindowsSettingsListBox`; `OpenWindowsSettings_Click` blocks unknown ids and non-`ms-settings:` links.
- Safety state: open-only and allowlisted. No settings button was clicked during verification, and no settings, files, uninstallers, registry keys, services, startup entries, scheduled tasks, installers, or cloud AI paths were modified.
- Last verification: focused settings tests passed 2/2; focused Agent/system/settings tests passed 5/5; `ProductExperienceTests` passed 83/83; full suite passed 144/144; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentSystemToolListFound=true`, `visibleOpenButtonCount=3`, `agentWindowsSettingsListFound=true`, `visibleSettingsOpenButtonCount=3`, screenshot `.omx/qa-agent-system-and-settings.png`.
- Next action: Add a confirmation gate for medium-risk Windows Settings pages such as Storage, Power/Sleep, and Installed Apps before opening them.

## 2026-07-08 - Active Slice: Windows Settings Confirmation Gate

- Objective: Require explicit confirmation before Agent shortcuts open medium-risk Windows Settings pages that can lead users toward uninstall, storage cleanup, or power behavior changes.
- Dependencies: `WindowsSettingsShortcutCatalog`, `MainWindow.xaml.cs`, `ProductExperienceTests`.
- Risks: Keep low-risk settings convenient while avoiding surprise navigation into pages where a beginner might accidentally change system behavior. Do not add any automation inside Windows Settings.
- Impact scope: Agent page settings-shortcut metadata and click handler only.
- Acceptance criteria: medium-risk settings expose `RequiresConfirmation=true`; low-risk settings remain no-confirmation open-only links; the WPF handler checks `shortcut.RequiresConfirmation` before launch and cancellation only updates status text.
- Status: Implemented and verified. Medium-risk Windows Settings entries (`power`, `storage`, `installed-apps`) now require confirmation; low-risk entries remain direct open-only links. `OpenWindowsSettings_Click` shows a confirmation dialog before opening medium-risk setting pages and cancels without launching when the user declines.
- Safety state: still open-only. No setting page was clicked during verification, and OMNIX-Entropy still does not toggle settings, uninstall apps, delete files, edit registry, mutate services/startup/scheduled tasks, run installers, or call cloud AI.
- Last verification: TDD red observed because `WindowsSettingsShortcut.RequiresConfirmation` did not exist; focused settings tests passed 2/2; `ProductExperienceTests` passed 83/83; full suite passed 144/144; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentSystemToolListFound=true`, `visibleOpenButtonCount=3`, `agentWindowsSettingsListFound=true`, `visibleSettingsOpenButtonCount=3`, screenshot `.omx/qa-agent-system-and-settings.png`.
- Next action: Continue with another small Agent-led, safety-preserving slice, such as richer startup/service plan previews or a GUI smoke for the app-drawer shared action host.

## 2026-07-08 - Active Slice: Agent Background Priority

- Objective: Make the Agent next-step panel prioritize "background/resident apps" when a scan finds several apps with startup entries, services, scheduled tasks, or running processes and no low-risk C-drive cleanup item is waiting.
- Dependencies: `AgentNextStepPresenter`, `SoftwareProfile`, `ProductExperienceTests`.
- Risks: Do not add process termination, service disable, startup mutation, or task mutation. This must stay navigation-only and plan/explanation-only.
- Impact scope: Agent next-step prioritization copy and navigation-action ordering only.
- Acceptance criteria: when resident apps reach a clear threshold, Agent title/reasons/actions emphasize checking background apps first; C-drive app advice still appears as a secondary safe action when relevant; all Agent actions remain `IsNavigationOnly=true` and `CanExecuteDirectly=false`.
- Status: Implemented and verified. `AgentNextStepPresenter` now prioritizes background/resident app review when at least three resident apps are found and no low-risk C-drive cleanup item is waiting. C-drive advice still appears as a secondary safe action when relevant.
- Safety state: navigation-only and non-executable. No process termination, service disable, startup/scheduled-task mutation, cleanup, uninstall, migration, registry edit, installer, file move, or cloud AI path was added.
- Last verification: TDD red observed because the Agent title still prioritized C-drive apps; focused Agent priority test passed 1/1; focused Agent next-step tests passed 4/4; `ProductExperienceTests` passed 84/84; full suite passed 145/145; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Next action: Continue with another small Agent-led, safety-preserving slice: GUI proof for the app-drawer shared action host, confirmation-dialog smoke for medium-risk settings, or a plan-only startup/service review model.

## 2026-07-08 - Active Slice: Agent Background Review Panel

- Objective: Add a compact Agent-page background/resident app summary that tells beginner users which apps deserve review without dumping service names, scheduled-task paths, or registry/source details.
- Dependencies: `SoftwareProfile`, `AgentNextStepPresenter`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `ProductExperienceTests`.
- Risks: Do not add process termination, service disable, startup mutation, scheduled-task mutation, or any execution handler. Keep the panel explanatory and navigation/plan-only.
- Impact scope: Agent page presentation and core presenter only.
- Acceptance criteria: core presenter summarizes resident apps into user-facing items with reason, risk, and recommended next step; technical identifiers stay hidden by default; WPF displays the summary after app scans; all items expose `CanExecuteDirectly=false`.
- Status: Implemented and verified. Added `AgentBackgroundReviewPresenter` and an Agent-page `AgentBackgroundReviewPanel`. After a real app scan, the Agent page now shows a first-screen background/resident app summary with friendly evidence, risk label, and recommended next step.
- Safety state: explanation-only and plan-only. No process termination, service disable, startup/scheduled-task mutation, cleanup, uninstall, migration, registry edit, installer, file move, session control, or cloud AI path was added.
- Last verification: TDD red observed because `AgentBackgroundReviewPresenter` did not exist; focused background review tests passed 2/2; `ProductExperienceTests` passed 86/86; full suite passed 147/147; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-agent-background-review-smoke.ps1` passed after a real app scan with `appTileCount=120`, `backgroundSummaryFound=true`, `backgroundReviewItemCount=3`, screenshot `.omx/qa-agent-background-review.png`.
- Issues fixed during slice: first GUI smoke failed because `Wait-Until` was defined after use; a second smoke proved the panel was hidden too low in the left card, so the panel was moved above the reasons list and explicit AutomationIds were added.
- Next action: Continue with a plan-only startup/service review model that can turn these summary items into auditable proposed actions, or add confirmation-dialog GUI smoke for medium-risk settings.

## 2026-07-08 - Active Slice: Agent Startup/Service Plan Preview

- Objective: Let the Agent turn background/resident app evidence into a plan-only review proposal: what evidence it has, what the safe steps are, what must be confirmed before any disable action, and what remains blocked.
- Dependencies: `SoftwareProfile`, `AgentBackgroundReviewPresenter`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `ProductExperienceTests`.
- Risks: Do not create an execution handler for disabling startup entries, services, scheduled tasks, or killing processes. Do not imply that clicking Agent advice changes system state.
- Impact scope: Agent page presentation and core plan presenter only.
- Acceptance criteria: generated plan includes title, evidence summary, planned review steps, required confirmations/evidence, blocked actions, rollback/snapshot requirement, and `CanExecuteDirectly=false`; WPF displays the plan in the Agent background section after app scans.
- Status: Implemented and verified. `AgentStartupServicePlanPresenter` generates a plan-only, non-executable startup/service review proposal from resident app evidence. The Agent page displays it immediately after the background summary and before the detailed resident-app list so users see the plan before app-level detail.
- Safety state: plan-only and non-executable. No startup disabling, service/scheduled-task mutation, process termination, cleanup, uninstall, migration, registry edit, installer, file move, session control, or cloud AI path is allowed by this slice.
- Last verification: TDD red observed for missing automation ids and then for plan preview being placed below the detailed background list; focused Agent plan/binding tests passed 3/3; full suite passed 148/148; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-agent-background-review-smoke.ps1` passed after a real app scan with `appTileCount=120`, `backgroundSummaryFound=true`, `backgroundReviewItemCount=3`, `startupServicePlanFound=true`, `startupServicePlanStepCount=3`, screenshot `.omx/qa-agent-startup-service-plan.png`.
- Issues fixed during slice: GUI smoke initially failed because a raw Chinese PowerShell string did not match under Windows PowerShell script encoding; it now constructs the expected phrase from Unicode code points.
- Next action: Continue with the next small visible Agent-led slice, preferably a confirmation-dialog GUI smoke for medium-risk Windows Settings or a compact action-plan host for selected resident apps, while keeping real startup/service changes disabled.

## 2026-07-08 - Active Slice: Windows Settings Confirmation Cancel Smoke

- Objective: Add real GUI proof that a medium-risk Windows Settings shortcut shows a confirmation dialog and that canceling it does not open Windows Settings.
- Dependencies: `WindowsSettingsShortcutCatalog`, `OpenWindowsSettings_Click`, `MainWindow.xaml`, existing Agent system/settings GUI smoke style.
- Risks: Do not accept the confirmation, do not open Windows Settings, and do not modify system settings.
- Impact scope: `.omx` GUI smoke script only unless verification exposes a product bug.
- Acceptance criteria: smoke launches `Css.App.exe`, opens AI Agent, invokes a medium-risk settings shortcut such as Storage, finds the confirmation dialog, cancels it, verifies the OMNIX window remains and no Settings process/window was launched by the test.
- Safety state: test-only; no settings toggles, uninstall, cleanup, registry edit, service/startup/scheduled-task mutation, installer, file move, or cloud AI path should be added.
- Status: Implemented and verified. Added a real GUI smoke for the medium-risk Storage settings shortcut. Reordered Windows Settings shortcuts so Storage/Installed Apps/Power appear first, added stable setting-button AutomationIds, and made the Agent capability column scrollable. The smoke proves the Storage confirmation dialog appears and canceling it does not launch `SystemSettings`.
- Last verification: TDD red observed for missing dynamic setting-button AutomationIds, non-scrollable Agent capability column, old Windows Settings order, and settings section appearing below system tools. Focused settings tests passed 2/2; full suite passed 148/148; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` passed with `confirmationDialogFound=true`, `cancelClicked=true`, `newSettingsProcessCount=0`, screenshot `.omx/qa-agent-settings-confirm-cancel.png`; `.omx/gui-agent-system-tools-smoke.ps1` passed with both Agent system tools and settings lists found.
- Issues fixed during slice: script initially looked only at top-level windows and missed the confirmation dialog; settings buttons were too low until Windows Settings moved above system tools; dialog cancel lookup needed a rightmost-button fallback.
- Next action: Continue with another safety-preserving Agent slice: selected resident-app plan details, app-drawer shared host GUI proof, or medium-risk settings confirmation copy cleanup.

## 2026-07-08 - Active Slice: App Drawer Shared Action Host GUI Proof

- Objective: Prove with a real WPF GUI smoke that app-drawer action buttons are visible, clickable, and update the single shared action preview host instead of doing nothing or piling multiple text-heavy sections.
- Dependencies: `.omx/gui-app-drawer-preview-smoke.ps1`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `AppDrawerActionHostPresenter`, `ProductExperienceTests`.
- Risks: The smoke must not execute cleanup, disable startup entries, run official uninstallers, move app files, edit registry, mutate services/tasks, or call cloud AI.
- Impact scope: Prefer test/smoke-script fixes only; product changes only if the real GUI reveals broken wiring or missing automation/accessibility hooks.
- Acceptance criteria: launch the app, open app management, run read-only app scan, select a real app, invoke visible app-drawer action buttons, verify `DrawerActionPreviewPanel` becomes visible with cache/startup preview text, capture a screenshot, and close only the launched app process.
- Status: Implemented and verified. Drawer action buttons now have stable AutomationIds, and `.omx/gui-app-drawer-preview-smoke.ps1` verifies all four main drawer actions: uninstall plan, migration plan, cache cleanup preview, and startup-control preview.
- Safety state: GUI smoke clicked preview buttons only. Uninstall and migration opened plan windows that were closed by the script; no uninstaller was run, no rollback manifest was created, no cleanup/startup/migration action executed, and no file/registry/service/task mutation was added.
- Last verification: TDD red observed for missing drawer button/action preview AutomationIds; focused AutomationId test passed 1/1; `ProductExperienceTests` passed 88/88; full suite passed 149/149; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-app-drawer-preview-smoke.ps1` passed with `verifiedActionButtons=4`, `closedDialogCount=2`, screenshot `.omx/qa-app-drawer-action-previews.png`.
- Issues fixed during slice: the first smoke only covered cache/startup; `Border` containers did not appear reliably in UIAutomation; the script initially assumed one selected app enabled every action, but migration is correctly disabled for already-reasonable D-drive installs.
- Next action: Continue the beginner-friendly app workflow with either selected-app resident/background plan details, cleaner app drawer layout, or undo-center visual proof while keeping real risky actions gated.

## 2026-07-08 - Active Slice: App Drawer Agent Action Cards

- Objective: Make app-drawer action previews read more like a concise Agent action card: what Agent thinks, what would happen next, what will not happen, and whether the action is currently executable.
- Dependencies: `AppDrawerActionHostPresenter`, `AppDrawerActionPreviewPresenter`, `AppPresentationBuilder`, `MainWindow.xaml`, `ProductExperienceTests`.
- Risks: Do not enable cleanup, startup disabling, official uninstaller execution, migration execution, rollback manifest creation, service/task/registry edits, installer execution, file moves, settings changes, session control, or cloud AI.
- Impact scope: Core presentation model and WPF binding only; real operations remain unchanged.
- Acceptance criteria: action host view model exposes plain `AgentTakeaway`, `NextStepText`, and `SafetyText`; cache/startup/uninstall/migration previews populate them; WPF shows these concise fields above the detail list; all states remain `CanExecuteDirectly=false` unless an existing safe gated operation explicitly says otherwise.
- Status: Implemented and verified. `AppDrawerActionHostViewModel` now exposes `AgentTakeaway`, `NextStepText`, and `SafetyText`; WPF binds those fields above the details list; the app drawer has a scroll viewer and calls `DrawerActionPreviewPanel.BringIntoView()` after an action click.
- Safety state: presentation-only. All existing drawer action states remain preview/plan-only; no cleanup, startup disabling, official uninstaller execution, migration execution, rollback manifest creation, registry edit, service/scheduled-task mutation, installer, file move, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing action-card fields and missing app-drawer scroll/bring-into-view behavior; focused action-card tests passed 3/3; enhanced `.omx/gui-app-drawer-preview-smoke.ps1` passed with `verifiedActionButtons=4`, `closedDialogCount=2` and verified Agent/next-step/safety fields; `ProductExperienceTests` passed 91/91; full suite passed 152/152; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; screenshot `.omx/qa-app-drawer-action-previews.png`.
- Issues fixed during slice: first screenshot showed the new action-card content was too low in the drawer; the drawer now scrolls and brings the preview card into view. A direct residue-review `AppDrawerActionHostViewModel` initializer also needed the new required fields.
- Next action: Continue with selected-app resident/background plan details or undo-center visual proof; consider extracting shared WPF GUI smoke helpers now that app-drawer and Agent smokes have repeated launch/scan/click/screenshot patterns.

## 2026-07-08 - Active Slice: Selected Resident App Plan Details

- Objective: When a user selects a resident/background app and clicks the startup action, show a concise Agent plan that classifies the app as keep, observe, or candidate-for-future-disable without exposing raw service, scheduled-task, or startup identifiers in the first-level UI.
- Dependencies: `AppStartupControlPreviewPresenter`, `AppDrawerActionHostPresenter`, `SoftwareProfile`, `ProductExperienceTests`, and optional app-drawer GUI smoke.
- Risks: This must remain plan-only. Do not disable startup entries, services, scheduled tasks, or running processes, and do not create an execution handler.
- Impact scope: core presentation model and drawer action-card copy; WPF binding should remain stable unless the model needs new visible fields.
- Acceptance criteria: focused tests prove keep/observe/future-disable categories, raw identifiers hidden by default, snapshot/rollback/user-confirmation requirements present, and `CanExecuteDirectly=false`.
- Status: Implemented and verified. Selected-app startup/background previews now classify resident apps as `建议保留`, `先观察`, or `未来可禁用候选`; the app drawer action card uses those conclusions in its Agent takeaway, next step, and safety boundary.
- Safety state: no real cleanup, startup disabling, service/scheduled-task mutation, process termination, registry edit, migration, installer execution, settings change, session control, or cloud AI path will be added in this slice.
- Last verification: TDD red observed for all three new selected-app plan tests; focused new tests passed 3/3; surrounding app-drawer/startup tests passed 4/4; `ProductExperienceTests` passed 94/94; full suite passed 155/155; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; `.omx/gui-app-drawer-preview-smoke.ps1` passed with `verifiedActionButtons=4`, `closedDialogCount=2`, screenshot `.omx/qa-app-drawer-action-previews.png`.
- Next action: start undo-center visual proof for quarantine/timeline display and restore affordance before broadening cleanup execution.

## 2026-07-08 - Active Slice: Undo Center Visual Proof

- Objective: Make the undo center visibly prove quarantine/timeline display and restore affordance, so future cleanup/residue actions have a beginner-readable "can I regret this?" place.
- Dependencies: existing undo/timeline/quarantine UI, `MainWindow.xaml`, `MainWindow.xaml.cs`, `ProductExperienceTests`, and optional GUI smoke script.
- Risks: Do not perform destructive cleanup. Any test data must be local, controlled, and restore-safe; prefer static UI/AutomationId proof first.
- Impact scope: undo-center UI proof hooks and smoke coverage, with product changes only if controls are not discoverable or too text-heavy.
- Acceptance criteria: tests or GUI smoke can find undo-center timeline, quarantine list/empty state, restore affordance, and safety copy with stable AutomationIds; no real cleanup, deletion, or overwrite happens.
- Status: Implemented with static/product verification. `TimelinePage` now has stable UIAutomation hooks for title, load button, description, quarantine policy, timeline list, restore line, and restore button; the historical malformed/mojibake XAML block was rewritten with XML character references.
- Safety state: verification-only; no permanent deletion, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path.
- Last verification: TDD red observed for missing undo-center AutomationIds; focused undo hook test passed 1/1; `ProductExperienceTests` passed 95/95; full suite passed 156/156; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. GUI smoke `.omx/gui-undo-center-smoke.ps1` now passed with `timelineTitleFound=true`, `quarantinePolicyFound=true`, `timelineListFound=true`, `restoreButtonFound=true`, `restoreButtonEnabled=false`; screenshot `.omx/qa-undo-center.png`.
- Next action: continue with isolated undo-center GUI data so future smokes can show a real restorable row without touching the user's actual timeline.

## 2026-07-09 - Active Slice: Isolated App Storage Roots for GUI Smokes

- Objective: Let WPF GUI smokes point OMNIX-Entropy local data and quarantine storage to isolated temporary roots, so tests can verify undo/quarantine behavior without touching the user's real timeline or quarantine area.
- Dependencies: `AppIdentity`, `MainWindow.xaml.cs`, `.omx/gui-undo-center-smoke.ps1`, `AppIdentityTests`, `ProductExperienceTests`.
- Risks: Defaults must not change for normal app runs. Environment overrides must be opt-in and easy to clean up. GUI smoke must not leave test data behind.
- Impact scope: storage path resolution and the undo-center smoke script only.
- Acceptance criteria: resolver supports `OMNIX_ENTROPY_DATA_ROOT` and `OMNIX_ENTROPY_QUARANTINE_ROOT`; defaults remain unchanged; WPF app uses the resolver; undo-center GUI smoke sets isolated roots and removes them in `finally`; tests and build pass.
- Status: Implemented and verified. Added `AppStoragePathResolver` and `AppStoragePaths`; `MainWindow` now uses the resolver for database, migration rollback, and quarantine roots; undo-center smoke now sets temporary isolated roots under `.omx` and cleans them after launch.
- Safety state: path-selection only. No cleanup, restore, quarantine move, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing `AppStoragePathResolver`; path resolver tests passed 2/2. TDD red observed for undo smoke lacking isolated env vars; script isolation test passed. `ProductExperienceTests` passed 96/96; full suite passed 159/159; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; isolated `.omx/gui-undo-center-smoke.ps1` passed and both `.omx/qa-undo-center-data` and `.omx/qa-undo-center-quarantine` were absent after cleanup.
- Next action: extend the isolated undo-center GUI smoke to seed one restorable timeline row in the isolated roots, then verify the restore button is enabled without clicking it.

## 2026-07-09 - Active Slice: Seeded Undo-Center Restorable GUI Proof

- Objective: Extend the isolated undo-center GUI smoke so it seeds one restorable quarantine/timeline record under temporary `.omx` roots and proves the restore affordance is enabled without clicking it.
- Dependencies: `.omx/gui-undo-center-smoke.ps1`, `FileQuarantineService`, `ActionTimelineStore`, `MainWindow.xaml.cs`, `ProductExperienceTests`.
- Risks: The smoke must not touch the user's real timeline/quarantine data, must not click the restore button, and must clean every seeded test path.
- Impact scope: test/smoke tooling first; production behavior should stay unchanged unless a real UI discoverability issue appears.
- Acceptance criteria: a failing test requires explicit seeding and no restore invocation; the smoke creates isolated data, launches the app with env overrides, verifies `TimelineRestoreButton` is enabled for the seeded row, captures a screenshot, and removes isolated data afterward.
- Status: Implemented and verified. Added `Css.SmokeTools seed-undo-center` and extended `.omx/gui-undo-center-smoke.ps1` so the WPF smoke seeds one restorable record under isolated roots, verifies `TimelineRestoreButton` is enabled, screenshots the page, and cleans the temporary roots. The timeline presenter now summarizes affected paths as `影响范围：N 个位置` instead of exposing long local paths in the first-level row.
- Safety state: verification-only. The smoke creates and quarantines a temporary file only inside `.omx` isolated roots, does not click restore, restores env vars in `finally`, and removes the isolated data/quarantine roots. No cleanup, real restore, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing seeded smoke behavior; focused undo smoke static tests passed 3/3. TDD red observed for timeline detail exposing a full path; focused timeline presentation tests passed 2/2. `ProductExperienceTests` passed 97/97; full suite passed 161/161; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. `.omx/gui-undo-center-smoke.ps1` passed with `restoreButtonEnabled=true`; cleanup checks for `.omx/qa-undo-center-data` and `.omx/qa-undo-center-quarantine` returned `False`; screenshot `.omx/qa-undo-center.png`.
- Issue fixed during slice: initial `dotnet restore ComputerSecuritySoftware.slnx` failed under sandbox network restrictions, then passed with approved escalation. The final build and tests used restored assets.
- Next action: extract shared `.omx` WPF smoke helpers and document the storage override env vars as development/test-only before packaging work.

## 2026-07-09 - Active Slice: Shared WPF Smoke Helper Foundation

- Objective: Start extracting repeated WPF GUI smoke functions into a shared `.omx` helper so future app-drawer, Agent, settings, and undo smokes do not duplicate launch/search/screenshot glue.
- Dependencies: `.omx/gui-undo-center-smoke.ps1`, future `.omx` GUI smoke scripts, `ProductExperienceTests`.
- Risks: Keep this refactor tooling-only; do not weaken the seeded undo smoke or accidentally remove its cleanup and no-restore-click guarantees.
- Impact scope: `.omx` smoke scripts and static product tests only.
- Acceptance criteria: a failing test requires the undo smoke to dot-source the shared helper; helper owns UIAutomation assembly initialization, `Find-ByAutomationId`, `Wait-Until`, `Invoke-Element`, and `Save-WindowScreenshot`; the seeded undo GUI smoke still passes.
- Status: Implemented and verified for the first consumer. Added `.omx/wpf-smoke-helpers.ps1` with shared UIAutomation initialization, `Find-ByAutomationId`, `Wait-Until`, `Invoke-Element`, and `Save-WindowScreenshot`; `.omx/gui-undo-center-smoke.ps1` now dot-sources the helper.
- Safety state: tooling refactor only. Seeded undo smoke still uses isolated roots, does not click restore, and cleans temporary roots. No product behavior or system-changing action was added.
- Last verification: TDD red observed because `.omx/wpf-smoke-helpers.ps1` did not exist; focused shared-helper test passed 1/1; focused undo smoke tests passed 4/4; full suite passed 162/162; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors; seeded undo GUI smoke passed with `restoreButtonEnabled=true`; cleanup checks returned `False` for both temp roots.
- Next action: migrate other GUI smokes (`gui-app-drawer-preview`, Agent, settings) onto `.omx/wpf-smoke-helpers.ps1` and document storage override env vars as development/test-only.

## 2026-07-09 - Active Slice: App Drawer Smoke Helper Migration

- Objective: Migrate `.omx/gui-app-drawer-preview-smoke.ps1` onto the shared WPF helper while preserving its four-action, no-execution GUI proof.
- Dependencies: `.omx/wpf-smoke-helpers.ps1`, `.omx/gui-app-drawer-preview-smoke.ps1`, `ProductExperienceTests`.
- Risks: Do not weaken the smoke's safety checks: it must still only click preview buttons, close plan windows, and never execute cleanup, uninstall, migration, startup/service/task mutation, registry edits, settings, installer, or AI actions.
- Impact scope: `.omx` smoke tooling and static product tests only.
- Acceptance criteria: failing test requires the app-drawer smoke to dot-source the helper and use shared helper functions; real app-drawer GUI smoke still verifies four action buttons and closes preview dialogs; full tests/build remain green.
- Status: Implemented and verified. `.omx/gui-app-drawer-preview-smoke.ps1` now dot-sources `.omx/wpf-smoke-helpers.ps1`, uses shared UIAutomation initialization, shared `Find-ByAutomationId`, `Invoke-Element`, and `Save-DesktopScreenshot`, while keeping app-drawer-specific selection/dialog logic local.
- Safety state: tooling refactor only; no product behavior is added.
- Last verification: TDD red observed for `App_drawer_gui_smoke_uses_shared_wpf_smoke_helpers`; focused app-drawer helper/action-host tests passed 4/4; real `.omx/gui-app-drawer-preview-smoke.ps1` passed with `verifiedActionButtons=4` and `closedDialogCount=2`; `ProductExperienceTests` passed 100/100; full suite passed 164/164; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Next action: migrate Agent/settings/system-tool GUI smokes onto the shared helper one by one.

## 2026-07-09 - Active Slice: GUI Smoke Development Documentation

- Objective: Document GUI smoke storage overrides and seed tooling as development/test-only so they are not mistaken for packaged user features.
- Dependencies: `docs/development/gui-smokes.md`, `ProductExperienceTests`, `AppStoragePathResolver`, `.omx` smoke scripts.
- Risks: Documentation must preserve the safety boundary: process-scoped env vars only, restore prior values, remove temporary roots, do not expose as normal user settings.
- Impact scope: development documentation and static tests only.
- Acceptance criteria: failing test requires docs to mention `OMNIX_ENTROPY_DATA_ROOT`, `OMNIX_ENTROPY_QUARANTINE_ROOT`, "development and GUI smoke tests only", previous env restoration, and `Css.SmokeTools seed-undo-center`.
- Status: Implemented and verified. Added `docs/development/gui-smokes.md`.
- Safety state: documentation only; no product behavior or system-changing action was added.
- Last verification: TDD red observed because `docs/development/gui-smokes.md` was missing; focused docs test passed 1/1; `ProductExperienceTests` passed 100/100; full suite passed 164/164; solution build passed with 0 warnings and 0 errors.
- Next action: continue migrating remaining GUI smokes and then consider a second-level technical-detail affordance for undo/timeline paths.

## 2026-07-09 - Active Slice: Agent System Tools Smoke Helper Migration

- Objective: Migrate `.omx/gui-agent-system-tools-smoke.ps1` onto `.omx/wpf-smoke-helpers.ps1` while preserving its no-click system-tool/settings proof.
- Dependencies: `.omx/wpf-smoke-helpers.ps1`, `.omx/gui-agent-system-tools-smoke.ps1`, `ProductExperienceTests`, and the built `Css.App.exe`.
- Risks: The smoke must not click system-tool or Windows Settings open buttons; it should only verify list presence, visible open-button affordances, and capture a screenshot.
- Impact scope: `.omx` smoke tooling and static product tests only.
- Acceptance criteria: a failing test first requires the system-tools smoke to dot-source the shared helper and stop owning common UIAutomation/screenshot code; the real GUI smoke still finds Agent system tools and settings lists without launching Windows tools or settings.
- Status: Implemented and verified. `.omx/gui-agent-system-tools-smoke.ps1` now dot-sources `.omx/wpf-smoke-helpers.ps1`, uses shared `Initialize-WpfSmokeAutomation`, `Find-ByAutomationId`, `Wait-Until`, `Invoke-Element`, and `Save-WindowScreenshot`, and keeps only Agent-system/settings checks locally.
- Safety state: tooling-only refactor. The real smoke did not invoke any system-tool or Windows Settings open button. No product behavior, cleanup, uninstall, migration, startup/service/task/registry mutation, settings change, session control, installer execution, or cloud AI path was added.
- Last verification: TDD red observed for `Agent_system_tools_gui_smoke_uses_shared_wpf_smoke_helpers` because the script lacked `wpf-smoke-helpers.ps1`; focused test passed 1/1 after migration. `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors before GUI launch. Real `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentSystemToolListFound=true`, `visibleOpenButtonCount=3`, `agentWindowsSettingsListFound=true`, `visibleSettingsOpenButtonCount=3`; screenshot `.omx/qa-agent-system-and-settings.png`; no `Css.App`/`Css.SmokeTools` process remained. `ProductExperienceTests` passed 101/101; full suite passed 165/165; final solution build passed with 0 warnings/errors.
- Next action: Migrate `gui-agent-settings-confirm-cancel-smoke.ps1` or `gui-agent-background-review-smoke.ps1` to the shared helper, then continue product work on clearer Agent-led remediation flows.

## 2026-07-09 - Active Slice: Agent Settings Confirm-Cancel Smoke Helper Migration

- Objective: Migrate `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` onto `.omx/wpf-smoke-helpers.ps1` while preserving its proof that canceling the settings confirmation does not open Windows Settings.
- Dependencies: `.omx/wpf-smoke-helpers.ps1`, `.omx/gui-agent-settings-confirm-cancel-smoke.ps1`, `ProductExperienceTests`, and the built `Css.App.exe`.
- Risks: The smoke intentionally clicks a medium-risk settings shortcut to open OMNIX-Entropy's confirmation dialog, but must cancel it and verify no new `SystemSettings` process appears.
- Impact scope: `.omx` smoke tooling and static product tests only.
- Acceptance criteria: a failing test first requires the settings smoke to dot-source the shared helper and stop owning common UIAutomation/screenshot code; the real GUI smoke still finds the confirmation dialog, cancels it, and reports `newSettingsProcessCount=0`.
- Status: Implemented and verified. `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` now dot-sources `.omx/wpf-smoke-helpers.ps1`, uses shared WPF automation primitives, and keeps only settings-confirmation-specific mouse/dialog/process checks locally.
- Safety state: tooling-only refactor. The real smoke clicked the Storage settings shortcut only to open OMNIX-Entropy's confirmation dialog, canceled it, and verified no new `SystemSettings` process appeared. No product behavior, settings mutation, cleanup, uninstall, migration, startup/service/task/registry mutation, session control, installer execution, or cloud AI path was added.
- Last verification: TDD red observed for missing helper usage; focused helper test passed 1/1 after migration. The first two real GUI smoke attempts failed: first with `RPC_E_SERVERFAULT` during root-descendant UIAutomation search, then with confirmation dialog not found. Added tests requiring protected root-window search and a Win32 `EnumWindows`/`GetWindowThreadProcessId` fallback; focused settings smoke tests passed 3/3. Real `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` passed with `confirmationDialogFound=true`, `cancelClicked=true`, `newSettingsProcessCount=0`; screenshot `.omx/qa-agent-settings-confirm-cancel.png`; no `Css.App`/`Css.SmokeTools`/new `SystemSettings` process remained. `ProductExperienceTests` passed 104/104; full suite passed 168/168; solution build passed with 0 warnings/errors.
- Next action: Migrate `gui-agent-background-review-smoke.ps1` to the shared helper, then return to product-facing Agent remediation flows.

## 2026-07-09 - Active Slice: Agent Background Review Smoke Helper Migration

- Objective: Migrate `.omx/gui-agent-background-review-smoke.ps1` onto `.omx/wpf-smoke-helpers.ps1` while preserving its read-only software scan and plan-only Agent background/startup-service proof.
- Dependencies: `.omx/wpf-smoke-helpers.ps1`, `.omx/gui-agent-background-review-smoke.ps1`, `ProductExperienceTests`, and the built `Css.App.exe`.
- Risks: The smoke scans installed software and navigates UI, but must not disable startup entries, stop services/processes, edit scheduled tasks or registry, uninstall apps, migrate files, open settings, or call cloud AI.
- Impact scope: `.omx` smoke tooling and static product tests only.
- Acceptance criteria: a failing test first requires the background-review smoke to dot-source the shared helper and stop owning common UIAutomation/screenshot code; the real GUI smoke still passes after a read-only app scan and reports background review plus startup/service plan visibility.
- Status: Implemented and verified. `.omx/gui-agent-background-review-smoke.ps1` now dot-sources `.omx/wpf-smoke-helpers.ps1`, uses shared `Initialize-WpfSmokeAutomation`, `Find-ByAutomationId`, `Wait-Until`, `Invoke-Element`, and `Save-WindowScreenshot`, and keeps only app-scan/Agent-background assertions locally.
- Safety state: tooling-only refactor. The real smoke performed a read-only app scan and verified plan-only Agent UI. It did not disable startup entries, stop services/processes, edit scheduled tasks or registry, uninstall apps, migrate files, open settings, run installers, or call cloud AI.
- Last verification: TDD red observed for `Agent_background_review_gui_smoke_uses_shared_wpf_smoke_helpers` because the script lacked `wpf-smoke-helpers.ps1`; focused test passed 1/1 after migration. `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed before GUI launch. Real `.omx/gui-agent-background-review-smoke.ps1` passed with `appTileCount=120`, `backgroundSummaryFound=true`, `backgroundReviewItemCount=3`, `startupServicePlanFound=true`, `startupServicePlanStepCount=3`; screenshot `.omx/qa-agent-startup-service-plan.png`; no `Css.App`/`Css.SmokeTools`/`SystemSettings` process remained. `ProductExperienceTests` passed 105/105; full suite passed 169/169; final solution build passed with 0 warnings/errors.
- Next action: Return to product-facing work. Good next slice: add an undo-center technical-details affordance so exact paths remain inspectable on demand while beginner-facing rows stay simple.

## 2026-07-09 - Active Slice: Undo Center Collapsed Technical Details

- Objective: Keep undo-center timeline rows beginner-readable by default while preserving exact affected paths, manifest paths, and restore metadata behind an explicit second-level technical-details affordance.
- Dependencies: `ActionTimelinePresenter`, `ActionTimelineItemViewModel`, `TimelinePage` XAML, `.omx/gui-undo-center-smoke.ps1`, `QuarantineAndTimelineTests`, and `ProductExperienceTests`.
- Risks: Do not re-expose long paths in first-level timeline rows; do not click restore in GUI smoke; do not change quarantine/restore execution behavior.
- Impact scope: presentation model, WPF timeline UI, undo-center smoke assertions, and tests only.
- Acceptance criteria: first-level timeline detail stays path-free; collapsed technical details include raw affected paths and manifest paths for auditing; XAML exposes stable `TimelineTechnicalDetailsExpander` and `TimelineTechnicalDetailsListBox` AutomationIds; seeded undo GUI smoke proves the expander exists without invoking restore.
- Status: Implemented and verified. `ActionTimelineItemViewModel` now carries `TechnicalDetailsButtonText` and `TechnicalDetails`; `ActionTimelinePresenter` builds collapsed technical details with record id, source, restore state, restore operation, affected paths, and manifest paths. `MainWindow.xaml` adds the collapsed expander under each timeline row. The undo-center smoke now asserts `technicalDetailsExpanderFound=true`.
- Safety state: UI/presentation only. No cleanup, restore click, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing timeline technical-detail properties and missing XAML/smoke expander hooks. Focused timeline/product tests passed 3/3; seeded undo GUI smoke passed earlier in this slice with `restoreButtonEnabled=true` and `technicalDetailsExpanderFound=true`; fresh `ProductExperienceTests` passed 105/105; fresh full suite passed 170/170; fresh `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Next action: Continue product-facing work toward gated low-risk cleanup/residue flows, with the undo-center now ready to show both beginner summaries and audit details.

## 2026-07-09 - Active Slice: Low-Risk Cleanup Preview Toward Quarantine

- Objective: Start turning low-risk C-drive cleanup findings into beginner-facing action previews that explain what will happen, why quarantine is used, and why nothing is permanently deleted without confirmation.
- Dependencies: C-drive recommendation presentation, cleanup recommendation execution path, `QuarantineOperationPolicy`, `SafetyOperationPipeline`, timeline/quarantine UI, and product tests.
- Risks: Do not add direct deletion, automatic cleanup, or any execution bypass. The first slice should remain plan/preview-oriented unless the existing safety pipeline already handles the operation safely.
- Impact scope: product presentation/tests first; execution behavior should stay gated by existing operation descriptors and quarantine policy.
- Acceptance criteria: a failing test requires low-risk cleanup preview copy to explain "move to quarantine" in plain language; preview must expose estimated impact, reversible state, and confirmation requirement; high-risk items remain explanation-only.
- Status: Implemented and verified. `RecommendationSelectionViewModel` now exposes `CanExecuteDirectly=false`, `AgentTakeaway`, `NextStepText`, `SafetyBoundary`, and `PlanLines`. Low-risk cleanup selections show a structured quarantine-first plan: review evidence and affected-count, move through the local safety pipeline into quarantine, and restore later from Undo Center. `MainWindow.xaml` now displays the plan in a dedicated C-drive recommendation preview panel with stable AutomationIds.
- Safety state: Presentation/selection preview only. The existing execution path still requires a selectable low-risk operation, `QuarantineOperationPolicy`, second confirmation, `SafetyOperationPipeline`, and `QuarantineOperationHandler`. No direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing structured selection fields and missing WPF preview hooks. New focused tests passed 2/2; surrounding C-drive recommendation tests passed 8/8; `ProductExperienceTests` passed 107/107; full suite passed 172/172; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Next action: Add a GUI smoke for the C-drive low-risk cleanup preview when a stable fixture or isolated scan path is available; then continue toward a clearer gated cleanup confirmation window that uses the same structured preview language.

## 2026-07-09 - Active Slice: Low-Risk Cleanup Confirmation Copy

- Objective: Make the final low-risk cleanup confirmation dialog reuse the beginner-friendly quarantine-first language, while keeping raw affected paths in a technical details section.
- Dependencies: `OperationDescriptor`, `RecommendationCardViewModel`, C-drive execution handler, quarantine safety policy, and product tests.
- Risks: Do not weaken second confirmation or safety pipeline gates. Do not hide technical paths entirely before execution; move them below a clear summary.
- Impact scope: confirmation presentation and WPF execution handler only; no execution policy change.
- Acceptance criteria: failing test requires plain-language summary, Agent recommendation, estimated impact, quarantine root, Undo Center restore explanation, affected-count, and separate technical details containing raw paths.
- Status: Implemented and verified. Added `CleanupConfirmationPresenter` / `CleanupConfirmationViewModel`; the low-risk cleanup confirmation now starts with Agent judgment, affected-count, estimated impact, quarantine-first behavior, Undo Center restore language, and local safety-pipeline boundary. Raw affected paths, evidence, operation kind, original confirmation text, and quarantine root are retained in a later technical details section. `ExecuteSelectedRecommendationAsync` now uses the presenter for the confirmation `MessageBox`.
- Safety state: Confirmation presentation only. The execution path still validates with `QuarantineOperationPolicy`, requires the user to press OK in the confirmation dialog, then runs `SafetyOperationPipeline` and `QuarantineOperationHandler`. No direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing `CleanupConfirmationPresenter`; focused confirmation tests passed 2/2; surrounding C-drive confirmation/recommendation tests passed 9/9; `ProductExperienceTests` passed 109/109; full suite passed 174/174; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Next action: Continue toward visual proof for C-drive cleanup preview/confirmation, preferably by adding a stable fixture rather than depending on the local real C-drive state.

## 2026-07-09 - Active Slice: Custom Cleanup Confirmation Dialog

- Objective: Replace the low-risk cleanup `MessageBox` with a small WPF confirmation window that shows beginner summary first and keeps technical paths collapsed by default.
- Dependencies: `CleanupConfirmationPresenter`, WPF windows, C-drive recommendation execution handler, and product tests.
- Risks: Do not weaken the confirmation gate; OK/Cancel behavior must remain explicit. Do not hide technical details entirely.
- Impact scope: WPF confirmation UI and handler integration only; no operation policy or execution handler change.
- Acceptance criteria: failing tests require a `CleanupConfirmationWindow`, stable AutomationIds for summary/details/confirm/cancel, collapsed technical details by default, and handler usage instead of `MessageBox.Show` for cleanup confirmation.
- Status: Implemented and verified. Added `CleanupConfirmationWindow.xaml` / `.xaml.cs`; the cleanup confirmation now shows `CleanupConfirmationPresenter` summary in a custom WPF dialog, keeps `TechnicalDetails` collapsed by default, and exposes stable AutomationIds for summary, details expander/list, confirm, and cancel. `ExecuteSelectedRecommendationAsync` now opens the custom window and proceeds only when `ShowDialog() == true`.
- Safety state: UI/confirmation gate only. The underlying execution rules did not change: low-risk cleanup still requires `QuarantineOperationPolicy`, explicit confirm, `SafetyOperationPipeline`, and `QuarantineOperationHandler`. No direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing custom window and handler still using `MessageBox.Show`; focused window/handler tests passed 2/2; surrounding C-drive tests passed 10/10; `ProductExperienceTests` passed 110/110; full suite passed 175/175; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Next action: Add a stable GUI smoke fixture for the C-drive cleanup preview/confirmation flow, then consider moving real low-risk cleanup execution behind an even clearer operation timeline preview before pressing confirm.

## 2026-07-09 - Active Slice: C-drive Cleanup Preview/Confirmation GUI Fixture

- Objective: Add stable GUI proof for the low-risk C-drive cleanup selection preview and custom confirmation dialog without relying on the user's real C drive.
- Dependencies: C-drive scanner entry point, `RecommendationSelectionPresenter`, `CleanupConfirmationWindow`, `.omx/wpf-smoke-helpers.ps1`, `ProductExperienceTests`, and any dev-only scan fixture plumbing required.
- Risks: The smoke must not execute cleanup, move real files, delete files, touch registry/services/startup/tasks, run installers, or call cloud AI. Any scan fixture must be process-scoped, documented as development/test-only, and keep normal app behavior on the real system drive.
- Impact scope: test/dev tooling and, only if necessary, a guarded dev-only scan-root override. Product default behavior should remain automatic C-drive scanning.
- Acceptance criteria: failing tests require a dedicated C-drive cleanup GUI smoke with isolated roots, a controlled low-risk cleanup candidate, shared helper usage, preview panel assertions, custom confirmation-window assertions, cancel behavior, screenshot output, and no confirmation/execution click.
- Status: Implemented with static/unit/build verification. Added a process-scoped `OMNIX_ENTROPY_CDRIVE_SCAN_ROOT` override for development GUI smoke fixtures, a dedicated `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1`, explicit C-drive cleanup AutomationIds, and app scan rules for top-level `Temp`/`tmp` directories. The smoke creates isolated data/quarantine/scan roots, selects a low-risk cleanup card, opens the custom confirmation dialog, and cancels instead of confirming.
- Safety state: dev/test fixture support only. Normal app runs still scan the automatic system drive unless the process environment override is set. The smoke script is designed to avoid cleanup execution, confirmation, file movement, deletion, registry/service/startup/task mutation, installer execution, settings changes, session control, and cloud AI.
- Last verification: TDD red observed for missing `AppDevelopmentPathResolver`, missing C-drive AutomationIds, missing smoke script, and missing top-level `Temp`/`tmp` rules. Focused fixture/static tests passed 3/3; top-level temp rules test passed 1/1; `ProductExperienceTests` passed 112/112; full suite passed 179/179; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Real GUI smoke launch was attempted but rejected by the approval/usage-limit system, so no fresh screenshot was captured in this slice. Process check found no `Css.App` or `Css.SmokeTools` process.
- Next action: When GUI launch approval/usage is available, run `powershell.exe -ExecutionPolicy Bypass -File .omx\gui-cdrive-cleanup-confirmation-smoke.ps1` and verify it reports `confirmationDialogFound=true`, `cancelClicked=true`, `fixtureStillExists=true`, and a screenshot path. Then continue improving cleanup execution audit flow before allowing broader execution.

## 2026-07-09 - Active Slice: Cleanup Confirmation Outcome Preview

- Objective: Make the low-risk cleanup confirmation dialog explain what happens after confirmation in beginner language before any technical details.
- Dependencies: `CleanupConfirmationPresenter`, `CleanupConfirmationWindow.xaml`, `ProductExperienceTests`, and the existing quarantine/timeline safety model.
- Risks: Do not change execution policy, bypass confirmation, click confirm in smokes, or add any direct-delete/system-mutation behavior.
- Impact scope: confirmation presentation model, WPF confirmation window, and product/static tests only.
- Acceptance criteria: a failing test first requires explicit outcome preview lines for quarantine, undo-center timeline, non-permanent deletion, and safety boundaries; WPF exposes stable AutomationIds and shows the outcome preview before technical details.
- Status: Implemented and verified. `CleanupConfirmationViewModel` now exposes `OutcomePreviewLines`; `CleanupConfirmationPresenter` explains quarantine, undo-center timeline, non-permanent deletion, and safety boundaries. `CleanupConfirmationWindow.xaml` shows the outcome preview before technical details with stable AutomationIds, and the C-drive cleanup GUI smoke script now checks `CleanupConfirmationOutcomeListBox`.
- Safety state: presentation and smoke assertion only. No cleanup execution, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing `OutcomePreviewLines`; focused confirmation tests passed 2/2. TDD red observed for the smoke script missing `CleanupConfirmationOutcomeListBox`; focused smoke static test passed 1/1. `ProductExperienceTests` passed 112/112; full suite passed 179/179; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App` or `Css.SmokeTools`.
- Next action: When GUI launch approval/usage is available, run `.omx\gui-cdrive-cleanup-confirmation-smoke.ps1` for screenshot proof of the new outcome preview, then continue product-facing work on gated uninstall-residue quarantine flow or C-drive post-confirm audit/timeline status.

## 2026-07-09 - Active Slice: Uninstall Plan Window Readability and Hooks

- Objective: Make the "卸载干净点" plan window readable and testable for beginner users without executing any uninstall or residue cleanup.
- Dependencies: `UninstallPlanWindow.xaml`, `UninstallPlanPresentationBuilder`, `AppDrawerActionHostPresenter`, `ProductExperienceTests`, and existing uninstall residue planning code.
- Risks: Do not run official uninstallers, remove residue, edit registry/services/startup/tasks, or add execution gates beyond plan-only UI.
- Impact scope: WPF uninstall plan presentation/static tests only.
- Acceptance criteria: a failing test first requires readable non-mojibake safety text, stable AutomationIds for title/summary/workflow/official-confirmation/sections/final reminder/close button, and clear plan-only language that says official uninstaller and residue deletion are not run.
- Status: Implemented and verified. `UninstallPlanWindow.xaml` now exposes stable AutomationIds for title, summary, safety, official uninstaller, post-scan, workflow, official confirmation, warning/checklist/preflight lists, execution gate, residue sections, final reminder, and close button. Key plan lists are `ListBox` controls for UIAutomation reliability.
- Safety state: UI/readability and test hook only. No uninstall execution, residue cleanup, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing uninstall-plan window hooks; focused test passed 1/1 after XAML update. `ProductExperienceTests` passed 113/113; full suite passed 180/180; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App` or `Css.SmokeTools`.
- Next action: Add a dedicated GUI smoke for opening an app's "卸载干净点" plan window and verifying the new AutomationIds without clicking any execution path, then continue toward safe post-uninstall residue review/quarantine.

## 2026-07-09 - Active Slice: Uninstall Plan Window GUI Smoke Script

- Objective: Add a cancel/close-only GUI smoke script for the "卸载干净点" plan window so future GUI runs can prove it opens and displays the safe plan without executing any uninstaller.
- Dependencies: `.omx/wpf-smoke-helpers.ps1`, `DrawerUninstallButton`, `UninstallPlanWindow` AutomationIds, `ProductExperienceTests`, and built `Css.App.exe`.
- Risks: Script must not run official uninstallers, clean residue, click any future execution gate, edit registry/services/startup/tasks, or rely on a single specific installed app.
- Impact scope: `.omx` smoke tooling and static product tests only.
- Acceptance criteria: a failing test first requires `.omx/gui-uninstall-plan-window-smoke.ps1` to use shared WPF helpers, scan/select an app with enabled `DrawerUninstallButton`, verify key uninstall-plan window controls, save a screenshot, click only `UninstallPlanCloseButton`, and avoid any uninstaller execution markers.
- Status: Implemented and verified, including real GUI smoke. Added `.omx/gui-uninstall-plan-window-smoke.ps1`; it uses the shared WPF helper, scans apps, selects an app with enabled `DrawerUninstallButton`, verifies the uninstall-plan window controls, saves `.omx\qa-uninstall-plan-window.png`, and clicks only `UninstallPlanCloseButton`. The first real GUI run failed because the WPF modal lookup only checked top-level child windows; the script now falls back to finding the stable descendant AutomationId and walking to its parent window.
- Safety state: smoke/tooling-only change. The script does not include any uninstaller execution marker and does not clean residue, permanently delete files, edit registry/services/startup/tasks, migrate files, run installers, change settings, control sessions, or call cloud AI.
- Last verification: TDD red observed for missing smoke script; focused static test passed 1/1 after script addition. Real GUI smoke first failed with "Uninstall plan window was not found"; TDD red then required descendant-window lookup and passed 1/1 after script update. Real `.omx\gui-uninstall-plan-window-smoke.ps1` passed with `planWindowFound=true`, `closedPlanWindow=true`, screenshot `.omx\qa-uninstall-plan-window.png`. Visual inspection showed readable plan-only copy. `ProductExperienceTests` passed 114/114; full suite passed 181/181; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App` or `Css.SmokeTools`.
- Next action: Continue safe post-uninstall residue review/quarantine: make the low-risk residue confirmation use the same clear custom confirmation/outcome pattern as C-drive cleanup, while keeping high-risk residue explanation-only.

## 2026-07-09 - Active Slice: Uninstall Residue Custom Confirmation

- Objective: Replace the low-risk post-uninstall residue `MessageBox` confirmation with the existing beginner-readable `CleanupConfirmationWindow`, so residue cleanup explains quarantine, undo timeline, non-permanent deletion, and technical details before any execution.
- Dependencies: `ReviewSelectedUninstallResidueAsync`, `CleanupConfirmationPresenter`, `CleanupConfirmationWindow`, `QuarantineOperationPolicy`, `SafetyOperationPipeline`, `ProductExperienceTests`, and `UninstallResidueScanTests`.
- Risks: Do not run official uninstallers, automatically clean residue, process medium/high-risk residue, edit registry/services/startup/tasks, or bypass the existing safety pipeline.
- Impact scope: WPF handler integration and static/product tests only; no operation policy or quarantine handler behavior change is intended.
- Acceptance criteria: a failing test first requires the residue flow to create `CleanupConfirmationWindow`, proceed only when `ShowDialog() == true`, and stop using the path-first `BuildResidueConfirmMessage` confirmation.
- Status: Implemented and verified. Low-risk post-uninstall residue confirmation now reuses `CleanupConfirmationPresenter` and `CleanupConfirmationWindow`, including outcome preview and collapsed technical details. The handler still validates with `QuarantineOperationPolicy`, proceeds only after explicit dialog confirmation, and then runs `SafetyOperationPipeline` with `QuarantineOperationHandler`.
- Safety state: confirmation UX only. No official uninstaller execution, automatic residue cleanup, medium/high-risk residue handling, permanent delete, registry/service/startup/task mutation, migration execution, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed because `ReviewSelectedUninstallResidueAsync` still used `MessageBox.Show(BuildResidueConfirmMessage(...))`. Focused red/green test passed 1/1; residue-focused tests passed 10/10; `ProductExperienceTests` passed 115/115; full suite passed 182/182; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Static search found `BuildResidueConfirmMessage` only in negative test assertions.
- Next action: Add a real GUI smoke or fixture for the post-uninstall low-risk residue confirmation path, or run the pending C-drive cleanup confirmation GUI smoke for outcome-preview screenshot proof.

## 2026-07-09 - Active Slice: C-drive Cleanup Confirmation GUI Proof

- Objective: Run the pending cancel-only C-drive cleanup confirmation GUI smoke to capture real screenshot proof for the shared `CleanupConfirmationWindow` outcome preview.
- Dependencies: `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1`, dev-only `OMNIX_ENTROPY_CDRIVE_SCAN_ROOT`, `CleanupConfirmationWindow`, WPF UIAutomation helpers, and the built `Css.App.exe`.
- Risks: The smoke must not click confirm, move fixture files, delete files, change registry/services/startup/tasks, run installers, or call cloud AI.
- Impact scope: verification/tooling only unless the smoke exposes a bug that needs a focused fix.
- Acceptance criteria: smoke reports `confirmationDialogFound=true`, `cancelClicked=true`, `fixtureStillExists=true`, and writes `.omx\qa-cdrive-cleanup-confirmation.png` showing the outcome preview.
- Status: Implemented and verified. The first GUI run failed because the cleanup confirmation modal was not found by root-child window lookup. The smoke now uses the shared descendant modal discovery helper and passed with `confirmationDialogFound=true`, `cancelClicked=true`, `fixtureStillExists=true`, `quarantineItemCount=0`, and screenshot `.omx\qa-cdrive-cleanup-confirmation.png`.
- Safety state: verification/tooling only. The smoke clicked cancel only; no cleanup confirmation, file movement, permanent delete, registry/service/startup/task mutation, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: real `.omx\gui-cdrive-cleanup-confirmation-smoke.ps1` passed after the window-discovery fix; screenshot visually shows the outcome preview in the confirmation window. `ProductExperienceTests` passed 115/115; full suite passed 182/182; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Next action: Keep the shared helper and consider a dedicated uninstall-residue confirmation GUI fixture next.

## 2026-07-09 - Active Slice: Shared WPF Modal Discovery Helper

- Objective: Promote the repeated descendant-based modal window discovery from individual GUI smoke scripts into `.omx/wpf-smoke-helpers.ps1`.
- Dependencies: `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1`, `.omx/gui-uninstall-plan-window-smoke.ps1`, shared WPF helper functions, and `ProductExperienceTests`.
- Risks: Do not loosen smoke safety checks or add any execution clicks. Both scripts must remain close/cancel-only.
- Impact scope: GUI smoke tooling and static tests only.
- Acceptance criteria: failing test requires the shared helper to contain `Find-WindowByDescendantAutomationId` / `Find-SecondaryWindowWithChild`, while the individual smoke scripts call the shared helper instead of duplicating descendant modal discovery implementation.
- Status: Implemented and verified. `Find-WindowByDescendantAutomationId` and `Find-SecondaryWindowWithChild` now live in `.omx/wpf-smoke-helpers.ps1`; both C-drive cleanup confirmation and uninstall-plan smoke scripts call the shared helper instead of defining duplicate modal-discovery functions.
- Safety state: smoke tooling refactor only. The C-drive smoke remains cancel-only and the uninstall-plan smoke remains close-only.
- Last verification: TDD red observed because both scripts still duplicated modal-discovery functions. Focused static tests passed 2/2 after extraction. Real C-drive cleanup smoke passed with `confirmationDialogFound=true`, `cancelClicked=true`, `fixtureStillExists=true`, `quarantineItemCount=0`; real uninstall-plan smoke passed with `planWindowFound=true`, `closedPlanWindow=true`. `ProductExperienceTests` passed 115/115; full suite passed 182/182; build passed with 0 warnings/errors; process check found no `Css.App` or `Css.SmokeTools`.
- Next action: Add a dedicated residue-confirmation GUI smoke fixture, then continue improving post-action inline status/timeline linkage.

## 2026-07-09 - Active Slice: Residue Confirmation GUI Fixture

- Objective: Add a dedicated GUI smoke fixture for the post-uninstall low-risk residue confirmation path without touching real installed software or running uninstallers.
- Dependencies: `ReviewSelectedUninstallResidueAsync`, `SoftwareInventoryScanner` entry points, dev-only environment overrides, `.omx/wpf-smoke-helpers.ps1`, `CleanupConfirmationWindow`, and `ProductExperienceTests`.
- Risks: Do not query or mutate real registry/services/startup/tasks for the fixture path; do not run uninstallers; do not click confirm; do not move or delete real files. Any software inventory fixture must be process-scoped, documented as dev/test-only, and default off.
- Impact scope: handler scan ordering, dev/test fixture plumbing, GUI smoke script, docs, and tests.
- Acceptance criteria: failing tests require residue review to rescan before deciding whether software is still installed; a dev-only software inventory fixture can return scan sequences; a cancel-only GUI smoke opens the residue confirmation window from the app drawer and verifies the outcome preview without moving files.
- Status: Implemented with static/unit/build verification. `ReviewSelectedUninstallResidueAsync` now rescans software inventory before deciding whether the selected app is still installed, then uses the existing low-risk residue confirmation path. Added a dev-only `OMNIX_ENTROPY_SOFTWARE_FIXTURE` JSON scan sequence so GUI smokes can simulate "installed before uninstall" and "gone after uninstall" without touching real registry, services, startup entries, scheduled tasks, or installed apps.
- Safety state: fixture and cancel-only smoke tooling only. No official uninstaller execution, automatic residue cleanup, confirm click, file movement, permanent delete, registry/service/startup/task mutation, migration execution, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for cached-first residue logic, missing software-fixture resolver, missing `SoftwareInventoryFixtureScanner`, and missing residue-confirmation smoke script. Focused residue rescan test passed 1/1; software fixture tests passed 3/3; residue GUI smoke static test passed 1/1; combined focused tests passed 5/5. `ProductExperienceTests` passed 116/116; full suite passed 186/186; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App` or `Css.SmokeTools`.
- Not verified: real `.omx\gui-uninstall-residue-confirmation-smoke.ps1` GUI launch was attempted but rejected by the approval/usage-limit system, so there is no residue-confirmation screenshot yet.
- Next action: When GUI launch approval/usage is available, run `powershell.exe -ExecutionPolicy Bypass -File .omx\gui-uninstall-residue-confirmation-smoke.ps1` and confirm `residueConfirmationFound=true`, `cancelClicked=true`, `residueStillExists=true`, `quarantineItemCount=0`, and screenshot `.omx\qa-uninstall-residue-confirmation.png`. Then continue improving cancel/confirm inline status and undo-center timeline linkage.

## 2026-07-09 - Active Slice: Residue Cancel/Quarantine Inline Outcome

- Objective: Make the post-uninstall residue flow show a clear inline result after cancel or successful quarantine, so beginner users do not have to infer the outcome from a status bar or modal text.
- Dependencies: `UninstallResidueDrawerReviewPresenter`, `ReviewSelectedUninstallResidueAsync`, `AppDrawerActionHostViewModel`, and `ProductExperienceTests`.
- Risks: Do not change residue execution policy, do not auto-confirm cleanup, and do not expose raw local paths in the beginner-facing result.
- Impact scope: residue drawer presentation and WPF handler integration only.
- Acceptance criteria: failing tests require cancel outcome copy saying no files moved and no undo-center record was added; successful quarantine outcome copy saying low-risk residue moved to quarantine and can be restored from the undo center; handler must show those outcomes inline.
- Status: Implemented and partially verified. Added `UninstallResidueDrawerReviewPresenter.CreateCanceled(...)` and `CreateQuarantined(...)`, and wired `ReviewSelectedUninstallResidueAsync` to call `ShowResidueOutcomeInline(...)` after cancel and after successful quarantine.
- Safety state: presentation-only and handler display wiring. No official uninstaller execution, auto residue cleanup, confirmation bypass, high-risk residue handling, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing `CreateCanceled` / `CreateQuarantined`; static handler test was added before WPF integration. Focused new tests passed 2/2; residue/product-focused tests passed 127/127; full suite passed 188/188; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App` or `Css.SmokeTools`.
- Next action: Run the real residue GUI smoke when approval/usage becomes available, then consider adding a success-outcome action that opens the undo center entry without restoring anything in smoke tests.

## 2026-07-09 - Active Slice: Residue Outcome Undo-Center Navigation

- Objective: After successful low-risk residue quarantine, offer a clear app-drawer action to open the undo center, while keeping the action navigation-only.
- Dependencies: `UninstallResidueDrawerReviewViewModel`, `AppDrawerActionHostViewModel`, `DrawerActionPreviewPrimaryButton`, and `ShowPage("Timeline")`.
- Risks: Do not trigger restore, execute cleanup, or run any safety pipeline from the outcome action.
- Impact scope: app drawer action host model, WPF drawer action panel, and residue outcome presentation only.
- Acceptance criteria: failing tests require successful quarantine outcome to expose `PrimaryActionText = "查看后悔药中心"` and `PrimaryActionKey = "Timeline"`; cancel outcome exposes no action; WPF has a stable `DrawerActionPreviewPrimaryButton`; click handler only navigates to Timeline and does not call restore or operation pipeline code.
- Status: Implemented and partially verified. Added optional primary action fields, a hidden-by-default drawer action button, and `DrawerActionPreviewPrimary_Click` with a Timeline-only safe navigation branch.
- Safety state: navigation-only UI. No restore execution, cleanup execution, official uninstaller execution, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing primary action fields and button; focused new tests passed 2/2; residue/product-focused tests passed 128/128; full suite passed 189/189; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App` or `Css.SmokeTools`.
- Next action: Run the real residue GUI smoke when approval/usage becomes available. Extend it to assert the primary action button is hidden after cancel and, later, visually prove the Timeline navigation button from a non-mutating success-like fixture path.

## 2026-07-09 - Active Slice: Residue Cancel Outcome Smoke Assertion

- Objective: Strengthen the cancel-only residue GUI smoke so it proves the post-cancel inline result, not only the confirmation dialog.
- Dependencies: `.omx/gui-uninstall-residue-confirmation-smoke.ps1`, `DrawerActionPreviewTitleTextBlock`, `DrawerActionPreviewPrimaryButton`, and `ProductExperienceTests`.
- Risks: The smoke must remain cancel-only; it must not click confirm, move residue, restore files, or invoke any operation pipeline.
- Impact scope: GUI smoke script and static product tests only.
- Acceptance criteria: a failing test requires the smoke script to verify `DrawerActionPreviewTitleTextBlock` after cancel, assert `DrawerActionPreviewPrimaryButton` stays hidden, and emit `cancelOutcomeVisible=true` / `primaryButtonHiddenAfterCancel=true`.
- Status: Implemented and partially verified. The smoke now waits for the cancel outcome panel after clicking cancel, verifies the primary button is absent/offscreen, and reports both outcome fields in JSON.
- Safety state: smoke assertion only. No product execution behavior changed; no confirm click, restore, cleanup execution, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed because the smoke lacked `DrawerActionPreviewTitleTextBlock` and `DrawerActionPreviewPrimaryButton` checks; focused static smoke test passed 1/1; related focused tests passed 3/3; `ProductExperienceTests` passed 118/118; full suite passed 189/189; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App` or `Css.SmokeTools`.
- Next action: Run the real residue GUI smoke when approval/usage becomes available, then consider adding a second screenshot after cancel to visually prove the inline outcome panel.

## 2026-07-09 - Active Slice: Residue Cancel Outcome Screenshot

- Objective: Make the residue GUI smoke capture a second screenshot after cancel, so visual QA can inspect the inline cancel outcome panel separately from the confirmation dialog.
- Dependencies: `.omx/gui-uninstall-residue-confirmation-smoke.ps1`, `Save-DesktopScreenshot`, and `ProductExperienceTests`.
- Risks: The smoke must remain cancel-only and must not click confirm, restore, or execute any cleanup operation.
- Impact scope: GUI smoke script and static product test only.
- Acceptance criteria: failing test requires `qa-uninstall-residue-cancel-outcome.png` and `cancelOutcomeScreenshot = $cancelOutcomeScreenshotPath` in the smoke script.
- Status: Implemented and partially verified. The script now defines `$cancelOutcomeScreenshotPath`, saves a desktop screenshot after the cancel outcome panel is visible and the primary action button is hidden, and emits `cancelOutcomeScreenshot` in the JSON output.
- Safety state: smoke evidence only. No product execution path changed; no confirm click, restore, cleanup execution, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed because the smoke lacked the second screenshot path; focused static smoke test passed 1/1; related focused tests passed 3/3; `ProductExperienceTests` passed 118/118; full suite passed 189/189; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App` or `Css.SmokeTools`.
- Next action: Run the real residue GUI smoke when approval/usage becomes available and inspect both confirmation and cancel-outcome screenshots.

## 2026-07-09 - Active Slice: Install Routing Learning Memory

- Objective: Move install guard toward the planned learning mode by letting OMNIX-Entropy remember user-chosen software/category install roots and reuse them during read-only installer analysis.
- Dependencies: `InstallRoutingEngine`, `InstallerAnalyzer`, `AppStoragePathResolver`, `MainWindow.AnalyzeInstaller_Click`, and installer tests.
- Risks: Do not run installers, do not auto-pass install arguments, and do not change Windows global install directories.
- Impact scope: install routing model/store, analyzer route selection, app storage path, and install-page analysis output.
- Acceptance criteria: failing tests require exact software memory to override category memory, category memory to override default roots, JSON persistence, WPF analysis loading the memory file, and installer analysis still keeping `WillRunInstaller=false` and `RequiresUserConfirmation=true`.
- Status: Implemented and partially verified. Added `InstallRoutingMemory`, `InstallRoutingMemoryStore`, `FromUserMemory`/`MemoryScope` on routes, optional `routingMemory` in `InstallerAnalyzer.AnalyzePath`, and `install-routing-memory.json` under the app data root. The install-page analysis now loads this memory file and labels the path source.
- Safety state: read-only recommendation logic only. No installer execution, global ProgramFiles change, automatic parameter passing, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing routing memory classes/route fields/store, missing storage path, and WPF handler not loading memory. `InstallerAnalyzerTests` passed 8/8; AppIdentity/WPF focused tests passed 3/3; install/AppIdentity focused tests passed 14/14; `ProductExperienceTests` passed 119/119; full suite passed 192/192; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App` or `Css.SmokeTools`.
- Next action: Add a user-facing "remember this route" confirmation action in the install page without running installers.

## 2026-07-09 - Active Slice: Install Route Remember Button

- Objective: Finish the user-facing install-page action that remembers the current recommended install route after explicit confirmation.
- Dependencies: `MainWindow.AnalyzeInstaller_Click`, `RememberInstallRoute_Click`, `InstallRoutingMemoryStore`, `InstallRememberRouteButton`, and install/product tests.
- Risks: Do not run installers, pass install arguments, globally change Windows install directories, or remember anything before the user confirms.
- Impact scope: install guard WPF page, memory persistence call, and static/product tests only.
- Acceptance criteria: focused tests prove the button appears with a stable AutomationId, stays disabled until analysis, writes `install-routing-memory.json` only through `InstallRoutingMemoryStore.Save(...)` after confirmation, and does not start installers or use the operation pipeline.
- Status: Implemented and verified. The install page now exposes a disabled-by-default `InstallRememberRouteButton`; after read-only installer analysis it can save the current recommended route to `install-routing-memory.json` only after user confirmation.
- Safety state: recommendation memory only. No installer execution, global install-directory change, automatic parameter passing, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added.
- Last verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "InstallerAnalyzerTests|Install_guard|AppIdentityTests"` passed 16/16; `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests` passed 120/120; full `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 194/194; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process.
- Next action: Continue install guard learning mode with a clearer beginner-facing choice for "remember this software only" versus "remember this category", still without running installers.

## 2026-07-09 - Active Slice: Install Route Memory Scope Choice

- Objective: Make install guard learning mode ask whether a remembered route applies only to the current software or to the whole software category.
- Dependencies: `InstallRoutingMemory`, `InstallRouteMemoryChoicePresenter`, `InstallRouteMemoryChoiceWindow`, `RememberInstallRoute_Click`, and install/product tests.
- Risks: Do not run installers, pass install arguments, globally change Windows install directories, or save a rule when the user cancels.
- Impact scope: install routing memory model, install route memory choice window, and install-page WPF handler only.
- Acceptance criteria: TDD red requires category route memory, a scope choice presenter that says it will not run installers, a WPF choice window with stable AutomationIds, and a handler that writes software/category memory based on the selected scope.
- Status: Implemented and verified. The remember action now opens `InstallRouteMemoryChoiceWindow`; users can choose software-only memory or category memory, and cancel writes nothing.
- Safety state: persisted recommendation memory only. No installer execution, global ProgramFiles change, automatic install-argument passing, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing `RememberRouteForCategory` and missing `InstallRouteMemoryChoicePresenter`. Focused new tests passed 3/3; install-focused tests passed 18/18; `ProductExperienceTests` passed 120/120; full suite passed 196/196; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process.
- Next action: Continue install guard UX with a read-only explanation of current learned rules and a reset/edit path, or add a GUI smoke for the install route memory choice window when GUI launch approval is available.

## 2026-07-09 - Active Slice: Learned Install Rules Read-Only View

- Objective: Show learned install routing rules on the install guard page in plain language so users can audit what OMNIX-Entropy remembers.
- Dependencies: `InstallRoutingMemoryStore`, `InstallRoutingMemoryRule`, a new presenter/view model, `MainWindow.xaml`, and install/product tests.
- Risks: Do not delete or edit learned rules in this slice; do not run installers or change Windows install defaults.
- Impact scope: read-only presentation and WPF binding only.
- Acceptance criteria: TDD red requires a presenter that turns software/category memory rules into beginner-readable rows, hides raw JSON, exposes stable AutomationIds on the install page, and loads the rules without writing the memory file.
- Status: Implemented and verified. The install guard page now shows learned install routing rules as beginner-readable rows via `InstallRoutingMemoryPresenter`.
- Safety state: read-only display only. It reads `install-routing-memory.json`; it does not edit, delete, run installers, or alter Windows install defaults.
- Last verification: TDD red observed for missing `InstallRoutingMemoryPresenter`; focused new tests passed 2/2; install-focused tests passed 20/20; `ProductExperienceTests` passed 121/121; full suite passed 198/198; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process.
- Next action: Add a safe "forget learned rule" plan/confirmation that only edits OMNIX-Entropy's memory JSON and never touches installed software.

## 2026-07-09 - Active Slice: Forget Learned Install Rule

- Objective: Let users forget a selected learned install-routing rule, with confirmation that this only affects future recommendations.
- Dependencies: `InstallRoutingMemory.ForgetRule`, `InstallRoutingMemoryPresenter`, `InstallRoutingMemoryListBox`, `ForgetInstallRoutingRuleButton`, and install/product tests.
- Risks: Do not touch installed applications, run installers, move files, change Windows install defaults, or delete anything outside OMNIX-Entropy's memory JSON.
- Impact scope: install routing memory model and install-page WPF handler only.
- Acceptance criteria: TDD red requires presented rules to carry a safe key, selection to enable the forget button only for real rules, confirmation before saving, and handler text stating it only affects future installation advice.
- Status: Implemented and verified. Users can select a learned rule and confirm forgetting it; the handler rewrites `install-routing-memory.json` and refreshes the read-only list.
- Safety state: app-memory edit only. No installer execution, global ProgramFiles change, file movement, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added.
- Last verification: TDD red observed for missing `RuleKey`, `CanForget`, and `ForgetRule`; focused new tests passed 2/2; install-focused tests passed 22/22; `ProductExperienceTests` passed 122/122; full suite passed 200/200; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process.
- Next action: Consider adding an install-page GUI smoke for the remember/forget learned-rule UX when GUI launch approval is available, or continue with post-install change report presentation.

## 2026-07-10 - Active Slice: Post-Install Change Report Cards

- Objective: Turn the install after-change report into beginner-readable cards before the raw technical diff text.
- Dependencies: `InstallSnapshotDiffReport`, a new presentation model, `BuildInstallDiff_Click`, `MainWindow.xaml`, and install/product tests.
- Risks: Do not run installers, capture extra data, change software inventory scanning behavior, or mutate startup/services/tasks/registry.
- Impact scope: presentation and WPF binding only.
- Acceptance criteria: TDD red requires install diff cards for added software, C-drive writes, startup/service/task changes, and a plain safety conclusion; WPF must expose stable AutomationIds and keep the raw diff as technical detail.
- Status: Implemented and verified. The install after-change report now presents beginner-readable summary cards before raw technical details.
- Safety state: read-only presentation only.
- Last verification: TDD red observed: focused install-diff test passed presenter coverage but failed WPF product coverage because `InstallDiffSummaryTextBlock` was missing. After implementation, focused install-diff tests passed 2/2; `ProductExperienceTests` passed 123/123; install-focused tests passed 21/21; full suite passed 202/202; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. Process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process.
- Next action: Add an isolated install-guard GUI smoke for learned rules and post-install diff cards when GUI launch approval/usage is available, or continue improving install-page Agent explanations for what to do after a report finds C-drive writes/background items.

## 2026-07-10 - Active Slice: Install Report Agent Explanation

- Objective: Turn an install change report into an on-demand Computer Agent explanation that tells a beginner what the findings mean and what should happen next.
- Dependencies: existing `InstallSnapshotDiffReport`, beginner report cards, WPF install page, and local safety-boundary wording.
- Risks: Exposing raw paths/service names in the beginner panel, implying that advice has already executed, or adding another always-visible block that makes the install page crowded.
- Impact scope: install-report presentation models, install-page bindings, product-experience tests, and protocol records only.
- Acceptance criteria: C-drive writes and background changes receive distinct plain-language advice; unchanged installs recommend observation; raw paths/service names remain in technical details only; the explanation is revealed on demand; `CanExecuteDirectly` remains false; no installer, migration, service, startup, task, registry, or cleanup operation is invoked.
- Verification expectation: Observe focused tests fail for the missing presenter/UI, implement the minimum behavior, then run install-focused tests, `ProductExperienceTests`, the full test suite, and a solution build.
- Status: Implemented and verified. Install reports now have an on-demand Computer Agent explanation, the install page is vertically scrollable, and the real fixture GUI smoke visually proves the report and explanation.
- Safety state: Read-only local presentation and fixture-only GUI verification. No installer, migration, cleanup, service, startup, task, registry, routing-memory, restore, settings, session, or cloud AI action was executed or added.
- Last verification: TDD red observed for the missing presenter, WPF surface, GUI smoke, and screenshot-state guards. Final focused tests passed 4/4; `ProductExperienceTests` passed 125/125; install-focused tests passed 25/25; full suite passed 206/206; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors. The real GUI smoke returned `fixtureOnly=true`, `reportCardCount=4`, `agentHeadlineVisible=true`, `agentStepCount=4`, and `technicalDetailsCollapsed=true`. Visual inspection of `.omx/qa-install-diff-cards.png` and `.omx/qa-install-diff-agent.png` shows the target content without clipping. No app/smoke process or temporary fixture state remained.
- Next action: Add a separate install-report action-plan surface that turns the Agent explanation into auditable plan choices for C-drive content and background items, still without executing system changes.

## 2026-07-10 - Active Slice: Install Report Action Plan

- Objective: Let Computer Agent turn the install report into a short, ordered treatment plan a beginner can follow without understanding paths, services, or scheduled tasks.
- Dependencies: `InstallSnapshotDiffReport`, the existing Agent explanation panel, `MainWindow.xaml`, the fixture GUI smoke, and install/product tests.
- Risks: Presenting too many technical choices, implying that a plan has executed, exposing raw paths/service names, or calling any system-changing pipeline from the plan button.
- Impact scope: install-report plan presentation models, install-page bindings, fixture smoke evidence, and protocol records only.
- Acceptance criteria: Agent orders C-drive review, background review, and follow-up observation; no-pressure reports recommend no action; every item is non-executable; raw paths/names stay hidden; the plan appears on demand before technical details; real GUI proof shows `尚未执行`.
- Verification expectation: observe missing-presenter/UI/smoke failures, implement the minimum behavior, then run focused tests, install/product suites, the full suite, a solution build, and screenshot-backed GUI smoke.
- Status: Implemented and verified. The install-report Agent can now generate an ordered, beginner-facing action plan before technical details.
- Safety state: plan-only presentation and fixture-only GUI verification. Every item has `CanExecuteDirectly=false`; no cleanup, migration, background change, installer execution, routing-memory edit, restore, settings, session, or cloud AI action was added.
- Last verification: TDD red observed for the missing presenter, WPF surface, smoke contract, and PowerShell-safe Unicode assertion. Final focused tests passed 4/4; `ProductExperienceTests` passed 127/127; install-focused tests passed 29/29; full suite passed 210/210; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors. Real GUI smoke returned `actionPlanItemCount=3`, `nothingExecutedVisible=true`, and `technicalDetailsCollapsed=true`; `.omx/qa-install-diff-action-plan.png` was visually inspected with no clipping. No process or temporary fixture remained.
- Next action: Add read-only Agent classification for each new C-drive location and background item so the generic plan can say what is likely cache/config/install data and what each background mechanism probably does, still without executing changes.

## 2026-07-10 - Active Slice: Install Report Evidence Classification

- Objective: Classify each new C-drive location and background mechanism into beginner-readable purpose/risk groups, then feed a compact conclusion into the Agent plan.
- Dependencies: `InstallSnapshotDiffReport`, action-plan presenter, install-page plan panel, fixture smoke, and install/product tests.
- Risks: Leaking raw paths or service/task names, overstating rule-based confidence, or adding another dense always-visible list.
- Impact scope: read-only presentation/classification models, one compact WPF summary line, tests, smoke evidence, and protocol records.
- Acceptance criteria: classify install/cache/config/log/model-data/unknown locations; classify startup/service/task purpose and caution; retain one review per finding; hide raw identifiers; mark confidence as preliminary; keep `CanExecuteDirectly=false`; show a compact summary before ordered plan items.
- Verification expectation: TDD red for missing classifier and WPF summary, then focused/product/install/full tests, build, and screenshot-backed fixture smoke.
- Status: Implemented and verified. Every new C-drive location and background mechanism receives a hidden-identifier, preliminary classification; the action plan shows one compact summary line.
- Safety state: read-only interpretation and fixture-only GUI proof. All review items and the containing plan remain non-executable; no new system evidence is collected and no system change handler was added.
- Last verification: TDD red observed for missing classifier/enums/summary property and WPF/smoke bindings. Focused tests passed 5/5; `ProductExperienceTests` passed 127/127; install-focused tests passed 32/32; full suite passed 213/213; solution build passed with 0 warnings/errors. Real GUI smoke returned `classificationSummaryVisible=true`, three plan items, `nothingExecutedVisible=true`, and collapsed technical details. The first post-change screenshot had transient black composition blocks and was rejected; an unchanged rerun produced a clean `.omx/qa-install-diff-action-plan.png`, which was visually inspected. No process or temporary fixture remained.
- Next action: Add an on-demand, read-only evidence-review drawer/expander for users who ask “why did Agent judge this way?”, showing generic numbered findings, purpose, confidence, and advice while keeping raw paths/names inside technical details only.

## 2026-07-10 - Active Slice: On-Demand Install Evidence Review

- Objective: Let a beginner ask why Agent reached its install-report conclusion without exposing raw paths, service names, startup names, or scheduled-task names.
- Dependencies: `InstallSnapshotDiffEvidenceReviewPresenter`, the action-plan view model, the install-page action-plan surface, fixture GUI smoke, and install/product tests.
- Risks: Turning the page back into a dense technical report, exposing private identifiers, leaving stale expanded evidence visible after a new report, or implying that review items are executable.
- Impact scope: read-only action-plan presentation, WPF bindings, fixture smoke evidence, tests, and protocol records.
- Acceptance criteria: the evidence review is collapsed by default; it appears directly below the compact classification summary; it exposes generic C-drive and background findings with purpose, advice, confidence, and risk; raw identifiers remain hidden; every review item and the review container stay non-executable; real GUI proof expands the review while technical details remain collapsed.
- Verification expectation: observe focused tests fail for the missing action-plan evidence property and WPF/smoke surface, implement the minimum behavior, then run focused/product/install/full tests, build, and screenshot-backed GUI smoke.
- Status: Implemented and verified. The action plan now carries a read-only evidence review that is collapsed by default and expands into generic C-drive/background findings without raw identifiers.
- Safety state: read-only explanation only. No system scan expansion, cleanup, migration, startup/service/task/registry change, installer execution, routing-memory edit, restore, settings, session, or cloud AI action is authorized in this slice.
- Last verification: TDD red observed for the missing `EvidenceReview` property, missing WPF surface, missing smoke proof, and interactive list styling. Full suite passed 215/215; solution build passed with 0 warnings/errors. Real GUI smoke returned `evidenceReviewCollapsedByDefault=true`, one C-drive item, three background items, `evidenceReviewHidesRawIdentifiers=true`, and `technicalDetailsCollapsed=true`. Clean screenshots `.omx/qa-install-diff-action-plan.png` and `.omx/qa-install-diff-evidence-review.png` were visually inspected. No app process or temporary fixture state remained.
- Next action: Use the classified evidence to derive a short list of plan-only eligible next steps: cache-clean plan, storage-setting guidance, reinstall/migration plan, startup-disable plan, or observe-only. Keep every item non-executable.

## 2026-07-10 - Active Slice: Evidence-Driven Eligible Actions

- Objective: Let Computer Agent turn classified install evidence into a short list of plan types it can safely consider, without asking a beginner to choose technical operations.
- Dependencies: `InstallSnapshotDiffEvidenceReviewViewModel`, its classified C-drive/background items, the on-demand evidence expander, fixture GUI smoke, and install/product tests.
- Risks: Treating a heuristic classification as execution authorization, producing duplicate or conflicting suggestions, adding direct buttons, or hiding missing evidence/rollback requirements.
- Impact scope: read-only recommendation presentation, WPF binding inside the existing collapsed expander, smoke evidence, tests, and protocol records.
- Acceptance criteria: derive cache-clean plan, storage-setting guidance, reinstall/migration plan, startup-disable plan, and observe-only from relevant evidence; deduplicate and order them; explain why each is considered and what evidence is still missing; state rollback/confirmation needs; keep every candidate non-executable and free of raw identifiers.
- Verification expectation: TDD red for missing eligible-action model/rules and WPF/smoke surface, then focused/product/install/full tests, build, and screenshot-backed fixture smoke.
- Status: Implemented and verified. Agent now derives a deduplicated, ordered list of plan-only candidate types from the classified evidence and shows them inside the optional evidence review.
- Safety state: recommendation types only. No operation descriptor, pipeline invocation, cleanup, migration, background/system mutation, installer execution, storage-setting change, session control, or cloud AI action is authorized.
- Last verification: TDD red observed for missing eligible-action models/rules, missing WPF binding, missing smoke proof, unstable focus, and unreliable nested-list offscreen detection. Full suite passed 217/217; solution build passed with 0 warnings/errors. Real GUI smoke returned three eligible actions, `eligibleActionsPlanOnly=true`, hidden identifiers, and collapsed technical details. `.omx/qa-install-diff-eligible-actions.png` was visually inspected and shows candidate reasons, missing evidence, and safety copy without direct buttons. No app process or temporary fixture state remained.
- Next action: Connect each plan-only candidate to an on-demand plan preview that reuses existing safe planners where available (cache quarantine, migration, startup review) and refuses preview when required evidence is missing. Do not add execution handlers.

## 2026-07-10 - Active Slice: On-Demand Candidate Plan Preview

- Objective: Let users ask Agent to expand one evidence-driven candidate into a safe preview, while refusing app-specific previews when evidence cannot be attributed to exactly one newly installed app.
- Dependencies: eligible action kinds, `InstallSnapshotDiffReport.AddedSoftware`, existing cache/startup/migration preview presenters, the collapsed evidence review, fixture GUI smoke, and install/product tests.
- Risks: Assuming all global diff evidence belongs to a new app, leaking paths/registry/service identifiers, turning preview buttons into execution affordances, or bypassing existing planner safety copy.
- Impact scope: install-report preview presentation, WPF on-demand binding, fixture smoke, tests, and protocol records only.
- Acceptance criteria: cache/startup/migration previews reuse existing safe presenters when one added profile owns the relevant evidence; ambiguous/missing ownership is refused with exact missing evidence; storage guidance and observation previews remain generic; previews hide raw identifiers, expose no execution action, and keep `CanExecuteDirectly=false`.
- Verification expectation: TDD red for missing preview model/presenter/UI/smoke, then focused/product/install/full tests, solution build, and screenshot-backed GUI proof.
- Status: Implemented and verified by model/static UI tests; real fixture GUI screenshot proof is pending launch approval.
- Safety state: preview-only. No operation descriptor, pipeline invocation, cleanup, migration, startup/service/task/registry mutation, installer execution, storage-setting change, session control, or cloud AI action is authorized.
- Implementation: Added `InstallSnapshotCandidatePreviewPresenter` and preview status/view models. Cache, startup, and migration candidates reuse existing safe presenters only when exactly one added software profile owns the relevant evidence; ambiguous or missing ownership is refused. Storage and observation remain generic guidance.
- UI: Candidate rows expose only `查看方案预览`; the resulting panel shows status, Agent conclusion, plan lines, missing evidence, and the no-execution boundary. It has no execution control and every preview keeps `CanExecuteDirectly=false`.
- Verification: TDD red was observed for missing preview models/UI and the obsolete GUI activation contract. Focused preview tests passed 5/5; install/product tests passed 146/146 before final smoke hardening; fresh full suite passed 222/222; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI state: Real fixture smoke is not yet accepted. The previous focus-based startup failed because a WPF top-level UIAutomation element was not keyboard-focusable and Windows rejected `SetForegroundWindow`. The shared helper now uses `ShowWindowAsync` plus `SetWindowPos(HWND_TOPMOST)` without requesting focus, and its static contract test passes. Launch approval was then rejected by the Codex GUI usage limit, so `.omx/qa-install-diff-candidate-preview.png` was not generated and no visual claim is made.
- Cleanup: No `Css.App`, `Css.SmokeTools`, or OMNIX process remains; `.omx/qa-install-diff-data` and `.omx/qa-install-diff-software-fixture.json` are absent.
- Exact next action: Completed on 2026-07-10. The fixture smoke returned `candidatePreviewReady=true`, `candidatePreviewNoExecution=true`, hidden raw identifiers, and collapsed technical details; `.omx/qa-install-diff-candidate-preview.png` was visually inspected and is clean.

## Active Slice - 2026-07-10 Beginner-safe uninstall recovery truth

- Objective: Replace the dense uninstall preview with an Agent-first explanation of what can and cannot be undone before any real official-uninstaller execution is considered.
- Dependency: Existing official-command trust, preflight, residue quarantine, and action-timeline foundations remain unchanged.
- Risk: The UI must not imply that quarantining residue can undo the official uninstall itself; raw commands and paths must stay in collapsed technical details.
- Acceptance: A recovery assessment says official uninstall requires reinstall to recover, low-risk residue quarantine is restorable, user data/high-risk items stay untouched, all actions remain preview-only, and a real GUI screenshot proves the beginner conclusion is visible first.
- Verification expectation: TDD red/green, focused product tests, full test suite, solution build, isolated WPF GUI smoke, screenshot inspection, and process/fixture cleanup.
- Status: Completed for this slice. The modal now leads with the Agent recovery conclusion and three steps; official commands, process/service details, and preflight stay in a default-collapsed advanced expander.
- Safety hardening: `OfficialUninstallExecutionGate` now rejects a snapshot id by itself. It also requires acknowledgment that official uninstall has no one-click undo, usable recovery evidence (verified reinstall source or Windows restore point), and confirmed user-data backup when data paths are known.
- Verification: TDD red/green completed; `ProductExperienceTests` passed 132/132; full suite passed 225/225; solution build passed with 0 warnings/errors. GUI smoke returned three protection lines, three steps, collapsed technical details, no execution control, and a clean inspected screenshot at `.omx/qa-uninstall-plan-window.png` after rejecting one black-block capture.
- Cleanup: No `Css.App`, `Css.SmokeTools`, or OMNIX process remains. Install-diff and uninstall-residue temporary fixture/data paths are absent.
- Exact next action: Extend `SoftwareProfile` and the Windows inventory scanner with trustworthy reinstall-source evidence (for example MSI product/source metadata where available), validate it read-only, and surface the Agent's recovery-readiness result. Do not wire an uninstaller process handler until this evidence can be collected and confirmed in the UI.

## Active Slice - 2026-07-10 Read-only reinstall-source discovery

- Objective: Preserve Windows reinstall-source/MSI metadata and let Computer Agent distinguish trustworthy reinstall evidence from weak hints before any official uninstall can be considered.
- Dependencies: `InstalledSoftwareRecord`, `SoftwareInventoryScanner`, `SoftwareInventoryBuilder`, `SoftwareProfile`, uninstall recovery presentation, and scanner/product tests.
- Risk: Registry `InstallSource`, an MSI product code, or an existing directory is only a hint. It must never satisfy the execution gate by itself. Only an existing installer file whose signature matches the software publisher may become automatically usable recovery evidence.
- Impact scope: read-only registry inventory, software-profile fields, recovery-readiness presentation, uninstall preview, tests, GUI smoke, and protocol records. No installer or uninstaller execution.
- Acceptance: Beginner copy hides raw paths and product codes; advanced details retain provenance; missing/unverified metadata cannot produce usable `OfficialUninstallRecoveryEvidence`; a publisher-signed installer file can produce typed reinstall evidence while personal-data backup remains a separate requirement.
- Verification expectation: TDD red/green, focused scanner/product tests, full suite, solution build, isolated uninstall-plan GUI smoke, screenshot inspection, and process cleanup.
- Status: Completed and verified. Windows inventory now preserves `InstallSource`, Windows Installer state, and GUID product-code hints. The uninstall preview validates existing EXE/MSI files against the scanned publisher signature and shows a compact recovery-readiness conclusion before advanced details.
- Safety state: Directory paths, product codes, missing files, unsigned files, and publisher mismatches remain hints and cannot produce usable recovery evidence. Verified installer evidence still has `UserDataBackupConfirmed=false`; no installer or uninstaller process handler exists.
- Verification: TDD red/green covered metadata parsing/mapping, weak hints, missing signatures, signature mismatch, real scanner wiring, real UI wiring, XAML order, and smoke hooks. `SoftwareInventoryTests` passed 15/15; `ProductExperienceTests` passed 137/137; full suite passed 232/232; solution build passed with 0 warnings/errors. Real GUI smoke returned `reinstallReadinessVisible=true`, collapsed details, no execution control, and the clean screenshot `.omx/qa-uninstall-plan-window.png` was inspected.
- Cleanup: No app/smoke process remains; install-diff and uninstall-residue temporary fixture/data paths are absent.
- Exact next action: Add a user-facing recovery-preparation flow that can verify a user-selected official installer or show an existing Windows restore point, while keeping personal-data backup confirmation separate and official uninstall execution disabled.

## Active Slice - 2026-07-10 Guided uninstall recovery preparation

- Objective: Let a beginner prepare and understand recovery before uninstall by verifying a chosen official installer, seeing read-only Windows restore-point availability, and separately acknowledging personal-data backup.
- Dependencies: reinstall-source readiness, Windows WMI read access, uninstall preview WPF, file picker, and product/scanner tests.
- Risks: An old restore point is a fallback hint rather than proof that the application can be fully recovered; choosing a file must not run it; checking a backup box must not imply OMNIX created a backup.
- Impact scope: read-only restore-point scanner, recovery-preparation presenter/session state, uninstall modal controls, tests, GUI smoke, and protocol records. No installer/uninstaller execution or system restore mutation.
- Acceptance: User-selected EXE/MSI is accepted only after publisher-signature verification; cancel/mismatch leaves execution blocked; existing restore points are summarized without raw WMI details; backup acknowledgment is explicit and separate; all state remains local to the preview and non-executable.
- Verification expectation: TDD red/green, focused scanner/product tests, full suite, build, real GUI smoke, screenshot inspection, and process cleanup.
- Status: Implemented with automated verification; real GUI screenshot validation is pending because the GUI launch request was rejected by the Codex usage limit before any process started.
- Implementation: Added read-only `WindowsRestorePointScanner`, user-selected installer verification through the existing publisher-signature rules, separate backup acknowledgment, `UninstallRecoveryPreparationSession`, and compact WPF controls for choosing an installer and viewing readiness.
- Safety state: The WMI adapter uses only a `SELECT ... FROM SystemRestore` query; static tests reject restore/create calls. File selection never launches the selected file. Existing restore points remain fallback hints and do not make preparation complete.
- Verification: TDD red/green covered the model, session, default WMI adapter, WPF hooks, and real composition. `SoftwareInventoryTests` passed 16/16; `ProductExperienceTests` passed 142/142; full suite passed 238/238; solution build passed with 0 warnings/errors.
- Verification gap: The updated `.omx/gui-uninstall-plan-window-smoke.ps1` could not run after implementation because GUI usage was rejected. The previous screenshot does not prove the new controls' layout, so frontend visual status remains Warn.
- Exact next action: When GUI usage is available, run the uninstall-plan smoke unchanged and inspect the new recovery controls. Meanwhile continue backend work on converting a completed preparation session into an auditable final confirmation request without adding a process launcher.

## Active Slice - 2026-07-10 Verifiable uninstall evidence snapshot

- Objective: Replace the arbitrary `SnapshotId` checkbox-equivalent with a real local manifest that records pre-uninstall evidence and can be verified by the official-uninstall gate.
- Dependencies: `Css.Snapshot`, `SoftwareProfile`, recovery evidence, official-uninstall readiness/gate/preflight, and tests.
- Risks: An evidence snapshot must never be described as application rollback. Snapshot paths contain local technical evidence and must remain on-device in OMNIX-owned storage. A stale, missing, mismatched, or fabricated manifest must not satisfy the gate.
- Impact scope: local JSON manifest/store, typed snapshot evidence, official uninstall gate/preflight, tests, and protocol records. No installer/uninstaller, restore point, file deletion, registry/service/startup/task mutation, or cloud transfer.
- Acceptance: Store writes an atomic versioned manifest; manifest says `CanRestoreApplication=false`; validator checks id/path/software/time; gate rejects plain `SnapshotId`; only verified matching evidence can populate a future operation descriptor.
- Verification expectation: TDD red/green, snapshot/product tests, full suite, build, and filesystem cleanup checks.
- Status: Completed and verified for the backend safety slice.
- Implementation: Added `UninstallEvidenceSnapshotStore` with atomic versioned JSON manifests and SHA-256 evidence; added typed snapshot validation for existence/hash, software identity, age, rollback truth, and displayed-id consistency; official uninstall preflight now states that the snapshot supports audit/post-scan comparison and cannot restore the application.
- Safety state: Snapshot files are created only in a caller-provided OMNIX-owned root. No real user snapshot was created during verification; tests used isolated temp roots. The store records technical evidence locally and never launches or mutates the target application.
- Verification: TDD red/green covered missing store, tampering, unbacked ids, id mismatch, stale evidence, wrong software, false rollback claims, operation provenance, and old ready cases. `ProductExperienceTests` passed 144/144; full suite passed 245/245; solution build passed with 0 warnings/errors. No temp snapshot directory or app process remained.
- Exact next action: Add a backend final-confirmation draft service that consumes a completed recovery-preparation session, creates/verifies the local evidence snapshot, and returns an auditable non-executable checklist. Keep WPF integration pending until the crowded recovery panel can be visually checked.

## Active Slice - 2026-07-10 Non-executable uninstall final-confirmation draft

- Objective: Turn completed recovery preparation into a verified local snapshot plus a beginner-readable final-confirmation draft, without creating an operation or process launcher.
- Dependencies: `UninstallRecoveryPreparationViewModel`, `UninstallEvidenceSnapshotStore`, typed recovery/snapshot evidence, and tests.
- Risks: Incomplete preparation must not write a snapshot; successful draft creation must not be mistaken for permission to uninstall; raw paths must stay in technical provenance.
- Impact scope: `Css.Snapshot` orchestration model/service, tests, and protocol records only. No WPF change in this slice because its current panel awaits visual review.
- Acceptance: Incomplete preparation returns a refusal and writes nothing; complete preparation writes and verifies one manifest; draft lists pending close-app/no-undo/rescan/final-confirmation steps; `CanExecuteDirectly=false`; no `OperationDescriptor` or pipeline call exists.
- Verification expectation: TDD red/green, focused draft/snapshot/product tests, full suite, build, and temp-root cleanup.
- Status: Completed and verified for the backend-only slice.
- Implementation: Added `UninstallFinalConfirmationDraftService` with explicit `Refused`, `SnapshotVerificationFailed`, and `ReadyForFinalConfirmation` states. Incomplete preparation writes nothing; complete preparation enriches recovery evidence with the separate backup acknowledgment, creates/verifies one evidence manifest, and returns ready facts plus five pending confirmations.
- Safety state: Every draft has `CanExecuteDirectly=false`. Static source tests reject `OperationDescriptor`, `SafetyOperationPipeline`, `Process.Start`, and `Start-Process`. No WPF control or execution path was added.
- Verification: TDD red/green completed; focused draft tests passed 3/3; full suite passed 248/248; solution build passed with 0 warnings/errors. No temp uninstall directory or app process remained.
- Exact next action: Add a bounded retention/privacy policy for local uninstall evidence manifests (age/count limits, OMNIX-manifest-only deletion, no target-software deletion). Keep WPF draft integration blocked on the pending visual review.

## Active Slice - 2026-07-10 Read-only uninstall snapshot retention plan

- Objective: Bound sensitive local evidence accumulation by identifying old/excess OMNIX manifests without deleting or moving anything.
- Dependencies: uninstall snapshot manifest schema/store and filesystem metadata.
- Risks: Malformed, unrelated, symlinked, or outside-root files must never become candidates; count pruning must retain the newest valid evidence; planning must remain non-executable.
- Impact scope: `Css.Snapshot.Uninstall` retention models/planner, tests, and protocol records only.
- Acceptance: configurable max age/count; valid OMNIX manifests only; deterministic keep/candidate reasons; unknown/corrupt/outside-root evidence preserved; `CanApplyDirectly=false`; no delete/move APIs in planner source.
- Verification expectation: TDD red/green, focused snapshot tests, full suite, build, and temp-root cleanup.
- Status: Completed and verified for the read-only planning slice.
- Implementation: Added `UninstallEvidenceRetentionPlanner` with configurable age/count limits, newest-first deterministic retention, explicit expired/excess reasons, and preservation buckets for malformed, unknown, reparse, or non-root evidence.
- Safety state: Planner enumerates only `uninstall-*.json` in the configured root with `SearchOption.TopDirectoryOnly`, validates schema/purpose/rollback truth/filename-id match, sets `CanApplyDirectly=false`, and contains no delete/move API.
- Verification: TDD red/green completed; focused retention tests passed 4/4; full suite passed 251/251; solution build passed with 0 warnings/errors. No temp uninstall directory or app process remained.
- Exact next action: Convert retention candidates into a reversible archive operation that moves only validated manifests into an OMNIX archive area through `OperationPipeline`, records restore provenance, and never permanently deletes.

## Active Slice - 2026-07-10 Reversible uninstall snapshot archive operation

- Objective: Apply selected retention candidates only through `SafetyOperationPipeline`, move them into OMNIX quarantine/archive storage, record timeline provenance, and support restore.
- Dependencies: retention plan candidates, SHA-256, `FileQuarantineService`, `ActionTimelineStore`, and operation pipeline.
- Risks: Crafted descriptors, changed files, outside-root paths, reparse points, name/schema mismatch, partial multi-file failure, or destination collision must not archive arbitrary files or leave partial state.
- Impact scope: snapshot archive policy/handler, retention item hashes, tests, and protocol records. No permanent deletion and no target-software files.
- Acceptance: preview descriptor is destructive/rollback-required/unconfirmed; pipeline blocks it until confirmed; handler revalidates every source/hash/manifest before moving; failure restores any prior move; timeline is restorable; quarantine restore returns original manifest.
- Verification expectation: TDD red/green, archive/quarantine/timeline tests, full suite, build, and temp-root cleanup.
- Status: Completed and verified for the reversible archive slice.
- Implementation: Retention items now carry SHA-256. Added `UninstallEvidenceArchiveOperationPolicy` and `UninstallEvidenceArchiveOperationHandler`; previews are low-risk destructive, rollback-required, unconfirmed operations. The handler revalidates root/direct-child, file existence, reparse state, planned hash, manifest schema/purpose/name, and entire batch before moving through `FileQuarantineService`.
- Rollback/timeline: Confirmed operations run only through `SafetyOperationPipeline`, record restorable `quarantine.restore` timeline entries, and restore prior moves in reverse order if a later runtime move fails.
- Safety state: No permanent-delete API exists in the handler. Outside-root, changed, unknown, or partial-failure cases leave source manifests in place. Target-software files are not in scope.
- Verification: TDD red/green; focused archive tests passed 6/6; full suite passed 257/257; solution build passed with 0 warnings/errors. No temp uninstall directory or app process remained.
- Exact next action: Implement an unregistered official-uninstaller handler with a fake/injected launcher, strict descriptor revalidation, exit-code capture, and mandatory post-uninstall-rescan result. Do not register or expose it in WPF until recovery UI visual proof and final confirmation wiring are complete.

## Active Slice - 2026-07-10 Unregistered official-uninstaller handler

- Objective: Implement the execution backend behind strict interfaces and fake-launcher tests, while keeping it absent from `Program.cs`, DI, and WPF.
- Dependencies: gate-generated operation descriptor, hashed snapshot manifest, injected launcher, injected post-uninstall scanner, timeline, and pipeline.
- Risks: Crafted descriptor/shell wrapper/external executable, tampered snapshot, nonzero exit, UAC cancellation, post-scan failure, or UI registration could cause unsafe/misreported execution.
- Impact scope: `Css.Elevated.Uninstall`, test project reference, tests, and protocol records. No real launcher adapter or app registration.
- Acceptance: pipeline confirmation required; handler revalidates manifest/hash/command/recovery/backup; only install-root executable or safe interactive MSI allowed; launcher and post-scan are injected; success requires exit 0 and post-scan; timeline says not restorable; source/Program contain no real process start or registration.
- Verification expectation: TDD red/green, elevated/product/full tests, build, source-contract checks, and temp cleanup.
- Status: Completed and verified as an unregistered backend.
- Implementation: Added injected launcher/post-scan contracts and `OfficialUninstallOperationHandler`. It revalidates high-risk flags, confirmation, snapshot hash/id/age/schema, recovery/backup evidence, manifest command equality, file existence, and existing command trust before invoking the launcher. Exit 0 requires post-scan; nonzero/not-started/post-scan-failure states preserve truthful payloads and non-restorable timeline entries.
- Reachability: `Css.Elevated/Program.cs` remains Hello World; App/Program contain no handler/launcher registration; handler contains no `Process.Start` or `ProcessStartInfo`.
- Safety state: Tests use a text file plus fake launcher and fake scanner. No executable is launched. Install-root commands with empty arguments work; changed arguments are blocked. External publisher-signed executables remain blocked in this elevated handler until signature verification can be repeated there.
- Verification: TDD red/green; focused handler tests passed 7/7; full suite passed 264/264; solution build passed with 0 warnings/errors. No temp uninstall directory or app/elevated process remained.
- Exact next action: When GUI usage is available, rerun the recovery-panel smoke and simplify layout if needed. Then implement a real but still unregistered launcher adapter with explicit UAC-cancel handling and an actual post-uninstall scanner adapter; only after separate tests should final confirmation wire them into App.

## Active Slice - 2026-07-10 Unregistered Windows uninstaller launcher adapter

- Objective: Add the real Windows process-start adapter behind the launcher interface, with testable start-info construction and UAC cancellation handling, while keeping it unregistered and uncalled.
- Dependencies: `IOfficialUninstallerLauncher`, `ProcessStartInfo`, injected process runner, and tests.
- Risks: Shell wrapping, wrong working directory, hidden silent arguments, UAC cancellation misreported as failure/success, cancellation while waiting, or accidental App/Program registration.
- Impact scope: `Css.Elevated.Uninstall` adapter/runner and tests only.
- Acceptance: exact executable/arguments, `UseShellExecute=true`, `Verb=runas` only when requested, working directory from executable, exit code captured, Win32 1223 mapped to user-cancelled, no registration, tests use fake runner and launch no process.
- Verification expectation: TDD red/green, focused/full tests, build, source registration checks, and process check.
- Status: Completed and verified as an unregistered adapter.
- Implementation: Added `WindowsOfficialUninstallerLauncher` and `SystemProcessRunner`. The launcher builds exact shell-execute start info, scopes `runas` to elevation requests, sets executable working directory, captures exit code, maps Win32 1223 to user cancellation, preserves operation cancellation, and reports other start failures without claiming success.
- Isolation: `SystemProcessRunner.cs` is the only new file with `Process.Start`; launcher uses `IWindowsProcessRunner`; App and Elevated Program contain no registration/reference.
- Verification: TDD red/green; focused launcher tests passed 6/6; full suite passed 270/270; solution build passed with 0 warnings/errors. No app/elevated process or temp uninstall root remained.
- Exact next action: Implement an unregistered real post-uninstall scanner adapter that consumes a fresh software inventory plus pre-uninstall manifest, reports software-still-present and residue candidates, and performs no cleanup.

## Active Slice - 2026-07-10 Unregistered real post-uninstall scan adapter

- Objective: Implement mandatory read-only post-scan logic from the pre-uninstall manifest plus a fresh software inventory/path probe, without cleanup or registration.
- Dependencies: `UninstallEvidenceSnapshotManifest`, `UninstallResidueScanBuilder`, injected inventory scan/path/size functions, and handler post-scan interface.
- Risks: Treating scan failure as clean uninstall, losing before-state ownership, exposing raw paths in summary, deleting/quarantining during scan, or accidental registration.
- Impact scope: `Css.Elevated.Uninstall` post-scan adapter and tests only.
- Acceptance: reconstruct before profile; report software still present; classify remaining paths via existing residue builder; return typed report/count; scan errors are explicit failure; cancellation propagates; no mutation/pipeline/process API or registration.
- Verification expectation: TDD red/green, focused/full tests, build, source checks, and temp/process cleanup.
- Status: Completed and verified as an unregistered read-only adapter.
- Implementation: Added `InventoryOfficialUninstallPostScanner`. It reconstructs path evidence from the manifest, uses fresh inventory plus path probes, keeps stale background identifiers out of residue groups, and returns them as a separate specialized-rescan requirement.
- Safety state: Inventory failure is explicit, cancellation propagates, mismatched software names are refused, beginner summaries hide paths, and no cleanup/quarantine/timeline/process/pipeline or registration was added.
- Verification: Focused adapter tests passed 6/6; related uninstall tests passed 23/23; full suite passed 276/276; solution build passed with 0 warnings/errors. Process and temporary-evidence checks were empty.
- GUI gate: The updated uninstall recovery panel still lacks a fresh real screenshot because prior GUI launch was rejected by the Codex usage limit. This remains Warn and blocks final WPF execution wiring.
- Exact next action: Add a beginner-facing, non-executable post-uninstall result presenter that turns typed scan outcomes into simple conclusions and next steps without paths. Then rerun `.omx/gui-uninstall-plan-window-smoke.ps1` unchanged when GUI usage becomes available before any handler registration.

## Active Slice - 2026-07-10 Beginner post-uninstall result presentation

- Objective: Turn typed post-uninstall scan outcomes into short, path-free Agent conclusions and next steps for beginners, without adding UI wiring or execution authority.
- Dependencies: `OfficialUninstallPostScanResult`, optional residue report/risk groups, and product presentation tests.
- Risks: Leaking raw paths or identifiers, describing failed scans as clean, offering residue handling while software remains installed, or letting a presentation model create operations.
- Impact scope: `Css.Elevated.Uninstall` presentation model/presenter and tests only.
- Acceptance: distinct failure/still-present/clean/residue outcomes; plain Chinese title/status/advice; counts only; technical-detail availability flag; `CanExecuteDirectly=false`; no operation/pipeline/process/quarantine API.
- Verification expectation: TDD red/green, focused/product/full tests, build, source contract, and process check.
- Status: Completed and verified as a pure non-executable presenter.
- Implementation: Added failure, software-still-present, no-visible-residue, and review-needed states with short conclusions, compact facts, Agent advice, and view/retry labels. Raw scanner summaries are never copied into visible text.
- Safety state: `CanExecuteDirectly=false`; residue review is blocked while software remains installed; no operation, pipeline, process, quarantine, move, or delete API is referenced.
- Verification: Focused presenter tests passed 5/5; product/uninstall tests passed 178/178; full suite passed 281/281; solution build passed with 0 warnings/errors. Process/temp checks were empty.
- GUI gate: No WPF result panel was added. The updated recovery panel still lacks fresh visual proof and remains Warn.
- Exact next action: Add fresh read-only background residue re-enumeration for startup entries, services, and scheduled tasks, keeping identifiers technical-only and refusing partial scan failure. Do not register the launcher/handler.

## Active Slice - 2026-07-10 Fresh background residue re-enumeration

- Objective: Recheck manifest-owned startup entries, services, and scheduled tasks against current Windows state without mutation or registration.
- Dependencies: pre-uninstall manifest identifiers, tri-state reader boundary, post-scan adapter, and residue risk grouping.
- Risks: Treating access failure as absence, probing crafted names outside expected roots, exposing identifiers in beginner text, or turning verified background residue into direct disable/delete authority.
- Impact scope: `Css.Elevated.Uninstall` background scanner/reader, post-scan result fields, presenter count wording, and tests.
- Acceptance: exact-name probes return Exists/Missing/Unknown; duplicates removed; unknown makes background scan incomplete; cancellation propagates; verified current entries enter high-risk report only; identifiers stay technical-only; no mutation or registration.
- Verification expectation: TDD red/green, focused/product/full tests, build, forbidden-API and registration checks, process/temp cleanup.
- Status: Completed and verified as an unregistered read-only evidence layer.
- Implementation: Added Exists/Missing/Unknown exact-name probes, a testable scanner over manifest startup/service/task hints, and a real Windows reader using read-only registry/task-file APIs. Verified current matches enter high-risk residue groups; Unknown fails mandatory background completion.
- Safety state: Crafted identifiers, traversal, reparse points, and access failures cannot become absence. Beginner output contains counts only and states that background records will not be directly closed. No mutation or registration was added.
- Verification: Focused scanner/presenter tests passed 12/12; product/uninstall tests passed 185/185; full suite passed 288/288; solution build passed with 0 warnings/errors. Process/temp/registration checks were empty.
- GUI gate: No WPF wiring was added. The updated uninstall recovery panel still lacks a fresh real screenshot because GUI usage was previously rejected; this remains Warn.
- Exact next action: Audit and model the unregistered elevated request/response composition boundary without registering it. Define how the App would submit a fully confirmed descriptor and receive typed post-scan presentation data, while retaining final GUI screenshot and explicit user-confirmation gates before any real launch.

## Active Slice - 2026-07-11 Final consent and authenticated fake transport

- Objective: prove the complete user confirmation-to-result experience and the authenticated request-to-fake-handler backend path before creating any real elevated IPC reachability.
- Dependencies: Core operation descriptor/gate, final consent contract, one-time visual proof, response presenter, WPF result window, HMAC-SHA256, and fake launcher/post-scanner adapters.
- Risks: forcing Css.App to reference Css.Elevated, enabling confirmation before all user acknowledgements, leaking paths in results, trusting caller hashes, replaying messages, swallowing cancellation, compiling smoke entry points into Release, or registering a real launcher.
- Impact scope: Core consent/response display contracts, final-consent WPF window, DEBUG-only consent-to-result flow, authenticated in-memory transport, fake end-to-end tests, GUI smoke, docs, and records.
- Status: Completed and verified. The confirmation button is disabled until all three plain acknowledgements; accepted Debug consent opens the path-free fake result window. The in-memory transport authenticates metadata with HMAC-SHA256, checks freshness, recomputes the operation hash, rejects replay and mismatched responses, and propagates cancellation.
- Verification: consent model tests 7/7; WPF contract tests 2/2; transport tests 7/7; authenticated real-pipeline/fake-launcher integration 1/1; full suite 326/326. Real GUI smoke proved disabled-to-enabled consent and fake result visibility. Debug and Release solution builds passed with 0 warnings/errors; Release assembly contains no smoke arguments; App/Program registration, project-reference, mutation, process, and temp-root audits passed.
- Remaining boundary: The WPF Debug flow and authenticated backend integration are deliberately separate proofs. There is no serialized named-pipe transport, Windows client/server identity validation, runtime screenshot capture, production final-consent reachability, elevated process launch, or real uninstaller call.
- Exact next action: Add a serialized named-pipe protocol with bounded payloads, protocol/version/schema checks, current-user Windows identity validation, server PID correlation, cancellation/timeouts, and fake endpoint integration. Keep Css.Elevated Program and all real launcher/handler/scanner registrations unchanged until that transport passes tamper and GUI tests.

## Active Slice - 2026-07-11 Post-scan WPF and one-time visual receipt

- Objective: finish the beginner-visible official-uninstall result surface and add the last in-memory request gate needed before any real elevated transport is reachable.
- Dependencies: path-free post-scan presenter, accepted final-confirmation GUI proof, stable AutomationIds, screenshot bytes, and the existing elevated request composer.
- Risks: mojibake reaching users, a result panel accidentally gaining execution authority, stale/replayed visual proof, mutable screenshot buffers, or accidental App/Program registration.
- Impact scope: shared post-scan display model, WPF result window, DEBUG-only GUI fixture, in-memory visual receipt issuer/request session, tests, smoke tooling, and records. No real uninstaller execution or registration.
- Status: Completed and verified. Final-confirmation GUI now shows its status in the visible working area. The post-scan result window shows plain Chinese status/facts/Agent advice/safety text and no execution control. Visual receipts hash PNG bytes, expire after ten minutes, and are single-use; the request session consumes the ticket before composing a request.
- Verification: final-confirmation smoke passed with 2 missing requirements, no evidence-root write, and no execution control; post-scan smoke passed with 3 visible facts and no execution control. Full suite passed 309/309; solution build passed with 0 warnings/errors; runtime registration and mutation-reference audits passed; no App/Elevated process or temporary evidence root remained.
- Remaining boundary: The App does not yet capture runtime PNG evidence, collect the exact final execution consent, authenticate an App-to-elevated channel, register the request session, or call the launcher/handler. The in-memory ticket prevents accidental replay but is not claimed as protection from a hostile local process.
- Exact next action: Design and test an authenticated App-to-elevated request/response transport plus a final consent dialog using a fake launcher end to end. Keep the real launcher, handler, and scanner unregistered until transport correlation, cancellation, visual proof, and result-window integration all pass.

## Active Slice - 2026-07-11 Elevated request/response boundary

- Objective: model the final-confirmation handoff to the elevated official-uninstall handler without registering or invoking real execution.
- Dependencies: `OfficialUninstallExecutionGateResult`, verified snapshot evidence, beginner post-scan presenter, and a future screenshot-backed WPF visual-gate receipt.
- Risks: a caller could otherwise set `ConfirmationAccepted` directly, submit a stale/tampered descriptor, or display raw technical response data.
- Impact scope: backend contracts and tests only; no WPF button, process launch, handler registration, pipeline invocation, or system mutation.
- Acceptance: missing/stale visual proof or mismatched final consent is refused; a ready request is correlation- and hash-bound; response correlation is checked and visible text stays path-free; focused, full, and build verification pass.
- Current status: contract audit complete; TDD RED tests are next.

Project: OMNIX-Entropy

## Active Slice - 2026-07-11 Elevated boundary and recovery GUI gate

- Status: Completed and verified; real execution remains unregistered.
- Request boundary: A ready draft now requires a fresh screenshot-backed UI receipt, exact final confirmation text, all safety acknowledgements, a manual high-risk gate descriptor, a correlation id, and an immutable SHA-256-bound descriptor copy. Missing/stale/mismatched evidence is refused.
- Response boundary: Typed elevated payloads must match the request id; launch failure, uninstall failure, invalid response, and post-scan presentation stay distinct. Beginner-visible text never copies raw handler errors, paths, or identifiers.
- Recovery reliability: The read-only Windows restore-point query now has a four-second WMI/outer timeout and returns Completed/TimedOut/Failed. Timeout is explained as unknown, never as “no restore point,” and the plan window still opens.
- GUI reliability: Shared WPF smoke helpers now fall back to Win32 `EnumWindows` plus `AutomationElement.FromHandle` for owned modal windows that are visible but absent from the UIAutomation root tree.
- Verification: TDD RED/GREEN completed; boundary tests 7/7; related official-uninstall tests 38/38; final full suite 298/298; solution build 0 warnings/errors. GUI smoke passed under the original 10-second gate with three protection lines, three simple steps, collapsed technical details, no execution control, successful close, and inspected screenshot `.omx/qa-uninstall-plan-window.png`.
- Safety state: No handler, launcher, scanner, or request composer is registered in App/Elevated Program; no real uninstaller or installer ran; process/temp checks are empty.
- Exact next action: Add a WPF final-confirmation checklist generated from completed recovery preparation and the existing non-executable draft service. It must show snapshot/reinstall/backup truth, require explicit confirmations, expose no run button, and receive its own AutomationIds/static order test/real screenshot before any execution registration.

## Active Slice - 2026-07-11 WPF final-confirmation checklist

- Status: Implemented and automated-test verified; final visual gate is Warn.
- UI: The recovery panel now offers `生成最终确认清单`. The result panel appears before technical details and shows status, summary, prepared facts, pending confirmations, missing requirements, and a fixed no-execution safety line. Stable AutomationIds exist on the button, title, status, summary, three lists, and safety text.
- Behavior: Incomplete preparation calls the existing draft service, reports missing installer/backup evidence, and does not create the evidence root. Complete preparation can create and verify one audit snapshot, but the UI still has no run button and no pipeline/handler call.
- Storage: Added process-scoped `OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT` resolution for isolated GUI tests; production defaults to LocalAppData/OMNIX-Entropy/Snapshots/Uninstall.
- Verification: TDD RED/GREEN completed for path isolation, WPF contract/order, and smoke contract. Full suite passed 300/300; solution build passed with 0 warnings/errors. Process/temp/evidence-root/forbidden-reference checks were empty.
- GUI evidence: A real run showed the final checklist, at least one missing item, no evidence-root creation, and the correct visible safety sentence. Its diagnostic screenshot contained large desktop-composition black blocks and was rejected. The Unicode-stable smoke assertion was fixed, but the final rerun was rejected by the Codex GUI usage limit, so `.omx/qa-uninstall-plan-window.png` is still the prior pre-checklist accepted screenshot.
- Exact next action: When GUI usage is available, run `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .omx\gui-uninstall-plan-window-smoke.ps1` unchanged. Require `finalChecklistVisible=true`, `finalChecklistMissingCount>=1`, `evidenceRootCreated=false`, `noExecutionControl=true`, and inspect a clean screenshot without black blocks. Only then start the path-free WPF post-uninstall result panel; do not register execution yet.

## Active Slice - 2026-07-11 Elevated boundary and recovery GUI gate

- Status: Completed and verified; real execution remains unregistered.
- Request boundary: A ready draft now requires a fresh screenshot-backed UI receipt, exact final confirmation text, all safety acknowledgements, a manual high-risk gate descriptor, a correlation id, and an immutable SHA-256-bound descriptor copy. Missing/stale/mismatched evidence is refused.
- Response boundary: Typed elevated payloads must match the request id; launch failure, uninstall failure, invalid response, and post-scan presentation stay distinct. Beginner-visible text never copies raw handler errors, paths, or identifiers.
- Recovery reliability: The read-only Windows restore-point query now has a four-second WMI/outer timeout and returns Completed/TimedOut/Failed. Timeout is explained as unknown, never as “no restore point,” and the plan window still opens.
- GUI reliability: Shared WPF smoke helpers now fall back to Win32 `EnumWindows` plus `AutomationElement.FromHandle` for owned modal windows that are visible but absent from the UIAutomation root tree.
- Verification: TDD RED/GREEN completed; boundary tests 7/7; related official-uninstall tests 38/38; final full suite 298/298; solution build 0 warnings/errors. GUI smoke passed under the original 10-second gate with three protection lines, three simple steps, collapsed technical details, no execution control, successful close, and inspected screenshot `.omx/qa-uninstall-plan-window.png`.
- Safety state: No handler, launcher, scanner, or request composer is registered in App/Elevated Program; no real uninstaller or installer ran; process/temp checks are empty.
- Exact next action: Add a WPF final-confirmation checklist generated from completed recovery preparation and the existing non-executable draft service. It must show snapshot/reinstall/backup truth, require explicit confirmations, expose no run button, and receive its own AutomationIds/static order test/real screenshot before any execution registration.
## Active Slice - 2026-07-12 Self-denying production worker command mode

- Objective: register an actual one-shot Elevated command mode that can compose only the official uninstaller launcher and mandatory manifest-bound read-only post-scan, while remaining unreachable from App/WPF and self-denying current unsigned packages before bootstrap.
- Dependencies: strict shared worker metadata parser, manifest-aware post-scanner factory, minimal fail-closed uninstall-registry inventory reader, background residue reader, timeline storage path, production package authorizer/session, and real unsigned-process denial smoke.
- Risks: constructing a post-scanner for a different manifest, nested UAC from an already elevated worker, allowing fake-only delay switches in production, accidentally exposing the production mode from App, reaching real process launch before trust/request checks, or leaving a worker process behind.
- Impact scope: Elevated handler/parser/production worker/Program and minimal read-only inventory reader, focused integration/static tests, and records. Production WPF stays disconnected; tests must not launch an uninstaller or mutate residues/system state.
- Acceptance: production mode accepts only six bounded metadata pairs; actual pipe peer and identical trusted signer are required before bootstrap; request freshness/descriptor/snapshot checks remain mandatory; handler builds the scanner from the exact validated manifest; official launcher runs without a second elevation request inside the elevated worker; post-scan is read-only and mandatory; current unsigned real worker roundtrip stops before request/launcher; App contains no production-mode string or composition.
- Status: Completed and verified. Elevated now registers a strict `official-uninstall-production-worker` mode. It composes the independently signed package gate, authenticated/fresh one-shot session, `SafetyOperationPipeline`, exact-manifest handler, already-elevated official launcher, minimal fail-closed read-only installed-software reader, background scanner, residue report, and timeline. App/WPF contains no production mode or session reference. Current unsigned real worker self-denies before bootstrap and exits without request transfer or uninstaller launch.
- Last verification: production mode/handler/lifecycle focused 57/57 Debug and Release; full 427/427; Debug/Release builds 0 warnings/errors. Release binary dual-encoding audit finds mode/session only in Elevated, Elevated deps excludes Css.Scanner, mutation scan is empty, registry reads are non-writable, and no process remains.
- Blockers: Positive end-to-end native production execution cannot be run until OMNIX App/worker are signed with the same certificate; injected trusted integration covers the allowed branch.
- Exact next action: generalize the App lifecycle from fake-only completion to typed production completion/failure, add a production-mode launcher that is constructible only from `CanLaunchProduction` trust evidence, and test the full one-shot response with injected trusted package/launcher/scanner. Keep WPF execution disconnected until result presentation and manual UAC evidence pass.
