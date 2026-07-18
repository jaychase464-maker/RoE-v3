# Bugs and Validation Gaps

## Open

### ROE-0056 — Milestone 7B live integration pending

- Priority: blocker for Milestone 7B closure
- Status: integration candidate
- Scope: automatic all-room mission completion, final evidence lock, seven-category score, S-through-F tier, casualty/policy caps, and final report presentation
- Required evidence: clean compilation; Milestone 7B validator; all EditMode tests; automatic completion after all six Pressure Point rooms clear; accurate civilian/suspect/officer/ROE/evidence/time results; unresolved manual debrief; Milestone 7A regressions; clean Console
- No code defect is currently claimed; this record prevents unverified implementation from being called complete.

### ROE-0057 — Legacy M3 validator required superseded NavMesh asset

- Status: code fixed; Unity rerun pending
- Test evidence: the 2026-07-18 EditMode report passed 108/113 tests; all five failures inherited the same `M3_PrototypeNavMesh.asset` dependency error while every Milestone 7B evaluator/completion test passed.
- Fix: validate the scene's actual loadable `NavMeshData` dependency so the legacy M3 bake and current M7A Pressure Point bake both satisfy the protected navigation contract.

### ROE-0054 — Incident seed is logged but not yet campaign-persisted

- Priority: medium replay/accountability debt
- Status: explicit Milestone 7A boundary
- Current behavior: each operation session derives and logs a seed used only for role-compatible placement.
- Missing: save-record ownership, restart-with-same-seed, after-action seed display, and replay comparison.
- Boundary: do not claim exact incident reproduction until the seed and relevant world state are persisted together.

### ROE-0055 — Pressure Point uses gameplay greybox art

- Priority: intentional presentation debt
- Status: blocked on layout validation
- Missing after layout lock: municipal/industrial shell, doors/windows, wet exterior yard, pumps/pipes/control equipment, fencing, office/maintenance props, decals, LODs, collision meshes, HDRP materials, lighting, audio, and optimization.
- Boundary: do not purchase or integrate final environment art until routes, dimensions, door swings, sight lines, cover, spawn fairness, and navigation pass.

### ROE-0045 — Tactical HUD requires live Unity validation

- Priority: blocker for Tactical HUD candidate closure
- Status: integration candidate
- Required evidence: clean compilation, successful Milestone 6B setup and validation, all EditMode tests, MMB/number-key smoke test, larger-squad roster test, resolution scaling check, and clean Console
- Regression focus: number-key selection when MMB is released, team orders reaching every configured officer, and Milestone 4 initiative/custody behavior

### ROE-0046 — NPC ammunition feed precedes full officer weapon simulation

- Priority: medium simulation debt
- Status: explicit prototype boundary
- Current behavior: each officer exposes a qualitative magazine-condition component used by the roster.
- Missing: authoritative NPC firearm consumption, reload choices, retained/dropped magazines, resupply, and AI low-ammunition decisions.
- Boundary: the HUD never invents exact round counts; the qualitative feed must be driven by the eventual officer firearm simulation.

### ROE-0039 — Headquarters is functional greybox, not final police-station art

- Priority: medium presentation debt
- Status: intentional
- Missing: final station shell, operations room, armory/loadout fixtures, lockers, office furnishings, shoot-house modules, signage, and physical rugged-tablet model.
- Boundary: gameplay layout and interaction ownership must pass before purchasing or integrating final environment models.

### ROE-0027 — Front-end and HUD presentation require live Unity validation

- Priority: blocker for Milestone 5.5 closure
- Status: integration candidate
- Environment: Unity `6000.5.2f1`, HDRP `17.5.0`, Input System `1.19.0`, AI Navigation `2.0.14`
- Required evidence: compilation, setup/validator, all tests, complete front-end/headquarters/tablet flow, settings persistence, prototype load, F10 diagnostic toggle, prior milestone regressions, and clean Console

### ROE-0028 — Authored splash does not replace required Unity legal branding

- Priority: informational
- Status: open by platform/license design
- Current behavior: `ROE_FrontEnd` begins with the Trooper Studios splash.
- Boundary: Unity may display its own required engine splash before the first scene depending on license, platform, and Player Settings.

### ROE-0029 — Presentation is not yet accessibility or localization complete

- Priority: medium production debt
- Missing: remappable UI navigation, device glyph switching, scalable type, high-contrast mode, motion reduction, screen-reader strategy, string tables, and translated-layout validation.

### ROE-0034 — Temporary suspect sample is not production-ready

- Priority: high before multi-character or population testing
- Status: accepted temporary art debt
- Current behavior: one supplied high-density Humanoid is used as the prototype suspect presentation with neutral generated HDRP materials and procedural state poses.
- Missing: production topology, LODs, texture maps, facial shapes, authored locomotion/custody/injury animations, IK contacts, clothing variation, and verified commercial source/license records.
- Boundary: do not duplicate this sample across large scenes or treat it as the final character pipeline.

## Existing realism debt

