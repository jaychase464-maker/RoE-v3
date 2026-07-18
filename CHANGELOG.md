# Changelog

## Milestone 6B Tactical HUD Visual Hotfix 3 — 2026-07-17

- Replaced the large upper-left squad card with a narrow, low-profile tactical element readout inspired by field-command interfaces.
- Reduced each officer to a compact two-line entry containing roster index, identity, colored condition pip, qualitative health, current order, and qualitative ammunition state.
- Removed column headers, officer pictograms, heavy row fills, and the roster outline while adding restrained text shadows for legibility over the live scene.
- Reduced the roster width and per-officer height so larger campaign elements can expand vertically without dominating the player's view.
- Extended Tactical HUD validation to require the compact roster dimensions and complete condition-pip binding.

## Milestone 6B Tactical HUD Visual Hotfix 2 — 2026-07-17

- Rebuilt the body-camera overlay around a dedicated rounded translucent vector shell rather than a generic rectangular Image.
- Added the approved internal divider, shield-shaped RoE mark, circular recording indicator, battery silhouette, and live-camera icon treatment.
- Re-aligned the officer, department, timestamp, battery, and LIVE fields to the hierarchy and proportions of the approved prototype.
- Added compact officer glyphs and retained the approved roster and command-panel composition.
- Extended Tactical HUD validation to require the authored body-camera shell and RoE shield components.

## Milestone 6B Tactical HUD Hotfix 1 — 2026-07-17

- Raised the Tactical HUD to a dedicated sorting order so the squad roster cannot render behind the prototype mission Canvas.
- Removed the old mission/officer HUD panels from normal operation presentation.
- Disabled the enormous world-space officer, suspect, and civilian diagnostic labels responsible for the mirrored text across the screen.
- Changed the idle weapon panel to remain hidden while preserving temporary check-magazine, reload, and operation feedback.
- Replaced overlapping number-action command detection with an explicit current-Input-System MMB plus number-slot path.
- Added public command-slot dispatch and strengthened validation for Canvas priority, hidden diagnostics, and command execution entry.
- Added compact officer glyphs and corrected roster text spacing to more closely match the approved mockup.

## Milestone 6B Tactical HUD — Integration Candidate — 2026-07-17

- Added the approved upper-left scalable squad roster and upper-right RoE body-camera overlay.
- Bound officer rows to live identity, selection, activity/order, injury condition, and qualitative ammunition data.
- Added configurable campaign-facing officer name, badge, department, recording state, and battery fields.
- Added a pause-aware in-game mission clock for the body-camera timestamp.
- Added a held middle-mouse command interface with numbered Move, Hold, Stack, Open/Clear, Follow, and Restrain actions.
- Added context focus and automatic command suggestion for world positions, doors, and custody subjects.
- Generalized team selection and order dispatch beyond the original exactly-two-officer array while preserving current two-officer controls.
- Disabled the superseded operation command debug panel after Tactical HUD setup.
- Added a deterministic editor builder, validator, and EditMode rule tests.
- Preserved the realism boundary: no player ammo counter, exact officer round count, player health bar, minimap, or automatic reload.

## Milestone 6A Hardware-Only Tablet Correction — 2026-07-17

- Replaced the full cinematic concept plate with a purpose-built transparent rugged-tablet hardware cutout.
- Removed all baked hands, arms, police-station background, furniture, signage, and environmental lighting from the tablet asset.
- Reconstructed the tablet side guards and shell areas that had been hidden behind the concept hands.
- Preserved the approved black polymer, rubber armor, fasteners, vents, sensors, side controls, blue indicator, and lower SRS hardware treatment.
- Removed the full-screen tablet backdrop entirely so the player's actual headquarters view remains fully visible outside the hardware silhouette.
- Remapped the authoritative live planning display to the isolated hardware's physical screen coordinates.

## Milestone 6A Cinematic Tablet Concept Integration — 2026-07-17

- Integrated the supplied 1672×941 rugged-tablet concept as the temporary full-screen presentation plate.
- Preserved the concept's gloved hands, physical shell, rubber guards, hardware controls, deep bezel, and cinematic headquarters framing.
- Covered the concept's example screen with an opaque live display so current mission, objectives, intelligence, entry, team, support, ROE, and deployment data remain authoritative and interactive.
- Anchored the live display to the concept's physical screen coordinates and fitted the complete plate through a 1672:941 aspect-ratio container.
- Removed the procedural fake-hardware frame because it no longer contributes to the presentation.
- Added lossless Sprite import configuration and validation for the concept artwork dependency.

