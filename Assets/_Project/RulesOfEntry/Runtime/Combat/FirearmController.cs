using RulesOfEntry.Core;
using RulesOfEntry.Input;
using RulesOfEntry.Player;
using UnityEngine;

namespace RulesOfEntry.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TacticalPlayerInput))]
    [RequireComponent(typeof(UseOfForceEventLedger))]
    public sealed class FirearmController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TacticalPlayerInput playerInput;
        [SerializeField] private FirstPersonLook firstPersonLook;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform muzzle;
        [SerializeField] private FirearmView firearmView;
        [SerializeField] private UseOfForceEventLedger forceEventLedger;
        [SerializeField] private FirearmDefinition firearmDefinition;
        [SerializeField] private AmmunitionDefinition ammunitionDefinition;
        [SerializeField] private LayerMask ballisticHitMask = ~0;

        [Header("Initial Physical Loadout")]
        [SerializeField, Min(0)] private int initialInsertedMagazineRounds = 29;
        [SerializeField] private bool initialChamberLoaded = true;
        [SerializeField] private int[] initialSpareMagazineRounds = { 30, 30, 30 };
        [SerializeField] private FireSelectorPosition initialSelector = FireSelectorPosition.Safe;
        [SerializeField] private WeaponReadyPosition initialReadyPosition =
            WeaponReadyPosition.LowReady;

        private FirearmStateMachine stateMachine;
        private FirearmOperation operation;
        private float operationElapsed;
        private float operationDuration;
        private bool pendingEmergencyReload;
        private float fireCooldown;
        private float weaponRaiseProgress;
        private bool initialized;

        public FirearmSnapshot Snapshot => stateMachine != null ? stateMachine.Snapshot : default;
        public FirearmOperation Operation => operation;
        public float OperationProgress => operationDuration > 0f
            ? Mathf.Clamp01(operationElapsed / operationDuration)
            : 0f;
        public WeaponReadyPosition ReadyPosition { get; private set; }
        public bool IsAiming => initialized
            && playerInput != null
            && playerInput.GameplayEnabled
            && !HasSprintIntent
            && playerInput.AimHeld
            && operation == FirearmOperation.Idle;
        public WeaponReadyPosition EffectiveReadyPosition => HasSprintIntent
            ? WeaponReadyPosition.LowReady
            : IsAiming
                ? WeaponReadyPosition.Shouldered
                : ReadyPosition;
        public string CurrentStatusMessage { get; private set; } = string.Empty;
        public float StatusMessageRemaining { get; private set; }
        public bool AutomaticReloadEnabled => false;
        public bool WeaponIsRaised => weaponRaiseProgress >= 0.98f;
        public bool HasCompleteConfiguration => playerInput != null
            && firstPersonLook != null
            && viewCamera != null
            && muzzle != null
            && firearmView != null
            && forceEventLedger != null
            && firearmDefinition != null
            && ammunitionDefinition != null;

        private bool HasSprintIntent => initialized
            && playerInput != null
            && playerInput.SprintHeld
            && playerInput.Move.y > 0.1f;

        public void Configure(
            TacticalPlayerInput configuredInput,
            FirstPersonLook configuredLook,
            Camera configuredCamera,
            Transform configuredMuzzle,
            FirearmView configuredView,
            UseOfForceEventLedger configuredLedger,
            FirearmDefinition configuredFirearm,
            AmmunitionDefinition configuredAmmunition,
            LayerMask configuredHitMask)
        {
            playerInput = configuredInput;
            firstPersonLook = configuredLook;
            viewCamera = configuredCamera;
            muzzle = configuredMuzzle;
            firearmView = configuredView;
            forceEventLedger = configuredLedger;
            firearmDefinition = configuredFirearm;
            ammunitionDefinition = configuredAmmunition;
            ballisticHitMask = configuredHitMask;
            initialized = false;
        }

        public void ConfigureInitialLoadout(
            int insertedMagazineRounds,
            bool chamberLoaded,
            int[] spareMagazineRounds,
            FireSelectorPosition selector,
            WeaponReadyPosition readyPosition)
        {
            initialInsertedMagazineRounds = Mathf.Max(0, insertedMagazineRounds);
            initialChamberLoaded = chamberLoaded;
            initialSpareMagazineRounds = spareMagazineRounds ?? System.Array.Empty<int>();
            initialSelector = selector;
            initialReadyPosition = readyPosition;
            initialized = false;
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (!initialized && !Initialize())
            {
                return;
            }

            fireCooldown = Mathf.Max(0f, fireCooldown - Time.deltaTime);
            StatusMessageRemaining = Mathf.Max(0f, StatusMessageRemaining - Time.deltaTime);
            UpdateOperation(Time.deltaTime);
            UpdateWeaponRaise(Time.deltaTime);

            if (playerInput.GameplayEnabled && Cursor.lockState == CursorLockMode.Locked)
            {
                HandlePlayerInput();
            }

            firearmView.SetPresentation(
                EffectiveReadyPosition,
                IsAiming,
                operation,
                OperationProgress,
                stateMachine.Snapshot.HasInsertedMagazine);
        }

        private bool Initialize()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<TacticalPlayerInput>();
            }

            if (firstPersonLook == null)
            {
                firstPersonLook = GetComponent<FirstPersonLook>();
            }

            if (forceEventLedger == null)
            {
                forceEventLedger = GetComponent<UseOfForceEventLedger>();
            }

            if (muzzle == null && firearmView != null)
            {
                muzzle = firearmView.Muzzle;
            }

            initialized = HasCompleteConfiguration;

            if (!initialized)
            {
                ProjectLog.Error(
                    "Firearm",
                    "Milestone 2 firearm references are incomplete. Run the Milestone 2 setup tool.",
                    this);
                return false;
            }

            int capacity = firearmDefinition.MagazineCapacity;
            int insertedRounds = Mathf.Clamp(initialInsertedMagazineRounds, 0, capacity);
            int[] spareRounds = new int[initialSpareMagazineRounds.Length];
            for (int index = 0; index < spareRounds.Length; index++)
            {
                spareRounds[index] = Mathf.Clamp(initialSpareMagazineRounds[index], 0, capacity);
            }

            stateMachine = new FirearmStateMachine(
                capacity,
                insertedRounds,
                initialChamberLoaded,
                spareRounds,
                initialSelector);
            ReadyPosition = initialReadyPosition;
            operation = FirearmOperation.Idle;
            PublishStatus("SAFE • LOW READY", 2.5f);
            return true;
        }

        private void HandlePlayerInput()
        {
            if (operation != FirearmOperation.Idle)
            {
                return;
            }

            if (HasSprintIntent)
            {
                bool weaponActionRequested = playerInput.FirePressedThisFrame
                    || playerInput.ReloadPressedThisFrame
                    || playerInput.CheckMagazinePressedThisFrame
                    || playerInput.ToggleReadyPressedThisFrame
                    || playerInput.CycleFireSelectorPressedThisFrame
                    || playerInput.CycleActionPressedThisFrame;
                if (weaponActionRequested)
                {
                    PublishStatus("WEAPON CONTROL UNAVAILABLE WHILE SPRINTING", 1.5f);
                }

                return;
            }

            if (playerInput.CycleFireSelectorPressedThisFrame)
            {
                FireSelectorPosition selector = stateMachine.CycleSelector();
                PublishStatus(
                    selector == FireSelectorPosition.Safe ? "SELECTOR: SAFE" : "SELECTOR: SEMI",
                    1.5f);
                return;
            }

            if (playerInput.ToggleReadyPressedThisFrame)
            {
                ReadyPosition = ReadyPosition == WeaponReadyPosition.LowReady
                    ? WeaponReadyPosition.Shouldered
                    : WeaponReadyPosition.LowReady;
                PublishStatus(
                    ReadyPosition == WeaponReadyPosition.LowReady
                        ? "LOW READY"
                        : "WEAPON SHOULDERED",
                    1.25f);
                return;
            }

            if (playerInput.CheckMagazinePressedThisFrame)
            {
                StartMagazineCheck();
                return;
            }

            if (playerInput.ReloadPressedThisFrame)
            {
                StartReload(playerInput.EmergencyReloadModifierHeld);
                return;
            }

            if (playerInput.CycleActionPressedThisFrame)
            {
                StartCycleAction();
                return;
            }

            if (playerInput.FirePressedThisFrame)
            {
                TryFire();
            }
        }

        private void UpdateWeaponRaise(float deltaTime)
        {
            bool wantsRaisedWeapon = !HasSprintIntent
                && (ReadyPosition == WeaponReadyPosition.Shouldered
                    || (playerInput.AimHeld && operation == FirearmOperation.Idle));
            float target = wantsRaisedWeapon ? 1f : 0f;
            float rate = 1f / Mathf.Max(0.05f, firearmDefinition.WeaponRaiseDuration);
            weaponRaiseProgress = Mathf.MoveTowards(
                weaponRaiseProgress,
                target,
                rate * deltaTime);
        }

        private void StartMagazineCheck()
        {
            if (!stateMachine.HasInsertedMagazine)
            {
                PublishStatus("NO MAGAZINE INSERTED", 2f);
                return;
            }

            BeginOperation(
                FirearmOperation.CheckingMagazine,
                firearmDefinition.MagazineCheckDuration);
            PublishStatus("CHECKING MAGAZINE…", firearmDefinition.MagazineCheckDuration);
        }

        private void StartReload(bool emergency)
        {
            if (stateMachine.SpareMagazineCount <= 0)
            {
                PublishStatus("NO SPARE MAGAZINE", 2.5f);
                return;
            }

            pendingEmergencyReload = emergency;
            float duration = emergency
                ? firearmDefinition.EmergencyReloadDuration
                : firearmDefinition.RetainedReloadDuration;
            BeginOperation(FirearmOperation.Reloading, duration);
            PublishStatus(
                emergency ? "EMERGENCY RELOAD — MAGAZINE DROPPED" : "RETAINED RELOAD",
                duration);
        }

        private void StartCycleAction()
        {
            BeginOperation(
                FirearmOperation.CyclingAction,
                firearmDefinition.CycleActionDuration);
            PublishStatus("CYCLING ACTION…", firearmDefinition.CycleActionDuration);
        }

        private void BeginOperation(FirearmOperation nextOperation, float duration)
        {
            operation = nextOperation;
            operationElapsed = 0f;
            operationDuration = Mathf.Max(0.01f, duration);
        }

        private void UpdateOperation(float deltaTime)
        {
            if (operation == FirearmOperation.Idle)
            {
                return;
            }

            operationElapsed += deltaTime;
            if (operationElapsed < operationDuration)
            {
                return;
            }

            FirearmOperation completedOperation = operation;
            operation = FirearmOperation.Idle;
            operationElapsed = 0f;
            operationDuration = 0f;

            switch (completedOperation)
            {
                case FirearmOperation.CheckingMagazine:
                    CompleteMagazineCheck();
                    break;
                case FirearmOperation.Reloading:
                    CompleteReload();
                    break;
                case FirearmOperation.CyclingAction:
                    CompleteCycleAction();
                    break;
            }
        }

        private void CompleteMagazineCheck()
        {
            MagazineEstimate estimate = stateMachine.CheckInsertedMagazine();
            PublishStatus("MAGAZINE: " + GetMagazineEstimateText(estimate), 3.5f);
        }

        private void CompleteReload()
        {
            if (!stateMachine.TryReload(pendingEmergencyReload, out ReloadResult result))
            {
                PublishStatus("RELOAD FAILED — NO SPARE MAGAZINE", 2.5f);
                return;
            }

            if (!result.Snapshot.ChamberLoaded)
            {
                PublishStatus("MAGAZINE INSERTED — CYCLE ACTION", 3f);
            }
            else if (result.ChamberedByBoltRelease)
            {
                PublishStatus("RELOAD COMPLETE — BOLT RELEASED", 2f);
            }
            else
            {
                PublishStatus("RELOAD COMPLETE", 1.5f);
            }
        }

        private void CompleteCycleAction()
        {
            CycleActionResult result = stateMachine.CycleAction();
            if (result.EjectedLiveRound)
            {
                PublishStatus(
                    result.ChamberedRound
                        ? "LIVE ROUND EJECTED — NEW ROUND CHAMBERED"
                        : "LIVE ROUND EJECTED — CHAMBER EMPTY",
                    3f);
            }
            else
            {
                PublishStatus(
                    result.ChamberedRound ? "ROUND CHAMBERED" : "CHAMBER EMPTY",
                    2f);
            }
        }

        private void TryFire()
        {
            if (fireCooldown > 0f)
            {
                return;
            }

            if (EffectiveReadyPosition == WeaponReadyPosition.LowReady || !WeaponIsRaised)
            {
                PublishStatus(
                    EffectiveReadyPosition == WeaponReadyPosition.LowReady
                        ? "LOW READY — RAISE WEAPON"
                        : "RAISING WEAPON",
                    1.5f);
                return;
            }

            FireAttemptResult result = stateMachine.TryFire();
            if (!result.Discharged)
            {
                switch (result.FailureReason)
                {
                    case FireFailureReason.SafetyOn:
                        PublishStatus("SAFE", 1.25f);
                        break;
                    case FireFailureReason.BoltLocked:
                        PublishStatus("BOLT LOCKED — MANUAL RELOAD REQUIRED", 2.5f);
                        break;
                    default:
                        PublishStatus("EMPTY CHAMBER — CYCLE ACTION OR RELOAD", 2.5f);
                        break;
                }

                return;
            }

            fireCooldown = firearmDefinition.MinimumSecondsBetweenShots;
            PerformDischarge(result.Snapshot);
            firearmView.PlayRecoil();
            float horizontalRecoil = Random.Range(
                -firearmDefinition.HorizontalRecoilDegrees,
                firearmDefinition.HorizontalRecoilDegrees);
            firstPersonLook.ApplyRecoil(
                firearmDefinition.VerticalRecoilDegrees,
                horizontalRecoil);
        }

        private void PerformDischarge(FirearmSnapshot postShotSnapshot)
        {
            float maximumRange = ammunitionDefinition.MaximumSimulationRangeMeters;
            Ray cameraRay = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            Vector3 intendedPoint = cameraRay.origin + cameraRay.direction * maximumRange;
            if (Physics.Raycast(
                cameraRay,
                out RaycastHit cameraHit,
                maximumRange,
                ballisticHitMask,
                QueryTriggerInteraction.Ignore))
            {
                intendedPoint = cameraHit.point;
            }

            Vector3 origin = muzzle.position;
            Vector3 direction = (intendedPoint - origin).normalized;
            float shotDistance = Mathf.Min(maximumRange, Vector3.Distance(origin, intendedPoint));
            RaycastHit? recordedHit = null;

            if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit muzzleHit,
                shotDistance,
                ballisticHitMask,
                QueryTriggerInteraction.Ignore))
            {
                recordedHit = muzzleHit;
            }

            forceEventLedger.RecordFirearmDischarge(
                gameObject,
                firearmDefinition,
                ammunitionDefinition,
                EffectiveReadyPosition,
                origin,
                direction,
                recordedHit,
                postShotSnapshot);

            if (recordedHit.HasValue)
            {
                DeliverBallisticHit(recordedHit.Value, origin, direction);
            }
        }

        private void DeliverBallisticHit(RaycastHit raycastHit, Vector3 origin, Vector3 direction)
        {
            BallisticHit ballisticHit = new BallisticHit(
                gameObject,
                firearmDefinition.FirearmId,
                ammunitionDefinition.AmmunitionId,
                origin,
                direction,
                raycastHit.point,
                raycastHit.normal,
                ammunitionDefinition.MuzzleEnergyJoules,
                raycastHit.collider);

            MonoBehaviour[] behaviours = raycastHit.collider
                .GetComponentsInParent<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IBallisticHitReceiver receiver)
                {
                    receiver.ReceiveBallisticHit(ballisticHit);
                    break;
                }
            }
        }

        private void PublishStatus(string message, float duration)
        {
            CurrentStatusMessage = message ?? string.Empty;
            StatusMessageRemaining = Mathf.Max(0f, duration);
        }

        private static string GetMagazineEstimateText(MagazineEstimate estimate)
        {
            return estimate switch
            {
                MagazineEstimate.Empty => "EMPTY",
                MagazineEstimate.NearlyEmpty => "NEARLY EMPTY",
                MagazineEstimate.Low => "LOW",
                MagazineEstimate.Partial => "PARTIAL",
                MagazineEstimate.MostlyFull => "MOSTLY FULL",
                MagazineEstimate.Full => "FULL",
                _ => "NO MAGAZINE"
            };
        }
    }
}
