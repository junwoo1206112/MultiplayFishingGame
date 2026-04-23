# 🎣 Webfishing 에셋 통합 & 낚시 시스템 작업 내역

---

## 1. 스프라이트 통합 (52개)

### 복사한 파일
- `fish1.png` ~ `fish42.png`
- `ocean_fish1.png` ~ `ocean_fish3.png`
- `prehistoric_fish1.png` ~ `prehistoric_fish3.png`
- `deep_sea_fish1.png` ~ `deep_sea_fish3.png`
- `alien_creature2.png`

### 위치
- `Assets/Fish/` (Unity 참조용)
- `Assets/Fishes_Itchio/` (원본 백업)

### 자동 매핑 툴
- `Assets/Editor/FishIconMatcher.cs`
- 메뉴: `Tools → Fish → Match Icons to Assets`
- `SpritePath = "Assets/Fish"`

---

## 2. 엑셀 데이터 (52종)

### 툴
- `Assets/Editor/FishDataPopulator.cs`
- 메뉴: `Tools → Excel → Populate 52 Webfishing Data`

### 변환 툴
- `Assets/Editor/ExcelDataConverter.cs`
- 메뉴: `Tools → Excel → Convert Fish Data to SO`

### 실행 순서
```
1. Populate 52 Webfishing Data  → 엑셀 생성
2. Convert Fish Data to SO       → ScriptableObject 생성
3. Match Icons to Assets         → 스프라이트 연결
```

---

## 3. 낚시 성공 로직

### 위치
- `Assets/Scripts/Gameplay/FishingPlayer.cs`

### 메서드
| 메서드 | 설명 |
|--------|------|
| `CmdCatchFish()` | 낚시 성공 서버 요청 `[Command]` |
| `CalculateCatch()` | 확률 기반 물고기 선택 |
| `TargetOnFishCaught()` | 결과 클라이언트 알림 `[TargetRpc]` |

### 사용법
```csharp
// 낚시 완료 시 호출
GetComponent<FishingPlayer>().CmdCatchFish();
```

---

## 4. 인벤토리 저장 (클리이언트 로컬)

### 저장 위치
- Windows: `%LocalAppData%Low\MultiplayFishingGame\UserData_{playerName}.json`

### 저장 흐름
```
CmdCatchFish() → 서버 확률 계산
    → TargetOnFishCaught() → 클라이언트
    → localUserData.AddToInventory()
    → SaveLocalData() → JSON 파일 저장
```

---

## 5. 수정한 파일 목록

| 파일 | 변경 내용 |
|------|----------|
| `Assets/Editor/FishIconMatcher.cs` | 52개 매핑, SpritePath 변경 |
| `Assets/Editor/FishDataPopulator.cs` | 52종 데이터, 메뉴 이름 변경 |
| `Assets/Scripts/Gameplay/FishingPlayer.cs` | 낚시 성공 로직, 클라이언트 로컬 저장 |
| `Assets/Scripts/Managers/Services/UserStorageService.cs` | List<InventoryItem> 대응 |

---

## 6. 다음 작업 (TODO)

- [ ] 낚시 완료 타이밍에 `CmdCatchFish()` 연결
- [ ] 인벤토리 UI 제작
- [ ] 도감 UI 제작
- [ ] Tree prefab 경고 해결
