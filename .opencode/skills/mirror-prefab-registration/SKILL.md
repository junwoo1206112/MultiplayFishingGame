---
name: mirror-prefab-registration
description: Register networked prefabs so Mirror can spawn and unspawn them at runtime. Use when creating new networked GameObjects that need to be instantiated across clients.
license: MIT
metadata:
  audience: developers
  workflow: unity-mirror
  domain: networking
---

Register networked prefabs so Mirror can spawn them across all connected clients.

## When to Use This Skill

Use this skill when you:
- Create a new prefab with a `NetworkIdentity` component
- Need to spawn a networked object at runtime via `NetworkServer.Spawn()`
- Get the error "Spawn prefab not found" at runtime

## Steps

1. **Create the prefab**

   - Create a new GameObject in the scene or hierarchy
   - Add required components:
     - `NetworkIdentity` (required for all networked objects)
     - `NetworkBehaviour` scripts (your custom networked logic)
   - Configure the `NetworkIdentity`:
     - **Server Only**: Check if the object should never exist on clients
     - **Client Authority**: Check if the client should have authority over this object
   - Drag the configured GameObject from the hierarchy into a `Assets/Prefabs/` folder to create a prefab
   - Delete the original from the scene

2. **Register the prefab** — choose one method:

   **Method A: NetworkManager spawnPrefabs list (recommended for most cases)**

   - Select the NetworkManager in the scene
   - In the Inspector, find the `Spawn Prefabs` list
   - Set the size and drag your prefab into a slot
   - This is the simplest method for a small number of prefabs

   **Method B: Programmatic registration (for larger projects)**

   ```csharp
   using UnityEngine;
   using Mirror;
   using MultiplayFishing.Network;

   public classPrefabRegistrar : NetworkBehaviour
   {
       [Header("Network Prefabs")]
       [SerializeField] private GameObject fishPrefab;
       [SerializeField] private GameObject fishingLinePrefab;
       [SerializeField] private GameObject bobberPrefab;

       public override void OnStartServer()
       {
           NetworkClient.RegisterPrefab(fishPrefab);
           NetworkClient.RegisterPrefab(fishingLinePrefab);
           NetworkClient.RegisterPrefab(bobberPrefab);
       }

       public override void OnStopServer()
       {
           NetworkClient.UnregisterPrefab(fishPrefab);
           NetworkClient.UnregisterPrefab(fishingLinePrefab);
           NetworkClient.UnregisterPrefab(bobberPrefab);
       }
   }
   ```

   **Method C: Custom spawn handler (for dynamic/pooled objects)**

   ```csharp
   // 서버에서 등록
   NetworkClient.RegisterHandler<FishSpawnMessage>(msg =>
   {
       // 커스텀 생성 로직 (풀링 등)
       GameObject fish = fishPool.Get();
       NetworkServer.Spawn(fish);
   });
   ```

3. **Spawn the object at runtime**

   ```csharp
   // 서버에서만 호출
   GameObject instance = Instantiate(myPrefab, position, rotation);
   NetworkServer.Spawn(instance);

   // 특정 클라이언트에 권한 부여
   NetworkServer.Spawn(instance, connectionToClient);

   // 제거
   NetworkServer.Destroy(instance);
   // 또는
   NetworkServer.UnSpawn(instance);
   ```

## Checklist

- [ ] Prefab has `NetworkIdentity` component
- [ ] Prefab registered in NetworkManager `spawnPrefabs` or via `NetworkClient.RegisterPrefab()`
- [ ] `NetworkServer.Spawn()` called only on server (use `[Server]` or `isServer` check)
- [ ] If using authority, pass the owning connection: `NetworkServer.Spawn(obj, conn)`
- [ ] Prefab asset exists in `Assets/Prefabs/` (not just in the scene)
- [ ] No `GameObject.Find()` — use references or NetworkIdentity

## Common Errors

| Error | Cause | Fix |
|-------|-------|-----|
| "Spawn prefab not found" | Prefab not registered | Add to NetworkManager spawnPrefabs or call RegisterPrefab |
| "Spawn prefab hash collision" | Two prefabs with same hash | Don't use custom spawn handlers with conflicting hashes |
| "ClientRpc called on unspawned object" | Object not yet spawned on client | Ensure Spawn completes before sending Rpcs |
| "NetworkIdentity not found" | Missing NetworkIdentity component | Add NetworkIdentity to the prefab |

## Rules

- Never modify files in `Assets/Mirror/`
- Prefab filename should match a meaningful name: `FishPrefab`, `BobberPrefab`, etc.
- Always call `NetworkServer.Spawn()` only on the server side
- Use `NetworkServer.Destroy()` for full removal, `NetworkServer.UnSpawn()` for pooling
- Prefer `[SerializeField] private` references over string-based loading