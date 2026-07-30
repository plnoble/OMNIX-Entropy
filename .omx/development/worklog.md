# Development Worklog

## 2026-07-30 - Began 0.1.5 review and release

- User explicitly authorized review, commit, version bump, push, signing, and publication for the combined working-tree slice.
- Chosen version is the next patch, 0.1.5. Existing main-direct release history and guarded personal-signing workflow remain in force; no installer execution or machine-maintenance operation is authorized.
- Review sequence: complete diff and safety-boundary audit; version/release-note update; focused/full/Release/integrity/GUI gates; source commit/push/CI; immutable target-bound signing/package verification; draft download-back verification; public latest receipt; final records commit.
- Review found one beginner-identity defect: unregistered paths could be called the main program. Wording and tests now distinguish exact registered main installs from programs, copies, and updater payloads. No new mutation authority or release-blocking safety defect was found.
- Updated source version and release notes to 0.1.5.
- Local pre-commit gates pass: full Debug 1100/1100; Release 0 warnings/errors with 0.1.5 product/file versions; source integrity 395 files/18 XAML; script parsers pass; app-family and home/C-drive WPF smokes pass with no operation executed; diff and mutation-authority checks are clean.
- Host release prerequisites pass: authenticated `plnoble` GitHub CLI with `repo`/`workflow`, fixed origin, SignTool found, and one eligible explicitly selected CurrentUser RSA code-signing certificate. Signing remains blocked until the pushed commit passes CI.

## 2026-07-30 - Completed system cleanup and app-family decision slice

- Finalized known-store measurement bounds for both files and directories, preserved exact software evidence through growth-profile cloning, and made the family drawer account for other C-drive program/update payloads separately from C-drive user data.
- Real app-family smoke verified three OpenCode records, one exact official uninstall entry, disabled actions for unregistered copies, and quantified C-drive remainder. The home/C-drive smoke reported 4.0 KB immediately safe cleanup versus at least 3.3 GB of temp/system-cache candidates and executed no operation.
- Completion gates: focused 12/12; full Debug 1099/1099; Release 0 warnings/errors; source integrity 395 files and 18/18 XAML; both Windows PowerShell scripts parse; screenshots inspected without product-content overlap.
- Delivery boundary: no cleanup, uninstall, migration, commit, version bump, push, signing, installer, or release action occurred. Public/installed 0.1.4 is unchanged.

## 2026-07-30 - Began system cleanup and app-family implementation

- User authorized continued development after reviewing evidence that the root scan misses material nested temp data and that Antigravity/OpenCode cards do not explain exact install/data/updater identities.
- Scope is read-only evidence and beginner decisions. Existing official-uninstaller, migration snapshot, quarantine, confirmation, and `OperationPipeline` boundaries remain unchanged.
- Planned sequence: red contracts; bounded known-store probe; cleanup handling classification; version/source/data-size profile evidence; family explanation without identity merging; quantified migration outcome; full and GUI gates.
- Added three user-shaped red contract files. The expected compile failure proves the product still lacks the cleanup-opportunity model/probe, family-aware drawer/tile overloads, exact-entry wording, version/source evidence, and C-drive data-size evidence; no production behavior changed yet.
- Implemented a bounded, cancellable, read-only known-store probe for user temp, Windows temp/update/delivery caches, DirectX shader cache, and crash dumps. User-review and Windows-managed findings use different beginner copy; truncated or partially inaccessible measurements render as lower bounds and never become cleanup authority.
- Preserved display version and registry source through inventory and growth enrichment; populated deduplicated data size and C-drive data size with cached bounded sizing. App total-size sorting no longer double-counts cache already included inside a measured data root.
- Added explanation-only app-family normalization for version/user-scope suffixes. Tiles show related-record counts; the drawer identifies the exact selected entry, enables official uninstall only for that entry, and states whether moving the current main program changes C space or leaves family data/program copies behind.
- Added stable drawer AutomationIds, first-view ordering contracts, a no-horizontal-scroll C-drive source list, and an isolated three-entry OpenCode WPF smoke. The smoke verifies one enabled exact official uninstall entry, one disabled D-drive copy, quantified C-data persistence, and no operation execution.
- The existing home/C-drive WPF smoke now reports C at 86.5%, safe cleanup 4.0 KB, and at least 3.3 GB of known temp/system-cache candidates while retaining the 19.5 GB comfort-target gap. It executed no operation.

