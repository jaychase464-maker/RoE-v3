using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Characters;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class TemporaryCharacterPoseRulesTests
    {
        [TestCase(
            HumanBehaviorState.Idle,
            CustodyState.Free,
            ActorConditionLevel.Stable,
            TemporaryCharacterPose.Idle)]
        [TestCase(
            HumanBehaviorState.Threatening,
            CustodyState.Free,
            ActorConditionLevel.Stable,
            TemporaryCharacterPose.Alert)]
        [TestCase(
            HumanBehaviorState.Surrendering,
            CustodyState.Surrendering,
            ActorConditionLevel.Stable,
            TemporaryCharacterPose.Surrendering)]
        [TestCase(
            HumanBehaviorState.Surrendering,
            CustodyState.Restrained,
            ActorConditionLevel.Stable,
            TemporaryCharacterPose.Kneeling)]
        [TestCase(
            HumanBehaviorState.Idle,
            CustodyState.Free,
            ActorConditionLevel.Incapacitated,
            TemporaryCharacterPose.Incapacitated)]
        public void Resolve_PrioritizesPhysicalAndCustodyState(
            HumanBehaviorState behavior,
            CustodyState custody,
            ActorConditionLevel condition,
            TemporaryCharacterPose expected)
        {
            Assert.That(
                TemporaryCharacterPoseRules.Resolve(behavior, custody, condition),
                Is.EqualTo(expected));
        }
    }
}
