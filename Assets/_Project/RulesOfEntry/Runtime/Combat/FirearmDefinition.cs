using UnityEngine;

namespace RulesOfEntry.Combat
{
    [CreateAssetMenu(
        fileName = "FirearmDefinition",
        menuName = "Rules of Entry/Combat/Firearm Definition")]
    public sealed class FirearmDefinition : ScriptableObject
    {
        [SerializeField] private string firearmId = "roe_patrol_carbine";
        [SerializeField] private string displayName = "Patrol Carbine";
        [SerializeField, Min(1)] private int magazineCapacity = 30;
        [SerializeField, Min(0.05f)] private float minimumSecondsBetweenShots = 0.12f;
        [SerializeField, Min(0.1f)] private float retainedReloadDuration = 2.35f;
        [SerializeField, Min(0.1f)] private float emergencyReloadDuration = 1.8f;
        [SerializeField, Min(0.1f)] private float magazineCheckDuration = 1.25f;
        [SerializeField, Min(0.1f)] private float cycleActionDuration = 0.85f;
        [SerializeField, Min(0.05f)] private float weaponRaiseDuration = 0.28f;
        [SerializeField, Min(0f)] private float verticalRecoilDegrees = 1.15f;
        [SerializeField, Min(0f)] private float horizontalRecoilDegrees = 0.3f;

        public string FirearmId => firearmId;
        public string DisplayName => displayName;
        public int MagazineCapacity => magazineCapacity;
        public float MinimumSecondsBetweenShots => minimumSecondsBetweenShots;
        public float RetainedReloadDuration => retainedReloadDuration;
        public float EmergencyReloadDuration => emergencyReloadDuration;
        public float MagazineCheckDuration => magazineCheckDuration;
        public float CycleActionDuration => cycleActionDuration;
        public float WeaponRaiseDuration => weaponRaiseDuration;
        public float VerticalRecoilDegrees => verticalRecoilDegrees;
        public float HorizontalRecoilDegrees => horizontalRecoilDegrees;

        public void Configure(
            string configuredId,
            string configuredDisplayName,
            int configuredMagazineCapacity,
            float configuredMinimumSecondsBetweenShots,
            float configuredRetainedReloadDuration,
            float configuredEmergencyReloadDuration,
            float configuredMagazineCheckDuration,
            float configuredCycleActionDuration,
            float configuredWeaponRaiseDuration,
            float configuredVerticalRecoil,
            float configuredHorizontalRecoil)
        {
            firearmId = configuredId;
            displayName = configuredDisplayName;
            magazineCapacity = Mathf.Max(1, configuredMagazineCapacity);
            minimumSecondsBetweenShots = Mathf.Max(0.05f, configuredMinimumSecondsBetweenShots);
            retainedReloadDuration = Mathf.Max(0.1f, configuredRetainedReloadDuration);
            emergencyReloadDuration = Mathf.Max(0.1f, configuredEmergencyReloadDuration);
            magazineCheckDuration = Mathf.Max(0.1f, configuredMagazineCheckDuration);
            cycleActionDuration = Mathf.Max(0.1f, configuredCycleActionDuration);
            weaponRaiseDuration = Mathf.Max(0.05f, configuredWeaponRaiseDuration);
            verticalRecoilDegrees = Mathf.Max(0f, configuredVerticalRecoil);
            horizontalRecoilDegrees = Mathf.Max(0f, configuredHorizontalRecoil);
        }

        private void OnValidate()
        {
            magazineCapacity = Mathf.Max(1, magazineCapacity);
            minimumSecondsBetweenShots = Mathf.Max(0.05f, minimumSecondsBetweenShots);
            retainedReloadDuration = Mathf.Max(0.1f, retainedReloadDuration);
            emergencyReloadDuration = Mathf.Max(0.1f, emergencyReloadDuration);
            magazineCheckDuration = Mathf.Max(0.1f, magazineCheckDuration);
            cycleActionDuration = Mathf.Max(0.1f, cycleActionDuration);
            weaponRaiseDuration = Mathf.Max(0.05f, weaponRaiseDuration);
            verticalRecoilDegrees = Mathf.Max(0f, verticalRecoilDegrees);
            horizontalRecoilDegrees = Mathf.Max(0f, horizontalRecoilDegrees);
        }
    }
}
