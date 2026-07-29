# Archived quality-gates (2026-06-30 to 2026-07-10)

Historical entries moved out of `.omx/development/quality-gates.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

## 2026-06-30 - App drawer migration preview gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Drawer migration output is presentation-only; no file movement handler or operation execution path was added. | Real migration remains blocked until snapshot, rollback, app-close checks, and post-migration monitoring exist. |
| Data, API, and consistency | Pass | `AppDrawerViewModel` exposes `MigrationSummary` and `MigrationPreviewLines`; tests cover C-drive, D-drive, cache-only, and system-tool cases. | Destination roots reuse existing category mapping. |
| Code quality and maintainability | Pass | `dotnet build src\Css.App\Css.App.csproj --no-restore` and `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | `MainWindow.xaml.cs` remains code-behind heavy until UX stabilizes. |
| Testing and verification | Pass | Focused migration tests passed 4/4; `ProductExperienceTests` passed 30/30; full suite passed 80/80. | Tests do not move real files. |
| Frontend, accessibility, and UX | Warn | XAML compiles with the new migration preview section. | No fresh GUI screenshot/click-through verification for the drawer preview yet. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no installer/updater. |

Open issues:

- GUI verify app drawer migration text readability for one C-drive app and one D-drive app.
- Add a separate migration plan page before any real migration executor exists.

## 2026-06-30 - Official uninstall preflight checklist

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `OfficialUninstallPreflightChecklistBuilder` wraps `OfficialUninstallExecutionGate`; no uninstaller execution handler was added. | Checklist cannot bypass the gate because it surfaces the gate result and operation only when the gate allows it. |
| Data, API, and consistency | Pass | Tests cover missing snapshot/confirmation/rescan states, all-ready operation exposure, and confirmation model exposure. | Step keys are stable ASCII ids for UI and tests. |
| Code quality and maintainability | Pass | Preflight logic is isolated in `Css.Core.Apps`; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Command parsing is duplicated with the gate and confirmation builder; later refactor should share it. |
| Testing and verification | Pass | Targeted preflight/gate tests passed 9/9; `ProductExperienceTests` passed 26/26; full suite passed 76/76. | Tests do not run uninstallers. |
| Frontend, accessibility, and UX | Warn | `UninstallPlanWindow.xaml` compiles and renders the preflight checklist binding. | No fresh GUI screenshot/click-through was run; checklist text uses English until localization is cleaned up. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | Repository still has no initial commit and no installer/updater. |

Open issues:

- GUI verify checklist readability and wrapping.
- Replace duplicated uninstall command parsing with a shared parser before adding more execution UI.

## 2026-06-30 - Publisher signature trust for external uninstallers

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | External uninstallers pass only when normalized publisher text appears in normalized executable signature subject; shell wrappers and unsafe MSI commands are still blocked first. | No process execution, registry mutation, service change, or cleanup handler was added. |
| Data, API, and consistency | Pass | `OfficialUninstallExecutionGate` passes `SoftwareProfile.Publisher` and `SignatureSubject` to the trust evaluator; targeted publisher tests passed 3/3. | Trust depends on scan-time signature evidence; missing evidence remains blocked. |
| Code quality and maintainability | Pass | Logic remains isolated in `Css.Core.Apps`; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | A richer trust evidence object may be cleaner than more optional parameters later. |
| Testing and verification | Pass | Trust/gate regression tests passed 11/11; `ProductExperienceTests` passed 23/23; full suite passed 73/73. | Tests do not run uninstallers. |
| Frontend, accessibility, and UX | Warn | Existing command trust summary binding compiles through `dotnet build src\Css.App\Css.App.csproj --no-restore`. | No fresh GUI screenshot/click-through was run for the trust text. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | Repository still has no initial commit and no installer/updater. |

Open issues:

- GUI verify trust summaries for inside-install, MSI, and publisher-signed external uninstallers.
- Design final official-uninstaller execution UI before adding any process-launch handler.

## 2026-06-30 - Safe MSI official uninstall trust

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `OfficialUninstallCommandTrustEvaluator` trusts only interactive MSI product uninstall commands and blocks silent/reduced-UI flags plus MSI install/repair commands. | No process execution, registry mutation, service change, or cleanup handler was added. |
| Data, API, and consistency | Pass | `OfficialUninstallExecutionGate` now passes parsed arguments to the trust evaluator; targeted tests passed 8/8. | MSI trust is based on executable path and arguments, not executable name alone. |
| Code quality and maintainability | Pass | Trust logic remains isolated in `Css.Core.Apps`; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Command parsing is still duplicated and should be shared later. |
| Testing and verification | Pass | Targeted trust/gate tests passed 8/8; `ProductExperienceTests` passed 20/20; full suite passed 70/70. | Tests are deterministic and do not run uninstallers. |
| Frontend, accessibility, and UX | Warn | `UninstallPlanWindow.xaml` already binds command trust summary and builds successfully through `dotnet build src\Css.App\Css.App.csproj --no-restore`. | No fresh GUI screenshot/click-through was run this slice. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | Repository still has no initial commit and no installer/updater. |

Open issues:

- Add signer/publisher matching and known vendor metadata before exposing any real official uninstaller execution handler.
- GUI verify the uninstall safety window with an MSI app.

## 2026-06-30 - Official uninstall command trust

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `OfficialUninstallCommandTrustEvaluator` blocks shell wrappers and outside-install-directory uninstallers; no execution handler was added. | This prevents obvious shell-command abuse before any real process launch exists. |
| Data, API, and consistency | Pass | Product tests cover trusted install-directory paths, shell-wrapper blocking, outside-directory blocking, and gate blocking for suspicious shell commands. | MSI-style uninstall commands are intentionally not trusted yet. |
| Code quality and maintainability | Pass | Trust logic is isolated in `Css.Core.Apps`; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Command parsing is still duplicated between confirmation and gate. |
| Testing and verification | Pass | Targeted trust tests passed 4/4; `ProductExperienceTests` passed 16/16; full suite passed 66/66. | No destructive operation was executed. |
| Frontend, accessibility, and UX | Warn | `UninstallPlanWindow.xaml` compiles and shows command trust summary. | No GUI screenshot/click-through was run. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | No installer/updater; repository still has no initial commit. |

Open issues:

- Add safe MSI uninstall recognition.
- Add signer/publisher trust checks.
- GUI verify the command trust summary in the uninstall safety window.

## 2026-06-30 - Official uninstaller execution gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `OfficialUninstallExecutionGate` is disabled by default; the WPF window only displays gate status. | No process execution, registry mutation, service change, or uninstaller handler was added. |
| Data, API, and consistency | Pass | Product tests cover disabled-by-default, missing snapshot/close/rescan blockers, and the high-risk operation descriptor shape. | The descriptor is not executable yet because no handler or final confirmation flow exists. |
| Code quality and maintainability | Pass | `OfficialUninstallExecutionGate` lives in `Css.Core.Apps`; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Command parsing is duplicated with the confirmation builder; later refactor can share a parser. |
| Testing and verification | Pass | Targeted gate tests passed 4/4; `ProductExperienceTests` passed 12/12; full suite passed 62/62. | No destructive operation was executed. |
| Frontend, accessibility, and UX | Warn | `UninstallPlanWindow.xaml` compiles and shows execution gate status/blocking reasons. | No fresh GUI screenshot/click-through was run. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | No installer/updater; repository still has no initial commit. |

Open issues:

- GUI verify the uninstall safety window shows gate status clearly.
- Add command trust/signer checks before any real official uninstaller execution.
- Add a final confirmation flow and handler only after snapshot and rollback strategy are verified.

## 2026-06-30 - Uninstall residue review UI gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `ReviewSelectedUninstallResidueAsync` only executes `review.LowRiskOperation` after `QuarantineOperationPolicy.ValidateCandidate` and a user confirmation. If the software still appears installed, `UninstallResidueScanBuilder` produces no operation. | No official uninstaller, registry, service, startup, or scheduled-task execution path was added. |
| Data, API, and consistency | Pass | `UninstallResidueScanTests` cover low-risk residue exposure and blocking when software still exists. | UI uses the existing scan report and operation planner rather than duplicating classification rules. |
| Code quality and maintainability | Pass | `dotnet build src\Css.App\Css.App.csproj --no-restore`: 0 warnings, 0 errors. `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 0 warnings, 0 errors. | MainWindow remains code-behind heavy until the UX stabilizes. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter UninstallResidueScanTests`: 6/6 passed. Full suite: 59/59 passed. | No real destructive operation was executed during tests. |
| Frontend, accessibility, and UX | Warn | XAML compiles with the new `DrawerResidueReviewButton`. | No fresh GUI screenshot/click-through for the new button in this run. |
| Operations, dependencies, and release | Warn | No new dependencies or packaging changes. | No installer/updater; repository still has no initial commit. |

Open issues:

- GUI verify the post-uninstall residue-review button visibility, click behavior, cancellation behavior, and text readability.
- Design the official uninstaller execution gate before enabling real uninstall.

### 2026-06-30 - V1 foundation delivery gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `SafetyOperationPipeline` blocks destructive operations without confirmation/evidence and snapshot-required operations without `SnapshotId`; covered by `V1FoundationTests`. | Real destructive executors are not implemented yet. |
| Data, API, and consistency | Warn | New public models compile: `Recommendation`, `SoftwareProfile`, `MigrationPlan`, `ScanSnapshot`, `InstallRoutingRule`, `ActionTimelineEntry`. | SQLite persistence is still pending. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore` completed with 0 warnings and 0 errors. | UI is a static shell pending binding. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 12 tests. | No UI automation yet. |
| Frontend, accessibility, and UX | Warn | WPF XAML builds and exposes all V1 module entries. | Keyboard/focus/visual QA not yet performed. |
| Operations, dependencies, and release | Warn | No new NuGet package was added; existing solution builds. | No release packaging, updater, or rollback executor yet. |

Open issues:

- Implement real C disk UI binding and SQLite growth storage.
- Implement real snapshot/quarantine/elevated execution before any destructive action ships.

### 2026-06-30 - Runnable C disk scan gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | WPF scan path calls only `DiskScanner`, `DiskRecommendationBuilder`, and `ScanSnapshotStore`; no cleanup/migration executor is wired. | SQLite stores local path/size/category data under `%LocalAppData%\ComputerAssistant\data.db`. |
| Data, API, and consistency | Pass | `ScanSnapshotStore` tested with temp SQLite db; `DiskScanSessionBuilder` tested for report/recommendation/growth aggregation. | No migrations yet; schema is first-use create. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 0 warnings, 0 errors. | UI is still code-behind, acceptable for first runnable test pass. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: 14/14 passed. | Full C drive scan not manually completed yet. |
| Frontend, accessibility, and UX | Warn | `Css.App.exe` launched and closed successfully (exitCode=0). | No screenshot or keyboard/accessibility QA yet. |
| Operations, dependencies, and release | Warn | App can run from `src\Css.App\bin\Debug\net8.0-windows\Css.App.exe`. | No installer/package yet. |

Open issues:

- Manual end-to-end C drive scan still needs to be run and observed.
- Real quarantine/snapshot/elevated worker remains required before enabling any destructive action.

### 2026-06-30 - Software inventory scan gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `SoftwareInventoryScanner` reads uninstall registry keys, Run keys, WMI `Win32_Service`; no write APIs are used. | Authenticode inspection reads local executable certificates only. |
| Data, API, and consistency | Warn | `SoftwareInventoryTests` cover category classification, dedupe, startup/service matching, signature subject mapping. | Scheduled task source is modeled but not yet scanned from Windows. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 0 warnings, 0 errors. | Matching heuristics are intentionally simple for first landing test. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: 18/18 passed. | No real-machine software scan click-through captured yet. |
| Frontend, accessibility, and UX | Warn | `Css.App.exe` launched and closed successfully (exitCode=0); UI exposes “扫描软件”. | No screenshot/keyboard QA yet. |
| Operations, dependencies, and release | Warn | No new NuGet dependency added; scanner uses existing Windows/.NET APIs. | No installer/package yet. |

Open issues:

- Add scheduled task scanning.
- Run manual UI software scan and capture result/screenshot.

### 2026-06-30 - Installer analysis gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `InstallerAnalyzer` analyzes path/name only and sets `WillRunInstaller=false`; WPF analysis does not start any process. | It provides candidate arguments only as text. |
| Data, API, and consistency | Warn | `InstallerAnalyzerTests` cover route mapping, MSI candidate args, Inno/NSIS hints. | Detection is heuristic; no binary metadata inspection yet. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 0 warnings, 0 errors. | Logic is isolated under `Css.InstallGuard.Installers`. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: 24/24 passed. | No manual installer path UI test captured yet. |
| Frontend, accessibility, and UX | Warn | `Css.App.exe` launched and closed successfully (exitCode=0); UI exposes installer path analysis. | No screenshot/keyboard QA yet. |
| Operations, dependencies, and release | Warn | No installer/package yet. | Actual install interception/diff remains future work. |

Open issues:

- Add install-before/install-after snapshot diff report.
- Improve detection with version metadata and known installer signatures.

### 2026-06-30 - Install diff and scheduled task scan gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `InstallSnapshotDiffBuilder` compares in-memory `SoftwareProfile` snapshots only; WPF buttons call `_softwareScanner.ScanAsync()` and do not run installers. `SoftwareInventoryScanner` reads task XML files and skips unreadable/malformed files. | No destructive operation was added. |
| Data, API, and consistency | Pass | `InstallSystemSnapshot` / `InstallSnapshotDiffReport` expose before/after times, added software, startup, services, scheduled tasks, and C drive paths. | Snapshots are not persisted yet. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 10 projects, 0 warnings, 0 errors after serial rerun. | Build/test should not be run in parallel against shared obj/bin outputs. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: 28/28 passed. | Manual UI click-through of the new buttons is still pending. |
| Frontend, accessibility, and UX | Warn | `Css.App.exe` launched and closed successfully (exitCode=0); XAML compiles with new install snapshot buttons. | No screenshot, keyboard, or accessibility QA yet. |
| Operations, dependencies, and release | Warn | No new NuGet dependency added; app still runs from debug output. | No packaged installer/updater yet. |

Open issues:

- Manually test “捕获安装前/捕获安装后/生成变化报告” in the WPF UI.
- Persist install snapshots if the user wants to compare across app restarts.
- Add real quarantine/restore before enabling cleanup actions.

### 2026-06-30 - Quarantine and timeline gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `QuarantineOperationHandler` is exercised behind `SafetyOperationPipeline`; tests verify unconfirmed destructive operations are blocked and missing paths fail before moving anything. | Real UI cleanup execution remains disabled. |
| Data, API, and consistency | Pass | `FileQuarantineService` writes manifest JSON; `ActionTimelineStore` persists title, evidence, affected paths, restore state, and restore operation kind in SQLite. | No schema migrations yet. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 10 projects, 0 warnings, 0 errors. | Quarantine capacity/retention policy is still pending. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: 33/33 passed. | No manual restore UI test because restore UI is not implemented yet. |
| Frontend, accessibility, and UX | Warn | `Css.App.exe` launched and closed successfully (exitCode=0); WPF exposes “加载时间线”. | No screenshot, keyboard, or accessibility QA yet. |
| Operations, dependencies, and release | Warn | Uses existing `Microsoft.Data.Sqlite` dependency in `Css.Core`; no new package added. | Elevated worker and packaged installer remain pending. |

Open issues:

- Wire a confirmation page from cleanup recommendation cards to `QuarantineOperationHandler`.
- Add restore action UI that reads manifest paths and refuses overwrite.
- Add quarantine root policy, retention days, max size, and low-space handling.

### 2026-06-30 - Low-risk cleanup execution gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `QuarantineOperationPolicy` tests require `clean.temp`, `RiskLevel.Low`, evidence, affected paths, and rollback before confirmation. WPF calls `SafetyOperationPipeline` before `QuarantineOperationHandler`. | Only low-risk temp cleanup is enabled. |
| Data, API, and consistency | Pass | Confirmed operation preserves descriptor fields and sets `ConfirmationAccepted=true`; timeline is refreshed from `ActionTimelineStore` after success. | No remote/cloud data involved. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 10 projects, 0 warnings, 0 errors. | UI remains code-behind for first runnable test loop. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: 34/34 passed. | Manual click-through of confirmation dialog is pending. |
| Frontend, accessibility, and UX | Warn | XAML compiles and exposes a selected-card confirmation button plus status text. | No fresh WPF launch/screenshot this turn due prior escalation quota limit; manual UI QA pending. |
| Operations, dependencies, and release | Warn | No new package added; quarantine root defaults to `D:\CssQuarantine` when D exists, otherwise LocalAppData fallback. | No packaged installer or elevated worker yet. |

Open issues:

- Manual end-to-end test: scan, select low-risk temp cleanup card, confirm, verify file moves to quarantine and timeline refreshes.
- Add restore UI from timeline entries.
- Add quarantine retention and max-size policy.

### 2026-06-30 - Manual UI feedback gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | UI changes preserve the existing `QuarantineOperationPolicy` and `SafetyOperationPipeline`; no new direct delete/move/registry/service write path was added. | The confirmation action still only allows low-risk `clean.temp`. |
| Data, API, and consistency | Pass | `SoftwareInventoryTests.Profile_builder_ignores_registry_placeholder_display_names` covers `${...}` placeholder filtering. | No schema change. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 10 projects, 0 warnings, 0 errors. | Enum mismatch was fixed and recorded in `error-ledger.md`. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: 36/36 passed. UI Automation invoked all 8 left navigation buttons. | Low-risk quarantine execution still needs manual end-to-end testing. |
| Frontend, accessibility, and UX | Warn | `.omx\qa-omnix-ui-current.png` shows `OMNIX-Entropy` title not clipped; drive input is now a dropdown; decision copy is simpler. | Full keyboard/focus/accessibility QA remains pending. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. App still runs from debug output. | No installer/package yet. |

Open issues:

- Manual end-to-end test: scan real C drive, select a low-risk card, confirm quarantine, verify timeline refresh.
- Implement restore UI and clearer in-app confirmation panel.

### 2026-06-30 - V1 intuitive manager refactor gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AgentSkillCatalog` marks risky process/service/session capabilities as plan-only; WPF still routes real cleanup through existing quarantine pipeline. | No new direct delete, registry, service, or installer execution path was added. |
| Data, API, and consistency | Pass | `ProductExperienceTests` cover `HealthCheckSummary`, app tile/drawer, Agent skill catalog, and uninstall plan shape. | App filtering/sorting is not bound yet. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 10 projects, 0 warnings, 0 errors. Static `rg` check found no old `BringIntoView`/old control references. | MainWindow remains code-behind heavy until UX stabilizes. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: 41/41 passed. Narrow tests also passed for product experience and running process association. | No destructive/manual system operation was tested. |
| Frontend, accessibility, and UX | Warn | UI Automation clicked all 6 left navigation entries and verified matching page titles; screenshot `.omx\qa-omnix-v1-refactor-clicks.png` inspected. | Full keyboard/focus and screen-reader QA remain pending. |
| Operations, dependencies, and release | Warn | No new NuGet dependency or packaging change. App runs from debug output. | No installer/updater/release packaging yet. |

