using RulesOfEntry.Core;
using RulesOfEntry.Input;
using UnityEngine;

namespace RulesOfEntry.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TacticalPlayerInput))]
    public sealed class CursorStateController : MonoBehaviour
    {
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private bool lockOnEnable = true;

        public bool IsLocked { get; private set; }

        public void Configure(TacticalPlayerInput configuredInput)
        {
            playerInput = configuredInput;
        }

        private void Awake()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<TacticalPlayerInput>();
            }
        }

        private void OnEnable()
        {
            if (playerInput == null)
            {
                ProjectLog.Error("Cursor", "TacticalPlayerInput is missing.", this);
                return;
            }

            playerInput.ToggleCursorRequested += ToggleCursor;
            SetLocked(lockOnEnable);
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.ToggleCursorRequested -= ToggleCursor;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            IsLocked = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsLocked)
            {
                SetLocked(false);
            }
        }

        private void ToggleCursor()
        {
            SetLocked(!IsLocked);
        }

        /// <summary>
        /// Allows an in-world interface such as the rugged planning tablet to
        /// explicitly transfer control between gameplay and UI navigation.
        /// </summary>
        public void SetCursorLocked(bool locked)
        {
            SetLocked(locked);
        }

        private void SetLocked(bool locked)
        {
            IsLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
            playerInput.SetGameplayEnabled(locked);
        }
    }
}
