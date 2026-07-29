# Archived error-ledger (2026-07-15 to 2026-07-19)

Historical entries moved out of `.omx/development/error-ledger.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

## 2026-07-15 - Adjacent recommendation panel leaked the personal fixture path

- Symptom: the personal candidate list and Home Agent were path-free, but the right-side recommendation card displayed the full `C:\tmp\OMNIX-PersonalStorage-Smoke-*\Downloads` path.
- Wrong assumption: privacy checks scoped to the feature's target controls covered the full first-level experience.
- Root cause: the shared recommendation presenter copied `Recommendation.Finding` verbatim, and the GUI smoke never inspected visible text outside the candidate list/Agent response.
- Detection method: screenshot review of the mechanically passing GUI run.
- Fix: added shared recommendation-text sanitization that preserves operation evidence, plus a full visible-window path assertion.
- Prevention rule: privacy-sensitive GUI acceptance must inspect all visible text in the owning window and visually review adjacent panels; technical evidence may retain paths only behind an explicit secondary surface.
- Skill candidate: yes

## 2026-07-15 - Growth static test bound to a parameter terminator

- Symptom: the first post-change full regression failed one static test expecting the literal `previousSnapshots);` even though history was still passed correctly.
- Wrong assumption: the history argument would remain the final parameter of `DiskScanSessionBuilder.Build`.
- Root cause: personal-storage roots/options extended the method signature, so the correct argument now ends with a comma.
- Detection method: full regression after focused tests and GUI proof.
- Fix: assert `previousSnapshots,` so the test verifies propagation without depending on final argument position.
- Prevention rule: source-contract tests should assert semantic calls/arguments and ordering only when order is itself a contract; avoid punctuation tied to a temporary signature shape.
- Skill candidate: no

## 2026-07-15 - Install-report handoff was clickable but below the visible preview

- Symptom: the first extended GUI smoke reached the exact application drawer, but the candidate screenshot did not show the new `打开对应应用` button.
- Wrong assumption: an enabled UIAutomation element with a successful invocation proved the next step was visibly connected.
- Root cause: the button was placed after the preview lines, missing-evidence list, and safety text at the bottom of a long nested scroll surface; the smoke captured the title before locating the button.
- Detection method: manual inspection of `.omx/qa-install-diff-candidate-preview.png` after the mechanically passing run.
- Fix: moved the button directly below the Agent conclusion, required title and button to intersect the actual install-page viewport together, and switched the candidate capture to a window-only screenshot.
- Prevention rule: every beginner next-action smoke must prove the conclusion and its action are simultaneously visible before invoking the action; successful invocation alone is insufficient.
- Skill candidate: yes

## 2026-07-15 - AutomationId audit printed a false zero after rg parse failure

- Symptom: the first uniqueness audit printed `uniqueAutomationIds=0` even though `rg` had reported a regex parse error.
- Wrong assumption: the compound PowerShell command would propagate the failed search exit code after later formatting/output commands.
- Root cause: quoting corrupted the regex, and the final successful output expression masked the earlier native-command failure.
- Detection method: read the complete command output and rejected the contradictory zero instead of recording it as evidence.
- Fix: reran with .NET regex over explicit UTF-8 XAML reads; 265 AutomationIds were unique.
- Prevention rule: never accept a compound audit with any intermediate native-command error; prefer .NET regex for quoted XAML attributes in PowerShell.
- Skill candidate: yes

## 2026-07-15 - BY_HANDLE_FILE_INFORMATION used an incorrectly aligned FILETIME field

- Symptom: two immediate identity reads of an unchanged directory returned different creation times; normal application-cache quarantine was refused as stale.
- Wrong assumption: three native `FILETIME` fields could be represented by C# `long` fields inside `BY_HANDLE_FILE_INFORMATION` without changing structure alignment.
- Root cause: the CLR aligned `long` differently after the leading DWORD, shifting every later field; volume serial read as zero and timestamps were nonsensical.
- Detection method: a focused unchanged-directory service test compared complete evidence records and printed the differing native fields.
- Fix: represented `FILETIME` as its two original 32-bit fields and combined them after marshalling; directory length no longer participates as a varying filesystem-internal value.
- Prevention rule: mirror Win32 structure field layout exactly; do not replace nested native structs with same-size managed primitives unless packing/alignment is proven by tests.
- Skill candidate: yes

## 2026-07-15 - File id alone did not detect immediate same-name recreation

- Symptom: deleting and immediately recreating a confirmed file at the same path passed the first volume/file-id comparison on NTFS.
- Wrong assumption: a file id would never be reused quickly enough to matter inside one confirmation flow.
- Root cause: the filesystem reused the identifier; path, type, volume, and file id alone did not distinguish the new object in this fixture.
- Detection method: the whole-batch stale-candidate test unexpectedly moved both files instead of refusing before the first move.
- Fix: bound creation time, last-write time, and file length in addition to volume/file id and type.
- Prevention rule: destructive path freshness needs a compound identity and an explicit delete/recreate regression test; path plus one numeric id is insufficient evidence.
- Skill candidate: yes

## 2026-07-15 - Windows PowerShell parsed a UTF-8 smoke script with the legacy code page

- Symptom: the Agent GUI smoke failed at parse time with corrupted Chinese string literals and unterminated-string errors before the app started.
- Wrong assumption: a UTF-8 script without BOM would be decoded as UTF-8 by Windows PowerShell 5.1.
- Root cause: Windows PowerShell used the active legacy code page for the script file.
- Detection method: parser output showed mojibake inside the first Chinese assertion and cascading quote errors.
- Fix: kept the script source ASCII and built required Chinese input/assertions from explicit Unicode code points; added a static fixture-only contract test.
- Prevention rule: `.ps1` files that must run under Windows PowerShell 5.1 must be ASCII or carry an intentionally tested BOM; prefer ASCII plus code points for small UIAutomation assertions.
- Skill candidate: yes

## 2026-07-15 - Select-String replacement-character audit matched every line

- Symptom: an extra U+FFFD audit reported replacement characters across nearly every source line and timed out while printing thousands of false matches.
- Wrong assumption: `Select-String -SimpleMatch ([char]0xFFFD)` would bind the parenthesized char unambiguously as the pattern in this pipeline.
- Root cause: PowerShell parameter binding produced an invalid broad match for the intended scalar check.
- Detection method: ordinary ASCII XAML lines were reported as matches, contradicting strict UTF-8 decoding and visual inspection.
- Fix: discarded the output and used `.NET string.Contains([string][char]0xFFFD)` on each strictly decoded file; zero replacements were found.
- Prevention rule: use explicit .NET string checks for single control/replacement characters instead of relying on positional `Select-String` binding.
- Skill candidate: yes

## 2026-07-15 - Home C-drive fixture inherited D-volume capacity

- Symptom: the first Home screenshot showed exactly the same 69.3% used value for C and D during a fixture scan.
- Wrong assumption: overriding only the scan root changed content scope without changing the `DriveInfo` capacity source used by the scanner.
- Root cause: the fixture directory lived under the D-volume workspace, so the scanner correctly measured D capacity while the development UI continued labeling the logical target as C.
- Detection method: compared the first-view C and D rows in the real screenshot and traced the fixture path in smoke output.
- Fix: moved the unique scan fixture under confined `C:\tmp`, added static enforcement, and switched from desktop-wide to window-only screenshots.
- Prevention rule: any disk fixture used to prove a named-volume UI must live on that volume or inject explicit capacity evidence; always compare displayed volume metrics during screenshot review.
- Skill candidate: yes

## 2026-07-15 - Native MessageBox was absent from the expected UIAutomation child tree

- Symptom: the Agent troubleshooting smoke clicked the protected next step but reported that no confirmation window opened.
- Wrong assumption: every owned WPF/native dialog would appear as a direct child of the desktop UIAutomation root under the application process condition.
- Root cause: the native WPF `MessageBox` was discoverable by its top-level Win32 handle but not through the script's original child-tree query.
- Detection method: the product path was covered by focused tests, the click completed, and native handle enumeration found another top-level window for the same process.
- Fix: enumerate top-level handles for the exact application process and convert each candidate through UIAutomation.
- Prevention rule: native dialog smokes must support process-confined top-level handle discovery; do not infer that a missing UIAutomation child means the product did not open a dialog.
- Skill candidate: yes

## 2026-07-15 - Broad process-window matching produced a false GUI pass

- Symptom: the smoke claimed it found and cancelled the confirmation, but the captured image was only 262x71 and did not show the warning or buttons; the main screenshot also had transient compositor blanks.
- Wrong assumption: the first non-main top-level window of control type `Window` was necessarily the MessageBox, and immediate capture was visually stable.
- Root cause: the WPF process owns hidden/native helper windows, and the selector had no title constraint; capture happened before a stable redraw.
- Detection method: mandatory manual inspection of both generated screenshots contradicted the passing JSON and exposed the wrong bounds/content.
- Fix: require the exact `确认打开系统工具` title, bring the selected window forward, wait for redraw, capture before closing, and close via `WindowPattern.Close()` rather than locating a localized cancel button.
- Prevention rule: modal GUI acceptance must assert semantic identity such as exact title plus plausible screenshot content; same-process/control-type matching alone is not evidence, and a mechanical result cannot override visual inspection.
- Skill candidate: yes

## 2026-07-15 - Hardware probe used an ambiguous management options type

- Symptom: the first hardware implementation did not compile because `EnumerationOptions` resolved ambiguously and the selected `ManagementObjectSearcher` overload rejected string scope/query arguments.
- Wrong assumption: implicit usings would not collide with `System.Management.EnumerationOptions`, and a three-string/options overload existed.
- Root cause: `System.IO.EnumerationOptions` is also in scope; the available constructor requires `ManagementScope`, `ObjectQuery`, and management options.
- Detection method: focused test compilation reported `CS0104` and `CS1503` before any runtime probe ran.
- Fix: fully qualified `System.Management.EnumerationOptions` and constructed explicit `ManagementScope`/`ObjectQuery` values.
- Prevention rule: for APIs whose type names collide with implicit usings, inspect the exact target-framework overload and fully qualify the boundary type in the first implementation.
- Skill candidate: no

## 2026-07-15 - WMI hardware queries were denied in the restricted process

- Symptom: source/constructed tests passed, but the real-machine probe returned a null CPU name; direct CIM queries reported access denied.
- Wrong assumption: ordinary user WMI access would also be available inside the current restricted test process.
- Root cause: the WMI/CIM provider denied the sandboxed process even though fixed read-only hardware sources remained available.
- Detection method: a real-machine focused test required non-empty bounded CPU/GPU evidence; a read-only diagnostic command confirmed WMI denial and the fixed CPU registry value.
- Fix: retained bounded WMI as primary, added one fixed read-only CPU registry fallback and bounded `EnumDisplayDevices` GPU fallback, then reran the real-machine test successfully.
- Prevention rule: Windows observation features must test provider denial and have a least-privilege read-only fallback or report unavailable; never request elevation only to obtain diagnostic labels.
- Skill candidate: yes

## 2026-07-15 - Skill test expected the score in the wrong presentation layer

- Symptom: the first skill-catalog focus failed because it searched evidence lines for the overall score even though the score was deliberately in the main Agent answer.
- Wrong assumption: every asserted fact should be duplicated into the evidence list regardless of visible information hierarchy.
- Root cause: the test was written before the final plain-language response shape and encoded placement rather than meaning.
- Detection method: failure output showed a correct path-free disk evidence line and the score in the response answer.
- Fix: asserted the score in the main answer and kept the evidence list focused on dimension details.
- Prevention rule: UI presentation tests should assert one intentional beginner-visible location for a fact; do not force duplicate copy merely to satisfy a preimplementation assumption.
- Skill candidate: no

## 2026-07-15 - Safer route-memory gating invalidated an old static assertion

- Symptom: the first focused green run failed two product tests after the MSIX implementation compiled.
- Wrong assumption: existing static source contracts would accept route memory being narrowed to automatic/guided modes, and the new safety sentence would satisfy a phrase-level assertion regardless of word order.
- Root cause: one old test encoded `package.HasStableIdentity` as the complete enablement rule; the new copy separated `不会替你` from `安装` with another clause.
- Detection method: the 222-test focused run reported the exact source-contract and copy mismatches.
- Fix: updated the route-memory contract to require automatic/guided modes and rewrote the safety sentence as the direct promise `不会替你安装应用`.
- Prevention rule: when narrowing a visible enablement rule, search for dependent static contracts before the first green run; make beginner safety promises direct enough to test semantically.
- Skill candidate: no

## 2026-07-15 - Computer Use timed out before the WPF window appeared

- Symptom: two `launch_app` requests timed out and subsequent app discovery found no OMNIX target window; the process check was empty.
- Wrong assumption: updated antivirus definitions also meant the Windows UI helper would complete visible app launch.
- Root cause: the Computer Use helper timed out independently of compilation and antivirus state; no product crash or quarantine evidence was produced.
- Detection method: repeated helper timeout, empty `list_apps` target result, and empty `Css.App`/`Css.SmokeTools` process check.
- Fix: stopped all app input, did not use PowerShell UIAutomation/SendKeys as a workaround, and retained the visual gate as Warn.
- Prevention rule: after one bounded retry of a Computer Use launch timeout, stop and record visual proof as unavailable; never convert source tests into a screenshot claim.
- Skill candidate: no

## 2026-07-15 - Static source check used an unavailable PowerShell string overload

- Symptom: the first static gate printed a non-terminating `Contains` overload error before continuing to later output.
- Wrong assumption: Windows PowerShell would bind `string.Contains(string, StringComparison)` like the target .NET runtime used by the application.
- Root cause: the current PowerShell host did not expose that overload through its method binder, and the script did not force non-terminating errors to fail the command.
- Detection method: inspected the complete gate output instead of accepting the final success-looking lines.
- Fix: replaced the call with deterministic `IndexOf(string, StringComparison) -ge 0` and reran the whole static gate without errors.
- Prevention rule: static-gate output containing any PowerShell error is invalid even when exit code is zero; prefer broadly supported APIs or set explicit terminating error behavior.
- Skill candidate: no

## 2026-07-15 - Root-card test used an ambiguous display prefix

- Symptom: the first green run threw `Sequence contains more than one matching element` while locating the system `Windows` card.
- Wrong assumption: `StartsWith("Windows ")` uniquely identified the system card.
- Root cause: the same fixture intentionally contained `Windows Temp`, so both beginner display lines shared that prefix.
- Detection method: focused test failure and line-level inspection of the fixture selector.
- Fix: matched the semantic display boundary `Windows 占用`, which excludes `Windows Temp` without changing product code.
- Prevention rule: presentation tests with overlapping names must match a complete semantic delimiter or a model property, not a broad name prefix.
- Skill candidate: no

## 2026-07-15 - PowerShell quoting broke two exploratory ripgrep patterns

- Symptom: two read-only `rg` commands failed with an unclosed regex group while searching quoted XAML values and `$Recycle` text.
- Wrong assumption: backslash escaping inside a double-quoted PowerShell command would preserve quotes and dollar signs exactly as in a POSIX shell.
- Root cause: PowerShell removed/interpreted characters before `rg` received the pattern.
- Detection method: `rg` printed the malformed effective regex.
- Fix: reran with a single-quoted PowerShell regex and escaped only the regex dollar sign.
- Prevention rule: pass literal ripgrep patterns in PowerShell single quotes, especially when they contain `"` or `$`; do not translate POSIX escaping mechanically.
- Skill candidate: no

