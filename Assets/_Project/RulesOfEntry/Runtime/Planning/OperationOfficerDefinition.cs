using System;
using UnityEngine;

namespace RulesOfEntry.Planning
{
    [Serializable]
    public sealed class OperationOfficerDefinition
    {
        [SerializeField] private string officerId = "officer_unassigned";
        [SerializeField] private string displayName = "Unassigned Officer";
        [SerializeField] private string role = "Operator";
        [SerializeField] private string qualification = "General Tactical";
        [SerializeField] private bool available = true;
        [SerializeField] private bool assignedByDefault = true;

        public string OfficerId => officerId;
        public string DisplayName => displayName;
        public string Role => role;
        public string Qualification => qualification;
        public bool Available => available;
        public bool AssignedByDefault => assignedByDefault;
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(officerId)
            && !string.IsNullOrWhiteSpace(displayName);

        public void Configure(
            string configuredOfficerId,
            string configuredDisplayName,
            string configuredRole,
            string configuredQualification,
            bool configuredAvailable,
            bool configuredAssignedByDefault)
        {
            if (string.IsNullOrWhiteSpace(configuredOfficerId))
            {
                throw new ArgumentException(
                    "Officer ID cannot be empty.",
                    nameof(configuredOfficerId));
            }

            officerId = configuredOfficerId.Trim();
            displayName = string.IsNullOrWhiteSpace(configuredDisplayName)
                ? officerId
                : configuredDisplayName.Trim();
            role = configuredRole?.Trim() ?? string.Empty;
            qualification = configuredQualification?.Trim() ?? string.Empty;
            available = configuredAvailable;
            assignedByDefault = configuredAvailable && configuredAssignedByDefault;
        }
    }
}
