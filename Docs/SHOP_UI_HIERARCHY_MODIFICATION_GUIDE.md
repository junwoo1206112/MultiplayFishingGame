# Store UI 하이라키 수정 가이드

이 문서는 현재 `Store.prefab`을 프로젝트의 상점 UI 스크립트 구조에 맞게 수정하는 방법을 정리합니다.  
`Inventory 1.prefab`은 기존 인벤토리 UI로 유지하고, `Store.prefab`만 상점 전용으로 고치는 방향입니다.

## 현재 확인된 상태

확인한 파일:

```text
Assets/Prefabs/Game UI/Store.prefab
Assets/Prefabs/Game UI/Inventory 1.prefab
Assets/Prefabs/Game UI/InventorySlot.prefab
Assets/Scenes/PlayScene.unity
```

현재 상태 요약:

```text
Store.prefab
|-- 루트 이름: Store
|-- 아직 Inventory UI 구조를 기반으로 되어 있음
|-- 루트에 InventoryUI가 붙어 있음
|-- 내부 슬롯에는 InventorySlotUI가 붙어 있음
|-- InventoryPanel / ItemGridPanel / RightContent 같은 인벤토리 구조가 남아 있음

Inventory 1.prefab
|-- 루트 이름: Inventory 1
|-- 정상 인벤토리 UI
|-- InventoryUI 사용
|-- InventorySlotUI 사용
|-- 플레이어 인벤토리로 그대로 유지해야 함

InventorySlot.prefab
|-- 기존 물고기 인벤토리 슬롯 프리팹
|-- InventorySlotUI 사용
|-- 상점 판매 슬롯을 만들 때 시각적 베이스로 복사해서 사용 가능
```

스크립트 GUID 확인 결과:

```text
ShopSlotUI.cs          = 20b26bada68c59d49a72f46262e2d301
ShopInventorySlotUI.cs = 6348e3b61cf63714a80838ac42898fc8
InventorySlotUI.cs     = ef4ffa91252ccca4bb7cb4f7db10159b
```

현재 프리팹들에는 `ShopSlotUI`나 `ShopInventorySlotUI`가 아니라 `InventorySlotUI`가 들어가 있습니다.

## 핵심 원칙

`Inventory 1.prefab`은 상점 작업 중에 수정하지 마세요.

역할을 이렇게 분리합니다:

```text
Inventory 1.prefab -> 플레이어 인벤토리
Store.prefab       -> 상점/스토어 창
```

이렇게 해야 기존 인벤토리를 망가뜨리지 않고 `Store.prefab`만 안전하게 상점용으로 수정할 수 있습니다.

## 씬 배치 위치

상점 UI는 `Dynamic UI Canvas` 아래에 배치하는 것이 맞습니다.

권장 씬 하이라키:

