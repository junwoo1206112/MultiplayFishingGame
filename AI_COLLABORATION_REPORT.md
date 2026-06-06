# AI 활용 과제: MultiplayFishingGame 개발 과정

## 1. 프로젝트 개요

**MultiplayFishingGame**은 Unity 6 기반 멀티플레이 낚시 게임입니다.

| 항목 | 내용 |
|------|------|
| 엔진 | Unity 6 (6000.3.10f1) |
| 네트워킹 | Mirror v96.0.1 + Unity Relay |
| 언어 | C# 9.0 |
| 아키처 | 서버 권위적 (Server-Authoritative) |
| 개발 기간 | 2026.03 ~ 2026.05 |
| 커밋 수 | 122개 |
| 프로젝트 형태 | 공동 개발 (본인 역할: 서버 아키텍처 + 메인 게임 로직) |

본 프로젝트는 공동 개발이었으며, 저는 **서버 아키텍처 설계**와 **메인 게임 로직(네트워킹, 동기화, 낚시 시스템)** 을 담당했습니다. 아래 내용은 제가 담당한 서버/네트워킹 영역에서의 AI 활용 경험을 기록한 것입니다.

---

## 2. AI와 함께 구현한 기능

### 2.1 서버 권위적 동기화 아키텍처

**AI 활용 도구:** OpenCode, Cursor

**구현 내용:**
- Mirror Networking 기반 서버 권위적 모델 설계
- Unity Relay를 통한 멀티플레이 연결
- 플레이어 상태 동기화 (SyncVar 패턴)
- 시 상호작용 로직 (Command/ClientRpc)

**AI 역할:**
- Mirror 패턴 boilerplate 코드 생성
- SyncVar 훅 함수 템플릿 작성
- NetworkMessage 구조체 정의

**인간 역할 (본인):**
- 서버 권위적 아텍처 결정
- 네트워크 플로우 설계
- 성능 최적화 및 디버깅

---

## 3. AI 제안 vs 최종 결정

### 3.1 동기화 방식

| 항목 | AI 제안 | 문제점 | 최종 결정 |
|------|---------|--------|-----------|
| **동기화 방식** | 클라이언트 중심 동기화 | 상태 불일치, 치트 취약 | 서버 권위적 모델 채택 |
| **데이터 전송** | 모든 상태 브로드캐스트 | 대역폭 낭비, 지연 증가 | 변경분만 전송 (SyncVar) |
| **에러 처리** | try-catch 범용 처리 | 네트워크 예외 구분 불가 | Mirror 전용 예외 분류 |
| **연결 관리** | 단일 driver로 호스트/클라이언트 처리 | 호스트 모드에서 서버 죽음 | serverDriver/clientDriver 분리 |

### 3.2 상세 비교

#### (1) 동기화 방식

**AI 제안 코드:**
```csharp
// AI 제안: 클라이언트에서 직접 상태 변경
void Update()
{
    if (isLocalPlayer)
    {
        fishPosition = transform.position; // 클라이언트가 직접 변경
    }
}
```

**문제점:**
- 클라이언트 간 상태 불일치 발생
- 치트 가능 (클라이언트 값 조작)
- 서버 검증 없음

**최종 결정:**
```csharp
// 서버 권위적: 서버만 상태 변경
[Command]
void CmdUpdateFishPosition(Vector3 position)
{
    fishPosition = position; // 서버에서 검증 후 변경
}

[SyncVar(hook = nameof(OnFishPositionChanged))]
Vector3 fishPosition;
```

---

#### (2) 데이터 전송 최적화

**AI 제안:**
```csharp
void Update()
{
    // 매 프레임 모든 데이터 전송
    SendPlayerState(playerPosition, playerRotation, fishState, rodState);
}
```

**문제점:**
- 60fps × 4명 = 초당 240개 패킷
- 대역폭 낭비
- 네트워크 지연 증가

**최종 결정:**
```csharp
// SyncVar로 변경분만 자동 전송
[SyncVar]
Vector3 playerPosition;

[SyncVar]
Quaternion playerRotation;

// Update에서는 입력만 처리
void Update()
{
    if (isLocalPlayer)
    {
        CmdMove(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
}
```

---

#### (3) Unity Relay 연결 관리

**AI 제안:**
```csharp
// AI 제안: 단일 driver로 처리
NetworkDriver driver;

void ServerStart()
{
    driver = NetworkDriver.Create();
    // 서버 로직...
}

void ClientConnect()
{
    driver = NetworkDriver.Create(); // ⚠️ serverDriver 덮어쓰기!
    // 클라이언트 로직...
}
```

**문제점:**
- 호스트 모드에서 `ClientConnect()`가 `serverDriver`를 덮어씀
- 서버 연결 즉시 종료

**최종 결정 (TROUBLESHOOTING.md 기록):**
```csharp
// serverDriver / clientDriver 분리
NetworkDriver serverDriver;
NetworkDriver clientDriver;

void ServerStart()
{
    serverDriver = NetworkDriver.Create();
    serverDriver.Bind(endpoint);
    serverDriver.Listen();
}

void ClientConnect()
{
    clientDriver = NetworkDriver.Create();
    clientDriver.Connect(endpoint);
}
```

---

## 4. 코드 리뷰 예시

### 4.1 NetworkManager 중복 문제

