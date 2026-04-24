using System;
using System.Collections;
using UnityEngine;
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
        [SerializeField] private float nibbleReactionWindow = 0.5f;
        [SerializeField] private float catchingDuration = 10f;

        [Header("References")]
        private FishingPlayer fishingPlayer;
        private Animator animator;
        private FishingLineVisual fishingLineVisual;
        private FishingRopeController ropeController;
        private FishingSplashController splashController;
        private FishingWaterSurfaceResolver waterResolver;

        // UI 이벤트
        public event Action<FishingState> OnStateChanged;
        public event Action<float> OnChargeProgressChanged; // 0 ~ 1
        public event Action<float, float> OnCatchProgressChanged; // current, target

        private Coroutine stateRoutine;
        private Vector3 targetPosition;
        private float stateTimer;
        private int spamCount;
        private int targetSpamCount;

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
            fishingLineVisual = lineVisual;
            ropeController = rope;
            splashController = splash;
            waterResolver = resolver;
        }

        private void Update()
        {
            if (!fishingPlayer.isLocalPlayer) return;

            HandleInput();
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            bool leftPressed = Mouse.current.leftButton.wasPressedThisFrame;
            bool leftHeld = Mouse.current.leftButton.isPressed;
            bool leftReleased = Mouse.current.leftButton.wasReleasedThisFrame;

            switch (CurrentState)
            {
                case FishingState.Idle:
                    if (leftPressed) StartCharging();
                    break;

                case FishingState.Charging:
                    if (leftHeld) UpdateCharging();
                    if (leftReleased) Cast();
                    break;

                case FishingState.Nibble:
                    if (leftPressed) TryHooking();
                    break;

                case FishingState.Catching:
                    if (leftPressed) RecordSpam();
                    break;
            }
        }

        private void ChangeState(FishingState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
            
            // 애니메이터 상태 업데이트 (기존 fishing 파라미터 활용)
            if (animator != null)
            {
                animator.SetBool("fishing", newState != FishingState.Idle);
            }
        }

        private void StartCharging()
        {
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
            // 물 표면 확인
            Vector3 castOffset = new Vector3(0, 0, currentChargeDistance);
            bool hasHit;
            Vector3 hitPoint;
            
            targetPosition = waterResolver.ResolveCastTarget(
                transform, 
                castOffset, 
                currentChargeDistance, 
                out hasHit, 
                out hitPoint);

            if (!hasHit)
            {
                Debug.Log("물이 아닌 곳에는 낚시를 할 수 없습니다.");
                ChangeState(FishingState.Idle);
                return;
            }

            ChangeState(FishingState.Casting);
            
            // 서버에 낚시 시작 알림
            fishingPlayer.CmdStartFishing(targetPosition);

            // 연출 시작
            if (stateRoutine != null) StopCoroutine(stateRoutine);
            stateRoutine = StartCoroutine(CastingRoutine(targetPosition, hitPoint, hasHit));
        }

        private IEnumerator CastingRoutine(Vector3 target, Vector3 hitPoint, bool hasHit)
        {
            // 기존 RopeController 활용
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
                    splashController.UpdatePendingPosition(hasHit, hitPoint, target, Vector3.zero, true, 0.02f);
                    splashController.Play();
                },
                fishingLineVisual);

            ChangeState(FishingState.Waiting);
        }

        private void TryHooking()
        {
            // 입질 시 0.5초 반응 체크
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
            // 입질 대기 (0.5초)
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
                // 시간 초과 시 서버에 현재 횟수 전달하여 검증
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
            // 물고기가 끌려 올라오는 애니메이션 (생략 또는 연출 추가)
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
            fishingPlayer.CmdFishingMissed();
            ChangeState(FishingState.Failure);
            StartCoroutine(FailureRoutine());
        }

        private void EndFishing()
        {
            ropeController.RestoreHookToRod();
            ropeController.SetVisible(false);
            fishingLineVisual.SetFishingActive(false);
            ChangeState(FishingState.Idle);
        }
    }
}