```text
PlayScene
|-- Dynamic UI Canvas
|   `-- Store                         <- 여기에 ShopUI.cs 추가. 이 오브젝트는 Active 유지.
|       `-- WindowRoot                <- 기존 InventoryPanel. ShopUI.windowRoot에 연결.
|
|-- Static UICanvas                   <- 항상 보이는 HUD만 유지.
|-- FishingUI
`-- Inventory 1                       <- 기존 인벤토리 UI로 유지.
```

루트 오브젝트 이름은 `Store` 그대로 둬도 됩니다. 꼭 `ShopUI`로 이름을 바꿀 필요는 없습니다. 중요한 것은 `Store` 루트에 `ShopUI.cs`가 붙어 있어야 한다는 점입니다.

## 최종 Store 하이라키 목표

`Store.prefab`을 아래 구조에 가깝게 수정합니다:

```text
Store                                  <- ShopUI.cs, Active 유지
`-- WindowRoot                         <- 실제 보이는 창, B 키로 열고 닫힘
    |-- TopBar
    |   |-- GoldIcon
    |   `-- GoldText                   <- ShopUI.goldText
    |
    |-- TabButtons
    |   |-- RodTab                     <- Button, ShopUI.rodTabButton
    |   |   |-- Text                   <- TMP_Text, "낚싯대"
    |   |   `-- Highlight              <- ShopUI.rodTabHighlight
    |   |-- BaitTab                    <- Button, ShopUI.baitTabButton
    |   |   |-- Text                   <- TMP_Text, "미끼"
    |   |   `-- Highlight              <- ShopUI.baitTabHighlight
    |   `-- SellTab                    <- Button, ShopUI.sellTabButton
    |       |-- Text                   <- TMP_Text, "물고기 판매"
    |       `-- Highlight              <- ShopUI.sellTabHighlight
    |
    |-- ItemListPanel                  <- 낚싯대/미끼 목록 영역
    |   `-- Viewport
    |       `-- Content                <- ShopUI.itemContentParent
    |
    |-- DetailPanel                    <- ShopDetailPanel.cs
    |   |-- IconImage
    |   |-- NameText
    |   |-- RankText
    |   |-- PriceText
    |   |-- StatsSection
    |   |   `-- StatsText
    |   |-- DescriptionText
    |   |-- BuyButton
    |   |-- EquipButton
    |   |-- UnequipButton
    |   `-- MessageText
    |
    |-- InventoryPanel                 <- ShopInventoryPanel.cs, SellTab에서만 사용
    |   |-- Header
    |   |-- EmptyText
    |   |-- SellAllButton
    |   `-- ScrollView
    |       `-- Viewport
    |           `-- Content            <- ShopInventoryPanel.contentParent
    |
    `-- ConfirmDialog                  <- ConfirmDialog.cs
        `-- DialogRoot                 <- 시작 시 비활성
            |-- DimBackground
            `-- Panel
                |-- TitleText
                |-- MessageText
                |-- ConfirmButton
                `-- CancelButton
```

## Store.prefab에서 수정할 내용

### 1. 루트 오브젝트

현재:

```text
Store 루트에 InventoryUI가 붙어 있음
```

변경:

```text
Store 루트에 ShopUI를 붙임
```

작업 순서:

```text
1. Store 루트 오브젝트 선택.
2. InventoryUI 컴포넌트 제거 또는 비활성.
3. ShopUI 컴포넌트 추가.
4. toggleKey를 B로 설정.
```

`ShopUI.cs`는 보이는 패널 자식이 아니라 루트 `Store`에 붙이세요.

### 2. 실제 보이는 창

현재:

```text
Store
`-- InventoryPanel
```

변경:

```text
Store
`-- WindowRoot
```

작업 순서:

```text
1. Store/InventoryPanel을 WindowRoot로 이름 변경.
2. Store의 ShopUI.windowRoot에 WindowRoot 연결.
3. Store 루트는 항상 Active 유지.
4. WindowRoot만 ShopUI 스크립트가 열고 닫게 둠.
```

`windowRoot`를 `Store` 자기 자신에 연결하면 안 됩니다. `Store`가 꺼지면 `B` 키 입력을 받는 `ShopUI`도 같이 꺼져서 다시 열리지 않을 수 있습니다.

### 3. 상단 탭

현재 Store에는 이런 구조가 있을 가능성이 큽니다:

```text
WindowRoot/TopTapGroup
```

이 구조를 상점용 탭으로 바꿉니다:

```text
WindowRoot/TabButtons
|-- RodTab
|   |-- Text
|   `-- Highlight
|-- BaitTab
|   |-- Text
|   `-- Highlight
`-- SellTab
    |-- Text
    `-- Highlight
```

`ShopUI`에 연결:

```text
rodTabButton        -> TabButtons/RodTab
baitTabButton       -> TabButtons/BaitTab
sellTabButton       -> TabButtons/SellTab
rodTabHighlight     -> RodTab/Highlight
baitTabHighlight    -> BaitTab/Highlight
sellTabHighlight    -> SellTab/Highlight
```

