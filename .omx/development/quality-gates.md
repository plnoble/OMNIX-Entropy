# Quality Gates

## 2026-08-02 - Beginner symptom-triage completion gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Goal alignment | Pass | Six beginner symptom choices now lead to conclusions instead of immediately handing the user a technical Windows page. | Rules do not diagnose unsupported root causes. |
| Evidence honesty | Pass | Every triage states checked observations, unknown evidence, urgency, and exactly one next step; negation and clause-local routing have focused contracts. | The first slice uses user-reported symptoms rather than active network/audio/device probes. |
| Execution authority | Pass | Presenter and quick-choice handlers have no operation, registry, service, process, installer, or filesystem-mutation authority; destinations resolve through existing exact allowlists. | Opening a tool still requires the existing protected confirmation and user cancellation remains possible. |
| Frontend and accessibility | Pass | Stable AutomationIds, source order, work-area geometry, scrolling, screenshot pixel check, and direct visual review prove all choices and the conclusion are usable. | Smaller unsupported windows can require scrolling after the conclusion grows. |
| Testing and verification | Pass | Focused 81/81; full Debug 1181/1181; Release 0 warnings/errors; integrity 425/19; both scripts parse; both real smokes pass with no external tool or operation. | No real system fault was induced. |
| Delivery | N/A | No version, commit, push, signing, installer, installed-product, or Release action. | Unified release remains after original Phase 10 closure. |

## 2026-08-02 - Phase 8 Agent capability audit pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Goal alignment | Pass | Phase 8 targets the approved Marvis-inspired diagnosis/decision role for beginners after operation primitives are safety-bounded. | Exact highest-value gap is pending repository audit. |
| Execution authority | Pass | New diagnosis must remain read-only or hand off to an existing typed plan and confirmation; arbitrary scripts/registry/service/process actions are excluded. | Each current Agent route needs source-level authority tracing. |
| Evidence honesty | Warn | Existing health, runtime, crash, hardware, and history models distinguish unavailable states. | Symptom routing may combine evidence with different freshness/completeness. |
| Frontend and accessibility | Warn | Existing Agent page and navigation have established UIAutomation patterns. | Current density, scrolling, first-view conclusion, and empty/error states require audit. |
| Testing and verification | Warn | Phase 7 baseline is full Debug 1155/1155, Release clean, integrity 423/19, and real WPF proof. | Phase 8 red contracts and GUI scenario are not yet defined. |
| Delivery | N/A | Unified publication remains Phase 10; public 0.1.5 is unchanged. | Combined worktree remains intentionally uncommitted. |

## 2026-08-02 - Phase 7 exact-file closure completion gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Security and privacy | Pass | Exact profile/pack/file bindings, no beginner path leakage, no direct core mutation authority, and no real user files processed. | User-imported rule quality remains external evidence. |
| Data and consistency | Pass | Pack bytes are reloaded/hash-verified; profile and full file set are rescanned before and after confirmation; batches are all-or-nothing and capped at 64. | Files/processes can still race, so final identity revalidation stops rather than adapts. |
| Destructive-operation safety | Pass | Only low-risk unconfirmed descriptors reach identity preparation; execution uses the existing pipeline, compensated quarantine, timeline, and restore. | GUI acceptance cancels; isolated integration proves actual mutation/restore. |
| Frontend and accessibility | Pass | Stable IDs, button `BringIntoView`, strict work-area geometry, nonblank pixel checks, and direct review show a concise preview and exact-file confirmation. | Technical path list remains intentionally behind the confirmation detail expander. |
| Testing and verification | Pass | Related 51/51; full Debug 1155/1155; Release 0 warnings/errors; integrity 423/19; diff/authority/PowerShell gates pass. | No real third-party pack was imported or executed. |
| Operations and release | N/A | No version, commit, push, signing, installer, installed-product, or Release action. | Unified release remains Phase 10. |

## 2026-08-02 - Phase 7 community exact-file preview pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Goal alignment | Pass | Phase 7 advances only Phase 6 eligible exact files into a user-confirmed reversible preview. | UI wording and exact action shape require red contracts. |
| Execution boundary | Warn | Existing `SafetyOperationPipeline`, confirmation, quarantine, timeline, and restore infrastructure is identified. | Community evidence has not yet been integrated; direct UI/quarantine calls must remain forbidden. |
| Evidence freshness | Warn | Planned bindings include active pack SHA, exact profile identity, inactive process state, current file identity, and complete candidate assessment. | Revalidation points and stale-result behavior need implementation tests. |
| Reversibility | Warn | Existing quarantine policy supports identity binding and compensated batch behavior. | New operation kind must be explicitly allowlisted without widening other kinds. |
| Frontend and accessibility | Warn | Existing app drawer provides the relevant beginner action surface. | Preview, refusal, button state, AutomationIds, first-view placement, and real screenshot are pending. |
| Delivery | N/A | No version, commit, push, signing, installer, installed-product, or GitHub Release action; unified release remains Phase 10. | Public 0.1.5 remains unchanged. |

