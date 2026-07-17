# Project Context

## Vision

Rules of Entry is a first-person tactical law-enforcement simulation centered on believable procedure, coordinated teams, dynamic human behavior, civilian safety, and accountability. The player should feel like one participant in a complete operation rather than an isolated shooter moving through a level.

## Design pillars

1. Procedure before spectacle.
2. Human behavior instead of target AI.
3. Coordination across officers and specialties.
4. Civilian and hostage preservation.
5. Rules of engagement evaluated from context.
6. Uncertain, replayable incidents.
7. Clear consequences and after-action review.
8. Realistic-feeling equipment without sacrificing usable controls.

## Repository audit — 2026-07-16

The initial commit was inspected before proposing gameplay architecture.

| Area | Verified state |
|---|---|
| Unity editor | `6000.5.2f1` (`eb73d3b415a1`) |
| Render pipeline | HDRP `17.5.0`; High Fidelity is the current quality tier |
| Input | Input System `1.19.0`; `activeInputHandler: 1` (new system only) |
| Scenes | `Assets/OutdoorsScene.unity`, enabled in Build Settings |
| Scene contents | Sun, Sky and Fog Volume, Main Camera, StaticLightingSky |
| Input asset | `Assets/InputSystem_Actions.inputactions` with generic Player and UI maps |
| Gameplay scripts | None |
| Existing C# | Unity template `Readme.cs` and `ReadmeEditor.cs` only |
| Prefabs | None |
| Project tags | None added |
| Custom layers | None added |
| Assembly definitions | None |
| Tests | None |
| Documentation | None at repository root before this foundation pack |
| Confirmed missing scene scripts | None detected in serialized scene data |
| Live Unity compiler state | Not available from repository data; must be checked in the editor |

## Installed direct packages relevant to development

- `com.unity.inputsystem` `1.19.0`
- `com.unity.render-pipelines.high-definition` `17.5.0`
- `com.unity.timeline` `1.8.12`
- `com.unity.ugui` `2.5.0`
- `com.unity.visualscripting` `1.9.11`
- `com.unity.modules.ai` `1.0.0`
- `com.unity.modules.physics` `1.0.0`

The AI Navigation authoring package is not currently declared. Its Unity-compatible version must be resolved through Package Manager before NavMesh baking components are used; do not guess a version in `manifest.json`.

## Prototype boundaries

The first prototype is single-player and graybox-first. It proves player control, interactions, force events, compliance, arrests, officer commands, mission objectives, ROE evaluation, and after-action reporting.

Not part of the first prototype:

- multiplayer or replication;
- campaign progression;
- final weapon animation and audio;
- drones, K9, negotiators, snipers, bomb technicians, medics, fire, or EMS;
- large crowds or city-scale perimeter simulation;
- final art, optimization, or platform certification.

## Technical principles

- Use composition for actors; do not build a single inheritance tree for every person type.
- Store authoring data in validated ScriptableObjects and runtime state in scene/runtime components.
- Make AI decisions explicit and inspectable with named states and reasons.
- Route consequential actions through events so mission logic, ROE, UI, and AAR use the same facts.
- Score from an immutable incident ledger rather than scattering score mutations through gameplay scripts.
- Give every mission a seed so incidents can be reproduced during debugging.
- Prefer small interfaces and dependencies assigned in the Inspector over global object searches.
- Use editor validation to catch missing references before Play Mode.
- Add automated tests around pure decision, ROE, and scoring logic first.
