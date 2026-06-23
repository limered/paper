# 11 — Exhaustive `switch` for refold Assignment restore

Status: ready-for-afk

## Parent

Grilling session 2026-06-22 on refold state restoration. Fragility found in `RefoldSelectionState.ConfirmRefoldInteraction`.

## What to build

`valleyfold/src/Interaction/SelectionStates/RefoldSelectionState.cs:30` derives the assignment to restore on refold via a 2-way ternary:

```csharp
var restored = change.ChangeType == ChangeType.MountainFold
    ? Assignment.M
    : Assignment.V;
```

This silently defaults *every non-`MountainFold` value* to `Assignment.V`. Today `ChangeType` has three values (`ValleyFold`, `MountainFold`, `Unfold`); the `Unfold` sentinel can't actually reach this line because it has empty `AddedEdges` and `ChangeMemory.ChangeContainingEdge` won't return it. So the bug is latent, not live.

The risk is purely forward: the moment a fourth `ChangeType` is added (squash, reverse, petal, preset combo…), it will silently restore as valley. The C# compiler could give exhaustiveness on a `switch` expression with a `_ => throw` arm — let it.

Replace with:

```csharp
var restored = change.ChangeType switch
{
    ChangeType.MountainFold => Assignment.M,
    ChangeType.ValleyFold   => Assignment.V,
    _ => throw new InvalidOperationException(
        $"Cannot refold change of type {change.ChangeType}"),
};
```

## Acceptance criteria

- [x] `RefoldSelectionState.ConfirmRefoldInteraction` uses an exhaustive `switch` expression with explicit `MountainFold` and `ValleyFold` arms and a throwing default.
- [x] All existing tests still pass.
- [x] Manual editor check: a valley fold can still be refolded; a mountain fold can still be refolded; both restore the correct dash style.

## Blocked by

None.
