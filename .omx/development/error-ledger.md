# Error Ledger

## 2026-08-01 - Record patch reconstructed an inexact source line

- Symptom: the first patch that closed the stale active-slice status failed verification without changing the file.
- Wrong assumption: the long status line could be reconstructed from the earlier read.
- Root cause: the patch omitted one existing `C-drive` qualifier, so the context was not exact.
- Detection method: `apply_patch` rejected the edit; `Select-String -Context` exposed the exact line.
- Fix: reread the local context and patch the exact text.
- Prevention rule: copy exact nearby text before replacing a long record line; do not rebuild it from memory.
- Skill candidate: no; existing editing guidance already covers exact context.

## 2026-08-01 - Public release manifest arrived as bytes, not text

- Symptom: the public `omnix-release.json` URL returned HTTP 200, but the first PowerShell parse printed empty manifest fields.
- Wrong assumption: `Invoke-WebRequest.Content` would always be a string for a JSON-named Release asset.
- Root cause: GitHub served the asset as `application/octet-stream`, and Windows PowerShell exposed `Content` as `System.Byte[]`.
- Detection method: the blank parsed receipt conflicted with the known 931-byte asset; inspecting response headers and runtime content type identified the byte array.
- Fix: decode byte content as UTF-8 before `ConvertFrom-Json`; the second receipt returned the exact 0.1.5 version, commit, setup hash/length, and signer.
- Prevention rule: HTTP release verification must record content type and normalize byte/string bodies before semantic parsing.
- Skill candidate: no; incorporate this into the proposed guarded release receipt.

## 2026-08-01 - Download-back hash audit piped directly from `foreach`

- Symptom: the first local four-asset hash comparison failed to parse with `EmptyPipeElement` before reading any asset.
- Wrong assumption: a PowerShell `foreach` statement could be followed directly by `| Format-List` in the composed one-line command.
- Root cause: Windows PowerShell statement grammar requires the loop output to be collected or wrapped before piping.
- Detection method: immediate parser error; no remote or local artifact changed.
- Fix: collect the loop results in `$rows = @(foreach (...) { ... })`, then pipe `$rows` and fail if any `Match` value is false.
- Prevention rule: for Windows PowerShell release audits, collect `foreach` output explicitly before formatting or filtering it.
- Skill candidate: no; the same rule is already documented in this ledger and `skill-candidates.md`.

## 2026-07-30 - Signed-candidate verifier was first given a relative path

- Symptom: `verify-signed-release-candidate.ps1` refused the signed 0.1.5 candidate before verification.
- Wrong assumption: because the producer accepted a repository-relative artifact path, the independent verifier would accept the same form.
- Root cause: the verifier intentionally requires a fully qualified local path as an additional trust boundary.
- Detection method: deterministic verifier error before any package inspection or state change.
- Fix: use the resolved absolute candidate path for all independent verifier calls.
- Prevention rule: resolve and print artifact paths from producer receipts; pass those exact absolute paths to independent verifiers.
- Skill candidate: no; this is a repository-specific command contract already documented by the verifier.

## 2026-07-30 - Restricted signature verification lacked CurrentUser trust visibility

- Symptom: the same signed candidate that reported valid App/worker signatures in host signing context reported “Package signature is not valid” in the restricted sandbox.
- Wrong assumption: a read-only Authenticode check would observe the same CurrentUser Root/TrustedPublisher chain in both trust contexts.
- Root cause: the restricted environment does not expose the host user's complete certificate trust context.
- Detection method: host signer receipt was valid; restricted independent verifier failed only at signature validity after the absolute path passed.
- Fix: require an explicitly approved host-context retry of the unchanged verifier against the unchanged candidate; never relax the verifier.
- Prevention rule: release verification that depends on CurrentUser certificate trust must run in the same explicitly authorized host trust context, while hashes and package paths remain fixed.
- Skill candidate: no; existing release records already require host-context trust verification.

## 2026-07-30 - Unregistered application evidence was called the main program

- Symptom: the migration outcome called every C- or D-drive install-path record the “main program,” including entries without an official uninstall route that may be a portable copy or updater payload.
- Wrong assumption: a non-empty install path was sufficient evidence that the selected record represented the canonical main installation.
- Root cause: storage placement and uninstall identity were modelled separately, but the beginner wording used only drive placement.
- Detection method: release review of the three-entry OpenCode drawer and its real screenshot.
- Fix: reserve “main program” wording for entries with an official uninstaller; describe other entries as a program, copy, or update payload, and add unit/GUI contracts.
- Prevention rule: beginner identity labels must combine path evidence with registration/uninstall evidence; never promote a path clue into canonical-install identity.
- Skill candidate: no; this is covered by the existing exact-entry/family identity decision.

## 2026-07-30 - Version audit repeated a prohibited wildcard path

- Symptom: a final read-only `rg` command produced OS error 123 for a `release*` path and also referenced an unverified root props path.
- Wrong assumption: filename globs were safe as path arguments in this Windows shell and the expected props file existed.
- Root cause: the command ignored the repository search rule already read at startup.
- Detection method: immediate `rg` path errors; no file or product state changed.
- Fix: resolve candidates with `rg --files -g` first, then search only the returned exact paths.
- Prevention rule: even during final verification, never place `*` or `?` in a Windows path argument and never add an unobserved path to a required read.
- Skill candidate: no; this is already an explicit repository rule.

## 2026-07-30 - Profile clone dropped newly added exact-entry evidence

- Symptom: unit deserialization preserved version/source/C-data fields, but the real WPF drawer showed “版本未识别” and “扫描来源未确认”.
- Wrong assumption: adding fields to `SoftwareProfile` and the inventory builder was sufficient for every downstream presentation path.
- Root cause: `SoftwareGrowthProfileEnricher.CloneWithGrowth` manually copied the old property set and silently omitted the three new fields.
- Detection method: isolated WPF fixture smoke, followed by a direct fixture-scanner contract and a search for `SoftwareProfile` cloning sites.
- Fix: copy `DisplayVersion`, `InventorySource`, and `CDriveDataSizeBytes` in the growth enricher and add a preservation regression test.
- Prevention rule: every manually cloned shared model needs a preservation contract or centralized copy mechanism when the model gains fields.
- Skill candidate: yes; source analysis can flag manual shared-model clones when properties change.

## 2026-07-30 - New Windows PowerShell smoke contained UTF-8 Chinese literals without a BOM