기존 `Exit_Button`은 `ShopUI.cs`에서 직접 사용하지 않습니다. 유지하고 싶다면 Button OnClick에 `Store -> ShopUI.CloseWindow()`를 직접 연결하세요.

### 4. 낚싯대/미끼 목록 영역

현재 Store에는 이런 아이템 그리드 오브젝트가 있습니다:

```text
ItemContent
|-- ItemGridPanel
`-- ItemGridPanel (1)
```

이 영역은 낚싯대/미끼 아이템 목록으로 사용합니다:

```text
ItemListPanel
`-- Viewport
    `-- Content
```

연결:

```text
ShopUI.itemContentParent -> ItemListPanel/Viewport/Content
```

`Content` 아래에는 런타임에 샘플 아이템 오브젝트를 계속 두지 않는 것이 좋습니다. `ShopUI`는 아래 코드처럼 슬롯 프리팹을 자동 생성합니다:

```csharp
Instantiate(itemSlotPrefab, itemContentParent)
```

따라서 실제 `ShopSlot.prefab`을 만든 뒤에는 `Content` 아래의 임시 샘플 슬롯들은 제거하거나 비활성화하세요.

### 5. 골드 패널

현재 Store에는:

```text
Gold_Panel
```

이 오브젝트를 아래처럼 재사용합니다:

```text
TopBar
|-- GoldIcon
`-- GoldText
```

연결:

```text
ShopUI.goldText -> TopBar/GoldText
```

이 텍스트는 `userService.UserData.gold` 값을 표시합니다.

### 6. 상세 정보 패널

상점에는 선택한 낚싯대/미끼의 상세 정보를 보여줄 영역이 필요합니다.

오른쪽 영역이나 기존 패널을 아래 구조로 만들거나 변환하세요:

```text
DetailPanel
|-- IconImage
|-- NameText
|-- RankText
|-- PriceText
|-- StatsSection
|   `-- StatsText
|-- DescriptionText
|-- BuyButton
|-- EquipButton
|-- UnequipButton
`-- MessageText
```

`DetailPanel`에 `ShopDetailPanel.cs`를 추가합니다.

연결:

```text
ShopUI.detailPanel -> DetailPanel
```

`ShopDetailPanel` 필드 연결:

```text
iconImage           -> IconImage
nameText            -> NameText
rankText            -> RankText
priceText           -> PriceText
descriptionText     -> DescriptionText
statsText           -> StatsSection/StatsText
statsSection        -> StatsSection
buyButton           -> BuyButton
equipButton         -> EquipButton
unequipButton       -> UnequipButton
messageText         -> MessageText
```

`BuyButton`, `EquipButton`, `UnequipButton`의 OnClick은 직접 넣지 않아도 됩니다. `ShopDetailPanel.cs`가 코드에서 자동 연결합니다.

### 7. 물고기 판매 패널

상점에는 보유 물고기를 판매하는 영역도 필요합니다.

아래 구조를 만들거나 기존 영역을 변환하세요:

```text
InventoryPanel
|-- Header
|-- EmptyText
|-- SellAllButton
`-- ScrollView
    `-- Viewport
        `-- Content
```

이 `InventoryPanel`에 `ShopInventoryPanel.cs`를 추가합니다.

연결:

```text
ShopUI.inventoryPanel              -> InventoryPanel
ShopInventoryPanel.contentParent   -> InventoryPanel/ScrollView/Viewport/Content
ShopInventoryPanel.sellAllButton   -> InventoryPanel/SellAllButton
ShopInventoryPanel.emptyText       -> InventoryPanel/EmptyText
```

이 패널은 `SellTab`을 눌렀을 때만 보입니다. `ShopUI.SwitchTab()`이 자동으로 처리합니다.

### 8. 확인 팝업

구매/판매 확인용 팝업을 `WindowRoot` 아래에 하나 만듭니다.

```text
ConfirmDialog
`-- DialogRoot
    |-- DimBackground
    `-- Panel
        |-- TitleText
        |-- MessageText
        |-- ConfirmButton
        `-- CancelButton
```

