using System.Collections;
using NUnit.Framework;
using RulesOfEntry.Actors;
using UnityEngine;
using UnityEngine.TestTools;

namespace RulesOfEntry.Tests.PlayMode
{
    public sealed class CustodyPipelineTests
    {
        [UnityTest]
        public IEnumerator CooperativeArrest_RecordsCompleteExactlyOrderedCustodyHistory()
        {
            GameObject actor = new GameObject("Custody Test Actor");
            ActorIdentity identity = actor.AddComponent<ActorIdentity>();
            identity.Configure("custody_test_actor", "Custody Test Actor", ActorRole.Suspect, 77);
            ActorInventory inventory = actor.AddComponent<ActorInventory>();
            inventory.Configure(true, new[] { "Test identification" });
            ActorCondition condition = actor.AddComponent<ActorCondition>();
            CustodyEventLedger ledger = actor.AddComponent<CustodyEventLedger>();
            CustodyComponent custody = actor.AddComponent<CustodyComponent>();
            custody.Configure(identity, inventory, condition, ledger);
            GameObject officer = new GameObject("Custody Test Officer");

            Assert.That(custody.TryBeginSurrender(officer, "Test command accepted."), Is.True);
            Assert.That(custody.TryOrderToKneel(officer), Is.True);
            Assert.That(custody.TryApplyRestraints(officer), Is.True);
            Assert.That(custody.TrySearch(officer, out ActorSearchResult search), Is.True);
            Assert.That(search.WeaponFound, Is.True);
            Assert.That(custody.TryTransferToCustody(officer), Is.True);

            Assert.That(custody.State, Is.EqualTo(CustodyState.InCustody));
            Assert.That(inventory.WeaponSecured, Is.True);
            Assert.That(ledger.Records.Count, Is.EqualTo(5));
            Assert.That(ledger.Records[0].Action, Is.EqualTo(CustodyAction.BeginSurrender));
            Assert.That(ledger.Records[4].Action, Is.EqualTo(CustodyAction.TransferCustody));

            Object.Destroy(actor);
            Object.Destroy(officer);
            yield return null;
        }
    }
}
