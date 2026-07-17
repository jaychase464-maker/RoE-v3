# Bugs and Validation Gaps

## Confirmed integration defects — resolved

### ROE-0014 — Force-context validator matched `distancePenalty`

- Priority: blocker for validation
- Status: resolved and validated on 2026-07-16
- Resolution: whole-identifier regular expressions replaced substring matching

### ROE-0013 — Ambiguous Unity PackageInfo blocked Milestone 3 compilation

- Priority: blocker
- Status: resolved and validated on 2026-07-16
- Resolution: explicit alias to `UnityEditor.PackageManager.PackageInfo`

## Open validation gaps and technical debt

### ROE-0021 — Bounded officer initiative requires live Unity validation

- Priority: blocker
- Status: integration candidate; live Unity validation pending
- Environment: Unity `6000.5.2f1`, AI Navigation `2.0.14`
- Scope: automatic visible-suspect challenge, two-officer/timed room-clear verification, automatic controlled custody, cover-loss abort, initiative audit history, and civilian exclusion
- Safety boundary: no free capable suspect may be automatically restrained and no action may bypass physical approach, kneeling, timed cuffing, or state verification
- Validation required: rerun setup and validator; run all tests; exercise refusal, deception, genuine surrender, cover loss, automatic custody, and civilian exclusion; confirm a clean Console

### ROE-0015 — Milestone 4 requires live Unity validation

- Priority: blocker for milestone closure
- Status: open
- Required evidence: clean compilation; M4 setup/validator; all EditMode and PlayMode tests; movement, cancellation, follow, door, refusal, and cooperative restraint smoke tests; Milestones 1–3 regressions; clean Play Mode Console
- Current impact: implementation is an integration candidate and must not be described as complete

### ROE-0016 — Officer tactical movement is individual-path prototype logic

- Priority: high tactical fidelity debt
- Status: open
- Current behavior: complete NavMesh paths, two-point spacing, door approach points, cancellation, timeout, and explicit failures
- Missing: formation authority, room/portal graph, hinge and fatal-funnel analysis, collision negotiation, sector ownership, cover selection, pieing, threshold evaluation, shield handling, and coordinated entry timing

### ROE-0017 — Officer arrest assistance has no production animation or equipment

- Priority: high presentation debt
- Status: open
- Current behavior: authoritative approach, surrender/kneel checks, settle timing, timed cuffing, and verified custody transition
- Missing: motion matching, inverse kinematics, subject/officer alignment, handcuff prop, weapon retention posture, partner cover, resistance struggle, and audio

### ROE-0002 — Default company identity remains configured

- Priority: low
- Status: open
- Resolution condition: approved studio identity is supplied before a distributable build

### ROE-0008 — External ballistics and terminal material response are simplified

- Priority: high realism debt
- Status: open
- Current behavior: instantaneous physical-muzzle raycast, muzzle obstruction, region-aware actor response, and factual hit capture
- Missing: time of flight, gravity, drag, zeroing, penetration, fragmentation, ricochet, and validated residual-energy injury model

### ROE-0011 — Actor physiology is a prototype approximation

- Priority: high realism debt
- Status: open
- Missing: validated wound profiles, armor, organ/vascular structures, pain, shock, airway, respiration, medical interventions, and expert review

### ROE-0012 — Threat behavior has no production weapon or attack execution

- Priority: high
- Status: planned for a later combat-AI slice
- Missing: suspect weapon presentation, draw timing, aim error, cover use, fire discipline, animations, and safe tactical testing

## Fixed or resolved entries

- `ROE-0001`: baseline compiler status recorded.
- `ROE-0003`: project-owned tactical input implemented.
- `ROE-0004`: AI Navigation dependency declared.
- `ROE-0005`: Milestone 0 validated.
- `ROE-0006`: Milestone 1 validated and stable.
- `ROE-0007`: Milestone 2 regression evidence passed.
- `ROE-0009`: obsolete instance IDs replaced with full 64-bit `EntityId` storage.
- `ROE-0010`: Milestone 3 live validation passed.
- `ROE-0013`: PackageInfo ambiguity resolved.
- `ROE-0014`: score-boundary false positive resolved.
- `ROE-0018`: generated officer actions now persist through reimport/domain reload; live user test passed.
- `ROE-0019`: scene squad references now persist as prefab-instance overrides; live user test passed.
- `ROE-0020`: the gated doorway NavMesh link passed the live two-officer traversal test.

Never delete resolved entries. New defects use `ROE-####` and record environment, reproduction, expected/actual behavior, Console text, evidence, workaround, resolution, and validation.