## 2026-08-02 - Phase 6 candidate-promotion completion gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Security and privacy | Pass | Candidate policy accepts only conservative exact files under approved Local/Roaming cache roots; ordinary UI omits exact candidate paths. | Third-party rules remain untrusted evidence. |
| Data and consistency | Pass | Exact candidate retention is all-or-nothing; incomplete/capped/missing evidence cannot become eligible. | Assessment becomes stale after pack/profile/process/file changes. |
| Destructive-operation safety | Pass | Static authority checks report zero operation, quarantine, registry, process, or file-mutation authority; UI action remains disabled. | Phase 7 will introduce the first operation-planning boundary. |
| Frontend and accessibility | Pass | Two real WPF smokes show the decision/refusal reason in the first working area with stable automation and no operation. | Eligible-state screenshot awaits Phase 7 fixture. |
| Testing and verification | Pass | Focused 27/27; full Debug 1146/1146; Release 0 warnings/errors; integrity 420/19; PowerShell and leak/authority checks pass. | No real third-party pack was imported or downloaded. |
| Operations and release | N/A | No version, commit, push, package, signing, installer, or release action occurred. | Unified release is intentionally deferred to Phase 10. |

## 2026-08-02 - Phase 6 candidate-promotion pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Goal alignment | Pass | Phase 6 turns broad community observations into explicit preview-only/refused/eligible decisions before any operation planning. | Exact policy interfaces and user copy still need red contracts. |
| Execution boundary | Pass | Design is pure classification and cannot create `OperationDescriptor`, mutate files, or call quarantine. | Static authority tests are pending. |
| Path/ownership safety | Warn | Planned policy rejects protected/system/install roots, escaped/reparse evidence, broad directory intent, and missing exact file identity. | Existing path policies must be traced and reused rather than duplicated inconsistently. |
| Live-use/evidence honesty | Warn | Planned policy rejects active ownership, warnings, young files, and incomplete/lower-bound evidence. | Process association and evidence-completeness inputs need explicit contracts. |
| Exclusions | Pass | Phase 5 ignore preferences are applied before enrichment; Phase 6 will also model global protected-path exclusions. | User-facing distinction between hidden and safety-refused items still needs presentation tests. |
| Delivery | N/A | No version, commit, push, signing, installer, installed-product, or GitHub Release action; unified release remains Phase 10. | Public 0.1.5 remains unchanged. |

## 2026-08-02 - Phase 5 rule-center completion gate

- Pass - Product fit: one secondary application-page modal contains status, preview, import/update, rollback, and exclusions without adding primary navigation density.
- Pass - Consent/supply chain: exact source/license/version/SHA-256 plus two confirmations are required before local activation or manual HTTPS transport; no discovery/background updater or bundled third-party data exists.
- Pass - Execution boundary: rule findings cannot create operations, delete files, modify registry, or enable cache cleanup; ignore/restore touches only OMNIX-managed preferences.
- Pass - Evidence/UX: conservative largest-observation summary, no overlap sum, paths/hash behind technical detail, stable AutomationIds, and explicit accessible list-item names.
- Pass - Regression/build: focused 10/10; full Debug 1136/1136; Release 0 warnings/errors.
- Pass - Integrity/authority: 418 source files valid UTF-8, 0 replacements, 19/19 XAML; diff check has no errors; authority, resolver mutation, bundled data, and PowerShell parse counts are zero.
- Pass - GUI: isolated real WPF smoke proves provenance/safety first view, ranked preview, ignore/restore round-trip, screenshots, and `OperationExecuted=false`.
- N/A - Delivery: no version, commit, push, signing, installer, installed-product, or GitHub Release action; unified release remains Phase 10.

## 2026-08-01 - Phase 5 rule-center pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Goal alignment | Pass | Closes the missing user-facing source/license/import/rollback and preview controls from Phases 3-4. | Detailed workflow and modal capacity still need red contracts. |
| Consent/supply chain | Pass | Design reuses exact descriptor-bound consent, HTTPS-only pinned transport, strict parse/hash, immutable activation, and stale-safe rollback. | File picker and URL input must not trigger access before final confirmation. |
| Execution boundary | Pass | Rule Lab is preview/exclusion only and cannot create an operation or enable rule-only cache cleanup. | Static/runtime authority contracts are pending. |
| Beginner UX | Pass | One secondary application-page entry keeps primary navigation quiet; ordinary view shows source/license/version and conclusions, paths remain technical. | Real modal screenshot and keyboard/scroll acceptance are pending. |
| Cancellation/data integrity | Pass | Planned preview cancellation is read-only; activation/update cancellation already leaves active state intact. | WPF orchestration and stale UI receipt tests are pending. |

## 2026-08-01 - Phase 4 profile/Agent integration completion gate

- Pass - Evidence boundary: community observations use a separate model and never populate `CachePaths`/`CacheSizeBytes`; rule-only cleanup remains disabled.
- Pass - Evidence honesty: source/version/hash/warning/lower-bound/sample details survive enrichment; beginner aggregate uses the largest overlapping observation and says `至少`/`可能重叠`.
- Pass - Privacy/UX: ordinary tile/drawer/Agent copy contains no path or hash; technical detail is bounded; Agent appears first and single-entry family text collapses.
- Pass - Runtime failure: absent or invalid optional stores preserve the ordinary software inventory and report a plain-language summary.
- Pass - Regression/build: full Debug 1125/1125; Release 0 warnings/errors.
- Pass - Integrity/authority: 411 source files valid UTF-8, 0 replacements, 18/18 XAML; diff check passes; new evidence/enricher authority search reports `AuthorityHits=0`.
- Pass - GUI: isolated WPF smoke validates tile, Agent, summary, disabled cleanup, real screen working-area geometry, screenshot, and `OperationExecuted=false`.
- N/A - Delivery: no version, commit, push, signing, installer, installed-product, or GitHub Release action; unified release remains Phase 10.

