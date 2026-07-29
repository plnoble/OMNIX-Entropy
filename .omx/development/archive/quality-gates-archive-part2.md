# Archived quality-gates (2026-07-10 to 2026-07-16)

Historical entries moved out of `.omx/development/quality-gates.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

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