## 2026-07-15 - Action-only AutomationIds collided across root cards

- Symptom: a post-completion audit showed `程序和工具` and `软件数据` could both render `CDriveRootCauseAction_OpenCDriveApps` in the same UI tree.
- Wrong assumption: one AutomationId per action type was sufficient because the action behavior was identical.
- Root cause: AutomationIds identify controls, not behaviors; repeated card categories can legitimately share an action.
- Detection method: inspected the runtime binding cardinality after the static literal-id gate passed.
- Fix: appended a deterministic path-free SHA-256 prefix derived from the normalized visible top-level name and added same-summary uniqueness plus repeated-build stability tests.
- Prevention rule: data-template AutomationIds must be tested against a multi-item rendered model; static XAML uniqueness cannot prove runtime uniqueness.
- Skill candidate: yes

### 2026-07-15 - Source method helper hid the intended red assertion

- Symptom: the Agent hydration red test failed with `ArgumentOutOfRangeException` instead of reporting the missing async handler marker.
- Wrong assumption: the helper could search for the end marker before validating that the start marker was found.
- Root cause: `String.IndexOf(endMarker, start, ...)` received `start=-1`.
- Detection method: focused TDD red run and stack trace at `AutomaticAppInventoryLoadingTests.Method`.
- Fix: assert the start marker before performing the end-marker search.
- Prevention rule: source extraction helpers must validate each boundary before using it as an index.
- Skill candidate: yes

## 2026-07-16 - Static method audit matched call sites instead of definitions

- Symptom: the seven-method audit reported zero pipeline/attempt counts for methods already proven by focused tests, while only one uniquely named click handler looked correct.
- Wrong assumption: searching a bare method name would locate its definition before any invocation.
- Root cause: `IndexOf` found earlier call sites such as switch dispatch and restore routing, then balanced braces extracted the caller block.
- Detection method: the impossible zero counts contradicted current source tests and the direct `rg` pipeline locations.
- Fix: reject that table and rerun with complete declaration signatures including access modifier, return type, and parameter prefix.
- Prevention rule: balanced-brace source extraction anchors on a full declaration signature, never a bare symbol name.
- Skill candidate: yes

## 2026-07-16 - Static audit piped directly from PowerShell foreach

- Symptom: the final multi-method static audit stopped with `不允许使用空管道元素` before producing accepted evidence.
- Wrong assumption: Windows PowerShell would parse `foreach (...) { ... } | Format-Table` as a pipeline expression.
- Root cause: statement-form `foreach` cannot be piped directly in this host syntax.
- Detection method: parser error pointed at the pipe following the closing foreach brace.
- Fix: discard the batch and assign foreach output to `$rows` before piping `$rows` to formatting.
- Prevention rule: verification scripts collect statement-loop output in a variable before formatting or piping.
- Skill candidate: no

## 2026-07-16 - Startup audit repeated unvalidated method slicing

- Symptom: a read-only startup audit threw two PowerShell range exceptions because the guessed method signature was absent and the script still called `IndexOf`/`Substring` with `-1`.
- Wrong assumption: the startup execution method would be named `ExecutePendingStartupControlAsync` based on nearby action terminology.
- Root cause: symbol discovery and dependent extraction were combined without validating the discovered start.
- Detection method: PowerShell reported negative `startIndex`; a subsequent symbol search and strict UTF-8 line read found the actual `ReviewAndExecutePendingStartupDisableAsync` method.
- Fix: discarded the failed output, resolved the real symbol first, and read the observed line range separately.
- Prevention rule: shell source slicing validates `start >= 0` before any dependent search/subsequence and never guesses a method signature from UI terminology.
- Skill candidate: yes

## 2026-07-16 - App-cache source extractor searched from negative start

