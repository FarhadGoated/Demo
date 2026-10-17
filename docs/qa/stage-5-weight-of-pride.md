# Stage 5 manual QA — The Weight of Pride

Open `Assets/HellSlice/Scenes/WeightOfPride.unity` and press Play (no generator/setup). To test the
transition, play `ShadowOfTruth.unity` and enter its exit after returning the receipt. Spec:
`docs/modules/stage-5-weight-of-pride.md`. Standalone: `All-Builds\5_Stage5-demo_partner_not-merged\game`.

**Routes**
1. **Start state:** objective "Cross to the tower." Ahead-left a stair rises to a gold-trimmed high bridge leading to the lit tower doorway. Ahead-right the lower bank (with a cell) is visible across open lava, with **no stones** — it cannot be reached. Health 5 / 5, "Q / Slow time ready". Space jumps, Shift dodges.
2. **Upper route collapse:** walk up the stair onto the bridge. About 4 m before the break, every segment ahead drops into the lava in a cascade (rumble cue); rubble marks the broken edge; the HUD shows the memory line for ~8 s, then "The high way is gone. Take the lower path." Three stones rise out of the lava toward the lower bank.
3. **Safe return:** at the broken edge, run, jump and dodge forward — you stay on the stub. Walk back down the stair to the start bank. Expected: no death unless you deliberately step off the side.
4. **Lower route:** jump the three zig-zag stones to the lower bank. Gaps should feel easy at walking speed. Before the collapse, try to reach the lower bank anyway (run + jump + dodge) → lava, restart.

**Cooperation**
5. **Free the figure:** behind the bars a figure that looks exactly like you (same body, same cyan mark, same shade) waits in a cell. At the lever by the bars, E ("E / Open the cell"): the lever turns green, the cell gate vanishes, the figure walks out and stands on the plate. The plate turns green, a cyan link lights from the plate under the bars to the gap, and a bridge slides out across the gap (~1 s).
6. **Cross:** walk the bridge to the flame causeway.
7. **Open their way:** after the flames, at the lever by the iron door, E ("E / Open their way"): door vanishes, the figure leaves the plate and walks its gallery to a spot beside the tower gate. The bridge stays.
8. **Together:** when the figure has arrived **and** you stand on the exit landing, it walks into you, shrinks and vanishes. Then the forward gate opens, you lighten by one transformation step and the HUD shows "The one I helped was me." Objective "Enter the tower." Try to walk away during the merge — it still catches you.

**Softlocks / bypass attempts** (all should fail safely)
9. Before freeing the figure: try jumping (run + Space + Shift) across the gap to the causeway → lava, restart. Try to climb or jump the bars → blocked.
10. Try to reach the plate yourself → impossible (bars); the plate only responds to the figure.
11. On the exit landing, look for a way around or over the forward gate → none (tower walls). Waiting outside the landing after the figure arrives keeps the gate closed until you step on.
12. Walk back across the causeway and bridge after opening the door → nothing closes behind you; you can always return.

**Hazards**
13. **Flames:** four rows burn as a wave rolling toward you (far row first). When the nearest row goes out, all four are cool: walking straight through at once is safe (~0.7 s of slack). Hesitate and the far row catches you. Touching a burning row is lethal.
14. **With Q:** press Q while the nearest row burns, then cross when it goes out — comfortable with seconds to spare. Shift-dodge through a row also works for skilled play.

**Reset**
15. **Death reset:** after partial progress (bridge collapsed, figure on the plate or walking, a lever pulled), die in lava or flames. Stage 5 alone reloads: bridge intact, lower stones gone again, figure in the closed cell, levers grey, gap empty, doors and gate closed, health 5, Q ready. Earlier-stage reveal (arriving from Stage 4) is kept.

**Completion**
16. Walk through the open forward gate into the exit marker: "Stage 5 complete / The next chamber is not available yet." No scene load; re-entering adds nothing. From Stage 4, the exit now loads this stage.

Safe Inspector tuning: stone/bank/bridge Transforms; `SliceCollapsingRoute` (`Stagger`, `FallDuration`, `FallDistance`, trigger size — keep it ahead of the break and ≥4 m tall); `SliceHelperFigure` `Speed` and its path points (keep the last `ToExit` point inside the exit area); `SlicePrideStage` `RouteExtendSeconds`, `LowerPathRiseSeconds`, `Memory`, `MergeLine`, `MemorySeconds`; figure `MergeSpeed` (keep it above 6.5 so a dodging player is still caught); lever `InteractionRange`/`InteractionLabel`; flame row `phaseOffset`/`Damage` (lower Damage to make the crossing forgiving). Keep the lower-path stones, final route, switch link and broken edge saved **inactive**; cell gate, passage door and forward gate saved **active**; keep the gap ≥6.2 m; StageNumber 5, SlowTimeUnlockStage 3, useLegacyConstruction off.

## Assistant verification (17 October)

See `docs/PROJECT_STATE.md` → "Stage 5". Report the first failing step.