## 2026-07-30 - Investigated system-cleanup blind spots and duplicate app cards

- Confirmed that current disk recommendations focus on top-level Temp classification plus recycle-bin/system-reserved probes; they do not surface material nested current-user temp, crash-dump, shader-cache, update-download, or delivery-optimization stores as a beginner cleanup inventory.
- A bounded read-only host check found at least about 4.59 GB in the first 20,000 current-user temp files, about 102.2 MB of crash dumps, 13.4 MB of DirectX shader cache, and 5.2 MB of Windows Update downloads. These are candidates requiring age/lock/ownership or Windows-managed cleanup checks, not automatic deletion authority.
- Confirmed that software inventory groups only exact display-name/install-path duplicates and does not preserve display version in `SoftwareProfile`. This explains why same-family entries remain separate and unexplained.
- Read-only Antigravity/OpenCode evidence shows that D-first installation does not close C-drive growth: updater and roaming data remain on C. OpenCode also has multiple C/D program locations, while only the exact registered entry currently has a trustworthy official uninstall route.
- Chosen product direction: group entries for explanation but never for execution; label exact entries as registered install, portable/unregistered copy, updater payload, or user data; quantify what moving the main program changes and what remains on C.
- No uninstaller, cleanup, migration, process, registry mutation, or product source change ran.

## 2026-07-30 - Began impact-aware C-drive priority correction

- User rejected the current primary suggestion on quantitative grounds: 337.6 MB is negligible beside the roughly 19.5 GB required to reach the stated 80% comfort target.
- Root cause is decision policy, not scanner absence: the current presenter treats any executable low-risk cleanup as the primary action, and the bounded key-finding list ranks safety ahead of impact.
- Chosen scope keeps the safe cleanup visible but makes it secondary when it contributes less than 10% of the target and larger read-only evidence exists. The Agent will quantify contribution and remaining gap, then route the primary action to personal files or major space sources.
- No deletion authority, cleanup eligibility, operation execution, installer, updater, version, or release behavior is included.
- Added a user-shaped 19.5 GB versus 337.6 MB contract and an impact-first bounded-finding contract. Safe evidence remains visible but no longer outranks multi-GB observations.
- Implemented contribution percentage, the 10% materiality rule, impact-first primary/optional-safe secondary actions, and quantified beginner copy.
- Repaired the GUI smoke readiness check after repeated WPF control-type enumeration failures; stable per-row AutomationIds and a failure screenshot now provide the evidence.
- Final gates: focused 12/12; full Debug 1087/1087; Release 0 warnings/errors; source integrity 390 files/18 XAML; PowerShell parse pass; diff check has no errors. Real WPF smoke verified the impact-first button, optional cleanup preview, scroll behavior, and `noOperationExecuted=true`.

## 2026-07-30 - Diagnosed the empty-adjustment impression

