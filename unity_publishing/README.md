# 0x02. Unity - Scripting

## Description
This project focuses on C# scripting and gameplay mechanics within Unity. Built on a maze environment, it implements player movement, camera tracking, collectible pickups with score tracking, hazards with health management, and win/game over conditions.

## Requirements
- All C# files compiled using dotnet / Unity C# compiler
- Projects built with Unity
- A `README.md` file at the root of the project folder
- All scene GameObjects, materials, and prefabs organized per task requirements

## Tasks

### 0. Ready player one
- Created a Sphere GameObject named `Player` with a `Rigidbody` component in `Assets/maze.unity`.
- Position: `(23, 1.2, 16)`, Scale: `(2, 2, 2)`.
- Material: `Assets/Materials/player.mat` with blue color (`#0000FFFF`).

### 1. Bust a move
- Created `Assets/Scripts/PlayerController.cs` and attached it to `Player`.
- Implemented player movement via WASD or arrow keys on the X and Z axes in `FixedUpdate()`.
- Added public float `speed` variable editable in the Inspector.

### 2. Camera ready
- Positioned `Main Camera` at `(22, 26, 7)`.
- Created `Assets/Scripts/CameraController.cs` and attached it to `Main Camera`.
- Added public `GameObject player` variable and implemented camera follow with constant offset along X and Z.

### 3. Insert coin
- Created Cylinder GameObject named `Coin` with position `(27, 1.7, 24)`, rotation `(0, 0, 90)`, and scale `(1, 0.05, 0.8)`.
- Tagged `Pickup`, collider set to `Is Trigger`.
- Material: `Assets/Materials/coin.mat` with yellow color (`#FFFF00FF`).
- Created prefab at `Assets/Prefabs/Coin.prefab`.

### 4. Coin collecting
- Created `Assets/Scripts/Rotator.cs` and attached to `Coin` prefab to rotate around X-axis by 45 degrees over time.
- Added score tracking (`score = 0`) to `PlayerController.cs` in `OnTriggerEnter()`.
- Logs `Score: <value>` and deactivates coin on contact.
- Placed coins under empty GameObject `Coins` in the scene at `Y = 1.7`.

### 5. Danger zone
- Created Plane GameObject named `Trap` at position `(9.5, 0.26, 27)`, scale `(0.5, 1, 0.5)`.
- Tagged `Trap`, MeshCollider set to `Convex` and `Is Trigger`.
- Material: `Assets/Materials/trap.mat` with red color (`#FF0000FF`).
- Created prefab at `Assets/Prefabs/Trap.prefab`.

### 6. You've activated my trap card
- Added `public int health = 5` to `PlayerController.cs`.
- Decrements health and logs `Health: <value>` on contact with `Trap`.
- Placed traps under empty GameObject `Traps` in the scene at `Y = 0.26`.

### 7. The finish line
- Created Plane GameObject named `Goal` at position `(-27, 0.26, 1.8)`, scale `(0.5, 1, 0.5)`.
- Tagged `Goal`, MeshCollider set to `Convex` and `Is Trigger`.
- Material: `Assets/Materials/goal.mat` with green color (`#00FF00FF`).

### 8. Goaaaaaaaaaaal
- Added goal detection in `PlayerController.cs` `OnTriggerEnter()`.
- Logs `You win!` to console when reaching `Goal`.

### 9. Game over
- Added `Update()` check in `PlayerController.cs` for `health == 0`.
- Logs `Game Over!` and reloads scene, resetting health and score.
