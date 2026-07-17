using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone5;
using RulesOfEntry.Missions;
using RulesOfEntry.UI;
using RulesOfEntry.UI.FrontEnd;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.UiPresentation
{
    public static class RulesOfEntryUiPresentationValidator
    {
        private const string MenuPath =
            "Tools/Rules of Entry/UI Presentation/Validate Front End and HUD";

        private static readonly string[] RequiredHudRoots =
        {
            "ROE_InteractionPromptUI",
            "ROE_WeaponStatusUI",
            "ROE_HumanBehaviorDebugUI",
            "ROE_OfficerCommandDebugUI",
            "ROE_MissionAfterActionDebugUI"
        };

        [MenuItem(MenuPath, false, 71)]
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
                ? $"UI Presentation validation passed with {passes} checks and {warnings} warning(s)."
                : $"UI Presentation validation failed with {errors} error(s), {warnings} warning(s), and {passes} passing checks.";
            EditorUtility.DisplayDialog(
                ProjectInfo.GameTitle,
                summary + "\n\nSee the Console for details.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(
            bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();
            results.AddRange(RulesOfEntryMilestoneFiveValidator.RunValidation(false));
            ValidateIdentity(results);
            ValidateArtwork(results);
            ValidateBuildScenes(results);
            ValidateFrontEndScene(results);
            ValidatePrototypePresentation(results);

            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateIdentity(
            ICollection<ProjectValidationResult> results)
        {
            bool valid = string.Equals(
                    ProjectInfo.StudioName,
                    "Trooper Studios",
                    StringComparison.Ordinal)
                && ProjectInfo.CurrentMilestone.IndexOf(
                    "Front-End",
                    StringComparison.Ordinal) >= 0
                && string.Equals(
                    PlayerSettings.companyName,
                    ProjectInfo.StudioName,
                    StringComparison.Ordinal)
                && string.Equals(
                    PlayerSettings.productName,
                    "RoE v3",
                    StringComparison.Ordinal);
            if (!valid)
            {
                AddError(
                    results,
                    "UI Project Identity",
                    "Studio must be Trooper Studios, the milestone must identify the front end, and the validated RoE v3 product name must remain unchanged.");
                return;
            }

            AddPass(
                results,
                "UI Project Identity",
                "Trooper Studios presentation metadata is configured without breaking the validated project identity.");
        }

        private static void ValidateBuildScenes(
            ICollection<ProjectValidationResult> results)
        {
            EditorBuildSettingsScene[] enabled = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .ToArray();
            bool valid = enabled.Length >= 3
                && string.Equals(
                    enabled[0].path,
                    ProjectInfo.FrontEndScenePath,
                    StringComparison.Ordinal)
                && string.Equals(
                    enabled[1].path,
                    ProjectInfo.HeadquartersScenePath,
                    StringComparison.Ordinal)
                && string.Equals(
                    enabled[2].path,
                    ProjectInfo.PrototypeScenePath,
                    StringComparison.Ordinal);
            if (!valid)
            {
                AddError(
                    results,
                    "UI Build Scene Order",
                    "The enabled build scene order must start with ROE_FrontEnd, ROE_Headquarters, then ROE_Prototype.");
                return;
            }

            AddPass(
                results,
                "UI Build Scene Order",
                "The authored splash and menu lead into the headquarters before training or operation scenes.");
        }

        private static void ValidateArtwork(
            ICollection<ProjectValidationResult> results)
        {
            string[] requiredArtwork =
            {
                RulesOfEntryUiPresentationSetup.SplashArtworkPath,
                RulesOfEntryUiPresentationSetup.WarningArtworkPath,
                RulesOfEntryUiPresentationSetup.MenuArtworkPath
            };
            string[] missing = requiredArtwork
                .Where(path => AssetDatabase.LoadAssetAtPath<Sprite>(path) == null)
                .ToArray();
            Font typography = AssetDatabase.LoadAssetAtPath<Font>(
                RulesOfEntryUiPresentationSetup.TypographyPath);
            if (missing.Length > 0 || typography == null)
            {
                AddError(
                    results,
                    "UI Branding Artwork",
                    "Required splash, warning, menu artwork, or licensed typography is missing. Sprite problems: "
                        + string.Join(", ", missing) + ".");
                return;
            }

            AddPass(
                results,
                "UI Branding Artwork",
                "Trooper Studios splash, photosensitivity warning, original tactical menu artwork, and licensed condensed typography are available.");
        }

        private static void ValidateFrontEndScene(
            ICollection<ProjectValidationResult> results)
        {
            string path = ProjectInfo.FrontEndScenePath;
            if (!File.Exists(path))
            {
                AddError(results, "UI Front-End Scene", $"Missing {path}.");
                return;
            }

            Scene scene = SceneManager.GetSceneByPath(path);
            bool openedForValidation = !scene.IsValid() || !scene.isLoaded;
            if (openedForValidation)
            {
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }

            try
            {
                FrontEndFlowController controller = FindInScene<FrontEndFlowController>(scene);
                EventSystem eventSystem = FindInScene<EventSystem>(scene);
                Canvas canvas = FindInScene<Canvas>(scene);
                InputSystemUIInputModule inputModule = eventSystem != null
                    ? eventSystem.GetComponent<InputSystemUIInputModule>()
                    : null;
                bool legacyUiModule = eventSystem != null
                    && eventSystem.GetComponents<BaseInputModule>().Any(module =>
                        module != null
                        && string.Equals(
                            module.GetType().Name,
                            "StandaloneInputModule",
                            StringComparison.Ordinal));
                bool hasMissingScripts = HasMissingScripts(scene);
                Text[] frontEndText = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Text>(true))
                    .ToArray();
                bool largeTypeVisible = frontEndText
                        .Where(text => string.Equals(
                                text.name,
                                "TitleRules",
                                StringComparison.Ordinal)
                            || string.Equals(
                                text.name,
                                "TitleEntry",
                                StringComparison.Ordinal)
                            || string.Equals(
                                text.name,
                                "LoadingDestination",
                                StringComparison.Ordinal))
                        .Count(text => text.verticalOverflow == VerticalWrapMode.Overflow)
                    == 5;
                FrontEndMenuItemVisual[] flatMenuItems = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<FrontEndMenuItemVisual>(true))
                    .ToArray();
                Button continueCampaign = flatMenuItems
                    .Select(item => item.GetComponent<Button>())
                    .FirstOrDefault(button => button != null && string.Equals(
                        button.name,
                        "ContinueCampaignButton",
                        StringComparison.Ordinal));
                Button newCampaign = flatMenuItems
                    .Select(item => item.GetComponent<Button>())
                    .FirstOrDefault(button => button != null && string.Equals(
                        button.name,
                        "NewCampaignButton",
                        StringComparison.Ordinal));
                Button operations = flatMenuItems
                    .Select(item => item.GetComponent<Button>())
                    .FirstOrDefault(button => button != null && string.Equals(
                        button.name,
                        "OperationsButton",
                        StringComparison.Ordinal));
                Button training = flatMenuItems
                    .Select(item => item.GetComponent<Button>())
                    .FirstOrDefault(button => button != null && string.Equals(
                        button.name,
                        "TrainingButton",
                        StringComparison.Ordinal));
                bool flatMenuValid = flatMenuItems.Length == 7
                    && continueCampaign != null
                    && !continueCampaign.interactable
                    && newCampaign != null
                    && !newCampaign.interactable
                    && operations != null
                    && operations.interactable
                    && training != null
                    && training.interactable
                    && !scene.GetRootGameObjects()
                        .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                        .Any(transform => string.Equals(
                                transform.name,
                                "OperationCard",
                                StringComparison.Ordinal)
                            || string.Equals(
                                transform.name,
                                "Horizon",
                                StringComparison.Ordinal)
                            || transform.name.StartsWith(
                                "TacticalGrid_",
                                StringComparison.Ordinal));
                string[] dependencies = AssetDatabase.GetDependencies(path, true);
                MissionDefinition operationDefinition =
                    AssetDatabase.LoadAssetAtPath<MissionDefinition>(
                        RulesOfEntryMilestoneFiveSetup.MissionDefinitionPath);
                bool missionDestinationSaved = operationDefinition != null
                    && dependencies.Contains(
                        RulesOfEntryMilestoneFiveSetup.MissionDefinitionPath,
                        StringComparer.Ordinal)
                    && string.Equals(
                        controller != null ? controller.OperationDisplayName : string.Empty,
                        "Calder City Police Department",
                        StringComparison.Ordinal);
                bool artworkSaved = dependencies.Contains(
                        RulesOfEntryUiPresentationSetup.SplashArtworkPath,
                        StringComparer.Ordinal)
                    && dependencies.Contains(
                        RulesOfEntryUiPresentationSetup.WarningArtworkPath,
                        StringComparer.Ordinal)
                    && dependencies.Contains(
                        RulesOfEntryUiPresentationSetup.MenuArtworkPath,
                        StringComparer.Ordinal)
                    && dependencies.Contains(
                        RulesOfEntryUiPresentationSetup.TypographyPath,
                        StringComparer.Ordinal);
                bool valid = controller != null
                    && controller.HasCompleteConfiguration
                    && string.Equals(
                        controller.OperationScenePath,
                        ProjectInfo.HeadquartersScenePath,
                        StringComparison.Ordinal)
                    && eventSystem != null
                    && inputModule != null
                    && canvas != null
                    && canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    && artworkSaved
                    && flatMenuValid
                    && largeTypeVisible
                    && missionDestinationSaved
                    && !legacyUiModule
                    && !hasMissingScripts;
                if (!valid)
                {
                    AddError(
                        results,
                        "UI Front-End Scene",
                        "Front-end scene requires saved artwork, visible large typography, the seven-item flat menu contract, the headquarters campaign destination, a complete flow controller, overlay canvas, Input System UI module, and no missing or legacy components.");
                    return;
                }

                AddPass(
                    results,
                    "UI Front-End Scene",
                    "Splash, title, seven-item cinematic menu, settings, credits, loading, and Input System navigation are saved and connected.");
            }
            finally
            {
                if (openedForValidation && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void ValidatePrototypePresentation(
            ICollection<ProjectValidationResult> results)
        {
            string path = ProjectInfo.PrototypeScenePath;
            if (!File.Exists(path))
            {
                AddError(results, "UI Prototype HUD", $"Missing {path}.");
                return;
            }

            Scene scene = SceneManager.GetSceneByPath(path);
            bool openedForValidation = !scene.IsValid() || !scene.isLoaded;
            if (openedForValidation)
            {
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }

            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                GameObject presentationRoot = roots.FirstOrDefault(root =>
                    string.Equals(
                        root.name,
                        RulesOfEntryUiPresentationSetup.PresentationRootName,
                        StringComparison.Ordinal));
                PrototypePresentationController controller = presentationRoot != null
                    ? presentationRoot.GetComponent<PrototypePresentationController>()
                    : null;
                string[] missingRoots = RequiredHudRoots
                    .Where(required => !roots.Any(root => string.Equals(
                        root.name,
                        required,
                        StringComparison.Ordinal)))
                    .ToArray();
                bool valid = controller != null
                    && controller.HasCompleteConfiguration
                    && missingRoots.Length == 0
                    && !HasMissingScripts(scene);
                if (!valid)
                {
                    string rootDetails = missingRoots.Length == 0
                        ? string.Empty
                        : " Missing roots: " + string.Join(", ", missingRoots) + ".";
                    AddError(
                        results,
                        "UI Prototype HUD",
                        "Prototype must retain every functional HUD root and have a complete presentation controller with no missing scripts."
                            + rootDetails);
                    return;
                }

                AddPass(
                    results,
                    "UI Prototype HUD",
                    "Interaction, weapon, officer, mission, and optional diagnostic displays retain their scene roots under the unified visual treatment.");
            }
            finally
            {
                if (openedForValidation && scene.IsValid())
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

        private static bool HasMissingScripts(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.GetComponents<Component>().Any(component => component == null))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void LogResults(
            IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                string message = $"{result.Check}: {result.Message}";
                switch (result.Severity)
                {
                    case ProjectValidationSeverity.Error:
                        ProjectLog.Error("UI Validation", message);
                        break;
                    case ProjectValidationSeverity.Warning:
                        ProjectLog.Warning("UI Validation", message);
                        break;
                    default:
                        ProjectLog.Info("UI Validation", message);
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
