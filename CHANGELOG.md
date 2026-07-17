# Changelog

## Milestone 4 Hotfix 4 — Bounded Officer Initiative — 2026-07-16

- Added automatic line-of-sight suspect challenges with a team-wide repeat cooldown.
- Added a reusable tactical room volume and pure room-clearance policy.
- The north training room requires two actionable officers and 2.5 continuous seconds without an active threat before clear status is verified.
- Added automatic custody assignment for surrendered, kneeling, or incapacitated suspects after verified clearance.
- Automatic custody uses the existing physical navigation, kneeling, timed cuffing, and verification sequence.
- Clearance loss, renewed threats, lost cover, non-compliance, and deceptive surrender can stop or fail the action.
- Civilians and free capable suspects cannot become automatic custody targets.
- Added immutable player/initiative order origin and append-only officer-initiative records.
- Updated the diagnostic HUD, setup tool, saved-scene validator, and tests.
- Includes Hotfixes 1 through 3; live Unity revalidation is pending.

## Milestone 4 Hotfix 3 — Physical Door Navigation — 2026-07-16

- Fixed officers being unable to path through the training doorway after physically opening its door.
- Added a fixed bidirectional `NavMeshLink` across the closed-bake navigation gap.
- The link remains blocked until the physical door leaf reaches its clearance threshold and blocks immediately when closing is ordered.
- Open-door orders now wait for verified physical clearance before completing.
- Officer agents traverse the link through normal NavMesh movement; no warp or teleport behavior was added.
- Setup creates the link automatically, validation inspects its saved configuration, and a PlayMode test covers the gate lifecycle.
- Added `Unity.AI.Navigation` to the runtime and PlayMode test assembly references.
- Includes Hotfix 1 and Hotfix 2 changes.
- Live Unity traversal test passed; the closed-door navigation defect is resolved.

## Milestone 4 Hotfix 2 — Saved Squad References — 2026-07-17

- Fixed scene-only squad references not being guaranteed as prefab-instance overrides.
- Setup now records component property modifications, saves the scene, reloads it from disk, and verifies all required references before reporting success.
- The validator now inspects the saved scene's `OfficerSquadController` rather than relying only on prefab existence and dependency text.
- Runtime and validator errors identify the exact missing reference.
- Includes Hotfix 1 input persistence and diagnostic changes.
- No officer order, custody, firearm, package, render, or level-layout change.
- Live Unity test passed; the saved squad-reference error is resolved.

## Milestone 4 Hotfix 1 — Input Action Persistence — 2026-07-16

- Fixed generated officer actions disappearing after a Play Mode/domain reload.
- Setup now persists the JSON-backed `.inputactions` source, forces a synchronous reimport, and reloads the authoritative asset before player assignment.
- Missing-action diagnostics now report exact map/action paths once instead of flooding the Console from multiple HUD queries.
- No officer execution, custody, firearm, navigation, package, render, or scene-layout change.
- Live Unity test passed; the missing-action error is resolved.

## Milestone 4 — Officer AI and Command System — Integration Candidate 2026-07-16

### Added

- Officer Alpha and Bravo prefabs with identity, condition, equipment facts, hit regions, navigation, visual diagnostics, and order history.
- Immutable per-officer orders and a pure lifecycle state machine.
- Pending, accepted, executing, completed, cancelled, failed, and refused order facts.
- Specific execution/refusal causes and append-only officer-order ledgers.
- Individual/team selection; move, hold, follow, stack, open, restrain, context, and cancel commands.
- Complete-path NavMesh execution, team spacing, timeout, supersession, and cancellation.
- Door approach and interaction execution.
- Assisted restraint with compliance validation, kneeling, settle timing, cuffing timing, and custody verification.
- World order marker, command diagnostic HUD, setup tool, validator, build gate, EditMode tests, and PlayMode ledger test.

### Changed

- `TacticalPlayerInput` resolves and exposes Milestone 4 Input System actions.
- `ProjectInfo.CurrentMilestone` identifies Milestone 4.
- The setup tool adds officer actions to `ROE_InputActions.inputactions` without replacing existing controls.
- Player prefab receives `OfficerSquadController`; the scene instance receives Alpha/Bravo/marker references.
- Documentation now tracks Milestone 4 as an integration candidate.

### Packages/settings

- No new package dependency.
- No render-pipeline, active-input-handler, physics-layer, or quality-setting change.
- Existing AI Navigation `2.0.14` remains required.

### Validation pending

- Live Unity compilation, setup, validator, all tests, command/arrest smoke tests, Milestones 1–3 regressions, and clean Console.

## Milestone 3 Validation Complete — 2026-07-16

- Unity compilation, AI Navigation resolution, setup, validator, all tests, seeded behavior/custody smoke tests, earlier regressions, and clean Console passed.
- Milestone 3 is the protected baseline for Milestone 4.

## Milestone 3 Hotfixes — 2026-07-16

- Fixed `CS0104` with an explicit Package Manager `PackageInfo` alias.
- Fixed the score-boundary validator false positive caused by `distancePenalty`.

## Milestones 0–2

- Milestone 0 established the project foundation.
- Milestone 1 delivered first-person movement and tactical interaction.
- Milestone 2 delivered manual firearm mechanics and immutable force events.
