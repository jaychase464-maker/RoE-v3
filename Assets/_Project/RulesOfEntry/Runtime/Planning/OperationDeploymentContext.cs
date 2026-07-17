using System;
using System.Collections.Generic;
using System.Linq;

namespace RulesOfEntry.Planning
{
    /// <summary>
    /// Carries only stable identifiers between the headquarters and operation
    /// scenes. Scene objects and ScriptableObjects never survive through this
    /// boundary.
    /// </summary>
    public static class OperationDeploymentContext
    {
        private static string[] assignedOfficerIds = Array.Empty<string>();
        private static string[] supportAssetIds = Array.Empty<string>();

        public static string MissionId { get; private set; } = string.Empty;
        public static string OperationCode { get; private set; } = string.Empty;
        public static string EntryPointId { get; private set; } = string.Empty;
        public static IReadOnlyList<string> AssignedOfficerIds => assignedOfficerIds;
        public static IReadOnlyList<string> SupportAssetIds => supportAssetIds;
        public static bool HasPendingDeployment => !string.IsNullOrWhiteSpace(MissionId)
            && !string.IsNullOrWhiteSpace(EntryPointId)
            && assignedOfficerIds.Length > 0;

        public static bool Confirm(
            OperationBriefingDefinition briefing,
            string entryPointId,
            IEnumerable<string> configuredOfficerIds,
            IEnumerable<string> configuredSupportIds)
        {
            string[] officers = Normalize(configuredOfficerIds);
            if (!OperationPlanningRules.CanDeploy(briefing, entryPointId, officers))
            {
                return false;
            }

            HashSet<string> permittedSupport = briefing.SupportAssets
                .Where(support => support.Available)
                .Select(support => support.SupportId)
                .ToHashSet(StringComparer.Ordinal);
            MissionId = briefing.Mission.MissionId;
            OperationCode = briefing.OperationCode;
            EntryPointId = entryPointId.Trim();
            assignedOfficerIds = officers;
            supportAssetIds = Normalize(configuredSupportIds)
                .Where(permittedSupport.Contains)
                .ToArray();
            return true;
        }

        public static void Clear()
        {
            MissionId = string.Empty;
            OperationCode = string.Empty;
            EntryPointId = string.Empty;
            assignedOfficerIds = Array.Empty<string>();
            supportAssetIds = Array.Empty<string>();
        }

        private static string[] Normalize(IEnumerable<string> values)
        {
            return values?
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>();
        }
    }
}
