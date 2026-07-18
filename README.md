# Rules of Entry

Current integration candidate: Milestone 6B Tactical HUD. After Unity finishes compiling, run `Rules of Entry > Milestone 6B > Build Tactical HUD`, then `Tools > Rules of Entry > Milestone 6B > Validate Tactical HUD`. See `TACTICAL_HUD_INSTALL.md` for the complete test and rollback checklist.

Unity `6000.5.2f1` HDRP tactical law-enforcement simulation by Trooper Studios.

## Current delivery

Milestone 6A is a headquarters and operation-planning integration candidate built on the validated Milestones 0–5 and pushed Milestone 5.5 presentation baseline.

The current project includes the cinematic front end and temporary suspect presentation from Milestone 5.5. Milestone 6A adds:

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

Follow `MILESTONE_6A_INSTALL.md` for headquarters and `TACTICAL_HUD_INSTALL.md` for the operation HUD. No manual Inspector wiring or new package is required.

The main-menu Operations shortcut enters headquarters. Continue Campaign and New Campaign remain disabled until campaign/save ownership is implemented. The headquarters is intentionally greybox: final police-station architecture, furniture, lockers, armory assets, and a physical rugged-tablet model are not required for this milestone.
