# Rules of Entry

Unity `6000.5.2f1` HDRP tactical law-enforcement simulation by Trooper Studios.

## Current delivery

Milestone 5.5 Visual Revision 4 with Temporary Character Hotfix 2 is a front-end, UI, and temporary-character integration candidate built on the validated and pushed Milestone 5 gameplay baseline.

This delivery adds:

- an authored Trooper Studios splash screen;
- a blocking photosensitivity and legal warning using the supplied project artwork;
- a Rules of Entry title screen;
- an original moonlit Calder City background with an officer overlook and clear menu-safe negative space;
- a cinematic, flat-text main menu with campaign placeholders, Operations, Training, Settings, Credits, and Quit;
- oversized stacked title typography and restrained police-blue selection response without boxed navigation cards;
- master-volume, fullscreen, and quality settings with persistence;
- a cinematic asynchronous loading screen whose large destination label is read from the authoritative mission definition instead of baked artwork or placeholder copy;
- a unified tactical visual language across the existing interaction, weapon, officer, mission, and AI displays;
- an F10 developer-diagnostics toggle to reduce normal HUD clutter;
- front-end generation, validation, pre-build checks, and EditMode tests.
- a reversible, presentation-only Humanoid bridge that applies the supplied sample FBX to the prototype suspect while preserving its AI, navigation, custody, injury, and hit-region systems.

Follow `UI_PRESENTATION_INSTALL.md`, then `TEMPORARY_CHARACTER_INSTALL.md`. No Inspector wiring is required.

The setup keeps `RoE v3` as the validated Unity product name and updates the company identity to `Trooper Studios`. Depending on Unity license and platform requirements, Unity's legal engine splash may appear before the authored Trooper Studios screen. The temporary character is intentionally not a production asset: it has neutral generated materials, no LODs, no facial shapes, and no authored animation set.
