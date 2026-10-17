# Stage 6 continuation handoff

Status (17 October 2026): BorrowedTime remains saved for reference but is not a playable stage. It is disabled in build settings and excluded from the build command. TheReturn (formerly chapter 7) is now final Stage 6; Stage 5 targets it directly. Do not follow the implementation instructions below unless the user explicitly restores this concept.

Updated 17 October 2026. Continuation is complete and Stage 6 is saved for manual QA. **The historical investigation below is superseded:** the last isolated run passed 81 checks; startup failure was harness focus pause, traversal failure was scripted navigation into the closed gate, and tiny labels were enlarged. Human timer fairness/readability and Stage 5 integration remain unverified/pending. See `docs/qa/stage-6-borrowed-time.md`.

**New user workflow:** do not launch Unity, run gameplay automation, capture screenshots or repeatedly inspect UI unless explicitly requested. After future code changes, at most one targeted compile without Unity if available, plus relevant code review. Do not repeat successful checks without a change warranting it. Keep docs/report brief and stop for manual QA feedback. Do not execute the historical test commands below under the new workflow.

## Read first

- `docs/PROJECT_STATE.md` (latest sections supersede historical notes).
- `docs/modules/stage-6-borrowed-time.md` (current design and editable fields).
- `docs/LEVEL_EDITING.md`, `docs/modules/stage-flow.md`, player/camera and transformation docs as needed.
- Runtime Stage 6 files and shared session, interaction, clock, hazard, player and progression systems.

Workspace: `C:\Users\Muhammad Farhad Amer\OneDrive\Desktop\Game\A day through Hell`. Unity 6000.3.13f1. Preserve every existing scene edit and unrelated local asset/change. Do not regenerate existing scenes or clean untracked files. No new menus, music, elaborate effects, editor tooling or abilities. No push/build requested. Stop for feedback after implementation, targeted checks and manual QA checklist.

## User requirements

Implement Stage 6, **Borrowed Time**, a fictional coma nightmare about delaying help until others suffer. Compact central hub with three rescue branches: platform jumps, existing timed flame jets and a moving obstacle. Visible authored fire fronts advance toward three captives. E releases each figure along a fixed safe path, without escort AI. All three rescues open a short, clear exit. An unreleased captive reached by fire restarts the whole stage without graphic death. Allow a reasonable first attempt; Q slow time affects fire and relevant hazards. Space jumps, Shift dodges, preserve unlocked abilities. Death/failure/Respawn reset timer, fronts, captives, hazards and ability resources while retaining previous-stage completion. Apply transformation once on success. Everything remains editable in Hierarchy/Inspector.

Claude is still making Stage 5 and will push it later (explicit user clarification). It was absent at the last fetch. Connect its `SliceWorld.NextScene` to `BorrowedTime` when it is actually available; do not fabricate Stage 5. If unavailable on completion, clearly document this remaining integration dependency.

## Saved implementation

- Scene: `Assets/HellSlice/Scenes/BorrowedTime.unity`, plus meta. Direct Play, StageNumber 6, Q unlocked from stage 3, no legacy runtime construction.
- New runtime scripts: `SliceBorrowedTimeStage`, `SliceTimedRescue`, `SliceMovingObstacle` under the existing HellSlice scripts directory, with metas.
- Session and interaction dispatch to Stage 6; existing `SliceBuild` and `ProjectSettings/EditorBuildSettings.asset` include the scene.
- Two new BorrowedTime jet materials with metas; concise spec already saved.
- Hub branches west across two jump stones with 0.7m gaps, east through two existing timed jets, north past a sliding pillar with space to pass. Captive release enables an authored side walkway for a safe return. Figure uses fixed waypoint motion. All rescues stop hazards and open the hub gate.
- Deadline 100 world seconds, including 8-second grace. Fronts interpolate between authored anchors. Existing HUD reports remaining time and rescue count; urgency text in final 25 seconds. Q scales world time to 0.25. Pillar refuses movement that would overlap/push the player.
- Existing visual assets reused. Extra inherited visual-polish effect root is inactive. Check route label readability: cloned TextMesh character size is 0.1 and new transform scale around 0.035–0.045, possibly too small. Prefer bounds/settings verification and one necessary view only; user requested no repeated UI inspection.

## Checks and current failure

