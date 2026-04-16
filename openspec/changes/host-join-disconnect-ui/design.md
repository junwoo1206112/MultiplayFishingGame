## Context

MultiplayFishingGame은 Mirror Networking v96.0.1 기반 멀티플레이 낚시 게임. 현재 FishingNetworkManager는 기본 연결 콜백만 있고, NetworkHUD는 IMGUI 기반이다. kart-mirror-sample 프로젝트의 NetworkMenuUI 패턴(이벤트 기반, TMP UI, 상태 표시)을 참고하여 TMP 기반 UI로 전환한다.

제약사항:
- Mirror의 NetworkManager.mode (Offline/Host/ClientOnly/ServerOnly)로 상태 판별
- KCP Transport 사용
- TMP(TextMeshPro) 사용 (레거시 UI Text 금지)
- 서버 권한: 연결/해제는 NetworkManager 메서드로 처리

## Goals / Non-Goals

**Goals:**
- FishingNetworkManager에 정적 이벤트와 프로퍼티 추가하여 UI가 상태 변화를 감지
- NetworkMenuUI 컴포넌트로 호스트/조인/연결 끊기 기능 제공
- 호스트 모드에서 내 IP 주소 표시하여 다른 플레이어가 접속 가능
- 연결 상태(호스트/클라이언트/오프라인)와 접속 인원 표시
- kart-mirror-sample의 NetworkMenuUI 패턴과 유사한 구조

**Non-Goals:**
- 채널 시스템, 방 시스템 (별도 OpenSpec 변경으로 관리)
- 씬 전환 (Lobby → FishingLake)
- 인증 시스템
- 채팅 기능

## Decisions

### 1. IMGUI → TMP UI 전환

**결정**: NetworkHUD(IMGUI)를 NetworkMenuUI(TMP 기반)로 완전 교체

**이유**: IMGUI는 런타임 전용이고 커스터마이징이 제한적. TMP는 시각적 품질이 좋고 에디터에서 프리팹으로 관리 가능. kart-mirror-sample에서 이미 검증된 패턴.

**대안**: IMGUI 유지 → 빠르지만 팀 협업과 UI 품질에 제한

### 2. 이벤트 기반 상태 갱신

**결정**: `FishingNetworkManager.NetworkStateChanged` 정적 이벤트로 UI가 구독

**이유**: Update() 폴링 대신 이벤트 기반이면 불필요한 매 프레임 갱신 방지. kart-mirror-sample에서 `KartNetworkManager.NetworkStateChanged` 패턴 검증됨.

### 3. 로컬 IP 표시

**결정**: `System.Net.Dns.GetHostAddresses`로 로컬 IPv4 주소 조회

**이유**: 호스트가 다른 플레이어에게 IP를 알려주어야 접속 가능. NAT/공유기 환경에서는 포트 포워딩이 필요하지만, LAN 환경에서는 직접 접속 가능.

### 4. Mirror 네트워킹 패턴

**표준 컨트롤러 사용**: 커스텀 이동 스크립트 대신 Mirror 예제의 `PlayerController (Reliable)`를 사용한다. 이는 물리 충돌과 네트워크 권한 처리에 있어 검증된 안정성을 제공하기 위함이다.

**카메라 자동화**: 플레이어 스폰 시 시네머신과의 연결은 `FishingCameraFollow` 보조 스크립트가 담당한다.

**SyncVar 사용 안 함**: UI 상태는 NetworkBehaviour가 아니므로 SyncVar 불필요. NetworkManager.mode와 NetworkServer/Client 상태로 직접 판별.

**Command/Rpc 사용 안 함**: 연결/해제는 NetworkManager.StartHost/StartClient/StopHost/StopClient로 처리. 서버-클라이언트 간 추가 메시지 불필요.

## Risks / Trade-offs

- **[TMP 의존성]** → Unity 6에서 TMP가 기본 패키지이므로 문제 없음
- **[IP 표시 정확성]** → 여러 네트워크 인터페이스가 있으면 잘못된 IP 표시 가능. 가장 첫 번째 비-루프백 IPv4를 반환하므로 대부분의 경우 정상 동작
- **[연결 중 상태 표시]** → Mirror의 NetworkClient.active로 연결 중 판별. 연결 타임아웃 시 UI가 멈출 수 있으나 Mirror가 자동 처리