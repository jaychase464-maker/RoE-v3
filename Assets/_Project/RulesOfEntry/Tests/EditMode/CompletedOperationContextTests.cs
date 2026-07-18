using System;
using NUnit.Framework;
using RulesOfEntry.Missions;
using RulesOfEntry.Operations;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class CompletedOperationContextTests
    {
        [SetUp]
        public void SetUp()
        {
            CompletedOperationContext.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            CompletedOperationContext.Clear();
        }

        [Test]
        public void Capture_FinalReport_PreservesReportAndStableDeploymentIds()
        {
            AfterActionReport report = CreateReport(true);

            bool captured = CompletedOperationContext.Capture(
                report,
                " OP-06-1 ",
                " entry_south ",
                new[] { "alpha", "bravo" },
                new[] { "shield" });

            Assert.That(captured, Is.True);
            Assert.That(CompletedOperationContext.TryGetLatest(out CompletedOperationRecord record), Is.True);
            Assert.That(record.Report, Is.SameAs(report));
            Assert.That(record.OperationCode, Is.EqualTo("OP-06-1"));
            Assert.That(record.EntryPointId, Is.EqualTo("entry_south"));
            CollectionAssert.AreEqual(new[] { "alpha", "bravo" }, record.AssignedOfficerIds);
            CollectionAssert.AreEqual(new[] { "shield" }, record.SupportAssetIds);
            Assert.That(record.SessionSequence, Is.GreaterThan(0));
        }

        [Test]
        public void Capture_ProvisionalReport_IsRejectedWithoutReplacingLatest()
        {
            AfterActionReport finalReport = CreateReport(true);
            Assert.That(CompletedOperationContext.Capture(
                finalReport,
                "OP-06-1",
                "entry_south",
                Array.Empty<string>(),
                Array.Empty<string>()), Is.True);
            CompletedOperationRecord original = CompletedOperationContext.Latest;

            bool captured = CompletedOperationContext.Capture(
                CreateReport(false),
                "OP-INVALID",
                "entry_invalid",
                Array.Empty<string>(),
                Array.Empty<string>());

            Assert.That(captured, Is.False);
            Assert.That(CompletedOperationContext.Latest, Is.SameAs(original));
        }

        [Test]
        public void Record_NormalizesIdsAndFallsBackToMissionIdForDirectLaunch()
        {
            CompletedOperationRecord record = new CompletedOperationRecord(
                0,
                CreateReport(true),
                " ",
                null,
                new[] { "alpha", "alpha", " ", null, "bravo" },
                new[] { "drone", "drone" });

            Assert.That(record.SessionSequence, Is.EqualTo(1));
            Assert.That(record.OperationCode, Is.EqualTo("pressure-point"));
            Assert.That(record.EntryPointId, Is.Empty);
            CollectionAssert.AreEqual(new[] { "alpha", "bravo" }, record.AssignedOfficerIds);
            CollectionAssert.AreEqual(new[] { "drone" }, record.SupportAssetIds);
        }

        [Test]
        public void Record_ProvisionalReport_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CompletedOperationRecord(
                1,
                CreateReport(false),
                "OP-06-1",
                "entry_south",
                Array.Empty<string>(),
                Array.Empty<string>()));
        }

        [Test]
        public void Clear_RemovesLatestRecord()
        {
            Assert.That(CompletedOperationContext.Capture(
                CreateReport(true),
                "OP-06-1",
                "entry_south",
                Array.Empty<string>(),
                Array.Empty<string>()), Is.True);

            CompletedOperationContext.Clear();

            Assert.That(CompletedOperationContext.HasCompletedOperation, Is.False);
            Assert.That(CompletedOperationContext.TryGetLatest(out _), Is.False);
        }

        private static AfterActionReport CreateReport(bool isFinal)
        {
            return new AfterActionReport(
                "pressure-point",
                "Pressure Point",
                125d,
                120d,
                isFinal,
                92,
                OperationalRating.Exemplary,
                new[]
                {
                    new MissionObjectiveEvaluation(
                        "secure-suspect",
                        "Secure the primary suspect",
                        MissionObjectiveType.SecureSubject,
                        MissionObjectiveStatus.Completed,
                        true,
                        30,
                        "Subject is in custody.")
                },
                Array.Empty<RoeFinding>(),
                "Operation resolved.");
        }
    }
}
