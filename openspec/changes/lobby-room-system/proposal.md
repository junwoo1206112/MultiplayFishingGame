## Why

플레이어가 서버에 접속하자마자 바로 게임에 들어가는 것이 아니라, 채널을 선택하고 로비에서 방을 만들거나 참여한 후 게임을 시작할 수 있는 구조가 필요함. 현재 FishingNetworkManager는 단순 호스트/클라이언트 연결만 지원하며, 멀티플레이 낚시 게임에 필요한 채널-로비-방 흐름이 없음.

## What Changes

- **채널 시스템**: 플레이어가 접속 후 채널(예: 초보자 채널, 일반 채널, 숙련자 채널)을 선택할 수 있는 기능 추가
- **로비 씬**: 채널 선택 후 방 목록을 보고 방 생성/참여/검색할 수 있는 로비 UI 추가
- **방 시스템**: 방장이 방을 생성하고 설정(방 이름, 최대 인원, 비공개/공개, 비밀번호)을 지정할 수 있는 방 기능 추가
- **대기실**: 방에 참여한 플레이어가 준비 상태를 토글하고, 방장이 게임을 시작할 수 있는 대기 기능 추가
- **씬 전환**: 로비 → 대기실 → 게임 씬으로의 전환 흐름 구현
- **BREAKING**: 기존 FishingNetworkManager의 직접 접속 방식이 채널-로비-방 흐름으로 대체됨

## Capabilities

### New Capabilities
- `channel-system`: 채널 선택 기능 — 서버 목록 또는 카테고리 기반 채널 선택, 채널별 플레이어 수 표시
- `lobby-room-system`: 로비 및 방 관리 — 방 생성, 참여, 나가기, 방 목록 조회, 방 설정(최대 인원, 공개/비공개, 비밀번호)
- `ready-system`: 대기실 준비 시스템 — 플레이어 준비 상태 토글, 전원 준비 시 게임 시작 트리거

### Modified Capabilities
- (초기 프로젝트이므로 기존 스펙 없음)

## Impact

- `FishingNetworkManager.cs`: NetworkManagerLobby/RoomManager로 대체 또는 확장 필요
- `FishingPlayer.cs`: 준비 상태(ready) 및 방 정보(runningMatch 등) SyncVar 추가 필요
- `NetworkHUD.cs`: 간단한 HUD → 채널/로비/방 UI로 전면 교체
- 새 씬 추가: `Lobby` 씬에 채널 선택과 로비 UI 통합
- Mirror NetworkRoomManager 및 NetworkRoomPlayer 기반으로 구현 권장
- 새 프리팹: RoomPlayer, 방 설정 UI, 채널 선택 UI