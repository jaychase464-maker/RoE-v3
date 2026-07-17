using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Input;
using RulesOfEntry.Interaction;
using RulesOfEntry.Player;
using RulesOfEntry.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RulesOfEntry.Editor.Milestone1
{
    public static class RulesOfEntryMilestoneOneValidator
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 1/Validate Gameplay Prototype";
        private const string PlayerLayerName = "Player";
        private const string InteractableLayerName = "Interactable";

        private static readonly string[] RequiredPlayerActions =
        {
            "Move",
            "Look",
            "Sprint",
            "Crouch",
            "Interact"
        };

        private const string RequiredGrayboxRootName = "[Milestone1_Graybox]";

        [MenuItem(MenuPath, false, 21)]
        public static void ValidateFromMenu()
        {
            IReadOnlyList<ProjectValidationResult> results = RunValidation(true);
            int errorCount = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Error);
            int warningCount = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Warning);
            int passCount = results.Count(result =>
                result.Severity == ProjectValidationSeverity.Pass);

            string summary = errorCount == 0
                ? $"Milestone 1 validation passed with {passCount} checks and {warningCount} warning(s)."
                : $"Milestone 1 validation failed with {errorCount} error(s), {warningCount} warning(s), and {passCount} passing checks.";

            EditorUtility.DisplayDialog(
                ProjectInfo.GameTitle,
                summary + "\n\nSee the Console for details.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();
            results.AddRange(RulesOfEntryProjectValidator.RunValidation(false));

            ValidateLayers(results);
            ValidateInputActions(results);
            ValidatePlayerPrefab(results);
            ValidateInteractionPrefabs(results);
            ValidatePromptPrefab(results);
            ValidatePrototypeScene(results);

            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateLayers(ICollection<ProjectValidationResult> results)
        {
            int playerLayer = LayerMask.NameToLayer(PlayerLayerName);
            int interactableLayer = LayerMask.NameToLayer(InteractableLayerName);
            if (playerLayer < 0 || interactableLayer < 0)
            {
                AddError(
                    results,
                    "M1 Layers",
                    "Player and Interactable layers are required. Run the Milestone 1 setup tool.");
                return;
            }

            if (playerLayer == interactableLayer)
            {
                AddError(results, "M1 Layers", "Player and Interactable must use separate layers.");
                return;
            }

            AddPass(
                results,
                "M1 Layers",
                $"Player uses layer {playerLayer}; Interactable uses layer {interactableLayer}.");
        }

        private static void ValidateInputActions(ICollection<ProjectValidationResult> results)
        {
            InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                RulesOfEntryMilestoneOneSetup.InputAssetPath);
            if (asset == null)
            {
                AddError(
                    results,
                    "M1 Input Actions",
                    $"Missing {RulesOfEntryMilestoneOneSetup.InputAssetPath}.");
                return;
            }

            InputActionMap playerMap = asset.FindActionMap("Player", false);
            InputActionMap systemMap = asset.FindActionMap("System", false);
            List<string> missingActions = RequiredPlayerActions
                .Where(actionName => playerMap == null || playerMap.FindAction(actionName, false) == null)
                .ToList();

            if (systemMap == null || systemMap.FindAction("ToggleCursor", false) == null)
            {
                missingActions.Add("System/ToggleCursor");
            }

            if (missingActions.Count > 0)
            {
                AddError(
                    results,
                    "M1 Input Actions",
                    "Missing required actions: " + string.Join(", ", missingActions) + ".");
                return;
            }

            AddPass(
                results,
                "M1 Input Actions",
                "Player movement, look, sprint, crouch, interact, and cursor actions exist.");
        }

        private static void ValidatePlayerPrefab(ICollection<ProjectValidationResult> results)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneOneSetup.PlayerPrefabPath);
            if (prefab == null)
            {
                AddError(
                    results,
                    "M1 Player Prefab",
                    $"Missing {RulesOfEntryMilestoneOneSetup.PlayerPrefabPath}. Run the setup tool.");
                return;
            }

            Type[] requiredComponents =
            {
                typeof(CharacterController),
                typeof(PlayerInput),
                typeof(TacticalPlayerInput),
                typeof(FirstPersonMotor),
                typeof(FirstPersonLook),
                typeof(CursorStateController),
                typeof(PlayerInteractor)
            };
            string[] missingComponents = requiredComponents
                .Where(type => prefab.GetComponent(type) == null)
                .Select(type => type.Name)
                .ToArray();

            Camera camera = prefab.GetComponentInChildren<Camera>(true);
            AudioListener listener = prefab.GetComponentInChildren<AudioListener>(true);
            PlayerInput playerInput = prefab.GetComponent<PlayerInput>();
            InputActionAsset requiredInputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                RulesOfEntryMilestoneOneSetup.InputAssetPath);
            bool inputAssetMatches = playerInput != null && playerInput.actions == requiredInputAsset;
            bool layerMatches = prefab.layer == LayerMask.NameToLayer(PlayerLayerName);

            if (missingComponents.Length > 0
                || camera == null
                || listener == null
                || !inputAssetMatches
                || !layerMatches)
            {
                List<string> problems = new List<string>();
                if (missingComponents.Length > 0)
                {
                    problems.Add("missing " + string.Join(", ", missingComponents));
                }

                if (camera == null || listener == null)
                {
                    problems.Add("camera or AudioListener missing");
                }

                if (!inputAssetMatches)
                {
                    problems.Add("PlayerInput asset mismatch");
                }

                if (!layerMatches)
                {
                    problems.Add("Player layer mismatch");
                }

                AddError(results, "M1 Player Prefab", string.Join("; ", problems) + ".");
                return;
            }

            AddPass(
                results,
                "M1 Player Prefab",
                "Movement, look, input, cursor, camera, and interaction components are wired.");
        }

        private static void ValidateInteractionPrefabs(
            ICollection<ProjectValidationResult> results)
        {
            GameObject door = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneOneSetup.DoorPrefabPath);
            GameObject panel = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneOneSetup.PanelPrefabPath);

            bool doorValid = door != null
                && door.GetComponent<PrototypeDoor>() != null
                && door.GetComponentInChildren<Collider>(true) != null;
            bool panelValid = panel != null
                && panel.GetComponent<PrototypeControlPanel>() != null
                && panel.GetComponentInChildren<Collider>(true) != null;
            int interactableLayer = LayerMask.NameToLayer(InteractableLayerName);
            bool layersValid = doorValid
                && panelValid
                && door.layer == interactableLayer
                && panel.layer == interactableLayer;

            if (!doorValid || !panelValid || !layersValid)
            {
                AddError(
                    results,
                    "M1 Interaction Prefabs",
                    "Door and control-panel prefabs must contain their interaction scripts, colliders, and Interactable layer.");
                return;
            }

            AddPass(
                results,
                "M1 Interaction Prefabs",
                "Instant door and hold-to-use control-panel prefabs are configured.");
        }

        private static void ValidatePromptPrefab(ICollection<ProjectValidationResult> results)
        {
            GameObject prompt = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneOneSetup.PromptPrefabPath);
            bool valid = prompt != null
                && prompt.GetComponent<Canvas>() != null
                && prompt.GetComponent<CanvasGroup>() != null
                && prompt.GetComponent<InteractionPromptUI>() != null;

            if (!valid)
            {
                AddError(
                    results,
                    "M1 Interaction UI",
                    $"Missing or incomplete {RulesOfEntryMilestoneOneSetup.PromptPrefabPath}.");
                return;
            }

            AddPass(results, "M1 Interaction UI", "Interaction prompt prefab is configured.");
        }

        private static void ValidatePrototypeScene(ICollection<ProjectValidationResult> results)
        {
            if (!File.Exists(ProjectInfo.PrototypeScenePath))
            {
                AddError(
                    results,
                    "M1 Prototype Scene",
                    $"Missing {ProjectInfo.PrototypeScenePath}.");
                return;
            }

            string[] dependencies = AssetDatabase.GetDependencies(
                ProjectInfo.PrototypeScenePath,
                true);
            string[] requiredDependencies =
            {
                RulesOfEntryMilestoneOneSetup.PlayerPrefabPath,
                RulesOfEntryMilestoneOneSetup.DoorPrefabPath,
                RulesOfEntryMilestoneOneSetup.PanelPrefabPath,
                RulesOfEntryMilestoneOneSetup.PromptPrefabPath
            };
            string[] missingDependencies = requiredDependencies
                .Where(path => !dependencies.Contains(path, StringComparer.Ordinal))
                .ToArray();

            string serializedScene = File.ReadAllText(ProjectInfo.PrototypeScenePath);
            bool grayboxRootMissing = serializedScene.IndexOf(
                RequiredGrayboxRootName,
                StringComparison.Ordinal) < 0;

            if (missingDependencies.Length > 0 || grayboxRootMissing)
            {
                string message = missingDependencies.Length > 0
                    ? "Missing prefab dependencies: " + string.Join(", ", missingDependencies) + "."
                    : $"Missing generated scene root: {RequiredGrayboxRootName}.";
                AddError(results, "M1 Prototype Scene", message);
                return;
            }

            Scene loadedScene = SceneManager.GetSceneByPath(ProjectInfo.PrototypeScenePath);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                int activeCameras = loadedScene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Camera>(false))
                    .Count(camera => camera.gameObject.activeInHierarchy);
                int activeListeners = loadedScene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<AudioListener>(false))
                    .Count(listener => listener.gameObject.activeInHierarchy);
                if (activeCameras != 1 || activeListeners != 1)
                {
                    AddError(
                        results,
                        "M1 Prototype Scene",
                        $"Expected one active camera and AudioListener; found {activeCameras} camera(s) and {activeListeners} listener(s).");
                    return;
                }
            }

            AddPass(
                results,
                "M1 Prototype Scene",
                "Graybox, player, interaction examples, UI, and scene dependencies exist.");
        }

        private static void LogResults(IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                string message = $"{result.Check}: {result.Message}";
                switch (result.Severity)
                {
                    case ProjectValidationSeverity.Error:
                        ProjectLog.Error("M1 Validation", message);
                        break;
                    case ProjectValidationSeverity.Warning:
                        ProjectLog.Warning("M1 Validation", message);
                        break;
                    default:
                        ProjectLog.Info("M1 Validation", message);
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
