# Project state

Updated 16 October 2026. Game/trailer deadline: Sunday 18 October, 9:30 AM Pakistan time. Gameplay first; retain the seven-chamber story and religious boundaries.

## Current decisions — supersede earlier projection/checkpoint plans

- Space remains ordinary combat dodge and is available from the beginning. The user's final correction delays **Q slow time**, not dodge. There is no separate Time Dash ability.
- Lava, lethal combat damage, and falling reload the **current scene** from its authored initial state. One transition guard prevents duplicate loads. No checkpoint recovery and no automatic return to the first stage.
- Completed-stage IDs survive scene reloads in a small run-local record. Enemies, mechanisms, carried objects, flame phase, health, cooldowns and temporary effects are scene-local and reset. No disk save system is implemented; stopping Play/quitting begins a new run.
- Playable projection/swapping, activation points, projected actor, body/range markers, F binding, and checkpoint objects/components have been removed. Ordinary cast shadows and gradual human appearance remain.

## Current implementation

Open `Assets/HellSlice/Scenes/FirstPlayable.unity`; normal Play uses its authored objects. NextLanding is the separate destination scene, not a finished second chamber. Both remain `useLegacyConstruction: 0`; do not regenerate them.

- Existing enemy/core positions, four flame rows, lava, route partitions, gates, floor geometry, prefab links and surviving object poses are preserved. The former projection-only bars/collider are removed so the body can enter the rescue cell.
- Current route: defeat manifestation -> carry core to socket -> time the four flame rows -> E on Open Route mechanism -> navigate opened passage -> E on Rescue Lock -> assist figure -> exit. No slow time is required by the early route. Mechanisms latch until stage reload; the gate never recloses behind the player.
- `Level/Systems/Gameplay Session`: `Stage Number` is 1 in FirstPlayable and 2 in NextLanding. `Slow Time Unlock Stage` is **0 in both**, meaning unassigned/locked. A positive threshold makes Q available when the current stage number reaches it. Keep the intended threshold consistent in future scenes. Availability depends on the current stage, never the highest stage visited; replaying an earlier stage respects its configuration.
- Q is ignored internally and at input level while locked, with no slow-time prompt/status. Once configured for a later stage, existing 5s slow time / 8s recharge works. A learning encounter and final unlock placement are pending agreement; do not add early puzzles requiring it.
- Assistance previews one transformation increment; exit commits the stage. On loading/restarting, appearance reflects only completed earlier stages (one seventh per completion). Death rolls back the current stage's provisional reveal without removing earlier progress. Body renderers and normal shadow-casting flags are preserved.
- Minimal HUD and direct Play remain. No new UI, music, settings, editor commands or generation frameworks.

## Verification

- Runtime **and editor** sources compiled with Unity's bundled Roslyn compiler/project references: exit 0, no diagnostics.
- Saved-file checks: valid local references/GUIDs; all scene prefab-instance overrides and surviving positions/rotations/scales preserved; surviving player mesh/renderer blocks unchanged; obsolete projection/checkpoint references absent. `Logs/design-decisions-static.txt`.
- Backup: `LocalOnly/DesignDecisions-20261016-231111/HellSlice`.
- Developer reports the current first stage done after manual play and moving the second button. This is user-reported acceptance, not an assistant-run regression suite.
- Windows x64 build completed 16 October at 23:46 Pakistan time: **Succeeded, 0 errors, 0 warnings**, 102,551,447 bytes, 1m45s. Both FirstPlayable and NextLanding included. Used the existing Build Windows command after saving the open scene; no regeneration or gameplay tests. Local report: `Logs/phase-1-build.txt`.
- Standalone launch, traversal, death/progress recovery, ability availability, visual readability, and performance require QA. Historical smoke results are not current evidence; the adjusted smoke suite was not run.
- Source destination chosen by the developer: `https://github.com/FarhadGoated/Demo`. Preserve the original initial commit and .gitattributes. Generated data/builds/local backups remain ignored.

## Next

Package the current Windows build, push approved source to Demo, then STOP for `docs/qa/stage-1-windows.md` feedback. Fix blockers before advancing. Final Level 1 false-exit design, chambers 2–7, and hospital/waking-action ending remain incomplete. Assets, audio, decorative UI, presentation and trailer polish stay deferred until playable completion.

Latest QA fix: rescue doorway widened to 2.4m; overlapping buttress/seam moved beside the north partition. The developer then adjusted the second button and accepted the stage. That latest authored scene was saved and used unchanged for this build.
