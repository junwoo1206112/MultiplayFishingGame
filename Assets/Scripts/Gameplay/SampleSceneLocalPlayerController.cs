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
        [SerializeField] private string walkParameter = "WalkSpeed";
        [SerializeField] private string fishingParameter = "fishing";
        [SerializeField] private string hasFishParameter = "HasFish";
        [SerializeField] private float walkAnimDampTime = 0.15f;

        [Header("Fishing Visual")]
        [SerializeField] private FishingLineVisual fishingLineVisual;

        [Header("Fish Catch Mechanics")]
        [SerializeField] private GameObject biteSignalPrefab;
        [SerializeField] private GameObject fishPrefab;
        [SerializeField] private Transform handsPoint;    
        [SerializeField] private GameObject rodModel;     
        [SerializeField] private float minBiteWaitTime = 2f;
        [SerializeField] private float maxBiteWaitTime = 5f;
        [SerializeField] private float biteWindowDuration = 1.5f;
        [SerializeField] private Vector3 biteSignalOffset = new Vector3(0f, 2.5f, 0f);
        [SerializeField] private float reelDurationWithFish = 1.2f;
        [SerializeField] private float fishWiggleAmount = 12f;    
        [SerializeField] private float fishWiggleSpeed = 15f;     

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
        private Vector2 movementInput;
        private int walkParameterHash;
        private int fishingParameterHash;
        private int hasFishParameterHash;
        private bool hasWalkParameter;
        private bool hasFishingParameter;
        private bool hasHasFishParameter;
        private bool isFishingActive;
        private bool hasLastWaterHitPoint;
        private Vector3 lastWaterHitPoint;
        private FishingWaterSurfaceResolver waterSurfaceResolver;
        private FishingRopeController fishingRopeController;
        private FishingSplashController fishingSplashController;

        private GameObject activeBiteSignal;
        private bool isBiteActive;
        private Coroutine biteWaitRoutine;
        private Coroutine biteWindowRoutine;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponent<Animator>();
            walkParameterHash = Animator.StringToHash(walkParameter);
            fishingParameterHash = Animator.StringToHash(fishingParameter);
            hasFishParameterHash = Animator.StringToHash(hasFishParameter);
            CacheAnimatorParameter();

            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (fishingRopeObject == null)
            {
                Transform fishingRopeTransform = transform.Find("FishingRope");
                if (fishingRopeTransform != null) fishingRopeObject = fishingRopeTransform.gameObject;
            }

            if (fishingRopeComponent == null && fishingRopeObject != null)
                fishingRopeComponent = fishingRopeObject.GetComponent("Rope");

            waterSurfaceResolver = new FishingWaterSurfaceResolver(playerCamera, tipPoint, waterSurfaceTransform, waterLayerMask, waterRayStartHeight, downwardCastBias, maxCastDistance);
            fishingRopeController = new FishingRopeController(tipPoint, hookPoint, fishingRopeObject, fishingRopeComponent);
            fishingSplashController = new FishingSplashController(fishingSplashParticle);

            if (fishingLineVisual != null && !fishingLineVisual.IsConfiguredForRuntime)
                fishingLineVisual.enabled = false;
        }

        private void Start()
        {
            if (fishingRopeController != null && fishingRopeController.IsConfigured)
                fishingRopeController.SetHookPosition(GetIdleHookPosition());

            fishingSplashController?.Reset();
            fishingRopeController?.SetVisible(false);
            fishingRopeController?.SetRopeLength(GetDesiredRopeLength(GetIdleHookPosition(), idleRopeLength, idleRopeSlack));

            if (fishingLineVisual != null && !UsesHookCasting())
                fishingLineVisual.SetFishingActive(isFishingActive);
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
            if (Mouse.current == null || !hasFishingParameter || animator == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            if (isBiteActive)
            {
                CatchFish();
                return;
            }

            isFishingActive = !isFishingActive;
            animator.SetBool(fishingParameterHash, isFishingActive);

            if (isFishingActive) StopBiteLogic();

            if (UsesHookCasting())
            {
                if (hookMoveRoutine != null) StopCoroutine(hookMoveRoutine);

                Vector3 targetPosition = isFishingActive ? GetCastTargetPosition() : GetIdleHookPosition();
                if (!isFishingActive)
                {
                    float ropeLength = GetDesiredRopeLength(targetPosition, idleRopeLength, idleRopeSlack);
                    fishingRopeController?.SetRopeLength(ropeLength);
                    StopBiteLogic();
                }

                float ropeSlack = isFishingActive ? castRopeSlack : idleRopeSlack;
                float minimumRopeLength = isFishingActive ? castRopeLength : idleRopeLength;
                float waterSurfaceY = 0f;
                bool stopAtWaterSurface = isFishingActive && waterSurfaceResolver != null && waterSurfaceResolver.TryGetSurfaceHeight(out waterSurfaceY);
                
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
                    ropeSlack, minimumRopeLength, true, !isFishingActive, true, stopAtWaterSurface, waterSurfaceY, isFishingActive && useLegacySplashEffect));
            }
        }

        private void StopBiteLogic()
        {
            if (biteWaitRoutine != null) StopCoroutine(biteWaitRoutine);
            if (biteWindowRoutine != null) StopCoroutine(biteWindowRoutine);
            isBiteActive = false;
            if (activeBiteSignal != null) Destroy(activeBiteSignal);
            biteWaitRoutine = null;
            biteWindowRoutine = null;
        }

        private void CatchFish()
        {
            Debug.Log("<color=yellow>Fish Caught!</color>");
            StopBiteLogic();

            if (hasHasFishParameter) animator.SetBool(hasFishParameterHash, true);

            if (fishPrefab != null && hookPoint != null)
            {
                GameObject caughtFish = Instantiate(fishPrefab, hookPoint.position, Quaternion.identity);
                caughtFish.transform.SetParent(hookPoint);
                
                StartCoroutine(AnimateCaughtFish(caughtFish, reelDurationWithFish));
                
                Destroy(caughtFish, 4f);
            }

            isFishingActive = false;
            animator.SetBool(fishingParameterHash, false);
            
            if (hookMoveRoutine != null) StopCoroutine(hookMoveRoutine);
            
            Vector3 targetPosition = GetIdleHookPosition();
            float ropeLength = GetDesiredRopeLength(targetPosition, idleRopeLength, idleRopeSlack);
            fishingRopeController?.SetRopeLength(ropeLength);

            hookMoveRoutine = StartCoroutine(RunHookMove(
                targetPosition, 0f, reelDurationWithFish, reelArcHeight, idleRopeSlack, idleRopeLength, true, true, true, false, 0f, false));
        }

        private IEnumerator AnimateCaughtFish(GameObject fish, float reelTime)
        {
            float elapsed = 0f;
            Quaternion hangRotation = Quaternion.Euler(90f, 0f, 0f); 

            while (elapsed < reelTime && fish != null)
            {
                elapsed += Time.deltaTime;
                float wiggleElapsed = elapsed * fishWiggleSpeed;
                float wiggleX = Mathf.Sin(wiggleElapsed) * fishWiggleAmount;
                float wiggleZ = Mathf.Cos(wiggleElapsed * 0.8f) * (fishWiggleAmount * 0.5f);
                
                if (fish.transform.parent == hookPoint)
                {
                    fish.transform.localRotation = hangRotation * Quaternion.Euler(wiggleX, 0f, wiggleZ);
                }

                if (elapsed >= reelTime * 0.9f && fish.transform.parent == hookPoint)
                {
                    if (rodModel != null) rodModel.SetActive(false);
                    if (handsPoint != null)
                    {
                        fish.transform.SetParent(handsPoint);
                        StartCoroutine(DropToHands(fish));
                    }
                }
                yield return null;
            }

            while (fish != null) yield return null;

            if (hasHasFishParameter) animator.SetBool(hasFishParameterHash, false);

            if (rodModel != null) rodModel.SetActive(true);
        }

        private IEnumerator DropToHands(GameObject fish)
        {
            float dropTime = 0.25f;
            float elapsed = 0f;
            Vector3 startLocalPos = new Vector3(0, 0.3f, 0); 
            Quaternion startLocalRot = fish.transform.localRotation;

            while (elapsed < dropTime && fish != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dropTime;
                fish.transform.localPosition = Vector3.Lerp(startLocalPos, Vector3.zero, t);
                fish.transform.localRotation = Quaternion.Slerp(startLocalRot, Quaternion.identity, t);
                yield return null;
            }
            if (fish != null)
            {
                fish.transform.localPosition = Vector3.zero;
                fish.transform.localRotation = Quaternion.identity;
            }
        }

        private IEnumerator WaitForBite()
        {
            float waitTime = Random.Range(minBiteWaitTime, maxBiteWaitTime);
            yield return new WaitForSeconds(waitTime);
            if (isFishingActive) biteWindowRoutine = StartCoroutine(BiteWindow());
        }

        private IEnumerator BiteWindow()
        {
            isBiteActive = true;
            Debug.Log("<color=red>BITE!</color>");
            if (biteSignalPrefab != null)
            {
                activeBiteSignal = Instantiate(biteSignalPrefab, transform.position + biteSignalOffset, Quaternion.identity);
                activeBiteSignal.transform.SetParent(transform);
            }
            yield return new WaitForSeconds(biteWindowDuration);
            isBiteActive = false;
            if (activeBiteSignal != null) Destroy(activeBiteSignal);
            if (isFishingActive) biteWaitRoutine = StartCoroutine(WaitForBite());
        }

        private void HandleRotation()
        {
            if (Mouse.current == null || (isFishingActive && !allowRotationWhileFishing)) return;
            float mouseDeltaX = Mouse.current.delta.ReadValue().x;
            if (!Mathf.Approximately(mouseDeltaX, 0f))
                transform.Rotate(Vector3.up, mouseDeltaX * rotationSpeed * Time.deltaTime, Space.World);
        }

        private void HandleMovement()
        {
            if (isFishingActive || Keyboard.current == null) { movementInput = Vector2.zero; return; }
            movementInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) movementInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) movementInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed) movementInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed) movementInput.x += 1f;
            Vector3 move = (transform.right * movementInput.x) + (transform.forward * movementInput.y);
            if (move.sqrMagnitude > 1f) move.Normalize();
            if (characterController.isGrounded && velocity.y < 0f) velocity.y = groundedVelocity;
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(((move * moveSpeed) + Vector3.up * velocity.y) * Time.deltaTime);
        }

        private void UpdateWalkAnimation()
        {
            if (!hasWalkParameter && animator != null) CacheAnimatorParameter();
            float targetSpeed = isFishingActive ? 0f : Mathf.Clamp01(movementInput.magnitude);
            if (animator != null && hasWalkParameter) animator.SetFloat(walkParameterHash, targetSpeed, walkAnimDampTime, Time.deltaTime);
        }

        private void CacheAnimatorParameter()
        {
            hasWalkParameter = hasFishingParameter = hasHasFishParameter = false;
            if (animator == null) return;
            foreach (var p in animator.parameters)
            {
                if (p.nameHash == walkParameterHash) hasWalkParameter = true;
                if (p.nameHash == fishingParameterHash) hasFishingParameter = true;
                if (p.nameHash == hasFishParameterHash) hasHasFishParameter = true;
            }
        }

        private bool UsesHookCasting() => fishingRopeController != null && fishingRopeController.IsConfigured;

        private Vector3 GetCastTargetPosition()
        {
            if (waterSurfaceResolver == null) { hasLastWaterHitPoint = false; return GetFallbackCastTargetPosition(); }
            Vector3 targetPosition = waterSurfaceResolver.ResolveCastTarget(transform, castTargetOffset, fallbackCastDistance, out hasLastWaterHitPoint, out lastWaterHitPoint);
            waterSurfaceTransform = waterSurfaceResolver.WaterSurfaceTransform;
            return targetPosition;
        }

        private Vector3 GetIdleHookPosition() => fishingRopeController != null ? fishingRopeController.GetIdleHookPosition(transform, idleHookOffset) : transform.position;
        private Vector3 GetFallbackCastTargetPosition() => GetIdleHookPosition() + transform.forward * fallbackCastDistance;
        private float GetDesiredRopeLength(Vector3 targetPos, float minLen, float slack) => fishingRopeController != null ? fishingRopeController.GetDesiredRopeLength(targetPos, minLen, slack) : minLen;

        private IEnumerator RunHookMove(Vector3 targetPos, float startDelay, float duration, float arcHeight, float ropeSlack, float minRopeLen, bool showRope, bool hideRope, bool useArc, bool stopAtWater, float waterY, bool playSplash)
        {
            System.Action onWaterHit = null;
            if (playSplash && fishingSplashController != null)
                onWaterHit = () => { fishingSplashController.UpdatePendingPosition(hasLastWaterHitPoint, lastWaterHitPoint, targetPos, splashWorldOffset, clampSplashToWaterSurface, minimumSplashHeightOffset); fishingSplashController.Play(); };

            yield return fishingRopeController.MoveHook(targetPos, startDelay, duration, arcHeight, ropeSlack, minRopeLen, showRope, hideRope, useArc, stopAtWater, waterY, onWaterHit, fishingLineVisual);

            if (isFishingActive && stopAtWater) biteWaitRoutine = StartCoroutine(WaitForBite());
            if (hideRope && fishingRopeController != null)
            {
                Vector3 idlePos = GetIdleHookPosition();
                fishingRopeController.RestoreHookToRod(idlePos);
                fishingRopeController.SetRopeLength(GetDesiredRopeLength(idlePos, idleRopeLength, idleRopeSlack));
            }
            hookMoveRoutine = null;
        }
    }
}
