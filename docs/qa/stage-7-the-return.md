# Stage 7 manual QA — The Return

Open `Assets/HellSlice/Scenes/TheReturn.unity` alone and press Play (or reach it by finishing Stage 6). Space jumps, Shift dodges, E interacts, Q slows time, Esc opens Settings/Respawn. Report the first failing step.

**Encounter**
1. Walk up the causeway: the Stage 1 Cyclops guardian stands still until you step onto the arena, then pursues after ~1.5 s at the familiar speed. Attacking it never hurts it.
2. Stay 3–10 m away: it stops, shows the red ring, ember lane and red tint, follows you briefly, then charges in a straight line along the lane. Sidestep as it locks, or Shift-dodge as it arrives (it passes through you): no damage. Standing in the lane: 2 damage, charge stops. Close up (<1.6 m): the old melee strike.
3. Stand with a seal behind you and dodge the charge: the seal breaks into rubble and the HUD says to press E. E at the exposed mechanism: core turns green, the guardian freezes ~4 s and shrinks. Count shows 1 / 3. A second E does nothing.
4. Seal 2: its mechanism sits inside the flame jet — step in only while it is teal (or use Q). Seal 3: the two jets alternate; dodge toward the cool side, reach the mechanism up the lane. Seal 1: lava on two sides — check dodges near the corner are fair. No spot behind any seal is out of the guardian's reach.
5. Q during a telegraph: wind-up, charge and jets slow to a quarter; you stay normal speed.
6. Any order works. Third activation: the guardian sinks and fades (~2 s), jets stop, the north gate opens.

**Softlocks / reset**
7. Try to get the guardian stuck (corners, behind seals, near the gate, on the causeway): it keeps returning and can still be baited into every remaining seal. It never leaves the floor.
8. Die mid-encounter (lava, damage, jet) and use Esc → Respawn after breaking/activating seals, during a stun and during the dissolve: guardian, all seals, mechanisms, jets, gate, health and Q reset; earlier stages' appearance/progress kept.

**Transformation and ending**
9. Walk the quiet path: halfway, the player becomes fully visible; the normal ground shadow is still there. You cannot leave the path or skip the gate.
10. At the light: you stop, the screen fades to white with the hospital text, then the room appears facing the bedside table; Q no longer works or shows. Alt-tab during the fade shows the pause label, not a stuck white screen. Walk to the table: "E / Call Mara back". Press once: line shows; ~6 s later the closing card fades in. Pressing E again or re-entering anything does nothing.
11. Stage 7 completion is recorded only by that E press (check: dying/respawning anywhere before it never counts Stage 7). Respawn after the end restarts Stage 7 from the arena.

Tuning (Inspector): `SliceEnemy` on `Level/Enemies` (ChargeRange/MinRange, ChargeWindup/Lock, ChargeSpeed/Distance, ChargeRecovery/Cooldown, SealStagger, ChargeDamage, MoveSpeed); `SliceReturnStage.StunSeconds`, fade timings and texts; jet `phaseOffset`/`Damage`.

**Passed (assistant):** static scene checks (452 records: unique IDs, local references, hierarchy, GUIDs); two independent adversarial reviews (logic, scene) with all confirmed findings fixed; geometric sweeps (no hiding spot, every seal breakable by a baited charge, jets guard Seal 2/3 access as intended); one Roslyn compile of runtime + editor scripts without Unity (0 errors, 0 warnings); earlier scenes unchanged except BorrowedTime's `NextScene`. **Not performed:** Unity import of the hand-authored scene, Play Mode, rendering/lighting, feel, fairness, transitions.
