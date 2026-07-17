using System.Text;
using RulesOfEntry.Input;
using RulesOfEntry.Officers;
using UnityEngine;
using UnityEngine.UI;

namespace RulesOfEntry.UI
{
    /// <summary>
    /// Prototype-only command diagnostics. Final diegetic command UI is outside Milestone 4.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OfficerCommandDebugUI : MonoBehaviour
    {
        [SerializeField] private OfficerSquadController squad;
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private Text commandText;
        [SerializeField] private Text officerStateText;

        private readonly StringBuilder builder = new StringBuilder(384);

        public void ConfigureSources(
            OfficerSquadController configuredSquad,
            TacticalPlayerInput configuredInput)
        {
            squad = configuredSquad;
            playerInput = configuredInput;
        }

        public void ConfigureVisuals(Text configuredCommandText, Text configuredOfficerStateText)
        {
            commandText = configuredCommandText;
            officerStateText = configuredOfficerStateText;
        }

        private void Update()
        {
            if (commandText != null)
            {
                string context = playerInput != null
                    ? playerInput.GetOfficerContextBindingDisplayString()
                    : "G";
                string selection = playerInput != null
                    ? playerInput.GetOfficerSelectionBindingDisplayString()
                    : "1 / 2 / 3";
                commandText.text = squad == null
                    ? "OFFICER COMMAND • squad unavailable"
                    : $"OFFICER COMMAND • selected {squad.Selection}\n"
                        + $"[{selection}] Select • [{context}] Context order • Z Cancel\n"
                        + squad.LastCommandSummary;
            }

            if (officerStateText == null)
            {
                return;
            }

            builder.Clear();
            if (squad != null)
            {
                foreach (TacticalOfficerController officer in squad.Officers)
                {
                    if (officer == null || officer.Identity == null)
                    {
                        continue;
                    }

                    builder.Append(officer.IsSelected ? "> " : "  ")
                        .Append(officer.Identity.DisplayName)
                        .Append(" • ")
                        .Append(officer.CurrentOrder != null
                            ? officer.CurrentOrder.Type.ToString()
                            : "NoOrder")
                        .Append(officer.CurrentOrder != null
                            && officer.CurrentOrder.Origin == OfficerOrderOrigin.OfficerInitiative
                                ? " [INITIATIVE]"
                                : string.Empty)
                        .Append(" • ")
                        .Append(officer.CurrentStatus);
                    if (officer.CurrentOutcomeReason != OfficerOrderOutcomeReason.None)
                    {
                        builder.Append(" • ").Append(officer.CurrentOutcomeReason);
                    }

                    builder.Append("\n    ").Append(officer.CurrentDetails);
                    OfficerInitiativeController initiative =
                        officer.GetComponent<OfficerInitiativeController>();
                    if (initiative != null)
                    {
                        builder.Append("\n    Initiative: ").Append(initiative.LastActivity);
                    }

                    builder.AppendLine();
                }
            }

            builder.Append("Keys: G move/context • H hold • J follow • Y stack • U open • K restrain");
            officerStateText.text = builder.ToString();
        }
    }
}
