using UnityEngine;

namespace RulesOfEntry.Officers
{
    [DisallowMultipleComponent]
    public sealed class OfficerOrderMarker : MonoBehaviour
    {
        [SerializeField] private GameObject markerRoot;
        [SerializeField] private TextMesh label;
        [SerializeField, Min(0.5f)] private float visibleSeconds = 6f;

        private float remainingSeconds;

        public void Configure(GameObject configuredRoot, TextMesh configuredLabel)
        {
            markerRoot = configuredRoot;
            label = configuredLabel;
            Hide();
        }

        public void Show(Vector3 worldPosition, string text)
        {
            transform.position = worldPosition + Vector3.up * 0.025f;
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }

            remainingSeconds = visibleSeconds;
            if (markerRoot != null)
            {
                markerRoot.SetActive(true);
            }
        }

        public void Hide()
        {
            remainingSeconds = 0f;
            if (markerRoot != null)
            {
                markerRoot.SetActive(false);
            }
        }

        private void Update()
        {
            if (remainingSeconds <= 0f)
            {
                return;
            }

            remainingSeconds -= Time.deltaTime;
            if (remainingSeconds <= 0f)
            {
                Hide();
            }
        }
    }
}
