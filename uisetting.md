# 🎨 UI/UX 초정밀 배치 및 기술 명세서 (UI Setting Guide)

본 문서는 디자이너와 개발자가 유니티 에디터에서 UI를 구성할 때 필요한 **계층 구조, 컴포넌트 설정, 앵커 포인트**를 정의합니다.

---

## 1. 캔버스 기본 설정 (Canvas Setup)

| 항목 | 설정값 | 비고 |
| :--- | :--- | :--- |
| **UI Scale Mode** | Scale With Screen Size | 기준 해상도: 1920 x 1080 |
| **Screen Match Mode** | Match (Width or Height: 0.5) | 모든 화면 비율 대응 |
| **Graphic Raycaster** | Enabled | 버튼 클릭 필수 |

---

## 2. 플레이어 상태 HUD (Top-Left)

플레이어의 기본 성장을 실시간으로 보여주는 영역입니다.

- **Anchor**: `Top-Left` (Pivot: 0, 1)
- **Position**: X: 20, Y: -20
- **계층 구조 및 컴포넌트**:
    - `PlayerStatus_Group` (Horizontal Layout Group)
        - `Tier_Badge` (Image): 티어 배경 아이콘
            - `Tier_Text` (TextMeshPro): "Tier 1"
        - `Exp_Container` (Image): 경험치 바 배경
            - `Exp_Fill` (Image): Slider의 Fill 영역 (Color: #90EE90)
            - `Exp_Text` (TextMeshPro): "150 / 500" (Center)
        - `Gold_Container` (Image)
            - `Gold_Icon` (Image): 코인 아이콘
            - `Gold_Text` (TextMeshPro): "12,500 G" (Color: #FFD700)

---

## 3. 낚시 미니게임 HUD (Center / Dynamic)

낚시 중 나타나는 상황별 UI입니다. `FishingUI.cs`에 할당해야 합니다.

### 3.1 캐스팅 충전 바 (`ChargingPanel`)
- **Anchor**: `Bottom-Center` (X: 0, Y: 250)
- **Size**: 400 x 40 (가로형)
- **설정**: 
    - `Slider` 컴포넌트: Interactable 해제, Transition None.
    - **색상**: 0% (흰색) -> 100% (주황색) 그라데이션 권장.

### 3.2 입질 경고 (`AlertPanel`)
- **Anchor**: `Center` (X: 0, Y: 150)
- **Size**: 100 x 100
- **구성**:
    - `Alert_Icon` (Image): 빨간색 느낌표(!).
    - **애니메이션**: `Punch Scale` 또는 `Floating` 애니메이션 적용 시 가시성 향상.

### 3.3 연타 게이지 (`CatchingPanel`)
- **Anchor**: `Center` (X: 0, Y: 0)
- **계층 구조**:
    - `Catching_Root`
        - `Fish_Info_Text` (TMP): "잡는 중: [별점] 물고기 이름"
        - `Spam_ProgressBar` (Slider): 600 x 60 사이즈.
        - `Spam_Instruction` (TMP): "왼쪽 버튼을 연타하세요!" (Blinking 효과)

---

## 4. 인벤토리 및 도감 (Full Overlay)

`Tab` 또는 `E` 키로 활성화되는 전체 화면 창입니다.

### 4.1 공통 창 구조
- **Anchor**: `Center` (Stretch All)
- **Background**: 검은색 반투명 (#000000, Alpha: 180).
- **Close Button**: 우측 상단 'X' 버튼 (Esc 키와 연동).

### 4.2 인벤토리 슬롯 (Slot UI)
- **Component**: `GridLayoutGroup`
- **Cell Size**: 180 x 220
- **Spacing**: X: 15, Y: 15
- **슬롯 내부 구성**:
    - `Icon_Image`: 물고기 스프라이트 (Preserve Aspect 체크).
    - `Rank_Stars`: TMP 텍스트 (예: "★★★").
    - `Length_Text`: 하단 우측 배치 (예: "45.2cm").

---

## 5. 알림 시스템 (Top-Right Notification)

- **Anchor**: `Top-Right` (Pivot: 1, 1)
- **Position**: X: -20, Y: -20
- **계층 구조**:
    - `Notification_Container` (Vertical Layout Group)
        - **Padding**: Top: 10, Right: 10
        - **Child Alignment**: Upper Right
        - **Child Force Expand**: Width: No, Height: No
    - `Notification_Item` (Prefab)
        - `Content_Text` (TMP): "OOO님이 5성 물고기 획득!"
        - `Background` (Image): Slice 방식의 9-Grid 이미지 권장.

---

## 6. 개발팀 전달 가이드 (Important)

1.  **모든 텍스트**는 `TextMeshPro - Text`를 사용하세요. (기본 UI Text 금지)
2.  **색상 상수**:
    - 별점(Star): #FFCC00 (Gold)
    - 레벨업/성공: #00FF00 (Green)
    - 실패/위험: #FF0000 (Red)
3.  **이미지 필터**: 도트 스타일 유지 시 `Filter Mode: Point`, `Compression: None` 설정 필수.
4.  **연결**: 모든 UI 패널은 생성 후 `FishingUI` 또는 `InventoryUI` 인스펙터의 해당 필드에 반드시 드래그하여 할당해야 합니다.

---
*본 가이드는 v2.0 초정밀 버전으로 업데이트되었습니다.*
