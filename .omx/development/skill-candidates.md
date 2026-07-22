# Skill Candidates

### 2026-07-22 - Discover SDK tools from installer-owned roots

- Trigger: a Windows tool is absent from PATH/default Program Files but the product supports custom install drives.
- Reusable lesson: read exact installer-owned root metadata, validate it as a rooted existing directory, and perform a narrowly bounded child lookup; never replace a false default-path assumption with a recursive drive scan.
- Evidence: OMNIX found seven SignTool copies under registered `D:\Windows Kits\10` after the default-only inspector reported none.
- Proposed form: Windows release-tool discovery helper/checklist.
- Promotion threshold: promote after another SDK/toolchain uses the same pattern.

### 2026-07-22 - Requirement-to-evidence completion matrix

- Trigger: a long-running product goal has many green tests and inherited “connected” claims but completion must be audited against the original user plan.
- Reusable lesson: list every explicit requirement, then separate direct source connection, focused automated tests, current-environment observation, visual proof, and external behavioral acceptance. Treat missing evidence as missing rather than averaging it into a suite count.
- Evidence: the OMNIX audit found the absent recent-install sort despite 1033 passing tests, then preserved the remaining signed-disposable gap without redefining completion.
- Proposed form: cross-project completion-audit checklist or template.
- Promotion threshold: promote after one more long-running repository benefits from the same matrix.

### 2026-07-22 - Bind signing policy to the target Windows protection path

- Trigger: building a Windows release pipeline that uses Authenticode and targets Smart App Control or another reputation/protection layer.
- Reusable lesson: code-signing EKU and signature validity do not prove algorithm compatibility; inspect current platform guidance, enforce the allowed public-key algorithm at certificate discovery and signing, record it in immutable evidence, and recheck it after transfer.
- Evidence: the OMNIX pipeline originally admitted any code-signing certificate until the official-doc guide audit identified the current ECC limitation and added RSA checks across all three gates.
- Proposed form: release-security checklist or signing-pipeline template.
- Promotion threshold: promote after reuse in one more Windows release repository.

Use this file to collect lessons that may become future global skills, project rules, scripts, or templates.

### 2026-07-16 - PowerShell-safe repository search helper

- Trigger: symbol/content searches that also need filename filtering on Windows.
- Reusable lesson: `rg` filename globs belong in `-g` arguments and expected-zero searches need exit-zero count handling; embedding `*` in a Windows path causes OS error 123 and can discard parallel evidence.
- Evidence: wildcard-path failures repeated in both Agent and background-summary audits despite a prior error-ledger rule; expected-zero raw searches also repeatedly broke fail-fast batches.
- Proposed form: project script/helper accepting search root, content pattern, and optional `-g` filters, with deterministic counts and missing-root validation.
- Promotion threshold: already repeated; implement before the next broad source audit.

### 2026-07-16 - Resolve source paths before required-read batches

- Trigger: reading a type or test file whose exact path has not been observed in the current repository state.
- Reusable lesson: public type names do not reliably map to filenames. A guessed missing path in `Promise.all` or another fail-fast batch discards otherwise useful reads.
- Evidence: `StartupEntryControlPolicy`, `AppPresentationBuilder`, and both health builders were each requested from guessed locations before symbol/file discovery corrected them.
- Proposed form: repository-navigation helper or agent rule that requires `rg --files` / symbol search before a path enters a required parallel read.
- Promotion threshold: already repeated; promote before the next multi-file audit.

### 2026-07-16 - Exit-zero static absence checker

- Trigger: expected-zero authority, legacy-pattern, secret, or forbidden-reference checks in a parallel verification batch.
- Reusable lesson: raw `rg` uses exit code 1 for a healthy zero-hit result, which makes fail-fast orchestration discard unrelated successful evidence.
- Evidence: the same failure shape interrupted both exploratory and final verification batches in one slice; explicit .NET regex counts returned stable `pattern=0` evidence and exit code 0.
- Proposed form: project script/helper that accepts paths and patterns, prints deterministic counts, and exits nonzero only when a forbidden count is positive or input is invalid.
- Promotion threshold: implement before the next slice needs more than two zero-hit static gates.

### 2026-07-15 - Semantic static-source assertion helper

- Repeated lesson: exact full method signatures and immediate-neighbor signatures are fragile anchors when handlers become async or orchestration is split.
- Candidate: a small test helper that locates C# methods by method name and balanced braces, then applies forbidden-authority and order assertions to the extracted body.
- Benefit: preserves valuable source-level security checks while reducing maintenance failures caused only by `void`/`async` or neighboring-method changes.
- New evidence: inserting installer presentation/retry helpers before an old extraction endpoint made two Prepare-method contracts falsely green because the slice silently included the new methods.
- Implemented: `tests/Css.Tests/SourceMethodExtractor.cs` now requires a full declaration prefix and performs balanced method extraction with lexical handling for comments, strings, and characters; five synchronization test groups use it and three tests cover the helper itself.
- Suggested destination: keep as a project utility; consider cross-project promotion after another repository reuses the same contract pattern.

## Candidate Template

### YYYY-MM-DD - Candidate title

- Trigger:
- Reusable lesson:
- Evidence:
- Proposed form: project rule / global skill / script / template / discard
- Promotion threshold:

## Candidates

### 2026-07-07 - Safe WPF UIAutomation condition construction

- Trigger: Writing PowerShell UIAutomation smoke tests for WPF pages, dialogs, and screenshots.
- Reusable lesson: Use direct .NET constructors such as `[System.Windows.Automation.PropertyCondition]::new(...)` for UIA conditions; avoid `New-Object` overload binding for `ControlType` and complex condition values.
- Evidence: Both homepage and C-drive GUI verification hit `PropertyCondition value ... must be 'ControlType'` before switching to direct constructors.
- Proposed form: script / project rule
- Promotion threshold: Create a shared script helper before the next GUI smoke that needs more than basic button lookup.

### 2026-07-07 - WPF modal text smoke with AutomationId navigation

