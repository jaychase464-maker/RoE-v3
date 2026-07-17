using System;
using UnityEngine;

namespace RulesOfEntry.AI
{
    [CreateAssetMenu(
        fileName = "HumanBehaviorProfile",
        menuName = "Rules of Entry/AI/Human Behavior Profile")]
    public sealed class HumanBehaviorProfile : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)] private float baselineCompliance = 0.55f;
        [SerializeField, Range(0f, 1f)] private float aggression = 0.25f;
        [SerializeField, Range(0f, 1f)] private float deception = 0.1f;
        [SerializeField, Range(0f, 1f)] private float flightTendency = 0.35f;
        [SerializeField, Range(0f, 1f)] private float hideTendency = 0.25f;
        [SerializeField, Range(0f, 1f)] private float commandComprehension = 0.92f;
        [SerializeField, Min(0.1f)] private float minimumReactionSeconds = 0.45f;
        [SerializeField, Min(0.1f)] private float maximumReactionSeconds = 1.4f;
        [SerializeField, Min(0.5f)] private float movementSpeed = 2.8f;

        public float BaselineCompliance => baselineCompliance;
        public float Aggression => aggression;
        public float Deception => deception;
        public float FlightTendency => flightTendency;
        public float HideTendency => hideTendency;
        public float CommandComprehension => commandComprehension;
        public float MinimumReactionSeconds => minimumReactionSeconds;
        public float MaximumReactionSeconds => maximumReactionSeconds;
        public float MovementSpeed => movementSpeed;

        public void Configure(
            float configuredCompliance,
            float configuredAggression,
            float configuredDeception,
            float configuredFlightTendency,
            float configuredHideTendency,
            float configuredCommandComprehension,
            float configuredMinimumReactionSeconds,
            float configuredMaximumReactionSeconds,
            float configuredMovementSpeed)
        {
            baselineCompliance = Mathf.Clamp01(configuredCompliance);
            aggression = Mathf.Clamp01(configuredAggression);
            deception = Mathf.Clamp01(configuredDeception);
            flightTendency = Mathf.Clamp01(configuredFlightTendency);
            hideTendency = Mathf.Clamp01(configuredHideTendency);
            commandComprehension = Mathf.Clamp01(configuredCommandComprehension);
            minimumReactionSeconds = Mathf.Max(0.1f, configuredMinimumReactionSeconds);
            maximumReactionSeconds = Mathf.Max(
                minimumReactionSeconds,
                configuredMaximumReactionSeconds);
            movementSpeed = Mathf.Max(0.5f, configuredMovementSpeed);
        }

        private void OnValidate()
        {
            maximumReactionSeconds = Math.Max(
                minimumReactionSeconds,
                maximumReactionSeconds);
        }
    }
}