## 2026-08-01 - Phase 4 profile/Agent integration pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Goal alignment | Pass | Converts FluentCleaner-inspired coverage into the beginner application experience requested by the user. | Import/consent UI remains separate from evidence presentation. |
| Execution boundary | Pass | Design adds a separate evidence list and explicitly does not modify `CachePaths` or create `OperationDescriptor`. | Contracts and real UI action state are pending. |
| Privacy/UX | Pass | Beginner summary omits paths; bounded samples appear only after technical-detail expansion. | WPF order/screenshot proof pending. |
| Performance | Pass | Design bounds profiles, rules per profile, total resolutions, per-rule duration, and total duration with cancellation/progress. | Real-machine timing is not yet measured. |
| Data integrity | Pass | A central `SoftwareProfile` copy constructor will replace one ad hoc clone and preserve new evidence. | Other future clone sites must adopt it deliberately. |
| Size honesty | Pass | Presentation uses the maximum observed rule size and explicit lower-bound wording, avoiding overlap sums. | Multiple disjoint rules may understate total, intentionally. |

## 2026-08-01 - Phase 3 rule-pack management completion gate

- Pass - Consent: source URI, license URI, version, and SHA-256 must match explicit source/license activation confirmation before local creation or network access.
- Pass - Supply chain: HTTPS-only manual transport, bounded content, exact hash, strict UTF-8/full parse, no discovery/background loop, and no bundled third-party bytes.
- Pass - Atomicity/rollback: immutable SHA-256 files plus atomic bounded state replacement; one prior descriptor; stale expected-hash rollback refusal; corrupt/missing content fails closed.
- Pass - Cancellation: an in-progress copy cancellation removes staging bytes and leaves the known-good state file byte-identical.
- Pass - Regression: full Debug passed 1119/1119; Release completed with 0 warnings/errors.
- Pass - Integrity: 407 source files valid UTF-8, 0 replacement files, and 18/18 XAML parsed.
- Pass - Authority: expected-zero search found no maintenance operation, registry, process-launch, directory-delete, periodic-timer, or delayed-background authority in the Winapp2 surfaces.
- N/A - GUI: source/license/import/update/rollback UI is intentionally deferred to the Phase 4 beginner presentation surface.
- N/A - Release: unified publication remains Phase 10 after all planned capabilities and acceptance gates.

## 2026-08-01 - Phase 3 rule-pack store pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Goal alignment | Pass | Optional verified rule updates are the next Phase 1-10 capability and directly support FluentCleaner-inspired coverage. | Beginner UI arrives in Phase 4. |
| Licensing | Pass | Design stores only user-imported/runtime bytes and preserves source/license metadata; no CC BY-SA data enters source or installer. | Redistribution remains prohibited until separately reviewed. |
| Activation safety | Pass | Design validates hash and full parse before atomically changing an immutable-content pointer. | Filesystem implementation and failure tests are pending. |
| Consent | Pass | Source URI, license URI, version, and expected hash must match the reviewed descriptor. | UI receipt is pending Phase 4. |
| Rollback | Pass | Previous descriptor remains in state and rollback uses expected hashes to reject stale confirmation. | Corruption/cancellation contracts are pending. |
| Execution boundary | Pass | Rule-pack management changes only OMNIX-owned data and cannot create maintenance operations. | Static authority tests must be updated to distinguish data persistence from cleanup authority. |

## 2026-08-01 - Phase 2 bounded Winapp2 resolver completion gate

- Pass - Objective and boundaries: read-only evidence resolution only; no operation or third-party data activation. Evidence: `current.md` and source authority contracts.
- Pass - Path safety: canonical deepest-root ownership, direct-child containment, volume-root rejection, reparse skipping, and unrelated-branch pruning. Evidence: `Winapp2EvidenceResolverTests`.
- Pass - Resource safety: target/root/directory/file/match/sample/time limits, cancellation, progress, and lower-bound reasons. Evidence: focused 11/11.
- Pass - Regression: `dotnet test ComputerSecuritySoftware.slnx --configuration Debug --no-restore -p:NuGetAudit=false` passed 1111/1111.
- Pass - Build: Release completed with 0 warnings and 0 errors.
- Pass - Integrity: 403 source files valid UTF-8, 0 replacement files, and 18/18 XAML parsed.
- Pass - Authority boundary: expected-zero search for mutation, network, process, registry, and `OperationDescriptor` references returned `AuthorityHits=0`.
- N/A - GUI acceptance: this phase adds no UI or user-visible workflow.
- N/A - Release: the active goal requires one unified release after all phases.

## 2026-08-01 - Phase 2 bounded evidence pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Goal alignment | Pass | Phase 2 directly advances the requested Phase 1-10 plan from compatible rules to useful cache evidence. | User-visible value arrives after Phase 4 presentation. |
| Read-only authority | Pass | Design exposes inventory evidence only and keeps filesystem access in `Css.Scanner`; no operation, mutation, registry, process, or network surface is planned. | Static and runtime contracts must prove the boundary. |
| Traversal safety | Warn | Planned deepest-owner selection, canonical containment, reparse skipping, and hard limits cover broad traversal risk. | Fake and real fixtures must prove escape/reparse behavior. |
| Evidence honesty | Warn | Planned result distinguishes exact totals from access/limit lower bounds and reports exclusions, samples, progress, and cancellation. | Implementation and contracts are pending. |
| Privacy | Pass | Result keeps bounded sample paths rather than every matched path. | Technical-detail UI must remain secondary in Phase 5. |
| Delivery | N/A | Unified version and release remain deferred until all phases and the full completion audit pass. | Public 0.1.5 remains unchanged. |

