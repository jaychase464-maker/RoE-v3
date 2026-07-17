using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RulesOfEntry.UI.FrontEnd
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class FrontEndButtonVisual : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image accent;
        [SerializeField] private Text label;
        [SerializeField] private Color normalColor = new Color(0.035f, 0.05f, 0.065f, 0.86f);
        [SerializeField] private Color highlightedColor = new Color(0.075f, 0.11f, 0.14f, 0.98f);
        [SerializeField] private Color pressedColor = new Color(0.13f, 0.17f, 0.2f, 1f);
        [SerializeField] private Color disabledColor = new Color(0.025f, 0.03f, 0.035f, 0.55f);
        [SerializeField] private Color accentColor = new Color(0.22f, 0.62f, 0.86f, 1f);
        [SerializeField] private Color labelColor = new Color(0.86f, 0.9f, 0.93f, 1f);
        [SerializeField] private Color highlightedLabelColor = Color.white;
        [SerializeField, Min(1f)] private float responseSpeed = 14f;

        private bool selected;
        private bool pointerInside;
        private bool pressed;

        public void Configure(
            Button configuredButton,
            Image configuredBackground,
            Image configuredAccent,
            Text configuredLabel)
        {
            button = configuredButton;
            background = configuredBackground;
            accent = configuredAccent;
            label = configuredLabel;
            ApplyImmediate();
        }

        private void Awake()
        {
            button ??= GetComponent<Button>();
            background ??= GetComponent<Image>();
            ApplyImmediate();
        }

        private void Update()
        {
            bool interactive = button != null && button.interactable;
            bool highlighted = interactive && (selected || pointerInside);
            Color targetBackground = !interactive
                ? disabledColor
                : pressed
                    ? pressedColor
                    : highlighted
                        ? highlightedColor
                        : normalColor;
            Color targetLabel = highlighted ? highlightedLabelColor : labelColor;
            float targetAccentAlpha = highlighted ? 1f : 0.42f;
            float targetScale = pressed ? 0.992f : highlighted ? 1.012f : 1f;
            float blend = 1f - Mathf.Exp(-responseSpeed * Time.unscaledDeltaTime);

            if (background != null)
            {
                background.color = Color.Lerp(background.color, targetBackground, blend);
            }

            if (label != null)
            {
                label.color = Color.Lerp(label.color, targetLabel, blend);
            }

            if (accent != null)
            {
                Color targetAccent = accentColor;
                targetAccent.a = targetAccentAlpha;
                accent.color = Color.Lerp(accent.color, targetAccent, blend);
            }

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.one * targetScale,
                blend);
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            pressed = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerInside = true;
            if (button != null && button.interactable && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;
            pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = eventData.button == PointerEventData.InputButton.Left;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
        }

        private void ApplyImmediate()
        {
            if (background != null)
            {
                background.color = normalColor;
            }

            if (accent != null)
            {
                Color color = accentColor;
                color.a = 0.42f;
                accent.color = color;
            }

            if (label != null)
            {
                label.color = labelColor;
            }

            transform.localScale = Vector3.one;
        }
    }
}
