using RulesOfEntry.Interaction;
using UnityEngine;

namespace RulesOfEntry.Actors
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CustodyComponent))]
    public sealed class CustodyInteractable : InteractableBehaviour
    {
        [SerializeField] private CustodyComponent custody;
        [SerializeField] private ActorCondition condition;

        public void Configure(
            CustodyComponent configuredCustody,
            ActorCondition configuredCondition)
        {
            custody = configuredCustody;
            condition = configuredCondition;
        }

        public override InteractionPrompt GetPrompt(InteractionContext context)
        {
            ResolveReferences();
            return custody.State switch
            {
                CustodyState.Free when condition != null
                    && condition.Snapshot.Level == ActorConditionLevel.Incapacitated =>
                    InteractionPrompt.Available("Secure incapacitated subject", 1.2f),
                CustodyState.Free => InteractionPrompt.Unavailable(
                    "Restrain subject",
                    "Subject has not surrendered."),
                CustodyState.Surrendering => InteractionPrompt.Available(
                    "Order subject to kneel",
                    0.75f),
                CustodyState.Kneeling => InteractionPrompt.Available(
                    "Apply and check handcuffs",
                    2.4f),
                CustodyState.Restrained => InteractionPrompt.Available(
                    "Search person",
                    2.8f),
                CustodyState.Searched => InteractionPrompt.Available(
                    "Confirm custody",
                    1f),
                _ => InteractionPrompt.Unavailable("Person secured", "Custody is complete.")
            };
        }

        public override void Interact(InteractionContext context)
        {
            ResolveReferences();
            switch (custody.State)
            {
                case CustodyState.Free:
                    custody.TrySecureIncapacitated(context.Actor);
                    break;
                case CustodyState.Surrendering:
                    custody.TryOrderToKneel(context.Actor);
                    break;
                case CustodyState.Kneeling:
                    custody.TryApplyRestraints(context.Actor);
                    break;
                case CustodyState.Restrained:
                    custody.TrySearch(context.Actor, out _);
                    break;
                case CustodyState.Searched:
                    custody.TryTransferToCustody(context.Actor);
                    break;
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            custody ??= GetComponent<CustodyComponent>();
            condition ??= GetComponent<ActorCondition>();
        }
    }
}
