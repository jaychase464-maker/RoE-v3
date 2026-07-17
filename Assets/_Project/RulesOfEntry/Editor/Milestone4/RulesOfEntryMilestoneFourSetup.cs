using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RulesOfEntry.Actors;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Milestone3;
using RulesOfEntry.Input;
using RulesOfEntry.Interaction;
using RulesOfEntry.Navigation;
using RulesOfEntry.Officers;
using RulesOfEntry.UI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone4
{
    public static class RulesOfEntryMilestoneFourSetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 4/Build Officer Team Prototype";

        internal const string OfficerAlphaPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_OfficerAlpha.prefab";
        internal const string OfficerBravoPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_OfficerBravo.prefab";
        internal const string MarkerPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_OfficerOrderMarker.prefab";
        internal const string DebugUiPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_OfficerCommandDebugUI.prefab";
        internal const string InputAssetPath =
            "Assets/_Project/RulesOfEntry/Input/ROE_InputActions.inputactions";
        internal const string PlayerPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_Player.prefab";
        internal const string GeneratedRootName = "[Milestone4_OfficerTeam]";
        internal const string DebugUiRootName = "ROE_OfficerCommandDebugUI";

        private const string ActorPrefabFolder =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors";
        private const string UiPrefabFolder = "Assets/_Project/RulesOfEntry/Prefabs/UI";
        private const string MaterialsFolder = "Assets/_Project/RulesOfEntry/Art/Materials";
        private const string PlayerRootName = "ROE_Player";
        private const string PlayerLayerName = "Player";
        private const string InteractableLayerName = "Interactable";

        [MenuItem(MenuPath, false, 50)]
        public static void BuildOfficerTeamPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building the Milestone 4 prototype.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("Milestone 4", "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                EnsureFolder(ActorPrefabFolder);
                EnsureFolder(UiPrefabFolder);
                EnsureFolder(MaterialsFolder);

                int playerLayer = RequireLayer(PlayerLayerName);
                int interactableLayer = RequireLayer(InteractableLayerName);
                InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                    InputAssetPath);
                if (inputAsset == null)
                {
                    throw new InvalidOperationException(
                        $"Milestone 3 input actions were not found at {InputAssetPath}.");
                }

                inputAsset = EnsureOfficerInputActions(inputAsset);
                Material uniformMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M4_OfficerUniform.mat",
                    new Color(0.055f, 0.09f, 0.14f, 1f),
                    0.32f,
                    0.05f);
                Material armorMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M4_OfficerArmor.mat",
                    new Color(0.012f, 0.018f, 0.024f, 1f),
                    0.23f,
                    0.02f);
                Material skinMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M4_OfficerAccent.mat",
                    new Color(0.43f, 0.32f, 0.25f, 1f),
                    0.36f,
                    0f);
                Material markerMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M4_OrderMarker.mat",
                    new Color(0.08f, 0.62f, 0.88f, 1f),
                    0.45f,
                    1.2f);

                UpdatePlayerPrefab(inputAsset, playerLayer);
                GameObject alphaPrefab = CreateOfficerPrefab(
                    OfficerAlphaPrefabPath,
                    "ROE_OfficerAlpha",
                    "m4_officer_alpha",
                    "Officer Alpha",
                    41001,
                    uniformMaterial,
                    armorMaterial,
                    skinMaterial,
                    interactableLayer);
                GameObject bravoPrefab = CreateOfficerPrefab(
                    OfficerBravoPrefabPath,
                    "ROE_OfficerBravo",
                    "m4_officer_bravo",
                    "Officer Bravo",
                    41002,
                    uniformMaterial,
                    armorMaterial,
                    skinMaterial,
                    interactableLayer);
                GameObject markerPrefab = CreateOrderMarkerPrefab(
                    markerMaterial,
                    interactableLayer);
                GameObject debugUiPrefab = CreateDebugUiPrefab();

                Scene scene = EditorSceneManager.OpenScene(
                    ProjectInfo.PrototypeScenePath,
                    OpenSceneMode.Single);
                SceneManager.SetActiveScene(scene);
                RemovePreviousGeneratedContent(scene);

                GameObject player = scene.GetRootGameObjects().FirstOrDefault(
                    root => string.Equals(root.name, PlayerRootName, StringComparison.Ordinal));
                if (player == null)
                {
                    throw new InvalidOperationException(
                        "ROE_Player is missing. Complete Milestones 1 through 3 before Milestone 4.");
                }

                if (scene.GetRootGameObjects().All(root =>
                        !string.Equals(root.name, "[Milestone3_HumanBehavior]", StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException(
                        "Milestone 3 scene content is missing. Rebuild Milestone 3 before Milestone 4.");
                }

                GameObject generatedRoot = new GameObject(GeneratedRootName);
                SceneManager.MoveGameObjectToScene(generatedRoot, scene);
                CreateRoomClearanceVolume(generatedRoot.transform);
                GameObject alpha = InstantiatePrefab(alphaPrefab, scene, generatedRoot.transform);
                alpha.name = "M4_OfficerAlpha";
                alpha.transform.SetPositionAndRotation(
                    new Vector3(-1.05f, 0f, -3.55f),
                    Quaternion.identity);
                GameObject bravo = InstantiatePrefab(bravoPrefab, scene, generatedRoot.transform);
                bravo.name = "M4_OfficerBravo";
                bravo.transform.SetPositionAndRotation(
                    new Vector3(1.05f, 0f, -3.55f),
                    Quaternion.identity);
                GameObject markerObject = InstantiatePrefab(
                    markerPrefab,
                    scene,
                    generatedRoot.transform);
                markerObject.name = "M4_OrderMarker";
                OfficerOrderMarker marker = markerObject.GetComponent<OfficerOrderMarker>();
                marker.Hide();
                CreateDoorTraversalLink(scene, generatedRoot.transform);

                TacticalPlayerInput tacticalInput = ConfigureScenePlayer(
                    player,
                    inputAsset);
                Camera playerCamera = player.GetComponentInChildren<Camera>(true);
                if (playerCamera == null)
                {
                    throw new InvalidOperationException(
                        "Player camera is missing. Rebuild Milestone 1 before Milestone 4.");
                }

                OfficerSquadController squad = player.GetComponent<OfficerSquadController>();
                if (squad == null)
                {
                    squad = player.AddComponent<OfficerSquadController>();
                }

                squad.Configure(
                    tacticalInput,
                    playerCamera.transform,
                    new[]
                    {
                        alpha.GetComponent<TacticalOfficerController>(),
                        bravo.GetComponent<TacticalOfficerController>()
                    },
                    marker,
                    ~(1 << playerLayer),
                    30f);
                squad.Select(OfficerSelection.Team);
                PersistPrefabInstanceOverrides(squad);
                if (!squad.HasCompleteConfiguration)
                {
                    throw new InvalidOperationException(
                        "Scene squad configuration is incomplete after assignment: "
                            + squad.ConfigurationProblems
                            + ".");
                }

                GameObject debugUi = InstantiatePrefab(debugUiPrefab, scene, null);
                debugUi.name = DebugUiRootName;
                OfficerCommandDebugUI commandUi =
                    debugUi.GetComponent<OfficerCommandDebugUI>();
                commandUi.ConfigureSources(
                    squad,
                    tacticalInput);
                PersistPrefabInstanceOverrides(commandUi);

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException(
                        $"Unity could not save {ProjectInfo.PrototypeScenePath}.");
                }

                GameObject savedSelection = ReloadAndVerifySceneReferences();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeGameObject = savedSelection;
                ProjectLog.Info(
                    "Milestone 4",
                    "Two-officer command, execution, cancellation, door, and assisted-restraint prototype created. Running validation now.");
                RulesOfEntryMilestoneFourValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Milestone 4", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 4 setup stopped. Check the first Console error for the root cause.",
                    "OK");
            }
        }

        private static InputActionAsset EnsureOfficerInputActions(InputActionAsset inputAsset)
        {
            // Input System setup APIs require every map in the asset to be disabled while
            // actions or bindings are changed. The setup tool is never allowed in Play Mode.
            inputAsset.Disable();
            InputActionMap playerMap = inputAsset.FindActionMap("Player", true);
            EnsureButton(playerMap, "SelectOfficerOne", "<Keyboard>/digit1", "Keyboard&Mouse");
            EnsureButton(playerMap, "SelectOfficerTwo", "<Keyboard>/digit2", "Keyboard&Mouse");
            EnsureButton(playerMap, "SelectOfficerTeam", "<Keyboard>/digit3", "Keyboard&Mouse");
            EnsureButton(playerMap, "CycleOfficerSelection", "<Gamepad>/buttonEast", "Gamepad");
            EnsureButton(
                playerMap,
                "IssueOfficerContextOrder",
                "<Gamepad>/dpad/right",
                "Gamepad");
            EnsureButton(playerMap, "OfficerMove", "<Keyboard>/g", "Keyboard&Mouse");
            EnsureButton(playerMap, "OfficerHold", "<Keyboard>/h", "Keyboard&Mouse");
            EnsureButton(playerMap, "OfficerFollow", "<Keyboard>/j", "Keyboard&Mouse");
            EnsureButton(playerMap, "OfficerStack", "<Keyboard>/y", "Keyboard&Mouse");
            EnsureButton(playerMap, "OfficerOpen", "<Keyboard>/u", "Keyboard&Mouse");
            EnsureButton(playerMap, "OfficerRestrain", "<Keyboard>/k", "Keyboard&Mouse");
            EnsureButton(playerMap, "CancelOfficerOrder", "<Keyboard>/z", "Keyboard&Mouse");
            EnsureButton(playerMap, "CancelOfficerOrder", "<Gamepad>/select", "Gamepad");

            // .inputactions files are JSON-backed imported assets. Marking the imported
            // ScriptableObject dirty can appear correct to an immediate validator yet lose
            // setup changes on a domain reload. Persist the authoritative JSON, force an
            // import, and use the reloaded asset for every prefab and scene assignment.
            string absolutePath = Path.GetFullPath(InputAssetPath);
            File.WriteAllText(
                absolutePath,
                inputAsset.ToJson(),
                new UTF8Encoding(false));
            AssetDatabase.ImportAsset(
                InputAssetPath,
                ImportAssetOptions.ForceSynchronousImport
                    | ImportAssetOptions.ForceUpdate);
            InputActionAsset reloaded = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                InputAssetPath);
            if (reloaded == null
                || reloaded.FindAction("Player/CancelOfficerOrder", false) == null)
            {
                throw new InvalidOperationException(
                    "Milestone 4 input actions were written but could not be reloaded. Reimport ROE_InputActions.inputactions and run setup again.");
            }

            return reloaded;
        }

        private static void EnsureButton(
            InputActionMap map,
            string actionName,
            string controlPath,
            string group)
        {
            InputAction action = map.FindAction(actionName, false)
                ?? map.AddAction(
                    actionName,
                    InputActionType.Button,
                    expectedControlLayout: "Button");
            bool bindingExists = action.bindings.Any(binding =>
                string.Equals(binding.path, controlPath, StringComparison.OrdinalIgnoreCase)
                && string.Equals(binding.groups, group, StringComparison.Ordinal));
            if (!bindingExists)
            {
                action.AddBinding(controlPath, groups: group);
            }
        }

        private static void UpdatePlayerPrefab(InputActionAsset inputAsset, int playerLayer)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefabAsset == null)
            {
                throw new InvalidOperationException(
                    $"Missing {PlayerPrefabPath}. Complete Milestone 3 first.");
            }

            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                PlayerInput playerInput = root.GetComponent<PlayerInput>();
                TacticalPlayerInput tacticalInput = root.GetComponent<TacticalPlayerInput>();
                Camera playerCamera = root.GetComponentInChildren<Camera>(true);
                if (playerInput == null || tacticalInput == null || playerCamera == null)
                {
                    throw new InvalidOperationException(
                        "PlayerInput, TacticalPlayerInput, or player camera is missing.");
                }

                playerInput.actions = inputAsset;
                tacticalInput.Configure(playerInput);
                OfficerSquadController squad = root.GetComponent<OfficerSquadController>();
                if (squad == null)
                {
                    squad = root.AddComponent<OfficerSquadController>();
                }

                squad.Configure(
                    tacticalInput,
                    playerCamera.transform,
                    Array.Empty<TacticalOfficerController>(),
                    null,
                    ~(1 << playerLayer),
                    30f);
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
                if (saved == null)
                {
                    throw new InvalidOperationException(
                        $"Unity could not save the updated player prefab at {PlayerPrefabPath}.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static TacticalPlayerInput ConfigureScenePlayer(
            GameObject player,
            InputActionAsset inputAsset)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            TacticalPlayerInput tacticalInput = player.GetComponent<TacticalPlayerInput>();
            if (playerInput == null || tacticalInput == null)
            {
                throw new InvalidOperationException(
                    "Scene player is missing the established Input System components.");
            }

            playerInput.actions = inputAsset;
            tacticalInput.Configure(playerInput);
            return tacticalInput;
        }

        private static GameObject CreateOfficerPrefab(
            string path,
            string prefabName,
            string actorId,
            string displayName,
            int seed,
            Material uniformMaterial,
            Material armorMaterial,
            Material accentMaterial,
            int interactableLayer)
        {
            GameObject root = new GameObject(prefabName);
            root.layer = interactableLayer;
            ActorIdentity identity = root.AddComponent<ActorIdentity>();
            identity.Configure(actorId, displayName, ActorRole.Officer, seed);
            ActorInventory inventory = root.AddComponent<ActorInventory>();
            inventory.Configure(false, new[] { "Handcuffs", "Radio", "Medical gloves" });
            ActorCondition condition = root.AddComponent<ActorCondition>();
            condition.ResetCondition();
            OfficerOrderLedger ledger = root.AddComponent<OfficerOrderLedger>();
            ledger.Configure(identity);

            NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.34f;
            agent.height = 1.82f;
            agent.baseOffset = 0f;
            agent.speed = 2.8f;
            agent.acceleration = 8f;
            agent.angularSpeed = 480f;
            agent.stoppingDistance = 0.72f;
            agent.autoBraking = true;
            agent.autoTraverseOffMeshLink = true;

            GameObject bodyRoot = new GameObject("BodyPresentation");
            bodyRoot.layer = interactableLayer;
            bodyRoot.transform.SetParent(root.transform, false);
            CreatePrimitiveVisual(
                PrimitiveType.Capsule,
                "UniformBody",
                bodyRoot.transform,
                new Vector3(0f, 0.93f, 0f),
                new Vector3(0.56f, 0.8f, 0.56f),
                uniformMaterial,
                interactableLayer);
            CreatePrimitiveVisual(
                PrimitiveType.Cube,
                "ArmorCarrier",
                bodyRoot.transform,
                new Vector3(0f, 1.18f, 0.02f),
                new Vector3(0.58f, 0.58f, 0.28f),
                armorMaterial,
                interactableLayer);
            CreatePrimitiveVisual(
                PrimitiveType.Sphere,
                "Head",
                bodyRoot.transform,
                new Vector3(0f, 1.74f, 0f),
                Vector3.one * 0.42f,
                accentMaterial,
                interactableLayer);
            CreatePrimitiveVisual(
                PrimitiveType.Cube,
                "Helmet",
                bodyRoot.transform,
                new Vector3(0f, 1.91f, 0f),
                new Vector3(0.46f, 0.2f, 0.48f),
                armorMaterial,
                interactableLayer);
            CreatePrimitiveVisual(
                PrimitiveType.Cube,
                "LeftArm",
                bodyRoot.transform,
                new Vector3(-0.39f, 1.18f, 0f),
                new Vector3(0.14f, 0.66f, 0.14f),
                uniformMaterial,
                interactableLayer);
            CreatePrimitiveVisual(
                PrimitiveType.Cube,
                "RightArm",
                bodyRoot.transform,
                new Vector3(0.39f, 1.18f, 0f),
                new Vector3(0.14f, 0.66f, 0.14f),
                uniformMaterial,
                interactableLayer);

            GameObject torsoHitbox = new GameObject("TorsoHitRegion");
            torsoHitbox.layer = interactableLayer;
            torsoHitbox.transform.SetParent(root.transform, false);
            CapsuleCollider torsoCollider = torsoHitbox.AddComponent<CapsuleCollider>();
            torsoCollider.center = new Vector3(0f, 1.03f, 0f);
            torsoCollider.height = 1.35f;
            torsoCollider.radius = 0.32f;
            torsoHitbox.AddComponent<ActorHitRegion>().Configure(ActorHitRegionType.Torso);

            GameObject headHitbox = new GameObject("HeadHitRegion");
            headHitbox.layer = interactableLayer;
            headHitbox.transform.SetParent(root.transform, false);
            headHitbox.transform.localPosition = new Vector3(0f, 1.74f, 0f);
            SphereCollider headCollider = headHitbox.AddComponent<SphereCollider>();
            headCollider.radius = 0.24f;
            headHitbox.AddComponent<ActorHitRegion>().Configure(ActorHitRegionType.Head);

            GameObject statusObject = new GameObject("OfficerStatusLabel");
            statusObject.layer = interactableLayer;
            statusObject.transform.SetParent(bodyRoot.transform, false);
            statusObject.transform.localPosition = new Vector3(0f, 2.38f, 0f);
            statusObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh statusText = statusObject.AddComponent<TextMesh>();
            statusText.anchor = TextAnchor.MiddleCenter;
            statusText.alignment = TextAlignment.Center;
            statusText.characterSize = 0.045f;
            statusText.fontSize = 42;
            statusText.color = Color.white;

            OfficerVisual visual = root.AddComponent<OfficerVisual>();
            visual.Configure(statusText);
            TacticalOfficerController controller = root.AddComponent<TacticalOfficerController>();
            controller.Configure(identity, condition, ledger, agent, visual);
            OfficerInitiativeLedger initiativeLedger =
                root.AddComponent<OfficerInitiativeLedger>();
            initiativeLedger.Configure(identity);
            OfficerInitiativeController initiative =
                root.AddComponent<OfficerInitiativeController>();
            initiative.Configure(
                identity,
                condition,
                controller,
                initiativeLedger,
                20f,
                ~0,
                true);
            visual.SetPresentation(
                identity,
                false,
                OfficerOrderStatus.Completed,
                null,
                OfficerOrderOutcomeReason.None,
                "Standing by");
            SetLayerRecursively(root, interactableLayer);
            return SavePrefabAndDestroy(root, path);
        }

        private static TacticalRoomVolume CreateRoomClearanceVolume(Transform generatedRoot)
        {
            GameObject roomObject = new GameObject("M4_NorthTrainingRoomClearance");
            roomObject.transform.SetParent(generatedRoot, false);
            roomObject.transform.position = new Vector3(0f, 0f, 4f);
            BoxCollider bounds = roomObject.AddComponent<BoxCollider>();
            bounds.isTrigger = true;
            bounds.center = new Vector3(0f, 1.5f, 0f);
            bounds.size = new Vector3(19.4f, 3f, 7.4f);
            TacticalRoomVolume room = roomObject.AddComponent<TacticalRoomVolume>();
            room.Configure(
                "prototype_north_training_room",
                "North Training Room",
                bounds,
                2,
                2.5f);
            return room;
        }

        private static GameObject CreateOrderMarkerPrefab(
            Material markerMaterial,
            int interactableLayer)
        {
            GameObject root = new GameObject("ROE_OfficerOrderMarker");
            GameObject presentation = new GameObject("MarkerPresentation");
            presentation.transform.SetParent(root.transform, false);
            GameObject disc = CreatePrimitiveVisual(
                PrimitiveType.Cylinder,
                "CommandDisc",
                presentation.transform,
                Vector3.zero,
                new Vector3(0.75f, 0.015f, 0.75f),
                markerMaterial,
                interactableLayer);
            disc.transform.localRotation = Quaternion.identity;
            GameObject labelObject = new GameObject("CommandLabel");
            labelObject.transform.SetParent(presentation.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.08f;
            label.fontSize = 42;
            label.color = new Color(0.35f, 0.9f, 1f, 1f);
            OfficerOrderMarker marker = root.AddComponent<OfficerOrderMarker>();
            marker.Configure(presentation, label);
            return SavePrefabAndDestroy(root, MarkerPrefabPath);
        }

        private static DoorTraversalLink CreateDoorTraversalLink(
            Scene scene,
            Transform generatedRoot)
        {
            PrototypeDoor door = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PrototypeDoor>(true))
                .FirstOrDefault();
            if (door == null)
            {
                throw new InvalidOperationException(
                    "Prototype door is missing. Rebuild Milestone 1 before Milestone 4.");
            }

            GameObject linkObject = new GameObject("M4_TrainingDoorTraversalLink");
            linkObject.transform.SetParent(generatedRoot, false);
            // The prototype door root is its hinge. Offset to the center of the 1.1 m leaf
            // while keeping this link independent from the rotating door transform.
            linkObject.transform.SetPositionAndRotation(
                door.transform.TransformPoint(new Vector3(0.55f, 0f, 0f)),
                door.transform.rotation);
            NavMeshLink navigationLink = linkObject.AddComponent<NavMeshLink>();
            navigationLink.startPoint = new Vector3(0f, 0f, -1.25f);
            navigationLink.endPoint = new Vector3(0f, 0f, 1.25f);
            navigationLink.width = 0.75f;
            navigationLink.bidirectional = true;
            navigationLink.area = 0;
            navigationLink.costModifier = -1f;
            navigationLink.autoUpdate = false;
            navigationLink.UpdateLink();
            navigationLink.activated = false;
            DoorTraversalLink traversal = linkObject.AddComponent<DoorTraversalLink>();
            traversal.Configure(door, navigationLink);
            return traversal;
        }

        private static GameObject CreateDebugUiPrefab()
        {
            GameObject root = new GameObject(
                DebugUiRootName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup),
                typeof(OfficerCommandDebugUI));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 82;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                throw new InvalidOperationException("Unity's LegacyRuntime font could not be loaded.");
            }

            GameObject panel = CreateUiImage(
                "OfficerCommandPanel",
                root.transform,
                new Color(0.012f, 0.02f, 0.03f, 0.91f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-32f, -32f);
            panelRect.sizeDelta = new Vector2(700f, 250f);

            Text commandText = CreateUiText(
                "CommandSummary",
                panel.transform,
                font,
                17,
                FontStyle.Bold,
                TextAnchor.UpperLeft);
            commandText.rectTransform.anchorMin = Vector2.zero;
            commandText.rectTransform.anchorMax = Vector2.one;
            commandText.rectTransform.offsetMin = new Vector2(18f, 140f);
            commandText.rectTransform.offsetMax = new Vector2(-18f, -12f);
            commandText.color = new Color(0.36f, 0.82f, 1f, 1f);

            Text stateText = CreateUiText(
                "OfficerStates",
                panel.transform,
                font,
                16,
                FontStyle.Normal,
                TextAnchor.UpperLeft);
            stateText.rectTransform.anchorMin = Vector2.zero;
            stateText.rectTransform.anchorMax = Vector2.one;
            stateText.rectTransform.offsetMin = new Vector2(18f, 12f);
            stateText.rectTransform.offsetMax = new Vector2(-18f, -112f);
            stateText.color = Color.white;

            root.GetComponent<OfficerCommandDebugUI>().ConfigureVisuals(
                commandText,
                stateText);
            return SavePrefabAndDestroy(root, DebugUiPrefabPath);
        }

        private static GameObject CreatePrimitiveVisual(
            PrimitiveType primitiveType,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            int layer)
        {
            GameObject result = GameObject.CreatePrimitive(primitiveType);
            result.name = name;
            result.layer = layer;
            result.transform.SetParent(parent, false);
            result.transform.localPosition = localPosition;
            result.transform.localRotation = Quaternion.identity;
            result.transform.localScale = localScale;
            result.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = result.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            return result;
        }

        private static Material CreateOrUpdateMaterial(
            string path,
            Color color,
            float smoothness,
            float emissionIntensity)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("HDRP/Lit");
                if (shader == null)
                {
                    throw new InvalidOperationException("HDRP/Lit shader could not be found.");
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

            if (emissionIntensity > 0f && material.HasProperty("_EmissiveColor"))
            {
                material.SetColor("_EmissiveColor", color * emissionIntensity);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreateUiImage(string name, Transform parent, Color color)
        {
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Image));
            result.transform.SetParent(parent, false);
            result.GetComponent<Image>().color = color;
            return result;
        }

        private static Text CreateUiText(
            string name,
            Transform parent,
            Font font,
            int size,
            FontStyle style,
            TextAnchor alignment)
        {
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Text));
            result.transform.SetParent(parent, false);
            Text text = result.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject SavePrefabAndDestroy(GameObject root, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Unity could not save prefab: {path}");
            }

            return prefab;
        }

        private static GameObject InstantiatePrefab(
            GameObject prefab,
            Scene scene,
            Transform parent)
        {
            UnityEngine.Object instanceObject = parent == null
                ? PrefabUtility.InstantiatePrefab(prefab)
                : PrefabUtility.InstantiatePrefab(prefab, parent);
            GameObject instance = instanceObject as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Unity could not instantiate {prefab.name}.");
            }

            if (parent == null && instance.scene != scene)
            {
                SceneManager.MoveGameObjectToScene(instance, scene);
            }

            return instance;
        }

        private static void RemovePreviousGeneratedContent(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, GeneratedRootName, StringComparison.Ordinal)
                    || string.Equals(root.name, DebugUiRootName, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static void PersistPrefabInstanceOverrides(Component component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            EditorUtility.SetDirty(component);
            if (PrefabUtility.IsPartOfPrefabInstance(component))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            }
        }

        private static GameObject ReloadAndVerifySceneReferences()
        {
            Scene reloadedScene = EditorSceneManager.OpenScene(
                ProjectInfo.PrototypeScenePath,
                OpenSceneMode.Single);
            GameObject reloadedPlayer = reloadedScene.GetRootGameObjects().FirstOrDefault(root =>
                string.Equals(root.name, PlayerRootName, StringComparison.Ordinal));
            OfficerSquadController reloadedSquad = reloadedPlayer != null
                ? reloadedPlayer.GetComponent<OfficerSquadController>()
                : null;
            if (reloadedSquad == null || !reloadedSquad.HasCompleteConfiguration)
            {
                throw new InvalidOperationException(
                    reloadedSquad == null
                        ? "Saved scene verification failed: player squad component is missing."
                        : "Saved scene verification failed; missing: "
                            + reloadedSquad.ConfigurationProblems
                            + ".");
            }

            GameObject generatedRoot = reloadedScene.GetRootGameObjects().FirstOrDefault(root =>
                string.Equals(root.name, GeneratedRootName, StringComparison.Ordinal));
            Transform alpha = generatedRoot != null
                ? generatedRoot.transform.Find("M4_OfficerAlpha")
                : null;
            return alpha != null ? alpha.gameObject : reloadedPlayer;
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
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(current, parts[index]);
                    if (string.IsNullOrEmpty(guid))
                    {
                        throw new InvalidOperationException($"Could not create folder {next}.");
                    }
                }

                current = next;
            }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
