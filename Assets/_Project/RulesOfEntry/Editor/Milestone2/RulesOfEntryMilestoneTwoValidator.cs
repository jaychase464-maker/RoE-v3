using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RulesOfEntry.Combat;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone1;
using RulesOfEntry.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RulesOfEntry.Editor.Milestone2
{
    public static class RulesOfEntryMilestoneTwoValidator
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 2/Validate Weapon Prototype";
        private const string RangeRootName = "[Milestone2_Range]";
        private const string WeaponUiSourcePath =
            "Assets/_Project/RulesOfEntry/Runtime/UI/WeaponStatusUI.cs";

        private static readonly string[] RequiredWeaponActions =
        {
            "Fire",
            "Aim",
            "Reload",
            "CheckMagazine",
            "ToggleReady",
            "CycleFireSelector",
            "CycleAction",
            "EmergencyReloadModifier"
        };

        [MenuItem(MenuPath, false, 31)]
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
                ? $"Milestone 2 validation passed with {passes} checks and {warnings} warning(s)."
                : $"Milestone 2 validation failed with {errors} error(s), {warnings} warning(s), and {passes} passing checks.";
            EditorUtility.DisplayDialog(
                ProjectInfo.GameTitle,
                summary + "\n\nSee the Console for details.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();
            results.AddRange(RulesOfEntryMilestoneOneValidator.RunValidation(false));

            ValidateWeaponInput(results);
            ValidateDefinitions(results);
            ValidatePlayerPrefab(results);
            ValidateGeneratedPrefabs(results);
            ValidateNoRoundCounter(results);
            ValidatePrototypeScene(results);

            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateWeaponInput(ICollection<ProjectValidationResult> results)
        {
            InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                RulesOfEntryMilestoneTwoSetup.InputAssetPath);
            InputActionMap playerMap = inputAsset != null
                ? inputAsset.FindActionMap("Player", false)
                : null;
            string[] missing = RequiredWeaponActions
                .Where(actionName => playerMap == null || playerMap.FindAction(actionName, false) == null)
                .ToArray();

            if (missing.Length > 0)
            {
                AddError(
                    results,
                    "M2 Weapon Input",
                    "Missing actions: " + string.Join(", ", missing) + ".");
                return;
            }

            AddPass(
                results,
                "M2 Weapon Input",
                "Fire, aim, manual reload, magazine check, posture, selector, and action-cycle inputs exist.");
        }

        private static void ValidateDefinitions(ICollection<ProjectValidationResult> results)
        {
            FirearmDefinition firearm = AssetDatabase.LoadAssetAtPath<FirearmDefinition>(
                RulesOfEntryMilestoneTwoSetup.FirearmDefinitionPath);
            AmmunitionDefinition ammunition = AssetDatabase.LoadAssetAtPath<AmmunitionDefinition>(
                RulesOfEntryMilestoneTwoSetup.AmmunitionDefinitionPath);

            bool valid = firearm != null
                && ammunition != null
                && firearm.MagazineCapacity == 30
                && firearm.MinimumSecondsBetweenShots > 0f
                && firearm.RetainedReloadDuration > firearm.EmergencyReloadDuration
                && firearm.WeaponRaiseDuration > 0f
                && ammunition.ProjectileMassGrains > 0f
                && ammunition.MuzzleVelocityMetersPerSecond > 0f
                && ammunition.MuzzleEnergyJoules > 0f;
            if (!valid)
            {
                AddError(
                    results,
                    "M2 Equipment Definitions",
                    "The firearm or ammunition definition is missing or mechanically invalid.");
                return;
            }

            AddPass(
                results,
                "M2 Equipment Definitions",
                $"{firearm.DisplayName} and {ammunition.DisplayName} definitions are valid.");
        }

        private static void ValidatePlayerPrefab(ICollection<ProjectValidationResult> results)
        {
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneTwoSetup.PlayerPrefabPath);
            FirearmController controller = player != null
                ? player.GetComponent<FirearmController>()
                : null;
            bool valid = player != null
                && controller != null
                && player.GetComponent<UseOfForceEventLedger>() != null
                && player.GetComponentInChildren<FirearmView>(true) != null
                && player.GetComponentInChildren<FirearmView>(true).Muzzle != null
                && controller.HasCompleteConfiguration
                && !controller.AutomaticReloadEnabled;

            if (!valid)
            {
                AddError(
                    results,
                    "M2 Player Weapon",
                    "Player prefab must contain the firearm controller, graybox view, muzzle, force ledger, and disabled automatic reload.");
                return;
            }

            AddPass(
                results,
                "M2 Player Weapon",
                "Manual firearm, physical muzzle, and immutable force-event ledger are wired.");
        }

        private static void ValidateGeneratedPrefabs(ICollection<ProjectValidationResult> results)
        {
            GameObject target = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneTwoSetup.TargetPrefabPath);
            GameObject weaponUi = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneTwoSetup.WeaponUiPrefabPath);
            bool valid = target != null
                && target.GetComponent<PrototypeBallisticTarget>() != null
                && target.GetComponentInChildren<Collider>(true) != null
                && weaponUi != null
                && weaponUi.GetComponent<Canvas>() != null
                && weaponUi.GetComponent<WeaponStatusUI>() != null;

            if (!valid)
            {
                AddError(
                    results,
                    "M2 Generated Prefabs",
                    "Ballistic target or weapon-status UI prefab is missing or incomplete.");
                return;
            }

            AddPass(
                results,
                "M2 Generated Prefabs",
                "Ballistic target and uncertainty-based weapon UI prefabs exist.");
        }

        private static void ValidateNoRoundCounter(ICollection<ProjectValidationResult> results)
        {
            if (!File.Exists(WeaponUiSourcePath))
            {
                AddError(results, "M2 Ammunition Uncertainty", $"Missing {WeaponUiSourcePath}.");
                return;
            }

            string source = File.ReadAllText(WeaponUiSourcePath);
            string[] forbiddenPlayerFacingTokens =
            {
                "InsertedMagazineRounds",
                "RoundCount",
                "AmmoCount",
                "AmmunitionCount"
            };
            string[] found = forbiddenPlayerFacingTokens
                .Where(token => source.IndexOf(token, StringComparison.Ordinal) >= 0)
                .ToArray();
            if (found.Length > 0)
            {
                AddError(
                    results,
                    "M2 Ammunition Uncertainty",
                    "Player-facing weapon UI references exact ammunition state: "
                    + string.Join(", ", found)
                    + ".");
                return;
            }

            AddPass(
                results,
                "M2 Ammunition Uncertainty",
                "Player-facing UI contains no exact magazine or ammunition counter.");
        }

        private static void ValidatePrototypeScene(ICollection<ProjectValidationResult> results)
        {
            if (!File.Exists(ProjectInfo.PrototypeScenePath))
            {
                AddError(
                    results,
                    "M2 Prototype Scene",
                    $"Missing {ProjectInfo.PrototypeScenePath}.");
                return;
            }

            string[] dependencies = AssetDatabase.GetDependencies(
                ProjectInfo.PrototypeScenePath,
                true);
            string[] requiredDependencies =
            {
                RulesOfEntryMilestoneTwoSetup.TargetPrefabPath,
                RulesOfEntryMilestoneTwoSetup.WeaponUiPrefabPath,
                RulesOfEntryMilestoneTwoSetup.FirearmDefinitionPath,
                RulesOfEntryMilestoneTwoSetup.AmmunitionDefinitionPath
            };
            string[] missing = requiredDependencies
                .Where(path => !dependencies.Contains(path, StringComparer.Ordinal))
                .ToArray();
            string sceneText = File.ReadAllText(ProjectInfo.PrototypeScenePath);
            bool rangeMissing = sceneText.IndexOf(RangeRootName, StringComparison.Ordinal) < 0;

            if (missing.Length > 0 || rangeMissing)
            {
                string message = missing.Length > 0
                    ? "Missing dependencies: " + string.Join(", ", missing) + "."
                    : $"Missing generated scene root {RangeRootName}.";
                AddError(results, "M2 Prototype Scene", message);
                return;
            }

            AddPass(
                results,
                "M2 Prototype Scene",
                "Target range, weapon UI, equipment data, and player weapon dependencies exist.");
        }

        private static void LogResults(IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                string message = $"{result.Check}: {result.Message}";
                switch (result.Severity)
                {
                    case ProjectValidationSeverity.Error:
                        ProjectLog.Error("M2 Validation", message);
                        break;
                    case ProjectValidationSeverity.Warning:
                        ProjectLog.Warning("M2 Validation", message);
                        break;
                    default:
                        ProjectLog.Info("M2 Validation", message);
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
