# Lava Giant (A07): Stage 3 fireball entity visual

A ready-to-use visual for the **Stage 3 (The Feeding Flame) stationary fireball entity**. It is **not wired into any scene** yet; the steps below are for the developer.

Credit (CC BY 4.0, required): "lava monster" by Satwik.Bandi, https://sketchfab.com/3d-models/lava-monster-2e2a1eac6f834857af5347a0662b075d. The full text is in `LICENSE-CC-BY-4.0.txt`. Add this credit to the README credits section when it ships.

## What's here

| File | What it is |
| --- | --- |
| `LavaGiant.fbx` | Mixamo-rigged giant: 46.7k triangles, 49 bones, one 264-frame breathing idle (imported as the looping clip `Idle`). Read/Write is enabled because the body flames emit from the skinned mesh. |
| `Textures/LavaGiant_*.png` | BaseColor, Normal and Emissive, downscaled from 4096 to 1024 px (the docs' character budget). |
| `LavaGiant-URP.mat` | URP Lit: dark rock base, normal map, and emissive lava veins (HDR orange ×4, so post-processing bloom makes them glow). |
| `LavaGiant-Idle.controller` | Loops `Idle`. |
| `LavaGiant-Visual.prefab` | **Use this one.** It contains `Model` (feet at the origin, centred, 2.3 m tall before parent scale, so **≈6.9 m** under the Stage 3 entity's 3× scale, taller than the Cyclops at 5.4 m), plus "Body flames", "Body flame cores", "Body embers" (particles emitted from the body surface; ≤230 live particles in total for the Intel Xe budget) and an unshadowed flickering "Heat light". |

`PolishImportRules.cs` lets this folder import its animation; every other character stays model-only. Keep that rule, or the idle clip disappears on reimport and the giant falls back to a T-pose.

## How to apply it (about 2 minutes, gameplay unchanged)

1. Open `Assets/HellSlice/Scenes/FeedingFlame.unity` and select **`Feeding Flame - stationary entity`**.
2. Drag `LavaGiant-Visual.prefab` onto it as a **child**, then set the child's local position to 0, rotation to 0 and scale to 1.
3. On the entity's `SliceEnemy`, set **Appearance** = the new `LavaGiant-Visual` child.
4. Deactivate the entity's child **`Visuals - replace artwork here`** (the placeholder primitives). Leave `Enemy strike warning` alone.
5. Optional: rotate the entity on Y to face the player's arrival (about 180° in the current layout). `SliceEnemy` turns it toward the player on every throw anyway.
6. Save the scene and play Stage 3.

What stays as it is: the CharacterController and collider, `FireOrigin`, wind-up and recovery timing, detection range, and the four pillars (each 8.5 m from the entity, clear of the giant's ≈2.4 m half-width). The pillar fall still targets the entity's collider.

Behaviour you get for free from the existing code:
- `SliceEnemy` uses the first renderer under Appearance (the `Model` body) for its state tint. The base colour turns dark red while hunting and bright red during the wind-up, and the lava veins keep glowing.
- On defeat, Appearance is deactivated, so the flames and light vanish with it.

## Notes for review

- **Docs deviation (flagged):** `docs/VISUAL_POLISH.md` says models are static poses and that animation is a separate decision. This model's bind pose is a T-pose, so the supplied idle loop is kept for it only. To revert, set `importAnimation` back to false in `PolishImportRules.cs` and expect a T-pose.
- `FireOrigin` is (0, 0.8, 0) local, about 2.4 m, so fireballs leave from the giant's belly. Raising it would move the throw to chest or hand height, but that changes the fireball paths into the pillars, so it needs playtesting and is left unchanged.
- **Provenance:** published by Satwik.Bandi on Sketchfab in 2019 with no description. It has a standard Mixamo rig, and no game-rip signs were found. The author doesn't state that it's their original work.
- **Theme:** this is a fictional dream manifestation of anger (rock and fire). It has no horns, wings or religious iconography, in line with `docs/GAME_DESIGN.md`.
- It was previewed in FeedingFlame with Unity 6000.3.13f1 (scale, glow and facing checked). The particles and the in-game tint have **not** been checked in play mode yet.
