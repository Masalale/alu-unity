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

## Credits

- Models: [Kenney's Nature Pack Extended](https://kenney.nl/assets/nature-pack-extended)
- Skyboxes: [Farland Skies - Cloudy Crown](https://assetstore.unity.com/packages/2d/textures-materials/sky/farland-skies-cloudy-crown-60004)