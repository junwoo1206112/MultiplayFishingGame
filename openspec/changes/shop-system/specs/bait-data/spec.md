## ADDED Requirements

### Requirement: BaitDataSO 데이터 모델 정의
시스템 SHALL `BaitDataSO` ScriptableObject를 정의하여 미끼 데이터를 관리한다.

#### Scenario: BaitDataSO 생성
- **WHEN** Editor에서 "Create → Fishing/Bait Data" 메뉴를 선택하면
- **THEN** 새로운 BaitDataSO 에셋이 생성되고 id, baitName, icon, rank, price, attractionFishIds[], catchChanceBonus, description 필드를 가진다

#### Scenario: 엑셀에서 Bait 데이터 로드
- **WHEN** ExcelDataConverter가 FishData.xlsx의 "Baits" 시트를 읽으면
- **THEN** 각 행에 대해 BaitDataSO 에셋이 생성/갱신되고 모든 필드가 매핑된다

#### Scenario: attractionFishIds 파싱
- **WHEN** 엑셀의 AttractionFishType 열에 "all" 또는 쉼표로 구분된 fish_id 목록이 있으면
- **THEN** 문자열 배열로 파싱되어 BaitDataSO.attractionFishIds에 저장된다

### Requirement: Bait 데이터 런타임 조회
시스템 SHALL IDataService를 통해 Bait 데이터를 조회할 수 있어야 한다.

#### Scenario: ID로 Bait 조회
- **WHEN** `GetBaitData("bait_worm")` 호출 시
- **THEN** 해당 ID의 BaitDataSO를 반환하고, 없으면 null을 반환한다

#### Scenario: 전체 Bait 목록 조회
- **WHEN** `GetAllBaitData()` 호출 시
- **THEN** price >= 0인 모든 BaitDataSO 리스트를 반환한다
