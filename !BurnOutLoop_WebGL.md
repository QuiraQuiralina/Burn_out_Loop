# WebGL Transition & Meta SDK Obliteration - Refined Implementation Plan

This plan details how to transition **Burn_out_Loop** from a Meta Quest 3 VR experience to a browser-compatible WebGL/Desktop experience. It details the complete deletion of all Meta/Oculus VR SDK elements, the introduction of a paused-by-default Desktop Player Controller, keyboard-based interactions, and a decoupled playthrough loop completion trigger.

---

## SOLID WebGL Architecture & System Overview

To achieve a clean WebGL structure, the project will transition to a keyboard-and-mouse driven architecture while keeping the core State-Machine Quest lifecycle intact:

```mermaid
graph TD
    GM[GameManager] -->|Updates Screen HUD| DPC[DesktopPlayerController]
    QSC[QuestStateController] -->|State Transitions| QB[QuestBase Scripts]
    
    QB -->|Q1| Q1[Quest1CleaningState]
    QB -->|Q2| Q2[Quest2StudyState]
    QB -->|Q3| Q3[Quest3KitchenState]
    QB -->|Q4| Q4[Quest4ShowerState]
    
    DPC -->|Raycast Interaction (E Key)| Interactables[Clutter, Books, Fridge, Food]
```

### 1. State-Driven Game Lifecycle (Unchanged)
The core states (`QuestBase`, `QuestStateController`, `GameManager`) remain active to preserve all quest transitions (`Q1 -> Q2 -> Q3 -> Q4 -> Exit Door`).

### 2. Screen Space Overlay HUD (WebGL Mode)
Instead of a VR wrist UI, the UI Canvas is configured as a **Screen Space - Overlay** canvas. It contains:
- **Quest Status Text**: Dynamic text displaying current goals.
- **Hunger Slider Bar**: Active during Quest 3 (Kitchen Snacking).
- **Fake Health / Countdown Timer**: Active during Quest 4 (Shower Climax).
- **Pause Menu Overlay**: Start/Pause display panel.
- **Center Reticle Dot**: A small dot image in the center of the screen.

---

## Feature-by-Feature Changes

### Feature 1: SDK Obliteration (First Phase)
All folders, assets, and configurations relating to Oculus, Meta, and XR plugins are deleted from the project to avoid compilation errors on non-Android build platforms.

#### [DELETE] [Oculus Folder](file:///d:/Repository/Burn_out_Loop/Assets/Oculus)
- Delete the folder and its `.meta` file.

#### [DELETE] [XR Folder](file:///d:/Repository/Burn_out_Loop/Assets/XR)
- Delete the folder and its `.meta` file.

#### [DELETE] [Resources Assets](file:///d:/Repository/Burn_out_Loop/Assets/Resources)
- Delete `MetaXRAcousticMaterialMapping.asset` (and `.meta`)
- Delete `MetaXRAcousticSettings.asset` (and `.meta`)
- Delete `MetaXRAudioSettings.asset` (and `.meta`)
- Delete `OculusRuntimeSettings.asset` (and `.meta`)

---

### Feature 2: Core Movement & Interaction System

#### [NEW] [DesktopPlayerController.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/DesktopPlayerController.cs)
A new script representing the first-person browser player:
- **WASD / Arrow Keys**: Player movement using a `CharacterController` component.
- **Mouse Look**: Camera lookup rotation with standard cursor locking.
- **Start / Pause Game State Manager**:
  - The game starts in a paused state (`timeScale = 0`, cursor visible, start/pause overlay UI is active).
  - Pressing **Enter** starts or resumes the game (locks cursor, hides pause UI, sets `timeScale = 1`).
  - Pressing **Escape** pauses the game (unlocks cursor, displays pause UI, sets `timeScale = 0`), allowing users to click out of the embedded WebGL browser window.
- **Center Reticle Dot**: Displays a small dot using a PNG texture (assignable in the Inspector) in the center of the screen.
- **Raycast Interaction (E Key)**:
  - Casts a ray from the camera center.
  - If looking at a clutter item (`ClutterSpawner`): Press `E` to collect.
  - If looking at the initial book spawner (`BookSpawner`): Press `E` to trigger book spawning.
  - If looking at a book (`BookHelper`): Press `E` to pick it up (attaches to a `holdPoint` in front of the camera).
  - If carrying a book and looking at a shelf socket (`BookSocket`): Press `E` to snap it into place.
  - If carrying a book and pressing `E` (or `G`) while *not* looking at a socket: Drops the book, re-enabling physics.
  - If looking at the fridge (`FoodCook`): Press `E` to spawn consumable food.
  - If looking at food (`FoodConsume`): Press `E` to eat it directly.
  - If looking at the unlocked exit door trigger: Walk through to escape.

