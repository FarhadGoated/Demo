# Stage 4 manual QA — The Shadow of Truth

Open `Assets/HellSlice/Scenes/ShadowOfTruth.unity` and press Play (no generator/setup). To test the
transition, play `FeedingFlame.unity` and enter its exit after the fourth pillar. Spec:
`docs/modules/stage-4-shadow-truth.md`.

1. **Start state:** objective asks you to set each brazier on its paired mark (0 / 3). Receipt is absent, exit gate closed, no cyan route bars, no memory text. Q shows "Slow time ready"; Space jumps, Shift dodges.
2. **Before solving, the veil blocks the crossing; real and fake stones look identical.** From the start bank, Crossing 1 looks like a full two-column bridge. The flame veil at the edge stops every jump (run + Space). The handle, brazier and three marked plates are in front of you; the paired-bar plate is findable without jumping.
3. **Section 1:** E at the handle → brazier slides to plate 2 (triple mark): nothing revealed, no penalty. E again → paired plate: the veil vanishes, cyan paired bars and dark cast shadows appear on/under the three real stones only, memory 1 shows on the HUD and stays readable above the plates. E again does nothing (latched).
4. **Order:** Banks 2 and 3 are unreachable until the previous section is solved (veil). The handles also keep a prerequisite as a backstop ("Reveal the earlier memory first").
5. **Crossings:** jump the marked zig-zag stones of Crossings 1 and 2. Landings should be reliable at walking speed; each unmarked fake stone drops you into lava. Banks 2 and 3 are veiled until their sections are solved. Retrace backwards once to confirm reverse jumps.
6. **Sections 2 and 3:** two presses each. Section 3 shows memory 3 and the glowing receipt appears on the pedestal at Bank 3's left side.
7. **Evidence:** E picks up the receipt (slow carry, no dodge/attack). Jump Crossing 3's two straight marked stones while carrying. At Mara's case file press E: receipt placed, "It was me. Mara did nothing wrong." appears, gate disappears, one transformation step previews. Also try: E away from the desk drops it (it can be picked up again); dropping it over lava loses it and the objective says to restart via lava.
8. **Exit:** walk into the exit marker: "Stage 4 complete / The next chamber is not available yet." No scene load. Re-entering does not add another reveal.
9. **Death reset:** after partial progress (or while carrying), fall into lava. Stage 4 alone reloads: braziers back at start plates, veils back, markings/shadows/memories hidden, receipt hidden, gate closed, full health, Q ready. Earlier-stage reveal (if arriving from Stage 3) is kept.
10. **Transition and preservation:** the Stage 3 → 4 link is added when Stage 4 moves onto the polish branch; until then play ShadowOfTruth directly. Stages 1–3 play as before.

Safe Inspector tuning: stone/fake stone/bank Transforms (move markings with their stones), each `SliceTruthBrazier` (positions, `CorrectPosition`, range, `MoveDuration`, memory text), receipt `InteractionRange`, case file position, `MemorySeconds`. Keep markings, memory labels and the receipt saved **inactive** and veils saved **active**; keep StageNumber 4, SlowTimeUnlockStage 3, useLegacyConstruction off.

## Assistant verification (17 October)

See `docs/PROJECT_STATE.md` → "Stage 4". Report the first failing step.
