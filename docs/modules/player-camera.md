# Module: player-camera

Phase: 1. Status: implemented; verification incomplete.

## Purpose and player experience
Responsive third-person navigation and a readable silhouette.

## Scope and exclusions
CharacterController walk/run, camera orbit with obstruction handling and dodge. No jumping, climbing, gamepad or animation rig yet.

## Controls, states and behaviour
WASD moves relative to camera, mouse orbits, Shift runs, Space dodges. One controller owns body/projection input. Camera and player use unscaled delta time.

## Failure and recovery
Pause/focus loss releases cursor; retry restores spawn. Dodge cannot cross colliders. Camera avoids solid geometry.

## Interfaces and dependencies
SlicePlayer, SliceCamera, SliceInput and SliceSession. Body layer 8, projection layer 9, shadow barrier layer 10; layout uses these project-owned slots.

## Unity setup
Generated FirstPlayable scene contains SliceSession; runtime constructs player and camera. Values exposed as fields; layout in SliceWorld.

## Acceptance criteria
Walk all lanes and orbit against walls without penetration; dodge visibly moves and cooldown prevents spam; body remains still during projection.

## Actual implementation and known limitations
Implemented in SlicePlayer.cs and SliceCamera.cs (runtime scripts under Assets/HellSlice/Scripts). Primitive assets and keyboard/mouse remain the baseline. Latest existing editor suite passes 34/34 after test-focus correction. User reports working controls; continuous manual route remains pending. See docs/qa/phase-1.md for current evidence and pending checks. The current UI revision compiled; runtime QA remains pending.

## Presentation-ready explanation
Planned: Responsive third-person navigation and a readable silhouette. Do not describe as verified until recorded checks pass.



