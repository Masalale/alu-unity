# Unity - Assets: Models, Textures

## Description
A 3D platformer built in Unity. The player moves across floating platforms with
WASD and jumps with Space, a follow camera tracks the player and can be rotated
with the mouse, and a timer starts once the player leaves the starting platform
and stops on reaching the flag. Cube placeholders are later replaced with 3D
models, materials and textures, and a skybox provides the environment.

## Requirements
- All C# files compiled using dotnet / Unity C# compiler
- Projects built with Unity
- A `README.md` file at the root of the project folder
- All scene GameObjects, materials, and prefabs organized per task requirements
- Unity version 6000.3.23f1

## Tasks

### 0. Primitive player
- Created the scene `Assets/Scenes/Level01.unity` with a capsule GameObject named `Player`.
- Position: `(0, 1.25, 0)`, Rotation: `(0, 0, 0)`, Scale: `(1, 1, 1)`.
- Saved `Player` as a prefab at `Assets/Prefabs/Player.prefab`.

### 1. Primitive prototype
- Created an empty GameObject named `Platforms` at position `(0, 0, 0)` to hold the level layout.
- Added 10 floating cube platforms, named `Platform01` to `Platform10`, all parented to `Platforms`.
- `Platform01` sits directly under the `Player` at `(0, -0.5, 0)` with scale `(5, 1, 5)` and acts as the starting platform.
- Every platform uses a different position and a different size, from `2x1x2` stepping stones to `5x1x5` wide landing pads.
- Consecutive gaps are 1.0 to 1.5 units with 1.0 unit rises, giving a clear jumpable path from the player to `Platform08`.
- `Platform09` and `Platform10` are optional dead-end branches off `Platform04` and `Platform07`.

### 2. Pole position
- Added a placeholder cylinder GameObject named `WinFlag` at the end point of the platform path, on top of `Platform08` at `(30, 3, 0)`.
- `WinFlag` is a scene root and is not parented under `Platforms`, so it stays independent of the level layout.
- Rotation is `(0, 0, 0)` and scale is `(1, 1, 1)`, the default cylinder proportions.
- The Capsule Collider on `WinFlag` is a trigger: Is Trigger `Yes`, Radius `0.7`, Height `6`, Direction `1` (Y axis).
- The 6 unit tall trigger spans `y = 0` to `y = 6` on the end platform, so it registers when the player reaches the flag.

### 3. Jump man
- Added `Assets/Scripts/PlayerController.cs` and attached it to the `Player` GameObject, so the capsule now takes input.
- Movement uses the `WASD` keys on the horizontal plane: `A` and `D` strafe, `W` and `S` walk forward and back, and the diagonals are normalised so walking sideways is not faster than walking straight.
- `Space` jumps, using a jump height and a gravity value that give a comfortable arc across the 1.0 unit gaps between platforms.
- Added a Character Controller to `Player` with Height `2`, Radius `0.5` and Center `(0, 0, 0)`, matching the capsule mesh. The script moves the player with it and reads from it whether the player is grounded, which is what allows jumping.
- The player also respawns at the start platform if it falls out of the level, so a missed jump never ends the run.

### 4. Camera ready
- Moved `Main Camera` to Position `(0, 2.5, -6.25)`, putting it 6.25 units behind the player and 2.5 units above the ground.
- Set its Rotation to `(9, 0, 0)`, a 9 degree downward tilt that frames the player and the platforms ahead of them in one shot.
- Scale stays `(1, 1, 1)`; only the position and rotation were changed.
- This is the resting position the follow camera starts from, so the level reads correctly before the player moves.

### 5. Steady cam
- Added `Assets/Scripts/CameraController.cs` and attached it to the `Main Camera` GameObject.
- Its `player` field is assigned to the `Player` GameObject, so the camera knows which object to track.
- The camera follows the player smoothly instead of snapping, damping towards the target position over roughly `0.15` seconds, which keeps the motion comfortable while jumping between platforms.
- Holding the right mouse button and dragging rotates the view: mouse X changes the yaw the player is followed from, mouse Y changes the pitch, which is clamped between `-40` and `40` degrees.
- The camera starts from the `(0, 2.5, -6.25)` position and 9 degree tilt authored in task 4 rather than snapping level on the first frame.
- The camera offset is applied around that pivot, so rotating the view orbits the camera around the player instead of only turning in place.

