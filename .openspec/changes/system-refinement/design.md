# Design: System Refinement and Cleanup

## 1. 데이터 필터링 설계
*   **필터링 기준**: `FishDataSO`의 `fishName`이 비어 있거나, `catchChance`가 0 이하인 에셋은 "유령 데이터"로 간주.
*   **구현 위치**: `ExcelDataService.GetAllFishData()` 내부에서 LINQ의 `Where` 문을 사용하여 필터링된 리스트 반환.

## 2. 다중 레벨업 설계
*   **기존**: `if (currentExp >= nextExp) tier++` (한 번에 1단계만 상승)
*   **수정**: `while (currentExp >= GetExpForNextTier()) tier++` (경험치가 소진될 때까지 계속 상승)

## 3. 일괄 판매 설계
*   **인터페이스**: `void SellAllFish()`
*   **로직**: 
    1. 인벤토리 내 모든 물고기의 `sellPrice` 합산.
    2. 소지 골드에 합산된 금액 가산.
    3. 인벤토리 리스트 `Clear()`.
    4. `OnDataChanged` 이벤트 호출하여 UI 즉시 갱신.
