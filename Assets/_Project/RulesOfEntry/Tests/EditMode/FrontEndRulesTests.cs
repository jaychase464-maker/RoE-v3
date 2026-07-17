using NUnit.Framework;
using RulesOfEntry.UI.FrontEnd;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class FrontEndRulesTests
    {
        [TestCase(0, 4, 1)]
        [TestCase(2, 4, 3)]
        [TestCase(3, 4, 0)]
        [TestCase(99, 4, 0)]
        [TestCase(2, 0, 0)]
        public void GetNextQualityIndex_AdvancesAndWraps(
            int current,
            int count,
            int expected)
        {
            Assert.That(
                FrontEndRules.GetNextQualityIndex(current, count),
                Is.EqualTo(expected));
        }

        [TestCase(-1f, 0f)]
        [TestCase(0f, 0f)]
        [TestCase(0.45f, 0.5f)]
        [TestCase(0.9f, 1f)]
        [TestCase(1f, 1f)]
        public void NormalizeLoadingProgress_MapsUnityLoadRange(
            float sceneProgress,
            float expected)
        {
            Assert.That(
                FrontEndRules.NormalizeLoadingProgress(sceneProgress),
                Is.EqualTo(expected).Within(0.0001f));
        }

        [TestCase(false, false, false, false)]
        [TestCase(true, false, false, true)]
        [TestCase(false, true, false, true)]
        [TestCase(false, false, true, true)]
        public void WarningContinue_RequiresAnAdvertisedControl(
            bool enter,
            bool numpadEnter,
            bool gamepadSouth,
            bool expected)
        {
            Assert.That(
                FrontEndRules.IsWarningContinueRequested(
                    enter,
                    numpadEnter,
                    gamepadSouth),
                Is.EqualTo(expected));
        }
    }
}
