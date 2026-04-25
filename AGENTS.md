# AGENTS.md

> Agent-facing documentation for `project-tutorial-passthrough`.  
> Last updated: 2026-04-24

---

## 1. Project Overview

`project-tutorial-passthrough` is a **Unity 6 VR/MR prototype** targeting the **Meta Quest 2** headset. It is built around **passthrough mixed-reality** and **scene understanding** using Meta's MR Utility Kit (MRUK). The current feature set includes:

- Spawning interactable objects on real-world surfaces detected by MRUK.
- Runtime NavMesh baking so that AI agents can navigate the user's physical room.
- A simple "stalker" AI that follows the player's head (HMD).
- Experimental tools for exporting and aligning reconstructed room geometry to MRUK anchors.
- Chess-piece scenes used for early prototyping.

The project is **research / tutorial grade**: code is minimal, there is no formal architecture, no automated tests, and no CI/CD pipeline. The repository lives at `https://github.com/kadu-ymd/project-tutorial-passthrough` and is currently on branch `passthrough/carlos-v0`.

---

## 2. Technology Stack

| Layer | Technology | Version / Notes |
|---|---|---|
| **Unity Editor** | Unity 6000.3.9f1 | Unity 6 LTS line |
| **Render Pipeline** | Universal Render Pipeline (URP) | 17.3.0 |
| **XR Framework** | OpenXR + Meta XR SDK | OpenXR 1.16.1, Meta XR SDK 85.0.0 |
| **Input** | Unity Input System | 1.18.0 |
| **Navigation** | Unity AI Navigation (NavMesh) | 2.0.10 |
| **Networking** | Netcode for GameObjects | 2.11.0 *(in manifest, no usage in scripts)* |
| **Multiplayer Services** | Unity Multiplayer Services | 2.2.1 *(in manifest, no usage in scripts)* |
| **Level Design** | ProBuilder | 6.0.9 |
| **Scripting Backend (PC)** | Mono | StandaloneWindows64 |
| **Scripting Backend (Android)** | IL2CPP | ARM64-only |
| **C# Language Level** | C# 9.0 | Target framework .NET Framework 4.7.1 |
| **IDE Support** | Visual Studio, Rider | `com.unity.ide.visualstudio` 2.0.26, `com.unity.ide.rider` 3.0.39 |

### Key Unity Packages
- `com.meta.xr.sdk.all` (85.0.0) — Core Meta XR SDK, including MR Utility Kit, Interaction, Audio, Platform, etc.
- `com.unity.xr.management` (4.5.4)
- `com.unity.xr.openxr` (1.16.1)
- `com.unity.render-pipelines.universal` (17.3.0)
- `com.unity.ai.navigation` (2.0.10)
- `com.unity.inputsystem` (1.18.0)

### Scripting Define Symbols
- `OVR_DISABLE_HAND_PINCH_BUTTON_MAPPING`
- `USE_INPUT_SYSTEM_POSE_CONTROL`
- `USE_STICK_CONTROL_THUMBSTICKS`

---

## 3. Project Structure & Code Organization

```
Assets/
├── Scripts/                     # Core gameplay scripts (flat, no sub-folders)
│   ├── EntitySpawner.cs         # Spawns prefabs on MRUK vertical surfaces
│   ├── RuntimeNavmeshBuilder.cs # Builds NavMeshSurface at runtime
│   └── StalkerObject.cs         # NavMeshAgent that follows Camera.main
│
├── RoomExporter.cs              # NEW / uncommitted: exports MRUK anchors as cubes
├── RoomAligner.cs               # NEW / uncommitted: aligns reconstructed room prefab to MRUK room
│
├── Prefabs/
│   ├── InteractableCube.prefab
│   ├── Obstacle Box.prefab      # NavMesh obstacle
│   ├── StalkerCube.prefab       # AI agent prefab
│   └── TouchInteractableCube.prefab
│
├── Scenes/
│   ├── 0.unity                  # NEW: clean passthrough / MRUK template
│   ├── carlos-scene.unity       # Chess scene (IN BUILD)
│   ├── Main Scene.unity         # Hand-tracking + EntitySpawner demo (NOT in build)
│   └── Yassuda.unity            # Chess + hand-tracking scene (IN BUILD)
│
├── Chess Set/                   # 3D chess-piece FBXs, materials, prefabs
├── Hovl Studio/Magic sword/     # VFX asset pack
├── Oculus/                      # OculusProjectConfig.asset
├── Plugins/Android/             # Android-specific plugins
├── Resources/                   # Runtime-loaded configs (InputActions, Meta XR settings)
├── Settings/                    # URP assets (PC_RPAsset, Mobile_RPAsset, renderers)
├── StreamingAssets/
├── TutorialInfo/                # Unity template readme assets
│   └── Scripts/
│       ├── Readme.cs
│       └── Editor/ReadmeEditor.cs
│
├── XR/                          # XR loader & settings (OpenXR, Oculus)
│   ├── XRGeneralSettingsPerBuildTarget.asset
│   └── Settings/
│       ├── OculusSettings.asset
│       └── OpenXRPackageSettings.asset
│
└── ... (other asset folders)
```

