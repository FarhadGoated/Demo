# Stage 6 — Borrowed Time

Fictional coma nightmare about delaying help until others suffer; no religious punishment claim. Saved scene: `Assets/HellSlice/Scenes/BorrowedTime.unity`. No combat, escort AI, projection or new player abilities.

One central platform branches west to two jump stones, east through two existing cycling flame jets, and north past a sliding pillar. Three visible captives and readable route labels establish the choices from the hub. Real floors stay fixed. Gaps are 0.7m with broad, level landings. The pillar moves between Inspector anchors using the existing world-time convention; it pauses movement rather than pushing/crushing an overlapping player. Both sides remain escapable.

E near a captive releases them once and removes their restraints. A saved side walkway becomes active and their simple figure follows authored waypoints to the hub; no navigation or escort task. The walkway also gives the player a safe return shortcut. Captives cannot fail after release. All three releases extinguish the fronts, stop the route hazards, open the hub exit and preview the next transformation step once; entering the exit commits Stage 6 once. No Stage 7 destination yet.

Three simple orange fire fronts advance from authored outer positions toward the captives. Initial deadline: 100 world seconds, including an 8-second visible grace period. After grace, each front passes through authored approach/warning/contact positions. Existing HUD shows rescues and remaining fire time; the last 25 seconds adds a short urgency cue. At deadline, any unrescued captive triggers the existing guarded stage restart without a death scene. Q slows fire, flame phases and the pillar to one quarter speed while the player remains responsive. Pause freezes everything. Tuning targets approximately 45–65 seconds for a cautious successful route, with margin for first-time reading; actual fairness needs human QA.

StageNumber=6, SlowTimeUnlockStage=3; Space jump, Shift dodge and Q preserved. Death, failure and Esc/Respawn reload all timer/front/captive/walkway/gate/hazard/ability state, retaining earlier completions. Direct Play does not invent earlier progress.

Editable objects live below Level/Environment, Puzzles, Hazards and Progression. Gameplay Session's `SliceBorrowedTimeStage` exposes deadline/grace, three rescues, hazards, obstacle and gate. Each `SliceTimedRescue` exposes figure, restraints, safe walkway, escape waypoints/speed, interaction range and fire positions. `SliceMovingObstacle` exposes its collider, end anchors, crossing/dwell timing. No runtime scene generation or persistent authoring tools.

Stage 5 (WeightOfPride) is now imported from Claude's branch and its `SliceWorld.NextScene` targets `BorrowedTime`. Both are included in build settings/the existing build command; transition QA awaits the user.
