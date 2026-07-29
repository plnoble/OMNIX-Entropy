# Decision Log

## 2026-07-29 - Installer guarantees the managed layout; updater keeps refusing malformed layouts

- Choice: set `AppendDefaultDirName=no` so Inno's Browse dialog cannot append a second `Install`, while preserving the exact `OMNIX-Entropy\Install` updater layout check.
- Choice: classify the layout mismatch with an internal typed exception and map only that type to beginner-visible reinstall guidance.
- Rejected: relaxing `ResolveProductRoot`, accepting arbitrary product roots, matching the English exception message, or treating every staging failure as an installation-layout problem.
- Consequence: a future installer cannot reproduce the observed doubled path; old or manually moved malformed installations still fail closed with a useful recovery action; unrelated failures retain the generic boundary.

## 2026-07-29 - Split records by date, never by position

- Choice: select archived entries by the date in each heading, keep undated structural sections in the live file, and write archives chunked to stay under the read limit.
- Rejected: cutting each file at a line offset. The records had grown in two directions at once — newest-first at the top, oldest-first in the tail — so a positional cut would have archived recent entries while retaining old ones.
- Rejected: archiving whole files and starting empty ones. That would have deleted the reusable Pre-Change Gate, Pre-Delivery Gate, and entry templates, which are operative policy referenced by `AGENTS.md`, not history.
- Rejected: one archive file per record. `current.md`'s history alone is about 500 KB, so a single archive would have reproduced the unreadability it was meant to fix.
- Consequence: the live records start at 2026-07-22, the public-release boundary. Anything older is one link away and still tracked, since `.gitignore` allows `!.omx/development/**`.

## 2026-07-29 - A cleanup guard should have one authority

- Choice: `DownloadAndVerifyAsync` cleans up staging only in its `finally` guard, and the guard says so in a comment.
- Rejected: keeping the same call in all five catch blocks. It was already unreachable as an effect, since `finally` runs afterwards with `retainVerifiedPackage` false on every catch path, and duplication makes a safety property harder to confirm by reading.
- Consequence: reviewers confirm one exit path instead of six. The behavior is unchanged, which the focused update contracts and the full suite both confirm.

## 2026-07-28 - Retry timestamp transport, never timestamp validation

- Decision: payload signing gets at most three SignTool attempts per executable, with ten seconds between failures; final Authenticode, thumbprint, and timestamp checks remain unchanged.
- Rejected: unbounded retry, publishing a signature without a timestamp, retrying into a reused candidate after the script exits, or weakening post-sign verification.
- Consequence: short TSA outages no longer force manual whole-chain retries as often, while a persistent outage still fails the candidate closed.

## 2026-07-28 - Add exact GlobalSign R45 timestamp fallback

- Decision: allow the exact official HTTP endpoint `timestamp.globalsign.com/tsa/r45standard` in both payload and installer signing scripts, alongside the existing exact DigiCert endpoint and the existing HTTPS policy.
- Evidence: GlobalSign's current SignTool guide documents the R45 URL; a real isolated probe returned a Valid Authenticode signature with a `GlobalSign R45 TSA for CodeSign 202510` timestamp certificate.
- Rejected: changing the DigiCert scheme by guess, accepting arbitrary HTTP timestamp hosts/paths, publishing without a timestamp, or reusing any partially signed output directory.
- Consequence: 0.1.1 can use a verified independent TSA during DigiCert instability without weakening the HTTP allowlist.

## 2026-07-28 - Borrow RogueCleaner evidence categories, not its verdicts or cleaner

- Decision: add right-click, Explorer, browser-host, and file-association presence as read-only `SoftwareProfile` evidence, correlated only by install path or full application-name evidence.
- Decision: present these as “where the app also appears,” explicitly state that presence is not malware, and keep source locators behind technical details.
- Rejected: copying upstream source/vendor lists, assigning risk from brand keywords, bulk-selecting entries, or adding direct registry/service/task cleanup. MIT permission does not replace product safety review or provenance requirements.
- Consequence: OMNIX gains a useful RogueCleaner-style diagnostic layer without broadening mutation authority. Post-uninstall footprint rescan is the next safe extension; entry removal remains separately gated.

