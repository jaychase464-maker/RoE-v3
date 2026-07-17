# Milestone 3 Installation and Validation

> Validation status: passed on 2026-07-16. This document remains the reproducible installation and regression procedure.

## Goal

Add deterministic suspect/civilian behavior, police commands, surrender/deception, region-aware condition, AI navigation, and a complete procedural arrest path without coupling any system to mission score.

## Dependency and project changes

- Required new package: `com.unity.ai.navigation 2.0.14`.
- `Packages/manifest.json` is a complete replacement preserving all existing dependencies and adding AI Navigation.
- `RulesOfEntry.Editor.asmdef` adds the `Unity.AI.Navigation` assembly reference.
- `RulesOfEntry.Runtime.asmdef` remains compatible with Input System and UGUI.
- The Input System remains the only active input handler.
- No Inspector assignment is performed manually; the setup tool wires references.
- No render, quality, or physics setting changes are required.

## Installation

1. Confirm Milestone 2 is committed and pushed.
2. Exit Play Mode and close Unity.
3. Extract `RulesOfEntry-Milestone-3.zip` into the `RoE v3` repository root.
4. Allow all replacement files to overwrite their existing versions.
5. Reopen the project in Unity `6000.5.2f1`.
6. Wait for Package Manager to install AI Navigation and for script compilation to finish. Do not run setup while Unity is compiling.
7. Confirm the Console has no red errors.
8. Run `Tools > Rules of Entry > Milestone 3 > Build Human Behavior Prototype`.
9. Allow the tool to update the player prefab, create actor/profile/UI assets, modify `ROE_Prototype.unity`, bake navigation, save assets, and run validation.

## Replaced files

- `Packages/manifest.json`
- `Assets/_Project/RulesOfEntry/Input/ROE_InputActions.inputactions`
- `Assets/_Project/RulesOfEntry/Runtime/RulesOfEntry.Runtime.asmdef`
- `Assets/_Project/RulesOfEntry/Editor/RulesOfEntry.Editor.asmdef`
- `Assets/_Project/RulesOfEntry/Runtime/Core/ProjectInfo.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Input/TacticalPlayerInput.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Combat/ForceEventRecord.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Combat/UseOfForceEventLedger.cs`
- root documentation files included in the package

All other `.cs` files in the package are new.

## Generated assets and scene changes

The setup tool creates or updates:

- `Data/AI/M3_UncertainSuspect.asset`
- `Data/AI/M3_PanickedCivilian.asset`
- `Data/AI/M3_PrototypeNavMesh.asset`
- `Prefabs/Actors/ROE_PrototypeSuspect.prefab`
- `Prefabs/Actors/ROE_PrototypeCivilian.prefab`
- `Prefabs/UI/ROE_HumanBehaviorDebugUI.prefab`
- three Milestone 3 HDRP materials
- `VerbalCommandEmitter` on `ROE_Player.prefab`
- `[Milestone3_HumanBehavior]` and `ROE_HumanBehaviorDebugUI` in `ROE_Prototype.unity`

## Controls

- Keyboard/mouse `F`: “Police! Show me your hands!”
- Gamepad left shoulder: same command
- `E` / gamepad west: perform the displayed custody step

## Automated validation

1. Run `Tools > Rules of Entry > Milestone 3 > Validate Human Behavior Prototype`.
2. Open `Window > General > Test Runner`.
3. Run all EditMode tests.
4. Run all PlayMode tests.
5. Confirm the Console remains clean after both runs.

Expected new tests cover:

- deterministic decision sequences;
- surrender and explicit decision reasons;
- panic/flee and unheard-command responses;
- custody transition ordering and invalid shortcuts;
- high-energy head-region incapacitation;
- complete arrest history and weapon recovery;
- complete Milestone 3 generated configuration.

## Milestone 3 manual realism test

1. Open `ROE_Prototype.unity` and enter Play Mode.
2. Confirm the top-left `AI DIAGNOSTICS` panel lists the suspect and civilian with no exceptions.
3. Approach until both actors are within command range and press `F` once.
4. Expected seeded response: the civilian surrenders; the suspect resists the first command.
5. Wait about 1.7 seconds so the suspect's first physical reaction is visible, then press `F` again.
6. Expected seeded response: the suspect surrenders deceptively.
7. Do not touch the suspect for approximately 5–8 seconds. Confirm he abandons surrender and returns to threat/flee behavior with reason `SurrenderAbandoned`.
8. Issue another command to regain compliance.
9. Look at a surrendering subject and hold `E` to order kneeling.
10. Hold `E` again to apply and check handcuffs.
11. Hold `E` again to search. Confirm the suspect search reports/records the weapon and secures it.
12. Hold `E` once more to confirm custody.
13. Confirm the final state is `InCustody` and the subject cannot break surrender.
14. Restart Play Mode and verify the same seed reproduces the same first/second command pattern.
15. Fire only in a controlled test run. Confirm actor condition changes and the force-event record captures the actor state from before impact.
16. Repeat the Milestone 1 movement/interaction test and Milestone 2 firearm/reload/magazine-check test.
17. Exit Play Mode and confirm the Console has no red errors or exceptions.

## Expected behavior

- Commands are limited by distance and obstruction; an unheard command records `CommandNotPerceived`.
- Responses are not universal or instant.
- Surrender does not equal custody.
- A capable free subject cannot be handcuffed or searched without the preceding steps.
- Restraints prevent deceptive surrender from being abandoned.
- Decision and custody histories are append-only and ordered.
- No command, arrest, injury, or firearm event changes mission score.

## Common problems

### AI Navigation package errors

- Confirm `Packages/manifest.json` contains `"com.unity.ai.navigation": "2.0.14"`.
- Wait for Package Manager and compilation to finish.
- If Unity was open during extraction, close and reopen the project before retrying.

### Setup reports no NavMesh data

- Confirm Milestone 1 floor geometry and colliders still exist.
- Confirm the active scene is saved at the project-owned prototype path.
- Re-run the Milestone 3 setup tool outside Play Mode.

### Actors do not move

- Run the validator and confirm `M3_PrototypeNavMesh.asset` exists.
- Select `[Milestone3_HumanBehavior]` and confirm its NavMesh Surface references baked data.
- Open the Navigation overlay and confirm the actors stand on blue walkable polygons.

### Commands reach zero subjects

- Move within 18 meters.
- Confirm a wall or closed door is not blocking the command path at long distance.
- Confirm the player prefab and scene player both have `VerbalCommandEmitter`.

### Interaction says the subject has not surrendered

- This is expected for a capable free subject. Issue a perceived command and obtain surrender first.
- Incapacitated subjects use the separate “Secure incapacitated subject” path.

## Git commands after every check passes

```powershell
git add .
git commit -m "Complete Milestone 3 human behavior and custody prototype"
git push origin main
```

Do not commit generated `Library`, `Temp`, `Logs`, or test-result output.
