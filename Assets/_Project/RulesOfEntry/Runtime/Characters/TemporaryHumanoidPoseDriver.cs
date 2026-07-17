using System;
using RulesOfEntry.Actors;
using RulesOfEntry.AI;
using RulesOfEntry.Core;
using UnityEngine;

namespace RulesOfEntry.Characters
{
    /// <summary>
    /// Provides minimal procedural poses for the temporary humanoid sample.
    /// This is intentionally presentation-only and never changes gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TemporaryHumanoidPoseDriver : MonoBehaviour
    {
        [SerializeField] private HumanActorController actor;
        [SerializeField] private CustodyComponent custody;
        [SerializeField] private ActorCondition condition;
        [SerializeField] private Animator humanoidAnimator;
        [SerializeField] private Renderer[] presentationRenderers;
        [SerializeField, Min(0.1f)] private float responseSpeed = 7f;

        private HumanPoseHandler poseHandler;
        private HumanPose humanPose;
        private float[] referenceMuscles;
        private float[] targetMuscles;
        private MuscleIndices indices;
        private bool initializationFailed;
        private bool referenceRootCaptured;
        private Vector3 referenceRootLocalPosition;
        private Quaternion referenceRootLocalRotation;
        private Vector3 referenceRootLocalScale;
        private Vector3 referenceBodyPosition;
        private Quaternion referenceBodyRotation;

        public bool HasCompleteConfiguration => actor != null
            && custody != null
            && condition != null
            && humanoidAnimator != null
            && humanoidAnimator.avatar != null
            && humanoidAnimator.avatar.isValid
            && humanoidAnimator.avatar.isHuman
            && presentationRenderers != null
            && presentationRenderers.Length > 0;

        public void Configure(
            HumanActorController configuredActor,
            CustodyComponent configuredCustody,
            ActorCondition configuredCondition,
            Animator configuredAnimator)
        {
            actor = configuredActor;
            custody = configuredCustody;
            condition = configuredCondition;
            humanoidAnimator = configuredAnimator;
            presentationRenderers = configuredAnimator != null
                ? configuredAnimator.GetComponentsInChildren<Renderer>(true)
                : Array.Empty<Renderer>();
            EnsurePresentationVisible();
            ResetPoseHandler();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            EnsurePresentationVisible();
            EnsurePoseHandler();
        }

        private void OnDisable()
        {
            DisposePoseHandler();
        }

        private void LateUpdate()
        {
            if (!EnsurePoseHandler())
            {
                return;
            }

            RestoreReferenceRoot();
            TemporaryCharacterPose targetPose = TemporaryCharacterPoseRules.Resolve(
                actor.State,
                custody.State,
                condition.Snapshot.Level);
            poseHandler.GetHumanPose(ref humanPose);
            if (humanPose.muscles == null
                || humanPose.muscles.Length != referenceMuscles.Length)
            {
                return;
            }

            Array.Copy(referenceMuscles, targetMuscles, referenceMuscles.Length);
            ApplyTargetPose(targetPose, targetMuscles);
            float blend = 1f - Mathf.Exp(-responseSpeed * Time.deltaTime);
            for (int index = 0; index < humanPose.muscles.Length; index++)
            {
                humanPose.muscles[index] = Mathf.Lerp(
                    humanPose.muscles[index],
                    targetMuscles[index],
                    blend);
            }

            // HumanPoseHandler can write normalized body translation back through
            // the imported skeleton. Keep the temporary art anchored to the actor;
            // authoritative movement remains on the actor/NavMeshAgent parent.
            humanPose.bodyPosition = referenceBodyPosition;
            humanPose.bodyRotation = referenceBodyRotation;
            poseHandler.SetHumanPose(ref humanPose);
            RestoreReferenceRoot();
        }

        private void ResolveReferences()
        {
            actor ??= GetComponent<HumanActorController>();
            custody ??= GetComponent<CustodyComponent>();
            condition ??= GetComponent<ActorCondition>();
            if (humanoidAnimator == null)
            {
                humanoidAnimator = GetComponentInChildren<Animator>(true);
            }

            if ((presentationRenderers == null || presentationRenderers.Length == 0)
                && humanoidAnimator != null)
            {
                presentationRenderers = humanoidAnimator
                    .GetComponentsInChildren<Renderer>(true);
            }
        }

        private void EnsurePresentationVisible()
        {
            if (presentationRenderers == null)
            {
                return;
            }

            foreach (Renderer renderer in presentationRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                Transform current = renderer.transform;
                while (current != null)
                {
                    current.gameObject.SetActive(true);
                    if (humanoidAnimator != null
                        && current == humanoidAnimator.transform)
                    {
                        break;
                    }

                    current = current.parent;
                }

                renderer.enabled = true;
                renderer.forceRenderingOff = false;
                if (renderer is SkinnedMeshRenderer skinnedRenderer)
                {
                    skinnedRenderer.updateWhenOffscreen = true;
                }
            }
        }

        private bool EnsurePoseHandler()
        {
            if (poseHandler != null)
            {
                return true;
            }

            if (initializationFailed)
            {
                return false;
            }

            ResolveReferences();
            if (!HasCompleteConfiguration)
            {
                initializationFailed = true;
                ProjectLog.Error(
                    "Temporary Character",
                    $"{name} requires a valid Humanoid avatar. Rerun the temporary-character setup tool.",
                    this);
                return false;
            }

            try
            {
                poseHandler = new HumanPoseHandler(
                    humanoidAnimator.avatar,
                    humanoidAnimator.transform);
                poseHandler.GetHumanPose(ref humanPose);
                if (humanPose.muscles == null || humanPose.muscles.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Unity created no humanoid muscle data for the temporary character.");
                }

                referenceMuscles = (float[])humanPose.muscles.Clone();
                targetMuscles = new float[referenceMuscles.Length];
                CaptureReferenceRoot();
                referenceBodyPosition = humanPose.bodyPosition;
                referenceBodyRotation = humanPose.bodyRotation;
                indices = MuscleIndices.Create();
                return true;
            }
            catch (Exception exception)
            {
                initializationFailed = true;
                DisposePoseHandler();
                ProjectLog.Exception("Temporary Character", exception, this);
                return false;
            }
        }

        private void ResetPoseHandler()
        {
            initializationFailed = false;
            referenceRootCaptured = false;
            DisposePoseHandler();
            if (Application.isPlaying && isActiveAndEnabled)
            {
                EnsurePoseHandler();
            }
        }

        private void DisposePoseHandler()
        {
            poseHandler?.Dispose();
            poseHandler = null;
            referenceMuscles = null;
            targetMuscles = null;
        }

        private void CaptureReferenceRoot()
        {
            if (humanoidAnimator == null)
            {
                return;
            }

            Transform root = humanoidAnimator.transform;
            referenceRootLocalPosition = root.localPosition;
            referenceRootLocalRotation = root.localRotation;
            referenceRootLocalScale = root.localScale;
            referenceRootCaptured = true;
        }

        private void RestoreReferenceRoot()
        {
            if (!referenceRootCaptured || humanoidAnimator == null)
            {
                return;
            }

            Transform root = humanoidAnimator.transform;
            root.localPosition = referenceRootLocalPosition;
            root.localRotation = referenceRootLocalRotation;
            root.localScale = referenceRootLocalScale;
        }

        private void ApplyTargetPose(
            TemporaryCharacterPose pose,
            float[] muscles)
        {
            switch (pose)
            {
                case TemporaryCharacterPose.Alert:
                    Set(muscles, indices.LeftArmDownUp, -0.22f);
                    Set(muscles, indices.RightArmDownUp, -0.22f);
                    Set(muscles, indices.LeftArmFrontBack, -0.52f);
                    Set(muscles, indices.RightArmFrontBack, -0.52f);
                    Set(muscles, indices.LeftForearmStretch, -0.38f);
                    Set(muscles, indices.RightForearmStretch, -0.38f);
                    Set(muscles, indices.SpineFrontBack, -0.08f);
                    break;

                case TemporaryCharacterPose.Surrendering:
                    Set(muscles, indices.LeftArmDownUp, 0.62f);
                    Set(muscles, indices.RightArmDownUp, 0.62f);
                    Set(muscles, indices.LeftArmFrontBack, -0.18f);
                    Set(muscles, indices.RightArmFrontBack, -0.18f);
                    Set(muscles, indices.LeftForearmStretch, -0.22f);
                    Set(muscles, indices.RightForearmStretch, -0.22f);
                    Set(muscles, indices.LeftHandDownUp, 0.18f);
                    Set(muscles, indices.RightHandDownUp, 0.18f);
                    break;

                case TemporaryCharacterPose.Kneeling:
                    ApplyArmsDown(muscles, -0.52f);
                    Set(muscles, indices.LeftUpperLegFrontBack, 0.34f);
                    Set(muscles, indices.RightUpperLegFrontBack, 0.34f);
                    Set(muscles, indices.LeftLowerLegStretch, -0.8f);
                    Set(muscles, indices.RightLowerLegStretch, -0.8f);
                    Set(muscles, indices.SpineFrontBack, 0.14f);
                    break;

                case TemporaryCharacterPose.Incapacitated:
                    ApplyArmsDown(muscles, 0.05f);
                    Set(muscles, indices.LeftForearmStretch, -0.46f);
                    Set(muscles, indices.RightForearmStretch, -0.28f);
                    Set(muscles, indices.LeftUpperLegFrontBack, 0.2f);
                    Set(muscles, indices.RightUpperLegFrontBack, -0.12f);
                    Set(muscles, indices.LeftLowerLegStretch, -0.36f);
                    Set(muscles, indices.RightLowerLegStretch, -0.18f);
                    break;

                default:
                    ApplyArmsDown(muscles, -0.82f);
                    Set(muscles, indices.LeftForearmStretch, 0.82f);
                    Set(muscles, indices.RightForearmStretch, 0.82f);
                    break;
            }
        }

        private void ApplyArmsDown(float[] muscles, float armValue)
        {
            Set(muscles, indices.LeftArmDownUp, armValue);
            Set(muscles, indices.RightArmDownUp, armValue);
            Set(muscles, indices.LeftArmFrontBack, 0f);
            Set(muscles, indices.RightArmFrontBack, 0f);
        }

        private static void Set(float[] muscles, int index, float value)
        {
            if (index >= 0 && index < muscles.Length)
            {
                muscles[index] = Mathf.Clamp(value, -1f, 1f);
            }
        }

        private struct MuscleIndices
        {
            public int SpineFrontBack;
            public int LeftArmDownUp;
            public int LeftArmFrontBack;
            public int RightArmDownUp;
            public int RightArmFrontBack;
            public int LeftForearmStretch;
            public int RightForearmStretch;
            public int LeftHandDownUp;
            public int RightHandDownUp;
            public int LeftUpperLegFrontBack;
            public int RightUpperLegFrontBack;
            public int LeftLowerLegStretch;
            public int RightLowerLegStretch;

            public static MuscleIndices Create()
            {
                return new MuscleIndices
                {
                    SpineFrontBack = Find("Spine Front-Back"),
                    LeftArmDownUp = Find("Left Arm Down-Up"),
                    LeftArmFrontBack = Find("Left Arm Front-Back"),
                    RightArmDownUp = Find("Right Arm Down-Up"),
                    RightArmFrontBack = Find("Right Arm Front-Back"),
                    LeftForearmStretch = Find("Left Forearm Stretch"),
                    RightForearmStretch = Find("Right Forearm Stretch"),
                    LeftHandDownUp = Find("Left Hand Down-Up"),
                    RightHandDownUp = Find("Right Hand Down-Up"),
                    LeftUpperLegFrontBack = Find("Left Upper Leg Front-Back"),
                    RightUpperLegFrontBack = Find("Right Upper Leg Front-Back"),
                    LeftLowerLegStretch = Find("Left Lower Leg Stretch"),
                    RightLowerLegStretch = Find("Right Lower Leg Stretch")
                };
            }

            private static int Find(string muscleName)
            {
                return Array.IndexOf(HumanTrait.MuscleName, muscleName);
            }
        }
    }
}
