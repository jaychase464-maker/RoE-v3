using RulesOfEntry.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace RulesOfEntry.UI
{
    /// <summary>
    /// Presents observable weapon state and action feedback. It intentionally has no round counter.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class WeaponStatusUI : MonoBehaviour
    {
        [SerializeField] private FirearmController firearmController;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text mechanicalStateText;
        [SerializeField] private Text statusMessageText;
        [SerializeField] private Image operationProgressFill;

        public void ConfigureVisuals(
            CanvasGroup configuredCanvasGroup,
            Text configuredMechanicalStateText,
            Text configuredStatusMessageText,
            Image configuredProgressFill)
        {
            canvasGroup = configuredCanvasGroup;
            mechanicalStateText = configuredMechanicalStateText;
            statusMessageText = configuredStatusMessageText;
            operationProgressFill = configuredProgressFill;
        }

        public void ConfigureSource(FirearmController configuredController)
        {
            firearmController = configuredController;
        }

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void LateUpdate()
        {
            if (firearmController == null)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            FirearmSnapshot snapshot = firearmController.Snapshot;
            if (mechanicalStateText != null)
            {
                string selector = snapshot.Selector == FireSelectorPosition.Safe ? "SAFE" : "SEMI";
                string posture = !firearmController.WeaponIsRaised
                    && firearmController.EffectiveReadyPosition == WeaponReadyPosition.Shouldered
                    ? "RAISING"
                    : firearmController.IsAiming
                        ? "AIMED"
                        : firearmController.EffectiveReadyPosition == WeaponReadyPosition.LowReady
                            ? "LOW READY"
                            : "SHOULDERED";
                mechanicalStateText.text = selector + "  •  " + posture;
            }

            if (statusMessageText != null)
            {
                bool hasMessage = firearmController.StatusMessageRemaining > 0f;
                statusMessageText.gameObject.SetActive(hasMessage);
                statusMessageText.text = hasMessage
                    ? firearmController.CurrentStatusMessage
                    : string.Empty;
            }

            if (operationProgressFill != null)
            {
                bool operationActive = firearmController.Operation != FirearmOperation.Idle;
                operationProgressFill.transform.parent.gameObject.SetActive(operationActive);
                operationProgressFill.fillAmount = firearmController.OperationProgress;
            }
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
