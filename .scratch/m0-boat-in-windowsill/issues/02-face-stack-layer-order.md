# 02 — Face-stack layer order and face-based picking

Status: ready-for-afk

## Parent

`docs/GDD.md` → Engine roadmap point 2 ("face-stacking / layer order"). Blocks `03-mountain-folds`, `04-boat-scripted-sequence`, and `05-ghost-crease-click`.

Design captured in `docs/adr/0002-face-stack-layer-order.md` (Accepted). This issue implements the ADR end-to-end.

## What to build

After ADR-0001 landed, two bugs surfaced when folding folded paper:

1. **Phantom creases on under-layers** — `FoldInteractionApplier.ApplyVertexValleyFold` (`valleyfold/src/Folding/FoldInteractionApplier.cs:33`) iterates *all* `frame.Edges` and splits any it crosses, regardless of which layer the player meant to fold. Reference render: `.scratch/screens/render_bug.png`.
2. **Wrong vertex picked from a stack** — vertex-drag picking can resolve to an underlayer vertex, so the renderer's "any face containing `PickedVertex`" anchor BFS's from the wrong layer.

The ADR fixes both via face-based picking + a derived layer-order map + an interaction-side BFS that restricts edge splits to the moving stack. Read the ADR before starting; this issue does not duplicate the rationale.

## Concrete changes expected

### A. Layer-order derived state

1. Owner: extend `valleyfold/src/ThreeDeeModels/Frame3D.cs` with a layer map. Suggested:
   ```csharp
   private readonly Dictionary<Id, int> _layers = new();
   public int LayerOf(Face face) => _layers.TryGetValue(face.Id, out var l) ? l : 0;
   internal void ResetLayers(Frame frame);            // all faces → 0
   internal void BumpLayers(IEnumerable<Face> moved, int bump);
   ```
2. `PaperRenderer.RebuildFrame3D` calls `ResetLayers` at the start of each replay, then `BumpLayers` once per `ChangeRecord` per the rule below.

### B. Layer-update rule per replayed `ChangeRecord`

After ADR-0001's BFS produces moving set `M` and the rotation is applied to `Frame3D.Vertices`:

1. Build `overlaps = { s ∈ Frame.Faces \ M : FacesOverlap(frame, s, m, frame3d.Vertices) for some m ∈ M }`.
2. If `overlaps` is empty → no layer change for this record.
3. Else:
   - `maxStationary = overlaps.Max(s => frame3d.LayerOf(s))`
   - `minMoved = M.Min(f => frame3d.LayerOf(f))`
   - `bump = maxStationary + 1 - minMoved`
   - `BumpLayers(M, bump)`.

### C. New `FaceQueries` primitive

```csharp
public static bool FacesOverlap(
    Frame frame,
    Face a,
    Face b,
    IReadOnlyList<Vector3> vertexPositions3D)
```

Returns true iff the 2D XY projections of `a` and `b` (using `vertexPositions3D` for vertex coordinates) have positive-area intersection. Touching edges / shared vertices alone return false. Use `Godot.Geometry2D.IntersectPolygons` if convenient; if used, ensure the test still runs without a scene tree (it does — `Geometry2D` is static and headless-safe).

Document the tolerance chosen for "positive area" in the method's XML doc.

### D. Face-based picking contract

1. `ChangeRecord` gains `Id PickedFace` (init-only, default `-1` for back-compat in tests).
2. New entry point on `FoldInteractionApplier`:
   ```csharp
   public static void ApplyValleyFold(Face pickedFace, Edge ghostCrease);
   ```
   - Derives the fold-line 3D points from `ghostCrease`'s endpoint vertices via `frame3d.Vertices`.
   - Computes the participating face set by face-graph BFS anchored at `pickedFace`, **refusing to traverse any face-adjacency edge whose 2D segment is crossed by the ghost-crease line** (this is the pre-split blocking criterion).
   - Restricts the edge-crossing/splitting loop to edges of faces in the participating set.
   - Builds `ChangeRecord` with `PickedFace = pickedFace.Id` and (for now) `PickedVertex` populated from any vertex on the ghost crease (so existing readers don't NPE).
3. Existing `ApplyVertexValleyFold(Id startVertex, Vector3 endPoint)` stays for now (legacy input path); add an `[Obsolete]` attribute pointing at the new method. Removal happens when `05-ghost-crease-click` migrates the input layer.

### E. Renderer anchor uses `PickedFace`

In `PaperRenderer.ApplyFoldRotation`, when `change.PickedFace >= 0`, anchor the BFS at `frame.Faces[change.PickedFace]`. Fall back to the ADR-0001 "any face containing `PickedVertex`" behaviour when `PickedFace == -1` (legacy records).

### F. Z-offset for coplanar disambiguation

In `FaceRendering` (or wherever face positions are written), nudge each face by `face.Normal * frame3d.LayerOf(face) * epsilon`. Pick a small epsilon (e.g. `1e-4f`); tune by eye if necessary.

## Acceptance criteria

- [ ] `Frame3D` holds the derived layer map; `Face` has no `Layer` field.
- [ ] `FaceQueries.FacesOverlap(...)` exists and is unit-tested for: two coplanar overlapping squares → true; touching-edge-only → false; shared-vertex-only → false; disjoint → false; full containment → true.
- [ ] `PaperRenderer.RebuildFrame3D` applies the layer-update rule per ADR-0002 section "Layer-update rule on fold". Unit-tested via a pure helper (extract the update step into something callable without `_Process`).
- [ ] `ChangeRecord.PickedFace` added (init-only `Id`, default `-1`).
- [ ] `FoldInteractionApplier.ApplyValleyFold(Face, Edge)` exists; restricts splits to the moving stack; populates `PickedFace`. Old vertex-based entry point marked `[Obsolete]` but still functional.
- [ ] `PaperRenderer.ApplyFoldRotation` prefers `PickedFace` when set, falls back to picked-vertex anchor otherwise. Both paths covered by unit tests.
- [ ] Renderer applies per-face Z-nudge proportional to `LayerOf(face)`.
- [ ] Existing tests still pass: `dotnet test valleyfold/valleyfold.sln`.
- [ ] Manual editor check, in order:
  - Fold the unfolded square along a diagonal: visual identical to today.
  - Fold the top-right corner onto the bottom-left corner (creates a triangle), then fold the top layer back along a line parallel to the hypotenuse: only the top layer moves, **no phantom crease appears on the under-layer**, and the back-folded flap renders cleanly above the bottom layer (no z-fighting).
  - The scenario captured in `.scratch/screens/render_bug.png` no longer occurs.

## Out of scope (per ADR-0002)

- Mountain folds, paper sidedness, and stack inversion under valley folds. (Issue `03-mountain-folds`.)
- Removing `PickedVertex` from `ChangeRecord` entirely — cleanup deferred until callers migrate.
- Removing the legacy `ApplyVertexValleyFold` entry point — deferred until issue `05-ghost-crease-click` switches the input layer.
- Refining behaviour for pathological partial overlaps where the "above the highest overlap" rule gives unintuitive results — documented as known-acceptable for M0.

## Blocked by

`01-multi-layer-fold-engine` (done, committed `10ba858`).
