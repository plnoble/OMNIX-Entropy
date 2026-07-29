# Quality Gates

## 2026-07-29 - Managed install-layout closure

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User asked Codex to implement the source and release work; machine inspection was read-only and installer execution remains a user action. | 0.1.3 publication is pending. |
| Current installation | Pass | Host evidence: correct 0.1.2 exe/registry location, valid expected signer and GlobalSign timestamp, fixed D drive, non-reparse directories, no doubled executable. | A future release is still needed to exercise self-update. |
| Installer layout | Pass | `AppendDefaultDirName=no` plus focused source contract; doubled-layout test uses the real Windows path policy. | Real compiled 0.1.3 setup has not yet been inspected. |
| Failure classification | Pass | Internal typed layout exception; downloader returns exact reinstall guidance and performs zero HTTP requests for the malformed layout. | Current 0.1.2 binary still has the old generic copy; this lands in 0.1.3. |
| Security boundary | Pass | Exact managed layout, D-first fixed-drive, reparse, hash, signer, confirmation, and pipeline checks are not relaxed. | First real end-to-end update remains pending. |
| Tests and build | Pass | TDD red was specific; focused installer/update/release-page contracts pass 16/16; full Debug 1068/1068; Release build 0 warnings/errors; source integrity 385 files/18 XAML; diff check clean. | GitHub CI is pending. |
| Records and handoff | Pass | Corrected machine state recorded; 15 archive EOF whitespace findings fixed mechanically; ignoring pure blank-line changes leaves no archive diff against `fe2e012`. | Final release evidence still needs a completion update. |
| Release | Warn | Version is 0.1.3 and beginner-facing release notes are present; source and local gates are ready for publication workflow. | Push/CI, package signing, download-back verification, and publication are pending. |

## 2026-07-29 - Released-state review and record readability

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User asked for a review, then approved fixing everything it found. Scope stayed at one WPF failure path, one cleanup-authority simplification, and record layout. | No update, signing, trust, installer, or system-mutation behavior was touched. |
| Evidence independence | Pass | Every recorded gate was re-derived on this machine and every public-release claim from GitHub, not read from the records. All matched. | A review confirms the released artifact's metadata, not its runtime behavior. |
| Security and privacy | Pass | Update chain re-read end to end: fixed repository/asset/URL pinning, double length check, hash check, signature before and after the final move, re-verification in the launch handler, zero-argument launcher, and package signer anchored to the running App's signer. No secret, path, or machine evidence entered the records. | Personal-certificate reputation and GitHub availability remain external. |
| Destructive-operation safety | Pass | No installer launch, product process, certificate, registry, service, or trust action occurred. The `finally` guard is now the single cleanup authority and behavior is unchanged. | Cleanup still cannot be proven against a same-user filesystem race. |
| Data and consistency | Pass | Record split proven lossless: all 1,724 entry blocks across seven records compared against `HEAD`, every block present verbatim in a live or archive file. | Archives are chronological, so within-file ordering differs from the original two-directional layout. |
| Beginner UX and accessibility | Pass | The new release-page failure path reports a path-free conclusion stating nothing was downloaded or installed, matching the project rule against raw exception text in beginner-visible controls. | No screenshot; Computer Use was not retried this slice, so visual evidence remains Warn overall. |
| Tests and build | Pass | Source integrity 385 files, InvalidUtf8 0, ReplacementFiles 0, 18/18 XAML; Release build 0 errors and 0 warnings; full Debug 1067/1067; focused update contracts 11/11. | Release warnings were 0 rather than the recorded 18 only because the NuGet vulnerability index was reachable. |
| Records and handoff | Pass | Live records now open under the standard read path; largest is 62 KB against a 256 KB limit. `handoff.md` is 13 KB and its canonical sections describe present state instead of the resolved 2026-07-22 blocker. | `AGENTS.md` still has no automated size gate; the prevention rule is written but unenforced. |

## 2026-07-28 - In-app verified update installation

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User reported that check-only behavior is insufficient; implementation adds user-started download and confirmed interactive setup only. | No installer has run; installation remains a visible user action. |
| Source and transport identity | Pass | Fixed repository/channel policy remains; exact package URL, declared length, SHA-256, expected signer, and current running App signer are all required. | GitHub availability remains external. |
| D-first storage | Pass | `WindowsPersonalUpdatePathPolicy` derives `Updates\v<version>\<random>` beside a managed `OMNIX-Entropy\Install` on a fixed non-system drive and rejects reparse paths. | Verified packages remain on D until a later retention policy removes them. |
| Destructive-operation safety | Pass | Final launch descriptor is High/destructive/confirmed; `SafetyOperationPipeline` gates a handler bound to the real current executable; handler revalidates path/length/hash/both signatures and passes zero arguments to the interactive launcher. | Installer behavior remains separately visible and user-controlled; automatic downgrade is absent. |
| Failure and cancellation | Pass | Tests cover length/hash mismatch, signer mismatch, final-move recheck cleanup, post-download mutation, alternate-current-executable substitution, missing confirmation, and cancellation. | Same-user filesystem races are reduced by random staging and repeated verification but cannot be eliminated before process creation. |
| Beginner UX and accessibility | Warn | Stable `DownloadAndInstallUpdateButton`; clear download/verified/failure copy; release-page fallback; XAML parses and static UX contract passes. | Computer Use app approval timed out before launch, so no current screenshot is claimed. |
| Tests and build | Pass | Focused update/UX 8/8; full Debug 1067/1067; Release 0 errors; integrity 385 files/18 XAML; diff check clean. | Release build has 18 environmental NU1900 warnings because NuGet vulnerability metadata is unreachable. |
| Release | Pass | Source `4a3e30c3feacdeaf8fcc1df6543ba14f1ddc6125` passed CI `30339140502`; signed payload/setup verification passed; all four draft assets were downloaded back and matched; downloaded setup verification returned `CanStageGitHubRelease=true`; public latest metadata reports non-draft/non-prerelease `v0.1.2`; setup SHA-256 `C5D861160E3A38367B38F8FA9473FA6EB7D06485EF5202B1D4A5E7A3E76912C1`. | 0.1.0/0.1.1 still require one manual bootstrap install; setup was not launched on the development machine. |

