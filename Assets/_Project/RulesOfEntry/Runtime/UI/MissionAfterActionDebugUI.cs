using System.Linq;
using System.Text;
using RulesOfEntry.Missions;
using UnityEngine;
using UnityEngine.UI;

namespace RulesOfEntry.UI
{
    /// <summary>
    /// Prototype-only visibility for objective and after-action evidence.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MissionAfterActionDebugUI : MonoBehaviour
    {
        [SerializeField] private MissionController missionController;
        [SerializeField] private Text reportText;

        private readonly StringBuilder builder = new StringBuilder(1024);

        public MissionController MissionController => missionController;
        public bool HasCompleteConfiguration => missionController != null
            && reportText != null;

        public void Configure(
            MissionController configuredController,
            Text configuredReportText)
        {
            missionController = configuredController;
            reportText = configuredReportText;
        }

        private void Awake()
        {
            missionController ??= Object.FindFirstObjectByType<MissionController>();
            reportText ??= GetComponentInChildren<Text>(true);
        }

        private void Update()
        {
            if (reportText == null)
            {
                return;
            }

            if (missionController == null)
            {
                reportText.text = "MISSION • controller unavailable";
                return;
            }

            MissionDefinition definition = missionController.Definition;
            AfterActionReport report = missionController.CurrentReport;
            builder.Clear();
            builder.Append("MISSION • ")
                .Append(definition != null ? definition.DisplayName : "Unassigned")
                .Append(" • ")
                .AppendLine(missionController.Phase.ToString());

            if (report == null)
            {
                builder.Append("Waiting for incident evidence…");
                reportText.text = builder.ToString();
                return;
            }

            foreach (MissionObjectiveEvaluation objective in report.Objectives)
            {
                string marker = objective.Status switch
                {
                    MissionObjectiveStatus.Completed => "[COMPLETE]",
                    MissionObjectiveStatus.Failed => "[FAILED]",
                    _ => "[PENDING]"
                };
                builder.Append(marker)
                    .Append(' ')
                    .Append(objective.DisplayName)
                    .AppendLine();
            }

            int violations = report.RoeFindings.Count(finding =>
                finding.Determination == RoeDetermination.Violation);
            int reviews = report.RoeFindings.Count(finding =>
                finding.Determination == RoeDetermination.ReviewRequired);
            builder.Append("ROE • ")
                .Append(violations)
                .Append(" violation(s) • ")
                .Append(reviews)
                .AppendLine(" review(s)");
            builder.Append(report.Final ? "FINAL" : "PROVISIONAL")
                .Append(" • Score ")
                .Append(report.Score);
            if (report.Final)
            {
                builder.Append(" • TIER ")
                    .Append(report.Tier)
                    .Append(" • ")
                    .Append(report.Rating);
            }
            else if (missionController.AutoCompletionPending)
            {
                builder.Append(" • AUTO-COMPLETE IN ")
                    .Append(missionController.AutoCompletionSecondsRemaining.ToString("0.0"))
                    .Append('S');
            }

            builder.AppendLine();
            int firstFindingIndex = Mathf.Max(0, report.RoeFindings.Count - 3);
            foreach (RoeFinding finding in report.RoeFindings.Skip(firstFindingIndex))
            {
                builder.Append("• ")
                    .Append(finding.Determination)
                    .Append(": ")
                    .Append(finding.Summary)
                    .AppendLine();
            }

            reportText.text = builder.ToString();
        }
    }
}
