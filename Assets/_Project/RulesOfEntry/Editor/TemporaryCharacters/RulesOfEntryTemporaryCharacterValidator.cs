using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Characters;
using RulesOfEntry.Combat;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone3;
using UnityEditor;
using UnityEngine;

namespace RulesOfEntry.Editor.TemporaryCharacters
{
    public static class RulesOfEntryTemporaryCharacterValidator
    {
        private const string MenuPath =
            "Tools/Rules of Entry/Temporary Characters/Validate Sample Suspect";
        private const int PrototypeVertexWarningThreshold = 60000;

        private static readonly string[] PrimitiveVisualNames =
        {
            "Body",
            "Head",
            "LeftArm",
            "RightArm"
        };

        [MenuItem(MenuPath, false, 78)]
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
                ? $"Temporary-character validation passed with {passes} checks and {warnings} warning(s)."
                : $"Temporary-character validation failed with {errors} error(s), {warnings} warning(s), and {passes} passing checks.";
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
            ValidateModelImport(results);
            ValidateSuspectContract(results);
            ValidatePresentation(results);
            ValidatePerformanceBoundary(results);

            if (logResults)
            {
                LogResults(results);
            }

            return results;
        }

        private static void ValidateModelImport(
            ICollection<ProjectValidationResult> results)
        {
            ModelImporter importer = AssetImporter.GetAtPath(
                RulesOfEntryTemporaryCharacterSetup.ModelPath) as ModelImporter;
            Avatar avatar = AssetDatabase
                .LoadAllAssetsAtPath(RulesOfEntryTemporaryCharacterSetup.ModelPath)
                .OfType<Avatar>()
                .FirstOrDefault(candidate => candidate != null && candidate.isHuman);
            bool valid = importer != null
                && importer.animationType == ModelImporterAnimationType.Human
                && importer.avatarSetup == ModelImporterAvatarSetup.CreateFromThisModel
                && !importer.importAnimation
                && avatar != null
                && avatar.isValid
                && avatar.isHuman;
            if (!valid)
            {
                AddError(
                    results,
                    "Temporary Model Import",
                    "SKM_Character.fbx must import as a valid Humanoid avatar with embedded animation import disabled.");
                return;
            }

            AddPass(
                results,
                "Temporary Model Import",
                "The supplied sample imports as a valid animation-free Humanoid presentation asset.");
        }

        private static void ValidateSuspectContract(
            ICollection<ProjectValidationResult> results)
        {
            GameObject suspect = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneThreeSetup.SuspectPrefabPath);
            ActorIdentity identity = suspect != null
                ? suspect.GetComponent<ActorIdentity>()
                : null;
            HumanActorController actor = suspect != null
                ? suspect.GetComponent<HumanActorController>()
                : null;
            ActorHitRegion[] hitRegions = suspect != null
                ? suspect.GetComponentsInChildren<ActorHitRegion>(true)
                : Array.Empty<ActorHitRegion>();
            bool valid = suspect != null
                && identity != null
                && identity.Role == ActorRole.Suspect
                && actor != null
                && actor.HasCompleteConfiguration
                && suspect.GetComponent<ActorCondition>() != null
                && suspect.GetComponent<CustodyComponent>() != null
                && suspect.GetComponent<HumanPerception>() != null
                && hitRegions.Any(region => region.Region == ActorHitRegionType.Head)
                && hitRegions.Any(region => region.Region == ActorHitRegionType.Torso);
            if (!valid)
            {
                AddError(
                    results,
                    "Temporary Suspect Contract",
                    "Applying the sample damaged or replaced a protected suspect AI, custody, condition, perception, or hit-region component.");
                return;
            }

