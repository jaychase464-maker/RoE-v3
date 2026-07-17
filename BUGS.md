# Bugs and Validation Gaps

## Confirmed bugs

No gameplay bugs are confirmed because gameplay systems have not yet been implemented.

## Open validation gaps and technical debt

### ROE-0002 — Default company identity remains configured

- Type: configuration debt
- Priority: low
- Status: open
- Evidence: `ProjectSettings/ProjectSettings.asset` contains `companyName: DefaultCompany`
- Planned resolution: set an approved studio/company identity during a future project-settings pass; do not guess the legal name

### ROE-0003 — Generic starter input actions are not tactical-game ready

- Type: foundation debt
- Priority: normal
- Status: planned for Milestone 1
- Evidence: current Player map provides Move, Look, Attack, Interact, Crouch, Jump, Previous, Next, and Sprint only
- Planned resolution: create project-owned action maps and add aim, reload, stance/lean, command, select-team, low-ready, and pause actions with their owning systems

### ROE-0004 — AI Navigation authoring package not declared

- Type: dependency gap
- Priority: normal
- Status: planned for Milestone 3
- Evidence: `com.unity.modules.ai` is present; `com.unity.ai.navigation` is absent from `Packages/manifest.json`
- Planned resolution: resolve and install the compatible version from Unity Package Manager immediately before navigation implementation

## Resolved validation gaps

### ROE-0001 — Live compiler status not recorded

- Type: validation gap
- Status: resolved on 2026-07-16
- Resolution: user opened the repository project in Unity `6000.5.2f1` and confirmed the pre-Milestone-0 Console contained no errors

### ROE-0005 — Milestone 0 integration was not validated

- Type: validation gap
- Status: resolved on 2026-07-16
- Resolution: foundation setup completed; validation reported 15 passes, zero errors, and one expected company-name warning; all Edit Mode and Play Mode tests passed; prototype Play Mode produced no errors or exceptions

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

Use `ROE-####` identifiers. Never delete a resolved entry; move it to a resolved section with the fixing commit and test result.