## 2026-08-01 - Read-only Winapp2 catalog completion gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User authorized development; implementation is limited to rules, pure attribution, tests, and records. | No user-visible integration is included. |
| Third-party boundary | Pass | Production implementation is independently written; no FluentCleaner code or Winapp2 data is bundled. Descriptor requires source, version, license, license URI, source URI, and expected SHA-256. | OMNIX still needs an explicit license decision before distributing a rule pack. |
| Input trust | Pass | Six contracts cover SHA mismatch, missing metadata, malformed/empty/invalid-UTF-8/oversized/over-target/duplicate input and preserve evidence. | Future update/download storage needs atomic replacement and rollback contracts. |
| Attribution safety | Pass | Path matcher requires equal/descendant ownership, accepts only literal-anchored wildcard segments, rejects name-only, broad-parent, sibling, and wildcard-only authority. | Path relationship is not proof of safe deletion or current ownership. |
| Execution boundary | Pass | Models report `IsExecutionAuthorized=false`; source scan reports 0 operation, delete, registry API, process, or HTTP authority hits. | A later resolver/presenter must retain the same boundary. |
| Real compatibility | Pass | Disposable local probe parsed Winapp2 260730 as 3,721 rules, 11,110 file targets, 3,368 registry targets, 17 warnings, 0 unknown-key diagnostics, execution false. | Probe data remains outside the product and is not a committed test dependency. |
| Tests and build | Pass | Focused 6/6; full Debug 1106/1106; Release 0 warnings/errors; integrity 399 files/18 XAML; changed-file whitespace and diff checks pass. | Existing unrelated test files do not satisfy a repository-wide whitespace-format gate; no GUI or installed-runtime acceptance is applicable to this backend-only slice. |
| Delivery | N/A | Worktree is uncommitted; version remains 0.1.5 and no package, push, signer, installer, or Release action ran. | Installed/public 0.1.5 does not contain this work. |

## 2026-08-01 - Read-only Winapp2 catalog pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User said `开始开发` immediately after accepting the FluentCleaner assessment. Slice is limited to parser/catalog/evidence tests. | UI, network updates, executable cleanup, versioning, and release are not included. |
| Third-party boundary | Pass | Implementation will be independently written from the public format behavior; no FluentCleaner source or Winapp2 database is copied or bundled. | Compatible behavior still needs clear source/license metadata for any future imported pack. |
| Safety authority | Pass | Design returns read-only evidence and path attribution only; no `OperationDescriptor`, filesystem mutation, registry mutation, process launch, or cleanup handler is in scope. | A later integration must preserve this boundary explicitly. |
| Input trust | Warn | Planned metadata pins source/version/license/hash and parser limits size, lines, entries, and targets. | Contracts must prove malformed, oversized, and hash-mismatched inputs fail closed. |
| Attribution | Warn | Planned matching requires canonical path overlap with a `SoftwareProfile` path; name similarity is not sufficient. | Wildcards and environment-variable expansion can still be ambiguous and must remain review evidence. |
| Delivery | N/A | No version, package, commit, push, installer, or Release is requested. | User-visible benefit requires a later presentation/integration slice. |

## 2026-08-01 - FluentCleaner reference-analysis gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User requested source download, code/function review, and optimization assessment. Review clone stayed under `C:\tmp`; no product implementation or machine maintenance ran. | Implementation still requires a separately accepted scope. |
| Source grounding | Pass | Analysis pinned FluentCleaner commit `be347511ae8c639bdc9ca1cfc38dec02fe92c7c5` and inspected parser, engine, UI, Rule Lab, updates, automation, AI/custom scripts, CI, and issue evidence. | FluentCleaner evolves quickly; a future implementation must pin its own reviewed source/rule versions. |
| License boundary | Warn | FluentCleaner source declares MIT; upstream Winapp2 data declares CC BY-SA 4.0; OMNIX has no repository license file. | Bundling or transforming the database is deferred until attribution/share-alike and OMNIX licensing are decided. |
| Safety boundary | Pass | Recommendation keeps third-party rules read-only and rejects direct deletion, registry cleaning, custom script execution, unattended cleanup, and unsigned live updates. | Imported rules can still be wrong; policy, ownership, path, process, confirmation, quarantine, and rollback checks remain mandatory. |
| Product fit | Pass | Proposed value is broader app-cache diagnosis, impact ordering, progress/cancel, exclusions, and technical dry-run detail while retaining beginner conclusions. | Exact coverage and performance need fixtures and a bounded prototype. |
| Delivery | Pass | Only protocol records changed; product source, version 0.1.5, installer, installed machine, certificates, and GitHub Release are unchanged. | No user-visible improvement exists until a later implementation is authorized and released. |

