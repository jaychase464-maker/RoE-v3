# Rules of Entry

Unity `6000.5.2f1` HDRP tactical law-enforcement simulation.

## Current delivery

Milestone 5 is an integration candidate built on the validated Milestone 4 officer-command and bounded-initiative prototype.

This delivery adds:

- ScriptableObject mission definitions and rules-of-engagement policy;
- secure-suspect, protect-civilian, verify-room, and preserve-team objectives;
- immutable evidence snapshots copied from existing factual ledgers;
- pure objective, ROE, and after-action evaluators;
- event-by-event `WithinPolicy`, `ReviewRequired`, and `Violation` findings;
- transparent deductions, rating caps, and reasoned report text;
- provisional mission diagnostics, automatic finalization, and an in-world manual debrief console;
- setup, saved-scene validation, build gate, and automated tests.

Run `Tools > Rules of Entry > Milestone 5 > Build Mission and After-Action Prototype`, then follow `MILESTONE_5_INSTALL.md`. Live Unity validation is required before Milestone 5 can be called complete.

Ending the operation early at the debrief console converts every unresolved required objective into a documented failure.
