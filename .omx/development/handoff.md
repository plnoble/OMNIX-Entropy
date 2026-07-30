# Agent Handoff

## Latest Update - 2026-07-30 Public 0.1.4 released

- Current objective: preserve the completed 0.1.4 release evidence and obtain a user-run in-app update receipt from the currently installed version.
- What changed: reviewed multi-disk read-only scans, bounded homepage scrolling, and the beginner C-drive 80% comfort plan are publicly available in `v0.1.4`.
- What is verified: release target `68a7bd4ba9b73c9dd425da92baaf60c45450a81d` passed GitHub CI run `30517231962`; the fresh payload/setup used signer `5688958FEA0056861558E8DCF9D2381AF46074B2`; independent installer verification returned `CanStageGitHubRelease=true`; all four draft assets matched after download-back; the downloaded setup independently passed; public `releases/latest` reports stable `v0.1.4`, four assets, and target `68a7bd4`.
- Public artifact: `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.4`; setup length 14,842,384 bytes; SHA-256 `9B32AB55D61637A14275D741D6E120F5D07E3FEC960DFF5E890BA445BA7AA48D`.
- What is not verified: no installer was launched, installed, or uninstalled; no user confirmation or UAC flow ran; disposable-machine behavioral acceptance remains absent.
- Known risks: personal signing does not provide public SmartScreen reputation; GitHub and timestamp availability remain external; the first real installed update receipt still depends on the user's visible confirmation.
- Exact next recommended action: open the installed OMNIX update window, check for 0.1.4, use the visible download/install action, review the final confirmation, and report whether the installed version becomes 0.1.4. Do not automate or silently run the installer.

## Latest Update - 2026-07-30 Public 0.1.4 release in progress

- Current objective: publish a signed, independently verified `v0.1.4` from one pushed CI-passing revision, then record and push the completion evidence.
- What changed: user authorization expanded from local commit to public push/release; release preflight records now bind the future package, manifest, tag, and Release target to the same preparation revision.
- What is verified: reviewed 0.1.4 source gates remain green; host preflight found SignTool, Inno Setup, the existing eligible signer `5688958FEA0056861558E8DCF9D2381AF46074B2`, and authenticated `plnoble` GitHub CLI access.
- What is not verified: preparation commit/push/CI, signed payload, setup, draft assets, download-back hashes, public Release, and latest endpoint are all still pending.
- Known risks: timestamp and GitHub availability are external; personal signing provides no public SmartScreen reputation; no installer may be run as part of release automation.
- Exact next recommended action: commit these preparation records, push `main`, wait for CI, then use only the guarded repository scripts to create and verify 0.1.4 artifacts.

## Latest Update - 2026-07-30 Local 0.1.4 review and source commit

- Current objective: preserve the reviewed 0.1.4 source state and obtain a separate user decision before any push, signing, installer, tag, or GitHub Release work.
- What changed: ready-fixed-drive selector and selected-drive read-only scan scope; system-only Windows diagnostics; bounded homepage result scrolling; explicit 80% disk comfort target and Agent-led preview; complete stale-state reset; system-drive-only history labelling/navigation; source/release version 0.1.4; process-scoped multi-disk GUI smoke.
- What is verified: focused review 26/26; final full Debug 1085/1085; Release 0 warnings/errors; source integrity 390 files, InvalidUtf8 0, ReplacementFiles 0, XAML 18/18; Debug `ProductVersion=0.1.4` and `FileVersion=0.1.4.0`; both PowerShell smoke parsers pass; real home smoke proves scroll movement, 80% target, selected low-risk preview, visible confirmation entry, and `noOperationExecuted=true`; real selector smoke lists C/D, selects D, clears current results, labels retained history as system-drive history, and captures `.omx/qa-multidisk-selector.png`; diff check has no errors.
- What is not verified: no real full D-drive scan was allowed to traverse user data; no cleanup confirmation or operation executed; no installer was built, signed, launched, installed, or uninstalled; no push, tag, CI, GitHub Release, or installed-app update occurred.
- Known risks: data-drive scans can take time depending on user data; only system-drive history is retained in 0.1.4; per-drive trends need a designed schema migration; public/installed 0.1.3 remains the latest available version.
- Exact next recommended action: if the user wants 0.1.4 distributed, review the clean local commit, push it, wait for CI, then run the existing signed personal installer/release gates. Do not call 0.1.4 available in the updater before the public Release and remote asset verification succeed.

