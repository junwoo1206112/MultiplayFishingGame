## Why

The character's feet are buried in the ground during movement. Detailed investigation reveals two primary causes:
1. **Collider Offset**: Incorrect offsets in the `Player.prefab` (CapsuleCollider and CharacterController Center/Height).
2. **Movement Script Logic**: Excessive downward force (`groundedVelocity = -2f` + `gravity = -20f`) in `SampleSceneLocalPlayerController.cs` pushes the character into the ground faster than the physics engine can resolve collisions, especially during walk animations where the model's vertical position might fluctuate.

## What Changes

- **Prefab Update**: Align `CharacterController` and `CapsuleCollider` centers to 1/2 of their height. Disable redundant `CapsuleCollider`.
- **Script Update**: Refactor gravity logic in `SampleSceneLocalPlayerController.cs` to apply a more stable downward force when grounded.
- **Animation Update**: Set `Walking.fbx` (mixamo.com) to Bake Into Pose (Y) based on Feet.

## Capabilities

### New Capabilities
- `player-collision-alignment`: Correct ground alignment by ensuring collider bottoms coincide with the model pivot (feet).
- `player-movement-stability`: Improve vertical stability by stabilizing gravity application on grounded state.

### Modified Capabilities
- (None)

## Impact

- `Assets/Prefabs/Player/Player.prefab`: Collider component settings.
- `Assets/Scripts/Gameplay/SampleSceneLocalPlayerController.cs`: Movement and gravity logic.
- `Assets/Zimni/Fantasy character/animations/Walking.fbx.meta`: Animation import settings.