- Trigger: Verifying a WPF modal/dialog after localized safety-copy changes.
- Reusable lesson: Navigate the main window with stable `AutomationId` controls, open the modal through the real user path, locate the modal by `AutomationProperties.Name` or a stable child control, read text only inside that modal, and close the process in `finally` without invoking arbitrary modal buttons.
- Evidence: The uninstall safety modal GUI smoke first failed due PowerShell UIA constructor binding, process-root-child modal lookup, and a disabled close-button assumption. The passing version used `AutomationId`, modal name lookup, scoped text checks, and no modal button invocation.
- Proposed form: script / project rule
- Promotion threshold: Create a reusable script after the next WPF modal verification or before enabling any real mutation flow.

### 2026-07-07 - WPF modal text smoke should scope to the modal window

- Trigger: Testing WPF modal/dialog visible text after localization or safety-copy changes.
- Reusable lesson: Locate a stable child control in the modal, walk up to its `ControlType.Window`, and search only that window's descendants. Root-wide searches can find hidden or unrelated controls and create false positives.
- Evidence: A migration-window GUI smoke falsely found old `Preview only` text when scanning root descendants. A scoped search under the migration window found no old English text and verified the localized modal copy.
- Proposed form: script / project rule
- Promotion threshold: Create a reusable script if another modal localization or confirmation-window QA pass is needed.

### 2026-06-30 - Preserve source encoding during scripted edits

- Trigger: Editing localized source files, mojibake-affected files, or files that must remain valid UTF-8.
- Reusable lesson: Avoid `Get-Content` / `Set-Content` rewrites for source files unless encoding is explicitly controlled. Prefer `apply_patch` with stable ASCII anchors; if rewriting an invalid file is unavoidable, use explicit UTF-8 and verify immediately.
- Evidence: A PowerShell rewrite corrupted `tests\Css.Tests\ProductExperienceTests.cs` enough that `apply_patch` later failed with an invalid UTF-8 sequence; explicit UTF-8 rewrite plus tests recovered it. A later installer XAML check also produced a false XML parse failure when `Get-Content` decoded Chinese through the default code page; strict `UTF8Encoding(false, true)` parsed the unchanged file successfully.
- Proposed form: project rule / global skill
- Promotion threshold: Promote if another localized source or project protocol file is damaged by implicit encoding.

### 2026-06-30 - Serialize .NET verification sharing obj/bin

- Trigger: Running multiple `dotnet test` / `dotnet build` commands in the same worktree.
- Reusable lesson: Commands that compile the same projects can race on shared `obj/bin` outputs and VBCSCompiler locks; serialize them or use isolated output paths.
- Evidence: `dotnet build` failed with CS2012 on `Css.Scanner.dll` while `dotnet test` was compiling; later, parallel filtered `dotnet test` commands failed with CS2012 on `Css.Core.dll`; `dotnet build-server shutdown` plus serial verification fixed both.
- Proposed form: project rule / global skill
- Promotion threshold: Add to a global verification skill if the same lock appears in another .NET repo.

### 2026-06-30 - WPF navigation smoke should assert visible page title

- Trigger: Testing WPF navigation after changing left-side buttons or page layout.
- Reusable lesson: UI Automation `Invoke` only proves a button can fire; it does not prove the user perceives navigation. After clicking, assert a visible page title or other page-specific text.
- Evidence: Earlier nav smoke only returned `invoked`, while the user still experienced “button clicked but no response”; the corrected smoke checks `ok: <button> -> <title>` for each page.
- Proposed form: script / project rule
- Promotion threshold: Create a reusable script after one more WPF navigation refactor or if another project needs similar UIA checks.

### 2026-06-30 - Windows scanner source fallback checklist

- Trigger: Reading Windows system facts such as services, startup entries, tasks, signatures, and install paths.
- Reusable lesson: WMI/CIM/COM/command output can fail under normal user permissions; scanner features need at least one read-only fallback where possible and must degrade per source instead of failing the whole scan.
- Evidence: Marvis service `Win32_Service` query returned access denied, while the service registry `ImagePath` was readable and enough to associate `MarvisSvr`.
- Proposed form: project rule / global skill
- Promotion threshold: Promote if another scanner source needs fallback or if this project adds network/device/driver scanning.

### 2026-06-30 - Persist rollback references at execution time

- Trigger: Any UI or agent action that claims the user can undo a mutation.
- Reusable lesson: The action timeline must store the exact rollback evidence, such as quarantine manifest paths or snapshot ids, when the mutation succeeds; affected paths alone are not enough to safely undo.
- Evidence: 后悔药中心 restore UI could not be safely wired until `QuarantineOperationHandler` wrote manifest paths into `ActionTimelineEntry`.
- Proposed form: project rule / global skill
- Promotion threshold: Promote if uninstall, migration, service disable, or registry-change flows need the same rollback reference pattern.

### 2026-06-30 - Rollback asset retention checklist

- Trigger: Adding storage for undo/rollback assets such as quarantine copies, snapshots, backups, or uninstall residual manifests.
- Reusable lesson: Rollback assets must have visible retention and capacity policy; automatic permanent cleanup should be a separately confirmed action, not a hidden side effect.
- Evidence: OMNIX-Entropy quarantine restore required copied files to remain available, but those copies can consume D drive space without a visible policy.
- Proposed form: project rule / global skill
- Promotion threshold: Promote when migration snapshots or uninstall residual backups add a second rollback asset type.

### 2026-06-30 - Safe uninstall residue classification checklist

- Trigger: Implementing uninstall cleanup, residue scanning, or “clean uninstall” UX.
- Reusable lesson: Treat cache/log path residue as the only default low-risk class; user data/install directories are medium risk; startup entries, services, scheduled tasks, registry keys, and drivers are high risk until snapshot and rollback evidence exists.
- Evidence: OMNIX-Entropy now only converts low-risk path residue into `uninstall.residue.quarantine`; high-risk service/task/startup candidates remain explanation-only.
- Proposed form: project rule / global skill
- Promotion threshold: Promote when registry residue scanning or real uninstall execution is added.
### 2026-07-07 - WPF foreground app-window screenshot helper