- Symptom: the intended cache-synchronization RED test threw `ArgumentOutOfRangeException` instead of reporting the missing helper method.
- Wrong assumption: the local `Extract` helper validated its start marker before using it to find the end marker.
- Root cause: `IndexOf(endMarker, start, ...)` ran while `start == -1`; assertions were ordered after both searches.
- Detection method: the focused test stack pointed to the helper's end-marker `IndexOf` call.
- Fix: assert a nonnegative start immediately, then search/assert the end marker.
- Prevention rule: every source-extraction helper validates each boundary before using it as an index; prefer the planned balanced-brace helper over copied local slicers.
- Skill candidate: yes

## 2026-07-16 - Audit regex was corrupted by PowerShell quoting

- Symptom: the first critical-entry audit batch failed with an `rg` regex parse error and discarded the required current/handoff/status reads in that batch.
- Wrong assumption: one double-quoted alternation containing escaped XAML quotes would survive JavaScript, PowerShell, and ripgrep parsing unchanged.
- Root cause: quoting layers transformed the pattern into invalid escape sequences.
- Detection method: `rg` printed `unrecognized escape sequence` and the orchestrated batch returned nonzero.
- Fix: reran the audit with separate single-quoted `-e` patterns and treated the failed batch as no evidence.
- Prevention rule: PowerShell repository searches that include quoted XAML fragments use multiple `-e` arguments; do not combine them into one escaped alternation.
- Skill candidate: yes

## 2026-07-16 - Source contract became falsely green after inserting helper methods

- Symptom: installer coordinator tests passed even though assertions intended for `PrepareInstaller_Click` were actually satisfied by newly inserted methods that followed it.
- Wrong assumption: `CaptureBeforeInstall_Click` would remain the next method and therefore a stable extraction boundary.
- Root cause: the source contract sliced from the prepare signature to an old downstream method name instead of the immediate current method boundary.
- Detection method: manual review showed `while (true)` and catalog binding had moved to `PresentInstallerExecutionResultsAsync`, but the prepare-scope test still passed.
- Fix: extract Prepare and the shared presenter separately using their current adjacent signatures and assert each responsibility in its real method.
- Prevention rule: source-order contracts must use the immediate next observed signature; when inserting methods, search every test that slices to the displaced signature.
- Skill candidate: yes

## 2026-07-16 - XAML check used the Windows PowerShell default code page

- Symptom: an independent XML cast reported a malformed Chinese attribute even though WPF compilation and strict UTF-8 decoding had succeeded.
- Wrong assumption: `Get-Content -Raw` would preserve a UTF-8 XAML file under the current Windows PowerShell host.
- Root cause: the host decoded the file through the system code page, corrupting Chinese bytes before XML parsing.
- Detection method: the displayed attribute text was mojibake; a rerun with strict `UTF8Encoding(false, true)` parsed successfully.
- Fix: read XAML with `File.ReadAllText(path, strictUtf8)` before independent XML validation.
- Prevention rule: all non-ASCII source/XAML static checks must use an explicit strict UTF-8 decoder, never PowerShell's default `Get-Content` decoding.
- Skill candidate: yes

## 2026-07-16 - Multi-file recovery patch omitted a file boundary

- Symptom: `apply_patch` tried to find a test assertion inside `error-ledger.md` and rejected the entire patch.
- Wrong assumption: the transition from the ledger entry to the test correction had a new `Update File` header.
- Root cause: one multi-file patch section marker was omitted while composing a large edit.
- Detection method: the patch error named `error-ledger.md` and printed the test-only expected line; no file was changed.
- Fix: split the ledger/test correction from production edits and include an explicit file header for each target.
- Prevention rule: use small patches for protocol records, tests, and production code; verify every target transition has its own `Update File` header.
- Skill candidate: no

## 2026-07-16 - String assertion used a collection-only method

- Symptom: the intended installer-recovery RED build included CS1061 because `StringAssertions` has no `ContainSingle` method.
- Wrong assumption: the collection assertion name was also available for counting substring occurrences.
- Root cause: the source contract was written without checking the existing FluentAssertions string API.
- Detection method: the compiler reported the test-only API error alongside the expected missing production members.
- Fix: assert that splitting on the exact coordinator call yields two segments, which proves one occurrence.
- Prevention rule: use an explicit count/split when a source contract needs exact substring cardinality; do not infer assertion API parity across types.
- Skill candidate: no

## 2026-07-16 - Installer recovery test patch trusted a truncated method name

- Symptom: `apply_patch` rejected the test edit because the expected interrupted-wait method name was not present.
- Wrong assumption: a method signature reconstructed from a truncated command result was accurate enough to use as a patch anchor.
- Root cause: the omitted middle of the long test-file output hid the actual `Interrupted_installer_wait...` name.
- Detection method: `apply_patch` failed before changing any file; an exact `rg -n` symbol search revealed the real signature.
- Fix: use the observed method name and small surrounding reads as patch anchors, then rerun the patch.
- Prevention rule: never use text from a truncated output as an exact patch anchor; resolve the symbol with `rg -n` first.
- Skill candidate: no

## 2026-07-16 - Installer audit passed guessed module directories to ripgrep

- Symptom: the installer symbol search returned exit code 1 because `src/Css.Core/Install` and `src/Css.Core/Installer` do not exist.
- Wrong assumption: likely namespace names were safe to use as repository directory arguments.
- Root cause: search scope was inferred from type naming instead of using the observed `src` root with `-g` filters.
- Detection method: ripgrep printed both missing-directory errors and a nonzero exit code after partial matches.
- Fix: discarded the batch as complete evidence and reran against existing roots only.
- Prevention rule: symbol discovery uses an observed broad root plus `-g`; narrowing to module directories is allowed only after `rg --files` has shown them.
- Skill candidate: yes

## 2026-07-16 - Uninstall audit repeated a guessed-path batch read

- Symptom: a batch found `OfficialUninstallOperationHandler` under `src/Css.Elevated/Uninstall` but still failed because a later `Get-Content` had already guessed a nonexistent Core path.
- Wrong assumption: placing discovery and a speculative dependent read in one shell batch was equivalent to resolving the path first.
- Root cause: command arguments are fixed before the symbol-search result is observed; the later read could not use that result.
- Detection method: exit code 1 and the explicit missing Core path after the real Elevated path appeared in earlier output.
- Fix: discarded the failed batch as complete evidence and reran the read against the observed `src/Css.Elevated/Uninstall/OfficialUninstallOperationHandler.cs` path.
- Prevention rule: path discovery and dependent required reads must be separate tool calls unless the path is already observed; never pre-compose a guessed dependent path in the discovery batch.
- Skill candidate: yes

## 2026-07-16 - Startup restore audit guessed a nonexistent timeline model filename

- Symptom: a read-only batch returned exit code 1 after `Get-Content` targeted `ActionTimelineModels.cs`, even though earlier commands in the batch printed useful output.
- Wrong assumption: the timeline model filename could be inferred from its type name without resolving the repository path first.
- Root cause: `ActionTimelineEntry` is defined in `ActionTimelineEntry.cs`; the batch mixed verified and guessed paths.
- Detection method: command exit code 1 and the missing-path branch at the end of the output.
- Fix: resolved the definition with symbol search and `rg --files`, then discarded the failed batch as complete evidence.
- Prevention rule: required batch reads may contain only observed paths; resolve every inferred filename with `rg --files` or symbol search before including it.
- Skill candidate: no

## 2026-07-16 - Wildcard-path failure repeated after skill-candidate promotion

- Symptom: the quarantine-restore evidence batch failed on `tests\\Css.Tests\\Quarantine*Tests.cs`, discarding four required reads.
- Wrong assumption: adding a skill candidate and commentary reminder was enough to prevent the same command shape in the immediately following audit.
- Root cause: the unsafe path-glob habit remained in the command template and had not yet been promoted to an enforced repository rule.
- Detection method: `rg` returned OS error 123; only the command that ran before the failure produced output.
- Fix: add an explicit `AGENTS.md` repository-search rule and rerun with `rg -g "Quarantine*Tests.cs" ... tests\\Css.Tests`.
- Prevention rule: repository search commands are invalid if a path argument contains `*` or `?`; use `-g` exclusively.
- Skill candidate: yes; the project rule is now promoted, while a reusable helper remains warranted.

## 2026-07-16 - Windows path wildcard rule was violated again

- Symptom: the first background-summary audit batch failed on `tests\\Css.Tests\\App*Tests.cs` and `Agent*Tests.cs`, discarding parallel read results.
- Wrong assumption: the earlier recorded Windows wildcard rule would be remembered without changing the command template.
- Root cause: filename filtering was again placed inside a Windows path instead of using `rg -g` against the test directory.
- Detection method: `rg` returned OS error 123 for both literal wildcard paths.
- Fix: rerun with `rg -g "App*Tests.cs" -g "Agent*Tests.cs" ... tests\\Css.Tests`.
- Prevention rule: all repository filename filters must use `rg -g`; raw `*` is forbidden inside Windows path arguments.
- Skill candidate: yes; repeated failure warrants a reusable PowerShell-safe repository search helper.

## 2026-07-16 - Count-sum assertion missed grouping parentheses

- Symptom: the intended RED build also reported CS0201 on the ownership-count assertion.
- Wrong assumption: the multiline arithmetic expression would bind to the following FluentAssertions `.Should()` call as a whole.
- Root cause: member access bound only to the final `Count`, leaving the preceding additions as an invalid statement expression.
- Detection method: the first focused RED build reported CS0201 alongside the expected missing product type/fields.
- Fix: parenthesize the complete sum before calling `.Should()`.
- Prevention rule: wrap arithmetic aggregates in parentheses before fluent assertion chains.
- Skill candidate: no.

