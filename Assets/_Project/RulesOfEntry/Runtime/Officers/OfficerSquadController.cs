using System;
using System.Collections.Generic;
using System.Linq;
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
        private int selectedOfficerIndex = -1;

        public event Action SelectionChanged;
        public event Action CommandIssued;

        public OfficerSelection Selection { get; private set; } = OfficerSelection.Team;
        public int SelectedOfficerIndex => selectedOfficerIndex;
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

                if (officers == null || officers.Length < 1)
                {
                    problems.Add("squad array with at least one officer");
                }
                else
                {
                    for (int index = 0; index < officers.Length; index++)
                    {
                        if (officers[index] == null)
                        {
                            problems.Add($"officer reference {index + 1}");
                        }
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

        /// <summary>
        /// Replaces only the scene-owned officer roster after deployment while
        /// preserving command input, view, marker, masks, and distances.
        /// </summary>
        public void SetDeployedOfficers(
            TacticalOfficerController[] configuredOfficers)
        {
            officers = configuredOfficers?
                .Where(officer => officer != null)
                .ToArray() ?? Array.Empty<TacticalOfficerController>();
            selectedOfficerIndex = -1;
            Selection = OfficerSelection.Team;
            RefreshOfficerSelection();
            SelectionChanged?.Invoke();
        }

        public void Select(OfficerSelection selection)
        {
            Selection = selection;
            selectedOfficerIndex = selection switch
            {
                OfficerSelection.OfficerOne => 0,
                OfficerSelection.OfficerTwo => 1,
                _ => -1
            };
            RefreshOfficerSelection();
            SelectionChanged?.Invoke();
        }

        public bool SelectOfficer(int officerIndex)
        {
            if (officers == null
                || officerIndex < 0
                || officerIndex >= officers.Length
                || officers[officerIndex] == null)
            {
                return false;
            }

            selectedOfficerIndex = officerIndex;
            Selection = officerIndex switch
            {
                0 => OfficerSelection.OfficerOne,
                1 => OfficerSelection.OfficerTwo,
                _ => OfficerSelection.Individual
            };
            RefreshOfficerSelection();
            SelectionChanged?.Invoke();
            return true;
        }

        public void CycleSelection()
        {
            if (officers == null || officers.Length == 0)
            {
                Select(OfficerSelection.Team);
                return;
            }

            int nextIndex = selectedOfficerIndex + 1;
            if (selectedOfficerIndex < 0)
            {
                nextIndex = 0;
            }

            while (nextIndex < officers.Length && officers[nextIndex] == null)
            {
                nextIndex++;
            }

            if (nextIndex >= officers.Length)
            {
                Select(OfficerSelection.Team);
                return;
            }

            SelectOfficer(nextIndex);
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

            bool commandMenuHeld = playerInput.OfficerCommandMenuHeld;
            if (!commandMenuHeld && playerInput.SelectOfficerOnePressedThisFrame)
            {
                Select(OfficerSelection.OfficerOne);
            }
            else if (!commandMenuHeld && playerInput.SelectOfficerTwoPressedThisFrame)
            {
                Select(OfficerSelection.OfficerTwo);
            }
            else if (!commandMenuHeld && playerInput.SelectOfficerTeamPressedThisFrame)
            {
                Select(OfficerSelection.Team);
            }
            else if (!commandMenuHeld && playerInput.CycleOfficerSelectionPressedThisFrame)
            {
                CycleSelection();
            }

            if (playerInput.CancelOfficerOrderPressedThisFrame)
            {
                CancelSelectedOrders();
            }
            else if (!commandMenuHeld)
            {
                if (playerInput.IssueOfficerContextOrderPressedThisFrame)
                {
                    IssueContextOrder();
                }

                return;
            }
            else
            {
                IssueCommandSlot(playerInput.OfficerCommandSlotPressedThisFrame);
            }
        }

        public bool IssueCommandSlot(int slot)
        {
            if (!OfficerCommandSlotRules.TryGetOrderType(slot, out OfficerOrderType orderType))
            {
                return false;
            }

            switch (orderType)
            {
                case OfficerOrderType.MoveTo:
                    IssueMoveOrder();
                    return true;
                case OfficerOrderType.HoldPosition:
                    IssueSimpleOrder(
                        OfficerOrderType.HoldPosition,
                        transform.position,
                        gameObject);
                    return true;
                case OfficerOrderType.StackAtDoor:
                    IssueDoorOrder(OfficerOrderType.StackAtDoor);
                    return true;
                case OfficerOrderType.OpenDoor:
                    IssueDoorOrder(OfficerOrderType.OpenDoor);
                    return true;
                case OfficerOrderType.Follow:
                    IssueSimpleOrder(
                        OfficerOrderType.Follow,
                        transform.position,
                        gameObject);
                    return true;
                case OfficerOrderType.RestrainSubject:
                    IssueRestraintOrder();
                    return true;
                default:
                    return false;
            }
        }

        public bool TryGetCurrentCommandContext(out OfficerCommandContext context)
        {
            context = default;
            if (!TryGetCommandHit(out RaycastHit hit))
            {
                return false;
            }

            float distance = commandView != null
                ? Vector3.Distance(commandView.position, hit.point)
                : 0f;
            CustodyComponent custody = hit.collider.GetComponentInParent<CustodyComponent>();
            if (custody != null)
            {
                ActorIdentity identity = custody.GetComponent<ActorIdentity>();
                context = new OfficerCommandContext(
                    OfficerCommandTargetType.Subject,
                    identity != null ? identity.DisplayName : "Subject",
                    distance);
                return true;
            }

            PrototypeDoor door = hit.collider.GetComponentInParent<PrototypeDoor>();
            if (door != null)
            {
                context = new OfficerCommandContext(
                    OfficerCommandTargetType.Door,
                    "Door",
                    distance);
                return true;
            }

            context = new OfficerCommandContext(
                OfficerCommandTargetType.Position,
                "Position",
                distance);
            return true;
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
            List<TacticalOfficerController> selected = new List<TacticalOfficerController>(
                officers?.Length ?? 0);
            if (officers == null || officers.Length == 0)
            {
                return selected;
            }

            if (selectedOfficerIndex >= 0 && selectedOfficerIndex < officers.Length)
            {
                TacticalOfficerController officer = officers[selectedOfficerIndex];
                if (officer != null)
                {
                    selected.Add(officer);
                }

                return selected;
            }

            foreach (TacticalOfficerController officer in officers)
            {
                if (officer != null)
                {
                    selected.Add(officer);
                }
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

                bool isSelected = selectedOfficerIndex < 0 || selectedOfficerIndex == index;
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
