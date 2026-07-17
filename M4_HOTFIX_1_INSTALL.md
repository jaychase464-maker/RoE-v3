# Milestone 4 Hotfix 1 — Input Action Persistence

## Fix

The original Milestone 4 setup modified the imported InputActionAsset in memory. The immediate validator could see those actions, but the JSON-backed `.inputactions` source could revert to Milestone 3 data after a Play Mode/domain reload.

Hotfix 1:

- serializes the configured asset through `InputActionAsset.ToJson()`;
- writes the authoritative `.inputactions` JSON source;
- forces a synchronous Unity reimport;
- reloads the imported asset before assigning it to player prefabs or the scene;
- logs a missing-input configuration error only once;
- includes the exact missing action paths in that error.

No officer behavior, firearm behavior, custody rules, scene geometry, package version, or render setting changes.

## Install

1. Exit Play Mode.
2. Close Unity.
3. Extract `RulesOfEntry-Milestone-4-Hotfix-1.zip` directly into `C:\Users\Shadow\Videos\RoE v3`.
4. Merge folders and replace files.
5. Reopen Unity and wait for compilation.
6. Run `Tools > Rules of Entry > Milestone 4 > Build Officer Team Prototype` again.
7. Wait for the setup and validator to finish.
8. Enter Play Mode and confirm the repeated input errors are gone.

## Validation

- Run `Tools > Rules of Entry > Milestone 4 > Validate Officer Team Prototype`.
- Run all EditMode and PlayMode tests.
- Verify `1`, `2`, `3`, `G`, `H`, `J`, `Y`, `U`, `K`, and `Z` respond as documented.
- Confirm the Console remains clean during Play Mode.

If input validation still fails, send the new single error. It will identify every missing action by full map/action path.
