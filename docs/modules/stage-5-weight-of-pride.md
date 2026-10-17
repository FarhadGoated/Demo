# Stage 5 — The Weight of Pride

Fictional coma nightmare about refusing help. The upper route fails on its own; the lower route only works through cooperation. No claim of religious punishment.

## Spec

- **Layout** (`Assets/HellSlice/Scenes/WeightOfPride.unity`, along +Z, lethal lava below everything): start bank → two routes. **Upper route** (x = −5): stair up to a high bridge leading straight to the lit upper doorway of a ruined tower. **Lower route** (x = 4–11): three platform stones → lower bank with the figure's cell → lava gap → flame causeway → exit landing → tower doorway with the forward gate → completion trigger. The figure's side (x = 11–14: cell, hold switch, gallery, passage door) is behind iron bars the player cannot pass.
- **Collapse:** walking onto the bridge stub enters a trigger volume 4 m before the break; every bridge segment ahead loses collision at once and falls into the lava in a cascade, and a solid broken edge (rubble + invisible barrier) appears so no one walks or jumps off it. The stair and stub stay: a safe way back. One memory line shows on the HUD for 8 s. The lower route is open from the start; the collapse is never required.
- **Cooperation:** E at the cell lever (player side) opens the cell gate. The figure walks fixed points to the hold switch and stays on it; the plate lights, a lit link runs to the gap and the final-route bridge extends across the 6.5 m lava gap. The player cannot reach or operate the switch. Beyond the flames, E at the passage lever (only while the figure holds) opens the figure's passage door; the figure walks its gallery to a fixed spot beside the forward gate. When **both** are inside the exit area, the forward gate opens and the next transformation step previews once; the completion trigger behind the gate commits Stage 5 once. The bridge stays once extended, so retracing never traps the player.
- **Flames:** four contiguous 2 m flame rows (`SliceHazard`, 7 s cycle, offsets 0 / 0.8 / 1.6 / 2.4 s) burn as a wave rolling toward the player; touching a burning row is lethal (Damage 5). When the nearest row goes out, all four are cool and a walking crossing has ≈0.7 s of slack — possible without Q; with Q (slow time) the slack is several seconds. Safe ground before and after.
- **Figure:** simple scripted mover between fixed points (no colliders, no AI, never blocks or gets stuck), speed 4 m/s on world time (Q slows it like other world objects).
- **Rules:** StageNumber 5, SlowTimeUnlockStage 3 (Q available). Space jump, Shift dodge unchanged. No projection, new ability or combat. Death (lava, fall, flames) reloads the scene: figure, collapse, levers, bridge, doors, gate and timers reset; earlier completed stages survive. NextScene empty (no Stage 6 yet).
- **No bypass:** collapse trigger cannot be jumped past (5 m tall, polled every frame); the gap is wider than run + jump + dodge reach (≈5.3 m); bars are continuous 3 m colliders; the tower facade has no way around the gate; completion code also requires the gate to have opened, which requires the figure to have arrived and the player in the exit area.
- **Memory (story proposal, pending approval):** "I thought asking for help made me small." — one Inspector field; clear it to show nothing.

## Editing

- `Level/Environment`: banks, `Upper route` (stair, stub, segments, tower ledge), `Lower path - platform stones`, causeway, landing, tower facade.
- `Level/Puzzles`: `Upper route collapse` (`SliceCollapsingRoute`: `Trigger`, `Segments` nearest first, `BrokenEdge` saved inactive, `Stagger`, `FallDuration`, `FallDistance`); `Figure's side` (cell, bars, `Cell lever`, `Hold switch`, `Passage door`, `Passage lever`); `Trapped figure` (`SliceHelperFigure`: `ToSwitch`, `ToExit` points under `Figure path`, `Speed`); `Final route` (saved inactive) and `Switch link` (saved inactive).
- `Level/Hazards`: lava and `Flame row 1–4` (`phaseOffset`, `Damage`). Each row has its own warning material (`Pride-FlameRow1–4`), because `SliceHazard` recolours its material.
- `Level/Progression`: `Exit area - both must stand here`, forward gate, completion trigger, floor marker.
- `Level/Systems/Gameplay Session` → `SlicePrideStage`: `UpperRoute`, `Figure`, `CellLever`, `SwitchPlate`, `FinalRoute`, `SwitchLink`, `RouteExtendSeconds`, `PassageLever`, `ExitArea`, `ForwardGate`, `Memory`, `MemorySeconds`.

## Implementation

New `SlicePrideStage` (controller), `SliceHelperFigure`, `SliceCollapsingRoute`; levers reuse `SlicePuzzleSwitch`, flames reuse `SliceHazard`, lava/gate/trigger reuse the Stage 4 objects. `SliceSession`/`SliceInteraction` dispatch to the controller like Stages 2–4. Stage 4 changes only `NextScene: WeightOfPride`. Scene added to build settings and `SliceBuild.BuildWindows`. Authored once by a temporary batch script from a copy of ShadowOfTruth (script deleted, not tooling).

QA: `docs/qa/stage-5-weight-of-pride.md`.
