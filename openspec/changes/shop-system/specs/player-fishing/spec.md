## ADDED Requirements

### Requirement: 장착 Rod 스탯 반영
시스템 SHALL 장착한 낚싯대(RodDataSO)의 스탯을 낚시 로직에 반영한다.

#### Scenario: 캐스팅 거리 보너스
- **WHEN** 플레이어가 낚시를 캐스팅할 때
- **THEN** 장착한 Rod의 `castDistanceBonus`만큼 최대 캐스팅 거리가 증가한다

#### Scenario: 포획 확률 보너스
- **WHEN** 서버에서 catch 결과를 계산(CalculateCatch)할 때
- **THEN** 장착한 Rod와 Bait의 `catchChanceBonus`를 합산하여 포획 확률에 추가한다

### Requirement: 장착 Bait 스탯 반영
시스템 SHALL 장착한 미끼(BaitDataSO)의 스탯을 낚시 로직에 반영한다.

#### Scenario: 특정 물고기 유인
- **WHEN** Bait.attractionFishIds가 비어 있지 않고 특정 fishId를 지정하면
- **THEN** 해당 물고기군의 등장 확률이 증가한다 (구체적인 확률 공식은 밸런스에 따라 조정)

#### Scenario: 포획 확률 보너스 중복
- **WHEN** Rod와 Bait 모두 catchChanceBonus가 있으면
- **THEN** 두 보너스가 합산되어 CalculateCatch()에 전달된다

### Requirement: SyncVar 동기화
시스템 SHALL FishingPlayer에 equippedRodId, equippedBaitId SyncVar를 선언하여 네트워크 동기화한다.

#### Scenario: 장착 동기화
- **WHEN** 서버에서 equippedRodId가 변경되면
- **THEN** 모든 클라이언트의 FishingPlayer에 자동 동기화된다 (SyncVar hook)
