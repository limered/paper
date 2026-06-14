# ADR-0004 — Mountain folds invert moved stack BELOW stationary

Status: Accepted
Date: 2026-06-15

Supersedes: nothing (extends ADR-0003 to the mountain-fold direction). ADR-0003's valley-fold rule remains in force unchanged.

## Context

ADR-0003 specified the layer-update rule for valley folds:

> `newLayer(f) = maxStationary + 1 + (maxMoved - layers[f])`

— inverts the relative ordering within the moving set `M`, and places the whole moved stack above the highest overlapping stationary face.

Issue 03 (`m0-boat-in-windowsill/issues/03-mountain-folds.md`) introduced `ChangeType.MountainFold`. Mountain folds rotate around the same crease axis in the opposite direction. The geometric rotation is handled by the renderer (`FoldRotation.SignedAngle` negates for mountain), but ADR-0003 explicitly deferred the layer-update side: it called out that "mountain folds, paper sidedness, and partial-angle layer semantics remain out of scope."

The unfixed layer-update rule would apply the valley rule to mountain folds, placing the moved stack ABOVE the stationary stack — physically wrong. A mountain fold rotates the moved corner DOWN, so the moved stack lands underneath the stationary paper, not on top of it. Without this fix, mountain folds would reproduce the symmetric mirror of the poke-through bug ADR-0003 was created to fix.

## Decision

`LayerUpdater.ApplyLayerUpdate` now takes a `ChangeType foldType` parameter (defaulted to `ChangeType.ValleyFold` to keep existing call sites and tests untouched). For each replayed `ChangeRecord` the renderer passes `change.ChangeType`.

For `ChangeType.MountainFold`, apply the mirror rule:

1. Build `overlaps` exactly as for valley folds (faces in `Frame.Faces \ M` that overlap some `m ∈ M` post-rotation).
2. If `overlaps` is empty → no layer change.
3. Else, let:
   - `minStationary = min(layers[s] for s in overlaps)`
   - `minMoved = min(layers[f] for f in M)`
4. For each `f ∈ M`, assign `newLayer(f) = minStationary - 1 - (layers[f] - minMoved)`.

This places the whole moved stack BELOW the lowest overlapping stationary face and inverts the relative ordering inside `M` — both symmetric to the ADR-0003 valley rule. When `|M| = 1` the rule degenerates to `newLayer = minStationary - 1`.

Negative layer indices are valid. The renderer's per-layer Z-nudge (see commit `0db4e02`) already handles any integer offset; no further changes were needed.

## Consequences

- A mountain fold of a two-face moved stack `[BIG=0, SMALL=1]` onto a stationary face at layer 0 produces `[SMALL=-2, BIG=-1, stationary=0]`. The face that was on top of the moved stack ends up at the bottom (geometrically correct for a 180° flip) and the whole stack sits beneath the stationary paper.
- ADR-0003's valley tests are untouched — the default parameter preserves binary and source compatibility.
- The `LayerUpdater` signature now leaks the fold-direction concept. This is fine: the layer rule is direction-dependent by nature.
- The "partial-angle" caveat from ADR-0003 carries over: the rule is only meaningfully correct at the flat end-state. For v1 all folds are flat, so this is not a practical concern.

## Alternatives considered

- **Branch in the renderer, keep two `ApplyLayerUpdate` functions.** Rejected — the rule is data-driven by the change record; carrying that choice into the renderer would spread the concept across two files.
- **Use a `bool inverted` parameter instead of `ChangeType`.** Rejected — `ChangeType` is the source of truth for fold direction; a bool would force every caller to repeat the same `change.ChangeType == MountainFold` derivation.
- **Skip the layer update for mountain folds entirely.** Rejected — that would leave the moved stack at its original layer, producing visible poke-through whenever a mountain fold lands on an already-folded stack (the mirror of the bug ADR-0003 fixed).
