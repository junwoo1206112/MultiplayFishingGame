using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Mirror;

namespace MultiplayFishing.Gameplay
{
    public enum FishingState
    {
        Idle,
        Charging,
        Casting,
        Waiting,
        Nibble,
        Catching,
        Success,
        Failure
    }

    public class FishingController : MonoBehaviour
    {
        [Header("State")]
        public FishingState CurrentState = FishingState.Idle;

        [Header("Casting Settings")]
        [SerializeField] private float minCastDistance = 2f;
        [SerializeField] private float maxCastDistance = 15f;
        [SerializeField] private float chargeSpeed = 10f;
        private float currentChargeDistance;

        [Header("Timing Settings")]
        [SerializeField] private float nibbleReactionWindow = 3f;
        [SerializeField] private float catchingDuration = 10f;

        [Header("Input Safety")]
        [SerializeField] private bool blockCastWhileMoving;
        [SerializeField] private float castInputLockDuration = 0.5f;
        [SerializeField] private float castReleaseFallbackDelay = 2.1f;

        [Header("References")]
        private FishingPlayer fishingPlayer;
        private Animator animator;
        private FishingLineVisual fishingLineVisual;
        private FishingRopeController ropeController;
        private FishingSplashController splashController;
        private FishingWaterSurfaceResolver waterResolver;
        public event Action<FishingState> OnStateChanged;
        public event Action<float> OnChargeProgressChanged; // 0 ~ 1
        public event Action<float, float> OnCatchProgressChanged; // current, target

        private Coroutine stateRoutine;
        private Vector3 targetPosition;
        private Vector3 castHitPoint;
        private bool castHasHit;
        private float stateTimer;
        private int spamCount;
        private int targetSpamCount;
        private int fishingCastTriggerHash;
        private int fishingBoolHash;
        private bool hasFishingCastTrigger;
        private bool hasFishingBool;
        private float inputLockedUntil;
        private bool wasLockedThisFrame;
        private Coroutine castReleaseFallbackRoutine;
        private bool waitingForCastRelease;
        private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

        public void Initialize(
            FishingPlayer player,
            Animator anim,
            FishingLineVisual lineVisual,
            FishingRopeController rope,
            FishingSplashController splash,
            FishingWaterSurfaceResolver resolver)
        {
            fishingPlayer = player;
            animator = anim;
            if (animator != null)
            {
                animator.applyRootMotion = false;
            }

            fishingLineVisual = lineVisual;
            ropeController = rope;
            splashController = splash;
            waterResolver = resolver;
            CacheAnimatorParameters();
        }

        private void Update()
        {
            if (!fishingPlayer.isLocalPlayer) return;

            HandleInput();
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;
            if (IsPointerOverUI()) return;

            wasLockedThisFrame = Time.time < inputLockedUntil;
            bool leftPressed = Mouse.current.leftButton.wasPressedThisFrame;
            bool leftHeld = Mouse.current.leftButton.isPressed;
            bool leftReleased = Mouse.current.leftButton.wasReleasedThisFrame;
            bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

            if (wasLockedThisFrame) return;

            switch (CurrentState)
            {
                case FishingState.Idle:
                    if (leftPressed) StartCharging();
                    break;

                case FishingState.Charging:
                    if (leftHeld) UpdateCharging();
                    if (leftReleased || !leftHeld) Cast();
                    break;

                case FishingState.Nibble:
                    if (leftPressed) TryHooking();
                    break;

                case FishingState.Catching:
                    if (leftPressed || spacePressed) RecordSpam();
                    break;
            }
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null || Mouse.current == null) return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, uiRaycastResults);

            foreach (RaycastResult result in uiRaycastResults)
            {
                if (result.gameObject == null) continue;
                if (result.gameObject.transform.IsChildOf(transform)) continue;

                return true;
            }