## Latest Update - 2026-07-29 Beginner C-drive health plan

- Current objective: preserve the completed working-tree answer to “how do I make C healthier?” without weakening local execution safety.
- What changed: a read-only 80% comfort-target presenter; first-view target/safe-contribution/remaining-gap band; detailed three-step disk plan; strict counting of only executable, reversible low-risk cleanup evidence; automatic preferred-card selection for preview; one scroll surface for the complete right-hand recommendation workflow.
- What is verified: focused 13/13; full Debug 1084/1084; Release 0 warnings/errors; source integrity 390 files and 18/18 XAML; diff check pass. Real WPF evidence showed a 19.1 GB target, 4.0 KB safe contribution, 19.1 GB gap, selected low-risk preview, visible “查看并确认移动到隔离区” entry, both existing home scroll surfaces, and `noOperationExecuted=true`. Screenshots: `.omx/qa-home-agent-next-action.png`, `.omx/qa-drive-health-plan.png`.
- What is not verified: no confirmation was accepted and no cleanup was executed; no installed build/package contains this change; no commit, push, version bump, or release was performed.
- Known risks: 80% is a transparent product comfort threshold rather than a Windows health guarantee; scanning and recommendation quality still depend on available evidence; installed 0.1.3 remains unchanged.
- Exact next recommended action: review and commit the combined working-tree slices, then decide separately whether to publish a new release for in-app update.

## Latest Update - 2026-07-29 Scrollable homepage results

- Current objective: preserve the completed working-tree fix for clipped/non-scrollable homepage key findings and health history.
- What changed: both homepage cards now use bounded grid rows; key findings retain native `ListBox` scrolling; the complete history summary, daily rows, and evidence button share one vertical `ScrollViewer` with a non-nested `ItemsControl`; the isolated WPF smoke creates overflow and proves both scroll positions move.
- What is verified: focused 10/10; full Debug 1080/1080; Release 0 warnings/errors; source integrity 388 files and 18/18 XAML; parser and diff check pass. Real UIAutomation moved key findings 0% to 25% and history 0% to 100%, kept internal next-action navigation working, executed no operation, and captured `.omx/qa-home-agent-next-action.png`.
- What is not verified: no installed build or package contains this change; no commit, push, version bump, or release was performed.
- Known risks: the current worktree also contains the preceding completed multi-disk slice; installed 0.1.3 remains unchanged; very small unsupported windows can require more scrolling.
- Exact next recommended action: review and commit the combined working-tree changes, then decide separately whether to publish a new release for in-app update.

## Latest Update - 2026-07-29 Multi-disk read-only health scan

- Current objective: preserve the completed working-tree implementation that lets beginners choose C/D/other ready fixed local disks for read-only health analysis without applying Windows-system cleanup rules to data drives.
- What changed: visible non-editable fixed-drive selector with free-space/accessibility labels; Windows drive first; dynamic selected-drive summaries; stale-result invalidation; scan-time selector lock; system-only big-rock/unexpected-root policy; whole selected data-drive large/duplicate scope; neutral data-drive folder conclusions; reusable WPF selector smoke.
- What is verified: focused 200/200; full Debug 1079/1079; Release 0 warnings/errors; source integrity 388 files and 18/18 XAML; smoke parser and diff check pass. Real WPF UIAutomation listed C/D, selected D, exposed the selected name and pending conclusion, and saved `.omx/qa-multidisk-selector.png`.
- What is not verified: no real full C/D scan was started in this slice; no installed build, package, or updater acceptance was run; the worktree changes are not committed, pushed, versioned, or released.
- Known risks: scanning a large data disk can take time despite cancellation and crawler bounds; removable/network disks are intentionally excluded; the installed 0.1.3 does not contain this slice.
- Exact next recommended action: review and commit the working-tree slice, then obtain a separate decision for a new release. For runtime acceptance, select a small fixed data drive or allow a cancellable D scan and confirm the summary remains drive-specific and read-only.