Open issues:

- Bind app page filtering, search, and sorting.
- Add safe uninstall-plan preview before any real uninstall.
- Run a Marvis-only read scan validation on this machine.

### 2026-06-30 - App management loop gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AppPresentationBuilder.CreateDrawer` only creates `UninstallPreviewLines`; `PreviewUninstall_Click` sets preview/status text and does not run uninstall commands. | Real uninstall remains intentionally disabled. |
| Data, API, and consistency | Pass | `ProductExperienceTests.App_catalog_filters_searches_and_sorts_beginner_tiles` and `App_drawer_contains_uninstall_preview_without_executing_uninstall` cover the new public behavior. | App category taxonomy still maps “办公学习” to `SoftwareCategory.Normal` until richer categories exist. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 10 projects, 0 warnings, 0 errors. | WPF still uses code-behind while UX is stabilizing. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: 43/43 passed. | No destructive operation was tested or executed. |
| Frontend, accessibility, and UX | Warn | UI Automation found 12 key app-management controls; screenshot `.omx\qa-omnix-app-management-loop-accessible.png` inspected. | Full keyboard and screen-reader flow still pending. |
| Operations, dependencies, and release | Warn | No new dependencies or packaging changes. | App still runs from debug output; no installer yet. |

Open issues:

- Validate Marvis in a real read-only software scan.
- Replace inline uninstall preview with a clearer full confirmation page before enabling any real uninstall flow.

### 2026-06-30 - Marvis scan and uninstall plan window gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `UninstallPlanPreviewViewModel.CanRunOfficialUninstaller=false`; `PreviewUninstall_Click` opens `UninstallPlanWindow` and still does not execute commands. | No real uninstall/delete/service change was added. |
| Data, API, and consistency | Pass | `SoftwareInventoryTests.Profile_builder_infers_marvis_root_category_size_service_and_processes` covers Marvis root, AI category, service/process association, size. | Directory size is bounded to avoid long scans and may undercount very large trees. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 10 projects, 0 warnings, 0 errors. | Service registry fallback is isolated through `ServiceEntryFactory`. |
| Testing and verification | Pass | Default `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: 47/47 passed. Explicit real-machine Marvis test with `OMNIX_REAL_MACHINE_TESTS=1`: 1/1 passed. | GUI smoke for the new modal window was not completed due approval/usage rejection. |
| Frontend, accessibility, and UX | Warn | `UninstallPlanWindow.xaml` compiles and has `AutomationProperties.Name="卸载安全方案窗口"`. | Need visual screenshot and click-through once GUI launch approval is available. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | No installer/updater yet. |

Open issues:

- Run GUI click-through for `UninstallPlanWindow`.
- Design real official uninstaller confirmation and post-uninstall residue scan before enabling execution.

### 2026-06-30 - Timeline restore UI gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `FileQuarantineService.RestoreAsync` refuses restore when the original path already exists; WPF restore button calls this service and never overwrites. | Restore only applies to entries with `RestoreOperationKind="quarantine.restore"` and manifest paths. |
| Data, API, and consistency | Pass | `ActionTimelineStore` persists `RestoreManifestPaths`, loads row `Id`, and updates restore state with `UpdateRestoreStateAsync`; `QuarantineOperationHandler` writes manifest paths. | Old rows without manifest remain visible but not actionable. |
| Code quality and maintainability | Pass | `ActionTimelinePresenter` keeps timeline button state outside WPF code-behind; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed. | MainWindow still has code-behind until UX stabilizes. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter QuarantineAndTimelineTests`: 8/8 passed. Full suite: 49/49 passed. | GUI click-through of the restore button is still pending. |
| Frontend, accessibility, and UX | Warn | XAML compiles with per-row restore buttons and tooltips explaining no-overwrite behavior. | No fresh screenshot/UIA due prior GUI approval limits. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | Still no installer/updater; repository has no initial commit. |

Open issues:

- Manually test low-risk cleanup -> timeline -> restore -> state refresh in the WPF app.
- Add quarantine capacity and retention policy before encouraging frequent cleanup.

### 2026-06-30 - Quarantine retention policy gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `QuarantineRetentionPlanner.WouldDeleteAutomatically=false`; candidates require confirmation. `LoadRecordsAsync` only reads manifest files. | No permanent delete execution path was added. |
| Data, API, and consistency | Pass | `QuarantineAndTimelineTests` cover manifest inventory and expired/over-capacity/already-restored candidate classification. | Candidate execution still pending. |
| Code quality and maintainability | Pass | Retention logic is isolated in `QuarantineRetentionPlanner`; WPF only renders a summary. `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed. | Formatting still uses scanner `RootCauseReportBuilder.Fmt` in WPF. |
| Testing and verification | Pass | `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter QuarantineAndTimelineTests`: 10/10 passed. Full suite: 51/51 passed. | No GUI screenshot due prior approval limits. |
| Frontend, accessibility, and UX | Warn | XAML compiles with `TimelineQuarantinePolicyTextBlock` showing policy summary. | Needs visual check for text length and wrapping. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | No packaged installer/updater; repository has no initial commit. |

Open issues:

- Add a confirmation page and safety-pipeline handler before allowing users to permanently remove quarantine copies.
- GUI verify the policy summary in the 后悔药中心.

### 2026-06-30 - Uninstall residue scan gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `UninstallResidueOperationPlanner` only includes low-risk path candidates; services/startup/tasks are high risk and excluded. `QuarantineOperationPolicy` still requires confirmation and rollback. | No official uninstaller execution was added. |
| Data, API, and consistency | Pass | `UninstallResidueScanBuilder` distinguishes software still installed vs. removed, and groups low/medium/high residue candidates. | Real post-uninstall inventory diff UI is pending. |
| Code quality and maintainability | Pass | Residue scan, operation planning, and presentation are separated across `Css.Core.Software` and `Css.Core.Apps`. `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed. | Need a later cleanup if more residue kinds are added. |
| Testing and verification | Pass | `UninstallResidueScanTests`: 4/4 passed. `ProductExperienceTests`: 7/7 passed. Full suite: 55/55 passed. | No GUI screenshot/click-through yet. |
| Frontend, accessibility, and UX | Warn | `UninstallPlanWindow.xaml` compiles with `PostUninstallScanLine`. | Needs visual check for line wrapping and readability. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | No installer/updater; repository still has no initial commit. |

Open issues:

- Add official uninstaller confirmation page before any execution.
- After official uninstall completes, run a fresh software scan and residue scan, then show low-risk quarantine plan.

### 2026-06-30 - Official uninstall confirmation gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `OfficialUninstallConfirmationViewModel.CanRunOfficialUninstaller=false`; `UninstallPlanWindow` only displays command details. | No process execution path was added. |
| Data, API, and consistency | Pass | `ProductExperienceTests` cover quoted command parsing, missing command blocking, running-process/service/task warnings, snapshot and post-uninstall scan requirements. | Command parsing is basic and presentation-focused. |
| Code quality and maintainability | Pass | Confirmation model is isolated in `Css.Core.Apps`; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed. | Future execution gate should reuse this model rather than parse again. |
| Testing and verification | Pass | `ProductExperienceTests`: 9/9 passed. Full suite: 57/57 passed. | No GUI screenshot/click-through yet. |
| Frontend, accessibility, and UX | Warn | `UninstallPlanWindow.xaml` compiles with official confirmation card. | Needs visual QA for long command wrapping. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | No installer/updater; repository still has no initial commit. |

Open issues:

- Add GUI verification for the uninstall safety window.
- Design the opt-in execution gate for official uninstallers.

## 2026-07-01 - App drawer top-summary localization

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Only `AppPresentationBuilder` summary/advice text and presentation tests changed. | No cleanup, uninstall, migration, service, startup, registry, or file-move path was added. |
| Data, API, and consistency | Pass | `App_drawer_top_summary_uses_plain_chinese_before_technical_details` asserts Chinese location, size, residency, and Agent advice text before technical details. | Old C-drive advice assertion now checks `迁移方案` instead of English `migration plan`. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | String literals still need a future resource table once UI stabilizes. |
| Testing and verification | Pass | Focused test passed 1/1; `ProductExperienceTests` passed 43/43; full suite passed 98/98. | First focused run had a test compile error and was not counted as red; corrected run failed for the expected old-English behavior. |
| Frontend, accessibility, and UX | Warn | Static `rg` check found old English summary phrases only in a negative test assertion. | GUIA drawer-summary read was not run because escalation was rejected by usage limits; no workaround was attempted. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no packaged installer/updater. |

Open issues:

- GUI-verify drawer summary text when GUI launch approval/usage is available.
- Continue localizing migration/uninstall window body text.
- GUI-verify uninstall preflight and post-uninstall residue review cancellation behavior.

## 2026-07-01 - App drawer action localization

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Only presentation strings and WPF labels changed; no cleanup, uninstaller, migration, registry, service, startup, or file-move handler was added. | GUI smoke inspected buttons but did not invoke operation buttons. |
| Data, API, and consistency | Pass | `App_drawer_actions_use_beginner_friendly_chinese_labels_and_reasons` asserts the five public drawer action labels and no English action reasons. | C# strings use Unicode escapes; XAML uses numeric character references. |
| Code quality and maintainability | Pass | `dotnet build src\Css.App\Css.App.csproj --no-restore` and `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Longer-term localization should move to a resource table. |
| Testing and verification | Pass | Focused test passed 1/1; `ProductExperienceTests` passed 42/42; full suite passed 97/97. | TDD red was observed first: old English labels failed the focused test. |
| Frontend, accessibility, and UX | Pass | GUI smoke launched the app, scanned apps, selected one app, and UIAutomation found `卸载干净点`, `迁移到 D 盘`, `清理缓存`, and `关闭自启动`. | Broader visual layout and text wrapping QA remains needed for the full migration/uninstall windows. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no packaged installer/updater. |

Open issues:

- Localize the remaining body text inside migration and uninstall safety windows.
- GUI-verify uninstall preflight and post-uninstall residue review cancellation behavior.
- Add real snapshot evidence before any future migration execution request.

## 2026-07-01 - App tile Chinese status labels

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Only `AppPresentationBuilder.CreateTile` status text and presentation tests changed. | No cleanup, uninstall, migration, service, startup, registry, or file-move path was added. |
| Data, API, and consistency | Pass | `AppTileViewModel.ShortTag` and `AccessibilityName` now use Chinese status text while preserving path-hiding tests. | Implemented with C# Unicode escapes to keep source edits ASCII-safe. |
| Code quality and maintainability | Pass | `dotnet build src\Css.App\Css.App.csproj --no-restore` and `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Further localization should move toward a resource table later. |
| Testing and verification | Pass | Focused status-label tests passed 2/2; `ProductExperienceTests` passed 41/41; full suite passed 96/96. | No destructive operation was executed. |
| Frontend, accessibility, and UX | Pass | GUI smoke read 130 app UI items and sampled Chinese labels such as `火绒安全软件, 需关注`; no old English status tags were sampled. | Broader visual layout QA still needed for long localized text. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no packaged installer/updater. |

Open issues:

- Localize migration/uninstall window text and action button labels.
- GUI verify uninstall plan and post-uninstall residue review flows.

## 2026-07-01 - App tile accessibility names

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Only `AppTileViewModel`, WPF binding, and presentation tests changed. | No cleanup, uninstall, migration, service, startup, registry, or file-move action was added. |
| Data, API, and consistency | Pass | `AppTileViewModel.AccessibilityName` is derived from app name and short status only, and tests assert it does not expose install paths. | Status text is still English and should be localized later. |
| Code quality and maintainability | Pass | `dotnet build src\Css.App\Css.App.csproj --no-restore` and `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | WPF binding is direct and small. |
| Testing and verification | Pass | Focused tile test passed 1/1; `ProductExperienceTests` passed 40/40; full suite passed 95/95. | First attempted focused filter matched 0 tests and was not used as evidence. |
| Frontend, accessibility, and UX | Pass | GUI smoke read 130 app UI items and sampled real names such as `火绒安全软件, Needs attention`; no sampled item used `AppTileUi`. | Screenshot capture had wrong foreground window and is not used as visual evidence for this gate. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no packaged installer/updater. |

Open issues:

- Localize app status tags and remaining English migration/uninstall text.
- Bring WPF window to foreground before future visual screenshots.

## 2026-07-01 - Migration rollback manifest UI action

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `MigrationRollbackManifestCreationService` writes only a JSON manifest; `MigrationPlanWindow` still says preview only and no migration handler was added. | GUI confirmation wrote one plan-only evidence file under `%LocalAppData%\OMNIX-Entropy\MigrationRollback`; no app files were moved. |
| Data, API, and consistency | Pass | Presentation options now accept readiness evidence and manifest existence checks; tests verify rollback manifest readiness and destination-space readiness. | Snapshot id is still placeholder evidence until a real snapshot flow is added. |
| Code quality and maintainability | Pass | `dotnet build src\Css.App\Css.App.csproj --no-restore` and `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | `MainWindow.xaml.cs` remains code-behind heavy while UX stabilizes. |
| Testing and verification | Pass | Focused tests passed 2/2; placeholder regression passed 1/1; `ProductExperienceTests` passed 40/40; full suite passed 95/95. | Tests do not execute real migration. |
| Frontend, accessibility, and UX | Warn | GUI smoke captured `.omx\qa-migration-manifest-created.png` after scanning apps and confirming rollback-manifest creation. | App tile automation names are still generic; migration text remains mixed English/Chinese. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no packaged installer/updater. |

Open issues:

- Fix app tile automation names and localize remaining English strings.
- GUI verify uninstall plan and post-uninstall residue review flows.
- Add real snapshot evidence before any future migration execution request.

## 2026-07-01 - Migration rollback manifest and space probe

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `MigrationRollbackManifestBuilder` builds in-memory evidence; `MigrationRollbackManifestStore` writes JSON only when called; `MigrationDestinationSpaceProbe` only reads drive free-space. | No file movement, service change, registry change, or redirect creation was added. |
| Data, API, and consistency | Pass | Manifest entries include original path, planned destination, restore path, monitor paths, and rollback steps. | The actual migration handler must consume this manifest later instead of recomputing paths. |
| Code quality and maintainability | Pass | `dotnet build src\Css.App\Css.App.csproj --no-restore` and `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Destination mapping is still duplicated across presentation paths and should be unified. |
| Testing and verification | Pass | `MigrationSafetyTests` passed 4/4; `ProductExperienceTests` passed 38/38; full suite passed 92/92. | No real migration was tested or executed. |
| Frontend, accessibility, and UX | Warn | `MigrationPlanWindow.xaml` compiles with rollback manifest and destination-space lines. | No fresh GUI screenshot/click-through was run for line wrapping/readability. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no installer/updater. |

Open issues:

- GUI verify migration plan readability.
- Add a user-confirmed UI action to write the rollback manifest draft and refresh readiness.

## 2026-07-01 - Migration readiness gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `MigrationExecutionGate` blocks by default and requires snapshot, rollback manifest, app-close confirmation, free-space check, and monitoring confirmation; no handler was added. | It can only create a descriptor, not move files. |
| Data, API, and consistency | Pass | `MigrationPreflightChecklistBuilder` exposes stable step keys and uses the same gate result as the future execution model. | Destination and affected path estimation remain heuristic. |
| Code quality and maintainability | Pass | `dotnet build src\Css.App\Css.App.csproj --no-restore` and `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Destination mapping is still duplicated between drawer, presentation, and future planning code. |
| Testing and verification | Pass | Focused migration gate tests passed 4/4; `ProductExperienceTests` passed 37/37; full suite passed 87/87. | Tests do not execute migration. |
| Frontend, accessibility, and UX | Warn | `MigrationPlanWindow.xaml` compiles with the readiness checklist binding. | No fresh GUI screenshot/click-through was run for text wrapping and readability. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no installer/updater. |

Open issues:

- GUI verify the migration readiness checklist.
- Add a rollback manifest generator and destination free-space probe before considering any real migration handler.

