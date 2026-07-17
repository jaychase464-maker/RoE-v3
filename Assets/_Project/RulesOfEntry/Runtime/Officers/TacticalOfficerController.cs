using System;
using RulesOfEntry.Actors;
using RulesOfEntry.Core;
using RulesOfEntry.Interaction;
using UnityEngine;
using UnityEngine.AI;

namespace RulesOfEntry.Officers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ActorIdentity))]
    [RequireComponent(typeof(ActorCondition))]
    [RequireComponent(typeof(OfficerOrderLedger))]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class TacticalOfficerController : MonoBehaviour
    {
        private enum ExecutionPhase
        {
            None = 0,
            Navigating = 1,
            DirectingToKneel = 2,
            ApplyingRestraints = 3,
            WaitingForDoorClearance = 4
        }

        [Header("Authoritative references")]
        [SerializeField] private ActorIdentity identity;
        [SerializeField] private ActorCondition condition;
        [SerializeField] private OfficerOrderLedger orderLedger;
        [SerializeField] private NavMeshAgent navigationAgent;
        [SerializeField] private OfficerVisual officerVisual;

        [Header("Physical execution")]
        [SerializeField, Min(0.5f)] private float targetSampleRadius = 1.8f;
        [SerializeField, Min(0.1f)] private float arrivalTolerance = 0.2f;
        [SerializeField, Min(1f)] private float orderTimeoutSeconds = 35f;
        [SerializeField, Min(0.1f)] private float kneelingSettleSeconds = 1.25f;
        [SerializeField, Min(0.5f)] private float restraintApplicationSeconds = 3.5f;
        [SerializeField, Min(0.05f)] private float followRepathSeconds = 0.25f;

        private OfficerOrder currentOrder;
        private OfficerOrderStateMachine currentState;
        private ExecutionPhase executionPhase;
        private float orderElapsedSeconds;
        private float phaseElapsedSeconds;
        private float followRepathTimer;
        private bool selected;
        private string activity = "Standing by";

        public event Action<TacticalOfficerController> OrderStateChanged;

        public ActorIdentity Identity => identity;
        public ActorCondition Condition => condition;
        public OfficerOrderLedger OrderLedger => orderLedger;
        public OfficerOrder CurrentOrder => currentOrder;
        public OfficerOrderStatus CurrentStatus => currentState?.Status
            ?? OfficerOrderStatus.Completed;
        public OfficerOrderOutcomeReason CurrentOutcomeReason =>
            currentState?.OutcomeReason ?? OfficerOrderOutcomeReason.None;
        public string CurrentDetails => currentState?.Details ?? "No order issued.";
        public string Activity => activity;
        public bool IsSelected => selected;
        public bool HasActiveOrder => currentState != null && !currentState.IsTerminal;
        public bool HasCompleteConfiguration => identity != null
            && identity.Role == ActorRole.Officer
            && condition != null
            && orderLedger != null
            && navigationAgent != null
            && officerVisual != null;

        public void Configure(
            ActorIdentity configuredIdentity,
            ActorCondition configuredCondition,
            OfficerOrderLedger configuredLedger,
            NavMeshAgent configuredAgent,
            OfficerVisual configuredVisual)
        {
            identity = configuredIdentity;
            condition = configuredCondition;
            orderLedger = configuredLedger;
            navigationAgent = configuredAgent;
            officerVisual = configuredVisual;
            RefreshPresentation();
        }

        public bool AssignOrder(OfficerOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            ResolveReferences();
            if (currentState != null && !currentState.IsTerminal)
            {
                CancelCurrent(
                    OfficerOrderOutcomeReason.Superseded,
                    "A newer command superseded this order.");
            }

            currentOrder = order;
            currentState = new OfficerOrderStateMachine();
            executionPhase = ExecutionPhase.None;
            orderElapsedSeconds = 0f;
            phaseElapsedSeconds = 0f;
            activity = "Order received";
            RecordState();

            if (!HasCompleteConfiguration || order.OfficerEntityId != identity.RuntimeEntityId)
            {
                Refuse(
                    OfficerOrderOutcomeReason.OfficerUnavailable,
                    "The order was addressed to an unavailable or misconfigured officer.");
                return false;
            }

            if (!condition.Snapshot.CanAct)
            {
                Refuse(
                    OfficerOrderOutcomeReason.OfficerIncapacitated,
                    "Officer cannot safely execute orders in the current condition.");
                return false;
            }

            currentState.TryAccept("Order acknowledged by the assigned officer.");
            RecordState();
            return BeginAcceptedOrder();
        }

        public bool CancelActiveOrder()
        {
            return CancelCurrent(
                OfficerOrderOutcomeReason.CancelledByPlayer,
                "Command cancelled by the player.");
        }

        public bool FailInitiativeOrder(
            OfficerOrderOutcomeReason reason,
            string details)
        {
            if (currentOrder == null
                || currentOrder.Origin != OfficerOrderOrigin.OfficerInitiative
                || currentState == null
                || currentState.IsTerminal)
            {
                return false;
            }

            return Fail(reason, details);
        }

        public void SetSelected(bool isSelected)
        {
            selected = isSelected;
            RefreshPresentation();
        }

        private void Awake()
        {
            ResolveReferences();
            RefreshPresentation();
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Officer AI",
                    $"{name} is missing Milestone 4 officer references. Run the Milestone 4 setup tool.",
                    this);
            }
        }

        private void Update()
        {
            if (currentState == null || currentState.IsTerminal)
            {
                return;
            }

            if (condition == null || !condition.Snapshot.CanAct)
            {
                Fail(
                    OfficerOrderOutcomeReason.OfficerIncapacitated,
                    "Officer became unable to continue the assigned order.");
                return;
            }

            orderElapsedSeconds += Time.deltaTime;
            if (currentOrder.Type != OfficerOrderType.Follow
                && orderElapsedSeconds >= orderTimeoutSeconds)
            {
                Fail(
                    OfficerOrderOutcomeReason.TimedOut,
                    "Officer could not complete the order within the safe execution window.");
                return;
            }

            switch (currentOrder.Type)
            {
                case OfficerOrderType.Follow:
                    UpdateFollow();
                    break;
                case OfficerOrderType.MoveTo:
                case OfficerOrderType.StackAtDoor:
                    UpdateMovementOrder();
                    break;
                case OfficerOrderType.OpenDoor:
                    UpdateDoorOrder();
                    break;
                case OfficerOrderType.RestrainSubject:
                    UpdateRestraintOrder();
                    break;
            }
        }

        private void ResolveReferences()
        {
            identity ??= GetComponent<ActorIdentity>();
            condition ??= GetComponent<ActorCondition>();
            orderLedger ??= GetComponent<OfficerOrderLedger>();
            navigationAgent ??= GetComponent<NavMeshAgent>();
            officerVisual ??= GetComponent<OfficerVisual>();
        }

        private bool BeginAcceptedOrder()
        {
            switch (currentOrder.Type)
            {
                case OfficerOrderType.HoldPosition:
                    StopNavigation();
                    BeginExecution("Officer is holding the current position.");
                    return Complete("Officer established a hold at the assigned position.");
                case OfficerOrderType.Follow:
                    if (ResolveTargetTransform(currentOrder.TargetObject) == null)
                    {
                        return Fail(
                            OfficerOrderOutcomeReason.InvalidTarget,
                            "Follow order has no valid leader transform.");
                    }

                    BeginExecution("Officer is following the team leader.");
                    followRepathTimer = 0f;
                    return true;
                case OfficerOrderType.OpenDoor:
                    if (ResolveTargetComponent<PrototypeDoor>(currentOrder.TargetObject) == null)
                    {
                        return Fail(
                            OfficerOrderOutcomeReason.InvalidTarget,
                            "Open order does not reference a valid prototype door.");
                    }

                    return BeginNavigation(currentOrder.TargetPosition, "Moving to door.");
                case OfficerOrderType.RestrainSubject:
                    if (ResolveTargetComponent<CustodyComponent>(currentOrder.TargetObject) == null)
                    {
                        return Fail(
                            OfficerOrderOutcomeReason.InvalidTarget,
                            "Restraint order does not reference a valid custody subject.");
                    }

                    return BeginNavigation(currentOrder.TargetPosition, "Approaching subject.");
                case OfficerOrderType.MoveTo:
                    return BeginNavigation(currentOrder.TargetPosition, "Moving to command point.");
                case OfficerOrderType.StackAtDoor:
                    if (ResolveTargetComponent<PrototypeDoor>(currentOrder.TargetObject) == null)
                    {
                        return Fail(
                            OfficerOrderOutcomeReason.InvalidTarget,
                            "Stack order does not reference a valid prototype door.");
                    }

                    return BeginNavigation(currentOrder.TargetPosition, "Moving to stack position.");
                default:
                    return Fail(
                        OfficerOrderOutcomeReason.InvalidTarget,
                        "Unknown officer order type.");
            }
        }

        private bool BeginNavigation(Vector3 requestedPosition, string details)
        {
            if (navigationAgent == null
                || !navigationAgent.enabled
                || !navigationAgent.isOnNavMesh)
            {
                return Fail(
                    OfficerOrderOutcomeReason.NoNavigationSurface,
                    "Officer is not standing on a baked NavMesh.");
            }

            if (!NavMesh.SamplePosition(
                    requestedPosition,
                    out NavMeshHit sampled,
                    targetSampleRadius,
                    navigationAgent.areaMask))
            {
                return Fail(
                    OfficerOrderOutcomeReason.TargetUnreachable,
                    "No navigable position exists near the requested point.");
            }

            NavMeshPath path = new NavMeshPath();
            bool calculated = navigationAgent.CalculatePath(sampled.position, path);
            if (!calculated || path.status != NavMeshPathStatus.PathComplete)
            {
                return Fail(
                    OfficerOrderOutcomeReason.NoPath,
                    "A complete navigation path to the command point could not be found.");
            }

            navigationAgent.isStopped = false;
            if (!navigationAgent.SetPath(path))
            {
                return Fail(
                    OfficerOrderOutcomeReason.NoPath,
                    "The calculated navigation path could not be assigned.");
            }

            executionPhase = ExecutionPhase.Navigating;
            BeginExecution(details);
            return true;
        }

        private void UpdateMovementOrder()
        {
            if (!ValidateNavigationProgress())
            {
                return;
            }

            if (!HasArrived())
            {
                return;
            }

            StopNavigation();
            string details = currentOrder.Type == OfficerOrderType.StackAtDoor
                ? "Officer reached and is holding the assigned stack position."
                : "Officer reached the assigned command point.";
            Complete(details);
        }

        private void UpdateFollow()
        {
            Transform leader = ResolveTargetTransform(currentOrder.TargetObject);
            if (leader == null)
            {
                Fail(
                    OfficerOrderOutcomeReason.InvalidTarget,
                    "The followed team leader is no longer available.");
                return;
            }

            if (navigationAgent == null
                || !navigationAgent.enabled
                || !navigationAgent.isOnNavMesh)
            {
                Fail(
                    OfficerOrderOutcomeReason.NoNavigationSurface,
                    "Officer left the baked navigation surface while following.");
                return;
            }

            followRepathTimer -= Time.deltaTime;
            if (followRepathTimer > 0f)
            {
                return;
            }

            followRepathTimer = followRepathSeconds;
            Vector3 followPoint = leader.position - leader.forward * 1.8f;
            if (!NavMesh.SamplePosition(
                    followPoint,
                    out NavMeshHit sampled,
                    targetSampleRadius,
                    navigationAgent.areaMask))
            {
                activity = "Waiting for a safe follow path";
                RefreshPresentation();
                return;
            }

            navigationAgent.isStopped = false;
            navigationAgent.SetDestination(sampled.position);
            activity = "Following team leader";
            RefreshPresentation();
        }

        private void UpdateDoorOrder()
        {
            PrototypeDoor door = ResolveTargetComponent<PrototypeDoor>(currentOrder.TargetObject);
            if (door == null)
            {
                Fail(
                    OfficerOrderOutcomeReason.InvalidTarget,
                    "The assigned door is no longer available.");
                return;
            }

            if (executionPhase == ExecutionPhase.WaitingForDoorClearance)
            {
                if (!door.IsOpen)
                {
                    Fail(
                        OfficerOrderOutcomeReason.DoorInteractionFailed,
                        "The assigned door began closing before the threshold was clear.");
                    return;
                }

                if (!door.IsTraversalClear)
                {
                    return;
                }

                Complete(
                    "Officer opened the assigned door and verified a clear threshold.");
                return;
            }

            if (!ValidateNavigationProgress() || !HasArrived())
            {
                return;
            }

            StopNavigation();
            FacePosition(door.transform.position);
            if (!door.IsOpen)
            {
                door.Interact(new InteractionContext(gameObject, transform, Time.time));
            }

            if (!door.IsOpen)
            {
                Fail(
                    OfficerOrderOutcomeReason.DoorInteractionFailed,
                    "Officer operated the door but it did not enter an open state.");
                return;
            }

            executionPhase = ExecutionPhase.WaitingForDoorClearance;
            activity = "Waiting for the door leaf to clear the threshold";
            RefreshPresentation();
        }

        private void UpdateRestraintOrder()
        {
            CustodyComponent custody = ResolveTargetComponent<CustodyComponent>(
                currentOrder.TargetObject);
            if (custody == null)
            {
                Fail(
                    OfficerOrderOutcomeReason.InvalidTarget,
                    "The assigned subject is no longer available.");
                return;
            }

            if (custody.IsRestrained)
            {
                StopNavigation();
                Complete("Subject was already restrained when the officer verified custody.");
                return;
            }

            if (executionPhase == ExecutionPhase.Navigating)
            {
                if (!ValidateNavigationProgress() || !HasArrived())
                {
                    return;
                }

                StopNavigation();
                FacePosition(custody.transform.position);
                if (custody.State == CustodyState.Surrendering)
                {
                    if (!custody.TryOrderToKneel(gameObject))
                    {
                        Fail(
                            OfficerOrderOutcomeReason.RestraintTransitionFailed,
                            "Subject could not be moved from surrender to a controlled kneel.");
                        return;
                    }

                    executionPhase = ExecutionPhase.DirectingToKneel;
                    phaseElapsedSeconds = 0f;
                    activity = "Directing subject to kneel";
                    RefreshPresentation();
                    return;
                }

                ActorCondition subjectCondition = custody.GetComponent<ActorCondition>();
                if (custody.State == CustodyState.Free
                    && subjectCondition != null
                    && subjectCondition.Snapshot.Level == ActorConditionLevel.Incapacitated)
                {
                    if (!custody.TrySecureIncapacitated(gameObject))
                    {
                        Fail(
                            OfficerOrderOutcomeReason.RestraintTransitionFailed,
                            "Incapacitated subject could not be positioned for restraint.");
                        return;
                    }

                    BeginRestraintApplication();
                    return;
                }

                if (custody.State == CustodyState.Kneeling)
                {
                    BeginRestraintApplication();
                    return;
                }

                Fail(
                    OfficerOrderOutcomeReason.SubjectNotCompliant,
                    "Officer refused to apply restraints to a mobile, non-compliant subject.");
                return;
            }

            phaseElapsedSeconds += Time.deltaTime;
            if (executionPhase == ExecutionPhase.DirectingToKneel)
            {
                if (custody.State != CustodyState.Kneeling)
                {
                    Fail(
                        OfficerOrderOutcomeReason.SubjectNoLongerCompliant,
                        "Subject left the controlled kneeling state before cuffing.");
                    return;
                }

                if (phaseElapsedSeconds >= kneelingSettleSeconds)
                {
                    BeginRestraintApplication();
                }

                return;
            }

            if (executionPhase != ExecutionPhase.ApplyingRestraints)
            {
                return;
            }

            if (custody.State != CustodyState.Kneeling)
            {
                Fail(
                    OfficerOrderOutcomeReason.SubjectNoLongerCompliant,
                    "Subject left the controlled kneeling state during cuffing.");
                return;
            }

            if (phaseElapsedSeconds < restraintApplicationSeconds)
            {
                return;
            }

            if (!custody.TryApplyRestraints(gameObject))
            {
                Fail(
                    OfficerOrderOutcomeReason.RestraintTransitionFailed,
                    "Handcuff application did not produce a restrained custody state.");
                return;
            }

            Complete("Officer applied handcuffs and verified the restraint.");
        }

        private void BeginRestraintApplication()
        {
            executionPhase = ExecutionPhase.ApplyingRestraints;
            phaseElapsedSeconds = 0f;
            activity = "Applying and checking handcuffs";
            RefreshPresentation();
        }

        private bool ValidateNavigationProgress()
        {
            if (navigationAgent == null
                || !navigationAgent.enabled
                || !navigationAgent.isOnNavMesh)
            {
                Fail(
                    OfficerOrderOutcomeReason.NoNavigationSurface,
                    "Officer is no longer on the baked navigation surface.");
                return false;
            }

            if (!navigationAgent.pathPending
                && navigationAgent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                Fail(
                    OfficerOrderOutcomeReason.NoPath,
                    "Navigation path became incomplete during order execution.");
                return false;
            }

            return true;
        }

        private bool HasArrived()
        {
            return navigationAgent != null
                && !navigationAgent.pathPending
                && navigationAgent.remainingDistance
                    <= navigationAgent.stoppingDistance + arrivalTolerance;
        }

        private void StopNavigation()
        {
            if (navigationAgent == null
                || !navigationAgent.enabled
                || !navigationAgent.isOnNavMesh)
            {
                return;
            }

            navigationAgent.isStopped = true;
            navigationAgent.ResetPath();
        }

        private void FacePosition(Vector3 target)
        {
            Vector3 direction = target - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private void BeginExecution(string details)
        {
            currentState.TryBeginExecution(details);
            activity = details;
            RecordState();
        }

        private bool Complete(string details)
        {
            if (!currentState.TryComplete(details))
            {
                return false;
            }

            executionPhase = ExecutionPhase.None;
            activity = "Order complete";
            RecordState();
            return true;
        }

        private bool CancelCurrent(OfficerOrderOutcomeReason reason, string details)
        {
            if (currentState == null || !currentState.TryCancel(reason, details))
            {
                return false;
            }

            StopNavigation();
            executionPhase = ExecutionPhase.None;
            activity = "Order cancelled";
            RecordState();
            return true;
        }

        private bool Fail(OfficerOrderOutcomeReason reason, string details)
        {
            if (currentState == null || !currentState.TryFail(reason, details))
            {
                return false;
            }

            StopNavigation();
            executionPhase = ExecutionPhase.None;
            activity = "Unable to complete order";
            RecordState();
            return false;
        }

        private void Refuse(OfficerOrderOutcomeReason reason, string details)
        {
            if (!currentState.TryRefuse(reason, details))
            {
                return;
            }

            activity = "Order refused";
            RecordState();
        }

        private void RecordState()
        {
            if (currentOrder != null && currentState != null)
            {
                orderLedger?.Record(currentOrder, currentState);
            }

            RefreshPresentation();
            OrderStateChanged?.Invoke(this);
            if (currentOrder != null && currentState != null)
            {
                ProjectLog.Development(
                    "Officer Order",
                    $"{identity?.DisplayName ?? name}: command {currentOrder.CommandSequence} "
                        + $"{currentOrder.Type} -> {currentState.Status} "
                        + $"({currentState.OutcomeReason}).",
                    this);
            }
        }

        private void RefreshPresentation()
        {
            officerVisual?.SetPresentation(
                identity,
                selected,
                CurrentStatus,
                currentOrder != null ? currentOrder.Type : null,
                CurrentOutcomeReason,
                activity);
        }

        private static Transform ResolveTargetTransform(UnityEngine.Object target)
        {
            if (target is GameObject gameObject)
            {
                return gameObject.transform;
            }

            return target is Component component ? component.transform : null;
        }

        private static T ResolveTargetComponent<T>(UnityEngine.Object target)
            where T : Component
        {
            if (target is GameObject gameObject)
            {
                return gameObject.GetComponentInParent<T>();
            }

            return target is Component component
                ? component.GetComponentInParent<T>()
                : null;
        }
    }
}
