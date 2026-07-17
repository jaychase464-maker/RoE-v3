using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PrototypeDoor : InteractableBehaviour
    {
        [SerializeField] private Transform doorPivot;
        [SerializeField, Range(-170f, 170f)] private float openAngle = 100f;
        [SerializeField, Min(1f)] private float degreesPerSecond = 180f;

        private Quaternion closedRotation;
        private Quaternion openRotation;

        public bool IsOpen { get; private set; }

        public void Configure(Transform configuredPivot, float configuredOpenAngle = 100f)
        {
            doorPivot = configuredPivot;
            openAngle = Mathf.Clamp(configuredOpenAngle, -170f, 170f);
            CacheRotations();
        }

        public override InteractionPrompt GetPrompt(InteractionContext context)
        {
            return InteractionPrompt.Available(IsOpen ? "Close Door" : "Open Door");
        }

        public override void Interact(InteractionContext context)
        {
            IsOpen = !IsOpen;
            ProjectLog.Development(
                "Interaction",
                $"{name} target state changed to {(IsOpen ? "open" : "closed")}.",
                this);
        }

        private void Awake()
        {
            if (doorPivot == null)
            {
                doorPivot = transform;
            }

            CacheRotations();
        }

        private void Update()
        {
            if (doorPivot == null)
            {
                return;
            }

            Quaternion target = IsOpen ? openRotation : closedRotation;
            doorPivot.localRotation = Quaternion.RotateTowards(
                doorPivot.localRotation,
                target,
                degreesPerSecond * Time.deltaTime);
        }

        private void CacheRotations()
        {
            if (doorPivot == null)
            {
                return;
            }

            closedRotation = doorPivot.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        }
    }
}
