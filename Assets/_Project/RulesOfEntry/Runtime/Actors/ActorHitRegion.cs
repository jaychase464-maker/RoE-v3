using UnityEngine;

namespace RulesOfEntry.Actors
{
    [DisallowMultipleComponent]
    public sealed class ActorHitRegion : MonoBehaviour
    {
        [SerializeField] private ActorHitRegionType region = ActorHitRegionType.Torso;

        public ActorHitRegionType Region => region;

        public float TraumaMultiplier => region switch
        {
            ActorHitRegionType.Head => 1.65f,
            ActorHitRegionType.Neck => 1.5f,
            ActorHitRegionType.Pelvis => 1.1f,
            ActorHitRegionType.Limb => 0.62f,
            _ => 1f
        };

        public float BleedingMultiplier => region switch
        {
            ActorHitRegionType.Neck => 1.8f,
            ActorHitRegionType.Pelvis => 1.35f,
            ActorHitRegionType.Head => 1.25f,
            ActorHitRegionType.Limb => 0.82f,
            _ => 1f
        };

        public float MobilityMultiplier => region switch
        {
            ActorHitRegionType.Pelvis => 1.55f,
            ActorHitRegionType.Limb => 1.35f,
            _ => 0.45f
        };

        public void Configure(ActorHitRegionType configuredRegion)
        {
            region = configuredRegion;
        }
    }
}
