# ChatGPT Start Here

## Project

Rules of Entry is a Unity `6000.5.2f1` HDRP tactical law-enforcement simulation by Trooper Studios. GitHub repository `jaychase464-maker/RoE-v3`, branch `main`, is the source of truth.

## Current state

- Milestones 0–5 are the protected and pushed gameplay baseline.
- Milestone 5.5 Visual Revision 4 cinematic front-end, destination-aware loading, and temporary suspect presentation are an integration candidate.
- Do not call Milestone 5.5 complete until live Unity evidence passes.

Read in order:

1. `PROJECT_CONTEXT.md`
2. `CURRENT_STATUS.md`
3. `SYSTEM_MAP.md`
4. `DEVELOPMENT_ROADMAP.md`
5. `BUGS.md`
6. `UI_PRESENTATION_INSTALL.md`
7. `TEMPORARY_CHARACTER_INSTALL.md`

## Non-negotiable invariants

- Do not replace existing gameplay UI data sources merely to change appearance.
- The authored front end is build scene 0; the playable prototype is build scene 1.
- Startup order is splash, blocking photosensitivity/legal warning, title, then menu.
- Loading text must identify its actual destination from authoritative project data.
- New Input System only; no legacy input or `StandaloneInputModule`.
- No automatic reload or exact player ammunition counter.
- No navigation warp, instant arrest, or direct custody forcing.
- Mission evaluation consumes facts but cannot mutate their producers.
- Ambiguous evidence requires review instead of invented certainty.
- Unity identity uses `GetEntityId()` with all 64 bits preserved.
- Temporary or final character visuals may not replace actor-root gameplay and hit-region components.
- Do not claim live success without compilation, validation, tests, regressions, and clean Console evidence.

## Next action

Install `RulesOfEntry-Frontend-UI-6.zip`, run the UI Presentation setup tool, reapply the sample suspect through the Temporary Characters menu, execute both validation checklists, and report the first error exactly if anything fails.
