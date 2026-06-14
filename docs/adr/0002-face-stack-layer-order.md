# ADR-0002 — Face-stack layer order and face-based picking

Status: Accepted (§"Layer-update rule on fold" superseded by ADR-0003)
Date: 2026-06-14

## Context

ADR-0001 introduced 2D face-graph BFS for "which faces participate in a fold" and explicitly deferred layer order, picking, and any cross-layer reasoning. Within hours of landing it, two bugs surfaced when folding already-folded paper:

1. **Phantom creases on under-layers.** `FoldInteractionApplier.ApplyVertexValleyFold` (`valleyfold/src/Folding/FoldInteractionApplier.cs:33`) iterates every edge in `frame.Edges` for the crossing test. The 2D graph has no notion of layers, so the fold line splits edges on faces the player wasn't folding. The renderer (per ADR-0001) correctly rotates only the participating stack, but the splits have already happened — leaving stale crease segments on the underlayer.
2. **Picking the wrong vertex on a stack.** The vertex-drag input model resolves screen click → vertex `Id`. When two 2D-distinct vertices project to the same XZ after a fold, the picker can resolve to an underlayer vertex. The renderer's anchor face (any face containing `PickedVertex`) then BFS's from the wrong layer, and the rotation moves the bottom paper instead of the top.

Both bugs are different surfaces of the same missing concept: the engine has no representation of stacking order, and the interaction code has no way to express "I meant the top layer".

Constraints going in:

- The vertex-drag interaction is a legacy of the previous game design. The new player input is a **ghost crease** — the engine presents a highlighted edge, the player clicks it, the engine derives everything else. Face-picking is the natural input granularity for the new model and removes the stack-ambiguity at the input boundary.
- 3D raycast picking against `FaceRendering`'s mesh instances returns the visually topmost face for free. Layer order is not needed to *resolve* a click; it is needed for engine-side reasoning about *where folded paper lands* and for *coplanar render disambiguation* (z-fighting).
- ADR-0001's replay model is in force. Per-face transforms remain explicitly rejected; this ADR augments that decision rather than superseding it.
- Mountain folds, paper sidedness ("which face points up"), and the "valley fold inverts the moved stack" question are out of scope. This ADR specifies stacking *order*, not orientation.

## Decision

Two coupled decisions.

### 1. Layer order representation

Each face has a notional integer **layer**. Higher integer = higher in the stack. Layers are **derived state**, not stored on `Face`:

- A new `Dictionary<Id, int> layers` (or equivalent) is built fresh by `PaperRenderer.RebuildFrame3D` on every replay, alongside the existing `Frame3D.Vertices` rebuild.
- Faces start at layer `0` on `RebuildFrame3D` entry. The replay loop updates layers per the rule below as each `ChangeRecord` is applied.
- A small accessor (`int LayerOf(Face)` on whatever class owns the dictionary — likely `Frame3D`) is the public read interface. `Face` itself stays clean of derived state.

Reasoning: the rest of the engine treats `Frame` as the authoritative 2D graph; mutating a `Face` field during render mixes two lifecycles. Building the layer map during replay matches the spirit of ADR-0001 (`Frame3D` is the place for replay-derived state).

### 2. Layer-update rule on fold (valley folds only)

For each `ChangeRecord` during replay, after the BFS-determined moving set `M` is rotated:

1. For each face `f ∈ M`, compute `f`'s post-rotation 2D footprint by projecting its rotated 3D vertices to XY.
2. `overlaps(f) = { s ∈ Frame.Faces \ M : 2D-polygon-intersection-area(f, s) > 0 }`. **Touching only at an edge or vertex does not count.** Only positive-area interior intersection counts as overlap.
3. If `overlaps(f)` is non-empty for any `f ∈ M`:
   - `maxStationary = max(layers[s] for s in ⋃ overlaps(f))`
   - `minMoved = min(layers[f] for f in M)`
   - `bump = maxStationary + 1 - minMoved`
   - Apply the same `bump` to every face in `M`. This preserves relative order within `M` while placing the entire moved stack above the highest stationary face it overlaps.
4. If `overlaps(f)` is empty for every `f ∈ M` (moved off-paper or onto only its own prior stack), no layer change.

A new primitive is needed: `FacesOverlap(Frame, Face a, Face b, IReadOnlyList<Vector3> postRotationVertices)` returning a bool for positive-area intersection. Lives on `FaceQueries`. Implementation may use Godot's `Geometry2D.IntersectPolygons` for expedience or be hand-rolled; either is acceptable but the function must be testable from xUnit without a scene tree.

