# Current Development State

## Completed Slice - 2026-07-30 Public 0.1.4 release

- Objective: push the reviewed 0.1.4 source, obtain passing GitHub CI, build and independently verify the existing personal-signer Windows installer, stage and download-back verify the four fixed GitHub assets, publish `v0.1.4`, and confirm the public latest endpoint.
- Authorization: the user explicitly requested “推送发布”, authorizing push, tag/Release creation, local signing with the existing non-exportable personal publisher key, and public GitHub publication of 0.1.4. This does not authorize installer execution, certificate creation/export, trust-store mutation, antivirus changes, or `LocalMachine` changes.
- Dependencies: clean 0.1.4 source, GitHub CLI host login, CI, `D:\Development\Inno Setup 6\ISCC.exe`, Windows SDK SignTool, existing signer `5688958FEA0056861558E8DCF9D2381AF46074B2`, approved RFC3161 timestamp service, and guarded release scripts.
- Risks: publishing an artifact from a revision different from the Release target, accepting a stale or unsigned payload, exposing local secrets, calling a draft public before download-back verification, or implying installation without running the visible installer.
- Impact scope: release records, one release-preparation commit, push, CI, ignored local artifacts, Authenticode signing/timestamping, tag `v0.1.4`, and four public GitHub Release assets. No product operation, installer launch, install/uninstall, or machine trust change.
- Acceptance: release-preparation revision pushed; CI passes; portable and signed candidates match the committed 0.1.4 revision; App, worker, setup, and uninstaller share the expected signer and timestamps; installer verifier returns `CanStageGitHubRelease=true`; draft assets re-download and hash-match; downloaded setup independently verifies; public latest is non-draft/non-prerelease `v0.1.4`; final records are committed and pushed.
- Status: complete. Release-preparation commit `68a7bd4ba9b73c9dd425da92baaf60c45450a81d` is pushed and GitHub CI run `30517231962` passed Release build, all tests, and source integrity. A fresh payload from that revision was signed with `5688958FEA0056861558E8DCF9D2381AF46074B2`; the App, worker, uninstaller, and setup passed the existing timestamp/same-signer checks. The setup independently returned `CanStageGitHubRelease=true`, and all four draft assets were downloaded back and matched the local SHA-256 values byte for byte. The downloaded setup independently passed the same installer verifier. Public `releases/latest` now reports non-draft/non-prerelease `v0.1.4`, target `68a7bd4`, and four uploaded assets.
- Public artifact: `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.4`; setup length 14,842,384 bytes; SHA-256 `9B32AB55D61637A14275D741D6E120F5D07E3FEC960DFF5E890BA445BA7AA48D`; signer `5688958FEA0056861558E8DCF9D2381AF46074B2`.
- Exact next action: the user may use the installed app's update window to check for 0.1.4, review its visible download/install confirmation, and report the resulting installed version. No installer or product update was run during publication.

## Completed Slice - 2026-07-30 Review, commit, and version 0.1.4

- Objective: review the combined multi-disk, homepage-scrolling, and beginner disk-health-plan working tree; fix release-blocking correctness or beginner-UX defects; set the source version to 0.1.4; verify; and create one local commit.
- Dependencies: the completed 2026-07-29 slices, selected-drive invalidation, recommendation preview state, system-drive health history, release version metadata, and the repository quality gates.
- Risks: committing stale cross-drive evidence, mixing data-drive scans into system-drive history, overwriting the selected recommendation explanation, suggesting that D-drive data should be moved to D, or implying that a version bump is already a published update.
- Impact scope: review fixes, focused regression contracts, source version and release notes, development records, and a local commit. No cleanup execution, installer launch, signing, trust-store change, push, tag, GitHub Release, or installed-product mutation is authorized.
- Acceptance: switching or starting a drive scan clears every prior health-plan detail; a selected safe recommendation keeps its specific explanation; data-drive advice is destination-neutral; system health history remains system-drive scoped; version reports 0.1.4; focused/full/build/integrity/diff gates pass; the committed worktree is clean.
- Status: complete and verified for the requested local 0.1.4 source commit. Review fixed stale plan steps/safety after drive changes and incomplete scans, preserved selected-card-specific explanation, made data-drive destinations neutral, kept health history system-drive scoped, labelled that scope in the UI, and made the multi-disk smoke process-scoped instead of scanning the whole desktop. Focused review contracts pass 26/26; final full Debug passes 1085/1085; Release builds with 0 warnings/errors; source integrity passes 390 files and 18/18 XAML; both real GUI smokes pass with no operation executed; product/file versions report 0.1.4; diff check has no errors.
- Exact next action: decide separately whether to push this local commit and prepare a signed 0.1.4 GitHub Release. The installed/public 0.1.3 remains unchanged until that separate release work is completed.

## Completed Slice - 2026-07-29 Beginner C-drive health plan

- Objective: answer “how do I make C healthier?” in the first visible homepage area with a numeric target, an honest safe-cleanup contribution, the remaining gap, and one Agent-led next step.
- Dependencies: `DriveScanResult`, low-risk cleanup recommendations, personal-storage candidates, homepage result layout, C-drive recommendation selection, and the isolated home/C-drive GUI smokes.
- Risks: implying that a few hundred MB of cleanup will solve a multi-GB shortage, turning an arbitrary score into medical-style certainty, auto-executing a recommendation, hiding data-drive neutrality, or adding another dense technical panel.
- Impact scope: one read-only plan presenter, a concise homepage action band, automatic selection of the safest recommendation for preview, C-drive beginner copy, focused contracts, real screenshot evidence, and development records. No cleanup execution, quarantine, migration, registry, service, installer, updater, signing, or trust behavior changes.
- Acceptance: for a drive above 80% used, the UI states how much space is needed to return to the product's 80% comfort target, how much is already supported by low-risk reversible evidence, and how much remains; it gives three plain-language steps and one navigation/preview action. No actionable evidence means “do not delete yet.” The first safe card may be selected for explanation but never executed without the existing confirmation pipeline.
- Status: complete in the working tree. The first homepage area now states the 80% comfort target, safe reversible contribution, remaining gap, and one Agent-led next step. The C-drive page shows the three-step plan, preselects the first executable low-risk card for preview, and scrolls the complete recommendation surface to the still-unexecuted confirmation entry. Focused contracts pass 13/13; full Debug passes 1084/1084; Release builds with 0 warnings/errors; source integrity passes 390 files and 18/18 XAML; diff check passes with only existing line-ending notices. Real WPF evidence reported `19.1 GB` target, `4.0 KB` safe contribution, `19.1 GB` remaining gap, a visible low-risk preview button, and `noOperationExecuted=true`.
- Exact next action: review and commit the current multi-disk, scrolling, and beginner-health-plan working tree, then obtain a separate decision before versioning or releasing. Installed 0.1.3 does not contain these changes.

## Completed Slice - 2026-07-29 Scrollable home results

- Objective: make the homepage health-history and key-findings regions actually mouse-wheel/scrollbar scrollable instead of letting unconstrained lists grow behind clipped cards.
- Dependencies: the homepage two-column body, `HealthDigestHistoryScrollViewer`, `HealthDigestHistoryItemsControl`, `KeyFindingsListBox`, their stable AutomationIds, and the isolated home WPF smoke.
- Risks: adding a decorative scrollbar without constraining list height, hiding the Agent conclusion, creating nested wheel traps, or regressing button interaction inside finding rows.
- Impact scope: homepage XAML layout, focused static contracts, one isolated WPF scrolling smoke, screenshot, and development records. No scan, cleanup, operation-pipeline, persistence, update, installer, or system-mutation behavior changes.
- Acceptance: the key-finding list and complete history region live in bounded star rows with explicit vertical `Auto` scrolling; the Agent conclusion remains visible; finding buttons remain clickable; a real constrained-window smoke proves both regions expose vertical scroll support and change position.
- Status: complete in the working tree. Focused contracts pass 10/10; full Debug passes 1080/1080; Release builds with 0 warnings/errors; source integrity passes 388 files and 18/18 XAML; diff check passes with only existing line-ending notices. Real WPF UIAutomation moved key findings from 0% to 25% and history from 0% to 100%; screenshot confirms both scrollbars and the history evidence button remain reachable.
- Exact next action: review and commit the current multi-disk plus homepage-scroll working tree, then obtain a separate decision before versioning or releasing. Installed 0.1.3 does not contain these changes.

## Completed Slice - 2026-07-29 Multi-disk read-only health scan

