# Quality Gates

## 2026-07-23 - Personal signer and first D-first installer

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | Separate user approvals covered CurrentUser TrustedPeople/TrustedPublisher and exact-thumbprint CurrentUser Root; installer execution and upload were not authorized or performed. | Trust persists until manually removed. |
| Certificate security | Pass | Independent store query: private key only in CurrentUser My; public copies in authorized CurrentUser stores; zero matching entries in inspected LocalMachine stores; no PFX/private-key export. | Any future binary signed by the same private key is trusted by this CurrentUser. |
| Signed payload | Pass | `verify-signed-release-candidate.ps1`: 110 files, valid RSA same signer, trusted timestamp, `CanBeginDisposableAcceptance=true`. | Ten-case disposable-machine behavioral acceptance is pending. |
| Installer | Pass | `verify-personal-installer.ps1`: version 0.1.0, same signer, D-first default, visible directory selection, silent install refused, `CanStageGitHubRelease=true`; SHA-256 `5680C3847F23291784BB38FB1D01FACAFC6013DC47F06B611C170BCDC63955BE`. | Setup has not been launched, installed, or uninstalled. |
| Build and tests | Pass | Focused 10/10; full 1054/1054; Release build 0 errors; integrity 380 files and 18/18 XAML; three scripts parse; GitHub CI `30019958502` passed every step in 2m52s. | Local NU1900 warnings remain because the sandbox could not reach NuGet vulnerability metadata. |
| Supply chain and privacy | Pass | Inno 6.7.3 compiler signature Valid; pinned official translation tag/hash/notice; 457 public candidates/about 6.3 MB; zero binary/signing-material candidates; CER/setup ignored under `.artifacts`. | Public certificate thumbprint remains intentionally auditable in records. |
| Release | Warn | Valid local installer exists and can be staged. | No GitHub draft/public Release or disposable acceptance receipt exists; separate approval required. |

### 2026-07-23 - D-first personal installer foundation

- Pass - Install UX policy: visible directory page, D-first default, Chinese language, optional shortcuts, and silent setup refusal are explicit in `installer/OMNIX-Entropy.iss` and contract tests.
- Pass - Trust boundary: builder verifies the signed payload first and requires one explicit signer for App/worker/setup/uninstaller; release staging independently verifies setup and copied hash. Evidence: builder/verifier/release scripts and 19/19 focused tests.
- Pass - Read-only transfer verification: fixed local path, reparse/extra-file, length/hash, manifest, Authenticode, timestamp, and signer checks have no write/launch authority. Evidence: `verify-personal-installer.ps1` source contract.
- Pass - Regression/integrity: full 1051/1051; Release build 0 errors; 379 strict UTF-8 source files, replacement 0, and 18/18 XAML valid.
- Warn - Build warnings: local .NET commands emitted NU1900 because the restricted environment could not reach the NuGet vulnerability index; no package restore/build/test failed.
- Warn - Real artifact: SignTool is present, but Inno compiler and eligible code-signing certificate are absent. No setup, signature evidence, install/uninstall run, or GitHub Release exists.
- Pass - Side effects: no installer/certificate generation or installation, trust change, UAC, setup launch, system file write, antivirus interaction, or GitHub release publication occurred.

### 2026-07-22 - First public CI remediation

- Pass - Scope/privacy: 22 top-level smoke/helper scripts contain neutral fixture data only; no credentials, real username, machine Marvis path, binary, or signing material is included. Evidence: `rg` privacy/secret scan and public candidate list.
- Pass - Local behavior: focused repository contracts 4/4; full Debug suite 1048/1048; Release build succeeded. Evidence: local `dotnet build`/`dotnet test` on 2026-07-22.
- Pass - Source integrity: 378 files strict UTF-8, replacement files 0, 18/18 XAML valid. Evidence: `.omx/verify-source-integrity.ps1`.
- Pass - Remote reproducibility: tracked-only archive passed Release/full/integrity; commit `06534d4` is pushed; replacement run `29933681994` passed Release build, 1048/1048 tests, and integrity 378 files/18 XAML with zero errors.
- N/A - Product runtime/destructive authority: no product behavior, installer, signing policy, system setting, file cleanup, migration, uninstall, or privileged execution changed.

### 2026-07-22 - GitHub personal release foundation

- Pass - Scope/trust: fixed public repository; CI contents read only; actions pinned to commits; local publisher creates drafts only. Evidence: `.github/workflows/ci.yml`, `scripts/prepare-personal-github-release.ps1`.
- Pass - Repository hygiene: 424 candidate files/about 6 MB; no binary, QA screenshot, database, certificate/private-key, token-pattern, or real-username candidate. Evidence: `git ls-files --cached --others --exclude-standard`, `.gitignore`, secret/privacy scans.
- Pass - Functional tests: focused release/update 13/13 and full Debug 1048/1048. Evidence: `dotnet test` results from 2026-07-22.
- Pass - Production build: Release build 0 warnings/errors; fake worker remains excluded. Evidence: `dotnet build ComputerSecuritySoftware.slnx --configuration Release --no-restore` and release command-surface contracts.
- Pass - Source integrity: 377 files strict UTF-8, replacement files 0, 18/18 XAML valid. Evidence: `.omx/verify-source-integrity.ps1`.
- Warn - Visual acceptance: stable AutomationIds and valid XAML exist, but Computer Use timed out launching Debug and no OMNIX window was returned; no screenshot is claimed.
- Warn - Install/update completion: metadata checking and draft release transport exist, but package download, D-first installer, same-signer installer verification, replacement, and rollback are deferred.
- Pass - Security side effects: no certificate/trust change, release publication, download, installation, UAC, antivirus interaction, or trust weakening occurred.

### 2026-07-22 - Non-default Windows SDK SignTool discovery

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Reads only two exact HKLM Windows Kits roots and existing files; retains CurrentUser certificate read and explicit selection. | No secret, certificate content beyond existing summary, or personal path added. |
| Data, API, and consistency | Pass | Registered root, resolved file, filename validation, resolution source, missing-requirement list, and JSON report agree. | Current report removes only the disproven SignTool requirement. |
| Destructive-operation safety | Pass | No recurse, drive scan, registry write, SDK install/change, process launch, signing, certificate import, or trust mutation. | Explicit invalid paths still fail without fallback. |
| Frontend, accessibility, and UX | N/A | Release prerequisite script and documentation only. | Product UI unchanged. |
| Testing and verification | Pass | TDD red; inspector 4/4; related guide/audit 8/8; full 1035/1035; Release 0 warnings/errors; integrity 370/17; parser and non-ASCII counts 0. | Current-machine JSON proves D-drive resolution. |
| Operations, dependencies, and release | Warn | SignTool now resolves as `WindowsKitsRegistry`; zero eligible RSA signers remain. | Signed candidate and disposable receipt still cannot be created. |

### 2026-07-22 - V1 completion audit and recent-install sort

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Install date reads only the existing uninstall record value; invalid/absent metadata remains null; Agent authority audit reports 0 direct delete/process/registry-write hits. | No personal path or raw registry locator was added to ordinary UI. |
| Data, API, and consistency | Pass | `InstalledSoftwareRecord` -> `SoftwareProfile` -> growth enrichment preserves `DateOnly?`; `RecentInstall` orders known newest-first and unknown-last; technical details show only known dates. | No timestamp inference or fallback fabrication. |
| Destructive-operation safety | Pass | This slice adds read-only metadata, presentation, tests, and documentation only. | No installer, cleanup, uninstall, migration, startup, restore, signing, or trust mutation ran. |
| Frontend, accessibility, and UX | Warn | XAML has stable `AppSortComboBox` AutomationId and `按最近安装`; 17/17 XAML parses. Computer Use launch timed out and no OMNIX window/process remained. | No current-version screenshot is claimed. |
| Testing and verification | Pass | Related 209/209; critical workflow group 337/337; focused audit group 205/205; full 1035/1035; Release 0 warnings/errors; integrity 370 files and 17/17 XAML. | Audit contract preserves all original named feature groups and external gates. |
| Operations, dependencies, and release | Warn | Completion audit shows local chains connected, but signed disposable behavior evidence remains absent. | No SignTool, eligible RSA signer, or checkpointed disposable operator run is available. |

### 2026-07-22 - Signing prerequisites and operator guidance

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Inspector/signing/verifier require RSA, code-signing EKU, validity, private key where needed, explicit thumbprint, timestamp, and same signer; guide rejects secret handling and trust bypasses. | No certificate, private key, PIN, password, or trust store was changed. |
| Data, API, and consistency | Pass | Inspector output, signing manifest `CertificatePublicKeyAlgorithm`, and transfer verifier share the RSA contract. | The release index links the exact supported sequence. |
| Destructive-operation safety | Pass | Documentation and inspector are read-only; no SDK install, certificate import/generation, signing, product launch, UAC, or system mutation ran. | Positive behavior remains confined to a disposable environment. |
| Frontend, accessibility, and UX | N/A | Release scripts and Chinese operator documentation only. | Product UI did not change. |
| Testing and verification | Pass | Guide/index plus related signing contracts 15/15; full 1033/1033; Release 0 warnings/errors; integrity 369 files and 17/17 XAML. | All locally runnable gates for this slice passed. |
| Operations, dependencies, and release | Warn | Read-only inspector at `2026-07-22T12:45:12Z` reports store readable but no SignTool and zero eligible RSA code-signing certificates. | Real candidate and ten-case receipt require user/vendor-controlled prerequisites. |

### 2026-07-19 - Disposable Windows behavioral acceptance protocol

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Session initialization requires non-primary/disposable attestation and fixture-only protocol; receipt verification reads local package/session evidence only. | No product, UAC, certificate, registry, service, task, or mutation action ran. |
| Data, API, and consistency | Pass | Candidate manifest hash/signer, session manifest hash, exact case set, timestamps, reset state, unique evidence paths, lengths, and SHA-256 are cross-checked. | Candidate package and session receipt remain separate and immutable during verification. |
| Destructive-operation safety | Pass | Initializer has no product launch/UAC automation; verifier has no write or mutation authority; unsigned refusal created no session directory. | Manual positive cases are restricted to disposable fixtures. |
| Frontend, accessibility, and UX | N/A | Release QA tooling and operator documentation only. | Real application UI is exercised later by the manual protocol. |
| Testing and verification | Pass | TDD red 0/6; focused 6/6; related 20/20; full 1000/1000; Release 0 warnings/errors; 361 strict UTF-8 source files; 17/17 XAML; parsers valid; scripts ASCII-only. | Positive receipt verification awaits real evidence by design. |
| Operations, dependencies, and release | Warn | Protocol is ready and current unsigned package refuses session creation before output. | Real code-signing material, signed candidate, checkpointed disposable Windows, and ten-case behavioral evidence are still required. |


### 2026-07-19 - Release-candidate transfer verification

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Verifier reads local package metadata/signatures only and contains no process, network, certificate-store mutation, registry, service, or file-write authority. | No personal certificate inventory was enumerated. |
| Data, API, and consistency | Pass | Manifest state, critical coverage, listed and actual file sets, length/hash, signer/timestamp, manifest thumbprints, and worker command surface are independently correlated. | Unlisted payloads and duplicate/unsafe paths fail. |
| Destructive-operation safety | Pass | Success says only `CanBeginDisposableAcceptance`; completed acceptance remains false. | Current unsigned package was refused before any launch. |
| Frontend, accessibility, and UX | N/A | Command-line release preflight only. | Behavioral UI evidence remains the next gate. |
| Testing and verification | Pass | TDD red 0/4; focused 4/4; related 14/14; full 994/994; Release 0 warnings/errors; 360 strict UTF-8 files; 17/17 XAML; parser valid; unsigned refusal observed. | Positive candidate verification awaits a real signed package. |
| Operations, dependencies, and release | Warn | Verifier is ready for the disposable machine and does not require the signing certificate/private key there. | No signed candidate or behavioral acceptance receipt exists yet. |

### 2026-07-19 - Trusted signed-release transformation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Script accepts thumbprint only, uses `Cert:\CurrentUser\My`, and contains no PFX password, import, certificate generation, trusted-store modification, or trust relaxation. | Certificate store was not enumerated or changed during verification. |
| Data, API, and consistency | Pass | Every source manifest entry is length/hash verified; critical files must be covered; signing occurs on copies; output hashes and manifest are generated after post-signature verification. | Source artifact remains immutable. |
| Destructive-operation safety | Pass | Output must be new; script contains no remove/move/overwrite force path; same-signer output remains awaiting disposable-machine acceptance. | No product mutation or actual signing ran. |
| Frontend, accessibility, and UX | N/A | Release tooling only; candidate readme explains signing and disposable-test boundaries in Chinese. | No WPF surface changed. |
| Testing and verification | Pass | TDD red 0/4; focused 4/4; related 10/10; full 990/990; Release 0 warnings/errors; 359 strict UTF-8 files; 17/17 XAML; parser valid; missing-sign-tool refusal created no output. | Positive signing requires external certificate/tooling and was not simulated. |
| Operations, dependencies, and release | Warn | Script requires explicit Windows SDK `signtool.exe`, existing CurrentUser code-signing certificate/private key, and HTTPS RFC3161 endpoint. | No signed candidate exists yet; disposable-machine real mutation acceptance remains pending. |

### 2026-07-19 - Execution result return handoff

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Changed result/plan code adds no path, command, registry, service, task, or private-data display. | Rescan remains the existing local read-only inventory path. |
| Data, API, and consistency | Pass | Plan closes only after `ProductionExecutionAttempted`; MainWindow already re-reads software inventory and migration closure/residue state from that flag. | Preview-only closure does not enter the rescan branch. |
| Destructive-operation safety | Pass | No signer, consent, request, worker, handler, rollback, quarantine, or operation-pipeline code changed. | Result acknowledgement never retries an operation. |
| Frontend, accessibility, and UX | Pass | Existing stable result button AutomationIds remain; nested flows display `返回并重新检查`, while standalone Debug hosting retains truthful generic close copy. | Positive runtime result is intentionally not simulated in an unsigned package. |
| Testing and verification | Pass | TDD red 0/3; focused 3/3; related 207/207; full 986/986; Release 0 warnings/errors; 358 strict UTF-8 files; 17/17 XAML. | Static contracts verify both terminal return paths and context-specific copy. |
| Operations, dependencies, and release | Warn | Final package/ZIP `.artifacts/OMNIX-Entropy-test-20260719-003423`; ProductionOnly command surface. | App/worker remain `NotSigned`; positive execution/result/rescan acceptance requires valid same-signer packaging and a disposable fixture. |

### 2026-07-19 - Cache and startup decision outcomes

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Process-scoped one-app fixture plus workspace-local data/quarantine roots; no raw path or registry locator appeared in the decision panels. | The Windows Settings handoff was not invoked. |
| Data, API, and consistency | Pass | Cache used current candidate validation; startup used current local-entry preparation and returned the name-only fallback state. | No fixture was treated as stronger evidence than it supplied. |
| Destructive-operation safety | Pass | Cache refusal exposed no primary action. Startup exposed only `在 Windows 中查看` and stated that OMNIX would not toggle settings or modify registry/services/tasks. | No cleanup confirmation, startup confirmation, pipeline, UAC, or mutation ran. |
| Frontend, accessibility, and UX | Pass | Computer Use captured stable Agent summary, next-step, safety, and list AutomationIds for both outcomes in the real Release window. | Both conclusions were visible inside the application drawer and remained path-free. |
| Testing and verification | Pass | Runtime acceptance reused package `.artifacts/OMNIX-Entropy-test-20260719-000736` on the inherited full 983/983, zero-warning Release, and 357-file/17-XAML integrity baseline. | No source changed, so rebuilding the identical package was not useful. |
| Operations, dependencies, and release | Warn | Antivirus definitions are updated and the package launched normally. | App/worker remain unsigned; positive cache/startup mutation requires valid same-signer packaging and a disposable fixture. |

Use this file before risky changes, handoff, delivery, or release. Apply only relevant categories and mark the rest as N/A.

### 2026-07-16 - Local mutation post-attempt state synchronization

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | Four read-only helpers report 0 pipeline/quarantine/restore/purge/process/registry/delete hits; handlers and operation policies are unchanged. | Only MainWindow post-attempt observation/presentation changed. |
| State consistency | Pass | Seven direct pipeline methods each report pipeline calls 1, attempt marks 1, catch guards 1, and synchronization calls 2. | Pre-execution refusal/cancel paths remain outside the synchronization boundary. |
| Destructive-operation safety | Pass | Catch paths observe and stop; no retry invokes a handler or pipeline. Success copy remains downstream of `result.Success`. | Unknown outcomes are never promoted to success. |
| Frontend and UX | N/A | No new control, panel, or layout was added. Existing status/inline surfaces receive refreshed evidence. | Runtime timing of a full post-cleanup scan remains a manual acceptance item. |
| Testing and verification | Pass | Focused groups 6/6, 8/8, 1/1, 11/11, and 11/11; related groups 213/213, 207/207, 243/243, 220/220, and 220/220; final full 937/937; build 0 warnings/errors; 336 strict UTF-8 files. | New contracts use the shared full-declaration balanced-method extractor. |
| Operations and release | Warn | No real C-drive cleanup, registry write, purge, or restore ran. | Positive mutations remain restricted to disposable fixtures/signed release acceptance. |

### 2026-07-16 - Persistent later install observation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | Dedicated post-scan coordinator has one software read, one footprint read, one diff build, and zero launcher/pipeline/process/registry/file mutation hits; persistent handler authority hits are also 0. | Trusted execution/consent/package inspection are unchanged. |
| State consistency | Pass | Non-refused launch gate precedes baseline retention and button visibility; reset/new prepare clears baseline, exit code, enabled state, and visibility; handler rechecks baseline object identity before presentation. | Baseline is intentionally session-only and not persisted. |
| Destructive-operation safety | Pass | Persistent action invokes only `InstallerPostScanCoordinator.CreateProduction`/`CaptureAsync` and the non-executable result flow. | It never launches an installer or treats a report as success. |
| Frontend and UX | Warn | Main XAML parses; button has one stable AutomationId/click binding, is collapsed by default, and appears before the advanced expander. | No real screenshot because the Debug-app Computer Use launch timed out earlier and passive polling found no window. |
| Testing and verification | Pass | Focused 25/25; related 243/243; full 927/927; build 0 warnings/errors; 332 strict UTF-8 files. | Falsely broad source-method slices were corrected before acceptance. |
| Operations and release | Warn | No real installer or system mutation ran. | Signed disposable-installer and visible later-rescan click-through remain release checks. |

### 2026-07-16 - Beginner-safe installer post-scan recovery

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | Retry method has one software read, one footprint read, one report build, and zero launcher/pipeline/process/registry/file mutation hits. | Package analysis, consent, launch, and installer interaction are unchanged. |
| State consistency | Pass | MainWindow retains the original `before`, invokes production launch once, invokes the read-only retry method once per explicit request, and uses the existing snapshot/report/catalog gate. | Failed reads publish no snapshot/report and remain retryable. |
| Destructive-operation safety | Pass | Retry availability is true only for interrupted-wait and failed-post-scan states; result window still rejects direct execution authority. | Exit code and observed changes never become an installation-success claim. |
| Frontend and UX | Warn | Stable retry AutomationId/click binding and strict XAML parse pass; the action is outside advanced diagnostics and paired with Close. | Computer Use launch timed out after antivirus update and passive polling found no OMNIX window, so no screenshot is claimed. |
| Testing and verification | Pass | TDD red/green; focused 23/23; related 241/241; full 925/925; build 0 warnings/errors; 332 strict UTF-8 files. | One test-only assertion/API error and one malformed patch were corrected and recorded before acceptance. |
| Operations and release | Warn | No real installer or system mutation ran. | Signed disposable-installer and visible retry click-through remain release acceptance items. |

### 2026-07-16 - Post-install inventory reuse

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | One MainWindow success-branch binding and one source contract changed; method-level process, registry, file mutation, and restore/purge hits are 0. | Installer inspection, consent, pipeline, launch, wait, and attribution are unchanged. |
| State consistency | Pass | Snapshot/report gate, catalog update, and report presentation each occur once and in that order. | The exact coordinator after-snapshot profile list is reused; no duplicate post-install scan exists. |
| Destructive-operation safety | Pass | Catalog synchronization occurs only when both trusted after evidence and report exist. | Refused, interrupted, timed-out, and failed post-scan outcomes do not update the catalog or claim success. |
| Frontend and UX | Warn | Applications now reflects the same verified state as the install report immediately. | No real installer or fresh WPF screenshot was used. |
| Testing and verification | Pass | TDD red/green; focused 16/16; related 232/232; full 918/918; build 0 warnings/errors; 332 strict UTF-8 files. | Static order check is true and mutation-authority hits are 0. |
| Operations and release | Warn | Automated fixture/source gates are green. | Signed disposable-installer acceptance remains a release item. |

### 2026-07-16 - Migration post-attempt state synchronization

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | Changed MainWindow post-window orchestration only; process, registry, file mutation, and pipeline hits are 0. | Migration coordinator/worker/rollback authority is unchanged. |
| State consistency | Pass | Attempt, inventory scan, closure refresh, and completion branches each occur once in the required order. | Unknown outcomes refresh both evidence surfaces before copy is selected. |
| Destructive-operation safety | Pass | No automatic retry or continuation exists; authenticated accepted completion remains the only success branch. | Unknown-result copy explicitly says OMNIX will not continue moving. |
| Frontend and UX | Warn | Existing result window remains authoritative; MainWindow now adds a path-free synchronization conclusion. | No signed migration or screenshot was performed. |
| Testing and verification | Pass | Focused 7/7; related 256/256; full 917/917; build 0 warnings/errors; 332 strict UTF-8 files. | Static correct-order check is true. |
| Operations and release | Warn | Fixture and source gates are green. | Signed disposable migration and positive rollback remain release acceptance items. |

### 2026-07-16 - Official uninstall post-attempt inventory refresh

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | Changed `ShowUninstallPlanAsync` only; method-level process, registry, file mutation, and pipeline hits are 0. | Existing coordinator/worker/signature/UAC authority is unchanged. |
| State consistency | Pass | One `ProductionExecutionAttempted` branch contains exactly one inventory scan; residue review has one completed-plus-recommended double gate. | Unknown outcomes synchronize read-only state only. |
| Destructive-operation safety | Pass | Residue review/quarantine still requires validated post-scan recommendation and its existing confirmation/pipeline; no new automatic cleanup exists. | UAC cancellation may cause a harmless extra read-only scan. |
| Frontend and UX | Warn | Existing truthful worker/result windows and path-free conclusion remain unchanged. | No real signed uninstall or new screenshot was used. |
| Testing and verification | Pass | Focused 6/6; related 387/387; full 916/916; build 0 warnings/errors; 332 strict UTF-8 files. | Static branch/scan/gate/call counts are each exactly one. |
| Operations and release | Warn | Automated source and fixture gates are green. | Signed disposable-package uninstall remains required before release. |

### 2026-07-16 - Startup restore pipeline

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Current timeline row, confined manifest id/hash, state fingerprint, and supported locator are bound before confirmation and rechecked before mutation; visible failure copy is fixed and path-free. | Manifest/registry details remain local and technical-only. |
| Data, API, and consistency | Pass | Focused tests cover current-row preparation, manifest tamper, stale state, locator mismatch, success, and conservative failure state. | The old public manifest-path restore API was removed. |
| Destructive-operation safety | Pass | Unconfirmed pipeline execution fails; MainWindow direct store/handler restore and direct timeline update counts are 0; only the existing exact startup store mutates the registry. | Store still refuses overwrite, ACL drift, StartupApproved drift, and invalid state. |
| Frontend, accessibility, and UX | Warn | Preparation precedes the existing confirmation; headline/confirm/cancel AutomationIds are each unique and copy remains beginner-focused. | Real WPF launch remains unavailable, so no screenshot is claimed. |
| Testing and verification | Pass | Focused 7/7; related 204/204; full 915/915; build 0 warnings/errors; 332 strict UTF-8 files with zero replacement characters. | Static pipeline/preparation/confirmation bindings are each exactly one in the startup method. |
| Operations, dependencies, and release | Warn | Tests use temporary manifests/databases and an in-memory startup store; antivirus definitions are updated. | Positive real-registry acceptance is deferred to a disposable signed fixture environment. |

