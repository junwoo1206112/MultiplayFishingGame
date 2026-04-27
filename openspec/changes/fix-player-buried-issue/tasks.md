## 1. Adjust Player Prefab Settings

- [x] 1.1 Update `CharacterController` Height to 2.0 and Center Y to 1.0.
- [x] 1.2 Update `CapsuleCollider` Height to 1.878 and Center Y to 0.939 and set `m_Enabled` to 0.
- [x] 1.3 Reset character model local Y position to 0.
- [x] 1.4 Correct `Walking.fbx.meta` clip animation settings (mixamo.com) with `keepOriginalPositionY: 0`.

## 2. Script Refactoring

- [ ] 2.1 Update `SampleSceneLocalPlayerController.cs` to use more stable grounded velocity.
- [ ] 2.2 Fix gravity accumulation logic while character is grounded to prevent excessive ground penetration.

## 3. Verification

- [ ] 3.1 Verify character height in SampleScene (LocalPlayer).
- [ ] 3.2 Confirm no ground sinking during walking animation.
