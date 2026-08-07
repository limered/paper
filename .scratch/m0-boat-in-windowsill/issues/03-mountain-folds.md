# 03 — Mountain folds

Status: done

## Parent

`docs/GDD.md` → Engine roadmap point 3 ("mountain folds"). Required by M0; the boat uses at least one mountain fold.

## What to build

Today the engine only knows valley folds: `ChangeType.ValleyFold` and a positive `TargetAngle` (rotation up out of the page). A mountain fold is geometrically a valley fold with the opposite rotation direction.

Two viable representations — pick one in the implementation and document why in the commit message:

- **Option A**: add `ChangeType.MountainFold` as a distinct enum value; the renderer negates the angle internally for mountain.
- **Option B**: keep one fold type and let `TargetAngle` carry the sign (positive = valley, negative = mountain).

Either way, the fold interaction API and the (forthcoming, see #03) template format must be able to express "mountain fold from A to B".

## Acceptance criteria

- [ ] The engine can produce a mountain fold that visibly rotates the moving faces in the opposite direction to a valley fold of the same crease.
- [ ] `FoldInteractionApplier` exposes a way to apply a mountain fold (`ApplyVertexMountainFold` or an existing method with a fold-direction parameter).
- [ ] The chosen representation (Option A or B above) is consistent across `ChangeRecord`, `FoldAnimator`, and the renderer.
- [ ] Unit test: a mountain fold applied to a vertex on the unfolded square lands the rotated vertex below the page plane (negative Z if valley goes positive Z, or vice versa — match the existing convention).
- [ ] Unit test: a mountain fold and a valley fold of the same crease produce vertex positions that are mirror images across the page plane.
- [ ] All existing tests still pass.
- [ ] Manual editor check: triggering a mountain fold via temporary debug input (a debug key is acceptable; permanent UI is not in scope here) animates downward.

## Blocked by

- #01 — face-graph traversal must already determine the moving set; mountain folds re-use that same set.