- Trigger: WPF visual QA screenshots can be obscured by other user windows, and repeated UIA scripts need stable condition construction.
- Reusable lesson: Launch the WPF app, bring its main window foreground/topmost, construct UIA conditions with explicit `-ArgumentList`, capture the app window bounds instead of the whole screen, and always stop the launched process.
- Evidence: Homepage Agent verification first produced an obscured screenshot because another app covered OMNIX; rerunning with foreground/topmost and app-window bounds produced `.omx\qa-home-agent-inline-response-visible.png`.
- Proposed form: script
- Promotion threshold: Create a small QA script under a future `scripts/qa/` folder before the next multi-step WPF visual verification pass.
## 2026-07-07 - Reusable WPF real-scan GUI smoke helper

- Trigger: Repeated need to launch `Css.App.exe`, navigate to a page, click a scan button, wait for UI text, capture a screenshot, and verify action-button state.
- Candidate type: Project script/template.
- Why reusable: Long PowerShell UIAutomation scripts are error-prone and were retyped several times for C-drive scan verification.
- Suggested asset: `scripts/gui-smoke-cdrive.ps1` with parameters for expected text, screenshot path, timeout, and optional button state checks.

## 2026-07-07 - Reusable WPF app-drawer action smoke helper

- Trigger: Repeated need to launch `Css.App.exe`, scan apps, select an app tile, invoke an app-drawer action, verify visible drawer text, and capture a screenshot.
- Candidate type: Project script/template.
- Why reusable: The residue-review flow required multiple similar UIAutomation scripts to prove the visible result was not refreshed away or hidden below unrelated sections.
- Suggested asset: `scripts/gui-smoke-app-drawer.ps1` with parameters for action button name, expected text fragments, screenshot path, and "must not execute" notes.
## 2026-07-07 - WPF app-drawer real-click smoke helper

- Trigger: UIAutomation `InvokePattern` can report success on a drawer action button while no modal or visible result appears.
- Candidate type: Project script/template.
- Why reusable: App-drawer flows now include uninstall preview, residue review, migration preview, cache cleanup planning, and future startup/service actions. Each needs scan/select/click/verify/screenshot/cleanup behavior.
- Suggested asset: `scripts/gui-smoke-app-drawer-click.ps1` with parameters for target drawer button AutomationId, required modal/drawer text fragments, screenshot path, timeout, and "no execution" assertions. It should prefer `GetClickablePoint()` real-click, capture diagnostics if no modal/result appears, and always stop the launched `Css.App` process.

## 2026-07-08 - Local Agent advice presenter checklist

- Trigger: Adding Agent-facing guidance, explanation, or next-step recommendations.
- Candidate type: Project rule/checklist.
- Why reusable: The product repeatedly needs AI-like advice that is beginner-readable but cannot bypass local evidence, confirmation, quarantine, rollback, or operation-pipeline rules.
- Suggested asset: A checklist requiring Agent advice to be produced by a core presenter, use only local summaries unless the user opts into cloud analysis, expose `CanExecuteDirectly=false`, include safe next actions, include blocked actions, and convert any future executable step into an auditable local operation plan.
## 2026-07-08 - Stable-anchor patching for mojibake-heavy WPF files

- Candidate: Add a small project guideline or helper for editing `MainWindow.xaml` / `MainWindow.xaml.cs` with stable anchors.
- Trigger: Repeated patch failures occur when long contexts include historical mojibake Chinese UI strings.
- Proposed rule: Use `x:Name`, method names, ASCII property names, or numeric XML references as patch anchors; avoid matching long localized text blocks.
- Benefit: Reduces wasted retries and lowers risk of accidental edits in the wrong UI section.

## 2026-07-08 - ASCII-safe localized UIAutomation smoke scripts

- Candidate: Create a reusable QA script template for WPF UIAutomation that constructs localized labels with `[char]0x...` and uses Windows PowerShell 5-compatible APIs.
- Trigger: Raw Chinese strings and newer .NET string overloads broke the app-drawer GUI smoke script before product behavior could be verified.
- Proposed rule: QA scripts should avoid raw localized string literals, avoid PowerShell 7-only behavior, emit screenshots, and close only the process they launch.
- Benefit: Makes GUI proof repeatable for app drawer, C-drive, Agent, and undo-center flows without repeatedly debugging encoding/runtime compatibility.

## 2026-07-08 - Agent capability card checklist

- Candidate: Project rule/checklist for adding or exposing Agent skills.
- Trigger: The Marvis-inspired skill catalog needed user-facing next-step labels and explicit safety hints before it was useful for beginners.
- Proposed rule: Every Agent capability shown in the UI should include category, plain title, next step, execution mode, risk label, and "what will not happen automatically" text. High-risk actions must remain plan-only until evidence, confirmation, rollback/quarantine, and operation-pipeline gates exist.
- Benefit: Keeps the Agent helpful without making it look like it can silently change system settings or run risky actions.

## 2026-07-08 - Allowlisted Windows tool/deep-link catalogs

- Candidate: Project rule/template for Agent "open this Windows place" capabilities.
- Trigger: System-tool shortcuts needed to be useful while avoiding arbitrary command execution.
- Proposed rule: Any Windows tool or settings shortcut exposed in UI must come from a fixed catalog with id, command/deep-link, risk, confirmation requirement, and safety hint. UI handlers must look up catalog ids instead of accepting arbitrary executable paths.
- Benefit: Lets OMNIX-Entropy feel capable and convenient without becoming a generic unsafe launcher.

## 2026-07-08 - First-screen visual proof for Agent conclusions

- Candidate: Project GUI QA rule/template.
- Trigger: Agent background review controls existed and UIAutomation could find them, but the first screenshot showed the panel was below the visible area and therefore not useful to a beginner.
- Proposed rule: Any primary Agent conclusion panel must have explicit AutomationIds, a static ordering test when placement matters, and a real screenshot proving it appears in the first visible working area after the relevant scan.
- Benefit: Prevents technically-present Agent advice from being effectively hidden under dense UI.
- 2026-07-08 update: The startup/service plan preview repeated the same issue; the lesson was promoted into `AGENTS.md` as a project UX rule. Next useful asset is a reusable Agent-page GUI smoke helper.

## 2026-07-08 - Encoding-safe localized WPF smoke assertions

