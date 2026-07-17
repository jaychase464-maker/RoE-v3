using System;
using System.Collections;
using System.Linq;
using System.Text;
using RulesOfEntry.Core;
using RulesOfEntry.Input;
using RulesOfEntry.Missions;
using RulesOfEntry.Planning;
using RulesOfEntry.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.UI.Planning
{
    public enum TabletPlanningPage
    {
        Overview = 0,
        Objectives = 1,
        Intelligence = 2,
        MapAndEntry = 3,
        Team = 4,
        LoadoutAndSupport = 5,
        RoeAndReady = 6
    }

    [DisallowMultipleComponent]
    public sealed class RuggedTabletController : MonoBehaviour
    {
        private static readonly Color ActiveTabColor =
            new Color(0.055f, 0.31f, 0.47f, 1f);
        private static readonly Color InactiveTabColor =
            new Color(0.025f, 0.033f, 0.038f, 1f);
        private static readonly Color ActiveTabTextColor =
            new Color(0.94f, 0.98f, 1f, 1f);
        private static readonly Color InactiveTabTextColor =
            new Color(0.58f, 0.68f, 0.73f, 1f);

        [Header("Player control")]
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private CursorStateController cursorController;

        [Header("Tablet display")]
        [SerializeField] private OperationBriefingDefinition defaultBriefing;
        [SerializeField] private CanvasGroup tabletGroup;
        [SerializeField] private Text operationHeaderText;
        [SerializeField] private Text metadataText;
        [SerializeField] private Text leftTitleText;
        [SerializeField] private Text leftBodyText;
        [SerializeField] private Text rightTitleText;
        [SerializeField] private Text rightBodyText;
        [SerializeField] private Text selectionStatusText;
        [SerializeField] private Text deploymentStatusText;
        [SerializeField] private Image loadingProgressFill;

        [Header("Top navigation")]
        [SerializeField] private Button overviewTabButton;
        [SerializeField] private Button objectivesTabButton;
        [SerializeField] private Button intelligenceTabButton;
        [SerializeField] private Button mapTabButton;
        [SerializeField] private Button teamTabButton;
        [SerializeField] private Button loadoutTabButton;
        [SerializeField] private Button roeTabButton;

        [Header("Bottom controls")]
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text previousButtonText;
        [SerializeField] private Text nextButtonText;
        [SerializeField] private Text primaryButtonText;

        private OperationBriefingDefinition briefing;
        private bool[] assignedOfficers = Array.Empty<bool>();
        private bool[] selectedSupport = Array.Empty<bool>();
        private int officerIndex;
        private int supportIndex;
        private int entryIndex;
        private bool loading;

        public bool IsOpen => tabletGroup != null && tabletGroup.gameObject.activeSelf;
        public TabletPlanningPage Page { get; private set; } = TabletPlanningPage.Overview;
        public OperationBriefingDefinition Briefing => briefing;
        public OperationBriefingDefinition DefaultBriefing => defaultBriefing;
        public string SelectedEntryPointId => GetSelectedEntry()?.EntryPointId
            ?? string.Empty;
        public bool HasCompleteConfiguration => playerInput != null
            && cursorController != null
            && defaultBriefing != null
            && defaultBriefing.HasValidConfiguration
            && tabletGroup != null
            && operationHeaderText != null
            && metadataText != null
            && leftTitleText != null
            && leftBodyText != null
            && rightTitleText != null
            && rightBodyText != null
            && selectionStatusText != null
            && deploymentStatusText != null
            && loadingProgressFill != null
            && overviewTabButton != null
            && objectivesTabButton != null
            && intelligenceTabButton != null
            && mapTabButton != null
            && teamTabButton != null
            && loadoutTabButton != null
            && roeTabButton != null
            && previousButton != null
            && nextButton != null
            && primaryButton != null
            && closeButton != null
            && previousButtonText != null
            && nextButtonText != null
            && primaryButtonText != null;

        public void Configure(
            TacticalPlayerInput configuredPlayerInput,
            CursorStateController configuredCursorController,
            OperationBriefingDefinition configuredDefaultBriefing,
            CanvasGroup configuredTabletGroup,
            Text configuredOperationHeaderText,
            Text configuredMetadataText,
            Text configuredLeftTitleText,
            Text configuredLeftBodyText,
            Text configuredRightTitleText,
            Text configuredRightBodyText,
            Text configuredSelectionStatusText,
            Text configuredDeploymentStatusText,
            Image configuredLoadingProgressFill,
            Button configuredOverviewTabButton,
            Button configuredObjectivesTabButton,
            Button configuredIntelligenceTabButton,
            Button configuredMapTabButton,
            Button configuredTeamTabButton,
            Button configuredLoadoutTabButton,
            Button configuredRoeTabButton,
            Button configuredPreviousButton,
            Button configuredNextButton,
            Button configuredPrimaryButton,
            Button configuredCloseButton,
            Text configuredPreviousButtonText,
            Text configuredNextButtonText,
            Text configuredPrimaryButtonText)
        {
            playerInput = configuredPlayerInput;
            cursorController = configuredCursorController;
            defaultBriefing = configuredDefaultBriefing;
            tabletGroup = configuredTabletGroup;
            operationHeaderText = configuredOperationHeaderText;
            metadataText = configuredMetadataText;
            leftTitleText = configuredLeftTitleText;
            leftBodyText = configuredLeftBodyText;
            rightTitleText = configuredRightTitleText;
            rightBodyText = configuredRightBodyText;
            selectionStatusText = configuredSelectionStatusText;
            deploymentStatusText = configuredDeploymentStatusText;
            loadingProgressFill = configuredLoadingProgressFill;
            overviewTabButton = configuredOverviewTabButton;
            objectivesTabButton = configuredObjectivesTabButton;
            intelligenceTabButton = configuredIntelligenceTabButton;
            mapTabButton = configuredMapTabButton;
            teamTabButton = configuredTeamTabButton;
            loadoutTabButton = configuredLoadoutTabButton;
            roeTabButton = configuredRoeTabButton;
            previousButton = configuredPreviousButton;
            nextButton = configuredNextButton;
            primaryButton = configuredPrimaryButton;
            closeButton = configuredCloseButton;
            previousButtonText = configuredPreviousButtonText;
            nextButtonText = configuredNextButtonText;
            primaryButtonText = configuredPrimaryButtonText;
        }

        private void Awake()
        {
            OperationDeploymentContext.Clear();
            WireControls();
            SetTabletVisible(false);
        }

        private void Update()
        {
            if (loading)
            {
                return;
            }

            if (Keyboard.current?.tabKey.wasPressedThisFrame == true)
            {
                ToggleTablet();
                return;
            }

            if (!IsOpen)
            {
                return;
            }

            bool cancelPressed = Keyboard.current?.escapeKey.wasPressedThisFrame == true
                || Gamepad.current?.buttonEast.wasPressedThisFrame == true;
            if (cancelPressed)
            {
                CloseTablet();
            }
        }

        public void ToggleTablet()
        {
            if (loading)
            {
                return;
            }

            if (IsOpen)
            {
                CloseTablet();
                return;
            }

            OpenBriefing(defaultBriefing);
        }

        private void OnDestroy()
        {
            UnwireControls();
        }

        public void OpenBriefing(OperationBriefingDefinition configuredBriefing)
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Planning Tablet",
                    "Tablet references are incomplete. Run the Milestone 6A setup tool.",
                    this);
                return;
            }

            if (configuredBriefing == null || !configuredBriefing.HasValidConfiguration)
            {
                ProjectLog.Error(
                    "Planning Tablet",
                    "The selected operation briefing is invalid.",
                    this);
                return;
            }

            briefing = configuredBriefing;
            assignedOfficers = briefing.Officers
                .Select(officer => officer.Available && officer.AssignedByDefault)
                .ToArray();
            selectedSupport = new bool[briefing.SupportAssets.Length];
            officerIndex = 0;
            supportIndex = 0;
            entryIndex = 0;
            loading = false;
            Page = TabletPlanningPage.Overview;
            loadingProgressFill.fillAmount = 0f;
            deploymentStatusText.text = "BRIEFING STATUS: ACTIVE";
            SetTabletVisible(true);
            cursorController.SetCursorLocked(false);
            Refresh();
            Select(overviewTabButton);
        }

        public void CloseTablet()
        {
            if (!IsOpen || loading)
            {
                return;
            }

            SetTabletVisible(false);
            cursorController.SetCursorLocked(true);
        }

        public void ShowOverview() => SetPage(TabletPlanningPage.Overview);
        public void ShowObjectives() => SetPage(TabletPlanningPage.Objectives);
        public void ShowIntelligence() => SetPage(TabletPlanningPage.Intelligence);
        public void ShowMapAndEntry() => SetPage(TabletPlanningPage.MapAndEntry);
        public void ShowTeam() => SetPage(TabletPlanningPage.Team);
        public void ShowLoadoutAndSupport() => SetPage(TabletPlanningPage.LoadoutAndSupport);
        public void ShowRoeAndReady() => SetPage(TabletPlanningPage.RoeAndReady);

        public void SelectPrevious()
        {
            if (briefing == null || loading)
            {
                return;
            }

            switch (Page)
            {
                case TabletPlanningPage.MapAndEntry:
                    entryIndex = OperationPlanningRules.WrapIndex(
                        entryIndex, -1, briefing.EntryPoints.Length);
                    break;
                case TabletPlanningPage.Team:
                    officerIndex = OperationPlanningRules.WrapIndex(
                        officerIndex, -1, briefing.Officers.Length);
                    break;
                case TabletPlanningPage.LoadoutAndSupport:
                    supportIndex = OperationPlanningRules.WrapIndex(
                        supportIndex, -1, briefing.SupportAssets.Length);
                    break;
                default:
                    SetPage((TabletPlanningPage)Mathf.Max(0, (int)Page - 1));
                    return;
            }

            Refresh();
        }

        public void SelectNext()
        {
            if (briefing == null || loading)
            {
                return;
            }

            switch (Page)
            {
                case TabletPlanningPage.MapAndEntry:
                    entryIndex = OperationPlanningRules.WrapIndex(
                        entryIndex, 1, briefing.EntryPoints.Length);
                    break;
                case TabletPlanningPage.Team:
                    officerIndex = OperationPlanningRules.WrapIndex(
                        officerIndex, 1, briefing.Officers.Length);
                    break;
                case TabletPlanningPage.LoadoutAndSupport:
                    supportIndex = OperationPlanningRules.WrapIndex(
                        supportIndex, 1, briefing.SupportAssets.Length);
                    break;
                default:
                    SetPage((TabletPlanningPage)Mathf.Min(6, (int)Page + 1));
                    return;
            }

            Refresh();
        }

        public void ActivatePrimaryAction()
        {
            if (briefing == null || loading)
            {
                return;
            }

            switch (Page)
            {
                case TabletPlanningPage.Team:
                    ToggleOfficer();
                    break;
                case TabletPlanningPage.LoadoutAndSupport:
                    ToggleSupport();
                    break;
                case TabletPlanningPage.RoeAndReady:
                    BeginDeployment();
                    break;
                default:
                    SetPage((TabletPlanningPage)Mathf.Min(6, (int)Page + 1));
                    break;
            }
        }

        private void SetPage(TabletPlanningPage nextPage)
        {
            if (loading)
            {
                return;
            }

            Page = nextPage;
            deploymentStatusText.text = "BRIEFING STATUS: ACTIVE";
            Refresh();
            Select(GetTabButton(nextPage));
        }

        private void ToggleOfficer()
        {
            OperationOfficerDefinition[] officers = briefing.Officers;
            officerIndex = NormalizeIndex(officerIndex, officers.Length);
            if (!officers[officerIndex].Available)
            {
                deploymentStatusText.text = "OFFICER UNAVAILABLE";
                return;
            }

            assignedOfficers[officerIndex] = !assignedOfficers[officerIndex];
            deploymentStatusText.text = assignedOfficers[officerIndex]
                ? "OFFICER ASSIGNED"
                : "OFFICER REMOVED";
            Refresh();
        }

        private void ToggleSupport()
        {
            OperationSupportDefinition[] support = briefing.SupportAssets;
            supportIndex = NormalizeIndex(supportIndex, support.Length);
            OperationSupportDefinition selected = support[supportIndex];
            if (!selected.Available)
            {
                deploymentStatusText.text = selected.Implemented
                    ? "SUPPORT ASSET UNAVAILABLE FOR THIS OPERATION"
                    : "SUPPORT SYSTEM RESERVED FOR A FUTURE MILESTONE";
                return;
            }

            selectedSupport[supportIndex] = !selectedSupport[supportIndex];
            deploymentStatusText.text = selectedSupport[supportIndex]
                ? "SUPPORT ASSET ASSIGNED"
                : "SUPPORT ASSET REMOVED";
            Refresh();
        }

        private void BeginDeployment()
        {
            if (!OperationDeploymentContext.Confirm(
                briefing,
                SelectedEntryPointId,
                GetAssignedOfficerIds(),
                GetSelectedSupportIds()))
            {
                deploymentStatusText.text =
                    "DEPLOYMENT BLOCKED — SELECT AN ENTRY AND AT LEAST ONE OFFICER";
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(briefing.ScenePath))
            {
                OperationDeploymentContext.Clear();
                deploymentStatusText.text = "MISSION SCENE IS NOT IN BUILD SETTINGS";
                ProjectLog.Error(
                    "Planning Tablet",
                    $"Operation scene is unavailable: {briefing.ScenePath}",
                    this);
                return;
            }

            StartCoroutine(LoadOperation());
        }

        private IEnumerator LoadOperation()
        {
            loading = true;
            SetControlsInteractable(false);
            OperationEntryPointDefinition entry = GetSelectedEntry();
            deploymentStatusText.text = $"DEPLOYING: {briefing.Location.ToUpperInvariant()}";
            selectionStatusText.text =
                $"{briefing.OperationCode}  •  {entry.DisplayName.ToUpperInvariant()}";
            loadingProgressFill.fillAmount = 0f;

            AsyncOperation operation = SceneManager.LoadSceneAsync(
                briefing.ScenePath,
                LoadSceneMode.Single);
            if (operation == null)
            {
                loading = false;
                SetControlsInteractable(true);
                OperationDeploymentContext.Clear();
                deploymentStatusText.text = "DEPLOYMENT LOAD FAILED";
                yield break;
            }

            operation.allowSceneActivation = false;
            while (operation.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                loadingProgressFill.fillAmount = progress;
                deploymentStatusText.text = progress < 0.5f
                    ? "TRANSMITTING OPERATION PLAN"
                    : "STAGING RESPONSE TEAM";
                yield return null;
            }

            loadingProgressFill.fillAmount = 1f;
            deploymentStatusText.text = "TEAM READY — ENTERING OPERATION";
            yield return new WaitForSecondsRealtime(0.35f);
            operation.allowSceneActivation = true;
        }

        private void Refresh()
        {
            if (briefing == null)
            {
                return;
            }

            operationHeaderText.text =
                $"RULES OF ENTRY   //   {briefing.OperationCode}";
            metadataText.text =
                $"{briefing.Mission.DisplayName.ToUpperInvariant()}  /  {briefing.IncidentType.ToUpperInvariant()}  //  SECURE";

            switch (Page)
            {
                case TabletPlanningPage.Overview:
                    RefreshOverview();
                    break;
                case TabletPlanningPage.Objectives:
                    RefreshObjectives();
                    break;
                case TabletPlanningPage.Intelligence:
                    RefreshIntelligence();
                    break;
                case TabletPlanningPage.MapAndEntry:
                    RefreshMapAndEntry();
                    break;
                case TabletPlanningPage.Team:
                    RefreshTeam();
                    break;
                case TabletPlanningPage.LoadoutAndSupport:
                    RefreshLoadoutAndSupport();
                    break;
                case TabletPlanningPage.RoeAndReady:
                    RefreshRoeAndReady();
                    break;
            }

            UpdateTabPresentation();
        }

        private void RefreshOverview()
        {
            leftTitleText.text = "OPERATION OVERVIEW";
            leftBodyText.text =
                $"{briefing.Mission.DisplayName.ToUpperInvariant()}\n\nLOCATION\n{briefing.Location}\n\nDATE / TIME\n{briefing.Time}\n\nINCIDENT\n{briefing.IncidentType}";
            rightTitleText.text = "COMMAND SUMMARY";
            rightBodyText.text =
                $"CONDITIONS\n{briefing.Conditions}\n\nBRIEFING SUMMARY\n{briefing.Mission.Briefing}";
            SetStandardPageControls(string.Empty, "OBJECTIVES  →");
        }

        private void RefreshObjectives()
        {
            StringBuilder objectiveSummary = new StringBuilder();
            StringBuilder accountability = new StringBuilder();
            foreach (MissionObjectiveDefinition objective in briefing.Mission.Objectives)
            {
                objectiveSummary.Append("• ")
                    .Append(objective.DisplayName.ToUpperInvariant())
                    .Append("\n  ")
                    .Append(objective.Briefing)
                    .Append("\n\n");
                accountability.Append(objective.Required ? "REQUIRED" : "OPTIONAL")
                    .Append("  //  ")
                    .Append(objective.FailureDeduction)
                    .Append("-POINT FAILURE REVIEW\n");
            }

            leftTitleText.text = "MISSION OBJECTIVES";
            leftBodyText.text = objectiveSummary.ToString();
            rightTitleText.text = "ACCOUNTABILITY";
            rightBodyText.text =
                accountability + "\nObjective results are determined from factual custody, injury, room-clearance, officer, and force records.";
            SetStandardPageControls("OVERVIEW", "INTELLIGENCE  →");
        }

        private void RefreshIntelligence()
        {
            leftTitleText.text = "DISPATCH / INTELLIGENCE";
            leftBodyText.text = briefing.Intelligence;
            rightTitleText.text = "KNOWN / UNKNOWN";
            rightBodyText.text =
                "KNOWN\n• Patrol perimeter established\n• Armed subject reported\n• One employee unaccounted for\n\nUNCONFIRMED\n• Exact weapon and ammunition\n• Interior barricades\n• Subject location\n• Employee condition\n\nTreat incomplete intelligence as incomplete. Do not invent certainty.";
            SetStandardPageControls("OBJECTIVES", "MAP / ENTRY  →");
        }

        private void RefreshMapAndEntry()
        {
            OperationEntryPointDefinition[] entries = briefing.EntryPoints;
            entryIndex = NormalizeIndex(entryIndex, entries.Length);
            OperationEntryPointDefinition entry = entries[entryIndex];
            StringBuilder list = new StringBuilder();
            for (int index = 0; index < entries.Length; index++)
            {
                list.Append(index == entryIndex ? "> " : "  ")
                    .Append(index + 1)
                    .Append("  ")
                    .Append(entries[index].DisplayName.ToUpperInvariant())
                    .Append('\n');
            }

            leftTitleText.text = "SELECTED ENTRY PLAN";
            leftBodyText.text =
                $"{entry.DisplayName.ToUpperInvariant()}\n\nAPPROACH\n{entry.Approach}\n\nKNOWN RISK\n{entry.Risk}";
            rightTitleText.text = "LOCATION / FLOOR PLAN";
            rightBodyText.text =
                $"{briefing.Location.ToUpperInvariant()}\n\n{list}\nFLOOR PLAN STATUS\nUNVERIFIED — MISSION GREYBOX AND ENTRY ANCHORS WILL BE AUTHORED IN MILESTONE 6B.";
            selectionStatusText.text =
                $"ENTRY {entryIndex + 1} OF {entries.Length}  •  {entry.EntryPointId.ToUpperInvariant()}";
            previousButtonText.text = "PREVIOUS ENTRY";
            nextButtonText.text = "NEXT ENTRY";
            primaryButtonText.text = "CONFIRM ENTRY";
            SetBottomControlVisibility(true, true, true);
            SetNavigationAvailability(entries.Length > 1, entries.Length > 1, true);
        }

        private void RefreshTeam()
        {
            OperationOfficerDefinition[] officers = briefing.Officers;
            officerIndex = NormalizeIndex(officerIndex, officers.Length);
            StringBuilder roster = new StringBuilder();
            for (int index = 0; index < officers.Length; index++)
            {
                OperationOfficerDefinition officer = officers[index];
                string state = !officer.Available
                    ? "UNAVAILABLE"
                    : assignedOfficers[index] ? "ASSIGNED" : "STANDBY";
                roster.Append(index == officerIndex ? "> " : "  ")
                    .Append(officer.DisplayName.ToUpperInvariant())
                    .Append("  [")
                    .Append(state)
                    .Append("]\n")
                    .Append("    ")
                    .Append(officer.Role)
                    .Append('\n');
            }

            OperationOfficerDefinition selected = officers[officerIndex];
            leftTitleText.text = "RESPONSE TEAM";
            leftBodyText.text = roster.ToString();
            rightTitleText.text = "SELECTED OFFICER";
            rightBodyText.text =
                $"{selected.DisplayName.ToUpperInvariant()}\n\nROLE\n{selected.Role}\n\nQUALIFICATIONS\n{selected.Qualification}\n\nSTATUS\n{(selected.Available ? "AVAILABLE" : "UNAVAILABLE")}";
            selectionStatusText.text = $"{GetAssignedOfficerIds().Length} OFFICER(S) ASSIGNED";
            previousButtonText.text = "PREVIOUS OFFICER";
            nextButtonText.text = "NEXT OFFICER";
            primaryButtonText.text = assignedOfficers[officerIndex]
                ? "REMOVE FROM TEAM"
                : "ASSIGN TO TEAM";
            SetBottomControlVisibility(true, true, true);
            SetNavigationAvailability(
                officers.Length > 1,
                officers.Length > 1,
                selected.Available);
        }

        private void RefreshLoadoutAndSupport()
        {
            OperationSupportDefinition[] support = briefing.SupportAssets;
            supportIndex = NormalizeIndex(supportIndex, support.Length);
            StringBuilder list = new StringBuilder();
            for (int index = 0; index < support.Length; index++)
            {
                OperationSupportDefinition asset = support[index];
                string state = !asset.Implemented
                    ? "FUTURE SYSTEM"
                    : !asset.Available
                        ? "UNAVAILABLE"
                        : selectedSupport[index] ? "ASSIGNED" : "AVAILABLE";
                list.Append(index == supportIndex ? "> " : "  ")
                    .Append(asset.DisplayName.ToUpperInvariant())
                    .Append("  [")
                    .Append(state)
                    .Append("]\n");
            }

            OperationSupportDefinition selected = support[supportIndex];
            leftTitleText.text = "LOADOUT / SPECIALIZED SUPPORT";
            leftBodyText.text = list.ToString();
            rightTitleText.text = "CAPABILITY / AVAILABILITY";
            rightBodyText.text =
                $"{selected.DisplayName.ToUpperInvariant()}\n\n{selected.Capability}\n\nSYSTEM STATUS\n{(selected.Implemented ? "IMPLEMENTED" : "FUTURE GAMEPLAY MILESTONE")}\n\nOfficer weapon and equipment editing will be performed physically in the PD loadout area after that system is implemented.";
            selectionStatusText.text =
                "K9, DRONE, MEDIC, AND NEGOTIATOR RECORDS ARE PRESENT BUT CANNOT BE DEPLOYED YET";
            previousButtonText.text = "PREVIOUS ASSET";
            nextButtonText.text = "NEXT ASSET";
            primaryButtonText.text = selected.Available
                ? selectedSupport[supportIndex] ? "REMOVE SUPPORT" : "ASSIGN SUPPORT"
                : "NOT AVAILABLE";
            SetBottomControlVisibility(true, true, true);
            SetNavigationAvailability(
                support.Length > 1,
                support.Length > 1,
                selected.Available);
        }

        private void RefreshRoeAndReady()
        {
            OperationEntryPointDefinition entry = GetSelectedEntry();
            string officers = string.Join(
                ", ",
                briefing.Officers
                    .Where((officer, index) => index < assignedOfficers.Length
                        && assignedOfficers[index])
                    .Select(officer => officer.DisplayName));
            string support = string.Join(
                ", ",
                briefing.SupportAssets
                    .Where((asset, index) => index < selectedSupport.Length
                        && selectedSupport[index])
                    .Select(asset => asset.DisplayName));
            bool canDeploy = OperationPlanningRules.CanDeploy(
                briefing,
                entry.EntryPointId,
                GetAssignedOfficerIds());
            string officerSummary = string.IsNullOrWhiteSpace(officers)
                ? "NONE"
                : officers;
            string supportSummary = string.IsNullOrWhiteSpace(support)
                ? "NONE"
                : support;

            leftTitleText.text = "ROE / LEGAL AUTHORITY";
            leftBodyText.text =
                $"AUTHORITY\n{briefing.LegalAuthority}\n\nRULES OF ENGAGEMENT\n{briefing.RulesOfEngagement}";
            rightTitleText.text = "FINAL DEPLOYMENT PLAN";
            rightBodyText.text =
                $"ENTRY\n{entry.DisplayName}\n\nASSIGNED OFFICERS\n{officerSummary}\n\nSPECIALIZED SUPPORT\n{supportSummary}\n\nDESTINATION\n{briefing.Location}";
            selectionStatusText.text = canDeploy
                ? "BRIEFING STATUS: READY"
                : "BRIEFING STATUS: TEAM INCOMPLETE";
            previousButtonText.text = "LOADOUT";
            nextButtonText.text = "REVIEW COMPLETE";
            primaryButtonText.text = canDeploy
                ? "START OPERATION  →"
                : "TEAM INCOMPLETE";
            SetBottomControlVisibility(true, false, true);
            SetNavigationAvailability(true, false, canDeploy);
        }

        private void SetStandardPageControls(
            string previousLabel,
            string primaryLabel)
        {
            selectionStatusText.text =
                $"PAGE {(int)Page + 1} OF 7  •  {briefing.OperationCode}";
            previousButtonText.text = previousLabel;
            nextButtonText.text = string.Empty;
            primaryButtonText.text = primaryLabel;
            bool showPrevious = !string.IsNullOrWhiteSpace(previousLabel);
            SetBottomControlVisibility(showPrevious, false, true);
            SetNavigationAvailability(showPrevious, false, true);
        }

        private void SetBottomControlVisibility(
            bool showPrevious,
            bool showNext,
            bool showPrimary)
        {
            previousButton.gameObject.SetActive(showPrevious);
            nextButton.gameObject.SetActive(showNext);
            primaryButton.gameObject.SetActive(showPrimary);
        }

        private void UpdateTabPresentation()
        {
            Button[] tabs =
            {
                overviewTabButton,
                objectivesTabButton,
                intelligenceTabButton,
                mapTabButton,
                teamTabButton,
                loadoutTabButton,
                roeTabButton
            };

            for (int index = 0; index < tabs.Length; index++)
            {
                Button tab = tabs[index];
                if (tab == null)
                {
                    continue;
                }

                bool active = index == (int)Page;
                if (tab.targetGraphic is Image background)
                {
                    background.color = active ? ActiveTabColor : InactiveTabColor;
                }

                Text label = tab.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.color = active
                        ? ActiveTabTextColor
                        : InactiveTabTextColor;
                }
            }
        }

        private string[] GetAssignedOfficerIds()
        {
            return briefing?.Officers
                .Where((officer, index) => index < assignedOfficers.Length
                    && assignedOfficers[index]
                    && officer.Available)
                .Select(officer => officer.OfficerId)
                .ToArray() ?? Array.Empty<string>();
        }

        private string[] GetSelectedSupportIds()
        {
            return briefing?.SupportAssets
                .Where((support, index) => index < selectedSupport.Length
                    && selectedSupport[index]
                    && support.Available)
                .Select(support => support.SupportId)
                .ToArray() ?? Array.Empty<string>();
        }

        private OperationEntryPointDefinition GetSelectedEntry()
        {
            OperationEntryPointDefinition[] entries = briefing != null
                ? briefing.EntryPoints
                : Array.Empty<OperationEntryPointDefinition>();
            entryIndex = NormalizeIndex(entryIndex, entries.Length);
            return entryIndex >= 0 && entryIndex < entries.Length
                ? entries[entryIndex]
                : null;
        }

        private Button GetTabButton(TabletPlanningPage page)
        {
            return page switch
            {
                TabletPlanningPage.Overview => overviewTabButton,
                TabletPlanningPage.Objectives => objectivesTabButton,
                TabletPlanningPage.Intelligence => intelligenceTabButton,
                TabletPlanningPage.MapAndEntry => mapTabButton,
                TabletPlanningPage.Team => teamTabButton,
                TabletPlanningPage.LoadoutAndSupport => loadoutTabButton,
                _ => roeTabButton
            };
        }

        private static int NormalizeIndex(int index, int count)
        {
            return OperationPlanningRules.WrapIndex(index, 0, count);
        }

        private void SetNavigationAvailability(
            bool previousAvailable,
            bool nextAvailable,
            bool primaryAvailable)
        {
            previousButton.interactable = previousAvailable;
            nextButton.interactable = nextAvailable;
            primaryButton.interactable = primaryAvailable;
        }

        private void SetControlsInteractable(bool interactable)
        {
            overviewTabButton.interactable = interactable;
            objectivesTabButton.interactable = interactable;
            intelligenceTabButton.interactable = interactable;
            mapTabButton.interactable = interactable;
            teamTabButton.interactable = interactable;
            loadoutTabButton.interactable = interactable;
            roeTabButton.interactable = interactable;
            previousButton.interactable = interactable;
            nextButton.interactable = interactable;
            primaryButton.interactable = interactable;
            closeButton.interactable = interactable;
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
            overviewTabButton?.onClick.AddListener(ShowOverview);
            objectivesTabButton?.onClick.AddListener(ShowObjectives);
            intelligenceTabButton?.onClick.AddListener(ShowIntelligence);
            mapTabButton?.onClick.AddListener(ShowMapAndEntry);
            teamTabButton?.onClick.AddListener(ShowTeam);
            loadoutTabButton?.onClick.AddListener(ShowLoadoutAndSupport);
            roeTabButton?.onClick.AddListener(ShowRoeAndReady);
            previousButton?.onClick.AddListener(SelectPrevious);
            nextButton?.onClick.AddListener(SelectNext);
            primaryButton?.onClick.AddListener(ActivatePrimaryAction);
            closeButton?.onClick.AddListener(CloseTablet);
        }

        private void UnwireControls()
        {
            overviewTabButton?.onClick.RemoveListener(ShowOverview);
            objectivesTabButton?.onClick.RemoveListener(ShowObjectives);
            intelligenceTabButton?.onClick.RemoveListener(ShowIntelligence);
            mapTabButton?.onClick.RemoveListener(ShowMapAndEntry);
            teamTabButton?.onClick.RemoveListener(ShowTeam);
            loadoutTabButton?.onClick.RemoveListener(ShowLoadoutAndSupport);
            roeTabButton?.onClick.RemoveListener(ShowRoeAndReady);
            previousButton?.onClick.RemoveListener(SelectPrevious);
            nextButton?.onClick.RemoveListener(SelectNext);
            primaryButton?.onClick.RemoveListener(ActivatePrimaryAction);
            closeButton?.onClick.RemoveListener(CloseTablet);
        }

        private static void Select(Button button)
        {
            if (EventSystem.current != null && button != null && button.interactable)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }
    }
}
