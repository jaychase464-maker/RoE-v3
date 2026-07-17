using UnityEngine;

namespace RulesOfEntry.Interaction
{
    /// <summary>
    /// Base contract for world objects that can receive a player interaction.
    /// </summary>
    public abstract class InteractableBehaviour : MonoBehaviour
    {
        public virtual Transform InteractionTransform => transform;

        public abstract InteractionPrompt GetPrompt(InteractionContext context);
        public abstract void Interact(InteractionContext context);
    }
}
