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

## 3. Communication Style
- Be concise and technical.
- Always explain the "Why" behind a code change based on existing conventions.
- If a task involves networking, refer to the `mirror-network-behaviour` and `mirror-sync-setup` skills.

---

*Note to Human Team Members: Please ensure your AI tool reads this file or copy-paste this content into the first prompt to maintain consistency across the team.*
