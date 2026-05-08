## ADDED Requirements

### Requirement: 골드 획득
시스템 SHALL 물고기 판매를 통해 골드를 획득할 수 있어야 한다.

#### Scenario: 판매 시 골드 증가
- **WHEN** 물고기를 판매하면
- **THEN** FishDataSO.sellPrice 만큼 UserSaveData.gold가 증가한다

#### Scenario: 일괄 판매 시 합산 골드
- **WHEN** 모든 물고기를 일괄 판매하면
- **THEN** 각 물고기의 sellPrice 합계만큼 gold가 증가한다

### Requirement: 골드 사용
시스템 SHALL 아이템 구매 시 골드를 차감한다.

#### Scenario: 구매 시 골드 차감
- **WHEN** 아이템을 구매하면
- **THEN** Item.price 만큼 UserSaveData.gold가 차감된다

#### Scenario: 골드 부족 시 구매 불가
- **WHEN** gold < item.price 일 때 구매를 시도하면
- **THEN** 구매가 거부되고 gold는 차감되지 않는다

### Requirement: 골드 저장/로드
시스템 SHALL 골드 데이터를 저장하고 로드한다.

#### Scenario: 게임 저장
- **WHEN** UserSaveData.Save()가 호출되면
- **THEN** gold가 JSON에 포함되어 persistentDataPath에 저장된다

#### Scenario: 게임 로드
- **WHEN** UserSaveData.Load()가 호출되면
- **THEN** 저장된 gold 값이 복원된다
