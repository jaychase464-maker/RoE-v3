using RulesOfEntry.Actors;
using UnityEngine;

namespace RulesOfEntry.AI
{
    [DisallowMultipleComponent]
    public sealed class ActorVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");

        [SerializeField] private Renderer[] renderers;
        [SerializeField] private TextMesh statusText;
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;

        private MaterialPropertyBlock propertyBlock;

        public void Configure(
            Renderer[] configuredRenderers,
            TextMesh configuredStatusText,
            Transform configuredBodyRoot,
            Transform configuredLeftArm,
            Transform configuredRightArm)
        {
            renderers = configuredRenderers;
            statusText = configuredStatusText;
            bodyRoot = configuredBodyRoot;
            leftArm = configuredLeftArm;
            rightArm = configuredRightArm;
        }

        public void SetPresentation(
            ActorIdentity identity,
            HumanBehaviorState behaviorState,
            CustodyState custodyState,
            ActorConditionLevel conditionLevel,
            HumanDecisionReason reason)
        {
            Color color = GetColor(identity != null ? identity.Role : ActorRole.Civilian,
                behaviorState,
                custodyState,
                conditionLevel);
            ApplyColor(color);
            ApplyPose(behaviorState, custodyState, conditionLevel);

            if (statusText != null)
            {
                string displayName = identity != null ? identity.DisplayName : name;
                statusText.text = $"{displayName}\n{GetReadableState(behaviorState, custodyState)}\n{reason}";
            }
        }

        private void ApplyColor(Color color)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            foreach (Renderer targetRenderer in renderers ?? System.Array.Empty<Renderer>())
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(EmissiveColorId, color * 0.06f);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ApplyPose(
            HumanBehaviorState behaviorState,
            CustodyState custodyState,
            ActorConditionLevel conditionLevel)
        {
            bool incapacitated = conditionLevel == ActorConditionLevel.Incapacitated
                || conditionLevel == ActorConditionLevel.Deceased;
            bool kneeling = custodyState == CustodyState.Kneeling
                || custodyState == CustodyState.Restrained
                || custodyState == CustodyState.Searched
                || custodyState == CustodyState.InCustody;
            bool handsVisible = behaviorState == HumanBehaviorState.Surrendering
                || behaviorState == HumanBehaviorState.Complying
                || custodyState == CustodyState.Surrendering;

            if (bodyRoot != null)
            {
                bodyRoot.localPosition = incapacitated
                    ? new Vector3(0f, 0.18f, 0f)
                    : kneeling
                        ? new Vector3(0f, -0.38f, 0f)
                        : Vector3.zero;
                bodyRoot.localRotation = incapacitated
                    ? Quaternion.Euler(0f, 0f, 86f)
                    : Quaternion.identity;
            }

            if (leftArm != null)
            {
                leftArm.localRotation = handsVisible
                    ? Quaternion.Euler(0f, 0f, -145f)
                    : kneeling
                        ? Quaternion.Euler(20f, 0f, 18f)
                        : Quaternion.Euler(0f, 0f, -8f);
            }

            if (rightArm != null)
            {
                rightArm.localRotation = handsVisible
                    ? Quaternion.Euler(0f, 0f, 145f)
                    : kneeling
                        ? Quaternion.Euler(20f, 0f, -18f)
                        : Quaternion.Euler(0f, 0f, 8f);
            }
        }

        private static Color GetColor(
            ActorRole role,
            HumanBehaviorState behaviorState,
            CustodyState custodyState,
            ActorConditionLevel conditionLevel)
        {
            if (conditionLevel == ActorConditionLevel.Incapacitated
                || conditionLevel == ActorConditionLevel.Deceased)
            {
                return new Color(0.18f, 0.18f, 0.2f, 1f);
            }

            if (custodyState == CustodyState.Restrained
                || custodyState == CustodyState.Searched
                || custodyState == CustodyState.InCustody)
            {
                return new Color(0.12f, 0.58f, 0.38f, 1f);
            }

            if (behaviorState == HumanBehaviorState.Surrendering
                || behaviorState == HumanBehaviorState.Complying)
            {
                return new Color(0.12f, 0.56f, 0.86f, 1f);
            }

            if (behaviorState == HumanBehaviorState.Threatening)
            {
                return new Color(0.88f, 0.08f, 0.04f, 1f);
            }

            return role == ActorRole.Suspect
                ? new Color(0.62f, 0.24f, 0.1f, 1f)
                : new Color(0.74f, 0.62f, 0.22f, 1f);
        }

        private static string GetReadableState(
            HumanBehaviorState behaviorState,
            CustodyState custodyState)
        {
            return custodyState == CustodyState.Free
                ? behaviorState.ToString().ToUpperInvariant()
                : custodyState.ToString().ToUpperInvariant();
        }
    }
}