### 2026-07-16 - Ordinary quarantine restore pipeline

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Confirmation is created from the current timeline row; beginner copy remains path-free; manifest/payload evidence stays in the local operation descriptor. | No cloud transfer or raw exception display was added. |
| Data, API, and consistency | Pass | `ActionTimelineStore.LoadByIdAsync`, manifest SHA-256, payload identity, affected-path matching, and same-row state revalidation are covered by focused tests. | Partial or unknown outcomes are not reported as success. |
| Destructive-operation safety | Pass | MainWindow direct quarantine restore calls 0; unconfirmed, changed payload, changed manifest, and stale timeline cases fail before movement; handler executes through `SafetyOperationPipeline`. | Restore can overwrite intent even when it is rollback, so it is classified as destructive and manual. |
| Frontend, accessibility, and UX | Warn | Existing path-free restore confirmation is retained; headline/confirm/cancel AutomationIds are each unique. | Real WPF launch remains unavailable, so no screenshot is claimed. |
| Testing and verification | Pass | Focused 6/6; related 227/227; full 908/908; build 0 warnings/errors; 330 strict UTF-8 files with zero replacement characters. | Static old-method/direct-call counts are zero. |
| Operations, dependencies, and release | Warn | Antivirus definitions are updated and automated build/test execution is normal. | Positive real-machine restore remains limited to disposable fixtures; startup restore is the next audit. |

### 2026-07-16 - Background application ownership summary

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | `ApplicationBackgroundOwnershipSummaryTests` proves exhaustive ownership, overlapping signal totals, Agent parity, and protected-only copy; focused mutation-authority search reports 0 hits. | No XAML structure, observation adapter, startup policy, plan, operation, pipeline, worker, registry, service, task, process, file, or mutation changed. |
| Behavioral verification | Pass | Focused 3/3; related application/Agent/startup/system tests 240/240; full regression 902/902. | Ordinary resident behavior and existing action guards remain green. |
| Build and encoding | Pass | Solution build reports 0 warnings/errors; 328 non-generated C#/XAML files decode as strict UTF-8 with no replacement characters. | None observed. |
| Frontend, accessibility, and UX | Warn | The existing compact `AppsSummaryTextBlock` is retained with exactly one stable AutomationId; no panel/card was added. | No fresh screenshot because the antivirus-updated Debug-app launch already timed out earlier in this turn; no fallback UI automation was used. |
| Beginner safety copy | Pass | Resident app ownership is distinct from overlapping signal totals; protected-only summaries say `仅供查看`; names, paths, and close/disable promises are absent. | Diagnostic evidence remains visible. |

### 2026-07-16 - C-drive application ownership summaries

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | `CDriveApplicationOwnershipSummaryTests` proves exhaustive groups and filter parity; focused mutation-authority search reports 0 hits. | No XAML structure, scanner discovery, recommendation, plan, pipeline, worker, trust, confirmation, registry, file, process, or mutation changed. |
| Behavioral verification | Pass | Focused 3/3; related application/health/Agent/system tests 238/238; full regression 899/899. | Existing C-drive total/filter membership and ordinary Agent behavior remain green. |
| Build and encoding | Pass | Solution build reports 0 warnings/errors; 325 non-generated C#/XAML files decode as strict UTF-8 with no replacement characters. | None observed. |
| Frontend, accessibility, and UX | Warn | Existing compact `AppsSummaryTextBlock` and `HealthDigestLatestSummaryTextBlock` remain the only controls; each AutomationId occurs exactly once and existing static order tests remain green. | No fresh screenshot because the antivirus-updated Debug-app launch already timed out earlier in this turn; no fallback UI automation was used. |
| Beginner safety copy | Pass | Ordinary, system, and ownership-pending C-drive counts are separate; protected-only summaries say `仅供查看`; names and local paths are absent. | Diagnostic evidence remains visible and no action is promised. |

### 2026-07-16 - Health risk and ownership wording

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | `HealthRiskOwnershipConsistencyTests` covers low/high clean findings and ordinary/system/ownership-pending startup profiles; focused mutation-authority search reports 0 hits. | No scanner discovery, recommendation generation, plan, pipeline, worker, trust, confirmation, registry, process, or file mutation changed. |
| Behavioral verification | Pass | Focused 5/5; related health/Agent/scanner tests 244/244; full regression 896/896. | High-risk-only C-drive guidance explicitly remains observation-only. |
| Build and encoding | Pass | Solution build reports 0 warnings/errors; 323 non-generated C#/XAML files decode as strict UTF-8 with no replacement characters. | None observed. |
| Frontend, accessibility, and UX | Warn | Existing first-view and Agent surfaces now receive risk-calibrated, path-free text from typed presenters. | No fresh screenshot because the antivirus-updated Debug-app launch already timed out earlier in this turn; no fallback UI automation was used. |
| Beginner safety copy | Pass | Only `None`/`Low` clean findings say low-risk/confirm-to-quarantine; medium/high findings say observe and prepare snapshot/rollback. Protected-only startup evidence rates `仅供查看`. | Higher-risk evidence is retained rather than hidden. |

### 2026-07-16 - Agent aggregate action authority

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | `AgentAggregateActionAuthorityTests` covers ordinary, D-data-only, system, and managed-root ownership-pending profiles; focused mutation-authority search reports 0 hits. | No scanner, hydration, exact-target identity, plan, pipeline, worker, trust, confirmation, or mutation authority changed. |
| Behavioral verification | Pass | Focused 5/5; related Agent/product/system/ownership/storage tests 234/234; full regression 891/891. | Existing navigation-only behavior and ordinary startup reviews remain green. |
| Build and encoding | Pass | Solution build reports 0 warnings/errors; 321 non-generated C#/XAML files decode as strict UTF-8 with no replacement characters. | None observed. |
| Frontend, accessibility, and UX | Warn | Homepage/Agent dynamic text now presents ordinary review, data-location review, and read-only system evidence separately. | No fresh screenshot because the antivirus-updated Debug-app launch already timed out earlier in this turn; no fallback UI automation was used. |
| Beginner safety copy | Pass | Protected commands/startups/C-drive clues say `仅供查看`; D-installed data clues say not to repeat main-program migration; all output is path-free and non-executable. | System evidence remains visible instead of being hidden. |

### 2026-07-16 - Homepage migration closure authority

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | `MigrationClosureHomepageAuthorityTests` covers reviewable, protected historical, and unavailable targets; focused mutation-authority search reports 0 hits. | No monitor, scanner, plan, pipeline, worker, trust, confirmation, or mutation authority changed. |
| Behavioral verification | Pass | Focused 4/4; related migration/home/product/personal-storage tests 193/193; full regression 886/886. | Ordinary exact-target navigation remains green while both read-only cases use generic Applications navigation. |
| Build and encoding | Pass | Solution build reports 0 warnings/errors; 319 non-generated C#/XAML files decode as strict UTF-8 with no replacement characters. | None observed. |
| Frontend, accessibility, and UX | Warn | Typed Core copy distinguishes one actionable review from two read-only historical records and removes false target labels. Computer Use discovery works, but Debug-app launch timed out and no window appeared on the follow-up poll. | A fresh real WPF screenshot remains pending; no PowerShell UIAutomation/SendKeys fallback was used. |
| Beginner safety copy | Pass | Protected history says `系统相关旧迁移记录，仅供查看`; ambiguous history says it cannot uniquely map to an app; both plans explicitly refuse migration action generation. | Visible finding and Agent text contain no local paths. |

### 2026-07-15 - Migration closure tile and catalog safety

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | `MigrationClosureCatalogPresentationTests` covers protected, ordinary-warning, ordinary-healthy, and mixed-summary behavior; focused authority search has zero hits. | No monitoring, matching, drawer action, plan, pipeline, or mutation authority changed. |
| Behavioral verification | Pass | Focused 4/4; related migration/catalog/system/ownership/product tests 191/191; full regression 882/882. | Historical record identity remains constrained by current name lookup but no longer changes protected tile authority. |
| Build and encoding | Pass | Solution build reports 0 warnings/errors; 318 non-generated C#/XAML files decode as strict UTF-8 with no replacement characters. | None observed. |
| Frontend, accessibility, and UX | Warn | Tile label/status/priority and summary copy now come from typed Core presenters; legacy WPF override hits are zero. Computer Use discovery succeeded, but `launch_app` timed out and a follow-up poll found no OMNIX window. | A fresh real WPF screenshot is still pending; no UIAutomation/SendKeys fallback was used. |
| Beginner safety copy | Pass | System-related historical records are explicitly `仅供查看`; protected tiles no longer appear as actionable red migration failures. | Drawer keeps the detailed historical reminder as secondary text. |

### 2026-07-15 - Central app action entry guards

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | `AppActionEntryGuardTests` proves policy parity and guard ordering; focused authority search has zero hits. | No scanner, plan contents, operation, pipeline, worker, trust, confirmation, or mutation authority changed. |
| Behavioral verification | Pass | Focused 2/2; related system/ownership/Agent/cache/startup/uninstall/product tests 409/409; full regression 878/878. | Manual click-through remains covered only by existing GUI smoke history. |
| Build and encoding | Pass | Solution build reports 0 warnings/errors; 317 non-generated C#/XAML files decode as strict UTF-8 with no replacement characters. | None observed. |
| Frontend, accessibility, and UX | Warn | Denied indirect entries render a shared action-host refusal and clear pending state; existing buttons/AutomationIds are unchanged. | No fresh real WPF screenshot/click-through in this slice. |
| Beginner safety copy | Pass | Existing system/ownership action reasons are reused; uninstall refusal explicitly says no restore preparation, uninstaller, or residue handling occurred. | None observed. |

### 2026-07-15 - Migration closure permission consistency

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | `MigrationClosurePermissionTests` covers system, managed-root ownership-pending, and ordinary D-installed closure states; focused authority search has zero hits. | No monitor, planner, snapshot, rollback, worker, trust, confirmation, pipeline, or mutation authority changed. |
| Behavioral verification | Pass | Focused 3/3; related migration/system/ownership/Agent/product tests 250/250; full regression 876/876. | Stale-record identity remains name-based but can no longer grant protected-profile review authority. |
| Build and encoding | Pass | Solution build reports 0 warnings/errors; 316 non-generated C#/XAML files decode as strict UTF-8 with no replacement characters. | None observed. |
| Frontend, accessibility, and UX | Warn | Existing drawer controls now bind typed advice, label, enabled state, and reason; unconditional migration button override is absent. | No fresh real WPF screenshot/click-through in this slice. |
| Beginner safety copy | Pass | Protected advice appears first and stale closure evidence is explicitly labeled `迁移记录提醒`; visible test text is path-free. | Tile/catalog closure labeling is separate presentation and remains a later audit candidate. |

### 2026-07-15 - Uninstall residue review availability

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and authority | Pass | `AppResidueReviewAvailabilityTests` proves ordinary external-uninstall recovery and protected-profile denial; focused authority search has zero hits. | No scanner, quarantine, pipeline, worker, trust, or confirmation authority changed. |
| Behavioral verification | Pass | Focused 3/3; related system/ownership/uninstall/product tests 200/200; full regression 873/873. | Real external-uninstall click-through remains a manual scenario. |
| Build and encoding | Pass | `dotnet build ComputerSecuritySoftware.slnx --no-restore` reports 0 warnings/errors; 315 non-generated C#/XAML files decode as strict UTF-8 with no replacement characters. | None observed. |
| Frontend, accessibility, and UX | Warn | Existing stable `DrawerResidueReviewButton` now receives the typed enabled state and plain disabled tooltip. | No fresh real WPF screenshot/click-through in this slice. |
| Beginner safety copy | Pass | Reasons distinguish system ownership refusal from ordinary external-uninstall review and expose no paths. | Disabled tooltips depend on WPF hover behavior; the handler also writes the reason to status if invoked indirectly. |

### 2026-07-15 - App drawer stale-state invalidation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Empty-state visible text is path/identifier free; focused authority hits 0; source contract proves all three pending fields are nulled through collapsed host. | No scan or mutation authority was added. |
| Data, API, and consistency | Pass | Focused 4/4 and related 206/206 cover empty/filter wiring, technical collapse, cache/startup pending flows, uninstall, Agent handoff, and inventory loading. | Loading and completed-empty copy remain distinct. |
| Destructive-operation safety | Pass | Zero context clears pending operation/target fields before buttons remain disabled; no handler, pipeline, worker, trust, or mutation code changed. | Old plans cannot survive through the shared clear path. |
| Beginner UX | Warn | Category and all metrics reset, technical button returns to `查看技术详情`, and stable `DrawerTechnicalDetailsButton` AutomationId exists. | Real WPF screenshot remains unavailable through Computer Use. |
| Regression and build | Pass | Full suite 870/870; solution build 0 warnings/errors; 314 strict UTF-8 files with zero replacement characters. | Normal app selection and action previews remain green. |

### 2026-07-15 - C-drive catalog summary consistency

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Focused text rejects C/D paths and cleanup/releasable claims; focused authority hits 0. | Only aggregate counts enter the beginner summary. |
| Data, API, and consistency | Pass | Focused 3/3 and related 232/232 cover C main, D main with C data, both, unknown main, duplicate/descendant clues, ordinary D, malformed non-C clue, Agent, digest, tiles, and catalog. | Filter count equals structured footprint total. |
| Destructive-operation safety | Pass | No cleanup, migration, uninstall, operation, pipeline, worker, trust, or mutation code changed. | Footprint is diagnostic evidence only. |
| Beginner UX | Warn | Summary separates main program and data/cache, has stable `AppsSummaryTextBlock` AutomationId, and is statically before the grid. | Computer Use launch remains unavailable, so no real screenshot is claimed. |
| Regression and build | Pass | Full suite 867/867; solution build 0 warnings/errors; 312 strict UTF-8 files with zero replacement characters. | Old private summary method is absent. |

### 2026-07-15 - Uninstallable catalog safety consistency

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Focused cases cover ordinary with/without command, system with command, unknown managed root, and publisher-only ordinary path; focused authority hits 0. | No path or command is added to beginner-visible catalog copy. |
| Data, API, and consistency | Pass | Focused 7/7 and related 188/188 prove catalog membership equals drawer uninstall-action availability and system preview refuses. | `CanReviewUninstall` is read-only and side-effect free. |
| Destructive-operation safety | Pass | No trust, launcher, operation, pipeline, worker, or mutation code changed; production execution readiness remains fail-closed. | Review availability is not final consent or execution authority. |
| Beginner UX | Warn | `可卸载` no longer contains profiles whose drawer immediately refuses uninstall; system preview gives the same conclusion. | No new XAML was needed, and the inherited Computer Use launch failure prevents fresh screenshot proof. |
| Regression and build | Pass | Full suite 864/864; solution build 0 warnings/errors; 311 strict UTF-8 files with zero replacement characters. | Ordinary and publisher-only behavior remains green. |

### 2026-07-15 - Truthful normal-application catalog filter

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Predicate remains exactly `Category == Normal`; focused method authority hits 0. | No scanner evidence, paths, or user data are added. |
| Data, API, and consistency | Pass | Focused 3/3 and ProductExperienceTests 169/169 cover enum parsing, behavior, WPF tag/id/copy, search, sort, and catalog contracts. | Active `OfficeStudy`, `OfficeAppsFilterButton`, and `办公学习` implementation hits are 0. |
| Destructive-operation safety | Pass | No action presenter, operation, pipeline, worker, trust, or mutation code changed. | Filter rename cannot enable an action. |
| Beginner UX | Warn | Visible filter now says `普通应用` and has stable `NormalAppsFilterButton` AutomationId. | Real WPF launch remains unavailable through Computer Use, so no screenshot is claimed. |
| Regression and build | Pass | Full suite 862/862; solution build 0 warnings/errors; 310 strict UTF-8 files with zero replacement characters. | Existing membership and ordering remain green. |

### 2026-07-15 - Software category evidence and confidence

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Beginner summaries are path-free; source tests cover name/publisher/path/fallback/unknown; focused test authority scan is empty. | Matched rules are fixed local constants and appear only in hidden technical details. |
| Data, API, and consistency | Pass | Focused 7/7 and related 218/218 cover scanner output, fallback, growth cloning, fixture compatibility, system/unknown denies, and static UI wiring. | Profile category remains the existing policy input; mismatched/missing observations are explained as unavailable. |
| Destructive-operation safety | Pass | No planner, handler, pipeline, worker, trust, or mutation code changed; assessment is read-only and does not participate in action availability. | Existing system and unknown managed-root denies remain green. |
| Beginner UX | Warn | Drawer shows one compact explanation before storage details with stable `DrawerCategorySummaryTextBlock` AutomationId and static order test. | Computer Use reached Windows after the antivirus update, but `launch_app` timed out and no real screenshot can be claimed. |
| Regression and build | Pass | Full suite 861/861; solution build 0 warnings/errors; 310 strict UTF-8 files with zero replacement characters. | Existing category precedence and application catalog behavior remain unchanged in this slice. |

### 2026-07-15 - Unknown system-ownership review

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Managed-root tests receive no enabled modifying action and visible text rejects root paths, service ids, and uninstall commands. | Category remains Unknown; raw paths remain technical-only. |
| Data, API, and consistency | Pass | Focused/system/handoff 17/17 and related 196/196 cover Windows root, WindowsApps, publisher-only ordinary D install, system-category deny, exact Agent handoffs, and existing drawer contracts. | Canonical current OS roots are used; publisher text is not an allow/deny input. |
| Destructive-operation safety | Pass | Uninstall, migration, cache, and startup actions/previews refuse while technical details remain enabled; no planner/handler/pipeline implementation changed. | Existing trust/revalidation boundaries remain authoritative. |
| Beginner UX | Warn | The grid and drawer consistently say `系统归属待确认` and explain that only details are available. | No XAML changed and the inherited Computer Use launch failure prevents a fresh screenshot claim. |
| Regression and build | Pass | Full suite 854/854; solution build 0 warnings/errors; 309 strict UTF-8 files with zero replacement characters. | Ordinary Microsoft publisher and normal third-party behavior remain green. |

### 2026-07-15 - System-application read-only boundary

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | System test includes uninstall/cache/startup/service/task evidence yet receives no enabled modifying action; visible reasons reject fixture paths/identifiers. | Technical details remain the only enabled drawer action. |
| Data, API, and consistency | Pass | Focused/system-handoff 14/14 and related 193/193 cover system retain advice, four disabled actions, ordinary-app preservation, exact Agent handoffs, product drawer contracts, and location/size labels. | Category is checked before C-drive or evidence-field advice. |
| Destructive-operation safety | Pass | No operation/planner/handler/pipeline implementation changed; focused test scope contains zero process, registry, or file/directory mutation references. | Existing lower-level trust gates remain unchanged and unreachable from disabled system drawer actions. |
| Beginner UX | Warn | Gray system tiles now agree with a retain-only drawer and plain disabled reasons. | No XAML changed and the recorded Computer Use launch failure prevents a fresh screenshot claim. |
| Regression and build | Pass | Full suite 851/851; solution build 0 warnings/errors; 308 strict UTF-8 files with zero replacement characters. | Ordinary app uninstall, migration, cache, startup, Agent, installer, and recovery contracts remain green. |

### 2026-07-15 - Compact application size explanation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Focused tests reject fixture paths and terms implying releasable bytes; the presenter formats aggregate numeric fields only. | Cache is labeled identifiable, not automatically deletable. |
| Data, API, and consistency | Pass | Focused/neighbor 19/19 and related 201/201 cover positive values, default zero, identified-but-unmeasured paths, recent growth, product copy, and storage advice. | Exact trend availability remains the responsibility of the separate growth observation. |
| Beginner UX | Warn | One compact sentence now covers main install, data, cache, and growth with explicit unavailable states. | No XAML changed and the recorded Computer Use launch failure prevents a fresh screenshot claim. |
| Regression and build | Pass | Full suite 849/849; solution build 0 warnings/errors. | Existing drawer, Agent, grid, installer, migration, uninstall, cache, startup, and recovery contracts remain green. |
| Source integrity | Pass | 307 non-generated C#/XAML files strict UTF-8; invalid/replacement-character files 0. | No model, scanner, XAML, handler, operation, pipeline, worker, or trust behavior changed. |

### 2026-07-15 - Explicit application-grid C-drive labels

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Focused tests reject C/D paths in tile visible/accessibility text; the new helper returns only fixed labels. | No raw path or identifier is added to the grid. |
| Data, API, and consistency | Pass | Focused 5/5 and related 198/198 cover C main, D main with C data, unknown main, growth, resident, system, catalog behavior, and existing product contracts. | `AppTileStatus`, risk, sort, filter, and closure override are unchanged. |
| Beginner UX | Warn | The grid now names why an app needs review instead of showing generic `需关注`. | No XAML changed and the same-turn Computer Use launch timed out, so no screenshot is claimed. |
| Regression and build | Pass | Full suite 846/846; solution build 0 warnings/errors. | Drawer, Agent, installer report, migration, uninstall, cache, startup, and recovery contracts remain green. |
| Source integrity | Pass | 306 non-generated C#/XAML files strict UTF-8; invalid/replacement-character files 0. | No handler, operation, pipeline, worker, trust, or mutation behavior changed. |

### 2026-07-15 - Installer report program/data placement

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Visible-output tests reject fixture C/D paths; scoped source scan found zero operation/pipeline, process start, registry, or file/directory mutation references. | Raw paths remain in the existing collapsed technical details only. |
| Data, API, and consistency | Pass | Focused 4/4 and related 261/261 cover D-owned, D-unattributed, C-with-external-data, no-unique-software, deduplication, existing evidence review, and app handoff contracts. | Footprint-only changes remain candidates and are not assigned to the new software. |
| Beginner UX | Warn | Summary, software card, C-drive card, and Agent answer now agree on main-program versus data placement. | Computer Use launch timed out and passive refresh found no OMNIX window; no fresh screenshot is claimed. |
| Regression and build | Pass | Full suite 842/842; solution build 0 warnings/errors. | Existing installer execution, snapshot, candidate preview, app handoff, and product contracts remain green. |
| Source integrity | Pass | 305 non-generated C#/XAML files strict UTF-8; invalid/replacement-character files 0. | No XAML, AutomationId, handler, worker, signer, or mutation behavior changed. |

### 2026-07-15 - Main-program versus C-drive data location

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Primary drawer/Agent tests reject fixture C/D paths; scoped source scan found zero operation pipeline, process start, registry, service-control, or file/directory mutation references. | Paths remain available only in the existing technical details surface. |
| Data, API, and consistency | Pass | Focused 5/5 and related 251/251 cover D/no-C, D/with-C, C/with-C, unknown/with-C, deduplication, migration availability, and exact location handoff. | C-installed main-program descendants are excluded from the separate data/write count. |
| Beginner UX | Warn | Location, advice, migration summary, preview, and disabled-action reason now agree that main program and data location are separate decisions. | No XAML changed; the inherited Computer Use launch failure means no fresh screenshot is claimed. |
| Regression and build | Pass | Full suite 838/838; solution build 0 warnings/errors. | Existing application, Agent, growth, migration, uninstall, cache, startup, and recovery contracts remain green. |
| Source integrity | Pass | 304 non-generated C#/XAML files strict UTF-8; invalid/replacement-character files 0. | No XAML, AutomationId, handler, operation, worker, trust, or mutation behavior changed. |

### 2026-07-15 - Application growth explanation and prevention

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Growth model/presenter scan found zero operation/pipeline, process start, registry write, or file/directory move/delete references; reflection test rejects path/file/operation fields. | Only software name, comparison state/count, bytes, and aggregate C-drive/cache counts cross into Agent presentation. |
| Data, API, and consistency | Pass | Focused 7/7 and related 286/286 cover exact/generic/ambiguous/explicit-operation targets, Available/Insufficient/Unavailable, positive/zero growth, mismatch, and inventory-scan-re-resolution ordering. | One snapshot remains a baseline; zero growth is stated only for the current comparison window. |
| Beginner UX | Warn | Answers visibly separate `现在腾空间` from `以后防止继续增长` and retain a details-only app handoff. | No XAML changed, and real launch remains under the recorded Computer Use Warn, so no fresh screenshot is claimed. |
| Regression and build | Pass | Full suite 833/833; solution build 0 warnings/errors. | Existing C-drive, health, Agent, application, cache, migration, uninstall, startup, and recovery contracts remain green. |
| Source integrity | Pass | 303 non-generated C#/XAML files strict UTF-8; invalid/replacement-character files 0. | No XAML, AutomationId, event binding, descriptor, handler, pipeline, worker, or trust policy changed. |

