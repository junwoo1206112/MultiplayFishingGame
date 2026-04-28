## Context

현재 플레이어 이동은 `CharacterController.Move()`를 통해 WASD 키로만 가능하며 고정 속도(`moveSpeed = 4.5f`)로 움직입니다. 대시 기능은 없으며, 낚시 중에는 `FishingController`가 `isFishingActive` 플래그로 이동을 차단합니다. 네트워크 환경에서는 `FishingPlayer`가 서버 권한 모델로 동작하며, 클라이언트는 `[Command]`를 통해 서버에 요청을 보냅니다.

## Goals / Non-Goals

**Goals:**
- Space 키 입력 시 지정된 거리만큼 빠르게 대시
- 대시 쿨타임 (3초) 으로 남용 방지
- 네트워크 멀티플레이어 환경에서도 대시 동기화 (서버 권한)
- 낚시 중에는 대시 불가
- `SampleSceneLocalPlayerController`에도 동일한 대시 적용

**Non-Goals:**
- 대시 중 무적/충돌 무시 기능
- 다단 대시 / 에어 대시
- 대시 관련 네트워크 메시지 구조체 추가 (SyncVar로 충분)

## Decisions

1. **대시 구현 방식: CharacterController.Move() 직접 이동**
   - 대안: `Rigidbody.AddForce()` → 물리 기반은 예측 어렵고 서버 동기화 복잡
   - 대시는 고정된 방향과 거리로 즉시 이동하는 단순 처리가 적합

2. **네트워크 동기화: `[Command]` 방식 (서버 권한)**
   - 클라이언트가 `CmdDash(Vector3 direction)` 호출 → 서버가 위치 검증 후 이동 → 모든 클라이언트에 자동 동기화 (Transform)
   - 대안: ClientRpc로 모든 클라이언트에 이동 전파 → 중복 처리 가능성 있음
   - `NetworkTransform`가 자체 동기화하므로 서버에서 위치만 변경하면 됨

3. **쿨타임: 서버에서만 관리**
   - 클라이언트는 대시 요청만 보내고, 서버가 쿨타임을 검증
   - 클라이언트는 요청 직후 로컬 쿨타임으로 UI/입력 피드백 처리
   - 부정 행위 방지를 위해 서버 쿨타임이 우선

4. **낚시 상태 체크: FishingController.CurrentState**
   - `FishingState.Idle`이 아닐 때는 대시 입력 무시
   - `FishingPlayer`가 `FishingController` 참조를 가지고 있으므로 서버/클라이언트 모두 체크 가능

## Risks / Trade-offs

- [대시 중 네트워크 지연] → 클라이언트에서 즉시 대시 시작 후 서버 검증 (예측-교정). 지연이 크면 순간이동처럼 보일 수 있음
- [대시 방향 꼬임] → 대시는 현재 `transform.forward` 방향으로 고정. 회전 중 입력 시 방향이 어긋날 수 있으나 이는 허용 (자연스러운 게임플레이)
- [낚시 중 대시 차단] → `FishingController.CurrentState`가 Idle인지 확인. 만약 서버에서 `FishingController`가 없으면 `isFishingActive` 같은 간단한 SyncVar 플래그 사용 고려
