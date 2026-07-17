using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone3;
using RulesOfEntry.Navigation;
using RulesOfEntry.Officers;
using RulesOfEntry.UI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RulesOfEntry.Editor.Milestone4
{
    public static class RulesOfEntryMilestoneFourValidator
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 4/Validate Officer Team Prototype";
        private const string RuntimeOfficerRoot =
            "Assets/_Project/RulesOfEntry/Runtime/Officers";
        private const string RuntimeAssemblyPath =
            "Assets/_Project/RulesOfEntry/Runtime/RulesOfEntry.Runtime.asmdef";

        [MenuItem(MenuPath, false, 51)]
        public static void ValidateFromMenu()
        {
            IReadOnlyList<ProjectValidationResult> results = RunValidation(true);
            int errors = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Error);
            int warnings = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Warning);
            int passes = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Pass);
            string summary = errors == 0
                ? $"Milestone 4 validation passed with {passes} checks and {warnings} warning(s)."
                : $"Milestone 4 validation failed with {errors} error(s), {warnings} warning(s), and {passes} passing checks.";
            EditorUtility.DisplayDialog(
                ProjectInfo.GameTitle,
                summary + "\n\nSee the Console for details.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();
            results.AddRange(RulesOfEntryMilestoneThreeValidator.RunValidation(false));
            ValidateOfficerInput(results);
            ValidateNavigationRuntimeReference(results);
            ValidatePlayerCommandComponent(results);
            ValidateOfficerPrefab(
                results,
                RulesOfEntryMilestoneFourSetup.OfficerAlphaPrefabPath,
                "m4_officer_alpha");
            ValidateOfficerPrefab(
                results,
                RulesOfEntryMilestoneFourSetup.OfficerBravoPrefabPath,
                "m4_officer_bravo");
            ValidateSupportingPrefabs(results);
            ValidatePrototypeScene(results);
            ValidateOrderArchitecture(results);
            ValidateRealismBoundary(results);

            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateOfficerInput(
            ICollection<ProjectValidationResult> results)
        {
            InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                RulesOfEntryMilestoneFourSetup.InputAssetPath);
            string[] actions =
            {
                "SelectOfficerOne",
                "SelectOfficerTwo",
                "SelectOfficerTeam",
                "CycleOfficerSelection",
                "IssueOfficerContextOrder",
                "OfficerMove",
                "OfficerHold",
                "OfficerFollow",
                "OfficerStack",
                "OfficerOpen",
                "OfficerRestrain",
                "CancelOfficerOrder"
            };
            string[] missing = actions.Where(actionName =>
                    inputAsset == null
                    || inputAsset.FindAction("Player/" + actionName, false) == null)
                .ToArray();
            bool hasContextGamepad = HasBinding(
                inputAsset,
                "Player/IssueOfficerContextOrder",
                "<Gamepad>/dpad/right");
            bool hasMoveKeyboard = HasBinding(
                inputAsset,
                "Player/OfficerMove",
                "<Keyboard>/g");
            bool hasCancelKeyboard = HasBinding(
                inputAsset,
                "Player/CancelOfficerOrder",
                "<Keyboard>/z");
            string source = File.Exists(RulesOfEntryMilestoneFourSetup.InputAssetPath)
                ? File.ReadAllText(RulesOfEntryMilestoneFourSetup.InputAssetPath)
                : string.Empty;
            bool sourcePersisted = source.IndexOf(
                    "\"IssueOfficerContextOrder\"",
                    StringComparison.Ordinal) >= 0
                && source.IndexOf(
                    "\"CancelOfficerOrder\"",
                    StringComparison.Ordinal) >= 0;
            if (missing.Length > 0
                || !hasContextGamepad
                || !hasMoveKeyboard
                || !hasCancelKeyboard
                || !sourcePersisted)
            {
                AddError(
                    results,
                    "M4 Officer Input",
                    missing.Length > 0
                        ? "Missing officer actions: " + string.Join(", ", missing) + "."
                        : !sourcePersisted
                            ? "Officer actions exist in memory but are absent from the JSON-backed ROE_InputActions.inputactions source. Re-run the Hotfix 1 setup."
                            : "Required keyboard or gamepad officer bindings are missing.");
                return;
            }

            AddPass(
                results,
                "M4 Officer Input",
                "Selection, context, explicit task, and cancellation actions use the current Input System.");
        }

        private static void ValidatePlayerCommandComponent(
            ICollection<ProjectValidationResult> results)
        {
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneFourSetup.PlayerPrefabPath);
            bool valid = player != null
                && player.GetComponent<RulesOfEntry.Input.TacticalPlayerInput>() != null
                && player.GetComponent<PlayerInput>() != null
                && player.GetComponent<OfficerSquadController>() != null;
            if (!valid)
            {
                AddError(
                    results,
                    "M4 Player Command Layer",
                    "Player prefab must contain PlayerInput, TacticalPlayerInput, and OfficerSquadController.");
                return;
            }

            AddPass(
                results,
                "M4 Player Command Layer",
                "Player prefab owns the squad command layer without absorbing officer execution logic.");
        }

        private static void ValidateNavigationRuntimeReference(
            ICollection<ProjectValidationResult> results)
        {
            string source = File.Exists(RuntimeAssemblyPath)
                ? File.ReadAllText(RuntimeAssemblyPath)
                : string.Empty;
            if (source.IndexOf("Unity.AI.Navigation", StringComparison.Ordinal) < 0)
            {
                AddError(
                    results,
                    "M4 Runtime Navigation Reference",
                    "RulesOfEntry.Runtime must reference Unity.AI.Navigation for the gated doorway link.");
                return;
            }

            AddPass(
                results,
                "M4 Runtime Navigation Reference",
                "Runtime assembly references the installed AI Navigation package.");
        }

        private static void ValidateOfficerPrefab(
            ICollection<ProjectValidationResult> results,
            string path,
            string expectedActorId)
        {
            GameObject officer = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            ActorIdentity identity = officer != null ? officer.GetComponent<ActorIdentity>() : null;
            TacticalOfficerController controller = officer != null
                ? officer.GetComponent<TacticalOfficerController>()
                : null;
            OfficerInitiativeController initiative = officer != null
                ? officer.GetComponent<OfficerInitiativeController>()
                : null;
            OfficerInitiativeLedger initiativeLedger = officer != null
                ? officer.GetComponent<OfficerInitiativeLedger>()
                : null;
            ActorHitRegion[] regions = officer != null
                ? officer.GetComponentsInChildren<ActorHitRegion>(true)
                : Array.Empty<ActorHitRegion>();
            bool valid = officer != null
                && identity != null
                && identity.Role == ActorRole.Officer
                && string.Equals(identity.ActorId, expectedActorId, StringComparison.Ordinal)
                && officer.GetComponent<ActorCondition>() != null
                && officer.GetComponent<ActorInventory>() != null
                && officer.GetComponent<OfficerOrderLedger>() != null
                && officer.GetComponent<NavMeshAgent>() != null
                && officer.GetComponent<OfficerVisual>() != null
                && controller != null
                && controller.HasCompleteConfiguration
                && initiative != null
                && initiative.HasCompleteConfiguration
                && initiativeLedger != null
                && initiativeLedger.HasCompleteConfiguration
                && regions.Any(region => region.Region == ActorHitRegionType.Head)
                && regions.Any(region => region.Region == ActorHitRegionType.Torso);
            if (!valid)
            {
                AddError(
                    results,
                    "M4 Officer Prefab",
                    $"{path} is missing officer identity, condition, navigation, command/initiative ledger, initiative controller, presentation, or hit regions.");
                return;
            }

            AddPass(
                results,
                "M4 Officer Prefab",
                $"{identity.DisplayName} has explicit identity, injury, navigation, execution, command history, and bounded-initiative components.");
        }

        private static void ValidateSupportingPrefabs(
            ICollection<ProjectValidationResult> results)
        {
            GameObject marker = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneFourSetup.MarkerPrefabPath);
            GameObject ui = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneFourSetup.DebugUiPrefabPath);
            bool valid = marker != null
                && marker.GetComponent<OfficerOrderMarker>() != null
                && ui != null
                && ui.GetComponent<OfficerCommandDebugUI>() != null;
            if (!valid)
            {
                AddError(
                    results,
                    "M4 Command Feedback",
                    "Order marker or officer command diagnostics prefab is missing.");
                return;
            }

            AddPass(
                results,
                "M4 Command Feedback",
                "World command marker and prototype officer diagnostics prefabs exist.");
        }

        private static void ValidatePrototypeScene(
            ICollection<ProjectValidationResult> results)
        {
            string scenePath = ProjectInfo.PrototypeScenePath;
            if (!File.Exists(scenePath))
            {
                AddError(results, "M4 Prototype Scene", $"Missing {scenePath}.");
                return;
            }

            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            string[] required =
            {
                RulesOfEntryMilestoneFourSetup.OfficerAlphaPrefabPath,
                RulesOfEntryMilestoneFourSetup.OfficerBravoPrefabPath,
                RulesOfEntryMilestoneFourSetup.MarkerPrefabPath,
                RulesOfEntryMilestoneFourSetup.DebugUiPrefabPath
            };
            string[] missing = required
                .Where(path => !dependencies.Contains(path, StringComparer.Ordinal))
                .ToArray();
            string sceneText = File.ReadAllText(scenePath);
            bool rootMissing = sceneText.IndexOf(
                RulesOfEntryMilestoneFourSetup.GeneratedRootName,
                StringComparison.Ordinal) < 0;
            if (missing.Length > 0 || rootMissing)
            {
                AddError(
                    results,
                    "M4 Prototype Scene",
                    missing.Length > 0
                        ? "Missing dependencies: " + string.Join(", ", missing) + "."
                        : $"Missing generated root {RulesOfEntryMilestoneFourSetup.GeneratedRootName}.");
                return;
            }

            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedForValidation = !scene.IsValid() || !scene.isLoaded;
            if (openedForValidation)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            try
            {
                GameObject player = scene.GetRootGameObjects().FirstOrDefault(root =>
                    string.Equals(root.name, "ROE_Player", StringComparison.Ordinal));
                OfficerSquadController squad = player != null
                    ? player.GetComponent<OfficerSquadController>()
                    : null;
                if (squad == null || !squad.HasCompleteConfiguration)
                {
                    AddError(
                        results,
                        "M4 Saved Squad References",
                        squad == null
                            ? "The saved prototype scene player has no OfficerSquadController."
                            : "The saved prototype scene is missing: "
                                + squad.ConfigurationProblems
                                + ". Re-run Milestone 4 setup.");
                    return;
                }

                AddPass(
                    results,
                    "M4 Saved Squad References",
                    "The saved scene retains player input, command view, Alpha, Bravo, and order-marker references.");

                DoorTraversalLink traversal = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<DoorTraversalLink>(true))
                    .FirstOrDefault();
                bool validTraversal = traversal != null
                    && traversal.HasCompleteConfiguration
                    && traversal.NavigationLink != null
                    && traversal.NavigationLink.bidirectional
                    && traversal.NavigationLink.width >= 0.7f
                    && traversal.Door != null
                    && !traversal.transform.IsChildOf(traversal.Door.transform);
                if (!validTraversal)
                {
                    AddError(
                        results,
                        "M4 Door Traversal",
                        "The saved scene requires a bidirectional, fixed NavMeshLink gated by the prototype door's physical clearance.");
                    return;
                }

                AddPass(
                    results,
                    "M4 Door Traversal",
                    "A fixed bidirectional NavMesh link opens only after the physical door clears the threshold.");

                TacticalRoomVolume room = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<TacticalRoomVolume>(true))
                    .FirstOrDefault();
                HumanActorController suspect = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<HumanActorController>(true))
                    .FirstOrDefault(actor => actor.Identity != null
                        && actor.Identity.Role == ActorRole.Suspect);
                bool validRoom = room != null
                    && room.HasCompleteConfiguration
                    && suspect != null
                    && room.Contains(suspect.transform.position);
                if (!validRoom)
                {
                    AddError(
                        results,
                        "M4 Room Clearance",
                        "The saved prototype scene requires a configured tactical room volume containing the training suspect.");
                    return;
                }

                AddPass(
                    results,
                    "M4 Room Clearance",
                    "The north training room requires two actionable officers and a timed no-threat verification before automatic custody.");
            }
            finally
            {
                if (openedForValidation && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            AddPass(
                results,
                "M4 Prototype Scene",
                "Two officers, command feedback, and the Milestone 4 root are wired into the existing prototype scene.");
        }

        private static void ValidateOrderArchitecture(
            ICollection<ProjectValidationResult> results)
        {
            bool immutableOrder = typeof(OfficerOrder)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite);
            bool pureStateMachine = !typeof(MonoBehaviour).IsAssignableFrom(
                typeof(OfficerOrderStateMachine));
            bool ledgerReadOnly = typeof(OfficerOrderLedger)
                .GetProperty(nameof(OfficerOrderLedger.Records))?.PropertyType
                .IsGenericType == true;
            bool originImmutable = typeof(OfficerOrder)
                .GetProperty(nameof(OfficerOrder.Origin))?.CanWrite == false;
            if (!immutableOrder || !pureStateMachine || !ledgerReadOnly || !originImmutable)
            {
                AddError(
                    results,
                    "M4 Order Architecture",
                    "Officer orders must be immutable, lifecycle logic must remain pure, and ledger history must be read-only to consumers.");
                return;
            }

            AddPass(
                results,
                "M4 Order Architecture",
                "Immutable order facts preserve player/initiative origin, pure lifecycle transitions, and append-only officer history.");
        }

        private static void ValidateRealismBoundary(
            ICollection<ProjectValidationResult> results)
        {
            if (!Directory.Exists(RuntimeOfficerRoot))
            {
                AddError(
                    results,
                    "M4 Execution Boundary",
                    $"Missing {RuntimeOfficerRoot}.");
                return;
            }

            string[] forbiddenPatterns =
            {
                @"\bMissionScore\b",
                @"\bPenalty\b",
                @"\bAwardPoints\b",
                @"\.Warp\s*\(",
                @"\bAutoArrest\b",
                @"\bAutoReload\b"
            };
            string[] found = Directory.EnumerateFiles(
                    RuntimeOfficerRoot,
                    "*.cs",
                    SearchOption.AllDirectories)
                .Where(path => forbiddenPatterns.Any(pattern =>
                    Regex.IsMatch(
                        File.ReadAllText(path),
                        pattern,
                        RegexOptions.CultureInvariant)))
                .ToArray();
            string controllerPath = RuntimeOfficerRoot + "/TacticalOfficerController.cs";
            string controllerSource = File.Exists(controllerPath)
                ? File.ReadAllText(controllerPath)
                : string.Empty;
            bool custodyStepsPresent = controllerSource.IndexOf(
                    "TryOrderToKneel",
                    StringComparison.Ordinal) >= 0
                && controllerSource.IndexOf(
                    "restraintApplicationSeconds",
                    StringComparison.Ordinal) >= 0
                && controllerSource.IndexOf(
                    "TryApplyRestraints",
                    StringComparison.Ordinal) >= 0;
            string initiativePath = RuntimeOfficerRoot + "/OfficerInitiativeController.cs";
            string initiativeSource = File.Exists(initiativePath)
                ? File.ReadAllText(initiativePath)
                : string.Empty;
            string roomPath = RuntimeOfficerRoot + "/TacticalRoomVolume.cs";
            string roomSource = File.Exists(roomPath)
                ? File.ReadAllText(roomPath)
                : string.Empty;
            bool boundedInitiativePresent = initiativeSource.IndexOf(
                    "IsEligibleForAutomaticCustody",
                    StringComparison.Ordinal) >= 0
                && initiativeSource.IndexOf(
                    "HasCoverOfficer",
                    StringComparison.Ordinal) >= 0
                && initiativeSource.IndexOf(
                    "RoomNoLongerClear",
                    StringComparison.Ordinal) >= 0
                && roomSource.IndexOf(
                    "clearVerificationSeconds",
                    StringComparison.Ordinal) >= 0
                && roomSource.IndexOf(
                    "minimumOfficerCount",
                    StringComparison.Ordinal) >= 0;
            if (found.Length > 0 || !custodyStepsPresent || !boundedInitiativePresent)
            {
                AddError(
                    results,
                    "M4 Execution Boundary",
                    found.Length > 0
                        ? "Officer code contains forbidden teleportation, scoring, or automation concepts: "
                            + string.Join(", ", found) + "."
                        : "Officer custody must preserve room-clear verification, cover, kneeling, timed cuffing, and restraint verification steps.");
                return;
            }

            AddPass(
                results,
                "M4 Execution Boundary",
                "Player and initiative orders use physical paths, verified room clearance, cover, and explicit custody transitions without teleportation or direct state forcing.");
        }

        private static bool HasBinding(
            InputActionAsset asset,
            string actionPath,
            string expectedControlPath)
        {
            InputAction action = asset != null ? asset.FindAction(actionPath, false) : null;
            return action != null && action.bindings.Any(binding =>
                string.Equals(
                    binding.effectivePath,
                    expectedControlPath,
                    StringComparison.OrdinalIgnoreCase));
        }

        private static void LogResults(IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                string message = $"{result.Check}: {result.Message}";
                switch (result.Severity)
                {
                    case ProjectValidationSeverity.Error:
                        ProjectLog.Error("M4 Validation", message);
                        break;
                    case ProjectValidationSeverity.Warning:
                        ProjectLog.Warning("M4 Validation", message);
                        break;
                    default:
                        ProjectLog.Info("M4 Validation", message);
                        break;
                }
            }
        }

        private static void AddPass(
            ICollection<ProjectValidationResult> results,
            string check,
            string message)
        {
            results.Add(new ProjectValidationResult(ProjectValidationSeverity.Pass, check, message));
        }

        private static void AddError(
            ICollection<ProjectValidationResult> results,
            string check,
            string message)
        {
            results.Add(new ProjectValidationResult(ProjectValidationSeverity.Error, check, message));
        }
    }
}
