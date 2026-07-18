# Milestone 7A Installation — Pressure Point Mission Greybox

## Goal

Replace the single training-room operation space with the first real multi-room mission foundation while preserving the existing player, firearm, suspect/civilian, custody, officer, ROE, after-action, headquarters, deployment, Tactical HUD, and in-mission tablet systems.

Milestone 7A authors Operation Pressure Point as a compact municipal pumping annex with three approaches, six interior clearance spaces, door-gated navigation, and controlled incident-location variation.

## Files

### New runtime folder

`Assets/_Project/RulesOfEntry/Runtime/Operations/`

- `OperationTopologyTypes.cs`
- `OperationTopologyRules.cs`
- `OperationRoomNode.cs`
- `OperationPortal.cs`
- `OperationTopology.cs`
- `OperationSpawnPoint.cs`
- `OperationScenarioDirector.cs`

### New editor folder

`Assets/_Project/RulesOfEntry/Editor/Milestone7A/`

- `RulesOfEntryMilestoneSevenASetup.cs`
- `RulesOfEntryMilestoneSevenAValidator.cs`

### New test

`Assets/_Project/RulesOfEntry/Tests/EditMode/MilestoneSevenAOperationTests.cs`

### Replaced or updated files

- `Assets/_Project/RulesOfEntry/Editor/Milestone4/RulesOfEntryMilestoneFourValidator.cs`
- `Assets/_Project/RulesOfEntry/Editor/Milestone5/RulesOfEntryMilestoneFiveValidator.cs`
- `Assets/_Project/RulesOfEntry/Runtime/Core/ProjectInfo.cs`
- root project documentation files

The setup tool generates or updates scene/material/navigation assets inside Unity. Do not hand-create those generated assets before running the tool.

## Required packages and settings

- Unity `6000.5.2f1`
- HDRP `17.5.0`
- Input System `1.19.0`; Active Input Handling must remain **Input System Package (New)**
- AI Navigation `2.0.14`
- `Interactable` and `Player` layers created by Milestone 1
- Milestones 1–6C installed and compiling

No new package, tag, layer, assembly reference, Input Action, or 3D model is required.

## Installation order

1. Back up or checkpoint the repository.
2. Copy the Milestone 7A package into the Unity project root and replace matching files.
3. Open the project in Unity `6000.5.2f1`.
4. Wait until script compilation and asset import finish.
5. Confirm the Console has no compiler errors.
6. Confirm Milestone 6C has already been built and its validator passes.
7. Exit Play Mode.
8. Run `Tools > Rules of Entry > Milestone 7A > Build Pressure Point Mission Greybox`.
9. Allow the setup tool to save the prototype scene and bake navigation.
10. Run `Tools > Rules of Entry > Milestone 7A > Validate Pressure Point Mission Greybox` again manually.
11. Open Test Runner and run every EditMode test.

Do not rerun an older scene-generation milestone after Milestone 7A unless rebuilding the full chain in order. Older generators can recreate training-room geometry or temporary entry positions; if that happens, rerun Milestone 6C and then Milestone 7A.

## Generated scene structure

The setup creates `[Milestone7A_PressurePoint]` in `ROE_Prototype.unity` with:

- `Geometry` — pumping-annex shell, exterior apron, room dividers, machinery placeholders, and labels;
- `Topology` — nine stable operation area nodes and six tactical-clearance volumes;
- `Portals` — seven physical door thresholds and two open passages;
- `Scenario` — twelve suspect/civilian placement points;
- `OperationTopology` — entry binding and route authority;
- `OperationScenarioDirector` — role-compatible incident placement;
- `NavMeshSurface` — persisted Pressure Point navigation data.

The original `[Milestone1_Graybox]` and `[Milestone2_Range]` roots remain in the scene but are disabled. The obsolete single training-room clearance volume and its one-door link are removed. Existing player, officers, actors, mission controller, UI, deployment coordinator, body cameras, and tablet references are retained.

## Inspector assignments

None are required when the setup succeeds. The tool assigns:

- all room IDs, types, bounds, and clearance components;
- portal endpoints, doors, and traversal links;
- all three existing `OperationEntryAnchor` poses;
- topology entry bindings;
- incident actors and role-compatible spawn points;
- the persisted `NavMeshData` reference;
- the Pressure Point pump-hall mission objective.

Use the Inspector only to diagnose a validator failure. Do not repair broken serialized references by guessing; rerun the setup after correcting the first reported prerequisite error.

## Expected layout

- South entry: employee-parking approach into Administration Reception.
- West entry: maintenance-yard approach into the Maintenance Bay.
- North entry: pipe-gallery approach into the Pump Hall.
- South interior: Maintenance, Reception, and Administration.
- Center: one long Central Corridor.
- North interior: Pump Hall and Control Room.

All three approaches support the current team and authored formations for up to eight scene officers. Final formations, sectors, and coordinated threshold tactics are later systems.

## Controls

Milestone 7A adds no new player binding.

