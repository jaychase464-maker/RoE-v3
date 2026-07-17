# Current Status

## Active milestone

**Milestone 1 — First-Person and Tactical Interaction: integration candidate**

Milestone 0 remains complete and stable. Milestone 1 code, setup tooling, validation, tests, and documentation have been prepared and require validation in the source-of-truth Unity project.

## Milestone 1 delivered for integration

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

## Required user validation

1. Import the package and allow Unity to compile with zero errors.
2. Run **Tools > Rules of Entry > Milestone 1 > Build Gameplay Prototype**.
3. Confirm Milestone 1 validation has zero errors.
4. Run all Edit Mode and Play Mode tests.
5. Complete the movement, crouch tunnel, door, panel, cursor, and clean-Console smoke test.

Milestone 1 is not stable until those results are recorded.

## Deferred to later milestones

- weapon handling and force-event capture — Milestone 2;
- suspect/civilian AI, verbal compliance, surrender, restraint, and arrest — Milestone 3;
- officer selection and commands — Milestone 4;
- objectives, ROE tracking, and after-action scoring — Milestone 5;
- production 3D art and animation — after graybox gameplay contracts are proven.

## Art requirement

No external 3D models are required for Milestone 1. The setup tool uses Unity primitives so movement scale, collision, and interaction can be validated before production assets are selected.