- Read the user's latest local health digest in read-only mode. It reports C at 86.5%, score 64, about 337.6 MB potentially safe to process, five unexpected-root observations, and 105 ordinary C-drive application clues.
- Found a concrete contradiction: `HealthCheckSummaryBuilder` calculates reclaimable bytes from all low-risk recommendations, but `HealthDigestBuilder` counts only the first five rendered key findings. Five higher-impact observation cards can occupy those slots and hide a later executable cleanup, producing “可安全处理 337.6 MB” beside “低风险清理 0 项”.
- Found a workflow gap: `DriveHealthPlanPresenter` correctly selects the safe item as the first action, but after that small contribution it offers no distinct secondary action for the much larger remaining gap.
- Chosen scope keeps cleanup authority unchanged: prioritize actionable findings in the summary, expose honest opportunity counts, and add a read-only “continue finding larger space” path.
- Added red contracts for hidden cleanup ordering, explicit opportunity counts, secondary actions, and stable UIAutomation identity. Red failed only on the missing secondary-plan model/API.
- Implemented recommendation priority, safe-now/investigate-next plan copy, and a secondary read-only action. The primary action still only previews an existing low-risk operation; the secondary action only navigates to large-file or root-cause evidence.
- Real WPF smoke showed 1 safe item, 7 investigation items, a 19.5 GB remaining gap, both visible buttons, successful root-cause navigation, and `noOperationExecuted=true`. Screenshot inspection found no clipping or overlap.
- Final gates: focused 11/11 plus adjacent 226/226; full Debug 1086/1086; Release 0 warnings/errors; source integrity 390 files/18 XAML; no new mutation-authority hits; diff check clean.

## 2026-07-30 - Published 0.1.4

- Committed release-preparation records as `68a7bd4`, pushed `main`, and required GitHub CI `30517231962` to pass before any artifact signing. CI passed Release build, full tests, and source integrity on the exact release target.
- Built a fresh portable candidate and a fresh signed payload from `68a7bd4`. Independent verification covered 110 payload files and confirmed the expected App/worker signer; disposable-machine behavioral acceptance remains intentionally unclaimed.
- Built the Inno setup with the existing personal publisher. Independent verification confirmed version 0.1.4, default `D:\Software\OMNIX-Entropy\Install`, visible directory selection, no silent install, signer `5688958FEA0056861558E8DCF9D2381AF46074B2`, and `CanStageGitHubRelease=true`.
- Created a draft with exactly four assets. All four were downloaded back and matched local SHA-256 values; the downloaded setup independently passed the same installer verifier.
- Published `v0.1.4` and confirmed `releases/latest` reports non-draft/non-prerelease, target `68a7bd4`, and four uploaded assets. Public setup: 14,842,384 bytes; SHA-256 `9B32AB55D61637A14275D741D6E120F5D07E3FEC960DFF5E890BA445BA7AA48D`.
- No installer, product process, UAC flow, install/uninstall, certificate/trust mutation, antivirus action, or system operation ran.

## 2026-07-30 - Began public 0.1.4 release

- User explicitly requested push and publication after the reviewed local 0.1.4 commit.
- Host-only read-only preflight found the existing personal publisher signer `5688958FEA0056861558E8DCF9D2381AF46074B2`, accessible non-exported private key, RSA/code-signing eligibility through 2029, Windows SDK SignTool, Inno Setup 6, and an authenticated `plnoble` GitHub CLI session.
- Release scope excludes installer execution, install/uninstall, certificate creation/export, trust-store changes, antivirus changes, and `LocalMachine` actions.
- Chosen sequence: commit release-preparation records; push and wait for CI; only then build/sign/verify; create draft; download all assets and independently verify; publish; recheck latest; record completion.

## 2026-07-30 - Began 0.1.4 combined-slice review

- Confirmed `main` at `f372153`, source version 0.1.3, and the expected uncommitted multi-disk, homepage-scroll, and beginner health-plan files.
- Review found three concrete defects: prior plan steps/safety survive a drive switch or new scan; selected recommendation-specific explanation is replaced by generic copy; and data-drive prevention guidance still says to move data to D.
- Expanded review to system-drive history scope before editing. No system operation, installer, signing, push, or release action was performed.
- Added complete plan reset for drive switches, scan starts, cancellation, and failure; preserved the specific selected-card explanation; made data-drive destination advice neutral; kept digest history system-drive-only and labelled it explicitly.
- The first full run passed 1084 tests and failed one stale source-string contract after “C 盘证据” became “系统盘证据”; the focused correction passed 6/6 and final full Debug passed 1085/1085.
- The multi-disk GUI smoke hit `RPC_E_SERVERFAULT` twice because it enumerated every desktop list item. Scoping enumeration to windows owned by the OMNIX process removed the external fault; the final smoke listed C/D, selected D, showed the D pending conclusion, and captured the updated system-history label.
- Final evidence: focused review 26/26; full Debug 1085/1085; Release 0 warnings/errors; integrity 390 files and 18/18 XAML; both smoke parsers and both real WPF smokes pass; `ProductVersion=0.1.4`; `git diff --check` has no errors.

