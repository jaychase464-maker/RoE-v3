using System;
using System.Collections.Generic;
using RulesOfEntry.Core;
using RulesOfEntry.Input;
using RulesOfEntry.Officers;
using UnityEngine;
using UnityEngine.UI;

namespace RulesOfEntry.UI.TacticalHud
{
    [DisallowMultipleComponent]
    public sealed class TacticalHudController : MonoBehaviour
    {
        private static readonly Color SuggestedCommandColor =
            new Color(0.025f, 0.26f, 0.43f, 0.96f);
        private static readonly Color DefaultCommandColor =
            new Color(0.015f, 0.02f, 0.025f, 0.94f);

        [Header("Runtime sources")]
        [SerializeField] private OfficerSquadController squad;
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private BodyCameraIdentity bodyCameraIdentity;
        [SerializeField] private MissionClock missionClock;

        [Header("Squad roster")]
        [SerializeField] private RectTransform rosterPanel;
        [SerializeField] private RectTransform rosterContent;
        [SerializeField] private TacticalHudOfficerRow officerRowTemplate;

        [Header("Body camera")]
        [SerializeField] private Graphic recordingDot;
        [SerializeField] private Text recordingHeaderText;
        [SerializeField] private Text officerIdentityText;
        [SerializeField] private Text departmentText;
        [SerializeField] private Text timestampText;
        [SerializeField] private Text batteryText;
        [SerializeField] private Text liveText;

        [Header("Command interface")]
        [SerializeField] private CanvasGroup commandGroup;
        [SerializeField] private Text commandTitleText;
        [SerializeField] private Image[] commandBackgrounds = Array.Empty<Image>();
        [SerializeField] private Text contextText;
        [SerializeField] private Text commandHintText;

        [Header("Refresh")]
        [SerializeField, Min(0.02f)] private float rosterRefreshSeconds = 0.1f;

        private readonly List<TacticalHudOfficerRow> rosterRows =
            new List<TacticalHudOfficerRow>(8);
        private float rosterRefreshTimer;
        private int lastOfficerCount = -1;
        private bool commandVisible;

        public bool HasCompleteConfiguration => squad != null
            && playerInput != null
            && bodyCameraIdentity != null
            && missionClock != null
            && rosterPanel != null
            && rosterContent != null
            && officerRowTemplate != null
            && recordingDot != null
            && recordingHeaderText != null
            && officerIdentityText != null
            && departmentText != null
            && timestampText != null
            && batteryText != null
            && liveText != null
            && commandGroup != null
            && commandTitleText != null
            && commandBackgrounds != null
            && commandBackgrounds.Length == 6
            && Array.TrueForAll(commandBackgrounds, image => image != null)
            && contextText != null
            && commandHintText != null;

        public void Configure(
            OfficerSquadController configuredSquad,
            TacticalPlayerInput configuredInput,
            BodyCameraIdentity configuredBodyCameraIdentity,
            MissionClock configuredMissionClock,
            RectTransform configuredRosterPanel,
            RectTransform configuredRosterContent,
            TacticalHudOfficerRow configuredOfficerRowTemplate,
            Graphic configuredRecordingDot,
            Text configuredRecordingHeaderText,
            Text configuredOfficerIdentityText,
            Text configuredDepartmentText,
            Text configuredTimestampText,
            Text configuredBatteryText,
            Text configuredLiveText,
            CanvasGroup configuredCommandGroup,
            Text configuredCommandTitleText,
            Image[] configuredCommandBackgrounds,
            Text configuredContextText,
            Text configuredCommandHintText)
        {
            Unsubscribe();
            squad = configuredSquad;
            playerInput = configuredInput;
            bodyCameraIdentity = configuredBodyCameraIdentity;
            missionClock = configuredMissionClock;
            rosterPanel = configuredRosterPanel;
            rosterContent = configuredRosterContent;
            officerRowTemplate = configuredOfficerRowTemplate;
            recordingDot = configuredRecordingDot;
            recordingHeaderText = configuredRecordingHeaderText;
            officerIdentityText = configuredOfficerIdentityText;
            departmentText = configuredDepartmentText;
            timestampText = configuredTimestampText;
            batteryText = configuredBatteryText;
            liveText = configuredLiveText;
            commandGroup = configuredCommandGroup;
            commandTitleText = configuredCommandTitleText;
            commandBackgrounds = configuredCommandBackgrounds ?? Array.Empty<Image>();
            contextText = configuredContextText;
            commandHintText = configuredCommandHintText;
            Subscribe();
            RebuildRoster();
            RefreshBodyCamera();
            SetCommandVisible(false);
        }

