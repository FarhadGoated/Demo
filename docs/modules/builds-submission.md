# Module: builds-submission

Outcome: share a runnable Windows Stage 1 QA package matching the saved authored scene and Git source.

Save in Edit Mode, then use existing Tools > A Day Through Hell > Build Windows (SliceBuild.BuildWindows). Unity 6000.3.13f1 builds FirstPlayable and NextLanding for Windows x64 into Builds/Windows. Normal play uses authored objects; no generator is required. Do not run migration or smoke commands to launch the game.

Package the executable, Data directory, UnityPlayer.dll, UnityCrashHandler64.exe, MonoBleedingEdge, and D3D12 together. Exclude *_BurstDebugInformation_DoNotShip. Include the QA checklist and manifest with source commit, scene hash, and build result. Build outputs/local reports stay outside Git.

Source destination authorized by the developer: FarhadGoated/Demo. Preserve existing history. Track Assets/.meta files, Packages, ProjectSettings, and docs; exclude Library, Temp, credentials, logs, backups, and builds. Do not add a license on the developer's behalf.

16 October 2026, 23:46 Pakistan time: current build succeeded, 0 errors, 0 warnings, 102,551,447 bytes, 1m45s. Developer reports Stage 1 complete after the final button move. Standalone launch/gameplay/performance unverified; see docs/qa/stage-1-windows.md. NextLanding is a transition destination and the seven-stage game is unfinished.

Build compilation/package integrity are assistant checks. Gameplay QA belongs to the developer/tester; stop for feedback before advancing. No new editor tooling or gameplay changes in this release phase.
