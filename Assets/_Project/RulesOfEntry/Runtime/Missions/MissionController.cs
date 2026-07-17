using System;
using System.Linq;
using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.Missions
{
    [DisallowMultipleComponent]
    public sealed class MissionController : MonoBehaviour
    {
        [SerializeField] private MissionDefinition definition;
        [SerializeField] private RulesOfEngagementPolicy roePolicy;
        [SerializeField] private bool beginOnStart = true;
        [SerializeField, Min(0.1f)] private float evaluationIntervalSeconds = 0.25f;

        private double missionStartedAtSeconds;
        private float evaluationTimer;

        public event Action<AfterActionReport> ReportUpdated;
        public event Action<MissionPhase> PhaseChanged;

        public MissionDefinition Definition => definition;
        public RulesOfEngagementPolicy RoePolicy => roePolicy;
        public MissionPhase Phase { get; private set; } = MissionPhase.Briefing;
        public MissionEvidenceSnapshot CurrentEvidence { get; private set; } =
            MissionEvidenceSnapshot.Empty;
        public AfterActionReport CurrentReport { get; private set; }
        public bool HasCompleteConfiguration => definition != null
            && definition.HasValidConfiguration
            && roePolicy != null
            && roePolicy.HasValidConfiguration;

        public void Configure(
            MissionDefinition configuredDefinition,
            RulesOfEngagementPolicy configuredPolicy,
            bool configuredBeginOnStart,
            float configuredEvaluationIntervalSeconds)
        {
            definition = configuredDefinition;
            roePolicy = configuredPolicy;
            beginOnStart = configuredBeginOnStart;
            evaluationIntervalSeconds = Mathf.Max(
                0.1f,
                configuredEvaluationIntervalSeconds);
        }

        public bool BeginMission()
        {
            if (!HasCompleteConfiguration || Phase != MissionPhase.Briefing)
            {
                return false;
            }

            missionStartedAtSeconds = Time.timeAsDouble;
            Phase = MissionPhase.Active;
            evaluationTimer = 0f;
            PhaseChanged?.Invoke(Phase);
            ProjectLog.Info(
                "Mission",
                $"{definition.DisplayName} began under {roePolicy.DisplayName}.",
                this);
            EvaluateCurrentState(true);
            return true;
        }

        public bool RequestAfterActionReview()
        {
            if (Phase != MissionPhase.Active || !HasCompleteConfiguration)
            {
                return false;
            }

            CurrentEvidence = MissionEvidenceCollector.Capture(missionStartedAtSeconds);
            FinalizeCurrentEvidence("Operation end was confirmed at the debrief console.");
            return true;
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Mission",
                    $"{name} is missing a valid mission definition or ROE policy. "
                        + "Run the Milestone 5 setup tool.",
                    this);
                return;
            }

            if (beginOnStart)
            {
                BeginMission();
            }
        }

        private void Update()
        {
            if (Phase != MissionPhase.Active)
            {
                return;
            }

            evaluationTimer -= Time.deltaTime;
            if (evaluationTimer > 0f)
            {
                return;
            }

            evaluationTimer = evaluationIntervalSeconds;
            EvaluateCurrentState(true);
        }

        private void EvaluateCurrentState(bool allowFinalization)
        {
            CurrentEvidence = MissionEvidenceCollector.Capture(missionStartedAtSeconds);
            AfterActionReport provisional = AfterActionEvaluator.Evaluate(
                definition,
                roePolicy,
                CurrentEvidence,
                false);
            CurrentReport = provisional;

            bool allRequiredTerminal = provisional.Objectives
                .Where(objective => objective.Required)
                .All(objective => objective.Status != MissionObjectiveStatus.Pending);
            if (allowFinalization && allRequiredTerminal)
            {
                FinalizeCurrentEvidence("All required objectives reached a terminal state.");
                return;
            }

            ReportUpdated?.Invoke(CurrentReport);
        }

        private void FinalizeCurrentEvidence(string reason)
        {
            CurrentReport = AfterActionEvaluator.Evaluate(
                definition,
                roePolicy,
                CurrentEvidence,
                true);
            Phase = MissionPhase.AfterAction;
            PhaseChanged?.Invoke(Phase);
            ReportUpdated?.Invoke(CurrentReport);
            ProjectLog.Info(
                "After Action",
                $"{definition.DisplayName}: {CurrentReport.Rating}, "
                    + $"score {CurrentReport.Score}. {CurrentReport.Summary} {reason}",
                this);
        }
    }
}
