using System;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone1;
using RulesOfEntry.Editor.Milestone5;
using RulesOfEntry.Editor.UiPresentation;
using RulesOfEntry.Headquarters;
using RulesOfEntry.Input;
using RulesOfEntry.Interaction;
using RulesOfEntry.Missions;
using RulesOfEntry.Officers;
using RulesOfEntry.Planning;
using RulesOfEntry.Player;
using RulesOfEntry.UI;
using RulesOfEntry.UI.Planning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone6
{
    public static class RulesOfEntryMilestoneSixHeadquartersSetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 6A/Build Headquarters and Tablet Prototype";

        internal const string HeadquartersRootName = "[Milestone6A_Headquarters]";
        internal const string TabletRootName = "ROE_RuggedPlanningTablet";
        internal const string MissionTerminalName = "M6A_MissionAssignmentTerminal";
        internal const string BriefingAssetPath =
            "Assets/_Project/RulesOfEntry/Data/Planning/M6_PressurePointBriefing.asset";
        internal const string TabletHardwareArtworkPath =
            "Assets/_Project/RulesOfEntry/Art/UI/Planning/RuggedTabletHardwareCutout.png";

        private const string PlanningDataFolder =
            "Assets/_Project/RulesOfEntry/Data/Planning";
        private const string HeadquartersSceneFolder =
            "Assets/_Project/RulesOfEntry/Scenes/Headquarters";
        private const string HeadquartersMaterialFolder =
            "Assets/_Project/RulesOfEntry/Art/Materials/Headquarters";
        private const string TypographyPath =
            "Assets/_Project/RulesOfEntry/Art/UI/Fonts/LatinModernSansDemiCondensed.otf";

        private static readonly Color TabletRaised =
            new Color(0.028f, 0.034f, 0.038f, 1f);
        private static readonly Color ScreenBackground =
            new Color(0.003f, 0.009f, 0.014f, 1f);
        private static readonly Color ScreenLine =
            new Color(0.08f, 0.3f, 0.43f, 0.82f);
        private static readonly Color Signal =
            new Color(0.15f, 0.63f, 0.87f, 1f);
        private static readonly Color TextPrimary =
            new Color(0.91f, 0.95f, 0.97f, 1f);
        private static readonly Color TextSecondary =
            new Color(0.57f, 0.68f, 0.74f, 1f);

        [MenuItem(MenuPath, false, 80)]
        public static void BuildHeadquartersAndTabletPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building the Milestone 6A headquarters.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning(
                    "Milestone 6A",
                    "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                RequireMilestoneFiveBaseline();
                EnsureFolder(PlanningDataFolder);
                EnsureFolder(HeadquartersSceneFolder);
                EnsureFolder(HeadquartersMaterialFolder);
                ConfigureTabletHardwareTexture();

                MissionDefinition mission = ConfigurePressurePointMission();
                OperationBriefingDefinition briefing = CreateOrUpdateBriefing(mission);
                BuildHeadquartersScene(briefing);

                // Rebuild the authored front end only after the headquarters
                // scene exists so its destination and build order are valid.
                RulesOfEntryUiPresentationSetup.BuildFrontEndAndHud();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                ProjectLog.Info(
                    "Milestone 6A",
                    "Headquarters campaign hub, physical mission selection, and rugged planning tablet created. Running validation now.");
                RulesOfEntryMilestoneSixHeadquartersValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Milestone 6A", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 6A setup stopped. Check the first Console error for the root cause.",
                    "OK");
            }
        }

        private static void RequireMilestoneFiveBaseline()
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneFiveValidator
                .RunValidation(false)
                .Where(result => result.Severity == ProjectValidationSeverity.Error)
                .ToArray();
            if (errors.Length == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                "Milestone 5 must pass before Milestone 6A. "
                + string.Join(" | ", errors.Select(error =>
                    $"{error.Check}: {error.Message}")));
        }

        private static MissionDefinition ConfigurePressurePointMission()
        {
            MissionDefinition mission =
                AssetDatabase.LoadAssetAtPath<MissionDefinition>(
                    RulesOfEntryMilestoneFiveSetup.MissionDefinitionPath);
            if (mission == null)
            {
                throw new InvalidOperationException(
                    "The Milestone 5 mission asset is missing. Rebuild Milestone 5 first.");
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
            MissionObjectiveDefinition clearInterior = CreateObjective(
                "verify_process_room_clear",
                "Verify the process room clear",
                "Confirm that no active threat remains in the primary interior room.",
                MissionObjectiveType.VerifyRoomClear,
                string.Empty,
                "prototype_north_training_room",
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
                "Respond to an armed-subject incident at a municipal pumping annex. Establish control, protect the reported employee, resolve the subject lawfully, and document every use of force.",
                new[]
                {
                    secureSuspect,
                    protectCivilian,
                    clearInterior,
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
            int failureDeduction)
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
                failureDeduction);
            return objective;
        }

        private static OperationBriefingDefinition CreateOrUpdateBriefing(
            MissionDefinition mission)
        {
            OperationBriefingDefinition briefing =
                AssetDatabase.LoadAssetAtPath<OperationBriefingDefinition>(
                    BriefingAssetPath);
            if (briefing == null)
            {
                briefing = ScriptableObject.CreateInstance<OperationBriefingDefinition>();
                briefing.name = "M6 Pressure Point Briefing";
                AssetDatabase.CreateAsset(briefing, BriefingAssetPath);
            }

            briefing.Configure(
                "OP 06-01 / PRESSURE POINT",
                mission,
                ProjectInfo.PrototypeScenePath,
                "Calder City Municipal Pumping Annex",
                "Armed barricaded subject / employee unaccounted for",
                "22:40 local",
                "Heavy rain, reduced exterior visibility, active industrial machinery",
                "Patrol established an initial perimeter after an employee reported an armed former contractor inside the annex. One municipal employee has not been located. The subject has access to maintenance corridors and may attempt to leave through the service yard. Exact weapon status and interior barricades are unconfirmed.",
                "Emergency response to an armed barricaded subject, protective sweep for the unaccounted employee, and lawful arrest authority based on reported violent conduct. Final warrant language remains subject to mission-authoring review.",
                "Use de-escalation and clear commands when feasible. Deadly force requires an objectively reasonable belief of an imminent deadly threat. Every discharge, injury, custody action, and failure to protect will be reviewed after the operation.",
                CreateEntryPoints(),
                CreateOfficerRoster(),
                CreateSupportRoster());
            EditorUtility.SetDirty(briefing);
            return briefing;
        }

        private static OperationEntryPointDefinition[] CreateEntryPoints()
        {
            return new[]
            {
                CreateEntry(
                    "entry_south_administration",
                    "South Administration Entrance",
                    "Covered approach from the employee parking area into the administrative corridor.",
                    "Most direct path, but glass panels expose the team during the initial threshold assessment."),
                CreateEntry(
                    "entry_west_service_yard",
                    "West Service Yard",
                    "Approach through the fenced maintenance yard near the loading and pump-access doors.",
                    "Machinery noise masks movement but limits verbal-command range and sight lines."),
                CreateEntry(
                    "entry_north_pipe_gallery",
                    "North Pipe Gallery",
                    "Longer exterior movement to a narrow utility access beside the pipe gallery.",
                    "Confined interior route with limited room for team movement and uncertain barricades.")
            };
        }

        private static OperationEntryPointDefinition CreateEntry(
            string id,
            string displayName,
            string approach,
            string risk)
        {
            OperationEntryPointDefinition entry = new OperationEntryPointDefinition();
            entry.Configure(id, displayName, approach, risk);
            return entry;
        }

        private static OperationOfficerDefinition[] CreateOfficerRoster()
        {
            return new[]
            {
                CreateOfficer(
                    "m4_officer_alpha",
                    "Officer Alpha",
                    "Element Lead",
                    "Team leadership / carbine / less-lethal qualified"),
                CreateOfficer(
                    "m4_officer_bravo",
                    "Officer Bravo",
                    "Cover Officer",
                    "Rear security / carbine / custody qualified")
            };
        }

        private static OperationOfficerDefinition CreateOfficer(
            string id,
            string displayName,
            string role,
            string qualification)
        {
            OperationOfficerDefinition officer = new OperationOfficerDefinition();
            officer.Configure(id, displayName, role, qualification, true, true);
            return officer;
        }

        private static OperationSupportDefinition[] CreateSupportRoster()
        {
            return new[]
            {
                CreateFutureSupport(
                    "support_k9_01",
                    "K9 Search Team",
                    OperationSupportType.K9,
                    "Tracking, building search, suspect location, and evidence search."),
                CreateFutureSupport(
                    "support_drone_01",
                    "Tactical Drone Team",
                    OperationSupportType.Drone,
                    "Remote exterior and interior reconnaissance with live command feed."),
                CreateFutureSupport(
                    "support_medic_01",
                    "Tactical Medic",
                    OperationSupportType.TacticalMedic,
                    "Warm-zone casualty care and coordinated transfer to EMS."),
                CreateFutureSupport(
                    "support_negotiator_01",
                    "Crisis Negotiation Team",
                    OperationSupportType.Negotiator,
                    "Structured contact, behavioral assessment, and surrender facilitation.")
            };
        }

        private static OperationSupportDefinition CreateFutureSupport(
            string id,
            string displayName,
            OperationSupportType type,
            string capability)
        {
            OperationSupportDefinition support = new OperationSupportDefinition();
            support.Configure(id, displayName, type, capability, false, false);
            return support;
        }

        private static void ConfigureTabletHardwareTexture()
        {
            TextureImporter importer = AssetImporter.GetAtPath(
                TabletHardwareArtworkPath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Required rugged-tablet hardware cutout could not be imported: {TabletHardwareArtworkPath}");
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void BuildHeadquartersScene(
            OperationBriefingDefinition briefing)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneOneSetup.PlayerPrefabPath);
            GameObject promptPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneOneSetup.PromptPrefabPath);
            Font font = AssetDatabase.LoadAssetAtPath<Font>(TypographyPath);
            Sprite tabletHardware = AssetDatabase.LoadAssetAtPath<Sprite>(
                TabletHardwareArtworkPath);
            if (playerPrefab == null
                || promptPrefab == null
                || font == null
                || tabletHardware == null)
            {
                throw new InvalidOperationException(
                    "The player, interaction prompt, UI typography, or transparent rugged-tablet hardware artwork is missing. Reinstall Milestone 6A or rebuild the required baseline.");
            }

            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer < 0)
            {
                throw new InvalidOperationException(
                    "The Interactable layer is missing. Rebuild Milestone 1.");
            }

            Material floor = CreateOrUpdateMaterial(
                HeadquartersMaterialFolder + "/M6A_HQFloor.mat",
                new Color(0.085f, 0.095f, 0.105f, 1f),
                0.12f,
                0.36f);
            Material wall = CreateOrUpdateMaterial(
                HeadquartersMaterialFolder + "/M6A_HQWall.mat",
                new Color(0.19f, 0.205f, 0.215f, 1f),
                0f,
                0.3f);
            Material dark = CreateOrUpdateMaterial(
                HeadquartersMaterialFolder + "/M6A_HQDark.mat",
                new Color(0.03f, 0.04f, 0.05f, 1f),
                0.15f,
                0.48f);
            Material accent = CreateOrUpdateMaterial(
                HeadquartersMaterialFolder + "/M6A_HQAccent.mat",
                new Color(0.025f, 0.32f, 0.62f, 1f),
                0.18f,
                0.5f);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject root = new GameObject(HeadquartersRootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            BuildHeadquartersGraybox(root.transform, floor, wall, dark, accent);
            BuildLighting(root.transform);

            GameObject player = InstantiatePrefab(playerPrefab, scene, root.transform);
            player.name = "ROE_Player";
            player.transform.SetPositionAndRotation(
                new Vector3(0f, 0.05f, -7.5f),
                Quaternion.identity);
            ConfigureHeadquartersPlayerMode(player);
            TacticalPlayerInput playerInput = player.GetComponent<TacticalPlayerInput>();
            CursorStateController cursor = player.GetComponent<CursorStateController>();
            PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();
            if (playerInput == null || cursor == null || interactor == null)
            {
                throw new InvalidOperationException(
                    "ROE_Player is missing required input, cursor, or interaction components.");
            }

            GameObject promptObject = InstantiatePrefab(promptPrefab, scene, root.transform);
            promptObject.name = "ROE_InteractionPromptUI";
            InteractionPromptUI prompt = promptObject.GetComponent<InteractionPromptUI>();
            prompt.ConfigureSources(interactor, playerInput);

            EnsureEventSystem(root.transform);
            RuggedTabletController tablet = CreateTablet(
                root.transform,
                font,
                playerInput,
                cursor,
                briefing,
                tabletHardware);
            CreateMissionTerminal(
                root.transform,
                briefing,
                tablet,
                dark,
                accent,
                font,
                interactableLayer);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ProjectInfo.HeadquartersScenePath))
            {
                throw new InvalidOperationException(
                    $"Unity could not save {ProjectInfo.HeadquartersScenePath}.");
            }
        }

        private static void ConfigureHeadquartersPlayerMode(GameObject player)
        {
            OfficerSquadController squad = player.GetComponent<OfficerSquadController>();
            if (squad == null)
            {
                throw new InvalidOperationException(
                    "ROE_Player is missing the Milestone 4 squad-command component.");
            }

            // The reusable player prefab intentionally has no scene officer or marker
            // references. Operational scenes enable and configure this component,
            // while the headquarters has no deployable squad to command.
            squad.enabled = false;
            EditorUtility.SetDirty(squad);
            if (PrefabUtility.IsPartOfPrefabInstance(squad))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(squad);
            }
        }

        private static void BuildHeadquartersGraybox(
            Transform parent,
            Material floor,
            Material wall,
            Material dark,
            Material accent)
        {
            Transform architecture = new GameObject("Architecture").transform;
            architecture.SetParent(parent, false);
            CreateCube(
                "HeadquartersFloor",
                architecture,
                new Vector3(0f, -0.1f, 0f),
                new Vector3(24f, 0.2f, 20f),
                floor);
            CreateCube("NorthWall", architecture, new Vector3(0f, 2.1f, 10f), new Vector3(24f, 4.2f, 0.25f), wall);
            CreateCube("SouthWall", architecture, new Vector3(0f, 2.1f, -10f), new Vector3(24f, 4.2f, 0.25f), wall);
            CreateCube("WestWall", architecture, new Vector3(-12f, 2.1f, 0f), new Vector3(0.25f, 4.2f, 20f), wall);
            CreateCube("EastWall", architecture, new Vector3(12f, 2.1f, 0f), new Vector3(0.25f, 4.2f, 20f), wall);

            CreateCube("OperationsDivider", architecture, new Vector3(0f, 1.45f, 3.2f), new Vector3(10f, 2.9f, 0.18f), dark);
            CreateCube("WestOfficeDivider", architecture, new Vector3(-6.2f, 1.45f, 4.8f), new Vector3(0.18f, 2.9f, 10.2f), wall);
            CreateCube("EastOfficeDivider", architecture, new Vector3(6.2f, 1.45f, 4.8f), new Vector3(0.18f, 2.9f, 10.2f), wall);

            CreateZone("LOADOUT CAGE", new Vector3(-8.9f, 0.04f, 5.7f), new Vector3(5.3f, 0.05f, 7.4f), accent, parent);
            CreateZone("OPERATIONS / MISSION ASSIGNMENT", new Vector3(0f, 0.04f, 6.2f), new Vector3(6.8f, 0.05f, 6.3f), accent, parent);
            CreateZone("OFFICER MANAGEMENT", new Vector3(8.9f, 0.04f, 5.7f), new Vector3(5.3f, 0.05f, 7.4f), accent, parent);
            CreateZone("SHOOT HOUSE / TRAINING ACCESS", new Vector3(-6.9f, 0.04f, -4.9f), new Vector3(6.6f, 0.05f, 6.2f), accent, parent);
            CreateZone("EQUIPMENT AND SUPPORT STAGING", new Vector3(6.9f, 0.04f, -4.9f), new Vector3(6.6f, 0.05f, 6.2f), accent, parent);

            CreateCounter("LoadoutCounter", new Vector3(-8.9f, 0.65f, 3.2f), new Vector3(4.6f, 1.3f, 0.75f), dark, parent);
            CreateCounter("OfficerDesk", new Vector3(8.9f, 0.65f, 3.2f), new Vector3(4.6f, 1.3f, 0.75f), dark, parent);
        }

        private static void CreateZone(
            string label,
            Vector3 position,
            Vector3 size,
            Material material,
            Transform parent)
        {
            GameObject stripe = CreateCube(
                label.Replace(' ', '_') + "_FloorMarker",
                parent,
                position,
                size,
                material);
            Collider collider = stripe.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            CreateWorldLabel(
                label,
                parent,
                position + new Vector3(0f, 0.07f, 0f),
                Quaternion.Euler(90f, 0f, 0f),
                0.18f,
                Color.white);
        }

        private static void CreateCounter(
            string name,
            Vector3 position,
            Vector3 size,
            Material material,
            Transform parent)
        {
            CreateCube(name, parent, position, size, material);
        }

        private static void BuildLighting(Transform parent)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.2f, 0.22f, 1f);

            GameObject directionalObject = new GameObject("HeadquartersDirectionalLight");
            directionalObject.transform.SetParent(parent, false);
            directionalObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            Light directional = directionalObject.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.intensity = 0.65f;
            directional.color = new Color(0.78f, 0.86f, 0.95f, 1f);

            Vector3[] positions =
            {
                new Vector3(-7f, 3.4f, -4f),
                new Vector3(7f, 3.4f, -4f),
                new Vector3(-7f, 3.4f, 5f),
                new Vector3(0f, 3.4f, 5f),
                new Vector3(7f, 3.4f, 5f)
            };
            for (int index = 0; index < positions.Length; index++)
            {
                GameObject lightObject = new GameObject($"CeilingLight_{index + 1:00}");
                lightObject.transform.SetParent(parent, false);
                lightObject.transform.localPosition = positions[index];
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 9f;
                light.intensity = 850f;
                light.color = new Color(0.72f, 0.84f, 1f, 1f);
            }
        }

        private static void EnsureEventSystem(Transform parent)
        {
            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(parent, false);
        }

        private static RuggedTabletController CreateTablet(
            Transform parent,
            Font font,
            TacticalPlayerInput playerInput,
            CursorStateController cursor,
            OperationBriefingDefinition defaultBriefing,
            Sprite hardwareCutout)
        {
            GameObject canvasObject = new GameObject(
                TabletRootName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(RuggedTabletController));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject tabletObject = CreateUiObject(
                "TabletInterface",
                canvasObject.transform,
                typeof(CanvasGroup),
                typeof(Image));
            RectTransform tabletRoot = tabletObject.GetComponent<RectTransform>();
            Stretch(tabletRoot);
            tabletObject.GetComponent<Image>().color = Color.clear;
            CanvasGroup tabletGroup = tabletObject.GetComponent<CanvasGroup>();

            Image device = CreateImage("RuggedDevice", tabletRoot, Color.white);
            Stretch(device.rectTransform);
            device.sprite = hardwareCutout;
            device.preserveAspect = true;
            device.raycastTarget = false;
            AspectRatioFitter hardwareFitter =
                device.gameObject.AddComponent<AspectRatioFitter>();
            hardwareFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            hardwareFitter.aspectRatio = 1672f / 941f;

            // The transparent cutout supplies only the rugged hardware. The
            // actual headquarters remains visible behind it, while this opaque
            // live screen covers the cutout's example display.
            Image screen = CreateImage(
                "TacticalDisplay",
                device.transform,
                ScreenBackground);
            SetNormalizedRect(
                screen.rectTransform,
                new Vector2(0.207f, 0.184f),
                new Vector2(0.793f, 0.826f));
            screen.raycastTarget = false;
            AddOutline(
                screen,
                new Color(0.035f, 0.12f, 0.17f, 1f),
                new Vector2(1f, -1f));

            Text operationHeader = CreateText("OperationHeader", screen.transform, font, 20, FontStyle.Bold, TextAnchor.MiddleLeft, Signal);
            SetNormalizedRect(operationHeader.rectTransform, new Vector2(0.03f, 0.925f), new Vector2(0.39f, 0.98f));
            operationHeader.verticalOverflow = VerticalWrapMode.Overflow;

            Text metadata = CreateText("OperationMetadata", screen.transform, font, 15, FontStyle.Bold, TextAnchor.MiddleRight, TextSecondary);
            SetNormalizedRect(metadata.rectTransform, new Vector2(0.41f, 0.925f), new Vector2(0.97f, 0.98f));

            Image headerLine = CreateImage("HeaderLine", screen.transform, Signal);
            SetNormalizedRect(headerLine.rectTransform, new Vector2(0.025f, 0.916f), new Vector2(0.975f, 0.919f));

            float tabLeft = 0.025f;
            float tabRight = 0.975f;
            float tabWidth = (tabRight - tabLeft) / 7f;
            Button overviewTab = CreateTabletButton("OverviewTab", screen.transform, font, "1  OVERVIEW", new Vector2(tabLeft, 0.83f), new Vector2(tabLeft + tabWidth, 0.905f), false, out _);
            Button objectivesTab = CreateTabletButton("ObjectivesTab", screen.transform, font, "2  OBJECTIVES", new Vector2(tabLeft + tabWidth, 0.83f), new Vector2(tabLeft + (tabWidth * 2f), 0.905f), false, out _);
            Button intelligenceTab = CreateTabletButton("IntelligenceTab", screen.transform, font, "3  INTEL", new Vector2(tabLeft + (tabWidth * 2f), 0.83f), new Vector2(tabLeft + (tabWidth * 3f), 0.905f), false, out _);
            Button mapTab = CreateTabletButton("MapTab", screen.transform, font, "4  MAP", new Vector2(tabLeft + (tabWidth * 3f), 0.83f), new Vector2(tabLeft + (tabWidth * 4f), 0.905f), false, out _);
            Button teamTab = CreateTabletButton("TeamTab", screen.transform, font, "5  TEAM", new Vector2(tabLeft + (tabWidth * 4f), 0.83f), new Vector2(tabLeft + (tabWidth * 5f), 0.905f), false, out _);
            Button loadoutTab = CreateTabletButton("LoadoutTab", screen.transform, font, "6  LOADOUT", new Vector2(tabLeft + (tabWidth * 5f), 0.83f), new Vector2(tabLeft + (tabWidth * 6f), 0.905f), false, out _);
            Button roeTab = CreateTabletButton("RoeTab", screen.transform, font, "7  ROE / READY", new Vector2(tabLeft + (tabWidth * 6f), 0.83f), new Vector2(tabRight, 0.905f), false, out _);

            Image leftPanel = CreateImage("LeftDataPanel", screen.transform, new Color(0.006f, 0.015f, 0.021f, 0.96f));
            SetNormalizedRect(leftPanel.rectTransform, new Vector2(0.025f, 0.20f), new Vector2(0.492f, 0.81f));
            AddOutline(leftPanel, ScreenLine, new Vector2(1f, -1f));
            Image leftPanelSignal = CreateImage("SignalLine", leftPanel.transform, Signal);
            SetNormalizedRect(leftPanelSignal.rectTransform, new Vector2(0f, 0.992f), Vector2.one);

            Image rightPanel = CreateImage("RightDataPanel", screen.transform, new Color(0.006f, 0.015f, 0.021f, 0.96f));
            SetNormalizedRect(rightPanel.rectTransform, new Vector2(0.508f, 0.20f), new Vector2(0.975f, 0.81f));
            AddOutline(rightPanel, ScreenLine, new Vector2(1f, -1f));
            Image rightPanelSignal = CreateImage("SignalLine", rightPanel.transform, Signal);
            SetNormalizedRect(rightPanelSignal.rectTransform, new Vector2(0f, 0.992f), Vector2.one);

            Text leftTitle = CreateText("LeftPanelTitle", leftPanel.transform, font, 21, FontStyle.Bold, TextAnchor.UpperLeft, Signal);
            SetNormalizedRect(leftTitle.rectTransform, new Vector2(0.045f, 0.84f), new Vector2(0.955f, 0.96f));
            Text leftBody = CreateText("LeftPanelBody", leftPanel.transform, font, 18, FontStyle.Normal, TextAnchor.UpperLeft, TextPrimary);
            SetNormalizedRect(leftBody.rectTransform, new Vector2(0.045f, 0.055f), new Vector2(0.955f, 0.84f));
            leftBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            leftBody.verticalOverflow = VerticalWrapMode.Truncate;
            leftBody.lineSpacing = 1.08f;

            Text rightTitle = CreateText("RightPanelTitle", rightPanel.transform, font, 21, FontStyle.Bold, TextAnchor.UpperLeft, Signal);
            SetNormalizedRect(rightTitle.rectTransform, new Vector2(0.045f, 0.84f), new Vector2(0.955f, 0.96f));
            Text rightBody = CreateText("RightPanelBody", rightPanel.transform, font, 18, FontStyle.Normal, TextAnchor.UpperLeft, TextPrimary);
            SetNormalizedRect(rightBody.rectTransform, new Vector2(0.045f, 0.055f), new Vector2(0.955f, 0.84f));
            rightBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            rightBody.verticalOverflow = VerticalWrapMode.Truncate;
            rightBody.lineSpacing = 1.08f;

            Text selectionStatus = CreateText("SelectionStatus", screen.transform, font, 15, FontStyle.Bold, TextAnchor.MiddleLeft, TextSecondary);
            SetNormalizedRect(selectionStatus.rectTransform, new Vector2(0.025f, 0.15f), new Vector2(0.975f, 0.195f));

            Button close = CreateTabletButton("CloseButton", screen.transform, font, "CLOSE TABLET", new Vector2(0.025f, 0.06f), new Vector2(0.18f, 0.135f), false, out _);
            Button previous = CreateTabletButton("PreviousButton", screen.transform, font, "PREVIOUS", new Vector2(0.195f, 0.06f), new Vector2(0.365f, 0.135f), false, out Text previousText);
            Button next = CreateTabletButton("NextButton", screen.transform, font, "NEXT", new Vector2(0.38f, 0.06f), new Vector2(0.55f, 0.135f), false, out Text nextText);
            Button primary = CreateTabletButton("PrimaryButton", screen.transform, font, "START OPERATION", new Vector2(0.69f, 0.06f), new Vector2(0.975f, 0.135f), true, out Text primaryText);

            Text deploymentStatus = CreateText("DeploymentStatus", screen.transform, font, 14, FontStyle.Bold, TextAnchor.MiddleLeft, Signal);
            SetNormalizedRect(deploymentStatus.rectTransform, new Vector2(0.035f, 0.01f), new Vector2(0.74f, 0.055f));

            Image progressTrack = CreateImage("LoadingTrack", screen.transform, new Color(0.08f, 0.12f, 0.14f, 1f));
            SetNormalizedRect(progressTrack.rectTransform, new Vector2(0.76f, 0.025f), new Vector2(0.955f, 0.04f));
            Image progressFill = CreateImage("LoadingFill", progressTrack.transform, Signal);
            Stretch(progressFill.rectTransform);
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillAmount = 0f;

            RuggedTabletController controller = canvasObject.GetComponent<RuggedTabletController>();
            controller.Configure(
                playerInput,
                cursor,
                defaultBriefing,
                tabletGroup,
                operationHeader,
                metadata,
                leftTitle,
                leftBody,
                rightTitle,
                rightBody,
                selectionStatus,
                deploymentStatus,
                progressFill,
                overviewTab,
                objectivesTab,
                intelligenceTab,
                mapTab,
                teamTab,
                loadoutTab,
                roeTab,
                previous,
                next,
                primary,
                close,
                previousText,
                nextText,
                primaryText);
            tabletGroup.alpha = 0f;
            tabletGroup.interactable = false;
            tabletGroup.blocksRaycasts = false;
            tabletObject.SetActive(false);
            return controller;
        }

        private static void CreateMissionTerminal(
            Transform parent,
            OperationBriefingDefinition briefing,
            RuggedTabletController tablet,
            Material dark,
            Material accent,
            Font font,
            int interactableLayer)
        {
            GameObject terminal = CreateCube(
                MissionTerminalName,
                parent,
                new Vector3(0f, 0.72f, 1.3f),
                new Vector3(2.8f, 1.45f, 0.65f),
                dark);
            terminal.layer = interactableLayer;
            SetLayerRecursively(terminal, interactableLayer);
            HeadquartersMissionTerminalInteractable interactable =
                terminal.AddComponent<HeadquartersMissionTerminalInteractable>();
            interactable.Configure(tablet, briefing, 0f);

            GameObject display = CreateCube(
                "TerminalDisplay",
                terminal.transform,
                new Vector3(0f, 0.18f, -0.54f),
                new Vector3(0.8f, 0.5f, 0.08f),
                accent);

            CreateWorldLabel(
                "MISSION ASSIGNMENT\nTAB: OPEN TABLET  //  E: REVIEW TERMINAL",
                terminal.transform,
                new Vector3(0f, 0.19f, -0.6f),
                Quaternion.identity,
                0.11f,
                Color.white);
        }

        private static Material CreateOrUpdateMaterial(
            string path,
            Color color,
            float metallic,
            float smoothness)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("HDRP/Lit")
                    ?? Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard");
                if (shader == null)
                {
                    throw new InvalidOperationException(
                        "No supported lit shader is available for headquarters materials.");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreateCube(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = scale;
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            return cube;
        }

        private static void CreateWorldLabel(
            string text,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation,
            float characterSize,
            Color color)
        {
            string safeName = text
                .Replace(' ', '_')
                .Replace('\n', '_')
                .Replace('/', '_');
            GameObject labelObject = new GameObject(safeName + "_Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;
            labelObject.transform.localRotation = localRotation;
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.characterSize = characterSize;
            label.fontSize = 64;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = color;
            Font font = AssetDatabase.LoadAssetAtPath<Font>(TypographyPath);
            if (font != null)
            {
                label.font = font;
                MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = font.material;
                }
            }
        }

        private static Button CreateTabletButton(
            string name,
            Transform parent,
            Font font,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            bool primary,
            out Text labelText)
        {
            Image background = CreateImage(
                name,
                parent,
                primary ? new Color(0.08f, 0.36f, 0.52f, 0.96f) : TabletRaised);
            SetNormalizedRect(background.rectTransform, anchorMin, anchorMax);
            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.72f, 0.82f, 0.9f, 1f);
            colors.selectedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.disabledColor = new Color(0.35f, 0.38f, 0.4f, 0.6f);
            button.colors = colors;
            button.navigation = new UnityEngine.UI.Navigation
            {
                mode = UnityEngine.UI.Navigation.Mode.Automatic
            };

            labelText = CreateText(
                "Label",
                background.transform,
                font,
                name.EndsWith("Tab", StringComparison.Ordinal) ? 15 : 17,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                TextPrimary);
            Stretch(labelText.rectTransform);
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 12;
            labelText.resizeTextMaxSize = name.EndsWith("Tab", StringComparison.Ordinal)
                ? 15
                : 17;
            labelText.text = label;
            return button;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject gameObject = CreateUiObject(name, parent, typeof(Image));
            Image image = gameObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Font font,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            Color color)
        {
            GameObject gameObject = CreateUiObject(name, parent, typeof(Text));
            Text text = gameObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.supportRichText = true;
            return text;
        }

        private static GameObject CreateUiObject(
            string name,
            Transform parent,
            params Type[] components)
        {
            Type[] allComponents = new[] { typeof(RectTransform) }
                .Concat(components)
                .Distinct()
                .ToArray();
            GameObject gameObject = new GameObject(name, allComponents);
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void AddOutline(Image image, Color color, Vector2 distance)
        {
            Outline outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetNormalizedRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Anchor(
            RectTransform rect,
            Vector2 anchor,
            Vector2 position,
            Vector2 size,
            Vector2 pivot)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static GameObject InstantiatePrefab(
            GameObject prefab,
            Scene scene,
            Transform parent)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene)
                as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Unity could not instantiate prefab {prefab.name}.");
            }

            if (parent != null)
            {
                instance.transform.SetParent(parent, true);
            }

            return instance;
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private static void EnsureFolder(string path)
        {
            string[] segments = path.Split('/');
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
    }
}
