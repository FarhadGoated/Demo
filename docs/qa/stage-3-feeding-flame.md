# Stage 3 manual QA — four pillars

Open Assets/HellSlice/Scenes/FeedingFlame.unity after import. No generator/setup needed.

- Move during the red windup: the shot targets your position when fired. Move again after launch: it does not home. Test at normal speed without Q, then optionally with Q.
- Press Q during a shot: it visibly slows to quarter speed (16 to 4m/s). Shift still dodges at normal speed; Space still jumps.
- Stand beyond each corner pillar and dodge after launch. The shot stops at the pillar, which tips/slides inward. Health must remain unchanged at the projectile strike, then drop once on pillar/entity contact. Repeat strikes cannot cause duplicate damage. A wall/player hit must not count.
- Hit the entity with ordinary attacks: no strength is removed. After one/two/three pillar impacts it continues attacking; after the fourth impact it dies, flames stop, live projectiles disappear immediately and gate/route open.
- Enter the exit: completion/transformation occurs once. Stage 4 does not load because it is not implemented.
- Die during a fall or after partial progress: only Stage 3 restarts, restoring all four standing pillars, full health, flames and closed gate/route. No old falls/projectiles remain. Earlier-stage completion persists; Q stays unlocked with reset cooldown.
- Stop Play: saved scene edits remain. Check the large primitive entity's red telegraph and disappearance on defeat. The Cyclops asset is assigned to Stage 1 instead.

Unity Play Mode and rendered appearance remain unverified. No new Windows build or push.

Contact fix: an isolated Unity check reproduced disabled colliders failing to register all four impacts and verified enabled triggers succeeding at all four poses. Retest the fourth-impact defeat/gate opening and death midway through a fall. All saved scenes were preserved byte-for-byte; no generator needed.
