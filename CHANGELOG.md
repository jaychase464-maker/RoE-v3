# Changelog

## Milestone 4 Hotfix 5 — Persistent Challenge Sequence — 2026-07-16

- Added short-term tactical focus after first visual contact with a suspect.
- Follow-up team challenges now continue at a controlled four-second cadence while the free, actionable suspect remains in unobstructed sight.
- Officers reorient toward the tracked subject without gaining perception through cover.
- Challenge activity remains visible long enough to read in the prototype diagnostics.
- Added pure cadence/focus rules and EditMode regression coverage.

## Milestone 5 Hotfix 1 — 2026-07-16

- Corrected the manual-finalization EditMode test to use `AfterActionReport.Objectives`, matching the production report contract.

## Milestone 5 — Mission, ROE, and After-Action Review — Integration Candidate 2026-07-16

### Added

- Mission and ROE ScriptableObject definitions.
- Secure-subject, protect-actor, verify-room-clear, and preserve-officer-team objectives.
- Immutable incident evidence snapshots from established factual state and ledgers.
- Pure objective, rules-of-engagement, and after-action evaluators.
- Event-level `WithinPolicy`, `ReviewRequired`, and `Violation` findings with rationale.
- Transparent objective/violation deductions and rating caps.
- Automatic mission start, provisional evaluation, finalization, and after-action report.
- In-world manual debrief console with pending-objective failure conversion.
- Mission/AAR debug HUD, setup tool, validator, pre-build gate, and tests.

### Boundaries

- No combat, AI, custody, navigation, officer-order, input, package, render, or level-layout logic was changed.
- Ambiguous force evidence is not assigned an automatic violation.
- Live Unity validation is pending.

## Milestone 4

- Officer commands, gated doorway traversal, bounded suspect challenges, timed room clearance, cover-gated automatic custody, and factual initiative history passed the user's live prototype tests.
