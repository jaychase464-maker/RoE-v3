using RulesOfEntry.Actors;

namespace RulesOfEntry.AI
{
    public readonly struct CommandDecisionContext
    {
        public CommandDecisionContext(
            ActorRole role,
            VerbalCommandType command,
            bool perceived,
            bool restrained,
            bool canAct,
            bool weaponPresented,
            bool actorHasWeapon,
            float distanceMeters,
            float stress,
            float morale,
            ActorConditionLevel conditionLevel,
            HumanBehaviorProfile profile)
        {
            Role = role;
            Command = command;
            Perceived = perceived;
            Restrained = restrained;
            CanAct = canAct;
            WeaponPresented = weaponPresented;
            ActorHasWeapon = actorHasWeapon;
            DistanceMeters = distanceMeters;
            Stress = stress;
            Morale = morale;
            ConditionLevel = conditionLevel;
            Profile = profile;
        }

        public ActorRole Role { get; }
        public VerbalCommandType Command { get; }
        public bool Perceived { get; }
        public bool Restrained { get; }
        public bool CanAct { get; }
        public bool WeaponPresented { get; }
        public bool ActorHasWeapon { get; }
        public float DistanceMeters { get; }
        public float Stress { get; }
        public float Morale { get; }
        public ActorConditionLevel ConditionLevel { get; }
        public HumanBehaviorProfile Profile { get; }
    }

    public readonly struct CommandDecision
    {
        public CommandDecision(
            HumanBehaviorState state,
            HumanDecisionReason reason,
            bool deceptive,
            float complianceScore,
            float decisionRoll)
        {
            State = state;
            Reason = reason;
            Deceptive = deceptive;
            ComplianceScore = complianceScore;
            DecisionRoll = decisionRoll;
        }

        public HumanBehaviorState State { get; }
        public HumanDecisionReason Reason { get; }
        public bool Deceptive { get; }
        public float ComplianceScore { get; }
        public float DecisionRoll { get; }
    }

    public sealed class HumanDecisionRecord
    {
        public HumanDecisionRecord(
            long sequence,
            double occurredAtSeconds,
            VerbalCommandType command,
            HumanBehaviorState previousState,
            HumanBehaviorState newState,
            HumanDecisionReason reason,
            bool deceptive,
            float complianceScore,
            float decisionRoll,
            float stress,
            float morale)
        {
            Sequence = sequence;
            OccurredAtSeconds = occurredAtSeconds;
            Command = command;
            PreviousState = previousState;
            NewState = newState;
            Reason = reason;
            Deceptive = deceptive;
            ComplianceScore = complianceScore;
            DecisionRoll = decisionRoll;
            Stress = stress;
            Morale = morale;
        }

        public long Sequence { get; }
        public double OccurredAtSeconds { get; }
        public VerbalCommandType Command { get; }
        public HumanBehaviorState PreviousState { get; }
        public HumanBehaviorState NewState { get; }
        public HumanDecisionReason Reason { get; }
        public bool Deceptive { get; }
        public float ComplianceScore { get; }
        public float DecisionRoll { get; }
        public float Stress { get; }
        public float Morale { get; }
    }
}
