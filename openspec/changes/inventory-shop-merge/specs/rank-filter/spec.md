## ADDED Requirements

### Requirement: Rank-based fish filtering

TopTapGroup SHALL filter the inventory fish grid by rank (star rating).

#### Scenario: Filter by rank group
- **WHEN** user clicks Tap 2 (하급)
- **THEN** only fish with ★~★★ rank are shown in the grid

#### Scenario: Show all
- **WHEN** user clicks Tap 1 (전체)
- **THEN** all fish in inventory are shown

#### Scenario: Mid rank filter
- **WHEN** user clicks Tap 3 (중급)
- **THEN** only fish with ★★★~★★★★ rank are shown

#### Scenario: Top rank filter
- **WHEN** user clicks Tap 4 (상급)
- **THEN** only fish with ★★★★★ rank are shown

### Requirement: Tab label mapping

| Tap | Label | Rank filter |
|-----|-------|-------------|
| Tap 1 | 전체 | All ranks |
| Tap 2 | 하급 | ★ (length=1) ~ ★★ (length=2) |
| Tap 3 | 중급 | ★★★ (length=3) ~ ★★★★ (length=4) |
| Tap 4 | 상급 | ★★★★★ (length=5) |

#### Scenario: Tab labels set at runtime
- **WHEN** InventoryUI starts
- **THEN** TopTapGroup button text SHALL be set to the Korean labels above

### Requirement: Active tab highlight

#### Scenario: Tab selected
- **WHEN** user clicks a TopTapGroup button
- **THEN** the selected tab SHALL appear highlighted (e.g., different color)
- **THEN** other tabs SHALL return to normal appearance

### Requirement: Filter applies only in InventoryView

#### Scenario: No filter in shop view
- **WHEN** view mode is ShopView
- **THEN** TopTapGroup SHALL not affect shop item list
