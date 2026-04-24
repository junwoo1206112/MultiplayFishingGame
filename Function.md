# Multiplay Fishing Game - 기능 명세서 (Function Specification)

본 문서는 프로젝트의 각 컴포넌트와 시스템이 수행하는 역할을 상세히 기록하여 팀원들 간의 개발 싱크를 맞추기 위해 작성되었습니다.

---

## 1. 핵심 아키텍처 (Core Architecture)

### 1.1 DI Container (`DIContainer.cs`)
- **역할**: 의존성 주입(Dependency Injection) 센터.
- **기능**: 서비스 인터페이스(`IUserService`, `IDataService` 등)와 실제 구현체를 매핑하여 프로젝트 전반에서 싱크톤처럼 접근할 수 있는 통로를 제공합니다.

### 1.2 Game Initializer (`GameInitializer.cs`)
- **역할**: 게임 시작 시 필수 시스템 부팅 및 서비스 초기화.

---

## 2. 낚시 시스템 (Fishing System)

### 2.1 Fishing Controller (`FishingController.cs`)
- **역할**: 로컬 플레이어의 낚시 행위 및 상태 머신 관리.
- **상태 시퀀스**:
    - `Idle`: 기본 상태.
    - `Charging`: 마우스 왼쪽 버튼 홀딩 시 캐스팅 거리(min 2m ~ max 15m) 충전.
    - `Casting`: 바늘 투척 및 수면 인식 로직 실행.
    - `Waiting`: 입질 대기 (서버에서 3~30초 랜덤 결정).
    - `Nibble`: 입질 발생 알림 (0.5초 내 반응 필수).
    - `Catching`: 연타(Spamming) 모드. 등급별 목표 횟수 충족 시 성공.
    - `Success/Failure`: 보상 획득 연출 및 상태 초기화.

### 2.2 Fishing Player (`FishingPlayer.cs`)
- **역할**: 서버 권한(Server Authoritative) 기반의 낚시 검증 및 보상 지급.
- **기능**:
    - **서버 검증**: 대기 시간 결정, 물고기 종류/크기 결정, 연타 성공 여부 최종 판정.
    - **보상 시스템**: 성공 시 `IUserService`를 통해 인벤토리 추가 및 경험치 부여.
    - **S급(5성) 알림**: 5성 물고기 포획 시 전 서버 유저에게 공지 메시지 전송.

---

## 3. 데이터 및 영속성 (Data & Persistence)

### 3.1 유저 데이터 시스템
- **데이터 종류**: 인벤토리(아이템 인스턴스), 도감 기록(발견 여부, 최대 크기), 골드, 티어(Level), 경험치.
- **서버 저장**: 서버의 로컬 파일 시스템(`PersistentDataPath`)에 JSON 형식으로 실시간 저장 및 로드.

### 3.2 물고기 데이터 및 엑셀 자동화
- **별점 등급 시스템**: 등급을 ★ ~ ★★★★★로 관리. 등급에 따라 포획 확률 및 연타 난이도가 자동 조절됨.
- **크기 범위**: 각 물고기는 `minSize`와 `maxSize` 범위를 가지며, 포획 시 이 범위 내에서 크기가 결정됨.
- **자동화 도구**:
    - `FishDataPopulator.cs`: 52종의 물고기 데이터를 별점 체계에 맞춰 엑셀로 자동 생성.
    - `ExcelDataConverter.cs`: 엑셀 데이터를 ScriptableObject로 변환하며, 한글 설명과 등급별 기본 경험치를 자동 주입.

---

## 4. 시각 효과 및 UI (Visual & UI)

### 4.1 시각적 연출
- **수면 인식**: 레이캐스트를 통한 실시간 수면 높이 감지 및 낚시 가능 구역 제한.
- **물리 연출**: 베지어 곡선 기반의 바늘 투척, 낚싯줄 시각화(`LineRenderer`), 수면 충돌 물보라 파티클.

### 4.2 UI 시스템 (`FishingUI.cs`)
- **실시간 피드백**: 거리 충전 바, 입질 알림(!), 연타 진행도 게이지바 구현.
- **데이터 연동**: 플레이어 상태 UI(`PlayerStatusUI`)를 통해 실시간 경험치 및 레벨업 반영.

---

## 5. 네트워크 인프라 (Networking)
- **Mirror 연동**: `Command`, `TargetRpc`, `SyncVar`를 활용하여 모든 낚시 상태를 서버-클라이언트 간 완벽 동기화.
- **보안**: 모든 중요 데이터(아이템 획득, 경험치)는 서버에서만 처리하여 클라이언트 변조 방지.
