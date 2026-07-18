using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Core;
using UnityEngine;
using UnityEngine.AI;

namespace RulesOfEntry.Operations
{
    /// <summary>
    /// Places already-authored incident actors at role-compatible locations before
    /// mission evaluation begins. It never spawns arbitrary combatants or changes
    /// mission objectives, which preserves evidence and actor identity contracts.
    /// </summary>
    [DefaultExecutionOrder(-650)]
    [DisallowMultipleComponent]
    public sealed class OperationScenarioDirector : MonoBehaviour
    {
        [SerializeField] private HumanActorController[] incidentActors =
            Array.Empty<HumanActorController>();
        [SerializeField] private OperationSpawnPoint[] spawnPoints =
            Array.Empty<OperationSpawnPoint>();
        [SerializeField] private OperationScenarioSeedMode seedMode =
            OperationScenarioSeedMode.NewSession;
        [SerializeField] private int authoredSeed = 70601;
        [SerializeField, Min(0.1f)] private float navigationSampleRadius = 1.5f;

        public int AppliedSeed { get; private set; }
        public bool ScenarioApplied { get; private set; }
        public HumanActorController[] IncidentActors => incidentActors?
            .Where(actor => actor != null).ToArray()
            ?? Array.Empty<HumanActorController>();
        public OperationSpawnPoint[] SpawnPoints => spawnPoints?
            .Where(point => point != null).ToArray()
            ?? Array.Empty<OperationSpawnPoint>();
        public bool HasCompleteConfiguration => incidentActors != null
            && incidentActors.Length > 0
            && incidentActors.All(actor => actor != null
                && actor.GetComponent<ActorIdentity>() != null)
            && spawnPoints != null
            && spawnPoints.Length >= incidentActors.Length
            && spawnPoints.All(point => point != null
                && point.HasValidConfiguration)
            && spawnPoints.Select(point => point.SpawnPointId)
                .Distinct(StringComparer.Ordinal)
                .Count() == spawnPoints.Length
            && HasEnoughRoleCompatiblePoints();

        public void Configure(
            HumanActorController[] configuredActors,
            OperationSpawnPoint[] configuredSpawnPoints,
            OperationScenarioSeedMode configuredSeedMode,
            int configuredAuthoredSeed,
            float configuredNavigationSampleRadius = 1.5f)
        {
            incidentActors = configuredActors?.Where(actor => actor != null).ToArray()
                ?? Array.Empty<HumanActorController>();
            spawnPoints = configuredSpawnPoints?.Where(point => point != null).ToArray()
                ?? Array.Empty<OperationSpawnPoint>();
            seedMode = configuredSeedMode;
            authoredSeed = configuredAuthoredSeed == 0 ? 70601 : configuredAuthoredSeed;
            navigationSampleRadius = Mathf.Max(0.1f, configuredNavigationSampleRadius);
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Operation Scenario",
                    "Incident actors or role-compatible spawn points are incomplete. "
                        + "Run the Milestone 7A setup tool.",
                    this);
                return;
            }

