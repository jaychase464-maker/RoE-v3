# Development Roadmap

## Stable baseline

- Milestone 0 — Project Foundation: complete.
- Milestone 1 — First-Person and Tactical Interaction: complete.

## Milestone 2 — Weapon and Force-Event Foundation — Integration candidate

Current scope:

- physical magazine/chamber/bolt/selector state;
- manual checks, retained reloads, emergency reloads, and action cycling;
- safe, low-ready, shouldered, aimed, semi-automatic handling;
- no automatic reload and no exact ammunition HUD;
- physical-muzzle raycast, hit facts, and immutable force events;
- graybox view/range, validation, and tests.

Exit: state cannot desynchronize; empty weapons never reload themselves; ammunition uncertainty remains player-facing; every discharge creates exactly one factual event; no dry action creates a discharge event; all Milestone 0–2 tests and smoke tests pass.

## Realism work following the mechanical foundation

1. Validated projectile flight time, gravity, drag, sight/bore zeroing, and deterministic environmental inputs.
2. Material definitions and documented penetration/deflection test cases.
3. Production first-person weapon/arms rig with correct movable-part pivots and procedural interruption handling.
4. Weapon audio propagation, hearing effects, muzzle flash, smoke, casing, and indoor pressure response.
5. Malfunction and maintenance modeling only after failure data and gameplay recovery controls are defined.

## Later prototype milestones

- Milestone 3: suspects, civilians, compliance, surrender, restraint, arrest, and actor condition.
- Milestone 4: officer AI, selection, and tactical command execution.
- Milestone 5: mission objectives, ROE policy evaluation, evidence, and after-action reporting.
- Milestone 6: first seeded tactical vertical slice.

Every milestone requires zero compiler/validator errors, all automated tests, all earlier smoke tests, current manual tests, clean Console results, updated documentation, and a source-control commit before advancing.
