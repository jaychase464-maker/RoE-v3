using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RulesOfEntry.Missions;
using RulesOfEntry.Operations;

namespace RulesOfEntry.Campaign
{
    [Serializable]
    public sealed class CampaignSaveData
    {
        public int schemaVersion = CampaignDataRules.CurrentSchemaVersion;
        public string campaignId = string.Empty;
        public string officerDisplayName = string.Empty;
        public string badgeIdentifier = string.Empty;
        public string departmentName = CampaignDataRules.DefaultDepartmentName;
        public string createdUtc = string.Empty;
        public string updatedUtc = string.Empty;
        public List<CampaignOperationRecordData> completedOperations =
            new List<CampaignOperationRecordData>();

        public int CompletedOperationCount => completedOperations?.Count ?? 0;
    }

    [Serializable]
    public sealed class CampaignOperationRecordData
    {
        public string recordId = string.Empty;
        public int operationSequence;
        public string completedUtc = string.Empty;
        public string missionId = string.Empty;
        public string missionName = string.Empty;
        public string operationCode = string.Empty;
        public string entryPointId = string.Empty;
        public string[] assignedOfficerIds = Array.Empty<string>();
        public string[] supportAssetIds = Array.Empty<string>();
        public double generatedAtSeconds;
        public double elapsedSeconds;
        public bool finalReport;
        public int score;
        public int operationalRating;
        public int performanceTier;
        public int scoreCap;
        public CampaignObjectiveRecordData[] objectives =
            Array.Empty<CampaignObjectiveRecordData>();
        public CampaignRoeFindingRecordData[] roeFindings =
            Array.Empty<CampaignRoeFindingRecordData>();
        public CampaignScoreCategoryRecordData[] categories =
            Array.Empty<CampaignScoreCategoryRecordData>();
        public CampaignOutcomeMetricsRecordData metrics =
            new CampaignOutcomeMetricsRecordData();
        public string summary = string.Empty;

        public static CampaignOperationRecordData FromCompletedOperation(
            CompletedOperationRecord record,
            int operationSequence,
            string completedUtc)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            AfterActionReport report = record.Report;
            return new CampaignOperationRecordData
            {
                recordId = record.RecordId,
                operationSequence = Math.Max(1, operationSequence),
                completedUtc = completedUtc ?? string.Empty,
                missionId = report.MissionId,
                missionName = report.MissionName,
                operationCode = record.OperationCode,
                entryPointId = record.EntryPointId,
                assignedOfficerIds = record.AssignedOfficerIds.ToArray(),
                supportAssetIds = record.SupportAssetIds.ToArray(),
                generatedAtSeconds = report.GeneratedAtSeconds,
                elapsedSeconds = report.ElapsedSeconds,
                finalReport = report.Final,
                score = report.Score,
                operationalRating = (int)report.Rating,
                performanceTier = (int)report.Tier,
                scoreCap = report.ScoreCap,
                objectives = report.Objectives
                    .Select(CampaignObjectiveRecordData.FromEvaluation)
                    .ToArray(),
                roeFindings = report.RoeFindings
                    .Select(CampaignRoeFindingRecordData.FromFinding)
                    .ToArray(),
                categories = report.Categories
                    .Select(CampaignScoreCategoryRecordData.FromCategory)
                    .ToArray(),
                metrics = CampaignOutcomeMetricsRecordData.FromMetrics(report.Metrics),
                summary = report.Summary
            };
        }

