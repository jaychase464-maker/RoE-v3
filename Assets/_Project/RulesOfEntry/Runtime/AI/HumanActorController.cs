using RulesOfEntry.Actors;
using RulesOfEntry.Core;
using UnityEngine;
using UnityEngine.AI;

namespace RulesOfEntry.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ActorIdentity))]
    [RequireComponent(typeof(ActorCondition))]
    [RequireComponent(typeof(ActorInventory))]
    [RequireComponent(typeof(CustodyComponent))]
    [RequireComponent(typeof(HumanPerception))]
    [RequireComponent(typeof(HumanDecisionLedger))]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class HumanActorController : MonoBehaviour, IVerbalCommandReceiver
    {
        [Header("Authoritative actor references")]
        [SerializeField] private ActorIdentity identity;
        [SerializeField] private ActorCondition condition;
        [SerializeField] private ActorInventory inventory;
        [SerializeField] private CustodyComponent custody;
        [SerializeField] private HumanPerception perception;
        [SerializeField] private HumanDecisionLedger decisionLedger;

        [Header("Decision and presentation")]
        [SerializeField] private HumanBehaviorProfile profile;
        [SerializeField] private NavMeshAgent navigationAgent;
        [SerializeField] private ActorVisual actorVisual;
        [SerializeField] private Transform officerTarget;
        [SerializeField, Range(0f, 1f)] private float initialStress = 0.35f;
        [SerializeField, Range(0f, 1f)] private float initialMorale = 0.62f;
        [SerializeField, Min(0.1f)] private float decisionIntervalSeconds = 0.25f;

        private DeterministicDecisionRandom random;
        private float decisionTimer;
        private float deceptiveSurrenderTimer;
        private float pendingReactionTimer;
        private CommandDecision pendingDecision;
        private VerbalCommandStimulus pendingStimulus;
        private bool hasPendingReaction;
        private bool initialized;
        private bool subscriptionsActive;

        public ActorIdentity Identity => identity;
        public ActorCondition Condition => condition;
        public ActorInventory Inventory => inventory;
        public CustodyComponent Custody => custody;
        public HumanBehaviorState State { get; private set; } = HumanBehaviorState.Idle;
        public HumanDecisionReason LastDecisionReason { get; private set; }
        public HumanDecisionRecord LastDecision { get; private set; }
        public float Stress { get; private set; }
        public float Morale { get; private set; }
        public bool IsDeceptiveSurrender { get; private set; }
        public bool HasCompleteConfiguration => identity != null
            && condition != null
            && inventory != null
            && custody != null
            && perception != null
            && decisionLedger != null
            && profile != null
            && navigationAgent != null
            && actorVisual != null;

        public void Configure(
            ActorIdentity configuredIdentity,
            ActorCondition configuredCondition,
            ActorInventory configuredInventory,
            CustodyComponent configuredCustody,
            HumanPerception configuredPerception,
            HumanDecisionLedger configuredDecisionLedger,
            HumanBehaviorProfile configuredProfile,
            NavMeshAgent configuredAgent,
            ActorVisual configuredVisual,
            Transform configuredOfficerTarget,
            float configuredInitialStress,
            float configuredInitialMorale)
        {
            identity = configuredIdentity;
            condition = configuredCondition;
            inventory = configuredInventory;
            custody = configuredCustody;
            perception = configuredPerception;
            decisionLedger = configuredDecisionLedger;
            profile = configuredProfile;
            navigationAgent = configuredAgent;
            actorVisual = configuredVisual;
            officerTarget = configuredOfficerTarget;
            initialStress = Mathf.Clamp01(configuredInitialStress);
            initialMorale = Mathf.Clamp01(configuredInitialMorale);
            initialized = false;
        }

        public void ConfigureOfficerTarget(Transform configuredOfficerTarget)
        {
            officerTarget = configuredOfficerTarget;
        }

        public HumanDecisionRecord ReceiveVerbalCommand(VerbalCommandStimulus stimulus)
        {
            if (!EnsureInitialized())
            {
                return null;
            }

            bool perceivedCommand = perception.CanPerceiveCommand(stimulus);
            float distance = Vector3.Distance(transform.position, stimulus.SourcePosition);
            if (perceivedCommand)
            {
                Stress = Mathf.Clamp01(Stress + (stimulus.WeaponPresented ? 0.18f : 0.1f));
                Morale = Mathf.Clamp01(Morale - (stimulus.WeaponPresented ? 0.12f : 0.06f));
            }

            CommandDecisionContext context = new CommandDecisionContext(
                identity.Role,
                stimulus.Command,
                perceivedCommand,
                custody.IsRestrained,
                condition.Snapshot.CanAct,
                stimulus.WeaponPresented,
                inventory.HasWeapon,
                distance,
                Stress,
                Morale,
                condition.Snapshot.Level,
                profile);
            CommandDecision decision = HumanDecisionModel.EvaluateCommand(
                context,
                random.Next01(),
                random.Next01(),
                random.Next01());
            HumanBehaviorState previousState = State;
            bool reactionRequired = perceivedCommand
                && !custody.IsRestrained
                && condition.Snapshot.CanAct
                && decision.State != State;
            if (reactionRequired)
            {
                pendingDecision = decision;
                pendingStimulus = stimulus;
                pendingReactionTimer = Mathf.Lerp(
                    profile.MinimumReactionSeconds,
                    profile.MaximumReactionSeconds,
                    decision.DecisionRoll);
                hasPendingReaction = true;
            }
            else
            {
                ApplyDecision(decision, stimulus);
            }

            HumanBehaviorState recordedState = reactionRequired ? decision.State : State;
            CommandDecision recordedDecision = new CommandDecision(
                recordedState,
                decision.Reason,
                decision.Deceptive,
                decision.ComplianceScore,
                decision.DecisionRoll);
            LastDecision = decisionLedger.Record(
                stimulus.Command,
                previousState,
                recordedDecision,
                Stress,
                Morale);
            LastDecisionReason = decision.Reason;
            RefreshPresentation();
            return LastDecision;
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            if (!EnsureInitialized())
            {
                ProjectLog.Error(
                    "Human AI",
                    $"{name} is missing Milestone 3 actor references. Run the Milestone 3 setup tool.",
                    this);
            }
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (!EnsureInitialized())
            {
                return;
            }

            UpdatePendingReaction(Time.deltaTime);
            decisionTimer -= Time.deltaTime;
            if (decisionTimer <= 0f)
            {
                decisionTimer = decisionIntervalSeconds;
                EvaluateWorldState(decisionIntervalSeconds);
            }

            UpdateMovement();
            UpdateDeceptiveSurrender(Time.deltaTime);
        }

        private void ResolveReferences()
        {
            identity ??= GetComponent<ActorIdentity>();
            condition ??= GetComponent<ActorCondition>();
            inventory ??= GetComponent<ActorInventory>();
            custody ??= GetComponent<CustodyComponent>();
            perception ??= GetComponent<HumanPerception>();
            decisionLedger ??= GetComponent<HumanDecisionLedger>();
            navigationAgent ??= GetComponent<NavMeshAgent>();
            actorVisual ??= GetComponent<ActorVisual>();
        }

        private bool EnsureInitialized()
        {
            if (initialized)
            {
                return true;
            }

            ResolveReferences();
            if (!HasCompleteConfiguration)
            {
                return false;
            }

            int seed = BuildStableSeed(identity.IncidentSeed, identity.ActorId);
            random = new DeterministicDecisionRandom(seed);
            Stress = initialStress;
            Morale = initialMorale;
            navigationAgent.speed = profile.MovementSpeed;
            initialized = true;
            Subscribe();
            RefreshPresentation();
            return true;
        }

        private void Subscribe()
        {
            if (subscriptionsActive || condition == null || custody == null)
            {
                return;
            }

            condition.ConditionChanged += OnConditionChanged;
            custody.StateChanged += OnCustodyStateChanged;
            subscriptionsActive = true;
        }

        private void Unsubscribe()
        {
            if (!subscriptionsActive)
            {
                return;
            }

            if (condition != null)
            {
                condition.ConditionChanged -= OnConditionChanged;
            }

            if (custody != null)
            {
                custody.StateChanged -= OnCustodyStateChanged;
            }

            subscriptionsActive = false;
        }

        private void ApplyDecision(CommandDecision decision, VerbalCommandStimulus stimulus)
        {
            if (decision.Reason == HumanDecisionReason.CommandNotPerceived)
            {
                return;
            }

            State = decision.State;
            IsDeceptiveSurrender = decision.Deceptive;
            switch (decision.State)
            {
                case HumanBehaviorState.Surrendering:
                    StopNavigation();
                    custody.TryBeginSurrender(stimulus.Source, decision.Reason.ToString());
                    deceptiveSurrenderTimer = decision.Deceptive
                        ? random.Range(5f, 8.5f)
                        : 0f;
                    break;
                case HumanBehaviorState.Fleeing:
                case HumanBehaviorState.Hiding:
                    MoveAwayFrom(stimulus.SourcePosition, decision.State == HumanBehaviorState.Hiding);
                    break;
                case HumanBehaviorState.Threatening:
                case HumanBehaviorState.Resisting:
                case HumanBehaviorState.Frozen:
                    StopNavigation();
                    break;
            }
        }

        private void UpdatePendingReaction(float deltaTime)
        {
            if (!hasPendingReaction)
            {
                return;
            }

            if (custody.IsRestrained || !condition.Snapshot.CanAct)
            {
                hasPendingReaction = false;
                return;
            }

            pendingReactionTimer -= deltaTime;
            if (pendingReactionTimer > 0f)
            {
                return;
            }

            hasPendingReaction = false;
            ApplyDecision(pendingDecision, pendingStimulus);
            RefreshPresentation();
        }

        private void EvaluateWorldState(float elapsedSeconds)
        {
            if (custody.IsRestrained || !condition.Snapshot.CanAct)
            {
                return;
            }

            bool seesOfficer = perception.CanSee(officerTarget);
            float stressDelta = seesOfficer
                ? (identity.Role == ActorRole.Civilian ? 0.09f : 0.045f) * elapsedSeconds
                : -0.035f * elapsedSeconds;
            Stress = Mathf.Clamp01(Stress + stressDelta);

            if (seesOfficer && State == HumanBehaviorState.Idle)
            {
                ChangeState(
                    HumanBehaviorState.Observing,
                    HumanDecisionReason.None);
            }

            if (State == HumanBehaviorState.Threatening && officerTarget != null)
            {
                FacePosition(officerTarget.position);
            }
        }

        private void UpdateMovement()
        {
            if (navigationAgent == null || !navigationAgent.enabled || !navigationAgent.isOnNavMesh)
            {
                return;
            }

            navigationAgent.speed = profile.MovementSpeed
                * Mathf.Lerp(0.4f, 1f, condition.Snapshot.Mobility);
            bool movingState = State == HumanBehaviorState.Fleeing
                || State == HumanBehaviorState.Hiding;
            if (!movingState)
            {
                navigationAgent.isStopped = true;
                return;
            }

            navigationAgent.isStopped = false;
            if (!navigationAgent.pathPending
                && navigationAgent.remainingDistance <= navigationAgent.stoppingDistance + 0.15f)
            {
                if (State == HumanBehaviorState.Hiding)
                {
                    ChangeState(HumanBehaviorState.Frozen, HumanDecisionReason.EscapeOpportunity);
                }
                else
                {
                    MoveAwayFrom(perception.LastKnownOfficerPosition, false);
                }
            }
        }

        private void UpdateDeceptiveSurrender(float deltaTime)
        {
            if (!IsDeceptiveSurrender || custody.IsRestrained)
            {
                return;
            }

            deceptiveSurrenderTimer -= deltaTime;
            if (deceptiveSurrenderTimer > 0f)
            {
                return;
            }

            IsDeceptiveSurrender = false;
            if (!custody.TryBreakSurrender("Deceptive surrender was abandoned before restraint."))
            {
                return;
            }

            HumanBehaviorState nextState = profile.Aggression >= profile.FlightTendency
                ? HumanBehaviorState.Threatening
                : HumanBehaviorState.Fleeing;
            ChangeState(nextState, HumanDecisionReason.SurrenderAbandoned);
            if (nextState == HumanBehaviorState.Fleeing)
            {
                MoveAwayFrom(perception.LastKnownOfficerPosition, false);
            }
        }

        private void MoveAwayFrom(Vector3 threatPosition, bool preferShorterMove)
        {
            if (navigationAgent == null || !navigationAgent.enabled || !navigationAgent.isOnNavMesh)
            {
                return;
            }

            Vector3 away = transform.position - threatPosition;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f)
            {
                away = transform.forward;
            }

            away.Normalize();
            float side = random.Range(-0.75f, 0.75f);
            Vector3 direction = (away + Vector3.Cross(Vector3.up, away) * side).normalized;
            float distance = preferShorterMove ? random.Range(3f, 5f) : random.Range(6f, 10f);
            Vector3 desired = transform.position + direction * distance;
            if (NavMesh.SamplePosition(desired, out NavMeshHit hit, 4f, navigationAgent.areaMask))
            {
                navigationAgent.isStopped = false;
                navigationAgent.SetDestination(hit.position);
            }
        }

        private void StopNavigation()
        {
            if (navigationAgent != null && navigationAgent.enabled && navigationAgent.isOnNavMesh)
            {
                navigationAgent.isStopped = true;
                navigationAgent.ResetPath();
            }
        }

        private void FacePosition(Vector3 position)
        {
            Vector3 direction = position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    240f * Time.deltaTime);
            }
        }

        private void OnConditionChanged(ActorConditionSnapshot snapshot)
        {
            if (!snapshot.CanAct)
            {
                hasPendingReaction = false;
                IsDeceptiveSurrender = false;
                ChangeState(HumanBehaviorState.Incapacitated, HumanDecisionReason.InjuryResponse);
                StopNavigation();
            }

            RefreshPresentation();
        }

        private void OnCustodyStateChanged(CustodyState custodyState)
        {
            HumanBehaviorState nextState = custodyState switch
            {
                CustodyState.Surrendering => HumanBehaviorState.Surrendering,
                CustodyState.Kneeling => HumanBehaviorState.Complying,
                CustodyState.Restrained => HumanBehaviorState.Restrained,
                CustodyState.Searched => HumanBehaviorState.Restrained,
                CustodyState.InCustody => HumanBehaviorState.Restrained,
                _ => State
            };
            if (nextState != State)
            {
                ChangeState(nextState, HumanDecisionReason.CustodyStateChanged);
            }

            if (custodyState == CustodyState.Restrained
                || custodyState == CustodyState.Searched
                || custodyState == CustodyState.InCustody)
            {
                hasPendingReaction = false;
                IsDeceptiveSurrender = false;
                StopNavigation();
            }

            RefreshPresentation();
        }

        private void ChangeState(HumanBehaviorState nextState, HumanDecisionReason reason)
        {
            if (State == nextState)
            {
                return;
            }

            HumanBehaviorState previousState = State;
            State = nextState;
            LastDecisionReason = reason;
            if (decisionLedger != null)
            {
                LastDecision = decisionLedger.RecordStateChange(
                    previousState,
                    nextState,
                    reason,
                    Stress,
                    Morale);
            }

            RefreshPresentation();
        }

        private void RefreshPresentation()
        {
            if (actorVisual != null && identity != null && condition != null && custody != null)
            {
                actorVisual.SetPresentation(
                    identity,
                    State,
                    custody.State,
                    condition.Snapshot.Level,
                    LastDecisionReason);
            }
        }

        private static int BuildStableSeed(int incidentSeed, string actorId)
        {
            unchecked
            {
                uint hash = 2166136261u;
                string value = actorId ?? string.Empty;
                for (int index = 0; index < value.Length; index++)
                {
                    hash ^= value[index];
                    hash *= 16777619u;
                }

                return (int)(hash ^ (uint)incidentSeed);
            }
        }
    }
}
