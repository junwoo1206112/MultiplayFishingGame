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
using NetworkConnection = Unity.Networking.Transport.NetworkConnection;

namespace MultiplayFishing.Network
{
    public class UnityRelayTransport : Transport
    {
        [Header("Transport Configuration")]
        public int maxPacketSize = 1400;

        private NetworkDriver serverDriver;
        private NetworkDriver clientDriver;


        private bool serverActive;
        private Dictionary<int, NetworkConnection> connections = new Dictionary<int, NetworkConnection>();
        private int nextConnectionId = 1;

        private bool clientActive;
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
            if (string.IsNullOrEmpty(pendingJoinCode))
            {
                Debug.LogError("[UnityRelayTransport] No join code. Call PrepareRelayJoin() first.");
                return;
            }
            _ = ConnectRelayAsync();
        }

        private async Task ConnectRelayAsync()
        {
            try
            {
                await InitServices();
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(pendingJoinCode);
                pendingJoinCode = null;

                var relayData = joinAllocation.ToRelayServerData("dtls");
                var settings = new NetworkSettings();
                settings.WithRelayParameters(ref relayData, maxPacketSize);

                clientDriver = NetworkDriver.Create(settings);
                clientConnection = clientDriver.Connect(NetworkEndpoint.AnyIpv4);
                Debug.Log($"[UnityRelayTransport] Client connecting... IsCreated={clientConnection.IsCreated}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityRelayTransport] Relay connect failed: {e.Message}");
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
                settings.WithRelayParameters(ref relayData, maxPacketSize);

                serverDriver = NetworkDriver.Create(settings);
                int bindResult = serverDriver.Bind(NetworkEndpoint.AnyIpv4);
                if (bindResult != 0)
                {
                    Debug.LogError($"[UnityRelayTransport] Bind failed: {bindResult}");
                    return;
                }
                int listenResult = serverDriver.Listen();
                if (listenResult != 0)
                {
                    Debug.LogError($"[UnityRelayTransport] Listen failed: {listenResult}");
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
            serverActive = false;
            connections.Clear();
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

            NetworkEvent.Type evt;
            while ((evt = clientDriver.PopEvent(out NetworkConnection conn, out Unity.Collections.DataStreamReader reader)) != NetworkEvent.Type.Empty)
            {
                switch (evt)
                {
                    case NetworkEvent.Type.Connect:
                        Debug.Log("[UnityRelayTransport] *** CLIENT CONNECT EVENT! ***");
                        clientConnection = conn;
                        clientActive = true;
                        OnClientConnected?.Invoke();
                        break;

                    case NetworkEvent.Type.Data:
                        byte[] data = new byte[reader.Length];
                        reader.ReadBytes(data);
                        OnClientDataReceived?.Invoke(new ArraySegment<byte>(data), Channels.Reliable);
                        break;

                    case NetworkEvent.Type.Disconnect:
                        clientActive = false;
                        OnClientDisconnected?.Invoke();
                        break;
                }
            }
        }

        public override void ClientLateUpdate()
        {
            if (!clientDriver.IsCreated || !clientActive) return;

            while (clientSendQueue.TryDequeue(out byte[] data))
            {
                if (clientDriver.BeginSend(NetworkPipeline.Null, clientConnection, out Unity.Collections.DataStreamWriter writer) == 0)
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

            NetworkEvent.Type evt;
            while ((evt = serverDriver.PopEvent(out NetworkConnection conn, out Unity.Collections.DataStreamReader reader)) != NetworkEvent.Type.Empty)
            {
                switch (evt)
                {
                    case NetworkEvent.Type.Connect:
                        Debug.Log("[UnityRelayTransport] *** SERVER CONNECT EVENT! ***");
                        int newId = nextConnectionId++;
                        connections[newId] = conn;
                        OnServerConnectedWithAddress?.Invoke(newId, "relay");
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
                    if (serverDriver.BeginSend(NetworkPipeline.Null, conn, out Unity.Collections.DataStreamWriter writer) == 0)
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
    }
}
