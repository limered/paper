# 04 — Ghost crease renderer + click to advance

Status: ready-for-afk

## Parent

`docs/GDD.md` → Engine roadmap point 5 ("ghost-crease renderer") and design decisions Q4 (click ghost), Q5 (one at a time, wrong clicks do nothing).

## What to build

Replace the auto-play debug from #03 with the real player-facing input: a ghost crease for the **next** required step of the current template is rendered on top of the paper; clicking on (or near) that ghost crease applies that step via `FoldInteractionApplier` + `FoldAnimator`, and the ghost advances to the next step. When the sequence is exhausted, the model is complete (emit a `TemplateCompletedEvent` carrying the template id and the paper id; the actual celebratory beat is #07's responsibility).

Per GDD Q5:

- Only one ghost crease is visible at a time — the next step's.
- Clicks anywhere except on the ghost are ignored. No undo, no error feedback.
- Clicks during `FoldAnimator.IsAnimating` are ignored.

Render the ghost as a coloured line segment on the top face of the relevant layer. A simple Godot `MeshInstance3D` with a flat unshaded material is fine; the visual polish iteration is post-M0.

The existing freeform interaction (`ThreeDeePaperSelector`, `StartPointSelectionState`, etc.) is **not** active during a template session. Introduce a "session mode" notion: either a `TemplateSession` is active (ghost-click input) or freeform is active (today's behaviour). For M0, the freeform path stays as the only way to enter without a template — its UI gating is part of #05.

## Acceptance criteria

- [ ] A `TemplateSession` class (or equivalent) holds: current template, current step index, current paper choice, and progresses on each completed fold.
- [ ] A ghost renderer draws the next step's crease line on the paper, anchored to the current `Frame` state via the feature resolver from #03.
- [ ] Mouse click on/near the ghost line triggers the corresponding fold step and the ghost moves to the next step.
- [ ] Clicks elsewhere are ignored; clicks during animation are ignored.
- [ ] On the final step, a `TemplateCompletedEvent` is emitted with template id and paper id.
- [ ] Unit tests around hit-testing the click-to-crease proximity check.
- [ ] All existing tests still pass.
- [ ] Manual editor check: launching a boat session (debug keybind is acceptable for this slice — UI entry comes in #05) lets the player fold the boat by clicking ghosts.

## Blocked by

- #03 — needs the boat sequence and feature resolver.
