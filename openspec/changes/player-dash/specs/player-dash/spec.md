## ADDED Requirements

### Requirement: Player can dash forward
플레이어는 Space 키를 눌러 현재 바라보는 방향으로 짧은 거리를 빠르게 대시할 수 있다.

#### Scenario: Idle 상태에서 대시
- **GIVEN** 플레이어가 Idle 상태이고 낚시 중이 아닐 때
- **WHEN** Space 키를 누른다
- **THEN** 플레이어가 `transform.forward` 방향으로 약 5유닛을 0.3초 동안 빠르게 이동한다
- **AND** 대시 쿨타임(3초)이 시작된다

#### Scenario: 낚시 중 대시 시도
- **GIVEN** 플레이어가 낚시 중(FishingController.CurrentState != Idle)일 때
- **WHEN** Space 키를 누른다
- **THEN** 대시가 실행되지 않는다
- **AND** 아무 피드백도 발생하지 않는다

#### Scenario: 쿨타임 중 대시 시도
- **GIVEN** 플레이어가 방금 대시를 사용한 직후(3초 쿨타임 중)일 때
- **WHEN** Space 키를 다시 누른다
- **THEN** 대시가 실행되지 않는다

#### Scenario: 네트워크 멀티플레이어 환경에서 대시 동기화
- **GIVEN** 두 명 이상의 플레이어가 접속한 서버에서
- **WHEN** 한 플레이어가 Space 키로 대시한다
- **THEN** 서버에서 위치 변경을 검증하고 모든 클라이언트에 동기화한다
- **AND** 다른 플레이어의 화면에서도 대시한 플레이어가 순간적으로 이동한 것이 보인다

#### Scenario: 대시 중 회전 가능
- **GIVEN** 플레이어가 대시 중일 때
- **WHEN** 마우스를 좌우로 움직인다
- **THEN** 대시 방향은 고정되지만 캐릭터 회전은 가능하다 (대시 종료 후 새로운 방향으로 자연스럽게 전환)
