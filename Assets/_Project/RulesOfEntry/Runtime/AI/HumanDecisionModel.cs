using RulesOfEntry.Actors;
using UnityEngine;

namespace RulesOfEntry.AI
{
    public static class HumanDecisionModel
    {
        public static CommandDecision EvaluateCommand(
            CommandDecisionContext context,
            float decisionRoll,
            float deceptionRoll,
            float responseRoll)
        {
            if (!context.Perceived)
            {
                return NoResponse(HumanDecisionReason.CommandNotPerceived, decisionRoll);
            }

            if (context.Restrained)
            {
                return new CommandDecision(
                    HumanBehaviorState.Restrained,
                    HumanDecisionReason.AlreadyRestrained,
                    false,
                    1f,
                    decisionRoll);
            }

            if (!context.CanAct)
            {
                return new CommandDecision(
                    HumanBehaviorState.Incapacitated,
                    HumanDecisionReason.PhysicallyUnable,
                    false,
                    1f,
                    decisionRoll);
            }

            HumanBehaviorProfile profile = context.Profile;
            if (profile == null)
            {
                return NoResponse(HumanDecisionReason.RefusedCommand, decisionRoll);
            }

            float injuryPressure = context.ConditionLevel == ActorConditionLevel.Wounded
                ? 0.16f
                : 0f;
            float authorityPressure = context.WeaponPresented ? 0.13f : 0f;
            float comprehension = profile.CommandComprehension * 0.22f;
            float distancePenalty = Mathf.InverseLerp(4f, 18f, context.DistanceMeters) * 0.13f;
            float complianceScore = profile.BaselineCompliance
                + comprehension
                + authorityPressure
                + injuryPressure
                + (1f - context.Morale) * 0.28f
                - profile.Aggression * 0.3f
                - distancePenalty;

            if (context.Role == ActorRole.Civilian)
            {
                complianceScore += 0.18f - context.Stress * 0.22f;
            }
            else if (context.ActorHasWeapon)
            {
                complianceScore -= 0.08f;
            }

            complianceScore = Mathf.Clamp01(complianceScore);
            if (decisionRoll <= complianceScore)
            {
                bool deceptive = context.Role == ActorRole.Suspect
                    && deceptionRoll < profile.Deception
                    && context.ConditionLevel != ActorConditionLevel.Wounded;
                HumanDecisionReason reason = deceptive
                    ? HumanDecisionReason.DeceptiveCompliance
                    : context.Morale < 0.38f
                        ? HumanDecisionReason.LowMorale
                        : context.WeaponPresented
                            ? HumanDecisionReason.OfficerAdvantage
                            : HumanDecisionReason.CommandUnderstood;
                return new CommandDecision(
                    HumanBehaviorState.Surrendering,
                    reason,
                    deceptive,
                    complianceScore,
                    decisionRoll);
            }

            float panic = Mathf.Clamp01(context.Stress + profile.FlightTendency * 0.35f);
            if (context.Role == ActorRole.Civilian && panic >= 0.62f)
            {
                bool freeze = responseRoll > profile.FlightTendency;
                return new CommandDecision(
                    freeze ? HumanBehaviorState.Frozen : HumanBehaviorState.Fleeing,
                    freeze ? HumanDecisionReason.FreezeResponse : HumanDecisionReason.HighPanic,
                    false,
                    complianceScore,
                    decisionRoll);
            }

            if (responseRoll < profile.FlightTendency)
            {
                HumanBehaviorState escapeState = responseRoll < profile.HideTendency
                    ? HumanBehaviorState.Hiding
                    : HumanBehaviorState.Fleeing;
                return new CommandDecision(
                    escapeState,
                    HumanDecisionReason.EscapeOpportunity,
                    false,
                    complianceScore,
                    decisionRoll);
            }

            if (context.Role == ActorRole.Suspect && profile.Aggression > responseRoll)
            {
                return new CommandDecision(
                    HumanBehaviorState.Threatening,
                    HumanDecisionReason.HostileIntent,
                    false,
                    complianceScore,
                    decisionRoll);
            }

            return new CommandDecision(
                HumanBehaviorState.Resisting,
                HumanDecisionReason.RefusedCommand,
                false,
                complianceScore,
                decisionRoll);
        }

        private static CommandDecision NoResponse(
            HumanDecisionReason reason,
            float decisionRoll)
        {
            return new CommandDecision(
                HumanBehaviorState.Idle,
                reason,
                false,
                0f,
                decisionRoll);
        }
    }
}
