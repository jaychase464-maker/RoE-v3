# Changelog

## Milestone 3 Validation Complete — 2026-07-16

- AI Navigation resolution and Unity compilation passed.
- Milestone 3 setup and validator passed with zero errors.
- All EditMode and PlayMode tests passed.
- Seeded civilian compliance, suspect resistance, deceptive surrender, and surrender-abandonment behavior passed.
- Kneel, restraint, search, weapon recovery, and custody confirmation passed.
- Milestone 1 and 2 regressions passed.
- Play Mode Console remained clean.
- Milestone 3 is closed and becomes the regression baseline for Milestone 4.

## Milestone 3 Hotfix 2 — Score Boundary Validation — 2026-07-16

- Fixed the force-context validator false positive caused by `distancePenalty` containing the substring `Penalty`.
- Accountability checks now match standalone prohibited identifiers only.
- No runtime, AI decision, scene, package, or Inspector change.

## Milestone 3 Hotfix 1 — PackageInfo Alias — 2026-07-16

- Fixed `CS0104` in `RulesOfEntryMilestoneThreeValidator.cs` by explicitly selecting `UnityEditor.PackageManager.PackageInfo`.
- No runtime, scene, package-version, or Inspector change.

## Milestone 3 — Human Behavior and Custody — Integration Candidate 2026-07-16

### Added

- Actor identity, role, condition, inventory, hit regions, and runtime EntityId access.
- Region-aware prototype ballistic trauma, bleeding, consciousness, and mobility.
- Human behavior profiles, deterministic decision random, command contexts, explicit reasons, and immutable decision ledger.
- Sight/hearing perception and last-known officer memory.
- Suspect surrender, deceptive surrender, hide, flee, resist, and threat responses.
- Civilian surrender, freeze, hide, flee, and panic responses.
- Police command emitter and prototype AI diagnostics.
- Procedural custody state machine and interaction flow.
- Immutable custody records with actor/officer 64-bit EntityIds.
- AI Navigation `2.0.14`, NavMesh actor movement, generated NavMesh data, actor prefabs, setup, validator, build gate, and tests.

### Changed

- `ROE_InputActions.inputactions` adds `IssueCommand` on keyboard `F` and gamepad left shoulder.
- `TacticalPlayerInput` exposes command intent and binding display.
- Player prefab receives `VerbalCommandEmitter`.
- Force-event records now contain factual pre-impact actor role, condition, custody, behavior, and accessible-weapon state.
- Runtime/editor assembly definitions support the navigation integration.
- `ProjectInfo.CurrentMilestone` identifies Milestone 3.

### Packages/settings

- Added `com.unity.ai.navigation 2.0.14` to `Packages/manifest.json`.
- No render-pipeline, active-input-handler, physics-layer, or quality-setting change.

### Validation pending

- Live package resolution, Unity compilation, setup, validators, all tests, M1/M2/M3 smoke tests, and clean Console.

## Milestone 2 Hotfix 1 — Unity 6000.5 Entity IDs — 2026-07-16

- Replaced compile-blocking `Object.GetInstanceID()` with `Object.GetEntityId()`.
- Preserved complete identifiers with `EntityId.ToULong(...)`.

## Milestones 0–2

- Milestone 0 established the project foundation.
- Milestone 1 delivered first-person movement and tactical interaction.
- Milestone 2 delivered manual firearm mechanics and immutable force events.
