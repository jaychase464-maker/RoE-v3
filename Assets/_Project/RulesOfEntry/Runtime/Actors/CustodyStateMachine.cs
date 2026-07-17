namespace RulesOfEntry.Actors
{
    /// <summary>
    /// Pure custody transition authority. Presentation and interaction timing live elsewhere.
    /// </summary>
    public sealed class CustodyStateMachine
    {
        public CustodyState State { get; private set; } = CustodyState.Free;

        public bool TryBeginSurrender()
        {
            return TryMove(CustodyState.Free, CustodyState.Surrendering);
        }

        public bool TryOrderToKneel()
        {
            return TryMove(CustodyState.Surrendering, CustodyState.Kneeling);
        }

        public bool TrySecureIncapacitated()
        {
            return TryMove(CustodyState.Free, CustodyState.Kneeling);
        }

        public bool TryApplyRestraints()
        {
            return TryMove(CustodyState.Kneeling, CustodyState.Restrained);
        }

        public bool TryMarkSearched()
        {
            return TryMove(CustodyState.Restrained, CustodyState.Searched);
        }

        public bool TryTransferToCustody()
        {
            return TryMove(CustodyState.Searched, CustodyState.InCustody);
        }

        public bool TryBreakSurrender()
        {
            if (State != CustodyState.Surrendering && State != CustodyState.Kneeling)
            {
                return false;
            }

            State = CustodyState.Free;
            return true;
        }

        private bool TryMove(CustodyState requiredState, CustodyState nextState)
        {
            if (State != requiredState)
            {
                return false;
            }

            State = nextState;
            return true;
        }
    }
}
