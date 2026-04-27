## ADDED Requirements

### Requirement: Player Collider Ground Alignment
The system MUST ensure that the player's colliders (`CharacterController` and `CapsuleCollider`) are aligned such that their bottom-most point coincides with the character's feet (local Y=0).

#### Scenario: Stationary Alignment
- **WHEN** the player is spawned on a flat horizontal surface
- **THEN** the character's feet MUST be touching the surface exactly, without being buried or floating.

#### Scenario: Movement Alignment
- **WHEN** the player moves using the `CharacterController.Move` method
- **THEN** the character's feet MUST remain aligned with the ground surface during horizontal movement.

### Requirement: Primary Movement Collider
The system MUST use `CharacterController` as the primary collider for movement and ground detection. Any secondary colliders (like `CapsuleCollider`) SHOULD be disabled by default to avoid physics conflicts.

#### Scenario: Collider Conflict Prevention
- **WHEN** both `CharacterController` and `CapsuleCollider` are present on the same GameObject
- **THEN** the `CapsuleCollider` SHOULD be disabled to ensure consistent movement behavior through the `CharacterController`.
