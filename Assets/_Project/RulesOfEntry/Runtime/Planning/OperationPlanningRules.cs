using System;
using System.Collections.Generic;
using System.Linq;

namespace RulesOfEntry.Planning
{
    public static class OperationPlanningRules
    {
        public static int WrapIndex(int currentIndex, int direction, int count)
        {
            if (count <= 0)
            {
                return -1;
            }

            int normalized = currentIndex % count;
            if (normalized < 0)
            {
                normalized += count;
            }

            int next = (normalized + direction) % count;
            return next < 0 ? next + count : next;
        }

        public static int FindEntryIndex(
            IReadOnlyList<OperationEntryPointDefinition> entries,
            string entryPointId)
        {
            if (entries == null || string.IsNullOrWhiteSpace(entryPointId))
            {
                return -1;
            }

            for (int index = 0; index < entries.Count; index++)
            {
                if (string.Equals(
                    entries[index]?.EntryPointId,
                    entryPointId,
                    StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        public static bool CanDeploy(
            OperationBriefingDefinition briefing,
            string entryPointId,
            IEnumerable<string> assignedOfficerIds)
        {
            if (briefing == null || !briefing.HasValidConfiguration)
            {
                return false;
            }

            bool hasEntry = FindEntryIndex(briefing.EntryPoints, entryPointId) >= 0;
            HashSet<string> availableOfficers = briefing.Officers
                .Where(officer => officer.Available)
                .Select(officer => officer.OfficerId)
                .ToHashSet(StringComparer.Ordinal);
            bool hasAssignedOfficer = assignedOfficerIds != null
                && assignedOfficerIds.Any(id => !string.IsNullOrWhiteSpace(id)
                    && availableOfficers.Contains(id));
            return hasEntry && hasAssignedOfficer;
        }
    }
}
