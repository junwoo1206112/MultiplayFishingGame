# 🎨 UI 작업 및 컴포넌트 설정 가이드 (UI Setting Guide) - v1.2

이 문서는 `PlayScene`의 **Static UI Canvas**와 **Dynamic UI Canvas** 구조에 맞춰 UI 로직을 연결하는 방법을 설명합니다.

---

## 🏗️ 캔버스별 역할 분담 (Canvas Roles)

### 1. Static UI Canvas (정적)
> **역할**: 게임 내내 고정된 위치에 있거나, 전체 화면을 덮는 "큰 틀"을 배치합니다.
*   **포함 요소**: 캐릭터 상태창(Status), 인벤토리 윈도우, 도감 윈도우.
*   **장점**: 한 번 배치되면 위치가 거의 변하지 않아 안정적입니다.

### 2. Dynamic UI Canvas (동적)
> **역할**: 실시간으로 생성/파괴되거나, 게임 월드의 오브젝트를 따라다니는 "변화무쌍한" 요소를 배치합니다.
*   **포함 요소**: 획득 알림 메시지(Notification), 플레이어 머리 위 이름표.
*   **장점**: 자주 갱신되는 요소들을 모아두어 성능 최적화(Canvas Rebuild 방지)에 유리합니다.

---

## 📁 상세 배치 가이드 (Implementation)

### ① [Static UI Canvas] 하위 배치
#### 1. 플레이어 상태바 (`PlayerStatusUI.cs`)
*   **오브젝트**: `StatusPanel` (항상 노출)
*   **연결**: `tierText`, `expBar`, `goldText` 등.

#### 2. 인벤토리 윈도우 (`InventoryUI.cs`)
*   **오브젝트**: `InventoryWindow` (평소 비활성, Tab키로 활성)
*   **중요**: `windowRoot` 필드에 자기 자신을 연결하세요.
*   **슬롯 생성**: `contentParent`는 인벤토리 내부의 `ScrollRect > Content`를 연결합니다.

#### 3. 도감 윈도우 (`EncyclopediaUI.cs`)
*   **오브젝트**: `EncyclopediaWindow` (평소 비활성, E키로 활성)
*   **상세창**: 우측 `DetailPanel`의 모든 텍스트/이미지를 인스펙터에 할당하세요.

---

### ② [Dynamic UI Canvas] 하위 배치
#### 1. 알림 시스템 (`NotificationUI.cs`)
*   **오브젝트**: `NotificationPanel` (세로 레이아웃 그룹 권장)
*   **동작**: 물고기를 낚았을 때 뜨는 "XX 물고기 획득!" 메시지들이 여기서 생성됩니다.

#### 2. 이름표 (`PlayerNameDisplay.cs`)
*   **오브젝트**: 플레이어 캐릭터를 따라다니는 월드 스페이스 UI 요소들.

---

## 🐟 프리팹(Prefab) 설정 가이드
다음 요소들은 **`Assets/Prefabs`**에 저장하여 동적으로 생성되도록 설정하세요.

| 프리팹 이름 | 부착 스크립트 | 사용처 | 생성 위치(Parent) |
| :--- | :--- | :--- | :--- |
| **InventoryItemSlot** | `InventorySlotUI.cs` | 인벤토리 리스트 | `InventoryUI`의 `contentParent` |
| **EncyclopediaSlot** | `EncyclopediaSlotUI.cs` | 도감 그리드 | `EncyclopediaUI`의 `gridParent` |
| **NotificationItem** | (기존 스크립트) | 알림 메시지 | `Dynamic UI Canvas` 하위 패널 |

---

## 💡 작업자 최종 체크리스트
1.  **Static Canvas**에는 배경과 큰 창들을 넣었나요?
2.  **InventoryUI**의 `slotPrefab` 자리에 `InventoryItemSlot` 프리팹을 연결했나요?
3.  **OnSellAllClicked** 메서드를 인벤토리의 "전체 판매" 버튼에 연결했나요?
4.  **silhouetteOverlay**가 도감 슬롯 프리팹에서 아이콘을 잘 가리고 있나요?

---
*본 가이드는 PlayScene의 하이어라키 구조를 기반으로 최적화되었습니다.*