## Latest Update - 2026-07-29 Public 0.1.3 released

- Current objective: use the correctly installed 0.1.2 for the first true 0.1.3 in-app update and preserve its runtime receipt.
- What changed on the machine: the user reinstalled 0.1.2 into `D:\Software\OMNIX-Entropy\Install`. Read-only host verification found the correct exe, valid expected signer and GlobalSign timestamp, fixed D drive, non-reparse product/install directories, matching uninstall-registry location, and no doubled executable.
- What changed in source: `AppendDefaultDirName=no`; typed doubled-layout refusal with exact reinstall guidance and no network request; release-page launch fallback contract; archive EOF whitespace repair. The strict managed-layout guard remains unchanged.
- What is verified: focused 16/16; full Debug 1068/1068; Release build 0 warnings/errors; source integrity 385 files/18 XAML; source `d052ae1` pushed; CI `30424760666` passed on a bounded clean retry; signed payload/setup passed independent verification; four draft assets matched after download-back; downloaded setup returned `CanStageGitHubRelease=true`; public latest reports non-draft/non-prerelease `v0.1.3` with four assets.
- Public artifact: `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.3`; setup length 14,833,040 bytes; SHA-256 `33C332A658B6B5CF08304AE67A0E2A7A5AAE3CC23A357E5407913E305F05C73A`; signer `5688958FEA0056861558E8DCF9D2381AF46074B2`.
- What is not verified: no installer or product update ran; the first true in-app download/verify/confirm/launch from installed 0.1.2 remains a user-run acceptance step.
- Known risks: GitHub/timestamp availability remains external; personal signing does not create public SmartScreen reputation; the first CI attempt's hosted testhost aborted after 429 passing tests, although the clean retry completed.
- Exact next recommended action: ask the user to open the installed 0.1.2 update window, check for 0.1.3, use the visible download/install action, and report the outcome. Preserve final confirmation and resulting installed path/version evidence; do not automate the installer.

## Latest Update - 2026-07-28 Public 0.1.2 released

- Current objective: preserve the completed verified in-app update chain and guide the one-time bootstrap from installed 0.1.0/0.1.1 to 0.1.2.
- What changed: user-visible `DownloadAndInstallUpdateButton`; bounded D-first package download; length/SHA-256/current-App signer/channel signer checks; repeated prelaunch verification; explicit High-risk confirmation through `SafetyOperationPipeline`; interactive zero-argument installer launch; cancellation and failure cleanup; 0.1.2 release notes and version.
- What is verified: focused 8/8; full Debug 1067/1067; Release 0 errors with 18 environmental NU1900 warnings; integrity 385 files/18 XAML; source commit `4a3e30c3feacdeaf8fcc1df6543ba14f1ddc6125`; CI `30339140502` passed; signed payload/setup same-signer verification passed; all four GitHub assets were downloaded back and matched; the downloaded setup independently returned `CanStageGitHubRelease=true`; public latest metadata reports `v0.1.2`, non-draft, non-prerelease, four assets.
- Public artifact: `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.2`; setup length 14,832,504 bytes; SHA-256 `C5D861160E3A38367B38F8FA9473FA6EB7D06485EF5202B1D4A5E7A3E76912C1`; signer `5688958FEA0056861558E8DCF9D2381AF46074B2`.
- What is not verified: no installer was launched, installed, or uninstalled on the development machine; Computer Use approval timed out before current UI launch; the first true self-update can only be tested by a version newer than 0.1.2 after 0.1.2 is installed.
- Known risks: 0.1.0/0.1.1 cannot acquire the new updater code and require one manual GitHub install; GitHub/timestamp availability is external; SmartScreen public reputation is not provided by the personal certificate; CurrentUser publisher trust remains persistent.
- Exact next recommended action: manually install 0.1.2 once from the public release page, leaving the directory visible and user-confirmed. For the next release, test the full in-app update path before expanding unrelated product scope.

