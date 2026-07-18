using System.Linq;
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
            Assert.That(report.Score, Is.EqualTo(55));
            Assert.That(report.Tier, Is.EqualTo(MissionPerformanceTier.F));
            Assert.That(report.Rating, Is.EqualTo(OperationalRating.CriticalFailure));
            StringAssert.Contains(
                "operation ended",
                report.Objectives[0].Rationale.ToLowerInvariant());
        }

        [Test]
        public void FinalReport_ScoresArrestsEvidenceAndTimeFromFacts()
        {
            MissionEvidenceSnapshot evidence = new MissionEvidenceSnapshot(
                25d,
                20d,
                new[]
                {
                    new ActorEvidenceSnapshot(
                        "subject",
                        20UL,
                        ActorRole.Suspect,
                        ActorConditionLevel.Stable,
                        CustodyState.Searched,
                        HumanBehaviorState.Restrained,
                        false,
                        true,
                        true,
                        true,
                        2),
                    new ActorEvidenceSnapshot(
                        "civilian",
                        30UL,
                        ActorRole.Civilian,
                        ActorConditionLevel.Stable,
                        CustodyState.Free,
                        HumanBehaviorState.Idle,
                        false),
                    new ActorEvidenceSnapshot(
                        "officer",
                        40UL,
                        ActorRole.Officer,
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

            Assert.That(report.Score, Is.EqualTo(100));
            Assert.That(report.Tier, Is.EqualTo(MissionPerformanceTier.S));
            Assert.That(report.Metrics.SuspectsArrested, Is.EqualTo(1));
            Assert.That(report.Metrics.CiviliansSaved, Is.EqualTo(1));
            Assert.That(report.Metrics.EvidenceItemsSecured, Is.EqualTo(3));
            Assert.That(report.Categories.Count, Is.EqualTo(7));
            Assert.That(
                report.Categories.Sum(category => category.MaximumScore),
                Is.EqualTo(100));
        }

        [Test]
        public void CivilianDeath_CapsFinalTierAtF()
        {
            MissionEvidenceSnapshot evidence = new MissionEvidenceSnapshot(
                25d,
                20d,
                new[]
                {
                    new ActorEvidenceSnapshot(
                        "subject",
                        20UL,
                        ActorRole.Suspect,
                        ActorConditionLevel.Stable,
                        CustodyState.Restrained,
                        HumanBehaviorState.Restrained,
                        false),
                    new ActorEvidenceSnapshot(
                        "civilian",
                        30UL,
                        ActorRole.Civilian,
                        ActorConditionLevel.Deceased,
                        CustodyState.Free,
                        HumanBehaviorState.Incapacitated,
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

            Assert.That(report.Score, Is.LessThanOrEqualTo(59));
            Assert.That(report.ScoreCap, Is.EqualTo(59));
            Assert.That(report.Tier, Is.EqualTo(MissionPerformanceTier.F));
            Assert.That(report.Metrics.CiviliansKilled, Is.EqualTo(1));
        }

        [Test]
        public void OfficerDeath_CapsFinalTierAtD()
        {
            MissionEvidenceSnapshot evidence = new MissionEvidenceSnapshot(
                25d,
                20d,
                new[]
                {
                    new ActorEvidenceSnapshot(
                        "subject",
                        20UL,
                        ActorRole.Suspect,
                        ActorConditionLevel.Stable,
                        CustodyState.Restrained,
                        HumanBehaviorState.Restrained,
                        false),
                    new ActorEvidenceSnapshot(
                        "officer",
                        40UL,
                        ActorRole.Officer,
                        ActorConditionLevel.Deceased,
                        CustodyState.Free,
                        HumanBehaviorState.Incapacitated,
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

            Assert.That(report.Score, Is.EqualTo(74));
            Assert.That(report.Tier, Is.EqualTo(MissionPerformanceTier.D));
            Assert.That(report.Metrics.OfficersKilled, Is.EqualTo(1));
        }

        [Test]
        public void UnsecuredEvidence_ReducesEvidenceCategory()
        {
            MissionEvidenceSnapshot evidence = new MissionEvidenceSnapshot(
                25d,
                20d,
                new[]
                {
                    new ActorEvidenceSnapshot(
                        "subject",
                        20UL,
                        ActorRole.Suspect,
                        ActorConditionLevel.Stable,
                        CustodyState.Restrained,
                        HumanBehaviorState.Restrained,
                        true,
                        true,
                        false,
                        false,
                        2)
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
            MissionScoreCategory evidenceCategory = report.Categories
                .Single(category => category.Type == MissionScoreCategoryType.Evidence);

            Assert.That(evidenceCategory.EarnedScore, Is.EqualTo(0));
            Assert.That(report.Score, Is.EqualTo(90));
            Assert.That(report.Tier, Is.EqualTo(MissionPerformanceTier.A));
        }

        [Test]
        public void OperationBeyondMaximumScoredTime_LosesTimeCategory()
        {
            MissionEvidenceSnapshot evidence = new MissionEvidenceSnapshot(
                1300d,
                1300d,
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
                System.Array.Empty<ForceEventRecord>(),
                System.Array.Empty<CustodyEventRecord>(),
                System.Array.Empty<OfficerOrderEventRecord>(),
                System.Array.Empty<OfficerInitiativeRecord>());

            AfterActionReport report = AfterActionEvaluator.Evaluate(
                definition,
                policy,
                evidence,
                true);
            MissionScoreCategory timeCategory = report.Categories
                .Single(category => category.Type == MissionScoreCategoryType.Time);

            Assert.That(timeCategory.EarnedScore, Is.EqualTo(0));
            Assert.That(report.Score, Is.EqualTo(95));
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
