using System;
using System.IO;
using System.Linq;
using System.Text;
using RulesOfEntry.Core;
using RulesOfEntry.Input;
using RulesOfEntry.Officers;
using RulesOfEntry.UI;
using RulesOfEntry.UI.TacticalHud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.TacticalHud
{
    public static class RulesOfEntryTacticalHudSetup
    {
        internal const string InputAssetPath =
            "Assets/_Project/RulesOfEntry/Input/ROE_InputActions.inputactions";
        internal const string PlayerPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_Player.prefab";
        internal const string OfficerAlphaPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_OfficerAlpha.prefab";
        internal const string OfficerBravoPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_OfficerBravo.prefab";
        internal const string HudPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_TacticalHUD.prefab";
        internal const string HudRootName = "[ROE_TacticalHUD]";

        private const string UiPrefabFolder = "Assets/_Project/RulesOfEntry/Prefabs/UI";
        private const string FontPath =
            "Assets/_Project/RulesOfEntry/Art/UI/Fonts/LatinModernSansDemiCondensed.otf";

        private static readonly Color PanelColor = new Color(0.008f, 0.012f, 0.016f, 0.9f);
        private static readonly Color RowColor = new Color(0.016f, 0.022f, 0.027f, 0.93f);
        private static readonly Color Blue = new Color(0.03f, 0.63f, 1f, 1f);
        private static readonly Color Muted = new Color(0.62f, 0.68f, 0.71f, 1f);
        private static readonly Color Border = new Color(0.55f, 0.62f, 0.66f, 0.42f);

        [MenuItem("Rules of Entry/Milestone 6B/Build Tactical HUD", priority = 610)]
        public static void BuildTacticalHud()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building the Tactical HUD.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("Tactical HUD", "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                EnsureFolder(UiPrefabFolder);
                InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                    InputAssetPath);
                if (inputAsset == null)
                {
                    throw new InvalidOperationException(
                        $"Input actions are missing at {InputAssetPath}.");
                }

                inputAsset = EnsureTacticalHudInput(inputAsset);
                UpdatePlayerPrefab(inputAsset);
                UpdateOfficerPrefab(OfficerAlphaPrefabPath);
                UpdateOfficerPrefab(OfficerBravoPrefabPath);
                GameObject hudPrefab = CreateHudPrefab();
                TacticalHudController sceneHud = InstallInPrototypeScene(
                    hudPrefab,
                    inputAsset);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeGameObject = sceneHud.gameObject;
                ProjectLog.Info(
                    "Tactical HUD",
                    "Scalable squad roster, RoE body-camera overlay, and MMB command interface created. Running validation now.");
                RulesOfEntryTacticalHudValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Tactical HUD", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Tactical HUD setup stopped. Check the first Console error for the root cause.",
                    "OK");
            }
        }

        private static InputActionAsset EnsureTacticalHudInput(InputActionAsset inputAsset)
        {
            inputAsset.Disable();
            InputActionMap playerMap = inputAsset.FindActionMap("Player", true);
            EnsureButton(
                playerMap,
                "OfficerCommandMenu",
                "<Mouse>/middleButton",
                "Keyboard&Mouse");
            EnsureButton(playerMap, "OfficerMove", "<Keyboard>/digit1", "Keyboard&Mouse");
            EnsureButton(playerMap, "OfficerHold", "<Keyboard>/digit2", "Keyboard&Mouse");
            EnsureButton(playerMap, "OfficerStack", "<Keyboard>/digit3", "Keyboard&Mouse");
            EnsureButton(playerMap, "OfficerOpen", "<Keyboard>/digit4", "Keyboard&Mouse");
            EnsureButton(playerMap, "OfficerFollow", "<Keyboard>/digit5", "Keyboard&Mouse");
            EnsureButton(
                playerMap,
                "OfficerRestrain",
                "<Keyboard>/digit6",
                "Keyboard&Mouse");

            string absolutePath = Path.GetFullPath(InputAssetPath);
            File.WriteAllText(absolutePath, inputAsset.ToJson(), new UTF8Encoding(false));
            AssetDatabase.ImportAsset(
                InputAssetPath,
                ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            InputActionAsset reloaded = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                InputAssetPath);
            if (reloaded?.FindAction("Player/OfficerCommandMenu", false) == null)
            {
                throw new InvalidOperationException(
                    "Tactical HUD input actions were written but could not be reloaded.");
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

        private static void UpdatePlayerPrefab(InputActionAsset inputAsset)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                PlayerInput playerInput = root.GetComponent<PlayerInput>();
                TacticalPlayerInput tacticalInput = root.GetComponent<TacticalPlayerInput>();
                OfficerSquadController squad = root.GetComponent<OfficerSquadController>();
                if (playerInput == null || tacticalInput == null || squad == null)
                {
                    throw new InvalidOperationException(
                        "Player prefab is missing PlayerInput, TacticalPlayerInput, or OfficerSquadController. Complete Milestone 4 first.");
                }

                playerInput.actions = inputAsset;
                tacticalInput.Configure(playerInput);
                BodyCameraIdentity identity = root.GetComponent<BodyCameraIdentity>()
                    ?? root.AddComponent<BodyCameraIdentity>();
                identity.Configure(
                    "A. Carter",
                    "A127",
                    "Calder City Police Department");
                identity.SetRecordingState(true, 97);
                MissionClock clock = root.GetComponent<MissionClock>()
                    ?? root.AddComponent<MissionClock>();
                clock.Configure(new DateTime(2026, 7, 17, 22, 41, 0), 1f);
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void UpdateOfficerPrefab(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (root.GetComponent<TacticalOfficerController>() == null)
                {
                    throw new InvalidOperationException(
                        $"{prefabPath} is not a configured tactical officer prefab.");
                }

                OfficerAmmunitionStatus ammunition =
                    root.GetComponent<OfficerAmmunitionStatus>()
                    ?? root.AddComponent<OfficerAmmunitionStatus>();
                ammunition.Configure(4, 1);
                root.GetComponent<OfficerVisual>()?.SetWorldStatusVisible(false);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static GameObject CreateHudPrefab()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath)
                ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                throw new InvalidOperationException("No compatible Tactical HUD font was found.");
            }

            GameObject root = new GameObject(
                HudRootName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(CanvasGroup),
                typeof(TacticalHudController));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            CanvasGroup rootGroup = root.GetComponent<CanvasGroup>();
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;

            RectTransform rosterPanel;
            RectTransform rosterContent;
            TacticalHudOfficerRow rowTemplate;
            CreateRoster(
                root.transform,
                font,
                out rosterPanel,
                out rosterContent,
                out rowTemplate);

            Graphic recordingDot;
            Text recordingHeader;
            Text officerIdentity;
            Text department;
            Text timestamp;
            Text battery;
            Text live;
            CreateBodyCamera(
                root.transform,
                font,
                out recordingDot,
                out recordingHeader,
                out officerIdentity,
                out department,
                out timestamp,
                out battery,
                out live);

            CanvasGroup commandGroup;
            Text commandTitle;
            Image[] commandBackgrounds;
            Text context;
            Text commandFooter;
            CreateCommandInterface(
                root.transform,
                font,
                out commandGroup,
                out commandTitle,
                out commandBackgrounds,
                out context,
                out commandFooter);

            Text hint = CreateText(
                "CommandHint",
                root.transform,
                font,
                19,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Blue);
            SetAnchored(
                hint.rectTransform,
                Vector2.zero,
                Vector2.zero,
                new Vector2(32f, 30f),
                new Vector2(260f, 36f));
            hint.text = "MMB  COMMANDS";

            root.GetComponent<TacticalHudController>().Configure(
                null,
                null,
                null,
                null,
                rosterPanel,
                rosterContent,
                rowTemplate,
                recordingDot,
                recordingHeader,
                officerIdentity,
                department,
                timestamp,
                battery,
                live,
                commandGroup,
                commandTitle,
                commandBackgrounds,
                context,
                commandFooter);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Could not save {HudPrefabPath}.");
            }

            return prefab;
        }

        private static void CreateRoster(
            Transform parent,
            Font font,
            out RectTransform rosterPanel,
            out RectTransform rosterContent,
            out TacticalHudOfficerRow rowTemplate)
        {
            Color lowProfilePanel = new Color(0.006f, 0.012f, 0.016f, 0.1f);
            Color lowProfileRow = Color.clear;
            GameObject panel = CreatePanel(
                "SquadStatusPanel",
                parent,
                lowProfilePanel,
                false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            rosterPanel = panelRect;
            SetAnchored(
                panelRect,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(32f, -32f),
                new Vector2(368f, 132f));

            Text title = CreateText(
                "Title",
                panel.transform,
                font,
                11,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Muted);
            SetAnchored(
                title.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(10f, -4f),
                new Vector2(340f, 25f));
            title.text = "ELEMENT 01  //  LIVE STATUS";
            AddTextShadow(title);

            CreateDivider(panel.transform, new Vector2(8f, -30f), new Vector2(352f, 1f));

            GameObject viewport = new GameObject(
                "RosterViewport",
                typeof(RectTransform),
                typeof(Image),
                typeof(RectMask2D));
            viewport.transform.SetParent(panel.transform, false);
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = false;
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(6f, 4f);
            viewportRect.offsetMax = new Vector2(-6f, -34f);

            GameObject content = new GameObject(
                "RosterContent",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            rosterContent = content.GetComponent<RectTransform>();
            rosterContent.anchorMin = new Vector2(0f, 1f);
            rosterContent.anchorMax = new Vector2(1f, 1f);
            rosterContent.pivot = new Vector2(0.5f, 1f);
            rosterContent.anchoredPosition = Vector2.zero;
            rosterContent.sizeDelta = Vector2.zero;
            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0f;
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject row = new GameObject(
                "OfficerRowTemplate",
                typeof(RectTransform),
                typeof(Image),
                typeof(LayoutElement),
                typeof(TacticalHudOfficerRow));
            row.transform.SetParent(content.transform, false);
            row.GetComponent<Image>().color = lowProfileRow;
            row.GetComponent<Image>().raycastTarget = false;
            LayoutElement rowLayout = row.GetComponent<LayoutElement>();
            rowLayout.preferredHeight = 46f;
            rowLayout.minHeight = 46f;

            Image selectedEdge = CreatePanel(
                "SelectionEdge",
                row.transform,
                Blue,
                false).GetComponent<Image>();
            SetAnchored(
                selectedEdge.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                Vector2.zero,
                new Vector2(2f, 36f));

            Text nameText = CreateText(
                "OfficerName",
                row.transform,
                font,
                15,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Color.white);
            nameText.resizeTextForBestFit = true;
            nameText.resizeTextMinSize = 11;
            nameText.resizeTextMaxSize = 15;
            AddTextShadow(nameText);
            SetAnchored(
                nameText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(10f, -2f),
                new Vector2(190f, 23f));

            Text conditionDot = CreateText(
                "ConditionDot",
                row.transform,
                font,
                13,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white);
            SetAnchored(
                conditionDot.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(208f, -3f),
                new Vector2(16f, 22f));
            conditionDot.text = "●";
            AddTextShadow(conditionDot);

            Text healthText = CreateText(
                "OfficerHealth",
                row.transform,
                font,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Color.white);
            AddTextShadow(healthText);
            SetAnchored(
                healthText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(225f, -2f),
                new Vector2(125f, 23f));

            Text activityText = CreateText(
                "OfficerActivity",
                row.transform,
                font,
                11,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                Blue);
            AddTextShadow(activityText);
            SetAnchored(
                activityText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(10f, -24f),
                new Vector2(205f, 18f));

            Text ammunitionText = CreateText(
                "OfficerAmmunition",
                row.transform,
                font,
                11,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                Color.white);
            AddTextShadow(ammunitionText);
            SetAnchored(
                ammunitionText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(225f, -24f),
                new Vector2(125f, 18f));

            rowTemplate = row.GetComponent<TacticalHudOfficerRow>();
            rowTemplate.ConfigureVisuals(
                selectedEdge,
                nameText,
                activityText,
                conditionDot,
                healthText,
                ammunitionText);
            row.SetActive(false);
        }

        private static void CreateBodyCamera(
            Transform parent,
            Font font,
            out Graphic recordingDot,
            out Text recordingHeader,
            out Text officerIdentity,
            out Text department,
            out Text timestamp,
            out Text battery,
            out Text live)
        {
            GameObject panel = new GameObject(
                "BodyCameraPanel",
                typeof(RectTransform),
                typeof(TacticalHudRoundedPanelGraphic));
            panel.transform.SetParent(parent, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            SetAnchored(
                panelRect,
                Vector2.one,
                Vector2.one,
                new Vector2(-32f, -32f),
                new Vector2(455f, 170f));
            panel.GetComponent<TacticalHudRoundedPanelGraphic>().Configure(
                new Color(0.025f, 0.035f, 0.042f, 0.86f),
                new Color(0.64f, 0.7f, 0.73f, 0.58f),
                13f,
                1.35f);

            Text recordingDotText = CreateText(
                "RecordingDot",
                panel.transform,
                font,
                19,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.9f, 0.1f, 0.09f, 1f));
            SetAnchored(
                recordingDotText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(64f, -11f),
                new Vector2(18f, 25f));
            recordingDotText.text = "●";
            recordingDot = recordingDotText;

            recordingHeader = CreateText(
                "RecordingHeader",
                panel.transform,
                font,
                18,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                Color.white);
            SetAnchored(
                recordingHeader.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(20f, -8f),
                new Vector2(310f, 28f));
            recordingHeader.text = "REC      ROE BODY CAM";

            officerIdentity = CreateText(
                "OfficerIdentity",
                panel.transform,
                font,
                16,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                Color.white);
            SetAnchored(
                officerIdentity.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(20f, -42f),
                new Vector2(315f, 24f));

            department = CreateText(
                "Department",
                panel.transform,
                font,
                15,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                Muted);
            SetAnchored(
                department.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(20f, -73f),
                new Vector2(320f, 24f));

            timestamp = CreateText(
                "MissionTimestamp",
                panel.transform,
                font,
                16,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                Color.white);
            SetAnchored(
                timestamp.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(20f, -106f),
                new Vector2(320f, 28f));

            GameObject divider = CreatePanel(
                "BodyCameraDivider",
                panel.transform,
                new Color(0.64f, 0.7f, 0.73f, 0.32f),
                false);
            SetAnchored(
                divider.GetComponent<RectTransform>(),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(348f, -10f),
                new Vector2(1f, 150f));

            GameObject shieldObject = new GameObject(
                "RoEShield",
                typeof(RectTransform),
                typeof(TacticalHudShieldGraphic));
            shieldObject.transform.SetParent(panel.transform, false);
            RectTransform shieldRect = shieldObject.GetComponent<RectTransform>();
            SetAnchored(
                shieldRect,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-17f, -9f),
                new Vector2(78f, 72f));
            shieldObject.GetComponent<TacticalHudShieldGraphic>().Configure(
                new Color(0.92f, 0.96f, 0.98f, 0.95f),
                2f);
            Text logo = CreateText(
                "RoELogo",
                shieldObject.transform,
                font,
                24,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white);
            Stretch(logo.rectTransform, 4f, 10f, 4f, 6f);
            logo.text = "RoE";

            CreateBatteryIcon(panel.transform);

            battery = CreateText(
                "Battery",
                panel.transform,
                font,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white);
            SetAnchored(
                battery.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-14f, -93f),
                new Vector2(43f, 22f));

            CreateCameraIcon(panel.transform);

            live = CreateText(
                "Live",
                panel.transform,
                font,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white);
            SetAnchored(
                live.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-14f, -126f),
                new Vector2(48f, 22f));
        }

        private static void CreateBatteryIcon(Transform parent)
        {
            GameObject root = new GameObject("BatteryIcon", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            SetAnchored(
                root.GetComponent<RectTransform>(),
                Vector2.one,
                Vector2.one,
                new Vector2(-67f, -96f),
                new Vector2(30f, 16f));
            CreateIconRect(root.transform, "Top", new Vector2(2f, -1f), new Vector2(23f, 2f));
            CreateIconRect(root.transform, "Bottom", new Vector2(2f, -13f), new Vector2(23f, 2f));
            CreateIconRect(root.transform, "Left", new Vector2(2f, -1f), new Vector2(2f, 14f));
            CreateIconRect(root.transform, "Right", new Vector2(23f, -1f), new Vector2(2f, 14f));
            CreateIconRect(root.transform, "Terminal", new Vector2(26f, -5f), new Vector2(3f, 6f));
            CreateIconRect(
                root.transform,
                "Charge",
                new Vector2(6f, -5f),
                new Vector2(15f, 6f),
                new Color(0.92f, 0.96f, 0.98f, 0.8f));
        }

        private static void CreateCameraIcon(Transform parent)
        {
            GameObject root = new GameObject("CameraIcon", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            SetAnchored(
                root.GetComponent<RectTransform>(),
                Vector2.one,
                Vector2.one,
                new Vector2(-67f, -129f),
                new Vector2(31f, 18f));
            CreateIconRect(root.transform, "CameraBody", new Vector2(1f, -3f), new Vector2(22f, 14f));
            CreateIconRect(root.transform, "CameraLens", new Vector2(24f, -6f), new Vector2(6f, 8f));
            CreateIconRect(root.transform, "CameraTop", new Vector2(5f, 0f), new Vector2(8f, 3f));
        }

        private static void CreateIconRect(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color? color = null)
        {
            GameObject part = CreatePanel(
                name,
                parent,
                color ?? new Color(0.92f, 0.96f, 0.98f, 0.95f),
                false);
            RectTransform rect = part.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void CreateCommandInterface(
            Transform parent,
            Font font,
            out CanvasGroup commandGroup,
            out Text commandTitle,
            out Image[] commandBackgrounds,
            out Text context,
            out Text footer)
        {
            GameObject panel = CreatePanel("TeamCommandPanel", parent, PanelColor, true);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            SetAnchored(
                panelRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -210f),
                new Vector2(650f, 290f));
            commandGroup = panel.AddComponent<CanvasGroup>();
            commandGroup.alpha = 0f;
            commandGroup.interactable = false;
            commandGroup.blocksRaycasts = false;

            commandTitle = CreateText(
                "CommandTitle",
                panel.transform,
                font,
                24,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white);
            SetAnchored(
                commandTitle.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -8f),
                new Vector2(610f, 38f));
            commandTitle.text = "TEAM COMMAND";
            CreateDivider(panel.transform, new Vector2(20f, -50f), new Vector2(610f, 1f));

            string[] labels =
            {
                "[1]  MOVE",
                "[2]  HOLD",
                "[3]  STACK",
                "[4]  OPEN / CLEAR",
                "[5]  FOLLOW",
                "[6]  RESTRAIN"
            };
            commandBackgrounds = new Image[labels.Length];
            for (int index = 0; index < labels.Length; index++)
            {
                int column = index >= 3 ? 1 : 0;
                int row = index >= 3 ? index - 3 : index;
                float x = column == 0 ? -158f : 158f;
                float y = 77f - row * 60f;
                GameObject command = CreatePanel(
                    $"Command_{index + 1}",
                    panel.transform,
                    RowColor,
                    true);
                SetAnchored(
                    command.GetComponent<RectTransform>(),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(x, y),
                    new Vector2(304f, 52f));
                commandBackgrounds[index] = command.GetComponent<Image>();
                Text label = CreateText(
                    "Label",
                    command.transform,
                    font,
                    22,
                    FontStyle.Normal,
                    TextAnchor.MiddleLeft,
                    Color.white);
                Stretch(label.rectTransform, 16f, 6f, 10f, 6f);
                label.text = labels[index];
            }

            footer = CreateText(
                "CommandFooter",
                panel.transform,
                font,
                15,
                FontStyle.Normal,
                TextAnchor.MiddleCenter,
                Muted);
            SetAnchored(
                footer.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 6f),
                new Vector2(620f, 30f));
            footer.text = "HOLD MMB  •  NUMBER KEY TO ISSUE  •  RELEASE TO CANCEL";

            context = CreateText(
                "CommandContext",
                panel.transform,
                font,
                20,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Color.white);
            SetAnchored(
                context.rectTransform,
                new Vector2(1f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(28f, 0f),
                new Vector2(290f, 100f));
            context.text = "FOCUS: NO VALID TARGET";
        }

        private static TacticalHudController InstallInPrototypeScene(
            GameObject hudPrefab,
            InputActionAsset inputAsset)
        {
            Scene scene = EditorSceneManager.OpenScene(
                ProjectInfo.PrototypeScenePath,
                OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);

            foreach (GameObject previous in scene.GetRootGameObjects().Where(root =>
                string.Equals(root.name, HudRootName, StringComparison.Ordinal)).ToArray())
            {
                UnityEngine.Object.DestroyImmediate(previous);
            }

            GameObject player = scene.GetRootGameObjects().FirstOrDefault(root =>
                string.Equals(root.name, "ROE_Player", StringComparison.Ordinal));
            if (player == null)
            {
                throw new InvalidOperationException(
                    "ROE_Player is missing from the prototype scene.");
            }

            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            TacticalPlayerInput tacticalInput = player.GetComponent<TacticalPlayerInput>();
            OfficerSquadController squad = player.GetComponent<OfficerSquadController>();
            if (playerInput == null || tacticalInput == null || squad == null)
            {
                throw new InvalidOperationException(
                    "Prototype player is missing the Milestone 4 command components.");
            }

            playerInput.actions = inputAsset;
            tacticalInput.Configure(playerInput);
            BodyCameraIdentity identity = player.GetComponent<BodyCameraIdentity>()
                ?? player.AddComponent<BodyCameraIdentity>();
            identity.Configure("A. Carter", "A127", "Calder City Police Department");
            identity.SetRecordingState(true, 97);
            MissionClock clock = player.GetComponent<MissionClock>()
                ?? player.AddComponent<MissionClock>();
            clock.Configure(new DateTime(2026, 7, 17, 22, 41, 0), 1f);

            foreach (TacticalOfficerController officer in squad.Officers)
            {
                if (officer == null)
                {
                    continue;
                }

                OfficerAmmunitionStatus ammunition =
                    officer.GetComponent<OfficerAmmunitionStatus>()
                    ?? officer.gameObject.AddComponent<OfficerAmmunitionStatus>();
                ammunition.Configure(4, 1);
            }

            HidePrototypeDiagnostics(scene);

            GameObject hudObject = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, scene);
            hudObject.name = HudRootName;
            TacticalHudController hud = hudObject.GetComponent<TacticalHudController>();
            hud.ConfigureSources(squad, tacticalInput, identity, clock);

            PrefabUtility.RecordPrefabInstancePropertyModifications(playerInput);
            PrefabUtility.RecordPrefabInstancePropertyModifications(tacticalInput);
            PrefabUtility.RecordPrefabInstancePropertyModifications(identity);
            PrefabUtility.RecordPrefabInstancePropertyModifications(clock);
            PrefabUtility.RecordPrefabInstancePropertyModifications(hud);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException(
                    $"Unity could not save {ProjectInfo.PrototypeScenePath}.");
            }

            return hud;
        }

        private static void HidePrototypeDiagnostics(Scene scene)
        {
            foreach (OfficerCommandDebugUI debugUi in scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<OfficerCommandDebugUI>(true)))
            {
                debugUi.gameObject.SetActive(false);
            }

            foreach (MissionAfterActionDebugUI missionUi in scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MissionAfterActionDebugUI>(true)))
            {
                missionUi.gameObject.SetActive(false);
            }

            foreach (OfficerVisual officerVisual in scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<OfficerVisual>(true)))
            {
                officerVisual.SetWorldStatusVisible(false);
            }

            foreach (RulesOfEntry.AI.ActorVisual actorVisual in scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<RulesOfEntry.AI.ActorVisual>(true)))
            {
                actorVisual.SetWorldStatusVisible(false);
            }
        }

        private static GameObject CreatePanel(
            string name,
            Transform parent,
            Color color,
            bool addOutline)
        {
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Image));
            result.transform.SetParent(parent, false);
            Image image = result.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (addOutline)
            {
                UnityEngine.UI.Outline outline =
                    result.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor = Border;
                outline.effectDistance = new Vector2(1f, -1f);
            }

            return result;
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
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Text));
            result.transform.SetParent(parent, false);
            Text text = result.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static void AddTextShadow(Text text)
        {
            if (text == null)
            {
                return;
            }

            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.88f);
            shadow.effectDistance = new Vector2(1f, -1f);
            shadow.useGraphicAlpha = true;
        }

        private static void CreateDivider(Transform parent, Vector2 position, Vector2 size)
        {
            GameObject divider = CreatePanel("Divider", parent, Border, false);
            SetAnchored(
                divider.GetComponent<RectTransform>(),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                position,
                size);
        }

        private static void SetAnchored(
            RectTransform rect,
            Vector2 anchor,
            Vector2 pivot,
            Vector2 position,
            Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(
            RectTransform rect,
            float left,
            float bottom,
            float right,
            float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = Path.GetFileName(folderPath);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException($"Invalid folder path: {folderPath}");
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
