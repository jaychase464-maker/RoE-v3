using System;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Milestone7B;
using RulesOfEntry.Headquarters;
using RulesOfEntry.Input;
using RulesOfEntry.Missions;
using RulesOfEntry.Player;
using RulesOfEntry.UI;
using RulesOfEntry.UI.Headquarters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone7C
{
    public static class RulesOfEntryMilestoneSevenCSetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 7C/Build Operation Closure and Headquarters Return";

        internal const string HeadquartersRootName = "[Milestone7C_OperationClosure]";
        internal const string ReviewCanvasName = "ROE_HeadquartersAfterActionReview";
        internal const string ReviewTerminalName = "M7C_AfterActionArchiveTerminal";

        private const string TypographyPath =
            "Assets/_Project/RulesOfEntry/Art/UI/Fonts/LatinModernSansDemiCondensed.otf";
        private const string DarkMaterialPath =
            "Assets/_Project/RulesOfEntry/Art/Materials/Headquarters/M6A_HQDark.mat";
        private const string AccentMaterialPath =
            "Assets/_Project/RulesOfEntry/Art/Materials/Headquarters/M6A_HQAccent.mat";

        private static readonly Color Background = new Color(0.004f, 0.009f, 0.014f, 0.975f);
        private static readonly Color Panel = new Color(0.012f, 0.025f, 0.035f, 0.985f);
        private static readonly Color Accent = new Color(0.02f, 0.62f, 0.95f, 1f);
        private static readonly Color Primary = new Color(0.9f, 0.95f, 0.98f, 1f);
        private static readonly Color Secondary = new Color(0.48f, 0.62f, 0.69f, 1f);

        [MenuItem(MenuPath, false, 94)]
        public static void BuildOperationClosureAndHeadquartersReturn()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building Milestone 7C.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("Milestone 7C", "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                GameObject reportPrefab =
                    RulesOfEntryMilestoneSevenBSetup.CreatePresentationPrefab();
                ConfigureOperationScene(reportPrefab);
                ConfigureHeadquartersScene();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                RulesOfEntryMilestoneSevenCValidator.RunValidation(true);
                ProjectLog.Info(
                    "Milestone 7C",
                    "Operation report return and headquarters archive review were built.");
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 7C was built. Run its validator, all EditMode tests, and the operation-return checklist.",
                    "OK");
            }
            catch (Exception exception)
            {
                ProjectLog.Error("Milestone 7C", exception.ToString());
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 7C setup failed. See the first Console error.",
                    "OK");
            }
        }

        internal static void ConfigureOperationScene(GameObject reportPrefab)
        {
            Scene scene = EditorSceneManager.OpenScene(
                ProjectInfo.PrototypeScenePath,
                OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            MissionController controller = FindInScene<MissionController>(scene);
            TacticalPlayerInput input = FindInScene<TacticalPlayerInput>(scene);
            CursorStateController cursor = FindInScene<CursorStateController>(scene);
            if (controller == null || !controller.HasCompleteConfiguration
                || input == null || cursor == null)
            {
                throw new InvalidOperationException(
                    "The operation scene is missing its mission, input, or cursor foundation. Rebuild Milestone 7B first.");
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(
                    root.name,
                    RulesOfEntryMilestoneSevenBSetup.PresentationRootName,
                    StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }

            EnsureEventSystem(scene);
            GameObject instance = PrefabUtility.InstantiatePrefab(reportPrefab, scene) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException(
                    "Unity could not instantiate the updated final report prefab.");
            }

            instance.name = RulesOfEntryMilestoneSevenBSetup.PresentationRootName;
            MissionAfterActionPresentation presentation =
                instance.GetComponent<MissionAfterActionPresentation>();
            presentation.ConfigureSources(controller, input, cursor);
            PrefabUtility.RecordPrefabInstancePropertyModifications(presentation);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException("Unity could not save the operation scene.");
            }
        }

        internal static void ConfigureHeadquartersScene()
        {
            Scene scene = EditorSceneManager.OpenScene(
                ProjectInfo.HeadquartersScenePath,
                OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            TacticalPlayerInput input = FindInScene<TacticalPlayerInput>(scene);
            CursorStateController cursor = FindInScene<CursorStateController>(scene);
            EventSystem eventSystem = FindInScene<EventSystem>(scene);
            Font font = AssetDatabase.LoadAssetAtPath<Font>(TypographyPath);
            Material dark = AssetDatabase.LoadAssetAtPath<Material>(DarkMaterialPath);
            Material accentMaterial = AssetDatabase.LoadAssetAtPath<Material>(AccentMaterialPath);
            if (input == null || cursor == null || eventSystem == null
                || eventSystem.GetComponent<InputSystemUIInputModule>() == null
                || font == null || dark == null || accentMaterial == null)
            {
                throw new InvalidOperationException(
                    "Headquarters input, EventSystem, typography, or materials are missing. Rebuild Milestone 6A first.");
            }

            RemoveHeadquartersRoot(scene);
            GameObject root = new GameObject(HeadquartersRootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            HeadquartersAfterActionReviewController review = CreateReviewCanvas(
                root.transform,
                font,
                input,
                cursor);
            CreateArchiveTerminal(
                root.transform,
                review,
                dark,
                accentMaterial);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException("Unity could not save headquarters.");
            }
        }

        private static HeadquartersAfterActionReviewController CreateReviewCanvas(
            Transform parent,
            Font font,
            TacticalPlayerInput input,
            CursorStateController cursor)
        {
            GameObject canvasObject = new GameObject(
                ReviewCanvasName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(HeadquartersAfterActionReviewController));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 126;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image interfaceImage = CreateImage("ArchiveInterface", canvasObject.transform, Background);
            Stretch(interfaceImage.rectTransform, Vector2.zero, Vector2.zero);
            CanvasGroup group = interfaceImage.gameObject.AddComponent<CanvasGroup>();

            Image topRule = CreateImage("TopRule", interfaceImage.transform, Accent);
            Anchor(topRule.rectTransform, new Vector2(0.06f, 0.925f), new Vector2(0.94f, 0.928f));
            Text eyebrow = CreateText(
                "Eyebrow",
                interfaceImage.transform,
                font,
                18,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Accent,
                "CALDER CITY POLICE  //  OPERATIONS ARCHIVE  //  MOST RECENT INCIDENT");
            Anchor(eyebrow.rectTransform, new Vector2(0.06f, 0.94f), new Vector2(0.88f, 0.98f));
            Text operation = CreateText(
                "Operation",
                interfaceImage.transform,
                font,
                42,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Primary,
                "NO COMPLETED OPERATION");
            Anchor(operation.rectTransform, new Vector2(0.06f, 0.84f), new Vector2(0.72f, 0.925f));

            Image tierPanel = CreateImage("TierPanel", interfaceImage.transform, Panel);
            Anchor(tierPanel.rectTransform, new Vector2(0.06f, 0.59f), new Vector2(0.255f, 0.82f));
            AddOutline(tierPanel.gameObject, Accent);
            Text tier = CreateText(
                "Tier",
                tierPanel.transform,
                font,
                108,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Accent,
                "—");
            Anchor(tier.rectTransform, new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.82f));
            Text score = CreateText(
                "Score",
                tierPanel.transform,
                font,
                24,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Primary,
                "000 / 100");
            Anchor(score.rectTransform, new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.25f));

            Text categories = CreatePanelText(
                "Categories",
                interfaceImage.transform,
                font,
                new Vector2(0.275f, 0.34f),
                new Vector2(0.55f, 0.82f));
            Text outcome = CreatePanelText(
                "Outcome",
                interfaceImage.transform,
                font,
                new Vector2(0.57f, 0.59f),
                new Vector2(0.94f, 0.82f));
            Text objectives = CreatePanelText(
                "Objectives",
                interfaceImage.transform,
                font,
                new Vector2(0.57f, 0.34f),
                new Vector2(0.94f, 0.57f));
            Text findings = CreatePanelText(
                "Findings",
                interfaceImage.transform,
                font,
                new Vector2(0.06f, 0.13f),
                new Vector2(0.94f, 0.31f));
            Text metadata = CreateText(
                "Metadata",
                interfaceImage.transform,
                font,
                15,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Secondary,
                "SESSION RECORD");
            Anchor(metadata.rectTransform, new Vector2(0.06f, 0.055f), new Vector2(0.46f, 0.1f));
            Text navigation = CreateText(
                "Navigation",
                interfaceImage.transform,
                font,
                13,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                Secondary,
                "OPERATION 01 / 01  //  LEFT / RIGHT OR LB / RB");
            Anchor(navigation.rectTransform, new Vector2(0.46f, 0.105f), new Vector2(0.94f, 0.135f));
            Button previous = CreateButton(
                "PreviousReport",
                interfaceImage.transform,
                font,
                "←  PREVIOUS",
                out _);
            Anchor(
                previous.GetComponent<RectTransform>(),
                new Vector2(0.48f, 0.045f),
                new Vector2(0.59f, 0.1f));
            Button next = CreateButton(
                "NextReport",
                interfaceImage.transform,
                font,
                "NEXT  →",
                out _);
            Anchor(
                next.GetComponent<RectTransform>(),
                new Vector2(0.60f, 0.045f),
                new Vector2(0.71f, 0.1f));
            Button close = CreateButton(
                "CloseReport",
                interfaceImage.transform,
                font,
                "CLOSE REPORT  //  TAB / B",
                out _);
            Anchor(
                close.GetComponent<RectTransform>(),
                new Vector2(0.74f, 0.045f),
                new Vector2(0.94f, 0.11f));

            HeadquartersAfterActionReviewController controller =
                canvasObject.GetComponent<HeadquartersAfterActionReviewController>();
            controller.Configure(
                input,
                cursor,
                interfaceImage.gameObject,
                group,
                operation,
                tier,
                score,
                categories,
                outcome,
                objectives,
                findings,
                metadata,
                navigation,
                previous,
                next,
                close,
                true);
            return controller;
        }

        private static void CreateArchiveTerminal(
            Transform parent,
            HeadquartersAfterActionReviewController review,
            Material dark,
            Material accentMaterial)
        {
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer < 0)
            {
                throw new InvalidOperationException("The Interactable layer is missing.");
            }

            GameObject terminal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terminal.name = ReviewTerminalName;
            terminal.transform.SetParent(parent, false);
            terminal.transform.localPosition = new Vector3(3.6f, 0.72f, 1.3f);
            terminal.transform.localScale = new Vector3(2.4f, 1.45f, 0.65f);
            terminal.layer = layer;
            terminal.GetComponent<Renderer>().sharedMaterial = dark;
            HeadquartersAfterActionTerminalInteractable interactable =
                terminal.AddComponent<HeadquartersAfterActionTerminalInteractable>();
            interactable.Configure(review, 0f);

            GameObject display = GameObject.CreatePrimitive(PrimitiveType.Cube);
            display.name = "ArchiveDisplay";
            display.transform.SetParent(terminal.transform, false);
            display.transform.localPosition = new Vector3(0f, 0.18f, -0.54f);
            display.transform.localScale = new Vector3(0.8f, 0.5f, 0.08f);
            display.layer = layer;
            display.GetComponent<Renderer>().sharedMaterial = accentMaterial;

            GameObject labelObject = new GameObject("AfterActionArchiveLabel");
            labelObject.transform.SetParent(terminal.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.19f, -0.6f);
            labelObject.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            labelObject.layer = layer;
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = "AFTER-ACTION ARCHIVE\nE: REVIEW LAST OPERATION";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.1f;
            label.fontSize = 42;
            label.color = Color.white;
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

        private static Button CreateButton(
            string name,
            Transform parent,
            Font font,
            string label,
            out Text labelText)
        {
            GameObject result = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            result.transform.SetParent(parent, false);
            Image image = result.GetComponent<Image>();
            image.color = new Color(0.015f, 0.22f, 0.34f, 0.98f);
            Button button = result.GetComponent<Button>();
            button.targetGraphic = image;
            labelText = CreateText(
                "Label",
                result.transform,
                font,
                18,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Primary,
                label);
            Stretch(labelText.rectTransform, new Vector2(12f, 4f), new Vector2(-12f, -4f));
            return button;
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

        private static void EnsureEventSystem(Scene scene)
        {
            EventSystem existing = FindInScene<EventSystem>(scene);
            if (existing != null)
            {
                if (existing.GetComponent<InputSystemUIInputModule>() == null)
                {
                    existing.gameObject.AddComponent<InputSystemUIInputModule>();
                }

                return;
            }

            GameObject eventSystem = new GameObject(
                "M7C_EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
        }

        private static void RemoveHeadquartersRoot(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, HeadquartersRootName, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }
    }
}
