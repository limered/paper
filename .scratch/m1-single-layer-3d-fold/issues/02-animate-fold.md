# 02 — Animate fold over time

Status: ready-for-afk

## Parent

`docs/GDD.md` → Roadmap → M-1, Engine roadmap point 1 (animation half).

## What to build

When the player triggers a fold, the paper must animate smoothly from its current position to the fold's target angle over time, instead of snapping in a single frame. New folds are blocked while one is animating.

A new `FoldAnimator` owns the in-flight animation (the active `ChangeRecord` and a `progress` value 0 → 1). It is ticked from `PaperRenderer._Process` each frame. While an animation is active, the renderer applies the in-flight fold at `progress * TargetAngle`; previously completed folds in `ChangeMemory` continue to render at their full `TargetAngle`. When `progress` reaches 1, the animator clears its in-flight slot and emits the completion event.

`FoldInteractionApplier.ApplyVertexValleyFold` no longer emits `PaperFoldedEvent` directly. It hands the new `ChangeRecord` to `FoldAnimator.Start(...)`; the animator emits `PaperFoldedEvent` on completion. The existing `Game._isAnimating` flag is repurposed: set when an animation starts, cleared when it finishes, and consulted by the interaction code to early-exit any new fold attempts during animation. The manual `AnimationModeChange` UI toggle (which today flips `_isAnimating` for no functional reason) is removed.

End-to-end behaviour:

- Trigger a fold via the existing interaction. The paper visibly lifts from 0° to its target angle over ~400ms with an ease-out curve.
- Attempting a second fold while the first is animating is silently ignored.
- After the animation completes, a subsequent fold works normally.

Vertical because it touches: new `FoldAnimator` class, `Statics` (new singleton slot), `ChangeRecord` (no new fields needed — `TargetAngle` from slice 01 already covers it), `PaperRenderer._Process` (drives animator + renders intermediate angle), `FoldInteractionApplier` (delegates to animator), `Game` (repurposed `_isAnimating`), `GameInterface` / UI scene (removes the `AnimationModeChange` button binding), tests, manual verification.

## Acceptance criteria

- [ ] New class `FoldAnimator` in `valleyfold/src/Folding/`, registered as `Statics.FoldAnimator`.
- [ ] `FoldAnimator` exposes at least: `bool IsAnimating`, `void Start(ChangeRecord change, float durationSeconds = 0.4f)`, `void Tick(double delta)`.
- [ ] `FoldAnimator.Tick` advances `progress` and emits `PaperFoldedEvent` when it reaches 1, then clears the in-flight slot.
- [ ] The interpolation curve is ease-out (e.g. `1 - (1 - t)^2`); update the constant in one place.
- [ ] `PaperRenderer._Process` calls `FoldAnimator.Tick(delta)` each frame and, while an animation is active, applies the in-flight fold's rotation at `progress * TargetAngle` instead of its full `TargetAngle`.
- [ ] `FoldInteractionApplier.ApplyVertexValleyFold` calls `FoldAnimator.Start(changeRecord)` instead of emitting `PaperFoldedEvent` itself, and early-exits if `FoldAnimator.IsAnimating` is true.
- [ ] `Game._isAnimating` is driven by `FoldAnimator`'s start/complete events (or by querying `IsAnimating`), not by the UI toggle.
- [ ] The `AnimationModeChange` event subscription and its UI button are removed from `Game` and `GameInterface` respectively. If the event type itself has no other listeners, delete it.
- [ ] Unit tests for `FoldAnimator`: `IsAnimating` is false initially, true after `Start`, false after enough `Tick` calls to cover the duration. Verify `PaperFoldedEvent` is emitted exactly once on completion (subscribe via `EventBus` in the test).
- [ ] All existing tests still pass: `dotnet test valleyfold/valleyfold.sln`.
- [ ] Manual check in the Godot editor: a fold triggered via the existing UI animates smoothly over ~400ms, and a second fold triggered mid-animation does nothing until the first completes.

## Blocked by

- `.scratch/m1-single-layer-3d-fold/issues/01-parameterise-fold-angle.md` — needs `ChangeRecord.TargetAngle` and the angle-parameterised rotation helper in place first.
