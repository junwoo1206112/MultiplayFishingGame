# Design: Fix UI Panel and Player Name Display

## Architecture

```
┌─────────────────────────────────────────────┐
│           NetworkMenuUI (DDOL)               │
│  ├─ Subscribes to NetworkStateChanged       │
│  ├─ Refreshes on scene load                 │
│  └─ Online panel shows IP/Disconnect        │
└─────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────┐
│           FishingPlayer (spawned)            │
│  ├─ SyncVar: playerName                     │
│  ├─ OnStartLocalPlayer → CmdUpdateName     │
│  └─ SyncVar hook → OnPlayerNameChanged     │
└─────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────┐
│         PlayerNameDisplay (auto-ref)        │
│  ├─ Finds FishingPlayer via GetComponent    │
│  ├─ Finds TMP_Text via GetComponentInChildren│
│  ├─ Subscribes to name change event         │
│  └─ Sets Canvas.worldCamera = Camera.main   │
└─────────────────────────────────────────────┘
```

## Key Decisions

1. **Self-Healing References**: Instead of relying on Inspector assignments that break during prefab copy, use `GetComponent*` family methods in `Awake()` and `OnEnable()`

2. **Scene Load Refresh**: Use `SceneManager.sceneLoaded` to force UI refresh after PlayScene loads, ensuring online panel appears

3. **Immediate Name Update**: In `PlayerNameDisplay.OnEnable()`, read current `playerName` value immediately (not just waiting for SyncVar hook) to handle cases where name was set before UI subscribed

4. **Canvas Camera Assignment**: Set `Canvas.worldCamera` in `Start()` to ensure World Space Canvas renders correctly with Cinemachine-controlled camera

## File Changes

- `Assets/Scripts/UI/NetworkMenuUI.cs` - Add scene load callback, strengthen Refresh
- `Assets/Scripts/UI/PlayerNameDisplay.cs` - Replace SerializeField with code-based lookup, add camera assignment
- `Assets/Scripts/Gameplay/FishingPlayer.cs` - Ensure name events fire reliably
