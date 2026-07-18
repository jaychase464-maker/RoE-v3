# Milestone 7C Installation — Operation Closure and Headquarters Return

## Goal

Milestone 7C completes the resolved-operation loop. After Milestone 7B locks the final evidence and displays the factual tier report, the player can continue back to headquarters. The exact immutable report and stable deployment identifiers survive the scene transition, open automatically in the PD, and remain available from a physical archive terminal for the rest of the application session.

No new Unity package, project setting, input action, Inspector assignment, or 3D model is required.

## Files created

- `Assets/_Project/RulesOfEntry/Runtime/Operations/CompletedOperationContext.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Operations/CompletedOperationContextRuntimeReset.cs`
- `Assets/_Project/RulesOfEntry/Runtime/UI/Headquarters/HeadquartersAfterActionReviewController.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Headquarters/HeadquartersAfterActionTerminalInteractable.cs`
- `Assets/_Project/RulesOfEntry/Editor/Milestone7C/RulesOfEntryMilestoneSevenCSetup.cs`
- `Assets/_Project/RulesOfEntry/Editor/Milestone7C/RulesOfEntryMilestoneSevenCValidator.cs`
- `Assets/_Project/RulesOfEntry/Tests/EditMode/CompletedOperationContextTests.cs`
- `MILESTONE_7C_INSTALL.md`

All matching `.meta` files are included.

## Files replaced or extended

- `MissionAfterActionPresentation.cs` adds the Continue interaction, safe record capture, load progress, and headquarters scene transition.
- `RulesOfEntryMilestoneSevenBSetup.cs` regenerates the final-report prefab with its Continue button and Input System UI support.
- `ProjectInfo.cs` identifies the current Milestone 7C boundary.
- Project documentation records the return flow, ownership rules, tests, and known boundaries.

The setup tool generates or replaces:

- `Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_AfterActionReport.prefab`
- the `ROE_AfterActionReport` instance in `ROE_Prototype.unity`
- the `[Milestone7C_OperationClosure]` root in `ROE_Headquarters.unity`
- the headquarters latest-report review canvas and After-Action Archive terminal

## Authority and persistence contract

- `AfterActionReport` remains the immutable scoring authority produced by Milestone 7B.
- `CompletedOperationRecord` stores that report plus stable operation, entry, assigned-officer, and support IDs.
- No GameObject, Component, Transform, ScriptableObject, Camera, or other scene reference crosses the transition.
- The headquarters review renders the stored report. It does not rerun or modify scoring.
- `OperationDeploymentContext` is cleared only after the completed record is captured successfully.
- The archive retains the latest report only while the application is running. It is not a campaign save, career record, or multi-operation history.

## Installation

1. Commit or back up the confirmed Milestone 7B project.
2. Exit Play Mode and close Unity before replacing files.
3. Extract the Milestone 7C ZIP into the Unity project root and replace matching files.
4. Reopen the project in Unity `6000.5.2f1` and wait for compilation to finish.
5. If the Console reports an error, stop and send the first complete error before running setup.
6. Run `Tools > Rules of Entry > Milestone 7C > Build Operation Closure and Headquarters Return` outside Play Mode.
7. Run `Tools > Rules of Entry > Milestone 7C > Validate Operation Closure and Headquarters Return` manually.
8. Open Test Runner and run all EditMode tests, including `CompletedOperationContextTests`.
9. Keep the Console clear and perform the live checklist below.

Do not rerun an older scene generator after Milestone 7C unless rebuilding the dependency chain in order. If an older setup replaces either saved scene, rerun Milestone 7B and then Milestone 7C.

## Inspector and scene setup

No manual Inspector assignment is required. The setup tool wires:

- the operation's mission controller, Tactical Player Input, cursor controller, report fields, Continue button, and Input System UI module;
- the headquarters player input and cursor controller;
- the automatic latest-report canvas;
- the archive terminal's reference to that review controller.

The archive terminal is generated beside the existing Operations area at approximately `(3.6, 0.72, 1.3)`. It is gameplay greybox presentation and can move when the final PD environment is authored.

## Expected behavior

