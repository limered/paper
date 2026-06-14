# 03 — Boat as scripted fold sequence + auto-play debug

Status: ready-for-afk

## Parent

`docs/GDD.md` → Roadmap M0 (the one template is the boat) and Engine roadmap point 4 ("template format"). This slice intentionally **defers** the data-driven template format to MVP — the boat is hardcoded in C# now to unblock #04+.

## What to build

A `BoatTemplate` class (under `valleyfold/src/Templates/` or similar) exposing:

- The required paper shape (square — assert this matches `Frame`'s initial state).
- An ordered list of fold steps. Each step has: fold type (V2V/V2E/E2E from #01's engine), mountain-or-valley (from #02), endpoints expressed in paper-local coordinates that resolve to actual `Frame` features at the time the step runs, and a target angle (usually `π`, but the template can choose).
- A way to resolve "the vertex/edge at paper-local position (x, y) on the current `Frame`" — needed because folding mutates the graph, so endpoint references can't be `Vertex.Id`s baked in advance.

Plus an **auto-play debug command**: a temporary keybind that walks the boat sequence end to end, applying each step via `FoldInteractionApplier` and `FoldAnimator`, waiting for animation completion between steps. No ghost rendering, no click input, no UI — just "press F10, watch the boat fold itself in the editor". This is the test harness for #01/#02/this slice.

The boat's fold sequence itself is the author's choice — a canonical traditional boat fold is fine. Document the chosen sequence in a comment.

## Acceptance criteria

- [ ] `BoatTemplate` (or equivalently named) class exists with the boat's fold sequence hardcoded.
- [ ] A position-based feature resolver maps "vertex near paper-local (x, y)" to the current matching `Vertex` in `Frame` after prior folds.
- [ ] Debug keybind triggers auto-play; pressing it once runs the whole sequence without further input.
- [ ] Auto-play waits for `FoldAnimator.IsAnimating` to be false between steps; does not stack folds.
- [ ] At the end of auto-play, the rendered geometry visibly resembles a paper boat.
- [ ] Unit tests around the feature resolver (given a `Frame` in a known state, "vertex at (0,0)" returns the expected `Vertex`).
- [ ] No new tests required for the boat sequence itself — visual editor check is acceptable per `AGENTS.md`.
- [ ] All existing tests still pass.
- [ ] Manual editor check: auto-play produces a recognisable boat.

## Blocked by

- #01 — needs multi-layer folding.
- #02 — needs mountain folds (the boat uses at least one).
