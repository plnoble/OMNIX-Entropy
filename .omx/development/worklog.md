# Development Worklog

## 2026-07-23 - First valid personal installer completed

- After separate explicit consent, imported only public certificate `5688958FEA0056861558E8DCF9D2381AF46074B2` into CurrentUser Root. Independent inspection found the private key only in CurrentUser My, public copies in the three authorized CurrentUser trust stores, and zero matches in inspected LocalMachine stores.
- A fresh 110-file App/worker candidate passed independent hash, Authenticode, timestamp, RSA, production-surface, and same-signer verification.
- The first installer compile exposed that standard Inno 6.7.3 omits the unofficial Simplified Chinese translation. Pinned the official `jrsoftware/issrc` `is-6_7_3` file in the repository with source/hash notice, then rebuilt from a new output.
- Final `OMNIX-Entropy-0.1.0-win-x64-setup.exe` is 14,812,824 bytes, SHA-256 `5680C3847F23291784BB38FB1D01FACAFC6013DC47F06B611C170BCDC63955BE`, valid same-signer, timestamped, D-first, directory-selectable, and silent-install-disabled. The installer was not launched or uploaded.
- Made signer initialization reproducible as two explicit approvals: publisher trust first, then an optional thumbprint-bound CurrentUser Root attestation. Runtime rerun was idempotent (`Created=False`, `RootStoreModified=False`, `RootTrusted=True`).
- Verification: focused 10/10; full 1054/1054; Release 0 errors; integrity 380/18; script parser pass; 457 public candidates/about 6.3 MB with zero binary or signing-material candidates.
- Pushed source commit `4aba92d`; GitHub CI run `30019958502` passed restore, production build, full suite, and source integrity in 2m52s.

## 2026-07-23 - Inno installed; signer trust awaiting explicit approval

- Winget verified and installed `JRSoftware.InnoSetup` 6.7.3 to `D:\Development\Inno Setup 6`; the selected `ISCC.exe` has valid Authenticode. An older 6.7.1 registration was observed and left untouched.
- Added a guarded CurrentUser-only personal signer initializer: exact attestation, RSA 3072, non-exportable private key, public CER evidence under ignored artifacts, no PFX/Root/LocalMachine/remove authority. Focused parser/contracts passed 5/5.
- Initial execution was rejected before any certificate was created because TrustedPeople/TrustedPublisher blast-radius consent was not explicit enough. No retry or workaround was attempted.

## 2026-07-23 - D-first personal installer foundation

- Added an Inno Setup definition with a visible directory page, default `D:\Software\OMNIX-Entropy\Install`, lowest-privilege per-user setup, x64-compatible architecture, signed uninstaller, and explicit silent-mode refusal.
- Added a bounded local builder that first runs the signed-candidate verifier, requires explicit ISCC/SignTool/certificate/timestamp inputs, enforces the App/worker signer, and emits a signed setup plus installer manifest.
- Added a separate read-only installer verifier for fixed local path, reparse/extra-file refusal, D-first/silent policy, length/hash, Authenticode, timestamp, and source-package signer matching.
- Changed personal GitHub release staging and channel metadata from portable ZIP to verified setup EXE plus installer manifest; staging rechecks the copied installer hash before any draft operation.
- Verification: focused 19/19; full 1051/1051; Release build 0 errors; integrity 379 files and 18/18 XAML; PowerShell parser pass. Read-only machine check found SignTool but no Inno compiler and no eligible signer, so no installer was compiled or published.

## 2026-07-22 - First public CI remediation

- Inspected failed GitHub Actions run `29932623250`: 31 failures reduced to missing ignored smoke scripts, CRLF source-text drift, absent pre-test Release output, and hosted-runner pipe contention.
- Audited all 22 newly public top-level `.omx` smoke/helper scripts; only neutral `Fixture` paths matched the privacy scan and no credential, real username, or installed-Marvis path was found.
- Added tracked smoke-script allowlisting, LF normalization, current commit-pinned Actions v5 identities, Release-before-test ordering, and deterministic xUnit scheduling.
- Local verification: repository contracts 4/4; full Debug 1048/1048; Release build succeeded; source integrity 378 files, invalid UTF-8/replacement 0, XAML 18/18.
- Committed as `06534d4`; a tracked-only archive independently passed Release build, full 1048/1048, and integrity 378/18. Pushed `main`; replacement GitHub Actions run `29933681994` passed all steps in 2m59s with full 1048/1048 and matching integrity evidence.

## 2026-07-22 - GitHub personal release foundation and read-only update check

- Confirmed the target `plnoble/OMNIX-Entropy` repository is public and empty, connected it as `origin`, and audited the first-publication candidate set before staging.
- Fixed the unanchored `quarantine/` ignore rule that hid real quarantine source directories; restricted `.omx` publication to development records and the source-integrity gate; ignored machine QA evidence and all common private signing material.
- Added commit-pinned, read-only GitHub CI with Debug full tests, a separate Release build, and source-integrity verification. Release tests were corrected after proving the fake worker command is intentionally absent from Release binaries.
- Added a local personal-release publisher that accepts only a verified same-signer candidate, produces a fixed-repository channel manifest and SHA-256 list, requires a committed revision, refuses existing tags, and can create only a draft GitHub Release.
- Added a compact, user-triggered update dialog and bounded fixed-repository GitHub client. It validates release/tag/asset URLs and channel identity without downloading or installing a package.
- Rejected Velopack for the install slice because its standard Windows path is `%LocalAppData%`; OMNIX needs a selectable D-drive-first installer.
- Verified 13/13 focused tests, 1048/1048 full Debug tests, Release 0 warnings/errors, 377 strict source files, 18/18 XAML, script parsing, and a 424-file public candidate inventory with no binaries, signing secrets, or real username.
- Computer Use launch timed out and no OMNIX window appeared; visual evidence is Warn with no antivirus/security bypass.

## 2026-07-22 - Non-default Windows SDK discovery corrected

