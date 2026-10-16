# What the prototype is becoming

Updated 16 October 2026 after user feedback. This is a presentation plan, not a claim that the art is complete.

## Current demo versus intended Level 1

The current FirstPlayable scene combines combat, carry/place, slow time, projection and rescue to validate reusable systems and provide a potential trailer sequence. It is not the final authored Level 1. The combined slice was approved; the seven provisional chapters remain the vision.

Level 1, The Unanswered Call, introduces exploration, a short encounter and a trapped figure. The apparent exit loops back. Stopping to help reveals the true route and triggers the first small human reveal. The loop and authored memory/story staging are not implemented. Later stages introduce possession/burden, deliberate positioning around fire, shadow evidence, cooperation, combined timing, and the return. No stage has been removed.

## Intended look and sound

Imagine a narrow, fractured stone route suspended above a lava chasm. Charred arches and distant jagged peaks frame the path. Orange firelight catches the edges of the nearly black protagonist; cool fill light keeps the body and safe ground readable. Sparse drifting ash, short smoke plumes and heat sounds establish the atmosphere. The scene should feel oppressive without hiding the puzzle.

The trapped figure is framed by a warmer light and an audible call. The false route draws attention first; the call makes the player reconsider. On helping, the gate mechanism visibly changes, the sound briefly quiets and a small human area emerges beneath the darkness. The peaceful final light and hospital awakening belong to the eventual ending, not this demonstration gate.

## Replacement plan

| Current placeholder | Intended replacement | What stays |
| --- | --- | --- |
| Capsule people | One rigged humanoid with walk, attack, dodge, hurt and helping animation; reusable enemy rig | Movement, health, interaction and narrative conditions |
| Box walls and floor | Modular scorched rock/ruin meshes, broken arches, rubble and uneven silhouettes | Verified collision and route dimensions |
| Capsule flame jets and flat orange plane | Animated flame effects, emission-based lava, restrained sparks/ash and local light variation | Hazard warning/burning/cooling timing and damage rules |
| Flat dark player material | Controlled darkness mask/reveal on the same character, readable edge lighting | Stage-based transformation, normal cast shadow |
| Synthesized tones | Fire ambience, footsteps, impacts, mechanism cues and a restrained call for help | Event timing; audio remains understandable without dialogue |
| Debug-like explanatory labels | Short contextual prompts, authored world cues, an introductory controls menu | Health, ability status and recoverability |

For Intel Xe, begin with one shadow-casting main light, modest unshadowed local lights, economical particles and limited post-processing. Measure at 720p with recording enabled. Darkness alone is not atmosphere; silhouettes, light direction, sound and movement matter.

## Order of work

1. Finish Phase 1 verification and the requested HUD cleanup. Record manual controls feedback separately from automated test results.
2. At the next phase agreement, present one concrete visual target, then dress the first courtyard and flame crossing to that target before applying it throughout the slice. Show a screenshot comparison for review; do not silently choose final art.
3. Integrate teammate assets after rig/material/performance and public-source license checks. Priorities: humanoid + animations; modular ruin/rock kit; flame/lava effects; ambient and interaction audio. Detailed formats are in ASSET_REQUESTS.md.
4. Author the Level 1 false-exit/helping sequence and additional approved chapters using stable systems. Decide feasible breadth against the Sunday 9:30 AM deadline; no promise of seven polished chapters before testing.
5. Freeze a tested capture build and record genuine gameplay for the trailer. Keep time for editing, export, rights review and upload.

## Communication agreement

Future handoffs identify: playable and tested now; placeholders still visible; next player-visible change; decision needed from the user. Show presentation progress before calling it capture-ready. User handles integration choices and play feedback; teammate requests are concrete and prioritized. Current scene is not an art-quality target.
