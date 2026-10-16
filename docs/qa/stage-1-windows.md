# Stage 1 Windows QA

Extract the entire ZIP, then run `A day through Hell.exe`. Keep companion folders/DLLs beside it. Unity is not required. Alt+F4 closes the game. Optional launch arguments for 720p windowed testing: `-screen-fullscreen 0 -screen-width 1280 -screen-height 720`.

Controls: WASD move, mouse look, Left Shift run, left mouse attack, Space dodge, E interact, Esc pause/resume. Q is locked. No shadow projection/swapping.

1. **Launch/layout:** Stage 1 starts; latest second-button placement and widened doorway match the saved scene. No duplicate objects or missing critical geometry.
2. **Route:** Defeat enemy, take core to socket, cross flames, operate first mechanism, pass through gate, operate second mechanism, help figure, exit to NextLanding. This is the end of this QA slice.
3. **Death/retry:** Move core or open a mechanism, then enter lava. Expect one Stage 1 reload: full health, live enemy, original core position, reset mechanisms/gates. Repeat with lethal enemy damage, then complete the puzzle again without a stuck state.
4. **Controls/appearance:** Space dodge works. Q/F do nothing with no obsolete prompts. Assistance reveals part of the character; exiting retains completed-stage appearance. Esc pauses/resumes.
5. **Progress recovery (Editor follow-up):** After reaching NextLanding, move the body below Y=-4 during Play. Expect only NextLanding to reload with earlier completion/appearance retained. Quitting starts a new run.
6. **Report:** Note blockers, missing/pink materials, camera/input problems or severe slowdown. Include reproduction steps, frequency, and source commit from BUILD_INFO.txt. Laptop performance has not been measured.

Evidence: developer accepted current Stage 1; Windows compilation passed with 0 errors / 0 warnings. The assistant did not launch or play-test this build. Full seven-chamber progression, final art and audio are outside this package.
