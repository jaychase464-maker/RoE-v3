namespace RulesOfEntry.Combat
{
    public enum FireSelectorPosition
    {
        Safe = 0,
        SemiAutomatic = 1
    }

    public enum WeaponReadyPosition
    {
        LowReady = 0,
        Shouldered = 1
    }

    public enum FirearmOperation
    {
        Idle = 0,
        CheckingMagazine = 1,
        Reloading = 2,
        CyclingAction = 3
    }

    public enum FireFailureReason
    {
        None = 0,
        SafetyOn = 1,
        EmptyChamber = 2,
        BoltLocked = 3
    }

    public enum MagazineEstimate
    {
        NoMagazine = 0,
        Empty = 1,
        NearlyEmpty = 2,
        Low = 3,
        Partial = 4,
        MostlyFull = 5,
        Full = 6
    }

    public readonly struct FirearmSnapshot
    {
        public FirearmSnapshot(
            FireSelectorPosition selector,
            bool chamberLoaded,
            bool boltLocked,
            bool hasInsertedMagazine,
            int insertedMagazineRounds,
            int spareMagazineCount,
            int droppedMagazineCount,
            int ejectedLiveRoundCount)
        {
            Selector = selector;
            ChamberLoaded = chamberLoaded;
            BoltLocked = boltLocked;
            HasInsertedMagazine = hasInsertedMagazine;
            InsertedMagazineRounds = insertedMagazineRounds;
            SpareMagazineCount = spareMagazineCount;
            DroppedMagazineCount = droppedMagazineCount;
            EjectedLiveRoundCount = ejectedLiveRoundCount;
        }

        public FireSelectorPosition Selector { get; }
        public bool ChamberLoaded { get; }
        public bool BoltLocked { get; }
        public bool HasInsertedMagazine { get; }

        // Authoritative simulation data. Never bind this value to player-facing UI.
        public int InsertedMagazineRounds { get; }
        public int SpareMagazineCount { get; }
        public int DroppedMagazineCount { get; }
        public int EjectedLiveRoundCount { get; }
    }

    public readonly struct FireAttemptResult
    {
        public FireAttemptResult(
            bool discharged,
            FireFailureReason failureReason,
            FirearmSnapshot snapshot)
        {
            Discharged = discharged;
            FailureReason = failureReason;
            Snapshot = snapshot;
        }

        public bool Discharged { get; }
        public FireFailureReason FailureReason { get; }
        public FirearmSnapshot Snapshot { get; }
    }

    public readonly struct ReloadResult
    {
        public ReloadResult(
            bool completed,
            bool emergency,
            bool retainedRemovedMagazine,
            bool droppedRemovedMagazine,
            bool chamberedByBoltRelease,
            FirearmSnapshot snapshot)
        {
            Completed = completed;
            Emergency = emergency;
            RetainedRemovedMagazine = retainedRemovedMagazine;
            DroppedRemovedMagazine = droppedRemovedMagazine;
            ChamberedByBoltRelease = chamberedByBoltRelease;
            Snapshot = snapshot;
        }

        public bool Completed { get; }
        public bool Emergency { get; }
        public bool RetainedRemovedMagazine { get; }
        public bool DroppedRemovedMagazine { get; }
        public bool ChamberedByBoltRelease { get; }
        public FirearmSnapshot Snapshot { get; }
    }

    public readonly struct CycleActionResult
    {
        public CycleActionResult(
            bool ejectedLiveRound,
            bool chamberedRound,
            FirearmSnapshot snapshot)
        {
            EjectedLiveRound = ejectedLiveRound;
            ChamberedRound = chamberedRound;
            Snapshot = snapshot;
        }

        public bool EjectedLiveRound { get; }
        public bool ChamberedRound { get; }
        public FirearmSnapshot Snapshot { get; }
    }
}
