# Module: player-camera

Current controls: WASD camera-relative walk, mouse orbit, Space grounded jump, Shift dodge, E interaction, left mouse attack, Esc pause. Shift sprint removed. No separate Time Dash action/binding exists in the inspected input map; Q slow time is retained and locked in both playable stages. Dodge was only unbound, not removed. Left and right Shift now bind the existing Dodge action: 0.28s directional movement, 0.95s cooldown, existing damage immunity, disabled while carrying. Shift never sprints or activates slow time/Time Dash.

SlicePlayer reuses the existing CharacterController. JumpHeight (default 1.5m), Gravity (24m/s2), WalkSpeed (4.3m/s) and CarrySpeed (2.5m/s) are Inspector fields. Grounded press only; holding Space does not repeat jumps and airborne presses do not double-jump. Carrying allows the same jump height with slower horizontal speed. Ceiling contact cancels upward velocity; airborne stepOffset is zero to avoid climbing platform sides. Old serialized RunSpeed is retained hidden but has no effect.

Authored player/camera prefab links remain; no runtime rebuild. Camera and player retain unscaled time. Death reloads the current authored stage. No new animation, UI or tools.

QA: grounded/held/airborne Space, head clearance, carrying jumps, Shift dodge/cooldown without sustained sprint, camera follow, lava reset. Compilation and geometry checks are distinct from physical Unity QA.
