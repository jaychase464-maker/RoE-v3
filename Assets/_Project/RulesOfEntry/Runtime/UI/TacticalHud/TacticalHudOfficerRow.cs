using RulesOfEntry.Actors;
using RulesOfEntry.Officers;
using UnityEngine;
using UnityEngine.UI;

namespace RulesOfEntry.UI.TacticalHud
{
    [DisallowMultipleComponent]
    public sealed class TacticalHudOfficerRow : MonoBehaviour
    {
        private static readonly Color Blue = new Color(0.03f, 0.63f, 1f, 1f);
        private static readonly Color Green = new Color(0.29f, 1f, 0.28f, 1f);
        private static readonly Color Amber = new Color(1f, 0.66f, 0.02f, 1f);
        private static readonly Color Red = new Color(1f, 0.22f, 0.18f, 1f);
        private static readonly Color Muted = new Color(0.62f, 0.68f, 0.71f, 1f);

        [SerializeField] private Image selectionEdge;
        [SerializeField] private Text officerText;
        [SerializeField] private Text activityText;
        [SerializeField] private Text conditionDotText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text ammunitionText;

        private TacticalOfficerController officer;
        private OfficerAmmunitionStatus ammunition;
        private int rosterIndex;

        public bool HasCompleteVisualConfiguration => selectionEdge != null
            && officerText != null
            && activityText != null
            && conditionDotText != null
            && healthText != null
            && ammunitionText != null;

        public void ConfigureVisuals(
            Image configuredSelectionEdge,
            Text configuredOfficerText,
            Text configuredActivityText,
            Text configuredConditionDotText,
            Text configuredHealthText,
            Text configuredAmmunitionText)
        {
            selectionEdge = configuredSelectionEdge;
            officerText = configuredOfficerText;
            activityText = configuredActivityText;
            conditionDotText = configuredConditionDotText;
            healthText = configuredHealthText;
            ammunitionText = configuredAmmunitionText;
        }

        public void Bind(TacticalOfficerController configuredOfficer, int configuredRosterIndex)
        {
            officer = configuredOfficer;
            rosterIndex = Mathf.Max(0, configuredRosterIndex);
            ammunition = officer != null
                ? officer.GetComponent<OfficerAmmunitionStatus>()
                : null;
            Refresh();
        }

        public void Refresh()
        {
            if (officerText != null)
            {
                string displayName = officer?.Identity != null
                    ? officer.Identity.DisplayName
                    : "OFFICER UNAVAILABLE";
                officerText.text = $"{rosterIndex + 1:00}  {displayName.ToUpperInvariant()}";
                officerText.color = officer != null && officer.IsSelected ? Blue : Color.white;
            }

            if (activityText != null)
            {
                activityText.text = "ORDER  " + TacticalHudRules.GetActivityLabel(officer);
                activityText.color = officer != null && officer.IsSelected ? Blue : Muted;
            }

            ActorConditionLevel condition = officer?.Condition != null
                ? officer.Condition.Snapshot.Level
                : ActorConditionLevel.Incapacitated;
            if (conditionDotText != null)
            {
                conditionDotText.text = "●";
                conditionDotText.color = GetConditionColor(condition);
            }

            if (healthText != null)
            {
                healthText.text = TacticalHudRules.GetConditionLabel(condition);
                healthText.color = Color.white;
            }

            if (ammunitionText != null)
            {
                if (ammunition == null)
                {
                    ammunitionText.text = "UNKNOWN";
                    ammunitionText.color = Muted;
                }
                else
                {
                    ammunitionText.text = "AMMO  " + TacticalHudRules.GetAmmunitionLabel(
                            ammunition.Condition);
                    ammunitionText.color = GetAmmunitionColor(ammunition.Condition);
                }
            }

            if (selectionEdge != null)
            {
                selectionEdge.color = officer != null && officer.IsSelected
                    ? Blue
                    : Color.clear;
            }
        }

        private static Color GetConditionColor(ActorConditionLevel condition)
        {
            return condition switch
            {
                ActorConditionLevel.Stable => Green,
                ActorConditionLevel.Wounded => Amber,
                ActorConditionLevel.Incapacitated => Red,
                ActorConditionLevel.Deceased => Red,
                _ => Muted
            };
        }

        private static Color GetAmmunitionColor(OfficerAmmunitionCondition condition)
        {
            return condition switch
            {
                OfficerAmmunitionCondition.Good => Green,
                OfficerAmmunitionCondition.Low => Amber,
                OfficerAmmunitionCondition.Critical => Red,
                _ => Muted
            };
        }
    }
}
