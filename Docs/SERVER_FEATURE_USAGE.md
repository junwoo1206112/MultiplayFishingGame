# Server Feature Usage

Updated: 2026-05-09

This document records the server/network features currently used by the project after the Unity Relay debugging pass.

## Active Server/Network Features

| Area | Feature Used | Where | Notes |
|------|--------------|-------|-------|
| Unity Gaming Services | Unity Relay allocation | `UnityRelayTransport.CreateRelayAllocation()` | Host creates an allocation and receives a join code. |
| Unity Gaming Services | Unity Relay join code | `UnityRelayTransport.JoinCode`, `FishingRoomManager.CurrentJoinCode` | Host shares the code; participant enters it in the join input. |
| Unity Gaming Services | Unity Relay join allocation | `UnityRelayTransport.ConnectRelayAsync()` | Participant joins the host allocation with `RelayService.Instance.JoinAllocationAsync()`. |
| Unity Gaming Services | Anonymous Authentication | `UnityRelayTransport.InitServices()` | Both host and participant sign in anonymously before Relay calls. |
| Unity Gaming Services | QoS region selection | Unity Relay SDK internals | `CreateAllocationAsync(..., region: null)` lets UGS select a region using QoS. |
| Unity Transport | Relay server data | `ToRelayServerData("dtls")` | Relay uses UTP Relay data with DTLS protocol. |
| Unity Transport | Reliable pipeline | `ReliableSequencedPipelineStage` | Mirror messages are sent through reliable UTP pipelines, not `NetworkPipeline.Null`. |
| Mirror | Server-authoritative host mode | `FishingRoomManager.StartHost()` | Host runs server and local client together. |
| Mirror | Client connection | `FishingRoomManager.StartClient()` | Participant starts a Mirror client after Relay join code preparation. |
| Mirror | Player spawn | `FishingRoomManager.OnServerAddPlayer()` | Server instantiates and attaches the player prefab to each connection. |
| Mirror | Disconnect/offline scene | `NetworkMenuUI.OnDisconnectClicked()`, `UnityRelayTransport.NotifyClientDisconnected()` | Disconnect now notifies Mirror so `offlineScene` can load back to `Assets/Scenes/Lobby.unity`. |
| Editor tooling | UnityCliConnector HTTP server | `Assets/Editor/UnityCliConnector/HttpServer.cs` | Editor-only local tooling server, not a gameplay server. |

## Features Not Currently Used

| Feature | Status |
|---------|--------|
| Unity Lobby Service | Not used. Join is direct by Relay join code. |
| Dedicated server build | Not used. Current flow is Mirror host mode. |
| Edgegap Relay/Lobby | Not active in current flow. Older setup docs exist, but the active transport is `UnityRelayTransport`. |
| LAN IP join | Replaced by Relay join code for the current UI flow. |
| Matchmaking | Not implemented. |
| Room list / lobby browser | Not implemented. |

## Current Connection Flow

### Host
1. Player clicks the host button.
2. `FishingRoomManager.CreateRoom()` calls `UnityRelayTransport.CreateRelayAllocation(maxConnections)`.
3. `UnityServices.InitializeAsync()` runs if needed.
4. `AuthenticationService.Instance.SignInAnonymouslyAsync()` signs in if needed.
5. `RelayService.Instance.CreateAllocationAsync(maxPlayers, region: null)` creates the Relay allocation.
6. `RelayService.Instance.GetJoinCodeAsync()` returns a join code.
7. `FishingRoomManager.StartHost()` starts Mirror host mode.
8. `UnityRelayTransport.ServerStart()` creates a UTP Relay driver, reliable pipeline, binds, and listens.

### Participant
1. Player enters the host join code and clicks the join button.
2. `FishingRoomManager.JoinRoom()` calls `UnityRelayTransport.PrepareRelayJoin(joinCode)`.
3. `FishingRoomManager.StartClient()` starts the Mirror client.
4. `UnityRelayTransport.ConnectRelayAsync()` signs in, calls `JoinAllocationAsync()`, creates UTP Relay data, and calls `clientDriver.Connect()`.
5. Client receives `CLIENT CONNECT EVENT`.
6. Server calls `serverDriver.Accept()` before processing data, then invokes Mirror server connection callbacks.
7. Mirror sends ready/add-player messages; server runs `OnServerAddPlayer()`.

### Disconnect
1. Participant clicks the disconnect button.
2. `NetworkMenuUI.OnDisconnectClicked()` calls `StopClient()` while `NetworkClient.active` is true.
3. Mirror calls `UnityRelayTransport.ClientDisconnect()`.
4. `UnityRelayTransport.NotifyClientDisconnected()` invokes Mirror's disconnect callback exactly once.
5. Mirror performs client shutdown and loads `offlineScene` (`Assets/Scenes/Lobby.unity`).

## Fixes Applied During Relay Debugging

- Changed Relay client connect from `clientDriver.Connect(NetworkEndpoint.AnyIpv4)` to `clientDriver.Connect()` so UTP uses Relay server data.
- Added reliable UTP pipelines for Mirror payloads.
- Added client connection timeout handling so the UI does not stay in "server connecting" forever.
- Added server-side `serverDriver.Accept()` before data processing to avoid discarded data before accept.
- Added guarded client disconnect notification so Mirror can clean up and return to the offline scene.
- Updated disconnect UI handling to call `StopClient()` even while the client is active but not fully connected.

## Validation

- `dotnet build MultiplayFishing.Network.csproj --no-restore` passed.
- `dotnet build MultiplayFishing.UI.csproj --no-restore` passed.
- Unity Editor runtime still needs manual host/join/disconnect verification because Relay requires live Unity Services.
