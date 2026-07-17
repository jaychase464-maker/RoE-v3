# Milestone 4 Installation and Validation

## Goal

Milestone 4 adds a two-officer, interruptible command prototype on top of the validated Milestone 3 human behavior and custody scene.

## Files

This package contains new runtime officer scripts, a complete replacement `TacticalPlayerInput.cs`, editor setup/validation tools, tests, and updated project documentation. It does not replace any package manifest or require a new Unity package.

## Install

1. Close Unity.
2. Extract `RulesOfEntry-Milestone-4.zip` into the repository root: `C:\Users\Shadow\Videos\RoE v3`.
3. Allow Windows to merge folders and replace files when prompted.
4. Reopen the project in Unity `6000.5.2f1` and wait for compilation to finish.
5. Confirm the Console has no compiler errors.
6. Run `Tools > Rules of Entry > Milestone 4 > Build Officer Team Prototype`.
7. Let the setup finish and review the validation results in the Console.

The setup tool creates the required Input System actions itself. Do not manually edit the input asset first.

## Generated assets and scene changes

- `Prefabs/Actors/ROE_OfficerAlpha.prefab`
- `Prefabs/Actors/ROE_OfficerBravo.prefab`
- `Prefabs/UI/ROE_OfficerOrderMarker.prefab`
- `Prefabs/UI/ROE_OfficerCommandDebugUI.prefab`
- four HDRP graybox materials under `Art/Materials`
- `[Milestone4_OfficerTeam]` and `ROE_OfficerCommandDebugUI` in `ROE_Prototype.unity`
- a fixed `M4_TrainingDoorTraversalLink` gated by the physical training door
- `M4_NorthTrainingRoomClearance`, requiring two officers and timed threat verification
- `OfficerSquadController` on the player prefab and scene instance
- officer command actions added to `ROE_InputActions.inputactions`

No manual Inspector assignments are required when the setup tool completes successfully.

## Controls

Keyboard and mouse:

- `1`: select Officer Alpha
- `2`: select Officer Bravo
- `3`: select both officers
- `G`: move to the point under the reticle
- `H`: hold current position
- `J`: follow the player
- `Y`: stack at the door under the reticle
- `U`: approach and open the door under the reticle
- `K`: approach and restrain the subject under the reticle
- `Z`: cancel selected officers' active orders

Gamepad prototype bindings:

- `B` / button east: cycle Alpha, Bravo, and Team selection
- D-pad right: contextual order; restrain a person, stack at a door, or move to a world point
- View / select: cancel selected officers' active orders

Existing movement, firearm, interaction, and verbal-command controls remain unchanged.

## Realistic custody behavior

An officer will not instantly arrest a free, capable subject. For a cooperative arrest:

1. Aim at the suspect and press `F` until the subject enters `Surrendering`.
2. Aim at that subject and press `K`.
3. The assigned officer finds a complete NavMesh path and physically approaches.
4. The officer directs the subject to kneel.
5. The officer waits for a controlled kneeling interval.
6. The officer spends time applying and checking handcuffs.
7. Only then does custody become `Restrained`.

If the subject is free, abandons surrender, the path fails, or the officer becomes incapacitated, the order fails with a specific reason instead of forcing the result.

## Test checklist

1. Run `Tools > Rules of Entry > Milestone 4 > Validate Officer Team Prototype` and confirm zero errors.
2. Run all EditMode tests.
3. Run all PlayMode tests.
4. Enter Play Mode with the Game view focused.
5. Select Alpha, issue `G`, and confirm only Alpha walks to the marker.
6. Press `Z` during a long move and confirm Alpha stops with `CancelledByPlayer`.
7. Select Team and issue `G`; confirm both officers use separated destinations.
8. Issue `Y` at a door, then `U`; confirm physical approach occurs before the door opens and the order waits for leaf clearance.
9. With the door open, aim at the floor beyond it and issue `G`; confirm both officers traverse the threshold without teleporting.
10. Issue `K` at the free suspect; confirm `SubjectNotCompliant` rather than an instant arrest.
11. Create a surrender with `F`, then issue `K`; confirm approach, kneel, timed cuffing, and restraint.
12. Press `J`, move the player, and confirm selected officers follow until cancelled.
13. Recheck Milestone 1 interaction, Milestone 2 manual firearm behavior, and Milestone 3 civilian/suspect behavior.
14. Confirm the Console contains no errors or exceptions during Play Mode.
15. Without pressing `F` or `K`, move both officers into the north room and confirm one challenges the suspect automatically.
16. Confirm refusal or deceptive compliance blocks unsafe automatic custody.
17. After genuine surrender and room-clear verification, confirm the closest officer physically cuffs while the partner remains available for cover.
18. Confirm the civilian is never automatically selected for custody.

## Expected behavior

- Every order records pending, accepted/refused, executing, and terminal facts.
- A newer order explicitly cancels the previous order as `Superseded`.
- Navigation never teleports an officer.
- Follow remains active until cancelled or replaced.
- Hold stops movement and completes at the officer's actual position.
- Door and restraint orders validate their targets and physical path.
- Doorway navigation remains blocked until the door leaf physically clears the configured threshold.
- Visible suspects are challenged automatically without forcing compliance.
- Automatic custody begins only after two-officer, timed room-clear verification and retains every physical restraint step.
- The right-side prototype HUD exposes selection, order status, details, and failure reasons.

## Common problems

### Input reports missing officer actions

Run the Milestone 4 build tool once. It adds and saves the actions before wiring the player.

### `NoNavigationSurface`

Re-run the Milestone 3 setup tool, wait for the NavMesh bake, then run the Milestone 4 setup again. Do not move the officers outside the graybox floor.

### `NoPath` or `TargetUnreachable`

Aim at a floor point inside the baked prototype space. A requested point across unbaked geometry is correctly rejected.

### Restraint reports `SubjectNotCompliant`

This is expected for a free, mobile subject. Establish surrender with the verbal-command system before assigning restraint.

### Door order targets the wall

Place the reticle directly on the door collider before pressing `Y`, `U`, or gamepad D-pad right.

### Door opens but officers report `NoPath`

Install Hotfix 3 and rerun the Milestone 4 build tool. The setup must regenerate `M4_TrainingDoorTraversalLink` in the prototype scene. Confirm the validator reports `M4 Door Traversal` as a pass, then issue a new `G` command to a floor point beyond the fully open door.

## Stability gate

Do not mark Milestone 4 complete until Unity compilation, the validator, all EditMode/PlayMode tests, the command/cancellation/door/restraint smoke tests, earlier milestone regressions, and a clean Play Mode Console are all confirmed.
