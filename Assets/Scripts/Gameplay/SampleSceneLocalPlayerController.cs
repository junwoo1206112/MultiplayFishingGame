using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MultiplayFishing.Gameplay
{
    /// <summary>
    /// Local-only movement helper for the Player placed directly in SampleScene.
    /// This is intentionally separate from the Mirror network player flow.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SampleSceneLocalPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedVelocity = -2f;
        [SerializeField] private bool allowRotationWhileFishing;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string walkParameter = "Walk";
        [SerializeField] private string fishingParameter = "fishing";
        [SerializeField] private float walkThreshold = 0.01f;

        [Header("Fishing Visual")]
        [SerializeField] private FishingLineVisual fishingLineVisual;

        [Header("Fishing Cast")]
        [SerializeField] private GameObject fishingRopeObject;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform tipPoint;
        [SerializeField] private Transform hookPoint;
        [SerializeField] private Transform waterSurfaceTransform;
        [SerializeField] private ParticleSystem fishingSplashParticle;
        [SerializeField] private LayerMask waterLayerMask;
        [SerializeField] private float waterRayStartHeight = 1.5f;
        [SerializeField] private float downwardCastBias = 0.2f;
        [SerializeField] private float fallbackCastDistance = 6f;
        [SerializeField] private float maxCastDistance = 12f;
        [SerializeField] private float castStartDelay = 0.18f;
        [SerializeField] private float castDuration = 0.45f;
        [SerializeField] private float reelDuration = 0.8f;
        [SerializeField] private float castArcHeight = 0.35f;
        [SerializeField] private float reelArcHeight = 0.2f;
        [SerializeField] private float idleRopeLength = 1.8f;
        [SerializeField] private float castRopeLength = 1.8f;
        [SerializeField] private float idleRopeSlack = 0.1f;
        [SerializeField] private float castRopeSlack = 0.05f;
        [SerializeField] private float hookWaterSubmergeDepth = 0.08f;
        [SerializeField] private Vector3 castTargetOffset = Vector3.zero;
        [SerializeField] private Vector3 splashWorldOffset = new Vector3(0f, 0.01f, 0f);
        [SerializeField] private float splashDelay = 0.05f;
        [SerializeField] private bool clampSplashToWaterSurface = true;
        [SerializeField] private float minimumSplashHeightOffset = 0.02f;
        [SerializeField] private bool useLegacySplashEffect = true;
        [SerializeField] private Vector3 idleHookOffset = new Vector3(0f, 0f, 0.1f);

        private CharacterController characterController;
        private Component fishingRopeComponent;
        private Coroutine hookMoveRoutine;
        private Vector3 velocity;
        private Vector3 lastPosition;
        private Vector2 movementInput;
        private int walkParameterHash;
        private int fishingParameterHash;
        private bool hasWalkParameter;
        private bool hasFishingParameter;
        private bool isFishingActive;
        private bool hasLastWaterHitPoint;
        private Vector3 lastWaterHitPoint;
        private FishingWaterSurfaceResolver waterSurfaceResolver;
        private FishingRopeController fishingRopeController;
        private FishingSplashController fishingSplashController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            walkParameterHash = Animator.StringToHash(walkParameter);
            fishingParameterHash = Animator.StringToHash(fishingParameter);
            CacheAnimatorParameter();

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }

            if (fishingRopeObject == null)
            {
                Transform fishingRopeTransform = transform.Find("FishingRope");
                if (fishingRopeTransform != null)
                {
                    fishingRopeObject = fishingRopeTransform.gameObject;
                }
            }

            if (fishingRopeComponent == null && fishingRopeObject != null)
            {
                fishingRopeComponent = fishingRopeObject.GetComponent("Rope");
            }

            waterSurfaceResolver = new FishingWaterSurfaceResolver(
                playerCamera,
                tipPoint,
                waterSurfaceTransform,
                waterLayerMask,
                waterRayStartHeight,
                downwardCastBias,
                maxCastDistance);
            fishingRopeController = new FishingRopeController(tipPoint, hookPoint, fishingRopeObject, fishingRopeComponent);
            fishingSplashController = new FishingSplashController(fishingSplashParticle);

            if (fishingLineVisual != null && !fishingLineVisual.IsConfiguredForRuntime)
            {
                fishingLineVisual.enabled = false;
            }
        }

        private void Start()
        {
            lastPosition = transform.position;
            if (fishingRopeController != null && fishingRopeController.IsConfigured)
            {
                fishingRopeController.SetHookPosition(GetIdleHookPosition());
            }

            fishingSplashController?.Reset();

            fishingRopeController?.SetVisible(false);
            fishingRopeController?.SetRopeLength(GetDesiredRopeLength(GetIdleHookPosition(), idleRopeLength, idleRopeSlack));

            if (fishingLineVisual != null && !UsesHookCasting())
            {
                fishingLineVisual.SetFishingActive(isFishingActive);
            }
        }

        private void Update()
        {
            HandleFishingInput();
            HandleRotation();
            HandleMovement();
            UpdateWalkAnimation();
        }

        private void HandleFishingInput()
        {
            if (Mouse.current == null || !hasFishingParameter || animator == null)
            {
                return;
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            isFishingActive = !isFishingActive;
            animator.SetBool(fishingParameterHash, isFishingActive);

            if (UsesHookCasting())
            {
                if (hookMoveRoutine != null)
                {
                    StopCoroutine(hookMoveRoutine);
                }

                Vector3 targetPosition = isFishingActive ? GetCastTargetPosition() : GetIdleHookPosition();
                if (!isFishingActive)
                {
                    float ropeLength = GetDesiredRopeLength(targetPosition, idleRopeLength, idleRopeSlack);
                    fishingRopeController?.SetRopeLength(ropeLength);
                }

                float ropeSlack = isFishingActive ? castRopeSlack : idleRopeSlack;
                float minimumRopeLength = isFishingActive ? castRopeLength : idleRopeLength;
                float waterSurfaceY = 0f;
                bool stopAtWaterSurface = isFishingActive && waterSurfaceResolver != null
                    && waterSurfaceResolver.TryGetSurfaceHeight(out waterSurfaceY);
                if (stopAtWaterSurface)
                {
                    float hookTargetWaterY = waterSurfaceY - hookWaterSubmergeDepth;
                    targetPosition.y = hookTargetWaterY;
                    waterSurfaceY = hookTargetWaterY;
                }

                hookMoveRoutine = StartCoroutine(RunHookMove(
                    targetPosition,
                    isFishingActive ? castStartDelay : 0f,
                    isFishingActive ? castDuration : reelDuration,
                    isFishingActive ? castArcHeight : reelArcHeight,
                    ropeSlack,
                    minimumRopeLength,
                    true,
                    !isFishingActive,
                    true,
                    stopAtWaterSurface,
                    waterSurfaceY,
                    isFishingActive && useLegacySplashEffect));
            }
            else if (fishingLineVisual != null)
            {
                fishingRopeController?.SetRopeLength(isFishingActive ? castRopeLength : idleRopeLength);
                fishingLineVisual.SetFishingActive(isFishingActive);
            }
        }

        private void HandleRotation()
        {
            if (Mouse.current == null)
            {
                return;
            }

            if (isFishingActive && !allowRotationWhileFishing)
            {
                return;
            }

            float mouseDeltaX = Mouse.current.delta.ReadValue().x;
            if (Mathf.Approximately(mouseDeltaX, 0f))
            {
                return;
            }

            transform.Rotate(Vector3.up, mouseDeltaX * rotationSpeed * Time.deltaTime, Space.World);
        }

        private void HandleMovement()
        {
            // 디버깅: Fishing 상태
            if (isFishingActive)
            {
                movementInput = Vector2.zero;
                Debug.Log("[WalkDebug] FishingActive - Movement stopped");
                return;
            }

            // 디버깅: Keyboard null 체크
            if (Keyboard.current == null)
            {
                movementInput = Vector2.zero;
                Debug.LogWarning("[WalkDebug] Keyboard.current is NULL!");
                return;
            }

            Vector2 previousInput = movementInput;
            movementInput = Vector2.zero;

            if (Keyboard.current.wKey.isPressed)
            {
                movementInput.y += 1f;
            }

            if (Keyboard.current.sKey.isPressed)
            {
                movementInput.y -= 1f;
            }

            if (Keyboard.current.aKey.isPressed)
            {
                movementInput.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed)
            {
                movementInput.x += 1f;
            }
            
            // 디버깅: 입력값 변화 로그
            if (movementInput != previousInput)
            {
                Debug.Log($"[WalkDebug] Input changed: {previousInput} -> {movementInput}");
            }

            // 실제 이동 처리
            Vector3 move = (transform.right * movementInput.x) + (transform.forward * movementInput.y);
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            if (characterController.isGrounded && velocity.y < 0f)
            {
                velocity.y = groundedVelocity;
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }

            Vector3 motion = (move * moveSpeed) + Vector3.up * velocity.y;
            characterController.Move(motion * Time.deltaTime);
        }

        private void UpdateWalkAnimation()
        {
            // Animator 파라미터가 아직 캐싱되지 않았다면 다시 시도
            if (!hasWalkParameter && animator != null)
            {
                CacheAnimatorParameter();
            }
            
            Vector3 delta = transform.position - lastPosition;
            delta.y = 0f;

            bool hasInput = movementInput.sqrMagnitude > 0.01f;
            bool isWalking = !isFishingActive && (hasInput || delta.magnitude > walkThreshold);
            
            // 디버깅: Walk 상태 로그
            if (hasWalkParameter && animator != null)
            {
                bool currentWalk = animator.GetBool(walkParameterHash);
                if (currentWalk != isWalking)
                {
                    Debug.Log($"[WalkDebug] Walk state changed: {currentWalk} -> {isWalking} (hasInput={hasInput}, isFishing={isFishingActive}, delta={delta.magnitude:F3})");
                }
            }
            
            // Walk 파라미터 설정 (파라미터가 없어도 오류 발생 안함)
            if (animator != null && hasWalkParameter)
            {
                animator.SetBool(walkParameterHash, isWalking);
            }

            lastPosition = transform.position;
        }

        private void CacheAnimatorParameter()
        {
            hasWalkParameter = false;
            hasFishingParameter = false;

            if (animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Bool &&
                    parameter.nameHash == walkParameterHash)
                {
                    hasWalkParameter = true;
                }

                if (parameter.type == AnimatorControllerParameterType.Bool &&
                    parameter.nameHash == fishingParameterHash)
                {
                    hasFishingParameter = true;
                }
            }
        }

        private bool UsesHookCasting()
        {
            return fishingRopeController != null && fishingRopeController.IsConfigured;
        }

        private Vector3 GetCastTargetPosition()
        {
            if (waterSurfaceResolver == null)
            {
                hasLastWaterHitPoint = false;
                return GetFallbackCastTargetPosition();
            }

            Vector3 targetPosition = waterSurfaceResolver.ResolveCastTarget(
                transform,
                castTargetOffset,
                fallbackCastDistance,
                out hasLastWaterHitPoint,
                out lastWaterHitPoint);
            waterSurfaceTransform = waterSurfaceResolver.WaterSurfaceTransform;
            return targetPosition;
        }

        private Vector3 GetIdleHookPosition()
        {
            return fishingRopeController != null
                ? fishingRopeController.GetIdleHookPosition(transform, idleHookOffset)
                : transform.position;
        }

        private Vector3 GetFallbackCastTargetPosition()
        {
            return GetIdleHookPosition()
                + transform.forward * fallbackCastDistance
                + transform.right * castTargetOffset.x
                + transform.up * castTargetOffset.y
                + transform.forward * castTargetOffset.z;
        }

        private float GetDesiredRopeLength(Vector3 targetPosition, float minimumLength, float slack)
        {
            return fishingRopeController != null
                ? fishingRopeController.GetDesiredRopeLength(targetPosition, minimumLength, slack)
                : minimumLength;
        }

        private IEnumerator RunHookMove(
            Vector3 targetPosition,
            float startDelay,
            float duration,
            float arcHeight,
            float ropeSlack,
            float minimumRopeLength,
            bool showRopeOnStart,
            bool hideRopeOnComplete,
            bool useArcPath,
            bool stopAtWaterSurface,
            float waterSurfaceY,
            bool playSplashOnComplete)
        {
            // 물에 닿을 때 스플래시를 재생하는 콜백
            System.Action onWaterHit = null;
            if (playSplashOnComplete && fishingSplashController != null)
            {
                onWaterHit = () =>
                {
                    fishingSplashController.UpdatePendingPosition(
                        hasLastWaterHitPoint,
                        lastWaterHitPoint,
                        targetPosition,
                        splashWorldOffset,
                        clampSplashToWaterSurface,
                        minimumSplashHeightOffset);
                    
                    fishingSplashController.Play();
                };
            }

            yield return fishingRopeController.MoveHook(
                targetPosition,
                startDelay,
                duration,
                arcHeight,
                ropeSlack,
                minimumRopeLength,
                showRopeOnStart,
                hideRopeOnComplete,
                useArcPath,
                stopAtWaterSurface,
                waterSurfaceY,
                onWaterHit,
                fishingLineVisual);

            if (hideRopeOnComplete && fishingRopeController != null)
            {
                Vector3 idleHookPosition = GetIdleHookPosition();
                fishingRopeController.RestoreHookToRod(idleHookPosition);
                fishingRopeController.SetRopeLength(
                    GetDesiredRopeLength(idleHookPosition, idleRopeLength, idleRopeSlack));
            }

            hookMoveRoutine = null;
        }
    }
}
