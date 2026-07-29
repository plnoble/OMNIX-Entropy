# Archived worklog (2026-07-11 to 2026-07-19)

Historical entries moved out of `.omx/development/worklog.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

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

## 2026-07-19 - Uninstall decision hierarchy completed

- Real Release inspection found that the unsigned uninstall preview still asked the beginner to choose an installer, inspect restore points, acknowledge backup state, and face a disabled final-checklist action before giving one decision.
- Added `UninstallPlanDecisionSummaryPresenter` with path-free explanations for current state, official-uninstall flow, residue handling, undo limits, and next step.
- Reordered the preview so the Agent decision is first; preparation, complete workflow, and technical evidence are collapsed. Unsigned packages hide the preparation expander and replace its next step with the signed-release requirement.
- The same isolated fixture proved the existing post-uninstall residue refusal: because the application is still detected, OMNIX does not classify its files as residue and exposes no quarantine/delete action.
- Computer Use exercised preview, Close, and read-only residue rescan only. No official uninstaller, evidence writer, final consent, UAC, quarantine, registry, service, startup, task, or file mutation ran; all package windows were closed.
- Final package: `.artifacts/OMNIX-Entropy-test-20260719-000736` and ZIP; ProductionOnly, App/worker `NotSigned`, mutation blocked. Verification: focused 3/3; related 397/397; full 983/983; Release build 0 warnings/errors; source integrity 357 files, 17/17 XAML parse.
