# Current chamber QA

Updated 16 October 2026. Outcome: finish the current combined chamber, reach NextLanding, restart and repeat. The seven-chapter game is unfinished.

## Performed

- Unity compilation and existing editor checks: 34/34 PASS, Logs/phase-1-smoke-editor.txt. Includes actual Input Actions and puzzle/gate/reset/transition state. Previous failures superseded after correcting test-focus/input routing.
- Visually verified tests leave Unity in Play Mode at a fresh paused courtyard. Resume plays; normal Play needs no Tools invocation.
- User reports working controls. Tests reposition actors for separate checks; continuous traversal still needs user QA.
- Previous Windows build succeeded (0 errors/1 warning), but is older than current source. No current standalone verification claimed.

## Manual route

Click Resume if already playing; otherwise open FirstPlayable and press Play/Begin. Use Game view, not Simulator.

1. Move/look, attack the enemy and dodge its red warning. Expected: responsive controls, enemy defeated, orange core usable; final exit stays locked.
2. Pick up the core with E, put it back once, recover it, carry it to the nearby socket and press E. Expected: recoverable item and crossing opens; no attack/projection while carrying.
3. Wait for blue/cooling flame strips, press Q and cross both rows. Expected: hazard timing slows while movement stays responsive; reach the far safe area without a collision trap.
4. At the blue floor marker, press F, cross blue bars, hold the switch, then F to return. Expected: body stationary, ordinary walls block shadow, 8m limit, rescue lock releases.
5. Approach the figure, E to help, then cross final exit. Expected: one transformation step, gate opens, NextLanding loads; combat alone cannot unlock it.
6. Esc → Restart chamber; also try death/restart. Expected: fresh enemy/core/health/flags, usable controls, no stale projection or repeated instructions.

All six continuous manual-route outcomes pending. Report first failing step. STOP before more chambers. Cosmetic issues are deferred unless they prevent reading the puzzle.
