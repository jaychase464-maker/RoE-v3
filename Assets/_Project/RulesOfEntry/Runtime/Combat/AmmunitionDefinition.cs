using UnityEngine;

namespace RulesOfEntry.Combat
{
    [CreateAssetMenu(
        fileName = "AmmunitionDefinition",
        menuName = "Rules of Entry/Combat/Ammunition Definition")]
    public sealed class AmmunitionDefinition : ScriptableObject
    {
        private const float GrainsToKilograms = 0.00006479891f;

        [SerializeField] private string ammunitionId = "roe_556_62gr";
        [SerializeField] private string displayName = "5.56x45 mm 62 gr";
        [SerializeField, Min(1f)] private float projectileMassGrains = 62f;
        [SerializeField, Min(1f)] private float muzzleVelocityMetersPerSecond = 850f;
        [SerializeField, Min(1f)] private float maximumSimulationRangeMeters = 500f;

        public string AmmunitionId => ammunitionId;
        public string DisplayName => displayName;
        public float ProjectileMassGrains => projectileMassGrains;
        public float MuzzleVelocityMetersPerSecond => muzzleVelocityMetersPerSecond;
        public float MaximumSimulationRangeMeters => maximumSimulationRangeMeters;
        public float MuzzleEnergyJoules
        {
            get
            {
                float massKilograms = projectileMassGrains * GrainsToKilograms;
                return 0.5f
                    * massKilograms
                    * muzzleVelocityMetersPerSecond
                    * muzzleVelocityMetersPerSecond;
            }
        }

        public void Configure(
            string configuredId,
            string configuredDisplayName,
            float massGrains,
            float velocityMetersPerSecond,
            float maximumRangeMeters)
        {
            ammunitionId = configuredId;
            displayName = configuredDisplayName;
            projectileMassGrains = Mathf.Max(1f, massGrains);
            muzzleVelocityMetersPerSecond = Mathf.Max(1f, velocityMetersPerSecond);
            maximumSimulationRangeMeters = Mathf.Max(1f, maximumRangeMeters);
        }

        private void OnValidate()
        {
            projectileMassGrains = Mathf.Max(1f, projectileMassGrains);
            muzzleVelocityMetersPerSecond = Mathf.Max(1f, muzzleVelocityMetersPerSecond);
            maximumSimulationRangeMeters = Mathf.Max(1f, maximumSimulationRangeMeters);
        }
    }
}
