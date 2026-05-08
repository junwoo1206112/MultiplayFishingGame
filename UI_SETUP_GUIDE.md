# Shop System — UI 프리팹 셋업 가이드

이 문서는 상점 시스템(Shop System)의 UI 프리팹을 Unity Editor에서 설정하는 방법을 설명합니다.

---

## 1. 사전 준비 (Code → Scripts)

> ✅ 아래 파일들은 이미 코드로 구현되어 있습니다. 프리팹에 컴포넌트로 할당만 하면 됩니다.

| Script | File |
|--------|------|
| `ShopUI.cs` | `Assets/Scripts/UI/ShopUI.cs` |
| `ShopSlotUI.cs` | `Assets/Scripts/UI/ShopSlotUI.cs` |
| `ShopDetailPanel.cs` | `Assets/Scripts/UI/ShopDetailPanel.cs` |
| `ShopInventoryPanel.cs` | `Assets/Scripts/UI/ShopInventoryPanel.cs` |
| `ShopInventorySlotUI.cs` | `Assets/Scripts/UI/ShopInventorySlotUI.cs` |
| `ConfirmDialog.cs` | `Assets/Scripts/UI/ConfirmDialog.cs` |

---

## 2. Canvas 배치

### 모든 ShopUI 관련 UI는 Dynamic UI Canvas에 배치

| UI | Canvas | 이유 |
|----|--------|------|
| ShopUI (전체 창) | **Dynamic** | `windowRoot.SetActive()` 토글, 기존 InventoryUI/EncyclopediaUI와 동일 패턴 |
| ShopDetailPanel | **Dynamic** | ShopUI의 일부로 함께 토글 |
| ShopInventoryPanel | **Dynamic** | "물고기 판매" 탭에서만 활성화 |
| ConfirmDialog | **Dynamic** | ShopUI 내부 최상위, 모든 UI 위에 표시 |

### Static Canvas에는 다음만 유지
- PlayerStatusUI (골드/경험치 HUD)
- NotificationUI (토스트 알림)
- 항상 보여야 하는 HUD 요소들

---

## 3. 프리팹 계층 구조

### ShopUI (최상위 부모)

```
ShopUI (GameObject)
├── TopBar
│   ├── GoldIcon (Image)
│   └── GoldText (TMP_Text)         ← goldText 연결
│
├── TabButtons
│   ├── RodTab (Button)             ← rodTabButton
│   │   ├── Text (TMP_Text)         "낚싯대"
│   │   └── Highlight (GameObject)  ← rodTabHighlight
│   ├── BaitTab (Button)            ← baitTabButton
│   │   ├── Text (TMP_Text)         "미끼"
│   │   └── Highlight (GameObject)  ← baitTabHighlight
│   └── SellTab (Button)            ← sellTabButton
│       ├── Text (TMP_Text)         "물고기 판매"
│       └── Highlight (GameObject)  ← sellTabHighlight
│
├── ItemListPanel (ScrollView)
│   └── Content (Vertical Layout Group)  ← itemContentParent
│       └── ShopSlot (Prefab)           ← itemSlotPrefab
│
├── DetailPanel (ShopDetailPanel)   ← detailPanel (ShopUI에서 연결)
│   ├── IconImage (Image)
│   ├── NameText (TMP_Text)
│   ├── RankText (TMP_Text)
│   ├── PriceText (TMP_Text)
│   ├── StatsSection (GameObject)
│   │   └── StatsText (TMP_Text)
│   ├── DescriptionText (TMP_Text)
│   ├── BuyButton (Button)
│   ├── EquipButton (Button)
│   ├── UnequipButton (Button)
│   └── MessageText (TMP_Text)
│
├── InventoryPanel (ShopInventoryPanel)  ← inventoryPanel (ShopUI에서 연결)
│   ├── Header (TMP_Text)            "내 인벤토리"
│   ├── EmptyText (TMP_Text)         "보유한 물고기가 없습니다"
│   ├── SellAllButton (Button)
│   └── Content (Horizontal Layout Group / Grid)  ← contentParent
│       └── SlotPrefab (Prefab)                ← slotPrefab
│
└── ConfirmDialog (ConfirmDialog)   ← confirmDialog
    ├── DialogRoot (GameObject)
    ├── TitleText (TMP_Text)
    ├── MessageText (TMP_Text)
    ├── ConfirmButton (Button)
    └── CancelButton (Button)
```

