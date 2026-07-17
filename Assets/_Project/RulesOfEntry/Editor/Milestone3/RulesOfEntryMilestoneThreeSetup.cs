using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Combat;
using RulesOfEntry.Core;
using RulesOfEntry.Input;
using RulesOfEntry.UI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone3
{
    public static class RulesOfEntryMilestoneThreeSetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 3/Build Human Behavior Prototype";

        internal const string InputAssetPath =
            "Assets/_Project/RulesOfEntry/Input/ROE_InputActions.inputactions";
        internal const string PlayerPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_Player.prefab";
        internal const string SuspectProfilePath =
            "Assets/_Project/RulesOfEntry/Data/AI/M3_UncertainSuspect.asset";
        internal const string CivilianProfilePath =
            "Assets/_Project/RulesOfEntry/Data/AI/M3_PanickedCivilian.asset";
        internal const string SuspectPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_PrototypeSuspect.prefab";
        internal const string CivilianPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_PrototypeCivilian.prefab";
        internal const string DebugUiPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_HumanBehaviorDebugUI.prefab";
        internal const string NavMeshDataPath =
            "Assets/_Project/RulesOfEntry/Data/AI/M3_PrototypeNavMesh.asset";

        private const string AiDataFolder = "Assets/_Project/RulesOfEntry/Data/AI";
        private const string ActorPrefabFolder =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors";
        private const string UiPrefabFolder = "Assets/_Project/RulesOfEntry/Prefabs/UI";
        private const string MaterialsFolder = "Assets/_Project/RulesOfEntry/Art/Materials";
        private const string GeneratedRootName = "[Milestone3_HumanBehavior]";
        private const string DebugUiRootName = "ROE_HumanBehaviorDebugUI";
        private const string PlayerRootName = "ROE_Player";
        private const string PlayerLayerName = "Player";
        private const string InteractableLayerName = "Interactable";

        [MenuItem(MenuPath, false, 40)]
        public static void BuildHumanBehaviorPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building the Milestone 3 prototype.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("Milestone 3", "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                EnsureFolder(AiDataFolder);
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

                HumanBehaviorProfile suspectProfile = CreateOrUpdateProfile(
                    SuspectProfilePath,
                    "M3 Uncertain Suspect",
                    0.36f,
                    0.48f,
                    0.22f,
                    0.52f,
                    0.24f,
                    0.9f,
                    0.55f,
                    1.55f,
                    3.35f);
                HumanBehaviorProfile civilianProfile = CreateOrUpdateProfile(
                    CivilianProfilePath,
                    "M3 Panicked Civilian",
                    0.74f,
                    0.03f,
                    0f,
                    0.46f,
                    0.32f,
                    0.96f,
                    0.4f,
                    1.25f,
                    3.05f);

                Material suspectMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M3_Suspect.mat",
                    new Color(0.62f, 0.24f, 0.1f, 1f));
                Material civilianMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M3_Civilian.mat",
                    new Color(0.74f, 0.62f, 0.22f, 1f));
                Material skinMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M3_ActorAccent.mat",
                    new Color(0.54f, 0.42f, 0.32f, 1f));

                UpdatePlayerPrefab(inputAsset, playerLayer);
                GameObject suspectPrefab = CreateActorPrefab(
                    SuspectPrefabPath,
                    "ROE_PrototypeSuspect",
                    "m3_suspect_01",
                    "Uncertain Suspect",
                    ActorRole.Suspect,
                    31010,
                    true,
                    new[] { "Folding knife", "Wallet" },
                    0.38f,
                    0.58f,
                    suspectProfile,
                    suspectMaterial,
                    skinMaterial,
                    interactableLayer);
                GameObject civilianPrefab = CreateActorPrefab(
                    CivilianPrefabPath,
                    "ROE_PrototypeCivilian",
                    "m3_civilian_01",
                    "Panicked Civilian",
                    ActorRole.Civilian,
                    31079,
                    false,
                    new[] { "Identification", "Mobile phone" },
                    0.62f,
                    0.84f,
                    civilianProfile,
                    civilianMaterial,
                    skinMaterial,
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
                        "ROE_Player is missing. Complete Milestones 1 and 2 before Milestone 3.");
                }

                VerbalCommandEmitter emitter = ConfigureScenePlayer(player, playerLayer);
                GameObject generatedRoot = new GameObject(GeneratedRootName);
                SceneManager.MoveGameObjectToScene(generatedRoot, scene);
                NavMeshSurface surface = generatedRoot.AddComponent<NavMeshSurface>();
                surface.collectObjects = CollectObjects.All;
                surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                surface.layerMask = ~(1 << playerLayer);
                surface.ignoreNavMeshAgent = true;
                surface.ignoreNavMeshObstacle = true;

                GameObject suspect = InstantiatePrefab(
                    suspectPrefab,
                    scene,
                    generatedRoot.transform);
                suspect.name = "M3_UncertainSuspect";
                suspect.transform.SetPositionAndRotation(
                    new Vector3(-3.2f, 0f, 4.4f),
                    Quaternion.Euler(0f, 180f, 0f));
                HumanActorController suspectController =
                    suspect.GetComponent<HumanActorController>();
                suspectController.ConfigureOfficerTarget(player.transform);

                GameObject civilian = InstantiatePrefab(
                    civilianPrefab,
                    scene,
                    generatedRoot.transform);
                civilian.name = "M3_PanickedCivilian";
                civilian.transform.SetPositionAndRotation(
                    new Vector3(3.4f, 0f, 3.5f),
                    Quaternion.Euler(0f, 180f, 0f));
                HumanActorController civilianController =
                    civilian.GetComponent<HumanActorController>();
                civilianController.ConfigureOfficerTarget(player.transform);

                GameObject debugUi = InstantiatePrefab(debugUiPrefab, scene, null);
                debugUi.name = DebugUiRootName;
                debugUi.GetComponent<HumanBehaviorDebugUI>().ConfigureSources(
                    emitter,
                    player.GetComponent<TacticalPlayerInput>(),
                    new[] { suspectController, civilianController });

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException(
                        $"Unity could not save {ProjectInfo.PrototypeScenePath} before navigation build.");
                }

                BuildAndPersistNavMesh(surface);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException(
                        $"Unity could not save {ProjectInfo.PrototypeScenePath} after navigation build.");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeGameObject = suspect;
                ProjectLog.Info(
                    "Milestone 3",
                    "Deterministic suspect, civilian, command, surrender, and custody prototype created. Running validation now.");
                RulesOfEntryMilestoneThreeValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Milestone 3", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 3 setup stopped. Check the first Console error for the root cause.",
                    "OK");
            }
        }

        private static HumanBehaviorProfile CreateOrUpdateProfile(
            string path,
            string assetName,
            float compliance,
            float aggression,
            float deception,
            float flight,
            float hide,
            float comprehension,
            float minimumReaction,
            float maximumReaction,
            float movementSpeed)
        {
            HumanBehaviorProfile profile = AssetDatabase.LoadAssetAtPath<HumanBehaviorProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<HumanBehaviorProfile>();
                profile.name = assetName;
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.Configure(
                compliance,
                aggression,
                deception,
                flight,
                hide,
                comprehension,
                minimumReaction,
                maximumReaction,
                movementSpeed);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void UpdatePlayerPrefab(InputActionAsset inputAsset, int playerLayer)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefabAsset == null)
            {
                throw new InvalidOperationException(
                    $"Missing {PlayerPrefabPath}. Complete Milestone 2 first.");
            }

            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                PlayerInput playerInput = root.GetComponent<PlayerInput>();
                TacticalPlayerInput tacticalInput = root.GetComponent<TacticalPlayerInput>();
                FirearmController firearmController = root.GetComponent<FirearmController>();
                Camera playerCamera = root.GetComponentInChildren<Camera>(true);
                if (playerInput == null
                    || tacticalInput == null
                    || firearmController == null
                    || playerCamera == null)
                {
                    throw new InvalidOperationException(
                        "Milestone 2 player input, camera, or firearm configuration is missing.");
                }

                playerInput.actions = inputAsset;
                tacticalInput.Configure(playerInput);
                VerbalCommandEmitter emitter = root.GetComponent<VerbalCommandEmitter>();
                if (emitter == null)
                {
                    emitter = root.AddComponent<VerbalCommandEmitter>();
                }

                emitter.Configure(
                    tacticalInput,
                    playerCamera.transform,
                    firearmController,
                    18f,
                    ~(1 << playerLayer));
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

        private static VerbalCommandEmitter ConfigureScenePlayer(GameObject player, int playerLayer)
        {
            TacticalPlayerInput tacticalInput = player.GetComponent<TacticalPlayerInput>();
            FirearmController firearmController = player.GetComponent<FirearmController>();
            Camera playerCamera = player.GetComponentInChildren<Camera>(true);
            VerbalCommandEmitter emitter = player.GetComponent<VerbalCommandEmitter>();
            if (emitter == null)
            {
                emitter = player.AddComponent<VerbalCommandEmitter>();
            }

            emitter.Configure(
                tacticalInput,
                playerCamera != null ? playerCamera.transform : player.transform,
                firearmController,
                18f,
                ~(1 << playerLayer));
            return emitter;
        }

        private static GameObject CreateActorPrefab(
            string path,
            string prefabName,
            string actorId,
            string displayName,
            ActorRole role,
            int seed,
            bool hasWeapon,
            string[] items,
            float initialStress,
            float initialMorale,
            HumanBehaviorProfile profile,
            Material uniformMaterial,
            Material accentMaterial,
            int interactableLayer)
        {
            GameObject root = new GameObject(prefabName);
            root.layer = interactableLayer;

            ActorIdentity identity = root.AddComponent<ActorIdentity>();
            identity.Configure(actorId, displayName, role, seed);
            ActorInventory inventory = root.AddComponent<ActorInventory>();
            inventory.Configure(hasWeapon, items);
            ActorCondition condition = root.AddComponent<ActorCondition>();
            condition.ResetCondition();
            CustodyEventLedger custodyLedger = root.AddComponent<CustodyEventLedger>();
            CustodyComponent custody = root.AddComponent<CustodyComponent>();
            custody.Configure(identity, inventory, condition, custodyLedger);
            HumanPerception perception = root.AddComponent<HumanPerception>();
            HumanDecisionLedger decisionLedger = root.AddComponent<HumanDecisionLedger>();

            NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.32f;
            agent.height = 1.8f;
            agent.baseOffset = 0f;
            agent.speed = profile.MovementSpeed;
            agent.acceleration = 9f;
            agent.angularSpeed = 540f;
            agent.stoppingDistance = 0.65f;
            agent.autoBraking = true;

            GameObject bodyRoot = new GameObject("BodyPresentation");
            bodyRoot.layer = interactableLayer;
            bodyRoot.transform.SetParent(root.transform, false);
            List<Renderer> renderers = new List<Renderer>();
            GameObject body = CreatePrimitiveVisual(
                PrimitiveType.Capsule,
                "Body",
                bodyRoot.transform,
                new Vector3(0f, 0.92f, 0f),
                new Vector3(0.55f, 0.78f, 0.55f),
                uniformMaterial,
                interactableLayer);
            renderers.Add(body.GetComponent<Renderer>());
            GameObject head = CreatePrimitiveVisual(
                PrimitiveType.Sphere,
                "Head",
                bodyRoot.transform,
                new Vector3(0f, 1.72f, 0f),
                Vector3.one * 0.42f,
                accentMaterial,
                interactableLayer);
            renderers.Add(head.GetComponent<Renderer>());
            GameObject leftArm = CreatePrimitiveVisual(
                PrimitiveType.Cube,
                "LeftArm",
                bodyRoot.transform,
                new Vector3(-0.38f, 1.18f, 0f),
                new Vector3(0.13f, 0.64f, 0.13f),
                uniformMaterial,
                interactableLayer);
            GameObject rightArm = CreatePrimitiveVisual(
                PrimitiveType.Cube,
                "RightArm",
                bodyRoot.transform,
                new Vector3(0.38f, 1.18f, 0f),
                new Vector3(0.13f, 0.64f, 0.13f),
                uniformMaterial,
                interactableLayer);
            renderers.Add(leftArm.GetComponent<Renderer>());
            renderers.Add(rightArm.GetComponent<Renderer>());

            GameObject torsoHitbox = new GameObject("TorsoHitRegion");
            torsoHitbox.layer = interactableLayer;
            torsoHitbox.transform.SetParent(root.transform, false);
            CapsuleCollider torsoCollider = torsoHitbox.AddComponent<CapsuleCollider>();
            torsoCollider.center = new Vector3(0f, 1.02f, 0f);
            torsoCollider.height = 1.32f;
            torsoCollider.radius = 0.31f;
            torsoHitbox.AddComponent<ActorHitRegion>().Configure(ActorHitRegionType.Torso);

            GameObject headHitbox = new GameObject("HeadHitRegion");
            headHitbox.layer = interactableLayer;
            headHitbox.transform.SetParent(root.transform, false);
            headHitbox.transform.localPosition = new Vector3(0f, 1.72f, 0f);
            SphereCollider headCollider = headHitbox.AddComponent<SphereCollider>();
            headCollider.radius = 0.23f;
            headHitbox.AddComponent<ActorHitRegion>().Configure(ActorHitRegionType.Head);

            GameObject eyes = new GameObject("Eyes");
            eyes.layer = interactableLayer;
            eyes.transform.SetParent(root.transform, false);
            eyes.transform.localPosition = new Vector3(0f, 1.68f, 0.12f);
            perception.Configure(
                eyes.transform,
                24f,
                125f,
                18f,
                ~0);

            GameObject statusObject = new GameObject("AIStatusLabel");
            statusObject.layer = interactableLayer;
            statusObject.transform.SetParent(bodyRoot.transform, false);
            statusObject.transform.localPosition = new Vector3(0f, 2.25f, 0f);
            statusObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh statusText = statusObject.AddComponent<TextMesh>();
            statusText.anchor = TextAnchor.MiddleCenter;
            statusText.alignment = TextAlignment.Center;
            statusText.characterSize = 0.055f;
            statusText.fontSize = 42;
            statusText.color = Color.white;

            ActorVisual visual = root.AddComponent<ActorVisual>();
            visual.Configure(
                renderers.ToArray(),
                statusText,
                bodyRoot.transform,
                leftArm.transform,
                rightArm.transform);
            CustodyInteractable interactable = root.AddComponent<CustodyInteractable>();
            interactable.Configure(custody, condition);
            HumanActorController controller = root.AddComponent<HumanActorController>();
            controller.Configure(
                identity,
                condition,
                inventory,
                custody,
                perception,
                decisionLedger,
                profile,
                agent,
                visual,
                null,
                initialStress,
                initialMorale);
            visual.SetPresentation(
                identity,
                HumanBehaviorState.Idle,
                CustodyState.Free,
                ActorConditionLevel.Stable,
                HumanDecisionReason.None);
            SetLayerRecursively(root, interactableLayer);
            return SavePrefabAndDestroy(root, path);
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
                typeof(HumanBehaviorDebugUI));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
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
                "AIDiagnosticsPanel",
                root.transform,
                new Color(0.018f, 0.025f, 0.035f, 0.9f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(32f, -32f);
            panelRect.sizeDelta = new Vector2(720f, 150f);

            Text commandText = CreateUiText(
                "CommandStatus",
                panel.transform,
                font,
                18,
                FontStyle.Bold,
                TextAnchor.UpperLeft);
            commandText.rectTransform.anchorMin = Vector2.zero;
            commandText.rectTransform.anchorMax = Vector2.one;
            commandText.rectTransform.offsetMin = new Vector2(18f, 88f);
            commandText.rectTransform.offsetMax = new Vector2(-18f, -12f);
            commandText.color = new Color(0.42f, 0.78f, 1f, 1f);

            Text actorStateText = CreateUiText(
                "ActorStates",
                panel.transform,
                font,
                17,
                FontStyle.Normal,
                TextAnchor.UpperLeft);
            actorStateText.rectTransform.anchorMin = Vector2.zero;
            actorStateText.rectTransform.anchorMax = Vector2.one;
            actorStateText.rectTransform.offsetMin = new Vector2(18f, 12f);
            actorStateText.rectTransform.offsetMax = new Vector2(-18f, -62f);
            actorStateText.color = Color.white;

            root.GetComponent<HumanBehaviorDebugUI>().ConfigureVisuals(
                commandText,
                actorStateText);
            return SavePrefabAndDestroy(root, DebugUiPrefabPath);
        }

        private static void BuildAndPersistNavMesh(NavMeshSurface surface)
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
                    "AI Navigation did not produce NavMesh data for the prototype scene.");
            }

            if (!AssetDatabase.Contains(data))
            {
                data.name = "M3 Prototype NavMesh";
                AssetDatabase.CreateAsset(data, NavMeshDataPath);
            }

            EditorUtility.SetDirty(surface);
        }

        private static void RemovePreviousGeneratedContent(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (!string.Equals(root.name, GeneratedRootName, StringComparison.Ordinal)
                    && !string.Equals(root.name, DebugUiRootName, StringComparison.Ordinal))
                {
                    continue;
                }

                NavMeshSurface oldSurface = root.GetComponent<NavMeshSurface>();
                if (oldSurface != null)
                {
                    oldSurface.RemoveData();
                }

                UnityEngine.Object.DestroyImmediate(root);
            }
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

        private static Material CreateOrUpdateMaterial(string path, Color color)
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
                material.SetFloat("_Smoothness", 0.28f);
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
