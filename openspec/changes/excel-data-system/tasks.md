## 1. Setup & Environment

- [ ] 1.1 Create `Assets/Plugins/NPOI` folder and prepare for DLL installation.
- [ ] 1.2 Create `Assets/Scripts/Managers/Interfaces` and `Assets/Scripts/Managers/Core` folders.
- [ ] 1.3 Create base data folders: `Assets/ExcelData` and `Assets/Resources/Data/Fish`.

## 2. Core DI Framework & Models

- [ ] 2.1 Implement `FishDataSO` model with specified fields.
- [ ] 2.2 Implement `UserSaveData` serializable class (Inventory, Encyclopedia, Gold).
- [ ] 2.3 Define `IDataService` and `IUserService` interfaces.
- [ ] 2.4 Implement a simple `DIContainer` for service registration and injection.

## 3. Excel Automation Tool

- [ ] 3.1 Implement `ExcelDataConverter` editor script.
- [ ] 3.2 Add "Create Blank Template" functionality with NPOI.
- [ ] 3.3 Add "Convert to ScriptableObject" functionality with NPOI.

## 4. Business Logic Implementation

- [ ] 4.1 Implement `ExcelDataService` to load SOs via DI.
- [ ] 4.2 Implement `UserStorageService` for JSON save/load logic.
- [ ] 4.3 Add methods to `IUserService` for `AddFish` and `SellFish` with persistence.

## 5. Verification

- [ ] 5.1 Test blank excel generation.
- [ ] 5.2 Test excel-to-SO conversion with sample data.
- [ ] 5.3 Verify that selling a fish doesn't remove it from the encyclopedia.
