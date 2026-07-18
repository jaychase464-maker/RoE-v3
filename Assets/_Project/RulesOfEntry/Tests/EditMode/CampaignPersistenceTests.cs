using System;
using NUnit.Framework;
using RulesOfEntry.Campaign;
using RulesOfEntry.Missions;
using RulesOfEntry.Operations;
using UnityEngine;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class CampaignPersistenceTests
    {
        [Test]
        public void IdentityRules_NormalizeRealisticOfficerIdentity()
        {
            bool valid = CampaignDataRules.TryValidateNewCampaign(
                "  Alex   O'Connor  ",
                " a-127 ",
                out string officer,
                out string badge,
                out string error);

            Assert.That(valid, Is.True, error);
            Assert.That(officer, Is.EqualTo("Alex O'Connor"));
            Assert.That(badge, Is.EqualTo("A-127"));
        }

        [TestCase("", "A127")]
        [TestCase("A", "A127")]
        [TestCase("Alex Carter", "")]
        [TestCase("Alex Carter", "!")]
        public void IdentityRules_RejectIncompleteIdentity(string officer, string badge)
        {
            Assert.That(CampaignDataRules.TryValidateNewCampaign(
                officer,
                badge,
                out _,
                out _,
                out _), Is.False);
        }

        [TestCase(-1, 3, 2)]
        [TestCase(3, 3, 0)]
        [TestCase(0, 0, -1)]
        public void ArchiveIndex_Wraps(int requested, int count, int expected)
        {
            Assert.That(
                CampaignDataRules.WrapArchiveIndex(requested, count),
                Is.EqualTo(expected));
        }

        [Test]
        public void SaveCodec_RoundTripPreservesCampaignAndReportFacts()
        {
            CampaignSaveData source = CreateCampaignWithOperation();

            string json = CampaignSaveCodec.Serialize(source, true);
            bool loaded = CampaignSaveCodec.TryDeserialize(
                json,
                out CampaignSaveData restored,
                out string error);

            Assert.That(loaded, Is.True, error);
            Assert.That(restored.schemaVersion, Is.EqualTo(1));
            Assert.That(restored.officerDisplayName, Is.EqualTo("Alex Carter"));
            Assert.That(restored.badgeIdentifier, Is.EqualTo("A127"));
            Assert.That(restored.CompletedOperationCount, Is.EqualTo(1));
            CompletedOperationRecord operation =
                restored.completedOperations[0].ToCompletedOperationRecord();
            Assert.That(operation.RecordId, Is.EqualTo(source.completedOperations[0].recordId));
            Assert.That(operation.Report.Score, Is.EqualTo(88));
            Assert.That(operation.Report.Tier, Is.EqualTo(MissionPerformanceTier.B));
            Assert.That(operation.Report.Metrics.CiviliansSaved, Is.EqualTo(1));
            Assert.That(operation.Report.Objectives[0].Status,
                Is.EqualTo(MissionObjectiveStatus.Completed));
            Assert.That(operation.Report.RoeFindings[0].ShooterEntityId,
                Is.EqualTo(ulong.MaxValue));
            Assert.That(operation.Report.RoeFindings[0].ForceEventSequence,
                Is.EqualTo(long.MaxValue));
        }

        [Test]
        public void SaveCodec_RejectsNewerSchema()
        {
            CampaignSaveData source = CreateCampaignWithOperation();
            string json = JsonUtility.ToJson(source, false)
                .Replace("\"schemaVersion\":1", "\"schemaVersion\":99");

            Assert.That(CampaignSaveCodec.TryDeserialize(
                json,
                out _,
                out string error), Is.False);
            StringAssert.Contains("newer", error);
        }

        [Test]
        public void SaveCodec_RejectsDuplicateOperationRecordIds()
        {
            CampaignSaveData source = CreateCampaignWithOperation();
            CampaignOperationRecordData duplicate =
                CampaignOperationRecordData.FromCompletedOperation(
                    CreateCompletedOperation(),
                    2,
                    "2026-07-18T12:10:00.0000000Z");
            duplicate.recordId = source.completedOperations[0].recordId;
            source.completedOperations.Add(duplicate);
            string json = JsonUtility.ToJson(source, false);

            Assert.That(CampaignSaveCodec.TryDeserialize(
                json,
                out _,
                out string error), Is.False);
            StringAssert.Contains("duplicate", error);
        }

        [Test]
        public void CompletedOperationRecord_PreservesExplicitPersistentRecordId()
        {
            string recordId = Guid.NewGuid().ToString("N");
            CompletedOperationRecord record = new CompletedOperationRecord(
                4,
                CreateReport(),
                "OP-06-1",
                "entry_south",
                new[] { "alpha" },
                Array.Empty<string>(),
                recordId);

            Assert.That(record.RecordId, Is.EqualTo(recordId));
        }

        private static CampaignSaveData CreateCampaignWithOperation()
        {
            CampaignSaveData campaign = new CampaignSaveData
            {
                schemaVersion = CampaignDataRules.CurrentSchemaVersion,
                campaignId = Guid.NewGuid().ToString("N"),
                officerDisplayName = "Alex Carter",
                badgeIdentifier = "A127",
                departmentName = CampaignDataRules.DefaultDepartmentName,
                createdUtc = "2026-07-18T12:00:00.0000000Z",
                updatedUtc = "2026-07-18T12:05:00.0000000Z"
            };
            campaign.completedOperations.Add(
                CampaignOperationRecordData.FromCompletedOperation(
                    CreateCompletedOperation(),
                    1,
                    "2026-07-18T12:05:00.0000000Z"));
            return campaign;
        }

        private static CompletedOperationRecord CreateCompletedOperation()
        {
            return new CompletedOperationRecord(
                1,
                CreateReport(),
                "OP-06-1",
                "entry_south",
                new[] { "alpha", "bravo" },
                new[] { "shield" });
        }

        private static AfterActionReport CreateReport()
        {
            return new AfterActionReport(
                "pressure-point",
                "Pressure Point",
                305d,
                300d,
                true,
                88,
                OperationalRating.Acceptable,
                MissionPerformanceTier.B,
                100,
                new[]
                {
                    new MissionObjectiveEvaluation(
                        "secure-primary",
                        "Secure the primary suspect",
                        MissionObjectiveType.SecureSubject,
                        MissionObjectiveStatus.Completed,
                        true,
                        30,
                        "Subject is in custody.")
                },
                new[]
                {
                    new RoeFinding(
                        "roe-1",
                        long.MaxValue,
                        45d,
                        ulong.MaxValue,
                        "suspect-1",
                        RoeDetermination.ReviewRequired,
                        RoeSeverity.Minor,
                        2,
                        "Force review",
                        "Review required by policy.")
                },
                new[]
                {
                    new MissionScoreCategory(
                        MissionScoreCategoryType.Objectives,
                        "Objectives",
                        28,
                        30,
                        "One deduction recorded.")
                },
                new MissionOutcomeMetrics(
                    1, 1, 0, 0, 0,
                    1, 1, 0, 0,
                    2, 0, 0, 0,
                    2, 2),
                "Operation resolved.");
        }
    }
}
