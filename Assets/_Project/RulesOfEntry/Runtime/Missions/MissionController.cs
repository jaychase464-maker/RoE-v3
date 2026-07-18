using System;
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
        [SerializeField] private bool autoCompleteWhenResolved = true;
        [SerializeField, Min(0f)] private float autoCompletionConfirmationSeconds = 3f;

        private double missionStartedAtSeconds;
        private double autoCompletionReadyAtSeconds = -1d;
        private float evaluationTimer;

        public event Action<AfterActionReport> ReportUpdated;
        public event Action<MissionPhase> PhaseChanged;

        public MissionDefinition Definition => definition;
        public RulesOfEngagementPolicy RoePolicy => roePolicy;
        public MissionPhase Phase { get; private set; } = MissionPhase.Briefing;
        public MissionEvidenceSnapshot CurrentEvidence { get; private set; } =
            MissionEvidenceSnapshot.Empty;
        public AfterActionReport CurrentReport { get; private set; }
        public bool AutoCompleteWhenResolved => autoCompleteWhenResolved;
        public float AutoCompletionConfirmationSeconds => autoCompletionConfirmationSeconds;
        public bool AutoCompletionPending => autoCompletionReadyAtSeconds >= 0d
            && Phase == MissionPhase.Active;
        public float AutoCompletionSecondsRemaining => !AutoCompletionPending
            ? 0f
            : Mathf.Max(
                0f,
                autoCompletionConfirmationSeconds
                    - (float)(Time.timeAsDouble - autoCompletionReadyAtSeconds));
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

        public void ConfigureAutomaticCompletion(
            bool configuredAutoCompleteWhenResolved,
            float configuredConfirmationSeconds)
        {
            autoCompleteWhenResolved = configuredAutoCompleteWhenResolved;
            autoCompletionConfirmationSeconds = Mathf.Max(
                0f,
                configuredConfirmationSeconds);
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
            autoCompletionReadyAtSeconds = -1d;
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
                        + "Run the current mission setup tool outside Play Mode.",
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

            MissionCompletionDecision completion = MissionCompletionRules.Evaluate(
                provisional,
                CurrentEvidence);
            if (!completion.Ready)
            {
                autoCompletionReadyAtSeconds = -1d;
            }
            else if (allowFinalization && autoCompleteWhenResolved)
            {
                if (autoCompletionReadyAtSeconds < 0d)
                {
                    autoCompletionReadyAtSeconds = Time.timeAsDouble;
                }

                if (Time.timeAsDouble - autoCompletionReadyAtSeconds
                    >= autoCompletionConfirmationSeconds)
                {
                    FinalizeCurrentEvidence(completion.Reason);
                    return;
                }
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
            autoCompletionReadyAtSeconds = -1d;
            PhaseChanged?.Invoke(Phase);
            ReportUpdated?.Invoke(CurrentReport);
            ProjectLog.Info(
                "After Action",
                $"{definition.DisplayName}: Tier {CurrentReport.Tier}, "
                    + $"score {CurrentReport.Score}/100. "
                    + $"{CurrentReport.Summary} {reason}",
                this);
        }
    }
}
