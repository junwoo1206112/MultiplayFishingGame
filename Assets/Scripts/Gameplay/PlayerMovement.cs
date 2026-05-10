using UnityEngine;
using UnityEngine.InputSystem;

namespace MultiplayFishing.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float runSpeed = 7.5f;
        [SerializeField] private float turnSnapSpeed = 12f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedVelocity = -2f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string walkParameter = "WalkSpeed";
        [SerializeField] private float walkAnimDampTime = 0.15f;

        private CharacterController characterController;
        private Vector3 velocity;
        private Vector2 movementInput;
        private Vector3 targetMoveDirection;
        private Quaternion targetRotation;
        private int walkParameterHash;
        private bool hasWalkParameter;
        private bool isMovementBlocked;

        public bool IsMovementBlocked { get => isMovementBlocked; set => isMovementBlocked = value; }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponent<Animator>();
            targetRotation = transform.rotation;
            targetMoveDirection = transform.forward;
            walkParameterHash = Animator.StringToHash(walkParameter);
            CacheAnimatorParameter();
        }

        private void Update()
        {
            HandleRotation();
            HandleMovement();
            UpdateWalkAnimation();
        }

        public void SetMovementBlocked(bool blocked)
        {
            isMovementBlocked = blocked;
            if (blocked) movementInput = Vector2.zero;
        }

        private void HandleRotation()
        {
            if (isMovementBlocked || Keyboard.current == null) return;

            Vector2 directionInput = GetDirectionInput();
            if (directionInput.sqrMagnitude > 0.01f)
            {
                targetMoveDirection = GetCameraRelativeDirection(directionInput);
                targetRotation = Quaternion.LookRotation(targetMoveDirection, Vector3.up);
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSnapSpeed * Time.deltaTime);
        }

        private void HandleMovement()
        {
            if (isMovementBlocked || Keyboard.current == null)
            {
                movementInput = Vector2.zero;
                return;
            }

            movementInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.sKey.isPressed ||
                Keyboard.current.dKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                movementInput.y = 1f;
            }

            Vector3 move = targetMoveDirection * movementInput.y;
            if (move.sqrMagnitude > 1f) move.Normalize();
            bool isRunning = Keyboard.current.shiftKey.isPressed && movementInput.sqrMagnitude > 0.1f;
            float currentSpeed = isRunning ? runSpeed : moveSpeed;

            if (characterController.isGrounded && velocity.y < 0f)
            {
                velocity.y = groundedVelocity;
            }

            velocity.y += gravity * Time.deltaTime;
            Vector3 motion = (move * currentSpeed) + Vector3.up * velocity.y;
            characterController.Move(motion * Time.deltaTime);
        }

        private Vector2 GetDirectionInput()
        {
            if (Keyboard.current == null) return Vector2.zero;

            Vector2 input = Vector2.zero;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            return input.normalized;
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return ((transform.forward * input.y) + (transform.right * input.x)).normalized;
            }

            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            return ((cameraForward * input.y) + (cameraRight * input.x)).normalized;
        }

        private void UpdateWalkAnimation()
        {
            if (!hasWalkParameter && animator != null) CacheAnimatorParameter();

            // 입력 크기를 0~1 사이 값으로 정규화하여 부드러운 블렌딩에 사용.
            bool isMoving = !isMovementBlocked && movementInput.magnitude > 0.1f;
            bool isRunning = Keyboard.current != null && Keyboard.current.shiftKey.isPressed && isMoving;
            float targetSpeed = isMoving ? (isRunning ? 2f : 1f) : 0f;

            if (animator != null && hasWalkParameter)
            {
                // dampTime을 사용하여 급격한 전환 없이 부드럽게 블렌딩.
                animator.SetFloat(walkParameterHash, targetSpeed, walkAnimDampTime, Time.deltaTime);
            }
        }

        private void CacheAnimatorParameter()
        {
            hasWalkParameter = false;
            if (animator == null) return;
            foreach (var p in animator.parameters)
            {
                if (p.nameHash == walkParameterHash) { hasWalkParameter = true; break; }
            }
        }
    }
}
