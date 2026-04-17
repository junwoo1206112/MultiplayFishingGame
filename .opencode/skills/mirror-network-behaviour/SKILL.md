---
name: mirror-network-behaviour
description: Create a new Mirror NetworkBehaviour script following project conventions. Use when you need to create a networked game object component with Commands, ClientRpcs, SyncVars, or server/client lifecycle hooks.
license: MIT
metadata:
  audience: developers
  workflow: unity-mirror
  domain: networking
---

Create a new Mirror NetworkBehaviour script for the multiplayer fishing game.

## When to Use This Skill

Use this skill when creating any networked game component that needs:
- Server-authoritative state ([SyncVar], [Command])
- Client-side visual updates ([ClientRpc], [TargetRpc])
- Network lifecycle hooks (OnStartServer, OnStartClient, etc.)
- Ownership-based synchronization (syncDirection)

## Steps

1. **Determine the namespace and folder**
   - `Scripts/Network/` → `MultiplayFishing.Network`
   - `Scripts/Gameplay/` → `MultiplayFishing.Gameplay`
   - `Scripts/UI/` → `MultiplayFishing.UI`
   - `Scripts/Managers/` → `MultiplayFishing.Core`

2. **Determine authority direction**
   - Server authoritative (default): Server owns the state, clients receive updates
   - Client authoritative: Owner client controls state, server broadcasts to others
   - Ask the user if unclear, or infer from the game design

3. **Create the script file** following this template:

```
using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MultiplayFishing.Gameplay
{
    public class MyClass : NetworkBehaviour
    {
        #region Server

        [ServerCallback]
        void Update()
        {
            // 서버 전용 로직
        }

        [Command]
        void CmdMyAction()
        {
            // 클라이언트→서버 요청 처리
        }

        #endregion

        #region Client

        [ClientRpc]
        void RpcOnSomethingHappened()
        {
            // 서버→모든 클라이언트 알림
        }

        void OnStartClient()
        {
            // 클라이언트 초기화
        }

        #endregion
    }
}
```

4. **Add SyncVars with hooks** (if needed):

```csharp
[SyncVar(hook = nameof(OnHealthChanged))]
public int health = 100;

void OnHealthChanged(int oldValue, int newValue)
{
    // 클라이언트 측 UI 업데이트
}
```

5. **Add inspector fields** with proper attributes:

```csharp
[Header("Settings")]
[SerializeField] private int maxHealth = 100;

[Header("References")]
[SerializeField] private Animator animator;
```

## Rules

- **Never modify files in `Assets/Mirror/`**
- Filename must match class name exactly: `MyClass.cs`
- Use `[Command]` with `Cmd` prefix, `[ClientRpc]` with `Rpc` prefix
- SyncVar hooks use `nameof()`: `[SyncVar(hook = nameof(OnHealthChanged))]`
- Hook methods take `(oldValue, newValue)` parameters
- Use `#region Server` / `#region Client` to organize code
- Private fields: camelCase without underscore prefix (`lastSendTime`)
- Public inspector fields: camelCase (`playerPrefab`)
- Use `[Server]` / `[ServerCallback]` for server-only methods
- Use `[Client]` / `[ClientCallback]` for client-only methods
- Validate all Command/Rpc parameters on the server side
- Use `Debug.Log($"[ClassName] message")` for logging with class name prefix
- Prefer guard clauses over try-catch for runtime code