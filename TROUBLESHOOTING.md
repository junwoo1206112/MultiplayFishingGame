# Unity Relay — 문제해결 가이드

Unity 6 (6000.3.10f1) + Mirror v96 + Unity Relay + UTP 2.6.0

---

## 1. NetworkManager 셋업

### 프리팹 vs 직접 생성

**직접 생성 (추천):**
1. Hierarchy에서 빈 GameObject 생성 → 이름 `NetworkManager`
2. Add Component → `FishingRoomManager` (Script)
3. Add Component → `UnityRelayTransport` (Script)
4. `FishingRoomManager` 설정:
   - `dontDestroyOnLoad`: ✅ 체크
   - `offlineScene`: `Assets/Scenes/Lobby.unity`
   - `onlineScene`: `Assets/Scenes/PlayScene.unity`
   - `playerPrefab`: 플레이어 프리팹 할당
   - `autoCreatePlayer`: ✅ 체크
   - `Transport` 필드: 같은 오브젝트의 `UnityRelayTransport` 드래그

### 프리팹으로 만들기
- Hierarchy의 `NetworkManager` → Project 폴더로 드래그
- 저장 후 Hierarchy의 것은 씬 인스턴스로 유지

### 주의: `Transform resides in a Prefab asset` 에러
- NetworkManager가 프리팹 모드(Prefab Mode)로 열려 있으면 발생
- **Prefab Mode 탭 닫기** (X 버튼 or Ctrl+W)
- Hierarchy 뷰만 남도록 함

---

## 2. Unity Services 프로젝트 연결

### Unity Cloud Dashboard
1. https://cloud.unity.com → 프로젝트(`MFishingOnline`) 생성
2. **Relay** → **Get Started** (활성화)
3. **Authentication** → **Get Started** (Anonymous 활성화, 기본 ON)
4. 브라우저 주소창에서 **Project ID** 복사
   - `.../projects/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/...`

### Unity Editor 연결
1. **Edit → Project Settings → Services**
2. `Project ID` 필드에 복사한 ID 붙여넣기 → **Save**
3. 또는 **Use an existing Unity project ID** → 프로젝트 선택 → **Link**

---

## 3. 패키지 설치 (manifest.json)

```json
"com.unity.transport": "2.6.0",        // 자동 해결 (직접 명시 X)
"com.unity.services.multiplayer": "2.2.2",
```

⚠️ **`com.unity.transport`를 manifest.json에 직접 명시하지 마세요.**
멀티플레이어 패키지가 의존하는 2.6.0이 자동으로 사용되어야 호환됩니다.

❌ `"com.unity.transport": "2.7.2"` → 버전 불일치로 릴레이 라우팅 실패

---

## 4. asmdef 참조

`Assets/Scripts/Network/MultiplayFishing.Network.asmdef`에 다음 참조 추가:

```json
"references": [
    "Mirror",
    "Mirror.Transports",
    "Unity.Networking.Transport",
    "Unity.Collections",
    "Unity.Services.Multiplayer",
    "Unity.Services.Core",
    "Unity.Services.Authentication"
]
```

---

## 5. 연결 플로우

### 호스트 (방 만들기)
```
CreateRoom()
  → CreateRelayAllocation()    ← HTTP 할당만 (UTP 없음)
  → StartHost()
    → ServerStart()            ← 여기서 UTP 드라이버 생성 + Bind + Listen
    → PlayScene 로드
```

### 클라이언트 (참가하기)
```
JoinRoom(code)
  → PrepareRelayJoin(code)     ← 코드 저장만
  → StartClient()
    → ClientConnect()          ← Mirror가 호출
      → ConnectRelayAsync()    ← JoinAllocation HTTP + UTP Connect
      → clientDriver.PopEvent() → Connect 이벤트
      → OnClientConnected()    ← Mirror 연결 완료
    → Ready() + AddPlayer()    ← 플레이어 생성 요청
    → PlayScene 로드 (서버가 지시)
```

---

## 6. UnityRelayTransport 구조

| 컴포넌트 | 역할 |
|---------|------|
| `serverDriver` | 서버 UTP 드라이버 (Bind + Listen) |
| `clientDriver` | 클라이언트 UTP 드라이버 (Connect) |
| `InitServices()` | UnityServices.InitializeAsync + 익명 로그인 |
| `CreateRelayAllocation()` | HTTP 할당 생성 + joinCode 발급 |
| `PrepareRelayJoin()` | joinCode 저장 (실제 연결은 ClientConnect에서) |
| `ServerEarlyUpdate()` | `serverDriver.ScheduleUpdate()` 처리 |
| `ClientEarlyUpdate()` | `clientDriver.ScheduleUpdate()` 처리 |
| `FindConnectionId()` | UTP Connection → Mirror connectionId 매핑 |

