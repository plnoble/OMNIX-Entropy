# GUI Smoke Test Notes

This page documents development and GUI smoke tests only. Do not expose these as normal user settings, installer options, or packaged end-user controls.

## Storage Overrides

`OMNIX_ENTROPY_DATA_ROOT` redirects the local app database and migration rollback evidence root for the launched process.

`OMNIX_ENTROPY_QUARANTINE_ROOT` redirects the quarantine root for the launched process.

`OMNIX_ENTROPY_CDRIVE_SCAN_ROOT` redirects the read-only C-drive crawl root for the launched process. It exists only so GUI smoke tests can scan a tiny controlled fixture instead of the user's real C drive.

`OMNIX_ENTROPY_SOFTWARE_FIXTURE` redirects the read-only software inventory scanner to a JSON scan sequence for the launched process. It exists only so GUI smoke tests can simulate app states such as "installed before uninstall" and "not present after uninstall" without reading or changing the user's real registry, services, startup entries, scheduled tasks, or installed applications.

`OMNIX_ENTROPY_STARTUP_FIXTURE` replaces the current-user Run adapter with a single in-memory startup entry for the launched process. It accepts only a matching structured software observation and never opens or changes the Windows registry. Use it only for cancel-first startup-control GUI smokes.

Use these variables only from smoke scripts that need isolated local state. A script that sets any variable must restore previous environment values in `finally` and remove its temporary roots when the smoke ends.

Normal app runs should keep the defaults from `AppStoragePathResolver`: `%LocalAppData%\OMNIX-Entropy\data.db` for local data and `D:\OMNIX-Entropy\Quarantine` when the D drive is available.

## Smoke Helpers

`.omx/wpf-smoke-helpers.ps1` owns common WPF UIAutomation helpers such as initialization, AutomationId lookup, polling, button invocation, and screenshots. New `.omx/gui-*.ps1` scripts should dot-source it instead of copying those functions.

## Seed Tools

`Css.SmokeTools seed-undo-center` creates an isolated restorable undo-center record for GUI proof. It must be run only after the storage override variables point to temporary test roots.

The seed tool reuses the same Core quarantine and timeline services as the app. It is development/test tooling, not a user-facing recovery or cleanup feature.

## Home Agent Next-Action Smoke

`.omx/gui-home-agent-next-action-smoke.ps1` launches the real WPF app with isolated app data and a unique 4 KB temporary C-drive scan fixture confined under `C:\tmp`. It verifies stable rows for D-drive space, memory plus count-only processes, battery state, startup clues, and manual usage history; unavailable/not-present results are accepted when the machine cannot provide a metric. It clicks the home health finding's `生成处理方案`, verifies that the Agent title, explanation, safety boundary, and next-action button are all visible in the first working area, and writes the OMNIX-window-only screenshot `.omx/qa-home-agent-next-action.png`.

The smoke then clicks the structured next action and requires the C-drive recommendation list to become visible. It `不执行任何建议`, does not open a confirmation dialog, quarantine a file, enumerate the user's real C-drive content, or retain process names. The script restores both environment variables, terminates the App, and removes its exact fixture roots in `finally`.

## Personal Storage Candidate Smoke

`.omx/gui-personal-storage-candidates-smoke.ps1` creates one unique directory under `C:\tmp` containing one 12 KB old-file fixture and two 4 KB same-name/same-size fixtures. `OMNIX_ENTROPY_PERSONAL_STORAGE_ROOT` is a process-scoped development seam that lowers candidate thresholds only for that fixture; normal scans keep the 512 MB large-file and 64 MB possible-duplicate thresholds.

The smoke verifies that Home shows personal-storage findings, `查看详情` offers the exact personal-file destination, and navigation scrolls the C-drive page directly to two path-free candidates with stable AutomationIds. The first level must say `疑似`/`只读候选` and that content was not compared. It never hashes, opens, moves, quarantines, or deletes a file and records `noOperationExecuted=true`. Recursive cleanup is guarded by exact `C:\tmp`/`.omx` confinement checks; screenshot: `.omx/qa-personal-storage-candidates.png`.

## App Drawer Preview Smoke

`.omx/gui-app-drawer-preview-smoke.ps1` launches the real WPF app with one isolated software profile that deliberately contains official-uninstall, C-drive, cache, and ordinary-startup evidence. It verifies the beginner-facing uninstall, migration, cache, and startup preview cards without depending on whichever applications happen to be installed on the development PC.