- Fresh blocker inspection noticed a Windows SDK app under D drive, then bounded reads found seven SignTool binaries and both standard `KitsRoot10` values pointing to `D:\Windows Kits\10\`.
- Added exact read-only registry-root discovery and retained bounded direct-child architecture probing; documentation and completion audit now reflect that SignTool is present.
- Final inspector resolves the newest x64 SignTool and reports only the missing RSA code-signing certificate. No SDK, registry, certificate, trust, signing, or process mutation occurred.
- Verification: TDD red; inspector 4/4; related 8/8; full 1035/1035; Release 0 warnings/errors; integrity 370 files/17 XAML; parser/non-ASCII both 0.

## 2026-07-22 - V1 completion audit closed the recent-install gap

- Re-read the original V1 requirements against current MainWindow entries, shared models, operation handlers, and focused test groups instead of relying on the inherited completion claim.
- Found the Application Management plan required recent-install sorting, while the UI implemented only risk, size, growth, and name. Added strict registry date capture, profile/enrichment propagation, recent-install ordering, stable UIA identity, and technical evidence.
- Added the V1 completion audit with explicit local/visual/external evidence levels. Computer Use again timed out launching the unsigned Debug app; no window/process remained and no bypass was attempted.
- Verification: TDD red; related 209/209; critical workflow group 337/337; focused audit group 205/205; full 1035/1035; Release 0 warnings/errors; integrity 370 files and 17/17 XAML; Agent mutation-authority hits 0.

## 2026-07-22 - Signing prerequisite guide and release entry point completed

- Audited current Microsoft guidance and found the release pipeline also needed to enforce the target Smart App Control RSA compatibility boundary.
- Added RSA filtering/refusal to the read-only prerequisite inspector, signed-package transform, manifest, and transfer verifier.
- Added the beginner Chinese signing guide and a short release index that connects inspection, signing, transfer verification, and disposable acceptance without automating certificate or trust changes.
- Verification: related 15/15, full 1033/1033, Release 0 warnings/errors, source integrity 369 files and 17/17 XAML. Final read-only inspection still reports no SignTool and zero eligible RSA signers.

## 2026-07-19 - Release-candidate transfer verifier completed

- Added a read-only verifier for a signed candidate after transfer to the disposable machine; it does not trust creation-time output alone.
- It requires a fully qualified fixed-local non-reparse package, rechecks awaiting-candidate manifest state, verifies every listed length/hash and rejects unlisted payloads, requires critical payload coverage, revalidates App/worker signatures/timestamps/same manifest thumbprint, and rescans the Release worker for the Debug-only command token.
- Success can only emit `CanBeginDisposableAcceptance=true`, `DisposableMachineAcceptance=false`, and `AwaitingBehavioralAcceptance`; the script contains no process launch, package write, certificate change, or system mutation authority.
- Verification: TDD red 0/4, focused 4/4, related release pipeline 14/14, full 994/994, Release 0 warnings/errors, 360 strict UTF-8 C#/XAML files, 17/17 XAML, parser valid. Current unsigned package was correctly refused. No product process or mutation ran.

## 2026-07-19 - Trusted signed-release transformation completed

- Added `publish-signed-release-package.ps1` as a separate transform over an already verified `ProductionOnly` portable artifact; the existing unsigned/read-only package script remains unable to sign or import certificates.
- The transform verifies every source-manifest hash, requires the four critical payloads to be covered, rejects reparse paths/content and debug worker metadata, and writes only to a new direct child of `.artifacts`.
- It accepts only an explicit 40-hex thumbprint from `Cert:\CurrentUser\My`, requires private key/current validity/code-signing EKU, invokes an explicitly supplied `signtool.exe` with SHA-256 and HTTPS RFC3161 timestamping, then rechecks both executables as `Valid`, same requested thumbprint, and timestamped before hashing and manifest creation.
- The generated truth remains `EligibleForDisposableMachineAcceptance`, `DisposableMachineAcceptance=false`, and `AwaitingDisposableMachineAcceptance`; signing alone is not production acceptance.
- Verification: TDD red 0/4, focused 4/4, related 10/10, full 990/990, Release 0 warnings/errors, 359 strict UTF-8 C#/XAML files, 17/17 XAML, PowerShell syntax valid. Missing-sign-tool runtime refusal created no output. No certificate enumeration/change or signing ran.

## 2026-07-19 - Execution result return handoff completed

- Found that migration and official-uninstall results returned to an exhausted plan window; MainWindow's existing current-state rescan did not start until the user closed that second window.
- Added explicit `返回并重新检查` result wording for application-return contexts and close the plan immediately after any acknowledged production attempt result. Preview-only close remains unchanged; typed uninstall post-scan actions retain their existing branches.
- Preserved truthful standalone Debug behavior: the independently hosted worker-connection result still says `我知道了` because no Application Management page exists there.
- Verification: TDD red 0/3, focused 3/3, related 207/207, full 986/986, Release 0 warnings/errors, 358 strict UTF-8 C#/XAML files, 17/17 XAML. Final package/ZIP `.artifacts/OMNIX-Entropy-test-20260719-003423`; App/worker `NotSigned`, mutation blocked.

## 2026-07-19 - Cache and startup decision outcomes accepted

- Exercised both application-drawer entries in the isolated one-app Release fixture without invoking either offered next action.
- Cache cleanup failed closed because its fixture cache did not pass current local validation. The first result stated that evidence was insufficient, explained why, promised no file move/delete, and exposed no primary action.
- Startup management found only name-level evidence. The first result refused to guess, explained that OMNIX would not modify registry/services/tasks, and exposed only the bounded Windows Startup Apps handoff.
- No source change was needed. This acceptance adds runtime evidence to the inherited 983/983, zero-warning Release, and integrity baseline for package `.artifacts/OMNIX-Entropy-test-20260719-000736`. Neither handoff nor any mutation was invoked, and the package window was closed.

## 2026-07-16 - Persistent later install observation completed with visual Warn

- Added a default-collapsed `安装界面都关了，重新扫描` action before advanced diagnostics. It becomes visible only after a non-refused trusted launch result and remains available for later bootstrap/child-installer completion during the current app session.
- Retained the exact automatic before snapshot and exit-code observation separately from manual diagnostics. Starting another prepare attempt or changing the installer path revokes and hides the old session before any new work.
- Extracted `InstallerPostScanCoordinator`, which owns one software read, one C-drive footprint read, and one diff build but no package inspector, launcher, operation pipeline, process API, or mutation authority. The execution coordinator delegates its existing post-scan compatibility method to it.
- Centralized result-window retry, report rendering, and application-catalog synchronization in `PresentInstallerExecutionResultsAsync`; both immediate recovery and later page rescan use the same truthful preliminary-result flow.
- Corrected two source contracts that had become falsely green because a newly inserted helper fell inside an old broad method slice. Verification passed: focused 25/25, related 243/243, full 927/927, build 0 warnings/errors, 332 strict UTF-8 files, strict XAML parse, stable button placement/id, and zero read-only-handler launch/mutation hits. Visual proof remains Warn after the already-recorded Computer Use timeout.

## 2026-07-16 - Persistent later install observation started

- Audited the completed initial-report branch. Its safety text correctly warns that bootstrap/child installer work may continue, but the only later comparison controls remain inside advanced manual diagnostics.
- Chose a session-only primary action that appears only after a non-refused trusted launch result and reuses the exact automatic before snapshot. Selecting or typing a different installer clears the session and hides the action.
- The persistent handler will use a dedicated read-only post-scan coordinator with no launcher or operation pipeline, then reuse the same result/report/application-catalog presentation. No baseline will be persisted across app restarts in this slice.

## 2026-07-16 - Beginner-safe installer post-scan recovery completed with visual Warn

- Added typed result-window retry availability for `InstallerWaitInterrupted` and `PostScanFailed` only, plus a stable `InstallerExecutionResultPostScanRetryButton` labeled `我已完成安装，重新扫描`.
- Extracted the existing initial post-scan into `CapturePostInstallSnapshotAsync`. Initial observation and each explicit retry now share one software/footprint read and one report builder; the read-only method contains no launcher, pipeline, registry, or file mutation authority.
- MainWindow keeps the original before snapshot, shows one result per attempt, scans again only after a button click, and reuses the existing verified report/catalog binding. Failed retries remain retryable and path-free; launch refusal and completed initial scans expose no retry command.
- Verification passed: focused 23/23, related 241/241, full 925/925, build 0 warnings/errors, 332 strict UTF-8 files, strict XAML parse, unique retry/close AutomationIds, and static authority/count checks. Computer Use reached Windows but the Debug-app launch timed out; no window appeared on the passive poll, so no screenshot is claimed.

## 2026-07-16 - Beginner-safe installer post-scan recovery started

- Audited interrupted-wait and failed-post-scan results. Both tell the beginner to capture an after snapshot, but the only matching control is hidden inside the advanced manual-comparison expander.
- Chose one result-window command, `我已完成安装，重新扫描`, shown only for those two uncertain states. It will run one read-only software/footprint scan against the original before snapshot and may be requested again only by another explicit click.
- The retry must not relaunch the installer, invoke an operation pipeline, infer success from exit code, or expose raw scan errors. A valid report will reuse the existing catalog/report binding.

## 2026-07-16 - Post-install inventory reuse completed

- Added a source contract that first failed only because MainWindow did not bind the verified after snapshot to the application catalog.
- In the existing valid snapshot-plus-report branch, reused `execution.AfterSnapshot.SoftwareProfiles` through `SetSoftwareProfiles` before report presentation. No duplicate scan, installer relaunch, or new mutation authority was added.
- Verification passed: focused 16/16, related installer/report/product tests 232/232, full 918/918, build 0 warnings/errors, 332 strict UTF-8 files, correct static ordering, and zero process/registry/file mutation authority in the changed method.

## 2026-07-16 - Post-install inventory reuse started

- Traced installer preparation through before snapshot, final consent, pipeline launch, process wait, after software/C-drive scan, placement attribution, and beginner report.
- Confirmed the coordinator returns the complete after software snapshot and MainWindow stores/displays its report, but `_softwareProfiles` is not updated. A newly installed application can therefore be visible in the install report while absent from Application Management until another scan.
- Chose zero-extra-scan synchronization: only when both after snapshot and report exist, pass that exact profile list to `SetSoftwareProfiles` before rendering the report. Interrupted/refused/failed results remain unchanged.

## 2026-07-16 - Migration post-attempt state synchronization completed

- Added a contract requiring the MainWindow migration method to place one inventory scan and one closure refresh inside the `ProductionExecutionAttempted` branch before evaluating authenticated completion.
- Restructured post-window orchestration accordingly. Accepted completion retains the existing success/closure copy; every other attempted outcome preserves its result title, reports which read-only evidence was refreshed, and explicitly refuses automatic continuation.
- Verification passed: focused 7/7, related migration/app-action/product tests 256/256, full 917/917, build 0 warnings/errors, 332 strict UTF-8 files, correct static ordering, and zero process/registry/file/pipeline authority in the changed method.

## 2026-07-16 - Migration post-attempt state synchronization started

- Traced migration plan execution through final consent, trusted coordinator, authenticated worker response, MainWindow inventory refresh, and closure-store refresh.
- Confirmed accepted completion is classified correctly, but MainWindow performs both read-only refreshes only for `ProductionCompleted`; timeout, transport failure, refusal, or a partial/uncertain worker outcome can leave application location and closure evidence stale.
- Chose the same conservative synchronization rule as uninstall: after any production attempt, observe current inventory and closure exactly once. Completion copy remains gated on the coordinator's authenticated accepted outcome; all other outcomes stop after observation.

## 2026-07-16 - Official uninstall post-attempt inventory refresh completed

- Added a source contract proving every `ProductionExecutionAttempted` refreshes current application inventory once and that local residue review additionally requires completed production plus an explicit residue recommendation.
- Changed only MainWindow post-window orchestration. Unknown, failed, canceled, or incomplete worker outcomes now update the application list but never create or execute residue cleanup; validated completion retains the existing review/quarantine flow.
- Verification passed: focused 6/6, related official-uninstall/residue/evidence/product tests 387/387, full 916/916, build 0 warnings/errors, 332 strict UTF-8 C#/XAML files, and zero process/registry/file/pipeline authority in the changed method.

## 2026-07-16 - Official uninstall post-attempt inventory refresh started

- Traced `卸载干净点` through drawer guards, recovery/snapshot evidence, final visual consent, trusted signed worker launch, authenticated transport, elevated operation pipeline, worker post-scan, local residue review, quarantine, timeline, and restore.
- Confirmed those gates are connected for valid completion. The first gap is MainWindow refreshing `_softwareProfiles` only when `ProductionCompleted` is true; a timeout, transport failure, worker-exit failure, or incomplete response may follow partial system change while leaving the UI inventory stale.
- Chose a read-only synchronization fix: after any `ProductionExecutionAttempted`, scan current applications once. Enter residue review only when production completed and the validated post-scan explicitly recommends it; every uncertain outcome updates inventory but performs no residue action.

## 2026-07-16 - Startup restore pipeline completed with visual Warn

- Added typed startup restore evidence, preparation, verification, outcome, and handler. Preparation reloads the current row and binds manifest path/hash/id, state fingerprint, and the one supported current-user Run locator.
- Handler rechecks the row and manifest after confirmation, then delegates registry mutation to the existing exact store, which still refuses same-name overwrite, ACL drift, StartupApproved drift, and invalid snapshots. The handler owns conservative same-row timeline updates.
- MainWindow now prepares before showing the existing path-free confirmation and executes only through `SafetyOperationPipeline`. The old public `StartupEntryControlOperationHandler.RestoreAsync(manifestPath)` bypass was removed.
- Seven disposable-fixture tests cover preparation, explicit confirmation, successful restore, manifest tamper, stale timeline state, mismatched registry scope, failed mutation state, and WPF wiring.
- Verification passed: focused 7/7, related 204/204, full 915/915, build 0 warnings/errors, 332 strict UTF-8 C#/XAML files, zero direct MainWindow restore/timeline-update calls, and unique confirmation AutomationIds. No real registry mutation or screenshot is claimed.

## 2026-07-16 - Startup restore pipeline closure started

- Traced startup timeline restore through MainWindow, manifest verification, the typed startup store, and registry/fixture backends.
- Confirmed the real store already refuses same-name overwrite, ACL drift, StartupApproved drift, and invalid snapshots; the remaining gap is that MainWindow trusts the cached timeline manifest path, calls `RestoreAsync` directly, and updates the journal itself.
- Chose one typed restore policy/handler: reload the current row by id, require one supported registry locator and one verified manifest, bind manifest id/hash plus state fingerprint, reconfirm, revalidate in the handler, delegate the exact mutation to the existing store, and update the same timeline row conservatively.
- Scope remains current-user 64-bit Run only. Tests will use disposable in-memory fixtures and temporary manifests/databases; no real registry mutation is part of this slice.

## 2026-07-16 - Ordinary quarantine restore pipeline completed with visual Warn

- Audited low-risk C-drive cleanup from recommendation selection through quarantine, timeline persistence, confirmation, and restore. Cleanup was already pipeline-bound; ordinary restore still called the quarantine service directly from MainWindow and trusted stale timeline/manifest data.
- Added timeline load-by-id, typed restore evidence/policy/outcome, manifest SHA-256 and payload identity binding, preflight revalidation, and a pipeline handler that owns restore plus conservative timeline-state updates.
- MainWindow now prepares from the current timeline row, shows the existing path-free confirmation, confirms the descriptor, and executes only through `SafetyOperationPipeline`. Startup restore remains a separate explicit branch.
- Added six tests for preparation, explicit confirmation, successful restore/journal update, payload change, manifest change, stale timeline state, and static WPF wiring. One related source-contract test still targeted the removed combined restore method; its boundary and helper were corrected and the miss was recorded.
- Verification passed: focused 6/6, related 227/227, full 908/908, solution build 0 warnings/errors, 330 strict UTF-8 C#/XAML files, zero direct MainWindow quarantine restore calls, and unique restore-confirmation AutomationIds. Real WPF proof remains Warn.

## 2026-07-16 - Quarantine restore pipeline closure started

- Traced the low-risk cleanup chain end to end. Selection, low-risk policy, candidate identity, final confirmation, quarantine pipeline, timeline persistence, and restore confirmation are connected.
- Confirmed ordinary quarantine restore still loops over `_quarantineService.RestoreAsync` directly and updates `_timelineStore` directly, bypassing `SafetyOperationPipeline`.
- Confirmed no restore preparation binds the current timeline entry, manifest content, or quarantined payload identity before the user confirms.
- Chose a typed restore preparation and handler: load current entry by id, inspect each manifest, bind quarantined payload identity, confirm a bounded path-free operation, revalidate everything inside the handler, then update timeline state from actual results.
- Startup restore is intentionally outside this slice so its registry-specific manifest and control policy remain unchanged until a separate audit.

## 2026-07-16 - Background application ownership summary started

- Confirmed `AppCatalogSummaryPresenter` counts running/startup/service/task applications but presents only raw totals, while Agent already separates ordinary versus read-only resident profiles.
- Chose one compact existing-control sentence: ownership counts first, then overlapping signal-type counts. No new card, panel, or action is added.
- The new read-only background catalog will feed both the app summary and Agent resident lists so first-view and conversational conclusions cannot drift.
- Added `BackgroundApplicationOwnershipCatalog` and a shared ownership-summary formatter reused by both C-drive and background catalogs.
- The application summary now reports distinct resident applications by ownership before listing overlapping running/startup/service/task app counts; Agent resident lists reuse the catalog.
- Verified focused 3/3, related 240/240, full 902/902, build 0 warnings/errors, 328 strict UTF-8 C#/XAML files, zero replacement characters, zero focused mutation-authority hits, zero old projection/summary patterns, two shared bindings, and one existing summary AutomationId.

## 2026-07-16 - C-drive application ownership summaries started

- Confirmed `HealthDigestBuilder` collapses every C-drive footprint into `观察到写入 C 盘的应用 X 个`, while `AppCatalogSummaryPresenter` exposes only a raw total.
- Chose to keep both existing first-view controls and total/filter membership, but split ordinary, explicit system, and managed-root ownership-pending evidence through one typed read-only catalog.
- The same catalog will feed `AgentActionCandidateCatalog` so aggregate Agent and first-view counts cannot drift. No new panel or action is added.
- Added `CDriveApplicationOwnershipCatalog` with exhaustive ordinary/system/ownership-pending groups, stable source ordering, and one path-free beginner summary.
- Bound `AppCatalogSummaryPresenter`, `HealthDigestBuilder`, and `AgentActionCandidateCatalog` to the catalog. Existing total/filter membership and the two existing summary controls remain unchanged.
- Verified focused 3/3, related 238/238, full 899/899, build 0 warnings/errors, 325 strict UTF-8 C#/XAML files, zero replacement characters, zero focused mutation-authority hits, zero old total-only/Agent projection patterns, three shared bindings, and one instance of each existing summary AutomationId.

## 2026-07-16 - Health risk and ownership wording started

- Confirmed C-drive Agent evidence and stored health digests count every `RecommendationAction.Clean` as low-risk without checking `RiskLevel`.
- Confirmed health key-finding text says `建议确认后清理` for medium/high clean recommendations even though the explanation layer later refuses direct handling.
- Confirmed the startup health dimension excludes only explicit `SystemTool`; managed-root ownership-pending profiles are counted as ordinary and a protected-only result can be rated `未发现`.
- Chose one shared action-plus-risk predicate and three-way startup grouping: ordinary, explicit system, and ownership pending.
- Added `HealthFindingRiskPolicy` and connected C-drive Agent answers, stored digests, reclaimable totals, finding action text, and startup dimension grouping to the shared policy.
- Added a high-risk-only regression: when no low-risk cleanup exists, the Agent says to observe and prepare snapshot/rollback evidence instead of offering quarantine handling.
- Verified focused 5/5, related 244/244, full 896/896, build 0 warnings/errors, 323 strict UTF-8 C#/XAML files, zero replacement characters, zero focused mutation-authority hits, and zero legacy action-only/category-only patterns.
- Kept real WPF status at Warn because the antivirus-updated Computer Use launch already timed out earlier in the turn; no fallback automation was used.

## 2026-07-16 - Agent aggregate action authority started

- Confirmed `AgentNextStepPresenter` counts every C-drive footprint together and can tell beginners to look for migration/cache actions even when every clue belongs to a protected profile.
- Confirmed general migration and uninstall replies use raw footprint/command presence; a D-installed app with C-data clues becomes a migration candidate and system/ownership-pending commands inflate the uninstall count.
- Confirmed aggregate startup review uses supported observation shape but lacks the managed-root ownership deny already enforced by the drawer.
- Chose one shared read-only projection that separates ordinary migration review, C-data-only review, protected C-drive evidence, uninstall review, startup review, and protected startup evidence.
- Added `CanUseOrdinaryApplicationActions` and public `CanReviewMigration` as the shared drawer-derived availability boundary; migration action binding now uses the same helper.
- Added `AgentActionCandidateCatalog` and connected homepage next steps plus C-drive, applications, migration, uninstall, and startup aggregate replies.
- Extended the same boundary to exact startup wording, Windows settings handoff, background review cards, and startup/service plan counts so ownership-pending profiles remain read-only everywhere.
- Verified focused 5/5, related 234/234, full 891/891, build 0 warnings/errors, 321 strict UTF-8 C#/XAML files, zero focused authority hits, and zero legacy raw candidate-count patterns.

## 2026-07-16 - Homepage migration closure authority started

- Confirmed `MigrationClosureHealthEnricher` assigns `RecommendationAction.Migrate` to every abnormal historical record and MainWindow supplies only a unique-name predicate.
- Confirmed migration findings without an app target fall through to C-drive navigation while their special Agent copy still says to open the corresponding app and generate a migration plan.
- Chose a typed target disposition: reviewable, protected historical, or unavailable/ambiguous. Only reviewable records may carry `Migrate` and an exact app target; all evidence remains visible and read-only otherwise.
- Added current-profile resolution through `AppDrawerTargetResolver` plus `CanReviewMigrationClosure`; protected and ambiguous records now project as path-free read-only history with no target.
- Split migration-specific Agent explanation, action-plan, and navigation copy so read-only history opens Applications generically and never claims that a migration plan will be generated.
- Verified focused 4/4, related 193/193, full 886/886, build 0 warnings/errors, 319 strict UTF-8 C#/XAML files, and zero focused mutation-authority hits.
- Retried the real Debug app after the user confirmed antivirus definitions were updated. Computer Use app/window discovery succeeded, but `launch_app` timed out and no OMNIX window appeared on a passive poll; visual status remains Warn without a fallback automation path.

## 2026-07-15 - Migration closure tile and catalog safety started

- Confirmed private `AppTileUi.From` replaces even protected tile status with red `迁移未闭环`, and catalog sorting/summary treat every matched historical record as actionable.
- Chose two typed Core projections: one preserves base tile authority unless a closure is reviewable; the other separates reviewable ordinary closure records from protected historical records.
- Drawer secondary evidence remains intact; this slice changes only beginner grid, priority, and aggregate wording.
- Added typed tile state and catalog summary presenters. Protected profiles retain base status and are counted only as `仅供查看` historical records; ordinary incomplete closures remain red and prioritized.
- MainWindow now delegates tile state, sort priority, and summary copy to Core; the previous raw `NeedsAttention` overrides are absent.
- Updated the older migration experience wiring assertion to the new tile priority authority.
- Verified focused 4/4, related 191/191, full 882/882, build 0 warnings/errors, 318 strict UTF-8 C#/XAML files, zero focused authority hits, and zero legacy closure override hits.
- Retried real WPF verification after the antivirus database update using Computer Use only. The helper answered app/window discovery, but launching the built `Css.App.exe` timed out; a follow-up app/window poll found no OMNIX window, so visual status remains Warn and no PowerShell UIAutomation/SendKeys fallback was used.

## 2026-07-15 - Central app action entry guards started

- Confirmed uninstall, cache cleanup, and startup control central methods currently trust disabled buttons/upstream Agent routing and do not recheck their drawer action before operation-specific work.
- Chose one typed fail-closed entry decision reused by all three methods; each guard must run before restore-point reads, plan construction, startup preparation, or pending target assignment.
- Migration and residue already have distinct specialized guards and remain outside this generic slice.
- Added `AppActionEntryPolicy` as a fail-closed projection of the drawer action model and a path-free uninstall refusal state.
- `ShowUninstallPlanAsync`, `ShowCacheCleanupPreview`, and `ShowStartupControlPreviewAsync` now deny before operation-specific evidence reads, plan/preparation work, windows, and pending target assignment.
- Verified focused 2/2, related 409/409, full 878/878, build 0 warnings/errors, 317 strict UTF-8 C#/XAML files, zero focused authority hits, and exactly three centralized guard bindings.

## 2026-07-15 - Migration closure permission consistency started

- Confirmed a `NeedsAttention` closure unconditionally sets `DrawerMigrateButton.IsEnabled = true` after the shared action policy, including for system and managed-root ownership-pending profiles.
- Confirmed the same closure replaces the protected-profile Agent conclusion instead of presenting stale migration evidence as secondary context.
- Chose a typed combined drawer state: current ownership policy controls plan availability; ordinary closure review remains useful for D-installed apps; the plan entry rechecks the same state.
- Added `MigrationClosureDrawerStatePresenter`: protected-profile Agent advice remains first, stale closure evidence stays visible as secondary context, and ordinary D-installed apps can still open a fresh review when the old migration did not close.
- Replaced the unconditional WPF button override with typed text/enabled/reason binding and added the same fail-closed check before `MigrationPlanWindow` construction.
- Updated the older migration experience wiring test to assert the new Core presenter instead of direct MainWindow string composition.
- Verified focused 3/3, related 250/250, full 876/876, build 0 warnings/errors, 316 strict UTF-8 C#/XAML files, zero focused authority hits, and zero unconditional migration-button override hits.

## 2026-07-15 - Uninstall residue review availability started

- Confirmed `ShowAppDrawer` enables `DrawerResidueReviewButton` for every selected profile and the async `finally` restores it from selection presence alone.
- Chose a distinct residue-review policy: ordinary profiles remain eligible without an uninstall command so an external uninstall can be reviewed; system-category and managed-root ownership-pending profiles remain read-only.
- Scope is presentation availability plus a handler-level guard. Residue candidate classification, quarantine planning, confirmation, pipeline, and execution authority remain unchanged.
- Added a typed `UninstallResidueReview` availability to the drawer. It deliberately does not depend on an uninstall command, so external-uninstall recovery remains available for ordinary applications.
- Bound the drawer button to that policy, denied protected profiles again at the review handler boundary, and made async restoration re-resolve the current selected profile instead of checking selection presence alone.
- Verified focused 3/3, related 200/200, full 873/873, build 0 warnings/errors, and 315 strict UTF-8 C#/XAML files; focused authority search returned zero hits.

## 2026-07-15 - App drawer stale-state invalidation started

- Confirmed `ApplyDrawerActionHost` already invalidates cache/startup pending operation and target fields whenever collapsed state is applied.
- Found that zero-profile `RefreshAppCatalog` returns without clearing the drawer; `ClearAppDrawer` omits category summary, selected tile, technical visibility/button text, and some no-selection affordance text.
- Chose one typed empty-drawer presentation plus an explicit technical-details collapsed state, reused by normal open and empty clear paths.
- Added `AppDrawerEmptyStatePresenter` and `AppDrawerTechnicalDetailsPresenter.Collapsed`; zero-profile and zero-filter branches now converge on `ClearAppDrawer`.
- Clear state deselects the tile, clears the new category line and technical list, collapses the preview host (which invalidates all three pending fields), resets migration/technical button copy, disables context buttons, and sets no-selection tooltips.
- Added stable `DrawerTechnicalDetailsButton` AutomationId; opening any normal drawer also restores collapsed technical details without overwriting status text.
- Verification completed: focused 4/4, related 206/206, full 870/870, build 0 warnings/errors, 314 strict UTF-8 files, focused authority hits 0, technical button AutomationId hit 1, and zero-profile clear call hit 1.

## 2026-07-15 - C-drive catalog summary consistency started

- Confirmed `BuildSoftwareSummary` counts `CDriveWritePaths.Count > 0`, while `AppCatalogFilter.CDrive` uses `HasCDriveFootprint`, which also checks a C-installed main program.
- Chose a structured summary with separate C-main, D-main, C-data/cache-app, deduplicated footprint, visible, running, startup, service, and task counts.
- The shared footprint predicate will validate actual C paths; C data/cache counts will reuse canonical install-root exclusion so install files are not relabeled as separate data.
- The first broad implementation patch was atomically rejected because its class-boundary anchor assumed a block-method closing shape; no source change occurred, and implementation continued with exact local patches.
- The first green run passed both behavior tests but a whole-MainWindow absence assertion found an unrelated legacy technical view; the assertion was narrowed to `RefreshAppCatalog` and the mistake was recorded.
- Added `AppCatalogSummaryPresenter` with structured totals and compact path-free text; MainWindow now uses it instead of a private counter.
- `HasCDriveFootprint` now requires a real C main-program path or canonical C data/cache location outside the install root; `CDriveDataLocationCount` is shared by drawer explanations and summary counts.
- Added stable `AppsSummaryTextBlock` AutomationId and static proof that the conclusion precedes the app grid.
- Verification completed: focused 3/3, related 232/232, full 867/867, build 0 warnings/errors, 312 strict UTF-8 files, focused authority hits 0, legacy summary method hits 0, and summary AutomationId hit 1.

## 2026-07-15 - Uninstallable catalog safety consistency started

- Confirmed the catalog uses only non-empty uninstall command while the drawer first denies system-category and managed-root ownership-pending profiles.
- Chose a shared read-only `CanReviewUninstall` policy. It means the drawer review is available, not that signer trust, final consent, or production execution is ready.
- Initial red proved system and managed-root profiles leaked into the filter. After centralizing the predicate, a remaining focused failure showed the disabled system drawer still carried generic ordinary uninstall preview lines; scope was widened only to make that preview refuse consistently.
- Added `CanReviewUninstall` and reused it in both `AppCatalogFilter.Uninstallable` and the ordinary drawer action. System and ownership-pending branches remain explicit denies.
- System-category uninstall preview now states that no ordinary uninstall plan will be generated and only technical details remain available.
- Verification completed: focused 7/7, related 188/188, full 864/864, build 0 warnings/errors, 311 strict UTF-8 files, and focused authority hits 0.

## 2026-07-15 - Truthful normal-application catalog filter started

- Confirmed `OfficeStudy` is only an alias for `profile.Category == SoftwareCategory.Normal`; no office/study evidence exists.
- Chose a full internal rename to `NormalApplications` / `NormalAppsFilterButton` plus visible `普通应用`, preserving the exact predicate and all action policy.
- Added a focused behavior/static identity test, observed the expected missing-enum compile failure, then renamed enum, WPF tag/name/copy, and highlight enumeration together.
- Verification completed: focused 3/3, ProductExperienceTests 169/169, full 862/862, build 0 warnings/errors, 310 strict UTF-8 files, active legacy catalog terms 0, and focused method authority hits 0.

## 2026-07-15 - Software category evidence and confidence started

- Audited `SoftwareProfile`, `SoftwareInventoryBuilder`, fixture JSON, growth enrichment, app drawer presentation, and all production `new SoftwareProfile` sites.
- Found that classification currently concatenates product name, publisher, and install path, returns only an enum, and falls back to Normal without preserving why or how certain that choice is.
- Chose a scanner-owned typed observation with source-specific confidence and a compact path-free drawer explanation; classification evidence remains read-only and cannot grant modifying actions.
- After the user reported updated antivirus definitions, Computer Use successfully listed Windows apps but the explicit OMNIX-Entropy `launch_app` request timed out and no targetable window appeared. No security UI was automated or bypassed; visual launch remains Warn.
- Added `SoftwareCategoryAssessment`, source-specific evidence, fallback state, and confidence. Existing rule precedence remains Game, AI, Development, System, then Normal fallback.
- Product-name evidence is high confidence, publisher-only evidence is medium, install-location-only evidence is low, and no-signal Normal remains an explicit low-confidence fallback. Unknown profiles remain Unknown.
- Added a compact drawer category line with a stable AutomationId and static first-area order test; full paths and matched path terms remain absent from the beginner line, while bounded rule evidence is available in hidden technical details.
- Growth enrichment preserves the scanner-owned observation; system-category and unknown managed-root read-only denies remain unchanged.
- Verification completed: focused 7/7, related 218/218, full 861/861, build 0 warnings/errors, and 310 strict UTF-8 source files with zero replacement characters.

## 2026-07-15 - Unknown system-ownership review started

- Confirmed inventory classification normally returns a concrete category, but `SoftwareProfile` still defaults to Unknown for incomplete/future sources.
- Chose only canonical current Windows root and Program Files `WindowsApps` as high-confidence read-only triggers. Microsoft publisher/signature text alone is explicitly insufficient.
- Added an ownership-pending tile label, path-free location/Agent conclusions, disabled ordinary actions, and refused uninstall/cache/startup/migration previews while preserving `Category=Unknown` and technical details.
- The first broad source patch was atomically rejected due stale method-order context; it was recorded and replaced with method-local patches.
- Verification completed: focused/system/handoff 17/17, related 196/196, full 854/854, build 0 warnings/errors, 309 strict UTF-8 files, and focused forbidden-authority hits 0.

## 2026-07-15 - Compact application size explanation started

- Audited `SizeSummary` and found it shows only installation/data/recent growth, omits cache, and cannot explain whether a zero field means no identified location or no measured value.
- Chose presentation-only wording based on value plus identified path evidence; no new scanner availability claim or deletable-byte estimate will be invented.
- Added four compact fields: main-program install, data, identifiable cache, and recent growth. Default zero is never shown as measured `0 B`; identified paths with no size differ from absent location evidence.
- A related product assertion retained the useful word `安装`, so the final label is `主程序安装` rather than the less precise `主程序`.
- Verification completed: focused/neighbor 19/19, related 201/201, full 849/849, build 0 warnings/errors, 307 strict UTF-8 files, and focused forbidden-authority hits 0.

## 2026-07-15 - System-application read-only boundary started

- Found that a `SystemTool` profile on C can receive a migrate recommendation while migration is disabled, and uninstall/cache/startup actions can still enable from ordinary evidence fields.
- Chose a category-first read-only drawer contract: keep technical details available, disable all four modifying review actions, and explain why in beginner language.
- Added a category-first retain recommendation and a fixed read-only action set for system applications. Ordinary applications with the same evidence fields still receive their existing review actions.
- Verification completed: focused/system-handoff 14/14, related 193/193, full 851/851, build 0 warnings/errors, 308 strict UTF-8 files, and focused forbidden-authority hits 0.

## 2026-07-15 - Explicit application-grid C-drive labels started

- Audited the icon grid and found every C-drive footprint receives the same `需关注` text, so a C-installed main program and a D-installed app with C-drive data look identical before the drawer opens.
- Chose a presentation-only split while preserving `AppTileStatus.Attention`, risk sorting, `占 C 盘` filtering, and the existing migration-closure override.
- Replaced only the Attention short-tag selection with `主程序在 C 盘`, `数据写入 C 盘`, or `C 盘线索待确认`; visible and accessibility text remain path-free.
- Verification completed: focused 5/5, related 198/198, full 846/846, build 0 warnings/errors, 306 strict UTF-8 files, and scoped forbidden-authority hits 0.

## 2026-07-15 - Installer report program/data placement completed with visual Warn

- Added a path-free read-only placement observation for a unique newly installed profile. It canonicalizes C-drive candidates, excludes the main install tree from separate data clues, and distinguishes profile-owned clues from concurrent footprint-only changes.
- The install summary, `装了什么` card, C-drive card, and Agent answer now state main program C/D/unknown placement separately from C-drive data/write evidence. D-installed programs explicitly avoid repeat migration.
- No-unique-software and unrelated-footprint states refuse attribution; raw paths remain only in the existing technical details.
- TDD initially hit one invalid test collection-spread expression, recorded in the error ledger. The true red run failed 4/4 on missing product behavior; green passed 4/4, related 261/261, full 842/842, build 0 warnings/errors, 305 strict UTF-8 files, and scoped authority hits 0.
- Computer Use `launch_app` timed out and passive refresh found no OMNIX window; visual proof remains Warn and no fallback desktop automation was used.

## 2026-07-15 - Main-program versus C-drive data location started

- Found that `InstallLocationSummary` calls any D-installed application reasonable even when attributed C-drive writes exist, while the migration text only says to observe.
- Chose to split the beginner conclusion into main-program location and aggregate C-drive data/cache evidence. An already D-installed main program will not gain a migration action merely because data writes exist.
- Exact location questions continue to open details only; no path or operation is added to Agent replies.
- Added path-free conclusions for D/no-C, D/with-C, C/with-C, and unknown/with-C states. C-drive write clues are canonicalized, deduplicated case-insensitively, and exclude a C-installed main-program tree from the separate data count.
- D-installed applications with C-drive clues now say not to repeat main-program migration, warn that one-time cleanup may regrow, and keep migration disabled because no reliable data-location redirection plan exists.
- Verification completed: focused 5/5, related app/Agent/growth/migration 251/251, full regression 838/838, build 0 warnings/errors, 304 strict UTF-8 files, replacement-character hits 0, and scoped authority hits 0.

## 2026-07-15 - Application growth explanation started

- Audited named-app Agent answers and found growth/write questions fall through to generic drawer advice despite existing per-profile recent growth and C-drive/cache evidence.
- Chose a bounded aggregate observation with Available, InsufficientBaseline, and Unavailable states; no path or individual file crosses into the Agent.
- Scoped automatic loading to one unique exact app growth/write question after inventory. Explicit operations and generic `哪个软件增长` wording do not trigger this path, and the resulting action remains details-only.
- Added an aggregate path-free growth observation with Available, InsufficientBaseline, and Unavailable states; zero recent growth is meaningful only when comparison evidence is Available.
- Exact app growth/write questions now reuse the shared read-only health gate when no baseline exists, re-resolve the app afterward, and show separately labeled `现在腾空间` and `以后防止继续增长` steps.
- Generic, ambiguous, explicit cleanup/migration/uninstall/startup, location, and troubleshooting questions keep their existing paths. The growth action remains details-only.
- Verification completed: focused 7/7, related 286/286, full 833/833, build 0 warnings/errors, 303 strict UTF-8 files, replacement-character hits 0, and scoped growth authority hits 0.

## 2026-07-15 - Exact Agent application-action handoff started

- Audited all six primary pages and confirmed C-drive cleanup, cache cleanup, startup control, installer launch/reporting, official uninstall, and migration have real guarded backends; unsigned uninstall/migration correctly remain production-denied.
- Found the next beginner gap in named-app Agent answers: explicit cache/startup/uninstall/migration questions resolve the correct app but only open its generic drawer, requiring the beginner to choose the same action again.
- Scoped the change to automatic preparation of an existing review surface after a fresh exact target resolution. Natural language remains non-executable, no confirmation is implied, and no operation construction moves into the Agent.
- Added a typed details/uninstall/migration/cache/startup handoff and precise action labels. Handoffs require the existing drawer action to be enabled and are details-only for system applications.
- Extracted four shared MainWindow preview methods so manual buttons and Agent handoffs use identical safety preparation. Agent clicks still re-resolve current inventory before any preview is selected.
- Verification completed: focused 12/12, related 257/257, corrected static contract plus focused 13/13, full 826/826, build 0 warnings/errors, 301 strict UTF-8 files, replacement-character hits 0, and Agent Core forbidden-authority hits 0.
- One old uninstall source-string assertion failed only in full regression after the helper extraction; it was updated to prove both manual-to-shared handoff and captured-profile residue review, and the related-suite omission was recorded.

## 2026-07-15 - Beginner-safe operation error boundary started

- Audited ten MainWindow displays of `result.Error`, `policy.Error`, or `validation.Error` across startup, quarantine purge, uninstall residue, and C-drive cleanup.
- Classified purge/residue/C-drive policy validation as pre-execution safety refusals with confirmed no change. Pipeline execution and startup restore/disable failures remain unknown or potentially partial and must request rescan/Timeline review.
- Scoped the correction to primary UI copy only; underlying error objects and failure control flow remain unchanged for tests and technical boundaries.
- Added a source guard for all ten raw error properties plus phase-specific beginner conclusions, then replaced startup disable/restore, purge, residue, and C-drive cleanup presentation branches without changing control flow.
- Verification completed: focused 3/3, related workflow/product 189/189, full 814/814, build 0 warnings/errors, 300 strict UTF-8 files, all-App raw operation/policy/validation error hits 0, and replacement-character hits 0.
- After updated antivirus definitions were confirmed, two Computer Use launch requests still timed out and a passive application refresh found no OMNIX window. Visual/antivirus proof remains Warn; no PowerShell UIAutomation or SendKeys fallback was used.

## 2026-07-15 - Beginner-safe WPF failure boundary started

- Audited six `MainWindow` catches that copied `Exception.Message` into status text or a message box: system-tool open, install snapshot, quarantine purge, timeline restore, uninstall residue review, and C-drive cleanup.
- Classified open/snapshot failures as confirmed no-modification, but purge/restore/residue/cleanup failures as unknown or potentially partial; their replacement copy must request a current-state reload instead of claiming nothing changed.
- Chose fixed path-free beginner text and a static regression guard. No new diagnostic store will be invented inside this UI-only correction.
- Replaced all six raw exception displays with workflow-specific conclusions. Potentially partial mutations now explicitly request Timeline/app rescan instead of claiming no changes.
- Verification completed: focused 2/2, related workflow/product 186/186, full 811/811, build 0 warnings/errors, 299 strict UTF-8 files, all-App raw exception hits 0, and unsafe fallback identifier hits 0.
- Follow-up audit found ten raw operation/policy/validation error displays; those require a separate classification because some are pre-execution refusals while others can follow partial durable work.

## 2026-07-15 - Bounded application runtime observation started

- Audited the software inventory process attribution and chose its already-associated running names plus display-icon executable name as the only runtime identity hints; command lines, executable paths, process ids, and fuzzy matching are excluded.
- Defined a 350 ms sample, 32 matched-process maximum, aggregate working set, and coarse CPU activity. Empty/untrusted identity is Unavailable rather than a false NotRunning result.
- Scoped automatic loading to a unique exact app freeze/resource question after inventory. Crash-only, vague, generic, ambiguous, and explicit-operation questions retain their current paths.
- No process ending, suspension, priority change, external tool launch, or system mutation authority will be added.
- Added Core aggregate availability/activity types, an injected exact-name Win32 sampler, automatic named-app resource hydration, MainWindow orchestration, and symptom-specific Agent evidence.
- Hardened generic subjects ending in `的` and covered attached/spaced CPU wording without globally rewriting identity-bearing input. The initial missing spaced variant was recorded in the error ledger.
- Verification completed: focused 37/37 including a real current-process sample, related Agent 152/152, full 809/809, build 0 warnings/errors, 298 strict UTF-8 files, and zero scoped authority/private-field hits.
- A subsequent key-workflow audit confirmed real pipelines for C-drive quarantine cleanup, app cache cleanup, startup disable, and installer before/after reporting; it also found six raw WPF exception-message disclosures as the next safety slice.

## 2026-07-15 - Bounded application crash observation implemented

- Added a Core observation that retains only availability, bounded count, observation window, and latest time; it is explicitly non-executable.
- Added a 24-hour/128-candidate Windows Application-log reader with three reviewed provider/event-id pairs. Raw property values stay inside the Win32 correlation boundary and formatted messages are never read.
- Exact named-app crash/hang questions now observe after inventory and pass optional evidence to the Agent. Generic, vague, ambiguous, and explicit-operation questions do not query logs.
- Focused tests passed 8/8. A whole-solution restore timeout caused by the blocked public feed was replaced by targeted restores from the existing global package cache and recorded in the error ledger.
- Hardened the target policy against generic subjects even if an inventory record has a generic name, and made NotFound wording symptom-specific for flash-crash versus freeze/no-response questions.
- Verification completed: focused crash/troubleshooting 19/19, related Agent 121/121, full 782/782, build 0 warnings/errors, strict UTF-8 294 files, and zero crash-boundary private-field/forbidden-authority hits.
- Retried real WPF launch after the antivirus update through Computer Use. `launch_app` timed out and a passive app/window refresh returned no OMNIX target, so visual proof remains Warn without PowerShell UIAutomation or security-setting bypass.

## 2026-07-15 - Bounded application crash-log observation started

- Audited Event Log support and confirmed `System.Diagnostics.EventLog` 10.0.9 is already cached locally. Chose the managed `EventLogReader` API rather than shelling out to `wevtutil` or PowerShell.
- Defined a 24-hour window, 128-candidate maximum, and fixed Application Error/1000, Windows Error Reporting/1001, Application Hang/1002 allowlist. `FormatDescription` and every clear/export API are forbidden.
- Designed Core evidence to retain only target software name, availability, observed/window times, match count, latest occurrence, and `CanExecuteDirectly=false`; raw properties exist only transiently inside the Win32 boundary.
- Added red tests for allowlist/time/token correlation, NotFound/Unavailable, bounds/privacy/authority source contracts, exact target policy, three beginner presentation states, and inventory-before-observation-before-answer ordering.

## 2026-07-15 - Application-specific troubleshooting answers started

- Found two linked gaps: `微信闪退` could route to generic Event Viewer before inventory hydration, and an already resolved exact app with crash/freeze wording received only generic drawer advice.
- Chose a narrow pre-inventory predicate based on a non-generic subject before an app-crash word. Generic `软件/应用/程序闪退` and system blue-screen wording skip inventory.
- Defined crash, freeze, and vague-abnormal replies using aggregate profile evidence only. The existing app drawer remains the sole action; Event Viewer/Task Manager are mentioned as follow-up questions that use existing protected open-only routes.
- Added red tests for hydration boundaries, exact target handoff, missing-log honesty, aggregate-only privacy, no invented CPU/memory cause, no process-ending promise, vague symptom clarification, and explicit uninstall priority.
- Implemented the named-app troubleshooting predicate plus crash/freeze/vague presentation branches. Visible evidence contains only running/startup/service/task counts; exact process/service/task names and installation paths are not copied into the response.
- Verification: focused 55/55; related 242/242; full regression 773/773; build 0 warnings/errors; Agent presenter forbidden authority hits 0; 290 non-generated strict UTF-8 files; no XAML or tool-launch changes.

## 2026-07-15 - Natural-language whole-computer diagnosis started

- Audited `帮我体检电脑`, `电脑整体状态怎么样`, and `电脑需要怎么优化`; all currently fell through to General and could hydrate only software inventory.
- Defined a distinct full-diagnosis intent that shares the existing health gate. Kept `电脑为什么卡` on lightweight machine observation, `C盘怎么优化` on C-drive diagnosis, and possible app wording on inventory.
- Added red tests for four whole-computer phrases, four neighboring intent boundaries, completed-summary reuse, no separate software/machine probe, await-before-answer ordering, and handler non-authority.
- Implemented `SystemDiagnosis`, bounded whole-computer wording, `QuestionNeedsFullHealthScan`, shared diagnosis presentation, and MainWindow await-before-answer orchestration. Focused routing/evidence tests passed 78/78.
- First full regression passed 762 tests and failed one unrelated real process-image assertion because the sandbox exposed an alias path through `Environment.ProcessPath` while the Win32 resolver returned the kernel path. Product signer/hash/path policy was unchanged; the test now validates existing file, matching image filename, and identical SHA-256 instead of alias-sensitive string equality.
- Final verification after the test correction: focused diagnosis tests 78/78; isolated process-image test 1/1; full regression 763/763; build 0 warnings/errors; Agent Ask forbidden authority hits 0; 289 non-generated strict UTF-8 files with zero replacements.

## 2026-07-15 - No-scan Agent greetings and capability help started

- Found that `QuestionNeedsSoftwareInventory` intentionally hydrated General questions so an unknown installed-app name could be resolved after scanning, but this also made `你好`, `谢谢`, and `你能做什么` scan the machine.
- Chose exact whole-question matching for a small closed set of greetings/help phrases. Mixed or extended sentences remain General and retain automatic inventory, preserving unknown-app discovery.
- Added red tests for greetings, capability/help, mixed greeting plus app text, direct local capability copy, no navigation, and non-execution.
- Added a closed, case-insensitive whole-question phrase set with terminal-punctuation trimming and a direct local capability reply before profile resolution. Substring/mixed questions deliberately do not enter this branch.
- Verification: focused Agent/inventory tests 50/50; full regression 751/751; build 0 warnings/errors; 288 non-generated strict UTF-8 files; no XAML or authority changes.

## 2026-07-15 - Automatic system-diagnosis skill evidence started

- Audited the System Diagnosis skill after completing lightweight machine observation. It still returned `先完成一次手动电脑体检` when no health summary existed, even though Home and explicit C-drive questions already share a retryable read-only health gate.
- Chose one pure `SkillNeedsHealthScan` policy and one awaited `EnsureHealthScanLoadedAsync` call before `ExplainSkill`; unrelated skill cards must never trigger the disk scanner.
- Added red tests for all eight skill categories, evidence reuse, await-before-answer ordering, and absence of operation/process/file/registry authority in the skill handler.
- Implemented `SkillNeedsHealthScan` and one `EnsureHealthScanLoadedAsync` await before `ExplainSkill`. System Diagnosis alone can request missing health evidence; the other seven categories cannot.
- Verification: related tests 223/223; full regression 744/744; build 0 warnings/errors; 288 non-generated strict UTF-8 files; 16 XAML parses; skill-handler forbidden authority hits 0. No new XAML or visual claim was introduced.

## 2026-07-15 - Agent lightweight machine observation implemented

- Added one shared presentation builder for D-drive, memory, process-count, and battery evidence so full health summaries and lightweight Agent answers use the same beginner wording.
- Added pure Agent evidence policies for hardware and machine-health questions plus the hardware skill card; unrelated C-drive, settings, and startup questions do not request this probe.
- Added a deduplicated machine-observation gate in `MainWindow`; Agent questions await it before answering, and the full C-drive scan refreshes and reuses the same observation instead of constructing a second probe directly.
- Kept the lightweight answer separate from `HealthCheckSummary`, so it cannot invent the disk-backed overall score. Unavailable observations are explicit and do not become a false all-clear.
- First focused green run passed 18/19; restored the pre-existing Home fallback for direct no-evidence presenter calls, then passed 19/19. Strengthened cache-reuse and unavailable-state assertions before broader regression.
- Final verification: focused 21/21; related 236/236; full 735/735; build 0 warnings/errors; 287 non-generated strict UTF-8 files; 16 XAML parses; 120 event bindings; 277 unique literal AutomationIds; machine-core forbidden authority hits 0.
- After the user confirmed updated antivirus definitions, Computer Use could list apps but `launch_app` still timed out. A passive follow-up found no OMNIX window and a read-only process check found no `Css.App`/`Css.SmokeTools`; no fallback UI automation was used, so visual acceptance remains Warn.

## 2026-07-15 - Automatic application inventory slice started

- Audited the application and install-control first-use flows after completing production readiness.
- Found that application management still displayed `还没有扫描应用。点击“扫描应用”` and the C-drive app handoff told beginners to initiate a read-only app scan themselves.
- Chose a one-time lazy read-only inventory load on first Apps navigation, with task deduplication and an explicit manual `重新扫描` refresh remaining available.
- TDD red first exposed the missing load gate; an initial async exception assertion also needed an explicit `Func<Task>` test type.
- Added `SoftwareInventoryLoadGate`: concurrent first entry shares one task, completed empty inventory is cached, failed/faulted loads retry, and manual refresh forces a new load.
- Apps navigation now starts the load without blocking home startup. The manual button says `重新扫描`, and empty/failure copy no longer instructs the beginner to understand software-profile scanning.
- The C-drive app handoff is now async and awaits the same load before refreshing the `占 C 盘` filter. Growth/app-target refresh also reuses the gate, avoiding a second scan.
- A failed manual refresh preserves the previous inventory and says so; it does not silently convert a known list into an empty result.
- Verification: focused 4/4; related 170/170; full 699/699; build 0 warnings/errors; strict UTF-8 282; XAML parse 16; event handlers 120/120; duplicate literal AutomationIds 0; lazy-load forbidden authority hits 0.

## 2026-07-15 - Post-antivirus critical workflow audit started

- Re-read `AGENTS.md`, current state, handoff, worktree status, and the active persistent goal.
- Confirmed `CanLaunchDevelopmentVerification` is used only by a DEBUG fake-worker lifecycle path; that worker has no real uninstall, cleanup, migration, or mutation authority.
- Confirmed production uninstall and migration launchers still require trusted same-signer package evidence plus an exact worker SHA-256.
- Started auditing beginner-visible disabled or inert controls to choose the next critical workflow connection.
- Chose the late trust-refusal dead end: an unsigned user could previously complete recovery preparation and final confirmation before learning that the package could not execute.
- TDD red failed on missing `ProductionExecutionCapability` and readiness presentation types. Added a shared current-package trust provider and beginner-facing readiness model.
- Added first-visible readiness panels with stable AutomationIds to uninstall and migration plan windows. Untrusted packages disable final checklist/evidence preparation and final confirmation; execution coordinators still re-assess immediately before launch.
- An existing WPF fixture initially failed because it did not declare package trust. Kept production fail-closed and updated the fixture to pass explicit trusted readiness; added a separate default-fail-closed WPF case.
- Verification: focused 7/7; related 24/24; full 695/695; build 0 warnings/errors; strict UTF-8 280; XAML parse 16; event handlers 120/120; duplicate literal AutomationIds 0; WPF forbidden authority hits 0.
- Visual gate: Computer Use launch timed out and discovery/process inspection found no app. Recorded Warn without UI fallback.

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

## 2026-07-11 - Authenticated in-memory fake transport completed

- Added HMAC-SHA256 authenticated messages bound to protocol, session, message id, nonce, request id, descriptor hash, and timestamp.
- Endpoint enforces two-minute freshness, 30-second clock skew, fixed-time tag comparison, descriptor hash recomputation, separate nonce/message replay tables, bounded capacity, cancellation propagation, and response correlation.
- End-to-end integration passed through the real SafetyOperationPipeline and OfficialUninstallOperationHandler with fake launcher/post-scanner, then produced a path-free beginner response model.
- Verification: transport tests 7/7; integration 1/1; full suite 326/326; Debug and Release builds 0 warnings/errors. Release smoke strings absent; App has no Elevated reference or runtime registration; no process/temp residue.

## 2026-07-12 - Bounded serialized fake named-pipe transport completed

- Added a 64 KiB length-prefixed strict JSON protocol for authenticated official-uninstall requests and typed path-free responses.
- Request decoding preserves only string/bool argument types and the elevated endpoint independently recomputes the descriptor hash. Responses are HMAC-bound to the request and omit scanner summaries, paths, and raw exception text.
- Added one-shot real Windows named-pipe client/server adapters around an injected fake endpoint. Pipes use `CurrentUserOnly`; both sides validate expected SID, PID, and Windows session before request handling.
- Added startup/response timeout, external cancellation, malformed/oversized frame, request/response tamper, replay, correlation, and early-disconnect behavior.
- Verification: focused 14/14; full suite 340/340; Debug and Release builds 0 warnings/errors; App/Program/runtime registration and mutation audits passed; no App/Elevated process remained.
- No WPF wiring, elevated worker startup, handler/launcher/scanner registration, process launch, or real uninstall occurred.

## 2026-07-12 - Neutral IPC library and DEBUG WPF pipe flow completed

- Moved pure request composition, descriptor integrity, safe execution-result models, post-scan presentation, and elevated-response presentation into Core.
- Added `Css.Ipc` referencing Core only; moved authenticated messages, replay/freshness checks, strict codec/framing, Windows peer identity, and fake named-pipe client/server into it.
- App now references Ipc but still not Elevated. Its DEBUG-only final-consent flow creates a fully checked request, crosses a real current-user pipe to an injected authenticated fake endpoint, and presents the decoded path-free result.
- GUI smoke passed with confirmation disabled until three acknowledgements, two typed pipe facts, visible Agent/safety text, two clean screenshots, and no real execution control.
- Verification: related 50/50; full suite 340/340; Debug and Release builds 0 warnings/errors; Release smoke strings absent; Program/authority/process audits passed.
- No elevated worker, handler, launcher, scanner, installer, uninstaller, registry/service/task change, or file mutation occurred.

## 2026-07-12 - Identity-bound ephemeral IPC session bootstrap completed

- Added strict bounded client/server hello and finished codecs, fresh P-256 ECDH key pairs, 32-byte nonces, transcript hashing over protocol/session/pipe/SID/PID/session/public keys/nonces, and HMAC-SHA256 extract/expand.
- Both sides verify role-specific finished MACs before returning the session key. A bounded expiring replay guard rejects reused client nonces.
- Session keys own 32 bytes, export copies explicitly, zero on Dispose/finalization, and refuse reuse. Authenticated client/endpoint now also zero owned key copies and replay tables on Dispose.
- Verification: bootstrap 7/7; related key lifecycle 15/15; full 348/348; Debug/Release 0 warnings/errors; secret-channel/authority/Program/Release/process audits passed.
- No process launch, WPF change, worker registration, handler/launcher/scanner call, or system mutation occurred.

## 2026-07-12 - Separate-process authenticated smoke worker

- Added a strict `official-uninstall-ipc-worker` mode to development-only `Css.SmokeTools`; it hosts one current-user pipe, validates the exact parent SID/PID/session, performs ephemeral ECDH bootstrap, handles one authenticated typed fake request, writes one path-free receipt, and exits.
- Added an injected test-side process launcher with argument-list construction, redirected bounded output, startup/response/shutdown deadlines, and whole-tree termination on disposal.
- Added real child-process coverage for successful bootstrap/request/response/exit, startup timeout, forced parent-side disposal, non-secret launch metadata, and absence of real execution authority.
- Verification: focused Debug 4/4; focused Release 4/4; related 33/33; full suite 352/352; Debug/Release builds 0 warnings/errors; authority/release/process audits passed.

## 2026-07-12 - Runtime final-consent visual receipt

- Moved the pure in-memory visual receipt issuer and one-time request session from `Css.Elevated` to `Css.Core`, allowing App to create evidence without referencing elevated authority.
- Added WPF content rendering through `RenderTargetBitmap`/PNG encoding, nonblank pixel validation, actual viewport checks for recovery truth, three confirmations, readiness/safety text, collapsed/absent technical details, and absent run control.
- Final consent now issues a ticket before closing, keeps the window open on capture/issue failure, immediately zeroes PNG bytes, and exposes only consent plus ticket id. The DEBUG fake pipe consumes the ticket through `OfficialUninstallElevatedRequestSession`; the fixed screenshot hash was removed.
- Added real STA modal-window tests for nonblank capture, hash agreement, one-time issue/consume, four safety flags, byte zeroization, and refusal of an unshown window. Updated static and smoke contracts.
- Verification: WPF 2/2; related 25/25; full 354/354; Release combined 6/6; Debug/Release builds 0 warnings/errors; boundary/process audits passed.
- GUI evidence gap: Computer Use connected successfully but its local-app launcher timed out for both command-line and separate-arguments attempts; no App process appeared, so no fresh external screenshot was claimed.

## 2026-07-12 - Reproducible render evidence and production final-consent entry

- Added test-only optional PNG export restricted to repository `.omx`; default tests still persist nothing and production capture remains free of file APIs.
- Fresh render inspection caught a transparent/cropped root caused by a margin on the rendered visual. Added a full-size background root and moved margin inward. A later rerender exposed first-frame black blocks; capture now flushes WPF Render priority and paints an origin-normalized `VisualBrush` before encoding.
- Final artifact `.omx/qa-runtime-final-consent-render.png` was inspected: all beginner safety content and both actions are complete, readable, and uncropped.
- Added production Continue from a Ready uninstall checklist to the final-consent window. The first checkbox now confirms app/tray closure plus official command. A Core request-preparation service revalidates uninstaller, snapshot hash, recovery, consent, and one-time visual ticket; it can only return a typed draft.
- Added four service tests and two STA plan-entry tests. Verification: uninstall-related 121/121; full 362/362; Release high-risk 12/12; Debug/Release 0 warnings/errors; authority/release/process audits passed.
## 2026-07-12 - Production fake elevated worker lifecycle started

- Inspected the latest protocol records, worktree, neutral IPC bootstrap/codec, separate smoke worker, final-consent request preparation, and current project references.
- Chose an injected launch/process-handle boundary: App owns Windows process creation; Ipc owns only metadata, peer verification, bootstrap, authenticated one-shot exchange, status mapping, and bounded cleanup orchestration.
- Kept the production WPF flow disconnected from the fake worker for this slice so a no-op response cannot be mistaken for a completed user uninstall.

## 2026-07-12 - Production fake elevated worker lifecycle verified

- Added neutral launch/process contracts and a lifecycle client that derives current identity, launches once, verifies the connected server against the exact launched PID/SID/Windows session, performs ephemeral bootstrap, sends one authenticated request, and guarantees bounded wait or whole-tree termination.
- Added an App-owned `runas` adapter with explicit Windows cancellation error `1223` mapping. Its arguments contain only pipe/session/client identity/timeout metadata.
- Added `Css.Elevated` mode `official-uninstall-fake-worker`; it hosts exactly one current-user pipe session and returns a typed response with `UninstallerStarted=false`. No real handler, launcher, post-scanner, pipeline, registry, service, task, delete, or move API is registered.
- Added seven tests covering fake success, injected UAC cancellation, wrong launched PID, bootstrap session mismatch, response timeout, delayed child cleanup, and static authority/secret-channel boundaries.
- Verification: lifecycle 7/7 Debug and 7/7 Release; official-uninstall 101/101; full 369/369; Debug/Release solution builds 0 warnings/errors; static audits and process audit passed.
- Actual interactive UAC cancel/accept remains a manual environment check; no production WPF control invokes the fake worker.
## 2026-07-12 - Beginner worker result and packaging slice started

- Re-read protocol/current/handoff and inspected App startup, existing post-scan result UI, App/Elevated project files, and static dependency-boundary tests.
- Chose a build-only MSBuild invocation/copy rather than a `ProjectReference`; App must not compile against or list Elevated in `Css.App.deps.json`.
- Scope remains fake-only and DEBUG-only for orchestration. The production uninstall plan stays disconnected and no UAC prompt will be launched without an explicit smoke action.

## 2026-07-12 - Beginner worker result and packaging verified

- Added a compact WPF result window and presenter for every worker lifecycle state. Visible copy contains only plain conclusions, Computer Agent advice, and no-change safety text; it never shows paths, PID, ECDH, protocol status, or raw errors.
- Added exact sibling worker resolution with missing/probe/reparse rejection. This is verification readiness only and is not yet a production code-signing trust decision.
- App build/publish now invokes and copies `Css.Elevated.exe`, `.dll`, `.deps.json`, and `.runtimeconfig.json` without a `ProjectReference`; `Css.App.deps.json` remains free of Elevated.
- Added DEBUG-only `--smoke-uninstall-worker-lifecycle` composition using the real `runas` adapter and a manual Accept/Cancel GUI script. The fake endpoint still reports `UninstallerStarted=false`; production WPF remains disconnected.
- Exported and inspected `.omx/qa-runtime-worker-lifecycle-result.png` after reducing the window from 520 to 430 px high. Title, status, conclusion, Agent advice, safety text, and close action are all visible without overlap.
- Verification: presentation 15/15; impacted 188/188; Release presentation+lifecycle 22/22; full 384/384; Debug/Release solution builds 0 warnings/errors; Release publish/route/deps audits and empty process audit pass.
- Actual secure-desktop Accept/Cancel clicks remain manual and were not triggered automatically in this slice.
## 2026-07-12 - Signed worker production trust gate started

- Re-read protocol/current/handoff and inspected the existing `SignatureInspector`, path resolver, launcher, DEBUG smoke, and actual Release signatures.
- Confirmed both current local App and worker builds are `NotSigned`; this is acceptable only for the explicit fake DEBUG smoke and must block future production execution.
- Rejected subject-only matching because another certificate can reuse the same subject. Selected Windows `WinVerifyTrust` plus exact signer certificate thumbprint comparison and SHA-256 evidence.

## 2026-07-12 - Signed worker production trust gate verified

- Added a Win32 Authenticode verifier using offline/cached full-chain `WinVerifyTrust`, strong-algorithm flags, embedded signer certificate extraction, normalized thumbprint, and SHA-256 evidence.
- Added an App trust policy: production requires both files trusted and exact signer-thumbprint equality. A pair of unsigned, hash-readable binaries may only enter the explicit DEBUG fake smoke; mixed unsigned/trusted, invalid, untrusted, missing, probe failure, or signer mismatch fail closed.
- Added path-free Agent trust conclusions for trusted, development-only, incomplete, unsigned, mismatch, and Windows verification failure states.
- Added an expected-worker hash to the App launcher and fixed-time revalidation before `Process.Start`, so a changed file fails before UAC.
- Verified a genuine embedded Microsoft signature (`Taskmgr.exe`) is trusted and an appended-byte copy loses trusted status. Current local App/worker both report `NotSigned`, with hashes, development verification true, production authorization false.
- Re-rendered and inspected `.omx/qa-runtime-worker-trust-result.png`; the compact window clearly says `当前是开发验证版本`, `仅允许测试`, and that no real uninstall/cleanup/system-modification authority is granted.
- Verification: trust 12/12, impacted 185/185, Release trust/presentation/lifecycle 34/34, full 396/396, Debug/Release builds 0 warnings/errors; source, Release route, project-reference, signature, temp, and process audits pass.

## 2026-07-12 - Post-start worker image correlation verified

- Added launch-time expected image evidence and an independent Windows post-start inspector using limited process-query rights plus `QueryFullProcessImageName` and SHA-256.
- The neutral lifecycle now checks exact normalized path and fixed-time hash before pipe connection. Missing evidence, path mismatch, hash mismatch, or inspection failure returns `WorkerImageRejected` and triggers bounded whole-tree cleanup.
- Added path-free Computer Agent copy and a fresh inspected 680x430 WPF render for the rejection state.
- Verification: lifecycle/presentation 28/28 Debug; trust/lifecycle/presentation 40/40 Release; full 402/402; Debug/Release builds 0 warnings/errors; authority/order/package/process audits passed.

## 2026-07-12 - Mandatory post-scan semantics verified

- Moved the read-only post-scan ahead of non-zero exit handling so any official uninstaller that actually starts and exits is re-inspected once, even after cancellation or partial failure.
- Non-zero exits remain unsuccessful and never claim completion; scan findings are attached, scan failure requires retry, and caller cancellation is preserved. A launcher that never starts still does not scan.
- Verification: handler 11/11; uninstall subsystem 35/35 Debug and Release; full 405/405; read-only/unregistered/process audits passed.

## 2026-07-12 - Elevated package authorization before bootstrap verified

- Centralized limited-rights process image-path resolution in Css.Win32 and reused it from App worker-image inspection.
- Added a one-shot server authorization hook after actual pipe-peer validation and before ECDH. Denial closes the session without bootstrap, request transfer, or handler invocation.
- Added Elevated independent App/worker package authorization: both embedded signatures must be Windows-trusted and certificate thumbprints must match exactly. Current unsigned Release binaries are production-denied.
- Added an unregistered production session composition that always wraps the real handler in `SafetyOperationPipeline`; fake integration proves denied packages call neither launcher nor scanner, while injected trusted packages execute once and post-scan once.
- Verification: focused 35/35 Debug and Release; full 417/417; Debug/Release 0 warnings/errors; order/thumbprint/no-mutation/Program/WPF/process audits passed.

## 2026-07-12 - Authenticated request preparation freshness verified

- Added final-confirmation time to every ready request. `CanSubmit` requires it; the composer binds the verified consent time rather than a later assembly time.
- Bumped authenticated transport to v2 and strict pipe schema to v2. Preparation ticks are serialized and included in the HMAC canonical tag.
- Endpoint and Elevated production session both reject requests older than 15 minutes or more than 30 seconds in the future before execution. Tampering after authentication fails the tag.
- Verification: focused 47/47 Debug; Release critical 73/73; full 421/421; Debug/Release 0 warnings/errors; freshness/schema/Program/process audits passed.
- One existing bootstrap tamper test produced `ProtocolRejected` instead of `KeyConfirmationFailed` once in the first full run, then passed standalone 10/10 and the next full run; strict assertion retained pending recurrence.

## 2026-07-12 - Self-denying production worker command mode verified

- Refactored the handler to create its post-scanner from the exact manifest that passed hash, age, recovery, software, and command validation. Inside the already elevated worker the official launcher no longer requests a nested UAC.
- Extracted one strict metadata parser. Production accepts only six required metadata pairs and rejects fake delay options; fake mode retains explicit test-only delays.
- Registered `official-uninstall-production-worker` in Elevated only. It composes package authorization, authenticated/fresh one-shot session, safety pipeline, official process launcher, manifest-bound read-only inventory/background/residue scan, and timeline.
- A direct Scanner project reference initially introduced System.Text.Json 8/10 warnings. Replaced it with a minimal fail-closed read-only uninstall-registry reader; builds returned to zero warnings and Elevated deps excludes Css.Scanner.
- Real process test launches current unsigned production worker and proves self-denial before bootstrap plus clean exit; no request or uninstaller can run. App source/binary contains no production mode/session.
- Verification: focused 57/57 Debug and Release; full 427/427; Debug/Release 0 warnings/errors; binary/deps/mutation/registry/process audits pass.

## 2026-07-13 - Trusted App production lifecycle and beginner result verified

- Added a production-launcher marker at the IPC boundary and distinct `CompletedProduction` / `ProductionLauncherRejected` lifecycle states. Fake and production entry methods now reject a launcher of the wrong authority before process start.
- Added `WindowsOfficialUninstallProductionWorkerLauncher.Create`, which accepts only a production-trusted package assessment with an exact worker SHA-256. Production arguments select only the real Elevated mode and contain no fake switches or secret material.
- Converted typed production payload truth into beginner conclusions for not-started, incomplete, failed post-scan, software-still-present, residue-found, and clean completion states. No technical path or raw protocol status is shown.
- Verification: focused 54/54 Debug/Release; full 436/436; Debug/Release builds 0 warnings/errors; inspected `.omx/qa-runtime-production-worker-result.png`; no Css/OMNIX process remains.

## 2026-07-13 - Final-consent WPF production coordinator verified

- Added an injected App execution coordinator that reassesses the current package, refuses unsigned builds before runner creation/UAC, creates the trusted production launcher only after production trust, and returns lifecycle plus typed post-scan presentation.
- `UninstallPlanWindow` now hands its one-time verified request to the coordinator and shows either a trust/lifecycle conclusion or the existing beginner post-scan window. MainWindow injects the current-package coordinator and reports the returned conclusion.
- Replaced the obsolete “no ExecuteAsync anywhere in WPF” test with the actual authority boundary: WPF may call the coordinator but cannot construct a production launcher/mode, lifecycle client, pipeline, handler, or process.
- Stabilized finished-message tamper classification: after key derivation, malformed/truncated/invalid finished evidence is consistently `KeyConfirmationFailed`; pre-finished protocol errors retain their original status.
- Verification: focused 43/43 and bootstrap/coordinator 11/11; two full runs 440/440; Release critical 67/67; Debug/Release 0 warnings/errors; WPF authority and process audits pass.

## 2026-07-13 - Production post-scan linked to local residue quarantine flow

- Added plan-window outcome flags for completed production and residue-review recommendation. MainWindow retains the exact pre-uninstall profile even after its tile disappears.
- After completed production, App refreshes software inventory once and reuses it for local `UninstallResidueScanBuilder` evidence. IPC continues to carry only path-free counts/status and never serializes `ResidueReport`.
- Refactored residue review to accept captured profile plus optional refreshed inventory. Catalog refresh now precedes inline review display, preventing selection changes from hiding the Agent conclusion.
- Existing confirmation, quarantine policy, `SafetyOperationPipeline`, timeline reload, and regret-center restore remain the only low-risk mutation path; medium/high-risk groups remain blocked.
- Verification: focused 29/29 Debug, 31/31 Release; full 441/441; Debug/Release 0 warnings/errors; path-free wire/pipeline/process audits pass.

## 2026-07-13 - Migration evidence, rollback coordinator, and closure monitor verified

- Added atomic, bounded migration rollback-manifest writes plus SHA-256 creation and fixed-time verified reads. Gate operations now carry manifest hash and affected process/task/startup evidence.
- Added an injected migration operation handler behind `SafetyOperationPipeline`: exact manifest/operation/freshness/path correlation, active-component denial, source/destination pre-observation, move+redirect delegation, reverse rollback, and typed incomplete-rollback status.
- Added persistent monitoring records and later reload/scan. Missing redirects, changed targets, or recreated real original directories become distinct closure findings.
- Added strict Windows path policy: C user paths only, D destination only, approved OMNIX roots only; Windows, Program Files, ProgramData, Recovery, recycle bin, and system-volume paths are blocked.
- Verification: focused 24/24 Debug/Release; full 449/449; Debug/Release 0 warnings/errors; no process/temp residue; WPF still has no real migration authority.

## 2026-07-13 - Windows directory migration adapter and beginner result verified

- Added reparse-safe bounded directory traversal, nested copy, per-file size/SHA-256 verification, unique staging, destination commit, source removal, and injected redirect creation without shell commands.
- Added rollback mechanics that remove the redirect, restore a missing source from the verified destination, verify both copies, then remove destination. Target collision stops before source change; redirect failure fixture restores fully with no staging residue.
- Added path-free result presentation for completed, refused, failed-and-restored, and incomplete rollback states plus a 680x430 WPF window with stable AutomationIds.
- Normal-process symbolic-link probe returned `UnauthorizedAccessException`, confirming native redirect must remain in elevated execution.
- Verification: focused 19/19 Debug/Release; full 460/460; Debug/Release 0 warnings/errors; screenshot inspected; no process/temp residue; MigrationPlanWindow still contains no adapter/pipeline/move authority.
## 2026-07-13 - Security product alert stopped migration worker work

- Read the user-exported Huorong log and stopped all Worker, pipe, build, and test execution.
- The alert consistently names only generated `Css.Tests.dll` in Debug `obj`/`bin`, beginning immediately after Huorong definitions updated at 09:50; production assemblies are not named in the supplied log.
- Confirmed no OMNIX, Css, dotnet testhost, or vstest process remained.
- Static source audit found no process-injection, reflective-loader, or download-execute primitives. The test assembly does combine legitimate process/UAC/pipe coverage with literal examples of blocked PowerShell and destructive cmd uninstall commands, which may trigger a heuristic signature.
- Classified the event as unproven, highly suspicious false positive. No whitelist, restore, execution, or antivirus bypass was attempted.

## 2026-07-13 - Huorong confirmed the test assembly was a false positive

- User relayed Huorong's analysis response: the submitted artifact is confirmed as a false positive and a forthcoming/recent virus-definition update will exclude it.
- Kept the artifact quarantined and builds paused until the corrected definitions are evidenced locally; no exclusion or protection bypass is required.
- No OMNIX, Css, dotnet testhost, or vstest process was running at the transition.

## 2026-07-13 - Source-only migration snapshot, coordinator, and final consent

- Added strict migration snapshot evidence storage and fixed-time hash comparison, then required the Elevated handler to re-observe each source immediately before any path mutation and refuse changed/unreadable sources.
- Wired the production worker to the Windows snapshot reader and tightened the handler contract to manual, elevated, high-risk confirmed operations only.
- Added source tests for hash tamper, unknown JSON fields, stale/mismatched evidence, unsafe source observations, post-snapshot changes, trust refusal, response correlation, typed refusal, and WPF authority boundaries.
- Added a path-free final consent presenter/window with four explicit acknowledgements and stable AutomationIds. MigrationPlanWindow calls only an injected coordinator; MainWindow still supplies readiness with the feature disabled.
- Static verification only: migration XAML XML parsing passed; WPF forbidden-authority scan passed; migration UI encoding scan passed. No build, test, Worker, UAC, or real filesystem migration was run.

## 2026-07-13 - Source-only trusted installer route and initial post-scan

- Replaced filename authority with bounded package inspection: extension/binary markers identify type, a no-write/no-delete-share handle binds marker bytes to an independent SHA-256, and the Authenticode verifier hash must match.
- Added conservative capability policy: only Windows-trusted high-confidence Inno/NSIS packages receive one interactive directory argument; MSI/Burn/generic EXE are guided without guessed arguments; MSIX is Windows-managed; unknown/untrusted packages are refused.
- Added a bounded hash-verified install-before evidence file, high-risk manual operation descriptor, four-item 15-minute final consent, handler-side snapshot/package/type/path/argument revalidation, and a dedicated interactive launcher with no silent switches or forced elevation.
- Added an App coordinator that waits for the observed installer process and produces only an initial post-scan diff; exit code is never treated as installation success. Added beginner final-consent/result windows and changed the install page to show Agent conclusion before folded technical details.
- Production WPF source is wired but `InstallerLaunchFeatureEnabled=false`. Static XML/AutomationId/order/authority scans passed. All new C# and test source remains uncompiled and unexecuted until corrected Huorong definitions are installed.

## 2026-07-13 - Source-only software growth history and home Agent linkage

- Replaced top-level-only snapshot construction with a globally bounded 2,048-item model that also records exact scanned software-known install/data/cache/log/C-write paths; ambiguous exact claims stay owned by `多个软件`.
- Added first-observation semantics and multi-snapshot trend evidence. `持续增长` now requires at least three contiguous recent observations, at least two positive intervals, a two-thirds positive majority, and positive total growth.
- Added SQLite `LoadRecentAsync`, latest-eight trend history, per-scan-root retention of 90 snapshots, foreign-key enforcement, item indexing, and independent snapshot payload validation.
- Tightened display folding so attributed descendants hide a broad parent only when non-overlapping children explain at least 80% and do not have weaker trend evidence.
- Wired path-free Agent conclusions into the C-drive page and promoted only sustained growth to the home key findings. Both surfaces have stable AutomationIds and separate current relief from prevention; no action can execute directly.
- Added source tests for bounds, exact/shared attribution, first observation, sustained/non-sustained trends, history integration, parent attribution coverage, SQLite order/retention/oversize refusal, home growth explanation, and UI ordering.
- Static verification only: XAML XML/order/AutomationIds, scoped authority/private-path scan, UTF-8 scan, retention/wiring invariants, and trend honesty invariants passed. No build, test, GUI, Worker, UAC, installer, or real path operation ran while corrected Huorong definitions remain pending.

## 2026-07-13 - Source-only growth finding to exact application drawer

- Added a structured application target to sustained software findings and growth decisions; shared/system findings remain untargeted.
- Added a fail-closed resolver that accepts only one exact case-insensitive current-profile match. Missing inventory may be refreshed once read-only; missing, duplicate, or unavailable profiles produce a path-free refusal instead of guessing.
- Wired home details and the C-drive growth conclusion through one internal navigation helper. It resets Apps filtering/search, selects the matching tile, opens its drawer, and owns no execution authority.
- Enriched unique software profiles with recent growth while deduplicating nested paths and summing independent roots. Application tiles now show `最近变大`, and the drawer shows the recent growth amount.
- Centralized all application profile replacement so later app scans, uninstall completion, and residue review reapply the latest growth evidence instead of silently clearing it.
- Static verification only: XAML parse/AutomationIds/order, two-assignment profile invariant, read-only navigation authority, and UTF-8 scans pass. New tests remain source-only until Huorong definitions update.

## 2026-07-13 - Source-only application cache quarantine closure

- Audited all 34 MainWindow click handlers and identified application cache cleanup as the clearest beginner-facing dead end: it explained candidates but offered no safe continuation.
- Added a bounded plan/path policy. Only cache-named existing directories under current-user Local/Roaming/LocalLow roots qualify; system apps, running apps, excess candidates, reparse ancestors, outside roots, overlaps, missing paths, and changed profile attribution refuse.
- Added a drawer primary action with a separate confirmation. After confirmation, MainWindow read-only rescans inventory, resolves one exact app, confirms it is stopped and still owns the same cache paths, then calls a dedicated cache handler through `SafetyOperationPipeline`.
- Added path-free refused/completed conclusions. Success reloads the timeline, refreshes application inventory, and offers `打开后悔药中心`; no direct deletion, registry, service, startup, installer, or migration authority was added.
- Changed quarantine ordering to persist the recovery manifest before moving. Multi-item or timeline-write failure now reverses already moved items; incomplete compensation records a partial-restore timeline when possible.
- Added source tests for accepted/refused/stale plans, profile correlation, temporary-directory quarantine/timeline/restore, specialized WPF wiring, and recovery ordering. Updated obsolete static tests that required navigation-only buttons or direct profile assignment.
- Static verification only: XAML/click handlers, path policy, WPF authority, manifest/timeline rollback ordering, execution gates, stale assertions, and UTF-8 scans pass. No build, test, GUI, or real cache operation ran.

## 2026-07-13 - Source-only startup settings handoff

- Confirmed from Microsoft Learn's current Windows Settings URI reference that `ms-settings:startupapps` is the documented Startup apps page.
- Added a medium-risk, confirmation-required, open-only `startup-apps` entry to the existing settings catalog.
- Added a startup handoff presenter. Only non-system profiles with ordinary startup entries get a settings action; system profiles and service/task-only evidence remain explanation-only because current strings are not sufficient rollback identities.
- Reused the single allowlisted settings launcher from both Agent and app drawer surfaces. It resolves a catalog id, requires `IsOpenOnly`, checks the `ms-settings:` prefix, confirms when required, and reports that no setting was changed.
- Changed the drawer label from `关闭自启动` to `管理自启动` so the UI does not imply an automatic modification.
- Added source tests for ordinary/system/service/task decisions, drawer primary routing, catalog URI/confirmation, and absence of registry/service/task authority. Static XAML/AutomationId/click/catalog/launcher/presenter/UTF-8 checks pass; Settings was not launched.

## 2026-07-13 - Source-only structured background component evidence

- Added deterministic identities for registry Run values, services, and scheduled tasks. Identity binds component kind, exact source locator, and name; a separate observation fingerprint changes when observed configuration changes.
- Added read-only per-component observations and a profile-level inventory snapshot. Both explicitly refuse direct change and rollback claims, while listing the exact original evidence a future privileged snapshot must capture.
- Extended the read-only scanner to retain exact Run key sources, WMI/registry service start mode and runtime state, and the task-level Settings enablement flag. Unknown evidence remains unknown.
- Kept compatible name lists for existing inventory consumers and preserved the structured observations when growth enrichment clones a profile.
- Put structured identities and readiness reasons only in the default-folded technical details; beginner summaries remain path-free. Corrected the runtime-bound action label to `管理自启动`.
- Added focused source tests for stable identity/configuration fingerprints, service/task state separation, structured builder output, name-only refusal, missing service state, task Settings scope, hidden technical details, and absence of mutation authority.
- Static verification only: seven fail-closed checks passed for Core authority, Scanner authority, unknown-state handling, folded presentation, test-source coverage, UTF-8, and XAML parsing. No compiler, test runner, GUI, Settings, registry/service/task mutation, Worker/UAC, or real C/D operation ran.

## 2026-07-13 - Source-only StartupApproved correlation without byte decoding

- Added distinct missing, binary, unsupported-type, and unreadable StartupApproved evidence states. Binary payloads are immediately SHA-256 fingerprinted and discarded; effective activation remains unknown and cannot authorize change.
- Extended registry Run records with optional approval evidence and included it in the startup observation fingerprint without changing stable component identity.
- Opened HKCU64, HKLM64, and HKLM32 explicitly. HKLM32 Run evidence is correlated to HKLM64 `StartupApproved\Run32`; all access remains `OpenSubKey(..., false)`/`GetValue` only.
- Added one beginner Agent line stating that the current switch must be confirmed in the Windows page and OMNIX will not guess internal bytes. Registry locators and fingerprints remain in default-folded technical details only.
- Added source tests for hash drift, no raw-byte property, no state decoding, missing/unsupported/unreadable separation, Builder propagation, explicit registry views, no byte rules, no registry-write authority, and beginner wording.
- Static verification only: seven checks passed. No compilation, tests, GUI, Settings, registry write, process launch, Worker/UAC, or real system mutation ran.

## 2026-07-13 - Source-only local Computer Agent conversation

- Added a deterministic local question presenter for C-drive, applications, startup/background, installation routing, migration, uninstall, restore, exact-app, empty, and general intents. It uses only current `HealthCheckSummary`/`SoftwareProfile` evidence and explicitly reports missing evidence.
- Added a beginner-first Agent question box and answer panel before the existing static suggestions. Stable AutomationIds cover the input, answer, evidence, next steps, safety/privacy lines, and navigation button; the left Agent column now has its own scroll viewer.
- Added path privacy fallback for evidence-derived text, exact unique app targeting, duplicate/stale refusal, and allowlisted internal-page navigation. Replies cannot execute and do not call cloud AI.
- Added `AgentConversationTests` source coverage for evidence honesty, path non-echo, startup uncertainty, install roots, restore, unique/duplicate apps, stale targets, first-visible order, and WPF/Core authority.
- Static verification only: MainWindow XML parses; 36 unique Click handlers resolve; required AutomationIds are unique and ordered; Agent handler/Core authority scans pass; 236 non-generated source/XAML files pass strict UTF-8; focused test source is present. No build, test, GUI, process, Worker/UAC, cloud call, or real C/D operation ran.

## 2026-07-14 - Source-only beginner migration wording

- Replaced the application drawer's remaining English migration summaries and preview lines with plain Chinese for safe, stop-and-verify, cache-only, D-drive, and system-tool outcomes.
- The preview now states the recommended destination, Agent judgment, snapshot/rollback requirement, normal-start verification, original-C-write verification, and that the drawer will not move files.
- Updated ProductExperience expectations and added an exact-app Agent migration answer source test. No planner, handler, WPF authority, destination policy, or execution gate changed.
- Six static checks passed for Chinese completeness/English removal, presentation-only authority, updated source contracts, MainWindow XML/36 handlers, strict UTF-8, and disabled installer/migration gates. No tests or GUI ran.

## 2026-07-14 - Source-only real application icon pipeline

- Added a bounded `DisplayIcon` parser for quoted/unquoted local-drive paths, optional signed resource indexes, environment expansion, extension allowlisting, and fail-closed refusal of network/URI/relative/unresolved/command-like values.
- Propagated icon path and index through software inventory, `SoftwareProfile`, app tiles, and growth enrichment.
- Added a WPF icon loader using fixed-drive/reparse checks, 16 MB raster limit, 64 px on-load decoding, `ExtractIconEx`, frozen images, `DestroyIcon` in `finally`, and a 256-entry file-version-bound cache.
- Updated the app tile template to show a real icon when available and the existing category-letter tile when any validation or decoding step fails. Visible/accessibility text remains path-free.
- Added source tests for parser allow/refuse cases, Marvis Builder propagation, tile/growth propagation, WPF binding, bounded cache, no execution/network authority, and native cleanup. Seven static checks passed; no build/test/GUI/native icon call ran.

## 2026-07-14 - Source-only home Agent next-action closure

- Audited the home health-finding controls and confirmed that explain/detail/plan ended in a text-only response even when the copy told the beginner to open another page.
- Added a closed `HomeAgentNavigationDestination` contract. Generic findings point to C-drive evidence; findings with an app target point to Applications and retain only the trimmed app name for later re-resolution.
- Added a first-visible `HomeAgentResponseNavigateButton` with a stable AutomationId. WPF maps only the closed destination enum to existing internal pages and still applies the internal navigation allowlist.
- Exact app targets pass through the existing current-inventory resolver; missing/duplicate/stale targets refuse and show a generic application-management action instead of selecting an app by guess.
- Added focused source contracts for panel order, handler authority, generic navigation, exact-app targeting, fallback behavior, and non-execution. Seven static checks passed; no build, test, GUI, scan, process, or real system operation ran.

## 2026-07-14 - Antivirus-gated executable verification resumed

- User confirmed corrected Huorong definitions are installed. The first compiler pass found Core CS1628 before test assembly generation; copying the canonical out value to a local fixed the lambda boundary without policy change.
- The second pass compiled every product project and found three CS8122 test-expression errors; equivalent null equality fixed the expression-tree incompatibility.
- The third narrow build passed with 0 warnings/errors. `Css.Tests.dll` remained present, 939,520 bytes, and SHA-256 stable at `4DC676881A27669922207E33D482439AE0882F18CA314D141057BE3359FF5520` through a 20-second observation window; no new Huorong alert was observed.
- The first focused beginner-workflow run passed 249/252. Its three failures were stale static expectations: a renamed local, an unsafe unconditional remember-button expectation, and the old `关闭自启动` label. Tests were updated to the current stable-identity gate and `管理自启动` wording.

## 2026-07-14 - Installer launch readiness connected

- Replaced the antivirus-era hard-coded disabled flag with a typed Windows readiness policy. Windows builds are available by default; `OMNIX_ENTROPY_DISABLE_INSTALLER_LAUNCH=1/true/yes/on` is a fail-closed emergency stop that preserves analysis and route advice.
- Added a preparation readiness layer so a launchable package also needs an available OMNIX-managed non-system target before snapshot work or the four-item final-consent window begins.
- Restricted package analysis, descriptor parsing, and the production launcher to fully qualified files on ready fixed drives. The package file and every ancestor directory must be free of reparse points; UNC, relative, alternate-stream, mapped/non-fixed, missing, and redirected paths refuse.
- Required `InstallerLaunchOperationPlanner` and the handler to receive and independently enforce an `IInstallerTargetPathPolicy`. Existing hash, length, timestamp, trusted signature, type, snapshot, 15-minute consent, argument allowlist, and launch-time hash checks remain intact.
- Initial installer-focused tests passed 86/86. A later build after target preflight passed with 0 warnings/errors. The pre-incident compiled regression passed 586/586 after excluding one obsolete static source assertion; current source corrects it and adds two target-refusal tests, but those additions are not compiled because an accidental restore invalidated NuGet assets.
- No installer, MSI association, UAC prompt, registry/service/task change, cleanup, migration, or real C/D mutation ran. Static gates and strict UTF-8 over 238 source/XAML files pass.

## 2026-07-14 - Migration production path audit

- Confirmed the application drawer already creates rollback-manifest and snapshot evidence, evaluates execution readiness, requires a dedicated final-consent window, composes an authenticated elevated request, and routes through the signed Worker lifecycle coordinator.
- Confirmed the unsigned development package refuses before UAC or filesystem movement. This is a release-signing gate, not a hidden feature flag; no development bypass was added.

## 2026-07-14 - Personal large-file and duplicate-candidate diagnosis

- Added explicit file identity to scan-tree nodes and a bounded analyzer over only the current user's Desktop, Downloads, Documents, Pictures, Videos, and Music roots.
- Added conservative long-unused large-file and same-name/exact-size duplicate candidates. No hashes or file-content reads are performed, and duplicate findings are always labeled `疑似`.
- Added a path-free beginner presenter, home health aggregation, C-drive summary/list bindings, and stable AutomationIds. Personal-file findings remain observe-only and cannot create operations.
- Repaired malformed Chinese text found in the health-summary and Home Agent presentation source before static acceptance.
- Added focused test source for scope, file identity, thresholds, duplicate semantics, bounds, privacy, non-execution, and WPF wiring.
- Static gates passed: 340 strict UTF-8 files, MainWindow XML, 42 event handlers, 122 unique AutomationIds, mojibake audit, read-only authority audit, and placement order. Compilation and screenshot remain deferred by the NuGet restore blocker.

## 2026-07-14 - Quarantine governance audit started

- Confirmed the 30-day/20-GB retention planner only generates advice and the regret-center UI only displays that summary.
- Identified missing enforceable pieces: option validation, overflow-safe totals, truthful active/projected bytes, and an explicit-confirmation permanent-cleanup path with confinement revalidation and non-restorable timeline evidence.
- No quarantine file, manifest, timeline entry, or real system state was changed during the audit.

## 2026-07-14 - Quarantine retention and capacity governance connected

- Hardened retention planning with validated 1-3650 day policy, positive capacity, a 100-candidate batch limit, saturating totals, active/reclaimable/projected bytes, and explicit truncation. Automatic deletion remains hard-coded false.
- Added path-free regret-center status and candidate rows showing current active use, projected release, remaining rollback content, and permanent-loss warnings.
- Added manifest trust validation used by both restore and purge: bounded `manifest.json`, exact recorded path, id/item-root relationship, immediate-child payload, local non-ADS original path, root confinement, and existing-chain reparse refusal.
- Added bounded iterative purge deletion that only touches the validated payload and manifest under one item root. Partial outcomes are treated as possibly changed and audit recording uses a non-cancellable token after irreversible work begins.
- Added a manual-only Medium-risk purge descriptor, 100-manifest limit, explicit non-rollback/no-snapshot semantics, preflight of the whole batch, `SafetyOperationPipeline`, and `NotRestorable` timeline evidence.
- Added a dedicated confirmation window with stable AutomationIds, first-visible red warning, projected effects, folded manifest details, acknowledgement checkbox, and disabled-by-default confirm button.
- Added focused governance test source. Static gates passed and the pre-incident compiled regression passed 586/586 with the known obsolete installer source assertion excluded. No real quarantine item was purged or restored.

## 2026-07-14 - Local health digest history connected

- Added a bounded SQLite health-digest store keyed by stable scan identity, with path-text refusal, per-row corruption tolerance, 90-record retention, and upsert semantics.
- Added path-free digest construction plus daily and weekly presentation from actual successful user-initiated scans only. Empty/history copy explicitly says there is no background scheduled scan.
- Wired successful snapshot/session application to digest save and added a compact Home history section with stable AutomationIds. Digest persistence failure does not convert a successful scan into a failed scan.
- Added focused digest test source and static placement/authority checks. No scheduler, cloud upload, operation, or real system mutation was added.

## 2026-07-14 - Migration final-consent dead end closed

- Audited the drawer-to-Worker path and found that rollback/snapshot evidence refresh never enabled the migration request, while the gate duplicated acknowledgements already owned by the final-consent window.
- Made the gate evaluate only machine-verifiable readiness before final consent. Evidence creation now enables only the transition to final consent; the final window still requires plan, app-close, rollback, and monitoring acknowledgements.
- Kept the signed app/Worker trust gate unchanged. Unsigned development packages still refuse before UAC or filesystem movement.
- Returned production completion to MainWindow so successful migration triggers a fresh application scan; canceled/refused/failed paths no longer show a false success or a false permanent-disable message.
- Corrected staged migration/uninstall copy and focused source tests. Static gates passed: 349 strict UTF-8 files, 14 XAML files, 108 event bindings, 251 unique per-window AutomationIds, and migration authority invariants. No real operation ran.

## 2026-07-14 - Migration closure monitoring surfaced and made actionable

- Audited the production Worker, monitoring store, closure monitor, and MainWindow and confirmed that successful migrations wrote closure records that the app never read.
- Split `IMigrationPathObserver` from mutation authority and added `WindowsMigrationPathObserver`; WPF can observe redirect state but cannot move, roll back, delete, or create links.
- Added latest-per-software monitoring with 64-software/32-path bounds and rejected UNC, non-C source, non-D target, root, duplicate, or malformed observation records before any path probe.
- Added path-free beginner summaries for healthy, original-write-returned, target-changed, and original-path-missing states. Raw paths and low-level summaries are never used by the presenter.
- Wired explicit health/app scans and successful migration refreshes into Home, app ordering/tags, catalog summary, drawer advice, and migration review. No scheduler or background-monitor claim was added.
- Duplicate app names are not resolved by guesswork. Closure findings can navigate only when current inventory has one exact app-name match.
- Closed the already-on-D dead end: an abnormal closure enables `复查迁移`, prefixes the safety plan with fresh-scan/snapshot/rollback steps, and never turns the old monitoring record into direct execution authority.
- Added focused source/tests for latest selection, severity, privacy, malformed record refusal, idempotent Home enrichment, first-visible ordering, read-only WPF authority, and non-executable review.
- Static verification: 351 strict UTF-8 files, 14 XAML files, 58 unique handlers, 251 unique AutomationIds, and all scoped authority/privacy/bounds/order checks passed. A Core no-restore build stopped before compilation on the known missing NuGet assets. No real C/D path, link, application, service, registry, task, installer, uninstall, or UAC operation ran.

## 2026-07-14 - Truthful whole-PC health dimensions connected

- Audited Home and found that the promised whole-PC summary contained only overall score and C-drive health.
- Added a data-only machine-health contract and `WindowsMachineHealthProbe`. It reads only a ready local fixed D drive, `GlobalMemoryStatusEx`, count-only/disposed process handles, and optional `GetSystemPowerStatus` data.
- The observation model stores no process name, executable path, window title, registry data, service identity, or operation authority.
- Extended the health table with D-drive space, memory/process count, battery status, startup inventory signals, and manual-scan usage trend. Missing D, no battery, failed reads, unscanned apps, and insufficient history have distinct plain-language states.
- Usage trend requires at least three real manual snapshots; startup remains explicitly a signal because effective Windows enablement is not inferred.
- Kept the existing disk-pressure score unchanged and labeled it `当前按磁盘空间` instead of silently treating one-time memory/battery samples as score inputs.
- Added local Agent intent for D drive, memory, battery, process, performance, and lag questions. Without a health summary it refuses to guess; with evidence it quotes only path-free dimensions and warns against bulk process/service changes.
- Reworked the Home table into a 260px vertical-scroll surface with wrapped cells and no horizontal scrollbar. Added stable AutomationIds to Home, Timeline, and Agent navigation buttons.
- Added `MachineHealthExperienceTests` source for real/unavailable/not-present states, score/authority stability, privacy, history thresholds, Agent answers, read-only probe ownership, and WPF wiring.
- Static verification: 354 strict UTF-8 files, 14 XAML files, 58 unique handlers, 254 unique AutomationIds, touched-copy mojibake check, and scoped machine-health/Agent/UI invariants passed. Compilation and GUI proof remain blocked by the recorded NuGet restore issue. No system operation ran.

## 2026-07-14 - Bounded post-install C-drive footprint evidence connected

- Audited installation snapshots and found that they contained only `SoftwareProfile` inventory, so an unregistered installer or unattributed AppData/ProgramData landing point could receive a false clean report.
- Added `InstallFootprintCapture` and `WindowsInstallFootprintProbe`: fixed common local C roots only, immediate children only, maximum 4096 entries, maximum eight supplied roots, reparse entries skipped, and no content reads or mutation APIs.
- Bumped install-before evidence to schema 2 and bound footprint status, count, and an order-independent SHA-256 fingerprint. The coordinator refuses a mismatched in-memory before snapshot before the fake/real launcher boundary.
- Automated preparation, manual before/after capture, and coordinator post-scan now use the same probe. Complete captures merge unregistered landing-point candidates with software-inventory evidence; incomplete captures do not contribute uncertain path differences.
- Updated report cards, Agent explanation, evidence review, action plan, and candidate preview. First-level copy is count-only/path-free, says candidates are not proven installer ownership, and refuses concrete previews while observation is truncated or unavailable.
- Added `InstallFootprintExperienceTests` and coordinator tests for unregistered paths, incomplete evidence, fingerprint safety, source bounds, shared wiring, before-snapshot mismatch, and post-scan use.
- Huorong definitions were updated and normal NuGet restore succeeded. Test-project and solution builds passed with 0 warnings/errors; installer-focused tests passed 52/52; full regression passed 623/623.
- Full regression exposed two accumulated issues from the source-only period: `电脑为什么卡` was not recognized as machine health, and one uninstall assertion expected obsolete `只预览` copy. Added ordinary-language intent phrases and aligned the assertion with the existing `先完成恢复准备` safety gate; focused rerun passed 5/5.
- Static gates passed for 257 non-generated strict UTF-8 C#/XAML files, 14 XAML parses, 58 handlers, 254 unique AutomationIds, no forbidden probe APIs, and no leftover fixture process/directory.
- Fixture-only install WPF smoke passed twice: four cards, four Agent steps, three plan items, collapsed technical details, hidden identifiers, and preview-only actions. Report/action-plan screenshots are clean; the standalone Agent desktop screenshot still has compositor black areas and remains a visual warning. No installer, UAC, cleanup, migration, uninstall, registry/service/task change, or real C/D mutation ran.
## 2026-07-14 - Migration-closure beginner GUI acceptance started

- Resumed after the user confirmed Huorong definitions were updated; kept the persistent objective `把关键功能全部接通` active.
- Re-audited the monitoring store, read-only path observer, Home health projection, app ordering/tagging, drawer advice, and software-fixture scanner.
- Chose a fixture-only proof: one ordinary unique directory under `C:\tmp` represents a returned write, one D-drive directory represents the expected target, and isolated `.omx` data/software fixtures feed the app. No redirect, migration, rollback, installer, uninstall, registry, service, or task operation will run.
## 2026-07-15 - Reversible startup-control slice started

- Audited the drawer action path and confirmed `管理自启动` currently ends at the allowlisted Windows Startup Apps settings page.
- Reused the scanner's exact structured Run identity and read-only StartupApproved observation; rejected direct service/task/HKLM/system mutation for this slice.
- Defined acceptance around fresh value/type/ACL evidence, an atomic rollback manifest, explicit confirmation, pipeline execution, timeline restore, and cancel-only GUI verification.

## 2026-07-15 - Startup-control backend implemented

- Added fresh exact-value/type/ACL capture, tamper-evident bounded rollback manifests, a medium-risk operation descriptor/handler, automatic restore when timeline journaling fails, and restorable startup timeline entries.
- Added the production Win32 adapter scoped to exactly `HKCU64\\Software\\Microsoft\\Windows\\CurrentVersion\\Run`; `StartupApproved` remains read-only evidence, and HKLM, services, and scheduled tasks have no mutation authority.
- Focused Core/timeline verification passed 19/19 and Core/Win32 startup verification passed 11/11. Tests used injected stores/backends and did not change the real registry.
- Began the WPF connection phase: local review is available only for one uniquely matched fresh entry; all unsupported or ambiguous cases retain the Windows-settings handoff.

## 2026-07-15 - Reversible startup control connected and visually verified

- Added a path-free local drawer review, dedicated two-acknowledgement confirmation, fresh pre-confirmation rescan, strict uncommitted-manifest cleanup on cancel, pipeline execution, completion/refusal states, and application rescan.
- Added operation-kind restore dispatch and a dedicated startup restore confirmation. Quarantine restores retain their own path semantics; startup restore explains next-login behavior and refuses overwrite on collision or permission drift.
- Added `OMNIX_ENTROPY_STARTUP_FIXTURE`, an exact-match in-memory adapter for GUI smokes, plus a Core-pipeline startup timeline seed command. Neither fixture contains registry/process authority.
- The first GUI run exposed a UTF-8 BOM bug in the fixture reader before the main window opened. Runtime-event inspection found the exact stack; text deserialization plus a BOM regression test fixed it.
- Focused startup/app/timeline tests passed 42/42; full regression passed 646/646; solution build passed with 0 warnings/errors. Static gates passed for 16 XAML parses, 372 strict UTF-8 files, 265 unique AutomationIds, exact startup authority, and fixture/process cleanup.
- `.omx/gui-startup-control-cancel-smoke.ps1` proved local review, disabled confirmation, three first-view outcomes, collapsed details, 1-to-0 uncommitted manifest cleanup, second-review reachability, and no execution.
- `.omx/gui-startup-restore-cancel-smoke.ps1` proved path-free restore confirmation, retained manifest and enabled timeline record after cancel, and no restore execution. Screenshots were visually inspected and clean.

## 2026-07-15 - Quarantine candidate identity and post-confirmation revalidation

- Audited the shared quarantine handler and found that it checked only existence after consent; the file service did not reject reparse parents or bind the confirmed object identity.
- Added a bounded preparation contract for at most 64 exact local candidates. It rejects UNC/ADS, disk roots, protected Windows/program/user-data roots, duplicate/overlapping paths, source/quarantine overlap, and reparse chains.
- Added a Windows handle-based identity reader that binds canonical path, file/directory type, volume serial, file id, creation time, last-write time, and file length. All three WPF quarantine entry points prepare this evidence before showing confirmation.
- Confirmation now refuses unprepared descriptors. The handler performs whole-batch identity preflight before moving anything; `FileQuarantineService` checks again after writing the recovery manifest and immediately before each move.
- TDD exposed an incorrect native `FILETIME` layout and immediate NTFS file-id reuse. Corrected the native structure and retained creation/write/length metadata in the identity comparison.
- Related tests passed 33/33; full regression passed 653/653; solution build passed with 0 warnings/errors; 375 strict UTF-8 files and 16 XAML parses passed.
- Fixture-only C-drive GUI smoke opened the real confirmation and clicked cancel. The fixture remained, quarantine stayed empty, and `.omx/qa-cdrive-cleanup-confirmation.png` was visually inspected. No real cleanup or C/D mutation ran.

## 2026-07-15 - Agent startup advice synchronized with reversible local control

- Added a shared presentation-only eligibility check for exactly one supported ordinary current-user Run observation on a non-system app. It grants no execution authority and does not replace fresh preparation.
- Updated aggregate startup answers, exact-app answers, background review, and startup/service plan copy to distinguish local reversible review from name-only, multiple, system, service, and task evidence.
- Added fixture-only WPF proof that asks about one exact app, verifies path-free rollback/fresh-read wording, navigates to the app drawer, observes an enabled startup review button, and executes nothing.
- Corrected Windows PowerShell 5 parsing by keeping the smoke source ASCII and constructing Chinese assertions from Unicode code points. Replaced oversized-panel `BringIntoView` with `ScrollToTop` so the first Agent title and conclusion are not clipped.
- Focused Agent tests passed 15/15; full regression passed 658/658; solution build passed with 0 warnings/errors; 274 non-generated source/XAML files passed strict UTF-8 and 16 XAML files parsed. `.omx/qa-agent-startup-advice.png` was visually inspected and clean.

## 2026-07-15 - Home whole-PC health runtime and first-view acceptance

- Ran the existing fixture Home flow and confirmed production read-only observations returned plausible D-drive, memory, process-count, battery, startup-clue, and manual-history results.
- Found that the old development scan fixture lived on D while the UI labels the scan as C, making the C and D percentages accidentally identical; the old screenshot also included the surrounding desktop.
- Confined the scan fixture to a unique `C:\tmp\OMNIX-HomeHealth-Smoke-*` directory, added stable row AutomationId/content/privacy/offscreen assertions, captured only the OMNIX window, and recorded `noOperationExecuted=true`.
- Runtime output showed seven dimensions, including D 69.3% with 200.2 GB free, memory 13.4/31.3 GB with 278 count-only processes, battery 100% on AC, 10 ordinary startup clues, and one manual check. No process names or private paths were displayed.
- Focused tests passed 5/5; full regression passed 658/658; solution build passed with 0 warnings/errors; 274 non-generated strict UTF-8 files and 16 XAML parses passed. The exact C fixture and `Css.App` process were absent after cleanup; `.omx/qa-home-agent-next-action.png` was visually inspected and clean.

## 2026-07-15 - Personal large-file and possible-duplicate GUI acceptance

- Added a process-scoped personal-root fixture under one GUID-named `C:\tmp` directory and fixture-only low thresholds. Production personal roots and default 512 MB/64 MB thresholds are unchanged.
- Added exact Home-to-C-drive personal-candidate navigation, stable item AutomationIds, and a 240px list that keeps both bounded fixture conclusions visible.
- Added `BeginnerTextSanitizer` at the recommendation presentation boundary. Visible cards replace local paths with `某个本机位置`; `OperationDescriptor.AffectedPaths` remains unchanged for confirmation and execution evidence.
- Strengthened the WPF smoke to require both candidate items onscreen and scan the full visible OMNIX window for fixture paths. Screenshot review confirmed the long-unused and possible-duplicate conclusions are readable and explicitly non-executable.
- Focused tests passed 29/29; full regression passed 661/661; solution build passed with 0 warnings/errors. Strict UTF-8 passed for 275 non-generated C#/XAML files, all 16 XAML files parsed, no `Css.App` process or fixture remained, and `.omx/qa-personal-storage-candidates.png` was visually inspected.

## 2026-07-15 - Install-report exact application handoff started

- Audited the install-change report's eligible-action and candidate-preview flow. Ready cache/startup/migration previews can prove one unique new software owner, but the panel has no next step after `查看方案预览`.
- Chose an internal-navigation-only closure: carry the unique app target in the preview, resolve it against current inventory, and open the existing app drawer. Existing drawer preparation, confirmation, rollback, and pipeline controls remain the only execution path.
- Refused and guidance-only previews, stale/missing inventory, and duplicate names will remain non-navigable and non-executable.

## 2026-07-15 - Install-report exact application handoff completed

- Added navigation metadata only to ready cache/startup/migration previews with one unique added software profile. Refused and guidance-only states retain no target and `CanNavigateToApp=false`; every preview remains `CanExecuteDirectly=false`.
- Added one WPF internal-navigation button that uses the existing exact target resolver/current-inventory refresh and opens the existing app drawer. No install-report code creates an operation or calls the safety pipeline.
- The first passing handoff smoke screenshot showed the new button below the visible preview. Moved it directly below the Agent conclusion and strengthened the smoke to require both conclusion and button in the actual install-page viewport before capture.
- The isolated two-scan fixture reached the exact `New Fixture Tool` drawer and its normal cache-review entry without invoking it. Focused tests passed 24/24; full regression passed 661/661; solution build passed with 0 warnings/errors; 275 strict UTF-8 files and 16 XAML parses passed; process and fixture cleanup were empty.

## 2026-07-15 - Natural-language settings and troubleshooting routing started

- Compared the Agent conversation intents with the visible Marvis-inspired skill catalog. The UI promises settings/troubleshooting/tool guidance, but questions about Wi-Fi, sound, display, drivers, crashes, or named Windows tools currently fall through to the generic answer.
- Chose to reuse the existing fixed shortcut catalogs and confirmation-aware openers. The Agent may carry only an enum plus a catalog id; it will never accept a command or URI from question text.
- Defined the ordinary-development boundary as answer/navigation proof plus cancel-only confirmation for protected tools; no external setting or tool action will modify the system.

## 2026-07-15 - Natural-language settings and troubleshooting routing completed

- Added fixed local intents for Windows settings, troubleshooting, and named system tools. Replies carry only `AgentShortcutKind` plus a catalog id; question text can never become a command or URI.
- Reused the existing allowlisted settings/tool openers. High-risk tools retain their confirmation and every Agent reply remains local, non-cloud, and non-executable.
- Added focused coverage for network, Bluetooth, sound, display, power, drivers, crashes, blue screens, and registry-editor wording, plus WPF authority and GUI-smoke source contracts.
- The first GUI attempt missed the native MessageBox in the normal UIAutomation child tree. A broad native-window retry then selected a hidden same-process window; screenshot inspection rejected that mechanical pass. The final smoke matches the exact confirmation title, captures it, and closes the window as cancel without invoking either button.
- Focused tests passed 20/20; full regression passed 671/671; solution build passed with 0 warnings/errors; strict UTF-8 passed for 275 non-generated C#/XAML files and all 16 XAML files parsed. Both screenshots were visually inspected; no `Css.App`, `mmc`, or fixture state remained.

## 2026-07-15 - Beginner hardware configuration answer started

- Audited the Marvis-inspired skill catalog and found `电脑配置查询` has no corresponding hardware intent or CPU/GPU evidence; the existing machine probe covers only D-drive capacity, memory, process count, and battery.
- Chose a separate bounded read-only hardware probe and plain summary. It will not query serial numbers, usernames, domains, device ids, or infer specific game/software compatibility without requirements.
- Ordinary verification is read-only real-machine observation plus isolated app data; no driver, process, registry, service, task, file, installer, migration, or session operation is allowed.
- Implemented the bounded hardware model/probe, manual-scan propagation, hardware conversation intent, and truthful catalog wording. Focused hardware/machine/Agent tests pass 28/28.
- The first dedicated WPF smoke request was rejected by the Codex GUI approval quota before process launch. No workaround or unapproved visible process was attempted; runtime probe tests and static/full verification will continue, with screenshot proof retained as Warn.

## 2026-07-15 - Beginner hardware configuration answer completed with visual Warn

- Added a path-free `HardwareSummaryObservation`, propagated it from the manual machine scan through health/migration enrichment, and added a dedicated `HardwareInfo` conversation intent.
- The probe uses three fixed, two-second WMI queries with bounded result counts. Real-machine testing exposed WMI access denial in the restricted process, so CPU falls back to one fixed read-only hardware registry value plus `Environment.ProcessorCount`, and GPU falls back to bounded `EnumDisplayDevices`; no serial, user, domain, PNP/device id, path, or write API is queried.
- Updated skill wording to promise only CPU, GPU, memory, and Windows evidence. Specific software/game suitability now explicitly requires official minimum/recommended requirements.
- Added `.omx/gui-agent-hardware-summary-smoke.ps1` and its source gate/documentation. Its visible execution was rejected by Codex quota before launch, so no screenshot pass is claimed.
- Real-machine hardware tests passed 5/5; full regression passed 676/676; solution build passed with 0 warnings/errors; 278 strict UTF-8 source/XAML files and 16 XAML parses passed; process and fixture cleanup were empty.

## 2026-07-15 - Interactive truthful Agent skill catalog started

- Audited the eight visible skill cards and confirmed their `下一步` lines are inert text with no handler. This reproduces the beginner problem of seeing capabilities without knowing how to use them.
- Chose one compact `问 Agent` action per card. The action only renders a local overview reply from current evidence; unsupported desktop/session categories must say they are unavailable instead of creating an operation or external command.
- Existing response rendering and internal allowlists remain the only next-step mechanisms; card clicks are never consent to modify the system.

## 2026-07-15 - Interactive truthful Agent skill catalog completed with visual Warn

- Added `AgentConversationPresenter.ExplainSkill` for all eight categories and reused the existing Agent response renderer. No card handler performs navigation, process launch, settings opening, operation creation, or pipeline execution.
- Added one compact `问 Agent` button per card with category-bound stable AutomationId. Current diagnosis/background/hardware categories use local evidence; settings/troubleshooting/tools explain how to choose the safe existing entry.
- Window/desktop replies state that no window title, desktop icon, or display-layout evidence is read. Input/session replies state that lock/sleep/shutdown/restart execution is not provided.
- Added an isolated no-operation skill-card WPF smoke and documentation. Visible execution remains blocked by Codex quota, so the visual gate stays Warn.
- Focused tests passed 25/25; full regression passed 679/679; solution build passed with 0 warnings/errors; 278 strict UTF-8 files, 16 XAML parses, 82 resolved handlers, unique literal IDs, and clean process/fixture checks passed.

## 2026-07-15 - MSIX managed-storage handoff started

- Audited the trusted MSIX path and found a visible dead end: policy correctly refuses arbitrary target arguments and installer launch, but the UI only says to open Windows new-app storage settings while exposing no action.
- Verified from Microsoft Learn that `ms-settings:savelocations` is the documented Default Save Locations URI.
- Chose a fixed catalog-id handoff with the existing confirmation-aware settings opener. The current analyzed capability must still match; no URI comes from the package or user text, and the MSIX itself will not be launched.

## 2026-07-15 - MSIX managed-storage handoff completed with visual Warn

- Added a typed fixed settings handoff only for trusted MSIX evidence. Refused, unknown, MSI, Burn, Inno, and NSIS states do not receive the handoff id.
- Added the Microsoft-documented `ms-settings:savelocations` destination to the open-only settings catalog with Medium risk and mandatory confirmation. Natural-language matching returns only the fixed catalog id.
- The install panel now says Windows manages MSIX location, suppresses the arbitrary D-path recommendation, disables route memory and installer preparation, and shows a stable `打开新应用保存位置` button. Its handler revalidates the current capability and calls the existing allowlisted opener; it has no direct process, operation, file, registry, service, task, or pipeline authority.
- TDD red first failed on the missing capability property/state/id. The first green run exposed one obsolete route-memory static assertion and one overly literal copy assertion; both were corrected to the intended safer contract.
- Focused tests passed 222/222; full regression passed 682/682; solution build passed with 0 warnings/errors. Static gates passed for 278 strict UTF-8 source/XAML files, 16 XAML parses, 61 unique resolved event handlers, unique literal AutomationIds, and allowlisted-only handler authority.
- Computer Use reached the Windows helper but timed out twice on `launch_app`; no OMNIX target window or process appeared. The run stopped without a PowerShell/SendKeys fallback, so first-view/cancel screenshot proof remains Warn.

## 2026-07-15 - Recycle Bin review handoff started

- Audited C-drive root-cause cards and confirmed `BigRocksProbe` reads the current-user C-drive Recycle Bin size, but the presenter labels it as generic `系统保留空间` and offers no next step.
- Chose an open-only handoff: a specialized beginner card may open the Windows Recycle Bin through one fixed catalog entry. OMNIX will not empty, delete, restore, quarantine, or move anything.
- The phrase `清空回收站` is not deletion consent. Agent routing must convert it into a review-only answer that explicitly says the button only opens the Recycle Bin.

## 2026-07-15 - Recycle Bin review handoff completed with visual Warn

- Specialized positive Recycle Bin big-rock evidence into a plain-language card. Pagefile, hibernation, and shadow-storage cards remain generic and have no action.
- Added one fixed `recycle-bin` system-tool entry using `explorer.exe` plus the Windows `RecycleBinFolder` shell namespace. The catalog copy and Agent answer state that OMNIX only opens the view and never clears, deletes, or restores files.
- Added a conditional stable card button and a typed fail-closed handler that accepts only `CDriveRootCauseAction.OpenRecycleBin`, then reuses the existing allowlisted opener. No operation or deletion authority was added.
- TDD red failed on the intentionally missing action fields/catalog id. Focused tests passed 5/5, related tests 191/191, full regression 686/686, and the solution built with 0 warnings/errors.
- Static checks passed for 278 strict UTF-8 files, 16 XAML parses, 62 resolved handlers, unique literal AutomationIds, and no source `SHEmptyRecycleBin` reference. Computer Use timed out on the bounded app launch and no process appeared, so screenshot proof remains Warn.

## 2026-07-15 - C-drive root-cause safe internal handoffs started

- Audited the other beginner root-cause cards after closing the Recycle Bin path. User files, programs, app data, and normal temp cards explain the next destination in prose but expose no action.
- Chose typed internal handoffs to existing evidence surfaces: personal-file candidates, the C-drive app filter, and the first actionable cleanup recommendation.
- Unexpected roots and Windows-managed stores remain actionless. Selecting a recommendation is not confirmation and must never invoke the cleanup handler or pipeline.

## 2026-07-15 - C-drive root-cause safe internal handoffs completed with visual Warn

- Added typed actions for ordinary user-profile, program/app-data, and temp cards, plus deterministic action-specific AutomationIds. Unexpected roots and Windows-managed categories still have no button.
- Program/app-data opens the existing `占 C 盘` application catalog; user files focus the read-only large-file candidates; temp selects the first recommendation already marked actionable by the existing policy.
- The expanded handler revalidates the card/action pair and contains no operation, confirmation, pipeline, execution-handler, process, or file-mutation call. Selecting a recommendation only prepares the existing explanation and keeps its second confirmation intact.
- TDD red first failed on the missing enum values and AutomationId property. One green run exposed an imprecise test prefix that matched both `Windows` and `Windows Temp`; the selector was narrowed without changing product behavior.
- Focused tests passed 3/3; product tests 166/166; full regression 687/687; solution build 0 warnings/errors; 278 strict UTF-8 files, 16 XAML parses, 62 resolved handlers, unique literal IDs, and navigation-only handler authority passed. Visual proof remains Warn due the already-recorded Computer Use launch failure.
- A post-gate accessibility audit found that action-only runtime AutomationIds could repeat when program and app-data cards were both visible. The ids now append a deterministic, path-free eight-hex hash of the visible top-level name; tests require uniqueness and stability across repeated builds. Final full regression/build/UTF-8/XAML gates were rerun and passed.

## 2026-07-15 - Beginner-first installer monitoring started

- Audited the installation page and confirmed the primary `PrepareInstaller_Click` path already captures before evidence, obtains final consent, launches through the production coordinator, performs the initial after scan, and renders the change report.
- The visible `捕获安装前` / `捕获安装后` / `生成变化报告` controls duplicate that automatic path and make a beginner perform an engineering fixture workflow.
- Chose to preserve the controls only as an explicitly expanded advanced diagnostic surface. The ordinary page will state that normal installation records changes automatically; no execution or evidence authority changes.

## 2026-07-15 - Beginner-first installer monitoring completed with visual Warn

- Added a visible statement that the normal confirmed installer flow records before/after changes automatically. The manual three-step comparison now lives in a default-collapsed `高级诊断：手动变化对比` expander with a stable AutomationId.
- Renamed the advanced buttons to `记录安装前状态`, `记录安装后状态`, and `对比变化`. The isolated fixture smoke explicitly verifies the collapsed default and expands the region before using those controls.
- TDD red failed on the missing automatic-monitoring text and expander. Focused tests passed 2/2, related product/installer tests 240/240, full regression 700/700, and the solution built with 0 warnings/errors.
- Static gates passed for 282 strict UTF-8 files, 16 XAML parses, 120 resolved event bindings, unique literal AutomationIds, and a parse-clean PowerShell smoke. Visual proof remains Warn due the already-recorded Computer Use launch failure.

## 2026-07-15 - Agent automatic read-only evidence hydration started

- Audited `AskComputerAgent_Click` and confirmed it answers immediately from the current `_softwareProfiles`; when empty, application/startup answers tell the beginner to go scan manually even though a shared automatic read-only inventory gate now exists.
- Chose intent-scoped hydration: application/C-drive/startup/migration/uninstall/general questions and the process/service skill may await the shared inventory gate. Settings, tools, hardware, installation routing, restore, troubleshooting, and empty questions do not trigger an unrelated scan.
- This is evidence preparation only. Agent answers remain non-executable and cannot call an operation pipeline or system mutation path.

## 2026-07-15 - Agent automatic read-only evidence hydration completed with visual Warn

- Added pure question/skill evidence-needs rules. C-drive/application/startup/migration/uninstall/general questions and the process/service skill request inventory; empty/settings/tool/hardware/install/restore questions do not.
- Converted the Ask and skill-card handlers to await the existing `SoftwareInventoryLoadGate` before rendering a reply, with disabled controls restored in `finally` and path-free scan failure behavior inherited from the shared loader.
- TDD red first proved the missing async orchestration. Two local source-extraction helpers exposed unsafe `-1` slicing when signatures changed; both now validate their start boundary, and the repeated lesson was promoted to `skill-candidates.md`.
- Focused tests passed 13/13, related tests 222/222, full regression 713/713, and build completed with 0 warnings/errors. Static gates found no process, pipeline, operation, file-move/delete, or registry-write authority in the Agent handlers.

## 2026-07-15 - Agent-triggered C-drive diagnosis completed with visual Warn

- Extracted `ReadOnlyEvidenceLoadGate` and kept `SoftwareInventoryLoadGate` as a behavior-compatible wrapper. Homepage refresh and Agent ensure requests now share in-flight read-only health work.
- Added a pure policy that triggers a full diagnosis only for explicit C-drive intent when `_lastHealthSummary` is absent. Software attribution is prepared first, then the new health summary is used for the final answer.
- A successful empty software inventory is no longer rescanned inside health diagnosis; failed inventory remains eligible for one attribution retry. Cancelled/failed health scans return `false`, stay retryable, and show no exception/path detail.
- TDD red failed on the intentionally missing gate and policy. Focused tests passed 14/14, related 226/226, full 722/722, build 0 warnings/errors, and static authority/privacy gates passed.

## 2026-07-15 - Automatic undo-center history loading started

- Confirmed Timeline already fires a read on navigation, but every repeated entry starts another load, the visible button still says `加载时间线`, and failure presentation concatenates `ex.Message`.
- Chose ensure-on-navigation plus force-refresh for the button and post-operation callers, all through the reusable read-only evidence gate. Restore and permanent cleanup authority will not move into loading.

## 2026-07-15 - Automatic undo-center history loading completed with visual Warn

- Added a dedicated `ReadOnlyEvidenceLoadGate` instance for timeline reads. `ShowPage("Timeline")` now ensures once; the refresh button and all existing `LoadTimelineAsync` post-operation calls force refresh while joining any in-flight load.
- Changed the visible action to `重新加载` and stated that entering the page reads recent safe operations automatically. Successful empty history is a completed load; failed reads return false and can retry.
- Removed `ex.Message` from beginner timeline output. Loading contains no restore, permanent-purge, operation-pipeline, or delete authority.
- TDD red failed on the missing ensure/core methods. Focused tests passed 11/11, related 212/212, full 723/723, build 0 warnings/errors, and static UTF-8/XAML/event/id/authority gates passed.

## 2026-07-15 - Agent lightweight machine observation started

- Audited `WindowsMachineHealthProbe`: it already reads bounded D-drive capacity, aggregate memory, process count, battery, and path/identifier-sanitized hardware without process names or mutation APIs.
- Rejected building a fake `HealthCheckSummary` for a machine-only observation because its score is explicitly disk based. Agent will accept optional machine evidence separately from the full health summary.
- Chose to move reusable D-drive/memory/battery formatting into Core so full health and lightweight Agent answers cannot disagree.

## 2026-07-16 - Cache-cleanup post-attempt synchronization started

- Traced app-cache cleanup from drawer evidence through final confirmation, current-profile revalidation, specialized quarantine handler, safety pipeline, Undo Center, and application rescan.
- Confirmed success refreshes both state surfaces, but a failed pipeline result returns before either refresh and an unexpected exception after invocation reaches a catch that also performs no refresh.
- The underlying quarantine handler can record partially restorable entries when automatic rollback is incomplete, so any invoked pipeline is now treated as a read-only synchronization boundary; pre-execution refusals remain unchanged.

## 2026-07-16 - Cache-cleanup post-attempt synchronization implemented

- Added a read-only synchronization helper that refreshes Undo Center and attempts one current software scan while preserving the operation conclusion if either read is unavailable.
- The cache workflow now marks the exact pipeline boundary, synchronizes before interpreting every returned result, and uses the same recovery helper after an exception only when execution had begun and state was not already refreshed.
- Focused cache tests passed 6/6 and related cache/quarantine/timeline/product tests passed 213/213. Full solution gates are deferred until the adjacent startup-disable boundary is closed.

## 2026-07-16 - Startup-disable post-attempt synchronization started

- Traced startup disable through current app resolution, fresh preparation, rollback manifest, explicit confirmation, exact registry store, timeline journal, and result presentation.
- Confirmed a successful result refreshes the application list but not Undo Center, while a failed or thrown post-invocation outcome refreshes neither despite possible registry/timeline change.
- Chose the same attempt-boundary rule as cache cleanup, with extra protection that a manifest is no longer considered uncommitted once the pipeline has been invoked.

## 2026-07-16 - Startup-disable post-attempt synchronization implemented

- Added a read-only helper that attempts a current software/startup scan and Undo Center refresh without changing the operation outcome when either read is unavailable.
- The startup workflow now marks pipeline invocation, synchronizes every returned result before success/failure presentation, and repeats the same read-only recovery only for an unsynchronized thrown attempt.
- `confirmed` still becomes true before pipeline invocation, so any rollback manifest that may have participated in execution is never deleted as an uncommitted draft. Focused tests passed 8/8 and related tests passed 207/207.

## 2026-07-16 - C-drive cleanup post-attempt rescan started

- Traced direct decision-card cleanup through identity preparation, final confirmation, quarantine pipeline, timeline, and health recommendations.
- Confirmed success refreshes only Undo Center; failed returned results and thrown attempts refresh neither state surface, allowing moved/missing paths and reclaimable totals to remain stale.
- Chose one post-attempt helper that refreshes Undo Center and requests the existing full read-only health scan after pipeline invocation only; outcome copy remains downstream and authoritative.

## 2026-07-16 - C-drive cleanup post-attempt rescan implemented

- Added a read-only post-attempt helper that refreshes Undo Center and requests the existing full health scan, while preserving operation truth if either refresh is unavailable or cancelled.
- Every returned pipeline result now synchronizes before success/failure presentation; thrown attempts synchronize once when needed. Pre-confirmation exits perform no additional scan.
- The final button state is recalculated from the refreshed recommendation selection instead of being unconditionally enabled against an old card. Focused test passed 1/1 and related tests passed 243/243.

## 2026-07-16 - Uninstall-residue post-attempt synchronization started

- Traced low-risk residue quarantine from after-uninstall profile scan through risk grouping, identity preparation, final confirmation, pipeline, timeline, application catalog, and inline outcome.
- Confirmed failed pipeline results return before loading Undo Center, while success reloads timeline but refreshes the catalog from the pre-move profile snapshot.
- Chose one post-attempt helper that reloads timeline and attempts a fresh software scan after pipeline invocation only; the reviewed inline outcome remains presentation evidence rather than execution authority.

## 2026-07-16 - Uninstall-residue post-attempt synchronization implemented

- Added a read-only post-attempt helper that reloads Undo Center and attempts a fresh software scan, falling back to the last catalog when inventory is unavailable.
- Every returned residue-quarantine result now synchronizes before success/failure presentation; thrown attempts synchronize once when execution had begun. Pre-operation review/refusal/cancel paths retain their existing behavior.
- Focused residue tests passed 11/11 and related uninstall/quarantine/product tests passed 220/220.

## 2026-07-16 - Undo-Center mutation exception synchronization started

- Audited quarantine retention purge, ordinary quarantine restore, and startup restore after their safety-pipeline boundaries.
- All three refresh on ordinary returned results, but their catch paths do not reload current state after a possible partial mutation; startup restore also refreshes applications only on success despite post-write verification failure being possible.
- Chose per-method attempt/synchronized guards and one shared startup-state refresh helper. Catch paths will observe and stop, never retry purge or restore.

## 2026-07-16 - Undo Center and local mutation post-attempt synchronization completed

- Added attempt/synchronized recovery to quarantine purge, ordinary quarantine restore, and startup restore. Returned outcomes refresh before final copy; catch paths refresh once only after pipeline invocation and never retry mutation.
- Renamed the startup read-only helper for shared disable/restore use. Startup restore now refreshes applications and timeline for failed as well as successful returned results.
- Audited all seven direct MainWindow safety-pipeline methods: each contains one pipeline invocation, one attempt marker, one catch guard, and two synchronization call sites. Four read-only synchronization helpers contain no pipeline, quarantine, restore, purge, process, registry, or delete authority.
- Verification passed: focused cache 6/6, startup 8/8, direct cleanup 1/1, residue 11/11, Undo/startup 11/11; related groups 213/213, 207/207, 243/243, 220/220, and 220/220; final full 937/937; build 0 warnings/errors; 336 strict UTF-8 files with no replacement characters.

## 2026-07-16 - Shared source-method contract helper completed

- Added `SourceMethodExtractor`, which requires a full declaration prefix and extracts one balanced method body while ignoring braces inside ordinary/verbatim strings, characters, and comments.
- Migrated the new cache, startup, direct-cleanup, residue, and Undo Center synchronization contracts to the shared helper; three focused helper tests cover method isolation, ignored lexical braces, and missing/bare marker refusal.
- Final verification after the test-infrastructure change passed at 937/937 with a 0-warning/0-error solution build and 336 strict UTF-8 files.
## 2026-07-16 - One-shot migration submission audit started

- Audited production execution outside MainWindow after completing local post-attempt synchronization.
- Found that `MigrationPlanWindow` re-enabled the same request after any returned outcome and treated an unexpected coordinator exception as no attempt, allowing stale migration evidence to be reused and skipping MainWindow's post-attempt rescan.
- Scoped the fix to one-shot window state and conservative unknown-result presentation; no real migration will run.

## 2026-07-16 - One-shot uninstall/migration submission and rescan recovery completed

- Migration and official-uninstall plan windows now mark the coordinator boundary before awaiting it. The current reviewed request remains locked after every returned or unknown outcome, so stale snapshot/rollback evidence cannot be submitted twice.
- Added a beginner-facing unknown uninstall result that says the return was incomplete, refuses to claim success, promises a fresh application scan, and states that no automatic retry or residue cleanup will occur.
- Added one read-only application-rescan recovery helper for both parent workflows. A read failure preserves the old list, stops residue/closure inference, and keeps the operation conclusion visible instead of escaping through an `async void` handler.
- Verification: focused contracts 4/4; related uninstall/migration/product tests 432/432; final full 942/942; build 0 warnings/errors; 339 strict UTF-8 files; unknown-result WPF render verified and saved as `.omx/qa-uninstall-unknown-attempt.png`. No real uninstall or migration ran.

## 2026-07-16 - Real application-search placeholder started

- Audited the remaining visible V1 entry surfaces after production synchronization closed.
- Found that Application Management stores `搜索应用` as real TextBox content and relies on a compatibility exception in Core filtering. The list remains correct, but a beginner must erase instruction text before typing.
- Scoped a presentation-only fix: empty query, overlay hint, stable automation ids, and text-change visibility. Existing filter compatibility remains for older callers/tests.

## 2026-07-16 - Real application-search placeholder completed with visual Warn

- Replaced the literal `搜索应用` TextBox value with an empty fixed-size input and a non-interactive overlay hint. Added stable AutomationIds to both controls.
- The existing text-change handler now updates hint visibility before refreshing the catalog; typed Agent/internal target names still filter and hide the hint, while clearing the value restores it.
- Focused search/catalog tests passed 5/5; final full regression 944/944; build 0 warnings/errors; 340 strict UTF-8 files. Computer Use launch timed out and the passive poll found no OMNIX window, so no real screenshot is claimed.

## 2026-07-16 - Actionable uninstall post-scan result started

- Parallel read-only audits agreed that the post-scan presenter already produces `重新扫描` / `查看残留清单`, but `UninstallPostScanResultWindow` renders the next step as text and exposes only Close.
- Chose a typed `Close` / `RetryReadOnlyScan` / `ReviewResidue` handoff. The result window remains presentation-only; MainWindow always rescans current inventory before following the requested read-only/review path.
- Retry will stop at an inline read-only residue conclusion. Review may reach the existing low-risk quarantine confirmation, but no post-scan click itself performs mutation.

## 2026-07-16 - Actionable uninstall post-scan result completed

- Added typed `Close`, `RetryReadOnlyScan`, and `ReviewResidue` actions. The result window returns intent only and contains no operation pipeline, quarantine, process, file, registry, service, or task authority.
- Added a stable primary-action button. Clean results show only Close; failure/still-present/incomplete-background results offer a read-only retry; verified residue offers explicit review.
- `UninstallPlanWindow` carries the action to `MainWindow`. Current application inventory is re-read after the production attempt before either action is followed. Retry shows an inline read-only residue conclusion; Review reuses the existing separate quarantine confirmation path; Close never reviews residue.
- Verification: focused 9/9; related 361/361; full 948/948; build 0 warnings/errors; 341 strict UTF-8 C#/XAML files. The first-view PNG `.omx/qa-uninstall-post-scan-action.png` was rendered and manually inspected. No real uninstaller or residue operation ran.

## 2026-07-16 - Personal-file read-only location inspection started

- Audited the personal large/possible-duplicate flow. The analyzer retains exact bounded evidence paths, while the beginner presenter and C-drive list deliberately drop every path and expose no inspection handoff.
- Chose one explicit `查看位置` action per finding. A presentation-only window will list only that finding's captured locations; MainWindow will verify the chosen path against current scan evidence; an isolated launcher will select the existing local file through the fixed Windows Explorer executable.
- This slice adds no file-content read, duplicate proof, cleanup recommendation, delete, move, quarantine, or direct Agent execution authority.

## 2026-07-16 - Personal-file read-only location inspection completed

- Preserved exact evidence paths in the internal finding view model while keeping default candidate titles, summaries, Agent advice, and safety copy path-free.
- Added one explicit `查看位置` action and a presentation-only detail window listing only the selected finding's captured paths. The window returns one selected path and contains no process or mutation authority.
- Added an isolated Explorer launcher that accepts only a fully qualified local existing file still present in the current scan evidence, rejects UNC/relative/alternate-stream/stale paths, resolves the fixed Windows `explorer.exe`, and uses `ArgumentList` for `/select,`.
- Verification: focused 10/10; related health/C-drive/product 191/191; full 953/953; build 0 warnings/errors; 345 strict UTF-8 files; all XAML parses; window/handler forbidden-authority hits 0. Render `.omx/qa-personal-storage-inspection.png` passed and was manually inspected. No real personal file or Explorer process was opened by tests.

## 2026-07-16 - Persisted digest evidence hydration started

- Confirmed the homepage digest button only called `ShowPage("CDrive")` and then claimed the latest evidence was open. After restart, digest history exists but `_lastHealthSummary`, recommendations, growth, personal-file findings, and root-cause cards do not.
- Scoped the fix to the existing read-only health gate: navigate with honest loading copy, start or join one scan, require current in-memory summary before success copy, and retain failure/cancel truth.
- No background schedule, automatic mutation, digest schema change, or cleanup execution will be added.

## 2026-07-16 - Persisted digest evidence hydration completed

- Changed the historical digest action to `重新体检并查看当前证据` until a current-process health session exists; after a successful session it becomes `查看当前 C 盘证据`.
- The action is now async and non-reentrant, immediately navigates with honest read-only loading copy, starts or joins the shared health gate, and requires both `HasCompletedLoad` and `_lastHealthSummary` before claiming current evidence is open.
- Failure/cancel keeps the historical digest visible but explicitly refuses to call it current detail. Digest reloads cannot re-enable the button during the in-flight action.
- Verification: focused 16/16; related health/home/Agent/product 195/195; full 954/954; build 0 warnings/errors; 346 strict UTF-8 files; all XAML parses; handler forbidden-authority hits 0. No real cleanup or mutation ran.

## 2026-07-16 - Agent background context handoff started

- Audited the background/startup Agent surfaces. The background review already identifies up to six application names but renders text-only rows; aggregate startup answers navigate to `Apps` without selecting the existing `Resident` filter.
- Chose details-only per-item navigation plus a typed Resident catalog handoff. Neither action will open the startup-control preview automatically; the user must still choose `管理自启动` from the application drawer and pass its existing evidence/confirmation flow.
- The first repository search accidentally used a wildcard in a Windows path argument; subsequent discovery uses `rg -g` as required and the error will be recorded.

## 2026-07-16 - Agent background context handoff completed

- Added safe display/target separation to background review items. Path-like names render as `这个应用` and cannot navigate; ordinary unique application names receive a `查看应用` details-only action.
- Added nullable typed `TargetAppFilter` to Agent replies. Both empty-inventory and populated startup/background replies carry `AppCatalogFilter.Resident`.
- MainWindow accepts only Resident for aggregate Agent catalog handoff, clears stale search, starts or joins inventory loading, refreshes the Resident grid, and never opens startup control automatically.
- Verification: focused 15/15; related Agent/application/product 251/251; full 956/956; build 0 warnings/errors; 347 strict UTF-8 files; all XAML parses; new-handler forbidden-authority hits 0. No startup, registry, service, task, or process operation ran.
# 2026-07-18 - Agent migration/uninstall catalog handoff planning

- Re-read `current.md`, `handoff.md`, and the uncommitted worktree before relying on continuation context.
- Confirmed startup/background Agent answers already preserve `Resident`, while aggregate migration and uninstall answers still open an unfiltered application catalog.
- Selected a bounded typed handoff: migration -> `CDrive`, uninstall -> `Uninstallable`, startup -> existing `Resident`. These are candidate views only and must not open plans or imply approval.

## 2026-07-18 - Agent migration/uninstall catalog handoff completed

- Aggregate migration answers now open the existing `占 C 盘` catalog and aggregate uninstall answers open `可卸载`; startup/background retains `后台常驻`.
- MainWindow accepts only those three typed Agent filters, clears stale search, starts or joins current inventory loading, refreshes the catalog, and uses filter-specific beginner copy for unavailable, empty, and populated states.
- The handoff remains read-only: it does not open migration, uninstall, or startup review and does not invoke an operation pipeline.
- Verification: focused 8/8; related 279/279; full 957/957; build 0 warnings/errors; 347 strict UTF-8 files; 17 XAML files parse; exact migration/uninstall assignments one each.

## 2026-07-18 - Agent next-step application handoff planning

- Audited visible preview/navigation surfaces after closing aggregate conversation filters.
- Found the persistent Agent next-step actions still bind only `TargetPage`; both `查看后台常驻` and C-drive application suggestions open an unfiltered Apps page.
- Selected typed per-action `TargetAppFilter` plus a stable filter-aware AutomationId, with the existing MainWindow filter allowlist as the only application-catalog handoff.

## 2026-07-18 - Agent next-step application handoff completed

- Added typed `TargetAppFilter` and computed stable `AutomationId` to each next-step navigation action.
- Resident recommendations now carry `Resident`; C-drive application recommendations carry `CDrive`; empty/general page navigation remains unfiltered.
- The XAML binds the complete action object. The async handler validates navigation-only state, page allowlist, and Apps/filter consistency, then delegates to the bounded catalog handoff.
- Upgraded three stale source contracts from old sync/string assumptions, including two brittle range extracts that now use `SourceMethodExtractor`.
- Verification: focused 2/2; related 275/275; full 959/959; build 0 warnings/errors; 348 strict UTF-8 files; all 17 XAML parse. Computer Use launch timed out and no OMNIX window appeared, so visual runtime proof remains Warn.

## 2026-07-18 - Home migration-closure catalog handoff planning

- Reviewed homepage finding actions after the persistent Agent next-step fix.
- Exact app findings already re-resolve current inventory, and personal-storage findings preserve their bounded C-drive location. Only targetless migration-closure findings open an unfiltered Apps catalog.
- Selected optional typed `CDrive` context on the home response, delegated to the same bounded catalog handoff used by Agent conversation and next-step actions.

## 2026-07-18 - Home migration-closure catalog handoff completed

- Added optional typed `TargetAppFilter` to homepage Agent responses and assigned `CDrive` only to targetless migration-closure findings.
- Exact app targets reject simultaneous aggregate filters and still re-resolve current inventory. Aggregate filters require the Applications destination and delegate to the bounded catalog handoff.
- Other C-drive evidence, personal-storage navigation, and target-unavailable fallback remain unchanged.
- Verification: focused 5/5; related 199/199; full 960/960; build 0 warnings/errors; 348 strict UTF-8 files; all 17 XAML parse.

## 2026-07-18 - C-drive application handoff truth planning

- Audited the C-drive root-cause actions after unifying Agent application handoffs.
- Found the CDrive Apps branch checks `_softwareProfiles.Count == 0` after filtering, so a nonempty inventory with zero C-drive matches receives incorrect populated-state copy.
- Selected delegation to the already-tested bounded CDrive handoff, eliminating duplicate load/filter/status logic without changing the root action's validation or authority.

## 2026-07-18 - C-drive application handoff truth completed

- Replaced the duplicate CDrive Apps branch with one awaited call to the bounded CDrive catalog handoff.
- The shared handoff now consistently owns page selection, stale-search clearing, load-before-refresh, filter selection, filtered item-count truth, and beginner status copy for root-cause and Agent entries.
- Root-cause card/action validation and recycle-bin, personal-storage, and cleanup-recommendation branches remain unchanged.
- Verification: focused 5/5; related 282/282; full 960/960; build 0 warnings/errors; 348 strict UTF-8 files; all 17 XAML parse.

## 2026-07-18 - Isolated GUI lifecycle diagnosis

- Computer Use direct `launch_app` and a full ten-second passive poll returned no target, so an isolated shell lifecycle probe was run with explicit approval and a workspace-local data root.
- The Debug app remained alive after five seconds and exposed `MainWindowTitle=OMNIX-Entropy`, proving it does not immediately crash on a clean data root.
- A second isolated instance was uniquely visible to Computer Use, but `get_window_state` stopped at `Computer Use app approval timed out`. No UI input occurred and the exact test process was stopped.
- Visual gate remains Warn without a screenshot; startup lifecycle is now Pass and no isolated process remains.

## 2026-07-18 - Source integrity gate script promoted

- Repeated inline UTF-8/XAML loops and shell quoting failures justified a repository helper.
- Added `.omx/verify-source-integrity.ps1` for strict UTF-8 decoding, U+FFFD detection, and XML parsing of all non-generated C#/XAML files.
- Updated `AGENTS.md` with the exact process-scoped execution-policy command; the script is read-only and does not change machine policy.
- Verified helper result: 348 source files, 0 invalid UTF-8, 0 replacement-character files, 17 XAML files, 0 invalid XAML.

## 2026-07-18 - Portable test package planning

- Completion audit confirmed the core workflows are source-connected, but current App and worker binaries both report Authenticode `NotSigned` and there is no reproducible test package command.
- Selected a timestamped `.artifacts` portable package that publishes App plus sibling worker/rules, generates SHA-256 and signature truth, writes beginner testing boundaries, and creates a zip.
- The script will never sign, import certificates, delete existing output, or relax current package trust. Unsigned mutation remains blocked and visible in the manifest/readme.

## 2026-07-18 - Portable test package completed

- Added four static safety/contents contracts, an ASCII-only Windows PowerShell 5.1 publishing script, a separate UTF-8 Chinese beginner readme template, and `.artifacts/` ignore policy.
- The script publishes App and Elevated into one new framework-dependent folder, refuses existing/out-of-root outputs, verifies required payloads, records file length/SHA-256 and both Authenticode states, derives same-signer/mutation readiness, and creates a ZIP without signing, importing trust, launching, or deleting.
- Live runs exposed and fixed UTF-8 script parsing plus two .NET Framework host API incompatibilities; the failed partial output was left untouched by policy.
- Final artifact: `.artifacts/OMNIX-Entropy-test-20260718-205628` and matching ZIP. Manifest: 110 files, App/Worker `NotSigned`, same signer false, mutation blocked. ZIP: 139 entries with App, worker, rules, readme, and manifest.
- Verification: focused 4/4; full 964/964; build 0 warnings/errors; source integrity 349 files, 0 invalid UTF-8/replacement files, 17/17 XAML parse.

## 2026-07-18 - Release debug-command surface planning

- Audited every MainWindow `Click` hook against code-behind and found no unbound visible button.
- Compared App and Elevated process entry points. App smoke arguments are already under `#if DEBUG`, but `official-uninstall-fake-worker` and its implementation remain compiled into the privileged Release worker.
- Confirmed the current Release `Css.Elevated.dll` contains the fake command in UTF-16 metadata.
- Selected Debug-only source inclusion plus a package-time UTF-8/UTF-16 binary token refusal, without changing production modes or IPC internals.