- ROE-0051: officer body-camera feeds use temporary chest transforms and binary signal state; RF range/occlusion, interference, dropped frames, latency, battery drain, physical devices, first-person tablet animation, and evidence retention remain future production systems.
- Individual officer paths lack production formations, sectors, cover selection, pieing, and coordinated entry timing.
- Custody lacks production animation, IK, handcuff props, search, and transport.
- Ballistics lack time of flight, penetration, fragmentation, ricochet, armor, and validated physiology.
- Suspects lack production weapons and attack execution.
- Final campaign/menu sound, production motion, accessibility, localization, and complete brand typography review remain outstanding.

## Resolved

- ROE-0053: the user confirmed clean Milestone 7A compilation, validator/tests, all entry and threshold traversal, AI/custody, room-clear behavior, tablet/body-camera continuity, debrief, prior regressions, and a clean Console, then pushed the checkpoint.

- ROE-0052: the three temporary single-room entry anchors were reauthored as south, west, and north exterior approaches on the Pressure Point multi-room greybox; live traversal validation remains tracked by ROE-0053.
- ROE-0038: headquarters entry plans now resolve through stable authored anchors, and Milestone 7A connects those anchors to validated operation topology and baked exterior formations.

- ROE-0050: the first scalable squad roster still presented as a large dashboard card; it now uses a narrow transparent element list with compact two-line officer status, condition pips, and qualitative ammunition.
- ROE-0049: the first functional body-camera overlay used generic rectangular uGUI chrome that did not reproduce the approved prototype; it now uses a rounded vector shell, shield-shaped RoE mark, divider, and authored recording/battery/camera icon layout.
- ROE-0048: MMB command numbers could fail to reach `OfficerSquadController` because command and selection Input Actions shared the same number controls; command slots now use explicit held-MMB current-Input-System detection and direct dispatch.
- ROE-0047: the Tactical HUD roster rendered beneath the old mission Canvas while large world-space prototype labels covered the screen; the approved HUD now owns sorting order 200 and normal play hides the superseded panels and labels.
- ROE-0044: the cinematic concept integration incorrectly included baked hands and a baked police-station background when only the rugged tablet design was requested; the current asset is a transparent hardware-only cutout over the player's real scene.
- ROE-0043: the generated rectangle-and-panel tablet did not visually match the approved reference despite functional planning controls; the supplied cinematic concept now provides the temporary physical device and hands while an opaque live screen preserves truthful gameplay data.
- ROE-0042: the planning tablet could only receive a briefing through the physical terminal, whose 0.35-second hold made an `E` tap appear unresponsive; the headquarters tablet now owns the authoritative briefing, toggles anywhere with `Tab`, and the terminal responds immediately to `E`.
- ROE-0041: the first tablet presentation could extend below the visible Game view, overlap its header fields, duplicate Close Tablet through the disabled Previous control, and render mission information too small; Revision 1 adds safe-area framing, separated headers, contextual control visibility, and a readable typography floor.
- ROE-0040: the headquarters reused the player prefab's intentionally unconfigured operation-only squad controller, causing a missing two-officer array and order-marker error on entering Play Mode; the headquarters scene now saves that component disabled while operational scenes retain their configured override.
- ROE-0037: applying procedural Humanoid muscles could allow imported body/root translation to drift away from the suspect actor; the fitted model root and reference Humanoid body pose are now locked after every pose write.
- ROE-0036: the temporary suspect model could be installed while its skinned renderers remained inactive or culled, leaving the actor invisible after fallback primitives were hidden; renderer ancestry, bounds, culling, and pre-hide validation are now enforced.
- ROE-0035: oversized Rules/Entry text could be clipped away by its generated uGUI rect; title and loading-destination labels now allow vertical overflow and use corrected bounds.
- ROE-0034A: loading presentation used generic destination language; it now reads the authoritative mission asset display name.
- ROE-0033: the first main-menu revision relied on boxed cards, tactical-grid decoration, and dashboard-style chrome that did not match the project's cinematic direction; Revision 3 replaces it with a clean city plate and flat authored navigation.
- ROE-0032: the initial front end used a generated text-only splash and lacked the required photosensitivity/legal acknowledgment state; both now use the supplied project artwork and the warning blocks until Enter/A.
- ROE-0031: `Navigation` in the UI setup tool resolved to the project namespace instead of Unity's uGUI navigation struct.
- ROE-0030: main-menu and settings controls used positive top-anchor coordinates that would have placed them off-screen; corrected before delivery.
- ROE-0026: officer challenge sequences could stop after one command despite retained visual contact.
- ROE-0025: Milestone 5 manual-finalization test used an obsolete report property name.
- ROE-0013: PackageInfo ambiguity.
- ROE-0014: score-boundary validator false positive.
- ROE-0018: generated input-action persistence.
- ROE-0019: saved squad prefab-instance references.
- ROE-0020: closed-door navigation link.
- ROE-0021: bounded officer initiative passed the live user test.
