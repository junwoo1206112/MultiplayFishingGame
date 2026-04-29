# Design: Fix Unity Errors and Conflicts

## Conflict Resolution Strategy
- **Player.prefab**: Accept `origin/Map` for the `SampleSceneLocalPlayerController` header (as it's a new component) and accept `HEAD` for the `FishingPlayer` speed fields.
- **PlayScene.unity**: Accept `HEAD` changes for `SprintStatusText` UI elements. These are added as children of the UI Canvas and registered in the `m_AddedGameObjects` list of the prefab instance.

## Asset Cleanup
- **Fonts**: Deleted `Assets/Art/Fonts/나눔 글꼴/나눔바른펜` and `Assets/NanumGothicEcoExtraBold.ttf` as they are reported as unsupported formats and are likely redundant or corrupted.
- **Animations**: Deleted `Fishing_Cast.backup.anim` as it is a mismatched backup file.

## Manual Steps
- **Lighting**: The `LightingData` asset is incompatible with the current Unity version. This requires a manual bake in the Unity Editor via `Window > Rendering > Lighting > Generate Lighting`.
- **Input System**: Note that the project is using the deprecated Input Manager. Migration to the new Input System is recommended but out of scope for this fix.
