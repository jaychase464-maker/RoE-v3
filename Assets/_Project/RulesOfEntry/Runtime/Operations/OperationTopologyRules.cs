using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.AI;

namespace RulesOfEntry.Operations
{
    /// <summary>
    /// Pure topology and spawn-selection rules. Keeping these decisions free of
    /// scene objects makes incident generation deterministic and testable.
    /// </summary>
    public static class OperationTopologyRules
    {
        public static OperationTopologyValidation Validate(
            IReadOnlyList<OperationRoomRecord> rooms,
            IReadOnlyList<OperationPortalRecord> portals,
            IReadOnlyList<OperationEntryRecord> entries)
        {
            List<string> errors = new List<string>();
            OperationRoomRecord[] safeRooms = rooms?.ToArray()
                ?? Array.Empty<OperationRoomRecord>();
            OperationPortalRecord[] safePortals = portals?.ToArray()
                ?? Array.Empty<OperationPortalRecord>();
            OperationEntryRecord[] safeEntries = entries?.ToArray()
                ?? Array.Empty<OperationEntryRecord>();

            if (safeRooms.Length == 0)
            {
                errors.Add("At least one operation room is required.");
            }

            AddEmptyOrDuplicateErrors(
                safeRooms.Select(room => room.RoomId),
                "room",
                errors);
            AddEmptyOrDuplicateErrors(
                safePortals.Select(portal => portal.PortalId),
                "portal",
                errors);
            AddEmptyOrDuplicateErrors(
                safeEntries.Select(entry => entry.EntryPointId),
                "entry point",
                errors);

            HashSet<string> roomIds = safeRooms
                .Where(room => !string.IsNullOrWhiteSpace(room.RoomId))
                .Select(room => room.RoomId)
                .ToHashSet(StringComparer.Ordinal);
            foreach (OperationPortalRecord portal in safePortals)
            {
                if (!roomIds.Contains(portal.RoomAId)
                    || !roomIds.Contains(portal.RoomBId))
                {
                    errors.Add($"Portal '{portal.PortalId}' references an unknown room.");
                }
                else if (string.Equals(
                    portal.RoomAId,
                    portal.RoomBId,
                    StringComparison.Ordinal))
                {
                    errors.Add($"Portal '{portal.PortalId}' cannot connect a room to itself.");
                }
            }

            foreach (OperationEntryRecord entry in safeEntries)
            {
                if (!roomIds.Contains(entry.RoomId))
                {
                    errors.Add(
                        $"Entry point '{entry.EntryPointId}' references an unknown room.");
                }
            }

            if (safeEntries.Length == 0)
            {
                errors.Add("At least one operation entry binding is required.");
            }

            if (errors.Count == 0)
            {
                HashSet<string> reachable = FindReachableRooms(
                    safeEntries.Select(entry => entry.RoomId),
                    safePortals);
                foreach (OperationRoomRecord room in safeRooms)
                {
                    if (!reachable.Contains(room.RoomId))
                    {
                        errors.Add(
                            $"Room '{room.RoomId}' is unreachable from every operation entry.");
                    }
                }
            }

            return new OperationTopologyValidation(errors.ToArray());
        }

        public static string[] FindShortestRoute(
            string startRoomId,
            string destinationRoomId,
            IReadOnlyList<OperationPortalRecord> portals)
        {
            if (string.IsNullOrWhiteSpace(startRoomId)
                || string.IsNullOrWhiteSpace(destinationRoomId))
            {
                return Array.Empty<string>();
            }

            if (string.Equals(startRoomId, destinationRoomId, StringComparison.Ordinal))
            {
                return new[] { startRoomId };
            }

            Dictionary<string, List<string>> adjacency = BuildAdjacency(portals);
            Queue<string> frontier = new Queue<string>();
            Dictionary<string, string> previous =
                new Dictionary<string, string>(StringComparer.Ordinal);
            HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal)
            {
                startRoomId
            };
            frontier.Enqueue(startRoomId);

            while (frontier.Count > 0)
            {
                string current = frontier.Dequeue();
                if (!adjacency.TryGetValue(current, out List<string> neighbours))
                {
                    continue;
                }

                foreach (string neighbour in neighbours)
                {
                    if (!visited.Add(neighbour))
                    {
                        continue;
                    }

                    previous[neighbour] = current;
                    if (string.Equals(
                        neighbour,
                        destinationRoomId,
                        StringComparison.Ordinal))
                    {
                        return ReconstructRoute(
                            startRoomId,
                            destinationRoomId,
                            previous);
                    }

                    frontier.Enqueue(neighbour);
                }
            }

            return Array.Empty<string>();
        }

