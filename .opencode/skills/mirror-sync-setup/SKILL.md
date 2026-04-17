---
name: mirror-sync-setup
description: Set up Mirror SyncVar synchronization with hooks, SyncDirection, and SyncObject collections. Use when creating networked state that needs to stay synchronized across server and clients.
license: MIT
metadata:
  audience: developers
  workflow: unity-mirror
  domain: networking
---

Set up state synchronization using Mirror's SyncVar system, SyncDirection, and SyncObject collections.

## When to Use This Skill

Use this skill when you need:
- Continuously synchronized state (health, score, position, etc.)
- Server-authoritative state that all clients see
- Client-authoritative state where the owner controls it
- Synchronized collections (lists, dictionaries, sets)

## SyncVar Patterns

### Basic SyncVar (Server Authoritative)

```csharp
[SyncVar(hook = nameof(OnScoreChanged))]
public int score;

void OnScoreChanged(int oldValue, int newValue)
{
    // 모든 클라이언트에서 호출됨
    OnScoreChangedEvent?.Invoke(newValue);
}

[Header("Events")]
public UnityEvent<int> OnScoreChangedEvent;
```

### Client-Authoritative SyncVar

```csharp
[SyncVar(hook = nameof(OnPositionChanged))]
[Tooltip("Client authoritative: owner client controls this")]
public Vector3 targetPosition;

// syncDirection을 인스펙터에서 ClientToServer로 설정
// 또는 코드에서:
void Awake()
{
    syncDirection = SyncDirection.ClientToServer;
}
```

### Multiple SyncVars

```csharp
[SyncVar(hook = nameof(OnHealthChanged))]
public int health = 100;

[SyncVar(hook = nameof(OnPlayerNameChanged))]
public string playerName = "";

[SyncVar(hook = nameof(OnReadyChanged))]
public bool isReady;

// 각 SyncVar는 고유한 hook 메서드 필요
void OnHealthChanged(int oldValue, int newValue) { }
void OnPlayerNameChanged(string oldValue, string newValue) { }
void OnReadyChanged(bool oldValue, bool newValue) { }
```

## SyncObject Collections

### SyncList

```csharp
// 선언 (readonly, 생성자에서 초기화)
public readonly SyncList<string> playerNames = new SyncList<string>();

// 서버에서만 수정
[Server]
public void AddPlayer(string name)
{
    playerNames.Add(name);
}

// 클라이언트에서 변경 감지
void OnStartClient()
{
    playerNames.Callback += OnPlayerNamesChanged;

    // 초기값 처리
    for (int i = 0; i < playerNames.Count; i++)
    {
        // UI 업데이트
    }
}

void OnPlayerNamesChanged(SyncList<string>.Operation op, int index, string item)
{
    // UI 업데이트
}
```

### SyncDictionary

```csharp
public readonly SyncDictionary<string, int> scores = new SyncDictionary<string, int>();

[Server]
public void UpdateScore(string player, int score)
{
    scores[player] = score;
}
```

### SyncSet

```csharp
public readonly SyncSet<string> tags = new SyncSet<string>();

[Server]
public void AddTag(string tag)
{
    tags.Add(tag);
}
```

## Important Rules

- **SyncVar hooks always take `(oldValue, newValue)` parameters** — both must be present even if unused
- **Use `nameof()` for hook references**: `[SyncVar(hook = nameof(OnScoreChanged))]`
- **SyncObject collections must be `readonly`** and initialized with `new` in declaration
- **Only modify SyncObjects on the server** — client modifications are ignored
- **Register SyncList callbacks in `OnStartClient()`** — not in Awake/Start
- **syncDirection default is ServerToClient** — change to ClientToServer only when the owning client should control the state
- **Don't use `[SyncVar]` on Unity object references** (GameObject, Transform, etc.) — use netId references instead
- **Hook methods are NOT called on the server** — only on clients
- **Hook methods are called when value changes** — not when set to the same value
- **For complex state, prefer SyncObject collections over many SyncVars**