## 2026-07-28 - 0.1.1 public release preparation

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User explicitly requested public release 0.1.1; current scope excludes installer execution, product installation, trust changes, private-key export, antivirus changes, and LocalMachine actions. | Public publication remains irreversible in practice even if a release is later deleted. |
| Version and source | Pass | `Css.App.csproj` reports 0.1.1; release notes cover the current system-footprint slice; release tag `v0.1.1` points to `63fbb5e868bbf2231ac8236b2b5d577e816ddfce`; channel manifest records the same commit/version. | The records-only completion commit is intentionally newer than the immutable release tag. |
| Tests and build | Pass | Focused 183/183; full Debug 1060/1060; Release build 0 errors; source integrity 383/18; five release scripts parse. | Release build emitted 18 NU1900 warnings because NuGet vulnerability metadata was unreachable. |
| Signing identity | Pass | Approved host-session read-only query found thumbprint `5688958FEA0056861558E8DCF9D2381AF46074B2` with private key only in CurrentUser My and public copies in the three authorized CurrentUser stores. | Restricted commands cannot see the host signer; final signing must run in the approved host context. |
| Supply chain and privacy | Pass | Fixed repository and explicit tool paths rechecked; secret/material audit found no signing material; final four-asset inventory was uploaded, downloaded back, and matched by SHA-256. | Public certificate identity and release hashes are intentionally auditable. |
| Source publication | Pass | Commits `46374e9`, `6053d7b`, `ba1df22`, and `63fbb5e` were pushed; CI runs `30333084515`, `30333783060`, `30334445147`, and `30334942521` passed all gates. | Local Release build retained 18 environmental NU1900 warnings. |
| Installer | Pass | Final 110-file payload and 0.1.1 setup passed same-signer, timestamp, hash, D-first, visible-directory, no-silent-install, and independent verification gates; setup SHA-256 is `F581F89A93E36145E8C6952E5C7D4B0F9E32C7A6EDC4F76C13E9E1B2F6ADACBB`. | Setup has not been launched, installed, or uninstalled; disposable ten-case behavioral acceptance remains pending. |
| Timestamp supply chain | Pass | Exact GlobalSign R45 fallback commit `ba1df22` passed CI; final payload/setup have valid independently observed RFC3161 timestamps from the allowlisted services. | Timestamp services remain external dependencies for future releases. |
| Timestamp resilience | Pass | Commit `63fbb5e` adds at most three SignTool attempts with ten-second waits and unchanged mandatory post-sign checks; CI passed and the real final candidate exercised retries before succeeding. | A complete outage still fails the release closed, by design. |
| Release | Pass | Public non-draft/non-prerelease `v0.1.1` exists at `https://github.com/plnoble/OMNIX-Entropy/releases/tag/v0.1.1`; unauthenticated latest metadata reports four assets; all assets were downloaded and reverified before publication. | GitHub availability and SmartScreen public reputation are external to the personal signing chain. |

## 2026-07-28 - RogueCleaner-inspired system-footprint diagnosis

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| External source and license | Pass | Reviewed upstream README/source at `e498db3` and MIT license; `docs/research/roguecleaner-reference.zh-CN.md` records adopted and rejected ideas. | Upstream rules and behavior may change; no automatic synchronization exists. |
| Data and ownership | Pass | Builder tests prove install-path/full-name correlation and unrelated evidence refusal; uncorrelated entries are not assigned to an app. | Conservative matching can miss integrations that expose neither app path nor full name. |
| Destructive-operation safety | Pass | New scanner contains no registry writes/deletes, process launch, service/task commands, operation descriptors, or handler registration; UI states that entries are not automatically deleted. | A future removal feature would require a separate risk review and rollback-capable pipeline. |
| Beginner UX and accessibility | Pass | Stable `DrawerSystemFootprintTextBlock` AutomationId, static order test, path-free summary, collapsed technical evidence, and inspected `.omx/qa-app-system-footprint.png`. | Dense drawers still depend on scrolling at smaller heights. |
| Testing and build | Pass | Focused 6/6; full Debug 1060/1060; Release build 0 errors; integrity 383 files/18 XAML; new smoke script parser pass. | Release emitted 18 NU1900 warnings because NuGet vulnerability metadata was unreachable. |
| Runtime scope | Pass | Isolated fixture GUI smoke displayed right-click/browser counts and Observe-only Agent advice; fixture/data were removed afterward. | Real-machine entry coverage was not used as correctness proof; protected registry areas can make scans partial. |

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


## Archived History

Entries before 2026-07-22 were moved verbatim to [quality-gates-archive-part1.md](archive/quality-gates-archive-part1.md), [quality-gates-archive-part2.md](archive/quality-gates-archive-part2.md), [quality-gates-archive-part3.md](archive/quality-gates-archive-part3.md).
