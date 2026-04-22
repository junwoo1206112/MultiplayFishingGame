## Context

멀티플레이어 낚시 게임에서 로비 씬에서 닉네임을 입력하고 방만들기/참가하기 버튼을 누르면 Mirror가 씬 전환을 시작한다. 이때 TMP_InputField의 `onEndEdit` 이벤트가 버튼 클릭과의 경쟁 조건(race condition)으로 인해 이름이 PlayerPrefs에 저장되지 않을 수 있다. 결과적으로 PlayScene에서 `FishingPlayer.OnStartLocalPlayer()`가 읽는 PlayerPrefs 값이 이전 세션의 것이거나 빈 값이 되어, 서버가 할당한 랜덤 이름("낚시꾼 456")만 표시된다.

현재 이름 전달 흐름:
1. 사용자가 nameInput에 이름 입력
2. `onEndEdit` → `SavePlayerName()` → `PlayerPrefs.SetString("PlayerName", ...)`
3. 버튼 클릭 → `StartHost()/StartClient()` → Mirror 씬 전환
4. 플레이어 스폰 → `OnStartLocalPlayer()` → `PlayerPrefs.GetString("PlayerName")` → `CmdUpdatePlayerName()`
5. 서버 → `playerName = newName` → SyncVar hook → `PlayerNameDisplay.UpdateName()`

문제점: 단계 2와 3 사이에 경쟁 조건이 있다.

## Goals / Non-Goals

**Goals:**
- Lobby에서 입력한 닉네임이 확실하게 PlayScene의 플레이어 이름으로 표시되는 것을 보장
- 입장 알림("XXX님이 입장하셨습니다")이 정상 표시되는 것을 보장
- 씬 전환 시 nameInput 참조가 확실히 복원되는 것을 보장

**Non-Goals:**
- 닉네임 중복 검증 (서버 측 기능, 별도 범위)
- 닉네임 실시간 변경 UI (이미 CmdUpdatePlayerName 존재)
- 채팅 시스템 (별도 범위)

## Decisions

### 결정 1: 버튼 클릭 핸들러에서 즉시 PlayerPrefs 저장

`OnHostClicked()`과 `OnJoinClicked()`에서 `StartHost()/StartClient()` 호출 **직전**에 `SavePlayerName()`을 명시적으로 호출한다.

**이유**: `onEndEdit` 이벤트는 포커스 이동 시 발동하지만, 버튼 클릭 이벤트와의 실행 순서가 보장되지 않는다. 버튼 핸들러에서 직접 저장하면 경쟁 조건을 완전히 제거할 수 있다.

**대안 고려**:
- `onValueChanged` 사용: 매 타이핑마다 저장되어 효율적이지 않음
- 씬 전환 전 콜백 대기: 복잡도 증가, Mirror 씬 전환은 비동기

### 결정 2: nameInput을 직접 읽어 PlayerPrefs에 저장

`SavePlayerName()`은 현재 `onEndEdit` 파라미터로 전달된 문자열만 저장한다. 버튼 핸들러에서는 `nameInput`의 현재 텍스트 값을 직접 읽어 저장하는 `ForceSaveName()` 메서드를 추가한다.

**이유**: `onEndEdit`와 버튼 클릭이 같은 프레임에 발생하면 이미 저장된 값이 있겠지만, `onEndEdit`가 누락된 경우를 완전히 커버하기 위함이다.

### 결정 3: SyncVar 이름 전달은 기존 방식 유지

`FishingPlayer.OnStartLocalPlayer()` → `CmdUpdatePlayerName()` → 서버 `playerName` SyncVar 설정은 그대로 유지한다. 이 흐름은 Mirror의 권장 패턴이며, PlayerPrefs 저장이 확실하면 정상 동작한다.

### 결정 4: NotificationUI 이벤트 구독 유지

`FishingPlayer.OnSystemMessage` 정적 이벤트와 `NotificationUI`의 구독은 기존 패턴을 유지한다. PlayScene에 Dynamic UI Canvas가 배치되어 있으므로 씬 전환 후 NotificationUI가 정상적으로 이벤트를 수신한다.

## Risks / Trade-offs

- **[Risk] nameInput이 여전히 null인 경우** → `FindReferences()`가 `GetComponentsInChildren<TMP_InputField>(true)`를 사용하여 비활성화된 자식까지 검색하므로, 프리팹 구조가 올바르면 null이 될 수 없다. `ForceSaveName()`에서 null 체크 추가로 방어.
- **[Risk] PlayerPrefs가 플랫폼별로 다르게 동작** → Unity의 PlayerPrefs는 모든 플랫폼에서 동일하게 동작하므로 문제없음.
- **[Trade-off] onEndEdit과 ForceSaveName() 이중 저장** → 중복 저장은 발생할 수 있지만, 같은 키에 같은 값을 저장하므로 부작용 없음.