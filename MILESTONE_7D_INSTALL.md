# Milestone 7D Installation — Campaign Saves and Operations Archive

## Goal

Milestone 7D turns the existing campaign placeholders into a real local campaign foundation. The player creates a sworn-officer identity, Continue Campaign reloads it after a restart, the RoE body-camera HUD uses that identity, and every successfully completed operation is appended to a versioned campaign archive before headquarters loads.

The headquarters After-Action Archive can browse all saved reports without recalculating or changing their scores.

No new Unity package, project setting, input action, Inspector assignment, or 3D model is required.

## Files created

- `Assets/_Project/RulesOfEntry/Runtime/Campaign/CampaignDataRules.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Campaign/CampaignSaveData.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Campaign/CampaignSaveCodec.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Campaign/CampaignSaveService.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Campaign/CampaignSession.cs`
- `Assets/_Project/RulesOfEntry/Runtime/UI/FrontEnd/CampaignFrontEndController.cs`
- `Assets/_Project/RulesOfEntry/Runtime/UI/TacticalHud/CampaignBodyCameraIdentityBinder.cs`
- `Assets/_Project/RulesOfEntry/Editor/Milestone7D/RulesOfEntryMilestoneSevenDSetup.cs`
- `Assets/_Project/RulesOfEntry/Editor/Milestone7D/RulesOfEntryMilestoneSevenDValidator.cs`
- `Assets/_Project/RulesOfEntry/Tests/EditMode/CampaignPersistenceTests.cs`
- `MILESTONE_7D_INSTALL.md`

All matching `.meta` files and the new folder metadata are included.

## Files replaced or extended

- `CompletedOperationContext.cs` adds a stable record ID used to make archive retries idempotent.
- `MissionAfterActionPresentation.cs` persists an active campaign operation before consuming deployment state or returning to headquarters.
- `HeadquartersAfterActionReviewController.cs` reads persisted reports and supports Previous/Next browsing.
- `HeadquartersAfterActionTerminalInteractable.cs` exposes persisted history after an application restart.
- `RulesOfEntryMilestoneSevenCSetup.cs` rebuilds the archive with navigation controls.
- `RulesOfEntryUiPresentationValidator.cs` recognizes the activated campaign menu contract.
- `ProjectInfo.cs` and project documentation identify Milestone 7D.

The setup tool modifies or generates:

- the New Campaign personnel panel inside `ROE_FrontEnd.unity`;
- functional Continue Campaign and New Campaign main-menu entries;
- the reusable player prefab's campaign/body-camera identity binding;
- the player bindings in headquarters and Pressure Point;
- the headquarters After-Action Archive with Previous and Next controls.

## Save contract

- Schema version: `1`.
- Save format: human-readable JSON using Unity `JsonUtility`.
- Active campaign marker: `active_campaign.txt`.
- Primary and `.bak` files are retained; replacement writes use a temporary file.
- Completed reports are appended by unique record ID so retrying a headquarters transition cannot duplicate an operation.
- Force-event sequence and 64-bit shooter identity values are stored as decimal strings to preserve every bit.
- A save created by a newer unsupported schema is rejected instead of guessed or downgraded.
- A campaign save failure blocks the headquarters transition and leaves deployment context available for retry.

On Windows, the default location is:

`%USERPROFILE%\AppData\LocalLow\Trooper Studios\RoE v3\ROE\Campaigns`

The file is local prototype data. It is not encrypted, signed, cloud-synchronized, or suitable for competitive anti-tamper enforcement.

## Current scope boundary

- One campaign is marked active at a time.
- New Campaign creates a separate file and switches the active marker; it does not delete the previous file.
- A profile selector, rename/delete UI, cloud saves, platform accounts, campaign consequences, officer careers, and save migration beyond schema 1 remain future milestones.
- The Operations main-menu entry remains a non-campaign prototype route. Operations completed through that route still return to headquarters but are not written into a campaign file.
- Campaign history stores final facts. It does not own AI, scoring, mission completion, inventory, injuries, or scene objects.

## Installation

1. Commit or back up the confirmed Milestone 7C project.
2. Exit Play Mode and close Unity before replacing files.
3. Extract the Milestone 7D ZIP into the Unity project root and replace matching files.
4. Reopen with Unity `6000.5.2f1` and wait for compilation.
5. If the Console reports an error, stop and send the first complete error before running setup.
6. Run `Tools > Rules of Entry > Milestone 7D > Build Campaign Saves and Operations Archive` outside Play Mode.
7. Run `Tools > Rules of Entry > Milestone 7D > Validate Campaign Saves and Operations Archive` manually.
8. Run all EditMode tests, including `CampaignPersistenceTests` and the Milestone 7B/7C tests.
9. Perform the live checklist below with a clear Console.

