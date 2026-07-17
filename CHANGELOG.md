# Changelog

All notable project changes are documented here. The project is pre-release and uses milestone labels until a public versioning scheme is selected.

## Milestone 0 — Project Foundation — Completed 2026-07-16

### Added

- `RulesOfEntry.Runtime`, `RulesOfEntry.Editor`, `RulesOfEntry.Tests.EditMode`, and `RulesOfEntry.Tests.PlayMode` assembly boundaries.
- Project identity constants and structured project logging.
- Serialized scene-purpose and foundation-version marker.
- Repeatable Milestone 0 setup command under the Unity Tools menu.
- Project validator covering Unity version, packages, input mode, HDRP, folders, assemblies, scene setup, missing scripts, and forbidden legacy input usage.
- Pre-build validation that blocks builds containing foundation errors.
- Edit Mode project-contract tests and a Play Mode scene-marker smoke test.
- Milestone 0 installation and testing guide.

### Changed

- `CURRENT_STATUS.md` now records the delivered Milestone 0 package and remaining validation steps.
- `BUGS.md` records the clean pre-integration baseline and the pending post-integration check.

### Verified

- Pre-Milestone-0 project opens in Unity `6000.5.2f1` with zero compiler errors.
- Existing package versions support the foundation; no `manifest.json` edits are required.

### Validation

- Post-import Unity compilation completed with zero errors.
- Foundation setup completed successfully.
- Project validator completed with 15 passing checks, zero errors, and one expected `DefaultCompany` warning.
- All Edit Mode and Play Mode tests passed.
- Prototype Play Mode completed without Console errors or exceptions.

## Unreleased — Milestone 1

### Planned

- First-person CharacterController foundation.
- Project-owned tactical input actions.
- Camera look, walk, sprint, and crouch.
- Interaction targeting, prompts, and test interactables.
- Repeatable graybox test environment.
