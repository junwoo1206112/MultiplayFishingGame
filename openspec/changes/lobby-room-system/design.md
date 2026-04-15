## Context

MultiplayFishingGame은 Mirror Networking v96.0.1 기반 클라이언트-서버 멀티플레이 낚시 게임. 현재 FishingNetworkManager는 단순 호스트/클라이언트 연결만 지원하며, 채널 선택이나 방 생성 기능이 없음. Mirror의 NetworkRoomManager와 NetworkRoomPlayer를 기반으로 채널-로비-방 흐름을 구현.

제약사항:
- Mirror v96.0.1의 NetworkRoomManager, NetworkRoomPlayer 활용
- 서버 권한: 방 생성/시작은 서버에서 검증
- 최대 4플레이어 per room (FishingNetworkManager.maxPlayers 설정)
- KCP Transport 사용

## Goals / Non-Goals

**Goals:**
- 채널 선택 → 로비 → 방 대기 → 게임 시작 흐름 구현
- 방 생성/참여/나가기 기능
- 방장 전용 게임 시작 권한
- 모든 플레이어 준비 시 게임 시작 가능
- 방 설정: 이름, 최대 인원, 공개/비공개, 비밀번호

**Non-Goals:**
- 매치메이킹/자동 방 매칭 (이후 구현)
- 친구 목록/초대 시스템
- 채팅 시스템
- 게임 씬 전환 후의 실제 낚시 플레이
- 서버 브라우저 (마스터 서버 없이 LAN만)

## Decisions

### 1. NetworkRoomManager vs 커스텀 구현

**결정**: Mirror의 `NetworkRoomManager`를 상속하여 구현

**이유**: NetworkRoomManager는 이미 방 관리, 준비 상태, 씬 전환 로직을 제공. 처음부터 만들면 NetworkManager의 연결/스폰 로직을 재구현해야 함.

**대안**: 순수 커스텀 NetworkManager + NetworkMessage로 방 시스템 구현 → 더 유연하지만 구현량이 많고 Mirror의 검증된 방 시스템을 재발명해야 함.

### 2. 채널 시스템 방식

**결정**: ScriptableObject 기반 정적 채널 목록 + NetworkMessage로 브로드캐스트

**이유**: 초기 버전에서는 마스터 서버가 없으므로, 채널을 사전 정의된 카테고리(초보자/일반/숙련자)로 제공. 런타임에 채널 목록을 SyncList로 동기화.

```
채널 구조 (ScriptableObject):
- ChannelData (ScriptableObject)
  - string channelId      // "beginner", "normal", "expert"
  - string displayName    // "초보자 채널", "일반 채널", "숙련자 채널"
  - int maxRooms          // 최대 방 수
  - Color32 color         // UI 색상
```

### 3. 방 데이터 동기화

**결정**: NetworkRoomManager의 기본 SyncVar + 커스텀 SyncVar 추가

**SyncVar 전략**:
```
FishingRoomPlayer (NetworkRoomPlayer):
  - [SyncVar(hook=nameof(OnPlayerNameChanged))] string playerName
  - [SyncVar(hook=nameof(OnReadyChanged))] bool ready

FishingRoomManager (NetworkRoomManager):
  - [SyncVar] string roomName
  - [SyncVar] int maxPlayers (4)
  - [SyncVar] bool isPublic
  - [SyncVar] string password
```

### 4. 흐름도

```
┌─────────────┐     ┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│  접속 화면  │────▶│  채널 선택  │────▶│    로비      │────▶│   대기실    │
│(NetworkHUD) │     │(채널 목록)  │     │(방 목록/생성)│     │(준비/시작)  │
└─────────────┘     └─────────────┘     └──────────────┘     └─────────────┘
                                                                  │
                                                                  ▼
                                                          ┌─────────────┐
                                                          │  게임 씬    │
                                                          │(FishingLake)│
                                                          └─────────────┘
```

씬 구조:
- `Offline` 씬: 타이틀, 접속 UI
- `Lobby` 씬: 채널 선택 + 방 목록 + 방 생성 + 대기실
- `FishingLake` 씬: 실제 낚시 게임플레이

### 5. NetworkMessage 구조체

```csharp
public struct ChannelSelectMessage : NetworkMessage
{
    public string channelId;
}

public struct RoomCreateMessage : NetworkMessage
{
    public string roomName;
    public int maxPlayers;
    public bool isPublic;
    public string password;
}

public struct RoomListUpdateMessage : NetworkMessage
{
    // 서버 → 클라이언트: 방 목록 동기화
    public string roomName;
    public int currentPlayerCount;
    public int maxPlayers;
    public bool isPublic;
    public bool isInProgress;
}
```

## Risks / Trade-offs

- **[NetworkRoomManager 커스터마이징 제한]** → NetworkRoomManager의 기본 씬 전환 로직을 오버라이드해야 함. 커스텀 로직이 많아지면 차라리 순수 커스텀 구현이 나을 수 있음 → 초기에는 NetworkRoomManager 기반으로 시작, 필요시 리팩토링
- **[방 목록 동기화 비용]** → 방이 많아지면 SyncList 브로드캐스트 비용 증가 → 초기에는 방 수가 적으므로 문제 없음, 이후 관심영역(Interest Management) 도입으로 최적화
- **[비밀번호 보안]** → 비밀번호가 평문 NetworkMessage로 전송됨 →初期에는 문제 없으나 프로덕션에서는 해시 필요