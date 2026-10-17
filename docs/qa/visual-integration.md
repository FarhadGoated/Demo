# Visual integration — manual QA

Source: `feature/visual-polish-ui` at `9361833`. This phase applies visuals/UI to the existing saved levels; Claude's Stage 4 is still awaiting push.

Automated checks passed: runtime/editor compilation; preservation of 1,131 existing scene records; serialized local/GUID references; isolated Unity scene/model/material/shader/font import (exit 0). Log: `Logs/visual-integration-unity.log`. No rendered or interactive gameplay pass was performed.

- Open each saved Stage 1–3 scene. Check sky/lava, readable safe ground, player model and unchanged geometry; confirm Stage 1 Cyclops still matches the guardian collider.
- Cross a flame row: particles must follow the existing hot/cool timing. Check visibility at 720p and frame rate on the target laptop.
- Esc opens Settings while pausing gameplay; Esc again resumes. Test mouse and W/S/A/D, volume, sensitivity/invert, brightness, quality and fullscreen. CHAMBER → Respawn resets the current stage; F1 is unbound.
- Complete/transition and die once: confirm earlier reveal progress, current-stage reset, appearance and settings survive the intended transitions. Space jumps; Shift dodges; Q stays stage-scoped.

Models are static poses; animations were not included in the pushed visual assets. This checklist is pending human playtesting, not an assistant-run gameplay pass.
