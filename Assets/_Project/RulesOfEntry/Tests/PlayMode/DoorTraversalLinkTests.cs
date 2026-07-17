using System.Collections;
using NUnit.Framework;
using RulesOfEntry.Interaction;
using RulesOfEntry.Navigation;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.TestTools;

namespace RulesOfEntry.Tests.PlayMode
{
    public sealed class DoorTraversalLinkTests
    {
        [UnityTest]
        public IEnumerator TraversalLink_ActivatesOnlyAfterDoorPhysicallyClearsThreshold()
        {
            GameObject doorObject = new GameObject("Door Traversal Test Door");
            PrototypeDoor door = doorObject.AddComponent<PrototypeDoor>();
            door.Configure(doorObject.transform, 100f);

            GameObject linkObject = new GameObject("Door Traversal Test Link");
            NavMeshLink navigationLink = linkObject.AddComponent<NavMeshLink>();
            navigationLink.startPoint = new Vector3(0f, 0f, -1f);
            navigationLink.endPoint = new Vector3(0f, 0f, 1f);
            navigationLink.width = 0.75f;
            navigationLink.bidirectional = true;
            navigationLink.activated = false;
            DoorTraversalLink traversal = linkObject.AddComponent<DoorTraversalLink>();
            traversal.Configure(door, navigationLink);

            Assert.That(door.IsTraversalClear, Is.False);
            Assert.That(traversal.TraversalActive, Is.False);

            door.Interact(new InteractionContext(null, null, Time.time));
            float timeoutAt = Time.time + 1.5f;
            while (!door.IsTraversalClear && Time.time < timeoutAt)
            {
                yield return null;
            }

            yield return null;
            Assert.That(door.IsTraversalClear, Is.True);
            Assert.That(traversal.TraversalActive, Is.True);

            door.Interact(new InteractionContext(null, null, Time.time));
            yield return null;
            Assert.That(door.IsTraversalClear, Is.False);
            Assert.That(traversal.TraversalActive, Is.False);

            Object.Destroy(doorObject);
            Object.Destroy(linkObject);
            yield return null;
        }
    }
}
