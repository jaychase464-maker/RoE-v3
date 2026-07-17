# Milestone 4 Hotfix 4 — Bounded Officer Initiative

## Goal

Officers now act on immediate police responsibilities without waiting for the player to micromanage every safe action:

- a visible, capable suspect is challenged automatically;
- the team shares a challenge cooldown so both officers do not shout over one another;
- walls and closed doors block visual detection;
- a free, mobile suspect continues to block room clearance;
- a room requires two actionable officers and 2.5 continuous seconds without an active threat before it is verified clear;
- after clearance, the closest available officer automatically begins custody while the second officer remains available for cover;
- the existing physical approach, kneeling, timed handcuff application, and restraint verification remain mandatory;
- the automatic action fails if the room becomes unsafe, the subject breaks compliance, or cover leaves;
- civilians are never selected for automatic handcuffing;
- challenges and initiative custody decisions are recorded separately from player commands.

This is controlled initiative, not instant arrest. Officers cannot directly force a custody state.

Hotfix 4 is cumulative and includes Hotfixes 1 through 3.

## Install

1. Exit Play Mode and close Unity.
2. Extract `RulesOfEntry-Milestone-4-Hotfix-4.zip` directly into `C:\Users\Shadow\Videos\RoE v3`.
3. Merge folders and replace files.
4. Reopen Unity and wait for compilation.
5. Run `Tools > Rules of Entry > Milestone 4 > Build Officer Team Prototype` again. This recreates both officer prefabs and the north-room clearance volume.
6. Confirm the Milestone 4 validator passes.
7. Run all EditMode and PlayMode tests.

No Inspector assignments, input changes, or new packages are required.

## Test

1. Enter Play Mode and press `3` to select both officers.
2. Aim at the door and press `U`.
3. After it opens, aim at the north-room floor and press `G` so both officers enter.
4. Confirm one officer automatically challenges the visible suspect.
5. If the suspect refuses, flees, threatens, or only pretends to comply, confirm the room does not produce an unsafe instant arrest.
6. Once the suspect genuinely surrenders and both officers are inside, confirm the room reports a 2.5-second verification.
7. Do not press `K`. Confirm the closest officer approaches, directs the suspect to kneel, applies handcuffs over time, and verifies restraint while the partner remains for cover.
8. Confirm the civilian is not automatically handcuffed.
9. Confirm the right-side HUD identifies initiative activity and the Console remains free of errors.

The seeded suspect may refuse or deceptively surrender before genuinely complying. Repeated, spaced challenges are intentional and the exact sequence is deterministic for replay validation.

## Expected validation

The Console should include:

`M4 Room Clearance: The north training room requires two actionable officers and a timed no-threat verification before automatic custody.`

If setup, validation, or Play Mode fails, send the first error and the right-side officer outcome exactly as shown.
