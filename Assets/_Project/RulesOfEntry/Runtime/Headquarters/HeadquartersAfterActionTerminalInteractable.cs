using RulesOfEntry.Interaction;
using RulesOfEntry.Operations;
using RulesOfEntry.UI.Headquarters;
using UnityEngine;

namespace RulesOfEntry.Headquarters
{
    [DisallowMultipleComponent]
    public sealed class HeadquartersAfterActionTerminalInteractable : InteractableBehaviour
    {
        [SerializeField] private HeadquartersAfterActionReviewController reviewController;
        [SerializeField, Min(0f)] private float reviewHoldSeconds;

        public HeadquartersAfterActionReviewController ReviewController => reviewController;
        public bool HasCompleteConfiguration => reviewController != null;

        public void Configure(
            HeadquartersAfterActionReviewController configuredController,
            float configuredReviewHoldSeconds)
        {
            reviewController = configuredController;
            reviewHoldSeconds = Mathf.Max(0f, configuredReviewHoldSeconds);
        }

        public override InteractionPrompt GetPrompt(InteractionContext context)
        {
            if (!HasCompleteConfiguration)
            {
                return InteractionPrompt.Unavailable(
                    "Review Last Operation",
                    "The after-action terminal is unavailable.");
            }

            if (reviewController.IsOpen)
            {
                return InteractionPrompt.Unavailable(
                    "After-Action Review Active",
                    "Close the current report first.");
            }

            if (!CompletedOperationContext.TryGetLatest(out CompletedOperationRecord record))
            {
                return InteractionPrompt.Unavailable(
                    "Review Last Operation",
                    "No completed operation is recorded in this session.");
            }

            return InteractionPrompt.Available(
                $"Review {record.OperationCode}: Tier {record.Report.Tier}",
                reviewHoldSeconds);
        }

        public override void Interact(InteractionContext context)
        {
            reviewController?.OpenLatest();
        }
    }
}