## 2026-08-01 - 0.1.5 release completion gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User authorized review/version/publication and separately approved host-context read-only verification, use of the existing non-exportable signer, and GitHub Release publication. | Installer execution, product operations, certificate export/creation, trust mutation, antivirus changes, and `LocalMachine` remained unauthorized. |
| Working-tree ownership | Pass | Product source was committed as `234bcd8`; the only remaining changes before this gate are required release records. | Final records commit intentionally follows the immutable Release target. |
| Safety boundary | Pass | Review added no family-level uninstall or cleanup authority; both real WPF smokes executed no operation. Release work did not run the setup or any product maintenance operation. | Disposable-machine behavioral acceptance remains absent. |
| Version and channel | Pass | Source/release notes report 0.1.5; public `releases/latest` reports stable `v0.1.5`, target `234bcd8`, four assets, non-draft and non-prerelease. | Installed clients still require a visible user-confirmed update. |
| Tests and build | Pass | Local full Debug 1100/1100; Release 0 warnings/errors; integrity 395 files/18 XAML; GUI smokes pass. GitHub CI run `30528588089` passed on exact target `234bcd8`. | No disposable installed-runtime acceptance. |
| Signed payload | Pass | Host independent verifier checked 110 target-bound payload files with signer `5688958FEA0056861558E8DCF9D2381AF46074B2`; App/worker signatures and GlobalSign timestamps were valid. | Personal signing does not provide public SmartScreen reputation. |
| Installer policy | Pass | Independent verifier confirmed 0.1.5, default `D:\Software\OMNIX-Entropy\Install`, visible directory choice, no silent install, expected signer/timestamp, and `CanStageGitHubRelease=true`. | Installer behavior was not executed. |
| Download-back | Pass | All four draft assets matched local lengths and SHA-256 values; the downloaded setup independently passed the installer verifier. | GitHub/CDN availability remains external. |
| Delivery | Pass | Public Release `v0.1.5` is Latest and the public update manifest returned HTTP 200 with the exact commit, setup length/hash, and signer. | The first installed 0.1.4-to-0.1.5 update receipt depends on the user's visible confirmation. |

## 2026-07-30 - System cleanup and app-family completion gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User explicitly said “继续开发” after reviewing the proposed read-only classification/presentation slice. | No new execution, version, commit, or release authority. |
| Real evidence | Pass | Bounded host evidence found at least 4.59 GB current-user temp plus Antigravity/OpenCode split program/updater/data locations. | Directory totals can be lower bounds or overlap; active-use intent remains unknown. |
| System cleanup safety | Pass | `KnownCleanupStoreProbe` is bounded, cancellable, skips reparse points, reports lower bounds, and produces evidence only. User-review and Windows-managed findings have distinct copy and no new cleanup operation. | Per-file age/lock/ownership and actual Windows cleanup routing remain future execution prerequisites. |
| Application identity safety | Pass | Family grouping is presentation-only; exact `SoftwareProfile` remains the operation target. The isolated OpenCode smoke exposed one enabled official-uninstall entry and two disabled non-registered entries. | Name normalization can group intentional parallel versions, so family-level execution must remain prohibited. |
| Beginner UX | Pass | `.omx/qa-app-family-decision.png` shows family count, exact selected version/source, uninstallability, migration outcome, C remainder, and future growth in the first drawer view. Home/C-drive screenshots separate 4 KB immediately safe cleanup from at least 3.3 GB of candidates. | Application-specific support for moving cache/data still requires per-product evidence; the drawer says so rather than promising redirection. |
| Operation safety | Pass | Both real WPF smokes report no operation executed; added production source contains no delete/registry/process authority, and existing official-uninstall/confirmation/quarantine/migration pipelines are unchanged. | Tests delete only isolated temporary fixtures during cleanup. |
| Tests and build | Pass | Focused 12/12; full Debug 1099/1099; Release 0 warnings/errors; source integrity 395 files/18 XAML; both PowerShell scripts parse. | No installed/package acceptance or real cleanup/uninstall/migration ran. |
| Delivery scope | Pass | Product version remains 0.1.4 and the worktree is uncommitted; no push, tag, signing, installer, or Release action occurred. | Installed/public 0.1.4 does not contain this work. |

## 2026-07-30 - Impact-aware C-drive priority pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User explicitly challenged the usefulness of a 337.6 MB primary action against the whole C-drive shortage. | This slice changes read-only guidance only; no release is included. |
| Quantitative honesty | Pass | Latest evidence requires about 19.5 GB to reach the 80% target; 337.6 MB contributes about 1.7%. | Observation sizes identify where to investigate, not guaranteed reclaimable bytes. |
| Safety boundary | Pass | Existing low-risk/reversible/operation eligibility, confirmation, quarantine, and `OperationPipeline` remain unchanged. | The larger-source route must remain read-only until ownership and action evidence exists. |
| Beginner UX | Pass | Real screenshots show a 19.5 GB target, 0.0% fixture contribution, impact-first primary button, and clearly optional safe-cleanup button without overlap. | Observation sizes are investigation clues, not promised recoverable amounts. |
| Tests and build | Pass | Focused 12/12; full Debug 1087/1087; Release 0 warnings/errors; integrity 390 files/18 XAML; PowerShell parse and diff check pass; GUI smoke reports `noOperationExecuted=true`. | No real user-file cleanup or installed-build acceptance ran. |

## 2026-07-30 - Actionable C-drive conclusion pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User reports that a completed C-drive scan appears to offer nothing adjustable; the change is limited to making existing evidence lead to understandable next steps. | No new cleanup authority or release is included. |
| Real evidence | Pass | Latest local digest: C 86.5%, score 64, reclaimable 337.6 MB, low-risk count 0, five observation findings, 105 ordinary C-drive app clues. | The digest is sanitized and intentionally does not persist the full in-memory recommendation graph. |
| Safety boundary | Pass | Existing low-risk/reversible/operation requirements, confirmation page, quarantine, and `OperationPipeline` remain unchanged. | Read-only navigation must not be confused with approval to delete unexpected roots. |
| Beginner UX | Pass | Real WPF screenshots show “不是没有可调整项”, safe-now/investigate-next counts, the remaining 80% gap, a primary safe-preview button, and a secondary larger-space button without overlap. | A small safe item can still be an insignificant contribution; the copy states the remaining gap rather than overstating it. |
| Tests and build | Pass | Focused 11/11; adjacent experience 226/226; full Debug 1086/1086; Release 0 warnings/errors; integrity 390 files/18 XAML; diff check clean; mutation-authority hits 0; GUI smoke `noOperationExecuted=true`. | No installed-build or real cleanup acceptance was run. |

