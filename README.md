# Rules of Entry

Rules of Entry is a hyper-realistic first-person tactical law-enforcement game built with Unity.

## Project facts

- Repository: `jaychase464-maker/RoE-v3`
- Unity: `6000.5.2f1`
- HDRP: `17.5.0`
- Input System: `1.19.0`; New Input System only
- uGUI: `2.5.0`
- Test Framework: `1.7.0`
- Milestones 0 and 1: complete and stable
- Milestone 2: integration candidate awaiting Unity validation

## Milestone 2 realism foundation

- per-magazine physical ammunition state and separate chamber state;
- no numerical ammunition HUD and no automatic reload;
- manual qualitative magazine checks;
- retained and emergency reloads with ordered pouch behavior;
- Safe/Semi selector, low ready, shouldered and aimed presentation;
- bolt lock and manual action cycling with live-round ejection;
- one discharge per trigger press;
- physical muzzle obstruction and ballistic hit facts;
- immutable exactly-once force-event records without scoring or ROE judgment;
- repeatable graybox weapon/target-range setup, validation, and tests.

See `MILESTONE_2_INSTALL.md` for the complete realism contract, controls, setup, tests, limitations, and model requirements.

## Guardrails

- Treat this repository as the source of truth.
- Do not silently upgrade Unity or packages.
- Never use `UnityEngine.Input`.
- Never expose authoritative exact ammunition state through player-facing UI.
- Never add automatic reload behavior.
- Combat emits facts; mission/ROE systems evaluate them later.
- Do not call Milestone 2 stable until Unity compilation, validation, all tests, the Milestone 1 regression test, and the Milestone 2 manual test pass.