### 2026-07-15 - Exact Agent application-action handoff

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Agent Core scan found zero `OperationDescriptor`, `SafetyOperationPipeline`, process start, or file/directory move/delete references; focused source contract verifies the WPF handoff helper as preview-only. | Natural language carries a typed review choice only; current operation, confirmation, signer, snapshot, and rollback gates remain authoritative. |
| Data, API, and consistency | Pass | Focused 12/12 and related 257/257 cover four exact actions, three details-only neighbors, four unavailable/system refusals, fresh identity resolution, and shared manual preview methods. | The existing drawer `IsEnabled` decision is reused; stale/duplicate target behavior is unchanged. |
| Beginner UX | Warn | Explicit app questions now show precise buttons and remove one repeated manual choice while location/troubleshooting questions remain simple details. | No XAML changed, and post-antivirus Computer Use launch still has the recorded timeout Warn, so no fresh screenshot is claimed. |
| Regression and build | Pass | Corrected uninstall contract plus focused 13/13; full suite 826/826; solution build 0 warnings/errors. | One obsolete exact source string was updated without weakening captured-profile or residue-review assertions. |
| Source integrity | Pass | 301 non-generated C#/XAML files strict UTF-8; invalid/replacement-character files 0. | No XAML, AutomationId, event binding, descriptor, handler, pipeline, worker, or trust policy changed. |

### 2026-07-15 - Beginner-safe operation error boundaries

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Whole `Css.App` scan found zero `result.Error`, `policy.Error`, or `validation.Error` UI uses. | Raw operation paths, policy vocabulary, and lower-layer details no longer enter primary beginner controls. |
| Data, API, and consistency | Pass | Focused 3/3 and related workflow/product 189/189 cover pre-execution refusal versus post-attempt unknown-state wording. | Underlying failure objects, returns, rescans, Timeline reloads, handlers, and pipeline calls are unchanged. |
| Beginner UX | Warn | Fixed copy states whether no change is known or completion is unconfirmed, then names app rescan or the Undo Center as the next authority. | Two post-antivirus Computer Use launch requests timed out and passive refresh found no OMNIX window, so no fresh screenshot or antivirus-clear claim is made. |
| Regression and build | Pass | Full suite 814/814; solution build 0 warnings/errors. | Startup, quarantine, residue, C-drive cleanup, uninstall, migration, Agent, and recovery contracts remain green. |
| Source integrity | Pass | 300 non-generated C#/XAML files strict UTF-8; invalid/replacement-character files 0. | No XAML, event binding, AutomationId, descriptor, or mutation implementation changed. |

### 2026-07-15 - Beginner-safe WPF failure boundaries

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Whole `Css.App` scan found zero `ex.Message`/`exception.Message` UI uses; six fallback-line scan found zero drive-path, registry-root, or SID patterns. | Raw OS/provider/exception details no longer enter status text or residue message boxes. |
| Data, API, and consistency | Pass | Focused 2/2 and related workflow/product 186/186 cover the source boundary while existing operation flows remain unchanged. | Open/snapshot failures are known no-change; purge/restore/residue/cleanup failures preserve unknown/partial-state semantics. |
| Beginner UX | Warn | Each failure now states what is known and names the next page/action to re-establish current state. | No XAML changed, but real WPF fault injection for all six branches was not performed. |
| Regression and build | Pass | Full suite 811/811; solution build 0 warnings/errors. | Installer, quarantine, Timeline, residue, C-drive, Agent, and signed-workflow contracts remain green. |
| Source integrity | Pass | 299 non-generated C#/XAML files strict UTF-8; invalid/replacement-character files 0. | No XAML, event binding, or AutomationId changed. |

### 2026-07-15 - Bounded application runtime observation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Core source contains zero process-name/id/path/command-line/executable/exact-CPU fields; Win32 authority scan found zero MainModule/command-line, kill/close/suspend/terminate/priority, process launch, pipeline/operation, registry-write, or file-delete hits. | Raw process names and objects remain inside Css.Win32; the Agent receives aggregate count, memory, coarse CPU, duration, and availability only. |
| Data, API, and consistency | Pass | Focused 37/37 and related Agent 152/152 cover exact tokens, 350 ms/32-process bounds, real Windows reading, availability states, generic/ambiguous/operation exclusions, natural resource wording, and inventory-before-sample ordering. | Empty trustworthy identity is Unavailable, not a false NotRunning state; CPU is a coarse one-sample conclusion. |
| Beginner UX | Warn | Freeze/resource answers explain current aggregate evidence and explicitly state that it cannot prove root cause or justify ending a process. | Existing Agent panel is reused; no XAML changed and fresh visual proof retains the current Computer Use launch Warn. |
| Regression and build | Pass | Full suite 809/809; solution build 0 warnings/errors. | Existing crash, inventory, health, app action, install, uninstall, migration, and recovery contracts remain green. |
| Source integrity | Pass | 298 non-generated C#/XAML files strict UTF-8; invalid/replacement-character files 0. | No XAML, event binding, or AutomationId changed. |

### 2026-07-15 - Bounded application crash-log observation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Core model contains zero message/property/provider/event-id/path fields; scoped authority scan found zero format/clear/export, process launch, pipeline/operation, registry-write, or file-delete calls. | Raw event values are bounded and transient inside Css.Win32; the Agent receives only availability, count, window, and latest time. |
| Data, API, and consistency | Pass | Focused crash/troubleshooting 19/19 and related Agent 121/121 cover allowlisted provider/id pairs, 24-hour/128-candidate bounds, token matching, target exclusions, three availability states, symptom wording, and inventory-before-observation ordering. | Reading failure is Unavailable, not NotFound; matching evidence is correlation, not root-cause proof. |
| Beginner UX | Warn | Answers say whether matching records were found and what that does not prove, while retaining the exact app drawer action. | After antivirus definitions updated, Computer Use launch still timed out and passive refresh found no app/window, so no fresh screenshot is claimed. |
| Regression and build | Pass | Full suite 782/782; solution build 0 warnings/errors. | Existing Agent, health, inventory, app, and protected-operation workflows remain green. |
| Source integrity | Pass | 294 non-generated C#/XAML files strict UTF-8; invalid/replacement-character files 0. | No XAML, event binding, or AutomationId changed. |

### 2026-07-15 - Application-specific troubleshooting answers

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Visible tests reject process/service/task names and private paths; presenter authority audit found zero process, pipeline/operation, registry-write, or file-mutation calls. | Only aggregate profile counts are shown; no event log or performance state is invented. |
| Data, API, and consistency | Pass | Focused 55/55 and related 242/242 cover named-app hydration, generic/system exclusions, exact target, crash/freeze/vague branches, and explicit action priority. | Event Viewer/Task Manager remain separate follow-up questions through the existing allowlisted open-only routes. |
| Beginner UX | Warn | Replies distinguish flash-crash, no-response, and vague abnormal wording, state missing evidence, and keep one clear app-details action. | Existing response panel is reused; fresh screenshot remains under the recorded Computer Use launch Warn. |
| Regression and build | Pass | Full suite 773/773; solution build 0 warnings/errors. | Existing Agent and app workflows remain green. |
| Source integrity | Pass | 290 non-generated C#/XAML files strict UTF-8; replacement-character files 0. | No XAML, event binding, or AutomationId changed. |

### 2026-07-15 - Natural-language whole-computer diagnosis

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Agent Ask authority audit found zero pipeline/operation, recommendation execution, process launch, registry write, or file/directory move/delete references. | Diagnosis loads read-only evidence only; findings are not consent to act. |
| Data, API, and consistency | Pass | Focused tests 78/78 cover four full-diagnosis phrases, four neighboring scopes, completed-summary reuse, no duplicate inventory/machine probe, and await ordering. | C-drive and SystemDiagnosis share full health; performance/hardware keep lightweight evidence; app-like text keeps inventory. |
| Beginner UX | Warn | Natural whole-computer questions now perform the scan and answer directly instead of returning software-only advice. | No XAML changed; first-visible runtime proof remains under the earlier Computer Use launch Warn. |
| Regression and build | Pass | Isolated process-image test 1/1; final full suite 763/763; solution build 0 warnings/errors. | One alias-sensitive test assumption was corrected without changing product trust policy. |
| Source integrity | Pass | 289 non-generated C#/XAML files strict UTF-8; replacement-character files 0. | No event binding or AutomationId changed. |

### 2026-07-15 - No-scan Agent greetings and capability help

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Exact non-diagnostic questions return before inventory hydration; no cloud, process, scanner, operation, or system API was added. | Capability replies retain the standard local-only privacy and non-execution boundary. |
| Data, API, and consistency | Pass | Focused tests 50/50 cover five no-scan phrases, unknown app text, and mixed greeting plus app text. | Whole-question matching preserves post-scan exact-profile resolution for possible app mentions. |
| Beginner UX | Pass | `你好`, `谢谢`, and `你能做什么` answer immediately with plain capabilities and no false `已扫描` claim. | No new visible layout or screenshot is required. |
| Regression and build | Pass | Full suite 751/751; solution build 0 warnings/errors. | Existing C-drive, app, startup, uninstall, migration, and routing answers remain green. |
| Source integrity | Pass | 288 non-generated C#/XAML files strict UTF-8; replacement-character files 0. | No XAML or event binding changed. |

### 2026-07-15 - Automatic System Diagnosis skill evidence

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Skill-handler audit found zero operation/pipeline, recommendation execution, process launch, registry write, or file/directory move/delete references. | The existing bounded read-only scan is reused; the skill click does not authorize handling any finding. |
| Data, API, and consistency | Pass | Policy tests cover all eight skill categories and existing-evidence reuse; related tests passed 223/223. | Home, C-drive questions, and System Diagnosis join the same in-flight gate; failure/cancellation is not cached as success. |
| Beginner UX | Warn | The extra manual homepage step is removed and the final Agent reply uses refreshed evidence. | No XAML changed; fresh WPF visual proof remains under the existing Computer Use launch Warn. |
| Regression and build | Pass | Full suite 744/744; solution build 0 warnings/errors. | Existing Agent, health scan, and product contracts remain green. |
| Source integrity | Pass | 288 non-generated C#/XAML files strict UTF-8; 16 XAML files parsed. | No new event binding or AutomationId was introduced. |

### 2026-07-15 - Agent lightweight machine observation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Scoped machine-core audit found zero disk scanner, software scanner, process start/kill, operation/pipeline, registry write, or file move/delete references. | Probe retains aggregate process count only; no process names, paths, serials, device ids, or mutation authority are added. |
| Data, API, and consistency | Pass | Focused tests 21/21 cover intent gating, full-health reuse, available/not-present/unavailable presentation, no fake score, and full-scan cache reuse. | Full health and lightweight Agent answers share one machine-dimension formatter and one in-flight observation gate. |
| Beginner UX | Warn | Agent automatically prepares evidence for configuration/cardinality questions and the hardware skill; unavailable evidence is not called normal. | Post-antivirus Computer Use launch timed out and no target window/process remained, so a fresh first-view screenshot is not claimed. |
| Regression and build | Pass | Related tests 236/236; full suite 735/735; solution build 0 warnings/errors. | Existing C-drive, Agent, hardware, skill-card, and health-summary contracts remain green. |
| Source integrity | Pass | 287 non-generated C#/XAML files strict UTF-8; 16 XAML parsed; 120 event bindings resolved; 277 literal AutomationIds unique. | No new XAML or event handler was required. |

### 2026-07-15 - Automatic application inventory loading

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Lazy orchestration source has zero `Process.Start`, pipeline, delete/move, or registry-write references; it only calls the existing read-only inventory scanner. | Navigation is not consent and gains no mutation authority. |
| Data, API, and consistency | Pass | Gate tests 4/4 prove one in-flight load, completed-empty caching, retry after failure/fault, and forced manual refresh. | Failed refresh retains the previous inventory. |
| Beginner UX | Warn | `扫描应用` became `重新扫描`; first Apps entry and C-drive handoff auto-load; path-free failure copy is covered by source tests. | Real screenshot remains unavailable after the earlier Computer Use launch timeout. |
| Regression and build | Pass | Related tests 170/170; full suite 699/699; build 0 warnings/errors. | Existing app filters and root-cause authority checks remain green. |
| Source integrity | Pass | 282 strict UTF-8 files; 16 XAML parsed; 120 event bindings resolved; zero duplicate literal AutomationIds. | No new XAML file or handler binding was introduced. |

### 2026-07-15 - Early production identity readiness for uninstall and migration

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Unsigned/untrusted packages have `CanPrepareExecution=false`; both WPF plan windows gate evidence/final-confirmation actions; production coordinators retain `_assessTrust()` and same-signer/hash launch policy. | No unsigned mutation mode was added. |
| Data, API, and consistency | Pass | One shared `CurrentPackageWorkerTrustProvider` and one capability-aware presenter serve both workflows; focused tests passed 7/7 and related tests 24/24. | Execution re-assesses instead of trusting the earlier UI result. |
| Beginner UX | Warn | Stable first-visible conclusion/status/next-step/safety AutomationIds exist and static order tests pass. | Computer Use launch timed out, so no real first-view screenshot is claimed. |
| Regression and build | Pass | Full suite 695/695; solution build 0 warnings/errors. | Debug App/Worker are unsigned and correctly present preview-only readiness. |
| Source integrity | Pass | 280 C#/XAML files strict UTF-8; 16 XAML parsed; 120 event bindings resolved; zero duplicate literal AutomationIds. | WPF authority scan found zero production launcher/worker-mode/file-move references. |

### 2026-07-15 - Interactive truthful Agent skill catalog

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Card handler passes only `AgentSkillCategory` to `ExplainSkill` and `ApplyAgentConversationReply`; focused source tests reject process/tool/settings openers, page calls, operations, pipeline, registry, and file APIs. | Replies use current local summaries and the existing privacy line. |
| Data, API, and consistency | Pass | All eight enum categories have explicit replies; diagnosis/background/hardware reuse current evidence; unsupported desktop/session categories expose no navigation. | Card clicks do not become consent or operation evidence. |
| Destructive-operation safety | Pass | Every reply retains `CanExecuteDirectly=false`; no handler execution authority exists. | No tool, setting, session, desktop, process, service, task, file, installer, uninstall, or migration action ran. |
| Frontend, accessibility, and UX | Warn | Compact buttons have stable category-bound AutomationIds; 16 XAML parse, 82 handlers resolve, and literal IDs are unique; isolated first-view smoke is prepared/source-tested. | Visible run and screenshot inspection are blocked by Codex GUI quota; button density/layout is not yet accepted. |
| Testing and verification | Pass | Focused 25/25; full 679/679; solution build 0 warnings/errors; 278 strict UTF-8 C#/XAML files; clean process/fixture state. | Source/runtime behavior is green; visual proof remains a separate Warn. |
| Operations, dependencies, and release | Warn | No new dependency, elevation, external command, or mutation authority was added. | Run `.omx/gui-agent-skill-cards-smoke.ps1` when visible-window approval returns. |

### 2026-07-15 - Beginner hardware configuration answer

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Probe/test source excludes serial, user, domain, PNP/device ids, paths, process launch, registry writes, and file mutation; labels are control-stripped, path-rejected, length-bounded. | Only CPU/GPU names, logical count, Windows caption/version, and architecture are retained. |
| Data, API, and consistency | Pass | Fixed WMI queries have two-second timeouts and bounded results; fixed CPU registry and display-enumeration fallbacks passed real-machine testing; hardware evidence survives health enrichment. | Missing providers/fields remain explicit; no compatibility result is invented. |
| Destructive-operation safety | Pass | Model and Agent retain `CanExecuteDirectly=false`; source gates reject pipeline/operation/mutation APIs. | No driver, process, service, task, registry write, file, installer, migration, or session action ran. |
| Frontend, accessibility, and UX | Warn | Stable existing Agent response AutomationIds and a dedicated first-view/path-privacy smoke are present and source-tested. | Visible smoke was rejected by Codex GUI quota before launch; `.omx/qa-agent-hardware-summary.png` is not yet accepted evidence. |
| Testing and verification | Pass | Real-machine hardware focus 5/5; full 676/676; solution build 0 warnings/errors; 278 strict UTF-8 C#/XAML files; 16 XAML parses; no process/fixture leftovers. | GUI screenshot remains a separate Warn, not a test failure. |
| Operations, dependencies, and release | Warn | Huorong definitions are current; no elevation or external command was added for hardware observation. | Rerun the documented smoke when visible-window approval is available. |

### 2026-07-15 - Natural-language settings and troubleshooting routing

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Replies carry only typed shortcut kind/catalog id; tests reject command echo, operation descriptors, pipeline calls, process launch, registry writes, and file mutation in the Agent handler/presenter. | User wording never becomes a command or URI; unknown ids fail closed. |
| Data, API, and consistency | Pass | Network, Bluetooth, sound, display, power, driver/device, crash/blue-screen, and named-tool cases resolve to reviewed catalog entries; high-risk registry routing retains catalog confirmation semantics. | The Agent explicitly says one sentence cannot prove a fault root cause. |
| Destructive-operation safety | Pass | Protected Device Manager flow opened the real confirmation and was closed as cancel; baseline/final `mmc` comparison found no external tool start and output records `noOperationExecuted=true`. | No setting, driver, registry, service, task, process, or file was changed. |
| Frontend, accessibility, and UX | Pass | Stable Agent response/button AutomationIds are exercised; answer, evidence, safety boundary, and next step share the first visible panel; exact confirmation title/content/buttons are visible in clean screenshots. | Visual evidence: `.omx/qa-agent-troubleshooting-routing.png` and `.omx/qa-agent-tool-open-confirmation.png`. |
| Testing and verification | Pass | Focused 20/20; full 671/671; solution build 0 warnings/errors; 275 non-generated strict UTF-8 C#/XAML files; 16 XAML parses; no process/fixture leftovers. | Two failed visual/mechanical attempts were rejected and recorded before the final clean run. |
| Operations, dependencies, and release | Pass | Huorong definitions are current and current binaries/tests run cleanly. | This slice is open-only guidance; automatic fault repair remains out of scope. |

### 2026-07-15 - Install-report exact application handoff

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Preview navigation is limited to ready/unique attribution; WPF calls only `ResolveAndOpenAppTargetAsync`; static tests reject pipeline/operation/process APIs in the handler. | Target app name is not shown in the generic preview button; no operation executed. |
| Data, API, and consistency | Pass | Cache/startup/migration ready previews carry exact target metadata; refused/guidance-only previews carry none; current inventory must resolve exactly once. | Stale missing targets trigger one read-only refresh; duplicate names refuse navigation. |
| Code quality and maintainability | Pass | Navigation metadata stays in the preview model; target resolution and app drawer workflow are reused. Solution build passed with 0 warnings/0 errors. | No second cache/startup/migration execution path was added. |
| Testing and verification | Pass | Focused 24/24; full 661/661; 275 strict UTF-8 C#/XAML files; 16 XAML parses; no fixture/process leftovers. | Both first mechanical and corrected visual runs were inspected. |
| Frontend, accessibility, and UX | Pass | Stable button/title AutomationIds, real viewport intersection checks, clean window-only candidate and app-drawer screenshots. | Conclusion and next action are visible together; exact drawer shows normal cache/startup entries. |
| Operations, dependencies, and release | Pass | Smoke uses two isolated JSON inventory snapshots and does not run an installer or invoke drawer actions. | `noOperationExecuted=true`; real mutations remain in their existing confirmed workflows. |

### 2026-07-15 - Personal large-file and possible-duplicate GUI acceptance

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Analyzer uses scan metadata only; tests reject file-content reads and operation creation; full visible-window smoke found no fixture path; operation evidence remains unchanged. | Personal-file findings cannot delete, move, quarantine, or execute directly. |
| Data, API, and consistency | Pass | One long-unused candidate and one same-name/same-size possible-duplicate group were produced from a confined fixture; copy says `疑似` and states content was not compared. | Production thresholds remain 512 MB/64 MB; lower thresholds require the validated process fixture root. |
| Code quality and maintainability | Pass | Sanitization is a shared Core presentation utility; scanner, presenter, WPF navigation, and fixture seam remain separate. Build completed with 0 warnings and 0 errors. | Existing private path sanitizers can be consolidated later, but are outside this slice. |
| Testing and verification | Pass | Focused 29/29; full 661/661; 275 strict UTF-8 C#/XAML files; 16 XAML parses; zero process/fixture leftovers. | First full run exposed and corrected one brittle static assertion. |
| Frontend, accessibility, and UX | Pass | Stable list/item AutomationIds, item-level `IsOffscreen=false`, 240px list, exact navigation, and visually inspected `.omx/qa-personal-storage-candidates.png`. | Both candidate conclusions are visible without technical paths or overlap. |
| Operations, dependencies, and release | Pass | Smoke used only one GUID-named `C:\tmp` fixture and isolated app data, cleaned in `finally`; `noOperationExecuted=true`. | No real personal file or system setting was changed. |

### 2026-07-14 - Truthful whole-PC health dimensions

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Probe source retains only D capacity, physical-memory totals/load, process count, and optional power status; static audit rejects process names/modules, registry/service access, process launch/termination, and filesystem mutation. | D scope is restricted to a ready local fixed drive; Agent remains non-executable. |
| Data, API, and consistency | Warn | Explicit available/not-present/unavailable states, clamped byte/percent values, startup-signal wording, and three-distinct-snapshot trend gate are implemented with focused test source. | P/Invoke and current test source are uncompiled; runtime Windows behavior remains unproved. |
| Code quality and maintainability | Warn | Core observation contract, Win32 probe, Scanner summary builder, Agent explanation, and WPF orchestration are separate. | Compiler/analyzer evidence is unavailable; MainWindow remains large. |
| Testing and verification | Warn | 354 strict UTF-8 files; 14 XAML parses; 58 unique handlers; 254 unique AutomationIds; mojibake and scoped probe/privacy/history/Agent/UI source gates pass. | Focused/full tests cannot run until normal NuGet restore repairs assets. |
| Frontend, accessibility, and UX | Warn | Home dimensions are ordered score/C/D/memory/battery/startup/trend; result/rating cells wrap in a fixed 260px vertical-scroll table; Home/Timeline/Agent nav controls now have stable AutomationIds. | Real screenshot/UIAutomation proof at default and minimum window sizes remains required. |
| Operations, dependencies, and release | Warn | Probe runs only inside explicit manual `RunScanAsync`; no scheduler/background claim or real system change was added. | No release claim; restore/build/test/GUI remain pending. |

### 2026-07-14 - Migration closure monitoring surfaced

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | WPF constructs only `WindowsMigrationPathObserver`; scoped source audit finds no move/rollback/delete/link methods in that type. `MigrationClosurePresentation.cs` does not consume `OriginalPath` or low-level `Summary`. | Duplicate app names are not operation targets; old records cannot authorize execution. |
| Data, API, and consistency | Warn | Latest-per-software selection, 64-software/32-path bounds, local C-source/D-target validation, record context, and idempotent Home enrichment are implemented with focused test source. | New C# and tests are not compiled; runtime JSON/redirect behavior remains unproved. |
| Code quality and maintainability | Warn | Observation and mutation interfaces are separated; presentation and health enrichment live in Core; WPF performs only orchestration. | `MainWindow.xaml.cs` remains large; compiler/analyzer evidence is blocked. |
| Testing and verification | Warn | 351 files pass strict UTF-8; 14 XAML files parse; 58 unique handlers resolve; 251 AutomationIds are unique; scoped static authority/privacy/bounds/order checks pass. | Core no-restore build stops before compilation on known `NU1101`/`NU1801`; focused/full tests pending normal restore. |
| Frontend, accessibility, and UX | Warn | Home inserts closure as the second dimension and first attention finding; app failures sort first, show `迁移未闭环`, update drawer advice, and expose `复查迁移`; existing conclusion controls have stable AutomationIds. | Required real WPF screenshot/UIAutomation evidence is pending compilation. |
| Operations, dependencies, and release | Warn | No scheduler, cloud call, Worker/UAC launch, or real filesystem/system mutation ran. | Normal NuGet restore is still required; no release claim is permitted. |