- Candidate: Project GUI QA script template.
- Trigger: The Agent startup/service GUI smoke failed because a raw Chinese PowerShell string literal did not match UIAutomation text under Windows PowerShell script encoding.
- Proposed rule: WPF smoke scripts should assert localized UI text using AutomationIds, code-point-built phrases, or stable nonlocalized state; avoid raw Chinese literals in `.ps1` assertions unless the file encoding is explicitly controlled and verified.
- Benefit: Keeps GUI smoke focused on product behavior instead of repeatedly debugging script encoding.

## 2026-07-08 - WPF MessageBox cancel-smoke template

- Candidate: Project script/template.
- Trigger: The Windows Settings confirmation smoke needed repeated logic for launching WPF, finding a visible button, finding a process-owned MessageBox, capturing it, canceling it, and proving no external Settings process launched.
- Proposed asset: `scripts/gui-smoke-messagebox-cancel.ps1` helper with parameters for target page/button AutomationId, expected dialog title, external process name to watch, screenshot path, and cancel strategy.
- Benefit: Makes future high/medium-risk confirmation gates testable without re-debugging UIAutomation window search or localized cancel buttons.

## 2026-07-08 - Do not parallelize dotnet commands with shared outputs

- Candidate: Project rule/checklist.
- Trigger: Running `dotnet test` and `dotnet build` in parallel caused `CS2012` because both commands wrote to the same `obj` output while `VBCSCompiler` held the file.
- Proposed rule: Run solution build and tests sequentially unless each command uses isolated output/intermediate paths.
- Benefit: Avoids false test failures and wasted diagnosis time on Windows file locks.

## 2026-07-08 - Unique-anchor XAML patching in repeated page layouts

- Candidate: Project editing guideline.
- Trigger: A generic `Border Grid.Column="1"` patch added an Agent-only ScrollViewer to the C-drive recommendation column and broke XAML.
- Proposed rule: For `MainWindow.xaml`, anchor edits on unique names (`AgentPage`, `AgentSkillListBox`, `AgentWindowsSettingsListBox`) and verify the surrounding page before patching repeated layout containers.
- Benefit: Reduces broken XAML and accidental edits in the wrong page section.

## 2026-07-08 - WPF smoke target exposed controls, not layout containers

- Candidate: Project GUI smoke rule/template.
- Trigger: The app drawer smoke could find the preview title and list but not the decorative `Border` host with an AutomationId.
- Proposed rule: Put and assert AutomationIds on UIAutomation-exposed controls such as buttons, text blocks, list boxes, and list items. Avoid `Border`, `Grid`, and layout panels as mandatory smoke targets unless a real run proves they appear in the automation tree.
- Benefit: Reduces false GUI failures and keeps smokes aligned with what the user can actually read or click.

## 2026-07-08 - Conditional action smokes should search for eligible data

- Candidate: Project GUI smoke template enhancement.
- Trigger: The app drawer migration button was correctly disabled for a selected D-drive app, causing a false failure when the smoke assumed one app enabled every action.
- Proposed rule: For action buttons whose availability depends on scanned data, loop through real list items until the target button is enabled, then invoke it. Report failure only when no eligible item exists.
- Benefit: Verifies real behavior without weakening product rules or relying on fragile local-machine ordering.

## 2026-07-08 - Build WPF app before GUI smoke after XAML/code-behind changes

- Candidate: Project verification checklist rule.
- Trigger: The enhanced app-drawer smoke launched a stale `Css.App.exe` that did not include newly added AutomationIds.
- Proposed rule: If `src/Css.App/MainWindow.xaml` or `MainWindow.xaml.cs` changes, run `dotnet build ComputerSecuritySoftware.slnx --no-restore` before any `.omx` GUI smoke that launches the WPF app.
- Benefit: Avoids false GUI failures caused by stale binaries.

## 2026-07-08 - Required view model fields need initializer search

- Candidate: Project refactor checklist rule.
- Trigger: Adding required fields to `AppDrawerActionHostViewModel` broke a direct initializer in `ShowResidueReviewInline(...)`.
- Proposed rule: After adding `required` members to a shared C# view model, use `rg "new <TypeName>"` and update all object initializers, not just the main presenter.
- Benefit: Catches compile failures earlier and keeps side paths such as inline residue review consistent with the main UI model.
## 2026-07-08 - Corrupted WPF/XAML block repair workflow

- Lesson: When WPF XAML contains mojibake or malformed attribute text, literal `apply_patch` can fail and UIAutomation proof can become unreliable.
- Candidate skill/script: A small verifier that prints line numbers, character codes, and XML parse/build status around suspected corrupted XAML blocks, then recommends XML character references for rewritten localized text.
- Trigger: XAML patch context fails on visible lines, or a localized UI block shows attributes swallowed into text.
- Reuse value: Cross-project for Windows desktop work with mixed encodings.

## 2026-07-08 - Read-only GUI smokes should avoid polluting user recovery data

- Lesson: For safety-critical pages such as undo/quarantine centers, GUI smokes should prove visible hooks and affordances without seeding the user's real app database unless an isolated profile/app-data override exists.
- Candidate skill/script: Shared smoke helper supporting isolated app-data roots for GUI tests, plus a default read-only mode for user-profile runs.
- Trigger: GUI smoke needs timeline/history/quarantine data to verify a restorable state.
- Reuse value: Cross-project for desktop apps that store recovery, history, or privacy-sensitive local state.

## 2026-07-09 - Process-scoped storage overrides for desktop GUI smokes

- Lesson: A small Core path resolver plus process-scoped environment variables lets GUI smokes exercise real app code without touching the user's real timeline/quarantine data.
- Candidate skill/script: A reusable PowerShell helper that creates temp app-data/quarantine roots, sets env vars before `Start-Process`, restores previous env vars, and removes temp roots in `finally`.
- Trigger: Desktop GUI smoke needs realistic local state for undo/history/recovery workflows.
- Reuse value: Cross-project for Windows apps with local state, history, caches, rollback manifests, or quarantine folders.

## 2026-07-09 - Desktop GUI state seed tools should reuse domain services