If an older UI Presentation, Tactical HUD, Milestone 6A, or Milestone 7C generator is rerun later, rerun Milestone 7D afterward.

## Expected behavior

- New Campaign opens a cinematic personnel record over the existing menu background.
- Personnel title, instructions, identity fields, status, and buttons remain in separate rows without clipping at the 1920×1080 reference resolution.
- Officer full name and badge number are required; unsupported characters are removed and badge letters normalize to uppercase.
- Creating a campaign saves it, activates the identity, and loads headquarters.
- Continue Campaign is disabled when no valid active save exists and enabled when one is available.
- Continue Campaign loads the active identity and enters headquarters without automatically forcing open an old report.
- The upper-right RoE body-camera overlay displays the created officer name, badge, and Calder City Police Department.
- A final operation report is copied to the active campaign exactly once before returning to headquarters.
- The final report owns the top UI layer, fully covering the tactical roster, body-camera block, and command hints while evidence is locked.
- The just-completed report opens automatically after the operation return.
- The archive terminal exposes all completed campaign reports after the review closes and after application restarts.
- Previous/Next buttons, left/right arrows, and controller LB/RB cycle history with wraparound.

## Live test checklist

- [ ] Unity compiles with zero errors.
- [ ] Milestone 7D validator reports zero errors.
- [ ] All EditMode tests pass.
- [ ] With no save, Continue Campaign is disabled and New Campaign is enabled.
- [ ] Open New Campaign, cancel with the button, Escape, and gamepad B; confirm normal menu control returns.
- [ ] Confirm the New Campaign title, instructions, both fields, status line, and buttons do not overlap or clip.
- [ ] Submit blank/invalid identity fields and confirm the campaign is not created.
- [ ] Create `Alex Carter` with badge `A127`; confirm headquarters loads.
- [ ] Deploy into Pressure Point and confirm the body-camera HUD reads `Alex Carter`, `A127`, and Calder City Police Department.
- [ ] Complete Pressure Point and confirm headquarters loads only after the campaign archive write succeeds.
- [ ] On the final report, confirm the tactical roster, body-camera block, and MMB command hint do not render above the report.
- [ ] Confirm the same score, tier, objectives, metrics, ROE findings, operation code, entry, and assigned team appear.
- [ ] Close the report and reopen it at the After-Action Archive terminal.
- [ ] Complete a second operation and confirm the archive reads `02 / 02`.
- [ ] Cycle both directions with buttons, keyboard arrows, and controller shoulders; confirm wraparound and factual reports.
- [ ] Exit Play Mode, start from the front end again, choose Continue Campaign, and confirm the identity and both reports persist.
- [ ] Confirm entering headquarters through Continue does not automatically open an old report; use the archive terminal to view it.
- [ ] Run Operations directly without loading a campaign and confirm the sandbox flow still works without modifying the campaign file.
- [ ] Run Milestone 7C and earlier regression validators.
- [ ] Confirm the Console remains clean through creation, two operations, restart, continue, and archive review.

## Common problems

### Continue Campaign remains disabled

Create a campaign first. If one was already created, inspect the first Console error and confirm `active_campaign.txt` and its referenced JSON file exist under the save location. Do not hand-edit IDs.

### New Campaign appears but typing does not work

Rerun Milestone 7D setup and validator. The front-end EventSystem must use `InputSystemUIInputModule`, and both generated `InputField` references must be saved.

### The body-camera still shows A. Carter

That is the safe prototype fallback when no campaign is active. Enter through New Campaign or Continue Campaign, not the direct Operations/Training route, and rerun Milestone 7D setup if the validator reports a missing identity binder.

### The mission report says campaign save failed

The operation remains on its final report so it can be retried safely. Check write permission and free disk space for `Application.persistentDataPath`, then press Continue again. Do not close the application until the record succeeds if the operation must be retained.

### The primary JSON is damaged

The service attempts the matching `.bak` automatically. It logs a warning when the backup is used. Preserve both files before manual investigation.

### Old reports do not open automatically after Continue Campaign

This is intentional. Automatic review happens only immediately after completing an operation. Existing history is available from the PD archive terminal.

## Regression risks

- Rerunning older scene generators can remove campaign UI or archive navigation.
- Changing public DTO field names breaks schema 1 compatibility and requires an explicit migration.
- UI code must never recalculate saved scores or rewrite casualty/ROE facts.
- Save cleanup tools must never delete a campaign without an explicit player action and confirmation.
- A future profile selector must operate on validated campaign IDs and cannot construct file paths from unrestricted user text.

Do not mark Milestone 7D complete until compilation, validator, tests, campaign creation, two archived operations, application restart, Continue Campaign, body-camera identity, archive browsing, prior regressions, clean Console, commit, and push all pass.
