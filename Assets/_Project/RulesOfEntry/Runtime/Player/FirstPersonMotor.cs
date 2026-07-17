using RulesOfEntry.Core;
using RulesOfEntry.Input;
using UnityEngine;

namespace RulesOfEntry.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(TacticalPlayerInput))]
    public sealed class FirstPersonMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private Transform cameraAnchor;

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float walkSpeed = 2.2f;
        [SerializeField, Min(0.1f)] private float sprintSpeed = 4.4f;
        [SerializeField, Min(0.1f)] private float crouchSpeed = 1.45f;
        [SerializeField, Min(0.1f)] private float acceleration = 14f;
        [SerializeField] private float gravity = -22f;

        [Header("Stance")]
        [SerializeField, Min(1f)] private float standingHeight = 1.8f;
        [SerializeField, Min(0.7f)] private float crouchingHeight = 1.15f;
        [SerializeField, Min(0.5f)] private float standingCameraHeight = 1.68f;
        [SerializeField, Min(0.4f)] private float crouchingCameraHeight = 1.02f;
        [SerializeField, Min(0.1f)] private float stanceTransitionSpeed = 3.5f;
        [SerializeField] private LayerMask obstructionMask = ~0;

        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private bool isCrouched;
        private bool initialized;

        public bool IsCrouched => isCrouched;
        public bool IsGrounded => characterController != null && characterController.isGrounded;
        public float CurrentPlanarSpeed => horizontalVelocity.magnitude;

        public void Configure(
            CharacterController configuredController,
            TacticalPlayerInput configuredInput,
            Transform configuredCameraAnchor,
            LayerMask configuredObstructionMask)
        {
            characterController = configuredController;
            playerInput = configuredInput;
            cameraAnchor = configuredCameraAnchor;
            obstructionMask = configuredObstructionMask;
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

            HandleStanceInput();
            UpdateStance(Time.deltaTime);
            UpdateMovement(Time.deltaTime);
        }

        private bool Initialize()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (playerInput == null)
            {
                playerInput = GetComponent<TacticalPlayerInput>();
            }

            initialized = characterController != null && playerInput != null && cameraAnchor != null;
            if (!initialized)
            {
                ProjectLog.Error(
                    "Player Motor",
                    "CharacterController, TacticalPlayerInput, or camera anchor is missing.",
                    this);
                return false;
            }

            characterController.height = standingHeight;
            characterController.center = new Vector3(0f, standingHeight * 0.5f, 0f);
            Vector3 cameraPosition = cameraAnchor.localPosition;
            cameraPosition.y = standingCameraHeight;
            cameraAnchor.localPosition = cameraPosition;
            return true;
        }

        private void HandleStanceInput()
        {
            if (!playerInput.CrouchPressedThisFrame)
            {
                return;
            }

            if (isCrouched && !CanStand())
            {
                ProjectLog.Development("Player Motor", "Standing blocked by overhead obstruction.", this);
                return;
            }

            isCrouched = !isCrouched;
        }

        private void UpdateStance(float deltaTime)
        {
            float targetHeight = isCrouched ? crouchingHeight : standingHeight;
            float targetCameraHeight = isCrouched ? crouchingCameraHeight : standingCameraHeight;
            float height = Mathf.MoveTowards(
                characterController.height,
                targetHeight,
                stanceTransitionSpeed * deltaTime);

            characterController.height = height;
            characterController.center = new Vector3(0f, height * 0.5f, 0f);

            Vector3 cameraPosition = cameraAnchor.localPosition;
            cameraPosition.y = Mathf.MoveTowards(
                cameraPosition.y,
                targetCameraHeight,
                stanceTransitionSpeed * deltaTime);
            cameraAnchor.localPosition = cameraPosition;
        }

        private void UpdateMovement(float deltaTime)
        {
            Vector2 moveInput = playerInput.Move;
            Vector3 desiredDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            if (desiredDirection.sqrMagnitude > 1f)
            {
                desiredDirection.Normalize();
            }

            float targetSpeed = isCrouched
                ? crouchSpeed
                : playerInput.SprintHeld && moveInput.y > 0.1f
                    ? sprintSpeed
                    : walkSpeed;

            Vector3 desiredVelocity = desiredDirection * targetSpeed;
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredVelocity,
                acceleration * deltaTime);

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += gravity * deltaTime;
            }

            Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(motion * deltaTime);
        }

        private bool CanStand()
        {
            float radius = Mathf.Max(0.05f, characterController.radius - characterController.skinWidth);
            Vector3 bottom = transform.position + Vector3.up * radius;
            Vector3 top = transform.position + Vector3.up * (standingHeight - radius);
            return !Physics.CheckCapsule(
                bottom,
                top,
                radius,
                obstructionMask,
                QueryTriggerInteraction.Ignore);
        }

        private void OnValidate()
        {
            standingHeight = Mathf.Max(1f, standingHeight);
            crouchingHeight = Mathf.Clamp(crouchingHeight, 0.7f, standingHeight - 0.1f);
            standingCameraHeight = Mathf.Clamp(standingCameraHeight, 0.5f, standingHeight);
            crouchingCameraHeight = Mathf.Clamp(
                crouchingCameraHeight,
                0.4f,
                crouchingHeight);
            gravity = Mathf.Min(-0.1f, gravity);
        }
    }
}
