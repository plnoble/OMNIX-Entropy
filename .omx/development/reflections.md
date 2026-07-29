# Reflections

Add an entry at the top of this file before declaring a meaningful task finished.
Keep entries short and focused on reusable learning.

## 2026-07-29 - The installer and updater must share one directory contract

- What worked: verifying the user's actual reinstall rather than assuming install success meant update readiness. The doubled `Install` was a product contract mismatch, and keeping the updater strict while fixing the installer preserved the security boundary.
- What caught real risk: TDD used the real Windows path policy and proved malformed layout refusal happens before any HTTP request. The release draft, download-back hash comparison, and independent verification kept publication separate from build trust.
- CI lesson: an assertion-free hosted testhost abort is inconclusive, not green and not automatically a code defect. One bounded clean retry produced decisive evidence; a second abort would have stopped release for isolation work.
- Waste: the archive verification guessed filenames and then compared reconstructed shell text. Resolve paths first and use Git's own diff semantics for repository content.
- Next improvement: add an installer-layout acceptance receipt that records the directory shown after using Browse, so `AppendDefaultDirName` behavior is covered by a real disposable installer run as well as source contracts.

## 2026-07-29 - Reviewing a record is not reading it

- What worked: re-deriving every claim instead of reading it. Integrity, build, suite, and all five public-release hashes matched exactly, which is a real result — it means the release-era records can be trusted, and that conclusion is only worth something because it was tested rather than assumed.
- What caught real risk: mapping every heading and date before cutting the records. They looked like ordinary newest-first logs but had grown in two directions with the operative gate checklists sandwiched in the middle. The obvious positional split would have archived recent entries and deleted policy.
- Root lesson: an append-only record has two failure modes, and the project had only defended against one. Nothing was ever lost, and that is exactly why `CLAUDE.md`'s mandatory startup read stopped working. Retention without a readability budget produces a record no agent can open.
- Second lesson: a validated argument is not a guarded call. `OpenReleasePage_Click` exact-matched its URL, which made the destination safe and the launch look safe, but ShellExecute can still fail and there is no global dispatcher handler to catch it.
- Waste: low, but one avoidable edit failed because the `old_string` included a comment that was not in the file. Copy exact surrounding text rather than reconstructing it from an earlier read.
- Next improvement: make the readability budget enforceable. A record-size check in `.omx/verify-source-integrity.ps1` or its own small script would turn today's written prevention rule into a gate, which is the difference between a lesson and a control.

## 2026-07-28 - Public 0.1.1 release

- What worked: source CI, same-signer policy, independent installer verification, draft-only staging, and download-back hashing kept each publication boundary separately checkable.
- What caught real risk: Windows PowerShell promoted an expected native CLI miss into a terminating error, and two timestamp providers showed transient failures between adjacent files. Both stopped publication before incomplete assets escaped.
- Waste: endpoint behavior and retry needs were discovered during final packaging, causing extra commit, CI, and rebuild rounds. Exact TSA probing and bounded retry simulation belong in release preflight.
- Reusable lesson: a Windows release is not complete when upload succeeds. Re-download every public asset, compare immutable hashes, rerun the independent installer verifier, and only then publish the draft.
- Next improvement: consolidate signer-context check, exact-TSA probe, bounded signing, draft staging, download-back verification, and public latest-endpoint inspection into one fail-closed release orchestration receipt.

## 2026-07-28 - External cleaner ideas as evidence, not authority

- What worked: comparing capability categories first showed that OMNIX already owned most background evidence and needed only four integration surfaces. That kept the implementation small and compatible with the existing profile/drawer architecture.
- Product lesson: “software added a right-click item” is useful evidence but not a risk verdict. Beginner wording must explain presence without turning normal integrations into a scare label.
- Safety lesson: open-source permission and execution permission are unrelated. Even MIT-licensed deletion code should not enter OMNIX without exact ownership, rollback, confirmation, and pipeline review.
- Waste: one missing namespace import, one guessed solution filename, and one unparsed PowerShell continuation caused avoidable failures. Narrow builds and parser gates caught all three before product-side effects.
- Next improvement: include system footprints in the official-uninstall post-scan so the Agent can say which visible integrations remain, still read-only.

## 2026-07-28 - In-app update completion

