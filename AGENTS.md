# MultiplayFishingGame — Agentic Coding Guide

## Project Overview

- **Unity 6** (6000.3.10f1) multiplayer fishing game
- **C# 9.0, netstandard2.1**, 6 assembly definitions (Data/Core/Network/Gameplay/UI/Editor)
- **Mirror Networking v96.0.1** (server-authoritative), **URP**, **New Input System**, **Cinemachine**, **TextMeshPro**
- **OpenSpec-driven development**: changes live in `openspec/changes/` with `proposal.md`, `design.md`, `tasks.md`
- **MCP server** at `http://127.0.0.1:8080/mcp`
- **OpenCode skills**: `.opencode/skills/` — 7 skills for Mirror, OpenSpec, and project conventions (auto-loaded by OpenCode)
- **AGENTS.md**: This file is auto-loaded by OpenCode every session. Use it as the primary project guide.

## ⚠️ Shop System — UI 프리팹 셋업 필요 (2026-05-08)

**상점 시스템(Shop System) 코드는 완료되었지만, Unity Editor에서 아래 작업이 필요합니다.**
👉 **`UI_SETUP_GUIDE.md`** 파일을 반드시 읽고 진행하세요.

| 단계 | 작업 | 담당 |
|------|------|------|
| 3 | ShopSlot/ShopInventorySlot/ConfirmDialog 프리팹 생성 | UI 담당 |
| 4 | ShopUI를 Dynamic UI Canvas에 배치 + SerializeField 연결 | UI 담당 |
| 5 | B키 토글 확인, 구매/판매/장착 테스트 | UI 담당 |

---

## Build / Lint / Test

- **No npm/CI/tests exist.** This is a pure Unity project — build is done inside the Unity Editor.
- **No `.editorconfig`, no `.stylecop`**, no static analysis config files.
- **Assembly definitions** (`*.asmdef`) in each `Scripts/*/` folder control dependencies and compilation.
- To validate code: open in Unity Editor and check Console for compilation errors.
- Solution file: `MultiplayFishingGame.slnx` (open in IDE for full project navigation).

## Folder → Namespace Mapping

| Folder | Namespace |
|--------|-----------|
| `Assets/Scripts/Network/` | `MultiplayFishing.Network` |
| `Assets/Scripts/Gameplay/` | `MultiplayFishing.Gameplay` |
| `Assets/Scripts/Data/` | `MultiplayFishing.Data` |
| `Assets/Scripts/Data/Models/` | `MultiplayFishing.Data.Models` |
| `Assets/Scripts/UI/` | `MultiplayFishing.UI` |
| `Assets/Scripts/Managers/` | `MultiplayFishing.Core` |
| `Assets/Scripts/Utilities/` | `MultiplayFishing.Utilities` |
| `Editor/` | `MultiplayFishing.Editor` |
| `Tests/` | `MultiplayFishing.Tests` |

## Code Style

### Using Statement Order
```
System.* → UnityEngine.* → Mirror → MultiplayFishing.*
```
Example:
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MultiplayFishing.Data.Models;
```

### Naming Conventions
| Element | Rule | Example |
|---------|------|---------|
| Class | PascalCase | `FishingRod` |
| Interface | `I` + PascalCase | `ICatchable` |
| Private field | camelCase, **no underscore** | `lastSendTime` |
| Serialized field | camelCase | `playerPrefab` |
| Public field | PascalCase (or camelCase for SerializeField) | `IsServer`, `playerName` |
| Property | PascalCase | `CurrentState` |
| Method | PascalCase | `OnStartServer()` |
| Parameter | camelCase | `netId` |
| Local variable | camelCase | `float waitTime` |
| Constant | PascalCase | `MaxConnections` |
| NetworkMessage struct | PascalCase + `Message` suffix | `CastMessage` |
| Command method | `Cmd` prefix | `CmdCastRod()` |
| ClientRpc method | `Rpc` prefix | `RpcFishCaught()` |
| TargetRpc method | `Target` prefix | `TargetOnNibble()` |
| SyncVar hook | `On` + PascalCase | `OnPlayerNameChanged` |

### Formatting Rules
- **Allman brace style** (braces on new line), **4-space indentation**, no tabs
- `[SerializeField] private` on same line as field
- `[SyncVar(hook = nameof(...))] public` on same line as field
- File name must match class/struct name exactly
- Use `#region Server` / `#region Client` to organize NetworkBehaviour methods
- Comment style: English for XML docs (`/// <summary>`), Korean for business logic notes

