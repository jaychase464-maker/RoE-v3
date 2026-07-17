using System;
using UnityEngine;

namespace RulesOfEntry.Missions
{
    [CreateAssetMenu(
        fileName = "ROE_RulesOfEngagementPolicy",
        menuName = "Rules of Entry/Missions/Rules of Engagement Policy")]
    public sealed class RulesOfEngagementPolicy : ScriptableObject
    {
        [SerializeField] private string policyId = "roe_unassigned";
        [SerializeField] private string displayName = "Unassigned ROE";
        [SerializeField, TextArea(3, 8)] private string policySummary = string.Empty;
        [SerializeField, Range(0, 100)] private int minorViolationDeduction = 5;
        [SerializeField, Range(0, 100)] private int seriousViolationDeduction = 20;
        [SerializeField, Range(0, 100)] private int criticalViolationDeduction = 45;
        [SerializeField, Range(0, 100)] private int criticalScoreCap = 59;

        public string PolicyId => policyId;
        public string DisplayName => displayName;
        public string PolicySummary => policySummary;
        public int MinorViolationDeduction => minorViolationDeduction;
        public int SeriousViolationDeduction => seriousViolationDeduction;
        public int CriticalViolationDeduction => criticalViolationDeduction;
        public int CriticalScoreCap => criticalScoreCap;
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(policyId)
            && minorViolationDeduction >= 0
            && seriousViolationDeduction >= minorViolationDeduction
            && criticalViolationDeduction >= seriousViolationDeduction
            && criticalScoreCap >= 0
            && criticalScoreCap <= 100;

        public void Configure(
            string configuredPolicyId,
            string configuredDisplayName,
            string configuredSummary,
            int configuredMinorDeduction,
            int configuredSeriousDeduction,
            int configuredCriticalDeduction,
            int configuredCriticalScoreCap)
        {
            if (string.IsNullOrWhiteSpace(configuredPolicyId))
            {
                throw new ArgumentException(
                    "ROE policy ID cannot be empty.",
                    nameof(configuredPolicyId));
            }

            policyId = configuredPolicyId.Trim();
            displayName = string.IsNullOrWhiteSpace(configuredDisplayName)
                ? policyId
                : configuredDisplayName.Trim();
            policySummary = configuredSummary?.Trim() ?? string.Empty;
            minorViolationDeduction = Mathf.Clamp(configuredMinorDeduction, 0, 100);
            seriousViolationDeduction = Mathf.Clamp(
                configuredSeriousDeduction,
                minorViolationDeduction,
                100);
            criticalViolationDeduction = Mathf.Clamp(
                configuredCriticalDeduction,
                seriousViolationDeduction,
                100);
            criticalScoreCap = Mathf.Clamp(configuredCriticalScoreCap, 0, 100);
        }

        public int GetViolationDeduction(RoeSeverity severity)
        {
            return severity switch
            {
                RoeSeverity.Minor => minorViolationDeduction,
                RoeSeverity.Serious => seriousViolationDeduction,
                RoeSeverity.Critical => criticalViolationDeduction,
                _ => 0
            };
        }
    }
}
