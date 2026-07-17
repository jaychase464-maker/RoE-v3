using System;
using RulesOfEntry.Combat;
using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.Actors
{
    /// <summary>
    /// Prototype physiological state. It intentionally exposes no arcade health value to UI.
    /// Terminal ballistics remain simplified until a validated wound model is implemented.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ActorCondition : MonoBehaviour, IBallisticHitReceiver
    {
        private const float InitialBloodVolumeLiters = 5f;

        [SerializeField, Min(0f)] private float bloodVolumeLiters = InitialBloodVolumeLiters;
        [SerializeField, Min(0f)] private float bleedingLitersPerMinute;
        [SerializeField, Range(0f, 1f)] private float consciousness = 1f;
        [SerializeField, Range(0f, 1f)] private float mobility = 1f;
        [SerializeField] private ActorConditionLevel level = ActorConditionLevel.Stable;

        private float lastPublishedBloodVolume = InitialBloodVolumeLiters;

        public event Action<ActorConditionSnapshot> ConditionChanged;

        public ActorConditionSnapshot Snapshot => new ActorConditionSnapshot(
            level,
            bloodVolumeLiters,
            bleedingLitersPerMinute,
            consciousness,
            mobility);

        public void ReceiveBallisticHit(BallisticHit hit)
        {
            if (level == ActorConditionLevel.Deceased)
            {
                return;
            }

            ActorHitRegion region = hit.Collider != null
                ? hit.Collider.GetComponent<ActorHitRegion>()
                : null;
            float traumaMultiplier = region != null ? region.TraumaMultiplier : 1f;
            float bleedingMultiplier = region != null ? region.BleedingMultiplier : 1f;
            float mobilityMultiplier = region != null ? region.MobilityMultiplier : 0.45f;
            float energyFactor = Mathf.Clamp(hit.MuzzleEnergyJoules / 2400f, 0.08f, 1.25f);
            float trauma = energyFactor * traumaMultiplier;

            consciousness = Mathf.Clamp01(consciousness - trauma * 0.52f);
            mobility = Mathf.Clamp01(mobility - trauma * 0.42f * mobilityMultiplier);
            bleedingLitersPerMinute = Mathf.Clamp(
                bleedingLitersPerMinute + trauma * 0.28f * bleedingMultiplier,
                0f,
                2.5f);

            if (region != null
                && (region.Region == ActorHitRegionType.Head
                    || region.Region == ActorHitRegionType.Neck)
                && trauma >= 0.9f)
            {
                consciousness = 0f;
            }

            RecalculateLevel();
            PublishChange();
            ProjectLog.Development(
                "Actor Condition",
                $"{name} sustained a {region?.Region.ToString() ?? "unspecified"} ballistic injury; condition is now {level}.",
                this);
        }

        public void StabilizeBleeding(float effectiveness01)
        {
            float effectiveness = Mathf.Clamp01(effectiveness01);
            bleedingLitersPerMinute *= 1f - effectiveness;
            PublishChange();
        }

        public void ResetCondition()
        {
            bloodVolumeLiters = InitialBloodVolumeLiters;
            bleedingLitersPerMinute = 0f;
            consciousness = 1f;
            mobility = 1f;
            level = ActorConditionLevel.Stable;
            PublishChange();
        }

        private void Update()
        {
            if (bleedingLitersPerMinute <= 0f || level == ActorConditionLevel.Deceased)
            {
                return;
            }

            ActorConditionLevel previousLevel = level;
            bloodVolumeLiters = Mathf.Max(
                0f,
                bloodVolumeLiters - bleedingLitersPerMinute / 60f * Time.deltaTime);
            consciousness = Mathf.Min(
                consciousness,
                Mathf.InverseLerp(2.1f, 4.1f, bloodVolumeLiters));
            RecalculateLevel();

            if (previousLevel != level
                || Mathf.Abs(lastPublishedBloodVolume - bloodVolumeLiters) >= 0.02f)
            {
                PublishChange();
            }
        }

        private void RecalculateLevel()
        {
            if (bloodVolumeLiters <= 1.8f)
            {
                level = ActorConditionLevel.Deceased;
                consciousness = 0f;
                mobility = 0f;
            }
            else if (consciousness <= 0.2f || mobility <= 0.12f || bloodVolumeLiters <= 2.7f)
            {
                level = ActorConditionLevel.Incapacitated;
            }
            else if (bleedingLitersPerMinute > 0f
                || consciousness < 0.98f
                || mobility < 0.98f)
            {
                level = ActorConditionLevel.Wounded;
            }
            else
            {
                level = ActorConditionLevel.Stable;
            }
        }

        private void PublishChange()
        {
            lastPublishedBloodVolume = bloodVolumeLiters;
            ConditionChanged?.Invoke(Snapshot);
        }
    }
}
