## ADDED Requirements

### Requirement: 낚싯대 장착/해제
시스템 SHALL 플레이어가 소유한 낚싯대를 장착/해제할 수 있어야 한다.

#### Scenario: 낚싯대 장착
- **WHEN** 소유한 낚싯대 아이템을 선택하고 [장착] 버튼을 클릭하면
- **THEN** UserSaveData.equippedRodId가 해당 rodId로 설정되고 장착 상태가 UI에 반영된다

#### Scenario: 낚싯대 장착 해제
- **WHEN** 장착 중인 낚싯대의 [장착 해제] 버튼을 클릭하면
- **THEN** UserSaveData.equippedRodId가 ""로 설정되고 해제 상태가 UI에 반영된다

#### Scenario: 다른 낚싯대로 교체 장착
- **WHEN** 낚싯대 A 장착 중 낚싯대 B의 [장착] 버튼을 클릭하면
- **THEN** A가 자동 해제되고 B가 장착된다

### Requirement: 미끼 장착/해제
시스템 SHALL 플레이어가 소유한 미끼를 장착/해제할 수 있어야 한다.

#### Scenario: 미끼 장착
- **WHEN** 소유한 미끼 아이템을 선택하고 [장착] 버튼을 클릭하면
- **THEN** UserSaveData.equippedBaitId가 해당 baitId로 설정된다

#### Scenario: 미끼 장착 해제
- **WHEN** 장착 중인 미끼의 [장착 해제] 버튼을 클릭하면
- **THEN** UserSaveData.equippedBaitId가 ""로 설정된다

### Requirement: 접속 시 장착 상태 복원
시스템 SHALL 플레이어 접속 시 저장된 장착 상태를 자동으로 복원한다.

#### Scenario: 접속 시 장착 복원
- **WHEN** 플레이어가 게임에 접속하면
- **THEN** UserSaveData에 저장된 equippedRodId와 equippedBaitId가 FishingPlayer의 SyncVar에 자동으로 설정된다

### Requirement: 서버 동기화
시스템 SHALL 장착 정보를 서버에 동기화한다.

#### Scenario: 장착 동기화
- **WHEN** 클라이언트에서 장착/해제 Command가 서버에 도착하면
- **THEN** 서버가 장착을 검증/처리하고 SyncVar가 모든 클라이언트에 자동 동기화된다