## Latest Update - 2026-07-28 Public 0.1.1 released

- Current objective: preserve the published 0.1.1 release evidence and resume product work from the read-only system-footprint slice.
- What changed: version/release notes, expected-missing-release handling, exact GlobalSign timestamp fallback, bounded SignTool retries, final signed payload/setup, four release assets, public `v0.1.1`, and completion records.
- What is verified: release source `63fbb5e868bbf2231ac8236b2b5d577e816ddfce`; CI `30334942521` passed; full local suite 1060/1060; Release 0 errors; integrity 383/18; final installer independently returned `CanStageGitHubRelease=true`; downloaded remote assets matched local hashes; public latest metadata reports 0.1.1 and four assets.
- Public artifact: `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.1`; setup SHA-256 `F581F89A93E36145E8C6952E5C7D4B0F9E32C7A6EDC4F76C13E9E1B2F6ADACBB`; signer `5688958FEA0056861558E8DCF9D2381AF46074B2`.
- What is not verified: setup was not launched, installed, or uninstalled; the ten-case disposable Windows acceptance receipt is absent; SmartScreen public reputation is not provided by the personal certificate.
- Known risks: timestamp and GitHub availability are external; CurrentUser trust remains persistent; future binaries signed by the same private key are trusted for this user; local builds can emit environmental NU1900 warnings.
- Exact next recommended action: carry `SystemFootprints` into the official-uninstall pre/post evidence and explain remaining integrations read-only. Do not add removal until exact targets, rollback evidence, explicit confirmation, and `OperationPipeline` coverage exist.

## Latest Update - 2026-07-28 RogueCleaner-inspired system-footprint diagnosis

- Current objective: preserve the completed read-only system-footprint slice and next extend official-uninstall post-scan evidence without adding removal authority.
- What changed: bounded scans for context menus, Explorer namespace entries, browser Native Messaging hosts, and common file associations; conservative app correlation; profile/enrichment propagation; path-free drawer summary; Observe-only Agent advice; hidden technical details; isolated GUI smoke.
- What is verified: focused 6/6; full 1060/1060; Release 0 errors; integrity 383 files and 18/18 XAML; PowerShell parser pass; inspected screenshot `.omx/qa-app-system-footprint.png`.
- What is not verified: no real registry entry was removed or disabled; no footprint cleanup plan/handler exists; protected/unreadable registry views can make a scan partial; post-uninstall scanning does not yet include the new footprint type.
- Known risks: install-path/full-name matching is deliberately conservative and may miss integrations; future vendor rules must never become execution authority by themselves; NU1900 warnings remain environmental.
- Exact next recommended action: carry `SystemFootprints` into the official-uninstall evidence snapshot and fresh post-scan comparison, then present remaining entry counts to the Agent. Keep it read-only until exact targets, rollback evidence, confirmation, and `OperationPipeline` coverage exist.

## Latest Update - 2026-07-23 First valid personal installer completed

