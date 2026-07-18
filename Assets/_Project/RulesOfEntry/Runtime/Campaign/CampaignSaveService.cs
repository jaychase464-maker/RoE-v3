using System;
using System.Globalization;
using System.IO;
using System.Text;
using RulesOfEntry.Core;
using RulesOfEntry.Operations;
using UnityEngine;

namespace RulesOfEntry.Campaign
{
    public static class CampaignSaveService
    {
        private const string CampaignFolderName = "Campaigns";
        private const string ActiveCampaignFileName = "active_campaign.txt";
        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);

        public static string CampaignRootPath => Path.Combine(
            Application.persistentDataPath,
            ProjectInfo.ProjectCode,
            CampaignFolderName);

        public static bool HasActiveCampaignSave
        {
            get
            {
                return TryReadActiveCampaignId(out string campaignId)
                    && (File.Exists(GetCampaignPath(campaignId))
                        || File.Exists(GetBackupPath(GetCampaignPath(campaignId))));
            }
        }

        public static bool TryCreateCampaign(
            string officerName,
            string badgeIdentifier,
            out CampaignSaveData campaign,
            out string error)
        {
            campaign = null;
            if (!CampaignDataRules.TryValidateNewCampaign(
                    officerName,
                    badgeIdentifier,
                    out string normalizedName,
                    out string normalizedBadge,
                    out error))
            {
                return false;
            }

            string now = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            CampaignSaveData created = new CampaignSaveData
            {
                schemaVersion = CampaignDataRules.CurrentSchemaVersion,
                campaignId = Guid.NewGuid().ToString("N"),
                officerDisplayName = normalizedName,
                badgeIdentifier = normalizedBadge,
                departmentName = CampaignDataRules.DefaultDepartmentName,
                createdUtc = now,
                updatedUtc = now
            };
            if (!TryWriteCampaign(created, out error)
                || !TryWriteAtomic(
                    GetActiveCampaignPath(),
                    created.campaignId,
                    out error))
            {
                return false;
            }

            campaign = CampaignSaveCodec.Clone(created);
            CampaignSession.SetActive(campaign);
            ProjectLog.Info(
                "Campaign",
                $"Created campaign {campaign.campaignId} for {campaign.officerDisplayName}.");
            return true;
        }

        public static bool TryContinueActiveCampaign(
            out CampaignSaveData campaign,
            out string error)
        {
            campaign = null;
            if (!TryReadActiveCampaignId(out string campaignId))
            {
                error = "No active campaign is selected.";
                return false;
            }

            if (!TryLoadCampaign(campaignId, out campaign, out error))
            {
                return false;
            }

            CampaignSession.SetActive(campaign);
            return true;
        }

