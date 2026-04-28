using UnityEngine;
using UnityEngine.InputSystem;

namespace MultiplayFishing.Input
{
    public class InputActionProvider : MonoBehaviour
    {
        [Header("Input Actions Asset")]
        [SerializeField] private InputActionAsset inputActionAsset;

        private InputActionMap playerActionMap;

        public InputAction MoveAction { get; private set; }
        public InputAction LookAction { get; private set; }
        public InputAction AttackAction { get; private set; }
        public InputAction SprintAction { get; private set; }
        public InputAction JumpAction { get; private set; }
        public InputAction InteractAction { get; private set; }

        public InputAction ToggleRodAction { get; private set; }
        public InputAction CastReelAction { get; private set; }

        public static InputActionProvider Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeActions();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            DisableAllActions();
        }

        private void InitializeActions()
        {
            if (inputActionAsset == null)
            {
                inputActionAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");
            }

#if UNITY_EDITOR
            if (inputActionAsset == null)
            {
                inputActionAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            }
#endif

            if (inputActionAsset == null)
            {
                Debug.LogError("[InputActionProvider] InputActionAsset not found! Place InputSystem_Actions.inputactions in Assets/Resources/ or assign in Inspector.");
                return;
            }

            playerActionMap = inputActionAsset.FindActionMap("Player");
            if (playerActionMap == null)
            {
                Debug.LogError("[InputActionProvider] Player action map not found!");
                return;
            }

            MoveAction = playerActionMap.FindAction("Move");
            LookAction = playerActionMap.FindAction("Look");
            AttackAction = playerActionMap.FindAction("Attack");
            SprintAction = playerActionMap.FindAction("Sprint");
            JumpAction = playerActionMap.FindAction("Jump");
            InteractAction = playerActionMap.FindAction("Interact");

            ToggleRodAction = new InputAction("ToggleRod", InputActionType.Button);
            ToggleRodAction.AddBinding("<Keyboard>/tab");

            CastReelAction = AttackAction;
        }

        public void EnablePlayerActions()
        {
            playerActionMap?.Enable();
            ToggleRodAction?.Enable();
        }

        public void DisablePlayerActions()
        {
            playerActionMap?.Disable();
            ToggleRodAction?.Disable();
        }

        private void DisableAllActions()
        {
            DisablePlayerActions();
        }

        public Vector2 GetMoveInput()
        {
            return MoveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        public Vector2 GetLookInput()
        {
            return LookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        public bool IsSprinting()
        {
            return SprintAction?.IsPressed() ?? false;
        }

        public bool WasAttackPressedThisFrame()
        {
            return AttackAction?.WasPressedThisFrame() ?? false;
        }

        public bool WasToggleRodPressedThisFrame()
        {
            return ToggleRodAction?.WasPressedThisFrame() ?? false;
        }

        public bool WasJumpPressedThisFrame()
        {
            return JumpAction?.WasPressedThisFrame() ?? false;
        }
    }
}