## 2026-07-01 - Migration plan window gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `MigrationPlanWindow` only receives `MigrationPlanPreviewViewModel`; no file movement handler, operation descriptor, service change, registry change, or shortcut redirect was added. | Real migration remains blocked. |
| Data, API, and consistency | Pass | `MigrationPlanPresentationBuilder` uses `MigrationPlanner` and exposes snapshot, rollback, blocker, and monitoring sections. | Destination root mapping duplicates the drawer mapping and should be shared later. |
| Code quality and maintainability | Pass | `dotnet build src\Css.App\Css.App.csproj --no-restore` and `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | WPF code-behind still owns button orchestration. |
| Testing and verification | Pass | Focused migration presentation tests passed 3/3; `ProductExperienceTests` passed 33/33; full suite passed 83/83. | Tests verify presentation and safety state, not real migration. |
| Frontend, accessibility, and UX | Warn | XAML compiles and the button is wired through `PreviewMigration_Click`. | No fresh GUI screenshot/click-through was run for the new window. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no installer/updater. |

Open issues:

- GUI verify the migration plan window with a C-drive app and a D-drive app.
- Add a migration readiness gate before any real migration operation descriptor exists.

### 2026-07-07 - Migration evidence wording gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Migration checklist wording only changed display evidence; no handler or operation execution path was added. | No file move, service, startup, scheduled-task, or registry mutation was invoked. |
| Data, API, and consistency | Pass | `MigrationPreflightChecklistBuilder` now names snapshot evidence and confirmed plan scope. | `MigrationExecutionGate` still blocks execution unless all readiness fields pass and feature enablement is true. |
| Code quality and maintainability | Pass | Change is localized to checklist presentation and a product test. | Existing migration model remains preview/gate based. |
| Testing and verification | Pass | TDD red observed; focused test passed 1/1; `ProductExperienceTests` 53/53; full suite 110/110; solution build 0 warnings/0 errors. | Verification commands used current workspace state. |
| Frontend, accessibility, and UX | Warn | User-facing checklist text is clearer. | No GUI screenshot was run for this text-only checklist update. |
| Operations, dependencies, and release | Warn | No packaging/dependency change. | Repository still has no initial commit. |

### 2026-07-07 - Residue review inline short-circuit gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Still-installed residue review creates no operation descriptor and `CanExecuteDirectly=false`. | No uninstall, delete, quarantine movement, migration, service, startup, scheduled-task, or registry action was invoked. |
| Data, API, and consistency | Pass | `UninstallResidueReviewPlanner.TryBuildStillInstalledReport(...)` uses existing app inventory to block unsafe residue handling before a full rescan. | Low-risk residue quarantine path still requires official uninstall completion, policy validation, user confirmation, and safety pipeline. |
| Code quality and maintainability | Pass | Added planner and `ShowResidueReviewInline(...)`; build passed 0 warnings/0 errors. | WPF code-behind remains heavy. |
| Testing and verification | Pass | TDD red observed for missing safety fields and missing planner; `UninstallResidueScanTests` passed 8/8; full suite passed 109/109. | Build: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed. |
| Frontend, accessibility, and UX | Warn | Model and WPF path now support inline drawer feedback. | GUI proof for this new inline path is pending because the previous GUI command was interrupted. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit. |

Open issues:

- Re-run a lightweight GUI proof later using a fake/cached inventory path instead of full real-machine software scanning.

### 2026-07-07 - C-drive automatic target and report-collapse gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Changes only alter display chrome and report visibility; no operation descriptor or handler was added. | No cleanup, quarantine, uninstall, migration, service, startup, scheduled-task, or registry action was invoked. |
| Data, API, and consistency | Pass | `CDrivePageChromePresenter` models automatic system-drive display, non-editable path UI, and default-collapsed technical report. | Hidden `DriveRootComboBox` still provides the scanner root; current default remains C drive. |
| Code quality and maintainability | Pass | Product chrome is now testable outside WPF; solution build passed with 0 warnings and 0 errors. | `MainWindow.xaml.cs` remains code-behind heavy and should be view-modeled later. |
| Testing and verification | Pass | TDD red observed for missing presenter; focused test passed 1/1; `ProductExperienceTests` 52/52; full suite 107/107. | Build: `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed 0 warnings/0 errors. |
| Frontend, accessibility, and UX | Pass | GUIA real C-drive scan found system-drive label, report toggle, report hidden before/after scan, 4 root-cause items, 3 growth items, 15 recommendations. | Screenshot: `.omx\qa-cdrive-system-drive-and-collapsed-report.png`. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and all files remain untracked. |

Open issues:

- The right-side recommendation copy is readable but still repetitive; a future pass should group similar "confirm source first" findings.
- The actual cleanup button remains quarantine-gated, but wording can be improved further to explain "why quarantine" inline.

### 2026-07-07 - Inline homepage Agent response gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `HomeAgentResponsePresenter` only returns text view models with `CanExecuteDirectly=false`; no operation descriptor is created. | No cleanup, delete, migration, uninstall, service, startup, scheduled task, or registry action was added. |
| Data, API, and consistency | Pass | Focused tests require explain/detail/plan responses to be non-executable and include safety-pipeline language. | The actual processing still belongs to decision cards and safety pipeline. |
| Code quality and maintainability | Pass | Removed unused homepage MessageBox formatting helpers; build passed with 0 warnings and 0 errors. | `MainWindow.xaml.cs` remains code-behind heavy overall. |
| Testing and verification | Pass | Focused tests 3/3, `ProductExperienceTests` 51/51, full suite 106/106, solution build 0 warnings/0 errors. | TDD red was observed for the missing presenter and for insufficient safety copy. |
| Frontend, accessibility, and UX | Pass | GUIA real-scan verification found inline Agent answer, plan, safety text, and `processWindows=1`; screenshot `.omx\qa-home-agent-inline-response-visible.png`. | The first screenshot showed the panel was too low; it was moved above the list and reverified. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | Repository still has no initial commit. |

Open issues:

- Continue replacing modal confirmations with in-app decision panels where it improves clarity, while keeping high-risk confirmations explicit.

### 2026-07-07 - Homepage key finding Agent buttons gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `HealthFindingAgentExplanation.cs` models explanation/detail/plan only; `CanExecuteDirectly=false`; no operation descriptor is created. | No cleanup, delete, migration, uninstall, service, startup, scheduled task, or registry action was added. |
| Data, API, and consistency | Pass | Tests require Agent/detail/plan visible text to hide raw `C:\` paths and remain non-executable. | Finding text remains presentation-level; actual actions still belong to decision cards and safety pipeline. |
| Code quality and maintainability | Pass | Builders live in `Css.Core.Apps`; WPF handlers only format and show the resulting view models. | Future work should move MessageBox flow into an in-app panel. |
| Testing and verification | Pass | Focused tests 2/2, `ProductExperienceTests` 50/50, full suite 105/105, solution build 0 warnings/0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Warn | Static check confirms `ExplainHealthFinding_Click`, `ShowHealthFindingDetails_Click`, and `CreateHealthFindingPlan_Click` are wired in XAML. | Real GUI click-through not run due prior usage-limit constraints. |
| Operations, dependencies, and release | Warn | No new dependency or packaging change. | Repository still has no initial commit. |

Open issues:

- GUI-verify the three homepage key-finding buttons after a real scan.
- Replace MessageBox explanations with an in-app Agent panel when the UX shape stabilizes.

### 2026-07-07 - C-drive beginner summary and growth cards gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | No operation handlers were changed; the work only adds presenters and WPF bindings. | No cleanup, delete, migration, service, startup, scheduled task, or registry action was executed. |
| Data, API, and consistency | Pass | `CDriveRootCauseSummaryBuilder` and `GrowthFindingPresenter` are covered by product tests that require beginner text and hide raw `C:\` paths in primary cards. | Technical report remains available for audit/debug context. |
| Code quality and maintainability | Warn | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 0 warnings, 0 errors. Static check shows `GrowthListBox.ItemsSource` only in scan reset and `GrowthFindingPresenter.CreateList(...)`. | An older unused recommendation-card view from prior work still remains. |
| Testing and verification | Pass | Full suite: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 103/103; `ProductExperienceTests` passed 48/48. | TDD red was observed for both new presenters before implementation. |
| Frontend, accessibility, and UX | Warn | Real GUI scan before the summary-card change verified right-side C-drive recommendation cards; screenshot `.omx\qa-cdrive-cards-real-scan.png`. | Post-change GUI verification for the new left-side summary cards was rejected by usage limits. |
| Operations, dependencies, and release | Warn | No dependency or packaging changes. | Repository still has no initial commit. |

Open issues:

- Run post-change GUI visual verification for the C-drive summary and growth cards once approval/usage limits allow.
- Remove the older unused recommendation-card view from `MainWindow.xaml.cs` when safe.

- Pass: verified with evidence.
- Warn: partial coverage or residual risk.
- Fail: issue must be fixed or explicitly deferred.
- N/A: not relevant to this project or change.

## 2026-07-07 - C-drive recommendation card presentation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Only presentation/model binding changed; `ExecuteSelectedRecommendationAsync` still requires a selected operation and existing safety pipeline policy. | No cleanup, delete, migration, registry, service, or startup operation was executed. |
| Data, API, and consistency | Pass | `C_drive_recommendation_card_explains_happened_agent_advice_undo_and_impact` covers the new public presentation fields and keeps the original `OperationDescriptor`. | Underlying `Recommendation` remains the scanner/AI data contract. |
| Code quality and maintainability | Warn | `RecommendationCardPresenter` moves active card copy out of WPF code-behind; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed. | Old unused private `RecommendationCardView` remains because the mojibake block could not be patched safely. |
| Testing and verification | Pass | Focused C-drive card test passed 1/1; `ProductExperienceTests` passed 46/46; full suite passed 101/101. | TDD red was observed for the missing presenter. |
| Frontend, accessibility, and UX | Warn | XAML now binds separate `WhatHappened`, `AgentSuggestion`, `UndoStatus`, and `ImpactText` lines. | No fresh C-drive page screenshot/UIA scan was run for text wrapping. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no packaged installer/updater. |

Open issues:

- GUI-verify the C-drive recommendation cards after a real scan.
- Safely remove the old unused code-behind `RecommendationCardView` during a future encoding-safe cleanup.

## 2026-07-07 - Uninstall safety copy localization

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `CanRunOfficialUninstaller` remains false in `Uninstall_safety_window_body_uses_plain_chinese_while_official_uninstaller_stays_disabled`; GUIA only opened the preview modal. | No uninstaller process, residue quarantine, service/startup/registry change, or delete path was executed. |
| Data, API, and consistency | Pass | Product tests cover the localized uninstall modal/preflight copy and localized drawer uninstall preview lines. | `uninstall.official.run` remains only a gated descriptor model with no handler/UI execution path. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore`: 0 warnings, 0 errors. | Strings remain inline Unicode escapes; future resource extraction is still desirable. |
| Testing and verification | Pass | Focused modal test passed 1/1; focused drawer preview test passed 1/1; `ProductExperienceTests` passed 45/45; full suite passed 100/100. | TDD red was observed for both changed visible behaviors. |
| Frontend, accessibility, and UX | Pass | GUIA launched the WPF app, scanned apps, selected `火绒安全软件`, opened `卸载安全方案窗口`, found required Chinese safety text, and found no old English phrases. | The GUI script had two harness issues before passing; recorded in `error-ledger.md`. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no packaged installer/updater. |

Open issues:

- GUI-verify the post-uninstall residue review button and cancellation behavior.
- Localize remaining migration internal gate strings when they become user-visible.
- Continue toward real uninstall only after snapshot, explicit final confirmation, and post-uninstall rescan UI are designed.

## 2026-07-07 - Migration plan body localization

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Only migration presentation/checklist copy and tests changed; no migration execution handler, file mover, service/startup/registry mutation, or rollback-manifest execution path was added. | `CanRunMigration` remains false in presentation tests. |
| Data, API, and consistency | Pass | `Migration_plan_presentation_body_uses_plain_chinese_while_staying_preview_only` asserts localized title, summary, safety banner, destination, rollback, space, sections, and checklist copy. | Existing migration gate tests still cover the low-level operation descriptor path. |
| Code quality and maintainability | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Localized strings are still inline Unicode escapes; move to resources later. |
| Testing and verification | Pass | Focused migration body test passed 1/1; `ProductExperienceTests` passed 44/44; full suite passed 99/99. | Older English assertions were updated to the new public Chinese copy while preserving safety assertions. |
| Frontend, accessibility, and UX | Pass | Scoped GUIA launched `Css.App.exe`, scanned apps, opened migration preview, and found `迁移方案`, `只预览`, `不会移动文件`, `迁移前检查`, `回滚方案`, `迁移后观察`, and `生成回滚清单`; old migration-window phrases were absent in the scoped modal search. | The first root-wide GUIA search was a false positive and is recorded in `error-ledger.md`. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit and no packaged installer/updater. |

Open issues:

- Localize the uninstall safety-window body and preflight checklist.
- GUI-verify the post-uninstall residue review button and cancellation behavior.
- Add real snapshot evidence before any future migration execution request.

### 2026-07-07 - C-drive recommendation grouping gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `RecommendationListPresenter` only changes card presentation; executable cleanup cards keep the existing `OperationDescriptor`; grouped observe card has `CanExecute=false` and `Operation=null`. | No cleanup, uninstall, migration, service, startup, scheduled-task, or registry action was added. |
| Data, API, and consistency | Pass | `C_drive_recommendation_list_groups_repeated_observe_items_and_explains_quarantine` verifies grouped observe findings and preserved low-risk cleanup operation. `C_drive_recommendation_execute_button_starts_disabled_until_actionable_card_selected` verifies the action button starts disabled until an actionable card is selected. | Per-path evidence remains in underlying `Recommendation` objects. |
| Code quality and maintainability | Pass | New grouping logic is isolated in `src/Css.Core/Apps/RecommendationListPresentation.cs`; WPF only consumes the resulting view model. | Future grouping types can be added to the presenter. |
| Testing and verification | Pass | Focused grouping, wrapping, and disabled-button tests passed; `ProductExperienceTests` passed 56/56; full suite passed 113/113; solution build passed with 0 warnings and 0 errors. | First short GUI scan timed out before cards appeared; longer real scan completed. |
| Frontend, accessibility, and UX | Pass | GUI screenshots `.omx\qa-cdrive-grouped-recommendations-wrapped.png` and `.omx\qa-cdrive-grouped-button-disabled.png` show grouped card text wraps and the non-actionable state disables the execute button. | Real scan was read-only; no cleanup button was invoked. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or release package. |

Open issues:

- Continue keeping system-changing C-drive cleanup behind quarantine confirmation and the safety pipeline.

### 2026-07-07 - App drawer residue-review inline result gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `UninstallResidueDrawerReviewPresenter` only creates visible drawer text and carries the existing low-risk operation reference; still-installed case has `LowRiskOperation=null` and `CanMoveLowRiskToQuarantine=false`. | No official uninstaller, cleanup, service/startup, scheduled-task, registry, or file-move execution path was added. |
| Data, API, and consistency | Pass | `Residue_drawer_inline_status_blocks_cleanup_when_app_still_installed_and_hides_paths` verifies blocked still-installed state, hidden local paths, and no operation. | Existing residue grouping and quarantine planning remain in `UninstallResidueScanBuilder` / `UninstallResidueOperationPlanner`. |
| Code quality and maintainability | Pass | Drawer residue text moved from WPF private string concatenation into `src/Css.Core/Software/UninstallResidueDrawerReviewPresentation.cs`. | Code-behind still orchestrates the click flow; future work can move more drawer state out of `MainWindow.xaml.cs`. |
| Testing and verification | Pass | `UninstallResidueScanTests` passed 9/9; `ProductExperienceTests` passed 59/59; full suite passed 117/117; solution build passed with 0 warnings and 0 errors. | Includes regression tests for cached branch not refreshing away the inline result, uninstall section ordering, and no horizontal scrollbar. |
| Frontend, accessibility, and UX | Pass | GUI screenshot `.omx\qa-residue-review-inline-wrapped.png` shows `残留检查结果` directly under action buttons with wrapped text. UIA found result title, still-installed text, official-uninstall-first text, and no-file-move safety text. | The app scan was read-only; selected app was `火绒安全软件`. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- Continue toward the official uninstall execution gate UI only after snapshot, command trust, close-app confirmation, post-uninstall rescan, and rollback evidence are all represented.
- Consider adding a reusable GUI smoke script for app scan plus drawer action verification.
### 2026-07-07 - Shared uninstall next-step flow gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `UninstallWorkflowGuidePresenter` is presentation-only; `UninstallPlanPreviewViewModel.CanRunOfficialUninstaller=false`; `OfficialConfirmation.CanRunOfficialUninstaller=false`; preflight `CanRequestExecution=false`. | No uninstaller, cleanup, migration, service/startup, scheduled-task, registry, or file-move execution path was added. |
| Data, API, and consistency | Pass | `Uninstall_workflow_guide_is_shared_by_drawer_and_safety_window` verifies drawer lines and safety-window `WorkflowGuide` come from the same guide. | Existing detailed preflight and residue sections remain available. |
| Code quality and maintainability | Pass | Shared copy is isolated in `src/Css.Core/Apps/UninstallWorkflowGuidePresentation.cs`; WPF only binds `WorkflowGuide`. | Future uninstall copy should extend the presenter rather than duplicating text in code-behind. |
| Testing and verification | Pass | Focused shared-flow test passed; `ProductExperienceTests` passed 60/60; full suite passed 118/118; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Warn | `UninstallPlanWindow.xaml` compiles and binds a `下一步流程` section. Diagnostic UIA screenshot `.omx\qa-uninstall-click-debug.png` shows the app-drawer state before modal open. | Final real-click GUI modal verification was blocked by usage-limit approval rejection. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- Re-run a real-click GUI smoke for `DrawerUninstallButton` when approvals are available, and verify the modal shows `下一步流程`, official-uninstaller, close-app, final-confirmation, residue-review, quarantine, and high-risk explanation text.
### 2026-07-07 - C-drive cleanup selection preview gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `RecommendationSelectionPresenter` only returns text/button state; execution still requires `RecommendationCardViewModel.Operation`, `QuarantineOperationPolicy`, confirmation, and `SafetyOperationPipeline`. | No cleanup, uninstall, migration, service/startup, scheduled-task, registry, or file-move execution path was added. |
| Data, API, and consistency | Pass | `C_drive_recommendation_selection_preview_explains_confirmation_quarantine_and_restore` verifies actionable/non-actionable/no-selection states. | The presenter uses existing `OperationDescriptor.EstimatedImpactBytes`; operation contents are unchanged. |
| Code quality and maintainability | Warn | Selection copy moved into `src/Css.Core/Apps/RecommendationSelectionPresentation.cs` and WPF handler now consumes it. | A renamed unused legacy handler remains because deleting mojibake-heavy code safely was deferred. |
| Testing and verification | Pass | Focused selection tests passed 2/2; `ProductExperienceTests` passed 62/62; full suite passed 120/120; solution build passed with 0 warnings and 0 errors. | No GUI run due earlier usage-limit rejection. |
| Frontend, accessibility, and UX | Warn | Selection text now explains second confirmation, quarantine, non-permanent delete, undo center restore, and estimated release. | Needs real C-drive GUI visual pass when approvals are available. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- Remove the legacy selection handler during a safer UTF-8/code-behind cleanup pass.
- GUI-verify selected actionable/non-actionable C-drive cards when usage limits allow app launch.

