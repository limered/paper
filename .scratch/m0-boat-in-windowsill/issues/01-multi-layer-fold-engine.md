# 01 — Multi-layer 3D fold engine

Status: ready-for-human

## Parent

`docs/GDD.md` → Engine roadmap point 2 ("face-stacking / layer order"). Blocks every subsequent M0 slice.

## What to build

Today `PaperRenderer` decides which vertices a fold rotates by asking "are they on the picked-vertex side of the fold line in 3D?". That rule has no concept of face layers and breaks the moment paper is folded on top of paper, because vertices from previously-folded layers can land on either side of a new crease purely by coincidence of where they were tweened to.

Replace that rule with a **face-graph traversal**:

- Each `Face` in `Frame` already has identity. Treat the faces as nodes in a graph; two faces are adjacent iff they share an `Edge`.
- For a fold along edge `E` triggered with picked vertex `V`, the moving set is the connected component of faces reachable from the face containing `V` **without crossing `E` or any of the new crease's collinear segments**.
- Per-face transforms accumulate fold history. The rendered position of any vertex is its rest position transformed by the composition of every fold transform applied to its face's stack, in order.

Per GDD decision Q12, this is the refactor we deferred from M-1. Now is the time. The replay-all-changes loop in `PaperRenderer._Process` becomes "replay all changes into per-face transform stacks, then position vertices from face transforms" rather than "rotate vertices one change at a time, filtering by 3D side".

This unlocks the boat (which folds already-folded paper) and is the foundation for mountain folds (#02) and the template player (#03).

## Why HITL

The data model and algorithm choices here are load-bearing for the rest of the game and deserve an ADR. Expected discussions:

- Where do per-face transforms live? On `Face` directly, on `Frame3D`, or in a new `FoldState` class consumed by the renderer?
- Does the moving set include the face containing the picked vertex, or only faces strictly on its side of every prior fold? (Convention question — pick one and document.)
- How is the in-flight (animating) fold represented in this model? A single "preview transform" applied on top of the stable per-face stacks?
- What does mountain-vs-valley mean once we have layered faces? (Affects #02.)
- How do we test this without standing up the Godot scene? Likely a `FrameRenderState` or similar pure-data class so xUnit can assert face transforms.

Write an ADR under `docs/adr/` before significant code lands.

## Acceptance criteria

- [ ] ADR published under `docs/adr/0001-multi-layer-fold-rendering.md` (or next free number) documenting: face-graph traversal rule, per-face transform composition, in-flight fold representation, test seam.
- [ ] `PaperRenderer` no longer uses the "vertices on picked side in 3D" rule. The moving set is determined from the `Frame` face graph.
- [ ] Per-face transforms (or equivalent) are computed in a pure-data class testable without Godot.
- [ ] Unit tests for the traversal: single fold of unfolded paper picks exactly the correct half; a second fold of the now-folded paper picks only the faces in the picked-vertex's stack on the correct side.
- [ ] Existing M-1 single-fold animation still works (`FoldAnimator` keeps driving in-flight progress; the renderer just consumes it differently).
- [ ] All existing tests still pass: `dotnet test valleyfold/valleyfold.sln`.
- [ ] Manual editor check: a vertex-to-vertex fold on the unfolded square looks identical to today; a second fold applied via interaction on the already-folded paper now folds only the top layers as expected.

## Blocked by

None — must land first.
