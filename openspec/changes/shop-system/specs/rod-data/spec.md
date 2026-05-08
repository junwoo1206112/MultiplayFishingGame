## ADDED Requirements

### Requirement: RodDataSO 데이터 모델 정의
시스템 SHALL `RodDataSO` ScriptableObject를 정의하여 낚싯대 데이터를 관리한다.

#### Scenario: RodDataSO 생성
- **WHEN** Editor에서 "Create → Fishing/Rod Data" 메뉴를 선택하면
- **THEN** 새로운 RodDataSO 에셋이 생성되고 id, rodName, icon, rank, price, castDistanceBonus, catchChanceBonus, durability, description 필드를 가진다

#### Scenario: 엑셀에서 Rod 데이터 로드
- **WHEN** ExcelDataConverter가 FishData.xlsx의 "Rods" 시트를 읽으면
- **THEN** 각 행에 대해 RodDataSO 에셋이 생성/갱신되고 모든 필드가 매핑된다

#### Scenario: 잘못된 엑셀 데이터 처리
- **WHEN** Rods 시트의 한 행에 ID가 없거나 price가 0 미만이면
- **THEN** 해당 행은 건너뛰고 에러 로그를 출력한다

### Requirement: Rod 데이터 런타임 조회
시스템 SHALL IDataService를 통해 Rod 데이터를 조회할 수 있어야 한다.

#### Scenario: ID로 Rod 조회
- **WHEN** `GetRodData("rod_golden")` 호출 시
- **THEN** 해당 ID의 RodDataSO를 반환하고, 없으면 null을 반환한다

#### Scenario: 전체 Rod 목록 조회
- **WHEN** `GetAllRodData()` 호출 시
- **THEN** price >= 0인 모든 RodDataSO 리스트를 반환한다
