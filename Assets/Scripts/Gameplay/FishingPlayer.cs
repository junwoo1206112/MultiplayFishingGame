using System;
using UnityEngine;
using Mirror;

namespace MultiplayFishing.Gameplay
{
    /// <summary>
    /// 플레이어의 상태(이름, 점수, 색상 등)를 관리하고 모든 클라이언트에 동기화합니다.
    /// </summary>
    public class FishingPlayer : NetworkBehaviour
    {
        public event Action<string> OnPlayerNameChangedEvent;
        public event Action<int> OnScoreChangedEvent;
        public event Action<bool> OnReadyChangedEvent;
        public event Action<Color> OnPlayerColorChangedEvent;

        static readonly System.Collections.Generic.List<FishingPlayer> playersList = new System.Collections.Generic.List<FishingPlayer>();

        [Header("Player Info")]
        [SyncVar(hook = nameof(OnPlayerNameChanged))]
        public string playerName = "";

        [SyncVar(hook = nameof(OnScoreChanged))]
        public int score = 0;

        [SyncVar(hook = nameof(OnReadyChanged))]
        public bool isReady = false;

        [SyncVar(hook = nameof(OnPlayerColorChanged))]
        public Color playerColor = Color.white;

        [Header("References")]
        [SerializeField] private Renderer characterRenderer; // 색상을 적용할 메쉬 렌더러

        #region Server

        public override void OnStartServer()
        {
            base.OnStartServer();
            playersList.Add(this);
            
            // 이름 부여
            playerName = $"낚시꾼 {playersList.Count}";
            
            // 랜덤 색상 부여 (서버에서 결정)
            playerColor = Color.HSVToRGB(UnityEngine.Random.value, 0.7f, 0.9f);
        }

        [Command]
        void CmdSetPlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return;
            if (newName.Length > 20) return;
            playerName = newName;
        }

        [Command]
        void CmdSetReady(bool ready)
        {
            isReady = ready;
        }

        [ServerCallback]
        internal static void ResetPlayerNumbers()
        {
            for (int i = 0; i < playersList.Count; i++)
            {
                if (playersList[i] != null)
                {
                    playersList[i].playerName = $"낚시꾼 {i + 1}";
                }
            }
        }

        public override void OnStopServer()
        {
            playersList.Remove(this);
            ResetPlayerNumbers();
            base.OnStopServer();
        }

        #endregion

        #region Client

        public override void OnStartClient()
        {
            // 초기 상태 적용
            OnPlayerNameChangedEvent?.Invoke(playerName);
            OnScoreChangedEvent?.Invoke(score);
            OnReadyChangedEvent?.Invoke(isReady);
            UpdateCharacterColor(playerColor);
        }

        public override void OnStartLocalPlayer()
        {
            Debug.Log($"[FishingPlayer] 로컬 플레이어 시작: {playerName}");
        }

        public override void OnStopClient()
        {
            OnPlayerNameChangedEvent = null;
            OnScoreChangedEvent = null;
            OnReadyChangedEvent = null;
            OnPlayerColorChangedEvent = null;
        }

        // SyncVar Hook 함수들
        void OnPlayerNameChanged(string oldValue, string newValue) => OnPlayerNameChangedEvent?.Invoke(newValue);
        void OnScoreChanged(int oldValue, int newValue) => OnScoreChangedEvent?.Invoke(newValue);
        void OnReadyChanged(bool oldValue, bool newValue) => OnReadyChangedEvent?.Invoke(newValue);
        
        void OnPlayerColorChanged(Color oldColor, Color newColor)
        {
            UpdateCharacterColor(newColor);
            OnPlayerColorChangedEvent?.Invoke(newColor);
        }

        /// <summary>
        /// 실제 캐릭터 모델의 색상을 변경합니다.
        /// </summary>
        void UpdateCharacterColor(Color color)
        {
            if (characterRenderer != null)
            {
                // 공유 머티리얼이 아닌 개별 인스턴스 머티리얼 색상을 변경합니다.
                characterRenderer.material.color = color;
            }
        }

        #endregion

        #region Public Client API

        public void SetPlayerName(string name)
        {
            if (isLocalPlayer) CmdSetPlayerName(name);
        }

        public void SetReady(bool ready)
        {
            if (isLocalPlayer) CmdSetReady(ready);
        }

        #endregion
    }
}