        public static int[] SelectSpawnIndices(
            IReadOnlyList<int> actorRoles,
            IReadOnlyList<OperationSpawnRecord> spawnPoints,
            int incidentSeed)
        {
            int[] roles = actorRoles?.ToArray() ?? Array.Empty<int>();
            OperationSpawnRecord[] points = spawnPoints?.ToArray()
                ?? Array.Empty<OperationSpawnRecord>();
            int[] selections = Enumerable.Repeat(-1, roles.Length).ToArray();
            HashSet<int> usedPoints = new HashSet<int>();
            HashSet<string> usedRooms = new HashSet<string>(StringComparer.Ordinal);
            DeterministicDecisionRandom random =
                new DeterministicDecisionRandom(incidentSeed);

            for (int actorIndex = 0; actorIndex < roles.Length; actorIndex++)
            {
                List<int> candidates = FindCandidates(
                    roles[actorIndex],
                    points,
                    usedPoints,
                    usedRooms,
                    true);
                if (candidates.Count == 0)
                {
                    candidates = FindCandidates(
                        roles[actorIndex],
                        points,
                        usedPoints,
                        usedRooms,
                        false);
                }

                if (candidates.Count == 0)
                {
                    continue;
                }

                int selection = SelectWeighted(candidates, points, random.Next01());
                selections[actorIndex] = selection;
                usedPoints.Add(selection);
                if (!string.IsNullOrWhiteSpace(points[selection].RoomId))
                {
                    usedRooms.Add(points[selection].RoomId);
                }
            }

            return selections;
        }

        private static List<int> FindCandidates(
            int role,
            IReadOnlyList<OperationSpawnRecord> points,
            ISet<int> usedPoints,
            ISet<string> usedRooms,
            bool requireUnusedRoom)
        {
            List<int> candidates = new List<int>();
            for (int index = 0; index < points.Count; index++)
            {
                OperationSpawnRecord point = points[index];
                if (usedPoints.Contains(index)
                    || point.ActorRole != role
                    || (requireUnusedRoom && usedRooms.Contains(point.RoomId)))
                {
                    continue;
                }

                candidates.Add(index);
            }

            return candidates;
        }

        private static int SelectWeighted(
            IReadOnlyList<int> candidates,
            IReadOnlyList<OperationSpawnRecord> points,
            float normalizedRoll)
        {
            float total = candidates.Sum(index => points[index].Weight);
            float clampedRoll = Math.Max(0f, Math.Min(0.999999f, normalizedRoll));
            float target = clampedRoll * total;
            float cursor = 0f;
            foreach (int candidate in candidates)
            {
                cursor += points[candidate].Weight;
                if (target < cursor)
                {
                    return candidate;
                }
            }

            return candidates[candidates.Count - 1];
        }

        private static HashSet<string> FindReachableRooms(
            IEnumerable<string> startRooms,
            IReadOnlyList<OperationPortalRecord> portals)
        {
            Dictionary<string, List<string>> adjacency = BuildAdjacency(portals);
            HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);
            Queue<string> frontier = new Queue<string>();
            foreach (string start in startRooms.Where(value =>
                !string.IsNullOrWhiteSpace(value)))
            {
                if (visited.Add(start))
                {
                    frontier.Enqueue(start);
                }
            }

            while (frontier.Count > 0)
            {
                string current = frontier.Dequeue();
                if (!adjacency.TryGetValue(current, out List<string> neighbours))
                {
                    continue;
                }

                foreach (string neighbour in neighbours)
                {
                    if (visited.Add(neighbour))
                    {
                        frontier.Enqueue(neighbour);
                    }
                }
            }

            return visited;
        }

        private static Dictionary<string, List<string>> BuildAdjacency(
            IReadOnlyList<OperationPortalRecord> portals)
        {
            Dictionary<string, List<string>> adjacency =
                new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (OperationPortalRecord portal in portals
                ?? Array.Empty<OperationPortalRecord>())
            {
                AddNeighbour(adjacency, portal.RoomAId, portal.RoomBId);
                AddNeighbour(adjacency, portal.RoomBId, portal.RoomAId);
            }

            return adjacency;
        }

        private static void AddNeighbour(
            IDictionary<string, List<string>> adjacency,
            string roomId,
            string neighbourId)
        {
            if (string.IsNullOrWhiteSpace(roomId)
                || string.IsNullOrWhiteSpace(neighbourId))
            {
                return;
            }

            if (!adjacency.TryGetValue(roomId, out List<string> neighbours))
            {
                neighbours = new List<string>();
                adjacency[roomId] = neighbours;
            }

            if (!neighbours.Contains(neighbourId, StringComparer.Ordinal))
            {
                neighbours.Add(neighbourId);
            }
        }

        private static string[] ReconstructRoute(
            string start,
            string destination,
            IReadOnlyDictionary<string, string> previous)
        {
            List<string> route = new List<string> { destination };
            string cursor = destination;
            while (!string.Equals(cursor, start, StringComparison.Ordinal))
            {
                if (!previous.TryGetValue(cursor, out cursor))
                {
                    return Array.Empty<string>();
                }

                route.Add(cursor);
            }

            route.Reverse();
            return route.ToArray();
        }

        private static void AddEmptyOrDuplicateErrors(
            IEnumerable<string> values,
            string label,
            ICollection<string> errors)
        {
            string[] safe = values?.ToArray() ?? Array.Empty<string>();
            if (safe.Any(string.IsNullOrWhiteSpace))
            {
                errors.Add($"Every {label} requires a stable ID.");
            }

            if (safe.Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .Count() != safe.Count(value => !string.IsNullOrWhiteSpace(value)))
            {
                errors.Add($"Duplicate {label} IDs are not permitted.");
            }
        }
    }
}
