using RulesOfEntry.Interaction;
using RulesOfEntry.Navigation;
using UnityEngine;

namespace RulesOfEntry.Operations
{
    [DisallowMultipleComponent]
    public sealed class OperationPortal : MonoBehaviour
    {
        [SerializeField] private string portalId = "portal_unassigned";
        [SerializeField] private OperationPortalType portalType;
        [SerializeField] private OperationRoomNode roomA;
        [SerializeField] private OperationRoomNode roomB;
        [SerializeField] private PrototypeDoor door;
        [SerializeField] private DoorTraversalLink traversalLink;

        public string PortalId => portalId;
        public OperationPortalType PortalType => portalType;
        public OperationRoomNode RoomA => roomA;
        public OperationRoomNode RoomB => roomB;
        public PrototypeDoor Door => door;
        public DoorTraversalLink TraversalLink => traversalLink;
        public bool IsTraversable => portalType == OperationPortalType.OpenPassage
            || (door != null && door.IsTraversalClear);
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(portalId)
            && roomA != null
            && roomB != null
            && roomA != roomB
            && roomA.HasValidConfiguration
            && roomB.HasValidConfiguration
            && (portalType == OperationPortalType.OpenPassage
                ? door == null && traversalLink == null
                : door != null
                    && traversalLink != null
                    && traversalLink.Door == door
                    && traversalLink.HasCompleteConfiguration);

        public void Configure(
            string configuredPortalId,
            OperationPortalType configuredPortalType,
            OperationRoomNode configuredRoomA,
            OperationRoomNode configuredRoomB,
            PrototypeDoor configuredDoor,
            DoorTraversalLink configuredTraversalLink)
        {
            portalId = string.IsNullOrWhiteSpace(configuredPortalId)
                ? "portal_unassigned"
                : configuredPortalId.Trim();
            portalType = configuredPortalType;
            roomA = configuredRoomA;
            roomB = configuredRoomB;
            door = configuredDoor;
            traversalLink = configuredTraversalLink;
        }

        public OperationRoomNode GetOtherRoom(OperationRoomNode room)
        {
            if (room == roomA)
            {
                return roomB;
            }

            return room == roomB ? roomA : null;
        }

        private void OnValidate()
        {
            portalId = portalId?.Trim() ?? string.Empty;
        }
    }
}
