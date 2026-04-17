# Tasks

## 1. 채널 시스템 기반
- [ ] 1.1 ChannelData ScriptableObject 생성 (`Assets/Scripts/Data/ChannelData.cs`)
- [ ] 1.2 채널 ScriptableObject 에셋 3개 생성 (초보자/일반/숙련자)
- [ ] 1.3 ChannelSelectMessage NetworkMessage 구조체 정의
- [ ] 1.4 서버 채널 초기화 로직 구현

## 2. 방 관리 시스템 (NetworkRoomManager 기반)
- [ ] 2.1 FishingRoomManager 생성 — NetworkRoomManager 상속 (`Assets/Scripts/Network/FishingRoomManager.cs`)
- [ ] 2.2 방 생성 로직 구현 (RoomCreateMessage 처리)
- [ ] 2.3 방 참여 로직 구현 (비밀번호 검증 포함)
- [ ] 2.4 방 나가기 로직 구현 (방장 이양 포함)
- [ ] 2.5 방 목록 동기화 구현 (SyncList 또는 ClientRpc)
- [ ] 2.6 방 설정 변경 로직 구현

## 3. 준비 시스템 (NetworkRoomPlayer 기반)
- [ ] 3.1 FishingRoomPlayer 생성 — NetworkRoomPlayer 상속 (`Assets/Scripts/Gameplay/FishingRoomPlayer.cs`)
- [ ] 3.2 준비 상태 토글 Command+Rpc 구현
- [ ] 3.3 게임 시작 권한 검증 (방장 + 전원 준비 + 최소 2명)
- [ ] 3.4 씬 전환 로직 구현 (Lobby → FishingLake)

## 4. UI 구현
- [ ] 4.1 채널 선택 UI 패널 (`Assets/Scripts/UI/ChannelSelectUI.cs`)
- [ ] 4.2 로비/방 목록 UI 패널 (`Assets/Scripts/UI/LobbyUI.cs`)
- [ ] 4.3 방 생성 다이얼로그 UI (`Assets/Scripts/UI/CreateRoomDialog.cs`)
- [ ] 4.4 대기실 UI 패널 (`Assets/Scripts/UI/RoomUI.cs`)
- [ ] 4.5 NetworkHUD → 채널/로비/대기실 흐름으로 교체

## 5. 씬 및 프리팹 설정
- [ ] 5.1 Offline 씬 생성 (접속 UI)
- [ ] 5.2 Lobby 씬 업데이트 (채널/로비/대기실)
- [ ] 5.3 FishingLake 씬 생성 (게임플레이)
- [ ] 5.4 FishingRoomPlayer 프리팹 생성 및 NetworkManager에 등록
- [ ] 5.5 Build Settings에 씬 등록

## 6. 기존 코드 리팩토링
- [ ] 6.1 FishingNetworkManager → FishingRoomManager로 대체
- [ ] 6.2 FishingPlayer → FishingRoomPlayer + 실제 게임플레이어로 분리
- [ ] 6.3 NetworkHUD 제거 (새 UI로 대체)