- Symptom: the first parser gate reported cascading missing-parenthesis and unterminated-string errors even though the UTF-8 source looked structurally valid.
- Wrong assumption: Windows PowerShell would decode the new no-BOM UTF-8 script the same way as .NET source files.
- Root cause: Windows PowerShell parsed the Chinese literal bytes using its legacy default encoding.
- Detection method: parser extents showed mojibake beginning at the first Chinese literal; `Get-Content -Encoding utf8` showed valid syntax.
- Fix: construct required Chinese UI text from Unicode code points, matching existing repository smoke patterns; parser gate then passed.
- Prevention rule: new Windows PowerShell 5 smoke scripts must stay ASCII or use the repository Unicode-code-point helper unless the file's BOM behavior is explicitly verified.
- Skill candidate: no; this is a stable repository script rule candidate.

## 2026-07-30 - Read-only host audit reused unverified PowerShell shapes

- Symptom: two directory-audit commands placed a pipeline directly after a `foreach` statement and failed to parse; one source read guessed a separate `BigRocksProbe.cs` although the class lives in `BigRock.cs`; one host-size probe assumed `System.IO.EnumerationOptions` was available in Windows PowerShell.
- Wrong assumption: PowerShell statement/pipeline grammar, source-file naming, and host runtime APIs matched the intended command without direct verification.
- Root cause: the audit commands were composed from memory instead of first resolving the source path and using the repository-compatible Windows PowerShell surface.
- Detection method: immediate parser/path/type errors; no mutation occurred.
- Fix: collect loop output in an explicit array, resolve source symbols with `rg`, and use bounded `Get-ChildItem` enumeration compatible with the current host.
- Prevention rule: never append a pipeline directly to a PowerShell `foreach` statement; resolve source files before reading; check the active PowerShell/.NET API surface before using newer runtime types.
- Skill candidate: yes; the recurring PowerShell host-compatibility checks can become a small read-only audit helper.

## 2026-07-30 - GUI smoke relied on WPF control-type enumeration

- Symptom: three isolated GUI smoke runs completed the fixture scan and visibly rendered the health table, but the script reported zero `ListItem`/`DataItem` descendants and timed out before testing the new drive-health actions.
- Wrong assumption: a populated WPF `ListView` will always expose its realized rows through UIAutomation control-type enumeration.
- Root cause: the rendered rows retained their stable per-row AutomationIds, but this host's UIAutomation provider did not expose them through the script's `ListItem`/`DataItem` descendant count.
- Detection method: repeated identical failures, completion status text, and a failure-point screenshot showing the full populated table.
- Fix: wait for a stable summary-row AutomationId and verify each required dimension by its own AutomationId; retain failure-point screenshot capture.
- Prevention rule: use stable semantic AutomationIds as GUI proof targets; do not use WPF control-type enumeration as the sole readiness condition.
- Skill candidate: no; this is covered by the repository GUI protocol and smoke-helper pattern.

## 2026-07-30 - Completion checks repeated unobserved command assumptions

- Symptom: the first GUI smoke extension called nonexistent `Get-Pattern`; a nested parser probe reported zero parser errors but failed on its own success-output quoting; the first Release build targeted nonexistent `OMNIX-Entropy.sln`.
- Wrong assumption: a helper API existed, nested PowerShell would preserve the final quoted string, and the solution name followed the product name.
- Root cause: the relevant helper surface and build entry were not resolved before use; the parser probe also repeated the repository's known nested-shell quoting hazard.
- Detection method: immediate command failures before product mutation; `rg` then found direct `GetCurrentPattern` usage and canonical `ComputerSecuritySoftware.slnx`.
- Fix: use the UIAutomation element's real `GetCurrentPattern`, rely on the successfully executed Windows PowerShell smoke as the parser/runtime gate, and build the resolved `.slnx`.
- Prevention rule: inspect helper definitions before calling them; do not nest ad hoc PowerShell parser probes; use the canonical solution entry recorded in `AGENTS.md`.
- Skill candidate: no; existing project rules cover source/helper resolution and nested PowerShell, and the canonical solution is now explicit.

## 2026-07-30 - Two source reads guessed paths after the owning symbols were searchable

- Symptom: reads failed for `src\Css.Core\Apps\DriveHealthPlanPresentation.cs` and `src\Css.Scanner\Persistence\HealthDigestStore.cs`.
- Wrong assumption: the namespace or conceptual owner predicted the physical source path.
- Root cause: the files actually live under `Css.Scanner\Experience` and `Css.Core\Apps`; the paths were not resolved before the required read.
- Detection method: immediate `PathNotFound`, followed by exact `rg --files`/symbol resolution.
- Fix: read the paths returned by repository search.
- Prevention rule: resolve every unobserved source path before adding it to a parallel read batch, including paths whose namespace appears obvious.
- Skill candidate: no; this is already a repository rule.

## 2026-07-30 - Restricted release staging could not see the trusted signer

- Symptom: the first local `prepare-personal-github-release.ps1` run rejected the already independently verified setup as having an invalid signature or timestamp.
- Wrong assumption: a restricted process would observe the same CurrentUser trust chain as the real Windows user context.
- Root cause: the sandboxed certificate provider did not expose the authorized CurrentUser Root/publisher trust used by Windows Authenticode.
- Detection method: the host-context verifier immediately returned the expected signer and `CanStageGitHubRelease=true`; the same full staging script then passed unchanged in that context.
- Fix: rerun the guarded verifier/staging script in the host certificate context without changing the package, certificate, trust stores, or verification policy.
- Prevention rule: personal-publisher release verification is host-context evidence; never interpret a restricted trust-store miss as package corruption or weaken signature checks to make it pass.
- Skill candidate: no; this context boundary is already documented in prior release reflections.

## 2026-07-30 - GitHub release transport failed in ambiguous and partial states

- Symptom: the first draft-create request timed out during TLS; a later download returned three complete assets and repeated EOF for the fourth 1 KB manifest.
- Wrong assumption: one bulk GitHub command would provide an atomic success/failure receipt for release creation and all downloads.
- Root cause: GitHub API/CDN transport failed independently across requests; the create timeout was ambiguous and the download command retained already completed files.
- Detection method: explicit `gh release view` after the create timeout, directory enumeration plus SHA-256 after the partial download, and comparison with GitHub asset digests.
- Fix: confirm the absent release before retrying creation; preserve the draft; inspect partial outputs; fetch only the missing asset through the authenticated asset API; compare all four files; independently reverify the downloaded setup.
- Prevention rule: release automation must reconcile remote state after create timeouts and support per-asset resumable download-back verification.
- Skill candidate: yes.

## 2026-07-30 - The first authenticated asset fallback used incompatible shell quoting

- Symptom: two attempts to redirect `gh api` output through `cmd` failed immediately with “filename, directory name, or volume label syntax is incorrect.”
- Wrong assumption: the execution wrapper's shell selection and quoting would preserve the nested `cmd` redirection exactly.
- Root cause: quoting was transformed before `cmd` parsed the command line.
- Detection method: immediate exit before any network transfer and no new asset file.
- Fix: stop composing nested shell redirection; use `curl.exe` with the in-process `gh auth token`, GitHub's asset API, bounded retries, and a direct binary output path.
- Prevention rule: use a binary-capable HTTP client with an explicit output file for authenticated release assets; do not nest shell redirection through another command parser.
- Skill candidate: yes.

