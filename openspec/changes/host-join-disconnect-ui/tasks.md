# Tasks

## 1. FishingNetworkManager 이벤트 시스템 확장
- [x] 1.1 `NetworkStateChanged` 정적 이벤트 추가
- [x] 1.2 `ConnectedClientCount` 프로퍼티 추가
- [x] 1.3 `ModeText` 프로퍼티 추가
- [x] 1.4 `GetLocalIPAddress()` 정적 메서드 추가
- [x] 1.5 모든 네트워크 콜백에 `NotifyStateChanged()` 호출 추가

## 2. NetworkMenuUI 컴포넌트 구현
- [x] 2.1 NetworkMenuUI.cs 생성 — `MultiplayFishing.UI` 네임스페이스
- [x] 2.2 호스트/조인/연결 끊기 버튼 이벤트 핸들러 구현
- [x] 2.3 IP 주소 입력(TMP_InputField) 처리 구현
- [x] 2.4 오프라인/온라인 UI 패널 전환 로직 구현
- [x] 2.5 연결 정보 표시 로직 구현 (모드, IP, 접속 인원)
- [x] 2.6 `NetworkStateChanged` 이벤트 구독/해제 (OnEnable/OnDisable)

## 3. 기존 코드 정리
- [x] 3.1 NetworkHUD.cs 삭제
- [x] 3.2 NetworkHUD.cs.meta 삭제