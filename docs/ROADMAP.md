# Roadmap

Updated 16 October 2026. Both submissions due Sunday 18 October, 9:30 AM Pakistan time. Gameplay first; preserve authored scenes and the seven-stage story/religious boundaries.

## Required for playable completion

| Milestone | Playable outcome | Status / gate |
| --- | --- | --- |
| 1. Current chamber | Movement/combat/jump, core, flame timing, direct rescue mechanisms, assistance, exit; death resets current stage | Developer reports Stage 1 done; final button placement saved. Windows build passes; STOP for standalone QA |
| 2. Weight of Possession | Two matching returns open three short platform routes; grounded carrying jumps and flame variation; exit completes Stage 2 | Separate authored scene implemented; runtime/editor compilation and file checks pass; STOP for guardian/parkour manual QA. Stage 1 remains editable; its false-exit revision stays outstanding |
| 3. The Feeding Flame / then chamber 4 | Four-pillar fireball encounter with optional Q slow time; later environmental evidence/responsibility | Stage 3 saved scene implemented; compilation/static checks pass; STOP for manual Stage 3 QA. Stage 4 not started |
| 4. Chambers 5 and 6 | Lower cooperative path, then positioning/timed-hazard assistance | Body movement and mechanisms only; STOP for QA |
| 5. Chamber 7 and ending | Combined corrective action, transformation, light/hospital/waking choice | Implemented 17 Oct in `TheReturn.unity` (guardian return, three seals, walk, hospital, "Call Mara back" pending lead approval); STOP for QA |
| 6. Complete build | All seven chambers and ending reachable in Windows build; reliable death recovery | Full manual path QA and basic laptop performance, submission/source checks |

Dependencies: movement/camera, interaction, attack/jump, direct mechanisms, objectives/transitions, seven-chamber path. The combined test chamber does not replace final story ordering. Each phase has one playable outcome, concise specs, targeted checks, manual QA and a stop for feedback.

Death reloads only the current scene. Earlier completed-stage progress survives reloads within a run; no checkpoint recovery. Shadow remains gradual visual transformation and ordinary cast shadow. Projection and swapping are removed. Space now performs a grounded jump; Shift sprint is removed. No separate Time Dash binding exists; Shift triggers the existing dodge. Q slow time stays locked in Stages 1/2 and unlocks in Stage 3 (`SlowTimeUnlockStage = 3`), including on restart; earlier-stage replays retain their locks. No early puzzle may require it.

## Deferred polish

Final assets/animation, detailed transformation art, decorative scenery/lighting/effects, music/audio systems, menus/settings, decorative UI, cinematics and trailer presentation. Keep existing work; no unrelated rewrites, frameworks, or editor-tool expansion. Teammate supplies final art; placeholders stay cheap.

Trailer and public-source submission remain required. Reserve capture/export/upload time, but do not substitute presentation for playable completion or silently remove chambers.
