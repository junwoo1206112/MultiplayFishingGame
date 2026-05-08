## Why

현재 게임에는 인벤토리에서 물고기를 골드에 판매하는 기능만 있을 뿐, 낚싯대나 미끼를 구매/관리하는 상점 시스템이 없다. 플레이어가 골드를 벌고 사용할 수 있는 경제 선순환 구조를 만들기 위해 종합 상점 시스템이 필요하다. 엑셀 데이터로 낚싯대와 미끼의 능력치와 가격을 관리하여 밸런스 조정이 용이하도록 한다.

## What Changes

- **엑셀 데이터 확장**: FishData.xlsx에 Rods(낚싯대), Baits(미끼) 시트 추가
- **새 데이터 모델**: RodDataSO, BaitDataSO ScriptableObject 생성
- **엑셀 컨버터 확장**: Rod/Bait 데이터 → SO 변환 기능 (Editor)
- **IDataService 확장**: Rod/Bait 데이터 로드 및 조회 메서드 추가
- **UserSaveData 확장**: 소유한 Rod/Bait 목록, 장착 중인 Rod/Bait 정보 추가
- **IUserService 확장**: 구매(물건 구입), 판매(물고기 판매) 메서드 추가
- **ShopUI**: 카테고리별(낚싯대/미끼/물고기판매) 상점 UI (단축키 B or 클릭)
- **인벤토리-상점 연계**: 하단 인벤토리 패널에서 물고기 선택 후 판매
- **장착 시스템**: 구매한 낚싯대/미끼를 장착/해제하는 기능
- **FishingPlayer 연동**: 장착한 낚싯대/미끼의 스탯이 낚시에 반영

## Capabilities

### New Capabilities
- `rod-data`: 낚싯대 데이터 정의 (id, name, icon, rank, price, castDistance, catchBonus, durability, description)
- `bait-data`: 미끼 데이터 정의 (id, name, icon, rank, price, attractionFishTypes, catchChanceBonus, description)
- `shop-ui`: 상점 UI (카테고리 탭, 아이템 리스트, 상세 정보, 구매/판매 버튼)
- `shop-inventory`: 하단 인벤토리 패널 (보유 물고기 목록, 개별 판매, 선택 판매)
- `equip-system`: 낚싯대/미끼 장착/해제 시스템
- `economy`: 골드 경제 시스템 (획득/사용 내역 등)

### Modified Capabilities

- `fish-data`: (기존) 피싱 데이터 — 변경 없음
- `inventory`: (기존) 인벤토리 → Shop 하단 패널로 통합되어 판매 기능과 연계
- `player-fishing`: FishingPlayer가 장착된 Rod/Bait 스탯을 낚시 로직에 반영

## Impact

영역 | 영향
--- | ---
Assets/ExcelData/FishData.xlsx | Rods, Baits 시트 2개 추가 (신규)
Assets/Scripts/Data/Models/ | RodDataSO.cs, BaitDataSO.cs 추가 (신규)
Assets/Scripts/Data/ | Data.asmdef 참조 업데이트
Assets/Scripts/Managers/Interfaces/ | IDataService.cs 확장 (Rod/Bait 조회), IUserService.cs 확장 (구매/판매)
Assets/Scripts/Managers/Services/ | ExcelDataService.cs 확장, UserStorageService.cs 확장
Assets/Scripts/UI/ | ShopUI.cs, ShopSlotUI.cs, ShopInventoryPanel.cs 추가 (신규)
Assets/Scripts/Gameplay/ | FishingPlayer.cs 확장 (장착 스탯 반영)
Assets/Editor/ | ExcelDataConverter.cs 확장 (Rod/Bait 시트 처리)
Assets/Resources/Data/ | Rods/, Baits/ 폴더 및 SO 에셋 생성
UserSaveData (저장 구조) | ownedRods, ownedBaits, equippedRodId, equippedBaitId 필드 추가
