## Context

현재 `SampleSceneLocalPlayerController`는 로컬 플레이어의 모든 동작을 하나의 클래스에서 처리하고 있습니다. 이 클래스는 591줄에 달하며 다음 책임들을 포함하고 있습니다:

1. **이동/회전**: CharacterController를 사용한 플레이어 이동 및 마우스 기반 회전
2. **애니메이션 제어**: Walk/Fishing 파라미터를 통한 Animator 제어
3. **낚시 입력**: 마우스 좌클릭으로 낚시 상태 토글
4. **훅 애니메이션**: 베지어 곡선을 이용한 훅 투척/회수 애니메이션
5. **물 표면 감지**: Raycast를 통한 물 표면 위치 감지
6. **파티클 효과**: 물 표면에 닿을 때 스플래시 파티클 생성
7. **Rope 길이 조절**: Reflection을 사용하여 외부 Rope 컴포넌트의 길이 동적 조절

이러한 단일 클래스 구조는 테스트하기 어렵고, 특정 기능만 재사용하기 어렵습니다.

## Goals / Non-Goals

**Goals:**
- 단일 클래스의 책임을 7개의 독립적인 컴포넌트로 분리
- 각 컴포넌트는 하나의 명확한 책임만 가짐 (SRP)
- 컴포넌트 간 느슨한 결합 (컴포넌트 간 직접 참조 최소화)
- 기존 Player 프리팹과 동일한 기능 유지
- Inspector에서 각 컴포넌트의 설정을 독립적으로 조정 가능

**Non-Goals:**
- 네트워크 동기화 (Mirror) 적용 - 로컬 플레이어 전용 유지
- Input System을 Unity Events 기반으로 변경
- Rope 컴포넌트의 직접적인 수정 (Reflection 사용 유지)
- 외부 API 변경 없음 (public 메서드 시그니처 유지)

## Decisions

### 1. Facade 패턴 적용

**Decision:** 기존 `SampleSceneLocalPlayerController`를 Facade로 유지하고, 내부 로직을 각 컴포넌트에 위임

**Rationale:**
- 기존 코드에서 이 클래스를 참조하는 외부 코드가 있을 수 있음
- Breaking change 최소화
- Facade가 컴포넌트들의 초기화 순서와 의존성을 관리

**Alternative:** 완전히 제거하고 각 컴포넌트를 독립적으로 배치
- Rejected: Breaking change가 너무 큼, 마이그레이션 비용 높음

### 2. Unity Events를 통한 컴포넌트 간 통신

**Decision:** 컴포넌트 간 직접 참조 대신 Unity Events 사용

**Rationale:**
- 느슨한 결합 유지
- Inspector에서 연결 관계를 시각적으로 확인 가능
- 테스트 시 Mock 컴포넌트로 쉽게 대체 가능

**예시:**
```csharp
// FishingController
event Action<bool> OnFishingStateChanged;

// HookAnimator는 Inspector에서 이 이벤트 구독
```

**Alternative:** 직접 컴포넌트 참조 (`GetComponent<T>()`)
- Rejected: 강한 결합, 테스트 어려움

### 3. 컴포넌트 계층 구조

**Decision:** Player 프리팹 구조 유지, 컴포넌트 추가 방식

```
Player (GameObject)
├── CharacterController
├── Animator
├── FishingLineVisual
├── SampleSceneLocalPlayerController (Facade)
├── PlayerMovement (NEW)
├── PlayerAnimation (NEW)
├── FishingController (NEW)
├── HookAnimator (NEW)
├── WaterDetector (NEW)
├── SplashEffect (NEW)
└── FishingRope (NEW)
```

**Rationale:**
- 기존 하이어라키 구조 변경 없음
- Facade가 모든 컴포넌트의 참조를 가지고 조정

### 4. Reflection 사용 유지

**Decision:** `FishingRope`에서 Reflection을 통한 Rope 컴포넌트 조작 유지

**Rationale:**
- Rope 컴포넌트는 외부 에셋(GogoGaga)이며 소스 코드 수정 불가
- 기존 동작 유지 필요

**Alternative:** Rope 컴포넌트를 직접 수정하거나 래퍼 클래스 생성
- Rejected: 외부 에셋 수정은 유지보수 문제 발생

## Risks / Trade-offs

**[성능 저하]** → 컴포넌트 분리로 인한 GetComponent 호출 증가
- **Mitigation:** Facade에서 Awake/Start에서 한 번만 캐싱, 이후 필드 직접 접근

**[복잡성 증가]** → 7개의 새 파일과 컴포넌트 관리
- **Mitigation:** 명확한 네이밍과 폴더 구조, 각 컴포넌트는 단일 책임만 가짐

**[Inspector 설정 복잡화]** → SerializeField가 7개 컴포넌트로 분산
- **Mitigation:** Facade에서 주요 설정만 노출, 세부 설정은 각 컴포넌트에서
- 기존 설정값은 Facade에서 각 컴포넌트에 전달하는 방식으로 마이그레이션

**[Regression Risk]** → 기존 동작과 동일하게 작동하는지 검증 필요
- **Mitigation:** 
  1. 기존 코드를 완전히 분석하여 모든 동작 파악
  2. 각 컴포넌트 단위로 테스트
  3. 통합 테스트로 전체 시나리오 검증

## Migration Plan

### Phase 1: 컴포넌트 생성 (순차적)
1. `PlayerMovement` 생성 및 테스트
2. `PlayerAnimation` 생성 및 테스트
3. `WaterDetector` 생성 및 테스트
4. `SplashEffect` 생성 및 테스트
5. `FishingRope` 생성 및 테스트
6. `HookAnimator` 생성 및 테스트
7. `FishingController` 생성 및 테스트

### Phase 2: Facade 리팩토링
8. `SampleSceneLocalPlayerController`에서 로직 제거
9. Facade가 각 컴포넌트 참조하도록 수정
10. Facade가 컴포넌트 간 이벤트 연결 담당

### Phase 3: Player 프리팹 업데이트
11. Player 프리팹에 새 컴포넌트 7개 추가
12. Inspector에서 각 컴포넌트 설정값 입력
13. Facade에서 각 컴포넌트 연결

### Phase 4: 검증
14. 모든 낚시 시나리오 테스트 (캐스팅, 회수, 이동, 회전 등)
15. 성능 프로파일링

## Open Questions

1. **PlayerMovement와 FishingController의 관계:** 낚시 중 이동 금지 로직은 어디에?
   - Proposal: FishingController가 `OnFishingStateChanged` 이벤트 발행, PlayerMovement가 구독하여 이동 제어

2. **WaterDetector의 재사용:** 다른 낚시 관련 기능에서도 물 감지가 필요한가?
   - Proposal: WaterDetector는 범용적으로 설계하여 어디서든 사용 가능하도록

3. **Animation 파라미터 해싱:** PlayerAnimation에서만 처리할 것인가?
   - Proposal: FishingController에서도 fishing 파라미터 설정 필요하므로, Animator 직접 접근 대신 PlayerAnimation 메서드 호출
