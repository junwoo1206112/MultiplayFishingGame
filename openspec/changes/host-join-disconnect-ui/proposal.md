## Why

현재 MultiplayFishingGame에는 IMGUI 기반의 NetworkHUD만 있고, 다른 플레이어가 호스트의 IP를 입력해서 접속하는 기능과 연결 상태를 시각적으로 보여주는 기능이 없다. kart-mirror-sample 프로젝트의 NetworkMenuUI 패턴을 참고하여, TMP(TextMeshPro) 기반의 호스트/조인/연결 끊기 UI와 네트워크 매니저 이벤트 시스템을 구현해야 한다.

## What Changes

- **FishingNetworkManager 확장**: 정적 `NetworkStateChanged` 이벤트, `ConnectedClientCount` 프로퍼티, `ModeText` 프로퍼티, `GetLocalIPAddress()` 추가
- **NetworkMenuUI 신규 생성**: TMP 기반의 호스트/조인/연결 끊기 UI 컴포넌트. IP 주소 입력, 연결 정보 패널(내 IP, 접속 인원) 포함
- **NetworkHUD 제거**: 기존 IMGUI HUD를 NetworkMenuUI로 대체

## Capabilities

### New Capabilities
- `network-menu-ui`: 호스트 시작, IP 주소 입력 후 조인, 연결 끊기 기능을 제공하는 TMP 기반 UI. 연결 상태 표시(호스트/클라이언트/오프라인), 내 IP 표시, 접속 인원 표시

### Modified Capabilities
- (초기 프로젝트이므로 기존 스펙 없음)

## Impact

- `FishingNetworkManager.cs`: 이벤트 시스템 및 프로퍼티 추가
- `NetworkMenuUI.cs`: 신규 파일 생성
- `NetworkHUD.cs`: 삭제 (NetworkMenuUI로 대체)
- Unity 에디터에서 Canvas + TMP UI 오브젝트 생성 및 Inspector 연결 필요