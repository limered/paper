# ADR-0003 — Valley folds invert moved-stack layer order

Status: Accepted
Date: 2026-06-14

Supersedes: ADR-0002 §"Layer-update rule on fold" only. Other sections of ADR-0002 (layer-state in `Frame3D`, face-based picking contract, alternatives considered) remain in force.

## Context

ADR-0002 introduced a per-face layer map and a fold-time update rule:

> `bump = maxStationary + 1 - minMoved`, applied uniformly to every face in `M`.

The "applied uniformly" preserves relative layer ordering within the moving stack `M`. ADR-0002's alternatives section explicitly rejected adding inversion, on the reasoning that "inversion is a sidedness/orientation concern that only matters once mountain folds and paper sidedness arrive."

This was wrong. Within hours of the implementation landing, the player hit a poke-through bug (see `.scratch/screens/poke_through_bug.png` and `poke_through_bug2.png`):

- Fold 1 brings a corner over, creating a small triangle on top of a larger face. Both faces are now in a stack of two; in layer terms `[BIG=0, SMALL=1]`.
- Fold 2's moving set `M` contains both `BIG` and `SMALL`. After the 180° rotation, the moved stack lands on the stationary paper.
- The current rule places both faces at uniform offset: `[stationary=0, BIG=1, SMALL=2]`. `SMALL` stays "above" `BIG`.
- But physically, a 180° rotation around a horizontal axis flips up/down. After the rotation, what was on top of `M` is now on the bottom, and vice versa. The correct ordering is `[stationary=0, SMALL=1, BIG=2]`.
- Rendering the wrong ordering puts `SMALL` higher in world Z than `BIG`, so it pokes out the far side of `BIG` instead of being tucked between `BIG` and the stationary paper.

The "inversion is sidedness/orientation" framing was the mistake. Sidedness (which face of the paper points up) is genuinely deferred to mountain folds. Stack inversion is a different concept — it follows purely from the geometry of a 180° rotation around a horizontal axis — and it applies to every valley fold of any stack with `|M| > 1`.

Constraints going in (unchanged from ADR-0002):

- Layer state lives in `Frame3D._layers`, derived per replay (β from ADR-0002).
- No per-face transforms.
- Replay loop in `PaperRenderer.RebuildFrame3D` stays.
- Mountain folds, paper sidedness, and partial-angle layer semantics remain out of scope.

## Decision

Replace ADR-0002's layer-update rule with:

For each replayed `ChangeRecord`, after BFS produces moving set `M` and rotation is applied:

1. Build `overlaps = { s ∈ Frame.Faces \ M : FacesOverlap(frame, s, m, postRotationVertices) for some m ∈ M }`. (Unchanged from ADR-0002.)
2. If `overlaps` is empty → no layer change.
3. Else, let:
   - `maxStationary = max(layers[s] for s in overlaps)`
   - `maxMoved = max(layers[f] for f in M)` *(now `max`, not `min`, and used differently)*
4. For each face `f ∈ M`, assign `newLayer(f) = maxStationary + 1 + (maxMoved - layers[f])`. This **inverts relative order within `M`** while placing the entire moved stack above the highest stationary face it overlaps.

When `|M| = 1`, `(maxMoved - layers[f]) = 0` for the single face, so the rule degenerates to `newLayer = maxStationary + 1` — matching ADR-0002's behaviour for the simple case.

The rule is correct at the **flat end state** of a fold (`TargetAngle = π`). For partial angles the rule still runs every frame but the result is only meaningfully "correct" at the end state. For v1 (per GDD) all folds are flat, so this is not a practical concern. If non-flat target angles arrive later, the rule may need a "skip layer updates when not flat" guard — flagged as a future concern, not a v1 problem.

## Consequences

- The poke-through bug is fixed for any multi-layer valley fold of an already-folded stack.
- The `LayerUpdater.ApplyLayerUpdate` implementation gains a per-face calculation instead of one uniform `bump`. Public signature unchanged.
- The ADR-0002 test `LayerUpdaterTests` scenario "relative-ordering preservation" must be replaced by "relative-ordering inversion".
- Mountain folds, when they arrive, will use the same inversion logic with the stack landing *below* the stationary set instead of above. A future mountain-folds ADR will spell this out and likely refactor the rule to take a `FoldDirection` parameter.

## Alternatives considered

**Skip inversion at partial angles, apply only at full flat.** Tempting because layer order is only physically meaningful when faces are coplanar. Rejected: introduces a discontinuity at the end of the animation and the inverted layers are harmless during the animated transit (faces are tilted, so the per-layer Z-nudge doesn't matter). Keep it simple and always-on.

**Per-face overlap target** (each `f ∈ M` is placed above the specific stationary faces *it* overlaps, not the global max). More physically honest for partial overlaps but introduces non-uniform spacing within `M` and risks intersecting layer numbers when `M`'s faces overlap different stationary stacks. Rejected for the same simplicity reasons ADR-0002 cited — keep `M` translated as a rigid block, just invert it.

**Amend ADR-0002 in place.** Per `docs/adr/README.md`, ADRs are append-only. The repo convention is to write a superseding ADR.
