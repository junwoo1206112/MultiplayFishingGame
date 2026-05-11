## ADDED Requirements

### Requirement: Shop item detail panel

In ShopView, the RightContent SHALL display detailed information of the selected shop item.

#### Scenario: Rod detail shown
- **WHEN** user selects a rod from the shop list
- **THEN** RightPanel shows: rod icon, rod name, rank, price, cast distance bonus, catch chance bonus, durability, description

#### Scenario: Bait detail shown
- **WHEN** user selects a bait from the shop list
- **THEN** RightPanel shows: bait icon, bait name, rank, price, catch chance bonus, description

### Requirement: Buy button

If the item is not owned, a "구매" (Buy) button SHALL be shown.

#### Scenario: Not enough gold
- **WHEN** user clicks "구매" button
- **AND** user does not have enough gold
- **THEN** a message SHALL indicate insufficient gold

#### Scenario: Successful purchase
- **WHEN** user clicks "구매" button
- **AND** user has enough gold
- **THEN** gold is deducted
- **THEN** item is added to owned list
- **THEN** UI refreshes

### Requirement: Equip/Unequip button

If the item is owned, buttons for 장착 (Equip) / 해제 (Unequip) SHALL be shown.

#### Scenario: Equip rod
- **WHEN** user clicks "장착" button on an owned rod
- **THEN** the rod is set as the equipped rod
- **THEN** UI refreshes

#### Scenario: Unequip rod
- **WHEN** user clicks "해제" button on the currently equipped rod
- **THEN** the equipped rod is cleared
- **THEN** UI refreshes

### Requirement: Gold display

ShopView SHALL display the user's current gold.

#### Scenario: Gold shown
- **WHEN** ShopView is active
- **THEN** current gold amount is displayed in the panel