        public static bool TryAppendCompletedOperation(
            CompletedOperationRecord completedOperation,
            out CampaignOperationRecordData archivedRecord,
            out string error)
        {
            archivedRecord = null;
            if (completedOperation == null)
            {
                error = "Completed operation is missing.";
                return false;
            }

            if (!CampaignSession.HasActiveCampaign)
            {
                error = "No active campaign is loaded.";
                return false;
            }

            CampaignSaveData updated = CampaignSaveCodec.Clone(
                CampaignSession.ActiveCampaign);
            CampaignOperationRecordData existing = updated.completedOperations.Find(
                record => string.Equals(
                    record.recordId,
                    completedOperation.RecordId,
                    StringComparison.Ordinal));
            if (existing != null)
            {
                archivedRecord = existing;
                error = string.Empty;
                return true;
            }

            archivedRecord = CampaignOperationRecordData.FromCompletedOperation(
                completedOperation,
                updated.completedOperations.Count + 1,
                DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            updated.completedOperations.Add(archivedRecord);
            updated.updatedUtc = DateTime.UtcNow.ToString(
                "O",
                CultureInfo.InvariantCulture);
            if (!TryWriteCampaign(updated, out error))
            {
                archivedRecord = null;
                return false;
            }

            CampaignSession.SetActive(updated);
            ProjectLog.Info(
                "Campaign",
                $"Archived {completedOperation.OperationCode} as operation {updated.completedOperations.Count}.");
            return true;
        }

        public static bool TryLoadCampaign(
            string campaignId,
            out CampaignSaveData campaign,
            out string error)
        {
            campaign = null;
            if (!CampaignDataRules.IsValidCampaignId(campaignId))
            {
                error = "Campaign identifier is invalid.";
                return false;
            }

            string path = GetCampaignPath(campaignId);
            if (TryReadCampaignFile(path, out campaign, out error)
                && string.Equals(
                    campaign.campaignId,
                    campaignId,
                    StringComparison.Ordinal))
            {
                return true;
            }

            if (campaign != null)
            {
                error = "The primary campaign file contains a mismatched identifier.";
            }

            string backupPath = GetBackupPath(path);
            if (TryReadCampaignFile(backupPath, out campaign, out string backupError)
                && string.Equals(
                    campaign.campaignId,
                    campaignId,
                    StringComparison.Ordinal))
            {
                ProjectLog.Warning(
                    "Campaign",
                    "Loaded the campaign backup because the primary save was unavailable.");
                error = string.Empty;
                return true;
            }

            if (campaign != null)
            {
                backupError = "The campaign backup contains a mismatched identifier.";
            }

            campaign = null;
            error = string.IsNullOrWhiteSpace(error) ? backupError : error;
            return false;
        }

        private static bool TryWriteCampaign(
            CampaignSaveData campaign,
            out string error)
        {
            try
            {
                string json = CampaignSaveCodec.Serialize(campaign, true);
                return TryWriteAtomic(
                    GetCampaignPath(campaign.campaignId),
                    json,
                    out error);
            }
            catch (Exception exception)
            {
                error = $"Campaign could not be serialized: {exception.Message}";
                return false;
            }
        }

        private static bool TryReadCampaignFile(
            string path,
            out CampaignSaveData campaign,
            out string error)
        {
            campaign = null;
            if (!File.Exists(path))
            {
                error = "Campaign save file does not exist.";
                return false;
            }

            try
            {
                string json = File.ReadAllText(path, Utf8WithoutBom);
                return CampaignSaveCodec.TryDeserialize(json, out campaign, out error);
            }
            catch (Exception exception)
            {
                error = $"Campaign save could not be read: {exception.Message}";
                return false;
            }
        }

        private static bool TryWriteAtomic(
            string destinationPath,
            string contents,
            out string error)
        {
            string temporaryPath = destinationPath + ".tmp";
            string backupPath = GetBackupPath(destinationPath);
            try
            {
                string directory = Path.GetDirectoryName(destinationPath);
                if (string.IsNullOrWhiteSpace(directory))
                {
                    error = "Campaign save directory is invalid.";
                    return false;
                }

                Directory.CreateDirectory(directory);
                File.WriteAllText(temporaryPath, contents, Utf8WithoutBom);
                if (File.Exists(destinationPath))
                {
                    try
                    {
                        File.Replace(temporaryPath, destinationPath, backupPath, true);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        File.Copy(destinationPath, backupPath, true);
                        File.Delete(destinationPath);
                        File.Move(temporaryPath, destinationPath);
                    }
                }
                else
                {
                    File.Move(temporaryPath, destinationPath);
                }

                error = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                error = $"Campaign save write failed: {exception.Message}";
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(temporaryPath))
                    {
                        File.Delete(temporaryPath);
                    }
                }
                catch (Exception)
                {
                    // Cleanup failure does not replace the actionable write error.
                }
            }
        }

        private static bool TryReadActiveCampaignId(out string campaignId)
        {
            string path = GetActiveCampaignPath();
            if (TryReadCampaignIdFile(path, out campaignId))
            {
                return true;
            }

            return TryReadCampaignIdFile(GetBackupPath(path), out campaignId);
        }

        private static bool TryReadCampaignIdFile(
            string path,
            out string campaignId)
        {
            campaignId = string.Empty;
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                campaignId = File.ReadAllText(path, Utf8WithoutBom).Trim();
                return CampaignDataRules.IsValidCampaignId(campaignId);
            }
            catch (Exception)
            {
                campaignId = string.Empty;
                return false;
            }
        }

        private static string GetCampaignPath(string campaignId)
        {
            return Path.Combine(CampaignRootPath, campaignId + ".json");
        }

        private static string GetActiveCampaignPath()
        {
            return Path.Combine(CampaignRootPath, ActiveCampaignFileName);
        }

        private static string GetBackupPath(string path)
        {
            return path + ".bak";
        }
    }
}