The script may open and immediately close the read-only uninstall/migration plan windows so it can inspect the shared drawer conclusion. It never runs an uninstaller, moves a directory, quarantines cache, opens Windows Settings, or changes startup state. Environment variables and fixture files are restored/removed in `finally`; the screenshot is `.omx/qa-app-drawer-action-previews.png`.

## Startup Control Cancel Smoke

`.omx/gui-startup-control-cancel-smoke.ps1` launches the real WPF app with isolated software and in-memory startup fixtures. It proves that one uniquely matched ordinary current-user Run entry produces a local review, that the two-acknowledgement confirmation starts disabled, and that technical details are collapsed below the beginner conclusion.

The script never toggles an acknowledgement or invokes the confirmation button. It captures `.omx/qa-startup-control-confirmation.png`, clicks cancel, requires the uncommitted rollback manifest to be removed, and opens the same review a second time to prove cancellation did not change even the in-memory fixture. It never opens or changes the Windows registry.

`.omx/gui-startup-restore-cancel-smoke.ps1` seeds a production-format startup rollback manifest and restorable timeline entry through Core's real operation pipeline with an in-memory store. It opens the dedicated startup restore confirmation from the real WPF timeline, captures `.omx/qa-startup-restore-confirmation.png`, and clicks cancel only. The manifest and enabled restore record must remain unchanged; the script does not invoke the restore confirmation button or touch the registry.

## Agent Troubleshooting Routing Smoke

`.omx/gui-agent-troubleshooting-routing-smoke.ps1` asks the real local Agent `驱动异常怎么办`, requires an uncertainty-aware answer and an `打开设备管理器` next step in the same visible working area, and captures `.omx/qa-agent-troubleshooting-routing.png`.

The script invokes that next step only to reach the existing protected-tool confirmation, captures `.omx/qa-agent-tool-open-confirmation.png`, and clicks `取消`. It compares `mmc` process ids before and after cancellation, does not start Device Manager, and does not change drivers, devices, registry, services, tasks, or settings.

## Agent Hardware Summary Smoke

`.omx/gui-agent-hardware-summary-smoke.ps1` launches the real WPF app with isolated app data and one unique 4 KB C-drive fixture under `C:\tmp`. After the manual scan, it asks `我的电脑配置怎么样` and requires the CPU, GPU, Windows version, and architecture evidence categories, the read-only explanation, and the compatibility-limit next step in the first visible Agent working area.

The smoke rejects local paths, registry locators, serial/device identifiers, and any operation invocation; it writes `.omx/qa-agent-hardware-summary.png`, terminates the App, and removes both exact fixture roots in `finally`. The script's existence is not visual proof: record a pass only after visible-window approval succeeds, its JSON reports `noOperationExecuted=true`, cleanup is empty, and the screenshot is manually inspected without compositor blanks or clipping.

## Agent Skill Card Smoke

`.omx/gui-agent-skill-cards-smoke.ps1` opens the Agent page with isolated app data and invokes the stable `问 Agent` action on the `窗口与桌面管理` skill. Because that capability is not implemented in V1, the first visible response must say that no window/desktop evidence was read and must expose no navigation or execution action.

The smoke writes `.omx/qa-agent-skill-card-response.png`, records `noOperationExecuted=true`, and removes its exact data root in `finally`. As with every beginner conclusion, the script and source test are preparation only; a real run plus manual screenshot inspection is required before changing the visual gate from Warn to Pass.

## Install Diff Agent Smoke

`.omx/gui-install-diff-agent-smoke.ps1` launches the real WPF app with an isolated two-scan `OMNIX_ENTROPY_SOFTWARE_FIXTURE`. It captures an installation-before snapshot, an installation-after snapshot, the beginner report cards, and the on-demand Computer Agent explanation.

The smoke does not run an installer, analyze an installer package, remember routing rules, or modify installed software. It only reads the fixture sequence, writes temporary app data under `.omx`, captures screenshots, restores the previous process environment, and removes the temporary fixture state.

After the explanation, the smoke also clicks `生成处理方案` and verifies that the real WPF page shows an ordered, non-executable Agent plan. The plan must contain at least three items for the C-drive-plus-background fixture, visibly say `尚未执行`, and keep technical details collapsed. Its screenshot is `.omx/qa-install-diff-action-plan.png`.

The smoke then proves that `为什么这样判断` is collapsed by default, expands it on demand, verifies one generic C-drive finding and three generic background findings, rejects any exposed fixture path/service/task identifier, and keeps technical details collapsed. Its screenshot is `.omx/qa-install-diff-evidence-review.png`.

