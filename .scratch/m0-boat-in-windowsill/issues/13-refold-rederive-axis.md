# 13 — Re-derive fold axis at replay time (drop `FoldLineA/B`)

Status: ready-for-afk

## Parent

Grilling session 2026-06-22. Reproduces an actual user-visible bug: *"Fold paper in half diagonally, then fold a flap. Unfold all. Refold the flap (allowed because it has no crossings). The model breaks."*

## Bug

`ChangeRecord.FoldLineA/FoldLineB` (`ChangeRecord.cs:29-30`) are `Vector3` world-space coordinates captured at fold-record creation time (`FoldInteractionApplier.cs:45,103-104,195-196,291-292`; `BoatTemplate.cs:102-103`). They freeze the **3D geometric context that was active when the fold was recorded** — including the rotations applied by any earlier fold.

`PaperRenderer.ApplyFoldRotation` (`PaperRenderer.cs:91-92`) uses these stored coordinates as the rotation axis at replay time. This is correct as long as the geometric context at replay matches the context at recording.

It does *not* match in this case:

1. Diagonal fold A. Crease α active. Paper folded in half.
2. Flap fold B inside one of A's triangles. B's crease β lies entirely within one face, so B does **not** split α — `WasSplitBy` does not link them. B records `FoldLineA/B` as 3D positions in the **diagonally-folded** configuration.
3. User unfolds A then B (reverse fold order). Paper flat.
4. Eligibility: `ChangesToRefold` returns B because `AllParentsFolded(B)` is trivially true (no `WasSplitBy` parent).
5. User refolds B. Renderer rotates around B's stored `FoldLineA/B` — coordinates from the folded-state geometry. But the paper is currently flat, so β's actual current position differs from those coordinates. The flap rotates around the wrong axis.

The "all parents folded" rule (`ChangeMemory.cs:77-83`) only catches **edge-splitting** dependency. It does not catch **geometric containment** — B sat on top of A's folded geometry without ever splitting A's edges.

## Fix (Option B: re-derive)

Per the grilling session, the chosen direction is **not** to broaden the dependency graph but to remove the frozen axis entirely. Origami is a continuous activity, not a replay tape: refolding B alone after unfolding A *should* produce a flap on flat paper.

- Drop `FoldLineA/B` from `ChangeRecord`.
- Derive the rotation axis at replay time from `AddedEdges[0]`'s vertices, looked up in the **current** `Frame3D.Vertices`. Because the replay loop processes changes in order and skips unfolded ones, the axis automatically reflects the current geometric context (whichever prior folds are currently folded have already moved the crease vertices to their current 3D positions; unfolded prior folds have been skipped, leaving those vertices at their flat positions).
- `AddedEdges[0]`'s endpoints lie on the fold axis by construction, and vertices on the fold axis are invariant under that fold's own rotation, so the lookup is order-independent within the in-flight change.

### Touchpoints

| File | Line | What |
|---|---|---|
| `ChangeRecord.cs` | 29-30 | Delete `FoldLineA`, `FoldLineB` fields |
| `FoldInteractionApplier.cs` | 103-104 | Delete writer (legacy path) |
| `FoldInteractionApplier.cs` | 291-292 | Delete writer (face-based path) |
| `BoatTemplate.cs` | 102-103 | Delete writer (template path) |
| `PaperRenderer.cs` | 87-92 | Rewrite `ApplyFoldRotation` to derive axis from `frame.Edges[change.AddedEdges[0]]` via `frame3d.Vertices[...].Coord` |
| `Testing/Folding/ApplyMountainFoldTests.cs` | 134, 136 | Update to derive axis the same way |

A small helper (private static in `PaperRenderer` or a new util) keeps the derivation in one place for the renderer and the test:

```csharp
static (Vector3 a, Vector3 b) CreaseAxis(ChangeRecord change, Frame frame, Frame3D frame3d)
{
    var edge = frame.Edges[change.AddedEdges[0]];
    return (frame3d.Vertices[edge.Vertices[0]].Coord,
            frame3d.Vertices[edge.Vertices[1]].Coord);
}
```

### Regression test

New file `Testing/Folding/RefoldGeometryTests.cs` reproducing the user's scenario:

- Build a `Frame` (unit square via `InitializePaper`).
- Apply a diagonal valley fold A (record into a local `ChangeMemory`).
- Apply a flap valley fold B inside one of A's triangles (record).
- Flip both records' `Unfolded = true`; set their `AddedEdges`' `Assignment = F` and `FoldAngle = 0`.
- Flip B's `Unfolded` back to `false` (simulating refold-in-progress).
- Build a `Frame3D`, replay equivalent of `PaperRenderer.EnsureFresh`.
- Assert the axis derived for B matches the **flat-paper** crease coords for B (i.e. equal to `(frame3d.Vertices[β₀], frame3d.Vertices[β₁])` with no prior rotations applied), not the diagonally-folded coords the old `FoldLineA/B` would have held.

This test fails under today's stored-axis implementation and passes under the re-derived one. It also locks against future "optimisations" that cache the axis at record time.

## Acceptance criteria

- [ ] `FoldLineA` and `FoldLineB` removed from `ChangeRecord`.
- [ ] All three writers (`FoldInteractionApplier` legacy + face-based, `BoatTemplate`) no longer set those fields.
- [ ] `PaperRenderer.ApplyFoldRotation` derives the axis from `AddedEdges[0]`'s vertices via `Frame3D.Vertices`.
- [ ] `ApplyMountainFoldTests.cs:134,136` migrated to the same derivation (extract a small helper used by both sites).
- [ ] New `Testing/Folding/RefoldGeometryTests.cs` containing the regression test described above.
- [ ] All existing tests still pass — in particular `ApplyValleyFoldTests`, `ApplyMountainFoldTests`, `FoldRotationTests`, and any layer-ordering tests must be unaffected.
- [ ] Manual editor check (user's exact scenario): fold paper diagonally, fold a flap inside one triangle, unfold both, refold the flap alone. The flap folds correctly around its current flat-paper crease, no model break.

## Blocked by

None. Compatible with #11 and #12 — order-independent.
