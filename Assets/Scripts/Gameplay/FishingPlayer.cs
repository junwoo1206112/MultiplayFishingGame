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
            
            // 낚시 컨트롤러 초기화 추가
            SetupFishingController();
            
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

        // ==================== 낚시 시스템 (네트워크) ====================

        private FishingController fishingController;
        private FishDataSO pendingFish;
        private float pendingFishLength;
        private Coroutine serverFishingRoutine;

        private void SetupFishingController()
        {
            if (fishingController != null) return;
            
            fishingController = GetComponent<FishingController>();
            if (fishingController == null) fishingController = gameObject.AddComponent<FishingController>();

            // 컴포넌트 및 오브젝트 검색 (방어적 코딩)
            var lineVisual = GetComponentInChildren<FishingLineVisual>();
            
            GameObject ropeObject = null;
            Transform ropeTransform = transform.Find("FishingRope");
            if (ropeTransform == null) ropeTransform = transform.GetComponentInChildren<FishingRopeController>()?.GetType().GetField("fishingRopeObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(null) as Transform; // 대체 검색 시도
            
            if (ropeTransform != null) ropeObject = ropeTransform.gameObject;
            else Debug.LogWarning($"[FishingPlayer] 'FishingRope' 오브젝트를 {playerName}에게서 찾을 수 없습니다.");

            var ropeComponent = ropeObject?.GetComponent("Rope");
            
            // 바늘(Hook)과 끝점(Tip) 검색 시도
            Transform tip = transform.Find("TipPoint") ?? transform.Find("Skeleton/Hand_R/Rod/Tip"); 
            Transform hook = transform.Find("HookPoint") ?? (ropeTransform != null ? ropeTransform.Find("Hook") : null);

            var splashParticle = GetComponentInChildren<ParticleSystem>();
            
            // 카메라 검색 (로컬 플레이어인 경우만 필요)
            Camera pCam = GetComponentInChildren<Camera>();
            if (pCam == null && isLocalPlayer) pCam = Camera.main;

            var waterResolver = new FishingWaterSurfaceResolver(
                pCam,
                tip,
                null,
                LayerMask.GetMask("Water"),
                1.5f, 0.2f, 15f);

            var ropeController = new FishingRopeController(tip, hook, ropeObject, ropeComponent);
            var splashController = new FishingSplashController(splashParticle);

            fishingController.Initialize(this, animator, lineVisual, ropeController, splashController, waterResolver);
            
            if (tip == null || hook == null) 
            {
                Debug.LogError($"[FishingPlayer] {playerName}의 낚시 포인트(Tip/Hook) 설정이 누락되었습니다. 낚시 연출이 정상 작동하지 않을 수 있습니다.");
            }
        }

        [Command]
        public void CmdStartFishing(Vector3 targetPos)
        {
            if (serverFishingRoutine != null) StopCoroutine(serverFishingRoutine);
            serverFishingRoutine = StartCoroutine(ServerFishingTimer());
        }

        private IEnumerator ServerFishingTimer()
        {
            // 3~30초 대기
            float waitTime = UnityEngine.Random.Range(3f, 30f);
            yield return new WaitForSeconds(waitTime);

            // 물고기 결정
            if (dataService == null) dataService = DIContainer.Resolve<IDataService>();
            pendingFish = CalculateCatch();

            if (pendingFish != null)
            {
                pendingFishLength = UnityEngine.Random.Range(pendingFish.minSize, pendingFish.maxSize);
                
                // 등급별 연타 횟수 설정
                int requiredSpam = GetRequiredSpam(pendingFish.rank);
                
                // 클라이언트에 입질 알림
                TargetOnNibble(connectionToClient, requiredSpam);
            }
            else
            {
                // 물고기 없음 (실패)
                TargetOnFishingResult(connectionToClient, false, "", 0, 0);
            }
        }

        private int GetRequiredSpam(string rank)
        {
            // 별 개수에 따라 연타 횟수 상이하게 설정
            return rank switch
            {
                "★★★★★" => 30, // 5성
                "★★★★" => 22,  // 4성
                "★★★" => 15,   // 3성
                "★★" => 10,    // 2성
                "★" => 6,       // 1성
                _ => 10
            };
        }

        [TargetRpc]
        private void TargetOnNibble(NetworkConnection target, int requiredSpam)
        {
            fishingController.OnServerNibble(requiredSpam);
        }

        [Command]
        public void CmdTryHook()
        {
            // 서버에서도 0.5초 체크 가능하지만, 일단 클라이언트 신뢰 후 상태 전환
            TargetOnEnterCatching(connectionToClient);
        }

        [TargetRpc]
        private void TargetOnEnterCatching(NetworkConnection target)
        {
            fishingController.OnServerEnterCatching();
        }

        [Command]
        public void CmdCompleteCatching(int spamCount)
        {
            if (pendingFish == null) return;

            int required = GetRequiredSpam(pendingFish.rank);
            bool success = spamCount >= required;

            if (success)
            {
                // 보상 지급
                TargetOnFishingResult(connectionToClient, true, pendingFish.id, pendingFishLength, pendingFish.expReward);
                
                // 알림
                if (pendingFish.rank == "S")
                {
                    RpcBroadcastSystemMessage($"{playerName}님이 [{pendingFish.fishName}] ({pendingFishLength:F1}cm)을(를) 낚았습니다! 🎉");
                }
            }
            else
            {
                TargetOnFishingResult(connectionToClient, false, "", 0, 0);
            }

            pendingFish = null;
        }

        [Command]
        public void CmdFishingMissed()
        {
            if (serverFishingRoutine != null) StopCoroutine(serverFishingRoutine);
            pendingFish = null;
        }

        [TargetRpc]
        private void TargetOnFishingResult(NetworkConnection target, bool success, string fishId, float length, int exp)
        {
            if (success)
            {
                if (userService == null) userService = DIContainer.Resolve<IUserService>();
                
                // IUserService.AddFish는 내부적으로 경험치 추가, 도감 갱신, 저장을 모두 수행합니다.
                userService.AddFish(fishId, length);
                
                Debug.Log($"<color=green>[낚시 성공]</color> {fishId} 획득!");
                OnSystemMessage?.Invoke($"[낚시 성공] {fishId} 획득!");
            }

            fishingController.OnFishingResult(success);
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