### 2026-07-08 - Agent next-step panel gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AgentNextStepPresenter` returns presentation text only and sets `CanExecuteDirectly=false`; blocked actions explicitly state no direct delete, migration, service disable, or registry edits. | No cloud AI or system-changing operation path was added. |
| Data, API, and consistency | Pass | `Agent_next_step_panel_turns_local_signals_into_safe_guidance` verifies C-drive cleanup, C-drive app count, resident app count, safe actions, blocked actions, and local-summary privacy line. | Uses existing `HealthCheckSummary` and `SoftwareProfile` data. |
| Code quality and maintainability | Pass | Agent advice lives in `src/Css.Core/Agent/AgentNextStepPresentation.cs`; WPF only binds the view model through `LoadAgentNextSteps()`. | Future Agent advice should extend this presenter or a sibling presenter, not hardcode long copy in event handlers. |
| Testing and verification | Pass | Focused Agent tests passed 2/2; `ProductExperienceTests` passed 64/64; full suite passed 122/122; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Warn | XAML contains named next-step panel controls and wrapping list templates. | No GUI screenshot was run for this slice; static XAML/build coverage only. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- GUI-verify the Agent page after a real C-drive scan and app scan when approvals/usage limits allow WPF launch.
- Continue moving Agent-facing product language into core presenters.

### 2026-07-08 - Agent safe navigation actions gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AgentNextActionViewModel.IsNavigationOnly=true`; `AgentNextAction_Click` only accepts known internal page ids and calls `ShowPage(targetPage)`. | No cleanup, uninstall, migration, service/startup, scheduled-task, registry, installer, or file-move execution path was added. |
| Data, API, and consistency | Pass | `Agent_next_step_panel_exposes_navigation_only_actions` verifies allowed target pages, C-drive route, app-management route, and empty-state routes. | Actions use existing `HealthCheckSummary` and `SoftwareProfile` signals. |
| Code quality and maintainability | Pass | Structured actions are produced in `src/Css.Core/Agent/AgentNextStepPresentation.cs`; WPF binds `panel.NavigationActions` through `AgentNextStepActionButtonsItemsControl`. | Future Agent actions should keep navigation and execution models separate. |
| Testing and verification | Pass | Focused Agent tests passed 3/3; `ProductExperienceTests` passed 65/65; full suite passed 123/123; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Warn | XAML now renders navigation buttons with labels/tooltips from the core model. | No GUI screenshot was run; duplicate legacy mojibake identity copy remains in the Agent card until a focused XAML cleanup pass. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- GUI-verify Agent navigation buttons after a real scan/app scan when approvals allow WPF launch.
- Replace the Agent left-card XAML region during a dedicated UTF-8 cleanup pass to remove duplicate legacy mojibake copy safely.

### 2026-07-08 - Agent left-card XAML cleanup gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | XAML-only duplicate text removal; no handlers, scanners, safety pipeline, operation descriptors, or execution gates changed. | No system-changing behavior was added. |
| Data, API, and consistency | Pass | `Agent_left_card_has_single_clean_identity_copy` checks the Agent left-card slice for clean identity text and no duplicate legacy copy. | Existing Agent next-step controls remain present. |
| Code quality and maintainability | Pass | Cleanup is localized to `src/Css.App/MainWindow.xaml`; the regression test protects the cleaned card. | The broader XAML still contains older localized strings elsewhere. |
| Testing and verification | Pass | Focused cleanup test passed 1/1; focused Agent tests passed 4/4; `ProductExperienceTests` passed 66/66; full suite passed 124/124; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Warn | Duplicate Agent identity copy is removed from the static XAML slice. | No GUI screenshot/click-through was run for this visual cleanup. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- GUI-verify the Agent page card layout after a real app launch when approval/usage limits allow.

### 2026-07-08 - App drawer cache/startup preview panels gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AppCacheCleanupPreviewPresenter` and `AppStartupControlPreviewPresenter` set `CanExecuteDirectly=false`; WPF handlers only update preview panels/status text. | No delete, quarantine movement, registry, service, startup, scheduled-task, migration, uninstall, or installer execution path was added. |
| Data, API, and consistency | Pass | `AppDrawerViewModel` now carries cache/startup preview summaries and lines; startup action enables for startup entries, services, or scheduled tasks. | Execution remains future gated operation-plan work. |
| Code quality and maintainability | Pass | Preview copy is isolated in `src/Css.Core/Apps/AppCacheCleanupPreview.cs` and `src/Css.Core/Apps/AppStartupControlPreview.cs`; WPF consumes view-model fields. | MainWindow code-behind still orchestrates drawer clicks. |
| Testing and verification | Pass | Focused cache tests passed 2/2; focused startup tests passed 2/2; `ProductExperienceTests` passed 70/70; full suite passed 128/128; solution build passed with 0 warnings and 0 errors. | TDD red was observed before implementation for missing public fields. |
| Frontend, accessibility, and UX | Warn | Static XAML tests verify named collapsed panels and click handlers for both buttons. | No GUI screenshot/click-through was run for the new panels. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- GUI-verify app drawer cache/startup preview panels after a real app scan when WPF launch approval/usage allows.
- Continue moving drawer action states from code-behind into a view-model presenter.

### 2026-07-08 - AppData cache candidates and drawer GUI smoke gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `SoftwareInventoryBuilder` only records candidate paths and bounded size estimates; drawer handlers still set `CanExecuteDirectly=false`. | No cleanup, delete, quarantine move, registry edit, service/startup/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added. |
| Data, API, and consistency | Pass | `Profile_builder_infers_appdata_cache_candidates_for_drawer_preview` verifies AppData data/cache/log path attribution and C-drive write evidence. | Exact folder-name matching is conservative and may miss vendor-nested folders until rules are expanded. |
| Code quality and maintainability | Pass | AppData inference is isolated in `SoftwareInventoryBuilder`; real roots are supplied by `SoftwareInventoryScanner.GetUserDataRoots()`. | Future attribution rules may deserve a separate rule file. |
| Testing and verification | Pass | Focused cache-candidate tests passed 2/2; `SoftwareInventoryTests` passed 11/11; `ProductExperienceTests` passed 71/71; full suite passed 130/130; solution build passed with 0 warnings and 0 errors. | TDD red was observed for missing builder and scanner integration. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-app-drawer-preview-smoke.ps1` passed with `cachePreviewVisible=True` and `startupPreviewVisible=True`; screenshot `.omx/qa-app-drawer-action-previews.png`. | The final screenshot shows the startup preview because it was the last clicked panel; the script also verified cache preview before that. |
| Operations, dependencies, and release | Warn | Added a repeatable `.omx` QA script; no runtime dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- Expand cache attribution rules for vendor-nested apps such as browser profiles and Electron app subdirectories.
- Continue moving drawer action preview orchestration out of code-behind.

### 2026-07-08 - Nested browser/Electron cache attribution gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `SoftwareInventoryBuilder` only records candidate data/cache/log paths and bounded cache sizes; app drawer preview models still set `CanExecuteDirectly=false`. | No cleanup, delete, quarantine move, registry edit, service/startup/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added. |
| Data, API, and consistency | Pass | `Profile_builder_infers_browser_profile_cache_candidates` and `Profile_builder_infers_electron_user_data_cache_candidates` verify nested `Vendor\\App`, `User Data`, profile cache paths, C-drive evidence, and cache sizes. | Candidate roots are exact and existence-gated. |
| Code quality and maintainability | Pass | Nested attribution helpers stay inside `src/Css.Scanner/Software/SoftwareInventoryBuilder.cs`; duplicate cache paths are only sized once. | Future broad AppData enumeration should be a separate tested rule. |
| Testing and verification | Pass | Focused nested tests passed 2/2; `SoftwareInventoryTests` passed 13/13; `ProductExperienceTests` passed 71/71; full suite passed 132/132; solution build passed with 0 warnings and 0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-app-drawer-preview-smoke.ps1` passed with `cachePreviewVisible=True` and `startupPreviewVisible=True`; screenshot `.omx/qa-app-drawer-action-previews.png`. | Smoke verifies preview visibility, not real cleanup. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- Add safe support for unusual browser profile folder names only after a bounded enumeration design exists.
- Continue extracting app-drawer action orchestration from `MainWindow.xaml.cs`.

### 2026-07-08 - App drawer action preview state presenter gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AppDrawerActionPreviewPresenter` only returns UI state and safety status text; `CanExecuteDirectly=false` for current cache/startup previews. | No cleanup, startup disabling, registry edit, service/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added. |
| Data, API, and consistency | Pass | `App_drawer_action_preview_presenter_switches_panels_without_execution` verifies cache/startup panel switching, copied summaries/lines, non-executability, and safety status text. | Uses existing `AppDrawerViewModel`. |
| Code quality and maintainability | Pass | `MainWindow.xaml.cs` now delegates cache/startup preview click state to `AppDrawerActionPreviewPresenter` and applies it through `ApplyDrawerActionPreviewState`. | More drawer action state can move to core presenters later. |
| Testing and verification | Pass | Focused presenter test passed 1/1; `ProductExperienceTests` passed 72/72; full suite passed 133/133; solution build passed with 0 warnings and 0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-app-drawer-preview-smoke.ps1` passed with `cachePreviewVisible=True` and `startupPreviewVisible=True`; screenshot `.omx/qa-app-drawer-action-previews.png`. | Smoke confirms real WPF click path still shows both preview panels. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- Continue extracting other drawer action state from `MainWindow.xaml.cs`, especially no-selection statuses and technical-detail toggling.
- Keep real cleanup/startup execution disabled until evidence, snapshot/rollback or quarantine, confirmation, and operation pipeline gates are represented.

### 2026-07-08 - App drawer no-selection preview states gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | No-selection states only hide preview panels and return guidance text; `CanExecuteDirectly=false`. | No cleanup, startup disabling, registry edit, service/scheduled-task mutation, uninstall, migration, installer, or cloud AI path was added. |
| Data, API, and consistency | Pass | `App_drawer_action_preview_presenter_handles_no_selection` verifies hidden panels, empty lines, non-executability, and cache/startup-specific "choose an app first" messages. | Uses the same `AppDrawerActionPreviewState` as selected-app previews. |
| Code quality and maintainability | Pass | `PreviewCacheCleanup_Click` and `PreviewStartupControl_Click` now use presenter no-selection states instead of hardcoded status text. | Technical details and other drawer actions still have code-behind state. |
| Testing and verification | Pass | Focused presenter tests passed 2/2; `ProductExperienceTests` passed 73/73; full suite passed 134/134; solution build passed with 0 warnings and 0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Warn | The selected-app drawer GUI smoke passed in the preceding slice. | No separate GUI smoke for the no-selection branch. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- Continue extracting technical-detail, uninstall, and migration drawer states from `MainWindow.xaml.cs`.

### 2026-07-08 - App drawer technical details toggle gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AppDrawerTechnicalDetailsPresenter` only toggles visibility/button/status text. | Technical details remain hidden by default and no system-changing path was added. |
| Data, API, and consistency | Pass | `App_drawer_technical_details_toggle_is_tested_and_changes_button_text` verifies show/hide states, button text, status text, and WPF presenter wiring. | Uses existing technical detail data from `AppDrawerViewModel`. |
| Code quality and maintainability | Pass | `ToggleTechnicalDetails_Click` delegates to `AppDrawerTechnicalDetailsPresenter` and `ApplyDrawerTechnicalDetailsState`. | XAML button remains unnamed because the localized line is fragile; handler uses sender. |
| Testing and verification | Pass | Focused technical-details toggle test passed 1/1; `ProductExperienceTests` passed 74/74; full suite passed 135/135; solution build passed with 0 warnings and 0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Warn | Button text now changes conceptually through handler state. | No GUI smoke was run for clicking technical details. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- Consider a future GUI smoke for app-drawer technical details and Agent navigation.

### 2026-07-08 - Shared app drawer action preview host gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AppDrawerActionHostPresenter` returns UI state only; all host states set `CanExecuteDirectly=false`. | No cleanup, startup disabling, official uninstaller execution, migration execution, registry edit, service/scheduled-task mutation, installer, or cloud AI path was added. |
| Data, API, and consistency | Pass | `App_drawer_shared_action_preview_host_replaces_stacked_action_sections` verifies uninstall, migration, cache, startup host wiring and non-executability. | Uses existing `AppDrawerViewModel` content and keeps modal safety plans. |
| Code quality and maintainability | Pass | `PreviewUninstall_Click`, `PreviewMigration_Click`, `PreviewCacheCleanup_Click`, `PreviewStartupControl_Click`, and `ShowResidueReviewInline` write to `DrawerActionPreviewPanel` through `ApplyDrawerActionHost`. | Old collapsed compatibility controls remain in XAML and can be removed in a later cleanup pass. |
| Testing and verification | Pass | Focused shared-host test passed 1/1; `ProductExperienceTests` passed 75/75; full suite passed 136/136; solution build passed with 0 warnings and 0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Warn | Static XAML/code tests verify a shared `DrawerActionPreviewPanel` and no default code writes to old uninstall/migration preview lists. | GUI smoke was attempted but rejected by usage-limit approval; no visual screenshot for this slice. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- Run `.omx/gui-app-drawer-preview-smoke.ps1` when approvals/usage allow to verify the shared host visually.
- Remove old collapsed drawer action preview controls during a dedicated clean-XAML pass.

### 2026-07-08 - Uninstall/migration no-selection host states gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | No-selection host states are collapsed and `CanExecuteDirectly=false`. | No system-changing behavior was added. |
| Data, API, and consistency | Pass | `App_drawer_action_host_handles_uninstall_and_migration_no_selection` verifies uninstall and migration no-selection messages use the shared host model; `App_drawer_action_host_no_selection_wiring_matches_each_button` verifies each handler calls the matching no-selection method. | Keeps all drawer actions on one presentation path. |
| Code quality and maintainability | Pass | `PreviewUninstall_Click`, `PreviewMigration_Click`, `PreviewCacheCleanup_Click`, and `PreviewStartupControl_Click` now route no-selection branches through matching host presenter methods. | A static regression test protects against repeated-handler patch mixups. |
| Testing and verification | Pass | Focused no-selection host/wiring tests passed 2/2; `ProductExperienceTests` passed 77/77; full suite passed 138/138; solution build passed with 0 warnings and 0 errors. | TDD red was observed before implementation and before the wiring fix. |
| Frontend, accessibility, and UX | Warn | Behavior is covered by core/static tests. | No separate GUI smoke for no-selection branches. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

Open issues:

- Remove the older overwritten uninstall no-selection status assignment during a safe code-behind cleanup pass.

