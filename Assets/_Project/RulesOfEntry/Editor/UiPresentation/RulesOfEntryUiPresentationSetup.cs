using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone5;
using RulesOfEntry.Missions;
using RulesOfEntry.UI;
using RulesOfEntry.UI.FrontEnd;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.UiPresentation
{
    public static class RulesOfEntryUiPresentationSetup
    {
        private const string MenuPath =
            "Tools/Rules of Entry/UI Presentation/Build Front End and Restyle HUD";
        internal const string FrontEndRootName = "[UI_FrontEnd]";
        internal const string PresentationRootName = "[UI_Presentation]";
        internal const string FrontEndScenePath = ProjectInfo.FrontEndScenePath;

        private const string FrontEndSceneFolder =
            "Assets/_Project/RulesOfEntry/Scenes/FrontEnd";
        internal const string SplashArtworkPath =
            "Assets/_Project/RulesOfEntry/Art/UI/Branding/TrooperStudiosSplash.png";
        internal const string WarningArtworkPath =
            "Assets/_Project/RulesOfEntry/Art/UI/Branding/PhotosensitivityWarning.png";
        internal const string MenuArtworkPath =
            "Assets/_Project/RulesOfEntry/Art/UI/Branding/TacticalMenuBackground.png";
        internal const string TypographyPath =
            "Assets/_Project/RulesOfEntry/Art/UI/Fonts/LatinModernSansDemiCondensed.otf";
        private const string InteractionUiRootName = "ROE_InteractionPromptUI";
        private const string WeaponUiRootName = "ROE_WeaponStatusUI";
        private const string HumanUiRootName = "ROE_HumanBehaviorDebugUI";
        private const string OfficerUiRootName = "ROE_OfficerCommandDebugUI";
        private const string MissionUiRootName = "ROE_MissionAfterActionDebugUI";

        private static readonly Color Background =
            new Color(0.0045f, 0.008f, 0.012f, 1f);
        private static readonly Color Surface =
            new Color(0.018f, 0.028f, 0.038f, 0.94f);
        private static readonly Color SurfaceRaised =
            new Color(0.032f, 0.047f, 0.06f, 0.97f);
        private static readonly Color Border =
            new Color(0.2f, 0.29f, 0.35f, 0.6f);
        private static readonly Color Primary =
            new Color(0.22f, 0.62f, 0.86f, 1f);
        private static readonly Color Signal =
            new Color(0.56f, 0.72f, 0.82f, 1f);
        private static readonly Color TextPrimary =
            new Color(0.92f, 0.95f, 0.97f, 1f);
        private static readonly Color TextSecondary =
            new Color(0.57f, 0.65f, 0.7f, 1f);

        [MenuItem(MenuPath, false, 70)]
        public static void BuildFrontEndAndHud()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before rebuilding the front end and HUD.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("UI Presentation", "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                RequireMilestoneFiveBaseline();
                EnsureFolder(FrontEndSceneFolder);
                ConfigureBrandingTextures();
                CreateFrontEndScene();
                RestylePrototypeHud();
                ConfigurePlayerPresentation();
                ConfigureBuildScenes();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Scene frontEnd = EditorSceneManager.OpenScene(
                    FrontEndScenePath,
                    OpenSceneMode.Single);
                Selection.activeGameObject = frontEnd.GetRootGameObjects()
                    .FirstOrDefault(root => string.Equals(
                        root.name,
                        FrontEndRootName,
                        StringComparison.Ordinal));
                ProjectLog.Info(
                    "UI Presentation",
                    "Trooper Studios splash, title, main menu, settings, credits, loading flow, and prototype HUD presentation were built. Running validation now.");
                RulesOfEntryUiPresentationValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("UI Presentation", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "UI Presentation setup stopped. Check the first Console error for the root cause.",
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
                "Milestone 5 must pass before the UI milestone. "
                + string.Join(" | ", errors.Select(error =>
                    $"{error.Check}: {error.Message}")));
        }

        private static void CreateFrontEndScene()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject root = new GameObject(FrontEndRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            GameObject cameraObject = new GameObject(
                "FrontEndCamera",
                typeof(Camera),
                typeof(AudioListener));
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.cullingMask = 0;
            camera.depth = -100f;

            GameObject canvasObject = new GameObject(
                "FrontEndCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root.transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(root.transform, false);
            EventSystem eventSystem = eventSystemObject.GetComponent<EventSystem>();
            eventSystem.firstSelectedGameObject = null;

            Font font = LoadFont();
            MissionDefinition operationDefinition =
                AssetDatabase.LoadAssetAtPath<MissionDefinition>(
                    RulesOfEntryMilestoneFiveSetup.MissionDefinitionPath);
            if (operationDefinition == null)
            {
                throw new InvalidOperationException(
                    "The Milestone 5 mission definition is missing. Rebuild Milestone 5 before generating the front end.");
            }

            Sprite splashArtwork = RequireSprite(SplashArtworkPath);
            Sprite warningArtwork = RequireSprite(WarningArtworkPath);
            Sprite menuArtwork = RequireSprite(MenuArtworkPath);
            CreateFrontEndBackdrop(canvasObject.transform, menuArtwork);
            Text buildLabel = CreateBuildLabel(canvasObject.transform, font);
            CanvasGroup splash = CreateSplashPanel(
                canvasObject.transform,
                splashArtwork);
            CanvasGroup warning = CreateWarningPanel(
                canvasObject.transform,
                warningArtwork);
            CanvasGroup title = CreateTitlePanel(
                canvasObject.transform,
                font,
                out Button titleContinue);
            CanvasGroup mainMenu = CreateMainMenuPanel(
                canvasObject.transform,
                font,
                out Button deploy,
                out Button training,
                out Button settings,
                out Button credits,
                out Button quit);
            CanvasGroup settingsPanel = CreateSettingsPanel(
                canvasObject.transform,
                font,
                out Slider masterVolume,
                out Toggle fullscreen,
                out Button quality,
                out Text qualityValue,
                out Button settingsBack);
            CanvasGroup creditsPanel = CreateCreditsPanel(
                canvasObject.transform,
                font,
                out Button creditsBack);
            CanvasGroup loading = CreateLoadingPanel(
                canvasObject.transform,
                font,
                out Text loadingContext,
                out Text loadingDestination,
                out Text loadingDetail,
                out Text loadingStatus,
                out Text loadingPercentage,
                out Image loadingFill);

            FrontEndFlowController controller = root.AddComponent<FrontEndFlowController>();
            controller.Configure(
                splash,
                warning,
                title,
                mainMenu,
                settingsPanel,
                creditsPanel,
                loading,
                titleContinue,
                deploy,
                training,
                settings,
                credits,
                quit,
                settingsBack,
                creditsBack,
                quality,
                masterVolume,
                fullscreen,
                qualityValue,
                loadingContext,
                loadingDestination,
                loadingDetail,
                loadingStatus,
                loadingPercentage,
                loadingFill,
                buildLabel,
                ProjectInfo.PrototypeScenePath,
                operationDefinition);
            EditorUtility.SetDirty(controller);

            splash.gameObject.SetActive(true);
            warning.gameObject.SetActive(false);
            title.gameObject.SetActive(false);
            mainMenu.gameObject.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
            creditsPanel.gameObject.SetActive(false);
            loading.gameObject.SetActive(false);

            if (!EditorSceneManager.SaveScene(scene, FrontEndScenePath))
            {
                throw new InvalidOperationException(
                    $"Unity could not save {FrontEndScenePath}.");
            }
        }

        private static void CreateFrontEndBackdrop(
            Transform parent,
            Sprite menuArtwork)
        {
            CreateFullScreenArtwork("TacticalMenuArtwork", parent, menuArtwork);

            Image colorGrade = CreateImage(
                "ColorGrade",
                parent,
                new Color(0.005f, 0.012f, 0.02f, 0.16f));
            Stretch(colorGrade.rectTransform);

            Image navigationField = CreateImage(
                "NavigationField",
                parent,
                new Color(0.002f, 0.006f, 0.01f, 0.68f));
            SetNormalizedRect(
                navigationField.rectTransform,
                Vector2.zero,
                new Vector2(0.43f, 1f));
        }

        private static CanvasGroup CreateSplashPanel(
            Transform parent,
            Sprite splashArtwork)
        {
            CanvasGroup group = CreatePanel("StudioSplash", parent);
            CreateFullScreenArtwork(
                "TrooperStudiosArtwork",
                group.transform,
                splashArtwork);
            return group;
        }

        private static CanvasGroup CreateWarningPanel(
            Transform parent,
            Sprite warningArtwork)
        {
            CanvasGroup group = CreatePanel("PhotosensitivityWarning", parent);
            CreateFullScreenArtwork(
                "WarningArtwork",
                group.transform,
                warningArtwork);
            return group;
        }

        private static CanvasGroup CreateTitlePanel(
            Transform parent,
            Font font,
            out Button continueButton)
        {
            CanvasGroup group = CreatePanel("TitleScreen", parent);
            Image clickSurface = group.gameObject.AddComponent<Image>();
            clickSurface.color = new Color(0f, 0f, 0f, 0f);
            continueButton = group.gameObject.AddComponent<Button>();
            continueButton.targetGraphic = clickSurface;
            continueButton.transition = Selectable.Transition.None;

            CreateStackedGameTitle(group.transform, font);

            Text descriptor = CreateText(
                "TitleDescriptor",
                group.transform,
                font,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                TextSecondary);
            Anchor(
                descriptor.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(86f, 142f),
                new Vector2(520f, 28f),
                Vector2.zero);
            descriptor.text = "TACTICAL LAW-ENFORCEMENT SIMULATION";

            Text prompt = CreateText(
                "ContinuePrompt",
                group.transform,
                font,
                18,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                TextPrimary);
            Anchor(
                prompt.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(86f, 98f),
                new Vector2(520f, 34f),
                Vector2.zero);
            prompt.text = "PRESS ANY KEY TO CONTINUE";
            return group;
        }

        private static CanvasGroup CreateMainMenuPanel(
            Transform parent,
            Font font,
            out Button deploy,
            out Button training,
            out Button settings,
            out Button credits,
            out Button quit)
        {
            CanvasGroup group = CreatePanel("MainMenu", parent);
            CreateStackedGameTitle(group.transform, font);

            CreateMainMenuItem(
                "ContinueCampaignButton",
                group.transform,
                font,
                "CONTINUE CAMPAIGN",
                new Vector2(86f, -446f),
                false);
            CreateMainMenuItem(
                "NewCampaignButton",
                group.transform,
                font,
                "NEW CAMPAIGN",
                new Vector2(86f, -502f),
                false);
            deploy = CreateMainMenuItem(
                "OperationsButton",
                group.transform,
                font,
                "OPERATIONS",
                new Vector2(86f, -558f),
                true);
            training = CreateMainMenuItem(
                "TrainingButton",
                group.transform,
                font,
                "TRAINING",
                new Vector2(86f, -614f),
                true);
            settings = CreateMainMenuItem(
                "SettingsButton",
                group.transform,
                font,
                "SETTINGS",
                new Vector2(86f, -670f),
                true);
            credits = CreateMainMenuItem(
                "CreditsButton",
                group.transform,
                font,
                "CREDITS",
                new Vector2(86f, -726f),
                true);
            quit = CreateMainMenuItem(
                "QuitButton",
                group.transform,
                font,
                "QUIT GAME",
                new Vector2(86f, -782f),
                true);

            Text footer = CreateText(
                "CommandFooter",
                group.transform,
                font,
                14,
                FontStyle.Bold,
                TextAnchor.LowerLeft,
                TextSecondary);
            Anchor(
                footer.rectTransform,
                Vector2.zero,
                new Vector2(86f, 46f),
                new Vector2(560f, 28f),
                Vector2.zero);
            footer.text = "<color=#389EDB>CALDER CITY, USA</color>   |   OPERATIONS COMMAND";

            return group;
        }

        private static CanvasGroup CreateSettingsPanel(
            Transform parent,
            Font font,
            out Slider masterVolume,
            out Toggle fullscreen,
            out Button quality,
            out Text qualityValue,
            out Button back)
        {
            CanvasGroup group = CreatePanel("Settings", parent);
            CreateSectionHeading(group.transform, font, "SETTINGS", "SYSTEM CONFIGURATION");
            RectTransform card = CreateCard(
                "SettingsCard",
                group.transform,
                new Vector2(0.66f, 0.5f),
                new Vector2(820f, 610f));

            Text audioLabel = CreateText(
                "AudioSection",
                card,
                font,
                16,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                Primary);
            SetOffsets(audioLabel.rectTransform, 42f, 34f, -42f, -72f);
            audioLabel.text = "AUDIO";
            masterVolume = CreateSlider(
                "MasterVolume",
                card,
                font,
                "MASTER VOLUME",
                new Vector2(0f, -94f));

            Text videoLabel = CreateText(
                "VideoSection",
                card,
                font,
                16,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                Primary);
            SetOffsets(videoLabel.rectTransform, 42f, 228f, -42f, -266f);
            videoLabel.text = "DISPLAY";

            fullscreen = CreateToggle(
                "FullscreenToggle",
                card,
                font,
                "FULLSCREEN",
                new Vector2(0f, -284f));
            quality = CreateMenuButton(
                "QualityButton",
                card,
                font,
                "QUALITY PRESET",
                new Vector2(42f, -372f),
                false,
                new Vector2(736f, 68f));
            qualityValue = CreateText(
                "QualityValue",
                quality.transform,
                font,
                17,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                Primary);
            qualityValue.rectTransform.anchorMin = Vector2.zero;
            qualityValue.rectTransform.anchorMax = Vector2.one;
            qualityValue.rectTransform.offsetMin = new Vector2(330f, 0f);
            qualityValue.rectTransform.offsetMax = new Vector2(-28f, 0f);
            qualityValue.text = "HIGH";

            back = CreateMenuButton(
                "SettingsBackButton",
                group.transform,
                font,
                "BACK",
                new Vector2(150f, -810f),
                false);
            return group;
        }

        private static CanvasGroup CreateCreditsPanel(
            Transform parent,
            Font font,
            out Button back)
        {
            CanvasGroup group = CreatePanel("Credits", parent);
            CreateSectionHeading(group.transform, font, "CREDITS", "DEVELOPMENT TEAM");
            RectTransform card = CreateCard(
                "CreditsCard",
                group.transform,
                new Vector2(0.69f, 0.5f),
                new Vector2(720f, 580f));

            Text studio = CreateText(
                "StudioCredit",
                card,
                font,
                34,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                TextPrimary);
            SetOffsets(studio.rectTransform, 42f, 42f, -42f, -108f);
            studio.text = "TROOPER STUDIOS";

            Text body = CreateText(
                "CreditBody",
                card,
                font,
                18,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                TextSecondary);
            SetOffsets(body.rectTransform, 42f, 130f, -42f, -420f);
            body.text = "RULES OF ENTRY\n\nDesign and direction\nTrooper Studios\n\nBuilt with Unity\n\nPrototype systems, visuals, and credits will expand as production contributors and licensed assets are added.";

            Text notice = CreateText(
                "PrototypeNotice",
                card,
                font,
                14,
                FontStyle.Normal,
                TextAnchor.LowerLeft,
                TextSecondary);
            notice.rectTransform.anchorMin = Vector2.zero;
            notice.rectTransform.anchorMax = Vector2.one;
            notice.rectTransform.offsetMin = new Vector2(42f, 34f);
            notice.rectTransform.offsetMax = new Vector2(-42f, -506f);
            notice.text = "PRE-PRODUCTION PROTOTYPE";

            back = CreateMenuButton(
                "CreditsBackButton",
                group.transform,
                font,
                "BACK",
                new Vector2(150f, -810f),
                false);
            return group;
        }

        private static CanvasGroup CreateLoadingPanel(
            Transform parent,
            Font font,
            out Text context,
            out Text destination,
            out Text detail,
            out Text status,
            out Text percentage,
            out Image fill)
        {
            CanvasGroup group = CreatePanel("Loading", parent);

            Text brand = CreateText(
                "LoadingBrand",
                group.transform,
                font,
                15,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                new Color(TextSecondary.r, TextSecondary.g, TextSecondary.b, 0.82f));
            Anchor(
                brand.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(86f, -62f),
                new Vector2(420f, 30f),
                new Vector2(0f, 1f));
            brand.text = "RULES OF ENTRY";

            context = CreateText(
                "LoadingContext",
                group.transform,
                font,
                19,
                FontStyle.Bold,
                TextAnchor.LowerLeft,
                Primary);
            Anchor(
                context.rectTransform,
                Vector2.zero,
                new Vector2(86f, 318f),
                new Vector2(760f, 34f),
                Vector2.zero);
            context.text = "MISSION";

            destination = CreateText(
                "LoadingDestination",
                group.transform,
                font,
                58,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                TextPrimary);
            Anchor(
                destination.rectTransform,
                Vector2.zero,
                new Vector2(86f, 222f),
                new Vector2(1180f, 96f),
                Vector2.zero);
            destination.verticalOverflow = VerticalWrapMode.Overflow;
            destination.text = "TRAINING OPERATION: CONTROLLED RESOLUTION";
            AddTextShadow(
                destination,
                new Color(0f, 0f, 0f, 0.88f),
                new Vector2(3f, -3f));

            detail = CreateText(
                "LoadingDetail",
                group.transform,
                font,
                18,
                FontStyle.Bold,
                TextAnchor.LowerLeft,
                TextSecondary);
            Anchor(
                detail.rectTransform,
                Vector2.zero,
                new Vector2(86f, 182f),
                new Vector2(900f, 32f),
                Vector2.zero);
            detail.text = "CALDER CITY  •  ROE PROTOTYPE";

            status = CreateText(
                "LoadingStatus",
                group.transform,
                font,
                16,
                FontStyle.Bold,
                TextAnchor.LowerLeft,
                TextSecondary);
            Anchor(
                status.rectTransform,
                Vector2.zero,
                new Vector2(86f, 132f),
                new Vector2(900f, 30f),
                Vector2.zero);
            status.text = "ESTABLISHING COMMAND LINK";

            percentage = CreateText(
                "LoadingPercentage",
                group.transform,
                font,
                17,
                FontStyle.Bold,
                TextAnchor.LowerRight,
                TextSecondary);
            Anchor(
                percentage.rectTransform,
                new Vector2(1f, 0f),
                new Vector2(-86f, 132f),
                new Vector2(300f, 30f),
                new Vector2(1f, 0f));
            percentage.text = "LOADING  0%";

            Image track = CreateImage(
                "LoadingTrack",
                group.transform,
                new Color(0.29f, 0.36f, 0.4f, 0.62f));
            Anchor(
                track.rectTransform,
                Vector2.zero,
                new Vector2(86f, 105f),
                new Vector2(1748f, 3f),
                Vector2.zero);
            track.raycastTarget = false;
            fill = CreateImage("LoadingFill", track.transform, Primary);
            Stretch(fill.rectTransform);
            fill.raycastTarget = false;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
            return group;
        }

        private static void RestylePrototypeHud()
        {
            Scene scene = EditorSceneManager.OpenScene(
                ProjectInfo.PrototypeScenePath,
                OpenSceneMode.Single);
            RemoveRoot(scene, PresentationRootName);
            Font font = LoadFont();

            RestyleInteraction(scene, font);
            RestyleWeapon(scene, font);
            CanvasGroup humanGroup = RestyleHuman(scene, font);
            RestyleOfficer(scene, font);
            RestyleMission(scene, font);
            if (humanGroup == null)
            {
                throw new InvalidOperationException(
                    $"{HumanUiRootName} is missing its CanvasGroup.");
            }

            GameObject presentationRoot = new GameObject(PresentationRootName);
            SceneManager.MoveGameObjectToScene(presentationRoot, scene);
            PrototypePresentationController presentation =
                presentationRoot.AddComponent<PrototypePresentationController>();
            GameObject hintCanvasObject = new GameObject(
                "PresentationHintCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup));
            hintCanvasObject.transform.SetParent(presentationRoot.transform, false);
            Canvas hintCanvas = hintCanvasObject.GetComponent<Canvas>();
            hintCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hintCanvas.sortingOrder = 112;
            CanvasScaler scaler = hintCanvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            CanvasGroup hintGroup = hintCanvasObject.GetComponent<CanvasGroup>();
            hintGroup.interactable = false;
            hintGroup.blocksRaycasts = false;
            Text hint = CreateText(
                "DiagnosticsHint",
                hintCanvasObject.transform,
                font,
                13,
                FontStyle.Bold,
                TextAnchor.LowerLeft,
                TextSecondary);
            hint.rectTransform.anchorMin = Vector2.zero;
            hint.rectTransform.anchorMax = Vector2.zero;
            hint.rectTransform.pivot = Vector2.zero;
            hint.rectTransform.anchoredPosition = new Vector2(26f, 24f);
            hint.rectTransform.sizeDelta = new Vector2(420f, 28f);
            hint.text = "F10  •  SYSTEM DIAGNOSTICS";
            presentation.Configure(
                new[] { humanGroup },
                hintGroup,
                hint,
                false);
            humanGroup.alpha = 0f;
            EditorUtility.SetDirty(presentation);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException(
                    $"Unity could not save {ProjectInfo.PrototypeScenePath} after the HUD restyle.");
            }
        }

        private static void RestyleInteraction(Scene scene, Font font)
        {
            GameObject root = RequireRoot(scene, InteractionUiRootName);
            RectTransform panel = RequireChild(root.transform, "PromptContainer") as RectTransform;
            StylePanel(panel, new Vector2(0f, 86f), new Vector2(520f, 72f), true);
            SetAllFonts(root, font);
            SetImageColor(root.transform, "Accent", Primary);
            SetImageColor(root.transform, "KeyBadge", SurfaceRaised);
            SetImageColor(root.transform, "HoldProgressFill", Primary);
            Text key = RequireChild(root.transform, "KeyText").GetComponent<Text>();
            key.color = Primary;
            key.fontSize = 20;
            Text prompt = RequireChild(root.transform, "PromptText").GetComponent<Text>();
            prompt.fontSize = 20;
        }

        private static void RestyleWeapon(Scene scene, Font font)
        {
            GameObject root = RequireRoot(scene, WeaponUiRootName);
            RectTransform panel = RequireChild(root.transform, "WeaponStatusPanel") as RectTransform;
            StylePanel(panel, new Vector2(-34f, 34f), new Vector2(410f, 88f), true);
            SetAllFonts(root, font);
            SetImageColor(root.transform, "OperationProgressFill", Primary);
            Text mechanical = RequireChild(root.transform, "MechanicalState").GetComponent<Text>();
            mechanical.color = Signal;
            mechanical.fontSize = 16;
            Text status = RequireChild(root.transform, "StatusMessage").GetComponent<Text>();
            status.fontSize = 18;
        }

        private static CanvasGroup RestyleHuman(Scene scene, Font font)
        {
            GameObject root = RequireRoot(scene, HumanUiRootName);
            RectTransform panel = RequireChild(root.transform, "AIDiagnosticsPanel") as RectTransform;
            panel.anchorMin = panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            StylePanel(panel, new Vector2(30f, -350f), new Vector2(640f, 156f), false);
            SetAllFonts(root, font);
            Text command = RequireChild(root.transform, "CommandStatus").GetComponent<Text>();
            command.color = Primary;
            command.fontSize = 16;
            Text actors = RequireChild(root.transform, "ActorStates").GetComponent<Text>();
            actors.fontSize = 15;
            return root.GetComponent<CanvasGroup>();
        }

        private static void RestyleOfficer(Scene scene, Font font)
        {
            GameObject root = RequireRoot(scene, OfficerUiRootName);
            RectTransform panel = RequireChild(root.transform, "OfficerCommandPanel") as RectTransform;
            StylePanel(panel, new Vector2(-30f, -30f), new Vector2(610f, 236f), true);
            SetAllFonts(root, font);
            Text command = RequireChild(root.transform, "CommandSummary").GetComponent<Text>();
            command.color = Signal;
            command.fontSize = 16;
            Text officers = RequireChild(root.transform, "OfficerStates").GetComponent<Text>();
            officers.fontSize = 15;
        }

        private static void RestyleMission(Scene scene, Font font)
        {
            GameObject root = RequireRoot(scene, MissionUiRootName);
            RectTransform panel = RequireChild(root.transform, "MissionPanel") as RectTransform;
            StylePanel(panel, new Vector2(30f, -30f), new Vector2(550f, 286f), true);
            SetAllFonts(root, font);
            Text report = RequireChild(root.transform, "ReportText").GetComponent<Text>();
            report.fontSize = 16;
            report.color = TextPrimary;
            report.lineSpacing = 1.08f;
        }

        private static void StylePanel(
            RectTransform panel,
            Vector2 anchoredPosition,
            Vector2 size,
            bool primaryChrome)
        {
            if (panel == null)
            {
                throw new InvalidOperationException("A required HUD panel RectTransform is missing.");
            }

            panel.anchoredPosition = anchoredPosition;
            panel.sizeDelta = size;
            Image image = panel.GetComponent<Image>();
            if (image != null)
            {
                image.color = Surface;
                AddOutline(image, Border, new Vector2(1f, -1f));
            }

            Transform existingChrome = panel.Find("_PresentationChrome");
            if (existingChrome != null)
            {
                UnityEngine.Object.DestroyImmediate(existingChrome.gameObject);
            }

            GameObject chrome = new GameObject("_PresentationChrome", typeof(RectTransform));
            chrome.transform.SetParent(panel, false);
            RectTransform chromeRect = chrome.GetComponent<RectTransform>();
            Stretch(chromeRect);
            Image top = CreateImage(
                "TopSignal",
                chrome.transform,
                primaryChrome ? Primary : Signal);
            top.rectTransform.anchorMin = new Vector2(0f, 1f);
            top.rectTransform.anchorMax = Vector2.one;
            top.rectTransform.offsetMin = new Vector2(0f, -3f);
            top.rectTransform.offsetMax = Vector2.zero;
            top.raycastTarget = false;

            Image corner = CreateImage(
                "CornerSignal",
                chrome.transform,
                primaryChrome ? Primary : Signal);
            corner.rectTransform.anchorMin = new Vector2(0f, 1f);
            corner.rectTransform.anchorMax = new Vector2(0f, 1f);
            corner.rectTransform.pivot = new Vector2(0f, 1f);
            corner.rectTransform.anchoredPosition = Vector2.zero;
            corner.rectTransform.sizeDelta = new Vector2(7f, 28f);
            corner.raycastTarget = false;
        }

        private static void ConfigurePlayerPresentation()
        {
            PlayerSettings.companyName = ProjectInfo.StudioName;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        }

        private static void ConfigureBrandingTextures()
        {
            ConfigureBrandingTexture(SplashArtworkPath);
            ConfigureBrandingTexture(WarningArtworkPath);
            ConfigureBrandingTexture(MenuArtworkPath);
        }

        private static void ConfigureBrandingTexture(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path)
                as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Required UI artwork could not be imported: {path}");
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

        private static void ConfigureBuildScenes()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(FrontEndScenePath, true),
                new EditorBuildSettingsScene(ProjectInfo.PrototypeScenePath, true)
            };
            scenes.AddRange(EditorBuildSettings.scenes.Where(scene =>
                !string.Equals(scene.path, FrontEndScenePath, StringComparison.Ordinal)
                && !string.Equals(
                    scene.path,
                    ProjectInfo.PrototypeScenePath,
                    StringComparison.Ordinal)));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static CanvasGroup CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasGroup));
            panel.transform.SetParent(parent, false);
            Stretch(panel.GetComponent<RectTransform>());
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            return group;
        }

        private static void CreateStackedGameTitle(Transform parent, Font font)
        {
            Text rules = CreateText(
                "TitleRules",
                parent,
                font,
                98,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                TextPrimary);
            Anchor(
                rules.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(86f, -54f),
                new Vector2(410f, 150f),
                new Vector2(0f, 1f));
            rules.verticalOverflow = VerticalWrapMode.Overflow;
            rules.text = "RULES";
            AddTextShadow(
                rules,
                new Color(0f, 0f, 0f, 0.88f),
                new Vector2(3f, -3f));

            Image leftRule = CreateImage(
                "TitleRuleLeft",
                parent,
                new Color(Primary.r, Primary.g, Primary.b, 0.9f));
            Anchor(
                leftRule.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(86f, -207f),
                new Vector2(116f, 2f),
                new Vector2(0f, 0.5f));
            leftRule.raycastTarget = false;

            Text connector = CreateText(
                "TitleOf",
                parent,
                font,
                28,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Primary);
            Anchor(
                connector.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(212f, -188f),
                new Vector2(64f, 40f),
                new Vector2(0f, 1f));
            connector.text = "OF";

            Image rightRule = CreateImage(
                "TitleRuleRight",
                parent,
                new Color(Primary.r, Primary.g, Primary.b, 0.9f));
            Anchor(
                rightRule.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(286f, -207f),
                new Vector2(116f, 2f),
                new Vector2(0f, 0.5f));
            rightRule.raycastTarget = false;

            Text entry = CreateText(
                "TitleEntry",
                parent,
                font,
                98,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                TextPrimary);
            Anchor(
                entry.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(86f, -230f),
                new Vector2(450f, 150f),
                new Vector2(0f, 1f));
            entry.verticalOverflow = VerticalWrapMode.Overflow;
            entry.text = "ENTRY";
            AddTextShadow(
                entry,
                new Color(0f, 0f, 0f, 0.88f),
                new Vector2(3f, -3f));
        }

        private static Button CreateMainMenuItem(
            string name,
            Transform parent,
            Font font,
            string label,
            Vector2 position,
            bool interactable)
        {
            Image hitArea = CreateImage(name, parent, Color.clear);
            Anchor(
                hitArea.rectTransform,
                new Vector2(0f, 1f),
                position,
                new Vector2(470f, 52f),
                new Vector2(0f, 1f));

            Button button = hitArea.gameObject.AddComponent<Button>();
            button.targetGraphic = hitArea;
            button.transition = Selectable.Transition.None;
            button.navigation = new UnityEngine.UI.Navigation
            {
                mode = UnityEngine.UI.Navigation.Mode.Automatic
            };
            button.interactable = interactable;

            Image selectionBar = CreateImage(
                "SelectionBar",
                hitArea.transform,
                new Color(Primary.r, Primary.g, Primary.b, 0f));
            Anchor(
                selectionBar.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0f),
                new Vector2(3f, 30f),
                new Vector2(0f, 0.5f));
            selectionBar.raycastTarget = false;

            Text text = CreateText(
                "Label",
                hitArea.transform,
                font,
                21,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                TextPrimary);
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(8f, 1f);
            text.rectTransform.offsetMax = new Vector2(-18f, -1f);
            text.text = label;

            Image divider = CreateImage(
                "Divider",
                hitArea.transform,
                new Color(0.48f, 0.57f, 0.62f, 0.14f));
            divider.rectTransform.anchorMin = Vector2.zero;
            divider.rectTransform.anchorMax = new Vector2(0f, 0f);
            divider.rectTransform.pivot = Vector2.zero;
            divider.rectTransform.anchoredPosition = Vector2.zero;
            divider.rectTransform.sizeDelta = new Vector2(430f, 1f);
            divider.raycastTarget = false;

            FrontEndMenuItemVisual visual =
                hitArea.gameObject.AddComponent<FrontEndMenuItemVisual>();
            visual.Configure(button, text, selectionBar, divider);
            return button;
        }

        private static void CreateSectionHeading(
            Transform parent,
            Font font,
            string title,
            string subtitle)
        {
            Text small = CreateText(
                "SectionSubtitle",
                parent,
                font,
                15,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Primary);
            Anchor(small.rectTransform, new Vector2(0f, 1f), new Vector2(150f, -96f), new Vector2(500f, 30f), new Vector2(0f, 1f));
            small.text = subtitle;

            Text heading = CreateText(
                "SectionTitle",
                parent,
                font,
                48,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                TextPrimary);
            Anchor(heading.rectTransform, new Vector2(0f, 1f), new Vector2(150f, -148f), new Vector2(650f, 64f), new Vector2(0f, 1f));
            heading.text = title;
        }

        private static RectTransform CreateCard(
            string name,
            Transform parent,
            Vector2 anchor,
            Vector2 size)
        {
            Image card = CreateImage(name, parent, Surface);
            Anchor(card.rectTransform, anchor, Vector2.zero, size);
            AddOutline(card, Border, new Vector2(1f, -1f));
            Image top = CreateImage("CardSignal", card.transform, Primary);
            top.rectTransform.anchorMin = new Vector2(0f, 1f);
            top.rectTransform.anchorMax = Vector2.one;
            top.rectTransform.offsetMin = new Vector2(0f, -4f);
            top.rectTransform.offsetMax = Vector2.zero;
            return card.rectTransform;
        }

        private static Button CreateMenuButton(
            string name,
            Transform parent,
            Font font,
            string label,
            Vector2 position,
            bool primary,
            Vector2? size = null)
        {
            Image background = CreateImage(name, parent, SurfaceRaised);
            Anchor(
                background.rectTransform,
                new Vector2(0f, 1f),
                position,
                size ?? new Vector2(500f, 68f),
                new Vector2(0f, 1f));
            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.navigation = new UnityEngine.UI.Navigation
            {
                mode = UnityEngine.UI.Navigation.Mode.Automatic
            };

            Image accent = CreateImage(
                "Accent",
                background.transform,
                primary ? Primary : Signal);
            accent.rectTransform.anchorMin = Vector2.zero;
            accent.rectTransform.anchorMax = new Vector2(0f, 1f);
            accent.rectTransform.offsetMin = Vector2.zero;
            accent.rectTransform.offsetMax = new Vector2(5f, 0f);

            Text text = CreateText(
                "Label",
                background.transform,
                font,
                19,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                TextPrimary);
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(28f, 0f);
            text.rectTransform.offsetMax = new Vector2(-20f, 0f);
            text.text = label;

            FrontEndButtonVisual visual =
                background.gameObject.AddComponent<FrontEndButtonVisual>();
            visual.Configure(button, background, accent, text);
            return button;
        }

        private static Slider CreateSlider(
            string name,
            Transform parent,
            Font font,
            string label,
            Vector2 position)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = position;
            rootRect.sizeDelta = new Vector2(736f, 70f);

            Text labelText = CreateText(
                "Label",
                root.transform,
                font,
                17,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                TextPrimary);
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.offsetMin = new Vector2(0f, 36f);
            labelText.rectTransform.offsetMax = Vector2.zero;
            labelText.text = label;

            Image track = CreateImage(
                "Track",
                root.transform,
                new Color(0.1f, 0.14f, 0.17f, 1f));
            track.rectTransform.anchorMin = new Vector2(0f, 0f);
            track.rectTransform.anchorMax = new Vector2(1f, 0f);
            track.rectTransform.offsetMin = new Vector2(0f, 8f);
            track.rectTransform.offsetMax = new Vector2(0f, 14f);

            Image fill = CreateImage("Fill", track.transform, Primary);
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image handle = CreateImage("Handle", root.transform, TextPrimary);
            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(0f, 0f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = new Vector2(0f, 11f);
            handleRect.sizeDelta = new Vector2(18f, 26f);

            Slider slider = root.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.direction = Slider.Direction.LeftToRight;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            return slider;
        }

        private static Toggle CreateToggle(
            string name,
            Transform parent,
            Font font,
            string label,
            Vector2 position)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(736f, 58f);

            Image box = CreateImage("Box", root.transform, SurfaceRaised);
            box.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            box.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            box.rectTransform.pivot = new Vector2(0f, 0.5f);
            box.rectTransform.anchoredPosition = Vector2.zero;
            box.rectTransform.sizeDelta = new Vector2(38f, 38f);
            AddOutline(box, Border, new Vector2(1f, -1f));

            Image check = CreateImage("Check", box.transform, Primary);
            Stretch(check.rectTransform);
            check.rectTransform.offsetMin = new Vector2(8f, 8f);
            check.rectTransform.offsetMax = new Vector2(-8f, -8f);

            Text text = CreateText(
                "Label",
                root.transform,
                font,
                17,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                TextPrimary);
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(58f, 0f);
            text.text = label;

            Toggle toggle = root.GetComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.transition = Selectable.Transition.ColorTint;
            toggle.isOn = true;
            return toggle;
        }

        private static Text CreateBuildLabel(Transform parent, Font font)
        {
            Text label = CreateText(
                "BuildLabel",
                parent,
                font,
                12,
                FontStyle.Normal,
                TextAnchor.LowerRight,
                new Color(TextSecondary.r, TextSecondary.g, TextSecondary.b, 0.72f));
            label.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(0f, 16f);
            label.rectTransform.offsetMax = new Vector2(-22f, -1038f);
            label.text = "PROTOTYPE";
            return label;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Image));
            result.transform.SetParent(parent, false);
            Image image = result.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Image CreateFullScreenArtwork(
            string name,
            Transform parent,
            Sprite sprite)
        {
            if (sprite == null)
            {
                throw new ArgumentNullException(nameof(sprite));
            }

            Image image = CreateImage(name, parent, Color.white);
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            Stretch(image.rectTransform);
            AspectRatioFitter fitter = image.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
            return image;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Font font,
            int fontSize,
            FontStyle style,
            TextAnchor alignment,
            Color color)
        {
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Text));
            result.transform.SetParent(parent, false);
            Text text = result.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static void SetAllFonts(GameObject root, Font font)
        {
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                text.font = font;
            }
        }

        private static void SetImageColor(Transform root, string name, Color color)
        {
            Image image = RequireChild(root, name).GetComponent<Image>();
            if (image == null)
            {
                throw new InvalidOperationException($"{name} has no Image component.");
            }

            image.color = color;
        }

        private static void AddOutline(Image image, Color color, Vector2 distance)
        {
            if (image == null)
            {
                return;
            }

            Outline outline = image.GetComponent<Outline>();
            if (outline == null)
            {
                outline = image.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static void AddTextShadow(Text text, Color color, Vector2 distance)
        {
            Shadow shadow = text.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = text.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
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
            Vector2? pivot = null)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
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

        private static Font LoadFont()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(TypographyPath);
            if (font == null)
            {
                throw new InvalidOperationException(
                    $"Required UI typeface could not be loaded: {TypographyPath}");
            }

            return font;
        }

        private static Sprite RequireSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new InvalidOperationException(
                    $"Required UI sprite is missing or not imported as a Sprite: {path}");
            }

            return sprite;
        }

        private static GameObject RequireRoot(Scene scene, string name)
        {
            GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate =>
                string.Equals(candidate.name, name, StringComparison.Ordinal));
            if (root == null)
            {
                throw new InvalidOperationException(
                    $"Required UI root {name} is missing from {scene.path}.");
            }

            return root;
        }

        private static Transform RequireChild(Transform root, string name)
        {
            Transform child = root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(candidate => string.Equals(
                    candidate.name,
                    name,
                    StringComparison.Ordinal));
            if (child == null)
            {
                throw new InvalidOperationException(
                    $"Required UI child {name} is missing below {root.name}.");
            }

            return child;
        }

        private static void RemoveRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, name, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
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
