namespace RulesOfEntry.Officers
{
    public sealed class OfficerOrderEventRecord
    {
        public OfficerOrderEventRecord(
            long ledgerSequence,
            double occurredAtSeconds,
            string officerActorId,
            ulong officerEntityId,
            long commandSequence,
            ulong issuerEntityId,
            OfficerOrderType orderType,
            OfficerOrderStatus status,
            OfficerOrderOutcomeReason outcomeReason,
            string targetDescription,
            ulong targetEntityId,
            string details,
            OfficerOrderOrigin origin)
        {
            LedgerSequence = ledgerSequence;
            OccurredAtSeconds = occurredAtSeconds;
            OfficerActorId = officerActorId ?? string.Empty;
            OfficerEntityId = officerEntityId;
            CommandSequence = commandSequence;
            IssuerEntityId = issuerEntityId;
            OrderType = orderType;
            Status = status;
            OutcomeReason = outcomeReason;
            TargetDescription = targetDescription ?? string.Empty;
            TargetEntityId = targetEntityId;
            Details = details ?? string.Empty;
            Origin = origin;
        }

        public long LedgerSequence { get; }
        public double OccurredAtSeconds { get; }
        public string OfficerActorId { get; }
        public ulong OfficerEntityId { get; }
        public long CommandSequence { get; }
        public ulong IssuerEntityId { get; }
        public OfficerOrderType OrderType { get; }
        public OfficerOrderStatus Status { get; }
        public OfficerOrderOutcomeReason OutcomeReason { get; }
        public string TargetDescription { get; }
        public ulong TargetEntityId { get; }
        public string Details { get; }
        public OfficerOrderOrigin Origin { get; }
    }
}
