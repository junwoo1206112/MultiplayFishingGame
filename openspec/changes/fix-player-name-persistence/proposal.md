## Why

Lobby 씬에서 닉네임 입력란에 적은 이름이 PlayScene에서 플레이어 위에 표시되지 않고, 서버에서 할당한 랜덤 이름("낚시꾼 456")이 대신 표시된다. TMP_InputField의 `onEndEdit` 이벤트가 버튼 클릭 시점에 반드시 발동한다는 보장이 없어서, PlayerPrefs에 이름이 저장되지 않은 채 씬 전환이 발생할 수 있다. 또한 씬 전환 후 PlayScene의 새 NetworkMenuUI 인스턴스가 `nameInput` 참조를 제때 복원하지 못하면 이름 입력 자체가 불가능해진다.

## What Changes

- `NetworkMenuUI.OnHostClicked()` / `OnJoinClicked()` 호출 시점에 현재 nameInput 값을 즉시 PlayerPrefs에 저장 (race condition 방지)
- `FishingPlayer.OnStartLocalPlayer()`에서 PlayerPrefs 읽기 전 이름이 확실히 저장되어 있도록 보장
- `NetworkMenuUI.FindReferences()`에서 nameInput 참조를 확실하게 복원하도록 개선
- 입장 알림 메시지("XXX님이 입장하셨습니다")가 정상 표시되도록 보장

## Capabilities

### New Capabilities

- `player-name-persistence`: 로비에서 입력한 닉네임이 플레이 씬에서 플레이어 SyncVar로 전달되어 표시되는 전체 흐름

### Modified Capabilities

(없음 - 기존 스펙 없음)

## Impact

- `Assets/Scripts/UI/NetworkMenuUI.cs`: 버튼 핸들러에 즉시 저장 로직 추가
- `Assets/Scripts/Gameplay/FishingPlayer.cs`: 이름 전달 흐름 검증
- `Assets/Scripts/UI/PlayerNameDisplay.cs`: 이름 표시 로직 검증
- `Assets/Scripts/UI/NotificationUI.cs`: 입장 알림 표시 검증