using System.Collections.Generic;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.Officers
{
    /// <summary>
    /// Gives an officer bounded initiative without bypassing the established command,
    /// navigation, custody, or accountability systems.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ActorIdentity))]
    [RequireComponent(typeof(ActorCondition))]
    [RequireComponent(typeof(TacticalOfficerController))]
    [RequireComponent(typeof(OfficerInitiativeLedger))]
    public sealed class OfficerInitiativeController : MonoBehaviour
    {
        private static readonly Dictionary<ulong, float> NextTeamChallengeTime =
            new Dictionary<ulong, float>();

        [Header("Authoritative references")]
        [SerializeField] private ActorIdentity identity;
        [SerializeField] private ActorCondition condition;
        [SerializeField] private TacticalOfficerController officerController;
        [SerializeField] private OfficerInitiativeLedger initiativeLedger;

        [Header("Perception and challenge")]
        [SerializeField, Min(2f)] private float detectionDistance = 20f;
        [SerializeField, Range(30f, 180f)] private float fieldOfViewDegrees = 140f;
        [SerializeField, Min(0.5f)] private float challengeRepeatSeconds = 4f;
        [SerializeField, Min(0.05f)] private float scanIntervalSeconds = 0.2f;
        [SerializeField] private LayerMask visibilityMask = ~0;
        [SerializeField] private bool weaponPresentedDuringChallenge = true;

        [Header("Automatic custody")]
        [SerializeField, Min(0.5f)] private float custodyApproachDistance = 0.9f;

        private TacticalRoomVolume[] roomVolumes = System.Array.Empty<TacticalRoomVolume>();
        private float scanTimer;
        private float roomRefreshTimer;
        private long nextInitiativeSequence = 1;

        public string LastActivity { get; private set; } = "Scanning for threats";
        public bool HasCompleteConfiguration => identity != null
            && identity.Role == ActorRole.Officer
            && condition != null
            && officerController != null
            && initiativeLedger != null
            && initiativeLedger.HasCompleteConfiguration;

        public void Configure(
            ActorIdentity configuredIdentity,
            ActorCondition configuredCondition,
            TacticalOfficerController configuredController,
            OfficerInitiativeLedger configuredLedger,
            float configuredDetectionDistance,
            LayerMask configuredVisibilityMask,
            bool configuredWeaponPresented)
        {
            identity = configuredIdentity;
            condition = configuredCondition;
            officerController = configuredController;
            initiativeLedger = configuredLedger;
            detectionDistance = Mathf.Max(2f, configuredDetectionDistance);
            visibilityMask = configuredVisibilityMask;
            weaponPresentedDuringChallenge = configuredWeaponPresented;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSharedChallengeState()
        {
            NextTeamChallengeTime.Clear();
        }

        private void Awake()
        {
            ResolveReferences();
            RefreshRooms();
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Officer Initiative",
                    $"{name} is missing initiative references. Re-run the Milestone 4 setup tool.",
                    this);
            }
        }

        private void Update()
        {
            if (!HasCompleteConfiguration || !condition.Snapshot.CanAct)
            {
                return;
            }

            scanTimer -= Time.deltaTime;
            roomRefreshTimer -= Time.deltaTime;
            if (roomRefreshTimer <= 0f)
            {
                roomRefreshTimer = 2f;
                RefreshRooms();
            }

            if (scanTimer > 0f)
            {
                return;
            }

            scanTimer = scanIntervalSeconds;
            HumanActorController[] subjects = Object.FindObjectsByType<HumanActorController>(
                FindObjectsSortMode.None);
            TacticalOfficerController[] officers =
                Object.FindObjectsByType<TacticalOfficerController>(FindObjectsSortMode.None);

            TryChallengeVisibleSuspect(subjects);
            if (MonitorAutomaticCustody(subjects, officers))
            {
                return;
            }

            TryAssignAutomaticCustody(subjects, officers);
            UpdateClearanceActivity();
        }

        private void ResolveReferences()
        {
            identity ??= GetComponent<ActorIdentity>();
            condition ??= GetComponent<ActorCondition>();
            officerController ??= GetComponent<TacticalOfficerController>();
            initiativeLedger ??= GetComponent<OfficerInitiativeLedger>();
        }

        private void RefreshRooms()
        {
            roomVolumes = Object.FindObjectsByType<TacticalRoomVolume>(
                FindObjectsSortMode.None);
        }

        private void TryChallengeVisibleSuspect(HumanActorController[] subjects)
        {
            HumanActorController nearest = null;
            float nearestSquaredDistance = float.PositiveInfinity;
            foreach (HumanActorController subject in subjects)
            {
                if (!MayChallenge(subject))
                {
                    continue;
                }

                ulong targetId = subject.Identity.RuntimeEntityId;
                if (NextTeamChallengeTime.TryGetValue(targetId, out float nextTime)
                    && Time.time < nextTime)
                {
                    continue;
                }

                float squaredDistance = (subject.transform.position - transform.position)
                    .sqrMagnitude;
                if (squaredDistance < nearestSquaredDistance && CanSee(subject))
                {
                    nearest = subject;
                    nearestSquaredDistance = squaredDistance;
                }
            }

            if (nearest == null)
            {
                return;
            }

            VerbalCommandType command = nearest.State == HumanBehaviorState.Threatening
                || nearest.State == HumanBehaviorState.Resisting
                ? VerbalCommandType.DropWeapon
                : VerbalCommandType.PoliceShowHands;
            Vector3 voicePosition = transform.position + Vector3.up * 1.55f;
            VerbalCommandStimulus stimulus = new VerbalCommandStimulus(
                gameObject,
                command,
                voicePosition,
                Time.time,
                detectionDistance,
                weaponPresentedDuringChallenge);
            HumanDecisionRecord decision = nearest.ReceiveVerbalCommand(stimulus);
            NextTeamChallengeTime[nearest.Identity.RuntimeEntityId] =
                Time.time + challengeRepeatSeconds;

            string spokenCommand = command == VerbalCommandType.DropWeapon
                ? "POLICE! DROP THE WEAPON!"
                : "POLICE! SHOW ME YOUR HANDS!";
            LastActivity = $"Challenged {nearest.Identity.DisplayName}";
            initiativeLedger.Record(
                OfficerInitiativeEventType.ChallengeIssued,
                nearest,
                FindContainingRoom(nearest.transform.position),
                command,
                decision != null
                    ? $"{spokenCommand} Decision: {decision.NewState} ({decision.Reason})."
                    : $"{spokenCommand} The subject could not evaluate the command.");
            ProjectLog.Info(
                "Officer Initiative",
                $"{identity.DisplayName}: {spokenCommand} Target: {nearest.Identity.DisplayName}.",
                this);
        }

        private bool MonitorAutomaticCustody(
            HumanActorController[] subjects,
            TacticalOfficerController[] officers)
        {
            OfficerOrder order = officerController.CurrentOrder;
            if (!officerController.HasActiveOrder
                || order == null
                || order.Origin != OfficerOrderOrigin.OfficerInitiative
                || order.Type != OfficerOrderType.RestrainSubject)
            {
                return false;
            }

            HumanActorController target = ResolveOrderSubject(order);
            TacticalRoomVolume room = target != null
                ? FindContainingRoom(target.transform.position)
                : null;
            bool roomSafe = room != null
                && room.IsVerifiedClear
                && !room.HasImmediateActiveThreat(subjects);
            if (!roomSafe)
            {
                AbortAutomaticCustody(
                    target,
                    room,
                    OfficerOrderOutcomeReason.RoomNoLongerClear,
                    "Automatic custody stopped because the room was no longer verified clear.");
                return true;
            }

            if (!HasCoverOfficer(room, officers))
            {
                AbortAutomaticCustody(
                    target,
                    room,
                    OfficerOrderOutcomeReason.CoverOfficerUnavailable,
                    "Automatic custody stopped because no actionable cover officer remained in the room.");
                return true;
            }

            return true;
        }

        private void TryAssignAutomaticCustody(
            HumanActorController[] subjects,
            TacticalOfficerController[] officers)
        {
            if (officerController.HasActiveOrder
                && officerController.CurrentOrder?.Type != OfficerOrderType.Follow)
            {
                return;
            }

            TacticalRoomVolume room = FindContainingRoom(transform.position);
            if (room == null
                || !room.IsVerifiedClear
                || room.HasImmediateActiveThreat(subjects)
                || !HasCoverOfficer(room, officers))
            {
                return;
            }

            HumanActorController target = FindNearestCustodyTarget(room, subjects, officers);
            if (target == null || !IsPreferredCustodyOfficer(room, target, officers))
            {
                return;
            }

            Vector3 towardOfficer = transform.position - target.transform.position;
            towardOfficer.y = 0f;
            if (towardOfficer.sqrMagnitude <= 0.01f)
            {
                towardOfficer = -target.transform.forward;
            }

            Vector3 approach = target.transform.position
                + towardOfficer.normalized * custodyApproachDistance;
            OfficerOrder order = new OfficerOrder(
                nextInitiativeSequence++,
                identity.RuntimeEntityId,
                identity.RuntimeEntityId,
                OfficerOrderType.RestrainSubject,
                approach,
                target,
                target.Identity.RuntimeEntityId,
                Time.timeAsDouble,
                OfficerOrderOrigin.OfficerInitiative);
            if (!officerController.AssignOrder(order))
            {
                return;
            }

            LastActivity = $"Securing {target.Identity.DisplayName} with cover";
            initiativeLedger.Record(
                OfficerInitiativeEventType.AutomaticCustodyAssigned,
                target,
                room,
                null,
                "Room clear was verified; a second actionable officer was present for cover.");
            ProjectLog.Info(
                "Officer Initiative",
                $"{identity.DisplayName} automatically began controlled custody of "
                    + $"{target.Identity.DisplayName} in {room.DisplayName}.",
                this);
        }

        private void AbortAutomaticCustody(
            HumanActorController target,
            TacticalRoomVolume room,
            OfficerOrderOutcomeReason reason,
            string details)
        {
            if (!officerController.FailInitiativeOrder(reason, details))
            {
                return;
            }

            LastActivity = details;
            initiativeLedger.Record(
                OfficerInitiativeEventType.AutomaticCustodyAborted,
                target,
                room,
                null,
                details);
        }

        private HumanActorController FindNearestCustodyTarget(
            TacticalRoomVolume room,
            HumanActorController[] subjects,
            TacticalOfficerController[] officers)
        {
            HumanActorController nearest = null;
            float nearestSquaredDistance = float.PositiveInfinity;
            foreach (HumanActorController subject in subjects)
            {
                if (subject == null
                    || !room.Contains(subject.transform.position)
                    || !RoomClearanceRules.IsEligibleForAutomaticCustody(subject)
                    || IsAssignedForRestraint(subject, officers))
                {
                    continue;
                }

                float squaredDistance = (subject.transform.position - transform.position)
                    .sqrMagnitude;
                if (squaredDistance < nearestSquaredDistance)
                {
                    nearest = subject;
                    nearestSquaredDistance = squaredDistance;
                }
            }

            return nearest;
        }

        private bool IsPreferredCustodyOfficer(
            TacticalRoomVolume room,
            HumanActorController target,
            TacticalOfficerController[] officers)
        {
            TacticalOfficerController preferred = null;
            float nearestSquaredDistance = float.PositiveInfinity;
            ulong preferredId = ulong.MaxValue;
            foreach (TacticalOfficerController candidate in officers)
            {
                if (!CanTakeInitiativeCustody(candidate, room))
                {
                    continue;
                }

                float squaredDistance = (candidate.transform.position - target.transform.position)
                    .sqrMagnitude;
                ulong candidateId = candidate.Identity != null
                    ? candidate.Identity.RuntimeEntityId
                    : ulong.MaxValue;
                if (squaredDistance < nearestSquaredDistance
                    || (Mathf.Approximately(squaredDistance, nearestSquaredDistance)
                        && candidateId < preferredId))
                {
                    preferred = candidate;
                    nearestSquaredDistance = squaredDistance;
                    preferredId = candidateId;
                }
            }

            return preferred == officerController;
        }

        private bool HasCoverOfficer(
            TacticalRoomVolume room,
            TacticalOfficerController[] officers)
        {
            foreach (TacticalOfficerController candidate in officers)
            {
                if (candidate == null
                    || candidate == officerController
                    || candidate.Condition == null
                    || !candidate.Condition.Snapshot.CanAct
                    || !room.Contains(candidate.transform.position))
                {
                    continue;
                }

                OfficerOrder candidateOrder = candidate.CurrentOrder;
                if (!candidate.HasActiveOrder
                    || candidateOrder?.Type == OfficerOrderType.Follow)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CanTakeInitiativeCustody(
            TacticalOfficerController candidate,
            TacticalRoomVolume room)
        {
            return candidate != null
                && candidate.Identity != null
                && candidate.Condition != null
                && candidate.Condition.Snapshot.CanAct
                && room.Contains(candidate.transform.position)
                && (!candidate.HasActiveOrder
                    || candidate.CurrentOrder?.Type == OfficerOrderType.Follow);
        }

        private static bool IsAssignedForRestraint(
            HumanActorController subject,
            TacticalOfficerController[] officers)
        {
            ulong subjectId = subject.Identity.RuntimeEntityId;
            foreach (TacticalOfficerController officer in officers)
            {
                OfficerOrder order = officer != null ? officer.CurrentOrder : null;
                if (officer != null
                    && officer.HasActiveOrder
                    && order != null
                    && order.Type == OfficerOrderType.RestrainSubject
                    && order.TargetEntityId == subjectId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool MayChallenge(HumanActorController subject)
        {
            return subject != null
                && subject.Identity != null
                && subject.Identity.Role == ActorRole.Suspect
                && subject.Custody != null
                && subject.Custody.State == CustodyState.Free
                && subject.Condition != null
                && subject.Condition.Snapshot.CanAct
                && (subject.transform.position - transform.position).sqrMagnitude
                    <= detectionDistance * detectionDistance;
        }

        private bool CanSee(HumanActorController subject)
        {
            Vector3 origin = transform.position + Vector3.up * 1.55f;
            Vector3 target = subject.transform.position + Vector3.up * 1.2f;
            Vector3 offset = target - origin;
            float distance = offset.magnitude;
            if (distance <= 0.01f
                || distance > detectionDistance
                || Vector3.Angle(transform.forward, offset) > fieldOfViewDegrees * 0.5f)
            {
                return false;
            }

            if (!Physics.Raycast(
                    origin,
                    offset / distance,
                    out RaycastHit hit,
                    distance,
                    visibilityMask,
                    QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return hit.transform.IsChildOf(subject.transform);
        }

        private TacticalRoomVolume FindContainingRoom(Vector3 position)
        {
            foreach (TacticalRoomVolume room in roomVolumes)
            {
                if (room != null && room.Contains(position))
                {
                    return room;
                }
            }

            return null;
        }

        private static HumanActorController ResolveOrderSubject(OfficerOrder order)
        {
            if (order?.TargetObject is HumanActorController subject)
            {
                return subject;
            }

            if (order?.TargetObject is Component component)
            {
                return component.GetComponentInParent<HumanActorController>();
            }

            if (order?.TargetObject is GameObject gameObject)
            {
                return gameObject.GetComponentInParent<HumanActorController>();
            }

            return null;
        }

        private void UpdateClearanceActivity()
        {
            TacticalRoomVolume room = FindContainingRoom(transform.position);
            if (room == null)
            {
                if (!officerController.HasActiveOrder)
                {
                    LastActivity = "Scanning for threats";
                }

                return;
            }

            if (room.State == TacticalRoomClearanceState.Verifying)
            {
                LastActivity = $"Verifying {room.DisplayName}: "
                    + $"{room.VerificationProgress * 100f:0}%";
            }
            else if (room.IsVerifiedClear)
            {
                LastActivity = $"{room.DisplayName} clear; maintaining cover";
            }
            else
            {
                LastActivity = $"Clearing {room.DisplayName}: "
                    + $"{room.ActiveThreatCount} active threat(s)";
            }
        }
    }
}