## 2026-07-30 - The combined disk flow leaked context across selections

- Symptom: switching disks or starting another scan cleared headline/progress but left old plan steps and safety copy; the selected safe recommendation's specific explanation was overwritten by generic list copy; a D-drive prevention step still said to move data to D; retained C-drive history was not visibly scoped.
- Wrong assumption: clearing the main result collections was enough, and wording that was correct for the original C-only page would remain correct after adding a drive selector.
- Root cause: health-plan state was reset field by field in multiple paths, assignment order replaced selected state, and destination/history copy was not derived from scan scope.
- Detection method: release review of the composed diff, focused source contracts, and final multi-disk screenshot inspection.
- Fix: centralize plan reset; assign generic recommendation copy before selected-card presentation; derive destination guidance from system/data scope; keep digest history system-drive-only, label it, and reselect the system drive before current-evidence loading.
- Prevention rule: any new scope selector must audit every cached field, history surface, action label, and destination sentence; screenshots must include the selected scope and retained historical evidence together.
- Skill candidate: no.

## 2026-07-30 - History wording changed without updating its safety-order contract

- Symptom: the first full test run passed 1084 tests and failed `HealthDigestEvidenceNavigationTests` because it searched for the old escaped “当前 C 盘证据已打开” source string.
- Wrong assumption: the broader focused set covered all static wording contracts affected by replacing C-drive history copy with system-drive copy.
- Root cause: one dedicated test protects the load-before-success ordering by locating the exact user-visible string, and it was outside the initial focused filter.
- Detection method: final full Debug test run.
- Fix: update the contract to the system-drive wording, require `SelectSystemDriveTarget`, and retain the ordering assertions around `EnsureHealthScanLoadedAsync` and readiness checks.
- Prevention rule: when a visible status sentence is also a static control-flow marker, search the entire test tree for the old text before the first full run.
- Skill candidate: no.

## 2026-07-30 - Multi-disk smoke searched the whole desktop

- Symptom: two consecutive real GUI smoke attempts failed at `RootElement.FindAll(Descendants, ListItem)` with `RPC_E_SERVERFAULT` before any OMNIX assertion ran.
- Wrong assumption: filtering by process id after whole-desktop enumeration was equivalent to scoping discovery to the product process.
- Root cause: the enumeration still invoked every unrelated desktop accessibility provider before filtering, so one external COM server fault aborted the smoke.
- Detection method: identical stack traces on two clean, bounded retries and line-level script inspection.
- Fix: enumerate top-level windows matching the OMNIX process id and search only their descendants; tolerate a COM fault from one process-owned window and continue; collapse the ComboBox before the screenshot.
- Prevention rule: GUI smokes must scope UIAutomation discovery before enumeration, not filter afterward.
- Skill candidate: yes.

## 2026-07-29 - The first health-plan layouts hid existing or downstream work

- Symptom: the first detailed homepage plan made the real smoke report zero rendered health rows; after compressing the homepage, the first disk screenshot still left the selected preview action outside the visible workflow.
- Wrong assumption: all three plan steps belonged on the homepage, and a fixed right-hand `StackPanel` plus `BringIntoView` was enough to reach content below a long recommendation list.
- Root cause: the homepage's auto-height plan competed with a bounded results row; the disk recommendation card had no owning scroll viewport, while its nested list consumed the visible height.
- Detection method: repeated real WPF UIAutomation runs and manual inspection of `.omx/qa-home-agent-next-action.png` and `.omx/qa-drive-health-plan.png`.
- Fix: keep only target/progress/action on Home; move detailed steps to the disk page; give the complete right recommendation workflow one outer scroll surface; disable nested recommendation-list scrolling; keep the preview button inside its action panel and scroll to it.
- Prevention rule: beginner first-view additions must preserve existing result capacity, and every multi-stage action workflow must prove the final next-step control in a real constrained window.
- Skill candidate: yes.

## 2026-07-29 - WPF parsed a bullet StringFormat as a markup extension

- Symptom: the first health-plan XAML build failed at the bullet row with a generated error about a missing `Extension` assembly.
- Wrong assumption: `Text="{Binding, StringFormat=• {0}}"` would be parsed as an ordinary composite format.
- Root cause: the unescaped braces were interpreted as XAML markup-extension syntax.
- Detection method: the focused test build failed before tests ran and reported the exact XAML line/position.
- Fix: use a two-column template with a static bullet `TextBlock` and a separately bound wrapping `TextBlock`.
- Prevention rule: avoid composite `StringFormat` for decorative WPF prefixes; use explicit template controls or escape the markup-extension braces.
- Skill candidate: no.

## 2026-07-29 - The home smoke timeout hid whether scanning or the app failed

- Symptom: two real runs ended with “health dimensions: 0” after the old 60-second bound, while intervening and later runs passed on the same build.
- Wrong assumption: a zero item count alone was enough evidence to diagnose a layout regression.
- Root cause: the waiter did not report process exit or the current status text, and its 60-second bound left no allowance for variable machine-observation timing.
- Detection method: rerunning the unchanged compact layout succeeded; the failure occurred before the new goal assertions.
- Fix: retain a bounded 90-second wait, stop immediately if the app exits, and include current status text in timeout failures.
- Prevention rule: GUI wait failures must distinguish process exit, still-running work, and rendered-but-empty state before assigning a product root cause.
- Skill candidate: yes.

## 2026-07-29 - A repository read guessed paths already contradicted by search output

- Symptom: `Get-Content` failed for two nonexistent `Css.Scanner\Experience\Recommendation*Presenter.cs` paths.
- Wrong assumption: presenter filenames and ownership matched their type names under Scanner.
- Root cause: the preceding symbol search had already shown the actual files under `src\Css.Core\Apps`, but the read command used guessed paths.
- Detection method: immediate `PathNotFound` errors.
- Fix: read the exact paths returned by `rg`.
- Prevention rule: resolve every unobserved source path from `rg --files` or symbol-search output before adding it to a required read.
- Skill candidate: no.

## 2026-07-29 - Nested PowerShell parser check expanded its variables too early

- Symptom: the first final parser-check command produced `[ref]` without variables and an empty pipeline.
- Wrong assumption: a double-quoted `powershell -Command` payload would reach the child process unchanged when launched from PowerShell.
- Root cause: the outer shell expanded `$tokens`, `$errors`, and `$_` before the child parser received the command.
- Detection method: the command failed immediately with an empty-pipe parser error rather than reporting an error from the target smoke script.
- Fix: enclose the child command payload in single quotes; the target script then passed parsing.
- Prevention rule: single-quote nested PowerShell command payloads that contain `$` variables, or invoke a checked `.ps1` file directly.
- Skill candidate: no.

