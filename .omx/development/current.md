# Current Development State

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

## Completed Slice - 2026-07-19 Disposable acceptance fixture kit

- Objective: provide deterministic, session-owned test applications and data for the ten real disposable-Windows acceptance cases so final verification never uses personal software or ad hoc paths.
- Dependencies: ordinary HKCU uninstall inventory records, HKCU Run startup evidence, app-data cache discovery, C-to-D migration planning, official-uninstall residue scanning, ownership manifests, and the existing disposable-environment attestation.
- Risks: accidentally running fixture mutation on the primary machine, overwriting an existing registry value or directory, deleting a migrated target through a redirect, shipping QA mutation tools in the user package, or making a fixture look like product acceptance evidence without an operator run.
- Impact scope: a separate `Css.AcceptanceFixtures` QA executable/project, injectable fixture planner/adapters, focused tests, a fixture publish script/manifest, protocol documentation, and development records. No changes to App/worker packages, product operation authority, trust policy, UI, or current-machine registry/files.
- Acceptance: every mutating command requires the exact disposable attestation and a canonical GUID session id; all paths/registry names are derived and bounded; provision refuses any collision and writes ownership markers before registration; uninstall removes only its exact HKCU fixture records and leaves marked residue; lock targets only its exact failure file; reset follows no redirect and deletes only roots/targets carrying a matching ownership marker; status is read-only; portable/signed product packages exclude all fixture payloads; no fixture mutating command runs during development.
- Status: Completed and verified without running a fixture mutation on the current machine. The separate QA executable supports attested `provision`, `uninstall`, `lock`, `reset`, and read-only `status`; product packages do not reference or include it.
- Last verification: fixture contracts 22/22; fixture packaging contracts 4/4; disposable protocol contracts 6/6; related product/release contracts 434/434; full regression 1026/1026; Release build 0 warnings/errors; source integrity 367 files, 0 invalid UTF-8/replacement files, and 17/17 XAML. Final package `.artifacts/OMNIX-Acceptance-Fixtures-20260719-014314` and ZIP verify five payload files with manifest SHA-256 `07C033F1B445DCF1E171ABC18E8FAC3AD9ECDA1ADFDECC0603C22FB712FA4FA3`.
- Blockers: positive provision/uninstall/lock/reset behavior remains intentionally reserved for the future disposable environment.
- Exact next action: bind this exact fixture manifest into a new disposable acceptance session only after the real signed candidate passes transfer verification.

## Completed Slice - 2026-07-19 Antivirus-updated Release launch retry

- Objective: retry the previously blocked real Release window observation after the user confirmed Huorong definitions were updated.
- Scope: launch and passive window discovery only. No scan, cleanup, uninstall, migration, startup change, UAC, security-setting interaction, or fallback UI automation.
- Status: Completed with a Warn result. Computer Use still timed out while launching `.artifacts/OMNIX-Entropy-test-20260719-003423/Css.App.exe`; one passive `list_windows` query found no OMNIX-Entropy window. A read-only process check found no `Css.App` process afterward.
- Blockers: the antivirus update did not make this unsigned package targetable through Computer Use. Do not treat the visual gate as passed or bypass the launcher/security boundary.
- Exact next action: create a valid same-signer release candidate through the approved signing transform, then retry visual and mutation acceptance only in the documented disposable Windows environment.

## Completed Slice - 2026-07-19 Disposable Windows behavioral acceptance protocol

- Objective: define a fail-closed, auditable protocol for testing the signed release candidate's real modifying workflows only on an explicitly attested disposable Windows environment.
- Dependencies: signed-candidate transfer verifier, candidate manifest/signature evidence, manual UAC interaction, fixture-only cleanup/startup/uninstall/migration scenarios, recovery evidence, and a separate immutable acceptance receipt.
- Risks: running on the primary machine, automating UAC, silently skipping cancel/rollback/restore cases, accepting screenshots without file hashes, modifying the candidate package, or confusing a prepared checklist with completed behavioral acceptance.
- Impact scope: new QA-session initializer, read-only receipt verifier, Chinese operator protocol, focused source/runtime refusal tests, and development records. No product launch, UAC automation, system mutation, certificate-store access, package mutation, or real acceptance run.
- Acceptance: session creation requires an allowlisted disposable environment, explicit `PrimaryMachine=false`, reset-checkpoint evidence, exact operator attestation, and a successfully preflighted signed candidate; all required behavioral case IDs are emitted once; the final verifier binds candidate manifest hash/signer, exact case set, timestamps, outcomes, and every evidence file's length/SHA-256; any skipped/failed/missing/tampered case refuses completion; success emits `BehavioralAcceptanceComplete=true` without writing to the candidate or receipt.
- Status: Completed and verified at protocol, parser, and fail-closed runtime level. Session creation requires an allowlisted disposable environment, explicit non-primary/disposable values, checkpoint id, exact attestation, and signed-candidate preflight before any output. The separate read-only verifier requires the exact ten-case Pass set, reset attestation, ordered timestamps, independent evidence files, and matching length/SHA-256 before emitting completion.
- Last verification: TDD red 0/6 then focused green 6/6; related release pipeline 20/20; full 1000/1000; Release build 0 warnings/errors; source integrity 361 files and 17/17 XAML; both PowerShell parsers valid and scripts ASCII-only. The current unsigned package failed preflight with child exit 1 and created no session directory. No product launch, UAC interaction, certificate access, or system mutation ran.
- Blockers: positive behavioral acceptance still requires one real same-signer signed candidate and an explicitly checkpointed disposable Windows environment.
- Exact next action: supply an approved real code-signing certificate and Windows SDK `signtool.exe`, create one signed candidate from `.artifacts/OMNIX-Entropy-test-20260719-003423`, transfer/reverify it, then follow `docs/release/disposable-windows-acceptance.zh-CN.md` manually in a disposable Windows fixture.

## Completed Slice - 2026-07-19 Release-candidate transfer verification

- Objective: independently reverify a signed candidate after it is copied to a disposable Windows environment, before any application launch or mutation acceptance begins.
- Dependencies: signed candidate manifest, payload hash inventory, Authenticode/timestamp evidence, ProductionOnly command-surface check, fixed-local-path/reparse policy, and a machine-readable preflight result.
- Risks: trusting the creation-time result after transfer, allowing unlisted extra payloads, accepting changed hashes or different signers, treating a path link as local evidence, launching the product from verification, or marking disposable acceptance complete before behavioral tests.
- Impact scope: one read-only verifier script and focused source contracts. No package creation/signing, certificate access beyond signature inspection, process launch, application runtime, system mutation, or acceptance-state write.
- Acceptance: candidate must live on a fixed local fully qualified path with no reparse ancestors/content; manifest must be `SignedReleaseCandidate`/`ProductionOnly`/same-signer eligible and still awaiting acceptance; every listed file and every actual payload file must match; App/worker signatures must be `Valid`, timestamped, same signer, and match manifest; debug worker token must be absent; success returns `CanBeginDisposableAcceptance=true` and `DisposableMachineAcceptance=false`; script has no launch/write/trust-changing authority.
- Status: Completed and verified at source/refusal level. `verify-signed-release-candidate.ps1` independently rechecks path locality, reparse state, manifest truth, exact payload coverage, every length/hash, App/worker same-signer Authenticode and timestamp evidence, manifest thumbprints, and Release worker command surface. It is read-only and returns eligibility to begin behavioral acceptance, never completed acceptance.
- Last verification: focused red 0/4 then green 4/4; related release pipeline 14/14; full 994/994; Release build 0 warnings/errors; source integrity 360 files and 17/17 XAML; PowerShell parser valid. Running it against the current unsigned package failed on candidate-manifest truth as expected. No application/process/mutation/certificate action ran.
- Blockers: no signed candidate exists yet, so positive verifier execution will remain pending; parser and source/failure contracts can still be completed.
- Exact next action: add a bounded disposable-environment behavioral acceptance protocol that records package preflight evidence, explicit manual UAC cancel/accept observations, fixture-only cleanup/startup/uninstall/migration/restore results, and a final fail-closed verdict; do not make it runnable against the primary machine by default.

## Completed Slice - 2026-07-19 Trusted signed-release transformation

- Objective: make an already verified portable package transformable into a same-signer release candidate using an explicitly supplied, preinstalled code-signing certificate, without creating/importing certificates or weakening runtime trust.
- Dependencies: portable test package manifest, App/worker Authenticode trust policy, Windows SDK `signtool.exe`, CurrentUser certificate store, RFC3161 timestamp service, and package hash/manifest generation.
- Risks: mutating the source package, signing only one executable, accepting a certificate without private key/code-signing EKU, stale hashes, untrusted signatures, password leakage, hidden certificate import, or claiming production readiness before disposable-machine acceptance.
- Impact scope: one new release-transform script, one signed-release readme template, static safety/contract tests, and development records. No product runtime, trust evaluator, operation pipeline, certificate store, system settings, or actual signing in this environment.
- Acceptance: input must be a verified package under `.artifacts`; output must be a new sibling directory; certificate is selected only by normalized thumbprint from `Cert:\CurrentUser\My`; certificate must be valid, private-key-backed, and code-signing capable; App and worker must be signed/timestamped and reverified as `Valid` with the same requested thumbprint; manifest hashes are regenerated after signing; failure cannot produce `EligibleForDisposableMachineAcceptance`; no certificate import/generation or trust relaxation exists.
- Status: Completed and verified at the source/fail-closed level. A verified portable package can now be copied into a new release-candidate directory, signed with one explicitly selected CurrentUser code-signing certificate, timestamped, reverified, rehashed, and marked only as awaiting disposable-machine acceptance. The source package is never modified.
- Last verification: focused red 0/4 then green 4/4; related portable/signed/Release-command tests 10/10; full 990/990; Release build 0 warnings/errors; source integrity 359 files and 17/17 XAML; PowerShell parser valid. A missing-`signtool` runtime refusal stopped before creating the requested output directory. No certificate store was enumerated or changed and no signing ran.
- Blockers: no real code-signing certificate or `signtool` acceptance is assumed; this slice builds and statically verifies the fail-closed release path only.
- Exact next action: with an explicitly provided real code-signing certificate and Windows SDK `signtool.exe`, generate one signed candidate from `.artifacts/OMNIX-Entropy-test-20260719-003423`, verify the manifest, then perform mutation/cancel/rollback acceptance only in a disposable Windows environment.

## Completed Slice - 2026-07-19 Execution result return handoff

- Objective: after a migration or official-uninstall execution attempt, return directly from the result to Application Management so the existing current-state rescan runs without an extra close on the locked plan window.
- Dependencies: migration/uninstall result presenters and windows, plan-window attempt state, MainWindow post-attempt inventory/closure/residue synchronization, and existing production safety gates.
- Risks: closing a plan before the result is acknowledged, refreshing after a mere preview, losing typed residue actions, or implying that a rescan retries the operation.
- Impact scope: result-button wording and plan-window return flow after an actual execution attempt. No worker request, signature trust, consent, operation handler, rollback, quarantine, registry, service, task, startup, or file mutation logic.
- Acceptance: result buttons state that OMNIX will return and recheck; acknowledging any attempted migration/uninstall result closes the exhausted plan; MainWindow's existing `ProductionExecutionAttempted` branch then refreshes current state; preview-only close remains unchanged; uninstall post-scan typed actions retain their existing routing.
- Status: Completed and verified. Migration and nested official-uninstall result acknowledgement now closes the exhausted plan so MainWindow immediately enters its existing post-attempt rescan. Button copy says `返回并重新检查` only in contexts that genuinely return to Application Management; the standalone Debug connection result retains `我知道了`.
- Last verification: focused red 0/3 then green 3/3; related migration/uninstall/product 207/207; full 986/986; Release build 0 warnings/errors; source integrity 358 files and 17/17 XAML; final package `.artifacts/OMNIX-Entropy-test-20260719-003423`, ProductionOnly and mutation blocked because App/worker remain unsigned.
- Blockers: positive runtime execution still requires valid same-signer packaging and a disposable fixture; source/contracts can be completed without weakening that gate.
- Exact next action: continue auditing remaining result/confirmation surfaces for a safe typed recovery action or stale-state dead end; valid same-signer disposable positive mutation acceptance remains a separate release gate.

## Completed Slice - 2026-07-19 Cache and startup decision outcomes

- Objective: make application cache cleanup and startup management answer one beginner decision before exposing paths, registry locators, operation descriptors, or confirmation mechanics.
- Dependencies: isolated software fixture, app drawer action host, cache operation planner/confirmation, startup-entry review/control pipeline, production safety policy, and real Release evidence.
- Risks: accidentally confirming quarantine or startup mutation, presenting fixture paths as primary advice, hiding why no action is available, or weakening confirmation/evidence authority while simplifying presentation.
- Impact scope: cache/startup preview, refusal, and cancel information hierarchy plus focused contracts and isolated Release evidence. No quarantine handler, startup registry store, operation pipeline, confirmation authority, signer policy, or production mutation.
- Acceptance: cache and startup entries first explain what is safe, what would change, whether it can be undone, and the next step; raw paths/registry evidence are secondary; cancel leaves the fixture and isolated stores unchanged; no production mutation runs.
- Status: Completed and verified without a code change. The cache refusal leads with a path-free Agent decision and exposes no execution action. The name-only startup case leads with a refusal/fallback decision and offers only the bounded Windows Startup Apps handoff; it does not offer local mutation.
- Last verification: Computer Use exercised both entries in the isolated Release fixture from package `.artifacts/OMNIX-Entropy-test-20260719-000736`. Cache showed `当前缓存候选没有通过本地安全校验` and no primary action. Startup showed `当前只有名称级线索，不能确定要关闭哪一个启动项` and only `在 Windows 中查看`. Neither primary handoff nor any mutation was invoked; the fixture window was closed.
- Blockers: none.
- Exact next action: audit the remaining execution-result windows for a typed, current-state next action after completion or failure; keep valid same-signer disposable real mutation acceptance as a separate release gate.

## Completed Slice - 2026-07-19 Uninstall decision hierarchy

- Objective: make `卸载干净点` and `卸载后检查残留` explain one safe beginner decision before exposing commands, paths, evidence archives, or residue mechanics.
- Dependencies: isolated software fixture, app drawer action host, uninstall plan presenter/window, production-readiness gate, post-uninstall inventory refresh, residue review presenter, and real Release evidence.
- Risks: accidentally launching an official uninstaller, treating an unsigned package as executable, exposing fixture paths as primary guidance, implying that residue cleanup is automatic, or weakening final consent/quarantine authority.
- Impact scope: uninstall preview and residue-result information hierarchy, unavailable-action visibility, focused presentation contracts, and isolated Release visual evidence. No official-uninstall launcher/coordinator, elevated worker, residue scanner/planner, quarantine pipeline, signature policy, or mutation authority.
- Acceptance: an isolated ordinary-app fixture opens a human-readable uninstall preview; the first view says whether uninstall can proceed, what will happen, how residue review works, and whether current packaging blocks execution; raw command/path/checklist evidence is secondary; unsigned execution controls are absent; the still-installed fixture's residue check gives a clear refusal/conclusion without offering cleanup; no production mutation runs.
- Status: Completed and verified. The uninstall preview now leads with a path-free Agent decision; preparation, full workflow, and technical evidence are collapsed. Unsigned packages hide preparation controls entirely. The still-installed residue check already gave a correct fail-closed conclusion and remains unchanged.
- Last verification: focused 3/3; related uninstall/readiness/product 397/397; full 983/983; Release build 0 warnings/errors; 357 strict UTF-8 C#/XAML files; 17/17 XAML parse; final package `.artifacts/OMNIX-Entropy-test-20260719-000736`. Computer Use captured the final uninstall first view and still-installed residue refusal; unavailable preparation controls and residue-mutation actions were absent. No mutation ran.
- Blockers: none.
- Exact next action: continue the visible Application workflow audit with cache-cleanup and startup-management decision outcomes; keep valid same-signer disposable real mutation acceptance as a separate release gate.

## Completed Slice - 2026-07-18 Migration plan decision hierarchy

- Objective: verify that one ordinary application's drawer leads with understandable guidance and make its migration follow-up explain the decision before exposing technical evidence.
- Dependencies: process-scoped software fixture, automatic Application Management inventory load, app tile/drawer presenters, action preview host, residue availability, migration closure state, and real Release evidence.
- Readiness matrix: icon catalog Pass; drawer summary Pass; action previews Pass; residue entry Pass; migration closure engine Pass; beginner migration decision hierarchy Pass; real Release drawer/preview evidence Pass.
- Risks: invoking an official uninstaller or migration worker during visual review, exposing fixture paths as user evidence, hiding legitimate disabled reasons, or redesigning operation authority while auditing presentation.
- Impact scope: migration preview decision presenter, information hierarchy, unsigned-action visibility, focused contracts, and isolated Release evidence. No inventory scanner, uninstall coordinator, residue pipeline, migration planner/worker, closure monitor, signature policy, or mutation authority.
- Acceptance: an isolated ordinary-app fixture auto-loads into the icon grid; selecting it shows human summaries and one Agent conclusion before actions; migration preview answers whether it can move, where, what happens next, whether it can be undone, and whether space is sufficient; raw paths/bytes/checklists are collapsed under technical details; unsigned execution buttons are absent; no production mutation runs.
- Status: Completed and verified. The drawer was already beginner-readable; the migration preview now defaults to one Agent decision summary and folds raw evidence. Unsigned packages show no preparation/request buttons.
- Last verification: focused 3/3; related migration/readiness/product 254/254; full 980/980; Release build 0 warnings/errors; 356 strict UTF-8 C#/XAML files; 17/17 XAML parse; final package `.artifacts/OMNIX-Entropy-test-20260718-224949`. Computer Use captured the fixture drawer and final migration first view; technical descendants and execution buttons were absent until requested/eligible. No mutation ran.
- Blockers: none.
- Exact next action: continue the Application drawer audit with the uninstall preview and post-uninstall residue conclusion, staying cancel/read-only and preserving real official-uninstall acceptance for a valid same-signer disposable fixture.

## Completed Slice - 2026-07-18 Undo Center first-view hierarchy

- Objective: make the Undo Center immediately explain whether any restorable action or quarantine-retention work exists, without reserving large empty list surfaces or exposing an unavailable permanent-purge action.
- Dependencies: automatic timeline loading, quarantine retention presenter, timeline item presenter, restore/purge safety pipelines, isolated Release storage, and real-window evidence.
- Readiness matrix: navigation Pass; automatic read-only loading Pass; restore pipeline Pass; purge pipeline Pass; empty-state hierarchy Pass; real Release first-view evidence Pass.
- Risks: hiding genuine restorable history or expiry candidates, implying that an empty isolated store proves the user's real history is empty, weakening restore/purge confirmation, or invoking any mutation during visual acceptance.
- Impact scope: Undo Center initial/loading/empty/populated visibility and beginner copy, focused static contracts, and isolated Release visual acceptance. No timeline store, quarantine retention rules, restore/purge handlers, confirmation windows, or mutation authority.
- Acceptance: loading and empty states use stable compact conclusion text; empty timeline/candidate lists do not reserve blank panels; cleanup review appears only for current candidates; populated states reveal the existing lists and controls; a real Release screenshot with isolated empty storage shows the conclusion in the first visible area and no mutation is invoked.
- Status: Completed and verified. Loading/unavailable/empty history is now a compact stable conclusion; current entries reveal the existing timeline. Retention candidates and permanent-cleanup review appear only from current candidates.
- Last verification: TDD red observed; focused 2/2; related timeline/quarantine/product 224/224; full 977/977; Release build 0 warnings/errors after a `NuGetAudit=false` restore; 355 strict UTF-8 C#/XAML files; 17/17 XAML parse; final package `.artifacts/OMNIX-Entropy-test-20260718-223259`. Computer Use captured the isolated empty first view and proved candidate list, cleanup button, timeline list, fake row, technical expander, and restore button are absent. No mutation ran.
- Blockers: none.
- Exact next action: continue the visible V1 audit in the Application drawer, prioritizing uninstall-residue and migration-closure conclusions while preserving signed/disposable mutation acceptance as a separate gate.

## Completed Slice - 2026-07-18 Installation Control first-view hierarchy

- Objective: make the installation page start with one clear trusted-package workflow and keep remembered-rule/change-report surfaces from looking like empty required steps.
- Dependencies: installer picker/analyzer, capability panel, remembered routing presenter, automatic post-install monitoring, advanced manual comparison, install-diff presenter, and real Release evidence.
- Readiness matrix: file picker Pass; read-only analysis Pass; target-path recommendation Pass; trusted execution gate Pass; first-view hierarchy Pass; empty-rule truth Pass; empty-diff truth Pass.
- Risks: hiding remembered rules or valid post-install reports, implying that file selection runs an installer, moving advanced diagnostics into the primary workflow, or weakening preparation/signature/confirmation gates.
- Impact scope: Installation Control default/result visibility and beginner copy, focused contracts, and Release visual acceptance. No installer launch, signature policy, route engine, process observation, operation pipeline, or system mutation.
- Acceptance: first view asks for one downloaded installer and states analysis is read-only; empty remembered-rule and install-diff lists do not reserve blank panels; capability/rule/report surfaces appear only when current data exists; advanced manual comparison stays secondary; real Release screenshot makes selection, analysis, and execution boundaries distinguishable.
- Status: Completed and verified. Empty routing memory is now an empty collection plus state summary; rule controls and report controls appear only from current rules or a valid report.
- Last verification: focused/related 217/217; full 975/975; build 0 warnings/errors; 354 strict UTF-8 C#/XAML files; 17/17 XAML parse; final package `.artifacts/OMNIX-Entropy-test-20260718-221512`; Computer Use captured the final first view and confirmed empty rule/report controls are absent. No file picker, installer analysis, installer launch, settings, or mutation action ran.
- Blockers: none.
- Exact next action: continue the next visible V1 audit with Undo Center/Timeline first-view hierarchy; real installer execution remains a separate trusted-package/disposable-fixture acceptance item.

## Completed Slice - 2026-07-18 C-drive first-view hierarchy

- Objective: make the C-drive page truthful and immediately understandable before its first scan, without presenting empty technical list surfaces as results.
- Dependencies: `CDrivePage`, root-cause/growth/personal-storage/recommendation collections, existing Start Scan header action, result binding in `ApplyScanSession`, and real Release evidence.
- Readiness matrix: navigation Pass; automatic system drive Pass; scan entry Pass; populated-result logic Pass; pre-scan hierarchy Pass; empty-list truth Pass; safe next action Pass.
- Risks: hiding real results after a completed scan, claiming the drive is clean before scanning, moving technical details into the first view, or changing cleanup authority while fixing presentation.
- Impact scope: initial/result visibility and beginner copy on the C-drive page, focused contracts, and Release visual acceptance. No scanner, recommendation classification, confirmation window, quarantine, operation pipeline, or system mutation.
- Acceptance: the pre-scan view names the automatic system drive and says a read-only scan is required; empty root-cause, growth, personal-storage, and recommendation lists do not reserve misleading blank panels; completed nonempty results reveal their lists; the safe next step remains the existing Start Scan action; real Release screenshot has no unexplained empty result boxes.
- Status: Completed and verified. Pre-scan, scanning, cancelled, failed, empty-complete, and populated-complete states now expose only truthful current surfaces; no empty result list or premature action preview occupies the first view.
- Last verification: focused 2/2; related 211/211 and final 171/171; full 972/972; build 0 warnings/errors; 353 strict UTF-8 C#/XAML files; 17/17 XAML parse; final package `.artifacts/OMNIX-Entropy-test-20260718-220108`; Computer Use captured the final C-drive first view and confirmed state AutomationIds while result lists/action controls were absent. No scan or modifying action ran.
- Blockers: none.
- Exact next action: continue the visible V1 workflow audit with Installation Control, keeping trusted launch and real mutation acceptance separate from read-only first-view UX.

## Completed Slice - 2026-07-18 Agent page information hierarchy

- Objective: make the Agent first view focused and beginner-readable by separating consultation/recommendations from settings/skills/tools.
- Dependencies: `AgentPage` root, both existing ScrollViewers, current AutomationIds/click handlers, `ShowPage`, and real Release navigation evidence.
- Readiness matrix: Agent navigation Pass; current-inventory advice Pass; controls/skills/tools Pass; first-view density Pass; explicit view separation Pass; default consultation focus Pass.
- Risks: losing existing named controls or handlers, hiding typed next actions, resetting navigation context, rendering both tabs together, or adding page-routing code where native tab selection is sufficient.
- Impact scope: Agent XAML container only, focused source tests, and Release visual/tab acceptance. No Agent intent/presenter, inventory scan, allowlisted settings/tools, operation pipeline, or system mutation.
- Acceptance: root is a stable TabControl; default first tab is `咨询与建议` and contains the conversation/recommendation ScrollViewer; second tab is `能力与工具` and contains settings/skills/tools; both tab headers have stable AutomationIds; existing descendant AutomationIds and click hooks remain; real first view shows only consultation and one click reveals tools.
- Status: Completed and verified. Agent now defaults to a full-width consultation/recommendation tab; settings, skills, and system tools live in a separate explicit tab.
- Last verification: focused/related 216/216; full regression 970/970; build 0 warnings/errors; 352 strict UTF-8 C#/XAML files; 17/17 XAML parse; final package `.artifacts/OMNIX-Entropy-test-20260718-214320`; Computer Use captured both real Release tabs, confirmed stable UIAutomation identities, and closed the window. No modifying or external tool action ran.
- Blockers: none. Computer Use can capture and navigate the Release window.
- Exact next action: continue the visible V1 workflow audit, prioritizing beginner comprehension and current evidence; keep real mutation behind valid same-signer packaging and disposable-machine acceptance.

## Completed Slice - 2026-07-18 Home key-findings empty state

- Objective: remove the large blank bordered list from the beginner's first Home view and show an honest compact state until key findings exist.
- Dependencies: Home right-column XAML, `RefreshHealthSummaryFromBase`, `HealthCheckSummary.KeyFindings`, stable text/list AutomationIds, and current Release GUI acceptance.
- Readiness matrix: real Release startup Pass; first-view screenshot Pass; Agent card Pass; empty key-findings truth Pass; blank-space UX Pass; result-state switch Pass; Apps/Agent navigation Pass.
- Risks: hiding real findings after a scan, presenting “no issue” before any scan, moving the empty state below the list, or using a decorative container as the only automation proof.
- Impact scope: one first-view TextBlock, default ListBox visibility, summary-driven visibility/copy, focused static tests, and a Release screenshot. No scanner, recommendation, Agent authority, operation pipeline, or system mutation.
- Acceptance: before scanning, a stable visible text says findings will appear after a check and the empty ListBox is collapsed; after a summary, nonempty findings show the list and hide the text; a valid empty summary says no priority item; the status text precedes the list in XAML; real Release first view no longer shows the blank inner rectangle.
- Status: Completed and verified. The first view now shows one compact stable empty-state line; the findings list stays collapsed until real findings exist, and valid empty summaries use distinct copy.
- Last verification: focused/related 218/218; full regression 968/968; build 0 warnings/errors; 351 strict UTF-8 C#/XAML files; 17/17 XAML parse; final package `.artifacts/OMNIX-Entropy-test-20260718-212514`; Computer Use captured the corrected Home first view and clicked Apps and AI Agent successfully. Apps rendered 391 real profiles with icons/drawer; Agent rendered current C-drive/background guidance. All test windows closed.
- Blockers: none. Computer Use now captures the shell-launched Release window.
- Exact next action: continue the requirement-by-requirement V1 audit, prioritizing a visible entry whose evidence or safe next action remains incomplete; real mutation stays behind signed disposable acceptance.

## Completed Slice - 2026-07-18 Release debug-command surface removal

- Objective: ensure the privileged Release worker cannot be launched in the test-only fake-worker mode while preserving the Debug smoke harness.
- Dependencies: `Css.Elevated/Program.cs`, `OfficialUninstallFakeWorker.cs`, Release MSBuild item selection, portable package verification, and existing worker lifecycle tests.
- Readiness matrix: App QA arguments Debug-only Pass; production uninstall/migration modes Pass; fake elevated mode Debug-only Pass; package command-surface verification Pass.
- Risks: breaking Debug lifecycle smokes, accidentally removing production worker modes, trusting only source guards without inspecting the actual Release binary, or broad refactoring of the IPC test transport.
- Impact scope: conditional compilation/item selection for one fake worker entry and one read-only Release binary token check in the package script. No production pipeline, named-pipe authentication, signer policy, process/service/task/registry/file mutation, or UI change.
- Acceptance: Debug still compiles and its fake lifecycle tests pass; Release excludes `OfficialUninstallFakeWorker.cs`; the fake command branch is explicitly Debug-guarded; package creation refuses a Release worker DLL containing the fake command in UTF-8 or UTF-16; production uninstall and migration modes remain present; final package binary scan is clean.
- Status: Completed and verified. Release excludes the fake worker source, Program guards its branch with `#if DEBUG`, and packaging scans both UTF-8 and UTF-16 metadata before writing a manifest/ZIP.
- Last verification: focused worker/release contracts 22/22; full regression 966/966; build 0 warnings/errors; 350 strict UTF-8 C#/XAML files; 17/17 XAML parse; final package `.artifacts/OMNIX-Entropy-test-20260718-210944`; worker fake token absent in both encodings, production uninstall/migration tokens present, manifest `ReleaseCommandSurface=ProductionOnly`.
- Blockers: none.
- Exact next action: continue the next visible V1 evidence/action audit; use the latest package for read-only UX acceptance and keep signed/disposable mutation acceptance separate.

## Completed Slice - 2026-07-18 reproducible portable test package

- Objective: create a reproducible beginner-testable OMNIX package while keeping unsigned production mutation fail-closed.
- Dependencies: `dotnet publish` for `Css.App`, packaged sibling `Css.Elevated`, `rules.scan.json`, Authenticode inspection, SHA-256 manifest, and current package trust policy.
- Readiness matrix: Debug startup Pass; source/build Pass; portable publish Pass; required-file/hash/ZIP verification Pass; signature truth Pass; signed production readiness Fail (`NotSigned` App/Worker).
- Risks: silently presenting an unsigned package as production-ready, signing/importing certificates, deleting arbitrary output folders, publishing an App without its worker/rules, or hiding framework/runtime requirements.
- Impact scope: one read-only/build-output publishing script, one static contract test, package documentation, and records. No signing, certificate-store change, trust-policy relaxation, installer launch, operation pipeline, process/service/task/registry/file cleanup, or real mutation.
- Acceptance: default output is a new timestamped folder under `.artifacts`; existing output is refused rather than deleted; publish must include App, worker, and rules; manifest records hashes, lengths, Authenticode statuses, signer match, runtime mode, and explicit mutation readiness; zip and beginner test note are produced; unsigned output remains clearly blocked for uninstall/migration execution.
- Status: Completed and verified. The latest framework-dependent package and ZIP were generated at `.artifacts/OMNIX-Entropy-test-20260718-212514` with Release-only command-surface verification.
- Last verification: package script contracts remain green within full regression 966/966; build 0 warnings/errors; final manifest has 110 hashed files; ZIP has 139 entries and all required payloads; App/Worker both `NotSigned`, `ValidSameSigner=false`, mutation readiness `BlockedUntilValidSameSignerPackage`.
- Blockers: none for read-only beginner testing. Valid same-signer release acceptance still requires an external signing identity and a disposable machine; whole-window Computer Use interaction remains Warn.
- Exact next action: use the final package for read-only UX acceptance, continue auditing visible V1 workflows, and keep real uninstall/migration/system-mutation acceptance deferred until one valid same-signer package is available on a disposable machine.

## Verification Update - 2026-07-18 isolated GUI lifecycle

- An isolated Debug instance using `.omx/qa-current-window` remained running after five seconds and exposed a real `OMNIX-Entropy` window handle/title; the probe process was then stopped.
- A second isolated instance was uniquely returned by Computer Use as `process:D:\Agent\Project\OMNIX-Entropy\src\Css.App\bin\Debug\net8.0-windows\Css.App.exe` with title `OMNIX-Entropy`.
- Computer Use state capture failed at its app-approval step with `Computer Use app approval timed out`; no click or screenshot occurred. The isolated process was explicitly stopped and no test instance remains.
- Conclusion: application startup lifecycle Pass; whole-window interactive/screenshot gate remains Warn because Computer Use approval did not complete.

## Active Slice - 2026-07-18 C-drive app handoff truth and reuse

- Objective: make the C-drive root-cause `占 C 盘应用` action report the filtered current result, not whether any software exists on the computer.
- Dependencies: `OpenCDriveRootCauseAction_Click`, bounded `OpenAgentAppCatalogFilterAsync(AppCatalogFilter.CDrive)`, inventory gate, catalog refresh, and existing root-cause AutomationId/action model.
- Readiness matrix: action identity Pass; inventory load Pass; CDrive filter Pass; empty-state truth Fail; shared handoff reuse Fail.
- Risks: changing root-cause action authority, weakening its action/card validation, opening an operation plan, or leaving stale duplicate load/filter/status code.
- Impact scope: one root-cause switch branch, focused source contract updates, and records. No root-cause classification, inventory scanner, app drawer policy, recommendation execution, process/service/task/registry/file mutation, or elevation change.
- Acceptance: the branch delegates once to the bounded CDrive catalog handoff; the shared handoff owns load-before-refresh and filtered item-count status; root action validation and all other switch actions remain unchanged; no operation opens.
- Status: Completed and verified. The root-cause branch now delegates once to the bounded CDrive handoff, whose filtered item count owns truthful empty/populated copy.
- Last verification: focused automatic inventory 5/5; related C-drive/Agent/product 282/282; full regression 960/960; solution build 0 warnings/errors; 348 strict UTF-8 C#/XAML files; 17/17 XAML parse.
- Blockers: none.
- Exact next action: continue the V1 audit, emphasizing result windows/preview controls that do not expose a typed next action or current-state refresh; keep signed/disposable mutation acceptance separate.

## Active Slice - 2026-07-18 Home migration-closure catalog handoff