        public CompletedOperationRecord ToCompletedOperationRecord()
        {
            if (!finalReport)
            {
                throw new InvalidOperationException(
                    "A persisted operation must contain a final report.");
            }

            AfterActionReport report = new AfterActionReport(
                missionId,
                missionName,
                generatedAtSeconds,
                elapsedSeconds,
                true,
                score,
                ReadRating(operationalRating),
                ReadTier(performanceTier),
                scoreCap,
                (objectives ?? Array.Empty<CampaignObjectiveRecordData>())
                    .Where(value => value != null)
                    .Select(value => value.ToEvaluation())
                    .ToArray(),
                (roeFindings ?? Array.Empty<CampaignRoeFindingRecordData>())
                    .Where(value => value != null)
                    .Select(value => value.ToFinding())
                    .ToArray(),
                (categories ?? Array.Empty<CampaignScoreCategoryRecordData>())
                    .Where(value => value != null)
                    .Select(value => value.ToCategory())
                    .ToArray(),
                (metrics ?? new CampaignOutcomeMetricsRecordData()).ToMetrics(),
                summary);
            return new CompletedOperationRecord(
                Math.Max(1, operationSequence),
                report,
                operationCode,
                entryPointId,
                assignedOfficerIds ?? Array.Empty<string>(),
                supportAssetIds ?? Array.Empty<string>(),
                recordId);
        }

        private static OperationalRating ReadRating(int value)
        {
            return Enum.IsDefined(typeof(OperationalRating), value)
                ? (OperationalRating)value
                : OperationalRating.NotRated;
        }

