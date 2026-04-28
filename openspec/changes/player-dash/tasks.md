## 1. Mirror PlayerControllerReliable 활성화

- [x] 1.1 PlayerControllerBase 소스 분석 및 동작 방식 확인
- [x] 1.2 프리팹 YAML에서 PlayerControllerReliable m_Enabled 0→1 변경
- [x] 1.3 FishingPlayer.OnStartLocalPlayer()에서 PlayerController 활성화 로직 추가

## 2. FishingPlayer — 대시 기능 구현

- [x] 2.1 대시 설정 필드 추가: dashDistance, dashDuration, dashCooldown
- [x] 2.2 대시 서버 검증용 [Command] CmdDash(Vector3 direction) 구현
- [x] 2.3 서버에서 쿨타임 체크 후 RpcPerformDash로 전파
- [x] 2.4 [ClientRpc] RpcPerformDash에서 모든 클라이언트 동기화
- [x] 2.5 DashRoutine 코루틴: CharacterController.Move() 기반 6유닛/0.25초 이동

## 3. 입력 처리 및 낚시 연동

- [x] 3.1 Left Shift 키 입력 처리 (Keyboard.current.leftShiftKey)
- [x] 3.2 낚시 중 대시 차단: FishingController.CurrentState != Idle 체크
- [x] 3.3 대시 중 중복 입력 방지: isDashing 플래그
- [x] 3.4 대시 쿨타임 공개 API: IsDashCooldownReady(), GetDashCooldownProgress()

## 4. 벽 충돌 처리

- [x] 4.1 Raycast로 대시 경로상 벽 감지 후 충돌 지점 앞까지 이동
