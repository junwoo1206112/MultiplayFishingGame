using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    const string WalkParameterName = "Walk";
    const string CastParameterName = "Cast";
    static readonly int WalkParameterHash = Animator.StringToHash(WalkParameterName);
    static readonly int CastParameterHash = Animator.StringToHash(CastParameterName);
    Rigidbody rb;
    Animator anim;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float acceleration = 12f;

    [Header("Look")]
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float maxLookAngle = 80f;

    [Header("Grounding")]
    [SerializeField] float groundCheckDistance = 0.25f;
    [SerializeField] LayerMask groundMask = Physics.DefaultRaycastLayers;

    Vector2 moveInput;
    Vector3 currentHorizontalVelocity;
    float verticalRotation;
    float horizontalRotation;

    Camera playerCamera;
    Animator animator;
    Rigidbody playerRigidbody;
    CapsuleCollider playerCollider;
    bool isGrounded;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        horizontalRotation = transform.eulerAngles.y;

        if (animator != null)
            animator.applyRootMotion = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.freezeRotation = true;
            playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        
        LockCursor();
    }

    void Update()
    {
        ReadMoveInput();
        HandleMouseLook();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        UpdateGroundedState();
        HandleMovement();
    }

    void ReadMoveInput()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;

        if (animator != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            animator.SetTrigger(CastParameterHash);

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
    }

    void HandleMouseLook()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        horizontalRotation += mouseDelta.x * mouseSensitivity;
        verticalRotation -= mouseDelta.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        if (playerRigidbody == null)
            return;

        Vector3 referenceForward = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        Vector3 referenceRight = playerCamera != null ? playerCamera.transform.right : transform.right;

        referenceForward.y = 0f;
        referenceRight.y = 0f;
        referenceForward.Normalize();
        referenceRight.Normalize();

        Vector3 targetHorizontalVelocity = (referenceForward * moveInput.y + referenceRight * moveInput.x) * moveSpeed;
        currentHorizontalVelocity = Vector3.Lerp(
            currentHorizontalVelocity,
            targetHorizontalVelocity,
            acceleration * Time.fixedDeltaTime);

        Vector3 rigidbodyVelocity = playerRigidbody.linearVelocity;
        rigidbodyVelocity.x = currentHorizontalVelocity.x;
        rigidbodyVelocity.z = currentHorizontalVelocity.z;

        if (isGrounded && rigidbodyVelocity.y < 0f)
            rigidbodyVelocity.y = Mathf.Max(rigidbodyVelocity.y, -2f);

        playerRigidbody.linearVelocity = rigidbodyVelocity;
    }

    void UpdateAnimation()
    {
        if (animator == null)
            return;

        bool isWalking = moveInput.sqrMagnitude > 0.0001f && currentHorizontalVelocity.sqrMagnitude > 0.01f;
        animator.SetBool(WalkParameterHash, isWalking);
    }

    void UpdateGroundedState()
    {
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        float castDistance = groundCheckDistance;

        if (playerCollider != null)
            castDistance += (playerCollider.height * 0.5f) - playerCollider.radius;

        isGrounded = Physics.SphereCast(
            origin,
            playerCollider != null ? playerCollider.radius * 0.9f : 0.25f,
            Vector3.down,
            out _,
            castDistance,
            groundMask,
            QueryTriggerInteraction.Ignore);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            LockCursor();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