### Assemblies
- **`Assembly-CSharp`** — All runtime scripts (including those in `Assets/` root).
- **`Assembly-CSharp-Editor`** — Editor-only scripts (`ReadmeEditor.cs`).

---

## 4. Scene Architecture & Runtime Flow

### Scenes in Build Settings
| Build Index | Scene | Description |
|---|---|---|
| 0 | `Assets/Scenes/carlos-scene.unity` | Chess board with ~17 chess-piece prefabs. |
| 1 | `Assets/Scenes/Yassuda.unity` | Chess scene with hand-tracking rig objects. |

**Important:** `Main Scene.unity` and `0.unity` are **not** currently included in `EditorBuildSettings.asset`. Any standalone build will omit them unless re-added.

### Runtime Object Hierarchy Patterns
- **Meta XR Building Blocks** are used heavily in scenes: `[BuildingBlock] Passthrough`, `[BuildingBlock] MR Utility Kit`, `[BuildingBlock] Camera Rig`.
- **Oculus hand anchors** (`LeftHandAnchorDetached`, `RightControllerInHandAnchor`, etc.) appear in hand-tracking scenes.
- **`EntitySpawner`** (in `Main Scene.unity`) periodically instantiates `StalkerCube.prefab` on MRUK-detected surfaces.
- **NavMesh setup:** `Obstacle Box.prefab` carries a `NavMeshModifier` (area = 1) to mark walkable/obstacle regions. `RuntimeNavmeshBuilder.cs` rebuilds the surface when MRUK signals a room update.

### MRUK Initialization Pattern
Several scripts use a **coroutine polling loop** to wait for MRUK readiness:

```csharp
while (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
    yield return null;
```

This is the dominant async pattern in the project. If you add new MRUK-dependent features, follow this convention.

---

## 5. Key Classes & Responsibilities

| Class | File | Responsibility |
|---|---|---|
| `EntitySpawner` | `Assets/Scripts/EntitySpawner.cs` | Timer-based spawner. Uses `MRUK.Instance.GetCurrentRoom().GenerateRandomPositionOnSurface(...)` targeting **vertical surfaces** (`WALL_FACE`), forces `y = 2`, and instantiates `prefabToSpawn`. |
| `RuntimeNavmeshBuilder` | `Assets/Scripts/RuntimeNavmeshBuilder.cs` | Caches a `NavMeshSurface`, builds on `Start`, and rebuilds via coroutine when MRUK fires `RegisterSceneLoadedCallback`. |
| `StalkerObject` | `Assets/Scripts/StalkerObject.cs` | Simple `NavMeshAgent`. Every `Update` sets destination to `Camera.main.transform.position`. |
| `RoomExporter` | `Assets/RoomExporter.cs` | Creates a runtime GameObject hierarchy of cubes representing MRUK anchors. Maps `PlaneRect` or `VolumeBounds` to scale, names objects by anchor label, and colors them via a switch expression. |
| `RoomAligner` | `Assets/RoomAligner.cs` | Aligns a user-provided reconstructed-room prefab to the real MRUK room transform. Supports anchor-level rotation correction and manual offset. |
| `Readme` / `ReadmeEditor` | `Assets/TutorialInfo/Scripts/...` | Unity template readme asset and its custom inspector. Safe to ignore or remove. |

---

## 6. Build Process

### Build Target Matrix
| Platform | Active Target | Scripting Backend | Architectures |
|---|---|---|---|
| **Standalone Windows** | Yes (Editor default) | Mono | x64 |
| **Android** | Yes (Quest deployment) | IL2CPP | ARM64 |

### How to Build
1. Open the project in **Unity 6000.3.9f1**.
2. Open **File > Build Settings**.
3. Select the desired platform (PC or Android).
4. Ensure scenes are added to the build list in the desired order.
5. Click **Build** or **Build And Run**.

> **Note:** There is no command-line build script, no CI/CD, and no automated packaging. Builds are performed manually from the Unity Editor. A legacy Windows standalone build already exists in the repository under `Build/`.

