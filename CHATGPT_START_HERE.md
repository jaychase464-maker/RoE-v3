# ChatGPT Start Here

## Project

Rules of Entry is a Unity `6000.5.2f1` HDRP tactical law-enforcement simulation. GitHub repository `jaychase464-maker/RoE-v3`, branch `main`, is the source of truth.

## Current state

- Milestones 0–4 are the protected working baseline.
- Milestone 5 mission, ROE, and after-action review is an integration candidate.
- Do not call Milestone 5 complete until live Unity evidence passes.

Read in order:

1. `PROJECT_CONTEXT.md`
2. `CURRENT_STATUS.md`
3. `SYSTEM_MAP.md`
4. `DEVELOPMENT_ROADMAP.md`
5. `BUGS.md`
6. `MILESTONE_5_INSTALL.md`

## Non-negotiable invariants

- No automatic reload or exact player ammunition counter.
- No navigation warp, instant arrest, or direct custody forcing.
- Every AI, custody, force, officer-order, and initiative decision remains factual and attributable.
- Mission code consumes facts but cannot mutate the systems producing them.
- Ambiguous evidence requires review instead of invented certainty.
- ROE findings retain event-level rationale.
- Critical misconduct and failed required objectives cannot receive a high final rating.
- Ending an operation manually turns unresolved required objectives into failures.
- Unity identity uses `GetEntityId()` with all 64 bits preserved.
- Do not claim live success without compilation, validation, tests, regressions, and Console evidence.

## Next action

Install `RulesOfEntry-Milestone-5.zip`, run the Milestone 5 setup/validator, execute the full checklist, and report the first error exactly if anything fails.
