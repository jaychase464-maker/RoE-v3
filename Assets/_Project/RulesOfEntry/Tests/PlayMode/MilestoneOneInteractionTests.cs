using System.Collections;
using NUnit.Framework;
using RulesOfEntry.Interaction;
using UnityEngine;
using UnityEngine.TestTools;

namespace RulesOfEntry.Tests.PlayMode
{
    public sealed class MilestoneOneInteractionTests
    {
        [UnityTest]
        public IEnumerator PrototypeDoor_TogglesOpenStateAndPrompt()
        {
            GameObject doorObject = new GameObject("Door Test");
            PrototypeDoor door = doorObject.AddComponent<PrototypeDoor>();
            door.Configure(doorObject.transform, 90f);
            InteractionContext context = new InteractionContext(
                doorObject,
                doorObject.transform,
                Time.time);

            Assert.That(door.IsOpen, Is.False);
            Assert.That(door.GetPrompt(context).ActionText, Is.EqualTo("Open Door"));

            door.Interact(context);
            yield return null;

            Assert.That(door.IsOpen, Is.True);
            Assert.That(door.GetPrompt(context).ActionText, Is.EqualTo("Close Door"));

            Object.Destroy(doorObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PrototypeControlPanel_TogglesStateAndUsesHoldPrompt()
        {
            GameObject panelObject = new GameObject("Panel Test");
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicator.transform.SetParent(panelObject.transform, false);
            PrototypeControlPanel panel = panelObject.AddComponent<PrototypeControlPanel>();
            panel.Configure(indicator.GetComponent<Renderer>());
            InteractionContext context = new InteractionContext(
                panelObject,
                panelObject.transform,
                Time.time);

            InteractionPrompt prompt = panel.GetPrompt(context);
            Assert.That(panel.IsActive, Is.False);
            Assert.That(prompt.ActionText, Is.EqualTo("Activate Training Panel"));
            Assert.That(prompt.HoldDuration, Is.GreaterThan(0f));

            panel.Interact(context);
            yield return null;

            Assert.That(panel.IsActive, Is.True);
            Assert.That(
                panel.GetPrompt(context).ActionText,
                Is.EqualTo("Deactivate Training Panel"));

            Object.Destroy(panelObject);
            yield return null;
        }
    }
}
