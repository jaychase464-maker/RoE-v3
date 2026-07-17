using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PrototypeControlPanel : InteractableBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");

        [SerializeField] private Renderer indicatorRenderer;
        [SerializeField] private Color inactiveColor = new Color(0.3f, 0.035f, 0.035f, 1f);
        [SerializeField] private Color activeColor = new Color(0.02f, 0.75f, 0.32f, 1f);
        [SerializeField, Min(0f)] private float holdDuration = 0.65f;

        private MaterialPropertyBlock propertyBlock;

        public bool IsActive { get; private set; }

        public void Configure(Renderer configuredIndicator)
        {
            indicatorRenderer = configuredIndicator;
            UpdateVisual();
        }

        public override InteractionPrompt GetPrompt(InteractionContext context)
        {
            return InteractionPrompt.Available(
                IsActive ? "Deactivate Training Panel" : "Activate Training Panel",
                holdDuration);
        }

        public override void Interact(InteractionContext context)
        {
            IsActive = !IsActive;
            UpdateVisual();
            ProjectLog.Info(
                "Interaction",
                $"Training panel is now {(IsActive ? "active" : "inactive")}.",
                this);
        }

        private void Awake()
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (indicatorRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            indicatorRenderer.GetPropertyBlock(propertyBlock);
            Color color = IsActive ? activeColor : inactiveColor;
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(EmissiveColorId, color * (IsActive ? 2f : 0.15f));
            indicatorRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
