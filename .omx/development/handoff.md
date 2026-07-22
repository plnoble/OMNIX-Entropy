# Agent Handoff

## Latest handoff - 2026-07-22 GitHub personal release foundation

- Current objective: publish the audited source baseline to `https://github.com/plnoble/OMNIX-Entropy`, then add a directory-selectable D-first installer and verified update application.
- What changed: origin connected; public ignore rules fixed; pinned read-only CI added; local draft-only same-signer release staging added; root README and personal update guide added; fixed-repository release manifest policy/client and compact user-triggered update window added; App version set to 0.1.0.
- What is verified: focused 13/13; full Debug 1048/1048; Release build 0 warnings/errors; source integrity 377 and XAML 18/18; PowerShell parser pass; 424 public candidates/about 6 MB; no binaries, signing secrets, or real username in candidates.
- What is not verified: no real GitHub Release exists; no personal certificate exists; no package download or install path exists; no D-first installer has been built; Computer Use timed out launching the app, so the update dialog has no real screenshot.
- Known risks/blockers: do not run the full fake-worker lifecycle tests in Release; do not parallelize dotnet commands sharing output; do not use Velopack's C-drive LocalAppData install layout; do not put a PFX/private key in GitHub or remove valid-same-signer authorization.
- Exact next recommended action: complete the initial audited commit/push, verify GitHub CI, then choose/install a reviewed directory-selectable installer tool and build a same-signer D-first Setup/update/rollback flow.

## Current Objective

Continue accepting OMNIX-Entropy V1's already-connected critical workflows only after the remaining external prerequisites change. Product and safety-pipeline contracts, signed-candidate transformation, transfer verification, deterministic fixtures, operator guidance, and the disposable Windows behavioral-acceptance protocol are implemented. The remaining release gate is external: obtain an approved RSA code-signing identity, create one same-signer candidate, then manually run the fixture-only checklist in a checkpointed disposable Windows environment.

Current status is blocked after the same certificate and disposable-environment conditions persisted across three resumed goal turns. SignTool is installed and now discovered correctly. Resume release execution only after an explicitly approved real RSA code-signing certificate and a checkpointed disposable Windows environment are available; do not manufacture trust or substitute source tests for behavioral evidence.

## What Changed

- Found seven real SignTool binaries under `D:\Windows Kits\10`; both standard Windows Kits installed-root registry values point there. The old inspector falsely reported NotFound because it only checked PATH and Program Files.
- Added read-only `KitsRoot10` discovery for the two exact standard installed-root registry keys. It validates rooted existing directories, deduplicates them, enumerates only direct `bin\<version>\x64|x86|arm64` candidates, preserves explicit-path precedence, and never recurses across a drive.
- Current report at `2026-07-22T14:04:45Z`: `SignToolFound=true`, path `D:\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe`, resolution `WindowsKitsRegistry`, store readable, zero eligible RSA signers, `CanCreateSignedCandidate=false`. The only signing prerequisite still missing is the approved RSA certificate.
- Verification: inspector 4/4; inspector/guide/audit 8/8; full 1035/1035; Release 0 warnings/errors; integrity 370 files and 17/17 XAML; parser errors 0; script non-ASCII bytes 0.
- Exact next action: obtain/install the approved RSA Authenticode certificate and provide a checkpointed disposable Windows environment. Do not install another SDK or search for another SignTool.

- Added `docs/development/v1-completion-audit.zh-CN.md`, a requirement-by-requirement matrix for navigation, health, C-drive cleanup/growth, applications, personal storage, install routing, uninstall, migration, startup, Undo, Agent, operation-pipeline authority, and release acceptance. It deliberately concludes that the entire goal is not yet proven without signed disposable behavior evidence.
- The audit found one local plan gap: Application Management exposed risk/size/growth/name sorting but not the required recent-install order. Added strict uninstall-registry `InstallDate` parsing, `SoftwareProfile.InstallDate`, enrichment preservation, `AppCatalogSort.RecentInstall`, unknown-last ordering, a stable `AppSortComboBox` AutomationId, the `按最近安装` option, and a technical-detail date line.
- Verification: related application/inventory 209/209; critical-workflow audit group 337/337 including the current-machine Marvis read-only scan; focused audit/inventory/growth/product 205/205; full 1035/1035; Release 0 warnings/errors; source integrity 370 files and 17/17 XAML; Agent direct mutation-authority hits 0.
- Visual evidence remains Warn: Computer Use timed out launching the current Debug executable, no OMNIX window appeared, and no `Css.App` process remained. No security bypass or fallback UI automation was used.
- Exact next action is unchanged externally: provide SignTool, an approved RSA code-signing certificate, and a checkpointed disposable Windows environment; then follow the release guide and complete the ten-case receipt.

- Added `docs/release/README.zh-CN.md` as the short operator entry point linking prerequisite inspection, signed-candidate creation, transfer verification, and the ten-case disposable acceptance sequence.
- Added `docs/release/signing-prerequisites.zh-CN.md`, backed by contracts and official Microsoft references. It documents only the local CurrentUser certificate route actually supported by the scripts and explicitly rejects self-signed production certificates, trust bypasses, secret handling, and unsupported remote-signing assumptions.
- Hardened inspector, signed-package transform, and transfer verifier to require RSA public keys and record/recheck `CertificatePublicKeyAlgorithm=RSA`; this matches the current Smart App Control compatibility boundary.
- Verification baseline after these changes: guide/index plus signing/inspector/verifier 15/15; full 1033/1033; Release build 0 warnings/errors; source integrity 369 files and 17/17 XAML.
- Current read-only prerequisite result at `2026-07-22T12:45:12Z` remains: no `signtool.exe`, zero eligible RSA code-signing certificates, store readable, `CanCreateSignedCandidate=false`.
- Exact next action: follow `docs/release/README.zh-CN.md`; provide the approved external prerequisites, rerun the inspector, explicitly select the certificate thumbprint, create and transfer the candidate, then complete the receipt in the disposable environment.

