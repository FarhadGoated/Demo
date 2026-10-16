# Asset register

No teammate/third-party gameplay assets have been integrated. Original procedural placeholder geometry/materials and synthesized tones are now implemented under Assets/HellSlice. This is an inventory and review ledger, not legal clearance. Record evidence before public source publication.

| ID | Files / purpose | Source / creator / tool | License and attribution | Public source redistribution | Integration status |
| --- | --- | --- | --- | --- | --- |
| BASE-01 | Assets/Scenes, Assets/Settings; initial URP setup | Existing Unity project/template; exact template provenance unconfirmed | Review original template/package terms before publishing | Pending review | Pre-existing; preserved |
| BASE-02 | Assets/TutorialInfo scripts/icon/layout | Existing Unity tutorial/template material | License evidence not supplied; do not infer rights from presence | Pending review | Pre-existing; preserved |
| PKG-01 | Dependencies in Packages/manifest.json and packages-lock.json | Unity registry/built-in packages; versions locked | Retain package notices; review package-specific terms | Do not vendor Library/PackageCache; publish manifest/lock only after audit | Pre-existing dependencies |
| SLICE-01 | Assets/HellSlice scripts, scenes, input JSON; generated primitive layout and tones | Created in this project with Codex; geometry from Unity primitives, original synthesis code | No third-party artwork/audio imported; project-wide license still to be chosen; Unity shader/font dependencies retain their terms | Project-created source; package dependency review still applies | Implemented placeholders, not final art |

For each incoming asset add: ID; paths; purpose; source URL; creator/tool and version; date; license name and stored evidence; attribution string; whether raw files may appear in a public source repository; modifications; dependency/rig requirements; validation results; reviewer/status. Pending, prohibited, or unknown rights must block public source inclusion.

New primitives/materials/code created for the project should also be identified when added. No blanket project license is granted here. Maintain final player-facing credits during implementation and verify against this register before submission.
