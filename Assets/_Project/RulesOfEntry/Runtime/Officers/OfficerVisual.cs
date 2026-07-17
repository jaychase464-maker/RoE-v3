using RulesOfEntry.Actors;
using UnityEngine;

namespace RulesOfEntry.Officers
{
    [DisallowMultipleComponent]
    public sealed class OfficerVisual : MonoBehaviour
    {
        [SerializeField] private TextMesh statusText;

        public void Configure(TextMesh configuredStatusText)
        {
            statusText = configuredStatusText;
        }

        public void SetPresentation(
            ActorIdentity identity,
            bool selected,
            OfficerOrderStatus status,
            OfficerOrderType? orderType,
            OfficerOrderOutcomeReason reason,
            string activity)
        {
            if (statusText == null)
            {
                return;
            }

            string selection = selected ? "< SELECTED" : string.Empty;
            string order = orderType.HasValue ? orderType.Value.ToString() : "No order";
            string outcome = reason == OfficerOrderOutcomeReason.None
                ? string.Empty
                : $"\n{reason}";
            statusText.text =
                $"{identity?.DisplayName ?? name} {selection}\n{order} • {status}\n{activity}{outcome}";
            statusText.color = selected
                ? new Color(0.35f, 0.88f, 1f, 1f)
                : Color.white;
        }
    }
}
