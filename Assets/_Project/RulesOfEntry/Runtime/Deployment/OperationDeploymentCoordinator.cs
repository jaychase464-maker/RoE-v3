using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Officers;
using RulesOfEntry.Planning;
using UnityEngine;
using UnityEngine.AI;

namespace RulesOfEntry.Deployment
{
    /// <summary>
    /// Consumes stable headquarters deployment identifiers and applies them to
    /// authored operation anchors and scene-owned officer objects.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    [DisallowMultipleComponent]
    public sealed class OperationDeploymentCoordinator : MonoBehaviour
    {
        [SerializeField] private Transform playerRoot;
        [SerializeField] private CharacterController playerCharacterController;
        [SerializeField] private OfficerSquadController squad;
        [SerializeField] private OperationEntryAnchor[] entryAnchors =
            Array.Empty<OperationEntryAnchor>();
        [SerializeField, Min(0.1f)] private float navigationSampleRadius = 2f;

        public string AppliedMissionId { get; private set; } = string.Empty;
        public string AppliedEntryPointId { get; private set; } = string.Empty;
        public bool DeploymentApplied { get; private set; }
        public bool HasCompleteConfiguration => playerRoot != null
            && playerCharacterController != null
            && squad != null
            && entryAnchors != null
            && entryAnchors.Length > 0
            && entryAnchors.All(anchor => anchor != null
                && anchor.HasValidConfiguration)
            && entryAnchors.Select(anchor => anchor.EntryPointId)
                .Distinct(StringComparer.Ordinal)
                .Count() == entryAnchors.Length;

        public void Configure(
            Transform configuredPlayerRoot,
            CharacterController configuredCharacterController,
            OfficerSquadController configuredSquad,
            OperationEntryAnchor[] configuredEntryAnchors,
            float configuredNavigationSampleRadius = 2f)
        {
            playerRoot = configuredPlayerRoot;
            playerCharacterController = configuredCharacterController;
            squad = configuredSquad;
            entryAnchors = configuredEntryAnchors?
                .Where(anchor => anchor != null)
                .ToArray() ?? Array.Empty<OperationEntryAnchor>();
            navigationSampleRadius = Mathf.Max(0.1f, configuredNavigationSampleRadius);
        }

        private void Start()
        {
            if (!HasCompleteConfiguration)
            {
                ProjectLog.Error(
                    "Operation Deployment",
                    "Deployment references or authored entry anchors are incomplete. "
                        + "Run the Milestone 6C setup tool.",
                    this);
                return;
            }

            if (OperationDeploymentContext.HasPendingDeployment)
            {
                ApplyPendingDeployment();
            }
        }

        public bool ApplyPendingDeployment()
        {
            if (!HasCompleteConfiguration
                || !OperationDeploymentContext.HasPendingDeployment)
            {
                return false;
            }

            OperationEntryAnchor anchor = entryAnchors.FirstOrDefault(candidate =>
                string.Equals(
                    candidate.EntryPointId,
                    OperationDeploymentContext.EntryPointId,
                    StringComparison.Ordinal));
            if (anchor == null)
            {
                ProjectLog.Error(
                    "Operation Deployment",
                    $"No authored operation anchor matches entry ID "
                        + $"'{OperationDeploymentContext.EntryPointId}'.",
                    this);
                return false;
            }

            HashSet<string> assignedIds = OperationDeploymentContext
                .AssignedOfficerIds
                .ToHashSet(StringComparer.Ordinal);
            TacticalOfficerController[] sceneOfficers = squad.Officers
                .Where(officer => officer != null)
                .ToArray();
            TacticalOfficerController[] deployedOfficers = sceneOfficers
                .Where(officer => assignedIds.Contains(
                    officer.Identity?.ActorId ?? string.Empty))
                .ToArray();
            if (deployedOfficers.Length != assignedIds.Count)
            {
                ProjectLog.Error(
                    "Operation Deployment",
                    "One or more confirmed officer IDs do not match scene officers.",
                    this);
                return false;
            }

            Vector3[] resolvedPositions = new Vector3[deployedOfficers.Length];
            Quaternion[] resolvedRotations = new Quaternion[deployedOfficers.Length];
            for (int index = 0; index < deployedOfficers.Length; index++)
            {
                Transform spawn = anchor.GetOfficerSpawn(index);
                if (!TryResolveOfficerPosition(spawn, out Vector3 position))
                {
                    ProjectLog.Error(
                        "Operation Deployment",
                        $"Could not resolve a NavMesh position for "
                            + $"{deployedOfficers[index].name} at "
                            + $"entry '{anchor.EntryPointId}'.",
                        deployedOfficers[index]);
                    return false;
                }

                resolvedPositions[index] = position;
                resolvedRotations[index] = spawn.rotation;
            }

            foreach (TacticalOfficerController officer in sceneOfficers)
            {
                officer.gameObject.SetActive(deployedOfficers.Contains(officer));
            }

            for (int index = 0; index < deployedOfficers.Length; index++)
            {
                if (!PlaceOfficer(
                    deployedOfficers[index],
                    resolvedPositions[index],
                    resolvedRotations[index]))
                {
                    ProjectLog.Error(
                        "Operation Deployment",
                        $"NavMeshAgent rejected the resolved deployment position for "
                            + $"{deployedOfficers[index].name}.",
                        deployedOfficers[index]);
                    return false;
                }
            }

            PlacePlayer(anchor.PlayerSpawn);
            squad.SetDeployedOfficers(deployedOfficers);
            AppliedMissionId = OperationDeploymentContext.MissionId;
            AppliedEntryPointId = anchor.EntryPointId;
            DeploymentApplied = true;
            ProjectLog.Info(
                "Operation Deployment",
                $"Applied entry '{anchor.DisplayName}' with "
                    + $"{deployedOfficers.Length} assigned officer(s).",
                this);
            return true;
        }

        private void PlacePlayer(Transform spawn)
        {
            bool wasEnabled = playerCharacterController.enabled;
            playerCharacterController.enabled = false;
            playerRoot.SetPositionAndRotation(spawn.position, spawn.rotation);
            playerCharacterController.enabled = wasEnabled;
            Physics.SyncTransforms();
        }

        private bool TryResolveOfficerPosition(
            Transform spawn,
            out Vector3 position)
        {
            position = Vector3.zero;
            if (spawn == null
                || !NavMesh.SamplePosition(
                    spawn.position,
                    out NavMeshHit hit,
                    navigationSampleRadius,
                    NavMesh.AllAreas))
            {
                return false;
            }

            position = hit.position;
            return true;
        }

        private static bool PlaceOfficer(
            TacticalOfficerController officer,
            Vector3 position,
            Quaternion rotation)
        {
            if (officer == null)
            {
                return false;
            }

            NavMeshAgent agent = officer.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                return false;
            }

            if (!agent.Warp(position))
            {
                return false;
            }

            officer.transform.rotation = rotation;
            return true;
        }

        private void OnValidate()
        {
            entryAnchors ??= Array.Empty<OperationEntryAnchor>();
            navigationSampleRadius = Mathf.Max(0.1f, navigationSampleRadius);
        }
    }
}