- Objective: let a beginner choose any ready local fixed disk for read-only space analysis while keeping the system disk as the default and preserving deeper Windows-specific conclusions only for the real system disk.
- Dependencies: the hidden `DriveRootComboBox`, `DiskScanner`, `CategoryClassifier`, `BigRocksProbe`, personal-storage analysis, scan snapshots, root-cause/health presenters, and the existing C-drive GUI smoke fixture.
- Risks: exposing arbitrary path input, listing removable/network/unready drives, showing C-drive system evidence while scanning D/E, flagging every data-drive folder as an unexpected system root, leaving stale C-drive results visible after a drive switch, or letting Agent reuse a completed scan from the previously selected drive.
- Impact scope: disk-target presentation and selection, scan-scope policy, dynamic beginner copy, selected-drive personal-file roots, focused contracts, GUI evidence, and development records. The scan remains read-only; no cleanup, quarantine, migration, registry, service, installer, certificate, trust, or antivirus authority changes.
- Acceptance: the selector lists only ready fixed local disks, orders the Windows system disk first, shows understandable labels and free space, does not accept typed paths, invalidates stale results when changed, and cannot change during an active scan. D/E scans omit system-only big-rock evidence and unexpected-system-root claims, use the selected drive for large/duplicate candidates, persist snapshots by selected root, and label all summaries with the selected drive. Existing system-drive behavior and fixture smokes remain compatible; focused/full/build/integrity/diff and a real WPF selector screenshot pass.
- Status: completed in the working tree. The visible non-editable fixed-drive selector, dynamic summaries, stale-result invalidation, system/data scan policy, selected-drive large/duplicate scope, and data-drive no-cleanup-authority rules are implemented. Focused tests pass 200/200; full Debug passes 1079/1079; Release builds with 0 warnings/errors; source integrity reports 388 files, zero invalid UTF-8/replacement files, and 18/18 valid XAML. The real WPF smoke listed system C and fixed D, selected D, exposed the selected value to UIAutomation, showed the pending D-drive conclusion, and saved `.omx/qa-multidisk-selector.png`.
- Exact next action: review/commit this source slice and decide separately whether to publish it in a new version. The installed 0.1.3 does not contain these uncommitted changes.

## Completed Slice - 2026-07-29 Managed install-layout closure and public 0.1.3

- Objective: close the installer/updater layout mismatch exposed by the 0.1.2 bootstrap, preserve the strict managed-layout guard, and prepare a trustworthy 0.1.3 end-to-end in-app update test.
- Dependencies: `installer/OMNIX-Entropy.iss`, `WindowsPersonalUpdatePathPolicy`, `PersonalReleasePackageDownloader`, beginner-facing update-window status, focused installer/update contracts, the unpushed `fe2e012` review commit, and the existing signed personal release chain.
- Risks: relaxing the managed-layout guard, matching raw exception text, letting Browse recreate `Install\\Install`, hiding the recovery action behind a generic failure, publishing stale records that still describe the corrected installation as broken, or pushing an archive split that fails `git diff --check`.
- Impact scope: one Inno setup directive, typed layout-refusal classification and copy, focused tests, release records, archive formatting, and a future 0.1.3 package. No installer execution, product update, trust-store change, certificate export, antivirus change, or system mutation is authorized.
- Acceptance: Browse cannot append a second `Install`; the doubled layout remains refused by the real path policy; the downloader returns a distinct path-free beginner conclusion with the exact managed destination and reinstall recovery; the release-page launch fallback is contract-covered; current records reflect the corrected 0.1.2 installation; `git diff --check`, focused/full/build/integrity, CI, signed package, remote re-download, and public latest gates pass before 0.1.3 is called complete.
- Current machine evidence: the user reinstalled 0.1.2. Read-only host verification found `D:\\Software\\OMNIX-Entropy\\Install\\Css.App.exe`, version `0.1.2+4a3e30c3feacdeaf8fcc1df6543ba14f1ddc6125`, valid signer `5688958FEA0056861558E8DCF9D2381AF46074B2`, GlobalSign timestamp, fixed D drive, non-reparse product/install directories, uninstall-registry `InstallLocation=D:\\Software\\OMNIX-Entropy\\Install\\`, and no `Install\\Install\\Css.App.exe`.
- Status: completed and publicly released as 0.1.3. Source `d052ae15be48cdf5b304d9c0d0fa4cc9a5b922fe` is pushed; GitHub CI `30424760666` passed on its bounded clean retry after the first hosted test process aborted without an assertion failure. Focused contracts pass 16/16; full Debug passes 1068/1068; Release builds with 0 warnings and 0 errors; source integrity reports 385 files, zero invalid UTF-8/replacement files, and 18/18 valid XAML; `git diff --check` passes. The signed 110-file payload and installer passed independent same-signer/timestamp/policy verification. All four draft assets were downloaded back and matched local SHA-256 values; the downloaded installer independently returned `CanStageGitHubRelease=true`. Public latest reports non-draft/non-prerelease `v0.1.3`, four assets, commit `d052ae1`, and setup SHA-256 `33C332A658B6B5CF08304AE67A0E2A7A5AAE3CC23A357E5407913E305F05C73A`.
- Public artifact: `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.3`; setup length 14,833,040 bytes; signer `5688958FEA0056861558E8DCF9D2381AF46074B2`.
- Exact next action: from the correctly installed 0.1.2, let the user run `检查更新`, choose the visible download/install action, review the final confirmation, and complete the first real in-app 0.1.2-to-0.1.3 update. Do not claim the launch/install path is runtime-proven until that user receipt exists.

## Completed Slice - 2026-07-29 Released-state review and record readability

