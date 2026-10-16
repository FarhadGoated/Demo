# Teammate asset requests

Use placeholders immediately; asset sourcing must not delay the playable slice. These are preparation requests, not purchase authorization. Pipeline: Unity 6000.3.13f1, URP 17.3.0, Windows laptop with 12th-gen Core i7, 32 GB RAM and Intel Xe graphics. Prefer one consistent stylized treatment: broken stone, ash, warm fire and cool readable silhouettes. Start at 720p with simple materials, modest texture sizes and low particle overdraw; final asset budgets depend on measured performance.

| ID / priority | Purpose and style | Scale / size / format | Animation and dependencies |
| --- | --- | --- | --- |
| A01 / P1 | Single protagonist, normal human revealed through darkness; distinct limbs/silhouette | ~1.8 m tall, FBX, textures PNG (start at 1024px); one main material where practical | Proposed Unity Humanoid-compatible rig, idle/walk/run/attack/dodge/hit; consistent skeleton, in-place clips; root-motion off initially. Validate before adoption |
| A02 / P1 | Modular ruined corridor/courtyard kit, stylized stone | 1 Unity unit = 1 m; 2 m floor/wall modules, doors ~2.5 m high; FBX, simple pivots, PNG textures | Static; simple separate colliders; URP-compatible materials, no HDRP-only shaders |
| A03 / P1 | Flame vent, gate, switch, carryable piece | Flame footprint ~1 m, gate ~2.5 m high, piece ~0.3–0.5 m; FBX/PNG | Static parts with separate moving pivots; logical states controlled by Unity, no baked puzzle logic |
| A04 / P1 | HUD artwork: health, time duration/recharge, shadow icon, range indicator, body marker, E prompt | 1920x1080 reference sheet; individual transparent PNG icons 128/256px, marker 128px, bar fill/frame separately at 512x64; document safe margins | No baked text in reusable icons; implementation uses actual interactive Unity UI and scalable layout |
| A05 / P1 | Fire, footsteps, attack/hit, switch, gate and completion cues; restrained dream ambience | WAV 48 kHz; short mono spatial cues, stereo ambient loop with clean seam | Label loop points and intended volume; no clipped peaks; source and public redistribution evidence required |
| A06 / P2 | Reusable dream enemy and trapped figure | ~human scale, FBX; reuse validated rig if possible; PNG textures | Enemy idle/move/attack/hit/defeat; figure idle/distress/help. Placeholder figures acceptable |
| A07 / P2 | Ash/smoke, projection and transformation reference art | PNG reference at 1920x1080; small transparent textures if useful | Low overdraw; retain readable hazard edges and character. Effects built in Unity |
| A08 / Later | Hospital props, light/ending visual references | Modular bed/room at human scale, FBX/PNG | Depends on approved ending and time after capture build |

## Intake checklist

1. Supply ID, purpose, source URL, creator/tool/version if known, acquisition date, license text/evidence, required attribution and explicit public source-redistribution status. Unknown status remains blocked for public repository.
2. Preserve generated prompts/source in `LocalOnly/AssetSources/<ID>/` until rights and relevance are checked; that directory is ignored. After clearance, select publishable files/prompts for a reviewed source-art folder. Never publish credentials or private source prompts.
3. For Meshy/AI models inspect geometry, disconnected parts, normals, UVs, material count, texture paths, import scale/orientation, rig/avatar validity, skin weights, animation compatibility and measured performance. A generated mesh is not automatically animation-ready.
4. Test in URP with the actual player/enemy pipeline before replacing placeholders. Record polygon/material/texture statistics and laptop profiling results, rather than claiming a universal polygon limit.
5. Update ASSET_REGISTER before integration/publication. Free, paid, and AI-generated are not license classifications. A right to ship a compiled game may not include raw source redistribution.
