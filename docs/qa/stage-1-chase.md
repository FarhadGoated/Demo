# Stage 1 chase and Unanswered Call — manual QA

- Visual update: stone textures, ruined walls/columns/rubble and distant lava skyline appear without pink/missing materials. Spawn, route clearance, button prompt and both doors remain readable; no floating slab returns. Check wall visuals against existing colliders and laptop frame rate.

- Reach the opening button and press E: the door opens immediately without waiting for a guardian attack. Check E during its wind-up too; the enemy resumes pursuit, and the door still closes after you pass. Respawn restores the button and closed door.

- Start and run forward: the guardian stays behind with a brief lead; Space jumps and Shift dodges. The old core/socket is absent and no interaction asks the player to carry anything.
- Spawn is centred farther back. Approach the first door from the left, centre and right: reaching it should make the guardian approach and strike the button in every case, then resume pursuit through the opening.
- Reach the closed doorway: the guardian's warning appears, its attack hits the wall button, and the door opens without requiring E or a combat win.
- Run through: after the player's full body clears the doorway, the door closes behind them and the invulnerable guardian remains blocked on the far side.
- The closing doorway must not load Stage 2. After completing rescue/assistance, continue through the final gate to the floor marker and exit beyond it; only that exit should load Stage 2.
- No raised slab should float above the chase doorway. The trapped figure must be visible; after releasing the rescue lock, E near the figure opens the final gate, and crossing the marker immediately behind it loads Stage 2.
- Continue into “The Unanswered Call”: both flame hazards, the rescue switches, the trapped figure, assistance, the exit gate and Stage 2 transition still work in their saved order.
- Die before the button, during the opening, and in the rescue section; also use Respawn. Each reload restores the guardian, button, closed door, chase, flames and rescue puzzle. Earlier-stage progress remains.

Unverified: Unity import/rendering, chase fairness, attack timing, collision clearance, reset behavior and all progression interactions. No Unity launch or gameplay tests were run.