## Milestone 6A Tablet Input Hotfix — 2026-07-17

- Added `Tab` as the headquarters-wide planning-tablet toggle using the current Input System keyboard device.
- Pressing `Tab` opens the authoritative headquarters briefing from anywhere in the PD; pressing it again closes the tablet and restores first-person control.
- Retained the physical mission terminal as a secondary entry point and changed it from a 0.35-second hold to an immediate `E` press.
- Added the default operation briefing directly to the generated tablet configuration so keyboard opening does not depend on terminal focus.
- Extended headquarters validation to require the authoritative default briefing and immediate terminal interaction.

## Milestone 6A Tablet Presentation Revision 1 — 2026-07-17

- Reframed the screen-space tablet inside resolution-independent safe-area anchors so the device and footer cannot extend beyond the display.
- Separated operation identity and incident metadata to prevent header text overlap.
- Raised mission body, panel-heading, navigation, and status typography to a readable prototype floor.
- Replaced translucent blue data slabs with near-black secure-display panels, restrained signal lines, and an explicit active-tab state.
- Removed redundant standard-page navigation controls while preserving persistent Close, Back, contextual selection, and primary actions.
- Added corner armor, a lower equipment rail, stronger bezel layering, and a device shadow to improve the temporary rugged-hardware silhouette.
- Extended Milestone 6A validation to enforce safe framing, header separation, and body-type readability.

## Milestone 6A Hotfix 2 — Headquarters Player Mode — 2026-07-17

- Disabled the operation-only `OfficerSquadController` on the headquarters player instance.
- Preserved the enabled, fully referenced Milestone 4 command layer in operational scenes.
- Added headquarters validation that rejects an enabled or missing hub squad-command component.

## Milestone 6A — Headquarters and Rugged Operation Planning — 2026-07-17

- Redirected the Operations route from direct deployment into a playable Calder City Police headquarters.
- Added a generated PD greybox with Operations, Loadout, Officer Management, Shoot House, and Support Staging zones.
- Added a physical mission-assignment terminal using the established first-person interaction contract.
- Added `OperationBriefingDefinition` with authoritative scene, intelligence, entry, personnel, and specialized-support data.
- Added an approved-concept rugged handheld tablet with side hardware keys, seven top sections, two-column mission information, briefing status, and persistent deployment controls.
- Added selectable prototype officers and three selectable entry plans.
- Added K9, drone, tactical-medic, and negotiator planning definitions while keeping those unimplemented systems unavailable.
- Added pure planning rules and identifier-only cross-scene deployment context.
- Preserved the existing prototype mission evidence targets while re-presenting it as `Operation: Pressure Point`.
- Updated loading behavior so Operations identifies headquarters and Training remains a direct prototype shortcut.
- Updated enabled build order to Front End, Headquarters, then Prototype.
- Added deterministic setup, validation, and EditMode tests.
- Recorded entry-anchor integration as Milestone 6B work instead of teleporting to guessed positions.


## Milestone 5.5 Temporary Character Hotfix 2 — 2026-07-17

- Fixed the temporary suspect model drifting or floating away from its actor root after procedural Humanoid poses were applied.
- Captured the fitted model-local transform and Humanoid body pose during initialization.
- Restored the reference body position, body rotation, local position, local rotation, and local scale after every procedural pose update.
- Preserved authoritative NavMeshAgent movement and parent-level kneeling/incapacitated presentation while blocking imported-skeleton root drift.
- Validation now requires root motion to remain disabled.

## Milestone 5.5 Temporary Character Hotfix 1 — 2026-07-17

- Fixed a case where the sample suspect hierarchy existed but its skinned presentation was invisible.
- The setup now activates renderer ancestry, explicitly enables renderers, clears forced-rendering suppression, expands skinned bounds, and keeps the temporary Humanoid animating while offscreen.
- Human-scale bounds and valid materials are verified before the primitive fallback is hidden.
- The runtime pose bridge reapplies its visibility contract when the suspect becomes active.
- Temporary-character validation now rejects disabled, forced-off, inactive, zero-scale, invalid-bounds, or culling-prone presentations.

## Milestone 5.5 Visual Revision 4 — 2026-07-17

