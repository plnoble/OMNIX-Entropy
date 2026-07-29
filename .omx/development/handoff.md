# Agent Handoff

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
