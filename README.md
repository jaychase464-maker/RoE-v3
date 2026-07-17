# Rules of Entry

Unity `6000.5.2f1` HDRP tactical law-enforcement simulation.

## Current delivery

Milestone 4 is an integration candidate. It adds a two-officer tactical command foundation to the validated first-person, manual-firearm, human-behavior, and custody systems.

Added in this candidate:

- distinct Officer Alpha and Officer Bravo identities, injury state, navigation, and command history;
- individual/team selection and move, hold, follow, stack, open, restrain, and cancel commands;
- immutable command facts plus a separate, testable lifecycle state machine;
- pending, accepted, executing, completed, cancelled, failed, and refused states;
- explicit failure causes for invalid targets, incapacity, missing NavMesh, incomplete paths, non-compliance, timeout, and supersession;
- physical NavMesh execution with no officer teleportation;
- multi-step assisted restraint that preserves surrender, kneeling, timed cuffing, and verified restraint;
- automatic line-of-sight suspect challenges;
- conservative room-clear verification requiring two actionable officers and a stable no-threat interval;
- automatic controlled custody with partner-cover monitoring and explicit abort reasons;
- separate immutable origin and history for player commands and officer initiative;
- world command marker, prototype diagnostics, validator, build gate, and automated tests.

Run `Tools > Rules of Entry > Milestone 4 > Build Officer Team Prototype`, then follow `MILESTONE_4_INSTALL.md`. Live Unity validation is still required before this milestone can be called complete.