### 2026-07-13 - Source-only trusted installer route and initial post-scan

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Warn | Static audit finds hash/signature/type/snapshot/fresh-consent revalidation; only the dedicated launcher contains `Process.Start`; no `runas`, hidden window, or silent switch is present. | New source is uncompiled. Real installer launch remains disabled. |
| Data, API, and consistency | Warn | Package evidence is bound to SHA-256; snapshot evidence is bounded/strict/hash-verified; automatic args are limited to high-confidence Inno/NSIS; initial post-scan never interprets exit code as success. | Runtime serialization, Authenticode, and process behavior remain deferred. |
| Code quality and maintainability | Warn | Inspector, routing policy, snapshot store, operation handler, launcher, App coordinator, consent window, and result window are separate boundaries. | Compiler/analyzer evidence is unavailable during the antivirus-definition pause. |
| Testing and verification | Fail | Temporary data-only tests were added for inspection, tamper, consent, handler, coordinator, and WPF authority, but were not run. | Build once and inspect Huorong before any test execution. |
| Frontend, accessibility, and UX | Warn | Main/consent/result XAML parses; beginner conclusion precedes folded technical details; stable AutomationIds and static order tests exist. | Real UIAutomation and first-view screenshots are required by `AGENTS.md`. |
| Operations, dependencies, and release | Warn | `InstallerLaunchFeatureEnabled=false`; no installer, UAC, build, test, registry write, or real install-directory mutation occurred. | Do not enable until corrected definitions, focused/full tests, clean security scan, and manual fixture proof pass. |

Statuses:

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

## Pre-Change Gate

- Objective, dependencies, risks, impact scope, and acceptance criteria are clear.
- Existing worktree state has been inspected.
- Affected files or modules are identified.
- Verification approach is known before implementation starts.
- Security, privacy, data, API, frontend, or release-sensitive areas are flagged when relevant.

## Pre-Delivery Gate

### Security and Privacy

- Inputs are validated and outputs are encoded where applicable.
- Secrets, tokens, passwords, and PII are not logged or committed.
- Authentication and authorization changes are reviewed for least privilege.
- File paths, uploads, system commands, and outbound requests are checked for injection or traversal risk.

### Data, API, and Consistency

- Schema changes use migrations where applicable.
- API shape, versioning, validation, pagination, and idempotency are reviewed when applicable.
- Money uses integer minor units or decimal types, never binary floating point.
- Time storage and transfer use UTC when the project handles time-sensitive data.

### Code Quality and Maintainability

- The change follows existing project patterns.
- Duplication, dead code, broad types, empty catches, and unhandled TODO/FIXME/HACK comments are reviewed.
- Error handling is explicit and does not silently swallow failures.
- Configuration is separated from code and required config fails fast.

### Testing and Verification

- Unit, integration, E2E, or manual verification is selected according to risk.
- Boundary cases and regression scenarios are covered where relevant.
- Verification commands and results are recorded in `current.md` or `handoff.md`.
- Known unverified areas are stated explicitly.

### Frontend, Accessibility, and UX

- Loading, empty, error, and boundary states are handled when UI is affected.
- Keyboard access, labels, alt text, focus visibility, and color contrast are checked when UI is affected.
- Mobile layout and user-visible text are checked when frontend behavior changes.

### Operations, Dependencies, and Release

- Dependency changes are reviewed for lockfiles, unused packages, vulnerabilities, and licenses.
- Logs, metrics, health checks, retries, timeouts, graceful shutdown, and rate limits are considered when relevant.
- README, API docs, ADRs, changelog, release notes, or migration notes are updated when needed.
- Rollback or recovery path is known for risky delivery.

## Gate Result Template

### YYYY-MM-DD - Gate name

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | N/A |  |  |
| Data, API, and consistency | N/A |  |  |
| Code quality and maintainability | N/A |  |  |
| Testing and verification | N/A |  |  |
| Frontend, accessibility, and UX | N/A |  |  |
| Operations, dependencies, and release | N/A |  |  |

Open issues:

- None.

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

### 2026-07-10 - On-demand candidate plan preview gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Ownership tests require exactly one added software profile for app-specific cache/startup/migration previews; visible models reject raw identifiers and set `CanExecuteDirectly=false`. | No operation descriptor, pipeline call, installer, cleanup, migration, startup/service/task/registry mutation, settings change, session control, or cloud AI path was added. |
| Data, API, and consistency | Pass | Cache/startup/migration paths reuse existing safe presenters; generic storage/observe previews remain guidance-only; ambiguous ownership returns `Refused`. | The preview is derived from the existing install-diff report and does not collect more system evidence. |
| Code quality and maintainability | Pass | Preview creation is centralized in `InstallSnapshotCandidatePreviewPresenter`; WPF binding is centralized in `ApplyInstallDiffCandidatePreview`; shared smoke visibility uses `Show-WpfWindowForSmoke`. | The obsolete focus/foreground helpers were removed. |
| Testing and verification | Pass | TDD red observed for missing model/UI and obsolete smoke activation. Focused preview/UI tests passed 5/5; install/product tests passed 146/146; fresh full suite passed 222/222; solution build passed with 0 warnings/errors. | Static and model evidence is current. |
| Frontend, accessibility, and UX | Pass | Stable AutomationIds and static order/handler tests cover the on-demand panel; the real fixture smoke returned `candidatePreviewReady=true`, `candidatePreviewNoExecution=true`, hidden identifiers, and collapsed technical details. | `.omx/qa-install-diff-candidate-preview.png` was generated and visually inspected without clipping or black composition blocks. |
| Operations, dependencies, and release | Warn | No app process or temporary fixture/data path remained. The shared helper uses topmost z-order without keyboard focus. | GUI proof is complete; `.omx` QA packaging exclusion remains outstanding. |

### 2026-07-10 - Uninstall recovery truth and gate hardening

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Recovery assessment hides raw paths/services; the execution gate rejects snapshot-only readiness and requires no-undo acknowledgment, usable recovery evidence, and data-backup confirmation when data paths exist. | No uninstaller, file deletion, residue move, service/startup/registry change, or elevated action was executed or enabled. |
| Data, API, and consistency | Pass | `OfficialUninstallRecoveryEvidence` records method/reference/recoverability/backup state; successful operation descriptors include recovery method/reference for later audit. | Evidence collection is not implemented yet, so real execution remains disabled. |
| Code quality and maintainability | Pass | Recovery presentation is isolated in `UninstallRecoveryAssessmentPresentation.cs`; evidence validation is shared by the gate and preflight checklist. | XAML uses ASCII plus character entities and stable AutomationIds. |
| Testing and verification | Pass | TDD red proved missing recovery truth, snapshot-only gate acceptance, missing checklist steps, smoke-contract gaps, and invalid bullet binding. Product tests 132/132; full suite 225/225; build 0 warnings/errors. | Fresh commands used the final workspace state. |
| Frontend, accessibility, and UX | Pass | GUI smoke found three protection lines, three steps, collapsed advanced details, and no execution control. Clean `.omx/qa-uninstall-plan-window.png` was visually inspected. | One compositor-black screenshot was rejected and replaced by an unchanged rerun. |
| Operations, dependencies, and release | Warn | No process or temporary fixture state remains. | Official uninstall handler, automated recovery-evidence discovery, and QA-asset packaging exclusion are still pending. |

### 2026-07-10 - Read-only reinstall-source discovery

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Presenter tests reject directories, product-code-only hints, missing files, unsigned installers, and publisher-signature mismatches; `CanExecuteDirectly=false` in every status. | Verified reinstall evidence explicitly leaves `UserDataBackupConfirmed=false`; no installer/uninstaller is launched. |
| Data, API, and consistency | Pass | `InstalledSoftwareRegistryRecordFactory` parses `InstallSource`, `WindowsInstaller`, and GUID product codes; `SoftwareInventoryBuilder` preserves them in `SoftwareProfile`. | Raw source paths/product codes appear only in technical details. |
| Code quality and maintainability | Pass | Recovery classification is centralized in `ReinstallSourceReadinessPresenter`; real scanner and WPF composition are protected by source-contract tests. | Publisher matching is local to the presenter and should be shared if another recovery source needs the same rule. |
| Testing and verification | Pass | TDD red/green covered missing types and both disconnected adapters. Scanner tests 15/15, product tests 137/137, full suite 232/232, build 0 warnings/errors. | Fresh commands used current workspace state. |
| Frontend, accessibility, and UX | Pass | Stable AutomationIds cover status/next action and advanced provenance. GUI smoke returned `reinstallReadinessVisible=true`, collapsed details, and no execution control; screenshot was visually inspected clean. | The tested real app had no trusted source, so the rendered state correctly showed recovery preparation still missing. |
| Operations, dependencies, and release | Warn | No process or temporary fixture state remained. | Official uninstall execution and `.omx` QA packaging exclusion remain pending. |

### 2026-07-10 - Guided uninstall recovery preparation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Selected installers reuse signature/publisher verification; restore scanner source contract requires SELECT from `SystemRestore` and rejects create/restore calls; all preparation models keep execution false. | Choosing a file does not launch it; restore points remain hints. |
| Data, API, and consistency | Pass | `UninstallRecoveryPreparationSession` keeps installer evidence and backup acknowledgment separate; user-selected evidence never sets backup confirmation implicitly. | Session state is local to the preview and not persisted yet. |
| Code quality and maintainability | Pass | Restore scanning is isolated in `Css.Scanner.Recovery`; preparation rules are centralized in the Core presenter/session; WPF delegates signature inspection to the existing scanner utility. | No new dependency package was added. |
| Testing and verification | Pass | TDD red/green; scanner tests 16/16, product tests 142/142, full suite 238/238, build 0 warnings/errors before the following snapshot slice. | Static and model evidence is current. |
| Frontend, accessibility, and UX | Warn | Stable AutomationIds and smoke assertions cover restore status, choose-installer button, backup checkbox, and summary. | Fresh GUI launch was rejected by the usage limit before startup; updated layout/screenshot is not visually verified. |
| Operations, dependencies, and release | Warn | No process started during the rejected GUI request. | File picker behavior is unit/static tested but not manually exercised in the updated window. |

### 2026-07-10 - Verifiable uninstall evidence snapshot

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Validator rejects missing/tampered/stale/wrong-software/id-mismatched evidence and manifests claiming rollback; test roots are isolated and removed. | Snapshot contains local technical paths, so future cloud flows must never upload it by default. |
| Data, API, and consistency | Pass | Schema version 1 records software/recovery evidence; SHA-256 and manifest identity flow into typed evidence and future operation arguments. | `CanRestoreApplication=false` is enforced in manifest, evidence, validator, and preflight copy. |
| Code quality and maintainability | Pass | Storage is isolated in `Css.Snapshot.Uninstall`; gate-facing types remain in Core to avoid circular references; writes use temp file plus atomic move. | Future retention/cleanup policy is not implemented. |
| Testing and verification | Pass | Snapshot/product focused tests and TDD red/green covered seven failure modes; final full suite passed 245/245; solution build passed with 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | N/A | Backend and gate slice only. | WPF integration intentionally deferred until visual verification is available. |
| Operations, dependencies, and release | Pass | No real snapshot root, process, or system mutation remained after tests. | Store writes only when a future caller explicitly provides an OMNIX-owned root. |

### 2026-07-10 - Non-executable uninstall final-confirmation draft

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Incomplete-preparation test proves no root directory is created; beginner text rejects C:/D: paths; complete draft preserves separate backup acknowledgment. | Snapshot manifest still contains local technical paths and needs retention policy. |
| Data, API, and consistency | Pass | Three explicit statuses distinguish refusal, verification failure, and ready draft; snapshot/recovery evidence remain typed and attached for future audit. | Pending confirmations are data only. |
| Code quality and maintainability | Pass | Orchestration is isolated in `Css.Snapshot.Uninstall`; static test rejects operation/pipeline/process APIs. | No WPF coupling was introduced. |
| Testing and verification | Pass | TDD red/green; focused tests 3/3; full suite 248/248; solution build 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | N/A | Backend-only by decision. | Recovery panel visual gate remains Warn from the previous slice. |
| Operations, dependencies, and release | Pass | Test roots are removed and no app process remains. | No real user snapshot or system mutation occurred. |

### 2026-07-10 - Read-only uninstall snapshot retention plan

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Planner recognizes only valid top-level OMNIX manifests; corrupt/unknown/reparse evidence is preserved; beginner execution is disabled. | Local paths remain only in backend plan items. |
| Data, API, and consistency | Pass | Policy uses explicit age/count limits and deterministic newest-first order; candidate reasons distinguish expiration from count. | Future archive must revalidate candidates at execution time. |
| Code quality and maintainability | Pass | Planner is isolated and source-contract tested to contain no move/delete APIs. | No new dependency added. |
| Testing and verification | Pass | TDD red/green; focused retention tests 4/4; full suite 251/251; build 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | N/A | Backend planning only. | Recovery WPF visual gate remains Warn. |
| Operations, dependencies, and release | Pass | Temp roots removed; no process or real filesystem action remained. | Next operation must be reversible archive, not purge. |

### 2026-07-10 - Reversible uninstall snapshot archive operation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Policy rejects outside-root/lacking-hash candidates; handler revalidates direct-child, reparse, hash, schema/purpose/id; no permanent-delete API. | Only OMNIX evidence manifests are eligible. |
| Data, API, and consistency | Pass | Planned SHA-256 flows through descriptor arguments; timeline records original paths and quarantine manifests; restore returns the original file. | Persisted JSON uses case-insensitive reader compatible with camelCase writer. |
| Code quality and maintainability | Pass | Policy, handler, and injected move/restore adapters are isolated in `Css.Snapshot.Uninstall`; production adapter reuses `FileQuarantineService`. | Future app registration remains intentionally absent. |
| Testing and verification | Pass | TDD red/green; focused tests 6/6 including changed source, whole-batch validation, outside root, confirmed restore, and mid-batch rollback; full suite 257/257; build 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | N/A | No UI exposure. | Recovery WPF visual gate remains Warn. |
| Operations, dependencies, and release | Pass | Pipeline blocks unconfirmed preview; confirmed tests use isolated temp roots and restore; no residue remains. | No real user archive or permanent purge occurred. |

### 2026-07-10 - Unregistered official-uninstaller handler

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Handler revalidates descriptor flags, hash/id/age/schema, recovery/backup, manifest command equality, file existence, and command trust; tampered snapshot/arguments never call launcher. | Elevated handler conservatively blocks external signed uninstallers until it can repeat signature verification. |
| Data, API, and consistency | Pass | Typed launch/post-scan/payload models distinguish not-started, nonzero exit, completed+scan, and completed+scan-failed states; timeline is not restorable. | No claim that quarantine can undo official uninstall. |
| Code quality and maintainability | Pass | Launcher and post-scan are interfaces; handler has no process API; Program/App have no registration. | Real adapters remain separate future work. |
| Testing and verification | Pass | TDD red/green; focused handler tests 7/7; full suite 264/264; build 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | N/A | Handler is unreachable from WPF. | Recovery panel visual gate remains Warn. |
| Operations, dependencies, and release | Pass | Tests use fake adapters and text fixture; no process or temp root remains; source search found no registration. | Do not ship/register until independent launcher/UI gates pass. |

### 2026-07-10 - Unregistered Windows uninstaller launcher adapter

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Exact executable/arguments only; shell execute and `runas` are explicit; no shell wrapper construction; UAC/app cancellation are distinct. | Handler still performs command trust before launcher. |
| Data, API, and consistency | Pass | Exit code and user-cancel status map into existing typed launch result. | No stdout/stderr capture because shell execute/elevated interactive uninstallers are expected. |
| Code quality and maintainability | Pass | Process API isolated in `SystemProcessRunner`; launcher depends on interface; Program/App have no registration. | Real runner remains unreachable. |
| Testing and verification | Pass | TDD red/green; focused 6/6; full suite 270/270; build 0 warnings/errors. | Tests inject fake runner and launch no process. |
| Frontend, accessibility, and UX | N/A | No UI exposure. | Recovery panel screenshot still pending. |
| Operations, dependencies, and release | Pass | Process check empty; temp root check empty; registration search empty. | Compiled capability must remain unregistered until final gates. |

### 2026-07-10 - Unregistered real post-uninstall scan adapter

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Only fresh path probes create residue candidates; historical background identifiers remain unverified counts; beginner summary hides raw paths. | No cleanup, quarantine, timeline, process, or pipeline API in adapter. |
| Data, API, and consistency | Pass | Typed result carries software-presence, residue count/report, background-rescan need, and explicit failure. | Manifest/software mismatch is refused before inventory access. |
| Code quality and maintainability | Pass | Adapter reuses `UninstallResidueScanBuilder` and injected inventory/path/size functions; App/Program registration absent. | Specialized service/startup/task rescans remain future work. |
| Testing and verification | Pass | Focused adapter 6/6; related uninstall 23/23; full suite 276/276; build 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | Warn | Summary is beginner-safe, but no WPF result view exists and updated recovery panel lacks fresh screenshot. | Do not wire execution before screenshot and confirmation-flow review. |
| Operations, dependencies, and release | Pass | Process and temp checks empty; source contract proves read-only/unregistered boundary. | No real uninstaller or scanner registration occurred. |

### 2026-07-10 - Beginner post-uninstall result presentation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Visible text ignores raw summaries and tests reject raw paths/background identifiers. | Technical details are not yet exposed. |
| Data, API, and consistency | Pass | Four typed states map failure, app presence, clean scan, and review-needed outcomes without overstating rollback. | Counts are informational only. |
| Code quality and maintainability | Pass | Pure presenter/view model; static test excludes operation, pipeline, process, quarantine, delete, and move APIs. | Future WPF can bind without acquiring execution authority. |
| Testing and verification | Pass | Focused 5/5; product/uninstall 178/178; full suite 281/281; build 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | Warn | Copy and hierarchy are product-tested, but no WPF rendering or screenshot exists. | Recovery-panel screenshot remains the prerequisite for UI wiring. |
| Operations, dependencies, and release | Pass | No process/temp residue and no registration/mutation. | No real uninstall occurred. |

### 2026-07-10 - Fresh background residue re-enumeration

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Exact manifest identifiers only; invalid service/task names, traversal, and reparse points return Unknown; visible text exposes counts only. | Registry and task identifiers remain technical evidence. |
| Data, API, and consistency | Pass | Exists/Missing/Unknown is preserved; Unknown fails mandatory background completion; verified matches map to high-risk report groups. | Access failure cannot become a clean claim. |
| Code quality and maintainability | Pass | Reader interface, pure scanner, isolated system reader, and optional post-scan composition keep boundaries testable. | Real composition remains intentionally absent. |
| Testing and verification | Pass | Focused 12/12; product/uninstall 185/185; full suite 288/288; build 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | Warn | Presenter clearly says background records are not directly closed, but no WPF/screenshot proof exists. | Recovery-panel visual gate remains open. |
| Operations, dependencies, and release | Pass | No processes, temp items, registration matches, mutation calls, or real Windows probes executed in tests. | All high-risk runtime pieces remain unreachable. |

### 2026-07-11 - Elevated request/response boundary and recovery GUI reliability

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Request requires fresh visual receipt, exact consent, manual high-risk flags, immutable descriptor copy/hash, and response correlation; visible response ignores raw errors/paths. | Runtime receipt issuance is not yet implemented, so execution remains unregistered. |
| Data, API, and consistency | Pass | Restore-point states preserve Completed/TimedOut/Failed; timeout is not converted to “none.” Descriptor arguments are deep-copied and fingerprinted. | Restore points remain fallback hints only. |
| Code quality and maintainability | Pass | Request/response boundary is isolated and static-tested for no execution/mutation path; WPF native fallback is centralized in the shared helper. | Temporary lifecycle diagnostics were removed from product code. |
| Testing and verification | Pass | TDD RED/GREEN; boundary 7/7; related official uninstall 38/38; final full suite 298/298; solution build 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | Pass | Real smoke at original 10-second gate: modal found/closed, 3 protection lines, 3 steps, recovery sections visible, technical details collapsed, no execution control; `.omx/qa-uninstall-plan-window.png` inspected. | Final-confirmation checklist and post-scan result still need their own UI proof. |
| Operations, dependencies, and release | Pass | No Css/OMNIX process, uninstall temp item, or App/Program registration match remained. No real installer/uninstaller ran. | `.omx` QA packaging exclusion remains future release work. |

### 2026-07-11 - WPF final-confirmation checklist

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Incomplete preparation returns missing requirements without creating the isolated evidence root; UI has no run control, pipeline, handler, or execution call. | Complete preparation writes only the existing hashed audit manifest. |
| Data, API, and consistency | Pass | WPF uses the tested draft service and process-scoped evidence-root resolver; ready/pending/missing collections remain distinct. | Beginner panel does not display manifest paths. |
| Code quality and maintainability | Pass | Stable AutomationIds on interactive/text/list peers; checklist appears before technical expander; stale checklist resets when preparation changes. | App build verifies XAML/code-behind integration. |
| Testing and verification | Pass | TDD RED/GREEN for resolver, WPF contract, and smoke contract; full suite 300/300; solution build 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | Warn | Real automation reached visible checklist, found missing items, no evidence-root write, and correct safety text. Diagnostic screenshot had black composition blocks; corrected rerun was rejected by GUI usage limit. | Must rerun unchanged and inspect a clean screenshot before advancing execution reachability. |
| Operations, dependencies, and release | Warn | No processes/temp/evidence roots remain, but a rejected diagnostic screenshot may remain because cleanup was denied after quota exhaustion. | `.omx` remains excluded from release work in a later packaging slice. |

### 2026-07-11 - Post-scan WPF and one-time visual receipt

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Result UI excludes paths/identifiers and execution APIs; PNG evidence is hashed in memory and image bytes are not retained; ticket is ten-minute single-use. | Process-local ticket is not claimed as hostile-process protection; authenticated IPC remains required. |
| Data, API, and consistency | Pass | Four typed post-scan states share one Core view model; receipt binds UI contract, SHA-256, capture time, and four visible-state flags; request session consumes before compose. | Failed composition burns the ticket conservatively and requires a fresh confirmation. |
| Code quality and maintainability | Pass | Css.App does not reference Css.Elevated; DEBUG fixture is compile-guarded; issuer/session are isolated and static-audited for no persistence/execution/mutation APIs. | Real runtime components remain separately searchable and unregistered. |
| Testing and verification | Pass | Focused post-scan tests 8/8; receipt/session tests 7/7; final full suite 309/309; solution build 0 warnings/errors; static registration/mutation audits pass. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | Pass | Final checklist smoke shows status and missing requirements; post-scan smoke shows title/status/conclusion/3 facts/Agent advice/safety line. Both screenshots inspected; no execution controls. | Technical identifiers remain hidden from the beginner result. |
| Operations, dependencies, and release | Pass | No Css.App/Css.Elevated process or temporary uninstall evidence root remained. DEBUG smoke argument is absent from release compilation. | No installer, uninstaller, service, registry, task, or user file was changed. |

### 2026-07-11 - Final consent and authenticated fake transport

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Final consent requires all three acknowledgements; transport uses HMAC-SHA256/fixed-time compare, freshness, replay tables, descriptor recomputation, and response correlation. | In-memory key is test/fake transport only; Windows process identity is not yet verified. |
| Data, API, and consistency | Pass | Consent and safe response models live in Core; exact confirmation text/time flows into the request; cancellation propagates; wrong/stale/tampered/replayed/mismatched messages have distinct statuses. | Real serialized schema and named-pipe framing remain future work. |
| Code quality and maintainability | Pass | Css.App has no Css.Elevated project reference; WPF windows contain no execution APIs; transport/issuer/session are isolated and unregistered; DEBUG flow is compile-guarded. | Elevated Program remains an empty placeholder. |
| Testing and verification | Pass | Consent 7/7, WPF contracts 2/2, transport 7/7, fake-launcher integration 1/1, full suite 326/326; Debug and Release builds 0 warnings/errors. | Release DLL binary scan found no smoke arguments. |
| Frontend, accessibility, and UX | Pass | Real GUI: confirm initially disabled, enabled after exactly three checks, then fake result visible; both screenshots inspected and clean. | Product flow is not yet reachable outside DEBUG. |
| Operations, dependencies, and release | Pass | Registration/reference/mutation audits passed; no Css.App/Css.Elevated process or handler temp root remained. | No real installer, uninstaller, process launch, registry, service, task, or user-file mutation occurred. |

