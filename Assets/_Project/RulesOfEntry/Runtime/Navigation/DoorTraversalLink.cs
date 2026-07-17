using RulesOfEntry.Core;
using RulesOfEntry.Interaction;
using Unity.AI.Navigation;
using UnityEngine;

namespace RulesOfEntry.Navigation
{
    /// <summary>
    /// Gates a fixed NavMesh connection with the physical clearance of a moving door.
    /// The link is never parented to the rotating door leaf.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshLink))]
    public sealed class DoorTraversalLink : MonoBehaviour
    {
        [SerializeField] private PrototypeDoor door;
        [SerializeField] private NavMeshLink navigationLink;

        private bool stateInitialized;
        private bool lastTraversalState;

        public PrototypeDoor Door => door;
        public NavMeshLink NavigationLink => navigationLink;
        public bool HasCompleteConfiguration => door != null && navigationLink != null;
        public bool TraversalActive => navigationLink != null && navigationLink.activated;

        public void Configure(
            PrototypeDoor configuredDoor,
            NavMeshLink configuredLink)
        {
            door = configuredDoor;
            navigationLink = configuredLink;
            ApplyTraversalState();
        }

        private void Awake()
        {
            navigationLink ??= GetComponent<NavMeshLink>();
            ApplyTraversalState();
        }

        private void Update()
        {
            ApplyTraversalState();
        }

        private void OnDisable()
        {
            if (navigationLink != null)
            {
                navigationLink.activated = false;
            }

            stateInitialized = false;
        }

        private void ApplyTraversalState()
        {
            if (navigationLink == null)
            {
                return;
            }

            bool shouldAllowTraversal = door != null && door.IsTraversalClear;
            navigationLink.activated = shouldAllowTraversal;
            if (stateInitialized && shouldAllowTraversal == lastTraversalState)
            {
                return;
            }

            stateInitialized = true;
            lastTraversalState = shouldAllowTraversal;
            ProjectLog.Development(
                "Door Navigation",
                $"{name} traversal is {(shouldAllowTraversal ? "available" : "blocked")}.",
                this);
        }
    }
}
