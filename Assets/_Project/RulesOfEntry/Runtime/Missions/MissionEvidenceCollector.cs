using System;
using System.Linq;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Combat;
using RulesOfEntry.Officers;
using UnityEngine;

namespace RulesOfEntry.Missions
{
    /// <summary>
    /// Read-only scene adapter. It copies authoritative state and ledger facts into an
    /// immutable snapshot; it never changes the systems it observes.
    /// </summary>
    public static class MissionEvidenceCollector
    {
        public static MissionEvidenceSnapshot Capture(double missionStartedAtSeconds)
        {
            double capturedAt = Time.timeAsDouble;
            ActorEvidenceSnapshot[] actors = UnityEngine.Object.FindObjectsByType<ActorIdentity>(
                    FindObjectsSortMode.None)
                .Where(identity => identity != null)
                .Select(CaptureActor)
                .OrderBy(actor => actor.ActorId, StringComparer.Ordinal)
                .ToArray();
            RoomEvidenceSnapshot[] rooms = UnityEngine.Object.FindObjectsByType<TacticalRoomVolume>(
                    FindObjectsSortMode.None)
                .Where(room => room != null)
                .Select(room => new RoomEvidenceSnapshot(
                    room.RoomId,
                    room.State,
                    room.ActiveThreatCount,
                    room.ActionableOfficerCount))
                .OrderBy(room => room.RoomId, StringComparer.Ordinal)
                .ToArray();
            ForceEventRecord[] forceEvents = UnityEngine.Object.FindObjectsByType<UseOfForceEventLedger>(
                    FindObjectsSortMode.None)
                .SelectMany(ledger => ledger.Records)
                .OrderBy(record => record.OccurredAtSeconds)
                .ThenBy(record => record.ShooterEntityId)
                .ThenBy(record => record.Sequence)
                .ToArray();
            CustodyEventRecord[] custodyEvents = UnityEngine.Object.FindObjectsByType<CustodyEventLedger>(
                    FindObjectsSortMode.None)
                .SelectMany(ledger => ledger.Records)
                .OrderBy(record => record.OccurredAtSeconds)
                .ThenBy(record => record.ActorId, StringComparer.Ordinal)
                .ThenBy(record => record.Sequence)
                .ToArray();
            OfficerOrderEventRecord[] orderEvents =
                UnityEngine.Object.FindObjectsByType<OfficerOrderLedger>(FindObjectsSortMode.None)
                    .SelectMany(ledger => ledger.Records)
                    .OrderBy(record => record.OccurredAtSeconds)
                    .ThenBy(record => record.OfficerActorId, StringComparer.Ordinal)
                    .ThenBy(record => record.LedgerSequence)
                    .ToArray();
            OfficerInitiativeRecord[] initiativeEvents =
                UnityEngine.Object.FindObjectsByType<OfficerInitiativeLedger>(FindObjectsSortMode.None)
                    .SelectMany(ledger => ledger.Records)
                    .OrderBy(record => record.OccurredAtSeconds)
                    .ThenBy(record => record.OfficerActorId, StringComparer.Ordinal)
                    .ThenBy(record => record.Sequence)
                    .ToArray();

            return new MissionEvidenceSnapshot(
                capturedAt,
                Math.Max(0d, capturedAt - missionStartedAtSeconds),
                actors,
                rooms,
                forceEvents,
                custodyEvents,
                orderEvents,
                initiativeEvents);
        }

        private static ActorEvidenceSnapshot CaptureActor(ActorIdentity identity)
        {
            ActorCondition condition = identity.GetComponent<ActorCondition>();
            CustodyComponent custody = identity.GetComponent<CustodyComponent>();
            HumanActorController behavior = identity.GetComponent<HumanActorController>();
            ActorInventory inventory = identity.GetComponent<ActorInventory>();
            return new ActorEvidenceSnapshot(
                identity.ActorId,
                identity.RuntimeEntityId,
                identity.Role,
                condition != null
                    ? condition.Snapshot.Level
                    : ActorConditionLevel.Stable,
                custody != null ? custody.State : CustodyState.Free,
                behavior != null ? behavior.State : HumanBehaviorState.Idle,
                inventory != null && inventory.HasWeapon);
        }
    }
}
