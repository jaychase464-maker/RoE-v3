using System;
using System.Collections.Generic;
using UnityEngine;

namespace RulesOfEntry.Actors
{
    [DisallowMultipleComponent]
    public sealed class CustodyEventLedger : MonoBehaviour
    {
        private readonly List<CustodyEventRecord> records = new List<CustodyEventRecord>();
        private long nextSequence = 1;

        public event Action<CustodyEventRecord> RecordAdded;

        public IReadOnlyList<CustodyEventRecord> Records => records.AsReadOnly();

        public CustodyEventRecord Record(
            ActorIdentity actor,
            GameObject officer,
            CustodyAction action,
            CustodyState previousState,
            CustodyState newState,
            string details)
        {
            if (actor == null)
            {
                throw new ArgumentNullException(nameof(actor));
            }

            CustodyEventRecord record = new CustodyEventRecord(
                nextSequence++,
                Time.timeAsDouble,
                actor.ActorId,
                actor.RuntimeEntityId,
                officer != null ? EntityId.ToULong(officer.GetEntityId()) : 0UL,
                action,
                previousState,
                newState,
                details);
            records.Add(record);
            RecordAdded?.Invoke(record);
            return record;
        }
    }
}
