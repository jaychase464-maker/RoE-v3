using System;
using System.Collections.Generic;
using RulesOfEntry.Actors;
using UnityEngine;

namespace RulesOfEntry.Officers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ActorIdentity))]
    public sealed class OfficerOrderLedger : MonoBehaviour
    {
        [SerializeField] private ActorIdentity identity;

        private readonly List<OfficerOrderEventRecord> records =
            new List<OfficerOrderEventRecord>();
        private long nextLedgerSequence = 1;

        public event Action<OfficerOrderEventRecord> RecordAdded;

        public IReadOnlyList<OfficerOrderEventRecord> Records => records.AsReadOnly();

        public void Configure(ActorIdentity configuredIdentity)
        {
            identity = configuredIdentity;
        }

        public OfficerOrderEventRecord Record(
            OfficerOrder order,
            OfficerOrderStateMachine state)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            identity ??= GetComponent<ActorIdentity>();
            if (identity == null)
            {
                throw new InvalidOperationException(
                    "OfficerOrderLedger requires an ActorIdentity.");
            }

            OfficerOrderEventRecord record = new OfficerOrderEventRecord(
                nextLedgerSequence++,
                Time.timeAsDouble,
                identity.ActorId,
                identity.RuntimeEntityId,
                order.CommandSequence,
                order.IssuerEntityId,
                order.Type,
                state.Status,
                state.OutcomeReason,
                order.TargetDescription,
                order.TargetEntityId,
                state.Details,
                order.Origin);
            records.Add(record);
            RecordAdded?.Invoke(record);
            return record;
        }
    }
}
