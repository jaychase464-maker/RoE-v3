using System;
using UnityEngine;

namespace RulesOfEntry.UI.TacticalHud
{
    /// <summary>
    /// Scene-facing projection of officer identity. Campaign persistence configures
    /// this component through a binder so the HUD never becomes a save-data authority.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BodyCameraIdentity : MonoBehaviour
    {
        [SerializeField] private string officerDisplayName = "A. Carter";
        [SerializeField] private string badgeIdentifier = "A127";
        [SerializeField] private string departmentName = "Calder City Police Department";
        [SerializeField, Range(0, 100)] private int batteryPercent = 97;
        [SerializeField] private bool recording = true;

        public event Action Changed;

        public string OfficerDisplayName => officerDisplayName;
        public string BadgeIdentifier => badgeIdentifier;
        public string DepartmentName => departmentName;
        public int BatteryPercent => batteryPercent;
        public bool Recording => recording;

        public void Configure(
            string configuredOfficerName,
            string configuredBadgeIdentifier,
            string configuredDepartmentName)
        {
            officerDisplayName = Normalize(configuredOfficerName, "UNASSIGNED OFFICER");
            badgeIdentifier = Normalize(configuredBadgeIdentifier, "NO BADGE");
            departmentName = Normalize(
                configuredDepartmentName,
                "CALDER CITY POLICE DEPARTMENT");
            Changed?.Invoke();
        }

        public void SetRecordingState(bool isRecording, int remainingBatteryPercent)
        {
            recording = isRecording;
            batteryPercent = Mathf.Clamp(remainingBatteryPercent, 0, 100);
            Changed?.Invoke();
        }

        private static string Normalize(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
