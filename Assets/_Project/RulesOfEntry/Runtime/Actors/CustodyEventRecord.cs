namespace RulesOfEntry.Actors
{
    public sealed class CustodyEventRecord
    {
        public CustodyEventRecord(
            long sequence,
            double occurredAtSeconds,
            string actorId,
            ulong actorEntityId,
            ulong officerEntityId,
            CustodyAction action,
            CustodyState previousState,
            CustodyState newState,
            string details)
        {
            Sequence = sequence;
            OccurredAtSeconds = occurredAtSeconds;
            ActorId = actorId ?? string.Empty;
            ActorEntityId = actorEntityId;
            OfficerEntityId = officerEntityId;
            Action = action;
            PreviousState = previousState;
            NewState = newState;
            Details = details ?? string.Empty;
        }

        public long Sequence { get; }
        public double OccurredAtSeconds { get; }
        public string ActorId { get; }
        public ulong ActorEntityId { get; }
        public ulong OfficerEntityId { get; }
        public CustodyAction Action { get; }
        public CustodyState PreviousState { get; }
        public CustodyState NewState { get; }
        public string Details { get; }
    }
}