### 2026-07-12 - Bounded serialized fake named-pipe transport

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `CurrentUserOnly`; OS-derived SID/PID/session checks; request HMAC, descriptor recomputation, replay protection; response HMAC/correlation; response JSON excludes injected private path/summary. | Session-key exchange and production elevated-process launch are intentionally absent. |
| Data, API, and consistency | Pass | 64 KiB length prefix; strict schema/message type; JSON rejects unknown members; only string/bool official-uninstall arguments round-trip; typed response fields reconstruct the existing handler payload. | Client/codec currently remain in Elevated and require neutral-library extraction before App use. |
| Code quality and maintainability | Pass | Codec, framing, OS identity reader, fake client/server, and endpoint authentication remain separate; timeout/identity helpers are centralized. | The one-shot server is deliberately not a production worker loop. |
| Testing and verification | Pass | Focused 14/14, including live Windows SID/PID/session; full suite 340/340; Debug and Release builds 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | N/A | Backend-only slice; no visible UI changed. | DEBUG WPF-to-fake-pipe proof is the next slice. |
| Operations, dependencies, and release | Pass | App has no Elevated reference; Program remains placeholder; source/release audits found no registration, process launch, mutation API, pipeline call, or smoke string; no App/Elevated process remained. | No real installer, uninstaller, service, registry, task, or user file was changed. |

### 2026-07-12 - Neutral IPC library and DEBUG WPF pipe flow

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | App csproj references Ipc, not Elevated; Ipc csproj references Core only; Ipc source has no handler/launcher/pipeline/process-start/file-mutation/registry/service authority; response fixture private path is absent from decoded visible facts. | Cross-process key establishment and runtime visual evidence remain unimplemented. |
| Data, API, and consistency | Pass | Core owns request/result/presenter contracts and descriptor hashing; both App and Ipc use the same types. GUI result contains exactly two typed pipe facts, excluding the fixed fallback (three) and failure state (one). | Fake endpoint is same-process and DEBUG-only. |
| Code quality and maintainability | Pass | Dependency graph is Core <- Ipc <- App/Elevated; no duplicate hash/DTO implementation; moved-source static tests use authoritative paths. | Type names retain `Elevated` wording for compatibility although pure contracts now live in Core. |
| Testing and verification | Pass | Related 50/50; full 340/340; Debug/Release 0 warnings/errors; GUI JSON reports all true plus `pipeResultFactCount=2`; both screenshots inspected and clean. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | Pass | Final consent shows three acknowledgements and disabled-to-enabled action; result first view shows two plain facts, Computer Agent advice, and no-further-mutation safety line. | DEBUG fixture only; production route remains absent. |
| Operations, dependencies, and release | Pass | Release App binary contains no smoke argument/method/fixture strings; Elevated Program remains placeholder; no App/Elevated process remained. | No real uninstaller or system mutation occurred. |

### 2026-07-12 - Identity-bound ephemeral IPC session bootstrap

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | ECDH P-256, fresh nonces, transcript-bound HMAC-SHA256 derivation, fixed-time two-way finished verification, replay guard, and zeroization. Static scan excludes command-line/environment/file secret transfer and execution/mutation authority. | Must be invoked only after actual pipe SID/PID/session checks; separate-process composition is next. |
| Data, API, and consistency | Pass | Strict JSON rejects unknown/malformed/schema-invalid payloads; hello nonce is exactly 32 bytes; public keys are bounded/import-validated; session keys are exactly 32 bytes and expose transcript hash/session id. | Client nonce replay is conservatively process-global within one guard retention window. |
| Code quality and maintainability | Pass | Bootstrap codec, replay guard, key owner, client/server sequence, and crypto helpers are isolated in neutral Ipc. Authenticated client/endpoint implement IDisposable and zero their copies. | Type is intentionally unregistered and has no worker-loop responsibility. |
| Testing and verification | Pass | Bootstrap 7/7; related 15/15; full 348/348; Debug/Release 0 warnings/errors. Live pipe, mismatch, tamper, replay, malformed/oversized/schema, timeout/cancel, disposal covered. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | N/A | No visible UI changed. | Existing DEBUG pipe GUI remains unchanged. |
| Operations, dependencies, and release | Pass | Elevated Program unchanged; Release App has no bootstrap strings; no App/Elevated process remained. | No process, installer, uninstaller, registry, service, task, or user file was changed. |

### 2026-07-12 - Separate-process authenticated smoke worker

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Worker accepts only pipe/session/client identity/timeout metadata; real peer SID/PID/session is checked before transcript-bound ECDH; key owners and exported copies are zeroed. Static audit found no secret argument/environment/file channel. | Smoke worker is non-elevated and development-only. |
| Data, API, and consistency | Pass | One strict request produces one correlated typed response and camel-case receipt; response remains path-free and reports two fake residue candidates. | No real scan or uninstall claim is made. |
| Code quality and maintainability | Pass | `Css.SmokeTools` references neutral Ipc; process start exists only in a test-injected adapter; `Css.Elevated/Program.cs` is unchanged. | Release packaging must continue to classify SmokeTools as development tooling. |
| Testing and verification | Pass | Debug worker 4/4, Release worker 4/4, related 33/33, full suite 352/352, Debug/Release builds 0 warnings/errors. | Success, startup timeout, forced disposal, exact identity, response and receipt are covered. |
| Frontend, accessibility, and UX | N/A | No user-visible UI changed. | Runtime final-confirmation visual receipt is next. |
| Operations, dependencies, and release | Pass | Release App binary lacks worker/SmokeTools/process-start strings; Elevated Program remains placeholder; process audit is empty. | No installer, uninstaller, service, registry, task, or user file was changed. |

### 2026-07-12 - Runtime final-consent visual receipt

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | App renders its own content; no desktop screen copy or file persistence; nonblank pixel check; PNG bytes zeroed after issuer hashes them; one-time ticket consumption. | Core stores only SHA-256/state/time, not PNG bytes. |
| Data, API, and consistency | Pass | Receipt/session moved to Core; fixed hash removed; exact consent and ticket create the draft; WPF test proves hash equality and all four flags. | App still has no Elevated project reference. |
| Code quality and maintainability | Pass | WPF capture is behind an interface; pure issuer/session have no WPF/execution dependency; old Elevated source files are absent. | Tests explicitly preserve standard SDK usings after enabling WPF. |
| Testing and verification | Pass | Runtime WPF 2/2; related 25/25; full 354/354; Release combined 6/6; Debug/Release builds 0 warnings/errors. | Ownership, authority, persistence, release strings, and process checks passed. |
| Frontend, accessibility, and UX | Warn | Existing AutomationIds and smoke contract cover all visible controls; real WPF render is nonblank and viewport-checked. Computer Use could not launch the DEBUG exe with arguments, so no fresh external screenshot was captured. | Do not treat this slice as the final visual gate until the unchanged smoke runs and screenshots are inspected. |
| Operations, dependencies, and release | Pass | Release excludes DEBUG smoke argument; capture itself is non-executable; Elevated Program remains placeholder; no process remained. | No installer, uninstaller, registry, service, task, or user file changed. |

### 2026-07-12 - Render evidence and production final-consent request entry

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Continue only for Ready verified draft; app/tray closure is explicit; recovery signature, uninstaller, actual manifest SHA-256, exact consent, and visual ticket are revalidated; refused attempts consume the ticket. | Request remains in memory and cannot execute. |
| Data, API, and consistency | Pass | Core preparation service combines gate plus one-time session; four behavior tests cover ready, replay, missing app closure, and hash change. | WPF stores only a typed draft. |
| Code quality and maintainability | Pass | WPF gathers evidence; Core owns pure preparation; capture uses Render flush plus normalized VisualBrush; test-only export is `.omx`-restricted. | No App-to-Elevated reference. |
| Testing and verification | Pass | Plan WPF 2/2; preparation 4/4; uninstall 121/121; full 362/362; Release high-risk 12/12; Debug/Release 0 warnings/errors. | Fresh current-workspace evidence. |
| Frontend, accessibility, and UX | Pass | Fresh 171,609-byte render inspected at original detail after two visual fixes; all three impacts/acknowledgements, readiness, safety, and both buttons are visible without crop/overlap. Stable AutomationIds and Ready/Refused visibility tests pass. | Regenerate artifact after future copy/layout changes. |
| Operations, dependencies, and release | Pass | Release contains production Continue/preparation service but no DEBUG smoke argument; Program remains placeholder; process audit empty. | No installer, uninstaller, registry, service, task, or user file changed. |

### 2026-07-12 - Production fake elevated worker lifecycle

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Exact launched PID/current SID/Windows session is compared with the OS-derived pipe server before ECDH; key material is created inside the pipe and zeroed. Static audit found no secret argument/environment/file channel. | Client identity metadata is non-secret and still rechecked from the connected pipe on the worker side. |
| Data, API, and consistency | Pass | UAC cancel, launch failure, peer rejection, bootstrap failure, response timeout, transport failure, cleanup failure, and fake completion are distinct typed states. Fake payload explicitly reports `UninstallerStarted=false`. | No fake result is exposed as a real uninstall result in WPF. |
| Code quality and maintainability | Pass | App owns injected process launch, Ipc owns neutral lifecycle/server mechanics, Elevated owns fake mode only. Program does not register the real handler/launcher/scanner. | The existing SmokeTools implementation remains separate and may be consolidated later. |
| Testing and verification | Pass | Lifecycle 7/7 Debug and Release; related 101/101; full 369/369; Debug/Release builds 0 warnings/errors. Wrong PID, session mismatch, timeout, and forced tree termination use real child processes. | Interactive secure-desktop UAC was not automated. |
| Frontend, accessibility, and UX | N/A | No visible WPF route changed. | A beginner presenter is the next UI-adjacent slice. |
| Operations, dependencies, and release | Warn | Static no-authority/no-secret audits and empty process audit passed; App is still disconnected. | Actual `runas` cancel/accept and packaged Elevated executable discovery require manual/release smoke before registration. |

### 2026-07-12 - Beginner worker result and build packaging

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Presenter tests reject paths/PID/ECDH/raw statuses; resolver accepts only exact non-reparse sibling; App deps has no Elevated; fake worker returns `UninstallerStarted=false`. | Production signing/publisher trust is not yet implemented. |
| Data, API, and consistency | Pass | Ten lifecycle statuses map deterministically to title/status/conclusion/advice/safety; packaging copies exactly four same-configuration files. | Availability means development verification ready, not production trust. |
| Code quality and maintainability | Pass | No App `ProjectReference` to Elevated; build and publish targets are explicit; window is non-executable; DEBUG orchestration is isolated under `#if DEBUG`. | Nested MSBuild may log Elevated twice in solution builds but remains incremental and correct. |
| Testing and verification | Pass | Presentation 15/15, impacted 188/188, Release presentation+lifecycle 22/22, full 384/384, Debug/Release builds 0 warnings/errors. Publish artifact/deps/Release-string audits pass. | Actual secure-desktop choices remain manual. |
| Frontend, accessibility, and UX | Pass | Stable AutomationIds/order tests plus inspected 680x430 PNG show all beginner conclusions, Agent advice, safety text, and close action in the first view without overlap. | Screenshot is a test-render artifact, not an actual UAC smoke screenshot. |
| Operations, dependencies, and release | Warn | Release publish contains all four worker files, App deps excludes Elevated, DEBUG smoke string is absent, process audit empty. | Run real Accept and Cancel smokes and verify code signing before production registration. |

### 2026-07-12 - Signed worker production trust gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | WinVerifyTrust cached full-chain/strong flags; signer data read only after trust; exact thumbprint comparison; fixed-time worker hash recheck; every non-trusted state fails closed. | A residual file-swap race after pre-launch hash and before Windows image creation remains for the future production launcher hardening. |
| Data, API, and consistency | Pass | Separate `CanLaunchProduction` and `CanLaunchDevelopmentVerification`; current unsigned pair gets only the latter; same subject/different thumbprint is rejected. | Embedded signature is intentionally required; catalog-only trust is not accepted for OMNIX package files. |
| Code quality and maintainability | Pass | Native trust lives in Css.Win32, pure policy/presenter in App, launcher owns final pre-start hash; no handler/pipeline/registry/service/file mutation dependency. | The legacy subject-only inventory helper remains for display/inventory use, not authorization. |
| Testing and verification | Pass | Trust 12/12, impacted 185/185, Release combined 34/34, full 396/396, Debug/Release builds 0 warnings/errors. Real signed/tampered files and current unsigned pair covered. | A real OMNIX signed package cannot be tested until release signing infrastructure exists. |
| Frontend, accessibility, and UX | Pass | Reused stable result window; fresh 680x430 render inspected with development-only title, status, Agent advice, safety, and close action all visible and non-overlapping. | No production button displays this state yet. |
| Operations, dependencies, and release | Warn | Current Release files are explicitly NotSigned and therefore production-blocked; no production route, DEBUG string, real authority registration, temp file, or process remains. | Configure code signing and run actual UAC Accept/Cancel before real registration. |

### 2026-07-12 - Post-start worker image correlation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Lifecycle requires expected evidence, independently queries the started PID image with limited rights, compares normalized path and SHA-256 in fixed time before `ExchangeAsync`, and cleans the child tree on every rejection. | Signed installation-directory ACL policy is still required for release hardening. |
| Data, API, and consistency | Pass | `WorkerImageRejected` is distinct; missing, path mismatch, hash mismatch, and inspection failure all fail closed; cancellation remains distinct. | Launchers must now provide image expectation on `Started`. |
| Code quality and maintainability | Pass | Neutral contracts/orchestration remain in Ipc; Windows process querying remains App-owned; injectable inspector keeps tests deterministic. | No App-to-Elevated assembly dependency was introduced. |
| Testing and verification | Pass | Debug focused 28/28; Release trust/lifecycle/presentation 40/40; full 402/402; Debug/Release builds 0 warnings/errors; authority/order/package/process audits pass. | Manual secure-desktop UAC remains separate. |
| Frontend, accessibility, and UX | Pass | Existing stable AutomationIds/order tests plus inspected `.omx/qa-runtime-worker-image-rejected.png` show the path-free Agent conclusion and safety statement in the first 680x430 view. | No retry or execution action is offered. |
| Operations, dependencies, and release | Warn | Production WPF remains disconnected, Release package files exist, no process remains, and current unsigned binaries remain production-blocked. | Real signed-package and UAC evidence are not yet available. |

### 2026-07-12 - Mandatory post-scan after started uninstaller

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Any started launcher with an exit code reaches `_postScanner.ScanAsync` before non-zero handling; not-started performs no scan; no mutation APIs exist in handler/scanner. | Findings remain candidates only. |
| Data, API, and consistency | Pass | Non-zero remains unsuccessful, carries scan result, and sets retry only when scan failed; caller cancellation propagates. | `UninstallerCompleted=false` remains truthful for non-zero. |
| Code quality and maintainability | Pass | One post-scan path serves zero/non-zero outcomes and timeline summaries distinguish them. | Production composition remains separate. |
| Testing and verification | Pass | Handler 11/11; uninstall subsystem 35/35 Debug and Release; full 405/405. | Fake launcher/scanner only. |
| Frontend, accessibility, and UX | N/A | No visible UI changed. | Existing typed presenter consumes the richer payload. |
| Operations, dependencies, and release | Pass | Program unregistered; static mutation audit empty; process audit empty. | No real uninstaller or UAC prompt ran. |

### 2026-07-12 - Elevated package authorization before bootstrap

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Actual pipe peer is read/required before authorization; authorization precedes ECDH; both process images require Windows trust and exact certificate-thumbprint equality. | Current unsigned package is denied. |
| Data, API, and consistency | Pass | `AuthorizationFailed` is distinct; denial returns no operation response and invokes no handler; production session recomputes descriptor SHA-256. | Request preparation age is the next gate. |
| Code quality and maintainability | Pass | Native process resolution lives in Win32; Ipc owns generic hook; Elevated owns package trust/session; App remains independent of Elevated. | Program registration deliberately absent. |
| Testing and verification | Pass | Focused 35/35 Debug and Release; full 417/417; Debug/Release builds 0 warnings/errors; static order/no-mutation/process audits pass. | Positive native signed OMNIX package unavailable; injected trusted evidence covers success. |
| Frontend, accessibility, and UX | N/A | No visible UI changed. | Authorization denial will surface through existing safe-stop lifecycle result after future registration. |
| Operations, dependencies, and release | Warn | Program and WPF remain disconnected; current unsigned Release is blocked; no process remains. | Signed package and manual UAC evidence still required. |

### 2026-07-12 - Authenticated request preparation freshness

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Verified final consent time is required, HMAC-bound, schema-v2 serialized, and rejected past 15 minutes or beyond 30-second future skew before handler. | Endpoint and Elevated session both enforce. |
| Data, API, and consistency | Pass | Ready/refused drafts remain distinct; `CanSubmit` rejects missing/default time; wire round-trip preserves exact UTC time; tampering fails authentication. | v1 is intentionally incompatible. |
| Code quality and maintainability | Pass | Time authority originates in Core composer, transport owns wire/auth checks, Elevated owns defense-in-depth. | Direct smoke drafts explicitly supply current time. |
| Testing and verification | Pass | Focused 47/47 Debug; Release high-risk 73/73; full 421/421; Debug/Release 0 warnings/errors. Stale, future, tampered, valid, serialized, worker, and session flows covered. | One unrelated fail-closed status fluctuation passed 10 repeated isolated runs and the repeated full suite. |
| Frontend, accessibility, and UX | N/A | No visible UI changed. | Expired requests will require a fresh confirmation in future production wiring. |
| Operations, dependencies, and release | Warn | Program/WPF production mode remains disconnected; no process remains. | Real mode composition, signed package, and UAC evidence remain pending. |

### 2026-07-12 - Self-denying production worker command mode

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Real mode order is actual peer -> same trusted signer -> ECDH -> authenticated fresh request -> SafetyOperationPipeline -> verified manifest -> official launcher -> read-only post-scan. Current unsigned real process self-denies before bootstrap. | Positive native execution requires signed package. |
| Data, API, and consistency | Pass | Production parser accepts exactly six bounded metadata pairs and rejects fake options; scanner factory receives exact validated manifest; non-zero exit still scans. | Worker returns only typed one-shot transport/result. |
| Code quality and maintainability | Pass | Shared parser; minimal Elevated inventory reader; no Css.Scanner dependency; real `Process.Start` isolated in SystemProcessRunner; no nested `runas` request from already elevated handler. | App remains free of Elevated reference. |
| Testing and verification | Pass | Focused 57/57 Debug/Release; full 427/427; Debug/Release builds 0 warnings/errors. Real unsigned child denial, fake-option rejection, exact-manifest binding, and static authority order covered. | No real uninstaller fixture was executed. |
| Frontend, accessibility, and UX | N/A | No visible UI changed; App source/binary has no production mode/session. | Production result presentation is next. |
| Operations, dependencies, and release | Warn | Elevated binary contains production mode/session; App binary does not; Elevated deps excludes Css.Scanner; mutation audit empty; no process remains. | Manual UAC and same-cert signed package are still prerequisites for WPF wiring. |

### 2026-07-13 - Trusted App production lifecycle and result

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Production lifecycle requires the marker launcher; Windows production factory requires `CanLaunchProduction` and exact worker hash; wrong launcher type stops before launch. | Current unsigned package still cannot construct the production launcher. |
| Data, API, and consistency | Pass | Fake and production completion are distinct; typed payload truth covers not-started, incomplete, post-scan failure, still-present, residue, and clean states. | WPF coordinator is the next boundary. |
| Code quality and maintainability | Pass | Ipc owns authority-neutral marker/status; App owns Windows trust factory and presentation; internal argument builder was not widened for tests. | No Elevated project reference added to App. |
| Testing and verification | Pass | Focused 54/54 Debug/Release; full 436/436; Debug/Release builds 0 warnings/errors; static secret/fake-switch and process audits pass. | Positive native signed package remains unavailable. |
| Frontend, accessibility, and UX | Pass | Stable existing AutomationIds/order test; inspected 680x430 production result screenshot with all conclusions/actions visible and no path leakage. | Screenshot: `.omx/qa-runtime-production-worker-result.png`. |
| Operations, dependencies, and release | Warn | No process remains; current unsigned package remains production-blocked. | Manual secure-desktop UAC and signed release package are still release gates. |

### 2026-07-13 - WPF production execution coordinator

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Unsigned trust creates no runner/UAC; trusted path uses the production factory; WPF contains no launcher/mode/lifecycle/process/pipeline/handler authority. | Current package remains fail-closed before UAC. |
| Data, API, and consistency | Pass | Only request-correlated `CompletedProduction` becomes an elevated response/post-scan view; failures retain lifecycle summary only. | Prepared request remains one-shot and memory-only. |
| Code quality and maintainability | Pass | Coordinator and runner are injected/testable; MainWindow composes current package; WPF remains presentation/orchestration only. | No App-to-Elevated project reference. |
| Testing and verification | Pass | Focused 43/43, bootstrap/coordinator 11/11, two full 440/440 runs, Release critical 67/67, Debug/Release builds 0 warnings/errors. | Finished tamper recurrence was fixed semantically and retested. |
| Frontend, accessibility, and UX | Pass | Trust/lifecycle/post-scan use existing stable AutomationId result windows and inspected beginner presentation. | Native signed result screenshot remains a release gate. |
| Operations, dependencies, and release | Warn | Static WPF audit passes; no Css/OMNIX process remains. | Same-cert signing and manual secure-desktop UAC Accept/Cancel still required before release. |

### 2026-07-13 - Production residue review linkage

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Wire schema contains no `ResidueReport`; local captured profile plus refreshed inventory produces path evidence; only confirmed low-risk operation reaches pipeline. | High-risk background groups stay read-only. |
| Data, API, and consistency | Pass | Review is requested only after completed production/post-scan; exact pre-uninstall profile survives tile removal; one refreshed inventory is reused. | Count-only IPC cannot directly create a mutation. |
| Code quality and maintainability | Pass | Selected-app wrapper delegates to shared correlated review method; existing quarantine/timeline/restore components are reused. | No duplicate residue execution path. |
| Testing and verification | Pass | Focused 29/29 Debug and 31/31 Release; full 441/441; Debug/Release builds 0 warnings/errors; static wire/pipeline/process audits pass. | Native signed uninstall remains externally gated. |
| Frontend, accessibility, and UX | Pass | Catalog refresh occurs before inline review, preserving the existing stable-AutomationId Agent conclusion; prior residue screenshots remain representative because layout did not change. | No new panel/layout was introduced. |
| Operations, dependencies, and release | Warn | No process remains and no new elevated dependency exists. | Signed package/UAC evidence still required for native positive path. |

### 2026-07-13 - Migration engine and closure monitor

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | 1 MiB bounded/fixed-time hashed manifest; exact operation correlation; active/path-policy denial before adapter; protected Windows roots blocked. | WPF remains preview-only. |
| Data, API, and consistency | Pass | Typed completed/refused/rolled-back/incomplete states; reverse rollback; monitoring record binds snapshot, manifest hash, original and expected target. | Actual snapshot store integration remains later. |
| Code quality and maintainability | Pass | Core coordinator depends on activity/path/policy/store interfaces; Windows mechanics are not embedded in plan/UI. | Atomic JSON rename is the only concrete `File.Move`. |
| Testing and verification | Pass | Focused 24/24 Debug/Release; full 449/449; Debug/Release 0 warnings/errors; no process/temp residue. | Success, tamper, active denial, stale evidence, unsafe policy, rollback, incomplete rollback, persistence and write-return covered. |
| Frontend, accessibility, and UX | N/A | No visible UI changed and MigrationPlanWindow remains preview-only. | Result presenter/window screenshot is next. |
| Operations, dependencies, and release | Warn | No real path adapter or WPF execution route exists. | Manual cross-volume rollback/redirect evidence required before enabling user paths. |

