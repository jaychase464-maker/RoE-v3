# Current Status

## Active milestone

**Milestone 4 — Officer AI and Command System: integration candidate; live validation pending**

Milestone 3 is complete and stable. Milestone 4 implementation is packaged but must pass Unity compilation, setup, validation, tests, smoke testing, earlier regressions, and a clean Console before closure.

Hotfix 1 fixed generated officer actions not surviving the JSON asset reimport/domain reload. The persistence fix and non-spamming exact diagnostics passed the user's live Unity test.

Hotfix 2 fixed scene-only squad references not being retained as prefab-instance overrides. The save/reload verification and Play Mode squad configuration passed the user's live Unity test.

Hotfix 3 fixed the doorway navigation defect. The fixed bidirectional link and physical-clearance gate passed the user's live Unity traversal test.

Hotfix 4 adds bounded officer initiative. Officers automatically challenge visible suspects. Automatic custody is authorized only after a bounded room has two actionable officers, every suspect is controlled or incapacitated, and the no-threat condition remains stable for 2.5 seconds. The closest officer then uses the existing physical custody sequence while the other provides cover. Unity revalidation is pending.

## Implemented in the candidate

- Officer Alpha and Bravo actor identities, conditions, inventories, hit regions, NavMesh agents, visuals, and order ledgers.
- Individual Alpha, individual Bravo, and Team selection.
- Move, hold, follow, stack-at-door, open-door, restrain-subject, contextual, and cancellation input.
- Immutable `OfficerOrder` facts with issuer, assigned officer, target, sequence, and issued time.
- Pure lifecycle transitions for pending, accepted, executing, completed, cancelled, failed, and refused orders.
- Specific outcomes including incapacity, invalid target, no navigation surface, no path, unreachable target, non-compliance, supersession, player cancellation, and timeout.
- Physical path calculation and movement without `NavMeshAgent.Warp`.
- Door approach, physical opening, clearance wait, and gated NavMesh traversal without teleportation.
- Line-of-sight suspect challenges, shared challenge discipline, timed room-clear verification, cover requirements, and automatic controlled custody.
- Separate append-only facts for player-command and officer-initiative actions.
- Restraint assistance with physical approach, compliant-state validation, kneeling direction, settle time, cuffing time, and verified custody transition.
- Command marker, right-side diagnostics, setup generator, validator, build gate, and tests.

## Validation pending

- Unity `6000.5.2f1` compilation.
- Milestone 4 setup and validator.
- All EditMode and PlayMode tests.
- Individual/team movement, cancellation, follow, door, non-compliant restraint refusal, and cooperative arrest smoke tests.
- Open-door cross-threshold movement for Alpha, Bravo, and Team, plus closed-door path rejection.
- Automatic challenge, refusal/deception handling, room-clear verification, automatic cuffing, cover-loss abort, and civilian exclusion.
- Milestones 1–3 regression checks.
- Clean Play Mode Console.

## Art requirement

No external 3D model is required for Milestone 4 validation. Generated primitives make selection, paths, timing, and lifecycle failures visible. After the command contract is stable, the first production model package should include a rigged tactical officer body, uniform/armor variants, hands, duty belt, radio, handcuffs, and animation-ready equipment attachment points.