## 2026-07-29 - The first bounded history layout still clipped its action

- Symptom: the first real WPF run made key findings scroll, but the history row collapsed; a second allocation showed the history row while clipping its evidence button below the card.
- Wrong assumption: constraining only the daily-history list would make the complete variable-height history section usable, and every health-dimension row would remain simultaneously visible after its list became scrollable.
- Root cause: the latest summary, weekly summary, monitoring notice, daily rows, and evidence action all consume the same limited card height; the old smoke also treated visibility as a fixed contract instead of scrolling offscreen rows into view.
- Detection method: real window screenshots and UIAutomation's offscreen-row failure, followed by direct `ScrollPattern` inspection.
- Fix: make the entire history section one bounded scroll surface with a non-nested `ItemsControl`; use `ScrollItemPattern.ScrollIntoView()` for health dimensions; require actual scroll-percent changes for both long result regions.
- Prevention rule: bound and scroll the complete semantic variable-height region, then prove viewport movement and action reachability in a real constrained window.
- Skill candidate: yes.

## 2026-07-29 - The first multi-disk WPF smoke made three UIAutomation assumptions

- Symptom: the new smoke first failed to parse under Windows PowerShell, then reported fewer than two drives, then tried to select an unrelated list item that did not support `SelectionItemPattern`.
- Wrong assumption: Windows PowerShell would decode Chinese literals in a BOM-less UTF-8 script, a data-template panel name would reliably name its `ComboBoxItem`, and any process list item containing a drive letter plus `GB` was a drive choice.
- Root cause: Windows PowerShell 5.1 decoding, accessibility metadata placed on a decorative panel instead of the control container, and a process-wide list-item search that also saw homepage results.
- Detection method: parser gate followed by three real WPF UIAutomation runs; the second run's unsupported-pattern exception distinguished a false-positive list item from a drive choice.
- Fix: use ASCII regexes in the smoke, bind `AutomationProperties.Name` on both the selector and its `ComboBoxItem` containers, and require `SelectionItemPattern` before treating a list item as a drive. Final smoke listed C/D, selected D, and captured an unobstructed window screenshot.
- Prevention rule: keep Windows PowerShell smoke control flow ASCII; place beginner accessibility names on real UIAutomation controls; qualify process-wide UI searches by required control patterns.
- Skill candidate: yes.

## 2026-07-29 - A renamed machine-observation status left one exact source contract stale

- Symptom: the first full suite had one failure after 1078 passes because an existing test still required the old fixed text `正在只读读取 D 盘`.
- Wrong assumption: the focused multi-disk source contracts covered every fixed-drive status string affected by the wording change.
- Root cause: `MachineHealthExperienceTests` exact-matched the old machine-observation progress text outside the focused test filter.
- Detection method: the full Debug suite identified the exact assertion after product-focused tests were green.
- Fix: update the contract to require the new drive-neutral `正在只读读取本机磁盘`; the final full suite passes 1079/1079.
- Prevention rule: search the full test tree for exact UI copy before renaming shared progress text, even when the behavior has dedicated focused contracts.
- Skill candidate: no.

## 2026-07-29 - A FluentAssertions predicate used unsupported pattern syntax

- Symptom: the first focused green compile failed with CS8122 in the new data-drive cleanup-authority test.
- Wrong assumption: `NotContain` would compile the `recommendation.Operation is not null` predicate as an ordinary delegate.
- Root cause: this FluentAssertions overload captures an expression tree, where C# pattern-matching syntax is unsupported.
- Detection method: the focused test build failed before execution and identified the exact predicate line.
- Fix: use the expression-tree-compatible `recommendation.Operation != null`; the focused suite then passed 200/200.
- Prevention rule: keep predicates passed to expression-tree assertion overloads to supported comparisons rather than pattern syntax.
- Skill candidate: no.

## 2026-07-29 - First GitHub CI attempt aborted the testhost without a failing test

- Symptom: GitHub CI `30424760666` built successfully, then the testhost crashed after reporting 429 passed, zero failed, and `Test Run Aborted`; source integrity did not run.
- Wrong assumption: a pushed commit that passes the same local build/test sequence will necessarily produce a decisive first hosted-run result.
- Root cause: undetermined hosted runner or testhost failure. The log contained no failed assertion or named failing test, and the same commit completed all 1068 tests on a fresh runner.
- Detection method: read the complete failed job log instead of treating the workflow conclusion as a code assertion failure.
- Fix: stop publication, perform one bounded rerun of only the failed job, and require the fresh runner to pass build, full suite, and integrity before resuming. The retry passed.
- Prevention rule: one assertion-free testhost abort may receive one clean failed-job retry after log inspection; a repeated abort is a reproducible blocker that requires test-level isolation, not further retries.
- Skill candidate: no.

## 2026-07-29 - Archive verification guessed a directory layout that did not exist

- Symptom: the first archive-content comparison emitted 15 `path does not exist` failures, then a second ad hoc text comparison reported false mismatches for files whose Git diff showed only a trailing blank line.
- Wrong assumption: archive files followed a nested `archive/<record>/part-N.md` convention and PowerShell line-array reconstruction was a trustworthy content comparison.
- Root cause: the real files use flat names such as `archive/current-archive-part1.md`; the guessed paths were not resolved with `rg --files` first, and reconstructing Git output through PowerShell introduced comparison ambiguity.
- Detection method: `rg --files .omx\development\archive`, followed by a direct `git diff --word-diff` on one reported mismatch.
- Fix: use the repository's real paths and let Git perform the intended semantic check: `git diff --ignore-blank-lines --exit-code fe2e012 -- .omx/development/archive`, which passed.
- Prevention rule: resolve archive paths before a required batch, and use Git's diff engine for repository-content equivalence instead of rebuilding blobs through shell text decoding.
- Skill candidate: no.

## 2026-07-29 - The 0.1.2 bootstrap install landed in a layout the updater refuses

