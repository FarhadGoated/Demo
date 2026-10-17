# Module: stage flow and death

Lava contact bypasses dodge/combat immunity. Lava, lethal damage and falling call `KillPlayer`, which marks the session dead and queues a single reload of its own scene path. Restart also reloads the current scene. No checkpoint recovery or forced FirstPlayable load.

Scene reload restores initial player/enemy/puzzle/core/hazard/temporary-ability state. `SliceStageProgress` retains completed stage IDs across scene loads in the current run; only stage exit commits completion, never enemy defeat or provisional rescue. Stopping Play/quitting clears this run-local state; disk saving is outside current scope.

All gameplay remains authored in saved scenes. `StageNumber` identifies the stage for earlier-completion appearance and stage-scoped ability availability. Preserve manual layout/Inspector changes and prefab links.

Stage 1 now targets WeightOfPossession; original NextLanding is retained. `CompleteStage` guards duplicate completion and uses the run-local record. Stage 2's final return previews transformation; its exit commits completion. An empty/unavailable next scene does not load: Stage 2 displays essential completion feedback while retaining movement. Death still reloads the current scene, and repeat completion cannot stack the reveal. Build settings/existing build command include Stage 2.

Acceptance: repeated death triggers queue one load; current scene remains current; state resets fully; earlier completions survive; early replay respects its ability configuration. Runtime/editor compilation and serialized-reference checks pass; manual QA pending.
