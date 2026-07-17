using System;
using System.Linq;
using RulesOfEntry.Actors;
using RulesOfEntry.Officers;

namespace RulesOfEntry.Missions
{
    /// <summary>
    /// Pure objective evaluator. The same evidence produces the same result.
    /// </summary>
    public static class MissionObjectiveEvaluator
    {
        public static MissionObjectiveEvaluation[] Evaluate(
            MissionDefinition definition,
            MissionEvidenceSnapshot evidence)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (evidence == null)
            {
                throw new ArgumentNullException(nameof(evidence));
            }

            return definition.Objectives
                .Select(objective => Evaluate(objective, evidence))
                .ToArray();
        }

        public static MissionObjectiveEvaluation Evaluate(
            MissionObjectiveDefinition objective,
            MissionEvidenceSnapshot evidence)
        {
            if (objective == null)
            {
                throw new ArgumentNullException(nameof(objective));
            }

            if (evidence == null)
            {
                throw new ArgumentNullException(nameof(evidence));
            }

            return objective.Type switch
            {
                MissionObjectiveType.SecureSubject => EvaluateSecureSubject(objective, evidence),
                MissionObjectiveType.ProtectActor => EvaluateProtectActor(objective, evidence),
                MissionObjectiveType.VerifyRoomClear => EvaluateRoomClear(objective, evidence),
                MissionObjectiveType.PreserveOfficerTeam => EvaluateOfficerTeam(objective, evidence),
                _ => Create(
                    objective,
                    MissionObjectiveStatus.Failed,
                    "Objective type is not supported by this evaluator.")
            };
        }

        private static MissionObjectiveEvaluation EvaluateSecureSubject(
            MissionObjectiveDefinition objective,
            MissionEvidenceSnapshot evidence)
        {
            ActorEvidenceSnapshot? actor = FindActor(evidence, objective.TargetActorId);
            if (!actor.HasValue)
            {
                return Create(
                    objective,
                    MissionObjectiveStatus.Failed,
                    $"Configured subject {objective.TargetActorId} is absent from incident evidence.");
            }

            ActorEvidenceSnapshot value = actor.Value;
            bool secured = value.Custody == CustodyState.Restrained
                || value.Custody == CustodyState.Searched
                || value.Custody == CustodyState.InCustody;
            if (secured)
            {
                return Create(
                    objective,
                    MissionObjectiveStatus.Completed,
                    $"{value.ActorId} is physically restrained in the custody record.");
            }

            if (value.Condition == ActorConditionLevel.Deceased)
            {
                return Create(
                    objective,
                    MissionObjectiveStatus.Failed,
                    $"{value.ActorId} died before lawful custody was established.");
            }

            return Create(
                objective,
                MissionObjectiveStatus.Pending,
                $"{value.ActorId} has not yet reached a restrained custody state.");
        }

        private static MissionObjectiveEvaluation EvaluateProtectActor(
            MissionObjectiveDefinition objective,
            MissionEvidenceSnapshot evidence)
        {
            ActorEvidenceSnapshot? actor = FindActor(evidence, objective.TargetActorId);
            if (!actor.HasValue)
            {
                return Create(
                    objective,
                    MissionObjectiveStatus.Failed,
                    $"Protected actor {objective.TargetActorId} is absent from incident evidence.");
            }

            ActorEvidenceSnapshot value = actor.Value;
            if (value.Condition == ActorConditionLevel.Stable)
            {
                return Create(
                    objective,
                    MissionObjectiveStatus.Completed,
                    $"{value.ActorId} remains uninjured.");
            }

            return Create(
                objective,
                MissionObjectiveStatus.Failed,
                $"{value.ActorId} condition is {value.Condition}; protection objective was not met.");
        }

        private static MissionObjectiveEvaluation EvaluateRoomClear(
            MissionObjectiveDefinition objective,
            MissionEvidenceSnapshot evidence)
        {
            RoomEvidenceSnapshot? room = evidence.Rooms
                .Where(value => string.Equals(
                    value.RoomId,
                    objective.TargetRoomId,
                    StringComparison.Ordinal))
                .Select(value => (RoomEvidenceSnapshot?)value)
                .FirstOrDefault();
            if (!room.HasValue)
            {
                return Create(
                    objective,
                    MissionObjectiveStatus.Failed,
                    $"Configured room {objective.TargetRoomId} is absent from incident evidence.");
            }

            RoomEvidenceSnapshot value = room.Value;
            if (value.State == TacticalRoomClearanceState.Clear
                && value.ActiveThreatCount == 0)
            {
                return Create(
                    objective,
                    MissionObjectiveStatus.Completed,
                    $"{value.RoomId} has a verified clear state with no active threat.");
            }

            return Create(
                objective,
                MissionObjectiveStatus.Pending,
                $"{value.RoomId} remains {value.State} with "
                    + $"{value.ActiveThreatCount} active threat(s).");
        }

        private static MissionObjectiveEvaluation EvaluateOfficerTeam(
            MissionObjectiveDefinition objective,
            MissionEvidenceSnapshot evidence)
        {
            ActorEvidenceSnapshot[] officers = evidence.Actors
                .Where(actor => actor.Role == ActorRole.Officer)
                .ToArray();
            if (officers.Length == 0)
            {
                return Create(
                    objective,
                    MissionObjectiveStatus.Failed,
                    "No officer actors were present in incident evidence.");
            }

            ActorEvidenceSnapshot? unavailable = officers
                .Where(actor => actor.Condition == ActorConditionLevel.Incapacitated
                    || actor.Condition == ActorConditionLevel.Deceased)
                .Select(actor => (ActorEvidenceSnapshot?)actor)
                .FirstOrDefault();
            if (unavailable.HasValue)
            {
                return Create(
                    objective,
                    MissionObjectiveStatus.Failed,
                    $"{unavailable.Value.ActorId} ended the operation "
                        + $"{unavailable.Value.Condition}.");
            }

            return Create(
                objective,
                MissionObjectiveStatus.Completed,
                $"All {officers.Length} recorded officers remain actionable.");
        }

        private static ActorEvidenceSnapshot? FindActor(
            MissionEvidenceSnapshot evidence,
            string actorId)
        {
            return evidence.Actors
                .Where(actor => string.Equals(
                    actor.ActorId,
                    actorId,
                    StringComparison.Ordinal))
                .Select(actor => (ActorEvidenceSnapshot?)actor)
                .FirstOrDefault();
        }

        private static MissionObjectiveEvaluation Create(
            MissionObjectiveDefinition objective,
            MissionObjectiveStatus status,
            string rationale)
        {
            return new MissionObjectiveEvaluation(
                objective.ObjectiveId,
                objective.DisplayName,
                objective.Type,
                status,
                objective.Required,
                objective.FailureDeduction,
                rationale);
        }
    }
}
