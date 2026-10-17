# Stage 6 — The Return

Integrated 17 October from `origin/stage-7-the-return` at `1afd8ea`, renumbered as final Stage 6. Supersedes the earlier planning-only note. Saved scene: `Assets/HellSlice/Scenes/TheReturn.unity` (StageNumber 6, SlowTimeUnlockStage 3, `useLegacyConstruction` 0, `NextScene` empty). Stage 5 (`WeightOfPride`) exits directly here; BorrowedTime remains archived. Fictional coma dream; the guardian is a dream manifestation, not a sacred or demonic being. Combat resolves the nightmare threat; the waking action completes the moral arc. No forgiveness or Paradise is promised.

## Finale

- **Monster:** the same `Manifestation` prefab + `Cyclops-Visual` (scale 1.5) as Stage 1, same pursuit speed (3.05 m/s), grace (1.5 s) and melee strike (1.25 s wind-up, red ring). It waits until the player steps into the arena. `SliceEnemy.ChargeEncounter` switches on the Stage 6 mode; Stage 1 (`CoreChase`) and Stage 3 (fireball/pillars) paths are unchanged.
- **Charge:** at 3–10 m with line of sight it stops, shows the ring plus a flat ember lane and turns to follow the player for 0.75 s, then locks for the last 0.45 s (`ChargeWindup` 1.2, `ChargeLock` 0.45) and charges straight at 12 m/s for up to 14 m (the lane strip shows the full length). It stops on a seal, the player (2 damage), a wall or the arena edge; a player who is dodging when it reaches them is passed through, so a late dodge still lets it hit the seal behind. Then it then recovers (1.2 s; 2 s after a seal). 1.5 s cooldown between charges. All on world time, so Q slows it to a quarter.
- **Arena:** 18 × 18 m floor over lava, entered from a south causeway. The monster is clamped to ±8.2 m and never steps off solid floor, so it cannot be lost in the lava and can touch every seal.
- **Seals (any order), flush with the arena edge so nothing hides behind them:** Seal 1 fills the south-west corner between two lava edges; Seal 2 on the east edge sits inside a timed flame jet that covers every spot within reach of its mechanism (activate during the cool window or with Q); Seal 3 on the north edge between two long flame jets with alternating phases (0 / 3.5 s), so one side is always cool and its mechanism is reached up the lane. A charge touching a seal breaks it (rubble appears) and exposes the mechanism. **E** at an exposed mechanism activates it once: the monster is stunned (4 s) and visibly smaller. Player attacks never damage it. The third activation dissolves it (2 s sink/shrink); the arena jets switch off and the final passage gate opens.

## Ending

Quiet 22 m walled walk (no hazards) to a warm lit doorway. The transformation area halfway applies the final step (`SetReveal(1)`: fully visible, body renderers keep casting shadows); it is also forced before the fade. At the light threshold (once grounded and not dodging) the player stands still and the screen fades to white; the player wakes in a small hospital room (far from the dream, enclosed) with the text "You wake in a hospital bed. Your phone is on the table beside you." Slow time is switched off once awake. **E** at the bedside table, "Call Mara back", shows the line and is the **only** `CompleteStage()` call (records Stage 6 once); after 6 s a closing card fades in: "He cannot undo that night. He can choose what he does next." Every step runs once; the phase only moves forward.

**Pending lead approval:** the waking action uses GAME_DESIGN's proposed "answer the ignored call" plus restitution of the wages (Stage 4's in-game names). The line does not say Mara called in Stage 1. Prompt, lines and props are Inspector fields / scene objects and can be swapped for "return a borrowed possession".

## Reset

Death (lava, damage, falling) or Esc → Respawn reloads the scene: monster, seals, mechanisms, jets, gate, walk, fade, health and Q timers reset; earlier completions survive. No checkpoints.

## Editing

- `Level/Environment`: causeway, arena floor and edge glow, `Quiet walk to the light` (path, parapets, invisible barriers, inlays, doorway), `Hospital room - after the fade` (move the group to move the room; spawn, waking point and ceiling light inside).
- `Level/Puzzles/Seal 1–3`: `SliceSealedMechanism` (`DisplayName`, `Seal` collider, `BrokenSeal` saved inactive, `Core`, `InteractionRange`).
- `Level/Hazards`: lava and three `SliceHazard` jets (own `Return-Jet1–3` materials).
- `Level/Enemies`: the guardian prefab instance; charge tuning on its `SliceEnemy`; `Charge lane - telegraph` saved inactive.
- `Level/Progression`: final gate, transformation area, light threshold (`SliceWorld.ExitVolume`).
- `Gameplay Session` → `SliceReturnStage`: monster, mechanisms, `StunSeconds`, arena hazards, gate, areas, hospital spawn, waking action/range/prompt, texts, fade timings/colour.

QA: `docs/qa/stage-6-the-return.md`.
