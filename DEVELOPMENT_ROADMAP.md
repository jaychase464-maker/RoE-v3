# Development Roadmap

A milestone closes only after committed implementation, clean Unity compilation, validators, automated tests, smoke tests, earlier regressions, clean Console evidence, and a GitHub push.

## Prototype milestones

- Milestone 0 — Project Foundation — Complete
- Milestone 1 — First-Person and Tactical Interaction — Complete
- Milestone 2 — Weapon and Force-Event Foundation — Complete
- Milestone 3 — Suspect, Civilian, Compliance, and Arrest — Complete
- Milestone 4 — Officer AI, Commands, Door Traversal, and Bounded Initiative — Complete
- Milestone 5 — Mission, ROE, and After-Action Review — Complete and pushed

### Milestone 5.5 — Front-End and UI Presentation — Integration Candidate

Implemented: authored studio/warning/title/menu flow, project-supplied branding artwork, original cinematic city-overlook background, flat text navigation, settings persistence, mission-definition-driven asynchronous loading, unified gameplay HUD presentation, diagnostic toggle, reversible temporary suspect presentation, scene generation, validation, build gate, and tests.

Exit pending: Unity compilation; setup and validator; all tests; complete menu/controller/settings/load flow; Milestones 1–5 regressions; clean Console; commit and push.

### Milestone 6A — Headquarters and Operation Planning — Integration Candidate

Implemented: playable PD greybox; physical mission terminal; rugged tablet; mission briefing; officer assignments; explicit future-support records; three entry plans; ready-up rules; headquarters-to-operation loading; setup, validation, and tests.

Exit pending: Unity compilation, setup/validator, all tests, complete front-end-to-headquarters-to-operation smoke test, prior regressions, clean Console, commit, and push.

### Milestone 6B — Headquarters Functional Expansion and Mission Greybox

First integration candidate implemented: approved in-operation Tactical HUD with a scalable squad roster, qualitative health/ammunition reports, dynamic RoE body-camera metadata, contextual focus, and held-MMB numbered commands.

Remaining planned work:

1. Build the first multi-room mission greybox and traversal graph.
2. Add functioning PD loadout and shoot-house access points.
3. Establish the officer-management data boundary without inventing career systems.
4. Return completed operations to headquarters and preserve after-action context.

Chosen entry IDs were connected to authored operation anchors in Milestone 6C.

No final environment models are required until scale, routes, doors, cover, AI navigation, and entry anchors pass in greybox.

### Milestone 6C — Deployment and In-Mission Operational Tablet — Integration Candidate

Implemented: stable-ID operation entry anchors; headquarters officer assignment applied to the scene-owned squad; one-officer deployment support; on-demand officer body-camera sources; and a separate operational rugged tablet with Situation, Objectives, and Body Cameras pages.

Exit pending: Unity compilation; setup and validator; all tests; all three entry routes on baked NavMesh; one- and two-officer deployment; live-feed switching; tablet input/control restoration; AI continuity while viewing feeds; previous milestone regressions; clean Console; commit and push.

The first mission-map greybox remains next. Current entry anchors and body-camera mounts are temporary gameplay-authoring points, not final art.

### Milestone 6 — First Tactical Vertical Slice

Goal: one short, replayable residence or storefront incident with multiple entry choices, incomplete intelligence, an uncertain armed suspect, a civilian, officer initiative, arrest/force outcomes, and complete after-action review.

Remaining foundations:

1. Multi-room portal and clearance graph.
2. Objective/event persistence for restart and replay comparison.
3. Pause/options/restart/return-to-headquarters flow during an operation.
4. Functional loadout confirmation and deployment equipment.
5. First environment and character presentation pass after the gameplay contracts stabilize.

Character-art direction for Milestone 6:

- Replace the temporary high-density sample with an optimized modular Humanoid baseline.
- Require licensed source records, body/clothing compatibility, clean deformation, LODs, HDRP texture sets, and controlled material-slot counts.
- Build animation retargeting, locomotion, surrender, kneeling, handcuff, search, injury, and weapon-presentation coverage before scaling population counts.
- Evaluate the cataloged modular mega bundle only if it is later purchased and licensed; the current project must not depend on it.

## Later presentation work

- Accessibility: subtitles, contrast modes, scalable text, motion reduction, hold/toggle preferences.
- Full control remapping and device-specific glyphs.
- Localization-safe layouts and string tables.
- Menu soundscape, focus/click audio, transition stingers, and loading ambience.
- Final Trooper Studios logo, licensed typography, legal notices, and contributor credits.
- Real mission thumbnails or restrained scene backgrounds after environments exist.

## Expansion after the vertical slice

Negotiation; patrol perimeter and traffic; medical/EMS; K9; drones; sniper/spotter; bomb technicians; evidence and detectives; command post; career consequences; multiplayer after authority and determinism are replication-ready.