### ⚠️ 주의: serverDriver / clientDriver 분리
- **절대 하나의 `driver`로 처리하지 말 것**
- 호스트 모드에서 `ClientConnect()`가 `serverDriver`를 덮어쓰면 서버가 죽음
- 항상 `serverDriver`(서버) / `clientDriver`(클라이언트) 분리 유지

---

## 7. 자주 발생하는 문제

### "Core Registry not initialized"

**원인:** `UnityServices.InitializeAsync()`가 실패했지만 `servicesInitialized = true`로 설정됨

**해결:**
- `InitServices()`에서 try-catch 밖에서 `servicesInitialized = true` 설정 금지
- 예외 발생 시 `throw`로 전파
- `servicesInitialized = false`를 매 `CreateRelayAllocation()` 시작 시 리셋

### "Transform resides in a Prefab asset"

**원인:** Prefab Mode가 열려 있어 NetworkManager가 프리팹 에셋으로 취급됨

**해결:** Prefab Mode 탭 닫기

### "Multiple NetworkManagers detected"

**원인:** NetworkManager가 씬에 2개 이상 존재

**해결:**
- Hierarchy에서 NetworkManager 1개만 남기고 모두 삭제
- `NetworkManager 1.prefab`과 수동 생성한 `NetworkManager`가 동시에 존재하지 않도록 확인

### "Server or Client already started"

**원인:** `async void CreateRoom()` await 중 모드가 변경됨

**해결:**
- `isCreatingRoom` 플래그로 중복 실행 방지
- `mode` 체크 대신 `isCreatingRoom` 사용

### 클라이언트가 PlayScene으로 안 넘어감

**원인 1:** `OnClientConnect()` 오버라이드 누락

**해결:**
```csharp
public override void OnClientConnect()
{
    base.OnClientConnect();
    if (NetworkServer.active) return;  // 호스트 모드면 스킵
    NetworkClient.Ready();
    NetworkClient.AddPlayer();
}
```

**원인 2:** NetworkManager가 DontDestroyOnLoad가 아님

**해결:** `dontDestroyOnLoad: ✅` 체크

### "The referenced script (Unknown) on this Behaviour is missing!"

**원인:** 프리팹에 예전 스크립트 참조가 남아있음 (Edgegap, Kcp 등)

**해결:**
- Console 에러 줄 더블클릭 → 해당 오브젝트 찾기
- Inspector에서 빨간 Missing Script → **Remove Component**
- 모든 프리팹에서 반복 제거

---

## 8. 빌드 에러

### "lib_burst_generated.dll failed: 다른 프로세스가 파일을 사용 중"

**원인:** 이전 빌드 실행 파일이 실행 중

**해결:**
1. 작업 관리자(Ctrl+Shift+Esc) → `MultiplayFishingGame.exe` 종료
2. 또는 PC 재부팅
3. `Build/` 폴더 삭제 후 재빌드

---

## 9. 참고: Edgegap으로 전환할 경우

Unity Relay 대신 Edgegap을 사용하려면:

1. `UnityRelayTransport` 제거
2. `EdgegapLobbyKcpTransport` 추가 (Mirror에 내장됨)
3. "Create & Deploy Lobby" 버튼 → API 키 입력
4. `FishingRoomManager`에서 `CreateRelayAllocation` 대신 `lt.SetServerLobbyParams()` 사용
5. 월 $1 (무료 티어 아님)

---

## 10. 파일 구조 (변경된 파일 목록)

| 파일 | 변경 |
|------|------|
| `Assets/Scripts/Network/UnityRelayTransport.cs` | 새 파일 (UTP + Relay Transport) |
| `Assets/Scripts/Network/FishingRoomManager.cs` | CreateRoom/JoinRoom Relay 방식 |
| `Assets/Scripts/UI/NetworkMenuUI.cs` | Join code UI, NetworkServer.active 사용 |
| `Assets/Scripts/Network/MultiplayFishing.Network.asmdef` | UTP/Relay 참조 추가 |
| `Assets/Scenes/Lobby.unity` | NetworkManager 1 → 수동 생성 NetworkManager |
| `Assets/Prefabs/Manager/NetworkManager.prefab` | 새 프리팹 (직접 생성) |
| `Assets/Prefabs/Player/Player.prefab` | PlayerPrefab 설정 |
| `Packages/manifest.json` | com.unity.transport 제거 |
| `Packages/packages-lock.json` | 패키지 버전 업데이트 |
| `.gitignore` | Builld/, *.csproj, *.slnx 추가 |
