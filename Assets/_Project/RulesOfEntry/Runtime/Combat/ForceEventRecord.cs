using UnityEngine;

namespace RulesOfEntry.Combat
{
    /// <summary>
    /// Immutable factual record of one projectile discharge. It contains no score or ROE judgment.
    /// </summary>
    public sealed class ForceEventRecord
    {
        public ForceEventRecord(
            long sequence,
            double occurredAtSeconds,
            int frame,
            ulong shooterEntityId,
            string firearmId,
            string ammunitionId,
            FireSelectorPosition selector,
            WeaponReadyPosition readyPosition,
            Vector3 origin,
            Vector3 direction,
            bool hit,
            Vector3 hitPoint,
            Vector3 hitNormal,
            ulong hitColliderEntityId,
            string hitObjectName,
            float muzzleEnergyJoules,
            FirearmSnapshot postShotSnapshot,
            ForceSubjectSnapshot subjectBeforeImpact)
        {
            Sequence = sequence;
            OccurredAtSeconds = occurredAtSeconds;
            Frame = frame;
            ShooterEntityId = shooterEntityId;
            FirearmId = firearmId;
            AmmunitionId = ammunitionId;
            Selector = selector;
            ReadyPosition = readyPosition;
            Origin = origin;
            Direction = direction;
            Hit = hit;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            HitColliderEntityId = hitColliderEntityId;
            HitObjectName = hitObjectName;
            MuzzleEnergyJoules = muzzleEnergyJoules;
            PostShotSnapshot = postShotSnapshot;
            SubjectBeforeImpact = subjectBeforeImpact;
        }

        public long Sequence { get; }
        public double OccurredAtSeconds { get; }
        public int Frame { get; }
        public ulong ShooterEntityId { get; }
        public string FirearmId { get; }
        public string AmmunitionId { get; }
        public FireSelectorPosition Selector { get; }
        public WeaponReadyPosition ReadyPosition { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public bool Hit { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public ulong HitColliderEntityId { get; }
        public string HitObjectName { get; }
        public float MuzzleEnergyJoules { get; }
        public FirearmSnapshot PostShotSnapshot { get; }
        public ForceSubjectSnapshot SubjectBeforeImpact { get; }
    }
}
