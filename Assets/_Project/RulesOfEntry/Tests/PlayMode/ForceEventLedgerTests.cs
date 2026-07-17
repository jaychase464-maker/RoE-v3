using System.Collections;
using NUnit.Framework;
using RulesOfEntry.Combat;
using UnityEngine;
using UnityEngine.TestTools;

namespace RulesOfEntry.Tests.PlayMode
{
    public sealed class ForceEventLedgerTests
    {
        [UnityTest]
        public IEnumerator FirearmDischarge_IsRecordedAsImmutableSequentialFact()
        {
            GameObject shooter = new GameObject("Force Event Test Shooter");
            UseOfForceEventLedger ledger = shooter.AddComponent<UseOfForceEventLedger>();
            FirearmDefinition firearm = ScriptableObject.CreateInstance<FirearmDefinition>();
            firearm.Configure("test_carbine", "Test Carbine", 30, 0.12f, 2.3f, 1.8f, 1.2f, 0.8f, 0.28f, 1f, 0.2f);
            AmmunitionDefinition ammunition = ScriptableObject.CreateInstance<AmmunitionDefinition>();
            ammunition.Configure("test_ammunition", "Test Ammunition", 62f, 850f, 500f);
            FirearmSnapshot snapshot = new FirearmSnapshot(
                FireSelectorPosition.SemiAutomatic,
                true,
                false,
                true,
                28,
                3,
                0,
                0);

            ForceEventRecord first = ledger.RecordFirearmDischarge(
                shooter,
                firearm,
                ammunition,
                WeaponReadyPosition.Shouldered,
                Vector3.zero,
                Vector3.forward,
                null,
                snapshot);
            ForceEventRecord second = ledger.RecordFirearmDischarge(
                shooter,
                firearm,
                ammunition,
                WeaponReadyPosition.Shouldered,
                Vector3.zero,
                Vector3.forward,
                null,
                snapshot);

            Assert.That(ledger.Records.Count, Is.EqualTo(2));
            Assert.That(first.Sequence, Is.EqualTo(1));
            Assert.That(second.Sequence, Is.EqualTo(2));
            Assert.That(first.FirearmId, Is.EqualTo("test_carbine"));
            Assert.That(first.ShooterEntityId, Is.EqualTo(EntityId.ToULong(shooter.GetEntityId())));
            Assert.That(first.HitColliderEntityId, Is.Zero);
            Assert.That(first.Hit, Is.False);

            Object.Destroy(shooter);
            Object.Destroy(firearm);
            Object.Destroy(ammunition);
            yield return null;
        }
    }
}
