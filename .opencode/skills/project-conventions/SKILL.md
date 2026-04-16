---
name: project-conventions
description: Quick reference for MultiplayFishingGame code style, naming rules, Mirror patterns, and folder structure. Use when creating new scripts, choosing naming, or deciding where to place files.
license: MIT
metadata:
  audience: developers
  workflow: reference
  domain: general
---

Quick reference for project conventions, naming, and Mirror patterns.

## Folder → Namespace Mapping

| Folder | Namespace |
|--------|-----------|
| `Scripts/Network/` | `MultiplayFishing.Network` |
| `Scripts/Gameplay/` | `MultiplayFishing.Gameplay` |
| `Scripts/Data/` | `MultiplayFishing.Data` |
| `Scripts/Data/Models/` | `MultiplayFishing.Data.Models` |
| `Scripts/UI/` | `MultiplayFishing.UI` |
| `Scripts/Managers/` | `MultiplayFishing.Core` |
| `Scripts/Utilities/` | `MultiplayFishing.Utilities` |
| `Editor/` | `MultiplayFishing.Editor` |
| `Tests/` | `MultiplayFishing.Tests` |

## Using Statement Order

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MultiplayFishing.Data.Models;
```

System → UnityEngine → Mirror → Project namespaces

## Naming Conventions

| Element | Rule | Example |
|---------|------|---------|
| Class | PascalCase | `FishingRod` |
| Interface | IPascalCase | `ICatchable` |
| Private field | camelCase (no underscore) | `lastSendTime` |
| Public/Inspector field | camelCase | `playerPrefab` |
| Property | PascalCase | `IsServer` |
| Method | PascalCase | `OnStartServer()` |
| Parameter | camelCase | `netId` |
| Constant | PascalCase | `MaxConnections` |
| Struct | PascalCase | `FishData` |
| Enum | PascalCase + PascalCase values | `SyncMode.Observers` |
| NetworkMessage | PascalCase + `Message` | `CastMessage` |
| Command method | `Cmd` prefix | `CmdCastRod()` |
| ClientRpc method | `Rpc` prefix | `RpcFishCaught()` |
| TargetRpc method | `Rpc` or `Target` prefix | `RpcReelIn()` |

## Mirror Attribute Quick Reference

| Attribute | Prefix | Called By | Runs On |
|-----------|--------|-----------|---------|
| `[Command]` | `Cmd` | Client | Server |
| `[ClientRpc]` | `Rpc` | Server | All Clients |
| `[TargetRpc]` | — | Server | One Client |
| `[SyncVar]` | — | Server (default) | Auto-synced |
| `[Server]` | — | Server only | Logs warning on client |
| `[ServerCallback]` | — | Server only | Silent on client |
| `[Client]` | — | Client only | Logs warning on server |
| `[ClientCallback]` | — | Client only | Silent on server |

SyncVar hooks: `[SyncVar(hook = nameof(OnChanged))]` with signature `void OnChanged(Type old, Type new)`

## File Template

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MultiplayFishing.Gameplay
{
    public class MyClass : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int myValue = 10;

        [SyncVar(hook = nameof(OnValueChanged))]
        public int syncedValue;

        [Header("Events")]
        public UnityEvent<int> OnValueChangedEvent;

        #region Server

        [ServerCallback]
        void Update()
        {
        }

        [Command]
        void CmdDoAction()
        {
        }

        #endregion

        #region Client

        [ClientRpc]
        void RpcNotifyClients()
        {
        }

        void OnValueChanged(int oldValue, int newValue)
        {
            OnValueChangedEvent?.Invoke(newValue);
        }

        #endregion
    }
}
```

## Singleton Pattern

```csharp
public static GameManager Instance { get; private set; }

private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}

private void OnDestroy()
{
    if (Instance == this)
    {
        Instance = null;
    }
}
```

## Movement & Physics System

- **Standard Controller**: Always use Mirror's built-in **`PlayerController (Reliable)`** for player movement.
- **Custom Movement**: Do NOT write custom movement scripts unless specifically required by game mechanics.
- **Camera Follow**: Use the `FishingCameraFollow` helper script to link Cinemachine with the spawned player.

## Forbidden Patterns

- `GameObject.Find()` → use references, singletons, or `FindFirstObjectByType<T>()`
- `Update()` for net sync → use `[SyncVar]`, `[ClientRpc]`, SyncObject collections
- Modify `Assets/Mirror/` → Mirror is read-only
- Manual `.meta` editing → Unity manages these
- Legacy `Input Manager` → use new Input System
- `FindObjectOfType<T>()` → use `FindFirstObjectByType<T>()`
- Try-catch in hot paths → use guard clauses with early return
- Underscore prefix on private fields → use camelCase without prefix