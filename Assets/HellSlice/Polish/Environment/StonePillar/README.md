# Stone Pillar (A09): Stage 3 pillar visual

A ready-to-use visual for the **four Stage 3 (The Feeding Flame) pillars** that fireballs knock onto the Lava Giant. It is **not wired into any scene** yet; the steps below are for the developer.

Credit (CC BY 4.0, required): "Stylized Stone Pillar" by Ngakau Blake (Punch-Kat), https://sketchfab.com/3d-models/bf6337f03be3464d8f5c113242306722. The full text is in `LICENSE-CC-BY-4.0.txt`. Add this credit to the README credits section when it ships.

## What's here

| File | What it is |
| --- | --- |
| `StonePillar.fbx` | Carved dark rock pillar with eye sockets: 1,022 triangles, one material, no rig or animation. |
| `Textures/StonePillar_*.png` | BaseColor, Normal, Emissive (the glowing eyes) and Occlusion, all 1024 px. |
| `StonePillar-URP.mat` | URP Lit: rock base, normal and occlusion maps, emissive eyes (HDR orange ×3, so bloom makes them glow). |
| `StonePillar-Visual.prefab` | **Use this one.** Its `Model` child is scaled to **4.2 m tall** (the Stage 3 pillar height), with the base at the prefab origin and centred. Footprint ≈ 1.64 × 1.24 m, close to the 1.4 × 1.4 m collider. |

## How to apply it (all four pillars, gameplay unchanged)

The pillars in `FeedingFlame.unity` are Unity cubes named `Pillar - <Southwest|Southeast|Northwest|Northeast> - one fireball hit`, scaled to (1.4, 4.2, 1.4), with a BoxCollider and a MeshRenderer. For each one:

1. Drag `StonePillar-Visual.prefab` onto the pillar as a **child**.
2. Set the child's transform to: **Position (0, -0.5, 0)**, **Rotation (0, 0, 0)**, **Scale (0.714, 0.238, 0.714)**. The scale is 1/1.4, 1/4.2, 1/1.4, which cancels the cube's non-uniform scale so the rock isn't stretched. The base sits on the floor.
3. Turn off only the pillar's **MeshRenderer**. Keep the **BoxCollider** and the GameObject active.
4. Optional: give each pillar a different Y rotation on the child (for example 0, 90, 180, 270) so the four don't look identical.
5. Save and play Stage 3.

This keeps the gameplay the same:
- The fireballs still hit the BoxCollider.
- `SliceFeedingFlameStage` still tips and slides the pillar transform, so the rock child falls with it.
- The pillar is still deactivated on impact, which hides the rock too.

## Notes for review

- **Provenance:** the author describes it as a rock prop "made in the art-style of Borderlands", so it's an original model in that style, not a game rip.
- **Theme:** carved eyes, with no religious symbols or text, fitting a fictional dream manifestation (`docs/GAME_DESIGN.md`).
- **Performance:** 4 × 1,022 triangles, with no lights or particles added.
- The prefab was built with Unity 6000.3.13f1 (import, material and prefab checked). It has **not** been previewed in Stage 3 or play-tested.
