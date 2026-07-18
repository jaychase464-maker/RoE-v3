using System;
using System.Linq;
using RulesOfEntry.Officers;

namespace RulesOfEntry.Missions
{
    public readonly struct MissionCompletionDecision
    {
        public MissionCompletionDecision(bool ready, string reason)
        {
            Ready = ready;
            Reason = reason ?? string.Empty;
        }

        public bool Ready { get; }
        public string Reason { get; }
    }

    /// <summary>
    /// Pure automatic-completion gate. Optional score opportunities never hold a mission
    /// open, but every required objective and every authored tactical room must resolve.
    /// </summary>
    public static class MissionCompletionRules
    {
        public static MissionCompletionDecision Evaluate(
            AfterActionReport provisionalReport,
            MissionEvidenceSnapshot evidence)
        {
            if (provisionalReport == null)
            {
                throw new ArgumentNullException(nameof(provisionalReport));
            }

            if (evidence == null)
            {
                throw new ArgumentNullException(nameof(evidence));
            }

            MissionObjectiveEvaluation[] required = provisionalReport.Objectives
                .Where(objective => objective.Required)
                .ToArray();
            if (required.Length == 0)
            {
                return new MissionCompletionDecision(
                    false,
                    "No required objectives are configured.");
            }

            MissionObjectiveEvaluation pending = required.FirstOrDefault(objective =>
                objective.Status == MissionObjectiveStatus.Pending);
            if (pending != null)
            {
                return new MissionCompletionDecision(
                    false,
                    $"Required objective remains pending: {pending.DisplayName}.");
            }

            RoomEvidenceSnapshot? unresolvedRoom = evidence.Rooms
                .Where(room => room.State != TacticalRoomClearanceState.Clear
                    || room.ActiveThreatCount > 0)
                .Select(room => (RoomEvidenceSnapshot?)room)
                .FirstOrDefault();
            if (unresolvedRoom.HasValue)
            {
                RoomEvidenceSnapshot room = unresolvedRoom.Value;
                return new MissionCompletionDecision(
                    false,
                    $"Tactical room {room.RoomId} remains {room.State} with "
                        + $"{room.ActiveThreatCount} active threat(s).");
            }

            return new MissionCompletionDecision(
                true,
                required.Any(objective => objective.Status == MissionObjectiveStatus.Failed)
                    ? "All required objectives and tactical rooms reached terminal states."
                    : "All required objectives are complete and every tactical room is verified clear.");
        }
    }
}
