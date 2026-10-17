# Stage 4 manual QA — The Shadow of Truth

Open `Assets/HellSlice/Scenes/ShadowOfTruth.unity` and press Play (no generator/setup). To test the
transition, play `FeedingFlame.unity` and enter its exit after the fourth pillar. Spec:
`docs/modules/stage-4-shadow-truth.md`.

1. **Start state:** objective asks you to set each brazier on its paired mark (0 / 3). Receipt is absent, exit gate closed, no cyan route bars, no memory text. Q shows "Slow time ready"; Space jumps, Shift dodges.
2. **Readability before jumping:** from the start bank, Crossing 1 shows grey stones and dark flat silhouettes. The handle, brazier and three marked plates are in front of you; the paired-bar plate is findable without jumping.
3. **Section 1:** E at the handle → brazier slides to plate 2 (triple mark): nothing revealed, no penalty. E again → paired plate: cyan paired bars appear on the three real stones, memory 1 shows on the HUD and stays readable above the plates. E again does nothing (latched).
4. **Order:** before solving Section 1, cross by the real stones and try the Section 2 handle: it says "Reveal the earlier memory first" and does not move.
5. **Crossings:** jump the marked zig-zag stones of Crossings 1 and 2. Landings should be reliable at walking speed; each silhouette drops you into lava. Retrace backwards once to confirm reverse jumps.
6. **Sections 2 and 3:** two presses each. Section 3 shows memory 3 and the glowing receipt appears on the pedestal at Bank 3's left side.
7. **Evidence:** E picks up the receipt (slow carry, no dodge/attack). Jump Crossing 3's two straight marked stones while carrying. At Mara's case file press E: receipt placed, "It was me. Mara did nothing wrong." appears, gate disappears, one transformation step previews. Also try: E away from the desk drops it (it can be picked up again); dropping it over lava loses it and the objective says to restart via lava.
8. **Exit:** walk into the exit marker: "Stage 4 complete / The next chamber is not available yet." No scene load. Re-entering does not add another reveal.
9. **Death reset:** after partial progress (or while carrying), fall into lava. Stage 4 alone reloads: braziers back at start plates, markings/memories hidden, receipt hidden, gate closed, full health, Q ready. Earlier-stage reveal (if arriving from Stage 3) is kept.
10. **Transition and preservation:** Stage 3's exit now loads Stage 4. Stages 1–3 play as before.

Safe Inspector tuning: stone/silhouette/bank Transforms (move markings with their stones), each `SliceTruthBrazier` (positions, `CorrectPosition`, range, `MoveDuration`, memory text), receipt `InteractionRange`, case file position, `MemorySeconds`. Keep markings, memory labels and the receipt saved **inactive**; keep StageNumber 4, SlowTimeUnlockStage 3, useLegacyConstruction off.

## Assistant verification (17 October)

See `docs/PROJECT_STATE.md` → "Stage 4". Report the first failing step.
