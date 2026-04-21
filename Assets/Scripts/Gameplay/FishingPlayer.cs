using System;
using UnityEngine;
using Mirror;
using System.Collections;

namespace MultiplayFishing.Gameplay
{
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

        private void Awake()
        {
            // 인스펙터 할당 누락 대비
            if (characterRenderer == null) characterRenderer = GetComponentInChildren<Renderer>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            // 이미 설정된 이름이 없다면(빈 문자열) 랜덤한 이름 부여
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = $"낚시꾼 {UnityEngine.Random.Range(100, 999)}";
            }
            
            playerColor = Color.HSVToRGB(UnityEngine.Random.value, 0.8f, 1.0f);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            UpdateCharacterColor(playerColor);
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
            
            // 로컬에 저장된 이름을 불러와서 서버로 전송
            string savedName = PlayerPrefs.GetString("PlayerName", $"낚시꾼 {UnityEngine.Random.Range(100, 999)}");
            CmdUpdatePlayerName(savedName);
        }

        [Command]
        public void CmdUpdatePlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return;

            string oldName = playerName;
            playerName = newName;
            Debug.Log($"[Server] 이름 변경 요청: '{oldName}' -> '{newName}'");

            // 알림 조건: 이전 이름이 비어있거나, 새로 설정된 이름이 이전과 다를 때 (처음 한 번만)
            // 중복 알림을 방지하기 위해 서버에서 체크
            RpcBroadcastSystemMessage($"{newName}님이 입장하셨습니다.");
            Debug.Log($"[Server] RpcBroadcastSystemMessage 호출 완료: {newName}");
        }

        [ClientRpc]
        private void RpcBroadcastSystemMessage(string message)
        {
            Debug.Log($"[Client] Rpc 수신됨: {message}");
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
    }
}
