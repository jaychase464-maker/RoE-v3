# Development Roadmap

Development moves one stable milestone at a time. A milestone is complete only after its implementation is committed, Unity compiles with zero errors, the test checklist passes, documentation is updated, and regressions are recorded.

## Prototype milestones

### Milestone 0 — Project Foundation

Goal: establish project ownership, dependency boundaries, validation, and a trustworthy clean baseline.

Deliverables:

- project-owned folder structure;
- runtime/editor/test assembly definitions;
- project identity and logging foundation;
- editor validation report;
- prototype scene copied from and separated from the HDRP template scene;
- documentation synchronized with the repository.

Exit: fresh checkout opens in Unity `6000.5.2f1`, validator passes, and Console has zero errors.

### Milestone 1 — First-Person and Tactical Interaction

Goal: make a small graybox environment reliably playable.

Deliverables:

- CharacterController-based first-person movement;
- mouse/gamepad look, walk, sprint, crouch, and stance-safe camera motion;
- cursor/input-mode handling;
- reusable focus and interaction contracts;
- interaction prompt UI;
- doors, simple pickups, and a restraint test target;
- editor tool that constructs the graybox test environment repeatably.

Exit: player can traverse the environment, focus targets, and complete interactions without input, cursor, collision, or missing-reference errors.

### Milestone 2 — Weapon and Force-Event Foundation

Goal: implement basic firearm operation while capturing every use-of-force fact needed later.

Deliverables:

- weapon and ammunition definitions;
- equip, aim, fire, reload, safe/low-ready states;
- hitscan prototype with validated layers and impact data;
- actor condition/damage foundation;
- weapon handling UI;
- immutable force-event emission;
- Edit Mode tests for weapon state transitions and event creation.

Exit: weapon state cannot desynchronize, shots generate exactly one force event, and no mission score is directly changed by combat code.

### Milestone 3 — Suspect, Civilian, Compliance, and Arrest

Goal: prove believable non-player behavior without treating people as simple targets.

Deliverables:

- actor identities and roles;
- perception, memory, suspicion, stress, morale, and decision context;
- navigation authoring after the compatible AI Navigation package is verified;
- suspect states including hide, flee, resist, fight, comply, and deceptive surrender;
- civilian states including panic, freeze, flee, hide, and comply;
- verbal command stimuli and response calculation;
- surrender, approach, restrain, search, and custody interactions;
- deterministic incident seed and AI debug overlay.

Exit: repeated seeded tests produce explainable behavior, subjects can be arrested without force, and failed compliance has a visible reason rather than a random opaque result.

### Milestone 4 — Officer AI and Command System

Goal: let the player coordinate a small team through explicit, interruptible orders.

Deliverables:

- officer actor and tactical locomotion;
- selection of officer/fireteam;
- move, hold, cover, follow, stack, open, restrain, and secure commands;
- order queue with accepted, executing, completed, failed, and cancelled states;
- officer safety checks and command refusal reasons;
- command UI and world markers;
- command execution diagnostics.

Exit: two officers can navigate the graybox, execute and cancel orders, report failures, and assist with arrest without blocking the player.

### Milestone 5 — Mission, ROE, and After-Action Review

Goal: turn the systems into an accountable tactical operation.

Deliverables:

- mission definition and runtime controller;
- objectives and mission phases;
- mission-specific ROE policy definition;
- evidence, custody, civilian safety, officer safety, and force evaluation;
- penalties with explicit reason and evidence;
- after-action report with timeline and performance categories;
- tests for justified/unjustified force and objective outcomes.

Exit: the same incident ledger deterministically produces the same objective result, ROE findings, and AAR.

### Milestone 6 — First Tactical Vertical Slice

Goal: deliver one short, replayable incident that exercises the full prototype loop.

Scenario: small residence or storefront with two officers, one uncertain suspect, one civilian, multiple entry routes, randomized subject placement/state, arrest and force outcomes, and a complete AAR.

Exit:

- three full runs complete without blocker defects;
- at least three meaningfully different seeded outcomes are observed;
- player can succeed through communication and arrest;
- justified and unjustified force are distinguished correctly;
- no missing references, compiler errors, or unhandled exceptions;
- performance baseline is recorded.

## Expansion after the vertical slice

1. Planning, briefing, floor plans, loadouts, entry selection, and intel uncertainty.
2. Negotiation and barricaded-subject behavior.
3. Patrol perimeter, traffic control, detention, evacuation, and escape interception.
4. Tactical medics, injury treatment, fire, and EMS handoff.
5. K9 search and apprehension.
6. Drone reconnaissance and overwatch.
7. Sniper/spotter, bomb technician, and command-post systems.
8. Evidence, detectives, scene handoff, and investigation consequences.
9. Career/campaign progression, personnel injuries, performance reviews, and persistent consequences.
10. Multiplayer only after authority, determinism, and AI ownership boundaries are designed for replication.

## Regression policy

Before closing every milestone:

- run the project validator;
- check the Unity Console after domain reload and Play Mode;
- run available Edit Mode and Play Mode tests;
- execute every earlier milestone's smoke test;
- update `CURRENT_STATUS.md`, `BUGS.md`, `SYSTEM_MAP.md`, and `CHANGELOG.md`;
- record Inspector assignments, required packages, project settings, and scene changes.
