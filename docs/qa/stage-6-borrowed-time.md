# Stage 6 manual QA — Borrowed Time

Open `Assets/HellSlice/Scenes/BorrowedTime.unity` alone and enter Play. Space jumps, Shift dodges, E rescues, Q slows time, Esc opens the existing settings/Respawn screen.

- **Fairness and clarity:** From the hub, identify all three captives and the jump, flame and pillar routes. Read the signs/HUD before moving. Try a first run without Q, then a different route order. The 100-world-second deadline includes 8 seconds before fronts move; check there is reasonable reading/waiting margin and no required damage or blind jumps.
- **Slow time:** Use Q while watching a front and a flame jet/pillar. Each should slow together while movement/jump/dodge remain responsive. Check recharge and pause/resume.
- **Rescues:** Reach each figure and press E. Restraints and that fire front disappear; a side walkway opens and the figure follows its fixed path to the hub. No escort, repeated rescue or unreachable prompt should be necessary. Three rescues extinguish/stop the route hazards and open the central exit.
- **Full reset:** Release one figure, spend Q, then let time expire. Repeat by falling in lava and by Esc → CHAMBER → Respawn. All captives, fronts, timer, walkways, gate, health and ability resources must reset; earlier-stage completion and unlocked abilities remain.
- **Softlocks:** Wait against the moving pillar, back away, pass either side and return on every opened walkway. Walk around the closed hub gate when returning from the west/east. Try the exit early: it must not complete the stage. Remaining routes stay accessible in any rescue order.
- **Completion:** After all three releases, return to the nearby hub exit. Verify one transformation increment and one Stage 6 completion, including standing/re-entering the trigger. No Stage 7 is configured yet. Stage 5 → Stage 6 transition remains pending until Claude supplies Stage 5.

Unverified: rendered readability, human first-attempt timing, all route-order/edge-case softlocks and Stage 5 transition. Earlier automated traversal checks passed before the user changed the verification workflow; do not rerun automation or launch Unity without an explicit request. Await user QA feedback.
