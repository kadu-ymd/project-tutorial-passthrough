# Session Summary — Ball Throwing & Target System

## Date
2026-04-24

## Features Implemented

### 1. Throwable Ball
- **File:** `Assets/Generated/Ball.prefab`
- Sphere with Rigidbody, SphereCollider, and Meta XR `Grabbable` component
- High mass (`10`) for heavy impact feel
- `HeavyBall.cs` script reduces gravity effect (`gravityScale = 0.3`)
- Ball destroys itself 0.05s after hitting a valid target (allows physics impulse to transfer)

### 2. Projectile Logic
- **File:** `Assets/Generated/Projectile.cs`
- Detects collisions with `StalkerObject` (StalkerCube) or objects named "TargetBlock"
- Plays sound on StalkerCube hit (if assigned)
- Destroys StalkerCube on impact
- Only self-destructs when hitting valid targets (not walls/floor)

### 3. Pinch-to-Spawn Ball
- **File:** `Assets/Generated/PinchBallSpawner.cs`
- Spawns a new ball at the pinch point (thumb + index) every time pinch starts
- Supports left and right hands via `OVRHand` references
- Configurable forward offset to avoid hand clipping

### 4. Target Block Pile
- **File:** `Assets/Generated/TargetBlock.prefab`
- Simple physical cube (Rigidbody + BoxCollider) that reacts to impacts
- **File:** `Assets/Generated/BlockPileSpawner.cs`
- Spawns a grid pile of blocks on **every TABLE** surface detected by MRUK
- Waits for MRUK room data before spawning (coroutine polling pattern)
- Configurable pile dimensions, spacing, and scale

### 5. Score System
- **File:** `Assets/Generated/ScoreManager.cs`
- Singleton pattern — accessible from any script
- Awards `basePoints + (distance × distanceMultiplier)` per hit
- More points for hitting distant targets
- Updates a Unity UI `Text` component
- **File:** `Assets/Generated/Projectile.cs` (updated) — reports hits to ScoreManager

### 6. StalkerCube Updates
- **File:** `Assets/Scripts/StalkerObject.cs`
- Added `AudioClip hitSound` field for per-stalker sound effects

---

## Files Created / Modified

### Created in `Assets/Generated/`
| File | Purpose |
|------|---------|
| `Ball.prefab` | Throwable sphere with grab & throw support |
| `Projectile.cs` | Collision logic for ball vs targets |
| `HeavyBall.cs` | Custom gravity scale for heavy-but-floaty feel |
| `PinchBallSpawner.cs` | Spawns ball on hand pinch gesture |
| `TargetBlock.prefab` | Physical cube target |
| `BlockPileSpawner.cs` | Runtime spawner for block piles on tables |
| `ScoreManager.cs` | Score tracking & distance-based points |

### Modified
| File | Change |
|------|--------|
| `Assets/Scripts/StalkerObject.cs` | Added `hitSound` AudioClip field |

---

## Manual Setup Required in Unity

### Ball Prefab
1. Open `Assets/Generated/Ball.prefab`
2. Add the **`Projectile`** script component
3. Add the **`HeavyBall`** script component
4. Assign an **AudioClip** to `Default Hit Sound` on Projectile (optional)

### Pinch Ball Spawner
1. Create empty GameObject → name it `PinchBallSpawner`
2. Add `PinchBallSpawner.cs`
3. Assign `Ball.prefab` to **Ball Prefab**
4. Assign `OVRHand` components for **Left Hand** and **Right Hand**
   - Usually found under `OVRCameraRig > TrackingSpace > LeftHandAnchor > OVRHand`

### Block Pile Spawner
1. Create empty GameObject → name it `BlockPileSpawner`
2. Add `BlockPileSpawner.cs`
3. Assign `TargetBlock.prefab` to **Block Prefab**
4. Adjust pile size/height/depth in Inspector

### Score Display
1. Create empty GameObject → name it `ScoreManager`
2. Add `ScoreManager.cs`
3. Create **UI > Text** in the scene
4. Set Canvas **Render Mode** to `World Space`
5. Scale Canvas to `(0.005, 0.005, 0.005)` and position in front of player
6. Assign the Text element to **Score Text** on ScoreManager

---

## Known Issues & Notes

- **Meta files:** Unity auto-generates `.meta` files on import. Do not create `.meta` files manually — let Unity handle GUIDs.
- **Prefab GUIDs:** When adding scripts to prefabs in Unity, always use the Inspector to avoid broken GUID references.
- **Line endings:** On Windows, Unity expects CRLF (`\r\n`) in `.meta` files. The WriteFile tool writes LF-only, which causes YAML parse errors.
- **MRUK readiness:** All MRUK-dependent scripts use coroutine polling:
  ```csharp
  while (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
      yield return null;
  ```
- **Guardian / Floor height:** If the virtual floor feels too high, check:
  1. Quest Settings → Guardian → Reset/Recalibrate floor
  2. Unity Camera Rig Tracking Origin Mode = `Floor` (not `Device`)
  3. Camera Rig Transform Y position = `0`

---

## Scoring Formula
```
points = basePoints + Round(distance × distanceMultiplier)
```
Defaults:
- `basePoints = 100`
- `distanceMultiplier = 20`

Example: hitting a target 3.5 meters away = `100 + Round(3.5 × 20)` = **170 points**

---

## Branch
`passthrough/carlos-v0`
