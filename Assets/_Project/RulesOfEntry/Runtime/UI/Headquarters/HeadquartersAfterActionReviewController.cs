using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RulesOfEntry.Campaign;
using RulesOfEntry.Input;
using RulesOfEntry.Missions;
using RulesOfEntry.Operations;
using RulesOfEntry.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RulesOfEntry.UI.Headquarters
{
    [DisallowMultipleComponent]
    public sealed class HeadquartersAfterActionReviewController : MonoBehaviour
    {
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private CursorStateController cursorController;
        [SerializeField] private GameObject interfaceRoot;
        [SerializeField] private CanvasGroup interfaceGroup;
        [SerializeField] private Text operationText;
        [SerializeField] private Text tierText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text categoryText;
        [SerializeField] private Text outcomeText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text findingsText;
        [SerializeField] private Text metadataText;
        [SerializeField] private Text navigationText;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private bool openOnStartWhenAvailable = true;

        private readonly StringBuilder builder = new StringBuilder(2048);
        private readonly List<CompletedOperationRecord> availableRecords =
            new List<CompletedOperationRecord>(16);
        private int currentRecordIndex = -1;

        public bool IsOpen { get; private set; }
        public bool OpenOnStartWhenAvailable => openOnStartWhenAvailable;
        public bool HasAvailableReports => CompletedOperationContext.HasCompletedOperation
            || CampaignSession.ActiveCampaign?.CompletedOperationCount > 0;
        public bool HasCompleteConfiguration => playerInput != null
            && cursorController != null
            && interfaceRoot != null
            && interfaceGroup != null
            && operationText != null
            && tierText != null
            && scoreText != null
            && categoryText != null
            && outcomeText != null
            && objectiveText != null
            && findingsText != null
            && metadataText != null
            && navigationText != null
            && previousButton != null
            && nextButton != null
            && closeButton != null;

        public void Configure(
            TacticalPlayerInput configuredInput,
            CursorStateController configuredCursor,
            GameObject configuredInterfaceRoot,
            CanvasGroup configuredInterfaceGroup,
            Text configuredOperationText,
            Text configuredTierText,
            Text configuredScoreText,
            Text configuredCategoryText,
            Text configuredOutcomeText,
            Text configuredObjectiveText,
            Text configuredFindingsText,
            Text configuredMetadataText,
            Text configuredNavigationText,
            Button configuredPreviousButton,
            Button configuredNextButton,
            Button configuredCloseButton,
            bool configuredOpenOnStart)
        {
            Unsubscribe();
            playerInput = configuredInput;
            cursorController = configuredCursor;
            interfaceRoot = configuredInterfaceRoot;
            interfaceGroup = configuredInterfaceGroup;
            operationText = configuredOperationText;
            tierText = configuredTierText;
            scoreText = configuredScoreText;
            categoryText = configuredCategoryText;
            outcomeText = configuredOutcomeText;
            objectiveText = configuredObjectiveText;
            findingsText = configuredFindingsText;
            metadataText = configuredMetadataText;
            navigationText = configuredNavigationText;
            previousButton = configuredPreviousButton;
            nextButton = configuredNextButton;
            closeButton = configuredCloseButton;
            openOnStartWhenAvailable = configuredOpenOnStart;
            SetVisible(false);
            Subscribe();
        }

        public bool OpenLatest()
        {
            if (!HasCompleteConfiguration)
            {
                return false;
            }

            RebuildAvailableRecords();
            if (availableRecords.Count == 0)
            {
                return false;
            }

            currentRecordIndex = availableRecords.Count - 1;
            RenderCurrent();
            SetVisible(true);
            cursorController.SetCursorLocked(false);
            closeButton.Select();
            return true;
        }

        public void CloseReview()
        {
            if (!IsOpen)
            {
                return;
            }

            SetVisible(false);
            cursorController.SetCursorLocked(true);
        }

        public void ShowPreviousReport()
        {
            if (!IsOpen || availableRecords.Count <= 1)
            {
                return;
            }

            currentRecordIndex = CampaignDataRules.WrapArchiveIndex(
                currentRecordIndex - 1,
                availableRecords.Count);
            RenderCurrent();
        }

        public void ShowNextReport()
        {
            if (!IsOpen || availableRecords.Count <= 1)
            {
                return;
            }

            currentRecordIndex = CampaignDataRules.WrapArchiveIndex(
                currentRecordIndex + 1,
                availableRecords.Count);
            RenderCurrent();
        }

        public bool TryGetLatestSummary(
            out string operationCode,
            out MissionPerformanceTier tier)
        {
            RebuildAvailableRecords();
            if (availableRecords.Count == 0)
            {
                operationCode = string.Empty;
                tier = MissionPerformanceTier.NotRated;
                return false;
            }

            CompletedOperationRecord latest = availableRecords[availableRecords.Count - 1];
            operationCode = latest.OperationCode;
            tier = latest.Report.Tier;
            return true;
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                return;
            }

            if (openOnStartWhenAvailable
                && CompletedOperationContext.HasCompletedOperation)
            {
                OpenLatest();
            }
            else
            {
                SetVisible(false);
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            if (keyboard?.escapeKey.wasPressedThisFrame == true
                || keyboard?.tabKey.wasPressedThisFrame == true
                || gamepad?.buttonEast.wasPressedThisFrame == true)
            {
                CloseReview();
            }
            else if (keyboard?.leftArrowKey.wasPressedThisFrame == true
                || gamepad?.leftShoulder.wasPressedThisFrame == true
                || gamepad?.dpad.left.wasPressedThisFrame == true)
            {
                ShowPreviousReport();
            }
            else if (keyboard?.rightArrowKey.wasPressedThisFrame == true
                || gamepad?.rightShoulder.wasPressedThisFrame == true
                || gamepad?.dpad.right.wasPressedThisFrame == true)
            {
                ShowNextReport();
            }
        }

        private void Subscribe()
        {
            if (previousButton != null)
            {
                previousButton.onClick.RemoveListener(ShowPreviousReport);
                previousButton.onClick.AddListener(ShowPreviousReport);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(ShowNextReport);
                nextButton.onClick.AddListener(ShowNextReport);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseReview);
                closeButton.onClick.AddListener(CloseReview);
            }
        }

        private void Unsubscribe()
        {
            previousButton?.onClick.RemoveListener(ShowPreviousReport);
            nextButton?.onClick.RemoveListener(ShowNextReport);
            closeButton?.onClick.RemoveListener(CloseReview);
        }

        private void RebuildAvailableRecords()
        {
            availableRecords.Clear();
            CampaignSaveData campaign = CampaignSession.ActiveCampaign;
            if (campaign?.completedOperations != null)
            {
                foreach (CampaignOperationRecordData saved in campaign.completedOperations)
                {
                    try
                    {
                        availableRecords.Add(saved.ToCompletedOperationRecord());
                    }
                    catch (Exception)
                    {
                        // The save codec validates records before the session accepts them.
                    }
                }
            }

            if (CompletedOperationContext.TryGetLatest(out CompletedOperationRecord latest)
                && availableRecords.All(record => !string.Equals(
                    record.RecordId,
                    latest.RecordId,
                    StringComparison.Ordinal)))
            {
                availableRecords.Add(latest);
            }
        }

        private void RenderCurrent()
        {
            if (currentRecordIndex < 0 || currentRecordIndex >= availableRecords.Count)
            {
                return;
            }

            Render(availableRecords[currentRecordIndex]);
            bool canBrowse = availableRecords.Count > 1;
            previousButton.interactable = canBrowse;
            nextButton.interactable = canBrowse;
            navigationText.text =
                $"OPERATION {currentRecordIndex + 1:00} / {availableRecords.Count:00}  //  "
                + "LEFT / RIGHT OR LB / RB";
        }

        private void Render(CompletedOperationRecord record)
        {
            AfterActionReport report = record.Report;
            operationText.text = report.MissionName.ToUpperInvariant();
            tierText.text = report.Tier.ToString();
            scoreText.text = $"{report.Score:000} / 100";
            categoryText.text = BuildCategories(report);
            outcomeText.text = BuildOutcome(report.Metrics);
            objectiveText.text = BuildObjectives(report);
            findingsText.text = BuildFindings(report);
            metadataText.text =
                $"{record.OperationCode.ToUpperInvariant()}  //  ENTRY "
                + $"{DisplayOrUnknown(record.EntryPointId)}  //  "
                + $"TEAM {record.AssignedOfficerIds.Count}  //  "
                + $"ELAPSED {FormatDuration(report.ElapsedSeconds)}  //  "
                + $"ARCHIVE RECORD {record.SessionSequence:000}";
        }

        private string BuildCategories(AfterActionReport report)
        {
            builder.Clear();
            builder.AppendLine("PERFORMANCE BREAKDOWN");
            foreach (MissionScoreCategory category in report.Categories)
            {
                builder.Append(category.DisplayName)
                    .Append("  ")
                    .Append(category.EarnedScore.ToString("00"))
                    .Append(" / ")
                    .Append(category.MaximumScore.ToString("00"))
                    .Append('\n')
                    .Append(category.Summary)
                    .Append("\n\n");
            }

            return builder.ToString();
        }

        private static string BuildOutcome(MissionOutcomeMetrics metrics)
        {
            return "INCIDENT OUTCOME\n"
                + $"CIVILIANS SAVED     {metrics.CiviliansSaved} / {metrics.CiviliansTotal}\n"
                + $"CIVILIANS KILLED    {metrics.CiviliansKilled}\n"
                + $"SUSPECTS ARRESTED   {metrics.SuspectsArrested} / {metrics.SuspectsTotal}\n"
                + $"SUSPECTS KILLED     {metrics.SuspectsKilled}\n"
                + $"OFFICERS WOUNDED    {metrics.OfficersWounded}\n"
                + $"OFFICERS KILLED     {metrics.OfficersKilled}\n"
                + $"EVIDENCE SECURED    {metrics.EvidenceItemsSecured} / "
                + $"{metrics.EvidenceOpportunities}";
        }

        private string BuildObjectives(AfterActionReport report)
        {
            builder.Clear();
            builder.AppendLine("OBJECTIVE REVIEW");
            foreach (MissionObjectiveEvaluation objective in report.Objectives)
            {
                builder.Append(objective.Status == MissionObjectiveStatus.Completed
                        ? "[COMPLETE]  "
                        : "[FAILED]    ")
                    .Append(objective.DisplayName.ToUpperInvariant())
                    .Append('\n');
            }

            return builder.ToString();
        }

        private string BuildFindings(AfterActionReport report)
        {
            RoeFinding[] findings = report.RoeFindings
                .Where(finding => finding.Determination != RoeDetermination.WithinPolicy)
                .ToArray();
            builder.Clear();
            builder.AppendLine("ROE / ACCOUNTABILITY");
            if (findings.Length == 0)
            {
                builder.Append("NO POLICY EXCEPTIONS RECORDED");
                return builder.ToString();
            }

            foreach (RoeFinding finding in findings.Take(6))
            {
                builder.Append('[')
                    .Append(finding.Determination.ToString().ToUpperInvariant())
                    .Append("]  ")
                    .Append(finding.Summary.ToUpperInvariant())
                    .Append('\n');
            }

            return builder.ToString();
        }

        private void SetVisible(bool visible)
        {
            IsOpen = visible;
            if (interfaceRoot != null)
            {
                interfaceRoot.SetActive(visible);
            }

            if (interfaceGroup != null)
            {
                interfaceGroup.alpha = visible ? 1f : 0f;
                interfaceGroup.interactable = visible;
                interfaceGroup.blocksRaycasts = visible;
            }
        }

        private static string DisplayOrUnknown(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "DIRECT"
                : value.ToUpperInvariant();
        }

        private static string FormatDuration(double seconds)
        {
            TimeSpan duration = TimeSpan.FromSeconds(Math.Max(0d, seconds));
            return $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}";
        }
    }
}