## 2026-07-18 - Release debug-command surface completed

- Guarded the fake worker process mode with `#if DEBUG` and excluded `OfficialUninstallFakeWorker.cs` from Release compilation.
- Restricted the portable package script to Release, added a Windows PowerShell-compatible byte-sequence check for UTF-8/UTF-16 fake command metadata, and recorded `ReleaseCommandSurface=ProductionOnly` in the manifest.
- Debug worker lifecycle/production-mode/release contracts pass 22/22. Actual Release worker shrank from 74,752 to 72,704 bytes; fake token is absent in both encodings while production uninstall and migration tokens remain present.
- Latest package: `.artifacts/OMNIX-Entropy-test-20260718-210944` plus ZIP, 110 manifest files/139 ZIP entries, command surface ProductionOnly, unsigned mutation still blocked.
- Verification: full 966/966; build 0 warnings/errors; source integrity 350 files, 0 invalid UTF-8/replacement files, 17/17 XAML parse.

## 2026-07-18 - Home key-findings empty-state planning

- Computer Use uniquely attached to the latest Release package after a shell-only isolated launch and captured a real 1268x778 Home screenshot.
- Startup, navigation, score cards, Agent card, and automatic system-drive label are visible and unclipped.
- The screenshot exposed a large empty bordered `KeyFindingsListBox` caused by `MinHeight=240` with no items before the first health scan.
- Selected a stable first-visible TextBlock plus summary-driven list/text visibility, with distinct not-scanned and valid-empty wording.