## 2026-07-30 - 0.1.4 public release preflight

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User explicitly requested “推送发布” after the reviewed local 0.1.4 commit. | Installer execution, certificate/trust changes, private-key export, antivirus changes, and `LocalMachine` actions remain unauthorized. |
| Source readiness | Pass | Feature commit `337e037` and release-preparation target `68a7bd4ba9b73c9dd425da92baaf60c45450a81d` are pushed; GitHub CI `30517231962` passed Release build, full tests, and source integrity on the exact target. | The final records-only commit is newer than the immutable release target by design. |
| Signing prerequisites | Pass | Host read-only inspection reports one eligible CurrentUser RSA code-signing certificate with private key/EKU, thumbprint `5688958FEA0056861558E8DCF9D2381AF46074B2`, valid through 2029; SignTool and Inno Setup exist. | RFC3161 availability remains external; signer must be selected explicitly. |
| GitHub identity | Pass | Host `gh auth status` reports active `plnoble` login with `repo` and `workflow`; origin is fixed to `plnoble/OMNIX-Entropy`. | Network and GitHub availability remain external. |
| CI and source publication | Pass | `main` contains target `68a7bd4`; CI run `30517231962` completed successfully before signing began. | GitHub-hosted availability remains external. |
| Signed payload and installer | Pass | Fresh target-bound App/worker payload and Inno setup passed independent signature/timestamp/same-signer checks. Setup reports version 0.1.4, D-first managed directory, visible directory selection, no silent install, and `CanStageGitHubRelease=true`. | Personal signing does not create public SmartScreen reputation; disposable-machine behavioral acceptance is still absent. |
| Draft download-back and public latest | Pass | Four draft assets matched local SHA-256 values; the downloaded setup independently verified. Public `releases/latest` reports stable `v0.1.4`, target `68a7bd4`, and four uploaded assets. Setup SHA-256 is `9B32AB55D61637A14275D741D6E120F5D07E3FEC960DFF5E890BA445BA7AA48D`. | No installer was run; installed-update acceptance remains a user-visible follow-up. |

## 2026-07-30 - 0.1.4 review pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User explicitly requested review, commit, and a version bump. | Push, tag, installer creation, signing, release publication, and installed-product mutation remain outside this request. |
| Evidence consistency | Pass | Drive switch/start/cancel/failure paths clear plan steps and safety copy; data-drive scans do not enter the system-drive timeline; the history action reselects the system drive. Focused source/unit contracts and final screenshots cover the boundaries. | Per-drive history is intentionally deferred and would require a schema/UX design. |
| Beginner UX | Pass | Selected safe-card explanation remains specific; D-drive plans say “other disk”; history is labelled “系统盘体检历史”; screenshots show the first-view target and the D pending state without stale D results. | A real full D-drive scan was not run because its duration depends on user data; fixture and policy tests cover the behavior. |
| Operation safety | Pass | The combined slice remains read-only until the existing explicit confirmation and `OperationPipeline`; no review change may add direct execution. | Real cleanup execution is intentionally not part of this gate. |
| Version and delivery | Pass | `Css.App.csproj`, Debug `ProductVersion`, `FileVersion`, and `docs/release/0.1.4.zh-CN.md` report 0.1.4. | A local versioned commit is not a published or installable update; installed/public 0.1.3 remains unchanged. |
| Tests and build | Pass | Focused review 26/26; final full Debug 1085/1085; Release 0 warnings/errors; source integrity 390 files, InvalidUtf8 0, ReplacementFiles 0, XAML 18/18; both smoke parsers and real WPF smokes pass; diff check has no errors. | Multi-disk smoke required one script fix after two external UIAutomation RPC faults; the final process-scoped run passed. |

## 2026-07-29 - Beginner C-drive health-plan pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User explicitly said the current product still does not explain how to make C healthier. | No release/version change is included. |
| Evidence honesty | Pass | `DriveHealthPlanExperienceTests` covers full/safe-gap, no executable evidence, and already-comfortable states. Safe contribution requires low risk, reversible status, and an attached operation. | The 80% threshold is an explicit OMNIX comfort policy, not a Windows guarantee. |
| Beginner UX | Pass | `.omx/qa-home-agent-next-action.png` shows target, safe contribution, gap, and one button before detail panels; `.omx/qa-drive-health-plan.png` shows the selected preview and its next entry. | Long recommendation collections still require ordinary scrolling by design. |
| Operation safety | Pass | Runtime smoke selected the preferred low-risk card and exposed “查看并确认移动到隔离区” but never clicked it; JSON reports `noOperationExecuted=true`. | Destructive execution is intentionally outside this layout/decision slice. |
| Tests and build | Pass | Focused 13/13; full Debug 1084/1084; Release 0 warnings/errors; source integrity 390 files and 18/18 XAML; real WPF smoke and diff check pass. | `git diff --check` reports only CRLF-to-LF notices for two already edited source files. |

