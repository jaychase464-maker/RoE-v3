using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RulesOfEntry.Actors;
using RulesOfEntry.Core;
using RulesOfEntry.Deployment;
using RulesOfEntry.Input;
using RulesOfEntry.Missions;
using RulesOfEntry.Officers;
using RulesOfEntry.Planning;
using RulesOfEntry.Player;
using RulesOfEntry.UI.TacticalHud;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RulesOfEntry.UI.Operations
{
    public enum OperationTabletPage
    {
        Situation = 0,
        Objectives = 1,
        BodyCameras = 2
    }

    /// <summary>
    /// In-operation rugged tablet. It never mutates mission evidence or officer
    /// decisions; it presents authoritative operation and body-camera data.
    /// </summary>
    [DefaultExecutionOrder(-400)]
    [DisallowMultipleComponent]
    public sealed class InMissionTabletController : MonoBehaviour
    {
        private static readonly Color ActiveTab =
            new Color(0.055f, 0.31f, 0.47f, 1f);
        private static readonly Color InactiveTab =
            new Color(0.025f, 0.033f, 0.038f, 1f);
        private static readonly Color ActiveText =
            new Color(0.94f, 0.98f, 1f, 1f);
        private static readonly Color InactiveText =
            new Color(0.58f, 0.68f, 0.73f, 1f);

        [Header("Runtime sources")]
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private CursorStateController cursorController;
        [SerializeField] private OfficerSquadController squad;
        [SerializeField] private MissionController missionController;
        [SerializeField] private MissionClock missionClock;
        [SerializeField] private OperationDeploymentCoordinator deploymentCoordinator;

        [Header("Tablet shell")]
        [SerializeField] private CanvasGroup tabletGroup;
        [SerializeField] private Text operationHeaderText;
        [SerializeField] private Text secureStatusText;

        [Header("Navigation")]
        [SerializeField] private Button situationTabButton;
        [SerializeField] private Button objectivesTabButton;
        [SerializeField] private Button bodyCamerasTabButton;
        [SerializeField] private Button previousFeedButton;
        [SerializeField] private Button nextFeedButton;
        [SerializeField] private Button closeButton;

        [Header("Text pages")]
        [SerializeField] private GameObject textPageRoot;
        [SerializeField] private Text pageTitleText;
        [SerializeField] private Text pageBodyText;

        [Header("Body-camera page")]
        [SerializeField] private GameObject bodyCameraPageRoot;
        [SerializeField] private RawImage feedImage;
        [SerializeField] private Text feedRecordingText;
        [SerializeField] private Text feedIdentityText;
        [SerializeField] private Text feedTelemetryText;
        [SerializeField] private Text feedSignalText;
        [SerializeField] private Text feedNavigationText;

        [Header("Feed rendering")]
        [SerializeField, Min(320)] private int feedWidth = 960;
        [SerializeField, Min(180)] private int feedHeight = 540;
        [SerializeField, Min(0.02f)] private float refreshIntervalSeconds = 0.1f;

        private readonly List<OfficerBodyCameraSource> feeds =
            new List<OfficerBodyCameraSource>(8);
        private RenderTexture sharedFeedTexture;
        private int selectedFeedIndex = -1;
        private float refreshTimer;

        public bool IsOpen => tabletGroup != null
            && tabletGroup.gameObject.activeSelf;
        public OperationTabletPage Page { get; private set; } =
            OperationTabletPage.BodyCameras;
        public int FeedCount => feeds.Count;
        public int SelectedFeedIndex => selectedFeedIndex;
        public bool HasCompleteVisualConfiguration => tabletGroup != null
            && operationHeaderText != null
            && secureStatusText != null
            && situationTabButton != null
            && objectivesTabButton != null
            && bodyCamerasTabButton != null
            && previousFeedButton != null
            && nextFeedButton != null
            && closeButton != null
            && textPageRoot != null
            && pageTitleText != null
            && pageBodyText != null
            && bodyCameraPageRoot != null
            && feedImage != null
            && feedRecordingText != null
            && feedIdentityText != null
            && feedTelemetryText != null
            && feedSignalText != null
            && feedNavigationText != null;
        public bool HasCompleteConfiguration => playerInput != null
            && cursorController != null
            && squad != null
            && missionController != null
            && missionClock != null
            && deploymentCoordinator != null
            && HasCompleteVisualConfiguration;

        public void ConfigureSources(
            TacticalPlayerInput configuredInput,
            CursorStateController configuredCursor,
            OfficerSquadController configuredSquad,
            MissionController configuredMission,
            MissionClock configuredClock,
            OperationDeploymentCoordinator configuredDeployment)
        {
            playerInput = configuredInput;
            cursorController = configuredCursor;
            squad = configuredSquad;
            missionController = configuredMission;
            missionClock = configuredClock;
            deploymentCoordinator = configuredDeployment;
        }

        public void ConfigureVisuals(
            CanvasGroup configuredTabletGroup,
            Text configuredOperationHeader,
            Text configuredSecureStatus,
            Button configuredSituationTab,
            Button configuredObjectivesTab,
            Button configuredBodyCamerasTab,
            Button configuredPreviousFeed,
            Button configuredNextFeed,
            Button configuredClose,
            GameObject configuredTextPageRoot,
            Text configuredPageTitle,
            Text configuredPageBody,
            GameObject configuredBodyCameraPageRoot,
            RawImage configuredFeedImage,
            Text configuredFeedRecording,
            Text configuredFeedIdentity,
            Text configuredFeedTelemetry,
            Text configuredFeedSignal,
            Text configuredFeedNavigation)
        {
            tabletGroup = configuredTabletGroup;
            operationHeaderText = configuredOperationHeader;
            secureStatusText = configuredSecureStatus;
            situationTabButton = configuredSituationTab;
            objectivesTabButton = configuredObjectivesTab;
            bodyCamerasTabButton = configuredBodyCamerasTab;
            previousFeedButton = configuredPreviousFeed;
            nextFeedButton = configuredNextFeed;
            closeButton = configuredClose;
            textPageRoot = configuredTextPageRoot;
            pageTitleText = configuredPageTitle;
            pageBodyText = configuredPageBody;
            bodyCameraPageRoot = configuredBodyCameraPageRoot;
            feedImage = configuredFeedImage;
            feedRecordingText = configuredFeedRecording;
            feedIdentityText = configuredFeedIdentity;
            feedTelemetryText = configuredFeedTelemetry;
            feedSignalText = configuredFeedSignal;
            feedNavigationText = configuredFeedNavigation;
        }

        private void Awake()
        {
            WireControls();
            SetTabletVisible(false);
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Operation Tablet",
                    "In-mission tablet references are incomplete. Run the "
                        + "Milestone 6C setup tool outside Play Mode.",
                    this);
            }
        }

        private void Update()
        {
            if (Keyboard.current?.tabKey.wasPressedThisFrame == true)
            {
                ToggleTablet();
                return;
            }

            if (!IsOpen)
            {
                return;
            }

            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true
                || Gamepad.current?.buttonEast.wasPressedThisFrame == true)
            {
                CloseTablet();
                return;
            }

            HandleOpenTabletShortcuts();
            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = refreshIntervalSeconds;
                RefreshCurrentPage();
            }
        }

        public void ToggleTablet()
        {
            if (IsOpen)
            {
                CloseTablet();
            }
            else
            {
                OpenTablet();
            }
        }

        public void OpenTablet()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Operation Tablet",
                    "Cannot open an incompletely configured in-mission tablet.",
                    this);
                return;
            }

            RebuildFeedList();
            EnsureFeedTexture();
            SetTabletVisible(true);
            cursorController.SetCursorLocked(false);
            SetPage(OperationTabletPage.BodyCameras);
            refreshTimer = 0f;
        }

        public void CloseTablet()
        {
            if (!IsOpen)
            {
                return;
            }

            StopAllFeeds();
            SetTabletVisible(false);
            cursorController.SetCursorLocked(true);
        }

        public void ShowSituation() => SetPage(OperationTabletPage.Situation);
        public void ShowObjectives() => SetPage(OperationTabletPage.Objectives);
        public void ShowBodyCameras() => SetPage(OperationTabletPage.BodyCameras);

        public void SelectPreviousFeed()
        {
            SelectFeed(OperationTabletRules.WrapFeedIndex(
                selectedFeedIndex,
                -1,
                feeds.Count));
        }

        public void SelectNextFeed()
        {
            SelectFeed(OperationTabletRules.WrapFeedIndex(
                selectedFeedIndex,
                1,
                feeds.Count));
        }

        private void SetPage(OperationTabletPage page)
        {
            Page = page;
            bool bodyCameraPage = page == OperationTabletPage.BodyCameras;
            textPageRoot.SetActive(!bodyCameraPage);
            bodyCameraPageRoot.SetActive(bodyCameraPage);
            previousFeedButton.gameObject.SetActive(bodyCameraPage);
            nextFeedButton.gameObject.SetActive(bodyCameraPage);

            if (bodyCameraPage)
            {
                if (selectedFeedIndex < 0 || selectedFeedIndex >= feeds.Count)
                {
                    SelectFeed(feeds.Count > 0 ? 0 : -1);
                }
                else
                {
                    SelectFeed(selectedFeedIndex);
                }
            }
            else
            {
                StopAllFeeds();
            }

            UpdateTabPresentation();
            RefreshCurrentPage();
            Select(GetTabButton(page));
        }

        private void RebuildFeedList()
        {
            string selectedId = selectedFeedIndex >= 0
                && selectedFeedIndex < feeds.Count
                ? feeds[selectedFeedIndex]?.Identity?.ActorId
                : string.Empty;
            StopAllFeeds();
            feeds.Clear();
            foreach (TacticalOfficerController officer in squad.Officers)
            {
                OfficerBodyCameraSource source = officer != null
                    ? officer.GetComponent<OfficerBodyCameraSource>()
                    : null;
                if (source != null && source.HasCompleteConfiguration
                    && source.gameObject.activeInHierarchy)
                {
                    feeds.Add(source);
                }
            }

            selectedFeedIndex = feeds.FindIndex(source => string.Equals(
                source.Identity?.ActorId,
                selectedId,
                StringComparison.Ordinal));
            if (selectedFeedIndex < 0 && feeds.Count > 0)
            {
                selectedFeedIndex = 0;
            }
        }

        private void SelectFeed(int index)
        {
            StopAllFeeds();
            selectedFeedIndex = index;
            if (index < 0 || index >= feeds.Count)
            {
                feedImage.texture = null;
                feedImage.color = Color.black;
                RefreshBodyCameraPage();
                return;
            }

            EnsureFeedTexture();
            OfficerBodyCameraSource source = feeds[index];
            bool streaming = source.BeginStreaming(sharedFeedTexture);
            feedImage.texture = streaming ? sharedFeedTexture : null;
            feedImage.color = streaming ? Color.white : Color.black;
            RefreshBodyCameraPage();
        }

        private void StopAllFeeds()
        {
            foreach (OfficerBodyCameraSource source in feeds)
            {
                source?.StopStreaming();
            }
        }

        private void RefreshCurrentPage()
        {
            RefreshHeader();
            switch (Page)
            {
                case OperationTabletPage.Situation:
                    RefreshSituationPage();
                    break;
                case OperationTabletPage.Objectives:
                    RefreshObjectivesPage();
                    break;
                case OperationTabletPage.BodyCameras:
                    RefreshBodyCameraPage();
                    break;
            }
        }

        private void RefreshHeader()
        {
            string operationCode = OperationDeploymentContext.OperationCode;
            string missionName = missionController.Definition != null
                ? missionController.Definition.DisplayName
                : "ACTIVE OPERATION";
            operationHeaderText.text = string.IsNullOrWhiteSpace(operationCode)
                ? $"RULES OF ENTRY  //  {missionName.ToUpperInvariant()}"
                : $"RULES OF ENTRY  //  {operationCode.ToUpperInvariant()}";
            secureStatusText.text =
                $"CALDER CITY POLICE  //  ENCRYPTED  //  {missionClock.CurrentTimestamp:HH:mm:ss}";
        }

        private void RefreshSituationPage()
        {
            MissionDefinition definition = missionController.Definition;
            pageTitleText.text = "OPERATION SITUATION";
            pageBodyText.text =
                $"MISSION\n{definition?.DisplayName ?? "UNAVAILABLE"}\n\n"
                + $"PHASE\n{missionController.Phase.ToString().ToUpperInvariant()}\n\n"
                + $"DEPLOYED ENTRY\n{GetAppliedEntryLabel()}\n\n"
                + $"ASSIGNED ELEMENT\n{squad.Officers.Count} OFFICER(S)\n\n"
                + $"BRIEFING\n{definition?.Briefing ?? "No briefing is available."}";
        }

        private void RefreshObjectivesPage()
        {
            pageTitleText.text = "LIVE MISSION OBJECTIVES";
            StringBuilder summary = new StringBuilder();
            AfterActionReport report = missionController.CurrentReport;
            if (report != null && report.Objectives.Count > 0)
            {
                foreach (MissionObjectiveEvaluation objective in report.Objectives)
                {
                    summary.Append('[')
                        .Append(objective.Status.ToString().ToUpperInvariant())
                        .Append("]  ")
                        .Append(objective.DisplayName.ToUpperInvariant())
                        .Append('\n')
                        .Append(objective.Rationale)
                        .Append("\n\n");
                }
            }
            else if (missionController.Definition != null)
            {
                foreach (MissionObjectiveDefinition objective in
                    missionController.Definition.Objectives)
                {
                    summary.Append("[PENDING]  ")
                        .Append(objective.DisplayName.ToUpperInvariant())
                        .Append('\n')
                        .Append(objective.Briefing)
                        .Append("\n\n");
                }
            }

            pageBodyText.text = summary.Length > 0
                ? summary.ToString()
                : "No mission objectives are available.";
        }

        private void RefreshBodyCameraPage()
        {
            bool hasFeed = selectedFeedIndex >= 0
                && selectedFeedIndex < feeds.Count;
            if (!hasFeed)
            {
                feedRecordingText.text = "NO DEPLOYED BODY-CAMERA FEEDS";
                feedIdentityText.text = "OFFICER\nUNAVAILABLE";
                feedTelemetryText.text =
                    "No active assigned officer exposes a valid body-camera source.";
                feedSignalText.text = "SIGNAL UNAVAILABLE";
                feedSignalText.color = OperationTabletRules.GetSignalColor(false, false);
                feedNavigationText.text = "0 / 0";
                previousFeedButton.interactable = false;
                nextFeedButton.interactable = false;
                return;
            }

            OfficerBodyCameraSource source = feeds[selectedFeedIndex];
            TacticalOfficerController officer = source.Officer;
            ActorConditionLevel condition = officer.Condition != null
                ? officer.Condition.Snapshot.Level
                : ActorConditionLevel.Incapacitated;
            OfficerAmmunitionStatus ammunition =
                officer.GetComponent<OfficerAmmunitionStatus>();
            string ammunitionLabel = ammunition != null
                ? TacticalHudRules.GetAmmunitionLabel(ammunition.Condition)
                : "UNKNOWN";
            bool streaming = source.IsStreaming;
            feedRecordingText.text =
                $"{(source.Recording ? "● REC" : "STANDBY")}  //  "
                + $"{source.CameraIdentifier.ToUpperInvariant()}  //  "
                + $"{missionClock.CurrentTimestamp:dd MMM yyyy  HH:mm:ss}".ToUpperInvariant();
            feedIdentityText.text =
                $"OFFICER {selectedFeedIndex + 1:00}\n"
                + $"{source.Identity.DisplayName.ToUpperInvariant()}\n\n"
                + $"ID\n{source.Identity.ActorId.ToUpperInvariant()}";
            feedTelemetryText.text =
                $"CONDITION\n{TacticalHudRules.GetConditionLabel(condition)}\n\n"
                + $"AMMUNITION\n{ammunitionLabel}\n\n"
                + $"CURRENT ORDER\n{TacticalHudRules.GetActivityLabel(officer)}";
            feedSignalText.text = OperationTabletRules.GetSignalLabel(
                source.SignalAvailable,
                streaming);
            feedSignalText.color = OperationTabletRules.GetSignalColor(
                source.SignalAvailable,
                streaming);
            feedNavigationText.text =
                $"FEED {selectedFeedIndex + 1} / {feeds.Count}   //   Q / E TO SWITCH";
            previousFeedButton.interactable = feeds.Count > 1;
            nextFeedButton.interactable = feeds.Count > 1;
        }

        private string GetAppliedEntryLabel()
        {
            if (deploymentCoordinator.DeploymentApplied)
            {
                return deploymentCoordinator.AppliedEntryPointId.ToUpperInvariant();
            }

            return string.IsNullOrWhiteSpace(OperationDeploymentContext.EntryPointId)
                ? "DIRECT PROTOTYPE ENTRY"
                : OperationDeploymentContext.EntryPointId.ToUpperInvariant();
        }

        private void HandleOpenTabletShortcuts()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard?.digit1Key.wasPressedThisFrame == true)
            {
                ShowSituation();
            }
            else if (keyboard?.digit2Key.wasPressedThisFrame == true)
            {
                ShowObjectives();
            }
            else if (keyboard?.digit3Key.wasPressedThisFrame == true)
            {
                ShowBodyCameras();
            }

            if (Page != OperationTabletPage.BodyCameras)
            {
                return;
            }

            if (keyboard?.qKey.wasPressedThisFrame == true
                || keyboard?.leftArrowKey.wasPressedThisFrame == true
                || Gamepad.current?.leftShoulder.wasPressedThisFrame == true)
            {
                SelectPreviousFeed();
            }
            else if (keyboard?.eKey.wasPressedThisFrame == true
                || keyboard?.rightArrowKey.wasPressedThisFrame == true
                || Gamepad.current?.rightShoulder.wasPressedThisFrame == true)
            {
                SelectNextFeed();
            }
        }

        private void UpdateTabPresentation()
        {
            Button[] tabs =
            {
                situationTabButton,
                objectivesTabButton,
                bodyCamerasTabButton
            };
            for (int index = 0; index < tabs.Length; index++)
            {
                Button tab = tabs[index];
                bool active = index == (int)Page;
                if (tab.targetGraphic is Image background)
                {
                    background.color = active ? ActiveTab : InactiveTab;
                }

                Text label = tab.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.color = active ? ActiveText : InactiveText;
                }
            }
        }

        private Button GetTabButton(OperationTabletPage page)
        {
            return page switch
            {
                OperationTabletPage.Situation => situationTabButton,
                OperationTabletPage.Objectives => objectivesTabButton,
                _ => bodyCamerasTabButton
            };
        }

        private void EnsureFeedTexture()
        {
            if (sharedFeedTexture != null
                && sharedFeedTexture.width == feedWidth
                && sharedFeedTexture.height == feedHeight)
            {
                if (!sharedFeedTexture.IsCreated())
                {
                    sharedFeedTexture.Create();
                }

                return;
            }

            ReleaseFeedTexture();
            sharedFeedTexture = new RenderTexture(
                feedWidth,
                feedHeight,
                24,
                RenderTextureFormat.ARGB32)
            {
                name = "ROE_OfficerBodyCamera_Live",
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false
            };
            sharedFeedTexture.Create();
        }

        private void ReleaseFeedTexture()
        {
            if (sharedFeedTexture == null)
            {
                return;
            }

            StopAllFeeds();
            sharedFeedTexture.Release();
            Destroy(sharedFeedTexture);
            sharedFeedTexture = null;
        }

        private void SetTabletVisible(bool visible)
        {
            if (tabletGroup == null)
            {
                return;
            }

            tabletGroup.alpha = visible ? 1f : 0f;
            tabletGroup.interactable = visible;
            tabletGroup.blocksRaycasts = visible;
            tabletGroup.gameObject.SetActive(visible);
        }

        private void WireControls()
        {
            situationTabButton?.onClick.AddListener(ShowSituation);
            objectivesTabButton?.onClick.AddListener(ShowObjectives);
            bodyCamerasTabButton?.onClick.AddListener(ShowBodyCameras);
            previousFeedButton?.onClick.AddListener(SelectPreviousFeed);
            nextFeedButton?.onClick.AddListener(SelectNextFeed);
            closeButton?.onClick.AddListener(CloseTablet);
        }

        private void UnwireControls()
        {
            situationTabButton?.onClick.RemoveListener(ShowSituation);
            objectivesTabButton?.onClick.RemoveListener(ShowObjectives);
            bodyCamerasTabButton?.onClick.RemoveListener(ShowBodyCameras);
            previousFeedButton?.onClick.RemoveListener(SelectPreviousFeed);
            nextFeedButton?.onClick.RemoveListener(SelectNextFeed);
            closeButton?.onClick.RemoveListener(CloseTablet);
        }

        private void OnDestroy()
        {
            UnwireControls();
            ReleaseFeedTexture();
        }

        private static void Select(Button button)
        {
            if (EventSystem.current != null && button != null && button.interactable)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }

        private void OnValidate()
        {
            feedWidth = Mathf.Max(320, feedWidth);
            feedHeight = Mathf.Max(180, feedHeight);
            refreshIntervalSeconds = Mathf.Max(0.02f, refreshIntervalSeconds);
        }
    }
}