- Added `scripts/inspect-release-signing-prerequisites.ps1`, an ASCII-only Windows PowerShell 5.1-compatible read-only report. It accepts an optional explicit `signtool.exe`; otherwise it checks PATH and bounded Windows Kits version/architecture paths. It reads only CurrentUser `My`, lists only current private-key certificates with the code-signing EKU, and never selects a certificate.
- Replaced provider-specific `EnhancedKeyUsageList` use in the signed-release transform with direct X509 enhanced-key-usage extension parsing, shared semantically with the inspector. This avoids misclassifying an ordinary certificate under strict Windows PowerShell 5.1 execution.
- Current-machine report: certificate store readable; zero eligible code-signing certificates; no `signtool.exe`; `CanCreateSignedCandidate=false`. The one ordinary localhost certificate is not treated as a signer.
- A fresh 2026-07-22 read-only rerun produced the same result. This is now the repeated external blocker, not an unimplemented local feature.
- Verification: inspector 4/4; inspector plus signed-release contracts 8/8; full 1030/1030; Release build 0 warnings/errors; source integrity 368 files and 17/17 XAML; both changed scripts parse and contain zero non-ASCII bytes.
- Latest package launch retry still timed out through Computer Use. One passive window list and read-only process check found no OMNIX window or `Css.App`; no bypass or security-software interaction was attempted.
- Exact next action: provide/install the Windows SDK signing tool and obtain an approved real code-signing certificate, rerun the inspector, then explicitly supply its approved thumbprint to the signed-package transform. Positive product mutation remains reserved for the checkpointed disposable Windows protocol.

- Added the isolated `Css.AcceptanceFixtures` QA executable and deterministic disposable-environment fixture model. It creates two session-owned HKCU-installed-app records, one attributable app cache, one exact `C:\Temp` cleanup candidate, one HKCU Run entry, and one migration source/target layout; every mutating command requires the exact disposable-environment attestation and a canonical GUID.
- Provision preflights every path/registry collision before writes and compensates partial creation. Uninstall removes only exact owned fixture registration and leaves marked residue. Lock targets only the exact failure file. Reset preflights ownership, refuses reparse traversal, and removes only matching marked roots and exact registry values.
- Added `scripts/publish-acceptance-fixture-kit.ps1` and `scripts/verify-acceptance-fixture-kit.ps1`. Product App/Elevated packages do not reference or contain the fixture executable; the existing package allowlist remains authoritative.
- Disposable session creation and receipt verification now require a verified fixture directory and bind `FixtureManifestSHA256`, so a different fixture payload cannot be substituted after the session begins.
- Final fixture package: `.artifacts/OMNIX-Acceptance-Fixtures-20260719-014314` and ZIP; five payload files; manifest SHA-256 `07C033F1B445DCF1E171ABC18E8FAC3AD9ECDA1ADFDECC0603C22FB712FA4FA3`.
- Verification: fixture 22/22, package 4/4, protocol 6/6, related 434/434, full 1026/1026, Release build 0 warnings/errors, source integrity 367 files and 17/17 XAML. No fixture mutation ran on this machine.
- Republished the current product package as `.artifacts/OMNIX-Entropy-test-20260719-014731` and ZIP. Its manifest has 110 files, `ReleaseCommandSurface=ProductionOnly`, zero fixture payloads, App/worker `NotSigned`, and `MutationReadiness=BlockedUntilValidSameSignerPackage`.
- Current external gate: provide an approved real code-signing certificate and `signtool.exe`, create/transfer/reverify one same-signer candidate, then run the ten manual cases in a checkpointed disposable Windows environment. The updated antivirus definitions do not replace signing or behavioral acceptance.

- After updated Huorong definitions, Computer Use still timed out launching the latest unsigned Release package. A passive window query found no OMNIX-Entropy window and a read-only process check found no `Css.App` process. No fallback automation or security bypass was used; visual acceptance remains Warn.

- Added `scripts/new-disposable-acceptance-session.ps1`, `scripts/verify-disposable-acceptance-receipt.ps1`, and `docs/release/disposable-windows-acceptance.zh-CN.md`.
- Session creation requires an allowlisted disposable environment, explicit primary=false/disposable=true values, a reset checkpoint id, exact operator attestation, a new fixed-local non-reparse session path, and successful signed-candidate preflight before writing anything.
- The generated template contains ten required cases: package preflight, both UAC cancellations, cleanup/cache quarantine restores, startup restore, official uninstall/residue review, migration closure, migration rollback, and Undo restore.
- Final verification is read-only. It requires every case to be Pass with notes, ordered timestamps, independent evidence files, correct length/SHA-256, candidate/session/signer binding, and completed environment reset before returning `BehavioralAcceptanceComplete=true`.
- Verification: TDD red 0/6; focused 6/6; related release pipeline 20/20; full 1000/1000; Release build 0 warnings/errors; source integrity 361 files and 17/17 XAML; PowerShell parsers valid and scripts ASCII-only. Current unsigned package returned exit 1 and created no session directory.
- No OMNIX process, UAC interaction, certificate access, signing, registry/service/task change, or file mutation was performed. Exact next action: create one signed candidate using an approved real code-signing certificate and `signtool.exe`, reverify transfer, then follow the manual protocol only in a disposable Windows fixture.

- Added `scripts/verify-signed-release-candidate.ps1`, a read-only transfer-time verifier for the future disposable Windows environment.
- It independently checks fixed-local/reparse safety, awaiting signed-candidate state, exact payload coverage, all hashes, valid timestamped same-signer App/worker signatures matching the manifest, and ProductionOnly worker command surface.
- It cannot launch OMNIX, write files, alter certificates, or claim behavioral acceptance. Success only permits beginning the explicit disposable checklist.
- Verification: focused 4/4, related 14/14, full 994/994, Release 0 warnings/errors, 360 strict UTF-8 C#/XAML files, 17/17 XAML, parser valid. The unsigned package was correctly refused. No process or mutation ran.
- Exact next action: implement a bounded behavioral acceptance protocol/receipt for fixture-only cleanup, startup disable/restore, uninstall/residue, migration/rollback/closure, and manual UAC cancel/accept evidence; make primary-machine execution fail closed.

