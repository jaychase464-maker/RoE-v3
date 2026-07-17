using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RulesOfEntry.Actors;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone4;
using RulesOfEntry.Missions;
using RulesOfEntry.Officers;
using RulesOfEntry.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RulesOfEntry.Editor.Milestone5
{
    public static class RulesOfEntryMilestoneFiveValidator
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 5/Validate Mission and After-Action Prototype";
        private const string RuntimeRoot = "Assets/_Project/RulesOfEntry/Runtime";
        private const string MissionRuntimeRoot = RuntimeRoot + "/Missions";

        [MenuItem(MenuPath, false, 61)]
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
                ? $"Milestone 5 validation passed with {passes} checks and {warnings} warning(s)."
                : $"Milestone 5 validation failed with {errors} error(s), {warnings} warning(s), and {passes} passing checks.";
            EditorUtility.DisplayDialog(
                ProjectInfo.GameTitle,
                summary + "\n\nSee the Console for details.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();
            results.AddRange(RulesOfEntryMilestoneFourValidator.RunValidation(false));
            ValidateProjectIdentity(results);
            ValidateMissionAssets(results);
            ValidateEvaluationArchitecture(results);
            ValidateOneWayEvidenceBoundary(results);
            ValidatePrototypeScene(results);

            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateProjectIdentity(
            ICollection<ProjectValidationResult> results)
        {
            if (ProjectInfo.CurrentMilestone.IndexOf(
                    "Milestone 5",
                    StringComparison.Ordinal) < 0)
            {
                AddError(
                    results,
                    "M5 Project Identity",
                    "ProjectInfo.CurrentMilestone must identify Milestone 5.");
                return;
            }

            AddPass(
                results,
                "M5 Project Identity",
                "Project metadata identifies the mission, ROE, and after-action milestone.");
        }

        private static void ValidateMissionAssets(
            ICollection<ProjectValidationResult> results)
        {
            MissionDefinition definition = AssetDatabase.LoadAssetAtPath<MissionDefinition>(
                RulesOfEntryMilestoneFiveSetup.MissionDefinitionPath);
            RulesOfEngagementPolicy policy =
                AssetDatabase.LoadAssetAtPath<RulesOfEngagementPolicy>(
                    RulesOfEntryMilestoneFiveSetup.RoePolicyPath);
            MissionObjectiveDefinition[] objectives = definition != null
                ? definition.Objectives
                : Array.Empty<MissionObjectiveDefinition>();
            MissionObjectiveType[] requiredTypes =
            {
                MissionObjectiveType.SecureSubject,
                MissionObjectiveType.ProtectActor,
                MissionObjectiveType.VerifyRoomClear,
                MissionObjectiveType.PreserveOfficerTeam
            };
            bool objectiveTargetsValid = objectives.Any(objective =>
                    objective.Type == MissionObjectiveType.SecureSubject
                    && string.Equals(
                        objective.TargetActorId,
                        "m3_suspect_01",
                        StringComparison.Ordinal))
                && objectives.Any(objective =>
                    objective.Type == MissionObjectiveType.ProtectActor
                    && string.Equals(
                        objective.TargetActorId,
                        "m3_civilian_01",
                        StringComparison.Ordinal))
                && objectives.Any(objective =>
                    objective.Type == MissionObjectiveType.VerifyRoomClear
                    && string.Equals(
                        objective.TargetRoomId,
                        "prototype_north_training_room",
                        StringComparison.Ordinal));
            bool valid = definition != null
                && definition.HasValidConfiguration
                && policy != null
                && policy.HasValidConfiguration
                && objectiveTargetsValid
                && requiredTypes.All(type => objectives.Any(objective =>
                    objective.Type == type && objective.Required));
            if (!valid)
            {
                AddError(
                    results,
                    "M5 Mission Assets",
                    "Mission definition and ROE policy must contain valid secure-subject, civilian-protection, room-clear, and team-preservation requirements.");
                return;
            }

            AddPass(
                results,
                "M5 Mission Assets",
                "Mission objectives and threat-based ROE policy are explicit ScriptableObject assets.");
        }

        private static void ValidateEvaluationArchitecture(
            ICollection<ProjectValidationResult> results)
        {
            bool reportImmutable = typeof(AfterActionReport)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite);
            bool evidenceImmutable = typeof(MissionEvidenceSnapshot)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => !property.CanWrite);
            bool objectiveEvaluatorPure = !typeof(MonoBehaviour).IsAssignableFrom(
                typeof(MissionObjectiveEvaluator));
            bool roeEvaluatorPure = !typeof(MonoBehaviour).IsAssignableFrom(
                typeof(RulesOfEngagementEvaluator));
            bool reportEvaluatorPure = !typeof(MonoBehaviour).IsAssignableFrom(
                typeof(AfterActionEvaluator));
            if (!reportImmutable
                || !evidenceImmutable
                || !objectiveEvaluatorPure
                || !roeEvaluatorPure
                || !reportEvaluatorPure)
            {
                AddError(
                    results,
                    "M5 Evaluation Architecture",
                    "Evidence and reports must be immutable, and objective/ROE/report evaluators must remain pure runtime services.");
                return;
            }

            AddPass(
                results,
                "M5 Evaluation Architecture",
                "Immutable evidence feeds pure objective, ROE, and report evaluators.");
        }

        private static void ValidateOneWayEvidenceBoundary(
            ICollection<ProjectValidationResult> results)
        {
            string[] producerRoots =
            {
                RuntimeRoot + "/AI",
                RuntimeRoot + "/Actors",
                RuntimeRoot + "/Combat",
                RuntimeRoot + "/Officers"
            };
            string[] reverseReferences = producerRoots
                .Where(Directory.Exists)
                .SelectMany(path => Directory.EnumerateFiles(
                    path,
                    "*.cs",
                    SearchOption.AllDirectories))
                .Where(path => File.ReadAllText(path).IndexOf(
                    "RulesOfEntry.Missions",
                    StringComparison.Ordinal) >= 0)
                .ToArray();
            string[] forbiddenMutationPatterns =
            {
                @"\bTryApplyRestraints\s*\(",
                @"\bTryBeginSurrender\s*\(",
                @"\bReceiveBallisticHit\s*\(",
                @"\bAssignOrder\s*\(",
                @"\bRecordFirearmDischarge\s*\("
            };
            string[] missionMutations = Directory.Exists(MissionRuntimeRoot)
                ? Directory.EnumerateFiles(
                        MissionRuntimeRoot,
                        "*.cs",
                        SearchOption.AllDirectories)
                    .Where(path => forbiddenMutationPatterns.Any(pattern => Regex.IsMatch(
                        File.ReadAllText(path),
                        pattern,
                        RegexOptions.CultureInvariant)))
                    .ToArray()
                : Array.Empty<string>();
            if (reverseReferences.Length > 0 || missionMutations.Length > 0)
            {
                AddError(
                    results,
                    "M5 One-Way Evidence Boundary",
                    reverseReferences.Length > 0
                        ? "Fact producers reference the mission evaluator: "
                            + string.Join(", ", reverseReferences) + "."
                        : "Mission evaluation contains forbidden state-changing calls: "
                            + string.Join(", ", missionMutations) + ".");
                return;
            }

            AddPass(
                results,
                "M5 One-Way Evidence Boundary",
                "Combat, AI, custody, and officer systems emit facts without depending on mission score or policy judgment.");
        }

        private static void ValidatePrototypeScene(
            ICollection<ProjectValidationResult> results)
        {
            string scenePath = ProjectInfo.PrototypeScenePath;
            if (!File.Exists(scenePath))
            {
                AddError(results, "M5 Prototype Scene", $"Missing {scenePath}.");
                return;
            }

            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            string[] requiredDependencies =
            {
                RulesOfEntryMilestoneFiveSetup.MissionDefinitionPath,
                RulesOfEntryMilestoneFiveSetup.RoePolicyPath,
                RulesOfEntryMilestoneFiveSetup.AfterActionUiPrefabPath
            };
            string[] missing = requiredDependencies
                .Where(path => !dependencies.Contains(path, StringComparer.Ordinal))
                .ToArray();
            if (missing.Length > 0)
            {
                AddError(
                    results,
                    "M5 Prototype Scene",
                    "Missing saved scene dependencies: " + string.Join(", ", missing) + ".");
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
                MissionController controller = scene.GetRootGameObjects()
                    .Select(root => root.GetComponent<MissionController>())
                    .FirstOrDefault(value => value != null);
                MissionAfterActionDebugUI ui = scene.GetRootGameObjects()
                    .Select(root => root.GetComponent<MissionAfterActionDebugUI>())
                    .FirstOrDefault(value => value != null);
                MissionDebriefInteractable debrief = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<MissionDebriefInteractable>(true))
                    .FirstOrDefault();
                ActorIdentity suspect = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<ActorIdentity>(true))
                    .FirstOrDefault(identity => string.Equals(
                        identity.ActorId,
                        "m3_suspect_01",
                        StringComparison.Ordinal));
                ActorIdentity civilian = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<ActorIdentity>(true))
                    .FirstOrDefault(identity => string.Equals(
                        identity.ActorId,
                        "m3_civilian_01",
                        StringComparison.Ordinal));
                TacticalRoomVolume room = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<TacticalRoomVolume>(true))
                    .FirstOrDefault(value => string.Equals(
                        value.RoomId,
                        "prototype_north_training_room",
                        StringComparison.Ordinal));
                bool valid = controller != null
                    && controller.HasCompleteConfiguration
                    && ui != null
                    && ui.HasCompleteConfiguration
                    && ui.MissionController == controller
                    && debrief != null
                    && debrief.HasCompleteConfiguration
                    && debrief.MissionController == controller
                    && suspect != null
                    && civilian != null
                    && room != null;
                if (!valid)
                {
                    AddError(
                        results,
                        "M5 Prototype Scene",
                        "Saved scene requires a configured mission controller, debrief console, retained UI reference, target actors, and north-room evidence source.");
                    return;
                }

                AddPass(
                    results,
                    "M5 Prototype Scene",
                    "Saved scene retains the training mission, ROE policy, debrief console, evidence sources, and after-action diagnostics.");
            }
            finally
            {
                if (openedForValidation && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void LogResults(IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                string message = $"{result.Check}: {result.Message}";
                switch (result.Severity)
                {
                    case ProjectValidationSeverity.Error:
                        ProjectLog.Error("M5 Validation", message);
                        break;
                    case ProjectValidationSeverity.Warning:
                        ProjectLog.Warning("M5 Validation", message);
                        break;
                    default:
                        ProjectLog.Info("M5 Validation", message);
                        break;
                }
            }
        }

        private static void AddPass(
            ICollection<ProjectValidationResult> results,
            string check,
            string message)
        {
            results.Add(new ProjectValidationResult(
                ProjectValidationSeverity.Pass,
                check,
                message));
        }

        private static void AddError(
            ICollection<ProjectValidationResult> results,
            string check,
            string message)
        {
            results.Add(new ProjectValidationResult(
                ProjectValidationSeverity.Error,
                check,
                message));
        }
    }
}
