## Context

현재 게임은 물고기 데이터(FishDataSO)만 엑셀로 관리되고 있으며, 낚싯대와 미끼는 단순 3D 모델로만 존재하고 데이터/스탯이 없다. 인벤토리에서 물고기 판매는 가능하지만, 골드를 사용할 곳이 없어 경제 시스템이 완성되지 않았다. 상점 시스템을 도입하여 골드의 획득-소비 선순환 구조를 만들고, 낚싯대/미끼의 스탯을 데이터 기반으로 관리하여 밸런스 조정을 용이하게 한다.

## Goals / Non-Goals

**Goals:**
- 엑셀(FishData.xlsx)에 Rods, Baits 시트 추가 및 SO 변환
- RodDataSO, BaitDataSO 데이터 모델 정의
- 상점 UI: 카테고리 탭(낚싯대/미끼/물고기판매) + 아이템 리스트 + 상세 정보 + 인벤토리 하단 패널
- 아이템 구매 (골드 차감 → 인벤토리 추가)
- 물고기 판매 (골드 획득 → 인벤토리 제거) — 기존 시스템 ShopUI로 통합
- 낚싯대/미끼 장착/해제 시스템
- 장착한 장비 스탯이 낚시 로직에 반영 (FishingPlayer 연동)
- 모든 데이터 CRUD는 서버 권위적 (Server-Authoritative)

**Non-Goals:**
- 제작/강화 시스템 (추후 별도 변경)
- 미끼 소모 시스템 (일회용 vs 무한 — 추후 결정)
- 아이템 드롭/획득 시스템 (상점 구매만)
- P2P 거래 시스템

## Decisions

### 1. 엑셀 시트 분리 vs 별도 엑셀 파일
**결정**: FishData.xlsx에 Rods, Baits 시트 추가
- **이유**: 기존 엑셀 컨버터 인프라(NPOI, EditorWindow) 재사용, 단일 파일 관리 용이
- **대안**: 별도 ShopData.xlsx — 파일 분리 장점이 크지 않아 기각

### 2. Rod/Bait 데이터 → SO 변환 시점
**결정**: Editor에서 Tools/Excel 메뉴를 통해 수동 변환 (기존 Fish 변환과 동일한 패턴)
- **이유**: FishDataSO와 동일한 워크플로우 유지, 실시간 변환 부하 없음
- **대안**: Runtime에서 직접 엑셀 파싱 — NPOI 종속성 유지 불가, 성능 이슈

### 3. 구매/판매 네트워크 동기화
**결정**: 클라이언트에서 Command 호출 → 서버 검증 → SyncVar/ClientRpc로 결과 전파
- **이유**: 서버 권위적 모델 준수, 골드/아이템 조작 방지
- **SyncVar**: `equippedRodId`, `equippedBaitId` (FishingPlayer에 선언)
- **ClientRpc**: RpcPurchaseResult(bool success, string message) — 구매 결과 통지

### 4. UI 아키텍처 (단일 ShopUI 창)
**결정**: 하나의 ShopUI 창에 카테고리 탭 + 좌측 리스트 + 우측 상세 + 하단 인벤토리
- **이유**: 화면 전환 없이 모든 상점 기능 접근, UI 상태 관리 단순
- **Toggle Key**: `B` 키 (Shop 전용) — Inventory(Tab)와 분리

### 5. Rod/Bait 장착 시스템
**결정**: UserSaveData에 `equippedRodId`, `equippedBaitId` (string) 저장 → 서버 FishingPlayer의 SyncVar에 반영
- **이유**: 저장 데이터 = 장착 상태 소스 오브 트루스, 접속 시 자동 복원
- **장착 UI는 ShopUI 내에서 처리** (별도 장착 화면 없음)

## Data Architecture

### 엑셀 → SO 변환 흐름
```
FishData.xlsx
├── FishList 시트 ──→ FishDataSO (기존)
├── Rods 시트 ──→ RodDataSO (신규)
└── Baits 시트 ──→ BaitDataSO (신규)
```

### SO 저장 경로
```
Assets/Resources/Data/
├── Fish/         (기존)
├── Rods/         (신규)
└── Baits/        (신규)
```

### UserSaveData 확장
```csharp
public List<string> ownedRodIds = new List<string>();
public List<string> ownedBaitIds = new List<string>();
public string equippedRodId = "";
public string equippedBaitId = "";
```

