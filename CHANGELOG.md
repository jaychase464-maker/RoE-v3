# Changelog

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