- Added a separate immutable signed-release transform: `scripts/publish-signed-release-package.ps1` plus the Chinese candidate readme and four safety contracts.
- It verifies the portable source package, rejects reparse/debug content, uses only an explicit existing CurrentUser code-signing certificate, signs/timestamps App and worker copies, revalidates same signer, regenerates hashes, and records disposable-machine acceptance as still pending.
- It does not generate/import certificates, accept PFX passwords, change trusted stores, mutate the source package, or label signing as production acceptance.
- Verification: focused 4/4, related 10/10, full 990/990, Release 0 warnings/errors, 359 strict UTF-8 C#/XAML files, 17/17 XAML, PowerShell parser valid. Missing-sign-tool refusal created no output. No real certificate/signing action ran.
- Exact next action: obtain an explicitly approved real code-signing certificate plus `signtool.exe`, generate one candidate from `.artifacts/OMNIX-Entropy-test-20260719-003423`, inspect its manifest, then use only a disposable Windows machine for positive mutation/cancel/rollback acceptance.

- Migration and official-uninstall result acknowledgement now closes the exhausted plan window, allowing MainWindow's existing `ProductionExecutionAttempted` branch to immediately rescan current application/closure/residue state.
- Nested result buttons say `返回并重新检查`. The standalone Debug worker-connection result retains `我知道了`; it has no Application Management host.
- Preview-only paths, typed uninstall post-scan actions, signer trust, final consent, worker requests, rollback, quarantine, and mutation handlers are unchanged.
- Verification: focused 3/3, related 207/207, full 986/986, Release 0 warnings/errors, 358 strict UTF-8 C#/XAML files, 17/17 XAML. Final package/ZIP `.artifacts/OMNIX-Entropy-test-20260719-003423`; unsigned mutation remains blocked.
- Exact next action: audit remaining result/confirmation surfaces for stale-state dead ends or missing typed recovery actions; do not claim positive mutation acceptance until a valid same-signer package and disposable fixture exist.

- Cache cleanup and startup management were exercised in the isolated Release fixture. Cache evidence failed local validation and exposed no execution action; name-only startup evidence refused local control and exposed only `在 Windows 中查看` with an explicit no-toggle/no-registry boundary.
- No source change was needed for this slice. The inherited package remains `.artifacts/OMNIX-Entropy-test-20260719-000736`, full 983/983, Release build 0 warnings/errors, 357 strict UTF-8 C#/XAML files, and 17/17 XAML parse.
- Neither the Windows Settings handoff nor any cleanup/startup mutation was invoked. The package window was closed.
- Exact next action: inspect remaining execution-result windows for typed recovery/current-state actions after failure or completion; do not weaken the valid same-signer and disposable-fixture release gates.

- The uninstall preview now leads with one path-free Agent decision: current read-only state, official-uninstall flow, residue policy, undo limitation, and next step. Preparation, complete workflow, and technical evidence are collapsed.
- Unsigned packages hide recovery-preparation controls entirely and show the signed-release requirement instead of installer selection, backup acknowledgement, restore-point preparation, or a disabled final-checklist button. Signed production readiness still reveals the original preparation/final-consent workflow.
- The same isolated fixture confirmed the existing residue fail-closed behavior: when the application is still detected, OMNIX says it is not residue, moves/deletes nothing, and exposes no quarantine action.
- Latest verified package: `.artifacts/OMNIX-Entropy-test-20260719-000736` and ZIP. Focused 3/3; related 397/397; full 983/983; Release build 0 warnings/errors; 357 strict UTF-8 C#/XAML files; 17/17 XAML parse. All test windows are closed.
- No official uninstall, evidence write, final consent, UAC, residue movement, registry/service/startup/task change, or file mutation ran. App/worker remain `NotSigned`; `MutationReadiness=BlockedUntilValidSameSignerPackage`.
- Exact next action: use one isolated ordinary-app fixture to inspect cache-cleanup and startup-management decision outcomes, staying preview/cancel-only; keep positive mutation acceptance for a valid same-signer disposable fixture.

- The migration preview no longer leads with user paths, a rollback manifest filename, raw byte counts, and long readiness/step lists. A new typed decision summary answers whether it can migrate, D-drive target, next step, rollback, and coarse space state.
- Raw destination/score/manifest/space/checklist/section evidence remains available under a collapsed `查看技术详情` expander. Existing planner, readiness gate, rollback writer, worker, final consent, and result synchronization are unchanged.
- Unsigned packages no longer show unavailable `生成回滚清单` or `请求迁移` buttons; valid production readiness still reveals and enables them through the existing gates.
- Latest verified package: `.artifacts/OMNIX-Entropy-test-20260718-224949` and ZIP. Focused 3/3; related 254/254; full 980/980; Release build 0 warnings/errors; 356 strict UTF-8 C#/XAML files; 17/17 XAML parse. Computer Use captured the final fixture drawer and migration first view; all windows are closed.
- No official uninstall, residue movement, rollback evidence write, migration request, UAC, file move, registry/service/startup/task change, or other mutation ran.
- Exact next action: use the same process-scoped fixture to open `卸载干净点` and `卸载后检查残留`, inspect only preview/refusal/cancel paths, and simplify any technical-first conclusion found; do not run the official uninstaller or confirm residue movement.

- Undo Center no longer renders empty fixed-height quarantine/history lists, a disabled permanent-cleanup action, or a synthetic non-restorable timeline row when isolated storage has no records. A stable compact conclusion explains that future OMNIX safety changes will appear there.
- Current timeline entries reveal the existing list; current retention candidates reveal the candidate list and permanent-cleanup review. Existing restore/purge confirmation and pipeline authority are unchanged.
- Latest verified package: `.artifacts/OMNIX-Entropy-test-20260718-223259` and ZIP. TDD red; focused 2/2; related 224/224; full 977/977; Release build 0 warnings/errors; 355 strict UTF-8 C#/XAML files; 17/17 XAML parse. Computer Use captured the final isolated first view and the test window is closed.
- The user reports antivirus definitions are now updated. App/worker remain `NotSigned`, `ValidSameSigner=false`, and `MutationReadiness=BlockedUntilValidSameSignerPackage`; the antivirus update does not satisfy release-signing or disposable-mutation gates.
- No restore, purge, scan, installer, uninstall, migration, registry, service, or file mutation ran during this slice.
- Exact next action: open Application Management from the latest Release package, scan read-only inventory, inspect one ordinary app drawer, and audit whether uninstall-residue and migration-closure outcomes are understandable without technical details; do not invoke a production mutation.

