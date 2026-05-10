> 2026-05-09 status: Edgegap Relay/Lobby is not active in the current runtime flow.
> Current gameplay networking uses Unity Relay join codes through `UnityRelayTransport`.
> See `SERVER_FEATURE_USAGE.md` for the active server feature list before following this legacy Edgegap guide.

# Edgegap Relay + Lobby — 셋업 가이드

외부 IP에서도 접속 가능한 멀티플레이를 위한 Edgegap 릴레이 서버 설정 방법입니다.

---

## 1. Edgegap 계정 및 API 키

1. https://app.edgegap.com 회원가입 (무료)
2. 우측 상단 프로필 → **Settings** → **API Tokens**
3. **Create Token** → 키 복사 (카드 등록 필요할 수 있음, 무료 티어 사용 가능)

---

## 2. Unity Editor 설정

### 2-1. NetworkManager 프리팹 열기

**프리팹 경로:** `Assets/Prefabs/Manager/NetworkManager 1.prefab`

### 2-2. Transport 교체

| 현재 | → 변경 |
|------|--------|
| `KcpTransport` | `EdgegapLobbyKcpTransport` |

방법:
1. `KcpTransport` 컴포넌트 우클릭 → **Remove Component**
2. **Add Component** → `EdgegapLobbyKcpTransport` 검색 후 추가
3. `EdgegapLobbyKcpTransport` Inspector에서 **KCP 설정 값**을 이전 KcpTransport와 동일하게 입력:
   - `Port`: 7777
   - `NoDelay`: 1
   - 그 외 기본값 유지

### 2-3. Lobby Service 배포

1. `EdgegapLobbyKcpTransport` Inspector에서 **"Create & Deploy Lobby"** 버튼 클릭
2. Edgegap API Key 입력
3. **Lobby Name** 입력 (4~5자 권장, 영문)
4. 배포 완료 시 `Lobby URL`이 자동으로 `lobbyUrl` 필드에 입력됨
5. `lobbyWaitTimeout`: 60 (기본값 유지)

### 2-4. Address Input UI 조정

`Panel_Offline` 프리팹 내 `InputField_Address`:
- **Placeholder 텍스트** 변경: `"IP 주소 입력 (기본: 127.0.0.1)"` → `"로비 코드 입력"`
- 이 필드는 이제 **lobbyId 입력용**으로 사용됩니다

---

## 3. 연결 플로우

### 호스트 (방 만들기)
1. 이름 입력 후 **"방만들기 (host)"** 버튼 클릭
2. 자동으로 Edgegap에 로비 생성 + 릴레이 서버 할당
3. `connectionInfoText`에 **로비 코드**가 표시됨
4. **"내 로비 코드 복사하기"** 버튼으로 클립보드에 복사
5. 친구에게 로비 코드를 전달 (카톡, 디코 등)

### 클라이언트 (참가하기)
1. 이름 입력
2. **로비 코드 입력** 필드에 호스트가 알려준 코드 입력
3. **"참가하기 (join)"** 버튼 클릭
4. Edgegap 릴레이를 통해 호스트에게 접속

---

## 4. 코드 구조

| 파일 | 역할 |
|------|------|
| `Assets/Scripts/Network/FishingRoomManager.cs` | `CreateRoom()`, `JoinRoom()`, `CurrentLobbyId` |
| `Assets/Scripts/UI/NetworkMenuUI.cs` | UI 버튼 → Manager 메서드 연결 |
| `Assets/Mirror/Transports/Edgegap/EdgegapLobby/EdgegapLobbyKcpTransport.cs` | 릴레이 + 로비 처리 (public GetLobbyId() 추가됨) |

---

## 5. 테스트 방법

1. Unity Editor에서 실행 → **방만들기**
2. 로비 코드가 표시되는지 확인
3. 빌드 후 다른 PC(또는同一 PC 2개 실행)에서 로비 코드로 접속 시도
4. 접속 성공 시 `connectionInfoText`에 인원 수 표시 확인

---

## 6. 문제 해결

| 증상 | 확인할 것 |
|------|----------|
| "Transport가 EdgegapLobbyKcpTransport가 아닙니다" 에러 | NetworkManager 프리팹에 `EdgegapLobbyKcpTransport`가 아닌 `KcpTransport`가 그대로 있음 |
| "릴레이 서버 할당 대기 중..."에서 넘어가지 않음 | `lobbyUrl`이 비어있는지 확인 → Create & Deploy Lobby 다시 실행 |
| Join 시 "Failed to join lobby" | 로비 코드가 정확한지, 호스트가 아직 방을 열고 있는지 확인 |
| 포트 관련 에러 | EdgegapLobbyKcpTransport는 포트 설정을 무시하고 릴레이 서버가 자동 할당 |
