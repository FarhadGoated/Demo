# Stage 1 — guardian chase

Saved scene: `Assets/HellSlice/Scenes/FirstPlayable.unity`. The recurring guardian is a fictional coma-nightmare manifestation. The protagonist cannot confront it yet; escape establishes its planned return in Stage 7.

Opening revision: player starts at (-3.8, 0, -9), facing forward toward the existing core at z=-5; guardian starts behind at z=-13. Normal third-person movement remains manual: no auto-run or lanes. Existing ground, core, socket, gate and carry/place behavior are preserved. Carry the core about 18m to the socket and press E once to open the passage. Former demonstration geometry remains editable.

`SliceChaseStage` starts pursuit on the first running frame with 1.5s grace and a 4m initial gap. Invulnerable guardian pursues at 3.05m/s; catching the player immediately reloads the stage, bypassing dodge/damage immunity. Stage 1 uses contact capture rather than melee wind-up; other stages retain combat. Putting the core back does not restart grace. Space jumps, Shift retains existing unladen dodge, and Q/time ability remains locked. Fairness is pending manual QA.

After placement, crossing the existing threshold arms the wall only when the player's entire capsule clears its far face by 0.15m. Collision seals immediately to prevent retreat/crushing during the visual drop; guardian stops, and the slab drops over 0.6s. Completion then continues to WeightOfPossession once, even if the player keeps moving. Editable wall objects are under Progression; references/timing are on Gameplay Session. The boundary follows the authored +Z passage. Death/Respawn restores wall collider disabled/visual raised, guardian, core, gate, grace and triggers; earlier-stage progress survives.

This revision received targeted code/scene-reference inspection only; no compile, Unity launch or gameplay tests. QA: `docs/qa/stage-1-chase.md`.
