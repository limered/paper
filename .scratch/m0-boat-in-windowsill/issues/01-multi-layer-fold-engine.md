# 01 — Multi-layer 3D fold engine

Status: ready-for-afk

## Parent

`docs/GDD.md` → Engine roadmap point 2 ("face-stacking / layer order"). Blocks every subsequent M0 slice.

Design captured in `docs/adr/0001-multi-layer-fold-via-face-graph-traversal.md` (Accepted).

## What to build

Today `PaperRenderer.ApplyFoldRotation` (`valleyfold/src/Render/ThreeDee/PaperRenderer.cs:79`) decides which vertices a fold rotates by asking "are they on the picked-vertex side of the fold line in 2D (XZ projection)?". That rule has no concept of face layers and breaks the moment paper is folded on top of paper.

Per ADR-0001, replace it with a **BFS over the 2D `Frame` face graph**:

- Anchor: any face in `frame.Faces` containing `change.PickedVertex`.
- Blocking edge set: `change.AddedEdges` (already maintained — these are the crease edges, kept up to date by `ChangeMemory.AddEdgeToExistingChange` when later folds subdivide a crease).
- Adjacency primitive: extend `FaceQueries` (e.g. `FacesAdjacentToFace`) reusing `FaceQueries.FacesAdjacentToEdge` (`valleyfold/src/FrameModifications/FaceQueries.cs:10`) for the per-edge step.
- Reachable set = the participating faces. A vertex rotates iff it belongs to at least one reachable face.

The replay loop in `RebuildFrame3D` stays. No `ChangeRecord` schema change. No per-face transform state. `Frame3D.Vertices.Count == Frame.Vertices.Count` invariant unchanged. The animator integration is unchanged — the in-flight change is rotated at `EasedProgress * TargetAngle` exactly as today, the only difference is *which* vertices it rotates.

## Acceptance criteria

- [ ] New method on `FaceQueries` (suggested: `Face[] FacesReachableFrom(Frame frame, Face anchor, ISet<Id> blockingEdgeIds)`), pure over `Frame`, no Godot dependencies, no `Statics` reads.
- [ ] `PaperRenderer.ApplyFoldRotation` no longer calls `FoldMath.AreOnSameSideOfLine`. The moving set is built from `FacesReachableFrom`, with the anchor chosen as any face containing `change.PickedVertex` and blocking set `change.AddedEdges`.
- [ ] Unit tests for `FacesReachableFrom`:
  - On an unfolded square split by a single diagonal crease, BFS from the face on one side returns exactly that face.
  - On a square split by two perpendicular creases (four faces), BFS from one quadrant blocked by one of the creases returns the two faces on its side of that crease (i.e. the BFS *can* cross the other, non-blocking crease).
  - Anchor-face independence: starting from any face containing a given picked vertex returns the same reachable set.
- [ ] Existing tests still pass: `dotnet test valleyfold/valleyfold.sln`.
- [ ] Manual editor check: a fold on the unfolded square looks identical to today. A second fold applied to already-folded paper rotates only the participating faces (verifiable visually by folding the same diagonal twice — the second fold should bring the top layer back instead of moving both layers as one).

## Out of scope (per ADR-0001)

- Click-picking a specific face out of a stack. Input still hands `FoldInteractionApplier` a `PickedVertex`; resolving stacks is a separate ADR (blocks M0 boat completion but not this slice).
- Mountain folds and non-flat target angles — issue 02.
- Per-face accumulated transform stack — explicitly rejected in ADR-0001 alternatives section. Do not introduce.

## Blocked by

None — must land first.
