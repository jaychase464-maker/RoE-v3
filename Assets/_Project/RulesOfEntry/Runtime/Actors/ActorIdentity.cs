using System;
using UnityEngine;

namespace RulesOfEntry.Actors
{
    [DisallowMultipleComponent]
    public sealed class ActorIdentity : MonoBehaviour
    {
        [SerializeField] private string actorId = "actor_unassigned";
        [SerializeField] private string displayName = "Unknown Person";
        [SerializeField] private ActorRole role = ActorRole.Civilian;
        [SerializeField] private int incidentSeed = 1;

        public string ActorId => actorId;
        public string DisplayName => displayName;
        public ActorRole Role => role;
        public int IncidentSeed => incidentSeed;
        public ulong RuntimeEntityId => EntityId.ToULong(gameObject.GetEntityId());

        public void Configure(
            string configuredActorId,
            string configuredDisplayName,
            ActorRole configuredRole,
            int configuredIncidentSeed)
        {
            if (string.IsNullOrWhiteSpace(configuredActorId))
            {
                throw new ArgumentException("Actor ID cannot be empty.", nameof(configuredActorId));
            }

            actorId = configuredActorId.Trim();
            displayName = string.IsNullOrWhiteSpace(configuredDisplayName)
                ? "Unknown Person"
                : configuredDisplayName.Trim();
            role = configuredRole;
            incidentSeed = configuredIncidentSeed == 0 ? 1 : configuredIncidentSeed;
        }
    }
}
