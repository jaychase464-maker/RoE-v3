# Project Context

## Identity

- Title: Rules of Entry
- Engine: Unity
- Locked editor: `6000.5.2f1`
- Rendering: HDRP `17.5.0`
- Input: Input System `1.19.0`, New Input System only
- Navigation: AI Navigation `2.0.14`
- Source of truth: `jaychase464-maker/RoE-v3`, branch `main`

## Design direction

Rules of Entry models complete, accountable tactical law-enforcement operations. Officers, suspects, and civilians are simulated actors with physical and procedural constraints, not animation-driven props.

## Architecture rules

- Runtime assemblies own simulation; editor assemblies own generation and validation.
- Input intent enters through `TacticalPlayerInput`.
- Firearm, custody, and officer-order lifecycle rules live in pure state machines.
- `OfficerOrder` is immutable; execution status is stored separately.
- Every order records whether it originated from the player or bounded officer initiative.
- Officers accept, refuse, execute, fail, cancel, or complete orders with an explicit reason.
- Physical tasks require a complete NavMesh path; officer execution never teleports.
- Assisted restraint uses the authoritative custody state machine and may not skip kneeling or timed cuffing.
- Automatic custody requires a controlled/incapacitated suspect, two-officer timed room clearance, and continuing partner cover.
- Incident seed plus actor ID produces deterministic human decision rolls.
- Firearm, AI, custody, and officer systems emit immutable facts; none alters mission score directly.
- Unity object identity uses full 64-bit `EntityId` values.
- Exact ammunition and physiological values are not player-facing HUD data.
- Final art and animation adapt to gameplay contracts rather than own authoritative state.

## Protected regression baseline

- Milestone 0: structure, assemblies, logging, setup, validation, scene, and tests.
- Milestone 1: first-person movement/look, cursor control, interaction, prompts, and graybox.
- Milestone 2: manual firearm state, physical magazines/chamber/bolt, no auto reload, qualitative checks, physical muzzle, and force facts.
- Milestone 3: deterministic human behavior, perception, surrender/deception, injury, custody, navigation, and factual ledgers.

## Current honesty boundaries

- Officer movement uses individual NavMesh paths; bounded room-clear truth exists, but no formation planner, portal sweep, sector assignment, cover-position analysis, collision negotiation, or tactical firing exists yet.
- Stack points are geometric prototype positions and do not analyze hinges, fatal funnels, shields, or weapon sectors.
- Officers do not yet carry or fire production weapons.
- Assisted restraint has authoritative timing/state and cover authority but no handcuff prop, inverse kinematics, motion matching, or partner cover animation.
- Commands use prototype text and world markers; radio/voice, command wheel, gestures, and multiplayer replication are deferred.
- ROE scoring remains deferred; Milestone 4 records order and custody facts for the future evaluator.
