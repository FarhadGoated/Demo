# Scene authoring and preservation

## How the saved scenes came to be
- Stage 1 (`FirstPlayable`) and `NextLanding` were produced once by the legacy `SliceWorld.Build`
  generator, then converted by `SliceSceneMigration.Migrate` into ordinary saved objects
  (`useLegacyConstruction` switched off). The developer has since tuned them by hand.
- Stages 2 and 3 were created by copying a saved stage and patching the `.unity` YAML text:
  new records use large readable fileIDs (`72…`, `73…`, `74…`, `75…`) and every change was
  counted ("changed 19 existing records, added 32, deleted none").

Both approaches are fine. What matters: the result is a normal saved scene with visible,
selectable, Inspector-editable objects, prefab links intact, no hide flags, no missing scripts.

## Rules when editing an existing scene
1. Ask the developer to save and close the scene (or the Editor) first. Back up the file to
   `LocalOnly/<Phase>/` before patching.
2. Prefer the smallest text patch for a field change (e.g. `NextScene: <NewSceneName>`). Opening
   and saving a scene in Unity re-serializes it and can touch many records — avoid for old scenes.
3. Keep existing object names, fileIDs, prefab instance overrides and Transform values.
4. After patching, verify: `git diff` shows only the intended lines; YAML still parses; all
   `{fileID: N}` references inside the scene resolve to a record in the same file (or are 0 /
   external GUIDs).

## Batch authoring a NEW scene
- Put a temporary `Assets/HellSlice/Editor/<Name>Authoring.cs` with a static method; run
  `Unity.exe -batchmode -quit -projectPath <repo> -executeMethod HellSlice.Editor.<Class>.<Method> -logFile <log>`.
- In the method: `AssetDatabase.CopyAsset(previousStage, newPath)`; `EditorSceneManager.OpenScene`;
  `Object.DestroyImmediate` the old stage-specific objects/components; create new objects with
  `GameObject.CreatePrimitive` (remove colliders from decorative/interaction props), assign
  existing materials via `AssetDatabase.LoadAssetAtPath<Material>`; set serialized fields directly
  (public fields) or with `SerializedObject` for private/`[field: SerializeField]` ones; then
  `EditorSceneManager.MarkSceneDirty` + `SaveScene`. Throw on any missing reference so the batch
  run exits non-zero.
- Delete the authoring script and its `.meta` afterwards (copy to `LocalOnly/` if you want it),
  then re-run the batch compile so the deleted script leaves no stale state.
- Do not touch other scenes from the authoring script.

## Naming and grouping conventions
- Root `Level` with groups `Player`, `Environment`, `Systems`, `Camera`, `Lighting`, `UI`,
  `Puzzles`, `Hazards`, `Progression`, `Enemies`.
- Names describe purpose and state: `Exit gate - opens after …`, `Stage N completion trigger`,
  `Route 2 - opens after gold return` (inactive), `Visuals - replace asset here`.
- Objects that must start hidden are saved inactive and activated by gameplay; tell the developer
  not to save them active.
- World text: TextMesh + `SliceWorldLabel`, built-in `LegacyRuntime.ttf` font.
- Lava: one `Lava - lethal perimeter and gaps` box using `LavaFlat.mat`, BoxCollider trigger,
  `SliceLava.Volume` = that collider, top surface below walkable tops.

## Build settings
`ProjectSettings/EditorBuildSettings.asset` lists scenes with `path` + `guid` (from the scene's
`.meta`). Add new stages after the previous stage and before `NextLanding`; keep `SampleScene`
disabled. Mirror the order in `SliceBuild.BuildWindows`.
