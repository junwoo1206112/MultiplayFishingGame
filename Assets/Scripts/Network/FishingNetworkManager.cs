using System;
using UnityEngine;
using Mirror;

namespace MultiplayFishing.Network
{
    public class FishingNetworkManager : NetworkManager
    {
        public static new FishingNetworkManager singleton => (FishingNetworkManager)NetworkManager.singleton;

        [Header("Game Settings")]
        [Tooltip("최대 플레이어 수 (1 connection = 1 player)")]
        [SerializeField] private int maxPlayers = 4;

        public int MaxPlayers => maxPlayers;
        public int ConnectedClientCount => NetworkServer.connections.Count;

        public string ModeText => mode switch
        {
            NetworkManagerMode.Host => "호스트",
            NetworkManagerMode.ServerOnly => "서버",
            NetworkManagerMode.ClientOnly => "클라이언트",
            _ => "오프라인"
        };

        public static event Action NetworkStateChanged;
        public event Action OnServerStartedEvent;
        public event Action OnServerStoppedEvent;
        public event Action OnClientStartedEvent;
        public event Action OnClientStoppedEvent;
        public event Action<NetworkConnectionToClient> OnPlayerJoinedEvent;
        public event Action<NetworkConnectionToClient> OnPlayerLeftEvent;

        #region Server System Callbacks

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (NetworkServer.connections.Count >= maxPlayers)
            {
                conn.Disconnect();
                Debug.LogWarning($"[FishingNetworkManager] 연결 거부: 최대 플레이어 수({maxPlayers}) 도달");
            }
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            OnPlayerJoinedEvent?.Invoke(conn);
            NotifyStateChanged();
            Debug.Log($"[FishingNetworkManager] 플레이어 접속: {conn.connectionId}");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            OnPlayerLeftEvent?.Invoke(conn);
            NotifyStateChanged();
            Debug.Log($"[FishingNetworkManager] 플레이어 퇴장: {conn.connectionId}");
            base.OnServerDisconnect(conn);
        }

        public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError, string message)
        {
            Debug.LogError($"[FishingNetworkManager] 서버 에러: {transportError} - {message}");
        }

        #endregion

        #region Client System Callbacks

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            OnClientStartedEvent?.Invoke();
            NotifyStateChanged();
            Debug.Log("[FishingNetworkManager] 클라이언트 연결 완료");
        }

        public override void OnClientDisconnect()
        {
            OnClientStoppedEvent?.Invoke();
            NotifyStateChanged();
            Debug.Log("[FishingNetworkManager] 클라이언트 연결 해제");
        }

        #endregion

        #region Start & Stop Callbacks

        public override void OnStartServer()
        {
            OnServerStartedEvent?.Invoke();
            NotifyStateChanged();
            Debug.Log($"[FishingNetworkManager] 서버 시작 - 최대 플레이어: {maxPlayers}");
        }

        public override void OnStopServer()
        {
            OnServerStoppedEvent?.Invoke();
            NotifyStateChanged();
        }

        public override void OnStartClient()
        {
            NotifyStateChanged();
        }

        public override void OnStopHost()
        {
            NotifyStateChanged();
        }

        #endregion

        static void NotifyStateChanged()
        {
            NetworkStateChanged?.Invoke();
        }

        public static string GetLocalIPAddress()
        {
            try
            {
                System.Net.IPAddress[] addresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
                foreach (System.Net.IPAddress ip in addresses)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        string ipStr = ip.ToString();
                        if (!ipStr.StartsWith("127."))
                            return ipStr;
                    }
                }
            }
            catch { }

            return "127.0.0.1";
        }
    }
}