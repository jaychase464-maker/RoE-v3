using System;
using System.Globalization;
using RulesOfEntry.Actors;
using RulesOfEntry.Officers;

namespace RulesOfEntry.UI.TacticalHud
{
    public static class TacticalHudRules
    {
        public static string GetConditionLabel(ActorConditionLevel condition)
        {
            return condition switch
            {
                ActorConditionLevel.Stable => "FIT",
                ActorConditionLevel.Wounded => "WOUNDED",
                ActorConditionLevel.Incapacitated => "DOWN",
                ActorConditionLevel.Deceased => "DECEASED",
                _ => "UNKNOWN"
            };
        }

        public static string GetAmmunitionLabel(OfficerAmmunitionCondition condition)
        {
            return condition switch
            {
                OfficerAmmunitionCondition.Good => "GOOD",
                OfficerAmmunitionCondition.Low => "LOW",
                OfficerAmmunitionCondition.Critical => "CRITICAL",
                _ => "UNKNOWN"
            };
        }

        public static string GetActivityLabel(TacticalOfficerController officer)
        {
            if (officer == null)
            {
                return "UNAVAILABLE";
            }

            if (officer.CurrentOrder == null || !officer.HasActiveOrder)
            {
                return string.IsNullOrWhiteSpace(officer.Activity)
                    ? "STANDING BY"
                    : officer.Activity.Trim().ToUpperInvariant();
            }

            return officer.CurrentOrder.Type switch
            {
                OfficerOrderType.MoveTo => "MOVING",
                OfficerOrderType.HoldPosition => "HOLDING",
                OfficerOrderType.Follow => "FOLLOWING",
                OfficerOrderType.StackAtDoor => "STACKING",
                OfficerOrderType.OpenDoor => "OPEN / CLEAR",
                OfficerOrderType.RestrainSubject => "RESTRAINING",
                _ => officer.CurrentOrder.Type.ToString().ToUpperInvariant()
            };
        }

        public static string FormatBodyCameraTimestamp(DateTime timestamp)
        {
            return timestamp.ToString(
                "dd MMM yyyy   HH:mm:ss",
                CultureInfo.InvariantCulture).ToUpperInvariant();
        }

        public static int GetSuggestedCommandIndex(OfficerCommandTargetType targetType)
        {
            return targetType switch
            {
                OfficerCommandTargetType.Door => 2,
                OfficerCommandTargetType.Subject => 5,
                OfficerCommandTargetType.Position => 0,
                _ => -1
            };
        }
    }
}