## 2026-07-18 - Home key-findings empty state completed

- Added `KeyFindingsEmptyStateTextBlock` before the findings list with a stable AutomationId; the empty `KeyFindingsListBox` is collapsed by default.
- `RefreshHealthSummaryFromBase` now shows the list only for real findings and otherwise replaces the initial not-scanned copy with a valid “no priority item” conclusion.
- Computer Use attached to a newly packaged isolated Release window. The corrected first view has no blank inner rectangle and exposes the empty-state text in UIAutomation.
- Clicked only the Apps and AI Agent navigation buttons. Apps completed a real read-only scan of 391 profiles and displayed icons, concise tags, drawer conclusions, and availability-gated actions. Agent displayed current background/C-drive guidance and typed next actions. No scan-cleanup, uninstall, migration, settings, or system tool action was invoked.
- Latest package: `.artifacts/OMNIX-Entropy-test-20260718-212514` and ZIP; manifest 110 files, ZIP 139 entries, ProductionOnly command surface, unsigned mutation blocked.
- Verification: focused/related 218/218; full 968/968; build 0 warnings/errors; source integrity 351 files, 0 invalid UTF-8/replacement files, 17/17 XAML parse. Both Release test windows were closed through Computer Use.

## 2026-07-18 - Agent page information-hierarchy planning

