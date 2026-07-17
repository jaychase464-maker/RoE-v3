namespace RulesOfEntry.Officers
{
    /// <summary>
    /// Pure timing rules for a coordinated officer challenge sequence.
    /// </summary>
    public static class OfficerChallengeRules
    {
        public static bool IsCooldownComplete(
            float currentTime,
            bool hasScheduledChallenge,
            float nextChallengeTime)
        {
            return !hasScheduledChallenge || currentTime >= nextChallengeTime;
        }

        public static bool MayRetainFocus(
            bool subjectRemainsEligible,
            float currentTime,
            float focusExpiresAt)
        {
            return subjectRemainsEligible && currentTime <= focusExpiresAt;
        }
    }
}
