# Reflections

## 2026-07-22 - GitHub personal release foundation

- What worked: separating Windows reputation warnings from OMNIX's internal privileged-worker trust made it possible to support a free personal signing path without weakening system-modification safety.
- What caught real risk: the public candidate audit found both machine QA evidence and an unanchored ignore rule that would have omitted quarantine source code. Treating first publication as a release gate prevented a broken repository.
- Waste/error: replacing a username with `ExampleUser` changed Agent classification semantics, parallel test commands contended on one output, and Release-mode full tests invoked a Debug-only fake worker. Each now has an error-ledger prevention rule.
- Product lesson: a convenient updater is still the wrong updater when it silently installs into C. Installation location is part of the user promise, not a packaging detail.
- Remaining risk: the new dialog has static/UIAutomation hooks and valid XAML, but real visual evidence is Warn because the local app launch timed out. Automatic installation remains intentionally absent.

## 2026-07-22 - Non-default SDK discovery

- A blocker report is still code output and must be challenged by independent machine evidence. The app inventory clue removed a false prerequisite without weakening safety.
- Registry discovery can be safer than filesystem discovery when the key is installer-owned, exact, and used only to bound subsequent reads.
- Precise blockers help the user: the machine needs a publisher identity and disposable environment, not another SDK installation.

## 2026-07-22 - V1 completion audit and recent-install sort

- A requirement matrix found a real omission that broad regression tests could not reveal because no test preserved the original “recently installed” wording.
- Unknown metadata is a product state, not an error to hide. Unknown-last ordering gives a useful list without inventing history from mutable file timestamps.
- Runtime evidence needs its own status. A failed launcher observation does not invalidate source/tests, but source/tests cannot be promoted into a screenshot or signed behavior receipt.

## 2026-07-22 - Beginner-safe signing preparation

- Documentation can expose a security gap that source-only review misses; writing the operator path forced the cryptographic compatibility boundary to become executable policy.
- A release guide should describe only routes the repository can verify end to end. Naming unsupported alternatives is useful only when the boundary is explicit.
- The honest stopping point is external identity and disposable evidence, not a fabricated local substitute. Readiness inspection makes that boundary concrete without weakening it.

## 2026-07-19 - Release-candidate transfer verification

- Creation-time evidence is not transfer-time evidence. A candidate should prove itself again in the environment where behavioral acceptance will run.
- Rejecting unlisted files matters as much as hashing listed files; otherwise a clean manifest can coexist with an unexpected executable payload.
- Keeping verification read-only prevents a package-integrity pass from silently becoming application launch or acceptance consent.

## 2026-07-19 - Trusted signed-release transformation

- Code signing is release evidence, not permission to skip behavioral acceptance. Recording `Eligible` and `Awaiting` separately keeps the product's safety story honest.
- Transforming an immutable verified package is easier to audit than mixing publish, certificate access, signing, and trust changes in one script.
- A release script must distrust its own inputs: manifest coverage, hashes, reparse paths, command surface, certificate purpose, timestamp, and post-signature identity all need independent checks.

## 2026-07-19 - Execution result return handoff

- Result acknowledgement and current-state synchronization are one user workflow even when they live in different windows. Requiring a second close made correct backend refresh logic feel disconnected.
- Navigation copy must describe the actual host, not just the view model. The same presentation window can be truthful in one host and misleading in another.
- Closing the exhausted plan does not add execution authority; it only reaches the already-existing read-only rescan sooner. Keeping that distinction explicit made the change small and reviewable.

## 2026-07-19 - Cache and startup decision outcomes

- Runtime acceptance can legitimately end with no code change. The useful outcome was proving that two ambiguous evidence cases refuse local mutation and still give a concrete beginner next step.
- A fallback action is different from an execution action. Opening Windows Startup Apps is acceptable only because the UI explicitly says OMNIX will not toggle anything and the handoff was not invoked during acceptance.
- Antivirus-definition updates reduce false-positive friction, but trusted same-signer packaging remains the independent authority for positive mutation acceptance.

Use this file at the end of meaningful tasks. Keep entries short and focused on reusable learning.

### 2026-07-16 - Local mutation post-attempt synchronization

- A failed result and a thrown call are not evidence that nothing changed. The moment a trusted handler is invoked is the right boundary for current-state observation.
- Recovery surfaces need stronger freshness than ordinary pages. A handler can record partial rollback evidence correctly while stale UI still hides it from the beginner.
- Refreshing only the journal is insufficient for C-drive cleanup; the old recommendation remains executable until disk evidence is rebuilt and selection revoked.
- Shared read-only helpers make the safety claim inspectable: observation dependencies are separate from every mutation handler and pipeline.

### 2026-07-16 - Persistent later install observation

- A warning about child installers needs a later action that survives the modal result window. Otherwise truthful copy still leaves a beginner stranded.
- Session-only evidence is the conservative first boundary: it solves delayed observation without storing personal paths or inviting stale cross-restart comparisons.
- Dependency shape matters for safety. A read-only method on a launcher-capable object can be behaviorally safe today, but a dedicated read-only coordinator makes future misuse harder.
- Source contracts can lie by slicing too far. Method extraction boundaries deserve the same maintenance attention as production call boundaries.

### 2026-07-16 - Beginner-safe installer post-scan recovery

- A recovery instruction is incomplete when its only matching control lives in an advanced diagnostic section. The Agent's next step and the ordinary UI must meet in the same flow.
- Preserving the original before snapshot is what makes a later read meaningful; replacing it would erase the very changes the user wants explained.
- A user-driven loop can remain conservative when every iteration requires a new click and performs observation only. Automatic retries would blur whether the installer had actually finished.
- Build success and AutomationIds reduce visual risk but do not replace a real screenshot. A timed-out Windows launch remains Warn, even after antivirus definitions are updated.

### 2026-07-16 - Post-install inventory reuse

- A single observation should feed every view that claims to describe the same moment. Re-scanning would create avoidable disagreement between the install report and Applications.
- An installer process ending is not installation success. Publishing catalog state only from the completed after-snapshot/report pair preserves that distinction.
- Read-only state synchronization can improve workflow closure without adding any installer control or system mutation authority.

### 2026-07-16 - Migration post-attempt state synchronization

- Migration has two observable truths after an attempt: where the application now appears and whether the old source stays closed. Refreshing only one can still mislead a beginner.
- Success copy should be downstream of fresh observation and authenticated operation completion, not the condition that decides whether observation happens.
- Unknown-result handling should reduce action: observe, explain, and stop. It must never silently retry a file/link mutation.

### 2026-07-16 - Official uninstall post-attempt inventory refresh

- A failed or missing response does not prove the system stayed unchanged. Once a trusted worker may have crossed the mutation boundary, current state must be observed again.
- Read-only recovery work can be broader than destructive follow-up work. Refreshing inventory after every attempt is safe; residue cleanup must still require stronger, specific evidence.
- Transport completion, operation success, post-scan success, and residue eligibility are separate facts. Keeping separate gates prevents one optimistic boolean from authorizing too much.

### 2026-07-16 - Startup restore pipeline

- A strong low-level exact-write adapter is necessary but not sufficient. The UI still needs current journal authority, evidence binding, consent, and one orchestration boundary before it may call that adapter.
- Recovery evidence may remain useful long after the original ten-minute disable window. Restore should verify immutable identity and current destination safety, not reject an old but intact rollback manifest by age alone.
- Removing the convenience restore API is stronger than relying on reviewers to remember not to use it. The safe path is now the easiest available production path.
- A failed registry restore is not equivalent to no change because verification can fail after a write. Conservative timeline state protects beginners from confident retries.

### 2026-07-16 - Ordinary quarantine restore pipeline

- A rollback button is still a system mutation. Its comforting label cannot substitute for current evidence, explicit consent, identity binding, and a single execution authority.
- The timeline page is a view over persisted state, not persisted state itself. Passing only the row id back into preparation keeps cached UI data outside the trust boundary.
- Binding the manifest alone is insufficient because the quarantined payload can change independently. Rechecking both immediately before movement closes the useful time-of-check/time-of-use gap.
- Related static source tests must be searched whenever a workflow method is split. A focused behavioral suite can be green while an obsolete structural contract still fails later.

### 2026-07-16 - Count entities before explaining overlapping signals

- What worked: a resident-app ownership total plus separate signal-type counts explains one app with multiple background mechanisms without inflating the apparent app count.
- Waste avoided: four ownership breakdowns would make the compact summary unreadable; one raw total would hide why the Agent refuses protected actions.
- Remaining risk: the next useful work is no longer another wording audit. Critical cleanup execution, timeline, and restore boundaries need an end-to-end readiness trace.

### 2026-07-16 - A diagnostic total needs an ownership legend

- What worked: preserving one deduplicated total while attaching mutually exclusive ownership counts kept the filter truthful and made the action boundary understandable.
- Waste avoided: hiding system evidence would weaken diagnosis; adding another dashboard panel would increase the clutter the user explicitly rejected.
- Remaining risk: application-page background totals still combine ordinary, system, and ownership-pending running/startup/service/task evidence.

### 2026-07-16 - An action verb is not a safety rating

- What worked: one action-plus-risk predicate made homepage totals, saved digests, Agent answers, and individual finding copy agree.
- Waste avoided: hiding medium/high findings would remove useful diagnosis; counting them as low-risk would turn a workflow label into a false safety promise.
- Remaining risk: C-drive application counts in stored summaries still need the same ordinary/system/ownership-pending separation already used by startup and aggregate Agent guidance.

### 2026-07-16 - Aggregate wording is part of the authorization surface

- What worked: a shared typed catalog made counts, names, exact answers, skill panels, and drawer availability agree without duplicating Windows-path rules in Agent code.
- Waste avoided: filtering only explicit `SystemTool` profiles would still leave managed-root unknown profiles actionable; filtering all C-drive evidence would hide useful D-data and system diagnostics.
- Remaining risk: health summaries still call every `Clean` finding low-risk and classify startup ownership with category alone.

### 2026-07-16 - Historical evidence needs a typed authorization projection

- What worked: resolving each closure to reviewable, protected historical, or unavailable made action, wording, target, and navigation change together instead of relying on a nullable name.
- Waste avoided: hiding unmatched/system history would discard useful diagnosis, while routing every record to the C-drive page would create a false next step.
- Remaining risk: aggregate Agent summaries still count all C-drive footprints as ordinary migration candidates and need the same current-profile availability distinction.

### 2026-07-15 - Visual urgency must follow action authority

- What worked: tying warning color, sort priority, and aggregate counts to the same review eligibility prevents the UI from urging beginners toward an action the safety policy denies.
- Waste avoided: hiding protected historical records entirely would lose useful diagnostics; the separate `仅供查看` count preserves evidence without creating urgency.
- Remaining risk: fresh real-WPF visual proof is pending and should be retried after the reported antivirus database update.

### 2026-07-15 - Disabled controls are not policy boundaries

- What worked: projecting the existing drawer action into one entry decision avoided duplicating category/path rules across three WPF methods.
- Waste avoided: guarding after plan construction would still read unnecessary protected-profile evidence and could leave pending state; source-order tests keep refusal first.
- Remaining risk: historical migration evidence still overrides protected-profile tile status and catalog priority even though it no longer grants action authority.

### 2026-07-15 - Historical evidence cannot grant current authority

- What worked: modeling the combined closure state in Core made the intended priority testable: current ownership protection first, historical warning second.
- Waste avoided: simply adding `&& button.IsEnabled` would have blocked ordinary D-installed closure review, because its base migration action is intentionally disabled after moving to D.
- Remaining risk: other action methods still rely too heavily on UI/upstream routing for entry protection and need their own policy guards.

### 2026-07-15 - Adjacent actions need separate authority models

- What worked: separating “launch the official uninstaller” from “review an uninstall that happened elsewhere” preserved a useful beginner workflow without weakening system ownership protection.
- Waste avoided: reusing `CanReviewUninstall` would have hidden legitimate external-uninstall recovery; enabling from selection presence would have reopened protected profiles after async work.
- Remaining risk: migration closure presentation has a similar post-policy override and must be audited next.

### 2026-07-15 - Empty UI state is also capability revocation

- Related task: prevent an empty app catalog from retaining the prior drawer's conclusions and pending targets.
- What worked: routing every empty path through one typed reset made visible text, selection, preview, technical details, and hidden target state change together.
- Waste avoided: no separate mutation cancellation protocol was invented; the existing collapsed action host already owns pending-field invalidation.
- Remaining risk: the separately wired residue-review button still bypasses the system/ownership action availability model on normal drawer open.
- Reusable lesson: when context disappears, clear-state handling must revoke capabilities and evidence, not merely render placeholders.

### 2026-07-15 - Overlapping evidence needs separate facts and one deduplicated total

- Related task: align the app-page C-drive summary with the C-drive filter.
- What worked: separate main-program and data/cache counts explain why an app is present, while one shared per-app predicate prevents double counting.
- Waste avoided: the UI no longer infers C-drive ownership from a non-empty list or labels all C clues as cleanable.
- Remaining risk: empty catalog paths can retain the previously selected app's category summary and drawer state.
- Reusable lesson: when evidence categories overlap, present each category independently and derive totals from entity membership rather than adding category counts.

### 2026-07-15 - A filter label is an availability promise

- Related task: align the `可卸载` catalog with system and unknown ownership denies already enforced by the drawer.
- What worked: one shared review-availability policy removed drift, and the behavior-first test exposed a second dormant contradiction in preview copy.
- Waste avoided: no execution gate or command-trust logic was duplicated into the catalog.
- Remaining risk: the C-drive summary count and C-drive filter use different footprint predicates and can disagree for manually/incompletely populated profiles.
- Reusable lesson: when a catalog groups items by an action, membership must use the same deny-first policy as the action surface, while deeper execution readiness remains separate.

### 2026-07-15 - Truthful grouping is better than speculative precision

- Related task: correct the app catalog's `办公学习` group, which actually contained every Normal fallback.
- What worked: renaming the concept end to end retained behavior while removing a false claim in the first visible application view.
- Waste avoided: no new keyword taxonomy or migration was invented for a wording defect.
- Remaining risk: the `可卸载` filter still equates command presence with reviewed uninstall availability.
- Reusable lesson: when evidence supports only a broad fallback, use a broad honest label before attempting a finer taxonomy.

### 2026-07-15 - A useful label needs provenance and calibrated wording

- Related task: explain why the scanner labels an application as AI, development, system, normal, or unknown.
- What worked: preserving source-specific evidence let the drawer say exactly which kind of clue was used without exposing a path or dumping technical fields.
- Waste avoided: weak install-path inference no longer sounds as certain as a product-name match, and Normal fallback no longer masquerades as a discovered fact.
- Remaining risk: the current `OfficeStudy` filter still treats every Normal fallback as office/study, which overstates what the scanner knows.
- Reusable lesson: classification output should carry provenance and confidence, and presentation should weaken its wording before policy ever considers the label.

### 2026-07-15 - A deny can be high confidence while classification stays uncertain

- Related task: protect unknown profiles in Windows-managed locations without falsely labeling Microsoft-published normal apps as system components.
- What worked: canonical managed-root containment supplied enough evidence to deny ordinary actions, while keeping the category and explanation explicitly unknown.
- Waste avoided: incomplete profiles no longer inherit dangerous buttons, and publisher-name heuristics do not block ordinary D-installed utilities.
- Remaining risk: the scanner model still lacks category evidence/confidence, so the UI cannot explain why a concrete category was chosen.
- Reusable lesson: safety denial and semantic classification need different evidence thresholds; denying an action does not justify asserting an identity.

### 2026-07-15 - Capability availability needs a category deny before evidence allows

- Related task: prevent system applications from inheriting ordinary app actions merely because actionable fields exist.
- What worked: one category-first recommendation and action-set branch removed contradictions while preserving technical inspection and all ordinary app behavior.
- Waste avoided: the Agent no longer recommends migration beside a disabled migration button, and system uninstall/cache/startup controls cannot appear enabled by accident.
- Remaining risk: unknown-category applications in system-like locations still need a separate conservative classification audit.
- Reusable lesson: action evidence can allow a workflow only after category/trust denies have run; a discovered command or path is never sufficient authority by itself.

### 2026-07-15 - Zero-valued fields need evidence context

- Related task: show install, data, cache, and recent growth in the application drawer.
- What worked: combining numeric values with whether data/cache locations were identified produced useful unavailable states without widening the model.
- Waste avoided: the UI no longer hides cache, prints misleading zero bytes, or describes cache size as guaranteed cleanup benefit.
- Remaining risk: the current profile model has no explicit measurement-availability flag, so recent zero growth still requires the separate typed growth observation for a real conclusion.
- Reusable lesson: a default numeric zero is often serialization state, not product evidence; presentation must consult provenance before calling it a measurement.

### 2026-07-15 - A status color still needs a reason

- Related task: make application-grid attention states understandable before opening a drawer.
- What worked: retaining the existing status while replacing one generic tag with three evidence-specific labels kept policy and UX changes separate.
- Waste avoided: beginners no longer need to open every red-dot app to learn whether the main program or only its data touches C.
- Remaining risk: the red Attention color itself was not re-evaluated, and the text change has no fresh real-WPF screenshot.
- Reusable lesson: compact status surfaces should state the observed reason; color alone is neither an explanation nor a decision.

### 2026-07-15 - A time-window change is not installer ownership

- Related task: explain where a newly installed program landed while also reporting C-drive changes seen during installation.
- What worked: separating unique-profile evidence from footprint-only candidates made both ownership and uncertainty visible without raw paths.
- Waste avoided: the Agent no longer treats a D-installed program with C-drive data as a failed main-program routing decision or blames unrelated concurrent changes on it.
- Remaining risk: path-based profile ownership is still evidence, not proof of why a program writes there; the changed report has no fresh real-WPF screenshot.
- Reusable lesson: installation placement, attributed mutable data, and concurrent system deltas require separate confidence levels even when observed in one before/after window.

### 2026-07-15 - Installation location and data location are separate facts

- Related task: explain a D-installed application that still writes to C without recommending a meaningless repeat migration.
- What worked: deriving one deduplicated, install-root-aware aggregate count let every beginner-facing conclusion agree while keeping raw paths in technical details.
- Waste avoided: the UI no longer calls the location simply reasonable, calls every write cache, or enables an unsupported data-redirection operation.
- Remaining risk: a C-drive write clue still does not prove why the application writes there, and the revised copy has no fresh real-WPF screenshot.
- Reusable lesson: main binary placement, mutable data placement, and growth trend are three different evidence questions; product advice must not collapse them into one migration button.

### 2026-07-15 - A write location is not a growth trend

- Related task: explain why one exact application is growing or still writing to C without guessing a cause.
- What worked: a typed comparison state plus on-demand baseline, current-profile re-resolution, and aggregate counts made positive, zero, insufficient, and unavailable evidence independently testable.
- Waste avoided: the Agent now separates immediate relief from prevention instead of turning every growth concern into a cleanup or migration suggestion.
- Remaining risk: the first scan intentionally cannot answer whether growth is sustained, and the interaction still lacks fresh WPF visual proof.
- Reusable lesson: trend advice needs an explicit comparison state; paths show possible ownership, while deltas show change, and neither alone proves root cause or consent to act.

### 2026-07-15 - A useful Agent prepares the next review, not the operation

- Related task: connect exact named-app action questions to existing uninstall, migration, cache, and startup review surfaces.
- What worked: a small typed handoff plus reuse of drawer availability and shared preview methods removed one beginner decision without granting the Agent operation authority.
- Waste avoided: users no longer repeat the action they already stated, while location/troubleshooting questions and unavailable/system actions avoid irrelevant plan windows.
- Remaining risk: the interaction has automated source coverage but no fresh WPF screenshot because real launch remains unavailable through Computer Use.
- Reusable lesson: natural language may safely choose a read-only review only after current identity and existing capability are revalidated; confirmation and execution must remain separate lower-layer decisions.

### 2026-07-15 - Operation failure copy needs an execution phase

- Related task: remove ten raw operation, policy, and validation errors from beginner-visible WPF controls.
- What worked: separating safety refusal before execution from unconfirmed state after a pipeline attempt produced short truthful guidance while leaving lower-layer diagnostics intact.
- Waste avoided: users no longer need to interpret policy vocabulary or machine-specific error details, and the UI does not falsely claim that a partially attempted operation changed nothing.
- Remaining risk: updated antivirus definitions did not make Computer Use launch observable; fresh UI and antivirus proof remains Warn despite full automated verification.
- Reusable lesson: translate failures at the presentation boundary from verified phase and recovery authority, never from an arbitrary raw error string.

### 2026-07-15 - Failure copy must describe certainty, not the exception

