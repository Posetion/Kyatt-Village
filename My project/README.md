# Kyatt Village

Kyatt Village is a 3D action-survival game built in Unity. Defeat enemies across procedurally selected rooms, collect coins and upgrades, visit the shop, improve the player's permanent stats, and survive increasingly difficult waves.

## Features

- Third-person movement with keyboard, mouse, and mobile controls
- Automatic player attacks against nearby enemies
- NavMesh enemy movement, regular enemies, fireballs, tanks, and elite enemies
- Random room progression with scaling enemy counts
- Shop encounters every five completed rooms
- Temporary run upgrades and permanent player upgrades
- Coins, health, player speed, damage, defense, luck, attack speed, and regeneration stats
- Boss and elite encounters
- Desktop and WebGL support
- Persistent progress and best-run records saved with Unity `PlayerPrefs`

## Controls

### Keyboard and mouse

| Action | Control |
|---|---|
| Move | W, A, S, D or arrow keys |
| Look/rotate camera | Mouse |
| Sprint | Left Shift |
| Jump | Space |
| Interact with shopkeeper or chest | E |
| Pause/settings | Escape |
| Select UI buttons | Mouse |

### Touch devices

- Use the on-screen movement joystick to move.
- Use the on-screen look controls to rotate the camera where enabled.
- Use the jump and sprint buttons for movement actions.
- Tap the shopkeeper, chest, or UI controls to interact.

The player attacks automatically when an enemy enters attack range.

## Gameplay

1. Select **Play** from the title screen.
2. Optionally purchase permanent upgrades using saved upgrade points.
3. Enter rooms and defeat all spawned enemies.
4. Reach the exit arrow and continue to the next room.
5. Enemy difficulty and enemy counts increase as the run progresses.
6. Visit the shop every five rooms to purchase power-ups with collected coins.
7. Survive as long as possible and beat the saved best run time.

## Project information

- Unity Editor: **6000.0.68f1**
- Render pipeline: **Universal Render Pipeline**
- Target platform: **WebGL**
- Additional platforms: **Windows and mobile input support**
- Input system: **Unity Input System**
- Navigation: **Unity AI Navigation/NavMesh**

The enabled build scenes are:

1. Title
2. Room1
3. Room2
4. Room3
5. Room4
6. Room5
7. Room6
8. Room7
9. Room8
10. Room9
11. Room10
12. Shop

## Open the project

1. Install Unity Hub and Unity Editor **6000.0.68f1**.
2. Add WebGL Build Support through Unity Hub if it is not already installed.
3. Open Unity Hub.
4. Select **Open > Add project from disk**.
5. Select the folder containing this Unity project.
6. Open the `Title` scene.
7. Enter Play mode to test the game.

## Build for WebGL

1. Open **File > Build Profiles**.
2. Select **WebGL**.
3. Confirm the active build scenes in **Project Settings > Build Settings**.
4. Use the recommended Unity WebGL compression settings supported by the target hosting service.
5. Select **Build and Run** to create and open a local WebGL build.

For itch.io or another static host, upload the complete generated build folder, not only the HTML file.

## Important project folders

- `Assets/Scripts` - game systems, enemy AI, spawning, upgrades, shop, UI, and scene management
- `Assets/Scenes` - title, rooms, shop, bosses, and other test scenes
- `Assets/Starter Assets` - third-person controller and Starter Assets input support
- `ASSET_CREDITS.md` - third-party asset sources and license-audit information
- `ASSET_DEPENDENCY_TRACE.csv` - asset dependency inventory used for the credit audit

## Assets and credits

This project uses third-party art, audio, UI, effects, terrain materials, and character assets.

Before redistribution or commercial publication, review:

- [ASSET_CREDITS.md](./ASSET_CREDITS.md)
- The original creator or publisher license for every used asset
- Asset Store receipts or package licenses for Unity Asset Store content
- itch.io download/license pages for itch.io packages
- Required artist attribution inside the game credits

"Free" or "no-cost" assets may still have restrictions, especially for redistribution of original asset files or WebGL distribution.

## Development notes

- Player and permanent-upgrade progress is stored locally with `PlayerPrefs`.
- Enemy rooms use NavMesh and Unity AI Navigation.
- Room selection is managed by `SceneController`.
- Enemy spawn scaling and room bonuses are managed by `EnemySpawner`.
- The shop appears every five completed rooms in the default scene configuration.
- Mobile UI should be tested through a WebGL build on a touch device or browser mobile-emulation tools.

## License

Project source code and original project-created content are maintained by the project owner unless otherwise stated. Third-party assets remain subject to their individual licenses documented in `ASSET_CREDITS.md`.
