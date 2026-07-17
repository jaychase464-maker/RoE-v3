using System;
using UnityEngine;

namespace RulesOfEntry.Officers
{
    /// <summary>
    /// Immutable command fact. Execution state is deliberately kept in a separate state machine.
    /// </summary>
    public sealed class OfficerOrder
    {
        public OfficerOrder(
            long commandSequence,
            ulong issuerEntityId,
            ulong officerEntityId,
            OfficerOrderType type,
            Vector3 targetPosition,
            UnityEngine.Object targetObject,
            ulong targetEntityId,
            double issuedAtSeconds,
            OfficerOrderOrigin origin = OfficerOrderOrigin.PlayerCommand)
        {
            if (commandSequence <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(commandSequence),
                    "Command sequence must be positive.");
            }

            if (officerEntityId == 0UL)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(officerEntityId),
                    "An order must identify its receiving officer.");
            }

            CommandSequence = commandSequence;
            IssuerEntityId = issuerEntityId;
            OfficerEntityId = officerEntityId;
            Type = type;
            TargetPosition = targetPosition;
            TargetObject = targetObject;
            TargetEntityId = targetEntityId;
            IssuedAtSeconds = issuedAtSeconds;
            Origin = origin;
        }

        public long CommandSequence { get; }
        public ulong IssuerEntityId { get; }
        public ulong OfficerEntityId { get; }
        public OfficerOrderType Type { get; }
        public Vector3 TargetPosition { get; }
        public UnityEngine.Object TargetObject { get; }
        public ulong TargetEntityId { get; }
        public double IssuedAtSeconds { get; }
        public OfficerOrderOrigin Origin { get; }

        public string TargetDescription
        {
            get
            {
                if (TargetObject != null)
                {
                    return TargetObject.name;
                }

                return $"({TargetPosition.x:0.0}, {TargetPosition.y:0.0}, {TargetPosition.z:0.0})";
            }
        }
    }
}
