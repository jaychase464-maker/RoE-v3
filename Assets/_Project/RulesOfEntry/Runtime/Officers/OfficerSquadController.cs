using System;
using System.Collections.Generic;
using RulesOfEntry.Actors;
using RulesOfEntry.Core;
using RulesOfEntry.Input;
using RulesOfEntry.Interaction;
using UnityEngine;

namespace RulesOfEntry.Officers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TacticalPlayerInput))]
    public sealed class OfficerSquadController : MonoBehaviour
    {
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private Transform commandView;
        [SerializeField] private TacticalOfficerController[] officers =
            Array.Empty<TacticalOfficerController>();
        [SerializeField] private OfficerOrderMarker orderMarker;
        [SerializeField] private LayerMask commandMask = ~0;
        [SerializeField, Min(2f)] private float commandDistance = 30f;
        [SerializeField, Min(0.25f)] private float teamSpacing = 0.72f;
        [SerializeField, Min(0.5f)] private float doorApproachDistance = 1.05f;
        [SerializeField, Min(0.5f)] private float subjectApproachDistance = 0.9f;

        private long nextCommandSequence = 1;

        public event Action SelectionChanged;
        public event Action CommandIssued;

        public OfficerSelection Selection { get; private set; } = OfficerSelection.Team;
        public IReadOnlyList<TacticalOfficerController> Officers =>
            officers ?? Array.Empty<TacticalOfficerController>();
        public string LastCommandSummary { get; private set; } = "No team command issued.";
        public bool HasCompleteConfiguration => ConfigurationProblems.Length == 0;
        public string ConfigurationProblems
        {
            get
            {
                List<string> problems = new List<string>();
                if (playerInput == null)
                {
                    problems.Add("player input");
                }

                if (commandView == null)
                {
                    problems.Add("command camera/view");
                }

                if (officers == null || officers.Length != 2)
                {
                    problems.Add("two-officer array");
                }
                else
                {
                    if (officers[0] == null)
                    {
                        problems.Add("Officer Alpha reference");
                    }

                    if (officers[1] == null)
                    {
                        problems.Add("Officer Bravo reference");
                    }
                }

                if (orderMarker == null)
                {
                    problems.Add("order marker reference");
                }

                return string.Join(", ", problems);
            }
        }

        public void Configure(
            TacticalPlayerInput configuredInput,
            Transform configuredView,
            TacticalOfficerController[] configuredOfficers,
            OfficerOrderMarker configuredMarker,
            LayerMask configuredMask,
            float configuredDistance)
        {
            playerInput = configuredInput;
            commandView = configuredView;
            officers = configuredOfficers ?? Array.Empty<TacticalOfficerController>();
            orderMarker = configuredMarker;
            commandMask = configuredMask;
            commandDistance = Mathf.Max(2f, configuredDistance);
            RefreshOfficerSelection();
        }

        public void Select(OfficerSelection selection)
        {
            Selection = selection;
            RefreshOfficerSelection();
            SelectionChanged?.Invoke();
        }

        public void CycleSelection()
        {
            Select(Selection switch
            {
                OfficerSelection.OfficerOne => OfficerSelection.OfficerTwo,
                OfficerSelection.OfficerTwo => OfficerSelection.Team,
                _ => OfficerSelection.OfficerOne
            });
        }

        public int CancelSelectedOrders()
        {
            int cancelled = 0;
            foreach (TacticalOfficerController officer in GetSelectedOfficers())
            {
                if (officer != null && officer.CancelActiveOrder())
                {
                    cancelled++;
                }
            }

            LastCommandSummary = cancelled > 0
                ? $"Cancelled {cancelled} active officer order(s)."
                : "Selected officer(s) had no active order to cancel.";
            CommandIssued?.Invoke();
            return cancelled;
        }

        private void Awake()
        {
            playerInput ??= GetComponent<TacticalPlayerInput>();
            if (commandView == null)
            {
                Camera camera = GetComponentInChildren<Camera>(true);
                commandView = camera != null ? camera.transform : transform;
            }

            RefreshOfficerSelection();
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Officer Command",
                    "Milestone 4 squad references are incomplete: "
                        + ConfigurationProblems
                        + ". Run the Milestone 4 setup tool outside Play Mode.",
                    this);
            }
        }

        private void Update()
        {
            if (playerInput == null || !playerInput.GameplayEnabled)
            {
                return;
            }

            if (playerInput.SelectOfficerOnePressedThisFrame)
            {
                Select(OfficerSelection.OfficerOne);
            }
            else if (playerInput.SelectOfficerTwoPressedThisFrame)
            {
                Select(OfficerSelection.OfficerTwo);
            }
            else if (playerInput.SelectOfficerTeamPressedThisFrame)
            {
                Select(OfficerSelection.Team);
            }
            else if (playerInput.CycleOfficerSelectionPressedThisFrame)
            {
                CycleSelection();
            }

            if (playerInput.CancelOfficerOrderPressedThisFrame)
            {
                CancelSelectedOrders();
            }
            else if (playerInput.OfficerHoldPressedThisFrame)
            {
                IssueSimpleOrder(OfficerOrderType.HoldPosition, transform.position, gameObject);
            }
            else if (playerInput.OfficerFollowPressedThisFrame)
            {
                IssueSimpleOrder(OfficerOrderType.Follow, transform.position, gameObject);
            }
            else if (playerInput.OfficerStackPressedThisFrame)
            {
                IssueDoorOrder(OfficerOrderType.StackAtDoor);
            }
            else if (playerInput.OfficerOpenPressedThisFrame)
            {
                IssueDoorOrder(OfficerOrderType.OpenDoor);
            }
            else if (playerInput.OfficerRestrainPressedThisFrame)
            {
                IssueRestraintOrder();
            }
            else if (playerInput.OfficerMovePressedThisFrame)
            {
                IssueMoveOrder();
            }
            else if (playerInput.IssueOfficerContextOrderPressedThisFrame)
            {
                IssueContextOrder();
            }
        }

        private void IssueContextOrder()
        {
            if (!TryGetCommandHit(out RaycastHit hit))
            {
                RejectCommand("No physical command point was found under the reticle.");
                return;
            }

            CustodyComponent custody = hit.collider.GetComponentInParent<CustodyComponent>();
            if (custody != null)
            {
                IssueRestraintOrder(custody);
                return;
            }

            PrototypeDoor door = hit.collider.GetComponentInParent<PrototypeDoor>();
            if (door != null)
            {
                IssueDoorOrder(OfficerOrderType.StackAtDoor, door);
                return;
            }

            IssueOrders(OfficerOrderType.MoveTo, hit.point, null, 0UL, true);
        }

        private void IssueMoveOrder()
        {
            if (!TryGetCommandHit(out RaycastHit hit))
            {
                RejectCommand("Move order rejected: no physical point was found under the reticle.");
                return;
            }

            IssueOrders(OfficerOrderType.MoveTo, hit.point, null, 0UL, true);
        }

        private void IssueDoorOrder(OfficerOrderType type)
        {
            if (!TryGetCommandHit(out RaycastHit hit))
            {
                RejectCommand($"{type} rejected: no door was found under the reticle.");
                return;
            }

            PrototypeDoor door = hit.collider.GetComponentInParent<PrototypeDoor>();
            if (door == null)
            {
                RejectCommand($"{type} rejected: reticle target is not an operable door.");
                return;
            }

            IssueDoorOrder(type, door);
        }

        private void IssueDoorOrder(OfficerOrderType type, PrototypeDoor door)
        {
            Vector3 approach = door.transform.position - door.transform.forward * doorApproachDistance;
            ulong targetId = EntityId.ToULong(door.gameObject.GetEntityId());
            IssueOrders(type, approach, door, targetId, true);
        }

        private void IssueRestraintOrder()
        {
            if (!TryGetCommandHit(out RaycastHit hit))
            {
                RejectCommand("Restraint order rejected: no subject was found under the reticle.");
                return;
            }

            CustodyComponent custody = hit.collider.GetComponentInParent<CustodyComponent>();
            if (custody == null)
            {
                RejectCommand("Restraint order rejected: reticle target has no custody state.");
                return;
            }

            IssueRestraintOrder(custody);
        }

        private void IssueRestraintOrder(CustodyComponent custody)
        {
            ActorIdentity targetIdentity = custody.GetComponent<ActorIdentity>();
            Vector3 approach = custody.transform.position
                - custody.transform.forward * subjectApproachDistance;
            IssueOrders(
                OfficerOrderType.RestrainSubject,
                approach,
                custody,
                targetIdentity != null ? targetIdentity.RuntimeEntityId : 0UL,
                true);
        }

        private void IssueSimpleOrder(
            OfficerOrderType type,
            Vector3 targetPosition,
            UnityEngine.Object targetObject)
        {
            ulong targetId = targetObject is GameObject targetGameObject
                ? EntityId.ToULong(targetGameObject.GetEntityId())
                : 0UL;
            IssueOrders(type, targetPosition, targetObject, targetId, false);
        }

        private void IssueOrders(
            OfficerOrderType type,
            Vector3 targetPosition,
            UnityEngine.Object targetObject,
            ulong targetEntityId,
            bool showMarker)
        {
            List<TacticalOfficerController> selectedOfficers = GetSelectedOfficers();
            if (selectedOfficers.Count == 0)
            {
                RejectCommand("No valid officer is selected.");
                return;
            }

            long commandSequence = nextCommandSequence++;
            ulong issuerId = EntityId.ToULong(gameObject.GetEntityId());
            int accepted = 0;
            for (int index = 0; index < selectedOfficers.Count; index++)
            {
                TacticalOfficerController officer = selectedOfficers[index];
                if (officer == null || officer.Identity == null)
                {
                    continue;
                }

                Vector3 assignedPosition = CalculateAssignedPosition(
                    type,
                    targetPosition,
                    index,
                    selectedOfficers.Count,
                    targetObject);
                OfficerOrder order = new OfficerOrder(
                    commandSequence,
                    issuerId,
                    officer.Identity.RuntimeEntityId,
                    type,
                    assignedPosition,
                    targetObject,
                    targetEntityId,
                    Time.timeAsDouble);
                if (officer.AssignOrder(order))
                {
                    accepted++;
                }
            }

            LastCommandSummary =
                $"Command {commandSequence}: {type} issued to {selectedOfficers.Count}; "
                + $"{accepted} accepted for execution.";
            if (showMarker && orderMarker != null)
            {
                orderMarker.Show(targetPosition, type.ToString());
            }

            ProjectLog.Development("Officer Command", LastCommandSummary, this);
            CommandIssued?.Invoke();
        }

        private Vector3 CalculateAssignedPosition(
            OfficerOrderType type,
            Vector3 targetPosition,
            int index,
            int count,
            UnityEngine.Object targetObject)
        {
            if (count <= 1
                || type == OfficerOrderType.Follow
                || type == OfficerOrderType.HoldPosition
                || type == OfficerOrderType.RestrainSubject)
            {
                return targetPosition;
            }

            Vector3 lateral = commandView != null ? commandView.right : transform.right;
            if (targetObject is PrototypeDoor door)
            {
                lateral = door.transform.right;
            }

            float centeredIndex = index - (count - 1) * 0.5f;
            return targetPosition + lateral * (centeredIndex * teamSpacing * 2f);
        }

        private bool TryGetCommandHit(out RaycastHit hit)
        {
            hit = default;
            if (commandView == null)
            {
                return false;
            }

            return Physics.Raycast(
                commandView.position,
                commandView.forward,
                out hit,
                commandDistance,
                commandMask,
                QueryTriggerInteraction.Ignore);
        }

        private List<TacticalOfficerController> GetSelectedOfficers()
        {
            List<TacticalOfficerController> selected = new List<TacticalOfficerController>(2);
            if (officers == null || officers.Length < 2)
            {
                return selected;
            }

            if ((Selection == OfficerSelection.OfficerOne || Selection == OfficerSelection.Team)
                && officers[0] != null)
            {
                selected.Add(officers[0]);
            }

            if ((Selection == OfficerSelection.OfficerTwo || Selection == OfficerSelection.Team)
                && officers[1] != null)
            {
                selected.Add(officers[1]);
            }

            return selected;
        }

        private void RefreshOfficerSelection()
        {
            if (officers == null)
            {
                return;
            }

            for (int index = 0; index < officers.Length; index++)
            {
                TacticalOfficerController officer = officers[index];
                if (officer == null)
                {
                    continue;
                }

                bool isSelected = Selection == OfficerSelection.Team
                    || (Selection == OfficerSelection.OfficerOne && index == 0)
                    || (Selection == OfficerSelection.OfficerTwo && index == 1);
                officer.SetSelected(isSelected);
            }
        }

        private void RejectCommand(string reason)
        {
            LastCommandSummary = reason;
            ProjectLog.Warning("Officer Command", reason, this);
            CommandIssued?.Invoke();
        }
    }
}