## 2026-07-29 - Homepage scrolling pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User reported that the homepage history and key-finding regions cannot scroll and asked for correction. | No release/version change is included. |
| Root-cause evidence | Pass | XAML inspection found unbounded `StackPanel` measurement; the first runtime iteration also proved that scrolling only the daily rows still clipped the complete history workflow. | None known for the reported two regions. |
| Beginner UX | Pass | `.omx/qa-home-agent-next-action.png` shows independent scrollbars; UIAutomation moved key findings 0% to 25% and history 0% to 100%, with the evidence action inside the history surface. | At very small unsupported window sizes, content density can still require more scrolling. |
| Interaction safety | Pass | The smoke generated a read-only plan, used its internal next action, confirmed `noOperationExecuted=true`, and removed the isolated fixture in `finally`. | No destructive operation path was exercised because this is a layout-only change. |
| Tests and build | Pass | Focused 10/10; full Debug 1080/1080; Release 0 warnings/errors; source integrity 388 files and 18/18 XAML; PowerShell parser and diff check pass. | `git diff --check` reports only CRLF-to-LF notices for two already edited source files. |

## 2026-07-29 - Multi-disk scan pre-change gate

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User explicitly asked to enable scanning drives other than C. | No release/version bump is included unless separately requested. |
| Read-only boundary | Pass | No handler or mutation authority was added. Data-drive folders cannot create cleanup operations, and a data folder named `Temp` gets no cleanup card action. | A future data-drive cleanup feature needs separate evidence and consent design. |
| Target safety | Pass | `DriveScanTargetPresenter` includes only ready `DriveType.Fixed` disks, orders the Windows disk first, and exposes no typed path input. Focused presenter tests pass. | Removable and network drives are intentionally unavailable in V1. |
| Evidence correctness | Pass | System big-rocks and unexpected-root policy run only on the Windows drive; data-drive large/duplicate analysis uses the selected root; summaries name the selected disk. Focused scan/product contracts pass 200/200. | A whole data-drive scan can be long, but remains cancellable and bounded by the existing crawler limits. |
| Beginner UX | Pass | Stable selector/item automation names; real WPF smoke listed C/D with free space, selected D, exposed `D 盘，剩余 256.2 GB`, and saved `.omx/qa-multidisk-selector.png`. | Screenshot covers the 1500x1000 desktop layout, not every DPI/display combination. |
| Stale-state control | Pass | Selection invalidates the read-only load gate, clears old conclusions, shows a selected-drive pending state, and is disabled during an active scan. Unit/source and real UIAutomation checks pass. | Selection is intentionally blocked until an active scan finishes or is cancelled. |
| Tests and build | Pass | Focused 200/200; full Debug 1079/1079; Release 0 warnings/errors; integrity 388 files/18 XAML; smoke parser and `git diff --check` pass. | No packaged installer or installed-version acceptance was run because this slice is not released. |

## 2026-07-29 - Managed install-layout closure

| Category | Status | Evidence | Residual risk |
| --- | --- | --- | --- |
| Scope and consent | Pass | User asked Codex to implement the source and release work; machine inspection was read-only and installer execution remains a user action. | 0.1.3 publication is pending. |
| Current installation | Pass | Host evidence: correct 0.1.2 exe/registry location, valid expected signer and GlobalSign timestamp, fixed D drive, non-reparse directories, no doubled executable. | A future release is still needed to exercise self-update. |
| Installer layout | Pass | `AppendDefaultDirName=no` plus focused source contract; doubled-layout test uses the real Windows path policy. | Real compiled 0.1.3 setup has not yet been inspected. |
| Failure classification | Pass | Internal typed layout exception; downloader returns exact reinstall guidance and performs zero HTTP requests for the malformed layout. | Current 0.1.2 binary still has the old generic copy; this lands in 0.1.3. |
| Security boundary | Pass | Exact managed layout, D-first fixed-drive, reparse, hash, signer, confirmation, and pipeline checks are not relaxed. | First real end-to-end update remains pending. |
| Tests and build | Pass | TDD red was specific; focused installer/update/release-page contracts pass 16/16; full Debug 1068/1068; Release build 0 warnings/errors; source integrity 385 files/18 XAML; diff check clean; source `d052ae1` passed GitHub CI `30424760666` on a bounded clean retry. | The first CI attempt's testhost aborted after 429 passing tests without a failed assertion; the clean retry passed the complete suite. |
| Records and handoff | Pass | Corrected machine state, local/CI/release evidence, and the remaining real-update acceptance step are recorded; 15 archive EOF whitespace findings fixed mechanically; ignoring pure blank-line changes leaves no archive diff against `fe2e012`. | The final records-only commit is newer than the immutable release source by design. |
| Release | Pass | Signed payload/setup verified; four draft assets downloaded back and matched; downloaded setup returned `CanStageGitHubRelease=true`; public latest reports non-draft/non-prerelease `v0.1.3`, four assets, source `d052ae1`, and setup SHA-256 `33C332A658B6B5CF08304AE67A0E2A7A5AAE3CC23A357E5407913E305F05C73A`. | The installer was not run; the first real 0.1.2-to-0.1.3 in-app update still requires the user's visible confirmation and runtime receipt. |

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

