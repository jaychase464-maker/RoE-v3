# Milestone 4 Hotfix 5 — Persistent Challenge Sequence

## Goal

Keep a coordinated verbal challenge active after first contact instead of requiring the suspect to remain inside the officer's normal forward field of view on every scan.

## Changed files

- Replaces `Assets/_Project/RulesOfEntry/Runtime/Officers/OfficerInitiativeController.cs`.
- Adds `Assets/_Project/RulesOfEntry/Runtime/Officers/OfficerChallengeRules.cs`.
- Adds `Assets/_Project/RulesOfEntry/Tests/EditMode/OfficerChallengeRulesTests.cs`.

## Install

1. Exit Play Mode and close Unity.
2. Extract `RulesOfEntry-M4-Initiative-Hotfix-5.zip` directly into the `RoE v3` project root.
3. Merge folders and replace files.
4. Reopen Unity and wait for compilation.

No package, Input System action, project setting, scene rebuild, Inspector assignment, or Milestone setup rerun is required.

## Expected behavior

- The first officer with valid sight challenges the suspect.
- The team retains tactical focus for 20 seconds and refreshes that memory after each valid follow-up.
- One officer repeats an appropriate command approximately every four seconds while the suspect remains free, actionable, within range, and in unobstructed sight.
- The sequence stops immediately after surrender, restraint, incapacitation, death, loss of range, or sustained loss of contact.
- A verified-clear room with two actionable officers allows one officer to begin automatic custody while the other maintains cover.
- Suspects still decide whether to comply, flee, resist, or threaten. Follow-up commands do not force a surrender outcome.

## Test

1. Enter Play Mode, select the team, open the door, and move both officers inside the north room.
2. Do not press the player verbal-command or restraint controls.
3. If the suspect remains noncompliant and visible, confirm another team challenge occurs about every four seconds.
4. When the suspect genuinely surrenders, confirm the room verifies clear.
5. Confirm one officer receives `RestrainSubject [INITIATIVE]`, approaches, directs the suspect to kneel, and applies restraints while the second officer remains available for cover.
6. Confirm the mission reaches its final report without Console errors.

If the suspect moves behind solid cover or outside detection range, the absence of another shouted command is intentional.
