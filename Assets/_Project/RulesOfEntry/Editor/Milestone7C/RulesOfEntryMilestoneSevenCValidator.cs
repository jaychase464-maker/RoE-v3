using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone7B;
using RulesOfEntry.Headquarters;
using RulesOfEntry.Operations;
using RulesOfEntry.UI;
using RulesOfEntry.UI.Headquarters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace RulesOfEntry.Editor.Milestone7C
{
    public static class RulesOfEntryMilestoneSevenCValidator
    {
        private const string ContextSourcePath =
            "Assets/_Project/RulesOfEntry/Runtime/Operations/CompletedOperationContext.cs";

        [MenuItem(
            "Tools/Rules of Entry/Milestone 7C/Validate Operation Closure and Headquarters Return",
            false,
            95)]
        public static void ValidateFromMenu()
        {
            IReadOnlyList<ProjectValidationResult> results = RunValidation(true);
            int errors = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Error);
            int passes = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Pass);
            EditorUtility.DisplayDialog(
                ProjectInfo.GameTitle,
                errors == 0
                    ? $"Milestone 7C validation passed with {passes} checks."
                    : $"Milestone 7C validation failed with {errors} error(s). See the Console.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();
            results.AddRange(RulesOfEntryMilestoneSevenBValidator.RunValidation(false));
            ValidateRuntimeBoundary(results);
            ValidateBuildScenes(results);
            ValidateOperationScene(results);
            ValidateHeadquartersScene(results);
            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateRuntimeBoundary(
            ICollection<ProjectValidationResult> results)
        {
            bool contractValid = typeof(CompletedOperationContext).IsAbstract
                && typeof(CompletedOperationContext).IsSealed
                && !typeof(UnityEngine.Object).IsAssignableFrom(
                    typeof(CompletedOperationRecord))
                && typeof(CompletedOperationRecord).GetProperty(
                    nameof(CompletedOperationRecord.Report)) != null
                && typeof(CompletedOperationRecord).GetProperty(
                    nameof(CompletedOperationRecord.AssignedOfficerIds)) != null;
            Add(
                results,
                contractValid,
                "M7C Completed Operation Contract",
                contractValid
                    ? "The session record is immutable, static-boundary owned, and not a Unity object."
                    : "The completed-operation session contract is incomplete.");

            bool sourceIsSceneReferenceFree = File.Exists(ContextSourcePath);
            if (sourceIsSceneReferenceFree)
            {
                string source = File.ReadAllText(ContextSourcePath);
                string[] forbidden =
                {
                    "using UnityEngine",
                    "UnityEngine.",
                    ": MonoBehaviour",
                    ": ScriptableObject",
                    "[SerializeField]"
                };
                sourceIsSceneReferenceFree = forbidden.All(token =>
                    source.IndexOf(token, StringComparison.Ordinal) < 0);
            }

            Add(
                results,
                sourceIsSceneReferenceFree,
                "M7C Scene Boundary",
                sourceIsSceneReferenceFree
                    ? "The cross-scene record contains stable values and no scene-object references."
                    : "CompletedOperationContext must not own Unity or scene-object references.");
        }

        private static void ValidateBuildScenes(
            ICollection<ProjectValidationResult> results)
        {
            string[] enabledPaths = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            bool valid = enabledPaths.Contains(
                    ProjectInfo.HeadquartersScenePath,
                    StringComparer.Ordinal)
                && enabledPaths.Contains(
                    ProjectInfo.PrototypeScenePath,
                    StringComparer.Ordinal);
            Add(
                results,
                valid,
                "M7C Return Destinations",
                valid
                    ? "Headquarters and the prototype operation are enabled build scenes."
                    : "Headquarters and the prototype operation must both be enabled in Build Settings.");
        }

        private static void ValidateOperationScene(
            ICollection<ProjectValidationResult> results)
        {
            ValidateScene(
                ProjectInfo.PrototypeScenePath,
                "M7C Operation Return",
                scene =>
                {
                    MissionAfterActionPresentation presentation = scene
                        .GetRootGameObjects()
                        .Where(root => string.Equals(
                            root.name,
                            RulesOfEntryMilestoneSevenBSetup.PresentationRootName,
                            StringComparison.Ordinal))
                        .Select(root => root.GetComponent<MissionAfterActionPresentation>())
                        .FirstOrDefault();
                    EventSystem eventSystem = FindInScene<EventSystem>(scene);
                    return presentation != null
                        && presentation.HasCompleteConfiguration
                        && eventSystem != null
                        && eventSystem.GetComponent<InputSystemUIInputModule>() != null
                        && !HasMissingScripts(scene);
                },
                "The final report has a fully wired Continue control, Input System UI, and no missing scripts.",
                "Rebuild Milestone 7C: the operation report return, EventSystem, or script references are incomplete.",
                results);
        }

        private static void ValidateHeadquartersScene(
            ICollection<ProjectValidationResult> results)
        {
            ValidateScene(
                ProjectInfo.HeadquartersScenePath,
                "M7C Headquarters Archive",
                scene =>
                {
                    GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate =>
                        string.Equals(
                            candidate.name,
                            RulesOfEntryMilestoneSevenCSetup.HeadquartersRootName,
                            StringComparison.Ordinal));
                    HeadquartersAfterActionReviewController review =
                        FindInScene<HeadquartersAfterActionReviewController>(scene);
                    HeadquartersAfterActionTerminalInteractable terminal =
                        FindInScene<HeadquartersAfterActionTerminalInteractable>(scene);
                    EventSystem eventSystem = FindInScene<EventSystem>(scene);
                    return root != null
                        && review != null
                        && review.HasCompleteConfiguration
                        && review.OpenOnStartWhenAvailable
                        && terminal != null
                        && terminal.HasCompleteConfiguration
                        && terminal.ReviewController == review
                        && eventSystem != null
                        && eventSystem.GetComponent<InputSystemUIInputModule>() != null
                        && !HasMissingScripts(scene);
                },
                "Headquarters has an automatic latest-report review, an archive terminal, Input System UI, and no missing scripts.",
                "Rebuild Milestone 7C: headquarters archive presentation or terminal wiring is incomplete.",
                results);
        }

        private static void ValidateScene(
            string path,
            string check,
            Func<Scene, bool> validator,
            string passMessage,
            string failureMessage,
            ICollection<ProjectValidationResult> results)
        {
            if (!File.Exists(path))
            {
                Add(results, false, check, $"Scene is missing: {path}");
                return;
            }

            Scene scene = SceneManager.GetSceneByPath(path);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened)
            {
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }

            try
            {
                bool valid = validator(scene);
                Add(results, valid, check, valid ? passMessage : failureMessage);
            }
            finally
            {
                if (opened && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static bool HasMissingScripts(Scene scene)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Any(transform => GameObjectUtility
                    .GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0);
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .FirstOrDefault();
        }

        private static void Add(
            ICollection<ProjectValidationResult> results,
            bool pass,
            string check,
            string message)
        {
            results.Add(new ProjectValidationResult(
                pass ? ProjectValidationSeverity.Pass : ProjectValidationSeverity.Error,
                check,
                message));
        }

        private static void LogResults(IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                string message = $"{result.Check}: {result.Message}";
                if (result.Severity == ProjectValidationSeverity.Error)
                {
                    ProjectLog.Error("M7C Validation", message);
                }
                else if (result.Severity == ProjectValidationSeverity.Warning)
                {
                    ProjectLog.Warning("M7C Validation", message);
                }
                else
                {
                    ProjectLog.Info("M7C Validation", message);
                }
            }
        }
    }
}