- Objective: preserve C-drive application context when a homepage migration-closure finding has no safe exact app target.
- Dependencies: `HomeAgentResponseViewModel`, `HomeAgentResponsePresenter.CreateNavigation`, `HomeAgentResponseNavigate_Click`, existing `AppCatalogFilter.CDrive` handoff, and current inventory gate.
- Readiness matrix: exact-app re-resolution Pass; personal-storage handoff Pass; C-drive detail handoff Pass; aggregate migration-closure Apps handoff broad/Fail; operation authority absent Pass.
- Risks: overriding a valid exact app target, treating every C-drive app as a closure problem, accepting filter/page mismatch, or opening a migration plan automatically.
- Impact scope: one optional typed filter in the home Agent response, one aggregate assignment, one bounded handler branch, focused tests, and records. No finding generation, migration closure evidence, plan, pipeline, process/service/task/registry/file mutation, elevation, or AI execution authority change.
- Acceptance: target-named findings still re-resolve the exact app; targetless migration-closure findings carry `CDrive`; other C-drive/personal-storage responses remain unchanged; handler validates Applications/filter consistency and only opens the filtered current catalog.
- Status: Completed and verified. Targetless migration-closure findings now carry typed `CDrive` context; exact app targets remain mutually exclusive and re-resolve current inventory.
- Last verification: focused 5/5; related home/health/migration/product 199/199; full regression 960/960; solution build 0 warnings/errors; 348 strict UTF-8 C#/XAML files; 17/17 XAML parse; bounded handler authority test passes.
- Blockers: none.
- Exact next action: continue the V1 visible-entry audit for another preview that cannot reach its intended safe review or current-state result; keep signed/disposable mutation acceptance separate.

## Active Slice - 2026-07-18 Agent next-step typed application handoff

- Objective: preserve `Resident` or `CDrive` context when the Agent page's persistent next-step buttons open Application Management.
- Dependencies: `AgentNextActionViewModel`, `AgentNextStepPresenter.BuildNavigationActions`, existing bounded `OpenAgentAppCatalogFilterAsync`, next-step XAML item template, and application inventory gate.
- Readiness matrix: recommendation labels Pass; navigation-only authority Pass; page handoff Pass; application filter context Fail; stable per-action AutomationId Fail.
- Risks: duplicate AutomationIds for two Apps actions, accepting a filter on a non-Apps page, losing navigation-only checks, or opening an operation review automatically.
- Impact scope: typed next-step view model context, presenter assignments, one async navigation handler, stable action identity, focused tests, and records. No recommendation ranking, operation plan, process/service/task/registry/file mutation, elevation, or AI execution authority change.
- Acceptance: resident actions carry `Resident`; C-drive application actions carry `CDrive`; empty/general navigation remains unfiltered; each button binds the whole typed action and a stable unique AutomationId; handler validates navigation-only/page/filter consistency and reuses the bounded catalog handoff without opening plans.
- Status: Completed and verified with visual Warn. Agent next-step buttons now preserve typed `Resident`/`CDrive` context, bind the whole action, and expose unique stable AutomationIds before using the bounded catalog handoff.
- Last verification: focused 2/2; related Agent/product 275/275; full regression 959/959; solution build 0 warnings/errors; 348 strict UTF-8 C#/XAML files; 17/17 XAML parse. Computer Use `launch_app` timed out and the follow-up window list contained no OMNIX target.
- Blockers: none.
- Exact next action: continue the V1 visible-entry audit for another action that still loses evidence/context or stops at a non-actionable preview; keep signed/disposable mutation acceptance separate.

## Active Slice - 2026-07-18 Agent migration/uninstall catalog handoff

- Objective: preserve aggregate migration and uninstall context when Agent answers hand off to Application Management, instead of opening the full application grid.
- Dependencies: aggregate `AgentConversationReply`, existing `AppCatalogFilter.CDrive` / `Uninstallable`, software inventory load gate, application catalog refresh, and current drawer review policies.
- Readiness matrix: aggregate evidence separation Pass; broad Apps navigation Fail; typed filter model Pass; filter whitelist Partial; per-application review/confirmation Pass.
- Risks: presenting every C-drive app as migratable, treating an uninstallable filter as uninstall consent, accepting arbitrary Agent filters, retaining stale search text, or opening a plan automatically.
- Impact scope: two typed Agent reply assignments, one bounded read-only catalog handoff helper, focused tests, and protocol records. No migration/uninstall plan generation, operation pipeline, process, service, task, registry, file mutation, elevation, or AI execution authority change.
- Acceptance: migration opens `CDrive`; uninstall opens `Uninstallable`; startup remains `Resident`; MainWindow accepts only those three filters, clears search, joins current inventory loading, refreshes the grid, and reports filter-specific beginner copy; no plan or operation is opened automatically.
- Status: Completed and verified. Aggregate migration, uninstall, and startup answers now preserve bounded `CDrive`, `Uninstallable`, and `Resident` application-catalog context without opening an operation review.
- Last verification: focused 8/8; related Agent/application/product 279/279; full regression 957/957; solution build 0 warnings/errors; 347 strict UTF-8 C#/XAML files; 17/17 XAML parse; migration/uninstall filter assignments exactly one each.
- Blockers: none.
- Exact next action: audit the remaining V1 visible entries for a preview or navigation action that still lacks a current-state handoff; keep signed/disposable mutation acceptance separate.

## Active Slice - 2026-07-16 Agent background context handoff

- Objective: preserve application context when Agent background/startup findings hand off to Application Management, instead of opening an unfiltered grid and asking the beginner to identify the same apps again.
- Dependencies: `AgentBackgroundReviewItemViewModel`, aggregate `AgentConversationReply`, existing `AppCatalogFilter.Resident`, `ResolveAndOpenAppTargetAsync`, software inventory gate, Agent XAML panels, and application catalog refresh.
- Readiness matrix: resident ownership catalog Pass; safe per-item names Pass partially; per-item application action Fail; aggregate Resident context Fail; application target resolver Pass; startup confirmation boundary Pass.
- Risks: treating resident as safe to close, opening startup control instead of details, leaking raw component identities, accepting an arbitrary filter from Agent output, retaining stale search text, or navigating before inventory is available.
- Impact scope: typed Agent app-filter handoff, one per-item navigation button, one Resident-filter navigation helper, focused tests, and records. No startup control policy, confirmation, registry write, service/task/process action, operation pipeline, or AI execution authority change.
- Acceptance: each review item exposes a safe app name and details-only action; aggregate startup reply carries `Resident`; MainWindow only accepts the whitelisted Resident filter, clears old search, ensures inventory, refreshes the grid, and reports read-only navigation; no path/component identity or mutation authority enters the new handlers.
- Status: Completed and verified. Each background item has a details-only app handoff; aggregate startup/background replies carry the typed Resident filter; MainWindow clears stale search, joins inventory loading, and shows only resident applications without opening a control plan.
- Last verification: focused background/startup handoff 15/15; related Agent/application/product 251/251; full regression 956/956; solution build 0 warnings/errors; 347 strict UTF-8 C#/XAML files; all XAML parses; both new handlers mutation-authority hits 0; typed Resident reply assignments 2; button hook/AutomationId/filter assignment each 1.
- Blockers: none.
- Exact next action: continue the remaining visible-entry audit, prioritizing aggregate migration/uninstall answers that still open broad application catalogs and any preview action that lacks a hardened current-state handoff. Keep signed/disposable mutation acceptance separate.

## Active Slice - 2026-07-16 Persisted digest to current C-drive evidence

- Objective: prevent a restart-loaded health digest from navigating to an empty/stale C-drive page while claiming that current evidence was opened.
- Dependencies: digest history loading/presentation, `OpenHealthDigestEvidenceButton`, `ReadOnlyEvidenceLoadGate`, `RunHealthScanCoreAsync`, and current-process `_lastHealthSummary` state.
- Readiness matrix: persisted path-free digest Pass; internal navigation Pass; current evidence hydration Fail; in-flight deduplication Pass; scan failure truth Pass in scanner; navigation failure truth Fail.
- Risks: presenting history as current evidence, launching duplicate full scans, overwriting scan failure copy with a success claim, leaving the button reentrant during loading, or adding mutation to a read-only navigation path.
- Impact scope: digest button copy/state, one async navigation handler, focused source/UX tests, and records. No digest schema/store, scanner scope, recommendation execution, operation pipeline, file mutation, registry, service, task, elevation, or Agent authority change.
- Acceptance: button says it will update evidence; click disables itself, shows the C-drive page and starts/joins the existing read-only scan gate; success requires both completed gate state and a current in-memory summary; failure/cancel never claims evidence is current; button state is restored from digest availability; handler has no mutation authority.
- Status: Completed and verified. Restart-loaded history now offers a fresh-current-evidence action, starts/joins one read-only scan, and only claims success after both the gate and current in-memory summary are present.
- Last verification: focused digest/gate 16/16; related health/home/Agent/product 195/195; full regression 954/954; solution build 0 warnings/errors; 346 strict UTF-8 C#/XAML files; all XAML parses; handler mutation-authority hits 0; one Ensure call, zero forced Refresh calls; success copy follows readiness check.
- Blockers: none.
- Exact next action: continue the visible V1 audit with Agent background/startup findings. Preserve each identified application's context when handing off to Application Management instead of opening an unfiltered grid.

## Active Slice - 2026-07-16 Personal-file read-only location inspection

- Objective: let a beginner act on a large/possible-duplicate finding by seeing the exact captured locations and opening one in Windows Explorer, without exposing deletion or treating inspection as cleanup consent.
- Dependencies: `PersonalStorageFinding` evidence paths, `PersonalStorageFindingPresenter`, the C-drive candidate list, a presentation-only location window, a bounded Explorer launcher, and current-scan evidence membership.
- Readiness matrix: bounded scan paths Pass; path-free default rows Pass; exact-location detail Fail; per-finding inspect action Fail; current evidence revalidation Fail; deletion/move authority absent Pass.
- Risks: opening an arbitrary injected path, executing the file instead of selecting it, using stale evidence after a new scan, showing a wall of paths in the default list, or placing process-launch authority in the detail window.
- Impact scope: personal-storage view model, one row action, one read-only detail window, one Explorer launcher/policy, current evidence cache, focused tests, visual proof, and records. No file content read, hash comparison, delete, move, quarantine, recommendation execution, registry, service, task, elevation, or AI authority change.
- Acceptance: default findings stay path-free; inspect opens a concise list of only the selected finding's captured paths; the window returns intent only; MainWindow accepts only a path still present in the current scan evidence; launcher revalidates a fully-qualified local existing file and uses the fixed Windows Explorer executable with an argument list; failures are path-free; no cleanup control exists.
- Status: Completed and verified. Default rows remain path-free; explicit inspection lists only captured locations; the window returns intent only; MainWindow and the launcher revalidate current evidence before fixed Explorer selection.
- Last verification: focused personal storage/inspection 10/10; related health/C-drive/product 191/191; full regression 953/953; solution build 0 warnings/errors; 345 strict UTF-8 C#/XAML files; all XAML parses; window/handler mutation-authority hits 0. First-view render passed and was inspected at `.omx/qa-personal-storage-inspection.png`.
- Blockers: none.
- Exact next action: fix persisted health-digest navigation so a restart-era summary cannot send the beginner to an empty or stale C-drive evidence page; require a fresh read-only health session before presenting current evidence.

## Active Slice - 2026-07-16 Actionable uninstall post-scan result

- Objective: turn the uninstall post-scan model's visible `重新扫描` / `查看残留清单` advice into typed, safe commands instead of non-clickable text.
- Dependencies: `OfficialUninstallPostScanViewModel/Presenter`, `UninstallPostScanResultWindow`, `UninstallPlanWindow` production result flow, MainWindow post-plan inventory refresh, and existing residue review/quarantine confirmation pipeline.
- Readiness matrix: post-scan evidence Pass; beginner conclusion Pass; action label Pass; clickable typed action Fail; explicit review intent Fail; read-only retry handoff Fail; residue mutation confirmation Pass.
- Risks: treating Close as consent to review, letting Retry jump into cleanup confirmation, reusing stale pre-uninstall profiles, adding mutation authority to the result window, or losing the post-attempt rescan boundary.
- Impact scope: typed post-scan action enum/state, one primary result-window button, plan-to-parent action handoff, read-only retry presentation, focused tests, and records. No official uninstaller, worker trust/IPC, residue classification, quarantine operation, confirmation, restore, timeline, registry, service, task, or elevation change.
- Acceptance: clean result has only Close; failed/still-present/incomplete-background result offers read-only retry; residue result offers explicit review; result window never executes system work; MainWindow re-resolves current inventory after every action; Retry only displays a fresh read-only residue conclusion; Review enters the existing confirmation-capable flow; Close never triggers residue review.
- Status: Completed and verified. Typed actions are clickable; Close is non-consent; Retry stops at a fresh read-only conclusion; Review alone can enter the existing confirmation-capable residue flow.
- Last verification: focused action/presentation 9/9; related uninstall/product 361/361; full regression 948/948; solution build 0 warnings/errors; 341 strict UTF-8 C#/XAML files with zero replacement characters. First-view render passed and was inspected at `.omx/qa-uninstall-post-scan-action.png`.
- Blockers: none. Real uninstall remains signed disposable-fixture release acceptance.
- Exact next action: audit the personal large/duplicate-file findings and add a bounded read-only inspection handoff without turning findings into cleanup consent.

## Active Slice - 2026-07-16 Real application-search placeholder

- Objective: make Application Management open with an actually empty search query while retaining a visible `搜索应用` hint that disappears as soon as the user types.
- Dependencies: `MainWindow.xaml` application toolbar, `AppSearchTextBox_TextChanged`, app catalog filtering, and Agent/internal app-target handoffs that set the search text programmatically.
- Risks: hiding all apps at first entry, leaving a stale hint over typed text, breaking programmatic app selection, or adding another nested card/toolbar shift.
- Impact scope: one inline search control presentation, one visibility update in the existing text-change handler, focused WPF/source contracts, and records. No inventory scan, filtering semantics, app action, Agent authority, or system mutation change.
- Acceptance: initial `Text` is empty; visible hint is non-interactive; typing or Agent navigation hides the hint and filters normally; clearing restores the hint and all applicable apps; stable AutomationIds exist; layout dimensions do not shift.
- Status: Completed with a real-window visual Warn. The search value is empty, a non-interactive overlay provides the hint, and the existing text-change handler updates visibility before filtering. The compatibility placeholder rule in Core remains harmless for older callers.
- Last verification: focused search/catalog tests 5/5; full regression 944/944; solution build 0 warnings/errors; 340 strict UTF-8 C#/XAML files; structural XAML proves fixed `160x34` host, stable control/hint AutomationIds, empty value, and non-interactive hint. Computer Use reached Windows but `launch_app` timed out and a passive poll found no OMNIX window.
- Blockers: none.
- Exact next action: continue the visible-entry audit; keep a real Application Management screenshot as a visual acceptance Warn until Computer Use can launch the Debug app.

## Active Slice - 2026-07-16 One-shot uninstall/migration submission and rescan boundary

- Objective: prevent reviewed uninstall/migration requests from being submitted twice and make unknown outcomes force a fresh, failure-tolerant application/closure scan.
- Dependencies: migration/uninstall plan-window submission handlers, their production coordinators, and MainWindow post-window application/residue/closure refresh.
- Readiness matrix: final consent Pass; request composition Pass; production coordinators Pass; ordinary returned-outcome refresh Pass; one-shot request lock Pass; thrown/unknown-attempt presentation Pass; parent read-failure recovery Pass.
- Risks: reusing stale snapshot/rollback evidence after a completed, failed, or uncertain migration; treating a thrown coordinator call as no attempt; allowing a second move from stale source paths; or claiming success from an exception.
- Impact scope: migration-plan execution-boundary state, beginner-safe unknown-result presentation, focused contracts/tests, and protocol records. No migration file movement, junction creation, process/service/task handling, manifest validation, worker trust, IPC, confirmation, or elevated authority change.
- Acceptance: after the final request crosses into the production coordinator, the current plan cannot be submitted again; ordinary outcomes preserve their existing truth; an exception is recorded as attempted but not completed, shows a path-free unknown-result explanation, and causes MainWindow to rescan applications and migration closure after the plan closes; no automatic retry occurs.
- Status: Completed with a signed/disposable runtime Warn. Both plan windows mark coordinator invocation before awaiting, preserve ordinary returned truth, show path-free unknown conclusions, and never re-enable the current submitted plan. Parent workflows use one read-only failure boundary and stop residue/closure inference when application refresh is unavailable.
- Last verification: focused one-shot/recovery contracts 4/4; uninstall/migration related tests 432/432; final full regression 942/942; solution build 0 warnings/errors; 339 strict UTF-8 C#/XAML files; `.omx/qa-uninstall-unknown-attempt.png` confirms the unknown uninstall conclusion, Agent advice, safety text, and close action are visible in the first view.
- Blockers: none. Real migration remains outside development acceptance.
- Exact next action: audit the remaining visible V1 entries for another user-facing action whose evidence, execution, completion, or recovery chain is still disconnected. Keep real uninstall/migration for signed disposable-fixture release acceptance.

## Active Slice - 2026-07-16 Undo-Center mutation exception synchronization

- Objective: refresh current Undo Center/application state when quarantine purge, ordinary quarantine restore, or startup restore throws after its safety pipeline has been invoked.
- Dependencies: `ReviewQuarantineCleanup_Click`, `RestoreQuarantineTimelineItemAsync`, `RestoreStartupTimelineItemAsync`, their operation handlers, `LoadTimelineAsync`, current software scan, and existing conservative handler journal states.
- Readiness matrix: current-row/evidence preparation Pass; confirmations Pass; safety pipelines Pass; returned-result timeline refresh Pass; purge thrown-attempt refresh Fail; quarantine-restore thrown-attempt refresh Fail; startup-restore thrown-attempt application/timeline refresh Fail.
- Risks: showing a restorable item after it was restored, hiding partially purged records, stale startup state after post-write verification failure, refreshing before confirmation, or retrying mutation automatically.
- Impact scope: MainWindow post-attempt read-only recovery flags and shared startup-state refresh naming, focused contracts, tests, and records. No purge candidate selection, permanent deletion, restore evidence, manifest/hash, exact file/registry mutation, journal schema, confirmation, pipeline, process/service/task, or elevation change.
- Acceptance: pre-execution refusal/cancel remains unchanged; once each pipeline is invoked, returned outcomes keep existing refresh behavior and thrown outcomes refresh exactly once before final copy; startup restore refreshes both application/startup evidence and timeline on every returned/unknown result; no catch retries an operation; read failure cannot become success.
- Status: Completed with a runtime Warn. Purge, ordinary restore, and startup restore now mark pipeline invocation, refresh returned outcomes, and refresh once from catch after an unsynchronized attempted mutation. Startup disable/restore share one application-plus-timeline read-only helper.
- Last verification: focused Undo Center synchronization plus startup experience 11/11; related Undo/quarantine/startup/product tests 220/220; final full regression 937/937; solution build 0 warnings/errors; 336 strict UTF-8 C#/XAML files; seven direct MainWindow pipeline methods each have pipeline/attempt/catch-guard counts 1 and synchronization-call count 2.
- Blockers: none.
- Exact next action: keep real purge/restore mutation as disposable-fixture release acceptance; continue the key-entry audit outside the now-unified local post-attempt synchronization boundary.

## Active Slice - 2026-07-16 Uninstall-residue post-attempt state synchronization

- Objective: refresh Undo Center and current application/residue evidence after every confirmed low-risk uninstall-residue quarantine attempt.
- Dependencies: `ReviewUninstallResidueAsync`, `QuarantineOperationHandler`, pre-operation `afterProfiles`, `LoadTimelineAsync`, `ScanSoftwareProfilesAsync`, app catalog refresh, and inline residue outcome presentation.
- Readiness matrix: ordinary-app guard Pass; residue scan/risk grouping Pass; identity preparation Pass; final confirmation Pass; safety pipeline Pass; success timeline refresh Pass; success post-move application refresh Fail; returned-failure refresh Fail; thrown-attempt refresh Fail.
- Risks: stale residue paths after successful movement, hidden partial-restorable history, losing the reviewed outcome when the app tile disappears, refreshing before confirmation, or treating read failure as clean uninstall.
- Impact scope: MainWindow residue-quarantine post-attempt read-only synchronization, focused contracts, tests, and records. No official uninstall, residue classification, identity binding, confirmation, quarantine movement, rollback, restore, timeline schema, process/registry/service/task, or elevation authority change.
- Acceptance: pre-execution review/refusal/cancel retains current reads only; after pipeline invocation, every returned result refreshes timeline and attempts one fresh software scan before presentation; thrown outcomes do the same once; app catalog reflects post-move state when available; inline outcome remains visible/path-free even if target tile disappears; read failure never becomes a no-residue claim or retries mutation.
- Status: Completed with a runtime Warn. Every returned residue-quarantine result reloads Undo Center and attempts one fresh software scan before presentation; thrown attempts synchronize once, while cancel/refusal paths retain their existing non-mutating behavior.
- Last verification: TDD red/green; focused residue tests 11/11; related uninstall/quarantine/product tests 220/220; final full regression 937/937; build 0 warnings/errors; synchronization helper mutation-authority hits 0.
- Blockers: none.
- Exact next action: include this slice in final full regression/build/static gates after closing Undo Center mutation exception synchronization.

## Active Slice - 2026-07-16 C-drive cleanup post-attempt rescan

- Objective: refresh Undo Center and the full C-drive health/recommendation state after every confirmed decision-card cleanup pipeline attempt, so successful, partial, failed, or uncertain outcomes never leave stale reclaimable space or executable cards.
- Dependencies: `ExecuteSelectedRecommendationAsync`, `QuarantineOperationHandler`, `LoadTimelineAsync`, `RefreshHealthScanAsync`, recommendation selection state, and existing path-free result copy.
- Readiness matrix: low-risk card policy Pass; identity preparation Pass; final confirmation Pass; safety pipeline Pass; success timeline refresh Pass; success health/recommendation refresh Fail; returned-failure refresh Fail; thrown-attempt refresh Fail.
- Risks: stale executable cards after moved paths, hiding partial-restorable history, treating rescan completion as operation success, running expensive diagnosis before confirmation, duplicate scans, or losing the operation conclusion when refresh fails/cancels.
- Impact scope: MainWindow direct-cleanup post-attempt read-only synchronization, one focused contract file, tests, and records. No scanner rules, recommendation policy, candidate identity, confirmation, quarantine movement, rollback, restore, timeline schema, process/registry/service/task, or elevation authority change.
- Acceptance: pre-execution refusal/cancel performs no rescan; once pipeline invocation begins, each returned result refreshes timeline and requests one full health scan before outcome presentation; thrown outcomes do the same if not already synchronized; refreshed recommendations are non-stale; refresh failure preserves the prior operation truth and never retries mutation; success wording remains gated only by `result.Success`.
- Status: Completed with a runtime Warn. Every returned cleanup result refreshes Undo Center and requests a full read-only health scan before presentation; thrown attempts synchronize once, and the execute button is recalculated from the refreshed card selection rather than re-enabled against stale evidence.
- Last verification: TDD red/green; focused direct-cleanup test 1/1; related C-drive/health/quarantine/product tests 243/243; final full regression 937/937; build 0 warnings/errors; synchronization helper mutation-authority hits 0.
- Blockers: none; full health refresh can be slow but is bounded/read-only and already user-cancellable.
- Exact next action: include this slice in final full regression/build/static gates after closing uninstall-residue synchronization.

## Active Slice - 2026-07-16 Startup-disable post-attempt state synchronization

- Objective: refresh current startup/application evidence and Undo Center history after every confirmed startup-disable pipeline attempt, including returned failure and thrown/uncertain outcomes.
- Dependencies: `ReviewAndExecutePendingStartupDisableAsync`, `StartupEntryControlOperationHandler`, rollback-manifest/timeline semantics, `ScanSoftwareProfilesAsync`, and `LoadTimelineAsync`.
- Readiness matrix: current target resolution Pass; fresh startup preparation Pass; rollback manifest Pass; final confirmation Pass; safety pipeline Pass; success application refresh Pass; success timeline refresh Fail; returned-failure refresh Fail; thrown-attempt refresh Fail.
- Risks: stale startup toggle after a registry write, hidden rollback record, deleting an uncommitted manifest after execution begins, claiming success from an uncertain outcome, or refreshing before the mutation boundary.
- Impact scope: MainWindow startup-disable post-attempt read-only synchronization, focused source contracts, tests, and records. No startup discovery, registry locator, manifest content/hash, confirmation, exact store mutation, rollback, restore, timeline schema, service/task/process, or elevation change.
- Acceptance: pre-confirmation refusal/cancel retains current behavior; once the pipeline is invoked, every returned result refreshes application/startup evidence and Undo Center before success/failure presentation; thrown outcomes do the same; committed/possibly used rollback manifests are never treated as uncommitted; read failures do not become success or trigger mutation/retry.
- Status: Completed with a runtime Warn. Every returned startup pipeline result refreshes current applications/startup evidence and Undo Center before presentation; thrown outcomes after invocation use the same read-only recovery helper. Manifest cleanup remains limited to never-confirmed work.
- Last verification: TDD red/green; focused startup-control tests 8/8; related startup/timeline/product tests 207/207; final full regression 937/937; build 0 warnings/errors; shared helper mutation-authority hits 0.
- Blockers: none.
- Exact next action: include this slice in final full regression/build/static gates after closing direct C-drive cleanup synchronization.

## Active Slice - 2026-07-16 Cache-cleanup post-attempt state synchronization

- Objective: refresh Undo Center history and current application/cache evidence after every confirmed cache-cleanup pipeline attempt, including partial rollback, returned failure, cancellation, and unexpected exception.
- Dependencies: `ExecutePendingAppCacheCleanupAsync`, `QuarantineOperationHandler` partial-restorable journal semantics, `LoadTimelineAsync`, `ScanSoftwareProfilesAsync`, and the existing drawer completion/refusal presenters.
- Readiness matrix: cache evidence Pass; final confirmation Pass; specialized quarantine handler Pass; safety pipeline Pass; success refresh Pass; returned-failure refresh Fail; thrown-attempt refresh Fail.
- Risks: stale Undo Center after partial movement, stale cache size after a successful or uncertain attempt, treating a failed result as no change, refreshing before the mutation boundary, or hiding the original outcome behind a read failure.
- Impact scope: MainWindow cache-cleanup post-attempt read-only synchronization, one focused source contract, tests, and records. No cache candidate policy, identity binding, confirmation, quarantine movement, rollback, timeline schema, restore, registry, process, service, or elevation authority change.
- Acceptance: pre-execution refusal/cancel performs no new refresh; once the pipeline is invoked, every returned result refreshes timeline and attempts one current software scan before presentation; an exception after invocation does the same in recovery; read failures do not overwrite the operation conclusion or trigger retries/mutation; success wording remains success-only.
- Status: Completed with a runtime Warn. Every returned pipeline result refreshes Undo Center and attempts one software scan before presentation; thrown outcomes after invocation use the same read-only recovery helper. Pre-execution exits remain unchanged.
- Last verification: TDD red/green; focused cache tests 6/6; related cache/quarantine/timeline/product tests 213/213; final full regression 937/937; build 0 warnings/errors; synchronization helper mutation-authority hits 0.
- Blockers: none.
- Exact next action: include this slice in the final full regression/build/static verification after closing the equivalent startup-disable boundary.

## Active Slice - 2026-07-16 Persistent later install observation

- Objective: let a beginner rescan later after an initial report when a bootstrapper or child installer may have outlived the observed process, without entering advanced manual comparison.
- Dependencies: the automatic before snapshot, `InstallerExecutionResult`, install-page report controls, `ResetInstallerAnalysis`, a read-only post-scan coordinator, and the existing result/report/catalog presentation.
- Readiness matrix: initial report Pass; child/bootstrap limitation explained Pass; session baseline retention Pass; primary-page later-rescan action Pass; new-package baseline revocation Pass; read-only-only persistent handler Pass.
- Risks: comparing a new package against an old baseline, showing the button before any trusted launch, creating a second scan implementation, retaining launcher authority in a read-only handler, claiming installation success, or cluttering the install page.
- Impact scope: two session-only baseline fields, one conditionally visible install-page button, one dedicated read-only post-scan coordinator shared by execution recovery and persistent rescan, result presentation reuse, focused tests, and records. No persisted personal path, package launch, confirmation, route, installer interaction, registry/file mutation, cleanup, migration, uninstall, or Agent execution authority change.
- Acceptance: the button is collapsed by default and visible only after a non-refused trusted launch result; it retains the exact original before snapshot for the current analyzed package; changing the installer path clears/hides it; each click performs one read-only software/footprint scan and reuses the ordinary result/report/catalog flow; failure remains retryable; no launcher/pipeline appears in the handler or dedicated coordinator; all copy remains preliminary and path-free.
- Status: Completed with a visual/release Warn. A session-bound later-rescan action now appears after any non-refused trusted launch result, retains the exact automatic before baseline, disappears when the selected package changes, and uses a dedicated read-only post-scan coordinator plus the shared result/report/catalog flow.
- Last verification: focused installer coordinator 25/25; related installer/report/product/failure-boundary tests 243/243; full regression 927/927; solution build 0 warnings/errors; 332 strict UTF-8 C#/XAML files; MainWindow XAML parses; persistent button id/click each 1, collapsed by default and before advanced diagnostics; handler launcher/pipeline/mutation hits 0; dedicated coordinator has one software read, one footprint read, one report build, and zero launch/mutation authority.
- Blockers: no source blocker. Real installer and visible WPF acceptance remain release Warns.
- Exact next action: leave real installer and visible click-through as release Warns, then resume the V1 critical-workflow audit at the next user-facing capability that still has an entry without a connected, truthful completion or recovery state.

## Active Slice - 2026-07-16 Beginner-safe installer post-scan recovery

- Objective: give a beginner one direct, read-only recovery action when installer waiting is interrupted or the initial post-install scan fails, without sending them into advanced diagnostics.
- Dependencies: `InstallerExecutionResultPresenter`, `InstallerExecutionResultWindow`, `InstallerExecutionCoordinator`, the captured `InstallSystemSnapshot` before evidence, `PrepareInstaller_Click`, and existing install-diff presentation/catalog synchronization.
- Readiness matrix: trusted installer launch Pass; original before snapshot Pass; interrupted/failed result explanation Pass; ordinary-page recovery action Pass; read-only retry primitive Pass; installer relaunch refusal Pass; preliminary-not-success wording Pass.
- Risks: retrying while an installer child window still runs, relaunching the package accidentally, losing the original before baseline, claiming installation success, creating an unbounded automatic retry, or exposing technical errors/paths.
- Impact scope: one result-window recovery command, one read-only coordinator retry method shared with the initial post-scan, MainWindow user-driven loop, focused tests, and records. No package analysis, target policy, route memory, confirmation, operation pipeline, process launch, installer clicks, registry/file mutation, migration, cleanup, or uninstall authority change.
- Acceptance: only `InstallerWaitInterrupted` and `PostScanFailed` expose `我已完成安装，重新扫描`; each click performs one software/footprint observation against the original before snapshot; no launcher/pipeline is invoked; a valid retry updates the report and application catalog; a failed retry remains retryable and never claims success; LaunchRefused and completed initial scans expose no recovery command.
- Status: Completed with a visual/release Warn. Interrupted wait and failed post-scan results now expose one user-driven retry; each click performs one read-only scan against the original before snapshot, and a valid result reuses the existing report/catalog binding.
- Last verification: TDD red/green; focused installer coordinator 23/23; related installer/report/product/failure-boundary tests 241/241; full regression 925/925; solution build 0 warnings/errors; 332 strict UTF-8 C#/XAML files; XAML parses under strict UTF-8; one launch, one retry-read call, one loop, one catalog binding, and zero changed-scope launcher/pipeline/mutation hits. Computer Use was reachable but Debug-app launch timed out and a passive poll found no OMNIX window.
- Blockers: no source blocker. Real installer execution remains outside development acceptance.
- Exact next action: audit the successful initial-report state for bootstrap/child installers that outlive the observed process. Provide a persistent beginner-visible later rescan only if it can preserve the original before baseline and avoid stale or guessed attribution.

## Active Slice - 2026-07-16 Post-install inventory reuse

- Objective: reuse the coordinator's verified installation-after software snapshot to update the application catalog immediately when the initial post-install report is available.
- Dependencies: `InstallerExecutionCoordinator.ExecuteConfirmedAsync`, `InstallerExecutionResult.AfterSnapshot`, `InstallSnapshotDiffReport`, `PrepareInstaller_Click`, and `SetSoftwareProfiles`.
- Readiness matrix: package analysis Pass; target recommendation Pass; before evidence Pass; final consent Pass; pipeline launch Pass; installer wait Pass; after software/C-drive scan Pass; actual-placement report Pass; application-catalog synchronization Pass.
- Risks: showing stale applications after a valid report, performing a duplicate scan, updating catalog from a refused/interrupted/failed result, or turning installer exit code into a success claim.
- Impact scope: MainWindow success-result binding and focused source contract. No installer inspection, routing, consent, launch, process wait, snapshot capture, report attribution, file/registry mutation, or execution authority change.
- Acceptance: only a result with both `AfterSnapshot` and `Report` updates `_softwareProfiles`; the exact after-snapshot profile list is reused without another scan; report and Agent explanation remain unchanged; refused/interrupted/failed outcomes do not update the catalog or claim success.
- Status: Completed with a release Warn. A valid initial post-install snapshot now updates the application catalog from the exact verified profile list before the placement report is presented; refused, interrupted, and failed outcomes remain unchanged.
- Last verification: TDD red isolated the missing binding; focused installer coordinator 16/16; related installer/report/product tests 232/232; full regression 918/918; solution build 0 warnings/errors; 332 strict UTF-8 C#/XAML files; success gate/catalog update/report presentation counts each 1 in correct order; changed-method mutation-authority hits 0.
- Blockers: no source blocker. Real installer execution remains out of development acceptance.
- Exact next action: audit the interrupted-wait and failed-post-scan installer outcomes. Decide whether the beginner needs one safe read-only `我已完成安装，重新扫描` recovery action that captures a fresh after snapshot without relaunching the installer or weakening report attribution.

## Active Slice - 2026-07-16 Migration post-attempt state synchronization

