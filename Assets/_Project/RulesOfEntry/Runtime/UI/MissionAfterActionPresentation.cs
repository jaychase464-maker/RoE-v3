using System;
using System.Linq;
using System.Text;
using RulesOfEntry.Input;
using RulesOfEntry.Missions;
using UnityEngine;
using UnityEngine.UI;

namespace RulesOfEntry.UI
{
    /// <summary>
    /// Final mission presentation. It displays the immutable final report and suppresses
    /// gameplay input only after the mission controller has locked its evidence snapshot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MissionAfterActionPresentation : MonoBehaviour
    {
        [SerializeField] private MissionController missionController;
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private CanvasGroup presentationGroup;
        [SerializeField] private Text operationText;
        [SerializeField] private Text tierText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text categoryText;
        [SerializeField] private Text metricsText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text findingsText;
        [SerializeField] private Text footerText;

        private readonly StringBuilder builder = new StringBuilder(2048);
        private bool inputSuppressed;

        public MissionController MissionController => missionController;
        public bool HasCompleteConfiguration => missionController != null
            && presentationGroup != null
            && operationText != null
            && tierText != null
            && scoreText != null
            && categoryText != null
            && metricsText != null
            && objectiveText != null
            && findingsText != null
            && footerText != null;

        public void Configure(
            MissionController configuredController,
            TacticalPlayerInput configuredPlayerInput,
            CanvasGroup configuredGroup,
            Text configuredOperationText,
            Text configuredTierText,
            Text configuredScoreText,
            Text configuredCategoryText,
            Text configuredMetricsText,
            Text configuredObjectiveText,
            Text configuredFindingsText,
            Text configuredFooterText)
        {
            Unsubscribe();
            missionController = configuredController;
            playerInput = configuredPlayerInput;
            presentationGroup = configuredGroup;
            operationText = configuredOperationText;
            tierText = configuredTierText;
            scoreText = configuredScoreText;
            categoryText = configuredCategoryText;
            metricsText = configuredMetricsText;
            objectiveText = configuredObjectiveText;
            findingsText = configuredFindingsText;
            footerText = configuredFooterText;
            Subscribe();
            Refresh(missionController != null ? missionController.CurrentReport : null);
        }

        public void ConfigureSources(
            MissionController configuredController,
            TacticalPlayerInput configuredPlayerInput)
        {
            Unsubscribe();
            missionController = configuredController;
            playerInput = configuredPlayerInput;
            Subscribe();
            Refresh(missionController != null ? missionController.CurrentReport : null);
        }

        private void Awake()
        {
            missionController ??=
                UnityEngine.Object.FindFirstObjectByType<MissionController>();
            playerInput ??=
                UnityEngine.Object.FindFirstObjectByType<TacticalPlayerInput>();
            SetVisible(false);
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh(missionController != null ? missionController.CurrentReport : null);
        }

        private void OnDisable()
        {
            Unsubscribe();
            if (inputSuppressed && playerInput != null)
            {
                playerInput.SetGameplayEnabled(true);
                inputSuppressed = false;
            }
        }

        private void Subscribe()
        {
            if (missionController == null)
            {
                return;
            }

            missionController.ReportUpdated -= Refresh;
            missionController.ReportUpdated += Refresh;
        }

        private void Unsubscribe()
        {
            if (missionController != null)
            {
                missionController.ReportUpdated -= Refresh;
            }
        }

        private void Refresh(AfterActionReport report)
        {
            if (report == null || !report.Final)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            if (playerInput != null && playerInput.GameplayEnabled)
            {
                playerInput.SetGameplayEnabled(false);
                inputSuppressed = true;
            }

            operationText.text = report.MissionName.ToUpperInvariant();
            tierText.text = report.Tier.ToString();
            scoreText.text = $"{report.Score:000} / 100";
            categoryText.text = BuildCategories(report);
            metricsText.text = BuildMetrics(report);
            objectiveText.text = BuildObjectives(report);
            findingsText.text = BuildFindings(report);
            footerText.text =
                $"FINAL EVIDENCE LOCK  //  ELAPSED {FormatDuration(report.ElapsedSeconds)}  //  "
                + $"SCORE CAP {report.ScoreCap:000}";
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

        private string BuildMetrics(AfterActionReport report)
        {
            MissionOutcomeMetrics metrics = report.Metrics;
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
            RoeFinding[] violations = report.RoeFindings
                .Where(finding => finding.Determination != RoeDetermination.WithinPolicy)
                .ToArray();
            builder.Clear();
            builder.AppendLine("ROE / ACCOUNTABILITY");
            if (violations.Length == 0)
            {
                builder.Append("NO POLICY EXCEPTIONS RECORDED");
                return builder.ToString();
            }

            foreach (RoeFinding finding in violations.Take(6))
            {
                builder.Append('[')
                    .Append(finding.Determination.ToString().ToUpperInvariant())
                    .Append("]  ")
                    .Append(finding.Summary.ToUpperInvariant())
                    .Append('\n');
            }

            if (violations.Length > 6)
            {
                builder.Append("+ ")
                    .Append(violations.Length - 6)
                    .Append(" ADDITIONAL FINDING(S)");
            }

            return builder.ToString();
        }

        private void SetVisible(bool visible)
        {
            if (presentationGroup == null)
            {
                return;
            }

            presentationGroup.alpha = visible ? 1f : 0f;
            presentationGroup.interactable = false;
            presentationGroup.blocksRaycasts = visible;
        }

        private static string FormatDuration(double seconds)
        {
            TimeSpan duration = TimeSpan.FromSeconds(Math.Max(0d, seconds));
            return $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}";
        }
    }
}
