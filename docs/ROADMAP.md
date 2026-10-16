# Roadmap

Updated 16 October 2026. Both submissions due Sunday 18 October, 9:30 AM Pakistan time. Gameplay first; preserve authored scenes and the seven-stage story/religious boundaries.

## Required for playable completion

| Milestone | Playable outcome | Status / gate |
| --- | --- | --- |
| 1. Current chamber | Movement/combat/dodge, core, flame timing, direct rescue mechanisms, assistance, exit; death resets current stage | Developer reports Stage 1 done; final button placement saved. Windows build passes; STOP for standalone QA |
| 2. First two story chambers | False-exit/helping loop, then return-the-valuables objective | After current QA; simple authored components; STOP for QA |
| 3. Chambers 3 and 4 | Flame positioning, then environmental evidence and responsibility | Agree later slow-time unlock and short learning encounter before enabling it; STOP for QA |
| 4. Chambers 5 and 6 | Lower cooperative path, then positioning/timed-hazard assistance | Body movement and mechanisms only; STOP for QA |
| 5. Chamber 7 and ending | Combined corrective action, transformation, light/hospital/waking choice | Exact waking action still requires agreement; STOP for QA |
| 6. Complete build | All seven chambers and ending reachable in Windows build; reliable death recovery | Full manual path QA and basic laptop performance, submission/source checks |

Dependencies: movement/camera, interaction, attack/dodge, direct mechanisms, objectives/transitions, seven-chamber path. The combined test chamber does not replace final story ordering. Each phase has one playable outcome, concise specs, targeted checks, manual QA and a stop for feedback.

Death reloads only the current scene. Earlier completed-stage progress survives reloads within a run; no checkpoint recovery. Shadow remains gradual visual transformation and ordinary cast shadow. Projection and swapping are removed. Space dodge stays available. Q slow time defaults locked (`SlowTimeUnlockStage = 0`); a positive agreed threshold enables it based on the current stage, including replays. No early puzzle may require it.

## Deferred polish

Final assets/animation, detailed transformation art, decorative scenery/lighting/effects, music/audio systems, menus/settings, decorative UI, cinematics and trailer presentation. Keep existing work; no unrelated rewrites, frameworks, or editor-tool expansion. Teammate supplies final art; placeholders stay cheap.

Trailer and public-source submission remain required. Reserve capture/export/upload time, but do not substitute presentation for playable completion or silently remove chambers.
