# Error Ledger

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
