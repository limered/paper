# 12 — `FoldAnimator.Cancel` on Reset Paper

Status: ready-for-afk

## Parent

Grilling session 2026-06-22 on refold state restoration. Closure-lifetime bug across all three animation flows (fold, unfold, refold), most explicit in the refold callback.

## What to build

`Game.ResetPaper` (`valleyfold/src/Game.cs:33`) creates a new `Frame`, clears `ChangeMemory`, and emits `PaperFoldedEvent`. It **does not** touch `Statics.FoldAnimator`. If the user hits Reset mid-animation:

1. `FoldAnimator._change` still points at a now-orphaned `ChangeRecord`.
2. `ChangeMemory._changes` is empty; the orphan is unreachable through normal queries.
3. `Frame` is replaced with a fresh one; edge IDs from the orphan no longer mean what they did.
4. `FoldAnimator.Tick` continues counting and eventually fires the completion callback.
5. For refold: the callback (`RefoldSelectionState.cs:38-47`) iterates `change.AddedEdges` and writes to `ctx.Frame.Edges[id]` against the **fresh** frame. Stale IDs either crash (out-of-range) or silently corrupt unrelated edges — most commonly the four border edges of the freshly initialised paper get their `Assignment` set to `V`/`M` and `FoldAngle` set to π.

Symmetric corruptions exist for the fold path (`FoldInteractionApplier`) and the unfold path (`EdgeSelectionState`).

Fix:

- Add `FoldAnimator.Cancel()`: null out `_change`, `_onComplete`, reset internals. **Does not** invoke the callback (invoking it would corrupt the freshly-reset Frame).
- Call `Statics.FoldAnimator.Cancel()` from `Game.ResetPaper` before `Statics.ChangeMemory.Clear()`.

The visible behaviour: pressing Reset mid-animation drops the in-flight animation immediately and re-initialises the paper. The half-finished fold disappears; the user sees flat paper.

## Acceptance criteria

- [x] `FoldAnimator.Cancel()` exists, resets `_change`, `_elapsed`, `_duration`, `_startProgress`, `_endProgress`, `_onComplete` to their construction-time defaults, and does NOT invoke the callback.
- [x] `Game.ResetPaper` calls `Statics.FoldAnimator.Cancel()` before clearing change memory.
- [x] Unit test: starting a fold animation, calling `Cancel`, then ticking the animator past its duration does NOT invoke the callback.
- [x] Unit test: `IsAnimating` is `false` immediately after `Cancel`.
- [x] Manual editor check: start a fold, immediately hit Reset before it finishes — paper resets cleanly, no console errors, no border-edge corruption (borders stay `B`).
- [x] All existing tests still pass.

## Blocked by

None.
