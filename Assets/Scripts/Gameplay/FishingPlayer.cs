using System;
using UnityEngine;
using Mirror;
using System.Collections;

namespace MultiplayFishing.Gameplay
{
    [DefaultExecutionOrder(200)]
    public class FishingPlayer : NetworkBehaviour
    {
        public event Action<string> OnPlayerNameChangedEvent;
        public event Action<Color> OnPlayerColorChangedEvent;
        public static event Action<string> OnSystemMessage;

        [Header("Player Identification")]
        [SyncVar(hook = nameof(OnPlayerNameChanged))] public string playerName = "";
        [SyncVar(hook = nameof(OnPlayerColorChanged))] public Color playerColor = Color.white;

        [Header("Setup References")]
        [SerializeField] private Renderer characterRenderer;
        [SerializeField] private float walkStopDelay = 0.3f;

        private Animator animator;
        private CharacterController characterController;
        private int walkParamHash;
        private bool hasWalkParam;
        private float walkStopTimer;
        private Vector3 lastPosition;

        private void Awake()
        {
            if (characterRenderer == null) characterRenderer = GetComponentInChildren<Renderer>();
            animator = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
            CacheWalkParam();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            playerColor = Color.HSVToRGB(UnityEngine.Random.value, 0.8f, 1.0f);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            UpdateCharacterColor(playerColor);
            if (!string.IsNullOrEmpty(playerName))
            {
                OnPlayerNameChangedEvent?.Invoke(playerName);
            }
        }

        void OnPlayerNameChanged(string oldValue, string newValue) => OnPlayerNameChangedEvent?.Invoke(newValue);
        
        void OnPlayerColorChanged(Color oldColor, Color newColor) 
        { 
            UpdateCharacterColor(newColor); 
            OnPlayerColorChangedEvent?.Invoke(newColor); 
        }

        private void UpdateCharacterColor(Color color)
        {
            if (characterRenderer != null) 
            {
                characterRenderer.material.color = color;
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            StartCoroutine(SmartEscapeRoutine());
            
            string savedName = PlayerPrefs.GetString("PlayerName", $"낚시꾼 {UnityEngine.Random.Range(100, 999)}");
            OnPlayerNameChangedEvent?.Invoke(savedName);
            CmdUpdatePlayerName(savedName);
        }

        [Command]
        public void CmdUpdatePlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return;

            bool isFirstName = string.IsNullOrEmpty(playerName);
            playerName = newName;

            if (isFirstName)
            {
                RpcBroadcastSystemMessage($"{newName}님이 입장하셨습니다.");
            }
        }

        [ClientRpc]
        private void RpcBroadcastSystemMessage(string message)
        {
            OnSystemMessage?.Invoke(message);
        }

        private IEnumerator SmartEscapeRoutine()
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                transform.position += Vector3.up * 0.2f; 
                yield return new WaitForFixedUpdate();
                cc.enabled = true;
            }
        }

        private void CacheWalkParam()
        {
            hasWalkParam = false;
            if (animator == null) return;

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool && param.name == "Walk")
                {
                    hasWalkParam = true;
                    walkParamHash = param.nameHash;
                    break;
                }
            }
        }

        private void Start()
        {
            CacheWalkParam();
            lastPosition = transform.position;
        }

        private void Update()
        {
            UpdateWalkAnimation();
        }

        private void UpdateWalkAnimation()
        {
            if (animator == null || !hasWalkParam) return;

            Vector3 delta = transform.position - lastPosition;
            delta.y = 0f;
            float moveSpeed = delta.sqrMagnitude / (Time.deltaTime * Time.deltaTime);

            if (moveSpeed > 0.1f)
            {
                walkStopTimer = walkStopDelay;
            }
            else
            {
                walkStopTimer -= Time.deltaTime;
            }

            animator.SetBool(walkParamHash, walkStopTimer > 0f);
            lastPosition = transform.position;
        }
    }
}
