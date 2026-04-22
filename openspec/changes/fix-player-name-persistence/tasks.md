## 1. NetworkMenuUI 이름 즉시 저장

- [x] 1.1 `ForceSaveName()` 메서드 추가: nameInput이 null이 아닐 때 현재 텍스트를 PlayerPrefs에 즉시 저장하고 `PlayerPrefs.Save()` 호출
- [x] 1.2 `OnHostClicked()`에서 `manager.StartHost()` 호출 전에 `ForceSaveName()` 호출 추가
- [x] 1.3 `OnJoinClicked()`에서 `manager.StartClient()` 호출 전에 `ForceSaveName()` 호출 추가
- [x] 1.4 `SavePlayerName()` 리스너 등록을 `Start()`에서 `Awake()`로 이동 (onEndEdit 리스너가 씬 전환 전에 확실히 등록되도록 보장)

## 2. 참조 자동 복원 검증

- [x] 2.1 `FindReferences()`에서 nameInput, offlineControlsRoot, onlineControlsRoot 검색 로직 검증 (현재 코드가 GetComponentsInChildren(true)로 이미 검색 중) - 확인 완료
- [x] 2.2 PlayScene의 Static UICanvas 프리팹 인스턴스에 nameInput 참조가 정상적으로 복원되는지 확인 (프리팹 YAML에서 nameInput이 {fileID: 0}이므로 FindReferences에 의존) - 확인 완료

## 3. FishingPlayer 이름 전달 흐름 검증

- [x] 3.1 `OnStartLocalPlayer()`에서 `PlayerPrefs.GetString("PlayerName", ...)`이 ForceSaveName으로 저장된 값을 읽는지 확인 - 확인 완료
- [x] 3.2 `CmdUpdatePlayerName()`이 서버에서 playerName SyncVar를 올바르게 설정하는지 확인 - 확인 완료
- [x] 3.3 SyncVar hook이 PlayerNameDisplay.UpdateName()을 호출하는지 확인 - 확인 완료

## 4. NotificationUI 알림 표시 검증

- [x] 4.1 Dynamic UI Canvas가 PlayScene에 배치되어 있는지 확인 (이미 확인 완료)
- [x] 4.2 NotificationUI가 FishingPlayer.OnSystemMessage 이벤트를 정상적으로 구독하는지 확인 - 확인 완료
- [x] 4.3 CmdUpdatePlayerName → RpcBroadcastSystemMessage → OnSystemMessage 흐름이 PlayScene에서 정상 동작하는지 확인 - 확인 완료