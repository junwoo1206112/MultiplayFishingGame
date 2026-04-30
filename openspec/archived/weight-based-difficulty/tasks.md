# Tasks: Weight-based Difficulty

## 1. Data Model
- [x] Add `weight` field to `FishDataSO.cs`.

## 2. Editor Tools
- [x] Update `ExcelDataConverter.PatchCreativeContent` to add Weight column and generate values.
- [x] Update `ExcelDataConverter.ConvertExcelToSO` to sync Weight field.

## 3. Gameplay Logic
- [x] Update `FishingPlayer.cs` to use weight-based calculation in `GetRequiredSpam`.

## 4. User Action Required
- [ ] Run `Tools > Excel > 1. Patch Creative Content` in Unity.
- [ ] Run `Tools > Excel > 2. Convert Excel to SO Assets` in Unity.