`ConfirmDialog`에 `ConfirmDialog.cs`를 추가합니다.

연결:

```text
ConfirmDialog.dialogRoot      -> DialogRoot
ConfirmDialog.titleText       -> DialogRoot/Panel/TitleText
ConfirmDialog.messageText     -> DialogRoot/Panel/MessageText
ConfirmDialog.confirmButton   -> DialogRoot/Panel/ConfirmButton
ConfirmDialog.cancelButton    -> DialogRoot/Panel/CancelButton

ShopUI.confirmDialog                  -> ConfirmDialog
ShopInventoryPanel.confirmDialog      -> ConfirmDialog
```

`DialogRoot`는 에디터에서 비활성 상태로 시작하세요.

## 슬롯 프리팹 계획

이미 프로젝트에 슬롯 모양이 있으므로 완전히 새로 만들 필요는 없습니다.

권장 작업:

```text
상점 낚싯대/미끼 슬롯 -> Store의 ItemGridPanel 시각 요소를 베이스로 만들고 ShopSlotUI 부착
상점 물고기 판매 슬롯 -> InventorySlot.prefab을 복사해서 만들고 ShopInventorySlotUI 부착
InventorySlot.prefab  -> 원본은 그대로 유지
```

### 낚싯대/미끼용 ShopSlot

생성 위치:

```text
Assets/Prefabs/Game UI/ShopSlot.prefab
```

베이스로 쓰기 좋은 오브젝트:

```text
Store/WindowRoot/ItemListPanel/.../ItemGridPanel
```

최종 프리팹 구조:

```text
ShopSlot                  <- ShopSlotUI.cs
|-- IconImage             <- Image
|-- NameText              <- TMP_Text
|-- RankText              <- TMP_Text
|-- PriceText             <- TMP_Text
|-- OwnedBadge            <- GameObject
|-- EquippedBadge         <- GameObject
`-- SlotButton            <- Button
```

연결:

```text
ShopSlotUI.iconImage      -> IconImage
ShopSlotUI.nameText       -> NameText
ShopSlotUI.rankText       -> RankText
ShopSlotUI.priceText      -> PriceText
ShopSlotUI.ownedBadge     -> OwnedBadge
ShopSlotUI.equippedBadge  -> EquippedBadge
ShopSlotUI.slotButton     -> SlotButton

ShopUI.itemSlotPrefab     -> ShopSlot.prefab
```

복사한 오브젝트에 `InventorySlotUI`가 남아 있다면 반드시 제거하세요. 이 프리팹은 `ShopSlotUI`를 써야 합니다.

### 물고기 판매용 ShopInventorySlot

생성 위치:

```text
Assets/Prefabs/Game UI/ShopInventorySlot.prefab
```

베이스로 쓰기 좋은 프리팹:

```text
Assets/Prefabs/Game UI/InventorySlot.prefab
```

권장 작업 순서:

```text
1. InventorySlot.prefab 복제.
2. 복제본 이름을 ShopInventorySlot.prefab으로 변경.
3. 복제본에서 InventorySlotUI 제거.
4. ShopInventorySlotUI 추가.
5. FishIcon, SellButton 같은 기존 자식은 재사용.
6. 부족한 텍스트 자식 추가: NameText, LengthText, PriceText.
```

최종 구조:

```text
ShopInventorySlot         <- ShopInventorySlotUI.cs
|-- FishIcon              <- Image
|-- NameText              <- TMP_Text
|-- LengthText            <- TMP_Text
|-- PriceText             <- TMP_Text
`-- SellButton            <- Button
```

연결:

```text
ShopInventorySlotUI.fishIcon    -> FishIcon
ShopInventorySlotUI.nameText    -> NameText
ShopInventorySlotUI.lengthText  -> LengthText
ShopInventorySlotUI.priceText   -> PriceText
ShopInventorySlotUI.sellButton  -> SellButton

ShopInventoryPanel.slotPrefab   -> ShopInventorySlot.prefab
```

