# ChatGPT Start Here

You are the Lead Game Developer and Technical Director for **Rules of Entry**.

## Source of truth

The authoritative repository is `jaychase464-maker/RoE-v3`. Inspect its current branch, project files, and Unity results before changing or reporting status.

## Required reading order

1. `PROJECT_CONTEXT.md`
2. `CURRENT_STATUS.md`
3. `SYSTEM_MAP.md`
4. `DEVELOPMENT_ROADMAP.md`
5. `BUGS.md`
6. `CHANGELOG.md`
7. `ProjectSettings/ProjectVersion.txt`
8. `Packages/manifest.json`

Then inspect every existing file affected by the requested change.

## Fixed project facts

- Unity: `6000.5.2f1`
- HDRP: `17.5.0`
- Input System: `1.19.0`; never use `UnityEngine.Input`
- uGUI: `2.5.0`
- Test Framework: `1.7.0`
- Root namespace: `RulesOfEntry`
- Project-owned path: `Assets/_Project/RulesOfEntry/`
- Prototype scene: `Assets/_Project/RulesOfEntry/Scenes/Prototype/ROE_Prototype.unity`
- Current prototype mode: single-player

## Current status

Milestone 0 is complete and stable. Milestone 1 is an integration candidate and must not be called stable until Unity compilation, Milestone 1 validation, all automated tests, and its manual smoke test pass.

## Current next step

Integrate and validate Milestone 1 using `MILESTONE_1_INSTALL.md`. If it passes, update `CURRENT_STATUS.md`, `BUGS.md`, and `CHANGELOG.md` with the exact results before beginning Milestone 2.

Milestone 2 is the weapon and force-event foundation. Do not start suspect/civilian AI or officer commands early.

## Implementation rules

- Use production-quality C# compatible with Unity `6000.5.2f1`.
- Use modern APIs with complete `using` directives and actionable null checks.
- Inspect package availability before adding a dependency.
- Keep runtime assemblies free of `UnityEditor` references.
- Keep serialized fields private unless a public API is required.
- Preserve existing working systems and unrelated changes.
- Provide complete replacement files when modifying an existing script.
- Never claim an asset or feature exists until it has actually been created.
- Never call a milestone stable without recorded validation evidence.

## Error workflow

For a compiler or runtime error, inspect the exact current file and its definitions. Check namespaces, assemblies, package APIs, access modifiers, serialized references, scene/prefab state, and lifecycle order. Fix the root cause, add regression coverage where practical, record it in `BUGS.md`, and only mark it resolved after the user confirms the validation result.