- Objective: refresh application location/C-drive evidence and migration-closure records after every trusted production migration attempt, including timeout, transport failure, refusal, cancellation, and uncertain partial outcomes.
- Dependencies: `MigrationPlanWindow.ProductionExecutionAttempted`, `ProductionCompleted`, `ShowMigrationPlanAsync`, `ScanSoftwareProfilesAsync`, and `RefreshMigrationClosureAsync`.
- Readiness matrix: drawer eligibility Pass; rollback evidence Pass; final consent Pass; signed worker/authenticated transport Pass; migration pipeline/rollback Pass; authenticated completion classification Pass; successful inventory/closure refresh Pass; uncertain-attempt inventory/closure refresh Fail.
- Risks: stale install-location and closure UI after partial file/link mutation, claiming completion from an uncertain response, continuing migration automatically, or presenting safe UAC cancellation as damage.
- Impact scope: MainWindow post-window read-only orchestration, one focused source contract, and records. No migration plan, descriptor, signer, worker, transport, file/link mutation, rollback, elevation, or action authority change.
- Acceptance: no execution attempt performs no extra read; every production attempt triggers one inventory scan and one closure refresh; only authenticated accepted completion shows completed copy; uncertain outcomes show their existing conclusion plus a no-auto-continue safety statement; no retry or mutation is triggered.
- Status: Completed with a release Warn. Every trusted production attempt now triggers exactly one application-location scan and one migration-closure refresh before MainWindow distinguishes authenticated completion from an uncertain outcome.
- Last verification: focused migration coordinator/orchestration 7/7; related migration/app-action/product tests 256/256; full regression 917/917; solution build 0 warnings/errors; 332 non-generated C#/XAML files strict UTF-8 with zero replacement characters; attempt/scan/closure/completion branches each exactly 1 in correct order; changed-method mutation-authority hits 0.
- Blockers: no source blocker. Signed disposable migration remains a release Warn and was not run on this machine.
- Exact next action: audit installer execution and post-install evidence from package recognition/path recommendation through final confirmation, trusted launch, after snapshot, actual install location, C-drive write attribution, and report presentation. Identify the first missing uncertain-attempt or evidence boundary without auto-running an installer.

## Active Slice - 2026-07-16 Official uninstall post-attempt inventory refresh

- Objective: make every trusted official-uninstall production attempt refresh current application inventory after the plan window closes, including timeout, transport failure, cancellation, incomplete response, and other uncertain outcomes.
- Dependencies: `UninstallPlanWindow.ProductionExecutionAttempted`, `ProductionCompleted`, `ProductionResidueReviewRecommended`, `ShowUninstallPlanAsync`, and the existing read-only `ScanSoftwareProfilesAsync`/residue review path.
- Readiness matrix: drawer entry guard Pass; recovery/snapshot evidence Pass; final visual consent Pass; signed same-package worker Pass; authenticated one-shot transport Pass; elevated operation pipeline Pass; worker post-scan Pass; successful local residue review/quarantine Pass; uncertain-attempt inventory refresh Fail.
- Risks: retaining stale software/profile state after a worker may have changed the system, treating a transport completion as uninstall success, running residue cleanup without a validated post-scan, or hiding a safe UAC cancellation behind alarming copy.
- Impact scope: MainWindow post-window orchestration and focused source/decision tests. No operation descriptor, trust, signer, UAC, worker, transport, launcher, post-scanner, residue classifier, quarantine, registry, file, service, task, or elevation change.
- Acceptance: no production attempt means no extra scan; every production attempt triggers exactly one current inventory refresh; residue review requires completed production plus explicit residue-review recommendation; uncertain/failed attempts update inventory only and never create or execute residue cleanup; beginner conclusion remains truthful and path-free.
- Status: Completed with a release Warn. MainWindow now performs exactly one read-only application inventory refresh after every trusted production attempt, while residue review still requires completed production plus an explicit validated recommendation.
- Last verification: focused coordinator/orchestration 6/6; related official-uninstall/residue/evidence/product tests 387/387; full regression 916/916; solution build 0 warnings/errors; 332 non-generated C#/XAML files strict UTF-8 with zero replacement characters; post-attempt branch/scan/double-gate/residue call each exactly 1; mutation-authority hits in the changed method 0.
- Blockers: no source blocker. Signed real-package uninstall acceptance remains a release Warn and was not exercised on this machine.
- Exact next action: audit migration post-attempt synchronization. MainWindow currently refreshes application location and closure state only on `ProductionCompleted`; timeout, transport failure, or partial execution can leave both views stale after a migration attempt.

## Active Slice - 2026-07-16 Startup restore pipeline closure

- Objective: connect startup-entry timeline restore to a prepared, explicitly confirmed `SafetyOperationPipeline` operation that reloads the current timeline row and revalidates the rollback manifest before any registry restore.
- Dependencies: `ActionTimelineStore.LoadByIdAsync`, `StartupRollbackManifestStore`, `StartupEntryControlOperationHandler`, `IStartupEntryControlStore`, `TimelineRestoreConfirmationPresenter`, `OperationDescriptor`, and `SafetyOperationPipeline`.
- Readiness matrix: restore confirmation Pass; manifest confinement/hash/schema verification Pass; exact registry restore guard Pass; current timeline reload Fail; prepared restore descriptor Fail; pipeline execution Fail; handler-owned journal update Fail.
- Risks: trusting stale UI paths/state, restoring from a changed manifest, writing a same-name Run value, registry ACL or StartupApproved drift, reporting success when timeline update fails, or exposing manifest/registry details in beginner copy.
- Impact scope: one typed startup-restore policy/evidence/outcome and pipeline handler, startup restore orchestration, focused fixture tests, and records. No new registry location, startup discovery, disable operation, service/task/process control, elevation, installer, cleanup, migration, or AI authority.
- Acceptance: preparation loads the current restorable startup row by id and requires exactly one verified manifest plus one supported registry locator; the descriptor binds row id, manifest id/hash, and state fingerprint; execution requires explicit confirmation and rechecks the row/manifest; the existing exact store owns registry mutation; success/failure updates the same row conservatively; stale, changed, unconfirmed, malformed, or mismatched evidence fails before mutation; visible copy remains path-free.
- Status: Completed with a visual Warn. Startup restore now prepares from the current timeline row, binds verified manifest id/hash and state fingerprint, reconfirms, executes through `SafetyOperationPipeline`, delegates exact registry mutation to the existing store, and updates the same timeline row conservatively.
- Last verification: focused startup restore 7/7; related startup/timeline/quarantine/product tests 204/204; full regression 915/915; solution build 0 warnings/errors; 332 non-generated C#/XAML files strict UTF-8 with zero replacement characters; direct MainWindow startup restore/timeline-update calls 0; old public handler restore API 0; restore confirmation AutomationIds exactly 1 each.
- Blockers: no source blocker. Real WPF launch remains Warn; no screenshot is claimed and no real registry mutation was used.
- Exact next action: audit the beginner-facing `卸载干净点` production chain from drawer handoff through official uninstall preparation, final consent, trusted worker execution, post-uninstall inventory refresh, residue review/quarantine, timeline persistence, and restore. Identify the first disconnected or overstated boundary without weakening signer, identity, confirmation, rollback, or elevation gates.

## Active Slice - 2026-07-16 Quarantine restore pipeline closure

- Objective: connect ordinary quarantine restore to the same explicit-confirmation `SafetyOperationPipeline` used by cleanup, with current timeline and payload identity revalidation before any file move.
- Dependencies: `ActionTimelineStore`, `TimelineRestoreConfirmationPresenter`, `FileQuarantineService`, `IQuarantineCandidateIdentityReader`, `OperationDescriptor`, and `SafetyOperationPipeline`.
- Readiness matrix: recommendation selection Pass; low-risk candidate policy Pass; pre-confirmation source identity Pass; final cleanup confirmation Pass; quarantine pipeline Pass; timeline journal Pass; restore confirmation Pass; restore pipeline Fail (direct service call); restore payload identity binding Fail.
- Risks: restoring a payload or manifest changed after confirmation, using a stale/forged timeline item, partial multi-item restore with inaccurate journal state, bypassing the pipeline, disabling startup restore accidentally, or exposing paths in beginner copy.
- Impact scope: one typed quarantine-restore policy/evidence/outcome, one handler, timeline load-by-id, ordinary quarantine restore orchestration, focused tests, and records. Startup restore remains separate. No cleanup candidate discovery, quarantine move policy, purge, installer, uninstall, migration, service, registry, or elevation change.
- Acceptance: preparation loads the current timeline entry by id and requires restorable `quarantine.restore`; manifest and quarantined payload identities are bound before confirmation; confirmation clones a destructive low-risk descriptor; handler rechecks timeline/manifests/payloads, restores through `SafetyOperationPipeline`, and updates timeline state; unconfirmed, stale, changed, duplicate, invalid, or over-limit evidence fails before move; partial results are not reported as success; visible copy stays path-free.
- Status: Completed with a visual Warn. Ordinary quarantine restore now reloads the current timeline row, binds manifest and payload identity before confirmation, revalidates all evidence before any move, executes through `SafetyOperationPipeline`, and updates the same timeline row conservatively.
- Last verification: focused restore pipeline 6/6; related quarantine/timeline/startup/product tests 227/227; full regression 908/908; solution build 0 warnings/errors; 330 non-generated C#/XAML files strict UTF-8 with zero replacement characters; MainWindow direct quarantine restore calls 0; old restore/aggregate helpers 0; restore confirmation AutomationIds exactly 1 each.
- Blockers: no source blocker. Real WPF launch remains Warn; no screenshot is claimed.
- Exact next action: audit and connect startup-entry timeline restore. It still calls `StartupEntryControlOperationHandler.RestoreAsync` directly and updates the timeline in MainWindow instead of using a prepared, explicitly confirmed `SafetyOperationPipeline` operation with current timeline and manifest identity revalidation.

## Active Slice - 2026-07-16 Background application ownership summary

- Objective: make the existing application-page first-view background summary distinguish ordinary resident applications from explicit system and managed-root ownership-pending evidence.
- Dependencies: `AppPresentationBuilder.IsResident`, `CanUseOrdinaryApplicationActions`, `AppCatalogSummaryPresenter`, and `AgentActionCandidateCatalog`.
- Risks: double-counting one application with several background clues, hiding signal-type totals, making protected evidence sound actionable, adding UI clutter, duplicating ownership policy, or changing startup/service/task control authority.
- Impact scope: one typed read-only background ownership catalog, structured summary counts and one existing text string, Agent resident catalog reuse, focused tests, and records. No XAML structure, observation adapter, startup policy, plan, operation, pipeline, worker, registry, service, task, process, file, or mutation change.
- Acceptance: resident application membership remains any running/startup/service/task clue; ownership groups are mutually exclusive and exhaustive; signal-type totals remain per application and can overlap; ordinary/system/ownership-pending counts appear in the existing summary; protected-only evidence says `仅供查看`; Agent resident lists share the same catalog.
- Status: Completed with a visual Warn. One typed background ownership catalog now separates ordinary, explicit system, and ownership-pending resident profiles while preserving overlapping running/startup/service/task application counts in the existing compact summary. Agent resident lists reuse the same catalog.
- Last verification: focused background ownership 3/3; related application/Agent/startup/system tests 240/240; full regression 902/902; solution build 0 warnings/errors; 328 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused mutation-authority hits 0; old Agent resident projection and raw background summary hits 0; shared catalog bindings 2; `AppsSummaryTextBlock` AutomationId exactly 1.
- Blockers: no source blocker. Real WPF launch remains Warn after the antivirus-updated Computer Use launch timed out earlier in this turn; no fallback UI automation was used.
- Exact next action: perform a critical workflow readiness audit beginning with low-risk C-drive cleanup: trace recommendation selection through confirmation, quarantine operation, timeline persistence, and restore; identify and implement the first unconnected boundary without weakening identity, snapshot, rollback, or signer gates.

## Active Slice - 2026-07-16 C-drive application ownership summaries

- Objective: make the existing homepage digest and application-page first-view summary separate ordinary C-drive application evidence from explicit system components and managed-root ownership-pending profiles.
- Dependencies: `AppPresentationBuilder.HasCDriveFootprint`, `CanUseOrdinaryApplicationActions`, `AppCatalogSummaryPresenter`, `HealthDigestBuilder`, and `AgentActionCandidateCatalog`.
- Risks: hiding protected diagnostic evidence, changing `占 C 盘` filter membership, making read-only profiles sound actionable, duplicating ownership rules, adding another visible panel, or changing scan/operation authority.
- Impact scope: one typed read-only C-drive ownership catalog, two existing summary strings and structured counts, Agent aggregate catalog reuse, focused tests, and records. No XAML structure, scanner discovery, recommendation, plan, pipeline, worker, trust, confirmation, registry, file, process, or mutation change.
- Acceptance: total C-drive footprint remains unchanged; ordinary, explicit system, and ownership-pending counts are mutually exclusive and exhaustive; both existing first-view summaries show the groups without paths or action claims; protected-only evidence says `仅供查看`; existing AutomationIds and filter membership remain unchanged.
- Status: Completed with a visual Warn. One typed catalog now keeps the existing C-drive footprint total while separating ordinary, explicit system, and ownership-pending evidence across the homepage digest, application-page summary, and Agent aggregate catalog.
- Last verification: focused ownership summaries 3/3; related application/health/Agent/system tests 238/238; full regression 899/899; solution build 0 warnings/errors; 325 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused mutation-authority hits 0; old total-only summary and Agent projection hits 0; shared catalog bindings 3; both existing summary AutomationIds exactly 1.
- Blockers: no source blocker. Real WPF launch remains Warn after the antivirus-updated Computer Use launch timed out earlier in this turn; no fallback UI automation was used.
- Exact next action: audit the application-page first-view background totals (`正在运行` / `自启动` / `后台服务` / `计划任务`). Separate ordinary reviewable application evidence from explicit system and ownership-pending read-only evidence without adding another panel.

## Active Slice - 2026-07-16 Health risk and ownership wording

- Objective: ensure health findings, C-drive Agent answers, stored digests, and startup dimensions describe only `None`/`Low` clean findings as low-risk and keep ownership-pending startup evidence read-only.
- Dependencies: `HealthCheckSummaryBuilder`, `AgentConversationPresenter.CDriveReply`, `HealthDigestBuilder`, `AppPresentationBuilder.CanUseOrdinaryApplicationActions`, and one shared clean-risk predicate.
- Risks: calling medium/high cleanup safe, counting high-risk impact as reclaimable, hiding higher-risk findings, labeling a managed-root profile ordinary, or changing operation authority.
- Impact scope: one typed risk predicate, health/digest count and copy, startup dimension grouping/rating, focused tests, and records. No scanner discovery, recommendation generation, operation descriptor, pipeline, worker, trust, confirmation, quarantine, registry, process, or file mutation change.
- Acceptance: only `None`/`Low` clean findings contribute to low-risk counts and reclaimable wording; medium/high clean findings remain visible with observe/rollback wording; startup dimension separates ordinary, explicit system, and ownership-pending clues; protected-only startup evidence rates `仅供查看`; all visible summaries remain path-free.
- Status: Completed with a visual Warn. One shared action-plus-risk policy now limits low-risk cleanup/reclaimable wording to `None`/`Low`; medium/high clean findings stay visible with observe/snapshot/rollback guidance. Startup dimensions now separate ordinary, explicit system, and ownership-pending evidence, with protected-only results rated `仅供查看`.
- Last verification: focused risk/ownership tests 5/5; related health/Agent/scanner tests 244/244; full regression 896/896; solution build 0 warnings/errors; 323 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused mutation-authority hits 0; action-only low-risk and category-only explicit-system patterns both 0.
- Blockers: no source blocker. Real WPF launch remains Warn after the antivirus-updated Computer Use launch timed out earlier in this turn; no fallback UI automation was used.
- Exact next action: audit stored health digest and first-view C-drive application summaries so ordinary reviewable profiles, explicit system profiles, and managed-root ownership-pending profiles are counted separately without changing scan or action authority.

## Active Slice - 2026-07-16 Agent aggregate action authority

- Objective: make homepage next-step guidance and general Agent answers distinguish ordinary reviewable actions from D-installed C-data review and protected/system read-only evidence.
- Dependencies: `AppPresentationBuilder` ordinary-action and migration availability, `StartupEntryControlPolicy`, `AgentNextStepPresenter`, and `AgentConversationPresenter` C-drive/applications/migration/uninstall/startup replies.
- Risks: counting a system component as an uninstall or startup-review candidate, calling a D-installed app with C-data clues a main-program migration candidate, hiding diagnostic evidence, duplicating action policy, or changing exact named-app handoff authority.
- Impact scope: one typed read-only aggregate candidate projection, shared availability helpers, beginner aggregate copy, focused tests, and records. No scanner, inventory hydration, exact-app handoff, plan, operation, worker, trust, confirmation, pipeline, registry, file, or process mutation change.
- Acceptance: ordinary eligible profiles appear in migration/uninstall/startup review groups; system and managed-root ownership-pending profiles appear only in explicit read-only groups; D-installed profiles with C-data clues appear only in data-location review; homepage and general Agent replies use these groups without paths or execution claims; all navigation remains internal and navigation-only.
- Status: Completed with a visual Warn. One shared candidate catalog now separates ordinary action review, D-installed/unknown data-location review, and protected read-only evidence across homepage next steps, general questions, exact startup advice, background review, and startup/service plan preview.
- Last verification: focused aggregate authority 5/5; related Agent/product/system/ownership/storage tests 234/234; full regression 891/891; solution build 0 warnings/errors; 321 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused authority hits 0; four legacy raw-candidate patterns each 0.
- Blockers: no source blocker. Real WPF launch remains Warn after the antivirus-updated Computer Use launch timed out earlier in this turn.
- Exact next action: audit risk and ownership wording in health summaries and digests. `Clean` findings may be medium/high risk but are currently counted as low-risk, and the startup health dimension treats managed-root ownership-pending profiles as ordinary.

## Active Slice - 2026-07-16 Homepage migration closure authority

- Objective: ensure homepage migration-closure findings recommend and navigate to migration review only when the current application is uniquely matched and eligible for the ordinary migration workflow.
- Dependencies: `MigrationClosureHealthEnricher`, MainWindow profile resolution, `HealthFindingAgentExplanationBuilder`, `HealthFindingActionPlanBuilder`, and `HomeAgentResponsePresenter.CreateNavigation`.
- Risks: marking protected or ambiguous historical records as `Migrate`, showing “打开对应应用” without a safe target, navigating migration evidence to the C-drive page, hiding historical evidence, or weakening ordinary reviewable closure navigation.
- Impact scope: typed closure target disposition, health finding projection/dimension copy, MainWindow resolver, migration-specific Agent explanation/plan/navigation copy, focused tests, and records. No monitoring, scanner, drawer action, migration plan, operation, worker, trust, pipeline, or mutation change.
- Acceptance: unique ordinary profile produces `Migrate` plus exact app target; unique protected profile produces path-free `Observe` with no app target and explicit system-history wording; ambiguous/unavailable profile produces path-free `Observe` with no target; both read-only cases navigate to Applications generically and do not claim a migration plan; dimension separates actionable reviews from read-only historical records.
- Status: Completed with a visual Warn. Homepage closure findings now use current profile authority: only ordinary reviewable profiles receive `Migrate` and an exact target; protected and unavailable historical records remain visible as path-free `Observe` evidence with generic Applications navigation.
- Last verification: focused homepage authority 4/4; related migration/home/product/personal-storage tests 193/193; full regression 886/886; solution build 0 warnings/errors; 319 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused authority hits 0.
- Blockers: no source blocker. After the user confirmed the antivirus definitions update, Computer Use discovery remained healthy but `launch_app` for `Css.App.exe` timed out and the follow-up app/window poll found no OMNIX window; real WPF proof remains Warn.
- Exact next action: audit aggregate Agent guidance that counts C-drive, migration, uninstall, or startup candidates. Protected/system-owned profiles and D-installed apps with C-data clues must not be described as ordinary actionable candidates.

## Active Slice - 2026-07-15 Migration closure tile and catalog safety

- Objective: prevent historical migration records from replacing protected-profile tile status, actionable priority, or beginner catalog counts while retaining those records as secondary read-only evidence.
- Dependencies: `AppTileUi.From`, app catalog closure sorting, `BuildMigrationClosureCatalogSummary`, `CanReviewMigrationClosure`, and current closure lookup.
- Risks: showing a system component as red `迁移未闭环`, prioritizing a protected profile as if it were actionable, hiding historical evidence entirely, or suppressing legitimate ordinary-app closure warnings.
- Impact scope: typed Core tile and catalog summary presenters, MainWindow binding, focused tests, and records. No monitoring store, observer, matching identity, drawer action, plan, operation, pipeline, or mutation change.
- Acceptance: protected profiles retain base tile label/status and no closure priority; ordinary attention records remain red/prioritized; healthy ordinary records only replace a normal base tag; catalog counts reviewable and protected historical records separately with path-free copy.
- Status: Completed with a visual Warn. Typed tile and catalog presenters now preserve protected status/priority, keep ordinary incomplete closures actionable, show healthy ordinary closure only on an otherwise normal tile, and separate protected historical records in path-free summary copy.
- Last verification: focused closure catalog tests 4/4; related migration/catalog/system/ownership/product tests 191/191; full regression 882/882; solution build 0 warnings/errors; 318 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused authority hits 0; legacy closure override hits 0.
- Blockers: no source blocker. Computer Use `list_apps`/`list_windows` is reachable after the antivirus update, but `launch_app` for the built `Css.App.exe` timed out and no OMNIX window appeared; real WPF proof remains Warn.
- Exact next action: audit homepage `MigrationClosureHealthEnricher` targeting and action wording. A historical closure matched to a protected profile must not become a `Migrate` recommendation or app target merely because its name is unique; ordinary reviewable closures remain actionable.

## Active Slice - 2026-07-15 Central app action entry guards

- Objective: make centralized uninstall, cache-cleanup, and startup-control methods enforce current drawer action policy before reading operation-specific evidence or creating plans.
- Dependencies: `AppDrawerViewModel.AvailableActions`, `ShowUninstallPlanAsync`, `ShowCacheCleanupPreview`, `ShowStartupControlPreviewAsync`, and existing refused action-host states.
- Risks: relying on disabled WPF controls or current Agent routing, constructing protected-profile plans before refusal, leaving pending cache/startup targets after denial, or changing ordinary-app behavior.
- Impact scope: one typed read-only action-entry decision, three method-entry guards, one uninstall refusal presentation, focused tests, and records. No scanner classification, plan contents, operation descriptor, pipeline, worker, trust, confirmation, or mutation change.
- Acceptance: missing/disabled actions fail closed with a plain reason; uninstall refuses before restore-point scan/plan/window; cache refuses before plan/pending operation; startup refuses before handoff/preparation/pending target; ordinary enabled actions retain current behavior.
- Status: Completed with a visual Warn. One typed entry policy now fails closed from the same drawer action state, and uninstall/cache/startup central methods refuse before restore-point reads, plan/preparation work, windows, or pending target assignment.
- Last verification: focused entry-guard tests 2/2; related system/ownership/Agent/cache/startup/uninstall/product tests 409/409; full regression 878/878; solution build 0 warnings/errors; 317 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused authority hits 0; centralized guard hits 3.
- Blockers: no source blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: audit migration-closure tile, sorting, and catalog summary presentation. Protected profiles must retain `系统组件` / `系统归属待确认` status instead of being promoted to actionable red `迁移未闭环`; historical records remain visible as read-only secondary evidence.

## Active Slice - 2026-07-15 Migration closure permission consistency

- Objective: show stale migration-closure evidence without allowing it to override current system/ownership read-only policy or hide the protected-profile Agent conclusion.
- Dependencies: `MigrationClosureSummaryViewModel`, drawer migration action availability, `ShowAppDrawer`, and `ShowMigrationPlanAsync`.
- Risks: forcing the migration button enabled after policy binding, suppressing “建议保留/系统归属待确认” with stale closure copy, blocking legitimate ordinary D-installed closure review, or relying only on a disabled button.
- Impact scope: one typed migration-closure drawer state presenter, WPF binding and plan-entry guard, focused tests, and records. No monitor observation, migration planner, snapshot, rollback, worker, trust, confirmation, pipeline, or mutation change.
- Acceptance: system and managed-root ownership-pending profiles keep their protection conclusion first, display closure evidence as secondary information, and cannot open the ordinary migration plan; ordinary profiles with a closure warning can still review even when their main program is already on D; WPF contains no unconditional closure-driven `IsEnabled = true`.
- Status: Completed with a visual Warn. A typed closure drawer state now keeps system/ownership protection copy first, shows stale closure evidence only as secondary context, permits ordinary D-installed closure review, and denies protected profiles again before a migration plan window can be created.
- Last verification: focused closure permission tests 3/3; related migration/system/ownership/Agent/product tests 250/250; full regression 876/876; solution build 0 warnings/errors; 316 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused authority hits 0; unconditional migration-button override hits 0.
- Blockers: no source blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: audit centralized uninstall, cache-cleanup, and startup-control preview methods. They should recheck the drawer action policy before building plans or pending operations, rather than relying on disabled WPF buttons and upstream Agent routing alone.

## Active Slice - 2026-07-15 Uninstall residue review availability

- Objective: keep post-uninstall residue review useful for ordinary applications while preventing the ordinary residue workflow from being offered to system applications or managed-root profiles whose ownership is not confirmed.
- Dependencies: `AppDrawerViewModel`, the existing system/ownership classification boundary, `ShowAppDrawer`, `ReviewSelectedUninstallResidueAsync`, and the post-official-uninstall continuation.
- Risks: equating residue review with uninstall-command availability, disabling legitimate external-uninstall recovery, relying only on a disabled WPF button, or allowing a stale/indirect call to reach residue scanning for protected profiles.
- Impact scope: one typed read-only residue-review availability in the drawer model, WPF binding and handler guard, focused tests, and records. No residue scanner candidate rules, quarantine authority, pipeline, worker, trust, confirmation, or system mutation change.
- Acceptance: system-category and managed-root ownership-pending profiles expose a plain disabled reason and cannot enter the ordinary residue review from UI or handler; ordinary profiles remain reviewable even without an uninstall command; button restoration after async work re-evaluates current selection policy instead of merely checking selection presence.
- Status: Completed with a visual Warn. Residue review now has a distinct read-only availability policy: ordinary applications remain reviewable after external uninstall even without an uninstall command, while system and managed-root ownership-pending profiles are denied in the drawer, handler entry, and async button restoration.
- Last verification: focused residue availability tests 3/3; related system/ownership/uninstall/product tests 200/200; full regression 873/873; solution build 0 warnings/errors; 315 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused authority hits 0.
- Blockers: no source blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: audit the migration-closure override in `ShowAppDrawer`. A stale `NeedsAttention` closure currently forces `DrawerMigrateButton.IsEnabled = true` after the system/ownership action policy is applied; preserve review visibility without granting migration review to protected profiles.

## Active Slice - 2026-07-15 App drawer stale-state invalidation

- Objective: ensure a filter or completed rescan that yields no applications removes every previous app conclusion, selection, technical-detail state, preview, and pending target.
- Dependencies: zero-profile `RefreshAppCatalog` branch, `ClearAppDrawer`, `ApplyDrawerActionHost`, technical-details toggle state, selected tile, and five action buttons.
- Risks: visually clearing text while retaining an executable pending target, leaving the previous category summary or `隐藏技术详情` label, treating an empty completed scan as an error, or breaking normal drawer selection.
- Impact scope: one typed empty-drawer presenter, one explicit collapsed technical-details state, centralized WPF reset wiring, focused tests, and records. No scan authority, operation descriptor, handler, pipeline, worker, trust, or system mutation change.
- Acceptance: empty filter and empty inventory both call the same clear path; category/publisher/location/size/residency/advice/technical details/selection/previews/pending targets/actions are reset; normal drawer open also starts with technical details collapsed; visible copy is path-free and distinguishes loading from completed empty inventory.
- Status: Completed with a visual Warn. Empty filter and zero-profile paths now share a typed clear state that removes selection, category/publisher/storage/residency/advice/technical evidence, collapses previews, invalidates pending targets, resets labels/tooltips, and disables all context actions; normal opens reset technical details too.
- Last verification: focused empty/technical tests 4/4; related product/cache/startup/uninstall/Agent/inventory tests 206/206; full regression 870/870; solution build 0 warnings/errors; 314 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused authority hits 0; technical button AutomationId hit 1; zero-inventory clear call hit 1.
- Blockers: no source blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: audit the separately wired `卸载后检查残留` button. `ShowAppDrawer` currently enables it unconditionally even for system-category and managed-root ownership-pending profiles; add a reviewed residue availability policy and plain disabled reason without changing residue execution.

## Active Slice - 2026-07-15 C-drive catalog summary consistency

- Objective: make the app-page C-drive summary use the exact same per-profile footprint policy as the `占 C 盘` filter while distinguishing main-program placement from attributed C-drive data/cache clues.
- Dependencies: `HasCDriveFootprint`, canonical install-root exclusion, `CDriveWritePaths`, MainWindow catalog refresh, and beginner summary copy.
- Risks: double-counting one app, treating a non-C entry as a C clue, calling all C-drive clues removable, exposing paths, or silently changing unrelated filters/actions.
- Impact scope: one structured read-only catalog summary presenter, a path-aware shared footprint predicate, MainWindow binding, focused tests, and records. No scanner mutation, cleanup, migration, uninstall, operation, pipeline, worker, or trust change.
- Acceptance: C-main, D-main-with-C-data, both, unknown-main-with-C-data, ordinary D, duplicate, descendant, and non-C clues produce truthful deduplicated counts; footprint total equals C-drive filter membership; visible text is path-free and does not claim releasable space; old private summary is removed.
- Status: Completed with a visual Warn. The app page now uses a structured summary that separates C/D main-program placement, C-drive data/cache app clues, and a deduplicated footprint total identical to `占 C 盘` filter membership; non-C field entries are ignored.
- Last verification: focused summary tests 3/3; related product/tile/storage/Agent/digest/catalog tests 232/232; full regression 867/867; solution build 0 warnings/errors; 312 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused authority hits 0; legacy summary method hits 0; summary AutomationId hit 1.
- Blockers: no source blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: audit empty/cleared app-catalog states. `ClearAppDrawer` does not reset the new category summary, and the zero-profile early return does not clear the drawer at all; prevent stale conclusions and enabled review affordances after a filter or rescan yields no applications.

## Active Slice - 2026-07-15 Uninstallable catalog safety consistency

- Objective: prevent the `可卸载` catalog filter from listing profiles whose drawer correctly denies uninstall review.
- Dependencies: `AppCatalogFilter.Uninstallable`, ordinary uninstall-command evidence, system-category deny, unknown managed-root ownership deny, and drawer action availability.
- Risks: treating command presence as execution readiness, duplicating policy that can drift, hiding ordinary uninstallable apps, or weakening system/unknown protection.
- Impact scope: one read-only uninstall-review availability predicate reused by catalog and drawer, system uninstall-preview refusal copy, focused tests, and records. No official-uninstaller trust, launcher, operation, pipeline, worker, or mutation change.
- Acceptance: ordinary profiles with commands remain listed; commandless profiles, system profiles, and ownership-pending managed-root profiles are excluded; publisher alone does not exclude an ordinary profile; filter membership equals drawer uninstall-action availability; system preview also refuses an ordinary uninstall plan.
- Status: Completed with the inherited visual Warn. `可卸载` membership now shares the drawer's system/ownership deny and command requirement; the system uninstall preview also refuses ordinary workflow generation; execution readiness remains a separate fail-closed boundary.
- Last verification: focused catalog/system/unknown tests 7/7; related product/system/unknown/Agent tests 188/188; full regression 864/864; solution build 0 warnings/errors; 311 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused test authority hits 0.
- Blockers: no source blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: audit the application catalog C-drive count and `占 C 盘` membership for the same main-program-versus-data consistency. The summary currently counts write-path entries directly while the filter uses `HasCDriveFootprint`; make them agree without treating every C-drive clue as removable.

## Active Slice - 2026-07-15 Truthful normal-application catalog filter

- Objective: stop labeling every low-confidence Normal application as office/study in the app catalog.
- Dependencies: `AppCatalogFilter`, Normal category predicate, WPF filter button tag/name/copy, and catalog product tests.
- Risks: changing which profiles are returned, breaking enum parsing/highlight state, or mixing a wording fix with category/action policy changes.
- Impact scope: one filter enum member, one unchanged predicate under a truthful name, one WPF button identity/copy, focused tests, and records. No scanner classification, drawer action, operation, pipeline, worker, trust, or mutation change.
- Acceptance: the visible button says `普通应用`; its stable id/tag and enum use the same truthful concept; it returns only Normal profiles exactly as before; `办公学习`/`OfficeStudy` no longer remain in the active app catalog implementation.
- Status: Completed with a visual Warn. The catalog now calls the unchanged Normal fallback group `普通应用`; enum, tag, stable button id, highlight parsing, and predicate use the same truthful concept.
- Last verification: focused catalog tests 3/3; ProductExperienceTests 169/169; full regression 862/862; solution build 0 warnings/errors; 310 non-generated C#/XAML files strict UTF-8 with zero replacement characters; active implementation legacy-term hits 0; focused method authority hits 0.
- Blockers: no source blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: audit the `Uninstallable` filter because command presence alone can include system or ownership-pending applications whose drawer correctly disables uninstall. Make catalog membership agree with reviewed action availability without weakening any deny.

## Active Slice - 2026-07-15 Software category evidence and confidence

- Objective: preserve why the read-only software scanner classified an application as normal, game, AI, development, system, or unknown, then explain that basis in one beginner-facing drawer line.
- Dependencies: `SoftwareProfile`, `SoftwareInventoryBuilder` classification rules, growth-profile cloning, app drawer presentation, and existing unknown/system read-only denies.
- Risks: treating publisher or install-path text as proof, leaking a full path into the beginner view, allowing classification confidence to grant mutation authority, or losing evidence during enrichment/fixture serialization.
- Impact scope: typed scanner-owned classification observation, category classifier output, one compact drawer subtitle, technical evidence, focused tests, and protocol records. No uninstall/migration/cache/startup planner, operation, pipeline, worker, trust, or mutation change.
- Acceptance: scanner profiles retain category, evidence source, matched rule, fallback state, and confidence; name/publisher/path-only signals have distinct confidence; ordinary fallback and unknown remain explicit; drawer text is concise and path-free; enrichment preserves the observation; existing system/unknown denies remain unchanged.
- Status: Completed with a visual Warn. Scanner profiles now retain category evidence source, matched rule, fallback state, and confidence; the drawer shows one path-free explanation line; growth enrichment preserves the observation; classification evidence grants no modifying authority.
- Last verification: focused classification/UI tests 7/7; related scanner/growth/system/unknown/product tests 218/218; full regression 861/861; solution build 0 warnings/errors; 310 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused test scope contains no operation/process/registry/file-mutation authority.
- Blockers: no source blocker; real WPF launch remains Warn because Computer Use reached Windows but `launch_app` timed out after the antivirus definition update.
- Exact next action: audit the `OfficeStudy` catalog filter because the scanner's low-confidence Normal fallback does not prove an application is for office or study. Make the beginner label/filter truthful without expanding action authority or guessing a use category.

