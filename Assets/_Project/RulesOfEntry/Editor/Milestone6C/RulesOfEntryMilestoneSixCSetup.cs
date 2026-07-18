using System;
using System.IO;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Deployment;
using RulesOfEntry.Input;
using RulesOfEntry.Missions;
using RulesOfEntry.Officers;
using RulesOfEntry.Player;
using RulesOfEntry.UI.Operations;
using RulesOfEntry.UI.TacticalHud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone6C
{
    public static class RulesOfEntryMilestoneSixCSetup
    {
        internal const string GeneratedRootName = "[Milestone6C_OperationDeployment]";
        internal const string TabletRootName = "[ROE_InMissionTablet]";
        internal const string TabletPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/UI/ROE_InMissionTablet.prefab";
        internal const string OfficerAlphaPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_OfficerAlpha.prefab";
        internal const string OfficerBravoPrefabPath =
            "Assets/_Project/RulesOfEntry/Prefabs/Actors/ROE_OfficerBravo.prefab";

        private const string FontPath =
            "Assets/_Project/RulesOfEntry/Art/UI/Fonts/LatinModernSansDemiCondensed.otf";
        private const string HardwarePath =
            "Assets/_Project/RulesOfEntry/Art/UI/Planning/RuggedTabletHardwareCutout.png";
        private const string BodyCameraObjectName = "ROE_OfficerBodyCamera";

        private static readonly Color ScreenBackground =
            new Color(0.003f, 0.009f, 0.013f, 1f);
        private static readonly Color Panel =
            new Color(0.006f, 0.015f, 0.021f, 0.98f);
        private static readonly Color Raised =
            new Color(0.025f, 0.033f, 0.038f, 1f);
        private static readonly Color Signal =
            new Color(0.02f, 0.64f, 1f, 1f);
        private static readonly Color Line =
            new Color(0.12f, 0.36f, 0.48f, 0.8f);
        private static readonly Color PrimaryText =
            new Color(0.91f, 0.95f, 0.97f, 1f);
        private static readonly Color SecondaryText =
            new Color(0.57f, 0.67f, 0.72f, 1f);

        [MenuItem(
            "Rules of Entry/Milestone 6C/Build Deployment and In-Mission Tablet",
            priority = 620)]
        public static void BuildDeploymentAndInMissionTablet()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building Milestone 6C.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning(
                    "Milestone 6C",
                    "Setup cancelled before saving open scenes.");
                return;
            }

            try
            {
                RequireBaselineAssets();
                Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath)
                    ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                Sprite hardware = AssetDatabase.LoadAssetAtPath<Sprite>(HardwarePath);
                if (font == null || hardware == null)
                {
                    throw new InvalidOperationException(
                        "The approved tablet font or transparent hardware cutout "
                            + "is missing. Rebuild Milestone 6A first.");
                }

                ConfigureOfficerBodyCamera(OfficerAlphaPrefabPath, "BC-ALPHA-01");
                ConfigureOfficerBodyCamera(OfficerBravoPrefabPath, "BC-BRAVO-02");
                GameObject tabletPrefab = CreateTabletPrefab(font, hardware);
                InstallOperationScene(tabletPrefab);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ProjectLog.Info(
                    "Milestone 6C",
                    "Entry deployment and live in-mission officer body-camera tablet "
                        + "created. Running validation now.");
                RulesOfEntryMilestoneSixCValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Milestone 6C", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Milestone 6C setup stopped. Check the first Console error.",
                    "OK");
            }
        }

        private static void RequireBaselineAssets()
        {
            string[] required =
            {
                ProjectInfo.PrototypeScenePath,
                OfficerAlphaPrefabPath,
                OfficerBravoPrefabPath,
                HardwarePath
            };
            string missing = required.FirstOrDefault(path =>
                AssetDatabase.LoadMainAssetAtPath(path) == null);
            if (!string.IsNullOrWhiteSpace(missing))
            {
                throw new InvalidOperationException(
                    $"Required baseline asset is missing: {missing}");
            }
        }

        private static void ConfigureOfficerBodyCamera(
            string prefabPath,
            string cameraIdentifier)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                TacticalOfficerController officer =
                    root.GetComponent<TacticalOfficerController>();
                if (officer == null)
                {
                    throw new InvalidOperationException(
                        $"{prefabPath} has no TacticalOfficerController.");
                }

                Transform existing = root.transform.Find(BodyCameraObjectName);
                if (existing != null)
                {
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }

                GameObject cameraObject = new GameObject(
                    BodyCameraObjectName,
                    typeof(Camera));
                cameraObject.transform.SetParent(root.transform, false);
                cameraObject.transform.localPosition = new Vector3(0f, 1.43f, 0.23f);
                cameraObject.transform.localRotation = Quaternion.Euler(3f, 0f, 0f);
                Camera bodyCamera = cameraObject.GetComponent<Camera>();
                bodyCamera.enabled = false;
                bodyCamera.fieldOfView = 78f;
                bodyCamera.nearClipPlane = 0.03f;
                bodyCamera.farClipPlane = 500f;
                bodyCamera.depth = -20f;
                bodyCamera.allowHDR = true;
                bodyCamera.allowMSAA = false;
                bodyCamera.allowDynamicResolution = false;
                bodyCamera.useOcclusionCulling = true;
                bodyCamera.stereoTargetEye = StereoTargetEyeMask.None;
                TryAddHdrpCameraData(cameraObject);

                OfficerBodyCameraSource source =
                    root.GetComponent<OfficerBodyCameraSource>()
                    ?? root.AddComponent<OfficerBodyCameraSource>();
                source.Configure(officer, bodyCamera, cameraIdentifier, true, true);
                if (PrefabUtility.SaveAsPrefabAsset(root, prefabPath) == null)
                {
                    throw new InvalidOperationException(
                        $"Unity could not save {prefabPath}.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void TryAddHdrpCameraData(GameObject cameraObject)
        {
            Type hdCameraType = Type.GetType(
                "UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData, "
                    + "Unity.RenderPipelines.HighDefinition.Runtime");
            if (hdCameraType != null && cameraObject.GetComponent(hdCameraType) == null)
            {
                cameraObject.AddComponent(hdCameraType);
            }
        }

        private static GameObject CreateTabletPrefab(Font font, Sprite hardware)
        {
            EnsureFolder(Path.GetDirectoryName(TabletPrefabPath)?.Replace('\\', '/'));
            GameObject root = new GameObject(
                "ROE_InMissionTablet",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(InMissionTabletController));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject interfaceObject = CreateUiObject(
                "TabletInterface",
                root.transform,
                typeof(CanvasGroup),
                typeof(Image));
            RectTransform interfaceRect = interfaceObject.GetComponent<RectTransform>();
            Stretch(interfaceRect);
            interfaceObject.GetComponent<Image>().color = Color.clear;
            CanvasGroup tabletGroup = interfaceObject.GetComponent<CanvasGroup>();

            Image device = CreateImage(
                "RuggedDevice",
                interfaceObject.transform,
                Color.white);
            Stretch(device.rectTransform);
            device.sprite = hardware;
            device.preserveAspect = true;
            device.raycastTarget = false;
            AspectRatioFitter fitter = device.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1672f / 941f;

            Image screen = CreateImage(
                "TacticalDisplay",
                device.transform,
                ScreenBackground);
            SetNormalizedRect(
                screen.rectTransform,
                new Vector2(0.207f, 0.184f),
                new Vector2(0.793f, 0.826f));
            screen.raycastTarget = false;
            AddOutline(screen, new Color(0.035f, 0.12f, 0.17f, 1f));

            Text header = CreateText(
                "OperationHeader",
                screen.transform,
                font,
                19,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Signal);
            SetNormalizedRect(
                header.rectTransform,
                new Vector2(0.025f, 0.925f),
                new Vector2(0.57f, 0.98f));
            Text secure = CreateText(
                "SecureStatus",
                screen.transform,
                font,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                SecondaryText);
            SetNormalizedRect(
                secure.rectTransform,
                new Vector2(0.57f, 0.925f),
                new Vector2(0.975f, 0.98f));
            Image headerLine = CreateImage("HeaderLine", screen.transform, Signal);
            SetNormalizedRect(
                headerLine.rectTransform,
                new Vector2(0.025f, 0.916f),
                new Vector2(0.975f, 0.919f));

            Button situationTab = CreateButton(
                "SituationTab",
                screen.transform,
                font,
                "1  SITUATION",
                new Vector2(0.025f, 0.83f),
                new Vector2(0.225f, 0.905f),
                false,
                out _);
            Button objectivesTab = CreateButton(
                "ObjectivesTab",
                screen.transform,
                font,
                "2  OBJECTIVES",
                new Vector2(0.225f, 0.83f),
                new Vector2(0.425f, 0.905f),
                false,
                out _);
            Button bodyCamerasTab = CreateButton(
                "BodyCamerasTab",
                screen.transform,
                font,
                "3  BODY CAMERAS",
                new Vector2(0.425f, 0.83f),
                new Vector2(0.67f, 0.905f),
                false,
                out _);
            Text liveLabel = CreateText(
                "LiveFeedLabel",
                screen.transform,
                font,
                13,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                SecondaryText);
            liveLabel.text = "AUTHORIZED LIVE VIDEO  //  COMMAND ACCESS";
            SetNormalizedRect(
                liveLabel.rectTransform,
                new Vector2(0.68f, 0.83f),
                new Vector2(0.975f, 0.905f));

            GameObject textPage = CreateUiObject(
                "TextPage",
                screen.transform,
                typeof(Image));
            SetNormalizedRect(
                textPage.GetComponent<RectTransform>(),
                new Vector2(0.025f, 0.18f),
                new Vector2(0.975f, 0.81f));
            Image textPanel = textPage.GetComponent<Image>();
            textPanel.color = Panel;
            textPanel.raycastTarget = false;
            AddOutline(textPanel, Line);
            Text pageTitle = CreateText(
                "PageTitle",
                textPage.transform,
                font,
                21,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                Signal);
            SetNormalizedRect(
                pageTitle.rectTransform,
                new Vector2(0.035f, 0.85f),
                new Vector2(0.965f, 0.96f));
            Text pageBody = CreateText(
                "PageBody",
                textPage.transform,
                font,
                18,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                PrimaryText);
            SetNormalizedRect(
                pageBody.rectTransform,
                new Vector2(0.035f, 0.06f),
                new Vector2(0.965f, 0.84f));
            pageBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            pageBody.verticalOverflow = VerticalWrapMode.Truncate;
            pageBody.lineSpacing = 1.08f;

            GameObject bodyCameraPage = CreateUiObject(
                "BodyCameraPage",
                screen.transform);
            SetNormalizedRect(
                bodyCameraPage.GetComponent<RectTransform>(),
                new Vector2(0.025f, 0.18f),
                new Vector2(0.975f, 0.81f));

            Image feedFrame = CreateImage(
                "FeedFrame",
                bodyCameraPage.transform,
                Color.black);
            SetNormalizedRect(
                feedFrame.rectTransform,
                Vector2.zero,
                new Vector2(0.74f, 1f));
            AddOutline(feedFrame, Line);
            GameObject rawFeedObject = CreateUiObject(
                "OfficerBodyCameraFeed",
                feedFrame.transform,
                typeof(RawImage));
            RawImage rawFeed = rawFeedObject.GetComponent<RawImage>();
            rawFeed.color = Color.black;
            rawFeed.raycastTarget = false;
            Stretch(rawFeed.rectTransform, 3f, 3f, 3f, 3f);
            Text recording = CreateText(
                "FeedRecording",
                feedFrame.transform,
                font,
                14,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                PrimaryText);
            SetNormalizedRect(
                recording.rectTransform,
                new Vector2(0.025f, 0.88f),
                new Vector2(0.975f, 0.975f));
            AddShadow(recording);

            Image sidePanel = CreateImage(
                "FeedDataPanel",
                bodyCameraPage.transform,
                Panel);
            SetNormalizedRect(
                sidePanel.rectTransform,
                new Vector2(0.755f, 0f),
                Vector2.one);
            AddOutline(sidePanel, Line);
            Text identity = CreateText(
                "FeedIdentity",
                sidePanel.transform,
                font,
                17,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                PrimaryText);
            SetNormalizedRect(
                identity.rectTransform,
                new Vector2(0.07f, 0.61f),
                new Vector2(0.93f, 0.95f));
            Text telemetry = CreateText(
                "FeedTelemetry",
                sidePanel.transform,
                font,
                15,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                PrimaryText);
            SetNormalizedRect(
                telemetry.rectTransform,
                new Vector2(0.07f, 0.18f),
                new Vector2(0.93f, 0.61f));
            Text signal = CreateText(
                "FeedSignal",
                sidePanel.transform,
                font,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Signal);
            SetNormalizedRect(
                signal.rectTransform,
                new Vector2(0.07f, 0.05f),
                new Vector2(0.93f, 0.17f));

            Text feedNavigation = CreateText(
                "FeedNavigation",
                screen.transform,
                font,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                SecondaryText);
            SetNormalizedRect(
                feedNavigation.rectTransform,
                new Vector2(0.025f, 0.135f),
                new Vector2(0.66f, 0.175f));

            Button close = CreateButton(
                "CloseButton",
                screen.transform,
                font,
                "CLOSE TABLET",
                new Vector2(0.025f, 0.04f),
                new Vector2(0.20f, 0.12f),
                false,
                out _);
            Button previous = CreateButton(
                "PreviousFeedButton",
                screen.transform,
                font,
                "←  PREVIOUS FEED",
                new Vector2(0.53f, 0.04f),
                new Vector2(0.745f, 0.12f),
                false,
                out _);
            Button next = CreateButton(
                "NextFeedButton",
                screen.transform,
                font,
                "NEXT FEED  →",
                new Vector2(0.76f, 0.04f),
                new Vector2(0.975f, 0.12f),
                true,
                out _);

            InMissionTabletController controller =
                root.GetComponent<InMissionTabletController>();
            controller.ConfigureVisuals(
                tabletGroup,
                header,
                secure,
                situationTab,
                objectivesTab,
                bodyCamerasTab,
                previous,
                next,
                close,
                textPage,
                pageTitle,
                pageBody,
                bodyCameraPage,
                rawFeed,
                recording,
                identity,
                telemetry,
                signal,
                feedNavigation);
            tabletGroup.alpha = 0f;
            tabletGroup.interactable = false;
            tabletGroup.blocksRaycasts = false;
            interfaceObject.SetActive(false);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, TabletPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab ?? throw new InvalidOperationException(
                $"Unity could not save {TabletPrefabPath}.");
        }

        private static void InstallOperationScene(GameObject tabletPrefab)
        {
            Scene scene = EditorSceneManager.OpenScene(
                ProjectInfo.PrototypeScenePath,
                OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            foreach (GameObject previous in scene.GetRootGameObjects().Where(root =>
                string.Equals(root.name, GeneratedRootName, StringComparison.Ordinal)
                || string.Equals(root.name, TabletRootName, StringComparison.Ordinal)).ToArray())
            {
                UnityEngine.Object.DestroyImmediate(previous);
            }

            GameObject player = scene.GetRootGameObjects().FirstOrDefault(root =>
                string.Equals(root.name, "ROE_Player", StringComparison.Ordinal));
            if (player == null)
            {
                throw new InvalidOperationException(
                    "ROE_Player is missing from the operation scene.");
            }

            TacticalPlayerInput input = player.GetComponent<TacticalPlayerInput>();
            CursorStateController cursor = player.GetComponent<CursorStateController>();
            CharacterController character = player.GetComponent<CharacterController>();
            OfficerSquadController squad = player.GetComponent<OfficerSquadController>();
            MissionClock clock = player.GetComponent<MissionClock>();
            if (clock == null)
            {
                clock = player.AddComponent<MissionClock>();
                clock.Configure(new DateTime(2026, 7, 17, 22, 41, 0), 1f);
            }

            MissionController mission = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MissionController>(true))
                .FirstOrDefault();
            if (input == null || cursor == null || character == null
                || squad == null || mission == null)
            {
                throw new InvalidOperationException(
                    "The operation scene is missing player input, cursor, character, "
                        + "squad, or mission references.");
            }

            foreach (TacticalOfficerController officer in squad.Officers)
            {
                if (officer == null
                    || officer.GetComponent<OfficerBodyCameraSource>() == null)
                {
                    throw new InvalidOperationException(
                        "Scene officers did not inherit configured body-camera sources. "
                            + "Rebuild Milestone 4, then rerun Milestone 6C.");
                }
            }

            GameObject generatedRoot = new GameObject(GeneratedRootName);
            SceneManager.MoveGameObjectToScene(generatedRoot, scene);
            OperationEntryAnchor[] anchors =
            {
                CreateEntryAnchor(
                    generatedRoot.transform,
                    "entry_south_administration",
                    "South Administration Entrance",
                    new Vector3(0f, 0.05f, -4.5f),
                    0f),
                CreateEntryAnchor(
                    generatedRoot.transform,
                    "entry_west_service_yard",
                    "West Service Yard",
                    new Vector3(-8f, 0.05f, 3f),
                    90f),
                CreateEntryAnchor(
                    generatedRoot.transform,
                    "entry_north_pipe_gallery",
                    "North Pipe Gallery",
                    new Vector3(0f, 0.05f, 6.5f),
                    180f)
            };
            OperationDeploymentCoordinator deployment =
                generatedRoot.AddComponent<OperationDeploymentCoordinator>();
            deployment.Configure(
                player.transform,
                character,
                squad,
                anchors,
                2.5f);

            GameObject tabletObject = (GameObject)PrefabUtility.InstantiatePrefab(
                tabletPrefab,
                scene);
            tabletObject.name = TabletRootName;
            InMissionTabletController tablet =
                tabletObject.GetComponent<InMissionTabletController>();
            tablet.ConfigureSources(input, cursor, squad, mission, clock, deployment);
            PrefabUtility.RecordPrefabInstancePropertyModifications(tablet);
            PrefabUtility.RecordPrefabInstancePropertyModifications(deployment);
            PrefabUtility.RecordPrefabInstancePropertyModifications(clock);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException(
                    $"Unity could not save {ProjectInfo.PrototypeScenePath}.");
            }
        }

        private static OperationEntryAnchor CreateEntryAnchor(
            Transform parent,
            string id,
            string displayName,
            Vector3 position,
            float yaw)
        {
            GameObject root = new GameObject($"EntryAnchor_{id}");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(
                position,
                Quaternion.Euler(0f, yaw, 0f));
            Transform playerSpawn = CreateSpawn(root.transform, "PlayerSpawn", Vector3.zero);
            Transform[] officerSpawns = new Transform[8];
            for (int index = 0; index < officerSpawns.Length; index++)
            {
                int row = index / 2;
                float side = index % 2 == 0 ? -0.72f : 0.72f;
                Vector3 localPosition = new Vector3(
                    side,
                    -0.05f,
                    1.15f + row * 0.92f);
                officerSpawns[index] = CreateSpawn(
                    root.transform,
                    $"OfficerSpawn_{index + 1:00}",
                    localPosition);
            }

            OperationEntryAnchor anchor = root.AddComponent<OperationEntryAnchor>();
            anchor.Configure(id, displayName, playerSpawn, officerSpawns);
            return anchor;
        }

        private static Transform CreateSpawn(
            Transform parent,
            string name,
            Vector3 localPosition)
        {
            GameObject spawn = new GameObject(name);
            spawn.transform.SetParent(parent, false);
            spawn.transform.localPosition = localPosition;
            spawn.transform.localRotation = Quaternion.identity;
            return spawn.transform;
        }

        private static GameObject CreateUiObject(
            string name,
            Transform parent,
            params Type[] componentTypes)
        {
            Type[] types = new[] { typeof(RectTransform) }
                .Concat(componentTypes ?? Array.Empty<Type>())
                .Distinct()
                .ToArray();
            GameObject result = new GameObject(name, types);
            result.transform.SetParent(parent, false);
            return result;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject result = CreateUiObject(name, parent, typeof(Image));
            Image image = result.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
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
            GameObject result = CreateUiObject(name, parent, typeof(Text));
            Text text = result.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
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
            Vector2 anchorMin,
            Vector2 anchorMax,
            bool primary,
            out Text labelText)
        {
            GameObject result = CreateUiObject(
                name,
                parent,
                typeof(Image),
                typeof(Button));
            SetNormalizedRect(
                result.GetComponent<RectTransform>(),
                anchorMin,
                anchorMax);
            Image background = result.GetComponent<Image>();
            background.color = primary
                ? new Color(0.055f, 0.31f, 0.47f, 1f)
                : Raised;
            Button button = result.GetComponent<Button>();
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            colors.pressedColor = new Color(0.75f, 0.86f, 0.94f, 1f);
            colors.selectedColor = new Color(0.9f, 1f, 1f, 1f);
            colors.disabledColor = new Color(0.35f, 0.38f, 0.4f, 0.7f);
            button.colors = colors;
            labelText = CreateText(
                "Label",
                result.transform,
                font,
                name.EndsWith("Tab", StringComparison.Ordinal) ? 14 : 16,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                PrimaryText);
            labelText.text = label;
            Stretch(labelText.rectTransform, 8f, 4f, 8f, 4f);
            return button;
        }

        private static void AddOutline(Image image, Color color)
        {
            Outline outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
        }

        private static void AddShadow(Text text)
        {
            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(1f, -1f);
            shadow.useGraphicAlpha = true;
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

        private static void Stretch(
            RectTransform rect,
            float left = 0f,
            float bottom = 0f,
            float right = 0f,
            float top = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath)
                || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = Path.GetFileName(folderPath);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