Inside the expanded review, the smoke verifies three evidence-driven, plan-only candidates for the fixture. Candidate buttons may only open read-only previews; they cannot execute an operation. The candidate screenshot is `.omx/qa-install-diff-eligible-actions.png`.

The smoke opens the cache candidate on demand and verifies that the preview reuses the local safety planner, explains missing evidence, says nothing was executed, and hides raw fixture identifiers. A ready preview may expose one internal-navigation button directly below the Agent conclusion, but no execution control. The conclusion and navigation button must intersect the real install-page viewport together; the window-only screenshot is `.omx/qa-install-diff-candidate-preview.png`.

The smoke then invokes that navigation button, requires the exact `New Fixture Tool` application drawer and its normal cache-review entry, and captures `.omx/qa-install-diff-app-handoff.png`. It does not invoke the cache-review entry or any confirmation/operation button.

Smoke startup waits for WPF input-idle readiness, then uses `ShowWindowAsync` and `SetWindowPos(HWND_TOPMOST)` only to make the window visible for screenshots. It does not require or attempt keyboard focus. Screenshot scrolling compares real control/viewport rectangles instead of trusting nested-list `IsOffscreen` state.

## Uninstall Recovery Truth Smoke

`.omx/gui-uninstall-plan-window-smoke.ps1` opens the real uninstall preview without invoking any uninstaller. The first visible area must show the Agent conclusion, the distinction between reinstalling the application and restoring quarantined residue, the reinstall-source readiness result, three beginner steps, and the next safe action.

The smoke requires `UninstallPlanTechnicalDetailsExpander` to be collapsed, rejects an official-uninstaller execution control, closes the preview, and writes `.omx/qa-uninstall-plan-window.png`. Raw reinstall paths and MSI product codes belong only in the advanced section. A screenshot with desktop-composition black blocks is invalid and must be replaced by an unchanged rerun.

The recovery-preparation variant also requires `UninstallPlanRestorePointStatusTextBlock`, `UninstallPlanChooseInstallerButton`, `UninstallPlanBackupCheckBox`, and `UninstallPlanPreparationSummaryTextBlock` in the first working area. It must not open the file picker during smoke verification. The final-confirmation variant additionally proves that missing recovery requirements are visible, no evidence directory is created, and no execution control exists.

## Uninstall Post-Scan Result Smoke

`.omx/gui-uninstall-post-scan-result-smoke.ps1` launches the real WPF result window through the DEBUG-only `--smoke-uninstall-post-scan-review` argument. Release builds do not compile this fixture entry point.

The smoke uses fixed path-free result data and verifies the status, three beginner-facing facts, Computer Agent advice, the no-further-deletion safety line, and the absence of an execution control. It closes the window and writes `.omx/qa-uninstall-post-scan-result.png`. It does not run an uninstaller, scan the registry, delete residue, close services, or call the safety operation pipeline.

## Uninstall Final Consent Smoke

`.omx/gui-uninstall-final-consent-smoke.ps1` launches the DEBUG-only `--smoke-uninstall-final-consent` flow. The confirmation button must start disabled, become enabled only after all three plain-language acknowledgements, and then send a correlation-bound request over a real current-user named pipe to an injected fake endpoint.

The smoke captures `.omx/qa-uninstall-final-consent.png` and `.omx/qa-uninstall-final-consent-result.png`. It requires the two path-free typed IPC facts, so the fixed fallback and failed-pipe result cannot pass. It does not launch an elevated helper, register an operation handler, call a real launcher/scanner, or run an installer/uninstaller.

## Elevated Worker Lifecycle Smoke

`.omx/gui-uninstall-worker-lifecycle-smoke.ps1 -ExpectedOutcome Accept|Cancel` launches the DEBUG-only `--smoke-uninstall-worker-lifecycle` route. It uses the packaged sibling `Css.Elevated.exe` and the production `runas` adapter, so Windows shows a real administrator confirmation. The tester must make the requested choice on the secure desktop; the script never automates or bypasses UAC.

After the choice, the script verifies the path-free beginner result, Computer Agent advice, no-change safety statement, screenshot, and clean App exit. `Accept` proves only the fake authenticated worker round trip and must show `安全连接测试已完成`; `Cancel` must show `你取消了 Windows 确认`. The fake worker explicitly returns `UninstallerStarted=false`; no real handler, launcher, scanner, registry edit, service change, deletion, migration, or installer/uninstaller call is registered.
