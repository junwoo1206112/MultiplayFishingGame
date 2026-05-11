## Why

Inventory(Tab)와 Shop(B)이 별도 창으로 분리되어 있어 UX가 단절됨. 하나의 패널에서 인벤토리와 상점을 전환하며 볼 수 있도록 통합. 탑 탭 그룹은 등급(★)별 필터로 사용.

## What Changes

- InventoryUI에 **View 전환** 추가: 인벤토리 뷰 / 상점 뷰
- SideTabs 2개 버튼을 "인벤" / "샵"으로 기능 연결
- TopTapGroup 4개 탭을 등급 필터(전체/★~★★/★★★~★★★★/★★★★★)로 연결
- 인벤토리 뷰: LeftContent=물고기 그리드, RightContent=물고기 상세
- 상점 뷰: LeftContent=낚싯대/미끼 리스트, RightContent=아이템 상세
- 상점 뷰에서 구매/장착/해제 기능 포함
- ShopUI(B키)는 유지하되 Inventory 패널 내 상점 뷰와 동일한 데이터 사용

## Capabilities

### New Capabilities

- `inventory-shop-view-switch`: LeftContent/RightContent가 현재 뷰에 따라 전환
- `rank-filter`: TopTapGroup으로 inventory 그리드를 등급별 필터링
- `shop-item-list`: Inventory 패널 내에서 낚싯대/미끼 목록 표시
- `shop-item-detail`: 우측 패널에서 낚싯대/미끼 상세 정보 표시

### Modified Capabilities

- (없음. InventoryUI와 ShopUI 모두 새 능력)

## Impact

- **InventoryUI.cs**: 대규모 수정 (view 전환, 필터, 상점 아이템 표시)
- **Inventory.prefab**: SideTabs/TopTapGroup 텍스트 배치, RightPanel 구조 확장
- **ShopUI.cs**: 변경 없음 (B키 별도 창 유지)
- **ExcelDataService**: 변경 없음 (기존 API로 rods/baits/fish 데이터 제공)
