# Tasks: System Refinement and Cleanup

## 🛠️ 작업 목록 (Tasks)

### 1. 데이터 서비스 고도화
- [ ] `ExcelDataService.cs` 수정: 유령 에셋 필터링 로직 추가.

### 2. 성장 및 판매 로직 수정
- [ ] `UserSaveData.cs` 수정: `AddExp` 내의 `while` 루프 적용.
- [ ] `IUserService.cs` 수정: `SellAllFish()` 인터페이스 정의.
- [ ] `UserStorageService.cs` 수정: `SellAllFish()` 구현 및 저장 로직 연결.

### 3. UI 연동
- [ ] `InventoryUI.cs` 수정: `SellAllFish` 호출 버튼용 공개 메서드 추가.

## 🧪 테스트 (Testing)
- [ ] 도감 UI를 열었을 때 엑셀에 없는 물고기가 나오지 않는지 확인.
- [ ] 경험치 대량 획득 시 티어 숫자가 한 번에 2단계 이상 오르는지 확인.
- [ ] 일괄 판매 버튼(코드 연동용) 호출 시 골드가 정확히 합산되는지 확인.
