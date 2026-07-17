using NUnit.Framework;
using RulesOfEntry.Combat;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class FirearmStateMachineTests
    {
        [Test]
        public void Safety_BlocksDischargeWithoutConsumingAmmunition()
        {
            FirearmStateMachine firearm = new FirearmStateMachine(
                30,
                29,
                true,
                new[] { 30 },
                FireSelectorPosition.Safe);

            FireAttemptResult result = firearm.TryFire();

            Assert.That(result.Discharged, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(FireFailureReason.SafetyOn));
            Assert.That(result.Snapshot.ChamberLoaded, Is.True);
            Assert.That(result.Snapshot.InsertedMagazineRounds, Is.EqualTo(29));
        }

        [Test]
        public void EmptyWeapon_NeverReloadsAutomatically()
        {
            FirearmStateMachine firearm = new FirearmStateMachine(
                30,
                1,
                true,
                new[] { 30 },
                FireSelectorPosition.SemiAutomatic);

            Assert.That(firearm.TryFire().Discharged, Is.True);
            Assert.That(firearm.TryFire().Discharged, Is.True);
            FireAttemptResult emptyAttempt = firearm.TryFire();

            Assert.That(emptyAttempt.Discharged, Is.False);
            Assert.That(emptyAttempt.FailureReason, Is.EqualTo(FireFailureReason.BoltLocked));
            Assert.That(emptyAttempt.Snapshot.SpareMagazineCount, Is.EqualTo(1));
            Assert.That(emptyAttempt.Snapshot.ChamberLoaded, Is.False);
            Assert.That(firearm.AutomaticReloadEnabled, Is.False);
        }

        [Test]
        public void MagazineCheck_ReturnsEstimateInsteadOfExactCount()
        {
            FirearmStateMachine firearm = new FirearmStateMachine(
                30,
                3,
                true,
                new[] { 30 });

            Assert.That(
                firearm.CheckInsertedMagazine(),
                Is.EqualTo(MagazineEstimate.NearlyEmpty));
        }

        [Test]
        public void MagazineMissingOneRound_StillFeelsFull()
        {
            FirearmStateMachine firearm = new FirearmStateMachine(
                30,
                29,
                true,
                new[] { 30 });

            Assert.That(firearm.CheckInsertedMagazine(), Is.EqualTo(MagazineEstimate.Full));
        }

        [Test]
        public void RetainedReload_PreservesPartialMagazineInPouchOrder()
        {
            FirearmStateMachine firearm = new FirearmStateMachine(
                30,
                10,
                true,
                new[] { 30, 20 });

            Assert.That(firearm.TryReload(false, out ReloadResult firstReload), Is.True);
            Assert.That(firstReload.RetainedRemovedMagazine, Is.True);
            Assert.That(firstReload.Snapshot.InsertedMagazineRounds, Is.EqualTo(30));

            Assert.That(firearm.TryReload(false, out ReloadResult secondReload), Is.True);
            Assert.That(
                secondReload.Snapshot.InsertedMagazineRounds,
                Is.EqualTo(20),
                "Reload selection must follow pouch order, not hidden round count.");
        }

        [Test]
        public void EmergencyReload_DropsRemovedMagazine()
        {
            FirearmStateMachine firearm = new FirearmStateMachine(
                30,
                5,
                true,
                new[] { 30 });

            Assert.That(firearm.TryReload(true, out ReloadResult result), Is.True);
            Assert.That(result.DroppedRemovedMagazine, Is.True);
            Assert.That(result.RetainedRemovedMagazine, Is.False);
            Assert.That(result.Snapshot.DroppedMagazineCount, Is.EqualTo(1));
            Assert.That(result.Snapshot.SpareMagazineCount, Is.Zero);
        }

        [Test]
        public void BoltLockedReload_ReleasesBoltAndChambersRound()
        {
            FirearmStateMachine firearm = new FirearmStateMachine(
                30,
                0,
                false,
                new[] { 30 },
                FireSelectorPosition.SemiAutomatic,
                true);

            Assert.That(firearm.TryReload(true, out ReloadResult result), Is.True);
            Assert.That(result.ChamberedByBoltRelease, Is.True);
            Assert.That(result.Snapshot.ChamberLoaded, Is.True);
            Assert.That(result.Snapshot.BoltLocked, Is.False);
            Assert.That(result.Snapshot.InsertedMagazineRounds, Is.EqualTo(29));
        }

        [Test]
        public void EmptyClosedChamber_RequiresManualActionCycle()
        {
            FirearmStateMachine firearm = new FirearmStateMachine(
                30,
                30,
                false,
                new[] { 30 },
                FireSelectorPosition.SemiAutomatic);

            FireAttemptResult emptyAttempt = firearm.TryFire();
            Assert.That(emptyAttempt.FailureReason, Is.EqualTo(FireFailureReason.EmptyChamber));

            CycleActionResult cycleResult = firearm.CycleAction();
            Assert.That(cycleResult.ChamberedRound, Is.True);
            Assert.That(cycleResult.Snapshot.ChamberLoaded, Is.True);
            Assert.That(cycleResult.Snapshot.InsertedMagazineRounds, Is.EqualTo(29));
        }

        [Test]
        public void CyclingLoadedAction_EjectsOneLiveRoundAndFeedsNext()
        {
            FirearmStateMachine firearm = new FirearmStateMachine(
                30,
                29,
                true,
                new[] { 30 });

            CycleActionResult result = firearm.CycleAction();

            Assert.That(result.EjectedLiveRound, Is.True);
            Assert.That(result.ChamberedRound, Is.True);
            Assert.That(result.Snapshot.EjectedLiveRoundCount, Is.EqualTo(1));
            Assert.That(result.Snapshot.InsertedMagazineRounds, Is.EqualTo(28));
        }
    }
}
