# Archived reflections (2026-07-16 to 2026-07-19)

Historical entries moved out of `.omx/development/reflections.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

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

