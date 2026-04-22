## ADDED Requirements

### Requirement: Excel-to-SO Conversion Tool
시스템은 에디터 상단 메뉴를 통해 엑셀 파일을 읽어 물고기 데이터 ScriptableObject를 자동 생성해야 한다.

#### Scenario: Generate Blank Template
- **WHEN** 사용자가 "Tools/Excel/Create Blank Fish Data" 메뉴를 클릭한다.
- **THEN** `Assets/ExcelData/FishData.xlsx` 경로에 정해진 컬럼(ID, Name, Rank, Chance, Price, Length)이 포함된 엑셀 파일이 생성된다.

#### Scenario: Convert Excel to ScriptableObjects
- **WHEN** 사용자가 "Tools/Excel/Convert Fish Data to SO" 메뉴를 클릭한다.
- **THEN** 엑셀의 각 로우가 `FishDataSO` 에셋으로 변환되어 `Assets/Resources/Data/Fish/` 폴더에 저장된다.

### Requirement: Encyclopedia Independence
물고기 도감 데이터는 인벤토리 소지 여부와 관계없이 영구적으로 유지되어야 한다.

#### Scenario: Fish Caught
- **WHEN** 플레이어가 물고기를 낚는다.
- **THEN** 인벤토리에 해당 물고기가 추가되고, 도감에도 해당 물고기의 ID가 등록된다.

#### Scenario: Fish Sold
- **WHEN** 플레이어가 인벤토리의 물고기를 판매한다.
- **THEN** 인벤토리에서는 해당 물고기가 제거되지만, 도감의 등록 정보는 삭제되지 않고 그대로 유지된다.
