using System.Collections;
using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Officers;
using UnityEngine;
using UnityEngine.TestTools;

namespace RulesOfEntry.Tests.PlayMode
{
    public sealed class OfficerInitiativeLedgerTests
    {
        [UnityTest]
        public IEnumerator InitiativeHistory_RecordsOfficerAndEventOrigin()
        {
            GameObject officer = new GameObject("Initiative Ledger Test Officer");
            ActorIdentity identity = officer.AddComponent<ActorIdentity>();
            identity.Configure(
                "initiative_ledger_test_officer",
                "Initiative Ledger Test Officer",
                ActorRole.Officer,
                45001);
            OfficerInitiativeLedger ledger = officer.AddComponent<OfficerInitiativeLedger>();
            ledger.Configure(identity);

            OfficerInitiativeRecord record = ledger.Record(
                OfficerInitiativeEventType.AutomaticCustodyAssigned,
                null,
                null,
                null,
                "Test room-clear custody assignment.");

            Assert.That(ledger.Records.Count, Is.EqualTo(1));
            Assert.That(record.Sequence, Is.EqualTo(1));
            Assert.That(record.OfficerActorId, Is.EqualTo(identity.ActorId));
            Assert.That(
                record.EventType,
                Is.EqualTo(OfficerInitiativeEventType.AutomaticCustodyAssigned));

            Object.Destroy(officer);
            yield return null;
        }
    }
}
