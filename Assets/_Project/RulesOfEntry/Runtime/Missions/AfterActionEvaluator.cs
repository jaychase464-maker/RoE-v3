using System;
using System.Linq;

namespace RulesOfEntry.Missions
{
    /// <summary>
    /// Deterministic report builder. Scoring is a transparent interpretation of evidence,
    /// never a side effect of combat, AI, custody, or officer execution.
    /// </summary>
    public static class AfterActionEvaluator
    {
        public static AfterActionReport Evaluate(
            MissionDefinition definition,
            RulesOfEngagementPolicy policy,
            MissionEvidenceSnapshot evidence,
            bool final)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            if (evidence == null)
            {
                throw new ArgumentNullException(nameof(evidence));
            }

            MissionObjectiveEvaluation[] objectives =
                MissionObjectiveEvaluator.Evaluate(definition, evidence);
            if (final)
            {
                objectives = objectives
                    .Select(objective => objective.Status == MissionObjectiveStatus.Pending
                        ? new MissionObjectiveEvaluation(
                            objective.ObjectiveId,
                            objective.DisplayName,
                            objective.Type,
                            MissionObjectiveStatus.Failed,
                            objective.Required,
                            objective.FailureDeduction,
                            "The operation ended before this objective was resolved. "
                                + objective.Rationale)
                        : objective)
                    .ToArray();
            }

            RoeFinding[] findings = RulesOfEngagementEvaluator.Evaluate(policy, evidence);
            int score = 100;
            foreach (MissionObjectiveEvaluation objective in objectives)
            {
                if (objective.Status == MissionObjectiveStatus.Failed)
                {
                    score -= objective.FailureDeduction;
                }
            }

            foreach (RoeFinding finding in findings)
            {
                if (finding.Determination == RoeDetermination.Violation)
                {
                    score -= finding.ScoreDeduction;
                }
            }

            bool requiredFailure = objectives.Any(objective => objective.Required
                && objective.Status == MissionObjectiveStatus.Failed);
            bool criticalViolation = findings.Any(finding =>
                finding.Determination == RoeDetermination.Violation
                && finding.Severity == RoeSeverity.Critical);
            score = Math.Max(0, Math.Min(100, score));
            if (requiredFailure)
            {
                score = Math.Min(score, 74);
            }

            if (criticalViolation)
            {
                score = Math.Min(score, policy.CriticalScoreCap);
            }

            OperationalRating rating = final
                ? DetermineRating(score)
                : OperationalRating.NotRated;
            int completed = objectives.Count(objective =>
                objective.Status == MissionObjectiveStatus.Completed);
            int failed = objectives.Count(objective =>
                objective.Status == MissionObjectiveStatus.Failed);
            int violations = findings.Count(finding =>
                finding.Determination == RoeDetermination.Violation);
            int reviews = findings.Count(finding =>
                finding.Determination == RoeDetermination.ReviewRequired);
            string summary = final
                ? $"Final: {completed} objective(s) completed, {failed} failed, "
                    + $"{violations} ROE violation(s), {reviews} event(s) requiring review."
                : $"Provisional: {completed} objective(s) completed, {failed} failed, "
                    + $"{objectives.Count(objective => objective.Status == MissionObjectiveStatus.Pending)} pending.";

            return new AfterActionReport(
                definition.MissionId,
                definition.DisplayName,
                evidence.CapturedAtSeconds,
                evidence.MissionElapsedSeconds,
                final,
                score,
                rating,
                objectives,
                findings,
                summary);
        }

        private static OperationalRating DetermineRating(int score)
        {
            if (score >= 90)
            {
                return OperationalRating.Exemplary;
            }

            if (score >= 75)
            {
                return OperationalRating.Acceptable;
            }

            if (score >= 60)
            {
                return OperationalRating.Deficient;
            }

            return OperationalRating.CriticalFailure;
        }
    }
}
