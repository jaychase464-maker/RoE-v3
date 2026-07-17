# Milestone 2 Installation and Validation

This package installs the **Milestone 2 — Weapon and Force-Event Foundation** integration candidate for Rules of Entry. It requires the stable Milestone 1 project and Unity `6000.5.2f1`.

## Realism contract

- There is no player-facing numerical ammunition counter.
- Every magazine stores its own authoritative round count; ammunition is never a pooled reserve number.
- The chamber is separate from the inserted magazine.
- The weapon starts on Safe and at low ready.
- Semi-automatic fire produces one discharge per trigger press.
- An empty weapon never reloads automatically.
- `R` performs a deliberate retained reload and puts the removed magazine at the back of the pouch order.
- `Left Alt + R` performs an emergency reload and discards the removed magazine.
- A partial magazine can return later because reload selection follows physical pouch order, not hidden round count.
- A manual magazine check gives only a physical estimate: Full, Mostly Full, Partial, Low, Nearly Empty, or Empty.
- Cycling a loaded action ejects and permanently loses one live round.
- An empty closed chamber requires a manual action cycle.
- Bolt-lock reloads release the bolt and chamber from the new magazine as part of the reload.
- Every actual projectile discharge produces exactly one immutable force-event fact. Dry trigger presses do not.
- Combat code records facts and never changes score or decides whether force was justified.

## Files

Complete replacements:

- `Assets/_Project/RulesOfEntry/Input/ROE_InputActions.inputactions`
- `Assets/_Project/RulesOfEntry/Runtime/Input/TacticalPlayerInput.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Player/FirstPersonLook.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Player/FirstPersonMotor.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Core/ProjectInfo.cs`

New runtime files:

- `Runtime/Combat/FirearmTypes.cs`
- `Runtime/Combat/MagazineState.cs`
- `Runtime/Combat/FirearmStateMachine.cs`
- `Runtime/Combat/FirearmDefinition.cs`
- `Runtime/Combat/AmmunitionDefinition.cs`
- `Runtime/Combat/FirearmController.cs`
- `Runtime/Combat/FirearmView.cs`
- `Runtime/Combat/BallisticHit.cs`
- `Runtime/Combat/PrototypeBallisticTarget.cs`
- `Runtime/Combat/ForceEventRecord.cs`
- `Runtime/Combat/UseOfForceEventLedger.cs`
- `Runtime/UI/WeaponStatusUI.cs`

New editor and test files are under `Editor/Milestone2`, `Tests/EditMode`, and `Tests/PlayMode`.

## Packages and settings

- No package installation or `Packages/manifest.json` change is required.
- No assembly-definition change is required.
- No new project layer is required.
- The existing Input System, uGUI, HDRP, and Test Framework packages remain authoritative.

## Import

1. Confirm `git status` is clean and close Unity.
2. Extract `RulesOfEntry-Milestone-2.zip` directly into `C:\Users\Shadow\Videos\RoE v3`.
3. Reopen the project using Unity `6000.5.2f1`.
4. Wait for import and script compilation to finish.
5. Confirm the Console has no red compiler errors.
6. Run **Tools > Rules of Entry > Milestone 2 > Build Weapon Prototype**.
7. Read the validation dialog and Console output.

The setup tool automatically creates:

- patrol-carbine and 5.56 mm prototype definition assets;
- a primitive first-person carbine rig with a physical muzzle origin;
- firearm, force-ledger, recoil, and input wiring on `ROE_Player.prefab`;
- ballistic-target and weapon-status UI prefabs;
- an indoor three-target backstop in `ROE_Prototype.unity`;
- HDRP graybox weapon, accent, target, and range materials.

No manual Inspector assignment is required. Re-running the Milestone 2 tool replaces only Milestone 2 generated scene content and weapon rig.

## Controls

