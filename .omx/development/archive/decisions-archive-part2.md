# Archived decisions (2026-07-15 to 2026-07-19)

Historical entries moved out of `.omx/development/decisions.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

## 2026-07-15 - Direct startup control is limited to one exact HKCU64 Run value

- Decision: V1 local startup mutation will support only one uniquely correlated structured `HKCU64\Software\Microsoft\Windows\CurrentVersion\Run` value for a non-system application. HKLM, Run32, services, scheduled tasks, multiple candidates, and name-only evidence stay read-only or hand off to Windows Settings.
- Rejected: decode/write undocumented `StartupApproved` binary payloads; broadly delete every startup/service/task associated by app name; make the existing settings shortcut look like local execution.
- Consequence: the first local control is deliberately narrow, but its blast radius is one exact value and it can carry trustworthy rollback evidence.

## 2026-07-15 - Preserve StartupApproved as observation, never mutation authority

- Decision: bind the current `StartupApproved` status/fingerprint into freshness checks, but do not interpret it as enabled/disabled and do not modify it. Disable is represented by removal of the exact Run value after an atomic rollback snapshot.
- Consequence: Windows may retain an approval record after disable; that is acceptable evidence, not a control surface. Apps that recreate their Run value are detected by re-scan instead of being silently fought.

## 2026-07-15 - Startup GUI proof uses an exact in-memory adapter and cancel-first windows

- Decision: use a process-scoped `OMNIX_ENTROPY_STARTUP_FIXTURE` adapter that reconstructs and verifies one structured observation, mutates only in-memory state, and shares the production preparation/presentation/pipeline contracts. GUI smokes may open disable/restore confirmations but invoke only cancel.
- Rejected: create or delete a real Run value during ordinary automated verification; weaken the production adapter behind a debug branch; claim source/static tests are visual proof.
- Consequence: WPF reachability, first-view wording, confirmation state, manifest cleanup, and timeline restore UI are repeatable without changing the user's registry. Positive Win32 mutation remains a disposable-account release check.

## 2026-07-15 - Timeline restore confirmation is operation-specific and path-free

- Decision: dispatch startup and quarantine restore semantics before confirmation. Startup copy explains next-login behavior and untouched services/tasks; manifest paths remain technical evidence and do not appear in the confirmation first view.
- Rejected: reuse the old `按隔离区 manifest 还原` message for registry startup values or display raw manifest paths in the beginner confirmation.
- Consequence: the same timeline button remains predictable while each restore type tells the truth about what will change.

## 2026-07-15 - Quarantine confirmation binds an object identity, not only a path

- Decision: before any low-risk quarantine confirmation, bind canonical path, type, volume/file identity, creation/write metadata, and file length; revalidate the whole batch after consent and each item again immediately before its move.
- Rejected: trust the scan-time path string, check only `File.Exists`/`Directory.Exists`, or let each item discover staleness after earlier batch items have already moved.
- Consequence: recreated files, changed directories, reparse chains, duplicates, overlaps, protected roots, and source/quarantine overlap fail closed. A changed batch asks for a fresh scan instead of moving the new object under an old recommendation.

## 2026-07-15 - File identity remains a Windows adapter responsibility

- Decision: keep the evidence contract and comparison in Core, but read Windows stable identity through an explicit Win32 handle adapter using the exact native structure layout.
- Rejected: infer identity from name/size alone, put `kernel32` calls in WPF, or make direct file movement responsible for constructing pre-consent evidence.
- Consequence: UI and Core tests can inject the boundary, production uses current Windows identity, and native layout mistakes are covered by replacement/stability tests.

## 2026-07-15 - Agent eligibility is navigation guidance, never operation evidence

- Decision: the Agent may describe a local reversible startup review only when a profile has exactly one presentation-eligible supported ordinary Run observation; actual disable still performs a fresh exact read, evidence binding, confirmation, and pipeline execution in the drawer workflow.
- Rejected: let the Agent's cached profile authorize registry mutation, promise local control for name-only/service/task signals, or keep directing every exact supported case to Windows Settings.
- Consequence: the beginner receives a truthful useful next step without expanding the Agent's authority or weakening stale-evidence checks.

## 2026-07-15 - Agent answer view returns to a stable top position

- Decision: after rendering an answer, reset the conversation ScrollViewer to the top instead of calling `BringIntoView` on a response panel taller than the viewport.
- Rejected: accept a partially clipped `Computer Agent` heading or rely on compositor timing to choose an arbitrary minimal offset.
- Consequence: the identity, question, conclusion, and safety context share the first visible area, and GUI proof can assert stable placement.

## 2026-07-15 - Disk GUI fixtures must use the volume named by the UI

- Decision: the Home C-drive smoke uses one GUID-named directory under `C:\tmp`, while app data remains isolated under `.omx`; screenshots capture only the OMNIX window.
- Rejected: scan a D-volume workspace fixture while presenting its capacity as C, scan arbitrary real C-drive content, or retain a desktop-wide screenshot containing unrelated windows.
- Consequence: the scan tree stays disposable and bounded while capacity/usage evidence matches the beginner-facing drive label and visual evidence contains only product UI.

## 2026-07-15 - Personal-file findings remain metadata-only and non-executable

- Decision: V1 may identify long-unused large-file candidates and same-name/same-size possible-duplicate groups from bounded scan metadata, but it will not read/hash content or create cleanup operations from these findings.
- Rejected: call filename/size similarity a confirmed duplicate, automatically quarantine personal files, or lower production thresholds to make a fixture easier to see.
- Consequence: users receive useful review candidates without OMNIX deciding that personal content is disposable. The lower 8 KB/4 KB thresholds exist only when an explicitly validated development fixture root is present.

## 2026-07-15 - Beginner text is sanitized without weakening operation evidence

- Decision: remove fixed local paths at the shared recommendation presentation boundary while retaining exact paths in the underlying operation descriptor and collapsed technical evidence.
- Rejected: erase paths from the safety pipeline, expose full paths in first-level recommendation cards, or patch only the personal candidate list while leaving the adjacent panel unchanged.
- Consequence: the beginner view protects privacy and stays readable; confirmation/revalidation code still receives the exact evidence it needs.

## 2026-07-15 - Exact navigation must land on visible candidate content

- Decision: Home personal-storage details scroll to `PersonalStorageFindingsListBox`, not only its summary heading, and GUI acceptance requires every bounded fixture item to report onscreen before capture.
- Rejected: treat an AutomationId in the UI tree or a barely visible section heading as proof that the requested details were opened.
- Consequence: the action now lands where the user can immediately read the evidence, and screenshot review matches the interaction claim.

## 2026-07-15 - Install-report previews may navigate but never authorize execution

- Decision: only a ready preview with exactly one added software owner may carry a generic internal-navigation action to that application's current drawer; the drawer must rerun its own readiness and confirmation workflow.
- Rejected: execute cache/startup/migration work from snapshot-diff evidence, navigate refused/guidance-only previews, or trust a stale report name without exact current-inventory resolution.
- Consequence: installation evidence now has a usable next step while remaining structurally separate from operation authority. Missing or duplicate current targets fail closed.

## 2026-07-15 - Safe next actions belong beside the Agent conclusion

- Decision: put the install-preview application handoff immediately after the Agent takeaway and require both to intersect the real scroll viewport in GUI acceptance.
- Rejected: bury the next action after detailed plan/missing-evidence lists or accept UIAutomation discovery as proof of first-view usability.
- Consequence: a beginner can understand the conclusion and act on it without searching the lower edge of a long technical flow.

## 2026-07-15 - Natural-language questions resolve only to fixed shortcut identities

- Decision: classify common settings/troubleshooting wording locally, then carry only a typed shortcut kind and an id from the existing settings/system-tool catalogs.
- Rejected: derive a command or URI from user text, let the Agent call `Process.Start`, or treat a question as consent to modify a setting.
- Consequence: the Agent can provide a useful exact next step while malformed or unknown ids fail closed and all real opening behavior remains in one allowlisted boundary.

## 2026-07-15 - Troubleshooting answers admit uncertainty before opening evidence

- Decision: driver, crash, and blue-screen answers state that one sentence is insufficient to determine root cause and offer a read-only Windows evidence surface.
- Rejected: claim a diagnosis, recommend bulk driver replacement, or disable/uninstall a device from the conversation surface.
- Consequence: beginner guidance is actionable without pretending observation is proof or expanding the Agent into an automatic repair authority.

## 2026-07-15 - Protected-tool GUI proof closes the exact confirmation window

- Decision: find the native MessageBox by exact allowlisted title, capture it, and use `WindowPattern.Close()` so the test follows the cancel branch without depending on localized button automation.
- Rejected: select the first non-main process window, invoke a guessed button, click OK, or launch Device Manager during ordinary verification.
- Consequence: the smoke proves the real warning and cancel path while guaranteeing no external tool launch; screenshot review remains a required independent gate.

## 2026-07-15 - Hardware configuration is a manual-scan evidence type

- Decision: capture a bounded hardware summary only during the existing user-initiated health scan, carry it as structured evidence, and answer hardware questions from that evidence.
- Rejected: query hardware on every keystroke/question, add a background monitor, or parse formatted Home text back into authority.
- Consequence: the result has a clear observation time, remains read-only, survives migration-health enrichment, and is unavailable honestly until a manual scan succeeds.

## 2026-07-15 - Hardware identity fields are excluded and compatibility claims require requirements

- Decision: retain only sanitized CPU/GPU names, logical processor count, Windows caption/version, and architecture; never query serials, usernames, domains, PNP/device ids, or raw registry locators. Software/game advice requires official minimum/recommended requirements.
- Rejected: collect broad WMI objects, expose hardware identifiers, benchmark silently, or infer that a named GPU guarantees performance.
- Consequence: the Agent can explain the machine's basic configuration without creating a fingerprinting dataset or making unsupported purchasing/performance claims.

## 2026-07-15 - Hardware probing has read-only provider fallbacks

- Decision: use fixed bounded WMI queries first, then one fixed CPU hardware registry read and bounded `EnumDisplayDevices` fallback when WMI is denied or unhealthy.
- Rejected: require administrator rights, shell out to `wmic`/PowerShell, enumerate arbitrary registry trees, or treat provider failure as an empty successful value.
- Consequence: standard/restricted users still receive useful evidence while every source remains local, read-only, bounded, and non-executable.

## 2026-07-15 - Skill cards ask the Agent; they do not execute the skill

- Decision: every catalog card has one compact `问 Agent` action that passes only its enum category to a local presenter and renders the ordinary Agent response panel.
- Rejected: make card clicks consent, call `ShowPage`/Windows tools directly, hide actions in list selection behavior, or create a separate execution path per skill.
- Consequence: the catalog is usable and predictable while all real next steps remain explicit responses subject to existing internal/shortcut allowlists.

## 2026-07-15 - Unsupported skill categories are visible but explicitly unavailable

- Decision: window/desktop and input/session cards explain exactly which evidence/actions V1 lacks and expose no navigation action.
- Rejected: remove the categories without explanation, label them usable because Marvis has them, or generate generic plans without observing windows/session state.
- Consequence: the roadmap remains visible without misleading beginners into expecting desktop rearrangement, lock, sleep, shutdown, or restart authority.

## 2026-07-15 - MSIX routing hands off to one Windows-managed storage setting

- Decision: a trusted MSIX capability may carry only the fixed `default-save-locations` catalog id, which resolves to Microsoft's documented `ms-settings:savelocations` page through the existing confirmation-aware open-only boundary.
- Rejected: launch the package, pass an arbitrary D-directory argument, expose a URI from package/user text, modify `ProgramFilesDir`, or let MSIX remember an OMNIX folder rule.
- Consequence: beginners get a real next step without OMNIX claiming control that Windows does not provide. The package remains non-launchable and the user still makes any setting change in Windows.

## 2026-07-15 - Managed-storage copy replaces irrelevant route copy

- Decision: for `WindowsManagedStorage`, show `由 Windows 决定` instead of the generic recommended D path and use a distinct non-preparable readiness state.
- Rejected: reuse `证据不足` or keep the D-path recommendation visible beside a statement that it cannot be applied.
- Consequence: the UI no longer contradicts itself, and code can distinguish a safe settings handoff from an untrusted/refused package.

## 2026-07-15 - Recycle Bin diagnosis hands off to review, never clearing

- Decision: specialize positive Recycle Bin size evidence and expose one fixed open-only `RecycleBinFolder` viewer through the existing system-tool allowlist.
- Rejected: call `SHEmptyRecycleBin`, create a cleanup operation from size alone, move Recycle Bin internals into OMNIX quarantine, or label the files safely disposable.
- Consequence: a beginner gets a concrete next step while Windows remains the only surface where the user can inspect, restore, or choose to clear items.

## 2026-07-15 - Destructive wording is not destructive consent

- Decision: questions containing `清空回收站` resolve to a review-only explanation and fixed viewer id; the response explicitly states clearing usually cannot be undone.
- Rejected: infer consent from a sentence, pass question text into shell arguments, or expose a direct clear button beside the diagnosis.
- Consequence: natural language can help find evidence but cannot cross the deletion boundary.

## 2026-07-15 - Root-cause cards navigate or select; they never execute

- Decision: map only ordinary user-profile, programs/app-data, and temp categories to existing internal evidence surfaces, with typed action-specific AutomationIds and card/action revalidation.
- Rejected: add cleanup logic to the root card, automatically run an app scan as a side effect, assign actions to unexpected roots/system stores, or treat selecting an actionable recommendation as final confirmation.
- Consequence: the diagnosis becomes usable for beginners while all mutation remains in the existing evidence, confirmation, pipeline, and rollback workflows.

## 2026-07-15 - Root-card runtime AutomationIds include a stable hashed identity

- Decision: combine the typed action name with the first eight hexadecimal characters of a SHA-256 over the normalized visible top-level name; keep the single Recycle Bin id fixed.
- Rejected: action-only ids that collide, unstable list indexes, raw path/name text in the id, or random GUIDs that change every scan.
- Consequence: repeated action types remain individually targetable and stable without exposing local paths.

## 2026-07-15 - Automatic installer monitoring is the primary workflow

- Decision: keep the normal `准备安装` flow responsible for before/after evidence and place the manual three-step snapshot comparison inside a default-collapsed advanced diagnostic expander.
- Rejected: leave all three manual buttons in the ordinary page, remove the fixture capability entirely, or add a second beginner-facing execution path.
- Consequence: beginners see one understandable action while isolated diagnostics and existing evidence tests remain available on demand.

## 2026-07-15 - Agent prepares required read-only evidence before answering

- Decision: classify the local evidence needed by a question/skill, await the existing deduplicated software inventory gate only when relevant, then generate the answer from the refreshed in-memory profiles.
- Rejected: tell the beginner to navigate and scan manually, scan for every settings/tool question, or let natural language directly create/execute an operation.
- Consequence: Agent performs the harmless preparation work while mutation authority, final confirmation, and rollback remain in the existing local workflows.

## 2026-07-15 - Explicit C-drive questions may start one read-only diagnosis

- Decision: when current health evidence is absent, a clear C-drive intent may await the same bounded diagnosis used by the homepage; other intents do not trigger a full disk scan.
- Rejected: always return a navigation instruction, scan the full disk for every Agent question, allow concurrent scans, or reuse stale/failed evidence as success.
- Consequence: Agent can answer the product's central question from current evidence without gaining cleanup authority or surprising unrelated questions with a long scan.

## 2026-07-15 - Undo Center navigation ensures; actions refresh

- Decision: cache a successful first-entry read for repeated navigation, while the visible refresh command and all post-operation callers force a new read through the same in-flight gate.
- Rejected: reload on every tab click, remove manual refresh, cache failures, or put restore/permanent cleanup inside the load routine.
- Consequence: the page feels automatic without stale post-operation state or duplicate reads, and read authority remains separate from mutation authority.

## 2026-07-15 - Lightweight machine evidence is not a disk health summary

- Decision: pass `MachineHealthObservation` to Agent presentation separately, reuse its explicit availability states, and reserve `HealthCheckSummary.OverallScore` for a real disk-backed health session.
- Rejected: fabricate a neutral score, run the full C-drive scanner for hardware questions, or answer from CPU/GPU names alone without current evidence.
- Consequence: Agent can answer common configuration and memory/battery questions quickly without misleading the user about a completed computer health score.

## 2026-07-16 - Pipeline invocation is a current-state synchronization boundary

- Decision: once a local destructive safety pipeline is invoked, refresh every user-facing evidence surface that may have changed before presenting success, failure, or unknown state.
- Rejected: refresh only on `result.Success`, infer a thrown operation made no change, retry mutation from catch, or make the user manually discover a partial timeline entry.
- Consequence: cache cleanup, startup disable, direct C-drive cleanup, residue quarantine, purge, and both restore paths now converge on current read-only state even when completion is uncertain.

## 2026-07-16 - Direct C-drive cleanup performs an automatic health rescan

- Decision: after a confirmed cleanup pipeline attempt, reuse the existing full read-only health scan so reclaimed-space totals and recommendation cards are rebuilt from current disk evidence.
- Rejected: leave the old executable card selected, merely enable a manual rescan instruction, or treat timeline refresh as proof that disk recommendations are current.
- Consequence: the old selection is revoked by the scan and the execute button is recalculated from current selection; scan cancellation/failure preserves operation truth and cannot trigger another cleanup.

## 2026-07-16 - Later install observation is session-bound and launcher-free

- Decision: keep the automatic before snapshot in memory for the currently analyzed package and expose one primary-page later-rescan action after a non-refused launch; revoke it when the package path changes or a new prepare attempt starts.
- Rejected: persist personal-path baselines across app restarts, use the advanced manual controls as the ordinary workflow, create a launcher-capable coordinator in the persistent handler, or compare a new package against an old baseline.
- Consequence: bootstrap/child installers can be re-observed after their windows close without relaunching anything, while package identity changes remove stale comparison authority immediately.

## 2026-07-16 - Installer post-scan has a dedicated read-only coordinator

- Decision: isolate software/footprint observation and diff construction in `InstallerPostScanCoordinator`, then let both the trusted execution coordinator and persistent page action delegate to it.
- Rejected: duplicate scan/report code in WPF or instantiate a launcher-holding execution coordinator solely to call its read-only method.
- Consequence: the persistent button's dependency graph contains no installer launcher or operation pipeline, and all valid results still feed one shared report/catalog presenter.

## 2026-07-16 - Installer observation recovery is explicit, read-only, and baseline preserving

- Decision: expose `我已完成安装，重新扫描` only after interrupted waiting or a failed post-scan, and compare each user-requested read against the original captured before snapshot.
- Rejected: relaunch the installer, run an automatic retry loop, replace the before baseline, direct beginners into advanced manual diagnostics, or infer installation success from exit code/change detection.
- Consequence: an uncertain observation can recover in the same simple result flow; each click performs exactly one read-only scan, valid evidence updates both report and Applications, and failure remains truthful and retryable.

## 2026-07-16 - The verified post-install snapshot is the catalog synchronization source

- Decision: when the coordinator returns both an after snapshot and an initial difference report, update Application Management from that exact snapshot before presenting the report.
- Rejected: run a second software scan, update the catalog after any launch attempt, or infer installation success from an installer exit code.
- Consequence: a newly observed application appears immediately in both the install report and the application catalog, while interrupted/refused/failed outcomes cannot publish unverified state or gain execution authority.

## 2026-07-16 - Migration attempts refresh both inventory and closure evidence

- Decision: after the trusted migration coordinator is invoked, refresh application location/C-drive evidence and the migration-closure store before interpreting the result.
- Rejected: refresh only after accepted completion, retry automatically after an uncertain response, or infer that a failed transport left all files and links unchanged.
- Consequence: partial or uncertain migration cannot leave stale location/closure UI, while success language remains limited to authenticated typed `Completed` outcomes.

## 2026-07-16 - A production attempt is an inventory synchronization boundary

- Decision: after the trusted uninstall coordinator has been invoked, refresh current application inventory regardless of lifecycle completion status.
- Rejected: refresh only after a complete authenticated response, infer no change from timeout/transport failure, or enter residue cleanup from a generic execution-attempt flag.
- Consequence: possible partial mutations no longer leave stale application state in the UI, while residue handling remains behind completed validated post-scan evidence and its own confirmation/pipeline.

## 2026-07-16 - Startup restore has its own prepared pipeline operation

- Decision: treat restoring a current-user Run value as a medium-risk destructive operation prepared from the current timeline row and one verified rollback manifest, then execute it through `SafetyOperationPipeline`.
- Rejected: reuse the cached view-model manifest path, call the disable handler's convenience restore method, or let MainWindow update timeline state after a direct registry restore.
- Consequence: stale rows, changed manifests, mismatched locators, unconfirmed descriptors, same-name values, and registry security drift fail closed at their respective boundaries.

## 2026-07-16 - Failed startup restore becomes partially restorable

- Decision: once the exact store has been asked to restore and does not report success, mark the timeline row `PartiallyRestorable` and retain the restore kind for technical review.
- Rejected: leave the row as confidently restorable after a possibly partial registry write, or mark it restored merely because the call returned.
- Consequence: the beginner is not invited to retry automatically from uncertain state; a fresh application/startup scan is required before further action.

## 2026-07-16 - Restore is a destructive operation and uses the same pipeline boundary

- Decision: ordinary quarantine restore must prepare a fresh operation from the current timeline row, bind both manifest and quarantined payload identity, require explicit confirmation, and execute through `SafetyOperationPipeline`.
- Rejected: trust the cached UI item, call `FileQuarantineService.RestoreAsync` directly from MainWindow, or treat rollback as inherently safe because it moves data toward an old location.
- Consequence: changed manifests, replaced payloads, stale timeline state, occupied destinations, and unconfirmed descriptors fail before movement; the operation handler owns both mutation and conservative journal updates.

## 2026-07-16 - Timeline UI models are presentation, not restore authority

- Decision: use only the clicked timeline id to load the current persisted entry and derive confirmation/evidence; compare the same entry again immediately before restore.
- Rejected: use manifest paths, affected paths, or restore state copied into the cached `ActionTimelineItemViewModel` as execution evidence.
- Consequence: automatic page caching cannot authorize a stale or forged restore, while the existing beginner-visible confirmation remains path-free.

### 2026-07-16 - Background application count and signal counts are different facts

- Decision: count resident applications once by ownership, while retaining separate per-application running, startup, service, and task signal totals that may overlap.
- Reason: one application can expose several background mechanisms. Adding those signal totals would exaggerate the number of applications, while hiding them would remove useful explanation.
- Consequence: the existing application summary first says how many ordinary/system/ownership-pending applications are involved, marks protected groups read-only, then lists the observed signal types. Agent resident lists reuse the same catalog and no control action changes.

### 2026-07-16 - C-drive totals retain evidence but expose ownership

- Decision: keep the existing deduplicated C-drive footprint total and filter membership, but derive ordinary, explicit system, and ownership-pending groups through one shared read-only catalog.
- Reason: system and uncertain-ownership footprints are useful diagnosis and must not disappear, yet a single undifferentiated total makes every application sound like an ordinary migration or cleanup candidate.
- Consequence: the homepage digest and application-page summary use their existing compact controls and label protected groups `仅供查看`; Agent aggregate grouping reuses the same profile lists. No new panel or action authority is introduced.

### 2026-07-16 - Cleanup wording requires both action and calibrated risk

- Decision: a health finding may be described as low-risk or included in reclaimable cleanup totals only when its action is `Clean` and its risk is `None` or `Low`.
- Reason: `Clean` describes a recommended workflow, not safety by itself. Medium/high findings must remain visible without implying that confirmation alone makes them suitable for quarantine handling.
- Consequence: C-drive Agent replies, stored digests, homepage totals, and finding copy share one predicate. Higher-risk clean findings say to observe and prepare snapshot/rollback evidence. Startup summaries independently reuse ordinary-app action authority so system and ownership-pending evidence remains read-only.

### 2026-07-16 - Aggregate Agent action lists reuse drawer availability

- Decision: Agent summaries classify each current profile through the same ordinary-action, migration, uninstall, and startup-review policies used by the app drawer.
- Reason: raw path, command, or startup-observation presence is evidence, not authorization. Aggregate wording is an action promise to a beginner even when the final drawer would later refuse it.
- Consequence: ordinary actionable profiles, D-installed/unknown data-location review, and protected read-only evidence are counted and named separately. Exact and aggregate startup surfaces now agree, and every resulting navigation remains internal and non-executable.

### 2026-07-16 - Homepage migration action follows current profile authority

- Decision: a historical migration-closure record becomes a homepage `Migrate` finding only when its current software profile is uniquely resolved and remains eligible for ordinary migration review.
- Reason: a past monitor record is diagnostic evidence, not current authorization. Name uniqueness alone cannot make a system component or ownership-pending application actionable.
- Consequence: reviewable ordinary applications retain exact drawer navigation; protected and unavailable records remain visible as `Observe`, carry no app target, use generic Applications navigation, and explicitly say that no migration action is generated.

## 2026-07-16 - Production coordinator invocation consumes the reviewed plan

- Decision: once an uninstall or migration window invokes its production coordinator, mark the plan as attempted before awaiting and never enable that same reviewed request again, even when the returned result is refused or unknown.
- Rejected: trust the coordinator to always return before marking state, re-enable migration after a failed/uncertain result, or let the user retry with the same snapshot and rollback evidence.
- Consequence: unknown outcomes conservatively trigger a fresh read-only scan, and every retry requires a newly generated plan and confirmation.

## 2026-07-16 - Failed post-production reads stop downstream inference

- Decision: route uninstall/migration application rescans through one failure-tolerant read-only helper and return before residue review or closure classification when it cannot produce current inventory.
- Rejected: allow the UI exception to escape, keep evaluating against the pre-operation list, or interpret a read failure as no residue/healthy closure.
- Consequence: the previous catalog remains visible, the operation truth remains primary, and the beginner receives a concrete rescan instruction without an automatic retry.

## 2026-07-16 - Instruction text is not an input value

- Decision: application search starts with an empty query and uses a non-interactive overlay for `搜索应用`; hint visibility follows actual text before catalog filtering.
- Rejected: store the hint as TextBox content and teach Core filtering to ignore it, or require the beginner to erase it before typing.
- Consequence: first entry still shows all applications, typing begins immediately, clearing has predictable meaning, and programmatic app targeting continues to use real search text.

## 2026-07-16 - Post-scan advice is a typed command, and Close is not consent

- Decision: map uninstall post-scan conclusions to `Close`, `RetryReadOnlyScan`, or `ReviewResidue`; keep the result window presentation-only and resolve the command in `MainWindow` after a fresh inventory read.
- Rejected: leave the suggested next step as passive text, automatically enter residue review after closing the result, or let Retry reach cleanup confirmation.
- Consequence: a beginner can act on the Agent's recommendation without interpreting technical text, while every mutation-capable path still requires a later explicit confirmation and Close changes nothing.

## 2026-07-16 - Personal-file inspection uses current evidence and fixed Explorer selection

- Decision: keep exact paths out of default beginner rows, reveal them only after an explicit `查看位置` action, and allow selection only when the path is still part of the current scan evidence and still exists locally.
- Rejected: put raw paths in every candidate row, open the file itself, accept arbitrary UI-provided paths, or add delete/move controls beside a possible-duplicate guess.
- Consequence: the user can understand where a candidate lives and compare locations in Explorer, while same-name/same-size remains a read-only hint and cannot silently become cleanup authority.

## 2026-07-16 - A persisted digest is history, not hydrated current evidence

- Decision: keep restart-loaded health digests as path-free history and require the shared read-only health gate plus a current in-memory summary before claiming that C-drive evidence is open.
- Rejected: restore only the digest then navigate to empty controls, silently label old summary text as current detail, or force a second scan when the current process already has a successful health session.
- Consequence: the homepage remains immediately useful after restart, while the evidence action tells the truth about when a new body of detailed observations must be prepared.

## 2026-07-16 - Background findings hand off to details or a Resident catalog, never directly to control

- Decision: give each safe background-review item a details-only application target, and represent aggregate startup/background navigation with typed `AppCatalogFilter.Resident` that MainWindow explicitly whitelists.
- Rejected: open an unfiltered application grid, infer one target from an aggregate answer, automatically open startup-control review, or let Agent output pass arbitrary filter strings.
- Consequence: the beginner keeps the context that motivated the click, while deciding whether to manage an application's startup remains a later explicit action behind existing evidence and confirmation boundaries.

## 2026-07-18 - Aggregate action answers narrow candidates without selecting an action

- Decision: migration answers hand off to `CDrive`, uninstall answers to `Uninstallable`, and startup answers to `Resident`; MainWindow explicitly whitelists only those typed filters.
- Rejected: open the full catalog, guess one application from an aggregate question, open a plan automatically, or let Agent supply arbitrary filter strings.
- Consequence: the next screen preserves the user's question while every application still requires an individual evidence review and explicit confirmation before any system modification.

## 2026-07-18 - Agent next-step buttons carry typed actions, not page strings

- Decision: bind each persistent next-step button to its complete `AgentNextActionViewModel`, including an optional typed application filter and filter-aware AutomationId.
- Rejected: continue binding only `TargetPage`, infer a filter from visible Chinese labels, or create separate handlers for resident and C-drive buttons.
- Consequence: two Apps actions can remain visually simple while preserving distinct context, stable automation identity, and one shared allowlisted navigation boundary.

## 2026-07-18 - Homepage migration-closure fallback uses CDrive context

- Decision: when a migration-closure finding cannot safely name one current application, navigate to the current `CDrive` application catalog; retain exact re-resolution whenever a target name exists.
- Rejected: open all applications, guess one app from a protected/ambiguous historical record, or open a migration plan from aggregate evidence.
- Consequence: the user sees a relevant review set without OMNIX claiming that every listed app caused the old closure warning.

## 2026-07-18 - C-drive root-cause navigation reuses the bounded catalog handoff

- Decision: delegate the root-cause `占 C 盘应用` action to the same typed CDrive handoff used by Agent surfaces.
- Rejected: keep a fourth copy of page/filter/load/status logic or merely change `_softwareProfiles.Count` to a local expression in the duplicate branch.
- Consequence: filtered empty-state truth, loading behavior, and safety copy have one implementation and future fixes cannot drift across entry points.

## 2026-07-18 - Source integrity uses a reviewed repository script

- Decision: promote strict UTF-8/U+FFFD/XAML parsing into `.omx/verify-source-integrity.ps1` and invoke it with process-scoped execution-policy bypass.
- Rejected: keep copying long inline loops, change the machine execution policy, or weaken the gate to compiler success only.
- Consequence: the completion gate is one repeatable read-only command; exact symbol counts remain separate fixed-string checks until the helper is extended.

## 2026-07-18 - Portable test packages expose trust truth instead of manufacturing trust

- Decision: publish App, worker, rules, hashes, Authenticode states, runtime requirement, Chinese test boundaries, and an explicit mutation-readiness value into a new timestamped `.artifacts` package.
- Rejected: self-sign during packaging, import a certificate, relax executable trust, overwrite/delete prior output, hide unsigned status, or call an unsigned test package production-ready.
- Consequence: beginners have one reproducible read-only test entry, while real mutation remains fail-closed until App and worker carry one externally provisioned valid signer and pass disposable-machine acceptance.

## 2026-07-18 - Windows PowerShell packaging source stays ASCII-only

- Decision: keep executable `.ps1` source ASCII-only and copy Chinese documentation from a separate UTF-8 template; use only .NET Framework-compatible path APIs.
- Rejected: depend on Windows PowerShell guessing UTF-8 without BOM, change machine policy/locale, or assume the .NET 8 application's BCL is the script host's BCL.
- Consequence: the documented Windows PowerShell 5.1 command is reproducible on the target operating system without sacrificing a readable Chinese package guide.

## 2026-07-18 - Privileged Release workers expose production process modes only

- Decision: keep the fake worker mode and implementation in Debug, exclude its source from Release, and make packaging reject either UTF-8 or UTF-16 fake-command metadata in the actual worker DLL.
- Rejected: rely only on an unreachable Release branch, ship test protocol code because it has no mutation authority, or trust only source inspection without checking the artifact.
- Consequence: Debug lifecycle smokes remain available, while Release has a smaller command surface and every portable package proves the exclusion before it is archived.

## 2026-07-18 - Empty findings use explicit state, not an empty fixed-height control

- Decision: collapse the Home findings list until it has real items and put a stable compact text state before it; switch initial and valid-empty copy after a completed summary.
- Rejected: leave a blank 240px ListBox, claim “no issue” before scanning, hide the entire right column, or use a decorative Border as the only automation proof.
- Consequence: the first screen remains balanced and truthful, while findings still reclaim the full list layout immediately after a real scan.

## 2026-07-18 - Agent separates decision help from capability catalog

- Decision: use one native WPF `TabControl` whose default `咨询与建议` tab owns conversation and current recommendations, while `能力与工具` owns allowlisted settings, skills, and system-tool entry points; both tabs stretch to the available width.
- Rejected: keep the dense two-column first view, create new left-navigation pages, hide existing capabilities, or retain a fixed-width consultation card that wastes the working area.
- Consequence: beginners first see what the Agent thinks they should do, while optional capabilities remain one explicit click away without changing any Agent authority, handler, or safety boundary.

## 2026-07-18 - C-drive result surfaces require current items

- Decision: keep state explanations visible, but collapse every root-cause, growth, personal-storage, recommendation, and action surface until its current presenter collection is nonempty; use one presentation-only visibility method.
- Rejected: reserve large empty lists, claim the scan is clean before it runs, show a disabled cleanup preview without evidence, or alter scanner/recommendation/operation behavior to solve a layout problem.
- Consequence: the first view directs the beginner to one read-only scan and explains safety, while completed result lists and action review still appear immediately from current collection counts.

## 2026-07-18 - Installation empty states are summaries, not synthetic rows

- Decision: represent zero remembered rules as an empty presenter collection and keep only the summary; show rule controls from row count and report controls only after a valid report/presenter.
- Rejected: retain a non-actionable placeholder row, leave disabled controls as first-view instructions, or change installer trust/execution behavior while simplifying presentation.
- Consequence: the primary workflow is visibly `选择文件 -> 让 Agent 看看`, while learned rules, report cards, Agent explanation, and technical detail remain available exactly when their evidence exists.

## 2026-07-18 - Undo Center empty state is a conclusion, not a timeline item

- Decision: keep quarantine policy and timeline state as compact text, while showing candidate/history lists and cleanup/restore controls only when current records exist.
- Rejected: model loading or no-history messages as selectable timeline rows, reserve blank list height, show a disabled permanent-delete preview without candidates, or alter restore/purge authority to solve presentation.
- Consequence: beginners see that there is currently nothing to undo instead of interpreting disabled controls as a failed task; real entries and expiry candidates still reclaim their existing workflows from current evidence.

## 2026-07-18 - Migration preview leads with a decision and folds evidence

- Decision: derive a path-free beginner summary from the existing typed preview and put raw destination, manifest, byte counts, readiness checklist, and plan sections under one collapsed technical-details expander; hide preparation/request buttons when production readiness cannot prepare execution.
- Rejected: remove evidence, weaken the signed readiness gate, show disabled actions as a roadmap, parse paths into the summary, or change migration planning/execution behavior while fixing information hierarchy.
- Consequence: the first view answers whether migration is appropriate, the next step, rollback, and coarse D-drive space; signed releases retain the complete evidence and execution progression, while unsigned previews expose no false action affordance.

## 2026-07-19 - Reverify signed candidates after transfer

- Decision: the disposable environment independently verifies package path, complete payload inventory, hashes, signatures, timestamps, signer correlation, and worker command surface before launching OMNIX.
- Rejected: trust the signed-transform console output or copied manifest alone; transfer/storage can alter payloads or add files.
- Rejected: let the verifier launch the application or write an acceptance result; package integrity and behavioral acceptance are separate authorities.
- Consequence: a positive verifier result authorizes only the start of the explicit behavioral checklist and remains non-mutating.

## 2026-07-19 - Signed release is an immutable transform, not a trust bootstrap

- Decision: transform a verified portable artifact into a new candidate directory and sign only the copied App/worker executables with an explicitly supplied certificate already present in `Cert:\CurrentUser\My`.
- Rejected: mutate the portable source package in place; that would invalidate its recorded hashes and destroy the read-only baseline.
- Rejected: accept a PFX/password or generate/import a self-signed certificate; release tooling must not handle secrets, change certificate stores, or bootstrap machine trust.
- Rejected: label same-signer output production-ready. The manifest records eligibility only and keeps disposable-machine acceptance false.
- Consequence: the repository now has a deterministic path from unsigned test artifact to signed candidate, while real signing and mutation acceptance remain external, explicit gates.

## 2026-07-19 - Execution results return to current inventory

- Decision: after a real migration or official-uninstall execution attempt is acknowledged, close the exhausted plan window and let MainWindow's existing post-attempt branch perform the authoritative read-only rescan.
- Rejected: leave the user on the locked plan and require a second close before synchronization; this delays current-state truth and makes the result button misleading.
- Rejected: give every official-uninstall result the same return wording; the Debug standalone connection smoke has no application page, so its generic close copy remains separate.
- Consequence: no operation retries automatically. Preview-only paths stay open, post-scan typed residue actions keep their routing, and all signer/consent/worker/mutation gates remain unchanged.

## 2026-07-19 - Uninstall preparation is optional evidence, not the first task

- Decision: lead with a path-free Agent decision about official uninstall, residue review, undo limits, and the next step; keep preparation, complete workflow, and technical evidence collapsed, and hide preparation entirely when production readiness cannot prepare execution.
- Rejected: leave installer selection, restore-point status, backup acknowledgement, and a disabled final-checklist button in the unsigned first view; remove recovery preparation; or weaken signing/final-consent gates to make the controls appear usable.
- Consequence: beginners can understand what OMNIX would do without preparing unavailable execution, while a valid signed release retains the existing recovery-evidence and final-consent workflow behind an explicit preparation entry.
## 2026-07-19 - Behavioral acceptance is a separate receipt, not package metadata

- Decision: keep the signed candidate immutable and store environment attestation, required cases, operator observations, reset state, and hashed evidence in a separate session manifest and acceptance receipt.
- Rejected: rewrite the candidate manifest after testing, infer acceptance from package signing, let a script launch OMNIX or automate UAC, or allow skipped cases to be summarized as an overall pass.
- Consequence: the same candidate can be independently reverified, transfer tampering remains detectable, and behavioral acceptance exists only when one exact evidence-bound receipt passes a read-only verifier.

## 2026-07-19 - Disposable acceptance uses explicit string switches at the Windows PowerShell boundary

- Decision: accept only literal `false` for `PrimaryMachine` and literal `true` for `IsDisposableEnvironment`, then serialize genuine JSON booleans into the session record.
- Rejected: rely on Boolean parameter transport through `powershell.exe -File`, omit explicit primary-machine state, or accept free-form truthy values.
- Consequence: the documented command remains reliable on Windows PowerShell 5.1 while the persisted receipt schema stays strongly typed and fail-closed.

## 2026-07-19 - Acceptance fixtures are a separate package, never product payload

- Decision: build deterministic test software/data in `Css.AcceptanceFixtures`, publish it through a dedicated manifest, and keep App/Elevated project references and product package allowlists free of fixture code.
- Rejected: embed hidden fixture commands in the product, reuse personal installed software, or let an operator create ad hoc test paths during acceptance.
- Consequence: behavioral cases are reproducible without expanding the user's privileged command surface, and artifact tests can prove product packages exclude the harness.

## 2026-07-19 - Acceptance sessions bind both product and fixture manifests

- Decision: require fixture verification before session output and persist the fixture manifest SHA-256 beside the signed-candidate manifest/signer binding; reverify both at receipt completion.
- Rejected: trust only a fixture directory name, copy fixture files into the candidate, or allow the operator to replace fixtures after observations begin.
- Consequence: evidence identifies the exact product and exact test-world payload while both packages remain independently immutable and verifiable.

## 2026-07-19 - Cleanup acceptance owns exact C Temp or refuses provision

- Decision: use exact `C:\Temp` for the disposable cleanup case only when it does not already exist, then mark and own it for the session.
- Rejected: place cleanup data under a nested user-temp/session directory that scanners can see but the current recommendation builder cannot authorize, or merge fixture data into an existing `C:\Temp`.
- Consequence: the real C-drive rule/recommendation pipeline emits the intended low-risk reversible operation, while collision refusal prevents touching preexisting machine data.
