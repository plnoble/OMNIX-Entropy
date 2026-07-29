# Archived quality-gates (2026-07-16 to 2026-07-19)

Historical entries moved out of `.omx/development/quality-gates.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

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