### 2026-07-13 - Windows directory adapter and migration result UI

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Reparse entries rejected; bounded traversal; exact inventory/size/fixed-time SHA-256; destination collision precheck; no shell/process command. | Real adapter is not reachable from WPF. |
| Data, API, and consistency | Pass | Staging -> final commit -> verify -> source remove -> redirect -> re-observe; rollback restores and verifies source before destination removal. | Redirect primitive is injected. |
| Code quality and maintainability | Pass | Copy verifier, redirect primitive and migration adapter are distinct; result presenter ignores raw errors/paths. | Css.Win32 already depends inward on Core. |
| Testing and verification | Pass | Focused 19/19 Debug/Release; full 460/460; Debug/Release 0 warnings/errors; nested/tamper/collision/redirect-failure/rollback/staging covered. | Native symbolic-link positive path is not automated without elevation. |
| Frontend, accessibility, and UX | Pass | Stable AutomationIds/order test; inspected `.omx/qa-runtime-migration-result.png`, all conclusions visible, no technical leakage. | MigrationPlanWindow remains preview-only. |
| Operations, dependencies, and release | Warn | Normal-process link probe denied; no process/temp residue remains. | Signed elevated worker and manual fixture rollback/UAC evidence required. |

### 2026-07-13 - Antivirus alert on generated test assembly

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Fail | User-exported Huorong log reports `Trojan/ShellLoader.gx` on Debug `Css.Tests.dll` in both `obj` and `bin`; artifact was deleted repeatedly. | No restore, whitelist, or bypass is permitted before independent verification. |
| Data, API, and consistency | Warn | Static audit found no injection/download primitives; generated production assemblies were not named in the supplied log. | This supports but does not prove a false-positive diagnosis. |
| Code quality and maintainability | Warn | Test assembly combines process/UAC/pipe integration coverage with literal hostile shell-command rejection fixtures. | Isolate these transparently if vendor verification confirms a heuristic collision. |
| Testing and verification | Fail | Focused migration protocol tests reported 8/8, but the generated test artifact is not security-accepted. | Do not use green tests as release evidence until the alert is resolved. |
| Frontend, accessibility, and UX | N/A | No user UI was executed or changed during alert triage. | Development execution is paused. |
| Operations, dependencies, and release | Fail | Compilation repeatedly recreates the flagged artifact. Process audit is empty after stopping. | Vendor sample review or independent scanner evidence is required before builds resume. |

### 2026-07-13 - Vendor false-positive confirmation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Warn | User relayed Huorong's explicit sample-analysis result: confirmed false positive. | Wait for corrected definitions locally; do not whitelist. |
| Testing and verification | Warn | Earlier focused protocol run passed 8/8. | One clean rebuild and focused rerun under corrected definitions are still required. |
| Operations, dependencies, and release | Warn | Process audit is empty; builds remain paused. | Promote to Pass only when rebuilt `Css.Tests.dll` remains present without a fresh alert. |

### 2026-07-13 - Source-only migration closure while definitions are pending

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Warn | Static audit: snapshot/manifest hashes, current-source recheck, trust gate, response correlation, no WPF mutation authority. | New source is uncompiled; corrected local Huorong definitions still required. |
| Data, API, and consistency | Warn | Snapshot id/path/hash flow from MainWindow through descriptor and handler; completion requires successful correlated `Completed` payload. | Runtime serialization and fixture execution remain deferred. |
| Code quality and maintainability | Warn | Core evidence reader, Elevated composition, App coordinator, and WPF consent are separate boundaries; stale authority test corrected. | Compiler and analyzer evidence unavailable during the security pause. |
| Testing and verification | Fail | New focused tests exist only as source. | Must build, scan artifact, and run focused/full suites after definition update. |
| Frontend, accessibility, and UX | Warn | XAML parses; stable AutomationIds and static order/authority tests were added; visible copy is path-free. | Real first-view screenshot and UIAutomation smoke are still required. |
| Operations, dependencies, and release | Warn | MainWindow keeps migration `FeatureEnabled=false`; no Worker/UAC/real paths were used. | Do not enable production migration before clean security/build/test/manual fixture evidence. |

### 2026-07-13 - Source-only growth history and home Agent linkage

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Scoped source scan found no new process launch, elevation, registry, file move/delete, real user path, or direct execution authority. Growth/home presenters remain path-free and `CanExecuteDirectly=false`. | Existing unrelated Windows settings/tool shortcuts in MainWindow were excluded from the focused new-code scan. |
| Data, API, and consistency | Warn | Source invariants show a 2,048-item snapshot bound, independent payload validation, 90-snapshot per-root retention, foreign keys, indexed item lookup, latest-eight load, and transaction-local trimming. | SQLite behavior and cascade/order remain unexecuted until corrected definitions permit tests. |
| Code quality and maintainability | Warn | Snapshot construction, trend analysis, persistence, presentation, health summary, and WPF wiring remain separate; explicit collection types remove target-typing ambiguity. | Compiler/type/analyzer evidence is unavailable. The attempted Roslyn loader check was invalidated and recorded. |
| Testing and verification | Fail | Focused test source covers bounds, attribution, trend thresholds, history integration, retention, oversized refusal, home explanation, and static UI order. Static XAML/invariant scans pass. | No test was run and no assembly was generated in accordance with the antivirus pause. |
| Frontend, accessibility, and UX | Warn | Home and C-drive Agent conclusions have stable AutomationIds and are statically ordered before their lists; MainWindow XAML parses. | Real UIAutomation and first-visible-area screenshots are mandatory after the virus-definition update. |
| Operations, dependencies, and release | Warn | Installer launch remains explicitly disabled; no GUI, Worker, UAC, installer, cleanup, or real C/D mutation ran. | One narrow rebuild plus Huorong inspection must precede all executable validation. |

### 2026-07-13 - Source-only growth-to-application navigation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Unique exact target resolver; shared/ambiguous/missing targets refuse; scoped navigation scan contains no process, pipeline, registry, move, or delete authority. | Navigation only opens an internal page/drawer. |
| Data, API, and consistency | Warn | Structured `TargetAppName`; nested growth deduplication; latest findings cached; static invariant proves only the initializer and centralized setter assign profiles. | Behavioral execution and compiler checks remain deferred. |
| Code quality and maintainability | Warn | Target resolution, growth enrichment, presentation, and WPF navigation are separate; every inventory refresh uses `SetSoftwareProfiles`. | New source is uncompiled. |
| Testing and verification | Fail | Focused behavioral/static test source exists; manual static checks pass. | No test assembly may be generated before corrected Huorong definitions arrive. |
| Frontend, accessibility, and UX | Warn | MainWindow XAML parses; growth button and drawer conclusion text have stable AutomationIds; button precedes safety text. | UIAutomation and real first-visible screenshots are still required. |
| Operations, dependencies, and release | Warn | No GUI, Worker, UAC, installer, cleanup, registry, or real C/D operation ran. | One security-observed build must precede runtime acceptance. |

### 2026-07-13 - Source-only application cache quarantine closure

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Static policy requires cache folder allowlist, current-user roots, <=32 paths, no system/running/reparse/outside/overlap candidates; WPF cache scope has no process/registry/file-move authority. | Current app ownership and paths are rechecked after confirmation. |
| Data, API, and consistency | Warn | Dedicated operation kind/handler; exact profile correlation; manifest-before-move; timeline write inside compensation block; reverse restore on failure. | Runtime races, SQLite failure injection, and incomplete rollback behavior remain unexecuted. |
| Code quality and maintainability | Warn | Plan/path policy, drawer presentation, WPF coordinator, specialized handler, generic quarantine handler, and file service remain separate. Obsolete static expectations were updated to durable authority rules. | Compiler/analyzer evidence is unavailable. |
| Testing and verification | Fail | New source tests cover allow/refuse/stale/profile/temporary quarantine/timeline/restore/wiring/order cases; static gates pass. | No test assembly was generated due to pending Huorong definitions. |
| Frontend, accessibility, and UX | Warn | Existing drawer result panel and primary button have stable AutomationIds; completed/refused copy is path-free and returns to the regret center. | Real button flow, confirmation window, first-visible result screenshot, and UIAutomation are required later. |
| Operations, dependencies, and release | Warn | Installer and migration execution gates remain disabled; no GUI, cleanup, quarantine move, registry, Worker/UAC, or real C/D operation ran. | First runtime acceptance must use temporary fixture roots after one clean security-observed build. |

### 2026-07-13 - Source-only startup settings handoff

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Fixed catalog id/URI; `IsOpenOnly`; `ms-settings:` check; medium-risk confirmation; drawer scope has no process/registry/service/task/pipeline authority. | Shared launcher owns the only Settings process start. |
| Data, API, and consistency | Pass | Presenter grants handoff only for ordinary startup entries on non-system profiles; service/task-only and system profiles have no action. | No mutation descriptor is created. |
| Code quality and maintainability | Warn | Catalog, decision presenter, drawer host, primary routing, and shared launcher are separate; Agent and drawer reuse one allowlist. | New source remains uncompiled. |
| Testing and verification | Fail | Source tests cover ordinary/system/service/task states, URI/confirmation, routing, and authority; static checks pass. | No test assembly or Settings launch is allowed before the Huorong update. |
| Frontend, accessibility, and UX | Warn | Stable existing AutomationIds retained; label changed to `管理自启动`; path-free explanation says Windows makes the final choice. | Confirmation UI and actual opened page require UIAutomation/manual inspection later. |
| Operations, dependencies, and release | Warn | No Settings page, process, registry, service, task, GUI, Worker/UAC, or real mutation ran. | Microsoft documents the URI, but Windows version/SKU runtime availability still needs local verification. |

### 2026-07-13 - Source-only structured background component evidence

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Static Core/Scanner authority checks find no `OperationDescriptor`, registry write/delete, service controller, process start, or file/directory write/delete authority in the new evidence path. All observations and snapshots hard-code read-only, rollback-incomplete, and non-executable state. | Exact source locators remain behind technical details. |
| Data, API, and consistency | Warn | Identity binds kind/source/name; observation fingerprint binds configuration; Run source, service start/runtime, and task Settings enablement are structured; unknown inputs remain unknown; growth clones preserve evidence. | StartupApproved state is intentionally unknown and persistence/serialization are unexecuted. |
| Code quality and maintainability | Warn | Core identity/observation/snapshot/readiness, Scanner records/readers/builder, and presentation are separate. Compatible name lists remain for existing consumers. | Compiler, nullable analysis, and analyzers remain unavailable during the antivirus pause. |
| Testing and verification | Fail | Focused source tests cover identity drift, state separation, structured mapping, legacy refusal, null service state, task Settings scope, folded details, and authority. Seven static checks passed. | No test assembly was generated and no test ran. |
| Frontend, accessibility, and UX | Warn | Beginner summaries remain path-free; structured evidence is only in `TechnicalDetailsHiddenByDefault`; XAML parses; runtime-bound action label is `管理自启动`. | UIAutomation and first-visible-area screenshot are still required by `AGENTS.md`. |
| Operations, dependencies, and release | Warn | No build, test, GUI, Settings, registry/service/task mutation, Worker/UAC, installer, migration, or real C/D operation ran. | Corrected Huorong definitions and one observed build are required before runtime acceptance. |

### 2026-07-13 - Source-only StartupApproved correlation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Core stores only status/length/SHA-256; no raw-byte property, byte indexing, decoder, `OperationDescriptor`, or registry mutation API exists. Scanner uses read-only base keys/subkeys/GetValue. | Registry locators appear only in folded technical details. |
| Data, API, and consistency | Warn | Explicit HKCU64/HKLM64/HKLM32 roots; HKLM32 Run maps to HKLM64 Run32 approval; missing/binary/unsupported/unreadable remain distinct; evidence participates in observation drift fingerprint. | Runtime registry-view behavior is unexecuted and activation intentionally remains unknown. |
| Code quality and maintainability | Warn | Approval factory, Run reader, Scanner record, Builder correlation, technical presentation, and beginner handoff remain separate. | Compiler/nullable/analyzer evidence is unavailable. |
| Testing and verification | Fail | Focused source tests cover drift, no retention/decoding, evidence states, explicit views, propagation, authority, and UX. Seven static checks passed. | No test assembly was generated and no test ran. |
| Frontend, accessibility, and UX | Warn | Beginner copy says to confirm in Windows and does not expose paths/fingerprints; technical details remain folded; XAML parses. | UIAutomation and screenshot remain deferred. |
| Operations, dependencies, and release | Warn | No build, test, GUI, Settings, registry write, Worker/UAC, or real system mutation occurred. | Corrected Huorong definitions are required before executable verification. |

### 2026-07-13 - Source-only local Computer Agent conversation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Replies hard-code local/non-executable state; evidence-derived absolute paths fall back; raw questions are not returned; WPF handler scope contains no process, pipeline, operation, registry, or file/directory mutation authority. | Recommended D-drive policy roots are intentional product policy, not scanned private paths. |
| Data, API, and consistency | Warn | Bounded intent enum; missing evidence is explicit; exact unique app name is required; duplicate/stale targets refuse; navigation pages are allowlisted. | Runtime scanner/profile behavior and nullable/type analysis remain unexecuted. |
| Code quality and maintainability | Warn | Core presenter, existing evidence presenters, WPF binding, and exact-app resolver remain separate. Seven source checks pass after XML defects were corrected. | C# compilation/analyzers are unavailable during the antivirus pause. |
| Testing and verification | Fail | `AgentConversationTests.cs` covers nine focused scenarios as source; MainWindow XML, 36 Click handlers, authority, order, strict UTF-8, and test-source audits pass. | No test assembly was generated and no test ran. Initial failed audit scripts were invalidated and recorded. |
| Frontend, accessibility, and UX | Warn | Stable AutomationIds cover the question/answer controls; answer panel precedes static suggestions; Agent and C-drive scroll viewers are structurally valid. | `AGENTS.md` still requires UIAutomation plus a real first-visible screenshot after the security gate clears. |
| Operations, dependencies, and release | Warn | No build, GUI, cloud call, process launch, Worker/UAC, registry write, cleanup, migration, installer, or real C/D mutation occurred. | Corrected Huorong definitions and one observed `Css.Tests` build remain the next executable gate. |

### 2026-07-14 - Source-only beginner migration wording

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Migration methods remain presentation-only; no process, pipeline, operation, registry, or file/directory mutation authority was added. | Destination is an intentional recommended D-drive policy path; source paths remain absent from beginner copy. |
| Data, API, and consistency | Pass | Every existing `MigrationRiskBand` has explicit Chinese summary and explanation; D-drive/system/cache-only states remain distinct. | A future enum value falls back to evidence-insufficient wording and needs an explicit mapping. |
| Code quality and maintainability | Warn | Planner semantics and display projection remain separate; ProductExperience and Agent source assertions now use durable Chinese meaning. | Compiler/analyzer evidence remains unavailable. |
| Testing and verification | Fail | Six source-only checks passed for copy, authority, contracts, XML/handlers, UTF-8, and feature gates. | No focused or full test ran. |
| Frontend, accessibility, and UX | Warn | Drawer/Agent copy states destination, snapshot/rollback, two verification goals, and no immediate movement. | Real rendered wrapping and screenshot remain unverified. |
| Operations, dependencies, and release | Pass | Installer launch and MainWindow migration readiness remain disabled; no real operation ran. | Runtime gate remains intentionally closed. |

### 2026-07-14 - Source-only real application icons

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Parser refuses UNC/URI/relative/unresolved/command-like values; loader requires fixed drive/non-reparse path and contains no process, Shell execution, HTTP, registry, or mutation authority. | Actual native decoder behavior is unexecuted. |
| Data, API, and consistency | Pass | Path+signed resource index propagate through Builder/Profile/Tile/growth clone; malformed evidence becomes null and preserves letter fallback. | Real registry formats may reveal additional safe formats later. |
| Code quality and maintainability | Warn | Parser, model propagation, loader, and XAML fallback are separated; cache is bounded and file-version-bound; images freeze and native handles release in `finally`. | C# compilation/PInvoke marshalling analysis is still missing. |
| Testing and verification | Fail | Focused source tests cover parser, refusal, propagation, UI binding, cache, authority, and cleanup; seven static checks pass. | No test assembly, native icon extraction, or GUI run occurred. |
| Frontend, accessibility, and UX | Warn | Stable 62 px tile dimensions retained; real icon and fallback layers are explicit; existing item accessibility name stays path-free. | Actual icon quality, mixed fallback rendering, and first-view screenshot are required later. |
| Operations, dependencies, and release | Warn | No network, process, build, test, GUI, Worker/UAC, or C/D mutation occurred. | Corrected Huorong definitions remain the executable gate. |

### 2026-07-14 - Source-only home Agent next-action closure

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Core exposes only a closed C-drive/Applications destination; WPF handler re-resolves exact apps, checks the internal allowlist, and contains no process, pipeline, operation, registry, or file/directory authority. | App names remain beginner-visible identity hints, not operation authority. |
| Data, API, and consistency | Pass | Generic findings map to C-drive evidence; exact app targets are trimmed and re-resolved; missing/duplicate/stale targets fall back without guessing; all four response factories set `CanExecuteDirectly=false`. | Runtime inventory refresh is still unexecuted. |
| Code quality and maintainability | Warn | Presenter owns navigation intent; WPF owns mapping and view state; existing app resolver owns freshness/uniqueness. Seven independent source checks pass. | Compiler, nullable analysis, and analyzers remain unavailable. |
| Testing and verification | Fail | ProductExperience source tests cover first-visible order, handler authority, generic/exact/fallback navigation, and non-execution. Static XML/37-handler/model/test/UTF-8 checks pass. | No test assembly was generated and no test ran. |
| Frontend, accessibility, and UX | Warn | `HomeAgentResponseNavigateButton` has a stable AutomationId and appears between the response conclusion and `KeyFindingsListBox`. | `AGENTS.md` still requires UIAutomation and a real first-visible screenshot after the antivirus gate clears. |
| Operations, dependencies, and release | Pass | No build, test, GUI, scan, process, Worker/UAC, installer, cleanup, migration, or real C/D operation occurred. | Corrected Huorong definitions remain the next executable gate. |

### 2026-07-14 - Installer launch readiness connection

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Trusted signature and stable package identity are rechecked; package paths require fixed local storage and no reparse chain; snapshot, manual source, four-part final consent, 15-minute expiry, target policy, argument reconstruction, launch-time SHA-256 lock, and `SafetyOperationPipeline` all fail closed. | No real package was launched; this is code/fixture evidence, not production installer acceptance. |
| Data, API, and consistency | Pass | Typed runtime and preparation states separate product availability, package capability, and target readiness. Planner and handler independently require the target policy. Post-scan presentation explicitly refuses to infer installation success from exit code. | Parent installers may spawn children; the result already tells the user to rescan when state is unclear. |
| Code quality and maintainability | Warn | Readiness, preparation, package-path policy, planner, handler, coordinator, and WPF responsibilities are separate. Last product build after target preflight had 0 warnings/errors; 238 current source/XAML files pass strict UTF-8 and MainWindow XML parses. | The final small WPF deduplication and two new tests are uncompiled after NuGet assets were invalidated. |
| Testing and verification | Warn | Initial installer-focused tests passed 86/86. Existing compiled regression passed 586/586 with one obsolete source assertion explicitly excluded; its replacement and two new target-refusal tests are present in source. | A normal restore, current focused tests, and current full regression are still required. |
| Frontend, accessibility, and UX | Warn | Existing analysis, capability, target, safety, four acknowledgement, readiness, confirm, and result controls have stable AutomationIds; unavailable targets now stop before snapshot/consent. | GUI approval quota prevented a current fixture screenshot and final-consent UIAutomation run. |
| Operations, dependencies, and release | Warn | Corrected Huorong definitions accepted generated assemblies; no alert appeared and no real installer/UAC/system mutation ran. Migration production remains same-signer gated. | NuGet assets require a normal network restore; signed release packaging and disposable-machine acceptance remain release gates. |

### 2026-07-14 - Personal-storage diagnosis gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `PersonalStorageAnalysis.cs` accepts explicit personal roots, uses the existing scan tree, and contains no file read/delete, process start, `OperationDescriptor`, or `SafetyOperationPipeline` authority. | Absolute paths remain backend evidence only. |
| Data and consistency | Pass | File identity is explicit; large-file age/size and duplicate same-name/exact-size thresholds are bounded; candidate-byte sums saturate. | Same name and size are labeled only as suspicious, never proof. |
| Frontend and accessibility | Pass | The C-drive page has unique summary/list AutomationIds after growth evidence; beginner copy is path-free and non-executable. | Real screenshot remains pending. |
| Testing and verification | Warn | 340 C#/XAML files pass strict UTF-8; MainWindow XML, 42 handlers, 122 unique AutomationIds, mojibake and authority audits pass; focused tests are present. | New test source is not compiled until NuGet restore succeeds. |
| Operations and release | Warn | No personal file was read, hashed, moved, quarantined, or deleted. | A normal restore, full regression, and GUI capture remain required. |

### 2026-07-14 - Quarantine governance gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Manifest inspection confines paths to the configured root, validates item/id relationships, rejects UNC original paths, ADS and existing reparse chains, and rechecks before restore/purge. | Type-level compile is still pending. |
| Data and consistency | Pass | Retention options validate; totals saturate; active/reclaimable/projected bytes are distinct; batches are capped at 100; permanent outcomes journal `NotRestorable`. | Corrupt manifests are skipped/refused rather than guessed. |
| Destructive-operation safety | Pass | Purge is Manual-only, Medium risk, no rollback/snapshot claim, whole-batch preflight, explicit irreversible text, checkbox final consent, `SafetyOperationPipeline`, bounded iterative deletion, and no automatic load/refresh execution. | No real purge was executed. |
| Frontend and accessibility | Pass | The regret center has stable summary/candidate/button IDs; confirmation has warning, outcome, acknowledgement and confirm IDs; warning precedes acknowledgement and confirm starts disabled. | Real WPF UIAutomation/screenshot remains pending. |
| Testing and verification | Warn | 346 strict UTF-8 files, 14 WPF XML files, 72 handlers, unique per-window IDs and all source-policy gates pass. Existing compiled regression passes 586/586 excluding one documented obsolete installer assertion. | New governance tests and current source have not compiled due the restore blocker. |
| Operations and release | Warn | No real quarantine, original file, timeline, registry, service, task, installer, migration, or C/D content changed. | Temp-fixture purge/restore tests and signed package acceptance remain required. |

### 2026-07-14 - Health digest and migration reachability gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Digest persistence rejects path-like visible text and is bounded; migration evidence only opens final consent; the four consent fields and signed-package trust refusal remain present. | Source evidence only for the latest edits. |
| Data and consistency | Pass | Digest identity upserts and history is bounded; migration success propagates back to MainWindow and triggers a fresh software scan. | Runtime SQLite and real migration monitoring remain unverified. |
| Destructive-operation safety | Pass | No consent flag is inferred from evidence creation; elevated request composition still requires all four final acknowledgements; no signing bypass was added. | No real operation was executed. |
| Frontend and accessibility | Pass | Migration plan/final-consent controls retain stable AutomationIds; 14 XAML files parse, 108 event bindings resolve, and 251 IDs are unique per window. | A real screenshot is pending. |
| Testing and verification | Warn | 349 C#/XAML files pass strict UTF-8 and focused source assertions were updated; static authority checks pass. | Current source and new tests are not compiled because normal NuGet restore is still required. |
| Operations and release | Warn | Corrected Huorong definitions are installed and are no longer the blocker; signed package trust remains mandatory. | NuGet recovery, full regression, solution build, and fixture GUI acceptance are still required. |

