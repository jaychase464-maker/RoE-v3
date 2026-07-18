using RulesOfEntry.Actors;
using UnityEngine;

namespace RulesOfEntry.Operations
{
    [DisallowMultipleComponent]
    public sealed class OperationSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnPointId = "spawn_unassigned";
        [SerializeField] private ActorRole actorRole = ActorRole.Suspect;
        [SerializeField] private OperationRoomNode room;
        [SerializeField, Min(0.01f)] private float selectionWeight = 1f;

        public string SpawnPointId => spawnPointId;
        public ActorRole ActorRole => actorRole;
        public OperationRoomNode Room => room;
        public float SelectionWeight => selectionWeight;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
        public bool HasValidConfiguration => !string.IsNullOrWhiteSpace(spawnPointId)
            && actorRole != ActorRole.Officer
            && room != null
            && room.HasValidConfiguration
            && selectionWeight >= 0.01f;

        public void Configure(
            string configuredSpawnPointId,
            ActorRole configuredActorRole,
            OperationRoomNode configuredRoom,
            float configuredSelectionWeight = 1f)
        {
            spawnPointId = string.IsNullOrWhiteSpace(configuredSpawnPointId)
                ? "spawn_unassigned"
                : configuredSpawnPointId.Trim();
            actorRole = configuredActorRole;
            room = configuredRoom;
            selectionWeight = Mathf.Max(0.01f, configuredSelectionWeight);
        }

        private void OnValidate()
        {
            spawnPointId = spawnPointId?.Trim() ?? string.Empty;
            selectionWeight = Mathf.Max(0.01f, selectionWeight);
        }
    }
}
