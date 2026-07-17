# Development Roadmap

A milestone closes only after implementation is committed, Unity compiles with zero errors, validators and tests pass, earlier smoke tests pass, and documentation records the evidence.

## Prototype milestones

### Milestone 0 — Project Foundation — Complete

Project-owned structure, assemblies, logging, setup, validation, tests, and prototype scene.

### Milestone 1 — First-Person and Tactical Interaction — Complete

CharacterController locomotion, camera look, cursor/input modes, raycast interaction, prompt UI, door/panel examples, graybox, validation, and tests.

### Milestone 2 — Weapon and Force-Event Foundation — Complete

Manual per-magazine ammunition, chamber/bolt state, selector, low-ready/shouldered handling, retained/emergency reloads, qualitative magazine checks, physical muzzle raycast, reactive targets, and immutable discharge facts.

### Milestone 3 — Suspect, Civilian, Compliance, and Arrest — Complete

Deterministic human decisions, perception, stress/morale, surrender/deception, injury, custody, factual ledgers, AI Navigation, graybox actors, validation, and tests. Closed 2026-07-16.

### Milestone 4 — Officer AI and Command System — Integration Candidate

Goal: coordinate a small, interruptible team through explicit and inspectable commands.

Implemented: officer identity/condition reuse; Alpha/Bravo/Team selection; move, hold, follow, stack, open, restrain, context, and cancel orders; immutable player/initiative origin; explicit lifecycle/outcome reasons; physical path execution; gated door traversal; automatic visible-suspect challenges; two-officer timed room clearance; cover-gated automatic custody; markers; diagnostics; validator; build gate; and tests.

Exit pending: clean Unity compilation, setup/validator, automated tests, command/cancellation/door/arrest/initiative smoke tests, Milestones 1–3 regressions, and clean Console.

### Milestone 5 — Mission, ROE, and After-Action Review

Goal: evaluate immutable facts without allowing combat or AI code to manipulate score.

Planned: mission definition/phases, objectives, mission-specific ROE policy, incident aggregation, custody/evidence/safety/force evaluation, reasoned findings, and deterministic after-action reports.

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
10. Multiplayer after authority, determinism, and AI ownership are replication-ready.

## Regression policy

- Run the foundation and current-milestone validators.
- Check the Console after domain reload and Play Mode.
- Run all EditMode and PlayMode tests.
- Execute every earlier milestone smoke test.
- Update status, bugs, system map, changelog, and a completion record only after live evidence.