- Lesson: When a GUI smoke needs realistic history/quarantine state, a small dev CLI that calls the app's domain services is safer than hand-writing SQLite/JSON in PowerShell or adding hidden WPF seed switches.
- Candidate skill/script: A `smoke-tools` template for commands such as `seed-undo-center`, plus a PowerShell wrapper that asserts the tool exists after solution build.
- Trigger: GUI smoke must verify a non-empty state that would otherwise require mutating the user's real profile.
- Reuse value: Cross-project for desktop apps with undo centers, audit histories, local queues, recovery records, or offline caches.

## 2026-07-09 - Beginner-facing history rows should summarize paths first

- Lesson: Timeline/history rows are often safety-critical but can become unreadable if they show full paths by default.
- Candidate skill/script: A presentation test helper that asserts first-level history rows do not include drive-root/user-profile paths and instead expose counts, app names, or friendly scopes.
- Trigger: A screenshot or test row shows `C:\Users\...`, `%LocalAppData%`, registry keys, service names, or scheduled-task identifiers in the beginner-facing layer.
- Reuse value: Cross-project for maintenance/security tools where technical evidence must exist but should be behind a details view.

## 2026-07-09 - WPF smoke helper extraction template

- Lesson: GUI smoke scripts for WPF repeatedly need the same UIAutomation initialization, AutomationId lookup, polling, invoke, screenshot, and cleanup patterns.
- Candidate skill/script: A shared `wpf-smoke-helpers.ps1` template plus static tests requiring scripts to dot-source it.
- Trigger: More than one `.omx/gui-*.ps1` script defines `Find-ByAutomationId`, `Wait-Until`, `Invoke-Element`, or screenshot helpers.
- Reuse value: Cross-project for Windows desktop UI testing and local smoke verification.

## 2026-07-09 - Native fallback for WPF confirmation dialogs

- Lesson: Desktop-wide UIAutomation descendant searches can fail or miss modal dialogs because unrelated providers throw COM exceptions.
- Candidate skill/script: Add a reusable helper that finds secondary windows by process-owned top-level native handles using `EnumWindows` and `GetWindowThreadProcessId`, then converts them with `AutomationElement.FromHandle`.
- Trigger: A WPF GUI smoke needs to find a `MessageBox`, modal, or confirmation dialog for the launched app process.
- Reuse value: Cross-project for Windows desktop smoke tests that must verify cancel/confirmation paths without opening real system tools or changing settings.

## 2026-07-09 - Collapsed evidence details for beginner-facing history

- Lesson: Beginner-facing history/timeline rows should hide raw paths by default, but hiding them entirely weakens auditability and future restore/debug workflows.
- Candidate skill/script: A presentation-test helper that asserts first-level rows hide raw paths/service names/registry keys, while a collapsed `TechnicalDetails` or equivalent second-level collection retains exact evidence.
- Trigger: A maintenance, recovery, security, or cleanup UI needs both "human summary" and "exact evidence" for the same action.
- Reuse value: Cross-project for PC managers, security tools, backup/restore tools, and admin utilities with undo timelines or quarantine histories.
## 2026-07-09 - WPF modal window discovery through stable descendant controls

- Trigger: Repeated WPF GUI smokes missed modal windows when searching only root child windows for the launched process.
- Reusable lesson: For WPF modal verification, find a stable child `AutomationId` or modal `AutomationProperties.Name`, then walk up with `TreeWalker.ControlViewWalker` to scope assertions to the owning window. Keep root-child and native process-window search as optional fast paths, but do not rely on them alone.
- Candidate form: Add a helper to `.omx/wpf-smoke-helpers.ps1` or a future GUI-smoke skill reference for `Find-WindowByDescendantAutomationId`.
- Evidence: `.omx/gui-uninstall-plan-window-smoke.ps1` first failed with `Uninstall plan window was not found`, then passed after adding descendant lookup; earlier error-ledger entries document similar settings/uninstall modal discovery issues.
- Distillation update: The helper now exists in `.omx/wpf-smoke-helpers.ps1` as `Find-SecondaryWindowWithChild` plus `Find-WindowByDescendantAutomationId`, and both C-drive cleanup confirmation and uninstall-plan smokes use it.

## 2026-07-09 - Process-scoped scan sequence fixtures for external-state transitions

- Trigger: The uninstall-residue confirmation path depends on state changing outside the app: software exists before official uninstall, then no longer appears after uninstall.
- Reusable lesson: For desktop GUI smokes that need an external-state transition, prefer a process-scoped JSON scan-sequence fixture over mutating real registry, installed apps, services, startup entries, scheduled tasks, or user data.
- Candidate form: A reusable fixture-scanner pattern with an env-var resolver, sequence JSON schema, docs that mark it dev/test-only, and static tests ensuring normal app behavior is unchanged when the env var is absent.
- Evidence: `OMNIX_ENTROPY_SOFTWARE_FIXTURE` plus `SoftwareInventoryFixtureScanner` lets `.omx/gui-uninstall-residue-confirmation-smoke.ps1` simulate installed-then-removed software while staying cancel-only.

## 2026-07-09 - UTF-8 localized source line inspector

- Trigger: Multiple WPF handler patches failed or became risky because terminal output rendered localized strings as mojibake or truncated large file reads.
- Reusable lesson: Before patching localized C#/XAML text, inspect a narrow line-number range from the file bytes with UTF-8-safe output instead of relying on normal terminal rendering.
- Candidate form: A small script such as `.omx/show-source-lines.ps1 -Path <file> -Start <n> -End <n>` that prints line numbers, escaped text, and optionally Unicode code points.
- Evidence: Error ledger entries for residue confirmation, install analysis route-source text, and install-route memory handler all point to localized patch context fragility.

## 2026-07-10 - Screenshot-backed WPF conclusion verification

- Trigger: A WPF GUI smoke reports a beginner-facing conclusion as visible, but the generated screenshot may still omit, clip, or visually obscure it.
- Reusable lesson: UIAutomation assertions prove the control tree and state; screenshot inspection proves the rendered result. Safety/product conclusions need both.
- Candidate form: Extend `wpf-smoke-helpers.ps1` with a reusable maximize-scroll-target-screenshot helper and a checklist requiring target text, non-offscreen bounds, no capture artifacts, and post-run process/fixture cleanup.
- Evidence: The first install-diff Agent smoke returned `agentHeadlineVisible=true` while the screenshot did not show the panel; forced bottom scroll, desktop capture, maximize guards, and visual rerun produced valid evidence.
- Reuse value: Cross-project for desktop maintenance, security, backup, and admin tools where a conclusion must appear in the first visible working area.