## 2026-07-29 - Beginner C-drive health-plan diagnosis

- User feedback exposed the remaining product gap: the UI reports 86% used and a few hundred MB of safe cleanup, but never says what “healthier” means, how many GB are actually needed, or what to do when safe cleanup is insufficient.
- Existing evidence is enough to answer honestly: total/free bytes, low-risk reversible cleanup recommendations, personal-storage candidates, and root-cause cards. The current C-drive page resets recommendation selection to null after every scan, forcing a beginner to make the first technical choice.
- Chosen scope: a read-only 80% comfort-target plan, explicit safe-cleanup contribution and remaining gap, three plain-language steps, and automatic selection of the first executable low-risk recommendation for preview only.
- Implemented `DriveHealthPlanPresenter` with exact target/contribution/gap bytes, strict executable + reversible low-risk evidence, comfortable/no-evidence states, and navigation-only actions. The homepage keeps only the concise target/progress/button band; the detailed three steps and safety boundary live on the disk page.
- Converted the complete right-hand recommendation region into one outer scroll surface and disabled nested scrolling on the recommendation list. The preferred low-risk card is selected after scanning; clicking the health-plan action scrolls to a self-contained preview panel with its still-unexecuted confirmation-entry button.
- Expanded the isolated WPF smoke to prove first-view 80% target copy, quantified bytes, navigation to the selected low-risk preview, visible preview entry, unchanged Agent response and scroll surfaces, `noOperationExecuted=true`, and two screenshots: `.omx/qa-home-agent-next-action.png` and `.omx/qa-drive-health-plan.png`.
- Verification closed with focused 13/13, full Debug 1084/1084, Release 0 warnings/errors, source integrity 390 files and 18/18 XAML, real WPF smoke pass, and diff check pass with only existing line-ending notices.

## 2026-07-29 - Homepage scrolling diagnosis

- Reproduced the screenshot structure in XAML: both `HealthDigestHistoryListBox` and `KeyFindingsListBox` are children of unconstrained vertical `StackPanel`s inside fixed-height cards.
- Root cause: the lists are measured at desired content height rather than remaining card height, so their internal scroll viewers have no bounded viewport and the outer card clips the overflow.
- Added a failing layout contract, then replaced both card roots with constrained `Grid` rows. The key-finding `ListBox` owns its scroll viewport; the complete variable-height history summary, rows, and evidence button share one outer `ScrollViewer` containing a non-scrolling `ItemsControl`.
- The first bounded-history attempt still allocated the summary and evidence button outside the useful viewport. A real screenshot exposed the clipped button, so the semantic history region became the scroll unit rather than only its daily rows.
- Expanded the isolated WPF smoke to create enough findings for overflow and to exercise `ScrollPattern` on both regions. Final runtime evidence moved key findings from 0% to 25% and history from 0% to 100%, kept the next-action navigation working, executed no operation, and captured `.omx/qa-home-agent-next-action.png`.
- Verification closed with focused contracts 10/10, full Debug 1080/1080, Release 0 warnings/errors, source integrity 388 files and 18/18 XAML, smoke parser pass, and diff check pass with only existing line-ending notices.

## 2026-07-29 - Multi-disk scan planning

