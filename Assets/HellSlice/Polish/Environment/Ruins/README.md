# Ruins kit (A11): broken masonry, columns, arches, rubble and rocks

Static mesh pieces for dressing the authored box geometry as ruins. Every piece uses the HellStone materials (`../Surfaces`), so ruins, floors and walls share one world-space stone look.

| File | Contents |
| --- | --- |
| `RuinsKit.fbx` | 27 pieces from Quaternius' "Modular Ruins Pack", including Wall, Wall_Broken, Wall_Double_Broken, Wall_ArchRound(_Broken), Arch_Round, Arch_Gothic, Column_Square/Round/Round_Short, Support_Tall, Bricks, Brick, Skull, Pot*_Broken, Floor_*, Stairs, BridgeSection and Curve_1. The grid is 2 m (wall 2 × 2 × 0.29 m, arch about 3.1 × 3.5 m, column 4 m). |
| `RuinRocks.fbx` | Rock_1 to Rock_5 from Quaternius' "Rocks". Rock_4 is the tall jagged spire. |
| `HellBone.mat` | URP Lit bone colour for skulls. |

The pieces import as raw meshes: no materials, clips or colliders (`PolishImportRules`). Gameplay keeps the original box colliders, so dressing a stage never changes collisions.

`Polish/Prefabs/EndlessHellBackdrop.prefab` uses these pieces: a 2 km lava sea plus three rings of peaks and sunken arches (60 m to 520 m out). Drop it in a stage centred on its walkway, about 0.03 m below the gameplay lava top, so the lava never visibly ends. Set the cameras' far clip to about 1500.

## Sources (CC0 1.0; downloaded 17 Oct 2026 from Poly Pizza)

| Asset | Page | Author |
| --- | --- | --- |
| Modular Ruins Pack | https://poly.pizza/m/F2LAK03B0r (also https://quaternius.com/packs/ultimatemodularruins.html) | Quaternius |
| Rocks | https://poly.pizza/m/gYhoEOKItJ | Quaternius |

Licence: CC0 1.0 Universal; no attribution required (courtesy credit "Ruins and rocks: Quaternius (CC0)").

Modifications: the GLBs were converted to FBX in Blender 5.2. Each piece was moved to the origin on its own base, transforms were baked into the mesh in metres, and only the pieces listed above were kept. The source GLBs are in `LocalOnly/AssetSources/A11-QuaterniusRuins/`.
