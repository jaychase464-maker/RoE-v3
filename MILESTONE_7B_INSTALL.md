# Milestone 7B Installation — Automatic Completion and After-Action Tiers

## Goal

Milestone 7B ends a resolved operation automatically and grades the final immutable incident evidence. The grade covers objectives, civilians, suspects, officers, rules of engagement, evidence, and elapsed time. The score presentation cannot change the facts it displays.

No new Unity package, project setting, Inspector assignment, input action, or 3D model is required.

## Files created

- `Assets/_Project/RulesOfEntry/Runtime/Missions/MissionCompletionRules.cs`
- `Assets/_Project/RulesOfEntry/Runtime/UI/MissionAfterActionPresentation.cs`
- `Assets/_Project/RulesOfEntry/Editor/Milestone7B/RulesOfEntryMilestoneSevenBSetup.cs`
- `Assets/_Project/RulesOfEntry/Editor/Milestone7B/RulesOfEntryMilestoneSevenBValidator.cs`
- `Assets/_Project/RulesOfEntry/Tests/EditMode/MissionCompletionRulesTests.cs`
- `MILESTONE_7B_INSTALL.md`

The setup tool generates or replaces:

- `Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_AfterActionReport.prefab`
- the `ROE_AfterActionReport` instance in `ROE_Prototype.unity`

## Files replaced or extended

- `ActorInventory.cs` exposes read-only original-weapon and reportable-item facts.
- `MissionDefinition.cs` owns target and maximum-scored mission time.
- `MissionTypes.cs` adds tiers, categories, outcome metrics, and search-aware actor evidence.
- `MissionEvidenceCollector.cs` captures search, weapon recovery, and reportable-item facts.
- `AfterActionEvaluator.cs` builds the complete category score and tier.
- `MissionController.cs` owns the stable automatic-completion confirmation and final evidence lock.
- `MissionAfterActionDebugUI.cs` displays tier/countdown information when diagnostics are enabled.
- `AfterActionEvaluatorTests.cs` covers casualty caps, evidence, arrests, metrics, and tiers.
- Project documentation records the new authority and validation boundary.

## Scoring contract

| Category | Maximum |
|---|---:|
| Objectives | 30 |
| Civilian Safety | 20 |
| Suspect Custody | 15 |
| Officer Safety | 10 |
| Rules of Engagement | 10 |
| Evidence | 10 |
| Time | 5 |
| Total | 100 |

Tier thresholds are S `95–100`, A `90–94`, B `80–89`, C `75–79`, D `60–74`, and F `0–59`.

- Any required-objective failure caps the score at 74 / Tier D.
- Any officer death caps the score at 74 / Tier D.
- Any civilian death caps the score at 59 / Tier F.
- Any critical ROE violation applies the configured ROE critical score cap, currently 59 / Tier F.
- Suspects earn custody credit only when restrained, searched, or transferred into custody.
- Suspect evidence credit comes from actually secured weapons and reportable items discovered by an actual search.
- Pressure Point target time is 10 minutes; time credit reaches zero at 20 minutes.

## Installation

1. Commit or back up the confirmed Milestone 7A project.
2. Exit Play Mode and close Unity before replacing files.
3. Extract the Milestone 7B ZIP into the Unity project root and replace matching files.
4. Reopen the project in Unity `6000.5.2f1` and wait for compilation to finish.
5. If the Console reports an error, stop and send the first complete error before running setup.
6. Run `Tools > Rules of Entry > Milestone 7B > Build Automatic After-Action Tier System` outside Play Mode.
7. Run `Tools > Rules of Entry > Milestone 7B > Validate Automatic After-Action Tier System` manually.
8. Open Test Runner and run all EditMode tests.
9. Keep the Console clear, enter Play Mode, and perform the live checklist below.

Do not rerun an older scene generator after Milestone 7B unless rebuilding the entire chain in order. If an older setup replaces the mission scene, rerun Milestone 7A and then Milestone 7B.

