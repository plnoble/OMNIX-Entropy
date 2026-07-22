# Decision Log

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

### 2026-07-15 - Protected tile identity outranks historical closure severity

- Decision: migration closure severity may change tile status and catalog priority only for profiles currently eligible for ordinary migration closure review.
- Reason: a red `迁移未闭环` system tile tells a beginner to act on something OMNIX intentionally refuses to modify. Historical records remain useful evidence but cannot replace current ownership classification.
- Consequence: system and ownership-pending tiles retain their base labels/status; aggregate copy separates their old records as read-only. Ordinary warning and healthy behavior remains available through typed Core presenters.

### 2026-07-15 - Central action methods enforce drawer policy themselves

- Decision: uninstall, cache-cleanup, and startup-control central methods must evaluate the current typed drawer action before any action-specific read or plan construction.
- Reason: disabled controls and current Agent routing are presentation conveniences, not durable security boundaries; future navigation or event changes must not make protected profiles reachable.
- Consequence: denied entries clear the shared host state, show the existing plain reason, and create no pending operation/target. Existing planner, pipeline, trust, confirmation, and execution behavior remains unchanged for allowed ordinary apps.

### 2026-07-15 - Current app ownership policy outranks stale migration records

- Decision: a migration closure warning may add evidence and enable re-review only for an ordinary profile that is currently eligible for the ordinary migration workflow. It cannot replace or weaken a current system/ownership protection conclusion.
- Reason: monitoring records describe a past migration state and are matched to current inventory by a constrained name mapping; they are evidence, not present-day authority.
- Consequence: the combined drawer presenter controls advice, label, availability, and reason. Protected profiles retain read-only behavior at both the button and plan-entry boundary, while ordinary D-installed apps can still regenerate a non-executable safety plan.

### 2026-07-15 - Residue review is independent from uninstall-launch availability

- Decision: ordinary applications may enter a read-only post-uninstall residue review even when no official uninstall command is known; system-category and managed-root ownership-pending profiles may not enter the ordinary residue workflow.
- Reason: a user may uninstall an application externally and return to OMNIX for residue review. Requiring an uninstall command would remove that recovery path, while system ownership uncertainty still requires a fail-closed boundary.
- Consequence: the drawer carries a separate typed residue-review availability/reason, and the async handler rechecks it. This does not grant cleanup authority; low-risk quarantine still requires identity binding, policy validation, explicit confirmation, pipeline execution, and Undo Center recording.

### 2026-07-15 - Empty inventory invalidates both visible and hidden app state

- Decision: an empty filter or completed empty inventory is an invalidation event, not only a text placeholder.
- Chosen approach: one typed empty presentation resets all beginner conclusions, selection, technical details, button labels/tooltips, preview host, and pending operation/target fields.
- Rejected: clear only the title and visible metrics. Hidden pending cache/startup state or an old category conclusion could survive and later be mistaken for the new context.
- Rejected: describe a completed empty inventory as a scan failure. No profiles is a valid read-only result distinct from loading or failure.
- Consequence: the drawer cannot visually or operationally retain the previously selected app after its catalog context disappears.

### 2026-07-15 - C-drive app totals separate placement, data, and deduplicated membership

- Decision: summarize C main-program placement, D main-program placement, C-drive data/cache app clues, and deduplicated `占 C 盘` membership as separate facts.
- Chosen approach: one path-aware `HasCDriveFootprint` and canonical install-root-excluding `CDriveDataLocationCount` feed the summary, filter, tiles, drawer, Agent, and digest callers.
- Rejected: count non-empty `CDriveWritePaths` directly. It misses C-installed profiles, accepts malformed non-C entries, and cannot explain main program versus data.
- Rejected: add main and data counts to claim a total. One app can have both, so only the shared per-profile footprint predicate supplies the deduplicated total.
- Consequence: the first visible app summary agrees with filtering without calling any clue removable or exposing a path.

### 2026-07-15 - Catalog action filters must share drawer availability denies

- Decision: `Uninstallable` membership uses the same read-only `CanReviewUninstall` policy as the drawer action.
- Chosen approach: require a command and reject system-category or managed-root ownership-pending profiles before listing them; publisher alone remains irrelevant.
- Rejected: keep command presence as the catalog predicate and explain the denial only after selection. That makes the first view promise an action the detail view refuses.
- Rejected: call this production execution readiness. Signer trust, package authorization, final consent, and the operation pipeline remain separate later gates.
- Consequence: catalog, button, and preview now agree while ordinary applications preserve their existing review path.

### 2026-07-15 - A fallback category must not claim a specific use

- Decision: rename the Normal catalog group from `办公学习` / `OfficeStudy` to `普通应用` / `NormalApplications`.
- Chosen approach: preserve the exact `Category == Normal` predicate while aligning enum, WPF tag, stable button identity, visible copy, and selection highlighting.
- Rejected: add office keywords in the same slice. The scanner has no trustworthy use taxonomy yet, and a few keywords would create a new unsupported claim.
- Consequence: utilities and other unclassified software are no longer mislabeled, with no change to catalog membership or action authority.

### 2026-07-15 - Classification evidence explains identity but never authorizes actions

- Decision: retain a typed scanner-owned category assessment beside the existing category, with evidence source, matched rule, fallback state, and confidence.
- Chosen approach: product-name matches are high confidence, publisher-only matches medium, install-location-only matches low; a no-signal profile remains a low-confidence Normal fallback, and an unassessed profile remains Unknown.
- Rejected: concatenate all text and expose only the final enum. That prevents the Agent from explaining its judgment and hides weak path-only inference.
- Rejected: let a high-confidence category assessment enable uninstall, migration, cache, or startup actions. Identity evidence is not consent, signer trust, rollback evidence, or operation readiness.
- Consequence: the drawer can explain its label in one path-free sentence while all existing system/unknown denies and lower-level safety gates remain authoritative.

### 2026-07-15 - Managed install roots can deny actions but cannot prove category

- Decision: an Unknown profile under the current Windows root or Program Files `WindowsApps` receives a read-only ownership-review presentation without rewriting its category.
- Chosen approach: canonical path containment is the trigger; tile, Agent, summaries, previews, and action availability all share the same deny.
- Rejected: treat Microsoft publisher/signature text alone as system ownership. Normal Office/developer/utility applications would be false positives.
- Rejected: silently reclassify Unknown to SystemTool in the presentation layer. Category evidence belongs at the scanner boundary.
- Consequence: incomplete profiles fail safely while ordinary Microsoft-published software in normal locations remains manageable.

### 2026-07-15 - System category overrides ordinary action evidence

- Decision: `SoftwareCategory.SystemTool` is a presentation-level deny for uninstall, migration, cache cleanup, and startup control in the ordinary app drawer.
- Chosen approach: recommend retain, disable all four modifying review buttons with fixed beginner reasons, and leave technical details enabled.
- Rejected: enable an action because an uninstall command, cache path, startup entry, service, or task was discovered. Presence is not proof that ordinary app handling is safe for a system component.
- Consequence: gray system tiles and their drawers no longer contradict each other; lower-level planners remain unchanged for explicitly specialized future workflows.

### 2026-07-15 - Unknown size is not measured zero or releasable space

- Decision: render positive install/data/cache/growth values, but describe default zero through evidence availability rather than `0 B`.
- Chosen approach: identified data/cache paths with no size say the location is known but size is unmeasured; absent paths say no separate location was identified.
- Rejected: label `CacheSizeBytes` as releasable space. Identification does not prove every byte is safe to remove.
- Consequence: the drawer fulfills the promised four-part summary without overstating scanner precision or cleanup benefit.

### 2026-07-15 - Attention tags name the reason without changing risk policy

- Decision: keep the existing Attention status and catalog behavior, but derive its short label from main-program drive and C-drive write evidence.
- Chosen approach: C main program, D main program with C writes, and unknown main location receive three fixed path-free labels.
- Rejected: change colors, risk order, or C-drive filtering in the same UX correction. Those are separate policy decisions and need separate evidence.
- Consequence: the first-view grid becomes understandable without silently changing what OMNIX considers review-worthy.

### 2026-07-15 - Installation reports separate confirmed placement from concurrent change

- Decision: derive one internal placement observation from a unique added profile and show main-program drive separately from owned or unattributed C-drive candidates.
- Chosen approach: canonicalize and deduplicate paths below the presentation boundary, exclude the main install tree from separate data counts, and expose only drive labels and aggregate counts.
- Rejected: attribute every footprint delta during the time window to the installer. Other software or Windows can change concurrently.
- Rejected: offer main-program migration when the program already landed on D. Data-location redirection needs app-specific evidence and its own safe operation.
- Consequence: the report answers where the program landed without overstating who created every C-drive change or gaining execution authority.

### 2026-07-15 - D-installed and C-writing is a data-location problem first

- Decision: when the main program is on D but attributed writes remain on C, advise against repeating main-program migration and explain that data/cache/log/configuration locations need separate review.
- Chosen approach: show deduplicated aggregate C-write counts, keep paths in technical details, and keep the existing migration action disabled until a proven redirection plan exists.
- Rejected: enable `迁移到 D 盘` for every D-installed C-writing app. The current migration backend models main-install movement and must not be repurposed into guessed application-specific data redirection.
- Rejected: call all C-write paths cache and offer cleanup. Some paths can be logs, configuration, models, downloads, or necessary data.
- Consequence: beginners get a truthful next decision without an unsafe or misleading action button; future redirection support will need its own evidence and operation type.

### 2026-07-15 - App growth advice requires a comparison state

- Decision: app-specific growth explanations use a typed aggregate observation carrying comparison availability, snapshot count, growth bytes, and location counts; `RecentGrowthBytes == 0` alone is never treated as proof of stability.
- Chosen approach: prepare the first read-only health snapshot on demand, preserve InsufficientBaseline until a later comparison exists, and re-resolve the exact app after scanning before constructing evidence.
- Rejected: answer directly from C-drive paths. A write location shows possible footprint, not how much or whether it recently grew.
- Rejected: turn `为什么变大` into an automatic cache or migration preview. A why-question requests explanation, not an operation review or consent.
- Consequence: the Agent can separate immediate relief from prevention without overstating a trend, exposing paths, or expanding authority.

### 2026-07-15 - Exact app intent may prepare, but never execute, an existing review

- Decision: a uniquely resolved explicit app action question may carry a typed preview handoff so the WPF layer opens the matching existing review after re-resolving the current inventory.
- Chosen approach: classify only uninstall, migration, cache, and startup wording; use precise button labels; keep location, troubleshooting, and general advice details-only; reuse the same four UI preview methods as manual buttons.
- Rejected: make the user open the app and choose the same action again. It preserves safety but leaves the Agent as a verbose navigator instead of a decision assistant.
- Rejected: let the Agent create or execute an `OperationDescriptor`. Natural-language intent is not confirmation, and duplicating operation authority would bypass current snapshot, signer, and final-consent gates.
- Consequence: the Agent removes one confusing manual step while stale/duplicate identity, safety review, final confirmation, production trust, and rollback rules remain authoritative.

### 2026-07-15 - Operation errors are translated by execution phase

- Decision: raw operation/policy errors never enter beginner UI; presentation is chosen from whether the failure occurred before execution or after a pipeline/restore attempt.
- Chosen approach: pre-execution refusals state that nothing was moved/deleted, while post-attempt failures state that completion is unconfirmed and name the authoritative rescan or Timeline review.
- Rejected: sanitize and display the raw `Error`. Even path-free errors can expose internal policy vocabulary and do not reliably describe partial state.
- Rejected: use one generic failure sentence. It would either understate a safety refusal or overstate certainty after durable work may have started.
- Consequence: users receive an accurate decision and next step; lower layers retain detailed typed failures without becoming primary UX content.

### 2026-07-15 - UI exception boundaries report state, not raw implementation detail

- Decision: beginner-visible catch blocks use fixed workflow-specific conclusions and next steps; they never append `Exception.Message`.
- Chosen approach: distinguish known no-modification failures from operations whose completion is unknown, then direct the latter to the authoritative current-state page or timeline.
- Rejected: pass exceptions through `BeginnerTextSanitizer`. Removing paths alone would still expose HRESULTs, registry keys, provider text, and implementation vocabulary that does not help a beginner decide safely.
- Rejected: say every failure changed nothing. Quarantine purge, restore, residue handling, or cleanup can fail after some durable work, so that sentence could conceal a partial state.
- Consequence: the UI remains honest and actionable without leaking machine detail; deeper diagnostics require an intentional existing technical boundary rather than being improvised in status text.

### 2026-07-15 - Runtime observation uses exact profile-derived process names only

- Decision: sample only exact normalized process names already attributed to the selected software profile, with the display-icon executable name as a bounded fallback.
- Chosen approach: keep raw process objects/names inside Css.Win32, cap the sample at 32 processes and 350 ms, and reduce the result to availability, count, aggregate memory, and coarse CPU activity before crossing into Core/Agent.
- Rejected: read command lines or executable paths to improve matching. Those fields increase access failures and privacy exposure and are unnecessary after inventory attribution.
- Rejected: fuzzy substring matching against the app display name. It can silently include unrelated helpers or similarly named programs and make the resource conclusion misleading.
- Consequence: some apps without a trustworthy process hint return Unavailable instead of a guess; successful results remain small, explainable, and non-executable.

### 2026-07-15 - Crash log messages never cross the Win32 observation boundary

- Decision: query only fixed recent Windows Application crash/hang events and reduce them inside Css.Win32 to availability, count, and latest time before passing evidence to Core/Agent.
- Chosen approach: use an injected `EventLogReader` boundary, fixed provider/id allowlist, bounded reverse query, profile-derived normalized tokens, and no `FormatDescription` call.
- Rejected: send event messages or property arrays to the Agent for better explanation. Those values commonly contain executable paths, user profile paths, module names, and other identifiers; they are not needed to answer whether matching crash evidence exists.
- Rejected: invoke Event Viewer, `wevtutil`, or PowerShell automatically. External tools add no structured safety guarantee and would turn a read-only question into a process launch.
- Consequence: the Agent can distinguish found/no-match/unavailable and cite a recent time without exposing technical/private content or claiming that correlation proves root cause.

### 2026-07-15 - App troubleshooting keeps the app drawer primary and system tools secondary

- Decision: exact app crash/freeze questions should first explain current app evidence and open the exact application drawer; Event Viewer or Task Manager remain separate follow-up questions routed through the existing allowlisted open-only flow.
- Chosen approach: hydrate inventory only when crash wording appears to have a concrete non-generic subject, then show aggregate running/background counts and explicit missing-log limits.
- Rejected: make Event Viewer the primary action for every `闪退`. It loses the exact app context and encourages beginners into a technical system surface before seeing what OMNIX already knows.
- Rejected: attach both app navigation and system-tool launch to one answer button. The current single-action model would make the visible command ambiguous, and a natural-language diagnosis must not silently choose an external tool.
- Consequence: application troubleshooting becomes specific and useful without exposing identifiers or adding log/process authority; users can deliberately ask to open a reviewed system tool after understanding why.

### 2026-07-15 - Whole-computer diagnosis is distinct from lightweight performance observation

- Decision: add a `SystemDiagnosis` conversation intent for explicit whole-computer 体检/整体状态/整体优化 wording and let it share the full read-only health scan with C-drive diagnosis and the System Diagnosis skill.
- Chosen approach: use a bounded phrase list and route it before hardware/machine/C-drive keywords, while keeping C-specific phrases out of the list. Generalize the evidence policy name from C-drive-only to full-health.
- Rejected: map every `优化` or `状态` mention to full diagnosis. That would make app questions and narrow settings/performance questions unexpectedly scan the disk.
- Rejected: answer whole-computer questions from lightweight memory/battery evidence. That would omit C-drive root cause, application attribution, growth, and other dimensions while sounding complete.
- Consequence: beginners can request a real whole-computer check naturally; narrower questions retain cheaper evidence scope and no diagnosis grants permission to apply findings.

### 2026-07-15 - Only exact non-diagnostic conversation skips unknown-app inventory

- Decision: skip software inventory for a closed set of complete greetings, thanks, and capability/help questions; keep the existing General-intent scan for all other unclassified text.
- Chosen approach: trim only surrounding whitespace and terminal punctuation, then compare the whole remaining question to an explicit phrase set. Answer those phrases with a local capability summary before profile matching.
- Rejected: remove General from the inventory policy. That would stop `微信最近有点奇怪` from hydrating inventory and prevent exact installed-app resolution.
- Rejected: use substring matching for `你好`, `帮助`, or `能做什么`. It would incorrectly suppress evidence for mixed questions that also contain an app problem.
- Consequence: harmless conversation is immediate and private while unknown app mentions retain the existing exact-profile safety path after read-only inventory.

### 2026-07-15 - Explicit System Diagnosis skill may start the shared read-only health scan

- Decision: clicking the `系统诊断与体检` skill with no current health summary may await the existing full read-only health scan before showing the Agent answer.
- Chosen approach: add a pure category/evidence policy and reuse `EnsureHealthScanLoadedAsync`, so Home, C-drive questions, and the skill join one in-flight task and failed/cancelled work remains retryable.
- Rejected: keep navigating the beginner to Home to click another button. The skill click already expresses the intent to diagnose and the extra step exposes an internal prerequisite.
- Rejected: apply the same scan to troubleshooting, hardware, settings, or tool skills. Those questions need narrower evidence or clarification and should not pay the cost/privacy scope of a full disk scan.
- Consequence: System Diagnosis becomes useful in one click while all modification authority remains outside the skill handler and the existing OperationPipeline boundaries stay unchanged.

### 2026-07-15 - Application inventory loads once on first page entry

- Decision: start the existing read-only software inventory automatically on first navigation to application management. Keep a manual `重新扫描` control for explicit refresh.
- Chosen approach: share one in-flight task, distinguish successful empty inventory from never-loaded state, and let the C-drive application handoff await the same task before applying its filter.
- Rejected: scan automatically at process startup. It would delay the home first view even when the user only wants a C-drive health scan.
- Rejected: keep asking the beginner to click `扫描应用`. That exposes an implementation prerequisite instead of presenting the actual application grid.
- Consequence: application management becomes useful on first entry without repeated scans on every navigation; scan remains local and read-only.

### 2026-07-15 - Show production worker readiness before uninstall or migration preparation

- Decision: add a path-free, read-only production readiness conclusion to the first visible area of uninstall and migration plan windows. Gate snapshot/final-confirmation preparation when the current App/Worker package is not trusted for production.
- Chosen approach: use one shared current-package trust assessor and a beginner presenter. The plan page assessment is advisory and fail-closed; the existing production coordinator must independently re-assess signer trust and worker hash immediately before any worker launch.
- Rejected: allow unsigned builds to mutate disposable fixtures through production worker modes. Even with path constraints, that would create a second privileged policy surface and make a developer switch capable of authorizing real mutation.
- Rejected: wait until after recovery preparation and final confirmation to show trust refusal. It wastes beginner effort and makes a disabled development build look executable until the last step.
- Consequence: unsigned DEBUG builds remain useful for read-only plans and fake lifecycle transport tests, but cannot create final uninstall/migration execution evidence or request elevation. Signed same-publisher packages retain the existing full path.