## Active Slice - 2026-07-15 Unknown system-ownership review

- Objective: prevent an unknown-category profile in a Windows-managed install root from inheriting ordinary modifying actions while avoiding false system classification from publisher text alone.
- Dependencies: canonical install path, current Windows/Program Files roots, `SoftwareCategory.Unknown`, tile/drawer advice/actions, and existing exact Agent handoff availability.
- Risks: blocking ordinary Microsoft apps solely by publisher, silently changing category, exposing managed paths, or leaving one preview surface contradictory.
- Impact scope: one presentation-level ownership-review policy, tile/drawer summaries/actions, focused tests, and records. No scanner classification, model category, operation, planner, pipeline, worker, trust, or mutation change.
- Acceptance: unknown profiles under Windows/WindowsApps are labeled ownership-pending and read-only; technical details remain available; publisher alone and ordinary paths do not trigger; category stays Unknown; text is path-free and non-executable.
- Status: Completed with the inherited visual Warn. Unknown profiles under the current Windows root or Program Files `WindowsApps` now remain category Unknown but receive an ownership-pending tile, read-only drawer, and non-executable Agent advice; publisher text alone does not trigger protection.
- Last verification: focused/system/handoff tests 17/17; related product/drawer/Agent tests 196/196; full regression 854/854; solution build 0 warnings/errors; 309 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused test scope contains no operation/process/registry/file-mutation authority.
- Blockers: no source/test blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: audit application identity/category confidence at the scanner boundary so the UI can explain why a profile is Normal/System/Unknown instead of relying only on an enum. Add evidence/confidence without weakening the new read-only deny or treating publisher text as proof.

## Active Slice - 2026-07-15 System-application read-only boundary

- Objective: make every system-category application consistently read-only in the beginner drawer so Agent advice and action availability cannot contradict the gray system tile.
- Dependencies: `SoftwareCategory.SystemTool`, `CreateAgentAdvice`, five drawer actions, existing system tile status, and exact Agent handoff availability checks.
- Risks: enabling official uninstall merely because a command exists, treating system cache/startup like ordinary app data, offering migration while the action is disabled, or hiding technical evidence needed for review.
- Impact scope: system-category drawer recommendation/action presentation, focused tests, and records. No scanner, uninstall/migration/cache/startup planner, operation, pipeline, worker, trust, or mutation change.
- Acceptance: system apps always recommend retain/observe; uninstall, migration, cache cleanup, and startup control are disabled with plain reasons; technical details remain enabled; non-system actions are unchanged; all Agent handoffs remain non-executable.
- Status: Completed with the inherited visual Warn. System-category drawers now recommend retain, disable uninstall/migration/cache/startup regardless of scanned evidence, and keep technical details available; ordinary applications retain their existing actions.
- Last verification: focused/system-handoff tests 14/14; related product/drawer/Agent tests 193/193; full regression 851/851; solution build 0 warnings/errors; 308 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused test scope contains no operation/process/registry/file-mutation authority.
- Blockers: no source/test blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: audit unknown-category applications with system-like install roots or publishers so scanner uncertainty cannot inherit ordinary uninstall/migration/cache/startup availability. Prefer a conservative presentation classification; do not silently reclassify installed software without evidence.

## Active Slice - 2026-07-15 Compact application size explanation

- Objective: make the app drawer show main-program, data, cache, and recent-growth size evidence in compact beginner language without calling default zero a measurement or cache automatically deletable.
- Dependencies: `InstalledSizeBytes`, `DataSizeBytes`, `CacheSizeBytes`, `RecentGrowthBytes`, identified data/cache paths, existing drawer text wrapping, and exact growth evidence for deeper trend questions.
- Risks: presenting unknown as zero, presenting all data as cache, calling cache bytes safely releasable, overstating zero recent growth without a comparison state, or exposing paths.
- Impact scope: `SizeSummary`, focused tests, and records. No model, scanner, XAML, Agent routing, operation, pipeline, worker, trust, or mutation change.
- Acceptance: known values show all four categories; unidentified locations differ from identified locations whose size is unmeasured; default zero is never rendered as `0 B`; cache wording says identifiable rather than deletable; visible text is path-free and compact.
- Status: Completed with the inherited visual Warn. The drawer now reports main-program install, data, identifiable cache, and recent growth; zero defaults are rendered as unavailable evidence, and identified-but-unmeasured locations remain distinct from unidentified ones.
- Last verification: focused/neighbor size tests 19/19; related product/storage/growth tests 201/201; full regression 849/849; solution build 0 warnings/errors; 307 non-generated C#/XAML files strict UTF-8 with zero replacement characters; focused test scope contains no operation/process/registry/file-mutation authority.
- Blockers: no source/test blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: enforce a consistent read-only drawer boundary for system-category applications whose scan profile contains uninstall commands, cache paths, or startup components.

## Active Slice - 2026-07-15 Explicit application-grid C-drive labels

- Objective: replace the generic `需关注` app-grid tag with the specific beginner reason: main program on C, data writing to C, or C-drive ownership unknown.
- Dependencies: `InstallPath`, `CDriveWritePaths`, existing `AppTileStatus`, catalog filter/sort behavior, migration-closure UI override, and path-free tile accessibility text.
- Risks: exposing paths, calling a D-installed app itself installed on C, downgrading migration-closure attention, changing catalog order/filter semantics, or labeling any C-drive presence as safe/unsafe without evidence.
- Impact scope: tile short-tag presentation, focused tests, and records. No XAML, scanner, drawer, Agent, operation, pipeline, worker, trust, or mutation change.
- Acceptance: C-main, D-main-with-C-data, unknown-main-with-C-clue, ordinary D, growth, resident, system, and closure-override labels remain distinct; visible/accessibility text stays path-free; status/filter/sort behavior is unchanged.
- Status: Completed with the inherited visual Warn. Attention tiles now say `主程序在 C 盘`, `数据写入 C 盘`, or `C 盘线索待确认`; growth, resident, system, risk, filter, sort, and migration-closure override behavior remain unchanged.
- Last verification: focused labels 5/5; related product/catalog/storage/growth tests 198/198; full regression 846/846; solution build 0 warnings/errors; 306 non-generated C#/XAML files strict UTF-8 with zero replacement characters; test scope contains no operation/process/registry/file-mutation authority.
- Blockers: no source/test blocker; real WPF launch remains under the recorded Computer Use Warn.
- Exact next action: audit the application drawer size summary against the promised install/data/cache/growth fields. Keep it compact, distinguish unknown from zero, and do not infer deletable bytes.

## Active Slice - 2026-07-15 Installer report program/data placement

- Objective: make the post-install report tell a beginner where the main program landed separately from any new C-drive data/write candidates.
- Dependencies: unique `AddedSoftware`, canonical install path, attributed profile data/cache/log/write paths, footprint-only candidates, existing report cards/Agent explanation, and existing candidate-to-app handoff.
- Risks: exposing paths, attributing concurrent footprint changes to the installer, treating the install tree as mutable data, recommending repeat migration for a D-installed program, or generating an operation from a report conclusion.
- Impact scope: one internal read-only placement observation, install-report/Agent wording, focused tests, and records. No XAML, installer execution, snapshot capture, application handoff, operation, pipeline, worker, trust, or mutation change.
- Acceptance: unique D/owned-C, unique D/unattributed-C, unique C/external-data, and no-unique-software states are distinct; counts are canonical and deduplicated; primary text is path-free; D main program never gets a repeat-migration recommendation; all actions remain non-executable.
- Status: Completed with a fresh real-launch Warn. The report now separates main program C/D/unknown placement from owned and unattributed C-drive candidates, and Agent next steps preserve those distinctions.
- Last verification: focused 4/4; related installer/report/product tests 261/261; full regression 842/842; solution build 0 warnings/errors; 305 non-generated C#/XAML files strict UTF-8 with zero replacement characters; scoped operation/process/registry/file-mutation authority hits 0.
- Blockers: no source/test blocker. Computer Use `launch_app` timed out and a passive app refresh found no OMNIX window, so no screenshot is claimed.
- Exact next action: make application-grid short labels state whether the main program is on C, only data writes to C, or ownership is unknown; keep the existing status/risk decision and do not expose paths.

## Active Slice - 2026-07-15 Main-program versus C-drive data location

- Objective: stop telling beginners that a D-installed application is simply `位置合理` when it still has attributed C-drive writes; distinguish the main program location from data/cache footprint and the appropriate next decision.
- Dependencies: `SoftwareProfile.InstallPath`, deduplicated `CDriveWritePaths`, cache evidence, `AppDrawerViewModel`, Agent exact location answers, and existing migration action availability/closure presentation.
- Risks: exposing paths, calling every C-drive write a cache, enabling an unsupported data redirection operation, telling users to migrate an already D-installed main program, or turning a location question into migration consent.
- Impact scope: path-free app drawer location/migration/advice presentation, exact location answer reuse, focused tests, and protocol records. No XAML, scanner, closure store, operation, pipeline, confirmation, worker, trust, or mutation behavior changes.
- Acceptance: D/no-C, D/with-C, C/with-C, unknown/with-C, and system app conclusions are distinct; C-drive counts are deduplicated; D/with-C says main program does not need repeat migration and data/cache needs separate review; migration remains disabled when no safe redirection plan exists; all primary text is path-free; exact `装在哪里` remains details-only and non-executable.
- Status: Completed with the inherited visual Warn. The drawer and exact Agent location answer now distinguish the main program from attributed C-drive data/cache writes; D-installed applications never gain a repeat-main-program migration action, and unsupported data redirection stays explicitly unavailable.
- Last verification: focused 5/5; related app/Agent/growth/migration tests 251/251; full regression 838/838; solution build 0 warnings/errors; 304 non-generated C#/XAML files strict UTF-8 with zero replacement characters; scoped operation/process/file-mutation authority hits 0.
- Blockers: no source/test blocker; visual proof remains under the existing Computer Use launch Warn.
- Exact next action: audit the installer post-install change report for the same beginner distinction between where the main program landed and where new C-drive data appeared. Prefer a path-free conclusion and existing app handoff; do not claim application ownership or redirect data without evidence.

## Active Slice - 2026-07-15 Application growth explanation and prevention

- Objective: answer exact named-app questions such as `为什么越来越大` or `为什么还写 C 盘` from bounded local trend evidence, clearly separating one-time space relief from preventing future C-drive growth.
- Dependencies: exact current profile resolution, successful health-scan snapshot count, `RecentGrowthBytes`, aggregate C-drive/cache location counts, shared read-only health gate, and the existing exact app drawer handoff.
- Risks: calling one snapshot a trend, exposing local paths, treating a C-drive write location as proven growth, turning a why-question into cleanup/migration consent, triggering a full scan for generic or explicit-operation wording, or retaining a stale profile after scanning.
- Impact scope: one aggregate Core observation, bounded target policy, Agent presentation, MainWindow read-only orchestration/state, focused tests, and protocol records. No XAML, operation, handler, pipeline, confirmation, process, registry, service, task, installer, or mutation behavior changes.
- Acceptance: only a unique exact app growth/write question loads evidence; explicit cleanup/migration/uninstall/startup and generic wording skip; zero/positive/insufficient/unavailable states remain distinct; output contains counts and formatted bytes but no paths; one-time and prevention steps are separately labeled; action remains details-only and non-executable; MainWindow re-resolves after any scan.
- Status: Completed with the inherited visual Warn. Unique exact app growth/write questions now prepare or reuse the bounded read-only health baseline, re-resolve the app, and explain immediate relief versus prevention from aggregate comparison evidence. Explicit operations and generic/ambiguous targets skip this path.
- Last verification: focused growth tests 7/7; related Agent/health/growth/inventory/product tests 286/286; full regression 833/833; solution build 0 warnings/errors; 303 non-generated C#/XAML files strict UTF-8 with zero replacement characters; growth model/presenter forbidden-authority hits 0.
- Blockers: no source/test blocker. No XAML changed; fresh WPF proof retains the recorded post-antivirus Computer Use launch Warn.
- Exact next action: audit the app drawer's `装在哪里` and migration advice for applications already installed on D but still writing to C. Make the beginner conclusion distinguish main program location, data/cache location, and whether a prior migration is closed, without exposing paths or auto-generating a migration operation.

## Active Slice - 2026-07-15 Exact Agent application-action handoff

- Objective: when a beginner explicitly asks about uninstalling, migrating, cleaning cache, or managing startup for one uniquely resolved application, let the Agent open that exact application and prepare the matching existing review surface instead of leaving the user to choose another button.
- Dependencies: exact profile mention resolution, `AgentConversationReply`, `ResolveAndOpenAppTargetAsync`, four existing application preview flows, and all current final confirmation/pipeline/readiness gates.
- Risks: treating natural language as execution consent, acting on a stale or duplicate app identity, opening the wrong preview, bypassing production signer readiness, or duplicating operation construction inside the Agent.
- Impact scope: one non-executable handoff enum/property, precise action labels, reusable MainWindow preview methods, focused Core/WPF source tests, and protocol records. No descriptor, handler, pipeline, worker, trust, confirmation, scanner, or mutation behavior changes.
- Acceptance: exact explicit app intent maps to one preview kind; location/troubleshooting/general app questions remain details-only; stale/ambiguous targets refuse; clicking the Agent action re-resolves current inventory before preparing a preview; the reply contains no operation and `CanExecuteDirectly` remains false; every real action still requires its existing confirmation and production gates.
- Status: Completed with the inherited real-launch visual Warn. Exact supported app-action questions now use precise labels, re-resolve current inventory, and prepare the same uninstall, migration, cache, or startup review as the manual drawer buttons. Details/troubleshooting, unavailable actions, and system apps remain details-only.
- Last verification: focused handoff tests 12/12; related Agent/app/uninstall/migration tests 257/257; corrected uninstall static contract plus focused tests 13/13; full regression 826/826; solution build 0 warnings/errors; 301 non-generated C#/XAML files strict UTF-8 with zero replacement characters; Agent Core authority hits 0.
- Blockers: no source/test blocker. No XAML changed; fresh WPF proof remains Warn because the two post-antivirus Computer Use `launch_app` attempts already timed out and passive refresh found no OMNIX window.
- Exact next action: audit Agent application advice for one-time C-drive growth versus prevention guidance. Prefer automatic read-only growth evidence and exact app drawer handoff; do not infer a cleanup, migration, or startup action when the user only asks why an app is growing.

## Active Slice - 2026-07-15 Beginner-safe operation error boundaries

- Objective: stop ten raw operation/policy/validation error strings from reaching beginner-visible MainWindow controls while retaining the underlying failure result and correct recovery route.
- Dependencies: startup disable/restore, quarantine purge validation/execution, uninstall residue policy/execution, C-drive cleanup policy/execution, and existing Timeline/app rescan flows.
- Risks: leaking affected paths or implementation vocabulary, calling a possible partial operation unchanged, hiding a pre-execution safety refusal, losing the correct current-state review destination, or weakening handler/pipeline failures.
- Impact scope: ten MainWindow presentation branches, focused source tests, and protocol records. No operation descriptor, policy, handler, pipeline, confirmation, storage, mutation, privilege, or worker behavior changes.
- Acceptance: no `result.Error`, `policy.Error`, or `validation.Error` reaches MainWindow UI; pre-execution refusal states no change; post-pipeline/restore states completion unknown; copy names the correct app rescan or Timeline review; all failures remain failures.
- Status: Completed with a real-launch visual Warn. All ten primary UI branches now use workflow-specific, path-free conclusions; pre-execution refusals state no change and post-attempt failures require app rescan or Timeline review.
- Last verification: focused boundary tests 3/3; related startup/quarantine/residue/Timeline/product tests 189/189; full regression 814/814; solution build 0 warnings/errors; 300 non-generated C#/XAML files strict UTF-8 with zero replacement characters; all-App raw operation/policy/validation error hits 0.
- Blockers: no source/test blocker. After the user confirmed updated antivirus definitions, two Computer Use `launch_app` requests still timed out and a passive app refresh found no OMNIX window; real antivirus/UI proof therefore remains Warn and no fallback UI automation was used.
- Exact next action: audit the six primary navigation workflows for controls that still stop at explanation/plan-only behavior. Choose the highest-value missing safe-pipeline connection, add a bounded acceptance test, and do not enable unsigned uninstall or migration execution.

## Active Slice - 2026-07-15 Beginner-safe WPF failure boundaries

- Objective: stop raw local exception details from reaching beginner-visible WPF text while preserving truthful retry and partial-state guidance for six key workflow failures.
- Dependencies: allowlisted system-tool opening, manual install snapshots, quarantine purge, timeline restore, uninstall residue review, C-drive quarantine cleanup, and the existing status/message panels.
- Risks: leaking local paths/registry/system messages, falsely claiming no change after a partially completed operation, hiding a failure as success, removing the useful retry destination, or creating an unreviewed ad hoc diagnostic store.
- Impact scope: six `MainWindow` exception catches, one static presentation/source guard, tests, and protocol records. No handler, pipeline, operation, scanner, storage, process, privilege, or system-mutation behavior changes.
- Acceptance: no `ex.Message` reaches MainWindow UI; read-only/open failures say no modification; potentially partial operations say completion is unknown and direct the user to rescan/reload the relevant page; residue failure is not called clean; all copy is path/registry/identifier free.
- Status: Completed with a runtime-fault visual Warn. All six catches now use workflow-specific, path-free conclusions. Open/snapshot failures state confirmed no-modification; purge/restore/residue/cleanup failures state that completion is unknown and direct the user to authoritative current-state review.
- Last verification: focused boundary tests 2/2; related install/quarantine/restore/residue/product tests 186/186; full regression 811/811; solution build 0 warnings/errors; 299 non-generated C#/XAML files strict UTF-8 with zero replacement characters; all-App raw exception-message hits 0; unsafe fallback identifier hits 0.
- Blockers: no source/test blocker. No XAML changed; the six failure branches were not fault-injected in a real WPF run, so visible runtime proof is Warn rather than claimed.
- Exact next action: classify and replace the ten remaining beginner-visible raw `OperationResult.Error`, policy error, or validation error uses in MainWindow. Preserve distinct safety-refusal versus possible-partial-completion guidance and keep technical detail out of the primary UI.

## Active Slice - 2026-07-15 Bounded application runtime observation

- Objective: let a unique exact app freeze/resource question automatically collect a short read-only runtime sample and explain current aggregate activity without exposing process identity or offering process control.
- Dependencies: current `SoftwareProfile.RunningProcesses`, display-icon executable hint, exact normalized name matching, a 350 ms/32-process bounded sampler, aggregate working set, coarse CPU activity, exact Agent target resolution, and MainWindow async orchestration.
- Risks: matching an unrelated similarly named process, exposing process names/ids/paths/command lines, treating one short sample as the cause of a freeze, claiming NotRunning when no trustworthy identity exists, blocking the UI, or introducing kill/suspend/priority/process-launch authority.
- Impact scope: Core runtime observation model/interface, Win32 injected aggregate reader/probe, named resource wording, optional Agent evidence, MainWindow read-only orchestration, focused tests, and protocol records. No process lifecycle, settings, operation pipeline, file, registry, service, task, installer, or cloud behavior changes.
- Acceptance: only a unique exact app freeze/resource question observes; explicit operations, generic/vague/crash-only questions skip it; matching is exact and profile-derived; sample/process count are bounded; Available/NotRunning/Unavailable stay distinct; visible output contains counts/aggregate memory/coarse CPU and its limits only; all authority remains read-only/non-executable.
- Status: Completed with the inherited visual Warn. Unique exact app freeze/resource questions now run one 350 ms/32-process maximum aggregate sample after inventory, distinguish Available/NotRunning/Unavailable, and explain current memory/coarse CPU without exposing identity or claiming root cause.
- Last verification: focused runtime/troubleshooting tests 37/37 including the real Windows reader; related Agent regression 152/152; full regression 809/809; solution build 0 warnings/errors; 298 non-generated C#/XAML files strict UTF-8 with zero replacement characters; runtime forbidden-authority and Core private-field hits 0.
- Blockers: no source/test blocker. No XAML changed, and fresh WPF screenshot proof retains the previously recorded Computer Use launch Warn.
- Exact next action: replace all six beginner-visible raw `Exception.Message` branches in MainWindow with path-free typed fallback copy. Preserve diagnostic detail only in a bounded local technical record if an existing logging boundary supports it; do not add one ad hoc.

## Active Slice - 2026-07-15 Bounded application crash-log observation

- Objective: let an exact app crash/hang question automatically collect recent Windows Application-log evidence while retaining only availability, bounded count, and latest time for beginner presentation.
- Dependencies: `System.Diagnostics.Eventing.Reader`, fixed Application log query, approved provider/event-id pairs, bounded candidate count/window, app-token derivation from current profile, exact target resolution, optional Agent evidence, and MainWindow read-only orchestration.
- Risks: retaining formatted messages or event properties, exposing user paths/provider identifiers, broad substring false matches, blocking the UI on an unbounded log scan, querying logs for generic/system/explicit-operation questions, or turning crash evidence into process/tool/mutation authority.
- Impact scope: Core observation model/interface, Win32 injected reader/probe, one cached package reference, exact crash target policy, Ask orchestration, Agent presentation, focused tests, and records. No log clear/export, process control, external tool launch, operation pipeline, or system mutation is added.
- Acceptance: only a unique exact app crash/hang target is observed after inventory; query is Application-only, 24-hour/128-candidate bounded, fixed provider/id reviewed, and never formats descriptions; model contains no message/path/property/provider/id; Available/NotFound/Unavailable remain distinct; answer shows count/time and evidence limits; all authority stays read-only/non-executable.
- Status: Completed with a visual Warn. Exact named-app crash/hang questions now read one bounded local Application-log window after inventory, reduce it to Available/NotFound/Unavailable plus count/latest time, and explain both the evidence and its limits. Generic, vague, ambiguous, and explicit-operation questions do not query logs.
- Last verification: focused crash/troubleshooting tests 19/19; related Agent regression 121/121; final full regression 782/782; solution build 0 warnings/errors; 294 non-generated C#/XAML files strict UTF-8 with zero replacement characters; crash model/probe forbidden authority and private-field hits 0.
- Blockers: no source/test blocker. After the user reported updated antivirus definitions, Computer Use `launch_app` still timed out and a passive refresh found no OMNIX process window, so fresh WPF visual proof remains Warn and no fallback UI automation was used.
- Exact next action: add a bounded, read-only aggregate runtime observation for a unique exact app freeze/resource question. Retain only availability, matched process count, aggregate memory, and a short sampled CPU conclusion; expose no process names/ids/paths and add no process-ending authority.

## Active Slice - 2026-07-15 Application-specific troubleshooting answers

- Objective: turn exact application crash/freeze/vague-abnormal questions into symptom-specific, evidence-bounded Agent guidance instead of generic drawer advice or an immediate generic Event Viewer route.
- Dependencies: pre-inventory `QuestionNeedsSoftwareInventory`, profile mention resolution, `ApplicationReply` action priority, aggregate running/background evidence, existing app drawer handoff, and allowlisted System Tool wording.
- Risks: scanning inventory for generic `软件闪退` or system blue-screen questions, exposing process/service/task names or paths, claiming a root cause without crash/performance logs, overriding explicit uninstall/cache/migration intent, or launching/ending a process from the answer.
- Impact scope: one bounded named-app-crash predicate, three presentation branches, focused tests, and protocol records. No event-log reader, process control, system-tool launcher, operation, cloud call, or system mutation is added.
- Acceptance: likely named-app crash wording hydrates inventory; generic crash/system questions do not; exact crash/freeze/vague answers state evidence limits, expose aggregate counts only, keep the exact app drawer action, and explain how to request protected Event Viewer/Task Manager viewing; explicit operations keep priority; all replies remain path-free and non-executable.
- Status: Completed with the inherited visual Warn. Likely named-app crash wording now hydrates inventory; exact app crash/freeze/vague questions produce symptom-specific, aggregate-only answers and retain the exact drawer handoff. Generic crash/system questions skip inventory, explicit action intent keeps priority, and system tools remain separate reviewed follow-up questions.
- Last verification: focused Agent tests 55/55; related Agent/inventory/product/target tests 242/242; full regression 773/773; solution build 0 warnings/errors; Agent presenter forbidden authority hits 0; 290 non-generated C#/XAML files are strict UTF-8 with zero replacement characters. No XAML changed; fresh visual proof retains the existing Computer Use Warn.
- Blockers: none.
- Exact next action: add a bounded read-only Windows Application event observation for an exact app crash question. Correlate only reviewed crash event ids/providers, return count/latest time/availability, never expose message text or paths, and keep the Agent non-executable.

## Active Slice - 2026-07-15 Natural-language whole-computer diagnosis

- Objective: recognize explicit whole-computer health/体检/整体优化 questions and automatically await the existing full read-only health scan instead of falling into software-only General handling.
- Dependencies: Agent intent ordering, full-diagnosis phrase scope, `QuestionNeedsFullHealthScan`, shared health gate, `DiagnosisSkillReply` evidence presentation, and the existing lightweight machine observation policy.
- Risks: turning `电脑为什么卡` into an expensive full scan, misrouting `C盘怎么优化` or `微信需要怎么优化`, running software inventory before the full scan, inventing a score when health is absent, or adding execution authority to the Agent handler.
- Impact scope: one new Agent intent, a bounded phrase set, a renamed/generalized health evidence policy, shared diagnosis presentation, focused tests, and protocol records. No scanner, operation, pipeline, cloud, process, or system mutation implementation changes.
- Acceptance: explicit whole-computer wording maps to SystemDiagnosis and requests missing full health; C-drive keeps CDrive/full health; performance/hardware keep lightweight probes; possible app text keeps inventory; completed summary is reused; handler awaits before answering; all replies remain path-free and non-executable.
- Status: Completed with the inherited visual Warn. Explicit whole-computer wording now maps to SystemDiagnosis, skips the separate inventory/lightweight probes, awaits the shared full read-only health gate when evidence is missing, and answers from the refreshed real summary. Neighboring C-drive, machine-health, hardware, and app questions retain narrower scopes.
- Last verification: focused routing/evidence tests passed 78/78; corrected real process-image regression passed 1/1; final full regression passed 763/763; solution build passed with 0 warnings/errors; Agent Ask authority hits 0; 289 non-generated C#/XAML files are strict UTF-8 with zero replacement characters. The earlier post-antivirus Computer Use launch Warn remains; no new XAML was added.
- Blockers: none.
- Exact next action: audit application-specific troubleshooting such as `微信闪退` / `某应用有点奇怪`; keep exact profile resolution, add symptom-specific plain-language evidence/next steps, and do not infer a fault root cause or launch Event Viewer without confirmation.

## Active Slice - 2026-07-15 No-scan Agent greetings and capability help

- Objective: prevent obvious greetings, thanks, and capability/help questions from triggering a full software inventory while retaining automatic inventory for possible unknown application mentions.
- Dependencies: `QuestionNeedsSoftwareInventory`, `Answer` routing before profile resolution, the General reply, exact whole-question matching, and the existing unknown-app hydration behavior.
- Risks: making the no-scan matcher broad enough to suppress evidence for `你好，微信最近有点奇怪`, weakening exact-profile resolution, adding a fake conversational AI claim, or returning an executable capability promise.
- Impact scope: one closed exact phrase set, one local capability reply, focused tests, and protocol records. No scanner implementation, cloud call, application identity rule, operation, process launch, or system mutation changes.
- Acceptance: empty/greeting/thanks/help/capability questions do not request inventory; unknown app-like General questions still do; mixed greeting plus app text still scans; the capability answer is immediate, honest, local, path-free, and non-executable.
- Status: Completed. Exact greeting/thanks/help/capability questions now return an immediate local capability reply and skip inventory. Mixed or otherwise unclassified text still hydrates inventory, so unknown installed-app mentions retain exact-profile resolution.
- Last verification: focused Agent/inventory tests passed 50/50; full regression passed 751/751; solution build passed with 0 warnings/errors; 288 non-generated C#/XAML files are strict UTF-8 with no replacement characters. No XAML, scanner, operation, or system-authority code changed.
- Blockers: none.
- Exact next action: add a distinct full-diagnosis intent for natural-language requests such as `帮我体检电脑` and `电脑整体需要怎么优化`; reuse the shared health gate and do not confuse those requests with lightweight performance observation or software-only General questions.

## Active Slice - 2026-07-15 Automatic system-diagnosis skill evidence

- Objective: make the `系统诊断与体检` Agent skill prepare its own full read-only health evidence instead of asking a beginner to understand and perform a separate homepage scan.
- Dependencies: `AgentSkillCategory.SystemDiagnosis`, `DiagnosisSkillReply`, `AgentSkillAction_Click`, `ReadOnlyEvidenceLoadGate`, the existing health scan cancellation/failure behavior, and the first-visible Agent response panel.
- Risks: triggering a full disk scan for unrelated skills, starting a second scan while Home or a C-drive question is already scanning, caching cancellation/failure, converting a skill click into cleanup consent, or leaking scanner errors/paths.
- Impact scope: one pure skill evidence policy, one handler await, focused tests, and protocol records. The existing full scan remains read-only; no recommendation execution, quarantine, uninstall, migration, installer, service/registry/task write, or file mutation is added.
- Acceptance: only System Diagnosis with missing health evidence requests the full scan; existing evidence skips it; Home/C-drive/skill share one in-flight gate; failed/cancelled scans remain retryable; the final reply uses refreshed evidence; the skill handler stays free of execution authority and re-enables its button.
- Status: Completed with the inherited visual Warn. Clicking System Diagnosis with no health summary now awaits the shared full read-only scan before replying; existing evidence skips the scan, all other skill categories skip it, and failed/cancelled work retains the gate's retry behavior.
- Last verification: focused/related diagnosis, Agent, health, and product tests passed 223/223; full regression passed 744/744; solution build passed with 0 warnings/errors. Static source authority found zero forbidden calls in the skill handler; 288 non-generated C#/XAML files are strict UTF-8 and 16 XAML files parse. No XAML or event binding changed.
- Blockers: no source/test blocker; fresh WPF screenshots retain the recorded Computer Use launch Warn.
- Exact next action: audit the Agent's eager software-inventory policy for generic greetings/capability questions; preserve automatic inventory for unknown application mentions without scanning for obvious non-diagnostic conversation.

## Active Slice - 2026-07-15 Agent lightweight machine observation

- Objective: let hardware/configuration and machine-health questions automatically collect bounded local evidence without requiring or pretending to perform a full C-drive diagnosis.
- Dependencies: `WindowsMachineHealthProbe`, `MachineHealthObservation`, hardware availability states, Agent intent routing, `ReadOnlyEvidenceLoadGate`, full-scan observation reuse, and the first-visible Agent response panel.
- Risks: inventing an overall score without disk evidence, running a full scan for CPU/memory questions, exposing process names or identifiers, repeatedly probing unavailable hardware, mixing stale full-scan and fresh lightweight evidence, or letting observation become process/power authority.
- Impact scope: reusable machine-dimension presentation, optional Agent evidence, one shared observation gate, Ask/hardware-skill orchestration, focused tests, and protocol records. No process termination, power action, registry/service/task write, file scan/mutation, installer, cleanup, migration, or operation pipeline is added.
- Acceptance: hardware and machine-health intents request lightweight evidence only when absent; the hardware skill does the same; one in-flight probe is shared and full C-drive scans seed the same cache; Agent can answer from machine evidence without a fake disk score; unavailable/not-present states remain explicit; visible text is path/identifier free; all replies stay non-executable.
- Status: Completed with a visual Warn. Hardware/configuration and machine-health questions plus the hardware skill now await one shared bounded machine observation. Full C-drive diagnosis refreshes the same cache; lightweight answers never construct a disk-backed score, and unavailable/not-present evidence remains explicit.
- Last verification: focused tests 21/21; related Agent/health/product tests 236/236; full regression 735/735; solution build 0 warnings/errors. Static gates passed for 287 non-generated strict UTF-8 C#/XAML files, 16 parsed XAML files, 120 resolved event bindings, 277 unique literal AutomationIds, and zero forbidden machine-observation authority hits. A post-antivirus Computer Use launch still timed out and no `Css.App` process/window remained, so fresh visual proof is Warn rather than claimed.
- Blockers: no source/test blocker; real WPF visual proof remains subject to the existing Computer Use availability Warn.
- Exact next action: audit the `系统诊断与体检` Agent skill, which still asks the beginner to perform a manual homepage scan; connect it to the existing shared read-only health gate without adding mutation authority.

## Active Slice - 2026-07-15 Automatic undo-center history loading

- Objective: make the Undo Center truthfully automatic on first entry, keep an explicit refresh action, deduplicate repeated navigation, and prevent timeline-store failures from exposing local exception details.
- Dependencies: `ShowPage`, `LoadTimeline_Click`, all post-operation `LoadTimelineAsync` refresh calls, timeline/quarantine stores, `ReadOnlyEvidenceLoadGate`, and existing restore confirmation.
- Risks: caching a failed/empty-unknown load as success, suppressing post-operation refresh, allowing two loads to race, disabling the refresh button permanently, or changing restore/purge authority.
- Impact scope: timeline read orchestration and beginner copy. Restore and permanent-purge execution paths remain unchanged.
- Acceptance: first Timeline navigation ensures one read-only load; repeated navigation after success does not reload; manual/post-operation calls force refresh but join in-flight work; button says `重新加载`; failures are path-free and retryable; no restore/purge runs from loading.
- Status: Completed with a visual Warn. First Timeline navigation now ensures one automatic read; repeated entry reuses success, while the visible `重新加载` action and all existing post-operation calls force refresh and join any in-flight work. Failures remain retryable and path-free.
- Last verification: focused timeline/quarantine tests passed 11/11; related timeline/quarantine/product tests passed 212/212; full regression passed 723/723; solution build passed with 0 warnings/errors. Static gates passed for 285 strict UTF-8 C#/XAML files, 16 parsed XAML files, 120 resolved event bindings, zero duplicate literal AutomationIds, and zero timeline load authority/privacy hits.
- Blockers: no source/test blocker; real WPF visual proof retains the existing Computer Use availability Warn.
- Exact next action: audit whether hardware/configuration and machine-health Agent questions can collect a bounded lightweight read-only observation without forcing a full C-drive scan; preserve explicit availability states and no execution authority.