## Expected behavior

- The mission remains Active while any required objective is pending.
- The mission remains Active while any authored tactical room is not verified Clear or contains an active threat.
- Once both conditions are satisfied continuously for three seconds, the controller captures final evidence exactly once and enters After Action.
- Player gameplay input is disabled after evidence lock and the final report appears over the live mission view.
- The report shows operation name, tier, score, score cap, all seven category results, incident metrics, objective outcomes, ROE findings, and elapsed time.
- Evidence is optional for automatic completion but missing recoverable suspect evidence loses Evidence points.
- Holding the legacy debrief interaction can still end an unresolved operation; every pending objective is then marked Failed.

## Live test checklist

- [ ] Unity compiles with zero errors.
- [ ] Milestone 7B validator reports zero errors.
- [ ] All EditMode tests pass, including `AfterActionEvaluatorTests` and `MissionCompletionRulesTests`.
- [ ] Begin Pressure Point and confirm no final report appears during an active unresolved operation.
- [ ] Restrain the suspect but leave at least one interior room uncleared; confirm the mission remains Active.
- [ ] Clear all six authored rooms and complete every required objective.
- [ ] Confirm the final report appears automatically after the stable three-second all-clear.
- [ ] Confirm the player can no longer move, fire, reload, or issue commands after final evidence lock.
- [ ] Confirm the report has seven categories and the displayed category maximums total 100.
- [ ] Search the restrained suspect and secure the weapon before the final room clears; confirm Evidence reports all available items secured.
- [ ] Repeat without searching/weapon recovery; confirm Evidence loses points without blocking completion.
- [ ] Confirm a restrained suspect counts as arrested and a free/deceased suspect does not.
- [ ] Confirm a stable civilian counts as saved.
- [ ] Manually end one run before a required objective completes; confirm it becomes Failed and the tier cannot exceed D.
- [ ] Trigger or use automated evidence for an officer death; confirm the score cap is 74 and the tier cannot exceed D.
- [ ] Trigger or use automated evidence for a civilian death; confirm the score cap is 59 and the tier is F.
- [ ] Trigger a critical ROE violation; confirm the configured critical cap applies and the finding appears.
- [ ] Run the Milestone 7A validator and prior regression checks.
- [ ] Confirm the Console remains clean.

## Common problems

### The mission does not end automatically

At least one required objective is still Pending or one `TacticalRoomVolume` is not `Clear`. Walk the complete Pressure Point interior and resolve every active threat. Automatic completion deliberately checks all authored tactical rooms, not only the pump-hall objective.

### The report appears before evidence is collected

Evidence does not secretly block mission completion. Search and secure the suspect before clearing the last room, or use manual scene sequencing during testing. Missing evidence is recorded as a score loss.

### The report is invisible

Exit Play Mode and rerun the Milestone 7B setup. Confirm `ROE_AfterActionReport` exists as an active scene root and the Milestone 7B validator passes.

### A category result looks wrong

The evaluator reads the final immutable snapshot. Check the actor's actual `ActorCondition`, `CustodyState`, inventory search/weapon state, the force ledger, and elapsed mission time. UI text is not scoring authority.

### The setup says the mission controller is missing

Rebuild and validate Milestone 7A first, then rerun Milestone 7B.

## Regression risks

- Older scene setup tools can replace the saved mission controller or remove the final presentation instance.
- A room volume that never reaches Clear will correctly prevent automatic completion.
- Changing actor inventory configuration changes evidence opportunities and must be intentional.
- Changing objective failure deductions or ROE policy deductions changes final scoring and requires updated tests.
- Disabling the final presentation component will not change scoring, but it will hide the user-facing result.

Do not mark Milestone 7B complete until compilation, validator, tests, live auto-completion, factual score verification, prior regressions, clean Console, commit, and push all pass.