## 2026-07-16 - Source filename inference rule was violated again

- Symptom: the first C-drive ownership audit batch lost its successful reads because `AppPresentationBuilder.cs` was requested at a guessed path that does not exist.
- Wrong assumption: the recently used public type still had a same-named file under the guessed folder.
- Root cause: the earlier prevention rule was recorded but not applied before composing the parallel batch; the type is declared in `AppPresentation.cs`.
- Detection method: `rg` reported the missing path; a symbol search located the declaration at `src\\Css.Core\\Apps\\AppPresentation.cs`.
- Fix: resolve every unobserved file with `rg --files` or a symbol search before placing it in a required-read batch.
- Prevention rule: guessed source paths are forbidden in required parallel reads; the path-discovery result must precede the read in a separate command.
- Skill candidate: yes; this repeated agent-behavior failure should be promoted into a reusable repository-navigation check.

## 2026-07-16 - Static audit used invalid pipeline syntax and stale source paths

- Symptom: the first audit command failed on a `foreach` pipeline parse error; the corrected command then mixed missing-path errors into match counts because two health builders were assumed to live under `Css.Core`.
- Wrong assumption: PowerShell would accept a statement-level `foreach` directly before a pipeline, and type names were enough to infer current source folders.
- Root cause: results were not collected before formatting, and the builders actually live under `src\\Css.Scanner\\Experience`.
- Detection method: PowerShell reported the empty-pipeline parser error and `Select-String` reported the missing paths; `rg --files` located the actual files.
- Fix: collect result objects before piping, locate files from the repository list, then rerun all checks. Final evidence is 323 strict UTF-8 files, authority hits 0, and both legacy-pattern counts 0.
- Prevention rule: resolve unobserved file paths with `rg --files` before static audits, and make audit scripts fail on missing targets instead of treating diagnostic output as a count.
- Skill candidate: yes; fold both constraints into the existing static absence-checker candidate.

## 2026-07-16 - Expected zero-hit `rg` was repeated inside a fail-fast batch

- Symptom: the final static-check batch discarded successful UTF-8 and authority outputs because the expected-zero legacy search returned `rg` exit code 1.
- Wrong assumption: recording the earlier prevention rule was enough without changing the command shape in the next batch.
- Root cause: an optional absence check was still grouped with required successful reads under fail-fast orchestration.
- Detection method: the batch returned only exit code 1 with no retained successful outputs; rerunning with regex counts produced all expected zeros.
- Fix: replace expected-absence `rg` calls with explicit regex match counts when they share a batch, or run them separately.
- Prevention rule: never put a raw expected-zero `rg` call in a fail-fast parallel batch; use a count command whose successful zero result exits 0.
- Skill candidate: yes; add a reusable static absence-count helper candidate.

## 2026-07-16 - Startup policy file location was inferred from the type name

- Symptom: a required-read batch failed because `StartupEntryControlPolicy.cs` did not exist at the guessed path.
- Wrong assumption: the public policy type lived in a same-named source file.
- Root cause: the type is declared in `StartupEntryControl.cs`.
- Detection method: `Get-Content` reported the missing path; symbol search found the declaration immediately.
- Fix: locate types with `rg -n "class <Type>"` before reading when the exact file has not been observed.
- Prevention rule: do not infer source filenames from public type names in this repository.
- Skill candidate: no.

## 2026-07-16 - Windows wildcard path caused an avoidable `rg` failure

- Symptom: a parallel read batch returned no retained snippets because one `rg` command rejected `tests\\Css.Tests\\*Agent*Tests.cs` as an invalid Windows path.
- Wrong assumption: `rg` would expand an embedded Windows path wildcard like a Unix shell glob.
- Root cause: PowerShell passed the wildcard path literally; `rg` expects file filtering through `-g` or a directory search root.
- Detection method: `rg` returned OS error 123 and the parallel batch surfaced the nonzero command.
- Fix: search the directory and use `-g "*Agent*Tests.cs"`, or omit the optional no-hit search from a fail-fast parallel read batch.
- Prevention rule: use `rg -g` for filename patterns on Windows and keep optional no-hit searches separate from required reads.
- Skill candidate: no.

## 2026-07-16 - Homepage authority test depended on incidental ordering and a stale method boundary

- Symptom: the focused homepage migration-closure suite had two failures after the product behavior compiled: the protected historical item was not at index 1, and the static source extractor could not find its end marker.
- Wrong assumption: read-only findings with different observation timestamps would retain input order, and `EnsureHealthScanLoadedAsync` followed `RefreshHealthSummaryFromBase` in the current MainWindow source.
- Root cause: the presenter intentionally sorts equal-authority records by timestamp, while the health-load helper appears before the refresh method and cannot be a forward extraction boundary.
- Detection method: the focused 2/4 test result showed the unavailable record at index 1 and an end-marker index of `-1`; symbol inspection showed the actual neighboring method.
- Fix: locate protected and unavailable findings by their typed presentation semantics while still asserting reviewable priority, and end the refresh extraction at `TrySaveHealthDigestAsync`.
- Prevention rule: collection tests should assert only contractual ordering tiers, and source-extraction tests must use freshly inspected forward method boundaries.
- Skill candidate: no.

## 2026-07-16 - Homepage closure test used unsupported expression-tree syntax

- Symptom: the first focused test build failed for both the expected missing disposition type and two unrelated test compilation errors.
- Wrong assumption: `RiskLevel` was in the already imported recommendation namespace, and FluentAssertions `OnlyContain` accepted C# `is null` pattern syntax in its expression tree.
- Root cause: `RiskLevel` is under `Css.Core.Operations`; expression trees in this target do not support the pattern-matching operator.
- Detection method: compiler errors identified the missing namespace and CS8122 expression-tree limitation.
- Fix: import `Css.Core.Operations` and use `TargetAppName == null` inside the predicate.
- Prevention rule: compile new cross-namespace behavior tests once before interpreting a red result as product-only; use expression-tree-compatible comparisons in FluentAssertions collection predicates.
- Skill candidate: no.

## 2026-07-16 - Restore refactor left a static test on removed method boundaries

- Symptom: the related restore regression failed with `ArgumentOutOfRangeException` while extracting a removed `RestoreTimelineItemAsync` method.
- Wrong assumption: the focused quarantine restore tests covered every source-contract test affected by splitting the restore dispatcher into startup and quarantine methods.
- Root cause: `StartupControlExperienceTests` still searched for the old method boundaries, and its extraction helper used the missing start index before validating it.
- Detection method: the 227-test related regression exposed the obsolete marker and misleading helper failure.
- Fix: point the contract at `RestoreTimeline_Click`, which owns the startup/quarantine dispatch, and validate the start marker before searching for the end marker.
- Prevention rule: after renaming or splitting a UI workflow method, search all static source contracts for the old symbol before the related regression; extraction helpers must validate boundaries before using them as indexes.
- Skill candidate: no
## 2026-07-16 - Ripgrep audit used a wildcard path argument

- Symptom: the read-only failure-boundary audit stopped with Windows OS error 123.
- Wrong assumption: a filename wildcard could be placed in the `tests\\Css.Tests\\BeginnerVisible*` path argument.
- Root cause: PowerShell passed the wildcard path literally to `rg`, contrary to the repository search rule.
- Detection method: `rg` reported the invalid path before any search result was accepted.
- Fix: discard the failed batch and rerun with the directory as a plain path plus `-g "*Failure*Tests.cs"` / `-g "*Boundary*Tests.cs"` filters.
- Prevention rule: every `rg` file filter on Windows belongs in `-g`; path arguments must be resolved literal directories or files.
- Skill candidate: no

## 2026-07-16 - Expected-zero ripgrep invalidated a parallel read batch

- Symptom: the corrected audit batch still failed without returning the successful source reads.
- Wrong assumption: the final failure-boundary search would necessarily find a matching source contract.
- Root cause: an expected-zero `rg` returned exit code 1 inside the required batch, so the orchestration rejected the batch and discarded its other outputs.
- Detection method: the batch ended at exit code 1 with no accepted output after the wildcard path had already been corrected.
- Fix: rerun required source reads separately from a count-form optional search whose zero result exits successfully.
- Prevention rule: optional or expected-zero searches must use an exit-zero count form and must not share a failure-sensitive batch with required reads.
- Skill candidate: no; this is already an explicit repository rule.

## 2026-07-16 - New one-shot contracts encoded the pre-recovery scan call

- Symptom: the combined one-shot/recovery focused run passed the new recovery contract but failed the two earlier one-shot contracts.
- Wrong assumption: those contracts could require the parent methods to call `ScanSoftwareProfilesAsync` directly.
- Root cause: the next safety step intentionally moved that read behind `TryScanSoftwareProfilesAfterProductionAttemptAsync`, making the direct-call assertion obsolete while preserving the required rescan.
- Detection method: focused failures showed the full extracted parent methods using the new helper in the correct post-attempt position.
- Fix: require the shared recovery helper in the one-shot contracts; keep direct read-only scanner authority asserted inside the helper contract.
- Prevention rule: workflow contracts should assert the semantic recovery boundary; only the boundary's own test should assert its low-level scanner dependency.
- Skill candidate: no

## 2026-07-16 - Combined test patch relied on copied truncated context

- Symptom: one combined `apply_patch` for the two old production-rescan contracts was rejected before changing either file.
- Wrong assumption: the copied output fragment was exact enough to use as one large multi-file patch context, including the mojibake assertion line.
- Root cause: at least one migration hunk did not match the file byte-for-byte, so atomic patch verification refused the whole edit.
- Detection method: `apply_patch` named the missing migration block and no diff was produced.
- Fix: read both exact method sections and apply small ASCII-anchored patches separately.
- Prevention rule: for files containing display text with encoding-sensitive output, anchor patches on stable ASCII symbols and split unrelated files into independent hunks.
- Skill candidate: no