### 2026-07-08 - App drawer legacy preview cleanup gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Removed only legacy collapsed controls and code-behind status/control writes; all drawer action output still goes through `AppDrawerActionHostPresenter` with `CanExecuteDirectly=false`. | No cleanup, startup disabling, official uninstall, migration, registry, service/scheduled-task, installer, file move, or cloud AI path was added. |
| Data, API, and consistency | Pass | `App_drawer_uses_only_one_shared_action_preview_host` verifies one shared host and absence of legacy preview controls in XAML/code. | Residue review and technical details controls remain. |
| Code quality and maintainability | Pass | `ShowAppDrawer`, `ClearAppDrawer`, and `ApplyDrawerActionHost` no longer reference legacy drawer preview controls; uninstall no-selection status comes from presenter. | Keeps action-state ownership in the core presenter. |
| Testing and verification | Pass | Focused shared-host cleanup tests passed 5/5; `ProductExperienceTests` passed 78/78; full suite passed 139/139; solution build passed with 0 warnings and 0 errors. | TDD red was observed for existing legacy controls and direct status assignment. |
| Frontend, accessibility, and UX | Warn | Static XAML tests verify one shared action host and wrapped text. | No GUI screenshot/click-through for this cleanup because WPF GUI smoke was previously blocked by usage-limit approval. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - C-drive legacy selection handler cleanup gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Active handler still delegates to `RecommendationSelectionPresenter`; execution still occurs only through existing recommendation button and safety pipeline. | No cleanup/quarantine behavior changed. |
| Data, API, and consistency | Pass | `C_drive_recommendation_selection_handler_uses_selection_presenter` verifies no legacy handler remains in the handler slice. | XAML still binds `SelectionChanged="RecommendationsListBox_SelectionChanged"`. |
| Code quality and maintainability | Pass | Removed `RecommendationsListBox_SelectionChangedLegacy` from `MainWindow.xaml.cs`. | Reduces chance of accidentally re-binding old copy. |
| Testing and verification | Pass | Focused C-drive selection tests passed 3/3; `ProductExperienceTests` passed 78/78; full suite passed 139/139; solution build passed with 0 warnings and 0 errors. | TDD red was observed before deletion. |
| Frontend, accessibility, and UX | N/A | Code-behind cleanup only. | No visible UI change intended. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - Agent skill capability cards gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AgentSkillCardPresenter` labels process/service and session-control skills as high-risk plan-only; system tools are open-only. | No direct system setting, process, service, shutdown, restart, registry, installer, cleanup, or cloud AI path was added. |
| Data, API, and consistency | Pass | `Agent_skill_cards_show_next_step_and_safety_mode_for_beginner_users` verifies next-step labels and safety hints for service, system tool, and session-control capabilities. | Uses the existing `AgentSkillCatalog` categories. |
| Code quality and maintainability | Pass | Skill UI language now lives in `src/Css.Core/Agent/AgentSkillCardPresentation.cs`; WPF consumes `AgentSkillCardPresenter.CreateDefault()`. | Some old private label helpers remain in `MainWindow.xaml.cs` but are no longer used. |
| Testing and verification | Pass | Focused skill-card test passed 1/1; focused Agent tests passed 4/4; `ProductExperienceTests` passed 79/79; full suite passed 140/140; solution build passed with 0 warnings and 0 errors. | TDD red was observed for missing presenter. |
| Frontend, accessibility, and UX | Warn | XAML now binds `NextStepLabel` and `SafetyHint`. | No GUI screenshot for the Agent skill card list in this slice. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - Agent system tool shortcuts gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `SystemToolShortcutCatalog` contains fixed commands only and tests assert no `cmd`/`powershell` wrappers; `OpenSystemTool_Click` uses `FindById` and blocks unknown ids. | Product code can open allowlisted Windows tools after explicit user action, but does not click inside them or mutate settings. |
| Data, API, and consistency | Pass | `Agent_system_tool_shortcuts_are_allowlisted_open_only_and_confirm_risky_tools` verifies ids, commands, open-only mode, risk, and confirmation requirements. | Registry Editor is high-risk and confirmation-gated. |
| Code quality and maintainability | Pass | Shortcut data is isolated in `src/Css.Core/Agent/SystemToolShortcuts.cs`; WPF maps through `SystemToolShortcutView`. | Future tools should be added to the catalog with tests. |
| Testing and verification | Pass | Focused shortcut tests passed 2/2; focused Agent tests passed 5/5; `ProductExperienceTests` passed 81/81; full suite passed 142/142; solution build passed with 0 warnings and 0 errors. | TDD red was observed before implementation. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentSystemToolListFound=true`, `visibleOpenButtonCount=4`; screenshot `.omx/qa-agent-system-tools.png`. | Smoke does not click tool-open buttons, by design. |
| Operations, dependencies, and release | Warn | Added a `.omx` GUI smoke script; no runtime dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - Windows settings confirmation gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `WindowsSettingsShortcutCatalog` is still a fixed `ms-settings:` allowlist; `OpenWindowsSettings_Click` rejects non-`ms-settings:` links and checks `shortcut.RequiresConfirmation` before medium-risk launches. | No setting toggles, uninstall, cleanup, registry edit, service/startup/scheduled-task mutation, installer, file move, or cloud AI path was added. |
| Data, API, and consistency | Pass | `Agent_windows_settings_shortcuts_are_ms_settings_allowlisted_and_open_only` verifies low-risk entries do not require confirmation and medium-risk entries do. | `RequiresConfirmation` is now part of the settings shortcut model. |
| Code quality and maintainability | Pass | The confirmation policy lives in `WindowsSettingsShortcutCatalog`; WPF only looks up catalog ids and applies the model. | Future settings links should declare risk and confirmation in the catalog. |
| Testing and verification | Pass | TDD red observed for missing `RequiresConfirmation`; focused settings tests passed 2/2; `ProductExperienceTests` passed 83/83; full suite passed 144/144; solution build passed with 0 warnings and 0 errors. | The focused red was a compile failure caused by the absent property, which is the expected missing behavior. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentWindowsSettingsListFound=true` and `visibleSettingsOpenButtonCount=3`; screenshot `.omx/qa-agent-system-and-settings.png`. | Smoke does not click settings-open buttons, by design, so it does not open Windows Settings. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - Agent background priority gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AgentNextStepPresenter` still returns `CanExecuteDirectly=false`; navigation actions remain `IsNavigationOnly=true` and target internal pages only. | No process termination, service disable, startup/scheduled-task mutation, cleanup, uninstall, migration, registry edit, installer, file move, or cloud AI path was added. |
| Data, API, and consistency | Pass | `Agent_next_step_prioritizes_many_resident_apps_before_c_drive_apps` verifies resident apps outrank C-drive app advice only after the threshold and without losing C-drive secondary advice. | Uses existing resident evidence from software profiles. |
| Code quality and maintainability | Pass | Priority is captured in `ShouldPrioritizeResidentApps(...)` and `ResidentPriorityThreshold`, not scattered through WPF code. | Future threshold changes are localized to `AgentNextStepPresenter`. |
| Testing and verification | Pass | TDD red observed for old C-drive priority; focused priority test passed 1/1; focused Agent next-step tests passed 4/4; `ProductExperienceTests` passed 84/84; full suite passed 145/145; solution build passed with 0 warnings and 0 errors. | No WPF GUI smoke was run for this presenter-only change. |
| Frontend, accessibility, and UX | Warn | Agent panel text/order is covered by presenter tests and existing WPF bindings. | No real app-scan GUI screenshot proves a local machine triggers the new threshold yet. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - Agent background review panel gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AgentBackgroundReviewPresenter` items set `CanExecuteDirectly=false`; WPF only displays summary/list/safety text. | No process termination, service disable, startup/scheduled-task mutation, cleanup, uninstall, migration, registry edit, installer, file move, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `Agent_background_review_summarizes_resident_apps_without_technical_dump_or_execution` verifies resident app summaries, hidden technical identifiers, risk labels, recommended next steps, and non-executability. | Uses existing `SoftwareProfile` resident evidence. |
| Code quality and maintainability | Pass | New presentation logic is isolated in `src/Css.Core/Agent/AgentBackgroundReviewPresentation.cs`; `MainWindow.xaml.cs` only applies the view model in `LoadAgentNextSteps()`. | Future action-plan generation should build on this presenter rather than WPF string logic. |
| Testing and verification | Pass | TDD red observed for missing presenter; focused background tests passed 2/2; `ProductExperienceTests` passed 86/86; full suite passed 147/147; solution build passed with 0 warnings and 0 errors. | Static test also protects first-screen order by requiring the panel before `AgentNextStepReasonsListBox`. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-agent-background-review-smoke.ps1` passed after a real app scan with `backgroundSummaryFound=true` and `backgroundReviewItemCount=3`; screenshot `.omx/qa-agent-background-review.png` shows the panel in the first visible Agent card area. | The smoke does not click any disable/close/uninstall/settings buttons. |
| Operations, dependencies, and release | Warn | Added a `.omx` GUI smoke script; no runtime dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - Agent startup/service plan preview gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AgentStartupServicePlanPresenter` sets `CanExecuteDirectly=false`, `RequiresSnapshot=true`, and lists blocked actions for service disabling, startup/task mutation, and process termination. | No startup disabling, service/scheduled-task mutation, process termination, cleanup, uninstall, migration, registry edit, installer, file move, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `Agent_startup_service_plan_preview_is_auditable_and_non_executable` verifies title, summary, evidence counts, required snapshot/rollback, blocked actions, and hidden raw service/task names. | Uses existing `SoftwareProfile` resident evidence. |
| Code quality and maintainability | Pass | Plan presentation logic lives in `src/Css.Core/Agent/AgentStartupServicePlanPresentation.cs`; WPF only applies the view model in `LoadAgentNextSteps()`. | Future executable plans must build on this model and still pass through `OperationPipeline`. |
| Testing and verification | Pass | TDD red observed for missing AutomationIds and wrong first-screen order; focused plan/binding tests passed 3/3; full suite passed 148/148; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Current evidence is fresh after the layout move. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-agent-background-review-smoke.ps1` passed with `startupServicePlanFound=true`, `startupServicePlanStepCount=3`; screenshot `.omx/qa-agent-startup-service-plan.png` shows the plan before the detailed app list. | The smoke performs read-only app scanning and does not click any risky system action. |
| Operations, dependencies, and release | Warn | Updated a `.omx` GUI smoke script and added a project UX rule to `AGENTS.md`; no runtime dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - Windows Settings confirmation cancel gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` clicked Storage, captured the confirmation dialog, canceled it, and reported `newSettingsProcessCount=0`. | No Windows Settings page was opened in this smoke; no settings toggle, cleanup, uninstall, registry edit, service/startup/scheduled-task mutation, installer, file move, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `Agent_windows_settings_shortcuts_are_ms_settings_allowlisted_and_open_only` verifies Settings entries remain fixed `ms-settings:` links and medium-risk entries require confirmation. | Storage/Installed Apps/Power are now ordered first because they match the product's C-drive/software-management jobs. |
| Code quality and maintainability | Pass | Dynamic setting-button AutomationIds are bound in XAML; `WindowsSettingsShortcutCatalog` still owns ids, URIs, risk, and confirmation policy. | GUI script uses the same visible WPF entry point rather than calling handlers directly. |
| Testing and verification | Pass | TDD red observed for missing button AutomationIds, non-scrollable capability column, old setting order, and Settings below system tools; focused settings tests passed 2/2; full suite passed 148/148; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | `dotnet build` and `dotnet test` were rerun sequentially after one parallel file-lock failure. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-agent-system-tools-smoke.ps1` found both settings and system tool lists after the right-card reorder; screenshot `.omx/qa-agent-system-and-settings.png` shows Windows Settings first. | Right card now has `AgentCapabilityScrollViewer` to avoid hidden capability sections. |
| Operations, dependencies, and release | Warn | Added `.omx/gui-agent-settings-confirm-cancel-smoke.ps1`; no dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - App drawer shared action host GUI gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-app-drawer-preview-smoke.ps1` verified four preview buttons and closed two plan windows; `AppDrawerActionHostPresenter` states remain `CanExecuteDirectly=false`. | No cleanup, startup disabling, official uninstaller execution, migration execution, rollback manifest creation, registry edit, service/scheduled-task mutation, installer, file move, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `App_drawer_action_controls_have_stable_automation_ids_for_gui_smoke` verifies stable AutomationIds on the drawer action buttons and exposed preview title/summary/list controls. | The smoke selects eligible real scanned apps for each conditional action instead of overriding disabled states. |
| Code quality and maintainability | Pass | `.omx/gui-app-drawer-preview-smoke.ps1` now uses `Find-ControlByAutomationId`, a four-action matrix, and modal-window close handling. | Future drawer actions should plug into the same shared host and smoke pattern. |
| Testing and verification | Pass | TDD red observed for missing drawer AutomationIds; focused AutomationId test passed 1/1; `ProductExperienceTests` passed 88/88; full suite passed 149/149; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors. | Commands were run sequentially to avoid shared output locks. |
| Frontend, accessibility, and UX | Pass | GUI smoke passed with `verifiedActionButtons=4`, `closedDialogCount=2`, screenshot `.omx/qa-app-drawer-action-previews.png`. | Screenshot shows the app grid and drawer with concise conclusion/action UI; the final visible preview is startup-control because it is the last action in the smoke matrix. |
| Operations, dependencies, and release | Warn | Updated a `.omx` GUI smoke script and project protocol docs; no runtime dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - App drawer Agent action card gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AppDrawerActionHostViewModel` additions are presentation fields only; focused tests assert drawer action states remain non-executable. | No cleanup, startup disabling, official uninstaller execution, migration execution, rollback manifest creation, registry edit, service/scheduled-task mutation, installer, file move, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `App_drawer_action_host_presents_agent_takeaway_next_step_and_safety_text` verifies uninstall, migration, cache, and startup states expose Agent takeaway, next step, and safety text. | Inline post-uninstall residue review also populates the new fields. |
| Code quality and maintainability | Pass | `AppDrawerActionHostPresenter` owns the shared action-card copy; WPF `ApplyDrawerActionHost` only copies fields to named controls. | Build caught and fixed one direct initializer outside the presenter. |
| Testing and verification | Pass | Focused action-card/scroll tests passed 3/3; enhanced app drawer GUI smoke passed; `ProductExperienceTests` passed 91/91; full suite passed 152/152; solution build passed with 0 warnings and 0 errors. | TDD red was observed for missing fields and missing scroll/bring-into-view behavior. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-app-drawer-preview-smoke.ps1` now verifies `DrawerActionPreviewAgentTextBlock`, `DrawerActionPreviewNextStepTextBlock`, and `DrawerActionPreviewSafetyTextBlock`; screenshot `.omx/qa-app-drawer-action-previews.png` shows the action card scrolled into view. | The detail list remains below the concise fields for users who want more context. |
| Operations, dependencies, and release | Warn | Updated WPF layout and GUI smoke only; no dependency or packaging change. | Repository still has no initial commit or packaged installer. |
### 2026-07-08 - Selected resident app plan details gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AppStartupControlPreviewPresenter` and `AppDrawerActionHostPresenter` only change presentation copy; all startup states remain `CanExecuteDirectly=false`. | No startup disabling, service/scheduled-task mutation, process termination, cleanup, uninstall, migration, registry edit, installer, file move, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | New tests classify selected resident apps as `建议保留`, `先观察`, or `未来可禁用候选` while hiding raw service/task/process names. | Uses existing `SoftwareProfile` evidence and drawer model. |
| Code quality and maintainability | Pass | Decision logic is isolated in `AppStartupControlPreviewPresenter`; the drawer action host derives concise Agent copy from the startup summary. | No WPF binding changes were required. |
| Testing and verification | Pass | TDD red observed for all three new tests; focused new tests passed 3/3; surrounding drawer/startup tests passed 4/4; `ProductExperienceTests` passed 94/94; full suite passed 155/155; solution build passed with 0 warnings/errors. | The old action-host test caught and fixed a missing `自启动` keyword in the new future-disable takeaway. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-app-drawer-preview-smoke.ps1` passed with `verifiedActionButtons=4`, `closedDialogCount=2`; screenshot `.omx/qa-app-drawer-action-previews.png` shows the selected-app action card. | The smoke does not execute cleanup, uninstall, migration, or startup changes. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-08 - Undo center visual proof hooks gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | UI hooks and smoke script are verification-only; the smoke does not seed user data, restore files, delete files, or move items to quarantine. | Real quarantine/restore behavior remains gated by existing pipeline and core tests. |
| Data, API, and consistency | Pass | `Undo_center_has_stable_visual_proof_hooks_for_timeline_quarantine_and_restore` verifies Timeline title/load/description/policy/list/restore controls and code paths. | TimelinePage XAML was rewritten with XML character references after historical mojibake corruption. |
| Code quality and maintainability | Pass | Stable AutomationIds are on UIAutomation-visible controls: TextBlocks, Button, and ListBox. | Aligns with AGENTS WPF GUI smoke rule. |
| Testing and verification | Pass | TDD red observed for missing AutomationIds; focused undo hook test passed 1/1; `ProductExperienceTests` passed 95/95; full suite passed 156/156; solution build passed with 0 warnings/errors. | Commands were run sequentially. |
| Frontend, accessibility, and UX | Warn | `.omx/gui-undo-center-smoke.ps1` was added but not executed. | Escalated GUI run was rejected by the environment usage limit; no workaround was attempted. |
| Operations, dependencies, and release | Warn | Added a `.omx` GUI smoke script; no runtime dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-09 - Isolated app storage roots for GUI smokes gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-undo-center-smoke.ps1` sets `OMNIX_ENTROPY_DATA_ROOT` and `OMNIX_ENTROPY_QUARANTINE_ROOT` to `.omx` temp roots, then restores previous values and removes both roots in `finally`. | The smoke does not touch the user's real LocalAppData timeline or D-drive quarantine folder. |
| Data, API, and consistency | Pass | `App_storage_paths_can_be_isolated_for_gui_smokes_without_touching_user_data` and `App_storage_paths_keep_existing_defaults_when_no_override_is_set` verify both override and default paths. | `MainWindow` now uses `AppStoragePathResolver` for database, rollback, and quarantine roots. |
| Code quality and maintainability | Pass | Storage path policy lives in `src/Css.Core/AppIdentity.cs`; WPF no longer duplicates local AppData/D-drive fallback logic. | Future path changes are centralized and testable. |
| Testing and verification | Pass | TDD red observed for missing resolver and missing smoke env vars; focused path/script tests passed 3/3; `ProductExperienceTests` passed 96/96; full suite passed 159/159; solution build passed with 0 warnings/errors. | Commands were run sequentially. |
| Frontend, accessibility, and UX | Pass | Isolated `.omx/gui-undo-center-smoke.ps1` passed with timeline/policy/list/restore controls found; screenshot `.omx/qa-undo-center.png`; cleanup checks for `.omx/qa-undo-center-data` and `.omx/qa-undo-center-quarantine` returned `False`. | Smoke verifies empty-state restore affordance; next slice can seed a restorable row in the isolated roots. |
| Operations, dependencies, and release | Warn | Added env-var override surface but no packaging/installer documentation yet. | Repository still has no initial commit or packaged installer. |

