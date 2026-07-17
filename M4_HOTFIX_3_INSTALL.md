# Milestone 4 Hotfix 3 — Physical Door Navigation

## Fix

The Milestone 3 NavMesh was baked while the training door was closed. That correctly left the two sides of the divider disconnected, but rotating the physical door at runtime did not rebuild or reconnect the baked navigation topology. Officers could open the door yet still receive no complete path through the doorway.

Hotfix 3 adds a fixed, bidirectional `NavMeshLink` across the training doorway and gates it with the actual door-leaf clearance:

- the link is blocked while the door is closed or still swinging;
- the link becomes available only after the door is at least 80 percent open;
- officers wait for physical clearance before completing an open-door order;
- officer agents traverse the link normally, without teleporting or using `NavMeshAgent.Warp`;
- the setup tool recreates and wires the link automatically;
- the validator inspects the saved scene link and its door reference;
- a PlayMode test verifies closed, opening, open, and closing states.

Hotfix 3 includes the Hotfix 1 input-persistence and Hotfix 2 saved-reference changes.

## Install

1. Exit Play Mode and close Unity.
2. Extract `RulesOfEntry-Milestone-4-Hotfix-3.zip` directly into `C:\Users\Shadow\Videos\RoE v3`.
3. Merge folders and replace files.
4. Reopen Unity and wait for compilation.
5. Run `Tools > Rules of Entry > Milestone 4 > Build Officer Team Prototype` again. This step is required because it creates the doorway link in the scene.
6. Confirm the Milestone 4 validator passes.
7. Run the PlayMode tests.

No Inspector assignments or new packages are required. The existing AI Navigation package is used.

## Test

1. Enter Play Mode and select the Team with `3`.
2. Aim at the training door and press `U`.
3. Confirm the officers approach, one opens the door, and the order waits until the leaf clears the threshold.
4. Aim at the floor beyond the open doorway and press `G`.
5. Confirm both officers walk through the opening. They may pass single-file due to local avoidance.
6. Close the door with the normal interaction and confirm a new path cannot cross the closed threshold.
7. Confirm the Console remains free of errors.

## Expected validation

The Console should include this passing check:

`M4 Door Traversal: A fixed bidirectional NavMesh link opens only after the physical door clears the threshold.`

If an officer still refuses the move, read the right-side command details and send the first `NoPath`, `NoNavigationSurface`, or other Console error exactly as shown.