- Installation Control no longer fabricates an empty routing-memory row or shows empty report cards, disabled Agent explanation, and technical details before evidence. Rule controls follow current rows; report controls follow a valid report and are revoked by new/incomplete comparisons.
- Latest verified package: `.artifacts/OMNIX-Entropy-test-20260718-221512` and ZIP. Related 217/217; full 975/975; build 0 warnings/errors; 354 strict UTF-8 C#/XAML files; 17/17 XAML parse. Computer Use captured the corrected Installation Control first view and all windows are closed.
- No installer package was selected, analyzed, or launched in the visual acceptance. Trusted execution and positive post-install behavior remain separate unsigned/disposable-fixture gates.
- C-drive pre-scan no longer displays large empty root-cause/recommendation boxes, empty growth/personal lists, a disabled action preview, or premature quarantine wording. Stable state lines explain that Start Scan is read-only and when recommendations will appear.
- Scan start/cancel/failure/completed-empty/populated states now switch presentation from the exact current presenter counts through one authority-free visibility helper. Scanner, cleanup confirmation, quarantine, and operation pipeline behavior are unchanged.
- Latest verified package: `.artifacts/OMNIX-Entropy-test-20260718-220108` and ZIP. Focused 2/2; related 211/211 and 171/171; full 972/972; build 0 warnings/errors; 353 strict UTF-8 C#/XAML files; 17/17 XAML parse. Computer Use captured the corrected C-drive first view and all windows are closed.
- Agent is no longer a dense two-column page. `咨询与建议` is the default full-width tab; `能力与工具` separately contains allowlisted settings, skills, and system tools. Stable AutomationIds identify the page and both tabs.
- Computer Use captured both real Release views and verified the default UI tree contains consultation/current next steps but not capability descendants. The first 780px implementation exposed a large blank area, was corrected to stretch, republished, and reverified.
- Latest verified package: `.artifacts/OMNIX-Entropy-test-20260718-214320` and ZIP. Focused/related 216/216; full 970/970; build 0 warnings/errors; 352 strict UTF-8 C#/XAML files; 17/17 XAML parse. All test windows are closed.
- Real modifying actions remain intentionally unverified and blocked because App/worker are still unsigned; no settings or system tool was opened during this acceptance.
- Added `scripts/publish-portable-test-package.ps1`, its Chinese readme template, four package safety contracts, and `.artifacts/` ignore policy.
- The Windows PowerShell 5.1-compatible script publishes App plus Elevated worker/rules, refuses existing or out-of-root output, hashes every payload, reads both Authenticode states, derives same-signer/mutation readiness, and writes a ZIP without signing, trust import, launch, or deletion.
- Previous Home-evidence package: `.artifacts/OMNIX-Entropy-test-20260718-212514` and matching `.zip`. It is FrameworkDependent and requires .NET 8 Desktop Runtime.
- Release no longer compiles the fake elevated worker mode; the package script rejects UTF-8/UTF-16 fake command metadata and records `ReleaseCommandSurface=ProductionOnly`.
- Final manifest: 110 hashed files; App/worker both `NotSigned`; `ValidSameSigner=false`; `MutationReadiness=BlockedUntilValidSameSignerPackage`. ZIP has 139 entries and all required payloads.
- Home no longer renders a blank fixed-height findings list before scanning. A stable compact empty-state line appears first; a completed empty summary uses distinct copy; real findings switch the list back on.
- Computer Use captured the corrected real Release Home first view, then clicked Apps and AI Agent. Apps rendered 391 current profiles with icons, labels, drawer summaries, and gated actions; Agent rendered current C-drive/background recommendations and typed navigation. No modifying action ran, and all test windows were closed.
- Verification: Home/related 218/218; full 968/968; build 0 warnings/errors; 351 strict UTF-8 C#/XAML files; 17/17 XAML parse. Package remains ProductionOnly and unsigned mutation remains blocked.
- Not verified: real uninstall/migration/system mutation remains intentionally untested without one valid same-signer package on a disposable machine.
- Exact next action: continue the requirement-by-requirement V1 visible-entry audit, then provision external code signing and a disposable fixture before any production mutation claim.

- Application Management search now starts with an empty query and shows `搜索应用` as a non-interactive overlay hint. Typing or Agent targeting hides the hint; clearing restores it without changing the fixed toolbar dimensions.
- Search input and hint have stable AutomationIds. Verification: focused 5/5, full 944/944, build 0 warnings/errors, and 340 strict UTF-8 files. Real-window proof remains Warn after a Computer Use launch timeout and empty passive poll.

- Reviewed migration and official-uninstall requests are now one-shot once their production coordinator is invoked. Attempt state is marked before `await`; unknown exceptions cannot re-enable stale evidence or skip the parent rescan.
- Unknown uninstall execution has a dedicated path-free beginner conclusion. It refuses to claim success, says the app will be rescanned, and promises no automatic retry or residue cleanup.
- Parent uninstall/migration workflows share one read-only application-rescan failure boundary. Missing current inventory stops residue or closure inference and preserves the prior catalog plus operation truth.
- Verification for this slice: focused 4/4, related 432/432, full 942/942, build 0 warnings/errors, 339 strict UTF-8 files, and inspected WPF evidence `.omx/qa-uninstall-unknown-attempt.png`. No real uninstall/migration ran; signed disposable-fixture acceptance remains Warn.

- All seven direct MainWindow safety-pipeline workflows now treat invocation as a read-only synchronization boundary: cache cleanup, startup disable, C-drive cleanup, residue quarantine, quarantine purge, ordinary restore, and startup restore.
- Returned failures and thrown/unknown attempts refresh relevant current state before final copy; catch paths never retry mutation. C-drive cleanup also reruns the health scan and revokes stale recommendation selection.
- Four synchronization helpers contain only timeline, software, or health reads and have zero mutation-authority hits.

- The install page now has a default-hidden `安装界面都关了，重新扫描` action before advanced diagnostics. It appears only after a non-refused trusted launch and remains available for delayed bootstrap/child-installer observation during the current session.
- Changing the installer path or starting a new prepare attempt clears the old automatic before baseline and hides the action, preventing cross-package comparison.
- `InstallerPostScanCoordinator` contains only software/C-drive reads and diff construction. Both execution recovery and the persistent page action use it, then share one result/report/Application Management presenter.