- Objective: independently review released `main` (`e4f0cf0`) rather than trusting the records, then fix what the review found.
- Dependencies: `.omx/verify-source-integrity.ps1`, Release build, full Debug suite, the public GitHub release endpoints, and the tracked development records.
- Risks: accepting recorded numbers as evidence, cutting oversized records positionally and destroying either recent entries or the reusable gate checklists, changing product behavior while repairing records, or launching the installer during review.
- Impact scope: one WPF failure path, one cleanup-authority simplification, and the layout of the development records. No update, signing, trust, installer, or system-mutation behavior changed.
- Acceptance: every recorded gate re-derived on this machine; every public-release claim re-derived from GitHub; each live record openable by the standard read path; no record content lost; focused/full/build/integrity gates pass.
- Status: completed. All recorded claims held. Three defects were found and fixed: (1) `UpdateWindow.OpenReleasePage_Click` called `Process.Start` with no catch while `Css.App` has no global `DispatcherUnhandledException` handler, so a ShellExecute failure could terminate the app from the update window; (2) `handoff.md`'s canonical sections still described the 2026-07-22 certificate blocker and contradicted the file's own newest entries; (3) four live records exceeded the 256 KB tooling read limit, so `CLAUDE.md`'s instruction to read `current.md` before changing the project failed outright.
- Verification: source integrity 385 files, InvalidUtf8 0, ReplacementFiles 0, 18/18 XAML; Release build 0 errors and 0 warnings; full Debug 1067/1067; focused update contracts 11/11. The record split was proven lossless by comparing all 1,724 entry blocks against their `HEAD` versions.
- Public re-verification (from GitHub, not from records): `v0.1.2` non-draft/non-prerelease with four assets; setup 14,832,504 bytes; GitHub digest `c5d8611…12c1` matches the records and the published `omnix-release.json`; `InstallerManifestSHA256` matches GitHub's digest for `installer-manifest.json`; `CommitSHA` `4a3e30c` passed CI `30339140502`; `e4f0cf0` passed CI `30340365520`.
- Not verified: no installer was launched, installed, or uninstalled; no product process was started; no current screenshot was taken, so visual evidence remains Warn.
- Bootstrap outcome: the first manual 0.1.2 install landed at `D:\Software\OMNIX-Entropy\Install\Install\`, exposing the installer/path-policy mismatch. The user subsequently reinstalled 0.1.2 into the correct managed directory. Read-only host verification now confirms `D:\Software\OMNIX-Entropy\Install\Css.App.exe`, valid expected signer and timestamp, fixed D drive, non-reparse directories, registry `InstallLocation=D:\Software\OMNIX-Entropy\Install\`, and no doubled executable. The machine-side bootstrap blocker is resolved; the installer source defect still requires the active-slice fix before another release.
- Exact next action: complete and publish the active managed-layout fix, then use the correctly installed 0.1.2 to exercise the first true in-app update against a future 0.1.3. Do not relax the layout guard.

## Completed Slice - 2026-07-28 In-app verified update installation

- Objective: replace the current check-only update window with a beginner-safe flow that downloads the fixed GitHub release package to D, verifies length/SHA-256/trusted same-signer continuity, asks for final confirmation, and launches only the interactive installer through `OperationPipeline`.
- Dependencies: validated `PersonalReleaseChannel`, fixed GitHub asset URL, current App Authenticode identity, `WindowsAuthenticodeSignatureVerifier`, a non-system-drive update staging directory, `SafetyOperationPipeline`, and an interactive non-silent process launcher.
- Risks: treating metadata as package trust, downloading to C, accepting a substituted signer, launching before verification, bypassing `OperationPipeline`, silently installing, overwriting the running application without user review, or claiming that 0.1.0 can acquire new updater code without one manual bootstrap install.
- Impact scope: update domain/client, D-first staging, package verification, explicit launch operation, update window UX, tests, version/release records, and a future signed release. No automatic install, silent arguments, trust-store change, certificate export, antivirus change, or unconfirmed process launch is authorized.
- Acceptance: update availability exposes a clear install action; download is bounded and D-first; package length/hash/trusted signer match the validated channel and the running App; failed evidence cannot expose launch; final launch has explicit confirmation and passes through `SafetyOperationPipeline`; installer remains interactive; focused/full/build/integrity gates pass; the bootstrap limitation is explained truthfully.
- Status: completed and publicly released as 0.1.2. Source commit `4a3e30c3feacdeaf8fcc1df6543ba14f1ddc6125` passed GitHub CI `30339140502`; the update window now exposes a verified install action; the downloader is length-bounded, D-first, SHA-256 checked, and anchored to the running App signer; final launch revalidates evidence and uses `SafetyOperationPipeline` with no silent arguments. Focused update/UX tests pass 8/8; full suite 1067/1067; Release build 0 errors with 18 environmental NU1900 warnings; integrity 385 files/18 XAML; diff check passes. The signed setup and all four GitHub assets were downloaded back and matched local SHA-256 evidence; the downloaded setup independently returned `CanStageGitHubRelease=true`. Public latest metadata reports non-draft/non-prerelease `v0.1.2`. Computer Use approval timed out before app launch, so current visual evidence remains Warn and no installer or product process was run.
- Public artifact: `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.2`; setup SHA-256 `C5D861160E3A38367B38F8FA9473FA6EB7D06485EF5202B1D4A5E7A3E76912C1`; signer `5688958FEA0056861558E8DCF9D2381AF46074B2`.
- Exact next action: the installed 0.1.0/0.1.1 build must use its release-page fallback and manually install 0.1.2 once. After that bootstrap, exercise a future test release through the new in-app download/verify/confirm path; do not claim automatic self-bootstrap for old code.

## Completed Slice - 2026-07-28 Public 0.1.1 release

- Objective: publish OMNIX-Entropy 0.1.1 from the current reviewed source, including the completed read-only system-footprint diagnosis, through the existing signed D-first GitHub update channel.
- Dependencies: a committed clean release revision, passing local and GitHub source gates, the existing non-exportable CurrentUser personal publisher certificate, the pinned Inno compiler, Windows SDK SignTool, the approved exact DigiCert/GlobalSign RFC3161 services, and the guarded installer/release scripts.
- Risks: publishing from a dirty or mismatched revision, creating assets with the wrong version, signing payload and setup with different identities, losing timestamp evidence, exposing local secrets or machine evidence, publishing before verification, or accidentally running the installer on the development machine.
- Impact scope: application/package version, release notes, source and release records, signed App/worker payload, signed D-first setup, fixed GitHub release manifests, tag `v0.1.1`, and public GitHub Release assets. No installer execution, product installation, trust-store mutation, private-key export, antivirus change, or LocalMachine certificate action is authorized.
- Acceptance: version reports 0.1.1; focused/full/build/source-integrity and public-candidate gates pass; the source revision is pushed and CI passes; payload and setup are signed by thumbprint `5688958FEA0056861558E8DCF9D2381AF46074B2` with a timestamp; independent installer verification reports `CanStageGitHubRelease=true`; the public `v0.1.1` Release contains the setup, installer manifest, channel manifest, and checksums; the latest-release endpoint reports 0.1.1; the installer is not run.
- Approval state: the user explicitly requested `发布 0.1.1`, authorizing public publication of this version. Existing certificate trust authorization remains CurrentUser-only; no additional system mutation is authorized.
- Status: completed. Release source commit `63fbb5e868bbf2231ac8236b2b5d577e816ddfce` passed GitHub CI `30334942521`; the final payload and installer passed independent signer, timestamp, hash, D-first, visible-directory, and no-silent-install checks. GitHub Release `v0.1.1` is public at `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.1`; the public latest endpoint reports 0.1.1 with four assets. The installer is 14,823,256 bytes with SHA-256 `F581F89A93E36145E8C6952E5C7D4B0F9E32C7A6EDC4F76C13E9E1B2F6ADACBB`, and downloaded assets were rehashed against their local release inputs.
- Exact next action: extend the official-uninstall post-scan to carry `SystemFootprints` as read-only residue evidence. Keep removal unavailable until exact current-state evidence, rollback material, explicit confirmation, and `OperationPipeline` coverage exist.

## Completed Slice - 2026-07-28 RogueCleaner-inspired system-footprint diagnosis

- Objective: study `aakk007/RogueCleaner` as an external product and source reference, then add the smallest useful OMNIX capability that explains where installed software inserts itself into Windows without copying its code, rules, branding, or destructive behavior.
- Dependencies: RogueCleaner README/source/license, OMNIX software-profile and application-drawer models, existing startup/service/task evidence, registry readers, Agent recommendation presentation, `OperationPipeline`, and Undo safety contracts.
- Risks: treating a vendor/name keyword as proof of unwanted software, exposing registry paths to beginners, introducing direct registry/service/task mutation, copying MIT-licensed code without attribution, or broadening antivirus-sensitive behavior before an isolated acceptance cycle.
- Impact scope: first prefer read-only system-footprint evidence and beginner-facing explanation for context-menu, Explorer, browser-host, file-association, startup, service, and scheduled-task presence. Any later mutation remains a separate confirmed operation with rollback evidence.
- Acceptance: the implementation is behavior/evidence based rather than brand-blacklist based; beginner output says where the software appears and what that means; technical locations stay behind details; no new direct delete/disable/process authority is introduced; focused/full/build/source-integrity gates pass.
- Status: completed. OMNIX now performs bounded read-only scans for context menus, Explorer namespace entries, browser Native Messaging hosts, and common file associations; correlates them only by install path or full application-name evidence; preserves them through growth enrichment; and shows a path-free “还会出现在哪” conclusion plus Agent advice in the application drawer. Technical evidence remains collapsed and no new mutation authority exists.
- Verification: focused 6/6; full Debug 1060/1060; Release build 0 errors with 18 NU1900 warnings from unavailable NuGet vulnerability metadata; source integrity 383 files, invalid UTF-8/replacement 0, and 18/18 XAML; smoke script parser pass; isolated real WPF smoke displayed both footprint categories and Agent advice and saved `.omx/qa-app-system-footprint.png`.
- Exact next action: extend the official-uninstall post-scan to report remaining system footprints as read-only residue evidence. Do not add removal until each entry has exact current-state evidence, rollback material, explicit confirmation, and an `OperationPipeline` handler.

## Completed Slice - 2026-07-23 Personal installer tool and signer bootstrap

- Objective: with the user's explicit continuation approval, install the reviewed Inno compiler on D and create one non-exportable CurrentUser personal code-signing identity scoped to OMNIX-Entropy, then build the first same-signer payload and setup without weakening application trust checks.
- Dependencies: winget package `JRSoftware.InnoSetup`, D-drive destination, Windows `New-SelfSignedCertificate`, CurrentUser certificate stores, existing portable/signed/installer builders and verifiers, and a safe timestamp endpoint.
- Risks: trusting an overly broad certificate, exporting a private key, duplicating identities, using the wrong Inno installation, confusing personal trust with public reputation, or running the resulting installer on the primary machine before separate acceptance.
- Impact scope: CurrentUser Inno installation and the exact named personal certificate in CurrentUser My, TrustedPeople, TrustedPublisher, and Root; one guarded bootstrap script and contracts; ignored artifacts; release candidate/setup generation; and records. No LocalMachine certificate store, PFX export, antivirus setting, SmartScreen bypass, installer launch, or product installation is authorized.
- Acceptance: exact Inno 6.7.3 compiler resolves from D with valid signature; one RSA non-exportable code-signing certificate exists in CurrentUser My with public trust copies in CurrentUser TrustedPeople, TrustedPublisher, and Root only; private key never leaves My; signed payload/setup pass existing independent verification; no setup is executed.
- Status: the user separately approved CurrentUser Root trust for certificate `5688958FEA0056861558E8DCF9D2381AF46074B2`. Independent inspection confirms the private key exists only in CurrentUser My; public copies exist in CurrentUser TrustedPeople, TrustedPublisher, and Root; all inspected LocalMachine stores contain zero matches. A fresh 110-file App/worker candidate is Valid, same-signer, DigiCert-timestamped, and independently verified. After pinning the missing official Inno 6.7.3 Simplified Chinese translation in the repository, `OMNIX-Entropy-0.1.0-win-x64-setup.exe` compiled and independently passed hash, signer, timestamp, D-first directory, visible-directory-selection, and no-silent-install checks. The setup was not launched.
- Approval state: explicitly granted by the user for certificate `5688958FEA0056861558E8DCF9D2381AF46074B2` in CurrentUser TrustedPeople, TrustedPublisher, and Root. No approval exists for LocalMachine, private-key export, antivirus changes, installer execution, product installation, or public release publication.
- Verification: focused signing/installer contracts 10/10; full Debug 1054/1054; Release build 0 errors (18 NU1900 warnings from unreachable NuGet vulnerability metadata); source integrity 380 files, invalid UTF-8/replacement 0, and 18/18 XAML; three release scripts parse; public candidate inventory 457 files/about 6.3 MB with zero binary or signing-material candidates. The final installer independently reports `CanStageGitHubRelease=true`; SHA-256 is `5680C3847F23291784BB38FB1D01FACAFC6013DC47F06B611C170BCDC63955BE`. Source commit `4aba92d` is pushed; GitHub CI run `30019958502` passed every step in 2m52s.
- Exact next action: wait for a separate explicit decision before uploading a draft Release, publishing a Release, or running the installer. Disposable-machine behavioral acceptance remains pending.

## Completed Slice - 2026-07-23 D-first personal installer foundation

- Objective: turn the verified signed payload into a directory-selectable Windows installer that defaults to `D:\Software\OMNIX-Entropy\Install`, then make GitHub release staging accept only that independently verified installer.
- Dependencies: Inno Setup compiler, existing signed-candidate verifier, Windows SDK SignTool, an explicitly selected RSA code-signing certificate, fixed GitHub release identity, and immutable SHA-256/manifests.
- Risks: silently installing, defaulting back to C, compiling from an unverified payload, signing installer and payload with different publishers, overwriting an existing release artifact, leaking certificate material, or claiming an installable release without a compiler/certificate run.
- Impact scope: installer definition, local build and read-only verifier scripts, personal release staging/channel asset contract, focused tests, release documentation, and development records. No application download, installer launch, UAC automation, certificate generation/import, trust-store change, or system installation is authorized in this slice.
- Acceptance: visible installer always exposes directory choice and defaults to D; silent setup is refused; only a verified same-signer payload can compile; output installer is valid Authenticode from the same signer and has a bounded manifest; release staging re-verifies it and emits the fixed setup asset; focused/full/build/integrity gates pass; missing external tools fail before publication.
- Status: Completed at source, policy, and refusal level. The Inno definition is visible and D-first, rejects silent setup, signs Setup/uninstaller, and copies only the verified payload. The builder requires explicit tools/certificate and same signer; the independent verifier is read-only; GitHub staging now accepts only a verified setup EXE plus installer manifest.
- Verification: focused installer/release/update/guide contracts 19/19; full Debug 1051/1051; Release build 0 errors; source integrity 379 files, invalid UTF-8/replacement 0, and 18/18 XAML; all three changed PowerShell scripts parse successfully. Official Inno documentation confirms the selected `x64compatible`, `SignTool`, and `SignedUninstaller` behavior.
- External readiness: read-only inspection at `2026-07-23T00:37:12Z` found SignTool at `D:\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe`, no Inno compiler, and zero eligible CurrentUser RSA code-signing certificates. No setup EXE or GitHub Release was created.
- Exact next action: after explicit user approval to install Inno Setup and create/select a personal code-signing certificate, build and independently verify the first setup; otherwise continue with application-side bounded download/verification without enabling installer execution.

## Completed Slice - 2026-07-22 First public CI remediation

- Objective: make the first public `main` build reproducible from tracked files only and obtain a passing GitHub Actions run without weakening signing or runtime safety.
- Dependencies: top-level `.omx` smoke scripts referenced by tests, LF-normalized checkout, Release binaries required by production-authorization contracts, and deterministic xUnit execution on a shared Windows runner.
- Risks: publishing machine evidence with the smoke scripts, letting local untracked files mask an incomplete repository, rerunning pipe-sensitive tests in parallel, or changing product behavior while repairing CI.
- Impact scope: repository tracking rules, line-ending policy, CI order/action pins, test-runner scheduling, focused repository contracts, and development records. Product runtime behavior is unchanged.
- Acceptance: the public candidate scan remains free of secrets and machine-specific evidence; a fresh tracked-files-only checkout contains every test dependency; focused/full local tests, Release build, source integrity, and the new GitHub Actions run pass.
- Status: Completed. The first public run `29932623250` exposed the four reproducibility gaps; commit `06534d4` closes them and replacement run `29933681994` passed every job in 2m59s.
- Verification: focused repository contracts 4/4; working-tree full Debug 1048/1048; tracked-only archive full Debug 1048/1048; both Release builds succeeded; both integrity runs reported 378 files, invalid UTF-8/replacement 0, and 18/18 XAML; GitHub Actions repeated 1048/1048 and the same integrity result.
- Exact next action: keep the now-green public CI as the source gate, then implement a directory-selectable D-first installer/update package with explicit confirmation, same-signer verification, and rollback; do not claim an installable GitHub release before that slice exists.

## Completed Slice - 2026-07-22 GitHub personal release foundation and read-only update check

- Objective: connect the empty public `plnoble/OMNIX-Entropy` repository to a beginner-safe personal install/update channel while preserving the existing valid-same-signer gate for privileged workers.
- Dependencies: first public-source commit, secret/privacy audit, reproducible Release build, immutable manifest and SHA-256 evidence, GitHub Releases, local-only personal Authenticode private key, explicit update confirmation, staged replacement, and rollback retention.
- Risks: publishing local/private evidence, leaking a certificate private key, treating GitHub transport as binary trust, accepting an unsigned substituted elevated worker, silent UAC/update behavior, overwriting a running installation, or claiming release readiness before a personal signer exists.
- Impact scope: repository hygiene, CI/release workflows, release metadata/scripts, update domain and UI, tests, and documentation. No certificate generation/import, trust-store change, SmartScreen bypass, silent installation, or weakening of production mutation authorization is authorized in this slice.
- Acceptance: public-source audit is clean; workflows use least privilege and pinned release identity; artifacts carry complete hashes/provenance; the client checks only the fixed repository, downloads only after user action, verifies manifest/hash/signer before exposing install, stages outside the running payload, requires explicit confirmation, and retains rollback evidence; full tests/build/source-integrity pass.
- Status: Completed for the release foundation and read-only update slice. The local repository is connected to the fixed public `plnoble/OMNIX-Entropy` origin; machine screenshots, QA payloads, runtime data, signing files, and local secrets are ignored; CI is least-privilege and commit-pinned; the local publisher only creates verified same-signer draft Releases; the app checks the fixed latest-release API only on a user click and exposes only an exact validated GitHub release page.
- Implementation: added root README, public repository hygiene, pinned Windows CI, a local signed-candidate-to-draft-Release staging script, a versioned fixed-repository channel manifest, bounded GitHub metadata client, and a compact update dialog. The dialog does not download, install, elevate, or change security settings.
- Installer decision: Velopack was rejected for this product because its normal Windows installer targets `%LocalAppData%`, conflicting with the explicit D-drive-first requirement. A directory-selectable installer with default `D:\Software\OMNIX-Entropy\Install` remains the next implementation slice.
- Verification: focused update/release tests 13/13; full Debug suite 1048/1048; Release build 0 warnings/errors; source integrity 377 files and 18/18 XAML; PowerShell parser pass; 424 public candidates, about 6 MB, with zero binary/signing-secret/real-username candidates. Real WPF visual acceptance remains Warn because Computer Use timed out launching the Debug app and no OMNIX window appeared afterward.
- Safety: no certificate was generated/imported/trusted, no release was created, no package was downloaded, no installer or UAC was launched, and no existing valid-same-signer production gate was weakened.
- Exact next action: after the first audited source push, implement and verify a directory-selectable installer/update package that defaults to D, preserves explicit confirmation, signs App/Worker/installer with the same local personal certificate, and retains a rollback package; do not adopt a C-drive-only updater.

## Completed Slice - 2026-07-22 Non-default Windows SDK SignTool discovery

- Objective: correct the signing prerequisite inspector's false `SignToolFound=false` result when Windows SDK is installed under the official registered root on a non-system drive.
- Dependencies: the two standard `HKLM\SOFTWARE[\WOW6432Node]\Microsoft\Windows Kits\Installed Roots` locations, `KitsRoot10`, existing bounded version/architecture enumeration, explicit-path precedence, and current-machine D-drive SDK evidence.
- Risks: broad registry or drive scans, accepting an arbitrary executable, changing the SDK installation, weakening explicit-path refusal, introducing writes, or claiming signing readiness while the certificate/disposable gates remain absent.
- Impact scope: read-only prerequisite inspector, focused contracts, signing guide/audit wording, and development records. No SDK install, certificate access beyond the existing CurrentUser read, signing, trust change, process launch, or system mutation.
- Acceptance: inspector reads only the two exact installed-root keys; validates fully qualified existing roots/files; enumerates only `bin` direct version directories and x64/x86/arm64 candidates; current machine resolves `D:\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe`; missing requirements no longer claim SignTool is absent; certificate and disposable gates remain unchanged; focused/full/build/integrity gates pass.
- Status: Completed. The inspector now reads only the two exact standard installed-root keys, deduplicates validated roots, and performs the existing direct version/architecture enumeration under each root before trying the default Program Files location.
- Last verification: TDD red 3/4 then focused 4/4 and guide/audit group 8/8; full 1035/1035; Release 0 warnings/errors; source integrity 370 files and 17/17 XAML; script parser errors 0 and non-ASCII bytes 0. Current report at `2026-07-22T14:04:45Z` resolves `D:\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe` as `WindowsKitsRegistry`.
- Safety: no recursive drive scan, SDK change, registry write, certificate selection, signing, process launch, or system mutation was added.
- Exact next action: retain registered-root discovery and explicit-path precedence; SignTool is no longer a blocker on this machine.

## Completed Slice - 2026-07-22 V1 requirement completion audit and recent-install sort

- Objective: audit the original beginner-facing V1 plan requirement by requirement and close any local implementation gap before treating external release evidence as the only remaining work.
- Dependencies: current MainWindow navigation and handlers, software inventory/profile mapping, application catalog query/presentation, operation-pipeline tests, and the original V1 acceptance statements preserved in project records.
- Risks: mistaking a large green suite for requirement coverage, adding a UI sort without real registry evidence, inventing installation dates when Windows does not provide one, losing metadata in profile enrichment, or expanding mutation authority during a read/presentation change.
- Impact scope: read-only installed-software metadata, catalog sorting, one application-page control, focused tests, completion-audit evidence, and development records. No installer launch, uninstall, cleanup, migration, startup change, signing, trust change, or system mutation.
- Acceptance: Windows uninstall `InstallDate` is parsed only from recognized date formats; unknown/invalid dates remain unknown; profile enrichment preserves the value; application catalog exposes and correctly orders `RecentInstall`; a stable UI control maps to that enum; related/full/build/integrity gates pass; remaining V1 requirements are classified against direct source/test/runtime evidence.
- Status: Completed. `docs/development/v1-completion-audit.zh-CN.md` now classifies every original V1 requirement against direct implementation, focused tests, current-machine evidence, and missing external evidence. The audit found and closed the absent recent-install sort.
- Implementation: uninstall-registry `InstallDate` accepts only `yyyyMMdd` or `yyyy-MM-dd`; invalid/missing values stay unknown; the value survives profile building and growth enrichment; `RecentInstall` sorts known dates newest first and unknown dates last; the application sort control exposes the option with stable `AppSortComboBox` AutomationId; known dates appear only in technical details.
- Last verification: TDD red date contracts failed at compile and audit contract failed on the missing document; related application/inventory 209/209; critical-workflow audit group 337/337; focused audit/inventory/growth/product 205/205; full 1035/1035; Release 0 warnings/errors; source integrity 370 files and 17/17 XAML; Agent direct mutation-authority hits 0.
- Visual evidence: Warn. Computer Use timed out launching the current Debug executable, a fresh window list contained no OMNIX window, and a read-only process check reported `CssAppProcessCount=0`; no screenshot is claimed and no security bypass was attempted.
- Exact next action: retain the audit as the requirement/evidence source of truth. Real modification and current-version visual acceptance remain downstream of the signed disposable-environment gate.

## Blocked External Slice - 2026-07-22 Signed candidate and disposable acceptance

- Objective: create one real same-signer signed OMNIX-Entropy candidate, independently verify it after transfer, and complete the exact ten-case behavioral receipt on a checkpointed disposable Windows environment.
- Dependencies: an explicitly approved real code-signing certificate with private key and code-signing EKU, Windows SDK `signtool.exe`, an allowlisted disposable Windows environment, reset-checkpoint evidence, the verified fixture kit, and manual operator evidence.
- Risks: treating an unsigned package as accepted, changing certificate trust, running fixture mutation on the primary machine, automating UAC, or claiming completion from protocol/source tests alone.
- Impact scope: release artifact creation and external behavioral acceptance only. Product mutation authority and local security settings must remain unchanged.
- Acceptance: App and Elevated worker are valid timestamped Authenticode binaries from the same approved signer; transfer verifier passes; fixture/session manifest hashes are bound; all ten cases pass with independent evidence; environment reset is attested; final read-only verifier emits `BehavioralAcceptanceComplete=true`.
- Status: Blocked after the same publisher-identity and disposable-environment conditions persisted across three resumed goal turns. Product workflows, packaging, verification, deterministic fixtures, release guidance, and the requirement audit are complete and fail closed; no honest repository edit can create an approved publisher identity or substitute for independent machine evidence.
- Last verification: full regression 1035/1035; Release build 0 warnings/errors; source integrity 370 files and 17/17 XAML. Latest read-only inspection at `2026-07-22T14:04:45Z` reports `SignToolFound=true`, `SignToolResolution=WindowsKitsRegistry`, `CertificateStoreReadable=true`, `CodeSigningCertificateCount=0`, and `CanCreateSignedCandidate=false`.
- Blockers: no currently valid CurrentUser RSA code-signing certificate with an accessible private key and code-signing EKU is available, and no explicitly checkpointed disposable Windows operator run has been provided. SignTool and antivirus definitions are no longer listed as missing prerequisites.
- Exact next action: after the user obtains/installs an approved RSA Authenticode certificate and provides a checkpointed disposable Windows environment, rerun the inspector, explicitly select the approved thumbprint, create/transfer/reverify the candidate, and complete the ten-case receipt from `docs/release/README.zh-CN.md`.

## Completed Slice - 2026-07-22 Beginner-safe signing prerequisite guide

- Objective: turn the verified external signing gap into an exact Chinese operator path that a non-expert can follow without weakening trust or choosing an unsupported distribution route.
- Status: Completed. The official-reference guide documents the one currently supported local certificate route, exact inspect/sign/transfer/acceptance sequence, user-controlled prerequisites, and explicit trust boundaries. A short release index now exposes that sequence from one entry point.
- Safety: the release pipeline accepts only RSA code-signing certificates because the target Smart App Control path does not currently support ECC signatures. The guide refuses self-signed production use, trust/security bypasses, secret handling, automatic certificate selection, and claims that remote signing routes work with the current scripts.
- Verification: red index contract 0/3; focused guide/index plus RSA signing/inspection/verification 15/15; full 1033/1033; Release 0 warnings/errors; source integrity 369 files and 17/17 XAML; changed scripts parse and are ASCII-only.
- Exact next action: retain the guide and release index as the operator source of truth; do not sign until the read-only inspector reports readiness and the user explicitly selects an approved certificate.

## Completed Slice - 2026-07-22 Read-only signing prerequisite inspection

- Objective: replace the informal release-signing blocker with one bounded, machine-readable, read-only report of Windows SDK `signtool.exe` availability and eligible CurrentUser code-signing certificates.
- Status: Completed. `scripts/inspect-release-signing-prerequisites.ps1` validates an optional explicit tool path or inspects only PATH and bounded Windows Kits version/architecture locations; it reads only `Cert:\CurrentUser\My`, filters current private-key code-signing certificates, never chooses one, and emits object or JSON readiness.
- Safety: no import, generation, signing, installation, trust modification, process launch, recursive drive scan, or file write authority exists. The production signing transform now uses the same Windows PowerShell 5.1-compatible X509 EKU extension parser.
- Verification: red 0/3; focused 4/4; signed-release plus inspector 8/8; full 1030/1030; Release 0 warnings/errors; source integrity 368 files and 17/17 XAML; scripts parse with zero non-ASCII bytes. Current-machine JSON reports the exact two missing prerequisites without exposing or selecting a certificate.
- Exact next action: retain the inspector as the prerequisite gate and do not invoke the signed-release transform until it reports readiness and the user explicitly supplies the approved certificate thumbprint.

## Objective

Build a beginner-friendly Windows PC manager. The foreground UI should show health summaries, app icons, status dots, and Computer Agent advice. The backend keeps C-drive scanning, software profiles, install routing, quarantine, action timeline, and the safety operation pipeline. Any system-changing action must first become an auditable recommendation, then pass user confirmation and the local safety pipeline.

## Status

V1 product refactor is still in progress. It is not a finished product. Current capabilities:

Active slice, 2026-07-07: make C-drive/home health findings more beginner-readable and interactive while keeping actions read-only or plan-only unless they go through the local safety pipeline.

- Home health summary and key findings entry points.
- Real left-navigation pages: Home, Apps, C-drive cleanup, Install guard, Undo center, AI Agent.
- App management grid, filters, search, sorting, and right-side drawer.
- `SoftwareProfile` backend for install path, uninstall command, publisher, services, startup entries, scheduled tasks, running processes, and size estimates.
- Read-only Marvis recognition: installed on D drive, AI category, `MarvisSvr` service, related processes, and install size.
- "Uninstall cleaner" safety-plan window: shows official uninstall command, pre-run warnings, and post-uninstall rescan requirement, but does not run uninstallers.
- Official uninstaller execution gate model: default disabled, requires explicit feature enablement, snapshot id, command confirmation, app-close confirmation, post-uninstall rescan confirmation, and existing uninstaller path before it can produce a high-risk operation descriptor.
- Official uninstaller preflight checklist: converts the execution gate into user-facing steps for command trust, uninstaller file existence, snapshot, command review, app-close confirmation, and post-uninstall rescan.
- Official uninstall command trust: trusts uninstallers under the app install directory; trusts interactive Windows Installer product uninstall commands such as `msiexec /x {GUID}`; trusts external uninstallers only when the captured executable signature subject matches the app publisher; blocks shell wrappers, unsigned/mismatched external uninstallers, silent MSI flags, and MSI install/repair commands.
- App-drawer migration preview: shows whether an app is already reasonably installed on D drive, can be planned for migration, should be cache-only, is blocked as a system tool, and which target root would be recommended. This preview does not move files.
- Migration plan window: the "Move to D drive" app-drawer action opens a plan-only window with destination, score, blockers, snapshot requirement, rollback plan, and post-migration monitoring. It still does not move files.
- Migration readiness gate: models the future execution request gate for migration. It requires feature enablement, snapshot id, plan confirmation, app-close confirmation, rollback manifest, destination free-space check, and post-migration monitoring confirmation before it can create a high-risk operation descriptor. No execution handler exists.
- Migration rollback evidence and destination space probe: can generate a plan-only rollback manifest JSON and check destination drive free space. The migration plan window shows a rollback manifest draft line and destination space line. The window now has a user-confirmed "Create rollback manifest" action that writes only JSON evidence and refreshes readiness. No app files are moved.
- Undo center: quarantine actions are written to the timeline; restore uses manifest paths and refuses to overwrite existing original paths.
- Quarantine retention/capacity planning: creates suggestions only, no automatic permanent deletion.
- Post-uninstall residue scan: if the software still appears installed, residue handling is blocked; if removed, only low-risk cache/log paths can become a quarantine operation.

Recent progress:

- Migration preflight checklist now shows explicit snapshot evidence (`快照证据：...`) and a stronger plan-confirmation scope: target location, affected paths, rollback plan, and post-migration monitoring.
- Added `Migration_readiness_checklist_shows_snapshot_evidence_and_plan_confirmation_scope`; it first failed because snapshot detail only said `快照 ID` and plan confirmation only said the user viewed the plan.
- Verification: focused migration evidence test passed 1/1; `ProductExperienceTests` passed 53/53; full suite passed 110/110; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- This did not add a migration execution handler or move any files.
- Post-uninstall residue review now short-circuits when the selected app is still visible in the current app inventory: it shows the "still installed, do not clean residue" result inline in the app drawer instead of launching a slow full rescan or opening an informational modal.
- Added `UninstallResidueReviewPlanner.TryBuildStillInstalledReport(...)` plus safety fields on `UninstallResidueReviewViewModel`: `SafetyText` and `CanExecuteDirectly=false`.
- `ReviewSelectedUninstallResidueAsync` now uses `ShowResidueReviewInline(...)` for non-actionable residue reviews. Low-risk residue movement still requires the existing second confirmation and safety pipeline.
- TDD red observed: `Residue_review_presentation_is_non_executable_until_user_confirms_a_safe_operation` failed because `CanExecuteDirectly`/`SafetyText` did not exist; `Residue_review_planner_uses_cached_inventory_to_block_when_app_is_still_visible` failed because `UninstallResidueReviewPlanner` did not exist.
- Verification: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter UninstallResidueScanTests` passed 8/8; full suite passed 109/109; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- GUI verification for the residue review path was attempted but the full-rescan path was too slow and the later GUI command was interrupted; no `Css.App` process remained. This is not counted as GUI proof for the new inline residue review path.
- C-drive page top scan target now presents `系统盘 C 盘` as an automatic, read-only target instead of a visible `C:\` path selector/input.
- Added `CDrivePageChromePresenter` / `CDrivePageChromeViewModel` so this page chrome is product-tested: automatic system drive, not editable, technical report hidden by default.
- C-drive technical report is now collapsed behind a `显示技术报告` button; root-cause cards, growth cards, and recommendations remain the first visible experience.
- Real GUI smoke after a C-drive scan verified `scanCompleted=true`, system-drive label present, technical report toggle present before/after scan, report box hidden before/after scan, 4 root-cause items, 3 growth items, and 15 recommendation items. Screenshot: `.omx\qa-cdrive-system-drive-and-collapsed-report.png`.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_page_chrome_marks_system_drive_as_automatic_and_hides_technical_report_by_default` failed because `CDrivePageChromePresenter` did not exist.
- Verification: focused chrome test passed 1/1; `ProductExperienceTests` passed 52/52; full test suite passed 107/107; `dotnet build ComputerSecuritySoftware.slnx --no-restore` passed with 0 warnings and 0 errors.
- Replaced the homepage key-finding MessageBox flow with an inline `Agent 回答` panel above the key-finding list, so repeated clicks update the page instead of stacking popups.
- Added `HomeAgentResponsePresenter` and `HomeAgentResponseViewModel` to normalize explain/detail/plan responses for the homepage.
- GUIA verified a real C-drive scan, clicked `让 Agent 解释`, `查看详情`, and `生成处理方案`, found inline safety/pipeline text for each, and confirmed the process still had only one window. Clean screenshot: `.omx\qa-home-agent-inline-response-visible.png`.
- Added `HealthFindingAgentExplanationBuilder`, `HealthFindingDetailPresentationBuilder`, and `HealthFindingActionPlanBuilder` so homepage key findings can explain, show details, or generate a plan without executing anything.
- Wired the homepage key-finding buttons `让 Agent 解释`, `查看详情`, and `生成处理方案` to WPF handlers. They show local MessageBox explanations/plans and update the status bar; they do not create cleanup, migration, service, startup, registry, or uninstall operations.
- Added TDD coverage requiring these Agent/detail/plan outputs to hide raw local paths and keep `CanExecuteDirectly=false`.
- Added `CDriveRootCauseSummaryBuilder` and WPF bindings so the C-drive page now leads with beginner-facing root-cause cards instead of a dense path report.
- Added `GrowthFindingPresenter` and WPF bindings so growth findings display friendly source names, size changes, and Agent suggestions instead of raw `C:\...` paths.
- Preserved the raw C-drive technical report as a smaller secondary section for advanced inspection.
- TDD red/green completed for `C_drive_root_cause_summary_turns_path_report_into_beginner_cards` and `C_drive_growth_presenter_hides_paths_and_explains_change`.
- Runtime C-drive right-side recommendation cards were GUI-verified before the latest summary-card changes; screenshot: `.omx\qa-cdrive-cards-real-scan.png`.
- Post-change GUI verification was attempted but rejected by the approval/usage-limit system; no workaround was attempted.
- Removed the old overwritten `GrowthListBox.ItemsSource` assignment from `MainWindow.xaml.cs`; growth findings now have only the new `GrowthFindingPresenter.CreateList(...)` binding after scan reset.

