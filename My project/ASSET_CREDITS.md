# Asset Credits and License Audit (Draft)

This audit traces the transitive assets referenced by the 12 enabled build scenes in the Unity Editor. The full 502-path inventory is in [ASSET_DEPENDENCY_TRACE.csv](./ASSET_DEPENDENCY_TRACE.csv). It includes project scenes, scripts, packages, and art/audio dependencies; the path counts below are not counts of unique third-party products.

The trace was generated with `AssetDatabase.GetDependencies` for `Title`, `Room1`–`Room10`, and `Shop`. A source scan found no runtime `Resources.Load`, Addressables, AssetBundle, or network-request calls in the game scripts checked. This is a strong scene-reference trace, but compare it with the final WebGL build before submitting.

## User-confirmed third-party sources

The source links below resolve part of the original audit. The linked product license still controls each asset. Keep the Asset Store receipt, itch.io download page, and exact license copy required by Digit7 Jam with the private repository or submission evidence.

| Category | Asset/project folders | Source | Attribution and submission status |
|---|---|---|---|
| Enemy characters | `Assets/2D Stuff/Tiny RPG Character Asset Pack 02...`, enemy prefabs under `Assets/KMT` | [Tiny RPG Character Asset Pack 02 by Zerie](https://zerie.itch.io/tiny-rpg-character-asset-pack-02) | Commercial and modified-project use are allowed by the supplied itch.io page. Redistribution/re-upload as an asset file is prohibited; AI training and NFT use are prohibited. Credit is appreciated but not required. |
| Fireball VFX | `Assets/fireballs`, `Assets/VFXPACK_FIRE_WALLCOEUR` | [VFX URP Fire Package](https://assetstore.unity.com/packages/vfx/particles/fire-explosions/vfx-urp-fire-package-305098) | Source confirmed. Keep the Unity Asset Store invoice, package license, and package version. |
| Trees | `Assets/KMT/blackTree` | [2D Forest Free 128x128 by Last Tick team](https://last-tick.itch.io/2d-forest-free-128x128) | Commercial, client, personal, and modified-project use are allowed. The page says edited versions cannot be redistributed as files; the game may use them, but source/extractable files must not be published. Credit is optional. |
| Water | `Assets/KMT/Water/Simple Water Shader` | [Simple Water Shader URP](https://assetstore.unity.com/packages/p/simple-water-shader-urp-191449) | Confirm the package version and save the Unity Asset Store invoice. Keep the package license together with the repository evidence. |
| UI | `Assets/PixelUIFree`; separately sampled UI under `Assets/Pixel_HUD_UI_FreeKit` must be checked for actual build use | [Pixel UI Free GUI Kit Sampler](https://assetstore.unity.com/packages/2d/gui/pixel-ui-free-gui-kit-sampler-412094) | Product source identified. Preserve the Asset Store invoice and package license. `Assets/PixelUIFree` has local documentation, but the exact license terms still need to be saved as evidence. |
| Rock wall | `Assets/GolemKin/Snow/RockWall` | [Cartoon Rock Wall](https://assetstore.unity.com/packages/3d/environments/landscapes/cartoon-rock-wall-285945) | Product source identified; save the Unity Asset Store invoice and package/license. Verify that the Snow/RockWall assets are not mixed with another rock-wall pack. |
| Rocks | `Assets/KMT/Rocks Stylized` | [Free Pack Rocks Stylized](https://assetstore.unity.com/packages/3d/environments/free-pack-rocks-stylized-316250) | Product source identified; save the Unity Asset Store invoice and package license. |
| Power-up icons | `Assets/2D Stuff/Buffs`, `Assets/2D Stuff/Spells` | [Vas Basic Spells and Buffs by Pixerelia](https://pixerelia.itch.io/vas-basic-spells-and-buffs) | Commercial and modified use are allowed. Do not resell or redistribute any part of the icon pack as files, even if modified. Attribution is optional but appreciated. |
| Effects | `Assets/MyAssets/JMO Assets/Cartoon FX Remaster`, legacy effects under `Assets/Effects/Cartoon FX (legacy)` | [Cartoon FX Remaster Free](https://assetstore.unity.com/packages/vfx/particles/cartoon-fx-remaster-free-109565) | The local remaster readme identifies Jean Moreno and Unity Asset Store terms. Keep the package readme/license and invoice. The legacy Cartoon FX package also has its own local license notice and must be credited separately if it remains in the build. |
| Player character | `Assets/Character` | [GanzSe Free Modular Character Fantasy Low Poly Pack](https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/ganzse-free-modular-character-fantasy-low-poly-pack-321521) | Product source identified. Save the Unity Asset Store invoice and package README/license evidence. |
| Character controller | `Assets/Starter Assets` | [First/Third Person Character Controllers](https://assetstore.unity.com/packages/3d/characters/first-person-third-person-character-controllers-196526) | Product source identified as Unity Starter Assets. Keep package source/version and applicable Unity Starter Assets license. |
| Lanterns and lamps | `Assets/MyAssets/Laterns and candles` | [Modular Medieval Lanterns](https://assetstore.unity.com/packages/3d/environments/historic/modular-medieval-lanterns-85527) | Product source identified; save the Unity Asset Store invoice and package/license README. |
| Wooden houses | `Assets/Scenes`, references under `Acronym Studio - Wooden Houses Free` | [Wooden House Free - Low Poly](https://assetstore.unity.com/packages/p/wooden-house-free-low-poly-270889) | Product source identified; save the Unity Asset Store invoice and package/license. |
| Blue/fantasy house | Fantasy house prefabs under `Assets/NaaszArts/Fantasy House` | [Stylised Fantasy House](https://assetstore.unity.com/packages/3d/environments/fantasy/stylised-fantasy-house-257964) | Product source identified; save the Unity Asset Store invoice and package/license. Verify that `NaaszArts/Fantasy House` matches this product exactly. |
| Environment/nature | `Assets/Polytope Studio`, possibly low-poly environment assets and terrain decorations | [Low Poly Environment Nature Free - Lowpoly Medieval Fantasy Series](https://assetstore.unity.com/packages/3d/environments/low-poly-environment-nature-free-lowpoly-medieval-fantasy-series-187052) | Product source identified. Save the Unity Asset Store invoice and license. Confirm Polytope Studio contents in the build belong to this package rather than another Polytope Studio pack. |
| Survival/crafting arrow and props | `Assets/Pandazole_Ultimate_Pack`, likely crafting-arrow assets | [Pandazole Survival Crafting Low Poly Pack](https://assetstore.unity.com/packages/3d/props/pandazole-survival-crafting-low-poly-pack-208575) | Product source identified; save the Unity Asset Store invoice and package/license. |
| Nature sound effects | `Assets/Nature - Essentials` | [Nature Essentials](https://assetstore.unity.com/packages/audio/ambient/nature/nature-essentials-208227) | User asked to skip adding sound evidence in this pass. Product source is recorded; preserve the Asset Store invoice/license if the same audio is retained in the final build. |
| Fences and building parts | `Assets/KMT/Fences 2026`, `Assets/KMT/rockDesk`, related EmaceArt fence assets | [Roadside Tales - Free Modular Fence Narrative Pack](https://assetstore.unity.com/packages/3d/environments/landscapes/roadside-tales-free-modular-fence-narrative-pack-133601) | Product source identified; save the Unity Asset Store invoice and license. The local folder `Fences 2026` contains EAS03 EmaceArt documentation, so verify package ownership and version before crediting. |
| Ground textures | `Assets/KMT/Handpainted_Grass_and_Ground_Textures` | [Handpainted Grass & Ground Textures Free - Terrain Tool URP/HDRP](https://assetstore.unity.com/packages/2d/textures-materials/nature/handpainted-grass-ground-textures-free-terrain-tool-urp-hdrp-187634) | Product source identified. Save the Unity Asset Store invoice and package license. |
| Hearts | `Assets/KMT/AXIALE_hearts`, `Assets/KMT/heart.prefab` | [2D Heart Assets by Axiale](https://axiale.itch.io/2d-heart-assets) | Product source identified. Preserve the itch.io download/purchase page and displayed license. |
| Shopkeeper | `Assets/KMT/NPC- metal man.png` (referenced by `Assets/Scenes/Shop.unity`) | [Christian Asset Pack by Team Griffin](https://teamgriffin.itch.io/christian-asset-pack) | Product source identified. Preserve the itch.io download/purchase page and displayed license/credit requirements. |

### Pixabay music supplied by the user

Preserve the exact attribution text in the game credits. The following creator links were supplied and should be matched to the corresponding file in `Assets/Scenes/Music` before submission:

- Claude Houde — track ID `114582` (`ob-lix-heilir-sir-norse-viking-background-music`)
- Vlad Bakutov — track ID `248213` (`deuslower-dark-fantasy-ambient-dungeon-synth`)
- Noah Broah / Noah Hansen — track ID `473049`
- Lofiewme — track ID `355123`
- Nicholas Panek — track ID `358521` (`nickpanek-tower-defense-8-bit-chiptune-game-music`)
## Referenced asset folders

| Referenced folder / source candidate | Paths in trace | What the trace indicates | License evidence in the project |
|---|---:|---|---|
| `Assets/KMT` | 178 | Game prefabs and content, including `blackTree`, `GreenTree`, `Handpainted_Grass_and_Ground_Textures`, `Rocks Stylized`, `Fences 2026`, `BuffCards`, `Water`, `rockDesk`, `Environment`, `AXIALE_hearts`, a coin asset, and audio. | The ground, fence, rock, water, coin, tree, heart, and several other sources are recorded above. Remaining custom KMT content still requires identification/evidence where not already cleared. Audio is handled separately and is not part of this asset-credit pass. |
| `Assets/Starter Assets` | 37 | Starter Assets runtime code and player footstep/landing sounds. | Unity Starter Assets is identifiable from the folder. Keep the original package/source and license information with the submission. |
| `Assets/Effects/Cartoon FX (legacy)` | 35 | Cartoon FX particle effects. | `CartoonFX Free.txt` identifies Jean Moreno and the free selection, but does not state the full license terms. Keep the Asset Store/source record or license proof. |
| `Assets/Scenes` | 32 | Game scenes/prefabs, six paths under `Acronym Studio - Wooden Houses Free`, and two referenced music files. | Source identified as Wooden House Free - Low Poly; retain its Asset Store license and receipt. |
| `Assets/MyAssets/Laterns and candles` | 24 | Lantern/candle assets and dependencies. | Source identified as Modular Medieval Lanterns; retain the Asset Store license and receipt. |
| `Assets/Polytope Studio` | 24 | Lowpoly Environments and Lowpoly Village assets. | Source identified as Low Poly Environment Nature Free / Polytope Studio; retain the Asset Store license and receipt and verify the exact package contents. |
| `Assets/NaaszArts/Fantasy House` | 22 | Fantasy House assets. | Source identified as Stylised Fantasy House; retain the Asset Store license and receipt and confirm the imported package match. |
| `Assets/PixelUIFree` | 17 | Pixel UI sprites, scripts, and font assets. | `Documentation.txt` describes the kit but does not identify its license terms or creator. |
| `Assets/2D Stuff` | 9 | Buff icons, a spell asset, and two Tiny RPG Demon/Blood Monster paths. | The Adventurer and Samurai license files in this folder are not proof for these referenced assets; those two packs are not in the scene dependency trace. Obtain the used packs' own source/license records. |
| `Assets/Fantasy Skybox FREE` | 7 | Skybox assets. | The readme identifies Render Knight and a free edition, but does not provide the license terms. Keep the source/Asset Store license record. |
| `Assets/TextMesh Pro` | 5 | TextMesh Pro resources, shaders, and the Liberation Sans font asset. | `Fonts/LiberationSans - OFL.txt` contains the SIL Open Font License 1.1 notice. Retain it when redistributing the font. |
| `Assets/Pandazole_Ultimate_Pack` | 4 | Pandazole Survival Crafting Pack assets, including arrow/prop dependencies. | Source identified as Pandazole Survival Crafting Low Poly Pack; retain the Asset Store license and receipt. |
| `Assets/GolemKin` | 4 | Snow assets, including rock-wall content that may come from the separate Cartoon Rock Wall source. | Source evidence for rock-wall content must be checked separately from remaining GolemKin snow assets. |
| `Assets/Character` | 3 | Character model/material dependencies. | The GanzSe README describes a free character kit, but does not establish the license terms. Keep the original source/license record. |
| `Assets/Violet Theme Ui` | 2 | UI icon/slider assets. | The local PDF documentation does not state license terms. |
| `Assets/Nature - Essentials` | 1 | `Ambiance_Night_Loop_Stereo.wav`. | User identified Nature Essentials. Sound evidence is skipped, but retain its Asset Store license if this audio remains in the build. |
| Root-level terrain/tree assets | 24 | `New Terrain ...` and `AdjustedTreeMesh_...` assets referenced by scenes. | Their origin is not recorded by the asset paths. Confirm whether these were created by the team or derived from a third-party pack. |

`Assets/Scenes` and `Assets/KMT` are project organization folders, so their counts include game-specific scenes, prefabs, and scripts as well as imported content. Use the CSV for individual paths.

## License files reviewed

- [Animated Gold Coin license](<./Assets/KMT/Animated Gold Coin - Free/LICENSE.txt>) is for a referenced coin asset. It allows use in games/commercial projects and prohibits standalone resale or redistribution. The file does not clearly state attribution requirements.
- [Liberation Sans OFL notice](<./Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt>) applies to a referenced font asset and must be retained with redistributed font software.
- The supplied [Noto Serif OFL notice](<./Assets/Pixel_HUD_UI_FreeKit/Font/LICENSE_NotoSerif.txt>) permits bundling the font with software when the notice is included. However, `Pixel_HUD_UI_FreeKit` and its Noto fonts are **not referenced by the enabled scenes** in this trace, so they are not currently identified as game-build assets.
- The [Lux Art Studios vegetation license](<./Assets/Vegetation_Stylized_Pack_ByLuxArtStudios/LUX ART STUDIOS - ASSET LICENSE AGREEMENT.txt>) requires visible credit and restricts making source assets separately extractable. That folder is **not referenced by the enabled scenes** in this trace.
- The Adventurer and Samurai license files and the Spooky Graveyard readme are present in the project, but those packs are not referenced by the enabled scenes in this trace.

## Before submitting

For every used pack marked as missing evidence, save a copy/screenshot of the actual source license, receipt, or publisher page and record the creator and required credit. "Free" describes cost or product tier; it does not by itself establish the rights to redistribute a WebGL build. In particular, verify music and sound licenses individually. Re-run the trace after any scene, prefab, or build-content changes.
