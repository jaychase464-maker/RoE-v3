using System;
using System.Linq;
using RulesOfEntry.Actors;
using RulesOfEntry.Combat;

namespace RulesOfEntry.Missions
{
    /// <summary>
    /// Pure policy evaluation over pre-impact force facts. Ambiguous events are sent to
    /// review instead of being guessed into compliance or misconduct.
    /// </summary>
    public static class RulesOfEngagementEvaluator
    {
        public static RoeFinding[] Evaluate(
            RulesOfEngagementPolicy policy,
            MissionEvidenceSnapshot evidence)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            if (evidence == null)
            {
                throw new ArgumentNullException(nameof(evidence));
            }

            return evidence.ForceEvents
                .Select(record => Evaluate(policy, record))
                .ToArray();
        }

        public static RoeFinding Evaluate(
            RulesOfEngagementPolicy policy,
            ForceEventRecord forceEvent)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            if (forceEvent == null)
            {
                throw new ArgumentNullException(nameof(forceEvent));
            }

            ForceSubjectSnapshot subject = forceEvent.SubjectBeforeImpact;
            string findingId = $"{policy.PolicyId}/force/"
                + $"{forceEvent.ShooterEntityId}/{forceEvent.Sequence}";
            if (!subject.HasActor)
            {
                return Create(
                    policy,
                    forceEvent,
                    findingId,
                    string.Empty,
                    RoeDetermination.ReviewRequired,
                    RoeSeverity.Advisory,
                    "Discharge requires contextual review",
                    "The factual record does not identify an impacted person, so target intent and necessity cannot be determined automatically.");
            }

            if (subject.Role == ActorRole.Officer)
            {
                return Create(
                    policy,
                    forceEvent,
                    findingId,
                    subject.ActorId,
                    RoeDetermination.Violation,
                    RoeSeverity.Critical,
                    "Officer struck by friendly fire",
                    "The pre-impact subject record identifies a law-enforcement officer.");
            }

            if (subject.Role == ActorRole.Civilian)
            {
                return Create(
                    policy,
                    forceEvent,
                    findingId,
                    subject.ActorId,
                    RoeDetermination.Violation,
                    RoeSeverity.Critical,
                    "Civilian struck by police gunfire",
                    "The pre-impact record identifies a civilian rather than an active deadly threat.");
            }

            bool controlledCustody = subject.Custody == CustodyState.Surrendering
                || subject.Custody == CustodyState.Kneeling
                || subject.Custody == CustodyState.Restrained
                || subject.Custody == CustodyState.Searched
                || subject.Custody == CustodyState.InCustody;
            bool controlledBehavior = subject.Behavior == HumanBehaviorState.Complying
                || subject.Behavior == HumanBehaviorState.Surrendering
                || subject.Behavior == HumanBehaviorState.Restrained;
            if (controlledCustody || controlledBehavior)
            {
                return Create(
                    policy,
                    forceEvent,
                    findingId,
                    subject.ActorId,
                    RoeDetermination.Violation,
                    RoeSeverity.Critical,
                    "Controlled subject struck by police gunfire",
                    $"The subject was recorded as {subject.Custody}/{subject.Behavior} before impact.");
            }

            if (subject.Condition == ActorConditionLevel.Incapacitated
                || subject.Condition == ActorConditionLevel.Deceased
                || subject.Behavior == HumanBehaviorState.Incapacitated)
            {
                return Create(
                    policy,
                    forceEvent,
                    findingId,
                    subject.ActorId,
                    RoeDetermination.Violation,
                    RoeSeverity.Critical,
                    "Incapacitated subject struck by police gunfire",
                    $"The subject was already {subject.Condition}/{subject.Behavior} before impact.");
            }

            if (subject.Behavior == HumanBehaviorState.Threatening
                && subject.WeaponAccessible)
            {
                return Create(
                    policy,
                    forceEvent,
                    findingId,
                    subject.ActorId,
                    RoeDetermination.WithinPolicy,
                    RoeSeverity.Advisory,
                    "Force consistent with recorded deadly-threat facts",
                    "The suspect was threatening and had an accessible weapon immediately before impact.");
            }

            if (subject.Behavior == HumanBehaviorState.Threatening
                || subject.WeaponAccessible)
            {
                return Create(
                    policy,
                    forceEvent,
                    findingId,
                    subject.ActorId,
                    RoeDetermination.ReviewRequired,
                    RoeSeverity.Serious,
                    "Force requires threat-context review",
                    "The record contains a threat indicator, but does not establish both threatening behavior and weapon access.");
            }

            return Create(
                policy,
                forceEvent,
                findingId,
                subject.ActorId,
                RoeDetermination.Violation,
                RoeSeverity.Serious,
                "Force not supported by recorded threat facts",
                $"The suspect was {subject.Behavior}, {subject.Custody}, and had no accessible weapon before impact.");
        }

        private static RoeFinding Create(
            RulesOfEngagementPolicy policy,
            ForceEventRecord forceEvent,
            string findingId,
            string subjectActorId,
            RoeDetermination determination,
            RoeSeverity severity,
            string summary,
            string rationale)
        {
            int deduction = determination == RoeDetermination.Violation
                ? policy.GetViolationDeduction(severity)
                : 0;
            return new RoeFinding(
                findingId,
                forceEvent.Sequence,
                forceEvent.OccurredAtSeconds,
                forceEvent.ShooterEntityId,
                subjectActorId,
                determination,
                severity,
                deduction,
                summary,
                rationale);
        }
    }
}
