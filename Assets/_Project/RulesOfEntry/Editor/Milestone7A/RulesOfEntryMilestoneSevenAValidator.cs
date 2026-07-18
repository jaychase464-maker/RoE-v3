using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Core;
using RulesOfEntry.Deployment;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Missions;
using RulesOfEntry.Navigation;
using RulesOfEntry.Officers;
using RulesOfEntry.Operations;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace RulesOfEntry.Editor.Milestone7A
{
    public static class RulesOfEntryMilestoneSevenAValidator
    {
        [MenuItem(
            "Tools/Rules of Entry/Milestone 7A/Validate Pressure Point Mission Greybox",
            false,
            91)]
        public static void ValidateFromMenu()
        {
            IReadOnlyList<ProjectValidationResult> results = RunValidation(true);
            int errors = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Error);
            int warnings = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Warning);
            int passes = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Pass);
            string summary = errors == 0
                ? $"Milestone 7A validation passed with {passes} checks and "
                    + $"{warnings} warning(s)."
                : $"Milestone 7A validation failed with {errors} error(s), "
                    + $"{warnings} warning(s), and {passes} passing checks.";
            EditorUtility.DisplayDialog(
                ProjectInfo.GameTitle,
                summary + "\n\nSee the Console for details.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(
            bool logResults)
        {
            List<ProjectValidationResult> results =
                new List<ProjectValidationResult>();
            ValidateNavigationPackage(results);
            ValidateScene(results);
            ValidateMissionAsset(results);
            ValidateArchitecture(results);
            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateNavigationPackage(
            ICollection<ProjectValidationResult> results)
        {
            PackageInfo package = PackageInfo.GetAllRegisteredPackages()
                .FirstOrDefault(candidate => string.Equals(
                    candidate.name,
                    "com.unity.ai.navigation",
                    StringComparison.Ordinal));
            if (package == null)
            {
                AddError(
                    results,
                    "M7A AI Navigation",
                    "Required package com.unity.ai.navigation is not installed.");
                return;
            }

            AddPass(
                results,
                "M7A AI Navigation",
                $"AI Navigation {package.version} is installed.");
        }

        private static void ValidateScene(
            ICollection<ProjectValidationResult> results)
        {
            Scene scene = SceneManager.GetSceneByPath(ProjectInfo.PrototypeScenePath);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened)
            {
                scene = EditorSceneManager.OpenScene(
                    ProjectInfo.PrototypeScenePath,
                    OpenSceneMode.Additive);
            }

            try
            {
                GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate =>
                    string.Equals(
                        candidate.name,
                        RulesOfEntryMilestoneSevenASetup.GeneratedRootName,
                        StringComparison.Ordinal));
                if (root == null)
                {
                    AddError(
                        results,
                        "M7A Generated Mission",
                        "Pressure Point has not been built in the prototype scene.");
                    return;
                }

                AddPass(
                    results,
                    "M7A Generated Mission",
                    "The Pressure Point greybox root exists in the operation scene.");
                ValidateTopology(results, scene, root);
                ValidateClearanceRooms(results, scene, root);
                ValidateDoorTraversal(results, root);
                ValidateScenarioVariation(results, scene, root);
                ValidateNavigationData(results, root);
                ValidateDeploymentEntries(results, scene, root);
                ValidateLegacyEnvironment(results, scene);
            }
            finally
            {
                if (opened && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void ValidateTopology(
            ICollection<ProjectValidationResult> results,
            Scene scene,
            GameObject root)
        {
            OperationTopology topology = root.GetComponent<OperationTopology>();
            string[] plannedEntryIds = scene.GetRootGameObjects()
                .SelectMany(candidate =>
                    candidate.GetComponentsInChildren<OperationEntryAnchor>(true))
                .Select(anchor => anchor.EntryPointId)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();
            string[] topologyEntryIds = topology?.EntryBindings
                .Select(binding => binding.EntryPointId)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>();
            bool valid = topology != null
                && topology.HasCompleteConfiguration
                && topology.Rooms.Length == 9
                && topology.Portals.Length == 9
                && plannedEntryIds.SequenceEqual(
                    topologyEntryIds,
                    StringComparer.Ordinal)
                && topology.TryFindRoute(
                    "entry_south_administration",
                    RulesOfEntryMilestoneSevenASetup.TargetClearanceRoomId,
                    out string[] southRoute)
                && southRoute.Length >= 3
                && topology.TryFindRoute(
                    "entry_west_service_yard",
                    "m7a_control_room",
                    out string[] westRoute)
                && westRoute.Length >= 3;
            if (!valid)
            {
                string details = topology == null
                    ? "Topology component is missing."
                    : string.Join(" | ", topology.Validation.Errors);
                AddError(
                    results,
                    "M7A Facility Topology",
                    "The mission requires nine connected areas, nine portals, and "
                        + "matching routes from all planning entries. " + details);
                return;
            }

            AddPass(
                results,
                "M7A Facility Topology",
                "Nine authored areas are connected by validated routes from all three entries.");
        }

        private static void ValidateClearanceRooms(
            ICollection<ProjectValidationResult> results,
            Scene scene,
            GameObject root)
        {
            TacticalRoomVolume[] rooms =
                root.GetComponentsInChildren<TacticalRoomVolume>(true);
            string[] ids = rooms.Select(room => room.RoomId).ToArray();
            MissionController mission = scene.GetRootGameObjects()
                .SelectMany(candidate =>
                    candidate.GetComponentsInChildren<MissionController>(true))
                .FirstOrDefault();
            string objectiveRoomId = mission?.Definition?.Objectives
                .FirstOrDefault(objective =>
                    objective.Type == MissionObjectiveType.VerifyRoomClear)
                ?.TargetRoomId ?? string.Empty;
            bool valid = rooms.Length == 6
                && rooms.All(room => room.HasCompleteConfiguration)
                && ids.Distinct(StringComparer.Ordinal).Count() == rooms.Length
                && string.Equals(
                    objectiveRoomId,
                    RulesOfEntryMilestoneSevenASetup.TargetClearanceRoomId,
                    StringComparison.Ordinal)
                && ids.Contains(objectiveRoomId, StringComparer.Ordinal);
            if (!valid)
            {
                AddError(
                    results,
                    "M7A Clearance Evidence",
                    "Six unique interior clearance volumes and the pump-hall objective "
                        + "must share stable room IDs.");
                return;
            }

            AddPass(
                results,
                "M7A Clearance Evidence",
                "Six interior spaces produce authoritative clearance evidence for the mission.");
        }

        private static void ValidateDoorTraversal(
            ICollection<ProjectValidationResult> results,
            GameObject root)
        {
            OperationPortal[] portals =
                root.GetComponentsInChildren<OperationPortal>(true);
            OperationPortal[] doorPortals = portals.Where(portal =>
                portal.PortalType != OperationPortalType.OpenPassage).ToArray();
            bool valid = portals.All(portal => portal.HasValidConfiguration)
                && doorPortals.Length == 7
                && doorPortals.All(portal => portal.Door != null
                    && portal.TraversalLink != null
                    && portal.TraversalLink.HasCompleteConfiguration
                    && portal.TraversalLink.NavigationLink.bidirectional
                    && portal.TraversalLink.NavigationLink.width >= 0.75f
                    && !portal.TraversalLink.transform.IsChildOf(
                        portal.Door.transform));
            if (!valid)
            {
                AddError(
                    results,
                    "M7A Door Traversal",
                    "Every physical portal requires a fixed bidirectional link gated "
                        + "by the door's actual open clearance.");
                return;
            }

            AddPass(
                results,
                "M7A Door Traversal",
                "Seven physical thresholds gate navigation with door-clearance links.");
        }

        private static void ValidateScenarioVariation(
            ICollection<ProjectValidationResult> results,
            Scene scene,
            GameObject root)
        {
            OperationScenarioDirector director =
                root.GetComponent<OperationScenarioDirector>();
            OperationSpawnPoint[] points =
                root.GetComponentsInChildren<OperationSpawnPoint>(true);
            int suspectPoints = points.Count(point =>
                point.ActorRole == ActorRole.Suspect);
            int civilianPoints = points.Count(point =>
                point.ActorRole == ActorRole.Civilian);
            int incidentActors = scene.GetRootGameObjects()
                .SelectMany(candidate =>
                    candidate.GetComponentsInChildren<HumanActorController>(true))
                .Count(actor => actor.GetComponent<ActorIdentity>()?.Role
                    != ActorRole.Officer);
            bool valid = director != null
                && director.HasCompleteConfiguration
                && points.Length == 12
                && points.All(point => point.HasValidConfiguration)
                && suspectPoints >= 5
                && civilianPoints >= 5
                && incidentActors >= 2;
            if (!valid)
            {
                AddError(
                    results,
                    "M7A Scenario Variation",
                    "The incident requires a configured director and multiple unique "
                        + "role-compatible locations for every authored subject.");
                return;
            }

            AddPass(
                results,
                "M7A Scenario Variation",
                "Twelve weighted locations vary suspect and civilian placement without changing actor identity.");
        }

        private static void ValidateNavigationData(
            ICollection<ProjectValidationResult> results,
            GameObject root)
        {
            NavMeshSurface surface = root.GetComponent<NavMeshSurface>();
            NavMeshData data = AssetDatabase.LoadAssetAtPath<NavMeshData>(
                RulesOfEntryMilestoneSevenASetup.NavMeshDataPath);
            bool valid = surface != null
                && surface.navMeshData != null
                && data != null
                && surface.navMeshData == data
                && surface.useGeometry == NavMeshCollectGeometry.PhysicsColliders
                && surface.ignoreNavMeshAgent
                && surface.ignoreNavMeshObstacle;
            if (!valid)
            {
                AddError(
                    results,
                    "M7A Baked Navigation",
                    "Pressure Point requires persisted collider-derived NavMesh data "
                        + "that excludes dynamic agents and obstacles.");
                return;
            }

            AddPass(
                results,
                "M7A Baked Navigation",
                "Pressure Point uses persisted NavMesh data authored from physical greybox colliders.");
        }

        private static void ValidateDeploymentEntries(
            ICollection<ProjectValidationResult> results,
            Scene scene,
            GameObject root)
        {
            OperationTopology topology = root.GetComponent<OperationTopology>();
            OperationEntryAnchor[] anchors = scene.GetRootGameObjects()
                .SelectMany(candidate =>
                    candidate.GetComponentsInChildren<OperationEntryAnchor>(true))
                .ToArray();
            bool valid = topology != null
                && anchors.Length == 3
                && anchors.All(anchor => anchor.HasValidConfiguration)
                && anchors.All(anchor => NavMesh.SamplePosition(
                    anchor.PlayerSpawn.position,
                    out _,
                    2.5f,
                    NavMesh.AllAreas))
                && anchors.All(anchor => anchor.OfficerSpawns.All(spawn =>
                    NavMesh.SamplePosition(
                        spawn.position,
                        out _,
                        2.5f,
                        NavMesh.AllAreas)));
            if (!valid)
            {
                AddError(
                    results,
                    "M7A Deployment Routes",
                    "All player and officer positions for the three selected entry "
                        + "plans must resolve onto the baked exterior NavMesh.");
                return;
            }

            AddPass(
                results,
                "M7A Deployment Routes",
                "South, west, and north deployment formations resolve onto navigable staging areas.");
        }

        private static void ValidateLegacyEnvironment(
            ICollection<ProjectValidationResult> results,
            Scene scene)
        {
            string[] legacyNames = { "[Milestone1_Graybox]", "[Milestone2_Range]" };
            GameObject[] legacyRoots = scene.GetRootGameObjects()
                .Where(root => legacyNames.Contains(root.name, StringComparer.Ordinal))
                .ToArray();
            if (legacyRoots.Any(root => root.activeSelf))
            {
                AddError(
                    results,
                    "M7A Legacy Geometry",
                    "The training-room and firing-range geometry must remain disabled "
                        + "during the Pressure Point operation.");
                return;
            }

            AddPass(
                results,
                "M7A Legacy Geometry",
                "Legacy prototype geometry is preserved but inactive behind the mission greybox.");
        }

        private static void ValidateMissionAsset(
            ICollection<ProjectValidationResult> results)
        {
            MissionDefinition mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(
                RulesOfEntryMilestoneSevenASetup.MissionDefinitionPath);
            MissionObjectiveDefinition clear = mission?.Objectives.FirstOrDefault(objective =>
                objective.Type == MissionObjectiveType.VerifyRoomClear);
            if (mission == null
                || !mission.HasValidConfiguration
                || !string.Equals(
                    mission.MissionId,
                    "m6_pressure_point",
                    StringComparison.Ordinal)
                || clear == null
                || !string.Equals(
                    clear.TargetRoomId,
                    RulesOfEntryMilestoneSevenASetup.TargetClearanceRoomId,
                    StringComparison.Ordinal))
            {
                AddError(
                    results,
                    "M7A Mission Definition",
                    "Operation Pressure Point must target the authored pump-hall evidence source.");
                return;
            }

            AddPass(
                results,
                "M7A Mission Definition",
                "Pressure Point objectives reference the first mission greybox by stable IDs.");
        }

        private static void ValidateArchitecture(
            ICollection<ProjectValidationResult> results)
        {
            bool rulesArePure = !typeof(MonoBehaviour).IsAssignableFrom(
                typeof(OperationTopologyRules));
            bool topologyDoesNotOwnAi = typeof(OperationTopology)
                .GetFields(System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.NonPublic)
                .All(field => !typeof(HumanActorController).IsAssignableFrom(
                    field.FieldType));
            bool scenarioDoesNotOwnMission = typeof(OperationScenarioDirector)
                .GetFields(System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.NonPublic)
                .All(field => !typeof(MissionController).IsAssignableFrom(
                    field.FieldType));
            if (!rulesArePure || !topologyDoesNotOwnAi || !scenarioDoesNotOwnMission)
            {
                AddError(
                    results,
                    "M7A Architecture",
                    "Topology must remain pure routing data, while scenario placement "
                        + "must not decide AI behavior or mission outcomes.");
                return;
            }

            AddPass(
                results,
                "M7A Architecture",
                "Routing, actor placement, AI decisions, and mission evidence remain separate authorities.");
        }

        private static void LogResults(IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                string message = $"{result.Check}: {result.Message}";
                switch (result.Severity)
                {
                    case ProjectValidationSeverity.Error:
                        ProjectLog.Error("M7A Validation", message);
                        break;
                    case ProjectValidationSeverity.Warning:
                        ProjectLog.Warning("M7A Validation", message);
                        break;
                    default:
                        ProjectLog.Info("M7A Validation", message);
                        break;
                }
            }
        }

        private static void AddPass(
            ICollection<ProjectValidationResult> results,
            string check,
            string message)
        {
            results.Add(new ProjectValidationResult(
                ProjectValidationSeverity.Pass,
                check,
                message));
        }

        private static void AddError(
            ICollection<ProjectValidationResult> results,
            string check,
            string message)
        {
            results.Add(new ProjectValidationResult(
                ProjectValidationSeverity.Error,
                check,
                message));
        }
    }
}
