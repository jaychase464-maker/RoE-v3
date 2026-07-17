using System.Collections.Generic;
using RulesOfEntry.Actors;
using RulesOfEntry.Combat;
using RulesOfEntry.Core;
using RulesOfEntry.Input;
using UnityEngine;

namespace RulesOfEntry.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TacticalPlayerInput))]
    public sealed class VerbalCommandEmitter : MonoBehaviour
    {
        private const int MaximumCommandColliders = 64;

        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private Transform voiceOrigin;
        [SerializeField] private FirearmController firearmController;
        [SerializeField, Min(2f)] private float commandRadius = 18f;
        [SerializeField, Min(0.1f)] private float commandCooldownSeconds = 0.8f;
        [SerializeField] private LayerMask receiverMask = ~0;

        private readonly Collider[] overlapResults = new Collider[MaximumCommandColliders];
        private readonly HashSet<HumanActorController> uniqueReceivers =
            new HashSet<HumanActorController>();
        private float cooldownRemaining;

        public int CommandsIssued { get; private set; }
        public int LastRecipientCount { get; private set; }
        public int LastSurrenderCount { get; private set; }
        public string LastCommandText { get; private set; } = string.Empty;

        public void Configure(
            TacticalPlayerInput configuredInput,
            Transform configuredVoiceOrigin,
            FirearmController configuredFirearmController,
            float configuredCommandRadius,
            LayerMask configuredReceiverMask)
        {
            playerInput = configuredInput;
            voiceOrigin = configuredVoiceOrigin;
            firearmController = configuredFirearmController;
            commandRadius = Mathf.Max(2f, configuredCommandRadius);
            receiverMask = configuredReceiverMask;
        }

        public void IssueCommand(VerbalCommandType command)
        {
            if (cooldownRemaining > 0f)
            {
                return;
            }

            Vector3 origin = voiceOrigin != null
                ? voiceOrigin.position
                : transform.position + Vector3.up * 1.6f;
            bool weaponPresented = firearmController != null
                && firearmController.WeaponIsRaised
                && firearmController.EffectiveReadyPosition == WeaponReadyPosition.Shouldered;
            VerbalCommandStimulus stimulus = new VerbalCommandStimulus(
                gameObject,
                command,
                origin,
                Time.time,
                commandRadius,
                weaponPresented);

            int colliderCount = Physics.OverlapSphereNonAlloc(
                origin,
                commandRadius,
                overlapResults,
                receiverMask,
                QueryTriggerInteraction.Collide);
            uniqueReceivers.Clear();
            for (int index = 0; index < colliderCount; index++)
            {
                Collider collider = overlapResults[index];
                HumanActorController receiver = collider != null
                    ? collider.GetComponentInParent<HumanActorController>()
                    : null;
                if (receiver != null)
                {
                    uniqueReceivers.Add(receiver);
                }
            }

            int surrenderCount = 0;
            foreach (HumanActorController receiver in uniqueReceivers)
            {
                HumanDecisionRecord decision = receiver.ReceiveVerbalCommand(stimulus);
                if (decision != null
                    && decision.NewState == HumanBehaviorState.Surrendering)
                {
                    surrenderCount++;
                }
            }

            CommandsIssued++;
            LastRecipientCount = uniqueReceivers.Count;
            LastSurrenderCount = surrenderCount;
            LastCommandText = GetSpokenCommand(command);
            cooldownRemaining = commandCooldownSeconds;
            ProjectLog.Info(
                "Verbal Command",
                $"{LastCommandText} Heard/evaluated by {LastRecipientCount} subject(s).",
                this);
        }

        private void Awake()
        {
            playerInput ??= GetComponent<TacticalPlayerInput>();
            firearmController ??= GetComponent<FirearmController>();
            if (voiceOrigin == null)
            {
                Camera camera = GetComponentInChildren<Camera>(true);
                voiceOrigin = camera != null ? camera.transform : transform;
            }
        }

        private void Update()
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
            if (playerInput != null
                && playerInput.GameplayEnabled
                && Cursor.lockState == CursorLockMode.Locked
                && playerInput.IssueCommandPressedThisFrame)
            {
                IssueCommand(VerbalCommandType.PoliceShowHands);
            }
        }

        private static string GetSpokenCommand(VerbalCommandType command)
        {
            return command switch
            {
                VerbalCommandType.GetDown => "POLICE! GET DOWN!",
                VerbalCommandType.DropWeapon => "POLICE! DROP THE WEAPON!",
                VerbalCommandType.Stop => "POLICE! STOP!",
                VerbalCommandType.DoNotMove => "POLICE! DO NOT MOVE!",
                _ => "POLICE! SHOW ME YOUR HANDS!"
            };
        }
    }
}