## Active Slice - 2026-07-15 Agent-triggered C-drive read-only diagnosis

- Objective: when a beginner explicitly asks why C drive is full and no current health evidence exists, let Computer Agent await the existing read-only diagnosis instead of returning another manual navigation instruction.
- Dependencies: `RunScanAsync`, current drive-root normalization, cancellation UI, health summary application, software-inventory hydration, a shared in-flight evidence gate, and the C-drive Agent intent.
- Risks: running a full disk scan for unrelated questions, starting two scans concurrently, treating a failed/cancelled scan as completed evidence, leaking scanner exception paths, or allowing a question to cross into cleanup execution.
- Impact scope: read-only scan orchestration, a reusable evidence-load gate, C-drive Agent evidence policy, failure copy, and focused tests. No recommendation execution, cleanup, quarantine, migration, uninstall, registry/service/task write, installer, or user-file mutation is added.
- Acceptance: only a C-drive intent with missing health requests a scan; homepage refresh and Agent entry share one in-flight scan; success is cached for Agent evidence, failure/cancellation can retry; scanner failures remain path-free; final Agent reply uses the refreshed summary; all execution authority stays outside the question handler.
- Status: Completed with a visual Warn. Explicit C-drive questions with no current health evidence now await a shared read-only full diagnosis before answering. Homepage refresh and Agent entry deduplicate in-flight work; failed/cancelled scans remain retryable and no longer expose exception details.
- Last verification: focused health/inventory gate tests passed 14/14; related scan/Agent/product tests passed 226/226; full regression passed 722/722; solution build passed with 0 warnings/errors. Static gates passed for 284 strict UTF-8 C#/XAML files, 16 parsed XAML files, 120 resolved event bindings, zero duplicate literal AutomationIds, zero Agent mutation-authority hits, and zero scanner exception-detail leaks.
- Blockers: no source/test blocker; real WPF visual proof retains the existing Computer Use availability Warn.
- Exact next action: make Undo Center's existing first-entry load deduplicated and truthful, retain explicit refresh after operations, and remove exception-detail leakage.

## Active Slice - 2026-07-15 Agent automatic read-only evidence hydration

- Objective: let Computer Agent answer application, C-drive ownership, startup/background, migration, and uninstall questions from automatically prepared local software evidence instead of telling a beginner to open Apps and run a scan.
- Dependencies: `AgentConversationPresenter` intent ordering, `SoftwareInventoryLoadGate`, `EnsureSoftwareInventoryLoadedAsync`, the Ask button, the process/service skill card, and existing path-free inventory failure handling.
- Risks: scanning for unrelated settings/hardware questions, starting duplicate scans, turning a question into mutation consent, leaving the Ask button disabled after failure, or allowing an inventory exception/path to leak into the Agent answer.
- Impact scope: a pure evidence-needs policy, two Agent UI handlers, focused tests, and status copy. The shared inventory scan remains read-only; no operation, process launch, installer, uninstall, migration, cleanup, registry/service/task write, or file mutation is added.
- Acceptance: empty/settings/tool/hardware/install/restore questions do not request inventory; software/C-drive/startup/migration/uninstall/general questions do; related skill cards await the shared gate before answering; repeated/concurrent requests reuse existing gate behavior; the UI re-enables controls; final answers remain `CanExecuteDirectly=false` and path-free.
- Status: Completed with a visual Warn. Evidence-dependent questions and the process/service skill now await the shared deduplicated software inventory before answering; unrelated questions do not scan. Controls are restored in `finally`, and the two handlers remain free of operation or mutation authority.
- Last verification: focused policy/orchestration tests passed 13/13; related Agent/inventory/product tests passed 222/222; full regression passed 713/713; solution build passed with 0 warnings/errors. Static gates passed for 282 strict UTF-8 C#/XAML files, 16 parsed XAML files, 120 resolved event bindings, zero duplicate literal AutomationIds, and zero dangerous-call hits in the Agent handlers.
- Blockers: none for source and automated tests; real WPF screenshots remain subject to the existing Computer Use launch issue.
- Exact next action: connect explicit C-drive questions to the existing read-only health diagnosis when current evidence is missing; share in-flight work with the homepage scan and keep failures path-free.

## Active Slice - 2026-07-15 Beginner-first installer monitoring

- Objective: remove the three-step manual snapshot workflow from the normal installation path because `PrepareInstaller_Click` already captures before evidence, runs the confirmed installer, captures after evidence, and renders the change report automatically.
- Dependencies: the existing installer capability gate, final-consent window, production execution coordinator, manual fixture controls, install-diff presentation, and GUI smoke contract.
- Risks: hiding evidence needed by diagnostics, breaking the isolated fixture smoke, making the automatic path look like execution without consent, or moving technical controls without a stable UIAutomation target.
- Impact scope: installation-page information architecture, one focused product contract test, and the fixture-only GUI smoke entry. No installer, file, registry, service, task, cleanup, uninstall, migration, or pipeline authority changes.
- Acceptance: beginner copy states that normal installation records changes automatically; `PrepareInstallerButton` remains the primary action; the three manual controls live only inside a default-collapsed advanced expander with a stable AutomationId; the fixture smoke explicitly expands that region; automatic before/after capture remains unchanged.
- Status: Completed with a visual Warn. The normal page now states that installation changes are recorded automatically; the manual three-step comparison is available only inside a default-collapsed advanced diagnostic expander. The primary production installer flow and its evidence authority were unchanged.
- Last verification: focused contract tests passed 2/2; related product/installer tests passed 240/240; full regression passed 700/700; solution build passed with 0 warnings/errors. Static gates passed for 282 strict UTF-8 C#/XAML files, 16 parsed XAML files, 120 resolved event bindings, zero duplicate literal AutomationIds, and zero PowerShell parse errors in the updated fixture smoke.
- Blockers: real WPF visual proof remains Warn because Computer Use could not launch the app earlier in this turn.
- Exact next action: audit the Timeline page's manual `加载时间线` requirement and determine whether first navigation can safely load the existing read-only history automatically while preserving explicit refresh and restore confirmation.

## Active Slice - 2026-07-15 Automatic beginner app inventory loading

- Objective: remove the beginner-facing requirement to understand and click `扫描应用` before the application grid or a C-drive app handoff becomes useful.
- Dependencies: `ShowPage`, the existing read-only `ScanSoftwareProfilesAsync` inventory, migration-closure refresh, app catalog rendering, manual refresh button, and the typed C-drive `OpenCDriveApps` handoff.
- Risks: starting duplicate expensive scans when navigation repeats, overwriting a selected filter after an asynchronous scan, leaking scanner exceptions into beginner text, treating an empty successful inventory as “not loaded,” or making page navigation mutate software/system state.
- Impact scope: one deduplicated lazy-load task/state in `MainWindow`, beginner copy, the manual refresh label, the C-drive app handoff, and focused tests. The scanner remains read-only; no uninstall, cleanup, migration, process launch, registry/service/task write, or file mutation is added.
- Acceptance: first navigation to Apps starts one read-only inventory scan automatically; repeated navigation while loading awaits the same task; a completed empty scan does not loop; manual `重新扫描` still forces refresh; C-drive app handoff awaits loading before applying the C-drive filter; failures are path-free and retryable.
- Status: Completed with a visual Warn. First Apps navigation now starts the existing read-only inventory automatically; concurrent entry points share one in-flight task, successful empty results are remembered, failures can retry, and the manual control is now `重新扫描`. C-drive app handoffs await the same load before showing the C-drive filter.
- Last verification: focused lazy-load tests passed 4/4; related product/C-drive/app-catalog tests passed 170/170; full regression passed 699/699; solution build passed with 0 warnings/errors. Static gates passed for 282 strict UTF-8 C#/XAML files, 16 parsed XAML files, 120 resolved event bindings, zero duplicate literal AutomationIds, and zero mutation/process/pipeline references in the lazy-load orchestration slice.
- Blockers: real WPF screenshot remains Warn because Computer Use already failed to launch the app in this turn; no retry or PowerShell UI fallback was used.
- Exact next action: audit the main installation page for beginner-facing manual snapshot/report controls that duplicate the automatic installer workflow; keep advanced evidence available without making it a first-step requirement.

## Active Slice - 2026-07-15 Critical workflow connection audit after antivirus update

- Objective: find the next beginner-visible feature that currently stops at explanation or disabled UI, then connect it through an existing hardened capability without weakening signed production execution.
- Dependencies: current WPF navigation and action handlers, uninstall/migration production coordinators, worker trust policy, operation pipeline, existing tests, and the updated Huorong definitions that allow normal build/test verification again.
- Risks: adding an unsigned mutation path for developer convenience, mistaking fake-worker transport verification for real uninstall coverage, exposing a button whose handler has no outcome, duplicating privileged authority in WPF, or touching real installed software during acceptance.
- Impact scope: initially source/status audit only; the eventual implementation will be the smallest beginner-facing handoff or readiness correction supported by existing safety infrastructure. No real uninstall, migration, cleanup, service/task/registry mutation, or user-file change is authorized by this audit.
- Acceptance: identify one concrete visible dead end with source evidence; record the chosen boundary; add a failing focused test before implementation; preserve production same-signer requirements; verify focused/full tests, build, and applicable static/visual gates.
- Status: Completed with a visual Warn. Uninstall and migration plan windows now show a path-free production identity conclusion in the first visible area. Unsigned or untrusted packages stay preview-only and cannot create final execution evidence, enter final confirmation, or request elevation; trusted packages retain the existing flow and are re-assessed again by the production coordinator before launch.
- Last verification: focused readiness tests passed 7/7; related uninstall/migration/final-consent tests passed 24/24; full regression passed 695/695; solution build passed with 0 warnings/errors. Static gates passed for 280 strict UTF-8 C#/XAML files, 16 parsed XAML files, 120 resolved event bindings, zero duplicate literal AutomationIds, and zero forbidden Worker-launch/file-move references in the two WPF plan windows. Debug App and Worker binaries were confirmed unsigned, matching the preview-only state.
- Blockers: real first-view screenshot remains Warn. Computer Use `launch_app` timed out after the antivirus update; one read-only discovery found no OMNIX app/window and process inspection found no process. No PowerShell UI or input fallback was used.
- Exact next action: audit the next beginner-visible critical-workflow dead end. Preserve the two-check design: plan-page readiness is advisory and fail-closed; production execution must independently re-assess signer/hash trust.

## Active Slice - 2026-07-15 C-drive root-cause safe internal handoffs

- Objective: turn the plain-language `用户文件`、`程序和工具`、`软件数据`、`临时缓存` root-cause cards into concrete safe next steps instead of asking a beginner to find the corresponding page or list manually.
- Dependencies: `CDriveRootCauseSummaryBuilder`, current health-scan software inventory and personal-storage findings, `AppCatalogFilter.CDrive`, recommendation card presentation, the existing root-cause action button, and internal page navigation.
- Risks: auto-executing a cleanup when selecting a recommendation, navigating unexpected root folders as if they were safe temp data, promising an app owner that was not attributed, duplicating operation authority in the card handler, or exposing actions for Windows-managed system stores.
- Impact scope: typed presentation actions/labels/AutomationIds, one expanded fail-closed WPF handler, focused tests, and pending visual proof. No cleanup, quarantine, migration, uninstall, process launch, registry/service/task/file mutation, operation confirmation, or pipeline execution will run.
- Acceptance: ordinary user-profile cards open the read-only personal-file candidates; ordinary program/app-data cards open the `占 C 盘` app catalog; ordinary temp cards select and reveal the first actionable recommendation if one exists; unexpected roots and Windows-managed categories remain actionless; every runtime button has a stable action-specific AutomationId; selection never calls execution.
- Status: Completed with a visual Warn. Ordinary user-file, program/app-data, and temp cards now expose typed, action-specific buttons. They reveal existing evidence by navigating to the C-drive app filter, focusing personal-file candidates, or selecting the first actionable cleanup recommendation; unexpected roots and Windows-managed stores remain actionless.
- Last verification: focused action tests passed 3/3; related product tests passed 166/166 before the AutomationId hardening; final full regression passed 687/687; solution build passed with 0 warnings/errors; 278 non-generated C#/XAML files passed strict UTF-8, all 16 XAML files parsed, 62 unique event handlers resolved, literal AutomationIds were unique per XAML file, runtime card action ids are deterministic and unique in one summary, and the isolated handler contained no execution, pipeline, process, delete, or Recycle Bin clearing authority.
- Blockers: real first-view screenshot remains Warn because the immediately preceding bounded Computer Use launch produced no app/window and the connection was not bypassed or retried for this adjacent UI slice.
- Exact next action: audit the next beginner-visible critical-workflow dead end. Retry the combined root-cause card visual proof when Computer Use can launch WPF; keep card clicks navigation/selection-only.

## Active Slice - 2026-07-15 Recycle Bin review handoff

- Objective: close the C-drive diagnosis dead end where a large Recycle Bin is reported as generic system-reserved space but offers no understandable next step.
- Dependencies: `BigRocksProbe` recycle-bin evidence, `CDriveRootCauseSummaryBuilder`, the fixed `SystemToolShortcutCatalog`, existing confirmation-aware system-tool opener, Agent shortcut routing, and the C-drive root-cause list template.
- Risks: treating the words `清空回收站` as deletion consent, calling `SHEmptyRecycleBin`, implying deleted files can be recovered after clearing, exposing a generic command/argument from data, or showing an action on pagefile/hibernation/shadow-storage cards.
- Impact scope: beginner presentation fields, one fixed open-only Recycle Bin catalog entry, one conditional C-drive card button, one fail-closed WPF handler, Agent wording, and tests. No empty/delete/restore/quarantine/move operation, pipeline call, registry/service/task change, or shell command from user input will run.
- Acceptance: a positive Recycle Bin big-rock becomes a `回收站` card with plain-language warning and a stable `打开回收站查看` action; other big rocks have no action; the handler accepts only the typed Recycle Bin action and reuses `OpenAllowlistedSystemTool`; `清空回收站` resolves only to the fixed viewer and explicitly says it will not clear anything; `CanExecuteDirectly=false`; no delete API exists in the path.
- Status: Completed with a visual Warn. Positive Recycle Bin evidence now becomes a plain-language `回收站` card with one `打开回收站查看` action; all other big-rock cards remain actionless. The Agent converts even `清空回收站` wording into the same review-only fixed entry and explicitly refuses to clear anything.
- Last verification: focused contract tests passed 5/5; related Agent/product tests passed 191/191; full regression passed 686/686; solution build passed with 0 warnings/errors; 278 non-generated C#/XAML files passed strict UTF-8, all 16 XAML files parsed, 62 unique event handlers resolved, literal AutomationIds were unique per XAML file, and source contained no `SHEmptyRecycleBin` reference.
- Blockers: real first-view screenshot is Warn. Computer Use again timed out on the single bounded launch request; app discovery and process inspection found no OMNIX window/process, and no PowerShell UI fallback was used.
- Exact next action: audit the next beginner-visible critical-workflow dead end. Retry this open-only card screenshot when Computer Use can launch WPF; never add a clear/delete API or infer deletion consent from Agent text.

## Active Slice - 2026-07-15 MSIX managed-storage handoff

- Objective: close the MSIX install-routing dead end by giving a trusted analyzed Windows app package an explicit allowlisted `打开新应用保存位置` next step instead of a disabled prepare button plus text telling the user to ask the Agent.
- Dependencies: `InstallerRoutingCapability`, `InstallerLaunchPreparationPolicy`, `WindowsSettingsShortcutCatalog`, Microsoft-documented `ms-settings:savelocations`, installer capability panel, existing confirmation-aware settings opener, and current analysis identity.
- Risks: implying OMNIX can force an MSIX into an arbitrary folder, opening settings for stale/non-MSIX evidence, treating settings navigation as package execution, bypassing confirmation, or exposing a generic URI/command from data.
- Impact scope: fixed catalog entry, typed capability handoff id, one conditional installer-panel button, fail-closed WPF handler, tests, and pending GUI proof. No MSIX/installer launch, save-location change, ProgramFilesDir change, file/registry/service/task mutation, operation, or pipeline execution will run.
- Acceptance: trusted MSIX capability carries only the fixed save-locations shortcut id; preparation copy explains Windows manages the location; other/refused capabilities show no handoff; WPF validates the current capability/id then reuses the confirmation-aware allowlisted settings opener; natural-language questions can resolve the same fixed entry; `CanRequestInstallerLaunch=false`; no direct process call exists in the new handler.
- Status: Completed with a visual Warn. Trusted MSIX analysis now exposes one conditional `打开新应用保存位置` action through a fixed catalog id. The panel says Windows manages the location, hides the arbitrary D-path recommendation, disables route memory and installer preparation, and never launches the package.
- Last verification: focused installer/Agent/product tests passed 222/222; full regression passed 682/682; solution build passed with 0 warnings/errors; 278 non-generated C#/XAML files passed strict UTF-8, all 16 XAML files parsed, 61 unique event handlers resolved, literal AutomationIds were unique per XAML file, and the new handler authority audit passed. No `Css.App`/`Css.SmokeTools` process remained.
- Blockers: real first-view/cancel screenshot is Warn. After the antivirus definition update, Computer Use still timed out twice while requesting app launch; no target window or process appeared, and no PowerShell UI fallback was used. Official Microsoft documentation confirms `ms-settings:savelocations` as Default Save Locations.
- Exact next action: rerun a cancel-only visual acceptance when Computer Use can launch the WPF app; meanwhile audit the next beginner-visible dead end. Do not add an MSIX launcher or arbitrary-folder promise.

## Active Slice - 2026-07-15 Interactive truthful Agent skill catalog

- Objective: turn the visible eight-item Agent skill catalog from inert text into real `问 Agent` entry points that explain current evidence, open only safe existing workflows, and say plainly when a promised capability is not implemented.
- Dependencies: `AgentSkillCategory`, `AgentSkillCardPresenter`, `AgentConversationPresenter`, existing response AutomationIds/renderer, internal navigation, settings/tool lists, and current health/software summaries.
- Risks: making an unavailable category look executable, treating a card click as consent, duplicating system-tool/process authority, hiding unsupported capabilities behind vague copy, adding eight cluttered controls, or showing a response outside the first visible Agent area.
- Impact scope: presentation-only skill overview replies, one compact button per skill card, WPF response wiring, focused tests, and pending GUI proof. No process/tool launch, setting/session action, desktop enumeration, operation creation, pipeline call, registry/service/task/file mutation, installer, uninstall, or migration will run.
- Acceptance: every card has a stable actionable control; diagnosis/process/hardware categories reuse current local evidence; settings/troubleshooting/tool categories explain the safe existing entry; window/desktop and input/session state that V1 does not inspect or execute them; every response is local and `CanExecuteDirectly=false`; handler only applies the reply; first-view GUI proof is required when approval is available.
- Status: Completed with a visual Warn. Every card now has a stable compact `问 Agent` button that renders a local category reply. Current diagnosis/background/hardware evidence is reused; settings/troubleshooting/tools explain the existing safe entries; desktop/window and session cards plainly state that V1 does not inspect or execute them. The handler only calls the presenter and existing renderer.
- Last verification: focused Agent/skill tests passed 25/25; full regression passed 679/679; solution build passed with 0 warnings/errors; 278 non-generated C#/XAML files passed strict UTF-8; all 16 XAML files parsed; 82 event handlers resolved; no duplicate literal AutomationIds, App process, or fixture remained. `.omx/gui-agent-skill-cards-smoke.ps1` is prepared and source-tested but was not visibly run because the Codex GUI quota remains blocked; no screenshot pass is claimed.
- Blockers: visible screenshot/UIAutomation may remain blocked by the same Codex GUI approval quota; source/runtime verification can continue without bypassing it.
- Exact next action: rerun the prepared skill-card smoke when visible-window approval is available; meanwhile audit the next beginner-visible dead end, preserving explicit `规划中/不提供执行` states for unsupported skills.

## Active Slice - 2026-07-15 Beginner hardware configuration answer

- Objective: make the visible `电脑配置查询` Agent skill real by adding a bounded read-only CPU/GPU/Windows/architecture summary and a plain-language natural-language answer after a manual health scan.
- Dependencies: a separate Win32 hardware probe, `MachineHealthObservation`, `HealthCheckSummaryBuilder`, `AgentConversationPresenter`, existing Home scan orchestration, stable Agent response controls, and a real read-only WPF smoke.
- Risks: reading device serials/user identity, exposing raw WMI identifiers, hanging on an unhealthy WMI provider, mistaking hardware names for proof that a specific game/software will run, cluttering the beginner health table, or giving the result execution authority.
- Impact scope: fixed/bounded local hardware observation, path-free summary presentation, one Agent intent, focused tests, and read-only GUI proof. No benchmark, driver operation, process launch/termination, registry write, service/task change, file mutation, installer, uninstall, migration, cloud call, or session action will run.
- Acceptance: a successful manual scan can report sanitized CPU, GPU, logical processor count, Windows version, and architecture; unavailable fields remain explicit; no serial/user/device id is queried; `我的电脑配置` resolves to a hardware intent and current evidence or asks for a manual scan; answers make no game/software compatibility guarantee; `CanExecuteDirectly=false`; GUI proof is path-free and executes nothing.
- Status: Completed with a visual Warn. The fixed/bounded probe, health-summary propagation, hardware intent, and truthful skill copy are connected. WMI failure falls back to a fixed read-only CPU registry value and `EnumDisplayDevices`; no device identifiers are retained or shown. The dedicated WPF script exists but its visible run was rejected by the Codex GUI approval quota before launch.
- Last verification: hardware-focused real-machine tests passed 5/5; combined hardware/machine/Agent focus passed 28/28 before the final real-probe case; full regression passed 676/676; solution build passed with 0 warnings/errors; 278 non-generated C#/XAML files passed strict UTF-8 and all 16 XAML files parsed. No `Css.App` process or workspace/`C:\tmp` fixture remained. Screenshot/UIAutomation proof is not claimed.
- Blockers: real screenshot/UIAutomation proof is temporarily blocked by the Codex visible-window approval quota, not by product code or Huorong. Specific software/game compatibility requires separately supplied requirements and is outside this slice.
- Exact next action: retain `.omx/gui-agent-hardware-summary-smoke.ps1` as the pending visual gate and rerun it when visible-window approval is available; meanwhile audit the next visible capability promise without weakening hardware privacy or inventing software/game compatibility.

## Active Slice - 2026-07-15 Natural-language settings and troubleshooting routing

- Objective: make Computer Agent understand common network, Bluetooth, sound, display, power, driver, crash, blue-screen, and named system-tool questions and route them to the existing allowlisted open-only entry instead of returning a generic maintenance answer.
- Dependencies: `AgentConversationPresenter`, `WindowsSettingsShortcutCatalog`, `SystemToolShortcutCatalog`, existing confirmation-aware openers, Agent response AutomationIds, and navigation-only WPF handling.
- Risks: treating a question as consent to modify settings, opening a high-risk tool without its existing confirmation, routing ambiguous wording to the wrong tool, claiming a root-cause diagnosis without evidence, or embedding raw commands/URIs in the reply.
- Impact scope: local intent classification, shortcut metadata, reuse of existing allowlisted openers, focused tests, and fixture-only/cancel-only GUI proof. No setting change, process termination, driver change, registry write, power/session action, installer, cleanup, migration, or uninstall will run.
- Acceptance: common setting questions map to the exact catalog entry; crash/blue-screen questions route to Event Viewer and driver/device questions to Device Manager; replies explain what the entry can show and what OMNIX will not do; unknown questions remain general; shortcut ids are never taken from user text; high-risk openers keep confirmation; `CanExecuteDirectly=false`; screenshot shows conclusion and safe next step together.
- Status: Completed. Common settings, troubleshooting, and named-tool questions now resolve only to fixed local catalog ids. Driver/device questions route to Device Manager and crash/blue-screen questions to Event Viewer; ambiguous questions remain general. Protected tools retain the existing confirmation, and the Agent adds no command, URI, operation, or execution authority.
- Last verification: focused Agent conversation tests passed 20/20; full regression passed 671/671; solution build passed with 0 warnings/errors; 275 non-generated C#/XAML files passed strict UTF-8 and all 16 XAML files parsed. The real WPF smoke showed the uncertain diagnosis and exact open-only next step, opened the protected Device Manager confirmation, closed it as cancel, started no `mmc`, executed no operation, and produced clean `.omx/qa-agent-troubleshooting-routing.png` and `.omx/qa-agent-tool-open-confirmation.png`; process and fixture cleanup were empty.
- Blockers: none for allowlisted open-only routing. The Agent does not yet diagnose a fault from telemetry or repair drivers/settings; it truthfully opens the relevant Windows evidence/settings surface.
- Exact next action: audit the next visible Marvis-inspired skill promise against its real implementation, prioritizing beginner-safe diagnosis or a dead-end action rather than adding broad execution authority.

## Completed Slice - 2026-07-15 Install-report exact application handoff

- Objective: turn a ready, uniquely attributed install-change candidate preview into a safe exact-application handoff so the user can continue through the existing cache/startup/migration review instead of stopping at a read-only preview.
- Dependencies: `InstallSnapshotCandidatePreviewPresenter`, exactly one added software profile, `AppDrawerTargetResolver`, current inventory refresh, app drawer action readiness, and the existing confirmation/pipeline boundaries.
- Risks: navigating by a stale or ambiguous name, implying navigation executed the preview, exposing raw install evidence, enabling a handoff for refused/guidance-only results, or bypassing fresh drawer preparation.
- Impact scope: presentation navigation metadata, one WPF internal-navigation button, exact target resolution, fixture-only GUI proof, tests, and records. No installer, cleanup, startup change, migration, uninstall, registry/service/task change, or real C/D mutation will run.
- Acceptance: ready cache/startup/migration previews for one uniquely added software expose a path-free `打开对应应用` next step; refused/guidance-only previews do not; WPF resolves against current inventory and refuses missing/duplicate targets; the drawer opens with its normal action gate; `CanExecuteDirectly=false`; GUI proof executes no operation.
- Status: Completed. Ready cache/startup/migration previews now carry a unique non-visible app target and generic navigation label; refused and guidance-only previews do not. The WPF next step sits directly below the Agent conclusion, resolves against current inventory, and opens the existing app drawer without creating or executing an operation.
- Last verification: focused install-diff tests passed 24/24; full regression passed 661/661; solution build passed with 0 warnings/errors; 275 non-generated C#/XAML files passed strict UTF-8 and 16 XAML files parsed. The fixture WPF smoke reached the exact `New Fixture Tool` drawer, found the normal cache-review entry, executed nothing, and produced clean window-only `.omx/qa-install-diff-candidate-preview.png` and `.omx/qa-install-diff-app-handoff.png`; all fixture/process state was removed.
- Blockers: none for internal navigation. Positive cache/startup/migration execution remains under each existing confirmation and disposable-fixture policy.
- Exact next action: audit the next beginner-visible warning or Agent skill that still lacks a truthful, safe next step. Do not use cached install-report attribution as execution authority.

## Completed Slice - 2026-07-15 Personal large-file and possible-duplicate GUI acceptance

- Objective: prove that bounded read-only personal-storage findings reach a beginner-visible Home/C-drive experience without presenting filename/size similarity as proven duplication or granting delete authority.
- Dependencies: `PersonalStorageAnalyzer`, known current-user personal-root policy, disk scan tree, `PersonalStorageFindingPresenter`, Home key findings, C-drive personal-storage list, stable AutomationIds, and an isolated development fixture path.
- Risks: creating test files in real Desktop/Documents/Downloads, reading file contents, calling same-name files definite duplicates, exposing full paths, allowing a finding to become a cleanup operation, or accepting an offscreen list as usable UI.
- Impact scope: development-only fixture injection if needed, read-only presentation, UIAutomation/screenshot proof, focused tests, and records. No content hashing, deletion, quarantine, archive, move, registry/service/task change, installer, uninstall, migration, startup mutation, or cloud call.
- Acceptance: a fixture-only scan shows one long-unused large-file finding and one possible-duplicate group using filenames and aggregate sizes only; copy explicitly says candidates/suspected and content was not compared; first-level UI hides full paths; no operation button is enabled from these findings; Home explanation remains read-only; fixtures/processes are removed.
- Status: Completed. A process-scoped fixture seam exercises the production metadata-only analyzer with lower fixture thresholds; normal scans keep the 512 MB/64 MB defaults. Home detail navigation now lands on the actual candidate list, both fixture candidates are visible, and beginner recommendation text replaces local paths while the operation descriptor retains exact evidence.
- Last verification: focused tests passed 29/29; full regression passed 661/661; solution build passed with 0 warnings/errors; 275 non-generated C#/XAML files passed strict UTF-8 and 16 XAML files parsed. The fixture-only WPF smoke showed one long-unused file and one possible-duplicate group, hid full paths across the visible window, kept both items non-executable, and produced `.omx/qa-personal-storage-candidates.png`. No fixture or `Css.App` process remained.
- Blockers: none for this read-only slice. Similarity remains name-and-size evidence only; content hashing and automatic personal-file cleanup are intentionally not implemented.
- Exact next action: audit the next beginner-visible action that still ends at explanation/preview without a safe completion path. Keep real-system mutation acceptance on disposable fixtures.

## Completed Slice - 2026-07-15 Home whole-PC health runtime acceptance

- Objective: close the current runtime/visual gap for the beginner Home health summary by proving the production read-only D-drive, memory, process-count, battery, startup-signal, and usage-trend observations appear as truthful first-view conclusions.
- Dependencies: isolated `OMNIX_ENTROPY_CDRIVE_SCAN_ROOT` and data roots, `WindowsMachineHealthProbe`, manual Home scan orchestration, `HealthCheckSummaryBuilder`, stable health-dimension AutomationIds, Home Agent explanations, and screenshot inspection.
- Risks: scanning the real C drive during automation, retaining process names, treating one memory sample as a diagnosis, labeling a desktop's absent battery as failure, exposing paths, changing system state, or accepting an offscreen/overlapping result because it exists in the UI tree.
- Impact scope: fixture-only C-drive scan plus production read-only machine observation, UIAutomation assertions, screenshots, focused fixes, tests, and protocol records. No cleanup, process termination, power change, registry/service/task mutation, installer, uninstall, migration, startup mutation, or cloud call.
- Acceptance: the first Home working area shows the overall conclusion and all applicable dimension rows with stable AutomationIds; D-drive and memory values are plausible or explicitly unavailable; process evidence remains count-only; battery is healthy/warning/not-present/unavailable without guessing; startup is labeled as inventory signal; usage trend states its history threshold; Agent explanations remain path-free and non-executable; all fixtures/processes are cleaned.
- Status: Completed. The strengthened fixture smoke validates the stable D-drive, memory/process-count, battery, startup-clue, and manual-usage-history rows against the production machine probe and captures only the OMNIX window. The C-drive fixture now resides on a unique C-volume path, so C/D percentages are not mislabeled by the development override.
- Last verification: machine-health/script focus passed 5/5; full regression passed 658/658; solution build passed with 0 warnings/errors; 274 non-generated C#/XAML files passed strict UTF-8 and 16 XAML files parsed. WPF output showed D 69.3%/200.2 GB free, memory 13.4/31.3 GB with 278 count-only processes, battery 100% on AC, 10 ordinary startup clues, and one manual-check history. `.omx/qa-home-agent-next-action.png` is a clean window-only screenshot; no operation ran and no process or `C:\tmp` fixture remained.
- Blockers: none. Real hardware values are read-only and user-initiated; the C-drive portion must stay fixture-bound.
- Exact next action: retain this smoke as the Home runtime gate; continue with personal-storage candidate GUI acceptance.

## Active Slice - 2026-07-15 Agent startup advice matches local reversible control

- Objective: make the beginner-facing Computer Agent tell the truth about the newly connected exact current-user Run control instead of sending every startup question to Windows Settings or claiming all startup changes are unavailable.
- Dependencies: structured `BackgroundComponentObservation`, `StartupEntryControlPolicy`, application-specific Agent routing, aggregate startup answer, background review, startup/service plan preview, exact-app navigation, and the existing drawer preparation/confirmation pipeline.
- Risks: promising local execution from name-only signals, treating services/tasks as ordinary startup entries, implying the Agent executed anything, exposing registry identifiers, or making stale advice itself operation authority.
- Impact scope: read-only local Agent wording, eligibility classification, and navigation only. No registry, service, task, process, file, installer, uninstall, migration, or cleanup authority is added.
- Acceptance: exactly one supported ordinary current-user Run observation for a non-system app yields an `审核关闭方案` recommendation with a fresh-recheck warning; name-only, multiple, unsupported, system, service, and task cases remain settings handoff or explanation-only; specific app questions navigate to the exact app drawer; aggregate answers count local-review candidates; `CanExecuteDirectly` remains false.
- Status: Completed. Aggregate startup answers, exact-app answers, background review, and startup/service plan preview now distinguish one locally reviewable ordinary Run entry from name-only, multiple, system, service, and task evidence. Advice remains read-only and exact-app navigation lands on the existing evidence-bound drawer review.
- Last verification: focused Agent conversation/advice tests passed 15/15; full regression passed 658/658; solution build passed with 0 warnings/errors; 274 non-generated C#/XAML files passed strict UTF-8 and 16 XAML files parsed. Fixture-only WPF proof showed the path-free answer, fresh-reread and rollback wording, exact-app navigation, an enabled review button, and `noOperationExecuted=true`. Screenshot `.omx/qa-agent-startup-advice.png` was visually inspected after fixing top-title clipping.
- Blockers: none.
- Exact next action: audit the next visible V1 action that still stops at explanation or preview, and connect only a narrowly evidenced, reversible scope.

## Active Slice - 2026-07-15 Quarantine candidate identity and execution-time revalidation

