# Bugs and Validation Gaps

## Confirmed integration defects — resolved

### ROE-0014 — Force-context validator matched `distancePenalty`

- Priority: blocker for validation
- Status: resolved and validated on 2026-07-16
- Actual behavior: the accountability scan treated the variable `distancePenalty` as a reference to a mission penalty system
- Expected behavior: only standalone prohibited score identifiers trigger validation
- Resolution: replace substring matching with culture-invariant, whole-identifier regular expressions
- Gameplay impact: none; this was a validator-only false positive

### ROE-0013 — Ambiguous Unity PackageInfo blocked Milestone 3 compilation

- Priority: blocker
- Status: resolved and validated on 2026-07-16
- Environment: Unity `6000.5.2f1`
- Actual behavior: `RulesOfEntryMilestoneThreeValidator.cs` produced `CS0104` because both `UnityEditor.PackageManager.PackageInfo` and `UnityEditor.PackageInfo` were in scope
- Expected behavior: the validator compiles against the Package Manager API
- Resolution: explicitly alias `PackageInfo` to `UnityEditor.PackageManager.PackageInfo`
- Validation required: clean Unity compilation and Milestone 3 validator execution

## Open validation gaps and technical debt

### ROE-0002 — Default company identity remains configured

- Priority: low
- Status: open
- Resolution condition: approved studio identity is supplied before a distributable build

### ROE-0008 — External ballistics and terminal material response are simplified

- Priority: high realism debt
- Status: open
- Current behavior: instantaneous physical-muzzle raycast, muzzle obstruction, region-aware actor response, and factual hit capture
- Missing: time of flight, gravity, drag, zeroing, penetration, fragmentation, ricochet, and validated residual-energy injury model

### ROE-0011 — Actor physiology is a prototype approximation

- Priority: high realism debt
- Status: open
- Current behavior: hit region, muzzle-energy proxy, bleeding, blood volume, consciousness, mobility, incapacitation, and death thresholds
- Missing: validated wound profiles, residual energy, armor, organ/vascular structures, pain, shock, airway, respiration, medical interventions, and expert review

### ROE-0012 — Threat behavior has no production weapon or attack execution

- Priority: high
- Status: planned for a later combat-AI slice
- Current behavior: a suspect may choose and visibly report a threatening state
- Missing: suspect weapon inventory presentation, draw timing, aim error, cover use, fire discipline, surrender interruption animation, and safe tactical testing

## Fixed or resolved entries

- `ROE-0001`: baseline compiler status recorded.
- `ROE-0003`: project-owned tactical input implemented.
- `ROE-0004`: AI Navigation authoring dependency declared as `com.unity.ai.navigation 2.0.14`; live install validation is covered by `ROE-0010`.
- `ROE-0005`: Milestone 0 validated.
- `ROE-0006`: Milestone 1 validated and stable.
- `ROE-0007`: Milestone 2 formal regression evidence passed during Milestone 3 closure.
- `ROE-0009`: `GetInstanceID()` replaced with full 64-bit `EntityId` storage; source regression test added.
- `ROE-0010`: Milestone 3 package resolution, compilation, setup, validator, all tests, seeded smoke test, prior regressions, and clean Console passed on 2026-07-16.
- `ROE-0013`: ambiguous `PackageInfo` resolved with an explicit Package Manager alias; Unity compilation passed.
- `ROE-0014`: score-boundary false positive resolved with whole-identifier matching; validation passed.

Never delete resolved entries. New defects use `ROE-####` and record environment, reproduction, expected/actual behavior, Console text, evidence, workaround, resolution, and validation.
