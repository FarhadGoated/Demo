# Manual QA — current-stage reset, body puzzle, delayed slow time

16 October handoff: developer reports Stage 1 done after the doorway fix and final second-button move. Individual regression checks below were not independently run by the assistant. Current standalone handoff: `stage-1-windows.md`.

Open `Assets/HellSlice/Scenes/FirstPlayable.unity` after import/reload. No Tools command is needed. These checks have not been run by the assistant.

1. **Preservation:** Confirm your right-side enemy/core, four flame rows, lava, floor and partition placements. Play/Stop and save/reopen should preserve all remaining authored objects and prefab links. Only projection/checkpoint-specific objects and the projection-only entrance were removed.
2. **Death reset:** Defeat the enemy, move/carry the core or operate a mechanism, then enter lava (also try during a dodge). Expect one reload of FirstPlayable at its initial spawn: full health, enemy alive, core at its authored start, mechanisms/gates reset, flames restarted. Lethal combat damage should do the same; no checkpoint respawn or death menu.
3. **Body puzzle:** Cross flames without Q. E near Open Route turns it green and opens the passage; walk through and E near Rescue Lock. Final lock must fail before the first or through a wall. Help figure and exit. Gate stays open; death permits a complete retry.
4. **No projection:** F does nothing. No activation point, projected actor, swapping, range/body marker or projection prompts. Player body/camera remain the only controlled pair.
5. **Earlier progress / appearance:** Assistance previews one reveal increment. Reach NextLanding, then during Play set the player body's Y to -4 in Inspector to trigger a fall. Expect reload of NextLanding (not FirstPlayable), retaining the earned appearance. Ordinary cast shadow should remain visible under the scene light. Death before finishing the first chamber should reset its provisional reveal.
6. **Default ability policy:** Space dodge works. Q does nothing; no slow-time UI/prompt, and enemy/flame timing stays normal. Death/restart keeps Q locked.
7. **Temporary unlock test:** In Edit mode, on Gameplay Session set Slow Time Unlock Stage equal to Stage Number, save and Play. Expect Q to activate with feedback, and death to retain availability with fresh timers. Set unlock stage above Stage Number, save/replay: Q is locked again, regardless of earlier completion history. Restore Stage Number 1/2 and unlock stage 0 after testing. Final unlock placement remains undecided.

Unverified: Unity import, Play/Stop, scene save/reopen, physical traversal, death/reload behavior, stage-progress continuity, ability activation and visual quality. Runtime/editor compilation plus saved-file reference/preservation checks passed. Stop and report the first failing check.
