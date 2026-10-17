# Settings and Stage 4 integration

Merged Stage 4 through `7d5477c`; Stage 3's destination is now ShadowOfTruth. Earlier scene layouts and existing visual updates are retained.

Automated: runtime/editor compile passed; 26 isolated Play Mode checks passed, covering scene transition, Esc/F1, pause/resume, mouse and keyboard respawn, and Stage 4 reset with earlier progress/Q retained. See `Logs/stage4-menu-unity.log`. Menu rendering and full Stage 4 traversal remain human QA.

- Press Esc while playing: Settings appears and movement stops. Press Esc or click RESUME: play resumes. F1 has no effect.
- At 720p, confirm the new CHAMBER tab and Respawn row match the existing fonts/highlights and fit above the footer. Check existing settings still respond to mouse and W/S/A/D.
- Solve a section or pick up the receipt, then Esc → Respawn. The same stage should restart with full health, cleared carrying, original puzzles/gate/receipt and retained earlier progress. Also test W/S → Respawn → Enter; A/D must not restart.
- Complete Stage 3 normally and traverse Stage 4's three sections. Verify clue readability, carrying jumps, evidence return and exit. Stage 4 retains Claude's saved visual design.
