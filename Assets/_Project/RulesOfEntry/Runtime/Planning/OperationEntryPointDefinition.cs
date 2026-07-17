using System;
using UnityEngine;

namespace RulesOfEntry.Planning
{
    [Serializable]
    public sealed class OperationEntryPointDefinition
    {
        [SerializeField] private string entryPointId = "entry_unassigned";
        [SerializeField] private string displayName = "Unassigned Entry";
        [SerializeField, TextArea(2, 5)] private string approach = string.Empty;
        [SerializeField, TextArea(2, 5)] private string risk = string.Empty;

        public string EntryPointId => entryPointId;
        public string DisplayName => displayName;
        public string Approach => approach;
        public string Risk => risk;
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(entryPointId)
            && !string.IsNullOrWhiteSpace(displayName);

        public void Configure(
            string configuredEntryPointId,
            string configuredDisplayName,
            string configuredApproach,
            string configuredRisk)
        {
            if (string.IsNullOrWhiteSpace(configuredEntryPointId))
            {
                throw new ArgumentException(
                    "Entry-point ID cannot be empty.",
                    nameof(configuredEntryPointId));
            }

            entryPointId = configuredEntryPointId.Trim();
            displayName = string.IsNullOrWhiteSpace(configuredDisplayName)
                ? entryPointId
                : configuredDisplayName.Trim();
            approach = configuredApproach?.Trim() ?? string.Empty;
            risk = configuredRisk?.Trim() ?? string.Empty;
        }
    }
}
