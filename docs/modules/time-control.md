# Module: time-control

The latest user correction preserves Space dodge and delays **Q slow time**. No separate Time Dash implementation exists.

`SliceSession.StageNumber` and `SlowTimeUnlockStage` are serialized Inspector fields. Unlock stage 0 means locked pending placement agreement; otherwise availability is current stage >= unlock stage. Both saved scenes use 0. Set the agreed threshold consistently on later scenes. Completed-stage history does not override earlier-stage ability restrictions.

Locked: Q and direct `SliceTime.Activate()` do nothing, timers stay reset, prompts/status are hidden. Unlocked: Q slows enemies/flames to 0.25 for 5s, then recharges for 8s; body/camera remain responsive. Death reloads the stage's authored availability with fresh timers. Space dodge is independent.

Early flames remain beatable through cooling-window timing; no early puzzle requires slow time. After agreeing a later unlock stage, introduce one short, clearly telegraphed flame crossing as a learning opportunity; no upgrade menu/tree.

Acceptance: locked Q silent, Space works, configured-stage Q activates, death preserves stage availability and resets timers, earlier replay locks correctly. Compilation passed; manual QA pending.
