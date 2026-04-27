## ADDED Requirements

### Requirement: 닉네임 입력값 즉시 저장
`NetworkMenuUI`는 Host/Join 버튼 클릭 시 `nameInput`의 현재 텍스트 값을 PlayerPrefs에 즉시 저장하는 `ForceSaveName()` 메서드를 제공해야 한다. `OnHostClicked()`과 `OnJoinClicked()`은 네트워크 연결 시작 전에 반드시 `ForceSaveName()`을 호출해야 한다.

#### Scenario: Host 버튼 클릭 시 이름 저장
- **WHEN** 사용자가 이름 입력란에 "홍길동"을 입력하고 "방만들기" 버튼을 클릭함
- **THEN** `PlayerPrefs.GetString("PlayerName")`이 "홍길동"을 반환해야 함 (StartHost() 호출 전에 저장 완료)

#### Scenario: Join 버튼 클릭 시 이름 저장
- **WHEN** 사용자가 이름 입력란에 "철수"를 입력하고 "참가하기" 버튼을 클릭함
- **THEN** `PlayerPrefs.GetString("PlayerName")`이 "철수"를 반환해야 함 (StartClient() 호출 전에 저장 완료)

#### Scenario: nameInput이 null인 경우
- **WHEN** `FindReferences()`가 nameInput을 찾지 못해 null인 상태로 버튼이 클릭됨
- **THEN** `ForceSaveName()`은 안전하게 무시되고, 기존 PlayerPrefs 값이 사용됨 (함수가 null 체크를 포함)

### Requirement: 플레이어 이름 SyncVar 전달
`FishingPlayer.OnStartLocalPlayer()`는 `PlayerPrefs.GetString("PlayerName", fallback)`으로 읽은 이름을 `CmdUpdatePlayerName()`으로 서버에 전달해야 한다. 서버는 이 이름을 `playerName` SyncVar에 설정하고, 모든 클라이언트의 SyncVar hook이 이를 표시에 반영해야 한다.

#### Scenario: 정상 이름 전달
- **WHEN** PlayerPrefs에 "PlayerName" 키로 "홍길동"이 저장되어 있고, 플레이어가 로컬로 스폰됨
- **THEN** `CmdUpdatePlayerName("홍길동")`이 서버로 전송되고, 서버가 `playerName = "홍길동"` 설정 후 모든 클라이언트에 동기화됨

#### Scenario: 이전 세션 이름 유지
- **WHEN** 이전 플레이 세션에서 "PlayerName" = "철수"가 저장되어 있고, 현재 세션에서 이름을 수정하지 않음
- **THEN** `OnStartLocalPlayer()`가 "철수"를 읽어 서버로 전송함

### Requirement: 이름 입력 참조 자동 복원
`NetworkMenuUI.FindReferences()`는 `[SerializeField]` 참조가 null일 때 런타임에 자동으로 `nameInput`, `offlineControlsRoot`, `onlineControlsRoot`를 찾아 할당해야 한다.

#### Scenario: nameInput 프리팹 참조가 null인 경우
- **WHEN** Static UICanvas 프리팹에서 nameInput 직렬화 참조가 null이지만 NameInputField 자식이 존재함
- **THEN** `FindReferences()`가 `GetComponentsInChildren<TMP_InputField>(true)`로 nameInput을 찾아 할당함

#### Scenario: 씬 전환 후 새 NetworkMenuUI 인스턴스
- **WHEN** Lobby에서 PlayScene으로 전환되어 새 Static UICanvas가 생성됨
- **THEN** `Awake()`에서 `FindReferences()`가 호출되어 모든 참조가 복원되고, `Start()`에서 nameInput의 onEndEdit 리스너가 등록됨

### Requirement: 입장 알림 표시
`FishingPlayer.CmdUpdatePlayerName()`이 서버에서 호출될 때, `RpcBroadcastSystemMessage()`로 "{이름}님이 입장하셨습니다." 메시지를 모든 클라이언트에 브로드캐스트해야 하며, `NotificationUI`가 이를 수신하여 화면에 표시해야 한다.

#### Scenario: 새 플레이어 입장 알림
- **WHEN** 플레이어가 "홍길동" 이름으로 입장함
- **THEN** 모든 클라이언트에서 NotificationUI가 "홍길동님이 입장하셨습니다." 메시지를 표시함