- Confirmed the current 0.1.3 UI collapses `DriveRootComboBox`, then initializes every logical drive internally and selects C. The crawler itself accepts another root, so the visible C-only limit is not a storage-engine limit.
- Found three correctness blockers behind simply unhiding the control: `DiskScanner` always adds Windows/C-drive big-rock evidence, `CategoryClassifier` treats every non-whitelisted top-level folder as a C-drive anomaly, and root-cause/health summaries hardcode C.
- Chosen scope: a non-editable menu of ready fixed local disks, system disk first; system-only probes and expected-root policy only on the actual Windows drive; whole selected data drive as the read-only large/duplicate-file scope; clear stale conclusions and invalidate the lazy load gate when selection changes.
- Added focused red contracts across drive-target presentation, disk scope/classification, load-gate invalidation, dynamic summaries, and WPF source. Red fails only on the intentionally absent interfaces and behavior.
- Implemented the drive-target presenter and visible selector, system/data scan-scope policy, dynamic beginner summaries, selected-drive personal-file analysis, and selection invalidation. The selector is disabled during an active scan.
- Tightened the safety boundary after review: a data-drive folder named `Temp` cannot create a cleanup operation or a “safe cleanup” card action; data-drive folder cards say that names and sizes alone are not proof.
- Added stable selector/item accessibility names and a reusable real-WPF smoke. The smoke listed `系统盘 C 盘，剩余 38.7 GB` and `D 盘，剩余 256.2 GB`, selected D, and verified the beginner pending conclusion without starting a scan.
- Final gates: focused 200/200; full Debug 1079/1079; Release 0 warnings/errors; source integrity 388 files, no invalid UTF-8/replacement characters, 18/18 valid XAML; PowerShell smoke parser and `git diff --check` pass. Screenshot: `.omx/qa-multidisk-selector.png`.
- No version bump, commit, push, release, installer execution, full-disk scan, or system mutation was performed.

## 2026-07-29 - Managed install-layout closure

- Verified the user's corrected 0.1.2 installation in the host CurrentUser context: exact managed exe and registry path, version 0.1.2, valid expected signer and GlobalSign timestamp, fixed D drive, non-reparse directories, and no doubled executable.
- Reviewed local unpushed commit `fe2e012`. Its guarded release-page launch and single cleanup authority are sound, but it did not implement the installer-layout prevention or distinct recovery message. Existing focused contracts passed 15/15.
- TDD red added three protections: `AppendDefaultDirName=no`, doubled-layout refusal with exact reinstall guidance/no HTTP request, and guarded release-page failure copy. Red failed only the first two missing behaviors; the Claude launch guard was already green.
- Added the Inno directive and an internal typed layout exception. `DownloadAndVerifyAsync` maps only that type to beginner guidance; the strict layout check and all other failure classes are unchanged. Focused green is 16/16.
- Mechanically removed only trailing blank lines from the 15 new archive files reported by `git diff --check`; no record body was rewritten.
- Completed local gates: full Debug 1068/1068; Release build 0 warnings/errors; source integrity 385 files, zero invalid UTF-8/replacement files, 18/18 XAML; `git diff --check` clean. Git's ignore-blank-lines comparison against `fe2e012` confirms the archive edits contain no nonblank content changes.
- Committed `d052ae1` on top of Claude's reviewed `fe2e012` and pushed both to `main`. GitHub CI `30424760666` first aborted its testhost after 429 passing tests with no failed assertion; a single failed-job retry on a fresh runner passed build, all 1068 tests, and source integrity.
- Built a fresh 110-file candidate from `d052ae1`, signed App and Worker with CurrentUser publisher `5688958FEA0056861558E8DCF9D2381AF46074B2` and a GlobalSign R45 timestamp, and independently verified same-signer readiness.
- Built and signed the 0.1.3 Inno setup. Independent verification confirmed version 0.1.3, the expected signer, D-first default, visible directory selection, no silent install, and `CanStageGitHubRelease=true`.
- Created a draft release, downloaded all four assets into a fresh directory, matched every SHA-256 to its local input, and independently reverified the downloaded setup. Published `v0.1.3`; the unauthenticated latest endpoint reports non-draft/non-prerelease, four assets, commit `d052ae1`, and the expected channel identity.
- Public setup: 14,833,040 bytes; SHA-256 `33C332A658B6B5CF08304AE67A0E2A7A5AAE3CC23A357E5407913E305F05C73A`.
- No installer, product process, update, UAC flow, certificate/trust, registry write, antivirus action, or system mutation ran.

