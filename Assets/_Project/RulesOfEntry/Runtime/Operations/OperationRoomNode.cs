using RulesOfEntry.Officers;
using UnityEngine;

namespace RulesOfEntry.Operations
{
    [DisallowMultipleComponent]
    public sealed class OperationRoomNode : MonoBehaviour
    {
        [SerializeField] private string roomId = "room_unassigned";
        [SerializeField] private string displayName = "Unassigned Area";
        [SerializeField] private OperationAreaType areaType = OperationAreaType.InteriorRoom;
        [SerializeField] private bool requiresClearance = true;
        [SerializeField] private TacticalRoomVolume clearanceVolume;

        public string RoomId => roomId;
        public string DisplayName => displayName;
        public OperationAreaType AreaType => areaType;
        public bool RequiresClearance => requiresClearance;
        public TacticalRoomVolume ClearanceVolume => clearanceVolume;
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(roomId)
            && !string.IsNullOrWhiteSpace(displayName)
            && (!requiresClearance
                || (clearanceVolume != null
                    && clearanceVolume.HasCompleteConfiguration
                    && string.Equals(clearanceVolume.RoomId, roomId)));

        public void Configure(
            string configuredRoomId,
            string configuredDisplayName,
            OperationAreaType configuredAreaType,
            bool configuredRequiresClearance,
            TacticalRoomVolume configuredClearanceVolume)
        {
            roomId = string.IsNullOrWhiteSpace(configuredRoomId)
                ? "room_unassigned"
                : configuredRoomId.Trim();
            displayName = string.IsNullOrWhiteSpace(configuredDisplayName)
                ? roomId
                : configuredDisplayName.Trim();
            areaType = configuredAreaType;
            requiresClearance = configuredRequiresClearance;
            clearanceVolume = configuredClearanceVolume;
        }

        private void OnValidate()
        {
            roomId = roomId?.Trim() ?? string.Empty;
            displayName = displayName?.Trim() ?? string.Empty;
        }
    }
}