- Reviewed the real Release Agent screenshot after successful Apps/Agent navigation.
- Functionality is present, but consultation, background recommendations, Windows settings, skills, and system tools all render simultaneously in two narrow columns.
- Selected native WPF tabs: default `咨询与建议` for conversation/current recommendations, and `能力与工具` for allowlisted shortcuts and skill catalog.
- Scope is XAML-only; all existing controls, AutomationIds, event handlers, safety wording, and routing remain authoritative.

## 2026-07-18 - Agent page information hierarchy completed

- Added a default `咨询与建议` tab and a separate `能力与工具` tab with stable page/tab AutomationIds; all existing conversation, recommendation, settings, skill, and system-tool controls remain intact.
- The first real Release screenshot exposed an over-constrained 780px consultation card and a large unused right area. Removed the fixed width, added a contract against its return, republished, and captured the corrected full-width result.
- Computer Use verified the default tab contains only consultation/current next steps and that one tab click reveals settings, skills, and tools. No setting, tool, scan, cleanup, uninstall, or migration action was invoked; both package windows were closed.
- Final package: `.artifacts/OMNIX-Entropy-test-20260718-214320` and ZIP. Manifest remains ProductionOnly; App/worker are `NotSigned`, so mutation readiness remains blocked.
- Verification: focused/related 216/216; full 970/970; build 0 warnings/errors; source integrity 352 files, 0 invalid UTF-8/replacement files, 17/17 XAML parse.

