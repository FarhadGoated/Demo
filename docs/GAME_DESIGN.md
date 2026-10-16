# Game design

Status: seven-stage story retained; combined first playable implemented but current revision awaits manual QA. PROJECT_STATE.md records actual completion.

## Vision and boundaries

After an accident, the protagonist enters a coma. In a fictional dream he crosses seven frightening chambers reflecting harmful behaviour in his life. He begins as a readable dark silhouette; corrective actions gradually reveal his human appearance. Fear and confusion develop into responsibility and hope. He approaches a peaceful light, wakes in hospital, and makes a concrete choice to change. The exact waking action needs creative agreement; a possible choice is answering a previously ignored call and offering practical help.

The chambers are fictional dream chapters, not an authoritative depiction or classification of Hell. Do not present Dante or the seven deadly sins as Islamic doctrine. Do not invent religious quotations, rulings, or unseen claims. Any religious claims/text require qualified human review. Completion never guarantees forgiveness, purity, or Paradise. Enemies are fictional dream manifestations, not sacred beings. Combat removes obstacles; assistance, restitution, truth, and other corrective actions advance the narrative.

## Play and themes

- Third-person movement, responsive camera, interaction, carry/place, one attack and dodge.
- **Fire:** environmental atmosphere plus readable flame cycles or lava routes. Movable braziers/temporary torches only where a puzzle needs them; no permanent lantern requirement.
- **Time:** Q slow time unlocks at a later, Inspector-configured stage; placement undecided and currently locked. Space dodge stays available. Early puzzles must work without slow time.
- **Shadow:** initial shadow-like appearance expresses incomplete self-awareness. There is no playable projection, activation point, or swap; body movement and environmental interaction solve puzzles.
- **Transformation:** one model with material-driven darkness recession across seven completions. Preserve a normal cast shadow at the end and clear silhouette readability at the start.
- Short reusable encounters, one enemy type, no extensive combos/equipment/bosses. Target each eventual chamber at roughly 1–3 minutes, validated through playtesting.

## Seven provisional chapters

| Stage | Introduce | Complicate once | Payoff |
| --- | --- | --- | --- |
| 1. The Unanswered Call | Explore and notice trapped figure | Apparent exit loops back; short encounter blocks help | Assistance reveals true route |
| 2. The Weight of Possession | Valuables burden movement | Burden makes traversal harder | Returning valuables changes environment |
| 3. The Feeding Flame | Position around enemy/fire | Time the mechanism; slow time only if unlocked by agreement | Controlled route through flames |
| 4. The Shadow of Truth | Position and interact with environmental mechanisms | Gather evidence and operate distant mechanism | Reconstruct memory and acknowledge responsibility |
| 5. The Lower Path | Attractive upper route | Upper route fails; seek cooperation below | Lower cooperative path succeeds |
| 6. Borrowed Time | Positioning and environmental mechanisms | Timed hazards while helping figure cross | Another figure reaches safety |
| 7. The Return | Combine learned mechanics | Final corrective action still needed after combat | Transformation, light, hospital and waking choice |

## First milestone proposal

The current demonstration preserves the seven-stage story: combat frees a core, timed flames lead to direct rescue mechanisms, assistance opens the exit, and appearance begins to change. Death reloads the current stage while keeping earlier completions within the run. No checkpoints/projection; slow time remains locked until a later stage is agreed. See FIRST_PLAYABLE.md and ROADMAP.md for scope and QA gates.

Presentation style: economical stylized ruins, ash and fire against cool silhouettes; clear hazard boundaries and warm assistance/progression cues. Avoid relying on darkness, bloom or smoke that hides controls or routes. Test hardware is a 12th-gen Core i7 laptop with 32 GB RAM and Intel Xe graphics. Provisional target is 60 FPS at 1280x720 with scalable effects; start capture at stable 30 FPS. Limit shadow-casting lights, transparent particles and post-processing, then measure with recording enabled. No performance results exist yet.
