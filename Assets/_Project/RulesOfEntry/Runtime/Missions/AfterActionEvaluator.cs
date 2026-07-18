using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Actors;

namespace RulesOfEntry.Missions
{
    /// <summary>
    /// Deterministic report builder. Every point is derived from the immutable mission
    /// evidence snapshot so UI, AI, and campaign systems cannot invent a better outcome.
    /// </summary>
    public static class AfterActionEvaluator
    {
        private const int ObjectiveMaximum = 30;
        private const int CivilianMaximum = 20;
        private const int SuspectMaximum = 15;
        private const int OfficerMaximum = 10;
        private const int RoeMaximum = 10;
        private const int EvidenceMaximum = 10;
        private const int TimeMaximum = 5;

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
            MissionOutcomeMetrics metrics = BuildMetrics(evidence);
            MissionScoreCategory[] categories = BuildCategories(
                definition,
                objectives,
                findings,
                metrics,
                evidence.MissionElapsedSeconds);
            int uncappedScore = categories.Sum(category => category.EarnedScore);
            int scoreCap = DetermineScoreCap(objectives, findings, metrics, policy);
            int score = Math.Max(0, Math.Min(scoreCap, uncappedScore));
            MissionPerformanceTier tier = final
                ? DetermineTier(score)
                : MissionPerformanceTier.NotRated;
            OperationalRating rating = final
                ? DetermineRating(tier)
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
                ? $"Tier {tier}: {completed} objective(s) completed, {failed} failed; "
                    + $"{metrics.CiviliansSaved}/{metrics.CiviliansTotal} civilian(s) saved; "
                    + $"{metrics.SuspectsArrested}/{metrics.SuspectsTotal} suspect(s) arrested; "
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
                tier,
                scoreCap,
                objectives,
                findings,
                categories,
                metrics,
                summary);
        }

        public static MissionPerformanceTier DetermineTier(int score)
        {
            int clamped = Math.Max(0, Math.Min(100, score));
            if (clamped >= 95)
            {
                return MissionPerformanceTier.S;
            }

            if (clamped >= 90)
            {
                return MissionPerformanceTier.A;
            }

            if (clamped >= 80)
            {
                return MissionPerformanceTier.B;
            }

            if (clamped >= 75)
            {
                return MissionPerformanceTier.C;
            }

            if (clamped >= 60)
            {
                return MissionPerformanceTier.D;
            }

            return MissionPerformanceTier.F;
        }

        private static MissionScoreCategory[] BuildCategories(
            MissionDefinition definition,
            IReadOnlyCollection<MissionObjectiveEvaluation> objectives,
            IReadOnlyCollection<RoeFinding> findings,
            MissionOutcomeMetrics metrics,
            double elapsedSeconds)
        {
            return new[]
            {
                BuildObjectiveCategory(objectives),
                BuildCivilianCategory(metrics),
                BuildSuspectCategory(metrics),
                BuildOfficerCategory(metrics),
                BuildRoeCategory(findings),
                BuildEvidenceCategory(metrics),
                BuildTimeCategory(definition, elapsedSeconds)
            };
        }

        private static MissionScoreCategory BuildObjectiveCategory(
            IReadOnlyCollection<MissionObjectiveEvaluation> objectives)
        {
            int deduction = objectives
                .Where(objective => objective.Status == MissionObjectiveStatus.Failed)
                .Sum(objective => objective.FailureDeduction);
            int complete = objectives.Count(objective =>
                objective.Status == MissionObjectiveStatus.Completed);
            int pending = objectives.Count(objective =>
                objective.Status == MissionObjectiveStatus.Pending);
            int failed = objectives.Count(objective =>
                objective.Status == MissionObjectiveStatus.Failed);
            return Category(
                MissionScoreCategoryType.Objectives,
                "OBJECTIVES",
                ObjectiveMaximum - deduction,
                ObjectiveMaximum,
                $"{complete} complete / {pending} pending / {failed} failed");
        }

        private static MissionScoreCategory BuildCivilianCategory(
            MissionOutcomeMetrics metrics)
        {
            if (metrics.CiviliansTotal == 0)
            {
                return Category(
                    MissionScoreCategoryType.CivilianSafety,
                    "CIVILIAN SAFETY",
                    CivilianMaximum,
                    CivilianMaximum,
                    "No civilians were present");
            }

            double weightedSafety = metrics.CiviliansSaved
                - metrics.CiviliansWounded * 0.25d
                - metrics.CiviliansIncapacitated * 0.65d;
            int score = ProportionalScore(
                weightedSafety,
                metrics.CiviliansTotal,
                CivilianMaximum);
            return Category(
                MissionScoreCategoryType.CivilianSafety,
                "CIVILIAN SAFETY",
                score,
                CivilianMaximum,
                $"{metrics.CiviliansSaved}/{metrics.CiviliansTotal} saved; "
                    + $"{metrics.CiviliansKilled} killed");
        }

        private static MissionScoreCategory BuildSuspectCategory(
            MissionOutcomeMetrics metrics)
        {
            if (metrics.SuspectsTotal == 0)
            {
                return Category(
                    MissionScoreCategoryType.SuspectCustody,
                    "SUSPECT CUSTODY",
                    SuspectMaximum,
                    SuspectMaximum,
                    "No suspects were present");
            }

            int score = ProportionalScore(
                metrics.SuspectsArrested,
                metrics.SuspectsTotal,
                SuspectMaximum);
            return Category(
                MissionScoreCategoryType.SuspectCustody,
                "SUSPECT CUSTODY",
                score,
                SuspectMaximum,
                $"{metrics.SuspectsArrested}/{metrics.SuspectsTotal} arrested; "
                    + $"{metrics.SuspectsKilled} killed");
        }

        private static MissionScoreCategory BuildOfficerCategory(
            MissionOutcomeMetrics metrics)
        {
            if (metrics.OfficersTotal == 0)
            {
                return Category(
                    MissionScoreCategoryType.OfficerSafety,
                    "OFFICER SAFETY",
                    OfficerMaximum,
                    OfficerMaximum,
                    "No scene officers were recorded");
            }

            double weightedSafety = metrics.OfficersTotal
                - metrics.OfficersWounded * 0.3d
                - metrics.OfficersIncapacitated * 0.75d
                - metrics.OfficersKilled;
            int score = ProportionalScore(
                weightedSafety,
                metrics.OfficersTotal,
                OfficerMaximum);
            return Category(
                MissionScoreCategoryType.OfficerSafety,
                "OFFICER SAFETY",
                score,
                OfficerMaximum,
                $"{metrics.OfficersKilled} killed; "
                    + $"{metrics.OfficersIncapacitated} incapacitated; "
                    + $"{metrics.OfficersWounded} wounded");
        }

        private static MissionScoreCategory BuildRoeCategory(
            IReadOnlyCollection<RoeFinding> findings)
        {
            int deduction = findings
                .Where(finding => finding.Determination == RoeDetermination.Violation)
                .Sum(finding => finding.ScoreDeduction);
            int violations = findings.Count(finding =>
                finding.Determination == RoeDetermination.Violation);
            int reviews = findings.Count(finding =>
                finding.Determination == RoeDetermination.ReviewRequired);
            return Category(
                MissionScoreCategoryType.RulesOfEngagement,
                "RULES OF ENGAGEMENT",
                RoeMaximum - deduction,
                RoeMaximum,
                $"{violations} violation(s); {reviews} review(s)");
        }

        private static MissionScoreCategory BuildEvidenceCategory(
            MissionOutcomeMetrics metrics)
        {
            int score = metrics.EvidenceOpportunities == 0
                ? EvidenceMaximum
                : ProportionalScore(
                    metrics.EvidenceItemsSecured,
                    metrics.EvidenceOpportunities,
                    EvidenceMaximum);
            string summary = metrics.EvidenceOpportunities == 0
                ? "No recoverable suspect evidence"
                : $"{metrics.EvidenceItemsSecured}/{metrics.EvidenceOpportunities} item(s) secured";
            return Category(
                MissionScoreCategoryType.Evidence,
                "EVIDENCE",
                score,
                EvidenceMaximum,
                summary);
        }

        private static MissionScoreCategory BuildTimeCategory(
            MissionDefinition definition,
            double elapsedSeconds)
        {
            double target = definition.TargetCompletionSeconds;
            double maximum = definition.MaximumScoredCompletionSeconds;
            int score;
            if (elapsedSeconds <= target)
            {
                score = TimeMaximum;
            }
            else if (elapsedSeconds >= maximum || maximum <= target)
            {
                score = 0;
            }
            else
            {
                double fraction = 1d - (elapsedSeconds - target) / (maximum - target);
                score = (int)Math.Round(TimeMaximum * fraction, MidpointRounding.AwayFromZero);
            }

            return Category(
                MissionScoreCategoryType.Time,
                "TIME",
                score,
                TimeMaximum,
                $"{FormatDuration(elapsedSeconds)} / target {FormatDuration(target)}");
        }

        private static MissionOutcomeMetrics BuildMetrics(MissionEvidenceSnapshot evidence)
        {
            ActorEvidenceSnapshot[] civilians = evidence.Actors
                .Where(actor => actor.Role == ActorRole.Civilian)
                .ToArray();
            ActorEvidenceSnapshot[] suspects = evidence.Actors
                .Where(actor => actor.Role == ActorRole.Suspect)
                .ToArray();
            ActorEvidenceSnapshot[] officers = evidence.Actors
                .Where(actor => actor.Role == ActorRole.Officer)
                .ToArray();
            int evidenceOpportunities = suspects.Sum(actor =>
                (actor.HadWeapon ? 1 : 0) + actor.ReportableItemCount);
            int evidenceSecured = suspects.Sum(actor =>
                (actor.HadWeapon && actor.WeaponSecured ? 1 : 0)
                + (actor.Searched ? actor.ReportableItemCount : 0));

            return new MissionOutcomeMetrics(
                civilians.Length,
                civilians.Count(actor => actor.Condition != ActorConditionLevel.Deceased),
                civilians.Count(actor => actor.Condition == ActorConditionLevel.Wounded),
                civilians.Count(actor => actor.Condition == ActorConditionLevel.Incapacitated),
                civilians.Count(actor => actor.Condition == ActorConditionLevel.Deceased),
                suspects.Length,
                suspects.Count(IsArrested),
                suspects.Count(actor => actor.Condition == ActorConditionLevel.Incapacitated),
                suspects.Count(actor => actor.Condition == ActorConditionLevel.Deceased),
                officers.Length,
                officers.Count(actor => actor.Condition == ActorConditionLevel.Wounded),
                officers.Count(actor => actor.Condition == ActorConditionLevel.Incapacitated),
                officers.Count(actor => actor.Condition == ActorConditionLevel.Deceased),
                evidenceOpportunities,
                evidenceSecured);
        }

        private static int DetermineScoreCap(
            IReadOnlyCollection<MissionObjectiveEvaluation> objectives,
            IReadOnlyCollection<RoeFinding> findings,
            MissionOutcomeMetrics metrics,
            RulesOfEngagementPolicy policy)
        {
            int cap = 100;
            if (objectives.Any(objective => objective.Required
                && objective.Status == MissionObjectiveStatus.Failed))
            {
                cap = Math.Min(cap, 74);
            }

            if (metrics.OfficersKilled > 0)
            {
                cap = Math.Min(cap, 74);
            }

            if (metrics.CiviliansKilled > 0)
            {
                cap = Math.Min(cap, 59);
            }

            if (findings.Any(finding =>
                finding.Determination == RoeDetermination.Violation
                && finding.Severity == RoeSeverity.Critical))
            {
                cap = Math.Min(cap, policy.CriticalScoreCap);
            }

            return cap;
        }

        private static OperationalRating DetermineRating(MissionPerformanceTier tier)
        {
            return tier switch
            {
                MissionPerformanceTier.S => OperationalRating.Exemplary,
                MissionPerformanceTier.A => OperationalRating.Exemplary,
                MissionPerformanceTier.B => OperationalRating.Acceptable,
                MissionPerformanceTier.C => OperationalRating.Acceptable,
                MissionPerformanceTier.D => OperationalRating.Deficient,
                _ => OperationalRating.CriticalFailure
            };
        }

        private static MissionScoreCategory Category(
            MissionScoreCategoryType type,
            string displayName,
            int earned,
            int maximum,
            string summary)
        {
            return new MissionScoreCategory(type, displayName, earned, maximum, summary);
        }

        private static int ProportionalScore(double achieved, double total, int maximum)
        {
            if (total <= 0d)
            {
                return maximum;
            }

            double fraction = Math.Max(0d, Math.Min(1d, achieved / total));
            return (int)Math.Round(maximum * fraction, MidpointRounding.AwayFromZero);
        }

        private static bool IsArrested(ActorEvidenceSnapshot actor)
        {
            return actor.Custody == CustodyState.Restrained
                || actor.Custody == CustodyState.Searched
                || actor.Custody == CustodyState.InCustody;
        }

        private static string FormatDuration(double seconds)
        {
            TimeSpan duration = TimeSpan.FromSeconds(Math.Max(0d, seconds));
            return $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}";
        }
    }
}
