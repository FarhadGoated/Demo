# Death screen QA

Open `Assets/HellSlice/Scenes/FirstPlayable.unity`, press Play, then die each way below.

1. Walk into lava. Expected: brief red flash, screen darkens and greys, embers rise, "DEATH, AGAIN" appears letter by letter, the fire verse appears, then Rise Again / Leave Hell slide in. No old "Restarting stage..." text.
2. Let the guardian catch you. Expected: same sequence with the "You ran..." verse. The death count line reads "THE SECOND DEATH".
3. Press any key during the reveal. Expected: it skips straight to the choices.
4. W/S and mouse hover move the highlight; Enter or click chooses.
5. Rise Again. Expected: fade to black, chamber reloads in its saved state, fades back in, normal play and Esc settings work.
6. Leave Hell once. Expected: label changes to PRESS AGAIN for about 3 s; choosing it again quits (in the Editor, stops Play).
7. Repeat in a Stage 2+ scene (MinimalUI) and fall off an edge. Expected: fall verse, same flow.

Report the first failing step.

**Passed (assistant):** Windows build; automated capture of reveal, causes, quit confirmation, Rise Again reload.
**Not performed:** hand play, audio feel, Stages 2-6, ultrawide and 4:3 layouts.
