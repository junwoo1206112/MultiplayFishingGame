using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;
    public float playerHeight = 1.0f;
    public float groundCheckDistance = 2.0f;
    
    private Vector2 moveInput;
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private float verticalVelocity = 0f;
    private float currentGroundHeight = 0f;
    
    private Camera playerCamera;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        
        // Remove CharacterController if exists
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            Destroy(cc);
        }
        
        // Remove all colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            Destroy(col);
        }
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Initialize ground height
        UpdateGroundHeight();
    }

    void Update()
    {
        // Update ground height
        UpdateGroundHeight();
        
        // Mouse look
        HandleMouseLook();
        
        // Handle movement
        HandleMovement();
        
        // Keep player at ground height
        MaintainGroundHeight();
    }
    
    void UpdateGroundHeight()
    {
        // Raycast down to find ground
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            currentGroundHeight = hit.point.y;
        }
    }
    
    void MaintainGroundHeight()
    {
        // Keep player at ground height + player height
        Vector3 pos = transform.position;
        pos.y = currentGroundHeight + playerHeight;
        transform.position = pos;
    }
    
    void HandleMouseLook()
    {
        if (Mouse.current == null) return;
        
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        // Horizontal rotation (player body)
        horizontalRotation += mouseDelta.x * mouseSensitivity * Time.deltaTime;
        
        // Vertical rotation (camera only)
        verticalRotation -= mouseDelta.y * mouseSensitivity * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        
        // Apply camera rotation
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
        
        // Apply player rotation (Y axis only)
        transform.rotation = Quaternion.Euler(0, horizontalRotation, 0);
    }
    
    void HandleMovement()
    {
        // Read WASD
        moveInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
        }
        
        // Calculate movement
        if (moveInput.magnitude > 0)
        {
            // Get camera forward/right (flat on XZ plane)
            Vector3 camForward = playerCamera != null ? playerCamera.transform.forward : Vector3.forward;
            Vector3 camRight = playerCamera != null ? playerCamera.transform.right : Vector3.right;
            
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
            
            // Calculate move direction relative to camera
            Vector3 move = (camForward * moveInput.y + camRight * moveInput.x).normalized;
            
            // Move on XZ plane only
            Vector3 newPos = transform.position;
            newPos += move * moveSpeed * Time.deltaTime;
            transform.position = newPos;
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
