using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RulesOfEntry.Campaign;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone7C;
using RulesOfEntry.Editor.TacticalHud;
using RulesOfEntry.UI;
using RulesOfEntry.UI.FrontEnd;
using RulesOfEntry.UI.Headquarters;
using RulesOfEntry.UI.TacticalHud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone7D
{
    public static class RulesOfEntryMilestoneSevenDValidator
    {
        private const string SaveServicePath =
            "Assets/_Project/RulesOfEntry/Runtime/Campaign/CampaignSaveService.cs";

        [MenuItem(
            "Tools/Rules of Entry/Milestone 7D/Validate Campaign Saves and Operations Archive",
            false,
            97)]
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
                    ? $"Milestone 7D validation passed with {passes} checks."
                    : $"Milestone 7D validation failed with {errors} error(s). See the Console.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();
            results.AddRange(RulesOfEntryMilestoneSevenCValidator.RunValidation(false));
            ValidateSaveContract(results);
            ValidatePlayerPrefab(results);
            ValidateFrontEnd(results);
            ValidateIdentityScene(ProjectInfo.HeadquartersScenePath, "M7D HQ Identity", results);
            ValidateIdentityScene(ProjectInfo.PrototypeScenePath, "M7D Operation Identity", results);
            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateSaveContract(
            ICollection<ProjectValidationResult> results)
        {
            bool contractValid = CampaignDataRules.CurrentSchemaVersion == 1
                && Attribute.IsDefined(
                    typeof(CampaignSaveData),
                    typeof(SerializableAttribute))
                && Attribute.IsDefined(
                    typeof(CampaignOperationRecordData),
                    typeof(SerializableAttribute))
                && typeof(CampaignSaveData).GetField(
                    nameof(CampaignSaveData.completedOperations)) != null
                && ProjectInfo.CurrentMilestone.IndexOf(
                    "7D",
                    StringComparison.Ordinal) >= 0;
            Add(
                results,
                contractValid,
                "M7D Versioned Save Contract",
                contractValid
                    ? "Campaign schema 1 owns officer identity and completed-operation history."
                    : "The Milestone 7D campaign schema or project identity is incomplete.");

            bool architectureValid = File.Exists(SaveServicePath);
            if (architectureValid)
            {
                string source = File.ReadAllText(SaveServicePath);
                string[] forbidden =
                {
                    "AfterActionEvaluator",
                    "SceneManager",
                    "GameObject",
                    "Transform",
                    "MonoBehaviour",
                    "ScriptableObject"
                };
                architectureValid = forbidden.All(token =>
                    source.IndexOf(token, StringComparison.Ordinal) < 0);
            }

            Add(
                results,
                architectureValid,
                "M7D Persistence Boundary",
                architectureValid
                    ? "Campaign persistence owns files and immutable record DTOs, not scene objects or scoring."
                    : "CampaignSaveService must not own scene objects or recalculate mission scores.");
        }

        private static void ValidatePlayerPrefab(
            ICollection<ProjectValidationResult> results)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryTacticalHudSetup.PlayerPrefabPath);
            CampaignBodyCameraIdentityBinder binder = prefab != null
                ? prefab.GetComponent<CampaignBodyCameraIdentityBinder>()
                : null;
            Add(
                results,
                binder != null && binder.HasCompleteConfiguration,
                "M7D Player Identity Projection",
                binder != null && binder.HasCompleteConfiguration
                    ? "The reusable player projects active campaign identity into the body-camera HUD."
                    : "The player prefab campaign identity binder is missing or incomplete.");
        }

        private static void ValidateFrontEnd(
            ICollection<ProjectValidationResult> results)
        {
            ValidateScene(
                ProjectInfo.FrontEndScenePath,
                "M7D Campaign Front End",
                scene =>
                {
                    CampaignFrontEndController controller =
                        FindInScene<CampaignFrontEndController>(scene);
                    Button newCampaign = FindNamed<Button>(scene, "NewCampaignButton");
                    Button continueCampaign =
                        FindNamed<Button>(scene, "ContinueCampaignButton");
                    InputField[] fields = scene.GetRootGameObjects()
                        .SelectMany(root => root.GetComponentsInChildren<InputField>(true))
                        .ToArray();
                    return controller != null
                        && controller.HasCompleteConfiguration
                        && newCampaign != null
                        && newCampaign.interactable
                        && continueCampaign != null
                        && fields.Length >= 2
                        && !HasMissingScripts(scene);
                },
                "New Campaign and Continue Campaign are wired to a two-field personnel record and versioned save service.",
                "Rebuild Milestone 7D: the campaign menu, identity fields, or saved references are incomplete.",
                results);
        }

        private static void ValidateIdentityScene(
            string scenePath,
            string check,
            ICollection<ProjectValidationResult> results)
        {
            ValidateScene(
                scenePath,
                check,
                scene =>
                {
                    CampaignBodyCameraIdentityBinder binder =
                        FindInScene<CampaignBodyCameraIdentityBinder>(scene);
                    HeadquartersAfterActionReviewController archive =
                        FindInScene<HeadquartersAfterActionReviewController>(scene);
                    bool archiveValid = !string.Equals(
                            scenePath,
                            ProjectInfo.HeadquartersScenePath,
                            StringComparison.Ordinal)
                        || archive != null && archive.HasCompleteConfiguration;
                    bool operationOverlayValid = !string.Equals(
                            scenePath,
                            ProjectInfo.PrototypeScenePath,
                            StringComparison.Ordinal)
                        || IsFinalReportAboveOperationUi(scene);
                    return binder != null
                        && binder.HasCompleteConfiguration
                        && archiveValid
                        && operationOverlayValid
                        && !HasMissingScripts(scene);
                },
                "Campaign identity is connected, the archive is complete, and the final report owns the top UI layer.",
                "The identity binder, headquarters archive, final-report UI priority, or script references are incomplete.",
                results);
        }

        private static bool IsFinalReportAboveOperationUi(Scene scene)
        {
            MissionAfterActionPresentation report =
                FindInScene<MissionAfterActionPresentation>(scene);
            Canvas reportCanvas = report != null ? report.GetComponent<Canvas>() : null;
            if (reportCanvas == null)
            {
                return false;
            }

            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Canvas>(true))
                .Where(canvas => canvas != reportCanvas
                    && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                .All(canvas => reportCanvas.sortingOrder > canvas.sortingOrder);
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

        private static T FindNamed<T>(Scene scene, string name) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .FirstOrDefault(component => string.Equals(
                    component.name,
                    name,
                    StringComparison.Ordinal));
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
                    ProjectLog.Error("M7D Validation", message);
                }
                else if (result.Severity == ProjectValidationSeverity.Warning)
                {
                    ProjectLog.Warning("M7D Validation", message);
                }
                else
                {
                    ProjectLog.Info("M7D Validation", message);
                }
            }
        }
    }
}
