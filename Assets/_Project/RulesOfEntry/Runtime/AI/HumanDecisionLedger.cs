using System;
using System.Collections.Generic;
using RulesOfEntry.Actors;
using UnityEngine;

namespace RulesOfEntry.AI
{
    [DisallowMultipleComponent]
    public sealed class HumanDecisionLedger : MonoBehaviour
    {
        private readonly List<HumanDecisionRecord> records = new List<HumanDecisionRecord>();
        private long nextSequence = 1;

        public event Action<HumanDecisionRecord> RecordAdded;

        public IReadOnlyList<HumanDecisionRecord> Records => records.AsReadOnly();

        public HumanDecisionRecord Record(
            VerbalCommandType command,
            HumanBehaviorState previousState,
            CommandDecision decision,
            float stress,
            float morale)
        {
            HumanDecisionRecord record = new HumanDecisionRecord(
                nextSequence++,
                Time.timeAsDouble,
                command,
                previousState,
                decision.State,
                decision.Reason,
                decision.Deceptive,
                decision.ComplianceScore,
                decision.DecisionRoll,
                stress,
                morale);
            records.Add(record);
            RecordAdded?.Invoke(record);
            return record;
        }

        public HumanDecisionRecord RecordStateChange(
            HumanBehaviorState previousState,
            HumanBehaviorState newState,
            HumanDecisionReason reason,
            float stress,
            float morale)
        {
            CommandDecision decision = new CommandDecision(
                newState,
                reason,
                false,
                0f,
                0f);
            return Record(
                VerbalCommandType.PoliceShowHands,
                previousState,
                decision,
                stress,
                morale);
        }
    }
}
