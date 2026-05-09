using UnityEngine;
using UnityEngine.InputSystem;
using MultiplayFishing.Input;

namespace MultiplayFishing.Gameplay
{
    public class FishingInputHandler : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionProvider inputProvider;

        [Header("Settings")]
        [SerializeField] private bool blockCastWhileMoving = true;
        [SerializeField] private float castInputLockDuration = 1.0f;
        [SerializeField] private float reelInputLockDuration = 0.8f;

        public event System.Action OnCastRequested;
        public event System.Action OnReelRequested;
        public event System.Action OnToggleRodRequested;

        private float inputLockedUntil;

        public bool IsInputLocked => Time.time < inputLockedUntil;
        public bool IsMovementInputPressed => CheckMovementInput();

        private void Awake()
            {
                if (inputProvider == null)
                {
                    inputProvider = InputActionProvider.Instance;
                }
                
                // Ensure InputActionProvider exists and is enabled
                if (inputProvider == null)
                {
                    GameObject providerGO = new GameObject("InputActionProvider");
                    inputProvider = providerGO.AddComponent<InputActionProvider>();
                }
                
                inputProvider?.EnablePlayerActions();
            }

        public void Initialize(bool blockCastWhileMoving, float castInputLockDuration, float reelInputLockDuration)
        {
            this.blockCastWhileMoving = blockCastWhileMoving;
            this.castInputLockDuration = castInputLockDuration;
            this.reelInputLockDuration = reelInputLockDuration;
        }

        public void LockInput(bool isCasting)
        {
            float lockDuration = isCasting
                ? castInputLockDuration
                : reelInputLockDuration;
            inputLockedUntil = Time.time + lockDuration;
        }

        public void ProcessInput(bool isFishingActive, bool isRodEquipped, bool isClickChallengeRunning)
        {
            // Tab: ToggleRod via InputAction
            if (inputProvider != null && inputProvider.WasToggleRodPressedThisFrame())
            {
                OnToggleRodRequested?.Invoke();
                return;
            }

            // Fallback to Keyboard.current if InputActionProvider not available
            if (inputProvider == null && Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                OnToggleRodRequested?.Invoke();
                return;
            }

            // Attack/CastReel via InputAction
            bool attackPressed = inputProvider != null && inputProvider.WasAttackPressedThisFrame();
            bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            
            // TEMP: Debug log for input troubleshooting
            if (mousePressed)
            {
                Debug.Log($"[FishingInputHandler] Mouse clicked! attackPressed={attackPressed}, isRodEquipped={isRodEquipped}, isFishingActive={isFishingActive}, isClickChallengeRunning={isClickChallengeRunning}, inputLocked={IsInputLocked}, movementPressed={IsMovementInputPressed}");
            }
            if (!attackPressed && !mousePressed) return;

            if (isClickChallengeRunning) return;

            if (IsInputLocked) return;

            if (!isRodEquipped) return;

            if (!isFishingActive && blockCastWhileMoving && IsMovementInputPressed)
            {
                return;
            }

            if (!isFishingActive)
            {
                OnCastRequested?.Invoke();
            }
            else
            {
                OnReelRequested?.Invoke();
            }
        }

        private bool CheckMovementInput()
        {
            if (inputProvider != null)
            {
                return inputProvider.GetMoveInput().sqrMagnitude > 0.01f;
            }

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
    }
}