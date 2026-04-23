# Tasks: Fix UI Panel and Player Name Display

## Task 1: Fix Online UI Panel Not Showing
- [x] Modify `NetworkMenuUI.cs`:
  - Add `SceneManager.sceneLoaded` event subscription in `OnEnable`
  - Add `OnSceneLoaded` callback that calls `Refresh()` after PlayScene loads
  - Ensure `Refresh()` re-finds manager if null using `FindAnyObjectByType`
  - Force `Refresh()` call in `Start()` after small delay (coroutine)

## Task 2: Fix Player Name Display References
- [x] Modify `PlayerNameDisplay.cs`:
  - Replace `[SerializeField] player` with `GetComponentInParent<FishingPlayer>()`
  - Replace `[SerializeField] nameText` with `GetComponentInChildren<TMP_Text>()`
  - In `OnEnable()`, immediately call `UpdateName(player.playerName)` after subscribing
  - Add null safety checks for all references

## Task 3: Ensure Canvas Camera Assignment
- [x] Modify `PlayerNameDisplay.cs`:
  - In `Start()`, find parent Canvas and set `canvas.worldCamera = Camera.main`
  - Add null check before assignment
  - Log warning if camera not found

## Task 4: Strengthen FishingPlayer Name Events
- [x] Modify `FishingPlayer.cs`:
  - In `OnStartClient()`, call `OnPlayerNameChangedEvent?.Invoke(playerName)` to ensure late-subscribers get current value
  - Ensure `CmdUpdatePlayerName` validates connection authority

## Task 5: Test and Verify
- [ ] Test Lobby → PlayScene transition
- [ ] Verify online panel (IP copy, disconnect) appears
- [ ] Verify player name displays above head
- [ ] Verify name changes when entering new name in Lobby
