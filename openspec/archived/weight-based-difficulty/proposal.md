# Proposal: Weight-based Fishing Difficulty

## Problem
Currently, fishing difficulty (required spam clicks) is based only on the fish's star rank. This is not granular enough. The user wants heavier fish to be harder to catch and lighter fish to be easier.

## Solution
1.  Add a `weight` field to `FishDataSO`.
2.  Update `ExcelDataConverter` to:
    *   Automatically generate reasonable weights based on fish size and rank during patching.
    *   Read the weight from Excel and populate the ScriptableObject.
3.  Modify `FishingPlayer.cs` to calculate `requiredSpam` based on the fish's weight using a non-linear formula (square root).

## Impact
- **Gameplay**: More dynamic and realistic fishing difficulty.
- **Data**: New `Weight` column in `FishData.xlsx`.