## 2026-07-29 - Released-state review and record readability

- Reviewed released `main` (`e4f0cf0`) by re-deriving evidence instead of reading it. Source integrity returned 385 files and 18/18 XAML; Release build 0 errors and 0 warnings; full Debug 1067/1067. Every number in the records matched, and the Release build was cleaner than recorded because the NuGet vulnerability index was reachable this time, so the 18 NU1900 warnings did not reappear.
- Re-derived the public release from GitHub rather than the records: `v0.1.2` non-draft/non-prerelease with four assets, setup 14,832,504 bytes, GitHub digest matching both the records and the published `omnix-release.json`, `InstallerManifestSHA256` matching GitHub's own digest, `CommitSHA` `4a3e30c` green on CI `30339140502`, and `e4f0cf0` green on `30340365520`.
- Read the update chain end to end. The security boundary holds; the design choice worth preserving is that the package signer must match the running App's signer, not merely the thumbprint the manifest claims, so a substituted manifest cannot introduce a new signer.
- Fixed an unguarded `Process.Start` in `UpdateWindow.OpenReleasePage_Click`. `Css.App` has no global `DispatcherUnhandledException` handler and every other launch site in the app already catches, so this was the one place a ShellExecute failure could terminate the app.
- Removed five redundant `CleanupStaging` calls from `DownloadAndVerifyAsync`'s catch blocks. The `finally` guard already covered every exit; it is now the single documented cleanup authority.
- Split the development records by date. Discovering the real file shape mattered: each record had grown in two directions at once, newest-first at the top and oldest-first in the tail, with the reusable Pre-Change/Pre-Delivery/Template sections sandwiched in the middle. A positional cut would have archived recent entries and deleted the operative gate checklists.
- Verified the split lost nothing by comparing all 1,724 entry blocks across the seven records against their `HEAD` versions; every block is present verbatim in a live or archive file. Largest live record is now 62 KB, largest archive 180 KB.
- Rewrote `handoff.md`'s canonical sections, which still described the 2026-07-22 signing blocker and contradicted the file's own newest entries. Its accumulated ~250-line rolling body moved to the archive; the file is now 13 KB.
- The user reported completing the manual 0.1.2 bootstrap install. A read-only uninstall-registry query confirmed it, and also exposed a new blocker: `InstallLocation` is `D:\Software\OMNIX-Entropy\Install\Install\`, so `ResolveProductRoot` rejects the layout and the in-app update path cannot run on this installation. Checking the install rather than accepting it as done is what surfaced this; the bootstrap succeeded as an install and failed as an enabler.

## 2026-07-28 - In-app verified update installation

- User runtime evidence proved 0.1.0 can detect public 0.1.1 but can only open the release page. The current source deliberately contains no package download or install-launch path, so this is a missing product capability rather than a failed update check.
- Chosen boundary: download only after a user click, stage beside the D-first installation, verify exact length/SHA-256 and trusted signer continuity with the running App, require final confirmation, and launch only the interactive setup through `SafetyOperationPipeline`. No silent update or trust bypass.
- Added `PersonalReleasePackageDownloader`, a managed non-system-drive staging policy, same-signer continuity against the running executable, final-move revalidation/cleanup, cancellation, and a launch handler bound to the real current executable.
- Reworked the update window with `DownloadAndInstallUpdateButton`, clear download/verified/failure states, final MessageBox confirmation, pipeline-only launch, and App shutdown only after the interactive setup successfully starts. The release page remains a fallback.
- Added 0.1.2 version/release documentation and the truthful bootstrap limit: 0.1.0/0.1.1 require one manual install of 0.1.2 because they do not contain the new updater code.
- Verification so far: TDD compile red; focused update/UX 8/8; related update/release 18/18 before the last two hardening tests; full 1066/1066 before the final path-binding test; Release 0 errors; integrity 385/18. Computer Use app approval timed out before launch, so no visual click or screenshot is claimed.
- Final local gates after cancellation and running-executable binding: focused 8/8; full Debug 1067/1067; Release build 0 errors with 18 NU1900 warnings from unavailable NuGet vulnerability metadata; source integrity 385 files/18 XAML; diff check clean.

## 2026-07-28 - 0.1.1 public release preparation

- Recorded the user's explicit request to publish 0.1.1, changed the App package version from 0.1.0 to 0.1.1, and added beginner-facing Chinese release notes.
- Rechecked the fixed GitHub repository, installer/release script contracts, D-first setup policy, tool paths, and the previously approved signer thumbprint.
- Verification so far: release/installer/system-footprint focused 183/183; full Debug 1060/1060; Release build 0 errors with 18 environmental NU1900 warnings; source integrity 383 files and 18/18 XAML; five release scripts parse; diff check clean.
- A restricted certificate query falsely appeared to show that the signer was missing. An approved host-session read-only query confirmed the original private key remains in CurrentUser My and public copies remain in CurrentUser TrustedPeople, TrustedPublisher, and Root. No certificate was created, imported, exported, or changed.
- Source commit `46374e9` was pushed and GitHub CI `30333084515` passed in 3m12s. A fresh 110-file payload and setup were signed and independently verified; the first two setup timestamp attempts failed transiently and Inno's third built-in retry succeeded.
- Draft creation then stopped safely before upload because Windows PowerShell promoted the expected missing-release stderr from `gh release view` to a terminating error. Added a regression contract and scoped native error handling; no GitHub Release was created.
- Release-script fix verification: red 1/4 before implementation; green 4/4 after implementation; full 1060/1060; Release build 0 errors; integrity 383/18; parser 0 errors; diff check clean.
- Fix commit `6053d7b` was pushed and GitHub CI `30333783060` passed in 2m59s.
- Final payload signing failed closed on transient DigiCert timestamp responses. An inferred DigiCert HTTPS variant was invalid, so it was abandoned. GlobalSign's current official R45 SignTool URL succeeded on an isolated copy with a valid TSA certificate; both scripts now add only that exact HTTP host/path. Timestamp policy contracts went red 2/8 then green 8/8.
- Timestamp fallback completion gates: full 1060/1060; Release build 0 errors; integrity 383/18; three release scripts parse; diff check clean.
- GlobalSign fallback commit `ba1df22` was pushed and GitHub CI `30334445147` passed in 3m8s.
- A fresh final candidate again signed App successfully but hit a transient timestamp failure on Worker. Added a shared three-attempt/ten-second SignTool retry while retaining mandatory signature/timestamp verification. Retry contract went red 1/4 then green 4/4.
- Bounded retry completion gates: full 1060/1060; Release 0 errors; integrity 383/18; parser 0; diff check clean.
- Bounded-retry commit `63fbb5e` was pushed and GitHub CI `30334942521` passed in 3m1s.
- The final 110-file payload completed bounded timestamp retries and passed independent same-signer/timestamp verification. The final setup independently returned `CanStageGitHubRelease=true`, is 14,823,256 bytes, and has SHA-256 `F581F89A93E36145E8C6952E5C7D4B0F9E32C7A6EDC4F76C13E9E1B2F6ADACBB`.
- Created the guarded draft, downloaded all four remote assets back, and matched every remote SHA-256 to its local release input. The downloaded installer and manifest passed the independent verifier again.
- Published `v0.1.1` at `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.1`. The unauthenticated public latest endpoint reports tag 0.1.1, non-draft, non-prerelease, and four assets.
- Installer execution has not occurred.

## 2026-07-28 - RogueCleaner-inspired system-footprint diagnosis

- Reviewed `aakk007/RogueCleaner` at commit `e498db3`, including README, MIT license, single-file scanner, cleanup/verification, recovery, and validation design. No upstream code, rule list, branding, or asset was copied.
- Compared its coverage with OMNIX and found OMNIX already had structured startup, service, scheduled-task, process, uninstall, rollback, and Agent layers. The useful missing evidence was UI/system integration surfaces.
- Added bounded read-only scans for context menus, Explorer namespace entries, browser Native Messaging hosts, and common file associations. Application ownership requires install-path or complete compact-name evidence; unknown entries remain unassigned.
- Added the beginner drawer line `还会出现在哪`, hidden technical evidence, an Observe-only Agent recommendation, stable AutomationId, static order contract, and an isolated real WPF smoke/screenshot.
- Verification: focused 6/6; full 1060/1060; Release 0 errors; integrity 383/18; smoke parser and GUI receipt passed. No registry/service/task/file mutation or real application operation ran.

## 2026-07-28 - Verified in-app updater and public 0.1.2 release completed

- Replaced the check-only update outcome with a beginner-visible `Download and install` path while retaining the release-page fallback.
- Added bounded D-first staging, exact length/SHA-256 verification, current-App/channel/package signer continuity, repeated prelaunch validation, cancellation cleanup, and a High-risk confirmed `SafetyOperationPipeline` launch with no silent arguments.
- Documented the unavoidable bootstrap boundary: installed 0.1.0/0.1.1 code cannot self-acquire this new behavior and must manually install 0.1.2 once.
- Source commit `4a3e30c3feacdeaf8fcc1df6543ba14f1ddc6125` was pushed; GitHub CI `30339140502` passed.
- The final payload and setup were signed by CurrentUser personal publisher thumbprint `5688958FEA0056861558E8DCF9D2381AF46074B2` with a valid GlobalSign timestamp. Installer verification passed with D-first default, visible directory selection, silent install disabled, and `CanStageGitHubRelease=true`.
- Created a four-asset GitHub draft, downloaded every asset back to a new directory, matched all SHA-256 values, and independently reverified the downloaded setup before publication.
- GitHub API transiently returned EOF during publication; the draft stayed intact. Published through the authenticated GitHub release page only after the existing explicit publication approval and verified the final public page plus public latest metadata.
- Public `v0.1.2`: `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.2`. Setup length 14,832,504 bytes; SHA-256 `C5D861160E3A38367B38F8FA9473FA6EB7D06485EF5202B1D4A5E7A3E76912C1`.
- No installer, product update, UAC flow, trust-store change, certificate export, antivirus change, or LocalMachine action ran.
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

## 2026-07-23 - Personal publisher created; signed candidate correctly failed on missing root trust

- Installed reviewed Inno Setup 6.7.3 to `D:\Development\Inno Setup 6`; its compiler Authenticode status is Valid.
- Added a guarded, idempotent personal signer initializer and contracts. After exact user consent, created non-exportable RSA code-signing certificate `5688958FEA0056861558E8DCF9D2381AF46074B2` for the interactive CurrentUser account.
- Independent store inspection found the private key only in CurrentUser My, public copies only in CurrentUser TrustedPeople/TrustedPublisher, and no matching entry in CurrentUser Root or inspected LocalMachine stores.
- Corrected the timestamp URL policy using DigiCert's official RFC3161 documentation: exact `http://timestamp.digicert.com` is narrowly allowed while other HTTP endpoints remain refused. Focused signer/installer/release contracts passed 9/9 and all three scripts parsed.
- Built `.artifacts/OMNIX-Entropy-test-20260723-225606`. Signing and timestamping App/worker succeeded, but Windows chain verification rejected the self-signed certificate because it is not in Trusted Root. The transform failed closed before manifest/ZIP completion; no installer was compiled or launched.

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


## Archived History

Entries before 2026-07-22 were moved verbatim to [worklog-archive-part1.md](archive/worklog-archive-part1.md), [worklog-archive-part2.md](archive/worklog-archive-part2.md), [worklog-archive-part3.md](archive/worklog-archive-part3.md).
