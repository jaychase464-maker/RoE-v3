using System;

namespace RulesOfEntry.Operations
{
    public enum OperationAreaType
    {
        ExteriorStaging = 0,
        InteriorRoom = 1,
        Corridor = 2,
        Utility = 3
    }

    public enum OperationPortalType
    {
        OpenPassage = 0,
        InteriorDoor = 1,
        ExteriorDoor = 2
    }

    public enum OperationScenarioSeedMode
    {
        Fixed = 0,
        NewSession = 1
    }

    public readonly struct OperationRoomRecord
    {
        public OperationRoomRecord(string roomId, bool requiresClearance)
        {
            RoomId = roomId?.Trim() ?? string.Empty;
            RequiresClearance = requiresClearance;
        }

        public string RoomId { get; }
        public bool RequiresClearance { get; }
    }

    public readonly struct OperationPortalRecord
    {
        public OperationPortalRecord(
            string portalId,
            string roomAId,
            string roomBId)
        {
            PortalId = portalId?.Trim() ?? string.Empty;
            RoomAId = roomAId?.Trim() ?? string.Empty;
            RoomBId = roomBId?.Trim() ?? string.Empty;
        }

        public string PortalId { get; }
        public string RoomAId { get; }
        public string RoomBId { get; }
    }

    public readonly struct OperationEntryRecord
    {
        public OperationEntryRecord(string entryPointId, string roomId)
        {
            EntryPointId = entryPointId?.Trim() ?? string.Empty;
            RoomId = roomId?.Trim() ?? string.Empty;
        }

        public string EntryPointId { get; }
        public string RoomId { get; }
    }

    public readonly struct OperationSpawnRecord
    {
        public OperationSpawnRecord(
            string spawnPointId,
            int actorRole,
            string roomId,
            float weight)
        {
            SpawnPointId = spawnPointId?.Trim() ?? string.Empty;
            ActorRole = actorRole;
            RoomId = roomId?.Trim() ?? string.Empty;
            Weight = Math.Max(0.01f, weight);
        }

        public string SpawnPointId { get; }
        public int ActorRole { get; }
        public string RoomId { get; }
        public float Weight { get; }
    }

    public sealed class OperationTopologyValidation
    {
        public OperationTopologyValidation(string[] errors)
        {
            Errors = errors ?? Array.Empty<string>();
        }

        public string[] Errors { get; }
        public bool IsValid => Errors.Length == 0;
    }
}