- Current objective: preserve the verified personal signing/installer chain and decide separately whether to run disposable acceptance or stage a GitHub draft Release.
- What changed: guarded two-step personal signer initialization, exact official DigiCert timestamp exception, pinned Inno Simplified Chinese resource/provenance, and completed development records.
- What is verified: certificate stores are correctly scoped; signed 110-file candidate passed; installer 0.1.0 passed independent verification; focused 10/10; full 1054/1054; Release 0 errors; integrity 380/18; parser and public-candidate audits pass; source commit `4aba92d` is pushed and CI `30019958502` passed.
- Local artifact: `.artifacts/OMNIX-Entropy-installer-v0.1.0-roottrusted-2/OMNIX-Entropy-0.1.0-win-x64-setup.exe`, SHA-256 `5680C3847F23291784BB38FB1D01FACAFC6013DC47F06B611C170BCDC63955BE`.
- What is not verified: setup was not launched/installed/uninstalled; the ten-case disposable Windows receipt is absent; no GitHub draft or public Release was created.
- Known risks: CurrentUser trust is persistent; private key is non-exportable; SmartScreen public reputation is not provided by personal trust; NU1900 warnings reflect unavailable vulnerability metadata.
- Exact next recommended action: after source push/CI, obtain a separate user decision for either disposable-machine acceptance or local-only GitHub draft staging. Do not run or publish setup implicitly.

## Latest Update - 2026-07-23 D-first installer foundation completed

- Current objective: provide a beginner-visible, D-first Windows setup and bind GitHub updates to a verified installer rather than a portable ZIP.
- What changed: `installer/OMNIX-Entropy.iss`, builder/verifier scripts, setup-based release manifest contract, focused tests, and Chinese operator documentation.
- What is verified: focused 19/19; full 1051/1051; Release 0 errors; integrity 379/18; script parsers valid; fixed setup asset and same-signer checks are covered.
- What is not verified: no real setup was compiled, signed, launched, installed, uninstalled, or uploaded.
- Known risks/blockers: Inno compiler is absent; SignTool exists; eligible CurrentUser RSA code-signing certificates are 0. Local builds emitted NU1900 warnings because the sandbox could not reach the NuGet vulnerability index, but compilation/tests completed.
- Exact next recommended action: obtain explicit approval before installing Inno or creating/selecting any personal certificate. In parallel, application-side download and verification may be implemented, but installer execution must remain disabled until a real verified setup exists.

## Latest Update - 2026-07-22 Public CI remediation completed

- Current objective: make `plnoble/OMNIX-Entropy` reproducible from tracked files and obtain a passing GitHub Actions run after the successful initial push.
- What changed: top-level `.omx` smoke/helper scripts are now public test inputs; LF checkout policy, Actions v5 pins, Release-before-Debug order, and disabled xUnit collection parallelism are implemented with repository contracts.
- What is verified: smoke-script privacy scan is clean except neutral `Fixture` paths; focused 4/4; working-tree and tracked-only archive full suites each 1048/1048; both Release builds succeeded; both integrity runs reported 378 files and 18/18 XAML; GitHub Actions run `29933681994` passed 1048/1048 and the same integrity gate.
- What is not verified: no GitHub installer or Release asset exists yet; package download, same-signer installer verification, D-first replacement, and rollback are not implemented.
- Known risk: local NuGet vulnerability-index access emitted `NU1900` warnings because the sandbox could not reach nuget.org; compilation and tests still completed.
- Exact next recommended action: implement a directory-selectable installer that defaults to `D:\Software\OMNIX-Entropy\Install`, then add explicit user-started download, same-signer/hash verification, replacement, and rollback before publishing a draft GitHub Release.

## Latest handoff - 2026-07-22 GitHub personal release foundation

