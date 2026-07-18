# Current Status

## Active milestone

**Milestone 7B — Automatic Mission Completion and After-Action Tiers: integration candidate; live Unity validation pending**

Milestones 0–7A are the protected working baseline. The user confirmed clean compilation, the Milestone 7A validator, all EditMode tests, and the live Pressure Point traversal/AI/tablet/debrief checks, then pushed the checkpoint before Milestone 7B began.

## Milestone 7B candidate

- Missions automatically lock their final evidence after every required objective reaches a terminal state and every authored tactical room is verified clear.
- A three-second confirmation window prevents a one-frame all-clear from prematurely ending the operation.
- Final reports score Objectives (30), Civilian Safety (20), Suspect Custody (15), Officer Safety (10), Rules of Engagement (10), Evidence (10), and Time (5).
- Final tiers are S, A, B, C, D, or F. A required-objective failure or officer death caps the tier at D; a civilian death or critical ROE violation caps it at F.
- Evidence scoring reads actual suspect searches, secured weapons, and reportable items from the immutable evidence snapshot.
- Mission time uses authored target and maximum-scored durations instead of a universal hidden timer.
- A dedicated final report presentation displays the tier, total score, score cap, category breakdown, outcome metrics, objective results, and policy findings.
- Manual debrief remains available for deliberately ending an unresolved operation; pending objectives become failed in the final report.
- Setup, validator, and EditMode scoring/completion coverage are included.

## Milestone 7A confirmed baseline

- Operation Pressure Point is now authored as a municipal pumping-annex greybox rather than the former single training room.
- Nine topology nodes connect three exterior staging approaches, three south-side work areas, a central corridor, the pump hall, and the control room.
- Six interior `TacticalRoomVolume` components provide independent clearance evidence.
- Seven physical doors use fixed, bidirectional `NavMeshLink` objects gated by actual door clearance; two wide thresholds remain open passages.
- Headquarters south, west, and north entry IDs are reauthored to exterior NavMesh formations sized for the current team and future squads up to eight officers.
- The existing suspect and civilian retain stable actor/evidence IDs while a logged incident seed selects among twelve weighted, role-compatible locations.
- The required room-clear objective now targets `m7a_pump_hall` rather than the deleted training-room placeholder.
- The original training room and firing range remain preserved but inactive in the operation scene.
- Mission topology, actor placement, AI behavior, custody, and after-action evaluation remain separate authorities.
- Setup, validator, persisted NavMesh generation, and EditMode rules coverage are included.

## Milestone 6C candidate

- Headquarters entry IDs now resolve to three authored operation-scene anchors.
- Confirmed officer IDs filter the scene-owned squad; a valid deployment may contain one or more officers.
- Each operation officer prefab owns a disabled-by-default chest body-camera source.
- `Tab` opens a separate in-mission rugged tablet without clearing deployment context.
- Situation and Objectives pages read the existing mission controller without mutating evidence.
- Body Cameras opens by default and switches among active deployed officers with `Q` / `E`, arrows, buttons, or gamepad shoulders.
- Only the selected camera renders into one shared runtime RenderTexture.
- Raising the tablet disables player gameplay control but does not pause officers, suspects, civilians, mission evaluation, or the operation clock.
- Feed metadata remains qualitative and exposes no exact officer rounds or hit points.

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

- Clean compilation after the Milestone 7B package is installed.
- Run the Milestone 7B setup and validator, then all EditMode tests.
- Complete every required objective and clear all six authored rooms; confirm the report appears automatically after the stable three-second all-clear.
- Confirm the report matches live civilian, suspect, officer, ROE, evidence, and elapsed-time facts.
- Manually end one unresolved run and confirm pending required objectives fail rather than being silently credited.
- Run the Milestone 7A validator and prior gameplay regressions with a clean Console.

## Historical regression checklist

- Clean compilation after the Milestone 7A package is installed.
- Run the Milestone 7A setup and validator, then all EditMode tests.
- Deploy from headquarters through all three entries with one and two selected officers.
- Verify every door can be opened, crossed by the full team, and crossed again in reverse.
- Confirm both open passages require no invented door interaction.
- Restart Play Mode several times and confirm suspect/civilian locations vary while their mission IDs and objectives remain correct.
- Clear every interior area, reintroduce an active threat to a clear room, and confirm clear status is revoked.
- Confirm automatic challenges/custody, MMB commands, body-camera tablet feeds, mission evidence, and debrief remain functional throughout the new layout.
- Run Milestones 1–6C regression validators and verify a clean Console.

- Clean compilation after the Milestone 6C package is installed.
- Run the Milestone 6C setup and validator.
- Verify all three selected entry IDs resolve to baked NavMesh positions.
- Verify one- and two-officer deployments, live feed switching, tablet close/control restoration, and clean Console.
- Verify AI initiative continues while the operational tablet is raised.
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

No new 3D model or package is required for Milestone 7B. The after-action system consumes existing gameplay evidence and uses generated uGUI presentation. The first environment-art request remains a modular municipal/industrial interior-exterior kit after greybox layout lock.
