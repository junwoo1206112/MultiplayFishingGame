# Webfishing Asset Integration & Fishing System

## Context
- Tech stack: Unity 6, C# 9.0, Mirror Networking v96.0.1, URP
- Existing: FishDataSO, ExcelDataConverter, FishDataPopulator, UserStorageService
- Source: https://github.com/SnailUsbs/Webfishing-Assets

---

## Scenario: 스프라이트 매핑

**Given** GitHub 저장소에 52개의 물고기 스프라이트가 존재함
**When** 프로젝트에 복사하고 ID를 매핑함
**Then** 70개 FishDataSO 중 52개에 고유 스프라이트가 연결됨

### Assets
- `fish1.png` ~ `fish42.png` (주요 물고기)
- `ocean_fish1.png` ~ `ocean_fish3.png` (해양 추가)
- `prehistoric_fish1.png` ~ `prehistoric_fish3.png` (선사 시대)
- `deep_sea_fish1.png` ~ `deep_sea_fish3.png` (심해)
- `alien_creature2.png` (특수)

### Mapping Table
| ID | Sprite |
|----|--------|
| fish_alligator ~ fish_walleye | fish1 ~ fish32 |
| fish_angelfish ~ fish_krill | fish33 ~ fish42 |
| fish_lionfish, fish_lobster, fish_manowar | ocean_fish1 ~ 3 |
| fish_mantaray, fish_mantaray_golden, fish_seaturtle | deep_sea_fish1 ~ 3 |
| fish_anomalocaris, fish_helicoprion, fish_leedsichthys | prehistoric_fish1 ~ 3 |
| fish_ufo | alien_creature2 |

---

## Scenario: 엑셀 데이터 생성

**Given** FishDataPopulator가 69종 데이터를 생성함
**When** 52종으로 축소하고 FishDataConverter로 SO를 생성함
**Then** 52개의 FishDataSO가 Resources/Data/Fish에 생성됨

### Menu Actions
```
Tools → Excel → Populate 52 Webfishing Data  // 엑셀 생성
Tools → Excel → Convert Fish Data to SO      // SO 생성
Tools → Fish → Match Icons to Assets         // 스프라이트 연결
```

---

## Scenario: 낚시 성공 로직

**Given** 플레이어가 낚시를 완료함
**When** CmdCatchFish()를 호출함
**Then** 서버가 확률 계산 후 결과를 클라이언트에게 전달함

### Network Flow
```
[Client] CmdCatchFish() → [Server]
[Server] CalculateCatch() → 확률 기반 선택
[Server] TargetOnFishCaught() → [Client]
[Client] localUserData.AddToInventory() → SaveLocalData()
```

---

## Scenario: 클라이언트 로컬 저장

**Given** 멀티플레이어 환경에서 여러 플레이어가 접속함
**When** 각자 낚시를 성공함
**Then** 각자 자신의 PC에 독립적으로 데이터가 저장됨

### Save Location
```
Windows: %LocalAppData%Low\[Company]\[Game]\UserData_{playerName}.json
```

### Why Client-Local?
- Original bug: UserStorageService singleton shared all player data
- Solution: Each FishingPlayer has its own localUserData
- Trade-off: Simple implementation vs. no cross-PC data

---

## Modified Files

| File | Change |
|------|--------|
| `Assets/Editor/FishIconMatcher.cs` | 52 mapping entries, SpritePath = "Assets/Fish" |
| `Assets/Editor/FishDataPopulator.cs` | 52 fish entries, menu: "Populate 52 Webfishing Data" |
| `Assets/Scripts/Gameplay/FishingPlayer.cs` | CmdCatchFish, CalculateCatch, local save/load |
| `Assets/Scripts/Managers/Services/UserStorageService.cs` | Fix List<InventoryItem> compile errors |

---

## TODO
- [ ] Connect CmdCatchFish() to actual fishing completion timing
- [ ] Create Inventory UI
- [ ] Create Encyclopedia UI
- [ ] Fix Terrain tree prefab warnings
