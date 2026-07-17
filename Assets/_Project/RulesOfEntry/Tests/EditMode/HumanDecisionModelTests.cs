using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using UnityEngine;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class HumanDecisionModelTests
    {
        [Test]
        public void DeterministicRandom_SameSeedProducesSameSequence()
        {
            DeterministicDecisionRandom first = new DeterministicDecisionRandom(31031);
            DeterministicDecisionRandom second = new DeterministicDecisionRandom(31031);

            for (int index = 0; index < 12; index++)
            {
                Assert.That(first.Next01(), Is.EqualTo(second.Next01()));
            }
        }

        [Test]
        public void LowMoraleSuspect_SurrendersWithInspectibleReason()
        {
            HumanBehaviorProfile profile = CreateProfile(
                compliance: 0.42f,
                aggression: 0.18f,
                deception: 0f,
                flight: 0.3f,
                hide: 0.1f);
            CommandDecisionContext context = new CommandDecisionContext(
                ActorRole.Suspect,
                VerbalCommandType.PoliceShowHands,
                true,
                false,
                true,
                true,
                true,
                5f,
                0.62f,
                0.2f,
                ActorConditionLevel.Stable,
                profile);

            CommandDecision decision = HumanDecisionModel.EvaluateCommand(
                context,
                0.3f,
                0.8f,
                0.5f);

            Assert.That(decision.State, Is.EqualTo(HumanBehaviorState.Surrendering));
            Assert.That(decision.Reason, Is.EqualTo(HumanDecisionReason.LowMorale));
            Assert.That(decision.Deceptive, Is.False);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void PanickedCivilian_MayFleeInsteadOfInstantCompliance()
        {
            HumanBehaviorProfile profile = CreateProfile(
                compliance: 0.02f,
                aggression: 0f,
                deception: 0f,
                flight: 0.85f,
                hide: 0.2f);
            CommandDecisionContext context = new CommandDecisionContext(
                ActorRole.Civilian,
                VerbalCommandType.PoliceShowHands,
                true,
                false,
                true,
                false,
                false,
                9f,
                0.96f,
                0.8f,
                ActorConditionLevel.Stable,
                profile);

            CommandDecision decision = HumanDecisionModel.EvaluateCommand(
                context,
                0.99f,
                0.5f,
                0.2f);

            Assert.That(decision.State, Is.EqualTo(HumanBehaviorState.Fleeing));
            Assert.That(decision.Reason, Is.EqualTo(HumanDecisionReason.HighPanic));
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void UnheardCommand_ProducesExplicitNoPerceptionReason()
        {
            HumanBehaviorProfile profile = CreateProfile(1f, 0f, 0f, 0f, 0f);
            CommandDecisionContext context = new CommandDecisionContext(
                ActorRole.Civilian,
                VerbalCommandType.Stop,
                false,
                false,
                true,
                false,
                false,
                30f,
                0.3f,
                0.8f,
                ActorConditionLevel.Stable,
                profile);

            CommandDecision decision = HumanDecisionModel.EvaluateCommand(
                context,
                0.1f,
                0.1f,
                0.1f);

            Assert.That(decision.Reason, Is.EqualTo(HumanDecisionReason.CommandNotPerceived));
            Object.DestroyImmediate(profile);
        }

        private static HumanBehaviorProfile CreateProfile(
            float compliance,
            float aggression,
            float deception,
            float flight,
            float hide)
        {
            HumanBehaviorProfile profile = ScriptableObject.CreateInstance<HumanBehaviorProfile>();
            profile.Configure(
                compliance,
                aggression,
                deception,
                flight,
                hide,
                1f,
                0.4f,
                1.2f,
                3f);
            return profile;
        }
    }
}