- Interrupted installer waiting and failed initial post-scans now show `我已完成安装，重新扫描` in the result window; launch refusal and completed initial reports do not.
- Initial post-scan and user-requested retries share one read-only coordinator method. MainWindow retains the original before snapshot, never relaunches the installer, and updates report/Application Management only from a valid after snapshot plus report.
- Failed retries remain path-free and retryable only through another explicit click; no automatic retry or success inference was added.

- A valid initial post-install after snapshot now updates Application Management from the exact verified profile list before the placement/C-drive report is presented.
- No second scan was added, and interrupted, refused, timed-out, or failed post-scan outcomes still publish neither catalog state nor a success claim.

- Every trusted migration attempt now refreshes current application location/C-drive evidence and migration-closure records before classifying the result.
- Only authenticated typed completion shows migration success; uncertain outcomes retain their result title, describe the read-only refresh, and explicitly refuse automatic continuation.

- `卸载干净点` now refreshes current application inventory after every trusted production execution attempt, including cancellation, timeout, transport failure, and incomplete response.
- Residue review still requires completed production plus an explicit validated recommendation; uncertain outcomes only refresh read-only state and never create cleanup authority.

- Startup-entry restore now reloads the current timeline row, verifies and binds one confined manifest id/hash plus state fingerprint and supported Run locator, then executes only after the existing explicit confirmation through `SafetyOperationPipeline`.
- The existing exact startup store remains the only registry mutation authority; it still refuses same-name overwrite, ACL/StartupApproved drift, and invalid state. The new handler owns conservative same-row timeline updates.
- MainWindow no longer calls a startup restore handler or timeline update directly, and the old public `StartupEntryControlOperationHandler.RestoreAsync(manifestPath)` bypass was removed.

- Ordinary quarantine restore now reloads the current timeline row, binds manifest SHA-256 and quarantined payload identity before confirmation, revalidates all evidence immediately before movement, and executes through `SafetyOperationPipeline`.
- The restore handler owns mutation and conservative same-row journal updates. MainWindow no longer calls `_quarantineService.RestoreAsync` directly, and cached timeline view-model paths/state are not execution authority.
- `ActionTimelineStore` can load one row by id; `FileQuarantineService` supports identity-bound restore; six focused tests cover success, confirmation, tampering, stale state, and WPF pipeline wiring.

- Application-page background summaries now count resident applications once by ordinary/system/ownership-pending ownership, then separately explain overlapping running/startup/service/task clues.
- One typed background ownership catalog is shared by the first-view summary and Agent resident candidates; protected-only evidence says `仅供查看`.

- Homepage history and application-page first-view summaries now split C-drive footprints into ordinary applications, explicit system components, and ownership-pending profiles; protected groups say `仅供查看`.
- One typed C-drive ownership catalog preserves the existing deduplicated total/filter membership and is reused by Agent aggregate candidates.

- Health C-drive answers, saved digests, homepage reclaimable totals, and finding text now call only `None`/`Low` clean recommendations low-risk.
- Medium/high clean findings remain visible with observation, snapshot, and rollback guidance; a high-risk-only scan no longer tells the beginner to select low-risk cleanup items.
- Startup dimensions now distinguish ordinary, explicit system, and managed-root ownership-pending evidence; protected-only results are rated `仅供查看`.

- Agent aggregate guidance now shares one candidate catalog that separates ordinary action review, D-installed/unknown C-data review, and protected/system read-only evidence.
- Homepage next steps, general C-drive/applications/migration/uninstall/startup answers, exact startup advice, background review, and startup/service plans now use the same ordinary-action authority as the app drawer.

- Homepage migration-closure findings now distinguish reviewable ordinary apps, protected historical records, and unavailable/ambiguous records through one typed disposition.
- Only reviewable ordinary apps receive `Migrate` plus exact app navigation; both read-only historical states receive `Observe`, no target, generic Applications navigation, and copy that refuses to generate a migration action.

