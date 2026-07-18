using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Combat;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone2;
using RulesOfEntry.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace RulesOfEntry.Editor.Milestone3
{
    public static class RulesOfEntryMilestoneThreeValidator
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Milestone 3/Validate Human Behavior Prototype";
        private const string RequiredNavigationVersion = "2.0.14";
        private const string GeneratedRootName = "[Milestone3_HumanBehavior]";
        private const string RuntimeRoot = "Assets/_Project/RulesOfEntry/Runtime";

        [MenuItem(MenuPath, false, 41)]
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
                ? $"Milestone 3 validation passed with {passes} checks and {warnings} warning(s)."
                : $"Milestone 3 validation failed with {errors} error(s), {warnings} warning(s), and {passes} passing checks.";
            EditorUtility.DisplayDialog(
                ProjectInfo.GameTitle,
                summary + "\n\nSee the Console for details.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();
            results.AddRange(RulesOfEntryMilestoneTwoValidator.RunValidation(false));
            ValidateNavigationPackage(results);
            ValidateCommandInput(results);
            ValidateProfiles(results);
            ValidatePlayerCommandEmitter(results);
            ValidateActorPrefab(results, RulesOfEntryMilestoneThreeSetup.SuspectPrefabPath,
                ActorRole.Suspect);
            ValidateActorPrefab(results, RulesOfEntryMilestoneThreeSetup.CivilianPrefabPath,
                ActorRole.Civilian);
            ValidateNavigationData(results);
            ValidatePrototypeScene(results);
            ValidateAccountabilityBoundary(results);

            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateNavigationPackage(
            ICollection<ProjectValidationResult> results)
        {
            PackageInfo package = PackageInfo.GetAllRegisteredPackages().FirstOrDefault(
                candidate => candidate != null
                    && string.Equals(
                        candidate.name,
                        "com.unity.ai.navigation",
                        StringComparison.Ordinal));
            if (package == null)
            {
                AddError(
                    results,
                    "M3 AI Navigation",
                    "com.unity.ai.navigation is not installed. Reimport the Milestone 3 manifest and allow package resolution to finish.");
                return;
            }

            if (!string.Equals(package.version, RequiredNavigationVersion, StringComparison.Ordinal))
            {
                AddError(
                    results,
                    "M3 AI Navigation",
                    $"Expected com.unity.ai.navigation {RequiredNavigationVersion}; found {package.version}.");
                return;
            }

            AddPass(
                results,
                "M3 AI Navigation",
                $"com.unity.ai.navigation {package.version} is installed.");
        }

        private static void ValidateCommandInput(ICollection<ProjectValidationResult> results)
        {
            InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                RulesOfEntryMilestoneThreeSetup.InputAssetPath);
            InputAction action = inputAsset != null
                ? inputAsset.FindAction("Player/IssueCommand", false)
                : null;
            bool hasKeyboard = action != null && action.bindings.Any(
                binding => string.Equals(
                    binding.effectivePath,
                    "<Keyboard>/f",
                    StringComparison.OrdinalIgnoreCase));
            bool hasGamepad = action != null && action.bindings.Any(
                binding => string.Equals(
                    binding.effectivePath,
                    "<Gamepad>/leftShoulder",
                    StringComparison.OrdinalIgnoreCase));
            if (!hasKeyboard || !hasGamepad)
            {
                AddError(
                    results,
                    "M3 Verbal Command Input",
                    "Player/IssueCommand must bind keyboard F and gamepad left shoulder.");
                return;
            }

            AddPass(
                results,
                "M3 Verbal Command Input",
                "Police command input exists for keyboard/mouse and gamepad.");
        }

        private static void ValidateProfiles(ICollection<ProjectValidationResult> results)
        {
            HumanBehaviorProfile suspect = AssetDatabase.LoadAssetAtPath<HumanBehaviorProfile>(
                RulesOfEntryMilestoneThreeSetup.SuspectProfilePath);
            HumanBehaviorProfile civilian = AssetDatabase.LoadAssetAtPath<HumanBehaviorProfile>(
                RulesOfEntryMilestoneThreeSetup.CivilianProfilePath);
            bool valid = suspect != null
                && civilian != null
                && suspect.Deception > 0f
                && suspect.Aggression > civilian.Aggression
                && civilian.BaselineCompliance > suspect.BaselineCompliance
                && suspect.MaximumReactionSeconds >= suspect.MinimumReactionSeconds
                && civilian.MaximumReactionSeconds >= civilian.MinimumReactionSeconds;
            if (!valid)
            {
                AddError(
                    results,
                    "M3 Behavior Profiles",
                    "Suspect or civilian profile is missing or behaviorally invalid.");
                return;
            }

            AddPass(
                results,
                "M3 Behavior Profiles",
                "Distinct suspect and civilian profiles contain valid compliance, deception, panic, and reaction data.");
        }

        private static void ValidatePlayerCommandEmitter(
            ICollection<ProjectValidationResult> results)
        {
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneThreeSetup.PlayerPrefabPath);
            bool valid = player != null
                && player.GetComponent<VerbalCommandEmitter>() != null
                && player.GetComponent<RulesOfEntry.Input.TacticalPlayerInput>() != null
                && player.GetComponent<FirearmController>() != null;
            if (!valid)
            {
                AddError(
                    results,
                    "M3 Player Command Emitter",
                    "Player prefab must contain tactical input, firearm context, and verbal command emitter.");
                return;
            }

            AddPass(
                results,
                "M3 Player Command Emitter",
                "Player prefab can emit commands with weapon-presentation context.");
        }

        private static void ValidateActorPrefab(
            ICollection<ProjectValidationResult> results,
            string path,
            ActorRole expectedRole)
        {
            GameObject actor = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            ActorIdentity identity = actor != null ? actor.GetComponent<ActorIdentity>() : null;
            HumanActorController controller = actor != null
                ? actor.GetComponent<HumanActorController>()
                : null;
            ActorHitRegion[] regions = actor != null
                ? actor.GetComponentsInChildren<ActorHitRegion>(true)
                : Array.Empty<ActorHitRegion>();
            bool valid = actor != null
                && identity != null
                && identity.Role == expectedRole
                && actor.GetComponent<ActorCondition>() != null
                && actor.GetComponent<ActorInventory>() != null
                && actor.GetComponent<CustodyComponent>() != null
                && actor.GetComponent<CustodyEventLedger>() != null
                && actor.GetComponent<CustodyInteractable>() != null
                && actor.GetComponent<HumanPerception>() != null
                && actor.GetComponent<HumanDecisionLedger>() != null
                && actor.GetComponent<NavMeshAgent>() != null
                && actor.GetComponent<ActorVisual>() != null
                && controller != null
                && controller.HasCompleteConfiguration
                && regions.Any(region => region.Region == ActorHitRegionType.Head)
                && regions.Any(region => region.Region == ActorHitRegionType.Torso);
            if (!valid)
            {
                AddError(
                    results,
                    $"M3 {expectedRole} Prefab",
                    $"{path} is missing identity, condition, perception, deterministic decision, navigation, hit-region, or custody configuration.");
                return;
            }

            AddPass(
                results,
                $"M3 {expectedRole} Prefab",
                $"{identity.DisplayName} has complete behavior, injury, navigation, interaction, and accountability components.");
        }

        private static void ValidateNavigationData(
            ICollection<ProjectValidationResult> results)
        {
            string[] navigationDependencies = GetPrototypeNavigationDependencies();
            if (navigationDependencies.Length == 0)
            {
                AddError(
                    results,
                    "M3 Baked Navigation",
                    "The prototype scene has no valid baked NavMeshData dependency. "
                        + "Run the current mission setup tool.");
                return;
            }

            AddPass(
                results,
                "M3 Baked Navigation",
                "Prototype NavMesh data exists: "
                    + string.Join(", ", navigationDependencies) + ".");
        }

        private static void ValidatePrototypeScene(
            ICollection<ProjectValidationResult> results)
        {
            string scenePath = ProjectInfo.PrototypeScenePath;
            if (!File.Exists(scenePath))
            {
                AddError(results, "M3 Prototype Scene", $"Missing {scenePath}.");
                return;
            }

            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            string[] required =
            {
                RulesOfEntryMilestoneThreeSetup.SuspectPrefabPath,
                RulesOfEntryMilestoneThreeSetup.CivilianPrefabPath,
                RulesOfEntryMilestoneThreeSetup.SuspectProfilePath,
                RulesOfEntryMilestoneThreeSetup.CivilianProfilePath,
                RulesOfEntryMilestoneThreeSetup.DebugUiPrefabPath
            };
            string[] missing = required
                .Where(path => !dependencies.Contains(path, StringComparer.Ordinal))
                .ToArray();
            bool navigationMissing = !dependencies.Any(path =>
                AssetDatabase.LoadAssetAtPath<NavMeshData>(path) != null);
            string sceneText = File.ReadAllText(scenePath);
            bool rootMissing = sceneText.IndexOf(GeneratedRootName, StringComparison.Ordinal) < 0;
            if (missing.Length > 0 || navigationMissing || rootMissing)
            {
                AddError(
                    results,
                    "M3 Prototype Scene",
                    missing.Length > 0
                        ? "Missing dependencies: " + string.Join(", ", missing) + "."
                        : navigationMissing
                            ? "The scene has no valid baked NavMeshData dependency."
                        : $"Missing generated root {GeneratedRootName}.");
                return;
            }

            AddPass(
                results,
                "M3 Prototype Scene",
                "Suspect, civilian, diagnostics, behavior data, and baked navigation are wired into the prototype scene.");
        }

        private static string[] GetPrototypeNavigationDependencies()
        {
            if (!File.Exists(ProjectInfo.PrototypeScenePath))
            {
                return Array.Empty<string>();
            }

            return AssetDatabase.GetDependencies(ProjectInfo.PrototypeScenePath, true)
                .Where(path => AssetDatabase.LoadAssetAtPath<NavMeshData>(path) != null)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }

        private static void ValidateAccountabilityBoundary(
            ICollection<ProjectValidationResult> results)
        {
            string ledgerPath = RuntimeRoot + "/Combat/UseOfForceEventLedger.cs";
            if (!File.Exists(ledgerPath))
            {
                AddError(results, "M3 Force Context", $"Missing {ledgerPath}.");
                return;
            }

            string source = File.ReadAllText(ledgerPath);
            bool capturesSubject = source.IndexOf(
                "ForceSubjectSnapshot",
                StringComparison.Ordinal) >= 0;
            string[] forbiddenScorePatterns =
            {
                @"\bMissionScore\b",
                @"\bPenalty\b",
                @"\bAwardPoints\b"
            };
            string[] found = Directory.EnumerateFiles(RuntimeRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => path.IndexOf("UI", StringComparison.OrdinalIgnoreCase) < 0)
                .Where(path => forbiddenScorePatterns.Any(pattern =>
                    Regex.IsMatch(File.ReadAllText(path), pattern, RegexOptions.CultureInvariant)))
                .ToArray();
            if (!capturesSubject || found.Length > 0)
            {
                AddError(
                    results,
                    "M3 Force Context",
                    !capturesSubject
                        ? "Firearm discharge facts do not capture pre-impact subject state."
                        : "Actor/combat code directly references future score concepts: "
                            + string.Join(", ", found) + ".");
                return;
            }

            AddPass(
                results,
                "M3 Force Context",
                "Force events capture pre-impact actor facts without assigning score or ROE judgment.");
        }

        private static void LogResults(IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                string message = $"{result.Check}: {result.Message}";
                switch (result.Severity)
                {
                    case ProjectValidationSeverity.Error:
                        ProjectLog.Error("M3 Validation", message);
                        break;
                    case ProjectValidationSeverity.Warning:
                        ProjectLog.Warning("M3 Validation", message);
                        break;
                    default:
                        ProjectLog.Info("M3 Validation", message);
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
