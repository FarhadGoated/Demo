# Mara (A08): trapped figure (link only, no files in this repo)

**Asset:** "Modern Female Character B" by Alpen Wolf (Unity Asset Store, free)
**Link:** https://assetstore.unity.com/packages/3d/characters/humanoids/humans/modern-female-character-b-283633
Version 1.0 (17 May 2024, made for Unity 2021.3); package size 130 MB.

## Why the files aren't here

The asset uses the **Standard Unity Asset Store EULA**. That licence lets us use it inside the built game, but it does **not** allow redistributing its raw model or texture files, so they can't go into this public repo. Each developer gets their own copy through their Unity account. Before release, confirm with the lead or the organisers that a link-only asset meets the "all assets in the repo" rule.

## How to get it

1. Open the link above and click **Add to My Assets** (you need to be signed in with a Unity account).
2. In this project, which must be opened with **Unity 6000.3.13f1**, go to **Window > Package Manager > My Assets**, select **Modern Female Character B**, then **Download** and **Import**.
3. Keep the imported folder **out of git**: add its path to `.gitignore`, or import it under `LocalOnly/`. Don't commit it.

## Intended use (from the docs and the story notes)

- Mara is the trapped figure and the coworker the protagonist blamed (Stage 4). Scale her to a normal adult height of about 1.8 m.
- Her look should be translucent, like a **dream memory** of her, not a "soul". The docs say there should be no religious claims (`docs/GAME_DESIGN.md`).
- She's a modest, normally dressed woman. Use her as a visual only and keep the gameplay colliders on the existing figure objects, the same pattern as `SliceEnemy.Appearance` for the giant and the Cyclops.
- She isn't integrated into any scene yet.