### ShopSlot 프리팹 (개별 아이템 슬롯)

```
ShopSlot (Prefab) = ShopSlotUI.cs
├── IconImage (Image)               ← iconImage
├── NameText (TMP_Text)             ← nameText
├── RankText (TMP_Text)             ← rankText
├── PriceText (TMP_Text)            ← priceText
├── OwnedBadge (GameObject)         ← ownedBadge ("소유함")
├── EquippedBadge (GameObject)      ← equippedBadge ("장착 중")
└── SlotButton (Button)             ← slotButton
```

### ShopInventorySlot 프리팹 (인벤토리 내 물고기 슬롯)

```
ShopInventorySlot (Prefab) = ShopInventorySlotUI.cs
├── FishIcon (Image)                ← fishIcon
├── NameText (TMP_Text)            ← nameText
├── LengthText (TMP_Text)          ← lengthText ("52.3 cm")
├── PriceText (TMP_Text)           ← priceText ("250 G")
└── SellButton (Button)            ← sellButton
```

### ConfirmDialog (확인 팝업 — Scene에 1개만)

> ⚠️ **ConfirmDialog는 프리팹이 아닌 Scene에 직접 배치하는 단일 인스턴스입니다.**
> ShopUI, ShopInventoryPanel, ShopInventorySlotUI 모두 **동일한 ConfirmDialog**를 참조합니다.

```
ConfirmDialog (GameObject, Scene) = ConfirmDialog.cs
├── DialogRoot (GameObject)         ← dialogRoot (초기값: 비활성)
├── TitleText (TMP_Text)            ← titleText
├── MessageText (TMP_Text)          ← messageText
├── ConfirmButton (Button)          ← confirmButton
└── CancelButton (Button)           ← cancelButton
```

### 참조 관계 (ConfirmDialog 연결 대상)

| 스크립트 | 필드명 | 연결 방법 |
|----------|--------|----------|
| `ShopUI` | `confirmDialog` | Inspector 드래그 |
| `ShopInventoryPanel` | `confirmDialog` | Inspector 드래그 |
| `ShopInventorySlotUI` | `confirmDialog` | **자동 전달** (ShopInventoryPanel이 `Setup()` 시 전달) |

---

## 4. ShopUI.cs SerializeField 연결 가이드

`ShopUI` 컴포넌트를 최상위 부모 GameObject에 추가한 후, 다음 필드들을 Inspector에서 연결합니다.

### Window
| 필드 | 연결 대상 |
|------|----------|
| `windowRoot` | ShopUI 최상위 GameObject (자기 자신) |
| `toggleKey` | KeyCode.B |

### Top Bar
| 필드 | 연결 대상 |
|------|----------|
| `goldText` | TopBar/GoldText (TMP_Text) |

### Category Tabs
| 필드 | 연결 대상 |
|------|----------|
| `rodTabButton` | TabButtons/RodTab (Button) |
| `baitTabButton` | TabButtons/BaitTab (Button) |
| `sellTabButton` | TabButtons/SellTab (Button) |
| `rodTabHighlight` | TabButtons/RodTab/Highlight (GameObject) |
| `baitTabHighlight` | TabButtons/BaitTab/Highlight (GameObject) |
| `sellTabHighlight` | TabButtons/SellTab/Highlight (GameObject) |

### Item List
| 필드 | 연결 대상 |
|------|----------|
| `itemContentParent` | ItemListPanel/Content (Transform) |
| `itemSlotPrefab` | ShopSlot 프리팹 (Project에서 드래그) |

### Detail Panel
| 필드 | 연결 대상 |
|------|----------|
| `detailPanel` | DetailPanel GameObject → ShopDetailPanel 컴포넌트 |

### Inventory Panel
| 필드 | 연결 대상 |
|------|----------|
| `inventoryPanel` | InventoryPanel GameObject → ShopInventoryPanel 컴포넌트 |

### Confirm Dialog
| 필드 | 연결 대상 |
|------|----------|
| `confirmDialog` | ConfirmDialog GameObject → ConfirmDialog 컴포넌트 |

---

## 5. ShopDetailPanel.cs SerializeField 연결