- Symptom: 0.1.2 is installed and registered (`OMNIX-Entropy 版本 0.1.2`, publisher `plnoble`, InstallDate `20260728`), but its `InstallLocation` is `D:\Software\OMNIX-Entropy\Install\Install\` and the executable is at `D:\Software\OMNIX-Entropy\Install\Install\Css.App.exe`. The expected managed layout is `D:\Software\OMNIX-Entropy\Install\Css.App.exe`.
- Consequence: `WindowsPersonalUpdatePathPolicy.ResolveProductRoot` requires the executable's parent to be named `Install` and its grandparent to be named `OMNIX-Entropy`. Here the grandparent is `Install`, so it throws `In-app update requires the managed OMNIX installation layout.` inside `DownloadAndVerifyAsync`, which the general catch converts into `更新包没有准备完成，也没有启动安装程序。` The in-app update path is unavailable on this installation, and the beginner-visible message does not say why.
- Wrong assumption: that completing the manual bootstrap install was sufficient to enable the in-app update path. Installation success and updatable layout are separate facts, and only the first was checked.
- Root cause: `installer/OMNIX-Entropy.iss` sets `DefaultDirName=D:\Software\OMNIX-Entropy\Install` and leaves `AppendDefaultDirName` at its default of yes. Inno's Browse dialog then appends the last component of `DefaultDirName` to whatever folder is selected, so confirming the pre-filled path through Browse yields `...\Install\Install`. Nothing in the installer or the app detects the resulting layout.
- Detection method: read-only uninstall-registry query plus a bounded filesystem check after the user reported the install; the doubled segment was visible in `InstallLocation`.
- Fix: the user reinstalled 0.1.2 into the correct managed directory and host verification confirmed the doubled executable is absent. Source prevention now sets `AppendDefaultDirName=no`; the real path policy keeps rejecting doubled layouts through a typed exception; the downloader returns exact reinstall guidance and makes no HTTP request. Focused contracts pass; broader gates and release are pending.
- Prevention rule: a path policy that encodes a required directory layout needs a matching installer guarantee and a distinct refusal message. Do not treat a successful install as proof that the layout the product depends on was produced.
- Skill candidate: yes.

## 2026-07-29 - Read-only installation check used an invalid pipeline after foreach

- Symptom: the first two PowerShell installation-inspection commands stopped at parse time with `EmptyPipeElement`.
- Wrong assumption: an inline `foreach (...) { ... } | Format-List` statement could be piped directly in Windows PowerShell.
- Root cause: the loop statement was not grouped or assigned before the pipeline.
- Detection method: immediate parser failure before any filesystem or registry access.
- Fix: collect loop results into an array and pipe the completed array to formatting; the corrected read-only checks then succeeded.
- Prevention rule: assign statement output before piping when a Windows PowerShell command starts with `foreach`.
- Skill candidate: no.

## 2026-07-29 - Development records grew past the tooling read limit

- Symptom: `Read` refused `current.md` (566 KB), `quality-gates.md` (418 KB), `worklog.md` (384 KB), and `error-ledger.md` (292 KB) outright. `CLAUDE.md` instructs every agent to read `current.md` before changing the project, so the project's own startup rule could not be followed.
- Wrong assumption: that append-only records stay usable indefinitely because nothing is ever lost. Retention was treated as the only property that mattered.
- Root cause: `AGENTS.md` requires updating eight records for every meaningful slice but sets no size or archival policy, and no gate measures record size, so growth was invisible until a read failed.
- Detection method: the failure surfaced at session start, from the mandatory read itself.
- Fix: split by date into live records (2026-07-22 onward) plus chunked archives under `.omx/development/archive/`, proven lossless by comparing all 1,724 entry blocks against `HEAD`.
- Prevention rule: archive a live record's older dated entries when it approaches roughly 200 KB. Keep undated templates and gate checklists in the live file — they are policy, not history.
- Skill candidate: yes.

## 2026-07-29 - A positional split would have destroyed the gate checklists

- Symptom: the records looked like ordinary newest-first logs, but `worklog.md`, `reflections.md`, and `error-ledger.md` each had recent entries at the *bottom* as well, and `quality-gates.md` held its Pre-Change/Pre-Delivery/Template sections in the middle at lines 1071-1137.
- Wrong assumption: that each record had one consistent ordering, so "keep the head, archive the tail" would be safe.
- Root cause: the files were originally appended oldest-first, and a later convention change began prepending newest-first without migrating the existing tail. Both runs then continued in place.
- Detection method: mapping every heading with its line number and date before cutting anything, rather than sampling the first and last few.
- Fix: date-aware selection with undated sections always retained, plus a verification pass comparing every block against `HEAD` before the change was accepted.
- Prevention rule: never cut a long-lived append-only file by position. Map its headings and dates first; an ordering convention that changed once may have changed only for new writes.
- Skill candidate: yes.

## 2026-07-29 - An unguarded launch in a WPF click handler could terminate the app

- Symptom: `UpdateWindow.OpenReleasePage_Click` called `Process.Start` with no catch, while `Css.App` registers no `DispatcherUnhandledException` handler.
- Wrong assumption: that opening a validated URL cannot fail. The URL is exact-matched by `IsExpectedReleasePage`, which makes the *destination* safe but says nothing about whether ShellExecute succeeds — a missing browser association or a policy block still throws.
- Root cause: the surrounding handlers in the same file already wrap their work in try/catch, so the omission read as consistent at a glance; the failure path was never the subject of a test.
- Detection method: source review comparing this launch site against `MainWindow.xaml.cs` and `OfficialUninstallWorkerLauncher.cs`, both of which catch.
- Fix: catch and report a path-free beginner conclusion stating that nothing was downloaded or installed.
- Prevention rule: every `Process.Start` reached from a WPF click handler needs a catch while the app has no global dispatcher handler. Validating an argument is not the same as guarding the call.
- Skill candidate: no.

## 2026-07-28 - Path-binding hardening patch missed one method brace

- Symptom: the focused updater test build reported CS1513/CS1022 at the end of `PersonalUpdateInstallation.cs`.
- Wrong assumption: the small tail patch inserted a helper after a complete expression-bodied method block.
- Root cause: `TryReadLong` was a block-bodied method and its closing brace was omitted before `PathsEqual`; one extra file-level brace remained.
- Detection method: immediate focused compile after the security hardening patch.
- Fix: close `TryReadLong` before the new helper and remove the extra final brace; rerun the same focused tests.
- Prevention rule: after manually inserting a helper near a file tail, run the narrow compile before adding more edits and inspect the numbered tail on any parser error.
- Skill candidate: no.

## 2026-07-28 - Payload signer had no bounded timestamp retry

- Symptom: both DigiCert and GlobalSign could sign one payload executable, then transiently fail the timestamp request for the second executable, invalidating the whole fresh candidate.
- Wrong assumption: one SignTool invocation per file was sufficient because timestamp availability had already been proven.
- Root cause: RFC3161 is an external network dependency and can fail transiently between adjacent files; the payload signer lacked the bounded retry behavior already provided by Inno Setup.
- Detection method: real fresh-candidate signing logs showed first-file success followed by second-file timestamp transport failure for two independent TSAs.
- Fix: add one shared SignTool helper with at most three attempts and ten seconds between attempts; every file still requires exit-code success and post-sign Authenticode/timestamp verification.
- Prevention rule: release-only network operations should use small bounded retries, fresh outputs, and independent final verification; retries must never turn a missing timestamp into success.
- Skill candidate: yes.

## 2026-07-28 - DigiCert timestamp fallback was not a valid HTTPS substitution

- Symptom: the final payload signing failed on transient DigiCert HTTP timestamp responses; substituting `https://timestamp.digicert.com` then failed immediately as an invalid timestamp URL.
- Wrong assumption: changing only the scheme of the documented DigiCert endpoint would produce an equivalent SignTool RFC3161 service.
- Root cause: the approved URL contract and the actual TSA endpoint are path/scheme specific; endpoint reachability must be proven with SignTool rather than inferred from HTTPS availability.
- Detection method: SignTool rejected the HTTPS URL, repeated HTTP attempts failed closed, and no failed output directory passed package verification.
- Fix: verify GlobalSign's currently documented `http://timestamp.globalsign.com/tsa/r45standard` against an isolated EXE copy, confirm a valid R45 TSA timestamp, then allowlist only that exact official HTTP host/path in both signing scripts while retaining the exact DigiCert fallback.
- Prevention rule: before changing a timestamp endpoint, use the vendor's current official SignTool guidance and validate the exact URL on a disposable artifact copy; never infer a timestamp URL by changing schemes.
- Skill candidate: yes.

