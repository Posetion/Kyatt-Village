# Asset Credits and License Audit (Draft)

This audit traces the transitive assets referenced by the 12 enabled build scenes in the Unity Editor. The full 502-path inventory is in [ASSET_DEPENDENCY_TRACE.csv](./ASSET_DEPENDENCY_TRACE.csv). It includes project scenes, scripts, packages, and art/audio dependencies; the path counts below are not counts of unique third-party products.

The trace was generated with `AssetDatabase.GetDependencies` for `Title`, `Room1`–`Room10`, and `Shop`. A source scan found no runtime `Resources.Load`, Addressables, AssetBundle, or network-request calls in the game scripts checked. This is a strong scene-reference trace, but compare it with the final WebGL build before submitting.

## Referenced asset folders

| Referenced folder / source candidate | Paths in trace | What the trace indicates | License evidence in the project |
|---|---:|---|---|
| `Assets/KMT` | 178 | Game prefabs and content, including `blackTree`, `GreenTree`, `Handpainted_Grass_and_Ground_Textures`, `Rocks Stylized`, `Fences 2026`, `BuffCards`, `Water`, `rockDesk`, `Environment`, `AXIALE_hearts`, a coin asset, and audio. | The coin pack has a local license; the other KMT subfolders do not have enough source/license evidence to clear them individually. Identify each original pack/creator. |
| `Assets/Starter Assets` | 37 | Starter Assets runtime code and player footstep/landing sounds. | Unity Starter Assets is identifiable from the folder. Keep the original package/source and license information with the submission. |
| `Assets/Effects/Cartoon FX (legacy)` | 35 | Cartoon FX particle effects. | `CartoonFX Free.txt` identifies Jean Moreno and the free selection, but does not state the full license terms. Keep the Asset Store/source record or license proof. |
| `Assets/Scenes` | 32 | Game scenes/prefabs, six paths under `Acronym Studio - Wooden Houses Free`, and two referenced music files. | No license proof for the wooden-house pack or music was found in the project. |
| `Assets/MyAssets/Laterns and candles` | 24 | Lantern/candle assets and dependencies. | Creator/source and license proof not found in the inspected project files. |
| `Assets/Polytope Studio` | 24 | Lowpoly Environments and Lowpoly Village assets. | Creator is indicated by the folder name; license proof was not found. |
| `Assets/NaaszArts/Fantasy House` | 22 | Fantasy House assets. | License proof was not found. |
| `Assets/PixelUIFree` | 17 | Pixel UI sprites, scripts, and font assets. | `Documentation.txt` describes the kit but does not identify its license terms or creator. |
| `Assets/2D Stuff` | 9 | Buff icons, a spell asset, and two Tiny RPG Demon/Blood Monster paths. | The Adventurer and Samurai license files in this folder are not proof for these referenced assets; those two packs are not in the scene dependency trace. Obtain the used packs' own source/license records. |
| `Assets/Fantasy Skybox FREE` | 7 | Skybox assets. | The readme identifies Render Knight and a free edition, but does not provide the license terms. Keep the source/Asset Store license record. |
| `Assets/TextMesh Pro` | 5 | TextMesh Pro resources, shaders, and the Liberation Sans font asset. | `Fonts/LiberationSans - OFL.txt` contains the SIL Open Font License 1.1 notice. Retain it when redistributing the font. |
| `Assets/Pandazole_Ultimate_Pack` | 4 | Pandazole Survival Crafting Pack assets. | License proof was not found. |
| `Assets/GolemKin` | 4 | Snow assets. | License proof was not found. |
| `Assets/VFXPACK_FIRE_WALLCOEUR` | 3 | Fire VFX materials/textures. | License proof was not found. |
| `Assets/Character` | 3 | Character model/material dependencies. | The GanzSe README describes a free character kit, but does not establish the license terms. Keep the original source/license record. |
| `Assets/Violet Theme Ui` | 2 | UI icon/slider assets. | The local PDF documentation does not state license terms. |
| `Assets/Nature - Essentials` | 1 | `Ambiance_Night_Loop_Stereo.wav`. | Creator/source and license proof were not found in the project. |
| Root-level terrain/tree assets | 24 | `New Terrain ...` and `AdjustedTreeMesh_...` assets referenced by scenes. | Their origin is not recorded by the asset paths. Confirm whether these were created by the team or derived from a third-party pack. |