- Added `RecommendationCardPresenter` and `RecommendationCardViewModel` so C-drive recommendation cards now present separate lines for `发生了什么`, `Agent 建议`, `能不能后悔`, `预计释放/影响`, and `安全边界`.
- Added `C_drive_recommendation_card_explains_happened_agent_advice_undo_and_impact`; it first failed because `RecommendationCardPresenter` did not exist, then passed after the new presenter and WPF bindings were added.
- C-drive recommendation execution still uses the existing `OperationDescriptor` and safety pipeline. No cleanup behavior was changed or executed.
- An old unused private `RecommendationCardView` remains in `MainWindow.xaml.cs`; the active UI path no longer references it. Removal was deferred because the historical mojibake block could not be patched safely.
- Localized the uninstall safety window, official-uninstall preflight checklist, command trust summary, and app-drawer uninstall preview to beginner-readable Chinese while official uninstaller execution remains disabled.
- Added `Uninstall_safety_window_body_uses_plain_chinese_while_official_uninstaller_stays_disabled`; it first failed because the modal summary did not explicitly contain `只预览`, then passed after localization.
- Tightened `App_drawer_contains_uninstall_preview_without_executing_uninstall`; it first failed on old English drawer copy, then passed after `AppPresentationBuilder.CreateUninstallPreview` was localized.
- GUIA launched `Css.App.exe`, scanned apps, selected `火绒安全软件`, opened the uninstall safety preview modal, and verified required Chinese safety/preflight text with no old English phrases visible. No uninstaller was launched and no files were deleted.
- Localized the migration plan window body and preflight checklist copy to beginner-readable Chinese while preserving preview-only behavior.
- Added `Migration_plan_presentation_body_uses_plain_chinese_while_staying_preview_only`; it first failed on the old English `Ollama migration plan` title, then passed after localization.
- Updated older migration presentation tests to assert the new Chinese copy while keeping safety assertions such as `CanRunMigration == false`.
- GUIA verified the scoped migration modal contains Chinese text for migration plan, preview-only state, no-file-move safety, preflight, rollback plan, post-migration monitoring, and rollback manifest creation; no rollback manifest was created and no migration action was executed.
- Localized app drawer top summaries and Agent advice: install location, size/growth, residency, C-drive migration advice, observe advice, and normal advice now show plain Chinese before technical details.
- Added `App_drawer_top_summary_uses_plain_chinese_before_technical_details`; after fixing a test assertion API mistake, it failed on the old English `Installed on D drive` text, then passed after implementation.
- Static old-phrase search found no production `AppPresentation.cs` matches for prior English drawer summary phrases; the only match was a negative test assertion.
- Localized app-drawer action labels/reasons to beginner-friendly Chinese: `卸载干净点`, `迁移到 D 盘`, `清理缓存`, `关闭自启动`, and `技术详情`.
- Added a product test that first failed on the old English action labels, then passed after the localization change.
- Updated the WPF app drawer buttons and migration preview shell labels with ASCII-safe XAML character references.
- Localized the migration plan window shell actions for rollback manifest creation/close and its rollback-manifest confirmation messages.
- GUI smoke launched `Css.App.exe`, scanned app inventory, selected one app, and UIAutomation found the Chinese drawer operation buttons without invoking them.
- Added app-drawer migration summary and preview lines in `AppDrawerViewModel`.
- Added tests for C-drive app migration preview, D-drive "already reasonable" status, cache-only migration for unknown install roots, and system-tool migration blocking.
- Updated the WPF app drawer to show a "Migration plan preview" section.
- Added `MigrationPlanPresentationBuilder`, `MigrationPlanWindow`, and `PreviewMigration_Click`.
- Added tests that the migration plan is preview-only, requires snapshot/rollback/monitoring, treats D-drive apps as monitor-only, and blocks system tools.
- Added `MigrationExecutionGate`, `MigrationExecutionReadiness`, `MigrationPreflightChecklistBuilder`, and checklist binding in the migration plan window.
- The migration gate can produce a high-risk `migration.execute` operation descriptor only when all readiness conditions pass, but no handler can execute it.
- Added `MigrationRollbackManifestBuilder`, `MigrationRollbackManifestStore`, and `MigrationDestinationSpaceProbe`.
- Added `MigrationPlanPresentationOptions`; the migration plan window now displays a rollback manifest draft and destination free-space status.
- Added `MigrationRollbackManifestCreationService`; the WPF migration plan window now asks for confirmation, writes a plan-only rollback JSON file, and refreshes the checklist to show the rollback manifest as ready.
- App tile status labels are localized to Chinese short tags such as `需关注`, `后台常驻`, and `正常`.
- Fixed app catalog search so localized placeholder text such as `搜索应用` does not filter out all scanned apps.
- GUI smoke verified app scanning, app selection, migration-plan opening, and the user-confirmed rollback manifest save. Screenshot: `.omx\qa-migration-manifest-created.png`.
- App tile UI Automation names now use app-specific readable text such as `火绒安全软件, Needs attention` instead of the internal `Css.App.MainWindow+AppTileUi` type name.
- Rewrote `ProductExperienceTests.cs` as explicit UTF-8/ASCII after a bad PowerShell encoding write corrupted the file; verification now passes.
- Added the app-drawer button `DrawerResidueReviewButton` ("post-uninstall residue review"). It rescans software inventory, builds a residue review, explains without action if the software still exists, and only after a second user confirmation sends low-risk cache/log residue through `QuarantineOperationPolicy -> SafetyOperationPipeline -> QuarantineOperationHandler` into quarantine and the undo timeline.
- Added `OfficialUninstallExecutionGate` and connected it to the uninstall safety-plan window so the UI can show why official uninstaller execution is still blocked.
- Added `OfficialUninstallCommandTrustEvaluator` and connected command trust into the official uninstall gate and safety-plan window.
- Added safe MSI command recognition to the official uninstall trust layer: interactive product uninstall is allowed to create a gated high-risk operation descriptor, but silent/reduced-UI MSI flags and repair/install commands are blocked.
- Added publisher/signature matching to the official uninstall trust layer: an external uninstaller can pass only when the signature subject matches the app publisher; mismatches remain blocked.
- Added `OfficialUninstallPreflightChecklistBuilder` and bound its steps into the uninstall safety-plan window so users can see what is ready, waiting, or blocked before any official uninstaller could be requested.

