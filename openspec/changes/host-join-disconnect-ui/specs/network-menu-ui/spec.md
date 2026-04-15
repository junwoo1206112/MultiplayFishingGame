## ADDED Requirements

### Requirement: Host/Join/Disconnect UI
시스템은 플레이어가 호스트를 시작하거나, IP 주소를 입력하여 서버에 조인하거나, 연결을 끊을 수 있는 UI를 제공해야 한다.

#### Scenario: Start host
- **WHEN** 플레이어가 "호스트 시작" 버튼을 클릭함
- **THEN** 서버와 클라이언트가 동시에 시작되고(Host 모드), UI가 온라인 상태로 전환되며 내 IP 주소가 표시됨

#### Scenario: Join server by IP
- **WHEN** 플레이어가 IP 주소를 입력하고 "접속" 버튼을 클릭함
- **THEN** 해당 IP의 서버에 클라이언트로 연결을 시도함

#### Scenario: Disconnect
- **WHEN** 플레이어가 "연결 끊기" 버튼을 클릭함
- **THEN** 현재 연결이 종료되고 UI가 오프라인 상태로 전환됨

#### Scenario: Default IP address
- **WHEN** UI가 처음 로드됨
- **THEN** IP 입력 필드에 "127.0.0.1"이 기본값으로 표시됨

### Requirement: Connection Status Display
시스템은 현재 연결 상태, 모드, 접속 인원을 표시해야 한다.

#### Scenario: Display host mode info
- **WHEN** 호스트 모드로 실행 중임
- **THEN** "호스트" 모드, 내 IP 주소, 접속 인원/최대 인원이 표시됨

#### Scenario: Display client mode info
- **WHEN** 클라이언트 모드로 서버에 연결됨
- **THEN** "클라이언트" 모드, 서버 주소, 접속 인원이 표시됨

#### Scenario: Display connecting state
- **WHEN** 클라이언트가 서버에 연결 중임
- **THEN** "연결 중..." 상태와 대상 주소가 표시됨

#### Scenario: Display offline state
- **WHEN** 네트워크에 연결되어 있지 않음
- **THEN** "오프라인" 상태가 표시되고 호스트/조인 버튼이 활성화됨

### Requirement: Network State Event System
FishingNetworkManager는 네트워크 상태 변화를 UI에 알리기 위한 정적 이벤트를 제공해야 한다.

#### Scenario: UI receives state change
- **WHEN** 네트워크 상태가 변화함 (접속, 해제, 호스트 시작/종료)
- **THEN** NetworkStateChanged 이벤트가 발생하고 구독한 UI 컴포넌트가 갱신됨

#### Scenario: Local IP address retrieval
- **WHEN** GetLocalIPAddress()이 호출됨
- **THEN** 루프백이 아닌 첫 번째 IPv4 주소를 반환함. 찾지 못하면 "127.0.0.1" 반환