## 2026-07-16 - Full regression guessed an unobserved solution filename

- Symptom: the first full-regression command stopped immediately with MSBuild error MSB1009.
- Wrong assumption: the repository solution was named `OMNIX-Entropy.sln`.
- Root cause: the actual solution is `ComputerSecuritySoftware.slnx`, and the path had not been resolved before use.
- Detection method: MSBuild reported the missing project file before any test ran; `rg --files -g "*.sln" -g "*.slnx"` found the real path.
- Fix: discard the failed command and rerun against the observed `.slnx` file.
- Prevention rule: resolve solution/project paths with `rg --files` in the current worktree before using them in required verification commands.
- Skill candidate: no; this is already an explicit repository rule.

## 2026-07-16 - Multi-record patch used misdecoded UTF-8 context

- Symptom: the first protocol-record completion patch was rejected at `reflections.md`, so none of its six files changed.
- Wrong assumption: PowerShell's default `Get-Content` rendering was safe to copy back as patch context for UTF-8 Chinese records.
- Root cause: the displayed mojibake did not match the file's actual UTF-8 text, and the large atomic patch made one mismatch reject every record update.
- Detection method: `apply_patch` reported the missing reflection line and later `Get-Content -Encoding utf8` showed the correct Unicode text.
- Fix: reread records with explicit UTF-8 and update each file with a small independent patch.
- Prevention rule: always use explicit UTF-8 when reading multilingual protocol records and never couple all required records to one encoding-sensitive patch.
- Skill candidate: no

## 2026-07-16 - MainWindow WPF test omitted application resources

- Symptom: the first application-search red test failed in `MainWindow.InitializeComponent` before reaching the search controls.
- Wrong assumption: `MainWindow` could be constructed in isolation like the smaller dialog windows.
- Root cause: its XAML depends on styles owned by `App.xaml`, including `NavButtonStyle`; the test did not create a WPF application resource scope.
- Detection method: the inner XAML parse exception named the missing resource at the first navigation button.
- Fix: replace the process-wide WPF fixture with structural XAML assertions plus a precise text-change handler contract; reserve whole-window proof for the existing app smoke path.
- Prevention rule: before instantiating a top-level WPF window in tests, inspect whether it depends on application resources and avoid introducing a process-wide `Application` singleton casually.
- Skill candidate: no

## 2026-07-16 - Optional WPF fixture search repeated expected-zero batch failure

- Symptom: the first App-resource inspection batch returned no accepted output.
- Wrong assumption: the repository necessarily contained an existing `new App()`/resource-dictionary test fixture.
- Root cause: the optional `rg` returned exit code 1 beside a required `App.xaml` read, repeating the expected-zero orchestration mistake.
- Detection method: the batch failed without output; the count-form rerun returned zero existing fixtures.
- Fix: run the required file read independently and use an exit-zero count form for optional fixture discovery.
- Prevention rule: never batch repository discovery that may be empty with a required source read; use count form first.
- Skill candidate: no; the rule already exists in `AGENTS.md` and earlier ledger entries.

## 2026-07-16 - XAML attached-property test used the wrong XML name model

- Symptom: the first green search-placeholder run found the controls but reported a null AutomationId.
- Wrong assumption: `AutomationProperties.AutomationId` would appear in `XDocument` with local name `AutomationId`.
- Root cause: WPF property-element syntax keeps the dotted attached-property name as the unqualified XML local name.
- Detection method: 4/5 related assertions passed and the failure was isolated to the test helper.
- Fix: match the stable `.AutomationId` suffix in the structural parser.
- Prevention rule: structural XAML helpers must account for dotted attached-property names rather than assuming XML namespace qualification.
- Skill candidate: no

## 2026-07-16 - Computer Use skill locator version was stale

- Symptom: the first mandatory skill read failed because the system-listed cached version directory no longer existed.
- Wrong assumption: the provided cache locator would remain current for the whole session.
- Root cause: the installed computer-use plugin had advanced from version directory `26.707.72221` to `26.707.91948`.
- Detection method: `Get-Content` reported the missing path; `rg --files` found exactly one current `SKILL.md` under the plugin root.
- Fix: resolve and read the current skill path before bootstrapping Windows control.
- Prevention rule: treat versioned plugin cache locators as hints and resolve the current file under the named plugin root before required use.
- Skill candidate: no

## 2026-07-16 - Debug application launch timed out again

- Symptom: Computer Use was reachable and listed apps, but launching the built `Css.App.exe` timed out; a single passive follow-up found no OMNIX app or window.
- Wrong assumption: a healthy lightweight Computer Use call implied local Debug-app launch would complete.
- Root cause: the Windows helper timed out specifically at `launch_app`, independently of build/test success and updated antivirus definitions.
- Detection method: launch timeout followed by empty filtered `list_apps` and `list_windows` results after a two-second wait.
- Fix: stop app input after the passive poll, retain visual acceptance as Warn, and rely only on the accepted XAML/handler tests for this slice.
- Prevention rule: after one local-app launch timeout and one passive poll, stop; do not retry input or fall back to PowerShell UI automation/SendKeys.
- Skill candidate: no

## 2026-07-16 - Full-history agent fork specified an incompatible agent type

- Symptom: the first read-only audit spawn was rejected before either audit started.
- Wrong assumption: a full-history `fork_context` request could also specify the explorer agent type.
- Root cause: the orchestration API treats full-history fork and explicit agent-type selection as incompatible options.
- Detection method: the spawn call returned an argument-validation failure.
- Fix: retry the two read-only audits without full-history fork and pass the required scope explicitly.
- Prevention rule: choose either full-history fork or an explicit agent type according to the tool contract; do not combine them.
- Skill candidate: no

## 2026-07-16 - Post-scan red fixtures omitted a required summary

- Symptom: the first intended-red build produced unrelated required-member compiler errors for every `OfficialUninstallPostScanResult` fixture.
- Wrong assumption: the new focused fixtures could omit presentation-only result fields.
- Root cause: `Summary` is a required member even when the test only inspects typed action mapping.
- Detection method: compiler diagnostics pointed to each object initializer before the intended missing-action API errors.
- Fix: give every fixture a fixed non-sensitive summary and rerun the red test.
- Prevention rule: construct test inputs from the complete public contract before interpreting compiler failures as the intended red state.
- Skill candidate: no

## 2026-07-16 - MainWindow action wiring missed the uninstall namespace

- Symptom: the first green compile could not resolve `OfficialUninstallPostScanAction` in `MainWindow`.
- Wrong assumption: an existing broad Core import already covered the enum's namespace.
- Root cause: the enum lives in `Css.Core.Uninstall`, which MainWindow had not imported.
- Detection method: the focused build failed with two `CS0103` diagnostics at the action switch.
- Fix: add the explicit `using Css.Core.Uninstall;` import and rerun the same test set.
- Prevention rule: after adding a cross-namespace type to a large window, run the narrow compile immediately before reasoning about behavior failures.
- Skill candidate: no

## 2026-07-16 - Residue static test spanned adjacent methods

- Symptom: one related product test compared a build call in the new read-only retry helper with a rescan call in the later mutation-capable review method.
- Wrong assumption: extracting from the selected-handler marker to a later display helper still isolated one workflow after inserting another helper.
- Root cause: the old range was positional and crossed adjacent method boundaries; the first correction also omitted the extractor's required `()` declaration suffix.
- Detection method: the ordering assertion failed despite focused behavior tests, then `SourceMethodExtractor` rejected the incomplete declaration prefix.
- Fix: extract the selection handler and `ReviewUninstallResidueAsync` independently with full declarations.
- Prevention rule: workflow authority/order tests must use balanced per-method extraction and full declaration prefixes, never broad source ranges.
- Skill candidate: no; the shared extractor already enforces this rule.

## 2026-07-16 - Multilingual presenter patch used escaped-text context

- Symptom: the first combined personal-storage implementation patch was rejected before writing any file.
- Wrong assumption: the presenter stored its Chinese copy as `\u` escapes like several neighboring App files.
- Root cause: the UTF-8 source contains direct Chinese text, so the escaped context did not match; coupling new files to that edit made the entire atomic patch fail.
- Detection method: `apply_patch` named the missing Agent/safety lines; an explicit UTF-8 read showed the actual direct text.
- Fix: add model fields with ASCII structural anchors, insert evidence through a helper, and add each new file in smaller patches.
- Prevention rule: inspect multilingual source with explicit UTF-8 before using text copy as patch context, and do not couple independent file additions to an encoding-sensitive hunk.
- Skill candidate: no

## 2026-07-16 - Inspection list depended on post-show binding completion

- Symptom: the first WPF detail-window test selected a second captured path but the returned request stayed null.
- Wrong assumption: assigning `DataContext` guaranteed the list binding had populated before the window was shown.
- Root cause: the test exercised the constructor-stage UI tree, before the binding engine completed item population.
- Detection method: all launcher/presenter/wiring checks passed while only the unshown-window selection assertion failed.
- Fix: initialize the critical paths `ItemsSource` directly from the same view model in the constructor, then select the first item deterministically.
- Prevention rule: controls used as immediate intent/automation boundaries must have deterministic constructor-stage content, not rely solely on post-show binding timing.
- Skill candidate: no

## 2026-07-16 - Digest patch assumed a nonexistent field anchor

