# Module: combat-enemies

Stage 1 has one authored Manifestation prefab instance. Legacy World.Build is disabled in saved scenes. Enemy.Start records the placed transform as its return position; it does not replace that placement. The previous 11m chase radius covered the player start despite the enemy being authored near the ore.

Fix: position the scene instance beside the ore; DetectionRange 4.5m and GuardRadius 5m keep it guarding that area until approached. These Inspector fields control detection and the player's allowable distance from the enemy's authored starting position. Existing windup, recovery, wall checks, health and core-release logic remain. Death reload restores the saved scene instance, including health and position.

LMB attacks. Space now jumps, not dodge; move clear of the red strike warning. Shift triggers the existing dodge; no new combat system or Time Dash binding is introduced. Enemy defeat still does not complete the stage.

QA: one enemy near ore, no immediate charge from spawn, approach/strike/retreat, death and restart restore the authored guard. Targeted code/reference checks do not substitute for Play Mode verification.

Stage 3 only: FireballPrefab and PillarEncounter configure a stationary entity. It telegraphs, aims when firing, and uses straight swept-collision projectiles. Four assigned pillars each remove one quarter strength once; ordinary attacks cannot bypass them. FireWindup, FireRecovery, FireOrigin and projectile Speed/Radius/Damage are editable. Stage 1 melee behavior and scene/prefab setup stay unchanged. Manual QA pending.

Stage 1 art correction: FirstPlayable now contains Cyclops-Visual as an added child of the existing Manifestation scene instance, assigned to Appearance. The existing primitive body renderers are disabled only in this scene; the warning ring, collider, position, detection, health and combat logic remain. The 1.8m visual follows its parent's chase/turns and existing defeat behavior. Its local transform/material are editable without moving the gameplay root. No animation was supplied. Stage 3 has reverted to its large primitive visual. Check chase alignment, textures, warning visibility, defeat and restart in Play Mode.