        public void ConfigureSources(
            OfficerSquadController configuredSquad,
            TacticalPlayerInput configuredInput,
            BodyCameraIdentity configuredBodyCameraIdentity,
            MissionClock configuredMissionClock)
        {
            Unsubscribe();
            squad = configuredSquad;
            playerInput = configuredInput;
            bodyCameraIdentity = configuredBodyCameraIdentity;
            missionClock = configuredMissionClock;
            Subscribe();
            RebuildRoster();
            RefreshBodyCamera();
            SetCommandVisible(false);
        }

        private void Awake()
        {
            SetCommandVisible(false);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Tactical HUD",
                    "HUD references are incomplete. Run the Tactical HUD setup tool outside Play Mode.",
                    this);
                return;
            }

            RebuildRoster();
            RefreshBodyCamera();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (!HasCompleteConfiguration)
            {
                return;
            }

            int officerCount = squad.Officers.Count;
            if (officerCount != lastOfficerCount)
            {
                RebuildRoster();
            }

            rosterRefreshTimer -= Time.unscaledDeltaTime;
            if (rosterRefreshTimer <= 0f)
            {
                rosterRefreshTimer = rosterRefreshSeconds;
                RefreshRoster();
                RefreshBodyCamera();
            }

            bool shouldShowCommands = playerInput.GameplayEnabled
                && playerInput.OfficerCommandMenuHeld;
            SetCommandVisible(shouldShowCommands);
            if (shouldShowCommands)
            {
                RefreshCommandContext();
            }
        }

        private void Subscribe()
        {
            if (squad != null)
            {
                squad.SelectionChanged -= OnSquadStateChanged;
                squad.SelectionChanged += OnSquadStateChanged;
                squad.CommandIssued -= OnSquadStateChanged;
                squad.CommandIssued += OnSquadStateChanged;
            }

            if (bodyCameraIdentity != null)
            {
                bodyCameraIdentity.Changed -= RefreshBodyCamera;
                bodyCameraIdentity.Changed += RefreshBodyCamera;
            }
        }

        private void Unsubscribe()
        {
            if (squad != null)
            {
                squad.SelectionChanged -= OnSquadStateChanged;
                squad.CommandIssued -= OnSquadStateChanged;
            }

            if (bodyCameraIdentity != null)
            {
                bodyCameraIdentity.Changed -= RefreshBodyCamera;
            }
        }

        private void OnSquadStateChanged()
        {
            RefreshRoster();
        }

        private void RebuildRoster()
        {
            foreach (TacticalHudOfficerRow row in rosterRows)
            {
                if (row != null)
                {
                    Destroy(row.gameObject);
                }
            }

            rosterRows.Clear();
            if (squad == null || rosterContent == null || officerRowTemplate == null)
            {
                lastOfficerCount = -1;
                return;
            }

            IReadOnlyList<TacticalOfficerController> officers = squad.Officers;
            for (int index = 0; index < officers.Count; index++)
            {
                TacticalOfficerController officer = officers[index];
                if (officer == null)
                {
                    continue;
                }

                TacticalHudOfficerRow row = Instantiate(officerRowTemplate, rosterContent);
                row.name = $"OfficerRow_{index + 1:00}_{officer.name}";
                row.gameObject.SetActive(true);
                row.Bind(officer, index);
                rosterRows.Add(row);
            }

            officerRowTemplate.gameObject.SetActive(false);
            lastOfficerCount = officers.Count;
            float desiredHeight = Mathf.Clamp(
                40f + rosterRows.Count * 46f,
                92f,
                420f);
            rosterPanel.sizeDelta = new Vector2(rosterPanel.sizeDelta.x, desiredHeight);
            RefreshRoster();
        }

        private void RefreshRoster()
        {
            foreach (TacticalHudOfficerRow row in rosterRows)
            {
                row?.Refresh();
            }
        }

        private void RefreshBodyCamera()
        {
            if (bodyCameraIdentity == null)
            {
                return;
            }

            bool recording = bodyCameraIdentity.Recording;
            if (recordingDot != null)
            {
                recordingDot.color = recording
                    ? new Color(0.88f, 0.12f, 0.1f, 1f)
                    : new Color(0.34f, 0.38f, 0.4f, 1f);
            }

            if (recordingHeaderText != null)
            {
                recordingHeaderText.text = recording
                    ? "REC      ROE BODY CAM"
                    : "STANDBY      ROE BODY CAM";
            }

            if (officerIdentityText != null)
            {
                officerIdentityText.text =
                    $"{bodyCameraIdentity.OfficerDisplayName.ToUpperInvariant()}  "
                    + $"[{bodyCameraIdentity.BadgeIdentifier.ToUpperInvariant()}]";
            }

            if (departmentText != null)
            {
                departmentText.text = bodyCameraIdentity.DepartmentName.ToUpperInvariant();
            }

            if (timestampText != null && missionClock != null)
            {
                timestampText.text = TacticalHudRules.FormatBodyCameraTimestamp(
                    missionClock.CurrentTimestamp);
            }

            if (batteryText != null)
            {
                batteryText.text = $"{bodyCameraIdentity.BatteryPercent}%";
            }

            if (liveText != null)
            {
                liveText.text = recording ? "LIVE" : "STANDBY";
            }
        }

        private void SetCommandVisible(bool visible)
        {
            if (commandGroup == null)
            {
                return;
            }

            if (commandVisible == visible
                && Mathf.Approximately(commandGroup.alpha, visible ? 1f : 0f))
            {
                return;
            }

            commandVisible = visible;
            commandGroup.alpha = visible ? 1f : 0f;
            commandGroup.interactable = false;
            commandGroup.blocksRaycasts = false;
        }

        private void RefreshCommandContext()
        {
            if (commandTitleText != null)
            {
                if (squad.SelectedOfficerIndex >= 0
                    && squad.SelectedOfficerIndex < squad.Officers.Count
                    && squad.Officers[squad.SelectedOfficerIndex]?.Identity != null)
                {
                    commandTitleText.text =
                        squad.Officers[squad.SelectedOfficerIndex]
                            .Identity.DisplayName.ToUpperInvariant()
                        + " COMMAND";
                }
                else
                {
                    commandTitleText.text = "TEAM COMMAND";
                }
            }

            int suggestion = -1;
            if (squad.TryGetCurrentCommandContext(out OfficerCommandContext context))
            {
                if (contextText != null)
                {
                    contextText.text =
                        $"FOCUS: {context.DisplayName.ToUpperInvariant()}\n"
                        + $"{context.DistanceMeters:0.0} M";
                }

                suggestion = TacticalHudRules.GetSuggestedCommandIndex(context.TargetType);
            }
            else if (contextText != null)
            {
                contextText.text = "FOCUS: NO VALID TARGET";
            }

            for (int index = 0; index < commandBackgrounds.Length; index++)
            {
                commandBackgrounds[index].color = index == suggestion
                    ? SuggestedCommandColor
                    : DefaultCommandColor;
            }

            if (commandHintText != null)
            {
                commandHintText.text =
                    "HOLD MMB  •  NUMBER KEY TO ISSUE  •  RELEASE TO CANCEL";
            }
        }
    }
}