| 필드 | 연결 대상 |
|------|----------|
| `iconImage` | DetailPanel/IconImage (Image) |
| `nameText` | DetailPanel/NameText (TMP_Text) |
| `rankText` | DetailPanel/RankText (TMP_Text) |
| `priceText` | DetailPanel/PriceText (TMP_Text) |
| `descriptionText` | DetailPanel/DescriptionText (TMP_Text) |
| `statsText` | DetailPanel/StatsSection/StatsText (TMP_Text) |
| `statsSection` | DetailPanel/StatsSection (GameObject) |
| `buyButton` | DetailPanel/BuyButton (Button) |
| `equipButton` | DetailPanel/EquipButton (Button) |
| `unequipButton` | DetailPanel/UnequipButton (Button) |
| `messageText` | DetailPanel/MessageText (TMP_Text) |

> **참고:** buyButton/equipButton/unequipButton의 onClick 리스너는 코드에서 자동으로 연결됩니다. Inspector에서 별도 설정 불필요.

---

## 6. ShopInventoryPanel.cs SerializeField 연결

| 필드 | 연결 대상 |
|------|----------|
| `contentParent` | InventoryPanel/Content (Transform) |
| `slotPrefab` | ShopInventorySlot 프리팹 (Project에서 드래그) |
| `sellAllButton` | InventoryPanel/SellAllButton (Button) |
| `emptyText` | InventoryPanel/EmptyText (TMP_Text) |
| `confirmDialog` | ConfirmDialog GameObject → ConfirmDialog 컴포넌트 |

---

## 7. ShopInventorySlotUI.cs SerializeField 연결

| 필드 | 연결 대상 |
|------|----------|
| `fishIcon` | FishIcon (Image) |
| `nameText` | NameText (TMP_Text) |
| `lengthText` | LengthText (TMP_Text) |
| `priceText` | PriceText (TMP_Text) |
| `sellButton` | SellButton (Button) |
| `confirmDialog` | **연결 불필요** — ShopInventoryPanel이 인스턴스화 시 자동 전달 |

> **참고:** `confirmDialog`는 `Setup()`의 `dialog` 파라미터로 전달되므로 프리팹에서 직접 연결할 필요 없습니다.

---

## 8. ConfirmDialog.cs SerializeField 연결

| 필드 | 연결 대상 |
|------|----------|
| `dialogRoot` | DialogRoot (GameObject) — SetActive(false) 초기값 |
| `titleText` | TitleText (TMP_Text) |
| `messageText` | MessageText (TMP_Text) |
| `confirmButton` | ConfirmButton (Button) |
| `cancelButton` | CancelButton (Button) |

---

## 9. 최종 체크리스트

### ✅ 완료됨 (코드 + 엑셀)
- [x] `Tools → Excel → 3. Populate Shop Data` 실행 (Rods/Baits 시트 생성)
- [x] `Tools → Excel → 3. Convert Rods Sheet to SO` 실행
- [x] `Tools → Excel → 4. Convert Baits Sheet to SO` 실행
- [x] RodDataSO / BaitDataSO 데이터 모델 생성
- [x] IDataService / IUserService 확장 (조회/구매/장착)
- [x] UserStorageService (BuyItem/Equip/Unequip/IsOwned)
- [x] FishingPlayer SyncVar + Command + CalculateCatch 보너스
- [x] ShopUI / ShopSlotUI / ShopDetailPanel 스크립트
- [x] ShopInventoryPanel / ShopInventorySlotUI / ConfirmDialog 스크립트

### ⬜ Unity Editor에서 해야 할 일
- [ ] **ShopUI 프리팹 생성** → Dynamic UI Canvas에 배치
- [ ] **ShopSlot 프리팹 생성** → `itemSlotPrefab`에 연결
- [ ] **ShopInventorySlot 프리팹 생성** → `slotPrefab`에 연결
- [ ] **ConfirmDialog는 Scene에 직접 배치** (프리팹 아님), 하나만 생성
- [ ] ConfirmDialog 참조를 ShopUI → confirmDialog, ShopInventoryPanel → confirmDialog 에 각각 연결
- [ ] ShopUI의 모든 SerializeField 누락 없이 연결 (4~8번 항목)
- [ ] `B` 키로 ShopUI 토글 확인
- [ ] 낚싯대/미끼 탭 전환 + 아이템 목록 표시 확인
- [ ] 물고기 판매 탭 → 인벤토리 목록 + 개별/전체 판매 확인
- [ ] 구매 → 골드 차감 + 소유 표시 확인
- [ ] 장착/해제 → SyncVar 동기화 확인
