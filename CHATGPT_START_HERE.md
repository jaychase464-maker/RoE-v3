# ChatGPT Start Here

## Project

Rules of Entry is a Unity `6000.5.2f1` HDRP tactical law-enforcement simulation. GitHub repository `jaychase464-maker/RoE-v3`, branch `main`, is the source of truth.

## Current state

- Milestones 0 and 1 are validated foundations.
- Milestones 0–3 are complete and validated.
- Milestone 3 human behavior and custody is the protected baseline for officer AI.

Read in order:

1. `PROJECT_CONTEXT.md`
2. `CURRENT_STATUS.md`
3. `SYSTEM_MAP.md`
4. `DEVELOPMENT_ROADMAP.md`
5. `BUGS.md`
6. `MILESTONE_3_INSTALL.md`

## Non-negotiable invariants

- No automatic reload and no exact player ammo counter.
- Unity object identifiers use `GetEntityId()` and preserve all 64 bits.
- AI decisions must be reproducible and record an explicit reason.
- No capable free subject may be instantly restrained or searched.
- Combat, AI, and custody code emit facts and never mutate mission score.
- Do not claim live Unity success until compilation, validation, tests, smoke tests, and Console evidence exist.
- Do not add final 3D art before the related graybox contract is stable.

## Current controls

- `F` / gamepad left shoulder: issue police compliance command.
- `E` / gamepad west: perform the displayed custody interaction.
- Weapon controls remain documented in the Milestone 2 files.

## Next action

Commit and push the completed Milestone 3 documentation. Then begin Milestone 4 officer selection, tactical orders, failure reasons, navigation/door coordination, and arrest assistance without weakening existing authority boundaries.
