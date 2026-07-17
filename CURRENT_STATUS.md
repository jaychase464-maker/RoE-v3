# Current Status

## Active milestone

**Milestone 2 — Weapon and Force-Event Foundation: integration candidate**

Milestones 0 and 1 remain complete and stable. Milestone 2 implementation, setup tooling, validation, tests, and documentation are ready for live Unity integration.

## Delivered for Milestone 2 validation

- Physical per-magazine round state, chamber state, bolt state, and ordered spare-magazine pouches.
- Safe/Semi selector and low-ready/shouldered/aimed handling.
- Manual retained reload, manual emergency reload, and manual action cycling.
- No automatic reload and no player-facing exact ammunition count.
- Timed physical magazine check with qualitative estimates.
- Semi-automatic one-shot-per-press behavior.
- Physical muzzle-origin raycast that respects near-cover obstruction.
- Ballistic hit contract and prototype reactive targets.
- Immutable append-only firearm-discharge records containing no score or ROE judgment.
- Graybox carbine, target range, status UI, editor builder, validator, build gate, and tests.

## Required validation

- zero Unity compiler errors;
- zero Milestone 2 validation errors;
- all Edit Mode and Play Mode tests pass;
- complete Milestone 1 regression smoke test passes;
- complete Milestone 2 manual realism test passes;
- Play Mode Console remains free of errors and exceptions.

## Honesty boundary

The current ballistic prototype is an instantaneous physical-muzzle raycast. It validates muzzle obstruction and immutable hit facts, but does not yet simulate flight time, gravity, aerodynamic drag, penetration, fragmentation, or ricochet.

## Art requirement

No external model is required for this integration. Use the generated primitive carbine until the mechanical contract passes live validation.
