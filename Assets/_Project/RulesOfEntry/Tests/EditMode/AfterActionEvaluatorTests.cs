using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Combat;
using RulesOfEntry.Missions;
using RulesOfEntry.Officers;
using UnityEngine;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class AfterActionEvaluatorTests
    {
        private MissionDefinition definition;
        private RulesOfEngagementPolicy policy;

        [SetUp]
        public void SetUp()
        {
            MissionObjectiveDefinition objective = new MissionObjectiveDefinition();
            objective.Configure(
                "secure_subject",
                "Secure subject",
                string.Empty,
                MissionObjectiveType.SecureSubject,
                "subject",
                string.Empty,
                true,
                30);
            definition = ScriptableObject.CreateInstance<MissionDefinition>();
            definition.Configure(
                "test_mission",
                "Test Mission",
                string.Empty,
                new[] { objective });
            policy = ScriptableObject.CreateInstance<RulesOfEngagementPolicy>();
            policy.Configure("test_roe", "Test ROE", string.Empty, 5, 20, 45, 59);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(policy);
        }

        [Test]
        public void SuccessfulCustodyWithoutForce_ProducesExemplaryFinalReport()
        {
            MissionEvidenceSnapshot evidence = CreateEvidence(
                System.Array.Empty<ForceEventRecord>());

            AfterActionReport report = AfterActionEvaluator.Evaluate(
                definition,
                policy,
                evidence,
                true);

            Assert.That(report.Score, Is.EqualTo(100));
            Assert.That(report.Rating, Is.EqualTo(OperationalRating.Exemplary));
            Assert.That(report.Final, Is.True);
        }

        [Test]
        public void CriticalRoeViolation_CapsFinalRating()
        {
            ForceEventRecord civilianForce =
                RulesOfEngagementEvaluatorTests.CreateForceEvent(
                    new ForceSubjectSnapshot(
                        true,
                        "civilian",
                        ActorRole.Civilian,
                        ActorConditionLevel.Stable,
                        CustodyState.Free,
                        HumanBehaviorState.Idle,
                        false));
            MissionEvidenceSnapshot evidence = CreateEvidence(new[] { civilianForce });

            AfterActionReport report = AfterActionEvaluator.Evaluate(
                definition,
                policy,
                evidence,
                true);

            Assert.That(report.Score, Is.LessThanOrEqualTo(59));
            Assert.That(report.Rating, Is.EqualTo(OperationalRating.CriticalFailure));
        }

        [Test]
        public void ManualFinalization_ConvertsPendingRequiredObjectiveToFailure()
        {
            MissionEvidenceSnapshot evidence = new MissionEvidenceSnapshot(
                5d,
                4d,
                new[]
                {
                    new ActorEvidenceSnapshot(
                        "subject",
                        20UL,
                        ActorRole.Suspect,
                        ActorConditionLevel.Stable,
                        CustodyState.Free,
                        HumanBehaviorState.Idle,
                        false)
                },
                System.Array.Empty<RoomEvidenceSnapshot>(),
                System.Array.Empty<ForceEventRecord>(),
                System.Array.Empty<CustodyEventRecord>(),
                System.Array.Empty<OfficerOrderEventRecord>(),
                System.Array.Empty<OfficerInitiativeRecord>());

            AfterActionReport report = AfterActionEvaluator.Evaluate(
                definition,
                policy,
                evidence,
                true);

            Assert.That(report.Objectives[0].Status,
                Is.EqualTo(MissionObjectiveStatus.Failed));
            Assert.That(report.Score, Is.EqualTo(70));
            Assert.That(report.Rating, Is.EqualTo(OperationalRating.Deficient));
            StringAssert.Contains(
                "operation ended",
                report.Objectives[0].Rationale.ToLowerInvariant());
        }

        private static MissionEvidenceSnapshot CreateEvidence(ForceEventRecord[] forceEvents)
        {
            return new MissionEvidenceSnapshot(
                5d,
                4d,
                new[]
                {
                    new ActorEvidenceSnapshot(
                        "subject",
                        20UL,
                        ActorRole.Suspect,
                        ActorConditionLevel.Stable,
                        CustodyState.Restrained,
                        HumanBehaviorState.Restrained,
                        false)
                },
                System.Array.Empty<RoomEvidenceSnapshot>(),
                forceEvents,
                System.Array.Empty<CustodyEventRecord>(),
                System.Array.Empty<OfficerOrderEventRecord>(),
                System.Array.Empty<OfficerInitiativeRecord>());
        }
    }
}