- Installer manual before/after/report controls are now inside a default-collapsed advanced expander; the primary page states normal installation records changes automatically.
- Agent questions and the process/service skill automatically await relevant software inventory through a shared read-only gate.
- Explicit C-drive questions with no current evidence automatically await the existing bounded read-only diagnosis; homepage/Agent requests deduplicate and failures do not leak exception paths.
- Undo Center loads automatically on first entry, exposes `重新加载`, deduplicates navigation, and still force-refreshes after operations.
- Hardware/configuration and machine-health questions plus the hardware skill automatically await one shared bounded machine observation; full diagnosis refreshes the same cache.
- Lightweight machine answers reuse the health-dimension wording but do not construct `HealthCheckSummary` or invent its disk-backed score. Unavailable/not-present states remain explicit.
- The System Diagnosis skill now awaits the shared full read-only health gate instead of sending the beginner to Home to click another button; all unrelated skills skip that scan.
- Exact greetings, thanks, and capability/help questions now skip software inventory and answer immediately; mixed or unknown-app text still hydrates inventory and resolves an exact current profile.
- `帮我体检电脑`, `电脑整体状态怎么样`, and bounded whole-computer optimization wording now use a distinct SystemDiagnosis intent and the shared full read-only health gate; performance/hardware/app questions retain narrower evidence.
- A sandbox-sensitive real process-image test now proves file existence, image filename, and SHA-256 rather than requiring two path APIs to return the same alias string; production exact-path/hash trust policy is unchanged.
- Likely named-app crashes now hydrate inventory, then exact crash/freeze/vague questions show symptom-specific aggregate evidence and keep the exact app drawer action. Generic crash/system questions do not scan apps.
- Unique exact crash/hang questions now read at most 128 approved Application-log candidates from the last 24 hours and retain only availability, count, and latest time. Generic, vague, ambiguous, and explicit-operation questions skip the log.
- Crash evidence has distinct Available/NotFound/Unavailable wording and never copies event messages, property arrays, paths, providers, or event ids into Core/Agent.
- Unique exact app freeze/resource questions now take one 350 ms, 32-process maximum aggregate sample and show count, total memory, and coarse CPU only. Generic, vague, ambiguous, crash-only, and explicit-operation questions skip it.
- Runtime evidence distinguishes Available/NotRunning/Unavailable and never exposes process names, ids, paths, command lines, or exact CPU percentages.
- Six MainWindow exception catches now use fixed path-free workflow conclusions. Known no-change and possible-partial-state failures are deliberately worded differently.
- Ten MainWindow operation/policy/validation failure branches now use fixed phase-aware conclusions. Pre-execution refusals say no change; post-attempt failures require app rescan or Undo Center review.
- Exact named-app uninstall, migration, cache, and startup questions now carry a typed non-executable handoff, re-resolve current inventory, and prepare the same review used by manual drawer buttons.
- Agent handoffs obey existing drawer availability; system apps, unsupported actions, location questions, and troubleshooting remain details-only.
- Unique exact app growth/write questions now prepare or reuse a read-only comparison baseline, re-resolve the current app, and present positive/zero/insufficient/unavailable evidence without paths.
- Growth replies have separate `现在腾空间` and `以后防止继续增长` steps and remain details-only; explicit operations and generic/ambiguous questions do not load this evidence.
- Application location summaries now distinguish the main program from attributed C-drive data/cache write clues and deduplicate those clues without exposing paths.
- A D-installed application with C-drive clues explicitly avoids repeat main-program migration; its migration action remains disabled until a reliable app-specific data-redirection plan exists.
- Post-install summaries/cards/Agent answers now say whether the unique main program landed on C or D and separately count owned versus concurrent C-drive candidates.
- Installation reports with no unique new software refuse to assign main-program location or C-drive ownership; existing app handoffs remain evidence-gated and non-executable.
- Application tiles now replace generic `需关注` with the specific path-free reason while retaining existing attention color, risk, filter, sort, and migration-closure behavior.
- Application drawers now show main-program install size, data, identifiable cache, and recent growth, with distinct unavailable states and no implied releasable-byte claim.
- System-category applications now always recommend retain, disable uninstall/migration/cache/startup in the ordinary drawer, and keep technical details available regardless of scanned commands or background evidence.
- Unknown profiles under the current Windows root or Program Files `WindowsApps` now show `系统归属待确认`, disable ordinary modifying actions/previews, preserve category Unknown, and ignore publisher text as proof.
- Scanner profiles now retain category evidence source, matched rule, fallback state, and confidence; the app drawer explains the basis in one path-free line before storage details, and growth enrichment preserves it.
- The app catalog now labels the unchanged Normal fallback group `普通应用`; enum/tag/button identity and selection logic no longer claim office/study use.
- The `可卸载` filter now shares `CanReviewUninstall` with the drawer, excluding system and managed-root ownership-pending profiles even when they expose commands; system preview also refuses ordinary uninstall generation.
- The application summary now separates main programs on C/D, C-drive data/cache app clues, and deduplicated `占 C 盘` membership; non-C field entries are ignored and the shared summary has a stable AutomationId before the grid.
- Empty filters and completed empty inventory now clear every drawer conclusion, selection, technical detail, preview, pending target, label, tooltip, and context action through one typed state; normal opens reset technical details too.
- Post-uninstall residue review now has its own drawer availability: ordinary profiles remain reviewable without an uninstall command, while system and managed-root ownership-pending profiles are denied again at the handler boundary and during async button restoration.
- Migration closure warnings now use one typed drawer state: protected Agent conclusions remain first and cannot open a migration plan, while ordinary D-installed apps with an incomplete closure can still regenerate a read-only safety plan.
- Central uninstall, cache cleanup, and startup control methods now recheck the shared drawer action before action-specific reads, plan/preparation construction, windows, or pending target assignment.
- Migration closure tile/sort/summary presentation now treats protected historical records as `仅供查看`; only ordinary reviewable closure warnings become red and prioritized.
- Production uninstall/migration identity readiness remains fail-closed for unsigned packages.

## Verified

- Latest synchronization slice: final full regression 937/937; solution build 0 warnings/errors; 336 strict UTF-8 C#/XAML files, invalid 0, replacement-character files 0.
- Related groups: cache 213/213; startup 207/207; C-drive cleanup 243/243; residue 220/220; Undo/restore 220/220.
- Static: each of seven direct pipeline methods has pipeline/attempt/catch-guard counts 1 and synchronization-call count 2; four read-only helpers have mutation-authority count 0; old startup helper references 0.
- Test infrastructure: `SourceMethodExtractor` is covered 3/3 and used by the new cache/startup/direct-cleanup/residue/Undo contracts, avoiding adjacent-method false greens.
- Runtime: no real destructive operation was run; positive mutation remains a disposable-fixture/signed release gate.

- Latest slice: focused persistent installer observation 25/25; related installer/report/product/failure tests 243/243; full 927/927; build 0 warnings/errors.
- Static: 332 strict UTF-8 files; Main XAML parse; persistent button id/click 1 each, default collapsed and before advanced diagnostics; read-only coordinator one software read/one footprint read/one report build; persistent handler and coordinator launcher/pipeline/mutation authority 0.
- Visual remains Warn from the already-recorded Computer Use Debug-app launch timeout; no screenshot is claimed.

- Latest slice: focused installer recovery 23/23; related installer/report/product/failure tests 241/241; full 925/925; build 0 warnings/errors.
- Static: 332 strict UTF-8 files; strict XAML parse; retry/close AutomationIds each 1; production launch 1; read-only retry call 1; user-driven loop 1; catalog binding 1; retry method launcher/pipeline/mutation authority 0.
- Visual: Computer Use helper was reachable, but launching the Debug app timed out and passive app/window polling found no OMNIX window. No screenshot is claimed.

- Latest slice: focused installer coordinator 16/16; related installer/report/product tests 232/232; full 918/918; build 0 warnings/errors.
- Static: 332 strict UTF-8 files; success gate/catalog update/report presentation each 1 in correct order; changed-method mutation authority 0; only the existing two read-only inventory calls remain in the method.

- Latest slice: focused migration coordinator/orchestration 7/7; related migration/app-action/product 256/256; full 917/917; build 0 warnings/errors.
- Static: 332 strict UTF-8 files; migration attempt/scan/closure/completion branches each 1 in correct order; changed-method mutation authority 0.

