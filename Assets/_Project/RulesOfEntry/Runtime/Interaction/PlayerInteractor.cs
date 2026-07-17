using RulesOfEntry.Core;
using RulesOfEntry.Input;
using UnityEngine;

namespace RulesOfEntry.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TacticalPlayerInput))]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0.25f)] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactionMask;

        private InteractableBehaviour focusedInteractable;
        private InteractableBehaviour heldInteractable;
        private InteractionPrompt currentPrompt;
        private float heldSeconds;
        private bool interactionConsumedForPress;
        private bool initialized;

        public bool HasFocus => focusedInteractable != null;
        public InteractableBehaviour FocusedInteractable => focusedInteractable;
        public InteractionPrompt CurrentPrompt => currentPrompt;
        public float HoldProgress01 { get; private set; }

        public void Configure(
            TacticalPlayerInput configuredInput,
            Camera configuredCamera,
            LayerMask configuredInteractionMask)
        {
            playerInput = configuredInput;
            viewCamera = configuredCamera;
            interactionMask = configuredInteractionMask;
            initialized = false;
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (!initialized && !Initialize())
            {
                return;
            }

            if (!playerInput.GameplayEnabled || Cursor.lockState != CursorLockMode.Locked)
            {
                ClearFocus();
                return;
            }

            UpdateFocus();
            UpdateInteraction(Time.deltaTime);
        }

        private bool Initialize()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<TacticalPlayerInput>();
            }

            initialized = playerInput != null && viewCamera != null;
            if (!initialized)
            {
                ProjectLog.Error("Interaction", "Player input or view camera is missing.", this);
            }

            return initialized;
        }

        private void UpdateFocus()
        {
            Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            InteractableBehaviour nextFocus = null;

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactionDistance,
                interactionMask,
                QueryTriggerInteraction.Collide))
            {
                nextFocus = hit.collider.GetComponentInParent<InteractableBehaviour>();
            }

            if (nextFocus != focusedInteractable)
            {
                focusedInteractable = nextFocus;
                ResetHold();
            }

            currentPrompt = focusedInteractable != null
                ? focusedInteractable.GetPrompt(CreateContext())
                : default;
        }

        private void UpdateInteraction(float deltaTime)
        {
            if (focusedInteractable == null || !currentPrompt.IsAvailable)
            {
                ResetHold();
                return;
            }

            if (!playerInput.InteractHeld)
            {
                ResetHold();
            }

            if (currentPrompt.HoldDuration <= 0f)
            {
                if (playerInput.InteractPressedThisFrame)
                {
                    ExecuteInteraction(focusedInteractable);
                }

                return;
            }

            if (playerInput.InteractPressedThisFrame)
            {
                heldInteractable = focusedInteractable;
                heldSeconds = 0f;
                HoldProgress01 = 0f;
                interactionConsumedForPress = false;
            }

            if (heldInteractable != focusedInteractable
                || !playerInput.InteractHeld
                || interactionConsumedForPress)
            {
                return;
            }

            heldSeconds += deltaTime;
            HoldProgress01 = Mathf.Clamp01(heldSeconds / currentPrompt.HoldDuration);
            if (HoldProgress01 >= 1f)
            {
                interactionConsumedForPress = true;
                ExecuteInteraction(focusedInteractable);
            }
        }

        private void ExecuteInteraction(InteractableBehaviour interactable)
        {
            if (interactable == null)
            {
                return;
            }

            interactable.Interact(CreateContext());
            currentPrompt = interactable.GetPrompt(CreateContext());
        }

        private InteractionContext CreateContext()
        {
            return new InteractionContext(gameObject, viewCamera.transform, Time.time);
        }

        private void ClearFocus()
        {
            focusedInteractable = null;
            currentPrompt = default;
            ResetHold();
        }

        private void ResetHold()
        {
            heldInteractable = null;
            heldSeconds = 0f;
            HoldProgress01 = 0f;
            interactionConsumedForPress = false;
        }
    }
}
