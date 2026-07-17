using RulesOfEntry.Interaction;
using UnityEngine;

namespace RulesOfEntry.Missions
{
    [DisallowMultipleComponent]
    public sealed class MissionDebriefInteractable : InteractableBehaviour
    {
        [SerializeField] private MissionController missionController;
        [SerializeField, Min(0.5f)] private float confirmationHoldSeconds = 1.25f;

        public MissionController MissionController => missionController;
        public bool HasCompleteConfiguration => missionController != null;

        public void Configure(
            MissionController configuredController,
            float configuredConfirmationHoldSeconds)
        {
            missionController = configuredController;
            confirmationHoldSeconds = Mathf.Max(0.5f, configuredConfirmationHoldSeconds);
        }

        public override InteractionPrompt GetPrompt(InteractionContext context)
        {
            if (missionController == null)
            {
                return InteractionPrompt.Unavailable(
                    "Begin Debrief",
                    "Mission controller is unavailable.");
            }

            return missionController.Phase switch
            {
                MissionPhase.Active => InteractionPrompt.Available(
                    "End Operation and Begin Debrief",
                    confirmationHoldSeconds),
                MissionPhase.AfterAction => InteractionPrompt.Unavailable(
                    "After-Action Report Complete",
                    "The operation has already ended."),
                _ => InteractionPrompt.Unavailable(
                    "Begin Debrief",
                    "The operation has not started.")
            };
        }

        public override void Interact(InteractionContext context)
        {
            missionController?.RequestAfterActionReview();
        }
    }
}
