using UnityEngine;

namespace RulesOfEntry.AI
{
    [DisallowMultipleComponent]
    public sealed class HumanPerception : MonoBehaviour
    {
        [SerializeField] private Transform eyeTransform;
        [SerializeField, Min(1f)] private float sightDistance = 24f;
        [SerializeField, Range(20f, 180f)] private float fieldOfViewDegrees = 125f;
        [SerializeField, Min(1f)] private float hearingDistance = 18f;
        [SerializeField] private LayerMask occlusionMask = ~0;

        public Vector3 LastKnownOfficerPosition { get; private set; }
        public float LastOfficerStimulusTime { get; private set; } = float.NegativeInfinity;

        public void Configure(
            Transform configuredEyes,
            float configuredSightDistance,
            float configuredFieldOfViewDegrees,
            float configuredHearingDistance,
            LayerMask configuredOcclusionMask)
        {
            eyeTransform = configuredEyes;
            sightDistance = Mathf.Max(1f, configuredSightDistance);
            fieldOfViewDegrees = Mathf.Clamp(configuredFieldOfViewDegrees, 20f, 180f);
            hearingDistance = Mathf.Max(1f, configuredHearingDistance);
            occlusionMask = configuredOcclusionMask;
        }

        public bool CanPerceiveCommand(VerbalCommandStimulus stimulus)
        {
            Vector3 origin = EyePosition;
            Vector3 offset = stimulus.SourcePosition - origin;
            float distance = offset.magnitude;
            float maximumHearingDistance = Mathf.Min(hearingDistance, stimulus.AudibleRadius);
            if (distance > maximumHearingDistance)
            {
                return false;
            }

            bool unobstructed = !Physics.Raycast(
                origin,
                offset.normalized,
                out RaycastHit hit,
                distance,
                occlusionMask,
                QueryTriggerInteraction.Ignore)
                || stimulus.Source == null
                || hit.transform.IsChildOf(stimulus.Source.transform);
            bool perceived = unobstructed || distance <= maximumHearingDistance * 0.45f;
            if (perceived)
            {
                RememberOfficer(stimulus.SourcePosition, stimulus.IssuedAtSeconds);
            }

            return perceived;
        }

        public bool CanSee(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 origin = EyePosition;
            Vector3 targetPosition = target.position + Vector3.up * 1.2f;
            Vector3 offset = targetPosition - origin;
            float distance = offset.magnitude;
            if (distance > sightDistance || distance <= 0.001f)
            {
                return false;
            }

            if (Vector3.Angle(transform.forward, offset) > fieldOfViewDegrees * 0.5f)
            {
                return false;
            }

            if (Physics.Raycast(
                origin,
                offset.normalized,
                out RaycastHit hit,
                distance,
                occlusionMask,
                QueryTriggerInteraction.Ignore)
                && !hit.transform.IsChildOf(target))
            {
                return false;
            }

            RememberOfficer(target.position, Time.time);
            return true;
        }

        private Vector3 EyePosition => eyeTransform != null
            ? eyeTransform.position
            : transform.position + Vector3.up * 1.6f;

        private void RememberOfficer(Vector3 position, float time)
        {
            LastKnownOfficerPosition = position;
            LastOfficerStimulusTime = time;
        }
    }
}
