# Project Context

## Identity

- Title: Rules of Entry
- Engine: Unity `6000.5.2f1`
- Rendering: HDRP `17.5.0`
- Input: Input System `1.19.0`, New Input System only
- Navigation: AI Navigation `2.0.14`
- Source of truth: `jaychase464-maker/RoE-v3`, branch `main`

## Architecture rules

- Runtime assemblies own simulation; editor assemblies own generation and validation.
- Firearm, AI-decision, custody, officer-order, and initiative systems emit factual records.
- Mission evaluation is a one-way consumer and cannot change combat, behavior, custody, navigation, or orders.
- `MissionEvidenceSnapshot`, objective results, ROE findings, and `AfterActionReport` are immutable to consumers.
- Ambiguous evidence produces `ReviewRequired`, not an invented compliant or misconduct determination.
- A completed operation reports both objective outcome and professional-conduct findings.
- Scoring methodology is explicit; critical misconduct and required-objective failure impose rating caps.
- No automatic reload, exact ammunition HUD, navigation warp, instant arrest, or direct score mutation is allowed.
- Unity object identity uses full 64-bit `EntityId` values.

## Protected baseline

- Milestones 0–3: foundation, player interaction, manual firearm, deterministic human behavior, and custody.
- Milestone 4: officer commands, gated door traversal, automatic suspect challenge, timed room clearance, cover-gated custody, and factual order/initiative ledgers.

## Current honesty boundaries

- ROE evaluation uses the pre-impact facts currently available; it is not a complete legal judgment engine.
- Non-actor impacts cannot establish intended target and therefore require review.
- Terminal ballistics, penetration, crossfire risk, backdrop, target identification quality, warnings, tactical feasibility, and complete officer perception history remain future work.
- The numeric score is a transparent prototype performance summary, not a substitute for a narrative professional review.
- The UI is diagnostic and not final presentation.