| Action | Keyboard and mouse | Gamepad |
|---|---|---|
| Aim/raise from low ready | Hold right mouse | Hold left trigger |
| Fire one semi-automatic shot | Left mouse | Right trigger |
| Retained reload | `R` | South/lower face button |
| Emergency reload | Hold Left Alt + `R` | Hold right shoulder + South button |
| Check inserted magazine | `T` | D-pad Down |
| Toggle low ready/shouldered | `V` | D-pad Up |
| Cycle Safe/Semi selector | `B` | D-pad Left |
| Cycle the action | `X` | North/upper face button |

Milestone 1 movement, interaction, and cursor controls remain unchanged.

## Expected starting state

- Selector: Safe.
- Posture: Low Ready.
- Chamber: one live round.
- Inserted magazine: 29 rounds internally.
- Spare magazines: three physical 30-round magazines.
- Player-visible round count: none.

The inserted magazine correctly estimates as Full because a person cannot reliably distinguish one missing cartridge by weight.

## Manual realism test

1. Open `ROE_Prototype.unity`, clear the Console, and enter Play Mode.
2. Pull the trigger while Safe; verify no discharge occurs and `SAFE` appears.
3. Press `B` to select Semi.
4. Pull the trigger while at low ready without aiming; verify no discharge occurs.
5. Hold Aim or press `V` to shoulder, open the Milestone 1 training door, and fire at a target.
6. Verify one target response and one recoil impulse occur per trigger press; holding the mouse button must not produce automatic fire.
7. Fire several rounds. Verify no number anywhere tells you the remaining ammunition.
8. Press `T`; wait for the check animation and verify only a qualitative estimate appears.
9. Press `R`; verify a retained reload completes and does not report a dropped magazine.
10. Fire additional rounds, then hold Left Alt and press `R`; verify the emergency reload reports that the magazine was dropped.
11. Continue firing until the bolt locks. Verify the weapon does not reload itself.
12. Press `R` manually; verify the bolt-release completion message appears.
13. Press `X` while a round is chambered; verify the UI reports a live round was ejected.
14. Stand close to cover and fire with the muzzle obstructed while the view can see past it; verify the nearby cover receives the shot rather than the distant target.
15. Exit Play Mode and confirm no errors or exceptions were logged.

## Automated validation

1. Run **Tools > Rules of Entry > Milestone 2 > Validate Weapon Prototype**.
2. Run all Edit Mode tests.
3. Run all Play Mode tests.
4. Repeat the complete Milestone 1 smoke test.

Expected: zero errors and the existing `DefaultCompany` warning only.

## Common problems

### Weapon does not fire

The weapon starts Safe and at low ready. Press `B` to select Semi, then aim or press `V` to shoulder it. A locked bolt, empty chamber, or active reload/check also blocks firing.

### Reload appears not to chamber

If the chamber was empty but the bolt was closed, inserting a magazine does not magically chamber a cartridge. Press `X` to cycle the action. A bolt-locked reload chambers through the bolt release automatically.

### No exact ammunition number appears

That is required behavior. Press `T` to physically check the inserted magazine and receive a temporary qualitative estimate.

### Target behind cover is not hit

Shots originate from the weapon muzzle, not the center of the screen. The muzzle can be obstructed even when the camera can see the target.

### Milestone 1 was rebuilt afterward

The Milestone 1 builder recreates its player prefab. Re-run the Milestone 2 builder to restore weapon components while preserving the Milestone 1 baseline.

## Current ballistic boundary

This milestone uses an instantaneous physical-muzzle raycast with correct near-cover obstruction and factual hit capture. Projectile time of flight, gravity, aerodynamic drag, material penetration, fragmentation, and ricochet are not falsely claimed. They require a dedicated external-ballistics/material pass and real distance/material validation before the vertical slice.

## 3D model requirement

No external model is required for this integration; the tool creates a primitive carbine. After the mechanical state passes validation, the first production-art request will be a correctly scaled first-person patrol carbine with separately movable magazine, selector, charging handle, bolt/bolt carrier, bolt catch, trigger, stock, sights/optic, and muzzle, plus rigged first-person arms with finger bones. Do not purchase or import one yet.
