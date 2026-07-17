using System;
using System.Collections.Generic;
using RulesOfEntry.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RulesOfEntry.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    public sealed class TacticalPlayerInput : MonoBehaviour
    {
        private const string PlayerMapName = "Player";
        private const string SystemMapName = "System";

        [SerializeField] private PlayerInput playerInput;

        private InputActionMap playerMap;
        private InputActionMap systemMap;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;
        private InputAction crouchAction;
        private InputAction interactAction;
        private InputAction fireAction;
        private InputAction aimAction;
        private InputAction reloadAction;
        private InputAction checkMagazineAction;
        private InputAction toggleReadyAction;
        private InputAction cycleFireSelectorAction;
        private InputAction cycleActionAction;
        private InputAction emergencyReloadModifierAction;
        private InputAction issueCommandAction;
        private InputAction selectOfficerOneAction;
        private InputAction selectOfficerTwoAction;
        private InputAction selectOfficerTeamAction;
        private InputAction cycleOfficerSelectionAction;
        private InputAction issueOfficerContextOrderAction;
        private InputAction officerMoveAction;
        private InputAction officerHoldAction;
        private InputAction officerFollowAction;
        private InputAction officerStackAction;
        private InputAction officerOpenAction;
        private InputAction officerRestrainAction;
        private InputAction cancelOfficerOrderAction;
        private InputAction toggleCursorAction;
        private bool actionsResolved;
        private bool inputConfigurationErrorLogged;

        public event Action ToggleCursorRequested;

        public Vector2 Move => ReadVector2(moveAction);
        public Vector2 Look => ReadVector2(lookAction);
        public bool SprintHeld => IsActionPressed(sprintAction);
        public bool CrouchPressedThisFrame => WasPressedThisFrame(crouchAction);
        public bool InteractPressedThisFrame => WasPressedThisFrame(interactAction);
        public bool InteractHeld => IsActionPressed(interactAction);
        public bool FirePressedThisFrame => WasPressedThisFrame(fireAction);
        public bool AimHeld => IsActionPressed(aimAction);
        public bool ReloadPressedThisFrame => WasPressedThisFrame(reloadAction);
        public bool CheckMagazinePressedThisFrame => WasPressedThisFrame(checkMagazineAction);
        public bool ToggleReadyPressedThisFrame => WasPressedThisFrame(toggleReadyAction);
        public bool CycleFireSelectorPressedThisFrame =>
            WasPressedThisFrame(cycleFireSelectorAction);
        public bool CycleActionPressedThisFrame => WasPressedThisFrame(cycleActionAction);
        public bool EmergencyReloadModifierHeld =>
            IsActionPressed(emergencyReloadModifierAction);
        public bool IssueCommandPressedThisFrame => WasPressedThisFrame(issueCommandAction);
        public bool SelectOfficerOnePressedThisFrame => WasPressedThisFrame(selectOfficerOneAction);
        public bool SelectOfficerTwoPressedThisFrame => WasPressedThisFrame(selectOfficerTwoAction);
        public bool SelectOfficerTeamPressedThisFrame => WasPressedThisFrame(selectOfficerTeamAction);
        public bool CycleOfficerSelectionPressedThisFrame =>
            WasPressedThisFrame(cycleOfficerSelectionAction);
        public bool IssueOfficerContextOrderPressedThisFrame =>
            WasPressedThisFrame(issueOfficerContextOrderAction);
        public bool OfficerMovePressedThisFrame => WasPressedThisFrame(officerMoveAction);
        public bool OfficerHoldPressedThisFrame => WasPressedThisFrame(officerHoldAction);
        public bool OfficerFollowPressedThisFrame => WasPressedThisFrame(officerFollowAction);
        public bool OfficerStackPressedThisFrame => WasPressedThisFrame(officerStackAction);
        public bool OfficerOpenPressedThisFrame => WasPressedThisFrame(officerOpenAction);
        public bool OfficerRestrainPressedThisFrame => WasPressedThisFrame(officerRestrainAction);
        public bool CancelOfficerOrderPressedThisFrame =>
            WasPressedThisFrame(cancelOfficerOrderAction);
        public bool GameplayEnabled => playerMap != null && playerMap.enabled;
        public bool IsUsingGamepad => string.Equals(
            playerInput != null ? playerInput.currentControlScheme : null,
            "Gamepad",
            StringComparison.Ordinal);

        public void Configure(PlayerInput configuredPlayerInput)
        {
            playerInput = configuredPlayerInput;
            actionsResolved = false;
            inputConfigurationErrorLogged = false;
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (!EnsureActions())
            {
                return;
            }

            if (enabled)
            {
                playerMap.Enable();
            }
            else
            {
                playerMap.Disable();
            }
        }

        public string GetInteractBindingDisplayString()
        {
            return GetBindingDisplayString(interactAction, "E", "X");
        }

        public string GetIssueCommandBindingDisplayString()
        {
            return GetBindingDisplayString(issueCommandAction, "F", "LB");
        }

        public string GetOfficerContextBindingDisplayString()
        {
            return GetBindingDisplayString(issueOfficerContextOrderAction, "G", "D-Pad Right");
        }

        public string GetOfficerSelectionBindingDisplayString()
        {
            return GetBindingDisplayString(cycleOfficerSelectionAction, "1 / 2 / 3", "B");
        }

        private void Awake()
        {
            EnsureActions();
        }

        private void OnEnable()
        {
            if (!EnsureActions())
            {
                return;
            }

            playerMap.Enable();
            systemMap.Enable();
            toggleCursorAction.performed += OnToggleCursorPerformed;
        }

        private void OnDisable()
        {
            if (!actionsResolved)
            {
                return;
            }

            toggleCursorAction.performed -= OnToggleCursorPerformed;
            playerMap.Disable();
            systemMap.Disable();
        }

        private bool EnsureActions()
        {
            if (actionsResolved)
            {
                return true;
            }

            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            if (playerInput == null || playerInput.actions == null)
            {
                LogInputConfigurationErrorOnce(
                    "PlayerInput or its InputActionAsset is missing. Run the current milestone setup tool.");
                return false;
            }

            playerMap = playerInput.actions.FindActionMap(PlayerMapName, false);
            systemMap = playerInput.actions.FindActionMap(SystemMapName, false);
            if (playerMap == null || systemMap == null)
            {
                LogInputConfigurationErrorOnce(
                    "ROE_InputActions must contain Player and System action maps.");
                return false;
            }

            moveAction = playerMap.FindAction("Move", false);
            lookAction = playerMap.FindAction("Look", false);
            sprintAction = playerMap.FindAction("Sprint", false);
            crouchAction = playerMap.FindAction("Crouch", false);
            interactAction = playerMap.FindAction("Interact", false);
            fireAction = playerMap.FindAction("Fire", false);
            aimAction = playerMap.FindAction("Aim", false);
            reloadAction = playerMap.FindAction("Reload", false);
            checkMagazineAction = playerMap.FindAction("CheckMagazine", false);
            toggleReadyAction = playerMap.FindAction("ToggleReady", false);
            cycleFireSelectorAction = playerMap.FindAction("CycleFireSelector", false);
            cycleActionAction = playerMap.FindAction("CycleAction", false);
            emergencyReloadModifierAction = playerMap.FindAction(
                "EmergencyReloadModifier",
                false);
            issueCommandAction = playerMap.FindAction("IssueCommand", false);
            selectOfficerOneAction = playerMap.FindAction("SelectOfficerOne", false);
            selectOfficerTwoAction = playerMap.FindAction("SelectOfficerTwo", false);
            selectOfficerTeamAction = playerMap.FindAction("SelectOfficerTeam", false);
            cycleOfficerSelectionAction = playerMap.FindAction("CycleOfficerSelection", false);
            issueOfficerContextOrderAction = playerMap.FindAction(
                "IssueOfficerContextOrder",
                false);
            officerMoveAction = playerMap.FindAction("OfficerMove", false);
            officerHoldAction = playerMap.FindAction("OfficerHold", false);
            officerFollowAction = playerMap.FindAction("OfficerFollow", false);
            officerStackAction = playerMap.FindAction("OfficerStack", false);
            officerOpenAction = playerMap.FindAction("OfficerOpen", false);
            officerRestrainAction = playerMap.FindAction("OfficerRestrain", false);
            cancelOfficerOrderAction = playerMap.FindAction("CancelOfficerOrder", false);
            toggleCursorAction = systemMap.FindAction("ToggleCursor", false);

            List<string> missingActions = new List<string>();
            AddMissingAction(missingActions, moveAction, "Player/Move");
            AddMissingAction(missingActions, lookAction, "Player/Look");
            AddMissingAction(missingActions, sprintAction, "Player/Sprint");
            AddMissingAction(missingActions, crouchAction, "Player/Crouch");
            AddMissingAction(missingActions, interactAction, "Player/Interact");
            AddMissingAction(missingActions, fireAction, "Player/Fire");
            AddMissingAction(missingActions, aimAction, "Player/Aim");
            AddMissingAction(missingActions, reloadAction, "Player/Reload");
            AddMissingAction(missingActions, checkMagazineAction, "Player/CheckMagazine");
            AddMissingAction(missingActions, toggleReadyAction, "Player/ToggleReady");
            AddMissingAction(
                missingActions,
                cycleFireSelectorAction,
                "Player/CycleFireSelector");
            AddMissingAction(missingActions, cycleActionAction, "Player/CycleAction");
            AddMissingAction(
                missingActions,
                emergencyReloadModifierAction,
                "Player/EmergencyReloadModifier");
            AddMissingAction(missingActions, issueCommandAction, "Player/IssueCommand");
            AddMissingAction(
                missingActions,
                selectOfficerOneAction,
                "Player/SelectOfficerOne");
            AddMissingAction(
                missingActions,
                selectOfficerTwoAction,
                "Player/SelectOfficerTwo");
            AddMissingAction(
                missingActions,
                selectOfficerTeamAction,
                "Player/SelectOfficerTeam");
            AddMissingAction(
                missingActions,
                cycleOfficerSelectionAction,
                "Player/CycleOfficerSelection");
            AddMissingAction(
                missingActions,
                issueOfficerContextOrderAction,
                "Player/IssueOfficerContextOrder");
            AddMissingAction(missingActions, officerMoveAction, "Player/OfficerMove");
            AddMissingAction(missingActions, officerHoldAction, "Player/OfficerHold");
            AddMissingAction(missingActions, officerFollowAction, "Player/OfficerFollow");
            AddMissingAction(missingActions, officerStackAction, "Player/OfficerStack");
            AddMissingAction(missingActions, officerOpenAction, "Player/OfficerOpen");
            AddMissingAction(
                missingActions,
                officerRestrainAction,
                "Player/OfficerRestrain");
            AddMissingAction(
                missingActions,
                cancelOfficerOrderAction,
                "Player/CancelOfficerOrder");
            AddMissingAction(missingActions, toggleCursorAction, "System/ToggleCursor");
            actionsResolved = missingActions.Count == 0;

            if (!actionsResolved)
            {
                LogInputConfigurationErrorOnce(
                    "Required input actions are missing: "
                        + string.Join(", ", missingActions)
                        + ". Run the Milestone 4 setup tool outside Play Mode.");
            }
            else
            {
                inputConfigurationErrorLogged = false;
            }

            return actionsResolved;
        }

        private string GetBindingDisplayString(
            InputAction action,
            string keyboardFallback,
            string gamepadFallback)
        {
            if (!EnsureActions())
            {
                return keyboardFallback;
            }

            string group = IsUsingGamepad ? "Gamepad" : "Keyboard&Mouse";
            string display = action.GetBindingDisplayString(group: group);
            return string.IsNullOrWhiteSpace(display)
                ? (IsUsingGamepad ? gamepadFallback : keyboardFallback)
                : display;
        }

        private void OnToggleCursorPerformed(InputAction.CallbackContext context)
        {
            ToggleCursorRequested?.Invoke();
        }

        private static Vector2 ReadVector2(InputAction action)
        {
            return action != null && action.enabled ? action.ReadValue<Vector2>() : Vector2.zero;
        }

        private static bool IsActionPressed(InputAction action)
        {
            return action != null && action.enabled && action.IsPressed();
        }

        private static bool WasPressedThisFrame(InputAction action)
        {
            return action != null && action.enabled && action.WasPressedThisFrame();
        }

        private void LogInputConfigurationErrorOnce(string message)
        {
            if (inputConfigurationErrorLogged)
            {
                return;
            }

            inputConfigurationErrorLogged = true;
            ProjectLog.Error("Input", message, this);
        }

        private static void AddMissingAction(
            ICollection<string> missingActions,
            InputAction action,
            string actionPath)
        {
            if (action == null)
            {
                missingActions.Add(actionPath);
            }
        }
    }
}
