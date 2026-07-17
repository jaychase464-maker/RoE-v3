using System;
using System.Collections.Generic;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using UnityEngine;

namespace RulesOfEntry.Officers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ActorIdentity))]
    public sealed class OfficerInitiativeLedger : MonoBehaviour
    {
        [SerializeField] private ActorIdentity officerIdentity;

        private readonly List<OfficerInitiativeRecord> records =
            new List<OfficerInitiativeRecord>();
        private long nextSequence = 1;

        public event Action<OfficerInitiativeRecord> RecordAdded;

        public IReadOnlyList<OfficerInitiativeRecord> Records => records.AsReadOnly();
        public bool HasCompleteConfiguration => officerIdentity != null
            && officerIdentity.Role == ActorRole.Officer;

        public void Configure(ActorIdentity configuredIdentity)
        {
            officerIdentity = configuredIdentity;
        }

        public OfficerInitiativeRecord Record(
            OfficerInitiativeEventType eventType,
            HumanActorController subject,
            TacticalRoomVolume room,
            VerbalCommandType? verbalCommand,
            string details)
        {
            officerIdentity ??= GetComponent<ActorIdentity>();
            if (!HasCompleteConfiguration)
            {
                throw new InvalidOperationException(
                    "OfficerInitiativeLedger requires an officer ActorIdentity.");
            }

            ActorIdentity subjectIdentity = subject != null ? subject.Identity : null;
            OfficerInitiativeRecord record = new OfficerInitiativeRecord(
                nextSequence++,
                Time.timeAsDouble,
                officerIdentity.ActorId,
                officerIdentity.RuntimeEntityId,
                eventType,
                subjectIdentity != null ? subjectIdentity.ActorId : string.Empty,
                subjectIdentity != null ? subjectIdentity.RuntimeEntityId : 0UL,
                room != null ? room.RoomId : string.Empty,
                verbalCommand,
                details);
            records.Add(record);
            RecordAdded?.Invoke(record);
            return record;
        }

        private void Awake()
        {
            officerIdentity ??= GetComponent<ActorIdentity>();
        }
    }
}
