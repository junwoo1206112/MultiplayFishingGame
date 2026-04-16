---
name: bug-fix
description: Fix bugs in the multiplayer fishing game following project conventions and Mirror networking constraints. Use for quick fixes (typos, 1-2 line changes) or structured fixes that may need an OpenSpec proposal.
license: MIT
metadata:
  audience: developers
  workflow: bug-fix
  domain: general
---

Fix bugs following project conventions. Decide whether a quick fix or an OpenSpec proposal is needed.

## Workflow

### Step 1: Assess the Bug

Determine severity and complexity:

| Type | Example | Approach |
|------|---------|----------|
| **Quick fix** | Typo, off-by-one, null check missing | Fix directly, no OpenSpec needed |
| **Structural fix** | Race condition, desync, architecture issue | Use `/opsx:propose` first |

### Step 2: Quick Fix (for simple bugs)

1. Identify the bug location
2. Fix following project conventions:
   - Correct namespace for the folder
   - Allman brace style, 4-space indent
   - camelCase for private fields (no underscore)
   - `Debug.LogError/LogWarning/Log` with `[ClassName]` prefix
   - Guard clauses over try-catch
3. Test mentally: does this break any network synchronization?

### Step 3: Mirror-Aware Debugging

When the bug involves networking, check these common issues:

**SyncVar not updating on client:**
- Is `[SyncVar]` attribute present?
- Is the hook method signature correct: `(oldValue, newValue)`?
- Is the value being changed on the server (not client)?

**Command not reaching server:**
- Does the method have `[Command]` and `Cmd` prefix?
- Is `isLocalPlayer` or `isOwned` true before calling?
- Are parameters serializable by Mirror?

**ClientRpc not firing:**
- Does the method have `[ClientRpc]` and `Rpc` prefix?
- Is the object spawned on the client (not just server)?
- Is it being called on the server side?

**Object not spawning:**
- Is the prefab registered in NetworkManager.spawnPrefabs?
- Is `NetworkServer.Spawn()` called only on server?
- Does the prefab have a NetworkIdentity component?

**Desync issues:**
- Are you modifying SyncVars on the client without authority?
- Is `syncDirection` set correctly (ServerToClient vs ClientToServer)?
- Are SyncObject collections being modified only on the server?

### Step 4: Fix with Mirror Constraints

```csharp
// 나쁨: 클라이언트에서 SyncVar 수정
public void UpdateScore(int newScore)
{
    score = newScore; // 서버 전용이어야 함
}

// 좋음: Command로 서버에 요청
[Command]
void CmdUpdateScore(int newScore)
{
    // 서버에서 검증
    if (newScore < 0 || newScore > MaxScore) return;
    score = newScore; // SyncVar는 서버에서 수정
}
```

### Step 5: Logging Pattern

Always add diagnostic logging when fixing bugs:

```csharp
Debug.Log($"[ClassName] 상태 변경: {oldValue} → {newValue}");
Debug.LogWarning($"[ClassName] 예상치 못한 상태: {currentState}");
Debug.LogError($"[ClassName] 치명적 오류: {description}");
```

## When to Use OpenSpec

Use `/opsx:propose` instead of a direct fix when:
- The fix affects multiple systems
- The bug is actually a design problem
- Adding a new feature to fix a limitation
- The fix might introduce regressions
- Multiple stakeholders need to agree on the approach

## Rules

- Never modify files in `Assets/Mirror/`
- Never use `GameObject.Find()` — use references, singletons, or `FindFirstObjectByType<T>()`
- Never modify `.meta` files by hand
- All Command parameters must be validated on the server
- SyncVars can only be modified on the side with authority
- Use guard clauses over try-catch for runtime code
- Log with `[ClassName]` prefix using string interpolation