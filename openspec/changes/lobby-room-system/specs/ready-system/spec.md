## ADDED Requirements

### Requirement: Ready State Toggle
플레이어는 대기실에서 준비 상태를 토글할 수 있어야 한다. 준비 상태는 모든 클라이언트에 동기화된다.

#### Scenario: Toggle ready state
- **WHEN** 플레이어가 "준비 완료" 버튼을 누름
- **THEN** 해당 플레이어의 준비 상태가 토글되고, 대기실의 모든 플레이어에게 동기화됨

#### Scenario: Toggle ready state off
- **WHEN** 준비 상태인 플레이어가 다시 버튼을 누름
- **THEN** 해당 플레이어의 준비 상태가 해제되고, 모든 클라이언트에 반영됨

### Requirement: Game Start Authorization
게임 시작은 방장만 가능하며, 모든 플레이어가 준비 상태여야 한다.

#### Scenario: Host starts game when all ready
- **WHEN** 방장이 "게임 시작" 버튼을 누르고, 모든 플레이어가 준비 상태임
- **THEN** 서버에서 게임 씬(FishingLake)으로 전환됨

#### Scenario: Host starts game when not all ready
- **WHEN** 방장이 "게임 시작" 버튼을 누르지만, 준비하지 않은 플레이어가 있음
- **THEN** 게임이 시작되지 않고 "모든 플레이어가 준비해야 합니다" 메시지가 표시됨

#### Scenario: Non-host tries to start game
- **WHEN** 방장이 아닌 플레이어가 게임 시작을 요청함
- **THEN** 서버가 요청을 무시하고 경고 로그를 출력함

### Requirement: Minimum Player Count
게임을 시작하려면 최소 2명의 플레이어가 필요하다.

#### Scenario: Start game with minimum players
- **WHEN** 2명 이상의 플레이어가 방에 있고 모두 준비 상태임
- **THEN** 방장이 게임을 시작할 수 있음

#### Scenario: Start game with only one player
- **WHEN** 방에 1명만 있고 준비 상태임
- **THEN** "최소 2명의 플레이어가 필요합니다" 메시지가 표시됨

### Requirement: Ready State UI
대기실 UI에는 각 플레이어의 이름과 준비 상태가 표시되어야 한다. 방장은 "게임 시작" 버튼을, 일반 플레이어는 "준비 완료" 버튼을 볼 수 있다.

#### Scenario: Display player ready states
- **WHEN** 플레이어가 대기실에 입장함
- **THEN** 모든 플레이어의 이름과 준비 상태(준비/대기)가 표시됨

#### Scenario: Host sees start button
- **WHEN** 방장이 대기실에 있음
- **THEN** "게임 시작" 버튼이 표시되고, "준비 완료" 버튼은 표시되지 않음

#### Scenario: Non-host sees ready button
- **WHEN** 일반 플레이어가 대기실에 있음
- **THEN** "준비 완료" 버튼이 표시되고, "게임 시작" 버튼은 표시되지 않음