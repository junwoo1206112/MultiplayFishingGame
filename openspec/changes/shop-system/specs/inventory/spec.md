## ADDED Requirements

### Requirement: Shop 하단 인벤토리 패널 통합
기존 InventoryUI 대신 Shop 하단 패널에서 물고기 판매를 수행한다.

#### Scenario: Shop 하단에서 인벤토리 접근
- **WHEN** ShopUI가 열리면
- **THEN** 하단에 인벤토리 패널이 표시되고 보유 물고기 목록을 볼 수 있다

#### Scenario: InventoryUI와 중복 관리
- **WHEN** Tab 키로 열리는 기존 InventoryUI가 열려 있어도
- **THEN** ShopUI의 인벤토리 패널과 동일한 데이터(UserSaveData.inventory)를 참조한다
