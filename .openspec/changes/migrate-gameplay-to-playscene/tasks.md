# Tasks: Migrate gameplay objects to PlayScene

## Context
PlayScene has the map/terrain but is missing gameplay infrastructure. Need to add 5 objects from GamePlay.unity and Lobby.unity.

## Tasks

- [x] 1. Add PlayerSpawnPosition PrefabInstance to PlayScene
- [x] 2. Add PlayerVcam PrefabInstance to PlayScene
- [x] 3. Add EventSystem inline object to PlayScene
- [x] 4. Add Main Camera with CinemachineBrain to PlayScene (replace existing)
- [ ] 5. Add NetworkManager PrefabInstance to PlayScene
- [ ] 6. Update PlayScene SceneRoots with all new objects
- [ ] 7. Fix NetworkManager onlineScene to point to PlayScene
- [ ] 8. Update EditorBuildSettings to include PlayScene