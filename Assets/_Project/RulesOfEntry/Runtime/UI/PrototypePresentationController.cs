using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RulesOfEntry.UI
{
    /// <summary>
    /// Keeps developer-only evidence panels available without making them permanent HUD clutter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypePresentationController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup[] developerPanels =
            System.Array.Empty<CanvasGroup>();
        [SerializeField] private CanvasGroup hintGroup;
        [SerializeField] private Text hintText;
        [SerializeField] private bool showDeveloperDiagnosticsByDefault;
        [SerializeField, Min(0.5f)] private float hintVisibleSeconds = 3.5f;

        private bool diagnosticsVisible;
        private float hintTimer;

        public bool DiagnosticsVisible => diagnosticsVisible;
        public bool HasCompleteConfiguration => developerPanels != null
            && developerPanels.Length > 0
            && hintGroup != null
            && hintText != null;

        public void Configure(
            CanvasGroup[] configuredDeveloperPanels,
            CanvasGroup configuredHintGroup,
            Text configuredHintText,
            bool configuredVisibleByDefault)
        {
            developerPanels = configuredDeveloperPanels
                ?? System.Array.Empty<CanvasGroup>();
            hintGroup = configuredHintGroup;
            hintText = configuredHintText;
            showDeveloperDiagnosticsByDefault = configuredVisibleByDefault;
        }

        private void Start()
        {
            SetDiagnosticsVisible(showDeveloperDiagnosticsByDefault);
        }

        private void Update()
        {
            if (Keyboard.current?.f10Key.wasPressedThisFrame == true)
            {
                SetDiagnosticsVisible(!diagnosticsVisible);
            }

            if (hintGroup == null)
            {
                return;
            }

            hintTimer -= Time.unscaledDeltaTime;
            float target = hintTimer > 0f ? 1f : 0f;
            hintGroup.alpha = Mathf.MoveTowards(
                hintGroup.alpha,
                target,
                Time.unscaledDeltaTime * 2.5f);
        }

        public void SetDiagnosticsVisible(bool visible)
        {
            diagnosticsVisible = visible;
            foreach (CanvasGroup panel in developerPanels
                ?? System.Array.Empty<CanvasGroup>())
            {
                if (panel == null)
                {
                    continue;
                }

                panel.alpha = visible ? 1f : 0f;
                panel.interactable = false;
                panel.blocksRaycasts = false;
            }

            if (hintText != null)
            {
                hintText.text = visible
                    ? "F10  •  SYSTEM DIAGNOSTICS ON"
                    : "F10  •  SYSTEM DIAGNOSTICS";
            }

            hintTimer = hintVisibleSeconds;
            if (hintGroup != null)
            {
                hintGroup.alpha = 1f;
                hintGroup.interactable = false;
                hintGroup.blocksRaycasts = false;
            }
        }
    }
}
