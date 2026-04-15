using System;
using UnityEngine;
using Mirror;

namespace MultiplayFishing.Gameplay
{
    public class FishingPlayer : NetworkBehaviour
    {
        public event Action<string> OnPlayerNameChangedEvent;
        public event Action<int> OnScoreChangedEvent;
        public event Action<bool> OnReadyChangedEvent;

        static readonly System.Collections.Generic.List<FishingPlayer> playersList = new System.Collections.Generic.List<FishingPlayer>();

        [Header("Player Info")]
        [SyncVar(hook = nameof(OnPlayerNameChanged))]
        public string playerName = "";

        [SyncVar(hook = nameof(OnScoreChanged))]
        public int score = 0;

        [SyncVar(hook = nameof(OnReadyChanged))]
        public bool isReady = false;

        #region Server

        public override void OnStartServer()
        {
            base.OnStartServer();
            playersList.Add(this);
            playerName = $"낚시꾼 {playersList.Count}";
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
            OnPlayerNameChangedEvent?.Invoke(playerName);
            OnScoreChangedEvent?.Invoke(score);
            OnReadyChangedEvent?.Invoke(isReady);
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
        }

        void OnPlayerNameChanged(string oldValue, string newValue)
        {
            OnPlayerNameChangedEvent?.Invoke(newValue);
        }

        void OnScoreChanged(int oldValue, int newValue)
        {
            OnScoreChangedEvent?.Invoke(newValue);
        }

        void OnReadyChanged(bool oldValue, bool newValue)
        {
            OnReadyChangedEvent?.Invoke(newValue);
        }

        #endregion

        #region Public Client API

        public void SetPlayerName(string name)
        {
            if (isLocalPlayer)
            {
                CmdSetPlayerName(name);
            }
        }

        public void SetReady(bool ready)
        {
            if (isLocalPlayer)
            {
                CmdSetReady(ready);
            }
        }

        #endregion
    }
}