## 2026-07-18 - C-drive first-view hierarchy completed

- Real Release inspection confirmed that four empty fixed/minimum-height result surfaces made the unscanned C-drive page look broken and obscured the Start Scan action.
- Added stable root-cause and recommendation state text, collapsed root-cause/growth/personal-storage/recommendation lists plus action preview/button by default, and centralized count-driven visibility after a current scan.
- Scan start, cancellation, failure, completed-empty, and completed-populated states now use distinct truthful copy. The final visual pass also removed the premature generic quarantine sentence from the unscanned first view.
- Computer Use captured the final Release C-drive page: automatic C-drive identity and read-only scan guidance are visible; no empty results, action preview, continuation button, or isolation wording appears before evidence. No scan or mutation ran and all windows were closed.
- Final package: `.artifacts/OMNIX-Entropy-test-20260718-220108` and ZIP; ProductionOnly, App/worker `NotSigned`, mutation blocked. Verification: focused 2/2; related 211/211 and 171/171; full 972/972; build 0 warnings/errors; source integrity 353 files, 17/17 XAML parse.

## 2026-07-18 - Installation Control first-view hierarchy completed

- Real Release inspection showed that empty routing memory was presented as a fake rule row and that a blank 186px change-report list, disabled Agent button, and technical expander occupied the default workflow.
- Empty routing memory now returns no rows; the summary remains the truthful empty state. Rule list/forget controls derive from current row count.
- Install-diff cards remain collapsed until a valid presenter has cards; Agent explanation and technical details remain collapsed until a valid report. New snapshot capture and incomplete manual comparison revoke those surfaces again.
- Computer Use captured the final Release first view with one clear selection/analyze workflow, concise rule state, automatic monitoring notice, collapsed advanced diagnostics, and report-not-generated copy. No picker, analyzer, installer, settings, or mutation action ran; the window was closed.
- Final package: `.artifacts/OMNIX-Entropy-test-20260718-221512` and ZIP; ProductionOnly and unsigned mutation blocked. Verification: related 217/217; full 975/975; build 0 warnings/errors; source integrity 354 files, 17/17 XAML parse.

