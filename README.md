# Rules of Entry

Current integration candidate: Milestone 7A Pressure Point Multi-Room Mission Greybox. After Milestone 6C passes, run `Tools > Rules of Entry > Milestone 7A > Build Pressure Point Mission Greybox`, then `Tools > Rules of Entry > Milestone 7A > Validate Pressure Point Mission Greybox`. See `MILESTONE_7A_INSTALL.md` for the complete test checklist.

Unity `6000.5.2f1` HDRP tactical law-enforcement simulation by Trooper Studios.

## Current delivery

Milestone 7A is the first mission-greybox integration candidate built on the Milestones 0–6C gameplay, planning, deployment, HUD, and tablet foundations.

The current project includes the cinematic front end, temporary suspect presentation, headquarters planning, tactical HUD, deployment, and operational tablet foundations. The cumulative delivery adds:

- an authored Trooper Studios splash screen;
- a blocking photosensitivity and legal warning using the supplied project artwork;
- a Rules of Entry title screen;
- an original moonlit Calder City background with an officer overlook and clear menu-safe negative space;
- a cinematic, flat-text main menu with campaign placeholders, Operations, Training, Settings, Credits, and Quit;
- oversized stacked title typography and restrained police-blue selection response without boxed navigation cards;
- master-volume, fullscreen, and quality settings with persistence;
- a playable Calder City Police headquarters greybox between the main menu and tactical deployment;
- physical mission selection through the established first-person interaction system;
- a wide, rugged handheld-tablet presentation with hardware bezel controls, seven top-navigation sections, two-column mission data, and persistent Start Operation controls;
- selectable officer assignments and three selectable entry plans;
- honest K9, drone, tactical-medic, and negotiator planning records that remain unavailable until their gameplay systems exist;
- identifier-only deployment context carried from headquarters into the operation;
- destination-aware loading from the menu into headquarters and from the tablet into the selected mission;
- a unified tactical visual language across the existing interaction, weapon, officer, mission, and AI displays;
- an F10 developer-diagnostics toggle to reduce normal HUD clutter;
- front-end generation, validation, pre-build checks, and EditMode tests.
- a reversible, presentation-only Humanoid bridge that applies the supplied sample FBX to the prototype suspect while preserving its AI, navigation, custody, injury, and hit-region systems.
- the approved operation HUD with a scalable squad roster, qualitative officer health/ammunition, dynamic RoE body-camera metadata, and held-MMB numbered commands.
- stable-ID operation entry deployment, assigned-officer filtering, and a `Tab` operational tablet with live switchable officer body-camera feeds.
- the first multi-room mission greybox: Calder City Municipal Pumping Annex / Operation Pressure Point;
- nine connected operational areas, including three exterior staging zones and six evidence-producing interior spaces;
- three entry-specific approaches with player and up-to-eight-officer formation anchors on baked NavMesh;
- seven door-gated thresholds plus two open passages in a validated room/portal graph;
- twelve weighted suspect/civilian locations selected by a logged incident seed without changing actor identity;
- a pump-hall clearance objective bound to an authored room ID instead of the former training-room placeholder;
- a Milestone 7A scene generator, validator, and EditMode topology/scenario tests.

Follow `MILESTONE_6A_INSTALL.md` for headquarters, `TACTICAL_HUD_INSTALL.md` for the operation HUD, `MILESTONE_6C_INSTALL.md` for deployment/body cameras, and `MILESTONE_7A_INSTALL.md` for the first mission greybox. No manual Inspector wiring or new package is required.

The main-menu Operations shortcut enters headquarters. Continue Campaign and New Campaign remain disabled until campaign/save ownership is implemented. The headquarters is intentionally greybox: final police-station architecture, furniture, lockers, armory assets, and a physical rugged-tablet model are not required for this milestone.
