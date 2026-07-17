# Development Roadmap

Development advances one stable milestone at a time. A milestone closes only after its implementation is committed, Unity compiles with zero errors, validation passes, automated and manual tests pass, and project documentation records the evidence.

## Prototype milestones

### Milestone 0 — Project Foundation — Complete

Established project ownership, assembly boundaries, structured logging, repeatable setup, automatic validation, tests, and a project-owned prototype scene.

### Milestone 1 — First-Person and Tactical Interaction — Complete

Goal: make a small HDRP graybox reliably playable and prove the input/interaction contracts used by later systems.

Delivered and validated:

- CharacterController first-person locomotion;
- mouse/gamepad look, walk, forward sprint, crouch, and stance-safe camera motion;
- cursor and gameplay-input mode handling;
- reusable interaction context, prompt, focus, instant-use, and hold-use contracts;
- interaction prompt UI;
- animated door and stateful hold-to-use panel examples;
- repeatable graybox, prefab, material, and layer setup;
- validation, build gate, and automated tests.

Exit passed on 2026-07-16: zero compiler and validation errors; all tests passed; movement, collision, crouch clearance, door, hold interaction, UI, and cursor passed the documented smoke test; Play Mode Console remained clean.

### Milestone 2 — Weapon and Force-Event Foundation

Goal: implement basic firearm operation while preserving every fact required for later accountability.

Planned:

- weapon/ammunition definitions;
- equip, aim, fire, reload, safe, and low-ready state;
- validated hitscan prototype and impact data;
- actor condition/damage foundation;
- weapon handling UI;
- immutable force-event emission;
- tests for weapon transitions and exactly-once force events.

Exit: weapon state cannot desynchronize, each shot emits exactly one force event, and combat code never changes mission score directly.

### Milestone 3 — Suspect, Civilian, Compliance, and Arrest

Goal: prove believable human behavior and a complete non-lethal custody path.

Planned:

- actor identities, roles, condition, and custody state;
- perception, memory, stress, morale, and explicit decision reasons;
- compatible AI Navigation package verification and navigation authoring;
- suspect hide, flee, resist, fight, comply, and deceptive-surrender behavior;
- civilian panic, freeze, flee, hide, and comply behavior;
- verbal-command stimuli and response evaluation;
- surrender, approach, restraint, search, and custody interactions;
- deterministic incident seeds and AI diagnostics.

Exit: subjects can be arrested without force and every failed compliance decision has an inspectable reason.

### Milestone 4 — Officer AI and Command System

Goal: coordinate a small, interruptible team.

Planned: officer selection, move/hold/cover/follow/stack/open/restrain/secure orders, explicit order lifecycle, refusal/failure reasons, world markers, and execution diagnostics.

Exit: two officers execute and cancel orders, report failures, and assist with arrest without blocking the player.

### Milestone 5 — Mission, ROE, and After-Action Review

Goal: turn the systems into an accountable tactical operation.

Planned: mission definitions/phases, objectives, mission-specific ROE policy, immutable incident ledger, custody/evidence/safety/force evaluation, reasoned penalties, and deterministic after-action reporting.

Exit: the same incident ledger always produces the same objective, ROE, and after-action results.

### Milestone 6 — First Tactical Vertical Slice

Goal: deliver one short replayable residence or storefront incident with two officers, one uncertain suspect, one civilian, multiple approaches, seeded placement/behavior, arrest and force outcomes, and a complete after-action report.

Exit: three full runs complete without blockers, seeded outcomes differ meaningfully, communication/arrest can succeed, justified and unjustified force are distinguished, and no missing references or unhandled exceptions occur.

## Expansion after the vertical slice

1. Briefing, intelligence uncertainty, floor plans, loadouts, assignments, and entry planning.
2. Negotiation and barricaded-subject behavior.
3. Patrol perimeter, traffic, detention, evacuation, and escape interception.
4. Tactical medicine, fire/EMS staging, treatment, and handoff.
5. K9 search and apprehension.
6. Drone reconnaissance and overwatch.
7. Sniper/spotter, bomb technician, and command-post systems.
8. Evidence, detectives, scene security, and investigative consequences.
9. Career progression, injuries, performance reviews, and persistent consequences.
10. Multiplayer only after authority, determinism, and AI ownership are designed for replication.

## Regression policy

Before closing every milestone:

- run the foundation and current-milestone validators;
- check the Console after domain reload and Play Mode;
- run all Edit Mode and Play Mode tests;
- execute every earlier milestone smoke test;
- update `CURRENT_STATUS.md`, `BUGS.md`, `SYSTEM_MAP.md`, and `CHANGELOG.md`;
- record packages, project settings, generated assets, scene changes, and required Inspector assignments.
