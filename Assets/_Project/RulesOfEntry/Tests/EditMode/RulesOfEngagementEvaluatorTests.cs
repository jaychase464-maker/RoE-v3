using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Combat;
using RulesOfEntry.Missions;
using UnityEngine;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class RulesOfEngagementEvaluatorTests
    {
        private RulesOfEngagementPolicy policy;

        [SetUp]
        public void SetUp()
        {
            policy = ScriptableObject.CreateInstance<RulesOfEngagementPolicy>();
            policy.Configure(
                "test_roe",
                "Test ROE",
                string.Empty,
                5,
                20,
                45,
                59);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(policy);
        }

        [Test]
        public void ThreateningArmedSuspect_IsWithinPolicy()
        {
            ForceEventRecord forceEvent = CreateForceEvent(new ForceSubjectSnapshot(
                true,
                "armed_suspect",
                ActorRole.Suspect,
                ActorConditionLevel.Stable,
                CustodyState.Free,
                HumanBehaviorState.Threatening,
                true));

            RoeFinding finding = RulesOfEngagementEvaluator.Evaluate(policy, forceEvent);

            Assert.That(finding.Determination, Is.EqualTo(RoeDetermination.WithinPolicy));
            Assert.That(finding.ScoreDeduction, Is.Zero);
        }

        [TestCase(ActorRole.Civilian, CustodyState.Free, HumanBehaviorState.Idle)]
        [TestCase(ActorRole.Suspect, CustodyState.Surrendering, HumanBehaviorState.Surrendering)]
        public void ProtectedOrControlledPerson_IsCriticalViolation(
            ActorRole role,
            CustodyState custody,
            HumanBehaviorState behavior)
        {
            ForceEventRecord forceEvent = CreateForceEvent(new ForceSubjectSnapshot(
                true,
                "protected_person",
                role,
                ActorConditionLevel.Stable,
                custody,
                behavior,
                false));

            RoeFinding finding = RulesOfEngagementEvaluator.Evaluate(policy, forceEvent);

            Assert.That(finding.Determination, Is.EqualTo(RoeDetermination.Violation));
            Assert.That(finding.Severity, Is.EqualTo(RoeSeverity.Critical));
            Assert.That(finding.ScoreDeduction, Is.EqualTo(45));
        }

        [Test]
        public void DischargeWithoutActorImpact_RequiresReviewWithoutAutomaticPenalty()
        {
            ForceEventRecord forceEvent = CreateForceEvent(ForceSubjectSnapshot.None);

            RoeFinding finding = RulesOfEngagementEvaluator.Evaluate(policy, forceEvent);

            Assert.That(finding.Determination, Is.EqualTo(RoeDetermination.ReviewRequired));
            Assert.That(finding.ScoreDeduction, Is.Zero);
        }

        internal static ForceEventRecord CreateForceEvent(ForceSubjectSnapshot subject)
        {
            return new ForceEventRecord(
                1,
                2d,
                10,
                100UL,
                "test_firearm",
                "test_ammunition",
                FireSelectorPosition.SemiAutomatic,
                WeaponReadyPosition.Shouldered,
                Vector3.zero,
                Vector3.forward,
                subject.HasActor,
                Vector3.forward,
                Vector3.back,
                subject.HasActor ? 200UL : 0UL,
                subject.ActorId,
                1600f,
                default,
                subject);
        }
    }
}
