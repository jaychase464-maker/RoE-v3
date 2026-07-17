using System.Collections;
using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Combat;
using RulesOfEntry.Missions;
using RulesOfEntry.Officers;
using UnityEngine.TestTools;

namespace RulesOfEntry.Tests.PlayMode
{
    public sealed class MissionEvidenceSnapshotTests
    {
        [UnityTest]
        public IEnumerator EvidenceSnapshot_ClonesInputArrays()
        {
            ActorEvidenceSnapshot[] source =
            {
                new ActorEvidenceSnapshot(
                    "original_actor",
                    1UL,
                    ActorRole.Suspect,
                    ActorConditionLevel.Stable,
                    CustodyState.Free,
                    HumanBehaviorState.Idle,
                    false)
            };
            MissionEvidenceSnapshot snapshot = new MissionEvidenceSnapshot(
                1d,
                1d,
                source,
                System.Array.Empty<RoomEvidenceSnapshot>(),
                System.Array.Empty<ForceEventRecord>(),
                System.Array.Empty<CustodyEventRecord>(),
                System.Array.Empty<OfficerOrderEventRecord>(),
                System.Array.Empty<OfficerInitiativeRecord>());
            source[0] = new ActorEvidenceSnapshot(
                "mutated_actor",
                2UL,
                ActorRole.Civilian,
                ActorConditionLevel.Deceased,
                CustodyState.Free,
                HumanBehaviorState.Incapacitated,
                false);

            Assert.That(snapshot.Actors[0].ActorId, Is.EqualTo("original_actor"));
            yield return null;
        }
    }
}
