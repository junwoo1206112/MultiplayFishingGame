using System;
using System.Collections;
using Mirror;
using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    public class FishingPlayer : NetworkBehaviour
    {
        public event Action<string> OnPlayerNameChangedEvent;
        public event Action<Color> OnPlayerColorChangedEvent;

        [Header("Player Identification")]
        [SyncVar(hook = nameof(OnPlayerNameChanged))] public string playerName = string.Empty;
        [SyncVar(hook = nameof(OnPlayerColorChanged))] public Color playerColor = Color.white;

        [Header("Setup References")]
        [SerializeField] private Renderer characterRenderer;

        private void Awake()
        {
            if (characterRenderer == null)
            {
                characterRenderer = GetComponentInChildren<Renderer>();
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = $"낚시꾼{UnityEngine.Random.Range(100, 999)}";
            }

            playerColor = Color.HSVToRGB(UnityEngine.Random.value, 0.8f, 1.0f);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            UpdateCharacterColor(playerColor);
        }

        private void OnPlayerNameChanged(string oldValue, string newValue)
        {
            OnPlayerNameChangedEvent?.Invoke(newValue);
        }

        private void OnPlayerColorChanged(Color oldColor, Color newColor)
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

            string savedName = PlayerPrefs.GetString("PlayerName", $"낚시꾼{UnityEngine.Random.Range(100, 999)}");
            CmdUpdatePlayerName(savedName);
        }

        [Command]
        public void CmdUpdatePlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            string oldName = playerName;
            playerName = newName;
            Debug.Log($"[Server] 이름 변경 요청: '{oldName}' -> '{newName}'");

            RpcBroadcastSystemMessage($"{newName}님이 입장했습니다.");
            Debug.Log($"[Server] RpcBroadcastSystemMessage 호출 완료: {newName}");
        }

        [ClientRpc]
        private void RpcBroadcastSystemMessage(string message)
        {
            Debug.Log($"[Client] Rpc 수신: {message}");

            if (!TryShowNotification(message))
            {
                Debug.LogError("[Client] NotificationUI 인스턴스를 찾을 수 없습니다. 씬 구성을 확인하세요.");
            }
        }

        private static bool TryShowNotification(string message)
        {
            Type notificationType = Type.GetType("MultiplayFishing.UI.NotificationUI, MultiplayFishing.UI");
            if (notificationType == null)
            {
                return false;
            }

            var instanceProperty = notificationType.GetProperty("Instance");
            object instance = instanceProperty?.GetValue(null);
            if (instance == null)
            {
                return false;
            }

            var showMessageMethod = notificationType.GetMethod("ShowMessage", new[] { typeof(string) });
            if (showMessageMethod == null)
            {
                return false;
            }

            showMessageMethod.Invoke(instance, new object[] { message });
            return true;
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
