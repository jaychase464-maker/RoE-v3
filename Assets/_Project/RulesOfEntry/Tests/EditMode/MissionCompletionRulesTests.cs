using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Missions;
using RulesOfEntry.Officers;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class MissionCompletionRulesTests
    {
        [Test]
        public void RequiredObjectivesAndEveryRoomClear_AreReadyForAutomaticCompletion()
        {
            AfterActionReport report = CreateReport(MissionObjectiveStatus.Completed);
            MissionEvidenceSnapshot evidence = CreateEvidence(
                new RoomEvidenceSnapshot(
                    "room_a",
                    TacticalRoomClearanceState.Clear,
                    0,
                    2),
                new RoomEvidenceSnapshot(
                    "room_b",
                    TacticalRoomClearanceState.Clear,
                    0,
                    1));

            MissionCompletionDecision decision = MissionCompletionRules.Evaluate(
                report,
                evidence);

            Assert.That(decision.Ready, Is.True);
            StringAssert.Contains("verified clear", decision.Reason);
        }

        [Test]
        public void UnclearAuthoredRoom_BlocksAutomaticCompletion()
        {
            AfterActionReport report = CreateReport(MissionObjectiveStatus.Completed);
            MissionEvidenceSnapshot evidence = CreateEvidence(
                new RoomEvidenceSnapshot(
                    "room_a",
                    TacticalRoomClearanceState.Clear,
                    0,
                    2),
                new RoomEvidenceSnapshot(
                    "room_b",
                    TacticalRoomClearanceState.Verifying,
                    1,
                    1));

            MissionCompletionDecision decision = MissionCompletionRules.Evaluate(
                report,
                evidence);

            Assert.That(decision.Ready, Is.False);
            StringAssert.Contains("room_b", decision.Reason);
        }

        [Test]
        public void PendingRequiredObjective_BlocksAutomaticCompletion()
        {
            MissionCompletionDecision decision = MissionCompletionRules.Evaluate(
                CreateReport(MissionObjectiveStatus.Pending),
                CreateEvidence());

            Assert.That(decision.Ready, Is.False);
            StringAssert.Contains("pending", decision.Reason.ToLowerInvariant());
        }

        private static AfterActionReport CreateReport(MissionObjectiveStatus status)
        {
            return new AfterActionReport(
                "test",
                "Test",
                1d,
                1d,
                false,
                100,
                OperationalRating.NotRated,
                new[]
                {
                    new MissionObjectiveEvaluation(
                        "required",
                        "Required objective",
                        MissionObjectiveType.SecureSubject,
                        status,
                        true,
                        30,
                        string.Empty)
                },
                System.Array.Empty<RoeFinding>(),
                string.Empty);
        }

        private static MissionEvidenceSnapshot CreateEvidence(
            params RoomEvidenceSnapshot[] rooms)
        {
            return new MissionEvidenceSnapshot(
                1d,
                1d,
                System.Array.Empty<ActorEvidenceSnapshot>(),
                rooms,
                System.Array.Empty<RulesOfEntry.Combat.ForceEventRecord>(),
                System.Array.Empty<CustodyEventRecord>(),
                System.Array.Empty<OfficerOrderEventRecord>(),
                System.Array.Empty<OfficerInitiativeRecord>());
        }
    }
}