### 2026-07-14 - Bounded post-install C-drive footprint evidence

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `InstallFootprintCapture.cs` uses five fixed local C roots, `TopDirectoryOnly`, 4096-entry/eight-root bounds, reparse refusal, and no file-content, delete, move, process, registry, service, or task API. | Technical paths remain behind the collapsed detail view; first-level copy is count-only. |
| Data, API, and consistency | Pass | Schema 2 binds status/count/fingerprint; coordinator compares all three before launch; footprint diffs require two complete captures; incomplete results retain known inventory evidence but cannot claim absence. | Top-level observation intentionally cannot detect every write inside an existing directory. |
| Destructive-operation safety | Pass | Incomplete evidence adds observe-only guidance and refuses concrete candidate previews; all report/Agent/plan models have `CanExecuteDirectly=false`. | No real installer or system mutation ran. |
| Code quality and maintainability | Pass | Probe, evidence service, coordinator, diff builder, presenters, and WPF orchestration have separate responsibilities. Test-project and solution builds passed with 0 warnings/errors. | MainWindow remains large but no new operation authority was added there. |
| Testing and verification | Pass | Installer-focused 52/52; full 623/623; 257 strict UTF-8 files; 14 XAML parses; 58 handlers; 254 unique AutomationIds; two fixture WPF smokes; no leftover process/temp fixture. | Real installer/root-latency acceptance is intentionally deferred. |
| Frontend, accessibility, and UX | Warn | UIAutomation proves four cards, Agent headline/four steps, three-item plan, collapsed details, hidden identifiers, and preview-only controls. `.omx/qa-install-diff-cards.png` and `.omx/qa-install-diff-action-plan.png` are clean and readable. | `.omx/qa-install-diff-agent.png` repeatedly contains desktop-compositor black areas; keep visual evidence warning until a clean dedicated Agent capture is obtained. |
| Operations, dependencies, and release | Pass | Normal restore succeeded after Huorong definitions update; current binaries are freshly built. No installer, UAC, cleanup, migration, uninstall, registry/service/task change, or real C/D mutation ran. | Signed/disposable-machine acceptance remains a separate release activity. |

### 2026-07-15 - Reversible current-user Run startup control

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Production adapter is limited to `RegistryHive.CurrentUser`, `RegistryView.Registry64`, and the exact Run key; StartupApproved is read-only evidence. WPF has no direct registry API. Fixture source has no registry/process API. | HKLM, Run32, services, tasks, system apps, ambiguous and name-only candidates are refused or handed to Windows Settings. |
| Data, API, and consistency | Pass | Exact observation identity/fingerprint, value type/data, ACL fingerprint, approval evidence, snapshot id/path/SHA, age and operation registry scope are revalidated. Cancel deletes only a verified uncommitted manifest. | Apps may recreate a Run entry later; a fresh scan reports that instead of fighting the app. |
| Destructive-operation safety | Pass | Disable requires two acknowledgements, `ConfirmationAccepted`, a Medium-risk descriptor, snapshot/rollback, `SafetyOperationPipeline`, and automatic restore if timeline journaling fails. Restore refuses collisions and ACL/approval drift. | Restore is separately confirmed from the timeline; failures remain restorable instead of being marked complete. |
| Frontend, accessibility, and UX | Pass | Stable AutomationIds exist on conclusions, outcomes, acknowledgements, technical expanders, and cancel/confirm controls. Both screenshots show path-free first views without overlap; confirmation starts disabled and details collapsed. | Visual evidence: `.omx/qa-startup-control-confirmation.png`, `.omx/qa-startup-restore-confirmation.png`. |
| Testing and verification | Pass | Focused 42/42; full 646/646; solution build 0 warnings/errors; 16 XAML parses; 372 strict UTF-8 C#/XAML files; 265 unique AutomationIds; two cancel-only GUI smokes; no leftover process/fixture. | No automated test changed the real registry. |
| Operations, dependencies, and release | Warn | Huorong definitions are updated and current binaries build cleanly. No real registry, service, task, installer, uninstall, migration, or C/D mutation ran. | Positive real-HKCU mutation/restore requires one explicitly disposable Run value and disposable test account before release. |

### 2026-07-15 - Quarantine candidate identity and revalidation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Candidate preparation rejects UNC/ADS, roots, protected directories, reparse chains, duplicates, overlaps, and quarantine overlap; beginner UI does not display identity values. | Technical identity remains process-local operation evidence. |
| Data, API, and consistency | Pass | Windows handle evidence binds path/type/volume/file id/creation/write/length; unprepared confirmation is refused; all three production UI entry points use the same preparation contract. | A changed candidate requires a new scan and consent. |
| Destructive-operation safety | Pass | Whole-batch post-consent preflight precedes all moves; each item is revalidated after manifest creation immediately before move; existing rollback and timeline behavior remains. | No real user file was moved in verification. |
| Frontend, accessibility, and UX | Pass | Existing stable confirmation AutomationIds remain; real WPF smoke reached the path-free first view and cancel preserved the fixture with zero quarantine items. | Visual evidence: `.omx/qa-cdrive-cleanup-confirmation.png`. |
| Testing and verification | Pass | Identity 7/7; related 33/33; full 653/653; solution build 0 warnings/errors; 375 strict UTF-8 files; 16 XAML parses. | Replacement, unchanged-directory, batch preflight, protected-root, and source-order tests are included. |
| Operations, dependencies, and release | Warn | Huorong definitions are updated; fixture-only GUI and temporary-directory tests completed with no leftover mutation. | Positive cleanup/restore remains a disposable-fixture release check, never an ordinary real-C-drive test. |

### 2026-07-15 - Agent startup advice truth and navigation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Agent uses a presentation-only single-observation predicate, exposes no registry locator/command/path, and retains `CanExecuteDirectly=false`. | Fresh exact evidence is still prepared only inside the drawer operation workflow. |
| Data, API, and consistency | Pass | Aggregate answer, exact-app answer, background review, and startup/service plan use the same eligibility rule and keep service/task/name-only cases separate. | Cached profile evidence can navigate but cannot authorize mutation. |
| Destructive-operation safety | Pass | GUI smoke invokes only question and navigation controls; it never opens confirmation or invokes startup mutation. | Output records `noOperationExecuted=true`; no real registry was changed. |
| Frontend, accessibility, and UX | Pass | Stable AutomationIds prove the answer and exact-app navigation; `ScrollToTop` prevents clipped first-view identity; screenshot is path-free and visually clean. | Visual evidence: `.omx/qa-agent-startup-advice.png`. |
| Testing and verification | Pass | Agent-focused 15/15; full 658/658; solution build 0 warnings/errors; 274 non-generated strict UTF-8 files; 16 XAML parses; fixture-only WPF smoke passed. | The script is ASCII-compatible with Windows PowerShell 5.1 and cleans all fixtures. |
| Operations, dependencies, and release | Warn | Huorong definitions are updated and current binaries build cleanly. | Positive real startup mutation remains a disposable-account release check. |

### 2026-07-15 - Home whole-PC health runtime acceptance

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Production probe returns only D capacity, memory totals/load, process count, and battery state; GUI rows reject paths, registry ids, and executable names. | No process names or file contents are retained. |
| Data, API, and consistency | Pass | C fixture is on C; D row reads the fixed local D drive; available/not-present/unavailable branches are accepted; startup remains explicitly a clue and trend explicitly manual. | Score still truthfully says it is based on disk space. |
| Destructive-operation safety | Pass | Smoke clicks scan, read-only plan generation, and internal navigation only; output says `noOperationExecuted=true`. | No cleanup, process/power/startup/registry/service/task or other mutation ran. |
| Frontend, accessibility, and UX | Pass | Seven rows render in the first Home working area with stable AutomationIds; Agent conclusion/safety/navigation are visible; window-only screenshot has no clipping or overlap. | Visual evidence: `.omx/qa-home-agent-next-action.png`. |
| Testing and verification | Pass | Focused 5/5; full 658/658; solution build 0 warnings/errors; 274 non-generated strict UTF-8 files; 16 XAML parses; zero leftover fixture/process. | Runtime values were captured from the production read-only machine probe. |
| Operations, dependencies, and release | Pass | Huorong definitions are current and the WPF process completed normally. | Hardware readings are point-in-time observations, not continuous monitoring or diagnosis. |

### 2026-07-15 - MSIX managed-storage handoff

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Capability carries only fixed id `default-save-locations`; catalog URI is fixed `ms-settings:savelocations`; the WPF handler revalidates mode/id and calls the existing allowlisted opener. | No URI, command, path, or argument comes from user/package text. |
| Data, API, and consistency | Pass | `WindowsManagedStorageHandoff` is distinct from untrusted/unsupported package states; only trusted MSIX receives the handoff id; other modes remain null. | `CanRequestInstallerLaunch=false` and no guessed directory arguments remain enforced. |
| Destructive-operation safety | Pass | MSIX preparation and route-memory buttons are disabled; the new action is open-only and confirmation-aware; no installer, setting mutation, operation, pipeline, registry, service, task, or file mutation ran. | Any actual save-location change remains a user action in Windows Settings. |
| Frontend, accessibility, and UX | Warn | Stable button/title/status AutomationIds, truthful managed-location copy, XAML parse, handler resolution, and static authority/order tests pass. | Computer Use timed out before the WPF window appeared, so no first-view/cancel screenshot is claimed. |
| Testing and verification | Pass | Focused 222/222; full 682/682; solution build 0 warnings/errors; 278 strict UTF-8 files; 16 XAML parses; 61 unique handlers resolved; literal IDs unique per XAML. | The focused set intentionally covers installer, Agent, settings catalog, and WPF source contracts. |
| Operations, dependencies, and release | Warn | Huorong definitions are updated and generated assemblies remain present; no test process remained. | Retry cancel-only visible acceptance when Computer Use launch is available; signed/disposable package acceptance remains a release check. |

### 2026-07-15 - Recycle Bin review handoff

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | The card carries only a typed action; the catalog owns fixed `explorer.exe`/`shell:RecycleBinFolder`; question text never becomes a command or argument. | Beginner text contains no physical Recycle Bin path or shell argument. |
| Data, API, and consistency | Pass | Only positive big-rock names containing the scanner's fixed `Recycle` identity become Recycle Bin cards; pagefile/shadow cards remain actionless. | Size remains current-user C-drive evidence from `SHQueryRecycleBin`. |
| Destructive-operation safety | Pass | Source has no `SHEmptyRecycleBin`; handler has no process/delete/pipeline authority and calls only the fixed allowlisted opener; Agent `清空` wording remains non-executable. | OMNIX does not clear, delete, restore, move, or quarantine Recycle Bin items. |
| Frontend, accessibility, and UX | Warn | Plain-language card copy, conditional stable `CDriveOpenRecycleBinButton`, XAML parsing, handler resolution, static order/authority tests pass. | Computer Use timed out before the app appeared, so no first-view screenshot is claimed. |
| Testing and verification | Pass | Focused 5/5; related 191/191; full 686/686; build 0 warnings/errors; 278 strict UTF-8 files; 16 XAML parses; 62 resolved handlers; literal IDs unique. | The first static script output was rejected after a PowerShell binding error; the corrected full rerun passed cleanly. |
| Operations, dependencies, and release | Warn | Huorong definitions are current; no OMNIX process remained after the timed-out launch. | Retry open-only visual acceptance when Computer Use is available; do not manually clear the user's Recycle Bin in testing. |

### 2026-07-15 - C-drive root-cause safe internal handoffs

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Cards carry only enum actions and beginner labels; physical paths, commands, and operation descriptors are not added to the presentation. | App and personal-file destinations reuse already-sanitized surfaces. |
| Data, API, and consistency | Pass | Action mapping is category based and refuses `IsUnexpectedRoot`; app navigation applies the existing `CDrive` filter; temp selection requires `CanExecute` plus an existing operation. | A card does not invent application ownership or cleanup eligibility. |
| Destructive-operation safety | Pass | Isolated handler audit excludes `ExecuteRecommendation`, pipeline, process, delete, and Recycle Bin clear calls; recommendation selection still requires the existing second confirmation. | No mutation or confirmation was performed in tests. |
| Frontend, accessibility, and UX | Warn | Runtime AutomationIds combine action plus a deterministic path-free hash and are tested unique/stable across a multi-card summary; button labels are plain language; XAML and all handlers compile/resolve. | The adjacent Computer Use launch failure prevents a real first-view screenshot claim. |
| Testing and verification | Pass | Focused 3/3; product 166/166; full 687/687; build 0 warnings/errors; 278 strict UTF-8 files; 16 XAML parses; 62 handlers; literal IDs unique. | Category mapping, unexpected-root refusal, button binding, and handler authority are covered. |
| Operations, dependencies, and release | Warn | No process or real-system action was needed for source verification. | Retry the combined C-drive visual smoke only through Computer Use when available. |

### 2026-07-15 - Beginner-first installer monitoring

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Only XAML hierarchy/copy and a fixture-only GUI smoke changed; package evidence, confirmation, coordinator, and worker trust code are unchanged. | No path, command, or installer identity is added to beginner copy. |
| Data, API, and consistency | Pass | Static handler contract proves the primary flow still creates before evidence, uses the production coordinator, accepts its after snapshot, and renders its report. | Manual diagnostics reuse the existing read-only snapshot functions. |
| Destructive-operation safety | Pass | No production handler or operation authority changed; the normal path still requires the existing final-consent window and coordinator checks. | No installer or system mutation ran during verification. |
| Frontend, accessibility, and UX | Warn | Automatic-monitoring copy precedes a stable default-collapsed advanced expander; literal AutomationIds are unique and all XAML/events resolve. | Real first-view screenshot is unavailable after the recorded Computer Use launch failure. |
| Testing and verification | Pass | Focused 2/2; related 240/240; full 700/700; build 0 warnings/errors; 282 strict UTF-8 files; 16 XAML parses; 120 handlers; smoke parses cleanly. | Red test first failed on the missing beginner hierarchy. |
| Operations, dependencies, and release | Warn | Updated antivirus definitions allowed normal test/build verification. | Signed/disposable installer acceptance remains a release gate. |

### 2026-07-15 - Agent automatic read-only evidence hydration

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Agent requests only the existing local inventory; loader failure copy does not include `ex.Message`, paths, registry locators, commands, or file content. | `UsedCloudAi=false` and no cloud transfer was added. |
| Data, API, and consistency | Pass | Pure policy tests cover relevant and irrelevant intents; the final presenter call occurs after the shared load and reads refreshed `_softwareProfiles`. | Successful empty inventory is cached; failure remains retryable via the existing gate. |
| Destructive-operation safety | Pass | Static handler audit found zero process, pipeline, descriptor, file move/delete, directory move/delete, or registry-write calls. | Natural-language input still cannot create or execute an operation. |
| Frontend, accessibility, and UX | Warn | Stable Ask/skill controls disable during preparation and restore in `finally`; existing response AutomationIds and first-view renderer remain. | A fresh real WPF screenshot is not claimed after the recorded Computer Use launch failure. |
| Testing and verification | Pass | Focused 13/13; related 222/222; full 713/713; build 0 warnings/errors; 282 strict UTF-8 files; 16 XAML parses; 120 event bindings; literal IDs unique. | One failed static command was rejected and rerun with Windows PowerShell-compatible string matching. |
| Operations, dependencies, and release | Warn | Antivirus definitions are current and normal build/test verification is restored. | Real-machine inventory latency remains a visual/runtime acceptance item. |

### 2026-07-15 - Agent-triggered C-drive read-only diagnosis

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Core scan failure copy omits `ex.Message`, stack traces, and paths; final Agent reply remains local and non-cloud. | Technical exception details are not rendered to either status or report text. |
| Data, API, and consistency | Pass | Gate tests prove in-flight identity, failure retry, cached success, and forced refresh; intent tests prove only missing C-drive evidence triggers. | Cancel/failure return false and are not cached as completed. |
| Destructive-operation safety | Pass | Agent handler authority audit found zero pipeline, descriptor, recommendation execution, file/directory move/delete, or registry-write references. | Scan still only observes and writes OMNIX-owned snapshot/digest records. |
| Frontend, accessibility, and UX | Warn | Ask button remains disabled during evidence preparation and final answer uses the refreshed summary; existing stable response hooks remain. | No fresh WPF screenshot is claimed after the recorded Computer Use launch failure. |
| Testing and verification | Pass | Focused 14/14; related 226/226; full 722/722; build 0 warnings/errors; 284 strict UTF-8 files; 16 XAML parses; 120 bindings; literal IDs unique. | Static privacy and authority checks both returned zero hits. |
| Operations, dependencies, and release | Warn | Antivirus definitions are current and full source/build verification is available. | Real C-drive scan duration and cancellation remain runtime acceptance items. |

### 2026-07-15 - Automatic undo-center history loading

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Load failure uses fixed path-free copy and omits `ex.Message`/stack details. | Timeline and quarantine data remain local. |
| Data, API, and consistency | Pass | Navigation ensure, manual/post-operation refresh, successful-empty caching, in-flight joining, and failure retry are covered by gate/source tests. | Existing operation callers retain `await LoadTimelineAsync()`. |
| Destructive-operation safety | Pass | Static load-scope audit found zero restore, purge policy, pipeline, file delete, or directory delete references. | Restore and permanent cleanup retain their separate confirmations. |
| Frontend, accessibility, and UX | Warn | Stable Timeline AutomationIds remain; copy says entry is automatic and the button says `重新加载`. | No fresh WPF screenshot is claimed after the recorded Computer Use launch failure. |
| Testing and verification | Pass | Focused 11/11; related 212/212; full 723/723; build 0 warnings/errors; 285 strict UTF-8 files; 16 XAML parses; 120 bindings; literal IDs unique. | Static timeline authority/privacy hits: zero. |
| Operations, dependencies, and release | Warn | Antivirus definitions are current and all automated gates run normally. | Restore-positive and purge-positive acceptance remain disposable-fixture release checks. |

### 2026-07-16 - One-shot uninstall/migration submission and post-attempt rescan

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Unknown uninstall presentation is fixed, path-free, omits raw exceptions, and exposes no command, registry, service, or process detail. | Screenshot: `.omx/qa-uninstall-unknown-attempt.png`. |
| Data, API, and consistency | Pass | Source contracts require attempt marking before coordinator invocation, one-shot button gating, null-aware parent rescans, and residue/closure checks only after current inventory is available. | Ordinary typed outcomes retain their existing completion semantics. |
| Destructive-operation safety | Pass | No catch retries execution; submitted migration/uninstall plans stay locked; the shared recovery helper contains one software read and no pipeline, move, or delete authority. | No real uninstall, migration, residue cleanup, registry edit, or file movement ran. |
| Frontend, accessibility, and UX | Pass | Existing stable result-window AutomationIds and order remain; the new unknown title, status, conclusion, Agent advice, safety text, and close button all render in the first view. | WPF render test persisted and manually inspected the PNG. |
| Testing and verification | Pass | Focused 4/4; related 432/432; full 942/942; solution build 0 warnings/errors; 339 strict UTF-8 C#/XAML files with zero replacement characters. | The first full command used a wrong unobserved solution name; only the corrected `.slnx` run is accepted. |
| Operations, dependencies, and release | Warn | Updated antivirus definitions permit normal build/test/render verification. | Signed package plus disposable-machine real uninstall/migration acceptance is still required before release. |

### 2026-07-16 - Real Application Management search placeholder

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Search still passes only local application-name/publisher text to the existing in-memory catalog presenter. | No path, command, cloud transfer, scan, or mutation was added. |
| Data, API, and consistency | Pass | Empty search remains unfiltered; programmatic target names remain real TextBox values; legacy placeholder compatibility stays covered by existing catalog tests. | Hint visibility is presentation-only and updates before catalog refresh. |
| Destructive-operation safety | N/A | The slice changes no action, operation descriptor, pipeline, process, file, registry, service, task, uninstall, cleanup, or migration behavior. | UI filtering only. |
| Frontend, accessibility, and UX | Warn | Fixed `160x34` host, empty value, non-interactive hint, stable input/hint AutomationIds, and handler ordering pass structural tests. | Computer Use `launch_app` timed out and one passive poll found no OMNIX window, so no real screenshot is claimed. |
| Testing and verification | Pass | Focused search/catalog 5/5; full 944/944; build 0 warnings/errors; 340 strict UTF-8 C#/XAML files with zero replacement characters. | The abandoned whole-MainWindow test failed only because App resources were not loaded; the accepted test is structural and handler-scoped. |
| Operations, dependencies, and release | Warn | Updated antivirus definitions allow normal compilation and tests. | Retry one real Application Management screenshot when the Windows helper can launch the local build. |

### 2026-07-16 - Actionable uninstall post-scan result

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Result-window code has no pipeline, quarantine, process, file mutation, registry, service, or task authority; beginner text is fixed and path-free. | Worker summaries and private paths are not copied into the result UI. |
| Data, API, and consistency | Pass | Typed enum covers Close/Retry/Review; plan handoff preserves the selected action; MainWindow refreshes current inventory before resolving it. | Scan failure remains unknown and does not become a clean result. |
| Destructive-operation safety | Pass | Close performs no review; Retry calls only residue scan/presentation; Review alone may enter the existing separate confirmation path. | No real uninstall or residue operation ran. |
| Frontend, accessibility, and UX | Pass | Stable primary/close AutomationIds, deterministic initialized button text, first-view visibility test, and manually inspected `.omx/qa-uninstall-post-scan-action.png`. | Clean state hides the redundant primary action. |
| Testing and verification | Pass | Focused 9/9; related 361/361; full 948/948; build 0 warnings/errors; 341 strict UTF-8 C#/XAML files with zero replacement characters. | Static tests use balanced method extraction. |
| Operations, dependencies, and release | Warn | Updated antivirus definitions allow normal build/test/render verification. | Signed package plus disposable-machine real uninstall acceptance remains required before release. |

### 2026-07-16 - Personal-file read-only location inspection

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Default visible finding text remains path-free; exact paths appear only in the explicit local detail window; failure messages never echo paths. | No cloud or file-content read was added. |
| Data, API, and consistency | Pass | View models preserve deduplicated captured evidence; MainWindow caches only the current session set; launcher requires case-insensitive membership plus current file existence. | A new scan clears the previous evidence before analysis. |
| Destructive-operation safety | Pass | Detail window and click handler have zero delete/move/quarantine/pipeline authority; launcher selects through Explorer and never opens the target file. | No cleanup control or operation descriptor exists. |
| Frontend, accessibility, and UX | Pass | Per-row dynamic inspect AutomationId; detail title/list/safety/open/close AutomationIds; first-view render `.omx/qa-personal-storage-inspection.png` inspected with two readable locations and no overlap. | Default C-drive list stays compact and path-free. |
| Testing and verification | Pass | Focused 10/10; related 191/191; full 953/953; build 0 warnings/errors; 345 strict UTF-8 C#/XAML files; all XAML parses; forbidden authority hits 0. | Launcher has exactly one fixed process-start site and one structured argument-list addition. |
| Operations, dependencies, and release | Pass | Verification did not open a real personal file or Explorer process. | A manual click-through can be performed later with a disposable fixture file. |

### 2026-07-16 - Persisted digest to current C-drive evidence

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | History remains path-free; scan/navigation failure uses fixed copy and does not expose raw exceptions or local paths. | No new persistence or cloud transfer. |
| Data, API, and consistency | Pass | Action starts/joins `EnsureHealthScanLoadedAsync`, then requires gate completion and `_lastHealthSummary`; digest reload tracks availability without re-enabling in flight. | Existing current session avoids a redundant forced refresh. |
| Destructive-operation safety | Pass | Handler authority scan has zero pipeline, descriptor, file/directory mutation, quarantine, registry, service, or process-start references. | The underlying health workflow remains observation plus OMNIX-owned snapshot/digest writes. |
| Frontend, accessibility, and UX | Warn | Stable button AutomationId remains; copy distinguishes restart history from current evidence; button disables during loading and success/failure text is explicit. | No fresh whole-MainWindow screenshot is claimed after the recorded Windows helper launch timeout. |
| Testing and verification | Pass | Focused 16/16; related 195/195; full 954/954; build 0 warnings/errors; 346 strict UTF-8 files; all XAML parses; one Ensure call, zero forced Refresh calls; success order true. | Source checks use balanced method extraction. |
| Operations, dependencies, and release | Pass | Updated antivirus definitions allow normal build/test verification. | Full scan latency remains a manual runtime observation, not a destructive release gate. |

