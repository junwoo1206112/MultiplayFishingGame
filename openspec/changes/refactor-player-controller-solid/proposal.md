## Why

현재 `SampleSceneLocalPlayerController`는 591줄에 달하는 거대한 클래스로, 플레이어 이동, 회전, 애니메이션 제어, 낚시 입력 처리, 훅 애니메이션, 물 표면 감지, 파티클 효과, Rope 길이 조절 등 너무 많은 책임을 가지고 있어 단일 책임 원칙(SRP)을 위반하고 있습니다. 이는 코드의 가독성을 떨어뜨리고, 테스트와 유지보수를 어렵게 만들며, 컴포넌트 재사용을 방해합니다.

## What Changes

- **PlayerMovement** 신규: CharacterController 기반 플레이어 이동 및 회전 담당
- **PlayerAnimation** 신규: Animator 제어 및 파라미터 관리 담당
- **FishingController** 신규: 낚시 상태 관리 및 입력 처리 담당
- **HookAnimator** 신규: 훅 이동 및 베지어 곡선 궤적 계산 담당
- **WaterDetector** 신규: Raycast 기반 물 표면 감지 담당
- **SplashEffect** 신규: 파티클 효과 생성 및 위치 관리 담당
- **FishingRope** 신규: Rope 컴포넌트 길이 조절 (Reflection 사용)
- **SampleSceneLocalPlayerController** 수정: 위 컴포넌트들을 조립하는 Facade 패턴 적용

## Capabilities

### New Capabilities

- `player-movement`: CharacterController 기반 이동 및 마우스 회전
- `player-animation`: Animator 파라미터 해시 캐싱 및 Walk/Fishing 상태 제어
- `fishing-controller`: 낚시 토글 입력 처리 및 상태 전환
- `hook-animator`: 베지어 곡선을 이용한 훅 투척/회수 애니메이션
- `water-detector`: 카메라/캐릭터 기반 Raycast 물 표면 감지
- `splash-effect`: 물 표면 파티클 효과 생성
- `fishing-rope`: Reflection을 통한 Rope 길이 동적 조절

### Modified Capabilities

- `player-controller-facade`: 기존 `SampleSceneLocalPlayerController`가 Facade로 리팩토링되어 각 컴포넌트를 조립

## Impact

- `SampleSceneLocalPlayerController.cs`: 기존 로직 제거 후 Facade 패턴 적용
- 새 파일 7개 생성 (위 New Capabilities 목록)
- Player 프리팹 구조 변경: 기존 단일 스크립트 → 여러 컴포넌트로 분리
- Inspector 설정 변경: SerializeField가 분산됨
- **BREAKING**: 외부에서 직접 `SampleSceneLocalPlayerController` 접근하는 코드 수정 필요
