# Milestone 0 Installation and Validation

## Completion status

Milestone 0 was successfully integrated and validated on 2026-07-16:

- Unity compilation: passed with zero errors;
- project validation: 15 passes, zero errors, one expected `DefaultCompany` warning;
- Edit Mode tests: passed;
- Play Mode tests: passed;
- prototype Play Mode smoke test: passed with no errors or exceptions.

The remaining instructions are retained for clean-checkout recovery and future workstation setup.

## What this package does

This package adds the Rules of Entry project-owned runtime, editor, and test assemblies. Its setup command creates the complete folder scaffold, copies the HDRP starter scene into a project-owned prototype scene, adds a foundation marker, and configures Build Settings without deleting `Assets/OutdoorsScene.unity`.

No package or manual Inspector changes are required.

## Install

1. Close Unity.
2. Open this package and copy its `Assets` folder plus the updated root Markdown files into `C:\Users\Shadow\Videos\RoE v3`.
3. Choose **Merge** for folders and **Replace** for the updated Markdown documentation.
4. Reopen the project in Unity `6000.5.2f1`.
5. Wait for script compilation and asset import to finish.
6. Confirm the Console has no compiler errors before continuing.
7. Select **Tools > Rules of Entry > Milestone 0 > Build Foundation**.
8. Allow the tool to save open scene changes if Unity asks.
9. The tool will open `Assets/_Project/RulesOfEntry/Scenes/Prototype/ROE_Prototype.unity` and automatically run validation.

## Expected validation

The validation dialog should report **zero errors**. A warning for `DefaultCompany` is expected until the approved studio/company name is selected.

Run validation again at any time from:

`Tools > Rules of Entry > Validate Project`

## Tests

1. Open **Window > General > Test Runner**.
2. Select **EditMode** and run all tests.
3. Select **PlayMode** and run all tests.
4. Both test groups should pass.
5. Press Play in `ROE_Prototype.unity`; the scene should display the same HDRP sky view as the original starter scene with no exceptions.

## Expected project changes

- `Assets/_Project/RulesOfEntry/` exists with the complete planned directory structure.
- `ROE_Prototype.unity` exists and is the first enabled Build Settings scene.
- `OutdoorsScene.unity` remains untouched and is disabled in Build Settings.
- A `[RulesOfEntry]` object containing `SceneFoundationMarker` exists in the prototype scene.
- Runtime scripts cannot reference editor-only code because the assemblies are separated.
- Future builds are blocked if foundation validation contains an error.

## Common problems

### Tools menu is missing

Wait for Unity to finish compiling. If it remains missing, inspect the first red Console error and do not run any other setup step.

### Prototype scene already exists

The setup command does not overwrite it. It opens the existing scene, ensures the foundation marker exists, and repairs Build Settings.

### Validator reports missing folders or scene

Run **Tools > Rules of Entry > Milestone 0 > Build Foundation** again. The operation is designed to be safe to repeat.

## Git push after validation

```powershell
git status
git add -A
git commit -m "Complete Milestone 0 project foundation"
git push
```
