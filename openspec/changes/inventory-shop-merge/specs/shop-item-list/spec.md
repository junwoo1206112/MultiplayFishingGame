## ADDED Requirements

### Requirement: Shop item list display

In ShopView, LeftContent SHALL display a list of purchasable items (rods and baits).

#### Scenario: Shop view opens
- **WHEN** view mode switches to ShopView
- **THEN** LeftContent shows a scrollable list of rods and baits

#### Scenario: Item slots created
- **WHEN** shop view is refreshed
- **THEN** one slot per rod/bait from IDataService is instantiated

### Requirement: Item slot visual

Each shop item slot SHALL show: icon, name, rank, price.

#### Scenario: Slot content
- **WHEN** a shop slot is created
- **THEN** the slot SHALL display the item icon
- **THEN** the slot SHALL display the item name
- **THEN** the slot SHALL display the item rank (star rating)
- **THEN** the slot SHALL display the item price

### Requirement: Owned/equipped state

#### Scenario: Owned item shown
- **WHEN** a rod/bait in the list is already owned by the user
- **THEN** the slot SHALL show an "owned" indicator

#### Scenario: Equipped item shown
- **WHEN** a rod/bait is currently equipped
- **THEN** the slot SHALL show an "equipped" indicator

### Requirement: Item selection

#### Scenario: Item clicked
- **WHEN** user clicks a shop item slot
- **THEN** the RightContent SHALL show the item's detail information
