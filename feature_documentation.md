# 📜 멀티플레이 낚시 게임 기능 명세서 (Feature Documentation)

최종 업데이트: 2026-04-23

---

## 1. 마스터 데이터 시스템 (Master Data & Excel)

모든 물고기의 정보는 `Assets/ExcelData/FishData.xlsx`에서 통합 관리됩니다.

### 🛠️ 주요 기능 및 도구 (`ExcelDataConverter.cs`)
*   **안전한 데이터 로드**: 셀의 형식이 숫자든 텍스트든 상관없이 자동으로 판별하여 읽어오는 예외 처리 로직 적용.
*   **크기 데이터 자동 패치**: 엑셀에 최소/최대 크기가 없을 경우 물고기 등급(S~D)에 맞춰 자동으로 적정 범위를 생성하여 엑셀에 직접 써넣음.
*   **크리에이티브 콘텐츠 생성**: 제가 직접 작성한 **한글 설명(Flavor Text)**과 등급별 **경험치(EXP)** 보상을 엑셀에 자동으로 채워주는 기능.
*   **SO 변환**: 엑셀 데이터를 유니티 `FishDataSO` 에셋으로 1초 만에 일괄 생성/업데이트.

---

## 2. 인벤토리 및 도감 시스템 (Inventory & Encyclopedia)

수량 중첩 방식에서 벗어나, 잡은 물고기 한 마리 한 마리가 고유한 가치를 가지도록 설계되었습니다.

### 🐟 개별 데이터 구조 (`InventoryItem`)
*   **instanceId**: 각 물고기마다 부여되는 고유 식별자 (GUID).
*   **length**: 서버에서 계산된 해당 물고기만의 고유 크기(cm).
*   **caughtTime**: 낚인 시간을 기록 (Unix Timestamp).

### 📖 도감(Encyclopedia) 자동 등록
*   물고기를 낚는 순간 `UserSaveData`에서 해당 물고기가 처음인지 체크하여 자동으로 발견 리스트에 등록합니다.

---

## 3. 플레이어 성장 시스템 (Progression & Tier)

낚시를 통해 캐릭터가 성장하는 RPG 요소를 도입했습니다.

### 🆙 티어(Tier) 상승 로직
*   **경험치 획득**: 물고기 등급에 따라 차등화된 EXP 보상 (D: 20, C: 80, B: 250, A: 1000, S: 5000).
*   **티어 구간**: `UserSaveData.cs`의 `GetExpForNextTier()`에 정의된 구간에 따라 티어 1~5 및 그 이상으로 상승.
*   **데이터 보존**: 모든 경험치와 티어 정보는 `UserData.json`에 저장되어 재접속 시에도 유지됩니다.

---

## 4. 에셋 자동화 도구 (Visuals)

수십 종의 이미지를 수동으로 연결하는 번거로움을 해결했습니다.

### 🖼️ 지능형 아이콘 매칭 (`FishIconMatcher.cs`)
*   **자동 임포트 설정**: `Assets/Fish` 폴더 내의 모든 이미지를 `Sprite (2D and UI)` 모드와 `Point Filter`로 강제 설정하여 품질과 호환성 확보.
*   **다중 모드 지원**: 이미지 설정이 `Multiple`이든 `Single`이든 상관없이 내부 Sprite를 찾아내어 연결.
*   **이름 기반 매칭**: ID가 조금 다르더라도(`fish_bass` vs `fish3`) 매칭 테이블을 통해 정확한 이미지를 `FishDataSO`에 할당.

---

## 5. 기술적 아키텍처 (Technical Architecture)

*   **DI (Dependency Injection)**: `IDataService`, `IUserService`를 통해 각 기능이 독립적으로 작동하며 유지보수가 용이함.
*   **Network (Mirror)**: 서버에서 물고기 종류와 크기를 공정하게 계산하고, 클라이언트는 이를 전달받아 자신의 로컬 저장소에 저장하는 방식.

---

## 🚀 유니티 에디터 사용 가이드 (How to Use)

새로운 물고기를 추가하거나 데이터를 갱신할 때 다음 순서로 클릭하세요:

1.  **`Tools > Excel > 1. Patch Creative Content (Desc & EXP)`**: 엑셀에 설명과 경험치를 채웁니다.
2.  **`Tools > Excel > 2. Convert Excel to SO Assets`**: 유니티 에셋을 생성합니다.
3.  **`Tools > Fish > Match Icons to Assets`**: 물고기 이미지를 에셋에 자동으로 연결합니다.

---
*본 문서는 Gemini CLI에 의해 자동 생성되었습니다.*