Runtime and editor Roslyn compilation both passed with no diagnostics. Static scene check passed: 450 unique records, resolved local references, expected captive/hazard counts. All earlier scene hashes match the Stage 6 baseline. Logs: `Logs/stage6-static.txt`.

The first isolated Unity Play Mode test **failed before gameplay assertions**:

`Logs/stage6-unity.log`: `BORROWED CHECK FAIL saved Stage 6 initializes without generator`, result 0 passed, exit 1.

Failing conjunction in local test driver is `s.StageNumber == 6 && s.BorrowedTime != null && s.Running`. Split these checks and log Started/Paused/Dead/Transitioning to diagnose. Scene serialization contains StageNumber 6, StartImmediately 1 and the correct attached controller script GUID. A possible cause is batch editor focus pausing the session before the driver enables `SliceSmokeDriver.IsRunning`; this is **unconfirmed**. The earlier Stage 4 harness explicitly called `s.Begin()`. Fix a harness issue only after identifying it; do not hide a real initialization bug.

Isolated test project: `LocalOnly/CyclopsImport`. Test driver: `LocalOnly/CyclopsImport/Assets/Editor/CheckBorrowedTime.cs`. Its planned checks cover grace, Q fire/jet/pillar scaling, pause, wrong-range interaction, rescue-failure reload, lava death reload, real CharacterController jump/jet/obstacle routes, all rescues/escape paths, timer margin, exit once and Esc/Respawn. None of these Stage 6 runtime checks has passed yet. Unity's known isolated SearchDatabase startup exception is separate from gameplay; inspect gameplay failures independently.

Local helpers (excluded from project source):

- `LocalOnly/Stage6/prepare_checks.py`: verifies prior scene preservation and new references, builds compiler response files, syncs source into isolated project. Safe to rerun after edits.
- `LocalOnly/Stage6/runtime.rsp`, `editor.rsp`: compiler response files.
- `LocalOnly/Stage6/scene-baseline.json`, `ids.json`: prior scene hashes and new scene IDs.
- `LocalOnly/author_stage6.py`: one-time scene authoring script; **do not rerun over the saved scene**. It has an overwrite guard. Make targeted scene edits.

Runtime paths:

- Python: `C:/Users/Muhammad Farhad Amer/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe`
- Unity: `C:/Program Files/Unity/Hub/Editor/6000.3.13f1/Editor/Unity.exe`
- Compiler: Unity's `Editor/Data/NetCoreRuntime/dotnet.exe` with `Editor/Data/DotNetSdkRoslyn/csc.dll`, `/nologo` and response files above.
- Isolated Unity test invocation uses `-batchmode -projectPath <absolute LocalOnly/CyclopsImport> -executeMethod CheckBorrowedTime.Run -logFile <absolute Logs/stage6-unity.log>`. Use hidden Start-Process for background app runs. Previous test process exited. Running Unity requires the established escalation flow.

## Remaining work

1. Diagnose initialization test failure and fix runtime or harness as evidence requires.
2. Finish targeted play checks, including actual jumping, fair timer margin, Q hazards, releases, full resets, softlocks, safe escape and single completion/transformation. Tune only as justified.
3. Check label readability and scene hierarchy/editability without repeated UI inspection.
4. Inspect whether Claude's Stage 5 has arrived; integrate its exit if available while preserving edits. Otherwise document the dependency.
5. Update this handoff and PROJECT_STATE with honest outcomes. Add a short Stage 6 manual QA checklist covering timer fairness, slow time, every rescue, death/failure/menu reset, softlocks and completion. Give the scene path and stop for user feedback.

## Preserve previous work

Local main HEAD is `7d5477c` after fast-forward merging Claude's Stage 4. Stage 3 now links to ShadowOfTruth. Stage 4 scene stayed byte-identical to Claude's commit. Prior visual integration from `origin/feature/visual-polish-ui` at `9361833`, Cyclops configuration and user scene edits remain in the working tree, with many untracked assets. Do not discard these or `Userdocs.zip`.

Esc settings/Respawn work is already complete, preserving UI/font style; F1 is unbound. Its isolated test passed 26 checks (`Logs/stage4-menu-unity.log`); see `docs/qa/settings-stage4-integration.md`. Earlier PROJECT_STATE entries mentioning F1 or a pending Stage 3→4 link are historical. Recovery backups/stash information live in `LocalOnly/Stage4Merge`; visual backups in `LocalOnly/VisualIntegration`. No push or new Windows build was made.