### 2026-07-16 - Agent background context handoff

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Path-like app names become `这个应用`, cannot navigate, and raw component identities are absent from visible item text. | Navigation remains local and does not add cloud transfer. |
| Data, API, and consistency | Pass | Item target and display name are separated; aggregate replies carry typed Resident; MainWindow validates Apps-page consistency and whitelists only Resident. | Stale search is cleared before catalog refresh. |
| Destructive-operation safety | Pass | Item and aggregate handlers contain zero pipeline, descriptor, startup-control, registry, service-controller, process-start, or file-mutation references. | Neither action opens startup review automatically. |
| Frontend, accessibility, and UX | Warn | Dynamic stable `AgentBackgroundOpen_{AppName}` AutomationId, explicit `查看应用`, selected Resident filter, and honest empty/failure/success copy pass source tests. | No fresh whole-MainWindow screenshot is claimed after the recorded Windows helper launch timeout. |
| Testing and verification | Pass | Focused 15/15; related 251/251; full 956/956; build 0 warnings/errors; 347 strict UTF-8 files; all XAML parses; hook/AutomationId/filter assignment each 1. | System/ownership-pending action restrictions remain covered. |
| Operations, dependencies, and release | Pass | Updated antivirus definitions allow normal verification; no OS operation ran. | Manual visual click-through remains a non-destructive UX check. |

### 2026-07-18 - Agent migration/uninstall catalog handoff

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Aggregate replies carry typed enum filters only; existing page-consistency and allowlist checks remain before navigation. | No paths, cloud data, or arbitrary filter strings were added. |
| Data, API, and consistency | Pass | Migration=`CDrive`, uninstall=`Uninstallable`, startup=`Resident`; current inventory is joined before catalog refresh and stale search is cleared. | Existing catalog/drawer policies remain authoritative. |
| Destructive-operation safety | Pass | Extracted handler tests exclude migration/uninstall/startup plan methods, operation pipelines, process launch, registry, and service control. | Handoff only selects a catalog filter. |
| Frontend, accessibility, and UX | Warn | Existing Agent navigation control now opens the relevant selected filter and filter-specific beginner status copy is source-tested. | No new control was added; real whole-window click-through remains unclaimed after the recorded Computer Use launch timeout. |
| Testing and verification | Pass | Focused 8/8; related 279/279; full 957/957; build 0 warnings/errors; 347 strict UTF-8 files; 17/17 XAML parse. | Exact migration/uninstall reply assignments are one each. |
| Operations, dependencies, and release | Pass | No OS mutation or external dependency ran. | Signed/disposable mutation acceptance remains separate. |

### 2026-07-18 - Agent next-step typed application handoff

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Handler requires typed action, `IsNavigationOnly`, internal page allowlist, and Apps/filter consistency before delegating to the existing filter allowlist. | No paths, cloud data, or free-form filter values added. |
| Data, API, and consistency | Pass | Resident and C-drive actions carry distinct enum filters; empty/general actions remain null; stable IDs include page plus filter. | Presenter tests cover simultaneous Resident and CDrive actions. |
| Destructive-operation safety | Pass | Extracted handler excludes migration/uninstall/startup plans, operation pipeline, process launch, registry, and service control. | It only navigates or filters the catalog. |
| Frontend, accessibility, and UX | Warn | XAML binds the complete action and stable `AgentNextAction_Apps_Resident` / `AgentNextAction_Apps_CDrive` identities; focused source/VM tests pass. | Computer Use launch timed out and follow-up window enumeration found no OMNIX target, so no screenshot is claimed. |
| Testing and verification | Pass | Focused 2/2; related 275/275; full 959/959; build 0 warnings/errors; 348 strict UTF-8 files; 17/17 XAML parse. | Three old sync/string source contracts were updated. |
| Operations, dependencies, and release | Pass | No OS mutation ran. | Signed/disposable mutation acceptance remains separate. |

### 2026-07-18 - Home migration-closure catalog handoff

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Exact target and aggregate filter are mutually exclusive; both remain local, path-free, and page-validated. | No cloud or raw closure paths added. |
| Data, API, and consistency | Pass | Only targetless `MigrationClosure` responses receive `CDrive`; exact targets remain filter-null and current-inventory-resolved. | Personal storage and ordinary C-drive responses remain unchanged. |
| Destructive-operation safety | Pass | Extracted home handler excludes migration plan, operation pipeline, process launch, registry, and service control. | Aggregate navigation only filters current applications. |
| Frontend, accessibility, and UX | Warn | Existing homepage action now preserves relevant catalog context with existing selected-filter UX and beginner copy. | No new control; whole-window runtime proof remains covered by the existing Computer Use launch Warn. |
| Testing and verification | Pass | Focused 5/5; related 199/199; full 960/960; build 0 warnings/errors; 348 strict UTF-8 files; 17/17 XAML parse. | Handler uses shared method extraction. |
| Operations, dependencies, and release | Pass | No OS mutation ran. | Signed/disposable mutation acceptance remains separate. |

### 2026-07-18 - C-drive application handoff truth and reuse

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Existing root card/action identity checks remain before the typed handoff; no new data exposure. | Shared handoff remains local and path-free. |
| Data, API, and consistency | Pass | Branch delegates to `CDrive`; shared handoff loads before refresh and uses filtered `AppTilesListBox.Items.Count`. | Global inventory count is no longer used for filtered truth. |
| Destructive-operation safety | Pass | Root handler still excludes operation pipeline, descriptor, recommendation execution, process launch, and file deletion. | Delegation only navigates/filters. |
| Frontend, accessibility, and UX | Warn | Existing stable root-cause action ID and selected CDrive filter now share truthful unavailable/empty/populated copy. | No new control; existing whole-window visual launch Warn applies. |
| Testing and verification | Pass | Focused 5/5; related 282/282; full 960/960; build 0 warnings/errors; 348 strict UTF-8 files; 17/17 XAML parse. | Contract prohibits duplicate branch refresh/status code. |
| Operations, dependencies, and release | Pass | No OS mutation ran. | Signed/disposable mutation acceptance remains separate. |

### 2026-07-18 - Isolated GUI lifecycle verification

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Both probes used workspace-local isolated data roots; no real OMNIX data or system mutation was requested. | Test processes were stopped. |
| Data, API, and consistency | Pass | The Debug process remained alive for five seconds and exposed a unique title/handle; Computer Use independently returned exactly one matching window. | This proves startup, not visual correctness. |
| Destructive-operation safety | Pass | No app action was clicked and no scan/mutation was started. | Only exact test-process lifecycle was controlled. |
| Frontend, accessibility, and UX | Warn | A real OMNIX window exists and is discoverable. | `get_window_state` failed with `Computer Use app approval timed out`; no screenshot or interaction is claimed. |
| Testing and verification | Pass | Full source regression remains 960/960 and build 0 warnings/errors from the immediately preceding slice. | GUI lifecycle evidence is additive. |
| Operations, dependencies, and release | Warn | Shell launch can produce the window, while Computer Use direct launch/state approval remains unreliable. | Release runtime acceptance still requires signed/disposable fixtures. |

### 2026-07-18 - Reproducible portable test package

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Publishing is local; the script sends no data, launches no package, and the Chinese guide directs evidence collection to the local manifest/screenshot only. | No cloud or personal-path export was added. |
| Data, API, and consistency | Pass | Final manifest records 110 file hashes/lengths, App/worker signature statuses, same-signer result, framework runtime mode, and mutation readiness; sampled hashes independently match. | ZIP independently contains App, worker, rules, readme, and manifest. |
| Destructive-operation safety | Pass | Source contracts prohibit signing, certificate import, trust relaxation, output deletion, replacement, and `-Force`; existing output is refused before publish. | Both unsigned executables produce `BlockedUntilValidSameSignerPackage`. |
| Frontend, accessibility, and UX | Warn | UTF-8 Chinese beginner guide explains launch, runtime, read-only scope, and mutation boundary. | No new whole-window screenshot; Computer Use interactive approval remains Warn. |
| Testing and verification | Pass | Focused 4/4; full 964/964; build 0 warnings/errors; 349 strict UTF-8 C#/XAML files; 17/17 XAML parse; final ZIP has 139 entries. | Windows PowerShell 5.1 default and explicit existing-output branches were exercised. |
| Operations, dependencies, and release | Warn | Reproducible framework-dependent package and ZIP exist under `.artifacts/OMNIX-Entropy-test-20260718-205628`; .NET 8 Desktop Runtime requirement is explicit. | App/worker remain unsigned; production mutation acceptance requires one valid same-signer package and a disposable machine. |

### 2026-07-18 - Release debug-command surface removal

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Release worker fake process mode is Debug-guarded and its implementation is removed from Release compilation. | No new data collection or transfer. |
| Data, API, and consistency | Pass | Package manifest records `ReleaseCommandSurface=ProductionOnly`; actual worker lacks fake token in UTF-8/UTF-16 while both production mode tokens remain. | Debug lifecycle protocol remains available in Debug builds. |
| Destructive-operation safety | Pass | No production coordinator, IPC authentication, operation pipeline, signer policy, or mutation authority changed. | Package byte check is read-only and fails before manifest/ZIP on mismatch. |
| Frontend, accessibility, and UX | N/A | No UI or visible copy changed. | Latest portable package remains the read-only UX artifact. |
| Testing and verification | Pass | Focused 22/22; full 966/966; build 0 warnings/errors; 350 strict UTF-8 C#/XAML files; 17/17 XAML parse; actual Release and packaged DLL scans pass. | Worker-touching commands used `-m:1 -p:UseSharedCompilation=false` after an output race. |
| Operations, dependencies, and release | Warn | Latest package/ZIP `.artifacts/OMNIX-Entropy-test-20260718-210944` has 110 manifest files and 139 ZIP entries; command surface is ProductionOnly. | App/worker still `NotSigned`; real mutation acceptance remains deferred to valid same-signer/disposable fixtures. |

### 2026-07-18 - Home key-findings empty state and Release navigation

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Release run used workspace-local data/quarantine roots; Apps/Agent performed read-only inventory/presentation only. | No cleanup, uninstall, migration, settings, system-tool, or cloud action was invoked. |
| Data, API, and consistency | Pass | Default not-scanned copy, valid-empty copy, and populated list state are distinct; current Apps scan produced 391 profiles and Agent consumed that inventory. | Findings list is never shown empty after the summary binding. |
| Destructive-operation safety | Pass | Change has no scanner or operation authority; visual run clicked only internal navigation. | Unsigned package remains `BlockedUntilValidSameSignerPackage`. |
| Frontend, accessibility, and UX | Pass | Real 1268x778 Release first-view screenshot shows no blank findings rectangle; stable empty-state AutomationId is visible. Apps and AI Agent navigation produced distinct real first views. | Application page shows icon grid plus human-readable drawer; all text remained inside its panels. |
| Testing and verification | Pass | Focused/related 218/218; full 968/968; build 0 warnings/errors; 351 strict UTF-8 C#/XAML files; 17/17 XAML parse; all test windows closed. | Computer Use recovered from stale user input by refreshing before the close action. |
| Operations, dependencies, and release | Warn | Latest verified package/ZIP `.artifacts/OMNIX-Entropy-test-20260718-212514` has 110 manifest files and 139 ZIP entries; ProductionOnly command surface. | App/worker are still unsigned; positive real mutation remains a separate release gate. |

### 2026-07-18 - Agent page information hierarchy

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | XAML-only container change; real Release run used workspace-local data/quarantine roots and clicked only internal navigation/tabs. | No scan, settings, system tool, cleanup, uninstall, migration, service, registry, or file action ran. |
| Data, API, and consistency | Pass | Existing conversation/capability controls and handlers remain; default selection is index 0 and stable AutomationIds identify the TabControl and both tabs. | No Agent presenter, inventory, routing, or operation type changed. |
| Destructive-operation safety | Pass | No mutation authority changed; final manifest remains `BlockedUntilValidSameSignerPackage`. | Settings and system-tool buttons were visible but never invoked. |
| Frontend, accessibility, and UX | Pass | Computer Use captured the corrected full-width consultation first view and the separate capability view; default UI tree excludes capability descendants until the second tab is selected. | Initial 780px blank-space defect was found visually, fixed, republished, and protected by a source contract. |
| Testing and verification | Pass | Focused/related 216/216; full 970/970; build 0 warnings/errors; 352 strict UTF-8 C#/XAML files; 17/17 XAML parse; both test windows closed. | Static order tests protect root/tab/content placement and existing identities. |
| Operations, dependencies, and release | Warn | Final package/ZIP `.artifacts/OMNIX-Entropy-test-20260718-214320`; ProductionOnly command surface and read-only UX are verified. | App/worker remain unsigned; real mutation acceptance still requires a valid same-signer package and disposable machine. |

### 2026-07-18 - C-drive first-view hierarchy

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Real Release used workspace-local data/quarantine roots and only navigated to the C-drive page. | No drive scan, file read result, cleanup, quarantine, migration, registry, service, or external tool action ran. |
| Data, API, and consistency | Pass | Result visibility derives from the exact current root-cause, growth, personal-storage, and recommendation presenter counts; scan/cancel/failure states have distinct copy. | Scanner and recommendation construction are unchanged. |
| Destructive-operation safety | Pass | Visibility helper contains no pipeline, quarantine, or delete authority; action preview and continue button are absent until current recommendations exist. | Final package remains `BlockedUntilValidSameSignerPackage`. |
| Frontend, accessibility, and UX | Pass | Final real Release screenshot and UI tree show automatic C drive, read-only Start Scan guidance, growth-baseline guidance, and recommendation empty state without blank result boxes or premature isolation wording. | Stable state AutomationIds are present; populated lists retain their existing identities. |
| Testing and verification | Pass | Focused 2/2; related 211/211 and 171/171; full 972/972; build 0 warnings/errors; 353 strict UTF-8 C#/XAML files; 17/17 XAML parse; all test windows closed. | Static XML and method contracts protect default visibility and current-count switching. |
| Operations, dependencies, and release | Warn | Final package/ZIP `.artifacts/OMNIX-Entropy-test-20260718-220108`; ProductionOnly read-only first view verified. | App/worker remain unsigned; real scan performance and positive mutation still require separate acceptance. |

### 2026-07-18 - Installation Control first-view hierarchy

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Real Release used workspace-local roots and only internal navigation. | No file picker, installer file read, installer launch, Windows setting, process observation, or mutation action ran. |
| Data, API, and consistency | Pass | Empty routing presenter returns zero rows; rule visibility follows row count; report visibility follows a valid presenter/report and is revoked on a new/incomplete comparison. | Existing route rows and report cards retain types and bindings. |
| Destructive-operation safety | Pass | Changed methods contain no process start or operation pipeline authority; signature/preparation/confirmation gates are unchanged. | Final unsigned package remains mutation-blocked. |
| Frontend, accessibility, and UX | Pass | Final screenshot/UI tree show only installer selection/Agent analysis, empty rule summary, automatic monitoring, collapsed advanced diagnostics, and honest report state. | Empty fake row, blank report list, disabled Agent button, and premature technical detail are absent. |
| Testing and verification | Pass | Focused/related 217/217; full 975/975; build 0 warnings/errors; 354 strict UTF-8 C#/XAML files; 17/17 XAML parse; test window closed. | Corrected fail-closed XML assertions proved all default visibility attributes before implementation. |
| Operations, dependencies, and release | Warn | Final package/ZIP `.artifacts/OMNIX-Entropy-test-20260718-221512`; read-only first view verified. | App/worker remain unsigned; real installer launch/post-install observation requires trusted disposable-fixture acceptance. |

### 2026-07-18 - Undo Center first-view hierarchy

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Real Release used workspace-local data/quarantine roots and only internal navigation. | No user history was read; no restore, purge, scan, uninstall, migration, registry, service, or file action ran. |
| Data, API, and consistency | Pass | Current entry count controls timeline visibility; current retention candidate count controls candidate list and cleanup review visibility. | Timeline store, retention planner/presenter, restore handlers, and purge handlers are unchanged. |
| Destructive-operation safety | Pass | Three new presentation helpers contain no pipeline, restore, or purge calls; existing confirmations and operation pipelines remain authoritative. | Final package remains `BlockedUntilValidSameSignerPackage`. |
| Frontend, accessibility, and UX | Pass | Final screenshot/UI tree show quarantine policy plus stable empty conclusion; candidate/list/action/technical descendants are absent without evidence. | Populated states retain existing stable list, restore, and technical-detail AutomationIds. |
| Testing and verification | Pass | TDD red; focused 2/2; related 224/224; full 977/977; Release build 0 warnings/errors; 355 strict UTF-8 C#/XAML files; 17/17 XAML parse; test window closed. | A clean build required refreshing restore assets with `NuGetAudit=false`; no dependency version changed. |
| Operations, dependencies, and release | Warn | Final package/ZIP `.artifacts/OMNIX-Entropy-test-20260718-223259`; ProductionOnly read-only view verified. User reports antivirus definitions are updated. | App/worker remain unsigned; positive restore/purge acceptance still requires valid same-signer packaging and a disposable environment. |

### 2026-07-18 - Migration plan decision hierarchy

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Final first view contains no fixture/user path, manifest filename, or raw byte count; those remain local under technical details. | No cloud transfer, inventory beyond the process fixture, or raw exception display was added. |
| Data, API, and consistency | Pass | Decision summary derives from typed preview booleans/readiness and only coarse space wording; all original bindings remain under the expander. | Planner, gate, closure monitor, rollback writer, worker, and result synchronization are unchanged. |
| Destructive-operation safety | Pass | Unsigned preview hides both mutation-preparation buttons; signed readiness still controls visibility and existing enablement. | No evidence file, final consent, migration request, UAC, or move ran. |
| Frontend, accessibility, and UX | Pass | Real Release screenshot/UI tree show status, conclusion, D-drive target, next step, rollback, space, collapsed technical details, reminder, and Close in the first view. | Stable AutomationIds protect conclusion/next-step/rollback/expander placement. |
| Testing and verification | Pass | Focused 3/3; related 254/254; full 980/980; Release build 0 warnings/errors; 356 strict UTF-8 C#/XAML files; 17/17 XAML parse; all test windows closed. | Initial test compile errors were corrected and recorded before product acceptance. |
| Operations, dependencies, and release | Warn | Final package/ZIP `.artifacts/OMNIX-Entropy-test-20260718-224949`; ProductionOnly read-only migration preview verified. | App/worker remain unsigned; valid same-signer disposable migration and rollback acceptance remain required. |

### 2026-07-19 - Uninstall decision hierarchy

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Decision summary contains no fixture path or uninstall command; real acceptance used one process-scoped software fixture and workspace-local data/quarantine roots. | No cloud transfer, real installed-app inventory, or personal path was used. |
| Data, API, and consistency | Pass | Decision derives from the typed uninstall preview; original recovery, workflow, command, checklist, and section bindings remain in collapsed surfaces. The residue refusal derives from a fresh current-inventory scan. | Uninstall builder, scanner, post-scan model, and residue planner are unchanged. |
| Destructive-operation safety | Pass | Unsigned preview removes the preparation expander from the UI tree; existing signing, snapshot, final consent, coordinator, worker, and quarantine pipeline remain unchanged. | No uninstaller, evidence write, consent, UAC, quarantine, delete, registry, service, startup, task, or file mutation ran. |
| Frontend, accessibility, and UX | Pass | Computer Use captured the final first view with five path-free decision answers and only two collapsed secondary entries. The still-installed residue result states refusal and exposes no mutation action. | Stable AutomationIds protect decision order and collapsed preparation/workflow/technical surfaces. |
| Testing and verification | Pass | Focused 3/3; related 397/397; full 983/983; Release build 0 warnings/errors; 357 strict UTF-8 C#/XAML files; 17/17 XAML parse; all test windows closed. | One fixture compile mistake and one safety-copy regression were corrected and recorded before acceptance. |
| Operations, dependencies, and release | Warn | Final package/ZIP `.artifacts/OMNIX-Entropy-test-20260719-000736`; ProductionOnly read-only preview/refusal verified. | App/worker remain `NotSigned`; positive official uninstall and residue quarantine still require valid same-signer packaging and a disposable fixture. |

### 2026-07-19 - Disposable acceptance fixture kit

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Fixture identities are session-derived; no personal software/data is required; package/session manifests carry hashes only. | No cloud transfer and no fixture mutation ran on the current machine. |
| Data, API, and consistency | Pass | Real `SoftwareInventoryBuilder`, uninstall trust, scan rules, and `DiskRecommendationBuilder` integration tests prove cache/startup/cleanup attribution. | Session creation and receipt verification bind fixture manifest SHA-256. |
| Destructive-operation safety | Pass | Exact attestation and GUID required; provision preflights collisions and compensates; reset validates markers and does not follow reparse points; product packages exclude fixture payloads. | Exact `C:\Temp` is created only when absent; ownership mismatch refuses reset. |
| Frontend, accessibility, and UX | N/A | The fixture is a QA console package with documented operator commands, not user-facing product UI. | Product UI behavior is accepted separately through the ten-case protocol. |
| Testing and verification | Pass | Fixture 22/22; package 4/4; protocol 6/6; related 434/434; full 1026/1026; Release 0 warnings/errors; source integrity 367 files and 17/17 XAML. | Final verifier reports five payload files and manifest SHA-256 `07C033F1B445DCF1E171ABC18E8FAC3AD9ECDA1ADFDECC0603C22FB712FA4FA3`. |
| Operations, dependencies, and release | Warn | Final fixture package/ZIP `.artifacts/OMNIX-Acceptance-Fixtures-20260719-014314`; verifier says mutation attestation required and primary machine disallowed. Current product package/ZIP `.artifacts/OMNIX-Entropy-test-20260719-014731` contains 110 manifest files and zero fixture payloads. | Product App/worker remain unsigned and fail closed; positive fixture and product mutation behavior still require a signed candidate and checkpointed disposable Windows run. |

### 2026-07-22 - Read-only signing prerequisite inspection

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Inspector reads bounded tool paths and CurrentUser certificate public metadata only; no auto-selection, import, generation, trust change, signing, or network action exists. | Output lists only subject, thumbprint, expiry, private-key/code-signing eligibility for eligible candidates. |
| Data, API, and consistency | Pass | Inspector and signed-release transform both parse X509 EKU extension OID `2.5.29.37` and require code-signing OID `1.3.6.1.5.5.7.3.3`. | Current JSON cleanly separates store readability, tool discovery, eligible count, missing requirements, and final readiness. |
| Destructive-operation safety | Pass | Static contracts reject process launch, signing, file writes/moves/copies/removal, package install, certificate creation/import, and trust-store tokens. | Invalid explicit tool path reports refusal and does not fall back silently. |
| Frontend, accessibility, and UX | N/A | Release-operator script only; no product UI changed. | Latest Computer Use launch still timed out and remains Warn separately. |
| Testing and verification | Pass | Red 0/3; focused 4/4; signed-release plus inspector 8/8; full 1030/1030; Release 0 warnings/errors; source integrity 368 files and 17/17 XAML; changed scripts parse and are ASCII-only. | Child Windows PowerShell JSON test covers zero eligible certificates without treating the store as unreadable. |
| Operations, dependencies, and release | Warn | Current report: no `signtool.exe`, readable store, zero eligible signer certificates, `CanCreateSignedCandidate=false`. | Real signing and disposable acceptance remain external release gates. |

### 2026-07-22 - Repeated external release blocker

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | No certificate creation/import, trust change, security-setting interaction, UAC automation, or primary-machine fixture mutation was attempted. | The ordinary localhost certificate remains ineligible. |
| Data, API, and consistency | Pass | Fresh inspector JSON matches the prior report: tool absent, store readable, eligible count zero, readiness false. | No contradictory local state was found. |
| Destructive-operation safety | Pass | Product mutation remains fail closed; fixture mutation remains restricted to exact disposable attestation. | No bypass path was introduced. |
| Frontend, accessibility, and UX | Warn | Latest Computer Use launch still timed out and no OMNIX window/process remained. | Valid signed-candidate visual acceptance is still unavailable. |
| Testing and verification | Pass | Last completed implementation gate remains full 1030/1030, Release 0 warnings/errors, source integrity 368 files and 17/17 XAML. | This continuation changed records only after the fresh read-only prerequisite audit. |
| Operations, dependencies, and release | Fail | No Windows SDK `signtool.exe`, no eligible approved code-signing certificate, and no checkpointed disposable Windows run. | Goal cannot truthfully complete until external prerequisites change. |
