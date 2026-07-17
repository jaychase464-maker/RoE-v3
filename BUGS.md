# Bugs and Validation Gaps

## Open

### ROE-0022 — Milestone 5 requires live Unity validation

- Priority: blocker for milestone closure
- Status: integration candidate
- Environment: Unity `6000.5.2f1`, HDRP `17.5.0`, Input System `1.19.0`, AI Navigation `2.0.14`
- Required evidence: compilation; setup/validator; all tests; peaceful final report; ambiguous discharge review; serious and critical ROE findings; prior milestone regressions; clean Console
- Safety boundary: mission evaluation must remain a one-way consumer of factual state and ledgers

### ROE-0023 — Current ROE evidence cannot determine intent for non-actor impacts

- Priority: high fidelity limitation
- Status: open by design
- Current behavior: non-actor impacts become `ReviewRequired` with no automatic deduction
- Missing evidence: intended subject, perceived threat, target identification, warnings, backdrop, crossfire, cover, visibility, feasibility of alternatives, and complete temporal context
- Resolution direction: add immutable perception/aim/command/context facts before expanding automatic determinations

### ROE-0024 — Numeric report is a prototype summary

- Priority: medium design debt
- Status: open
- Current behavior: explicit objective and violation deductions plus rating caps
- Missing: department-specific policy configuration, narrative supervisor review, unresolved-finding workflow, evidence links, appeals/corrections, campaign consequences, and expert legal/procedural review

## Existing realism debt

- Individual officer paths lack production formations, sectors, cover selection, pieing, and coordinated entry timing.
- Custody lacks production animation, IK, handcuff props, search, and transport.
- Ballistics lack time of flight, penetration, fragmentation, ricochet, armor, and validated physiology.
- Suspects lack production weapons and attack execution.
- Default company identity remains until an approved studio identity is supplied.

## Resolved

- ROE-0026: officer challenge sequences could stop after one command when normal forward-FOV reacquisition failed despite retained visual contact.
- ROE-0025: Milestone 5 manual-finalization test referenced the nonexistent `ObjectiveEvaluations` property instead of `Objectives`.
- ROE-0013: PackageInfo ambiguity.
- ROE-0014: score-boundary validator false positive.
- ROE-0018: generated input action persistence.
- ROE-0019: saved squad prefab-instance references.
- ROE-0020: closed-door navigation link.
- ROE-0021: bounded officer initiative passed the live user test.