### 2026-07-09 - Seeded undo-center restorable GUI proof gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-undo-center-smoke.ps1` uses `OMNIX_ENTROPY_DATA_ROOT` and `OMNIX_ENTROPY_QUARANTINE_ROOT`, seeds only under `.omx`, does not contain `Invoke-Element $restoreButton`, and cleanup checks returned `False` for both temp roots. | The smoke does not touch the user's real LocalAppData timeline or D-drive quarantine folder and does not click restore. |
| Data, API, and consistency | Pass | `Css.SmokeTools seed-undo-center` reuses `AppStoragePathResolver`, `FileQuarantineService`, `ActionTimelineStore`, and `SafetyOperationPipeline`; GUI output reported one restorable manifest and `restoreButtonEnabled=true`. | Avoids duplicated SQLite/manifest schemas in PowerShell. |
| Code quality and maintainability | Pass | Dev smoke behavior lives in `src/Css.SmokeTools`; first-level timeline presentation remains in `src/Css.Core/Timeline/ActionTimelinePresentation.cs`. | Future smoke seeders should extend the tool rather than adding hidden WPF switches. |
| Testing and verification | Pass | TDD red observed for missing seeded smoke behavior and raw-path timeline detail. Focused undo tests passed 3/3; focused timeline tests passed 2/2; `ProductExperienceTests` passed 97/97; full suite passed 161/161; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors. | Initial non-escalated restore failed due sandbox network and then passed with approved escalation. |
| Frontend, accessibility, and UX | Pass | `.omx/gui-undo-center-smoke.ps1` passed with `restoreButtonEnabled=true`; screenshot `.omx/qa-undo-center.png` shows an enabled `还原` button and `影响范围：1 个位置` instead of a long raw path. | This is still a proof page; a future technical-detail affordance can expose paths on demand. |
| Operations, dependencies, and release | Warn | Added `src/Css.SmokeTools` to the solution and performed `dotnet restore` once with escalation to create assets. | Dev/test tool is not a packaged user feature; packaging docs still need to exclude or classify it appropriately. |

### 2026-07-09 - Shared WPF smoke helper foundation gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Only `.omx` tooling changed; seeded undo smoke still sets isolated env roots, avoids `Invoke-Element $restoreButton`, and removes temp roots in `finally`. | No product behavior or system-changing path was added. |
| Data, API, and consistency | Pass | `.omx/wpf-smoke-helpers.ps1` centralizes UIAutomation assembly initialization and common helper functions; undo smoke still owns only undo-specific seeding. | Other smoke scripts are not migrated yet. |
| Code quality and maintainability | Pass | `Undo_center_gui_smoke_uses_shared_wpf_smoke_helpers` verifies dot-sourcing and helper function names. | Future smokes should reuse this helper rather than copying functions. |
| Testing and verification | Pass | TDD red observed for missing helper; focused shared-helper test passed 1/1; focused undo tests passed 4/4; full suite passed 162/162; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors. | Commands were run sequentially. |
| Frontend, accessibility, and UX | Pass | Seeded `.omx/gui-undo-center-smoke.ps1` passed after helper extraction with `restoreButtonEnabled=true`; temp root cleanup checks returned `False`. | Screenshot remains `.omx/qa-undo-center.png`. |
| Operations, dependencies, and release | Warn | Added a shared `.omx` helper but no packaging change. | Need documentation that `.omx` helper scripts are dev/test tooling. |

### 2026-07-09 - App drawer smoke helper migration gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-app-drawer-preview-smoke.ps1` still only clicks preview buttons and closes preview plan windows; GUI output reported `closedDialogCount=2`. | No cleanup, uninstall, migration, startup/service/task mutation, registry edit, installer, settings, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | Script now dot-sources `.omx/wpf-smoke-helpers.ps1` and uses shared `Find-ByAutomationId`, `Invoke-Element`, and `Save-DesktopScreenshot`. | App-drawer-only selection/dialog helpers remain local. |
| Code quality and maintainability | Pass | `App_drawer_gui_smoke_uses_shared_wpf_smoke_helpers` verifies helper usage and no local `Add-Type -AssemblyName UIAutomationClient`. | Future smoke migrations can follow this pattern. |
| Testing and verification | Pass | TDD red observed for missing helper usage; focused app-drawer tests passed 4/4; full suite passed 164/164; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors. | `ProductExperienceTests` now has 100 tests. |
| Frontend, accessibility, and UX | Pass | Real `.omx/gui-app-drawer-preview-smoke.ps1` passed with `verifiedActionButtons=4`, screenshot `.omx/qa-app-drawer-action-previews.png`; screenshot shows the app grid and selected action preview card. | A browser window behind the app is visible in the desktop screenshot but does not affect the WPF smoke result. |
| Operations, dependencies, and release | Warn | `.omx` helper migration only; no packaged release change. | Remaining GUI smokes still need migration. |

### 2026-07-09 - GUI smoke development docs gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `docs/development/gui-smokes.md` says the storage overrides are for development and GUI smoke tests only and must restore previous environment values. | Prevents accidentally presenting storage redirection as a beginner-facing feature. |
| Data, API, and consistency | Pass | The docs name `OMNIX_ENTROPY_DATA_ROOT`, `OMNIX_ENTROPY_QUARANTINE_ROOT`, `.omx/wpf-smoke-helpers.ps1`, and `Css.SmokeTools seed-undo-center`. | Matches current tooling names. |
| Code quality and maintainability | Pass | `Development_docs_describe_storage_overrides_as_test_only` protects the required documentation phrases. | Docs now live under `docs/development`. |
| Testing and verification | Pass | TDD red observed for missing docs; focused docs test passed 1/1; `ProductExperienceTests` passed 100/100; full suite passed 164/164; solution build passed with 0 warnings/errors. | Documentation-only change after GUI smoke verification. |
| Frontend, accessibility, and UX | N/A | No UI changed. | Documentation only. |
| Operations, dependencies, and release | Warn | Docs clarify dev/test tooling, but no release/package exclusion manifest exists yet. | Future packaging work should ensure `Css.SmokeTools` and `.omx` scripts are not bundled as normal user features unless intentionally classified. |

### 2026-07-09 - Agent system-tools smoke helper migration gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-agent-system-tools-smoke.ps1` only navigates to AI Agent, finds `AgentSystemToolListBox` and `AgentWindowsSettingsListBox`, counts buttons, and screenshots. | It does not invoke any system-tool or Windows Settings open button. |
| Data, API, and consistency | Pass | Script now dot-sources `.omx/wpf-smoke-helpers.ps1` and uses shared `Initialize-WpfSmokeAutomation`, `Find-ByAutomationId`, `Wait-Until`, `Invoke-Element`, and `Save-WindowScreenshot`. | Agent-specific list/button assertions remain local. |
| Code quality and maintainability | Pass | `Agent_system_tools_gui_smoke_uses_shared_wpf_smoke_helpers` verifies helper usage, no local `Add-Type -AssemblyName UIAutomationClient`, and no local `Find-ByAutomationId`/`Invoke-Element` functions. | Matches the app-drawer and undo-center helper pattern. |
| Testing and verification | Pass | TDD red observed for missing helper usage; focused test passed 1/1; `ProductExperienceTests` passed 101/101; full suite passed 165/165; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors. | Commands were run sequentially. |
| Frontend, accessibility, and UX | Pass | Real `.omx/gui-agent-system-tools-smoke.ps1` passed with `agentSystemToolListFound=true`, `visibleOpenButtonCount=3`, `agentWindowsSettingsListFound=true`, `visibleSettingsOpenButtonCount=3`; screenshot `.omx/qa-agent-system-and-settings.png` reviewed. | Screenshot shows the AI Agent page with Windows Settings and system-tools sections visible. |
| Operations, dependencies, and release | Warn | Tooling-only migration; no packaging boundary change. | Remaining Agent smokes still need helper migration and `Css.SmokeTools`/`.omx` release classification still needs future work. |

### 2026-07-09 - Agent settings confirm-cancel smoke helper migration gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` clicked Storage, captured OMNIX-Entropy's confirmation dialog, canceled it, and reported `newSettingsProcessCount=0`. | No Windows Settings page was opened; no setting, cleanup, uninstall, migration, registry, service/startup/task, session, installer, or cloud AI action was added. |
| Data, API, and consistency | Pass | Script dot-sources `.omx/wpf-smoke-helpers.ps1` and uses shared WPF primitives; native dialog fallback uses process id to find only windows owned by the launched app. | Settings-specific click/cancel/process checks remain local. |
| Code quality and maintainability | Pass | Static tests verify helper usage, protected root-window search, and `EnumWindows`/`GetWindowThreadProcessId` fallback. | Avoids duplicating UIAutomation boilerplate while making secondary-window discovery more robust. |
| Testing and verification | Pass | TDD red observed for missing helper usage and missing native fallback; focused settings smoke tests passed 3/3; `ProductExperienceTests` passed 104/104; full suite passed 168/168; solution build passed with 0 warnings/errors. | First real GUI run exposed `RPC_E_SERVERFAULT`; second exposed dialog-not-found; both were fixed before final verification. |
| Frontend, accessibility, and UX | Pass | Real `.omx/gui-agent-settings-confirm-cancel-smoke.ps1` passed with `confirmationDialogFound=true`, `cancelClicked=true`, `newSettingsProcessCount=0`; screenshot `.omx/qa-agent-settings-confirm-cancel.png` reviewed. | Screenshot shows the confirmation overlay, proving the user-facing safety prompt still appears. |
| Operations, dependencies, and release | Warn | Tooling-only migration; no packaging boundary change. | Remaining `gui-agent-background-review-smoke.ps1` still needs helper migration. |

### 2026-07-09 - Agent background-review smoke helper migration gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-agent-background-review-smoke.ps1` performs a read-only app scan and only verifies Agent text/list controls. | No startup disable, process/service stop, task/registry edit, uninstall, migration, settings launch, installer, session, or cloud AI path was added. |
| Data, API, and consistency | Pass | Script dot-sources `.omx/wpf-smoke-helpers.ps1` and uses shared WPF primitives; app-scan and Agent-background assertions remain local. | Matches the existing helper boundary used by app-drawer/system/settings smokes. |
| Code quality and maintainability | Pass | `Agent_background_review_gui_smoke_uses_shared_wpf_smoke_helpers` verifies helper usage and no local `Find-ByAutomationId`, `Wait-Until`, `Invoke-Element`, or `Save-WindowScreenshot` definitions. | The shared helper now covers undo, app drawer, Agent system tools, settings confirm-cancel, and background-review smokes. |
| Testing and verification | Pass | TDD red observed for missing helper usage; focused helper test passed 1/1; `ProductExperienceTests` passed 105/105; full suite passed 169/169; solution build passed with 0 warnings/errors. | Commands were run sequentially after the real GUI smoke. |
| Frontend, accessibility, and UX | Pass | Real `.omx/gui-agent-background-review-smoke.ps1` passed with `appTileCount=120`, `backgroundReviewItemCount=3`, and `startupServicePlanStepCount=3`; screenshot `.omx/qa-agent-startup-service-plan.png` reviewed. | Screenshot shows the Agent next-step area recommends reviewing background resident apps and keeps the plan preview visible. |
| Operations, dependencies, and release | Warn | Tooling-only migration; no packaging boundary change. | Next product work can use the now-stabilized GUI smoke base. |

### 2026-07-09 - Undo center collapsed technical details gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `ActionTimelinePresenter` only changes presentation data; `.omx/gui-undo-center-smoke.ps1` still does not invoke `TimelineRestoreButton`. | No cleanup, restore click, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `Timeline_presentation_keeps_raw_paths_in_collapsed_technical_details` verifies first-level details hide raw paths while collapsed `TechnicalDetails` retain affected paths, manifest paths, and restore operation. | Preserves auditability without making the beginner row path-heavy. |
| Code quality and maintainability | Pass | `ActionTimelineItemViewModel` owns `TechnicalDetailsButtonText` and `TechnicalDetails`; `BuildTechnicalDetails(...)` centralizes the technical rows. | Keeps WPF binding simple and avoids ad hoc string parsing in XAML. |
| Testing and verification | Pass | TDD red observed for missing properties/hooks; focused timeline/product tests passed 3/3; `ProductExperienceTests` passed 105/105; full suite passed 170/170; solution build passed with 0 warnings/errors. | Commands were run sequentially after the GUI smoke evidence from this slice. |
| Frontend, accessibility, and UX | Pass | `TimelineTechnicalDetailsExpander` and `TimelineTechnicalDetailsListBox` are stable AutomationIds; seeded undo GUI smoke output included `technicalDetailsExpanderFound=true`. | First-level timeline row remains concise; technical details are second-level by user choice. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Release packaging still needs explicit classification for `.omx` scripts and `Css.SmokeTools`. |

### 2026-07-09 - Low-risk C-drive cleanup selection preview gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `RecommendationSelectionViewModel.CanExecuteDirectly=false`; execution still requires `QuarantineOperationPolicy`, second confirmation, `SafetyOperationPipeline`, and `QuarantineOperationHandler`. | No direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `C_drive_low_risk_cleanup_selection_preview_is_structured_and_quarantine_first` verifies quarantine-first copy, estimated impact, affected-count, Undo Center restore, and no raw path in the preview text. | Raw paths remain reserved for the second confirmation/technical evidence layer. |
| Code quality and maintainability | Pass | `RecommendationSelectionPresenter` owns selection-preview copy; `ApplyRecommendationSelection(...)` centralizes WPF field updates. | Avoids scattering button/text/list assignments across scan and selection flows. |
| Testing and verification | Pass | TDD red observed for missing fields and missing hooks; focused tests passed 2/2; surrounding C-drive tests passed 8/8; `ProductExperienceTests` passed 107/107; full suite passed 172/172; solution build passed with 0 warnings/errors. | Commands were run sequentially. |
| Frontend, accessibility, and UX | Warn | Static product test verifies `RecommendationActionTakeawayTextBlock`, `RecommendationActionNextStepTextBlock`, `RecommendationActionSafetyTextBlock`, and `RecommendationActionPlanListBox` AutomationIds. | No fresh real GUI screenshot was captured because there is not yet a dedicated C-drive preview smoke fixture; add one when a stable low-risk scan fixture exists. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-09 - Low-risk cleanup confirmation copy gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `ExecuteSelectedRecommendationAsync` still validates with `QuarantineOperationPolicy`, waits for `MessageBoxResult.OK`, then uses `SafetyOperationPipeline` and `QuarantineOperationHandler`. | No direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `C_drive_cleanup_confirmation_puts_plain_summary_before_technical_paths` verifies beginner text contains Agent judgment, estimated impact, affected-count, Undo Center restore, and no raw path; technical details retain paths and quarantine root. | Preserves audit evidence before execution while reducing first-read complexity. |
| Code quality and maintainability | Pass | `CleanupConfirmationPresenter` centralizes confirmation copy; WPF handler no longer builds the path-first message inline. | A future custom confirmation window can reuse the presenter model. |
| Testing and verification | Pass | TDD red observed for missing presenter; focused confirmation tests passed 2/2; surrounding C-drive tests passed 9/9; `ProductExperienceTests` passed 109/109; full suite passed 174/174; solution build passed with 0 warnings/errors. | One narrow mechanical rewrite was used after `apply_patch` could not match mojibake-heavy string context; method inspection and build verified the result. |
| Frontend, accessibility, and UX | Warn | Confirmation is still a `MessageBox`, but its body now starts with plain summary and moves paths below `technical details`. | A richer custom dialog with collapsible details would be better than MessageBox for V1 polish. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Repository still has no initial commit or packaged installer. |

### 2026-07-09 - Custom cleanup confirmation dialog gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Handler still uses `QuarantineOperationPolicy`, requires `ShowDialog() == true`, then runs `SafetyOperationPipeline` and `QuarantineOperationHandler`. | No direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `CleanupConfirmationWindow` binds the existing `CleanupConfirmationViewModel`; technical details remain available through the same `TechnicalDetails` collection. | No new execution data model was introduced. |
| Code quality and maintainability | Pass | Window code-behind only sets `DialogResult`; copy remains in `CleanupConfirmationPresenter`. | Keeps presentation copy testable outside WPF. |
| Testing and verification | Pass | TDD red observed for missing window and handler usage; focused tests passed 2/2; C-drive tests passed 10/10; `ProductExperienceTests` passed 110/110; full suite passed 175/175; solution build passed with 0 warnings/errors. | Commands were run sequentially. |
| Frontend, accessibility, and UX | Warn | Static tests verify `CleanupConfirmationSummaryTextBlock`, `CleanupConfirmationTechnicalDetailsExpander`, `CleanupConfirmationTechnicalDetailsListBox`, `CleanupConfirmationConfirmButton`, and `CleanupConfirmationCancelButton`; technical details default to collapsed. | No real GUI screenshot yet because a stable C-drive cleanup fixture/smoke is still missing. |
| Operations, dependencies, and release | Warn | Added a WPF window; no packaging/installer change. | Need future packaging/release review. |

### 2026-07-09 - C-drive cleanup fixture smoke gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1` sets isolated `OMNIX_ENTROPY_DATA_ROOT`, `OMNIX_ENTROPY_QUARANTINE_ROOT`, and `OMNIX_ENTROPY_CDRIVE_SCAN_ROOT`, opens the cleanup confirmation, and clicks only `CleanupConfirmationCancelButton`. Static test asserts the script does not reference `CleanupConfirmationConfirmButton` or `Invoke-Element $confirm`. | No direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `AppDevelopmentPathResolver.ResolveCDriveScanRoot` defaults to the normal drive root unless the process-scoped env var is present. `RunScanAsync` uses the override only for scan/snapshot roots. | Normal user UI remains automatic C-drive scanning; the override is documented as dev/test-only. |
| Code quality and maintainability | Pass | C-drive smoke reuses `.omx/wpf-smoke-helpers.ps1`; explicit AutomationIds were added for C-drive nav, scan, recommendation list, and execute button. | A future helper could centralize secondary-window discovery used by multiple smokes. |
| Testing and verification | Pass | TDD red observed for missing resolver, AutomationIds, smoke script, and top-level temp rules. Focused fixture/static tests passed 3/3; top-level temp rules test passed 1/1; `ProductExperienceTests` passed 112/112; full suite passed 179/179; solution build passed with 0 warnings/errors. | Commands used current workspace state after implementation. |
| Frontend, accessibility, and UX | Warn | Static tests verify the C-drive preview/confirmation smoke can find stable AutomationIds and cancel the confirmation window. | Real GUI smoke launch was rejected by the approval/usage-limit system, so no screenshot was captured in this slice. |
| Operations, dependencies, and release | Warn | Added a new process-scoped dev/test environment variable and script; `docs/development/gui-smokes.md` documents it as development and GUI smoke test tooling only. | Packaging still needs an explicit decision to exclude or classify `.omx` scripts and `Css.SmokeTools`. |

