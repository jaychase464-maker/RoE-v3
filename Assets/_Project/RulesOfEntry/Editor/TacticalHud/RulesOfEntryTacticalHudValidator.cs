using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Input;
using RulesOfEntry.Officers;
using RulesOfEntry.UI;
using RulesOfEntry.UI.TacticalHud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.TacticalHud
{
    public static class RulesOfEntryTacticalHudValidator
    {
        [MenuItem("Tools/Rules of Entry/Milestone 6B/Validate Tactical HUD", false, 82)]
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
                ? $"Tactical HUD validation passed with {passes} checks and {warnings} warning(s)."
                : $"Tactical HUD validation failed with {errors} error(s), {warnings} warning(s), and {passes} passing checks.";
            EditorUtility.DisplayDialog(
                ProjectInfo.GameTitle,
                summary + "\n\nSee the Console for details.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();
            ValidateInput(results);
            ValidatePlayerPrefab(results);
            ValidateOfficerPrefab(results, RulesOfEntryTacticalHudSetup.OfficerAlphaPrefabPath);
            ValidateOfficerPrefab(results, RulesOfEntryTacticalHudSetup.OfficerBravoPrefabPath);
            ValidateHudPrefab(results);
            ValidatePrototypeScene(results);

            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateInput(ICollection<ProjectValidationResult> results)
        {
            InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                RulesOfEntryTacticalHudSetup.InputAssetPath);
            InputAction menu = asset?.FindAction("Player/OfficerCommandMenu", false);
            bool menuValid = HasBinding(menu, "<Mouse>/middleButton");
            bool commandsValid = HasBinding(
                    asset?.FindAction("Player/OfficerMove", false),
                    "<Keyboard>/digit1")
                && HasBinding(
                    asset?.FindAction("Player/OfficerHold", false),
                    "<Keyboard>/digit2")
                && HasBinding(
                    asset?.FindAction("Player/OfficerStack", false),
                    "<Keyboard>/digit3")
                && HasBinding(
                    asset?.FindAction("Player/OfficerOpen", false),
                    "<Keyboard>/digit4")
                && HasBinding(
                    asset?.FindAction("Player/OfficerFollow", false),
                    "<Keyboard>/digit5")
                && HasBinding(
                    asset?.FindAction("Player/OfficerRestrain", false),
                    "<Keyboard>/digit6");
            if (!menuValid || !commandsValid)
            {
                AddError(
                    results,
                    "HUD Command Input",
                    "Middle mouse must hold the menu and number keys 1-6 must map to the six tactical commands.");
                return;
            }

            AddPass(
                results,
                "HUD Command Input",
                "The current Input System gates six numbered commands behind middle mouse.");
        }

        private static void ValidatePlayerPrefab(
            ICollection<ProjectValidationResult> results)
        {
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryTacticalHudSetup.PlayerPrefabPath);
            bool valid = player != null
                && player.GetComponent<TacticalPlayerInput>() != null
                && player.GetComponent<OfficerSquadController>() != null
                && player.GetComponent<BodyCameraIdentity>() != null
                && player.GetComponent<MissionClock>() != null;
            if (!valid)
            {
                AddError(
                    results,
                    "HUD Player Data",
                    "Player prefab requires tactical input, squad command, body-camera identity, and mission-clock components.");
                return;
            }

            AddPass(
                results,
                "HUD Player Data",
                "Campaign identity and the in-game mission clock feed the body-camera overlay.");
        }

        private static void ValidateOfficerPrefab(
            ICollection<ProjectValidationResult> results,
            string path)
        {
            GameObject officer = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            bool valid = officer != null
                && officer.GetComponent<TacticalOfficerController>() != null
                && officer.GetComponent<OfficerAmmunitionStatus>() != null;
            if (!valid)
            {
                AddError(
                    results,
                    "HUD Officer Feed",
                    $"{path} requires tactical control and qualitative ammunition status.");
                return;
            }

            AddPass(
                results,
                "HUD Officer Feed",
                $"{officer.name} exposes injury, activity, selection, and qualitative ammunition data.");
        }

        private static void ValidateHudPrefab(ICollection<ProjectValidationResult> results)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryTacticalHudSetup.HudPrefabPath);
            TacticalHudController hud = prefab != null
                ? prefab.GetComponent<TacticalHudController>()
                : null;
            Canvas canvas = prefab != null ? prefab.GetComponent<Canvas>() : null;
            TacticalHudOfficerRow row = prefab != null
                ? prefab.GetComponentInChildren<TacticalHudOfficerRow>(true)
                : null;
            LayoutElement rowLayout = row != null
                ? row.GetComponent<LayoutElement>()
                : null;
            Transform rosterPanelTransform = prefab != null
                ? prefab.transform.Find("SquadStatusPanel")
                : null;
            RectTransform rosterPanel = rosterPanelTransform != null
                ? rosterPanelTransform.GetComponent<RectTransform>()
                : null;
            TacticalHudRoundedPanelGraphic bodyCameraPanel = prefab != null
                ? prefab.GetComponentInChildren<TacticalHudRoundedPanelGraphic>(true)
                : null;
            TacticalHudShieldGraphic roeShield = prefab != null
                ? prefab.GetComponentInChildren<TacticalHudShieldGraphic>(true)
                : null;
            if (hud == null
                || row == null
                || !row.HasCompleteVisualConfiguration
                || rowLayout == null
                || rowLayout.preferredHeight > 48f
                || rosterPanel == null
                || rosterPanel.sizeDelta.x > 400f
                || canvas == null
                || canvas.sortingOrder < 200
                || bodyCameraPanel == null
                || roeShield == null)
            {
                AddError(
                    results,
                    "HUD Prefab",
                    "ROE_TacticalHUD must contain its controller, compact configured officer rows, narrow roster, high-priority Canvas, rounded body-camera shell, and shield-shaped RoE mark.");
                return;
            }

            AddPass(
                results,
                "HUD Prefab",
                "The HUD prefab contains the low-profile squad roster, body-camera block, and MMB command presentation.");
        }

        private static void ValidatePrototypeScene(
            ICollection<ProjectValidationResult> results)
        {
            Scene scene = SceneManager.GetSceneByPath(ProjectInfo.PrototypeScenePath);
            bool openedForValidation = !scene.IsValid() || !scene.isLoaded;
            if (openedForValidation)
            {
                scene = EditorSceneManager.OpenScene(
                    ProjectInfo.PrototypeScenePath,
                    OpenSceneMode.Additive);
            }

            try
            {
                TacticalHudController hud = FindInScene<TacticalHudController>(scene);
                OfficerCommandDebugUI legacy = FindInScene<OfficerCommandDebugUI>(scene);
                MissionAfterActionDebugUI missionDebug =
                    FindInScene<MissionAfterActionDebugUI>(scene);
                TextMesh[] worldLabels = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<TextMesh>(true))
                    .Where(text => string.Equals(
                            text.name,
                            "OfficerStatusLabel",
                            StringComparison.Ordinal)
                        || string.Equals(
                            text.name,
                            "AIStatusLabel",
                            StringComparison.Ordinal))
                    .ToArray();
                bool legacyHidden = (legacy == null || !legacy.gameObject.activeInHierarchy)
                    && (missionDebug == null || !missionDebug.gameObject.activeInHierarchy)
                    && worldLabels.All(label => !label.gameObject.activeInHierarchy);
                bool commandEntryPointExists = typeof(OfficerSquadController).GetMethod(
                    "IssueCommandSlot") != null;
                if (hud == null
                    || !hud.HasCompleteConfiguration
                    || !legacyHidden
                    || !commandEntryPointExists)
                {
                    AddError(
                        results,
                        "HUD Scene Integration",
                        "Prototype scene requires a configured Tactical HUD, direct command-slot entry point, and no superseded mission/officer/world-label diagnostics in normal play.");
                    return;
                }

                AddPass(
                    results,
                    "HUD Scene Integration",
                    "The operation scene uses live squad, campaign identity, mission time, and command context data.");
            }
            finally
            {
                if (openedForValidation && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static bool HasBinding(InputAction action, string path)
        {
            return action != null && action.bindings.Any(binding =>
                string.Equals(binding.path, path, StringComparison.OrdinalIgnoreCase));
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .FirstOrDefault();
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
                if (result.Severity == ProjectValidationSeverity.Error)
                {
                    ProjectLog.Error("HUD Validation", $"{result.Check}: {result.Message}");
                }
                else
                {
                    ProjectLog.Info("HUD Validation", $"{result.Check}: {result.Message}");
                }
            }
        }
    }
}
