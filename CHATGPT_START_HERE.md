# ChatGPT Start Here

## Current integration task

Milestone 7C operation closure and headquarters return is the newest candidate on the user-confirmed and pushed Milestone 7B baseline. Read `MILESTONE_7C_INSTALL.md`, `MILESTONE_7B_INSTALL.md`, `CURRENT_STATUS.md`, and `SYSTEM_MAP.md` before changing final report presentation, scene return, deployment cleanup, or completed-operation context.

## Project

Rules of Entry is a Unity `6000.5.2f1` HDRP tactical law-enforcement simulation by Trooper Studios. GitHub repository `jaychase464-maker/RoE-v3`, branch `main`, is the source of truth.

## Current state

- Milestones 0–5 are the protected and pushed gameplay baseline.
- The user confirmed and pushed the Milestone 6C deployment and operational body-camera tablet checkpoint before Milestone 7A began.
- The user confirmed and pushed Milestone 7A after clean compilation, validation, tests, traversal, AI, tablet, and debrief checks.
- The user confirmed and pushed Milestone 7B after compilation, validators, automated tests, automatic completion, and live factual report checks.
- Milestone 7C is the current integration candidate.
- Do not call Milestone 7C complete until live Unity operation-to-headquarters evidence passes.

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
11. `MILESTONE_7B_INSTALL.md`
12. `MILESTONE_7C_INSTALL.md`

## Non-negotiable invariants

- Do not replace existing gameplay UI data sources merely to change appearance.
- Build order is Front End, Headquarters, then Prototype.
- Campaign mission selection happens physically inside the PD hub, not on the main menu.
- After selecting a mission, the rugged tablet owns briefing, officer assignment, future support planning, entry selection, and ready-up.
- K9, drone, and other specialized units cannot become selectable until their gameplay systems actually exist.
- Cross-scene deployment state stores stable IDs only.
- Completed-operation session state stores the immutable final report and stable IDs only; it owns no Unity scene references.
- Deployment context is cleared only after successful completed-operation capture.
- Every operation area and portal uses a stable unique ID, and all authored areas must be reachable from a planning entry.
- Physical-door navigation links activate only after actual door clearance; open passages do not invent door state.
- Scenario placement may move existing actors to compatible authored points but cannot create actors, change roles, decide behavior, or mutate objectives/evidence.
- Startup order is splash, blocking photosensitivity/legal warning, title, then menu.
- Loading text must identify its actual destination from authoritative project data.
- New Input System only; no legacy input or `StandaloneInputModule`.
- No automatic reload or exact player ammunition counter.
- No navigation warp, instant arrest, or direct custody forcing.
- Mission evaluation consumes facts but cannot mutate their producers.
- Automatic completion requires every required objective to be terminal and every authored tactical room to be clear.
- Final grades must show category deductions and caps; casualty or policy facts cannot be hidden or rewritten by UI.
- Ambiguous evidence requires review instead of invented certainty.
- Unity identity uses `GetEntityId()` with all 64 bits preserved.
- Temporary or final character visuals may not replace actor-root gameplay and hit-region components.
- Do not claim live success without compilation, validation, tests, regressions, and clean Console evidence.

## Next action

Install the Milestone 7C replacement ZIP on the confirmed Milestone 7B checkpoint. Run `Tools > Rules of Entry > Milestone 7C > Build Operation Closure and Headquarters Return`, run its validator and all EditMode tests, then complete Pressure Point and verify Continue returns to headquarters with the same report. Report the first Console error exactly if anything fails.
