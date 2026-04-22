## Context

Detailed analysis of `SampleSceneLocalPlayerController.cs` (lines 290-298) reveals that a constant downward force is applied when the character is grounded:
```csharp
if (characterController.isGrounded && velocity.y < 0f)
{
    velocity.y = groundedVelocity; // -2f
}
velocity.y += gravity * Time.deltaTime; // -20f * Time.deltaTime
```
This causes the `CharacterController` to be constantly pushed into the ground, leading to visual clipping (burying) during animation states.

## Goals / Non-Goals

**Goals:**
- Stabilize the grounded state by reducing downward force.
- Prevent the animation from causing vertical sinking by forcing "Bake Into Pose (Y)".

## Decisions

### 1. Reduce Grounded Downward Force
**Decision:** Change `groundedVelocity` to `-0.5f` in `SampleSceneLocalPlayerController.cs` and skip further gravity accumulation while grounded.
- **Rationale:** A force of -0.5f is sufficient for stable ground detection (slope climbing) without causing excessive penetration of the ground mesh.

### 2. Standardize Collider Alignment in Prefab
**Decision:** CharacterController (Height 2, Center 1), CapsuleCollider (Height 1.878, Center 0.939, Disabled).
- **Rationale:** Ensures all collision bottoms match the model pivot at Y=0.

### 3. Force "Bake Into Pose" for Walking Animation
**Decision:** In `Walking.fbx.meta`, set `keepOriginalPositionY: 0` and `heightFromFeet: 1`.
- **Rationale:** Forcing "Based Upon Feet" ensures the animation clip doesn't have an internal offset that moves the model relative to its root transform.

## Risks / Trade-offs

- **[Risk]** Reduced grounded force might make the player float slightly on steep slopes.
  - **Mitigation:** Testing the -0.5f value on various terrains to find a balance.