- Objective: close the time-of-check/time-of-use gap shared by C-drive cleanup, application-cache cleanup, and low-risk uninstall-residue quarantine so a path confirmed by the user cannot silently become a different file, directory, or reparse target before the move.
- Dependencies: `OperationDescriptor`, `QuarantineOperationPolicy`, a Windows stable file-identity reader, `FileQuarantineService`, `QuarantineOperationHandler`, the three WPF confirmation entry points, action timeline, and temporary-directory tests.
- Risks: moving a recreated path after a long confirmation, following a junction/symlink parent, accepting duplicate or overlapping candidates, allowing source/quarantine overlap, moving part of a batch before discovering a stale item, weakening direct service callers, or touching real user files during verification.
- Impact scope: low-risk quarantine preparation and execution only. No permanent deletion, installer, uninstaller, migration, registry, service, task, or real C/D cleanup will run in development verification.
- Acceptance: every production UI entry binds canonical path, file/directory type, volume identity, and file identity before confirmation; confirmation refuses unprepared descriptors; the handler revalidates the entire batch before moving anything; the service revalidates again immediately before each move; reparse/UNC/ADS/root/overlap/duplicate/stale candidates fail closed; rollback/timeline behavior stays intact; tests use isolated temporary paths and GUI proof remains cancel-only.
- Status: Completed for production UI entry points. C-drive cleanup, application-cache cleanup, and low-risk uninstall-residue cleanup now bind canonical path, type, volume/file identity, creation/write metadata, and file length before confirmation; unprepared confirmation is refused. The handler revalidates the whole batch before any move and the service revalidates again after writing the recovery manifest immediately before each move.
- Last verification: identity tests passed 7/7; related quarantine/cache/residue tests passed 33/33; full regression passed 653/653; solution build passed with 0 warnings/errors; 375 C#/XAML files passed strict UTF-8 and 16 XAML files parsed. The fixture-only C-drive WPF smoke opened the real confirmation, clicked cancel, left the candidate in place, created zero quarantine items, and produced `.omx/qa-cdrive-cleanup-confirmation.png`.
- Blockers: none for this source/fixture slice. Positive cleanup/restore against a disposable non-user fixture outside the repo remains a release acceptance item; ordinary development must not touch real C-drive content.
- Exact next action: audit the next visible V1 control that still ends in explanation or navigation instead of a safe completion path.

## Active Slice - 2026-07-15 Reversible current-user Run startup control

- Objective: replace the application drawer's Windows-settings-only dead end with a local, evidence-bound disable/restore loop for one precisely identified ordinary current-user Run entry.
- Dependencies: structured `BackgroundComponentObservation`, exact `HKCU64\...\Run` identity, fresh registry value/type and key-ACL observation, bounded atomic rollback manifest, `OperationDescriptor`, `SafetyOperationPipeline`, explicit WPF confirmation, action timeline, and re-scan after change/restore.
- Risks: disabling the wrong value, trusting name-only correlation, changing undocumented `StartupApproved` bytes, touching HKLM/services/tasks/system tools, stale evidence races, storing unbounded command data, failing to restore after timeline failure, or presenting a settings handoff as successful local control.
- Impact scope: one current-user 64-bit Run registry value only when exactly one structured app-owned candidate is fresh and uniquely matched. Unsupported, ambiguous, stale, system, HKLM, service, task, or approval-state cases remain read-only/settings handoff. No real registry mutation will run during automated or GUI verification.
- Acceptance: preparation re-reads and fingerprints the exact value/type/ACL and current approval observation; a strict atomic manifest is created before confirmation; confirmation names the scope and untouched components; handler revalidates manifest/hash/fresh state before deleting; timeline records a restorable startup action; restore refuses collisions and ACL drift; UI tests cancel rather than confirm; real execution stays user-initiated.
- Status: Completed for the supported V1 scope. The app drawer now offers a local review only for one fresh, uniquely correlated ordinary HKCU64 Run value; it re-scans, creates a verified rollback manifest, requires two explicit acknowledgements, executes through `SafetyOperationPipeline`, journals a restorable timeline entry, and dispatches startup restore separately from file quarantine. Unsupported cases still hand off to Windows Settings.
- Last verification: focused startup/app/timeline tests passed 42/42; full regression passed 646/646; solution build passed with 0 warnings/errors; 16 XAML files and 372 C#/XAML files passed parse/strict UTF-8 checks; 265 AutomationIds are unique. Cancel-only WPF smokes proved both disable and restore confirmations, path-free first views, manifest lifecycle, and no operation execution. Screenshots: `.omx/qa-startup-control-confirmation.png`, `.omx/qa-startup-restore-confirmation.png`.
- Blockers: none for source/UI completion. Positive real-registry acceptance remains a release-only check using an explicitly disposable HKCU Run value in a disposable test account; it must not use an ordinary installed application's startup entry.
- Exact next action: audit the next visible V1 action that still ends in explanation or navigation without a safe completion path, while keeping installer/uninstall/migration real-system acceptance on disposable signed fixtures.

## Active Slice - 2026-07-14 Migration-closure beginner GUI acceptance

- Objective: prove that a persisted migration-monitoring record whose original C-drive folder has returned becomes a visible, path-free warning on Home and in the matching application drawer.
- Dependencies: isolated `OMNIX_ENTROPY_DATA_ROOT`, `JsonMigrationMonitoringStore`, `WindowsMigrationPathObserver`, a one-app software fixture, Home health refresh, app catalog ordering/tagging, drawer advice, and stable UIAutomation controls.
- Risks: touching a real migrated application, creating a junction/symlink, leaking fixture paths into beginner UI, selecting the wrong app tile, deleting outside exact fixture roots, or treating source/static assertions as GUI proof.
- Impact scope: one fixture-only WPF smoke, stable verification assertions, focused tests, screenshot evidence, and protocol records. The smoke may create one ordinary uniquely named directory under `C:\tmp` and one target directory under `.omx`; it must create no redirect, move, rollback, installer, uninstaller, registry/service/task change, or production data.
- Acceptance: Home renders a `迁移闭环` dimension and first-level warning without raw fixture paths; Apps renders the matching application first with `迁移未闭环`; the drawer repeats the plain-language warning and exposes an enabled `复查迁移` button; no operation is executed; the launched process and exact fixture directories are removed in `finally`.
- Status: Completed. The Home warning, application warning tag, drawer advice, and review button are visible from a production-format fixture record; raw fixture paths stay hidden and no operation is invoked. Screenshot review also fixed horizontal key-finding overflow and a clipped application status tag.
- Last verification: focused product/closure tests passed 168/168; full regression passed 624/624; WPF output reported `closureDimensionAutomationId=HealthDimension_迁移闭环`, `migrationReviewEnabled=true`, `rawFixturePathHidden=true`, and `noOperationExecuted=true`. Screenshots: `.omx/qa-migration-closure-home.png` and `.omx/qa-migration-closure-app.png`.
- Blockers: none for fixture-only acceptance. Real redirect behavior and signed-package migration remain separate release checks.
- Exact next action: keep the fixture smoke as the regression gate; real redirect behavior remains a signed-package/disposable-app release check.

## Active Slice - 2026-07-14 Bounded post-install C-drive footprint evidence

- Objective: make the install before/after report detect newly created C-drive landing points even when an installer does not register an uninstall entry or the software inventory cannot attribute the path.
- Dependencies: `InstallSystemSnapshot`, install-before evidence binding, `InstallerExecutionCoordinator`, manual snapshot buttons, bounded common-root observation, `InstallSnapshotDiffBuilder`, and existing path-free first-level install cards.
- Risks: recursive/slow C-drive crawling, following reparse points, scanning network/removable locations, treating unrelated background changes as installer ownership, leaking user paths in first-level UI, unbounded evidence, stale/tampered before snapshots, or claiming no C writes after an incomplete observation.
- Impact scope: top-level read-only observations under Program Files, ProgramData, LocalAppData, and RoamingAppData; snapshot/evidence fingerprints; post-install diff completeness; focused tests and records. No file content read, hash, recursion, process launch beyond the already gated installer flow, cleanup, migration, registry/service/task mutation, or real installation during verification.
- Acceptance: at most 4096 immediate non-reparse entries are observed across fixed local C roots; complete before/after captures add genuinely new paths to the report; truncated/unavailable captures never produce a false `未发现` conclusion; before evidence binds status/count/fingerprint; automated and manual snapshots use the same probe; first-level cards remain count-only/path-free and say changes are candidates, not proven installer ownership.
- Status: Source and fixture implementation complete. Automated/manual before snapshots and the automated after scan now share a fixed-root, top-level, 4096-entry, non-reparse C-drive observation. Complete captures add unregistered landing-point candidates; truncated/unavailable captures cannot produce a clean bill or a concrete candidate preview.
- Last verification: normal restore succeeded; test project and solution build passed with 0 warnings/errors; installer-focused tests passed 52/52; full regression passed 623/623. Static gates passed for 257 non-generated strict UTF-8 C#/XAML files, 14 XAML parses, 58 handlers, 254 unique AutomationIds, fixed C-only scope, bounds, no mutation APIs, evidence status/count/fingerprint binding, and first-level privacy. Fixture WPF smoke passed twice with four cards, four Agent steps, three plan items, collapsed technical details, hidden raw identifiers, and no execution authority. Clean screenshots: `.omx/qa-install-diff-cards.png` and `.omx/qa-install-diff-action-plan.png`.
- Blockers: no real installer was run, so production root latency and real installer attribution remain unverified. `.omx/qa-install-diff-agent.png` repeatedly contains desktop-compositor black areas even though UIAutomation and the visible conclusion pass; retain this as a screenshot-quality warning.
- Exact next action: audit the next V1 critical workflow for a visible warning or button without safe completion; keep real installation acceptance for a disposable signed test package and never use it as ordinary development verification.

## Active Slice - 2026-07-14 Truthful whole-PC health dimensions

- Objective: make the Home health table match the promised beginner experience with real read-only D-drive, memory, process, battery, startup-signal, and multi-scan usage-trend conclusions instead of presenting a C-drive-only scan as a whole-PC check.
- Dependencies: a Core machine-health observation contract, a read-only Win32 probe, current software profiles, scan-history count, growth findings, `HealthCheckSummaryBuilder`, MainWindow's explicit manual scan, and existing Home `HealthDimensionListView`.
- Risks: inventing unavailable data, treating momentary memory use as a diagnosis, calling startup registry presence proof that an item is enabled, probing network/removable drives, leaking process names or paths, blocking the long disk scan, or claiming scheduled/background monitoring.
- Impact scope: read-only machine observation, health dimension presentation, explicit-scan orchestration, focused tests, and protocol records. No process termination, cleanup, installation, migration, registry/service/task mutation, scheduler, cloud call, or real system change.
- Acceptance: D is reported only when a ready local fixed drive exists; memory uses Windows-reported totals/load and only a process count; battery is optional/not-applicable; startup is labeled as inventory signals rather than effective state; usage trend requires at least three real manual snapshots; unavailable data says so; no private process names/paths are retained; existing cleanup score/action authority is unchanged.
- Status: Source implementation complete with verification Warn. A manual Home scan reads only local fixed D-drive capacity, Windows physical-memory load, a count-only process sample, and optional battery state; combines them with current startup inventory signals and at least three real manual snapshots; and presents unavailable/not-present/history-insufficient states without guessing. The local Agent can explain these dimensions but cannot execute anything.
- Last verification: current source compiled with 0 warnings/errors; `MachineHealthExperienceTests` passed 4/4 within the 623/623 full regression. Static fixed-drive/privacy/authority/history checks remain green.
- Blockers: real P/Invoke values, minimum-window wrapping/scrolling, and a current first-view Home screenshot remain unverified.
- Exact next action: capture fixture-only Home/Agent UI evidence, then keep real hardware observations read-only and user-initiated.

## Active Slice - 2026-07-14 Migration closure monitoring surfaced to beginners

- Objective: consume the migration monitoring records already written by the elevated Worker and tell the beginner whether original C-drive paths still redirect correctly or have been recreated/changed.
- Dependencies: `MigrationMonitoringRecord`, `MigrationClosureMonitor`, the local migration monitoring root, a read-only Win32 path observer, Home health findings, application tiles/drawer, and exact-app navigation.
- Risks: giving WPF move/rollback authority, exposing private paths, attributing a name-only old record to the wrong duplicate app, scanning unbounded records, treating a missing redirect as proof of data loss, or claiming background monitoring exists.
- Impact scope: split read-only path observation from mutation adapters, add bounded/latest closure presentation, load it during explicit health/app scans and after a successful migration, then surface path-free state in Home and Apps. No scheduler, cloud call, directory move, redirect creation/removal, rollback, UAC, registry/service/task change, installer, uninstall, or real C/D mutation will run during verification.
- Acceptance: production records remain written by the Worker; the WPF process uses only a read-only observer; latest records are bounded and grouped; healthy/recreated/missing/changed states become plain Chinese summaries without paths; ambiguous app names do not become operation identity; Home and app UI refresh after explicit scans and successful migration; all findings remain explanation/navigation only.
- Status: Source implementation complete with verification Warn. Explicit health/app scans and successful migration refreshes consume the latest bounded monitoring records through a read-only Win32 observer. Home, app ordering/tags, drawer advice, and migration review show path-free closure state; duplicate app names never become an operation target. Closure review can reopen the evidence/snapshot plan but old records never grant execution authority.
- Last verification: current source compiled with 0 warnings/errors; closure tests are included in the 623/623 full regression; static read-only authority/privacy/bounds/order checks remain green.
- Blockers: fixture GUI proof and real redirect observations remain unverified; no filesystem migration ran.
- Exact next action: capture fixture-only Home/app closure warnings before any disposable signed-package migration acceptance.

## Active Slice - 2026-07-14 Dependency recovery and accumulated runtime acceptance

- Objective: restore valid NuGet assets once approval is available, compile every accumulated source slice, run focused/full tests, and capture the required beginner-first WPF evidence without touching real system data.
- Dependencies: normal `dotnet restore`, corrected Huorong definitions, `Css.Tests`, solution build, fixture-only UIAutomation, and screenshot inspection.
- Risks: treating old test assemblies as proof of current source, retrying a blocked restore through an indirect route, launching real migration/uninstall/installer operations during verification, or calling static checks compiled evidence.
- Impact scope: dependency recovery and verification only. No product gate will be weakened to make tests pass.
- Acceptance: current source builds with 0 errors; focused health-digest, quarantine, personal-storage, migration, uninstall, and installer tests pass; full regression passes; WPF screenshots prove the first visible conclusions and final-confirmation boundaries; no real C/D mutation occurs.
- Status: Dependency recovery complete. Corrected Huorong definitions are installed, the normal NuGet restore succeeded, and current assemblies are fresh.
- Last verification: `dotnet build tests\Css.Tests\Css.Tests.csproj --no-restore --nologo` and `dotnet build ComputerSecuritySoftware.slnx --no-restore --nologo` passed with 0 warnings/errors; full regression passed 623/623; install-diff fixture UIAutomation passed twice.
- Blockers: not every accumulated WPF slice has a fresh screenshot; real installer/migration/uninstall actions remain outside ordinary development verification.
- Exact next action: run the remaining fixture-only Home, migration-closure, and final-consent visual gates as their slices are resumed; keep signed/disposable-machine release acceptance separate.

## Completed Slice - 2026-07-14 Migration final-consent reachability and truthful copy

- Objective: remove the migration preview dead end so evidence-complete plans can reach one explicit final-consent screen, while keeping package trust, elevation, rollback, and post-migration verification gates fail-closed.
- Dependencies: migration snapshot/rollback evidence, destination-space probe, `MigrationExecutionGate`, final-consent composer/window, production coordinator trust policy, and the existing WPF migration entry.
- Risks: treating evidence creation as user consent, enabling unsigned development packages, moving files during verification, preserving duplicate/conflicting confirmations, or showing stale preview copy that contradicts the production flow.
- Impact scope: migration gate/checklist readiness, MainWindow evidence refresh, migration result copy, uninstall preview copy, focused source tests, and protocol records. No real migration, uninstall, installer launch, UAC, registry/service/task mutation, or real C/D file operation will run during development verification.
- Acceptance: initial migration remains preview-only; a valid manifest, snapshot hash, and sufficient target space can enable only the final-consent button; the final screen still requires all four acknowledgements; signed-package trust remains a separate production gate; beginner-facing migration/uninstall text is valid Chinese and truthful; focused static gates pass.
- Status: Source implementation complete with verification Warn. Evidence-complete migration plans can now reach the separate final-consent window; plan/close/rollback/monitoring acknowledgements remain required there, and the signed-package Worker trust boundary remains unchanged. Successful production outcomes return to MainWindow and trigger a fresh application scan. Migration/uninstall preview copy now describes the staged flow instead of claiming the version is permanently disabled.
- Last verification: 349 C#/XAML files passed strict UTF-8; 14 WPF XAML files parsed; 108 event bindings resolved; 251 AutomationIds were unique per window. Static authority gates passed for evidence-before-capability, four final acknowledgements, signed-package trust, result propagation, and truthful uninstall copy. New/updated tests are uncompiled.
- Blockers: new source cannot be compiled until normal NuGet restore assets are recovered. Runtime and GUI acceptance remain deferred; static checks must not be presented as compiled proof.
- Exact next action: after the normal restore, compile and run migration final-consent/ProductExperience tests, then capture the migration preparation and final-confirmation windows without confirming a real operation.

## Completed Slice - 2026-07-14 Local health digest history

- Objective: persist beginner-facing daily/weekly health digests so the user can see whether C-drive pressure and recommendations are improving without reading raw scan reports.
- Dependencies: `HealthCheckSummary`, scan snapshots/growth findings, application profiles, the existing SQLite data root, Home/Agent presentation, and a bounded local history UI.
- Risks: inventing a scheduled scan that never ran, storing private absolute paths, duplicating the same scan into noisy history, unbounded database growth, implying a background service exists, or turning a digest into automatic execution authority.
- Impact scope: digest model/builder, bounded local persistence, history presentation and UI entry, focused source tests, and records. V1 will save a digest only after a user-initiated successful scan; it will not add a background service, Task Scheduler entry, cloud upload, file cleanup, installer, migration, registry/service/task mutation, or real system operation.
- Acceptance: one successful scan produces one path-free digest; duplicate scan identities upsert instead of multiplying; history is bounded; daily and weekly views are computed from real stored scans only; every recommendation navigates to evidence and remains non-executable; empty history says no scheduled monitoring is active.
- Status: Source implementation complete with verification Warn. Successful user-initiated scans now upsert path-free bounded local digests; the Home page shows recent daily rows and a weekly trend while explicitly stating that no background schedule exists. Duplicate identities update instead of multiplying, malformed rows are skipped, and the digest has no execution authority.
- Last verification: 349 C#/XAML files passed strict UTF-8; MainWindow XAML/handlers and digest source invariants passed; the pre-incident compiled regression passed 586/586 with the known obsolete installer assertion excluded. New digest tests are uncompiled.
- Blockers: current C# compilation and GUI screenshots still require the normal NuGet restore. This slice will use local source/static gates until then.
- Exact next action: after dependency recovery, compile/run `HealthDigestTests`, perform a two-scan fixture check, and capture the current-health-before-history first view.

## Completed Slice - 2026-07-14 Quarantine retention and capacity governance

- Objective: turn the existing 30-day/20-GB quarantine advice into an enforceable, beginner-visible governance flow without silently destroying the user's last rollback copy.
- Dependencies: `QuarantineRecord`, `QuarantineRetentionPlanner`, manifest-root validation, `OperationDescriptor`, `SafetyOperationPipeline`, timeline journaling, and the regret-center UI.
- Risks: automatic permanent deletion, trusting a forged or redirected manifest, deleting outside the quarantine root, capacity arithmetic overflow, purging a still-restorable item without explicit consent, or presenting a recommendation as already executed.
- Impact scope: first harden the pure retention plan and presentation, then add a dedicated irreversible purge descriptor/handler only if every manifest and payload is confined to the configured quarantine root. No automatic purge, real file deletion, GUI launch, UAC, registry/service/task mutation, installer, migration, or real C/D operation is allowed during development verification.
- Acceptance: invalid options refuse; byte totals saturate; restored/expired/over-capacity reasons are distinct; the UI shows current usage, candidate count, projected space, and that cleanup is permanent; every candidate is reloaded and revalidated at execution time; only one explicit final confirmation can enter the local safety pipeline; no purge is scheduled or automatic; timeline records permanent cleanup as not restorable.
- Status: Source implementation complete with verification Warn. Retention planning now validates options, saturates size arithmetic, distinguishes active/reclaimable/projected bytes, bounds one batch to 100, and remains non-automatic. Manifest restore/purge paths are confined and reparse/ADS checked. Permanent cleanup is manual-only, Medium risk, explicitly non-restorable, double-inspected, routed through `SafetyOperationPipeline`, and journaled as not restorable. The WPF confirmation requires a visible irreversible warning and acknowledgement checkbox.
- Last verification: 346 C#/XAML files passed strict UTF-8; 14 WPF XAML files parsed; 72 event handlers resolved; per-window AutomationIds were unique; mojibake, iterative deletion, manifest confinement, manual-only policy, no-auto-load, and warning-order gates passed. The pre-incident compiled suite passed 586/586 with the one known obsolete installer source assertion excluded. New governance tests are uncompiled.
- Blockers: current C# compilation and GUI screenshots still require a normal NuGet restore after generated assets were invalidated. Source work can continue only with fail-closed static checks until restore is available.
- Exact next action: after dependency restore, compile/run `QuarantineRetentionGovernanceTests`, the full suite, and a fixture-only confirmation/operation test; capture the warning, acknowledgement, and projected-impact first view without touching real quarantine data.

## Completed Slice - 2026-07-14 Personal large-file and duplicate-candidate diagnosis

- Objective: connect the missing read-only personal-storage diagnosis promised by V1 so C-drive results can show long-unused large files and conservative duplicate candidates in beginner language.
- Dependencies: the existing bounded `CategoryNode` tree, explicit file/directory identity, current-user known folders, `DiskScanSession`, home health findings, the C-drive first-level summary, and static/source tests.
- Risks: scanning other users' data, treating same size as proof of duplication, exposing private absolute paths, hashing or reading huge files unexpectedly, turning a diagnosis into delete authority, or adding unbounded traversal/memory work.
- Impact scope: Scanner read-only models/analyzer/presenter, session and health-summary projection, one C-drive list with stable AutomationIds, focused source tests, and protocol records. No file hashing, process launch, cleanup operation, quarantine move, installer, migration, registry/service/task change, or real C/D mutation is allowed.
- Acceptance: only explicit current-user personal roots are considered; file nodes are distinguished from empty folders; traversal and result counts are bounded; stale large files require size and age thresholds; duplicate groups are labeled "疑似" and require same normalized name and exact size; first-level text contains names/sizes but no absolute paths; every item is non-executable; empty/refused states remain clear.
- Status: Source implementation complete with verification Warn. The bounded analyzer only projects explicit current-user personal roots from the existing scan tree, requires `IsFile`, labels same-name/exact-size groups as suspicious rather than proven duplicates, exposes path-free beginner text, and cannot create operations. Home findings and the C-drive list are wired with stable AutomationIds.
- Last verification: strict UTF-8 passed for 340 C#/XAML files; MainWindow XAML parsed; all 42 event handlers resolved; 122 AutomationIds were unique; touched-file mojibake and read-only authority audits passed. New focused tests exist but cannot compile until NuGet assets are restored.
- Blockers: current C# compilation and GUI screenshots require the normal NuGet restore that Codex approval quota currently blocks. This does not prevent bounded source implementation and static XAML/UTF-8/authority checks.
- Exact next action: after dependency restore, compile and run `PersonalStorageAnalyzerTests`, full regression, then capture a real C-drive screenshot showing the summary and candidate list in the visible working area.

## Active Slice - 2026-07-14 Installer launch readiness audit

- Objective: determine whether the installer-routing workflow has enough typed evidence, final consent, snapshot coverage, launch confinement, and post-launch verification to replace the hard-coded disabled gate with a truthful readiness decision.
- Dependencies: `InstallerRoutingCapabilityPolicy`, package inspection and stable identity, `InstallerLaunchOperationPlanner`, final-consent service, execution coordinator, production launcher adapter, WPF capability presentation, and focused installer tests.
- Risks: treating antivirus acceptance as authorization to execute arbitrary installers, silently installing software, losing the recommended D-drive route between preview and launch, claiming installation success from process exit, or enabling a feature whose rollback and post-scan evidence are incomplete.
- Impact scope: installer readiness/presentation policy and focused tests first. No installer will be launched, no silent arguments or UI automation will be added, and no real C/D files, registry keys, services, tasks, or installed applications will be changed during this slice.
- Acceptance: the UI reports a typed enabled/disabled reason; launch is available only for a local, stable, revalidated package with snapshot and explicit final consent; the production adapter only opens the interactive installer; completion is reported as "installer opened" until a post-scan proves actual changes; every refusal is fail-closed and tested.
- Status: Implementation complete with verification Warn. The temporary hard-coded false gate is replaced by typed Windows readiness with an emergency disable override. Preparation now also requires a usable managed non-system target. Package inspection, descriptor parsing, and the production launcher require an existing fixed-local package whose file and ancestor chain have no reparse points. The planner and handler independently require an allowed target. No installer was launched.
- Last verification: after the readiness/path change, installer-focused tests passed 86/86. After adding target preflight/planner enforcement, `Css.Tests` built with 0 warnings/errors. The existing compiled suite then ran 85/86 against current source; its only failure was a static assertion for the superseded direct readiness expression, which is corrected in source but not yet recompiled. Earlier full regression remains 581/581 and the solution build 0 warnings/errors. No Huorong alert appeared.
- Blockers: an accidental test command omitted `--no-build --no-restore`, and failed NuGet attempts rewrote generated restore assets. A normal network restore is required; the request was rejected by Codex usage quota, so current final tests cannot compile. Visible WPF acceptance is blocked by the same quota. Signing, production UAC/Worker, real installer launch, and real system mutation remain separate release gates.
- Exact next action: when approval is available, run one normal `dotnet restore tests\\Css.Tests\\Css.Tests.csproj -p:NuGetAudit=false`, then build with `--no-restore`, run installer-focused tests with `--no-build --no-restore`, run the full suite and solution build, and capture the fixture-only install analysis/final-consent UI without launching a package.

## Active Slice - 2026-07-14 Antivirus-gated verification resume

- Objective: safely resume executable verification after the user confirmed that the corrected Huorong virus definitions are installed, then turn the accumulated source-only slices into compiler, test, and GUI evidence.
- Dependencies: current untracked worktree, `Css.Tests` project, Huorong's corrected classification, generated test assembly, focused deferred test groups, full regression, UIAutomation, and screenshot inspection.
- Risks: a fresh heuristic quarantine, assuming an existing DLL proves the new build is accepted, running broad tests before compiler issues are isolated, leaving test/App processes alive, or confusing fixture/runtime acceptance with permission to touch real C/D software data.
- Impact scope: one narrow test-project build, post-build artifact existence/hash observation, focused and full tests if the artifact remains stable, solution build, fixture/read-only GUI acceptance, and protocol records. Real cleanup, uninstall, migration, installer launch, registry/service/task mutation, and production Worker/UAC remain out of scope.
- Acceptance: `Css.Tests` builds once; the generated DLL remains present with a stable hash after an observation delay and no new alert is reported; focused deferred tests pass; full regression and solution build pass; required beginner panels receive UIAutomation and inspected screenshots; no relevant process remains.
- Status: Verification completed with one visual Warn. The corrected definitions are installed; compiler defects and stale expectations found during recovery were fixed. The Home Agent next-action GUI smoke passed, while the final application-drawer rerun was blocked by Codex's visible-window approval quota after its deterministic fixture exposed and helped fix a real startup-action binding defect.
- Last verification: full Debug tests passed 581/581; `ComputerSecuritySoftware.slnx` built with 0 warnings/errors; the observed test DLL remained stable and no Huorong alert appeared. `.omx/qa-home-agent-next-action.png` was inspected and shows the conclusion, safety boundary, and next action in the first working area. Relevant processes and fixtures were clean afterward.
- Blockers: final application-drawer screenshot/UIAutomation rerun is deferred by the Codex GUI quota, not a product failure. Signing, real UAC, native uninstall, installer execution, and real C/D mutation remain separate release gates.
- Exact next action: retain the GUI gap as Warn, continue unit/static work on installer launch readiness, and rerun the deterministic app-drawer smoke when GUI approval is available again.

## Active Slice - 2026-07-14 Source-only home Agent next-action closure

- Objective: turn the home health finding Agent response from a text-only endpoint into one clear, safe next step for a beginner.
- Dependencies: `HealthFinding`, `HomeAgentResponsePresenter`, the first-visible home Agent panel, exact-app re-resolution, the internal page allowlist, and existing Apps/C-drive pages.
- Risks: treating an old app name as identity, navigating to an invented page, making a plan look like execution, hiding the next step below the findings list, or adding operation/process authority to WPF.
- Impact scope: Core home-response navigation metadata, MainWindow response binding and internal navigation, one first-visible button with stable AutomationId, focused source tests, static authority/order checks, and protocol records. No compilation, test execution, GUI, process launch, operation pipeline, registry write, Worker/UAC, or real C/D mutation is allowed.
- Acceptance: every explain/detail/plan response exposes at most one structured next step; exact unique app findings re-resolve into the Apps drawer; other health findings navigate to C-drive evidence; unavailable/ambiguous app targets fall back to generic Apps management without guessing; the button is hidden when invalid; all targets are allowlisted; all responses remain non-executable.
- Status: Source implementation completed. Explain, detail, and plan responses now carry one typed destination. Generic health findings open C-drive evidence; exact software findings reopen the current inventory and require one exact app before the drawer appears. Unavailable/ambiguous targets fall back to generic application management without guessing. The first-visible response panel owns a stable next-step button and all responses remain non-executable.
- Last verification: seven independent source-only checks on 2026-07-14 passed for strict MainWindow XML, all 37 unique Click handlers, unique AutomationIds and title-button-findings order, WPF re-resolution/allowlist/non-authority, closed Core destinations/non-executable factories, focused test-source contracts, and strict UTF-8 across 237 non-generated C#/XAML files. No build, test, GUI, or runtime navigation ran.
- Blockers: corrected Huorong definitions are not installed locally, so this slice must remain uncompiled and unexecuted.
- Exact next action: continue the visible-control outcome audit and close the next source-safe beginner workflow. After corrected Huorong definitions arrive, perform one security-observed `Css.Tests` build, run focused ProductExperience tests, then UIAutomation all three home response buttons and capture the first-visible response/action screenshot.

## Active Slice - 2026-07-14 Source-only real application icons

- Objective: connect the registry `DisplayIcon` evidence to the application grid so recognizable local app icons appear when safely available, with the existing letter tile as a deterministic fallback.
- Dependencies: uninstall registry `DisplayIcon`, `InstalledSoftwareRecord`, `SoftwareInventoryBuilder`, `SoftwareProfile`, `AppTileViewModel`, growth-profile cloning, `AppTileUi`, WPF tile template, and a bounded read-only Windows icon decoder.
- Risks: treating command text as a path, loading UNC/network icons, executing a file or shell extension, leaking icon paths into accessibility text, leaking native icon handles, blocking scans on malformed values, losing icon evidence during profile enrichment, or hiding the fallback when decoding fails.
- Impact scope: Scanner icon-reference parser and Builder propagation, Core profile/tile fields, App read-only icon loader/tile binding, focused source tests, static native-resource/authority checks, and records. No process launch, shell execution, registry write, network path, compilation, test execution, GUI, Worker/UAC, or real C/D mutation is allowed.
- Acceptance: quoted/unquoted local `DisplayIcon` values with optional integer index normalize; environment variables may expand but unresolved, relative, URI, UNC, overlong, control-character, and unsupported extensions refuse; Builder/tile/growth cloning preserve path+index; WPF displays a frozen local icon when decoding succeeds and always destroys native handles; missing/invalid icons show the letter fallback; visible/accessibility copy contains no path.
- Status: Source implementation completed. `DisplayIconReferenceParser` now normalizes only bounded local-drive icon references and optional indexes. Builder, profile, tile, and growth enrichment preserve the reference. The app grid uses a bounded cached loader for frozen raster/native icons and explicitly falls back to the existing letter tile. Native handles are released in `finally`; fixed-drive and reparse checks prevent network/shell-style loading.
- Last verification: seven independent source-only checks on 2026-07-14 passed for parser policy/non-authority, Builder/Profile/Tile/growth propagation, bounded cached loader and native-handle release, WPF real-icon/fallback layers plus XML parsing, focused source tests, all 36 unique Click handlers, and strict UTF-8 across 237 non-generated source/XAML files. Tests and GUI were not run.
- Blockers: corrected Huorong definitions are not installed, so icon code and tests must remain uncompiled/unexecuted; visual icon acceptance requires later UIAutomation and screenshot evidence.
- Exact next action: audit the 36 visible click workflows against real beginner outcomes and close the next safe dead end. After corrected Huorong definitions arrive, build once under observation, run `SoftwareInventoryTests`, `GrowthDecisionTests`, focused ProductExperience tests, then scan fixture/real apps and capture the application grid with both decoded icons and fallback tiles.

## Active Slice - 2026-07-13 Source-only beginner migration wording

- Objective: remove English migration verdicts from the application drawer and exact-app Agent answers so a beginner sees one clear Chinese conclusion, destination, safety limit, and verification plan.
- Dependencies: `AppPresentationBuilder.CreateMigrationSummary`, migration preview lines, `MigrationRiskBand`, existing destination policy, application drawer action host, and Agent app-specific reuse.
- Risks: changing migration logic while translating copy, exposing raw source paths, hiding the destination the user needs, overstating that migration can execute, or leaving stale English-dependent tests.
- Impact scope: App presentation wording and focused source expectations only. No planner/scanner/handler/WPF authority, execution flag, build, test, GUI, Worker/UAC, or real C/D operation is allowed.
- Acceptance: all four migration bands have plain Chinese summaries; preview says target location, why, snapshot requirement, two verification goals, and that no files move from the drawer; D-drive/system/cache-only cases stay distinct; source tests no longer depend on English beginner copy; no execution authority changes.
- Status: Source implementation completed. All four migration bands now have plain Chinese summaries and preview lines. The drawer shows the recommended D-drive destination, snapshot/rollback requirement, normal-start verification, original-C-write verification, and an explicit no-move boundary. Exact-app Agent migration answers reuse the same Chinese conclusion.
- Last verification: six independent source-only checks on 2026-07-13/14 passed for complete Chinese migration copy with no legacy English strings, presentation-only authority, updated drawer/Agent source contracts, MainWindow XML and all 36 Click handlers, strict UTF-8 across 236 non-generated source/XAML files, and unchanged disabled installer/migration gates. Tests were not run.
- Blockers: corrected Huorong definitions are not installed, so changes remain source-only and uncompiled.
- Exact next action: connect real application icons from scanner evidence to the app grid with local-path-only loading and a deterministic fallback. After corrected Huorong definitions arrive, run focused ProductExperience/Agent tests and capture the migration drawer/Agent answer as part of visual acceptance.

