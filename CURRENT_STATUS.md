# Current Status

## Active milestone

**Milestone 2 — Weapon and Force-Event Foundation: planning**

Milestones 0 and 1 are complete and stable. The first-person graybox and tactical interaction foundation are now the protected regression baseline for Milestone 2.

## Milestone 1 completed

- Project-owned `ROE_InputActions.inputactions` with keyboard/mouse and gamepad bindings.
- CharacterController movement with walk, forward sprint, crouch, acceleration, gravity, and obstruction-safe standing.
- First-person camera look with separate mouse and gamepad response.
- Escape/Start cursor release and gameplay-map switching.
- Raycast interaction focus with instant and hold interactions.
- Interaction prompt UI with binding label and hold-progress display.
- Animated training door and stateful training control panel.
- Repeatable editor tool that builds all prefabs, HDRP materials, and the prototype graybox.
- Milestone 1 validator and build gate.
- Edit Mode configuration test and Play Mode interaction tests.

## Milestone 1 validation — passed 2026-07-16

- Unity compilation completed with zero errors.
- The Milestone 1 setup and project validation completed with zero errors.
- All Edit Mode tests passed.
- All Play Mode tests passed.
- Movement, sprint, crouch clearance, collision, door interaction, hold interaction, prompt UI, and cursor behavior worked correctly.
- Play Mode completed with no Console errors or exceptions.

## Milestone 2 objective

Implement basic firearm handling and immutable use-of-force event capture without coupling combat code to mission score or future ROE decisions.

## Deferred to later milestones

- suspect/civilian AI, verbal compliance, surrender, restraint, and arrest — Milestone 3;
- officer selection and commands — Milestone 4;
- objectives, ROE tracking, and after-action scoring — Milestone 5;
- production 3D art and animation — after graybox gameplay contracts are proven.

## Art requirement

No external 3D models were required for Milestone 1. Before Milestone 2 weapon presentation begins, the approved temporary weapon model requirements will be documented separately; gameplay state and force-event logic should be implemented before final art is required.
