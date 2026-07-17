using RulesOfEntry.AI;
using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.Officers
{
    /// <summary>
    /// Conservative truth gate for a bounded tactical space. A room is not verified clear
    /// until every suspect in it is controlled and the required officer count remains inside
    /// for the full verification interval.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TacticalRoomVolume : MonoBehaviour
    {
        [SerializeField] private string roomId = "room_unassigned";
        [SerializeField] private string displayName = "Unassigned Room";
        [SerializeField] private BoxCollider roomBounds;
        [SerializeField, Min(1)] private int minimumOfficerCount = 2;
        [SerializeField, Min(0.5f)] private float clearVerificationSeconds = 2.5f;
        [SerializeField, Min(0.05f)] private float evaluationIntervalSeconds = 0.2f;

        private float evaluationTimer;
        private float verificationElapsed;
        private TacticalRoomClearanceState state = TacticalRoomClearanceState.Unclear;

        public string RoomId => roomId;
        public string DisplayName => displayName;
        public TacticalRoomClearanceState State => state;
        public bool IsVerifiedClear => state == TacticalRoomClearanceState.Clear;
        public int ActiveThreatCount { get; private set; }
        public int ActionableOfficerCount { get; private set; }
        public float VerificationProgress => clearVerificationSeconds <= 0f
            ? 0f
            : Mathf.Clamp01(verificationElapsed / clearVerificationSeconds);
        public bool HasCompleteConfiguration => roomBounds != null
            && !string.IsNullOrWhiteSpace(roomId)
            && minimumOfficerCount >= 1
            && clearVerificationSeconds >= 0.5f;

        public void Configure(
            string configuredRoomId,
            string configuredDisplayName,
            BoxCollider configuredBounds,
            int configuredMinimumOfficerCount,
            float configuredVerificationSeconds)
        {
            roomId = string.IsNullOrWhiteSpace(configuredRoomId)
                ? "room_unassigned"
                : configuredRoomId.Trim();
            displayName = string.IsNullOrWhiteSpace(configuredDisplayName)
                ? roomId
                : configuredDisplayName.Trim();
            roomBounds = configuredBounds;
            minimumOfficerCount = Mathf.Max(1, configuredMinimumOfficerCount);
            clearVerificationSeconds = Mathf.Max(0.5f, configuredVerificationSeconds);
            if (roomBounds != null)
            {
                roomBounds.isTrigger = true;
            }

            ResetClearance();
        }

        public bool Contains(Vector3 worldPosition)
        {
            if (roomBounds == null)
            {
                return false;
            }

            Vector3 localPoint = roomBounds.transform.InverseTransformPoint(worldPosition)
                - roomBounds.center;
            Vector3 halfSize = roomBounds.size * 0.5f;
            return Mathf.Abs(localPoint.x) <= halfSize.x
                && Mathf.Abs(localPoint.y) <= halfSize.y
                && Mathf.Abs(localPoint.z) <= halfSize.z;
        }

        public bool HasImmediateActiveThreat(HumanActorController[] subjects)
        {
            if (subjects == null)
            {
                return false;
            }

            foreach (HumanActorController subject in subjects)
            {
                if (subject != null
                    && Contains(subject.transform.position)
                    && RoomClearanceRules.IsActiveThreat(subject))
                {
                    return true;
                }
            }

            return false;
        }

        private void Awake()
        {
            roomBounds ??= GetComponent<BoxCollider>();
            if (roomBounds != null)
            {
                roomBounds.isTrigger = true;
            }
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Room Clearance",
                    $"{name} is missing its room ID, bounds, officer count, or verification interval.",
                    this);
            }
        }

        private void Update()
        {
            evaluationTimer -= Time.deltaTime;
            if (evaluationTimer > 0f)
            {
                return;
            }

            float elapsed = evaluationIntervalSeconds;
            evaluationTimer = evaluationIntervalSeconds;
            Evaluate(elapsed);
        }

        private void Evaluate(float elapsedSeconds)
        {
            HumanActorController[] subjects = Object.FindObjectsByType<HumanActorController>(
                FindObjectsSortMode.None);
            TacticalOfficerController[] officers =
                Object.FindObjectsByType<TacticalOfficerController>(FindObjectsSortMode.None);

            int threats = 0;
            foreach (HumanActorController subject in subjects)
            {
                if (subject != null
                    && Contains(subject.transform.position)
                    && RoomClearanceRules.IsActiveThreat(subject))
                {
                    threats++;
                }
            }

            int actionableOfficers = 0;
            foreach (TacticalOfficerController officer in officers)
            {
                if (officer != null
                    && officer.Condition != null
                    && officer.Condition.Snapshot.CanAct
                    && Contains(officer.transform.position))
                {
                    actionableOfficers++;
                }
            }

            ActiveThreatCount = threats;
            ActionableOfficerCount = actionableOfficers;
            bool mayVerify = threats == 0
                && actionableOfficers >= minimumOfficerCount;
            if (!mayVerify)
            {
                ResetClearance();
                return;
            }

            if (state == TacticalRoomClearanceState.Clear)
            {
                return;
            }

            if (state != TacticalRoomClearanceState.Verifying)
            {
                state = TacticalRoomClearanceState.Verifying;
                verificationElapsed = 0f;
                ProjectLog.Info(
                    "Room Clearance",
                    $"{displayName}: beginning a {clearVerificationSeconds:0.0}s clear verification.",
                    this);
            }

            verificationElapsed += Mathf.Max(0f, elapsedSeconds);
            if (verificationElapsed < clearVerificationSeconds)
            {
                return;
            }

            state = TacticalRoomClearanceState.Clear;
            verificationElapsed = clearVerificationSeconds;
            ProjectLog.Info(
                "Room Clearance",
                $"{displayName}: verified clear with {actionableOfficers} actionable officers present.",
                this);
        }

        private void ResetClearance()
        {
            bool wasClear = state == TacticalRoomClearanceState.Clear;
            state = TacticalRoomClearanceState.Unclear;
            verificationElapsed = 0f;
            if (wasClear)
            {
                ProjectLog.Warning(
                    "Room Clearance",
                    $"{displayName}: clear status was revoked because a threat or staffing condition changed.",
                    this);
            }
        }
    }
}
