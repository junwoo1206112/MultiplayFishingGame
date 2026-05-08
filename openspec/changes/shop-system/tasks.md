## 1. 엑셀 데이터 및 데이터 모델

- [x] 1.1 FishData.xlsx에 "Rods" 시트 추가 (ShopDataPopulator.cs 생성)
- [x] 1.2 FishData.xlsx에 "Baits" 시트 추가 (ShopDataPopulator.cs 생성)
- [x] 1.3 RodDataSO.cs 생성 (id, rodName, icon, rank, price, castDistanceBonus, catchChanceBonus, durability, description)
- [x] 1.4 BaitDataSO.cs 생성 (id, baitName, icon, rank, price, attractionFishIds[], catchChanceBonus, description)
- [x] 1.5 Rod/Bait 기본 데이터 엑셀에 작성 (기본 낚싯대/미끼 포함 5~10종)

## 2. 엑셀 컨버터 및 데이터 서비스

- [x] 2.1 ExcelDataConverter.cs 확장 — Rods 시트 → RodDataSO 변환 로직 추가
- [x] 2.2 ExcelDataConverter.cs 확장 — Baits 시트 → BaitDataSO 변환 로직 추가
- [x] 2.3 IDataService.cs 확장 — Rod/Bait 조회 메서드 추가 (GetRodData, GetAllRodData, GetBaitData, GetAllBaitData)
- [x] 2.4 ExcelDataService.cs 확장 — RodDataSO/BaitDataSO Resources.LoadAll 구현
- [ ] 2.5 Tools/Excel 메뉴로 변환 실행 → Rods/, Baits/ SO 에셋 생성 확인 (Unity Editor 필요)

## 3. 저장 데이터 및 서비스 확장

- [x] 3.1 UserSaveData.cs 확장 — ownedRodIds, ownedBaitIds, equippedRodId, equippedBaitId 필드 추가
- [x] 3.2 IUserService.cs 확장 — BuyItem, EquipRod, EquipBait, UnequipRod, UnequipBait 메서드 추가
- [x] 3.3 UserStorageService.cs 확장 — BuyItem 로직 구현 (골드 차감, 소유 목록 추가)
- [x] 3.4 UserStorageService.cs 확장 — EquipRod/EquipBait/UnequipRod/UnequipBait 로직 구현

## 4. ShopUI 구현

- [x] 4.1 ShopItemType enum 생성 (IUserService.cs에 추가)
- [x] 4.2 ShopUI.cs 생성 — 메인 윈도우 (B 키 토글, 카테고리 탭, 레이아웃)
- [x] 4.3 ShopSlotUI.cs 생성 — 아이템 슬롯 (아이콘, 이름, 등급, 가격, 소유 상태 배지)
- [x] 4.4 ShopDetailPanel.cs 생성 — 선택 아이템 상세 정보 (아이콘, 이름, 등급, 설명, 스탯, 구매/장착 버튼)
- [x] 4.5 ShopUI 카테고리 탭 전환 로직 구현 (낚싯대/미끼/물고기판매)
- [x] 4.6 구매 로직 구현 (CmdBuyItem Command → 서버 검증 → TargetRpcBuyResult)
- [x] 4.7 장착/해제 로직 구현 (CmdEquipRod/CmdEquipBait/CmdUnequipRod/CmdUnequipBait Command)

## 5. ShopInventoryPanel (하단 인벤토리)

- [x] 5.1 ShopInventoryPanel.cs 생성 — ShopUI 하단 인벤토리 패널
- [x] 5.2 ShopInventorySlotUI.cs 생성 — 물고기 슬롯 (이름, 크기, 판매 가격, 판매 버튼)
- [x] 5.3 판매 확인 다이얼로그 (ConfirmDialog.cs)
- [x] 5.4 개별 판매 버튼 → userService.SellFish 연결
- [x] 5.5 전체 판매 버튼 → userService.SellAllFish 연결 (확인 다이얼로그 포함)
- [x] 5.6 판매 후 UI 갱신 (OnDataChanged 구독)

## 6. FishingPlayer 네트워크 연동

- [x] 6.1 FishingPlayer.cs에 equippedRodId, equippedBaitId SyncVar 추가 (hook: OnEquippedRodChanged/OnEquippedBaitChanged)
- [x] 6.2 CmdBuyItem, CmdEquipRod, CmdEquipBait Command 추가
- [x] 6.3 TargetRpcBuyResult ClientRpc 추가
- [x] 6.4 FishingPlayer.CalculateCatch()에 Rod/Bait catchChanceBonus 반영
- [x] 6.5 FishingPlayer.GetCastDistanceBonus() 구현 (Rod castDistanceBonus)
- [x] 6.6 접속 시 장착 상태 복원 로직 (OnStartServer + OnStartLocalPlayer)

## 7. UI 프리팹 및 씬 배치

- [ ] 7.1 ShopUI 프리팹 생성 (Canvas, 탭 버튼, 리스트, 상세 패널, 인벤토리 패널)
- [ ] 7.2 ShopSlotUI 프리팹 생성 (아이콘 Image, 이름 Text, 등급 Text, 가격 Text, 배지)
- [ ] 7.3 ShopInventorySlotUI 프리팹 생성 (아이콘, 이름, 크기, 가격, 판매 버튼)
- [ ] 7.4 ShopUI 프리팹을 PlayScene Canvas에 배치
- [ ] 7.5 기존 InventoryUI와 충돌 없도록 ShopUI B 키 등록 확인
- [ ] 7.6 ConfirmDialog 프리팹 생성 (확인/취소 버튼이 있는 다이얼로그)
