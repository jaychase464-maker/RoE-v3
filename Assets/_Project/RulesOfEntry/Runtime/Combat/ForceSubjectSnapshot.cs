using RulesOfEntry.Actors;

namespace RulesOfEntry.Combat
{
    /// <summary>
    /// Factual pre-impact subject state captured for later ROE review. It contains no judgment.
    /// </summary>
    public readonly struct ForceSubjectSnapshot
    {
        public ForceSubjectSnapshot(
            bool hasActor,
            string actorId,
            ActorRole role,
            ActorConditionLevel condition,
            CustodyState custody,
            HumanBehaviorState behavior,
            bool weaponAccessible)
        {
            HasActor = hasActor;
            ActorId = actorId ?? string.Empty;
            Role = role;
            Condition = condition;
            Custody = custody;
            Behavior = behavior;
            WeaponAccessible = weaponAccessible;
        }

        public bool HasActor { get; }
        public string ActorId { get; }
        public ActorRole Role { get; }
        public ActorConditionLevel Condition { get; }
        public CustodyState Custody { get; }
        public HumanBehaviorState Behavior { get; }
        public bool WeaponAccessible { get; }

        public static ForceSubjectSnapshot None => new ForceSubjectSnapshot(
            false,
            string.Empty,
            ActorRole.Civilian,
            ActorConditionLevel.Stable,
            CustodyState.Free,
            HumanBehaviorState.Idle,
            false);
    }
}
