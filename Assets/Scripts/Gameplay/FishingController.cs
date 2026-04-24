using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MultiplayFishing.Gameplay
{
    public class FishingController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private FishingBiteSystem biteSystem;
        [SerializeField] private FishingCatchPresenter catchPresenter;
        [SerializeField] private Animator animator;
        [SerializeField] private string fishingParameter = "fishing";
        [SerializeField] private string hasFishParameter = "HasFish";
        [SerializeField] private string rodEquippedParameter = "RodEquipped";
        [SerializeField] private string rodTakeOutTrigger = "RodTakeOut";
        [SerializeField] private string rodPutAwayTrigger = "RodPutAway";

        [Header("Fishing Visuals")]
        [SerializeField] private FishingLineVisual fishingLineVisual;
        [SerializeField] private GameObject fishingRopeObject;
        [SerializeField] private ParticleSystem fishingSplashParticle;
        [SerializeField] private BobberWaveEffect bobberWaveEffect;
        [SerializeField] private FishingClickChallengeUI clickChallengeUI;

        [Header("Rod Visibility")]
        [SerializeField] private GameObject rodVisualRoot;
        [SerializeField] private string rodHideStateName = "fishing-out";
        [SerializeField] private string rodShowStateName = "fishing-in";

        [Header("Fishing References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform tipPoint;
        [SerializeField] private Transform hookPoint;
        [SerializeField] private Transform idleAnchorPoint; // 손 뼈대 Transform 연결 시 hookPoint 복귀 기준점으로 사용
        [SerializeField] private Transform waterSurfaceTransform;

        [Header("Cast Settings")]
        [SerializeField] private LayerMask waterLayerMask;
        [SerializeField] private float waterRayStartHeight = 1.5f;
        [SerializeField] private float downwardCastBias = 0.2f;
        [SerializeField] private float fallbackCastDistance = 6f;
        [SerializeField] private float maxCastDistance = 12f;
        [SerializeField] private float castInputLockDuration = 1.0f;
        [SerializeField] private float reelInputLockDuration = 0.8f;
        [SerializeField] private bool blockCastWhileMoving = true;
        [SerializeField] private bool useCastReleaseAnimationEvent = true;
        [SerializeField] private float castStartDelay = 0.18f;
        [SerializeField] private float castDuration = 0.45f;
        [SerializeField] private float reelDuration = 0.8f;
        [SerializeField] private float reelDurationWithFish = 1.2f;
        [SerializeField] private float castArcHeight = 0.35f;
        [SerializeField] private float reelArcHeight = 0.2f;
        [SerializeField] private float idleRopeLength = 1.8f;
        [SerializeField] private float castRopeLength = 1.8f;
        [SerializeField] private float idleRopeSlack = 0.1f;
        [SerializeField] private float castRopeSlack = 0.05f;
        [SerializeField] private float hookWaterSubmergeDepth = 0.08f;
        [Tooltip("물 오브젝트의 Transform.position.y와 실제 시각적 수면 사이의 높이 차이 보정")]
        [SerializeField] private float waterSurfaceYOffset = 0f;
        [SerializeField] private Vector3 castTargetOffset = Vector3.zero;
        [SerializeField] private Vector3 idleHookOffset = new Vector3(0f, 0f, 0.1f);
        [SerializeField] private Vector3 splashWorldOffset = new Vector3(0f, 0.01f, 0f);
        [SerializeField] private float splashDelay = 0.05f;
        [SerializeField] private bool clampSplashToWaterSurface = true;
        [SerializeField] private float minimumSplashHeightOffset = 0.02f;
        [SerializeField] private bool useSplashEffect = true;

        [Header("Line Style")]
        [SerializeField] private float lineWidth = 0.03f;

        [Header("Hook Bobbing")]
        [SerializeField] private bool enableBobbing = true;
        [SerializeField] private float bobAmplitude = 0.03f;
        [SerializeField] private float bobFrequency = 1.2f;
        [Tooltip("2차 파동 비율 (자연스러운 불규칙 움직임)")]
        [SerializeField] private float bobSecondaryRatio = 0.3f;
        [SerializeField] private float bobSecondaryFrequencyMultiplier = 1.7f;
        [SerializeField] private float swayAmplitude = 0.01f;
        [SerializeField] private float swayFrequency = 0.8f;
        [Tooltip("보빙 시작 시 부드럽게 전환되는 시간(초)")]
        [SerializeField] private float bobFadeInDuration = 0.5f;
        [SerializeField] private float bobberWaveDelay = 2.0f;

        private FishingRopeController fishingRopeController;
        private FishingSplashController fishingSplashController;

        private Coroutine hookMoveRoutine;
        private Coroutine splashRoutine;
        private int fishingParameterHash;
        private int rodEquippedParameterHash;
        private int rodTakeOutTriggerHash;
        private int rodPutAwayTriggerHash;
        private int rodHideStateHash;
        private int rodShowStateHash;
        private bool isFishingActive;
        private bool hasFishingParameter;
        private bool hasRodEquippedParameter;
        private bool hasRodTakeOutTrigger;
        private bool hasRodPutAwayTrigger;
        private bool isRodEquipped;
        private float inputLockedUntil;
        private bool castReleaseReceived;
        private bool wasInRodHideState;
        private bool wasInRodShowState;

        private Vector3 bobbingBasePosition;
        private float bobbingStartTime;
        private bool isBobbingActive;
        private Coroutine waveEffectRoutine;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            fishingParameterHash = Animator.StringToHash(fishingParameter);
            rodEquippedParameterHash = Animator.StringToHash(rodEquippedParameter);
            rodTakeOutTriggerHash = Animator.StringToHash(rodTakeOutTrigger);
            rodPutAwayTriggerHash = Animator.StringToHash(rodPutAwayTrigger);
            rodHideStateHash = Animator.StringToHash(rodHideStateName);
            rodShowStateHash = Animator.StringToHash(rodShowStateName);

            EnsureInitialized();
            ResolveRodVisualRoot();
            catchPresenter?.Initialize(animator, hasFishParameter);
            if (biteSystem != null)
            {
                biteSystem.BiteStarted += HandleBiteStarted;
                biteSystem.BiteEnded += HandleBiteEnded;
            }

            if (clickChallengeUI == null)
            {
                clickChallengeUI = GetComponent<FishingClickChallengeUI>();
            }

            if (clickChallengeUI == null)
            {
                clickChallengeUI = gameObject.AddComponent<FishingClickChallengeUI>();
            }

            clickChallengeUI.ChallengeSucceeded += HandleClickChallengeSucceeded;
            CacheAnimatorParameters();
        }

        private void OnDestroy()
        {
            if (biteSystem != null)
            {
                biteSystem.BiteStarted -= HandleBiteStarted;
                biteSystem.BiteEnded -= HandleBiteEnded;
            }

            if (clickChallengeUI != null)
            {
                clickChallengeUI.ChallengeSucceeded -= HandleClickChallengeSucceeded;
            }
        }

        private void EnsureInitialized()
        {
            RepairHookParentIfNeeded();

            if (fishingRopeController == null)
            {
                Component ropeComp = fishingRopeObject != null ? fishingRopeObject.GetComponent("Rope") : null;
                fishingRopeController = new FishingRopeController(tipPoint, hookPoint, fishingRopeObject, ropeComp);
            }

            if (fishingSplashController == null)
            {
                fishingSplashController = new FishingSplashController(fishingSplashParticle);
            }

            ApplyLineWidth();
        }

        private void Start()
        {
            EnsureInitialized();
            fishingSplashController?.Reset();
            fishingRopeController?.SetHookPosition(GetIdleHookPosition());
            fishingRopeController?.SetVisible(false);
            fishingRopeController?.SetRopeLength(fishingRopeController.GetDesiredRopeLength(GetIdleHookPosition(), idleRopeLength, idleRopeSlack));
        }

        private void Update()
        {
            ApplyLineWidth();
            HandleInput();
            ApplyHookBobbing();
            UpdateRodVisibilityFromAnimator();
        }

        private void HandleInput()
        {
            // EnsureInitialized()를 Update마다 호출하지 않음.
            // 캐스팅 중 hookPoint가 분리(parent=null)된 상태에서 재생성되면
            // originalHookParent=null로 캡처되어 RestoreHookToRod가 동작하지 않게 된다.
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                ToggleRodEquipped();
                return;
            }

            if (Mouse.current == null || !hasFishingParameter || !Mouse.current.leftButton.wasPressedThisFrame) return;

            if (clickChallengeUI != null && clickChallengeUI.IsRunning)
            {
                clickChallengeUI.RegisterClick();
                return;
            }

            // Fishing Start / Fishing Finish 애니메이션 재생 중에는 모든 입력 차단.
            // hookMoveRoutine은 캐스팅(Start)과 릴인(Finish) 애니메이션 구간 동안 실행되므로
            // 별도 플래그 없이 이것을 입력 잠금 신호로 활용한다.
            if (!isRodEquipped)
            {
                return;
            }

            if (hookMoveRoutine != null || Time.time < inputLockedUntil) return;

            // 입질 중일 때 클릭
            if (biteSystem.IsBiteActive)
            {
                clickChallengeUI?.RegisterClick();
                return;
            }

            if (!isFishingActive && blockCastWhileMoving && IsMovementInputPressed())
            {
                return;
            }

            isFishingActive = !isFishingActive;
            animator.SetBool(fishingParameterHash, isFishingActive);
            movement.SetMovementBlocked(isFishingActive);

            if (isFishingActive)
            {
                biteSystem.StopBiteLogic();
                StartFishingProcess(true);
            }
            else
            {
                biteSystem.StopBiteLogic();
                StartFishingProcess(false);
            }
        }

        private void StartFishingProcess(bool isCasting)
        {
            if (hookMoveRoutine != null) StopCoroutine(hookMoveRoutine);

            StopBobbing();
            LockFishingInput(isCasting);

            Vector3 targetPosition = isCasting ? GetCastTargetPosition() : GetIdleHookPosition();
            float duration = isCasting ? castDuration : reelDuration;
            float arc = isCasting ? castArcHeight : reelArcHeight;

            hookMoveRoutine = StartCoroutine(RunHookMoveRoutine(targetPosition, duration, arc, isCasting));
        }

        public void OnCastRelease()
        {
            castReleaseReceived = true;
        }

        private void PerformCatch()
        {
            Debug.Log("<color=yellow>Fish Caught!</color>");

            // catchPresenter가 preview fish를 먼저 캡처한 뒤 StopBiteLogic 호출.
            // StopBiteLogic → BiteEnded → ClearFishPreview 순서로 동기 실행되므로
            // PerformCatch 이전에 StopBiteLogic을 호출하면 preview fish가 삭제되어
            // EnsureFishAttachedToHook이 fish를 새로 생성하고 바로 Destroy 예약하는 버그 발생.
            catchPresenter.PerformCatch(hookPoint, reelDurationWithFish);
            biteSystem.StopBiteLogic();

            isFishingActive = false;
            animator.SetBool(fishingParameterHash, false);
            movement.SetMovementBlocked(false);

            StopBobbing();

            if (hookMoveRoutine != null) StopCoroutine(hookMoveRoutine);
            hookMoveRoutine = StartCoroutine(RunHookMoveRoutine(GetIdleHookPosition(), reelDurationWithFish, reelArcHeight, false));
        }

        private void LockFishingInput(bool isCasting)
        {
            float lockDuration = isCasting
                ? Mathf.Max(castInputLockDuration, castStartDelay + castDuration)
                : Mathf.Max(reelInputLockDuration, reelDuration);

            inputLockedUntil = Mathf.Max(inputLockedUntil, Time.time + lockDuration);
        }

        private bool IsMovementInputPressed()
        {
            if (Keyboard.current == null)
            {
                return false;
            }

            return Keyboard.current.wKey.isPressed
                || Keyboard.current.aKey.isPressed
                || Keyboard.current.sKey.isPressed
                || Keyboard.current.dKey.isPressed
                || Keyboard.current.upArrowKey.isPressed
                || Keyboard.current.leftArrowKey.isPressed
                || Keyboard.current.downArrowKey.isPressed
                || Keyboard.current.rightArrowKey.isPressed;
        }

        private IEnumerator RunHookMoveRoutine(Vector3 targetPos, float duration, float arc, bool isCasting)
        {
            if (isCasting)
            {
                castReleaseReceived = false;

                if (tipPoint != null)
                {
                    fishingRopeController?.RestoreHookToRod(tipPoint.position);
                }
                else
                {
                    fishingRopeController?.RestoreHookToRod();
                }
            }

            System.Action onWaterHit = null;
            if (isCasting && useSplashEffect)
            {
                onWaterHit = () =>
                {
                    FishingWaterSurfaceResolver resolver = CreateWaterSurfaceResolver();
                    resolver.ResolveCastTarget(transform, castTargetOffset, fallbackCastDistance, out bool hit, out Vector3 hitPoint);
                    fishingSplashController.UpdatePendingPosition(
                        hit,
                        hitPoint,
                        targetPos,
                        splashWorldOffset,
                        clampSplashToWaterSurface,
                        minimumSplashHeightOffset);

                    if (splashRoutine != null)
                    {
                        StopCoroutine(splashRoutine);
                    }

                    splashRoutine = StartCoroutine(PlaySplashAfterDelay());
                };
            }

            // 물결 이펙트 지연 실행을 위한 Action
            System.Action onWaterHitForWave = () =>
            {
                if (waveEffectRoutine != null) StopCoroutine(waveEffectRoutine);
                waveEffectRoutine = StartCoroutine(EnableWaveEffectDelayed());
            };

            System.Func<float> startDelayProvider = () =>
            {
                if (!isCasting)
                {
                    return 0f;
                }

                return useCastReleaseAnimationEvent && castReleaseReceived
                    ? 0f
                    : castStartDelay;
            };
            System.Func<float> durationProvider = () => isCasting ? castDuration : reelDuration;
            System.Func<float> arcHeightProvider = () => isCasting ? castArcHeight : reelArcHeight;
            System.Func<float> ropeSlackProvider = () => isCasting ? castRopeSlack : idleRopeSlack;
            System.Func<float> minimumRopeLengthProvider = () => isCasting ? castRopeLength : idleRopeLength;
            System.Func<float> waterSurfaceYProvider = () =>
            {
                FishingWaterSurfaceResolver resolver = CreateWaterSurfaceResolver();
                return resolver.TryGetSurfaceHeight(out float waterSurfaceY)
                    ? waterSurfaceY + waterSurfaceYOffset - hookWaterSubmergeDepth
                    : targetPos.y;
            };

            yield return fishingRopeController.MoveHookDynamic(
                targetPos,
                startDelayProvider,
                durationProvider,
                arcHeightProvider,
                ropeSlackProvider,
                minimumRopeLengthProvider,
                true,
                !isFishingActive,
                true,
                isCasting,
                waterSurfaceYProvider,
                () => { onWaterHit?.Invoke(); onWaterHitForWave?.Invoke(); },
                fishingLineVisual,
                !isCasting ? idleAnchorPoint : null);

            if (isFishingActive && isCasting)
            {
                biteSystem.StartWaitingForBite();
                StartBobbing();
            }

            // RestoreHookToRod는 MoveHook 내부(hideRopeOnComplete=true)에서 이미 호출됨.
            // 여기서 중복 호출하면 tipPoint 기준 좌표로 덮어써서 hookPoint가 허공에 뜬다.
            hookMoveRoutine = null;
        }

        private Vector3 GetCastTargetPosition()
        {
            EnsureInitialized();
            FishingWaterSurfaceResolver resolver = CreateWaterSurfaceResolver();

            Vector3 targetPosition = resolver.ResolveCastTarget(
                transform,
                castTargetOffset,
                fallbackCastDistance,
                out _,
                out _);

            if (resolver.TryGetSurfaceHeight(out float waterSurfaceY))
            {
                targetPosition.y = waterSurfaceY + waterSurfaceYOffset - hookWaterSubmergeDepth;
            }

            return targetPosition;
        }

        private Vector3 GetIdleHookPosition()
        {
            // idleAnchorPoint(손 뼈대)가 연결되어 있으면 그 위치를 idle 복귀 목적지로 사용.
            // 연결되어 있지 않으면 tipPoint 기준 idleHookOffset 위치로 복귀.
            if (idleAnchorPoint != null)
                return idleAnchorPoint.position;

            return fishingRopeController != null
                ? fishingRopeController.GetIdleHookPosition(transform, idleHookOffset)
                : transform.position;
        }

        private IEnumerator PlaySplashAfterDelay()
        {
            if (splashDelay > 0f)
            {
                yield return new WaitForSeconds(splashDelay);
            }

            fishingSplashController?.Play();
            splashRoutine = null;
        }

        private void CacheAnimatorParameters()
        {
            hasFishingParameter = false;
            hasRodEquippedParameter = false;
            hasRodTakeOutTrigger = false;
            hasRodPutAwayTrigger = false;
            foreach (var p in animator.parameters)
            {
                if (p.nameHash == fishingParameterHash)
                {
                    hasFishingParameter = true;
                }

                if (p.nameHash == rodEquippedParameterHash)
                {
                    hasRodEquippedParameter = true;
                }

                if (p.nameHash == rodTakeOutTriggerHash)
                {
                    hasRodTakeOutTrigger = true;
                }

                if (p.nameHash == rodPutAwayTriggerHash)
                {
                    hasRodPutAwayTrigger = true;
                }
            }

            if (hasRodEquippedParameter)
            {
                isRodEquipped = animator.GetBool(rodEquippedParameterHash);
            }
        }

        private void ToggleRodEquipped()
        {
            if (!hasRodEquippedParameter || hookMoveRoutine != null || Time.time < inputLockedUntil)
            {
                return;
            }

            isRodEquipped = !isRodEquipped;
            if (hasRodEquippedParameter)
            {
                animator.SetBool(rodEquippedParameterHash, isRodEquipped);
            }

            if (isRodEquipped && hasRodTakeOutTrigger)
            {
                animator.ResetTrigger(rodPutAwayTriggerHash);
                animator.SetTrigger(rodTakeOutTriggerHash);
            }
            else if (!isRodEquipped && hasRodPutAwayTrigger)
            {
                animator.ResetTrigger(rodTakeOutTriggerHash);
                animator.SetTrigger(rodPutAwayTriggerHash);
            }

            if (!isRodEquipped)
            {
                isFishingActive = false;
                animator.SetBool(fishingParameterHash, false);
                movement.SetMovementBlocked(false);
                biteSystem.StopBiteLogic();
                clickChallengeUI?.CancelChallenge();
                StopBobbing();
                HideFishingRuntimeVisuals();
            }
        }

        private void ResolveRodVisualRoot()
        {
            if (rodVisualRoot != null)
            {
                return;
            }

            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == "RodSocket")
                {
                    rodVisualRoot = child.gameObject;
                    return;
                }
            }
        }

        private void UpdateRodVisibilityFromAnimator()
        {
            if (animator == null || rodVisualRoot == null)
            {
                return;
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool isInRodHideState = stateInfo.shortNameHash == rodHideStateHash;
            bool isInRodShowState = stateInfo.shortNameHash == rodShowStateHash;

            if (isInRodShowState && !wasInRodShowState)
            {
                SetRodVisible(true);
            }

            if (wasInRodHideState && !isInRodHideState)
            {
                SetRodVisible(false);
            }

            wasInRodHideState = isInRodHideState;
            wasInRodShowState = isInRodShowState;
        }

        private void SetRodVisible(bool visible)
        {
            if (rodVisualRoot != null && rodVisualRoot.activeSelf != visible)
            {
                rodVisualRoot.SetActive(visible);
            }

            if (!visible)
            {
                HideFishingRuntimeVisuals();
            }
        }

        private void HideFishingRuntimeVisuals()
        {
            if (hookMoveRoutine != null)
            {
                StopCoroutine(hookMoveRoutine);
                hookMoveRoutine = null;
            }

            if (splashRoutine != null)
            {
                StopCoroutine(splashRoutine);
                splashRoutine = null;
            }

            fishingRopeController?.SetVisible(false);
            fishingRopeController?.RestoreHookToRod();
            fishingLineVisual?.SetHookControlledByRope(false);

            if (bobberWaveEffect != null)
            {
                bobberWaveEffect.SetEffectActive(false);
            }
        }

        private void HandleBiteStarted()
        {
            biteSystem.HoldBiteForChallenge();
            catchPresenter?.ShowFishPreview(hookPoint);
            clickChallengeUI?.BeginChallenge();
        }

        private void HandleBiteEnded()
        {
            clickChallengeUI?.CancelChallenge();
            catchPresenter?.ClearFishPreview();
        }

        private void HandleClickChallengeSucceeded()
        {
            if (biteSystem != null && biteSystem.IsBiteActive)
            {
                PerformCatch();
            }
        }

        private void OnValidate()
        {
            ApplyLineWidth();
        }

        private void ApplyLineWidth()
        {
            float safeWidth = Mathf.Max(0.001f, lineWidth);

            fishingLineVisual?.ApplyLineWidth(safeWidth);

            if (fishingRopeObject == null)
            {
                return;
            }

            LineRenderer ropeLine = fishingRopeObject.GetComponent<LineRenderer>();
            if (ropeLine != null)
            {
                ropeLine.widthMultiplier = safeWidth;
            }

            Component ropeComp = fishingRopeObject.GetComponent("Rope");
            if (ropeComp == null)
            {
                return;
            }

            PropertyInfo ropeWidthProperty = ropeComp.GetType().GetProperty("ropeWidth");
            if (ropeWidthProperty != null && ropeWidthProperty.CanWrite)
            {
                ropeWidthProperty.SetValue(ropeComp, safeWidth);
                return;
            }

            FieldInfo ropeWidthField = ropeComp.GetType().GetField("ropeWidth");
            if (ropeWidthField != null)
            {
                ropeWidthField.SetValue(ropeComp, safeWidth);
            }
        }

        private FishingWaterSurfaceResolver CreateWaterSurfaceResolver()
        {
            return new FishingWaterSurfaceResolver(
                playerCamera,
                tipPoint,
                waterSurfaceTransform,
                waterLayerMask,
                waterRayStartHeight,
                downwardCastBias,
                maxCastDistance);
        }

        private void RepairHookParentIfNeeded()
        {
            if (hookPoint == null || tipPoint == null || tipPoint.parent == null || hookPoint.parent != null)
            {
                return;
            }

            hookPoint.SetParent(tipPoint.parent, false);
            hookPoint.localPosition = tipPoint.localPosition + idleHookOffset;
            hookPoint.localRotation = tipPoint.localRotation;
            hookPoint.localScale = tipPoint.localScale;
        }

        // ── Hook Bobbing ──────────────────────────────────────────

        private void StartBobbing()
        {
            if (!enableBobbing || hookPoint == null) return;

            bobbingBasePosition = hookPoint.position;
            bobbingStartTime = Time.time;
            isBobbingActive = true;
        }

        private void StopBobbing()
        {
            if (waveEffectRoutine != null)
            {
                StopCoroutine(waveEffectRoutine);
                waveEffectRoutine = null;
            }
            
            if (bobberWaveEffect != null)
            {
                bobberWaveEffect.SetEffectActive(false);
            }

            if (!isBobbingActive) return;

            isBobbingActive = false;

            // 보빙 종료 시 기준 위치로 복원
            if (hookPoint != null)
            {
                hookPoint.position = bobbingBasePosition;
            }
        }

        private void ApplyHookBobbing()
        {
            if (!isBobbingActive || hookPoint == null) return;

            float elapsed = Time.time - bobbingStartTime;

            // 부드러운 페이드인
            float fadeIn = bobFadeInDuration > 0f
                ? Mathf.Clamp01(elapsed / bobFadeInDuration)
                : 1f;

            // 1차 파동 + 2차 파동 조합 (자연스러운 불규칙 움직임)
            float t = Time.time;
            float primaryWave = Mathf.Sin(t * bobFrequency * Mathf.PI * 2f);
            float secondaryWave = Mathf.Sin(t * bobFrequency * bobSecondaryFrequencyMultiplier * Mathf.PI * 2f);
            float bobY = (primaryWave + secondaryWave * bobSecondaryRatio) * bobAmplitude * fadeIn;

            // 수평 흔들림 (X, Z)
            float swayX = Mathf.Sin(t * swayFrequency * Mathf.PI * 2f) * swayAmplitude * fadeIn;
            float swayZ = Mathf.Cos(t * swayFrequency * 0.8f * Mathf.PI * 2f) * swayAmplitude * 0.5f * fadeIn;

            hookPoint.position = bobbingBasePosition + new Vector3(swayX, bobY, swayZ);

            // HookPoint rotation stays stable so the fishing line and child offsets do not drift.
        }

        private IEnumerator EnableWaveEffectDelayed()
        {
            yield return new WaitForSeconds(bobberWaveDelay);
            if (bobberWaveEffect != null && isFishingActive)
            {
                bobberWaveEffect.SetEffectActive(true);
            }
            waveEffectRoutine = null;
        }
    }
}
