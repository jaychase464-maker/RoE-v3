# Current Status

## Active milestone

**Milestone 6B — Scalable Tactical HUD and MMB Squad Commands: integration candidate; live Unity validation pending**

Milestones 0–5 are the protected working gameplay baseline. The user confirmed the Milestone 5 gameplay sequence, cinematic UI, and temporary suspect presentation work and pushed commit `c62b09d` before Milestone 6A began.

## Tactical HUD candidate

- Approved upper-left squad roster is now a narrow, transparent, low-profile element list that scales from the current two officers to larger configured squads.
- Each compact two-line row reads live officer identity, selection, current order/activity, physiological condition pip/label, and qualitative ammunition state without health bars or large cards.
- Injury labels are `FIT`, `WOUNDED`, `DOWN`, or `DECEASED`; no arcade hit-point value is exposed.
- Ammunition labels are `GOOD`, `LOW`, or `CRITICAL`; precise hidden round counts remain unavailable to the player.
- Upper-right `RoE Body Cam` block reads configurable campaign officer name, badge ID, Calder City Police Department identity, recording/battery state, and the in-game mission clock.
- Holding middle mouse reveals the approved six-command panel; keys `1–6` issue Move, Hold, Stack, Open/Clear, Follow, and Restrain only while the menu is held.
- Releasing middle mouse without a selection closes the panel without issuing an order.
- Command focus is read from the authoritative squad raycast and suggests Move for positions, Stack for doors, and Restrain for custody subjects.
- The squad controller no longer rejects arrays larger than two; team orders include every valid configured officer.
- The old Milestone 4 command diagnostic panel is disabled in the operation scene but its prefab and data remain available for development.
- Setup tool, validator, and EditMode rule coverage are included.

## Implemented in this candidate

- Campaign flow is now `front end → headquarters → physical mission selection → rugged tablet → ready up → operation`.
- New generated headquarters scene: `Assets/_Project/RulesOfEntry/Scenes/Headquarters/ROE_Headquarters.unity`.
- Playable PD greybox with labeled Operations, Loadout, Officer Management, Shoot House, and Support Staging zones.
- Existing `ROE_Player` and interaction prompt reused without a second controller stack.
- The reusable player's operation-only squad-command layer is disabled in headquarters and remains enabled/configured in operation scenes.
- Physical mission terminal implements the existing `InteractableBehaviour` contract.
- Rugged planning tablet follows the approved handheld concept: wide hardware bezel, side function keys, seven top tabs, dual mission-data panels, and bottom deployment controls.
- Tablet Presentation Revision 1 keeps the entire device inside a resolution-independent safe area, prevents header overlap, removes redundant standard-page actions, increases readable type, and adds explicit active-tab and layered hardware treatment.
- `Tab` opens or closes the authoritative operation tablet anywhere in headquarters; the physical mission terminal remains available through an immediate `E` press.
- A transparent hardware-only tablet cutout supplies the rugged shell, rubber guards, sensors, side controls, vents, fasteners, and deep bezel. It contains no hands, environment, or full-screen dimmer; the live camera view remains fully visible everywhere outside the tablet.
- Tablet sections are Overview, Objectives, Intel, Map/Entry, Team, Loadout/Support, and ROE/Ready.
- Two existing prototype officers can be assigned or removed from the response team.
- Three stable entry plans can be reviewed and selected.
- K9, drone, tactical medic, and negotiator definitions are visible but explicitly unavailable until implemented.
- Ready-up validation requires a valid mission, entry, and at least one available officer.
- Deployment stores only stable mission, entry, officer, and support IDs before loading the operation scene.
- Operations enters headquarters; Training remains a direct prototype shortcut.
- Build order is Front End, Headquarters, then Prototype.
- Setup tool, validator, pure-rule EditMode tests, and configuration test added.

- Dedicated front-end scene with Trooper Studios splash, blocking photosensitivity/legal warning, title screen, main menu, settings, credits, and loading panels.
- Supplied 1672×941 splash and warning artwork imported losslessly as full-screen sprites.
- Original 1672×941 moonlit Calder City overlook behind the title and menus, with no baked-in UI text.
- Licensed Latin Modern Sans Demi Condensed typography with the GUST Font License included.
- Keyboard, mouse, and gamepad-compatible uGUI navigation through Input System `1.19.0` defaults.
- Flat, left-aligned main-menu navigation with disabled future campaign entries and functional Operations, Training, Settings, Credits, and Quit entries.
- Front-end Operations loading identifies `Calder City Police Department`; Training identifies the tactical training prototype.
- Persistent master-volume, fullscreen, and quality selections.
- Trooper Studios company identity while preserving the validated `RoE v3` product name.
- Clean charcoal, steel, and cold police-blue visual language with no menu cards, dashboard panels, decorative grids, or generic sci-fi framing.
- Restyled interaction prompt, manual-weapon status, officer-command status, mission/AAR status, and AI diagnostics.
- F10 diagnostic visibility toggle.
- Scene generator, project validator, build gate, and EditMode test coverage.
- Supplied `SKM_Character.fbx` configured as a Humanoid and attached only to the prototype suspect's presentation hierarchy.
- Neutral HDRP material generation and procedural idle, alert, surrender, kneeling, and incapacitated pose response.
- Reversible Apply, Restore, and Validate editor tools for the temporary suspect presentation.
- Existing suspect AI, NavMeshAgent, perception, condition, custody, evidence, and hit regions remain authoritative.

## Validation pending

- Clean compilation after the Tactical HUD package is installed.
- Run `Rules of Entry > Milestone 6B > Build Tactical HUD` and pass the Tactical HUD validator.
- Confirm the roster, body-camera data, target focus, MMB visibility, and numbered commands at 1920×1080 and ultrawide-safe resolutions.
- Confirm number keys still select the current prototype officers/team when MMB is not held.
- Confirm prior direct team orders, autonomous challenges, room clearance, and custody behavior remain stable.
- Clean Unity compilation after installation.
- Milestone 6A setup, UI Presentation validation, and Milestone 6A validation pass.
- All EditMode and PlayMode tests.
- Splash-to-warning-to-title timing, Enter/A warning acknowledgment, and menu transitions.
- Keyboard, mouse, and controller navigation, including disabled-item skipping and the flat-menu focus response.
- Settings persistence after Play Mode restart.
- Operations loads headquarters; the player can walk to the terminal and open/close the tablet.
- Personnel assignment, support availability, entry cycling, ready-up gating, and deployment loading.
- Training still loads the prototype directly.
- F10 diagnostics toggle and Milestones 1–5 regression checks.
- Clean Console during the full flow.
- Humanoid import, temporary-character validator, pose transitions, custody interaction, hit response, and performance warning.

## Asset boundary

No new 3D model is required for the Tablet Presentation Revision 1 installation. The headquarters and mission terminal use generated greybox geometry, and the planning device is still a screen-space rugged-tablet interface. Matching the approved physical reference will require a separate-part rugged tablet/MDT model, emissive screen mesh, PBR materials, compatible first-person gloved arms, and tablet raise/idle/interact/lower animations. Final headquarters art will additionally need a modular police-station interior kit, operations-room furniture, armory/loadout fixtures, lockers, office props, and shoot-house modules. No new package, audio file, layer, tag, Input Action, or Inspector assignment is required for this revision.