### Render Pipeline
- **PC:** `PC_RPAsset.asset` → `PC_Renderer.asset`
- **Mobile (Quest):** `Mobile_RPAsset.asset` → `Mobile_Renderer.asset`
- `UniversalRenderPipelineGlobalSettings.asset` is the shared global settings object.

---

## 7. Development Conventions

### Naming
- **PascalCase** for class names and public members.
- **camelCase** for private fields and local variables.
- No namespaces are used anywhere in the project.

### Language in Code
- **Primary language:** English.
- **Caveat:** Recent uncommitted scripts (`RoomAligner.cs`, `RoomExporter.cs`) contain **Portuguese comments, UI tooltips, and log strings** (e.g., `// Prefab Reconstruído`, `Debug.Log("✅ Room alinhada!")`). When modifying these files, maintain consistency with the existing language in that file, or translate to English if refactoring broadly.

### Patterns
- **Direct singleton access** to `MRUK.Instance` is the norm.
- **Coroutine-based initialization** is preferred over `async/await` for MRUK polling.
- **Inspector references** are used for prefabs; there are no `Resources.Load` calls in production code.
- **Minimal abstraction:** No service locators, no event buses, no scriptable-object architecture. Scripts are small, self-contained MonoBehaviours.

### Git Hygiene
- The working tree is frequently dirty with uncommitted scenes, settings, and new assets. Check `git status` before assuming a clean state.
- The branch `passthrough/carlos-v0` is the current active development branch.

---

## 8. Testing Strategy

**There is no automated test infrastructure in this project.**

- No `Tests/` assemblies.
- No unit tests, integration tests, or play-mode tests.
- No test runners configured.

**Manual testing workflow:**
1. Load the relevant scene in the Unity Editor or deploy to a Meta Quest 2 device.
2. For MRUK features, ensure the Quest has a scanned room (scene data) or use Meta's Link simulation.
3. Verify passthrough rendering, hand tracking, and object spawning visually in-headset.
4. Use `adb logcat` (Android) or the Unity Console (PC) to inspect Portuguese/English debug logs from `RoomExporter` and `RoomAligner`.

---

## 9. Deployment Process

- **Manual deployment only.**
- For Quest 2: Build an Android APK / AAB from Unity, then sideload via `adb install` or Meta Quest Developer Hub.
- For PC VR: Build a Windows standalone executable. A pre-built binary already exists at `Build/project-tutorial-passthrough.exe`.
- **No automated versioning:** `ProjectSettings/ProjectSettings.asset` shows bundle version `0.1.0`.

---

## 10. Security Considerations

- **Passthrough camera access is enabled.** `OculusProjectConfig.asset` sets `isPassthroughCameraAccessEnabled: 1` and `_insightPassthroughSupport: 1`. Any code running in this project has access to raw camera imagery on Quest. Do not log frame data or stream it off-device without explicit user consent.
- **Eye tracking support is enabled.** The project config declares `eyeTrackingSupport: 1`. Eye-tracking data is sensitive biometric information; treat it accordingly and do not transmit it to remote servers.
- **Scene data (room geometry) is processed at runtime.** MRUK anchors represent the user's physical environment. Avoid serializing or uploading this data unless necessary and consented.
- **Networking packages are present but unused.** `Netcode for GameObjects` and `Unity Multiplayer Services` are in the manifest but no networking code exists. If networking is added later, follow Unity and Meta's security best practices for multiplayer VR.

---

## 11. Quick Reference for Agents

| Task | Guidance |
|---|---|
| **Add a new C# script** | Place gameplay scripts in `Assets/Scripts/`. Place MRUK utility scripts in `Assets/` root to match recent convention, or create a new `Assets/Scripts/MRUK/` folder if the team agrees. |
| **Add a new scene** | Save to `Assets/Scenes/`. Add it to `File > Build Settings` if it needs to be in builds. |
| **Modify XR settings** | Edit `Assets/XR/Settings/OculusSettings.asset` or `OpenXRPackageSettings.asset`. Do not hard-code Quest 2-only restrictions unless intentional. |
| **Add a package** | Use Unity's Package Manager. Update `Packages/manifest.json` and `packages-lock.json`. Be mindful of Quest 2 performance. |
| **Change render pipeline** | Modify assets in `Assets/Settings/`. The active asset is selected in `GraphicsSettings` and `QualitySettings`. |
| **Refactor Portuguese comments** | If you perform a broad refactor of `RoomAligner.cs` or `RoomExporter.cs`, consider translating Portuguese comments to English for consistency with the rest of the codebase. |
| **Build for testing** | Use Unity Editor → `File > Build Settings` → target platform. There is no CLI build script. |

---

*End of AGENTS.md*
