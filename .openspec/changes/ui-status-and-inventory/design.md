# Design: Player Status and Inventory UI System

## 1. 클래스 구조 (Class Structure)

### 📊 PlayerStatusUI (상태 표시)
*   **역할**: 화면 상단에 상시 노출되는 티어 및 경험치 정보 관리.
*   **주요 변수**:
    *   `TMP_Text tierText`: 현재 티어 표시.
    *   `Slider expBar`: 현재 경험치 진행도 (currentExp / nextTierExp).
    *   `TMP_Text goldText`: 보유 골드 수치.
*   **로직**: `IUserService`의 데이터를 참조하여 `OnEnable` 및 데이터 갱신 시 업데이트.

### 🎒 InventoryUI (인벤토리 창)
*   **역할**: 수집한 물고기 목록을 스크롤 뷰 형태로 표시.
*   **주요 변수**:
    *   `GameObject slotPrefab`: 개별 아이템 슬롯 프리팹.
    *   `Transform contentParent`: 슬롯이 생성될 부모 오브젝트.
*   **로직**:
    *   인벤토리 오픈 시 `IUserService.UserData.inventory` 리스트를 순회하며 슬롯 생성.
    *   데이터 중복 생성을 방지하기 위해 열릴 때마다 기존 리스트 클리어 후 재수정.

### 🐟 InventorySlotUI (아이템 슬롯)
*   **역할**: 개별 물고기의 시각적 정보 표시.
*   **데이터**: `InventoryItem` 모델 객체와 `FishDataSO` 마스터 데이터 결합.
*   **표시 항목**: 물고기 아이콘(`Sprite`), 이름, 낚인 크기(`length`).

## 2. 데이터 흐름 (Data Flow)

1.  **초기화**: `GameInitializer`에 의해 등록된 `IUserService`를 `DIContainer.Resolve<IUserService>()`로 UI가 주입받음.
2.  **데이터 변경**: 플레이어가 물고기를 낚으면 `IUserService.AddFish()` 호출 → 데이터 저장.
3.  **UI 갱신 알림**: (추가 예정) `IUserService`에 `OnDataChanged` 이벤트를 추가하여 데이터 변경 시 UI가 자동으로 `Refresh()` 하도록 유도.
4.  **렌더링**: 각 UI 클래스는 최신 `UserSaveData`를 읽어 화면 요소를 갱신.

## 3. UI 프리팹 구조 (Prefab Hierarchy)
*   **StatusCanvas**:
    *   `TierPanel` (Icon, Text)
    *   `ExpBar` (Background, Fill, PercentageText)
    *   `GoldPanel` (Icon, Text)
*   **InventoryCanvas**:
    *   `Background` (Overlay)
    *   `ScrollView` -> `Viewport` -> `Content` (Vertical Layout Group)
    *   `CloseButton`
