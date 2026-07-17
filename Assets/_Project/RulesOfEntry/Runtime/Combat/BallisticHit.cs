using UnityEngine;

namespace RulesOfEntry.Combat
{
    public readonly struct BallisticHit
    {
        public BallisticHit(
            GameObject shooter,
            string firearmId,
            string ammunitionId,
            Vector3 origin,
            Vector3 direction,
            Vector3 point,
            Vector3 normal,
            float muzzleEnergyJoules,
            Collider collider)
        {
            Shooter = shooter;
            FirearmId = firearmId;
            AmmunitionId = ammunitionId;
            Origin = origin;
            Direction = direction;
            Point = point;
            Normal = normal;
            MuzzleEnergyJoules = muzzleEnergyJoules;
            Collider = collider;
        }

        public GameObject Shooter { get; }
        public string FirearmId { get; }
        public string AmmunitionId { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public float MuzzleEnergyJoules { get; }
        public Collider Collider { get; }
    }

    public interface IBallisticHitReceiver
    {
        void ReceiveBallisticHit(BallisticHit hit);
    }
}
