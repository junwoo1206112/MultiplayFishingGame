# Unity Relay — 셋업 가이드

외부 IP에서도 접속 가능한 멀티플레이 (완전 무료, 50 CCU 이하)

---

## 1. Unity Editor에서 패키지 설치

Unity 6000.3.10f1에서 다음 패키지들이 `manifest.json`에 추가되어 있습니다:

```
com.unity.transport
com.unity.services.relay
com.unity.services.core
com.unity.services.authentication
```

**Unity Editor를 열면 자동으로 패키지가 다운로드/설치됩니다.**
만약 설치 실패 시, 메뉴에서 **Window → Package Manager**로 이동 후 각 패키지의 호환 버전을 확인하세요.

| 패키지 | 최소 버전 |
|--------|-----------|
| `com.unity.transport` | 3.2.0+ |
| `com.unity.services.relay` | 1.1.1+ |
| `com.unity.services.core` | 1.14.0+ |
| `com.unity.services.authentication` | 3.4.0+ |

---

## 2. Unity Cloud 프로젝트 설정

1. https://cloud.unity.com 접속
2. **Create Project** → 프로젝트 이름 입력
3. 생성된 프로젝트 ID 복사
4. Unity Editor → **Edit → Project Settings → Services** (태블릿 아이콘)
5. **Use an existing Unity project ID** → 복사한 ID 붙여넣기 → **Link project ID**
6. Services 탭 → **Relay → Enable** (Relay 서비스 활성화)
7. 동일하게 **Authentication → Enable** (익명 인증 필요)

> 신용카드 등록이 필요할 수 있으나, 50 CCU 이하(2~4인 기준)는 **완전 무료**입니다.
> 등록해도 50 CCU를 넘지 않으면 과금되지 않습니다.

---

## 3. NetworkManager 프리팹 Transport 교체

| 프리팹 | 경로 |
|--------|------|
| `NetworkManager 1.prefab` | `Assets/Prefabs/Manager/` |

1. 프리팹 열기 (Lobby 씬에서 사용 중)
2. `EdgegapLobbyKcpTransport` 또는 `KcpTransport` **Remove Component**
3. **Add Component** → `UnityRelayTransport` 검색 후 추가
4. 추가 설정 불필요 (maxPacketSize = 1400 기본값)

---

## 4. UI 텍스트 변경 (Panel_Offline.prefab / Panel_Online.prefab)

| 위치 | 변경 전 | 변경 후 |
|------|---------|--------|
| `InputField_Address` placeholder | `"IP 주소 입력..."` | `"참가 코드 입력"` |
| `Button_CopyIP` 텍스트 | `"내 IP 복사하기"` | `"참가 코드 복사하기"` |

---

## 5. 연결 플로우

### 호스트 (방 만들기)
1. 이름 입력 → **"방만들기 (host)"** 클릭
2. Unity Services 초기화 → Relay 할당 → joinCode 생성 (async, 1~3초)
3. 화면에 **참가 코드** 표시됨
4. **"참가 코드 복사하기"** 버튼으로 클립보드 복사
5. 친구에게 코드 전달

### 클라이언트 (참가하기)
1. 이름 입력
2. **참가 코드 입력** 필드에 코드 입력
3. **"참가하기 (join)"** 클릭
4. Relay join → 서버 접속

---

## 6. 코드 구조

| 파일 | 역할 |
|------|------|
| `Assets/Scripts/Network/UnityRelayTransport.cs` | Unity Relay + UTP Transport |
| `Assets/Scripts/Network/FishingRoomManager.cs` | `CreateRoom()` / `JoinRoom()` async Relay 호출 |
| `Assets/Scripts/UI/NetworkMenuUI.cs` | UI → Manager 연결 |

---

## 7. 문제 해결

| 증상 | 확인할 것 |
|------|----------|
| `Unity.Services.Core` 관련 컴파일 에러 | 패키지가 정상 설치되었는지 Package Manager에서 확인 |
| `RelayService.Instance.CreateAllocationAsync` 실패 | Unity Cloud Dashboard에서 Relay가 **Enabled** 상태인지 확인 |
| `AuthenticationService` 실패 | Cloud Dashboard에서 Anonymous Authentication이 **Enabled** 상태인지 확인 |
| "Transport가 UnityRelayTransport가 아닙니다" | NetworkManager 프리팹에서 Transport가 UnityRelayTransport로 바뀌었는지 확인 |
| 접속은 되는데 플레이어가 안 보임 | Mirror가 올바른 NetworkManager 인스턴스를 사용하는지 확인 |

---

## 8. 빌드 테스트

1. **File → Build Profiles** → Windows, Mac, or Linux
2. **Build** → 빌드 완료 후 실행
3. 첫 실행 시 Unity Services 초기화에 시간이 걸릴 수 있음 (Log 확인)
4. 두 번째 창도 실행 (또는 다른 PC)
5. 한쪽은 Host, 다른 쪽은 Join Code로 접속 테스트
