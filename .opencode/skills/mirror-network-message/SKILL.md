---
name: mirror-network-message
description: Create Mirror NetworkMessage structs and register message handlers for client-server communication. Use when adding custom network messages for systems that don't need SyncVar synchronization.
license: MIT
metadata:
  audience: developers
  workflow: unity-mirror
  domain: networking
---

Create NetworkMessage structs and register handlers for client-server communication beyond what SyncVars provide.

## When to Use This Skill

Use this skill when you need to:
- Send one-off messages between client and server (not continuously synced state)
- Implement custom authentication flows
- Create system-level messages (matchmaking, chat, trade, etc.)
- Send data that doesn't need continuous synchronization

Do NOT use this for:
- Continuously synced state → use `[SyncVar]` instead
- Server-to-clients visual updates → use `[ClientRpc]` instead
- Client-to-server requests → use `[Command]` instead

## Steps

1. **Determine message direction**
   - Client → Server (e.g., chat message, action request)
   - Server → Client (e.g., server announcement, match result)
   - Server → Specific Client (e.g., personal notification)
   - Bidirectional (e.g., trade system)

2. **Create the message struct** in the appropriate namespace folder:

```csharp
using Mirror;
using UnityEngine;

namespace MultiplayFishing.Network
{
    public struct CastMessage : NetworkMessage
    {
        public uint netId;
        public Vector3 position;
        public float power;
    }
}
```

**Naming convention**: PascalCase + `Message` suffix (e.g., `CastMessage`, `ChatMessage`, `TradeRequestMessage`)

3. **Register message handlers** — typically in a NetworkManager subclass or a dedicated system:

**Server-side (receiving from clients):**
```csharp
void OnStartServer()
{
    NetworkServer.RegisterHandler<CastMessage>(OnCastMessage);
}

void OnCastMessage(NetworkConnectionToClient conn, CastMessage msg)
{
    // 항상 메시지 검증
    if (msg.power < 0 || msg.power > 100)
    {
        Debug.LogWarning("[FishingSystem] 잘못된 power 값 수신");
        return;
    }

    // 메시지 처리
}
```

**Client-side (receiving from server):**
```csharp
void OnStartClient()
{
    NetworkClient.RegisterHandler<FishCaughtMessage>(OnFishCaughtMessage);
}

void OnFishCaughtMessage(FishCaughtMessage msg)
{
    // 클라이언트 측 처리
}
```

4. **Send messages:**

**Client → Server:**
```csharp
NetworkClient.Send(new CastMessage
{
    netId = netIdentity.netId,
    position = transform.position,
    power = calculatedPower
});
```

**Server → All Clients:**
```csharp
NetworkServer.SendToReady(new FishCaughtMessage
{
    fishName = caughtFish.name,
    score = caughtFish.score
});
```

**Server → Specific Client:**
```csharp
conn.Send(new TradeOfferMessage
{
    offerId = tradeId,
    itemName = item.name
});
```

5. **Unregister on cleanup:**
```csharp
void OnStopServer()
{
    NetworkServer.UnregisterHandler<CastMessage>();
}

void OnStopClient()
{
    NetworkClient.UnregisterHandler<FishCaughtMessage>();
}
```

## Rules

- All NetworkMessage structs are `public struct` implementing `NetworkMessage`
- Filename must match struct name: `CastMessage.cs`
- Place in `Scripts/Network/` with namespace `MultiplayFishing.Network`
- Use PascalCase + `Message` suffix for naming
- Always validate incoming messages on the server side (guard clauses, range checks)
- Unregister handlers in `OnStopServer()` / `OnStopClient()` to prevent leaks
- For continuously synced state, prefer `[SyncVar]` over messages
- Use `Debug.Log($"[ClassName] ...")` for all log messages