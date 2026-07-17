using NUnit.Framework;
using RulesOfEntry.Officers;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class OfficerChallengeRulesTests
    {
        [Test]
        public void ScheduledFollowUp_BecomesReadyAtCadenceBoundary()
        {
            Assert.That(
                OfficerChallengeRules.IsCooldownComplete(7.99f, true, 8f),
                Is.False);
            Assert.That(
                OfficerChallengeRules.IsCooldownComplete(8f, true, 8f),
                Is.True);
        }

        [Test]
        public void FocusPersistsOnlyWhileSubjectRemainsEligibleAndMemoryIsValid()
        {
            Assert.That(OfficerChallengeRules.MayRetainFocus(true, 10f, 20f), Is.True);
            Assert.That(OfficerChallengeRules.MayRetainFocus(false, 10f, 20f), Is.False);
            Assert.That(OfficerChallengeRules.MayRetainFocus(true, 20.01f, 20f), Is.False);
        }
    }
}