            AddPass(
                results,
                "Temporary Suspect Contract",
                "The original suspect AI, custody, injury, perception, and hit-region contract remains authoritative.");
        }

        private static void ValidatePresentation(
            ICollection<ProjectValidationResult> results)
        {
            GameObject suspect = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneThreeSetup.SuspectPrefabPath);
            Transform bodyRoot = suspect != null
                ? suspect.transform.Find("BodyPresentation")
                : null;
            Transform visual = bodyRoot != null
                ? bodyRoot.Find(RulesOfEntryTemporaryCharacterSetup.VisualRootName)
                : null;
            Animator animator = visual != null
                ? visual.GetComponent<Animator>()
                : null;
            TemporaryHumanoidPoseDriver driver = suspect != null
                ? suspect.GetComponent<TemporaryHumanoidPoseDriver>()
                : null;
            SkinnedMeshRenderer[] renderers = visual != null
                ? visual.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                : Array.Empty<SkinnedMeshRenderer>();
            Collider[] importedColliders = visual != null
                ? visual.GetComponentsInChildren<Collider>(true)
                : Array.Empty<Collider>();
            bool materialsValid = renderers.Length > 0
                && renderers.All(renderer =>
                    renderer.sharedMaterials.Length > 0
                    && renderer.sharedMaterials.All(material =>
                        material != null
                        && material.shader != null
                        && string.Equals(
                            material.shader.name,
                            "HDRP/Lit",
                            StringComparison.Ordinal)));
            bool renderersVisible = renderers.Length > 0
                && visual != null
                && visual.gameObject.activeSelf
                && IsFiniteNonZeroScale(visual.localScale)
                && renderers.All(renderer =>
                    renderer != null
                    && renderer.gameObject.activeSelf
                    && renderer.enabled
                    && !renderer.forceRenderingOff
                    && renderer.updateWhenOffscreen
                    && renderer.sharedMesh != null
                    && HasFinitePositiveBounds(renderer.localBounds));
            bool fallbacksHidden = bodyRoot != null
                && PrimitiveVisualNames.All(name =>
                {
                    Transform primitive = bodyRoot.Find(name);
                    Renderer renderer = primitive != null
                        ? primitive.GetComponent<Renderer>()
                        : null;
                    return renderer != null && !renderer.enabled;
                });
            bool valid = visual != null
                && animator != null
                && animator.avatar != null
                && animator.avatar.isValid
                && animator.avatar.isHuman
                && !animator.applyRootMotion
                && animator.cullingMode == AnimatorCullingMode.AlwaysAnimate
                && driver != null
                && driver.HasCompleteConfiguration
                && materialsValid
                && renderersVisible
                && fallbacksHidden
                && importedColliders.Length == 0;
            if (!valid)
            {
                AddError(
                    results,
                    "Temporary Suspect Presentation",
                    "The suspect requires an active sample root, enabled non-culled skinned renderers with valid bounds, a valid root-motion-disabled Humanoid, the anchored procedural pose driver, HDRP materials, hidden fallbacks, and no imported colliders.");
                return;
            }

            AddPass(
                results,
                "Temporary Suspect Presentation",
                "The sample character is visible through HDRP materials and follows actor state without replacing authoritative hitboxes.");
        }

        private static bool IsFiniteNonZeroScale(Vector3 scale)
        {
            return IsFinite(scale.x)
                && IsFinite(scale.y)
                && IsFinite(scale.z)
                && Mathf.Abs(scale.x) > 0.0001f
                && Mathf.Abs(scale.y) > 0.0001f
                && Mathf.Abs(scale.z) > 0.0001f;
        }

        private static bool HasFinitePositiveBounds(Bounds bounds)
        {
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            return IsFinite(center.x)
                && IsFinite(center.y)
                && IsFinite(center.z)
                && IsFinite(size.x)
                && IsFinite(size.y)
                && IsFinite(size.z)
                && size.x > 0.001f
                && size.y > 0.001f
                && size.z > 0.001f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void ValidatePerformanceBoundary(
            ICollection<ProjectValidationResult> results)
        {
            Mesh[] meshes = AssetDatabase
                .LoadAllAssetsAtPath(RulesOfEntryTemporaryCharacterSetup.ModelPath)
                .OfType<Mesh>()
                .ToArray();
            int vertexCount = meshes.Sum(mesh => mesh != null ? mesh.vertexCount : 0);
            GameObject suspect = AssetDatabase.LoadAssetAtPath<GameObject>(
                RulesOfEntryMilestoneThreeSetup.SuspectPrefabPath);
            Transform bodyRoot = suspect != null
                ? suspect.transform.Find("BodyPresentation")
                : null;
            Transform visual = bodyRoot != null
                ? bodyRoot.Find(RulesOfEntryTemporaryCharacterSetup.VisualRootName)
                : null;
            bool hasLodGroup = visual != null
                && visual.GetComponentInChildren<LODGroup>(true) != null;
            if (vertexCount > PrototypeVertexWarningThreshold && !hasLodGroup)
            {
                AddWarning(
                    results,
                    "Temporary Character Performance",
                    $"The sample imports approximately {vertexCount:N0} vertices and has no LODGroup. Keep it limited to the prototype suspect and replace it before population-scale testing.");
                return;
            }

            AddPass(
                results,
                "Temporary Character Performance",
                $"The temporary presentation reports approximately {vertexCount:N0} imported vertices and an acceptable prototype LOD configuration.");
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
                        ProjectLog.Error("Temporary Character Validation", message);
                        break;
                    case ProjectValidationSeverity.Warning:
                        ProjectLog.Warning("Temporary Character Validation", message);
                        break;
                    default:
                        ProjectLog.Info("Temporary Character Validation", message);
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

        private static void AddWarning(
            ICollection<ProjectValidationResult> results,
            string check,
            string message)
        {
            results.Add(new ProjectValidationResult(
                ProjectValidationSeverity.Warning,
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