## 2026-07-23 - Use Inno Setup for visible D-first personal installation

- Decision: use an authored Inno Setup definition and the official compiler instead of a custom privileged bootstrapper. Always expose the directory page, default to D, reject silent setup, and use one explicit Authenticode signer for App, worker, setup, and uninstaller.
- Rejected: Velopack because its normal Windows ownership defaults to `%LocalAppData%` on C; MSIX because install-drive choice is not controlled in the required beginner flow; a hand-written installer because it would add avoidable privileged file-replacement authority.
- Consequence: repository policy and release staging are complete, but producing a real setup requires the external Inno compiler and an explicitly approved personal signer.

## 2026-07-23 - Official HTTP timestamping is an exact provider exception

- Decision: retain HTTPS as the general timestamp transport rule, but permit only `http://timestamp.digicert.com` with the default port, root path, no query, no fragment, and no user information.
- Rejected: weaken the check to any HTTP URL, use an undocumented HTTPS variant, omit the timestamp, or treat a successful SignTool exit as sufficient without post-sign verification.
- Consequence: the scripts match DigiCert's supported RFC3161 interface while keeping the network destination bounded and preserving signed timestamp verification.

## 2026-07-23 - Root trust requires separate consent

- Decision: fail the personal release build when the self-signed signer does not form a Windows-trusted Authenticode chain, and require separate explicit consent before adding its public certificate to CurrentUser Root.
- Rejected: interpret TrustedPeople/TrustedPublisher approval as Root approval, accept `UnknownError` based only on thumbprint, add LocalMachine trust, or publish a signed-but-untrusted package as valid.
- Consequence: no installer is falsely labeled ready; the persistent trust expansion remains visible and reversible, and production same-signer gates keep requiring Windows `Valid`.
# 2026-07-28 - Verified interactive updates require one manual bootstrap

- Decision: release the first complete in-app updater as 0.1.2 and state plainly that 0.1.0/0.1.1 must install it once from the GitHub release page.
- Chosen flow: user-started check -> user-started D-drive download -> exact length/SHA-256/current-App same-signer verification -> final confirmation -> `SafetyOperationPipeline` -> no-argument interactive installer -> App exit after successful launch.
- Rejected: pretending check-only 0.1.0 can self-acquire new code; replacing the already-public 0.1.1 asset; downloading to C; silent setup arguments; trusting GitHub metadata without anchoring to the running signer; launching directly from the WPF handler.
- Consequence: the first upgrade remains manual, but every later compatible release can use the same audited local flow without weakening Windows or OMNIX trust checks.
## 2026-07-22 - Public CI must reproduce from tracked files with deterministic scheduling

- Decision: version the top-level `.omx` PowerShell smoke contracts that tests consume, enforce LF through `.gitattributes`, build required Release outputs before Debug tests, and disable xUnit collection parallelization for this Windows integration-heavy suite.
- Rejected: keep smoke scripts as local evidence, normalize line endings inside individual tests, or retry timed-out parallel tests. Each option would preserve a mismatch between local and public evidence or hide nondeterminism.
- Consequence: a fresh public checkout is self-contained and runner-stable; the 1048-test suite takes longer because collection parallelism is disabled.

## 2026-07-22 - Personal GitHub updates keep same-signer trust and D-drive ownership

- Decision: use GitHub Releases for version transport, a local-only personal Authenticode key for publisher identity, and an app-side fixed-repository metadata check. CI never receives the private key, and the release helper creates drafts only.
- Decision: accepting a Windows SmartScreen warning does not authorize removing OMNIX's valid-same-signer check for the privileged worker.
- Decision: reject Velopack for the installer/update layer because its standard Windows installation goes to `%LocalAppData%`; the product promise requires a selectable installer defaulting to `D:\Software\OMNIX-Entropy\Install` when D is available.
- Consequence: the current UI can check and explain an update but deliberately cannot download or install it yet. The next slice must add a D-first installer, same-signer installer verification, explicit confirmation, and rollback retention before enabling that authority.

