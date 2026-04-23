using System;
using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using MultiplayFishing.Core;
using MultiplayFishing.Data.Models;

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

        // 서비스 참조 (DI)
        private IDataService dataService;
        private IUserService userService;

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
            dataService = DIContainer.Resolve<IDataService>();
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

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            userService = DIContainer.Resolve<IUserService>();
            
            StartCoroutine(SmartEscapeRoutine());
            
            string savedName = PlayerPrefs.GetString("PlayerName", $"낚시꾼 {UnityEngine.Random.Range(100, 999)}");
            OnPlayerNameChangedEvent?.Invoke(savedName);
            CmdUpdatePlayerName(savedName);
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

        // ==================== 낚시 성공 로직 ====================

        /// <summary>
        /// 낚시 성공 시 호출 (서버에서 확률 및 크기 계산 → 클라이언트에게 결과 전달)
        /// </summary>
        [Command]
        public void CmdCatchFish()
        {
            if (dataService == null)
                dataService = DIContainer.Resolve<IDataService>();

            FishDataSO caughtFish = CalculateCatch();
            
            if (caughtFish != null)
            {
                // 서버에서 랜덤 크기 결정 (Min ~ Max)
                float randomLength = UnityEngine.Random.Range(caughtFish.minSize, caughtFish.maxSize);
                
                // 해당 클라이언트에게만 결과 전달
                TargetOnFishCaught(connectionToClient, caughtFish.id, caughtFish.fishName, caughtFish.rank, randomLength);
                
                // S급 이상이면 전체 공지
                if (caughtFish.rank == "S")
                {
                    RpcBroadcastSystemMessage($"{playerName}님이 [{caughtFish.fishName}] ({randomLength:F1}cm)을(를) 낚았습니다! 🎉");
                }
            }
            else
            {
                TargetOnFishMissed(connectionToClient);
            }
        }

        [TargetRpc]
        void TargetOnFishCaught(NetworkConnection target, string fishId, string fishName, string rank, float length)
        {
            if (isLocalPlayer)
            {
                // IUserService를 통해 로컬 인벤토리에 개별 데이터로 추가
                if (userService == null) userService = DIContainer.Resolve<IUserService>();
                userService.AddFish(fishId, length);
                
                Debug.Log($"<color=green>[낚시 성공]</color> [{rank}급] {fishName} ({length:F1}cm)을(를) 낚았습니다!");
                OnSystemMessage?.Invoke($"[{rank}급] {fishName} ({length:F1}cm) 획득!");
            }
        }

        [TargetRpc]
        void TargetOnFishMissed(NetworkConnection target)
        {
            if (isLocalPlayer)
            {
                Debug.Log("<color=red>[낚시 실패]</color> 물고기를 놓쳤습니다...");
            }
        }

        FishDataSO CalculateCatch()
        {
            List<FishDataSO> allFish = dataService.GetAllFishData();
            if (allFish == null || allFish.Count == 0) return null;

            float totalChance = 0f;
            foreach (var fish in allFish)
            {
                totalChance += fish.catchChance;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalChance);
            float currentChance = 0f;

            foreach (var fish in allFish)
            {
                currentChance += fish.catchChance;
                if (randomValue <= currentChance)
                {
                    return fish;
                }
            }

            return allFish[allFish.Count - 1];
        }

        // ==================== 애니메이션 및 캐릭터 제어 ====================

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

            if (moveSpeed > 0.1f) walkStopTimer = walkStopDelay;
            else walkStopTimer -= Time.deltaTime;

            animator.SetBool(walkParamHash, walkStopTimer > 0f);
            lastPosition = transform.position;
        }
    }
}
