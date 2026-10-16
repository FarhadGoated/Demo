# Module: shadow identity (projection retired)

Current decision supersedes all earlier projection/anchor/swap specifications. Shadow is the protagonist's initial appearance and an ordinary cast shadow, not a separately controlled actor.

Removed: projection input/F binding, activation points, projected CharacterController, body/range markers, projection-only barriers and runtime control code. Player body visuals, normal cast shadows and transformation remain.

The rescue puzzle now uses `SlicePuzzleSwitch`: E on first mechanism opens the route; the final mechanism requires the first and direct body interaction in range/line of sight. No extra actor, remote interaction or camera swap.

Acceptance: no projection component or prompt, F has no action, body solves both mechanisms, original silhouette/cast shadow remain. Source/static checks pass; manual QA pending.
