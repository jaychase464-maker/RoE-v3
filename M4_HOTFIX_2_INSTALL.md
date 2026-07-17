# Milestone 4 Hotfix 2 — Saved Squad References

## Fix

The Milestone 4 builder assigned Officer Alpha, Officer Bravo, the command camera, player input, and the order marker to a prefab-instance component in the scene, but did not explicitly record those assignments as prefab-instance property overrides. The scene could therefore reopen with an incomplete `OfficerSquadController` even though setup had run.

Hotfix 2:

- explicitly marks the squad and command UI components dirty;
- records their prefab-instance property modifications;
- saves and reloads `ROE_Prototype.unity` from disk during setup;
- stops setup if the reloaded scene is missing any squad reference;
- makes the validator inspect the actual saved scene component configuration;
- names each missing reference in runtime and validator errors.

Hotfix 2 includes the Hotfix 1 input-action persistence changes.

## Install

1. Exit Play Mode and close Unity.
2. Extract `RulesOfEntry-Milestone-4-Hotfix-2.zip` directly into `C:\Users\Shadow\Videos\RoE v3`.
3. Merge folders and replace files.
4. Reopen Unity and wait for compilation.
5. Run `Tools > Rules of Entry > Milestone 4 > Build Officer Team Prototype` again.
6. Let setup save, reload, and validate the prototype scene.
7. Enter Play Mode and confirm the squad-reference error is gone.

## Expected validation

The Console should include this passing check:

`M4 Saved Squad References: The saved scene retains player input, command view, Alpha, Bravo, and order-marker references.`

If setup or validation still stops, send the first error exactly. It will identify the missing saved reference.