## 2026-07-10 - Encoding-stable localized assertions in Windows PowerShell GUI smokes

- Trigger: Windows PowerShell 5.1 compared a no-BOM UTF-8 CJK source literal against correct WPF UIAutomation Unicode text and reported false; a multi-char `String.Concat` workaround also failed overload binding.
- Reusable lesson: Keep smoke-script source assertions ASCII-only. Prefer AutomationIds; when localized text is safety-critical, build a short keyword with `-join @([char]0x....)` and print the actual UIA name on mismatch.
- Candidate form: Add an encoding section and helper such as `New-UnicodeTextFromCodePoints` to the shared WPF smoke tooling/skill, plus a static guard against localized literals in assertion code.
- Evidence: `.omx/gui-install-diff-agent-smoke.ps1` failed twice despite correct rendered text, then passed with `nothingExecutedVisible=true` after char-code `-join` construction.
- Reuse value: High across Windows desktop automation executed by Windows PowerShell 5.1 on mixed-locale machines.

## 2026-07-10 - Apply-patch recovery checklist after an anchor miss

- Trigger: Two combined patches in the same slice were rejected because documentation/test anchors came from stale remembered wording.
- Reusable lesson: After one anchor miss, stop multi-file patches for the rest of the turn. Re-read each target boundary, patch one file at a time, and immediately verify the changed anchor.
- Candidate form: A short checklist in `agent-dev-protocol` or a helper that previews unique anchor counts before applying a patch.
- Evidence: The action-plan test and smoke-doc patches both failed safely; separate current-state anchors succeeded.
- Reuse value: Cross-project for long-lived append-only logs and localized source files.

## 2026-07-10 - Composition-aware WPF screenshot capture

- Trigger: A desktop screenshot intermittently showed large black rectangles over WPF list-item content even though UIAutomation state and an unchanged rerun were correct.
- Reusable lesson: Screen-copy capture can race desktop composition. A retained QA artifact needs post-capture validation, not just a successful save and nonzero file size.
- Candidate form: Extend `wpf-smoke-helpers.ps1` with DWM flush/focus stabilization, optional retry, and black-region sampling scoped to a target AutomationElement rectangle.
- Evidence: The first classification-summary screenshot was rejected visually; the same smoke rerun without code changes produced a clean 606,740-byte image showing all content.
- Reuse value: Cross-project for hardware-accelerated WPF/WinUI desktop smoke tests.

## 2026-07-10 - Visibility-only and viewport-aware WPF smoke helpers

- Trigger: A WPF smoke failed `SetFocus()` after `WaitForInputIdle`; bounded focus polling later timed out with `keyboardFocusable=false`, and native `SetForegroundWindow` was rejected by Windows foreground-lock policy. Nested-list `IsOffscreen` also contradicted a screenshot.
- Reusable lesson: Process readiness, keyboard focus, UIAutomation invocation, z-order visibility, and rendered viewport intersection are separate states. Most button-driven screenshot smokes need only input-idle readiness, UIAutomation `InvokePattern`, visible z-order, and rectangle-based viewport checks.
- Candidate form: Keep `Show-WpfWindowForSmoke` in the shared helper with `ShowWindowAsync` plus `SetWindowPos(HWND_TOPMOST)`, never foreground acquisition. Move viewport-intersection scrolling into the helper and reserve focus helpers for explicit keyboard-input tests.
- Evidence: Focus and foreground strategies both timed out despite an enabled onscreen WPF window. The visibility-only static contract passes; real screenshot proof awaits GUI launch approval. Earlier rectangle-based scrolling produced the clean eligible-actions screenshot after `IsOffscreen` omitted the target.
- Reuse value: High across Windows desktop GUI automation, especially security/maintenance apps where smoke runs should not steal the user's keyboard focus.

## 2026-07-11 - WPF binary freshness before GUI smoke

- Trigger: A WPF source-contract test passed, but the GUI smoke launched an older Css.App.exe because the test project does not reference the app project.
- Reusable lesson: Desktop GUI verification has a binary-freshness gate between source tests and UI automation. A smoke script that intentionally does not build must require a successful app/solution build after the latest WPF edit.
- Candidate form: Add a helper that compares source/project timestamps with the target executable, or make the verification checklist run an explicit app build before any binary-launch smoke.
- Evidence: The checklist scroll-target change appeared only after dotnet build ComputerSecuritySoftware.slnx --no-restore; the unchanged smoke then showed the status in the visible area.
- Reuse value: High for WPF, WinUI, Electron, Tauri, and other GUI projects whose tests do not necessarily rebuild the launched artifact.

## 2026-07-11 - Separate screenshot-file integrity from viewer rendering

- Trigger: The original-detail viewer showed black blocks while PNG pixel sampling and a normal-size render showed a complete light UI.
- Reusable lesson: Screenshot QA must distinguish capture bytes, compositor output, image decoding, and viewer scaling. A single viewer rendering is insufficient evidence of file corruption.
- Candidate form: Add sampled dark/alpha ratios and a resized decode check to screenshot validation before deciding whether to rerun capture.
- Evidence: The saved uninstall-plan PNG sampled roughly 0.2% dark pixels and reopened cleanly at high detail despite one corrupted-looking original preview.
- Reuse value: Cross-project for large high-DPI desktop screenshots.

## 2026-07-11 - Authenticated replay-safe local command envelope

- Trigger: A future local elevated helper must reject wrong keys, stale/future messages, duplicate message ids/nonces, mutated payloads, mismatched responses, and cancellation without invoking a handler.
- Reusable lesson: Validate structure and freshness, recompute payload integrity, verify HMAC with fixed-time comparison, then allocate replay state before invoking side effects. Never let unauthenticated traffic consume the replay table, and never convert cancellation into a normal failure response.
- Candidate form: A reusable local-command envelope/endpoint template with canonical length-prefixed fields, minimum 256-bit key, bounded clocks/capacity, separate message/nonce tables, response correlation, and injected handler.
- Evidence: OfficialUninstallAuthenticatedTransportTests cover valid, wrong-key, stale, replay, mutated-descriptor, cancellation, and response-mismatch paths; fake-handler integration reaches the real safety pipeline without a process launch.
- Reuse value: High for desktop apps using elevated helpers, local agents, update services, or privileged maintenance brokers.

