using System;
using UnityEngine;

namespace RulesOfEntry.Officers
{
    public enum OfficerAmmunitionCondition
    {
        Good = 0,
        Low = 1,
        Critical = 2
    }

    /// <summary>
    /// Qualitative ammunition feed for the squad HUD. It intentionally exposes
    /// magazine condition rather than precise round counts.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OfficerAmmunitionStatus : MonoBehaviour
    {
        [SerializeField, Min(0)] private int usableMagazineCount = 4;
        [SerializeField, Min(0)] private int lowMagazineThreshold = 1;

        public event Action StatusChanged;

        public int UsableMagazineCount => usableMagazineCount;
        public OfficerAmmunitionCondition Condition => EvaluateCondition(
            usableMagazineCount,
            lowMagazineThreshold);

        public void Configure(int configuredUsableMagazines, int configuredLowThreshold = 1)
        {
            usableMagazineCount = Mathf.Max(0, configuredUsableMagazines);
            lowMagazineThreshold = Mathf.Max(0, configuredLowThreshold);
            StatusChanged?.Invoke();
        }

        public void SetUsableMagazineCount(int count)
        {
            int normalized = Mathf.Max(0, count);
            if (usableMagazineCount == normalized)
            {
                return;
            }

            usableMagazineCount = normalized;
            StatusChanged?.Invoke();
        }

        public static OfficerAmmunitionCondition EvaluateCondition(
            int availableMagazines,
            int lowThreshold = 1)
        {
            int normalizedCount = Mathf.Max(0, availableMagazines);
            if (normalizedCount == 0)
            {
                return OfficerAmmunitionCondition.Critical;
            }

            return normalizedCount <= Mathf.Max(0, lowThreshold)
                ? OfficerAmmunitionCondition.Low
                : OfficerAmmunitionCondition.Good;
        }
    }
}