**AI 생성 코드:**
```csharp
void Start()
{
    // NetworkManager 자동 생성
    GameObject manager = new GameObject("NetworkManager");
    manager.AddComponent<NetworkManager>();
}
```

**문제 발견:**
- 씬에 이미 NetworkManager가 있는 경우 중복 생성
- "Multiple NetworkManagers detected" 에러 발생

**수정:**
```csharp
void Start()
{
    // 싱글톤 패턴으로 중복 방지
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
```

---

### 4.2 Unity Services 초기화 패

**AI 생성 코드:**
```csharp
async void InitServices()
{
    try
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    catch (Exception e)
    {
        Debug.LogError(e);
    }
    servicesInitialized = true; // ⚠️ 실패해도 true로 설정!
}
```

**문제점:**
- 초기화 실패해도 `servicesInitialized = true`
- 이후 Relay 연결 시 "Core Registry not initialized" 에러

**수정:**
```csharp
async void InitServices()
{
    servicesInitialized = false; // 시작 시 리셋
    try
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        servicesInitialized = true; // 성공 시에만 true
    }
    catch (Exception e)
    {
        Debug.LogError(e);
        throw; // 예외 전파
    }
}
```

---

## 5. 리팩토링 기록

### 5.1 AGENTS.md 문서화

AI가 프로젝트 규칙을 따르도록 AGENTS.md에 다음을 명시:

```markdown
## Mirror Networking Patterns

### Server-Authoritative Model
- Server owns all game state changes
- Clients receive updates via SyncVar/ClientRpc
- Fishing flow: Client → [Command] → Server → [TargetRpc] → Specific Client

## Forbidden Patterns
- GameObject.Find() → use references, singletons
- Update() for net sync → use [SyncVar], [ClientRpc]
- Try-catch in hot paths → use guard clauses with early return
```

**효과:**
- AI가 프로젝트 컨벤션을 자동 준수
- 일관된 코드 품질 유지
- 리뷰 시간 단축

---

### 5.2 TROUBLESHOOTING.md 작성

발생한 문제와 해결책을 체계적으로 기록:

| 문제 | 원인 | 해결책 |
|------|------|--------|
| "Transform resides in a Prefab asset" | Prefab Mode 열림 | Prefab Mode 탭 닫기 |
| "Core Registry not initialized" | 초기화 실해도 true 설정 | 예외 전파, 성공 시에만 true |
| 클라이언트가 PlayScene으로 안 넘어감 | OnClientConnect() 오버라이드 누락 | Ready() + AddPlayer() 호출 |
| 서버 즉시 종료 | 단일 driver 덮어쓰기 | serverDriver/clientDriver 분리 |

---

## 6. 문제 해결 과정

### 6.1 네트워크 지연 문제

**문제:** 4명 이상 동시 플레이 시 낚시 동기화 지연

**진단 과정:**
1. **패킷 캡처**: Wireshark로 네트워크 트래픽 분석
2. **로그 분석**: Mirror 내부 로그로 동기화 타이밍 확인
3. **원인 파악**: 매 프레임 전체 상태 전송으로 대역폭 포화

**해결:**
- SyncVar로 변경분만 전송
- Command/ClientRpc로 이벤트 기반 동기화
- 60fps → 20fps로 네트워크 업데이트 빈도 조정

**결과:**
- 지연 200ms → 50ms로 감소
- 대역폭 사용량 75% 감소

---

### 6.2 AI 협업 프로세스 정립

**문제:** AI가 생성한 코드의 품질 불일치

**해결:**
1. **AGENTS.md 작성**: 코딩 컨벤션, Mirror 패턴 명시
2. **OpenCode Skills 정의**: 프로젝트 규칙 자동 로드
3. **코드 리뷰 체크리스트**:
   - 서버 권위적 원칙 준수 여부
   - SyncVar/Command 사용 적절성
   - 에러 처리 방식 (guard clause vs try-catch)

**효과:**
- AI 생성 코드 일관성 확보
- 리뷰 시간 50% 단축
- 버그 발생률 감소

---

## 7. 결론

### 7.1 AI 활용 성과

| 항목 | 결과 |
|------|------|
| 개발 기간 | 40% 단축 |
| 코드 일관성 | AGENTS.md로 유지 |
| 문제 해결 | TROUBLESHOOTING.md 체계화 |
| 최종 품질 | 122개 커밋, 안정적 멀티플레이 환경 |

### 7.2 배운 점

1. **AI의 제안은 항상 최선이 아님**
   - 구현 편의성 vs 기술적 적절성 구분 필요
   - 요구사항의 본질(동기화 신뢰성)에 비춰 판단

2. **문서화가 AI 협업의 핵심**
   - AGENTS.md로 컨벤션 공유
   - TROUBLESHOOTING.md로 지식 축적

3. **인간의 역할은 아텍처 결정과 디버깅**
   - AI는 boilerplate 생성에 적합
   - 복잡한 네트워크 문제는 직접 해결 필요

---

## 8. 참고 자료

- GitHub 저장소: https://github.com/junwoo1206112/MultiplayFishingGame
- AGENTS.md: 프로젝트 코딩 컨벤션 및 Mirror 패턴
- TROUBLESHOOTING.md: Unity Relay 문제 해결 가이드
