using System;
using System.Linq;
using RulesOfEntry.Combat;
using RulesOfEntry.Core;
using RulesOfEntry.Input;
using RulesOfEntry.Player;
using RulesOfEntry.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone2
{
    public static class RulesOfEntryMilestoneTwoSetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 2/Build Weapon Prototype";

        internal const string InputAssetPath =
            "Assets/_Project/RulesOfEntry/Input/ROE_InputActions.inputactions";
        internal const string PlayerPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_Player.prefab";
        internal const string FirearmDefinitionPath =
            "Assets/_Project/RulesOfEntry/Data/Equipment/M2_PatrolCarbine.asset";
        internal const string AmmunitionDefinitionPath =
            "Assets/_Project/RulesOfEntry/Data/Equipment/M2_556_62gr.asset";
        internal const string TargetPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Combat/ROE_BallisticTarget.prefab";
        internal const string WeaponUiPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_WeaponStatusUI.prefab";

        private const string EquipmentFolder =
            "Assets/_Project/RulesOfEntry/Data/Equipment";
        private const string CombatPrefabFolder =
            "Assets/_Project/RulesOfEntry/Prefabs/Combat";
        private const string MaterialsFolder =
            "Assets/_Project/RulesOfEntry/Art/Materials";
        private const string WeaponRigName = "[Milestone2_WeaponRig]";
        private const string RangeRootName = "[Milestone2_Range]";
        private const string WeaponUiRootName = "ROE_WeaponStatusUI";
        private const string PlayerRootName = "ROE_Player";

        [MenuItem(MenuPath, false, 30)]
        public static void BuildWeaponPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building the Milestone 2 weapon prototype.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("Milestone 2", "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                EnsureFolder(EquipmentFolder);
                EnsureFolder(CombatPrefabFolder);
                EnsureFolder(MaterialsFolder);

                InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                    InputAssetPath);
                if (inputAsset == null)
                {
                    throw new InvalidOperationException(
                        $"Milestone 2 input actions were not found at {InputAssetPath}.");
                }

                FirearmDefinition firearmDefinition = CreateOrUpdateFirearmDefinition();
                AmmunitionDefinition ammunitionDefinition = CreateOrUpdateAmmunitionDefinition();
                Material weaponMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M2_Weapon.mat",
                    new Color(0.035f, 0.043f, 0.05f, 1f),
                    0.55f,
                    0.36f);
                Material accentMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M2_WeaponAccent.mat",
                    new Color(0.12f, 0.15f, 0.17f, 1f),
                    0.7f,
                    0.48f);
                Material targetMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M2_Target.mat",
                    new Color(0.68f, 0.72f, 0.76f, 1f),
                    0.25f,
                    0.22f);
                Material rangeMaterial = CreateOrUpdateMaterial(
                    MaterialsFolder + "/M2_Range.mat",
                    new Color(0.18f, 0.20f, 0.23f, 1f),
                    0f,
                    0.2f);

                UpdatePlayerPrefab(
                    inputAsset,
                    firearmDefinition,
                    ammunitionDefinition,
                    weaponMaterial,
                    accentMaterial);
                GameObject targetPrefab = CreateTargetPrefab(targetMaterial, rangeMaterial);
                GameObject weaponUiPrefab = CreateWeaponUiPrefab();

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
                        "ROE_Player is missing from the prototype scene. Rebuild Milestone 1 first.");
                }

                FirearmController firearmController = player.GetComponent<FirearmController>();
                if (firearmController == null)
                {
                    throw new InvalidOperationException(
                        "The updated player prefab did not supply FirearmController.");
                }

                BuildTargetRange(scene, targetPrefab, rangeMaterial);
                GameObject weaponUi = InstantiatePrefab(weaponUiPrefab, scene, null);
                weaponUi.name = WeaponUiRootName;
                weaponUi.GetComponent<WeaponStatusUI>().ConfigureSource(firearmController);

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
                    "Milestone 2",
                    "Manual weapon and force-event prototype created. Running validation now.");
                RulesOfEntryMilestoneTwoValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Milestone 2", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 2 setup stopped. Check the first Console error for the root cause.",
                    "OK");
            }
        }

        private static FirearmDefinition CreateOrUpdateFirearmDefinition()
        {
            FirearmDefinition definition = AssetDatabase.LoadAssetAtPath<FirearmDefinition>(
                FirearmDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<FirearmDefinition>();
                definition.name = "M2 Patrol Carbine";
                AssetDatabase.CreateAsset(definition, FirearmDefinitionPath);
            }

            definition.Configure(
                "roe_patrol_carbine",
                "Patrol Carbine",
                30,
                0.12f,
                2.35f,
                1.8f,
                1.25f,
                0.85f,
                0.28f,
                1.15f,
                0.3f);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static AmmunitionDefinition CreateOrUpdateAmmunitionDefinition()
        {
            AmmunitionDefinition definition = AssetDatabase.LoadAssetAtPath<AmmunitionDefinition>(
                AmmunitionDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<AmmunitionDefinition>();
                definition.name = "M2 5.56x45 mm 62 gr";
                AssetDatabase.CreateAsset(definition, AmmunitionDefinitionPath);
            }

            definition.Configure(
                "roe_556_62gr",
                "5.56x45 mm 62 gr",
                62f,
                850f,
                500f);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void UpdatePlayerPrefab(
            InputActionAsset inputAsset,
            FirearmDefinition firearmDefinition,
            AmmunitionDefinition ammunitionDefinition,
            Material weaponMaterial,
            Material accentMaterial)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefabAsset == null)
            {
                throw new InvalidOperationException(
                    $"Missing {PlayerPrefabPath}. Complete Milestone 1 before Milestone 2.");
            }

            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                PlayerInput playerInput = root.GetComponent<PlayerInput>();
                TacticalPlayerInput tacticalInput = root.GetComponent<TacticalPlayerInput>();
                FirstPersonLook firstPersonLook = root.GetComponent<FirstPersonLook>();
                Camera playerCamera = root.GetComponentInChildren<Camera>(true);
                if (playerInput == null
                    || tacticalInput == null
                    || firstPersonLook == null
                    || playerCamera == null)
                {
                    throw new InvalidOperationException(
                        "Milestone 1 player input, look, or camera component is missing.");
                }

                playerInput.actions = inputAsset;
                tacticalInput.Configure(playerInput);

                Transform oldRig = playerCamera.transform.Find(WeaponRigName);
                if (oldRig != null)
                {
                    UnityEngine.Object.DestroyImmediate(oldRig.gameObject);
                }

                int playerLayer = root.layer;
                GameObject rig = new GameObject(WeaponRigName);
                rig.layer = playerLayer;
                rig.transform.SetParent(playerCamera.transform, false);
                rig.transform.localPosition = new Vector3(0.29f, -0.34f, 0.52f);
                rig.transform.localRotation = Quaternion.Euler(18f, -7f, 8f);

                FirearmView firearmView = rig.AddComponent<FirearmView>();
                CreateViewCube("Receiver", rig.transform, new Vector3(0f, 0f, 0.32f),
                    new Vector3(0.11f, 0.12f, 0.48f), weaponMaterial, playerLayer);
                CreateViewCube("Handguard", rig.transform, new Vector3(0f, 0f, 0.67f),
                    new Vector3(0.095f, 0.095f, 0.32f), accentMaterial, playerLayer);
                CreateViewCube("Stock", rig.transform, new Vector3(0f, -0.015f, 0.02f),
                    new Vector3(0.12f, 0.13f, 0.24f), accentMaterial, playerLayer);
                CreateViewCube("Barrel", rig.transform, new Vector3(0f, 0.005f, 0.93f),
                    new Vector3(0.032f, 0.032f, 0.32f), weaponMaterial, playerLayer);
                CreateViewCube("RearSight", rig.transform, new Vector3(0f, 0.105f, 0.28f),
                    new Vector3(0.055f, 0.07f, 0.04f), accentMaterial, playerLayer);
                CreateViewCube("FrontSight", rig.transform, new Vector3(0f, 0.105f, 0.82f),
                    new Vector3(0.035f, 0.07f, 0.035f), accentMaterial, playerLayer);
                CreateViewCube("PistolGrip", rig.transform, new Vector3(0f, -0.12f, 0.28f),
                    new Vector3(0.08f, 0.23f, 0.1f), accentMaterial, playerLayer,
                    Quaternion.Euler(-12f, 0f, 0f));
                GameObject magazine = CreateViewCube(
                    "InsertedMagazine",
                    rig.transform,
                    new Vector3(0f, -0.17f, 0.42f),
                    new Vector3(0.08f, 0.25f, 0.11f),
                    accentMaterial,
                    playerLayer,
                    Quaternion.Euler(-8f, 0f, 0f));

                GameObject muzzleObject = new GameObject("Muzzle");
                muzzleObject.layer = playerLayer;
                muzzleObject.transform.SetParent(rig.transform, false);
                muzzleObject.transform.localPosition = new Vector3(0f, 0.005f, 1.11f);
                muzzleObject.transform.localRotation = Quaternion.identity;
                firearmView.Configure(rig.transform, muzzleObject.transform, magazine);

                UseOfForceEventLedger ledger = root.GetComponent<UseOfForceEventLedger>();
                if (ledger == null)
                {
                    ledger = root.AddComponent<UseOfForceEventLedger>();
                }

                FirearmController controller = root.GetComponent<FirearmController>();
                if (controller == null)
                {
                    controller = root.AddComponent<FirearmController>();
                }

                LayerMask hitMask = ~(1 << playerLayer);
                controller.Configure(
                    tacticalInput,
                    firstPersonLook,
                    playerCamera,
                    muzzleObject.transform,
                    firearmView,
                    ledger,
                    firearmDefinition,
                    ammunitionDefinition,
                    hitMask);
                controller.ConfigureInitialLoadout(
                    29,
                    true,
                    new[] { 30, 30, 30 },
                    FireSelectorPosition.Safe,
                    WeaponReadyPosition.LowReady);

                SetLayerRecursively(rig, playerLayer);
                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
                if (savedPrefab == null)
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

        private static GameObject CreateTargetPrefab(Material targetMaterial, Material frameMaterial)
        {
            GameObject root = new GameObject("ROE_BallisticTarget");
            PrototypeBallisticTarget target = root.AddComponent<PrototypeBallisticTarget>();

            GameObject plate = CreateWorldCube(
                "TargetPlate",
                root.transform,
                new Vector3(0f, 1.25f, 0f),
                new Vector3(0.72f, 1.35f, 0.12f),
                targetMaterial);
            CreateWorldCube(
                "Stand",
                root.transform,
                new Vector3(0f, 0.45f, 0.08f),
                new Vector3(0.09f, 0.9f, 0.09f),
                frameMaterial);
            CreateWorldCube(
                "Base",
                root.transform,
                new Vector3(0f, 0.04f, 0.08f),
                new Vector3(0.8f, 0.08f, 0.45f),
                frameMaterial);
            target.Configure(plate.GetComponent<Renderer>());
            return SavePrefabAndDestroy(root, TargetPrefabPath);
        }

        private static GameObject CreateWeaponUiPrefab()
        {
            GameObject root = new GameObject(
                WeaponUiRootName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup),
                typeof(WeaponStatusUI));

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                throw new InvalidOperationException("Unity's LegacyRuntime font could not be loaded.");
            }

            GameObject panel = CreateUiImage(
                "WeaponStatusPanel",
                root.transform,
                new Color(0.018f, 0.025f, 0.035f, 0.88f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.anchoredPosition = new Vector2(-42f, 42f);
            panelRect.sizeDelta = new Vector2(450f, 94f);

            Text mechanicalText = CreateUiText(
                "MechanicalState",
                panel.transform,
                font,
                18,
                FontStyle.Bold,
                TextAnchor.UpperRight);
            mechanicalText.rectTransform.anchorMin = Vector2.zero;
            mechanicalText.rectTransform.anchorMax = Vector2.one;
            mechanicalText.rectTransform.offsetMin = new Vector2(18f, 44f);
            mechanicalText.rectTransform.offsetMax = new Vector2(-18f, -10f);
            mechanicalText.color = new Color(0.42f, 0.78f, 1f, 1f);

            Text statusText = CreateUiText(
                "StatusMessage",
                panel.transform,
                font,
                19,
                FontStyle.Normal,
                TextAnchor.LowerRight);
            statusText.rectTransform.anchorMin = Vector2.zero;
            statusText.rectTransform.anchorMax = Vector2.one;
            statusText.rectTransform.offsetMin = new Vector2(18f, 13f);
            statusText.rectTransform.offsetMax = new Vector2(-18f, -42f);
            statusText.color = Color.white;
            statusText.text = "SAFE • LOW READY";

            GameObject progressBackground = CreateUiImage(
                "OperationProgressBackground",
                panel.transform,
                new Color(0.12f, 0.15f, 0.18f, 1f));
            RectTransform backgroundRect = progressBackground.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0f);
            backgroundRect.anchorMax = new Vector2(1f, 0f);
            backgroundRect.offsetMin = new Vector2(0f, 0f);
            backgroundRect.offsetMax = new Vector2(0f, 4f);

            GameObject progressObject = CreateUiImage(
                "OperationProgressFill",
                progressBackground.transform,
                new Color(0.05f, 0.62f, 1f, 1f));
            SetStretch(progressObject.GetComponent<RectTransform>());
            Image progressFill = progressObject.GetComponent<Image>();
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillAmount = 0f;

            WeaponStatusUI weaponStatusUi = root.GetComponent<WeaponStatusUI>();
            weaponStatusUi.ConfigureVisuals(
                canvasGroup,
                mechanicalText,
                statusText,
                progressFill);
            return SavePrefabAndDestroy(root, WeaponUiPrefabPath);
        }

        private static void BuildTargetRange(
            Scene scene,
            GameObject targetPrefab,
            Material rangeMaterial)
        {
            GameObject root = new GameObject(RangeRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            CreateWorldCube(
                "FiringLine",
                root.transform,
                new Vector3(0f, 0.015f, 2f),
                new Vector3(8f, 0.03f, 0.12f),
                rangeMaterial);
            CreateWorldCube(
                "Backstop",
                root.transform,
                new Vector3(0f, 1.5f, 7.65f),
                new Vector3(8.5f, 3f, 0.22f),
                rangeMaterial);

            float[] targetXPositions = { -2.7f, 0f, 2.7f };
            for (int index = 0; index < targetXPositions.Length; index++)
            {
                GameObject target = InstantiatePrefab(targetPrefab, scene, root.transform);
                target.name = $"BallisticTarget_{index + 1}";
                target.transform.localPosition = new Vector3(targetXPositions[index], 0f, 7.46f);
                target.transform.localRotation = Quaternion.identity;
            }
        }

        private static GameObject CreateViewCube(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            int layer,
            Quaternion? localRotation = null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.layer = layer;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = localRotation ?? Quaternion.identity;
            cube.transform.localScale = localScale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            return cube;
        }

        private static GameObject CreateWorldCube(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
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

        private static void SetStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
            string[] generatedNames = { RangeRootName, WeaponUiRootName };
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (generatedNames.Contains(root.name, StringComparer.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
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
