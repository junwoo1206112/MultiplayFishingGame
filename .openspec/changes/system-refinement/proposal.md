# Proposal: System Refinement and Cleanup

## 1. 개요 (Overview)
현재 완성된 낚시 게임의 핵심 시스템(데이터, 인벤토리, 성장)에서 발견된 논리적 허점과 사용자 편의성 결여 문제를 해결하고, 데이터의 무결성을 확보함.

## 2. 목표 (Goals)
*   **유령 에셋 차단**: 엑셀에 없는 불필요한 SO 데이터가 도감 등에 노출되는 문제 해결.
*   **다중 레벨업 지원**: 고효율 경험치 획득 시 플레이어가 즉각적인 피드백을 받을 수 있도록 로직 고도화.
*   **일괄 판매 편의성**: 인벤토리 정리 시간을 단축하기 위한 전체 판매 기능 도입.

## 3. 주요 수정 사항
*   `ExcelDataService`: 데이터 목록 반환 시 유효성 검사 필터 추가.
*   `UserSaveData`: `AddExp` 로직을 루프 구조로 개선.
*   `IUserService` & `UserStorageService`: `SellAllFish` 기능 추가 및 데이터 동기화.
