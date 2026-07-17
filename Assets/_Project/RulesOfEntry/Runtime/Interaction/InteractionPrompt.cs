using System;

namespace RulesOfEntry.Interaction
{
    public readonly struct InteractionPrompt
    {
        public InteractionPrompt(
            string actionText,
            bool isAvailable,
            string unavailableReason,
            float holdDuration)
        {
            ActionText = string.IsNullOrWhiteSpace(actionText) ? "Interact" : actionText.Trim();
            IsAvailable = isAvailable;
            UnavailableReason = unavailableReason ?? string.Empty;
            HoldDuration = Math.Max(0f, holdDuration);
        }

        public string ActionText { get; }
        public bool IsAvailable { get; }
        public string UnavailableReason { get; }
        public float HoldDuration { get; }

        public static InteractionPrompt Available(string actionText, float holdDuration = 0f)
        {
            return new InteractionPrompt(actionText, true, string.Empty, holdDuration);
        }

        public static InteractionPrompt Unavailable(string actionText, string reason)
        {
            return new InteractionPrompt(actionText, false, reason, 0f);
        }
    }
}
