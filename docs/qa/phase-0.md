# Phase 0 QA

Date: 16 October 2026. Documentation/planning only; no gameplay changes.

## Performed inspection

- PASS: read version, package manifest/lock, graphics/quality/input/build/serialization settings and SampleScene content.
- PASS: inventory found only template scene and tutorial scripts; no gameplay code or model assets.
- PASS: Windows standalone support exists in the installed 6000.3.13f1 editor; actual building not tested.
- PASS: source size audit found no large binaries requiring LFS today (largest 25,579 bytes).
- FINDING: no Git repository initialized; .gitignore added without initializing/publishing.
- FINDING: input-actions GUID in build settings unresolved in inspected source/package cache. Verify in editor; fix during approved Phase 1.
- PASS: all 13 expected planning/source-policy files exist; Markdown file links resolve; package manifest and lock parse as JSON.
- PASS (static policy check only): .gitignore includes Unity caches, local settings, build outputs, recordings and local credentials; it does not exclude Assets, Packages, ProjectSettings or .meta files. No repository was initialized to test Git matching.
- NOT RUN: Unity compile/Play Mode, graphics, gameplay, performance, standalone build/launch, fresh clone.

## Manual checklist and outcomes

| # | Check | Expected result | Recorded outcome |
| --- | --- | --- | --- |
| 1 | Open in Unity Hub using 6000.3.13f1; allow import; inspect Console | Editor opens; record any errors verbatim rather than assuming a clean baseline | Pending user QA |
| 2 | Open Assets/Scenes/SampleScene.unity and enter/exit Play Mode | Only template camera/light/volume; no player or game expected; record errors | Pending user QA |
| 3 | Inspect Graphics/Quality and Player input settings | URP PC asset selected for PC; new Input System enabled; input-actions reference checked and any missing assignment recorded | Pending user QA |
| 4 | Review ROADMAP and FIRST_PLAYABLE | Explicit agreement or requested changes to connected demo scope, controls, assistance gate and deferred chapter expansion | Pending approval |
| 5 | Confirm deadline and laptop details | Exact trailer/final cutoffs with timezone and CPU/GPU/RAM known; target remains Windows keyboard/mouse | User confirms both submissions Sunday 18 October, 9:30 AM Pakistan time; 12th-gen Core i7, 32 GB RAM, Intel Xe. Exact chip models unspecified; performance untested |
| 6 | Review GAME_DESIGN, asset requests and trailer handoff | Fictional framing preserved; provisional stages and unimplemented features clearly labeled; teammate requests usable | Pending user review |

Phase 0 ends with these documents. STOP for agreement before gameplay implementation. Record user results here; do not convert pending checks to passes without evidence.

