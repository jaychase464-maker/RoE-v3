using RulesOfEntry.Actors;

namespace RulesOfEntry.Officers
{
    public enum TacticalRoomClearanceState
    {
        Unclear = 0,
        Verifying = 1,
        Clear = 2
    }

    public enum OfficerInitiativeEventType
    {
        ChallengeIssued = 0,
        AutomaticCustodyAssigned = 1,
        AutomaticCustodyAborted = 2
    }

    public sealed class OfficerInitiativeRecord
    {
        public OfficerInitiativeRecord(
            long sequence,
            double occurredAtSeconds,
            string officerActorId,
            ulong officerEntityId,
            OfficerInitiativeEventType eventType,
            string subjectActorId,
            ulong subjectEntityId,
            string roomId,
            VerbalCommandType? verbalCommand,
            string details)
        {
            Sequence = sequence;
            OccurredAtSeconds = occurredAtSeconds;
            OfficerActorId = officerActorId ?? string.Empty;
            OfficerEntityId = officerEntityId;
            EventType = eventType;
            SubjectActorId = subjectActorId ?? string.Empty;
            SubjectEntityId = subjectEntityId;
            RoomId = roomId ?? string.Empty;
            VerbalCommand = verbalCommand;
            Details = details ?? string.Empty;
        }

        public long Sequence { get; }
        public double OccurredAtSeconds { get; }
        public string OfficerActorId { get; }
        public ulong OfficerEntityId { get; }
        public OfficerInitiativeEventType EventType { get; }
        public string SubjectActorId { get; }
        public ulong SubjectEntityId { get; }
        public string RoomId { get; }
        public VerbalCommandType? VerbalCommand { get; }
        public string Details { get; }
    }
}
