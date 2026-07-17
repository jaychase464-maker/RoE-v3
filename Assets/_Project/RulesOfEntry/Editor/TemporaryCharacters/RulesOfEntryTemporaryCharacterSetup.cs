using System;
using System.Collections.Generic;
using System.Linq;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Characters;
using RulesOfEntry.Core;
using RulesOfEntry.Editor.Foundation;
using RulesOfEntry.Editor.Milestone3;
using RulesOfEntry.Editor.Milestone5;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RulesOfEntry.Editor.TemporaryCharacters
{
    public static class RulesOfEntryTemporaryCharacterSetup
    {
        private const string ApplyMenuPath =
            "Tools/Rules of Entry/Temporary Characters/Apply Sample to Suspect";
        private const string RestoreMenuPath =
            "Tools/Rules of Entry/Temporary Characters/Restore Prototype Suspect";

        internal const string ModelPath =
            "Assets/_Project/RulesOfEntry/Art/Characters/Temporary/SKM_Character.fbx";
        internal const string VisualRootName = "TemporarySampleCharacter";
        internal const string MaterialFolder =
            "Assets/_Project/RulesOfEntry/Art/Characters/Temporary/GeneratedMaterials";

        private static readonly string[] PrimitiveVisualNames =
        {
            "Body",
            "Head",
            "LeftArm",
            "RightArm"
        };

        [MenuItem(ApplyMenuPath, false, 76)]
        public static void ApplyTemporarySuspect()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before changing the temporary character presentation.",
                    "OK");
                return;
            }

            try
            {
                RequireMilestoneFiveBaseline();
                ConfigureModelImporter();
                MaterialSet materials = CreateOrUpdateMaterials();
                ApplyToSuspectPrefab(materials);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ProjectLog.Info(
                    "Temporary Character",
                    "The sample Humanoid was applied to the prototype suspect without changing gameplay components. Running validation now.");
                RulesOfEntryTemporaryCharacterValidator.ValidateFromMenu();
            }
            catch (Exception exception)
            {
                ProjectLog.Exception("Temporary Character", exception);
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Temporary-character setup stopped. Check the first Console error.",
                    "OK");
            }
        }

        [MenuItem(RestoreMenuPath, false, 77)]
        public static void RestorePrototypeSuspect()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    ProjectInfo.GameTitle,
                    "Exit Play Mode before restoring the prototype presentation.",
                    "OK");
                return;
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(
                RulesOfEntryMilestoneThreeSetup.SuspectPrefabPath);
            try
            {
                RemoveTemporaryPresentation(contents);
                SetPrimitiveRenderersEnabled(contents.transform, true);
                PrefabUtility.SaveAsPrefabAsset(
                    contents,
                    RulesOfEntryMilestoneThreeSetup.SuspectPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ProjectLog.Info(
                "Temporary Character",
                "The sample character was removed and the original prototype suspect visuals were restored.");
        }

        private static void RequireMilestoneFiveBaseline()
        {
            ProjectValidationResult[] errors = RulesOfEntryMilestoneFiveValidator
                .RunValidation(false)
                .Where(result => result.Severity == ProjectValidationSeverity.Error)
                .ToArray();
            if (errors.Length == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                "Milestone 5 must pass before applying a temporary character. "
                + string.Join(" | ", errors.Select(error =>
                    $"{error.Check}: {error.Message}")));
        }

        private static void ConfigureModelImporter()
        {
            ModelImporter importer = AssetImporter.GetAtPath(ModelPath)
                as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"The temporary FBX is missing: {ModelPath}");
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeGameObjects = false;
            importer.isReadable = false;
            importer.weldVertices = true;
            importer.SaveAndReimport();

            Avatar avatar = LoadHumanoidAvatar();
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
            {
                throw new InvalidOperationException(
                    "Unity could not create a valid Humanoid avatar from SKM_Character.fbx. Open the model Rig tab, inspect the mapping, then report the first unmapped required bone.");
            }
        }

        private static Avatar LoadHumanoidAvatar()
        {
            return AssetDatabase.LoadAllAssetsAtPath(ModelPath)
                .OfType<Avatar>()
                .FirstOrDefault(avatar => avatar != null && avatar.isHuman);
        }

        private static MaterialSet CreateOrUpdateMaterials()
        {
            EnsureFolder(MaterialFolder);
            Shader shader = Shader.Find("HDRP/Lit");
            if (shader == null)
            {
                throw new InvalidOperationException(
                    "HDRP/Lit was not found. Confirm the active HDRP package and pipeline asset before character setup.");
            }

            return new MaterialSet(
                CreateOrUpdateMaterial(
                    "Temporary_Skin.mat",
                    shader,
                    new Color(0.34f, 0.20f, 0.14f, 1f),
                    0f,
                    0.28f),
                CreateOrUpdateMaterial(
                    "Temporary_Shirt.mat",
                    shader,
                    new Color(0.075f, 0.10f, 0.12f, 1f),
                    0f,
                    0.2f),
                CreateOrUpdateMaterial(
                    "Temporary_Jeans.mat",
                    shader,
                    new Color(0.045f, 0.075f, 0.12f, 1f),
                    0f,
                    0.16f),
                CreateOrUpdateMaterial(
                    "Temporary_Gear.mat",
                    shader,
                    new Color(0.055f, 0.065f, 0.06f, 1f),
                    0.02f,
                    0.24f),
                CreateOrUpdateMaterial(
                    "Temporary_Eyes.mat",
                    shader,
                    new Color(0.025f, 0.035f, 0.04f, 1f),
                    0f,
                    0.68f),
                CreateOrUpdateMaterial(
                    "Temporary_Teeth.mat",
                    shader,
                    new Color(0.72f, 0.69f, 0.62f, 1f),
                    0f,
                    0.32f));
        }

        private static Material CreateOrUpdateMaterial(
            string filename,
            Shader shader,
            Color baseColor,
            float metallic,
            float smoothness)
        {
            string path = MaterialFolder + "/" + filename;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = filename.Substring(0, filename.Length - 4)
                };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_BaseColor", baseColor);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ApplyToSuspectPrefab(MaterialSet materials)
        {
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Avatar avatar = LoadHumanoidAvatar();
            if (modelAsset == null || avatar == null)
            {
                throw new InvalidOperationException(
                    "The temporary model or its Humanoid avatar could not be loaded after import.");
            }

            string prefabPath = RulesOfEntryMilestoneThreeSetup.SuspectPrefabPath;
            GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                HumanActorController actor = contents.GetComponent<HumanActorController>();
                ActorIdentity identity = contents.GetComponent<ActorIdentity>();
                CustodyComponent custody = contents.GetComponent<CustodyComponent>();
                ActorCondition condition = contents.GetComponent<ActorCondition>();
                Transform bodyRoot = contents.transform.Find("BodyPresentation");
                if (actor == null
                    || identity == null
                    || identity.Role != ActorRole.Suspect
                    || custody == null
                    || condition == null
                    || bodyRoot == null)
                {
                    throw new InvalidOperationException(
                        "The prototype suspect prefab no longer matches the protected Milestone 3 actor contract.");
                }

                RemoveTemporaryPresentation(contents);
                SetPrimitiveRenderersEnabled(contents.transform, true);

                GameObject modelInstance = UnityEngine.Object.Instantiate(modelAsset);
                modelInstance.name = VisualRootName;
                modelInstance.transform.SetParent(bodyRoot, false);
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
                modelInstance.transform.localScale = Vector3.one;
                Renderer[] renderers = PrepareRenderers(modelInstance, materials);
                SetLayerRecursively(modelInstance, bodyRoot.gameObject.layer);
                FitAndGround(modelInstance, bodyRoot, renderers, 1.78f);
                ValidateRenderablePresentation(modelInstance, renderers);

                Animator animator = modelInstance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = modelInstance.AddComponent<Animator>();
                }

                animator.avatar = avatar;
                animator.runtimeAnimatorController = null;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.updateMode = AnimatorUpdateMode.Normal;

                TemporaryHumanoidPoseDriver driver =
                    contents.AddComponent<TemporaryHumanoidPoseDriver>();
                driver.Configure(actor, custody, condition, animator);
                if (!driver.HasCompleteConfiguration)
                {
                    throw new InvalidOperationException(
                        "The temporary pose driver could not retain its Humanoid renderer references.");
                }

                SetPrimitiveRenderersEnabled(contents.transform, false);

                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void RemoveTemporaryPresentation(GameObject contents)
        {
            foreach (TemporaryHumanoidPoseDriver driver in
                contents.GetComponents<TemporaryHumanoidPoseDriver>())
            {
                UnityEngine.Object.DestroyImmediate(driver);
            }

            Transform bodyRoot = contents.transform.Find("BodyPresentation");
            Transform existing = bodyRoot != null
                ? bodyRoot.Find(VisualRootName)
                : null;
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static Renderer[] PrepareRenderers(
            GameObject modelInstance,
            MaterialSet materials)
        {
            Renderer[] renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException(
                    "The imported temporary character contains no renderers.");
            }

            foreach (Renderer renderer in renderers)
            {
                ActivateRendererHierarchy(renderer.transform, modelInstance.transform);
                Material material = materials.Resolve(renderer.name);
                int slotCount = Mathf.Max(1, renderer.sharedMaterials.Length);
                renderer.sharedMaterials = Enumerable
                    .Repeat(material, slotCount)
                    .ToArray();
                renderer.enabled = true;
                renderer.forceRenderingOff = false;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                if (renderer is SkinnedMeshRenderer skinnedRenderer)
                {
                    skinnedRenderer.updateWhenOffscreen = true;
                    skinnedRenderer.quality = SkinQuality.Bone4;
                    ExpandSkinnedBounds(skinnedRenderer);
                }
            }

            return renderers;
        }

        private static void FitAndGround(
            GameObject modelInstance,
            Transform bodyRoot,
            IReadOnlyList<Renderer> renderers,
            float targetHeight)
        {
            Bounds bounds = GetCombinedBounds(renderers);
            if (!HasFinitePositiveBounds(bounds))
            {
                throw new InvalidOperationException(
                    "The imported character bounds are invalid and cannot be scaled safely.");
            }

            float uniformScale = targetHeight / bounds.size.y;
            modelInstance.transform.localScale *= uniformScale;
            bounds = GetCombinedBounds(renderers);
            if (!HasFinitePositiveBounds(bounds))
            {
                throw new InvalidOperationException(
                    "The temporary character produced invalid bounds after scaling.");
            }

            Vector3 localBottom = bodyRoot.InverseTransformPoint(
                new Vector3(bounds.center.x, bounds.min.y, bounds.center.z));
            Vector3 localCenter = bodyRoot.InverseTransformPoint(bounds.center);
            modelInstance.transform.localPosition += new Vector3(
                -localCenter.x,
                -localBottom.y,
                -localCenter.z);
        }

        private static void ValidateRenderablePresentation(
            GameObject modelInstance,
            IReadOnlyList<Renderer> renderers)
        {
            if (!modelInstance.activeSelf
                || modelInstance.transform.localScale.sqrMagnitude <= 0.0001f
                || renderers.Count == 0
                || renderers.Any(renderer => renderer == null
                    || !renderer.enabled
                    || renderer.forceRenderingOff
                    || !renderer.gameObject.activeSelf
                    || renderer.sharedMaterials.Length == 0
                    || renderer.sharedMaterials.Any(material => material == null)))
            {
                throw new InvalidOperationException(
                    "The temporary model failed its renderer visibility gate. The prototype fallback was left enabled.");
            }

            Bounds bounds = GetCombinedBounds(renderers);
            if (!HasFinitePositiveBounds(bounds)
                || bounds.size.y < 1.5f
                || bounds.size.y > 2.1f)
            {
                throw new InvalidOperationException(
                    $"The temporary character bounds are not human-scale after fitting: {bounds.size}.");
            }
        }

        private static void ActivateRendererHierarchy(
            Transform rendererTransform,
            Transform modelRoot)
        {
            Transform current = rendererTransform;
            while (current != null)
            {
                current.gameObject.SetActive(true);
                if (current == modelRoot)
                {
                    return;
                }

                current = current.parent;
            }
        }

        private static void ExpandSkinnedBounds(
            SkinnedMeshRenderer renderer)
        {
            Bounds bounds = renderer.localBounds;
            if (!HasFinitePositiveBounds(bounds) && renderer.sharedMesh != null)
            {
                bounds = renderer.sharedMesh.bounds;
            }

            if (!HasFinitePositiveBounds(bounds))
            {
                return;
            }

            bounds.Expand(new Vector3(0.5f, 0.5f, 0.5f));
            renderer.localBounds = bounds;
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

        private static Bounds GetCombinedBounds(IReadOnlyList<Renderer> renderers)
        {
            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Count; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static void SetPrimitiveRenderersEnabled(
            Transform root,
            bool enabled)
        {
            foreach (string visualName in PrimitiveVisualNames)
            {
                Transform visual = root.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(candidate => candidate.parent != null
                        && string.Equals(
                            candidate.parent.name,
                            "BodyPresentation",
                            StringComparison.Ordinal)
                        && string.Equals(
                            candidate.name,
                            visualName,
                            StringComparison.Ordinal));
                Renderer renderer = visual != null
                    ? visual.GetComponent<Renderer>()
                    : null;
                if (renderer == null)
                {
                    throw new InvalidOperationException(
                        $"The fallback primitive {visualName} is missing from the suspect prefab.");
                }

                renderer.enabled = enabled;
            }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                transform.gameObject.layer = layer;
            }
        }

        private static void EnsureFolder(string path)
        {
            string[] segments = path.Split('/');
            string current = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }

        private readonly struct MaterialSet
        {
            private readonly Material skin;
            private readonly Material shirt;
            private readonly Material jeans;
            private readonly Material gear;
            private readonly Material eyes;
            private readonly Material teeth;

            public MaterialSet(
                Material skin,
                Material shirt,
                Material jeans,
                Material gear,
                Material eyes,
                Material teeth)
            {
                this.skin = skin;
                this.shirt = shirt;
                this.jeans = jeans;
                this.gear = gear;
                this.eyes = eyes;
                this.teeth = teeth;
            }

            public Material Resolve(string rendererName)
            {
                string normalized = rendererName?.ToLowerInvariant() ?? string.Empty;
                if (normalized.Contains("eye"))
                {
                    return eyes;
                }

                if (normalized.Contains("teeth"))
                {
                    return teeth;
                }

                if (normalized.Contains("head")
                    || normalized.Contains("body")
                    || normalized.Contains("arm"))
                {
                    return skin;
                }

                if (normalized.Contains("shirt"))
                {
                    return shirt;
                }

                if (normalized.Contains("jean"))
                {
                    return jeans;
                }

                return gear;
            }
        }
    }
}
