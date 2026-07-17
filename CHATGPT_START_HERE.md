# ChatGPT Start Here

## Project

Rules of Entry is a Unity `6000.5.2f1` HDRP tactical law-enforcement simulation by Trooper Studios. GitHub repository `jaychase464-maker/RoE-v3`, branch `main`, is the source of truth.

## Current state

- Milestones 0–5 are the protected and pushed gameplay baseline.
- The user confirmed and pushed the cinematic UI and temporary suspect baseline at commit `c62b09d`.
- Milestone 6A headquarters and rugged operation planning are the current integration candidate.
- Do not call Milestone 6A complete until live Unity evidence passes.

Read in order:

1. `PROJECT_CONTEXT.md`
2. `CURRENT_STATUS.md`
3. `SYSTEM_MAP.md`
4. `DEVELOPMENT_ROADMAP.md`
5. `BUGS.md`
6. `UI_PRESENTATION_INSTALL.md`
7. `TEMPORARY_CHARACTER_INSTALL.md`
8. `MILESTONE_6A_INSTALL.md`

## Non-negotiable invariants

- Do not replace existing gameplay UI data sources merely to change appearance.
- Build order is Front End, Headquarters, then Prototype.
- Campaign mission selection happens physically inside the PD hub, not on the main menu.
- After selecting a mission, the rugged tablet owns briefing, officer assignment, future support planning, entry selection, and ready-up.
- K9, drone, and other specialized units cannot become selectable until their gameplay systems actually exist.
- Cross-scene deployment state stores stable IDs only.
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

Install the Milestone 6A replacement ZIP, run `Tools > Rules of Entry > Milestone 6A > Build Headquarters and Tablet Prototype`, execute all validation and tests, then smoke-test the complete front-end-to-headquarters-to-operation flow. Report the first Console error exactly if anything fails.
