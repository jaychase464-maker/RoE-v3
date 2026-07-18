using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Missions;
using RulesOfEntry.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RulesOfEntry.Editor.Milestone7B
{
    public static class RulesOfEntryMilestoneSevenBValidator
    {
        [MenuItem(
            "Tools/Rules of Entry/Milestone 7B/Validate Automatic After-Action Tier System",
            false,
            93)]
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
                    ? $"Milestone 7B validation passed with {passes} checks."
                    : $"Milestone 7B validation failed with {errors} error(s). See the Console.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();
            ValidateTypes(results);
            ValidatePrefab(results);
            ValidateScene(results);
            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateTypes(ICollection<ProjectValidationResult> results)
        {
            bool reportHasTier = typeof(AfterActionReport)
                .GetProperty(nameof(AfterActionReport.Tier)) != null;
            bool reportHasCategories = typeof(AfterActionReport)
                .GetProperty(nameof(AfterActionReport.Categories)) != null;
            bool evidenceIsSearchAware = typeof(ActorEvidenceSnapshot)
                .GetProperty(nameof(ActorEvidenceSnapshot.Searched)) != null
                && typeof(ActorEvidenceSnapshot)
                    .GetProperty(nameof(ActorEvidenceSnapshot.WeaponSecured)) != null;
            Add(
                results,
                reportHasTier && reportHasCategories && evidenceIsSearchAware,
                "M7B Evidence Contract",
                reportHasTier && reportHasCategories && evidenceIsSearchAware
                    ? "Final reports expose tiers, category scores, and search-aware evidence."
                    : "The Milestone 7B report or evidence contract is incomplete.");

            bool weightsTotalOneHundred = Enum.GetValues(typeof(MissionScoreCategoryType)).Length == 7
                && AfterActionEvaluator.DetermineTier(100) == MissionPerformanceTier.S
                && AfterActionEvaluator.DetermineTier(59) == MissionPerformanceTier.F;
            Add(
                results,
                weightsTotalOneHundred,
                "M7B Tier Rules",
                weightsTotalOneHundred
                    ? "Seven scored categories and the S-through-F tier boundary are available."
                    : "Tier/category rules are missing or inconsistent.");
        }

        private static void ValidatePrefab(ICollection<ProjectValidationResult> results)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneSevenBSetup.PresentationPrefabPath);
            MissionAfterActionPresentation presentation = prefab != null
                ? prefab.GetComponent<MissionAfterActionPresentation>()
                : null;
            Add(
                results,
                presentation != null,
                "M7B Report Prefab",
                presentation != null
                    ? "The final after-action presentation prefab exists."
                    : "The final after-action presentation prefab is missing. Run setup.");
        }

        private static void ValidateScene(ICollection<ProjectValidationResult> results)
        {
            Scene scene = SceneManager.GetSceneByPath(ProjectInfo.PrototypeScenePath);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened)
            {
                scene = EditorSceneManager.OpenScene(
                    ProjectInfo.PrototypeScenePath,
                    OpenSceneMode.Additive);
            }

            try
            {
                MissionController controller = FindInScene<MissionController>(scene);
                Add(
                    results,
                    controller != null
                        && controller.AutoCompleteWhenResolved
                        && Mathf.Approximately(
                            controller.AutoCompletionConfirmationSeconds,
                            RulesOfEntryMilestoneSevenBSetup.AutoCompletionConfirmationSeconds),
                    "M7B Automatic Completion",
                    controller != null && controller.AutoCompleteWhenResolved
                        ? "The mission auto-completes after a stable all-clear confirmation."
                        : "The mission controller is not configured for automatic completion.");

                bool timingValid = controller != null
                    && controller.Definition != null
                    && Mathf.Approximately(
                        controller.Definition.TargetCompletionSeconds,
                        RulesOfEntryMilestoneSevenBSetup.TargetCompletionSeconds)
                    && Mathf.Approximately(
                        controller.Definition.MaximumScoredCompletionSeconds,
                        RulesOfEntryMilestoneSevenBSetup.MaximumScoredCompletionSeconds);
                Add(
                    results,
                    timingValid,
                    "M7B Mission Timing",
                    timingValid
                        ? "Pressure Point has authored target and maximum scored times."
                        : "Mission performance timing is missing or incorrect.");

                MissionAfterActionPresentation presentation = scene.GetRootGameObjects()
                    .Where(root => string.Equals(
                        root.name,
                        RulesOfEntryMilestoneSevenBSetup.PresentationRootName,
                        StringComparison.Ordinal))
                    .Select(root => root.GetComponent<MissionAfterActionPresentation>())
                    .FirstOrDefault();
                Add(
                    results,
                    presentation != null
                        && presentation.HasCompleteConfiguration
                        && presentation.MissionController == controller,
                    "M7B Scene Presentation",
                    presentation != null && presentation.HasCompleteConfiguration
                        ? "The scene report presentation is fully wired to the mission controller."
                        : "The scene report presentation is missing or incompletely wired.");
            }
            finally
            {
                if (opened && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
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
                    ProjectLog.Error("M7B Validation", message);
                }
                else
                {
                    ProjectLog.Info("M7B Validation", message);
                }
            }
        }
    }
}
