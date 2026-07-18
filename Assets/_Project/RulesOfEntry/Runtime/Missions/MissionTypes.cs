using System;
using System.Collections.Generic;
using RulesOfEntry.Actors;
using RulesOfEntry.Combat;
using RulesOfEntry.Officers;

namespace RulesOfEntry.Missions
{
    public enum MissionPhase
    {
        Briefing = 0,
        Active = 1,
        AfterAction = 2
    }

    public enum MissionObjectiveType
    {
        SecureSubject = 0,
        ProtectActor = 1,
        VerifyRoomClear = 2,
        PreserveOfficerTeam = 3
    }

    public enum MissionObjectiveStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2
    }

    public enum RoeDetermination
    {
        WithinPolicy = 0,
        ReviewRequired = 1,
        Violation = 2
    }

    public enum RoeSeverity
    {
        Advisory = 0,
        Minor = 1,
        Serious = 2,
        Critical = 3
    }

    public enum OperationalRating
    {
        NotRated = 0,
        Exemplary = 1,
        Acceptable = 2,
        Deficient = 3,
        CriticalFailure = 4
    }

    public enum MissionPerformanceTier
    {
        NotRated = 0,
        S = 1,
        A = 2,
        B = 3,
        C = 4,
        D = 5,
        F = 6
    }

    public enum MissionScoreCategoryType
    {
        Objectives = 0,
        CivilianSafety = 1,
        SuspectCustody = 2,
        OfficerSafety = 3,
        RulesOfEngagement = 4,
        Evidence = 5,
        Time = 6
    }

    public readonly struct ActorEvidenceSnapshot
    {
        public ActorEvidenceSnapshot(
            string actorId,
            ulong entityId,
            ActorRole role,
            ActorConditionLevel condition,
            CustodyState custody,
            HumanBehaviorState behavior,
            bool weaponAccessible)
            : this(
                actorId,
                entityId,
                role,
                condition,
                custody,
                behavior,
                weaponAccessible,
                weaponAccessible,
                false,
                false,
                0)
        {
        }

        public ActorEvidenceSnapshot(
            string actorId,
            ulong entityId,
            ActorRole role,
            ActorConditionLevel condition,
            CustodyState custody,
            HumanBehaviorState behavior,
            bool weaponAccessible,
            bool hadWeapon,
            bool searched,
            bool weaponSecured,
            int reportableItemCount)
        {
            ActorId = actorId ?? string.Empty;
            EntityId = entityId;
            Role = role;
            Condition = condition;
            Custody = custody;
            Behavior = behavior;
            WeaponAccessible = weaponAccessible;
            HadWeapon = hadWeapon;
            Searched = searched;
            WeaponSecured = weaponSecured;
            ReportableItemCount = Math.Max(0, reportableItemCount);
        }

        public string ActorId { get; }
        public ulong EntityId { get; }
        public ActorRole Role { get; }
        public ActorConditionLevel Condition { get; }
        public CustodyState Custody { get; }
        public HumanBehaviorState Behavior { get; }
        public bool WeaponAccessible { get; }
        public bool HadWeapon { get; }
        public bool Searched { get; }
        public bool WeaponSecured { get; }
        public int ReportableItemCount { get; }
    }

    public readonly struct RoomEvidenceSnapshot
    {
        public RoomEvidenceSnapshot(
            string roomId,
            TacticalRoomClearanceState state,
            int activeThreatCount,
            int actionableOfficerCount)
        {
            RoomId = roomId ?? string.Empty;
            State = state;
            ActiveThreatCount = Math.Max(0, activeThreatCount);
            ActionableOfficerCount = Math.Max(0, actionableOfficerCount);
        }

        public string RoomId { get; }
        public TacticalRoomClearanceState State { get; }
        public int ActiveThreatCount { get; }
        public int ActionableOfficerCount { get; }
    }

    /// <summary>
    /// Immutable incident evidence captured from factual ledgers and authoritative actor state.
    /// </summary>
    public sealed class MissionEvidenceSnapshot
    {
        public MissionEvidenceSnapshot(
            double capturedAtSeconds,
            double missionElapsedSeconds,
            ActorEvidenceSnapshot[] actors,
            RoomEvidenceSnapshot[] rooms,
            ForceEventRecord[] forceEvents,
            CustodyEventRecord[] custodyEvents,
            OfficerOrderEventRecord[] officerOrderEvents,
            OfficerInitiativeRecord[] initiativeEvents)
        {
            CapturedAtSeconds = capturedAtSeconds;
            MissionElapsedSeconds = Math.Max(0d, missionElapsedSeconds);
            Actors = Array.AsReadOnly(actors != null
                ? (ActorEvidenceSnapshot[])actors.Clone()
                : Array.Empty<ActorEvidenceSnapshot>());
            Rooms = Array.AsReadOnly(rooms != null
                ? (RoomEvidenceSnapshot[])rooms.Clone()
                : Array.Empty<RoomEvidenceSnapshot>());
            ForceEvents = Array.AsReadOnly(forceEvents != null
                ? (ForceEventRecord[])forceEvents.Clone()
                : Array.Empty<ForceEventRecord>());
            CustodyEvents = Array.AsReadOnly(custodyEvents != null
                ? (CustodyEventRecord[])custodyEvents.Clone()
                : Array.Empty<CustodyEventRecord>());
            OfficerOrderEvents = Array.AsReadOnly(officerOrderEvents != null
                ? (OfficerOrderEventRecord[])officerOrderEvents.Clone()
                : Array.Empty<OfficerOrderEventRecord>());
            InitiativeEvents = Array.AsReadOnly(initiativeEvents != null
                ? (OfficerInitiativeRecord[])initiativeEvents.Clone()
                : Array.Empty<OfficerInitiativeRecord>());
        }

        public double CapturedAtSeconds { get; }
        public double MissionElapsedSeconds { get; }
        public IReadOnlyList<ActorEvidenceSnapshot> Actors { get; }
        public IReadOnlyList<RoomEvidenceSnapshot> Rooms { get; }
        public IReadOnlyList<ForceEventRecord> ForceEvents { get; }
        public IReadOnlyList<CustodyEventRecord> CustodyEvents { get; }
        public IReadOnlyList<OfficerOrderEventRecord> OfficerOrderEvents { get; }
        public IReadOnlyList<OfficerInitiativeRecord> InitiativeEvents { get; }

        public static MissionEvidenceSnapshot Empty => new MissionEvidenceSnapshot(
            0d,
            0d,
            Array.Empty<ActorEvidenceSnapshot>(),
            Array.Empty<RoomEvidenceSnapshot>(),
            Array.Empty<ForceEventRecord>(),
            Array.Empty<CustodyEventRecord>(),
            Array.Empty<OfficerOrderEventRecord>(),
            Array.Empty<OfficerInitiativeRecord>());
    }

    public sealed class MissionObjectiveEvaluation
    {
        public MissionObjectiveEvaluation(
            string objectiveId,
            string displayName,
            MissionObjectiveType type,
            MissionObjectiveStatus status,
            bool required,
            int failureDeduction,
            string rationale)
        {
            ObjectiveId = objectiveId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Type = type;
            Status = status;
            Required = required;
            FailureDeduction = Math.Max(0, failureDeduction);
            Rationale = rationale ?? string.Empty;
        }

        public string ObjectiveId { get; }
        public string DisplayName { get; }
        public MissionObjectiveType Type { get; }
        public MissionObjectiveStatus Status { get; }
        public bool Required { get; }
        public int FailureDeduction { get; }
        public string Rationale { get; }
    }

    public sealed class RoeFinding
    {
        public RoeFinding(
            string findingId,
            long forceEventSequence,
            double occurredAtSeconds,
            ulong shooterEntityId,
            string subjectActorId,
            RoeDetermination determination,
            RoeSeverity severity,
            int scoreDeduction,
            string summary,
            string rationale)
        {
            FindingId = findingId ?? string.Empty;
            ForceEventSequence = forceEventSequence;
            OccurredAtSeconds = occurredAtSeconds;
            ShooterEntityId = shooterEntityId;
            SubjectActorId = subjectActorId ?? string.Empty;
            Determination = determination;
            Severity = severity;
            ScoreDeduction = Math.Max(0, scoreDeduction);
            Summary = summary ?? string.Empty;
            Rationale = rationale ?? string.Empty;
        }

        public string FindingId { get; }
        public long ForceEventSequence { get; }
        public double OccurredAtSeconds { get; }
        public ulong ShooterEntityId { get; }
        public string SubjectActorId { get; }
        public RoeDetermination Determination { get; }
        public RoeSeverity Severity { get; }
        public int ScoreDeduction { get; }
        public string Summary { get; }
        public string Rationale { get; }
    }

    public sealed class MissionScoreCategory
    {
        public MissionScoreCategory(
            MissionScoreCategoryType type,
            string displayName,
            int earnedScore,
            int maximumScore,
            string summary)
        {
            Type = type;
            DisplayName = displayName ?? string.Empty;
            MaximumScore = Math.Max(0, maximumScore);
            EarnedScore = Math.Max(0, Math.Min(MaximumScore, earnedScore));
            Summary = summary ?? string.Empty;
        }

        public MissionScoreCategoryType Type { get; }
        public string DisplayName { get; }
        public int EarnedScore { get; }
        public int MaximumScore { get; }
        public int LostScore => MaximumScore - EarnedScore;
        public string Summary { get; }
    }

    public sealed class MissionOutcomeMetrics
    {
        public MissionOutcomeMetrics(
            int civiliansTotal,
            int civiliansSaved,
            int civiliansWounded,
            int civiliansIncapacitated,
            int civiliansKilled,
            int suspectsTotal,
            int suspectsArrested,
            int suspectsIncapacitated,
            int suspectsKilled,
            int officersTotal,
            int officersWounded,
            int officersIncapacitated,
            int officersKilled,
            int evidenceOpportunities,
            int evidenceItemsSecured)
        {
            CiviliansTotal = Math.Max(0, civiliansTotal);
            CiviliansSaved = Math.Max(0, civiliansSaved);
            CiviliansWounded = Math.Max(0, civiliansWounded);
            CiviliansIncapacitated = Math.Max(0, civiliansIncapacitated);
            CiviliansKilled = Math.Max(0, civiliansKilled);
            SuspectsTotal = Math.Max(0, suspectsTotal);
            SuspectsArrested = Math.Max(0, suspectsArrested);
            SuspectsIncapacitated = Math.Max(0, suspectsIncapacitated);
            SuspectsKilled = Math.Max(0, suspectsKilled);
            OfficersTotal = Math.Max(0, officersTotal);
            OfficersWounded = Math.Max(0, officersWounded);
            OfficersIncapacitated = Math.Max(0, officersIncapacitated);
            OfficersKilled = Math.Max(0, officersKilled);
            EvidenceOpportunities = Math.Max(0, evidenceOpportunities);
            EvidenceItemsSecured = Math.Max(0, evidenceItemsSecured);
        }

        public int CiviliansTotal { get; }
        public int CiviliansSaved { get; }
        public int CiviliansWounded { get; }
        public int CiviliansIncapacitated { get; }
        public int CiviliansKilled { get; }
        public int SuspectsTotal { get; }
        public int SuspectsArrested { get; }
        public int SuspectsIncapacitated { get; }
        public int SuspectsKilled { get; }
        public int OfficersTotal { get; }
        public int OfficersWounded { get; }
        public int OfficersIncapacitated { get; }
        public int OfficersKilled { get; }
        public int EvidenceOpportunities { get; }
        public int EvidenceItemsSecured { get; }
    }

    public sealed class AfterActionReport
    {
        public AfterActionReport(
            string missionId,
            string missionName,
            double generatedAtSeconds,
            double elapsedSeconds,
            bool final,
            int score,
            OperationalRating rating,
            MissionObjectiveEvaluation[] objectives,
            RoeFinding[] roeFindings,
            string summary)
            : this(
                missionId,
                missionName,
                generatedAtSeconds,
                elapsedSeconds,
                final,
                score,
                rating,
                GetLegacyTier(final, score),
                100,
                objectives,
                roeFindings,
                Array.Empty<MissionScoreCategory>(),
                new MissionOutcomeMetrics(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                summary)
        {
        }

        public AfterActionReport(
            string missionId,
            string missionName,
            double generatedAtSeconds,
            double elapsedSeconds,
            bool final,
            int score,
            OperationalRating rating,
            MissionPerformanceTier tier,
            int scoreCap,
            MissionObjectiveEvaluation[] objectives,
            RoeFinding[] roeFindings,
            MissionScoreCategory[] categories,
            MissionOutcomeMetrics metrics,
            string summary)
        {
            MissionId = missionId ?? string.Empty;
            MissionName = missionName ?? string.Empty;
            GeneratedAtSeconds = generatedAtSeconds;
            ElapsedSeconds = Math.Max(0d, elapsedSeconds);
            Final = final;
            Score = Math.Max(0, Math.Min(100, score));
            Rating = rating;
            Tier = final ? tier : MissionPerformanceTier.NotRated;
            ScoreCap = Math.Max(0, Math.Min(100, scoreCap));
            Objectives = Array.AsReadOnly(objectives != null
                ? (MissionObjectiveEvaluation[])objectives.Clone()
                : Array.Empty<MissionObjectiveEvaluation>());
            RoeFindings = Array.AsReadOnly(roeFindings != null
                ? (RoeFinding[])roeFindings.Clone()
                : Array.Empty<RoeFinding>());
            Categories = Array.AsReadOnly(categories != null
                ? (MissionScoreCategory[])categories.Clone()
                : Array.Empty<MissionScoreCategory>());
            Metrics = metrics ?? new MissionOutcomeMetrics(
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            Summary = summary ?? string.Empty;
        }

        public string MissionId { get; }
        public string MissionName { get; }
        public double GeneratedAtSeconds { get; }
        public double ElapsedSeconds { get; }
        public bool Final { get; }
        public int Score { get; }
        public OperationalRating Rating { get; }
        public MissionPerformanceTier Tier { get; }
        public int ScoreCap { get; }
        public IReadOnlyList<MissionObjectiveEvaluation> Objectives { get; }
        public IReadOnlyList<RoeFinding> RoeFindings { get; }
        public IReadOnlyList<MissionScoreCategory> Categories { get; }
        public MissionOutcomeMetrics Metrics { get; }
        public string Summary { get; }

        private static MissionPerformanceTier GetLegacyTier(bool final, int score)
        {
            if (!final)
            {
                return MissionPerformanceTier.NotRated;
            }

            int clamped = Math.Max(0, Math.Min(100, score));
            if (clamped >= 95)
            {
                return MissionPerformanceTier.S;
            }

            if (clamped >= 90)
            {
                return MissionPerformanceTier.A;
            }

            if (clamped >= 80)
            {
                return MissionPerformanceTier.B;
            }

            if (clamped >= 75)
            {
                return MissionPerformanceTier.C;
            }

            if (clamped >= 60)
            {
                return MissionPerformanceTier.D;
            }

            return MissionPerformanceTier.F;
        }
    }
}
