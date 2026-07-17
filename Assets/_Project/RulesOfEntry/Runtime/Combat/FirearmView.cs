using UnityEngine;

namespace RulesOfEntry.Combat
{
    [DisallowMultipleComponent]
    public sealed class FirearmView : MonoBehaviour
    {
        [SerializeField] private Transform weaponRig;
        [SerializeField] private Transform muzzle;
        [SerializeField] private GameObject insertedMagazineVisual;
        [SerializeField, Min(1f)] private float positionSharpness = 18f;
        [SerializeField, Min(1f)] private float rotationSharpness = 18f;

        private readonly Vector3 lowReadyPosition = new Vector3(0.29f, -0.34f, 0.52f);
        private readonly Vector3 shoulderedPosition = new Vector3(0.18f, -0.21f, 0.5f);
        private readonly Vector3 aimedPosition = new Vector3(0f, -0.185f, 0.43f);
        private readonly Vector3 lowReadyRotation = new Vector3(18f, -7f, 8f);
        private readonly Vector3 shoulderedRotation = new Vector3(2f, -2f, 1f);
        private readonly Vector3 aimedRotation = Vector3.zero;

        private WeaponReadyPosition readyPosition;
        private bool aiming;
        private FirearmOperation operation;
        private float operationProgress;
        private float recoilOffset;

        public Transform Muzzle => muzzle;

        public void Configure(
            Transform configuredRig,
            Transform configuredMuzzle,
            GameObject configuredMagazineVisual)
        {
            weaponRig = configuredRig;
            muzzle = configuredMuzzle;
            insertedMagazineVisual = configuredMagazineVisual;
        }

        public void SetPresentation(
            WeaponReadyPosition configuredReadyPosition,
            bool configuredAiming,
            FirearmOperation configuredOperation,
            float configuredOperationProgress,
            bool hasInsertedMagazine)
        {
            readyPosition = configuredReadyPosition;
            aiming = configuredAiming;
            operation = configuredOperation;
            operationProgress = Mathf.Clamp01(configuredOperationProgress);
            if (insertedMagazineVisual != null)
            {
                insertedMagazineVisual.SetActive(hasInsertedMagazine);
            }
        }

        public void PlayRecoil()
        {
            recoilOffset = Mathf.Min(1f, recoilOffset + 1f);
        }

        private void LateUpdate()
        {
            if (weaponRig == null)
            {
                return;
            }

            Vector3 targetPosition;
            Vector3 targetEuler;
            if (aiming)
            {
                targetPosition = aimedPosition;
                targetEuler = aimedRotation;
            }
            else if (readyPosition == WeaponReadyPosition.Shouldered)
            {
                targetPosition = shoulderedPosition;
                targetEuler = shoulderedRotation;
            }
            else
            {
                targetPosition = lowReadyPosition;
                targetEuler = lowReadyRotation;
            }

            ApplyOperationPose(ref targetPosition, ref targetEuler);
            targetPosition += new Vector3(0f, 0f, -0.045f * recoilOffset);
            targetEuler += new Vector3(-2.4f * recoilOffset, 0f, 0f);

            float positionT = 1f - Mathf.Exp(-positionSharpness * Time.deltaTime);
            float rotationT = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
            weaponRig.localPosition = Vector3.Lerp(weaponRig.localPosition, targetPosition, positionT);
            weaponRig.localRotation = Quaternion.Slerp(
                weaponRig.localRotation,
                Quaternion.Euler(targetEuler),
                rotationT);
            recoilOffset = Mathf.MoveTowards(recoilOffset, 0f, 8f * Time.deltaTime);
        }

        private void ApplyOperationPose(ref Vector3 position, ref Vector3 euler)
        {
            float arc = Mathf.Sin(operationProgress * Mathf.PI);
            switch (operation)
            {
                case FirearmOperation.CheckingMagazine:
                    position += new Vector3(-0.12f, -0.1f * arc, -0.08f);
                    euler += new Vector3(12f * arc, -30f * arc, 18f * arc);
                    break;
                case FirearmOperation.Reloading:
                    position += new Vector3(0.08f, -0.26f * arc, -0.1f);
                    euler += new Vector3(18f * arc, 15f * arc, -30f * arc);
                    break;
                case FirearmOperation.CyclingAction:
                    position += new Vector3(0.06f * arc, -0.04f * arc, -0.08f * arc);
                    euler += new Vector3(0f, 8f * arc, -10f * arc);
                    break;
            }
        }
    }
}
