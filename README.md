# Rules of Entry

Unity `6000.5.2f1` HDRP tactical law-enforcement simulation.

## Current delivery

Milestone 3 is complete and validated. It adds deterministic human behavior and a complete non-lethal custody path to the stable first-person and manual-firearm foundations.

Delivered systems:

- suspect and civilian identity, condition, inventory, perception, stress, morale, and behavior state;
- replayable decision rolls tied to incident seed and actor identity;
- inspectable decision reasons and immutable decision records;
- verbal police commands through the current Input System;
- surrender, deceptive surrender, panic, freeze, hide, flee, resist, and threat responses;
- procedural kneel, restraint, search, and custody interactions;
- immutable custody history with full Unity 6 `EntityId` values;
- region-aware prototype ballistic injury response;
- pre-impact actor facts captured in firearm force-event records;
- AI Navigation `2.0.14`, generated NavMesh data, graybox actors, diagnostics, validation, build gate, and tests.

## Controls added in Milestone 3

- Keyboard/mouse: `F` — issue “Police! Show me your hands!”
- Gamepad: left shoulder — issue the same command
- Existing `E`/gamepad west interaction performs the custody step shown in the prompt.

Exact physiological state, decision scores, deception, and ammunition remain simulation/debug data. They are not final player HUD information.

Live validation passed on 2026-07-16: clean compilation and validator, all EditMode and PlayMode tests, seeded behavior/custody smoke test, earlier milestone regressions, and clean Play Mode Console.
