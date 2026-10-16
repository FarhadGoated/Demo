# Module: puzzle and hazards

Preserve the user's four flame rows and lava perimeter. Flames telegraph 1s, burn 2s, cool 4s; offsets alternate 0/1. Their common cooling window permits crossing without Q. Actual traversal remains unverified.

Rescue sequence: E near Open Route plate -> permanent opening in the existing partition -> navigate through -> E near Rescue Lock. Final mechanism requires the first. `InteractionRange`, `InteractionLabel`, `Prerequisite`, `Visual`, and `OpensGate` are editable on `SlicePuzzleSwitch`. Body proximity and clear line of sight are required. Completed mechanisms turn green. Gate stays open, so no timed closure can trap the player; death/restart resets the complete sequence.

The former projection-only entrance is open. Cell walls and route partitions stay intact. Assistance remains mandatory before the exit opens. Lava is an editable `SliceLava` volume and reloads the current stage, never a checkpoint.

Acceptance: final lock cannot activate first or through a wall; E drives both actions; passage is traversable without projection or slow time; death resets all latches/flames/core/enemy. Source/static checks pass; manual QA pending.

Doorway QA fix: existing buttress overlapped the intermediate opening, leaving ~0.6m clearance vs a 0.64m player collider. Moved that buttress and its ember seam to z=34.9 beside the north partition; resized the partitions/gate for a clear 2.4m opening (z=32.0–34.4). Only five scene transforms changed. Saved geometry/reference checks pass; manual passage QA pending.
