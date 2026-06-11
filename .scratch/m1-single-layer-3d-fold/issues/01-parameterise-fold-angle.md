# 01 — Parameterise fold angle

Status: done

## Parent

`docs/GDD.md` → Roadmap → M-1, Engine roadmap point 1 (parameterisation half).

## What to build

The rotation angle used by the 3D fold renderer must come from data on the fold itself, not a hardcoded `Math.PI`. Today every fold snaps to a fully flat 180° because the renderer's rotation helper is called with a constant. After this slice, the angle is read from a per-fold field (a new `TargetAngle` on `ChangeRecord`) populated by the fold interaction code.

This slice does **not** introduce animation. The fold still snaps to its target in a single frame. The purpose is to separate the "what angle should this fold end at" concern from the "interpolate over time" concern, so the next slice can layer animation cleanly on top.

End-to-end behaviour:

- A fold triggered via the existing interaction continues to look identical (target is `Math.PI`, snap is instant).
- A unit test can construct a `ChangeRecord` with `TargetAngle = π/2` and assert that the rendered vertex positions land at the corresponding partially-folded location.
- The renderer no longer contains a hardcoded `Math.PI` for fold rotation.

Vertical because it touches: `ChangeRecord` (new `TargetAngle` field), `FoldInteractionApplier` (writes the default `Math.PI`), `PaperRenderer` (reads it; rename `ReflectedAroundLine` to something angle-parameterised such as `RotatedAroundEdge`), tests.

## Acceptance criteria

- [ ] `ChangeRecord` has a `float TargetAngle` field with a sensible default (`Math.PI`).
- [ ] `FoldInteractionApplier.ApplyVertexValleyFold` populates `TargetAngle` on the `ChangeRecord` it creates.
- [ ] `PaperRenderer.OnPaperFolded` reads `TargetAngle` from each `ChangeRecord` instead of using `Math.PI`.
- [ ] `ReflectedAroundLine` is renamed (e.g. `RotatedAroundEdge`) and takes an explicit `float angle` parameter.
- [ ] Unit test: rotating a vertex around a fold axis with `angle = π` matches the previous snap behaviour bit-for-bit.
- [ ] Unit test: rotating a vertex around a fold axis with `angle = π/2` lands at the geometrically correct partially-folded position (above the original plane, perpendicular distance preserved).
- [ ] Existing tests still pass: `dotnet test valleyfold/valleyfold.sln`.
- [ ] Manual check in the Godot editor: triggering a fold via the existing UI behaves visually identically to today.

## Blocked by

None — can start immediately.
