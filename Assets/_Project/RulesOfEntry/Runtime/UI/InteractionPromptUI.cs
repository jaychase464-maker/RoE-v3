using RulesOfEntry.Core;
using RulesOfEntry.Input;
using RulesOfEntry.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace RulesOfEntry.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [Header("Runtime Sources")]
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private TacticalPlayerInput playerInput;

        [Header("Visuals")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text keyText;
        [SerializeField] private Text promptText;
        [SerializeField] private Image progressFill;

        public void ConfigureVisuals(
            CanvasGroup configuredCanvasGroup,
            Text configuredKeyText,
            Text configuredPromptText,
            Image configuredProgressFill)
        {
            canvasGroup = configuredCanvasGroup;
            keyText = configuredKeyText;
            promptText = configuredPromptText;
            progressFill = configuredProgressFill;
            Hide();
        }

        public void ConfigureSources(
            PlayerInteractor configuredInteractor,
            TacticalPlayerInput configuredInput)
        {
            interactor = configuredInteractor;
            playerInput = configuredInput;
        }

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            Hide();
        }

        private void LateUpdate()
        {
            if (interactor == null || playerInput == null)
            {
                Hide();
                return;
            }

            if (!interactor.HasFocus || !playerInput.GameplayEnabled)
            {
                Hide();
                return;
            }

            InteractionPrompt prompt = interactor.CurrentPrompt;
            if (keyText != null)
            {
                keyText.text = playerInput.GetInteractBindingDisplayString().ToUpperInvariant();
            }

            if (promptText != null)
            {
                promptText.text = prompt.IsAvailable
                    ? prompt.ActionText
                    : string.IsNullOrWhiteSpace(prompt.UnavailableReason)
                        ? "Unavailable"
                        : prompt.UnavailableReason;
                promptText.color = prompt.IsAvailable
                    ? Color.white
                    : new Color(1f, 0.45f, 0.4f, 1f);
            }

            if (progressFill != null)
            {
                progressFill.gameObject.SetActive(prompt.HoldDuration > 0f);
                progressFill.fillAmount = interactor.HoldProgress01;
            }

            Show();
        }

        private void Show()
        {
            if (canvasGroup == null)
            {
                ProjectLog.Error("Interaction UI", "CanvasGroup reference is missing.", this);
                return;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void Hide()
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            if (progressFill != null)
            {
                progressFill.fillAmount = 0f;
            }
        }
    }
}
