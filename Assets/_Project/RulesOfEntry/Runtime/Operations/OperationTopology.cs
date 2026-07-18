using System;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Deployment;
using UnityEngine;

namespace RulesOfEntry.Operations
{
    [Serializable]
    public sealed class OperationEntryRoomBinding
    {
        [SerializeField] private string entryPointId = "entry_unassigned";
        [SerializeField] private OperationRoomNode stagingRoom;

        public string EntryPointId => entryPointId;
        public OperationRoomNode StagingRoom => stagingRoom;
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(entryPointId)
            && stagingRoom != null
            && stagingRoom.HasValidConfiguration;

        public OperationEntryRoomBinding(
            string configuredEntryPointId,
            OperationRoomNode configuredStagingRoom)
        {
            entryPointId = configuredEntryPointId?.Trim() ?? string.Empty;
            stagingRoom = configuredStagingRoom;
        }
    }

    /// <summary>
    /// Scene-owned operation graph. It maps stable planning entry IDs to authored
    /// areas and exposes routes without deciding tactical behavior for AI.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OperationTopology : MonoBehaviour
    {
        [SerializeField] private OperationRoomNode[] rooms =
            Array.Empty<OperationRoomNode>();
        [SerializeField] private OperationPortal[] portals =
            Array.Empty<OperationPortal>();
        [SerializeField] private OperationEntryAnchor[] entryAnchors =
            Array.Empty<OperationEntryAnchor>();
        [SerializeField] private OperationEntryRoomBinding[] entryBindings =
            Array.Empty<OperationEntryRoomBinding>();

        public OperationRoomNode[] Rooms => rooms?.Where(room => room != null).ToArray()
            ?? Array.Empty<OperationRoomNode>();
        public OperationPortal[] Portals => portals?.Where(portal => portal != null).ToArray()
            ?? Array.Empty<OperationPortal>();
        public OperationEntryAnchor[] EntryAnchors => entryAnchors?
            .Where(anchor => anchor != null).ToArray()
            ?? Array.Empty<OperationEntryAnchor>();
        public OperationEntryRoomBinding[] EntryBindings => entryBindings?
            .Where(binding => binding != null).ToArray()
            ?? Array.Empty<OperationEntryRoomBinding>();
        public OperationTopologyValidation Validation => ValidateConfiguration();
        public bool HasCompleteConfiguration => Validation.IsValid;

        public void Configure(
            OperationRoomNode[] configuredRooms,
            OperationPortal[] configuredPortals,
            OperationEntryAnchor[] configuredEntryAnchors,
            OperationEntryRoomBinding[] configuredEntryBindings)
        {
            rooms = configuredRooms?.Where(room => room != null).Distinct().ToArray()
                ?? Array.Empty<OperationRoomNode>();
            portals = configuredPortals?.Where(portal => portal != null).Distinct().ToArray()
                ?? Array.Empty<OperationPortal>();
            entryAnchors = configuredEntryAnchors?
                .Where(anchor => anchor != null).Distinct().ToArray()
                ?? Array.Empty<OperationEntryAnchor>();
            entryBindings = configuredEntryBindings?
                .Where(binding => binding != null).ToArray()
                ?? Array.Empty<OperationEntryRoomBinding>();
        }

        public bool TryFindRoute(
            string entryPointId,
            string destinationRoomId,
            out string[] roomRoute)
        {
            roomRoute = Array.Empty<string>();
            OperationEntryRoomBinding binding = entryBindings?.FirstOrDefault(candidate =>
                candidate != null
                && string.Equals(
                    candidate.EntryPointId,
                    entryPointId,
                    StringComparison.Ordinal));
            if (binding?.StagingRoom == null)
            {
                return false;
            }

            roomRoute = OperationTopologyRules.FindShortestRoute(
                binding.StagingRoom.RoomId,
                destinationRoomId,
                BuildPortalRecords());
            return roomRoute.Length > 0;
        }

        public OperationRoomNode FindRoomContaining(Vector3 position)
        {
            return rooms?.FirstOrDefault(room => room != null
                && room.ClearanceVolume != null
                && room.ClearanceVolume.Contains(position));
        }

        private void Start()
        {
            OperationTopologyValidation validation = ValidateConfiguration();
            if (!validation.IsValid)
            {
                ProjectLog.Error(
                    "Operation Topology",
                    string.Join(" | ", validation.Errors),
                    this);
            }
        }

        private OperationTopologyValidation ValidateConfiguration()
        {
            OperationTopologyValidation rulesValidation = OperationTopologyRules.Validate(
                BuildRoomRecords(),
                BuildPortalRecords(),
                BuildEntryRecords());
            string[] componentErrors = rooms == null || portals == null
                || entryAnchors == null || entryBindings == null
                ? new[] { "Topology arrays cannot be null." }
                : rooms.Any(room => room == null || !room.HasValidConfiguration)
                    ? new[] { "One or more authored room nodes are incomplete." }
                    : portals.Any(portal => portal == null || !portal.HasValidConfiguration)
                        ? new[] { "One or more authored portals are incomplete." }
                        : entryAnchors.Any(anchor =>
                            anchor == null || !anchor.HasValidConfiguration)
                            ? new[] { "One or more deployment anchors are incomplete." }
                            : entryBindings.Any(binding =>
                                binding == null || !binding.HasValidConfiguration)
                                ? new[] { "One or more entry-to-room bindings are incomplete." }
                                : FindEntryMismatchErrors();
            return new OperationTopologyValidation(
                rulesValidation.Errors.Concat(componentErrors).ToArray());
        }

        private string[] FindEntryMismatchErrors()
        {
            string[] anchorIds = entryAnchors.Select(anchor => anchor.EntryPointId).ToArray();
            string[] bindingIds = entryBindings.Select(binding => binding.EntryPointId).ToArray();
            bool exactMatch = anchorIds.Length == bindingIds.Length
                && anchorIds.All(id => bindingIds.Contains(id, StringComparer.Ordinal));
            return exactMatch
                ? Array.Empty<string>()
                : new[] { "Deployment anchors and topology entry bindings must match exactly." };
        }

        private OperationRoomRecord[] BuildRoomRecords()
        {
            return rooms?.Where(room => room != null)
                .Select(room => new OperationRoomRecord(
                    room.RoomId,
                    room.RequiresClearance))
                .ToArray() ?? Array.Empty<OperationRoomRecord>();
        }

        private OperationPortalRecord[] BuildPortalRecords()
        {
            return portals?.Where(portal => portal != null)
                .Select(portal => new OperationPortalRecord(
                    portal.PortalId,
                    portal.RoomA?.RoomId,
                    portal.RoomB?.RoomId))
                .ToArray() ?? Array.Empty<OperationPortalRecord>();
        }

        private OperationEntryRecord[] BuildEntryRecords()
        {
            return entryBindings?.Where(binding => binding != null)
                .Select(binding => new OperationEntryRecord(
                    binding.EntryPointId,
                    binding.StagingRoom?.RoomId))
                .ToArray() ?? Array.Empty<OperationEntryRecord>();
        }

        private void OnValidate()
        {
            rooms ??= Array.Empty<OperationRoomNode>();
            portals ??= Array.Empty<OperationPortal>();
            entryAnchors ??= Array.Empty<OperationEntryAnchor>();
            entryBindings ??= Array.Empty<OperationEntryRoomBinding>();
        }
    }
}
