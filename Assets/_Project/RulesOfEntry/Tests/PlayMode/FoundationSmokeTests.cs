using System.Collections;
using NUnit.Framework;
using RulesOfEntry.Core;
using RulesOfEntry.World;
using UnityEngine;
using UnityEngine.TestTools;

namespace RulesOfEntry.Tests.PlayMode
{
    public sealed class FoundationSmokeTests
    {
        [UnityTest]
        public IEnumerator SceneFoundationMarker_CanBeCreatedAtRuntime()
        {
            GameObject testObject = new GameObject("Foundation Smoke Test");
            SceneFoundationMarker marker = testObject.AddComponent<SceneFoundationMarker>();

            Assert.That(marker.SceneId, Is.EqualTo("roe_prototype"));
            Assert.That(marker.Purpose, Is.EqualTo(ScenePurpose.Prototype));
            Assert.That(marker.SchemaVersion, Is.EqualTo(ProjectInfo.FoundationSchemaVersion));

            Object.Destroy(testObject);
            yield return null;

            Assert.That(testObject == null, Is.True);
        }
    }
}
