# Editing the three implemented levels

Stop Play Mode before editing. In Unity's Project window, open `Assets/HellSlice/Scenes` and double-click:

| Stage | Scene asset |
| --- | --- |
| 1 | FirstPlayable.unity |
| 2 | WeightOfPossession.unity |
| 3 | FeedingFlame.unity |
| 4 | ShadowOfTruth.unity |

Expand `Level` in the Hierarchy. Persistent environment, lighting, player, camera, puzzles, hazards, progression, UI and systems are saved scene objects; Stage 1/3 also contain enemies. No generator is required. The scene's `useLegacyConstruction` is off and object hide flags are zero.

Stage 3: corner pillars are under `Level/Puzzles`; the entity is under `Level/Enemies`; the forward gate and route are authored scene objects referenced by `Level/Systems/Gameplay Session` -> `SliceFeedingFlameStage`. That component exposes the four pillar references and fall timing. Entity Inspector exposes wind-up/recovery; projectile speed is on `Assets/HellSlice/Prefabs/FeedingFlame-Fireball.prefab`.

Stage 4: stones and silhouettes are under `Level/Environment/Crossing 1–3`; each brazier section (handle, brazier, marked positions, inactive markings and memory) is under `Level/Puzzles/Section 1–3` with `SliceTruthBrazier`; the receipt (saved inactive) and Mara's case file are under `Level/Puzzles`; `SliceShadowTruthStage` on Gameplay Session references sections, receipt and exit gate. See docs/modules/stage-4-shadow-truth.md.

Move/resize objects using their Transforms and save with Ctrl+S. Preserve component/reference links when replacing visuals. Changes made during Play Mode are temporary.

For all scenes visible together while editing, drag the other scene assets from Project into Hierarchy to open them additively. Each has its own scene heading. Before pressing Play, close the other scenes so exactly one gameplay scene is loaded: these levels overlap in world coordinates and each has its own player, camera and session. Edit/open one at a time for the simplest workflow.

Saved-file structure was checked; no Unity UI actions or scene opening were performed by the assistant in this phase.
