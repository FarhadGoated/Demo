# Module: interaction

Phase: 1. Status: implemented; verification incomplete.

## Purpose and player experience
Pick up a released mechanism piece, place it, and help someone.

## Scope and exclusions
One piece/socket and one rescue action. No inventory/equipment system.

## Controls, states and behaviour
E uses closest valid interaction in reach with line-of-sight. While carrying, E at socket places; elsewhere drops on safe ground. Attack/projection blocked while carrying.

## Failure and recovery
Drop returns piece to last safe pickup point, avoiding lost items. Retry resets all puzzle state. Locked prompts explain prerequisites.

## Interfaces and dependencies
SliceInteraction uses Session flags and body distance/raycast; SliceWorld creates piece/socket/figure.

## Unity setup
Runtime objects are explicitly referenced; prompt appears only when within range.

## Acceptance criteria
Cannot take piece before enemy defeat; place unlocks flame lane; rescue remains locked until shadow latch; dropping and retry always recover piece.

## Actual implementation and known limitations
Implemented in SliceInteraction.cs (runtime scripts under Assets/HellSlice/Scripts). Primitive assets and keyboard/mouse remain the baseline. Latest existing editor suite passes 34/34 after test-focus correction. User reports working controls; continuous manual route remains pending. See docs/qa/phase-1.md for current evidence and pending checks. The current UI revision compiled; runtime QA remains pending.

## Presentation-ready explanation
Planned: Pick up a released mechanism piece, place it, and help someone. Do not describe as verified until recorded checks pass.



