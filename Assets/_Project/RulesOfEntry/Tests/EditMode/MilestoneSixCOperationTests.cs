using NUnit.Framework;
using RulesOfEntry.Deployment;
using RulesOfEntry.UI.Operations;
using UnityEngine;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class MilestoneSixCOperationTests
    {
        [TestCase(0, 1, 2, 1)]
        [TestCase(1, 1, 2, 0)]
        [TestCase(0, -1, 2, 1)]
        [TestCase(2, 1, 4, 3)]
        public void BodyCameraFeedSelectionWraps(
            int current,
            int delta,
            int count,
            int expected)
        {
            Assert.That(
                OperationTabletRules.WrapFeedIndex(current, delta, count),
                Is.EqualTo(expected));
        }

        [Test]
        public void EmptyBodyCameraRosterHasNoSelection()
        {
            Assert.That(
                OperationTabletRules.WrapFeedIndex(0, 1, 0),
                Is.EqualTo(-1));
        }

        [TestCase(false, false, "SIGNAL UNAVAILABLE")]
        [TestCase(true, false, "CONNECTING")]
        [TestCase(true, true, "LIVE / ENCRYPTED")]
        public void BodyCameraSignalLabelIsExplicit(
            bool signal,
            bool streaming,
            string expected)
        {
            Assert.That(
                OperationTabletRules.GetSignalLabel(signal, streaming),
                Is.EqualTo(expected));
        }

        [Test]
        public void EntryAnchorRequiresStableIdAndAuthoredSpawns()
        {
            GameObject root = new GameObject("EntryAnchorTest");
            GameObject player = new GameObject("PlayerSpawn");
            GameObject officer = new GameObject("OfficerSpawn");
            player.transform.SetParent(root.transform);
            officer.transform.SetParent(root.transform);
            try
            {
                OperationEntryAnchor anchor =
                    root.AddComponent<OperationEntryAnchor>();
                anchor.Configure(
                    "entry_test",
                    "Test Entry",
                    player.transform,
                    new[] { officer.transform });

                Assert.That(anchor.HasValidConfiguration, Is.True);
                Assert.That(anchor.EntryPointId, Is.EqualTo("entry_test"));
                Assert.That(anchor.GetOfficerSpawn(0), Is.SameAs(officer.transform));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
