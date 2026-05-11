## 1. InventoryUI.cs — View 전환 + 등급 필터 + 도감 뷰

- [x] 1.1 `ViewMode` enum 추가 (Inventory, Shop)
- [x] 1.2 SideTabs 버튼 SerializeField + AutoBindReferences + onClick 연결
- [x] 1.3 TopTapGroup SerializeField + AutoBindReferences + 필터 로직
- [x] 1.4 인벤토리 뷰: RefreshList() with 등급 필터 (전체/하급/중급/상급)
- [x] 1.5 도감(Shop) 뷰: RefreshShopList() → GetAllFishData() 표시
- [x] 1.6 선택된 탭 하이라이트
- [x] 1.7 SetupCatalog() in InventorySlotUI (가격 표시, Sell 버튼 숨김)

## 2. Editor — RightPanel에 DetailPriceText 추가

- [ ] 2.1 RightPanel 자식으로 DetailPriceText (TMP) 생성 → 판매가 표시
- [ ] 2.2 SideTabs 버튼 텍스트 "인벤"/"샵" 으로 변경 (Prefab에서)
- [ ] 2.3 TopTapGroup 탭 텍스트 "전체"/"하급"/"중급"/"상급" 변경 (Prefab에서)

## 3. Play 테스트

- [ ] 3.1 인벤/샵 전환 테스트
- [ ] 3.2 등급 필터 테스트
- [ ] 3.3 도감 뷰에서 물고기 클릭 → 우측 상세 정보 확인
