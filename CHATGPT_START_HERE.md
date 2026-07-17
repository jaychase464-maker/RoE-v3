# ChatGPT Start Here

## Project

Rules of Entry is a Unity `6000.5.2f1` HDRP tactical law-enforcement simulation. GitHub repository `jaychase464-maker/RoE-v3`, branch `main`, is the source of truth.

## Current state

- Milestones 0–3 are complete and validated.
- Milestone 4 officer AI and tactical commands is implemented as an integration candidate.
- Live Milestone 4 validation is not yet recorded; do not call it complete.

Read in order:

1. `PROJECT_CONTEXT.md`
2. `CURRENT_STATUS.md`
3. `SYSTEM_MAP.md`
4. `DEVELOPMENT_ROADMAP.md`
5. `BUGS.md`
6. `MILESTONE_4_INSTALL.md`

## Non-negotiable invariants

- No automatic reload and no exact player ammo counter.
- Unity object identifiers use `GetEntityId()` and preserve all 64 bits.
- AI decisions are reproducible and record an explicit reason.
- No capable free subject may be instantly restrained or searched.
- Officer initiative may challenge a visible suspect, but automatic custody requires a controlled or incapacitated suspect, verified room clearance, and actionable partner cover.
- Officer movement must use physical complete paths; no navigation warp or teleport.
- Every officer order retains lifecycle and terminal outcome facts.
- Combat, AI, custody, and officer code emit facts and never mutate mission score.
- Do not claim live Unity success until compilation, validation, tests, smoke tests, regressions, and Console evidence exist.
- Do not add final 3D art before the related graybox contract is stable.

## Milestone 4 controls

- `1`, `2`, `3`: select Alpha, Bravo, or Team.
- `G`: move; `H`: hold; `J`: follow.
- `Y`: stack at door; `U`: open door; `K`: restrain subject.
- `Z`: cancel selected orders.
- Gamepad B: cycle selection; D-pad right: contextual order; View/select: cancel.
- `F` / gamepad left shoulder remains the player's police compliance command; officers also challenge visible suspects automatically.

## Next action

Install the Milestone 4 package, run its setup and validator, then execute the full checklist in `MILESTONE_4_INSTALL.md`. Record any first compiler or validator error exactly before changing code.