## 2026-07-28 - Missing GitHub release was promoted to a terminating PowerShell error

- Symptom: verified 0.1.1 assets were staged locally, but draft creation stopped when `gh release view v0.1.1` correctly reported that the release did not exist.
- Wrong assumption: redirecting all native command output to `$null` under Windows PowerShell 5.1 would still allow the script's `$LASTEXITCODE` branch to handle the expected exit code.
- Root cause: with `$ErrorActionPreference = "Stop"`, the native stderr record became a terminating `NativeCommandError` before the exit-code check ran.
- Detection method: the real draft-stage execution failed at the release lookup and no remote release was created.
- Fix: scope the expected lookup to `SilentlyContinue`, capture its exit code, restore the original preference in `finally`, treat 0 as an existing-tag refusal, accept only 1 as not found, and reject all other exit codes.
- Prevention rule: native CLI probes that use a nonzero exit code for an expected negative result must explicitly suppress the PowerShell error record and validate the captured exit code.
- Skill candidate: yes.

## 2026-07-28 - Restricted certificate view was mistaken for host certificate loss

- Symptom: the sandboxed CurrentUser certificate query returned only a localhost certificate and the release inspector reported zero eligible code-signing certificates.
- Wrong assumption: the restricted command environment exposed the same CurrentUser certificate-store view as the host Windows user session.
- Root cause: certificate-store visibility differs across the restricted and approved host execution contexts.
- Detection method: an approved read-only host-session query found the original thumbprint in CurrentUser My, TrustedPeople, TrustedPublisher, and Root, with the private key still only in My.
- Fix: retain sandbox checks for source work, but perform the final signer preflight and signing commands in the approved host context; do not create a replacement certificate based on a restricted-view miss.
- Prevention rule: before declaring a CurrentUser signing identity missing, repeat the exact read-only store query in the same host context that will run SignTool.
- Skill candidate: yes.

## 2026-07-28 - Release script parser loop used an ambiguous variable reference

- Symptom: the combined read-only parser check failed before inspecting any release script and reported `InvalidVariableReferenceWithDrive`.
- Wrong assumption: PowerShell would parse `"$file: ..."` as the variable `file` followed by a literal colon.
- Root cause: a colon immediately after a variable name inside an interpolated string is parsed as part of a scoped variable reference.
- Detection method: the PowerShell parser pointed at `$file:` in the check command.
- Fix: delimit the name as `"${file}: ..."` and rerun the same read-only parser check.
- Prevention rule: use `${name}` whenever punctuation immediately follows an interpolated PowerShell variable.
- Skill candidate: no.

## 2026-07-23 - Inno compiler did not bundle Simplified Chinese

- Symptom: the first setup compile stopped because `compiler:Languages\ChineseSimplified.isl` did not exist, after signing a temporary uninstaller but before producing setup.
- Wrong assumption: the winget Inno Setup 6.7.3 installation included every language referenced by the installer definition.
- Root cause: Simplified Chinese is an Inno user-contributed translation stored under the official source repository's `Files/Languages/Unofficial`, not in the installed standard language set.
- Detection method: real ISCC output named the missing include path and exit code 2.
- Fix: pinned the translation from official tag `is-6_7_3`, recorded SHA-256 and license/source, switched to a script-relative path, and rebuilt in a fresh output directory.
- Prevention rule: compile installer definitions against the exact selected tool installation before calling language/tool prerequisites complete; vendor nonstandard build inputs with provenance.
- Skill candidate: yes.

## 2026-07-23 - Full test command guessed the solution filename

- Symptom: `dotnet test OMNIX-Entropy.sln --no-restore` failed immediately with MSB1009.
- Wrong assumption: the product name matched the repository solution name.
- Root cause: the repository uses `ComputerSecuritySoftware.slnx`.
- Detection method: `rg --files -g "*.sln" -g "*.slnx"` resolved the actual file.
- Fix: reran the full suite against `ComputerSecuritySoftware.slnx`; 1054/1054 passed.
- Prevention rule: resolve unobserved solution paths with `rg --files` before invoking required completion checks.
- Skill candidate: no; this is already a repository search rule.

## 2026-07-23 - Personal publisher trust needed more explicit blast-radius consent

- Symptom: the guarded certificate initializer was rejected before execution even though the user had said to continue after the earlier prerequisite summary.
- Wrong assumption: general continuation approval was sufficiently explicit for persistent CurrentUser publisher trust.
- Root cause: TrustedPeople/TrustedPublisher makes any future binary signed by the corresponding private key trusted for that Windows user; this blast radius must be named directly at the approval point.
- Detection method: approval reviewer rejected process creation before the certificate script ran.
- Fix: do not retry or work around the refusal; explain exact stores, persistence, scope, and what the trust means, then request an explicit yes/no answer.
- Prevention rule: certificate-store trust prompts must state that all binaries signed by the key become trusted for the selected scope, even when the key is non-exportable and Root/LocalMachine are untouched.
- Skill candidate: yes.

## 2026-07-23 - Combined audit regex broke PowerShell quoting

- Symptom: the final status/privacy audit stopped at a PowerShell parser error before either `rg` search ran.
- Wrong assumption: a double-quoted command could safely contain a regex character class with an unescaped double quote alongside another quoted regex.
- Root cause: the inner quote terminated the outer PowerShell command string.
- Detection method: parser reported a missing string terminator and invalid pipeline expression; no audit output was produced.
- Fix: split the audit into simple single-quoted searches without embedded quote characters.
- Prevention rule: keep final audit regexes single-quoted and run structurally different searches as separate commands.
- Skill candidate: no.

## 2026-07-23 - Trusted publisher stores did not establish a self-signed Authenticode chain