### 2026-07-09 - Cleanup confirmation outcome preview gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `CleanupConfirmationPresenter` only adds presentation copy; `CleanupConfirmationWindow` still only returns `DialogResult`, and execution remains in `ExecuteSelectedRecommendationAsync` behind `QuarantineOperationPolicy`, explicit confirm, `SafetyOperationPipeline`, and `QuarantineOperationHandler`. | No direct delete, automatic cleanup, high-risk cleanup, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `CleanupConfirmationViewModel.OutcomePreviewLines` is generated from the same operation/quarantine context as the existing beginner summary and technical details. | Outcome copy does not include raw affected paths; those remain in collapsed technical details. |
| Code quality and maintainability | Pass | `C_drive_cleanup_confirmation_puts_plain_summary_before_technical_paths` verifies outcome preview content and path hiding; XAML static test verifies `CleanupConfirmationOutcomeListBox` binds `OutcomePreviewLines` before technical details. | Keeps copy testable in core presenter and WPF binding simple. |
| Testing and verification | Pass | TDD red observed for missing `OutcomePreviewLines`; focused tests passed 2/2. TDD red observed for smoke script missing `CleanupConfirmationOutcomeListBox`; focused smoke static test passed 1/1. `ProductExperienceTests` passed 112/112; full suite passed 179/179; solution build passed with 0 warnings/errors. | Commands used current workspace state. |
| Frontend, accessibility, and UX | Warn | `CleanupConfirmationOutcomeHeaderTextBlock` and `CleanupConfirmationOutcomeListBox` have stable AutomationIds; `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1` now requires the outcome list before screenshot/cancel. | Real GUI smoke was not rerun in this slice due prior GUI approval/usage-limit rejection, so no fresh screenshot. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Packaging still needs explicit classification for `.omx` scripts and `Css.SmokeTools`. |

### 2026-07-09 - Uninstall residue custom confirmation gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `ReviewSelectedUninstallResidueAsync` now opens `CleanupConfirmationWindow` only after `QuarantineOperationPolicy.ValidateCandidate(lowRiskOperation)` succeeds; execution still uses `SafetyOperationPipeline` and `QuarantineOperationHandler`. | No official uninstaller execution, automatic residue cleanup, medium/high-risk residue handling, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | The residue flow reuses the existing `CleanupConfirmationPresenter.Create(lowRiskOperation, DefaultQuarantineRoot())` model, so quarantine outcome preview and collapsed technical details are shared with C-drive cleanup. | No new operation descriptor type or handler was introduced. |
| Code quality and maintainability | Pass | Removed the unused path-first `BuildResidueConfirmMessage` / `FormatPathList` helpers; new test asserts the handler no longer references the old builder. | Keeps confirmation copy centralized in `CleanupConfirmationPresenter`. |
| Testing and verification | Pass | TDD red observed for missing custom confirmation window in the residue handler; focused test passed 1/1 after implementation. Residue-focused tests passed 10/10; `ProductExperienceTests` passed 115/115; full suite passed 182/182; solution build passed with 0 warnings/errors. | Commands used current workspace state. |
| Frontend, accessibility, and UX | Warn | The flow now uses the already-tested `CleanupConfirmationWindow` with outcome preview and collapsed technical details. | No dedicated real GUI smoke was added for the residue-confirmation path in this slice. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Packaging still needs explicit classification for `.omx` scripts and `Css.SmokeTools`. |

### 2026-07-09 - Uninstall plan window readability and hooks gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `UninstallPlanWindow.xaml` change only adds UI hooks and list controls; `UninstallPlanPresentationBuilder.CanRunOfficialUninstaller` remains false and no handler was added to execute uninstall or residue cleanup. | No official uninstaller execution, residue cleanup, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | Existing `UninstallPlanPreviewViewModel` bindings remain the source of title, summary, workflow guide, official confirmation, sections, and final reminder. | No new data model or execution state was introduced. |
| Code quality and maintainability | Pass | `Uninstall_plan_window_has_readable_text_and_stable_hooks` verifies required AutomationIds, workflow/sections bindings, close click, and no known mojibake fragments. | Static hook coverage prepares a later GUI smoke without duplicating presentation logic. |
| Testing and verification | Pass | TDD red observed for missing window hooks; focused test passed 1/1 after XAML update. `ProductExperienceTests` passed 113/113; full suite passed 180/180; solution build passed with 0 warnings/errors. | Commands used current workspace state. |
| Frontend, accessibility, and UX | Warn | Title, summary, plan-only safety text, official confirmation, workflow, preflight, sections, final reminder, and close controls now have stable AutomationIds; key collections are `ListBox` targets. | No fresh GUI screenshot was captured because this slice did not launch the app/window. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Packaging still needs explicit classification for `.omx` scripts and `Css.SmokeTools`. |

### 2026-07-09 - Uninstall plan window GUI smoke script gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-uninstall-plan-window-smoke.ps1` verifies the plan window and clicks only `UninstallPlanCloseButton`; static test asserts it does not contain `UninstallPlanConfirmButton`, `Start-Process -FilePath $uninstaller`, or `Invoke-Element $run`. | No official uninstaller execution, residue cleanup, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | The smoke selects any scanned app with an enabled `DrawerUninstallButton` instead of hard-coding a local app name. | Still depends on the real machine having at least one uninstallable app when run. |
| Code quality and maintainability | Pass | Script dot-sources `.omx/wpf-smoke-helpers.ps1` and keeps app-selection/secondary-window logic local. | Mirrors existing app-drawer and cleanup-confirmation smoke patterns. |
| Testing and verification | Pass | TDD red observed for missing smoke script; focused static test passed 1/1 after script addition. `ProductExperienceTests` passed 114/114; full suite passed 181/181; solution build passed with 0 warnings/errors. | Commands used current workspace state. |
| Frontend, accessibility, and UX | Warn | Script checks `UninstallPlanTitleTextBlock`, `UninstallPlanSummaryTextBlock`, `UninstallPlanSafetyTextBlock`, `UninstallPlanWorkflowListBox`, `UninstallPlanOfficialConfirmationTextBlock`, `UninstallPlanSectionsListBox`, `UninstallPlanFinalReminderTextBlock`, and `UninstallPlanCloseButton`. | Real GUI smoke was not run in this slice; screenshot `.omx\qa-uninstall-plan-window.png` is pending. |
| Operations, dependencies, and release | Warn | Added a new `.omx` smoke script. | Packaging still needs explicit classification for `.omx` scripts and `Css.SmokeTools`. |

### 2026-07-09 - Uninstall plan window real GUI smoke gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Real `.omx/gui-uninstall-plan-window-smoke.ps1` clicked only the app drawer uninstall-plan button and `UninstallPlanCloseButton`; output was `planWindowFound=true`, `closedPlanWindow=true`. | No official uninstaller execution, residue cleanup, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path occurred. |
| Data, API, and consistency | Pass | The smoke selected an app with an enabled `DrawerUninstallButton`; screenshot showed `115生活 卸载安全方案` with official uninstaller and residue summary from the scanned profile. | Uses real machine inventory, so the specific app may differ on another machine. |
| Code quality and maintainability | Pass | TDD red required `Find-WindowByDescendantAutomationId`; script now scopes modal assertions by walking from `UninstallPlanTitleTextBlock` to its owner window. | This local pattern should move to `.omx/wpf-smoke-helpers.ps1` if reused again. |
| Testing and verification | Pass | First real run failed with `Uninstall plan window was not found`; focused static test then passed 1/1 after descendant lookup. Final real GUI smoke passed; `ProductExperienceTests` passed 114/114; full suite passed 181/181; solution build passed with 0 warnings/errors. | Commands used current workspace state after the fix. |
| Frontend, accessibility, and UX | Pass | Screenshot `.omx\qa-uninstall-plan-window.png` was visually inspected: readable plan-only copy, official uninstaller path, post-uninstall residue summary, workflow steps, and a single `知道了` close button are visible. | The lower parts of the modal are scrollable; first view still clearly communicates "only preview". |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Packaging still needs explicit classification for `.omx` scripts and `Css.SmokeTools`. |

### 2026-07-09 - C-drive cleanup confirmation real GUI smoke gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-cdrive-cleanup-confirmation-smoke.ps1` clicked `CleanupConfirmationCancelButton`; output included `cancelClicked=true`, `fixtureStillExists=true`, and `quarantineItemCount=0`. | No cleanup confirmation, file movement, permanent delete, registry/service/startup/task mutation, installer execution, settings change, session control, or cloud AI path occurred. |
| Data, API, and consistency | Pass | The smoke used isolated `OMNIX_ENTROPY_DATA_ROOT`, `OMNIX_ENTROPY_QUARANTINE_ROOT`, and `OMNIX_ENTROPY_CDRIVE_SCAN_ROOT` roots. | Normal app runs still use the automatic system drive unless the dev-only override is set. |
| Code quality and maintainability | Pass | TDD red required descendant modal lookup after the real smoke failed with `Cleanup confirmation window was not found`. | The eventual helper extraction is recorded in the next gate. |
| Testing and verification | Pass | Focused static test passed after the modal lookup fix; final real smoke passed with screenshot `.omx\qa-cdrive-cleanup-confirmation.png`; `ProductExperienceTests` passed 115/115; full suite passed 182/182; build passed with 0 warnings/errors. | Commands used current workspace state. |
| Frontend, accessibility, and UX | Pass | Visual inspection of `.omx\qa-cdrive-cleanup-confirmation.png` shows the confirmation window starts with beginner copy and the "what happens after confirm" outcome preview before technical details. | The screenshot is captured before cancel by design. |
| Operations, dependencies, and release | Warn | No dependency or packaging change. | Packaging still needs explicit classification for `.omx` scripts and `Css.SmokeTools`. |

### 2026-07-09 - Shared WPF modal discovery helper gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Refactor only moved modal-discovery functions into `.omx/wpf-smoke-helpers.ps1`; C-drive smoke remains cancel-only and uninstall-plan smoke remains close-only. | No production execution path changed. |
| Data, API, and consistency | Pass | Both scripts call `Find-SecondaryWindowWithChild $process.Id ...`; shared helper owns `Find-WindowByDescendantAutomationId` and `Find-SecondaryWindowWithChild`. | `rg -F` confirmed function definitions exist only in the helper and call sites remain in both scripts. |
| Code quality and maintainability | Pass | TDD red required shared helper extraction and no duplicate function definitions in individual scripts; focused tests passed 2/2 after extraction. | Reduces duplicated WPF modal-discovery behavior. |
| Testing and verification | Pass | Real C-drive cleanup confirmation smoke passed; real uninstall-plan smoke passed; `ProductExperienceTests` passed 115/115; full suite passed 182/182; build passed with 0 warnings/errors; no `Css.App` or `Css.SmokeTools` process remained. | Commands used current workspace state. |
| Frontend, accessibility, and UX | Pass | Both modal smokes still verify stable AutomationIds before screenshot/close or screenshot/cancel. | No UI layout change in this refactor. |
| Operations, dependencies, and release | Warn | `.omx/wpf-smoke-helpers.ps1` gained shared helper functions. | Packaging still needs explicit classification for `.omx` scripts and `Css.SmokeTools`. |

