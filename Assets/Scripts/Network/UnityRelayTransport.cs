using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Mirror;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Networking.Transport.Utilities;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection;

namespace MultiplayFishing.Network
{
    public class UnityRelayTransport : Transport
    {
        [Header("Transport Configuration")]
        public int maxPacketSize = 1400;
        [SerializeField] private float clientConnectTimeoutSeconds = 15f;
        [SerializeField] private int relayHeartbeatIntervalMs = 1000;
        [SerializeField] private int maxFrameTimeMs = 100;

        private NetworkDriver serverDriver;
        private NetworkDriver clientDriver;
        private NetworkPipeline serverReliablePipeline;
        private NetworkPipeline clientReliablePipeline;


        private bool serverActive;
        private Dictionary<int, NetworkConnection> connections = new Dictionary<int, NetworkConnection>();
        private int nextConnectionId = 1;

        private bool clientActive;
        private bool clientConnecting;
        private bool clientDisconnectNotified;
        private float clientConnectStartedTime;
        private NetworkConnection clientConnection;

        private ConcurrentQueue<ServerSendMessage> serverSendQueue = new ConcurrentQueue<ServerSendMessage>();
        private ConcurrentQueue<byte[]> clientSendQueue = new ConcurrentQueue<byte[]>();

        private string joinCode;
        private string pendingJoinCode;
        private Allocation pendingAllocation;
        private bool servicesInitialized;

        private struct ServerSendMessage
        {
            public int connectionId;
            public byte[] data;
        }

        public string JoinCode => joinCode;

        public async Task<string> CreateRelayAllocation(int maxPlayers)
        {
            servicesInitialized = false;
            await InitServices();
            Debug.Log("[UnityRelayTransport] Calling CreateAllocationAsync...");
            pendingAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers, region: null);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(pendingAllocation.AllocationId);
            Debug.Log($"[UnityRelayTransport] Allocation created. Join code: {joinCode}");
            return joinCode;
        }

        public void PrepareRelayJoin(string code)
        {
            pendingJoinCode = code;
        }

