using RulesOfEntry.Core;
using RulesOfEntry.Input;
using UnityEngine;

namespace RulesOfEntry.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TacticalPlayerInput))]
    public sealed class FirstPersonLook : MonoBehaviour
    {
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private Transform body;
        [SerializeField] private Transform cameraPivot;
        [SerializeField, Range(0.01f, 0.5f)] private float mouseSensitivity = 0.08f;
        [SerializeField, Range(30f, 360f)] private float gamepadDegreesPerSecond = 145f;
        [SerializeField, Range(45f, 89f)] private float verticalLimit = 85f;

        private float pitch;
        private bool initialized;

        public void Configure(
            TacticalPlayerInput configuredInput,
            Transform configuredBody,
            Transform configuredCameraPivot)
        {
            playerInput = configuredInput;
            body = configuredBody;
            cameraPivot = configuredCameraPivot;
            initialized = false;
        }

        public void ApplyRecoil(float verticalDegrees, float horizontalDegrees)
        {
            if ((!initialized && !Initialize()) || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            pitch = Mathf.Clamp(
                pitch - Mathf.Max(0f, verticalDegrees),
                -verticalLimit,
                verticalLimit);
            body.Rotate(Vector3.up, horizontalDegrees, Space.World);
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if ((!initialized && !Initialize()) || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            Vector2 look = playerInput.Look;
            float scale = playerInput.IsUsingGamepad
                ? gamepadDegreesPerSecond * Time.deltaTime
                : mouseSensitivity;

            float yawDelta = look.x * scale;
            float pitchDelta = look.y * scale;
            pitch = Mathf.Clamp(pitch - pitchDelta, -verticalLimit, verticalLimit);

            body.Rotate(Vector3.up, yawDelta, Space.World);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private bool Initialize()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<TacticalPlayerInput>();
            }

            if (body == null)
            {
                body = transform;
            }

            initialized = playerInput != null && body != null && cameraPivot != null;
            if (!initialized)
            {
                ProjectLog.Error("Player Look", "Input, body, or camera pivot is missing.", this);
            }

            return initialized;
        }
    }
}