### 2026-07-09 - Residue confirmation GUI fixture gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-uninstall-residue-confirmation-smoke.ps1` uses isolated `.omx` data/quarantine/residue roots and `OMNIX_ENTROPY_SOFTWARE_FIXTURE`; static test asserts the script verifies `CleanupConfirmationCancelButton` and does not invoke `CleanupConfirmationConfirmButton`. | No official uninstaller execution, confirmation click, residue movement, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `SoftwareInventoryFixtureScanner` returns scripted scan sequences and repeats the final scan; `ReviewSelectedUninstallResidueAsync` refreshes software profiles before building the residue report. | Normal app behavior still uses real `SoftwareInventoryScanner` unless the process-scoped env var is set. |
| Code quality and maintainability | Pass | `ScanSoftwareProfilesAsync()` centralizes fixture-vs-real scanner selection; docs describe `OMNIX_ENTROPY_SOFTWARE_FIXTURE` as development and GUI smoke tests only. | Avoids hidden WPF demo mode and keeps the fixture outside user-facing settings. |
| Testing and verification | Pass | Focused residue rescan test passed 1/1; fixture tests passed 3/3; residue GUI smoke static test passed 1/1; combined focused tests passed 5/5; fresh `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore` passed 186/186; fresh `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors. | Commands used current workspace state after record updates. |
| Frontend, accessibility, and UX | Warn | XAML now exposes `AppsNavButton`, `ScanSoftwareButton`, `AppTilesListBox`, and `DrawerResidueReviewButton`; the smoke asserts the cleanup confirmation outcome controls before cancel. | Real GUI launch was rejected by the approval/usage-limit system, so no `.omx\qa-uninstall-residue-confirmation.png` screenshot is available yet. |
| Operations, dependencies, and release | Warn | Added one dev-only scanner fixture and one `.omx` smoke script. | Packaging still needs explicit classification for `.omx` scripts, fixture env vars, and `Css.SmokeTools`. |

### 2026-07-09 - Residue cancel/quarantine inline outcome gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `CreateCanceled` / `CreateQuarantined` only build non-executable drawer view models; `ReviewSelectedUninstallResidueAsync` still executes quarantine only after `CleanupConfirmationWindow.ShowDialog() == true`, `QuarantineOperationPolicy`, and `SafetyOperationPipeline`. | No official uninstaller execution, confirmation bypass, automatic cleanup, high-risk residue handling, permanent delete, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | Outcome models hide local paths by default and keep `CanMoveLowRiskToQuarantine=false` / `LowRiskOperation=null`. | Detailed paths remain in operation/timeline/confirmation evidence rather than first-level drawer text. |
| Code quality and maintainability | Pass | Outcome copy lives in `UninstallResidueDrawerReviewPresenter`; WPF handler calls `ShowResidueOutcomeInline(...)` rather than assembling result text inline in multiple branches. | Future destructive-adjacent flows can reuse this presenter-first pattern. |
| Testing and verification | Pass | TDD red observed for missing outcome presenter methods; focused new tests passed 2/2; `UninstallResidueScanTests|ProductExperienceTests` passed 127/127; full suite passed 188/188; solution build passed with 0 warnings/errors. | Commands used current workspace state after implementation and record updates. |
| Frontend, accessibility, and UX | Warn | Handler now updates the app drawer action host after cancel or success. | Real GUI residue smoke remains blocked by approval/usage limit, so no visual screenshot of this exact outcome state yet. |
| Operations, dependencies, and release | N/A | Presentation-only code change; no dependencies or packaging changes. | Existing packaging warning for `.omx` scripts remains on the fixture slice. |

### 2026-07-09 - Residue outcome undo-center navigation gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `DrawerActionPreviewPrimary_Click` handles only `case "Timeline": ShowPage("Timeline");` and the static test asserts the handler does not call `RestoreSelectedTimelineEntryAsync`, `SafetyOperationPipeline`, or `QuarantineOperationHandler`. | No restore execution, cleanup execution, official uninstaller execution, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | Successful residue outcome sets `PrimaryActionText = "查看后悔药中心"` and `PrimaryActionKey = "Timeline"`; cancel outcome keeps both empty. | The action is optional and hidden by default for other drawer previews. |
| Code quality and maintainability | Pass | `AppDrawerActionHostViewModel` owns optional primary action fields; WPF binding is centralized in `ApplyDrawerActionHost`. | Future action keys need the same static safety checks. |
| Testing and verification | Pass | TDD red observed for missing action fields/button; focused new tests passed 2/2; `UninstallResidueScanTests|ProductExperienceTests` passed 128/128; full suite passed 189/189; solution build passed with 0 warnings/errors. | Commands used current workspace state after implementation and record updates. |
| Frontend, accessibility, and UX | Warn | XAML exposes `DrawerActionPreviewPrimaryButton` with stable AutomationId and default collapsed visibility. | Real GUI proof remains blocked by approval/usage limit. |
| Operations, dependencies, and release | N/A | No dependency, packaging, or fixture change. | Existing `.omx` packaging warning remains on smoke-tool slices. |

### 2026-07-09 - Residue cancel outcome GUI smoke assertion gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `.omx/gui-uninstall-residue-confirmation-smoke.ps1` clicks `CleanupConfirmationCancelButton`, asserts the primary outcome button is hidden after cancel, and still does not contain `CleanupConfirmationConfirmButton` or `Invoke-Element $confirm`. | No confirm click, residue movement, restore, cleanup execution, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path is added. |
| Data, API, and consistency | Pass | The smoke continues using isolated data/quarantine/residue roots plus `OMNIX_ENTROPY_SOFTWARE_FIXTURE`. | Cancel leaves fixture residue in place and quarantine count at zero. |
| Code quality and maintainability | Pass | Static product test now requires the cancel outcome controls and JSON fields in the smoke script. | Keeps future GUI run aligned with the new outcome panel behavior. |
| Testing and verification | Pass | TDD red observed for missing cancel-outcome smoke checks; focused static smoke test passed 1/1; focused action/outcome/smoke tests passed 3/3; `ProductExperienceTests` passed 118/118; full suite passed 189/189; solution build passed with 0 warnings/errors. | Commands used current workspace state after implementation and record updates. |
| Frontend, accessibility, and UX | Warn | Smoke now waits for `DrawerActionPreviewTitleTextBlock` after cancel and checks `DrawerActionPreviewPrimaryButton` is absent/offscreen. | Real GUI proof still waits on launch approval/usage. |
| Operations, dependencies, and release | Warn | `.omx/gui-uninstall-residue-confirmation-smoke.ps1` changed. | Packaging still needs explicit classification for `.omx` scripts and smoke fixtures. |

### 2026-07-09 - Residue cancel outcome screenshot gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | The script saves a second screenshot only after clicking cancel and after checking the primary action button is hidden. | No confirm click, restore, cleanup execution, registry/service/startup/task mutation, migration, installer execution, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | JSON output includes both `screenshot` for the confirmation dialog and `cancelOutcomeScreenshot` for the post-cancel panel. | Both screenshots are written under `.omx`. |
| Code quality and maintainability | Pass | Static product test requires `qa-uninstall-residue-cancel-outcome.png` and `cancelOutcomeScreenshot = $cancelOutcomeScreenshotPath`. | Keeps future GUI proof explicit. |
| Testing and verification | Pass | TDD red observed for missing second screenshot path; focused static smoke test passed 1/1; focused action/outcome/smoke tests passed 3/3; `ProductExperienceTests` passed 118/118; full suite passed 189/189; solution build passed with 0 warnings/errors. | Commands used current workspace state after implementation and record updates. |
| Frontend, accessibility, and UX | Warn | The future GUI run will capture the inline cancel outcome separately. | Real GUI proof still waits on launch approval/usage. |
| Operations, dependencies, and release | Warn | `.omx/gui-uninstall-residue-confirmation-smoke.ps1` changed. | Packaging still needs explicit classification for `.omx` scripts and screenshots. |

### 2026-07-09 - Install routing learning memory gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `InstallerAnalyzer.AnalyzePath(..., routingMemory)` still sets `WillRunInstaller=false` and `RequiresUserConfirmation=true`; `AnalyzeInstaller_Click` does not call `Start-Process` or `Process.Start`. | No installer execution, global ProgramFiles change, automatic install-argument passing, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `InstallRoutingMemory` prefers exact software rules, then category rules, then defaults; `InstallRoutingMemoryStore` persists JSON; `AppStoragePathResolver` exposes `install-routing-memory.json`. | Memory rules affect recommendations only. |
| Code quality and maintainability | Pass | Routing memory lives in `Css.InstallGuard\Routing`; WPF loads memory through `DefaultInstallRoutingMemoryPath()` rather than hard-coding another path. | Future UI can add a confirmation action to write this store. |
| Testing and verification | Pass | TDD red observed for missing memory classes/route fields/store, missing storage path, and missing WPF loading. `InstallerAnalyzerTests` passed 8/8; AppIdentity/WPF focused tests passed 3/3; install/AppIdentity focused tests passed 14/14; `ProductExperienceTests` passed 119/119; full suite passed 192/192; solution build passed with 0 warnings/errors. | Commands used current workspace state after implementation and record updates. |
| Frontend, accessibility, and UX | Warn | Install analysis output now includes path-source text for memory/default route source. | No GUI screenshot for install page yet. |
| Operations, dependencies, and release | Warn | Adds a new app-data JSON file name. | Need later UI for user-confirmed rule creation and docs for the memory file. |

### 2026-07-09 - Install route remember button gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `RememberInstallRoute_Click` only loads/saves `InstallRoutingMemoryStore`; static product test asserts the handler does not call `Start-Process`, `Process.Start`, or `SafetyOperationPipeline`. | No installer execution, global ProgramFiles change, automatic install-argument passing, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `memory.RememberRoute(_lastInstallerAnalysis.RecommendedRoute)` persists the same route produced by the read-only installer analysis. | Current behavior remembers the software route; category-vs-software choice is a future UX improvement. |
| Code quality and maintainability | Pass | The button has `AutomationProperties.AutomationId="InstallRememberRouteButton"` and `DefaultInstallRoutingMemoryPath()` centralizes the app-data file path. | Keeps GUI smoke/static tests anchored on stable control names. |
| Testing and verification | Pass | Focused install/app identity/product tests passed 16/16; `ProductExperienceTests` passed 120/120; full suite passed 194/194; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors. | Commands used current workspace state after context compaction. |
| Frontend, accessibility, and UX | Warn | The install page now has a visible "remember this route" button with a confirmation dialog. | No real GUI screenshot for this install-page path yet. |
| Operations, dependencies, and release | Warn | Reuses the new `install-routing-memory.json` app-data file. | Later docs should explain how learning rules can be reset or edited. |

### 2026-07-09 - Install route memory scope choice gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `RememberInstallRoute_Click` opens `InstallRouteMemoryChoiceWindow` and only calls `InstallRoutingMemoryStore.Save(...)` after `ShowDialog() == true` and `SelectedScope` is set; static product test asserts no `MessageBox.Show`, `Start-Process`, `Process.Start`, or installer analysis call inside the save handler. | No installer execution, global ProgramFiles change, automatic install-argument passing, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `RememberRouteForCategory(...)` uses the analyzed route root for category memory; focused test proves an AI route remembered from Ollama applies to another AI app as category memory. | Exact software memory still exists and remains higher priority than category memory. |
| Code quality and maintainability | Pass | `InstallRouteMemoryChoicePresenter` owns copy; `InstallRouteMemoryChoiceWindow` exposes stable AutomationIds for software, category, and cancel buttons. | Future GUI smoke can target the window reliably. |
| Testing and verification | Pass | TDD red observed for missing category memory and presenter; focused new tests passed 3/3; install-focused tests passed 18/18; `ProductExperienceTests` passed 120/120; full suite passed 196/196; build passed with 0 warnings/errors. | Commands used current workspace state. |
| Frontend, accessibility, and UX | Warn | The choice window explains software-only versus category memory and says no installer will run. | No real GUI screenshot for this new modal yet. |
| Operations, dependencies, and release | Warn | Adds one WPF window and extends the app-data memory semantics. | Later docs/settings should expose learned-rule reset/edit behavior. |

### 2026-07-09 - Learned install rules read-only view gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `LoadInstallRoutingMemoryRules()` calls `InstallRoutingMemoryStore.Load(...)` and `InstallRoutingMemoryPresenter.Create(...)`; static test asserts the loader does not call `InstallRoutingMemoryStore.Save`. | No learned-rule deletion/editing, installer execution, global ProgramFiles change, automatic install-argument passing, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `InstallRoutingMemoryPresenter` converts exact software rules and category rules into rows without JSON field names such as `SoftwareName` or `TargetRoot`. | Raw JSON remains on disk but is not shown in the beginner-facing page. |
| Code quality and maintainability | Pass | Presentation logic lives in `InstallRoutingMemoryPresenter`; WPF only binds `Summary` and `Rows`. | Future reset/edit can reuse the row model with a stable rule identity. |
| Testing and verification | Pass | TDD red observed for missing presenter and WPF loader; focused new tests passed 2/2; install-focused tests passed 20/20; `ProductExperienceTests` passed 121/121; full suite passed 198/198; build passed with 0 warnings/errors. | Commands used current workspace state. |
| Frontend, accessibility, and UX | Warn | XAML exposes `InstallRoutingMemorySummaryTextBlock` and `InstallRoutingMemoryListBox` with stable AutomationIds. | No real GUI screenshot for the install page learned-rules section yet. |
| Operations, dependencies, and release | Warn | Reuses `install-routing-memory.json`. | Later docs/settings should explain reset/edit semantics. |

### 2026-07-09 - Forget learned install rule gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `ForgetInstallRoutingRule_Click` confirms first, then calls `memory.ForgetRule(row.RuleKey)` and saves app memory; static test asserts no `Start-Process`, `Process.Start`, or installer analysis in the handler. | No installed app mutation, installer execution, global ProgramFiles change, file movement, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `ForgetRule(...)` removes a matching software/category rule by stable key while preserving unrelated rules. | Focused test proves removing the software rule leaves the category rule intact. |
| Code quality and maintainability | Pass | Row model owns `RuleKey` and `CanForget`; placeholder rows cannot trigger forget. | Future edit/reset flows can reuse the same stable key model. |
| Testing and verification | Pass | TDD red observed for missing forget key/model/handler; focused new tests passed 2/2; install-focused tests passed 22/22; `ProductExperienceTests` passed 122/122; full suite passed 200/200; build passed with 0 warnings/errors. | Commands used current workspace state. |
| Frontend, accessibility, and UX | Warn | XAML exposes `ForgetInstallRoutingRuleButton` with stable AutomationId and disabled default state. | No real GUI screenshot for the forget flow yet. |
| Operations, dependencies, and release | Warn | Edits `install-routing-memory.json`. | Later docs should explain that forgetting rules only affects future recommendations. |

### 2026-07-10 - Post-install change report cards gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `BuildInstallDiff_Click` only builds `InstallSnapshotDiffReport`, creates `InstallSnapshotDiffPresenter`, and binds view data. | No installer execution, snapshot data expansion, software inventory behavior change, registry/service/startup/task mutation, migration, cleanup, restore, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | `InstallSnapshotDiffPresenter.Create(report)` derives cards from existing report fields and keeps raw paths/services/tasks only in `TechnicalDetails`. | The diff model and scanner behavior were not changed. |
| Code quality and maintainability | Pass | Presentation logic lives in `Css.InstallGuard.Installers`; WPF binds through `ApplyInstallDiffPresentation(view)`. | Keeps the UI from formatting raw report details inline. |
| Testing and verification | Pass | TDD red observed for missing presenter and missing WPF controls. Focused install-diff tests passed 2/2; `ProductExperienceTests` passed 123/123; install-focused tests passed 21/21; full suite passed 202/202; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors. | Process check found no `Css.App`, `Css.SmokeTools`, or `OMNIX` process. |
| Frontend, accessibility, and UX | Warn | XAML exposes `InstallDiffSummaryTextBlock`, `InstallDiffCardsListBox`, and `InstallDiffTechnicalDetailsExpander` with stable AutomationIds; raw diff appears after the cards. | No real GUI screenshot for post-install report cards yet. |
| Operations, dependencies, and release | N/A | No dependency, packaging, fixture, or app-data file change. | Existing no-initial-commit repo state remains. |

### 2026-07-10 - Install report Agent explanation and GUI proof gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `InstallSnapshotDiffAgentPresenter` uses report counts only; visible-text tests reject raw C-drive paths and service names; `ExplainInstallDiff_Click` only creates and binds advice. | No installer, cleanup, migration, startup/service/task/registry mutation, routing-memory edit, restore, settings, session, or cloud AI action was added. |
| Data, API, and consistency | Pass | C-drive/background/no-pressure branches derive only from `InstallSnapshotDiffReport`; `CanExecuteDirectly=false`; new snapshot capture clears the previous report and hides stale advice. | Raw evidence remains in the collapsed technical details. |
| Code quality and maintainability | Pass | Presentation logic lives in `Css.InstallGuard.Installers`; WPF binding is centralized in `ApplyInstallDiffAgentAdvice`; GUI helpers are reused by `.omx/gui-install-diff-agent-smoke.ps1`. | No new runtime dependency. |
| Testing and verification | Pass | TDD red observed for presenter, WPF, smoke, and screenshot guards. Final focused tests 4/4, product tests 125/125, install tests 25/25, full suite 206/206, build 0 warnings/errors. | Commands used current workspace state after final GUI corrections. |
| Frontend, accessibility, and UX | Pass | `InstallPageScrollViewer` and stable AutomationIds are present. Real smoke returned 4 cards/4 Agent steps with technical details collapsed; `.omx/qa-install-diff-cards.png` and `.omx/qa-install-diff-agent.png` were visually inspected and show unclipped content. | First screenshot attempt was rejected because it did not visually prove the panel; the rerun fixed scrolling/capture state. |
| Operations, dependencies, and release | Warn | Adds a dev-only `.omx` GUI smoke and two PNG evidence files. Temporary data/fixture files were removed and no app process remained. | Packaging still needs an explicit rule excluding smoke scripts/screenshots from end-user artifacts. |

### 2026-07-10 - Install report action plan gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `InstallSnapshotDiffActionPlanPresenter` derives only counts and plain conclusions; visible-text tests reject raw paths/service names; every plan/item has `CanExecuteDirectly=false`. | No cleanup, migration, startup/service/task/registry change, installer execution, routing-memory edit, restore, settings, session, or cloud AI action was added. |
| Data, API, and consistency | Pass | C-drive/background/no-pressure branches derive from the existing report; a fresh report or snapshot collapses stale plan UI. | Raw evidence remains in collapsed technical details. |
| Code quality and maintainability | Pass | Plan logic is isolated in `Css.InstallGuard.Installers`; WPF binding is centralized in `ApplyInstallDiffActionPlan`; the existing fixture smoke was extended. | No runtime dependency added. |
| Testing and verification | Pass | TDD red observed for presenter, UI, smoke, and PowerShell-safe text assertion. Focused tests 4/4, product tests 127/127, install tests 29/29, full suite 210/210, build 0 warnings/errors. | Final commands used current workspace state. |
| Frontend, accessibility, and UX | Pass | Stable AutomationIds exist for generate button, summary, list, and safety text. GUI smoke returned three items and `nothingExecutedVisible=true`; `.omx/qa-install-diff-action-plan.png` was visually inspected with all decisions visible and no clipping. | The page is still information-dense above the plan; future work should avoid adding another always-visible block. |
| Operations, dependencies, and release | Warn | Smoke uses isolated env overrides, removes fixture/data state, stops the app, and retains one new PNG. | End-user packaging still needs an exclusion rule for `.omx` QA assets. |

### 2026-07-10 - Install report evidence classification gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Visible-text tests reject raw paths and service names; review models use numbered generic names and `CanExecuteDirectly=false`. | Classification uses existing report evidence only and sends nothing externally. |
| Data, API, and consistency | Pass | Presenter returns one review item per source finding and six/three deterministic category kinds; action plan consumes only the compact summary. | Rules are preliminary heuristics, not authoritative system facts. |
| Code quality and maintainability | Pass | Classification is centralized in `InstallSnapshotDiffEvidenceReviewPresenter`; C-drive segment matching avoids treating `AppData` as generic data. | Future rule expansion should stay table-driven if the category list grows. |
| Testing and verification | Pass | TDD red observed for missing types/property/UI. Focused tests 5/5, product tests 127/127, install tests 32/32, full suite 213/213, build 0 warnings/errors. | Current workspace evidence. |
| Frontend, accessibility, and UX | Pass | One stable-ID summary TextBlock appears before the plan list. Real GUI returned `classificationSummaryVisible=true`; clean rerun screenshot visibly shows the blue classification line without adding another default list. | First screenshot was rejected due transient black capture blocks. |
| Operations, dependencies, and release | N/A | No dependency, storage schema, installer, or packaging behavior changed. | Existing QA asset packaging warning remains. |

### 2026-07-10 - On-demand install evidence review gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Unit tests reject raw C-drive paths, app names, service names, and task names; GUI smoke returned `evidenceReviewHidesRawIdentifiers=true`; every review item and container has `CanExecuteDirectly=false`. | Raw identifiers remain in the existing collapsed technical-details expander only. |
| Data, API, and consistency | Pass | `InstallSnapshotDiffActionPlanViewModel.EvidenceReview` reuses the exact review that produces `ReviewSummary`; test asserts both summaries match. | No duplicate classifier or additional evidence scan was introduced. |
| Code quality and maintainability | Pass | WPF binding is centralized in `ApplyInstallDiffActionPlan`; fresh plans reset `InstallDiffEvidenceReviewExpander.IsExpanded=false`; stable AutomationIds cover expander, lists, and safety text. | No runtime dependency added. |
| Testing and verification | Pass | TDD red observed for missing model/UI/smoke/read-only styling. Full suite passed 215/215; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings/errors. | Commands used current workspace state after final styling changes. |
| Frontend, accessibility, and UX | Pass | GUI smoke returned default collapse, one C-drive row, three background rows, and collapsed technical details. Clean action-plan and evidence-review screenshots were visually inspected; read-only lists no longer show selection highlighting. | Expanded evidence is intentionally detailed but absent from the default view. |
| Operations, dependencies, and release | Warn | Smoke retains `.omx/qa-install-diff-evidence-review.png` and updates a development script/doc. Temporary data and fixture paths were absent and no app process remained. | End-user packaging still needs an exclusion rule for `.omx` QA assets. |

### 2026-07-10 - Evidence-driven eligible actions gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Candidate tests reject raw paths/names and require `CanExecuteDirectly=false`; GUI smoke returned `eligibleActionsPlanOnly=true` and found no buttons under the list. | Unknown paths, services, and tasks resolve to observe-only; no operation descriptor or handler was added. |
| Data, API, and consistency | Pass | Five enum kinds are generated deterministically from classified review items, deduplicated by category, and ordered cache/storage/migration/startup/observe. | Every candidate states evidence, missing evidence, safety, confirmation, and rollback requirements. |
| Code quality and maintainability | Pass | Candidate derivation stays in `InstallSnapshotDiffEvidenceReviewPresenter`; WPF only binds `EligibleActions`. Focus and viewport helpers are isolated in the smoke script. | Shared-helper promotion is recorded as a skill candidate. |
| Testing and verification | Pass | TDD red observed for missing models/rules/UI/smoke and both automation fixes. Full suite passed 217/217; solution build passed with 0 warnings/errors. | Fresh commands used the final workspace state. |
| Frontend, accessibility, and UX | Pass | `InstallDiffEligibleActionsListBox` has stable AutomationId, is non-selectable, is inside the default-collapsed review, and contains no buttons. Clean `.omx/qa-install-diff-eligible-actions.png` was visually inspected. | The screenshot shows candidate reasons, evidence, missing evidence, and safety copy. |
| Operations, dependencies, and release | Warn | Smoke retains one additional QA PNG and now uses bounded focus/viewport logic. No process or temporary fixture/data path remained. | End-user packaging still needs an exclusion rule for `.omx` QA assets. |
