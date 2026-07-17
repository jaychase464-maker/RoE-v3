using System;
using System.Collections.Generic;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using UnityEngine;

namespace RulesOfEntry.Combat
{
    [DisallowMultipleComponent]
    public sealed class UseOfForceEventLedger : MonoBehaviour
    {
        private readonly List<ForceEventRecord> records = new List<ForceEventRecord>();
        private long nextSequence = 1;

        public event Action<ForceEventRecord> RecordAdded;

        public IReadOnlyList<ForceEventRecord> Records => records.AsReadOnly();

        public ForceEventRecord RecordFirearmDischarge(
            GameObject shooter,
            FirearmDefinition firearm,
            AmmunitionDefinition ammunition,
            WeaponReadyPosition readyPosition,
            Vector3 origin,
            Vector3 direction,
            RaycastHit? hit,
            FirearmSnapshot postShotSnapshot)
        {
            if (shooter == null)
            {
                throw new ArgumentNullException(nameof(shooter));
            }

            if (firearm == null)
            {
                throw new ArgumentNullException(nameof(firearm));
            }

            if (ammunition == null)
            {
                throw new ArgumentNullException(nameof(ammunition));
            }

            bool hasHit = hit.HasValue;
            RaycastHit hitValue = hasHit ? hit.Value : default;
            Collider collider = hasHit ? hitValue.collider : null;
            ForceSubjectSnapshot subjectBeforeImpact = CreateSubjectSnapshot(collider);
            ForceEventRecord record = new ForceEventRecord(
                nextSequence++,
                Time.timeAsDouble,
                Time.frameCount,
                EntityId.ToULong(shooter.GetEntityId()),
                firearm.FirearmId,
                ammunition.AmmunitionId,
                postShotSnapshot.Selector,
                readyPosition,
                origin,
                direction,
                hasHit,
                hasHit ? hitValue.point : Vector3.zero,
                hasHit ? hitValue.normal : Vector3.zero,
                collider != null ? EntityId.ToULong(collider.GetEntityId()) : 0UL,
                collider != null ? collider.gameObject.name : string.Empty,
                ammunition.MuzzleEnergyJoules,
                postShotSnapshot,
                subjectBeforeImpact);

            records.Add(record);
            RecordAdded?.Invoke(record);
            return record;
        }

        private static ForceSubjectSnapshot CreateSubjectSnapshot(Collider collider)
        {
            if (collider == null)
            {
                return ForceSubjectSnapshot.None;
            }

            ActorIdentity identity = collider.GetComponentInParent<ActorIdentity>();
            if (identity == null)
            {
                return ForceSubjectSnapshot.None;
            }

            ActorCondition condition = identity.GetComponent<ActorCondition>();
            CustodyComponent custody = identity.GetComponent<CustodyComponent>();
            HumanActorController behavior = identity.GetComponent<HumanActorController>();
            ActorInventory inventory = identity.GetComponent<ActorInventory>();
            return new ForceSubjectSnapshot(
                true,
                identity.ActorId,
                identity.Role,
                condition != null
                    ? condition.Snapshot.Level
                    : ActorConditionLevel.Stable,
                custody != null ? custody.State : CustodyState.Free,
                behavior != null ? behavior.State : HumanBehaviorState.Idle,
                inventory != null && inventory.HasWeapon);
        }
    }
}
