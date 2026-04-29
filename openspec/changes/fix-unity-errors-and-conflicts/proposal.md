# Proposal: Fix Unity Errors and Conflicts

## Problem
The project currently has several critical and non-critical errors reported by Unity:
1. **Merge Conflicts**: `Assets/Prefabs/Player/Player.prefab` and `Assets/Scenes/PlayScene.unity` contain git conflict markers, preventing Unity from loading them correctly.
2. **Font Import Errors**: Several Nanum fonts are in an unsupported format, causing console errors.
3. **Asset Name Mismatch**: `Fishing_Cast.backup.anim` has an internal name mismatch.
4. **Mesh/Lighting Issues**: Warnings about self-intersecting meshes and incompatible lighting data.

## Proposed Solution
- Resolve all merge conflicts by prioritizing the new Sprint feature additions from `HEAD` while ensuring core functionality from `origin/Map` is preserved.
- Clean up problematic assets:
    - Remove unsupported font files.
    - Remove mismatched backup animation files.
- Document remaining manual steps (e.g., Lighting rebake).

## Expected Outcome
- Unity console should be clear of critical import and conflict errors.
- The `PlayScene` and `Player` prefab should be fully functional with the latest Sprint features.
