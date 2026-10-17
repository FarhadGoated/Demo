# Runtime systems reference (Assets/HellSlice/Scripts)

Namespace `HellSlice`. All gameplay reads `SliceSession.Current`; nothing runs unless
`session.Running` (started, not paused/dead/transitioning). Player and camera use
`Time.unscaledDeltaTime`; world objects (enemies, hazards, fireballs, pillar falls) use
`session.Clock.WorldDelta` so Q slow time affects them only.

## SliceSession (`[DefaultExecutionOrder(-100)]`)
- Serialized: `landing`, `useLegacyConstruction` (**0 in every saved scene**), `StartImmediately`,
  `StageNumber`, `SlowTimeUnlockStage`, `Enemy`.
- `Awake`: caches components on the same GameObject (`SliceTime`, `SliceFeedback`, `SliceWorld`,
  `SliceInteraction`, stage controllers). Missing references → logs an error and disables itself;
  it never rebuilds the scene. Calls `World.InitializeRuntime()` and
  `Player.SetReveal(EarlierCompletedStages / 7f, true)`.
- `Update`: Esc pause; Q → `Clock.Activate()` when `SlowTimeAvailable`; then dispatches
  `Possession.Tick` / `FeedingFlame.Tick` / (new controllers) and returns; Stage 1 logic otherwise.
- `Objective`: first non-null controller's `Objective`, else Stage 1 text.
- `CompleteStage()`: guarded by `StageCompleted`; `SliceStageProgress.Complete(StageNumber)`;
  loads `World.NextScene` only if `Application.CanStreamedLevelBeLoaded`.
- `KillPlayer()` → `Restart()`: sets `Transitioning`, reloads `gameObject.scene.path` after one
  frame. Every later death source sees `Transitioning` and does nothing (single reload).
- `Notify(text, seconds)`: message; hidden when the HUD's `MinimalUI` is on (Stages 2+).

## SliceWorld
- Serialized: `Player`, `CameraRig`, `Piece`, `SocketAnchor`, `FigureAnchor`, `RescueSwitch`,
  `PuzzleSwitches`, `ExitVolume` (BoxCollider trigger), `NextScene` (scene name), `sceneRoot`
  (the `Level` root).
- `InitializeRuntime()`: clones every material under `sceneRoot` so runtime colour changes never
  touch assets. `ExitContains(feet)` tests feet + 0.9 m against `ExitVolume` local bounds.
- `SolidMask = Default | Barrier(10)`; `BodyLayer = 8`, `EnemyLayer = 11`.
- `Build()` / `BuildChamber()` are the legacy generator. Do not call.

## SliceInteraction
- Each frame: `Prompt = ""`; if a stage controller exists, `Prompt = controller.HandleInteraction(session)`.
  Stage 1 path handles core carry/socket, figure assist and `SlicePuzzleSwitch`es.
- Typical `Near` test: distance ≤ range AND no `Physics.Linecast` hit on `SolidMask` between
  player+1 m and target+1 m (ignore triggers). Give interaction props no colliders, or the
  linecast can hit the prop itself.

## SlicePlayer
- Fields: `Body` (CharacterController radius .32, height 1.8), `WalkSpeed 4.3`, `CarrySpeed 2.5`,
  hidden `RunSpeed`, `JumpHeight 1.5`, `Gravity 24`, `BodyRenderers` (reveal targets).
- `Carrying` (set by controllers) → carry speed, no attack, no dodge.
- Jump: grounded press only, `v = sqrt(2 g h)` ≈ 8.49 m/s, air time ≈ 0.71 s.
- Dodge: 10 m/s for 0.28 s (gravity still applies, so dodging off a ledge falls), 0.95 s cooldown,
  damage immunity while dodging.
- Health 5; `TakeDamage` → 1.1 s immunity; 0 → `KillPlayer`. y < −3 → `KillPlayer`.
- `SetReveal(amount, immediate)`: lerps body colour from dark to revealed at 0.6/s.

## SliceValuable (carryables: Stage 2 valuables)
- `DisplayName`, `Receptacle`, `Placement`, `RestoredRoute` (activated on return), `Prerequisite`,
  `InteractionRange` (1.65). `Returned`, `Lost`, `Available`.
- Controller pattern while carried: position = player + up 1.1 + forward 0.65; E near matching
  receptacle → `ReturnToPlace()`; elsewhere → `Drop()` (falls vertically onto `SolidMask`; any
  enabled `SliceLava` volume in the swept bounds → `Lost`, object disabled until scene reload).
  `Awake` caches lava volumes, so an object activated later still finds them.

## SlicePossessionStage (Stage 2)
`Valuables[]`, `ExitGate`. Carry/return/drop logic as above; all returns → `ExitGate` inactive +
reveal preview; exit trigger → `CompleteStage()`. Objective shows lost-valuable restart hint.

## SliceFeedingFlameStage (Stage 3)
`Entity` (SliceEnemy with `FireballPrefab` + `PillarEncounter`), `Pillars[4]` (BoxColliders),
`CentralFire`, `ExitGate`, `ExitRoute`, `PillarFallDuration 1.1`. Fireball hit on a pillar →
tip-and-slide toward the entity; pillars stay enabled triggers so `ComputePenetration` registers
contact; four contacts defeat the entity, clear fireballs, open gate/route, preview reveal.

## SliceHazard (timed flame)
`phaseOffset`, `Flames`, `Warning` renderer, `DamageVolume`, `Damage`. 7 s cycle: 0–1 warning
(orange), 1–3 burning (damage), 3–7 cool (teal). World time.

## SliceLava
`Volume` BoxCollider (set `isTrigger`). Polls `Volume.bounds.Intersects(player.Body.bounds)`.
Keep its top well below walkable tops (player bounds bottom ≈ feet).

## SliceTime
`Duration 5`, `RechargeDuration 8`, `Factor 0.25` while active. Locked stages call `ResetState()`
every frame. `Activate()` refuses when locked, active, or recharging.

## SliceStageProgress
Static `HashSet<int>`; `Complete(stage)`, `CountBefore(stage)`. Reset on
`SubsystemRegistration` (each Play session / app launch).

## SliceEnemy / SliceFireball / SlicePuzzleSwitch
- `SliceEnemy`: melee guardian (`DetectionRange`, `GuardRadius`, warning ring, 4 HP) or Stage 3
  stationary shooter (`FireballPrefab`, `FireWindup 1.25`, `FireRecovery 2`, `FireOrigin`,
  `PillarEncounter` → immune to melee). `Appearance` = optional replacement visual (Cyclops in Stage 1).
- `SliceFireball` (prefab `FeedingFlame-Fireball`): `Speed 16`, `Radius .22`, `Damage 1`;
  straight, non-homing, swept-sphere collision, notifies the pillar encounter.
- `SlicePuzzleSwitch`: `InteractionRange`, `InteractionLabel`, `Visual`, `Prerequisite`,
  `OpensGate`; latches `Completed`, turns green.

## UI and feedback
- `SliceHud`: uGUI `ObjectiveText`, `StatusText` (health + slow-time state), `PromptText`;
  `MinimalUI` shows prompt only and a tiny pause/restart label. Begin/pause menu via OnGUI.
- `SliceWorldLabel`: on a TextMesh; renderer enabled within 9 m of the player and in camera line
  of sight. Assign the built-in `LegacyRuntime.ttf` font + its material when authoring labels.
- `SliceFeedback.Cue(frequency, length)`: synthesized sine beeps; no audio assets.
- `SliceSmokeDriver`: optional editor smoke suite (Tools > Run Smoke Checks); not current evidence.