## 2026-07-22 - Discover Windows SDK from its registered install root

- Decision: after explicit path and PATH checks, read only the two standard Windows Kits `Installed Roots` keys, validate `KitsRoot10`, and enumerate only direct version/architecture SignTool candidates beneath that root.
- Rejected: assume the SDK is under Program Files, hardcode `D:`, recursively search drives, or modify PATH/registry.
- Consequence: non-default official SDK installations are recognized without broad filesystem access, and the current machine's SignTool blocker is removed accurately.

## 2026-07-22 - Recent-install ordering uses explicit registry dates only

- Decision: parse uninstall-registry `InstallDate` only as `yyyyMMdd` or `yyyy-MM-dd`, preserve it as `DateOnly?`, and sort unknown dates after known dates without inference.
- Rejected: use registry key write time, executable timestamps, directory timestamps, or the first observation date as the installation date; each can change independently of installation.
- Consequence: the requested sort is available and deterministic while applications that do not publish reliable metadata remain honestly unknown.

## 2026-07-22 - V1 completion requires an evidence matrix, not suite size

- Decision: preserve every original feature in a completion audit and classify source connection, automated evidence, current-machine evidence, visual evidence, and signed disposable behavior evidence separately.
- Rejected: infer that 1000+ passing tests prove the whole V1, or call mutation workflows complete from fixtures alone.
- Consequence: one missing sort was found and fixed; external release/behavior gates remain visible instead of being hidden by the green suite.

## 2026-07-22 - Release signer is RSA-only

- Decision: prerequisite inspection, signed-package creation, manifest evidence, and transfer verification all require an RSA code-signing public key.
- Rejected: accept any certificate with the code-signing EKU; that would allow an ECC signer even though the target Smart App Control path does not currently support ECC signatures.
- Consequence: unsupported signer algorithms fail before candidate acceptance, and the manifest preserves the checked algorithm for independent transfer-time verification.

## 2026-07-22 - Document only the signing route the repository implements

- Decision: the beginner guide describes the existing local `CurrentUser\My` certificate plus Windows SDK SignTool route and names Store/cloud signing only as unsupported by current scripts.
- Rejected: present Microsoft Store, Azure Artifact Signing, SignPath, self-signed certificates, or trust-store changes as interchangeable steps.
- Consequence: the operator path matches executable parameters and does not invite secrets, fake trust, or a release status the repository cannot verify.

## 2026-07-22 - Signing readiness is read-only discovery, never signer selection

- Decision: report tool availability and every eligible CurrentUser code-signing certificate, but require a separate explicit thumbprint when invoking the signing transform.
- Rejected: auto-select the only certificate, generate/import a development certificate, install SDK components, search drives recursively, or call the signing transform from the inspector.
- Consequence: missing prerequisites are machine-readable without turning local certificate presence into user authorization or weakening the same-signer release gate.

## 2026-07-22 - Code-signing EKU is parsed from the X509 extension

- Decision: inspect OID `2.5.29.37` as `X509EnhancedKeyUsageExtension` and require code-signing OID `1.3.6.1.5.5.7.3.3` in both prerequisite and signing scripts.
- Rejected: depend on provider-specific `EnhancedKeyUsageList`, accept certificates with no EKU, or let the inspector and signer apply different eligibility rules.
- Consequence: Windows PowerShell 5.1 reports ordinary certificates as ineligible rather than treating the certificate store as unreadable, and the final transform rechecks the identical security condition.


## Archived History

Entries before 2026-07-22 were moved verbatim to [decisions-archive-part1.md](archive/decisions-archive-part1.md), [decisions-archive-part2.md](archive/decisions-archive-part2.md).
