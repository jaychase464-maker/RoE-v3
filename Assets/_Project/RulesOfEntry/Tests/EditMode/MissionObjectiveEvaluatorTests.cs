using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Missions;
using RulesOfEntry.Officers;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class MissionObjectiveEvaluatorTests
    {
        [TestCase(
            CustodyState.Free,
            ActorConditionLevel.Stable,
            MissionObjectiveStatus.Pending)]
        [TestCase(
            CustodyState.Restrained,
            ActorConditionLevel.Stable,
            MissionObjectiveStatus.Completed)]
        [TestCase(
            CustodyState.Free,
            ActorConditionLevel.Deceased,
            MissionObjectiveStatus.Failed)]
        public void SecureSubject_UsesCustodyAndConditionEvidence(
            CustodyState custody,
            ActorConditionLevel condition,
            MissionObjectiveStatus expected)
        {
            MissionObjectiveDefinition objective = new MissionObjectiveDefinition();
            objective.Configure(
                "secure_test_subject",
                "Secure test subject",
                string.Empty,
                MissionObjectiveType.SecureSubject,
                "test_subject",
                string.Empty,
                true,
                30);
            MissionEvidenceSnapshot evidence = CreateEvidence(
                new ActorEvidenceSnapshot(
                    "test_subject",
                    11UL,
                    ActorRole.Suspect,
                    condition,
                    custody,
                    HumanBehaviorState.Idle,
                    true));

            MissionObjectiveEvaluation result =
                MissionObjectiveEvaluator.Evaluate(objective, evidence);

            Assert.That(result.Status, Is.EqualTo(expected));
        }

        [Test]
        public void RoomClear_RequiresVerifiedStateAndNoActiveThreat()
        {
            MissionObjectiveDefinition objective = new MissionObjectiveDefinition();
            objective.Configure(
                "clear_test_room",
                "Clear test room",
                string.Empty,
                MissionObjectiveType.VerifyRoomClear,
                string.Empty,
                "test_room",
                true,
                25);
            MissionEvidenceSnapshot evidence = new MissionEvidenceSnapshot(
                1d,
                1d,
                System.Array.Empty<ActorEvidenceSnapshot>(),
                new[]
                {
                    new RoomEvidenceSnapshot(
                        "test_room",
                        TacticalRoomClearanceState.Clear,
                        0,
                        2)
                },
                System.Array.Empty<RulesOfEntry.Combat.ForceEventRecord>(),
                System.Array.Empty<CustodyEventRecord>(),
                System.Array.Empty<OfficerOrderEventRecord>(),
                System.Array.Empty<OfficerInitiativeRecord>());

            MissionObjectiveEvaluation result =
                MissionObjectiveEvaluator.Evaluate(objective, evidence);

            Assert.That(result.Status, Is.EqualTo(MissionObjectiveStatus.Completed));
        }

        private static MissionEvidenceSnapshot CreateEvidence(
            params ActorEvidenceSnapshot[] actors)
        {
            return new MissionEvidenceSnapshot(
                1d,
                1d,
                actors,
                System.Array.Empty<RoomEvidenceSnapshot>(),
                System.Array.Empty<RulesOfEntry.Combat.ForceEventRecord>(),
                System.Array.Empty<CustodyEventRecord>(),
                System.Array.Empty<OfficerOrderEventRecord>(),
                System.Array.Empty<OfficerInitiativeRecord>());
        }
    }
}
