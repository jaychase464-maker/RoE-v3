using System;
using UnityEngine;

namespace RulesOfEntry.Planning
{
    public enum OperationSupportType
    {
        K9 = 0,
        Drone = 1,
        TacticalMedic = 2,
        BreachingElement = 3,
        Negotiator = 4,
        SniperObserver = 5
    }

    [Serializable]
    public sealed class OperationSupportDefinition
    {
        [SerializeField] private string supportId = "support_unassigned";
        [SerializeField] private string displayName = "Unassigned Support";
        [SerializeField] private OperationSupportType type;
        [SerializeField, TextArea(2, 4)] private string capability = string.Empty;
        [SerializeField] private bool implemented;
        [SerializeField] private bool available;

        public string SupportId => supportId;
        public string DisplayName => displayName;
        public OperationSupportType Type => type;
        public string Capability => capability;
        public bool Implemented => implemented;
        public bool Available => implemented && available;
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(supportId)
            && !string.IsNullOrWhiteSpace(displayName);

        public void Configure(
            string configuredSupportId,
            string configuredDisplayName,
            OperationSupportType configuredType,
            string configuredCapability,
            bool configuredImplemented,
            bool configuredAvailable)
        {
            if (string.IsNullOrWhiteSpace(configuredSupportId))
            {
                throw new ArgumentException(
                    "Support ID cannot be empty.",
                    nameof(configuredSupportId));
            }

            supportId = configuredSupportId.Trim();
            displayName = string.IsNullOrWhiteSpace(configuredDisplayName)
                ? supportId
                : configuredDisplayName.Trim();
            type = configuredType;
            capability = configuredCapability?.Trim() ?? string.Empty;
            implemented = configuredImplemented;
            available = configuredImplemented && configuredAvailable;
        }
    }
}
