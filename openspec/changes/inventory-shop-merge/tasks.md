## 완료된 작업

### 키 변경
- [x] InventoryUI: `KeyCode.Tab` → `KeyCode.I` (인벤토리)
- [x] ShopUI: `KeyCode.B` → `KeyCode.Tab` (상점)

### InventoryUI 단순화
- [x] ViewMode enum 제거 — 순수 인벤토리 전용
- [x] Shop 뷰 관련 코드 제거 (RefreshShopList, shopSlotPrefab, shop detail)
- [x] SideTabs 참조 제거 (별도 창으로 분리)
- [x] 등급 필터 (TopTapGroup) 유지
- [x] 우측 상세 패널 유지

## 남은 작업 (Play 테스트)

- [ ] I 키로 인벤토리 열기/닫기
- [ ] Tab 키로 상점 열기/닫기
- [ ] 인벤토리 등급 필터 (전체/하급/중급/상급)
- [ ] 인벤토리 물고기 클릭 → 우측 상세 정보 확인