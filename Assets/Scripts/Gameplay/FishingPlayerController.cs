using UnityEngine;
using UnityEngine.InputSystem;

namespace MultiplayFishing.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class FishingPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 7.5f;
        [SerializeField] private float jumpSpeed = 5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedVelocity = -2f;
        [SerializeField] private float groundSnapSearchHeight = 3f;
        [SerializeField] private float groundSnapSearchDistance = 8f;

        [Header("Rotation")]
        [SerializeField] private float maxTurnSpeed = 100f;
        [SerializeField] private float turnAcceleration = 3f;
        [SerializeField] private bool allowRotationWhileMovementBlocked = true;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string walkParameter = "WalkSpeed";
        [SerializeField] private float walkAnimDampTime = 0.15f;

        private CharacterController characterController;
        private Vector3 velocity;
        private bool isMovementBlocked;
        private bool isSprinting;
        private float currentSpeed;
        private float turnSpeed;
        private float mouseInputX;
        private bool mouseSteerEnabled;
        private int walkParameterHash;
        private bool hasWalkParameter;

        public bool IsMovementBlocked => isMovementBlocked;
        public bool IsSprinting => isSprinting;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController != null && !characterController.enabled)
                characterController.enabled = true;

            if (animator == null) animator = GetComponentInChildren<Animator>();

            walkParameterHash = Animator.StringToHash(walkParameter);
            CacheAnimatorParameter();
        }

        private void Update()
        {
            if (!enabled) return;

            HandleRotationInput();
            HandleMovementInput();
            UpdateWalkAnimation();
        }

        private void OnDisable()
        {
            turnSpeed = 0f;
            mouseInputX = 0f;
            if (mouseSteerEnabled)
            {
                mouseSteerEnabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void HandleRotationInput()
        {
            if (Keyboard.current == null) return;
            if (isMovementBlocked && !allowRotationWhileMovementBlocked) return;

            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                mouseSteerEnabled = !mouseSteerEnabled;
                Cursor.lockState = mouseSteerEnabled ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !mouseSteerEnabled;
            }

            float targetTurnSpeed = 0f;

            if (mouseSteerEnabled && Mouse.current != null)
            {
                float dpiScale = (Screen.dpi > 0) ? (Screen.dpi / 96f) : 1f;
                float sensitivity = turnAcceleration * dpiScale;

                mouseInputX += Mouse.current.delta.ReadValue().x * sensitivity;
                mouseInputX = Mathf.Clamp(mouseInputX, -1f, 1f);
                targetTurnSpeed = mouseInputX * maxTurnSpeed;
                mouseInputX = Mathf.MoveTowards(mouseInputX, 0f, sensitivity * Time.deltaTime);
            }
            else
            {
                if (Keyboard.current.qKey.isPressed) targetTurnSpeed -= maxTurnSpeed;
                if (Keyboard.current.eKey.isPressed) targetTurnSpeed += maxTurnSpeed;
            }

            turnSpeed = Mathf.MoveTowards(turnSpeed, targetTurnSpeed, turnAcceleration * maxTurnSpeed * Time.deltaTime);
            transform.Rotate(0f, turnSpeed * Time.deltaTime, 0f);
        }

        private void HandleMovementInput()
        {
            if (Keyboard.current == null) return;
            if (characterController == null) return;

            if (isMovementBlocked)
            {
                velocity = Vector3.zero;
                isSprinting = false;
                currentSpeed = 0f;
                return;
            }

            Vector2 input = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;

            bool wantsToSprint = input.sqrMagnitude > 0.01f
                && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
            isSprinting = wantsToSprint;
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            Vector3 move = (transform.forward * input.y) + (transform.right * input.x);
            if (move.sqrMagnitude > 1f) move.Normalize();

            if (characterController.isGrounded && velocity.y < 0f)
                velocity.y = groundedVelocity;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && characterController.isGrounded)
                velocity.y = Mathf.Sqrt(jumpSpeed * -2f * gravity);

            velocity.y += gravity * Time.deltaTime;

            Vector3 motion = (move * currentSpeed) + (Vector3.up * velocity.y);
            characterController.Move(motion * Time.deltaTime);
        }

        private void UpdateWalkAnimation()
        {
            if (animator == null || !hasWalkParameter) return;

            float targetSpeed;
            if (isMovementBlocked)
                targetSpeed = 0f;
            else
            {
                bool hasInput = Keyboard.current != null
                    && (Keyboard.current.wKey.isPressed || Keyboard.current.sKey.isPressed
                        || Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed);
                targetSpeed = hasInput ? (isSprinting ? 2f : 1f) : 0f;
            }

            animator.SetFloat(walkParameterHash, targetSpeed, walkAnimDampTime, Time.deltaTime);
        }

        public void SetMovementBlocked(bool blocked)
        {
            isMovementBlocked = blocked;
            if (!blocked)
            {
                return;
            }

            velocity = Vector3.zero;
            isSprinting = false;
            currentSpeed = 0f;
            turnSpeed = 0f;
            SnapToGround();
        }

        private void SnapToGround()
        {
            if (characterController == null) return;

            Vector3 rayOrigin = transform.position + Vector3.up * groundSnapSearchHeight;
            int groundMask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask("Water");
            RaycastHit[] hits = Physics.RaycastAll(
                rayOrigin,
                Vector3.down,
                groundSnapSearchDistance,
                groundMask,
                QueryTriggerInteraction.Ignore);

            if (hits == null || hits.Length == 0)
            {
                return;
            }

            RaycastHit bestHit = default;
            bool hasHit = false;
            float bestDistance = float.MaxValue;

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == null || hit.transform.IsChildOf(transform)) continue;
                if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Water")) continue;
                if (hit.distance >= bestDistance) continue;

                bestHit = hit;
                bestDistance = hit.distance;
                hasHit = true;
            }

            if (!hasHit)
            {
                return;
            }

            float controllerBottomOffset = characterController.center.y - characterController.height * 0.5f;
            Vector3 targetPosition = transform.position;
            targetPosition.y = bestHit.point.y - controllerBottomOffset;
            Vector3 displacement = targetPosition - transform.position;
            characterController.Move(displacement);
        }

        private void CacheAnimatorParameter()
        {
            hasWalkParameter = false;
            if (animator == null) return;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == walkParameterHash &&
                    parameter.type == AnimatorControllerParameterType.Float)
                {
                    hasWalkParameter = true;
                    break;
                }
            }
        }
    }
}
