using RulesOfEntry.Actors;

namespace RulesOfEntry.Characters
{
    public enum TemporaryCharacterPose
    {
        Idle = 0,
        Alert = 1,
        Surrendering = 2,
        Kneeling = 3,
        Incapacitated = 4
    }

    /// <summary>
    /// Keeps the temporary model presentation subordinate to authoritative actor state.
    /// </summary>
    public static class TemporaryCharacterPoseRules
    {
        public static TemporaryCharacterPose Resolve(
            HumanBehaviorState behavior,
            CustodyState custody,
            ActorConditionLevel condition)
        {
            if (condition == ActorConditionLevel.Incapacitated
                || condition == ActorConditionLevel.Deceased
                || behavior == HumanBehaviorState.Incapacitated)
            {
                return TemporaryCharacterPose.Incapacitated;
            }

            if (custody == CustodyState.Kneeling
                || custody == CustodyState.Restrained
                || custody == CustodyState.Searched
                || custody == CustodyState.InCustody
                || behavior == HumanBehaviorState.Restrained)
            {
                return TemporaryCharacterPose.Kneeling;
            }

            if (custody == CustodyState.Surrendering
                || behavior == HumanBehaviorState.Surrendering
                || behavior == HumanBehaviorState.Complying)
            {
                return TemporaryCharacterPose.Surrendering;
            }

            if (behavior == HumanBehaviorState.Threatening
                || behavior == HumanBehaviorState.Resisting
                || behavior == HumanBehaviorState.Fleeing)
            {
                return TemporaryCharacterPose.Alert;
            }

            return TemporaryCharacterPose.Idle;
        }
    }
}
