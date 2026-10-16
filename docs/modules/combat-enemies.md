# Module: combat-enemies

Phase: 1. Status: implemented; verification incomplete.

## Purpose and player experience
One short readable encounter releases a resource without granting narrative completion.

## Scope and exclusions
One chasing enemy, melee attack, dodge, health and death. No bosses/combos or weapon inventory.

## Controls, states and behaviour
LMB attacks within reach and facing cone; cooldown prevents repeated damage. Enemy chases in its arena then telegraphs, strikes, recovers. Dodge grants short invulnerability.

## Failure and recovery
Enemy returns toward arena if target escapes; player death offers retry. Damage interrupts projection; pause freezes all combat timers.

## Interfaces and dependencies
SliceEnemy and SlicePlayer share SliceSession; enemy uses WorldDelta, player uses real delta. Raycasts prevent attacks through walls.

## Unity setup
Enemy spawned at courtyard z=8, with constrained arena; primitive figure and visual windup marker.

## Acceptance criteria
Enemy responds to slow time; attack/dodge feedback clear; kill releases piece but never exit gate; death/retry resets health/enemy.

## Actual implementation and known limitations
Implemented in SliceEnemy.cs and SlicePlayer.cs (runtime scripts under Assets/HellSlice/Scripts). Primitive assets and keyboard/mouse remain the baseline. Latest existing editor suite passes 34/34 after test-focus correction. User reports working controls; continuous manual route remains pending. See docs/qa/phase-1.md for current evidence and pending checks. The current UI revision compiled; runtime QA remains pending.

## Presentation-ready explanation
Planned: One short readable encounter releases a resource without granting narrative completion. Do not describe as verified until recorded checks pass.