프리팹 안의 아래 필드는 비워둬도 됩니다:

```text
ShopInventorySlotUI.confirmDialog
```

`ShopInventoryPanel`이 런타임에 확인 팝업을 전달합니다.

## 전체 Inspector 연결 체크리스트

### Store 루트: ShopUI

`ShopUI.cs`를 붙일 오브젝트:

```text
Store
```

연결:

```text
windowRoot          -> WindowRoot
toggleKey           -> B
goldText            -> WindowRoot/TopBar/GoldText

rodTabButton        -> WindowRoot/TabButtons/RodTab
baitTabButton       -> WindowRoot/TabButtons/BaitTab
sellTabButton       -> WindowRoot/TabButtons/SellTab

rodTabHighlight     -> WindowRoot/TabButtons/RodTab/Highlight
baitTabHighlight    -> WindowRoot/TabButtons/BaitTab/Highlight
sellTabHighlight    -> WindowRoot/TabButtons/SellTab/Highlight

itemContentParent   -> WindowRoot/ItemListPanel/Viewport/Content
itemSlotPrefab      -> ShopSlot.prefab

detailPanel         -> WindowRoot/DetailPanel
inventoryPanel      -> WindowRoot/InventoryPanel
confirmDialog       -> WindowRoot/ConfirmDialog
```

### DetailPanel: ShopDetailPanel

`ShopDetailPanel.cs`를 붙일 오브젝트:

```text
WindowRoot/DetailPanel
```

연결:

```text
iconImage           -> IconImage
nameText            -> NameText
rankText            -> RankText
priceText           -> PriceText
descriptionText     -> DescriptionText
statsText           -> StatsSection/StatsText
statsSection        -> StatsSection
buyButton           -> BuyButton
equipButton         -> EquipButton
unequipButton       -> UnequipButton
messageText         -> MessageText
```

### InventoryPanel: ShopInventoryPanel

`ShopInventoryPanel.cs`를 붙일 오브젝트:

```text
WindowRoot/InventoryPanel
```

연결:

```text
contentParent       -> InventoryPanel/ScrollView/Viewport/Content
slotPrefab          -> ShopInventorySlot.prefab
sellAllButton       -> InventoryPanel/SellAllButton
emptyText           -> InventoryPanel/EmptyText
confirmDialog       -> WindowRoot/ConfirmDialog
```

### ConfirmDialog

`ConfirmDialog.cs`를 붙일 오브젝트:

```text
WindowRoot/ConfirmDialog
```

연결:

```text
dialogRoot          -> ConfirmDialog/DialogRoot
titleText           -> DialogRoot/Panel/TitleText
messageText         -> DialogRoot/Panel/MessageText
confirmButton       -> DialogRoot/Panel/ConfirmButton
cancelButton        -> DialogRoot/Panel/CancelButton
```

## 그대로 둬야 하는 것

아래 프리팹은 별도 인벤토리 작업이 아니라면 수정하지 마세요:

```text
Assets/Prefabs/Game UI/Inventory 1.prefab
Assets/Prefabs/Game UI/InventorySlot.prefab
```

이 프리팹들의 `InventoryUI`나 `InventorySlotUI`는 교체하지 않습니다. 상점은 원본 인벤토리 슬롯을 직접 쓰지 말고, 복제한 슬롯 프리팹을 사용해야 합니다.

## Play Mode 테스트 체크리스트

프리팹과 씬 연결 후 Play Mode에서 확인:

