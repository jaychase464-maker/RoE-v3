# Project Context

Current UI direction: the operation HUD must remain sparse and realistic. It exposes qualitative squad condition and ammunition, never player hit points or exact ammunition counts. Campaign identity and the mission clock drive the RoE body-camera overlay. Squad commands are contextual and appear while middle mouse is held.

## Identity

- Title: Rules of Entry
- Studio: Trooper Studios
- Engine: Unity `6000.5.2f1`
- Rendering: HDRP `17.5.0`
- Input: Input System `1.19.0`, New Input System only
- Navigation: AI Navigation `2.0.14`
- Source of truth: `jaychase464-maker/RoE-v3`, branch `main`
- Protected pushed baseline commit: `c62b09d99b3ced36c386d132cba2a2b0df4297a8`

## Architecture rules

- Runtime assemblies own player-visible state and navigation; editor assemblies own deterministic scene generation and validation.
- Enabled build order is `ROE_FrontEnd.unity`, `ROE_Headquarters.unity`, then `ROE_Prototype.unity`.
- Campaign operation selection belongs inside the playable headquarters, never directly on the main menu.
- The player physically selects an available operation, then uses the rugged tablet for briefing, personnel, support, entry selection, and ready-up.
- Headquarters-to-operation state crosses scenes only through stable identifiers; it never retains scene object references.
- Operation entry anchors resolve those identifiers only after the destination scene owns the player, NavMesh, and officers.
- The in-mission tablet is separate from the headquarters planning controller and cannot clear or rewrite deployment state.
- Live officer body-camera views are read-only, render one selected source at a time, and cannot expose hidden AI state or mutate officer commands.
- Front-end panels are controlled by one explicit state machine: splash, warning, title, main menu, settings, credits, and loading.
- Loading destinations come from the actual destination context: headquarters from the front end, and operation/location/entry data from the planning tablet.
- UI navigation uses `InputSystemUIInputModule`; legacy `StandaloneInputModule` and legacy input calls are forbidden.
- Settings persist through namespaced `PlayerPrefs` keys and are applied through current Unity APIs.
- Existing gameplay UI components remain authoritative. Presentation setup changes their layout, colors, typography, and chrome without replacing their data sources.
- Developer diagnostics remain accessible but are hidden by default.
- Mission evaluation remains a one-way consumer of factual combat, AI, custody, room, order, and initiative evidence.
- No automatic reload, exact ammunition HUD, navigation warp, instant arrest, or direct score mutation is allowed.
- Unity object identity uses full 64-bit `EntityId` values.
- Character art is a replaceable presentation layer. Actor AI, navigation, injury, custody, evidence, and hit regions remain on the protected actor prefab root.

## Protected gameplay baseline

- Milestone 0: project foundation.
- Milestone 1: first-person controller and tactical interaction.
- Milestone 2: manual firearm operation and force-event ledger.
- Milestone 3: suspect, civilian, surrender, custody, and room behavior.
- Milestone 4: officer commands, door traversal, persistent challenges, room clearance, and bounded custody initiative.
- Milestone 5: mission objectives, ROE review, evidence aggregation, and after-action scoring.

## Presentation boundary

The current visual system combines deterministic uGUI layout with supplied Trooper Studios splash/warning artwork, an original moonlit city-overlook background, oversized stacked title typography, flat text navigation, destination-aware loading, and a rugged police mobile-data-terminal interface. Headquarters architecture remains gameplay-first greybox. A user-supplied high-density FBX is accepted only as a reversible temporary suspect presentation. Final station art, physical tablet art, character assets, animation clips, sound design, accessibility, localization, and complete remapping remain future production work.