- Related task: remove raw exception messages from six beginner-visible WPF failure paths.
- What worked: classifying failures by whether modification was impossible or potentially partial produced more honest copy than a generic sanitizer.
- Waste avoided: local paths and system-provider text no longer reach the UI, while users still learn exactly where to reload or rescan.
- Remaining risk: typed operation/policy error strings are still directly shown in ten branches and need their own pre-execution/partial-state classification.
- Reusable lesson: at UI boundaries, report the verified operation state and recovery step; exception text is neither a safe diagnosis nor a beginner decision aid.

### 2026-07-15 - Aggregate runtime evidence should stay coarse

- Related task: answer exact app freeze/resource questions from a real short Windows sample.
- What worked: exact profile-derived process names, an injected aggregate reader, a strict duration/count cap, and a real current-process test kept the observation both useful and reviewable.
- Waste avoided: the Agent no longer sends a beginner to Task Manager just to learn whether the app is currently active or roughly resource-heavy.
- Remaining risk: a 350 ms sample is intentionally not a trend or diagnosis, and fresh WPF visual proof remains under the existing launch Warn.
- Reusable lesson: operational telemetry for beginner decisions should cross boundaries as coarse aggregates with explicit time scope; preserve Unknown and never attach control authority to observation.

### 2026-07-15 - Reduce private diagnostics before they cross a boundary

- Related task: let an exact app crash/freeze question use real Windows Application-log evidence without exposing event content.
- What worked: a fixed log/window/count/provider/id boundary plus injected reader tests made privacy, availability states, and non-execution authority directly reviewable.
- Waste avoided: the Agent no longer asks the beginner to open Event Viewer just to know whether matching records exist, and generic or operation questions do not pay the log-query cost.
- Remaining risk: the event correlation cannot prove cause, and real WPF launch still lacks fresh visual evidence after the antivirus update.
- Reusable lesson: reduce sensitive OS diagnostics to the smallest decision fact inside the platform adapter; keep raw values out of shared models and preserve Unknown separately from no finding.

### 2026-07-15 - Preserve the user's object before routing the tool

- Related task: make `微信闪退` about 微信 first instead of immediately opening generic Event Viewer guidance.
- What worked: a conservative likely-named-subject predicate allowed one inventory hydration, then exact current-profile resolution kept the answer grounded and private.
- Waste avoided: generic `软件闪退` and system blue-screen questions do not scan the app inventory, while exact app problems no longer lose their target context.
- Remaining risk: no Windows crash records are observed yet, so the Agent correctly stops at symptom guidance.
- Reusable lesson: fault routing should preserve the object the user named, present current evidence first, then offer technical tools as an explicit secondary step rather than replacing the object with the tool.

### 2026-07-15 - Separate diagnostic intent from evidence size

- Related task: make natural whole-computer questions run a real full diagnosis without making every performance question expensive.
- What worked: an explicit intent plus bounded phrase list made the full, lightweight, C-drive, hardware, and app evidence scopes independently testable.
- Waste avoided: a whole-computer request no longer performs a software-only scan first, and a card/hardware question does not trigger the disk scanner.
- Remaining risk: language coverage is intentionally conservative; new phrases should be added through boundary tests, not broad `状态` or `优化` substrings.
- Reusable lesson: route by the user's requested decision first, then select the smallest sufficient evidence source; never infer broad consent or broad scope from generic optimization words.

### 2026-07-15 - Conversation is not evidence consent

- Related task: prevent greetings and capability questions from scanning the installed-app inventory.
- What worked: exact whole-question matching avoided the privacy/performance cost without weakening unknown-app discovery.
- Waste avoided: simple conversation no longer waits for a machine scan or presents irrelevant `暂无结果` evidence.
- Remaining risk: unclassified natural language still needs evidence intent improvements rather than ever-broader keyword exclusions.
- Reusable lesson: evidence loading should follow a concrete diagnostic intent; harmless conversation must remain cheap, and no-scan matchers should be closed and exact so mixed real questions are not suppressed.

### 2026-07-15 - A diagnostic action should prepare its own evidence

- Related task: remove the manual homepage scan prerequisite from the System Diagnosis Agent skill.
- What worked: one pure category/evidence policy reused the proven shared scan gate and made all seven non-diagnostic categories easy to exclude in tests.
- Waste avoided: the user no longer clicks a skill only to be sent to another page and button; concurrent Home/C-drive/skill requests still perform one scan.
- Remaining risk: a full disk diagnosis is intentionally expensive, so only the explicit System Diagnosis category may request it.
- Reusable lesson: when the user explicitly asks for a diagnosis, the product should prepare read-only evidence itself; scope the evidence policy narrowly and keep findings separate from permission to act.

### 2026-07-15 - Match observation cost to the question

- Related task: let Computer Agent answer hardware/configuration and machine-health questions without requiring a full C-drive scan.
- What worked: passing a purpose-specific observation beside the disk health summary kept CPU/memory/battery answers automatic while preventing an invented overall score.
- Waste avoided: the Agent and full diagnosis now share one in-flight observation instead of probing the machine twice.
- Remaining risk: post-antivirus visual launch still times out in Computer Use, so the first-visible answer has automated but not fresh screenshot proof.
- Reusable lesson: collect the smallest read-only evidence set that answers the user's question, preserve explicit unknown states, and never reuse a broader score outside the evidence that defines it.

### 2026-07-15 - Hide data-loading prerequisites from beginners

- Related task: make application management useful without asking the user to understand `扫描应用`.
- What worked: a small task-sharing gate made concurrency, empty success, retry, and forced refresh directly testable while leaving the scanner unchanged.
- Waste avoided: first entry, C-drive handoff, and growth-to-app navigation now share inventory work instead of starting separate scans.
- Remaining risk: the real grid/loading screenshot is still blocked by Computer Use launch failure.
- Reusable lesson: beginner pages should lazily prepare read-only evidence on first use; keep refresh explicit, preserve the last trusted result on refresh failure, and never present internal evidence collection as a user decision.

### 2026-07-15 - Fail early on package trust, revalidate late

- Related task: make signed uninstall and migration execution truthful in unsigned development builds.
- What worked: treating trust readiness as a beginner-facing prerequisite exposed the real dead end without adding a second mutation path. A dedicated WPF test caught the desired fail-closed constructor default.
- Waste avoided: users no longer need to create snapshots or complete final confirmations before learning that the current build is preview-only.
- Remaining risk: a real screenshot is still missing because Computer Use could not launch the WPF app; signed-package acceptance also requires a controlled release certificate and disposable fixtures.
- Reusable lesson: for privileged desktop workflows, explain trust readiness before costly preparation, then independently revalidate at action time.

### 2026-07-07 - Migration evidence wording

- Related task: Make future migration execution harder to request accidentally and easier to audit.
- What worked: A small product test caught that "viewed the plan" was too weak for a high-risk operation.
- What failed or slowed down: No GUI verification was run because this was a narrow checklist text/model change.
- Root lessons: High-risk preflight UI should name the exact evidence and confirmation scope; generic done labels are not enough.
- Token or time waste: Minimal.
- Process improvements: Keep adding focused tests for safety copy before any execution handler exists.
- Future optimizations: Add an explicit plan-confirmation UI control only after the surrounding migration workflow is ready to represent final confirmation safely.
- Potential skill candidates: none.

### 2026-07-07 - Residue review inline short-circuit

- Related task: Make `卸载后检查残留` safe and understandable before any real residue movement exists.
- What worked: The failed/slow GUI attempt exposed a product problem, not just a test problem: "software still installed" should be answered immediately from known evidence.
- What failed or slowed down: Real-machine app scanning is too slow for repeated GUI smoke loops and can obscure whether a click actually helped the user.
- Root lessons: Safety refusals should be fast, local, and inline. Expensive rescans belong after cached evidence cannot answer the question.
- Token or time waste: Time was spent waiting for a full GUI scan that was not needed for the common blocked case.
- Process improvements: Add fake/cached-inventory GUI paths or narrower UI hooks before relying on real-machine full scans.
- Future optimizations: Convert more no-op safety decisions into inline Agent responses with explicit "why not" and "next safe step" text.
- Potential skill candidates: none.

### 2026-07-07 - C-drive page automatic target and hidden technical report