- Symptom: the first digest hydration patch was rejected before changing XAML or source.
- Wrong assumption: MainWindow had an `_isLoadingTimeline` field suitable as a nearby boolean-state anchor.
- Root cause: timeline loading is represented by a gate rather than that guessed field.
- Detection method: `apply_patch` reported the missing field line; a direct first-100-lines read showed the actual state layout.
- Fix: insert digest state beside `_lastHealthSummary` and patch XAML/handler separately.
- Prevention rule: observe the exact field block before adding state to a large class; never use a remembered or inferred field name as patch context.
- Skill candidate: no

## 2026-07-16 - PowerShell regex quoting broke the field inspection command

- Symptom: the first follow-up source-read command failed in PowerShell before reading either file.
- Wrong assumption: backslash-escaped quotes inside a PowerShell double-quoted regex would behave like shell/C# escaping.
- Root cause: PowerShell parsed the embedded quote as syntax and treated the remaining Chinese pattern as an expression.
- Detection method: parser error pointed to the quoted `C 盘证据` pattern.
- Fix: use a single-quoted regex argument and rerun the read.
- Prevention rule: in PowerShell, wrap literal `rg` patterns containing double quotes in single quotes rather than backslash-escaping them.
- Skill candidate: no

## 2026-07-16 - Digest source checks mixed rendered Chinese and escaped source text

- Symptom: the first focused source test and first static order script could not find the success copy despite correct compiled behavior.
- Wrong assumption: MainWindow stored the new copy as direct Chinese and the PowerShell literal needed doubled backslashes.
- Root cause: the source uses one literal `\u` escape sequence per character; rendered text, C# assertion strings, and PowerShell single-quoted strings represent that sequence differently.
- Detection method: related tests compiled and only the source substring index was `-1`; the corrected static indices were readiness 717 and success 1166.
- Fix: assert the actual escaped source representation and rerun the order check with one literal backslash in PowerShell.
- Prevention rule: source-text tests must match storage representation, not rendered UI text; inspect one actual line before writing cross-language escape assertions.
- Skill candidate: no

## 2026-07-16 - Agent background discovery used a wildcard path argument

- Symptom: the first broad background/catalog discovery command ended with Windows OS error 123 after producing truncated partial output.
- Wrong assumption: `tests\Css.Tests\AppCatalog*` would be expanded as a filename filter.
- Root cause: PowerShell passed the wildcard path literally to `rg`; repository protocol requires `-g` for filename patterns.
- Detection method: `rg` reported the invalid literal path and the command exited 1.
- Fix: rerun targeted reads with observed file paths and `rg -g "*AppCatalog*.cs" -g "*Background*.cs"`.
- Prevention rule: on Windows, never place `*` or `?` in an `rg` path argument; use `-g` even for one directory.
- Skill candidate: no; this rule already exists in `AGENTS.md`.

## 2026-07-16 - Agent handoff patches reused mixed-copy anchors

- Symptom: two combined Core/App patches were rejected before writing because expected direct/escaped Chinese lines did not match actual storage.
- Wrong assumption: nearby Agent conversation and MainWindow copy followed one consistent storage style.
- Root cause: these large files mix direct UTF-8 Chinese and `\u` escapes; coupling independent XAML/model/handler edits made one text mismatch reject all hunks.
- Detection method: `apply_patch` named the missing startup reply and final status lines; explicit UTF-8 reads showed their actual representation.
- Fix: split model, reply, XAML, per-item handler, and aggregate handler edits; use structural ASCII anchors or exact observed direct text.
- Prevention rule: large multilingual files require small independent patches and observed method-boundary anchors; never bind unrelated edits to guessed copy.
- Skill candidate: no

## 2026-07-16 - XAML click count used regex quoting instead of fixed text

- Symptom: the first static button-hook count raised an unclosed-regex error; the second exact pattern returned an empty count because the quoting representation was still ambiguous.
- Wrong assumption: an XAML attribute containing quotes was easier to count as a regular expression inside nested PowerShell/JavaScript strings.
- Root cause: multiple parser layers changed the quote/backslash representation before `rg` received the pattern.
- Detection method: `rg` reported the unclosed group; a simple symbol search found the hook at line 1715; counting the handler name returned 1.
- Fix: use `rg -F -c` on the unique handler symbol rather than the entire quoted XAML attribute.
- Prevention rule: static XAML hook counts should target unique symbol names with fixed-string mode, not quoted attribute regexes.
- Skill candidate: yes; repeated cross-shell exact-count checks should be wrapped in a reusable source-integrity script.
# 2026-07-18 - Parallel shell reads failed with Windows sandbox error 1056

- Symptom: a `Promise.all` batch of four independent `shell_command` reads failed before returning any repository evidence.
- Wrong assumption: the sandbox would reliably create all parallel PowerShell child processes for a read-only startup batch.
- Root cause: the Windows sandbox returned `CreateProcessWithLogonW failed: 1056` while starting the parallel batch.
- Detection method: direct tool failure before command output.
- Fix: retried the same read-only checks sequentially and obtained all required state.
- Prevention rule: when this Windows sandbox returns 1056 for a parallel shell batch, retry sequentially; do not reinterpret it as a repository failure.
- Skill candidate: no.

## 2026-07-18 - FluentAssertions expression tree rejected `is null`

- Symptom: the focused test project did not compile with CS8122 in an `OnlyContain` assertion.
- Wrong assumption: the assertion predicate accepted all modern C# pattern syntax.
- Root cause: this FluentAssertions overload captures an expression tree, and expression trees do not support the `is` pattern operator used there.
- Detection method: focused test compilation failed before test execution and identified the exact assertion line.
- Fix: replaced `reply.TargetAppName is null` with the expression-tree-compatible `reply.TargetAppName == null`.
- Prevention rule: use expression-tree-compatible comparisons in collection assertion predicates.
- Skill candidate: no.

## 2026-07-18 - Repeated direct `foreach` pipeline parse failure

- Symptom: a static-count command failed with `不允许使用空管道元素` before producing evidence.
- Wrong assumption: none new; the command repeated an already-recorded invalid Windows PowerShell form.
- Root cause: piped `foreach (...) { ... }` directly into `Format-Table` instead of collecting the loop output first.
- Detection method: PowerShell parser error at the pipe following the loop.
- Fix: collect records in a list/variable and format the completed collection afterward.
- Prevention rule: before running an ad hoc verification script, search the error ledger for the same command shape; never pipe directly from a Windows PowerShell `foreach` statement.
- Skill candidate: yes; this repeated failure strengthens the existing source-integrity helper candidate.

## 2026-07-18 - FluentAssertions expression tree rejected null propagation

- Symptom: focused tests did not compile with CS8072 in an `OnlyContain` assertion.
- Wrong assumption: replacing an unsupported pattern expression with null propagation would remain valid inside the captured predicate.
- Root cause: expression trees also do not support the null-propagating `?.` operator.
- Detection method: focused test compilation identified the exact predicate line.
- Fix: replaced the expression-tree predicate with an ordinary `foreach` and direct assertions.
- Prevention rule: when reflection or modern null/pattern syntax is needed, assert in an ordinary loop instead of a FluentAssertions collection predicate.
- Skill candidate: no.

## 2026-07-18 - Repeated XAML attribute search quoting failure

- Symptom: two read-only `rg` audits failed with `unclosed group` while searching quoted XAML attribute values.
- Wrong assumption: escaped double quotes would survive the JavaScript, JSON, PowerShell, and regex parsing layers unchanged.
- Root cause: the nested command stripped/reshaped the quote sequence before `rg` parsed it; this exact class of mistake was already documented.
- Detection method: `rg` regex parser error before any search result.
- Fix: use PowerShell single-quoted fixed strings with `rg -F`, or search a unique symbol instead of the whole attribute.
- Prevention rule: never send quoted XAML attributes to default regex mode from a nested tool command; use `-F` plus single quotes.
- Skill candidate: yes; repeated again, so the source-integrity/search helper candidate remains high priority.

## 2026-07-18 - Repository integrity script blocked by machine execution policy

- Symptom: direct invocation of `.omx/verify-source-integrity.ps1` failed with `PSSecurityException` before the script ran.
- Wrong assumption: repository-local PowerShell scripts were directly executable under the user's current policy.
- Root cause: the machine execution policy prohibits direct script loading.
- Detection method: PowerShell security error named the script and `UnauthorizedAccess`.
- Fix: invoke the reviewed read-only script with process-scoped `powershell -NoProfile -ExecutionPolicy Bypass -File`; do not modify machine policy.
- Prevention rule: document and use the exact process-scoped command for this repository script.
- Skill candidate: no.

## 2026-07-18 - Windows PowerShell could not parse UTF-8 script text

- Symptom: the portable-package script stopped at parse time and reported a missing here-string terminator before any publish command ran.
- Wrong assumption: Windows PowerShell 5.1 would decode an unmarked UTF-8 `.ps1` source file consistently.
- Root cause: Windows PowerShell read the UTF-8-without-BOM Chinese here-string through the local code page, corrupting the source token stream.
- Detection method: parser error plus a comparison of default `Get-Content` with `Get-Content -Encoding UTF8` around the here-string.
- Fix: keep the executable script ASCII-only and copy a separate UTF-8 Chinese readme template as bytes.
- Prevention rule: repository PowerShell scripts that must run under Windows PowerShell 5.1 stay ASCII-only unless their encoding marker is explicitly controlled and tested.
- Skill candidate: yes; this is a reusable Windows packaging rule.

## 2026-07-18 - Windows PowerShell host lacked Path.GetRelativePath