### 2026-07-07 - Migration checklist evidence must name what was confirmed

- Decision: Migration preflight steps must show explicit snapshot evidence and spell out the plan-confirmation scope, not just "viewed the plan".
- Alternatives considered: Keep generic "snapshot ID" / "user viewed plan" text; add a real migration request button now; defer until migration execution exists.
- Why chosen: Migration is a high-risk future operation. Even while V1 remains preview-only, the UI should train the correct safety model: snapshot evidence, target location, affected paths, rollback, and monitoring must be visible before any future request can exist.
- Consequences: The checklist is more verbose but more auditable. Future migration execution work must preserve this evidence wording and still pass the execution gate before creating any operation descriptor.

### 2026-07-07 - Residue review blocks from cached inventory before full rescan

- Decision: When the selected app is still visible in the current app inventory at the same install path, post-uninstall residue review immediately shows an inline "still installed, do not clean residue" result.
- Alternatives considered: Always run a full software rescan before responding; keep showing an informational modal; allow the user to continue to residue candidates anyway.
- Why chosen: The user wants the assistant to make safe decisions and avoid confusing waits. If the app is still installed, residue handling is unsafe by definition, so a full rescan is unnecessary for the common pre-uninstall click path.
- Consequences: The drawer gives immediate feedback and does not create an operation. A full rescan remains available only when the cached inventory cannot prove the app is still present.

### 2026-07-07 - C-drive page hides manual-looking path controls and raw reports by default