- Current objective: publish the audited source baseline to `https://github.com/plnoble/OMNIX-Entropy`, then add a directory-selectable D-first installer and verified update application.
- What changed: origin connected; public ignore rules fixed; pinned read-only CI added; local draft-only same-signer release staging added; root README and personal update guide added; fixed-repository release manifest policy/client and compact user-triggered update window added; App version set to 0.1.0.
- What is verified: focused 13/13; full Debug 1048/1048; Release build 0 warnings/errors; source integrity 377 and XAML 18/18; PowerShell parser pass; 424 public candidates/about 6 MB; no binaries, signing secrets, or real username in candidates.
- What is not verified: no real GitHub Release exists; no personal certificate exists; no package download or install path exists; no D-first installer has been built; Computer Use timed out launching the app, so the update dialog has no real screenshot.
- Known risks/blockers: do not run the full fake-worker lifecycle tests in Release; do not parallelize dotnet commands sharing output; do not use Velopack's C-drive LocalAppData install layout; do not put a PFX/private key in GitHub or remove valid-same-signer authorization.
- Exact next recommended action: complete the initial audited commit/push, verify GitHub CI, then choose/install a reviewed directory-selectable installer tool and build a same-signer D-first Setup/update/rollback flow.

## Current Objective

OMNIX-Entropy 0.1.2 is publicly released and the verified in-app update chain is complete in source. The remaining step is a one-time human bootstrap: machines running installed 0.1.0/0.1.1 predate the updater code, so they must install 0.1.2 once from the public release page. Only after that bootstrap can a future release exercise the real download/verify/confirm/launch path end to end.

The 2026-07-22 signing blocker is resolved and must not be reintroduced into this section: a personal CurrentUser RSA signer exists (`5688958FEA0056861558E8DCF9D2381AF46074B2`), and two public releases have shipped through it. The still-open external gate is narrower: no installer has ever been launched, installed, or uninstalled, and the ten-case disposable Windows behavioral receipt does not exist.

## What Changed

- Independent review of released `main` (`e4f0cf0`): every recorded gate and public-release claim was re-derived from the machine and from GitHub rather than trusted from the records. All matched.
- `UpdateWindow.OpenReleasePage_Click` no longer calls `Process.Start` unguarded. `Css.App` has no global `DispatcherUnhandledException` handler, so a ShellExecute failure there could terminate the app from the update window; it now reports a path-free beginner conclusion like every other launch site.
- `PersonalReleasePackageDownloader.DownloadAndVerifyAsync` no longer repeats `CleanupStaging` in five catch blocks. The `finally` guard was already the effective authority for every exit; it is now the only one, and says so.
- Development records were split by date. Live files keep entries from 2026-07-22 onward plus their reusable templates and gate checklists; older entries moved verbatim into `.omx/development/archive/`.
- `handoff.md`'s `What Changed` / `Verified` / `Not Verified` sections had accumulated into a ~250-line rolling dump spanning many slices. That body is archived; these sections now describe only current state.

## What Is Verified

- Source integrity: 385 files, InvalidUtf8 0, ReplacementFiles 0, 18/18 XAML — unchanged by this slice.
- Release build: 0 errors and 0 warnings. The 18 NU1900 warnings in earlier records were environmental; the NuGet vulnerability index was reachable during this run.
- Full Debug suite: 1067/1067, failures 0. Focused update contracts 11/11 after the code fix.
- Record split is provably lossless: all 1,724 entry blocks across the seven records were compared against their `HEAD` versions and every block is present verbatim in either the live file or an archive file.
- Public 0.1.2 cross-check against GitHub, not against the records: tag `v0.1.2` is non-draft and non-prerelease with four assets; setup is 14,832,504 bytes; GitHub's own digest `c5d8611…12c1` matches both the records and the published `omnix-release.json`; the manifest's `InstallerManifestSHA256` matches GitHub's digest for `installer-manifest.json`; `CommitSHA` is `4a3e30c`, whose CI run `30339140502` passed. CI also passed on `e4f0cf0` (run `30340365520`).

## What Is Not Verified

- The installed 0.1.2 has not been launched by an agent, and no product process was started during the review. Installation itself was performed by the user, not by an agent.
- No current screenshot exists. Computer Use approval timed out before app launch in the previous slice and was not retried here; visual evidence remains Warn.
- The first true self-update is still untested. The corrected current installation now satisfies the observed layout, drive, reparse, version, and signer prerequisites.
- The ten-case disposable Windows behavioral receipt is still absent.

