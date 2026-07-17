# Milestone 2 Hotfix 1 — Unity 6000.5 Entity IDs

## Goal

Remove the `CS0619` compile errors caused by Unity 6000.5 deprecating `Object.GetInstanceID()` as an error, while preserving the full 64-bit object identity in force-event records.

## Files

Replace these existing files:

- `Assets/_Project/RulesOfEntry/Runtime/Combat/ForceEventRecord.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Combat/UseOfForceEventLedger.cs`
- `Assets/_Project/RulesOfEntry/Tests/PlayMode/ForceEventLedgerTests.cs`
- `BUGS.md`
- `CHANGELOG.md`

Add this new file:

- `Assets/_Project/RulesOfEntry/Tests/EditMode/UnitySixApiCompatibilityTests.cs`

No Inspector assignments, scene changes, packages, or project-setting changes are required.

## Install

1. Close Play Mode if it is running.
2. Extract the hotfix ZIP into the `RoE v3` repository root.
3. Allow overwrite/replace when prompted.
4. Return to Unity and wait for script compilation to finish.
5. Confirm that both `CS0619` errors are gone.

## Validate

1. Confirm the Console has no red compiler errors.
2. Run the Milestone 2 validator.
3. Run all EditMode tests.
4. Run all PlayMode tests.
5. Run the Milestone 2 realism smoke test and confirm the Console remains clean.

## Expected behavior

- The project compiles on Unity 6000.5.2f1.
- Every firearm discharge records the shooter's complete Unity `EntityId` value.
- Hits record the hit collider's complete `EntityId`; misses record `0`.
- Weapon behavior, magazine state, and reload behavior are unchanged.

## Common problems

- If `GetInstanceID()` errors remain, verify Unity replaced `UseOfForceEventLedger.cs` and did not create a duplicate copy.
- If a test reports another runtime `GetInstanceID()` call, replace that call with `GetEntityId()` and preserve it with `EntityId.ToULong(...)` rather than casting to `int`.
- If Unity still shows stale compiler output after replacement, use `Assets > Refresh` and wait for compilation before clearing the Console.
