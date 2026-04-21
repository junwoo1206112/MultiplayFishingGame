# Proposal: Migrate Gameplay to PlayScene

## Problem
PlayScene has the map/terrain from the original scene but is missing all gameplay infrastructure (NetworkManager, PlayerSpawn, Camera, EventSystem). The game cannot function in PlayScene without these components.

## Approach
Copy the essential gameplay PrefabInstances from GamePlay.unity into PlayScene.unity by adding matching PrefabInstance YAML blocks with unique fileIDs.

## Required Objects (from GamePlay.unity)

1. **NetworkManager** (guid: `98bb94192f233ed4e8bb322a1889ccb3`) - Network room system
2. **PlayerVcam** (guid: need to find) - Cinemachine camera follow
3. **CinemachineBrain** - Camera controller on Main Camera
4. **PlayerSpawnPosition** (guid: `e98bb7a8b13fdea43bd972066bd011a5`) - Spawn points
5. **EventSystem** (guid: need to find) - UI input handling

## Alternative (Recommended)
Since text-based scene editing is fragile and error-prone, the safest approach is:
1. Add PrefabInstances via YAML using unique fileIDs
2. Ensure SceneRoots references are correct
3. Test in Unity editor

## Risks
- YAML fileID collisions can corrupt scenes
- Missing script references if prefabs reference scripts not in the project
- GUID conflicts with existing scene objects