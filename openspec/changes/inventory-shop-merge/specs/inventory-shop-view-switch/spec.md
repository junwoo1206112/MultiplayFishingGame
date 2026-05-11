## ADDED Requirements

### Requirement: View mode switching

InventoryUI SHALL support two view modes: InventoryView and ShopView.
SideTabs buttons SHALL toggle between views.

#### Scenario: Switch to shop view
- **WHEN** user clicks "샵" button in SideTabs
- **THEN** LeftContent displays shop item list instead of fish grid

#### Scenario: Switch to inventory view
- **WHEN** user clicks "인벤" button in SideTabs
- **THEN** LeftContent displays fish grid instead of shop item list

#### Scenario: Default view on open
- **WHEN** InventoryUI window opens (Tab key)
- **THEN** view mode SHALL be InventoryView

### Requirement: Content area replacement

LeftContent SHALL clear all children when switching views.

#### Scenario: Content swap
- **WHEN** view mode changes
- **THEN** all existing slot objects are destroyed
- **THEN** new content matching the view mode is instantiated

### Requirement: Cursor state

#### Scenario: Window opens
- **WHEN** Inventory window opens
- **THEN** cursor SHALL be unlocked and visible

#### Scenario: Window closes
- **WHEN** Inventory window closes
- **THEN** cursor SHALL be locked and hidden
