using UnityEngine;
using Mirror;
using System;

namespace MultiplayFishing.Network
{
    public class FishingRoomManager : NetworkManager
    {
        public static event Action NetworkStateChanged;

        public string ModeText => mode switch
        {
            NetworkManagerMode.Host => "호스트",
            NetworkManagerMode.ClientOnly => "클라이언트",
            _ => "오프라인"
        };

        public int ConnectedClientCount
        {
            get
            {
                if (NetworkServer.active) return NetworkServer.connections.Count;
                if (NetworkClient.active) return FindObjectsByType<NetworkIdentity>(FindObjectsSortMode.None).Length;
                return 0;
            }
        }

        public override void Awake()
        {
            base.Awake();
            // NetworkManager에는 showRoomGUI가 없으므로 해당 줄을 삭제했습니다.
        }

        public override void OnStartHost() { base.OnStartHost(); NetworkStateChanged?.Invoke(); }
        public override void OnStopHost() { base.OnStopHost(); NetworkStateChanged?.Invoke(); }
        public override void OnStartClient() 
        { 
            base.OnStartClient(); 
            NetworkStateChanged?.Invoke(); 
        }
        public override void OnStopClient() { base.OnStopClient(); NetworkStateChanged?.Invoke(); }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // 이미 플레이어가 있다면 생성하지 않음 (안전 장치)
            if (conn.identity != null) return;

            Transform startPos = GetStartPosition();
            GameObject playerObj = (startPos != null)
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            NetworkServer.AddPlayerForConnection(conn, playerObj);
            Debug.Log($"[NetworkManager] 플레이어 {conn.connectionId} 입장 완료.");
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            NetworkStateChanged?.Invoke();
        }

        public static string GetLocalIPAddress()
        {
            try {
                foreach (var ip in System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName()))
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ip.ToString().StartsWith("127.")) 
                        return ip.ToString();
            } catch { }
            return "127.0.0.1";
        }
    }
}
