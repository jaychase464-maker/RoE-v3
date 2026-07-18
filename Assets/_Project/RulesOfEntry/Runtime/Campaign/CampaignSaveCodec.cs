using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace RulesOfEntry.Campaign
{
    public static class CampaignSaveCodec
    {
        public static string Serialize(CampaignSaveData campaign, bool prettyPrint)
        {
            if (campaign == null)
            {
                throw new ArgumentNullException(nameof(campaign));
            }

            if (!TryNormalize(campaign, out string error))
            {
                throw new InvalidOperationException(error);
            }

            return JsonUtility.ToJson(campaign, prettyPrint);
        }

        public static bool TryDeserialize(
            string json,
            out CampaignSaveData campaign,
            out string error)
        {
            campaign = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Campaign save data is empty.";
                return false;
            }

            try
            {
                campaign = JsonUtility.FromJson<CampaignSaveData>(json);
            }
            catch (Exception exception)
            {
                error = $"Campaign JSON could not be read: {exception.Message}";
                return false;
            }

            if (!TryNormalize(campaign, out error))
            {
                campaign = null;
                return false;
            }

            return true;
        }

        public static CampaignSaveData Clone(CampaignSaveData campaign)
        {
            string json = Serialize(campaign, false);
            if (!TryDeserialize(json, out CampaignSaveData clone, out string error))
            {
                throw new InvalidOperationException(error);
            }

            return clone;
        }

        private static bool TryNormalize(CampaignSaveData campaign, out string error)
        {
            if (campaign == null)
            {
                error = "Campaign save data is missing.";
                return false;
            }

            if (campaign.schemaVersion > CampaignDataRules.CurrentSchemaVersion)
            {
                error =
                    $"Campaign schema {campaign.schemaVersion} is newer than supported schema {CampaignDataRules.CurrentSchemaVersion}.";
                return false;
            }

            if (campaign.schemaVersion <= 0)
            {
                campaign.schemaVersion = CampaignDataRules.CurrentSchemaVersion;
            }

            if (!CampaignDataRules.IsValidCampaignId(campaign.campaignId))
            {
                error = "Campaign identifier is invalid.";
                return false;
            }

            campaign.officerDisplayName = CampaignDataRules.NormalizeOfficerName(
                campaign.officerDisplayName);
            campaign.badgeIdentifier = CampaignDataRules.NormalizeBadgeIdentifier(
                campaign.badgeIdentifier);
            campaign.departmentName = CampaignDataRules.NormalizeDepartmentName(
                campaign.departmentName);
            if (campaign.officerDisplayName.Length < 2
                || campaign.badgeIdentifier.Length < 2)
            {
                error = "Campaign officer identity is incomplete.";
                return false;
            }

            if (!IsUtcTimestamp(campaign.createdUtc)
                || !IsUtcTimestamp(campaign.updatedUtc))
            {
                error = "Campaign timestamps are invalid.";
                return false;
            }

            campaign.completedOperations ??= new List<CampaignOperationRecordData>();
            HashSet<string> recordIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (CampaignOperationRecordData operation in campaign.completedOperations)
            {
                if (!TryNormalizeOperation(operation, recordIds, out error))
                {
                    return false;
                }
            }

            campaign.completedOperations = campaign.completedOperations
                .OrderBy(operation => operation.operationSequence)
                .ToList();
            for (int index = 0; index < campaign.completedOperations.Count; index++)
            {
                campaign.completedOperations[index].operationSequence = index + 1;
            }

            error = string.Empty;
            return true;
        }

        private static bool TryNormalizeOperation(
            CampaignOperationRecordData operation,
            ISet<string> recordIds,
            out string error)
        {
            if (operation == null
                || !operation.finalReport
                || !CampaignDataRules.IsValidCampaignId(operation.recordId)
                || string.IsNullOrWhiteSpace(operation.missionId)
                || string.IsNullOrWhiteSpace(operation.missionName)
                || !IsUtcTimestamp(operation.completedUtc))
            {
                error = "A completed-operation record is invalid.";
                return false;
            }

            if (!recordIds.Add(operation.recordId))
            {
                error = "Campaign history contains a duplicate operation record.";
                return false;
            }

            operation.operationSequence = Math.Max(1, operation.operationSequence);
            operation.missionId = operation.missionId.Trim();
            operation.missionName = operation.missionName.Trim();
            operation.operationCode = operation.operationCode?.Trim() ?? string.Empty;
            operation.entryPointId = operation.entryPointId?.Trim() ?? string.Empty;
            operation.assignedOfficerIds = NormalizeIds(operation.assignedOfficerIds);
            operation.supportAssetIds = NormalizeIds(operation.supportAssetIds);
            operation.score = Math.Max(0, Math.Min(100, operation.score));
            operation.scoreCap = Math.Max(0, Math.Min(100, operation.scoreCap));
            operation.elapsedSeconds = Math.Max(0d, operation.elapsedSeconds);
            operation.objectives ??= Array.Empty<CampaignObjectiveRecordData>();
            operation.roeFindings ??= Array.Empty<CampaignRoeFindingRecordData>();
            operation.categories ??= Array.Empty<CampaignScoreCategoryRecordData>();
            operation.metrics ??= new CampaignOutcomeMetricsRecordData();
            operation.summary ??= string.Empty;
            error = string.Empty;
            return true;
        }

        private static string[] NormalizeIds(IEnumerable<string> values)
        {
            return values?
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>();
        }

        private static bool IsUtcTimestamp(string value)
        {
            return DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out DateTimeOffset timestamp)
                && timestamp.Offset == TimeSpan.Zero;
        }
    }
}
