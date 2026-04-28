# MultiplayFishingGame — Copilot Instructions

## Tech Stack
- Unity 6 (6000.3.10f1), C# 9.0, netstandard2.1
- Mirror Networking v96.0.1 (server-authoritative)
- URP, New Input System, Cinemachine, TextMeshPro

## Folder → Namespace Mapping
| Folder | Namespace |
|--------|-----------|
| Assets/Scripts/Network/ | MultiplayFishing.Network |
| Assets/Scripts/Gameplay/ | MultiplayFishing.Gameplay |
| Assets/Scripts/Data/ | MultiplayFishing.Data |
| Assets/Scripts/Data/Models/ | MultiplayFishing.Data.Models |
| Assets/Scripts/UI/ | MultiplayFishing.UI |
| Assets/Scripts/Managers/ | MultiplayFishing.Core |
| Assets/Scripts/Utilities/ | MultiplayFishing.Utilities |
| Editor/ | MultiplayFishing.Editor |

## Code Style
- Allman braces, 4-space indent
- Private fields: camelCase, no underscore prefix
- Using order: System → UnityEngine → Mirror → MultiplayFishing
- Guard clauses over try-catch
- Log with `Debug.Log($"[ClassName] ...")`

## Mirror Patterns
- `[SyncVar(hook = nameof(OnXChanged))]` with `void OnXChanged(Type old, Type new)`
- `[Command]` → `Cmd` prefix, `[ClientRpc]` → `Rpc` prefix, `[TargetRpc]` → `Target` prefix
- Server validates all incoming Command/Rpc parameters
- Use `#region Server` / `#region Client` in NetworkBehaviour scripts

## Forbidden
- `GameObject.Find()`, `FindObjectOfType<T>()` — use `FindFirstObjectByType<T>()` or DI
- `Update()` for network sync — use SyncVar/ClientRpc
- Modifying Assets/Mirror/ — read-only
- Try-catch in hot paths — use guard clauses
- Underscore prefix on private fields