- Symptom: both Release projects published, but manifest generation stopped before ZIP creation because `[IO.Path]::GetRelativePath` was unavailable.
- Wrong assumption: a script launched by Windows PowerShell 5.1 would expose the same base-class-library APIs as the .NET 8 application being packaged.
- Root cause: Windows PowerShell 5.1 runs on .NET Framework, whose `System.IO.Path` has no `GetRelativePath` method.
- Detection method: the live package run failed at the exact file-manifest expression after both publish commands succeeded.
- Fix: use `System.Uri.MakeRelativeUri`, which is available on the supported PowerShell host, and protect that choice with a static compatibility contract.
- Prevention rule: packaging scripts target the host runtime's API surface, not the packaged application's target framework; verify them with the repository's documented Windows PowerShell command.
- Skill candidate: yes; this is a cross-project Windows packaging compatibility rule.

## 2026-07-18 - Custom package path used another unavailable Path API

- Symptom: the default package completed, but the explicit-output refusal check failed before reaching its existence guard.
- Wrong assumption: replacing only `GetRelativePath` covered the script's .NET Framework compatibility gap.
- Root cause: the explicit path branch still called `Path.IsPathFullyQualified`, which is also absent from the Windows PowerShell 5.1 host runtime.
- Detection method: exercising the caller-provided `OutputDirectory` branch against an existing package.
- Fix: use the older compatible `Path.IsPathRooted` API and add positive/negative source contracts for both names.
- Prevention rule: audit every static BCL method in a Windows PowerShell 5.1 script, including branches not covered by the default invocation.
- Skill candidate: yes; extend the same cross-project packaging compatibility lesson.

## 2026-07-18 - Expected-failure wrapper lost nested quotes

- Symptom: an ad hoc wrapper intended to classify an expected refusal failed in the nested PowerShell parser.
- Wrong assumption: embedded double-quoted literals would survive the outer command and inner `-Command` layers.
- Root cause: the nested command removed literal delimiters before the child parser evaluated `-notlike` and `throw` expressions.
- Detection method: parser errors showed the formerly quoted words as bare tokens.
- Fix: discard the wrapper and invoke the reviewed script directly with the existing path; classify its fixed error output at the caller level.
- Prevention rule: do not wrap expected PowerShell failures in another inline PowerShell parser layer; use a script/test or the direct command.
- Skill candidate: no.

## 2026-07-18 - Repeated quoted XAML regex command failure

- Symptom: the first click-hook audit command failed in the PowerShell parser before `rg` ran.
- Wrong assumption: a quoted XAML attribute regex was acceptable despite the repository rule and multiple earlier ledger entries.
- Root cause: the nested JavaScript/PowerShell quoting layers reinterpreted the attribute quotes as PowerShell syntax.
- Detection method: PowerShell reported an array-index parser error at the quoted regex.
- Fix: search the stable unquoted token `Click=` with `rg -F`, then inspect the returned lines.
- Prevention rule: for XAML attribute inventory, search an unquoted fixed token or a unique symbol; never include the attribute quotes in a nested shell command.
- Skill candidate: yes; repeated behavior reinforces the exact-count/search helper candidate.

## 2026-07-18 - Elevated worker output race during focused test build

- Symptom: focused tests stopped with CS2012 because `Css.Elevated.dll` in `obj/Debug` was locked by `VBCSCompiler`.
- Wrong assumption: the default parallel test build would serialize the App post-build worker target and the test project's direct Elevated project reference.
- Root cause: two build paths targeted the same Elevated intermediate output while shared compilation retained the file.
- Detection method: compiler error named the locked intermediate DLL and `VBCSCompiler` process.
- Fix: rerun worker-touching verification with `-m:1 -p:UseSharedCompilation=false`; focused 22/22 and full 966/966 then passed.
- Prevention rule: tests/builds that touch both `Css.App` and `Css.Elevated` use single-node, non-shared compilation until the duplicate project-build edge is redesigned.
- Skill candidate: no; this is currently repository-specific build topology.

## 2026-07-18 - Quoted XAML regex failed again during completion audit

- Symptom: the disabled/collapsed-control inventory failed with an `rg` unclosed-group error while the two preceding audits succeeded.
- Wrong assumption: escaping quotes inside a nested regex command was acceptable after repeated failures and an explicit repository prevention rule.
- Root cause: the attribute quote/backslash sequence was altered before `rg` parsed it.
- Detection method: `rg` reported the malformed generated group; no source result was produced for that subcommand.
- Fix: rerun with fixed unquoted tokens such as `IsEnabled=` and search control names directly.
- Prevention rule: this repository no longer uses nested regex commands for quoted XAML attributes at all; only unquoted `rg -F` tokens or direct file reads are allowed.
- Skill candidate: yes; this repeated failure should be absorbed by the planned search/count helper.

## 2026-07-18 - Quoted AgentPage search ignored the new fixed-token rule

- Symptom: a test-coverage search failed with a missing string terminator before `rg` ran.
- Wrong assumption: a short quoted `AgentPage` XAML fragment would be harmless even though quoted XAML searches had just been banned.
- Root cause: nested shell parsing consumed the attribute quote boundary.
- Detection method: PowerShell parser failure; the follow-up control-name-only search succeeded.
- Fix: search `AgentPage` and ScrollViewer symbols separately without quotes.
- Prevention rule: all future repository search commands are composed from symbol names or unquoted fixed tokens only; inspect exact markup with `Get-Content` after locating the line.
- Skill candidate: yes; this is the same repeated agent-behavior candidate and should not recur manually.

## 2026-07-18 - Agent consultation tab left a large unused area

- Symptom: the first tabbed Release screenshot showed the consultation card confined to 780px with a large blank region on the right.
- Wrong assumption: a readable maximum line width would improve the Agent view without making the WPF working surface look incomplete.
- Root cause: `MaxWidth=780` plus left alignment was chosen before inspecting the actual 1268x778 window composition.
- Detection method: real Release Computer Use screenshot immediately after navigating to AI Agent.
- Fix: remove the fixed maximum width and alignment, let the tab content stretch, add a source contract against the fixed width, republish, and recapture both tabs.
- Prevention rule: verify fixed-width WPF working panels at the real default window size before acceptance; avoid fixed maxima when the surrounding tool surface is expected to fill the workspace.
- Skill candidate: no; current lesson is covered by the repository visual gate.

## 2026-07-18 - Expected-zero search was combined without an exit-zero form

- Symptom: a combined source-inspection command returned exit code 1 after the second `rg` search found no matches, even though the first search produced useful output.
- Wrong assumption: a likely-empty exploratory search could be appended directly to another required read.
- Root cause: `rg` correctly returns 1 for zero matches, and the command did not convert that expected state into a count or explicit success.
- Detection method: shell result showed the first search output followed by overall exit code 1 with no execution error.
- Fix: separated the follow-up read and used already-resolved source paths; no product code depended on the failed command.
- Prevention rule: when a search may legitimately return zero and shares a command with required checks, use an exit-zero count form or run it separately.
- Skill candidate: no; this rule already exists in `AGENTS.md`.

## 2026-07-18 - Null-conditional XML assertion produced a false green

- Symptom: the first red run reported only presenter/source failures even though five XAML controls lacked the required `Visibility=Collapsed` attributes.
- Wrong assumption: `element.Attribute("Visibility")?.Value.Should().Be(...)` would assert null when the attribute was missing.
- Root cause: the null-conditional operator skipped the entire FluentAssertions call when `Attribute(...)` returned null.
- Detection method: mismatch between direct XAML inspection and the unexpectedly passing XAML test.
- Fix: extract the nullable value through a helper, then invoke `.Should().Be(...)` on that returned value; the corrected red run failed all three contracts before implementation.
- Prevention rule: never put FluentAssertions calls behind `?.`; resolve nullable values first so missing evidence is asserted, not skipped.
- Skill candidate: yes; this is a reusable test-authoring rule.

## 2026-07-18 - Build-time NuGet audit flag did not clear stale restore warnings

- Symptom: the first Release build succeeded but reported 13 `NU1900` warnings even with `-p:NuGetAudit=false` on the build command.
- Wrong assumption: a build-time property would replace audit metadata already recorded in the existing restore assets.
- Root cause: the prior assets were produced while the vulnerability feed was unavailable; `--no-restore` reused that warning state.
- Detection method: the build summary explicitly reported 13 warnings instead of the required clean 0-warning evidence.
- Fix: run one normal solution restore with `-p:NuGetAudit=false`, then rerun the single-node Release build with `--no-restore`; it passed with 0 warnings and 0 errors.
- Prevention rule: when clean build evidence matters after a prior `NU1900`, refresh restore assets with audit disabled before the no-restore verification build; never report the first noisy build as warning-free.
- Skill candidate: no; this repository already records the restore-before-verification workflow.

## 2026-07-18 - Migration hierarchy test mixed product red with test compile errors

- Symptom: the first focused red run found the missing presenter but also failed because the fixture called a nonexistent `MigrationExecutionGateResult.Refused` helper and FluentAssertions expression trees rejected null-propagating lambdas.
- Wrong assumption: the gate exposed the same refusal factory as adjacent policies, and collection predicate assertions accepted `?.` in their expression trees.
- Root cause: the test fixture API was not inspected before use; the assertion overload builds an expression tree where null propagation is unsupported.
- Detection method: compiler errors `CS0117` and `CS8072` in the focused test run.
- Fix: initialize the required gate fields directly, use ordinary LINQ `Any` before asserting, and keep nullable attribute evidence behind a value-returning helper.
- Prevention rule: inspect required fixture types before authoring object builders, and avoid null-propagating operators inside expression-tree assertion predicates.
- Skill candidate: no; the nullable assertion lesson is already recorded and this API detail is project-specific.

## 2026-07-18 - Repository search included an unresolved guessed source path

