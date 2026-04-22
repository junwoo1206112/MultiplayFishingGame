# 🤖 Mandatory Instructions for AI Assistants

This project is a Mirror Networking-based Multiplayer Fishing Game. Any AI assistant working on this codebase **MUST** strictly adhere to the following rules and resources before writing or modifying any code.

## 1. Project-Specific Skills & Conventions
All coding styles, naming conventions, and Mirror-specific patterns are defined in:
👉 **`.opencode/skills/project-conventions/SKILL.md`**

**Key Rules:**
- **Networking**: Use Mirror Networking. Never write custom low-level socket code.
- **Movement**: Use Mirror's standard `PlayerController (Reliable)`. Do NOT create custom movement scripts (already decided and documented).
- **UI**: Use TextMeshPro (TMP) for all UI elements.
- **Namespaces**: Follow the `MultiplayFishing.[Category]` structure.

## 2. Design Specs & Task Tracking
Current project goals and implementation details are managed via OpenSpec:
👉 **`openspec/` folder**

- Read `openspec/specs/` for architectural decisions.
- Follow active changes in `openspec/changes/`.

## 3. Mandatory Architecture Standards

### 🛡️ Dependency Injection (DI) Pattern
- **Manager Classes**: All core managers (Data, Excel, Sound, etc.) MUST be accessed via interfaces (e.g., `IDataService`).
- **No Singletons**: `Instance` pattern is prohibited. Use `DIContainer` for service registration and resolution.

### 📊 Data System (NPOI ONLY)
- **Format**: Master data MUST be managed via `.xlsx` Excel files.
- **Library**: Use **NPOI** for Excel handling.
- **NO CSV**: Custom CSV parsing or usage is strictly prohibited to maintain data consistency.

### 🐟 Fish Data Logic
- **Persitence**: Caught fish go to both Inventory and Encyclopedia.
- **Persistence Logic**: Selling a fish removes it from Inventory but **MUST KEEP** it in the Encyclopedia.

## 4. Communication Style
- Be concise and technical.
- Always explain the "Why" behind a code change based on existing conventions.
- If a task involves networking, refer to the `mirror-network-behaviour` and `mirror-sync-setup` skills.

---

*Note to Human Team Members: Please ensure your AI tool reads this file or copy-paste this content into the first prompt to maintain consistency across the team.*
