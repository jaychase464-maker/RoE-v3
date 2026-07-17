using RulesOfEntry.Interaction;
using RulesOfEntry.Planning;
using RulesOfEntry.UI.Planning;
using UnityEngine;

namespace RulesOfEntry.Headquarters
{
    [DisallowMultipleComponent]
    public sealed class HeadquartersMissionTerminalInteractable : InteractableBehaviour
    {
        [SerializeField] private RuggedTabletController tabletController;
        [SerializeField] private OperationBriefingDefinition operation;
        [SerializeField, Min(0f)] private float reviewHoldSeconds = 0.35f;

        public OperationBriefingDefinition Operation => operation;
        public float ReviewHoldSeconds => reviewHoldSeconds;
        public bool HasCompleteConfiguration => tabletController != null
            && operation != null
            && operation.HasValidConfiguration;

        public void Configure(
            RuggedTabletController configuredTabletController,
            OperationBriefingDefinition configuredOperation,
            float configuredReviewHoldSeconds)
        {
            tabletController = configuredTabletController;
            operation = configuredOperation;
            reviewHoldSeconds = Mathf.Max(0f, configuredReviewHoldSeconds);
        }

        public override InteractionPrompt GetPrompt(InteractionContext context)
        {
            if (!HasCompleteConfiguration)
            {
                return InteractionPrompt.Unavailable(
                    "Review Available Operation",
                    "The headquarters mission terminal is not configured.");
            }

            if (tabletController.IsOpen)
            {
                return InteractionPrompt.Unavailable(
                    "Planning Tablet Active",
                    "Close the tablet before selecting another operation.");
            }

            return InteractionPrompt.Available(
                $"Review {operation.OperationCode}: {operation.Mission.DisplayName}",
                reviewHoldSeconds);
        }

        public override void Interact(InteractionContext context)
        {
            if (HasCompleteConfiguration)
            {
                tabletController.OpenBriefing(operation);
            }
        }
    }
}
