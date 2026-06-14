# ADR-0001 — Multi-layer fold via 2D face-graph traversal

Status: Accepted
Date: 2026-06-14

## Context

The 3D fold renderer (`PaperRenderer.RebuildFrame3D`, `valleyfold/src/Render/ThreeDee/PaperRenderer.cs:46`) decides which vertices to rotate for each fold using a 2D side-of-line test (`FoldMath.AreOnSameSideOfLine`, `PaperRenderer.cs:79`). The test asks "in the XZ projection, is this vertex on the same side of the fold line as the picked vertex?" and rotates the matching vertices around the stored 3D fold axis.

This rule is correct for the very first fold on flat paper. It is wrong for any fold applied to already-folded paper. After a prior fold has rotated half the sheet up and over, two faces that the player perceives as on opposite layers can sit at the same XZ position. The 2D side-test cannot distinguish them and rotates both, or neither, depending on which side of the new crease the projection lands on.

The GDD (`docs/GDD.md`, "Engine roadmap" point 2) names this as the rendering refactor blocking M0 (the first model — a boat — requires at least one fold of already-folded paper). It proposes "a face-graph traversal: starting from the face containing the picked vertex, walk the adjacency graph without crossing the new crease".

Constraints going in:

- The replay model is endorsed for M-1 by GDD decision Q12 and is in active use: `_Process` resets `Frame3D` from `Frame` each frame and reapplies every `ChangeRecord` in `ChangeMemory.Changes` order. The animator handles intermediate angles for the in-flight fold.
- `ChangeRecord` already carries `PickedVertex`, the 3D fold axis (`FoldLineA`/`FoldLineB`, captured at fold time), and `AddedEdges` — the crease edges produced by this fold (see `FoldInteractionApplier.cs:59–76`, where the inner loop pushes the crease edge id returned by `VertexToVertexFold.Apply` into `addedEdges`). `ChangeMemory.AddEdgeToExistingChange` keeps `AddedEdges` updated when a later fold subdivides this crease.
- `FaceQueries.FacesAdjacentToEdge` (`valleyfold/src/FrameModifications/FaceQueries.cs:10`) already returns the faces sharing a given edge — the natural BFS adjacency primitive.
- Layer order (which of two stacked faces the player clicked, render-order between coplanar layers) and mountain folds are listed as separate engine-roadmap items (#3) and a follow-up ADR. They are out of scope here.

## Decision

Replace the per-vertex 2D side-test with a participating-face-set computed by BFS over the 2D `Frame` face graph.

For each `ChangeRecord` replayed during `RebuildFrame3D`:

1. **Anchor face** — pick any face in `Frame.Faces` containing `ChangeRecord.PickedVertex`.
2. **Walk** — BFS from the anchor face over face adjacency (`FaceQueries.FacesAdjacentToEdge`), refusing to traverse any edge whose id is in `ChangeRecord.AddedEdges`. The reachable set is the participating face set.
3. **Rotate** — a vertex rotates iff it belongs to at least one face in the participating set. Apply `FoldMath.RotatedAroundEdge` around the stored `FoldLineA`/`FoldLineB` at the change's effective angle (full `TargetAngle` for completed folds, `EasedProgress * TargetAngle` for the in-flight fold, as today).

The BFS lives as a new method on `FaceQueries` (e.g. `FaceQueries.FacesReachableFrom(frame, anchorFace, blockingEdges)`). `PaperRenderer.ApplyFoldRotation` consumes it; the `AreOnSameSideOfLine` call at `PaperRenderer.cs:79` goes away.

No `ChangeRecord` schema change. No per-face transform state. The replay loop, the animator integration, and the `Frame3D.Vertices.Count == Frame.Vertices.Count` invariant all stay as they are.

### Invariants this decision relies on

- **`ChangeMemory` is append-only and replayed in insertion order.** The stored `FoldLineA`/`FoldLineB` were captured in post-prior-folds 3D space; replay only stays correct because the same prior changes have been reapplied in the same order before this one's turn.
- **`AddedEdges` is the full crease at replay time.** `ChangeMemory.AddEdgeToExistingChange` is the mechanism that maintains this when later folds subdivide a crease; any new mutating operation that splits an existing crease must continue to call it.
- **Anchor-face independence.** All faces incident to `PickedVertex` lie on the same side of the new crease, because the crease passes *through* `PickedVertex`. Therefore the BFS reachable set does not depend on which incident face is chosen as the anchor.

## Consequences

What becomes possible:

- Folding already-folded paper rotates the correct subset of faces, regardless of how the layers project in XZ. Unblocks M0.
- The "which faces participate?" question is answered in 2D, on the authoritative `Frame` graph, where adjacency is unambiguous. We never have to answer "in 3D, which side am I on?" — a question that has no good answer once paper folds back over itself.

What becomes harder / what we deliberately rule out:

- **Click-picking a specific face out of a stack** is not solved here. The renderer still receives a `PickedVertex` and must somehow be told *which* face the player meant. Until the layer-order ADR lands, picking will continue to use whatever the input layer hands `FoldInteractionApplier`. This is acceptable for M-1 (paper is flat) but the M0 boat will force a layer-order decision before completion.
- **Mountain folds and non-flat target angles applied to folded paper** will work kinematically under this decision (the rotation math is angle-agnostic). What they will *not* get is any concept of "which layer ends up on top" — that is the layer-order follow-up.
- **No per-face transform stack.** A future shift to per-face transforms (for picking, layer order, or non-manifold paper) is a separate decision and will replace this one if taken; this ADR should be marked superseded at that point rather than amended.

Performance: BFS per change per frame is `O(faces + edges)` per change; total `O(N · (faces + edges))` per frame for N changes. For M0/MVP paper sizes (tens of faces, tens of changes) this is negligible and not worth optimising.

## Alternatives considered

**(ii) Per-face accumulated transform stack.** Each face carries a composed `Transform3D` built by walking `ChangeMemory` once and composing the rotation of each fold whose participating set included that face. Rejected for now: identical kinematic output to the chosen decision, but introduces a new piece of state (per-face transform) and an invariant ("adjacent faces only differ in transform if separated by a current fold line") that is easy to violate when mountain folds and non-flat angles arrive. Worth revisiting if and when layer order or picking forces it. If adopted later, this ADR is superseded, not amended.

**(iii) Per-face-corner vertex cloud.** `Frame3D.Vertices` no longer mirrors `Frame.Vertices` 1:1; each face stores its own 3D corner positions, transformed by that face's stack. Rejected: solves problems explicitly out of scope here (non-manifold paper, divergent layer transforms) at the cost of a representational shift that ripples through every renderer and query. YAGNI for M0.

**Stay with the 2D side-test and patch the picking layer instead.** Rejected: the side-test is fundamentally 2D and the bug is fundamentally 3D-layered. No amount of picking smartness fixes a renderer that will rotate the wrong faces once told the right picked vertex.
