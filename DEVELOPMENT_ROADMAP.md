# Development Roadmap

A milestone closes only after implementation is committed, Unity compiles with zero errors, validators and tests pass, earlier smoke tests pass, and documentation records the evidence.

## Prototype milestones

### Milestone 0 — Project Foundation — Complete

Project-owned structure, assemblies, logging, setup, validation, tests, and prototype scene.

### Milestone 1 — First-Person and Tactical Interaction — Complete

CharacterController locomotion, camera look, cursor/input modes, raycast interaction, prompt UI, door/panel examples, graybox, validation, and tests.

### Milestone 2 — Weapon and Force-Event Foundation — Complete

Manual per-magazine ammunition, chamber/bolt state, selector, low-ready/shouldered handling, manual retained/emergency reloads, magazine checks without an ammo counter, physical muzzle raycast, reactive targets, and immutable discharge facts.

Milestone 2 regression behavior and tests passed again during Milestone 3 closure.

### Milestone 3 — Suspect, Civilian, Compliance, and Arrest — Complete

Goal: prove believable, explainable human behavior and a complete non-lethal custody path.

Delivered and validated:

- identity, role, condition, inventory, custody, and hit-region contracts;
- perception, memory, stress, morale, profiles, deterministic rolls, and explicit decision reasons;
- suspect resistance, threat, flight, hiding, surrender, and deceptive surrender;
- civilian surrender, panic, freeze, flight, and hiding;
- verbal command stimuli and weapon-presentation context;
- procedural kneel, restraint, search, and custody interactions;
- immutable decision/custody events and pre-impact actor facts;
- AI Navigation package, bake, diagnostics, setup, validation, build gate, and tests.

Exit passed on 2026-07-16: subjects were arrested without force, invalid custody shortcuts failed, the seeded scenario reproduced, every command decision retained an inspectable reason, all automated/regression checks passed, and the Console remained clean.

### Milestone 4 — Officer AI and Command System

Goal: coordinate a small, interruptible team.

Planned: officer identity/condition reuse, selection, move/hold/cover/follow/stack/open/restrain/secure orders, explicit order lifecycle, refusal/failure reasons, world markers, door/NavMesh coordination, and two-officer assistance with arrest.

### Milestone 5 — Mission, ROE, and After-Action Review

Goal: evaluate immutable facts without allowing combat or AI code to manipulate score.

Planned: mission definition/phases, objectives, mission-specific ROE policy, incident ledger aggregation, custody/evidence/safety/force evaluation, reasoned findings, and deterministic after-action reports.

### Milestone 6 — First Tactical Vertical Slice

Goal: one short, replayable residence or storefront incident with two officers, one uncertain suspect, one civilian, multiple approaches, seeded behavior, arrest and force outcomes, and complete after-action review.

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
10. Multiplayer only after authority, determinism, and AI ownership are replication-ready.

## Regression policy

- Run the foundation and current-milestone validators.
- Check the Console after domain reload and Play Mode.
- Run all EditMode and PlayMode tests.
- Execute every earlier milestone smoke test.
- Update `CURRENT_STATUS.md`, `BUGS.md`, `SYSTEM_MAP.md`, and `CHANGELOG.md`.
- Record packages, settings, generated assets, scene changes, Inspector assignments, and exact live evidence.