- `E` — interact with a door or custody subject.
- Hold middle mouse — open the squad-command list.
- `1–6` while middle mouse is held — issue the displayed command.
- `Tab` — open or close the in-mission tablet.
- `Q` / `E`, arrow keys, or gamepad shoulders while on Body Cameras — change the selected officer feed.
- `F10` — development diagnostics.

## Test checklist

### Compilation and automated validation

- [ ] Unity Console has zero compiler errors.
- [ ] Milestone 7A validator reports zero errors.
- [ ] All EditMode tests pass, including `MilestoneSevenAOperationTests`.
- [ ] Milestones 1–6C validators still report zero errors.
- [ ] Console remains clean after entering and exiting Play Mode.

### Headquarters deployment

- [ ] Enter headquarters from the main menu.
- [ ] Select Pressure Point at the physical terminal.
- [ ] Choose South Administration, deploy, and confirm the player/team appear outside the south door.
- [ ] Repeat for West Service Yard.
- [ ] Repeat for North Pipe Gallery.
- [ ] Repeat at least one route with one assigned officer and one with both current officers.
- [ ] No selected officer is inactive, under the floor, inside a wall, or off NavMesh.

### Navigation and doors

- [ ] Open and cross all three exterior doors.
- [ ] Open and cross Maintenance–Corridor, Administration–Corridor, Corridor–Control, and Pump–Control doors.
- [ ] Order the full team across each door and back again.
- [ ] Officers wait for physical door clearance instead of walking through a closed leaf.
- [ ] Reception–Corridor and Corridor–Pump Hall behave as open passages.
- [ ] No officer reports an incomplete path while standing on a valid route.

### Incident variation and evidence

- [ ] Start Play Mode at least five times.
- [ ] The Console logs a nonzero incident seed and the chosen actor locations.
- [ ] Suspect and civilian appear only at role-compatible authored points.
- [ ] Actor IDs remain `m3_suspect_01` and `m3_civilian_01`.
- [ ] The mission objective displays `Verify the pump hall clear`.
- [ ] Enter each interior with the required team presence and observe timed clearance.
- [ ] An active suspect prevents or revokes clear status in the room containing them.
- [ ] Securing the suspect and deliberately checking the Pump Hall completes the room objective.

### Protected gameplay regressions

- [ ] Manual firearm reload and magazine checking remain unchanged; no automatic reload or exact ammunition counter appears.
- [ ] Officers accept MMB commands and cross thresholds.
- [ ] Officers repeatedly challenge actionable suspects until behavior changes.
- [ ] Officers perform bounded automatic custody only after the established room/cover rules permit it.
- [ ] Suspect surrender, deception, flight, restraint, search, and injury states still function.
- [ ] ROE findings and after-action scoring consume the same factual ledgers.
- [ ] `Tab` opens the operational tablet and live officer body-camera feeds continue working.
- [ ] AI and mission time continue while the tablet is raised.

## Common problems

### Setup says Milestones 1–6C are incomplete

Run the first failing earlier validator. Most commonly, rerun the Milestone 6C setup so the operation scene contains three valid entry anchors, the deployment coordinator, officer body cameras, and the in-mission tablet. Then rerun Milestone 7A.

### AI Navigation does not produce data

Confirm `com.unity.ai.navigation` version `2.0.14` is installed, exit Play Mode, close Prefab Mode, and rerun the setup. Do not manually add a second `NavMeshSurface`.

### An entry or officer spawn fails validation

Do not move individual officer spawn children. Rerun the Milestone 7A setup so the entire authored entry formation is restored and the NavMesh is rebuilt.

### Officers stop at an open door

Confirm the door leaf reaches its open target and the portal's `DoorTraversalLink` becomes active. If the physical leaf moves but the link stays inactive, rerun the setup and report the first Console message mentioning `Door Navigation`.

### Suspect or civilian cannot be placed

Confirm the M3 actor prefabs and identities still exist. The setup requires at least one suspect and one civilian plus enough matching `OperationSpawnPoint` records. Rerun Milestone 3 only as part of a full ordered rebuild; otherwise it can restore obsolete training geometry.

### An older validator asks for the north training room

The Milestone 4 and 5 validators must be replaced by this package. Milestone 5 now reads the target room ID from the mission definition, and Milestone 4 accepts any configured authored room containing the suspect.

## 3D-model boundary

No model is needed now. After this greybox passes and the layout is locked, the environment request will be:

- modular municipal/industrial exterior and interior shell pieces;
- separate functional doors, frames, windows, and shutters;
- fencing, gates, bollards, service-yard surfaces, drains, and utility clutter;
- pumps, pipes, valves, electrical cabinets, control consoles, safety rails, and catwalk-compatible pieces;
- administration desks, maintenance benches, storage, lockers, signage, and evidence-scale props;
- clean collision meshes, LODs, controlled material slots, metric scale, and HDRP PBR textures.

Do not buy a map that is only a sealed visual mesh. Doors, rooms, approaches, and major cover must remain separately authorable for navigation, interaction, damage, and mission variation.
