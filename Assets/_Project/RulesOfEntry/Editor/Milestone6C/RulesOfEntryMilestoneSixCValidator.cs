using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.Deployment;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Officers;
using RulesOfEntry.Planning;
using RulesOfEntry.UI.Operations;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RulesOfEntry.Editor.Milestone6C
{
    public static class RulesOfEntryMilestoneSixCValidator
    {
        private const string BriefingAssetPath =
            "Assets/_Project/RulesOfEntry/Data/Planning/M6_PressurePointBriefing.asset";

        [MenuItem(
            "Tools/Rules of Entry/Milestone 6C/Validate Deployment and In-Mission Tablet",
            false,
            83)]
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
                ? $"Milestone 6C validation passed with {passes} checks and "
                    + $"{warnings} warning(s)."
                : $"Milestone 6C validation failed with {errors} error(s), "
                    + $"{warnings} warning(s), and {passes} passing checks.";
            EditorUtility.DisplayDialog(
                ProjectInfo.GameTitle,
                summary + "\n\nSee the Console for details.",
                "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(
            bool logResults)
        {
            List<ProjectValidationResult> results =
                new List<ProjectValidationResult>();
            ValidateOfficerPrefab(
                results,
                RulesOfEntryMilestoneSixCSetup.OfficerAlphaPrefabPath);
            ValidateOfficerPrefab(
                results,
                RulesOfEntryMilestoneSixCSetup.OfficerBravoPrefabPath);
            ValidateTabletPrefab(results);
            ValidateOperationScene(results);
            ValidateArchitecture(results);
            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateOfficerPrefab(
            ICollection<ProjectValidationResult> results,
            string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            OfficerBodyCameraSource source = prefab != null
                ? prefab.GetComponent<OfficerBodyCameraSource>()
                : null;
            Camera camera = prefab != null
                ? prefab.transform.Find("ROE_OfficerBodyCamera")
                    ?.GetComponent<Camera>()
                : null;
            Type hdCameraType = Type.GetType(
                "UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData, "
                    + "Unity.RenderPipelines.HighDefinition.Runtime");
            bool hdrpCameraValid = hdCameraType != null
                && camera != null
                && camera.GetComponent(hdCameraType) != null;
            if (source == null
                || !source.HasCompleteConfiguration
                || camera == null
                || !hdrpCameraValid
                || camera.enabled
                || camera.targetTexture != null)
            {
                AddError(
                    results,
                    "M6C Officer Body Camera",
                    $"{path} requires a configured HDRP, non-rendering-by-default "
                        + "body-camera source and camera.");
                return;
            }

            AddPass(
                results,
                "M6C Officer Body Camera",
                $"{prefab.name} exposes an on-demand live body-camera source.");
        }

        private static void ValidateTabletPrefab(
            ICollection<ProjectValidationResult> results)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneSixCSetup.TabletPrefabPath);
            InMissionTabletController tablet = prefab != null
                ? prefab.GetComponent<InMissionTabletController>()
                : null;
            Canvas canvas = prefab != null ? prefab.GetComponent<Canvas>() : null;
            RawImage liveFeed = prefab != null
                ? prefab.GetComponentInChildren<RawImage>(true)
                : null;
            if (tablet == null
                || !tablet.HasCompleteVisualConfiguration
                || canvas == null
                || canvas.sortingOrder < 300
                || liveFeed == null)
            {
                AddError(
                    results,
                    "M6C Operation Tablet Prefab",
                    "The operation tablet requires complete visuals, a priority "
                        + "Canvas, and a live RawImage feed surface.");
                return;
            }

            AddPass(
                results,
                "M6C Operation Tablet Prefab",
                "The rugged operation tablet contains situation, objective, and "
                    + "live body-camera presentation.");
        }

        private static void ValidateOperationScene(
            ICollection<ProjectValidationResult> results)
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
                OperationDeploymentCoordinator deployment =
                    FindInScene<OperationDeploymentCoordinator>(scene);
                InMissionTabletController tablet =
                    FindInScene<InMissionTabletController>(scene);
                OfficerSquadController squad =
                    FindInScene<OfficerSquadController>(scene);
                OperationEntryAnchor[] anchors = scene.GetRootGameObjects()
                    .SelectMany(root =>
                        root.GetComponentsInChildren<OperationEntryAnchor>(true))
                    .ToArray();
                string[] anchorIds = anchors
                    .Where(anchor => anchor != null)
                    .Select(anchor => anchor.EntryPointId)
                    .ToArray();
                OperationBriefingDefinition briefing =
                    AssetDatabase.LoadAssetAtPath<OperationBriefingDefinition>(
                        BriefingAssetPath);
                string[] plannedEntryIds = briefing?.EntryPoints
                    .Select(entry => entry.EntryPointId)
                    .ToArray() ?? Array.Empty<string>();
                bool entriesMatch = anchorIds.Length == plannedEntryIds.Length
                    && plannedEntryIds.All(id => anchorIds.Contains(
                        id,
                        StringComparer.Ordinal));
                bool squadFeedsValid = squad != null
                    && squad.Officers.Count > 0
                    && squad.Officers.All(officer => officer != null
                        && officer.GetComponent<OfficerBodyCameraSource>() != null);
                if (deployment == null
                    || !deployment.HasCompleteConfiguration
                    || tablet == null
                    || !tablet.HasCompleteConfiguration
                    || anchors.Length < 3
                    || anchors.Any(anchor => !anchor.HasValidConfiguration)
                    || anchorIds.Distinct(StringComparer.Ordinal).Count()
                        != anchorIds.Length
                    || !entriesMatch
                    || !squadFeedsValid)
                {
                    AddError(
                        results,
                        "M6C Operation Scene",
                        "The prototype requires matching unique entry anchors, a "
                            + "configured deployment coordinator, a configured "
                            + "operation tablet, and body-camera-equipped squad members.");
                    return;
                }

                AddPass(
                    results,
                    "M6C Operation Scene",
                    "Planning entry IDs, deployed squad data, and live officer feeds "
                        + "are connected in the operation scene.");
            }
            finally
            {
                if (opened && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void ValidateArchitecture(
            ICollection<ProjectValidationResult> results)
        {
            bool hasRosterReplacement = typeof(OfficerSquadController).GetMethod(
                "SetDeployedOfficers") != null;
            bool contextRemainsIdentifierOnly = typeof(OperationDeploymentContext)
                .GetProperties()
                .Where(property => property.GetMethod != null
                    && property.GetMethod.IsStatic)
                .All(property => property.PropertyType == typeof(string)
                    || property.PropertyType == typeof(bool)
                    || typeof(System.Collections.IEnumerable).IsAssignableFrom(
                        property.PropertyType));
            if (!hasRosterReplacement || !contextRemainsIdentifierOnly)
            {
                AddError(
                    results,
                    "M6C Deployment Architecture",
                    "Deployment must preserve identifier-only cross-scene state and "
                        + "replace only the scene-owned officer roster.");
                return;
            }

            AddPass(
                results,
                "M6C Deployment Architecture",
                "Cross-scene deployment remains identifier-only and operation-owned "
                    + "objects receive the applied plan.");
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

        private static void LogResults(
            IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                if (result.Severity == ProjectValidationSeverity.Error)
                {
                    ProjectLog.Error(
                        "M6C Validation",
                        $"{result.Check}: {result.Message}");
                }
                else
                {
                    ProjectLog.Info(
                        "M6C Validation",
                        $"{result.Check}: {result.Message}");
                }
            }
        }
    }
}
