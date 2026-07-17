using System;
using System.Linq;
using UnityEngine;

namespace RulesOfEntry.Missions
{
    [CreateAssetMenu(
        fileName = "ROE_MissionDefinition",
        menuName = "Rules of Entry/Missions/Mission Definition")]
    public sealed class MissionDefinition : ScriptableObject
    {
        [SerializeField] private string missionId = "mission_unassigned";
        [SerializeField] private string displayName = "Unassigned Mission";
        [SerializeField, TextArea(3, 8)] private string briefing = string.Empty;
        [SerializeField] private MissionObjectiveDefinition[] objectives =
            Array.Empty<MissionObjectiveDefinition>();

        public string MissionId => missionId;
        public string DisplayName => displayName;
        public string Briefing => briefing;
        public MissionObjectiveDefinition[] Objectives => objectives?.ToArray()
            ?? Array.Empty<MissionObjectiveDefinition>();
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(missionId)
            && objectives != null
            && objectives.Length > 0
            && objectives.All(objective => objective != null
                && !string.IsNullOrWhiteSpace(objective.ObjectiveId))
            && objectives.Select(objective => objective.ObjectiveId).Distinct().Count()
                == objectives.Length;

        public void Configure(
            string configuredMissionId,
            string configuredDisplayName,
            string configuredBriefing,
            MissionObjectiveDefinition[] configuredObjectives)
        {
            if (string.IsNullOrWhiteSpace(configuredMissionId))
            {
                throw new ArgumentException(
                    "Mission ID cannot be empty.",
                    nameof(configuredMissionId));
            }

            missionId = configuredMissionId.Trim();
            displayName = string.IsNullOrWhiteSpace(configuredDisplayName)
                ? missionId
                : configuredDisplayName.Trim();
            briefing = configuredBriefing?.Trim() ?? string.Empty;
            objectives = configuredObjectives?
                .Where(objective => objective != null)
                .ToArray() ?? Array.Empty<MissionObjectiveDefinition>();
        }
    }
}
