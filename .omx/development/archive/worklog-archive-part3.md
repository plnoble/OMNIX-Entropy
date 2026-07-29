# Archived worklog (2026-07-19 to 2026-07-19)

Historical entries moved out of `.omx/development/worklog.md` so the live record stays readable. Entries are verbatim, ordered oldest first. Active records start at 2026-07-22.

## 2026-07-19 - Disposable Windows behavioral acceptance protocol completed

- Added `new-disposable-acceptance-session.ps1`. It refuses primary/non-disposable environments, missing checkpoint evidence, incorrect attestation, existing/nonlocal/reparse session paths, nested package/session trees, and any candidate that fails the independent signed-candidate verifier before output creation.
- Added `verify-disposable-acceptance-receipt.ps1`. It is read-only and binds the current signed candidate, signer, session manifest, exact ten-case Pass set, reset attestation, ordered timestamps, unique evidence paths, and every evidence file length/SHA-256.
- Added a Chinese operator protocol covering manual UAC accept/cancel, fixture-only cleanup/cache/startup/uninstall/migration/rollback/Undo scenarios, evidence export, environment reset, and final read-only verification.
- TDD red was 0/6; focused green 6/6; release pipeline 20/20; full 1000/1000; Release build 0 warnings/errors; source integrity 361 files and 17/17 XAML; both scripts parse and remain ASCII-only.
- Runtime refusal against the current unsigned package returned exit 1 before creating the unique session directory. No product launch, UAC interaction, signing, certificate access, or system mutation ran.
## 2026-07-19 - Antivirus-updated Release launch retry remained unavailable

- Retried the latest ProductionOnly unsigned Release package through Computer Use after the user confirmed updated Huorong definitions.
- `launch_app` timed out; the one allowed passive window query found no OMNIX-Entropy window. No alternate GUI automation, security-software interaction, or launcher bypass was used.
- A read-only process check found no `Css.App` process, so the failed attempt left no OMNIX-Entropy background instance. Visual acceptance remains Warn and unsigned mutation remains blocked.

## 2026-07-19 - Disposable acceptance fixture kit completed

- Added an isolated `Css.AcceptanceFixtures` console project that is never referenced by App/Elevated packaging. Its mutation commands require the exact disposable-environment attestation and a canonical GUID; `status` remains read-only.
- Implemented preflighted, ownership-marked provision/uninstall/lock/reset behavior through injectable file/registry adapters. Provision compensates partial writes, uninstall removes only exact owned HKCU records and leaves residue, and reset refuses reparse traversal or mismatched ownership.
- Integrated the fixture shape with real `SoftwareInventoryBuilder`, uninstall trust, C-drive scan rules, and `DiskRecommendationBuilder`: the app cache/startup evidence attributes to the fixture, and exact `C:\Temp` produces a low-risk reversible cleanup operation.
- Added fixture publishing/verifying scripts and bound the verified fixture-manifest hash into disposable session creation and final receipt verification.
- An unsigned product package plus a valid fixture package was still refused before session creation, proving the fixture dependency does not weaken the signed-candidate gate.
- Verification: fixture 22/22, fixture package 4/4, disposable protocol 6/6, related product/release 434/434, full 1026/1026, Release build 0 warnings/errors, source integrity 367 files and 17/17 XAML.
- Final fixture package/ZIP: `.artifacts/OMNIX-Acceptance-Fixtures-20260719-014314`; five payload files; manifest SHA-256 `07C033F1B445DCF1E171ABC18E8FAC3AD9ECDA1ADFDECC0603C22FB712FA4FA3`. No fixture mutation ran on the current machine.
- Republished product package/ZIP `.artifacts/OMNIX-Entropy-test-20260719-014731`; manifest contains 110 files, the artifact contains zero fixture payloads, the Release command surface is ProductionOnly, and unsigned mutation remains blocked.

