using System;
using System.Linq;
using UnityEngine;

namespace RulesOfEntry.Deployment
{
    /// <summary>
    /// An authored, stable deployment location for an operation. Planning data
    /// refers to this component only through EntryPointId.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OperationEntryAnchor : MonoBehaviour
    {
        [SerializeField] private string entryPointId = "entry_unassigned";
        [SerializeField] private string displayName = "Unassigned Entry";
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform[] officerSpawns = Array.Empty<Transform>();

        public string EntryPointId => entryPointId;
        public string DisplayName => displayName;
        public Transform PlayerSpawn => playerSpawn;
        public Transform[] OfficerSpawns => officerSpawns?.ToArray()
            ?? Array.Empty<Transform>();
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(entryPointId)
            && !string.IsNullOrWhiteSpace(displayName)
            && playerSpawn != null
            && officerSpawns != null
            && officerSpawns.Length > 0
            && officerSpawns.All(spawn => spawn != null);

        public void Configure(
            string configuredEntryPointId,
            string configuredDisplayName,
            Transform configuredPlayerSpawn,
            Transform[] configuredOfficerSpawns)
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
            playerSpawn = configuredPlayerSpawn;
            officerSpawns = configuredOfficerSpawns?
                .Where(spawn => spawn != null)
                .Distinct()
                .ToArray() ?? Array.Empty<Transform>();
        }

        public Transform GetOfficerSpawn(int officerIndex)
        {
            if (officerSpawns == null || officerSpawns.Length == 0)
            {
                return null;
            }

            int normalized = Mathf.Abs(officerIndex) % officerSpawns.Length;
            return officerSpawns[normalized];
        }

        private void OnValidate()
        {
            entryPointId = entryPointId?.Trim() ?? string.Empty;
            displayName = displayName?.Trim() ?? string.Empty;
            officerSpawns ??= Array.Empty<Transform>();
        }
    }
}
