# Bugs and Validation Gaps

## Confirmed bugs

No Milestone 1 gameplay bug was found during live Unity validation.

## Open validation gaps and technical debt

### ROE-0002 — Default company identity remains configured

- Type: configuration debt
- Priority: low
- Status: open
- Evidence: the stable validator reports `DefaultCompany`
- Planned resolution: set the approved studio identity before a distributable build; do not guess the legal name

### ROE-0004 — AI Navigation authoring package not declared

- Type: dependency gap
- Priority: normal
- Status: planned for Milestone 3
- Evidence: `com.unity.modules.ai` is present; `com.unity.ai.navigation` is not declared
- Planned resolution: verify and install the Unity-compatible version immediately before navigation work

## Resolved validation gaps

### ROE-0001 — Live compiler status not recorded

- Status: resolved on 2026-07-16
- Resolution: the project opened in Unity `6000.5.2f1` with no baseline compiler errors

### ROE-0003 — Generic starter input actions were not tactical-game ready

- Status: implementation delivered in Milestone 1
- Resolution: `ROE_InputActions.inputactions` now owns movement, look, sprint, crouch, interact, and cursor actions; later milestones will add weapon and command actions only when their systems exist

### ROE-0005 — Milestone 0 integration was not validated

- Status: resolved on 2026-07-16
- Resolution: foundation validation reported 15 passes, zero errors, and one expected warning; all tests and the Play Mode smoke test passed

### ROE-0006 — Milestone 1 live integration was not validated

- Status: resolved on 2026-07-16
- Resolution: Unity compiled with zero errors; Milestone 1 setup and validation completed without errors; all Edit Mode and Play Mode tests passed; the complete manual gameplay smoke test passed; Play Mode produced no Console errors or exceptions

## New bug format

```text
ID:
Title:
Milestone/version:
Severity:
Status:
Environment:
Reproduction steps:
Expected:
Actual:
Console/error text:
Evidence:
Suspected system:
Workaround:
Resolution and validation:
```

Use `ROE-####` identifiers. Never delete a resolved entry; retain its resolution and validation evidence.
