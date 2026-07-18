# ChatGPT Start Here

## Current integration task

Milestone 7A Pressure Point is the newest candidate. The first operation now has nine connected areas, six clearance spaces, three reauthored deployment approaches, seven door links, two open passages, and seed-driven placement of the existing suspect/civilian. Read `MILESTONE_7A_INSTALL.md`, `MILESTONE_6C_INSTALL.md`, `TACTICAL_HUD_INSTALL.md`, `CURRENT_STATUS.md`, and `SYSTEM_MAP.md` before changing mission topology, navigation, deployment, scenario placement, cameras, squad behavior, or evidence.

## Project

Rules of Entry is a Unity `6000.5.2f1` HDRP tactical law-enforcement simulation by Trooper Studios. GitHub repository `jaychase464-maker/RoE-v3`, branch `main`, is the source of truth.

## Current state

- Milestones 0–5 are the protected and pushed gameplay baseline.
- The user confirmed and pushed the Milestone 6C deployment and operational body-camera tablet checkpoint before Milestone 7A began.
- Milestone 7A is the current integration candidate built on the user-confirmed Milestone 6C operation-tablet behavior.
- Do not call Milestone 7A complete until live Unity evidence passes.

Read in order:

1. `PROJECT_CONTEXT.md`
2. `CURRENT_STATUS.md`
3. `SYSTEM_MAP.md`
4. `DEVELOPMENT_ROADMAP.md`
5. `BUGS.md`
6. `UI_PRESENTATION_INSTALL.md`
7. `TEMPORARY_CHARACTER_INSTALL.md`
8. `MILESTONE_6A_INSTALL.md`
9. `MILESTONE_6C_INSTALL.md`
10. `MILESTONE_7A_INSTALL.md`

## Non-negotiable invariants

- Do not replace existing gameplay UI data sources merely to change appearance.
- Build order is Front End, Headquarters, then Prototype.
- Campaign mission selection happens physically inside the PD hub, not on the main menu.
- After selecting a mission, the rugged tablet owns briefing, officer assignment, future support planning, entry selection, and ready-up.
- K9, drone, and other specialized units cannot become selectable until their gameplay systems actually exist.
- Cross-scene deployment state stores stable IDs only.
- Every operation area and portal uses a stable unique ID, and all authored areas must be reachable from a planning entry.
- Physical-door navigation links activate only after actual door clearance; open passages do not invent door state.
- Scenario placement may move existing actors to compatible authored points but cannot create actors, change roles, decide behavior, or mutate objectives/evidence.
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

Install the Milestone 7A replacement ZIP after the Milestone 6C setup passes. Run `Tools > Rules of Entry > Milestone 7A > Build Pressure Point Mission Greybox`, run its validator and all EditMode tests, then smoke-test all three entries and every threshold. Report the first Console error exactly if anything fails.