- Symptom: a parallel search reported OS error 2 for `src\Css.Core\Migration\MigrationExecutionGate.cs`.
- Wrong assumption: the execution-gate type lived under the Migration folder because of its namespace role.
- Root cause: the path was guessed instead of resolved with `rg --files` or a symbol search, violating the repository search protocol.
- Detection method: `rg` reported the missing path; a symbol-only search located the file under `src\Css.Core\Apps`.
- Fix: reran symbol searches against the resolved source tree and read only the returned paths.
- Prevention rule: never add an unobserved source path to a required parallel read; locate the symbol first.
- Skill candidate: no; this prevention rule already exists in `AGENTS.md`.

## 2026-07-19 - Verifier test patch contained a stray quote

- Symptom: the initial new test source contained an extra quote after `AwaitingBehavioralAcceptance`.
- Wrong assumption: the multi-line assertion patch was syntactically complete.
- Root cause: a transcription typo in the test-first patch.
- Detection method: immediate source review before running the focused test.
- Fix: removed the stray quote, then ran the intended red test against the missing verifier.
- Prevention rule: reread newly added fluent assertion chains before invoking the compiler.
- Skill candidate: no

## 2026-07-19 - Negative readme assertion matched an explicit denial

- Symptom: the focused test rejected the sentence `不代表已经完成生产验收` because it searched for the broad substring `已经完成生产验收`.
- Wrong assumption: absence of a phrase was a reliable way to distinguish a false claim from an explicit denial.
- Root cause: the negative assertion ignored sentence polarity.
- Detection method: focused green run after adding the readme.
- Fix: require the explicit denial and reject only an affirmative status form.
- Prevention rule: test safety copy semantically with positive boundary phrases; avoid polarity-blind substring negatives.
- Skill candidate: no

## 2026-07-19 - Refusal smoke initially hit the host execution policy

- Symptom: invoking the new script directly from the host PowerShell refused to load it before reaching its own missing-sign-tool guard.
- Wrong assumption: the current host process permitted local script execution.
- Root cause: the machine execution policy blocks direct script invocation in that shell.
- Detection method: first dynamic refusal smoke.
- Fix: run a child `powershell -NoProfile -ExecutionPolicy Bypass -File` process, then verify its nonzero exit and absent output directory from the parent.
- Prevention rule: use the repository's established child-PowerShell invocation for script smokes; distinguish host policy refusal from product guard evidence.
- Skill candidate: no

## 2026-07-19 - Assumed an OperationResult property that does not exist

- Symptom: the new focused test failed to compile because it initialized `OperationResult.Message`.
- Wrong assumption: the result model exposed a writable message property.
- Root cause: the test fixture was written before checking the existing `OperationResult.Fail(...)` factory/API.
- Detection method: first focused test build.
- Fix: construct the fixture with `OperationResult.Fail("test")`.
- Prevention rule: inspect or reuse existing test factories before manually initializing safety result models.
- Skill candidate: no

## 2026-07-19 - Generic return wording was false in standalone Debug hosting

- Symptom: changing the shared official-uninstall result model to `返回并重新检查` also changed the independently hosted Debug worker-connection result, which has no application page to return to.
- Wrong assumption: every use of `OfficialUninstallWorkerResultWindow` is nested under Application Management.
- Root cause: the result window has both nested production and standalone Debug hosts.
- Detection method: package-boundary source review after the first green implementation.
- Fix: keep generic close copy on the shared model and opt into application-return wording only from nested uninstall-plan callers.
- Prevention rule: search every constructor call site before changing shared result navigation copy or behavior.
- Skill candidate: yes

## 2026-07-19 - Uninstall decision fixture used invalid verbatim quoting

- Symptom: the first focused test run failed to compile at the fixture uninstall-command string.
- Wrong assumption: backslash-escaped quotes could be used inside a C# verbatim string.
- Root cause: verbatim strings escape quotes by doubling them, not with backslashes.
- Detection method: focused compiler errors `CS1003`, `CS1056`, and `CS1009` at the new fixture line.
- Fix: changed the fixture to doubled-quote verbatim syntax and reran the focused suite, which passed 3/3.
- Prevention rule: when a fixture needs a quoted Windows command, copy an existing repository example or use ordinary escaped strings; do not mix verbatim and backslash quote rules.
- Skill candidate: no; this was a local test-authoring mistake.

## 2026-07-19 - Summary simplification dropped a safety-contract word

- Symptom: the final full suite failed two product-experience tests after changing `不会直接运行卸载器` to `不会运行卸载器`.
- Wrong assumption: removing `直接` preserved the same user-facing safety contract.
- Root cause: the word distinguishes the read-only preview from a later, explicitly confirmed official-uninstall flow.
- Detection method: full regression failures in two uninstall presentation contracts.
- Fix: restored `不会直接运行卸载器`, kept the new readiness-neutral second sentence, and reran all 983 tests successfully.
- Prevention rule: treat tested safety copy as semantic behavior; preserve qualifiers that distinguish preview, preparation, request, and execution.
- Skill candidate: no; the repository tests already encode this rule.
## 2026-07-19 - Boolean script parameters were not robust across Windows PowerShell `-File`

- Symptom: compatibility review found that the initial `[bool]` environment parameters could fail during native `powershell.exe -File` argument binding before the safety checks ran.
- Wrong assumption: a PowerShell Boolean expression in the calling shell would arrive as a Boolean in a Windows PowerShell 5.1 script process.
- Root cause: native process arguments cross the boundary as text, and Windows PowerShell 5.1 does not reliably support explicit Boolean script parameters through `-File`.
- Detection method: reviewed the documented command against the repository's Windows PowerShell 5.1 packaging compatibility decision before runtime acceptance.
- Fix: replaced both inputs with one-value `ValidateSet` strings and serialized fixed genuine Boolean values only after validation.
- Prevention rule: for scripts documented through `powershell.exe -File`, use switches or allowlisted textual values at the process boundary; do not depend on cross-process Boolean binding.
- Skill candidate: yes; this applies to Windows PowerShell-compatible release tooling across projects.

## 2026-07-19 - Fixture TDD project initially had no entry point

- Symptom: the first red test build stopped at a missing `Main` method instead of the intended missing fixture contracts.
- Wrong assumption: an incomplete console project could participate in the red build before its entry point existed.
- Root cause: `OutputType=Exe` requires a compilable entry point even when tests are intentionally red on unimplemented domain types.
- Detection method: focused build reported the entry-point compiler error before contract failures.
- Fix: add a minimal temporary `Program` entry point, then continue the red/green cycle against the real missing types.
- Prevention rule: scaffold the smallest compilable executable shell before writing red tests for its internal contracts.
- Skill candidate: no.

## 2026-07-19 - Fixture paths did not initially reach real product decisions

- Symptom: the first fixture cache and cleanup layouts were valid files but did not produce attributed cache evidence or an executable C-drive cleanup recommendation through the real builders.
- Wrong assumption: any session-nested cache/temp path found by a scanner would exercise the same product workflow as an installed application's cache or a top-level cleanup root.
- Root cause: software attribution expects app data under the application's discoverable identity, and `DiskRecommendationBuilder` authorizes cleanup from supported top-level Temp findings rather than arbitrary nested nodes.
- Detection method: integration tests through `SoftwareInventoryBuilder`, real scan rules, and `DiskRecommendationBuilder` failed to yield the required profile/operation.
- Fix: place cache under `%LocalAppData%\<fixture display name>\Cache`; use exact session-owned `C:\Temp` with fail-closed collision refusal; add end-to-end builder assertions.
- Prevention rule: acceptance fixtures must be proven through the real discovery and recommendation chain, not only through fixture-local file assertions.
- Skill candidate: yes.

## 2026-07-19 - Provision compensation tracked a root too late

- Symptom: an injected ownership-marker write failure left the newly created root behind.
- Wrong assumption: tracking the root after marker persistence was sufficient for compensation.
- Root cause: directory creation was already a mutation, but the compensation ledger did not include it until the next step succeeded.
- Detection method: fault-injection test asserted zero remaining fixture roots after marker failure.
- Fix: track each owned root before creation and conditionally remove it during reverse compensation when it exists.
- Prevention rule: register compensation before the first mutation in every multi-step resource creation sequence.
- Skill candidate: yes.

## 2026-07-19 - Test helpers assumed wrong serialized and profile shapes

- Symptom: a migrated marker helper used `sessionId` instead of `SessionId`, and a software-profile assertion treated startup entries as objects instead of names.
- Wrong assumption: test-only helpers could infer JSON casing and view-model element types from domain intent.
- Root cause: helpers were authored before inspecting the actual serializer output and `SoftwareProfile.StartupEntries` contract.
- Detection method: ownership validation and focused test compilation/assertions failed while production behavior was correct.
- Fix: match the real serialized property casing and assert the string startup-entry collection.
- Prevention rule: derive fixture helpers from observed persisted schemas and public types; do not recreate either from memory.
- Skill candidate: no.

## 2026-07-19 - Fixture verifier was invoked with a relative path

- Symptom: final package verification refused `FixtureKitDirectory` before reading the manifest.
- Wrong assumption: the verifier would resolve a repository-relative package path.
- Root cause: its safety boundary intentionally accepts only fully qualified local paths.
- Detection method: direct error stated that `FixtureKitDirectory` must be fully qualified.
- Fix: rerun with `D:\Agent\Project\OMNIX-Entropy\.artifacts\OMNIX-Acceptance-Fixtures-20260719-014314`; verification passed.
- Prevention rule: use absolute local paths for all release/acceptance verifier inputs and keep their refusal behavior unchanged.
- Skill candidate: no.
