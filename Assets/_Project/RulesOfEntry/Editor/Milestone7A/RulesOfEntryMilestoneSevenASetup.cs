using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Core;
using RulesOfEntry.Deployment;
using RulesOfEntry.Interaction;
using RulesOfEntry.Missions;
using RulesOfEntry.Navigation;
using RulesOfEntry.Officers;
using RulesOfEntry.Operations;
using RulesOfEntry.Player;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace RulesOfEntry.Editor.Milestone7A
{
    public static class RulesOfEntryMilestoneSevenASetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 7A/Build Pressure Point Mission Greybox";

        internal const string GeneratedRootName = "[Milestone7A_PressurePoint]";
        internal const string NavMeshDataPath =
            "Assets/_Project/RulesOfEntry/Data/Navigation/M7A_PressurePointNavMesh.asset";
        internal const string MissionDefinitionPath =
            "Assets/_Project/RulesOfEntry/Data/Missions/M5_TrainingOperation.asset";
        internal const string TargetClearanceRoomId = "m7a_pump_hall";

        private const string NavigationFolder =
            "Assets/_Project/RulesOfEntry/Data/Navigation";
        private const string MaterialFolder =
            "Assets/_Project/RulesOfEntry/Art/Materials/MissionGreybox";
        private const string PlayerRootName = "ROE_Player";
        private const string LegacyGrayboxRootName = "[Milestone1_Graybox]";
        private const string LegacyRangeRootName = "[Milestone2_Range]";
        private const string LegacyRoomName = "M4_NorthTrainingRoomClearance";
        private const string LegacyDoorLinkName = "M4_TrainingDoorTraversalLink";

        private readonly struct WallOpening
        {
            public WallOpening(float center, float width)
            {
                Center = center;
                Width = width;
            }

            public float Center { get; }
            public float Width { get; }
        }

        [MenuItem(MenuPath, false, 90)]
        public static void BuildPressurePointMissionGreybox()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building the Milestone 7A mission greybox.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning(
                    "Milestone 7A",
                    "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                EnsureFolder(NavigationFolder);
                EnsureFolder(MaterialFolder);
                MissionDefinition mission = ConfigurePressurePointMission();
                Material exterior = CreateOrUpdateMaterial(
                    MaterialFolder + "/M7A_WetAsphalt.mat",
                    new Color(0.025f, 0.035f, 0.042f, 1f),
                    0.72f,
                    0.02f);
                Material floor = CreateOrUpdateMaterial(
                    MaterialFolder + "/M7A_ConcreteFloor.mat",
                    new Color(0.095f, 0.11f, 0.12f, 1f),
                    0.34f,
                    0.04f);
                Material wall = CreateOrUpdateMaterial(
                    MaterialFolder + "/M7A_ConcreteWall.mat",
                    new Color(0.19f, 0.215f, 0.225f, 1f),
                    0.22f,
                    0.02f);
                Material doorMaterial = CreateOrUpdateMaterial(
                    MaterialFolder + "/M7A_SecurityDoor.mat",
                    new Color(0.035f, 0.15f, 0.24f, 1f),
                    0.42f,
                    0.48f);
                Material machinery = CreateOrUpdateMaterial(
                    MaterialFolder + "/M7A_Machinery.mat",
                    new Color(0.12f, 0.17f, 0.18f, 1f),
                    0.38f,
                    0.62f);
                Material accent = CreateOrUpdateMaterial(
                    MaterialFolder + "/M7A_OperationalAccent.mat",
                    new Color(0.02f, 0.36f, 0.65f, 1f),
                    0.48f,
                    0.2f);

                Scene scene = EditorSceneManager.OpenScene(
                    ProjectInfo.PrototypeScenePath,
                    OpenSceneMode.Single);
                SceneManager.SetActiveScene(scene);
                SceneDependencies dependencies = RequireSceneDependencies(scene);
                RemovePreviousGeneratedContent(scene);
                DisableLegacyEnvironment(scene);
                RemoveLegacyRoomAndDoorLink(scene);
                RemovePreviousNavMeshSurfaces(scene);

                GameObject root = new GameObject(GeneratedRootName);
                SceneManager.MoveGameObjectToScene(root, scene);
                GameObject geometryRoot = CreateChild("Geometry", root.transform);
                GameObject topologyRoot = CreateChild("Topology", root.transform);
                GameObject portalRoot = CreateChild("Portals", root.transform);
                GameObject scenarioRoot = CreateChild("Scenario", root.transform);

                BuildFacilityGeometry(
                    geometryRoot.transform,
                    exterior,
                    floor,
                    wall,
                    machinery,
                    accent);
                Dictionary<string, OperationRoomNode> rooms = BuildRoomNodes(
                    topologyRoot.transform,
                    accent);
                int interactableLayer = RequireLayer("Interactable");
                OperationPortal[] portals = BuildPortals(
                    portalRoot.transform,
                    rooms,
                    doorMaterial,
                    interactableLayer);

                OperationEntryAnchor[] anchors = ConfigureEntryAnchors(
                    dependencies.EntryAnchors);
                OperationTopology topology = root.AddComponent<OperationTopology>();
                topology.Configure(
                    rooms.Values.ToArray(),
                    portals,
                    anchors,
                    new[]
                    {
                        new OperationEntryRoomBinding(
                            "entry_south_administration",
                            rooms["m7a_staging_south"]),
                        new OperationEntryRoomBinding(
                            "entry_west_service_yard",
                            rooms["m7a_staging_west"]),
                        new OperationEntryRoomBinding(
                            "entry_north_pipe_gallery",
                            rooms["m7a_staging_north"])
                    });
                if (!topology.HasCompleteConfiguration)
                {
                    throw new InvalidOperationException(
                        "The authored Pressure Point topology is invalid: "
                            + string.Join(" | ", topology.Validation.Errors));
                }

                OperationSpawnPoint[] spawnPoints = BuildScenarioSpawnPoints(
                    scenarioRoot.transform,
                    rooms);
                OperationScenarioDirector scenario =
                    root.AddComponent<OperationScenarioDirector>();
                scenario.Configure(
                    dependencies.IncidentActors,
                    spawnPoints,
                    OperationScenarioSeedMode.NewSession,
                    70601,
                    1.75f);
                if (!scenario.HasCompleteConfiguration)
                {
                    throw new InvalidOperationException(
                        "Pressure Point does not have enough role-compatible incident spawns.");
                }

                SetDirectPlayStaging(dependencies, anchors, spawnPoints);
                RepositionDebriefConsole(scene);

                int playerLayer = RequireLayer("Player");
                NavMeshSurface surface = root.AddComponent<NavMeshSurface>();
                surface.collectObjects = CollectObjects.All;
                surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                surface.layerMask = ~(1 << playerLayer);
                surface.ignoreNavMeshAgent = true;
                surface.ignoreNavMeshObstacle = true;

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException(
                        $"Unity could not save {ProjectInfo.PrototypeScenePath} before the navigation build.");
                }

                BuildAndPersistNavMesh(surface, portals);
                EditorUtility.SetDirty(mission);
                EditorUtility.SetDirty(topology);
                EditorUtility.SetDirty(scenario);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException(
                        $"Unity could not save {ProjectInfo.PrototypeScenePath} after the navigation build.");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeGameObject = root;
                ProjectLog.Info(
                    "Milestone 7A",
                    "Pressure Point multi-room greybox, connected portal graph, three deployment routes, and deterministic role-aware scenario variation created. Running validation now.");
                RulesOfEntryMilestoneSevenAValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Milestone 7A", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 7A setup stopped. Check the first Console error for the root cause.",
                    "OK");
            }
        }

        private static MissionDefinition ConfigurePressurePointMission()
        {
            MissionDefinition mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(
                MissionDefinitionPath);
            if (mission == null)
            {
                throw new InvalidOperationException(
                    "The operation mission asset is missing. Rebuild Milestone 6A first.");
            }

            MissionObjectiveDefinition secureSuspect = CreateObjective(
                "secure_primary_suspect",
                "Secure the primary suspect",
                "Take the identified armed subject into lawful custody.",
                MissionObjectiveType.SecureSubject,
                "m3_suspect_01",
                string.Empty,
                30);
            MissionObjectiveDefinition protectCivilian = CreateObjective(
                "protect_facility_employee",
                "Protect the facility employee",
                "Locate and protect the reported municipal employee.",
                MissionObjectiveType.ProtectActor,
                "m3_civilian_01",
                string.Empty,
                25);
            MissionObjectiveDefinition clearPumpHall = CreateObjective(
                "verify_pump_hall_clear",
                "Verify the pump hall clear",
                "Conduct a deliberate search and confirm that no active threat remains in the primary process space.",
                MissionObjectiveType.VerifyRoomClear,
                string.Empty,
                TargetClearanceRoomId,
                25);
            MissionObjectiveDefinition preserveTeam = CreateObjective(
                "preserve_response_team",
                "Preserve the response team",
                "No assigned officer may become incapacitated or die.",
                MissionObjectiveType.PreserveOfficerTeam,
                string.Empty,
                string.Empty,
                20);
            mission.Configure(
                "m6_pressure_point",
                "Operation: Pressure Point",
                "Respond to an armed-subject incident at a municipal pumping annex. Establish control, protect the reported employee, clear the pump hall, resolve the subject lawfully, and document every use of force.",
                new[]
                {
                    secureSuspect,
                    protectCivilian,
                    clearPumpHall,
                    preserveTeam
                });
            EditorUtility.SetDirty(mission);
            return mission;
        }

        private static MissionObjectiveDefinition CreateObjective(
            string id,
            string displayName,
            string briefing,
            MissionObjectiveType type,
            string targetActorId,
            string targetRoomId,
            int deduction)
        {
            MissionObjectiveDefinition objective = new MissionObjectiveDefinition();
            objective.Configure(
                id,
                displayName,
                briefing,
                type,
                targetActorId,
                targetRoomId,
                true,
                deduction);
            return objective;
        }

        private static void BuildFacilityGeometry(
            Transform parent,
            Material exterior,
            Material floor,
            Material wall,
            Material machinery,
            Material accent)
        {
            CreateBlock(
                "ExteriorServiceApron",
                parent,
                new Vector3(0f, -0.2f, 4f),
                new Vector3(44f, 0.3f, 38f),
                exterior,
                true);
            CreateBlock(
                "FacilityFloor",
                parent,
                new Vector3(0f, -0.05f, 4f),
                new Vector3(30f, 0.1f, 24f),
                floor,
                true);

            CreateSegmentedWall(
                "SouthExterior",
                parent,
                true,
                -8f,
                -15f,
                15f,
                new[] { new WallOpening(0f, 1.6f) },
                wall);
            CreateSegmentedWall(
                "NorthExterior",
                parent,
                true,
                16f,
                -15f,
                15f,
                new[] { new WallOpening(-6f, 1.6f) },
                wall);
            CreateSegmentedWall(
                "WestExterior",
                parent,
                false,
                -15f,
                -8f,
                16f,
                new[] { new WallOpening(-3f, 1.6f) },
                wall);
            CreateSegmentedWall(
                "EastExterior",
                parent,
                false,
                15f,
                -8f,
                16f,
                Array.Empty<WallOpening>(),
                wall);

            CreateSegmentedWall(
                "SouthToCorridor",
                parent,
                true,
                2f,
                -15f,
                15f,
                new[]
                {
                    new WallOpening(-10f, 1.6f),
                    new WallOpening(0f, 2f),
                    new WallOpening(10f, 1.6f)
                },
                wall);
            CreateSegmentedWall(
                "CorridorToProcess",
                parent,
                true,
                6f,
                -15f,
                15f,
                new[]
                {
                    new WallOpening(0f, 2f),
                    new WallOpening(11f, 1.6f)
                },
                wall);
            CreateSegmentedWall(
                "MaintenanceDivider",
                parent,
                false,
                -5f,
                -8f,
                2f,
                Array.Empty<WallOpening>(),
                wall);
            CreateSegmentedWall(
                "AdministrationDivider",
                parent,
                false,
                5f,
                -8f,
                2f,
                Array.Empty<WallOpening>(),
                wall);
            CreateSegmentedWall(
                "ControlRoomDivider",
                parent,
                false,
                7f,
                6f,
                16f,
                new[] { new WallOpening(11f, 1.6f) },
                wall);

            CreateBlock(
                "PumpSkid_A",
                parent,
                new Vector3(-9f, 0.8f, 10f),
                new Vector3(3.4f, 1.6f, 1.5f),
                machinery,
                true);
            CreateBlock(
                "PumpSkid_B",
                parent,
                new Vector3(-2.5f, 0.8f, 12.5f),
                new Vector3(3.4f, 1.6f, 1.5f),
                machinery,
                true);
            CreateBlock(
                "PumpSkid_C",
                parent,
                new Vector3(3.5f, 0.8f, 9f),
                new Vector3(2.8f, 1.6f, 1.4f),
                machinery,
                true);
            CreateBlock(
                "ControlConsole",
                parent,
                new Vector3(11f, 0.65f, 12.8f),
                new Vector3(4.8f, 1.3f, 0.8f),
                accent,
                true);
            CreateBlock(
                "MaintenanceWorkbench",
                parent,
                new Vector3(-11f, 0.55f, -5.8f),
                new Vector3(5f, 1.1f, 0.75f),
                machinery,
                true);
            CreateBlock(
                "AdministrationDesk",
                parent,
                new Vector3(10f, 0.45f, -4.5f),
                new Vector3(3.8f, 0.9f, 1.1f),
                machinery,
                true);

            CreateAreaLabel(parent, "MAINTENANCE", new Vector3(-10f, 2.7f, -2f));
            CreateAreaLabel(parent, "RECEPTION", new Vector3(0f, 2.7f, -2f));
            CreateAreaLabel(parent, "ADMINISTRATION", new Vector3(10f, 2.7f, -2f));
            CreateAreaLabel(parent, "CENTRAL CORRIDOR", new Vector3(0f, 2.7f, 4f));
            CreateAreaLabel(parent, "PUMP HALL", new Vector3(-4f, 2.7f, 10f));
            CreateAreaLabel(parent, "CONTROL ROOM", new Vector3(11f, 2.7f, 10f));
        }

        private static Dictionary<string, OperationRoomNode> BuildRoomNodes(
            Transform parent,
            Material accent)
        {
            Dictionary<string, OperationRoomNode> rooms =
                new Dictionary<string, OperationRoomNode>(StringComparer.Ordinal);
            AddInteriorRoom(
                rooms, parent, "m7a_maintenance", "Maintenance Bay",
                OperationAreaType.Utility, new Vector3(-10f, 0f, -3f),
                new Vector3(9.6f, 3f, 9.6f));
            AddInteriorRoom(
                rooms, parent, "m7a_reception", "Administration Reception",
                OperationAreaType.InteriorRoom, new Vector3(0f, 0f, -3f),
                new Vector3(9.6f, 3f, 9.6f));
            AddInteriorRoom(
                rooms, parent, "m7a_administration", "Administration Office",
                OperationAreaType.InteriorRoom, new Vector3(10f, 0f, -3f),
                new Vector3(9.6f, 3f, 9.6f));
            AddInteriorRoom(
                rooms, parent, "m7a_central_corridor", "Central Corridor",
                OperationAreaType.Corridor, new Vector3(0f, 0f, 4f),
                new Vector3(29.6f, 3f, 3.6f));
            AddInteriorRoom(
                rooms, parent, TargetClearanceRoomId, "Primary Pump Hall",
                OperationAreaType.Utility, new Vector3(-4f, 0f, 11f),
                new Vector3(21.6f, 3f, 9.6f));
            AddInteriorRoom(
                rooms, parent, "m7a_control_room", "Control Room",
                OperationAreaType.InteriorRoom, new Vector3(11f, 0f, 11f),
                new Vector3(7.6f, 3f, 9.6f));

            AddStagingRoom(
                rooms, parent, "m7a_staging_south", "South Staging",
                new Vector3(0f, 0f, -11f));
            AddStagingRoom(
                rooms, parent, "m7a_staging_west", "West Service Staging",
                new Vector3(-18f, 0f, -3f));
            AddStagingRoom(
                rooms, parent, "m7a_staging_north", "North Pipe Staging",
                new Vector3(-6f, 0f, 19f));

            foreach (OperationRoomNode room in rooms.Values.Where(room =>
                room.AreaType == OperationAreaType.ExteriorStaging))
            {
                CreateBlock(
                    room.DisplayName.Replace(" ", string.Empty) + "Marker",
                    parent,
                    room.transform.position + new Vector3(0f, 0.02f, 0f),
                    new Vector3(3f, 0.025f, 3f),
                    accent,
                    false);
            }

            return rooms;
        }

        private static void AddInteriorRoom(
            IDictionary<string, OperationRoomNode> rooms,
            Transform parent,
            string id,
            string displayName,
            OperationAreaType areaType,
            Vector3 center,
            Vector3 size)
        {
            GameObject roomObject = new GameObject("Room_" + id);
            roomObject.transform.SetParent(parent, false);
            roomObject.transform.position = center;
            BoxCollider bounds = roomObject.AddComponent<BoxCollider>();
            bounds.isTrigger = true;
            bounds.center = new Vector3(0f, 1.5f, 0f);
            bounds.size = size;
            TacticalRoomVolume clearance =
                roomObject.AddComponent<TacticalRoomVolume>();
            clearance.Configure(id, displayName, bounds, 1, 3f);
            OperationRoomNode node = roomObject.AddComponent<OperationRoomNode>();
            node.Configure(id, displayName, areaType, true, clearance);
            rooms.Add(id, node);
        }

        private static void AddStagingRoom(
            IDictionary<string, OperationRoomNode> rooms,
            Transform parent,
            string id,
            string displayName,
            Vector3 position)
        {
            GameObject roomObject = new GameObject("Room_" + id);
            roomObject.transform.SetParent(parent, false);
            roomObject.transform.position = position;
            OperationRoomNode node = roomObject.AddComponent<OperationRoomNode>();
            node.Configure(
                id,
                displayName,
                OperationAreaType.ExteriorStaging,
                false,
                null);
            rooms.Add(id, node);
        }

        private static OperationPortal[] BuildPortals(
            Transform parent,
            IReadOnlyDictionary<string, OperationRoomNode> rooms,
            Material doorMaterial,
            int interactableLayer)
        {
            List<OperationPortal> portals = new List<OperationPortal>
            {
                CreateDoorPortal(
                    parent, "portal_south_entry", OperationPortalType.ExteriorDoor,
                    rooms["m7a_staging_south"], rooms["m7a_reception"],
                    new Vector3(0f, 0f, -8f), 0f, 1.4f, 100f,
                    doorMaterial, interactableLayer),
                CreateDoorPortal(
                    parent, "portal_west_entry", OperationPortalType.ExteriorDoor,
                    rooms["m7a_staging_west"], rooms["m7a_maintenance"],
                    new Vector3(-15f, 0f, -3f), 90f, 1.4f, -100f,
                    doorMaterial, interactableLayer),
                CreateDoorPortal(
                    parent, "portal_north_entry", OperationPortalType.ExteriorDoor,
                    rooms["m7a_staging_north"], rooms[TargetClearanceRoomId],
                    new Vector3(-6f, 0f, 16f), 180f, 1.4f, 100f,
                    doorMaterial, interactableLayer),
                CreateDoorPortal(
                    parent, "portal_maintenance_corridor", OperationPortalType.InteriorDoor,
                    rooms["m7a_maintenance"], rooms["m7a_central_corridor"],
                    new Vector3(-10f, 0f, 2f), 0f, 1.4f, -100f,
                    doorMaterial, interactableLayer),
                CreateOpenPortal(
                    parent, "portal_reception_corridor",
                    rooms["m7a_reception"], rooms["m7a_central_corridor"],
                    new Vector3(0f, 0f, 2f)),
                CreateDoorPortal(
                    parent, "portal_administration_corridor", OperationPortalType.InteriorDoor,
                    rooms["m7a_administration"], rooms["m7a_central_corridor"],
                    new Vector3(10f, 0f, 2f), 0f, 1.4f, 100f,
                    doorMaterial, interactableLayer),
                CreateOpenPortal(
                    parent, "portal_corridor_pump_hall",
                    rooms["m7a_central_corridor"], rooms[TargetClearanceRoomId],
                    new Vector3(0f, 0f, 6f)),
                CreateDoorPortal(
                    parent, "portal_corridor_control", OperationPortalType.InteriorDoor,
                    rooms["m7a_central_corridor"], rooms["m7a_control_room"],
                    new Vector3(11f, 0f, 6f), 0f, 1.4f, -100f,
                    doorMaterial, interactableLayer),
                CreateDoorPortal(
                    parent, "portal_pump_control", OperationPortalType.InteriorDoor,
                    rooms[TargetClearanceRoomId], rooms["m7a_control_room"],
                    new Vector3(7f, 0f, 11f), 90f, 1.4f, 100f,
                    doorMaterial, interactableLayer)
            };
            return portals.ToArray();
        }

        private static OperationPortal CreateDoorPortal(
            Transform parent,
            string id,
            OperationPortalType type,
            OperationRoomNode roomA,
            OperationRoomNode roomB,
            Vector3 center,
            float yaw,
            float width,
            float openAngle,
            Material doorMaterial,
            int interactableLayer)
        {
            GameObject portalObject = new GameObject("Portal_" + id);
            portalObject.transform.SetParent(parent, false);
            portalObject.transform.SetPositionAndRotation(
                center,
                Quaternion.Euler(0f, yaw, 0f));

            GameObject pivot = new GameObject("DoorPivot");
            pivot.layer = interactableLayer;
            pivot.transform.SetParent(portalObject.transform, false);
            pivot.transform.localPosition = new Vector3(-width * 0.5f, 0f, 0f);
            GameObject leaf = CreateBlock(
                "DoorLeaf",
                pivot.transform,
                Vector3.zero,
                Vector3.one,
                doorMaterial,
                true);
            leaf.layer = interactableLayer;
            leaf.transform.localPosition = new Vector3(width * 0.5f, 1.1f, 0f);
            leaf.transform.localScale = new Vector3(width, 2.2f, 0.12f);
            SetLayerRecursively(leaf, interactableLayer);
            PrototypeDoor door = pivot.AddComponent<PrototypeDoor>();
            door.Configure(pivot.transform, openAngle);

            GameObject linkObject = new GameObject("TraversalLink");
            linkObject.transform.SetParent(portalObject.transform, false);
            NavMeshLink link = linkObject.AddComponent<NavMeshLink>();
            link.startPoint = new Vector3(0f, 0f, -1.2f);
            link.endPoint = new Vector3(0f, 0f, 1.2f);
            link.width = Mathf.Max(0.75f, width - 0.25f);
            link.bidirectional = true;
            link.area = 0;
            link.costModifier = -1f;
            link.autoUpdate = false;
            link.UpdateLink();
            link.activated = false;
            DoorTraversalLink traversal =
                linkObject.AddComponent<DoorTraversalLink>();
            traversal.Configure(door, link);

            OperationPortal portal = portalObject.AddComponent<OperationPortal>();
            portal.Configure(id, type, roomA, roomB, door, traversal);
            return portal;
        }

        private static OperationPortal CreateOpenPortal(
            Transform parent,
            string id,
            OperationRoomNode roomA,
            OperationRoomNode roomB,
            Vector3 center)
        {
            GameObject portalObject = new GameObject("Portal_" + id);
            portalObject.transform.SetParent(parent, false);
            portalObject.transform.position = center;
            OperationPortal portal = portalObject.AddComponent<OperationPortal>();
            portal.Configure(
                id,
                OperationPortalType.OpenPassage,
                roomA,
                roomB,
                null,
                null);
            return portal;
        }

        private static OperationSpawnPoint[] BuildScenarioSpawnPoints(
            Transform parent,
            IReadOnlyDictionary<string, OperationRoomNode> rooms)
        {
            List<OperationSpawnPoint> points = new List<OperationSpawnPoint>();
            AddSpawn(points, parent, "suspect_maintenance_01", ActorRole.Suspect,
                rooms["m7a_maintenance"], new Vector3(-9f, 0.05f, -3f), 1f, 35f);
            AddSpawn(points, parent, "suspect_administration_01", ActorRole.Suspect,
                rooms["m7a_administration"], new Vector3(10f, 0.05f, -1f), 1f, 190f);
            AddSpawn(points, parent, "suspect_pump_01", ActorRole.Suspect,
                rooms[TargetClearanceRoomId], new Vector3(-11f, 0.05f, 13f), 1.35f, 145f);
            AddSpawn(points, parent, "suspect_pump_02", ActorRole.Suspect,
                rooms[TargetClearanceRoomId], new Vector3(3f, 0.05f, 13.5f), 1.2f, 210f);
            AddSpawn(points, parent, "suspect_control_01", ActorRole.Suspect,
                rooms["m7a_control_room"], new Vector3(11f, 0.05f, 9f), 1.25f, 180f);
            AddSpawn(points, parent, "suspect_corridor_01", ActorRole.Suspect,
                rooms["m7a_central_corridor"], new Vector3(-7f, 0.05f, 4f), 0.7f, 90f);

            AddSpawn(points, parent, "civilian_reception_01", ActorRole.Civilian,
                rooms["m7a_reception"], new Vector3(2f, 0.05f, -4f), 1.1f, 180f);
            AddSpawn(points, parent, "civilian_maintenance_01", ActorRole.Civilian,
                rooms["m7a_maintenance"], new Vector3(-12f, 0.05f, -5f), 1f, 40f);
            AddSpawn(points, parent, "civilian_administration_01", ActorRole.Civilian,
                rooms["m7a_administration"], new Vector3(12f, 0.05f, -5f), 1.2f, 225f);
            AddSpawn(points, parent, "civilian_pump_01", ActorRole.Civilian,
                rooms[TargetClearanceRoomId], new Vector3(-2f, 0.05f, 8f), 0.9f, 15f);
            AddSpawn(points, parent, "civilian_control_01", ActorRole.Civilian,
                rooms["m7a_control_room"], new Vector3(13f, 0.05f, 13f), 1.2f, 160f);
            AddSpawn(points, parent, "civilian_corridor_01", ActorRole.Civilian,
                rooms["m7a_central_corridor"], new Vector3(6f, 0.05f, 4f), 0.7f, 270f);
            return points.ToArray();
        }

        private static void AddSpawn(
            ICollection<OperationSpawnPoint> points,
            Transform parent,
            string id,
            ActorRole role,
            OperationRoomNode room,
            Vector3 position,
            float weight,
            float yaw)
        {
            GameObject spawnObject = new GameObject("Spawn_" + id);
            spawnObject.transform.SetParent(parent, false);
            spawnObject.transform.SetPositionAndRotation(
                position,
                Quaternion.Euler(0f, yaw, 0f));
            OperationSpawnPoint point = spawnObject.AddComponent<OperationSpawnPoint>();
            point.Configure(id, role, room, weight);
            points.Add(point);
        }

        private static OperationEntryAnchor[] ConfigureEntryAnchors(
            OperationEntryAnchor[] anchors)
        {
            Dictionary<string, (Vector3 position, float yaw)> poses =
                new Dictionary<string, (Vector3, float)>(StringComparer.Ordinal)
                {
                    ["entry_south_administration"] =
                        (new Vector3(0f, 0.05f, -14f), 0f),
                    ["entry_west_service_yard"] =
                        (new Vector3(-21f, 0.05f, -3f), 90f),
                    ["entry_north_pipe_gallery"] =
                        (new Vector3(-6f, 0.05f, 22f), 180f)
                };
            foreach (OperationEntryAnchor anchor in anchors)
            {
                if (!poses.TryGetValue(
                    anchor.EntryPointId,
                    out (Vector3 position, float yaw) pose))
                {
                    throw new InvalidOperationException(
                        $"Unexpected Milestone 6C entry anchor '{anchor.EntryPointId}'.");
                }

                anchor.transform.SetPositionAndRotation(
                    pose.position,
                    Quaternion.Euler(0f, pose.yaw, 0f));
                EditorUtility.SetDirty(anchor);
            }

            if (anchors.Length != poses.Count
                || poses.Keys.Any(id => !anchors.Any(anchor =>
                    string.Equals(anchor.EntryPointId, id, StringComparison.Ordinal))))
            {
                throw new InvalidOperationException(
                    "Pressure Point requires all three Milestone 6C entry anchors.");
            }

            return anchors.OrderBy(anchor => anchor.EntryPointId, StringComparer.Ordinal).ToArray();
        }

        private static void SetDirectPlayStaging(
            SceneDependencies dependencies,
            IReadOnlyList<OperationEntryAnchor> anchors,
            IReadOnlyList<OperationSpawnPoint> spawnPoints)
        {
            OperationEntryAnchor south = anchors.First(anchor => string.Equals(
                anchor.EntryPointId,
                "entry_south_administration",
                StringComparison.Ordinal));
            CharacterController character =
                dependencies.Player.GetComponent<CharacterController>();
            bool characterEnabled = character != null && character.enabled;
            if (character != null)
            {
                character.enabled = false;
            }

            dependencies.Player.transform.SetPositionAndRotation(
                south.PlayerSpawn.position,
                south.PlayerSpawn.rotation);
            if (character != null)
            {
                character.enabled = characterEnabled;
            }

            for (int index = 0; index < dependencies.Squad.Officers.Count; index++)
            {
                TacticalOfficerController officer = dependencies.Squad.Officers[index];
                Transform spawn = south.GetOfficerSpawn(index);
                if (officer != null && spawn != null)
                {
                    officer.transform.SetPositionAndRotation(
                        spawn.position,
                        spawn.rotation);
                }
            }

            foreach (HumanActorController actor in dependencies.IncidentActors)
            {
                ActorIdentity identity = actor.GetComponent<ActorIdentity>();
                OperationSpawnPoint point = spawnPoints.First(candidate =>
                    candidate.ActorRole == identity.Role);
                actor.transform.SetPositionAndRotation(point.Position, point.Rotation);
            }

            Physics.SyncTransforms();
        }

        private static void RepositionDebriefConsole(Scene scene)
        {
            MissionDebriefInteractable debrief = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<MissionDebriefInteractable>(true))
                .FirstOrDefault();
            if (debrief != null)
            {
                debrief.transform.SetPositionAndRotation(
                    new Vector3(10f, 0.7f, -12f),
                    Quaternion.Euler(0f, 180f, 0f));
                EditorUtility.SetDirty(debrief);
            }
        }

        private static void BuildAndPersistNavMesh(
            NavMeshSurface surface,
            IEnumerable<OperationPortal> portals)
        {
            if (AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshDataPath) != null)
            {
                AssetDatabase.DeleteAsset(NavMeshDataPath);
            }

            surface.BuildNavMesh();
            NavMeshData data = surface.navMeshData;
            if (data == null)
            {
                throw new InvalidOperationException(
                    "AI Navigation did not produce NavMesh data for Pressure Point.");
            }

            if (!AssetDatabase.Contains(data))
            {
                data.name = "M7A Pressure Point NavMesh";
                AssetDatabase.CreateAsset(data, NavMeshDataPath);
            }

            foreach (OperationPortal portal in portals)
            {
                portal.TraversalLink?.NavigationLink?.UpdateLink();
            }

            EditorUtility.SetDirty(surface);
        }

        private static SceneDependencies RequireSceneDependencies(Scene scene)
        {
            GameObject player = scene.GetRootGameObjects().FirstOrDefault(root =>
                string.Equals(root.name, PlayerRootName, StringComparison.Ordinal));
            OfficerSquadController squad = player != null
                ? player.GetComponent<OfficerSquadController>()
                : null;
            OperationEntryAnchor[] anchors = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<OperationEntryAnchor>(true))
                .ToArray();
            OperationDeploymentCoordinator deployment = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<OperationDeploymentCoordinator>(true))
                .FirstOrDefault();
            HumanActorController[] actors = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<HumanActorController>(true))
                .Where(actor => actor.GetComponent<ActorIdentity>()?.Role
                    != ActorRole.Officer)
                .ToArray();
            MissionController mission = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MissionController>(true))
                .FirstOrDefault();
            if (player == null
                || player.GetComponent<FirstPersonMotor>() == null
                || squad == null
                || !squad.HasCompleteConfiguration
                || anchors.Length != 3
                || anchors.Any(anchor => !anchor.HasValidConfiguration)
                || deployment == null
                || !deployment.HasCompleteConfiguration
                || actors.Length < 2
                || mission == null
                || !mission.HasCompleteConfiguration)
            {
                throw new InvalidOperationException(
                    "Milestones 1 through 6C must be installed and valid before Milestone 7A. "
                        + "Rerun the Milestone 6C setup tool, then retry.");
            }

            return new SceneDependencies(player, squad, anchors, actors);
        }

        private static void RemovePreviousGeneratedContent(Scene scene)
        {
            GameObject previous = scene.GetRootGameObjects().FirstOrDefault(root =>
                string.Equals(root.name, GeneratedRootName, StringComparison.Ordinal));
            if (previous == null)
            {
                return;
            }

            NavMeshSurface surface = previous.GetComponent<NavMeshSurface>();
            surface?.RemoveData();
            UnityEngine.Object.DestroyImmediate(previous);
        }

        private static void DisableLegacyEnvironment(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects().Where(root =>
                string.Equals(root.name, LegacyGrayboxRootName, StringComparison.Ordinal)
                || string.Equals(root.name, LegacyRangeRootName, StringComparison.Ordinal)))
            {
                root.SetActive(false);
                EditorUtility.SetDirty(root);
            }
        }

        private static void RemoveLegacyRoomAndDoorLink(Scene scene)
        {
            Transform[] legacy = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(transform => string.Equals(
                        transform.name,
                        LegacyRoomName,
                        StringComparison.Ordinal)
                    || string.Equals(
                        transform.name,
                        LegacyDoorLinkName,
                        StringComparison.Ordinal))
                .ToArray();
            foreach (Transform item in legacy)
            {
                UnityEngine.Object.DestroyImmediate(item.gameObject);
            }
        }

        private static void RemovePreviousNavMeshSurfaces(Scene scene)
        {
            NavMeshSurface[] surfaces = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<NavMeshSurface>(true))
                .ToArray();
            foreach (NavMeshSurface surface in surfaces)
            {
                surface.RemoveData();
                UnityEngine.Object.DestroyImmediate(surface);
            }
        }

        private static void CreateSegmentedWall(
            string name,
            Transform parent,
            bool horizontal,
            float fixedCoordinate,
            float minimum,
            float maximum,
            IReadOnlyList<WallOpening> openings,
            Material material)
        {
            float cursor = minimum;
            int segment = 1;
            foreach (WallOpening opening in (openings ?? Array.Empty<WallOpening>())
                .OrderBy(value => value.Center))
            {
                float start = Mathf.Clamp(
                    opening.Center - opening.Width * 0.5f,
                    minimum,
                    maximum);
                float end = Mathf.Clamp(
                    opening.Center + opening.Width * 0.5f,
                    minimum,
                    maximum);
                if (start > cursor + 0.05f)
                {
                    CreateWallSegment(
                        name + $"_{segment++:00}",
                        parent,
                        horizontal,
                        fixedCoordinate,
                        cursor,
                        start,
                        material);
                }

                cursor = Mathf.Max(cursor, end);
            }

            if (cursor < maximum - 0.05f)
            {
                CreateWallSegment(
                    name + $"_{segment:00}",
                    parent,
                    horizontal,
                    fixedCoordinate,
                    cursor,
                    maximum,
                    material);
            }
        }

        private static void CreateWallSegment(
            string name,
            Transform parent,
            bool horizontal,
            float fixedCoordinate,
            float start,
            float end,
            Material material)
        {
            float length = end - start;
            Vector3 position = horizontal
                ? new Vector3((start + end) * 0.5f, 1.5f, fixedCoordinate)
                : new Vector3(fixedCoordinate, 1.5f, (start + end) * 0.5f);
            Vector3 scale = horizontal
                ? new Vector3(length, 3f, 0.28f)
                : new Vector3(0.28f, 3f, length);
            CreateBlock(name, parent, position, scale, material, true);
        }

        private static GameObject CreateBlock(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool keepCollider)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.position = position;
            block.transform.localScale = scale;
            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            if (!keepCollider)
            {
                Collider collider = block.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }

            return block;
        }

        private static void CreateAreaLabel(
            Transform parent,
            string label,
            Vector3 position)
        {
            GameObject labelObject = new GameObject(label.Replace(" ", string.Empty) + "Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.SetPositionAndRotation(
                position,
                Quaternion.Euler(0f, 180f, 0f));
            TextMesh text = labelObject.AddComponent<TextMesh>();
            text.text = label;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 42;
            text.characterSize = 0.055f;
            text.color = new Color(0.42f, 0.78f, 1f, 0.72f);
        }

        private static GameObject CreateChild(string name, Transform parent)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static Material CreateOrUpdateMaterial(
            string path,
            Color color,
            float smoothness,
            float metallic)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("HDRP/Lit");
                if (shader == null)
                {
                    throw new InvalidOperationException(
                        "HDRP/Lit shader could not be found.");
                }

                material = new Material(shader)
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(path)
                };
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private static int RequireLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                throw new InvalidOperationException(
                    $"Required layer '{layerName}' is missing. Rebuild Milestone 1 first.");
            }

            return layer;
        }

        private static void EnsureFolder(string assetPath)
        {
            string[] segments = assetPath.Split('/');
            string current = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }

        private sealed class SceneDependencies
        {
            public SceneDependencies(
                GameObject player,
                OfficerSquadController squad,
                OperationEntryAnchor[] entryAnchors,
                HumanActorController[] incidentActors)
            {
                Player = player;
                Squad = squad;
                EntryAnchors = entryAnchors;
                IncidentActors = incidentActors;
            }

            public GameObject Player { get; }
            public OfficerSquadController Squad { get; }
            public OperationEntryAnchor[] EntryAnchors { get; }
            public HumanActorController[] IncidentActors { get; }
        }
    }
}
