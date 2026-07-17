using UnityEngine;

namespace RulesOfEntry.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(GameObject actor, Transform viewTransform, float time)
        {
            Actor = actor;
            ViewTransform = viewTransform;
            Time = time;
        }

        public GameObject Actor { get; }
        public Transform ViewTransform { get; }
        public float Time { get; }
    }
}
