using System.Collections;
using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Combat;
using UnityEngine;
using UnityEngine.TestTools;

namespace RulesOfEntry.Tests.PlayMode
{
    public sealed class ActorConditionTests
    {
        [UnityTest]
        public IEnumerator HighEnergyHeadImpact_IncapacitatesWithoutHealthHudContract()
        {
            GameObject actor = new GameObject("Condition Test Actor");
            ActorCondition condition = actor.AddComponent<ActorCondition>();
            GameObject hitRegionObject = new GameObject("Head Hit Region");
            hitRegionObject.transform.SetParent(actor.transform);
            SphereCollider collider = hitRegionObject.AddComponent<SphereCollider>();
            hitRegionObject.AddComponent<ActorHitRegion>().Configure(ActorHitRegionType.Head);
            BallisticHit hit = new BallisticHit(
                null,
                "test_firearm",
                "test_ammunition",
                Vector3.zero,
                Vector3.forward,
                Vector3.zero,
                Vector3.back,
                1800f,
                collider);

            condition.ReceiveBallisticHit(hit);

            Assert.That(
                condition.Snapshot.Level,
                Is.EqualTo(ActorConditionLevel.Incapacitated));
            Assert.That(condition.Snapshot.CanAct, Is.False);
            Object.Destroy(actor);
            yield return null;
        }
    }
}
