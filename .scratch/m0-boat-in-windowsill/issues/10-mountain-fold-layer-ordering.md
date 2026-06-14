# 10 — Mountain-fold layer ordering

Status: done

## Parent

`docs/adr/0003-valley-fold-inverts-moved-stack.md` (explicitly defers mountain fold layer behaviour) and issue `03-mountain-folds.md` (introduces `ChangeType.MountainFold` but does not change `LayerUpdater`).

## What to build

ADR-0003 specifies the layer-update rule for **valley** folds: the moved stack `M` is placed *above* the highest stationary face it overlaps, with internal order inverted (because a 180° rotation around a horizontal axis flips up/down).

Mountain folds rotate around the *same* horizontal axis in the *opposite* direction. The stack-internal inversion still applies (any 180° flip inverts), but the moved stack ends up *below* the stationary stack, not above.

Today, `LayerUpdater.ApplyLayerUpdate` is unaware of fold direction. After issue 03 lands, mountain folds will:

- Correctly mirror their geometry across the page plane (the renderer negates the angle).
- **Incorrectly** receive the valley layer rule — the moved stack will be placed *above* the stationary stack instead of below, producing poke-through bugs symmetric to the one ADR-0003 fixed.

Fix: extend `LayerUpdater.ApplyLayerUpdate` (or its caller in `PaperRenderer`) to consult `ChangeRecord.ChangeType` and apply a mirrored rule for `MountainFold`:

For each `f ∈ M` after a mountain fold:

```
minStationary = min(layers[s] for s in overlaps)
minMoved      = min(layers[f] for f in M)
newLayer(f)   = minStationary - 1 - (layers[f] - minMoved)
```

This places the entire moved stack *below* the lowest stationary face it overlaps and inverts the relative ordering inside `M` — symmetric to the valley rule.

Negative layer indices are acceptable; the renderer's per-layer Z-nudge already handles any integer (see commit `0db4e02`).

## Acceptance criteria

- [ ] `LayerUpdater.ApplyLayerUpdate` (or its caller) branches on `ChangeRecord.ChangeType` (or an equivalent direction signal).
- [ ] A mountain fold on a two-face stack `[BIG=0, SMALL=1]` produces final layers `[SMALL=-2, BIG=-1, stationary=0]` (or any sequence with the same relative ordering and the moved stack strictly below stationary).
- [ ] Unit test: mountain fold of a single moved face onto a single stationary face places the moved face strictly *below* the stationary face in layer order.
- [ ] Unit test: mountain fold of a two-face moved stack inverts the moved-stack ordering AND places the whole stack below the stationary stack — the mirror of the ADR-0003 valley test.
- [ ] All existing tests still pass — in particular ADR-0003's valley-fold layer tests must be untouched.
- [ ] Add a short ADR (ADR-0004) recording the mirrored rule and citing this issue.

## Blocked by

- #03 — mountain folds (`ChangeType.MountainFold` must exist).