- Latest slice: focused official-uninstall coordinator/orchestration 6/6; related official-uninstall/residue/evidence/product 387/387; full regression 916/916; build 0 warnings/errors.
- Static: 332 strict UTF-8 C#/XAML files; uninstall post-attempt branch, scan, double gate, and residue call each exactly 1; changed-method mutation authority 0.

- Latest slice: focused startup restore 7/7; related startup/timeline/quarantine/product 204/204; full regression 915/915; solution build 0 warnings/errors.
- Static: 332 non-generated C#/XAML files strict UTF-8, replacement-character files 0, direct MainWindow startup restore/store/timeline calls 0, old public startup restore API 0, restore confirmation AutomationIds exactly 1 each.

- Latest slice: focused quarantine restore 6/6; related quarantine/timeline/startup/product 227/227; full regression 908/908; solution build 0 warnings/errors.
- Static: 330 non-generated C#/XAML files strict UTF-8, replacement-character files 0, direct MainWindow quarantine restore calls 0, old combined restore helpers 0, restore confirmation AutomationIds exactly 1 each.

- Latest slice: focused background ownership 3/3; related application/Agent/startup/system 240/240; full regression 902/902; build 0 warnings/errors.
- Static: 328 non-generated C#/XAML files strict UTF-8, replacement-character files 0, focused mutation-authority hits 0, old resident projection/raw summary patterns 0, shared background catalog bindings 2, app-summary AutomationId 1.
- Latest slice: focused C-drive ownership summaries 3/3; related application/health/Agent/system 238/238; full regression 899/899; build 0 warnings/errors.
- Static: 325 non-generated C#/XAML files strict UTF-8, replacement-character files 0, focused mutation-authority hits 0, old total-only/Agent projection patterns 0, shared catalog bindings 3, summary AutomationIds 1 each.
- Latest slice: focused risk/ownership 5/5; related health/Agent/scanner 244/244; full regression 896/896; build 0 warnings/errors.
- Static: 323 non-generated C#/XAML files strict UTF-8, replacement-character files 0, focused mutation-authority hits 0, action-only low-risk pattern 0, category-only explicit-system pattern 0.
- Latest slice: focused aggregate authority 5/5; related Agent/product/system/ownership/storage 234/234; full regression 891/891; build 0 warnings/errors.
- Static: 321 non-generated C#/XAML files strict UTF-8, replacement-character files 0, focused mutation-authority hits 0, four legacy raw-candidate patterns 0.
- Real WPF retry after the user confirmed updated antivirus definitions: Computer Use discovery reachable; `launch_app` for built `Css.App.exe` timed out; follow-up app/window poll found no OMNIX window; visual gate remains Warn.
- Static: focused empty-state authority hits 0; `DrawerTechnicalDetailsButton` AutomationId hit 1; zero-profile clear call hit 1; 314 non-generated strict UTF-8 C#/XAML files; replacement-character files 0.
- Agent automatic inventory/C-drive/machine diagnosis and Timeline loading contain no operation, mutation, restore, purge, or delete authority.
- Beginner failure copy for software inventory, health scan, and Timeline loading is path-free.
- Production readiness still requires same-signer trusted App/Worker evidence and revalidates before execution.

## Not Verified

- Fresh screenshots for the latest installer/Agent/Timeline/machine/crash/error-boundary/category/closure changes: after the user confirmed updated antivirus definitions, Computer Use listed Windows apps but the OMNIX-Entropy `launch_app` request timed out, passive refresh found no target, and no fallback UI automation was used.
- Signed uninstall/migration on disposable fixtures remains a release gate.
- Earlier MSIX, hardware Agent, skill-card, and C-drive root-cause visual gates retain Warn status.

## Latest Update - 2026-07-16 Actionable uninstall post-scan result

- Objective completed: post-uninstall advice now has typed, clickable `RetryReadOnlyScan` and `ReviewResidue` commands; clean results retain only Close.
- The result window is presentation-only. It returns intent to the plan, and `MainWindow` re-reads current inventory before following it. Retry stops at an inline read-only conclusion. Review alone can enter the existing separate cleanup-confirmation flow. Close is never consent.
- Verified: focused 9/9; related uninstall/product 361/361; full 948/948; solution build 0 warnings/errors; 341 strict UTF-8 C#/XAML files; inspected first-view render `.omx/qa-uninstall-post-scan-action.png`.
- Not verified: no real uninstaller or residue cleanup ran; signed/disposable-fixture production acceptance remains a release gate.
- Exact next recommended action: preserve the exact bounded paths already present in personal large/duplicate-file findings and add a read-only inspection handoff from the beginner-facing result without turning selection into cleanup consent.

## Latest Update - 2026-07-16 Personal-file read-only location inspection

- Objective completed: each large/possible-duplicate candidate now has an explicit `查看位置` action. The default row stays path-free; a concise modal lists only that finding's captured locations.
- The detail window returns intent only. MainWindow validates the selected value against the current session evidence, and the isolated launcher revalidates a fully qualified local existing file before selecting it through fixed Windows Explorer plus `ArgumentList`.
- Verified: focused 10/10; related 191/191; full 953/953; build 0 warnings/errors; 345 strict UTF-8 source/XAML files; all XAML parses; no mutation authority in the window/handler; inspected `.omx/qa-personal-storage-inspection.png`.
- Not verified: no real Explorer or personal file was opened during automated verification. This is a read-only manual fixture check, not a release blocker for destructive authority.
- Exact next recommended action: repair persisted health-digest navigation after restart. A displayed digest must trigger or hydrate a fresh read-only health session before `查看最新 C 盘证据` claims that the detailed C-drive page is current.

## Latest Update - 2026-07-16 Persisted digest to current C-drive evidence

- Objective completed: a restart-loaded digest no longer claims that empty C-drive controls are the latest evidence. The button says `重新体检并查看当前证据` until a successful current-process health session exists.
- Click is async/non-reentrant, navigates with honest loading state, starts or joins the shared read-only gate, and only shows success after gate completion plus `_lastHealthSummary`. Failure retains history but refuses to call it current detail.
- Verified: focused 16/16; related 195/195; full 954/954; build 0 warnings/errors; 346 strict UTF-8 source/XAML files; all XAML parses; mutation authority 0; success copy order verified.
- Visual status: static UX contracts pass; whole-MainWindow screenshot remains Warn because the already-recorded Computer Use local-app launch timed out and no fallback automation is permitted.
- Exact next recommended action: improve Agent background/startup handoffs so a finding that names one or more applications opens Application Management with the relevant application context preserved instead of an unfiltered grid.