            return false;
        }

        private bool IsMovementInputPressed()
        {
            if (Keyboard.current == null) return false;

            return Keyboard.current.wKey.isPressed
                || Keyboard.current.aKey.isPressed
                || Keyboard.current.sKey.isPressed
                || Keyboard.current.dKey.isPressed
                || Keyboard.current.upArrowKey.isPressed
                || Keyboard.current.leftArrowKey.isPressed
                || Keyboard.current.downArrowKey.isPressed
                || Keyboard.current.rightArrowKey.isPressed;
        }

        private void ChangeState(FishingState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);

            if (animator != null)
            {
                SetFishingBool(false);
            }
        }

        private void StartCharging()
        {
            if (blockCastWhileMoving && IsMovementInputPressed()) return;

            if (animator != null && hasFishingCastTrigger)
            {
                animator.ResetTrigger(fishingCastTriggerHash);
            }

            currentChargeDistance = minCastDistance;
            ChangeState(FishingState.Charging);
        }

        private void UpdateCharging()
        {
            currentChargeDistance += chargeSpeed * Time.deltaTime;
            currentChargeDistance = Mathf.Min(currentChargeDistance, maxCastDistance);
            OnChargeProgressChanged?.Invoke((currentChargeDistance - minCastDistance) / (maxCastDistance - minCastDistance));
        }

        private void Cast()
        {
            if (waterResolver == null)
            {
                Debug.LogWarning("[FishingController] Water resolver is missing. Cast cancelled.");
                ChangeState(FishingState.Idle);
                return;
            }

            Vector3 castOffset = new Vector3(0, 0, currentChargeDistance);
            Vector3 hitPoint;

            targetPosition = waterResolver.ResolveCastTarget(
                transform,
                castOffset,
                currentChargeDistance,
                out castHasHit,
                out hitPoint);

            if (!castHasHit && waterResolver != null && waterResolver.TryGetSurfaceHeight(out float waterSurfaceY))
            {
                targetPosition.y = waterSurfaceY;
                hitPoint = targetPosition;
                castHasHit = true;
            }

            if (!castHasHit)
            {
                Debug.Log("물이 없는 곳에 낚시질 할 수 없습니다.");
                ChangeState(FishingState.Idle);
                return;
            }

            castHitPoint = hitPoint;
            inputLockedUntil = Time.time + castInputLockDuration;
            ChangeState(FishingState.Casting);
            PlayCastAnimation();
            waitingForCastRelease = true;
            StartCastReleaseFallback();
        }

        public void OnCastRelease()
        {
            if (!waitingForCastRelease) return;

            if (CurrentState != FishingState.Casting)
            {
                Debug.LogWarning($"[FishingController] OnCastRelease ignored: current state is {CurrentState}, expected Casting.");
                return;
            }

            waitingForCastRelease = false;
            StopCastReleaseFallback();
            fishingPlayer.CmdStartFishing(targetPosition);

            if (stateRoutine != null) StopCoroutine(stateRoutine);
            stateRoutine = StartCoroutine(CastingRoutine(targetPosition, castHitPoint, castHasHit));
        }

        private IEnumerator CastingRoutine(Vector3 target, Vector3 hitPoint, bool hasHit)
        {
            if (ropeController == null || !ropeController.IsConfigured)
            {
                Debug.LogWarning("[FishingController] Fishing rope is not configured. Starting fishing wait state without hook animation.");
                ChangeState(FishingState.Waiting);
                yield break;
            }

            yield return ropeController.MoveHook(
                target,
                0.15f, // startDelay
                0.5f,  // duration
                0.5f,  // arcHeight
                0.05f, // slack
                1.5f,  // minLength
                true,  // showRope
                false, // hideRopeOnComplete
                true,  // useArc
                true,  // stopAtWater
                target.y,
                () => {
                    splashController?.UpdatePendingPosition(hasHit, hitPoint, target, Vector3.zero, true, 0.02f);
                    splashController?.Play();
                },
                fishingLineVisual);

            ChangeState(FishingState.Waiting);
        }

        private void TryHooking()
        {
            if (stateTimer <= nibbleReactionWindow)
            {
                fishingPlayer.CmdTryHook();
            }
            else
            {
                Miss();
            }
        }

        public void OnServerNibble(int requiredSpam)
        {
            if (CurrentState != FishingState.Waiting) return;

            targetSpamCount = requiredSpam;
            spamCount = 0;
            stateTimer = 0f;
            ChangeState(FishingState.Nibble);

            if (stateRoutine != null) StopCoroutine(stateRoutine);
            stateRoutine = StartCoroutine(NibbleTimeoutRoutine());
        }

        private IEnumerator NibbleTimeoutRoutine()
        {
            while (stateTimer < nibbleReactionWindow)
            {
                stateTimer += Time.deltaTime;
                yield return null;
            }

            if (CurrentState == FishingState.Nibble)
            {
                Miss();
            }
        }

        public void OnServerEnterCatching()
        {
            ChangeState(FishingState.Catching);
            stateTimer = 0f;
            OnCatchProgressChanged?.Invoke(spamCount, targetSpamCount);

            if (stateRoutine != null) StopCoroutine(stateRoutine);
            stateRoutine = StartCoroutine(CatchingRoutine());
        }

        private IEnumerator CatchingRoutine()
        {
            while (stateTimer < catchingDuration)
            {
                stateTimer += Time.deltaTime;
                yield return null;
            }

            if (CurrentState == FishingState.Catching)
            {
                fishingPlayer.CmdCompleteCatching(spamCount);
            }
        }

        private void RecordSpam()
        {
            spamCount++;
            OnCatchProgressChanged?.Invoke(spamCount, targetSpamCount);

            if (spamCount >= targetSpamCount)
            {
                fishingPlayer.CmdCompleteCatching(spamCount);
            }
        }

        public void OnFishingResult(bool success)
        {
            if (stateRoutine != null) StopCoroutine(stateRoutine);
            waitingForCastRelease = false;
            StopCastReleaseFallback();

            if (success)
            {
                ChangeState(FishingState.Success);
                StartCoroutine(SuccessRoutine());
            }
            else
            {
                ChangeState(FishingState.Failure);
                StartCoroutine(FailureRoutine());
            }
        }

        private IEnumerator SuccessRoutine()
        {
            yield return new WaitForSeconds(2f);
            EndFishing();
        }

        private IEnumerator FailureRoutine()
        {
            yield return new WaitForSeconds(1.5f);
            EndFishing();
        }

        private void Miss()
        {
            waitingForCastRelease = false;
            StopCastReleaseFallback();
            fishingPlayer.CmdFishingMissed();
            ChangeState(FishingState.Failure);
            StartCoroutine(FailureRoutine());
        }

        private void EndFishing()
        {
            waitingForCastRelease = false;
            StopCastReleaseFallback();
            ropeController?.RestoreHookToRod();
            ropeController?.SetVisible(false);
            fishingLineVisual?.SetFishingActive(false);
            ChangeState(FishingState.Idle);
        }

        private void StartCastReleaseFallback()
        {
            StopCastReleaseFallback();
            castReleaseFallbackRoutine = StartCoroutine(CastReleaseFallbackRoutine());
        }

        private void StopCastReleaseFallback()
        {
            if (castReleaseFallbackRoutine == null) return;

            StopCoroutine(castReleaseFallbackRoutine);
            castReleaseFallbackRoutine = null;
        }

        private IEnumerator CastReleaseFallbackRoutine()
        {
            yield return new WaitForSeconds(castReleaseFallbackDelay);
            castReleaseFallbackRoutine = null;

            if (waitingForCastRelease && CurrentState == FishingState.Casting)
            {
                OnCastRelease();
            }
        }

        private void CacheAnimatorParameters()
        {
            hasFishingCastTrigger = false;
            hasFishingBool = false;
            if (animator == null) return;

            fishingCastTriggerHash = Animator.StringToHash("FishingCast");
            fishingBoolHash = Animator.StringToHash("fishing");
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger &&
                    parameter.nameHash == fishingCastTriggerHash)
                {
                    hasFishingCastTrigger = true;
                }
                else if (parameter.type == AnimatorControllerParameterType.Bool &&
                         parameter.nameHash == fishingBoolHash)
                {
                    hasFishingBool = true;
                }
            }
        }

        private void SetFishingBool(bool value)
        {
            if (animator == null || !hasFishingBool) return;

            animator.SetBool(fishingBoolHash, value);
        }

        private void PlayCastAnimation()
        {
            if (animator == null || !hasFishingCastTrigger) return;

            SetFishingBool(false);
            animator.ResetTrigger(fishingCastTriggerHash);
            animator.SetTrigger(fishingCastTriggerHash);
        }
    }
}
