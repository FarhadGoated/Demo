# Stage 4 — The Shadow of Truth

Fictional coma nightmare about a lie that harmed a coworker, Mara. No claim of religious punishment.

## Spec

- **Layout** (`Assets/HellSlice/Scenes/ShadowOfTruth.unity`, along +Z): start bank → Crossing 1 (3 stones) → Bank 2 → Crossing 2 (3 stones) → Bank 3 → Crossing 3 (2 stones) → exit bank with Mara's case file, exit gate and completion trigger. Lethal lava below everything.
- **Platforms:** real stones are permanent solid 2 × 0.5 × 2 m basalt blocks with level tops (y = 0), 0.9 m gaps between rows and from each bank. Two columns (x = ±1.25 m). Each row has one real stone; the other column holds a **fake stone that looks identical** (same size, material, top height) but has no collider. Before solving, each crossing reads as a full two-column bridge, so straight and mirrored routes look plausible. Real stones never move, appear or change collision.
- **Veils (skip fix, 17 Oct QA):** each departure edge has a veil, a solid invisible 12 × 5 m barrier with a visible row of low ember flames. It blocks every jump onto that crossing until its section is solved, so no jump is committed before the information exists. Crossing 3 (the carry) is veiled until Section 3.
- **Brazier sections:** each departure bank has a fixed handle, a movable brazier and three anchor plates marked with one, two or three cyan bars. E at the handle moves the brazier to the next anchor (cycles). The **paired-bar** anchor latches the section: its veil disappears, paired cyan bars and dark cast shadows ("a lie casts no shadow", offset away from the brazier on the lava) appear for that crossing's real stones only, and a brief memory appears (HUD objective for 8 s + a world label that stays readable). Wrong positions cost nothing. Every solution needs two presses. Section 2 and 3 handles stay locked until the previous section is solved.
- **Memories:** 1 "I blamed Mara for the missing wages." · 2 "Mara lost her job. Her family went without." · 3 "I hid the signed receipt that proved I took them."
- **Evidence:** solving Section 3 exposes the signed receipt on Bank 3. Use the existing carry/drop/return rules (`SliceValuable`, CarrySpeed 2.5, grounded jumps while carrying) to take it across Crossing 3 to Mara's case file. Returning it acknowledges responsibility ("It was me. Mara did nothing wrong."), opens the exit gate and previews the next transformation step once. Dropping it in lava loses it until the current-stage restart (objective explains).
- **Completion:** entering the completion trigger after the return commits Stage 4 once. `NextScene` is empty (no Stage 5 yet): completion objective shows, movement continues.
- **Rules:** StageNumber 4, SlowTimeUnlockStage 3 (Q available, optional). Space jump, Shift dodge unchanged. No shadow projection/detection, no new ability. Death (lava/fall) reloads the whole scene: sections, markings, receipt, gate, carrying and ability timers reset; earlier completed stages survive.
- **Fairness:** brazier, handle and clue label sit on safe ground before each crossing; the objective names the goal; the veil means the crossing opens only once markings and shadows are showing, so guessing is impossible rather than discouraged. Carry jump reach ≈1.8 m and walk jump reach ≈3.0 m exceed the 0.9 m gaps (and the 0.98 m diagonal edge distance in zig-zags).

## Editing

- `Level/Environment`: banks and `Crossing 1–3` groups (`Stone N - solid`, `Fake stone N - no floor (looks real)`). Move stones freely; keep markings and cast shadows aligned (they are separate objects).
- `Level/Puzzles/Section 1–3`: `SliceTruthBrazier` with `Handle`, `Brazier` (starts at the anchor it is saved nearest to), `Positions[3]`, `CorrectPosition`, `InteractionRange`, `MoveDuration`, `Prerequisite`, `Veil` (saved active; child `Section N - veil`), `Markings` (saved inactive; bars + cast shadows), `MemoryLabel` (saved inactive), `Memory`. Anchor names say which mark they carry.
- `Level/Puzzles/Evidence - appears after Section 3/Signed receipt - evidence (saved inactive)`: `SliceValuable` (`Receptacle` = case file, `Placement`, `RestoredRoute` = acknowledgement label, `InteractionRange`). `Level/Puzzles/Mara's case file - return the receipt here`.
- `Level/Systems/Gameplay Session` → `SliceShadowTruthStage`: `Sections`, `Evidence`, `ExitGate`, `MemorySeconds`.
- Lava, exit gate, completion trigger and floor marker are the Stage 2 objects, moved/resized.

## Implementation

`SliceTruthBrazier` (per section) and `SliceShadowTruthStage` (stage controller) are new; `SliceSession` and `SliceInteraction` dispatch to the controller like Stages 2–3. Stage 3's `FeedingFlame.unity` changes only `NextScene: ShadowOfTruth`. Scene added to build settings and `SliceBuild.BuildWindows`. The scene was authored once by a temporary batch script (copy of WeightOfPossession, Stage 2 puzzle objects removed, primitives with existing materials added); the script was deleted and is not tooling.

Verification and manual QA: `docs/qa/stage-4-shadow-of-truth.md`.