## Latest Update - 2026-07-16 Agent background context handoff

- Objective completed: each safe background-review item has a `查看应用` details-only action; path-like/unsafe names cannot navigate. Aggregate startup/background replies now carry typed `AppCatalogFilter.Resident`.
- MainWindow only accepts Resident aggregate context, clears old search, starts/joins inventory loading, refreshes the resident grid, and does not open startup control. Per-item actions reuse the unique app resolver and stop on ambiguous/missing targets.
- Verified: focused 15/15; related 251/251; full 956/956; build 0 warnings/errors; 347 strict UTF-8 source/XAML files; all XAML parses; mutation authority 0; button hook/AutomationId/filter assignment each 1.
- Visual status: source and accessibility hooks pass; whole-MainWindow screenshot remains Warn because the existing Computer Use local-app launch timeout has not cleared and fallback UI automation is forbidden.
- Exact next recommended action: continue aggregate migration/uninstall application-context handoffs and the remaining visible preview audit; keep mutation acceptance behind signed/disposable fixtures.

## Latest Update - 2026-07-18 Agent migration/uninstall catalog handoff

- Objective completed: aggregate migration, uninstall, and startup/background answers now open `占 C 盘`, `可卸载`, and `后台常驻` application views respectively.
- MainWindow accepts only those three typed filters, clears stale search, joins current inventory loading, refreshes the grid, and shows honest filter-specific state. It does not open an operation plan.
- Verified: focused 8/8; related 279/279; full 957/957; build 0 warnings/errors; 347 strict UTF-8 source/XAML files; all 17 XAML parse; migration/uninstall filter assignments exactly one each.
- Visual status: no new control was added; whole-MainWindow runtime click-through remains Warn because the recorded Computer Use local-app launch timeout has not yet cleared and fallback UI automation is forbidden.
- Exact next recommended action: audit the remaining V1 visible entries for a preview or navigation action that stops before current-state handoff; keep signed/disposable real mutation acceptance separate.

## Latest Update - 2026-07-18 Agent next-step typed application handoff

- Objective completed: persistent Agent next-step buttons now preserve `Resident` or `CDrive` application context instead of opening a broad Apps page.
- Each action has a stable filter-aware AutomationId and the XAML passes the complete typed action. MainWindow validates navigation-only state, page/filter consistency, and delegates to the existing bounded filter handoff without opening any plan.
- Verified: focused 2/2; related 275/275; full 959/959; build 0 warnings/errors; 348 strict UTF-8 source/XAML files; all 17 XAML parse.
- Visual status: Computer Use `launch_app` for the built Debug exe timed out; a follow-up `list_windows` found no OMNIX target. No fallback UI automation or screenshot is claimed.
- Exact next recommended action: continue the V1 visible-entry audit for a remaining preview/navigation action that loses current evidence or cannot reach its intended safe next step.

## Latest Update - 2026-07-18 Home migration-closure catalog handoff

- Objective completed: targetless homepage migration-closure findings now open the current `占 C 盘` application catalog instead of all applications.
- Exact app findings remain current-inventory-resolved and cannot carry an aggregate filter at the same time. The aggregate branch only accepts the Applications destination and delegates to the existing bounded catalog handoff.
- Verified: focused 5/5; related 199/199; full 960/960; build 0 warnings/errors; 348 strict UTF-8 source/XAML files; all 17 XAML parse.
- Visual status: no new control was added; the existing whole-window Computer Use launch Warn remains the applicable runtime gap.
- Exact next recommended action: continue auditing previews and navigation for another user-visible safe next step that remains disconnected from current evidence or result synchronization.

## Latest Update - 2026-07-18 C-drive application handoff truth and reuse

- Objective completed: the C-drive root-cause `占 C 盘应用` action now reuses the same bounded typed CDrive handoff as Agent surfaces.
- This removes a false populated-state case caused by checking global inventory count instead of filtered items and eliminates duplicate page/filter/load/status code.
- Verified: focused 5/5; related 282/282; full 960/960; build 0 warnings/errors; 348 strict UTF-8 source/XAML files; all 17 XAML parse.
- Visual status: no new control; the existing Computer Use whole-window launch Warn remains applicable.
- Exact next recommended action: audit result windows and remaining previews for missing typed next actions or post-attempt current-state synchronization.

## Latest Verification - 2026-07-18 isolated GUI lifecycle

- An isolated workspace-data Debug instance remained alive after five seconds with a real `OMNIX-Entropy` window title and handle; startup is not crashing.
- Computer Use then uniquely returned the isolated OMNIX window, but state capture failed at `Computer Use app approval timed out`. No click/screenshot occurred.
- The exact isolated process was stopped. No test app instance remains.
- Visual status: startup lifecycle Pass; interactive whole-window screenshot remains Warn. Do not use PowerShell UIAutomation/SendKeys as fallback.

## Known Risks Or Blockers

- Do not turn natural-language questions or automatic read-only evidence loading into consent for system modification.
- Do not trigger a full C-drive scan for unrelated hardware/settings questions; only the explicit system-diagnosis/C-drive intents may use the shared full health gate.
- Do not cache failed/cancelled loads or suppress post-operation Timeline refresh.
- Do not default package trust to true or add unsigned mutation modes.
- Do not use PowerShell UIAutomation/SendKeys to bypass Computer Use failure.
- Do not treat Application-log correlation as a root cause or retain formatted messages/property values outside the Win32 adapter.
- Do not present one short runtime sample as a cause, trend, or reason to end a process; no trustworthy process hint must remain Unavailable.
- Do not copy raw exception, operation, policy, or validation error text into beginner-visible status/message controls; classify by verified state and next recovery action.

## Exact Next Recommended Action

Audit the remaining non-MainWindow execution surfaces and visible navigation/actions against the V1 plan. Prioritize any entry that still stops at preview without a hardened handoff, or any worker/window execution that can return without refreshing current state; keep signed/disposable runtime acceptance separate.