## 2026-07-12 - OS-correlated bounded Windows named-pipe boundary

- Trigger: A desktop app needs to send a high-risk request to a local elevated helper without trusting JSON identity claims or exposing raw machine evidence.
- Reusable lesson: Combine `CurrentUserOnly` with server-side client SID/PID/session derivation, client-side server PID/session correlation, a bounded length-prefixed strict schema, request replay protection, request-bound response authentication, explicit timeouts, and path-free typed results.
- Candidate form: A reusable Windows local-IPC test harness with fake endpoint and mandatory malformed/oversized/tampered/replayed/wrong-peer/timeout/cancellation/early-close cases.
- Evidence: `OfficialUninstallSerializedNamedPipeTransportTests` passes 14 cases, including a real current-user pipe round trip and private-summary exclusion.
- Reuse value: High for elevated brokers, update helpers, local agents, and security/maintenance tools.

## 2026-07-12 - Authority-shaped project reference gate

- Trigger: A desktop UI needs to communicate with an elevated/helper executable while compile-time dependencies risk exposing execution adapters.
- Reusable lesson: Put pure contracts in Core, transport in a neutral inward-only library, and execution adapters in the elevated executable. Assert the UI references transport but not Elevated, and the transport references Core but not Elevated.
- Candidate form: A static project-graph test plus forbidden-authority source scan and Release binary smoke-string audit.
- Evidence: OMNIX App-to-Ipc-to-Core graph supports a real DEBUG pipe GUI flow while handler/launcher/Program remain unreachable; full tests and both configurations build cleanly.
- Reuse value: High for desktop apps with privileged brokers, updaters, maintenance workers, or local agents.

## 2026-07-12 - Identity-bound ephemeral local IPC bootstrap

- Trigger: Two local processes already have an OS-authenticated channel but must establish a fresh command-authentication key without command-line/environment/file secrets.
- Reusable lesson: Bind ephemeral ECDH and fresh nonces to a canonical transcript containing protocol, channel, logical session, and independently observed peer identities; derive with HMAC-SHA256 extract/expand; verify server/client finished MACs; expire/reject nonce replay; zero all owned key material.
- Candidate form: A reusable stream bootstrap with bounded strict codec, replay guard, disposable key owner, and mandatory mismatch/tamper/replay/timeout/cancel tests.
- Evidence: `OfficialUninstallSessionBootstrapTests` proves live named-pipe agreement and all refusal cases; OMNIX full suite/builds remain green with no worker registration.
- Reuse value: High for elevated brokers, update workers, local agents, and privileged maintenance helpers.

## 2026-07-12 - Disposable child-process smoke harness

- Candidate: a reusable .NET test harness that launches a local tool through `ProcessStartInfo.ArgumentList`, captures bounded stdout/stderr, enforces startup/response/shutdown deadlines, and kills the entire child tree on failed assertions or disposal.
- Evidence: the OMNIX authenticated worker slice needed the same lifecycle guarantees across success, timeout, and parent-failure paths; direct tests caught output-path and receipt-schema mistakes early.

## 2026-07-13 - Fail-closed source-only syntax audit

- Trigger: artifact generation was intentionally paused by endpoint-security policy, and an attempted Roslyn parse under Windows PowerShell failed while the script still printed misleading PASS lines.
- Reusable lesson: a static parser is itself a verification dependency. Set terminating error behavior, prove the parser type/runtime loaded, require a non-null syntax tree for every file, and suppress all PASS output after any startup or per-file failure.
- Candidate form: a repository script that performs UTF-8 validation, fail-closed syntax parsing without emitting assemblies, XML/XAML parsing, scoped authority scans, and an explicit `not compiled` receipt.
- Evidence: the failed OMNIX Roslyn loader attempt was caught only by reading its error stream; explicit invariant scans remained usable but cannot replace compiler/type checking.
- Reuse value: High for security incidents, offline environments, or repositories where compilation is temporarily prohibited.

### Candidate extension - deterministic PowerShell audit wrapper

- Add repository path discovery before scoped searches, accept regex patterns as data rather than interpolated command text, run checks independently, and emit a failed receipt for missing scopes or parser dependencies. This avoids the repeated nested JavaScript/PowerShell/regex quoting failures seen during the OMNIX source-only pause.
- Exclude `bin/obj` by construction, decode owned source with strict UTF-8, parse XAML structurally, and preserve every independent result even when one check fails. Expected zero-match assertions need an explicit success state instead of inheriting `rg` exit code 1.

## 2026-07-13 - Compensating local mutation with visible recovery commit

- Trigger: a desktop maintenance action moves multiple local resources and promises the user that every change can be undone.
- Reusable lesson: persist recovery coordinates before each mutation, reject overlapping targets, keep all mutations plus the user-visible recovery index in one protected transaction, and compensate completed items in reverse order if any later move or index write fails. Revalidate both current owner identity and resource state immediately before execution.
- Candidate form: an injected batch-operation coordinator with prepare/mutate/index/compensate phases, path-free typed outcomes, fault injection at every boundary, and an orphan-manifest recovery view.
- Evidence: the OMNIX cache closure found that post-confirmation path checks did not bind refreshed app ownership and that timeline failure was outside quarantine rollback; both were corrected in source.
- Reuse value: High for quarantine, migration, uninstall residue cleanup, file archival, local backup, and other reversible desktop maintenance workflows.
# Candidate - Follow evidence records back to a beginner-visible safe next step

- Trigger: a Worker/service writes diagnostics, rollback evidence, monitoring records, or audit state that the UI is supposed to use.
- Workflow: trace write -> bounded validation -> read-only observer -> privacy-safe presenter -> first-visible UI -> exact target resolution -> safe next action; test every warning for a reachable next click.
- Reuse: maintenance tools, installers, migration systems, backup clients, and other apps where backend safety evidence can exist without becoming a usable feature.
# Candidate - Model not-present, unavailable, and insufficient-history separately

