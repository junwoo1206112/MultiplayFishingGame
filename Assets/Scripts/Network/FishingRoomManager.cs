using UnityEngine;
using Mirror;
using System;
using System.Threading.Tasks;

namespace MultiplayFishing.Network
{
    public class FishingRoomManager : NetworkManager
    {
        public static event Action NetworkStateChanged;
        public static event Action<string> JoinCodeChanged;

        public UnityRelayTransport RelayTransport
        {
            get
            {
                if (transport is UnityRelayTransport relay)
                    return relay;
                return GetComponent<UnityRelayTransport>();
            }
        }

        public string CurrentJoinCode
        {
            get
            {
                var rt = RelayTransport;
                return rt != null ? rt.JoinCode : null;
            }
        }

        public bool IsRelayReady => RelayTransport != null && !string.IsNullOrEmpty(CurrentJoinCode);

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
        }

        public override void OnStartHost() { base.OnStartHost(); NetworkStateChanged?.Invoke(); }
        public override void OnStopHost() { base.OnStopHost(); NetworkStateChanged?.Invoke(); }
        public override void OnClientConnect()
        {
            base.OnClientConnect();
        }

        public override void OnStartClient() 
        { 
            base.OnStartClient(); 
            NetworkStateChanged?.Invoke(); 
        }
        public override void OnStopClient() { base.OnStopClient(); NetworkStateChanged?.Invoke(); }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            if (conn.identity != null) return;

            Transform startPos = GetStartPosition();
            GameObject playerObj = (startPos != null)
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            NetworkServer.AddPlayerForConnection(conn, playerObj);
            if (startPos != null)
            {
                Debug.Log($"[FishingRoomManager] Player {conn.connectionId} spawned at {startPos.name} ({startPos.position}).");
            }
            else
            {
                Debug.LogWarning($"[FishingRoomManager] Player {conn.connectionId} spawned without a NetworkStartPosition.");
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            NetworkStateChanged?.Invoke();
        }

        private bool isCreatingRoom;

        public async void CreateRoom(string roomName)
        {
            var rt = RelayTransport;
            if (rt == null)
            {
                Debug.LogError("[FishingRoomManager] Transport가 UnityRelayTransport가 아닙니다. NetworkManager 프리팹을 확인하세요.");
                return;
            }

            if (isCreatingRoom)
            {
                Debug.LogWarning("[FishingRoomManager] 이미 방 생성 중입니다.");
                return;
            }

            isCreatingRoom = true;
            Debug.Log($"[FishingRoomManager] Unity Relay 방 생성 중... ({roomName})");
            try
            {
                string code = await rt.CreateRelayAllocation(maxConnections);
                JoinCodeChanged?.Invoke(code);
                StartHost();
                NetworkStateChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[FishingRoomManager] Relay 방 생성 실패: {e.Message}");
            }
            finally
            {
                isCreatingRoom = false;
            }
        }

        public void JoinRoom(string joinCode)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
            {
                Debug.LogWarning("[FishingRoomManager] joinCode가 비어 있습니다.");
                return;
            }

            var rt = RelayTransport;
            if (rt == null)
            {
                Debug.LogError("[FishingRoomManager] Transport가 UnityRelayTransport가 아닙니다.");
                return;
            }

            Debug.Log($"[FishingRoomManager] Unity Relay 방 참가 중... (joinCode: {joinCode})");
            rt.PrepareRelayJoin(joinCode.Trim());
            StartClient();
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
