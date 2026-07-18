using System;
using System.Linq;
using NUnit.Framework;
using RulesOfEntry.Actors;
using RulesOfEntry.Operations;

namespace RulesOfEntry.Tests.EditMode
{
    public sealed class MilestoneSevenAOperationTests
    {
        private static readonly OperationRoomRecord[] Rooms =
        {
            new OperationRoomRecord("entry", false),
            new OperationRoomRecord("corridor", true),
            new OperationRoomRecord("target", true),
            new OperationRoomRecord("side", true)
        };

        private static readonly OperationPortalRecord[] Portals =
        {
            new OperationPortalRecord("p1", "entry", "corridor"),
            new OperationPortalRecord("p2", "corridor", "target"),
            new OperationPortalRecord("p3", "corridor", "side")
        };

        [Test]
        public void ConnectedTopologyWithStableIdsIsValid()
        {
            OperationTopologyValidation validation = OperationTopologyRules.Validate(
                Rooms,
                Portals,
                new[] { new OperationEntryRecord("entry_south", "entry") });

            Assert.That(validation.IsValid, Is.True,
                string.Join(" | ", validation.Errors));
        }

        [Test]
        public void UnreachableRoomIsRejected()
        {
            OperationTopologyValidation validation = OperationTopologyRules.Validate(
                Rooms,
                Portals.Take(2).ToArray(),
                new[] { new OperationEntryRecord("entry_south", "entry") });

            Assert.That(validation.IsValid, Is.False);
            Assert.That(validation.Errors.Any(error =>
                error.IndexOf("unreachable", StringComparison.OrdinalIgnoreCase) >= 0),
                Is.True);
        }

        [Test]
        public void DuplicatePortalIdIsRejected()
        {
            OperationPortalRecord[] duplicate =
            {
                new OperationPortalRecord("p1", "entry", "corridor"),
                new OperationPortalRecord("p1", "corridor", "target"),
                new OperationPortalRecord("p3", "corridor", "side")
            };

            OperationTopologyValidation validation = OperationTopologyRules.Validate(
                Rooms,
                duplicate,
                new[] { new OperationEntryRecord("entry_south", "entry") });

            Assert.That(validation.IsValid, Is.False);
            Assert.That(validation.Errors.Any(error =>
                error.IndexOf("Duplicate portal", StringComparison.OrdinalIgnoreCase) >= 0),
                Is.True);
        }

        [Test]
        public void ShortestRouteUsesConnectedRoomSequence()
        {
            string[] route = OperationTopologyRules.FindShortestRoute(
                "entry",
                "target",
                Portals);

            Assert.That(route, Is.EqualTo(new[] { "entry", "corridor", "target" }));
        }

        [Test]
        public void ScenarioSelectionIsDeterministicAndRoleCompatible()
        {
            int[] roles =
            {
                (int)ActorRole.Suspect,
                (int)ActorRole.Civilian
            };
            OperationSpawnRecord[] points =
            {
                new OperationSpawnRecord("s1", (int)ActorRole.Suspect, "room_a", 1f),
                new OperationSpawnRecord("s2", (int)ActorRole.Suspect, "room_b", 2f),
                new OperationSpawnRecord("c1", (int)ActorRole.Civilian, "room_c", 1f),
                new OperationSpawnRecord("c2", (int)ActorRole.Civilian, "room_d", 1f)
            };

            int[] first = OperationTopologyRules.SelectSpawnIndices(roles, points, 70601);
            int[] second = OperationTopologyRules.SelectSpawnIndices(roles, points, 70601);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(first.Distinct().Count(), Is.EqualTo(first.Length));
            Assert.That(points[first[0]].ActorRole, Is.EqualTo((int)ActorRole.Suspect));
            Assert.That(points[first[1]].ActorRole, Is.EqualTo((int)ActorRole.Civilian));
            Assert.That(points[first[0]].RoomId, Is.Not.EqualTo(points[first[1]].RoomId));
        }

        [Test]
        public void MissingCompatibleSpawnReturnsUnassignedIndex()
        {
            int[] selection = OperationTopologyRules.SelectSpawnIndices(
                new[] { (int)ActorRole.Civilian },
                new[]
                {
                    new OperationSpawnRecord(
                        "s1",
                        (int)ActorRole.Suspect,
                        "room_a",
                        1f)
                },
                12);

            Assert.That(selection, Is.EqualTo(new[] { -1 }));
        }
    }
}