- Trigger: a beginner-facing diagnostic combines optional hardware, permission-dependent probes, and trend data.
- Workflow: define explicit availability states; keep raw probe failures private; map hardware absence to `不适用`, read failure to `未检测`, and short history to `历史不足`; never backfill with estimates.
- Reuse: health dashboards, hardware utilities, backup tools, security checks, and any Agent that must explain why it cannot yet decide.

# Candidate - Bind observation completeness into every absence claim

- Trigger: a before/after scanner is bounded, permission-dependent, cancelable, or otherwise capable of returning partial evidence.
- Workflow: model completeness explicitly; bind status/count/fingerprint into persisted evidence; compare uncertain sets only when both sides are complete; preserve independently known findings; propagate incompleteness through summaries, Agent advice, action eligibility, and final confirmation.
- Prevention rule: zero findings plus incomplete evidence must never become `未发现`, low risk, or executable authority.
- Reuse: installers, backup verification, malware scans, migration closure checks, inventory audits, log collectors, and any bounded monitoring system.

# Candidate - Privacy-smoke the whole visible window, not only the target panel

- Trigger: a scan-derived conclusion hides private paths or identifiers in one primary panel while adjacent cards, status areas, or Agent summaries render the same evidence through another presenter.
- Workflow: assert target-control text, collect every currently visible text element under the owning window, reject private fixture tokens globally, then inspect a window-only screenshot for clipping and cross-panel leaks.
- Evidence: OMNIX personal-storage UIAutomation passed while the adjacent recommendation panel exposed the full fixture path; the full-window assertion and screenshot review caught and prevented the leak.
- Reuse: diagnostics, security tools, backup clients, uninstallers, migration assistants, and any multi-panel desktop UI using shared scan evidence.

# Candidate - Use one semantic source-method extraction helper

- Trigger: static authority/order tests repeatedly extract C# method bodies with exact signature strings and local index arithmetic.
- Workflow: centralize extraction around method names, validate start/end boundaries before slicing, tolerate intentional `async` signature changes, and report the missing semantic anchor directly.
- Evidence: two separate OMNIX test helpers threw `ArgumentOutOfRangeException` when async handler transitions made the start marker absent; both needed the same boundary fix.
- Reuse: repositories that supplement runtime tests with source-level authority, ordering, or forbidden-call contracts.
# Source-integrity exact-count helper

- Candidate: a small cross-project script that counts unique source/XAML symbols with fixed-string semantics, performs strict UTF-8 and replacement-character checks, and parses XAML without embedding quoted attributes in nested shell regexes.
- Trigger: repeated PowerShell/JavaScript/`rg` quoting mistakes while verifying AutomationId and click-hook counts.
- Expected benefit: one stable verification command with structured output, avoiding false negatives and parser-specific escaping mistakes.
- 2026-07-18 project promotion: `.omx/verify-source-integrity.ps1` now covers strict UTF-8, U+FFFD, and XAML parsing. The cross-project candidate remains open for declarative fixed-string count assertions.

# Candidate - Windows PowerShell 5.1 safe package manifests

- Trigger: a Windows desktop repository needs a reproducible local package before formal installer/signing infrastructure exists.
- Workflow: keep executable script source ASCII-only; copy localized UTF-8 documentation as bytes; use .NET Framework-compatible APIs; publish into a new confined directory; refuse replacement; verify required payloads; record SHA-256 and Authenticode truth; derive a fail-closed mutation-readiness state; archive without launching or signing.
- Evidence: OMNIX live packaging caught UTF-8 source decoding, `Path.GetRelativePath`, and `Path.IsPathFullyQualified` incompatibilities that static default-path review missed.
- Reuse: WPF/WinForms utilities, privileged helper pairs, portable QA builds, and early release pipelines targeting stock Windows PowerShell.

# Candidate - Preserve typed booleans across Windows PowerShell process boundaries

- Trigger: a script is documented through `powershell.exe -File` and needs explicit true/false safety declarations.
- Workflow: accept an allowlisted textual value or a switch at the native argument boundary, validate it before side effects, then serialize a genuine Boolean into the persisted record; test the documented Windows PowerShell 5.1 invocation rather than only same-process calls.
- Evidence: OMNIX behavioral-acceptance setup initially used `[bool]` parameters even though native arguments arrive as text and Windows PowerShell 5.1 does not reliably bind explicit Boolean values through `-File`.
- Reuse: release tooling, deployment scripts, audit/attestation workflows, and any fail-closed Windows automation that must distinguish an explicit false from an omitted value.

# Candidate - Ownership-marked disposable acceptance fixtures

- Trigger: a desktop product needs realistic destructive-workflow acceptance without touching personal applications, directories, startup entries, or registry state.
- Workflow: derive every object from a canonical session id; preflight all filesystem/registry collisions before mutation; persist ownership markers; register compensation before creation; use exact no-follow reset; publish the harness separately from the product; bind both package manifests into the acceptance receipt.
- Evidence: OMNIX fixture fault injection caught an orphan root, and integration through real inventory/recommendation builders caught cache and nested-temp layouts that local fixture assertions would have accepted.
- Reuse: uninstallers, cleaners, migration assistants, startup managers, backup/restore clients, update agents, and any Windows tool whose final evidence must come from a disposable environment.

# Candidate - Read-only code-signing prerequisite report

- Trigger: a Windows release pipeline needs a real external signer but must not create/import certificates, change trust, install SDKs, or choose credentials for the user.
- Workflow: inspect an explicit tool path or bounded SDK locations; read CurrentUser certificate metadata; parse EKU from host-independent X509 extensions; list all eligible candidates; keep explicit signer selection in the separate signing command; emit JSON missing requirements; runtime-test the oldest supported PowerShell host with zero candidates.
- Evidence: OMNIX static tests missed provider-specific EKU and empty-pipeline behavior; a real Windows PowerShell child test exposed both and now keeps no-certificate readiness stable.
- Reuse: desktop signing, update systems, enterprise packaging, CI handoff, and any local release flow where discovery must remain separate from credential authorization.