```text
1. B 키 누르기
   기대 결과: Store 창이 열린다.

2. B 키 다시 누르기
   기대 결과: Store 창이 닫힌다.

3. 낚싯대 탭 선택
   기대 결과: ItemListPanel에 낚싯대 목록이 생성된다.

4. 미끼 탭 선택
   기대 결과: ItemListPanel에 미끼 목록이 생성된다.

5. 낚싯대 또는 미끼 선택
   기대 결과: DetailPanel 정보가 갱신된다.

6. 구매 버튼 클릭
   기대 결과: ConfirmDialog가 뜨고, 확인하면 골드가 차감된다.

7. 장착 / 해제 클릭
   기대 결과: 장착 상태와 배지가 갱신된다.

8. 물고기 판매 탭 선택
   기대 결과: DetailPanel은 숨겨지고 InventoryPanel이 보인다.

9. 물고기 1마리 판매
   기대 결과: ConfirmDialog가 뜨고, 확인하면 물고기가 제거되고 골드가 증가한다.

10. 전체 판매
    기대 결과: 보유 물고기가 모두 판매되고 골드가 증가한다.
```

## 자주 하는 실수

`Store`에 `InventoryUI`를 그대로 두지 마세요. Store는 `ShopUI`를 써야 합니다.

상점 낚싯대/미끼 슬롯에 `InventorySlotUI`를 그대로 두지 마세요. 낚싯대/미끼 슬롯은 `ShopSlotUI`를 써야 합니다.

복제한 물고기 판매 슬롯에 `InventorySlotUI`를 그대로 두지 마세요. 판매 슬롯은 `ShopInventorySlotUI`를 써야 합니다.

`ShopUI.windowRoot`를 `Store` 자기 자신에 연결하지 마세요. 자식 `WindowRoot`를 연결해야 합니다.

Store를 고치는 중에 `Inventory 1.prefab`을 수정하지 마세요.

Store를 `Static UICanvas` 아래에 두지 마세요. Store는 `Dynamic UI Canvas` 아래에 두는 것이 맞습니다.

## 요약

현재 가장 안전한 작업 순서:

```text
1. Inventory 1.prefab은 기존 인벤토리로 유지.
2. Store.prefab을 상점으로 사용.
3. Store 루트에서 InventoryUI를 제거하고 ShopUI 추가.
4. Store/InventoryPanel을 WindowRoot로 이름 변경.
5. TopTapGroup을 TabButtons 구조로 변경.
6. ItemGridPanel 시각 요소를 활용해 ShopSlot.prefab 생성, ShopSlotUI 부착.
7. InventorySlot.prefab을 복제해 ShopInventorySlot.prefab 생성, ShopInventorySlotUI 부착.
8. WindowRoot 아래에 DetailPanel, InventoryPanel, ConfirmDialog 구성.
9. 모든 SerializeField를 Inspector에서 연결.
10. B 키, 탭 전환, 구매, 장착, 개별 판매, 전체 판매를 테스트.
```

## 자동 정리 도구

수동으로 프리팹을 하나씩 고치기 어렵다면 아래 Editor 메뉴를 실행하세요.

```text
Tools > UI > Repair Store And Inventory UI
```

이 메뉴는 `Assets/Editor/StoreInventoryUIRepairTool.cs`에 추가된 도구입니다. 실행하면 다음 작업을 자동으로 처리합니다.

```text
1. ShopSlot.prefab 생성
   - ShopSlotUI 연결
   - 상점 낚싯대/미끼 목록용 슬롯으로 사용

2. ShopInventorySlot.prefab 생성
   - ShopInventorySlotUI 연결
   - 상점의 물고기 판매 목록용 슬롯으로 사용

3. Store.prefab 재구성
   - InventoryUI 제거
   - ShopUI 추가
   - WindowRoot / TopBar / TabButtons / ItemListPanel / DetailPanel / InventoryPanel / ConfirmDialog 생성
   - ShopUI SerializeField 자동 연결
   - toggleKey를 B로 설정

4. PlayScene 배치 확인
   - Store를 Dynamic UI Canvas 아래로 이동
   - Inventory 1이 없으면 Dynamic UI Canvas 아래에 생성
   - Inventory 1.prefab 자체는 인벤토리 UI로 유지
```

실행 후 Unity Console에 컴파일 오류가 없는지 보고, Play Mode에서 위 체크리스트를 다시 확인하면 됩니다.
