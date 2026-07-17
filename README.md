# Rules of Entry

Rules of Entry is a hyper-realistic first-person tactical law-enforcement game built with Unity. Its long-term goal is to simulate complete, accountable police operations rather than an isolated entry team.

## Project facts

- Repository: `jaychase464-maker/RoE-v3`
- Unity: `6000.5.2f1`
- Render pipeline: HDRP `17.5.0`
- Input: Unity Input System `1.19.0`; New Input System only
- UI: uGUI `2.5.0`
- Test Framework: `1.7.0`
- Milestone 0: complete and stable
- Milestone 1: complete and stable
- Current milestone: Milestone 2 — Weapon and Force-Event Foundation

## Implemented foundation

- project-owned runtime, editor, and test assemblies;
- organized `Assets/_Project/RulesOfEntry/` structure;
- project identity and structured logging;
- repeatable setup and automatic validation;
- project-owned prototype scene;
- stable Milestone 0 Edit Mode, Play Mode, and smoke-test baseline.

## Stable Milestone 1 gameplay foundation

- CharacterController movement with walk, sprint, and crouch;
- mouse/gamepad camera look and cursor handling;
- project-owned tactical input actions;
- reusable instant and hold interaction framework;
- interaction prompt UI;
- prototype door and control panel;
- repeatable HDRP graybox environment builder;
- Milestone 1 project/build validation and tests.

Unity compilation, project validation, all Edit Mode and Play Mode tests, the manual gameplay smoke test, and the clean-Console check passed on 2026-07-16.

See `MILESTONE_1_INSTALL.md` for installation, controls, setup, and regression testing.

## Development guardrails

- Treat this repository as the source of truth.
- Use Unity `6000.5.2f1`; do not silently upgrade the editor or packages.
- Use the current Input System. Do not use `UnityEngine.Input`.
- Keep project-owned content under `Assets/_Project/RulesOfEntry/`.
- Do not add dependencies without documenting them.
- Do not call a milestone stable until Unity compilation, validation, automated tests, and its manual smoke test pass.
