# HellStone surfaces (A10): shared environment textures

Tiling stone for the authored box geometry. The materials use the `HellSlice/Triplanar Stone` shader (`Polish/Shaders/TriplanarStone.shader`), which projects textures in world space. Any scaled cube in any stage therefore tiles at real-world size with no UV work, and colliders and layout stay untouched.

| Material | Top faces | Side faces | Use for |
| --- | --- | --- | --- |
| `HellStone-Floor.mat` | Volcanic Rock Tiles (1.8 m tile) | Dark Rock | Walkable ground: islands, banks, landings, stepping stones |
| `HellStone-Wall.mat` | Dark Rock | Rock Wall 08 (1.8 m) | Built walls, shoulders, arches, buttresses, cells |
| `HellStone-Rock.mat` | Dark Rock (4 m) | Dark Rock (4 m) | Distant spires, cliffs, backdrop rock |

To reuse in another stage, assign one of these materials to the existing MeshRenderer. Nothing else changes. Surfaces below world Y −0.1 glow faintly, as if lit by the lava (`Lava heat` settings on the material).

## Sources (all CC0 1.0, Poly Haven; downloaded 17 Oct 2026, 1K JPG)

| Folder | Poly Haven asset | Author |
| --- | --- | --- |
| `VolcanicRockTiles/` | https://polyhaven.com/a/volcanic_rock_tiles | Charlotte Baglioni |
| `RockWall/` | https://polyhaven.com/a/rock_wall_08 | Amal Kumar |
| `DarkRock/` | https://polyhaven.com/a/dark_rock | Amal Kumar |

Licence: CC0 1.0 Universal (https://polyhaven.com/license). No attribution is required; the courtesy credit is "Textures: Poly Haven (CC0)". Files are renamed (`_diff` → `_BaseColor`, `_nor_gl` → `_Normal`, `_arm` → `_ARM`); otherwise they are unmodified.
