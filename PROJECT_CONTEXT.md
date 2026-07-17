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

Rules of Entry models complete, accountable tactical law-enforcement operations. Suspects and civilians are people with perception limits, stress, morale, injury, misunderstanding, deception, and custody state—not target categories.

## Architecture rules

- Runtime assemblies own simulation; editor assemblies own project generation and validation.
- Input intent enters through `TacticalPlayerInput`.
- Pure state machines own firearm and custody transitions.
- Decision models return explicit reasons and never hide a failed choice inside an animation controller.
- Incident seed plus actor ID produces deterministic AI decision rolls.
- Firearm, AI, and custody systems emit immutable facts; none may alter mission score directly.
- Unity object identity uses full 64-bit `EntityId` values.
- Exact ammunition and physiological values are not player-facing HUD data.
- Final art and animation must adapt to gameplay contracts rather than own authoritative state.

## Protected regression baseline

- Milestone 0: structure, assemblies, logging, setup, validation, scene, and tests.
- Milestone 1: first-person movement/look, cursor control, interaction, prompts, and graybox.
- Milestone 2: manual firearm state, per-magazine rounds, chamber/bolt, no auto reload, qualitative magazine checks, physical muzzle raycast, and force-event ledger.

## Current honesty boundaries

- Navigation uses a baked prototype NavMesh and does not yet coordinate doors, links, crowds, or tactical formations.
- Human condition is region-aware but not a validated medical or terminal-ballistics model.
- Threatening suspects do not yet possess production weapons, attacks, cover selection, or animations.
- Commands use text/debug feedback; recorded voice, facial animation, and full command selection are deferred.
- ROE scoring remains deferred; Milestone 3 records the facts needed by the later evaluator.