Real official uninstaller execution, real app migration, startup/service disabling, and registry cleanup are still intentionally not enabled.

## Current Branch

`main`. The repository still has no initial commit; all project files are untracked.

## Last Verification

2026-07-07:

- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Home_agent_response_presents_explain_detail_and_plan_without_modal_execution` failed because `HomeAgentResponsePresenter` did not exist.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Home_agent_response_presents_explain_detail_and_plan_without_modal_execution`: first failed because the detail safety boundary did not explicitly include direct-execution/safety-pipeline language, then passed after tightening the copy.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Home_agent_response_presents_explain_detail_and_plan_without_modal_execution|Agent_explanation_for_health_finding_is_plain_and_non_executable|Health_finding_detail_and_plan_buttons_are_read_only_and_safe"`: Pass, 3/3 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 51/51 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 106/106 tests.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors.
- GUIA inline-response verification: clicked homepage key-finding buttons after a real scan; `hasAgentAnswer=True`, `hasPlan=True`, `hasSafety=True`, `processWindows=1`; screenshot `.omx\qa-home-agent-inline-response-visible.png`.
- Static check: only one `HomeAgentResponseTitleTextBlock`/body/safety block exists in XAML; homepage button handlers no longer call `MessageBox.Show`.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Agent_explanation_for_health_finding_is_plain_and_non_executable` failed because `HealthFindingAgentExplanationBuilder` did not exist.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Agent_explanation_for_health_finding_is_plain_and_non_executable`: Pass, 1/1 test.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Health_finding_detail_and_plan_buttons_are_read_only_and_safe` failed because `HealthFindingDetailPresentationBuilder` and `HealthFindingActionPlanBuilder` did not exist.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Health_finding_detail_and_plan_buttons_are_read_only_and_safe`: Pass, 1/1 test.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Agent_explanation_for_health_finding_is_plain_and_non_executable|Health_finding_detail_and_plan_buttons_are_read_only_and_safe"`: Pass, 2/2 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 50/50 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 105/105 tests.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors.
- Static check: homepage key finding buttons bind `ExplainHealthFinding_Click`, `ShowHealthFindingDetails_Click`, and `CreateHealthFindingPlan_Click`; `HealthFindingAgentExplanation.cs` keeps explanation/plan models non-executable.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_root_cause_summary_turns_path_report_into_beginner_cards` failed because `CDriveRootCauseSummaryBuilder` did not exist.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_root_cause_summary_turns_path_report_into_beginner_cards`: Pass, 1/1 test.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 47/47 tests at that point.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 102/102 tests.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_growth_presenter_hides_paths_and_explains_change` failed because `GrowthFindingPresenter` did not exist.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_growth_presenter_hides_paths_and_explains_change`: Pass, 1/1 test.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 48/48 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 103/103 tests.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors.
- Static check: C-drive UI binds `CDriveSummaryHeadlineTextBlock`, `CDriveRootCauseListBox`, and `GrowthFindingPresenter.CreateList(...)`; raw `ReportTextBox` remains as a smaller technical report.
- GUI smoke before the summary-card change launched `Css.App.exe`, ran a C-drive scan, and verified the right-side recommendation cards after a real scan. Screenshot: `.omx\qa-cdrive-cards-real-scan.png`.
- GUI post-change verification for the new left-side summary cards was rejected by usage limits; no workaround was attempted.
- Refactor cleanup: removed the old raw-path growth-list assignment from `MainWindow.xaml.cs`.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_growth_presenter_hides_paths_and_explains_change`: Pass, 1/1 test after cleanup.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 48/48 tests after cleanup.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 103/103 tests after cleanup.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors after cleanup.
- Static check: `GrowthListBox.ItemsSource` now appears only in the scan reset and the `GrowthFindingPresenter.CreateList(...)` assignment.

- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_recommendation_card_explains_happened_agent_advice_undo_and_impact` failed because `RecommendationCardPresenter` did not exist.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter C_drive_recommendation_card_explains_happened_agent_advice_undo_and_impact`: Pass, 1/1 test.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 46/46 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 101/101 tests.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors.
- Static check: active C-drive recommendation XAML binds `WhatHappened`, `AgentSuggestion`, `UndoStatus`, and `ImpactText`; execution selection uses `RecommendationCardViewModel`.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Uninstall_safety_window_body_uses_plain_chinese_while_official_uninstaller_stays_disabled` failed because the summary lacked explicit `只预览`.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Uninstall_safety_window_body_uses_plain_chinese_while_official_uninstaller_stays_disabled`: Pass, 1/1 test.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_drawer_contains_uninstall_preview_without_executing_uninstall` failed on old English drawer copy such as `Uninstall preview only` and `First step`.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_drawer_contains_uninstall_preview_without_executing_uninstall`: Pass, 1/1 test.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 45/45 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 100/100 tests.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors.
- Static check: old uninstall-window/app-drawer phrases no longer appear in relevant production files; only the internal key `post-uninstall-rescan` remains.
- GUI smoke: launched `Css.App.exe`, scanned apps, selected `火绒安全软件`, opened `卸载安全方案窗口`, found `只预览`, `不会运行卸载器`, `卸载前安全检查`, `命令可信度`, `卸载器文件`, and `卸载后重新扫描残留`, and found no old English uninstall phrases. No official uninstaller or cleanup operation was invoked.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Migration_plan_presentation_body_uses_plain_chinese_while_staying_preview_only` failed because the title still said `Ollama migration plan`.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter Migration_plan_presentation_body_uses_plain_chinese_while_staying_preview_only`: Pass, 1/1 test.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 44/44 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 99/99 tests.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors.
- Static check: old migration-window phrases such as `Preview only`, `Suggested destination`, `Rollback plan`, and `Monitoring after migration` no longer appear in production migration presentation/checklist files.
- GUI smoke: launched `Css.App.exe`, scanned apps, opened a migration preview window, and scoped UIAutomation to the migration modal. It found Chinese text including `迁移方案`, `只预览`, `不会移动文件`, `迁移前检查`, `回滚方案`, `迁移后观察`, and `生成回滚清单`; no old migration-window phrases were visible in the scoped modal search.

2026-07-01:

- TDD correction: initial focused drawer-summary test run failed to compile due a wrong FluentAssertions overload; after fixing the assertion, the test failed for the expected old-English location summary.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_drawer_top_summary_uses_plain_chinese_before_technical_details`: Pass, 1/1 test.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 43/43 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 98/98 tests.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors.
- Static check: `rg -n 'Installed on|Install size|data size|running process\(es\)|Observe for now|Looks normal|Generate a migration plan' src\Css.Core\Apps\AppPresentation.cs tests\Css.Tests\ProductExperienceTests.cs` found only a negative test assertion.
- TDD red observed: `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_drawer_actions_use_beginner_friendly_chinese_labels_and_reasons` failed because drawer action labels were still English.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_drawer_actions_use_beginner_friendly_chinese_labels_and_reasons`: Pass, 1/1 test.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 42/42 tests.
- `dotnet build src\Css.App\Css.App.csproj --no-restore`: Pass, 0 warnings, 0 errors.
- GUI smoke: launched `Css.App.exe`, scanned apps, selected one app, and UIAutomation found `卸载干净点`, `迁移到 D 盘`, `清理缓存`, and `关闭自启动`; operation buttons were not invoked.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 97/97 tests.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "Migration_rollback_manifest_creation|Migration_plan_presentation_marks_rollback"`: Pass, 2/2 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter App_catalog_ignores_localized_search_placeholder`: Pass, 1/1 test.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter "App_tile_status_labels_are_localized|App_presentation_maps_software_profile"`: Pass, 2/2 tests.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore --filter ProductExperienceTests`: Pass, 41/41 tests.
- `dotnet build src\Css.App\Css.App.csproj --no-restore`: Pass, 0 warnings, 0 errors.
- GUI smoke: launched `Css.App.exe`, scanned apps, saw 130 app UI items, opened migration plan, confirmed rollback manifest generation, and captured `.omx\qa-migration-manifest-created.png`; a plan-only JSON file was written under `%LocalAppData%\OMNIX-Entropy\MigrationRollback`.
- GUI smoke: launched `Css.App.exe`, scanned apps, saw 130 app UI items, and UIAutomation read names like `火绒安全软件, Needs attention`; no generic `AppTileUi` names remained in the sampled items.
- GUI smoke: launched `Css.App.exe`, scanned apps, saw 130 app UI items, and UIAutomation read names like `火绒安全软件, 需关注`; no sampled tile names contained the old English tags.
- `dotnet test tests\Css.Tests\Css.Tests.csproj --no-restore`: Pass, 96/96 tests.
- `dotnet build ComputerSecuritySoftware.slnx --no-restore`: Pass, 0 warnings, 0 errors.

## Blockers / Not Verified

- GUIA verification for the newly localized drawer summary text was not run because the escalation request was rejected by usage limits; no workaround was attempted.
- No real C-drive cleanup was executed.
- No real official uninstaller was launched; the new gate only creates a high-risk operation descriptor when readiness is satisfied, and no handler/UI execution path is enabled.
- No real app migration was executed.
- No migration execution handler exists; `migration.execute` is only a gated descriptor model.
- Rollback manifest writing is now wired to a user-confirmed UI action, but it only writes JSON evidence and does not move app files.
- No real service, startup entry, scheduled task, or registry item was changed.
- No cloud AI is connected; Agent explanations are local rules/catalog only.
- The app-drawer migration preview and migration plan window now have build/unit coverage and one GUI screenshot/click-through for a C-drive app.
- The uninstall safety modal/preflight text now has GUIA verification; the post-uninstall residue review button has not yet had GUI screenshot/click verification.
- The new C-drive recommendation card layout has unit/build/static verification but no real-scan GUI wrapping verification yet.
- The repository still needs an initial commit before publishing to `plnoble/OMNIX-Entropy.git`.

## Next Action

Continue the app-management loop, still conservative by default:

Completed active slices: app-drawer action labels/reasons, migration window shell actions, app drawer top-level summaries/Agent advice, and migration-plan body/preflight checklist now use beginner-friendly Chinese labels. Keep using failing product tests before changing visible behavior.

1. GUI-verify the C-drive recommendation card layout after a real scan.
2. GUI-verify the post-uninstall residue review button and cancellation behavior.
3. GUI-verify the newly localized drawer summary text if a future visual pass touches the drawer.
4. Continue migration safety only in plan mode: add explicit snapshot evidence before allowing any future `migration.execute` descriptor to be requested.

## Notes for Next Agent

Do not turn "software profile" back into the main page. It is a backend engine. The foreground must answer user-understandable questions: where is this app, does it occupy C drive, does it run in the background, should I keep/clean/migrate/uninstall/observe it, and can I undo the action.

## Archived History

Entries before 2026-07-22 were moved verbatim to [current-archive-part1.md](archive/current-archive-part1.md), [current-archive-part2.md](archive/current-archive-part2.md), [current-archive-part3.md](archive/current-archive-part3.md).
