# Mara's cage (A12): hanging cage, door and lever

Visual kit for the Stage 1 trapped figure. The cage hangs on its chain from a ruined column and beam, and its door lifts. The lever stands in the rescue cell on the existing rescue-lock pad.

| File | Contents |
| --- | --- |
| `CageKit.fbx` | `Cage_Body` (barrel cage and chain, pivot at the cage floor, 9.66 m tall at scale 1), `Cage_Door` (front bar panel cut from the body, same pivot), `Lever_Base`, and `Lever_Handle` (pivot at its hinge). |
| `Iron.mat` / `IronGlow.mat` | URP Lit dark iron. `IronGlow` has emission enabled so the lever can pulse. |

The behaviour lives in `Polish/Scripts/PolishCageRescue.cs`, on `Level/Environment/Mara's cage (added)` in `FirstPlayable.unity`. It is visual only and reads the session:
- The cage sways on its chain.
- The lever pulses until it is used.
- Releasing the rescue lock (`SwitchLatched`) pulls the lever and lifts the door.
- Helping Mara (`Assisted`) dims the cage light.

Mara's own fade stays with `SliceMemoryFigure`. No colliders were added; the old placeholder bars are hidden and their gameplay toggling is unchanged.

## Sources (CC0 1.0; downloaded 17 Oct 2026 from Poly Pizza)

| Asset | Page | Author |
| --- | --- | --- |
| Cage Medieval | https://poly.pizza/m/mybhRIyuL9 | hat_my_guy |
| Lever | https://poly.pizza/m/guR2QhKFLT | Quaternius |

Licence: CC0 1.0 Universal; no attribution required (courtesy: "Cage: hat_my_guy, lever: Quaternius (CC0)").

Modifications: converted from GLB to FBX in Blender 5.2. The door panel was cut out of the cage bars (a ±38° front sector), pivots were moved to the cage floor and the lever hinge, and transforms were baked in metres. The sources are in `LocalOnly/AssetSources/A12-CageLever/`.