### 2026-08-02 - Phase 9 capability closure gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | Focused install/migration 225/225; cleanup read-only authority contracts; home smoke `noOperationExecuted=true` | No installer, uninstall, cleanup, migration, registry, or service mutation ran. |
| Data, API, and consistency | Pass | Mixed `0+7` probe/card/home regression; independent re-review | Per-location candidate totals no longer inflate recent files; duplicate-family actions remain exact-entry only. |
| Code quality and maintainability | Pass | Existing guarded workflows reused; `git diff --check` passed | No parallel mutation pipeline was introduced. |
| Testing and verification | Warn | Cleanup 194/194; install/migration 225/225; real home WPF pass; prior install/migration screenshots reviewed | Fresh install/migration GUI rerun was denied by exhausted platform host-approval quota and remains a final release gate. |
| Frontend, accessibility, and UX | Pass | Stable cleanup card AutomationId; untouched initial viewport smoke; nonblank screenshot | Mixed-policy copy explains that locations use different conditions. |
| Operations, dependencies, and release | Warn | Public/product version still 0.1.5; worktree uncommitted | Unified Phase 1-10 review, build, signing, and GitHub Release remain pending. |

### 2026-08-02 - Phase 10 Agent decision gate

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Security and privacy | Pass | `AgentDecisionContextBuilder` path-like text fallback; exact-family target tests; presenter authority contracts | Agent remains local, navigation-only, and cannot create or execute operations. |
| Data, API, and consistency | Pass | Five decision contracts; numeric target/gap assertions; snapshot/window caveats; non-additive source copy | Root clues, growth bytes, and safe reversible bytes remain separate quantities. |
| Code quality and maintainability | Pass | One Core context plus one Scanner adapter; existing App/drive/growth presenters reused | No duplicate scanner or mutation handler was added. |
| Testing and verification | Pass | Agent 54/54; related 235/235; final full Debug 1208/1208; Release build 0 warnings/errors; Release command-surface 2/2; integrity 427/19; diff/script/authority checks pass | Static and automated local gates pass. Full Release lifecycle tests are intentionally inapplicable because the fake worker is excluded from Release. |
| Frontend, accessibility, and UX | Pass | Stable quick-choice/response AutomationIds and order contracts; real isolated smoke passed five decisions and four directly inspected nonblank screenshots | No fixture path was visible; the first smoke's decorative-container lookup was replaced by full-window inspection. |
| Operations, dependencies, and release | Warn | Source is prepared for 0.2.0 while public latest remains 0.1.5; package, signing, CI and GitHub actions remain pending | Continue through the guarded personal release flow; do not run the installer. |

### 2026-08-08 - 0.2.0 unified release preflight

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and consent | Pass | Active user goal explicitly requires one unified Phase 1-10 release; GUI authorization remained isolated and non-operational | Installer execution, trust-store changes, certificate creation/export, antivirus changes, and system maintenance remain outside scope. |
| Security and privacy | Pass | Credential-pattern audit 0; Agent direct-authority contracts; GUI whole-window fixture-path check; `noOperationExecuted=true` | Community data is not bundled and Agent does not gain mutation authority. |
| Data, API, and consistency | Pass | Exact rule/app/file identity tests; non-additive storage evidence; stale-state refusals; product source version 0.2.0 | Public channel remains 0.1.5 until publication completes. |
| Code quality and maintainability | Pass | Existing pipeline/quarantine/timeline/update abstractions reused; no new privileged path; `git diff --check` passes | One Debug-only fake worker remains intentionally excluded from Release. |
| Testing and verification | Pass | Debug 1208/1208; Release build 0 warnings/errors; Release command-surface 2/2; integrity 427 files and 19/19 XAML | Release full lifecycle suite is intentionally inapplicable under the recorded project rule. |
| Frontend, accessibility, and UX | Pass | Stable AutomationIds, static placement contracts, real five-decision smoke, four directly inspected screenshots | First attempted smoke exposed and then fixed a decorative-container automation dependency. |
| Operations, dependencies, and release | Warn | `ProductVersion=0.2.0`, `FileVersion=0.2.0.0`, release notes present, credential audit clean | Commit, push, CI, signing, installer, draft download-back, public latest, and final record commit remain pending. |

### 2026-08-08 - 0.2.0 public release completion

| Category | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Scope and consent | Pass | Active user goal required unified publication; all privileged prompts were scoped to signing or read-only verification | No installer execution, trust change, certificate export, antivirus change, or maintenance operation. |
| Security and privacy | Pass | Credential audit zero; same-signer/timestamp verification; Agent whole-window privacy smoke; no third-party rule data bundled | Personal signing does not create public SmartScreen reputation. |
| Data, API, and consistency | Pass | Source/tag/channel target `349e7fc`; four draft assets byte-match; public manifest version/hash/length/signer match setup evidence | Public updater metadata is internally consistent. |
| Code quality and maintainability | Pass | Final source commit contains only text source/tests/records; existing guarded release scripts used | No release-only product bypass was added. |
| Testing and verification | Pass | Local Debug 1208/1208; Release 0 warnings/errors; command-surface 2/2; integrity 427/19; CI `31243516928` passed | Formal disposable-machine behavioral acceptance remains unclaimed. |
| Frontend, accessibility, and UX | Pass | Five-decision real WPF smoke, no operation, four inspected screenshots | Installer/update confirmations remain manual and visible. |
| Operations, dependencies, and release | Pass | Public stable `v0.2.0`, four assets, target `349e7fc`; downloaded setup reverified; public latest/channel receipt passed | Setup SHA-256 `B743FF64F881350D78921355053E64087D36468759D2149A95BE65DE10572AC3`. |


## Archived History

Entries before 2026-07-22 were moved verbatim to [quality-gates-archive-part1.md](archive/quality-gates-archive-part1.md), [quality-gates-archive-part2.md](archive/quality-gates-archive-part2.md), [quality-gates-archive-part3.md](archive/quality-gates-archive-part3.md).
