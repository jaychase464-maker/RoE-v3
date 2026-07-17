using System;
using System.Text;
using RulesOfEntry.AI;
using RulesOfEntry.Input;
using UnityEngine;
using UnityEngine.UI;

namespace RulesOfEntry.UI
{
    /// <summary>
    /// Prototype-only diagnostics. This exposes decision reasons for validation and is not final HUD.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HumanBehaviorDebugUI : MonoBehaviour
    {
        [SerializeField] private VerbalCommandEmitter commandEmitter;
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private HumanActorController[] actors = Array.Empty<HumanActorController>();
        [SerializeField] private Text commandText;
        [SerializeField] private Text actorStateText;

        private readonly StringBuilder builder = new StringBuilder(256);

        public void ConfigureSources(
            VerbalCommandEmitter configuredEmitter,
            TacticalPlayerInput configuredInput,
            HumanActorController[] configuredActors)
        {
            commandEmitter = configuredEmitter;
            playerInput = configuredInput;
            actors = configuredActors ?? Array.Empty<HumanActorController>();
        }

        public void ConfigureVisuals(Text configuredCommandText, Text configuredActorStateText)
        {
            commandText = configuredCommandText;
            actorStateText = configuredActorStateText;
        }

        private void Update()
        {
            if (commandText != null)
            {
                string binding = playerInput != null
                    ? playerInput.GetIssueCommandBindingDisplayString()
                    : "F";
                string lastCommand = commandEmitter != null
                    && !string.IsNullOrWhiteSpace(commandEmitter.LastCommandText)
                    ? $"\nLast: {commandEmitter.LastCommandText} • evaluated {commandEmitter.LastRecipientCount} • surrendered {commandEmitter.LastSurrenderCount}"
                    : string.Empty;
                commandText.text = $"AI DIAGNOSTICS • [{binding}] Police command{lastCommand}";
            }

            if (actorStateText == null)
            {
                return;
            }

            builder.Clear();
            foreach (HumanActorController actor in actors ?? Array.Empty<HumanActorController>())
            {
                if (actor == null || actor.Identity == null)
                {
                    continue;
                }

                builder.Append(actor.Identity.DisplayName)
                    .Append(" • ")
                    .Append(actor.State)
                    .Append(" • custody ")
                    .Append(actor.Custody != null ? actor.Custody.State.ToString() : "Unknown")
                    .Append(" • reason ")
                    .Append(actor.LastDecisionReason)
                    .AppendLine();
            }

            actorStateText.text = builder.ToString();
        }
    }
}
