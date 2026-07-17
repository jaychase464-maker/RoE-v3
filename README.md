# Rules of Entry

Rules of Entry is a hyper-realistic first-person tactical law-enforcement game built in Unity. Its long-term goal is to simulate complete police operations: tactical entry, patrol containment, negotiation, K9, drones, tactical medicine, fire/EMS coordination, evidence handling, command-post activity, and accountable after-action review.

## Project facts

- Repository: `jaychase464-maker/RoE-v3`
- Unity: `6000.5.2f1`
- Render pipeline: HDRP `17.5.0`
- Input: Unity Input System `1.19.0`; New Input System only
- Current stage: clean HDRP starter project; gameplay development has not begun
- Current milestone: Milestone 0 — Project Foundation

## Current prototype target

The first playable prototype will contain:

- first-person tactical movement and camera control;
- a reusable interaction system;
- basic firearm handling;
- one small graybox test environment;
- suspect and civilian behavior;
- verbal compliance, surrender, restraint, and arrest;
- officer selection and basic commands;
- mission objectives, rules-of-engagement tracking, and after-action scoring.

Advanced specialties are intentionally deferred until the core actor, command, mission, and accountability systems are stable.

## Start here

Read these files before making changes:

1. `CHATGPT_START_HERE.md`
2. `PROJECT_CONTEXT.md`
3. `CURRENT_STATUS.md`
4. `SYSTEM_MAP.md`
5. `DEVELOPMENT_ROADMAP.md`
6. `BUGS.md`
7. `CHANGELOG.md`

## Development guardrails

- Treat this repository as the source of truth.
- Use Unity `6000.5.2f1`; do not silently upgrade the editor or packages.
- Use the current Input System. Do not use `UnityEngine.Input`.
- Keep project-owned content under `Assets/_Project/RulesOfEntry/`.
- Keep third-party assets outside the project-owned code folders.
- Do not add a dependency without documenting why it is needed.
- Do not report a milestone complete until Unity opens with zero compiler errors and its exit checklist passes.
- When replacing an existing C# file, provide the complete updated file.

## Opening the project

1. Clone or pull the repository.
2. Open its root folder in Unity Hub with Unity `6000.5.2f1`.
3. Allow Package Manager and shader imports to finish.
4. Open `Assets/OutdoorsScene.unity`.
5. Confirm the Console has zero compiler errors before beginning a milestone.

The current scene contains only the HDRP starter camera, sun, sky/fog volume, and static-lighting sky. A controllable player does not yet exist.
