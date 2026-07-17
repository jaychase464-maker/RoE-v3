using RulesOfEntry.Actors;
using UnityEngine;

namespace RulesOfEntry.AI
{
    public readonly struct VerbalCommandStimulus
    {
        public VerbalCommandStimulus(
            GameObject source,
            VerbalCommandType command,
            Vector3 sourcePosition,
            float issuedAtSeconds,
            float audibleRadius,
            bool weaponPresented)
        {
            Source = source;
            Command = command;
            SourcePosition = sourcePosition;
            IssuedAtSeconds = issuedAtSeconds;
            AudibleRadius = audibleRadius;
            WeaponPresented = weaponPresented;
        }

        public GameObject Source { get; }
        public VerbalCommandType Command { get; }
        public Vector3 SourcePosition { get; }
        public float IssuedAtSeconds { get; }
        public float AudibleRadius { get; }
        public bool WeaponPresented { get; }
    }

    public interface IVerbalCommandReceiver
    {
        HumanDecisionRecord ReceiveVerbalCommand(VerbalCommandStimulus stimulus);
    }
}
