# Module: visual transformation

The protagonist begins as a dark silhouette; corrective actions progressively reveal human appearance across seven stages. Preserve the ordinary cast shadow. This is visual storytelling, not projection or an ability unlock.

`SlicePlayer.SetReveal` blends existing body materials toward their revealed color. Stage entry uses the count of completed earlier stages divided by seven. Assistance previews one further increment; exiting commits that stage once. Death resets the current stage's provisional increment while preserving earlier completions. Replaying an earlier stage uses its intended earlier-stage appearance. No new character art or visual polish in this phase.

Body renderer/mesh blocks and cast-shadow flags were preserved. Compilation/static checks pass; appearance, blend readability and continuity after death/transition need manual QA.
