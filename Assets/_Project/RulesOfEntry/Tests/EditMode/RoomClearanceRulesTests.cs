using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Officers;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class RoomClearanceRulesTests
    {
        [Test]
        public void FreeCapableSuspect_BlocksClearanceAndCannotBeAutomaticallyRestrained()
        {
            Assert.That(
                RoomClearanceRules.IsActiveThreat(
                    ActorRole.Suspect,
                    CustodyState.Free,
                    ActorConditionLevel.Stable),
                Is.True);
            Assert.That(
                RoomClearanceRules.IsEligibleForAutomaticCustody(
                    ActorRole.Suspect,
                    CustodyState.Free,
                    ActorConditionLevel.Stable),
                Is.False);
        }

        [TestCase(CustodyState.Surrendering)]
        [TestCase(CustodyState.Kneeling)]
        public void ControlledSuspect_AllowsClearanceAndIsEligibleForCustody(
            CustodyState custodyState)
        {
            Assert.That(
                RoomClearanceRules.IsActiveThreat(
                    ActorRole.Suspect,
                    custodyState,
                    ActorConditionLevel.Stable),
                Is.False);
            Assert.That(
                RoomClearanceRules.IsEligibleForAutomaticCustody(
                    ActorRole.Suspect,
                    custodyState,
                    ActorConditionLevel.Stable),
                Is.True);
        }

        [Test]
        public void IncapacitatedSuspect_IsEligibleButDeceasedSuspectIsNot()
        {
            Assert.That(
                RoomClearanceRules.IsEligibleForAutomaticCustody(
                    ActorRole.Suspect,
                    CustodyState.Free,
                    ActorConditionLevel.Incapacitated),
                Is.True);
            Assert.That(
                RoomClearanceRules.IsEligibleForAutomaticCustody(
                    ActorRole.Suspect,
                    CustodyState.Free,
                    ActorConditionLevel.Deceased),
                Is.False);
        }

        [Test]
        public void Civilian_NeverBecomesAnAutomaticCustodyTarget()
        {
            Assert.That(
                RoomClearanceRules.IsActiveThreat(
                    ActorRole.Civilian,
                    CustodyState.Free,
                    ActorConditionLevel.Stable),
                Is.False);
            Assert.That(
                RoomClearanceRules.IsEligibleForAutomaticCustody(
                    ActorRole.Civilian,
                    CustodyState.Surrendering,
                    ActorConditionLevel.Stable),
                Is.False);
        }
    }
}
