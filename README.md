# A day through Hell

Third-person narrative puzzle-adventure for NASCON. Seven fictional dream chambers explore accountability and change after a coma. They are not a doctrinal depiction of Hell.

**Status: Current Stage 1 accepted by the developer after manual play and final button placement. Windows build passes with 0 errors / 0 warnings; standalone QA pending. Seven-chapter path unfinished. Gameplay completion precedes polish.**

Stage 2 implementation is now available in `Assets/HellSlice/Scenes/WeightOfPossession.unity`; Stage 1 exits there. Open it and press Play for [Stage 2 manual QA](docs/qa/stage-2-possession.md). Compilation/file checks passed, but Unity import and gameplay are unverified. The existing Windows QA ZIP predates Stage 2; no new build has been made.

- Unity: **6000.3.13f1** (keep this version).
- Rendering: Universal Render Pipeline **17.3.0**.
- Input: Input System **1.19.0**, new input backend enabled.
- Target: Windows desktop, keyboard and mouse; 12th-gen Core i7, 32 GB RAM, Intel Xe laptop. Start at 720p; performance unmeasured.
- Submission: both game and trailer due Sunday, 18 October 2026, 9:30 AM Pakistan time, per latest user clarification.
- Current scene: `Assets/HellSlice/Scenes/FirstPlayable.unity`; original SampleScene preserved.

## Open and run

1. Add this folder in Unity Hub and open using Unity 6000.3.13f1.
2. Allow the existing packages to resolve; do not upgrade them for this prototype.
3. Open `Assets/HellSlice/Scenes/FirstPlayable.unity` and press Play. The saved scene contains editable gameplay objects; no generator is needed.
4. WASD moves, mouse looks, left mouse attacks, Space jumps while grounded, E interacts, and Esc pauses/resumes. Shift sprint is removed. Q slow time is locked pending a later-stage unlock decision. Projection/swapping is removed.
5. Follow [Windows QA](docs/qa/stage-1-windows.md). Death restarts the current stage; earlier completion survives within the current run only.

Save the scene, then choose **Tools > A Day Through Hell > Build Windows** outside Play Mode. Output: `Builds/Windows/A day through Hell.exe` plus required companion files. Share the complete QA ZIP, not the executable alone. Includes FirstPlayable and NextLanding; NextLanding is a transition destination, not a finished second chamber. Do not regenerate authored scenes.

## Project guide

- Resume work from [PROJECT_STATE](docs/PROJECT_STATE.md).
- Review [ROADMAP](docs/ROADMAP.md), [GAME_DESIGN](docs/GAME_DESIGN.md), and [phase QA](docs/qa/phase-0.md).
- Teammate: use [ASSET_REQUESTS](docs/ASSET_REQUESTS.md) and [ASSET_REGISTER](docs/ASSET_REGISTER.md).
- Trailer collaborator: use [TRAILER_HANDOFF](docs/TRAILER_HANDOFF.md).

## Source and submission

Source repository: [FarhadGoated/Demo](https://github.com/FarhadGoated/Demo), selected by the developer; rename deferred. Track Assets and .meta files, Packages including the lockfile, ProjectSettings, and docs. Caches, local backups, logs, builds, and raw captures are excluded. Preserve the original repository history. No project-wide license selected; provenance remains in `docs/ASSET_REGISTER.md`.




## Third-party model credit

Stage 3 uses [Stylized Stone Pillar](https://sketchfab.com/3d-models/bf6337f03be3464d8f5c113242306722) by Ngakau Blake (Punch-Kat), licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/). Supplied Unity material/prefab retained and scene instances fitted to existing pillars; original license/credit is beside the asset.

Stage 3 uses [lava monster](https://sketchfab.com/3d-models/lava-monster-2e2a1eac6f834857af5347a0662b075d) by [Satwik.Bandi](https://sketchfab.com/Satwik.Bandi), licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/). Supplied FBX renamed, textures downscaled to 1024px, and Unity material, idle controller and flame visual prefab added. Original credit is retained beside the asset.

This work is based on [Cyclops Rig](https://sketchfab.com/3d-models/cyclops-rig-e5cc86878c314f5bae6d7268bb7541d9) by [DM-913](https://sketchfab.com/SuperKapoo913), licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/). Converted to FBX by the teammate; rig helper geometry removed, textures configured for Unity. Original credit is preserved beside the asset.
