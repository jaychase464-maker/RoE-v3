# Changelog

All notable project changes are documented here. The project is pre-release and uses milestone labels until a public versioning scheme is selected.

## Milestone 0 — Project Foundation — Completed 2026-07-16

### Added

- Project-owned runtime, editor, Edit Mode test, and Play Mode test assemblies.
- Project identity, structured logging, scene marker, repeatable setup, validation, build gate, and tests.
- Organized project folder scaffold and project-owned prototype scene.

### Validated

- Unity compilation completed with zero errors.
- Foundation validation completed with 15 passes, zero errors, and one expected `DefaultCompany` warning.
- All Edit Mode and Play Mode tests and the Play Mode smoke test passed.

## Milestone 1 — First-Person and Tactical Interaction — Completed 2026-07-16

### Added

- Project-owned Player and System Input System action maps.
- CharacterController movement, walk, forward sprint, crouch, gravity, and obstruction-safe stance transition.
- Mouse/gamepad camera look and cursor/input-mode control.
- Reusable interaction context, prompt, target base, focus ray, instant use, and hold use.
- Runtime interaction prompt UI.
- Animated training door and stateful hold-to-use control panel.
- Repeatable editor tool that creates layers, HDRP graybox materials, prefabs, and prototype scene content.
- Milestone 1 validator, pre-build gate, Edit Mode configuration test, and Play Mode interaction tests.
- Complete installation, setup, controls, testing, troubleshooting, and stability guide.

### Changed

- Runtime and editor assembly definitions now reference the installed Input System and uGUI assemblies.
- Project status and system documentation now describe the Milestone 1 integration candidate.

### Packages and settings

- No package manifest change.
- Setup adds `Player` and `Interactable` layers using available user-layer slots.

### Validated

- Unity compilation completed with zero errors.
- Milestone 1 setup and validation completed with zero errors.
- All Edit Mode and Play Mode tests passed.
- Movement, sprint, crouch clearance, collision, interaction prompts, door operation, hold interaction, and cursor behavior passed manual testing.
- Play Mode produced no Console errors or exceptions.
