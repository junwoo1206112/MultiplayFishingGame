## Context

멀티플레이어 환경에서 마스터 데이터의 일관성과 사용자 데이터의 영속성은 매우 중요합니다. 특히 싱글톤 패턴은 대규모 프로젝트에서 의존성 꼬임과 테스트 난항을 유발하므로 인터페이스 기반의 DI 컨테이너를 도입합니다.

## Goals / Non-Goals

**Goals:**
- NPOI를 사용하여 `.xlsx` 파일을 직접 핸들링.
- 엑셀 데이터 -> ScriptableObject(SO) 자동 변환.
- 도감(Encyclopedia)은 판매 여부와 관계없이 영구 저장.
- 모든 매니저는 인터페이스를 통해 접근.

**Non-Goals:**
- 외부 DB(MySQL/Firebase) 연동 (이번 단계에서는 로컬 JSON 파일 저장만 수행).
- 복잡한 DI 라이브러리(Zenject/VContainer) 사용 (단순한 커스텀 DIContainer 구현).

## Decisions

### 1. DI Framework (Manual/ServiceLocator)
**Decision:** `DIContainer` 클래스를 통해 앱 초기화 시점에 서비스 등록 및 `Inject()` 수행.
- **Rationale:** 싱글톤 패턴을 완전히 배제하고 인터페이스 기반의 확장을 보장함.

### 2. Excel Integration (NPOI ONLY)
**Decision:** NPOI 라이브러리를 사용하여 `.xlsx` 파일을 직접 처리.
- **Rationale:** 기획자가 친숙한 엑셀 환경에서 복잡한 데이터(확률, 등급 등)를 직접 관리함. **CSV 방식은 고려하지 않음.**

### 3. Fish Data Structure
**Decision:** ID, 이름, 등급(A~E), 확률(%), 가격, 길이를 포함한 `FishDataSO`와 `UserSaveData` 연동.
- **Rationale:** 도감 데이터의 영속성(판매 후에도 유지)을 보장하는 비즈니스 로직 적용.

## Risks / Trade-offs

- **[Risk]** NPOI DLL 충돌 가능성.
  - **Mitigation:** Unity 6 호환 버전(sarmalev2) 사용 및 `Plugins` 폴더에 격리.
- **[Risk]** 도감 데이터 유실 우려.
  - **Mitigation:** 인벤토리와 도감 데이터를 별도의 컬렉션으로 분리하여 관리.
