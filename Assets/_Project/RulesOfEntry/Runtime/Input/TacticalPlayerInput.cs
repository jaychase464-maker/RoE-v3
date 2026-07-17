using System;
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
        private InputAction toggleCursorAction;
        private bool actionsResolved;

        public event Action ToggleCursorRequested;

        public Vector2 Move => ReadVector2(moveAction);
        public Vector2 Look => ReadVector2(lookAction);
        public bool SprintHeld => IsActionPressed(sprintAction);
        public bool CrouchPressedThisFrame => WasPressedThisFrame(crouchAction);
        public bool InteractPressedThisFrame => WasPressedThisFrame(interactAction);
        public bool InteractHeld => IsActionPressed(interactAction);
        public bool GameplayEnabled => playerMap != null && playerMap.enabled;
        public bool IsUsingGamepad => string.Equals(
            playerInput != null ? playerInput.currentControlScheme : null,
            "Gamepad",
            StringComparison.Ordinal);

        public void Configure(PlayerInput configuredPlayerInput)
        {
            playerInput = configuredPlayerInput;
            actionsResolved = false;
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
            if (!EnsureActions())
            {
                return "E";
            }

            string group = IsUsingGamepad ? "Gamepad" : "Keyboard&Mouse";
            string display = interactAction.GetBindingDisplayString(group: group);
            return string.IsNullOrWhiteSpace(display) ? (IsUsingGamepad ? "X" : "E") : display;
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
                ProjectLog.Error(
                    "Input",
                    "PlayerInput or its InputActionAsset is missing. Run the Milestone 1 setup tool.",
                    this);
                return false;
            }

            playerMap = playerInput.actions.FindActionMap(PlayerMapName, false);
            systemMap = playerInput.actions.FindActionMap(SystemMapName, false);
            if (playerMap == null || systemMap == null)
            {
                ProjectLog.Error(
                    "Input",
                    "ROE_InputActions must contain Player and System action maps.",
                    this);
                return false;
            }

            moveAction = playerMap.FindAction("Move", false);
            lookAction = playerMap.FindAction("Look", false);
            sprintAction = playerMap.FindAction("Sprint", false);
            crouchAction = playerMap.FindAction("Crouch", false);
            interactAction = playerMap.FindAction("Interact", false);
            toggleCursorAction = systemMap.FindAction("ToggleCursor", false);

            actionsResolved = moveAction != null
                && lookAction != null
                && sprintAction != null
                && crouchAction != null
                && interactAction != null
                && toggleCursorAction != null;

            if (!actionsResolved)
            {
                ProjectLog.Error(
                    "Input",
                    "One or more required Milestone 1 input actions are missing.",
                    this);
            }

            return actionsResolved;
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
    }
}