            int seed = seedMode == OperationScenarioSeedMode.Fixed
                ? authoredSeed
                : CreateSessionSeed(authoredSeed);
            ApplyScenario(seed);
        }

        public bool ApplyScenario(int incidentSeed)
        {
            if (!HasCompleteConfiguration)
            {
                return false;
            }

            ActorIdentity[] identities = incidentActors
                .Select(actor => actor.GetComponent<ActorIdentity>())
                .ToArray();
            int[] roles = identities.Select(identity => (int)identity.Role).ToArray();
            OperationSpawnRecord[] records = spawnPoints.Select(point =>
                new OperationSpawnRecord(
                    point.SpawnPointId,
                    (int)point.ActorRole,
                    point.Room.RoomId,
                    point.SelectionWeight)).ToArray();
            int[] selections = OperationTopologyRules.SelectSpawnIndices(
                roles,
                records,
                incidentSeed);
            if (selections.Any(index => index < 0 || index >= spawnPoints.Length))
            {
                ProjectLog.Error(
                    "Operation Scenario",
                    "The scenario could not find a unique role-compatible spawn for every actor.",
                    this);
                return false;
            }

            List<string> placementSummary = new List<string>();
            for (int index = 0; index < incidentActors.Length; index++)
            {
                HumanActorController actor = incidentActors[index];
                ActorIdentity identity = identities[index];
                OperationSpawnPoint point = spawnPoints[selections[index]];
                if (!TryPlaceActor(actor, point))
                {
                    ProjectLog.Error(
                        "Operation Scenario",
                        $"Could not place {identity.DisplayName} on the baked NavMesh at "
                            + $"'{point.SpawnPointId}'.",
                        actor);
                    return false;
                }

                identity.Configure(
                    identity.ActorId,
                    identity.DisplayName,
                    identity.Role,
                    CombineSeed(incidentSeed, identity.ActorId));
                placementSummary.Add(
                    $"{identity.ActorId} -> {point.Room.RoomId}/{point.SpawnPointId}");
            }

            AppliedSeed = incidentSeed == 0 ? 1 : incidentSeed;
            ScenarioApplied = true;
            Physics.SyncTransforms();
            ProjectLog.Info(
                "Operation Scenario",
                $"Incident seed {AppliedSeed}: {string.Join(", ", placementSummary)}.",
                this);
            return true;
        }

        private bool TryPlaceActor(
            HumanActorController actor,
            OperationSpawnPoint point)
        {
            if (!NavMesh.SamplePosition(
                point.Position,
                out NavMeshHit hit,
                navigationSampleRadius,
                NavMesh.AllAreas))
            {
                return false;
            }

            NavMeshAgent agent = actor.GetComponent<NavMeshAgent>();
            bool placed = agent != null && agent.enabled
                ? agent.Warp(hit.position)
                : SetTransformPosition(actor.transform, hit.position);
            if (placed)
            {
                actor.transform.rotation = point.Rotation;
            }

            return placed;
        }

        private bool HasEnoughRoleCompatiblePoints()
        {
            Dictionary<ActorRole, int> actorCounts = incidentActors
                .Where(actor => actor != null)
                .Select(actor => actor.GetComponent<ActorIdentity>())
                .Where(identity => identity != null)
                .GroupBy(identity => identity.Role)
                .ToDictionary(group => group.Key, group => group.Count());
            Dictionary<ActorRole, int> pointCounts = spawnPoints
                .Where(point => point != null)
                .GroupBy(point => point.ActorRole)
                .ToDictionary(group => group.Key, group => group.Count());
            return actorCounts.All(pair => pointCounts.TryGetValue(pair.Key, out int count)
                && count >= pair.Value);
        }

        private static bool SetTransformPosition(Transform target, Vector3 position)
        {
            target.position = position;
            return true;
        }

        private static int CreateSessionSeed(int baseSeed)
        {
            unchecked
            {
                long ticks = DateTime.UtcNow.Ticks;
                int seed = baseSeed
                    ^ Environment.TickCount
                    ^ (int)ticks
                    ^ (int)(ticks >> 32);
                return seed == 0 ? 1 : seed;
            }
        }

        private static int CombineSeed(int incidentSeed, string actorId)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char character in actorId ?? string.Empty)
                {
                    hash ^= character;
                    hash *= 16777619u;
                }

                int combined = incidentSeed ^ (int)hash;
                return combined == 0 ? 1 : combined;
            }
        }

        private void OnValidate()
        {
            incidentActors ??= Array.Empty<HumanActorController>();
            spawnPoints ??= Array.Empty<OperationSpawnPoint>();
            if (authoredSeed == 0)
            {
                authoredSeed = 70601;
            }

            navigationSampleRadius = Mathf.Max(0.1f, navigationSampleRadius);
        }
    }
}
