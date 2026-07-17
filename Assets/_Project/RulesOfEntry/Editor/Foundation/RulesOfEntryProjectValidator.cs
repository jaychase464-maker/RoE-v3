using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RulesOfEntry.Core;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace RulesOfEntry.Editor.Foundation
{
    public enum ProjectValidationSeverity
    {
        Pass = 0,
        Warning = 1,
        Error = 2
    }

    public sealed class ProjectValidationResult
    {
        public ProjectValidationResult(
            ProjectValidationSeverity severity,
            string check,
            string message)
        {
            Severity = severity;
            Check = check;
            Message = message;
        }

        public ProjectValidationSeverity Severity { get; }
        public string Check { get; }
        public string Message { get; }
    }

    public static class RulesOfEntryProjectValidator
    {
        private const string MenuPath = "Tools/Rules of Entry/Validate Project";

        private static readonly IReadOnlyDictionary<string, string> RequiredPackages =
            new Dictionary<string, string>
            {
                { "com.unity.inputsystem", "1.19.0" },
                { "com.unity.render-pipelines.high-definition", "17.5.0" },
                { "com.unity.test-framework", "1.7.0" }
            };

        [MenuItem(MenuPath, false, 50)]
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
                ? $"Validation passed with {passCount} checks and {warningCount} warning(s)."
                : $"Validation failed with {errorCount} error(s), {warningCount} warning(s), and {passCount} passing checks.";

            EditorUtility.DisplayDialog(ProjectInfo.GameTitle, summary + "\n\nSee the Console for details.", "OK");
        }

        public static IReadOnlyList<ProjectValidationResult> RunValidation(bool logResults)
        {
            List<ProjectValidationResult> results = new List<ProjectValidationResult>();

            ValidateUnityVersion(results);
            ValidateProjectIdentity(results);
            ValidatePackages(results);
            ValidateInputConfiguration(results);
            ValidateRenderPipeline(results);
            ValidateFoldersAndAssemblies(results);
            ValidatePrototypeScene(results);
            ValidateMissingScripts(results);
            ValidateLegacyInputUsage(results);

            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateUnityVersion(List<ProjectValidationResult> results)
        {
            if (string.Equals(
                Application.unityVersion,
                ProjectInfo.ExpectedUnityVersion,
                StringComparison.Ordinal))
            {
                AddPass(results, "Unity Version", $"Using {Application.unityVersion}.");
                return;
            }

            AddError(
                results,
                "Unity Version",
                $"Expected {ProjectInfo.ExpectedUnityVersion}, but the project is open in {Application.unityVersion}.");
        }

        private static void ValidateProjectIdentity(List<ProjectValidationResult> results)
        {
            if (string.Equals(PlayerSettings.productName, "RoE v3", StringComparison.Ordinal))
            {
                AddPass(results, "Product Name", "Player Settings product name is RoE v3.");
            }
            else
            {
                AddError(
                    results,
                    "Product Name",
                    $"Expected Player Settings product name RoE v3, found {PlayerSettings.productName}.");
            }

            if (string.Equals(PlayerSettings.companyName, "DefaultCompany", StringComparison.Ordinal))
            {
                AddWarning(
                    results,
                    "Company Name",
                    "Player Settings still uses DefaultCompany. Set the approved studio identity before a distributable build.");
            }
            else
            {
                AddPass(results, "Company Name", $"Company name is {PlayerSettings.companyName}.");
            }
        }

        private static void ValidatePackages(List<ProjectValidationResult> results)
        {
            try
            {
                Dictionary<string, string> installedPackages = new Dictionary<string, string>();
                PackageInfo[] packages = PackageInfo.GetAllRegisteredPackages();
                foreach (PackageInfo package in packages)
                {
                    if (package != null && !string.IsNullOrEmpty(package.name))
                    {
                        installedPackages[package.name] = package.version;
                    }
                }

                foreach (KeyValuePair<string, string> requiredPackage in RequiredPackages)
                {
                    if (!installedPackages.TryGetValue(requiredPackage.Key, out string installedVersion))
                    {
                        AddError(
                            results,
                            "Package",
                            $"Required package {requiredPackage.Key} is not installed.");
                        continue;
                    }

                    if (!string.Equals(
                        installedVersion,
                        requiredPackage.Value,
                        StringComparison.Ordinal))
                    {
                        AddError(
                            results,
                            "Package",
                            $"Package {requiredPackage.Key} must be {requiredPackage.Value}; found {installedVersion}.");
                        continue;
                    }

                    AddPass(
                        results,
                        "Package",
                        $"{requiredPackage.Key} {installedVersion} is installed.");
                }
            }
            catch (Exception exception)
            {
                AddError(results, "Package", $"Package inspection failed: {exception.Message}");
            }
        }

        private static void ValidateInputConfiguration(List<ProjectValidationResult> results)
        {
            string projectSettingsPath = GetAbsoluteProjectPath(
                "ProjectSettings/ProjectSettings.asset");

            try
            {
                string settings = File.ReadAllText(projectSettingsPath);
                bool newInputOnly = Regex.IsMatch(
                    settings,
                    @"^\s*activeInputHandler:\s*1\s*$",
                    RegexOptions.Multiline);

                if (newInputOnly)
                {
                    AddPass(results, "Input Mode", "New Input System is the active input handler.");
                }
                else
                {
                    AddError(
                        results,
                        "Input Mode",
                        "Player Settings must use the New Input System only (activeInputHandler: 1)." );
                }
            }
            catch (Exception exception)
            {
                AddError(
                    results,
                    "Input Mode",
                    $"Could not inspect ProjectSettings.asset: {exception.Message}");
            }

            UnityEngine.Object inputAsset = AssetDatabase.LoadMainAssetAtPath(
                RulesOfEntryFoundationPaths.InputActionAssetPath);
            if (inputAsset != null)
            {
                AddPass(
                    results,
                    "Input Actions",
                    $"Found {RulesOfEntryFoundationPaths.InputActionAssetPath}.");
            }
            else
            {
                AddError(
                    results,
                    "Input Actions",
                    $"Missing {RulesOfEntryFoundationPaths.InputActionAssetPath}.");
            }
        }

        private static void ValidateRenderPipeline(List<ProjectValidationResult> results)
        {
            RenderPipelineAsset activePipeline = GraphicsSettings.currentRenderPipeline;
            if (activePipeline == null)
            {
                AddError(results, "Render Pipeline", "No Scriptable Render Pipeline is active.");
                return;
            }

            string typeName = activePipeline.GetType().Name;
            if (string.Equals(typeName, "HDRenderPipelineAsset", StringComparison.Ordinal))
            {
                AddPass(
                    results,
                    "Render Pipeline",
                    $"HDRP is active through {activePipeline.name}.");
                return;
            }

            AddError(
                results,
                "Render Pipeline",
                $"Expected HDRenderPipelineAsset, found {typeName}.");
        }

        private static void ValidateFoldersAndAssemblies(List<ProjectValidationResult> results)
        {
            List<string> missingFolders = RulesOfEntryFoundationPaths.RequiredFolders
                .Where(folder => !AssetDatabase.IsValidFolder(folder))
                .ToList();

            if (missingFolders.Count == 0)
            {
                AddPass(results, "Folder Structure", "All Milestone 0 project folders exist.");
            }
            else
            {
                AddError(
                    results,
                    "Folder Structure",
                    "Missing folders: " + string.Join(", ", missingFolders));
            }

            List<string> missingAssemblies = RulesOfEntryFoundationPaths.RequiredAssemblyDefinitions
                .Where(path => !File.Exists(GetAbsoluteProjectPath(path)))
                .ToList();

            if (missingAssemblies.Count == 0)
            {
                AddPass(results, "Assembly Definitions", "Runtime, editor, and test assemblies exist.");
            }
            else
            {
                AddError(
                    results,
                    "Assembly Definitions",
                    "Missing assembly definitions: " + string.Join(", ", missingAssemblies));
            }
        }

        private static void ValidatePrototypeScene(List<ProjectValidationResult> results)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                ProjectInfo.PrototypeScenePath);
            if (sceneAsset == null)
            {
                AddError(
                    results,
                    "Prototype Scene",
                    $"Missing {ProjectInfo.PrototypeScenePath}. Run the Milestone 0 foundation setup.");
                return;
            }

            AddPass(results, "Prototype Scene", $"Found {ProjectInfo.PrototypeScenePath}.");

            string[] dependencies = AssetDatabase.GetDependencies(
                ProjectInfo.PrototypeScenePath,
                true);
            bool markerReferenced = dependencies.Any(path => string.Equals(
                path,
                RulesOfEntryFoundationPaths.SceneMarkerScriptPath,
                StringComparison.Ordinal));

            if (markerReferenced)
            {
                AddPass(results, "Scene Marker", "Prototype scene contains its foundation marker.");
            }
            else
            {
                AddError(
                    results,
                    "Scene Marker",
                    "Prototype scene does not reference SceneFoundationMarker. Rerun foundation setup.");
            }

            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            int prototypeIndex = Array.FindIndex(
                buildScenes,
                scene => scene.enabled && string.Equals(
                    scene.path,
                    ProjectInfo.PrototypeScenePath,
                    StringComparison.Ordinal));

            if (prototypeIndex < 0)
            {
                AddError(
                    results,
                    "Build Settings",
                    "Prototype scene is not enabled in Build Settings.");
            }
            else if (prototypeIndex == 0)
            {
                AddPass(results, "Build Settings", "Prototype scene is the first enabled build scene.");
            }
            else
            {
                AddWarning(
                    results,
                    "Build Settings",
                    $"Prototype scene is enabled at build index {prototypeIndex}; index 0 is recommended for Milestone 0.");
            }
        }

        private static void ValidateMissingScripts(List<ProjectValidationResult> results)
        {
            int missingCount = 0;
            List<string> affectedObjects = new List<string>();

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                {
                    continue;
                }

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    missingCount += CountMissingScripts(
                        root,
                        $"Scene {scene.path}",
                        affectedObjects);
                }
            }

            string[] prefabGuids = AssetDatabase.FindAssets(
                "t:Prefab",
                new[] { ProjectInfo.ProjectAssetRoot });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    missingCount += CountMissingScripts(prefab, $"Prefab {path}", affectedObjects);
                }
            }

            if (missingCount == 0)
            {
                AddPass(
                    results,
                    "Missing Scripts",
                    "No missing MonoBehaviour references were found in loaded scenes or project-owned prefabs.");
            }
            else
            {
                AddError(
                    results,
                    "Missing Scripts",
                    $"Found {missingCount} missing script reference(s): {string.Join("; ", affectedObjects)}");
            }
        }

        private static int CountMissingScripts(
            GameObject root,
            string location,
            List<string> affectedObjects)
        {
            int count = 0;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in transforms)
            {
                int objectCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                    child.gameObject);
                if (objectCount <= 0)
                {
                    continue;
                }

                count += objectCount;
                affectedObjects.Add($"{location} -> {GetHierarchyPath(child)} ({objectCount})");
            }

            return count;
        }

        private static string GetHierarchyPath(Transform target)
        {
            StringBuilder builder = new StringBuilder(target.name);
            Transform parent = target.parent;
            while (parent != null)
            {
                builder.Insert(0, parent.name + "/");
                parent = parent.parent;
            }

            return builder.ToString();
        }

        private static void ValidateLegacyInputUsage(List<ProjectValidationResult> results)
        {
            string runtimePath = GetAbsoluteProjectPath(ProjectInfo.RuntimeAssetRoot);
            if (!Directory.Exists(runtimePath))
            {
                AddError(results, "Legacy Input", $"Runtime folder does not exist: {runtimePath}");
                return;
            }

            Regex legacyInputPattern = new Regex(
                @"\bUnityEngine\.Input\b|\bInput\.(GetAxis|GetAxisRaw|GetButton|GetButtonDown|GetButtonUp|GetKey|GetKeyDown|GetKeyUp|mousePosition)\b",
                RegexOptions.Compiled);
            List<string> violations = new List<string>();

            foreach (string file in Directory.GetFiles(
                runtimePath,
                "*.cs",
                SearchOption.AllDirectories))
            {
                string source = File.ReadAllText(file);
                if (legacyInputPattern.IsMatch(source))
                {
                    violations.Add(GetProjectRelativePath(file));
                }
            }

            if (violations.Count == 0)
            {
                AddPass(results, "Legacy Input", "No forbidden legacy input calls exist in runtime code.");
            }
            else
            {
                AddError(
                    results,
                    "Legacy Input",
                    "Legacy input usage found in: " + string.Join(", ", violations));
            }
        }

        private static string GetAbsoluteProjectPath(string projectRelativePath)
        {
            DirectoryInfo projectRoot = Directory.GetParent(Application.dataPath);
            if (projectRoot == null)
            {
                throw new InvalidOperationException("Unity project root could not be resolved.");
            }

            string normalizedPath = projectRelativePath.Replace(
                '/',
                Path.DirectorySeparatorChar);
            return Path.GetFullPath(Path.Combine(projectRoot.FullName, normalizedPath));
        }

        private static string GetProjectRelativePath(string absolutePath)
        {
            DirectoryInfo projectRoot = Directory.GetParent(Application.dataPath);
            if (projectRoot == null)
            {
                return absolutePath;
            }

            Uri rootUri = new Uri(projectRoot.FullName.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
            Uri fileUri = new Uri(absolutePath);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString())
                .Replace('\\', '/');
        }

        private static void LogResults(IEnumerable<ProjectValidationResult> results)
        {
            foreach (ProjectValidationResult result in results)
            {
                string message = $"{result.Check}: {result.Message}";
                switch (result.Severity)
                {
                    case ProjectValidationSeverity.Error:
                        ProjectLog.Error("Validation", message);
                        break;
                    case ProjectValidationSeverity.Warning:
                        ProjectLog.Warning("Validation", message);
                        break;
                    default:
                        ProjectLog.Info("Validation", message);
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

        private static void AddWarning(
            ICollection<ProjectValidationResult> results,
            string check,
            string message)
        {
            results.Add(new ProjectValidationResult(ProjectValidationSeverity.Warning, check, message));
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
