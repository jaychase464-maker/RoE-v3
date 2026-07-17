using System;
using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.Actors
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ActorIdentity))]
    [RequireComponent(typeof(ActorInventory))]
    [RequireComponent(typeof(CustodyEventLedger))]
    public sealed class CustodyComponent : MonoBehaviour
    {
        [SerializeField] private ActorIdentity identity;
        [SerializeField] private ActorInventory inventory;
        [SerializeField] private ActorCondition condition;
        [SerializeField] private CustodyEventLedger eventLedger;

        private CustodyStateMachine stateMachine;

        public event Action<CustodyState> StateChanged;

        public CustodyState State => stateMachine?.State ?? CustodyState.Free;
        public bool IsRestrained => State == CustodyState.Restrained
            || State == CustodyState.Searched
            || State == CustodyState.InCustody;

        public void Configure(
            ActorIdentity configuredIdentity,
            ActorInventory configuredInventory,
            ActorCondition configuredCondition,
            CustodyEventLedger configuredLedger)
        {
            identity = configuredIdentity;
            inventory = configuredInventory;
            condition = configuredCondition;
            eventLedger = configuredLedger;
            stateMachine = new CustodyStateMachine();
        }

        public bool TryBeginSurrender(GameObject officer, string reason)
        {
            return TryTransition(
                officer,
                CustodyAction.BeginSurrender,
                stateMachine.TryBeginSurrender,
                reason);
        }

        public bool TryOrderToKneel(GameObject officer)
        {
            return TryTransition(
                officer,
                CustodyAction.OrderToKneel,
                stateMachine.TryOrderToKneel,
                "Subject was directed into a controlled kneeling position.");
        }

        public bool TrySecureIncapacitated(GameObject officer)
        {
            if (condition == null
                || condition.Snapshot.Level != ActorConditionLevel.Incapacitated)
            {
                return false;
            }

            return TryTransition(
                officer,
                CustodyAction.SecureIncapacitated,
                stateMachine.TrySecureIncapacitated,
                "Incapacitated subject was positioned for restraint.");
        }

        public bool TryApplyRestraints(GameObject officer)
        {
            return TryTransition(
                officer,
                CustodyAction.ApplyRestraints,
                stateMachine.TryApplyRestraints,
                "Handcuffs were applied and checked.");
        }

        public bool TrySearch(GameObject officer, out ActorSearchResult searchResult)
        {
            searchResult = default;
            CustodyState previous = State;
            if (inventory == null || !stateMachine.TryMarkSearched())
            {
                return false;
            }

            searchResult = inventory.Search();
            RecordAndPublish(
                officer,
                CustodyAction.Search,
                previous,
                searchResult.Summary);
            return true;
        }

        public bool TryTransferToCustody(GameObject officer)
        {
            return TryTransition(
                officer,
                CustodyAction.TransferCustody,
                stateMachine.TryTransferToCustody,
                "Search completed and custody confirmed.");
        }

        public bool TryBreakSurrender(string reason)
        {
            CustodyState previous = State;
            if (!stateMachine.TryBreakSurrender())
            {
                return false;
            }

            RecordAndPublish(
                null,
                CustodyAction.BreakSurrender,
                previous,
                reason);
            return true;
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            identity ??= GetComponent<ActorIdentity>();
            inventory ??= GetComponent<ActorInventory>();
            condition ??= GetComponent<ActorCondition>();
            eventLedger ??= GetComponent<CustodyEventLedger>();
            stateMachine ??= new CustodyStateMachine();
        }

        private bool TryTransition(
            GameObject officer,
            CustodyAction action,
            Func<bool> transition,
            string details)
        {
            ResolveReferences();
            CustodyState previous = State;
            if (!transition())
            {
                return false;
            }

            RecordAndPublish(officer, action, previous, details);
            return true;
        }

        private void RecordAndPublish(
            GameObject officer,
            CustodyAction action,
            CustodyState previous,
            string details)
        {
            eventLedger?.Record(identity, officer, action, previous, State, details);
            StateChanged?.Invoke(State);
            ProjectLog.Development(
                "Custody",
                $"{identity?.DisplayName ?? name}: {previous} -> {State} ({action}).",
                this);
        }
    }
}
