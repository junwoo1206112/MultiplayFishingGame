using UnityEngine;
using UnityEngine.InputSystem;

namespace MultiplayFishing.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedVelocity = -2f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string walkParameter = "WalkSpeed";
        [SerializeField] private float walkAnimDampTime = 0.15f;

        private CharacterController characterController;
        private Vector3 velocity;
        private Vector2 movementInput;
        private int walkParameterHash;
        private bool hasWalkParameter;
        private bool isMovementBlocked;

        public bool IsMovementBlocked { get => isMovementBlocked; set => isMovementBlocked = value; }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponent<Animator>();
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
            if (Mouse.current == null || isMovementBlocked) return;

            float mouseDeltaX = Mouse.current.delta.ReadValue().x;
            if (!Mathf.Approximately(mouseDeltaX, 0f))
            {
                transform.Rotate(Vector3.up, mouseDeltaX * rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        private void HandleMovement()
        {
            if (isMovementBlocked || Keyboard.current == null)
            {
                movementInput = Vector2.zero;
                return;
            }

            movementInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) movementInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) movementInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed) movementInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed) movementInput.x += 1f;

            Vector3 move = (transform.right * movementInput.x) + (transform.forward * movementInput.y);
            if (move.sqrMagnitude > 1f) move.Normalize();

            if (characterController.isGrounded && velocity.y < 0f)
            {
                velocity.y = groundedVelocity;
            }

            velocity.y += gravity * Time.deltaTime;
            Vector3 motion = (move * moveSpeed) + Vector3.up * velocity.y;
            characterController.Move(motion * Time.deltaTime);
        }

        private void UpdateWalkAnimation()
        {
            if (!hasWalkParameter && animator != null) CacheAnimatorParameter();

            // 입력 크기를 0~1 사이 값으로 정규화하여 부드러운 블렌딩에 사용.
            float targetSpeed = isMovementBlocked ? 0f : Mathf.Clamp01(movementInput.magnitude);

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
