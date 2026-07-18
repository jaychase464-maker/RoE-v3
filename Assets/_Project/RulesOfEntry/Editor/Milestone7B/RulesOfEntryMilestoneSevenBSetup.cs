using System;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Input;
using RulesOfEntry.Missions;
using RulesOfEntry.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone7B
{
    public static class RulesOfEntryMilestoneSevenBSetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 7B/Build Automatic After-Action Tier System";

        internal const string PresentationRootName = "ROE_AfterActionReport";
        internal const string PresentationPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_AfterActionReport.prefab";
        internal const float TargetCompletionSeconds = 600f;
        internal const float MaximumScoredCompletionSeconds = 1200f;
        internal const float AutoCompletionConfirmationSeconds = 3f;

        private static readonly Color Background = new Color(0.004f, 0.009f, 0.014f, 0.965f);
        private static readonly Color Panel = new Color(0.012f, 0.025f, 0.035f, 0.98f);
        private static readonly Color Accent = new Color(0.02f, 0.62f, 0.95f, 1f);
        private static readonly Color Primary = new Color(0.9f, 0.95f, 0.98f, 1f);
        private static readonly Color Secondary = new Color(0.48f, 0.62f, 0.69f, 1f);

        [MenuItem(MenuPath, false, 92)]
        public static void BuildAutomaticAfterActionTierSystem()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building Milestone 7B.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("Milestone 7B", "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                GameObject prefab = CreatePresentationPrefab();
                Scene scene = EditorSceneManager.OpenScene(
                    ProjectInfo.PrototypeScenePath,
                    OpenSceneMode.Single);
                SceneManager.SetActiveScene(scene);
                MissionController controller = FindInScene<MissionController>(scene);
                TacticalPlayerInput input = FindInScene<TacticalPlayerInput>(scene);
                if (controller == null || !controller.HasCompleteConfiguration)
                {
                    throw new InvalidOperationException(
                        "A configured mission controller is required. Run Milestone 7A setup first.");
                }

                if (input == null)
                {
                    throw new InvalidOperationException(
                        "Tactical player input is missing. Rebuild the current gameplay foundation first.");
                }

                controller.ConfigureAutomaticCompletion(
                    true,
                    AutoCompletionConfirmationSeconds);
                controller.Definition.ConfigurePerformanceTiming(
                    TargetCompletionSeconds,
                    MaximumScoredCompletionSeconds);
                EditorUtility.SetDirty(controller.Definition);
                RemovePreviousPresentation(scene);

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
                if (instance == null)
                {
                    throw new InvalidOperationException(
                        "Unity could not instantiate the after-action presentation prefab.");
                }

                instance.name = PresentationRootName;
                MissionAfterActionPresentation presentation =
                    instance.GetComponent<MissionAfterActionPresentation>();
                presentation.ConfigureSources(controller, input);
                foreach (MissionAfterActionDebugUI debugUi in scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<MissionAfterActionDebugUI>(true)))
                {
                    debugUi.gameObject.SetActive(false);
                }

                PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
                PrefabUtility.RecordPrefabInstancePropertyModifications(presentation);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException("Unity could not save the prototype scene.");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                RulesOfEntryMilestoneSevenBValidator.RunValidation(true);
                ProjectLog.Info(
                    "Milestone 7B",
                    "Automatic completion, factual category scoring, tiers, and the final report presentation were built.",
                    instance);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 7B was built. Run its validator, all EditMode tests, and the live mission checklist.",
                    "OK");
            }
            catch (Exception exception)
            {
                ProjectLog.Error("Milestone 7B", exception.ToString());
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 7B setup failed. See the first Console error.",
                    "OK");
            }
        }

        private static GameObject CreatePresentationPrefab()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject root = new GameObject(
                PresentationRootName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup),
                typeof(MissionAfterActionPresentation));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 125;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            Image backdrop = CreateImage("EvidenceLockBackdrop", root.transform, Background);
            Stretch(backdrop.rectTransform, Vector2.zero, Vector2.zero);

            Image topRule = CreateImage("TopRule", root.transform, Accent);
            Anchor(topRule.rectTransform, new Vector2(0.06f, 0.925f), new Vector2(0.94f, 0.928f));
            Text eyebrow = CreateText(
                "Eyebrow",
                root.transform,
                font,
                18,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Accent,
                "CALDER CITY POLICE  //  AFTER-ACTION REVIEW");
            Anchor(eyebrow.rectTransform, new Vector2(0.06f, 0.94f), new Vector2(0.75f, 0.98f));
            Text operation = CreateText(
                "Operation",
                root.transform,
                font,
                42,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Primary,
                "OPERATION");
            Anchor(operation.rectTransform, new Vector2(0.06f, 0.84f), new Vector2(0.72f, 0.925f));

            Image tierPanel = CreateImage("TierPanel", root.transform, Panel);
            Anchor(tierPanel.rectTransform, new Vector2(0.06f, 0.59f), new Vector2(0.255f, 0.82f));
            AddOutline(tierPanel.gameObject, Accent);
            Text tierLabel = CreateText(
                "TierLabel",
                tierPanel.transform,
                font,
                15,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Secondary,
                "OPERATIONAL TIER");
            Anchor(tierLabel.rectTransform, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.92f));
            Text tier = CreateText(
                "Tier",
                tierPanel.transform,
                font,
                108,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Accent,
                "S");
            Anchor(tier.rectTransform, new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.75f));
            Text score = CreateText(
                "Score",
                tierPanel.transform,
                font,
                24,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Primary,
                "100 / 100");
            Anchor(score.rectTransform, new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.24f));

            Text categories = CreatePanelText(
                "Categories",
                root.transform,
                font,
                new Vector2(0.275f, 0.34f),
                new Vector2(0.55f, 0.82f));
            Text metrics = CreatePanelText(
                "Metrics",
                root.transform,
                font,
                new Vector2(0.57f, 0.59f),
                new Vector2(0.94f, 0.82f));
            Text objectives = CreatePanelText(
                "Objectives",
                root.transform,
                font,
                new Vector2(0.57f, 0.34f),
                new Vector2(0.94f, 0.57f));
            Text findings = CreatePanelText(
                "Findings",
                root.transform,
                font,
                new Vector2(0.06f, 0.13f),
                new Vector2(0.94f, 0.31f));
            Text footer = CreateText(
                "Footer",
                root.transform,
                font,
                15,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Secondary,
                "FINAL EVIDENCE LOCK");
            Anchor(footer.rectTransform, new Vector2(0.06f, 0.055f), new Vector2(0.94f, 0.1f));
            Image bottomRule = CreateImage("BottomRule", root.transform, Accent);
            Anchor(bottomRule.rectTransform, new Vector2(0.06f, 0.047f), new Vector2(0.94f, 0.05f));

            root.GetComponent<MissionAfterActionPresentation>().Configure(
                null,
                null,
                group,
                operation,
                tier,
                score,
                categories,
                metrics,
                objectives,
                findings,
                footer);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PresentationPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Unity could not save {PresentationPrefabPath}.");
            }

            return prefab;
        }

        private static Text CreatePanelText(
            string name,
            Transform parent,
            Font font,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            Image panel = CreateImage(name + "Panel", parent, Panel);
            Anchor(panel.rectTransform, anchorMin, anchorMax);
            AddOutline(panel.gameObject, new Color(0.05f, 0.27f, 0.38f, 1f));
            Text text = CreateText(
                name,
                panel.transform,
                font,
                17,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                Primary,
                name.ToUpperInvariant());
            Stretch(text.rectTransform, new Vector2(20f, 16f), new Vector2(-20f, -16f));
            return text;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Image));
            result.transform.SetParent(parent, false);
            Image image = result.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Font font,
            int size,
            FontStyle style,
            TextAnchor alignment,
            Color color,
            string value)
        {
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Text));
            result.transform.SetParent(parent, false);
            Text text = result.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static void AddOutline(GameObject target, Color color)
        {
            Outline outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1f, -1f);
        }

        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .FirstOrDefault();
        }

        private static void RemovePreviousPresentation(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, PresentationRootName, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }
    }
}