## Active Slice - 2026-07-13 Source-only local Computer Agent conversation

- Objective: turn the static Agent page into an actual beginner-facing local question/answer surface grounded in current health and software evidence, with safe internal navigation to the exact next place.
- Dependencies: `HealthCheckSummary`, `SoftwareProfile`, app presentation/startup handoff, exact unique app matching, Agent page XAML, existing internal navigation, and growth-to-app drawer navigation.
- Risks: echoing a private path from the user's question, inventing facts when scans are absent, selecting the wrong same-name app, interpreting free text as execution consent, exposing technical evidence, navigating to an unsafe/unrecognized page, or letting WPF create operations/processes.
- Impact scope: Core Agent conversation intent/presentation, MainWindow Agent bindings/navigation, Agent first-visible XAML controls and AutomationIds, focused source tests, static order/authority checks, and protocol records. No cloud call, compilation, test execution, GUI, process launch, operation pipeline, registry write, Worker/UAC, or real C/D mutation is allowed.
- Acceptance: empty/unknown questions receive honest guidance; core intents cover C drive, installed apps, startup/background, install routing, migration, uninstall, and regret/restore; replies use local evidence and never echo raw paths; exact unique app names can navigate to that drawer while duplicate/missing targets refuse; all other navigation is allowlisted internal pages; every reply is non-executable and says what evidence is missing; question/answer controls have stable AutomationIds and the response panel sits before the static next-step lists.
- Status: Source implementation completed. The Agent page now accepts a local question, classifies C-drive/app/startup/install/migration/uninstall/restore intents, answers from current health/profile evidence, and exposes only allowlisted internal navigation. Exact unique app mentions can open that drawer; duplicate/stale targets refuse. Scan-derived absolute paths are replaced with a beginner-safe fallback, raw questions are never echoed, and every reply is hard-coded local/non-executable.
- Last verification: seven independent source-only checks on 2026-07-13 passed for MainWindow XML parsing, all 36 unique Click handlers, unique AutomationIds and first-visible ordering, WPF navigation-only authority, Core non-authority/path guard, strict UTF-8 across 236 non-generated source/XAML files, and focused test-source coverage. The checks also caught and corrected misplaced/unclosed C-drive and Agent scroll containers. Tests were not run.
- Blockers: corrected Huorong definitions are not installed, so resulting source remains uncompiled and unexecuted; runtime screenshot/AutomationId acceptance must wait.
- Exact next action: continue source-only workflow auditing and close the next beginner-facing dead end without adding system authority. After corrected Huorong definitions arrive, build `Css.Tests` once under observation, then run `AgentConversationTests`, focused Agent/startup/product tests, full regression, UIAutomation the question/answer/navigation flow, and capture the required Agent first-visible screenshot.

## Active Slice - 2026-07-13 Source-only StartupApproved correlation

- Objective: correlate each registry Run observation with its exact Windows StartupApproved registry value as read-only drift evidence, without decoding undocumented bytes or enabling direct startup changes.
- Dependencies: structured `BackgroundComponentObservation`, exact Run hive/key/value identity, read-only `RegistryKey.OpenSubKey`, observation fingerprints, startup settings handoff, and folded technical details.
- Risks: assuming a missing approval value means enabled, interpreting undocumented binary bytes, correlating HKLM 32-bit Run with the wrong approval key, storing raw binary payloads, leaking registry details into the beginner surface, or adding registry write authority.
- Impact scope: Core startup-approval evidence model/factory, Scanner startup records and registry reader, Builder observation correlation, folded technical presentation, focused source tests, static authority checks, and protocol records. No compilation, test execution, GUI/Settings launch, registry write, Worker/UAC, or real C/D mutation is allowed.
- Acceptance: HKCU Run, HKLM Run, and HKLM WOW6432 Run each map to their exact known approval-key locator; missing, unreadable, binary, and unsupported-type evidence remain distinct; no binary state is decoded; raw approval bytes are never retained; approval evidence changes the observation fingerprint but not component identity; effective activation remains unknown; beginner summaries remain path-free; no change operation or registry mutation API exists.
- Status: Source implementation completed. Each Run record now carries a correlated StartupApproved observation with explicit registry view/key/value identity, missing/binary/unsupported/unreadable status, payload length, and SHA-256 drift fingerprint. Raw bytes are discarded, effective activation stays unknown, and no observation can authorize change. HKLM32 Run is explicitly paired with HKLM64 StartupApproved Run32. The Agent tells beginners to confirm the current switch in Windows rather than guessing bytes.
- Last verification: seven source-only static checks on 2026-07-13 passed for fingerprint-only/no-decode Core evidence, explicit Run/approval registry views, read-only Scanner authority, Scanner-to-Builder propagation, folded technical details, path-free beginner wording, focused test-source coverage, UTF-8, and MainWindow XAML parsing. Tests were not run.
- Blockers: corrected Huorong definitions are not installed, so all resulting source must remain uncompiled and unexecuted.
- Exact next action: connect an actual local Computer Agent question/answer surface to health and software evidence, with exact-app targeting, path-free replies, internal navigation only, and no operation authority. After corrected Huorong definitions arrive, compile and run focused startup evidence tests before accepting runtime registry behavior.

## Active Slice - 2026-07-13 Source-only structured background component evidence

- Objective: replace name-only startup/service/task hints with exact, structured, read-only component identities and observations that can support future rollback design without enabling direct system changes.
- Dependencies: `SoftwareProfile`, registry Run inventory, WMI/registry service inventory, scheduled-task XML inventory, `SoftwareInventoryBuilder`, startup settings handoff, and the hidden technical-details surface.
- Risks: treating Run-key presence as proof that Windows currently enables an item, confusing service runtime state with start mode, using display names as identities, claiming an observation hash is restore evidence, exposing technical details in the beginner surface, or accidentally creating registry/service/task execution authority.
- Impact scope: Core background-component evidence/readiness models, Scanner records/read-only collection/building, growth-profile cloning, focused source tests, static authority audits, and protocol records. No compilation, test execution, GUI/Settings launch, registry/service/task mutation, Worker/UAC, or real C/D mutation is allowed.
- Acceptance: each scanner-attributed startup item, service, and scheduled task has a deterministic identity bound to its exact source locator; observed configuration changes alter the observation fingerprint without changing identity; unknown enablement remains unknown; every observation explicitly says it is read-only and not rollback-ready; name-only legacy evidence cannot create a change operation; the beginner drawer remains concise and technical evidence stays behind the secondary details control.
- Status: Source implementation completed. `SoftwareProfile` now carries deterministic component identities and read-only observations alongside the compatible name lists; a profile-level observation snapshot binds the sorted identities/fingerprints; the scanner records exact Run sources, service start/runtime state, and task Settings enablement; every readiness path remains non-executable and rollback-incomplete. Technical identities appear only in the default-folded details surface. The selected-app action label is consistently `管理自启动`.
- Last verification: source-only static checks on 2026-07-13. Core evidence contains required identity/snapshot/rollback-requirement fields and no mutation APIs; Scanner contains required read-only wiring and no registry/service/task/process/file-write authority; missing service values and unknown startup approval remain unknown; task enablement is Settings-scoped; growth cloning preserves observations; technical details remain folded; focused test source is present; UTF-8 replacement scan and MainWindow XAML parse pass. Tests were not run.
- Blockers: corrected Huorong definitions are not installed, so all resulting source must remain uncompiled and unexecuted.
- Exact next action: source-design a separate read-only Windows StartupApproved state reader and correlate it by exact hive/view/value identity without guessing undocumented bytes; keep unknown values unknown and do not create a change operation. After corrected Huorong definitions arrive, first perform one observed build, run focused background-inventory/startup/product tests, then UIAutomation the drawer and capture the required first-visible-area screenshot.

## Active Slice - 2026-07-13 Source-only startup settings handoff

- Objective: replace the app drawer's startup-control dead end with a safe Agent decision and an allowlisted handoff to the official Windows Startup apps settings page, without pretending OMNIX can safely edit unstructured startup/service/task evidence.
- Dependencies: current startup/service/task counts, system-app classification, `WindowsSettingsShortcutCatalog`, existing confirmation/open-only launcher, shared drawer action host, and Microsoft-documented `ms-settings:startupapps`.
- Risks: suggesting a service/task can be changed on the Startup apps page, opening an arbitrary URI, bypassing confirmation, implying Windows changed a setting, or adding direct registry/service/task authority.
- Impact scope: Core startup handoff presentation and settings allowlist, MainWindow shared settings launcher/drawer primary routing, focused source tests, and records. No compilation, test execution, GUI/settings launch, registry/service/task change, or real system mutation is allowed.
- Acceptance: only a non-system profile with at least one identified startup entry gets the settings button; service/task-only and system profiles remain explanation-only; the button resolves the fixed allowlisted shortcut, confirms, and opens only `ms-settings:startupapps`; copy says the user must choose in Windows and OMNIX changes nothing; no direct mutation operation is created.
- Status: Source implementation completed for this slice. A non-system app with at least one ordinary startup entry now gets a path-free Agent handoff and `在 Windows 中查看` action. System apps and service/task-only evidence remain explanation-only. The action resolves the fixed catalog id, verifies `IsOpenOnly` and `ms-settings:`, confirms, and delegates to the shared settings launcher; no change operation is created. The drawer label now says `管理自启动` rather than promising an immediate close.
- Last verification: source-only static checks on 2026-07-13. Microsoft Learn documents `ms-settings:startupapps`; MainWindow XAML parses and retains stable drawer/button AutomationIds; all 34 click handlers resolve; catalog id/URI/medium-risk confirmation pass; drawer owns no process/registry/service/task/pipeline authority; presenter separates normal startup from system/service/task evidence and creates no `OperationDescriptor`; UTF-8 scan passes. Settings was not launched.
- Blockers: corrected Huorong definitions are not installed; this slice remains source-only and must not launch Settings during development.
- Exact next action: source-design structured startup component identities and read-only snapshots for future rollback-capable direct management, but do not enable registry/service/task execution. After corrected Huorong definitions arrive, build once under observation, run focused startup/catalog/product tests, then UIAutomation the confirmation and inspect the opened Windows page before accepting runtime behavior.

## Active Slice - 2026-07-13 Source-only application cache quarantine closure

- Objective: replace the application drawer's cache-cleanup dead end with a beginner-visible, reversible flow that can move only proven low-risk cache directories into quarantine after confirmation.
- Dependencies: current `SoftwareProfile` cache/running-process evidence, current-user data roots, strict cache path/reparse validation, unique app re-resolution, `QuarantineOperationPolicy`, `SafetyOperationPipeline`, timeline, and drawer action host.
- Risks: trusting a broad or forged cache path, moving data while the app is running, following a junction, partial multi-path quarantine, losing the result when inventory refreshes, exposing local paths, or treating Agent preview as consent.
- Impact scope: Core cache cleanup plan/path policy and drawer presentation, MainWindow preview/confirmation/pipeline/result wiring, focused source tests, and records. No compilation, test execution, GUI, cleanup, quarantine move, registry write, Worker/UAC, or real C/D mutation is allowed during this slice.
- Acceptance: only bounded existing non-reparse cache-named directories below current-user data roots can enter an operation; system/active/unknown/ambiguous apps refuse; the user sees a path-free plan and separate confirmation; execution re-scans the exact app and revalidates paths immediately before the existing safety pipeline; success leaves a timeline/restore entry and a visible conclusion; all mutation remains user-triggered and reversible.
- Status: Source implementation completed for this slice. The drawer now builds a strict cache plan, shows a separate continue button, confirms with the user, re-scans the exact unique app, refuses running/system/changed/ambiguous evidence, revalidates bounded current-user cache paths in a specialized handler, then uses the existing quarantine pipeline. Success returns to a visible Agent conclusion and timeline navigation. Quarantine now persists its recovery manifest before moving and compensates already-moved items when a later move or timeline write fails.
- Last verification: source-only static checks on 2026-07-13. MainWindow XAML parses; all 34 click handlers resolve; cache coordination contains confirmation, current inventory/profile/path rechecks, specialized handler, pipeline, and timeline but no WPF process/registry/file-move authority; path policy is bounded and fail-closed for system/running/reparse/outside/overlap cases; manifest-before-move and timeline-inside-rollback ordering pass; stale test assumptions and UTF-8 replacement characters are absent. New behavior tests remain unrun.
- Blockers: corrected Huorong definitions are not installed; this slice must remain source-only and unexecuted.
- Exact next action: source-design the startup-control safety contract: item-level identity, current-state snapshot, exact rollback evidence, system/service/task refusal rules, and a plan-only UI that cannot enable anything until runtime/security gates pass. After corrected Huorong definitions arrive, first perform one observed build and run the focused cache/quarantine tests before any real cache path is touched.

## Active Slice - 2026-07-13 Source-only growth-to-app navigation

- Objective: connect a software-attributed growth conclusion to the exact application drawer so a beginner can move from “who is growing” to understandable cache/migration/uninstall previews without searching the app grid or exposing technical paths.
- Dependencies: structured `HealthFinding` and `GrowthDecisionViewModel` targets, current `SoftwareProfile` inventory, exact unique app resolution, existing app catalog filters/selection/drawer, and home/C-drive event handlers.
- Risks: parsing a localized sentence as identity, selecting the wrong app when names collide, silently hiding filters, scanning software with side effects, navigating when attribution is shared/unknown, or letting navigation imply execution authority.
- Impact scope: Core target resolution/presentation, Scanner growth/home target propagation, MainWindow read-only navigation, one compact C-drive button, focused source tests, and records. No build, test execution, GUI launch, Worker/UAC, installer, cleanup, registry write, or real C/D mutation is allowed.
- Acceptance: only a unique exact current-profile name can open a drawer; shared/non-software findings have no target; missing inventory may trigger one read-only inventory scan; missing/ambiguous results show a path-free Agent refusal; home details and C-drive growth both select the matching app in the Apps page; stable AutomationIds and static authority/order checks exist; no direct action is executed.
- Status: Source implementation completed for this slice. Growth and home findings carry a structured application target only for software attribution; a unique exact current profile opens the Apps page and drawer, while missing, ambiguous, shared, and unavailable inventory states stop with a path-free Agent explanation. Recent growth is also enriched into application tiles/drawers and reapplied through one centralized profile setter after every inventory refresh.
- Last verification: source-only static checks on 2026-07-13. MainWindow XAML parses; required AutomationIds and first-view order pass; only the profile field initializer and centralized setter assign `_softwareProfiles`; target navigation contains no process, pipeline, registry, move, or delete authority; UTF-8 replacement-character scan passes. Test source covers exact/missing/ambiguous targets, software-only propagation, nested growth deduplication, and stale-growth clearing. No compiler or runtime was used.
- Blockers: corrected Huorong definitions are not installed, so all new source remains uncompiled and unexecuted.
- Exact next action: continue source-only auditing of user-facing controls and close the next preview-only/dead-end entry. After corrected Huorong definitions arrive, build once under observation, run focused target/growth/product tests, UIAutomation both entry points, and capture real first-visible screenshots before accepting this slice as runtime verified.

## Active Slice - 2026-07-13 Source-only software-attributed growth decisions

- Objective: turn C-drive snapshot deltas from broad top-level folders into honest software-attributed observations and a proactive Agent conclusion that separates one-time relief from preventing future growth.
- Dependencies: existing recursive category tree, `SoftwareProfile` install/data/cache/log/C-write paths, SQLite snapshot store, `GrowthAnalyzer`, beginner growth presenter, and App scan flow.
- Risks: claiming a newly added watch point is historical growth, assigning a shared path to the wrong app, double-counting parent and child deltas, exposing private paths in beginner text, or turning a two-snapshot guess into an executable cleanup.
- Impact scope: Css.Scanner snapshot item construction/analyzer/presentation, MainWindow growth conclusion binding, focused source tests, and records. No build, test execution, cleanup, process launch, registry write, or real path mutation is allowed.
- Acceptance: snapshot keeps broad roots plus bounded exact software-known paths found in the scanned tree; ambiguous paths remain shared; first observation is labeled as baseline rather than growth; Agent conclusion states evidence limits, offers separate “现在” and “以后” advice, contains no raw C path, and cannot execute directly; UI has stable AutomationIds and shows the conclusion before technical details.
- Status: Source implementation completed for this slice. Snapshots are globally bounded to 2,048 monitored locations, exact software-known paths remain attribution evidence, SQLite keeps at most 90 snapshots per scan root and loads the latest eight, trend enrichment requires at least three contiguous recent observations and repeated positive intervals before saying "持续增长", and parent-directory findings are folded only when attributed children explain at least 80% with no weaker trend evidence. C-drive and home Agent conclusions are path-free, separate current relief from future prevention, and cannot execute directly.
- Last verification: static source inspection only on 2026-07-13. MainWindow XAML parses; home/growth conclusion AutomationIds and order pass; scoped authority/private-path, UTF-8 replacement-character, retention/wiring, trend-threshold, and non-execution invariant scans pass. The invalid backslash literal and global snapshot-bound defect found during review were corrected. No compiler, test runner, GUI, Worker, UAC, installer, cleanup, or real C/D mutation was used.
- Blockers: corrected Huorong definitions are not installed, so new source will remain uncompiled and unexecuted.
- Exact next action: after corrected Huorong definitions arrive, build once and inspect the generated test assembly before running the focused growth/store/home tests, then run full regression and capture real first-view screenshots for the C-drive and home Agent conclusions. Until then continue only source/static work and keep installer/migration execution disabled.

## Active Slice - 2026-07-13 Source-only trusted installer routing

- Objective: move install control from filename-only advice to hash/signature-bound package evidence, reliable installer-kind detection, an auditable interactive routing plan, and a future post-install report without launching an installer during the antivirus-definition pause.
- Dependencies: existing `InstallerAnalyzer`, routing memory, Authenticode verifier, bounded binary inspection, recognized interactive directory parameters, before/after software snapshots, `SafetyOperationPipeline`, and an injected process boundary.
- Risks: treating filename guesses as execution evidence, passing unsupported MSI/Burn properties, launching a file changed after confirmation, silently requesting elevation, exposing raw paths in beginner conclusions, or claiming a process start proves installation success.
- Impact scope: Css.InstallGuard installer evidence/planning, focused source tests, later App confirmation/coordinator, and records. No build, test execution, installer process, UAC, registry mutation, or real install directory write is allowed in this slice.
- Acceptance: selected package must exist, have bounded metadata and SHA-256, and have a known signature state; binary/extension evidence overrides filename hints; only high-confidence Inno/NSIS routing receives automatic directory arguments; MSI/Burn/MSIX/unknown remain guided until package-specific evidence exists; any later launch must revalidate the exact hash and require final confirmation plus a before snapshot.
- Status: Source implementation completed for this slice. Package inspection, conservative routing, snapshot-bound operation planning, four-item fresh final consent, handler-side revalidation, injected interactive process boundary, automatic initial post-scan coordination, beginner result presentation, and WPF source wiring are present. `InstallerLaunchFeatureEnabled` remains false.
- Last verification: static source inspection only on 2026-07-13. Main/install consent/install result XAML parse as XML; AutomationIds and conclusion order are present; the scoped authority scan finds exactly one `Process.Start` in `WindowsInteractiveInstallerProcessLauncher` and no `runas`, hidden window, or silent switch in production install source. Existing executable evidence predates the antivirus stop and does not cover this new slice.
- Blockers: corrected Huorong definitions are not installed, so new source cannot be compiled or executed yet.
- Exact next action: after corrected definitions arrive, build once, inspect Huorong, run installer package/launch/coordinator/product tests, UIAutomation the four-item consent, and capture the Agent conclusion first view. Until then keep `InstallerLaunchFeatureEnabled=false`; continue source-only work on growth findings and Agent decision closure.

## Active Slice - 2026-07-13 Source-only migration snapshot closure

- Objective: complete the migration snapshot evidence chain and App-side production coordinator in source while Huorong definitions are pending, without generating or running assemblies.
- Dependencies: rollback-manifest correlation, bounded atomic snapshot evidence, fixed-time SHA-256 verification, migration execution gate, authenticated lifecycle, package trust, and path-free beginner results.
- Risks: treating a display-only snapshot id as evidence, leaving a migration request construction without evidence path/hash, allowing WPF to own worker mode/process authority, or claiming uncompiled source as verified behavior.
- Impact scope: migration Core/Win32/App/Ipc source, focused test source, and protocol records. No build, test run, Worker/UAC launch, whitelist, artifact restore, or real C/D mutation is allowed in this slice.
- Acceptance: every executable migration descriptor carries a real evidence file and SHA-256; handler revalidates evidence identity/freshness/manifest/source set before activity or path mutation; package/coordinator tests express fail-closed trust and response correlation; WPF remains free of direct worker launch authority; all new source is explicitly marked uncompiled until corrected definitions are installed.
- Status: Source implementation and static audits completed for this slice. Snapshot evidence, current-source revalidation, production coordinator correlation, and four-acknowledgement final consent are source-wired; MainWindow still keeps migration execution disabled.
- Last verification: source inspection only on 2026-07-13. Migration XAML parsed as XML; WPF authority and encoding scans passed. Last executable evidence remains 8/8 protocol tests before the antivirus stop; vendor confirmed the earlier alert was a false positive.
- Blockers: corrected Huorong definitions are not installed locally, so compilation/runtime acceptance remains deferred.
- Exact next action: after corrected definitions arrive, perform one narrow build and security scan, then run the focused snapshot/execution/worker/coordinator/consent tests and capture the final-consent first view with fixture data. Keep production migration disabled and do not touch real software paths.

## Active Slice - 2026-07-13 Authenticated migration worker protocol

- Objective: define a migration-specific confirmed request, authenticated message, strict pipe codec, typed path-free response, and identity-bound one-shot worker server while reusing only the proven bootstrap/pipe identity primitives from the uninstall transport.
- Dependencies: `MigrationExecutionGate`, canonical descriptor hashing with string-list arguments, bounded strict JSON, HMAC request/response correlation, replay/freshness checks, one-shot named-pipe bootstrap, and injected response handler.
- Risks: accidentally serializing arrays as type names, accepting editable descriptors whose hash does not bind every path/component, reusing uninstall result payloads, leaking paths through errors, replaying a confirmed request, or authorizing after bootstrap.
- Impact scope: Css.Core migration request contract, new Css.Ipc migration protocol/server, focused protocol tests, and records. No worker mode, App launcher, WPF execution, UAC, or real filesystem mutation in this sub-slice.
- Acceptance: final confirmation is fresh and exact; descriptor hash binds scalar and ordered list values; codec supports only string/bool/string-list arguments and rejects unknown fields/types/oversize; authenticated endpoint rejects tamper/replay/stale requests before handler; response is request-correlated and path-free; package authorization occurs before bootstrap; all tests use memory streams or injected pipe identities.
- Status: Protocol implementation completed. Huorong explicitly confirmed the detection was a false positive. At the user's request, source development now continues while all compilation, test execution, Worker launch, UAC, and real C/D mutation remain paused until corrected local definitions arrive.
- Last verification: focused protocol tests passed 8/8 before the security stop. Vendor analysis now clears the malware classification, but a clean rebuild under the corrected local definitions is still required before artifact acceptance returns to Pass.
- Blockers: corrected Huorong definitions are not yet evidenced, so compile/runtime verification is deferred. This does not block source-only composition and static authority audits. No OMNIX/Css/dotnet test process remains.
- Exact next action: implement source-only Elevated migration package authorization, freshness/hash revalidation, `SafetyOperationPipeline` composition, actual process/service/task activity probes, monitoring storage, and worker mode. Add tests as source but do not build them. Record every unverified assumption for the post-update verification pass.

## Active Slice - 2026-07-13 Windows directory migration adapter and result UI

- Objective: implement directory-only Windows copy/hash/redirect/rollback mechanics behind the verified migration engine, then add a path-free beginner result window with stable automation evidence while keeping real MigrationPlanWindow execution disabled.
- Dependencies: `IMigrationPathAdapter`, strict Windows path policy, bounded reparse-safe tree copier, injected redirect primitive, handler reverse rollback, result presenter, WPF AutomationIds, and render test.
- Risks: following reparse points, deleting source before a verified destination exists, destination collision, partial copy, redirect creation failure after source removal, deleting the only valid copy during rollback, exposing local paths in beginner text, or accidentally enabling the user button.
- Impact scope: Css.Win32 migration adapter/copier/redirect boundary, focused temporary-directory tests, Core/App result presentation, new result window, screenshot, and records. Main migration WPF remains preview-only and no real C/D directory is touched.
- Acceptance: only directories are supported; source/destination/staging are exact and collision-free; traversal is bounded and rejects every reparse entry; all copied files are size/SHA-256 verified before source removal; redirect is injected and independently re-observed; redirect failure restores source and removes destination/staging; rollback never deletes an unverified sole copy; beginner states cover completed/refused/rolled-back/incomplete with no paths; AutomationIds/order/render screenshot pass; no execution control is added to MigrationPlanWindow.
- Status: Completed and verified. Css.Win32 now provides a directory-only adapter, bounded reparse-rejecting tree copier, per-file size/SHA-256 comparison, unique destination staging, commit-before-source-removal order, injected redirect primitive, independent redirect observation, and restore-from-destination rollback. Target collision stops before copy; redirect failure after source removal is recoverable; no shell command is used. A path-free migration result presenter/window covers completed, refused, rolled-back, and incomplete rollback states with stable AutomationIds. Main migration WPF remains preview-only.
- Last verification: adapter/engine/presentation focused 19/19 Debug and Release; full 460/460; Debug/Release builds 0 warnings/errors. `.omx/qa-runtime-migration-result.png` was inspected at 680x430 with all conclusions visible. No Css/OMNIX process or Windows migration temp root remains.
- Blockers: current normal process cannot create directory symbolic links (`UnauthorizedAccessException` probe), so native redirect creation must run in the signed/elevated worker and still needs manual fixture rollback evidence. This does not block worker integration.
- Exact next action: define a migration-specific authenticated one-shot worker contract or safely generalize the existing worker transport, then compose `MigrationOperationHandler` in Css.Elevated with the Windows adapter, actual process/service/task activity probe, strict path policy, monitoring store, and timeline. Current unsigned package must self-deny before request; injected signed tests must execute only temporary fixture roots. Add App coordinator and DEBUG fixture WPF flow before enabling real plan confirmation.

## Active Slice - 2026-07-13 Migration engine, rollback, and write-return monitoring

- Objective: implement the mutation-neutral migration coordinator boundary that verifies hashed rollback evidence, blocks active components/unsafe paths, delegates bounded move+redirect steps, rolls back in reverse on failure, and persists monitor records that detect original-path writes returning after migration.
- Dependencies: migration plan/manifest/gate, SHA-256 verified structured manifest reader, operation descriptor, path policy, activity probe, move/redirect adapter, observation reader, monitor store, and `SafetyOperationPipeline` integration contract.
- Risks: trusting an editable JSON manifest, moving overlapping/root/system paths, partial cross-volume loss, treating user “closed” confirmation as process truth, failing rollback, or considering a newly recreated C directory a successful redirect.
- Impact scope: Core migration contracts/verification/coordinator/monitoring, gate metadata, temporary-directory/injected-adapter tests, and records. No WPF execution or real C/D path mutation in this slice.
- Acceptance: manifest bytes are bounded and fixed-time SHA-256 verified; manifest/request/path sets and destination are exact; active components and unsafe/overlapping paths fail before mutation; each move is delegated and recorded; failure rolls back reverse order with typed incomplete state if needed; success stores expected redirects; monitor detects missing/replaced redirects and direct original-path recreation; tests prove no mutation on denial and no real user paths are touched.
- Status: Completed and verified. Rollback manifests are atomically written, bounded to 1 MiB, SHA-256 evidenced, and fixed-time verified before coordinator use. The operation handler checks exact snapshot/destination/path-set correlation, freshness, active components, source/destination observations, and injected path policy before any move. It delegates move+redirect, rolls attempted entries back in reverse on failure/cancellation, distinguishes incomplete rollback, and persists expected redirect records. The closure monitor detects missing/replaced/wrong-target redirects. A strict Windows policy allows only user-side C paths to approved D roots and blocks Windows/Program Files/ProgramData/system locations. WPF remains preview-only.
- Last verification: focused migration 24/24 Debug and Release; full 449/449; Debug/Release builds 0 warnings/errors. No Css/OMNIX process or migration temp root remains. Static audit finds no real user-path move/redirect/process authority in the engine or MigrationPlanWindow; only atomic monitoring JSON rename uses `File.Move`.
- Blockers: none for injected engine implementation. Actual Windows redirect adapter and WPF enablement remain later high-risk slices.
- Exact next action: implement a Windows migration path adapter for directories only using copy-to-staging, bounded tree/hash verification, source removal, explicit redirect creation, and restore-on-failure. Keep it constructible only with `WindowsMigrationPathPolicy`; reject files/reparse sources/collisions. Add a beginner result presenter/window with stable AutomationIds and screenshot. Connect only a fixture-enabled WPF coordinator first; real C/D execution remains disabled until manual rollback and redirect evidence pass.

## Active Slice - 2026-07-13 Production residue review linkage

- Objective: after a request-correlated production post-scan, refresh software inventory and feed the captured pre-uninstall profile into the existing local residue risk review, confirmed low-risk quarantine, timeline, and restore flow without sending residue paths through IPC.
- Dependencies: `UninstallPlanWindow` production outcome flags, captured `SoftwareProfile`, local software rescan, `UninstallResidueScanBuilder`, review presentation, quarantine policy/pipeline, timeline, and regret center.
- Risks: trusting path counts as executable evidence, losing the removed app profile after catalog refresh, showing residue review for incomplete uninstall, auto-opening mutation without confirmation, or overwriting review UI during refresh.
- Impact scope: `UninstallPlanWindow` outcome handoff, MainWindow residue review refactor/order, focused tests, and records. Elevated/path-free IPC schema remains unchanged; high-risk residues remain read-only.
- Acceptance: only completed production post-scan can request local residue review; exact captured pre-uninstall profile drives the local scan even if the tile disappears; refreshed inventory is reused; review remains visible after catalog refresh; low-risk action still requires confirmation and `SafetyOperationPipeline`; high-risk groups cannot enter quarantine; no path list crosses IPC.
- Status: Completed and verified. A completed production post-scan now returns outcome flags to MainWindow, which keeps the exact captured pre-uninstall profile, refreshes installed software once, and reuses that inventory for local residue risk grouping. Path lists remain out of IPC. Low-risk cache/log candidates still require confirmation and `SafetyOperationPipeline` quarantine; medium/high-risk groups stay explanation-only. Catalog refresh now happens before inline review display so the result is not overwritten when the removed app tile disappears.
- Last verification: focused residue/coordinator/quarantine 29/29 Debug and 31/31 Release; full 441/441; Debug/Release builds 0 warnings/errors. Static audit confirms no `ResidueReport` in the wire schema, captured profile reuse, safety pipeline and confirmed quarantine; no Css/OMNIX process remains.
- Blockers: Native signed production evidence is unavailable; injected completed-post-scan outcomes and local fixtures will cover the linkage.
- Exact next action: connect the migration plan to an execution coordinator with the same standards: snapshot and rollback manifest must be revalidated, only cache-only/safe scored paths can move, processes/services must be closed or block execution, destination space and path policy must pass, links/config redirection must be explicit, and post-migration monitoring must detect writes returning to the original C path. Start with injected filesystem/process adapters and keep system-wide migration disabled until rollback and screenshot evidence pass.

## Active Slice - 2026-07-13 WPF production execution coordinator

- Objective: carry the final verified one-time uninstall request out of `UninstallPlanWindow` through a trust-gated production coordinator and return a beginner result, while current unsigned builds stop before runner creation or UAC.
- Dependencies: current-package trust reassessment, trusted production launcher factory, production lifecycle runner, typed elevated response presenter, final-consent request, and existing result windows.
- Risks: constructing production mode directly in WPF, showing UAC for an unsigned package, treating transport completion as uninstall success, losing the one-time request, or showing a post-scan result for a failed lifecycle.
- Impact scope: new App coordinator/runner boundary, `UninstallPlanWindow`, MainWindow status handoff, focused coordinator/WPF tests, and records. No test invokes UAC or a real uninstaller.
- Acceptance: WPF depends only on an injected coordinator; unsigned trust invokes no runner/UAC; trusted typed completion returns the post-scan view; lifecycle failures return path-free summary without a post-scan claim; MainWindow injects the current-package coordinator; production command construction remains outside WPF; full tests/builds stay green.
- Status: Completed and verified. The final verified request now crosses an injected App coordinator. Current unsigned package evidence returns the existing development-only trust conclusion before runner creation or UAC. Trusted evidence creates the production lifecycle runner, and only a typed `CompletedProduction` response can become a post-scan view. WPF itself contains no production launcher, mode string, lifecycle client, or process start.
- Last verification: focused coordinator/plan/lifecycle/presentation 43/43 Debug; bootstrap/coordinator 11/11; two consecutive full runs 440/440; focused Release 67/67; Debug/Release builds 0 warnings/errors. Static WPF authority audit passes and no Css/OMNIX process remains.
- Blockers: Positive native execution still requires a same-certificate signed package and manual UAC evidence; current unsigned builds intentionally stop before those gates.
- Exact next action: carry the typed production post-scan result into the existing app-level residue review and quarantine flow: preserve the exact software/profile/request correlation, show risk groups, permit only low-risk confirmed quarantine through `OperationPipeline`, refresh the app tile, and expose restore in the regret center. Keep high-risk residue read-only.

## Active Slice - 2026-07-13 App-side trusted production lifecycle

