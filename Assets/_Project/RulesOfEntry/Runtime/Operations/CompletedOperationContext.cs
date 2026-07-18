using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Missions;

namespace RulesOfEntry.Operations
{
    /// <summary>
    /// Immutable, scene-reference-free record of one completed operation. This is a
    /// session boundary only; disk save ownership remains a future campaign milestone.
    /// </summary>
    public sealed class CompletedOperationRecord
    {
        public CompletedOperationRecord(
            int sessionSequence,
            AfterActionReport report,
            string operationCode,
            string entryPointId,
            IEnumerable<string> assignedOfficerIds,
            IEnumerable<string> supportAssetIds)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            if (!report.Final)
            {
                throw new ArgumentException(
                    "Only a final after-action report can become a completed operation.",
                    nameof(report));
            }

            SessionSequence = Math.Max(1, sessionSequence);
            Report = report;
            OperationCode = string.IsNullOrWhiteSpace(operationCode)
                ? report.MissionId.Trim()
                : operationCode.Trim();
            EntryPointId = entryPointId?.Trim() ?? string.Empty;
            AssignedOfficerIds = Array.AsReadOnly(Normalize(assignedOfficerIds));
            SupportAssetIds = Array.AsReadOnly(Normalize(supportAssetIds));
        }

        public int SessionSequence { get; }
        public AfterActionReport Report { get; }
        public string MissionId => Report.MissionId;
        public string MissionName => Report.MissionName;
        public string OperationCode { get; }
        public string EntryPointId { get; }
        public IReadOnlyList<string> AssignedOfficerIds { get; }
        public IReadOnlyList<string> SupportAssetIds { get; }

        private static string[] Normalize(IEnumerable<string> values)
        {
            return values?
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Carries the most recent final report from an operation scene back to headquarters.
    /// It deliberately owns no GameObject, Component, Transform, or ScriptableObject.
    /// </summary>
    public static class CompletedOperationContext
    {
        private static int sequence;

        public static CompletedOperationRecord Latest { get; private set; }
        public static bool HasCompletedOperation => Latest != null;

        public static bool Capture(
            AfterActionReport report,
            string operationCode,
            string entryPointId,
            IEnumerable<string> assignedOfficerIds,
            IEnumerable<string> supportAssetIds)
        {
            if (report == null || !report.Final)
            {
                return false;
            }

            sequence++;
            Latest = new CompletedOperationRecord(
                sequence,
                report,
                operationCode,
                entryPointId,
                assignedOfficerIds,
                supportAssetIds);
            return true;
        }

        public static bool TryGetLatest(out CompletedOperationRecord record)
        {
            record = Latest;
            return record != null;
        }

        public static void Clear()
        {
            Latest = null;
        }
    }
}
