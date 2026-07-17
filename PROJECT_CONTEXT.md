# Project Context

## Identity

- Title: Rules of Entry
- Studio: Trooper Studios
- Engine: Unity `6000.5.2f1`
- Rendering: HDRP `17.5.0`
- Input: Input System `1.19.0`, New Input System only
- Navigation: AI Navigation `2.0.14`
- Source of truth: `jaychase464-maker/RoE-v3`, branch `main`
- Protected UI-baseline commit: `e9a3503b4a3f15abbe93d89bc10382af5c293297`

## Architecture rules

- Runtime assemblies own player-visible state and navigation; editor assemblies own deterministic scene generation and validation.
- `ROE_FrontEnd.unity` is the first enabled build scene and loads `ROE_Prototype.unity` asynchronously.
- Front-end panels are controlled by one explicit state machine: splash, warning, title, main menu, settings, credits, and loading.
- Loading destinations come from the authoritative `MissionDefinition.DisplayName`; no mission name is baked into the background image.
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

The current visual system combines deterministic uGUI layout with supplied Trooper Studios splash/warning artwork, an original moonlit city-overlook background, oversized stacked title typography, flat text navigation, and a destination-aware lower-third loading treatment. The cinematic plate contains no baked menu or loading content. A user-supplied high-density FBX is accepted only as a reversible temporary suspect presentation. Final character assets, animation clips, sound design, accessibility options, localization, and complete remapping remain future production work.