- Automatic mission completion and grading behave exactly as in confirmed Milestone 7B.
- The final report unlocks the cursor and accepts mouse selection.
- Clicking Continue, pressing Enter/numpad Enter, or pressing gamepad South/A captures the record once and starts the headquarters load.
- The status line shows return progress; repeated input cannot start a second transition.
- Headquarters loads and automatically presents the same mission name, tier, score, category breakdown, incident metrics, objectives, and ROE findings.
- Closing the headquarters review with its button, Tab, Escape, or gamepad East/B hides the report, locks the cursor, and restores first-person gameplay input.
- Looking at the After-Action Archive terminal and pressing `E` reopens the latest report.
- Starting another operation receives fresh deployment state; the completed report remains available until another report replaces it or the application session ends.

## Live test checklist

- [ ] Unity compiles with zero errors.
- [ ] Milestone 7C validator reports zero errors.
- [ ] All EditMode tests pass, including all Milestone 7B tests and `CompletedOperationContextTests`.
- [ ] Enter headquarters through Operations, select Pressure Point, assign officers, choose an entry, and deploy.
- [ ] Resolve the operation and confirm the final report still matches every factual Milestone 7B result.
- [ ] Click Continue with the mouse and confirm one headquarters load begins.
- [ ] Repeat separate runs using Enter and gamepad South/A.
- [ ] Confirm headquarters automatically opens the same mission name, tier, score, category values, metrics, objectives, and policy findings.
- [ ] Close the report with the button, Tab, Escape, and gamepad East/B on separate checks.
- [ ] Confirm player look/movement is disabled while review is open and restored after it closes.
- [ ] Walk to the After-Action Archive terminal and press `E`; confirm the latest report reopens.
- [ ] Close the archive, open the normal mission tablet, and confirm a new deployment can be configured normally.
- [ ] Confirm no stale entry or officer selection is silently reused unless the player selects it again.
- [ ] Run the Milestone 7B validator and prior regression checks.
- [ ] Confirm the Console remains clean through two full operation-return loops.

## Common problems

### Continue does not respond to the mouse

Exit Play Mode and rerun Milestone 7C setup. Confirm the operation scene has an enabled `EventSystem` with `InputSystemUIInputModule`, and run the validator. The generated report CanvasGroup must be interactable while visible.

### Continue says headquarters is not in Build Settings

Run Milestone 6A setup, then Milestone 7C setup. `ROE_Headquarters.unity` and `ROE_Prototype.unity` must both be enabled build scenes; the expected build order remains Front End, Headquarters, Prototype.

### Headquarters loads but no report opens

Confirm the final report—not a provisional diagnostic report—was visible before Continue. Run Milestone 7C setup and validator, then verify `[Milestone7C_OperationClosure]` exists in the saved headquarters scene.

### The archive terminal says no operation is recorded

The archive is intentionally session-only. Entering headquarters directly after an editor domain reload, restarting Play Mode, or restarting the application produces no completed record. Complete an operation and return through the final report first.

### Player control does not return after closing the review

Verify the generated headquarters review references the scene's `CursorStateController` and `TacticalPlayerInput`. Rerun Milestone 6A setup, then Milestone 7C setup and its validator.

### The report metadata says DIRECT

That indicates the operation was launched directly rather than through headquarters planning. The factual report is still valid; entry/team metadata exists only when `OperationDeploymentContext` was confirmed before deployment.

## Regression risks

- An older Milestone 7B setup can replace the report prefab/scene instance without the latest Milestone 7C headquarters wiring; rerun Milestone 7C afterward.
- An older Milestone 6A setup can replace the headquarters scene and remove the archive root; rerun Milestone 7C afterward.
- Clearing `CompletedOperationContext` during a scene load will intentionally erase the latest session report and must not be added to generic scene cleanup.
- Campaign persistence must serialize a dedicated versioned DTO in a future milestone; it must not serialize Unity scene references or make UI a score authority.

Do not mark Milestone 7C complete until compilation, validator, tests, the full operation-return-review loop, prior regressions, clean Console, commit, and push all pass.