`Assets/Scenes` and `Assets/KMT` are project organization folders, so their counts include game-specific scenes, prefabs, and scripts as well as imported content. Use the CSV for individual paths.

## Audio files referenced by the scenes

The names below are taken from the referenced files. A filename or uploader-like label is not license proof; confirm the original download page, creator, license, and any required attribution for each before submission.

| File | Credit/license status |
|---|---|
| `Assets/KMT/cartoon_music-bomb-explosion-large-517330.mp3` | Source/license proof not found |
| `Assets/KMT/freesound_community-monster-attack-47786.mp3` | Source/license proof not found |
| `Assets/KMT/freesound_gamestudio-drop-coin-384921.mp3` | Source/license proof not found |
| `Assets/KMT/lesiakower-8-bit-game-over-sound-effect-331435.mp3` | Source/license proof not found |
| `Assets/KMT/Musics/ashfallMusic - Copy.mp3` | Source/license proof not found |
| `Assets/KMT/phatphrogstudio-male-fighter-voice-heavy-attack-grunt-544355.mp3` | Source/license proof not found |
| `Assets/KMT/universfield-combat-impact-352458.mp3` | Source/license proof not found |
| `Assets/KMT/universfield-punch-04-383965.mp3` | Source/license proof not found |
| `Assets/Scenes/Music/deuslower-dark-fantasy-ambient-dungeon-synth-248213.mp3` | Source/license proof not found |
| `Assets/Scenes/Music/lofiewme-pixel-fantasia-355123.mp3` | Source/license proof not found |
| `Assets/Nature - Essentials/Ambiance_Night_Loop_Stereo.wav` | Source/license proof not found |
| `Assets/Starter Assets/Runtime/ThirdPersonController/Character/Sfx/Player_Footstep_01.wav`–`Player_Footstep_10.wav` and `Player_Land.wav` | Unity Starter Assets source is identifiable; retain its applicable source/license record. |

## License files reviewed

- [Animated Gold Coin license](<./Assets/KMT/Animated Gold Coin - Free/LICENSE.txt>) is for a referenced coin asset. It allows use in games/commercial projects and prohibits standalone resale or redistribution. The file does not clearly state attribution requirements.
- [Liberation Sans OFL notice](<./Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt>) applies to a referenced font asset and must be retained with redistributed font software.
- The supplied [Noto Serif OFL notice](<./Assets/Pixel_HUD_UI_FreeKit/Font/LICENSE_NotoSerif.txt>) permits bundling the font with software when the notice is included. However, `Pixel_HUD_UI_FreeKit` and its Noto fonts are **not referenced by the enabled scenes** in this trace, so they are not currently identified as game-build assets.
- The [Lux Art Studios vegetation license](<./Assets/Vegetation_Stylized_Pack_ByLuxArtStudios/LUX ART STUDIOS - ASSET LICENSE AGREEMENT.txt>) requires visible credit and restricts making source assets separately extractable. That folder is **not referenced by the enabled scenes** in this trace.
- The Adventurer and Samurai license files and the Spooky Graveyard readme are present in the project, but those packs are not referenced by the enabled scenes in this trace.

## Before submitting

For every used pack marked as missing evidence, save a copy/screenshot of the actual source license, receipt, or publisher page and record the creator and required credit. "Free" describes cost or product tier; it does not by itself establish the rights to redistribute a WebGL build. In particular, verify music and sound licenses individually. Re-run the trace after any scene, prefab, or build-content changes.