- Objective: connect the App-side one-shot uninstall lifecycle to the registered production worker mode without making unsigned or weakly assessed packages executable, and return a typed production result suitable for beginner presentation.
- Dependencies: `OfficialUninstallWorkerTrustResult`, exact packaged worker SHA-256 evidence, strict worker launch arguments, authenticated one-shot lifecycle, typed elevated response, and existing production worker authorization/post-scan.
- Risks: reusing the DEBUG fake launcher for production, allowing construction from development-only trust, confusing fake verification with a real completed operation, exposing technical paths in UI, or wiring WPF before result evidence is complete.
- Impact scope: Ipc lifecycle contracts/client, App launcher/presenter, focused tests, screenshots, and protocol records. This slice will not invoke a real uninstaller, mutate residues, or connect the production WPF button until automated result coverage passes.
- Acceptance: launch mode is explicit and authenticated by code ownership; production launcher construction requires `CanLaunchProduction=true` plus the exact trusted worker hash; production arguments contain no fake switches; lifecycle returns distinct production completion; injected trusted end-to-end response preserves uninstall/post-scan payload; beginner presentation is path-free with stable AutomationIds and screenshot evidence; full tests/builds remain green.
- Status: Completed and verified. App now has separate development and production launcher types; only the production type satisfies the production lifecycle marker and its private factory refuses anything except `CanLaunchProduction=true` with an exact trusted worker hash. Lifecycle completion is typed as fake or production, mismatched launcher/method combinations fail before process start, and production payloads become path-free beginner conclusions.
- Last verification: focused 54/54 Debug and Release; full 436/436; Debug/Release builds 0 warnings/errors. Static audit found no fake switches or secret channels in the production launcher, no Css/OMNIX process remains, and `.omx/qa-runtime-production-worker-result.png` was inspected with every conclusion visible in the first 680x430 view.
- Blockers: Native positive production execution still requires a same-certificate signed OMNIX package and manual secure-desktop UAC evidence. These do not block injected production lifecycle implementation and tests.
- Exact next action: inject a production execution coordinator into `UninstallPlanWindow` so the final verified request can evaluate package trust, stop unsigned builds before UAC, run `RunProductionOnceAsync` only through the trusted production launcher, and present the typed lifecycle/post-scan result. Keep native signed-package execution and manual UAC evidence as release gates.

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

## Active Slice - 2026-07-12 Authenticated request preparation freshness

- Objective: preserve the time when final consent produced the one-time uninstall request, bind it into authentication and strict wire serialization, and reject stale/future-dated requests before any production handler or official uninstaller launch.
- Dependencies: Core request draft/composer, authenticated canonical tag, strict pipe schema, endpoint age validation, Elevated session defense-in-depth clock, direct test drafts, and protocol versioning.
- Risks: authenticating only message send time while replaying an old consent request, allowing wire tampering of preparation time, breaking serialized compatibility silently, returning an untyped failure the response codec cannot carry, or making development fixtures accidentally timeless.
- Impact scope: Core request contract, Ipc auth/codec/endpoint, Elevated production session, focused tests, and records. No Program/WPF production registration, real uninstaller launch, UAC prompt, or residue mutation.
- Acceptance: Ready requests require UTC preparation evidence; composer sets it from the verified final-consent transaction; auth tag and schema-v2 wire include it; endpoint rejects requests older than 15 minutes or more than 30 seconds in the future before handler; production session independently rechecks; stale/tampered tests invoke neither launcher nor scanner; current valid flows remain green.
- Status: Completed and verified. Ready requests now carry the exact final-confirmation time; it is required by `CanSubmit`, included in HMAC canonicalization, serialized in strict schema v2, and checked by both the authenticated endpoint and Elevated production session. Requests older than 15 minutes, more than 30 seconds in the future, or modified after authentication invoke neither launcher nor scanner.
- Last verification: protocol/session focused 47/47 Debug; Release high-risk 73/73; full 421/421; Debug/Release builds 0 warnings/errors. Static audit confirms confirmed-time source, HMAC binding, schema v2, dual freshness checks, no Program registration, and no process residue. One unrelated bootstrap tamper test transiently returned an alternate fail-closed classification once, then passed standalone 10/10 and the repeated full suite.
- Blockers: None.
- Exact next action: compose the actual production worker command mode with real official-uninstaller launcher and manifest-bound read-only post-scan factory, but keep App/WPF unable to request that mode and prove current unsigned builds self-deny before bootstrap.

## Active Slice - 2026-07-12 Elevated-side package authorization before bootstrap

- Objective: make the elevated worker independently authorize the actual connected App process and itself as trusted, identically signed OMNIX package binaries after OS pipe-peer identity validation but before ECDH bootstrap or request transfer.
- Dependencies: reusable limited-rights process image-path resolver, Windows Authenticode verifier, exact signer thumbprint comparison, one-shot server authorization hook, typed denial status, and an unregistered production session composition.
- Risks: trusting command-line client metadata, authorizing before real pipe-peer validation, accepting unsigned/mismatched files, running the request handler after denial, duplicating native process-query code, or accidentally registering the production mode in `Program`/WPF.
- Impact scope: Win32 process-path resolver, App inspector refactor, Ipc one-shot authorization hook, Elevated package authorizer/session, focused tests, and records. No production Program mode, WPF connection, real uninstaller process, UAC prompt, or residue mutation.
- Acceptance: actual connected client identity is established first; authorization sees that actual PID plus current worker PID; both images must pass Windows embedded-signature trust and exact thumbprint equality; denial occurs before bootstrap/request handler and produces no operation result; authorized fake composition executes exactly once through `SafetyOperationPipeline`; current unsigned local package is denied; Program and production UI stay disconnected.
- Status: Completed and verified. Windows process-image resolution is centralized in Win32. The one-shot server validates the actual pipe peer, then runs package authorization before bootstrap. Elevated independently resolves the connected App and current worker images, requires Windows-trusted embedded signatures and exact signer-thumbprint equality, and exposes an unregistered production session that enters the handler only through `SafetyOperationPipeline`.
- Last verification: focused authorization/lifecycle/session 35/35 Debug and Release; full 417/417; Debug/Release builds 0 warnings/errors. Current unsigned Release App/worker are denied. Static order/thumbprint/no-mutation/Program/WPF/process audits pass.
- Blockers: A genuinely signed OMNIX package is unavailable, so positive native authorization will use injected trusted evidence while current local binaries must prove denial.
- Exact next action: bind request preparation time into the authenticated wire request and make the Elevated production session reject stale or future-dated final-consent requests before the pipeline. Keep Program/WPF disconnected.

## Active Slice - 2026-07-12 Mandatory post-scan after every started uninstaller

- Objective: ensure every official uninstaller that actually starts and returns an exit code is followed by the mandatory read-only inventory/background/residue scan, including non-zero exit codes that may represent cancellation or partial changes.
- Dependencies: `OfficialUninstallOperationHandler`, typed payload truth, cancellation propagation, read-only post-scanner boundary, timeline evidence, and focused fake launcher/scanner tests.
- Risks: reporting a non-zero exit as success, skipping evidence after partial uninstall, hiding post-scan failure, swallowing operation cancellation, or accidentally enabling residue mutation/production registration.
- Impact scope: Elevated handler semantics and tests plus protocol records. No Program registration, production WPF connection, real process execution in tests, residue deletion, registry/service/task mutation, or UAC prompt.
- Acceptance: not-started launch performs no post-scan; any started launch with an exit code calls post-scan exactly once; non-zero remains an unsuccessful uninstall result but carries read-only findings; post-scan failure requires retry; caller cancellation propagates; static read-only/unregistered boundaries remain true.
- Status: Completed and verified. The handler now scans after every started launcher that returns an exit code, including non-zero cancellation/partial-change outcomes; the exit remains unsuccessful, findings are attached, scan failure requires retry, and caller cancellation propagates. A launcher that never starts still performs no scan.
- Last verification: handler 11/11; uninstall subsystem 35/35 Debug and Release; full 405/405. Static order/read-only/unregistered audit passes and no Css/OMNIX process remains.
- Blockers: None.
- Exact next action: preserve these mandatory post-scan semantics when composing the unregistered production one-shot worker behind independent signed-package authorization.

## Active Slice - 2026-07-12 Post-start worker image correlation

- Objective: close the remaining worker-launch TOCTOU gap by verifying the actual started process image path and SHA-256 against the exact packaged worker evidence before opening the authenticated pipe or sending an uninstall request.
- Dependencies: injected lifecycle process contract, exact packaged path/hash supplied by the launcher, Windows process-image inspection, fixed-time hash comparison, bounded whole-tree termination, and beginner path-free presentation.
- Risks: trusting only the pre-launch file, normalizing paths incorrectly, hashing a different file than the running image, leaking paths/hashes to the beginner UI, or allowing bootstrap/request transfer after an inspection failure.
- Impact scope: neutral worker lifecycle contracts/client, App Windows launcher/image inspector, lifecycle/presentation tests, and protocol records. No production WPF wiring, real uninstall handler, official uninstaller execution, residue scan, or system mutation.
- Acceptance: after `runas` returns a process and before pipe bootstrap, the lifecycle obtains independently inspected image evidence; exact normalized path and 32-byte SHA-256 must match launch evidence in fixed time; missing/mismatched/failed evidence returns a distinct fail-closed status and terminates the process tree; tests cover success, path mismatch, hash mismatch, and inspection failure; beginner copy is path-free.
- Status: Completed and verified. A started worker now carries exact expected image evidence; before pipe connection the lifecycle independently queries the actual process image through limited process rights, hashes it, compares normalized path plus SHA-256 in fixed time, and fails closed with whole-tree cleanup on missing, mismatched, or unreadable evidence. The beginner result is path-free and explicitly says OMNIX stopped before any uninstall.
- Last verification: lifecycle/presentation 28/28 Debug; worker trust/lifecycle/presentation 40/40 Release; full suite 402/402; Debug/Release builds 0 warnings/errors. Static authority/order/package/process audits pass. Fresh `.omx/qa-runtime-worker-image-rejected.png` was inspected at 680x430 with every conclusion/action visible and no overlap or technical leakage.
- Blockers: Manual secure-desktop UAC Accept/Cancel evidence and a genuinely signed OMNIX package remain external/manual, but neither blocks this injected safety slice.
- Exact next action: retain manual UAC Accept/Cancel as an external smoke, then implement an injectable production one-shot worker composition that independently refuses unsigned/mismatched App-worker trust, invokes only the official uninstaller, and always runs a read-only post-scan. Keep production WPF and residue mutation disconnected.

## Active Slice - 2026-07-12 Signed worker production trust gate

- Objective: require Windows-validated Authenticode trust and an identical signer certificate for `Css.App.exe` and its packaged sibling worker before any future production execution can be authorized, while allowing unsigned local builds only for the explicitly DEBUG fake lifecycle verification.
- Dependencies: WinVerifyTrust generic verification, signer certificate thumbprint/subject extraction, exact sibling availability, SHA-256 evidence, path-free trust presentation, and the existing DEBUG smoke boundary.
- Risks: treating a certificate subject as identity, accepting an invalid/expired/revoked signature, trusting an unsigned development binary for production, leaking certificate/path/error details to beginners, TOCTOU between assessment and launch, or accidentally enabling a production route.
- Impact scope: Win32 Authenticode verifier, App trust assessment/presentation, DEBUG smoke preflight, focused trust/static tests, and protocol records. No production request transfer, real handler registration, official uninstaller launch, or system mutation.
- Acceptance: Windows trust result is authoritative; trusted status requires a valid signer certificate and thumbprint; App and worker thumbprints must match exactly; all other states fail closed with plain path-free Agent guidance; development verification and production authorization are distinct booleans; current unsigned builds are proven production-blocked; verifier/policy are rechecked by tests for missing, unsigned, invalid/untrusted, mismatch, probe failure, and same signer.
- Status: Completed and verified. Win32 now validates embedded Authenticode with `WinVerifyTrust`, extracts signer thumbprint/subject only after Windows trust succeeds, and hashes the file. App requires both trusted files and an identical thumbprint for production; only a pair of unsigned hash-readable local binaries can enter explicit DEBUG fake verification. Launcher rechecks the expected worker SHA-256 immediately before `runas`.
- Last verification: trust policy 12/12; impacted worker/product 185/185; Release trust+presentation+lifecycle 34/34; full suite 396/396; Debug/Release builds 0 warnings/errors. A genuine embedded Microsoft signature passes, tampering fails closed, current unsigned App/worker are production-blocked, source/release/dependency/process/temp audits pass. Fresh 680x430 trust-result render was inspected and all text/actions fit without overlap.
- Blockers: None.
- Exact next action: complete manual UAC Accept/Cancel smoke evidence, then add a real one-shot worker mode whose entry requires a fresh production trust authorization plus the existing authenticated request. Initially compose only the official uninstaller and mandatory read-only post-scan; keep residue handling and production WPF wiring disabled until fake/integration tests prove every result state.

## Active Slice - 2026-07-12 Beginner worker result and build packaging

- Objective: translate the verified worker lifecycle into a path-free beginner result panel, resolve the packaged worker only from the App directory, and make App build/publish output contain the fake-only Elevated worker without creating a compile/runtime assembly dependency from App to Elevated.
- Dependencies: lifecycle typed statuses, WPF AutomationIds/render evidence, exact sibling path resolution, MSBuild build/publish targets, `Css.App.deps.json` dependency inspection, and a DEBUG-only real-`runas` smoke entry.
- Risks: showing technical protocol terms, leaking local paths, presenting the fake response as an uninstall, accidentally adding an App-to-Elevated assembly reference, copying stale/wrong-configuration binaries, compiling the smoke route into Release, or surprising the user with a UAC prompt.
- Impact scope: App presenter/view/window/path resolver, App build packaging target, DEBUG smoke composition, product/WPF/packaging tests, screenshot evidence, and protocol records. No production uninstall button wiring and no real handler/launcher/scanner registration.
- Acceptance: every lifecycle status maps to short plain Chinese plus Agent advice and an explicit no-change safety statement; the first visible panel has stable AutomationIds and an inspected render; only exact sibling `Css.Elevated.exe` is resolved; Debug/Release App outputs contain exe/dll/deps/runtimeconfig; App deps and compile references exclude Elevated; DEBUG smoke route is absent from Release; no real uninstall authority becomes reachable.
- Status: Completed at the automated/visual boundary. All lifecycle states now map to a compact path-free Agent result; exact non-reparse sibling resolution is implemented; App build and publish package the four fake-only worker files through a build target while retaining no Elevated project/deps dependency; a DEBUG-only real-`runas` smoke and manual Accept/Cancel script are available.
- Last verification: presentation 15/15; impacted product/boundary 188/188; Release presentation+lifecycle 22/22; full suite 384/384; Debug/Release solution builds 0 warnings/errors. Release publish contains exe/dll/deps/runtimeconfig, App deps excludes Elevated, Release App excludes the DEBUG smoke string, source authority audit and process audit pass. Fresh 680x430 WPF render was inspected with all conclusion/advice/safety/actions visible and no overlap.
- Blockers: None.
- Exact next action: run `.omx/gui-uninstall-worker-lifecycle-smoke.ps1` once with `Cancel` and once with `Accept`, making the requested choice manually on the secure desktop; inspect both screenshots and process cleanup. After that, require a signed-worker trust policy and compose the real handler behind the same one-shot boundary without yet enabling automatic residue handling.

## Active Slice - 2026-07-12 Production fake elevated worker lifecycle

- Objective: add an injected App-side lifecycle client and a one-shot `Css.Elevated` worker mode that can exchange exactly one authenticated typed request with a fake endpoint, while keeping all real uninstall authority unreachable.
- Dependencies: the prepared one-time request, exact current-user SID/PID/session correlation, current-user named pipe, ephemeral ECDH bootstrap, authenticated request/response codec, an injected launch result/process handle, and bounded startup/response/shutdown deadlines.
- Risks: treating UAC cancellation as an error or success, trusting launch metadata instead of the connected pipe peer, leaking key material through arguments/environment/files, leaving an elevated child orphaned, presenting a fake response as a real uninstall, or accidentally registering `OfficialUninstallOperationHandler`, launcher, scanner, pipeline, registry, service, task, or file mutation authority.
- Impact scope: neutral lifecycle contracts/client in `Css.Ipc`, an App-owned Windows process-launch adapter, a fake-only one-shot mode in `Css.Elevated`, focused process/protocol/static tests, and protocol records. The production WPF uninstall button remains unconnected to the fake mode in this slice.
- Acceptance: launch cancellation is a first-class non-success result; the connected worker must match the exact launched PID/current SID/Windows session before bootstrap; bootstrap failure and response timeout are distinct; every failure disposes or terminates the entire child tree within a bound; success returns one correlated path-free fake response and observes clean exit; arguments contain metadata only; static audits prove no real handler/launcher/scanner/pipeline/mutation registration.
- Status: Completed at the automated boundary. App owns an injectable `runas` launcher; neutral Ipc owns exact-child lifecycle/identity/bootstrap/exchange/cleanup; Elevated exposes one fake-only one-shot mode whose typed response explicitly says no uninstaller started. The production WPF route remains disconnected.
- Last verification: lifecycle 7/7 in Debug and Release; related official-uninstall 101/101; full suite 369/369; Debug/Release solution builds 0 warnings/errors. Static audits found no real handler/launcher/scanner/pipeline/mutation registration, no secret argument/environment/file channel, and no production WPF registration. Process audit was empty.
- Blockers: None.
- Exact next action: add a path-free beginner lifecycle presenter and a test-only App orchestration smoke that uses the real `runas` adapter, then manually verify actual Windows UAC cancel/accept behavior and worker-path packaging before any real handler registration or user-facing execution button is connected.

## Active Slice - 2026-07-12 Production final-consent request entry

- Objective: connect the real application uninstall plan to the final-consent window and produce a one-time auditable request draft from fresh snapshot/recovery evidence, without launching a worker or uninstaller.
- Dependencies: ready `UninstallFinalConfirmationDraft`, final-consent presenter/window/visual ticket, execution gate revalidation, one-time request session, and plan-page status controls.
- Risks: treating an unclosed app as confirmed, reusing stale draft evidence, consuming a visual ticket without a matching gate, accidentally exposing execution, or presenting a prepared request as a completed uninstall.
- Impact scope: Core final-consent contract, plan-window XAML/code-behind, product/WPF tests, screenshot/status contracts, and records. No process launch, Elevated Program change, handler/adapter registration, or system mutation.
- Acceptance: only a ready verified draft exposes Continue; first acknowledgement explicitly covers closing app/tray plus official command; cancel/refusal stays in plan; acceptance re-evaluates command/existence/snapshot/hash/recovery and consumes one visual ticket; a ready request is held in memory and clearly labeled not executed; no execution APIs are reachable.
- Status: Completed and verified. A ready checklist exposes a production Continue action; the first acknowledgement explicitly covers closing the app/tray and confirming the official command. Acceptance revalidates signed recovery source, uninstaller existence/trust, actual snapshot hash, exact consent, and one-time visual ticket, then holds a typed request in memory with an explicit not-executed status.
- Last verification: request-preparation behavior 4/4; plan-entry WPF 2/2; uninstall-related 121/121; full suite 362/362; Release high-risk set 12/12; Debug/Release builds 0 warnings/errors. Release contains production Continue/service but no DEBUG smoke argument. Static audits found no process/handler/pipeline/mutation authority; Elevated Program remains placeholder; no process remained.
- Blockers: None.
- Exact next action: add an injected App-side worker lifecycle client and a one-shot `Css.Elevated` fake-handler mode, preserving exact PID/session/SID bootstrap, UAC cancel truth, bounded shutdown, and no real handler/launcher/scanner registration.

## Active Slice - 2026-07-12 Reproducible WPF render evidence

- Objective: close the pending visual gate by exporting the actual final-consent WPF render as a test-only artifact, inspect it, and keep production capture in-memory only.
- Dependencies: existing STA modal-window test, real `RenderTargetBitmap` PNG bytes, explicit test environment variable, workspace `.omx` evidence root, and local image inspection.
- Risks: accidentally adding file persistence to product code, writing outside the evidence root, saving after the production buffer is zeroed, or treating a render artifact as proof of UIAutomation behavior it does not cover.
- Impact scope: test-only optional artifact export, static contract, screenshot inspection, and records. No App runtime persistence, execution route, worker registration, or system mutation.
- Acceptance: default tests write nothing; explicit output is restricted to `.omx`, contains the same nonblank PNG/hash used by the receipt, is visually readable with all safety content visible, and product capture remains free of file APIs.
- Status: Completed and visually inspected. Test-only export is restricted to repository `.omx`; product capture remains in-memory. Inspection caught and fixed root-margin cropping/transparent background, then caught and fixed first-render black blocks with a Render-priority flush plus origin-normalized `VisualBrush`.
- Last verification: fresh `.omx/qa-runtime-final-consent-render.png` is 171,609 bytes and was inspected at original detail: title, software, three impact lines, all three checked acknowledgements, readiness, safety text, cancel, and continue are complete with no overlap or crop. Focused render/product tests passed 2/2.
- Blockers: None.
- Exact next action: keep the artifact as the current visual receipt evidence and regenerate after any final-consent layout/copy change.

## Active Slice - 2026-07-12 Runtime final-consent visual receipt

- Objective: replace the DEBUG final-consent fixture hash with an App-captured PNG receipt bound to the actual visible confirmation state, while keeping every execution path fake and unregistered.
- Dependencies: WPF `RenderTargetBitmap`, final-consent AutomationIds/viewport state, pure in-memory receipt issuer/request session, exact consent, and the existing fake pipe response flow.
- Risks: signing a blank/offscreen render, trusting booleans unrelated to the captured window, retaining screenshot bytes, closing after a refused receipt, introducing an App-to-Elevated reference, or accidentally making the confirmation action execute.
- Impact scope: move pure receipt/session types from Elevated to Core, add App-only WPF capture, wire DEBUG final consent to issue/consume a real one-time receipt, update tests/smoke evidence/records. No production route, worker launch, handler registration, real adapter, or system mutation.
- Acceptance: capture is a bounded valid PNG rendered from the shown consent content; recovery truth, all three confirmations, readiness/safety text, collapsed/absent technical details, and absent run control are checked against actual WPF elements; issue failure keeps the window open; successful DEBUG flow consumes the ticket once and still returns the typed fake result; App has no Elevated reference; GUI screenshot and full verification pass.
- Status: Completed and verified. Pure receipt/session types live in Core. The shown modal window renders its actual content to a nonblank PNG, checks visible safety elements, issues a one-time ticket, zeros PNG bytes, and the DEBUG fake pipe consumes that ticket before composing a request.
- Last verification: WPF runtime capture 2/2; related receipt/consent/boundary/product tests 25/25 at implementation; later full suite 362/362 and Debug/Release 0 warnings/errors. Fresh receipt bytes were exported test-only and inspected after fixing crop/transparency and first-render composition; all required content/actions are visible.
- Blockers: None.
- Exact next action: covered by the newer production final-consent request-entry slice; preserve the receipt artifact regeneration test after future UI changes.

## Active Slice - 2026-07-12 Separate-process authenticated smoke worker

- Objective: prove the identity-bound bootstrap and one authenticated typed fake uninstall request/response across a disposable, separate, non-elevated child process before changing the elevated worker entry point.
- Dependencies: `Css.SmokeTools`, neutral `Css.Ipc`, current-user named pipes, OS-derived SID/PID/session identity, ephemeral session bootstrap, authenticated request/response codec, and injected process launch/lifecycle orchestration in tests.
- Risks: secret material leaking through arguments/environment/files, trusting caller-claimed child identity, startup/response/shutdown hangs, process orphaning, multiple-request listeners, or accidentally registering real launcher/handler/scanner authority.
- Impact scope: smoke-only executable mode, test-side launch adapter/orchestration, focused integration/static tests, and records. `Css.Elevated/Program.cs`, WPF production routes, real adapters, and system state remain unchanged.
- Acceptance: the parent launches a separate non-elevated worker with non-secret metadata only, verifies the exact child PID/session/SID from the connected pipe, completes ephemeral bootstrap, receives one correlated path-free typed fake response, observes bounded clean child exit, and proves no orphan. Timeout/cancellation/failure cleanup and static no-authority/no-secret-channel checks pass.
- Status: Completed and verified. `Css.SmokeTools` now hosts one strict non-elevated worker session, accepts only bounded non-secret launch metadata, validates the real client identity, bootstraps an ephemeral key, handles one authenticated fake request, emits a path-free receipt, and exits. Test-side launch is injected and kills the entire child tree on failure or disposal.
- Last verification: focused worker tests 4/4 in Debug and Release; related transport/bootstrap tests 33/33; full suite 352/352; Debug and Release solution builds 0 warnings/errors. Static audits found no secret argument/environment/file channel, real handler/launcher/pipeline/mutation authority, App Release worker strings, or Elevated Program registration. No Css/OMNIX process remained.
- Blockers: None.
- Exact next action: implement App-side runtime visual receipt capture for the real final-confirmation window, binding screenshot bytes and visible safety state to the exact request without exposing execution. Keep production worker launch and all real adapters unregistered until that receipt is proven by GUI automation and screenshot inspection.

## Active Slice - 2026-07-12 Identity-bound ephemeral IPC session bootstrap

- Objective: establish a fresh authenticated session key between the future App and elevated worker without placing secrets in command-line arguments, environment variables, or files.
- Dependencies: current-user named pipes, OS-derived SID/PID/session correlation, ECDH P-256, SHA-256/HMAC, bounded framing, strict JSON, replay tracking, and disposable key ownership.
- Risks: unauthenticated ECDH man-in-the-middle, transcript ambiguity, nonce/public-key replay, unbounded handshake reads, key copies surviving disposal, identity mismatch deadlock, or accidental worker/handler registration.
- Impact scope: neutral `Css.Ipc` cryptographic bootstrap and tests only. No WPF change, Elevated Program change, process launch, handler registration, or system mutation.
- Acceptance: both sides derive the same fresh 256-bit key only when protocol/session/pipe/SID/PID/session/public keys/nonces match; server and client finished MACs are verified; malformed/oversized/tampered/replayed/mismatched/timeout/cancel cases fail; owned key bytes are zeroed on dispose; no secret-transfer side channel or execution authority is added.
- Status: Completed and verified. Client/server exchange strict bounded hello frames with fresh P-256 ECDH keys and 32-byte nonces, derive a transcript-bound 256-bit key through HMAC-SHA256 extract/expand, and mutually verify server/client finished MACs. Replay guard, timeouts, cancellation, and disposable/finalizable key zeroization are implemented.
- Verification: bootstrap tests 7/7; bootstrap plus authenticated-key lifecycle tests 15/15; full suite 348/348; Debug and Release builds 0 warnings/errors. Live named-pipe agreement, identity mismatch, finished-message tamper, client-nonce replay, malformed/oversized/schema errors, timeout/cancellation, and post-disposal refusal are covered. Static audits found no command-line/environment/file secret channel, execution authority, Program registration, Release App bootstrap strings, or running process.
- Remaining boundary: bootstrap is not yet composed with the authenticated request transport in a separate process. No worker launch protocol, child PID lifecycle, runtime screenshot receipt, production consent route, handler/launcher/scanner registration, or real uninstaller call exists.
- Exact next action: add a test-only separate worker host in `Css.SmokeTools` that performs only identity-bound bootstrap plus typed fake request/response, launched through an injected non-elevated test process adapter. Correlate exact child PID/session, enforce startup/response/shutdown deadlines, prove no orphan, and keep Elevated Program unchanged.

## Active Slice - 2026-07-12 Neutral IPC library and DEBUG WPF pipe flow

- Objective: let the non-elevated WPF app use the verified fake named-pipe protocol without referencing or gaining access to the elevated executable project.
- Dependencies: pure final-consent/request/result contracts, strict IPC codec, authenticated fake endpoint, Windows peer identity reader, final-consent WPF fixture, and post-scan result window.
- Risks: circular project references, moving executable authority into a shared assembly, duplicating descriptor hashing, compiling fake smoke entry points into Release, UI deadlock around named-pipe tasks, or accidentally registering the real handler/launcher/scanner.
- Impact scope: Core pure contracts/presenters, a new neutral `Css.Ipc` class library, App/Elevated project references, DEBUG-only final-consent smoke flow, tests, GUI smoke evidence, and records.
- Acceptance: App references Core/Ipc but not Elevated; Ipc contains no handler/launcher/pipeline/mutation authority; DEBUG consent creates a correlation-bound request, crosses a real current-user pipe to an injected fake endpoint, and displays the typed path-free result; Release excludes the smoke route; all tests/builds/audits pass.
- Status: Completed and verified. Pure request/result/presentation contracts now live in Core; authenticated transport, strict codec, framing, peer identity, client, and fake server live in neutral `Css.Ipc`. DEBUG final consent now crosses a real current-user named pipe to an authenticated fake endpoint and displays the typed result rather than a fixed fallback.
- Verification: related contract/transport tests 50/50; final full suite 340/340; Debug and Release builds 0 warnings/errors. GUI smoke proved disabled-to-enabled consent, `pipeResultFactCount=2`, visible Agent/safety text, clean screenshots, close/exit, and no real execution control. Release App binary has no smoke route/pipe fixture strings; App references Ipc but not Elevated; Ipc references Core only; Program remains placeholder; no App/Elevated process remained.
- Remaining boundary: the fake server runs in the App process and the DEBUG visual receipt is a fixture. There is no separate elevated worker lifecycle, secure cross-process session-key establishment, runtime screenshot capture, production consent reachability, handler/launcher/scanner registration, or real uninstaller call.
- Exact next action: implement an unregistered identity-bound ephemeral session bootstrap over the named pipe (fresh nonces plus ephemeral ECDH/HKDF-style derivation), with transcript/PID/session binding, key zeroization, timeout/cancellation/tamper tests, and no command-line/environment secret transfer. Keep Elevated Program and real adapters unchanged.

## Active Slice - 2026-07-12 Bounded serialized fake pipe transport

- Objective: add a bounded, serialized named-pipe-shaped request/response boundary with explicit Windows user and process correlation before any real elevated runtime registration.
- Dependencies: authenticated uninstall messages, correlation/hash-bound elevated requests, final-consent contracts, fake handler integration, JSON serialization, and injectable peer identity/time abstractions.
- Risks: serializing executable objects or secrets, trusting caller-claimed SID/PID, unbounded frames, handler invocation after malformed input, replay across sessions, timeout/cancellation races, accidental `Css.App` to `Css.Elevated` coupling, or registering the real launcher/handler in `Program`.
- Impact scope: pure IPC contracts/codecs, fake pipe client/server adapters, focused tests, static registration audits, and development records. No production WPF wiring or real uninstaller execution in this slice.
- Acceptance: protocol/schema/version and bounded payload checks pass; wrong SID/PID/session, malformed JSON, tamper, replay, timeout, and cancellation are refused before the endpoint runs; response correlation is checked; full tests and Debug/Release builds pass; App/Program remain free of real uninstall runtime registration.
- Status: Completed and verified. The 64 KiB length-prefixed JSON protocol preserves strict string/bool operation types, signs correlated path-free responses, and rejects malformed/oversized/tampered/replayed traffic. A real local named-pipe test derives the current SID, PID, and Windows session from the connection; fake identities prove each mismatch is refused before the endpoint.
- Verification: focused serialized-pipe tests 14/14; full suite 340/340; Debug and Release solution builds 0 warnings/errors. Static audits found no App-to-Elevated reference, Program registration, process launch, mutation API, safety-pipeline call, release App smoke string, running App/Elevated process, or raw path in serialized responses.
- Remaining boundary: the codec/client currently live in `Css.Elevated`, so WPF cannot consume them without violating the project boundary. The server is test-only and accepts only an injected authenticated fake endpoint; no elevated worker startup, key exchange, runtime screenshot capture, production consent wiring, handler registration, or real uninstaller call exists.
- Exact next action: extract the client-safe wire contract and client into a neutral IPC library referenced by App and Elevated, while keeping the endpoint/server and every real execution adapter in Elevated. Then connect the DEBUG final-consent fixture to a fake pipe and render the typed result window.

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

## Active Slice - 2026-07-11 Elevated boundary and recovery GUI gate

- Status: Completed and verified; real execution remains unregistered.
- Request boundary: A ready draft now requires a fresh screenshot-backed UI receipt, exact final confirmation text, all safety acknowledgements, a manual high-risk gate descriptor, a correlation id, and an immutable SHA-256-bound descriptor copy. Missing/stale/mismatched evidence is refused.
- Response boundary: Typed elevated payloads must match the request id; launch failure, uninstall failure, invalid response, and post-scan presentation stay distinct. Beginner-visible text never copies raw handler errors, paths, or identifiers.
- Recovery reliability: The read-only Windows restore-point query now has a four-second WMI/outer timeout and returns Completed/TimedOut/Failed. Timeout is explained as unknown, never as “no restore point,” and the plan window still opens.
- GUI reliability: Shared WPF smoke helpers now fall back to Win32 `EnumWindows` plus `AutomationElement.FromHandle` for owned modal windows that are visible but absent from the UIAutomation root tree.
- Verification: TDD RED/GREEN completed; boundary tests 7/7; related official-uninstall tests 38/38; final full suite 298/298; solution build 0 warnings/errors. GUI smoke passed under the original 10-second gate with three protection lines, three simple steps, collapsed technical details, no execution control, successful close, and inspected screenshot `.omx/qa-uninstall-plan-window.png`.
- Safety state: No handler, launcher, scanner, or request composer is registered in App/Elevated Program; no real uninstaller or installer ran; process/temp checks are empty.
- Exact next action: Add a WPF final-confirmation checklist generated from completed recovery preparation and the existing non-executable draft service. It must show snapshot/reinstall/backup truth, require explicit confirmations, expose no run button, and receive its own AutomationIds/static order test/real screenshot before any execution registration.

## Latest Update - 2026-07-13 Source-only migration execution closure (authoritative tail)

- Objective: finish the source path from real migration snapshot evidence through an authenticated production coordinator and beginner final confirmation while local antivirus definitions are still pending.
- Status: source implementation and static audits completed for this slice; compilation, tests, Worker/UAC launch, and real path mutation were intentionally not performed.
- What changed: snapshot evidence is atomic, bounded, strict-JSON, hash verified, manifest bound, and now re-observed immediately before mutation so changed source size/time/path/type refuses execution. Elevated validation additionally requires manual source and elevation. App has a trust-gated coordinator that accepts completion only for a correlated successful `Completed` payload. A four-acknowledgement final consent window and plan-page coordinator call were added with stable AutomationIds; current MainWindow readiness keeps `FeatureEnabled=false`, so the request button remains disabled.
- Static evidence: all three migration XAML files parse as XML; WPF authority scan found no worker mode, production launcher, lifecycle call, process start, or file/directory move in plan/consent/result windows; snapshot id/path/hash are correlated in MainWindow source; no mojibake marker was found in migration UI source.
- Blockers: corrected Huorong definitions are not installed locally. All new C# and tests are uncompiled and cannot be treated as verified. Final consent needs a real first-view screenshot after compilation resumes.
- Exact next action: after Huorong definitions update, perform one narrow clean test-project build and scan the generated assembly. If clean, compile and run only migration snapshot/coordinator/consent tests, then capture the final-consent window using fixture data. Keep production migration disabled and do not touch real C/D software paths.

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