        private static MissionPerformanceTier ReadTier(int value)
        {
            return Enum.IsDefined(typeof(MissionPerformanceTier), value)
                ? (MissionPerformanceTier)value
                : MissionPerformanceTier.F;
        }
    }

    [Serializable]
    public sealed class CampaignObjectiveRecordData
    {
        public string objectiveId = string.Empty;
        public string displayName = string.Empty;
        public int objectiveType;
        public int status;
        public bool required;
        public int failureDeduction;
        public string rationale = string.Empty;

        public static CampaignObjectiveRecordData FromEvaluation(
            MissionObjectiveEvaluation evaluation)
        {
            return new CampaignObjectiveRecordData
            {
                objectiveId = evaluation.ObjectiveId,
                displayName = evaluation.DisplayName,
                objectiveType = (int)evaluation.Type,
                status = (int)evaluation.Status,
                required = evaluation.Required,
                failureDeduction = evaluation.FailureDeduction,
                rationale = evaluation.Rationale
            };
        }

        public MissionObjectiveEvaluation ToEvaluation()
        {
            MissionObjectiveType type = Enum.IsDefined(
                typeof(MissionObjectiveType),
                objectiveType)
                ? (MissionObjectiveType)objectiveType
                : MissionObjectiveType.SecureSubject;
            MissionObjectiveStatus savedStatus = Enum.IsDefined(
                typeof(MissionObjectiveStatus),
                status)
                ? (MissionObjectiveStatus)status
                : MissionObjectiveStatus.Failed;
            return new MissionObjectiveEvaluation(
                objectiveId,
                displayName,
                type,
                savedStatus,
                required,
                failureDeduction,
                rationale);
        }
    }

    [Serializable]
    public sealed class CampaignRoeFindingRecordData
    {
        public string findingId = string.Empty;
        public string forceEventSequence = "0";
        public double occurredAtSeconds;
        public string shooterEntityId = "0";
        public string subjectActorId = string.Empty;
        public int determination;
        public int severity;
        public int scoreDeduction;
        public string summary = string.Empty;
        public string rationale = string.Empty;

        public static CampaignRoeFindingRecordData FromFinding(RoeFinding finding)
        {
            return new CampaignRoeFindingRecordData
            {
                findingId = finding.FindingId,
                forceEventSequence = finding.ForceEventSequence.ToString(
                    CultureInfo.InvariantCulture),
                occurredAtSeconds = finding.OccurredAtSeconds,
                shooterEntityId = finding.ShooterEntityId.ToString(
                    CultureInfo.InvariantCulture),
                subjectActorId = finding.SubjectActorId,
                determination = (int)finding.Determination,
                severity = (int)finding.Severity,
                scoreDeduction = finding.ScoreDeduction,
                summary = finding.Summary,
                rationale = finding.Rationale
            };
        }

        public RoeFinding ToFinding()
        {
            long.TryParse(
                forceEventSequence,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long sequence);
            ulong.TryParse(
                shooterEntityId,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out ulong entityId);
            RoeDetermination savedDetermination = Enum.IsDefined(
                typeof(RoeDetermination),
                determination)
                ? (RoeDetermination)determination
                : RoeDetermination.ReviewRequired;
            RoeSeverity savedSeverity = Enum.IsDefined(typeof(RoeSeverity), severity)
                ? (RoeSeverity)severity
                : RoeSeverity.Serious;
            return new RoeFinding(
                findingId,
                sequence,
                occurredAtSeconds,
                entityId,
                subjectActorId,
                savedDetermination,
                savedSeverity,
                scoreDeduction,
                summary,
                rationale);
        }
    }

    [Serializable]
    public sealed class CampaignScoreCategoryRecordData
    {
        public int categoryType;
        public string displayName = string.Empty;
        public int earnedScore;
        public int maximumScore;
        public string summary = string.Empty;

        public static CampaignScoreCategoryRecordData FromCategory(
            MissionScoreCategory category)
        {
            return new CampaignScoreCategoryRecordData
            {
                categoryType = (int)category.Type,
                displayName = category.DisplayName,
                earnedScore = category.EarnedScore,
                maximumScore = category.MaximumScore,
                summary = category.Summary
            };
        }

        public MissionScoreCategory ToCategory()
        {
            MissionScoreCategoryType type = Enum.IsDefined(
                typeof(MissionScoreCategoryType),
                categoryType)
                ? (MissionScoreCategoryType)categoryType
                : MissionScoreCategoryType.Objectives;
            return new MissionScoreCategory(
                type,
                displayName,
                earnedScore,
                maximumScore,
                summary);
        }
    }

    [Serializable]
    public sealed class CampaignOutcomeMetricsRecordData
    {
        public int civiliansTotal;
        public int civiliansSaved;
        public int civiliansWounded;
        public int civiliansIncapacitated;
        public int civiliansKilled;
        public int suspectsTotal;
        public int suspectsArrested;
        public int suspectsIncapacitated;
        public int suspectsKilled;
        public int officersTotal;
        public int officersWounded;
        public int officersIncapacitated;
        public int officersKilled;
        public int evidenceOpportunities;
        public int evidenceItemsSecured;

        public static CampaignOutcomeMetricsRecordData FromMetrics(
            MissionOutcomeMetrics metrics)
        {
            return new CampaignOutcomeMetricsRecordData
            {
                civiliansTotal = metrics.CiviliansTotal,
                civiliansSaved = metrics.CiviliansSaved,
                civiliansWounded = metrics.CiviliansWounded,
                civiliansIncapacitated = metrics.CiviliansIncapacitated,
                civiliansKilled = metrics.CiviliansKilled,
                suspectsTotal = metrics.SuspectsTotal,
                suspectsArrested = metrics.SuspectsArrested,
                suspectsIncapacitated = metrics.SuspectsIncapacitated,
                suspectsKilled = metrics.SuspectsKilled,
                officersTotal = metrics.OfficersTotal,
                officersWounded = metrics.OfficersWounded,
                officersIncapacitated = metrics.OfficersIncapacitated,
                officersKilled = metrics.OfficersKilled,
                evidenceOpportunities = metrics.EvidenceOpportunities,
                evidenceItemsSecured = metrics.EvidenceItemsSecured
            };
        }

        public MissionOutcomeMetrics ToMetrics()
        {
            return new MissionOutcomeMetrics(
                civiliansTotal,
                civiliansSaved,
                civiliansWounded,
                civiliansIncapacitated,
                civiliansKilled,
                suspectsTotal,
                suspectsArrested,
                suspectsIncapacitated,
                suspectsKilled,
                officersTotal,
                officersWounded,
                officersIncapacitated,
                officersKilled,
                evidenceOpportunities,
                evidenceItemsSecured);
        }
    }
}
