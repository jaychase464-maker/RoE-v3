using RulesOfEntry.Actors;
using UnityEngine;

namespace RulesOfEntry.Officers
{
    /// <summary>
    /// Owns the physical officer body-camera viewpoint. Rendering is enabled
    /// only while an authorized tablet is watching this source.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TacticalOfficerController))]
    public sealed class OfficerBodyCameraSource : MonoBehaviour
    {
        [SerializeField] private TacticalOfficerController officer;
        [SerializeField] private Camera bodyCamera;
        [SerializeField] private string cameraIdentifier = "BC-UNASSIGNED";
        [SerializeField] private bool signalAvailable = true;
        [SerializeField] private bool recording = true;

        public TacticalOfficerController Officer => officer;
        public ActorIdentity Identity => officer != null ? officer.Identity : null;
        public string CameraIdentifier => cameraIdentifier;
        public bool SignalAvailable => signalAvailable;
        public bool Recording => recording;
        public bool IsStreaming => bodyCamera != null && bodyCamera.enabled
            && bodyCamera.targetTexture != null;
        public bool HasCompleteConfiguration => officer != null
            && officer.Identity != null
            && bodyCamera != null
            && !string.IsNullOrWhiteSpace(cameraIdentifier);

        public void Configure(
            TacticalOfficerController configuredOfficer,
            Camera configuredCamera,
            string configuredCameraIdentifier,
            bool configuredSignalAvailable = true,
            bool configuredRecording = true)
        {
            officer = configuredOfficer;
            bodyCamera = configuredCamera;
            cameraIdentifier = string.IsNullOrWhiteSpace(configuredCameraIdentifier)
                ? "BC-UNASSIGNED"
                : configuredCameraIdentifier.Trim();
            signalAvailable = configuredSignalAvailable;
            recording = configuredRecording;
            StopStreaming();
        }

        public bool BeginStreaming(RenderTexture target)
        {
            if (!HasCompleteConfiguration || !signalAvailable || target == null)
            {
                StopStreaming();
                return false;
            }

            bodyCamera.targetTexture = target;
            bodyCamera.enabled = true;
            return true;
        }

        public void StopStreaming()
        {
            if (bodyCamera == null)
            {
                return;
            }

            bodyCamera.enabled = false;
            bodyCamera.targetTexture = null;
        }

        public void SetSignalAvailable(bool available)
        {
            signalAvailable = available;
            if (!available)
            {
                StopStreaming();
            }
        }

        public void SetRecording(bool isRecording)
        {
            recording = isRecording;
        }

        private void Awake()
        {
            officer ??= GetComponent<TacticalOfficerController>();
            StopStreaming();
        }

        private void OnDisable()
        {
            StopStreaming();
        }
    }
}
