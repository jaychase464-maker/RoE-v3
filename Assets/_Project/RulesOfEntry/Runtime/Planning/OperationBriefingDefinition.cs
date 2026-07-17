using System;
using System.Linq;
using RulesOfEntry.Missions;
using UnityEngine;

namespace RulesOfEntry.Planning
{
    [CreateAssetMenu(
        fileName = "ROE_OperationBriefing",
        menuName = "Rules of Entry/Planning/Operation Briefing")]
    public sealed class OperationBriefingDefinition : ScriptableObject
    {
        [SerializeField] private string operationCode = "UNASSIGNED";
        [SerializeField] private MissionDefinition mission;
        [SerializeField] private string scenePath = string.Empty;
        [SerializeField] private string location = "Unknown Location";
        [SerializeField] private string incidentType = "Unknown Incident";
        [SerializeField] private string time = "Unknown";
        [SerializeField] private string conditions = "Unknown";
        [SerializeField, TextArea(4, 10)] private string intelligence = string.Empty;
        [SerializeField, TextArea(2, 6)] private string legalAuthority = string.Empty;
        [SerializeField, TextArea(2, 6)] private string rulesOfEngagement = string.Empty;
        [SerializeField] private OperationEntryPointDefinition[] entryPoints =
            Array.Empty<OperationEntryPointDefinition>();
        [SerializeField] private OperationOfficerDefinition[] officers =
            Array.Empty<OperationOfficerDefinition>();
        [SerializeField] private OperationSupportDefinition[] supportAssets =
            Array.Empty<OperationSupportDefinition>();

        public string OperationCode => operationCode;
        public MissionDefinition Mission => mission;
        public string ScenePath => scenePath;
        public string Location => location;
        public string IncidentType => incidentType;
        public string Time => time;
        public string Conditions => conditions;
        public string Intelligence => intelligence;
        public string LegalAuthority => legalAuthority;
        public string RulesOfEngagement => rulesOfEngagement;
        public OperationEntryPointDefinition[] EntryPoints => entryPoints?.ToArray()
            ?? Array.Empty<OperationEntryPointDefinition>();
        public OperationOfficerDefinition[] Officers => officers?.ToArray()
            ?? Array.Empty<OperationOfficerDefinition>();
        public OperationSupportDefinition[] SupportAssets => supportAssets?.ToArray()
            ?? Array.Empty<OperationSupportDefinition>();
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(operationCode)
            && mission != null
            && mission.HasValidConfiguration
            && !string.IsNullOrWhiteSpace(scenePath)
            && entryPoints != null
            && entryPoints.Length > 0
            && entryPoints.All(entry => entry != null && entry.HasValidConfiguration)
            && entryPoints.Select(entry => entry.EntryPointId).Distinct().Count()
                == entryPoints.Length
            && officers != null
            && officers.Any(officer => officer != null && officer.Available)
            && officers.All(officer => officer != null && officer.HasValidConfiguration)
            && officers.Select(officer => officer.OfficerId).Distinct().Count()
                == officers.Length
            && supportAssets != null
            && supportAssets.Length > 0
            && supportAssets.All(support => support != null
                && support.HasValidConfiguration)
            && supportAssets.Select(support => support.SupportId).Distinct().Count()
                == supportAssets.Length;

        public void Configure(
            string configuredOperationCode,
            MissionDefinition configuredMission,
            string configuredScenePath,
            string configuredLocation,
            string configuredIncidentType,
            string configuredTime,
            string configuredConditions,
            string configuredIntelligence,
            string configuredLegalAuthority,
            string configuredRulesOfEngagement,
            OperationEntryPointDefinition[] configuredEntryPoints,
            OperationOfficerDefinition[] configuredOfficers,
            OperationSupportDefinition[] configuredSupportAssets)
        {
            if (configuredMission == null)
            {
                throw new ArgumentNullException(nameof(configuredMission));
            }

            if (string.IsNullOrWhiteSpace(configuredScenePath))
            {
                throw new ArgumentException(
                    "Operation scene path cannot be empty.",
                    nameof(configuredScenePath));
            }

            operationCode = string.IsNullOrWhiteSpace(configuredOperationCode)
                ? configuredMission.MissionId.ToUpperInvariant()
                : configuredOperationCode.Trim().ToUpperInvariant();
            mission = configuredMission;
            scenePath = configuredScenePath.Trim();
            location = configuredLocation?.Trim() ?? string.Empty;
            incidentType = configuredIncidentType?.Trim() ?? string.Empty;
            time = configuredTime?.Trim() ?? string.Empty;
            conditions = configuredConditions?.Trim() ?? string.Empty;
            intelligence = configuredIntelligence?.Trim() ?? string.Empty;
            legalAuthority = configuredLegalAuthority?.Trim() ?? string.Empty;
            rulesOfEngagement = configuredRulesOfEngagement?.Trim() ?? string.Empty;
            entryPoints = configuredEntryPoints?
                .Where(entry => entry != null)
                .ToArray() ?? Array.Empty<OperationEntryPointDefinition>();
            officers = configuredOfficers?
                .Where(officer => officer != null)
                .ToArray() ?? Array.Empty<OperationOfficerDefinition>();
            supportAssets = configuredSupportAssets?
                .Where(support => support != null)
                .ToArray() ?? Array.Empty<OperationSupportDefinition>();
        }
    }
}
