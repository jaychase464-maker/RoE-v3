namespace RulesOfEntry.Officers
{
    /// <summary>
    /// Pure lifecycle rules for one officer order. It performs no scene or navigation work.
    /// </summary>
    public sealed class OfficerOrderStateMachine
    {
        public OfficerOrderStatus Status { get; private set; } = OfficerOrderStatus.Pending;
        public OfficerOrderOutcomeReason OutcomeReason { get; private set; }
        public string Details { get; private set; } = string.Empty;
        public bool IsTerminal => Status == OfficerOrderStatus.Completed
            || Status == OfficerOrderStatus.Cancelled
            || Status == OfficerOrderStatus.Failed
            || Status == OfficerOrderStatus.Refused;

        public bool TryAccept(string details = "Order acknowledged.")
        {
            return TryTransition(
                OfficerOrderStatus.Pending,
                OfficerOrderStatus.Accepted,
                OfficerOrderOutcomeReason.None,
                details);
        }

        public bool TryBeginExecution(string details = "Order execution started.")
        {
            return TryTransition(
                OfficerOrderStatus.Accepted,
                OfficerOrderStatus.Executing,
                OfficerOrderOutcomeReason.None,
                details);
        }

        public bool TryComplete(string details = "Order completed.")
        {
            if (Status != OfficerOrderStatus.Executing)
            {
                return false;
            }

            Status = OfficerOrderStatus.Completed;
            OutcomeReason = OfficerOrderOutcomeReason.None;
            Details = details ?? string.Empty;
            return true;
        }

        public bool TryCancel(
            OfficerOrderOutcomeReason reason,
            string details = "Order cancelled.")
        {
            if (IsTerminal
                || (reason != OfficerOrderOutcomeReason.CancelledByPlayer
                    && reason != OfficerOrderOutcomeReason.Superseded))
            {
                return false;
            }

            Status = OfficerOrderStatus.Cancelled;
            OutcomeReason = reason;
            Details = details ?? string.Empty;
            return true;
        }

        public bool TryFail(OfficerOrderOutcomeReason reason, string details)
        {
            if (IsTerminal || reason == OfficerOrderOutcomeReason.None)
            {
                return false;
            }

            Status = OfficerOrderStatus.Failed;
            OutcomeReason = reason;
            Details = details ?? string.Empty;
            return true;
        }

        public bool TryRefuse(OfficerOrderOutcomeReason reason, string details)
        {
            if (Status != OfficerOrderStatus.Pending
                || reason == OfficerOrderOutcomeReason.None)
            {
                return false;
            }

            Status = OfficerOrderStatus.Refused;
            OutcomeReason = reason;
            Details = details ?? string.Empty;
            return true;
        }

        private bool TryTransition(
            OfficerOrderStatus required,
            OfficerOrderStatus next,
            OfficerOrderOutcomeReason reason,
            string details)
        {
            if (Status != required)
            {
                return false;
            }

            Status = next;
            OutcomeReason = reason;
            Details = details ?? string.Empty;
            return true;
        }
    }
}
