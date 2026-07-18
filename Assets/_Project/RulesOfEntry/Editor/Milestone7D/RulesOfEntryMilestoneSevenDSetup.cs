using System;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Milestone7B;
using RulesOfEntry.Editor.Milestone7C;
using RulesOfEntry.Editor.TacticalHud;
using RulesOfEntry.Editor.UiPresentation;
using RulesOfEntry.UI.FrontEnd;
using RulesOfEntry.UI.TacticalHud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone7D
{
    public static class RulesOfEntryMilestoneSevenDSetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 7D/Build Campaign Saves and Operations Archive";

        internal const string FrontEndCampaignRootName = "[Milestone7D_Campaign]";
        internal const string NewCampaignPanelName = "M7D_NewCampaignPanel";

        private static readonly Color Backdrop =
            new Color(0.002f, 0.006f, 0.009f, 0.88f);
        private static readonly Color Panel =
            new Color(0.009f, 0.018f, 0.025f, 0.985f);
        private static readonly Color Field =
            new Color(0.025f, 0.045f, 0.058f, 0.98f);
        private static readonly Color Accent =
            new Color(0.02f, 0.62f, 0.95f, 1f);
        private static readonly Color Primary =
            new Color(0.92f, 0.95f, 0.97f, 1f);
        private static readonly Color Secondary =
            new Color(0.53f, 0.63f, 0.69f, 1f);

        [MenuItem(MenuPath, false, 96)]
        public static void BuildCampaignSavesAndOperationsArchive()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building Milestone 7D.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("Milestone 7D", "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                ConfigurePlayerPrefabIdentity();
                ConfigureFrontEndScene();
                GameObject reportPrefab =
                    RulesOfEntryMilestoneSevenBSetup.CreatePresentationPrefab();
                RulesOfEntryMilestoneSevenCSetup.ConfigureOperationScene(reportPrefab);
                RulesOfEntryMilestoneSevenCSetup.ConfigureHeadquartersScene();
                ConfigureSceneIdentity(ProjectInfo.HeadquartersScenePath);
                ConfigureSceneIdentity(ProjectInfo.PrototypeScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                RulesOfEntryMilestoneSevenDValidator.RunValidation(true);
                ProjectLog.Info(
                    "Milestone 7D",
                    "Campaign identity, versioned saves, persistent operation history, and archive browsing were built.");
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 7D was built. Run its validator, all EditMode tests, and the campaign persistence checklist.",
                    "OK");
            }
            catch (Exception exception)
            {
                ProjectLog.Error("Milestone 7D", exception.ToString());
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 7D setup failed. See the first Console error.",
                    "OK");
            }
        }

        private static void ConfigureFrontEndScene()
        {
            Scene scene = EditorSceneManager.OpenScene(
                ProjectInfo.FrontEndScenePath,
                OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            FrontEndFlowController flow = FindInScene<FrontEndFlowController>(scene);
            Canvas canvas = FindInScene<Canvas>(scene);
            CanvasGroup mainMenu = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<CanvasGroup>(true))
                .FirstOrDefault(group => string.Equals(
                    group.name,
                    "MainMenu",
                    StringComparison.Ordinal));
            Button continueButton = FindNamed<Button>(scene, "ContinueCampaignButton");
            Button newButton = FindNamed<Button>(scene, "NewCampaignButton");
            Font font = AssetDatabase.LoadAssetAtPath<Font>(
                RulesOfEntryUiPresentationSetup.TypographyPath);
            if (flow == null || !flow.HasCompleteConfiguration
                || canvas == null || mainMenu == null
                || continueButton == null || newButton == null || font == null)
            {
                throw new InvalidOperationException(
                    "The cinematic front-end campaign seams are incomplete. Rebuild UI Presentation first.");
            }

            RemoveNamedObject(scene, FrontEndCampaignRootName);
            GameObject root = new GameObject(
                FrontEndCampaignRootName,
                typeof(RectTransform),
                typeof(CampaignFrontEndController));
            root.transform.SetParent(canvas.transform, false);
            Stretch(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

            Text menuStatus = CreateText(
                "CampaignStatus",
                root.transform,
                font,
                13,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Secondary,
                "NO CAMPAIGN RECORD  //  NEW CAMPAIGN REQUIRED");
            Anchor(
                menuStatus.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(86f, 88f),
                new Vector2(660f, 28f));

            CanvasGroup campaignPanel = CreateNewCampaignPanel(
                root.transform,
                font,
                out InputField officerName,
                out InputField badge,
                out Button create,
                out Button cancel,
                out Text status);
            CampaignFrontEndController controller =
                root.GetComponent<CampaignFrontEndController>();
            controller.Configure(
                flow,
                mainMenu,
                continueButton,
                newButton,
                campaignPanel,
                officerName,
                badge,
                create,
                cancel,
                status,
                menuStatus);
            newButton.interactable = true;
            continueButton.interactable = false;
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException("Unity could not save the campaign front end.");
            }
        }

        private static CanvasGroup CreateNewCampaignPanel(
            Transform parent,
            Font font,
            out InputField officerName,
            out InputField badge,
            out Button create,
            out Button cancel,
            out Text status)
        {
            GameObject panelObject = new GameObject(
                NewCampaignPanelName,
                typeof(RectTransform),
                typeof(CanvasGroup));
            panelObject.transform.SetParent(parent, false);
            Stretch(panelObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            CanvasGroup group = panelObject.GetComponent<CanvasGroup>();

            Image backdrop = CreateImage("Backdrop", panelObject.transform, Backdrop);
            Stretch(backdrop.rectTransform, Vector2.zero, Vector2.zero);
            Image sheet = CreateImage("PersonnelSheet", panelObject.transform, Panel);
            sheet.rectTransform.anchorMin = new Vector2(0.57f, 0.16f);
            sheet.rectTransform.anchorMax = new Vector2(0.92f, 0.84f);
            sheet.rectTransform.offsetMin = Vector2.zero;
            sheet.rectTransform.offsetMax = Vector2.zero;
            Outline outline = sheet.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.42f, 0.62f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);

            Image rule = CreateImage("SignalRule", sheet.transform, Accent);
            rule.rectTransform.anchorMin = new Vector2(0f, 1f);
            rule.rectTransform.anchorMax = Vector2.one;
            rule.rectTransform.offsetMin = new Vector2(0f, -4f);
            rule.rectTransform.offsetMax = Vector2.zero;

            Text eyebrow = CreateText(
                "Eyebrow",
                sheet.transform,
                font,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Accent,
                "CALDER CITY POLICE  //  PERSONNEL SERVICES");
            SetTopLeft(eyebrow.rectTransform, 42f, 30f, 534f, 28f);
            Text title = CreateText(
                "Title",
                sheet.transform,
                font,
                36,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Primary,
                "NEW CAMPAIGN");
            SetTopLeft(title.rectTransform, 42f, 68f, 534f, 54f);
            Text instructions = CreateText(
                "Instructions",
                sheet.transform,
                font,
                16,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                Secondary,
                "Create the sworn-officer identity used by the body-camera overlay and permanent operation record. Department assignment is Calder City Police Department.");
            SetTopLeft(instructions.rectTransform, 42f, 142f, 534f, 86f);

            officerName = CreateInputField(
                "OfficerName",
                sheet.transform,
                font,
                "OFFICER FULL NAME",
                "Example: Alex Carter",
                new Vector2(42f, -252f),
                48);
            badge = CreateInputField(
                "BadgeIdentifier",
                sheet.transform,
                font,
                "BADGE NUMBER",
                "Example: A127",
                new Vector2(42f, -366f),
                12);

            status = CreateText(
                "Status",
                sheet.transform,
                font,
                13,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                Secondary,
                "ENTER THE PERSONNEL IDENTITY USED BY THE BODY-CAMERA AND OPERATION RECORD.");
            SetTopLeft(status.rectTransform, 42f, 476f, 534f, 72f);
            create = CreateButton(
                "CreateCampaign",
                sheet.transform,
                font,
                "CREATE CAMPAIGN  →",
                true,
                new Vector2(42f, -574f),
                new Vector2(330f, 58f));
            cancel = CreateButton(
                "Cancel",
                sheet.transform,
                font,
                "CANCEL",
                false,
                new Vector2(386f, -574f),
                new Vector2(190f, 58f));
            return group;
        }

        private static InputField CreateInputField(
            string name,
            Transform parent,
            Font font,
            string label,
            string placeholder,
            Vector2 position,
            int characterLimit)
        {
            GameObject root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(InputField));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(534f, 84f);
            Image background = root.GetComponent<Image>();
            background.color = Field;

            Text labelText = CreateText(
                "FieldLabel",
                root.transform,
                font,
                13,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                Accent,
                label);
            SetOffsets(labelText.rectTransform, 16f, 8f, -16f, -31f);
            Text valueText = CreateText(
                "Value",
                root.transform,
                font,
                19,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Primary,
                string.Empty);
            SetOffsets(valueText.rectTransform, 16f, 30f, -16f, -4f);
            Text placeholderText = CreateText(
                "Placeholder",
                root.transform,
                font,
                17,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                new Color(Secondary.r, Secondary.g, Secondary.b, 0.62f),
                placeholder);
            SetOffsets(placeholderText.rectTransform, 16f, 30f, -16f, -4f);
            InputField input = root.GetComponent<InputField>();
            input.targetGraphic = background;
            input.textComponent = valueText;
            input.placeholder = placeholderText;
            input.characterLimit = characterLimit;
            input.lineType = InputField.LineType.SingleLine;
            input.contentType = InputField.ContentType.Standard;
            return input;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            Font font,
            string label,
            bool primary,
            Vector2 position,
            Vector2 size)
        {
            Image background = CreateImage(
                name,
                parent,
                primary
                    ? new Color(0.015f, 0.22f, 0.34f, 0.98f)
                    : Field);
            RectTransform rect = background.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            Text text = CreateText(
                "Label",
                background.transform,
                font,
                17,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Primary,
                label);
            Stretch(text.rectTransform, new Vector2(10f, 4f), new Vector2(-10f, -4f));
            return button;
        }

        private static void ConfigurePlayerPrefabIdentity()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                RulesOfEntryTacticalHudSetup.PlayerPrefabPath);
            try
            {
                BodyCameraIdentity identity = root.GetComponent<BodyCameraIdentity>();
                if (identity == null)
                {
                    throw new InvalidOperationException(
                        "The player prefab is missing BodyCameraIdentity. Rebuild the Tactical HUD first.");
                }

                CampaignBodyCameraIdentityBinder binder =
                    root.GetComponent<CampaignBodyCameraIdentityBinder>()
                    ?? root.AddComponent<CampaignBodyCameraIdentityBinder>();
                binder.Configure(identity);
                if (PrefabUtility.SaveAsPrefabAsset(
                        root,
                        RulesOfEntryTacticalHudSetup.PlayerPrefabPath) == null)
                {
                    throw new InvalidOperationException(
                        "Unity could not save the campaign identity binding on the player prefab.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureSceneIdentity(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            BodyCameraIdentity identity = FindInScene<BodyCameraIdentity>(scene);
            if (identity == null)
            {
                throw new InvalidOperationException(
                    $"Body-camera identity is missing in {scenePath}.");
            }

            CampaignBodyCameraIdentityBinder binder =
                identity.GetComponent<CampaignBodyCameraIdentityBinder>()
                ?? identity.gameObject.AddComponent<CampaignBodyCameraIdentityBinder>();
            binder.Configure(identity);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Unity could not save {scenePath}.");
            }
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Image));
            result.transform.SetParent(parent, false);
            Image image = result.GetComponent<Image>();
            image.color = color;
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

        private static void Anchor(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 position,
            Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetOffsets(
            RectTransform rect,
            float left,
            float top,
            float right,
            float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, -bottom);
            rect.offsetMax = new Vector2(right, -top);
        }

        private static void SetTopLeft(
            RectTransform rect,
            float left,
            float top,
            float width,
            float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void Stretch(
            RectTransform rect,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static T FindNamed<T>(Scene scene, string name) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .FirstOrDefault(component => string.Equals(
                    component.name,
                    name,
                    StringComparison.Ordinal));
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .FirstOrDefault();
        }

        private static void RemoveNamedObject(Scene scene, string name)
        {
            Transform existing = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(transform => string.Equals(
                    transform.name,
                    name,
                    StringComparison.Ordinal));
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }
        }
    }
}