## 2026-07-18 - Undo Center first-view hierarchy completed

- The initial isolated Release screenshot showed two fixed-height empty lists, a disabled permanent-cleanup button, and a synthetic non-restorable timeline row even though quarantine usage was 0 B and history was empty.
- Added a stable compact timeline state, collapsed empty history/candidate surfaces by default, and made current entries and retention candidates the only visibility authority for their lists and cleanup review action.
- Loading, unavailable, and valid-empty history now use distinct truthful conclusions. Existing restore and permanent-purge confirmation/pipeline code was not changed.
- Computer Use captured the final isolated Release page: policy and empty conclusion are visible; candidate list, cleanup button, timeline list, fake row, technical expander, and restore action are absent. No restore, purge, scan, uninstall, migration, or file action ran; the package window was closed.
- Final package: `.artifacts/OMNIX-Entropy-test-20260718-223259` and ZIP; ProductionOnly, App/worker `NotSigned`, mutation blocked. Verification: focused 2/2; related 224/224; full 977/977; Release build 0 warnings/errors; source integrity 355 files, 17/17 XAML parse.

## 2026-07-18 - Migration plan decision hierarchy completed

- An isolated one-app Release fixture confirmed the icon grid and drawer already lead with human location, size, residency, and Agent advice. Opening migration exposed the defect: raw user paths, rollback manifest, byte counts, and long readiness/step lists dominated the first view.
- Added `MigrationPlanDecisionSummaryPresenter` and a computed preview decision that answers status, conclusion, D-drive target, next step, rollback, and coarse space state without raw paths.
- Reordered the migration window so Agent decision is first and all raw destination/rollback/space/checklist/section evidence lives under a collapsed `查看技术详情` expander with stable AutomationIds.
- Unsigned packages now collapse unavailable rollback-evidence and migration-request buttons; valid production readiness still reveals them and keeps all existing enablement/confirmation gates.
- Computer Use captured the final Release preview. Only the human conclusion, safety/readiness explanation, collapsed technical entry, reminder, and Close are visible. No evidence file, migration request, UAC, file move, or system action ran; all windows were closed.
- Final package: `.artifacts/OMNIX-Entropy-test-20260718-224949` and ZIP; ProductionOnly and unsigned mutation blocked. Verification: focused 3/3; related 254/254; full 980/980; Release build 0 warnings/errors; source integrity 356 files, 17/17 XAML parse.

