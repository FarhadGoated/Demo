# Stage 1 — The Unanswered Call

Saved scene: `Assets/HellSlice/Scenes/FirstPlayable.unity`. The opening is a manual Temple Run-style chase with normal third-person movement: no auto-run or lane controls. The recurring guardian is invulnerable; getting caught reloads Stage 1. Space jumps, Shift dodges, and the Stage 1 time ability remains locked.

The player begins ahead of the guardian and runs to the existing crossing. Press E near the large wall button to open the door immediately using the existing switch interaction. The guardian can also strike it, but waiting for that is optional. The player runs through; once the full player capsule clears the far side, the door closes and the guardian stops at the barrier.

Opening QA correction: player starts centred at (0, 0, -11), guardian at (0, 0, -15), with the existing 1.5-second grace. Reaching within 2m of the doorway directs the guardian toward the button before its strike, so approaching on either side cannot leave it outside button range. After impact it resumes pursuit of the player.

The chase controller then hands off to the existing “Unanswered Call” section. Its flame crossing, rescue mechanisms, trapped figure, assistance, exit and Stage 1 completion flow remain active. Stage 1 no longer exposes or references the old ember core/socket; the shared carry/puzzle systems remain for later stages.

The chase door is at z=15. Stage completion is separate: the final exit trigger and floor marker are at z=42.5, immediately beyond the assistance gate at z=41. Closing the chase door never completes the stage. The trapped figure is saved active so the required assistance interaction is visible. The obsolete raised slab assembly is retained inactive.

Death or Respawn reloads the saved scene, resetting the guardian, button, door, chase state, flames, rescue mechanisms, figure and exit while preserving earlier-stage progress. The saved button is under `Progression`; its position, strike range and wind-up are editable. Door and chase references/tuning are on Gameplay Session's `SliceChaseStage`.

Visual layer (17 October, `b7f5c65`): Poly Haven stone surfaces, Quaternius ruins/rocks and EndlessHellBackdrop applied around the saved layout. The original colliders remain authoritative; new ruin meshes/backdrop add no collisions. Gate masonry is parented to the existing doors and follows their activation. The obsolete raised-slab decoration is excluded. Import/rendering, wall-to-collider alignment and performance require manual QA.
