using System;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Milestone4;
using RulesOfEntry.Missions;
using RulesOfEntry.Officers;
using RulesOfEntry.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone5
{
    public static class RulesOfEntryMilestoneFiveSetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 5/Build Mission and After-Action Prototype";

        internal const string MissionDefinitionPath =
            "Assets/_Project/RulesOfEntry/Data/Missions/M5_TrainingOperation.asset";
        internal const string RoePolicyPath =
            "Assets/_Project/RulesOfEntry/Data/Missions/M5_TrainingROE.asset";
        internal const string AfterActionUiPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_MissionAfterActionDebugUI.prefab";
        internal const string GeneratedRootName = "[Milestone5_Mission]";
        internal const string AfterActionUiRootName = "ROE_MissionAfterActionDebugUI";

        private const string MissionDataFolder =
            "Assets/_Project/RulesOfEntry/Data/Missions";
        private const string UiPrefabFolder =
            "Assets/_Project/RulesOfEntry/Prefabs/UI";
        private const string MarkerMaterialPath =
            "Assets/_Project/RulesOfEntry/Art/Materials/M4_OrderMarker.mat";

        [MenuItem(MenuPath, false, 60)]
        public static void BuildMissionPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building the Milestone 5 prototype.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("Milestone 5", "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                EnsureFolder(MissionDataFolder);
                EnsureFolder(UiPrefabFolder);
                MissionDefinition definition = CreateOrUpdateMissionDefinition();
                RulesOfEngagementPolicy policy = CreateOrUpdateRoePolicy();
                GameObject uiPrefab = CreateAfterActionUiPrefab();

                Scene scene = EditorSceneManager.OpenScene(
                    ProjectInfo.PrototypeScenePath,
                    OpenSceneMode.Single);
                SceneManager.SetActiveScene(scene);
                RequireMilestoneFourScene(scene);
                RemovePreviousGeneratedContent(scene);

                GameObject missionRoot = new GameObject(GeneratedRootName);
                SceneManager.MoveGameObjectToScene(missionRoot, scene);
                MissionController controller = missionRoot.AddComponent<MissionController>();
                controller.Configure(definition, policy, true, 0.25f);
                CreateDebriefConsole(missionRoot.transform, controller);

                GameObject uiObject = InstantiatePrefab(uiPrefab, scene);
                uiObject.name = AfterActionUiRootName;
                MissionAfterActionDebugUI ui =
                    uiObject.GetComponent<MissionAfterActionDebugUI>();
                Text reportText = uiObject.GetComponentInChildren<Text>(true);
                ui.Configure(controller, reportText);
                EditorUtility.SetDirty(ui);
                PrefabUtility.RecordPrefabInstancePropertyModifications(ui);

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException(
                        $"Unity could not save {ProjectInfo.PrototypeScenePath}.");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                GameObject savedRoot = ReloadAndVerifyScene();
                Selection.activeGameObject = savedRoot;
                ProjectLog.Info(
                    "Milestone 5",
                    "Mission definition, ROE policy, evidence evaluation, and after-action prototype created. Running validation now.");
                RulesOfEntryMilestoneFiveValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Milestone 5", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 5 setup stopped. Check the first Console error for the root cause.",
                    "OK");
            }
        }

        private static MissionDefinition CreateOrUpdateMissionDefinition()
        {
            MissionDefinition definition = AssetDatabase.LoadAssetAtPath<MissionDefinition>(
                MissionDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<MissionDefinition>();
                definition.name = "M5 Training Operation";
                AssetDatabase.CreateAsset(definition, MissionDefinitionPath);
            }

            MissionObjectiveDefinition secureSuspect = new MissionObjectiveDefinition();
            secureSuspect.Configure(
                "secure_primary_suspect",
                "Secure the primary suspect",
                "Establish lawful physical custody of the identified suspect.",
                MissionObjectiveType.SecureSubject,
                "m3_suspect_01",
                string.Empty,
                true,
                30);
            MissionObjectiveDefinition protectCivilian = new MissionObjectiveDefinition();
            protectCivilian.Configure(
                "protect_civilian",
                "Protect the civilian",
                "The civilian must remain uninjured throughout the response.",
                MissionObjectiveType.ProtectActor,
                "m3_civilian_01",
                string.Empty,
                true,
                25);
            MissionObjectiveDefinition clearRoom = new MissionObjectiveDefinition();
            clearRoom.Configure(
                "verify_north_room",
                "Verify the north room clear",
                "Two actionable officers must verify that no active threat remains.",
                MissionObjectiveType.VerifyRoomClear,
                string.Empty,
                "prototype_north_training_room",
                true,
                25);
            MissionObjectiveDefinition preserveTeam = new MissionObjectiveDefinition();
            preserveTeam.Configure(
                "preserve_response_team",
                "Preserve the response team",
                "No assigned officer may become incapacitated or die.",
                MissionObjectiveType.PreserveOfficerTeam,
                string.Empty,
                string.Empty,
                true,
                20);
            definition.Configure(
                "m5_training_operation",
                "Training Operation: Controlled Resolution",
                "Resolve the armed-subject incident, protect the civilian, clear the north room, and account for every use of force.",
                new[] { secureSuspect, protectCivilian, clearRoom, preserveTeam });
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static RulesOfEngagementPolicy CreateOrUpdateRoePolicy()
        {
            RulesOfEngagementPolicy policy =
                AssetDatabase.LoadAssetAtPath<RulesOfEngagementPolicy>(RoePolicyPath);
            if (policy == null)
            {
                policy = ScriptableObject.CreateInstance<RulesOfEngagementPolicy>();
                policy.name = "M5 Training ROE";
                AssetDatabase.CreateAsset(policy, RoePolicyPath);
            }

            policy.Configure(
                "m5_training_roe",
                "Threat-Based Deadly Force Policy",
                "Deadly force requires recorded facts supporting an imminent deadly threat. Force against civilians, officers, controlled subjects, or incapacitated subjects is prohibited. Ambiguous discharges require human review.",
                5,
                20,
                45,
                59);
            EditorUtility.SetDirty(policy);
            return policy;
        }

        private static GameObject CreateAfterActionUiPrefab()
        {
            GameObject root = new GameObject(
                "ROE_MissionAfterActionDebugUI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup),
                typeof(MissionAfterActionDebugUI));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 84;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            GameObject panelObject = new GameObject("MissionPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(root.transform, false);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0f, 1f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            panel.anchoredPosition = new Vector2(24f, -24f);
            panel.sizeDelta = new Vector2(610f, 300f);
            panelObject.GetComponent<Image>().color = new Color(0.015f, 0.025f, 0.035f, 0.86f);

            GameObject textObject = new GameObject("ReportText", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panelObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 14f);
            textRect.offsetMax = new Vector2(-16f, -14f);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.color = new Color(0.88f, 0.95f, 1f, 1f);
            text.text = "MISSION • awaiting setup";

            root.GetComponent<MissionAfterActionDebugUI>().Configure(null, text);
            return SavePrefabAndDestroy(root, AfterActionUiPrefabPath);
        }

        private static MissionDebriefInteractable CreateDebriefConsole(
            Transform parent,
            MissionController controller)
        {
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer < 0)
            {
                throw new InvalidOperationException(
                    "Required Interactable layer is missing. Rebuild Milestone 1.");
            }

            GameObject console = GameObject.CreatePrimitive(PrimitiveType.Cube);
            console.name = "M5_DebriefConsole";
            console.layer = interactableLayer;
            console.transform.SetParent(parent, false);
            console.transform.position = new Vector3(3f, 0.7f, -6.5f);
            console.transform.localScale = new Vector3(0.9f, 1.4f, 0.35f);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MarkerMaterialPath);
            Renderer renderer = console.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            MissionDebriefInteractable interactable =
                console.AddComponent<MissionDebriefInteractable>();
            interactable.Configure(controller, 1.25f);

            GameObject labelObject = new GameObject("DebriefLabel");
            labelObject.layer = interactableLayer;
            labelObject.transform.SetParent(console.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.62f, -0.55f);
            labelObject.transform.localRotation = Quaternion.identity;
            labelObject.transform.localScale = new Vector3(1.1f, 0.7f, 1f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = "DEBRIEF";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.12f;
            label.fontSize = 42;
            label.color = Color.white;
            return interactable;
        }

        private static void RequireMilestoneFourScene(Scene scene)
        {
            bool hasMilestoneFour = scene.GetRootGameObjects().Any(root =>
                string.Equals(
                    root.name,
                    RulesOfEntryMilestoneFourSetup.GeneratedRootName,
                    StringComparison.Ordinal));
            TacticalRoomVolume room = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<TacticalRoomVolume>(true))
                .FirstOrDefault();
            if (!hasMilestoneFour || room == null || !room.HasCompleteConfiguration)
            {
                throw new InvalidOperationException(
                    "Milestone 4 officer initiative and room clearance are missing. Rebuild Milestone 4 before Milestone 5.");
            }
        }

        private static void RemovePreviousGeneratedContent(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, GeneratedRootName, StringComparison.Ordinal)
                    || string.Equals(root.name, AfterActionUiRootName, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static GameObject ReloadAndVerifyScene()
        {
            Scene scene = EditorSceneManager.OpenScene(
                ProjectInfo.PrototypeScenePath,
                OpenSceneMode.Single);
            GameObject missionRoot = scene.GetRootGameObjects().FirstOrDefault(root =>
                string.Equals(root.name, GeneratedRootName, StringComparison.Ordinal));
            MissionController controller = missionRoot != null
                ? missionRoot.GetComponent<MissionController>()
                : null;
            MissionDebriefInteractable debrief = missionRoot != null
                ? missionRoot.GetComponentInChildren<MissionDebriefInteractable>(true)
                : null;
            GameObject uiRoot = scene.GetRootGameObjects().FirstOrDefault(root =>
                string.Equals(root.name, AfterActionUiRootName, StringComparison.Ordinal));
            MissionAfterActionDebugUI ui = uiRoot != null
                ? uiRoot.GetComponent<MissionAfterActionDebugUI>()
                : null;
            if (controller == null
                || !controller.HasCompleteConfiguration
                || debrief == null
                || !debrief.HasCompleteConfiguration
                || debrief.MissionController != controller
                || ui == null
                || !ui.HasCompleteConfiguration
                || ui.MissionController != controller)
            {
                throw new InvalidOperationException(
                    "The saved scene did not retain the Milestone 5 mission or UI references.");
            }

            return missionRoot;
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Scene scene)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Could not instantiate {prefab.name}.");
            }

            if (instance.scene != scene)
            {
                SceneManager.MoveGameObjectToScene(instance, scene);
            }

            return instance;
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