- An updater cannot retrofit itself into an already installed older binary. Naming the one-time manual bootstrap early is part of correctness, not an implementation caveat to hide.
- Release metadata is only routing evidence. The safe launch decision still needs exact bytes, bounded length, SHA-256, the current running App identity, the expected channel signer, the downloaded package signer, explicit confirmation, and a local operation gate.
- Download verification and launch verification are separate trust moments. Rechecking after final placement and immediately before process creation materially narrows replacement races.
- A successful upload is not release evidence. Downloading all assets back, comparing every hash, and independently verifying the returned setup caught transport as a separate auditable boundary.
- Restricted shells can report a valid self-signed personal publisher as untrusted when they cannot see the authorized CurrentUser Root store. Conflicting signature results must be resolved in the real user trust context without weakening the verifier.
- GitHub API EOF failures did not justify bypassing draft verification. Keeping the release as a draft until all evidence passed made the browser publication fallback safe and recoverable.
## 2026-07-23 - Personal signer and first valid installer

- What worked: every boundary failed closed. Missing Root trust prevented a false valid candidate; missing Chinese resources prevented a partial setup; independent verifiers, not builder output, decided readiness.
- What caught real risk: testing against the real Windows trust provider showed that TrustedPublisher does not establish a self-signed chain. Binding the second attestation to the actual thumbprint makes the persistent Root decision explicit and reproducible.
- Waste: assuming an installer language existed and guessing the solution filename both caused avoidable failed commands. Resolve tool payloads and repository paths before required gates.
- Reusable lesson: personal Windows signing needs four distinct truths: protected private key, explicit publisher trust, explicit root-chain trust for self-signed certificates, and independent post-sign verification. None implies the others.
- Remaining risk: local cryptographic/install-policy verification is complete, but behavioral acceptance in a disposable Windows environment and any GitHub Release publication remain separate gates.

## 2026-07-23 - D-first installer foundation

- What worked: separating installer definition, signed build, read-only transfer verification, and GitHub staging made each trust transition independently testable.
- What was deliberately deferred: installing Inno and creating a certificate are machine-level choices; source completion must not be used as consent for either.
- Lesson: a beginner-friendly setup can still be fail-closed. Visible path choice and human confirmation belong in the installer definition, while signer/hash evidence belongs outside it in immutable manifests.
- Next improvement: add bounded application-side download and verification that stops before execution, then design explicit install confirmation and rollback around a real verified setup artifact.

## 2026-07-22 - Public CI remediation

- What worked: reducing the failed run by failure class produced four small infrastructure fixes; the tracked-only archive then reproduced GitHub's real input boundary before another push.
- What was wasteful: the first publication trusted a green dirty working tree, so 31 failures were discovered remotely instead of by a clean-source gate.
- Lesson: repository completeness is a property of committed inputs, not the current filesystem. Every release-facing change should include a tracked-only archive or clean-clone verification before push.
- Next improvement: turn the tracked-only source verification into a reusable repository script so future agents cannot accidentally skip it.

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

## 2026-07-22 - Read-only signing prerequisite inspection

- A release blocker is more useful when it is a reproducible report, but discovery still must not imply authorization. Listing candidates and selecting one belong to separate steps.
- Security predicates need one implementation model across readiness and execution. If the inspector and signer interpret EKU differently, a green readiness report is misleading.
- Static source contracts did not catch Windows PowerShell provider behavior or empty-pipeline semantics. One small real-host JSON test found both before the script became operational guidance.
- The current result is now actionable and honest: the store works, the localhost certificate is not a code-signing certificate, and the SDK signing tool is absent.

## 2026-07-22 - External release boundary

- Persistence does not mean inventing local work after the product, safety pipeline, package transform, verifier, fixture, and acceptance protocol are ready. At that point, unrelated changes reduce confidence instead of moving the actual objective.
- A self-signed certificate or primary-machine mutation run would make the final claim less true. Blocking with exact machine-readable prerequisites is the correct engineering outcome until the external state changes.
- Resumption is deterministic: rerun the inspector, create the signed candidate with an explicitly approved thumbprint, independently verify transfer, then complete the ten evidence-bound cases on a resettable Windows environment.

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

## Archived History

Entries before 2026-07-22 were moved verbatim to [reflections-archive-part1.md](archive/reflections-archive-part1.md), [reflections-archive-part2.md](archive/reflections-archive-part2.md).
