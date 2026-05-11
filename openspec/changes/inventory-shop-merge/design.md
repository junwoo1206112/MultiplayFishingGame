## Context

현재 InventoryUI(Tab)와 ShopUI(B)는 별도의 창으로 동작. InventoryUI 내 SideTabs(2개 빈 버튼)와 TopTapGroup(4개 빈 탭)이 존재하지만 기능 미구현 상태. ShopUI는 스크립트는 완성되었으나 프리팹이 없어 실제 동작하지 않음.

## Goals / Non-Goals

**Goals:**
- InventoryUI에서 SideTabs로 인벤/샵 뷰 전환
- TopTapGroup으로 인벤토리 물고기 등급(★)별 필터링
- 상점 뷰에서 낚싯대/미끼 목록 및 상세 정보 표시
- 상점 뷰에서 구매/장착/해제 가능
- 기존 ShopUI(B키)는 유지 (병행 사용)

**Non-Goals:**
- ShopUI 프리팹 생성 (별도 작업, UI_SETUP_GUIDE.md 참조)
- 도감(Encyclopedia) 기능
- 네트워크 동기화 변경

## Decisions

### 1. View 전환 방식 — 단일 Content 영역 교체

- LeftContent 내 inventory grid와 shop list를 **동일한 위치**에 번갈아 표시
- InventoryUI.cs 내 `ViewMode` enum으로 현재 뷰 관리
- `RefreshList()`가 viewMode에 따라 다른 콘텐츠 생성

### 2. 등급 필터 — TopTapGroup 4탭

| 탭 | 필터 | 매핑 |
|----|------|------|
| Tap 1 | 전체 | 전체 물고기 |
| Tap 2 | 하급 | ★~★★ |
| Tap 3 | 중급 | ★★★~★★★★ |
| Tap 4 | 상급 | ★★★★★ |

- `FishDataSO.rank` 문자열 길이로 구분 (★=1, ★★=2, ★★★=3, ★★★★=4, ★★★★★=5)

### 3. 상점 뷰 아이템 — 좌측 리스트, 우측 상세

- 좌측: 스크롤 가능한 아이템 리스트 (로드/베이트 구분 없이 통합 or 탭 전환)
- 우측: 선택한 아이템의 상세 정보 (가격, 능력치, 설명) + 구매/장착 버튼
- `ShopUI.cs`의 `ShopDetailPanel`과 동일한 데이터 표시

### 4. SideTabs — 인벤/샵 전환

| 버튼 | 라벨 | 뷰모드 |
|------|------|-------|
| 첫번째 Button | "인벤" | Inventory |
| 두번째 Button (1) | "샵" | Shop |

### 5. 데이터 흐름

```
IDataService.GetFishData()  → Inventory 뷰 물고기 목록
IDataService.GetAllRodData() → Shop 뷰 낚싯대 목록  
IDataService.GetAllBaitData() → Shop 뷰 미끼 목록
IUserService.UserData.gold  → Shop 뷰 골드 표시
IUserService.BuyItem()      → 구매 처리
IUserService.EquipRod/Bait() → 장착 처리
```

### 6. 상점 뷰 전환 시 TopTapGroup 재사용

인벤토리 뷰: 등급 필터
상점 뷰: "낚싯대" / "미끼" / "인벤토리" 탭으로 재사용 (탭 라벨 동적 변경)

### 7. RightPanel 동적 변경

인벤토리 뷰: FishIcon + FishName + Description + Weight(cm/kg)
상점 뷰: ItemIcon + ItemName + Rank + Price + Stats + Description + Buy/Equip/Unequip 버튼

## Risks / Trade-offs

- [TopTapGroup 4탭 부족] 인벤토리 필터+상점 탭 전환에 4탭으로는 부족 → 상점 뷰에서는 자체 탭 구조 사용
- [RightPanel 복잡도] 인벤토리/상점 뷰에 따라 다른 UI 필요 → 별도 detailRoot 교체 방식 고려
- [ShopUI 중복] B키 ShopUI와 Inventory 내 Shop 뷰가 기능 중복 → ShopUI는 유지, Inventory 내 Shop 뷰는 간소화된 버전