- Symptom: SignTool successfully signed and RFC3161-timestamped App and worker, but `Get-AuthenticodeSignature` returned `UnknownError` and SignTool verification returned exit 1.
- Wrong assumption: placing the self-signed end-entity code-signing certificate in CurrentUser TrustedPeople and TrustedPublisher would also make its Authenticode chain valid.
- Root cause: Windows still requires the self-signed certificate to anchor in Trusted Root; TrustedPublisher expresses publisher trust but does not replace root-chain trust.
- Detection method: the signed files reported `A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider`; both carried the expected signer and a valid DigiCert timestamp.
- Fix: the signed-candidate transform failed closed and no valid manifest was emitted. Await separate explicit CurrentUser Root authorization before changing trust scope or rebuilding.
- Prevention rule: prove a proposed personal certificate topology with SignTool `/pa` before claiming it can produce `Valid`; name Root-store impact separately from publisher trust.
- Skill candidate: yes.

## 2026-07-23 - HTTPS-only timestamp validation rejected official SignTool endpoints

- Symptom: the reviewed signing scripts could not accept the official DigiCert RFC3161 URL even though timestamping was a required release gate.
- Wrong assumption: an RFC3161 endpoint used by SignTool should always be HTTPS.
- Root cause: DigiCert documents `http://timestamp.digicert.com` as its supported RFC3161 endpoint and specifically warns that HTTPS can fail; the protocol sends a digest and verifies the TSA-signed response.
- Detection method: official DigiCert documentation review before the first signing invocation.
- Fix: allow arbitrary absolute HTTPS endpoints plus only the exact default-port/path/query-free DigiCert HTTP host; retain rejection for all other HTTP URLs and add source contracts.
- Prevention rule: validate release-service transport assumptions against the provider's current primary documentation, then narrowly allowlist any protocol exception.
- Skill candidate: yes.
# 2026-07-28 - New WPF smoke script was not parsed before launch

- Symptom: the first system-footprint GUI smoke attempt stopped at parse time because Windows PowerShell 5.1 rejected line-leading `-and` operators inside an `if`.
- Wrong assumption: the multiline boolean style accepted by newer PowerShell syntax would parse unchanged under the repository's Windows PowerShell 5.1 smoke runner.
- Root cause: the conjunction operators were placed at the beginning of continuation lines.
- Detection method: `powershell -NoProfile -ExecutionPolicy Bypass -File .omx\gui-app-system-footprint-smoke.ps1` failed before starting the app.
- Fix: put conjunction operators at the end of continued expressions and run the parser gate before retrying GUI launch.
- Prevention rule: parse every newly created `.ps1` with the Windows PowerShell parser before any GUI or system-facing invocation.
- Skill candidate: yes.

# 2026-07-28 - Guessed the Release solution filename

- Symptom: `dotnet build OMNIX-Entropy.sln -c Release --no-restore` failed with MSB1009 because the file does not exist.
- Wrong assumption: the product name was also the solution filename.
- Root cause: the repository solution is `ComputerSecuritySoftware.slnx`.
- Detection method: the build refusal followed by `rg --files -g "*.sln" -g "*.slnx"`.
- Fix: use the observed `.slnx` path for the Release gate.
- Prevention rule: resolve solution/project paths with `rg --files` before required build commands when the path has not been observed in the current task.
- Skill candidate: no.

# 2026-07-28 - New footprint scanner missed Core namespace import

- Symptom: the first narrow `Css.Scanner` build failed with CS0246 for `SoftwareSystemFootprintKind`.
- Wrong assumption: the new scanner file would inherit the Core namespace import used by the adjacent inventory model file.
- Root cause: C# using directives are file-scoped; `WindowsSoftwareSystemFootprintScanner.cs` referenced the Core enum without `using Css.Core.Software`.
- Detection method: immediate `dotnet build src\Css.Scanner\Css.Scanner.csproj --no-restore`.
- Fix: add the exact Core namespace import to the scanner file.
- Prevention rule: after introducing a cross-project model, build the narrow consumer project before adding UI wiring.
- Skill candidate: no.
## 2026-07-22 - Local green suite masked an incomplete public checkout

- Symptom: the first GitHub Actions run failed 31 tests even though the same 1048-test suite passed locally before publication.
- Wrong assumption: a passing test run in the working tree proved that every runtime test dependency would be present in a fresh clone and that Debug tests were independent of Release output.
- Root cause: `.omx/*` ignored 22 smoke scripts still read by source-contract tests; those untracked local files masked the omission. The workflow also ran Debug tests before building Release binaries, lacked an LF checkout policy, and allowed pipe-sensitive tests to contend in parallel on the hosted Windows runner.
- Detection method: inspected failed run `29932623250`, correlated every FileNotFound/line-ending/production-authorizer/timeout failure with repository tracking, workflow order, and runner scheduling.
- Fix: allowlist top-level `.omx/*.ps1`, add `.gitattributes` with LF normalization, build Release before Debug tests, disable xUnit collection parallelization, and add repository contracts for all four conditions.
- Prevention rule: before public push, verify a source archive or clean clone made only from tracked files; CI must encode artifact prerequisites in order and portable source-text tests need an explicit line-ending policy.
- Skill candidate: yes.

## 2026-07-22 - CI ran debug-only worker tests against Release binaries

- Symptom: ten worker-lifecycle tests timed out or observed early child exit when run with `--configuration Release`.
- Wrong assumption: the entire suite was configuration-independent and should run against Release binaries.
- Root cause: lifecycle tests intentionally invoke the fake worker command, which is removed by `#if DEBUG`; the production Release worker correctly refuses it.
- Detection method: focused Release rerun plus inspection of `Css.Elevated/Program.cs` and the lifecycle launch arguments.
- Fix: CI runs the full suite in Debug, then performs a separate Release build; existing package tests continue to prove the fake command is absent from Release output.
- Prevention rule: preserve configuration-specific security boundaries when translating local gates into CI; test Debug-only harnesses in Debug and verify production exclusion separately.
- Skill candidate: yes

## 2026-07-22 - Parallel test commands contended on one build output

- Symptom: the focused worker-lifecycle rerun failed with `CS2012` because `Css.Tests.dll` was locked.
- Wrong assumption: separate filtered `dotnet test` commands could safely build the same test project in parallel.
- Root cause: both commands wrote the same `obj/Release` and output paths while the compiler server held the assembly.
- Detection method: compiler error named the locked test assembly and owning `VBCSCompiler` process.
- Fix: rerun solution tests serially; parallelize only read-only inspections that do not share build outputs.
- Prevention rule: never run build, test, publish, or format commands concurrently against the same solution/configuration.
- Skill candidate: yes

## 2026-07-22 - Privacy fixture replacement changed Agent intent

