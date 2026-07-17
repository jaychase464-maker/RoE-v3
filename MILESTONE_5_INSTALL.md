# Milestone 5 — Mission, ROE, and After-Action Review

## Goal

Milestone 5 turns existing simulation facts into an accountable operation report without allowing combat, AI, custody, or officer code to manipulate score.

The training operation evaluates:

- lawful physical custody of the primary suspect;
- protection of the civilian;
- verified clearance of the north training room;
- preservation of the two-officer response team;
- every recorded player firearm discharge under a threat-based deadly-force policy.

## Install

1. Confirm Milestone 4 Hotfix 4 is installed, working, committed, and pushed.
2. Exit Play Mode and close Unity.
3. Extract `RulesOfEntry-Milestone-5.zip` directly into `C:\Users\Shadow\Videos\RoE v3`.
4. Merge folders and replace files.
5. Reopen Unity `6000.5.2f1` and wait for compilation.
6. Confirm the Console has no compiler errors.
7. Run `Tools > Rules of Entry > Milestone 5 > Build Mission and After-Action Prototype`.
8. Let setup save, reload, and validate the prototype scene.
9. Run all EditMode and PlayMode tests.

No new package, Input System action, physics layer, render setting, or Inspector assignment is required.

## Generated assets and scene content

- `Data/Missions/M5_TrainingOperation.asset`
- `Data/Missions/M5_TrainingROE.asset`
- `Prefabs/UI/ROE_MissionAfterActionDebugUI.prefab`
- `[Milestone5_Mission]` in `ROE_Prototype.unity`
- `ROE_MissionAfterActionDebugUI` in `ROE_Prototype.unity`
- `M5_DebriefConsole` near the player staging area

## Evidence and scoring contract

The mission layer copies facts from actor state, room-clearance state, firearm events, custody events, officer orders, and officer-initiative ledgers. It does not write back to those systems.

The provisional score starts at 100. Failed objectives and confirmed ROE violations apply documented deductions. A failed required objective caps the result below `Acceptable`; a critical ROE violation caps it at 59. Events with insufficient evidence are marked `ReviewRequired` and receive no automatic deduction.

ROE examples:

- threatening suspect with an accessible weapon: `WithinPolicy`;
- discharge without an identified person impact: `ReviewRequired`;
- force against a civilian or officer: critical `Violation`;
- force against a surrendering, kneeling, restrained, incapacitated, or deceased person: critical `Violation`;
- force against a suspect with no recorded threat fact: serious `Violation`.

## Peaceful-resolution test

1. Enter Play Mode and confirm the mission HUD appears in the upper-left.
2. Select both officers with `3`.
3. Aim at the door and press `U`.
4. Aim at the north-room floor and press `G`.
5. Allow officers to challenge and resolve the suspect. Do not fire and do not press `K`.
6. Confirm automatic custody completes after genuine compliance and verified room clearance.
7. Confirm the report changes from `PROVISIONAL` to `FINAL`.
8. Expected result: all four objectives complete, no ROE violations, score 100, `Exemplary`.

The report finalizes automatically when every required objective reaches a terminal state. To end the operation manually, approach `M5_DebriefConsole` and hold the normal Interact control for 1.25 seconds. Any objective still pending when the operation ends is recorded as failed; the console is not a way to bypass unresolved duties.

The seeded suspect may refuse, flee, threaten, or deceptively surrender before genuine compliance. This is expected.

## ROE regression tests

Restart Play Mode before each scenario:

1. Fire at non-actor range geometry and confirm a `ReviewRequired` finding with no automatic deduction.
2. Fire at a free suspect who has no recorded threatening behavior and confirm a serious violation.
3. Fire at a surrendering or restrained suspect and confirm a critical violation.
4. Fire at the civilian and confirm both a critical ROE finding and failed protection objective.
5. If the suspect is recorded as threatening with an accessible weapon, confirm the firearm impact is classified `WithinPolicy`.

## Expected validation

The Console should include:

- `M5 Mission Assets: Mission objectives and threat-based ROE policy are explicit ScriptableObject assets.`
- `M5 Evaluation Architecture: Immutable evidence feeds pure objective, ROE, and report evaluators.`
- `M5 One-Way Evidence Boundary: Combat, AI, custody, and officer systems emit facts without depending on mission score or policy judgment.`
- `M5 Prototype Scene: Saved scene retains the training mission, ROE policy, debrief console, evidence sources, and after-action diagnostics.`

## Common problems

### Setup says Milestone 4 content is missing

Run the Milestone 4 build tool first and confirm `M4 Room Clearance` passes. Then rerun Milestone 5 setup.

### Mission HUD says controller unavailable

Exit Play Mode and rerun Milestone 5 setup. The setup saves and reloads the UI-to-controller reference and stops if persistence fails.

### Report remains provisional

At least one required objective is still pending. Read the objective list. The suspect must be physically restrained, the civilian must remain stable, both officers must remain actionable, and the north room must hold a verified clear state. You can use the debrief console to close the operation, but unresolved objectives will fail in the final report.

### Debrief console does not respond

Aim at the console from interaction range and hold the existing Interact binding until the progress completes. If no prompt appears, exit Play Mode and rerun the Milestone 5 setup tool so the Interactable layer and controller reference are rebuilt.

### A missed shot is not automatically called misconduct

This is intentional. The current force record cannot prove the intended target or full threat context for a non-actor impact, so it requires human review rather than an invented judgment.

## Stability gate

Do not close Milestone 5 until compilation, setup, validator, all tests, peaceful resolution, ROE scenarios, Milestones 1–4 regressions, and a clean Play Mode Console pass in Unity.
