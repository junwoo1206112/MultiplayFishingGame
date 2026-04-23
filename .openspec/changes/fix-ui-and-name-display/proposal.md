# Proposal: Fix UI Panel and Player Name Display

## Problem
Two critical issues exist when transitioning from Lobby to PlayScene:

1. **Online UI Panel Not Showing**: IP Copy and Disconnect buttons don't appear in PlayScene
2. **Player Name Not Displaying**: Player name doesn't show above character head

## Root Causes

### 1. Online UI Panel
- `NetworkMenuUI` relies on `FishingRoomManager.NetworkStateChanged` event
- After scene transition, `Refresh()` may not be called because:
  - `manager` field reference might be stale
  - Event subscription timing issues during scene load
  - `NetworkStateChanged` event may have already fired before UI subscribes

### 2. Player Name Display
- `PlayerNameDisplay` uses `[SerializeField]` references that break when copied between prefabs
- `nameText` and `player` references become null after prefab copy
- `SyncVar` hook fires but UI isn't subscribed yet at spawn time
- World Space Canvas lacks camera reference

## Solution

### UI Panel Fix
- Ensure `NetworkMenuUI` finds manager and refreshes UI on every scene load
- Add `SceneManager.sceneLoaded` callback to force refresh after scene transition
- Use `FindAnyObjectByType` as fallback in `OnEnable`

### Name Display Fix
- Make `PlayerNameDisplay` use `GetComponentInParent/Children` instead of broken `[SerializeField]`
- Initialize name display in `OnEnable` with current SyncVar value
- Ensure Canvas gets camera reference in Start/Awake
- Add defensive null checks throughout

## Scope
- Modify: `NetworkMenuUI.cs`, `PlayerNameDisplay.cs`, `FishingPlayer.cs`
- No new prefabs or scenes needed
- Minimal code changes with maximum reliability improvement
