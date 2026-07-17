using System;
using UnityEngine;

namespace RulesOfEntry.Missions
{
    [Serializable]
    public sealed class MissionObjectiveDefinition
    {
        [SerializeField] private string objectiveId = "objective_unassigned";
        [SerializeField] private string displayName = "Unassigned Objective";
        [SerializeField, TextArea] private string briefing = string.Empty;
        [SerializeField] private MissionObjectiveType type;
        [SerializeField] private string targetActorId = string.Empty;
        [SerializeField] private string targetRoomId = string.Empty;
        [SerializeField] private bool isRequired = true;
        [SerializeField, Range(0, 100)] private int failureDeduction = 20;

        public string ObjectiveId => objectiveId;
        public string DisplayName => displayName;
        public string Briefing => briefing;
        public MissionObjectiveType Type => type;
        public string TargetActorId => targetActorId;
        public string TargetRoomId => targetRoomId;
        public bool Required => isRequired;
        public int FailureDeduction => failureDeduction;

        public void Configure(
            string configuredObjectiveId,
            string configuredDisplayName,
            string configuredBriefing,
            MissionObjectiveType configuredType,
            string configuredTargetActorId,
            string configuredTargetRoomId,
            bool configuredRequired,
            int configuredFailureDeduction)
        {
            if (string.IsNullOrWhiteSpace(configuredObjectiveId))
            {
                throw new ArgumentException(
                    "Objective ID cannot be empty.",
                    nameof(configuredObjectiveId));
            }

            objectiveId = configuredObjectiveId.Trim();
            displayName = string.IsNullOrWhiteSpace(configuredDisplayName)
                ? objectiveId
                : configuredDisplayName.Trim();
            briefing = configuredBriefing?.Trim() ?? string.Empty;
            type = configuredType;
            targetActorId = configuredTargetActorId?.Trim() ?? string.Empty;
            targetRoomId = configuredTargetRoomId?.Trim() ?? string.Empty;
            isRequired = configuredRequired;
            failureDeduction = Mathf.Clamp(configuredFailureDeduction, 0, 100);
        }
    }
}