## Bootstrap Install State - 2026-07-29

The user completed the manual 0.1.2 bootstrap. A read-only uninstall-registry query confirms `OMNIX-Entropy 版本 0.1.2`, publisher `plnoble`, InstallDate `20260728`.

The first install landed at `D:\Software\OMNIX-Entropy\Install\Install\Css.App.exe`, one level deeper than the managed layout. `WindowsPersonalUpdatePathPolicy.ResolveProductRoot` correctly refused it, while the generic downloader message failed to explain recovery.

Cause: `installer/OMNIX-Entropy.iss` leaves `AppendDefaultDirName` at its default of yes, so Inno's Browse dialog appends the last component of `DefaultDirName` to the selected folder. Confirming the pre-filled default through Browse produces the doubled path. The installer default itself is correct; only the Browse route produces this.

The user then reinstalled 0.1.2 into `D:\Software\OMNIX-Entropy\Install`. Host verification confirms the current executable, registry location, signer/timestamp, fixed drive, and non-reparse chain are correct; the doubled executable is absent. Machine remediation is complete. Source prevention and recovery copy are implemented in the current active slice and await broader gates/release.

## Known Risks Or Blockers

- Do not turn natural-language questions or automatic read-only evidence loading into consent for system modification.
- Do not trigger a full C-drive scan for unrelated hardware/settings questions; only the explicit system-diagnosis/C-drive intents may use the shared full health gate.
- Do not cache failed/cancelled loads or suppress post-operation Timeline refresh.
- Do not default package trust to true or add unsigned mutation modes.
- Do not use PowerShell UIAutomation/SendKeys to bypass Computer Use failure.
- Do not treat Application-log correlation as a root cause or retain formatted messages/property values outside the Win32 adapter.
- Do not present one short runtime sample as a cause, trend, or reason to end a process; no trustworthy process hint must remain Unavailable.
- Do not copy raw exception, operation, policy, or validation error text into beginner-visible status/message controls; classify by verified state and next recovery action.
- Do not launch `Process.Start` from a WPF click handler without a catch; `Css.App` has no global `DispatcherUnhandledException` handler, so an unguarded launch failure terminates the app.
- Keep the live development records inside the tooling read limit. When a live record approaches roughly 200 KB, archive its older dated entries under `.omx/development/archive/` instead of letting the file grow; a record an agent cannot open is a record that will not be read.
- External and unchanged: GitHub and RFC3161 timestamp availability, SmartScreen public reputation (not provided by a personal certificate), and persistent CurrentUser publisher trust, which extends to any future binary signed by the same private key.

## Exact Next Recommended Action

The bootstrap install is done, so the next work is closing the layout defect it exposed. Two source changes belong together, before any release newer than 0.1.2:

1. Make the doubled layout unproducible. Set `AppendDefaultDirName=no` in `installer/OMNIX-Entropy.iss`, or otherwise stop the directory page from appending `Install` to a selected folder, and add a focused contract so the rule cannot regress.
2. Make the refusal legible. `ResolveProductRoot`'s layout rejection currently reaches the user as `更新包没有准备完成，也没有启动安装程序。`, which describes neither the cause nor the recovery. Give it a distinct beginner-visible conclusion that names the install location and says the app must be reinstalled into the managed directory.

Do not relax the layout check itself; it is a deliberate guard and the installer is what should guarantee the layout.

The user-run reinstall into `D:\Software\OMNIX-Entropy\Install` is complete. Do not launch, install, or uninstall another setup on the user's behalf.

Only after that can a release newer than 0.1.2 prove the in-app download/verify/confirm/launch path end to end. Prefer also consolidating the release preflight into one fail-closed receipt, as `reflections.md` recommends.

## Archived History

Entries before 2026-07-22 were moved verbatim to [handoff-archive.md](archive/handoff-archive.md).