- Related task: Make the C-drive page answer beginner questions without looking like a path editor or raw report viewer.
- What worked: The real-scan screenshot made the issue obvious: the old `C:\` selector looked like manual input and the technical report stole first-screen attention.
- What failed or slowed down: The first GUIA verification failed because the UIAutomation condition helper still used fragile `New-Object` construction.
- Root lessons: Beginner UI should show "what this is" before "where this lives"; raw evidence belongs behind an explicit detail action.
- Token or time waste: One GUI rerun was needed after the UIA condition-construction error.
- Process improvements: Use direct .NET constructors for UIA conditions and continue pairing product tests with real screenshots.
- Future optimizations: Group repeated C-drive recommendation cards and add a short inline "why quarantine" explanation near the execute button.
- Potential skill candidates: WPF UIA helper for safe condition construction and app-window screenshot capture.

### 2026-07-07 - Inline homepage Agent response

- Related task: Make OMNIX-Entropy feel like a calm Computer Agent instead of a pile of popups.
- What worked: GUI verification did more than prove click handlers existed; the screenshot showed modal stacking and then verified the in-page replacement.
- What failed or slowed down: The first inline panel placement was below a tall list and was effectively hidden in the viewport.
- Root lessons: For beginner UX, "the button works" is not enough; the result must appear where the user is already looking.
- Token or time waste: Some time went to rerunning GUI screenshots because another application initially obscured the window.
- Process improvements: For WPF UI changes, capture the specific app window bounds and put the target window foreground/topmost before visual QA.
- Future optimizations: Convert the remaining low-risk explanatory MessageBoxes into inline panels or drawers where appropriate.
- Potential skill candidates: WPF foreground app-window screenshot helper.

### 2026-07-07 - Homepage key finding Agent buttons

- Related task: Continue making OMNIX-Entropy feel like a beginner-friendly Computer Agent rather than a static report.
- What worked: Treating button behavior as presentation models made the safety boundary testable: explain, detail, and plan buttons can respond without executing anything.
- What failed or slowed down: GUI click-through remains unavailable in this environment because WPF launch approval had previously hit usage limits.
- Root lessons: For this product, every visible button should either explain, navigate, generate a plan, or clearly say why it cannot act; silent buttons break trust quickly.
- Token or time waste: Low; the first red tests captured the missing builders cleanly.
- Process improvements: New UI buttons should be created with `Tag`/handler wiring and a core presentation test in the same slice.
- Future optimizations: Replace modal MessageBoxes with a persistent Agent side panel that keeps context and avoids interrupting scanning.
- Potential skill candidates: none.

### 2026-07-07 - C-drive beginner summaries

- Related task: Continue OMNIX-Entropy V1 by making C-drive results understandable to non-technical users.
- What worked: Presenter-level TDD made the UX rule concrete: primary cards must hide raw `C:\` paths and explain what the source means before showing technical details.
- What failed or slowed down: Post-change GUI verification could not run because the approval system rejected the escalated WPF launch due usage limits.
- Root lessons: A safe PC manager needs two layers for every finding: a plain-language decision layer for the user, and a secondary technical layer for audit/debug.
- Token or time waste: Some time went to working around mojibake-affected patch context, especially around the old growth list assignment.
- Process improvements: Continue anchoring localized WPF/code-behind edits on ASCII control names and method names.
- Future optimizations: Move C-drive page state out of `MainWindow.xaml.cs` so old bindings and presenter transitions are easier to clean up safely.
- Potential skill candidates: none.

## Entry Template

### YYYY-MM-DD - Task or milestone

- Related task:
- What worked:
- What failed or slowed down:
- Root lessons:
- Token or time waste:
- Process improvements:
- Future optimizations:
- Potential skill candidates:

## Entries

### 2026-07-07 - C-drive recommendation card presentation

- Related task: Make C-drive recommendation cards easier for beginner users to understand.
- What worked: Extracting the card presenter into `Css.Core.Apps` made the UX language testable without launching WPF.
- What failed or slowed down: Removing the old unused code-behind class failed because the surrounding mojibake strings could not be patched safely.
- Root lessons: Improve the active path first; defer risky cleanup of historical encoding-damaged blocks unless there is a stable ASCII boundary.
- Token or time waste: Low; one failed patch attempt, no source damage.
- Process improvements: For UI text refactors, create new presenter/view-model classes with ASCII-safe strings, switch bindings, then delete old code only in an encoding-safe cleanup pass.
- Future optimizations: Run a visual C-drive scan page smoke to check card wrapping and button spacing.
- Potential skill candidates: none.

### 2026-07-07 - Uninstall safety copy localization

- Related task: Make the uninstall cleaner preview understandable without enabling real uninstall execution.
- What worked: Two focused red-green loops caught both the modal copy and the app-drawer preview leak; the GUI smoke then validated the real WPF window instead of relying only on unit tests.
- What failed or slowed down: The GUIA harness had three avoidable issues: bad `PropertyCondition` binding, too-narrow modal lookup, and attempting to invoke a disabled modal button.
- Root lessons: For safety UI, text verification should be scoped to the exact modal and should not click arbitrary buttons just to close the window.
- Token or time waste: Moderate; several GUIA retries were needed, but they produced a better reusable pattern.
- Process improvements: Prefer `AutomationId` for main-window controls and modal `AutomationProperties.Name` for dialog lookup in WPF smoke scripts.
- Future optimizations: Extract uninstall/migration visible copy into a resource-backed presentation catalog after UX stabilizes.
- Potential skill candidates: WPF modal text smoke script with AutomationId navigation.

### 2026-07-07 - Migration plan body localization

- Related task: Make the migration safety window readable for beginner users while preserving preview-only behavior.
- What worked: The focused product test forced the user-visible copy to move from English control phrases to Chinese safety language before implementation.
- What failed or slowed down: The first GUIA text check searched root descendants and produced a false positive from outside the migration modal.
- Root lessons: Modal UI text QA must scope assertions to the modal window, not the process root.
- Token or time waste: Moderate; one extra GUIA run was needed to diagnose the false positive.
- Process improvements: Use stable child control -> parent window scoping for future WPF modal checks.
- Future optimizations: Extract localized migration/uninstall copy into resources or a presentation text catalog.
- Potential skill candidates: WPF modal text smoke should scope to the modal window.

### 2026-07-01 - App drawer top-summary localization

- Related task: Make the app drawer's first visible conclusions readable for non-technical users.
- What worked: The focused test captured the exact UX rule: visible summaries should be Chinese and should not leak old English control phrases.
- What failed or slowed down: The first test run hit a FluentAssertions overload mistake, and GUIA verification was later blocked by usage limits.
- Root lessons: A TDD red is only useful when it fails for the missing behavior; compile-error reds need to be repaired immediately.
- Token or time waste: Low; the compile error was fixed in one edit.
- Process improvements: For localized UI work, pair product tests with a static old-phrase search when GUI automation is unavailable.
- Future optimizations: Centralize drawer copy in a localization/view-text catalog to avoid scattering Unicode escapes.
- Potential skill candidates: none.

### 2026-07-01 - App drawer action localization

- Related task: Continue app-management UI polish for beginner-readable actions.
- What worked: A focused red/green product test made the desired action labels explicit before WPF binding changed.
- What failed or slowed down: One `rg` command used brittle PowerShell quoting and failed before search ran.
- Root lessons: For this app, Chinese labels are product behavior, not decoration; tests should guard them like any other public contract.
- Token or time waste: Low; the failed search was immediately corrected.
- Process improvements: Keep localized C# strings as Unicode escapes and XAML static text as numeric character references until a resource system exists.
- Future optimizations: Move action labels/reasons into a centralized localization/resource table after the UI shape stabilizes.
- Potential skill candidates: none.

### 2026-07-01 - App tile Chinese status labels

- Related task: Make app management understandable for beginner users.
- What worked: Unicode escape literals let the UI show Chinese labels while avoiding another localized-source encoding incident.
- What failed or slowed down: The repository still has mojibake in older UI files, so localized work needs narrow tests and stable anchors.
- Root lessons: Small visible words like status tags matter because users scan the grid before reading any details.
- Token or time waste: Low.
- Process improvements: Keep localization changes tiny, test visible runtime strings, then verify with UIAutomation.
- Future optimizations: Move user-facing strings into a resource layer after the V1 interaction model stabilizes.
- Potential skill candidates: none.

### 2026-07-01 - App tile accessibility names

- Related task: Continue app-management polish for beginner-friendly OMNIX-Entropy.
- What worked: A small core property plus WPF binding made UIAutomation names meaningful without touching risky system behavior.
- What failed or slowed down: The first focused test filter did not match any test, and the screenshot captured the wrong foreground app.
- Root lessons: Verification needs both a matching test filter and the right evidence type; UIAutomation output can prove accessibility even when a screenshot is visually stale.
- Token or time waste: Low to moderate.
- Process improvements: After any filtered test says "no tests matched", rerun with the exact method name or broader class filter before making claims.
- Future optimizations: Localize `ShortTag` values so accessibility names are fully Chinese.
- Potential skill candidates: none.

### 2026-07-01 - Migration rollback manifest UI action

- Related task: Continue OMNIX-Entropy V1 toward safe app migration.
- What worked: The UI action stays evidence-only; TDD covered no source movement and readiness refresh before WPF wiring.
- What failed or slowed down: GUI verification found an unrelated but real placeholder-search bug, and the first UIAutomation scripts were too narrow.
- Root lessons: Real GUI smoke tests are valuable because product-visible bugs can sit outside unit-tested core logic.
- Token or time waste: Moderate; debugging UIAutomation took extra time but produced screenshots and a better test.
- Process improvements: When a GUI test says a window is missing, capture first and inspect before changing product code.
- Future optimizations: Give app tiles meaningful automation names and localize the migration window text.
- Potential skill candidates: none.

### 2026-07-01 - Migration rollback manifest and space probe

- Related task: Continue safe migration development for OMNIX-Entropy V1.
- What worked: Writing tests around "no source movement" kept the manifest helper safely evidence-only.
- What failed or slowed down: A wording mismatch in the space-probe summary failed one test; the clearer phrase is better for users too.
- Root lessons: Rollback evidence and destination capacity should exist before migration execution, not be discovered during execution.
- Token or time waste: Low.
- Process improvements: Future migration work should update readiness through evidence artifacts instead of adding ad hoc UI booleans.
- Future optimizations: Add a user-confirmed UI flow to write the rollback manifest and populate `RollbackManifestPath`.
- Potential skill candidates: none.

### 2026-07-01 - Migration readiness gate

- Related task: Continue safe migration development for OMNIX-Entropy V1.
- What worked: Mirroring the official uninstall gate made migration readiness explicit and testable.
- What failed or slowed down: No GUI click-through was run, so readability of the checklist is still an open UI task.
- Root lessons: A migration "plan" and a migration "execution request" are different products; the latter needs snapshot, rollback, free-space, close-app, and monitoring proof.
- Token or time waste: Low; focused tests made the API shape clear.
- Process improvements: Keep future mutation flows split into presentation, readiness gate, operation descriptor, and handler.
- Future optimizations: Share destination routing and affected-path estimation across drawer preview, plan window, and execution gate.
- Potential skill candidates: none.

### 2026-07-01 - Migration plan window

- Related task: Continue OMNIX-Entropy V1 toward safe app migration.
- What worked: The plan page turns drawer advice into a fuller user decision surface without introducing a mover.
- What failed or slowed down: Existing UI localization remains mixed; new window uses ASCII English to avoid expanding current encoding issues.
- Root lessons: Migration needs a full readiness gate before execution, because "move files" alone does not stop future C-drive writes.
- Token or time waste: Low; TDD caught the public model shape before WPF binding.
- Process improvements: Share destination-route logic between drawer preview and plan presentation before adding more migration features.
- Future optimizations: Add a migration readiness model with snapshot id, app-close confirmation, free-space check, rollback manifest, and monitoring confirmation.
- Potential skill candidates: none.

### 2026-06-30 - App drawer migration preview

- Related task: Continue OMNIX-Entropy V1 toward intuitive app management and migration guidance.
- What worked: Keeping migration as drawer-level advice made the feature useful without creating a risky file-moving path.
- What failed or slowed down: A PowerShell scripted edit damaged test-file encoding and forced a full UTF-8 rewrite.
- Root lessons: Preview-first is the right product stance for migration; encoding-sensitive files need explicit edit discipline.
- Token or time waste: Moderate; recovery and verification took extra time but left a cleaner ASCII test file.
- Process improvements: Use `apply_patch` and stable ASCII anchors for localized UI/test files; record any fallback full rewrite immediately.
- Future optimizations: Build a dedicated migration-plan page that can show snapshot, rollback, app-close checks, redirect strategy, and post-migration monitoring before any execution.
- Potential skill candidates: Preserve source encoding during scripted edits.

### 2026-06-30 - Official uninstall preflight checklist

- Related task: Make the clean-uninstall path understandable before real execution exists.
- What worked: Building the checklist from the existing execution gate kept the user-facing model aligned with the safety model.
- What failed or slowed down: Existing localized strings are mojibake in source/terminal views, so patches had to anchor on ASCII method names and line numbers.
- Root lessons: Beginner-facing safety gates should have two layers: a stable machine gate and a readable checklist generated from that same gate.
- Token or time waste: Low.
- Process improvements: Keep step keys ASCII and stable; localize display text in one later pass rather than mixing more corrupted strings.
- Future optimizations: Extract shared uninstall command parsing and add an interactive readiness view model with checkbox state updates.
- Potential skill candidates: none.

### 2026-06-30 - Publisher signature trust for external uninstallers

- Related task: Add richer command trust without opening official uninstaller execution.
- What worked: Reusing `SoftwareProfile.Publisher` and `SignatureSubject` avoided adding scanner dependencies to the core trust evaluator.
- What failed or slowed down: Optional parameters are starting to stretch the evaluator signature.
- Root lessons: Trust evidence should eventually be modeled as a named evidence object so the UI can show which evidence source made the command acceptable.
- Token or time waste: Low.
- Process improvements: Keep adding one evaluator test and one execution-gate test for every new trust source.
- Future optimizations: Replace optional evaluator parameters with `OfficialUninstallCommandTrustEvidence` and display evidence source labels in the confirmation window.
- Potential skill candidates: none.

### 2026-06-30 - Safe MSI official uninstall trust

- Related task: Continue the safe uninstall loop without enabling real process execution.
- What worked: Adding gate-level tests caught the important integration point: the evaluator needs uninstall arguments, not just the executable path.
- What failed or slowed down: The first production patch missed file context and had to be split into smaller patches.
- Root lessons: Trust decisions for uninstallers must consider executable, arguments, UI mode, and readiness as separate evidence; executable name alone is too weak.
- Token or time waste: Low.
- Process improvements: For command-trust work, write one evaluator test and one gate integration test before implementation.
- Future optimizations: Extract a shared uninstall command parser and add signer/publisher matching.
- Potential skill candidates: none.

### 2026-06-30 - Official uninstall command trust

- Related task: Add command trust before official uninstaller execution can ever be enabled.
- What worked: A small enum-based trust result kept tests away from brittle UI strings and made the gate decision explicit.
- What failed or slowed down: Localized source still appears as mojibake through some `Get-Content` reads; using `Select-String` and ASCII anchors avoided further patch mistakes.
- Root lessons: Execution safety should be layered: readiness state, command trust, final confirmation, and handler execution are separate concerns.
- Token or time waste: Low.
- Process improvements: Keep new safety models ASCII-friendly where possible, especially if protocol logs will be read by multiple tools.
- Future optimizations: Share uninstall command parsing and add Authenticode signer matching as the next trust layer.
- Potential skill candidates: none.

### 2026-06-30 - Official uninstaller execution gate

- Related task: Continue moving the uninstall flow toward safe real execution.
- What worked: Modeling readiness separately from execution made it possible to progress without adding a dangerous process launch path.
- What failed or slowed down: I repeated the known mistake of running `dotnet test` commands in parallel, causing an `obj` output lock.
- Root lessons: Any .NET command that writes shared `obj/bin` output must be run serially in this repository unless output paths are isolated.
- Token or time waste: Moderate; the lock required `dotnet build-server shutdown` and a rerun.
- Process improvements: Treat the existing error-ledger rule as active, not archival.
- Future optimizations: Share uninstall command parsing between confirmation display and execution gate.
- Potential skill candidates: none.

### 2026-06-30 - Uninstall residue review UI

- Related task: Continue development toward a safe uninstall-cleanup flow.
- What worked: Reusing the residue scan report and quarantine operation policy kept the UI button from becoming a special unsafe path.
- What failed or slowed down: The first XAML patch used mojibake text copied from `Get-Content` output and failed to match; switching to stable control names avoided touching unrelated localized strings.
- Root lessons: For localized WPF files, patch against `x:Name`, method names, and `Select-String` evidence rather than terminal-rendered Chinese text.
- Token or time waste: Low, but the failed patch was avoidable.
- Process improvements: Future UI work should add automation names and perform a GUI click-through when approval budget allows.
- Future optimizations: Replace MessageBox review with an in-app decision panel so beginner-friendly text, risk grouping, and undo state are easier to scan.
- Potential skill candidates: none.

### 2026-06-30 - V1 foundation implementation

- Related task: Implement Windows 全能电脑助手 V1 plan foundation.
- What worked: TDD quickly fixed the public API shape for safety gates, recommendations, growth, install routing, and migration planning.
- What failed or slowed down: WPF XAML patching needed whole-file replacement because the original file did not match the patch context cleanly.
- Root lessons: Keep mutation safety as a shared model first; feature modules can then plug into one pipeline instead of inventing local safeguards.
- Token or time waste: Some plan scope is too large for one implementation pass; splitting into core contracts first kept the result buildable.
- Process improvements: Next pass should add SQLite persistence and real UI binding with tests around data adapters before adding more module breadth.
- Future optimizations: Add visual QA once the WPF UI becomes interactive.
- Potential skill candidates: none.

### 2026-06-30 - Runnable scan UI

- Related task: Move toward a locally testable WPF app.
- What worked: Keeping scan session aggregation in `Css.Scanner` made the WPF layer thin and testable.
- What failed or slowed down: SQLite pooling kept temp db files locked during tests; disabling pooling fixed the issue.
- Root lessons: Local test stores should disable pooling or explicitly clear pools when tests create/delete database files.
- Token or time waste: None significant; direct WPF code-behind is faster for first runnable testing.
- Process improvements: Add manual UI screenshots after the first real C drive scan.
- Future optimizations: Move UI state into a ViewModel once scan UX stabilizes.
- Potential skill candidates: none.

### 2026-06-30 - Software inventory first pass

- Related task: Add first read-only software profile scan.
- What worked: Separating raw installed/startup/service records from `SoftwareInventoryBuilder` made tests deterministic while allowing a real Windows scanner.
- What failed or slowed down: Plan-task scanning is locale/API sensitive and was deferred to avoid brittle command parsing.
- Root lessons: Software ownership matching should begin conservative; false positives in migration/install decisions are worse than missing a weak association.
- Token or time waste: None significant.
- Process improvements: Add a small manual QA checklist for “扫描软件” before wiring migration recommendations.
- Future optimizations: Add Task Scheduler COM scanning and richer signature trust status.
- Potential skill candidates: none.

### 2026-06-30 - Installer analysis first pass

- Related task: Add first read-only installation control test surface.
- What worked: Treating installer control as analysis first avoided risky process execution while making the install-location policy testable.
- What failed or slowed down: Static installer kind detection is necessarily heuristic without parsing binaries or running installers.
- Root lessons: For a safety assistant, “recommend and explain” should land before “automate and enforce.”
- Token or time waste: None significant.
- Process improvements: Next install-control pass should compare system snapshots before/after a test install rather than attempting interception first.
- Future optimizations: Add installer metadata extraction and known setup framework fingerprints.
- Potential skill candidates: none.

### 2026-06-30 - Install snapshot diff and scheduled tasks

- Related task: Make installation control closer to a locally testable read-only loop.
- What worked: TDD shaped a small `InstallSystemSnapshot` / `InstallSnapshotDiffReport` API, then WPF could call it without adding unsafe installer execution.
- What failed or slowed down: Running `dotnet test` and `dotnet build` in parallel caused a VBCSCompiler file lock.
- Root lessons: Install control should first prove “what changed” before trying to enforce installation paths; .NET verification sharing obj/bin should be serialized.
- Token or time waste: Some time went to recovering from the parallel build lock.
- Process improvements: Keep future Windows scanner sources read-only and degradable; record exact skipped sources instead of failing entire scans.
- Future optimizations: Persist install snapshots and add a guided manual QA flow for test installs.
- Potential skill candidates: Serialize .NET verification sharing obj/bin.

### 2026-06-30 - Quarantine and timeline foundation

- Related task: Build the first safe mutation foundation for cleanup actions.
- What worked: TDD exposed the missing-handler and partial-move risks before wiring the handler into future UI paths.
- What failed or slowed down: WPF `ItemsSource` rejected collection expressions against non-generic `IEnumerable`.
- Root lessons: “后悔药” needs three pieces together: manifest, timeline, and pipeline-gated handler; any one alone is not enough for user trust.
- Token or time waste: Minimal; the build failure was fast and recorded.
- Process improvements: For any future multi-path mutation, add a preflight test that proves no partial action happens when one target is invalid.
- Future optimizations: Add manifest browsing, restore UI, retention policy, and elevated worker integration.
- Potential skill candidates: none.

### 2026-06-30 - Low-risk cleanup confirmation loop

- Related task: Make the first cleanup recommendation executable without violating safety boundaries.
- What worked: A tiny `QuarantineOperationPolicy` made the WPF button simple and kept the safety decision outside UI code.
- What failed or slowed down: No fresh WPF smoke launch was attempted this turn because the previous escalation path hit usage limits; build coverage still caught XAML/code-behind compile issues.
- Root lessons: The first executable path should be deliberately narrow; broad execution can follow after restore UI and elevated worker exist.
- Token or time waste: Minimal.
- Process improvements: Future UI execution features should have a core policy test before code-behind changes.
- Future optimizations: Replace MessageBox with an in-app confirmation panel that shows evidence, paths, rollback, and action timeline side-by-side.
- Potential skill candidates: none.

### 2026-06-30 - OMNIX-Entropy branding

- Related task: Rename user-visible software identity to `OMNIX-Entropy`.
- What worked: Centralizing product name and storage folder names in `AppIdentity` made WPF and path defaults easier to keep consistent.
- What failed or slowed down: Old names were spread across UI strings and local path helpers.
- Root lessons: Brand and storage identity should be constants, not scattered literals.
- Token or time waste: Minimal.
- Process improvements: Add a small branding test whenever app identity affects paths or UI.
- Future optimizations: Rename solution/project/package metadata before publishing an installer.
- Potential skill candidates: none.

### 2026-06-30 - Manual UI feedback pass

- Related task: Respond to first hands-on WPF feedback about unclear controls and broken-feeling navigation.
- What worked: Treating the screenshots as product evidence led to concrete fixes: no free-form C drive text box, clickable navigation, software-profile summary, simpler recommendation copy, and an explicit quarantine explanation.
- What failed or slowed down: The first screenshot capture accidentally recorded Chrome instead of the WPF window; a second launch-and-capture was required.
- Root lessons: For a beginner-facing system tool, a technically correct model is still a UX bug if the card text does not answer “what happened, can I touch it, can I undo it”.
- Token or time waste: Low; UI Automation gave faster evidence than manual clicking for the nav buttons.
- Process improvements: Future UI feedback passes should include one screenshot and one UI Automation smoke for named buttons before handoff.
- Future optimizations: Replace MessageBox confirmation with an in-app decision panel using the same beginner-friendly language.
- Potential skill candidates: none.

### 2026-06-30 - Intuitive manager refactor

- Related task: Implement the approved OMNIX-Entropy V1 product refactor.
- What worked: The TDD slice made the product language executable: health summary, app tile, app drawer, Agent skill catalog, and uninstall plan each got a test before UI binding.
- What failed or slowed down: The first UI Automation smoke script had a condition-construction bug and produced false missing-button results.
- Root lessons: For this product, UI verification must prove the user-visible page changed, not merely that a button handler ran.
- Token or time waste: Some time went to repairing code-behind references left over from the older single-scroll layout.
- Process improvements: Future WPF UI work should pair compile checks with one UIA click-to-title assertion for each navigation entry.
- Future optimizations: Move page state and app filters into view models once the current UX direction stabilizes.
- Potential skill candidates: UIA navigation smoke template for WPF apps.

### 2026-06-30 - App management loop

- Related task: Continue development by making app management filters, search, sorting, and uninstall preview usable.
- What worked: Keeping app catalog filtering in `Css.Core.Apps` made the behavior testable and kept WPF code focused on binding.
- What failed or slowed down: UI Automation could not find the search box and sort combo until explicit automation names were added.
- Root lessons: Beginner-friendly UI controls also need automation names; visual clarity and testability reinforce each other.
- Token or time waste: Minimal; one red-green cycle caught the desired public API shape before UI binding.
- Process improvements: Add `AutomationProperties.Name` while creating new WPF controls, not after UIA misses them.
- Future optimizations: Move app page state into a view model and add a richer software category model beyond `Normal/Game/Ai/DevelopmentTool/SystemTool`.
- Potential skill candidates: none.

### 2026-06-30 - Marvis real scan and uninstall plan window

- Related task: Continue toward full app-management safety loop.
- What worked: A real-machine opt-in xUnit test gave stronger evidence than ad-hoc PowerShell object loading and confirmed scanner behavior through the project runtime.
- What failed or slowed down: Direct PowerShell `Add-Type` loading of net8 assemblies failed due dependency loading; GUI smoke was later blocked by usage-limit approval rejection.
- Root lessons: Real Windows scanner features need both deterministic unit tests and explicit machine probes; WMI must not be the only service source.
- Token or time waste: Some time went to the failed `Add-Type` probe, but it led to a better test-host approach.
- Process improvements: Use opt-in real-machine tests for environment-specific validation, and keep them disabled by default.
- Future optimizations: Add a diagnostic command or internal test harness for read-only scan snapshots so GUI and scanner can be validated without temporary scripts.
- Potential skill candidates: Windows scanner source fallback checklist.

### 2026-06-30 - Timeline restore UI

- Related task: Make the 后悔药中心 actionable for quarantined cleanup records.
- What worked: Recording manifest paths in the timeline made restore deterministic and kept the UI simple: button state comes from `ActionTimelinePresenter`, execution from `FileQuarantineService`.
- What failed or slowed down: No GUI click-through was run because the previous approval path for launching WPF hit usage limits; verification stayed at unit/build level.
- Root lessons: A “restore” button must point to the exact rollback evidence, not infer from affected paths.
- Token or time waste: Low; the first red test captured the missing API shape cleanly.
- Process improvements: Future mutation flows should persist rollback references in the timeline at execution time, not add them later.
- Future optimizations: Add one reusable WPF smoke script that creates a temp quarantined item, launches the app, restores it, and verifies state.
- Potential skill candidates: rollback-reference persistence checklist.

### 2026-06-30 - Quarantine retention policy

- Related task: Prevent the quarantine/undo center from becoming another hidden disk-space problem.
- What worked: Keeping retention as a planner made it easy to show space pressure without adding a destructive delete path.
- What failed or slowed down: No GUI screenshot was available, so text fit and wrapping still need manual verification.
- Root lessons: “Can undo” has an operational cost; the product must surface that cost plainly and let the user decide when to permanently clear copies.
- Token or time waste: Minimal; tests clarified expired/over-capacity/already-restored cases.
- Process improvements: Any future cleanup of rollback assets should be modeled as a separate recommendation with its own confirmation.
- Future optimizations: Add a policy editor for retention days and max size after the default behavior is visually validated.
- Potential skill candidates: rollback asset retention checklist.

### 2026-06-30 - Uninstall residue scan

- Related task: Move “卸载干净点” closer to a Geek-like but safer workflow.
- What worked: Separating scan report from operation planning made the safety rule obvious: low-risk path residue can become a quarantine operation; service/task/startup residue stays explanation-only.
- What failed or slowed down: The first red test missed a `Css.Core.Quarantine` using, causing a namespace compile error before the intended missing-planner failure.
- Root lessons: For beginner-facing uninstall cleanup, “still installed” must block residue cleanup suggestions; otherwise the app could encourage deleting files for software that remains active.
- Token or time waste: Minimal.
- Process improvements: Future post-uninstall UI should always begin with a fresh inventory check before presenting residue actions.
- Future optimizations: Add registry residue candidates, but keep them high-risk until snapshot and rollback support exists.
- Potential skill candidates: safe uninstall residue classification checklist.

### 2026-06-30 - Official uninstall confirmation

- Related task: Make the uninstall flow understandable before any official uninstaller can run.
- What worked: A presentation-only confirmation model let the UI show command, args, running-process warnings, service/task warnings, snapshot requirement, and post-uninstall scan requirement without adding execution risk.
- What failed or slowed down: One test expected the exact human phrase “卸载后重新扫描”; the first checklist wording was technically fine but less direct.
- Root lessons: Confirmation pages for beginner tools should use the user's workflow language, not implementation language.
- Token or time waste: Low.
- Process improvements: Future execution gates should reuse the confirmation model and add only the missing permission/snapshot state.
- Future optimizations: Add command trust checks, signer display, and known uninstaller framework detection before enabling execution.
- Potential skill candidates: none.

### 2026-07-07 - C-drive recommendation grouping

- Related task: Make C-drive decision cards understandable for beginner users.
- What worked: Testing the list-level presentation first exposed the right boundary: group repeated observe findings without touching scanner evidence or executable cleanup operations.
- What failed or slowed down: The first GUI wait was too short for a real C-drive scan; the first visual pass also exposed a horizontal scrollbar and an enabled button under a non-executable card.
- Root lessons: For beginner UX, four similar "confirm source" cards should become one conclusion with examples and next-step guidance. Real screenshots are still necessary for WPF list wrapping and button-state issues.
- Token or time waste: Moderate; the extra GUI pass was worth it because it caught two confusing UI states before delivery.
- Process improvements: Add future recommendation grouping at the list-presenter layer unless execution semantics truly change.
- Future optimizations: Add a reusable UIAutomation helper for "scan, wait for text, screenshot, verify action button state" to avoid retyping long scripts.
- Potential skill candidates: none.

### 2026-07-07 - Residue review drawer result

- Related task: Make `卸载后检查残留` understandable and safe in the app drawer.
- What worked: Extracting a core drawer presenter made the still-installed state testable without a full GUI scan and removed path-heavy text from the beginner view.
- What failed or slowed down: First GUI pass found text via UIA but the user-visible drawer still showed migration preview; a second visual pass found a horizontal scrollbar in the residue list.
- Root lessons: UIA text presence is not enough for beginner UX. The action result must appear in the first relevant viewport and wrap without horizontal scrolling.
- Token or time waste: Moderate; repeated GUI scripts were necessary but should become a reusable smoke script.
- Process improvements: For each drawer action, test both state generation and XAML ordering relative to unrelated previews.
- Future optimizations: Create a small `scripts/gui-smoke-app-drawer.ps1` for scan/select/action/screenshot checks.
- Potential skill candidates: reusable WPF drawer action smoke script.
### 2026-07-07 - Shared uninstall workflow guide

- Related task: Make `卸载干净点` feel like one clear guided flow instead of separate drawer/window explanations.
- What worked: A single failing product test forced the right API shape: a shared presenter, drawer lines, safety-window workflow, and execution still disabled.
- What failed or slowed down: GUI automation `InvokePattern` did not open the drawer uninstall modal even though the button was enabled. The real-click fallback was blocked by usage-limit approval, so GUI modal proof remains pending.
- Root lessons: Beginner-facing action flows need one shared presentation source; WPF GUI smoke for drawer actions also needs a reliable real-click helper rather than relying only on `InvokePattern`.
- Token or time waste: Moderate; the failed GUI attempts produced useful diagnostics but should become a reusable helper.
- Process improvements: Add or reuse an app-drawer GUI smoke script that selects an actionable app, clicks by clickable point, verifies modal text, captures a screenshot, and always cleans up the process.
- Future optimizations: Use the same shared-flow pattern for future migration and cleanup confirmation pages so drawer previews and modal confirmations do not drift.
- Potential skill candidates: reusable WPF app-drawer click smoke script.
### 2026-07-07 - C-drive cleanup selection preview

- Related task: Make low-risk C-drive cleanup feel understandable before the user reaches any confirmation dialog.
- What worked: A small presenter gave one place to test "no selection", "observe only", and "low-risk cleanup can continue" states.
- What failed or slowed down: Historical mojibake in `MainWindow.xaml.cs` made deleting the old handler body risky, so an unused legacy method remains.
- Root lessons: Product language for risky actions should live in core presenters, not WPF event handlers. Code-behind cleanup should be deliberate and encoding-aware.
- Token or time waste: Low to moderate; the patch failure was quick, but it shows the cost of leaving mojibake-heavy UI logic in place.
- Process improvements: Schedule a code-behind cleanup pass after the next UX slice: extract more state helpers, then delete unused legacy handlers with full tests/build.
- Future optimizations: Add a static test or analyzer later to prevent event handlers from hardcoding long user-facing safety copy.
- Potential skill candidates: none.

### 2026-07-08 - Agent next-step panel

- Related task: Make the AI Agent page act like a local operations guide instead of only a static skill catalog.
- What worked: The failing product test defined the right user-facing contract: title, summary, reasons, safe actions, blocked actions, privacy line, and no direct execution.
- What failed or slowed down: A XAML patch failed when anchored on mojibake-rendered localized copy; using the stable `Computer Agent` line and numeric XML references avoided further encoding risk.
- Root lessons: Agent advice should be a tested presenter over local facts. The UI should bind that presenter and keep execution elsewhere behind the safety pipeline.
- Token or time waste: Low; the first focused test confirmed core logic after the presenter, then the remaining failure pointed directly to missing UI controls/hooks.
- Process improvements: Keep adding Agent affordances as small local presenters before any cloud AI integration.
- Future optimizations: Add a GUI smoke for "scan C drive, scan apps, open Agent, verify next-step title and safe/blocked actions".
- Potential skill candidates: local Agent advice presenter checklist.

### 2026-07-08 - Agent safe navigation actions

- Related task: Let the Agent guide beginners toward the relevant page without executing fixes.
- What worked: Modeling buttons as `AgentNextActionViewModel` kept labels, tooltips, and target pages testable in core code while WPF only performs internal navigation.
- What failed or slowed down: The old Agent identity copy could not be safely deleted due mojibake-heavy XAML context; keeping it is less ideal visually but safer than a risky rewrite inside this slice.
- Root lessons: Agent "do this next" affordances should start as navigation or explanation. Execution must be a separate, auditable operation model with evidence, confirmation, and rollback.
- Token or time waste: Low; the focused red test made the missing property obvious.
- Process improvements: Add a future XAML cleanup slice that replaces the whole Agent left-card region with stable XML-reference text and verifies visually.
- Future optimizations: Add UIAutomation coverage that clicks an Agent navigation button and asserts the page title changes without opening any modal or executing an operation.
- Potential skill candidates: none.

### 2026-07-08 - Agent left-card XAML cleanup

- Related task: Remove duplicate/legacy identity copy from the Agent page so the first read is clear.
- What worked: A static product test over the Agent left-card slice made the duplicate text easy to prove and remove.
- What failed or slowed down: Earlier attempts to delete mojibake-rendered text failed; after the file/test output showed valid UTF-8 Chinese, the focused deletion was straightforward.
- Root lessons: Encoding-sensitive UI cleanup should be protected by a slice-level static test before editing, especially when terminal output has previously displayed mojibake.
- Token or time waste: Low.
- Process improvements: For future XAML cleanup, test the exact region and unwanted legacy markers first, then patch the smallest stable block.
- Future optimizations: Add a visual Agent-page smoke when GUI approval is available.
- Potential skill candidates: none.
## 2026-07-08 - App drawer action preview panels

- What worked: Small presenter-first slices fit the user's feedback well. The app drawer now gives visible feedback for two buttons that previously felt inert, while preserving the no-blind-mutation safety boundary.
- What slowed down: Older mojibake UI text made long-context patches fail; stable control-name anchors worked better.
- Lesson: Drawer actions should first answer "what will happen if I continue?" in a collapsed, plain-language panel. Execution should remain a separate auditable plan/pipeline path.
- Future improvement: Create a reusable drawer action preview host so cache, startup, migration, and uninstall previews share one visual surface instead of accumulating separate panels in XAML.
- Skill candidate: stable-anchor patching for mojibake-heavy WPF files.

## 2026-07-08 - Real GUI smoke for app drawer previews

- What worked: Running the WPF GUI smoke after unit/build verification caught a real gap that tests with synthetic profiles missed: production scans did not provide enough cache evidence for the cache button to become useful.
- What failed or slowed down: The first smoke script was brittle because of raw Chinese strings and Windows PowerShell 5 API differences. Rebuilding it as ASCII-safe made the GUI path repeatable.
- Lesson: For user-facing "button works" claims, at least one real-machine UIAutomation pass is high value. Static XAML tests cannot prove real scanned data makes controls usable.
- Future improvement: Add more real-machine smoke scripts for Agent navigation, C-drive actionable card selection, and undo-center restore visibility.
- Skill candidate: ASCII-safe localized UIAutomation smoke scripts.

## 2026-07-08 - Nested browser/Electron cache attribution

- What worked: The red tests were small and concrete: one Chrome-like profile tree and one Electron-like `User Data` tree. That kept the implementation conservative.
- What slowed down: Existing direct AppData matching did not have a reusable helper for "data root plus nested subroots", so the scanner needed a small internal extraction before adding behavior.
- Lesson: AppData attribution should expand by exact, evidence-gated patterns first. Broad fuzzy matching is tempting but would undermine user trust if an app is blamed for another app's cache.
- Future improvement: If unusual browser profile names matter, add bounded directory enumeration with tests and a privacy/safety gate rather than guessing arbitrary folder names.
- Skill candidate: none.

## 2026-07-08 - App drawer preview state presenter

- What worked: A single state object made the cache/startup preview click behavior easy to test and kept WPF from owning safety copy.
- What slowed down: Existing static product tests were still tied to code-behind fields, so they needed a small correction after the presenter landed.
- Lesson: For visible safety behavior, test the presenter contract and the WPF wiring separately. Avoid requiring a specific code-behind implementation unless the implementation itself is the product rule.
- Future improvement: Add a presenter for no-selection drawer action statuses and a shared drawer preview host model for uninstall/cache/startup/migration.
- Skill candidate: none.

## 2026-07-08 - App drawer no-selection states

- What worked: Extending the same presenter kept selected and no-selection button behavior consistent.
- What slowed down: None significant; the red compile failure cleanly identified the missing methods.
- Lesson: Every user-click branch should have a tested presentation state, including "nothing selected" branches that are easy to leave as ad hoc status text.
- Future improvement: Use the same pattern for migration/uninstall no-selection branches and technical-detail toggling.
- Skill candidate: none.

## 2026-07-08 - App drawer technical details toggle

- What worked: A small presenter made the toggle behavior and button text testable without rewriting the drawer layout.
- What slowed down: The existing XAML button line could not be patched because of historical localized text corruption.
- Lesson: When a named control is not strictly needed, avoid risky XAML edits and use the sender plus tested presenter output.
- Future improvement: Replace the whole app-drawer XAML action area with clean XML-reference text during a dedicated visual cleanup pass.
- Skill candidate: none.

## 2026-07-08 - Shared app drawer action host

- What worked: A shared host directly addressed the user's "do not pile up text" feedback while keeping the implementation scoped and testable.
- What slowed down: Existing app-drawer XAML has historical localized text corruption, so fully deleting old sections in the same pass would have been risky.
- Lesson: For UI clutter reduction in an encoding-fragile file, first redirect active behavior to a clean named host, then remove old collapsed compatibility controls in a later visual cleanup pass.
- Future improvement: Replace the drawer action area as one clean XAML region with XML-reference Chinese text, then run a real GUI smoke.
- Skill candidate: none.

## 2026-07-08 - Drawer/C-drive cleanup plus Agent skill cards

- What worked: Static product tests were effective for removing stale WPF paths after the active behavior had already moved into presenters. The cleanup stayed low-risk and immediately caught old controls/handlers.
- What slowed down: Mojibake-heavy `MainWindow.xaml.cs` still made full-block patching fragile; smaller symbol-anchored patches worked.
- Root lessons: Once a shared presenter owns a behavior, remove old compatibility controls promptly so future UI work has one path. Agent capability lists need "what can I do next?" and "what will not happen?" fields, not only category descriptions.
- Token or time waste: Moderate; a failed full-block patch on `AgentSkillView` repeated a known encoding issue.
- Process improvements: Prefer presenter-level capability cards over WPF private formatting classes, and keep GUI claims separate from static/build verification when approval limits block WPF smoke.
- Future optimizations: Add a GUI smoke for the Agent skill card list and shared drawer host when WPF launch approval is available.
- Potential skill candidates: none.

## 2026-07-08 - Agent system tool shortcuts

- What worked: A fixed allowlist let the app borrow Marvis-style system-tool convenience without opening an arbitrary command surface.
- What slowed down: The first GUI smoke failed on PowerShell/UIAutomation constructor binding; explicit `-ArgumentList` fixed it.
- Root lessons: "Open tool" is safer than "perform fix", but still needs visible risk labels and confirmation for tools that can damage a system if misused.
- Token or time waste: Low; the GUI script failure was quickly diagnosed and the rerun produced real screenshot evidence.
- Process improvements: Keep adding Agent conveniences as explicit, allowlisted, user-clicked entries before considering any automation inside those tools.
- Future optimizations: Add a similar allowlisted Windows settings deep-link catalog for network, display, sound, power, and apps.
- Potential skill candidates: UIAutomation `PropertyCondition` construction checklist already recorded.

## 2026-07-08 - Windows settings confirmation gate

- What worked: The settings shortcut model could reuse the same risk/confirmation idea as system tools without introducing a generic launcher or any settings mutation path.
- What slowed down: Little; the failing test was a clean compile failure for the missing `RequiresConfirmation` property.
- Root lessons: "Open-only" still needs risk gradation. Pages that lead to uninstall, storage cleanup, or power behavior changes should remind the user before navigation, even when OMNIX-Entropy does not click inside those pages.
- Token or time waste: Low.
- Process improvements: Treat future Agent shortcuts as catalog-driven items with id, deep link/command, risk, confirmation policy, and safety copy.
- Future optimizations: Add a reusable UI smoke that can verify confirmation dialogs without accepting them, so medium-risk open buttons get real click proof without opening Settings.
- Potential skill candidates: already covered by the allowlisted Windows tool/deep-link catalog candidate.

## 2026-07-08 - Agent background priority

- What worked: A small presenter-level priority rule made the Agent feel more like an operations assistant without adding any risky execution path.
- What slowed down: Little; the red test clearly showed the previous C-drive-first behavior.
- Root lessons: The user's four core questions are peers, not a strict C-drive-only funnel. When several apps are resident, the Agent should surface "who is running or auto-starting?" as a first-class recommendation.
- Token or time waste: Low.
- Process improvements: Keep Agent prioritization in core presenters with thresholds and tests, then let WPF bind the result.
- Future optimizations: Add real-machine GUI proof after an app scan that shows the Agent panel switching to background priority when enough resident apps are detected.
- Potential skill candidates: none.

## 2026-07-08 - Agent background review panel

- What worked: A small core presenter gave the Agent an app-level resident summary without exposing raw service/task names or enabling any risky action.
- What failed or slowed down: The first GUI smoke script had a helper-order bug; the first placement put the new panel below the visible area even though UIAutomation could find it.
- Root lessons: For beginner-facing UI, "present in the tree" is not enough. The most important conclusion needs screenshot proof that it is actually visible in the first working area.
- Token or time waste: Moderate, because the first GUI smoke and first screenshot caught issues after implementation.
- Process improvements: Add explicit AutomationIds to new dynamic WPF panels and verify visible placement with screenshots, not only static XAML tests.
- Future optimizations: Create a reusable Agent-page GUI smoke helper that can verify a named panel after a real scan and assert it is visible within the app window bounds.
- Potential skill candidates: promote "first-screen visual proof for primary Agent conclusions" if another Agent panel has the same issue.

## 2026-07-08 - Agent startup/service plan preview

- What worked: The plan presenter kept risky background-management work in a safe shape: evidence, steps, required confirmation, rollback, and blocked actions without an execution path.
- What failed or slowed down: The first GUI screenshot showed the plan was technically visible but still too low; the first smoke assertion also failed because raw Chinese text matching was not encoding-safe in Windows PowerShell.
- Root lessons: Agent UX should put "what I recommend" before "all the detail I found." GUI smoke scripts for localized WPF should prefer AutomationIds and code-point-built strings over raw Chinese literals.
- Token or time waste: Moderate; the extra visual pass caught a real layout issue before handoff.
- Process improvements: Added a project UX rule to `AGENTS.md` requiring first-screen screenshot evidence for primary Agent conclusion panels.
- Future optimizations: Extract a reusable Agent-page smoke helper that can verify panel order, visible bounds, and screenshot capture after a real app scan.
- Potential skill candidates: promote the ASCII-safe localized UIAutomation smoke guidance if another script hits the same encoding problem.

## 2026-07-08 - Windows Settings confirmation cancel smoke

- What worked: The cancel smoke proved the safety gate at the real WPF interaction layer: the Storage confirmation dialog appeared, cancel was clicked, and no `SystemSettings` process launched.
- What failed or slowed down: The first implementation over-trusted nested scrollable lists and top-level-only dialog discovery. A generic XAML patch also landed in the wrong repeated `Border Grid.Column=1` region and broke XML until build caught it.
- Root lessons: Medium-risk UI needs to be visible before it can be safely tested. For WPF MessageBox automation, search all descendant windows for the launched process and use native handles to exclude the main window.
- Token or time waste: High for a smoke script, but it exposed real UX clutter and gave reusable rules for this app's WPF automation.
- Process improvements: Avoid running `dotnet build` and `dotnet test` in parallel for this solution. Anchor XAML edits on unique control names, not repeated layout shapes.
- Future optimizations: Extract a shared WPF smoke library for launch, navigate, find visible button, capture dialog, cancel, and assert no external process was launched.
- Potential skill candidates: WPF MessageBox cancel smoke template; no-parallel-dotnet rule for shared output directories.

## 2026-07-08 - App drawer shared action host GUI proof

- What worked: Stable AutomationIds plus a four-action smoke finally turned the user's "buttons should respond" concern into real click evidence for uninstall, migration, cache, and startup previews.
- What failed or slowed down: The first smoke update relied on a decorative `Border` host and assumed the first app enabled every action. Real UIAutomation and real app data disproved both assumptions.
- Root lessons: WPF GUI proof should target controls users can read or click, and conditional action tests should search for eligible data rather than overriding product state.
- Token or time waste: Moderate; two smoke failures were useful because they exposed verification assumptions rather than product execution bugs.
- Process improvements: Added a project rule for WPF AutomationIds on exposed controls and updated the smoke to close plan windows without accepting any action.
- Future optimizations: Extract reusable app-drawer smoke helpers for launch, scan, eligible-app selection, action invocation, modal close, screenshot capture, and process cleanup.
- Potential skill candidates: WPF smoke target exposed controls; conditional action smokes should search for eligible data.

## 2026-07-08 - App drawer Agent action cards

- What worked: The shared host made it cheap to improve every drawer action at once: uninstall, migration, cache, startup, and residue review all now speak in the same Agent/next-step/safety pattern.
- What failed or slowed down: The first screenshot showed the action card was generated but too low to be useful. I also launched GUI smoke before rebuilding the WPF exe once, which caused a stale-binary false failure.
- Root lessons: A clearer card is only useful if it scrolls into view after the click. For WPF changes, build before GUI smoke, and search for direct initializers when adding required view-model fields.
- Token or time waste: Moderate; the extra GUI/build loop was worthwhile because it caught real visibility and stale-binary issues before handoff.
- Process improvements: Action-click GUI smokes should assert the actual concise fields, not only that a title/list exists.
- Future optimizations: Extract an app-drawer smoke helper and consider replacing the remaining detail list with grouped rows if the card still feels dense in later screenshots.
- Potential skill candidates: build WPF before GUI smoke after XAML changes; required view-model initializer search.
## 2026-07-08 - Selected resident plan and undo-center proof reflection

- What worked: TDD made the selected-app plan gap explicit: old copy only said "found components", while the desired UX needed `建议保留 / 先观察 / 未来可禁用候选`. The app-drawer GUI smoke gave real visual confidence for the updated action card.
- What slowed down: The TimelinePage XAML had historical mojibake/malformed attributes, causing `apply_patch` context matching to fail and making the UI harder to reason about.
- Root lesson: Beginner-facing safety pages need both readable copy and reliable automation hooks; if localized XAML is corrupted, repair the small block with XML character references before adding more tests around it.
- Waste: The undo-center GUI smoke could not be run because escalation was rejected by usage limits; static/product/build evidence remains useful, but visual proof is still pending.
- Process improvement: Add shared smoke helpers and eventually an isolated app-data root so undo-center smokes can show restorable entries without touching the user's real timeline.
- Potential skill candidate: yes, for corrupted WPF/XAML repair and read-only GUI smoke design.

## 2026-07-09 - Isolated storage root reflection

- What worked: Adding the path resolver in Core made default/override behavior testable and avoided hard-coding `.omx` test paths in WPF.
- What slowed down: The repo is still uncommitted/untracked, so `git diff` is not a useful narrow change view; file-specific inspection and tests remain the best evidence.
- Root lesson: Safety-oriented GUI proof should separate "real app behavior" from "real user data"; process-scoped env vars are a clean way to get both.
- Waste: None significant; the existing undo-center smoke was small enough to adapt directly.
- Process improvement: Next time, seed restorable GUI data only after adding isolated roots, not before.
- Potential skill candidate: yes, for shared PowerShell smoke isolation helpers.

## 2026-07-09 - Seeded undo-center GUI proof reflection

- What worked: Reusing the real quarantine/timeline services through `Css.SmokeTools` gave a realistic restorable row without adding fake WPF behavior or touching user data.
- What slowed down: Adding a new project required restore; the first restore hit sandbox network restrictions. Visual review then caught a path-heavy UX issue that automation alone would have missed.
- Root lesson: For beginner-facing recovery UI, "the button exists" is not enough. Screenshot review should also check that the row can be understood without reading full local paths.
- Waste: Low to moderate; the restore retry cost time but left a cleaner dev-tool pattern for future GUI state seeding.
- Process improvement: Build the solution after adding smoke tools, and review screenshots for information density before considering a GUI smoke complete.
- Potential skill candidate: yes, for reusable desktop smoke seed tooling and timeline/history path summarization checks.

## 2026-07-09 - Shared WPF smoke helper foundation reflection

- What worked: Starting with the already-verified undo smoke kept the helper extraction small and quickly re-verifiable.
- What slowed down: Nothing significant; the static test made the helper contract straightforward.
- Root lesson: Shared smoke helpers are safest when introduced one consumer at a time, with the original GUI smoke rerun immediately.
- Waste: Minimal.
- Process improvement: Migrate repeated `.omx` scripts gradually and keep action-specific seed/selection logic local to each smoke.
- Potential skill candidate: yes, for a reusable WPF smoke helper template.

## 2026-07-09 - App drawer helper migration and smoke docs reflection

- What worked: Migrating one high-value smoke after adding a static red test kept the refactor honest; the real app-drawer smoke still proved all four preview actions.
- What slowed down: The app-drawer script has useful action-specific helpers that should not be globalized prematurely, so the helper boundary needed a small design choice.
- Root lesson: Shared tooling should absorb stable primitives first, while leaving product-flow decisions local until reuse proves they belong in the shared helper.
- Waste: Low. The GUI smoke took longer than unit tests because it scans real software, but it verifies a user-critical path.
- Process improvement: After each smoke helper migration, rerun that exact GUI smoke, not just static tests.
- Potential skill candidate: no new global skill beyond the existing WPF smoke helper template.

## 2026-07-09 - Agent system-tools helper migration reflection

- What worked: The system-tools smoke was a good third consumer because it uses the same WPF primitives but has a different safety boundary: verify entry points without opening tools.
- What slowed down: Very little. The red test made the migration target precise, and the existing helper already had the needed screenshot/window functions.
- Root lesson: For safety-sensitive GUI smokes, the shared helper should reduce mechanical duplication while the script keeps its "what not to click" workflow easy to audit.
- Waste: Minimal.
- Process improvement: Keep migrating one GUI smoke at a time and run the real smoke immediately after each script refactor.
- Potential skill candidate: no new global skill; this reinforces the existing shared WPF smoke helper candidate.

## 2026-07-09 - Agent settings confirm-cancel helper migration reflection

- What worked: The real GUI smoke caught a fragile secondary-window lookup that static helper tests could not see.
- What slowed down: UIAutomation's desktop-wide descendant search failed in two different ways; it needed root-cause debugging rather than another wait/sleep.
- Root lesson: Confirmation-dialog smokes should find windows by process-owned native handles when possible, then use UIAutomation only for interaction.
- Waste: Moderate but useful. The failures produced a reusable prevention rule for future settings/system-tool confirmation smokes.
- Process improvement: When a GUI smoke needs a dialog, include a protected/native fallback test from the start instead of relying on `RootElement` descendants.
- Potential skill candidate: yes, for native-window fallback in WPF GUI smoke helpers or templates.

## 2026-07-09 - Agent background-review helper migration reflection

- What worked: This migration was mostly mechanical because the smoke only needed common WPF primitives and had no secondary-window behavior.
- What slowed down: The real smoke takes longer than static tests because it performs a read-only scan of the user's installed software, but that is exactly the user-relevant path it needs to prove.
- Root lesson: After the shared helper is established, product-flow scripts should stay readable: the common UIAutomation plumbing disappears and the safety promise of each smoke is easier to audit.
- Waste: Low.
- Process improvement: With the Agent smoke migration set complete, shift back to user-visible product work instead of continuing to polish test tooling.
- Potential skill candidate: no new candidate beyond the existing WPF smoke helper template.

## 2026-07-09 - Undo center technical-details reflection

- What worked: The two-layer model maps well to the product direction: the row stays readable, and the evidence remains available without inventing a separate technical page.
- What slowed down: The timeline presentation file still contains historical localized/mojibake strings, so the safer path was to add new ASCII/Unicode-escape-backed fields and avoid broad localization edits.
- Root lesson: Safety UI should not choose between simplicity and auditability. Put the action summary first, then keep exact evidence behind a named disclosure control.
- Waste: Low. The existing seeded undo smoke made it easy to prove the affordance in a real WPF window.
- Process improvement: For future timeline/history additions, add tests for both layers: first-level text must hide raw identifiers, second-level details must preserve them.
- Potential skill candidate: yes, extending the history-row summarization candidate with a "collapsed evidence details" pattern.

## 2026-07-09 - Low-risk cleanup preview reflection

- What worked: The existing recommendation-selection seam was the right place to improve clarity without touching the actual quarantine execution path.
- What slowed down: Broad `apply_patch` hunks failed again on mojibake-heavy XAML/context; small ASCII attribute anchors worked.
- Root lesson: For beginner-facing destructive-action previews, `CanContinue` and `CanExecuteDirectly` should be separate. A user may continue to confirmation while the preview itself remains non-executable.
- Waste: Low to moderate; patch retries cost a little time but avoided rewriting a larger XAML block.
- Process improvement: Future C-drive UI changes should use named control anchors and XML character references, then run build before any GUI smoke.
- Potential skill candidate: no new candidate; this reinforces existing WPF/XAML mojibake and structured safety-preview lessons.

## 2026-07-09 - Cleanup confirmation copy reflection

- What worked: Moving confirmation copy into a presenter made the safety language testable and removed path-first string assembly from the WPF handler.
- What slowed down: The old inline MessageBox string was embedded in mojibake-heavy code, so removing it required a narrow mechanical rewrite after `apply_patch` could not match the exact block.
- Root lesson: Path-heavy technical evidence should still be shown before execution, but not as the first thing a beginner sees. The first screen should say risk, effect, quarantine, and how to undo.
- Waste: Moderate; patch retries and the mechanical rewrite took time, but the final method is shorter and easier to test.
- Process improvement: For future cleanup UI, prefer adding a presenter first and binding/displaying its fields second. Avoid assembling user-facing safety text inside WPF event handlers.
- Potential skill candidate: no new candidate; this reinforces structured safety-preview and corrupted-XAML/code patch lessons.

## 2026-07-09 - Custom cleanup confirmation dialog reflection

- What worked: The presenter made the custom window straightforward: WPF just binds summary/details and returns a boolean confirmation.
- What slowed down: The window has static and build verification but not a real screenshot yet, because selecting an actionable cleanup card currently depends on real C-drive scan contents.
- Root lesson: Custom dialogs are worth it when the user must make a decision. MessageBox is too flat for "safe but scary" actions.
- Waste: Low. The existing presenter kept the new WPF surface small.
- Process improvement: Next step should be a stable fixture or smoke seeding path for C-drive recommendations so visual proof does not depend on the user's actual C drive.
- Potential skill candidate: no new candidate; reinforces "presenter first, WPF dialog second" for safety prompts.

## 2026-07-09 - C-drive cleanup fixture smoke reflection

- What worked: A process-scoped scan-root override lets the WPF app exercise its real C-drive scan, recommendation selection, and confirmation-window code against a tiny `.omx` fixture instead of the user's real disk.
- What slowed down: GUI launch was rejected by the approval/usage-limit system, so the new smoke could only be statically verified and build-tested in this slice.
- Root lesson: Stable GUI proof for safety-critical flows needs two layers: a fixture that cannot touch user data, and a real screenshot run when GUI approval is available.
- Waste: Moderate. Some time went into failed `apply_patch` and PowerShell `rg` quote checks, but those failures produced useful prevention notes.
- Process improvement: Keep smoke fixture environment variables documented as dev/test-only and run the real GUI smoke as soon as approval/usage is available.
- Potential skill candidate: no new candidate; reinforces existing isolated-storage and WPF smoke-helper lessons.
## 2026-07-09 - Cleanup confirmation outcome preview

- What worked: A tiny TDD slice cleanly separated Agent summary, after-confirm outcome, and technical details. This matches the product goal better than adding more text to the existing summary.
- What slowed down: The repository still has no initial commit, so `git diff --stat` does not summarize untracked-file changes; direct file/test evidence is more useful here.
- Lesson: For beginner-facing destructive-action gates, the last confirmation should answer "what happens next" explicitly, not only "why this is safe".
- Future optimization: Once GUI launch approval is available, rerun the isolated cleanup confirmation smoke and visually inspect the new outcome preview in the screenshot.

## 2026-07-09 - Uninstall plan window readability and hooks

- What worked: Static XAML tests quickly exposed that the uninstall plan window lacked UIAutomation proof targets even though the presentation model was already beginner-safe.
- What slowed down: Console/file encoding made some earlier inspection output look mojibake-heavy; test output showed the file text was readable, so the real fix was hooks and reliable list targets.
- Lesson: For WPF safety-plan windows, `ListBox` plus stable AutomationId is a better smoke-test target than anonymous `ItemsControl`.
- Future optimization: Add a GUI smoke that selects an app, clicks "卸载干净点", verifies the plan window hooks, captures a screenshot, and closes it without invoking any uninstall gate.

## 2026-07-09 - Uninstall plan window GUI smoke script

- What worked: Adding the smoke as a static red/green slice gave a concrete future GUI command without pretending the visual proof already exists.
- What slowed down: A fully stable GUI run still depends on approval/usage availability and at least one uninstallable app on the machine.
- Lesson: For destructive-adjacent flows, smoke scripts should explicitly prove the "close/cancel only" behavior and have static tests against execution markers.
- Future optimization: Run the smoke when GUI approval is available and, if `InvokePattern` fails to open the modal again, add a documented mouse-click fallback like the earlier settings confirmation smoke pattern.

## 2026-07-09 - Uninstall plan real GUI smoke fix

- What worked: The prior error ledger had enough evidence to avoid guessing. Adding a failing static test for descendant modal lookup kept the fix focused.
- What slowed down: The first smoke failure had no failure screenshot because the script only saved on success; future modal smokes should capture a diagnostic screenshot before throwing on window-not-found.
- Lesson: WPF modal windows can be missed by process root-child enumeration even when visible and accessible. Stable child AutomationIds plus parent-window walking are a better proof path.
- Future optimization: Promote `Find-WindowByDescendantAutomationId` or a safer equivalent into `.omx/wpf-smoke-helpers.ps1` if another smoke needs it.

## 2026-07-09 - Uninstall residue custom confirmation

- What worked: Reusing the existing cleanup confirmation model avoided inventing a second destructive-adjacent dialog and immediately gave residue cleanup the same quarantine/outcome/technical-details language.
- What slowed down: Normal PowerShell output rendered localized source text as mojibake, causing one failed deletion patch until exact UTF-8 lines were inspected.
- Lesson: In this repo, localized UI text should be patched with exact UTF-8 evidence or avoided as context; ASCII code anchors are safer.
- Future optimization: Add a residue-confirmation GUI smoke fixture so the shared confirmation window is visually proven from the uninstall residue path, not only through static handler tests.

## 2026-07-09 - C-drive confirmation GUI proof and shared modal helper

- What worked: The failed C-drive smoke matched a known WPF modal discovery pattern, so the fix stayed evidence-based: red test, descendant fallback, real smoke, then helper extraction.
- What slowed down: The same modal-discovery logic was duplicated before the second failure proved it belonged in `.omx/wpf-smoke-helpers.ps1`.
- Lesson: Once a GUI smoke workaround appears twice, promote it immediately into shared helper code and require scripts to dot-source it.
- Future optimization: Add diagnostic screenshots on modal-not-found failures before throwing, so future GUI failures have visual evidence even before the fix.

## 2026-07-09 - Residue confirmation fixture

- What worked: Designing the GUI fixture exposed a real product bug: cached installed-app state made the post-uninstall residue confirmation path unreachable. The fix improved both the testability and the product behavior.
- What slowed down: The real residue GUI smoke could not run because GUI launch escalation was rejected by the approval/usage-limit system, so the slice has strong static/unit/build evidence but no screenshot yet.
- Lesson: Flows that depend on a user doing something outside the app, such as running an official uninstaller, must refresh external state before making safety decisions.
- Future optimization: Run `.omx\gui-uninstall-residue-confirmation-smoke.ps1` when GUI approval is available, then add explicit inline "nothing happened" status after cancel and "moved to quarantine/timeline" status after confirm.

## 2026-07-09 - Residue cancel/quarantine inline outcome

- What worked: The existing app drawer action host could show outcome panels without adding a new modal or changing execution behavior.
- What slowed down: A localized-string patch failed again; ASCII control-flow anchors were the reliable path.
- Lesson: For beginner-facing maintenance tools, cancel is also an outcome. The UI should explicitly say "nothing moved" instead of quietly returning to the prior screen.
- Future optimization: Capture this path in the real residue GUI smoke once launch approval is available, then consider adding an undo-center jump/action from the success outcome.

## 2026-07-09 - Residue outcome undo-center navigation

- What worked: Making the outcome action key explicit kept the button safe: success can navigate to the undo center, while cancel has no action.
- What slowed down: None significant; the prior action-host structure absorbed the button cleanly.
- Lesson: A good "next step" button after a risky action should usually navigate to evidence first, not execute the next mutation.
- Future optimization: In the real GUI smoke, assert the button appears only after a success-like fixture path or add a non-mutating fixture state for the success outcome.

## 2026-07-09 - Residue cancel outcome smoke assertion

- What worked: The existing cancel-only smoke could be extended to prove the new inline result without adding any execution path.
- What slowed down: Real GUI launch is still unavailable in this continuation, so this remains static-script verified until approval/usage is available.
- Lesson: When a UI flow gains an outcome panel, the smoke should verify the post-dialog state, not just the dialog itself.
- Future optimization: Add a diagnostic screenshot after cancel, or capture a second screenshot for the outcome panel once GUI execution is available.

## 2026-07-09 - Residue cancel outcome screenshot

- What worked: Adding a second screenshot target was a tiny change that makes the future GUI evidence much more useful.
- What slowed down: Nothing significant; this stayed in the smoke layer.
- Lesson: For modal flows, keep one screenshot for the decision gate and another for the post-decision result when the user-facing state changes.
- Future optimization: Apply the same two-screenshot pattern to other confirmation flows only when the post-confirm/cancel state is meaningful.

## 2026-07-09 - Install routing learning memory

- What worked: The existing install-routing engine was small enough to extend with memory priority without changing analyzer safety behavior.
- What slowed down: I initially looked for install routing in the wrong project and then had to repair a localized output patch in `AnalyzeInstaller_Click`.
- Lesson: Learning mode should start as remembered recommendations, not automatic execution. That gives the user a smarter assistant without letting the app silently run installers.
- Future optimization: Add a visible "remember this route" control in the install page that writes `install-routing-memory.json` only after confirmation.

## 2026-07-09 - Install route remember button

- What worked: Keeping the button behind the existing installer analysis meant the save action could reuse the analyzed `InstallRoute` without adding any installer-running surface.
- What slowed down: Context compaction happened after implementation but before verification records, so fresh focused/product/full-suite verification was needed before claiming the slice.
- Lesson: For long-running autonomous development, record and verify the small product slice immediately after green tests so later continuations can trust the handoff.
- Future optimization: Replace the single coarse "remember this route" action with a small beginner-facing choice: remember for this software, remember for this category, or do not remember.

## 2026-07-09 - Install route memory scope choice

- What worked: The small custom window expressed the product decision better than trying to overload MessageBox buttons.
- What slowed down: One patch failed because I trusted a previous truncated/mojibake output instead of re-reading the exact UTF-8 lines first.
- Lesson: When the user-facing decision has multiple meanings, make those meanings visible as explicit choices rather than hiding them behind generic confirmation buttons.
- Future optimization: Add a learned-rules view/reset entry so the user can see what OMNIX-Entropy has remembered without opening the JSON file.

## 2026-07-09 - Learned install rules read-only view

- What worked: Making the memory visible first kept the slice safe and product-facing; the user can now audit what the Agent learned before we add any reset/edit action.
- What slowed down: Nothing significant; the existing memory store and install page were easy to extend with a presenter.
- Lesson: For assistant memory, visibility should precede mutation. Users need to see the rule before being asked to trust or change it.
- Future optimization: Add a forget/reset flow with a custom confirmation that states it only changes future recommendations and never touches installed software.

## 2026-07-09 - Forget learned install rule

- What worked: The existing row presenter made it straightforward to add a stable key and a disabled-by-default forget button.
- What slowed down: A static test method-extraction marker used the wrong existing handler signature and had to be corrected.
- Lesson: Even app-owned memory changes should use explicit confirmation and plain safety copy; "forget" should not imply uninstalling or moving software.
- Future optimization: Add GUI smoke coverage for select-rule -> cancel forget, using isolated `OMNIX_ENTROPY_DATA_ROOT` so the user's real learned rules are not touched.

## 2026-07-10 - Post-install change report cards

- What worked: Separating `InstallSnapshotDiffPresenter` from WPF made the beginner-facing copy testable before touching UI.
- What slowed down: The first WPF patch missed because old localized XAML text was fragile to match; anchoring the patch on `InstallDiffTextBox` after re-reading exact lines was reliable.
- Lesson: For install/security reports, the first visible result should answer "what changed?" and "should I care?" while raw evidence stays available behind a technical-detail affordance.
- Future optimization: Add a real install-page GUI smoke with an isolated before/after fixture and screenshot proving the cards appear before technical details.

## 2026-07-10 - Install report Agent explanation and GUI proof

- What worked: A small presenter turned technical diff counts into direct language, and the existing scan-sequence fixture exercised the real WPF flow without running an installer or touching installed software.
- What slowed down: Two patches repeated the known localized-context problem. More importantly, the first GUI JSON claimed visibility while the screenshot contradicted it, requiring a second TDD loop for scroll/capture state.
- Root lesson: Desktop automation state and rendered evidence answer different questions. Both are required for a beginner-facing safety conclusion.
- Process improvement: Inspect screenshots immediately after every GUI smoke; reject the gate when the target conclusion is absent even if UIAutomation reports success. Keep localized C# patch anchors entirely identifier-based.
- Future optimization: Extract the maximize/scroll/target-visible screenshot sequence into `wpf-smoke-helpers.ps1`, then add an install-report action-plan surface that remains plan-only until confirmation and rollback evidence exist.

## 2026-07-10 - Install report action plan

- What worked: Modeling the plan as ordered decisions kept the Agent in charge of the judgment while preserving the user's final authority. The same two-scan fixture proved the entire report -> explanation -> plan flow without touching real software.
- What slowed down: Two repeated patch-anchor mistakes ignored a prevention rule stated minutes earlier. GUI verification then exposed two Windows PowerShell 5.1 encoding/overload traps before the stable assertion form was found.
- Root lesson: “The screen contains the right sentence” and “the script literal compares equal” are separate encoding facts. Diagnostic output should print boundary values before any product change is considered.
- Process improvement: After the first patch mismatch, switch to one-file patches with freshly read anchors. For PowerShell GUI assertions, default to ASCII AutomationIds and char-code arrays; reserve localized literals for product XAML/C#.
- Future optimization: Build a read-only classifier for C-drive locations and background mechanisms so the Agent can replace generic review steps with evidence-specific conclusions while keeping execution disabled.

## 2026-07-10 - Install report evidence classification

- What worked: Segment-based C-drive rules avoided the common `AppData` false positive, and generic numbered items preserved one-to-one evidence without exposing identifiers. Feeding only the aggregate summary to the default UI kept the plan readable.
- What slowed down: I violated the one-file patch recovery rule once even after recording it. The first final screenshot also contained transient black composition blocks, requiring an unchanged rerun and another inspection.
- Root lesson: Local heuristics are useful only when confidence and privacy boundaries are explicit. Visual evidence remains probabilistic until the actual artifact is inspected.
- Process improvement: Treat turn-scoped prevention rules as hard constraints. For WPF evidence, keep the automation result and screenshot verdict separate and retain only artifacts that pass both.
- Future optimization: Add an on-demand evidence-review surface for “why?” that lists generic numbered findings, likely purpose, confidence, risk, and advice while leaving raw identifiers in technical details.

## 2026-07-10 - On-demand install evidence review

- What worked: Reusing the classifier’s existing per-finding models kept the change small and ensured the compact summary and expanded explanation could not disagree. The fixture smoke proved privacy and default-collapse behavior in the real app.
- What slowed down: The first record patch trusted mojibake terminal text, and desktop capture again produced intermittent black composition regions. Visual inspection also found misleading selection highlighting that static tests had not anticipated.
- Root lesson: A read-only explanation should look read-only, not merely lack click handlers. Default collapse controls information density; non-selectable styling controls perceived agency.
- Process improvement: Add visual-state assertions after functional UIA checks, and turn visible UX defects into a second red/green loop before accepting screenshots.
- Future optimization: Derive only a short list of eligible plan types from the evidence review, then ask the user for confirmation only when a complete evidence/rollback plan exists.

## 2026-07-10 - Evidence-driven eligible actions

- What worked: Modeling candidate kinds before UI copy made the safety policy explicit: cache/startup can produce plan candidates, while services/tasks/unknown evidence only produce observation. The final screenshot reads like Agent judgment instead of a menu of technical operations.
- What slowed down: Desktop verification uncovered two independent automation assumptions. `WaitForInputIdle` did not guarantee focus, and nested `IsOffscreen` did not guarantee rendered visibility. A mistaken Windows `rg` glob also interrupted the first diagnostic batch.
- Root lesson: GUI automation needs to test the state it actually depends on. Process idle is not focus readiness, and automation-tree visibility is not screenshot visibility.
- Process improvement: Use bounded condition polling and viewport geometry, inspect every retained screenshot, and promote the helpers after they prove stable in this smoke.
- Future optimization: Turn an eligible candidate into an on-demand preview only when its required evidence can be supplied by an existing safe planner; otherwise explain the missing evidence and refuse preview generation.

## 2026-07-10 - On-demand candidate plan preview

- What worked: Reusing the cache, startup, and migration presenters preserved mature safety language and made ownership/refusal behavior testable without duplicating operation logic. The inline preview keeps the Agent responsible for interpretation while exposing no execution control.
- What slowed down: GUI validation followed three increasingly forceful focus strategies before questioning whether focus was needed at all. A broad patch anchor also misplaced a test helper declaration, and a repeated worklog line misplaced the first record append.
- Root lesson: Verify the prerequisite itself before hardening retries around it. UIAutomation invocation and screenshot visibility do not imply keyboard focus; install-diff correlation also does not imply app ownership.
- Process improvement: For GUI smokes, state the required observable outcome first, then choose the weakest OS interaction that proves it. For append-only records and large test files, anchor on unique section or method identities and verify placement immediately.
- Future optimization: Rerun the exact fixture smoke when GUI approval is available, inspect the candidate preview screenshot, then choose the next V1 slice from user-visible product value rather than adding more installation-report detail.

## 2026-07-10 - Uninstall recovery truth

- What worked: Separating application recovery from residue recovery produced much clearer copy and exposed a real safety flaw in the old snapshot-only gate. The modal now answers the user's concern before showing technical machinery.
- What slowed down: A fragile WPF bullet format failed only at the full app build, and a repeated test initializer received data intended for a neighboring method. The first final screenshot also had transient compositor black blocks.
- Root lesson: "Has a snapshot id" is not the same as "can recover the application." Recovery must be typed, explainable, and evidenced; visual proof must be rejected when the artifact itself is corrupted.
- Process improvement: Build immediately after structural XAML edits, anchor large-test edits on method signatures, and keep screenshot automation results separate from screenshot quality verdicts.
- Future optimization: Teach the inventory scanner to discover trustworthy reinstall sources so the Agent can prepare recovery evidence automatically instead of asking a beginner to understand it.

## 2026-07-10 - Read-only reinstall-source discovery

- What worked: Separating raw registry hints from verified installer evidence kept the user-facing answer simple while making the gate materially safer. Connection tests caught the risk that a correct presenter could remain disconnected from the real scanner or WPF entry point.
- What slowed down: One source path was assumed instead of discovered, and the initial TDD pass did not directly lock the scanner-to-factory and UI-to-signature-inspector connections; both were then removed, tested red, and restored.
- Root lesson: Evidence models are only useful when provenance and real composition are tested. A directory, product code, or plausible filename must not silently become a recovery guarantee.
- Process improvement: Add one composition test for each safety-critical adapter after unit-level red/green, and use `rg --files` before parallel source reads.
- Future optimization: Let the user select an official installer or inspect an existing restore point through a guided recovery-preparation flow, while preserving separate backup and execution confirmations.

## 2026-07-10 - Recovery preparation and evidence snapshots

- What worked: Reusing one reinstall-source verifier for automatic and user-selected files avoided divergent trust rules. Treating restore points as hints and evidence snapshots as non-rollback facts kept the user promise honest.
- What slowed down: GUI usage was exhausted before the updated recovery panel could be rendered, so the frontend gate remains deliberately Warn. Two source-file assumptions and one quoted search command also failed before symbol discovery corrected them.
- Root lesson: Every readiness token must map to a verifiable artifact. A string id, an old restore point, and an evidence manifest have different meanings and must not be interchangeable.
- Process improvement: Add adapter composition tests, validate identifiers against typed evidence, and separate automated UI contract evidence from screenshot evidence in status reports.
- Future optimization: Create a non-executable final-confirmation draft from the verified installer, backup acknowledgment, and local evidence snapshot, then visually simplify the recovery panel before exposing that draft in WPF.

## 2026-07-10 - Non-executable final-confirmation draft

- What worked: Refusing before snapshot-root creation made the no-side-effect boundary easy to test. Keeping ready facts separate from pending confirmations preserved the distinction between evidence and intent.
- What slowed down: One test used invalid collection spread syntax and had to be corrected before behavior could be observed.
- Root lesson: A confirmation draft should aggregate facts, not silently acquire authority. Filesystem side effects should begin only after prerequisites are complete and must remain independently verifiable.
- Process improvement: Add source-level no-execution contracts to orchestration services whose names could tempt future callers to expand scope.
- Future optimization: Bound local manifest retention and then expose the draft only after the existing recovery panel receives real screenshot review.

## 2026-07-10 - Read-only snapshot retention planning

- What worked: Top-level enumeration plus schema/purpose/filename validation made the ownership boundary straightforward. Testing corrupt and unknown files prevented an attractive but unsafe wildcard cleanup.
- What slowed down: None beyond the normal red/green cycle.
- Root lesson: Retention is still destructive unless ownership is proven. Unknown evidence should be retained and explained, not guessed away.
- Process improvement: Separate candidate discovery from action creation and make the discovery model explicitly non-executable.
- Future optimization: Add a reversible archive operation for validated candidates, with source/destination provenance and restore support before any purge policy exists.

## 2026-07-10 - Reversible snapshot archive operation

- What worked: Reusing quarantine/timeline gave archive records an immediate, tested restore path. Hash-bound descriptors and whole-batch prevalidation kept plan/execution drift visible.
- What slowed down: One JSON reader omitted the writer's naming policy and rejected a valid manifest until the error was surfaced before payload assertions.
- Root lesson: Execution must revalidate plans, and persisted-schema settings are part of that evidence contract. Batch rollback needs a directly injected failure test, not just a preflight-failure test.
- Process improvement: Assert result success/error before payload, provide injectable boundary adapters for runtime failure tests, and keep destructive handlers free of permanent-delete APIs.
- Future optimization: Build an unregistered official-uninstaller handler around an injected launcher, then connect it only after UI confirmation and visual gates are satisfied.

## 2026-07-10 - Unregistered official-uninstaller handler

- What worked: Gate-generated manifests provided enough evidence to revalidate command identity inside the elevated boundary. Fake launcher/post-scan adapters made nonzero exit and post-scan failure semantics testable without process risk.
- What slowed down: A generic non-empty string reader initially rejected valid no-argument uninstall commands; a focused red test isolated the field-specific rule.
- Root lesson: Elevated code should distrust even gate-generated descriptors and reconstruct trust from the hashed manifest. Empty command arguments are valid, while empty evidence fields are not.
- Process improvement: Keep high-risk handlers unreachable until their adapter, registration, UI confirmation, and visual gates each have independent evidence.
- Future optimization: Add a real launcher adapter and real post-scan adapter as separate unregistered units, then perform a final requirement audit before any App wiring.

## 2026-07-10 - Unregistered Windows launcher adapter

- What worked: Injecting a runner let tests inspect exact `ProcessStartInfo` and simulate UAC cancellation/exit codes without process risk. Static isolation tests made reachability visible.
- What slowed down: Cancellation semantics needed an explicit red test to prove generic exception handling would otherwise swallow `OperationCanceledException`.
- Root lesson: User-cancelled UAC and application cancellation are different outcomes. Real process APIs should be isolated to one file and registration should be separately reviewable.
- Process improvement: Test cancellation before broad exception catches and keep adapter construction separate from runtime registration.
- Future optimization: Implement the mandatory post-uninstall scan adapter using fresh inventory and manifest evidence, still unregistered.

## 2026-07-10 - Unregistered real post-uninstall scan adapter

- What worked: Reusing the existing residue builder preserved established risk grouping while an injected inventory/path boundary made every current-state claim testable.
- What slowed down: Two assertion syntax errors obscured the initial missing-type TDD red until the focused compile separated them.
- Root lesson: A pre-uninstall manifest is evidence of what existed, not proof of what remains. Current path probes can support residue candidates; old background identifiers require a fresh specialized scan.
- Process improvement: Keep fresh-state evidence and historical hints as separate typed fields, and report inventory failure instead of collapsing it into an empty result.
- Future optimization: Add a beginner-facing post-uninstall result presenter before any WPF/handler registration, then visually validate the recovery panel when GUI usage returns.

## 2026-07-10 - Beginner post-uninstall result presentation

- What worked: Four explicit states prevented scan failure, app presence, clean scan, and review-needed results from collapsing into one vague message. Source-level forbidden-API checks kept the presenter pure.
- What slowed down: The first green attempt placed the app-presence evidence only in the conclusion, while the compact facts list needed the same reason for scanning clarity.
- Root lesson: Beginner interfaces need the blocking reason in the first scannable facts, not only inside explanatory prose. Raw backend summaries are not safe presentation content.
- Process improvement: Test visible text for path/identifier absence and test each safety-blocking state independently.
- Future optimization: Add fresh background re-enumeration so service/startup/task counts can become verified technical evidence rather than historical hints.

## 2026-07-10 - Fresh background residue re-enumeration

- What worked: Exact-name tri-state probing made the safety semantics small and testable. A real reader could be compiled while fake readers proved behavior without touching Windows state.
- What slowed down: None beyond the deliberate second red test that required explicit beginner wording for high-risk background records.
- Root lesson: Read failure is not absence. Background system entries require stronger uncertainty semantics than ordinary path candidates, and verified presence still does not imply permission to disable.
- Process improvement: Isolate every Windows read boundary, reject crafted identifiers before API access, and retain a static no-registration/no-mutation contract.
- Future optimization: Audit the complete unregistered composition and define the serialized elevated request/response boundary before any WPF wiring; the recovery panel screenshot remains a prerequisite.

## 2026-07-11 - Elevated boundary and recovery GUI gate

- What worked: Separating request integrity, visual proof, user consent, and response correlation made the final handoff auditable without adding reachability. Status markers plus a desktop screenshot turned an ambiguous GUI timeout into two precise root causes.
- What slowed down: The first static helper assertion landed in a similar earlier test, and the first WMI compile used an ambiguous type name. Several GUI reruns were needed because the initial status covered multiple boundaries.
- Root lesson: Optional OS evidence needs a timeout and an explicit Unknown state. Visible WPF owned windows are not guaranteed to appear in UIAutomation root enumeration even when their child peers are valid.
- Process improvement: Add boundary status markers earlier during GUI diagnosis; anchor repetitive test-file patches by method name; keep a shared native-window fallback while preserving semantic AutomationId checks.
- Future optimization: Put the existing non-executable final-confirmation draft into WPF now that the recovery panel has screenshot proof, then add a screenshot-backed result panel before reviewing any handler registration.

## 2026-07-11 - WPF final-confirmation checklist

- What worked: Reusing the draft service preserved the tested no-write-on-refusal and no-execution guarantees. Stable AutomationIds let the smoke prove missing requirements and the absent evidence root without reading technical paths.
- What slowed down: Source-contract tests did not compile the WPF project, so an `ItemsSource` type error appeared only at build. Windows PowerShell source encoding caused a false negative, and the desktop compositor produced an unusable black-block screenshot before GUI quota ended.
- Root lesson: UI source tests, WPF compile, semantic automation, and visual screenshot are four separate gates. Passing the first three cannot substitute for the fourth.
- Process improvement: Add explicit App build immediately after WPF edits; use Unicode code points in PowerShell; reject compositor artifacts even when automation data is correct.
- Future optimization: Rerun the unchanged smoke when quota returns, then surface the already path-free post-scan presenter in WPF without registering execution.

## 2026-07-11 - Post-scan WPF and one-time visual receipt

- What worked: Keeping the display model in Core gave WPF safe data without an Elevated project reference. Stable AutomationIds plus a DEBUG-only fixed fixture produced direct visual proof without touching installed software. Hashing PNG bytes before storing the receipt made caller-buffer mutation irrelevant.
- What slowed down: The first checklist rerun used a stale WPF binary because focused tests did not build the app. The image viewer's original-size rendering briefly looked like a bad capture until file-level pixel checks separated viewer behavior from PNG content.
- Root lesson: UI source, built binary, UIAutomation state, saved image bytes, and image-viewer rendering are distinct evidence layers. A high-risk request gate should consume fresh state exactly once, but process-local replay prevention is not a substitute for authenticated IPC.
- Process improvement: Add a mandatory WPF binary build before GUI smokes; use shared path-free view models across process boundaries; document precisely what each security gate does and does not defend.
- Future optimization: Build an authenticated fake end-to-end App/elevated transport with exact final consent and typed result-window presentation before registering any real launcher.

## 2026-07-11 - Final consent and authenticated fake transport

- What worked: Extracting pure consent/response contracts let WPF remain independent of the elevated project. The three-checkbox GUI produced an immediately understandable final decision. Recomputing the descriptor hash at the endpoint caught mutation that a metadata-only HMAC would miss.
- What slowed down: One assumed source filename broke a parallel inspection batch, and the first integration test exposed a nullable warning even though behavior passed.
- Root lesson: UX proof and authority proof should be connected conceptually but verified independently until the real IPC boundary exists. Authentication must cover canonical metadata and the receiver must independently validate the referenced payload.
- Process improvement: Keep a zero-warning focused-test gate; locate type declarations before reading guessed files; require authentication before replay-state allocation and before any handler call.
- Future optimization: Convert the in-memory protocol semantics into a bounded serialized named-pipe transport with Windows identity/PID checks, then connect the production final-consent flow without changing real launcher registration.

## 2026-07-12 - Bounded serialized fake named-pipe transport

- What worked: Building the strict codec before WPF wiring exposed type preservation, payload bounds, response privacy, identity, timeout, and replay as independently testable facts. A live same-process Windows pipe proved the real SID/PID/session reader instead of relying only on mocks.
- What slowed down: An adversarial client closing early caused response-delivery failure to mask the stronger endpoint rejection until the focused test isolated it.
- Root lesson: Privileged IPC needs two truthful result layers: whether the request was accepted and whether the response was delivered. Peer disconnect must never rewrite a pre-execution security refusal.
- Process improvement: Test malformed length, malformed JSON, request tamper, response tamper, replay, wrong SID/PID/session, startup timeout, response timeout, cancellation, and early disconnect before adding any runtime registration.
- Future optimization: Move client-safe contracts into a neutral IPC assembly, then connect the DEBUG final-consent fixture to the fake endpoint without giving App a reference to Elevated.

## 2026-07-12 - Neutral IPC library and DEBUG WPF pipe flow

- What worked: Moving pure contracts inward allowed the exact authenticated/serialized pipe implementation to reach WPF without importing execution authority. Distinct fact counts made the GUI proof discriminate real pipe success from both fixed and failure fallbacks.
- What slowed down: Adding a project required an escalated restore; one multi-file test-path patch used a stale anchor; the first GUI run had a generic transient visibility failure.
- Root lesson: Project references are security boundaries, and GUI evidence must prove the intended data journey rather than merely show a plausible result window.
- Process improvement: After source moves, compile projects before tests, update static source paths one at a time after any anchor miss, and give every required UI peer a named diagnostic.
- Future optimization: Add a separate-process session bootstrap that derives fresh keys from identity-bound ephemeral exchange without command-line/environment secrets, still before registering any handler.

## 2026-07-12 - Identity-bound ephemeral IPC session bootstrap

- What worked: Treating OS pipe identity as authentication and ECDH as secret establishment kept responsibilities precise. Two-way finished proofs made identity/transcript tamper fail deterministically rather than producing mismatched keys later.
- What slowed down: Only one version-specific FluentAssertions method assumption; focused compile caught it before behavior work.
- Root lesson: Ephemeral key exchange is not self-authenticating. It becomes suitable here only when bound to independently observed SID/PID/session and confirmed over the full canonical transcript.
- Process improvement: Test key agreement, context mismatch, proof tamper, nonce replay, malformed frames, timeout/cancel, and zeroization as separate invariants before composing the command transport.
- Future optimization: Exercise bootstrap plus authenticated request/response across an actual test child process with exact PID/session and bounded shutdown, while Elevated Program remains untouched.

## 2026-07-12 - Separate-process authenticated smoke worker

- What worked: composing the already-tested bootstrap, strict codec, and authenticated endpoint in a disposable child exposed path/configuration and receipt-contract mistakes without risking a real uninstaller.
- Waste: two small test-fixture assumptions caused avoidable red runs: a missing `Css.Core.Apps` import and an incorrect output-directory parent traversal.
- Improvement: future process smokes should define an explicit machine-readable receipt schema and derive tool paths from a tested helper before the first launch.
- Safety lesson: successful IPC is not enough; exact peer identity, bounded exit, and forced cleanup need direct tests because they are independent failure modes.

## 2026-07-12 - Runtime final-consent visual receipt

- What worked: a real STA modal-window test exercised WPF layout, button behavior, rendering, receipt issuance, ticket consumption, and byte zeroization without adding a production execution route.
- What slowed down: enabling WPF in the existing test project changed implicit-using behavior; preserving the prior SDK using set fixed the broad compile fallout. One stale static assertion and one smoke-comment symbol were also missed in the ownership migration.
- Root lesson: visual proof should be generated by the UI-owning process, while pure ticket/hash state belongs in an inward dependency. A fixed hash is not evidence.
- Remaining evidence: automated pixels prove a nonblank render and viewport checks, but a human-inspected fresh screenshot is still required for beginner-facing layout confidence.

## 2026-07-12 - Production fake elevated worker lifecycle

- What worked: reusing the established pipe identity, bootstrap, authenticated codec, and typed response contracts kept the new production-shaped slice focused on launch ownership and process cleanup. Real child tests made wrong PID, session mismatch, timeout, and delayed exit observable.
- What slowed down: parallel builds of projects sharing Ipc contended on one compiler output, and the new WPF source relied on an unavailable implicit `System.IO` import.
- Root lesson: privileged worker correctness has two independent endings: the request result and the child-process result. A valid response is not sufficient until the child exits or its whole tree is terminated within a bound.
- Product lesson: a fake elevated response must explicitly say no uninstaller ran and must stay disconnected from the production button; otherwise technically safe scaffolding would still mislead a beginner.
- Next improvement: provide a path-free lifecycle presenter and a test-only real-`runas` orchestration smoke, then verify the actual Windows prompt and packaged worker path before composing real uninstall authority.

## 2026-07-12 - Beginner worker result and build packaging

- What worked: treating packaging and beginner presentation as security gates exposed two distinct truths: the worker can be present without becoming an App dependency, and a transport outcome can be explained without revealing protocol details or pretending an uninstall occurred.
- What slowed down: one combined publish check produced misleading empty formatting, and raw Chinese in a no-BOM PowerShell script failed under Windows PowerShell's legacy encoding rules.
- Root lesson: release artifact existence, dependency absence, and smoke-route absence need independent named checks. Empty command output is not evidence.
- UX lesson: the most useful failure screen answers only three questions: did anything start, did the computer change, and what should I do next. The inspected compact panel does that without a technical details card.
- Next improvement: collect actual Accept/Cancel screenshots on the secure desktop path, enforce worker signature/publisher trust, then keep the first real handler registration limited to the official uninstaller plus mandatory read-only rescan.

## 2026-07-12 - Signed worker production trust gate

- What worked: separating Windows trust, signer identity, file hash, policy, presentation, and launch-time recheck made each claim independently testable. A real Microsoft fixture and a tampered copy gave stronger evidence than mocks alone.
- What slowed down: the first empty dictionary fixture used an unsupported collection expression; `notepad.exe` exposed the catalog-versus-embedded distinction; byte tampering changed the native diagnostic category while still correctly removing authority.
- Root lesson: a display certificate subject is not an authorization identity. Production trust needs OS validation plus a stable certificate fingerprint, and tamper tests should assert loss of trust rather than overfit one HRESULT.
- Product lesson: unsigned local builds can remain useful without weakening release safety when `development verification` and `production authorization` are different types/facts and only the former reaches the fake endpoint.
- Next improvement: close the post-launch image-correlation gap, collect real UAC screenshots, then introduce a real worker composition whose only mutation is the user-confirmed official uninstaller and whose mandatory follow-up is read-only.

## 2026-07-12 - Render evidence and production consent entry

- What worked: exporting the same PNG bytes used for the receipt turned an abstract passing test into inspectable evidence and immediately found two real capture defects.
- What slowed down: a source-text assertion decoded `\u` escapes instead of matching their literal form; a test ticket factory initially captured an `out` parameter. Both were narrow test mistakes.
- Root lesson: nonblank pixel checks do not prove a trustworthy screenshot. Crop, transparency, first-frame composition, and visible control order need image inspection plus stable element tests.
- Product lesson: “official command confirmed” is not equivalent to “app closed.” The first beginner checkbox can combine them in plain language, but the consent contract must retain both facts separately.

## 2026-07-12 - Privileged execution boundary progression

- What worked: moving one gate at a time from fake transport to real process composition kept every authority increase observable: post-start image check, worker-side signer check, pre-bootstrap authorization, request freshness, manifest-bound scanner, then production mode.
- Waste: repeated PowerShell `&&` mistakes and the broad Scanner reference created avoidable noise. Both now have explicit prevention rules.
- Product lesson: “safe” is not only refusing dangerous input; it also means never treating unreadable state as clean, never extending an old user confirmation, and always observing partial uninstall outcomes.
- Next improvement: App production lifecycle/result types should distinguish trust denial, stale request, official uninstaller outcome, and post-scan truth without exposing protocol language or offering residue mutation.

## 2026-07-13 - Typed production lifecycle result

- What worked: expressing production authority as a separate interface made incorrect fake/production combinations testable before any process starts. Reusing the typed handler payload let the UI explain real outcomes without parsing strings.
- What slowed down: the first white-box test assumed App internals were visible and briefly tried to call an internal argument seam directly; a negative `rg` audit also needed explicit no-match handling.
- Root lesson: tests should not widen a security boundary. Keep command construction internal and test it through bounded reflection or behavior, while public construction remains tied to trust evidence.
- Product lesson: “worker returned” is not a useful conclusion. Beginners need separate plain-language outcomes for uninstaller not started, uncertain completion, failed recheck, software still present, residue found, and clean removal.
- Next improvement: carry the prepared request through a WPF execution coordinator that first presents trust denial or then runs the typed production lifecycle and post-scan result.

## 2026-07-13 - WPF execution boundary connected

- What worked: injecting one coordinator let the product path advance without granting the click handler command/process authority. The current unsigned build now fails in a user-visible place before UAC instead of leaving a dead-end request.
- What slowed down: an old static test represented a temporary “never execute” phase as a permanent rule; the known finished-message status fluctuation recurred during full regression.
- Root lesson: staged safety tests need to state the durable invariant. Here it is “WPF cannot own privileged execution,” not “WPF can never call an async coordinator.”
- Product lesson: a disabled production package should still answer the user clearly. “This is an unsigned development build, so I stopped before Windows confirmation” is part of the real workflow, not an implementation footnote.
- Next improvement: connect a successful production post-scan to the existing residue risk grouping and quarantine/restore flow, preserving exact request/software correlation.

## 2026-07-13 - Residue review correlation after tile removal

- What worked: retaining the pre-uninstall profile solved the otherwise awkward case where successful uninstall removes the very tile needed to identify cache/log/data candidates. Reusing the refreshed inventory avoided a second scan.
- What slowed down: one older static test matched an exact local declaration and failed after the freshness source became injectable.
- Root lesson: cross-process results should carry the minimum authenticated truth needed to decide the next local observation, not every sensitive detail needed for a later operation.
- Product lesson: the result must stay visible after the app disappears from the grid. Refresh first, then render the Agent review or outcome.
- Next improvement: apply the same coordinator/evidence pattern to migration, including rollback manifest validation and post-migration original-path write monitoring.

## 2026-07-13 - Migration coordinator before Windows mechanics

- What worked: separating path mechanics from orchestration let tests prove denial-before-mutation, reverse rollback, and persistent closure monitoring without touching a real C/D directory. Hashing the exact JSON closed the obvious editable-plan gap.
- What slowed down: one test fixture used invalid object-initializer syntax on a factory result; the compiler caught it before runtime.
- Root lesson: for risky filesystem work, first define what evidence authorizes the operation and what observable state proves rollback. The copy/junction implementation is secondary and replaceable.
- Product lesson: “C path exists after migration” is not automatically bad because a healthy redirect must exist there. The Agent should warn only when the redirect is missing, changed, or replaced by a real folder.
- Next improvement: implement and fixture-test Windows directory copy verification plus redirect/rollback, then build the beginner result panel before any real WPF enablement.

## 2026-07-13 - Verified copy before redirect

- What worked: keeping redirect creation injected allowed real nested-file copy/restore tests without requiring Developer Mode or UAC. The explicit second verification before source deletion makes the destructive boundary easy to audit.
- What slowed down: a test helper named `Path` shadowed `System.IO.Path`; normal-user symbolic-link creation was unavailable as expected.
- Root lesson: the rollback adapter must be able to recover from the exact point after source deletion but before redirect creation. Testing that point directly is more valuable than a generic “move failed” mock.
- Product lesson: the beginner result should say that OMNIX will keep watching C, not merely “migration succeeded.” Closing the loop is the feature users care about.
- Next improvement: run the adapter only inside an authenticated signed elevated worker, add actual component probes, and expose a DEBUG fixture flow before any real plan button.

## 2026-07-13 - Green tests are not artifact security acceptance

- What worked: the user supplied the actual antivirus log quickly, and stopping all execution plus checking live processes limited uncertainty.
- What slowed down: high-risk negative fixtures and real worker integration tests were compiled into one large test assembly, making heuristic attribution opaque.
- Root lesson: software that manages security-sensitive Windows operations needs a malware-scanner acceptance gate for generated test artifacts, separate from source review and passing tests.
- Product lesson: never teach a beginner to whitelist an alert merely because the developer expects a false positive; preserve quarantine and obtain independent classification first.
- Next improvement: isolate hostile-input fixtures and process-launch integration tests by transparent responsibility boundaries, and retain per-artifact scan evidence after security-sensitive changes.

## 2026-07-13 - Source-only development still needs executable-quality discipline

- What worked: keeping artifact generation paused did not prevent tightening the real safety contract. The review found both a missing `SnapshotId` UI correlation and the need to compare current source state with snapshot evidence.
- What slowed down: one partial LINQ replacement left a duplicate file-read projection; immediate full-expression inspection caught it without compilation.
- Root lesson: a valid snapshot file proves what was observed, not that the source stayed unchanged. Risky execution needs a final observation near the mutation boundary.
- Product lesson: “Windows confirmation appeared” and “the worker replied” are not success. The beginner UI should say completed only for an exact correlated successful migration status.
- Next improvement: after corrected definitions arrive, rebuild narrowly, scan the artifact, run focused source tests, and capture the four-acknowledgement consent window before considering any feature-flag change.

## 2026-07-13 - Growth evidence should change the product conclusion, not add another report

- What worked: exact software-known paths plus bounded snapshot history let the product say “Docker Desktop 近期多次变大” without exposing paths or pretending one scan proves a trend. Bringing only sustained findings to home made the Agent more proactive without making the interface denser.
- What slowed down: source-only work hid a malformed backslash literal until direct inspection, and an attempted Roslyn parser produced misleading PASS output after failing to load.
- Root lesson: distinguish baseline, one delta, and sustained trend in the data model before writing UI copy. Also treat the verification toolchain as fail-closed software, especially when compilation is intentionally unavailable.
- Product lesson: one-time relief and preventing recurrence are separate decisions. A beginner should first hear what is repeatedly growing, then see a current action and a future prevention action; neither should automatically become migration.
- Next improvement: after corrected Huorong definitions arrive, run the focused growth/store/home tests and capture both first views. Then connect the sustained-growth home finding to a selected app drawer when attribution is unique, still as navigation/preview rather than direct execution.

## 2026-07-13 - Navigation needs identity and durable derived state

- What worked: separating a structured target from beginner copy made exact, missing, shared, and duplicate-name behavior explicit and testable. The same helper now serves home and C-drive entry points without gaining execution authority.
- What slowed down: the first implementation enriched application growth only in `ApplySession`; reviewing every inventory assignment exposed that a later app scan or uninstall refresh would erase the warning. One static-audit command also used the wrong PowerShell quote escape.
- Root lesson: derived view state must be recomputed at every replacement boundary, not only where it was first created. Static check scripts also need fail-closed output isolation.
- Product lesson: “take me to the app” is useful automation for a beginner; “choose the first thing whose name looks close” is not. A refusal is better than opening the wrong uninstall or migration preview.
- Next improvement: audit all visible controls for real handler and outcome coverage, then close the highest-value remaining preview-only entry while compilation remains paused.

## 2026-07-13 - Reversible means recovery is part of success

- What worked: auditing visible controls from the user's perspective exposed a real dead end, then reusing confirmation, pipeline, quarantine, timeline, and drawer result components kept the new flow understandable. A second policy narrower than scanner heuristics made cache cleanup defensible.
- What slowed down: connecting the operation uncovered two hidden transaction assumptions: refreshed path existence did not prove current app ownership, and timeline persistence sat outside rollback. Both required widening the review from the click handler to the whole recovery chain.
- Root lesson: a reversible operation is complete only when the exact current owner is proven, recovery coordinates exist before mutation, and the user-visible recovery index commits. Failure in any later step must compensate earlier mutation.
- Product lesson: the right beginner automation is not “clean everything called cache.” It is “I found a small set I can prove is low risk; close the app, confirm once, and I will keep a way back.”
- Next improvement: apply the same evidence/rollback discipline to startup control, but keep it plan-only until item identity, exact current-state snapshot, restore semantics, and runtime security gates are all proven.

## 2026-07-13 - A safe handoff can be a real feature

- What worked: checking the official Microsoft URI reference and reusing one allowlisted launcher turned a dead preview into a useful next step without inventing registry/service authority. Separating ordinary startup entries from services/tasks kept the explanation honest.
- What slowed down: the original `关闭自启动` label survived after the implementation strategy changed, briefly overstating what the click would do.
- Root lesson: when direct automation lacks rollback-grade identity, a confirmed handoff to a supported OS surface can close the workflow safely. The label and outcome must still match the exact authority of that click.
- Product lesson: for a beginner, “I found one ordinary startup item; I can take you to the correct Windows page, but you choose the switch” is more useful than either a technical report or a risky hidden registry edit.
- Next improvement: enrich scanner models with exact startup registry source/value, service mode, and task identity/state so future plans can explain each component and prepare real snapshots before any direct-management proposal.

## 2026-07-13 - A component observation is not a rollback snapshot

- What worked: separating stable identity from configuration fingerprint gives future revalidation a clean rule: find the same exact component, then stop if its observed configuration changed. Keeping the compatible name lists avoided broad churn in unrelated uninstall and migration code.
- What slowed down: source-only review had to compensate for the unavailable compiler. It caught two real semantic issues before runtime: null service start values coercing to Boot and task parsing potentially reading a trigger's Enabled flag. The audit command itself also repeated a nested backtick failure before the real checks ran.
- Root lesson: unknown Windows state must remain unknown, and every state field needs an exact schema scope. Observation fingerprints detect drift but cannot restore bytes, XML, ACLs, recovery settings, or approval state.
- Product lesson: better backend evidence should make the beginner surface shorter, not more technical. The main drawer keeps the plain conclusion; exact component sources belong only behind technical details.
- Next improvement: add a separate read-only StartupApproved correlation layer only when its state decoding can be justified and unknown bytes fail closed. Do not add direct disable until privileged rollback evidence and runtime gates exist.

## 2026-07-13 - Unknown is a useful product answer

- What worked: explicit registry views and separate Run/approval roots made correlation auditable without turning undocumented data into authority. Hashing then discarding the payload preserved drift evidence with minimal retention.
- What slowed down: the first model said “exact source” while still relying on an implicit HKLM registry view; a second review was needed to catch the 32-bit Run to 64-bit Run32 approval relationship. A quote-heavy XAML regex also failed before execution.
- Root lesson: exact Windows registry identity includes hive, view, key, and value. Presence and change are different facts from semantic state.
- Product lesson: “我找到了对应记录，但不能可靠判断开关，请在 Windows 页面确认” is better assistance than a confident guess that may disable the wrong behavior.
- Next improvement: move from static Agent panels to a real local question/answer interaction grounded in these structured observations, while keeping replies path-free and navigation-only.

## 2026-07-13 - Conversation should reduce decisions, not become execution authority

- What worked: one deterministic presenter turned health and application evidence into short answers, safe next steps, and exact internal navigation. The test design forced missing evidence, duplicate apps, stale targets, and private paths to become explicit refusal/fallback cases.
- What slowed down: a scroll viewer landed in the wrong repeated XAML shape, and several oversized PowerShell audits failed for quoting, path guessing, or generated-file scope before the final independent checks passed.
- Root lesson: natural-language UX and system authority must remain separate types and handlers. Verification tooling also needs the same fail-closed, bounded-input discipline as production code.
- Product lesson: the useful Agent answer is “这里是我从本机证据知道的结论，我可以带你去正确页面”；it is not another technical report and not silent consent to change the PC.
- Next improvement: after the antivirus gate clears, prove the first-visible answer and exact-app navigation with UIAutomation/screenshots. Until then, continue auditing source-only workflows for buttons that still end in explanation without a safe next step.

## 2026-07-14 - Translate at the beginner boundary

- What worked: mapping migration bands in the drawer removed every English sentence without touching planner semantics or execution readiness. Exact-app Agent answers improved automatically because they reuse the same projection.
- What slowed down: old tests encoded English wording rather than the durable beginner conclusion; they needed to be rewritten around Chinese safety meaning.
- Root lesson: keep engine contracts stable and build an explicit beginner projection. Presentation tests should assert decisions, proofs, and limits, not incidental internal phrases.
- Product lesson: “可以评估迁移，但先关闭后台、准备回滚、迁移后确认 C 盘不再增长” is a useful answer; an English enum/reason dump is not.
- Next improvement: prove these lines in the real drawer screenshot after the antivirus gate clears and continue removing technical leakage from first-level surfaces.

## 2026-07-14 - Visual identity is a data pipeline, not an XAML decoration

- What worked: following `DisplayIcon` end to end exposed both missing assignments, then a fail-closed parser and explicit fallback made every malformed or unavailable icon state understandable. File-version caching prevents repeated icon extraction during filtering.
- What slowed down: the first search batch repeated the known no-match `rg` failure, and `rg *.slnx` showed that an unconditional success exit can conceal stderr even when the final code work is sound.
- Root lesson: user-visible assets need provenance, normalization, propagation, bounded decoding, resource ownership, and fallback just like other evidence. Static audit wrappers must treat stderr as failure, not only exit codes.
- Product lesson: beginners recognize WeChat, Chrome, QQ, and Marvis by icon faster than by text. A real icon is valuable, but a predictable letter tile is better than a blank or network-loaded surprise.
- Next improvement: after corrected definitions arrive, run the parser tests, inspect actual registry icon formats, and capture a mixed real/fallback app grid before tuning icon sizes or cache policy.

## 2026-07-14 - A useful explanation must lead somewhere safe

- What worked: tracing each visible button to its final beginner outcome exposed a real product gap that a click-handler count could not detect. A typed two-destination contract reused existing pages and exact-app re-resolution without adding mutation authority.
- What slowed down: a narrow `rg` search assumed the health-finding builder lived beside its model and returned no match; symbol discovery immediately located the Scanner owner. The result was not used as verification evidence.
- Root lesson: “handler exists” and “workflow is connected” are different quality claims. Static audits should follow the user-visible outcome through response state, button placement, identity refresh, and final page.
- Product lesson: after Agent says what to do, one clearly labeled button is more useful than another paragraph or a menu of technical actions.
- Next improvement: once executable verification is safe, automate explain/detail/plan clicks and prove the response plus next-step button remain in the first visible home area before auditing the next dead-end workflow.

## 2026-07-14 - Opening an installer is a capability, not installation success

- What worked: auditing from the final `Process.Start` boundary backward showed that most of the difficult safeguards already existed: trusted signature, stable file identity, before snapshot, four explicit acknowledgements, short-lived consent, launch-time reinspection, parameter reconstruction, and a post-scan that refuses to equate exit code with success. That evidence justified removing the temporary product-wide block.
- What slowed down: the first availability design considered only the global flag and package capability. A second pass caught that an unavailable D drive would waste a scan and consent cycle. More seriously, one omitted `--no-build --no-restore` command triggered a blocked restore and invalidated generated NuGet assets.
- Root lesson: a high-risk feature should have three separate answers: is this product capability available, is this exact input eligible, and is the current environment ready. Build/test commands need equally explicit boundaries because generated dependency state is shared project state.
- Product lesson: the beginner-facing promise must remain “I can safely open the verified installer and tell you what changed,” never “I installed it successfully” or “it will never write C.”
- Next improvement: restore NuGet assets once approval is available, compile the two new target-refusal tests, rerun the full suite, then capture the fixture-only analysis and final-consent first view without opening any package.

## 2026-07-14 - Personal-storage diagnosis

- The safest useful duplicate feature is not deletion; it is a bounded evidence list that repeatedly says what has not been proven.
- Adding `IsFile` closed an important ambiguity: a large empty or aggregate directory must never appear as a personal-file candidate.
- Static UTF-8 validation was insufficient for Chinese UI quality. Semantic mojibake checks caught a defect that a parser and compiler would accept.
- The next iteration should add content hashing only as an explicit, cancellable secondary action after the user selects a small candidate set, not during routine scans.

## 2026-07-14 - Quarantine governance

- A retention limit is not a license for background deletion. The safe product shape is pressure detection, one bounded batch, an irreversible warning, and fresh path validation at the last responsible moment.
- Reusing manifest data for restore and purge required treating it as untrusted input. The important checks are relationships between root, item id, manifest, payload, and original path, not merely whether each string is absolute.
- Irreversible operations need different cancellation semantics: cancellation is useful before work starts, but once one record begins permanent deletion, finishing and journaling that record is safer than stopping midway.
- Source-only high-risk work remains a Warn. Formal compiler, temp-fixture execution, and real screenshot evidence are mandatory before this control can be considered release-ready.

## 2026-07-14 - Local health digest history

- Saving a digest only after a successful user scan preserves truth: history can summarize observed scans without pretending a background monitor exists.
- A beginner history needs trend and next action, not another path list. Path refusal at the store boundary is a useful second defense after presentation sanitization.
- The slice is source-complete, but SQLite behavior and first-view placement still need compiled tests and a real screenshot after dependency recovery.

## 2026-07-14 - Migration final-consent reachability

- Auditing from the button to the final Worker boundary exposed a workflow deadlock that isolated unit safety checks did not: the same consent facts were required both before and inside the only consent window.
- Separating machine evidence from human consent made the flow reachable without reducing safety. The important invariant is that evidence can enable only the final-consent transition, never execution itself.
- Truthful status propagation matters after execution too. A fixed “nothing moved” message is as harmful as a false success because it teaches the user not to trust the Agent's conclusions.
- Current confidence remains Warn until the new source compiles and fixture UIAutomation proves the enabled request button leads to the four-check confirmation without starting a real migration.
# 2026-07-14 - Migration closure monitoring slice

- The migration engine already had the hard part, but a record that never reaches the beginner is not a product feature. End-to-end audits must follow evidence all the way back to a visible conclusion and a safe next step.
- Separating observation from mutation made the desktop trust boundary easy to inspect. This should be preferred whenever a UI needs status from a subsystem that also owns destructive methods.
- The first UI pass still had a dead end for apps already on D. Checking the enabled state and the next click after every warning found it before delivery.
- Static safety evidence is useful during the dependency outage, but it cannot prove C# type correctness, redirect behavior, or first-view layout. Those remain explicit Warn items.
# 2026-07-14 - Whole-PC health dimensions slice

- The Home title had outgrown its evidence: a C-drive summary was being presented as a whole-PC check. Product audits should compare visible nouns such as `体检` against the exact data sources behind them.
- `NotPresent` and `Unavailable` are different beginner experiences. Treating a desktop's missing battery as a failed check would create needless anxiety.
- Count-only process observation is enough for a first health summary. Process names would add privacy and false-action pressure without supporting a safe decision.
- A score is also a claim. Keeping it disk-only and saying so is more trustworthy than quietly adding momentary metrics without a validated weighting model.

# 2026-07-14 - Post-install footprint and dependency-recovery slice

- Installation inventory and installation footprint answer different questions. The first can identify software; the second can notice a new landing point even when registration is absent. A useful report needs both and must not pretend either proves ownership alone.
- Completeness belongs in the data model, evidence fingerprint, diff algorithm, Agent wording, and plan gate. Handling it only in the final card would leave other surfaces able to issue false clean conclusions.
- Restoring dependencies paid down more than the current slice: it exposed two stale assumptions accumulated while source could not compile. The first full run after an outage should be treated as a reconciliation step, not a ceremonial check.
- A passing UIAutomation smoke and a visually clean screenshot are separate gates. The install flow is mechanically reachable and safe, while the repeated black compositor areas in the standalone Agent screenshot remain worth recording rather than explaining away.

## 2026-07-15 - Reversible startup control

- A useful beginner control needs a narrow authority boundary and a plain explanation of what remains untouched. “One ordinary current-user startup entry” is both safer and easier to understand than a broad background-component switch.
- Rollback evidence must exist before consent, but cancelled consent should not accumulate sensitive orphan manifests. Verified uncommitted cleanup makes the cancellation promise observable.
- Operation-specific restore copy matters. Reusing file-quarantine language for a registry value would make a technically working feature feel untrustworthy.
- The first GUI failure reinforced a process rule: when a WPF window never appears, inspect the terminated process and runtime event before assuming UIAutomation is flaky. The BOM stack trace made the fix small and testable.
- In-memory system-boundary fixtures are valuable only when they preserve production identity/freshness contracts and are impossible to enable accidentally as user settings. Real mutation acceptance still belongs to a disposable environment.

## 2026-07-15 - Quarantine identity closure

- “The path still exists” is not the same as “this is what the user confirmed.” A beginner-safe assistant must bind the object behind the path and be willing to ask for a rescan.
- Whole-batch preflight matters as much as rollback. Discovering a stale second item after moving the first creates avoidable recovery work even when rollback exists.
- Native interop tests need both a change case and an unchanged stability case. The former caught ID reuse; the latter caught the structure-layout bug.
- Cancel-only WPF proof remains the right ordinary-development boundary for cleanup: it proves reachability and wording without normalizing real destructive testing.

## 2026-07-15 - Agent startup truth and first-view proof

- An Agent becomes misleading when product capability changes but its wording does not. Every newly connected local action needs a sweep across aggregate answers, exact-object answers, summaries, and plan previews.
- Potential eligibility is useful for explaining where to go, but it must remain structurally separate from execution preparation. Cached presentation data should never become mutation authority.
- UIAutomation visibility alone missed a clipped heading; screenshot inspection caught it. Stable scroll positioning and a short redraw wait make the first-view evidence repeatable.
- Cross-version PowerShell encoding belongs in GUI-smoke design, not cleanup after failure. Small Chinese assertions can remain readable in test intent while the script source stays ASCII through named Unicode variables.

## 2026-07-15 - Home whole-PC health runtime acceptance

- Runtime values need semantic review, not only range checks. Two individually plausible 69.3% rows exposed a mislabeled-volume fixture that list-count assertions could never catch.
- Count-only process evidence gives useful scale without naming what the user is running. This is the right privacy level for a beginner summary.
- A product screenshot should contain the product. Window-only capture removed unrelated desktop context and made layout review much more reliable.
- Not-present and unavailable remained valid first-class outcomes in the GUI script, so hardware differences do not turn a safe acceptance check into a false failure.

## 2026-07-15 - Personal-file candidate acceptance

- The right product boundary is “help me notice and decide,” not “prove duplicate and remove.” Metadata-only candidates remain useful when the UI plainly names the uncertainty and refuses execution.
- Privacy is a composition property. A path-free target list can still fail the user when an adjacent recommendation card repeats the raw evidence.
- Item-level visibility assertions and screenshot inspection caught what container-level UIAutomation missed. Both gates are necessary for beginner-facing conclusions.
- Keeping real paths in the operation model while sanitizing only presentation avoided a false security tradeoff: privacy improved without weakening revalidation or rollback evidence.
- Full regression after a narrow GUI slice remains valuable; it caught a stale source-contract assertion that focused tests correctly did not exercise.

## 2026-07-15 - Install-report application handoff

- The safest way to connect a diagnostic to action is often navigation into an already hardened workflow, not duplicating its authority in the diagnostic surface.
- Unique ownership in an old report is enough to propose a destination, but not enough to execute. Resolving again against current inventory preserves that distinction.
- An Agent conclusion without a nearby next step still feels like a dead end. Placement is part of feature completeness, not polish after the fact.
- The fixture's app drawer made the boundary visible: the report opened the right application, while cache/startup/migration remained separate user choices with their existing evidence and confirmation gates.

## 2026-07-15 - Agent settings and troubleshooting routing

- A capable assistant does not need broad execution authority to be useful. Mapping ordinary language to one trustworthy evidence surface is a meaningful step when the answer also states what remains unknown.
- Catalog ids are a clean trust boundary: language may select among reviewed destinations, but it cannot synthesize an executable destination.
- Native WPF dialogs sit outside some convenient UIAutomation assumptions. Process confinement is necessary but not sufficient; semantic window identity and visual evidence are still required.
- The failed 262x71 screenshot justified the protocol's separate screenshot gate. Without it, the test would have reported a protected confirmation that the user never actually saw in the evidence.

## 2026-07-15 - Beginner hardware configuration answer

- A skill label is a product promise. Changing `外设状态和运行建议` to the narrower evidence actually collected improved trust as much as adding the probe.
- Real-machine tests matter for read-only adapters. Constructed evidence could not reveal that WMI was denied under the restricted process.
- Provider fallback should reduce privilege, not increase it. A fixed registry read and display enumeration were better choices than requesting administrator rights or spawning another tool.
- Hardware names are not performance conclusions. Requiring official minimum/recommended requirements keeps the Agent helpful without turning model names into false certainty.
- A prepared GUI script is not screenshot evidence. Recording the approval-quota block as Warn preserves honesty while allowing unrelated development to continue.

## 2026-07-15 - Interactive truthful Agent skill catalog

- A textual next step without an action is still a dead end. One predictable `问 Agent` command makes the catalog understandable without turning it into a control panel.
- Capability honesty belongs in product behavior, not only documentation. Unsupported cards now return a concrete absence-of-evidence explanation instead of vague roadmap language.
- Reusing the same response renderer keeps identity, safety, privacy, and first-view rules consistent across typed questions and skill-card entry points.
- The narrow handler authority test is valuable: a future edit cannot quietly turn a skill-card click into `Process.Start`, page execution, or an operation pipeline call.
- Visual layout remains unproven until the prepared smoke runs. Static XAML validity and AutomationIds reduce risk but do not show whether eight buttons feel crowded.

## 2026-07-15 - MSIX managed-storage handoff

- A truthful refusal still needs a usable next step. The right closure here was a Windows-owned setting, not inventing installer authority.
- Generic install UI can become contradictory at capability boundaries. Hiding the arbitrary D path and route-memory action mattered as much as adding the new button.
- A fixed catalog id keeps language and package metadata outside the executable URI boundary. The same reviewed destination can safely serve the installer panel and Agent conversation.
- Updated antivirus definitions restored normal compilation, but they do not guarantee UI automation availability. Build evidence, product runtime evidence, and screenshot evidence remain separate gates.

## 2026-07-15 - Recycle Bin review handoff

- A useful cleanup assistant does not have to perform the destructive step. Making evidence understandable and taking the user to the exact Windows review surface closes a real workflow gap without weakening consent.
- `清空` is an intent clue, not authorization. Reframing it as “look first” is especially important because ordinary Recycle Bin clearing is not covered by OMNIX quarantine or rollback.
- Typed presentation actions prevent a reusable list template from turning every large system store into a clickable command.
- Static checks need their own trust discipline: a zero shell exit cannot override a visible PowerShell binding error, so the clean rerun is the only accepted evidence.

## 2026-07-15 - C-drive root-cause safe internal handoffs

- An explanation becomes a useful assistant interaction when its next step lands on the exact existing evidence, not merely the right page name.
- Selecting an already-actionable recommendation is a good middle ground: the Agent reduces navigation work, while the user still sees the plan and retains the independent confirmation boundary.
- Typed actions let one compact card template serve several workflows without passing page names, commands, or arbitrary strings through the UI.
- Unexpected roots deserve deliberate asymmetry. A folder named `temp` at the drive root is not equivalent to a classified trusted temp area, so it correctly remains actionless.
- Static AutomationId checks are necessary but incomplete for data templates. The model must prove stable uniqueness when multiple items share one action type.

## 2026-07-15 - Beginner-first installer monitoring

- An automatic workflow can still feel manual when old fixture controls remain in the primary information hierarchy. Product truth includes what the page asks the user to understand, not only what the handler does.
- Preserving advanced diagnostics behind an explicit expander kept testability without teaching beginners an unnecessary three-step snapshot procedure.
- The static order test protects the intended hierarchy, while the fixture smoke's explicit expansion keeps diagnostic coverage independent from the ordinary default state.

## 2026-07-15 - Agent automatic read-only evidence hydration

- An Agent should do harmless preparation work itself. Asking a beginner to understand “应用画像” before receiving an answer defeated the product's central promise.
- Evidence needs should be intent scoped. Reusing the same inventory for software questions improved answers without making every settings or hardware question pay for an unrelated scan.
- Shared in-flight loading is both UX and safety infrastructure: it prevents duplicate expensive observations while keeping natural language outside every mutation boundary.

## 2026-07-15 - Agent-triggered C-drive read-only diagnosis

- “Go scan first” is the wrong interaction when the user has already asked the exact question that justifies a safe read-only scan.
- Automatic evidence work needs a narrower trigger as its cost rises. Software inventory can support broad general questions; a full disk diagnosis should require explicit C-drive intent.
- Failure privacy belongs in every automatic path. A previously tolerable technical exception becomes a direct beginner-facing leak once the Agent can invoke the workflow.

## 2026-07-15 - Automatic undo-center history loading

- A button label can contradict real behavior. Calling an already automatic page action `加载时间线` made users wonder whether the page was empty until they understood an internal data concept.
- Ensure-on-navigation and refresh-after-mutation are different contracts. Naming and testing both prevents either unnecessary work or stale recovery state.
- Read failures in a recovery surface should be especially conservative: “temporarily unavailable, nothing changed” is useful; a store path or exception message is not.

## 2026-07-16 - One-shot production submission and read recovery

- A final-confirmation dialog does not make its evidence reusable. Once a production coordinator is called, the reviewed snapshot and rollback plan must be consumed even if the outcome is refused, timed out, or unknown.
- Post-operation observation is part of the safety contract. A read failure must block residue and closure claims just as firmly as a failed preflight blocks mutation.
- Marking the attempt before `await` is what lets an outer window recover from coordinator exceptions without pretending nothing happened.
- The most useful unknown-result copy says what is known, what is not known, what the Agent will do next, and what it will not retry automatically.
- Source-level workflow tests should assert semantic boundaries; the low-level dependency belongs in the boundary helper's own test.

## 2026-07-16 - Real application-search placeholder

- Placeholder compatibility in a filter can hide a usability flaw without fixing it. The data layer was correct, but the beginner still had to erase instruction text.
- Search hints belong to presentation state; actual input values should represent only user or Agent intent.
- A fixed overlay keeps toolbar dimensions stable and avoids introducing a custom control or converter for one simple state.
- Top-level WPF windows often depend on `App.xaml`; structural XAML plus scoped handler tests are safer than casually creating a process-wide `Application` singleton.

## 2026-07-16 - Actionable uninstall post-scan result

- Advice that cannot be acted on is still technical debt for a beginner-facing Agent. The safe improvement is a small typed command, not another paragraph.
- Closing an explanation window must remain semantically neutral. Explicit review intent is distinct from acknowledging that the explanation was read.
- Read-only retry and mutation-capable review may share scanners and presenters, but they should remain separate methods so authority tests can prove the retry cannot drift into confirmation or quarantine.
- Rendering before delivery caught an accessibility detail that source binding alone did not: the primary button needed a deterministic initialized label for tests and assistive inspection.

## 2026-07-16 - Personal-file read-only location inspection

- Hiding every path protected the beginner from technical clutter, but it also removed the evidence needed to make a personal decision. Progressive disclosure is the useful middle ground.
- “Open location” must mean selecting the file in Explorer, never shell-opening the file itself. That distinction prevents an inspection action from executing unknown content.
- Current-scan membership matters even for harmless handoffs: it prevents a stale dialog or injected UI value from widening a bounded observation into arbitrary filesystem navigation.
- Possible duplicate is not duplicate proof. A convenient inspection button is appropriate; a cleanup button is not.

## 2026-07-16 - Persisted digest to current evidence

- Persisting a conclusion does not persist the evidence tree that produced it. Navigation must account for that difference after restart.
- Historical value and current truth can coexist in one interface if the action copy names the transition: `重新体检并查看当前证据` is more honest than `查看最新证据`.
- A shared read-only gate is useful beyond Agent questions. It lets home navigation, manual scanning, and natural language join the same observation without duplicate work.
- Success copy is part of the data contract. It belongs after an explicit current-summary check, not immediately after page navigation.

## 2026-07-16 - Agent background context handoff

- An explanation loses value when navigation drops its subject. Preserving a safe application target or catalog filter is part of the answer, not optional UI polish.
- Aggregate evidence should remain aggregate. Filtering to all resident applications is honest; guessing one application or opening a control plan is not.
- “Worth looking at” and “safe to disable” are separate claims. The first may justify details navigation, while the second still requires per-application evidence and confirmation.
- Typed UI context makes whitelisting straightforward. A nullable enum is safer and easier to test than a free-form filter name in Agent output.

## 2026-07-18 - Aggregate migration/uninstall catalog handoff

- A broad answer should narrow the workspace, not fabricate a specific target. `占 C 盘` and `可卸载` are honest candidate sets.
- `可卸载` means an official uninstall entry can be reviewed; it does not mean uninstalling is recommended or approved.
- Filter-specific completion copy matters because the same navigation mechanism carries three different user intentions.
- Repeating a documented shell mistake is avoidable waste; the error ledger must be consulted as an operational checklist, not only updated afterward.

## 2026-07-18 - Agent next-step typed application handoff

- A button label is presentation, not a reliable routing contract. The typed action object should carry the context that produced it.
- Stable automation identity must distinguish semantic destinations, not just top-level pages; two Apps buttons can represent different user intentions.
- Shared source-method extraction reduced maintenance noise when a handler changed from sync to async and prevented unrelated adjacent methods from satisfying security assertions.
- Real-window evidence remains separate from source evidence. A failed launch does not invalidate the implementation, but it does prevent claiming a visual Pass.

## 2026-07-18 - Home migration-closure catalog handoff

- Historical evidence may be useful even when identity attribution is no longer safe. The fallback should narrow investigation while staying explicit that it is not an exact match.
- Exact-target and aggregate-filter context should be mutually exclusive; rejecting mixed context prevents future routing ambiguity.
- Reusing one bounded catalog handoff keeps inventory loading, stale-search clearing, beginner copy, and mutation absence consistent across three Agent surfaces.

## 2026-07-18 - C-drive application handoff truth

- Empty-state truth must be measured after applying the user's filter. A nonempty global inventory says nothing about whether the current candidate view has results.
- Once a handoff has a strong typed boundary and honest status model, other entry points should delegate to it instead of reimplementing the sequence.
- Removing duplicated navigation code reduced both a real UX bug and the surface area that security tests need to audit.

## 2026-07-18 - Source integrity gate promotion

- A prevention rule that is repeatedly violated should become tooling, not another paragraph in a log.
- Process-scoped execution-policy bypass is appropriate for a reviewed repository script; changing the user's machine policy is not.
- The current helper removes the largest duplicated loop. A future cross-project version should also accept fixed-string symbol-count assertions with expected ranges.

## 2026-07-18 - Reproducible portable test package

- Packaging is part of the safety model. A folder of binaries is not a test release until its worker, rules, runtime requirement, hashes, signer relationship, and allowed test boundary are explicit.
- A successful default path does not prove script compatibility. Caller-provided paths and manifest enumeration exercised branches that exposed two host-runtime API gaps.
- Refusing old output and leaving failed partial output visible is less convenient, but it prevents a packaging retry from silently erasing evidence or unrelated files.
- `NotSigned` is useful evidence when the product reacts honestly to it. The manifest now turns an informal caveat into a machine-readable fail-closed release state.

## 2026-07-18 - Release debug-command surface

- Test-only code with no destructive authority can still be an unnecessary privileged process entry. Release scope should be judged by reachability and attack surface, not only by what the handler eventually does.
- Source guards and artifact checks protect different failure modes. The condition expresses intent; the byte scan catches build-item drift and proves the actual package.
- Preserving Debug smokes while removing Release source is a useful compromise: safety verification remains easy without asking production users to carry the harness.
- Duplicate worker build edges make parallel verification flaky. Until project topology is simplified, deterministic single-node verification is cheaper than retrying unexplained compiler locks.

## 2026-07-18 - Home empty state and real Release navigation

- A source-complete control can still look broken when empty. The real screenshot found a large blank rectangle that static binding tests had no reason to reject.
- Initial, loading, valid-empty, and populated are separate product states. Reusing one empty ListBox for all four made the interface ambiguous.
- The user's original complaints are now directly observable in one Release run: system drive selection is automatic, navigation changes pages, application profiles appear as icon tiles and human summaries, and Agent guidance is tied to current inventory.
- Computer Use correctly invalidated stale coordinates after user input. Refreshing the exact returned window before acting preserved both safety and reliable visual evidence.
## 2026-07-18 - Agent page information hierarchy

- Native tabs solved the actual beginner problem without creating more navigation or changing any execution authority: recommendations are primary, capability inventory is optional.
- The first screenshot mattered as much as the static tests. The code was functionally correct but the fixed-width card made the page visibly unfinished; one short visual loop caught and corrected it.
- AutomationIds on the TabControl and both TabItems made the content split provable in the real UI tree, including absence of capability content from the default selected tab.
- Keep the next audits grounded in one visible user question at a time, and continue separating read-only evidence acceptance from signed destructive-operation acceptance.

## 2026-07-18 - C-drive first-view hierarchy

- Empty result containers are not neutral for beginners; they look like failed scans. A truthful sentence and one clear next action carry more information than a large disabled surface.
- Visibility must derive from the same presenter collections that populate the UI. This avoids both stale boxes and false “nothing found” claims.
- The final screenshot caught a generic quarantine sentence that was technically safe but contextually premature. Safety details are easier to understand after a real recommendation exists.
- Real scan acceptance and real cleanup acceptance remain separate: this slice proved first-view truth without reading the user's drive or creating mutation authority.

## 2026-07-18 - Installation Control first-view hierarchy

- A placeholder row inside a list is semantically different from an empty state: it becomes selectable UI and makes a beginner believe there is an object to manage.
- Disabled controls are not always helpful previews. Showing them before their evidence exists added apparent steps without teaching the primary workflow.
- The corrected first view makes analysis and execution easier to distinguish because only selection and read-only Agent analysis are presented up front.
- Test assertions can lie by omission when fluent calls are guarded by null-conditionals; test evidence needs the same fail-closed posture as product safety evidence.

## 2026-07-18 - Undo Center first-view hierarchy

- A synthetic status row is especially misleading in an operation history because it inherits selection, technical-detail, and restore affordances even when no operation exists.
- Empty quarantine and empty history are independent conclusions. Keeping policy text while collapsing candidate controls preserves useful safety context without inventing work for the user.
- Real UI evidence again found the product problem faster than source completeness checks: every control was wired correctly, but together they made an empty state look complex and broken.
- Updated antivirus definitions reduce local false-positive friction, but they do not replace Authenticode, same-signer, confirmation, or disposable-machine mutation acceptance.

## 2026-07-18 - Migration plan decision hierarchy

- The application drawer had already achieved the intended product shape; the deeper migration modal had not. Auditing one full user path matters more than judging the entry page alone.
- Safety evidence can remain complete without being the default reading task. Folding it made the safe boundary clearer because the user first sees why execution is unavailable and what OMNIX will do next.
- A disabled destructive-action button in an unsigned build still implies a product promise. Hiding it until the package can genuinely prepare execution is more truthful than leaving it as a gray roadmap.
- Computer Use approval can time out independently of the target app. The lightweight retry recovered; no alternate UI automation or security bypass was needed.

## 2026-07-19 - Uninstall decision hierarchy

- A safe workflow can still ask the user to do unnecessary work. In an unsigned build, recovery-preparation controls were technically disabled downstream but visually implied that selecting an installer was the current task.
- Undo semantics need two separate sentences: official software uninstall usually requires reinstalling, while only later low-risk residue quarantine belongs to the Undo Center.
- The existing residue refusal was stronger than expected once exercised end to end: current inventory remained authoritative, the software was still detected, and no cleanup affordance appeared.
- Safety wording such as `不会直接运行` is part of product behavior, not prose decoration; the regression contract correctly protected the distinction between preview and later confirmed execution.
## 2026-07-19 - Disposable Windows behavioral acceptance protocol

- Signing, transfer verification, and behavioral acceptance are three different claims. Keeping them separate made the final release state more truthful and easier to audit.
- Evidence hashes prove that the reviewed files did not change; they do not prove the screenshot's meaning. The protocol therefore retains explicit human observation, notes, timestamps, and environment attestation instead of pretending automation can prove disposability or UAC behavior.
- Cancellation deserves first-class acceptance coverage. A beginner-facing safety product must prove that UAC cancellation does not become a false success, residue scan, migration closure, or automatic retry.
- Catching Windows PowerShell Boolean transport during review avoided a protocol that looked correct in source but failed before reaching its safety boundary on the target host.

## 2026-07-19 - Deterministic disposable acceptance fixtures

- A checklist becomes materially safer when every test object has a derived identity, collision preflight, ownership marker, and exact reset boundary. Operator-created paths would have weakened both repeatability and deletion safety.
- Fixture-local correctness was insufficient. Real inventory, startup attribution, uninstall trust, scan rules, and recommendation construction exposed two layouts that looked plausible but could not exercise the actual product workflow.
- Compensation must begin before the first mutation, not after an ownership marker succeeds. Fault injection at every boundary is the practical way to prove that promise.
- Keeping the harness outside the product package lets acceptance become more realistic without creating a hidden production command surface.
- The fixture package and product candidate are separate evidence objects. Binding both hashes into one session makes substitution detectable while preserving independent signing and packaging rules.

## 2026-07-22 - Read-only signing prerequisite inspection

- A release blocker is more useful when it is a reproducible report, but discovery still must not imply authorization. Listing candidates and selecting one belong to separate steps.
- Security predicates need one implementation model across readiness and execution. If the inspector and signer interpret EKU differently, a green readiness report is misleading.
- Static source contracts did not catch Windows PowerShell provider behavior or empty-pipeline semantics. One small real-host JSON test found both before the script became operational guidance.
- The current result is now actionable and honest: the store works, the localhost certificate is not a code-signing certificate, and the SDK signing tool is absent.

## 2026-07-22 - External release boundary

- Persistence does not mean inventing local work after the product, safety pipeline, package transform, verifier, fixture, and acceptance protocol are ready. At that point, unrelated changes reduce confidence instead of moving the actual objective.
- A self-signed certificate or primary-machine mutation run would make the final claim less true. Blocking with exact machine-readable prerequisites is the correct engineering outcome until the external state changes.
- Resumption is deterministic: rerun the inspector, create the signed candidate with an explicitly approved thumbprint, independently verify transfer, then complete the ten evidence-bound cases on a resettable Windows environment.
