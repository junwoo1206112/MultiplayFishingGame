## ADDED Requirements

### Requirement: Channel Selection
시스템은 플레이어가 서버 접속 후 채널을 선택할 수 있게 해야 한다. 채널은 사전 정의된 카테고리(초보자, 일반, 숙련자)로 구성된다.

#### Scenario: Display channel list
- **WHEN** 플레이어가 서버에 연결됨
- **THEN** 사용 가능한 채널 목록이 표시되며, 각 채널의 이름, 현재 접속자 수, 최대 방 수가 보임

#### Scenario: Select a channel
- **WHEN** 플레이어가 채널을 선택함
- **THEN** 해당 채널의 로비(방 목록)로 전환됨

#### Scenario: Channel data synchronization
- **WHEN** 채널의 접속자 수가 변경됨
- **THEN** 모든 클라이언트에 업데이트된 채널 정보가 동기화됨

### Requirement: Channel Data Model
채널 데이터는 ScriptableObject로 정의되고, 런타임에 SyncList로 동기화된다. 각 채널은 고유 ID, 표시 이름, 최대 방 수를 가져야 한다.

#### Scenario: Channel configuration
- **WHEN** 게임이 시작됨
- **THEN** ScriptableObject에 정의된 채널 목록이 로드되어 서버에 설정됨

#### Scenario: Invalid channel selection
- **WHEN** 플레이어가 존재하지 않는 채널 ID를 전송함
- **THEN** 서버가 요청을 무시하고 경고 로그를 출력함