- Decision: The C-drive page shows a read-only `系统盘 C 盘` scan target and keeps the raw technical report collapsed behind `显示技术报告`.
- Alternatives considered: Keep the visible `C:\` selector; keep the technical report visible as a small box; remove the technical report completely.
- Why chosen: The user explicitly asked whether `C:\` was manual input and said dense technical text is not useful for beginners. Hiding the report by default keeps the first-screen experience focused on cards and Agent advice while preserving audit evidence for advanced checks.
- Consequences: Future C-drive UI should not expose raw paths or path-entry-looking controls as primary UI. Advanced evidence should remain available through explicit detail/toggle actions.

### 2026-07-07 - Homepage Agent responses use an inline panel instead of stacked modals

- Decision: Homepage key-finding actions update an inline `Agent 回答` panel rather than opening MessageBoxes.
- Alternatives considered: Keep modal MessageBoxes; open a separate Agent window; put the response below the key-finding list.
- Why chosen: The user asked for a direct, non-piled-up interface. GUI verification showed modal messages could stack during repeated automation, and placing the panel below the list made it hard to see.
- Consequences: Home Agent responses are easier to scan and remain non-executable. Future homepage actions should update the inline panel or navigate to a clear page, not create more modal clutter.

### 2026-07-07 - C-drive page leads with beginner summaries, technical report stays secondary

- Decision: The C-drive page should present root-cause summary cards and growth cards before any raw path report. The raw report remains available as a smaller technical section for advanced inspection.
- Alternatives considered: Keep the existing full path report as the main content; remove technical details entirely; show only recommendation cards.
- Why chosen: The user explicitly said dense technical text is not useful for beginners. Keeping the technical report secondary preserves auditability without forcing it into the first reading path.
- Consequences: C-drive presenters now have to translate scanner output into user-facing conclusions. Future C-drive features should update presenter tests first and should not expose raw `C:\...` paths in primary cards unless the user opens technical details.

### 2026-07-07 - Overwrite growth list binding instead of risky mojibake deletion

- Decision: Add the new `GrowthFindingPresenter.CreateList(...)` assignment after the old raw-path `GrowthListBox.ItemsSource` assignment, leaving the old assignment temporarily dead at runtime.
- Alternatives considered: Delete the old assignment immediately; rewrite the surrounding mojibake-affected block; leave growth findings unchanged.
- Why chosen: The old block contains localized/mojibake text that made direct patch context fragile. A later assignment safely changes runtime behavior with minimal risk.
- Consequences: This was used as a low-risk bridge, then the dead raw-path assignment was removed once a safe ASCII anchor was confirmed. Future cleanup should still avoid localized text as deletion context.

## 2026-07-01 - Localized UI strings use ASCII-safe source escapes

Decision: For current localized WPF/C# edits, use C# Unicode escape literals for production/test strings and XML numeric character references for XAML static text.

Alternatives considered:

- Type Chinese directly into every source file.
- Keep English strings until a full localization resource system exists.
- Rewrite affected files with PowerShell encoding commands.

Why chosen:

The user needs the UI to become readable now, but this repository has already had localized-source encoding damage. Escape literals keep patches stable and still render Chinese at runtime.

Consequences:

- Source readability is less pleasant in localized lines.
- Tests can assert Chinese runtime strings without fragile file encoding rewrites.
- A future resource table can replace these literals once the UI direction stabilizes.

## 2026-07-01 - Migration rollback manifest is evidence-first

Decision: Add a plan-only rollback manifest builder/store before any migration handler. The manifest records original paths, planned destinations, restore paths, services/startup/tasks to restore, monitor paths, verification steps, and rollback steps. Writing the manifest is a JSON evidence action only and does not move app files.

Alternatives considered:

- Generate rollback evidence only during real migration.
- Store rollback data only inside the operation descriptor.
- Skip manifest writing until the migration mover exists.

Why chosen:

The user needs migration to be reversible and understandable. A manifest gives a concrete artifact for review and future rollback before any high-risk movement is possible.

Consequences:

- The migration gate can rely on a real manifest file later.
- The next UI slice should create the manifest only after user confirmation, then refresh readiness.
- The actual migration handler must use this manifest rather than recomputing paths.

## 2026-07-01 - Destination free-space check is read-only evidence

Decision: Add `MigrationDestinationSpaceProbe` as a read-only check that reports drive root, available bytes, required bytes, and whether the destination has enough space. Failures degrade to "could not check" instead of throwing into the UI.

Alternatives considered:

- Let the migration handler discover insufficient space during execution.
- Estimate space but not check the destination drive.
- Require manual user inspection of D drive.

Why chosen:

Migration must not begin if D drive space is insufficient. The check is safe, read-only, testable, and gives the Agent a clear reason to block.

Consequences:

- The migration plan window can explain space readiness before execution exists.
- Future readiness refresh can populate `DestinationAvailableBytes` from the probe.

## 2026-07-01 - Migration execution request needs a dedicated readiness gate

Decision: Real migration must be guarded by `MigrationExecutionGate` before any handler exists. The gate requires feature enablement, snapshot id, plan confirmation, app-close confirmation, rollback manifest, destination free-space check, and post-migration monitoring confirmation. Passing the gate creates only a high-risk operation descriptor; it does not execute.

Alternatives considered:

- Add checkboxes directly to the WPF migration window.
- Let the existing `MigrationPlan` imply readiness.
- Wait until a real mover exists before modeling readiness.

Why chosen:

The user's core requirement is "help me decide, but do not mess things up." Migration can fail even when the move steps look plausible, so readiness needs its own testable model before any execution path is possible.

Consequences:

- The migration UI can explain exactly what is missing.
- Future migration execution must consume this gate result instead of bypassing it.
- Next safe slices are rollback manifest generation and destination free-space probing, still plan-only.

## 2026-07-01 - Migration plan page stays execution-disabled

Decision: The "Move to D drive" button opens a structured migration plan window, but it remains execution-disabled. It can explain destination, score, blockers, snapshot, rollback, and monitoring requirements; it cannot move files or create shortcuts/redirects.

Alternatives considered:

- Make the drawer button begin migration after a simple confirmation.
- Keep only the short drawer preview until real migration exists.
- Add a hidden operation descriptor now and wire execution later.

Why chosen:

Migration is high-risk for beginner users because apps can keep writing to C drive, depend on services, or break when paths change. A full plan page makes the assistant more useful without crossing the safety boundary.

Consequences:

- The next implementation slice should be a readiness gate, not a mover.
- Real migration still needs snapshot id, rollback manifest, app-close confirmation, free-space check, redirect strategy, and post-migration C-drive monitoring confirmation.

## 2026-06-30 - App drawer migration is preview-first

Decision: The app drawer may show migration scoring, target path suggestions, and verification/rollback requirements, but it must not move files. Real migration needs a separate plan page and a future operation descriptor that includes snapshot, rollback, app-close checks, and post-migration monitoring.

Alternatives considered:

- Make the "Move to D drive" drawer button directly migrate files.
- Hide migration until a full executor exists.
- Keep migration guidance only in technical details.

Why chosen:

The user wants the assistant to decide and explain, but "must not mess things up". A preview-first drawer gives a clear answer for beginner users while preserving the safety boundary for high-risk migration.

Consequences:

- `AppDrawerViewModel` exposes `MigrationSummary` and `MigrationPreviewLines`.
- UI can say whether an app is already fine on D drive, cache-only, blocked, or needs a future plan.
- Real file movement remains blocked until safety evidence and rollback are implemented.

## 2026-06-30 - Show official uninstall readiness as a preflight checklist

Decision: Represent official uninstaller readiness as a `OfficialUninstallPreflightChecklistViewModel` with explicit step states before adding any real execution handler. The WPF uninstall plan window renders this checklist ahead of lower-level gate details.

Alternatives considered:

- Keep showing only raw execution-gate blocking reasons.
- Add confirmation checkboxes directly in WPF code-behind.
- Hide the execution gate entirely until real execution is implemented.

Why chosen:

The user wants the app to make decisions understandable for non-experts. A checklist translates technical blockers into visible "ready / needed / blocked" steps while still using the same core gate that protects real execution.

Consequences:

- The UI can explain what is missing without enabling process launch.
- Future checkbox/button work should update `OfficialUninstallExecutionReadiness`, not bypass the checklist or gate.
- The current checklist uses ASCII text to avoid worsening existing localized-string encoding issues; localization should be cleaned up separately.

## 2026-06-30 - External uninstallers need publisher signature evidence

Decision: External official uninstallers can pass command trust only when the app publisher matches the captured executable signature subject. If either side is missing or mismatched, the command remains blocked unless another specific trust rule, such as safe MSI product uninstall, applies.

Alternatives considered:

- Keep blocking all external uninstallers.
- Trust any external uninstaller that exists on disk.
- Trust any signed external uninstaller, regardless of publisher.

Why chosen:

Some legitimate uninstallers live in common vendor folders outside the app install directory. Existence alone is not enough for a beginner-facing safety assistant, and "signed by someone" is too broad. Matching the captured signature subject to the app publisher is conservative, explainable, and testable.

Consequences:

- Legitimate external uninstallers can now reach the high-risk readiness gate when publisher evidence is available.
- Missing or mismatched signature evidence remains blocked.
- This still does not launch uninstallers; it only allows operation descriptor creation after all other readiness checks pass.

## 2026-06-30 - Trust only interactive MSI product uninstall commands

Decision: The official uninstall command trust layer now treats Windows Installer as trusted only for interactive product removal commands such as `msiexec /x {GUID}` or `msiexec /uninstall {GUID}`. Silent/reduced-UI flags and MSI install/repair commands remain blocked.

Alternatives considered:

- Continue blocking all `msiexec.exe` uninstall commands because they live outside the app install directory.
- Trust every `msiexec.exe` command found in an uninstall registry entry.
- Allow silent MSI uninstall when the user confirms.

Why chosen:

Many legitimate Windows apps use MSI uninstall registry entries, so blocking all MSI commands would make the future clean-uninstall flow too weak. Trusting all MSI commands or silent flags would let OMNIX-Entropy run hidden high-impact changes. Interactive product-code removal is a narrow, explainable middle path.

Consequences:

- MSI apps can now reach the high-risk operation descriptor gate when all other readiness checks pass.
- Silent MSI removal, repair, install, and custom MSI commands remain blocked until stronger evidence exists.
- Signer/publisher matching is still required before any real official uninstaller execution handler should be exposed.

## 2026-06-30 - Official uninstall command trust blocks shells and outside paths

Decision: The official uninstall execution gate now requires a trusted command. V1 trusts uninstallers under the app install directory, blocks shell wrappers such as `cmd.exe` / `powershell.exe`, and blocks uninstallers outside the install directory until signer/source checks exist.

Alternatives considered:

- Treat any existing executable from the uninstall registry entry as trusted.
- Allow shell wrappers if the user confirms.
- Require Authenticode signature checks before any trust decision.

Why chosen:

Registry uninstall commands can point to wrappers or arbitrary scripts. Path trust is simple, testable, and conservative enough for a beginner-facing product. Signature checks are useful but need a separate source of truth and should be added as a later layer, not used as the only first gate.

Consequences:

- Some legitimate uninstallers outside the install directory, including MSI-style entries, remain blocked for now.
- The UI can explain why execution is blocked without running anything.
- Future work should add safe MSI recognition and signer/publisher matching.

## 2026-06-30 - Official uninstaller execution requires a separate readiness gate

Decision: Real official uninstaller execution must be represented by `OfficialUninstallExecutionGate` before any UI or handler can run it. The gate is disabled by default and only creates a high-risk operation descriptor after feature enablement, snapshot id, official-command confirmation, app-close confirmation, post-uninstall rescan confirmation, and existing uninstaller path are all present.

Alternatives considered:

- Keep the uninstall plan permanently presentation-only.
- Add a "run uninstaller" button directly to the WPF window.
- Let `SafetyOperationPipeline` alone discover missing readiness fields at execution time.

Why chosen:

The product needs to move toward real app management, but direct uninstaller execution is a high-risk system-changing action. A separate readiness gate makes missing prerequisites visible to the user before any process execution exists.

Consequences:

- The uninstall safety window can now explain why execution is blocked.
- No official uninstaller handler exists yet; adding one later must still require final confirmation and the operation pipeline.
- Tests cover readiness rules independently of WPF and process launching.

## 2026-06-30 - Post-uninstall residue execution is low-risk quarantine only

Decision: The app drawer residue-review action first rescans software inventory. If the same software still exists, residue handling is blocked. If the software is gone, only low-risk cache/log paths may enter `uninstall.residue.quarantine` after a second confirmation.

Alternatives considered:

- Directly clean every residue candidate after the uninstall-plan button.
- Move medium-risk install/data directories into quarantine too.
- Handle service, startup-entry, and scheduled-task residue in the same flow.

Why chosen:

The user wants a cleaner Geek-like uninstall experience, but the core product constraint is "do not mess things up." Low-risk paths can be rolled back through quarantine; services, startup entries, scheduled tasks, and data directories need stronger evidence, snapshots, and rollback design.

Consequences:

- The current uninstall loop can handle a narrow safe residue class, while real official uninstallers still do not run.
- Medium/high-risk residue is explained only; later work needs snapshot and rollback design before enabling it.

## 2026-06-17 - Adopt shared agent protocol

Decision: Use `AGENTS.md` and `.omx/development/` as the shared source of truth for agent collaboration.

Alternatives considered:

- Startup prompt only.
- Tool-specific skill only.

Why chosen:

Shared project files survive handoff across tools and sessions.

Consequences:

- Agents must keep records current.
- Tool-specific skills can improve behavior but should not replace project state files.

## 2026-06-17 - 架构：单一 OperationPipeline 咽喉点

Decision: 所有操作（UI 点击与 agent 发起）统一走 `IOperationPipeline.ExecuteAsync(OperationDescriptor)`，强制 快照→确认→提权→执行→隔离→日志。

Alternatives considered:
- 各功能模块各自实现删除/提权逻辑。

Why chosen: 安全软件破坏性操作多，统一咽喉点确保不可绕过快照与隔离，agent 与手动操作安全等价。

Consequences: `ITool`（agent 侧）必须包装 `OperationDescriptor`；新增操作类型只需注册到 pipeline。

## 2026-06-17 - 痛点1：强制安装用"检测+自动化+重定位"三招组合

Decision: 不做内核 minifilter 驱动（需 EV 证书 + WHQL，周期/成本对个人自用不合理），也不全局改 `ProgramFilesDir`（破坏 Windows Update/Store）。改用：①前门检测安装程序弹窗选目标 ②按安装程序类型(MSI/Inno/NSIS/Burn)自动改写路径到 D: ③不合作的安装后 diff+robocopy 迁移+junction+注册表修复。

Alternatives considered:
- 文件系统过滤驱动（最强但需签名，排除）。
- 仅目录 junction 全局重定向（脆弱，排除作主机制，仅作重定位细节）。

Why chosen: 无需驱动签名，覆盖多数规范安装程序，并有真实安全网兜底不合作安装程序。

Consequences: 重定位正确性最难点（服务/计划任务/COM 引用旧路径需改写）；安装边界检测需组合进程退出+MSI 变更通知+延迟。

## 2026-06-17 - 痛点2：C: 根因用四阶段扫描器 + 非预期根目录标红

Decision: 四阶段（大块头探测/目录爬取/分类/趋势），杀手特性为"非预期根目录文件夹"检测器（合法根白名单，其余标红）。本机即会标出 ` SETUP539`、`KRECYCLE`、`ZCONFIG55`、`temp`、`tmp`。

Alternatives considered:
- 仅清缓存式瘦身（不解"为什么满"，排除）。

Why chosen: 用户明确"不知什么吃 C:"，需要根因可见而非盲目清理。Phase 1 先发因其纯只读零风险。

Consequences: WinSxS 用 DISM 求真（硬链接虚高）；VSS 尺寸需管理员降级处理；爬取需跳过 reparse point 防死循环。

## 2026-06-17 - 目标框架 net8.0-windows（而非 net10.0-windows）

Decision: 目标 `net8.0-windows`（LTS）。本机仅 .NET 10 SDK，需安装 .NET 8 SDK。

Alternatives considered:
- 直接用本机已有的 net10.0-windows（省安装）。

Why chosen: net8 为 LTS，WPF/Vanara/WPF-UI 等生态兼容更稳；避免预览/最新版坑。

Consequences: 需 `winget install Microsoft.DotNet.SDK.8`；构建前必须先装。

## 2026-06-17 - LLM provider：抽象 + 先接 Claude

Decision: `ILlmProvider` 抽象（Claude/OpenAI/Qwen 可换），先用 Claude，原始 HttpClient+System.Text.Json 实现，API key 用 DPAPI 加密。

Alternatives considered:
- 直接绑某 SDK；本地小模型。

Why chosen: 云端效果好（用户选）；抽象避免供应商锁定；agent 在 Phase 8 最后做（编排的操作需先以工具形式存在）。

Consequences: 破坏性工具由代码硬门控（不靠提示词），绝不自动执行。

## 2026-06-17 - 最小权限：标准用户外壳 + Elevated worker

Decision: WPF 外壳标准用户运行不提权；破坏性/需管理员操作经 `Css.Elevated.exe`（命名管道 IPC，PipeSecurity 限当前用户 SID+SYSTEM）执行，一次性 UAC 会话常驻；只读操作零 UAC。删除=移入 D:\CssQuarantine 隔离区。

Alternatives considered:
- 整个 App 以管理员运行（攻击面大、UAC 烦，排除）。

Why chosen: 最小权限原则；本机系统还原被禁用，首破性操作前检测并提示开启。

Consequences: 需 IPC 协议；隔离区有保留期自动清理与低空间保护。

## 2026-06-30 - V1 采用“决策卡片 + 安全管线”作为统一执行模型

Decision: 所有扫描器、AI 助手、UI 模块先生成 `Recommendation` 决策卡片；只有用户确认后的 `OperationDescriptor` 才能进入 `OperationPipeline`。破坏性动作必须带证据，需快照动作必须带 `SnapshotId`。

Alternatives considered:
- 各模块直接执行清理/迁移/禁用动作。
- AI 助手直接调用工具执行系统修改。

Why chosen: 用户明确要求“帮我做决策，但不能瞎弄”。决策卡片让证据、风险、收益、回滚状态统一可审计；安全管线保证 UI 与 AI 无法绕过确认和快照。

Consequences: 后续功能必须先补推荐卡和操作描述，再接真实执行器；这会稍慢，但能避免清理/迁移误伤。

## 2026-06-30 - 安装路径策略按用户 D 盘分类习惯实现

Decision: 默认安装路径映射为普通软件 `D:\Software\<软件名>\Install`、游戏 `D:\Game\<软件名>\Install`、AI 相关 `D:\Agent\<软件名>\Install`、开发/系统工具 `D:\Development\<软件名>\Install`，并采用“推荐后记住”模式。

Alternatives considered:
- 统一放到一个 `D:\ComputerAssistant` 目录。
- 每次安装都询问。
- 自动强制路径不确认。

Why chosen: 用户已有明确存储体系；“推荐后记住”兼顾控制感和低打扰。

Consequences: 后续需要软件分类器、用户记忆规则和安装后报告来修正误判。

## 2026-06-30 - 计划任务先用 XML 文件只读解析

Decision: 软件画像的计划任务来源先读取 `Windows\System32\Tasks` 下的任务 XML 文件，解析 `Exec/Command`，并按安装路径或名称归属到 `SoftwareProfile`。

Alternatives considered:
- 直接调用 Task Scheduler COM API。
- 用 `schtasks.exe /query /xml` 命令解析输出。

Why chosen: 文件读取方式保持只读、易测试、无进程执行和本地化输出问题；权限不足或 XML 损坏时可以跳过单项，不影响整体软件画像扫描。

Consequences: 某些受保护任务可能无法读取；后续如需更完整覆盖，可补 Task Scheduler COM 作为第二来源，但仍必须保持只读和可降级。

## 2026-06-30 - 清理删除先落为隔离区移动 + 时间线

Decision: V1 的清理类破坏性动作先实现为“移动到隔离区 + 写入 `ActionTimelineStore`”，不直接永久删除。隔离区处理器必须放在 `SafetyOperationPipeline` 之后执行，并在移动前预检所有目标路径。

Alternatives considered:
- 直接删除临时目录或缓存。
- UI 直接调用文件移动服务。
- 每个清理模块自己写日志和还原逻辑。

Why chosen: 用户核心要求是“不能瞎弄、能后悔”。隔离区 manifest 保存原路径和隔离路径，时间线保存证据和影响范围，统一处理器避免 UI 或 AI 绕过安全门。

Consequences: 清理动作释放空间前会先占用隔离区空间；后续需要隔离区容量策略、保留期清理、真实还原 UI 和 elevated worker。

## 2026-06-30 - V1 UI 只允许 low-risk clean.temp 进入隔离区执行

Decision: 决策卡执行入口只允许 `Kind == clean.temp` 且 `Risk == Low`、有证据、有影响路径、要求回滚的操作进入确认对话框；确认后由 `QuarantineOperationPolicy` 复制为 `ConfirmationAccepted=true`，再交给 `SafetyOperationPipeline -> QuarantineOperationHandler`。

Alternatives considered:
- 让所有带 `OperationDescriptor` 的建议卡都能执行。
- 让 UI 直接设置确认并调用隔离区服务。
- 暂时继续只展示建议不执行。

Why chosen: 这让 V1 有第一条可落地测试的安全清理闭环，同时避免误把迁移、禁服务、注册表修改或高风险路径暴露给普通确认按钮。

Consequences: 用户现在只能执行低风险临时目录移动到隔离区；高风险清理、迁移、服务/注册表操作仍需系统快照、提权 worker、还原 UI 和更严格的确认页。
### 2026-06-30 - 前台改为直观电脑管家，软件画像退到后台

- Decision: V1 前台不再展示“软件画像”技术列表，改为首页体检摘要、应用图标网格和应用右侧抽屉。
- Alternatives considered: 继续以软件画像列表为主；纯聊天式 Marvis 入口；只做底层扫描器。
- Why chosen: 用户明确反馈“不懂电脑的人看不懂，懂电脑的不需要”，需要图标、状态、结论和 Agent 建议。
- Consequences: `SoftwareProfile` 继续作为后台数据源；新增前台模型 `HealthCheckSummary`、`AppTileViewModel`、`AppDrawerViewModel`、`AgentRecommendation` 等；UI 必须真实页面切换。

### 2026-06-30 - Marvis 能力只作为安全分级技能目录参考

- Decision: 借鉴 Marvis 的能力分类（诊断、设置、修复、桌面管理、进程服务、硬件查询、工具直达、会话控制），但 V1 只暴露安全分级入口，不复制实现。
- Alternatives considered: 复制 Marvis 式全能 Agent；忽略 Marvis 能力表只做清理工具。
- Why chosen: 用户希望软件像 Agent 一样告诉自己该怎么做，但 OMNIX-Entropy 的安全原则要求 AI 不得绕过确认和回滚。
- Consequences: 新增 Agent 技能目录；高风险技能默认只解释或打开系统工具，不直接修改系统。

### 2026-06-30 - 应用管理先做筛选和卸载方案预览，不执行真实卸载

- Decision: 应用页分类/搜索/排序先落成 `AppCatalogPresenter`，抽屉里的“卸载干净点”先展示 `UninstallPlan` 预览，不运行官方卸载器，也不处理真实残留。
- Alternatives considered: 直接调用卸载命令；只保留静态应用网格；把筛选逻辑写在 WPF code-behind。
- Why chosen: 用户需要直观管理应用，但真实卸载风险高；核心层 presenter 可测试，UI 只负责展示和选择。
- Consequences: V1 现在能解释“可以怎么卸载干净点”，但不会替用户卸载。后续必须补官方卸载确认页、卸载后残留扫描、隔离区还原 UI 后，才允许真实执行。

### 2026-06-30 - 服务扫描增加注册表兜底

- Decision: `SoftwareInventoryScanner` 在 WMI 服务读取之外，额外只读枚举 `HKLM\SYSTEM\CurrentControlSet\Services` 的 `ImagePath`，并按服务名去重。
- Alternatives considered: 继续只依赖 WMI；直接调用 `Get-Service`；要求管理员权限。
- Why chosen: 本机验证显示 CIM/WMI 查询 `Win32_Service` 会被拒绝访问，但服务注册表路径可读；注册表兜底保持标准用户只读能力。
- Consequences: 能识别 MarvisSvr 这类服务；某些系统服务 `ImagePath` 带环境变量或系统路径，后续展示前仍要按软件归属过滤。

### 2026-06-30 - 卸载方案窗口默认不可执行

- Decision: “卸载干净点”打开 `UninstallPlanWindow`，只显示结构化方案；`UninstallPlanPreviewViewModel.CanRunOfficialUninstaller=false`。
- Alternatives considered: 直接运行官方卸载器；用 MessageBox 展示命令；继续只在抽屉小列表展示。
- Why chosen: 用户需要更直观的方案页，但真实卸载仍需要官方卸载确认、残留扫描、隔离区和还原 UI 完整闭环。
- Consequences: 当前可安全解释卸载流程；真实卸载入口仍不开放。

### 2026-06-30 - 后悔药还原以 manifest 为准，不按原路径猜

- Decision: 时间线为隔离区动作保存 `RestoreManifestPaths`，UI 点击还原时调用 manifest；还原成功或失败后用 `ActionTimelineStore.UpdateRestoreStateAsync` 更新同一条时间线记录。
- Alternatives considered: 只显示原路径让用户手工找隔离区；按原路径在隔离区搜索；直接提供全局“全部还原”。
- Why chosen: manifest 是唯一能证明原路径、隔离路径、大小和还原状态的证据；按路径猜容易误还原或覆盖。
- Consequences: 旧时间线记录如果没有 manifest 只显示“不可还原”；已有旧 SQLite 表会自动补 `restore_manifest_paths_json` 列，但旧记录不会凭空获得 manifest。

### 2026-06-30 - 隔离区容量整理先做建议，不自动永久删除

- Decision: `QuarantineRetentionPlanner` 只生成过期、超容量、已还原记录的整理候选；`WouldDeleteAutomatically=false`，每个候选仍要求确认。
- Alternatives considered: 达到保留期后自动删除；只显示总大小不做建议；立即实现永久删除执行器。
- Why chosen: 用户需要释放空间，但“后悔药”一旦自动永久删除会破坏可回滚承诺；V1 应先解释和建议。
- Consequences: 后悔药中心能看到隔离区空间压力，但真正整理隔离区副本还需要单独确认页和安全管线执行器。

### 2026-06-30 - 卸载残留只允许低风险路径进入隔离区计划

- Decision: 卸载后残留扫描按风险分组；缓存/日志路径为低风险，可生成 `uninstall.residue.quarantine` 操作描述；数据/安装目录为中风险，服务/启动项/计划任务为高风险，默认只解释。
- Alternatives considered: 像 Geek 一样把所有残留一次性勾选删除；卸载后只扫描不生成任何可执行计划；直接禁用服务/删计划任务。
- Why chosen: 用户想“卸载干净点”，但不懂服务/任务/注册表风险；低风险路径进隔离区能提供可回滚清理，高风险项需要快照和更明确证据。
- Consequences: 后续 UI 可以把低风险残留接到 `SafetyOperationPipeline -> QuarantineOperationHandler`；高风险残留必须另做快照/回滚方案后才可能开放。

### 2026-06-30 - 官方卸载器确认页先展示，不执行

- Decision: `OfficialUninstallConfirmationBuilder` 只解析并展示官方卸载命令、参数、运行中进程/服务/任务提醒、快照要求和卸载后重扫清单；`CanRunOfficialUninstaller=false`。
- Alternatives considered: 在“卸载干净点”窗口直接运行卸载命令；只显示原始命令不解析；完全不展示确认页。
- Why chosen: 用户需要知道软件会做什么，但直接运行卸载器风险高；解析确认页能降低黑盒感，同时为未来执行门打基础。
- Consequences: 当前 UI 更接近可用卸载流程，但真实执行仍需单独的启用门、快照验证、关闭软件确认和卸载后扫描。

### 2026-07-07 - C-drive recommendation grouping stays in the presentation layer

- Decision: Add `RecommendationListPresenter` to group repeated unexpected-root observe cards for the C-drive page, while leaving `DiskRecommendationBuilder` and `OperationDescriptor` generation unchanged.
- Alternatives considered: Merge findings inside the scanner; hide all observe recommendations; keep one card per path and rely on copy changes.
- Why chosen: Scanner-level merging could lose per-path evidence needed later, and hiding observe findings would reduce user trust. A presentation-layer group reduces UI noise without changing safety or execution semantics.
- Consequences: Beginner users see one source-confirmation card instead of many repeated observe cards. Low-risk cleanup cards still carry their original operation and must pass the same quarantine/safety pipeline.

### 2026-07-07 - Residue review uses a drawer presenter and stays before migration preview

- Decision: Add `UninstallResidueDrawerReviewPresenter` and render post-uninstall residue review in the app drawer's uninstall area, above migration preview.
- Alternatives considered: Keep using WPF private string concatenation; show residue review only in status text; keep residue review below migration preview.
- Why chosen: Residue review is an uninstall workflow, so the result should appear directly under the uninstall actions. A core presenter makes "still installed, do not clean residue" testable without slow GUI scans.
- Consequences: Users see the residue result immediately after clicking `卸载后检查残留`; local paths stay hidden in the beginner drawer text; no new uninstall or cleanup execution path was added.
### 2026-07-07 - Shared uninstall workflow guide is the source of truth

- Decision: Add `UninstallWorkflowGuidePresenter` and use it from both the app drawer uninstall preview and `UninstallPlanPresentationBuilder`.
- Alternatives considered: Keep duplicate drawer/window copy; only add another safety-window section; move all residue details into the drawer.
- Why chosen: The user experience should not force beginners to reconcile two different uninstall explanations. A shared presenter keeps the foreground flow consistent while detailed preflight and residue groups remain available underneath.
- Consequences: Drawer and window now share the same six-step next-action story. Real official uninstaller execution and residue cleanup remain gated/disabled until snapshot, final confirmation, command trust, app-close confirmation, post-uninstall rescan, quarantine, and rollback evidence are represented.
### 2026-07-07 - C-drive recommendation selection state uses a presenter

- Decision: Add `RecommendationSelectionPresenter` for the text and button state shown when the user selects a C-drive recommendation card.
- Alternatives considered: Keep selection copy directly in `MainWindow.xaml.cs`; only rely on each recommendation card's safety line; open the confirmation dialog immediately when selecting a card.
- Why chosen: Beginners need a clear pause between "I selected this" and "I am about to confirm an action." A core presenter makes that pause testable and keeps WPF code-behind from owning product language.
- Consequences: Selecting a low-risk cleanup card now explains quarantine, second confirmation, estimated release, and undo before the button is clicked. Execution semantics stay unchanged.

### 2026-07-08 - Agent next-step advice stays local and presentation-only

- Decision: Add `AgentNextStepPresenter` as a local rules presenter that turns health summary and software profile signals into an Agent page next-step panel.
- Alternatives considered: Keep the Agent page as a static skill catalog; wire a cloud AI answer directly into the UI; create executable operations directly from Agent advice.
- Why chosen: The user wants the software to behave like a helpful computer operations Agent, but the safety model requires AI/advice to explain and prioritize first. Local presenter output is testable, privacy-preserving, and cannot bypass the operation pipeline.
- Consequences: The Agent page now shows a top suggestion, reasons, safe next actions, blocked actions, and privacy/safety boundaries. Future cloud AI can only consume summarized local facts and must still produce auditable local recommendations before execution.

### 2026-07-08 - Agent next actions are navigation-only

- Decision: Represent Agent next-step buttons as structured navigation-only actions with target internal pages, not as operation descriptors.
- Alternatives considered: Keep next actions as plain text only; wire buttons directly to scan/cleanup/app actions; create `OperationDescriptor` objects from Agent suggestions immediately.
- Why chosen: The user wants the Agent to guide decisions, but clicking an Agent suggestion must not surprise them by modifying the system. Navigation-only buttons let the Agent take the user to the relevant page, where existing confirmations and safety pipeline boundaries still apply.
- Consequences: Agent buttons can route to Home, Apps, CDrive, Install, Timeline, or Agent only. Future executable Agent suggestions must be modeled separately as auditable local operation plans after evidence, risk, confirmation, and rollback/quarantine requirements are visible.

### 2026-07-08 - Clean Agent left card by deleting duplicate copy only

- Decision: Remove the duplicate legacy Agent identity/description text blocks while keeping the rest of the Agent left-card XAML structure intact.
- Alternatives considered: Replace the whole Agent left-card region; defer all XAML cleanup until a larger UI pass; leave the duplicate copy because tests/build pass.
- Why chosen: The duplicate copy directly hurts beginner readability, but a whole-region rewrite risks unnecessary churn in a historically encoding-sensitive XAML file. A focused deletion plus regression test fixes the visible issue with low blast radius.
- Consequences: The Agent card now has one clean identity copy and retains existing next-step controls. Broader XAML modernization can happen later with visual QA.
## 2026-07-08 - App drawer action buttons reveal preview panels first

- Decision: The app drawer `clean cache` and `disable startup` buttons now open beginner-facing preview panels instead of attempting execution or showing technical details.
- Alternatives considered: directly create cleanup/startup operation descriptors from the drawer; keep buttons as passive enabled/disabled UI with tooltips only.
- Rationale: The user needs visible guidance when clicking an action, but these actions can affect files, services, startup entries, or scheduled tasks. A collapsed preview panel gives feedback without bypassing evidence, confirmation, rollback/quarantine, or the safety pipeline.
- Consequences: Real cache cleanup and startup disabling remain future work. Any execution path must still be modeled as auditable operation plans with snapshot/confirmation/rollback or quarantine gates.

## 2026-07-08 - Software inventory infers AppData cache candidates

- Decision: The software inventory scanner now augments installed-app profiles with AppData cache/log candidates derived from LocalAppData, Roaming AppData, and LocalLow.
- Alternatives considered: keep cache paths only in tests/manual profiles; wait for a future full disk attribution engine; scan all AppData children and try fuzzy matching every folder.
- Rationale: The real GUI smoke showed the cache button could remain disabled because production profiles often lacked `CachePaths`. Exact candidate generation from app names/install-root segments gives enough evidence for preview without a broad, slow, or risky full attribution scan.
- Consequences: Some apps with vendor-nested data such as `Google\\Chrome` may still need future specialized rules. Current behavior remains read-only and only enables preview/planning, not cleanup execution.

## 2026-07-08 - Nested cache attribution remains exact and read-only

- Decision: Expand cache attribution by generating exact nested roots such as `Vendor\\App`, `App\\User Data`, and known browser profile folders, then only accepting paths that actually exist.
- Alternatives considered: enumerate every AppData child and fuzzy-match names; add browser-specific hardcoded products only; defer all nested attribution to a later disk indexer.
- Rationale: The user needs real app cache previews to work for common browsers/Electron apps, but broad fuzzy matching could blame the wrong software and erode trust.
- Consequences: Chrome/Edge/Electron-style cache evidence is more likely to appear in app drawers. Some unusual profile names may still be missed until a future directory-enumeration rule can be made safe and tested. No cleanup execution was added.

## 2026-07-08 - App drawer preview switching uses a core presenter

- Decision: Move cache/startup preview switching state into `AppDrawerActionPreviewPresenter`.
- Alternatives considered: keep toggling panels and status text directly in `MainWindow.xaml.cs`; bind every panel directly to `AppDrawerViewModel`; create operation descriptors when buttons are clicked.
- Rationale: The user-visible effect of a drawer button is product behavior, not just UI plumbing. Keeping it in a core presenter makes "button gives feedback but does not execute" testable.
- Consequences: WPF code-behind now only applies a state object to controls. Real cleanup/startup execution remains future gated operation-plan work.

## 2026-07-08 - App drawer action previews use one host

- Decision: Introduce one shared action preview host for uninstall, migration, cache cleanup, startup control, and residue-review output.
- Alternatives considered: keep one fixed section per action; remove drawer previews and rely only on modal windows; rewrite the whole drawer layout in one pass.
- Rationale: The user explicitly rejected piled-up technical text. A single host keeps the drawer visually calm: conclusions and buttons first, then only the clicked action's Agent preview.
- Consequences: The drawer no longer defaults to showing uninstall and migration preview lists. Detailed modal windows still exist for uninstall/migration safety plans. Future drawer actions should plug into the shared host instead of adding new always-visible sections.

## 2026-07-08 - Agent skill catalog uses capability-card presenter

- Decision: Add `AgentSkillCardPresenter` as the user-facing presentation layer for Marvis-inspired Agent capabilities.
- Alternatives considered: keep rendering `AgentSkill` directly in `MainWindow`; add more text into the existing `SafetyLine`; wait for cloud AI before improving the skill catalog.
- Rationale: The user wants an Agent that tells them what they can do next, but the app must not imply direct system modification. A core presenter makes each capability's mode, risk, next step, and safety boundary reusable and testable.
- Consequences: The Agent skill page now distinguishes read-only diagnosis, plan-only high-risk actions, and open-only system-tool shortcuts. Future Agent skills should add explicit next-step and safety-hint copy before appearing in the UI.

## 2026-07-08 - System tool shortcuts are allowlisted and explicit

- Decision: Implement system-tool direct access as a fixed allowlist with per-tool risk and confirmation requirements.
- Alternatives considered: accept arbitrary commands from configuration; make system tools only explanatory text; hide Registry Editor entirely.
- Rationale: The user wants Marvis-like "system tools direct" convenience, but arbitrary command launching or surprise high-risk tool opening would violate the product's no-blind-action posture. A fixed catalog gives useful entry points while keeping the action bounded.
- Consequences: The Agent page can open known Windows tools after an explicit button click. Medium/high-risk tools require a confirmation dialog. OMNIX-Entropy still does not click inside those tools or modify the system.

## 2026-07-08 - Medium-risk Windows Settings shortcuts require confirmation

- Decision: Keep low-risk settings deep links one-click, but require confirmation before opening medium-risk Settings pages such as Power/Sleep, Storage, and Installed Apps.
- Alternatives considered: require confirmation for every settings page; never require confirmation because all settings shortcuts are open-only; avoid settings shortcuts entirely.
- Rationale: Beginners benefit from quick navigation to harmless inspection pages, but Storage and Installed Apps can lead to file deletion or uninstall decisions after one more click. A confirmation gate preserves convenience while reminding the user that OMNIX-Entropy is only opening the page and not acting inside it.
- Consequences: Settings shortcuts remain a fixed `ms-settings:` allowlist. Medium-risk entries visibly mark that confirmation is needed and show a safety dialog before launch. No automation inside Windows Settings was added.

## 2026-07-08 - Many resident apps can outrank C-drive app advice

- Decision: If at least three apps are resident/background-capable and no low-risk C-drive cleanup item is waiting, the Agent next-step panel prioritizes reviewing background/resident apps before C-drive app advice.
- Alternatives considered: always prioritize C-drive because it is the product's first pain point; always prioritize resident apps when any one app is running; create a separate heavy background dashboard before changing Agent advice.
- Rationale: The user explicitly wants the assistant to answer "who is secretly running or auto-starting?" A single resident app can be normal, but three or more is enough to deserve a plain-language review prompt without overreacting.
- Consequences: Agent advice becomes more situation-aware while remaining navigation-only. The app still does not terminate processes, disable services, edit startup, or change scheduled tasks from the Agent panel.

## 2026-07-08 - Agent background review shows summaries before technical names

- Decision: The Agent background review panel shows app-level summaries first and hides service names, scheduled-task paths, and raw startup identifiers behind future technical details or app drawers.
- Alternatives considered: show the exact service/task/startup identifiers in the Agent panel; only keep background evidence inside each app drawer; create a separate full background-management page immediately.
- Rationale: The user has repeatedly said dense technical text is not useful for beginners. The Agent should answer "which apps deserve attention and what should I do next?" before exposing implementation details.
- Consequences: The Agent page now gives a compact resident-app review after software scans. Real disabling/closing remains future work and must become an auditable plan with evidence, confirmation, rollback, and safety pipeline gates.

## 2026-07-08 - Startup/service review starts as a plan preview

- Decision: Turn resident app evidence into an Agent plan preview before implementing any startup, service, scheduled-task, or process action.
- Alternatives considered: add a real disable/close button now; show only the background app list; put raw startup/service/task names in the Agent panel; defer this until a dedicated background-management page exists.
- Rationale: The user wants the software to decide and explain, but "disable service" and "close process" can break sync, drivers, security tools, or input devices. A plan preview lets the Agent explain what it would check and what evidence is required without modifying the system.
- Consequences: The Agent now shows a safe next-step proposal with snapshot, confirmation, and rollback requirements. Any future real action still needs an `OperationDescriptor`, safety pipeline gates, and rollback evidence.

## 2026-07-08 - Agent plan previews appear before detailed app lists

- Decision: In the Agent left card, show the startup/service plan preview immediately after the background summary and before the detailed resident-app list.
- Alternatives considered: leave the plan below the app list; make the whole Agent left card scroll-only; move the plan into the right skill catalog column.
- Rationale: Screenshot review showed the plan existed but was nearly hidden at the bottom. Beginner users need the conclusion and proposed next step before a scrollable list of apps.
- Consequences: The first visible Agent area now prioritizes summary and plan. The detailed app list remains available below for context, and a static order test protects this placement.

## 2026-07-08 - Settings shortcuts prioritize Storage and Apps

- Decision: Show Windows Settings direct links before system tools/skill catalog, and order Storage, Installed Apps, and Power/Sleep before lower-risk settings.
- Alternatives considered: keep low-risk settings first; keep settings below system tools; rely on the small settings ListBox scrollbar; only verify confirmation behavior in unit tests.
- Rationale: Storage and Installed Apps map directly to the user's C-drive and software-management goals, but they are medium-risk because Windows Settings can lead to cleanup or uninstall decisions. Putting them first with visible "needs confirmation" copy makes the useful path obvious while preserving safety.
- Consequences: Agent right-card content is now scrollable, Settings appears first, and GUI smoke can click the Storage entry and cancel the confirmation dialog without launching Settings.

## 2026-07-08 - Confirmation cancel path needs GUI proof

- Decision: Add a real WPF smoke that opens the medium-risk Storage confirmation dialog, cancels it, and checks no new `SystemSettings` process appears.
- Alternatives considered: trust the existing `RequiresConfirmation` unit tests; click OK in smoke and rely on not changing settings; skip GUI proof because MessageBox automation is brittle.
- Rationale: The safety promise is user-facing: cancel must actually stop navigation. A real cancel smoke proves the gate works at the WPF interaction layer without opening or modifying Windows Settings.
- Consequences: The `.omx` smoke script has robust dialog search and cancel fallbacks. Future medium-risk shortcuts should reuse this pattern rather than only relying on catalog tests.

## 2026-07-08 - App drawer action smoke verifies exposed controls, not decorative containers

- Decision: Use stable AutomationIds on the app drawer action buttons and exposed preview title/summary/list controls. Do not require UIAutomation discovery of the decorative `Border` host.
- Alternatives considered: keep relying on localized button text; require `DrawerActionPreviewPanel` itself to be discoverable; switch the visual host to a different WPF control just for automation.
- Rationale: The user cares that the clicked action produces readable feedback. WPF `Border` containers are not reliable UIAutomation targets, while buttons, text blocks, and list boxes are.
- Consequences: The four-button GUI smoke is less brittle and closer to what the user sees. Future WPF smokes should target interactive/readable controls rather than layout containers.

## 2026-07-08 - Drawer action GUI smoke selects an eligible app per action

- Decision: The app-drawer smoke searches scanned app tiles for an enabled button separately for uninstall, migration, cache, and startup previews.
- Alternatives considered: use the first scanned app for every action; force-enable disabled buttons for smoke; assert only cache/startup because those are usually available.
- Rationale: Disabled migration can be correct when an app is already installed reasonably on D drive. The smoke should verify real product behavior without weakening those UI rules.
- Consequences: The smoke now proves all four user-facing action entry points while respecting disabled states. Future action smokes should choose data that satisfies the action preconditions instead of assuming a single selected item covers every case.

## 2026-07-08 - App drawer action host becomes an Agent action card

- Decision: Add explicit `AgentTakeaway`, `NextStepText`, and `SafetyText` fields to the shared app-drawer action host instead of relying only on a summary plus a detail list.
- Alternatives considered: keep the existing list-only preview; replace the detail list entirely; push all explanations into modal windows.
- Rationale: The user does not want dense technical instructions. A small action card lets the drawer answer "what do you think, what happens next, and what will not happen?" before showing details.
- Consequences: Uninstall, migration, cache cleanup, startup control, and residue review now share the same three-part explanation model. Future drawer actions should populate these fields and keep `CanExecuteDirectly` explicit.

## 2026-07-08 - Drawer action previews scroll into view after clicks

- Decision: Wrap the app drawer content in `AppDrawerScrollViewer` and call `DrawerActionPreviewPanel.BringIntoView()` whenever an action preview becomes visible.
- Alternatives considered: shrink the top drawer summaries further; leave the preview below the fold because the smoke can find it; open every action in a modal.
- Rationale: Screenshot review showed the action card could be technically generated but too low to read. Beginner-facing feedback must move into the visible area immediately after a click.
- Consequences: The drawer can hold richer context without clipping the action card. Future drawer content additions should check screenshot visibility after action clicks, not only static XAML presence.
### 2026-07-08 - Undo center GUI proof does not seed user timeline data

- Decision: The undo-center GUI smoke verifies the visible title, quarantine policy, timeline list, and restore-button affordance using the app's existing empty/message state. It does not insert fake entries into `%LocalAppData%` or create quarantine files just to make a screenshot.
- Alternatives considered: Seed a temporary timeline row in the user's default app database; create a real quarantine record before launching the app; rely only on core unit tests with no GUI proof hooks.
- Why chosen: The undo center is safety-critical and should not pollute the user's real recovery history. Static UI hooks plus core quarantine/restore tests prove the restore model, while GUI smoke can remain read-only.
- Consequences: The current GUI smoke proves the restore affordance exists, but a future end-to-end screenshot with a real restorable record should use an isolated test profile or app-data override before running.

### 2026-07-09 - GUI smokes use opt-in storage root overrides

- Decision: Add `AppStoragePathResolver` with `OMNIX_ENTROPY_DATA_ROOT` and `OMNIX_ENTROPY_QUARANTINE_ROOT`. Normal app runs keep existing defaults, while GUI smokes can opt into isolated data/quarantine roots.
- Alternatives considered: Keep using the real `%LocalAppData%` and D-drive quarantine root; add command-line arguments to the WPF app; hard-code `.omx` test paths in `MainWindow`.
- Why chosen: Environment variables are explicit, process-scoped for smoke scripts, easy to restore in `finally`, and avoid adding UI-facing test switches. Keeping the resolver in Core makes default path behavior testable.
- Consequences: Future GUI smokes can safely seed timeline/quarantine records under `.omx` without touching the user's real recovery data. Any production launcher that sets these variables would redirect storage, so smoke scripts must restore prior values.

### 2026-07-09 - Seed undo-center GUI state through a dev smoke tool

- Decision: Add `Css.SmokeTools seed-undo-center` and call it from `.omx/gui-undo-center-smoke.ps1` after setting isolated storage roots.
- Alternatives considered: Hand-write SQLite and manifest JSON in PowerShell; seed the user's default app database; add hidden seed behavior to the WPF app; rely only on unit tests and skip a restorable-row screenshot.
- Why chosen: The dev tool reuses the real Core services and safety pipeline, avoids brittle manual SQLite/JSON schema duplication in PowerShell, and does not add test-only behavior to the user-facing WPF app.
- Consequences: The solution now includes a small dev/test executable that must be built before the seeded GUI smoke. Future GUI smokes can add commands to this tool instead of polluting production UI switches.

### 2026-07-09 - Undo timeline first-level detail hides raw paths

- Decision: `ActionTimelinePresenter` summarizes affected paths as `影响范围：N 个位置` in the first-level timeline row.
- Alternatives considered: Keep showing the first two affected paths; only hide paths for smoke-seeded records; add a technical details expander in this slice.
- Why chosen: The user explicitly rejected path-heavy screens. A count preserves the safety/audit signal without making beginners read long local paths.
- Consequences: Technical path inspection still needs a future secondary detail entry. The first-level undo center is cleaner and the seeded GUI screenshot no longer exposes test-local paths.

### 2026-07-09 - Shared WPF smoke helper starts with undo-center consumer

- Decision: Introduce `.omx/wpf-smoke-helpers.ps1` and migrate the undo-center smoke first.
- Alternatives considered: Refactor every GUI smoke in one pass; leave duplication until more smokes are added; put all helpers in each script to keep them standalone.
- Why chosen: The undo-center smoke had fresh verification and uses the core helper set. Migrating one consumer keeps the refactor small while establishing the shared pattern.
- Consequences: Future GUI smoke scripts should dot-source the helper instead of duplicating UIAutomation setup, wait loops, element invocation, and screenshot capture. A later slice can migrate app-drawer, Agent, and settings smokes.

### 2026-07-09 - App drawer smoke keeps action-specific helpers local

- Decision: Migrate app-drawer smoke to shared WPF primitives but keep action-specific helpers (`Find-ControlByNameParts`, list-item selection, preview-window closing) in `.omx/gui-app-drawer-preview-smoke.ps1`.
- Alternatives considered: Move every helper into `.omx/wpf-smoke-helpers.ps1`; leave app-drawer smoke unchanged; rewrite all GUI smokes to a framework-like script.
- Why chosen: The shared helper should stay generic. App-drawer selection and modal-closing behavior is tied to this specific smoke and should not become a global API until another script needs it.
- Consequences: The script now has less duplicated UIAutomation boilerplate while retaining clear local flow for its four action previews.

### 2026-07-09 - Storage override env vars are documented as dev/test only

- Decision: Add `docs/development/gui-smokes.md` and explicitly state that `OMNIX_ENTROPY_DATA_ROOT` and `OMNIX_ENTROPY_QUARANTINE_ROOT` are for development and GUI smoke tests only.
- Alternatives considered: Put notes only in `.omx/development`; add comments only in scripts; expose the variables as an advanced user setting.
- Why chosen: Packaging and future agents need a durable, discoverable warning outside transient worklogs. The variables redirect core storage and should not be treated as a normal user-facing option.
- Consequences: Future packaging docs should classify `.omx` scripts and `Css.SmokeTools` as development/test tooling and avoid exposing these env vars in beginner-facing UI.

### 2026-07-09 - Agent system-tools smoke uses shared primitives only

- Decision: Migrate `.omx/gui-agent-system-tools-smoke.ps1` to shared WPF primitives while keeping Agent-specific navigation/list assertions local.
- Alternatives considered: Move Agent list/button counting into the shared helper; leave the smoke unchanged because it was already passing; migrate multiple Agent smokes in one patch.
- Why chosen: The shared helper should stay generic and low-risk. The system-tools smoke has a narrow safety promise: verify visible entry points without clicking tool/settings buttons. Keeping that flow local makes the no-open boundary obvious.
- Consequences: Three GUI smokes now share the same UIAutomation/screenshot foundation. Remaining Agent smokes can be migrated one at a time without turning `.omx/wpf-smoke-helpers.ps1` into a product-flow framework.

### 2026-07-09 - Settings confirmation smoke uses native window fallback

- Decision: For `.omx/gui-agent-settings-confirm-cancel-smoke.ps1`, find confirmation dialogs by first checking UIAutomation child windows, then Win32 `EnumWindows` by process id, and only then protected UIAutomation descendants.
- Alternatives considered: Keep root-descendant UIAutomation search with catch/retry only; rely on screenshots and skip dialog automation; click OK instead of cancel to prove the dialog appeared.
- Why chosen: Root-descendant UIAutomation can fail because unrelated desktop providers throw COM exceptions. Native top-level window enumeration is narrower and directly models what the smoke needs: another window owned by the launched WPF process.
- Consequences: The smoke remains able to cancel the real confirmation dialog and verify no `SystemSettings` process is launched, while being less sensitive to desktop UIAutomation provider failures.

### 2026-07-09 - Undo timeline raw paths live behind collapsed technical details

- Decision: Keep timeline row summaries path-free by default, but add a collapsed technical-details affordance that includes raw affected paths, manifest paths, record id, restore state, and restore operation.
- Alternatives considered: Keep hiding raw paths entirely; put raw paths back into the first-level row; open a separate technical window for every timeline entry.
- Why chosen: Beginner users need the row to answer "what happened and can I undo it?" without reading paths, while safety/audit workflows still need exact evidence available before restore or investigation.
- Consequences: Future timeline rows should populate concise `Detail` plus structured `TechnicalDetails`. GUI smokes should prove the details affordance exists, but should not click restore or mutate data.

### 2026-07-09 - C-drive cleanup selection preview is structured but non-direct

- Decision: Add structured selection-preview fields for C-drive cleanup (`AgentTakeaway`, `NextStepText`, `SafetyBoundary`, and `PlanLines`) while keeping `CanExecuteDirectly=false`.
- Alternatives considered: Keep the existing single explanatory paragraph; make the first button execute low-risk cleanup immediately; move all explanation into the confirmation MessageBox only.
- Why chosen: The user needs the assistant to explain what should happen before asking for a risky confirmation. A structured preview answers "what do you recommend, what happens next, and can I regret it?" without bypassing the existing quarantine/safety pipeline.
- Consequences: The C-drive recommendation area now has a clearer pre-confirmation plan. Future cleanup confirmation windows should reuse the same language and avoid introducing a one-click direct cleanup path.

### 2026-07-09 - Cleanup confirmation starts with plain summary and keeps technical paths later

- Decision: Move cleanup confirmation copy into `CleanupConfirmationPresenter`, with beginner summary first and raw paths/quarantine root in a technical details section.
- Alternatives considered: Keep the inline path-first MessageBox; hide paths entirely; build a custom confirmation window immediately.
- Why chosen: A presenter is the smallest safe improvement: the user sees a clearer decision prompt now, while exact evidence remains available before execution. A custom window can still come later.
- Consequences: Future cleanup confirmation UI should use the presenter or its model. If a custom dialog replaces MessageBox, it should keep the same two-layer structure instead of reintroducing path-first copy.

### 2026-07-09 - Cleanup confirmation uses a custom WPF dialog

- Decision: Replace the low-risk cleanup `MessageBox` with `CleanupConfirmationWindow`, using visible summary plus collapsed technical details.
- Alternatives considered: Keep MessageBox with improved text; open the technical details expanded by default; delay custom dialog until a full cleanup wizard exists.
- Why chosen: The user wants fewer dense text piles. A small custom dialog gives clearer hierarchy now and keeps the confirmation gate explicit without changing execution policy.
- Consequences: Future cleanup confirmations should use this window or a shared successor. Real GUI smoke still needs a stable low-risk fixture before claiming visual proof.

### 2026-07-09 - C-drive GUI smoke uses process-scoped scan-root override

- Decision: Add `OMNIX_ENTROPY_CDRIVE_SCAN_ROOT` through `AppDevelopmentPathResolver` for development GUI smoke fixtures, instead of exposing a user-facing C-drive path picker or relying on the real C drive.
- Alternatives considered: Scan the user's real C drive in the smoke; add a visible test/demo mode to the WPF UI; seed recommendations directly without running the scanner; remap fixture paths to fake `C:\` paths.
- Why chosen: A process-scoped override keeps normal product behavior automatic and beginner-safe, while the smoke can scan a tiny controlled fixture and open the real recommendation/confirmation UI. Keeping fixture paths real also means an accidental confirm would only affect `.omx` fixture data, not `C:\Temp`.
- Consequences: The override must remain documented as development/test-only and should not be packaged as a normal user setting. Future GUI smokes that need controlled local state should prefer process-scoped env vars plus cleanup in `finally`.

### 2026-07-09 - Top-level Temp/tmp roots become temp cleanup candidates

- Decision: Extend app scan rules so top-level `Temp` and `tmp` directories classify as `UsageCategory.Temp`, not `Other`.
- Alternatives considered: Keep them as unexpected-root observations; make the smoke use a custom rules file; special-case only `.omx` fixture paths.
- Why chosen: Real users commonly have `C:\Temp` or `C:\tmp`, and the product can now route them into the same low-risk, quarantine-gated cleanup flow instead of a confusing observe-only card. The fixture gets the same behavior without special recommendation injection.
- Consequences: `C:\Temp`/`C:\tmp` can now become actionable low-risk cleanup recommendations, but execution remains gated by selection, custom confirmation, `QuarantineOperationPolicy`, `SafetyOperationPipeline`, and quarantine/undo records.
## 2026-07-09 - Show cleanup outcome preview as a separate confirmation section

- Decision: Add `OutcomePreviewLines` to the cleanup confirmation view model and render it as a dedicated section between the Agent summary and collapsed technical details.
- Alternatives considered: Fold the outcome copy into `BeginnerText`; rely on the technical details expander; leave the outcome only in the earlier recommendation preview.
- Rationale: Beginner users need to understand "what happens if I confirm" at the last safety gate, while exact paths remain available only on demand.
- Consequence: The presenter gains one more display collection, and GUI smokes can assert a stable `CleanupConfirmationOutcomeListBox` before screenshot/cancel.

## 2026-07-09 - Use ListBox hooks for uninstall plan proof targets

- Decision: In `UninstallPlanWindow.xaml`, expose core plan collections as named `ListBox` controls with stable AutomationIds instead of relying on anonymous `ItemsControl` elements.
- Alternatives considered: Add AutomationIds only to section headers; keep `ItemsControl` and rely on visual screenshots; wait until a GUI smoke exists.
- Rationale: Project UX rules require beginner-facing safety panels to have stable UIAutomation targets. `ListBox` is more reliably visible to WPF UIAutomation smokes than decorative or anonymous containers.
- Consequence: The window remains plan-only, but future smokes can verify workflow steps, official confirmation checks, and residue sections without invoking uninstall or cleanup.

## 2026-07-09 - Add uninstall plan GUI smoke as close-only proof

- Decision: Add `.omx/gui-uninstall-plan-window-smoke.ps1` as a close-only GUI smoke that opens the uninstall plan window and verifies readable safety-plan controls.
- Alternatives considered: Extend the existing app-drawer action smoke; wait for real GUI approvals; implement real official-uninstall execution first.
- Rationale: The user wants "卸载干净点", but the safe next proof is that the plan window opens and explains the flow without executing anything.
- Consequence: A future GUI run can capture `.omx\qa-uninstall-plan-window.png`; real uninstall remains gated and unimplemented.

## 2026-07-09 - Locate uninstall plan modal by descendant control when root-child search misses it

- Decision: Keep the top-level child window check, but add a fallback that finds `UninstallPlanTitleTextBlock` from root descendants and walks up to its owning window.
- Alternatives considered: Use only native process-window enumeration; use a real mouse-click fallback first; broaden all assertions to root descendants without scoping to the modal.
- Rationale: Prior project evidence and the failed smoke show this modal can be missed as a root child, while the stable child AutomationId is a reliable proof target. Walking to the parent window keeps later assertions scoped.
- Consequence: The uninstall plan smoke now has a reusable local pattern for this modal; if the same pattern repeats in more smokes, promote it into `.omx/wpf-smoke-helpers.ps1`.

## 2026-07-09 - Reuse cleanup confirmation window for low-risk uninstall residue

- Decision: Make the post-uninstall low-risk residue path use `CleanupConfirmationPresenter` and `CleanupConfirmationWindow` instead of its own path-first `MessageBox` confirmation.
- Alternatives considered: Keep a dedicated residue MessageBox; create a second residue-specific confirmation window; defer residue confirmation UX until real uninstall execution exists.
- Rationale: The cleanup confirmation already explains quarantine, undo-center timeline, non-permanent deletion, safety boundaries, and collapsed technical details. Reusing it keeps destructive-adjacent confirmations consistent for beginner users.
- Consequence: Low-risk residue confirmation is easier to understand and test, while the actual execution remains behind `QuarantineOperationPolicy`, explicit confirmation, `SafetyOperationPipeline`, and `QuarantineOperationHandler`.

## 2026-07-09 - Promote WPF modal discovery into shared smoke helper

- Decision: Move descendant-based modal discovery into `.omx/wpf-smoke-helpers.ps1` and make both C-drive cleanup confirmation and uninstall-plan smokes call the shared helper.
- Alternatives considered: Keep duplicated functions in both scripts; copy the function only when the next smoke fails; broaden smoke assertions to root descendants without scoping to the modal.
- Rationale: The same root-child modal lookup failure occurred in two independent WPF smokes. A shared helper keeps the reliable child-AutomationId-plus-parent-window-walk pattern in one place.
- Consequence: Future modal smokes can reuse `Find-SecondaryWindowWithChild`; individual scripts stay focused on scenario setup and close/cancel safety.

## 2026-07-09 - Residue review refreshes software inventory before deciding

- Decision: `ReviewSelectedUninstallResidueAsync` must run a fresh software inventory scan before deciding whether the selected app is still installed.
- Alternatives considered: Keep the cached `_softwareProfiles` shortcut; compare only install path existence; add a test-only bypass around the still-installed branch.
- Rationale: The selected app comes from the current cached inventory, so a cached-first still-installed check makes the real low-risk residue confirmation path unreachable after a user supposedly uninstalls the app. A fresh scan models the product truth better and keeps residue cleanup blocked unless the app is no longer detected.
- Consequence: Residue review can now reach the low-risk confirmation path after a fresh scan shows the app is gone, while still blocking cleanup if the app remains installed.

## 2026-07-09 - Software inventory GUI fixture is process-scoped and dev-only

- Decision: Add `OMNIX_ENTROPY_SOFTWARE_FIXTURE` and `SoftwareInventoryFixtureScanner` for GUI smokes, instead of touching real installed applications or adding a visible demo mode.
- Alternatives considered: Use the real registry/uninstall inventory in the smoke; seed fake apps into production state; add a user-facing test mode; mock the residue handler without launching WPF.
- Rationale: The residue confirmation path needs a state transition from "installed" to "not installed". A process-scoped JSON scan sequence proves that transition through the real WPF flow without querying or mutating the user's registry, services, startup entries, scheduled tasks, or installed apps.
- Consequence: Future software-inventory GUI smokes can script safe state transitions. The override must stay documented as development/test-only and must not become a packaged user setting.

## 2026-07-09 - Residue outcomes are shown inline after cancel or quarantine

- Decision: After a residue confirmation is canceled or succeeds, update the app drawer action host with a clear outcome panel.
- Alternatives considered: Keep only `StatusTextBlock`; show another informational `MessageBox`; defer outcome display until a full audit/timeline UI exists.
- Rationale: Beginner users should see "nothing happened" after cancel and "moved to quarantine, restorable in undo center" after success in the same working area where they made the decision.
- Consequence: The residue flow now has explicit non-executable outcome view models. Future destructive-adjacent flows should follow the same pattern instead of relying on transient status text alone.

## 2026-07-09 - Outcome primary actions are navigation-only

- Decision: Add an optional primary action to app drawer outcome panels, but allow only safe page navigation for now.
- Alternatives considered: Add a restore button directly in the residue outcome; keep the undo-center mention as plain text only; make every action host preview show a button.
- Rationale: The user needs a clear next step after quarantine, but restore is a separate high-impact action with its own evidence and overwrite checks. A navigation-only button preserves clarity without bypassing the undo center.
- Consequence: Successful residue quarantine can offer "查看后悔药中心"; cancel outcomes and ordinary previews keep the button hidden. Any future outcome action must declare a safe action key and be tested against direct restore/pipeline calls.

## 2026-07-09 - Install routing memory affects recommendations only

- Decision: Store install-routing memory as app-data JSON and apply it only during read-only installer analysis.
- Alternatives considered: Add a visible global Windows install-directory change; auto-pass remembered paths to installers; store rules only in memory without persistence; keep only default D-drive roots.
- Rationale: The user wants OMNIX-Entropy to remember where software should go, but V1 must not silently run installers or globally alter Windows installation behavior. A persisted recommendation memory is the safe next step toward learning mode.
- Consequence: Exact software rules override category rules, category rules override defaults, and analyzer output marks when a route came from memory. Future UI can add "remember this route" confirmation without changing installer execution safety.

## 2026-07-09 - Remember route action writes memory only after confirmation

- Decision: The install page's "remember this route" action saves the analyzed route to `install-routing-memory.json` only after a user confirmation dialog.
- Alternatives considered: Save automatically after every analysis; wait for a richer rule editor before saving anything; run the installer with the remembered path immediately.
- Rationale: Learning mode should feel helpful but not surprising. Explicit confirmation creates a clear user-intent boundary while still making future installer analysis smarter.
- Consequence: The current button is safe but coarse: it remembers the current software route. A future UI should let the user choose whether the memory applies to this software only or the whole category.

## 2026-07-09 - Use a scope choice window for install route memory

- Decision: Replace the install-route memory `MessageBox` with a dedicated choice window that offers software-only memory, category memory, and cancel.
- Alternatives considered: Keep OK/Cancel and always remember software; use Yes/No/Cancel with implicit meanings; build a full rule editor immediately.
- Rationale: Beginner users need the decision stated plainly. A dedicated window can explain "this software only" versus "same category" and repeat that no installer will run.
- Consequence: Learning mode is more understandable and less surprising. The next improvement should show learned rules and offer reset/edit without exposing raw JSON.

## 2026-07-09 - Show learned install rules before editing them

- Decision: Add a read-only learned-rules list before adding reset/edit actions.
- Alternatives considered: Add delete/reset buttons immediately; leave rules hidden in JSON; build a full rule-management page.
- Rationale: The user should first be able to see what the Agent remembers. Visibility is lower risk than mutation and gives a clearer product loop.
- Consequence: Install guard now has an audit surface for learned rules. Deleting or editing rules can be added later with confirmation and clear "this only affects future suggestions" wording.

## 2026-07-09 - Forgetting install memory only affects future recommendations

- Decision: The learned-rule forget action edits only OMNIX-Entropy's install-routing memory file and must state that installed apps and installers are untouched.
- Alternatives considered: Require a full operation pipeline action; hide rule deletion until a settings page exists; allow immediate delete without confirmation.
- Rationale: Forgetting an app-owned recommendation rule is lower risk than system mutation, but it is still a user-visible memory change and should require confirmation.
- Consequence: Users can correct Agent memory safely. Future reset/edit features should reuse the same "future recommendations only" safety boundary.

## 2026-07-10 - Post-install reports lead with cards, not raw diffs

- Decision: Show install after-change results as beginner-readable summary cards first, with raw paths/services/tasks kept in a collapsed technical-details section.
- Alternatives considered: Keep writing the full diff directly into the first visible textbox; hide technical details entirely; add execution buttons for detected C-drive/background changes immediately.
- Rationale: The user needs to know what changed and whether it deserves attention before seeing paths or service names. Keeping technical details available preserves auditability without making the first screen intimidating.
- Consequence: The install page now has a clearer read-only report surface. Future actions for "clean cache", "migrate", or "review background item" should be generated as separate auditable plans rather than executed from the report itself.

## 2026-07-10 - Install report explanations are on demand and plan-only

- Decision: Keep the install report cards visible by default, reveal the fuller Computer Agent explanation only after the user asks, and keep all resulting next steps non-executable.
- Alternatives considered: Always show another large explanation block; place raw paths/service names in the explanation; add direct clean/migrate/disable buttons to the report.
- Rationale: The install page already contains routing memory and snapshot controls. An on-demand explanation avoids more default clutter while still giving beginners a clear judgment and sequence. Direct actions would skip evidence-specific plan and confirmation stages.
- Consequence: The report now explains C-drive writes and background changes without exposing technical identifiers. A later action-plan surface must remain separate and route any real mutation through the local safety pipeline.

## 2026-07-10 - Fixture GUI proof must include screenshot inspection

- Decision: Treat UIAutomation assertions as necessary but insufficient; beginner-facing conclusion work also requires a screenshot that visibly shows the conclusion in the working area.
- Alternatives considered: Trust `IsOffscreen=false`; keep only static XAML order tests; use the user's real installed software for visual proof.
- Rationale: The first smoke returned a positive visibility flag while its screenshot did not show the Agent panel. An isolated two-scan fixture plus visual inspection proves the real rendered flow without touching installed software.
- Consequence: The install page now has a process-scoped GUI smoke, scroll support, stable AutomationIds, and two retained evidence images. Future desktop smokes should fail review when their screenshots contradict automation output.

## 2026-07-10 - Agent chooses the safe sequence before asking for operations

- Decision: The install report action plan automatically orders “preserve and classify C-drive content”, “keep and review background items”, then “observe future growth”; it does not ask beginners to choose among raw paths, services, or tasks.
- Alternatives considered: Show technical checkboxes; immediately offer delete/migrate/disable buttons; provide only another prose explanation.
- Rationale: The product promise is decision support for people who do not understand Windows internals. A short ordered plan gives a concrete next step while preserving evidence and avoiding premature system changes.
- Consequence: The current surface is intentionally non-executable. The next slice should classify findings more specifically before any proposal can become an `OperationDescriptor`.

## 2026-07-10 - PowerShell smoke assertions use ASCII source for localized text

- Decision: Build localized assertion keywords from Unicode char codes in Windows PowerShell smoke scripts instead of embedding non-ASCII literals in no-BOM UTF-8 `.ps1` source.
- Alternatives considered: Add a BOM to one script; trust visual screenshots only; compare the entire localized UIA name.
- Rationale: Windows PowerShell 5.1 can decode the source literal differently from WPF's runtime Unicode text, while ASCII-only char codes are stable across host encoding settings.
- Consequence: GUI smokes can still validate critical localized safety copy without changing product text or relying on machine code pages.

## 2026-07-10 - Evidence classification is preliminary, private, and compact

- Decision: Classify every install-report finding in the presentation layer, expose only aggregate category counts in the default plan, and label individual judgments as preliminary.
- Alternatives considered: Show every path/name inline; provide no classification until cloud AI exists; add another always-visible detail list below the plan.
- Rationale: Local rules can already distinguish high-value categories without sending data anywhere. A compact summary improves Agent judgment while preserving the user's “no text pile” requirement and keeping technical evidence auditable on demand.
- Consequence: The plan now says what the new C-drive content likely is and which background mechanisms appeared. A later on-demand review surface can explain each numbered item without revealing identifiers by default.

## 2026-07-10 - Evidence explanations stay collapsed and non-selectable

- Decision: Put per-finding evidence behind one collapsed `为什么这样判断` expander and render its lists as read-only, non-selectable content.
- Alternatives considered: Show all evidence below every plan by default; expose raw paths and service names in the beginner view; open a separate technical window; add action buttons beside each finding.
- Rationale: The user explicitly rejected dense software-profile text. A single optional explanation preserves Agent accountability without making beginners operate on technical evidence or mistake a selected row for a required choice.
- Consequence: Default plans remain compact. Users can inspect generic purpose/confidence/risk when curious, while raw identifiers stay in technical details and future actions must be derived as separate plan-only recommendations.

## 2026-07-10 - Evidence authorizes plan types, never operations

- Decision: Map classifications only to candidate plan types with explicit missing-evidence and rollback fields; never treat a heuristic category as permission to execute.
- Alternatives considered: Add a clean/migrate/disable button for each detected finding; let services and scheduled tasks produce disable candidates; show every technically possible action regardless of evidence.
- Rationale: Beginners asked the Agent to make decisions for them, but a local name/path heuristic cannot safely authorize system mutation. Services and tasks can be core functionality, so uncertain or high-risk evidence must resolve to observation.
- Consequence: Agent can narrow the next step to five understandable plan types. Future preview generation may reuse safe planners, but execution still requires complete evidence, explicit confirmation, rollback/quarantine, and `OperationPipeline`.

## 2026-07-10 - Candidate previews require unique evidence ownership

- Decision: Generate app-specific cache, startup, or migration previews only when exactly one newly added software profile owns the relevant evidence; otherwise return a visible refusal with missing-evidence guidance.
- Alternatives considered: Attribute all new C-drive/background evidence to the first newly added app; show a generic app-specific plan despite ambiguity; hide the candidate entirely.
- Rationale: Install diffs can contain unrelated concurrent changes. A plausible path or startup name is not enough to authorize even a personalized preview, while an explicit refusal teaches the user what must be verified next.
- Consequence: Cache/startup/migration previews reuse existing safety presenters only after ownership checks. Storage and observation stay generic, all previews remain non-executable, and no `OperationDescriptor` is created.

## 2026-07-10 - WPF smokes manage visibility, not keyboard focus

- Decision: Desktop smoke startup may restore and place its test window at `HWND_TOPMOST`, but must not require `SetFocus` or call `SetForegroundWindow`.
- Alternatives considered: Poll UIAutomation focus; repeatedly request native foreground ownership; treat a focus timeout as a product failure.
- Rationale: UIAutomation `InvokePattern` does not need keyboard focus, screenshots need only a visible unobscured window, and Windows intentionally limits background foreground activation.
- Consequence: Shared smoke helpers now use `ShowWindowAsync` plus `SetWindowPos`. GUI verification can distinguish app readiness from OS focus policy instead of failing on an irrelevant state.

## 2026-07-10 - Tell the truth about uninstall recovery before enabling execution

- Decision: Treat official uninstall and residue cleanup as two different recovery stories. Official uninstall is not restored by the quarantine center and generally requires reinstall; only confirmed low-risk residue moved to quarantine is directly restorable.
- Rejected: Presenting a JSON evidence snapshot or residue quarantine as if either could restore an uninstalled application.
- Consequence: The beginner UI must show this distinction before technical preflight details, and real official-uninstaller execution remains disabled until recovery evidence and confirmation are stronger.

## 2026-07-10 - Reinstall metadata is a hint until file and publisher identity agree

- Decision: Automatically accept reinstall recovery evidence only when the source is an existing EXE/MSI installer and its signature subject matches the scanned software publisher.
- Alternatives considered: Trust `InstallSource` directories; treat an MSI product code as proof; accept any existing installer file; ask beginners to judge raw paths and signatures.
- Rationale: Registry values can be stale or point only to a folder, product codes do not guarantee that installation media is available, and unrelated installer files can be dangerous. Publisher-signature matching is the minimum local evidence that can be explained without asking the user to understand Windows internals.
- Consequence: Weak metadata remains visible only as an Agent hint and advanced provenance. Verified installer evidence can support a future confirmation flow, but it does not confirm personal-data backup or authorize execution.

## 2026-07-10 - Existing restore points are fallback hints, not automatic recovery proof

- Decision: Read existing Windows restore points with a SELECT-only WMI query and summarize their count, but do not let them complete application recovery preparation.
- Alternatives considered: Treat any restore point as sufficient rollback; automatically create or invoke a restore point; hide restore-point information entirely.
- Rationale: A restore point may be old and may not reconstruct every application file or personal-data state. It is useful context but not a substitute for a verified installer and separate backup confirmation.
- Consequence: The recovery panel can reassure users that a system-level fallback exists without overstating it. Restore creation/use remains outside this slice and would require a separate high-risk operation.

## 2026-07-10 - Evidence snapshots support audit, never application rollback

- Decision: Replace arbitrary snapshot identifiers with an atomic, versioned, hashed local manifest that explicitly says `CanRestoreApplication=false`.
- Alternatives considered: Keep accepting any non-empty id; call the evidence manifest a rollback snapshot; depend only on a Windows restore point.
- Rationale: Post-uninstall residue comparison needs a trustworthy before-state, but that record cannot recreate removed program files. Conflating audit evidence with rollback would mislead the beginner and weaken the gate.
- Consequence: Official-uninstall readiness now requires a recent matching manifest and typed reinstall recovery evidence. Future operation descriptors retain manifest provenance for audit.

## 2026-07-10 - Final-confirmation drafts remain data, not operations

- Decision: Let completed recovery preparation create a verified evidence manifest and a final-confirmation draft, but prohibit the draft service from producing an operation or calling execution infrastructure.
- Alternatives considered: Create a high-risk operation as soon as preparation completes; write snapshots even for refused drafts; expose technical paths in the beginner checklist.
- Rationale: Recovery preparation proves only that rollback prerequisites exist. Closing software, reading the official command, accepting no-one-click-undo, committing to post-scan, and final user intent are separate facts.
- Consequence: A later UI can present the draft without gaining execution authority. Any future request must explicitly convert the confirmed draft through the official gate and local pipeline.

## 2026-07-10 - Snapshot retention starts with a plan and preserves unknown files

- Decision: Identify expired/excess evidence using a read-only planner; never delete or move malformed, unfamiliar, reparse, or outside-root files.
- Alternatives considered: Delete all old JSON files by filename; auto-clean the snapshot directory; ignore retention/privacy entirely.
- Rationale: Snapshot roots contain sensitive paths, but a broad cleanup rule could destroy unrelated evidence. Valid schema/purpose/id checks and a plan-only first stage keep ownership explicit.
- Consequence: Future retention execution can accept only planner candidates and must archive reversibly through the local operation pipeline.

## 2026-07-10 - Retention execution reuses quarantine and rolls back batches

- Decision: Archive validated evidence manifests through the existing quarantine/timeline infrastructure, with hash-bound descriptors and reverse-order rollback for partial failures.
- Alternatives considered: Directly move into a new archive format; permanently delete expired manifests; trust the earlier plan without execution-time validation.
- Rationale: Existing quarantine records already provide restore provenance and user-facing undo semantics. Files can change after planning, so root/hash/schema checks must repeat immediately before movement.
- Consequence: Snapshot retention has a reversible execution primitive but no UI exposure. Any future caller must select planner candidates, confirm the descriptor, and use `SafetyOperationPipeline`.

## 2026-07-10 - Build official uninstall execution logic before making it reachable

- Decision: Implement the official-uninstall handler behind injected launcher/post-scan interfaces, but provide no real launcher adapter and no App/Elevated registration in this slice.
- Alternatives considered: Wire `Process.Start` directly from WPF; keep only plan previews indefinitely; add the launcher and handler registration together.
- Rationale: Descriptor/snapshot/command validation and failure semantics can be exhaustively tested without touching a real application. Reachability should be a separate, auditable decision after GUI recovery preparation is visually accepted.
- Consequence: Backend semantics now exist for future execution, including mandatory post-scan and truthful partial-failure payloads, while current users still cannot launch an uninstaller from OMNIX-Entropy.

## 2026-07-10 - Isolate the real Windows process API behind one unregistered runner

- Decision: Keep `Process.Start` only in `SystemProcessRunner`; let `WindowsOfficialUninstallerLauncher` build/interpret requests through `IWindowsProcessRunner`.
- Alternatives considered: Call process APIs directly in the handler or WPF; mock static process methods; register the real launcher immediately.
- Rationale: A narrow runner makes start-info, UAC cancellation, exit code, and cancellation propagation testable without launching anything. Registration remains a separate high-risk decision.
- Consequence: Real launch capability is compiled but unreachable. Future wiring can be audited by searching for one adapter type and one runner.

## 2026-07-10 - Reverify post-uninstall paths and defer background conclusions

- Decision: Build directory residue candidates only from current path probes; treat startup, service, and scheduled-task names captured before uninstall as unverified hints requiring a fresh specialized scan.
- Alternatives considered: Reuse every manifest entry as a residue candidate; claim the uninstall is clean when inventory fails; immediately register the adapter with the real launcher.
- Rationale: A pre-uninstall manifest proves prior state, not current residue. Acting on stale service or startup names could disable unrelated or recreated system state, and failed inventory cannot prove removal.
- Consequence: The adapter can truthfully report app presence and current path candidates while keeping background review explicit, read-only, unregistered, and non-executable.

## 2026-07-10 - Post-uninstall beginner text is derived from typed state, not raw summaries

- Decision: Build visible conclusions only from success, software-presence, residue counts, risk groups, and background-rescan state; never echo raw scanner summaries into the beginner UI.
- Alternatives considered: Display the scanner summary directly; show paths and service names in the main result; offer a cleanup button whenever the count is nonzero.
- Rationale: Exceptions and low-level reports may contain private paths or identifiers, and a candidate count does not authorize mutation. Fixed state-based language is auditable and easier for beginners to trust.
- Consequence: The presenter can be reused by a future WPF result panel without leaking technical evidence or gaining execution authority; detailed evidence remains a separate future view.

## 2026-07-10 - Background residue uses tri-state exact-name re-enumeration

- Decision: Probe only startup/service/task identifiers captured in the hashed pre-uninstall manifest and classify each as Exists, Missing, or Unknown; Unknown prevents a successful mandatory background scan.
- Alternatives considered: Enumerate and heuristically associate all background items; treat access failures as missing; copy historical identifiers directly into the residue report.
- Rationale: Exact manifest ownership narrows scope, while tri-state results preserve uncertainty caused by permissions, malformed identifiers, traversal attempts, or reparse points. Absence must be observed, not inferred from an exception.
- Consequence: Verified current matches can appear as high-risk technical residue, but they never become direct disable/delete authority. Real Windows probes remain isolated and unregistered.

## 2026-07-11 - Final elevated handoff is correlation- and evidence-bound data

- Decision: Convert a gate descriptor into a confirmed elevated request only after fresh visual proof and exact final consent; deep-copy and hash the descriptor, then correlate any typed response by request id.
- Alternatives considered: Let WPF set `ConfirmationAccepted` directly; pass the mutable gate descriptor; display raw operation errors/payloads; wire the handler immediately.
- Rationale: Final intent, UI truth, descriptor integrity, and response identity are separate facts. A request DTO can model all four without gaining execution authority.
- Consequence: The future transport has a narrow contract, but no current caller can launch anything because the composer/handler/launcher remain unregistered.

## 2026-07-11 - Restore-point availability preserves Unknown and never blocks plan viewing

- Decision: Bound the read-only WMI query and return Completed/TimedOut/Failed; timeout or failure is presented as “temporarily cannot confirm,” while verified reinstall and backup evidence remain the actual preparation gate.
- Alternatives considered: Wait indefinitely; call timeout an empty result; remove restore-point context; make restore points mandatory.
- Rationale: A fallback hint must not freeze the product or be overstated as application recovery proof.
- Consequence: Beginners can always open the safety plan, and uncertainty remains visible rather than becoming a false “none found” claim.

## 2026-07-11 - WPF smoke discovery uses native handles after semantic UIA lookup

- Decision: Keep AutomationId/semantic UIA lookup first, then enumerate process-owned top-level HWNDs and create AutomationElements from handles before the descendant fallback.
- Alternatives considered: Increase waits; click by screen coordinates; weaken the window assertion; assume root children contain every owned modal.
- Rationale: The screenshot and lifecycle markers proved the modal was rendered with a distinct HWND while UIA root enumeration omitted it. Native enumeration identifies the real window without bypassing child AutomationId checks.
- Consequence: Shared GUI smokes remain semantic but tolerate Windows UIA tree differences for owned modal windows.

## 2026-07-11 - Final confirmation is visible preparation, not an execution control

- Decision: Put the existing final-confirmation draft inside the recovery panel as an on-demand checklist; allow incomplete state to explain missing evidence without writing, and complete state to create only a verified audit snapshot.
- Alternatives considered: Add the real run button alongside the checklist; hide the checklist until everything is ready; generate snapshots on window open; expose manifest paths in the beginner panel.
- Rationale: Beginners need to see why the Agent is refusing before they can supply recovery evidence. Snapshot creation should follow complete preparation, while execution intent remains a separate future gate.
- Consequence: WPF now demonstrates the final safety handoff but still cannot execute. A clean screenshot of this new state is mandatory before post-scan UI or registration work advances.

## 2026-07-11 - Post-scan display contracts belong in Core, not the elevated executable

- Decision: Move only the path-free post-scan view model/state to Css.Core.Uninstall; keep the scanner-to-view presenter in Css.Elevated, and let WPF bind the safe model without referencing the elevated project.
- Alternatives considered: Add a Css.App reference to Css.Elevated; duplicate a WPF-only display model; expose raw post-scan result types directly to the window.
- Rationale: The app needs presentation data, not launcher/handler authority. A neutral display contract avoids making the elevated runtime a UI dependency and preserves one source of beginner text.
- Consequence: The result window is compile-time isolated from execution components. A DEBUG-only fixed fixture provides visual proof and is absent from release compilation.

## 2026-07-11 - Visual proof tickets are short-lived single-use process state

- Decision: Hash validated PNG bytes at issue time, retain only the receipt in memory, expire tickets after ten minutes, and require one-time consumption before request composition.
- Alternatives considered: Persist screenshots; accept a caller-provided hash; allow reusable receipts for 24 hours; treat the ticket as a cryptographic defense against hostile local software.
- Rationale: Current need is to prevent accidental bypass, stale UI state, mutable evidence, and replay while avoiding private screenshot persistence. Cross-process authenticity needs a separately designed authenticated channel.
- Consequence: The request session is now stateful and replay-safe, but remains unregistered. Future transport work must not overstate this process-local gate as a complete security boundary.

## 2026-07-11 - Final consent and safe response display contracts are shared Core data

- Decision: Place the final user-consent record, consent presentation/builder, and path-free response view model in Css.Core.Uninstall while keeping response interpretation and execution in Css.Elevated.
- Alternatives considered: Reference Css.Elevated from Css.App; duplicate consent/response models in WPF; pass handler payloads directly to the UI.
- Rationale: The non-elevated app needs exact intent and safe display data, not elevated execution types. Shared pure contracts preserve one meaning across UI and backend without broadening reachability.
- Consequence: Css.App remains independent of Css.Elevated, and both Debug and Release builds retain the same production dependency boundary.

## 2026-07-11 - Visual fake flow and authenticated backend proof remain separate

- Decision: Use a DEBUG-only fixed consent-to-result flow for WPF proof, and a separate authenticated in-memory endpoint for request/handler/result proof.
- Alternatives considered: Reference the elevated project from WPF solely for the smoke; claim the fixed UI fixture exercised the backend; register a real endpoint early.
- Rationale: Combining layers before a real IPC contract would hide authority boundaries. Separate proofs make it explicit that a screenshot validates UX while handler integration validates backend semantics.
- Consequence: Both halves are verified, but production reachability remains absent until a serialized, identity-checked transport connects them.

## 2026-07-11 - Authenticate before recording replay state and recompute descriptor integrity

- Decision: Validate structure, freshness, HMAC, and recomputed descriptor SHA-256 before recording message id/nonce; record authenticated messages before invoking the endpoint handler.
- Alternatives considered: Trust the descriptor hash sent by the caller; record every received nonce; mark replay only after handler completion; catch cancellation as endpoint failure.
- Rationale: Caller-owned request objects may be mutated, unauthenticated traffic must not exhaust replay capacity, concurrent replay must be blocked before side effects, and cancellation is not a failure response.
- Consequence: The in-memory transport has deterministic tamper/replay semantics suitable for a future named-pipe codec, but does not yet authenticate Windows process identity.

## 2026-07-12 - Named-pipe identity is OS-derived and responses are path-free typed data

- Decision: Use `PipeOptions.CurrentUserOnly`, derive the connected client SID/PID/session and server PID/session from the pipe, compare them with launch-session expectations, and serialize only typed status/count/exit-code response fields.
- Alternatives considered: Trust SID/PID fields supplied in JSON; serialize `OperationResult` and scanner summaries directly; reference `Css.Elevated` from WPF; register the real handler while transport tests are green.
- Rationale: Caller-claimed identity is not identity, and raw handler payloads can contain local paths or exception text. OS-derived peer identity plus request-bound response authentication gives a narrow local boundary without exposing execution authority to the UI.
- Consequence: A real Windows pipe can exercise an authenticated fake endpoint safely. The client/codec still need extraction to a neutral library before WPF can use them without depending on the elevated executable.

## 2026-07-12 - Shared IPC depends inward on Core, never outward on Elevated

- Decision: Put pure request/result/presentation contracts in Core and transport/authentication/pipe mechanics in `Css.Ipc`, with Ipc referencing only Core. App may reference Ipc; Elevated may reference Ipc; App must never reference Elevated.
- Alternatives considered: Add a DEBUG-only App-to-Elevated project reference; duplicate DTOs/hash logic in App; keep fixed WPF data disconnected from the pipe; move the handler into Ipc.
- Rationale: The UI needs communication capability and safe data, not process-launch or system-mutation authority. A one-way dependency graph makes accidental authority expansion visible in project files and Release binaries.
- Consequence: DEBUG WPF can prove the real pipe journey while real execution remains separately searchable and unreachable. Production work now needs a secure cross-process session bootstrap, not another project-boundary exception.

## 2026-07-12 - Derive session secrets after OS identity binding

- Decision: Use fresh ephemeral P-256 ECDH plus client/server nonces, bind the transcript to pipe/session/SID/PID/Windows-session/public keys, derive 32 bytes with HMAC-SHA256 extract/expand, and require role-specific finished MACs from both peers.
- Alternatives considered: pass a key through command line/environment/file; use ECDH without finished confirmation; reuse one application key; trust a caller-supplied PID inside JSON.
- Rationale: The named pipe already supplies OS peer identity and exact process correlation. ECDH avoids transporting a secret, transcript binding prevents context substitution, and two-way confirmation proves both peers derived the same key before authenticated commands begin.
- Consequence: The protocol can support a future elevated worker without secret side channels. It still must be composed only after real pipe identity checks and tested in a separate process before any Elevated Program registration.

## 2026-07-12 - Prove worker lifecycle in SmokeTools before touching Elevated Program

- Decision: host the first complete bootstrap plus authenticated request cycle in development-only `Css.SmokeTools`, launched through a test-injected non-elevated adapter, while leaving `Css.Elevated/Program.cs` unchanged.
- Rejected alternative: register a provisional fake or real worker mode directly in the elevated executable before child identity, timeout, and orphan cleanup were proven.
- Consequences: cross-process protocol and lifecycle behavior now have real evidence without broadening production authority. The smoke tool may receive only pipe/session/client identity/timeout metadata; all session keys remain ephemeral inside the pipe handshake.

## 2026-07-12 - App owns capture; Core owns pure visual receipt state

- Decision: move the non-executable in-memory receipt issuer/request session to Core and keep WPF rendering in App. The elevated project retains no screenshot ownership or UI dependency.
- Rejected alternatives: keep a fixed DEBUG hash; make App reference Elevated; capture the entire desktop with screen-copy APIs; persist PNG evidence to disk.
- Consequences: App can bind a real shown window to one-time request composition without gaining process/system authority. PNG bytes exist only long enough to hash and are then zeroed; Core stores only the receipt hash and flags. External screenshot inspection remains a separate UX gate.

## 2026-07-12 - Prepare production request before exposing any worker

- Decision: wire the real plan page through final consent to an in-memory typed request, but keep worker launch and every uninstall adapter unreachable in this slice.
- Rejected alternatives: expose the existing handler directly from WPF; mark command/app-closed confirmation true before the user acts; accept the snapshot hash stored in memory without rereading the manifest; show Continue for refused drafts.
- Consequences: the production UI now reaches the exact request boundary, including explicit app/tray closure, fresh file hash, revalidated signed recovery installer, and one-time visual receipt. The next lifecycle slice can consume this typed request without redesigning user consent.

## 2026-07-12 - Normalize WPF visuals before receipt encoding

- Decision: render a full-size background root through an origin-normalized `VisualBrush` after flushing Dispatcher Render priority.
- Rejected alternatives: keep direct `RenderTargetBitmap.Render` on a margin-offset child; accept nonblank pixels as sufficient despite visible crop/black blocks; use desktop screen-copy APIs.
- Consequences: receipt rendering is deterministic in immediate STA tests and remains window-content-only, in-memory, and independent of desktop occlusion.

## 2026-07-12 - App owns worker launch; Ipc owns lifecycle; Elevated remains fake-only

- Decision: keep `Process.Start`/`runas` in an injectable App adapter, place only process-neutral lifecycle contracts and authenticated one-shot orchestration in Ipc, and register a fake-only Elevated mode whose result truthfully reports that no uninstaller ran.
- Rejected alternatives: reference Elevated from App; launch directly from the WPF plan window; register the real handler as soon as transport tests passed; expose the fake response through the production uninstall button.
- Rationale: process creation is a host concern, transport must remain reusable and authority-free, and a fake response must never be mistaken for a user-visible completed uninstall.
- Consequences: exact child identity, UAC-cancel mapping, handshake/response failure states, and orphan cleanup are testable. The WPF request still dies with its window until packaging and real UAC smoke are verified.

## 2026-07-12 - Package Elevated as files, not as an App assembly dependency

- Decision: let the App MSBuild target build and copy the four worker runtime files for the same configuration, but do not add an App `ProjectReference` to Elevated and require `Css.App.deps.json` to remain free of Elevated.
- Rejected alternatives: add a normal or `ReferenceOutputAssembly=false` project reference; locate a worker elsewhere on the machine; let users browse for a replacement executable; defer worker packaging until after real handler registration.
- Rationale: the worker must be a predictable sibling artifact without granting the UI compile-time access to privileged implementation types. Build/publish evidence is required before any runtime wiring.
- Consequences: Debug, Release, and publish outputs are runnable as a pair. A signed-binary/publisher trust policy is still required before a real handler can be exposed.

## 2026-07-12 - Worker lifecycle results are Agent conclusions, not protocol reports

- Decision: map every typed lifecycle state to a short title, status, plain conclusion, Agent next step, and explicit no-change statement; keep all paths, identity values, cryptography, raw status names, and errors out of visible text.
- Rejected alternatives: display enum names or exception messages; reuse the post-uninstall result even when no uninstaller ran; hide failures behind a generic toast; expose retry as an immediate action.
- Rationale: beginners need to know whether anything happened and what to do next. A distinct result prevents a fake transport success or UAC cancellation from looking like an uninstall result.
- Consequences: the panel is non-executable and visually verified. Technical audit data can be retained separately if needed later.

## 2026-07-12 - Production worker identity is Windows trust plus exact certificate thumbprint

- Decision: require successful embedded Authenticode `WinVerifyTrust` for both App and worker, then compare normalized signer certificate thumbprints exactly. Subject text is informational only and never grants authority.
- Rejected alternatives: subject-name equality; certificate presence without Windows trust; file hash alone; accepting unsigned Release files; trusting catalog signatures for separately shipped OMNIX executables.
- Rationale: subjects can be duplicated, an embedded certificate can accompany an invalid signature, and a hash has no publisher identity. OMNIX controls its package and can require embedded signatures on both shipped executables.
- Consequences: current local unsigned builds cannot authorize production. Release signing must use the same certificate for App and worker; certificate rollover must sign both together.

## 2026-07-12 - Unsigned development pairs may verify transport but never production execution

- Decision: allow development verification only when both exact sibling files are unsigned, readable, and SHA-256 bound; mixed trust states, invalid signatures, mismatch, or probe failures are rejected. Production remains a separate `CanLaunchProduction` fact.
- Rejected alternatives: disable all local smoke until a signing certificate exists; treat DEBUG as implicitly trusted; reuse the production boolean for tests.
- Rationale: local development needs to test real UAC/IPC without purchasing or exposing a signing key, but that convenience must be structurally incapable of enabling real operations.
- Consequences: DEBUG smoke can continue with fake `UninstallerStarted=false`; production composition must explicitly consume `CanLaunchProduction` and revalidate the worker hash.

## 2026-07-12 - Recheck the actual worker image before any IPC bootstrap

- Decision: require every successful launch result to carry expected full path and SHA-256, then independently query and hash the started process image before opening the pipe. Compare paths case-insensitively after full-path normalization and hashes with `CryptographicOperations.FixedTimeEquals`.
- Rejected alternatives: trust the pre-launch hash alone; wait until after bootstrap to inspect; accept a missing expectation in development; expose actual paths or hashes in the result UI.
- Rationale: the launcher check and Windows image creation are separate events. The lifecycle must fail closed before an uninstall request can leave App, while beginners only need to know that OMNIX stopped safely.
- Consequences: every launcher implementation must provide image evidence; inspection failures become a distinct security stop and the child tree is cleaned up. This narrows but does not eliminate attacks by an administrator capable of manipulating process images or package ACLs; signed installation location policy remains required before release.

## 2026-07-12 - Every started uninstaller is followed by read-only observation

- Decision: once an official uninstaller starts and returns any exit code, run the mandatory post-scan exactly once before interpreting success or failure. Preserve non-zero as failure and attach scan truth separately.
- Rejected alternatives: scan only after exit code zero; treat common non-zero codes as success; mutate residues during the same worker session; swallow cancellation as a scan failure.
- Rationale: cancellation and errors can still leave partial filesystem, registry, startup, service, or task changes. OMNIX must observe the resulting state without pretending the uninstall succeeded.
- Consequences: partial outcomes become auditable and may require a scan retry; no residue action is authorized.

## 2026-07-12 - Elevated worker independently authorizes the actual connected package

- Decision: after OS pipe-peer validation and before cryptographic bootstrap, resolve the actual App PID and current worker PID, require Windows-trusted embedded signatures on both, and compare certificate thumbprints exactly. Production session composition remains absent from Program and WPF.
- Rejected alternatives: trust App command-line metadata; reuse an App-side authorization boolean; compare publisher subjects; authorize after reading the request; register real mode before signed-package evidence exists.
- Rationale: the elevated boundary must not inherit authority from an untrusted caller, and denial must occur before any authenticated request can reach execution code.
- Consequences: current unsigned builds are intentionally unable to run production uninstall. A release signer and signed-package smoke remain prerequisites for registration.

## 2026-07-12 - Final-confirmation time is authenticated request authority

- Decision: store the verified final consent's `ConfirmedAtUtc` in the ready request, include it in HMAC and strict wire schema v2, and enforce a 15-minute maximum age plus 30-second future skew at both endpoint and production session.
- Rejected alternatives: rely only on fresh message send time; set preparation time when the worker connection begins; leave time outside the authentication tag; silently accept schema v1.
- Rationale: a signed App must not be able to refresh an old user decision merely by wrapping it in a new transport message.
- Consequences: old clients are deliberately incompatible with v2; users must reconfirm after 15 minutes; stale rejection remains a typed no-uninstaller-started result.

## 2026-07-12 - Production worker mode is registered before App can request it

- Decision: register the narrow real mode in Elevated now, protected internally by actual pipe-peer validation, independent same-certificate trust, fresh authenticated request, safety pipeline, verified manifest, and read-only post-scan; keep every App/WPF call site absent.
- Rejected alternatives: wait to compile real adapters until UI wiring; expose the mode from App before signed-package evidence; rely only on App trust; reuse fake delay switches; launch a second `runas` from the already elevated worker.
- Rationale: the privileged composition must be testable as a real process and prove unsigned self-denial before any user-facing execution control can reach it.
- Consequences: manually invoking the current mode cannot authorize unsigned packages. Future App wiring must explicitly select production mode only after `CanLaunchProduction` and must preserve the same image/hash checks.

## 2026-07-12 - Elevated post-scan uses a minimal fail-closed inventory reader

- Decision: read only DisplayName, Publisher, InstallLocation, and UninstallString from the three Windows uninstall registry views inside Elevated; throw on unreadable product entries so post-scan fails rather than claiming disappearance.
- Rejected alternatives: reference the full Css.Scanner project; silently skip unreadable entries; duplicate all software-profile enrichment in Elevated.
- Rationale: production residue confirmation needs presence truth, while the general scanner introduced dependency-version warnings and unnecessary high-privilege surface.
- Consequences: Elevated dependencies stay narrow and zero-warning; detailed profile enrichment remains in the normal App scanner.

## 2026-07-13 - Production launch authority is a distinct launcher type

- Decision: require `IOfficialUninstallProductionWorkerLauncher` for `RunProductionOnceAsync`; construct the Windows implementation only from `CanLaunchProduction=true`, the trusted worker path, and its exact SHA-256. Reject production launchers in the fake method and development launchers in the production method before process creation.
- Rejected alternatives: add a boolean mode to the existing public launcher; let WPF choose a command string; infer production completion from response content; expose internal argument construction publicly for tests.
- Rationale: a caller should not gain production authority by toggling a value on the DEBUG launcher, and a fake transport success must never be worded as a real uninstall completion.
- Consequences: WPF must obtain the production launcher through the trust factory. Unsigned builds can still run explicit DEBUG verification but cannot enter the production lifecycle.

## 2026-07-13 - WPF submits only to a trust-gated execution coordinator

- Decision: inject `IOfficialUninstallProductionExecutionCoordinator` into the uninstall plan window. The coordinator owns package reassessment, production runner creation, lifecycle invocation, and typed response conversion; WPF owns only final request delivery and result display.
- Rejected alternatives: construct launcher/lifecycle directly in the click handler; let MainWindow pass a mode string; leave the prepared request inert; show a post-scan view for any transport response.
- Rationale: connecting the user workflow must not move privileged command composition into UI code, and unsigned builds need a useful fail-closed conclusion before UAC.
- Consequences: the product path is now wired, but current unsigned builds intentionally stop at trust. A successful post-scan can be displayed only after `CompletedProduction` and request-correlated response validation.

## 2026-07-13 - Finished-phase framing failures are key-confirmation failures

- Decision: once both sides have derived a session key and are reading the role-bound finished message, map malformed payload, invalid frame, truncation, or disconnect to `KeyConfirmationFailed`; preserve cancellation/timeout and all pre-finished protocol classifications.
- Rejected alternatives: relax the flaky test to allow two statuses; disable test parallelism; keep generic `ProtocolRejected` after key derivation.
- Rationale: failure at this exact stage means the peer did not prove possession of the negotiated key/transcript, regardless of whether the bad evidence was a tag, schema, frame, or early close.
- Consequences: tamper telemetry is stable and semantically stronger; generic codec tests still report protocol errors outside a live key-confirmation phase.

## 2026-07-13 - Residue paths stay local after production uninstall

- Decision: keep the elevated IPC result limited to authenticated status/counts. After a completed production post-scan recommends review, retain the exact pre-uninstall `SoftwareProfile` in App, refresh inventory locally, and rebuild path/identifier groups through the existing residue scanner.
- Rejected alternatives: serialize residue paths/services/tasks through IPC; rely only on counts for quarantine; require the removed app tile to remain selected; automatically quarantine immediately after the result window.
- Rationale: concrete paths are needed for a confirmed local operation but are unnecessary private data at the elevated transport boundary. The pre-uninstall profile is the correct correlation anchor after the registry entry disappears.
- Consequences: App performs one local rescan before review; only locally revalidated low-risk paths can reach quarantine. Background/high-risk counts can trigger explanation but cannot become a file operation through the wire result.

## 2026-07-13 - Migration execution is adapter-owned and manifest-hash bound

- Decision: keep Core migration coordination independent of Windows move mechanics. Require an atomically written, bounded, SHA-256 verified rollback manifest whose snapshot, destination, and original path set exactly match the confirmed operation; delegate every move/redirect/rollback to an injected adapter.
- Rejected alternatives: parse human-readable plan steps; trust a mutable JSON path without hash; let WPF call `Directory.Move`; assume “apps closed” confirmation proves processes/services stopped; serialize raw filesystem commands in operation arguments.
- Rationale: cross-volume migration is the highest-risk local mutation after uninstall. Evidence validation, activity observation, move mechanics, rollback, and monitoring need separately testable authority boundaries.
- Consequences: the engine is fully testable with fixture adapters and cannot itself move user paths. Production enablement still requires a Windows adapter and manual evidence.

## 2026-07-13 - Migration closure means the original path remains a verified redirect

- Decision: persist each original path and expected redirect target after success. Later scans classify a missing redirect, changed target, or recreated non-redirect directory as needing attention; only an intact expected redirect is healthy.
- Rejected alternatives: compare directory size through a junction; treat any existing original path as failure; monitor only the destination; mark migration complete permanently after the first successful move.
- Rationale: normal app writes through a junction change destination contents and should not look like C-drive regression. The closure invariant is redirect identity, not size stability.
- Consequences: OMNIX can distinguish healthy writes routed to D from software that removed/bypassed the redirect and resumed real C writes.

## 2026-07-13 - Directory migration commits only after full tree hash verification

- Decision: copy a real directory to a unique destination-side staging directory, reject all reparse entries and bounded-limit overflow, compare sorted relative inventory, length, and SHA-256 for every file, then atomically rename staging to the final destination. Verify again before deleting source and creating the redirect.
- Rejected alternatives: `Directory.Move` across volumes; follow junctions; compare only total bytes; create redirect before destination verification; invoke `cmd /c mklink`; delete source incrementally during copy.
- Rationale: the source must remain the authoritative copy until a complete independently verified destination exists. Shell command construction would add parsing/injection and process authority unnecessarily.
- Consequences: migrations take additional I/O but have a clear recovery point. Files/reparse roots and very large trees outside safety limits are refused.

## 2026-07-13 - Native migration redirect requires elevation

- Decision: use the .NET directory symbolic-link primitive behind `IWindowsDirectoryRedirector` and run it only inside the future signed/elevated migration worker. Do not fall back to shell-based junction creation from the normal App.
- Rejected alternatives: silently require Developer Mode; call `mklink`; weaken the operation's elevation requirement; fake a redirect by leaving a copied directory at C.
- Rationale: the normal-process probe returned `UnauthorizedAccessException`, and the overall migration operation already needs elevation for consistent identity, service checks, and rollback authority.
- Consequences: real WPF migration remains disabled until the elevated worker contract and manual UAC/rollback evidence exist.
## 2026-07-13 - Continue source development while artifact generation is paused

- Decision: continue implementing migration composition and tests as source, but do not compile, execute tests, launch Worker/UAC, restore the flagged DLL, add antivirus exclusions, or touch real migration paths until corrected Huorong definitions are installed.
- Alternatives rejected: stopping all development wastes time after vendor false-positive confirmation; whitelisting or disabling protection would remove an important safety signal.
- Consequence: functional progress continues, but all new code remains explicitly unverified and cannot enable user-facing execution until the deferred build/test/security gate passes.

## 2026-07-13 - A migration snapshot must still match the source at execution time

- Decision: after verifying snapshot and rollback evidence, re-observe every source directory in Elevated immediately before path mutation and compare canonical path, directory/redirect state, aggregate bytes, and last-write time.
- Rejected alternatives: treat a fresh snapshot JSON as permanent authorization; rely only on the user's “app closed” acknowledgement; compare only path existence in the move adapter.
- Rationale: evidence can be valid while the source changes between confirmation and execution. A beginner-facing safety promise requires stale plans to stop rather than silently move new data.
- Consequences: preflight performs another bounded read-only inventory and may ask the user to regenerate the plan. Failures stay path-free and occur before mutation.

## 2026-07-13 - WPF may request migration only through an injected coordinator

- Decision: the plan and final-consent windows may compose the confirmed request and call `IMigrationProductionExecutionCoordinator`; worker mode strings, launcher construction, lifecycle clients, process launch, and file movement remain outside WPF.
- Rejected alternatives: construct the elevated launcher in the click handler; expose a generic command box; mark transport completion as migration success.
- Rationale: the UI needs a complete future flow without owning privileged authority. Correlation and typed `Completed` semantics must remain enforceable at the coordinator boundary.
- Consequences: the UI path is source-wired but stays disabled under current readiness. Unsigned packages stop before UAC, and refused/rolled-back responses cannot set `CompletedProduction`.

## 2026-07-13 - Installer routing arguments require hash-bound type evidence

- Decision: treat filename inference as display-only. Bind marker inspection to the same locked file bytes and SHA-256 used by Authenticode evidence; automatically pass a target directory only for trusted, high-confidence Inno/NSIS packages.
- Rejected alternatives: infer Inno/NSIS/Burn from filenames; try common MSI/Burn properties; pass silent switches; open unsigned packages with a warning; globally change `ProgramFilesDir`.
- Rationale: unsupported arguments can silently install to C, break installers, or hide choices from a beginner. Unknown or unsigned evidence should stop, not invite experimentation.
- Consequences: some legitimate installers remain guided or refused. MSI/Burn/generic EXE show the recommended folder but receive no guessed arguments; MSIX stays Windows-managed.

## 2026-07-13 - Installer launch is a fresh manual operation, not Agent authority

- Decision: require a hash-verified before-inventory evidence file, four explicit acknowledgements, a 15-minute confirmation, `OperationSource.Manual`, and independent handler revalidation before a dedicated interactive launcher may run. Keep the WPF feature flag false until deferred build/security/UI proof passes.
- Rejected alternatives: let analysis imply consent; let Agent call the launcher; request `runas`; treat process exit code as install success; put `Process.Start` in WPF; skip post-scan when the installer exits nonzero.
- Rationale: an installer can change the whole machine and may still write C even when its main files target D. The user must see that limitation, and the execution boundary must fail closed if any evidence changes.
- Consequences: the App can later open a verified interactive installer and produce only an initial post-scan report. Child/bootstrap activity may outlive the observed process, so the UI explicitly offers later rescanning and never claims success from exit status.

## 2026-07-13 - Growth navigation requires a structured unique application identity

- Decision: carry the attributed software name as a separate target field and resolve it against the current inventory using one exact case-insensitive match. Never parse beginner copy or choose among duplicate names.
- Rejected alternatives: infer identity from localized titles; use partial/fuzzy search as authority; open the first same-name tile; expose the raw growth path so the user can decide.
- Rationale: navigation should reduce beginner work without creating a false association that could later influence uninstall, cache, or migration previews.
- Consequences: shared/system growth has no application shortcut, and ambiguous/missing targets stop with a clear explanation. Navigation remains read-only and cannot invoke an operation pipeline.

## 2026-07-13 - Derived growth state is reapplied at every application inventory boundary

- Decision: store the latest growth findings separately and route every `_softwareProfiles` replacement through one setter that rebuilds `RecentGrowthBytes` before refreshing the catalog or Agent.
- Rejected alternatives: enrich only after a C-drive scan; mutate profile objects in place; let individual scan/uninstall handlers remember to preserve the derived value.
- Rationale: inventory refreshes replace immutable profiles. Without a single boundary, an unrelated application rescan makes a valid growth warning disappear.
- Consequences: fresh scans clear stale/ambiguous growth honestly and preserve current unique evidence consistently across the app grid, drawer, Agent, uninstall, and residue-review flows.

## 2026-07-13 - Application cache cleanup is a narrow user-data quarantine operation

- Decision: qualify only bounded, existing, cache-named directories below the current user's Local, Roaming, or LocalLow roots. Refuse system apps, running apps, reparse ancestors, overlapping paths, unknown folder names, and more than 32 candidates. Re-resolve the exact app and current cache ownership after confirmation.
- Rejected alternatives: treat every `CachePaths` value as executable evidence; clean while a process is running; allow broad AppData/data/install directories; follow links; truncate a large candidate set; permanently delete; let Agent confirmation substitute for the user's click.
- Rationale: the inventory uses useful heuristics, but heuristics are not mutation authority. A beginner-facing one-click action needs a much narrower second policy and current-state proof.
- Consequences: some real caches will remain preview-only. Accepted ones move only through a specialized handler, generic safety pipeline, quarantine manifest, timeline, and restore path.

## 2026-07-13 - Recovery manifest and timeline are part of the quarantine transaction

- Decision: write each quarantine manifest before moving its source. Keep all batch moves and the final timeline write inside one compensating block; on any failure, restore completed moves in reverse order and report incomplete rollback without paths.
- Rejected alternatives: write the manifest after moving; treat timeline failure as a successful cleanup; leave earlier batch items moved after a later failure; expose raw failed paths in beginner errors.
- Rationale: a moved item without recovery coordinates or a visible restore entry violates the product's central “can regret” promise.
- Consequences: quarantine may return failure even after temporary movement, but it attempts to restore original state. Any remaining moved item still has a prewritten manifest; runtime fault-injection remains required after the antivirus pause.

## 2026-07-13 - Unstructured startup evidence may hand off to Windows but cannot authorize direct disable

- Decision: when a non-system profile has ordinary startup entries, offer a confirmed open-only handoff to the fixed `ms-settings:startupapps` catalog entry. Keep service/task-only and system evidence explanation-only. Label the action `管理自启动`, not `关闭自启动`.
- Rejected alternatives: write Run keys from names alone; call service/task commands; send service/task evidence to the Startup apps page; open Task Manager or Registry Editor as a generic answer; claim opening Settings changed startup behavior.
- Rationale: the current profile retains only display names, not exact registry hive/key/value, startup approval state, service start mode, task XML/state, or rollback snapshots. Windows' own startup page is a safer user-controlled bridge for ordinary entries.
- Consequences: OMNIX helps the beginner reach the right supported page with an Agent explanation and confirmation, but the user still chooses the toggle. Future direct management requires structured identities, current-state proof, rollback, and separate high-risk gates.

## 2026-07-13 - Background component identity and observation are separate from rollback evidence

- Decision: identify a startup item, service, or scheduled task by component kind plus exact source locator and name; hash that canonical identity separately from observed configuration. Store activation/runtime observations and required rollback evidence, but hard-code current observations and profile snapshots as read-only, rollback-incomplete, and unable to create a change operation.
- Rejected alternatives: treat display names as identities; include command/path in identity so a configuration edit looks like a different component; infer a Run entry is enabled merely because its value exists; call an observation hash a backup; expose direct registry/service/task actions once structured names exist.
- Rationale: future disable/restore must re-find the same component even when configuration changed, while refusing stale evidence. A hash can detect change but cannot restore original registry bytes, StartupApproved state, service configuration/recovery/ACLs, or task XML/ACLs.
- Consequences: current scans become more auditable and useful for Agent explanations, yet all direct management remains blocked. A future privileged snapshot needs its own strict evidence format, authenticity/freshness checks, final confirmation, safety pipeline, and restore verification.

## 2026-07-13 - StartupApproved bytes are drift evidence, not an enablement API

- Decision: read the exact StartupApproved value paired with a Run item, retain only status/length/SHA-256, and always report effective activation as unknown. Use explicit registry views: HKCU64 Run to HKCU64 Run approval, HKLM64 Run to HKLM64 Run approval, and HKLM32 Run to HKLM64 Run32 approval.
- Rejected alternatives: decode the first byte using community conventions; treat a missing value as enabled; store raw payload bytes in `SoftwareProfile`; read 32-bit Run and approval from one implicit registry view; use the evidence to create a disable operation.
- Rationale: the binary format is not a stable public contract for OMNIX. It is useful for detecting that Windows' approval record appeared or changed, but not sufficient to tell a beginner the effective switch state or restore it safely.
- Consequences: Agent wording remains honest and Windows Settings remains the authoritative control surface. Future direct management still requires independently justified state semantics and complete rollback evidence.

## 2026-07-13 - Sustained growth requires repeated contiguous evidence

- Decision: label a location as `持续增长` only after at least three contiguous recent observations, two positive intervals, a two-thirds positive majority, and positive total growth. A newly added software watch point is a baseline; one positive delta is only `最近增长`.
- Rejected alternatives: call every newly discovered software path growth; call any two-snapshot increase sustained; infer a rate from non-contiguous observations; let Agent turn the finding directly into cleanup or migration.
- Rationale: beginners will treat the headline as a diagnosis. The evidence wording must distinguish current size, one change, and a repeated trend.
- Consequences: OMNIX may wait one more scan before escalating a real issue, but it avoids false certainty. Every growth conclusion remains read-only and path-free.

## 2026-07-13 - Snapshot history is bounded at both builder and storage boundaries

- Decision: cap each snapshot at 2,048 monitored locations, reject oversized/invalid payloads again in persistence, retain at most 90 snapshots per scan root, and load only the latest eight prior snapshots for the active trend decision.
- Rejected alternatives: store every scan forever; rely only on the current builder; silently truncate an oversized payload in SQLite; delete history globally instead of per scan root.
- Rationale: long-running maintenance software must not become a new disk-growth source, and persistence must not trust one producer forever.
- Consequences: history remains sufficient for short-term trend evidence while database growth is bounded. Old internal snapshot rows are removed transactionally with foreign-key cascade; user files are unaffected.

## 2026-07-13 - Home promotes only sustained, path-free growth findings

- Decision: put up to two sustained growth findings before ordinary recommendations on the home page. Keep first observations and single deltas on the C-drive page. Use a dedicated health-finding kind so Agent explains current relief and future prevention without pretending that migration is already appropriate.
- Rejected alternatives: show every delta on home; expose software paths; map sustained growth directly to `Migrate`; leave growth evidence buried only in the technical C-drive view.
- Rationale: the home page should tell a beginner what deserves attention without creating alarm or asking them to interpret paths.
- Consequences: repeated software growth becomes visible and actionable as a plan, but execution still requires later evidence, confirmation, and the local safety pipeline.

## 2026-07-13 - Local Agent conversation is evidence presentation, not consent

- Decision: classify a bounded set of beginner intents locally, build replies only from current health/application summaries, hard-code `CanExecuteDirectly=false` and `UsedCloudAi=false`, and let replies open only allowlisted internal pages. Free text never creates or confirms an operation.
- Rejected alternatives: send every question to cloud AI; echo the raw question as context; infer missing scan facts; treat phrases such as “帮我卸载” as execution approval; let the WPF handler construct an operation or launch a process.
- Rationale: natural language is useful for explanation and navigation but is ambiguous as mutation authority. The product promise requires a separate evidence/plan/confirmation/rollback path for every real change.
- Consequences: V1 answers a known set of questions predictably and can honestly say it lacks evidence. Broader language coverage may be added later, but it must still emit an auditable recommendation and cannot bypass local gates.

## 2026-07-13 - Application mentions resolve exactly or refuse

- Decision: match only a full current `SoftwareProfile.Name` mention with exactly one case-insensitive inventory record. Use the structured target only for internal drawer navigation; duplicate, multi-match, stale, and missing targets refuse. Beginner-visible names and evidence fall back when they resemble absolute paths.
- Rejected alternatives: fuzzy matching; selecting the first partial result; parsing identity from localized answer text; displaying raw install/data paths in the response; retaining a stale profile object as authority.
- Rationale: opening the wrong app drawer can lead a beginner toward the wrong uninstall, migration, or cache plan. Navigation needs the same fail-closed identity discipline as later operations.
- Consequences: some natural questions require the user to select an app in the grid, but the Agent will not confidently choose the wrong item or disclose a private path.

## 2026-07-14 - Migration engine reasons stay technical; beginner projections are Chinese

- Decision: keep `MigrationPlanner`'s internal score/reason contract unchanged and translate each `MigrationRiskBand` into a stable Chinese drawer projection with explicit destination, snapshot, rollback, verification, and no-move wording.
- Rejected alternatives: expose enum names and English engine reasons; rewrite the planner while changing copy; hide the destination; imply the drawer can migrate immediately.
- Rationale: a presentation defect should not churn a safety engine, and beginners need the decision and next proof rather than internal terminology.
- Consequences: App drawer and exact-app Agent replies are readable now, while technical plan behavior and execution gates remain unchanged. New risk bands require a new explicit Chinese mapping rather than falling through to English.

## 2026-07-14 - Application icons are local evidence with a deterministic fallback

- Decision: accept only bounded absolute drive-letter `DisplayIcon` references with an allowlisted extension and optional integer resource index. At display time require a fixed drive, existing non-reparse path, bounded raster, frozen image, bounded cache, and unconditional native-handle cleanup. Any failure shows the existing letter tile.
- Rejected alternatives: load arbitrary URIs/UNC paths; ask the Shell to resolve links or web icons; execute the icon target; enumerate install directories heuristically; hide the tile when decoding fails; expose icon paths in UI/accessibility text.
- Rationale: recognizable icons make the app center usable for beginners, but registry icon strings are untrusted input and visual polish cannot create network, process, or native-resource risk.
- Consequences: some apps with unusual icon formats or redirected paths keep the letter fallback. Supported local EXE/DLL/ICO/raster icons become visible without changing software identity or operation authority.

## 2026-07-14 - Home Agent responses have one typed next step

- Decision: project every home health response to either C-drive evidence or application management through a closed enum. When an exact app name exists, re-resolve it against the current inventory immediately before opening the drawer; otherwise navigate to the C-drive page. An unavailable app target falls back to the unfiltered app grid.
- Rejected alternatives: leave the plan as text; store an arbitrary page string in the response; select the first fuzzy app match; create an operation from the home response; add several competing action buttons.
- Rationale: the beginner needs one obvious continuation, while stale natural-language context cannot become software identity or execution consent.
- Consequences: explain/detail/plan no longer strand the user. The home panel remains navigation-only, and actual cleanup, migration, uninstall, or startup changes still require their separate evidence and confirmation pipelines.

## 2026-07-14 - Drawer action GUI proof uses a capability fixture

- Decision: verify uninstall, migration, cache, and startup preview cards with one isolated software profile that explicitly supplies each required evidence type. Keep real-machine software scanning as a separate read-only icon/inventory acceptance.
- Rejected alternatives: require the developer PC to have at least one eligible app for every action; loosen startup eligibility so service/task-only apps pass; skip missing preview states; combine real icon proof with safety-action proof.
- Rationale: a safety smoke must be deterministic. Installed software is external state, and no eligible ordinary startup entry is a valid machine state rather than a product defect.
- Consequences: all four drawer outcomes can be reproduced without registry/service/task dependency or real mutation. Real icon rendering still needs its own scan/screenshot evidence.

## 2026-07-14 - Drawer actions use stable kinds, not display labels

- Decision: add `AppActionKind` to every drawer action and let WPF bind button availability/tooltips by this enum. Keep `Label` purely beginner-facing.
- Rejected alternatives: fix only the current Chinese string; compare localized labels case-insensitively; derive action identity from button content.
- Rationale: display copy is expected to evolve, while control behavior must remain stable. A translated or clarified label cannot be allowed to disable a safety workflow.
- Consequences: all current labels and eligibility rules stay unchanged, but future copy edits no longer alter WPF action lookup. New action kinds require an explicit presenter assignment and WPF mapping.

## 2026-07-14 - Installer launch is available through typed readiness, not a permanent bool

- Decision: remove the temporary antivirus-era `false` gate now that corrected definitions, compilation, focused tests, full regression, and antivirus observation have succeeded. Enable only through `InstallerLaunchReadinessPolicy`, retain an environment emergency stop, and require per-package preparation readiness before the button becomes available.
- Rejected alternatives: flip the bool to `true`; keep the feature permanently disabled; let an environment variable enable an otherwise unsafe path; interpret antivirus acceptance as package trust.
- Rationale: product availability and package authorization are different facts. The runtime policy can truthfully expose the feature, while every selected package still needs its own stable identity, trusted Windows signature, snapshot, four-part consent, revalidation, and bounded interactive launch.
- Consequences: beginners can complete the install-routing workflow in normal Windows builds. Operators can stop launching without disabling read-only advice. No silent installation, automatic click, forced elevation, or success claim is introduced.

## 2026-07-14 - Installer packages must remain on fixed local non-reparse paths

- Decision: accept launch evidence only for fully qualified package files on a ready fixed drive with no reparse point in the file or ancestor chain. Recheck the same rule during inspection, descriptor parsing, and the final launcher boundary.
- Rejected alternatives: accept any drive-letter path; allow mapped drives after hashing; follow junctions/symlinks; rely on the file picker; check only once during initial analysis.
- Rationale: mapped or redirected storage weakens local file-lock and identity assumptions. A high-risk launch should fail closed when the path can change storage semantics between evidence capture and process start.
- Consequences: some redirected Downloads folders or network-mapped installers must be copied to an ordinary local folder first. The UI remains beginner-facing and does not expose the technical path rule unless evidence is refused.

## 2026-07-14 - Migration development builds do not bypass package signing

- Decision: keep the existing same-trusted-signer app/Worker requirement as the production migration gate. Treat current unsigned-build refusal as expected release readiness, not as a reason to add a development execution flag.
- Rejected alternatives: launch the elevated Worker from an unsigned debug build; trust a path or hash without package identity; run migration in the WPF process; call the migration UI a preview-only dead end.
- Rationale: real migration already crosses an authenticated request, Worker trust, snapshot, rollback, and final-consent boundary. Weakening signer identity merely to demonstrate movement would undermine the central “不能瞎弄” promise.
- Consequences: migration plans, evidence, and consent can be tested with fixtures now; real production movement waits for a signed release package and explicit acceptance on disposable test software.

## 2026-07-14 - Personal files stay diagnosis-only in V1

- Decision: large personal files and same-name/exact-size duplicate candidates are never converted into cleanup operations.
- Rejected: hashing every candidate during the normal C-drive scan or automatically moving suspected duplicates into quarantine.
- Consequence: normal scans remain bounded and do not read large file contents; the user must open and verify personal files before manual archival or deletion outside this workflow.

## 2026-07-14 - Quarantine retention is never automatic

- Decision: expiry or capacity pressure may produce a permanent-cleanup plan, but it must never schedule or execute automatically.
- Rejected: silently deleting the oldest quarantine items at startup or immediately when the 20-GB recommendation is exceeded.
- Consequence: a dedicated irreversible operation needs explicit final consent, root and manifest revalidation, safety-pipeline routing, and a not-restorable timeline entry.

## 2026-07-14 - Migration evidence readiness and user consent are separate gates

- Decision: let `MigrationExecutionGate` evaluate only machine-verifiable plan eligibility, snapshot/rollback evidence, and destination capacity. Collect plan review, app-close, rollback, and monitoring acknowledgements once in the dedicated final-consent window before composing an elevated request.
- Rejected alternatives: mark acknowledgements true when evidence is written; add a second set of plan-page checkboxes; keep the request button permanently unreachable; weaken signed app/Worker trust for development builds.
- Rationale: evidence creation is not consent, and duplicated hidden confirmations created a dead end. One visible final-consent screen is clearer for beginners and remains fail-closed because the composer validates all four fresh acknowledgements.
- Consequences: evidence-complete plans can reach final consent, but cannot execute without fresh consent, a valid authenticated request, and trusted signed package identity. Unsigned development builds still refuse before UAC.
# 2026-07-14 - Migration closure observation stays separate from mutation authority

- Decision: expose `IMigrationPathObserver` to the desktop app and keep move/redirect/rollback methods only on `IMigrationPathAdapter` in the elevated Worker path.
- Rejected: reuse `WindowsDirectoryMigrationPathAdapter` in WPF for convenience. That would give the beginner UI process unnecessary mutation methods.
- Consequence: explicit scans can report redirect health without being able to repair or move anything.

# 2026-07-14 - Closure records are hints for fresh planning, never execution evidence

- Decision: old closure records may open a path-free `复查迁移` plan, but execution still requires current inventory, a fresh snapshot, rollback evidence, final consent, and the signed Worker boundary.
- Rejected: automatically repair a changed/missing redirect or reuse the old monitoring path as a new migration source.
- Consequence: a broken closure is visible and has a next step, while stale/tampered records cannot authorize filesystem mutation.

# 2026-07-14 - Name-only monitoring identity is presentation-only

- Decision: app navigation is allowed only when current inventory contains one exact case-insensitive name match; duplicate names remain unlinked.
- Rejected: pick the first matching application or infer identity from a private path.
- Consequence: some old records may require a fresh inventory scan, but OMNIX does not act on ambiguous software identity.
# 2026-07-14 - Whole-PC health uses explicit absence states instead of estimates

- Decision: distinguish `Available`, `NotPresent`, and `Unavailable` for D drive, memory, and battery; require a ready local fixed D drive; and retain only a process count.
- Rejected: probe network/removable drives, store process names, infer battery health from charge percentage, or replace missing facts with typical values.
- Consequence: the table can say `未配置`, `不适用`, or `未检测` truthfully without creating privacy or latency surprises.

# 2026-07-14 - One-time machine samples do not alter the existing score

- Decision: keep the current disk-pressure score unchanged and label it `当前按磁盘空间`; use memory, battery, startup, and trend as separate dimensions.
- Rejected: invent an undocumented weighted score from one-time memory/battery samples.
- Consequence: users gain broader evidence without a misleading precision claim; a future score model needs its own documented decision and tests.

# 2026-07-14 - Usage trend needs three real manual snapshots

- Decision: report `历史不足` until at least three distinct manual scan snapshots exist; do not claim background monitoring.
- Rejected: call a single before/after comparison a usage habit or synthesize a seven-day history.
- Consequence: trend advice arrives later but reflects actual repeated observations.

# 2026-07-14 - Installation footprint observation is fixed-root and shallow

- Decision: observe only immediate non-reparse children of Program Files, Program Files (x86), ProgramData, LocalAppData, and RoamingAppData on local C, with a hard 4096-entry bound.
- Rejected: recursive C-drive crawling, file-content hashing, USN/driver interception, network/removable roots, or treating all changed paths as installer-owned.
- Consequence: V1 can catch newly created top-level landing points missed by uninstall inventory without turning install reporting into a slow or invasive disk scanner. Writes inside an already-existing top-level directory may still require software-profile or later growth evidence.

# 2026-07-14 - Incomplete observation cannot support an absence claim or concrete plan

- Decision: bind `Complete`/`Truncated`/`Unavailable` into before evidence and every report layer. Compare footprint paths only when both captures are complete; otherwise show known inventory evidence, require a new observation, and refuse non-observation candidate previews.
- Rejected: merge partial path sets, call zero candidates `未发现`, or let an incomplete snapshot produce cache/migration/startup handling authority.
- Consequence: some users will be asked to rescan, but OMNIX will not convert missing access or a capacity limit into false reassurance.
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

## 2026-07-22 - Signing readiness is read-only discovery, never signer selection

- Decision: report tool availability and every eligible CurrentUser code-signing certificate, but require a separate explicit thumbprint when invoking the signing transform.
- Rejected: auto-select the only certificate, generate/import a development certificate, install SDK components, search drives recursively, or call the signing transform from the inspector.
- Consequence: missing prerequisites are machine-readable without turning local certificate presence into user authorization or weakening the same-signer release gate.

## 2026-07-22 - Code-signing EKU is parsed from the X509 extension

- Decision: inspect OID `2.5.29.37` as `X509EnhancedKeyUsageExtension` and require code-signing OID `1.3.6.1.5.5.7.3.3` in both prerequisite and signing scripts.
- Rejected: depend on provider-specific `EnhancedKeyUsageList`, accept certificates with no EKU, or let the inspector and signer apply different eligibility rules.
- Consequence: Windows PowerShell 5.1 reports ordinary certificates as ineligible rather than treating the certificate store as unreadable, and the final transform rechecks the identical security condition.
