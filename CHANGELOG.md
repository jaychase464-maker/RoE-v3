# Changelog

## Milestone 2 Hotfix 1 — Unity 6000.5 Entity IDs — 2026-07-16

- Replaced compile-blocking `Object.GetInstanceID()` calls with `Object.GetEntityId()`.
- Changed force-event shooter and hit-collider identifiers from truncated `int` values to complete `ulong` entity IDs.
- Added PlayMode assertions for recorded entity identity.
- Added an EditMode regression scan that rejects deprecated `GetInstanceID()` calls in runtime code.
- Live Unity validation remains pending.

## Milestone 0 — Project Foundation — Completed 2026-07-16

- Project-owned structure, assemblies, logging, setup, validation, tests, and prototype scene.
- Unity compilation, validator, automated tests, and smoke test passed.

## Milestone 1 — First-Person and Tactical Interaction — Completed 2026-07-16

- First-person movement/look, cursor handling, project input, interactions, UI, door/panel examples, graybox builder, validation, and tests.
- Unity compilation, validator, all tests, manual smoke test, and clean Console passed.

## Milestone 2 — Weapon and Force-Event Foundation — Integration candidate 2026-07-16

### Added

- Data-driven patrol-carbine and ammunition definitions.
- Per-magazine ammunition, separate chamber, bolt lock, selector, and physical pouch-order state.
- Manual qualitative magazine checks with no numerical ammo HUD.
- Manual retained and emergency reloads; no automatic reload path.
- Manual action cycling and live-round ejection accounting.
- Safe/Semi and low-ready/shouldered/aimed weapon handling.
- Semi-automatic one-shot-per-trigger-press control.
- Physical muzzle-origin shooting with near-cover obstruction.
- Ballistic hit contract and reactive graybox targets.
- Immutable append-only force-event facts without score or ROE evaluation.
- Graybox weapon view, status UI, target range, setup tool, validator, build gate, and regression tests.

### Changed

- Project input actions now include weapon manipulation controls.
- `TacticalPlayerInput` exposes weapon intent through the Input System.
- `FirstPersonLook` accepts controlled recoil impulses.
- `FirstPersonMotor` prevents sprint acceleration during magazine checks, reloads, and action cycling.
- `ProjectInfo.CurrentMilestone` identifies Milestone 2.

### Packages/settings

- No package, assembly-definition, or project-layer change.

### Validation pending

- Live Unity compilation, setup, validation, all automated tests, Milestone 1 regression, Milestone 2 realism test, and clean Console.