        private async Task InitServices()
        {
            if (servicesInitialized) return;
            try
            {
                Debug.Log("[UnityRelayTransport] Initializing Unity Services...");
                await UnityServices.InitializeAsync();
                Debug.Log("[UnityRelayTransport] UnityServices initialized.");
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("[UnityRelayTransport] Signing in anonymously...");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log("[UnityRelayTransport] Signed in.");
                }
                servicesInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityRelayTransport] Init failed: {e.Message}");
                throw;
            }
        }

        public override bool Available() => true;
        public override bool ClientConnected() => clientActive;

        public override void ClientConnect(string address)
        {
            DisposeClientDriver();
            clientDisconnectNotified = false;

            if (string.IsNullOrEmpty(pendingJoinCode))
            {
                Debug.LogError("[UnityRelayTransport] No join code. Call PrepareRelayJoin() first.");
                NotifyClientDisconnected();
                return;
            }
            _ = ConnectRelayAsync();
        }

        private async Task ConnectRelayAsync()
        {
            try
            {
                await InitServices();
                string joinCodeToUse = pendingJoinCode.Trim();
                pendingJoinCode = null;

                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCodeToUse);

                var relayData = joinAllocation.ToRelayServerData("dtls");
                var settings = new NetworkSettings();
                try
                {
                    ApplyRelaySettings(ref settings, ref relayData);

                    DisposeClientDriver();

                    clientDriver = NetworkDriver.Create(settings);
                }
                finally
                {
                    settings.Dispose();
                }

                clientReliablePipeline = clientDriver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
                clientConnection = clientDriver.Connect();
                clientActive = false;
                clientConnecting = clientConnection.IsCreated;
                clientDisconnectNotified = false;
                clientConnectStartedTime = Time.realtimeSinceStartup;
                Debug.Log($"[UnityRelayTransport] Client connecting... IsCreated={clientConnection.IsCreated}");

                if (!clientConnection.IsCreated)
                {
                    clientConnecting = false;
                    Debug.LogError("[UnityRelayTransport] Client connection was not created.");
                    NotifyClientDisconnected();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityRelayTransport] Relay connect failed: {e.Message}");
                pendingJoinCode = null;
                clientActive = false;
                clientConnecting = false;
                NotifyClientDisconnected();
            }
        }

        public override void ClientSend(ArraySegment<byte> segment, int channelId)
        {
            byte[] copy = new byte[segment.Count];
            Array.Copy(segment.Array, segment.Offset, copy, 0, segment.Count);
            clientSendQueue.Enqueue(copy);
        }

        public override void ClientDisconnect()
        {
            if (clientDriver.IsCreated && clientConnection.IsCreated)
                clientDriver.Disconnect(clientConnection);
            clientActive = false;
            clientConnecting = false;
            while (clientSendQueue.TryDequeue(out _)) { }
            NotifyClientDisconnected();
            DisposeClientDriver();
        }

        public override Uri ServerUri() => null;
        public override bool ServerActive() => serverActive;

        public override void ServerStart()
        {
            if (pendingAllocation == null)
            {
                Debug.LogError("[UnityRelayTransport] No allocation. Call CreateRelayAllocation() first.");
                return;
            }

            try
            {
                var relayData = pendingAllocation.ToRelayServerData("dtls");
                var settings = new NetworkSettings();
                try
                {
                    ApplyRelaySettings(ref settings, ref relayData);

                    DisposeServerDriver();

                    serverDriver = NetworkDriver.Create(settings);
                }
                finally
                {
                    settings.Dispose();
                }

                serverReliablePipeline = serverDriver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
                int bindResult = serverDriver.Bind(NetworkEndpoint.AnyIpv4);
                if (bindResult != 0)
                {
                    Debug.LogError($"[UnityRelayTransport] Bind failed: {bindResult}");
                    serverDriver.Dispose();
                    return;
                }
                int listenResult = serverDriver.Listen();
                if (listenResult != 0)
                {
                    Debug.LogError($"[UnityRelayTransport] Listen failed: {listenResult}");
                    serverDriver.Dispose();
                    return;
                }
                serverActive = true;
                Debug.Log("[UnityRelayTransport] Server started.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityRelayTransport] Server start failed: {e.Message}");
            }
        }

        public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
        {
            byte[] copy = new byte[segment.Count];
            Array.Copy(segment.Array, segment.Offset, copy, 0, segment.Count);
            serverSendQueue.Enqueue(new ServerSendMessage { connectionId = connectionId, data = copy });
        }

        public override void ServerDisconnect(int connectionId)
        {
            if (connections.TryGetValue(connectionId, out NetworkConnection conn))
            {
                if (serverDriver.IsCreated && conn.IsCreated)
                    serverDriver.Disconnect(conn);
                connections.Remove(connectionId);
                OnServerDisconnected?.Invoke(connectionId);
            }
        }

        public override string ServerGetClientAddress(int connectionId) => "relay";
        public override int GetMaxPacketSize(int channelId) => maxPacketSize;

        public override void ServerStop()
        {
            DisconnectAllServerConnections();
            DisposeServerDriver();
        }

        public override void Shutdown()
        {
            ServerStop();
            ClientDisconnect();
            if (serverDriver.IsCreated) serverDriver.Dispose();
            if (clientDriver.IsCreated) clientDriver.Dispose();
        }

        public override void ClientEarlyUpdate()
        {
            if (!clientDriver.IsCreated) return;

            clientDriver.ScheduleUpdate().Complete();

            if (clientDriver.GetRelayConnectionStatus() == RelayConnectionStatus.AllocationInvalid)
            {
                Debug.LogWarning("[UnityRelayTransport] Relay allocation became invalid on client. Disconnecting.");
                clientActive = false;
                clientConnecting = false;
                NotifyClientDisconnected();
                DisposeClientDriver();
                return;
            }

            if (clientConnecting && !clientActive && Time.realtimeSinceStartup - clientConnectStartedTime > clientConnectTimeoutSeconds)
            {
                Debug.LogWarning("[UnityRelayTransport] Client relay connection timed out.");
                clientConnecting = false;
                ClientDisconnect();
                return;
            }

            NetworkEvent.Type evt;
            while ((evt = clientDriver.PopEvent(out NetworkConnection conn, out Unity.Collections.DataStreamReader reader)) != NetworkEvent.Type.Empty)
            {
                switch (evt)
                {
                    case NetworkEvent.Type.Connect:
                        Debug.Log("[UnityRelayTransport] *** CLIENT CONNECT EVENT! ***");
                        clientConnection = conn;
                        clientActive = true;
                        clientConnecting = false;
                        OnClientConnected?.Invoke();
                        break;

                    case NetworkEvent.Type.Data:
                        byte[] data = new byte[reader.Length];
                        reader.ReadBytes(data);
                        OnClientDataReceived?.Invoke(new ArraySegment<byte>(data), Channels.Reliable);
                        break;

                    case NetworkEvent.Type.Disconnect:
                        clientActive = false;
                        clientConnecting = false;
                        NotifyClientDisconnected();
                        break;
                }
            }
        }

        public override void ClientLateUpdate()
        {
            if (!clientDriver.IsCreated || !clientActive) return;

            while (clientSendQueue.TryDequeue(out byte[] data))
            {
                if (clientDriver.BeginSend(clientReliablePipeline, clientConnection, out Unity.Collections.DataStreamWriter writer) == 0)
                {
                    writer.WriteBytes(data);
                    clientDriver.EndSend(writer);
                }
            }
        }

        public override void ServerEarlyUpdate()
        {
            if (!serverActive || !serverDriver.IsCreated) return;

            serverDriver.ScheduleUpdate().Complete();

            if (serverDriver.GetRelayConnectionStatus() == RelayConnectionStatus.AllocationInvalid)
            {
                Debug.LogWarning("[UnityRelayTransport] Relay allocation became invalid on server. Disconnecting clients.");
                DisconnectAllServerConnections();
                serverActive = false;
                DisposeServerDriver();
                return;
            }

            NetworkConnection acceptedConnection;
            while ((acceptedConnection = serverDriver.Accept()).IsCreated)
            {
                Debug.Log("[UnityRelayTransport] *** SERVER ACCEPT EVENT! ***");
                int newId = nextConnectionId++;
                connections[newId] = acceptedConnection;
                OnServerConnectedWithAddress?.Invoke(newId, "relay");
            }

            NetworkEvent.Type evt;
            while ((evt = serverDriver.PopEvent(out NetworkConnection conn, out Unity.Collections.DataStreamReader reader)) != NetworkEvent.Type.Empty)
            {
                switch (evt)
                {
                    case NetworkEvent.Type.Connect:
                        Debug.Log("[UnityRelayTransport] Ignored server connect event after Accept().");
                        break;

                    case NetworkEvent.Type.Data:
                        int id = FindConnectionId(conn);
                        if (id >= 0)
                        {
                            byte[] data = new byte[reader.Length];
                            reader.ReadBytes(data);
                            OnServerDataReceived?.Invoke(id, new ArraySegment<byte>(data), Channels.Reliable);
                        }
                        break;

                    case NetworkEvent.Type.Disconnect:
                        int discId = FindConnectionId(conn);
                        if (discId >= 0)
                        {
                            connections.Remove(discId);
                            OnServerDisconnected?.Invoke(discId);
                        }
                        break;
                }
            }
        }

        public override void ServerLateUpdate()
        {
            if (!serverActive || !serverDriver.IsCreated) return;

            while (serverSendQueue.TryDequeue(out ServerSendMessage msg))
            {
                if (connections.TryGetValue(msg.connectionId, out NetworkConnection conn))
                {
                    if (serverDriver.BeginSend(serverReliablePipeline, conn, out Unity.Collections.DataStreamWriter writer) == 0)
                    {
                        writer.WriteBytes(msg.data);
                        serverDriver.EndSend(writer);
                    }
                }
            }
        }

        private int FindConnectionId(NetworkConnection conn)
        {
            foreach (var kvp in connections)
            {
                if (kvp.Value.Equals(conn))
                    return kvp.Key;
            }
            return -1;
        }

        private void ApplyRelaySettings(ref NetworkSettings settings, ref RelayServerData relayData)
        {
            settings.WithNetworkConfigParameters(
                maxFrameTimeMS: Mathf.Max(0, maxFrameTimeMs),
                maxMessageSize: maxPacketSize);
            settings.WithRelayParameters(
                ref relayData,
                Mathf.Clamp(relayHeartbeatIntervalMs, 1, 9000));
        }

        private void DisconnectAllServerConnections()
        {
            foreach (int connectionId in new List<int>(connections.Keys))
            {
                OnServerDisconnected?.Invoke(connectionId);
            }

            connections.Clear();
            while (serverSendQueue.TryDequeue(out _)) { }
        }

        private void DisposeClientDriver()
        {
            if (clientDriver.IsCreated)
                clientDriver.Dispose();

            clientConnection = default;
            clientActive = false;
            clientConnecting = false;
            while (clientSendQueue.TryDequeue(out _)) { }
        }

        private void DisposeServerDriver()
        {
            if (serverDriver.IsCreated)
                serverDriver.Dispose();

            serverActive = false;
            connections.Clear();
            nextConnectionId = 1;
            while (serverSendQueue.TryDequeue(out _)) { }
        }

        private void NotifyClientDisconnected()
        {
            if (clientDisconnectNotified) return;

            clientDisconnectNotified = true;
            OnClientDisconnected?.Invoke();
        }
    }
}