---

### Feature 3: Quest 1 - Clutter Cleanup
- Modifies [ClutterSpawner.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/ClutterSpawner.cs) so that looking at it and pressing `E` triggers collection.
- Walking into clutter trigger colliders and pressing `E` can also trigger collection.

---

### Feature 4: Quest 2 - Book Spawning & Shelf Snapping
- Modifies [BookSpawner.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/BookSpawner.cs) so that spawned books instantiate in a circle around the player's current first-person transform position rather than a VR hand anchor transform.

---

### Feature 5: Quest 3 - Kitchen Snacking (Simplified)
- Modifies [FoodCook.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/FoodCook.cs) to simplify interaction:
  - Interacting with the fridge (pressing `E` on it) directly instantiates the consumable food prefab (`cookedFoodPrefab`) at the fridge spawn point.
  - All cooking surfaces, stove triggers, and cooking timers are bypassed.
- Modifies [FoodConsume.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/FoodConsume.cs):
  - Looking at the spawned food and pressing `E` consumes it directly, adding `10%` to the hunger bar and dissolving nearby food.

---

### Feature 6: Quest 4 - Climax Shower & Exit Doorway Completion
- Modifies [BedRecoveryTrigger.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/BedRecoveryTrigger.cs):
  - Exposes public parameters for `frontDoorObject` (to disable/open) and `exitTriggerObject` (to enable/activate).
  - On playthrough 3 bed/pool collision: stops the timer, disables the `frontDoorObject` (opening the doorway), enables the `exitTriggerObject`, and updates HUD status to: `"Escape through the front door!"`.
- **[NEW]** [ExitTrigger.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/ExitTrigger.cs):
  - Attached to the exit doorway trigger.
  - Exposes a `public UnityEngine.Events.UnityEvent OnExitReached;`
  - Walking through the doorway (tagged `"Player"`) completes Quest 4/Game, and invokes the `OnExitReached` callback so you can trigger custom scene loads or quit logic in the editor (no hardcoded credits canvas).

---

## Proposed File Changes

These are the exact scripts that will be created or modified under `Assets/Scripts/`:

### Architecture & Managers
#### [MODIFY] [GameManager.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/GameManager.cs)
Remove all VR hand-parenting properties, offsets, and wrist-HUD positioning functions.
#### [MODIFY] [BedRecoveryTrigger.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/BedRecoveryTrigger.cs)
On loop 3, unlocks the front door and activates the exit trigger instead of displaying credits.

### WebGL Input & Interaction
#### [NEW] [DesktopPlayerController.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/DesktopPlayerController.cs)
First-person controller with keyboard WASD movement, mouse camera look, cursor locking (ESC to unlock/pause, Enter to lock/start), reticle PNG dot display, and raycast interaction mapping.
#### [NEW] [ExitTrigger.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/ExitTrigger.cs)
Trigger script placed at the exit doorway that invokes a UnityEvent callback upon exit.

### Quest Mechanics
#### [MODIFY] [BookSpawner.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/BookSpawner.cs)
Radial book spawning centered on player controller coordinates.
#### [MODIFY] [FoodCook.cs](file:///d:/Repository/Burn_out_Loop/Assets/Scripts/FoodCook.cs)
Simplified fridge snacking trigger spawning cooked food directly.

---

## Verification Plan

### Compiler Checks
- Run `dotnet build Assembly-CSharp.csproj` to confirm zero errors or warning states.

### Playtest Verification
1. Verify Meta/XR assets are deleted.
2. Launch in editor:
   - Game starts paused, mouse is free.
   - Press **Enter**: cursor locks, movement WASD is enabled.
   - Press **Escape**: pause state is triggered, cursor is released.
   - Verify Reticle Dot shows in screen center.
   - Interact with clutter items, book spawner, fridge food, and books using the `E` key.
   - In loop 3, recover at the bed to unlock the front door, walk through to trigger `OnExitReached`.
