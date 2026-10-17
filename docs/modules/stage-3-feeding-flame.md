# Stage 3 — The Feeding Flame

Current design supersedes the cooling-mechanism sequence. Fictional dream encounter, not a doctrinal classification.

A huge stationary entity occupies the centre of an 18m square arena, surrounded by simple flame visuals. Four solid pillars near the corners accept one fireball hit each. The enemy telegraphs for 1.25s, aims at the player's chest when firing, then sends a straight non-homing projectile at 16m/s; 2s recovery. Q slows shots to 4m/s, helping the player bait and dodge. Shift dodge remains responsive at normal player speed. Space jumps, no sprint/projection.

Projectiles use an initial-overlap check and nearest swept sphere collision; they stop at the first solid/player obstacle. A strike on an assigned pillar starts a controlled tip-and-slide toward the entity and changes its enabled BoxCollider into a trigger; it does not damage the entity immediately. The trigger neither blocks movement nor intercepts trigger-ignoring projectiles. Translation is necessary because the corner pillars cannot reach the centre by tipping in place. The path uses the authored pillar pose/size and targets the entity collider surface. Explicit ComputePenetration contact gates damage, not the fall timer. Each pillar removes one of four health points once, then disappears. Ordinary attacks cannot bypass this. Fourth impact immediately clears remaining encounter projectiles, extinguishes flames and opens gate/route. Old cooling objects remain inactive.

Contact bug diagnosis: an isolated Unity 6000.3.13f1 check reproduced false ComputePenetration results for all four disabled pillar colliders despite visual overlap. Keep them enabled as triggers during the fall. Do not disable the collider before the contact query. Scene reload restores the authored solid collider state. No scene/layout changes are needed for this fix.

Scene: Assets/HellSlice/Scenes/FeedingFlame.unity. Persistent arena, pillars, entity, fire, exit, player/camera/UI/systems are Inspector-editable. Existing prefab links and unrelated saved scene records are preserved. Stage 1/2 scenes are untouched this revision. Stage 2 still leads here.

Tuning: existing entity FireWindup=1.25, FireRecovery=2, FireOrigin local (0,.8,0); PillarEncounter prevents movement and melee damage. Fireball prefab exposes Speed=16, Radius=.22, Damage=1. Encounter exposes PillarFallDuration=1.1 world seconds, four Pillars, Entity, CentralFire, ExitGate, ExitRoute. Q slows falls too. Keep pillar BoxColliders; authored poses/sizes are used without resetting scene edits.

Q remains the existing stage-3 slow time, not a separate Time Dash implementation. Its 5s duration/8s recharge and earlier-stage locks are unchanged. No Q input is required for the encounter.

Death reloads Stage 3 with four intact pillars, four health, enemy timer, flames, gate/route and temporary abilities restored; active falls/projectiles are discarded. Earlier stage completion survives. Fourth pillar impact previews the next transformation step once; entering exit records completion once. Stage 4 is absent, so retain simple completion state.

Targeted compilation and saved-file checks required. Manual QA must confirm aim-at-fire/no-homing, first-obstacle collisions, all four approach lanes, duplicate-hit rejection, ordinary attack immunity, gate/route, full restart and prior progress. No UI automation/gameplay tests. Stop for QA before Stage 4.

Asset assignment corrected: the partner's Cyclops belongs to Stage 1. Stage 3 uses its original large primitive entity again; only the Cyclops visual attachment and primitive-renderer hide overrides were removed. Pillars, fireballs, gameplay settings and authored layout are unchanged. The hell skybox/background may be shared across stages when those assets are pushed; they are not currently present on the remote branch.

Latest verification: runtime/editor compilation; Stage 1/2 byte-identical including saved Cyclops edits; Stage 3 scene differs only by fall-duration field and fireball prefab only by speed. References, input/time routing, contact/deduplication guards and four final contact poses checked. No Unity contact-query, Play Mode or rendered validation. See docs/qa/stage-3-feeding-flame.md and Logs/stage-3-falling-pillars.txt. No build/push; stop for QA.
