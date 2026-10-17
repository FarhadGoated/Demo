# Module: time-control

The latest controls use Space for grounded jump and remove Shift sprint. **Q slow time** unlocks in Stage 3. No separate Time Dash implementation or binding exists; Q is preserved. Shift uses the existing dodge.

`SliceSession.StageNumber` and `SlowTimeUnlockStage` are serialized Inspector fields. Unlock stage 0 means locked pending placement agreement; otherwise availability is current stage >= unlock stage. FirstPlayable, WeightOfPossession (Stage 2), and retained NextLanding all use 0. FeedingFlame uses StageNumber=3 and SlowTimeUnlockStage=3. Completed-stage history does not override earlier-stage ability restrictions.

Locked: Q and direct `SliceTime.Activate()` do nothing, timers stay reset, prompts/status are hidden. Unlocked: Q slows enemies/fireballs/flames to 0.25 for 5s, then recharges for 8s; body/camera remain responsive. Death reloads the stage's authored availability with fresh timers. Space jump is independent and available while carrying.

Early flames remain beatable through cooling-window timing; no early puzzle requires slow time. Stage 3 introduces Q on safe ground, then uses it for a straight incoming fireball and timed flame crossing; no upgrade menu/tree.

Acceptance: locked Q silent, grounded Space jump works, configured-stage Q activates, death preserves stage availability and resets timers, earlier replay locks correctly. Compilation passed; manual QA pending.
