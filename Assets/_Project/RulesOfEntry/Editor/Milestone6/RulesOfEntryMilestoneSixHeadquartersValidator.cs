using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone5;
using RulesOfEntry.Editor.UiPresentation;
using RulesOfEntry.Headquarters;
using RulesOfEntry.Input;
using RulesOfEntry.Interaction;
using RulesOfEntry.Missions;
using RulesOfEntry.Officers;
using RulesOfEntry.Planning;
using RulesOfEntry.Player;
using RulesOfEntry.UI.Planning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone6
{
    public static class RulesOfEntryMilestoneSixHeadquartersValidator
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 6A/Validate Headquarters and Tablet Prototype";

        [MenuItem(MenuPath, false, 81)]
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
                ? $"Milestone 6A validation passed with {passes} checks and {warnings} warning(s)."
                : $"Milestone 6A validation failed with {errors} error(s), {warnings} warning(s), and {passes} passing checks.";
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
            ValidatePlanningAsset(results);
            ValidateHeadquartersScene(results);
            ValidateBuildSceneOrder(results);
            ValidateFrontEndDestination(results);
            ValidatePlanningArchitecture(results);

            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateIdentity(
            ICollection<ProjectValidationResult> results)
        {
            if (ProjectInfo.CurrentMilestone.IndexOf(
                    "6A",
                    StringComparison.Ordinal) < 0
                || string.IsNullOrWhiteSpace(ProjectInfo.HeadquartersScenePath))
            {
                AddError(
                    results,
                    "M6A Project Identity",
                    "ProjectInfo must identify Milestone 6A and the headquarters scene path.");
                return;
            }

            AddPass(
                results,
                "M6A Project Identity",
                "Project metadata identifies the headquarters and planning milestone.");
        }

        private static void ValidatePlanningAsset(
            ICollection<ProjectValidationResult> results)
        {
            OperationBriefingDefinition briefing =
                AssetDatabase.LoadAssetAtPath<OperationBriefingDefinition>(
                    RulesOfEntryMilestoneSixHeadquartersSetup.BriefingAssetPath);
            if (briefing == null || !briefing.HasValidConfiguration)
            {
                AddError(
                    results,
                    "M6A Operation Briefing",
                    "Pressure Point requires a valid mission, scene, entry plans, officer roster, and support roster.");
                return;
            }

            bool targetsPreserved = briefing.Mission.Objectives.Any(objective =>
                    objective.Type == MissionObjectiveType.SecureSubject
                    && string.Equals(
                        objective.TargetActorId,
                        "m3_suspect_01",
                        StringComparison.Ordinal))
                && briefing.Mission.Objectives.Any(objective =>
                    objective.Type == MissionObjectiveType.ProtectActor
                    && string.Equals(
                        objective.TargetActorId,
                        "m3_civilian_01",
                        StringComparison.Ordinal))
                && briefing.Mission.Objectives.Any(objective =>
                    objective.Type == MissionObjectiveType.VerifyRoomClear
                    && string.Equals(
                        objective.TargetRoomId,
                        "prototype_north_training_room",
                        StringComparison.Ordinal));
            bool planningContentValid = briefing.EntryPoints.Length == 3
                && briefing.Officers.Length >= 2
                && briefing.Officers.Count(officer => officer.Available) >= 2
                && briefing.SupportAssets.Any(support =>
                    support.Type == OperationSupportType.K9)
                && briefing.SupportAssets.Any(support =>
                    support.Type == OperationSupportType.Drone)
                && briefing.SupportAssets.All(support => !support.Implemented)
                && string.Equals(
                    briefing.ScenePath,
                    ProjectInfo.PrototypeScenePath,
                    StringComparison.Ordinal)
                && targetsPreserved;
            if (!planningContentValid)
            {
                AddError(
                    results,
                    "M6A Planning Content",
                    "The prototype must retain its evidence targets, expose three entries and two officers, and represent K9/drone support honestly as future systems.");
                return;
            }

            AddPass(
                results,
                "M6A Planning Content",
                "Pressure Point has three entry plans, a two-officer roster, future support definitions, and evidence-compatible mission targets.");
        }

        private static void ValidateHeadquartersScene(
            ICollection<ProjectValidationResult> results)
        {
            string path = ProjectInfo.HeadquartersScenePath;
            if (!File.Exists(path))
            {
                AddError(results, "M6A Headquarters Scene", $"Missing {path}.");
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
                GameObject headquartersRoot = roots.FirstOrDefault(root =>
                    string.Equals(
                        root.name,
                        RulesOfEntryMilestoneSixHeadquartersSetup.HeadquartersRootName,
                        StringComparison.Ordinal));
                RuggedTabletController tablet =
                    FindInScene<RuggedTabletController>(scene);
                HeadquartersMissionTerminalInteractable terminal =
                    FindInScene<HeadquartersMissionTerminalInteractable>(scene);
                TacticalPlayerInput playerInput = FindInScene<TacticalPlayerInput>(scene);
                OfficerSquadController squad = FindInScene<OfficerSquadController>(scene);
                CursorStateController cursor = FindInScene<CursorStateController>(scene);
                PlayerInteractor interactor = FindInScene<PlayerInteractor>(scene);
                EventSystem eventSystem = FindInScene<EventSystem>(scene);
                OperationBriefingDefinition authoritativeBriefing =
                    AssetDatabase.LoadAssetAtPath<OperationBriefingDefinition>(
                        RulesOfEntryMilestoneSixHeadquartersSetup.BriefingAssetPath);
                bool valid = headquartersRoot != null
                    && tablet != null
                    && tablet.HasCompleteConfiguration
                    && tablet.DefaultBriefing == authoritativeBriefing
                    && terminal != null
                    && terminal.HasCompleteConfiguration
                    && terminal.ReviewHoldSeconds <= 0f
                    && playerInput != null
                    && squad != null
                    && !squad.enabled
                    && cursor != null
                    && interactor != null
                    && eventSystem != null
                    && eventSystem.GetComponent<InputSystemUIInputModule>() != null
                    && !HasMissingScripts(scene);
                if (!valid)
                {
                    AddError(
                        results,
                        "M6A Headquarters Scene",
                        "Headquarters requires its greybox root, player, disabled operation-only squad command layer, immediate terminal interaction, authoritative default tablet briefing, Input System UI event system, and no missing scripts.");
                    return;
                }

                string[] dependencies = AssetDatabase.GetDependencies(path, true);
                bool dependenciesSaved = dependencies.Contains(
                        RulesOfEntryMilestoneSixHeadquartersSetup.BriefingAssetPath,
                        StringComparer.Ordinal)
                    && dependencies.Contains(
                        RulesOfEntryMilestoneSixHeadquartersSetup.TabletHardwareArtworkPath,
                        StringComparer.Ordinal)
                    && dependencies.Contains(
                        RulesOfEntry.Editor.Milestone1.RulesOfEntryMilestoneOneSetup.PlayerPrefabPath,
                        StringComparer.Ordinal);
                if (!dependenciesSaved)
                {
                    AddError(
                        results,
                        "M6A Headquarters Dependencies",
                        "The saved headquarters scene must reference the authoritative operation briefing, transparent rugged-tablet hardware cutout, and player prefab.");
                    return;
                }

                RectTransform device = FindNamedTransform(scene, "RuggedDevice")
                    ?.GetComponent<RectTransform>();
                Image deviceImage = device != null
                    ? device.GetComponent<Image>()
                    : null;
                AspectRatioFitter hardwareFitter = device != null
                    ? device.GetComponent<AspectRatioFitter>()
                    : null;
                RectTransform tacticalDisplay = FindNamedTransform(
                    scene,
                    "TacticalDisplay")?.GetComponent<RectTransform>();
                Image tabletInterfaceBackground = FindNamedTransform(
                    scene,
                    "TabletInterface")?.GetComponent<Image>();
                Text operationHeader = FindNamedTransform(scene, "OperationHeader")
                    ?.GetComponent<Text>();
                Text metadata = FindNamedTransform(scene, "OperationMetadata")
                    ?.GetComponent<Text>();
                Text leftBody = FindNamedTransform(scene, "LeftPanelBody")
                    ?.GetComponent<Text>();
                Text rightBody = FindNamedTransform(scene, "RightPanelBody")
                    ?.GetComponent<Text>();
                bool hardwareHasTransparentCorners = HasTransparentCorners(
                    RulesOfEntryMilestoneSixHeadquartersSetup.TabletHardwareArtworkPath);
                bool presentationValid = device != null
                    && deviceImage != null
                    && deviceImage.sprite != null
                    && hardwareHasTransparentCorners
                    && tabletInterfaceBackground != null
                    && tabletInterfaceBackground.color.a <= 0.01f
                    && hardwareFitter != null
                    && hardwareFitter.aspectMode
                        == AspectRatioFitter.AspectMode.FitInParent
                    && Mathf.Abs(hardwareFitter.aspectRatio - (1672f / 941f)) < 0.01f
                    && tacticalDisplay != null
                    && tacticalDisplay.anchorMin.x >= 0.20f
                    && tacticalDisplay.anchorMin.y >= 0.18f
                    && tacticalDisplay.anchorMax.x <= 0.80f
                    && tacticalDisplay.anchorMax.y <= 0.83f
                    && operationHeader != null
                    && metadata != null
                    && operationHeader.rectTransform.anchorMax.x
                        <= metadata.rectTransform.anchorMin.x
                    && leftBody != null
                    && rightBody != null
                    && leftBody.fontSize >= 18
                    && rightBody.fontSize >= 18;
                if (!presentationValid)
                {
                    AddError(
                        results,
                        "M6A Tablet Presentation",
                        "The tablet must use the transparent 1672x941 hardware-only cutout, align the live display inside its physical screen, keep header fields separated, and use readable mission-body typography.");
                    return;
                }

                AddPass(
                    results,
                    "M6A Tablet Presentation",
                    "The background-free, hand-free tablet hardware is aspect-safe, its live screen is aligned to the physical bezel, and the real headquarters remains visible behind it.");

                AddPass(
                    results,
                    "M6A Headquarters Scene",
                    "The playable PD greybox contains physical mission selection and a complete rugged-tablet planning interface.");
            }
            finally
            {
                if (openedForValidation && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void ValidateBuildSceneOrder(
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
                    "M6A Build Scene Order",
                    "Build Settings must begin with front end, headquarters, then the operation prototype.");
                return;
            }

            AddPass(
                results,
                "M6A Build Scene Order",
                "Campaign flow proceeds from the front end into headquarters before deployment.");
        }

        private static void ValidateFrontEndDestination(
            ICollection<ProjectValidationResult> results)
        {
            IReadOnlyList<ProjectValidationResult> uiResults =
                RulesOfEntryUiPresentationValidator.RunValidation(false);
            ProjectValidationResult[] uiErrors = uiResults
                .Where(result => result.Severity == ProjectValidationSeverity.Error)
                .ToArray();
            if (uiErrors.Length > 0)
            {
                AddError(
                    results,
                    "M6A Front-End Integration",
                    "The front end must pass after being redirected to headquarters. "
                        + string.Join(" | ", uiErrors.Select(error =>
                            $"{error.Check}: {error.Message}")));
                return;
            }

            AddPass(
                results,
                "M6A Front-End Integration",
                "Operations enters Calder City Police Headquarters while Training remains a direct prototype shortcut.");
        }

        private static void ValidatePlanningArchitecture(
            ICollection<ProjectValidationResult> results)
        {
            bool rulesPure = !typeof(MonoBehaviour).IsAssignableFrom(
                typeof(OperationPlanningRules));
            bool contextStatic = typeof(OperationDeploymentContext).IsAbstract
                && typeof(OperationDeploymentContext).IsSealed;
            bool terminalUsesInteractionContract = typeof(InteractableBehaviour)
                .IsAssignableFrom(typeof(HeadquartersMissionTerminalInteractable));
            if (!rulesPure || !contextStatic || !terminalUsesInteractionContract)
            {
                AddError(
                    results,
                    "M6A Planning Architecture",
                    "Planning rules must remain pure, deployment context identifier-only/static, and the mission terminal must use the established interaction contract.");
                return;
            }

            AddPass(
                results,
                "M6A Planning Architecture",
                "Pure planning rules and stable deployment identifiers separate headquarters UI from mission scene objects.");
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .FirstOrDefault();
        }

        private static Transform FindNamedTransform(Scene scene, string name)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(transform => string.Equals(
                    transform.name,
                    name,
                    StringComparison.Ordinal));
        }

        private static bool HasMissingScripts(Scene scene)
        {
            return scene.GetRootGameObjects().Any(root =>
                root.GetComponentsInChildren<Transform>(true).Any(transform =>
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        transform.gameObject) > 0));
        }

        private static bool HasTransparentCorners(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            Texture2D texture = new Texture2D(
                2,
                2,
                TextureFormat.RGBA32,
                false);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(path), false))
                {
                    return false;
                }

                int maxX = texture.width - 1;
                int maxY = texture.height - 1;
                return texture.GetPixel(0, 0).a <= 0.02f
                    && texture.GetPixel(maxX, 0).a <= 0.02f
                    && texture.GetPixel(0, maxY).a <= 0.02f
                    && texture.GetPixel(maxX, maxY).a <= 0.02f;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
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

        private static void LogResults(IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                switch (result.Severity)
                {
                    case ProjectValidationSeverity.Error:
                        ProjectLog.Error("M6A Validation", $"{result.Check}: {result.Message}");
                        break;
                    case ProjectValidationSeverity.Warning:
                        ProjectLog.Warning("M6A Validation", $"{result.Check}: {result.Message}");
                        break;
                    default:
                        ProjectLog.Info("M6A Validation", $"{result.Check}: {result.Message}");
                        break;
                }
            }
        }
    }
}
