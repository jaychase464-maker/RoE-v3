using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Core;
using RulesOfEntry.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RulesOfEntry.Editor.Foundation
{
    public static class RulesOfEntryFoundationSetup
    {
        private const string MenuPath = "Tools/Rules of Entry/Milestone 0/Build Foundation";

        [MenuItem(MenuPath, false, 10)]
        public static void BuildFoundation()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before building the project foundation.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                ProjectLog.Warning("Foundation", "Foundation setup was cancelled before saving open scenes.");
                return;
            }

            try
            {
                EnsureRequiredFolders();
                EnsurePrototypeScene();
                ConfigureBuildSettings();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                ProjectLog.Info(
                    "Foundation",
                    "Milestone 0 foundation created. Running project validation now.");

                RulesOfEntryProjectValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Foundation", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Foundation setup stopped because an unexpected error occurred. Check the Console for the complete exception.",
                    "OK");
            }
        }

        private static void EnsureRequiredFolders()
        {
            foreach (string folder in RulesOfEntryFoundationPaths.RequiredFolders)
            {
                EnsureFolder(folder);
            }
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string[] parts = assetPath.Split('/');
            if (parts.Length == 0 || !string.Equals(parts[0], "Assets", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Invalid Unity asset folder path: {assetPath}");
            }

            string currentPath = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string nextPath = currentPath + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    string guid = AssetDatabase.CreateFolder(currentPath, parts[index]);
                    if (string.IsNullOrEmpty(guid))
                    {
                        throw new InvalidOperationException($"Unity could not create folder: {nextPath}");
                    }
                }

                currentPath = nextPath;
            }
        }

        private static void EnsurePrototypeScene()
        {
            SceneAsset sourceScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                ProjectInfo.OriginalTemplateScenePath);
            if (sourceScene == null)
            {
                throw new InvalidOperationException(
                    $"The HDRP template scene was not found at {ProjectInfo.OriginalTemplateScenePath}.");
            }

            SceneAsset prototypeScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                ProjectInfo.PrototypeScenePath);
            if (prototypeScene == null)
            {
                bool copied = AssetDatabase.CopyAsset(
                    ProjectInfo.OriginalTemplateScenePath,
                    ProjectInfo.PrototypeScenePath);
                if (!copied)
                {
                    throw new InvalidOperationException(
                        $"Unity could not copy the prototype scene to {ProjectInfo.PrototypeScenePath}.");
                }

                AssetDatabase.Refresh();
            }

            Scene scene = EditorSceneManager.OpenScene(
                ProjectInfo.PrototypeScenePath,
                OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);

            SceneFoundationMarker marker = FindMarker(scene);
            if (marker == null)
            {
                GameObject foundationRoot = new GameObject("[RulesOfEntry]");
                marker = foundationRoot.AddComponent<SceneFoundationMarker>();
                EditorSceneManager.MarkSceneDirty(scene);
            }

            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException(
                    $"Unity could not save the prototype scene at {ProjectInfo.PrototypeScenePath}.");
            }

            Selection.activeGameObject = marker.gameObject;
        }

        private static SceneFoundationMarker FindMarker(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                SceneFoundationMarker marker =
                    root.GetComponentInChildren<SceneFoundationMarker>(true);
                if (marker != null)
                {
                    return marker;
                }
            }

            return null;
        }

        private static void ConfigureBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
                .Where(scene => !string.Equals(
                    scene.path,
                    ProjectInfo.PrototypeScenePath,
                    StringComparison.Ordinal))
                .Select(scene => new EditorBuildSettingsScene(
                    scene.path,
                    string.Equals(
                        scene.path,
                        ProjectInfo.OriginalTemplateScenePath,
                        StringComparison.Ordinal)
                        ? false
                        : scene.enabled))
                .ToList();

            scenes.Insert(
                0,
                new EditorBuildSettingsScene(ProjectInfo.PrototypeScenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
