using System;

namespace RulesOfEntry.Combat
{
    /// <summary>
    /// One physical detachable magazine. Rounds are never pooled globally.
    /// </summary>
    public sealed class MagazineState
    {
        public MagazineState(string magazineId, int capacity, int roundCount)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            MagazineId = string.IsNullOrWhiteSpace(magazineId)
                ? throw new ArgumentException("A magazine ID is required.", nameof(magazineId))
                : magazineId.Trim();
            Capacity = capacity;
            RoundCount = Math.Max(0, Math.Min(roundCount, capacity));
        }

        public string MagazineId { get; }
        public int Capacity { get; }
        public int RoundCount { get; private set; }

        public bool TryTakeRound()
        {
            if (RoundCount <= 0)
            {
                return false;
            }

            RoundCount--;
            return true;
        }
    }
}