### Service 인터페이스 확장

**IDataService 추가:**
```csharp
RodDataSO GetRodData(string id);
List<RodDataSO> GetAllRodData();
BaitDataSO GetBaitData(string id);
List<BaitDataSO> GetAllBaitData();
```

**IUserService 추가:**
```csharp
bool BuyItem(ShopItemType type, string itemId); // Rod or Bait
bool EquipRod(string rodId);
bool EquipBait(string baitId);
void UnequipRod();
void UnequipBait();
event Action OnInventoryChanged; // 기존 OnDataChanged로 충분
```

### RodDataSO 구조
```csharp
public class RodDataSO : ScriptableObject
{
    public string id;
    public string rodName;
    public Sprite icon;
    public string rank; // ★~★★★★★
    public int price;
    public float castDistanceBonus; // 추가 캐스팅 거리 (m)
    public float catchChanceBonus; // 추가 포획 확률 (%)
    public float durability; // 내구도
    [TextArea] public string description;
}
```

### BaitDataSO 구조
```csharp
public class BaitDataSO : ScriptableObject
{
    public string id;
    public string baitName;
    public Sprite icon;
    public string rank; // ★~★★★★★
    public int price;
    public string[] attractionFishIds; // 특정 물고기 유인 (비우면 all)
    public float catchChanceBonus; // 추가 포획 확률 (%)
    [TextArea] public string description;
}
```

### Shop UI Layout
```
┌──────────────────────────────────────────────┐
│ [Gold: 12,345 G]                     [X] 닫기 │
├──────────────────────────────────────────────┤
│ [낚싯대] [미끼] [물고기 판매] ← Tab 버튼      │
├──────────────────────┬───────────────────────┤
│                      │                       │
│  아이템 리스트        │  상세 정보 패널        │
│  (ScrollView)        │  [아이콘]             │
│                      │  이름 / 등급           │
│  ┌────┐ ┌────┐      │  가격: 1,000 G        │
│  │    │ │    │      │  설명: ...            │
│  │아이템│ │아이템│    │  스탯: ...            │
│  └────┘ └────┘      │                       │
│  ┌────┐ ┌────┐      │  [구매] or [장착]     │
│  │    │ │    │      │                       │
│  └────┘ └────┘      │                       │
│                      │                       │
├──────────────────────┴───────────────────────┤
│ [내 인벤토리]                    [전체 판매]  │
│ ┌────┐ ┌────┐ ┌────┐ ┌────┐                │
│ │물고기│ │물고기│ │물고기│ │물고기│  (가로 스크롤)│
│ │100 G│ │250 G│ │80 G│ │... │                │
│ └─판매┘ └─판매┘ └─판매┘ └────┘                │
└──────────────────────────────────────────────┘
```

### 네트워크 플로우 (구매 예시)
```
Client (ShopUI)                  Server (FishingPlayer)
     │                                │
     │  CmdBuyItem("rod_golden")      │
     │───────────────────────────────>│
     │                                │  Validate: gold >= price
     │                                │  Validate: not already owned
     │                                │  gold -= price
     │                                │  ownedRodIds.Add("rod_golden")
     │  TargetRpcBuyResult(true)      │
     │<───────────────────────────────│
     │                                │
     │  (SyncVar equippedRodId 자동 동기화)
```

## Risks / Trade-offs

| 리스크 | 완화 방안 |
|--------|-----------|
| 엑셀 시트 추가로 인한 기존 FishList 시트 영향 | 기존 시트 구조 변경 없음, 별도 시트로 추가 |
| NPOI 종속성 (Editor 전용) | Editor 폴더로 제한, Runtime 의존성 없음 |
| 골드 치트/변조 | 모든 구매/판매는 Command → 서버 검증 |
| 장착 장비 데이터 동기화 지연 | SyncVar 사용으로 자동 동기화, 접속 시 초기화 |
| 많은 아이템으로 인한 UI 성능 | ScrollView + Object Pooling (가상 스크롤은 불필요) |

## Open Questions

1. 미끼를 소모품(1회 사용)으로 할지 영구 장비로 할지?
2. 기본 낚싯대/미끼는 모든 플레이어가 무료로 소유하는가?
3. 장착한 Rod/Bait 스탯이 FishingPlayer.CalculateCatch()에 구체적으로 어떻게 반영되는가?
