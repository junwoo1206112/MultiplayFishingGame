## ADDED Requirements

### Requirement: 하단 인벤토리 패널
시스템 SHALL ShopUI 하단에 인벤토리 패널을 표시한다.

#### Scenario: 인벤토리 패널 표시
- **WHEN** ShopUI가 열리면
- **THEN** 하단에 "내 인벤토리" 헤더와 함께 보유한 물고기 목록이 가로 스크롤로 표시된다

#### Scenario: 빈 인벤토리
- **WHEN** 인벤토리에 물고기가 없으면
- **THEN** "보유한 물고기가 없습니다" 메시지를 표시한다

### Requirement: 물고기 개별 판매
시스템 SHALL 인벤토리 패널에서 개별 물고기를 판매할 수 있어야 한다.

#### Scenario: 개별 판매
- **WHEN** 물고기 슬롯의 [판매] 버튼을 클릭하면
- **THEN** userService.SellFish(instanceId)가 호출되고 해당 물고기가 인벤토리에서 제거되며 골드가 증가한다

#### Scenario: 판매 확인
- **WHEN** [판매] 버튼을 클릭하면
- **THEN** "XX물고기를 YYG에 판매하시겠습니까?" 확인 다이얼로그가 표시되고 [확인] 시 판매가 진행된다

### Requirement: 물고기 일괄 판매
시스템 SHALL "전체 판매" 버튼으로 모든 물고기를 한 번에 판매할 수 있어야 한다.

#### Scenario: 전체 판매 성공
- **WHEN** [전체 판매] 버튼을 클릭하면
- **THEN** 모든 물고기가 판매되고 합산 골드가 지급되며 "총 XX,XXX G를 획득했습니다" 메시지가 표시된다

#### Scenario: 전체 판매 확인
- **WHEN** [전체 판매] 버튼을 클릭하면
- **THEN** "보유한 모든 물고기를 판매합니다 (총 N마리, 예상 수익: XXX G)" 확인 다이얼로그가 표시된다

### Requirement: 판매 가격 표시
시스템 SHALL 각 물고기 슬롯에 판매 가격을 표시한다.

#### Scenario: 판매 가격 표시
- **WHEN** 물고기 슬롯이 생성될 때
- **THEN** 물고기 이름, 크기와 함께 판매 가격( FishDataSO.sellPrice)이 표시된다
