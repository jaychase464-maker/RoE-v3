using System;
using System.Collections.Generic;

namespace RulesOfEntry.Combat
{
    /// <summary>
    /// Authoritative mechanical state for one detachable-magazine, closed-bolt firearm.
    /// This type has no input or presentation dependency and never reloads automatically.
    /// </summary>
    public sealed class FirearmStateMachine
    {
        private readonly int magazineCapacity;
        private readonly List<MagazineState> spareMagazines = new List<MagazineState>();

        private MagazineState insertedMagazine;
        private bool chamberLoaded;
        private bool boltLocked;
        private int droppedMagazineCount;
        private int ejectedLiveRoundCount;

        public FirearmStateMachine(
            int configuredMagazineCapacity,
            int insertedMagazineRounds,
            bool initialChamberLoaded,
            IEnumerable<int> spareMagazineRounds,
            FireSelectorPosition initialSelector = FireSelectorPosition.Safe,
            bool initialBoltLocked = false)
        {
            if (configuredMagazineCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(configuredMagazineCapacity));
            }

            magazineCapacity = configuredMagazineCapacity;
            insertedMagazine = new MagazineState(
                "inserted-0",
                magazineCapacity,
                insertedMagazineRounds);
            chamberLoaded = initialChamberLoaded;
            Selector = initialSelector;

            int index = 0;
            if (spareMagazineRounds != null)
            {
                foreach (int rounds in spareMagazineRounds)
                {
                    spareMagazines.Add(new MagazineState(
                        $"spare-{index++}",
                        magazineCapacity,
                        rounds));
                }
            }

            boltLocked = !chamberLoaded && initialBoltLocked;
        }

        public FireSelectorPosition Selector { get; private set; }
        public bool AutomaticReloadEnabled => false;
        public int SpareMagazineCount => spareMagazines.Count;
        public bool HasInsertedMagazine => insertedMagazine != null;

        public FirearmSnapshot Snapshot => new FirearmSnapshot(
            Selector,
            chamberLoaded,
            boltLocked,
            insertedMagazine != null,
            insertedMagazine != null ? insertedMagazine.RoundCount : 0,
            spareMagazines.Count,
            droppedMagazineCount,
            ejectedLiveRoundCount);

        public FireSelectorPosition CycleSelector()
        {
            Selector = Selector == FireSelectorPosition.Safe
                ? FireSelectorPosition.SemiAutomatic
                : FireSelectorPosition.Safe;
            return Selector;
        }

        public FireAttemptResult TryFire()
        {
            if (Selector == FireSelectorPosition.Safe)
            {
                return new FireAttemptResult(false, FireFailureReason.SafetyOn, Snapshot);
            }

            if (boltLocked)
            {
                return new FireAttemptResult(false, FireFailureReason.BoltLocked, Snapshot);
            }

            if (!chamberLoaded)
            {
                return new FireAttemptResult(false, FireFailureReason.EmptyChamber, Snapshot);
            }

            chamberLoaded = false;
            if (insertedMagazine != null && insertedMagazine.TryTakeRound())
            {
                chamberLoaded = true;
                boltLocked = false;
            }
            else
            {
                // An empty inserted magazine engages the follower and locks the bolt.
                boltLocked = insertedMagazine != null;
            }

            return new FireAttemptResult(true, FireFailureReason.None, Snapshot);
        }

        public bool TryReload(bool emergency, out ReloadResult result)
        {
            if (spareMagazines.Count == 0)
            {
                result = new ReloadResult(
                    false,
                    emergency,
                    false,
                    false,
                    false,
                    Snapshot);
                return false;
            }

            MagazineState nextMagazine = spareMagazines[0];
            spareMagazines.RemoveAt(0);

            MagazineState removedMagazine = insertedMagazine;
            insertedMagazine = nextMagazine;
            bool retained = false;
            bool dropped = false;

            if (removedMagazine != null)
            {
                if (emergency)
                {
                    droppedMagazineCount++;
                    dropped = true;
                }
                else
                {
                    // The freed pouch is filled with the removed magazine. It returns at the
                    // back of the physical pouch order; it is not sorted by hidden round count.
                    spareMagazines.Add(removedMagazine);
                    retained = true;
                }
            }

            bool chamberedByBoltRelease = false;
            if (!chamberLoaded && boltLocked)
            {
                boltLocked = false;
                if (insertedMagazine.TryTakeRound())
                {
                    chamberLoaded = true;
                    chamberedByBoltRelease = true;
                }
                else
                {
                    boltLocked = true;
                }
            }

            result = new ReloadResult(
                true,
                emergency,
                retained,
                dropped,
                chamberedByBoltRelease,
                Snapshot);
            return true;
        }

        public CycleActionResult CycleAction()
        {
            bool ejectedLiveRound = chamberLoaded;
            if (ejectedLiveRound)
            {
                ejectedLiveRoundCount++;
                chamberLoaded = false;
            }

            bool chamberedRound = insertedMagazine != null && insertedMagazine.TryTakeRound();
            chamberLoaded = chamberedRound;
            boltLocked = !chamberedRound && insertedMagazine != null;

            return new CycleActionResult(ejectedLiveRound, chamberedRound, Snapshot);
        }

        public MagazineEstimate CheckInsertedMagazine()
        {
            if (insertedMagazine == null)
            {
                return MagazineEstimate.NoMagazine;
            }

            int rounds = insertedMagazine.RoundCount;
            if (rounds == 0)
            {
                return MagazineEstimate.Empty;
            }

            if (rounds <= 3)
            {
                return MagazineEstimate.NearlyEmpty;
            }

            int lowThreshold = Math.Max(5, (int)Math.Ceiling(magazineCapacity * 0.25f));
            if (rounds <= lowThreshold)
            {
                return MagazineEstimate.Low;
            }

            if (rounds <= (int)Math.Floor(magazineCapacity * 0.6f))
            {
                return MagazineEstimate.Partial;
            }

            int fullThreshold = (int)Math.Ceiling(magazineCapacity * 0.9f);
            if (rounds >= fullThreshold)
            {
                return MagazineEstimate.Full;
            }

            return MagazineEstimate.MostlyFull;
        }
    }
}