### 3. Face-based picking contract

`FoldInteractionApplier` switches from `(Id pickedVertex, Vector3 endPoint)` to `(Face pickedFace, Edge ghostCrease)`. The interaction code:

- Anchors the BFS at the supplied `pickedFace` directly — no more "any face containing `PickedVertex`" guess.
- Restricts the edge-crossing scan in `ApplyVertexValleyFold` to **edges of faces in the moving stack only**. This requires the BFS to run *before* the splitting loop, using the ghost crease's geometry (not yet inserted as a `frame.Edges` entry) as the blocking line. The straightforward formulation: compute the participating face set provisionally by "faces on the picked-face side of the ghost crease line, walked via face adjacency from `pickedFace`, refusing to cross any edge that the ghost crease line would cross", then only split edges belonging to those faces.

`ChangeRecord` gains `Id PickedFace`. `PickedVertex` is retained as informational for now (existing code paths read it; ADR-0001's anchor logic is superseded by `PickedFace` in the new renderer path) and may be removed in a follow-up cleanup once nothing consumes it.

### Replay invariant (extends ADR-0001's invariants)

- Layer state is a pure function of `(Frame, ChangeMemory)`. Replaying the same change history produces the same `layers` map.
- Layer updates during replay use the **face graph and overlap geometry as they exist at the moment of that change's reapplication** — same principle as ADR-0001 for the rotation axis.

## Consequences

What becomes possible:

- The two bugs above are addressable. Bug 1 is fixed by the interaction-side BFS + edge-restriction. Bug 2 is fixed by the input-contract change (face-picking) and the renderer reading `PickedFace` directly.
- The boat sequence (which folds non-graph-adjacent flaps onto the same surface) can correctly compute "which flap is on top" without any per-face transform machinery.
- Renderer can apply a per-face Z-nudge (`normal * layer * epsilon`) to break coplanar z-fighting. Exact epsilon is a render-side tactical choice, not an ADR concern.
- The ghost-crease UI has a clean engine contract to integrate against: it picks a face, it picks an edge, the engine does the rest.

What is harder / what we deliberately rule out:

- **Mountain folds, paper sidedness, and "valley fold inverts the moved stack"** remain deferred. The layer-update rule above is *stacking order only*; it does not track which face of the paper points up, and it does not invert the moved stack's internal order. Mountain folds will require both (a separate ADR).
- **Partial overlaps** (a moved face that overlaps two stationary faces at different layers) are handled by "above the highest stationary overlap". This may produce surprising stack orderings for pathological geometries but is correct for all M0 templates. A future ADR can refine if needed.
- **The 2D polygon-intersection-area test** has well-known numerical edge cases (collinear segments, vertex-on-edge). The implementation must pick a tolerance and document it; this ADR does not prescribe the number.
- **`FoldInteractionApplier`'s vertex-based public method is going away.** Callers (including the current input layer and any tests) must migrate to the face-based signature.

Performance: layer-update adds an `O(|M| · |Frame.Faces \ M|)` polygon-intersection pass per change per frame. For M0/MVP paper sizes this is well within budget; not worth optimising preemptively.

## Alternatives considered

**`Face.Layer` field on the domain object.** Mutating `Face` during `RebuildFrame3D` mixes domain state with replay-derived state and conflicts with how ADR-0001 treats `Frame3D` as the replay-side mirror. Rejected — though it remains a viable fallback if the dictionary indirection proves awkward.

**DAG of "above" relationships, derived from fold history.** More honest about partial orders (non-overlapping faces have no defined relation), avoids integer arithmetic. Rejected for M0: overkill for the boat's depth, and integer layer + per-pair overlap test gives the same answers for the cases we have.

**No stored layer state; recompute on demand from `ChangeMemory`.** The purist's "zero new state" option. Rejected: every query walks history, and any caching reintroduces the dictionary anyway.

**Layer state in `Frame3D` versus `Frame`.** Settled on `Frame3D` (or a new collaborator owned by the renderer). `Frame` stays the pure 2D authoritative graph.

**Keep vertex-based picking; resolve stack ambiguity in the picker.** Rejected: vertices have no surface and no unambiguous topmost. Face-picking via 3D raycast is the natural fix and aligns with the ghost-crease input model the game is moving toward.

**Spell out "valley fold inverts the moved stack" in this ADR.** Rejected: inversion is a sidedness/orientation concern that only matters once mountain folds and paper sidedness arrive. Bundling it here would force premature decisions on those. The pure stacking-order rule above is sufficient for valley-only M0.
