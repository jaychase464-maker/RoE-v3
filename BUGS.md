# Bugs and Validation Gaps

## Open

### ROE-0027 — Front-end and HUD presentation require live Unity validation

- Priority: blocker for Milestone 5.5 closure
- Status: integration candidate
- Environment: Unity `6000.5.2f1`, HDRP `17.5.0`, Input System `1.19.0`, AI Navigation `2.0.14`
- Required evidence: compilation, setup/validator, all tests, complete front-end flow, settings persistence, prototype load, F10 diagnostic toggle, prior milestone regressions, and clean Console

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

- Individual officer paths lack production formations, sectors, cover selection, pieing, and coordinated entry timing.
- Custody lacks production animation, IK, handcuff props, search, and transport.
- Ballistics lack time of flight, penetration, fragmentation, ricochet, armor, and validated physiology.
- Suspects lack production weapons and attack execution.
- Final campaign/menu sound, production motion, accessibility, localization, and complete brand typography review remain outstanding.

## Resolved

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
