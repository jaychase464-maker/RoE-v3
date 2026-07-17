using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.Combat
{
    [DisallowMultipleComponent]
    public sealed class PrototypeBallisticTarget : MonoBehaviour, IBallisticHitReceiver
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");

        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color untouchedColor = new Color(0.68f, 0.72f, 0.76f, 1f);
        [SerializeField] private Color struckColor = new Color(0.82f, 0.12f, 0.06f, 1f);

        private MaterialPropertyBlock propertyBlock;

        public int HitCount { get; private set; }
        public BallisticHit LastHit { get; private set; }

        public void Configure(Renderer configuredRenderer)
        {
            targetRenderer = configuredRenderer;
            UpdateVisual(untouchedColor);
        }

        public void ReceiveBallisticHit(BallisticHit hit)
        {
            LastHit = hit;
            HitCount++;
            UpdateVisual(struckColor);
            ProjectLog.Development(
                "Ballistics",
                $"{name} received hit {HitCount} at {hit.Point} with {hit.MuzzleEnergyJoules:F0} J muzzle energy.",
                this);
        }

        private void Awake()
        {
            UpdateVisual(HitCount > 0 ? struckColor : untouchedColor);
        }

        private void UpdateVisual(Color color)
        {
            if (targetRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(EmissiveColorId, color * 0.2f);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
