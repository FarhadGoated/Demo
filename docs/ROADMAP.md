# Roadmap

Updated 17 October 2026. The playable game is six stages. Borrowed Time is archived and disabled; Claude's chapter 7 is integrated as final Stage 6, The Return. Gameplay first; preserve authored scenes and fictional-dream framing.

## Required for playable completion

| Milestone | Playable outcome | Status / gate |
| --- | --- | --- |
| 1. Unanswered Call | Invulnerable guardian chase, core placement, escape; death resets Stage 1 | Implemented; manual QA remains in its checklist |
| 2. Weight of Possession | Matching returns open routes; death resets current stage | Implemented |
| 3. The Feeding Flame | Fireball encounter with optional Q slow time | Implemented |
| 4. The Shadow of Truth | Brazier positions reveal safe routes and evidence | Implemented |
| 5. The Weight of Pride | Collapse, cooperation and merge open the exit | Implemented; exits directly to TheReturn |
| 6. The Return (formerly chapter 7) | Recurring guardian, three seals, final transformation, hospital and waking action | Integrated from `1afd8ea`; manual QA pending |

The build includes all six connected stages. WeightOfPride exits directly to TheReturn; BorrowedTime remains saved but disabled. Completion of Stage 6 is recorded only at the hospital waking action. Each stage retains the existing movement/camera, interaction, jump, objectives and scene-reload systems.

Death reloads only the current scene. Earlier completed-stage progress survives reloads within a run; no checkpoint recovery. Shadow remains gradual visual transformation and ordinary cast shadow. Projection and swapping are removed. Space now performs a grounded jump; Shift sprint is removed. No separate Time Dash binding exists; Shift triggers the existing dodge. Q slow time stays locked in Stages 1/2 and unlocks in Stage 3 (`SlowTimeUnlockStage = 3`), including on restart; earlier-stage replays retain their locks. No early puzzle may require it.

## Deferred polish

Final assets/animation, detailed transformation art, decorative scenery/lighting/effects, music/audio systems, menus/settings, decorative UI, cinematics and trailer presentation. Keep existing work; no unrelated rewrites, frameworks, or editor-tool expansion. Teammate supplies final art; placeholders stay cheap.

Trailer and public-source submission remain required. Describe six stages; exclude archived BorrowedTime from the playable sequence. Capture the final encounter/ending only after manual QA.
