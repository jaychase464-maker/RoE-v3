using System;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Input;
using RulesOfEntry.Interaction;
using RulesOfEntry.Player;
using RulesOfEntry.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone1
{
    public static class RulesOfEntryMilestoneOneSetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 1/Build Gameplay Prototype";

        internal const string InputAssetPath =
            "Assets/_Project/RulesOfEntry/Input/ROE_InputActions.inputactions";
        internal const string PlayerPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_Player.prefab";
        internal const string DoorPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Interactions/ROE_PrototypeDoor.prefab";
        internal const string PanelPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Interactions/ROE_PrototypeControlPanel.prefab";
        internal const string PromptPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_InteractionPromptUI.prefab";

        private const string MaterialsFolder =
            "Assets/_Project/RulesOfEntry/Art/Materials";
        private const string PlayerLayerName = "Player";
        private const string InteractableLayerName = "Interactable";
        private const string GrayboxRootName = "[Milestone1_Graybox]";
        private const string PlayerRootName = "ROE_Player";
        private const string UiRootName = "ROE_InteractionPromptUI";

        [MenuItem(MenuPath, false, 20)]
        public static void BuildGameplayPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building the Milestone 1 prototype.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("Milestone 1", "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                EnsureFolder(MaterialsFolder);
                int playerLayer = EnsureLayer(PlayerLayerName);
                int interactableLayer = EnsureLayer(InteractableLayerName);

                InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                    InputAssetPath);
                if (inputAsset == null)
                {
                    throw new InvalidOperationException(
                        $"Milestone 1 input asset was not found at {InputAssetPath}.");
                }

                Material floorMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M1_Floor.mat",
                    new Color(0.105f, 0.12f, 0.14f, 1f),
                    0f,
                    0.18f);
                Material wallMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M1_Wall.mat",
                    new Color(0.24f, 0.27f, 0.31f, 1f),
                    0f,
                    0.28f);
                Material accentMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M1_Accent.mat",
                    new Color(0.025f, 0.36f, 0.72f, 1f),
                    0.15f,
                    0.45f);
                Material doorMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M1_Door.mat",
                    new Color(0.16f, 0.19f, 0.22f, 1f),
                    0.45f,
                    0.5f);

                GameObject playerPrefab = CreatePlayerPrefab(
                    inputAsset,
                    playerLayer,
                    interactableLayer);
                GameObject doorPrefab = CreateDoorPrefab(doorMaterial, interactableLayer);
                GameObject panelPrefab = CreatePanelPrefab(
                    wallMaterial,
                    accentMaterial,
                    interactableLayer);
                GameObject promptPrefab = CreatePromptPrefab();

                Scene scene = EditorSceneManager.OpenScene(
                    ProjectInfo.PrototypeScenePath,
                    OpenSceneMode.Single);
                SceneManager.SetActiveScene(scene);

                RemovePreviousGeneratedContent(scene);
                DisableTemplateCamera(scene);

                GameObject grayboxRoot = BuildGraybox(
                    floorMaterial,
                    wallMaterial,
                    accentMaterial,
                    doorPrefab,
                    panelPrefab,
                    scene);

                GameObject player = InstantiatePrefab(playerPrefab, scene, null);
                player.name = PlayerRootName;
                player.transform.SetPositionAndRotation(
                    new Vector3(0f, 0.05f, -4.5f),
                    Quaternion.identity);

                GameObject promptUi = InstantiatePrefab(promptPrefab, scene, null);
                promptUi.name = UiRootName;
                InteractionPromptUI prompt = promptUi.GetComponent<InteractionPromptUI>();
                prompt.ConfigureSources(
                    player.GetComponent<PlayerInteractor>(),
                    player.GetComponent<TacticalPlayerInput>());

                grayboxRoot.transform.SetAsFirstSibling();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException(
                        $"Unity could not save {ProjectInfo.PrototypeScenePath}.");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeGameObject = player;

                ProjectLog.Info(
                    "Milestone 1",
                    "First-person and interaction prototype created. Running validation now.");
                RulesOfEntryMilestoneOneValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Milestone 1", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 1 setup stopped. Check the first Console error for the root cause.",
                    "OK");
            }
        }

        private static GameObject CreatePlayerPrefab(
            InputActionAsset inputAsset,
            int playerLayer,
            int interactableLayer)
        {
            GameObject root = new GameObject(PlayerRootName);
            root.layer = playerLayer;

            CharacterController characterController = root.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.32f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            characterController.slopeLimit = 50f;
            characterController.stepOffset = 0.3f;
            characterController.skinWidth = 0.03f;
            characterController.minMoveDistance = 0f;

            PlayerInput playerInput = root.AddComponent<PlayerInput>();
            playerInput.actions = inputAsset;
            playerInput.defaultActionMap = "Player";
            playerInput.neverAutoSwitchControlSchemes = false;
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            TacticalPlayerInput tacticalInput = root.AddComponent<TacticalPlayerInput>();
            tacticalInput.Configure(playerInput);

            GameObject cameraPivotObject = new GameObject("CameraPivot");
            cameraPivotObject.layer = playerLayer;
            cameraPivotObject.transform.SetParent(root.transform, false);
            cameraPivotObject.transform.localPosition = new Vector3(0f, 1.68f, 0f);

            GameObject cameraObject = new GameObject("PlayerCamera");
            cameraObject.layer = playerLayer;
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(cameraPivotObject.transform, false);
            Camera playerCamera = cameraObject.AddComponent<Camera>();
            playerCamera.fieldOfView = 75f;
            playerCamera.nearClipPlane = 0.03f;
            playerCamera.farClipPlane = 750f;
            cameraObject.AddComponent<AudioListener>();
            playerInput.camera = playerCamera;

            LayerMask obstructionMask = ~(1 << playerLayer);
            FirstPersonMotor motor = root.AddComponent<FirstPersonMotor>();
            motor.Configure(
                characterController,
                tacticalInput,
                cameraPivotObject.transform,
                obstructionMask);

            FirstPersonLook look = root.AddComponent<FirstPersonLook>();
            look.Configure(tacticalInput, root.transform, cameraPivotObject.transform);

            CursorStateController cursor = root.AddComponent<CursorStateController>();
            cursor.Configure(tacticalInput);

            PlayerInteractor interactor = root.AddComponent<PlayerInteractor>();
            interactor.Configure(
                tacticalInput,
                playerCamera,
                1 << interactableLayer);

            SetLayerRecursively(root, playerLayer);
            return SavePrefabAndDestroy(root, PlayerPrefabPath);
        }

        private static GameObject CreateDoorPrefab(Material doorMaterial, int interactableLayer)
        {
            GameObject root = new GameObject("ROE_PrototypeDoor");
            root.layer = interactableLayer;
            PrototypeDoor door = root.AddComponent<PrototypeDoor>();

            GameObject leaf = CreateCube(
                "DoorLeaf",
                root.transform,
                new Vector3(0.55f, 1.1f, 0f),
                new Vector3(1.1f, 2.2f, 0.12f),
                doorMaterial,
                interactableLayer);

            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handle.name = "Handle";
            handle.layer = interactableLayer;
            handle.transform.SetParent(leaf.transform, false);
            handle.transform.localPosition = new Vector3(0.38f, 0f, -0.65f);
            handle.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            ApplyMaterial(handle, doorMaterial);

            door.Configure(root.transform, 100f);
            SetLayerRecursively(root, interactableLayer);
            return SavePrefabAndDestroy(root, DoorPrefabPath);
        }

        private static GameObject CreatePanelPrefab(
            Material bodyMaterial,
            Material indicatorMaterial,
            int interactableLayer)
        {
            GameObject root = new GameObject("ROE_PrototypeControlPanel");
            root.layer = interactableLayer;
            PrototypeControlPanel panel = root.AddComponent<PrototypeControlPanel>();

            CreateCube(
                "PanelBody",
                root.transform,
                Vector3.zero,
                new Vector3(0.72f, 0.9f, 0.12f),
                bodyMaterial,
                interactableLayer);
            GameObject indicator = CreateCube(
                "Indicator",
                root.transform,
                new Vector3(0f, 0.18f, -0.075f),
                new Vector3(0.34f, 0.18f, 0.035f),
                indicatorMaterial,
                interactableLayer);

            panel.Configure(indicator.GetComponent<Renderer>());
            SetLayerRecursively(root, interactableLayer);
            return SavePrefabAndDestroy(root, PanelPrefabPath);
        }

        private static GameObject CreatePromptPrefab()
        {
            GameObject root = new GameObject(
                UiRootName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup),
                typeof(InteractionPromptUI));

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                throw new InvalidOperationException("Unity's built-in LegacyRuntime font could not be loaded.");
            }

            GameObject container = CreateUiImage(
                "PromptContainer",
                root.transform,
                new Color(0.018f, 0.025f, 0.035f, 0.92f));
            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0f);
            containerRect.anchorMax = new Vector2(0.5f, 0f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.anchoredPosition = new Vector2(0f, 95f);
            containerRect.sizeDelta = new Vector2(540f, 78f);

            GameObject accent = CreateUiImage(
                "Accent",
                container.transform,
                new Color(0.05f, 0.52f, 1f, 1f));
            SetRect(accent.GetComponent<RectTransform>(),
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(5f, 0f));

            GameObject keyBadge = CreateUiImage(
                "KeyBadge",
                container.transform,
                new Color(0.08f, 0.12f, 0.17f, 1f));
            RectTransform badgeRect = keyBadge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0f, 0.5f);
            badgeRect.anchorMax = new Vector2(0f, 0.5f);
            badgeRect.pivot = new Vector2(0f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(18f, 2f);
            badgeRect.sizeDelta = new Vector2(54f, 48f);

            Text keyText = CreateUiText(
                "KeyText",
                keyBadge.transform,
                font,
                22,
                FontStyle.Bold,
                TextAnchor.MiddleCenter);
            SetRect(keyText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            keyText.text = "E";
            keyText.color = new Color(0.4f, 0.78f, 1f, 1f);

            Text promptText = CreateUiText(
                "PromptText",
                container.transform,
                font,
                22,
                FontStyle.Normal,
                TextAnchor.MiddleLeft);
            promptText.rectTransform.anchorMin = new Vector2(0f, 0f);
            promptText.rectTransform.anchorMax = new Vector2(1f, 1f);
            promptText.rectTransform.offsetMin = new Vector2(92f, 10f);
            promptText.rectTransform.offsetMax = new Vector2(-22f, -10f);
            promptText.text = "Interact";
            promptText.color = Color.white;

            GameObject progressBackground = CreateUiImage(
                "HoldProgressBackground",
                container.transform,
                new Color(0.15f, 0.18f, 0.22f, 1f));
            SetRect(
                progressBackground.GetComponent<RectTransform>(),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                Vector2.zero,
                new Vector2(0f, 4f));

            GameObject progressObject = CreateUiImage(
                "HoldProgressFill",
                progressBackground.transform,
                new Color(0.05f, 0.62f, 1f, 1f));
            SetRect(
                progressObject.GetComponent<RectTransform>(),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            Image progressFill = progressObject.GetComponent<Image>();
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillAmount = 0f;

            InteractionPromptUI promptUi = root.GetComponent<InteractionPromptUI>();
            promptUi.ConfigureVisuals(canvasGroup, keyText, promptText, progressFill);
            return SavePrefabAndDestroy(root, PromptPrefabPath);
        }

        private static GameObject BuildGraybox(
            Material floorMaterial,
            Material wallMaterial,
            Material accentMaterial,
            GameObject doorPrefab,
            GameObject panelPrefab,
            Scene scene)
        {
            GameObject root = new GameObject(GrayboxRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            CreateCube("Floor", root.transform, new Vector3(0f, -0.1f, 0f),
                new Vector3(20f, 0.2f, 16f), floorMaterial, 0);
            CreateCube("NorthWall", root.transform, new Vector3(0f, 1.5f, 8f),
                new Vector3(20f, 3f, 0.3f), wallMaterial, 0);
            CreateCube("SouthWall", root.transform, new Vector3(0f, 1.5f, -8f),
                new Vector3(20f, 3f, 0.3f), wallMaterial, 0);
            CreateCube("WestWall", root.transform, new Vector3(-10f, 1.5f, 0f),
                new Vector3(0.3f, 3f, 16f), wallMaterial, 0);
            CreateCube("EastWall", root.transform, new Vector3(10f, 1.5f, 0f),
                new Vector3(0.3f, 3f, 16f), wallMaterial, 0);

            CreateCube("DividerLeft", root.transform, new Vector3(-5.3f, 1.5f, 0f),
                new Vector3(9.4f, 3f, 0.3f), wallMaterial, 0);
            CreateCube("DividerRight", root.transform, new Vector3(5.3f, 1.5f, 0f),
                new Vector3(9.4f, 3f, 0.3f), wallMaterial, 0);
            CreateCube("DoorHeader", root.transform, new Vector3(0f, 2.65f, 0f),
                new Vector3(1.2f, 0.7f, 0.3f), accentMaterial, 0);
            CreateCube("DoorFrameLeft", root.transform, new Vector3(-0.68f, 1.15f, 0f),
                new Vector3(0.16f, 2.3f, 0.36f), accentMaterial, 0);
            CreateCube("DoorFrameRight", root.transform, new Vector3(0.68f, 1.15f, 0f),
                new Vector3(0.16f, 2.3f, 0.36f), accentMaterial, 0);

            CreateCube("CoverBlockA", root.transform, new Vector3(-4f, 0.65f, 4f),
                new Vector3(2.4f, 1.3f, 0.7f), accentMaterial, 0);
            CreateCube("CoverBlockB", root.transform, new Vector3(4.5f, 0.9f, 5.1f),
                new Vector3(0.8f, 1.8f, 2.6f), wallMaterial, 0);
            CreateCube("CrouchTunnelTop", root.transform, new Vector3(-6.8f, 1.55f, -4f),
                new Vector3(3.6f, 0.25f, 2.5f), accentMaterial, 0);
            CreateCube("CrouchTunnelLeft", root.transform, new Vector3(-8.5f, 0.75f, -4f),
                new Vector3(0.2f, 1.5f, 2.5f), wallMaterial, 0);
            CreateCube("CrouchTunnelRight", root.transform, new Vector3(-5.1f, 0.75f, -4f),
                new Vector3(0.2f, 1.5f, 2.5f), wallMaterial, 0);

            GameObject door = InstantiatePrefab(doorPrefab, scene, root.transform);
            door.name = "TrainingDoor";
            door.transform.localPosition = new Vector3(-0.55f, 0f, 0f);
            door.transform.localRotation = Quaternion.identity;

            GameObject panel = InstantiatePrefab(panelPrefab, scene, root.transform);
            panel.name = "TrainingControlPanel";
            panel.transform.localPosition = new Vector3(4f, 1.25f, -7.78f);
            panel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            return root;
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
                throw new InvalidOperationException($"Unity could not instantiate prefab {prefab.name}.");
            }

            if (parent == null && instance.scene != scene)
            {
                SceneManager.MoveGameObjectToScene(instance, scene);
            }

            return instance;
        }

        private static GameObject CreateCube(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            int layer)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.layer = layer;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;
            ApplyMaterial(cube, material);
            return cube;
        }

        private static void ApplyMaterial(GameObject target, Material material)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
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
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void RemovePreviousGeneratedContent(Scene scene)
        {
            string[] generatedNames = { GrayboxRootName, PlayerRootName, UiRootName };
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (generatedNames.Contains(root.name, StringComparer.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static void DisableTemplateCamera(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Camera camera = root.GetComponentInChildren<Camera>(true);
                if (camera != null && string.Equals(root.name, "Main Camera", StringComparison.Ordinal))
                {
                    root.SetActive(false);
                }
            }
        }

        private static int EnsureLayer(string layerName)
        {
            int existingLayer = LayerMask.NameToLayer(layerName);
            if (existingLayer >= 0)
            {
                return existingLayer;
            }

            UnityEngine.Object tagManager = AssetDatabase
                .LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")
                .FirstOrDefault();
            if (tagManager == null)
            {
                throw new InvalidOperationException("TagManager.asset could not be loaded.");
            }

            SerializedObject serializedTagManager = new SerializedObject(tagManager);
            SerializedProperty layers = serializedTagManager.FindProperty("layers");
            for (int index = 8; index < layers.arraySize; index++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(index);
                if (!string.IsNullOrEmpty(layer.stringValue))
                {
                    continue;
                }

                layer.stringValue = layerName;
                serializedTagManager.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                return index;
            }

            throw new InvalidOperationException(
                $"No free User Layer slot is available for {layerName}.");
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
