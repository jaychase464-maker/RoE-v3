using System;
using System.Linq;
using System.Text;
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
        [SerializeField] private Button closeButton;
        [SerializeField] private bool openOnStartWhenAvailable = true;

        private readonly StringBuilder builder = new StringBuilder(2048);

        public bool IsOpen { get; private set; }
        public bool OpenOnStartWhenAvailable => openOnStartWhenAvailable;
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
            closeButton = configuredCloseButton;
            openOnStartWhenAvailable = configuredOpenOnStart;
            SetVisible(false);
            Subscribe();
        }

        public bool OpenLatest()
        {
            if (!HasCompleteConfiguration
                || !CompletedOperationContext.TryGetLatest(out CompletedOperationRecord record))
            {
                return false;
            }

            Render(record);
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

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                return;
            }

            if (openOnStartWhenAvailable && CompletedOperationContext.HasCompletedOperation)
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
        }

        private void Subscribe()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseReview);
                closeButton.onClick.AddListener(CloseReview);
            }
        }

        private void Unsubscribe()
        {
            closeButton?.onClick.RemoveListener(CloseReview);
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
                + $"SESSION RECORD {record.SessionSequence:000}";
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