- Rebuilt the loading presentation as a cinematic lower-third composition over the approved Calder City background.
- Bound the loading destination to the authoritative Milestone 5 `MissionDefinition`; the current prototype displays `Training Operation: Controlled Resolution` instead of generic or baked copy.
- Added live destination context, scene detail, phase text, normalized percentage, and a restrained full-width progress line.
- Corrected oversized title text clipping by allowing vertical overflow on the title and loading destination labels.
- Added the supplied `SKM_Character.fbx` as a reversible temporary suspect presentation.
- Added Humanoid import configuration, neutral HDRP materials, automatic fit/grounding, and procedural actor-state poses.
- Preserved the original suspect AI, navigation, custody, injury, hit-region, and evidence components.
- Added Apply, Restore, and Validate editor commands plus pure pose-rule EditMode tests.
- Added a performance boundary warning because the temporary sample is high-density and contains no LOD group.
- Recorded the future modular mega bundle only as an optional future character-art candidate; it is not a package dependency.

## Milestone 5.5 Visual Revision 3 — 2026-07-17

- Rebuilt the main menu around the project owner's cinematic city-overlook direction.
- Replaced the tactical staging plate with a clean moonlit Calder City scene containing no baked title, menu labels, footer text, or social icons.
- Replaced boxed menu cards and dashboard chrome with oversized stacked title typography and a flat left-aligned navigation list.
- Removed the decorative tactical grid and horizon overlays from the front-end background.
- Added restrained text movement, a narrow police-blue focus bar, and subtle divider response for mouse, keyboard, and controller selection.
- Added disabled Continue Campaign and New Campaign placeholders without presenting unfinished features as playable.
- Added functional Operations and Training entries; both load the current playable prototype until distinct modes exist.
- Added `FrontEndMenuItemVisual` as the dedicated flat-navigation presentation component.

## Milestone 5.5 Visual Revision 2 — 2026-07-17

- Replaced the temporary generated studio mark with the supplied full-screen Trooper Studios artwork.
- Added the supplied photosensitivity/legal artwork as a dedicated blocking state after the splash.
- Warning acknowledgment accepts Enter, numpad Enter, or controller South/A and never auto-advances.
- Added an original photorealistic tactical staging background with dark menu-safe space on the left.
- Reworked the title and main-menu composition around the cinematic background.
- Replaced the brass accent with a cold police-blue visual system matching the supplied artwork.
- Added Latin Modern Sans Demi Condensed under the included GUST Font License for consistent tactical typography.
- Added sprite import configuration and validator checks for all saved artwork dependencies.

## Milestone 5.5 Hotfix 1 — 2026-07-17

- Qualified `UnityEngine.UI.Navigation` in the editor setup tool so it cannot collide with the existing `RulesOfEntry.Navigation` namespace.

## Milestone 5.5 — Front-End and UI Presentation — Integration Candidate — 2026-07-16

### Added

- Dedicated `ROE_FrontEnd.unity` scene generated from a controlled editor tool.
- Trooper Studios authored splash and Rules of Entry title screen.
- Clean main menu with Begin Operation, Settings, Credits, and Exit to Desktop.
- Persistent master-volume, fullscreen, and quality controls.
- Async operation loading screen with normalized progress feedback.
- Keyboard, mouse, and gamepad menu navigation using `InputSystemUIInputModule` defaults.
- Responsive button selection, hover, press, and focus presentation.
- Unified charcoal, brass, and signal-blue treatment for existing gameplay HUD systems.
- F10 toggle for developer AI diagnostics.
- UI Presentation validator, pre-build gate, and EditMode tests.
- `Trooper Studios` company identity and 1920×1080 borderless-fullscreen defaults.

### Preserved

- `RoE v3` product identity required by foundation validation.
- Prototype scene gameplay, mission, AI, custody, officer, interaction, weapon, and scoring logic.
- All existing gameplay UI components and their data references.
- New Input System-only project policy.

### Boundaries

- No new package, Input Action, 3D model, texture, font, audio file, tag, layer, or manual Inspector assignment.
- Unity's legal engine splash may still precede the authored studio splash.
- Live Unity validation is pending.

## Milestone 5

- Mission objectives, threat-based ROE review, immutable evidence aggregation, pure evaluation, after-action scoring, mission HUD, and debrief finalization passed live tests and were pushed before Milestone 5.5 began.

## Milestone 4

- Officer commands, gated doorway traversal, persistent suspect challenges, timed room clearance, cover-gated automatic custody, and factual initiative history passed the live prototype tests.
