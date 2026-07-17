# Bugs and Validation Gaps

## Confirmed bugs

### ROE-0009 — Deprecated Unity object identifier blocked compilation

- Priority: blocker
- Status: fixed; awaiting Unity validation
- Reproduction: import the original Milestone 2 package in Unity 6000.5.2f1
- Actual behavior: `UseOfForceEventLedger.cs` failed with `CS0619` because `Object.GetInstanceID()` is obsolete with errors enabled
- Expected behavior: Milestone 2 compiles on the project's locked Unity version
- Resolution: use `Object.GetEntityId()` and preserve the complete identifier with `EntityId.ToULong(...)`
- Regression coverage: EditMode source scan plus PlayMode ledger identifier assertions
- Validation required: clean Unity compilation, EditMode and PlayMode tests, Milestone 2 validator, and clean Console

## Open validation gaps and technical debt

### ROE-0002 — Default company identity remains configured

- Priority: low
- Status: open
- Resolution condition: approved studio identity is supplied before a distributable build

### ROE-0004 — AI Navigation authoring package not declared

- Priority: normal
- Status: planned for Milestone 3
- Resolution condition: verify and install the Unity-compatible package immediately before navigation work

### ROE-0007 — Milestone 2 live integration not yet validated

- Priority: blocker for closing Milestone 2
- Status: open
- Required evidence: clean compilation, zero validator errors, all tests passed, both milestone smoke tests passed, and clean Play Mode Console

### ROE-0008 — External ballistics and terminal material response are simplified

- Priority: high realism debt
- Status: open
- Current behavior: instantaneous physical-muzzle raycast with muzzle obstruction and factual hit capture
- Missing: time of flight, gravity, drag, zeroing, material penetration, fragmentation, and ricochet
- Resolution condition: dedicated solver and material database validated against documented ammunition, barrel, distance, and barrier test cases

## Resolved validation gaps

- `ROE-0001`: baseline compiler status recorded.
- `ROE-0003`: project-owned tactical input implemented.
- `ROE-0005`: Milestone 0 validated.
- `ROE-0006`: Milestone 1 validated and stable.

Never delete resolved entries from repository history. New defects use the `ROE-####` format and include reproduction steps, expected/actual behavior, Console text, suspected system, workaround, resolution, and validation evidence.