### 6. Falling up
- `PlayerController` watches its world Y against a `fallThreshold` of `-10`, and once the player drops below that they have fallen out of the level.
- Instead of ending the run, the player is dropped back in from the top of the screen: `Respawn()` teleports them to their start X and Z at `y = 10`, so they fall back down onto the start platform under `Platform01`.
- The respawn zeroes the movement velocity before teleporting and toggles the Character Controller off and on around the move, so the drop starts as a clean fall with no leftover momentum from the failed jump.
- An `isRespawning` flag makes the drop fire once per fall, however many frames the player spends below the threshold, and is only cleared once the player is back on solid ground.
- The camera keeps following through the whole thing: `CameraController` never resets its `smoothVelocity`, `pivotYaw` or `pivotPitch`, so it simply damps across to the new player position and holds the look direction the player had chosen.

### 7. Time trial
- Added a screen space canvas GameObject named `TimerCanvas`, holding the run clock label. Render Mode is `Screen Space - Overlay` and Pixel Perfect is `No`.
- Added a Canvas Scaler to `TimerCanvas` so the label stays legible at any window size: `UI Scale Mode` is `Scale With Screen Size` with a `Reference Resolution` of `1280x800`.
- `Screen Match Mode` is `Match Width Or Height` with `Match` set to `1` (Height), so the canvas scales to the window height and the label keeps its authored pixel height at any window width.
- Under `TimerCanvas` is a UI Text GameObject named `TimerText`, a `160x30` box anchored to the top centre of the screen and offset `(0, -40)` below the top edge.
- The label starts at `0:00.00`, Font Size `48`, white, `Middle Center` alignment, with Horizontal Overflow `Overflow` and Vertical Overflow `Overflow`, so the `m:ss.ff` reading is never wrapped or cut off by the `160x30` box.
- Raycast Target is `No`, so the overlay never swallows a click meant for the level.
- The font is the builtin `LegacyRuntime`, which replaces the old builtin `Arial.ttf` name removed in Unity 6.
- Saved the canvas as a prefab at `Assets/Prefabs/TimerCanvas.prefab`, and left the instance in the scene so the clock reads `0:00.00` from the moment the level loads.

### 8. Clock's ticking
- Added `Assets/Scripts/Timer.cs` to the `Player` GameObject and pointed its `timerText` field at the `TimerText` label from task 7, so the clock drives the same text that is already on screen.
- The component ships **disabled**, so the clock does not start counting while the player is still standing on the start platform. It is armed by leaving the start line instead.
- The clock counts up in hundredths and formats them as `m:ss.ff`, working in whole hundredths of a second so the seconds field can never round up to `60` and wrap onto the minute.
- The elapsed total is accumulated in place rather than read from a frame clock, so the time survives the falling up respawn from task 6.
- Added `Assets/Scripts/TimerTrigger.cs` on a cube GameObject named `TimerTrigger` at `(0, 1.25, 0)`, straddling the spawn point so the player starts inside it.
- The cube is flattened with scale `(0, 2, 0)` into a zero-thickness plane across the start line, and its Mesh Renderer is disabled so it is never drawn.
- Its Box Collider is set to `Is Trigger: Yes`. This is what makes `OnTriggerExit` fire at all: a solid collider there would simply block the player on the spawn platform, and the clock would never start.
- `OnTriggerExit` enables the player's `Timer` the first time the player walks out through the line, and a `hasStarted` flag means walking back over the start line can neither reset nor re-trigger the clock.

### 9. Finish line
- Added `Assets/Scripts/WinTrigger.cs` and attached it to the `WinFlag` GameObject, with its `player` field assigned to the `Player` GameObject.
- When the player enters the flag's trigger, the clock stops, the label grows from font size `48` to `60`, and the text turns green.
- Only the first entry into the flag counts. A `hasWon` flag stops later entries from restyling the label or stopping the clock again.

