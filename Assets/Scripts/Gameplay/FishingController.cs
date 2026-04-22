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

        [Header("Fishing Visuals")]
        [SerializeField] private FishingLineVisual fishingLineVisual;
        [SerializeField] private GameObject fishingRopeObject;
        [SerializeField] private ParticleSystem fishingSplashParticle;

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
        [SerializeField] private Vector3 castTargetOffset = Vector3.zero;
        [SerializeField] private Vector3 idleHookOffset = new Vector3(0f, 0f, 0.1f);
        [SerializeField] private Vector3 splashWorldOffset = new Vector3(0f, 0.01f, 0f);
        [SerializeField] private float splashDelay = 0.05f;
        [SerializeField] private bool clampSplashToWaterSurface = true;
        [SerializeField] private float minimumSplashHeightOffset = 0.02f;
        [SerializeField] private bool useSplashEffect = true;

        [Header("Line Style")]
        [SerializeField] private float lineWidth = 0.03f;

        private FishingRopeController fishingRopeController;
        private FishingSplashController fishingSplashController;

        private Coroutine hookMoveRoutine;
        private Coroutine splashRoutine;
        private int fishingParameterHash;
        private bool isFishingActive;
        private bool hasFishingParameter;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            fishingParameterHash = Animator.StringToHash(fishingParameter);

            EnsureInitialized();
            catchPresenter?.Initialize(animator, hasFishParameter);
            if (biteSystem != null)
            {
                biteSystem.BiteStarted += HandleBiteStarted;
                biteSystem.BiteEnded += HandleBiteEnded;
            }
            CacheAnimatorParameters();
        }

        private void OnDestroy()
        {
            if (biteSystem != null)
            {
                biteSystem.BiteStarted -= HandleBiteStarted;
                biteSystem.BiteEnded -= HandleBiteEnded;
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
        }

        private void HandleInput()
        {
            // EnsureInitialized()를 Update마다 호출하지 않음.
            // 캐스팅 중 hookPoint가 분리(parent=null)된 상태에서 재생성되면
            // originalHookParent=null로 캡처되어 RestoreHookToRod가 동작하지 않게 된다.
            if (Mouse.current == null || !hasFishingParameter || !Mouse.current.leftButton.wasPressedThisFrame) return;

            // Fishing Start / Fishing Finish 애니메이션 재생 중에는 모든 입력 차단.
            // hookMoveRoutine은 캐스팅(Start)과 릴인(Finish) 애니메이션 구간 동안 실행되므로
            // 별도 플래그 없이 이것을 입력 잠금 신호로 활용한다.
            if (hookMoveRoutine != null) return;

            // 입질 중일 때 클릭
            if (biteSystem.IsBiteActive)
            {
                PerformCatch();
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

            Vector3 targetPosition = isCasting ? GetCastTargetPosition() : GetIdleHookPosition();
            float duration = isCasting ? castDuration : reelDuration;
            float arc = isCasting ? castArcHeight : reelArcHeight;

            hookMoveRoutine = StartCoroutine(RunHookMoveRoutine(targetPosition, duration, arc, isCasting));
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

            if (hookMoveRoutine != null) StopCoroutine(hookMoveRoutine);
            hookMoveRoutine = StartCoroutine(RunHookMoveRoutine(GetIdleHookPosition(), reelDurationWithFish, reelArcHeight, false));
        }

        private IEnumerator RunHookMoveRoutine(Vector3 targetPos, float duration, float arc, bool isCasting)
        {
            if (isCasting)
            {
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

            System.Func<float> startDelayProvider = () => isCasting ? castStartDelay : 0f;
            System.Func<float> durationProvider = () => isCasting ? castDuration : reelDuration;
            System.Func<float> arcHeightProvider = () => isCasting ? castArcHeight : reelArcHeight;
            System.Func<float> ropeSlackProvider = () => isCasting ? castRopeSlack : idleRopeSlack;
            System.Func<float> minimumRopeLengthProvider = () => isCasting ? castRopeLength : idleRopeLength;
            System.Func<float> waterSurfaceYProvider = () =>
            {
                FishingWaterSurfaceResolver resolver = CreateWaterSurfaceResolver();
                return resolver.TryGetSurfaceHeight(out float waterSurfaceY) ? waterSurfaceY : targetPos.y;
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
                onWaterHit,
                fishingLineVisual,
                !isCasting ? idleAnchorPoint : null);

            if (isFishingActive && isCasting)
            {
                biteSystem.StartWaitingForBite();
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
                targetPosition.y = waterSurfaceY - hookWaterSubmergeDepth;
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
            foreach (var p in animator.parameters)
            {
                if (p.nameHash == fishingParameterHash) { hasFishingParameter = true; break; }
            }
        }

        private void HandleBiteStarted()
        {
            catchPresenter?.ShowFishPreview(hookPoint);
        }

        private void HandleBiteEnded()
        {
            catchPresenter?.ClearFishPreview();
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
    }
}