### Error Handling & Defensive Coding
- **Guard clauses with early return** — never try-catch in hot paths
- Null checks before dereference: `if (pendingFish == null) return;`
- Validate all Command/Rpc parameters on the server side before processing
- Use `Debug.Log($"[ClassName] message")` with class name prefix
- Use `Debug.LogWarning` / `Debug.LogError` for warnings and errors

### Lifecycle Method Pattern
```
Awake()      → component references, caching
OnEnable()   → event subscription
Start()      → service resolution (DI), UI init
Update()     → input handling (with isLocalPlayer guard)
OnDisable()  → event unsubscription
OnDestroy()  → cleanup
```

## Mirror Networking Patterns

### Server-Authoritative Model
- Server owns all game state changes; clients receive updates via SyncVar/ClientRpc
- Fishing flow: Client → `[Command]` → Server → `[TargetRpc]` → Specific Client
- Use `[Server]`/`[ServerCallback]` for server-only methods, `[Client]`/`[ClientCallback]` for client-only

### SyncVar Pattern
```csharp
[SyncVar(hook = nameof(OnPlayerNameChanged))]
public string playerName = "";

void OnPlayerNameChanged(string oldValue, string newValue)
    => OnPlayerNameChangedEvent?.Invoke(newValue);
```
- SyncVar hooks use `nameof()` and have signature `void OnChanged(Type old, Type new)`
- SyncVars are `readonly` on clients — only server modifies them

### NetworkMessage Pattern
```csharp
public struct ChannelSelectMessage : NetworkMessage
{
    public string channelId;
}
```
- Place in `Scripts/Network/`, namespace `MultiplayFishing.Network`

### Singleton Pattern (non-Mirror)
```csharp
public static GameManager Instance { get; private set; }

private void Awake()
{
    if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
```

### DI Pattern
```csharp
dataService = DIContainer.Resolve<IDataService>();
userService = DIContainer.Resolve<IUserService>();
```
Simple static `DIContainer` (keyed by `Type`) — register in `GameInitializer`.

## Forbidden Patterns
- `GameObject.Find()` → use references, singletons, or `FindFirstObjectByType<T>()`
- `Update()` for net sync → use `[SyncVar]`, `[ClientRpc]`, SyncObject collections
- Modifying `Assets/Mirror/` → Mirror is read-only
- Manual `.meta` editing → Unity manages these
- Legacy `Input Manager` → use New Input System
- `FindObjectOfType<T>()` → use `FindFirstObjectByType<T>()`
- Try-catch in hot paths → use guard clauses with early return
- Underscore-prefixed private fields → use camelCase without prefix

## OpenSpec Workflow
1. **Explore** → load `openspec-explore` skill to investigate problems and clarify requirements
2. **Propose** → load `openspec-propose` skill to generate `proposal.md`, `design.md`, `tasks.md`
3. **Apply** → load `openspec-apply-change` skill to implement tasks
4. **Archive** → load `openspec-archive-change` skill to finalize and archive the change

## Available Skills (auto-loaded by OpenCode)
| Skill | When to use |
|-------|-------------|
| `project-conventions` | Quick reference for code style, naming, Mirror patterns, folder structure |
| `mirror-network-behaviour` | Creating new Mirror NetworkBehaviour scripts |
| `mirror-network-message` | Creating Mirror NetworkMessage structs |
| `mirror-prefab-registration` | Registering networked prefabs for spawning |
| `mirror-sync-setup` | Setting up SyncVar/SyncObject synchronization |
| `bug-fix` | Fixing bugs following project conventions |
| `openspec-explore` / `openspec-propose` / `openspec-apply-change` / `openspec-archive-change` | OpenSpec 4-step workflow |