### 10. The sky's the limit
- Imported the Farland Skies Cloudy Crown skyboxes into `Assets/Materials/Skyboxes/`, keeping the original `.meta` files so the GUIDs and texture references survive.
- Set `CloudyCrown_01_Midday/CloudyCrown_Midday.mat` as the skybox in the Render Settings of `Level01`.
- The material uses the `Skybox/6 Sided` shader with six face textures, so the sky surrounds the level in every direction.
- The evening, midnight, sunset and daybreak variants are imported alongside it, ready to swap in.

### 11. The great outdoors
- Imported all 329 Kenney Nature Pack Extended models into `Assets/Models/`, flat with no subfolders.
- Moved the materials embedded in the models into `Assets/Materials/`, one `.mat` per unique name such as `grass.mat`, `dirt.mat` and `woodBark.mat`, 23 in total. Shared names like `grass` are one material used by every model that needs it.
- Renamed `wood.mat` to `Wood.mat` so the flag task can refer to it.
- Replaced each cube `Platform01` to `Platform10` with `cliff_block_rock.fbx`, a grass-topped rock block. The names and the `Platforms` parent are unchanged.
- Each model is scaled and moved so its bounds match the cube it replaces exactly. The platform tops keep their heights, so the jump path from task 1 still works.
- Every model mesh has a non-convex Mesh Collider, so the player stands on the real shape of the rock.

### 12. Environmental awareness
- Added an `Environment` root with `Trees`, `Rocks` and `Plants` groups, built by `Task12Builder`.
- Placed six trees, five rocks and ten flowers, grass and mushrooms on the platform tops, scaled so the trees are about three units tall.
- Trees and rocks sit on the edges of the wider platforms and the dead ends, and have non-convex Mesh Colliders so the player cannot walk through them.
- Flowers, grass and mushrooms have no colliders, so they never block a jump.
- Every item stays more than one unit away from the straight line between consecutive path platforms, and clear of the spawn and the flag, so the jump path is not blocked.

### 13. What's left of the flag
- Imported `Flag.fbx` into `Assets/Models/` and placed it as a child of `WinFlag`, built by `Task13Builder`.
- The cylinder placeholder is hidden by turning off its Mesh Renderer. Its Capsule Collider (trigger, radius 0.7, height 6) and `WinTrigger` are unchanged, so the finish still works.
- The pole stands on the top of `Platform08` at the flag position. The flag is scaled to a four unit pole and aligned by the base of its post, so the post sits on the axis of the trigger.
- The flag model has no colliders, so it never blocks the trigger.

### 14. (Sea)horse race
- Imported the flag texture `Flag.png` into `Assets/Textures/` as a repeating 2D texture.
- Created `Assets/Materials/Flag.mat` with the same built-in Standard shader as the Kenney materials, with the texture as its albedo and a white colour.
- The flag cloth in `Level01` uses `Flag.mat`, and the pole uses the existing `Wood.mat`. Both are overrides on the flag instance, so the scene file stores them.
- The flag model has no embedded materials, so nothing needed remapping.

### 15. Under development
- Built `Level01` three times with `Task15Builder`, into `Builds/Windows`, `Builds/Linux` and `Builds/Mac`, each named `Platformer`.
- The Windows and Linux builds are x86_64. The Mac build is universal, with x86_64 and ARM64 in one app.
- All three use the Mono scripting backend, since IL2CPP cannot be cross compiled from Windows.
- Folder layout:
  - `Builds/Windows/`: `Platformer.exe`, `UnityPlayer.dll`, `Platformer_Data/`, plus `D3D12/`, `MonoBleedingEdge/` and `UnityCrashHandler64.exe`.
  - `Builds/Linux/`: `Platformer.x86_64`, `Platformer_Data/`, plus `UnityPlayer.so` and the `libdecor` libraries.
  - `Builds/Mac/`: `Platformer.app`.
- Tested on Windows: the player starts in batch mode and the log shows no exceptions, missing script errors or null reference errors.
- The `*_BurstDebugInformation_DoNotShip/` and `*_BackUpThisFolder_ButDontShipItWithYourGame/` folders are git ignored, so they never reach the repo.

## Credits

- Models: [Kenney's Nature Pack Extended](https://kenney.nl/assets/nature-pack-extended)
- Skyboxes: [Farland Skies - Cloudy Crown](https://assetstore.unity.com/packages/2d/textures-materials/sky/farland-skies-cloudy-crown-60004)