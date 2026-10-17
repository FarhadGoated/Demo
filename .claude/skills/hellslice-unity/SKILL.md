---
name: hellslice-unity
description: How to work on "A day through Hell" (Unity 6000.3.13f1 + URP hackathon game, Assets/HellSlice). Use this skill for ANY task in this repository — adding or fixing a stage/chamber, editing saved scenes, touching Slice* scripts (SliceSession, SliceWorld, SlicePlayer, SliceInteraction, SliceValuable, SliceHazard, SliceLava, stage controllers), compiling or building in batch mode, updating docs/PROJECT_STATE.md or QA checklists, taking in teammate assets, or writing story text. Trigger it even for "small" requests like changing a control, moving a platform, or answering "how do I build", because this project has hard rules (never regenerate saved scenes, preserve the developer's Inspector edits, fictional-dream story boundaries that respect Islamic values, stop for manual QA after each phase) that are easy to break without it.
---

# A day through Hell — project skill

Third-person narrative puzzle-adventure for NASCON. A protagonist in a coma crosses seven
fictional dream chambers about accountability; corrective actions slowly reveal his human
appearance. Two-person team: the **developer** (gameplay, scenes, scripts) and **Umar**
(assets, UI, QA, trailer). Deadline: **Sunday 18 Oct 2026, 9:30 AM Pakistan time** (game + trailer).
Gameplay completion comes before polish.

Always start by reading `docs/PROJECT_STATE.md` (newest notes are near the top and supersede
older sections), then the module doc for the area you touch (`docs/modules/*.md`).

## Non-negotiable rules

1. **Never regenerate saved scenes.** Every gameplay scene is a saved, hand-tuned scene with
   `SliceSession.useLegacyConstruction = 0`. Do not run `SliceWorld.Build`, the
   "Migrate Existing Layout Once" menu, or any generator against an existing scene. The developer
   edits layouts in the Inspector; those edits must survive your change byte-for-byte unless the
   change list says otherwise.
2. **Preserve the developer's edits.** Before touching a scene, confirm the developer has saved in
   Unity (ask if unsure — an open Editor with unsaved changes will overwrite or conflict). Change
   only the records your task needs. Never reorder, re-serialize, or "clean up" unrelated records.
3. **No new menus, editor windows, music, settings, frameworks, or unrelated refactors** unless the
   developer asks. A one-off batch authoring script is acceptable only if its result is an ordinary
   saved scene; delete the script afterwards (keep a copy under `LocalOnly/` if useful) — never
   commit it as tooling.
4. **Stop for feedback after each phase.** Written code is not verified gameplay. Finish with an
   updated `PROJECT_STATE.md`, a short manual QA checklist, and a clear list of what you did *not*
   verify. Do not start the next stage until the developer reports back.
5. **Git:** never commit `Library/`, `Temp/`, `Logs/`, `Builds/`, `LocalOnly/`, `UserSettings/`.
   Ask before pushing, merging, publishing or uploading. Preserve history (repo: `FarhadGoated/Demo`).
6. **Story boundaries** (see below) apply to every line of in-game text, docs, and trailer copy.

## Engine and layout

| Item | Value |
| --- | --- |
| Unity | **6000.3.13f1** — do not upgrade. Editor: `C:\Program Files\Unity\Hub\Editor\6000.3.13f1\Editor\Unity.exe` |
| Render pipeline | URP **17.3.0** (`Assets/Settings`); Input System **1.19.0** (new backend) |
| Target | Windows x64, keyboard + mouse, Intel Xe laptop; 720p start, performance unmeasured |
| Runtime code | `Assets/HellSlice/Scripts` (namespace `HellSlice`, all `Slice*`) |
| Editor code | `Assets/HellSlice/Editor` — `SliceBuild` (build/smoke menus), `SliceSceneMigration` (historic, do not run) |
| Scenes | `Assets/HellSlice/Scenes` |
| Input map | `Assets/HellSlice/Resources/SliceControls.json` (project-owned, loaded by `SliceInput`) |
| Shared materials | `Assets/HellSlice/Materials` (reuse these; `LavaFlat`, `NextLanding-*` stone/gold/cyan/dark) |
| Prefabs | `Assets/HellSlice/Prefabs` — player prefab `NextLanding-Player.prefab` is linked in Stage 2+ scenes |

## Stages (scene list)

| # | Title | Scene | Controller on `Level/Systems/Gameplay Session` | Slow-time unlock |
| --- | --- | --- | --- | --- |
| 1 | The Unanswered Call | `FirstPlayable.unity` | built-in session logic (enemy, core socket, switches, assist) | 0 (locked) |
| 2 | The Weight of Possession | `WeightOfPossession.unity` | `SlicePossessionStage` (+ `SliceValuable`s) | 0 (locked) |
| 3 | The Feeding Flame | `FeedingFlame.unity` | `SliceFeedingFlameStage` (four pillars, fireball entity) | 3 |
| — | legacy landing | `NextLanding.unity` | `landing = 1`; retained, no longer a destination | 0 |

Chain: Stage N's `SliceWorld.NextScene` names Stage N+1. An empty or unbuildable `NextScene`
shows a completion objective instead of loading. Chambers 4 (The Shadow of Truth), 5 (The Lower Path), 6 (Borrowed Time)
and 7 (The Return + hospital ending) are not built yet — see `docs/GAME_DESIGN.md`.

Every saved stage has the same skeleton under one root `Level`:
`Player` (prefab instance `Player body`), `Environment`, `Systems/Gameplay Session`,
`Camera/Player Camera`, `Lighting`, `UI/Gameplay HUD`, `Puzzles`, `Hazards`, `Progression`
(exit gate, `Stage N completion trigger`, floor marker) and optionally `Enemies`.
`Gameplay Session` carries `SliceSession`, `SliceWorld`, `SliceTime`, `SliceInteraction`,
`SliceFeedback`, `AudioSource`, `SliceHud` (MinimalUI on in Stages 2+) and the stage controller.

## Systems at a glance

Full field/behaviour notes: [references/systems.md](references/systems.md).

- `SliceSession` — singleton per scene (`SliceSession.Current`). Owns run state (`Running`,
  `Dead`, `Transitioning`), `StageNumber`, `SlowTimeUnlockStage`, objective dispatch, `KillPlayer`
  (reloads the **current** scene once), `CompleteStage` (records progress once, loads `NextScene`).
  Dispatches `Tick`/`Objective` to whichever stage controller component is present.
- `SliceWorld` — scene references (`Player`, `CameraRig`, `ExitVolume`, `NextScene`, `sceneRoot`),
  per-scene material copies, `ExitContains(feet)`. `Build()` is legacy construction — never call it.
- `SliceInteraction` — computes the E prompt each frame and dispatches to the stage controller's
  `HandleInteraction(session)`; the returned string is the on-screen prompt.
- `SlicePlayer` — CharacterController movement: Walk 4.3, Carry 2.5, JumpHeight 1.5, Gravity 24
  (≈0.71 s air time; carry jump ≈1.8 m, walk jump ≈3.0 m horizontal). `Carrying` flag disables
  attack/dodge. `SetReveal(amount, immediate)` drives the transformation. Falling below y = −3 kills.
- `SliceValuable` — carryable object with `Receptacle`, `Placement`, `RestoredRoute`,
  `Prerequisite`, `InteractionRange`; `Drop()` falls vertically, lava marks it `Lost` until restart.
- `SliceHazard` — timed flame (1 s warning, 2 s burn, 4 s cool; `phaseOffset`), uses world time.
- `SliceLava` — polls its `BoxCollider` (trigger) against the player; contact → `KillPlayer`.
- `SliceEnemy` / `SliceFireball` — one melee guardian type (Stage 1) and the Stage 3 stationary
  fireball entity. `SlicePuzzleSwitch` — latched E mechanism with prerequisite (Stage 1).
- `SliceTime` — Q slow time: 0.25× world time for 5 s, 8 s recharge, only when
  `SlowTimeAvailable` (`SlowTimeUnlockStage > 0 && StageNumber >= SlowTimeUnlockStage`).
- `SliceStageProgress` — run-local `HashSet<int>` of completed stages; survives scene reloads,
  cleared when Play stops. No disk saves, no checkpoints.
- `SliceHud`, `SliceWorldLabel` (world TextMesh visible within 9 m with line of sight),
  `SliceFeedback` (synthesized beeps), `SliceCamera`, `SliceSmokeDriver` (optional old smoke suite).

## Controls (current, do not change without being asked)

WASD move (camera-relative) · mouse look · **Space** grounded jump (also while carrying, no double
jump) · **Shift** (left/right) dodge 0.28 s, 0.95 s cooldown, not while carrying · **E** interact ·
left mouse attack · **Q** slow time from Stage 3 on (locked in 1–2) · Esc pause. No sprint, no
projection/swap, no separate Time Dash, no F binding.

## Stage-flow contract (every stage must honour it)

- Lava, lethal damage and falling → `session.KillPlayer()` → the whole current scene reloads from
  its saved state. All puzzle state, carried items, gates and temporary ability timers reset.
  Earlier-stage completions survive (run-local). Never add checkpoints.
- Transformation: on scene load the player shows `EarlierCompletedStages / 7`. When the stage's
  puzzle is solved, preview one step with `player.SetReveal((EarlierCompletedStages + 1) / 7f, false)`
  exactly once; the exit trigger calls `session.CompleteStage()` (guarded, commits once).
- Abilities are per stage: `StageNumber` + `SlowTimeUnlockStage` in the scene. A later stage keeps
  `SlowTimeUnlockStage = 3`; replaying an earlier stage respects its own lock.
- Objectives/prompts are short strings. In MinimalUI scenes `Notify` messages are not shown, so put
  essential feedback in the controller's `Objective` or prompt.
- No early puzzle may require slow time. Avoid unavoidable deaths and guessing; clues must be
  discoverable before a committing jump.

## Adding a new stage — checklist

1. Write a concise spec first (module doc `docs/modules/stage-N-*.md`, ~20 lines: outcome, layout,
   rules, Inspector fields, completion/death, verification). Agree on anything ambiguous.
2. Create the scene as a copy of the latest saved stage (keeps player prefab link, camera, HUD,
   lighting, render settings). Remove the old stage's puzzle objects and controller, add new
   objects as ordinary GameObjects with clear names (e.g. `Stone 2 - solid`,
   `Silhouette - no floor`). See [references/scene-authoring.md](references/scene-authoring.md).
3. New behaviour goes in a small stage controller (`Slice<Name>Stage`) plus per-object components;
   expose all tuning/references as serialized fields. Register it in `SliceSession` (property,
   `GetComponent` in `Awake`, `Objective`, `Tick` dispatch) and in `SliceInteraction.Update`.
4. Set `StageNumber`, `SlowTimeUnlockStage`, `useLegacyConstruction: 0`, `NextScene` empty.
5. Point the previous stage at it by editing only its `NextScene:` line in the `.unity` text — do
   not open/re-save the previous scene in Unity (that re-serializes it).
6. Add the scene to `ProjectSettings/EditorBuildSettings.asset` (enabled, correct GUID from its
   `.meta`) and to `SliceBuild.BuildWindows` `scenes` (keep FirstPlayable first).
7. Compile/build in batch mode (below), run targeted checks, update docs, write the QA checklist,
   commit on a feature branch, stop.

## Batch compile and build

Only one Unity instance may have the project open; if the developer's Editor is open, ask them to
close it (or to run the menu command themselves). Keep logs outside the repo or in `Logs/`.

```powershell
$unity = "C:\Program Files\Unity\Hub\Editor\6000.3.13f1\Editor\Unity.exe"
# Import + compile only (first import of a fresh clone takes ~10-15 min; run in background)
& $unity -batchmode -quit -nographics -projectPath "D:\path\to\repo" -logFile "D:\logs\compile.log"
# Windows build (also compiles); writes Logs/phase-1-build.txt inside the project
& $unity -batchmode -quit -projectPath "D:\path\to\repo" -executeMethod HellSlice.Editor.SliceBuild.BuildWindows -logFile "D:\logs\build.log"
```

Check: process exit code 0; `grep -E "error CS|Compilation failed|Scripts have compiler errors"`
on the log is empty; build log contains `Windows build complete`; `Logs/phase-1-build.txt` says
`Succeeded`. Output: `Builds/Windows/A day through Hell.exe` (+ Data, UnityPlayer.dll,
MonoBleedingEdge, D3D12). Exclude `*_BurstDebugInformation_DoNotShip` from QA ZIPs.
In the Editor the same build is **Tools > A Day Through Hell > Build Windows**.

Low disk on C: — keep clones, Library and builds on D:.

## Verification style

Targeted, cheap, honest:
- Compile runtime + editor code in batch mode; zero `error CS`.
- Static scene checks: every new serialized reference resolves (fileIDs exist), no missing scripts,
  no hide flags, inactive-by-design objects really start inactive, build settings include the scene.
- Preservation: `git diff --stat` shows only intended files; earlier scenes byte-identical except
  approved lines (a previous stage's diff should be the single `NextScene` line).
- Geometry sanity: gaps vs jump reach (carry ≈1.8 m, walk ≈3.0 m), gaps wider than the 0.64 m
  capsule (+skin) so players cannot walk across, lava trigger top below platform tops.
- Do not repeatedly open the UI or drive Play Mode to "inspect"; one batch run per check.
- Report exactly what ran, what passed, and what remains for manual QA (physical traversal, feel,
  rendering, full playthrough). Never describe written code as play-verified.

## Documentation conventions

- `docs/PROJECT_STATE.md`: add a dated section near the top ("## Stage N — … (17 October)") with:
  what changed, scenes/records touched, what passed, what is unverified, and a final
  **STOP for docs/qa/…** line. Mark superseded notes as superseded rather than deleting history.
- `docs/modules/*.md`: one module per system/stage; concise, current behaviour + Inspector fields.
- `docs/qa/*.md`: "Open `Assets/HellSlice/Scenes/X.unity`, press Play" → numbered checks with
  expected results → safe Inspector tuning → **Passed** (assistant checks) vs **Not performed**.
  Ask the tester to report the first failing step.
- Also keep `README.md`, `docs/LEVEL_EDITING.md`, `docs/ROADMAP.md` status lines in sync when a
  stage is added.

## Assets (teammate intake)

- Requests live in `docs/ASSET_REQUESTS.md`; every imported asset needs a row in
  `docs/ASSET_REGISTER.md` (source URL, creator, date, licence + evidence, attribution, public
  redistribution yes/no, modifications, validation) **before** integration or publication.
  Unknown licence = blocked from the public repo.
- Raw sources/prompts go in `LocalOnly/AssetSources/<ID>/` (ignored). Keep credits (e.g. the
  CC BY 4.0 Cyclops credit in README).
- Replace visuals under a "Visuals" child; keep gameplay roots, colliders and references intact.
- Umar's branch `feature/visual-polish-ui` adds a visual layer (hell sky, lava shader, flame prefabs,
  Business Man player via `PolishSkin`) re-applied with **Tools > A Day Through Hell > Apply Visual
  Polish**. New stage scenes can receive it later; don't hand-build competing visual systems.

## Story and religious boundaries

- The chambers are a **fictional coma dream**, not a depiction or classification of Hell. No Dante
  framing, no seven-deadly-sins-as-doctrine, no claims about punishment, the unseen, forgiveness,
  purity or Paradise. Enemies are dream manifestations, not sacred beings.
- No invented religious quotations or rulings; any religious text needs qualified human review.
- Progress comes from corrective actions (helping, returning, telling the truth, acknowledging
  responsibility); combat only removes obstacles. Completion never guarantees forgiveness.
- Keep in-game text short and humane; describe harm and responsibility plainly, without preaching.

## Developer's working style

Concise spec → implement → targeted checks (no repeated UI inspection) → update
`PROJECT_STATE.md` → short manual QA checklist → **stop for feedback**. Ask only when blocked on a
real decision; otherwise follow the nearest existing pattern. Keep answers and docs terse and
factual. Use the developer's names for things (stage titles, Inspector fields, folder names).