## 2026-07-19 - Uninstall decision hierarchy completed

- Real Release inspection found that the unsigned uninstall preview still asked the beginner to choose an installer, inspect restore points, acknowledge backup state, and face a disabled final-checklist action before giving one decision.
- Added `UninstallPlanDecisionSummaryPresenter` with path-free explanations for current state, official-uninstall flow, residue handling, undo limits, and next step.
- Reordered the preview so the Agent decision is first; preparation, complete workflow, and technical evidence are collapsed. Unsigned packages hide the preparation expander and replace its next step with the signed-release requirement.
- The same isolated fixture proved the existing post-uninstall residue refusal: because the application is still detected, OMNIX does not classify its files as residue and exposes no quarantine/delete action.
- Computer Use exercised preview, Close, and read-only residue rescan only. No official uninstaller, evidence writer, final consent, UAC, quarantine, registry, service, startup, task, or file mutation ran; all package windows were closed.
- Final package: `.artifacts/OMNIX-Entropy-test-20260719-000736` and ZIP; ProductionOnly, App/worker `NotSigned`, mutation blocked. Verification: focused 3/3; related 397/397; full 983/983; Release build 0 warnings/errors; source integrity 357 files, 17/17 XAML parse.
## 2026-07-19 - Disposable Windows behavioral acceptance protocol completed

- Added `new-disposable-acceptance-session.ps1`. It refuses primary/non-disposable environments, missing checkpoint evidence, incorrect attestation, existing/nonlocal/reparse session paths, nested package/session trees, and any candidate that fails the independent signed-candidate verifier before output creation.
- Added `verify-disposable-acceptance-receipt.ps1`. It is read-only and binds the current signed candidate, signer, session manifest, exact ten-case Pass set, reset attestation, ordered timestamps, unique evidence paths, and every evidence file length/SHA-256.
- Added a Chinese operator protocol covering manual UAC accept/cancel, fixture-only cleanup/cache/startup/uninstall/migration/rollback/Undo scenarios, evidence export, environment reset, and final read-only verification.
- TDD red was 0/6; focused green 6/6; release pipeline 20/20; full 1000/1000; Release build 0 warnings/errors; source integrity 361 files and 17/17 XAML; both scripts parse and remain ASCII-only.
- Runtime refusal against the current unsigned package returned exit 1 before creating the unique session directory. No product launch, UAC interaction, signing, certificate access, or system mutation ran.
## 2026-07-19 - Antivirus-updated Release launch retry remained unavailable

- Retried the latest ProductionOnly unsigned Release package through Computer Use after the user confirmed updated Huorong definitions.
- `launch_app` timed out; the one allowed passive window query found no OMNIX-Entropy window. No alternate GUI automation, security-software interaction, or launcher bypass was used.
- A read-only process check found no `Css.App` process, so the failed attempt left no OMNIX-Entropy background instance. Visual acceptance remains Warn and unsigned mutation remains blocked.

## 2026-07-19 - Disposable acceptance fixture kit completed

- Added an isolated `Css.AcceptanceFixtures` console project that is never referenced by App/Elevated packaging. Its mutation commands require the exact disposable-environment attestation and a canonical GUID; `status` remains read-only.
- Implemented preflighted, ownership-marked provision/uninstall/lock/reset behavior through injectable file/registry adapters. Provision compensates partial writes, uninstall removes only exact owned HKCU records and leaves residue, and reset refuses reparse traversal or mismatched ownership.
- Integrated the fixture shape with real `SoftwareInventoryBuilder`, uninstall trust, C-drive scan rules, and `DiskRecommendationBuilder`: the app cache/startup evidence attributes to the fixture, and exact `C:\Temp` produces a low-risk reversible cleanup operation.
- Added fixture publishing/verifying scripts and bound the verified fixture-manifest hash into disposable session creation and final receipt verification.
- An unsigned product package plus a valid fixture package was still refused before session creation, proving the fixture dependency does not weaken the signed-candidate gate.
- Verification: fixture 22/22, fixture package 4/4, disposable protocol 6/6, related product/release 434/434, full 1026/1026, Release build 0 warnings/errors, source integrity 367 files and 17/17 XAML.
- Final fixture package/ZIP: `.artifacts/OMNIX-Acceptance-Fixtures-20260719-014314`; five payload files; manifest SHA-256 `07C033F1B445DCF1E171ABC18E8FAC3AD9ECDA1ADFDECC0603C22FB712FA4FA3`. No fixture mutation ran on the current machine.
- Republished product package/ZIP `.artifacts/OMNIX-Entropy-test-20260719-014731`; manifest contains 110 files, the artifact contains zero fixture payloads, the Release command surface is ProductionOnly, and unsigned mutation remains blocked.

## 2026-07-22 - Signing prerequisite inspection planning

- Re-read the repository protocol, current state, handoff, and untracked worktree before continuing the persistent goal.
- Retried the newest product package through Computer Use after the antivirus update. Launch timed out; one passive window query found no OMNIX window and a read-only process query found no `Css.App`. No security UI, fallback automation, or bypass was used.
- Audited the V1 feature surfaces and confirmed the remaining release authority is the real same-signer gate rather than another missing product operation handler.
- Direct read-only checks found no `signtool.exe` on PATH and no standard `C:\Program Files (x86)\Windows Kits\10\bin` directory.
- Selected a bounded read-only prerequisite inspector so the next operator sees exact tool/certificate readiness without certificate import, generation, installation, trust changes, auto-selection, or signing.

## 2026-07-22 - Signing prerequisite inspection completed

- Added the read-only inspector with object/JSON output, explicit-path validation, bounded PATH/Windows Kits discovery, CurrentUser-only certificate metadata, and no automatic certificate selection.
- The first runtime exposed two compatibility bugs hidden by static contracts: provider-specific `EnhancedKeyUsageList` could turn an ineligible certificate into a false store-read failure, and sorting an empty candidate pipeline produced null under strict mode. Replaced EKU access with direct X509 extension parsing and preserved the empty result as an array; added a child-process JSON regression.
- Applied the same X509 EKU parser to the production signed-release transform so its certificate decision matches the prerequisite report under Windows PowerShell 5.1.
- Current result is precise: signing tool absent, certificate store readable, zero eligible code-signing certificates, and candidate creation unavailable. An invalid explicit tool path is reported as `ExplicitPathInvalid` without fallback or mutation.
- Verification: focused 4/4; signed-release plus inspector 8/8; full 1030/1030; Release build 0 warnings/errors; source integrity 368 files and 17/17 XAML; both scripts parse and are ASCII-only.

## 2026-07-22 - Repeated external release blocker confirmed

- Re-read protocol/current/handoff/worktree and reran the reviewed prerequisite inspector in a fresh goal continuation.
- Result remained unchanged: `signtool.exe` absent, CurrentUser certificate store readable, zero eligible code-signing certificates, and `CanCreateSignedCandidate=false`.
- The same signing/disposable-environment condition has now repeated across at least three consecutive goal turns. No local code change can truthfully create production trust, approve a signer, or produce disposable-machine behavioral evidence.
- Recorded the goal as blocked rather than adding unrelated scope, creating a self-signed certificate, weakening trust, or running mutation fixtures on the primary machine.

## 2026-07-23 - Personal publisher created; signed candidate correctly failed on missing root trust

- Installed reviewed Inno Setup 6.7.3 to `D:\Development\Inno Setup 6`; its compiler Authenticode status is Valid.
- Added a guarded, idempotent personal signer initializer and contracts. After exact user consent, created non-exportable RSA code-signing certificate `5688958FEA0056861558E8DCF9D2381AF46074B2` for the interactive CurrentUser account.
- Independent store inspection found the private key only in CurrentUser My, public copies only in CurrentUser TrustedPeople/TrustedPublisher, and no matching entry in CurrentUser Root or inspected LocalMachine stores.
- Corrected the timestamp URL policy using DigiCert's official RFC3161 documentation: exact `http://timestamp.digicert.com` is narrowly allowed while other HTTP endpoints remain refused. Focused signer/installer/release contracts passed 9/9 and all three scripts parsed.
- Built `.artifacts/OMNIX-Entropy-test-20260723-225606`. Signing and timestamping App/worker succeeded, but Windows chain verification rejected the self-signed certificate because it is not in Trusted Root. The transform failed closed before manifest/ZIP completion; no installer was compiled or launched.
