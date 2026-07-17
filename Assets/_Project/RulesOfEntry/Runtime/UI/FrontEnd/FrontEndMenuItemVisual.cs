using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RulesOfEntry.UI.FrontEnd
{
    /// <summary>
    /// Restrained text-navigation response for the cinematic main menu.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class FrontEndMenuItemVisual : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private Text label;
        [SerializeField] private Image selectionBar;
        [SerializeField] private Image divider;
        [SerializeField] private Color normalColor = new Color(0.82f, 0.85f, 0.87f, 1f);
        [SerializeField] private Color focusedColor = Color.white;
        [SerializeField] private Color pressedColor = new Color(0.55f, 0.78f, 0.94f, 1f);
        [SerializeField] private Color disabledColor = new Color(0.38f, 0.42f, 0.45f, 0.72f);
        [SerializeField] private Color accentColor = new Color(0.1f, 0.58f, 1f, 1f);
        [SerializeField, Min(0f)] private float focusedOffset = 12f;
        [SerializeField, Min(1f)] private float responseSpeed = 18f;

        private RectTransform labelRect;
        private Vector2 restingPosition;
        private bool selected;
        private bool pointerInside;
        private bool pressed;

        public void Configure(
            Button configuredButton,
            Text configuredLabel,
            Image configuredSelectionBar,
            Image configuredDivider)
        {
            button = configuredButton;
            label = configuredLabel;
            selectionBar = configuredSelectionBar;
            divider = configuredDivider;
            CacheLayout();
            ApplyImmediate();
        }

        private void Awake()
        {
            button ??= GetComponent<Button>();
            label ??= GetComponentInChildren<Text>(true);
            CacheLayout();
            ApplyImmediate();
        }

        private void Update()
        {
            bool interactive = button != null && button.interactable;
            bool focused = interactive && (selected || pointerInside);
            Color targetLabel = !interactive
                ? disabledColor
                : pressed
                    ? pressedColor
                    : focused
                        ? focusedColor
                        : normalColor;
            float accentAlpha = focused ? 1f : 0f;
            float blend = 1f - Mathf.Exp(-responseSpeed * Time.unscaledDeltaTime);

            if (label != null)
            {
                label.color = Color.Lerp(label.color, targetLabel, blend);
            }

            if (selectionBar != null)
            {
                Color targetAccent = accentColor;
                targetAccent.a = accentAlpha;
                selectionBar.color = Color.Lerp(
                    selectionBar.color,
                    targetAccent,
                    blend);
            }

            if (divider != null)
            {
                Color targetDivider = focused
                    ? new Color(accentColor.r, accentColor.g, accentColor.b, 0.36f)
                    : new Color(0.48f, 0.57f, 0.62f, 0.14f);
                divider.color = Color.Lerp(divider.color, targetDivider, blend);
            }

            if (labelRect != null)
            {
                Vector2 target = restingPosition
                    + new Vector2(focused ? focusedOffset : 0f, 0f);
                labelRect.anchoredPosition = Vector2.Lerp(
                    labelRect.anchoredPosition,
                    target,
                    blend);
            }
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

        private void CacheLayout()
        {
            labelRect = label != null ? label.rectTransform : null;
            if (labelRect != null)
            {
                restingPosition = labelRect.anchoredPosition;
            }
        }

        private void ApplyImmediate()
        {
            bool interactive = button == null || button.interactable;
            if (label != null)
            {
                label.color = interactive ? normalColor : disabledColor;
            }

            if (selectionBar != null)
            {
                Color color = accentColor;
                color.a = 0f;
                selectionBar.color = color;
            }

            if (divider != null)
            {
                divider.color = new Color(0.48f, 0.57f, 0.62f, 0.14f);
            }
        }
    }
}