- Symptom: the full suite classified a C-drive question as application-specific after a privacy cleanup.
- Wrong assumption: replacing a numeric Windows username with `ExampleUser` would preserve the fixture's language semantics.
- Root cause: `Example` is meaningful application-like text to the Agent classifier, while the original numeric username was neutral.
- Detection method: full Release test failure in `C_drive_answer_uses_summary_but_hides_question_and_evidence_paths`.
- Fix: use the anonymous numeric username `10001`, which removes the real local identity without introducing application vocabulary.
- Prevention rule: privacy substitutions in classifier fixtures must use semantically neutral tokens and rerun the affected behavioral test.
- Skill candidate: no

## 2026-07-22 - SignTool readiness assumed the default SDK location

- Symptom: the prerequisite inspector reported no SignTool while seven valid copies existed under the registered D-drive Windows Kits root.
- Wrong assumption: checking PATH and `%ProgramFiles(x86)%\Windows Kits\10\bin` covered supported SDK installations.
- Root cause: the first implementation did not read the Windows Kits installer-owned `KitsRoot10` location.
- Detection method: Computer Use application inventory exposed a D-drive Windows SDK tool; bounded filesystem and registry reads confirmed the mismatch.
- Fix: read both exact standard installed-root keys, validate/deduplicate roots, and reuse bounded direct version/architecture enumeration.
- Prevention rule: for installed Windows SDK components, prefer installer-owned root metadata over assumptions about the system drive; keep explicit paths and bounded validation as fallbacks.
- Skill candidate: yes

## 2026-07-22 - Audit searches repeated two known shell hazards

- Symptom: one repository search used a malformed grouped regular expression, and a later Agent authority search used raw `rg` for an expected-zero result.
- Wrong assumption: the inline escaped group was balanced and an empty raw search was acceptable because sibling parallel reads would still return.
- Root cause: the audit batch did not apply the repository's existing regex and expected-zero search rules consistently.
- Detection method: ripgrep reported an unclosed group; review of the empty authority result identified the exit-code ambiguity.
- Fix: discarded the malformed search, used separate literal patterns, and reran the final Agent authority audit with an explicit exit-zero `AgentMutationAuthorityHits=0` result.
- Prevention rule: use separate `-e` arguments for complex Windows searches and always wrap expected-zero `rg` checks with explicit exit-code/count reporting before treating them as evidence.
- Skill candidate: no; the rules already exist in `AGENTS.md` and `skill-candidates.md`.

## 2026-07-22 - Code-signing eligibility initially omitted the public-key algorithm

- Symptom: the original prerequisite and candidate checks accepted any certificate with a private key and code-signing EKU.
- Wrong assumption: code-signing EKU plus a valid Authenticode result was sufficient for the intended Windows protection path.
- Root cause: certificate-purpose validation was implemented before checking the current Smart App Control algorithm limitation.
- Detection method: official Microsoft documentation audit while writing the beginner signing guide.
- Fix: require RSA OID `1.2.840.113549.1.1.1` during inspection and signing, record RSA in the manifest, and recheck both signatures plus manifest after transfer.
- Prevention rule: bind release cryptographic policy to the exact target Windows protection/distribution path and verify it at discovery, creation, and independent receipt.
- Skill candidate: yes

## 2026-07-22 - Continuation used a tool name that was no longer available

- Symptom: the first startup read failed because `tools.shell_command` was not available in the current tool set.
- Wrong assumption: a dynamically exposed tool from the previous continuation remained callable in this one.
- Root cause: enabled nested tools can change between turns; the current environment exposes `exec_command` instead.
- Detection method: the orchestration runtime returned `TypeError: tools.shell_command is not a function` before any command ran.
- Fix: switched immediately to `tools.exec_command` and completed all required startup reads.
- Prevention rule: inspect the current tool list after a continuation before reusing a dynamically discovered tool name.
- Skill candidate: no.

## 2026-07-22 - Installer audit repeated the forbidden wildcard path form

- Symptom: `rg` reported Windows error 123 for `tests\Css.Tests\Installer*` while the source-side search succeeded.
- Wrong assumption: appending a wildcard to a path argument was acceptable in a one-off audit.
- Root cause: PowerShell passed the wildcard path literally, exactly as the repository protocol warns.
- Detection method: `rg` emitted the invalid filename/directory error for the test path.
- Fix: subsequent searches used resolved directories and `-g` file filters or symbol searches only.
- Prevention rule: never place `*` or `?` in an `rg` path argument on Windows; use `-g` after a real directory.
- Skill candidate: no; this is already a repository rule.

## 2026-07-22 - Nested PowerShell diagnostic quoting failed twice

- Symptom: two ad hoc `powershell -Command` probes lost `$` variables and string quotes, producing parser/command errors instead of certificate evidence.
- Wrong assumption: single and double quoting across the outer shell and child Windows PowerShell would preserve the diagnostic expression.
- Root cause: the outer PowerShell expanded or stripped tokens before the child parser received them.
- Detection method: errors showed `.Exception` without `$_` and bare `Stop`/`COUNT` tokens.
- Fix: stopped nesting diagnostic logic, used direct shell reads, and added a reviewed child-process regression test for the actual script.
- Prevention rule: put nontrivial Windows PowerShell compatibility behavior in a repository script/test; do not debug it through nested inline parser layers.
- Skill candidate: yes.

## 2026-07-22 - Certificate filtering and empty sorting produced false readiness failures

- Symptom: the first report labeled a readable certificate store unreadable; after compatibility parsing, zero eligible certificates caused strict-mode `Count` failure.
- Wrong assumption: provider-specific `EnhancedKeyUsageList` was reliable in Windows PowerShell 5.1 and an empty pipeline remained an array after `Sort-Object`.
- Root cause: the certificate provider surface differs by host, and pipeline assignment converts zero output to null unless the sorted result is explicitly array-wrapped.
- Detection method: real Windows PowerShell JSON execution first returned `CertificateStoreReadable=false`, then failed at `$eligibleCertificates.Count` after the EKU correction.
- Fix: separate store enumeration from eligibility filtering, parse the X509 EKU extension directly, wrap the sorted result as an array, and add a child-process JSON test.
- Prevention rule: distinguish provider access from per-item parsing, use host-independent X509 structures for security decisions, and test empty collections under the oldest supported PowerShell host.
- Skill candidate: yes.

## Entry Template

### YYYY-MM-DD - Short title

- Symptom:
- Wrong assumption:
- Root cause:
- Detection method:
- Fix:
- Prevention rule:
- Skill candidate: yes/no

## Archived History

Entries before 2026-07-22 were moved verbatim to [error-ledger-archive-part1.md](archive/error-ledger-archive-part1.md), [error-ledger-archive-part2.md](archive/error-ledger-archive-part2.md).
