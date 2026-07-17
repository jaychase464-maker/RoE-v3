using System.Collections;
using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Officers;
using UnityEngine;
using UnityEngine.TestTools;

namespace RulesOfEntry.Tests.PlayMode
{
    public sealed class OfficerOrderLedgerTests
    {
        [UnityTest]
        public IEnumerator OrderHistory_RecordsImmutableLifecycleFactsInSequence()
        {
            GameObject officer = new GameObject("Order Ledger Test Officer");
            ActorIdentity identity = officer.AddComponent<ActorIdentity>();
            identity.Configure(
                "order_ledger_test_officer",
                "Order Ledger Test Officer",
                ActorRole.Officer,
                44001);
            OfficerOrderLedger ledger = officer.AddComponent<OfficerOrderLedger>();
            ledger.Configure(identity);
            OfficerOrder order = new OfficerOrder(
                21,
                9001UL,
                identity.RuntimeEntityId,
                OfficerOrderType.MoveTo,
                new Vector3(2f, 0f, 4f),
                null,
                0UL,
                Time.timeAsDouble);
            OfficerOrderStateMachine state = new OfficerOrderStateMachine();

            ledger.Record(order, state);
            Assert.That(state.TryAccept("Acknowledged."), Is.True);
            ledger.Record(order, state);
            Assert.That(state.TryBeginExecution("Moving."), Is.True);
            ledger.Record(order, state);
            Assert.That(state.TryComplete("Arrived."), Is.True);
            ledger.Record(order, state);

            Assert.That(ledger.Records.Count, Is.EqualTo(4));
            Assert.That(ledger.Records[0].LedgerSequence, Is.EqualTo(1));
            Assert.That(ledger.Records[3].LedgerSequence, Is.EqualTo(4));
            Assert.That(ledger.Records[0].Status, Is.EqualTo(OfficerOrderStatus.Pending));
            Assert.That(ledger.Records[3].Status, Is.EqualTo(OfficerOrderStatus.Completed));
            Assert.That(ledger.Records[3].CommandSequence, Is.EqualTo(21));
            Assert.That(
                ledger.Records[3].Origin,
                Is.EqualTo(OfficerOrderOrigin.PlayerCommand));
            Assert.That(ledger.Records[3].OfficerActorId, Is.EqualTo(identity.ActorId));
            Assert.That(
                ledger.Records[3].TargetDescription,
                Is.EqualTo(order.TargetDescription));

            Object.Destroy(officer);
            yield return null;
        }
    }
}
